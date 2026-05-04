# BridgeApi Dokümantasyon Cilt 5: Environment, Deployment & Integration
*(Çevresel Değişkenler, Sistem Entegrasyonu ve Dağıtım Mimarisi TR/EN)*

Bu doküman, BridgeApi sisteminin sunucularda (veya lokalde) nasıl ayağa kalktığını, C# ile Python'un nasıl iletişim kurduğunu, Veritabanı (Neon Serverless) bağlantı ayarlarını ve Rate-Limiting (İstek Sınırı) önlemlerini belgeler.

---

## 1. Veritabanı Mimarisi (Neon PostgreSQL Serverless)
Sistem, klasik bir PostgreSQL yerine **Neon Tech** (Serverless PostgreSQL) kullanmaktadır. Bunun nedeni vektör arama (pgvector) işlemlerinin bulutta ölçeklenebilmesidir.

### 1.1. C# Tarafı (Entity Framework Core)
- C#, veritabanı şemasını yönetmek (Migrations) ve sosyal ağ / kural tabanlı işlemleri gerçekleştirmek için `Npgsql.EntityFrameworkCore.PostgreSQL` kullanır.
- `appsettings.json` içerisindeki `DefaultConnection` ile bağlanır. Vektör işlemleri için EF Core'un `UseVectorSearch()` eklentisi aktif edilmiştir.

### 1.2. Python Tarafı (Connection Pooling)
- Python, vektör işlemlerini ve AI sorgularını yönetmek için `psycopg_pool.AsyncConnectionPool` kullanır.
- **Neden Pool Kullanıyoruz?** Sunucuya saniyede 100 istek geldiğinde veritabanının çökmemesi için Python tarafında maksimum 2 boşta (idle) bağlantı tutan ve en fazla 300 saniye açık kalan bir bağlantı havuzu oluşturduk.
- Python, veritabanı adresini kök dizindeki `.env` dosyasından `PGVECTOR_DATABASE_URL` anahtarıyla okur.

---

## 2. C# ve Python Entegrasyonu (HTTP Köprüsü)
BridgeApi monolit (tek parça) bir sistem değildir; **Mikroservis** mantığıyla C# ve Python olarak ikiye bölünmüştür.

### 2.1. İletişim Protokolü
- İki sistem birbirleriyle RESTful HTTP üzerinden, JSON formatında haberleşir.
- C# tarafındaki `Program.cs` veya `ServiceRegistration.cs` içinde, Python servisine bağlanmak üzere özel bir HTTP Client (`AddHttpClient("AiService")`) tanımlanmıştır.
- Eğer Python servisi 8000 portunda çalışıyorsa, C# eşleştirme algoritmasına geldiğinde `http://localhost:8000/api/v1/semantic-match` adresine otomatik olarak bir POST isteği fırlatır.

### 2.2. Hata Yönetimi (Resilience & Fallback)
- Eğer Python servisi kapalıysa (veya çökertilmişse), C# sistemi **ÇÖKMEZ**. `AiMatchingService` içindeki `try-catch` blokları hatayı yakalar ve kullanıcıya (Yatırımcıya) bir hata göstermek yerine, sadece "Kural Tabanlı (Rule-based)" skorlarla eşleşme sonuçlarını döndürür (`Fallback Mechanism`).

---

## 3. Rate-Limiting ve Ölçeklenebilirlik (Scalability)
Google Gemini (LLM) ve Embedding modelleri çok ağır işlemlerdir. Sistemin API limitlerine takılmaması için uygulanan mimari kararlar:

### 3.1. Batching (Paketleme Sistemi)
- C#'taki `StartupIndexingWorker` veya Toplu Yükleme (`ImportController`) işlemleri, veritabanına 1726 girişimi aynı anda fırlatmaz. Python servisine 100'erlik paketler halinde yollanır.
- Python tarafında `EmbeddingEngine.encode_batch()` metodu, bu 100 metni tek seferde vektöre çevirerek işlem süresini dakikalardan saniyelere indirir.

### 3.2. Top 20 LLM Reranking Sınırı
- Eşleşme arandığında, SQL tabanlı `pgvector` binlerce girişimi milisaniyeler içinde (Cosine Distance) sıralar.
- Ancak her girişimin verisi Gemini'ye yollanmaz! Sadece vektörel olarak **En İyi 20** (Top 20) aday seçilir ve Gemini'nin "Bu şirketler ortak olmalı mı?" promptuna sokulur. Bu sayede Token (Para) tasarrufu ve zaman tasarrufu %90 oranında sağlanır.

---

## 4. Çevresel Değişkenler (Environment Setup)

Sistemin çalışması için gereken temel ayarlar:

**Python (AI Service) - `.env` Dosyası:**
```env
# pgvector bağlantı adresi (Neon DB)
PGVECTOR_DATABASE_URL="postgresql://user:pass@ep-restless...neon.tech/matching_db"

# Gemini LLM Kullanılsın mı? (Bütçe dostu testler için False yapılabilir)
USE_LLM=True

# Google Gemini API Anahtarı
GEMINI_API_KEY="AIzaSyB..."
```

**C# (Backend) - `appsettings.json`:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=ep-restless...neon.tech;Database=matching_db;Username=...;"
  },
  "AiService": {
    "BaseUrl": "http://localhost:8000",
    "UseLlm": true
  }
}
```
