# Startup–Investor Matching Sistemi — Mimari Dokümantasyon

## İçindekiler

1. [Genel Bakış](#genel-bakış)
2. [Sistem Bileşenleri](#sistem-bileşenleri)
3. [Veri Akışı](#veri-akışı)
4. [Matching Pipeline](#matching-pipeline)
   - [Faz 1 — Rule-Based Ön Eleme](#faz-1--rule-based-ön-eleme)
   - [Faz 2 — Semantic Similarity](#faz-2--semantic-similarity)
   - [Faz 3 — Skor Birleştirme](#faz-3--skor-birleştirme)
   - [Faz 4 — LLM Reranking (Opsiyonel)](#faz-4--llm-reranking-opsiyonel)
5. [Embedding Cache Mimarisi](#embedding-cache-mimarisi)
6. [PostgreSQL Şeması](#postgresql-şeması)
7. [ChromaDB Şeması](#chromadb-şeması)
8. [API Endpoints](#api-endpoints)
9. [Skor Hesaplama Detayları](#skor-hesaplama-detayları)

---

## Genel Bakış

Bu sistem, yatırımcıları startup'larla eşleştirmek için **kural tabanlı** ve **yapay zeka destekli** iki farklı yöntem sunar. İki servis birlikte çalışır:

```
┌──────────────────────────────┐        ┌──────────────────────────────┐
│   .NET Backend               │        │   Python AI Service          │
│   localhost:5000             │◄──────►│   localhost:8000             │
│                              │  HTTP  │                              │
│  - Startup / Investor CRUD   │        │  - Embedding (all-MiniLM)    │
│  - Rule-based matching       │        │  - Cosine similarity         │
│  - PostgreSQL (kalıcı veri)  │        │  - ChromaDB (kalıcı cache)   │
│  - Skor birleştirme          │        │  - LLM reranking (opsiyonel) │
└──────────────────────────────┘        └──────────────────────────────┘
            │                                        │
            ▼                                        ▼
      PostgreSQL DB                            ChromaDB
   (startup, investor,                   (startup embedding
     match_results)                           vektörleri)
```

---

## Sistem Bileşenleri

### .NET Backend (`backend/MatchingApi/`)

| Dosya | Görev |
|---|---|
| `Controllers/StartupController.cs` | Startup CRUD, CSV import |
| `Controllers/InvestorController.cs` | Investor CRUD |
| `Controllers/MatchController.cs` | Matching endpoint'leri |
| `Services/RuleBasedMatchingService.cs` | Kural tabanlı eşleştirme motoru |
| `Services/AiMatchingService.cs` | Hybrid AI matching orkestrasyonu |
| `Services/CsvImportService.cs` | CSV veri yükleme |
| `Helpers/SectorSimilarity.cs` | Sektör benzerlik ağırlık haritası |
| `Helpers/RegionMapper.cs` | Coğrafi bölge/şehir yakınlık haritası |
| `Models/` | Startup, Investor, MatchResult entity'leri |
| `Data/AppDbContext.cs` | Entity Framework DbContext |

### Python AI Service (`ai-service/`)

| Dosya | Görev |
|---|---|
| `main.py` | FastAPI uygulaması, endpoint'ler |
| `matching/embeddings.py` | Sentence Transformer engine + L1 bellek cache |
| `matching/vector_store.py` | ChromaDB L2 disk cache wrapper |
| `matching/reranker.py` | OpenAI / Anthropic LLM reranker |

---

## Veri Akışı

### CSV Import → Matching Tam Akışı

```
[CSV Dosyası]
     │
     ▼
POST /api/v1/startups/import          ← .NET Backend
     │  CsvImportService
     │  - Sütun adları normalize edilir (boşluk/slash → underscore)
     │  - ID varsa güncelle, yoksa ekle
     ▼
[PostgreSQL: Startups tablosu]
     │
     │
     ▼
POST /api/v1/match/ai-powered         ← .NET Backend
     │
     ├─► Faz 1: RuleBasedMatchingService
     │     └─ Top 50 aday seçilir
     │
     ├─► Faz 2: HTTP → Python AI Service
     │     POST /api/v1/semantic-match
     │     │
     │     ├─ investor embedding → L1 cache (bellek)
     │     │
     │     └─ startup embeddings:
     │           ChromaDB'de var mı? (L2 cache)
     │           ├─ EVET: anında döner
     │           └─ HAYIR: encode et → ChromaDB'ye yaz → döner
     │
     ├─► Faz 3: Skor birleştirme
     │     Rule(×0.6) + Semantic(×0.3) + LLM(×0.1)
     │
     └─► [PostgreSQL: MatchResults tablosu]
```

---

## Matching Pipeline

### Faz 1 — Rule-Based Ön Eleme

**Dosya:** `RuleBasedMatchingService.cs`

Tüm "Alive" startup'lar arasından **hard filter** + **skor** ile top 50 aday seçilir.

#### Hard Filter (Zorunlu Geçme Kriteri)

Yatırımcının tercih ettiği bölge/şehir ile startup'ın HQ'su arasında hiç örtüşme yoksa startup **tamamen elenir**.

```
Ülke eşleşmesi    → geçer
Bölge eşleşmesi   → geçer (Turkey, Europe, USA, MENA, Asia...)
Şehir eşleşmesi   → geçer
Yakın şehir        → geçer (proximity > 0)
Hiç eşleşme yok  → ELENİR
```

#### Ağırlık Sistemi

Yatırımcının iş modeli tercihi varsa:

| Bileşen | Ağırlık |
|---|---|
| Sektör Skoru | 40 puan |
| Coğrafi Skor | 35 puan |
| İş Modeli Skoru | 25 puan |
| **Toplam** | **100 puan** |

Yatırımcının iş modeli tercihi yoksa:

| Bileşen | Ağırlık |
|---|---|
| Sektör Skoru | 55 puan |
| Coğrafi Skor | 45 puan |
| **Toplam** | **100 puan** |

#### Sektör Skoru

`SectorSimilarity.cs` içindeki benzerlik haritasını kullanır. Tam eşleşme dışında ilişkili sektörler de puan alır:

```
Yatırımcı: ["AI", "SaaS"]
Startup:   ["Fintech", "SaaS"]

  AI    → Fintech: tanımsız → 0
  AI    → SaaS:   0.3 (ilişkili)
  SaaS  → Fintech: 0.5 (benzer)
  SaaS  → SaaS:   1.0 (tam eşleşme)

Max eşleşme per startup tag alınır, normalize edilir.
```

Benzerlik ağırlıkları:
- Tam eşleşme: `1.0`
- Benzer (örn. Fintech ↔ SaaS): `0.5`
- İlişkili (örn. AI ↔ SaaS): `0.3`
- Uzak ilişki: `0.25`

#### Coğrafi Skor

`RegionMapper.cs` ile hesaplanır:

```
Tam şehir eşleşmesi         → maxGeo puanın tamamı
Yakın şehir (proximity 0.7) → maxGeo × 0.7
Aynı ülke                   → maxGeo × 0.4
Aynı bölge                  → maxGeo × 0.25
```

Şehir yakınlık örnekleri:
- İstanbul ↔ Bursa / Kocaeli: `0.7`
- İstanbul ↔ Ankara: `0.3`
- London ↔ Paris: `0.4`
- London ↔ Berlin: `0.3`

---

### Faz 2 — Semantic Similarity

**Dosya:** `AiMatchingService.cs` → `POST /api/v1/semantic-match`

Rule-based'den gelen top 50 aday Python AI servisine gönderilir.

#### Metin Oluşturma

**Yatırımcı metni:**
```
"{Ad}. Sectors: {Sektörler}. Regions: {Bölgeler}. Stage: {Aşama}. Model: {İş Modeli}. {Açıklama}"
```

**Startup metni:**
```
"{Ad}. {Tag'ler}. {Açıklama}. {İş Modeli}. {HQ}"
```

#### Embedding Modeli

- Model: `all-MiniLM-L6-v2` (Sentence Transformers)
- Boyut: **384 boyutlu vektör**
- Cihaz: Apple Silicon için MPS, diğerleri için CPU
- Normalize: L2 normalize → dot product = cosine similarity

#### Cosine Similarity

Yatırımcı vektörü ile her startup vektörü arasında nokta çarpımı hesaplanır (normalize vektörler için dot product = cosine):

```
similarity = dot(investor_vec, startup_vec)  ∈ [0.0, 1.0]
```

---

### Faz 3 — Skor Birleştirme

**Dosya:** `AiMatchingService.cs`

```
Final Skor = min(100,
    RuleScore  × 0.6   (max 60 puan)
  + SemanticScore × 30  (max 30 puan, similarity [0,1] × 30)
  + LlmBonus            (max 10 puan, opsiyonel)
)
```

Örnek:
```
RuleScore      = 56  → 56 × 0.6  = 33.6
SemanticScore  = 0.56 → 0.56 × 30 = 16.8
LlmBonus       = 0   (LLM kapalı)
─────────────────────────────────────
Final          = 50.4  →  min(100, 50.4) = 50.4
```

---

### Faz 4 — LLM Reranking (Opsiyonel)

**Dosya:** `matching/reranker.py`

`AiService:UseLlm = true` olduğunda aktif olur. Yalnızca **top 20** adaya uygulanır (maliyet optimizasyonu).

- Desteklenen sağlayıcılar: OpenAI (`gpt-4o-mini`), Anthropic (`claude-sonnet`)
- Env değişkenleri: `LLM_PROVIDER`, `LLM_API_KEY`, `LLM_MODEL`
- Her startup için 0–10 arası skor + açıklama döner
- Ağırlık: `LlmBonus` olarak final skora eklenir (max 10 puan)

---

## Embedding Cache Mimarisi

İki katmanlı cache sistemi kullanılır:

```
İstek geldi
     │
     ▼
[L1: EmbeddingEngine._cache]
 Scope: Process ömrü (bellek)
 Anahtar: MD5(text)
 İçerik: Her metin (investor + startup)
     │
     │ Cache miss?
     ▼
[L2: ChromaDB — startup_embeddings collection]
 Scope: Kalıcı (disk)
 Anahtar: startup_id (integer → string dönüşüm)
 İçerik: Sadece startup embedding vektörleri
 Stale detection: text_hash (MD5) karşılaştırması
     │
     │ Cache miss?
     ▼
[Sentence Transformer encode]
 → L1'e yaz
 → L2'ye upsert
```

### Stale Detection

Startup'ın metni değiştiğinde eski embedding otomatik geçersiz sayılır:

```python
stored_hash = meta["text_hash"]      # ChromaDB'deki MD5
current_hash = MD5(gelen_text)       # Şu anki metnin MD5'i

if stored_hash != current_hash:
    # Yeniden encode et, upsert et
```

---

## PostgreSQL Şeması

### Tablo: `Startups`

```sql
CREATE TABLE "Startups" (
    "Id"                 INTEGER        PRIMARY KEY,
    "Name"               VARCHAR(200)   NOT NULL,
    "Website"            VARCHAR(500),
    "Twitter"            VARCHAR(500),
    "Instagram"          VARCHAR(500),
    "Status"             VARCHAR(50)    NOT NULL DEFAULT 'Alive',
    "Description"        VARCHAR(2000),
    "YearFounded"        INTEGER,
    "HQ"                 VARCHAR(200),  -- Format: "Şehir / Ülke"
    "Founders"           VARCHAR(500),
    "Tags"               VARCHAR(1000), -- Virgülle ayrılmış: "Fintech, AI, SaaS"
    "BusinessModel"      VARCHAR(100),  -- Virgülle ayrılmış: "B2B, B2C"
    "RevenueModel"       VARCHAR(500),
    "RevenueState"       VARCHAR(50),   -- "Pre-Revenue" | "Post-Revenue" | "Post-Profit"
    "TotalFunding"       VARCHAR(50),
    "Stage"              VARCHAR(50),   -- "Pre-Seed" | "Seed" | "Series A" | ...
    "WebsiteEmail"       VARCHAR(500),
    "WebsiteDescription" VARCHAR(4000)
);

CREATE INDEX IX_Startups_Status ON "Startups" ("Status");
CREATE INDEX IX_Startups_Tags   ON "Startups" ("Tags");
CREATE INDEX IX_Startups_HQ     ON "Startups" ("HQ");
```

### Tablo: `Investors`

```sql
CREATE TABLE "Investors" (
    "InvestorId"            VARCHAR(50)   PRIMARY KEY,  -- Format: "inv_xxxxxxxx"
    "Name"                  VARCHAR(200)  NOT NULL,
    "Type"                  VARCHAR(50)   NOT NULL,     -- angel | vc | corporate | accelerator | family_office
    "PreferredSectors"      VARCHAR(1000) NOT NULL,     -- Virgülle ayrılmış
    "PreferredBusinessModel"VARCHAR(100)  NOT NULL,     -- Virgülle ayrılmış
    "PreferredRegions"      VARCHAR(500)  NOT NULL,     -- Virgülle ayrılmış
    "PreferredCities"       VARCHAR(500),               -- Virgülle ayrılmış (opsiyonel)
    "InvestmentStage"       VARCHAR(200)  NOT NULL,     -- Virgülle ayrılmış
    "TicketSizeMin"         BIGINT        NOT NULL DEFAULT 0,
    "TicketSizeMax"         BIGINT        NOT NULL DEFAULT 0,
    "PreferredRevenueState" VARCHAR(200),               -- Virgülle ayrılmış
    "Portfolio"             VARCHAR(2000),
    "Description"           VARCHAR(2000),
    "Website"               VARCHAR(500),
    "ContactEmail"          VARCHAR(200),
    "LinkedIn"              VARCHAR(500),
    "Active"                BOOLEAN       NOT NULL DEFAULT TRUE,
    "CreatedAt"             TIMESTAMPTZ   NOT NULL DEFAULT NOW()
);

CREATE INDEX IX_Investors_Active ON "Investors" ("Active");
CREATE INDEX IX_Investors_Type   ON "Investors" ("Type");
```

### Tablo: `MatchResults`

```sql
CREATE TABLE "MatchResults" (
    "Id"           BIGINT        PRIMARY KEY GENERATED BY DEFAULT AS IDENTITY,
    "InvestorId"   VARCHAR(50)   NOT NULL REFERENCES "Investors"("InvestorId") ON DELETE CASCADE,
    "StartupId"    INTEGER       NOT NULL REFERENCES "Startups"("Id")          ON DELETE CASCADE,
    "MatchingMode" VARCHAR(20)   NOT NULL,   -- "rule-based" | "ai-powered"

    -- Skor bileşenleri
    "TotalScore"   DOUBLE PRECISION NOT NULL DEFAULT 0,
    "SectorScore"  DOUBLE PRECISION NOT NULL DEFAULT 0,
    "GeoScore"     DOUBLE PRECISION NOT NULL DEFAULT 0,
    "ModelScore"   DOUBLE PRECISION NOT NULL DEFAULT 0,
    "StageScore"   DOUBLE PRECISION NOT NULL DEFAULT 0,
    "FundingBonus" DOUBLE PRECISION NOT NULL DEFAULT 0,
    "SemanticScore"DOUBLE PRECISION NOT NULL DEFAULT 0,
    "LlmBonus"     DOUBLE PRECISION NOT NULL DEFAULT 0,

    "AiReason"     VARCHAR(2000),   -- LLM açıklaması (opsiyonel)
    "CreatedAt"    TIMESTAMPTZ   NOT NULL DEFAULT NOW()
);

CREATE INDEX IX_MatchResults_InvestorId_StartupId ON "MatchResults" ("InvestorId", "StartupId");
CREATE INDEX IX_MatchResults_CreatedAt            ON "MatchResults" ("CreatedAt");
CREATE INDEX IX_MatchResults_StartupId            ON "MatchResults" ("StartupId");
```

### İlişki Diyagramı

```
Investors ─────────────────────────── MatchResults ─── Startups
 InvestorId (PK)         InvestorId (FK) ─────────┘     Id (PK)
 Name                    StartupId (FK) ──────────────── Name
 Type                    MatchingMode                    Tags
 PreferredSectors        TotalScore                      HQ
 ...                     SectorScore                     ...
                         GeoScore
                         ModelScore
                         SemanticScore
                         LlmBonus
                         AiReason
```

---

## ChromaDB Şeması

ChromaDB ilişkisel bir veritabanı değildir; **vektör tabanlı embedding store**dur. Startup embedding'lerini kalıcı olarak diske yazar.

### Konum

```
ai-service/chroma_data/          ← CHROMA_PERSIST_DIR env (default)
├── chroma.sqlite3               ← Metadata, koleksiyon bilgisi
└── [uuid]/                      ← HNSW indeks dosyaları
    ├── data_level0.bin          ← Vektör indeksi
    ├── header.bin
    ├── length.bin
    └── link_lists.bin
```

### Collection: `startup_embeddings`

```
Collection Adı  : startup_embeddings
Distance Space  : cosine  (hnsw:space = "cosine")
Embedding Modeli: all-MiniLM-L6-v2  (collection metadata'da saklanır)
Boyut           : 384
```

### Her Kayıt (Document) Yapısı

| Alan | Tip | Açıklama |
|---|---|---|
| `id` | `string` | Startup'ın PostgreSQL PK'sı (`str(startup_id)`) |
| `embedding` | `list[float]` | 384 boyutlu normalize vektör |
| `metadata.text_hash` | `string` | Startup metninin MD5 hash'i (stale detection) |
| `metadata.startup_id` | `int` | Orijinal integer ID (referans için) |

### Örnek Kayıt

```json
{
  "id": "43497",
  "embedding": [0.023, -0.187, 0.441, ... ],
  "metadata": {
    "text_hash": "a3f2c1d8e9b7...",
    "startup_id": 43497
  }
}
```

### HNSW İndeks Parametreleri

ChromaDB varsayılan HNSW (Hierarchical Navigable Small World) parametreleri:

| Parametre | Değer | Açıklama |
|---|---|---|
| `hnsw:space` | `cosine` | Uzaklık metriği |
| `hnsw:construction_ef` | `100` | İndeks oluşturma kalitesi |
| `hnsw:search_ef` | `10` | Arama kalitesi |
| `hnsw:M` | `16` | Graf bağlantı sayısı |

> Bu projede ChromaDB **ANN (Approximate Nearest Neighbor) search** için değil, **ID bazlı lookup cache** olarak kullanılmaktadır. Startup ID'leri bilindiğinden `collection.get(ids=[...])` ile direkt erişilir — vektör araması yapılmaz.

### ID Dönüşümü

PostgreSQL integer ID'leri ChromaDB string ID'ye dönüştürülür:

```
PostgreSQL: 43497  (int)
ChromaDB:  "43497" (string)

Okuma:  result["ids"] → [str] → int() ile geri dönüştür
Yazma:  startup_id   → str(startup_id)
```

---

## API Endpoints

### .NET Backend (`localhost:5000`)

#### Startup

| Method | Endpoint | Açıklama |
|---|---|---|
| `GET` | `/api/v1/startups` | Listele (sayfalama destekli) |
| `GET` | `/api/v1/startups/{id}` | Tekil startup |
| `POST` | `/api/v1/startups/import` | CSV import (multipart/form-data) |
| `GET` | `/api/v1/startups/tags` | Benzersiz tag listesi |
| `GET` | `/api/v1/startups/stats` | İstatistikler |

#### Investor

| Method | Endpoint | Açıklama |
|---|---|---|
| `GET` | `/api/v1/investors` | Listele |
| `GET` | `/api/v1/investors/{id}` | Tekil investor |
| `POST` | `/api/v1/investors` | Yeni investor oluştur |
| `PUT` | `/api/v1/investors/{id}` | Güncelle |
| `DELETE` | `/api/v1/investors/{id}` | Sil |

#### Matching

| Method | Endpoint | Açıklama |
|---|---|---|
| `POST` | `/api/v1/match/rule-based` | Sadece kural tabanlı eşleştirme |
| `POST` | `/api/v1/match/ai-powered` | Hybrid AI eşleştirme |
| `POST` | `/api/v1/match/compare` | İki modu yan yana karşılaştır |
| `GET` | `/api/v1/match/history` | Geçmiş eşleştirmeler |

### Python AI Service (`localhost:8000`)

| Method | Endpoint | Açıklama |
|---|---|---|
| `POST` | `/api/v1/semantic-match` | Semantic benzerlik hesapla |
| `POST` | `/api/v1/index-startups` | Toplu embedding ön yükleme |
| `POST` | `/api/v1/embed` | Ham embedding üretimi |
| `GET` | `/api/v1/vector-store/stats` | ChromaDB durumu |
| `GET` | `/health` | Servis sağlık kontrolü |

---

## Skor Hesaplama Detayları

### Rule-Based Skor Örneği

```
Yatırımcı: AI, SaaS tercihli — İstanbul — B2B
Startup:   AI, Business intelligence — İstanbul / Turkey — B2B

Sektör Skoru (max 40):
  AI  → AI:                    1.0 × normalize → ~20
  AI  → Business intelligence: 0.5 × normalize → ~10
  Toplam normalize → 20 puan

Coğrafi Skor (max 35):
  İstanbul = İstanbul (tam eşleşme) → 35 puan

Model Skoru (max 25):
  B2B = B2B (tam eşleşme) → 25 puan

Rule Toplam: 20 + 35 + 25 = 80 puan
```

### Hybrid Final Skor Örneği

```
Rule Toplam:   80
Semantic:      0.56  (cosine similarity)

Final =  80 × 0.6   +  0.56 × 30
      =  48          +  16.8
      =  64.8 puan
```

### Skor Aralıkları

| Skor | Yorum |
|---|---|
| 70–100 | Güçlü eşleşme |
| 50–70  | Orta eşleşme |
| 30–50  | Zayıf eşleşme |
| 0–30   | Uyumsuz |
