# BridgeApi Vektörizasyon ve Tetikleyici (Trigger) Mimari Raporu

## 1. Mimari Karar: Neden Eşzamanlı Vektörizasyon Yapılmadı?
BridgeApi sistemine CSV üzerinden 1726 adet girişim verisi import edilirken, verilerin anında (eşzamanlı olarak) vektörel veritabanına (Pinecone/Qdrant) aktarılmama kararı alınmıştır.

**Sebepler:**
1. **Performans Darboğazı:** 1726 büyük metnin aynı anda Embedding modeline (vektörleştiriciye) gönderilmesi, API'nin Rate Limit'e (istek sınırına) takılmasına ve ana sunucunun (BridgeApi.API) kilitlenmesine yol açabilirdi.
2. **Kuvvetler Ayrılığı (Decoupling):** Veritabanına ham veri kaydetme işlemi (Hamal Görevi) ile veriyi anlamsal formata dönüştürme işlemi (Beyin Görevi) birbirinden tamamen izole edilmiştir.

## 2. Tetikleyici (Trigger) Mekanizması Nasıl Çalışır?
Sistemde otomatik bir SQL Trigger yerine, manuel veya zamanlanmış (CRON) olarak çalışabilen bir **API Tetikleyicisi** (`POST /api/v1/Indexing/trigger`) kurulmuştur.

**İşlem Akışı:**
1. **Sorgulama:** Trigger çalıştığında veritabanına bakar: *"Hangi şirketlerin henüz vektörü oluşturulmamış veya verisi güncellenmiş?"*
2. **Batch (Grup) İşleme:** Eksik olan verileri sistemi yormamak için gruplar (Örn: 100'erli batch'ler) halinde alır.
3. **Python AI Servisine İletim:** Bu grupları asenkron olarak Python servisine (`ai-service`) gönderir.
4. **Pinecone/Qdrant Senkronizasyonu:** Üretilen vektörler (Embedding'ler) güvenli bir şekilde vektör veritabanına yazılır.

## 3. Vektörizasyon (Embedding) vs. LLM (Scoring) Farkı
Bu noktada ortaya çıkan en kritik ayrım, Vektörizasyon işlemi ile LLM Akıl Yürütme (Scoring) işleminin birbirinden farklı olmasıdır:

### A. Vektörizasyon Aşaması (Şu Anki Aşama)
- **Görev:** Şirket açıklamalarını, metinleri makine dilindeki matematiksel sayılara (vektörlere) çevirmek. (Tıpkı kütüphanedeki kitaplara barkod basmak gibi).
- **Maliyet ve Hız:** Son derece ucuz, çok hızlı ve basittir. 
- **LLM Kullanımı:** Yoktur. Generative AI (örn: GPT-4) kullanılmaz. Sadece Embedding modeli (örn: OpenAI `text-embedding-3-small` veya lokal `SentenceTransformers`) çalışır.

### B. LLM Scoring / Eşleştirme Aşaması (Gelecek Aşama)
- **Görev:** Vektör uzayında birbirine yakın/benzer bulunan yatırımcı ve girişimi LLM'e (GPT-4) verip, *"Bu yatırımcı neden bu girişime yatırım yapmalı? Yüzde kaç uyumlular?"* sorusuna insansı bir mantıkla cevap almak.
- **Maliyet ve Hız:** Görece yavaştır ve maliyetlidir. (Bu yüzden önceden vektör araması ile sayı 5-10 adaya düşürülür).
- **LLM Kullanımı:** Yoğundur. Prompt mühendisliği gerektirir.

## 4. Sonuç
Kurgulanan bu Trigger mimarisi sayesinde, sistem milyarlarca satır veriyi bile çökmeden, sıraya dizerek ve minimum maliyetle vektörize edebilecek kurşun geçirmez bir altyapıya kavuşmuştur.
