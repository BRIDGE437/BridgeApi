# 🌉 BridgeApi: The AI-Powered Startup & Investor Ecosystem
*(Yapay Zeka Destekli Girişimci ve Yatırımcı Sosyal Ağı)*

## 🚀 Proje Nedir?
BridgeApi, standart bir form/kayıt sitesi değildir. **Startupların, melek yatırımcıların ve fonların bir araya gelip hem LinkedIn tarzı sosyalleşebildiği hem de en son teknoloji yapay zeka (LLM & Vector DB) modelleriyle birbirleriyle saniyeler içinde "Eşleşebildiği" tam teşekküllü bir Profesyonel Sosyal Ağdır.**

Sistem, dışarıdan topladığı binlerce ham girişimi (Şu anda içeride **1726 tekilleştirilmiş girişim** bulunmaktadır) kendi veritabanında temizler, indeksler ve kullanıcıları "Etkinlikler (Events)" çatısı altında buluşturarak doğru yatırımı/doğru ortağı saniyeler içinde karşınıza çıkarır.

---

## 🌟 Ana Özellikler (Core Features)

### 1. 🤝 Profesyonel Sosyal Ağ (The Social Layer)
Sistem sadece yapay zekadan ibaret değildir. İçeride canlı bir ekosistem döner:
- **Kişisel ve Kurumsal Profiller:** Kullanıcılar Avatar, Kapak Fotoğrafı, Bio (UserProfile) oluşturur; aynı zamanda "Yatırımcı" veya "Girişim" şapkalarını giyerler.
- **Takip ve Bağlantı (Follow & Connection):** İnsanlar birbirine arkadaşlık/ağ isteği gönderebilir, ilgilendikleri şirketleri takip edebilir.
- **Zaman Tüneli ve Gönderiler (Feed & Posts):** Platform üzerinde herkes yeni yatırım turu aradığını veya vizyonunu LinkedIn tarzı paylaşımlarla duyurabilir, beğeni/yorum yapabilir.
- **DM ve Anlık Mesajlaşma:** Kullanıcılar eşleştikleri kişilerle anında iletişime geçebilir.

### 2. 🧠 Yapay Zeka Eşleştirme Motoru (AI Matching Engine)
Doğru yatırımcıyla doğru girişimi bulmak için tasarlanmış 3 aşamalı (Hybrid) mükemmeliyetçi bir sistemdir:
- **Kural Tabanlı Ön Filtre (Rule-Based):** Sektör, Büyüme Aşaması ve Ülke/Şehir uyumuna göre adayları hızlıca daraltır.
- **Anlamsal Vektör Analizi (pgvector):** Python AI servisi, cümleleri 384-boyutlu matematiksel dizilere çevirir. PostgreSQL üzerinden Kosinüs uzaklığı ile "Gizli Benzerlikleri" keşfeder.
- **Gemini LLM (Reranker) Bonusu:** Sadece "kelimeler benziyor" diye değil, Google Gemini kullanılarak "Bu iki şirket ortak olmalı ÇÜNKÜ..." mantığıyla son bir Reranking (Yeniden sıralama) uygulanır ve %100 uyuşan adaylar sunulur.

### 3. 🌐 B2B Sinerji Algoritması (Startup-to-Startup)
Sadece Yatırımcı eşleşmesi değil, iki farklı girişimin birbirlerine teknoloji, tedarik zinciri veya müşteri tabanı açısından "Mükemmel Bir Ortak (Partner)" olup olamayacağını ölçen yepyeni bir "Networking" arama motoruna sahiptir.

### 4. 📅 Olay Güdümlü Etkinlik Sistemi (Event-Driven Architecture)
Eşleşmeler rastgele yapılmaz. Sistemde "Demo Day", "Q3 Pitching" veya "Fintech Networking" gibi **Etkinlikler (Events)** oluşturulur.
- Girişimler ve Yatırımcılar bu etkinliklere JSON payload'larıyla kaydolur.
- Arka planda 7/24 uyuyan **Background Workers**, etkinliğin saati geldiğinde uyanır, veritabanını kilitler (Status: Processing) ve o etkinlikteki herkesi arka planda (kullanıcıyı hiç bekletmeden) yüzlerce kez eşleştirir.

### 5. 🛡️ Veri Temizleme ve Tekilleştirme (Deduplication)
- İnternetten kazınan/eklenen binlerce veri, özel bir MD5 algoritması (`Website + CompanyName`) ile parmak izine dönüştürülür. Sisteme aynı şirket 2 kez asla giremez!
- E-posta çakışmalarını önlemek için (Örn: Sentry teknik mailleri) arkaplanda e-postalar `+ID` eklenerek benzersiz hale getirilirken, orijinal veriler hiçbir zaman kaybedilmez.

---

## 🏗️ Kullanılan Teknolojiler (Tech Stack)
BridgeApi, "Separation of Concerns" (Sorumlulukların Ayrılması) prensibiyle iki farklı sunucudan oluşan devasa bir mikroservis yapısıdır:

- **Ana Backend (C# .NET Core 8):** Sosyal ağ işlemleri (Post, Follow, Auth) temiz ve modern **CQRS & MediatR** mimarisiyle işlenirken; eşleşme matematik kuralları **Services** katmanında hesaplanır.
- **Yapay Zeka Sunucusu (Python FastAPI):** SentenceTransformers ve Google Gemini API kullanılarak ağır vektör/yapay zeka işlemleri yapılır.
- **Veritabanı (Neon Serverless PostgreSQL):** `pgvector` eklentisi kullanılarak klasik veri (User, Post) ile matematiksel veri (Vektörler) tek bir bulut veritabanında olağanüstü hızlarda saklanır ve sorgulanır.

---

## 📖 Dokümantasyon Rehberi (Neye Nereden Ulaşacaksınız?)
Sistemin kaputunun altını merak eden geliştiriciler ve mimarlar için hazırladığımız **5 Ciltlik Master Teknik Şartname** `raporlar/` klasörünün içindedir:

📍 **[`raporlar/01_Database_and_Entity_Layer_Analysis.md`](./raporlar/01_Database_and_Entity_Layer_Analysis.md)**
- Sistemde hangi veritabanı tabloları var?
- `AppUser` sosyal ağı nasıl yönetiyor? Tekilleştirme (`ExternalFingerprint`) nasıl çalışıyor? Tüm kolonların açıklamaları burada.

📍 **[`raporlar/02_Core_Business_Logic_and_Workers.md`](./raporlar/02_Core_Business_Logic_and_Workers.md)**
- Kural tabanlı algoritmanın puan ağırlıkları (Weight) nelerdir? (Örn: Lokasyon %35, Sektör %40).
- Arka planda 7/24 çalışan Etkinlik ve İndeksleme İşçileri (Workers) sistemi kilitlemeden nasıl çalışır?

📍 **[`raporlar/03_API_Endpoints_and_Payloads.md`](./raporlar/03_API_Endpoints_and_Payloads.md)**
- Bir Frontend veya Mobil geliştiricisi nereye istek atacak?
- İstisnasız **TÜM API uçlarının** (Sosyal Ağ, AI Motoru, Eşleşme Etkinlikleri) JSON Request ve Response örneklerinin bulunduğu devasa Swagger sözlüğü.

📍 **[`raporlar/04_AI_and_Mathematical_Engine.md`](./raporlar/04_AI_and_Mathematical_Engine.md)**
- Python sunucumuzdaki matematik (Kosinüs Uzaklığı `<=>`) nasıl çalışıyor?
- Google Gemini'ye "Bu iki şirket ortak olmalı" dedirten İngilizce **Prompt Engineering** formüllerimizin tam metinleri nerede?

📍 **[`raporlar/05_Environment_and_Deployment_Architecture.md`](./raporlar/05_Environment_and_Deployment_Architecture.md)**
- Sistem lokalde veya sunucuda nasıl ayağa kalkar?
- `.env` ve `appsettings.json` dosyalarındaki değişkenler, Neon PostgreSQL Connection Pooling (Bağlantı Havuzu) mimarisi ve Rate-Limit (Token Tasarrufu) önlemlerinin detayları.
