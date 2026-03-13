# 🚀 Startup ↔ Yatırımcı Eşleştirme Sistemi — Kurulum Rehberi

## 📁 Proje Yapısı

```
startup-investor-matching/
├── backend/                          # .NET 8 Web API (Ana Backend)
│   ├── MatchingApi.sln
│   └── MatchingApi/
│       ├── Program.cs                # Uygulama giriş noktası
│       ├── MatchingApi.csproj        # NuGet bağımlılıkları
│       ├── appsettings.json          # Konfigürasyon
│       ├── Models/
│       │   ├── Startup.cs            # Startup entity
│       │   ├── Investor.cs           # Yatırımcı entity
│       │   └── MatchResult.cs        # Eşleştirme sonucu entity
│       ├── Data/
│       │   └── AppDbContext.cs        # EF Core DbContext
│       ├── DTOs/
│       │   └── AllDtos.cs            # Request/Response modelleri
│       ├── Helpers/
│       │   └── RegionMapper.cs       # Bölge haritalama
│       ├── Services/
│       │   ├── RuleBasedMatchingService.cs   # Kural tabanlı algoritma
│       │   ├── AiMatchingService.cs          # AI destekli algoritma
│       │   └── CsvImportService.cs           # CSV import
│       └── Controllers/
│           ├── StartupController.cs   # /api/v1/startups
│           ├── InvestorController.cs  # /api/v1/investors
│           └── MatchController.cs     # /api/v1/match
│
├── ai-service/                        # Python FastAPI (AI Mikroservis)
│   ├── main.py                        # FastAPI app
│   ├── requirements.txt               # Python bağımlılıkları
│   └── matching/
│       ├── __init__.py
│       ├── embeddings.py              # Embedding motoru
│       └── reranker.py                # LLM reranker (opsiyonel)
│
└── SETUP.md                           # Bu dosya
```

---

## ⚙️ Ön Gereksinimler (macOS)

### Homebrew Kurulumu (yoksa)

```bash
/bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"
```

### Gerekli Araçları Kur

Terminali aç ve sırayla çalıştır:

```bash
# 1. .NET 8 SDK
brew install --cask dotnet-sdk

# 2. Python 3.11+
brew install python@3.12

# 3. PostgreSQL
brew install postgresql@16
brew services start postgresql@16

# 4. Node.js (frontend için, ileride lazım olacak)
brew install node

# 5. Git (macOS'ta genelde zaten var)
git --version || xcode-select --install
```

### Versiyon Kontrolü

```bash
dotnet --version     # 8.0.x olmalı
python3 --version    # 3.11+ olmalı (macOS'ta python3 kullan!)
psql --version       # 16+ olmalı
node --version       # 20+ olmalı
```

> ⚠️ **Önemli:** macOS'ta `python` yerine `python3`, `pip` yerine `pip3` kullan. Rehberin geri kalanında buna dikkat et.

---

## 📦 Adım 1: Projeyi Bilgisayarına Kopyala

Zip dosyasını aç veya Git repo'sundan klonla:

```bash
# ZIP'ten açtıysan:
cd startup-investor-matching

# Veya repo'dan:
git clone <repo-url>
cd startup-investor-matching
```

---

## 🐘 Adım 2: PostgreSQL Veritabanını Kur

### 2a. PostgreSQL servisinin çalıştığından emin ol

```bash
# Homebrew ile başlattıysan zaten çalışıyor olmalı:
brew services list | grep postgresql

# Çalışmıyorsa:
brew services start postgresql@16

# Bağlantı testi:
pg_isready
```

### 2b. Veritabanını oluştur

```bash
# macOS'ta Homebrew PostgreSQL'de varsayılan kullanıcı senin macOS kullanıcı adın
# Şifre gerekmez genelde. Direkt bağlan:
psql postgres

# Veritabanını oluştur
CREATE DATABASE matching_db;

# Çık
\q
```

### 2c. Bağlantı bilgilerini güncelle

`backend/MatchingApi/appsettings.json` dosyasını aç ve connection string'i güncelle.

macOS + Homebrew PostgreSQL için genelde şifre gerekmez:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=matching_db;Username=MACOS_KULLANICI_ADIN"
  }
}
```

> 💡 macOS kullanıcı adını bulmak için terminalde `whoami` yaz.
```

---

## 🔷 Adım 3: .NET Backend'i Kur ve Çalıştır

### 3a. Bağımlılıkları yükle

```bash
cd backend/MatchingApi
dotnet restore
```

### 3b. EF Core araçlarını kur (migration için)

```bash
dotnet tool install --global dotnet-ef
```

### 3c. İlk migration'ı oluştur ve uygula

```bash
# Migration oluştur
dotnet ef migrations add InitialCreate

# Veritabanına uygula
dotnet ef database update
```

### 3d. Backend'i çalıştır

```bash
dotnet run
```

Başarılıysa şu çıktıyı görmelisin:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
```

### 3e. Swagger UI'ı kontrol et

Tarayıcında aç: **http://localhost:5000/swagger**

Tüm API endpoint'lerini görebilmelisin.

---

## 🐍 Adım 4: Python AI Servisini Kur ve Çalıştır

### 4a. Virtual environment oluştur

```bash
cd ai-service

python3 -m venv venv
source venv/bin/activate
```

> Terminalinde `(venv)` yazısını görmelisin. Her yeni terminal açtığında `source venv/bin/activate` çalıştırmayı unutma.

### 4b. Bağımlılıkları yükle

```bash
pip install -r requirements.txt
```

> ⚠️ **Not:** `sentence-transformers` ilk kurulumda PyTorch da indirir (~2GB). İlk çalıştırmada model de indirilecek (~90MB). Sabırla bekle.

### 4c. (Opsiyonel) LLM API Key'ini ayarla

Eğer LLM reranking kullanmak istersen:

```bash
export LLM_PROVIDER=openai
export LLM_API_KEY=sk-...
```

> 💡 Kalıcı yapmak istersen `~/.zshrc` dosyasına ekle (macOS Catalina+ varsayılan shell zsh).

### 4d. AI servisini çalıştır

```bash
uvicorn main:app --host 0.0.0.0 --port 8000 --reload
```

Başarılıysa:
```
INFO:     Uvicorn running on http://0.0.0.0:8000
INFO:     Loading embedding model...
INFO:     AI Service ready!
```

### 4e. Health check

```bash
curl http://localhost:8000/health
# {"status":"ok","model":"all-MiniLM-L6-v2"}
```

---

## 📊 Adım 5: Veri Yükleme

### 5a. Startup verilerini CSV'den yükle

Backend çalışırken, Swagger UI veya curl ile CSV'yi import et:

```bash
curl -X POST http://localhost:5000/api/v1/startups/import \
  -F "file=@startups_data_enriched.csv"
```

Yanıt:
```json
{
  "totalRows": 100,
  "imported": 100,
  "skipped": 0,
  "errors": []
}
```

### 5b. Örnek yatırımcı verilerini ekle

```bash
# Yatırımcı 1: Revo Capital
curl -X POST http://localhost:5000/api/v1/investors \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Revo Capital",
    "type": "vc",
    "preferredSectors": "Fintech, SaaS, Artificial intelligence, Developer Tools",
    "preferredBusinessModel": "B2B",
    "preferredRegions": "Turkey, Europe, MENA",
    "preferredCities": "Istanbul, London",
    "investmentStage": "Seed, Series A",
    "ticketSizeMin": 250000,
    "ticketSizeMax": 5000000,
    "preferredRevenueState": "Post-Revenue",
    "description": "Türkiye ve bölgedeki B2B SaaS ve fintech startuplara odaklanan erken aşama VC fonu."
  }'

# Yatırımcı 2: Hasan Aslanoba (Angel)
curl -X POST http://localhost:5000/api/v1/investors \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Hasan Aslanoba",
    "type": "angel",
    "preferredSectors": "Gaming, Social media, E-commerce",
    "preferredBusinessModel": "B2C, B2B",
    "preferredRegions": "Turkey",
    "preferredCities": "Istanbul, Ankara",
    "investmentStage": "Pre-Seed, Seed",
    "ticketSizeMin": 25000,
    "ticketSizeMax": 200000,
    "preferredRevenueState": "Pre-Revenue, Post-Revenue",
    "description": "Türkiyedeki genç girişimcilere odaklanan melek yatırımcı."
  }'
```

---

## 🎯 Adım 6: Eşleştirmeyi Test Et

### Rule-Based Eşleştirme

```bash
curl -X POST http://localhost:5000/api/v1/match/rule-based \
  -H "Content-Type: application/json" \
  -d '{"investorId": "INVESTOR_ID_BURAYA", "topN": 10}'
```

### AI-Powered Eşleştirme

```bash
# AI servisi çalışıyor olmalı (port 8000)
curl -X POST http://localhost:5000/api/v1/match/ai-powered \
  -H "Content-Type: application/json" \
  -d '{"investorId": "INVESTOR_ID_BURAYA", "topN": 10}'
```

### İki Modu Karşılaştır

```bash
curl -X POST http://localhost:5000/api/v1/match/compare \
  -H "Content-Type: application/json" \
  -d '{"investorId": "INVESTOR_ID_BURAYA", "topN": 10}'
```

---

## 🖥️ Adım 7: Frontend (Next.js) — İlerisi İçin

Frontend henüz dahil değil, ancak backend API'leri hazır. Next.js projesini oluşturmak için:

```bash
npx create-next-app@latest frontend --typescript --tailwind --app
cd frontend
npm install axios
npm run dev
```

API çağrıları `http://localhost:5000/api/v1/...` üzerinden yapılacak. CORS ayarları zaten yapılmış durumda.

---

## 🔧 Sık Karşılaşılan Sorunlar

### ❌ "Connection refused" — PostgreSQL bağlantı hatası

```bash
# PostgreSQL çalışıyor mu kontrol et
pg_isready -h localhost -p 5432

# appsettings.json'daki connection string doğru mu?
# Kullanıcı adı ve şifre doğru mu?
```

### ❌ "AI service unavailable" — Python servisi bağlantı hatası

```bash
# AI servisinin çalıştığını kontrol et
curl http://localhost:8000/health

# Port 8000 başka bir şey tarafından kullanılıyor olabilir
# Farklı port kullan:
uvicorn main:app --port 8001

# appsettings.json'da AiService:BaseUrl'i güncelle
```

### ❌ Migration hatası

```bash
# Migration'ı sıfırla
dotnet ef database drop --force
dotnet ef migrations remove
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### ❌ Python model indirme hatası

```bash
# Model'i manuel indir
python3 -c "from sentence_transformers import SentenceTransformer; SentenceTransformer('all-MiniLM-L6-v2')"
```

### ❌ Apple Silicon (M1/M2/M3) PyTorch sorunu

Apple Silicon Mac'lerde PyTorch MPS backend kullanır. Eğer sorun çıkarsa:

```bash
# MPS'i devre dışı bırak
export PYTORCH_MPS_HIGH_WATERMARK_RATIO=0.0

# Veya pip ile CPU-only PyTorch kur (daha hafif):
pip3 install torch --index-url https://download.pytorch.org/whl/cpu
pip3 install sentence-transformers
```

### ❌ Port çakışması

macOS'ta 5000 portu AirPlay Receiver tarafından kullanılıyor olabilir. İki seçenek:

**Seçenek 1:** AirPlay Receiver'ı kapat → System Settings > General > AirDrop & Handoff > AirPlay Receiver → kapat

**Seçenek 2:** Farklı port kullan:

```bash
dotnet run --urls "http://localhost:5050"
```

Bu durumda frontend ve curl komutlarında da 5050 kullan.

---

## 📋 Servisleri Başlatma Sırası

Her seferinde şu sırayla başlat:

```
1. PostgreSQL       →  zaten arka planda çalışıyor olmalı
2. .NET Backend     →  cd backend/MatchingApi && dotnet run
3. Python AI Servis →  cd ai-service && source venv/bin/activate && uvicorn main:app --port 8000
4. Frontend         →  cd frontend && npm run dev  (ileride)
```

> 💡 **İpucu:** Her servisi ayrı bir terminal penceresinde çalıştır.

---

## 🌐 Ortam Değişkenleri Özeti

| Değişken | Nerede | Varsayılan | Açıklama |
|----------|--------|------------|----------|
| `ConnectionStrings:DefaultConnection` | appsettings.json | localhost:5432 | PostgreSQL bağlantısı |
| `AiService:BaseUrl` | appsettings.json | http://localhost:8000 | Python AI servis adresi |
| `AiService:UseLlm` | appsettings.json | false | LLM reranking aktif mi |
| `Frontend:Url` | appsettings.json | http://localhost:3000 | CORS için frontend adresi |
| `LLM_PROVIDER` | Env variable | openai | "openai" veya "anthropic" |
| `LLM_API_KEY` | Env variable | (boş) | API anahtarı (opsiyonel) |
| `LLM_MODEL` | Env variable | gpt-4o-mini | Kullanılacak model |

---

## 📈 Sonraki Adımlar

1. **Frontend geliştirme** — Next.js + TailwindCSS ile arayüz
2. **Yatırımcı scraping** — LinkedIn/Crunchbase'den otomatik veri toplama
3. **Feedback loop** — Kullanıcı geri bildirimiyle skor ağırlıklarını ayarlama
4. **Docker Compose** — Tüm servisleri tek komutla başlatma
5. **CI/CD** — GitHub Actions ile otomatik deployment

---

*Bu rehber, projenin v1.0 sürümüne aittir. Sorularınız için: devam eden konuşmamızda sorun!* 🎯
