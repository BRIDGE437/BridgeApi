# BridgeApi Dokümantasyon Cilt 4: Yapay Zeka ve Matematiksel Motor
*(AI Engine, Vector Store & Prompt Engineering Raporu TR/EN)*

Bu doküman, sistemin düşünme yeteneğini sağlayan Python (FastAPI) mikroservisinin kalbine inmektedir. Vektörizasyon (Embedding) süreçleri, `pgvector` üzerindeki SQL-matematik optimizasyonları ve Google Gemini'ye yollanan "Prompt" mühendislikleri satır satır incelenmiştir.

---

## 1. Vektörizasyon ve Boyutsal Matrisler (Embedding Engine)
Girişimlerin ve yatırımcıların kelimelerden arındırılıp matematiğe döküldüğü katmandır.

### 1.1. SentenceTransformers (`all-MiniLM-L6-v2`)
- **Neden Seçildi?** Çok hafif ve inanılmaz hızlı (Milisaniyeler içinde çalışır) olduğu için. Açıklamaları (Description), Etiketleri (Tags) ve İş Modellerini birleştirerek tek bir metin paragrafı oluşturur ve bunu **384-boyutlu** (`[0.12, -0.45, 0.99, ...]`) bir matematiksel uzay vektörüne dönüştürür.
- **Performans Darboğazı Çözümü:** 1726 girişimin aynı anda modeli kilitlememesi için, Python servisindeki `/api/v1/index-startups` ucuna istekler 100'erlik (Batch) paketler halinde yollanır ve RAM şişmesi engellenir.

---

## 2. PostgreSQL `pgvector` Optimizasyonu (Matematiksel Sıçrama)
Sistemin eski versiyonunda Python (NumPy) üzerinden RAM kullanılarak yapılan hantal hesaplama, doğrudan Veritabanı SQL seviyesine çekilmiştir.

### 2.1. Kosinüs Benzerliği (Cosine Similarity) `<=>` Operatörü
- İki girişim (veya Yatırımcı ile Girişim) arasındaki 384 boyutlu iki vektör, SQL'in `pgvector` eklentisi kullanılarak **Kosinüs Uzaklığı (`<=>`)** formülüyle kıyaslanır.
- **SQL Sorgusunun Anatomisi (`get_similarities_sql`):**
  ```sql
  SELECT id, 1 - (embedding <=> $1::vector) AS similarity 
  FROM startup_embeddings 
  WHERE id = ANY($2);
  ```
- **Neden 1'den Çıkardık?** `pgvector` operatörü olan `<=>`, "Uzaklık" (Distance) döndürür. (0: Aynı, 2: Tam Zıt). Bizim "Benzerlik" (Similarity) skoruna ihtiyacımız olduğu için uzaklığı 1'den çıkararak, 1.00 (Mükemmel eşleşme) ile 0.00 (Alakasız) arasında bir "Yüzde" elde ettik.

---

## 3. Gemini LLM (Large Language Model) Prompt Engineering
Yapay Zeka botunun (Google Gemini), gelen eşleşmeleri sadece "Kelime benzerliği" olarak değil, "Mantıksal Sinerji" olarak yorumladığı asıl Reranker (Yeniden Sıralama) aşamasıdır.

### 3.1. Yatırımcı -> Girişim Promptu
Yatırımcının tezi ve sistemin kısa listeye (Top 20) aldığı girişimler Gemini'a gönderilir. İstek yapılan Prompt'un felsefesi şöyledir:
- **Görev:** Sen bir Uzman Yatırım (VC) Analistisin. Bu yatırımcının hedeflerine bak ve aşağıdaki girişimleri 0 ile 10 arasında puanla.
- **Zorunluluklar:** Döneceğin cevap KESİNLİKLE geçerli bir JSON olmalıdır. Her startup ID'si için mantıksal bir `reason` (sebep) yazmalısın.

### 3.2. Girişim -> Girişim (B2B Sinerji) Promptu
Sisteme yepyeni eklediğimiz ve iki şirketin birbirleriyle ortaklık kurma potansiyelini ölçen prompt:
- **Görev:** Sen bir B2B Ortaklık Uzmanısın (B2B Partnership Expert). Kaynak şirket (Source Startup) ile diğer potansiyel şirketler (Target Startups) arasında "Müşteri Paylaşımı", "Teknoloji Entegrasyonu" veya "Tedarik Zinciri" açısından bir sinerji var mı?
- **Puanlama Kriteri:** Eğer tamamen rakipseler ve birbirlerini bitireceklerse Puanı düşür. Eğer biri diğerinin eklentisi (addon) olabiliyorsa puanı yükselt.
- **Çıktı Örneği:**
  ```json
  {
    "guid_startup_b": {
      "score": 8.5,
      "reason": "Şirket A bir e-ticaret altyapısı sunarken, Şirket B lojistik optimizasyon AI aracı üretiyor. Doğrudan entegrasyon şansları çok yüksek."
    }
  }
  ```

### 3.3. Puan Birleştirme (Hibrit Skor)
Gemini'dan dönen bu 0-10 arası skor (`llm_score`), C# tarafındaki `AiMatchingService`'e JSON olarak iletilir. C# bu değeri alır, kural tabanlı puan ve `pgvector` puanının üstüne "LLM Bonus" (Altın vuruş) olarak ekleyip nihai sonucu yatırımcıya sunar.
