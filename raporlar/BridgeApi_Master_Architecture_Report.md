# BridgeApi & AI Matching Engine - Comprehensive Architecture Master Report
*(Kapsamlı Ana Mimari ve Entegrasyon Raporu TR/EN)*

---

## 1. Executive Summary & System Philosophy / Genel Bakış ve Sistem Felsefesi
**(EN)** BridgeApi is not just a standard CRUD application; it is an intelligent, event-driven ecosystem designed to seamlessly connect Startups, Investors, and B2B partners. The core philosophy of the system relies on decoupling heavy AI tasks from the primary database operations. By utilizing mathematical vectors stored directly inside PostgreSQL and leveraging Large Language Models (LLMs) for final human-like synergy evaluations, BridgeApi ensures unparalleled matching accuracy at scale.

**(TR)** BridgeApi sıradan bir CRUD uygulaması değil; Girişimleri, Yatırımcıları ve B2B partnerlerini sorunsuz bir şekilde bağlamak için tasarlanmış akıllı ve olay tabanlı (event-driven) bir ekosistemdir. Sistemin temel felsefesi, ağır yapay zeka görevlerini ana veritabanı operasyonlarından izole etmektir (decoupling). Doğrudan PostgreSQL içinde saklanan matematiksel vektörleri ve nihai insansı sinerji değerlendirmeleri için Büyük Dil Modellerini (LLM) kullanarak büyük ölçekte benzersiz bir eşleştirme doğruluğu sağlar.

---

## 2. Technology Stack & Infrastructure / Teknoloji Yığını ve Altyapı
Sistem üç ana sacayağı üzerine inşa edilmiştir:

1. **.NET 8 (C#) Backend API:** 
   - **Görevi:** Kullanıcı kimlik doğrulama (Identity), veritabanı yönetimi (Entity Framework Core), asenkron arka plan görevleri (Background Workers) ve istemcilere veri sunma işlemlerini üstlenir.
   - **Avantajı:** Güçlü tip güvenliği (strict typing) ve binlerce eşzamanlı isteği çökmeden kaldırabilme kapasitesi.

2. **Python FastAPI (AI Service):** 
   - **Görevi:** C# tarafından gönderilen verileri alır, Embedding modelleri ile vektörize eder ve Gemini gibi LLM modelleriyle iletişim kurarak Reranking (Yeniden Puanlama) işlemlerini yapar.
   - **Avantajı:** Python'ın yapay zeka ekosistemindeki tartışılmaz üstünlüğü ve FastAPI'ın sunduğu asenkron, ışık hızında yanıt süreleri.

3. **Neon PostgreSQL & `pgvector`:**
   - **Görevi:** Tüm uygulama verilerini (`AppUser`, `StartupProfile`) ve 384 boyutlu vektör verilerini aynı merkezi bulutta tutmak.
   - **Avantajı:** Vektör benzerlik hesaplamalarını (`<=>` Kosinüs Benzerliği) Python'da RAM tüketerek yapmak yerine doğrudan SQL seviyesinde hesaplayıp ağ (network) darboğazını ortadan kaldırması.

---

## 3. Data Ingestion & Deduplication Module / Veri Aktarımı ve Tekilleştirme Modülü

Bu modül, dışarıdan gelen kirli veriyi temizleyerek sistemin ana damarlarına pompalayan en kritik aşamalardan biridir.

### 3.1. Database Merging (3-CSV Unification)
Daha önceden dağınık halde bulunan 3 farklı veri kaynağı (CSV), programatik olarak tek bir `concatted_enriched.csv` dosyasında birleştirilmiştir. Bu sayede sistem için "Tek Bir Gerçeklik Kaynağı" (Single Source of Truth) oluşturulmuştur.

### 3.2. Index-Based Mapping & Culture Invariant
CSV okuma işlemleri, sütun isimlerindeki (Header) Türkçe karakter, BOM (Byte Order Mark) veya büyük/küçük harf tutarsızlıklarından etkilenmemesi için **İndeks Bazlı** (`values[0]`, `values[1]`) yapıya geçirilmiştir. Tüm metin normalizasyon işlemleri `ToLowerInvariant()` kullanılarak Türkçe 'ı/i' karakter hatalarına karşı kurşun geçirmez hale getirilmiştir.

### 3.3. MD5 Fingerprint Deduplication (Parmak İzi ile Tekilleştirme)
Sistemde asla aynı girişimin iki kez yer almaması için dışarıdan gelen ID'ler iptal edilmiştir. Bunun yerine her girişimin **Web Sitesi ve Şirket Adı** birleştirilerek temizlenir ve benzersiz bir MD5 Hash üretilir (`ExternalFingerprint`). Sistem her satırı okuduğunda bu MD5'i kontrol eder; varsa üzerine yazar (Update), yoksa yeni yaratır (Create). Bu sistem sayesinde veri sayısı tam **1726** gerçek girişime sabitlenmiştir.

### 3.4. Identity Email Collision Engine (E-posta Çakışma Çözücüsü)
Verilerdeki en büyük sorun; Wix, Sentry gibi sistemlerden çekilen altyapı e-postalarının birden fazla farklı girişim tarafından kullanılıyor olmasıydı. ASP.NET Identity aynı e-posta ile iki farklı hesaba izin vermediği için girişimler birbirini eziyordu (Overwrite).
- **MakeUniqueEmail:** Çakışan veya şüpheli (Wix/Sentry) e-postalar tespit edildiğinde sistem bunlara `+{ID}` ekleyerek (`sentry+40157@...`) benzersiz hesaplar oluşturur.
- **ContactEmails:** Girişimin asıl sahip olduğu tüm e-postalar virgülle ayrılarak hiçbir veri kaybı olmadan `StartupProfile` tablosunda saklanır.
- **NeedsManualReview:** Çakışma yaşayan hesaplar veritabanında `NeedsManualReview = true` olarak işaretlenir, böylece yöneticiler sonradan sadece bu hesapları filtreleyip düzeltebilir.

---

## 4. AI Vectorization & Indexing (Yapay Zeka Vektörizasyon Motoru)

Sistemin "okuma ve anlama" aşamasıdır.

### 4.1. The Decoupled Trigger Mechanism (İzole Tetikleyici)
1726 girişimin eşzamanlı olarak vektörize edilmesi, OpenAI Rate Limit'lerine (istek sınırına) takılacağı ve C# sunucusunu kilitleyeceği için bu işlem veri aktarımından **tamamen ayrılmıştır (Decoupling)**.

### 4.2. Batch Processing (Grup İşleme)
`POST /api/v1/Indexing/trigger` ucu çağrıldığında sistem:
1. Veritabanında henüz vektörü (`EmbeddingHash`) olmayan veya güncellenmiş girişimleri bulur.
2. Bunları 100'erli paketler (Batch) halinde Python AI servisine iletir.
3. Sadece bu aşamada sistem metinleri anlamsal matematik dizilerine (Vektörlere) çevirir ve kaydeder.
*(Not: Bu aşamada ağır ve pahalı olan LLM (GenAI) kullanılmaz, sadece Embedding işlemi yapılır).*

---

## 5. The Tri-Layer Matching Algorithms / 3 Katmanlı Eşleştirme Algoritmaları

BridgeApi'nin kalbi olan eşleştirme sistemi, performansı ve doğruluğu artırmak için 3 filtreleme katmanından oluşur:

### Layer 1: Rule-Based Filtering (Kural Tabanlı Filtreleme - C#)
- **Mantık:** Veritabanındaki kesin eşleşmeler aranır. (Örn: Yatırımcı Türkiye'de yatırım yapmak istiyor mu? Girişimin Büyüme Aşaması (Stage) yatırımcının portföyüne uyuyor mu?)
- **Amaç:** Yapay zekaya gitmeden önce on binlerce ihtimali saniyeler içinde 100-200 adaya düşürmek.

### Layer 2: Semantic Vector Matching (Anlamsal Vektör Eşleştirme - pgvector)
- **Mantık:** İlk katmandan geçen adayların vektörleri, yatırımcının teziyle (veya diğer girişimin özetiyle) karşılaştırılır. `pgvector` eklentisi sayesinde bu işlem SQL seviyesinde `<=>` operatörü ile anında hesaplanır.
- **Amaç:** Aday sayısını anlamsal benzerliğe göre en iyi 10-20 kişiye düşürmek.

### Layer 3: LLM Reranking (Yapay Zeka Puanlaması - Python Gemini)
- **Mantık:** En iyi adaylar (ID'leri ile birlikte) Python servisine iletilir. Python servisi detaylı verileri veritabanından çeker ve Gemini (LLM) modeline uzman bir Prompt ile gönderir.
- **Amaç:** Sadece kelime benzerliğini değil; "Bu girişimin gelir modeli bu yatırımcının risk iştahına uyuyor mu?" gibi insansı (human-like) mantıksal bir `LlmBonus` skoru üreterek nihai sıralamayı belirlemek.

---

## 6. Event-Driven Architecture & B2B Networking / Etkinlik Tabanlı Sistem ve B2B

Eşleşmeler rastgele zamanlarda değil, planlı etkinlikler (MatchEvent) çatısı altında yapılır.

### 6.1. Startup-to-Startup (B2B) Synergy
Sistem sadece Yatırımcı-Girişim değil, **Girişim-Girişim (B2B)** eşleşmelerini de destekler. Sektörleri, hedefleri ve teknolojileri örtüşen girişimler birbirleriyle stratejik ortaklıklar kurabilmeleri için yukarıdaki 3 katmanlı algoritma kullanılarak puanlanır.

### 6.2. Background Event Workers (Arka Plan İşçileri)
- **`EventMatchingWorker`**: Her dakika uyanarak planlanan etkinliklerin (Event) saatinin gelip gelmediğini kontrol eder. Saati gelen etkinliklerdeki tüm katılımcıları kilitler ve eşleştirmeleri arka planda (kullanıcıları bekletmeden) otomatik olarak yapar. Sonuçları `MatchResult` ve `StartupMatchResult` tablolarına kaydeder.
- **`StartupIndexingWorker`**: Vektörü olmayan yeni girişimleri 24 saatte bir tarar ve otomatik olarak indekslenmeleri için Python servisine gönderir.

---

## 7. Comprehensive Endpoints Guide / Kapsamlı API Rehberi

### [Import & Data Management]
- `POST /api/v1/Import/startups` : CSV'den verileri çeker, temizler, MD5 tekilleştirmesi yapar ve Identity hesaplarını oluşturur.
- `DELETE /api/v1/Import/clear` : Veritabanındaki tüm aktarılmış verileri ve kullanıcıları temizler.

### [AI Vectorization]
- `POST /api/v1/Indexing/trigger` : Arka planda bekleyen verilerin vektörlere (Embedding) dönüştürülme işlemini başlatır.

### [Event Management]
- `POST /api/v1/match/event` : Yeni bir Eşleşme Etkinliği (örn. Demo Day) oluşturur.
- `POST /api/v1/match/event/{id}/join` : Kullanıcıların belirli bir etkinliğe kaydolmasını sağlar.
- `POST /api/v1/match/event/{id}/leave` : Katılımcıyı etkinlikten çıkarır.

### [Match Execution]
- `POST /api/v1/semantic-match/startup-startup` : (Python API) B2B Girişim-Girişim eşleşmesini tetikler.
- `POST /api/v1/semantic-match` : (Python API) Yatırımcı-Girişim eşleşmesini tetikler.
- `POST /api/v1/match/execute-event/{eventId}` : (C# API) Belirli bir etkinlik için kural tabanlı + AI eşleşme kombinasyonunu manuel tetikler (Worker beklenmek istenmiyorsa).
- `GET /api/v1/match/history` : Kullanıcının daha önceki etkinliklerde elde ettiği tüm eşleşme skorlarını ve partnerlerini listeler.
