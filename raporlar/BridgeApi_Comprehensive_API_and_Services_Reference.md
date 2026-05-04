# BridgeApi Comprehensive API & Services Reference Guide
*(Kapsamlı API ve Servisler Referans Kitapçığı TR/EN)*

Bu doküman, BridgeApi projesinin "Nasıl çalıştığını" değil, "İçeride nelerin olduğunu" satır satır belgeleyen resmi teknik şartnamedir. 

---

## 1. C# BACKEND ENDPOINTS (SWAGGER REFERENCE)

### 1.1. Import Controller (`/api/v1/Import`)
Sistemin dış dünya ile veri alışverişini sağlayan ana kapısıdır. Eskiden sadece basit kayıt atarken, artık devasa bir veri temizleme ve tekilleştirme makinesi olarak çalışmaktadır.

**A. `POST /api/v1/Import/startups`**
- **Ne İşe Yarar?** Sunucudaki `concatted_enriched.csv` dosyasını okur, verileri temizler ve veritabanına yazar.
- **Parametre (Request):** Parametre almaz. Dosya yolu içeriden okunur.
- **Ne Döndürür (Response):** `200 OK` (Başarılı kayıt sayısı, hata mesajları).
- **Biz Ne Ekledik?** 
  - **Eskiden:** Verileri doğrudan kaydediyordu, aynı isimde şirket gelince sistemi çökertiyordu veya tekrar kaydediyordu.
  - **Şimdi:** `ExternalFingerprint` (MD5) sistemiyle Web Sitesi ve İsim üzerinden tekilleştirme yapıyor. Identity Email Override hatasını `MakeUniqueEmail` (+ID) ile çözüyor. Bozuk/Sahte mailli hesaplara `NeedsManualReview = true` bayrağı dikiyor.

**B. `DELETE /api/v1/Import/clear`**
- **Ne İşe Yarar?** Test süreçlerini hızlandırmak için veritabanındaki tüm aktarılmış girişimleri ve kullanıcı hesaplarını tamamen siler.
- **Parametre:** Yok.
- **Response:** `200 OK` "All imported startups and their user accounts have been deleted."

---

### 1.2. Indexing Controller (`/api/v1/Indexing`)
Veritabanı ile Yapay Zeka (AI) arasındaki köprüdür. 

**A. `POST /api/v1/Indexing/trigger`**
- **Ne İşe Yarar?** Veritabanında henüz vektörize edilmemiş (veya güncellenmiş) girişim metinlerini Python AI servisine göndererek `pgvector` verilerini oluşturur.
- **Parametre:** `[FromQuery] int batchSize` (Default: 100). Bir seferde AI servisine gönderilecek veri sayısı.
- **Response:** `200 OK` (Kaç adet girişimin başarıyla indekslendiği).
- **Biz Ne Ekledik?** Bu ucu tamamen biz inşa ettik. Amacımız 1726 girişimin aynı anda OpenAI'ye gidip rate limit yemesini ve veritabanını kilitlemesini (Decoupling) önlemekti. 

---

### 1.3. Event Controller (`/api/v1/match/event`)
Etkinlik tabanlı eşleştirme (Event-Driven Matching) sisteminin merkezidir.

**A. `POST /api/v1/match/event`**
- **Ne İşe Yarar?** Yeni bir eşleşme etkinliği (Pitching, Demo Day, Networking) oluşturur.
- **Parametre:** `CreateEventDto` (Başlık, Tarih, Etkinlik Tipi vb.)
- **Response:** `200 OK` (Oluşturulan Etkinliğin ID'si).

**B. `POST /api/v1/match/event/{id}/join`**
- **Ne İşe Yarar?** Sisteme giriş yapmış bir kullanıcının (Girişim veya Yatırımcı) belirtilen etkinliğe katılmasını sağlar.
- **Parametre:** `[FromRoute] string id` (Etkinlik ID'si), `[FromBody] JoinEventDto` (Kullanıcı ID'si).
- **Response:** `200 OK` "Joined event successfully."

**C. `POST /api/v1/match/event/{id}/leave`**
- **Ne İşe Yarar?** Kullanıcıyı etkinlik havuzundan çıkarır.
- **Parametre:** `[FromRoute] string id`, `[FromBody] LeaveEventDto`.
- **Response:** `200 OK` "Left event successfully."

---

### 1.4. Match Controller (`/api/v1/match`)
Eşleşme işlemlerinin sonuçlarını yönetir.

**A. `POST /api/v1/match/execute-event/{eventId}`**
- **Ne İşe Yarar?** Arka planda çalışan Worker'ı beklemeden, belirtilen etkinlikteki katılımcılar için Kural Tabanlı + AI eşleştirmelerini anında tetikler.
- **Parametre:** `[FromRoute] string eventId`.
- **Response:** `200 OK` (Oluşan eşleşme sayısı ve skorları).

**B. `GET /api/v1/match/history`**
- **Ne İşe Yarar?** Kullanıcının geçmiş etkinliklerde elde ettiği tüm eşleşme skorlarını (`MatchResult` ve `StartupMatchResult`) listeler.
- **Parametre:** `[FromQuery] string userId`.
- **Response:** `List<MatchResultDto>` (Karşı tarafın bilgileri, etkinlik adı, Rule Score, Semantic Score ve LLM Bonus'u içeren detaylı liste).

---

## 2. PYTHON AI ENGINE ENDPOINTS (FASTAPI REFERENCE)

### 2.1. Semantic Match API (`/api/v1/semantic-match`)
Veritabanı (`pgvector`) ve Büyül Dil Modeli (Gemini) kullanılarak anlamsal benzerlik kuran zeka katmanı.

**A. `POST /api/v1/semantic-match` (Yatırımcı -> Girişim)**
- **Ne İşe Yarar?** Yatırımcının tezini (Thesis) alır, veritabanındaki tüm girişimlerin vektörleriyle karşılaştırır ve en iyi adayları LLM'e (Gemini) sokup nihai puanı çıkarır.
- **Parametre (Request):** 
  ```json
  {
    "investor_id": "string",
    "investor_thesis": "string",
    "target_startup_ids": ["string", "string"], // Sadece etkinlikteki katılımcılar
    "top_k": 10
  }
  ```
- **Response:** 
  ```json
  [
    {
      "startup_id": "string",
      "vector_score": 0.85,
      "llm_bonus": 0.12,
      "total_score": 0.97,
      "reasoning": "This startup matches the investor's focus on AI tech..."
    }
  ]
  ```

**B. `POST /api/v1/semantic-match/startup-startup` (Girişim -> Girişim / B2B)**
- **Ne İşe Yarar?** Bir girişimin diğer girişimlerle olan sinerjisini (B2B Ortaklık) hesaplar.
- **Parametre:** `source_startup_id`, `target_startup_ids`, `top_k`.
- **Response:** Yatırımcı eşleşmesine benzer şekilde `total_score` ve `reasoning` (neden ortak olmalılar) döner.
- **Biz Ne Ekledik?** Eskiden sadece Yatırımcı aranıyordu. Sistemi tamamen baştan modifiye edip **B2B Sinerji Algoritmasını** ekledik.

---

## 3. C# CORE SERVICES & WORKERS (DEEP DIVE)

### 3.1. `CsvImportService`
- **Görevi:** CSV'den veritabanına ham veri aktarmak.
- **Biz Üzerine Ne Ekledik?**
  - **Index-Based Mapping:** Sütun isimleri ("Startup Name", "HQ") bozuk olabileceği için kolonları indeks sırasına göre okuyan (`values[1]`) kırılmaz bir yapı kurduk.
  - **MD5 Fingerprinting:** `GenerateFingerprint()` metodunu yazdık. Web sitesi ve İsim normalleştirilip `ExternalFingerprint` (MD5) oluşturuluyor. Bu sayede veritabanına aynı şirket asla iki kez girmiyor.
  - **MakeUniqueEmail Logic:** Sentry/Wix gibi çöp ve ortak e-postaları yakalayıp sonlarına `+{ID}` ekleyerek Identity Overwrite (Ezme) hatasını çözdük.
  - **NeedsManualReview:** Hatalı olanlara flag dikerek geriye dönük sorgulanabilir yaptık.

### 3.2. `AiMatchingService`
- **Görevi:** C# ile Python AI servisi arasındaki iletişimi (HTTP Client) yönetir.
- **Biz Üzerine Ne Ekledik?** `int` olan ID yapılarını `string`'e çevirdik. Etkinlik bazlı filtreleme için Python servisine `target_startup_ids` dizisini gönderme yeteneği ekledik (böylece tüm DB taranmıyor, sadece etkinliktekiler taranıyor).

### 3.3. `RuleBasedMatchingService`
- **Görevi:** Sektör, Büyüme Aşaması, Lokasyon gibi sert (hard) filtreleri veritabanında LINQ ile saniyeler içinde hesaplayıp bir `RuleScore` oluşturur.
- **Biz Üzerine Ne Ekledik?** B2B eşleşmeleri için `MatchStartupsAsync` metodunu yazdık. İki girişimin sektörlerinin ve teknoloji etiketlerinin birbirine uyumunu hesaplayan yepyeni bir matematiksel sinerji formülü ekledik.

### 3.4. `EventMatchingWorker` (Background Service)
- **Görevi:** Arka planda 7/24 çalışır. Her dakika uyanır ve `MatchEvent` tablosundaki etkinliklerin saati geldi mi diye kontrol eder.
- **Biz Üzerine Ne Ekledik?** Sistemi baştan yazdık. Saati gelen etkinlikleri kilitler (`Status = Processing`), içindeki kullanıcıları çeker ve eşleştirmeleri HTTP isteklerini yormadan asenkron (arka planda) hesaplayıp sonuçları `MatchResult` tablosuna yazar.

### 3.5. `StartupIndexingWorker` (Background Service)
- **Görevi:** 24 Saatte bir uyanır. `EmbeddingHash` (Vektör) değeri NULL olan veya metni güncellenmiş girişimleri bulup `IndexingController`'ı otomatik olarak tetikler.

---

## 4. PYTHON AI SERVICES (DEEP DIVE)

### 4.1. `vector_store.py` (pgvector Optimizer)
- **Görevi:** Vektör benzerlik hesaplamalarını (Cosine Similarity) yapar.
- **Biz Üzerine Ne Ekledik?** İnanılmaz bir performans optimizasyonu yaptık. Eskiden Python, veritabanından devasa matrisleri (`[384]`) çekip NumPy ile kendi RAM'inde hesaplıyordu. Biz `get_similarities_sql` metodunu yazarak bu hesaplamayı doğrudan PostgreSQL'in içine (SQL Seviyesine) ittik. Ağ (Network) kullanımı %99 oranında düştü.

### 4.2. `reranker.py` (Gemini LLM Integration)
- **Görevi:** Sadece anlamsal kelime benzerliğine değil, yatırım tezindeki mantıksal uyuma bakarak son kararı (Reranking) verir.
- **Biz Üzerine Ne Ekledik?** B2B eşleşmeleri için özel bir "Synergy Expert Prompt" yazdık. "Bu iki şirket ortak olabilir mi?" sorusunu Google Gemini'a soran ve dönen yanıtı JSON olarak `LlmBonus` skoruna çeviren `call_gemini` fonksiyonunu tamamen revize ettik. (DRY prensibiyle kod tekrarını önlemek için `rerank_by_ids` metodunu merkezileştirdik).

---

## 5. DATABASE SCHEMA SUMMARY (Neon PostgreSQL)

1. **AppUser:** Identity tablosu (Email, Şifre, Login işlemleri).
2. **StartupProfile:** Girişim detayları. Biz ekledik: `ExternalFingerprint`, `ContactEmails`, `NeedsManualReview`.
3. **InvestorProfile:** Yatırımcı detayları ve yatırım tezi (Thesis).
4. **MatchEvent & EventParticipation:** Etkinlikleri ve katılımcı listelerini tutar (Yeni Eklendi).
5. **MatchResult & StartupMatchResult:** Eşleşme skorlarının (RuleScore, SemanticScore, LlmBonus) tarihçesiyle beraber kalıcı olarak saklandığı arşiv tablolarıdır (Yeni Eklendi).
