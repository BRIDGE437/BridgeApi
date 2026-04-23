# Bridge API - Geliştirme Raporu

Bu rapor, proje üzerinde bu zamana kadar gerçekleştirilen hata düzeltmelerini, mimari değişiklikleri ve eklenen yeni özellikleri detaylandırmaktadır.

## 1. Veritabanı ve Bağlantı Sorunlarının Çözülmesi

> [!WARNING]
> Neon PostgreSQL veritabanı ile hem C# hem de Python servislerinde yaşanan spesifik bağlantı ve zaman aşımı sorunları tamamen çözüldü.

- **C# Tarafı (Npgsql Uyumluluğu):** 
  - Neon veritabanı bağlantı metinleri (URL formatı) Npgsql'in okuyabileceği standart `Host=...;Password=...` formatına çevrildi (`Program.cs`). Desteklenmeyen `channel_binding` gibi parametrelerin çökertmesi engellendi.
  - Uygulama sadece "Development" modundayken çalışan **Swagger UI**, her zaman çalışacak şekilde güncellendi. Artık terminalden `dotnet run` yazıldığında doğrudan Swagger sayfasına erişilebiliyor.

- **Python Tarafı (Asenkron Döngü ve SSL Hataları):**
  - **Uvicorn Kilitlenmesi:** Windows'ta `psycopg` kütüphanesinin ihtiyaç duyduğu `WindowsSelectorEventLoopPolicy` ayarı, Uvicorn'un komut satırından çalıştırılması yüzünden eziliyordu. `main.py` içerisine `if __name__ == "__main__":` bloğu eklenerek Uvicorn'un doğru asenkron döngü ile başlaması zorunlu kılındı.
  - **Neon Idle Connection Drop:** Sunucunun boşta kalan bağlantıları kesmesi sebebiyle alınan "SSL connection has been closed unexpectedly" hatası çözüldü. Bağlantı havuzuna (`AsyncConnectionPool`) `keepalives` parametreleri eklenerek sunucuya düzenli ping atması sağlandı.

---

## 2. JSON Tabanlı Girişim Kaydetme (Direct Import)

Daha öncesinde sadece CSV dosyası ile veri kabul eden sisteme, doğrudan JSON veri altyapısı kuruldu.

- **`MatchController.cs`**: `POST /api/v1/match/index-startups` isimli yeni bir uç (endpoint) açıldı.
- **`AiMatchingService.cs`**: C# üzerinden gelen yeni girişimlerin metinlerini Python AI servisine gönderip vektörlemeyi (embedding) başlatan bağlantı eklendi.
- **İş Akışı**: Dışarıdan gelen JSON verisi önce ana veritabanına kaydediliyor, ardından Python servisi tetiklenerek bu verilerin 384-boyutlu matematiksel karşılıkları `pgvector` eklentisiyle işleniyor.

---

## 3. Etkinlik Tabanlı Planlanmış Eşleşme Sistemi (Event-Based Matching)

Sistemi manuel buton tetiklemelerinden kurtarıp tam otomatik ve etkinlik odaklı hale getirecek dev mimari güncellemesi yapıldı.

### A. Yeni Veritabanı Modelleri
- **`MatchEvent`**: Etkinlik bilgilerini tutar (Başlık, Planlanan Zaman, Katılımcı Kısıtı vb.).
- **`EventParticipation`**: Etkinliğe katılan Yatırımcı ve Girişimleri eşleştirir.
- **`MatchResult`**: Eski eşleşme sonuçlarına `EventId` kolonu eklendi, böylece hangi eşleşmenin hangi etkinlikte yapıldığı takip edilebilir hale geldi.

### B. Otomasyon (Background Workers)
- **`StartupIndexingWorker`**: Arka planda 24 saatte bir çalışır. Veritabanındaki yeni/vektörü olmayan girişimleri bulup Python AI servisine gönderir ve etkinlik saati gelmeden tüm vektörlerin hazır olmasını sağlar.
- **`EventMatchingWorker`**: Her dakika uyanıp etkinlik saatini kontrol eder. Saati gelen "Open" durumundaki etkinlikleri bulur, içerisindeki katılımcıları çeker ve eşleştirme sürecini toplu halde arka planda yürütür.

### C. API Uçları ve Eşleşme Kısıtı
- **`EventController.cs`**: Etkinlikleri oluşturmak ve listelemek için eklendi.
- **Katılım/Ayrılma**: Yatırımcıların ve girişimlerin etkinliklere kayıt olması için `POST /api/v1/match/event/{id}/join` ucu eklendi.
- **`AiMatchingService.cs`**: Eşleştirme algoritması yeniden düzenlendi. Artık tüm veritabanını taramak yerine, sadece ilgili etkinliğe kayıt olan (`startupIds`) girişimler arasında semantik ve kural tabanlı arama yapacak şekilde sınırlandırıldı.

### D. SDK ve Migration Yönetimi
- **Sürüm Çakışması**: Projedeki .NET 10 ve .NET 8 uyumsuzlukları giderildi. Geçici `global.json` kullanılarak `dotnet-ef` aracı .NET 8'e düşürüldü ve `AddEventMatchingModels` migration'ı veritabanı için başarıyla hazırlandı.

---

## 4. Vektör Eşleşmelerinin SQL Seviyesine İndirilmesi (pgvector Optimizasyonu)

Bu sürecin en başında, yapay zeka eşleştirme motorunun (Python) bellek (RAM) ve ağ (Network) darboğazına girmesini önleyecek muazzam bir mimari optimizasyon yapıldı.

- **NumPy'dan PostgreSQL'e Geçiş**: Daha önce Python tarafına yüzlerce girişimin `[384]` boyutlu vektörleri çekilip NumPy ile Kosinüs Benzerliği hesaplanıyordu. Bu işlem veritabanı seviyesine indirildi.
- **`vector_store.py` Güncellemesi**: `get_similarities_sql` metodu yazılarak hesaplamaların PostgreSQL `pgvector` eklentisi kullanılarak (`<=>` operatörü ile) yapılması sağlandı.
- **Kazanım**: Artık devasa vektör dizileri network üzerinden taşınmıyor; veritabanı bu dizileri kendi içinde çarpıp `(id, skor)` olarak Python'a geri döndürüyor. Bu sayede bellek kullanımı inanılmaz düştü ve hız katlandı.

---

## 5. Çevresel Değişken (Environment) ve Güvenlik Altyapısı

Projedeki tüm şifrelerin ve ayarların kod içerisinden arındırılması sağlandı.

- **`.env` Entegrasyonu (C#)**: `Program.cs` içerisine `.env` dosyasını okuyan özel bir kod bloğu eklendi. Uygulama başlatıldığında veritabanı bağlantı cümlesi (`PGVECTOR_DATABASE_URL`) otomatik olarak buradan okunuyor.
- **Python Tarafı**: `dotenv` kütüphanesi kullanılarak API anahtarları (Gemini) ve veritabanı adresleri `os.getenv` aracılığıyla kod dışına çıkarıldı.

---

## 6. Yapay Zeka LLM Reranking (Gemini Entegrasyonu)

Eşleştirme kalitesini artırmak için projeye Büyük Dil Modelleri (LLM) dahil edildi.

- **`call_gemini` Fonksiyonu**: Python tarafındaki `reranker.py` içerisine, Google Gemini API ile konuşan ve yatırımcı ile girişim arasındaki uyumu metinsel olarak analiz eden bir fonksiyon (`call_gemini`) yazıldı.
- **Mantık**: Vector (Semantik) eşleşmesinden başarıyla geçen ilk X (örneğin ilk 20) aday, son aşamada Gemini modeline gönderilerek LLM tabanlı olarak yeniden puanlanıyor (Reranking) ve `LlmBonus` olarak sisteme dahil ediliyor. Bu sayede sadece kelime benzerliği değil, yatırım vizyonu uyumu da ölçülmüş oluyor.

---

## 7. B2B Networking: Startup-to-Startup Eşleşme Sistemi

Girişimlerin birbirleriyle stratejik ortaklık kurmasını sağlayan yeni bir eşleşme katmanı eklendi.

- **`StartupMatchResult` Tablosu**: Yatırımcı eşleşmelerinden bağımsız olarak, girişimlerin birbirleriyle olan uyum skorlarını tutan yeni bir veritabanı tablosu oluşturuldu.
- **B2B Sinerji Algoritması**: `RuleBasedMatchingService` içerisine eklenen `MatchStartupsAsync` metodu ile girişimlerin sektörleri, teknoloji etiketleri ve büyüme aşamaları arasındaki sinerji hesaplanıyor.
- **Vektör Optimizasyonu**: Python servisi tarafında girişimlerin metinlerini her seferinde yeniden vektörlemek yerine, `pgvector` üzerindeki mevcut vektörlerini kullanarak saniyeler içinde binlerce girişim arasında çapraz eşleşme yapabilen `/api/v1/semantic-match/startup-startup` ucu açıldı.
- **B2B LLM Prompt**: Gemini reranker modülüne B2B partnerliklerini değerlendirmek üzere özelleşmiş bir uzman komutu (prompt) eklendi.
- **Otomatik Networking İşleyicisi**: `EventMatchingWorker` arka plan servisi güncellendi. Artık bir etkinliğin tipi "Networking" olarak belirlenmişse, sistem tüm katılımcı girişimleri birbirleriyle çapraz eşleştirerek B2B potansiyellerini otomatik olarak hesaplıyor.
- **LLM Reranker Refactor (DRY)**: Python tarafındaki LLM mantığı merkezileştirildi. `rerank_by_ids` metodu ile veritabanından metin çekme ve formatlama işlemleri servis katmanına taşınarak kod tekrarı önlendi ve `main.py` sadeleştirildi.
