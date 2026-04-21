# JSON Startup Indexing Implementation Plan

Mevcut sistemde girişimleri (startups) kaydetmek için sadece CSV yükleme ucu bulunuyordu. Bu plan, doğrudan JSON verisi göndererek girişimleri sisteme kaydetmeyi ve vektörlemeyi sağlayan bir uç (endpoint) eklemeyi amaçlamaktadır.

## Proposed Changes

### 1. AI Servisi Bağlantısı (AiMatchingService.cs)
C# üzerinden Python yapay zeka servisine (AI Microservice) veri göndermek için gerekli bağlantı metodu eklenecek.

#### [MODIFY] [AiMatchingService.cs](file:///c:/Users/nefise/Desktop/BridgeApi/backend/MatchingApi/Services/AiMatchingService.cs)
- `IndexStartupsAsync(List<Startup> startups)` metodu eklenecek.
- Bu metod, girişimlerin metinsel verilerini (isim, sektör, açıklama vb.) birleştirip Python servisindeki `/api/v1/index-startups` ucuna JSON olarak gönderecek.
- Python servisi bu verileri vektörlere (embedding) çevirip PostgreSQL veritabanındaki `vector` kolonuna kaydedecek.

### 2. Yeni Endpoint Ekleme (MatchController.cs)
Swagger üzerinden veya dış sistemlerden doğrudan JSON ile veri alınmasını sağlayacak uç tanımlanacak.

#### [MODIFY] [MatchController.cs](file:///c:/Users/nefise/Desktop/BridgeApi/backend/MatchingApi/Controllers/MatchController.cs)
- `[HttpPost("index-startups")]` rotasıyla yeni bir metod eklenecek.
- `[FromBody] List<Startup> startups` parametresi ile liste halinde girişim verisi alınacak.
- Gelen veriler PostgreSQL ana veritabanına kaydedilecek. (Varsa güncellenecek, yoksa yeni eklenecek).
- `_db.SaveChangesAsync()` sonrası `_aiEngine.IndexStartupsAsync` tetiklenerek vektörleme işlemi başlatılacak.

## Verification Plan
1. **API Testi**: Swagger arayüzü (veya cURL) kullanılarak mock JSON datası ile `/api/v1/match/index-startups` ucuna istek atılacak.
2. **Database Testi**: Neon veritabanında `Startups` tablosuna kayıtların eklendiği kontrol edilecek.
3. **AI Testi**: Eklenen girişimlerin `Embedding` kolonlarının Python servisi tarafından başarıyla doldurulduğu teyit edilecek.
