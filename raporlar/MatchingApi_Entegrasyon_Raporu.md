# BridgeApi & MatchingApi Integration Report / Entegrasyon Raporu

## 1. Overview / Genel Bakış
(EN) The MatchingApi service has been decoupled from its isolated `Startup` and `Investor` models and transitioned to use the `StartupProfile` and `InvestorProfile` models under `BridgeApi.Shared.Entities`. This ensures the entire platform (BridgeApi.API and AI Engine) shares a **single database schema** (Neon DB).

(TR) MatchingApi servisi, kendi içerisindeki izole `Startup` ve `Investor` modellerinden arındırılarak, `BridgeApi.Shared.Entities` altındaki `StartupProfile` ve `InvestorProfile` modellerine geçirilmiştir. Bu sayede tüm platform (BridgeApi.API ve AI Engine) **tek bir veritabanı şemasını** (Neon DB) ortaklaşa kullanır hale gelmiştir.

-----------------------

## 2. Changes and Decisions / Yapılan Değişiklikler ve Kararlar

### 2.1. Model and Database Refactoring / Model ve Veritabanı Refactoring
(EN) 
- Local models in `MatchingApi.Models` were removed.
- `AppDbContext` was updated to connect to shared models (`StartupProfiles`, `InvestorProfiles`).
- Due to Identity management, IDs that were previously `int` were converted to `string` (GUID) type to match the main system.
- The Pgvector library was pinned to version `v0.2.0` in the `.NET 8` environment for compatibility with Npgsql 8.x.

(TR)
- `MatchingApi.Models` altındaki lokal modeller silindi.
- `AppDbContext` güncellenerek paylaşılan modellere (`StartupProfiles`, `InvestorProfiles`) bağlandı.
- Kimlik yönetimi (Identity) nedeniyle önceden `int` olan ID'ler, ana sistemle uyumlu olacak şekilde `string` (GUID) tipine dönüştürüldü.
- Pgvector kütüphanesi Npgsql 8.x ile uyumlu olması için `.NET 8` ortamında `v0.2.0` sürümüne sabitlendi.

### 2.2. Service and Controller Updates (Bug Fixes) / Servis ve Kontrolcü Güncellemeleri (Hata Giderimleri)
(EN) Over 100 compilation errors were successfully resolved:
- **AiMatchingService:** Switched from `int` to `string` IDs in AI matching and LLM similarity algorithms. Added null checks for `CompanyName` and nullable fields. Updated `SemanticResult` structure to support string IDs.
- **RuleBasedMatchingService:** Replaced static properties like "ParsedHQ" with on-the-fly `ModelHelpers.ParseCsv` methods since Startup properties changed.
- **MatchController:** Updated navigation properties (`m.Startup`, `m.Investor`) and removed `int.TryParse` blocks in B2B Event matching, participation (Join/Leave), and history endpoints.
- **Worker Services:** Directed `EventMatchingWorker` and `StartupIndexingWorker` asynchronous services to `db.StartupProfiles` and cleaned up incorrect property mappings.

(TR) Toplamda 100'ü aşkın derleme hatası başarıyla giderildi:
- **AiMatchingService:** AI eşleştirme ve LLM benzerlik algoritmalarında `int` ID'lerden string'e geçildi. `CompanyName` ve null kontrolleri eklendi. `SemanticResult` yapısı string ID destekleyecek şekilde güncellendi.
- **RuleBasedMatchingService:** Startup özellikleri değiştiği için statik "ParsedHQ" vb. propertiler yerine anlık `ModelHelpers.ParseCsv` metotlarına geçildi.
- **MatchController:** B2B Event eşleştirme, katılım (Join/Leave) ve geçmiş (History) endpointlerindeki navigasyon özellikleri (`m.Startup`, `m.Investor`) ve `int.TryParse` blokları güncellendi.
- **Worker Servisleri:** `EventMatchingWorker` ve `StartupIndexingWorker` asenkron servisleri `db.StartupProfiles` hedefine yönlendirildi ve hatalı property eşleşmeleri temizlendi.

### 2.3. Automated Data Ingestion, Deduplication & Identity Integration / Veri Aktarımı, Tekilleştirme ve Identity
(EN)
- **Database Merging:** 3 separate CSV databases were programmatically merged into a single `concatted_enriched.csv` dataset, forming the primary truth-source for all startups.
- **Fingerprint Deduplication:** Implemented an MD5-based `ExternalFingerprint` (Normalized Website + Name) to accurately identify unique startups. This ensures the ingestion count drops precisely to 1726 true entities.
- **Identity Email Override Fix:** Resolved the ASP.NET Identity collision where startups sharing technical emails (e.g., Wix/Sentry) overwrote each other. A `MakeUniqueEmail` helper appends `+{ID}` to duplicates, while preserving all original emails in the `ContactEmails` property. Flagged conflicting ones with `NeedsManualReview = true`.
- **Decoupled Vectorization Trigger:** The vectorization process is completely decoupled from the data ingestion. A manual API trigger (`POST /api/v1/Indexing/trigger`) is introduced to batch-process and embed startup descriptions into Pinecone/Qdrant without overwhelming the DB or hitting OpenAI rate limits. LLM Scoring is kept strictly separate for future matching phases.

(TR)
- **Veritabanı Birleşimi:** 3 ayrı CSV veritabanı programatik olarak birleştirilerek tek bir `concatted_enriched.csv` seti oluşturuldu ve sistemin ana veri kaynağı yapıldı.
- **Fingerprint Tekilleştirme:** Girişimleri doğru ayırt edebilmek için MD5 tabanlı `ExternalFingerprint` (Web Sitesi + İsim) kurgulandı. Bu sayede aktarılan kayıt sayısı tam 1726 gerçek girişime sabitlendi.
- **Identity Email Çakışma Çözümü:** Teknik e-postaları (Wix/Sentry) aynı olan girişimlerin birbirini ezmesi (Overwrite) sorunu çözüldü. Çakışan maillere `+{ID}` eklenerek girişler benzersizleştirildi. Gerçek mailler ise `ContactEmails` alanında korumaya alındı. Çakışanlar `NeedsManualReview = true` ile işaretlendi.
- **İzole Vektörizasyon Tetikleyicisi (Trigger):** Veri aktarımı ile vektörizasyon (Embedding) birbirinden ayrıldı. Ana veritabanını ve OpenAI limitlerini yormamak için, verileri sonradan gruplar (batch) halinde okuyup Pinecone/Qdrant'a yazan bir API Tetikleyicisi (`POST /api/v1/Indexing/trigger`) kuruldu. LLM Scoring işlemi ileriki aşamalar için izole edildi.

-----------------------

## 3. Current Status / Güncel Durum
(EN)
- **Compilation:** ✅ All .NET compilation errors (0 Errors) resolved; build successful.
- **Database:** ✅ `vector(384)` tables ready on Neon DB; migrations applied.
- **Dependency:** ✅ `MatchingApi` is now centrally fed via `BridgeApi.Shared`.

(TR)
- **Derleme:** ✅ Tüm .NET derleme hataları (0 Hata) giderildi ve build işlemi başarılı.
- **Veritabanı:** ✅ Neon DB üzerinde `vector(384)` tabloları hazır, migration basıldı.
- **Bağımlılık:** ✅ `MatchingApi` artık `BridgeApi.Shared` üzerinden merkezden besleniyor.

-----------------------

## 4. Next Steps / Sonraki Adımlar
(EN) From this point, APIs can be executed to test the accuracy of AI matching rules using `concatted_enriched.csv` data.
(TR) Bu aşamadan sonra API'lerin çalıştırılarak `concatted_enriched.csv` datasıyla AI eşleştirme kurallarının doğruluğu test edilebilir.
