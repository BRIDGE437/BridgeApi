# Changelog

## [Unreleased] — 2026-03-30

### Yeni Özellikler

#### Startup-to-Startup Benzerlik Motoru
- `backend/MatchingApi/Services/StartupSimilarityService.cs` oluşturuldu.
  - `GET /api/v1/startups/{id}/similar?topN=10` endpointi üzerinden çalışır.
  - Hibrit puanlama: pgvector cosine (34 puan) + SectorSimilarity (30) + RegionMapper (13) + BusinessModel (8) + LLM bonus (max +15).
  - Embedding varsa pgvector HNSW indeksi ile en yakın 200 aday önceden filtrelenir; cosine C# içinde hesaplanır.
  - LLM reranker `mode="startup_similarity"` ile çağrılır — farklı prompt şablonu kullanılır.
- `backend/MatchingApi/DTOs/AllDtos.cs` — `StartupSimilarityResultDto` ve `StartupSimilarityResponseDto` eklendi.
- `backend/MatchingApi/Controllers/StartupController.cs` — `/similar` endpoint eklendi, `StartupSimilarityService` enjekte edildi.
- `backend/MatchingApi/Program.cs` — `StartupSimilarityService` DI'ya kaydedildi.

#### HNSW İndeksi
- `backend/MatchingApi/Migrations/20260318100048_AddHnswIndex.cs` oluşturuldu.
  - `Startups.Embedding` kolonuna cosine distance için HNSW indeksi eklendi (`m=16`, `ef_construction=64`).
  - Benzerlik sorgularında `ORDER BY "Embedding" <=> $vector LIMIT N` sorgularını hızlandırır.

#### LLM Cache Stale Temizleme
- `ai-service/matching/reranker.py` — `_store_cache()` genişletildi:
  - **Per-row cleanup**: Startup metni değiştiğinde (`StartupTextHash` farklıysa) ilgili satır silinir.
  - **Periyodik TTL cleanup**: Her çağrıda %1 ihtimalle 90 günden eski tüm cache kayıtları temizlenir (investor metni değişikliklerine karşı).

#### LLM Reranker Mod Desteği
- `ai-service/matching/reranker.py` — `rerank()` ve `_build_prompt()` metotlarına `mode` parametresi eklendi.
  - `"investor_startup"` (varsayılan): investor-startup eşleştirme promptu.
  - `"startup_similarity"`: startup benzerliği promptu — sektör, teknoloji, hedef pazar benzerliğini değerlendirir.
- `ai-service/main.py` — `SemanticMatchRequest`'e `mode: str = "investor_startup"` alanı eklendi; reranker'a iletilir.

---

### İyileştirmeler

#### Kod Tekrarı Giderildi
- `ai-service/matching/utils.py` oluşturuldu: `md5()` yardımcı fonksiyonu buraya taşındı.
  - `vector_store.py` ve `reranker.py` içindeki duplicate implementasyonlar kaldırıldı.
- `backend/MatchingApi/Helpers/ModelHelpers.cs` oluşturuldu: `ParseCsv()` yardımcı metodu buraya taşındı.
  - `Startup.cs` ve `Investor.cs` içindeki duplicate private `ParseCsv` metotları kaldırıldı.

#### Veritabanı Yazma Optimizasyonu (Upsert)
- `RuleBasedMatchingService.PersistResultsAsync`: Her çağrıda yeni kayıt eklemek yerine mevcut `MatchResult` satırları güncellenir; tablo şişmesi önlenir.
- `AiMatchingService.PersistResultsAsync`: Aynı upsert deseni uygulandı.

#### Döngü İçi Gereksiz Hesaplama Kaldırıldı
- `RuleBasedMatchingService.MatchAsync`: `investor.ParsedRegions`, `ParsedCities`, `ParsedSectors`, `ParsedBusinessModels` ayrıştırması `foreach` döngüsünün dışına alındı. Her startup için tekrarlanan ayrıştırma ortadan kalktı.
- `StartupSimilarityService.ScoreCandidate`: Hedef startup'ın parsed alanları döngü başında bir kez hesaplanıp parametre olarak iletilir.

#### Semantic Skorlama Düzeltmesi
- `AiMatchingService.GetSemanticScoresAsync`: Semantik sonuçlar artık pozisyonel index yerine `startup_id` bazlı dictionary ile eşleştirilir. Sıralama farklılıklarından kaynaklanan yanlış skor atamalarına karşı güvenli.

#### Gereksiz Değişken Kaldırıldı
- `AiMatchingService`: `aiBaseUrl` yerel değişkeni kaldırıldı; `HttpClient.BaseAddress` ile yapılandırılmış göreli yol (`/api/v1/semantic-match`) doğrudan kullanılır.

#### Batch Veritabanı Yazma
- `ai-service/matching/vector_store.py` — `upsert_embeddings()`: N ayrı `execute()` çağrısı yerine tek `executemany()` ile batch upsert. Her startup embedding güncellemesi için ayrı DB round-trip ortadan kalktı.
