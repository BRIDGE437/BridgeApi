# Bridge API - Advanced Startup & Investor Matching Platform

(EN) Bridge API is a comprehensive ecosystem designed to connect startups and investors through social networking and AI-powered matching logic. It integrates a .NET-based social platform with a hybrid matching engine (Rule-based + AI-powered Semantic Search).

(TR) Bridge API, sosyal ağlar ve yapay zeka destekli eşleştirme mantığı aracılığıyla girişimleri ve yatırımcıları bir araya getirmek için tasarlanmış kapsamlı bir ekosistemdir. .NET tabanlı bir sosyal platformu, hibrit bir eşleştirme motoruyla (Kural tabanlı + AI destekli Semantik Arama) birleştirir.

---

## 🚀 Key Features / Temel Özellikler

- **AI-Powered Matching**: Uses `pgvector` and Sentence-Transformers for semantic similarity. / **AI Destekli Eşleştirme**: Semantik benzerlik için `pgvector` ve Sentence-Transformers kullanır.
- **Unified Identity**: GUID-based authentication system shared across all microservices. / **Birleşik Kimlik**: Tüm mikroservisler arasında paylaşılan GUID tabanlı kimlik doğrulama sistemi.
- **Hybrid Scoring**: Combines business rules (%60) with semantic AI analysis (%40). / **Hibrit Skorlama**: İş kurallarını (%60) semantik yapay zeka analiziyle (%40) birleştirir.
- **Social Networking**: Post sharing, commenting, and professional connection requests. / **Sosyal Ağ**: Post paylaşımı, yorum yapma ve profesyonel bağlantı istekleri.
- **B2B Synergy**: Measures synergy between startups for networking events. / **B2B Sinerji**: Etkinlikler için girişimler arası sinerjiyi ölçer.

---

## 🏗️ Architecture / Mimari Yapı

The project is built on a **Modular Monolith/Microservices** hybrid architecture:
Proje, **Modüler Monolit/Mikroservis** hibrit mimarisi üzerine inşa edilmiştir:

1.  **BridgeApi.API** (.NET 8):
    (EN) Central hub for user management and social features.
    (TR) Kullanıcı yönetimi ve sosyal özellikler için merkezi merkez.
2.  **MatchingApi** (.NET 8):
    (EN) Specialized engine for calculating similarity scores and managing events.
    (TR) Benzerlik skorlarını hesaplayan ve etkinlikleri yöneten özel motor.
3.  **BridgeApi.Shared**:
    (EN) Common library containing `StartupProfile`, `InvestorProfile`, and shared DTOs.
    (TR) `StartupProfile`, `InvestorProfile` ve paylaşılan DTO'ları içeren ortak kütüphane.
4.  **AI Microservice** (Python/FastAPI):
    (EN) Handles vector embeddings, LLM reranking, and semantic search queries.
    (TR) Vektör gömülmeleri, LLM reranking ve semantik arama sorgularını yönetir.

---

## 🛠️ Tech Stack / Teknoloji Yığını

- **Backend**: .NET 8 (C#), Entity Framework Core
- **AI/ML**: Python 3.10, FastAPI, Sentence-Transformers (all-MiniLM-L6-v2)
- **Database**: PostgreSQL with **pgvector** (Hosted on Neon DB)
- **Security**: JWT Authentication, Identity Framework
- **Infrastructure**: Asynchronous Background Workers (Worker Services)

---

## 📂 Project Structure / Proje Yapısı

```text
BridgeApi/
├── Core/
│   └── BridgeApi.Shared/       # Shared Entities & DTOs
├── Infrastructure/
│   └── BridgeApi.Persistence/  # Central DbContext & Migrations
├── Presentation/
│   └── BridgeApi.API/          # Main Social Platform API
├── backend/
│   └── MatchingApi/            # Matching & AI Logic Hub
└── ai_service/                 # Python FastAPI AI Microservice
```

---

## ⚙️ Setup & Installation / Kurulum Notları

### 1. Database / Veritabanı
(EN) Ensure you have a PostgreSQL database with `pgvector` enabled.
(TR) `pgvector` eklentisi etkinleştirilmiş bir PostgreSQL veritabanına sahip olduğunuzdan emin olun.
```sql
CREATE EXTENSION IF NOT EXISTS vector;
```

### 2. Configuration / Yapılandırma
(EN) Set your connection strings and AI service URLs in `appsettings.json` or environment variables.
(TR) Bağlantı dizelerini ve AI servis URL'lerini `appsettings.json` veya çevre değişkenlerinde ayarlayın.

### 3. Run APIs / Çalıştırma
```bash
# Bridge API
dotnet run --project Presentation/BridgeApi.API

# Matching API
dotnet run --project backend/MatchingApi

# AI Service
cd ai_service && uvicorn main:app --reload
```

---

## 📄 Documentation / Dokümantasyon

(EN) For detailed endpoint lists and system architecture, see the [raporlar](./raporlar/) directory.
(TR) Detaylı endpoint listeleri ve sistem mimarisi için [raporlar](./raporlar/) dizinine bakın.

- [System Architecture / Sistem Mimarisi](./raporlar/BridgeApi_Tum_Sistem_Mimarisi.md)
- [Integration Report / Entegrasyon Raporu](./raporlar/MatchingApi_Entegrasyon_Raporu.md)
