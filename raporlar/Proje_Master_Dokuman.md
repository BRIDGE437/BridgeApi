# Bridge API - Kapsamlı Proje ve Teknik Dokümantasyon

Bu doküman, sistem mimarisini ve hem C# hem de Python tarafındaki tüm API uç noktalarının (endpoint) detaylı parametrelerini içerir.

---

## 1. SİSTEM MİMARİSİ VE AKIŞ
Sistem hibrit bir yapıdadır:
- **C# Backend (.NET 8)**: Ana veri otoritesidir. PostgreSQL (Relational) verileri yönetir.
- **Python AI Microservice (FastAPI)**: pgvector (Vector DB) işlemlerini, Embedding üretimini ve Gemini (LLM) analizlerini yapar.
- **Akış**: Bir kullanıcı "AI Match" istediğinde; C# tarafı önce kural bazlı bir ön eleme yapar, ardından aday listesini Python servisine göndererek derinlematik analiz sonuçlarını alır ve kullanıcıya birleştirilmiş (hybrid) bir rapor sunar.

---

## 2. C# BACKEND API REFERANSI (Port: 5000)

### 2.1. Yatırımcı Yönetimi (`/api/v1/investors`)

#### [POST] / (Yeni Yatırımcı)
- **Request Body:**
  ```json
  {
    "name": "Yatırımcı Adı",
    "type": "VC | Angel | Accelerator | Fund",
    "preferredSectors": "AI, Fintech, Health",
    "preferredBusinessModel": "B2B, SaaS, Marketplace",
    "preferredRegions": "Europe, Turkey",
    "investmentStage": "Seed | Series A",
    "ticketSizeMin": 50000,
    "ticketSizeMax": 500000,
    "description": "Strateji açıklaması (opsiyonel)",
    "website": "URL (opsiyonel)"
  }
  ```
- **Response:** Oluşturulan `Investor` objesinin tamamı (ID dahil).

#### [PUT] /{id} (Güncelleme)
- **Request Body:** Yukarıdaki alanların herhangi biri (Sadece değişenler gönderilebilir).
- **Response:** Güncel `Investor` objesi.

### 2.2. Eşleştirme Motoru (`/api/v1/match`)

#### [POST] /ai-powered (AI Destekli Hibrit Eşleşme)
- **Request Body:**
  ```json
  {
    "investorId": "inv_abc123",
    "topN": 10
  }
  ```
- **Response Structure:**
  ```json
  {
    "investorId": "inv_abc123",
    "investorName": "Sinan VC",
    "matchingMode": "ai-powered",
    "totalCandidates": 150,
    "results": [
      {
        "rank": 1,
        "startupId": 12,
        "startupName": "Global AI",
        "score": 95.5,
        "breakdown": {
          "sectorScore": 30.0,
          "geoScore": 15.0,
          "modelScore": 10.0,
          "stageScore": 5.0,
          "semanticScore": 35.5,
          "llmBonus": 10.0
        },
        "aiReason": "Bu girişim, yatırımcının derin teknoloji vizyonuyla örtüşüyor...",
        "tags": ["AI", "NLP"],
        "hq": "San Francisco",
        "businessModel": "SaaS",
        "description": "Girişim özeti...",
        "revenueState": "Post-Revenue",
        "website": "https://..."
      }
    ],
    "metadata": {
      "processingTimeMs": 520,
      "embeddingModel": "all-MiniLM-L6-v2",
      "llmUsed": true
    }
  }
  ```

---

## 3. PYTHON AI SERVISI REFERANSI (Port: 8000)

### 3.1. Semantik Eşleşme (`/api/v1/semantic-match`)

#### [POST] / (Vektör Bazlı Karşılaştırma)
- **Request Body:**
  ```json
  {
    "investor_text": "Yatırımcının tüm kriterlerini içeren birleştirilmiş metin",
    "startups": [
      { "id": 1, "text": "Startup adı, sektörü ve açıklaması" }
    ],
    "use_llm": true,
    "mode": "investor_startup" 
  }
  ```
- **Response:**
  ```json
  {
    "results": [
      {
        "startup_id": 1,
        "similarity_score": 0.88,   // 0-1 arası semantik yakınlık
        "llm_score": 9.2,           // 0-10 arası LLM stratejik puanı
        "reason": "Yapay zeka analizi açıklaması"
      }
    ],
    "model": "all-MiniLM-L6-v2",
    "processing_time_ms": 150
  }
  ```

### 3.2. B2B Sinerji Analizi (`/api/v1/semantic-match/startup-startup`)

#### [POST] / (Girişimden Girişime Eşleşme)
- **Request Body:**
  ```json
  {
    "source_startup_id": 1,
    "target_startup_ids": [2, 3, 4, 5],
    "use_llm": true
  }
  ```
- **Response:** Belirtilen `source_startup_id` ile diğerleri arasındaki sinerji puanlarını içeren `results` listesi.

---

## 4. VERİTABANI VE İNDEKSLEME
Sistemde iki ana tablo/alan kritik öneme sahiptir:
1. **PostgreSQL / MatchResults**: Yapılan her eşleşme (AI veya Kural) bu tabloda detaylı skor kırılımıyla saklanır.
2. **pgvector / Embeddings**: Python servisi, her girişimin metnini 384 boyutlu bir vektöre çevirir ve pgvector üzerinde hızlı komşuluk araması yapar.
