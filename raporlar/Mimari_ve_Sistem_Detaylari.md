# Bridge API - Mimari ve Sistem Detayları (Fonksiyonel Analiz)

Bu döküman, Bridge API projesinin arka plandaki çalışma mantığını, algoritma detaylarını ve servislerin birbirleriyle olan derin entegrasyonunu açıklar.

---

## 1. HİBRİT EŞLEŞTİRME ALGORİTMASI (Core Logic)
Sistemin en güçlü yanı, geleneksel veritabanı sorgularıyla modern yapay zekayı birleştiren hibrit yapısıdır. Bir eşleşme isteği (`/ai-powered`) atıldığında şu adımlar izlenir:

### Adım 1: Kural Bazlı Ön Eleme (%60 Ağırlık)
`RuleBasedMatchingService.cs` sınıfı, yatırımcının kriterlerini girişimlerin meta verileriyle (Sector, Region, Stage) karşılaştırır:
- **Sektör Uyumu (30 Puan):** Girişimin etiketleri (Tags) yatırımcının tercihleriyle ne kadar örtüşüyor?
- **Coğrafi Uyum (15 Puan):** HQ lokasyonu yatırımcının hedef bölgesinde mi?
- **Yatırım Aşaması (10 Puan):** Seed, Series A gibi aşamaların tam eşleşmesi.
- **Model Uyumu (5 Puan):** B2B, SaaS gibi iş modeli sinerjisi.

### Adım 2: Semantik Analiz (%30 Ağırlık)
Kural bazlı elemeyi geçen adaylar Python servisine gönderilir. `embeddings.py` ve `vector_store.py` devreye girer:
- **Embedding:** Metinler 384 boyutlu vektörlere dönüştürülür.
- **pgvector:** PostgreSQL üzerinde "Cosine Similarity" (Kosinüs Benzerliği) hesaplanarak, sadece kelime eşleşmesi değil, anlam bütünlüğü ölçülür.

### Adım 3: LLM Reranking & Bonus (%10 Ağırlık)
En yüksek skora sahip ilk 20 aday Google Gemini'ye (`reranker.py`) gönderilir:
- **Stratejik Karar:** LLM, yatırımcının vizyonu ile girişimin çözümünü "okur" ve 0-10 arası bir bonus puan verir.
- **Neden Analizi:** LLM, eşleşmenin neden yapıldığını insan dilinde açıklar (`aiReason`).

---

## 2. B2B NETWORKING MANTIĞI (Startup-to-Startup)
Bu modül, girişimlerin sadece yatırımcı bulmasını değil, birbirleriyle partnerlik kurmasını sağlar:
- **Sinerji Puanlaması:** İki girişimin ürünlerinin birbirini tamamlayıp tamamlamadığına bakar (Örn: Bir ödeme sistemi girişimi ile bir e-ticaret altyapısı girişimi).
- **Vektör Cache:** Python tarafındaki `semantic-match/startup-startup` ucu, daha önce üretilmiş vektörleri kullanarak çok hızlı (ms seviyesinde) çapraz analiz yapar.

---

## 3. ARKA PLAN İŞÇİLERİ (Background Workers)
Sistem, ana işlemleri yavaşlatmamak için bazı görevleri asenkron olarak arka planda yürütür:

- **StartupIndexingWorker:** Yeni eklenen girişimleri tespit eder ve Python servisine "Bunları vektörleştir" komutu gönderir.
- **EventMatchingWorker:** Bir etkinlik (Event) başladığında veya tamamlandığında, katılımcılar arasındaki tüm olası eşleşmeleri toplu halde hesaplar ve veritabanına yazar.

---

## 4. VERİTABANI TASARIMI (Dual-Storage)
- **PostgreSQL (Relational):** İlişkisel veriler, kullanıcılar ve etkinlik kayıtları.
- **pgvector (Vector Store):** Girişimlerin "semantik kimlikleri" vektör olarak burada saklanır. Bu sayede milyonlarca girişim arasından anlam benzerliği olanlar saniyeler içinde bulunabilir.
