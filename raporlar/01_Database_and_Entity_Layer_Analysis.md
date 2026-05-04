# BridgeApi Dokümantasyon Cilt 1: Database & Entity Layer Deep Dive
*(Veritabanı ve EF Core Şema Analizi Raporu TR/EN - Tam Sürüm)*

Bu doküman, sistemin en alt katmanı olan Veritabanı Entity'lerinin (C# Class'larının) yapılarını, içerdiği **tüm alanları (fields) eksiksiz olarak**, neden bu şekilde tasarlandıklarını ve taşıdıkları iş mantıklarını açıklamaktadır.

---

## 1. `AppUser` (Identity ve Sosyal Kimlik Entitesi)
Tüm kullanıcıların (Girişimci, Yatırımcı vb.) sisteme giriş yaptığı ASP.NET Identity Core sınıfıdır.

### Tüm Özellikler (Properties):
- `string Id [Key]`: IdentityUser'dan miras alınan ana kimlik UUID'si.
  - **⚡ Biz Ne Değiştirdik?** Eskiden ID'ler `int` formatındaydı. Identity altyapısıyla sorunsuz çalışması ve alt profillere (Startup/Investor) 1-1 bağlanabilmesi için `string` (GUID) yapısına geçirdik.
- `string? RefreshToken`: JWT (JSON Web Token) yenileme token'ını tutar. Oturumun açık kalmasını sağlar.
- `DateTime? RefreshTokenExpiryTime`: Refresh token'ın son kullanım tarihi.
- `string? AuthProvider`: OAuth sağlayıcısı (Google, LinkedIn vb.).
- `string? ProviderKey`: Dış kimlik sağlayıcının (SSO) verdiği eşsiz anahtar.
- `DateTime CreatedAt`: Hesabın oluşturulma tarihi (Default: `UtcNow`).
- `DateTime? UpdatedAt`: Hesap bilgilerinin son güncellenme tarihi.

**Profesyonel ve Sosyal Bağlantılar (Navigation Properties):**
- `InvestorProfile? InvestorProfile`: (1-1 İlişki) Kullanıcının varsa Yatırımcı profili.
- `StartupProfile? StartupProfile`: (1-1 İlişki) Kullanıcının varsa Girişim profili.
- `UserProfile? UserProfile`: Standart sosyal ağ profil bilgileri.
- `ICollection<UserIntent> UserIntents`: Kullanıcının sisteme giriş amaçları (örn. "Yatırım Arıyorum").
- `ICollection<Post> Posts`: Kullanıcının paylaştığı LinkedIn tarzı gönderiler.
- `ICollection<Connection> SentConnections & ReceivedConnections`: Bağlantı (Connection) ağistekleri.
- `ICollection<Message> Messages`: Mesajlaşma sistemi.
- `ICollection<Follow> Followers & Following`: Takipçi altyapısı.

---

## 2. `StartupProfile` (Girişim Profili Entitesi)
Girişimlere ait tüm teknik, finansal ve yapay zeka verilerini taşıyan ana tablodur.

### Tüm Özellikler (Properties):
- `string UserId [Key]`: `AppUser` tablosundaki `Id` ile eşleşen Foreign Key.
  - **⚡ Biz Ne Değiştirdik?** Kendi başına auto-increment `int` olan ID'leri iptal ettik ve doğrudan `AppUser` tablosuna bağlanan `string` (GUID) tabanlı kimlik modeline geçtik.
- `string Stage` [Max:100]: Girişimin bulunduğu aşama (Pre-seed, Seed, Series A vb.). Kural tabanlı eşleşmede (Rule-Based) kullanılır.
- `string Tags` [Max:1000]: Girişimin kullandığı teknolojiler veya etiketler (Örn: "AI, Fintech"). B2B sinerjisinde kritik rol oynar.
- `string BusinessModel` [Max:100]: B2B, B2C, B2B2C gibi iş modeli verisi.
- `string? RevenueModel` [Max:100]: Abonelik, Komisyon, Tek Seferlik gibi gelir modeli.
- `string? RevenueState` [Max:100]: "Pre-revenue", "Generating Revenue" gibi gelir durumu.
- `long TotalFunding`: Bugüne kadar alınan toplam yatırım miktarı (Dolar bazında).
- `string? Description` [Max:5000]: Girişimin detaylı açıklaması. Vektörizasyon (AI) için beslenen ana metindir.
- `string? WebsiteDescription` [Max:5000]: Scraper'ın web sitesinden çektiği ekstra tanıtım verisi.
- `string? WebsiteUrl` [Max:500]: Şirketin resmi web adresi. Tekilleştirmede kullanılır.
- `string? CompanyName` [Max:200]: Şirket adı. Tekilleştirmede kullanılır.
- `string? HQ` [Max:200]: Şirket merkezi (Ülke/Şehir).
- `Pgvector.Vector? Embedding` [Type: "vector(384)"]: `Description` metninin SentenceTransformers tarafından dönüştürülmüş 384-boyutlu matematiksel hali.
  - **⚡ Neden Ekledik?** Numpy/Python ile yapılan bellek (RAM) tüketen vektör analizlerini çöpe atıp, hesaplamaları doğrudan veritabanı (SQL) düzeyine indirerek ağ trafiğini engellemek için.
- `string? EmbeddingHash` [Max:46]: `Description` değiştiğinde vektörün bayatladığını anlamak için tutulan MD5 özeti.
- `string? ContactEmails` [Max:2000]: Identity çakışmalarını çözmek için eklenen ve girişimin gerçek maillerini tutan ham veri kolonu.
  - **⚡ Neden Ekledik?** Veritabanına aktarım sırasında sahte (`+ID`) e-postalar üretmek zorunda kaldığımız durumlarda, girişimin orijinal ve gerçek e-postalarının hiçbir veri kaybı olmadan saklanması için.
- `bool NeedsManualReview`: Teknik e-postalar (Wix/Sentry) yüzünden sahte mail üretilmişse yöneticinin incelemesi için `true` olur.
  - **⚡ Neden Ekledik?** Aktarım sırasında oluşan çakışmaları sonradan kolayca bulup, admin panelinden topluca düzeltebilmek (Filtreleme) için.
- `string? ExternalFingerprint` [Max:64]: `MD5(WebsiteUrl + CompanyName)` formülüyle üretilmiş tekilleştirme parmak izi.
  - **⚡ Neden Ekledik?** 3 farklı CSV birleştirilirken oluşan aynı (mükerrer) kayıtların veritabanını çökertmesini veya ezmesini (Overwrite) engellemek; girişimin parmak izinden tanınmasını sağlamak için.

---

## 3. `InvestorProfile` (Yatırımcı Profili Entitesi)
Yatırımcıların bütçelerini ve tezlerini (hedeflerini) saklayan tablo.

### Tüm Özellikler (Properties):
- `string UserId [Key]`: `AppUser` ile eşleşen Foreign Key.
- `string Type` [Max:50]: Yatırımcı tipi ("angel", "vc", "syndicate").
- `string PreferredSectors` [Max:1000]: Tercih edilen sektörler (Rule-based için).
- `string PreferredBusinessModel` [Max:100]: Yatırım yapmak istenen iş modeli (B2B, B2C).
- `string PreferredRegions` [Max:500]: Yatırım yapılacak coğrafya (HQ) tercihleri.
- `string InvestmentStage` [Max:200]: Tercih edilen büyüme aşaması (Pre-seed, Seed).
- `long TicketSizeMin` & `long TicketSizeMax`: Yatırımcının masaya koyabileceği min-max yatırım bütçesi aralığı.
- `string? PreferredRevenueState` [Max:200]: Gelir bekliyor mu, yoksa pre-revenue uygun mu?
- `string? Portfolio` [Max:2000]: Önceden yatırım yaptığı şirketler.
- `string? Description` [Max:2000]: Yatırım şirketinin/meleğin tanıtım metni.
- `string? CompanyName` [Max:200]: Kurumsal yatırımcılar için şirket adı.
- `string Thesis` (Eğer Description kullanılıyorsa Tez olarak da geçer): Yatırımcının ne aradığını serbest metin olarak girdiği alan. LLM Prompt'larını besleyen ana kaynaktır.
- `Pgvector.Vector? Embedding` & `string? EmbeddingHash`: AI motorunun, yatırımcının tezini vektörize ettiği kolonlar.

---

## 4. `MatchEvent` (Eşleşme Etkinliği Entitesi)
Tüm eşleşmelerin rastgele değil, planlı ve zamanlı olmasını sağlayan Event-Driven altyapısının merkezidir.

### Tüm Özellikler (Properties):
- `int Id [Key]`: Etkinliğin otomatik artan benzersiz kimliği.
- `string Title` [Max:200]: Etkinliğin adı (Örn: "Q3 Fintech Demo Day").
- `DateTime ScheduledAt`: Etkinliğin ve arka plandaki eşleştirme işçilerinin (Workers) ne zaman devreye gireceğinin kesin tarihi.
- `string Status` [Max:50]: (Default: "Upcoming"). İşçi devreye girdiğinde "Processing", bittiğinde "Completed" olur. Asenkron işlemlerde kilit mekanizması sağlar.
- `int TopMatchingCount`: Eşleşme sonucunda bir yatırımcıya kaç tane (Örn: En iyi 5) girişim gösterileceğinin sınırı.
- `string? EventType` [Max:50]: "Pitching", "Networking" gibi etkinlik tipleri.
- `string? FilterValue` [Max:100]: Özel filtrelemeler için.
- `ICollection<EventParticipation> Participations`: Etkinliğe kayıt olan kullanıcıların (Girişim/Yatırımcı) listesi.
- `ICollection<MatchResult> MatchResults`: Bu etkinlikte doğan eşleşmelerin arşiv listesi.

---

## 5. `MatchResult` (Yatırımcı-Girişim Puan Tablosu)
Yatırımcı ile Girişim arasındaki eşleşme kırılımlarını tutan devasa puanlama tablosudur.

### Tüm Özellikler (Properties):
- `long Id [Key]`: Kayıt numarası.
- `string InvestorId` & `string StartupId`: Eşleşen tarafların Foreign Key'leri.
- `string MatchingMode` [Max:20]: Bu eşleşme kural tabanlı mı ("rule-based") yoksa yapay zeka ile mi ("ai-powered") yapıldı?
- `double TotalScore`: Tüm alt puanların belirli ağırlıklarla (Weight) toplanmış hali. (Nihai skor).
- `double SectorScore`: Kural tabanlı sektör uyuşmazlığı puanı.
- `double GeoScore`: Kural tabanlı lokasyon uyumu puanı.
- `double ModelScore`: İş modeli ve gelir durumu puanı.
- `double StageScore`: Yatırım aşaması (Pre-seed vs Seed) uyumu.
- `double FundingBonus`: Beklenen ve verilebilecek bütçenin uyuşma puanı.
- `double SemanticScore`: AI Vektör Motoru (`pgvector`) tarafından döndürülen Kosinüs benzerliği puanı.
- `double LlmBonus`: Gemini Yapay Zekasının bu eşleşmeye verdiği son analiz puanı.
- `string? AiReason` [Max:2000]: Gemini tarafından üretilen "Bu yatırımcı sana uygun ÇÜNKÜ..." metni.
- `DateTime CreatedAt`: Eşleşmenin ne zaman hesaplandığı.
- `int? EventId`: Bu eşleşmenin hangi etkinlik çatısı altında doğduğu (EventDriven bağlantısı).

---

## 6. `StartupMatchResult` (B2B Girişim-Girişim Puan Tablosu)
Girişimlerin birbirleriyle stratejik ortaklık veya partnerlik (Networking) kurması için hesaplanan özel tablodur.

### Tüm Özellikler (Properties):
- `long Id [Key]`: Kayıt numarası.
- `int? EventId`: Eşleşmenin gerçekleştiği etkinlik.
- `string SourceStartupId` & `string TargetStartupId`: Birbiriyle kıyaslanan iki farklı girişimin (Girişim A ve Girişim B) Foreign Key'leri.
- `double TotalScore`: Nihai Sinerji Skoru.
- `double SectorScore`: Aynı veya birbirini besleyen sektörlerde olmanın sinerji puanı.
- `double GeoScore`: Aynı ülkede veya şehirde olmanın fiziksel avantaj puanı.
- `double StageScore`: Büyüme aşamalarının benzerliği (Örn: İki Series A şirketinin birleşmesi).
- `double SemanticScore`: Girişimlerin açıklamalarının `pgvector` üzerindeki Kosinüs uyumu.
- `double LlmBonus`: Gemini Reranker'ın B2B Sinerji Prompt'u ile verdiği ortaklık potansiyeli bonusu.
- `string? AiReason` [Max:2000]: LLM'in ürettiği "Siz ikiniz ortak ürün çıkarabilirsiniz ÇÜNKÜ..." açıklaması.
- `DateTime CreatedAt`: Hesaplama tarihi.

---

## 7. Ana Backend: Sosyal Ağ Entiteleri (Social Network Entities)
Projenin sadece bir eşleştirme botu değil, LinkedIn benzeri tam teşekküllü bir platform olmasını sağlayan veritabanı tablolarıdır.

### Kritik Tablolar ve Özellikleri:
- **`UserProfile`:** `AppUser` tablosuna bağlanan, kullanıcının avatar URL'si, Kapak Fotoğrafı, Bio'su ve Headline (Örn: "Founder at X") bilgilerini tutar.
- **`Post` & `PostComment` & `PostLike`:** 
  - `Content` [Max:2000] ve `MediaUrl` kolonlarıyla kullanıcıların zaman tüneline gönderi atmasını sağlar.
  - `Comments` ve `Likes` navigasyonlarıyla etkileşimler yönetilir.
- **`Connection` (Ağ Bağlantısı):**
  - İki kullanıcı arasındaki arkadaşlık isteğini yönetir.
  - `SenderId`, `ReceiverId` ve `Status` (Pending, Accepted, Rejected) kolonlarına sahiptir.
- **`Follow` (Takip Sistemi):**
  - Tek taraflı takip sistemidir (Twitter stili). `FollowerId` ve `FollowingId` kolonları üzerinden "Kim kimi takip ediyor" grafiğini çizer.
- **`Message` (Mesajlaşma):**
  - `SenderId`, `ReceiverId`, `Content` ve `IsRead` (Okundu mu?) kolonlarıyla anlık iletişim geçmişini veritabanında saklar.
