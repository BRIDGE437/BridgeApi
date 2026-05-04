# Bridge API - Gelişmiş Profil ve Eşleştirme Entegrasyonu Planı

Bu plan, sosyal platform (`BridgeApi.API`) ile eşleştirme motorunu (`MatchingApi` & `AI Service`) teknik olarak birleştirmeyi hedefler. Temel amaç, sosyal profil verilerini eşleştirme algoritmasının kullanabileceği teknik derinliğe ulaştırmaktır.

## User Review Required

> [!IMPORTANT]
> **Veritabanı Birleşimi**: Hem ana backend hem de MatchingApi aynı Neon DB'yi kullanacaktır. Bu süreçte MatchingApi'nin eski yerel tabloları (`matching_db`) devredışı kalacak, tüm veriler ana veritabanı şemasına taşınacaktır.

> [!WARNING]
> **Profil Güncelleme Akışı**: Bir kullanıcı profilini güncellendiğinde, AI servisinin (Python) yeni bir vektör üretmesi gerekecektir. Bu süreci performansı düşürmemek için asenkron (Background Job) olarak yöneteceğiz.

## Team Collaboration & Migration Strategy

> [!IMPORTANT]
> **Shared Library (Ortak Kütüphane)**: Kod tekrarını önlemek ve ekip senkronizasyonunu sağlamak için **BridgeApi.Shared** adında yeni bir proje oluşturulacaktır.
>
> - `InvestorProfile`, `StartupProfile` ve ortak DTO'lar bu kütüphanede duracaktır.
> - Hem **BridgeApi.API** hem de **MatchingApi** bu kütüphaneyi referans alacaktır.

> [!IMPORTANT]
> **Migration Sahipliği**: 3 kişilik ekip yapısında karışıklığı önlemek için tüm yeni tablo (InvestorProfile, StartupProfile) migration'ları **BridgeApi.API** projesi üzerinden üretilecektir.
>
> - Diğer ekip üyeleri kendi local ortamlarında sadece `Update-Database` yaparak şemayı güncelleyebilecektir.
> - **MatchingApi**, bu tabloları veritabanında "hazır" bulacak ve sadece okuma/yazma (Consumer) rolü üstlenecektir.

## Proposed Changes

### 1. Domain Katmanı (Core) - [X] (TAMAMLANDI)

Eşleştirme motorunun ihtiyaç duyduğu tüm teknik alanları içeren yeni modeller eklenecektir.

#### [NEW] InvestorProfile.cs

- `AppUser`'a 1-to-1 bağlı olacak.
- `PreferredSectors`, `TicketSizeMin/Max`, `PreferredBusinessModel`, `InvestmentStage`, `PreferredRegions` alanlarını içerecek.
- **[YENİ]** `CompanyName` (Kurumsal yatırımcılar/fonlar için)
- **[YENİ]** `Embedding` (Pgvector Vector tipinde - AI eşleşmesi için)
- **[YENİ]** `EmbeddingHash` (MD5 formatında bayatlama kontrolü için)

#### [NEW] StartupProfile.cs

- `AppUser`'a 1-to-1 bağlı olacak.
- `Stage`, `RevenueModel`, `RevenueState`, `TotalFunding`, `Tags` (Sektörler) alanlarını içerecek.
- **[YENİ]** `CompanyName` (Girişimin Adı)
- **[YENİ]** `HQ` (Girişimin Merkez Lokasyonu)
- **[YENİ]** `Embedding` (Pgvector Vector tipinde - AI eşleşmesi için)
- **[YENİ]** `EmbeddingHash` (MD5 formatında bayatlama kontrolü için)

#### [MODIFY] AppUser.cs

- `InvestorProfile` ve `StartupProfile` için navigation property'ler eklenecek.

---

### 2. Persistence Katmanı (Infrastructure) - [X] (TAMAMLANDI)

Veritabanı tablolarının oluşturulması ve yapılandırılması.

#### [MODIFY] ApplicationDbContext.cs

- Yeni `DbSet`'ler eklenecek.
- Fluent API ile 1-to-1 ilişkiler (OnModelCreating) tanımlanacak.

---

### 3. Application Katmanı (Business Logic) - [X] (TAMAMLANDI)

Profillerin yönetimi için gerekli servisler ve DTO'lar.

#### [NEW] MatchProfileDtos.cs

- Create/Update işlemleri için gerekli DTO'lar tanımlanacak.

---

### 4. MatchingApi Entegrasyonu - [X] (TAMAMLANDI)

Eşleştirme motorunun (MatchingApi) izole veriler yerine ana backend tablolarından beslenmesi sağlanacak.

- `MatchingApi` içindeki veritabanı sorguları, yeni şemaya (`BridgeApi.API` şeması) göre revize edilecek.

---

### 5. Automated Data Ingestion (CSV Import) - [X] (TAMAMLANDI)

Büyük veri setlerini (CSV) sisteme otomatik olarak aktaran ve kullanıcıları yaratan modül.

- `CsvImportService` (BridgeApi.Application) implemente edildi.
- `ImportController` (BridgeApi.API) üzerinden dosya yükleme desteği eklendi.
- CSV'deki her satır için otomatik Identity hesabı ve teknik profil oluşturma mantığı kuruldu.
- Python AI Servisi, yeni `StartupProfiles` şeması ve `string` ID yapısıyla senkronize edildi.

---

## Verification Plan

### Automated Tests

- `POST /api/v1/import/startups` endpoint'i ile `concatted_enriched.csv` yükleme testi.
- Veritabanında `AppUsers` ve `StartupProfiles` tablolarının doğru eşleştiğinin kontrolü.
- `MatchingApi` üzerinden yeni yüklenen verilerle AI eşleşme doğrulaması.

### Manual Verification

- Bir kullanıcı Founder rolüyle kaydolduğunda, sistemin ondan "Girişim Detaylarını" isteyip istemediği kontrol edilecek.
- Eşleşme motorunun yeni eklenen `InvestorProfile` verileriyle doğru skor üretip üretmediği doğrulanacak.
