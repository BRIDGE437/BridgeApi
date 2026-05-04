# BridgeApi Dokümantasyon Cilt 2: Core Business Logic & Workers
*(Çekirdek İş Mantığı, Algoritmalar ve Arka Plan İşçileri Raporu TR/EN - Tam Sürüm)*

Bu doküman, sistemin "Beyin" katmanında yer alan C# servislerini (Services) ve asenkron arka plan işçilerini (Workers) içerir. Sadece yeni yazılanlar değil, **bizim tarafımızdan köklü değişikliklere uğratılan tüm mevcut servisler** (Örn: `int` ID'lerden `string`e geçiş, AI köprüleri) bu listede eksiksiz belgelenmiştir.

---

## 1. `CsvImportService` (Veri Aktarımı ve Temizlik Felsefesi)
Dış kaynaklı, kirli verilerin sisteme zararsız ve tekilleştirilmiş bir şekilde aktarılmasını sağlayan en kritik güvenlik duvarıdır.

### Kritik Değişikliklerimiz ve Algoritmalar:
- **İndeks Bazlı Okuma:** Farklı CSV'lerdeki kolon isimlerindeki (Header) gizli BOM karakterleri yüzünden patlayan sistemi, sütun sırasına (`values[0]`, `values[1]`) göre okuyacak şekilde değiştirdik.
- **`MakeUniqueEmail` (Identity Çakışma Önleyici):** ASP.NET Identity aynı maille 2 hesap açtırmaz. Ancak Scraper, onlarca girişime `sentry.io` maili vermişti. Biz araya girişimin ID'sini sıkıştırarak (`sentry+14502@wixpress.com`) sistemi ezilmekten kurtardık. Girişimin kendi asıl e-postalarını kaybolmasın diye `ContactEmails` kolonuna sakladık.
- **Evrensel Karakter Koruması:** Türkçe 'I' harfi veya farklı alfabeler yüzünden MD5'lerin farklı çıkmasını engellemek için tüm metinlerde `ToLowerInvariant()` standardını zorunlu kıldık.

---

## 2. `RuleBasedMatchingService` (Kural Tabanlı Puanlama Matematiği)
Yapay Zekaya (`pgvector` ve Gemini) gidilmeden önceki "Sert Filtreleme" algoritmalarının merkezidir. Burada yapılan puanlama, veritabanından çekilen adayları (Örn: 200 kişi) kurala göre eler.

### Algoritma Ağırlıkları (Weights):
- **Yatırımcı -> Girişim Puanlaması (Maks: 100)**
  - Yatırımcı İş Modeli (B2B vs B2C) seçmişse: `SectorScore` (40), `GeoScore` (35), `ModelScore` (25).
  - Seçmemişse: `SectorScore` (55), `GeoScore` (45).
- **Girişim -> Girişim (B2B Sinerji) Puanlaması:**
  - `SectorScore` (40), `GeoScore` (30), `StageScore` (30), `ModelScore` (10).

---

## 3. `AiMatchingService` (Yapay Zeka Köprüsü ve Hibrit Skorlama)
C# .NET Backend ile Python (FastAPI) arasındaki ana iletişim ve skor birleştirme köprüsüdür.

### Kritik Değişikliklerimiz ve Algoritmalar:
- **⚡ Biz Ne Değiştirdik?** Eskiden sistemdeki tüm ID'ler (Yatırımcı ve Girişim) `int` tabanlıydı. Biz Identity mimarisine geçerken bu servisin içindeki tüm imzaları `string` (GUID) olarak yeniden yazdık.
- **Hibrit Puan (Hybrid Scoring) Formülü:** Bu servis, RuleBased (Kural) skorunu %60, Semantic (Vektör) skorunu %30, ve LlmBonus (Gemini) skorunu toplayarak (Maksimum 100 olacak şekilde) nihai bir sonuç (`hybridTotal`) üretir.
- **B2B Networking Modu Ekledik:** Eskiden sadece Yatırımcı-Girişim eşleşiyordu. Biz `MatchStartupToStartupsAsync` metodunu yazarak, etkinlikteki iki farklı girişimin Python servisine gönderilip "Siz Ortak Olabilirsiniz" analiziyle eşleşmesini sağladık.

---

## 4. `StartupSimilarityService` (Saf Benzerlik Motoru)
Bir girişimin "Tıpatıp aynısı veya direkt rakibi" olan diğer girişimleri bulmaya yarayan servistir. Yatırımcıların "Buna benzer başka ne var?" sorusunu cevaplar.

### Kritik Değişikliklerimiz ve Ağırlıklar:
- **⚡ Biz Ne Değiştirdik?** Bu serviste de `int` ID'leri `string`e geçirdik. Vektör hesaplamalarını (`CosineSimilarity`) hedef alarak, boş (Embedding = null) olan girişimlerin sistemi çökertmemesi için hata yakalama (Fallback to rule-based) ekledik.
- **Benzerlik Puan Dağılımı:** Toplam 100 üzerinden; `MaxSector` (30), `MaxSemantic` (34), `MaxGeo` (13), `MaxLlm` (15), `MaxModel` (8) şeklinde dağıtılmıştır.

---

## 5. `EventMatchingWorker` (Etkinlik Motoru)
Sistemde HTTP isteklerini kitlemeden 7/24 arka planda dönen ana işçidir.

### Algoritma ve Kilit (Lock) Mekanizması:
- **Zamanlama:** `PeriodicTimer` kullanarak her 1 Dakikada (`TimeSpan.FromMinutes(1)`) bir uyanır.
- **Görev:** Durumu `Status == "Open"` olan ve zamanı gelmiş (`ScheduledAt <= DateTime.UtcNow`) etkinlikleri bulur.
- **⚡ Biz Ne Değiştirdik?** Diğer sunucuların aynı işe el atmasını engellemek için anında durumu `Status = "Processing"` (İşleniyor) yapar.
- Eğer etkinlik bir "Networking" (Ağ Kurma) etkinliğiyse B2B köprüsünü (`MatchStartupToStartupsAsync`), eğer standart etkinlikse Yatırımcı köprüsünü (`MatchEventAsync`) çağırır.

---

## 6. `StartupIndexingWorker` (Vektör Tarayıcısı)
Yeni eklenen veya güncellenen girişimlerin Yapay Zeka tarafından tekrar analiz edilmesini sağlayan arka plan işçisidir.

### Çalışma Mantığı:
- Her 24 saatte bir çalışır.
- Veritabanındaki `EmbeddingHash` değeri boş olan veya dışarıdan tetiklenmiş girişimleri paketler (Batch).
- Sistemin rate-limit yememesi için bunları Python servisinin `/api/v1/index-startups` ucuna fırlatarak 384 boyutlu vektörlerin doldurulmasını sağlar.

---

## 7. Ana Backend İş Mantığı (CQRS & MediatR Mimarisi)
Eşleştirme motoru (Matching API) yoğun matematik barındırdığı için "Services" (Servis) mimarisini kullanırken; Ana Backend'deki (BridgeApi.API) Sosyal Ağ sistemi **CQRS (Command/Query Responsibility Segregation)** mimarisiyle inşa edilmiştir.

### Nasıl Çalışır?
- **Features Klasörü:** `BridgeApi.Application/Features` altında bulunur. Her bir işlem (Gönderi atma, Mesajlaşma, Giriş yapma) tek bir devasa servis içinde (Örn: `UserService`) toplanmaz.
- **Commands (Yazma İşlemleri):** `CreatePostCommand`, `AcceptConnectionCommand` gibi sınıflarla veritabanını değiştiren olaylar izole edilmiştir.
- **Queries (Okuma İşlemleri):** `GetPostsQuery`, `GetUserProfileQuery` ile sadece veri getiren ve performans odaklı çalışan yapılar ayrılmıştır.
- **MediatR:** API Controller'lar sadece isteği alır ve MediatR aracılığıyla ilgili Handler'a (İşleyiciye) iletir. Bu sayede kod inanılmaz derecede bağımsız (decoupled) ve test edilebilir hale gelmiştir.

