# Bridge API - Holistic System & Endpoint Documentation / Bütünsel Sistem ve Endpoint Dokümantasyonu

(EN) This document brings together the three main pillars of the project (Social Platform, Matching Service, and AI Engine) and their technical details.
(TR) Bu doküman, projenin üç ana sütununu (Sosyal Platform, Eşleştirme Servisi ve Yapay Zeka Motoru) ve bunların teknik detaylarını bir araya getirir.

---

## 1. GENERAL ARCHITECTURE / GENEL MİMARİ ŞEMA

(EN) The system consists of a Clean Architecture-based social platform and specialized microservices:
(TR) Sistem, Clean Architecture tabanlı bir sosyal platform ve ona bağlı uzmanlaşmış mikroservislerden oluşur:

1. **Bridge Platform (BridgeApi.API)**
   (EN) User management, social feed, messaging, and connections.
   (TR) Kullanıcı yönetimi, sosyal akış, mesajlaşma ve bağlantılar.

2. **Matching Engine (MatchingApi)**
   (EN) Investor-Startup and Startup-Startup synergy analysis.
   (TR) Yatırımcı-Girişim ve Girişim-Girişim uyum analizleri.

3. **AI Service (FastAPI)**
   (EN) Vectorization, pgvector search, and LLM reranking.
   (TR) Vektörleştirme, pgvector araması ve LLM reranking.

---

## 2. LAYER 1: SOCIAL PLATFORM (BridgeApi.API) - Port 5000/5001 / KATMAN 1: SOSYAL PLATFORM

(EN) This layer is the backend of the main interface the user interacts with.
(TR) Bu katman kullanıcının etkileşimde olduğu ana arayüzün backendidir.

### 2.1. Authentication (Auth) / Kimlik Doğrulama
- **[POST] /api/v1/auth/login**
  (EN) Log in a user.
  (TR) Kullanıcı girişi yapar.
  - **Body**: `{"email": "string", "password": "string"}`
  - **Response**: (EN) JWT Token and user info. (TR) JWT Token ve kullanıcı bilgileri.

### 2.2. Social Feed (Posts & Comments) / Sosyal Akış
- **[POST] /api/v1/posts**
  (EN) Share a new post/announcement.
  (TR) Yeni bir duyuru/post paylaşır.
  - **Body**: `{"content": "text", "type": "News|Need|Opportunity"}`
- **[GET] /api/v1/posts**
  (EN) List posts from followed users.
  (TR) Takip edilen kişilerin postlarını listeler.

### 2.3. Connections / Bağlantılar
- **[POST] /api/v1/connections/request**
  (EN) Send a connection request to a user.
  (TR) Bir kullanıcıya bağlantı isteği gönderir.
  - **Body**: `{"targetUserId": "guid"}`

---

## 3. LAYER 2: MATCHING ENGINE (MatchingApi) - Port 5002 / KATMAN 2: EŞLEŞTİRME SERVİSİ

(EN) Finds the most accurate matches in the ecosystem using AI and rule engines.
(TR) Yapay zeka ve kural motorunu kullanarak ekosistemdeki en doğru eşleşmeleri bulur.

### 3.1. Smart Matching / Akıllı Eşleştirme
- **[POST] /api/v1/match/ai-powered**
  (EN) List startups for an investor.
  (TR) Yatırımcı için girişimleri listeler.
  - **Body**: `{"investorId": "guid", "topN": 10}`
  - **Function**: (EN) Hybrid score combining Rule Engine (60%) and AI service (40%). (TR) Kural motoru (%60) ve AI servisini (%40) birleştirerek hibrit skor üretir.

### 3.2. B2B Networking (Networking Events)
- **[POST] /api/v1/match/event/match-startup**
  (EN) Measure synergy between startups in an event.
  (TR) Bir etkinliğe katılan girişimler arasındaki sinerjiyi ölçer.
  - **Params**: `sourceStartupId` (guid), `eventId` (int).
  - **Response**: (EN) Potential partners and "AI Reason" explanations. (TR) Potansiyel iş ortağı adayları ve "AI Reason" açıklamaları.

---

## 4. LAYER 3: AI SERVICE (Python) - Port 8000 / KATMAN 3: YAPAY ZEKA SERVİSİ

(EN) Manages heavy mathematical and NLP processes.
(TR) Ağır matematiksel ve NLP süreçlerini yönetir.

### 4.1. Semantic Analysis / Semantik Analiz
- **[POST] /api/v1/semantic-match**
  (EN) Measure semantic similarity between text blocks.
  (TR) İki metin bloğu arasındaki anlam benzerliğini ölçer.
  - **Body**: `{"investor_text": "...", "startups": [{"id": "guid", "text": "..."}]}`
  - **Function**: (EN) Performs cosine similarity search on pgvector. (TR) pgvector üzerinde kosinüs benzerliği araması yapar.

---

## 5. INTER-SERVICE COMMUNICATION & INFRASTRUCTURE / SERVİSLER ARASI İLETİŞİM VE ALTYAPI

- **Unified Database / Veritabanı Birleşimi**
  (EN) Both BridgeApi.API and MatchingApi share the same Neon DB.
  (TR) Hem ana platform hem de MatchingApi aynı Neon DB'yi ortaklaşa kullanır.

- **Shared Entities / Ortak Modeller**
  (EN) Profiles are managed via `BridgeApi.Shared` library with unified string-based GUIDs.
  (TR) Profiller, merkezi string-tabanlı GUID'ler kullanılarak `BridgeApi.Shared` üzerinden yönetilir.

- **Data Sync / Veri Senkronizasyonu**
  (EN) When a profile is updated, `StartupIndexingWorker` detects the change and requests new vectors from the AI Service.
  (TR) Bir profil güncellendiğinde, `StartupIndexingWorker` değişikliği algılar ve AI servisinden yeni vektörler talep eder.

- **Security / Güvenlik**
  (EN) Communication is protected via internal networks or private API keys.
  (TR) Mikroservisler arası iletişim dahili ağ üzerinden veya özel API anahtarları ile korunur.
