# B2B Networking: Startup-to-Startup Matching System

Extend the BridgeApi architecture to support Startup-to-Startup matching. This feature allows startups to find synergetic partners, B2B clients, or complementary businesses during networking events. All code comments will be written in English.

## Database Challenge & Solution
Currently, `MatchResult.cs` enforces a strict relationship: `InvestorId` (String) matched with a `StartupId` (Int). For Startup-to-Startup matching, both sides are `StartupId` (Int). 
**Solution:** We will create a new table `StartupMatchResult` to keep the database normalized and pure, rather than hacking the existing investor table with nulls.

---

## Proposed Changes

### 1. Database & Models (.NET)

#### [NEW] [StartupMatchResult.cs](file:///c:/Users/nefise/Desktop/BridgeApi/backend/MatchingApi/Models/StartupMatchResult.cs)
Create a new persistence entity specifically for B2B matches.
- Fields: `Id`, `EventId`, `SourceStartupId` (Int), `TargetStartupId` (Int), `TotalScore`, `SectorScore`, `GeoScore`, `StageScore`, `SemanticScore`, `LlmBonus`, `AiReason`, `CreatedAt`.

#### [MODIFY] [AppDbContext.cs](file:///c:/Users/nefise/Desktop/BridgeApi/backend/MatchingApi/Data/AppDbContext.cs)
- Add `DbSet<StartupMatchResult>`.
- Configure foreign keys so both `SourceStartupId` and `TargetStartupId` point to the `Startups` table.

---

### 2. Rule Engine & Pre-filtering (.NET)

#### [MODIFY] [RuleBasedMatchingService.cs](file:///c:/Users/nefise/Desktop/BridgeApi/backend/MatchingApi/Services/RuleBasedMatchingService.cs)
- Add `public async Task<List<MatchResultDto>> MatchStartupsAsync(int sourceStartupId, List<Startup> participants, int topN)`
- **Rule Strategy:**
  - *Sector/Tags Synergy (40pts)*: Reuse `SectorSimilarity.CalculateSectorScore` by intelligently passing the **Source Startup's Tags** as if they were "Investor Sectors". This guarantees that highly synergistic sectors (e.g., *E-commerce + Logistics*) automatically get high scores based on our existing Dictionary map!
  - *Geo-Proximity (30pts)*: Check if `HQ` country or region matches.
  - *Stage Alignment (30pts)*: Check if `Stage` is identical or adjacent (e.g., both "Seed").
  - *Business Model Synergy*: Bonus points if they have complementing models (B2B + B2C) or matching ones.

#### [MODIFY] [AiMatchingService.cs](file:///c:/Users/nefise/Desktop/BridgeApi/backend/MatchingApi/Services/AiMatchingService.cs)
- Add `MatchStartupToStartupsAsync(...)` which combines the `RuleBasedMatchingService` results with the Python vector DB.
- Instead of sending a large `source_text`, this routine will call the new Python REST endpoint passing only IDs.

---

### 3. Vector Math & AI (.NET `HttpClient` -> Python)

#### [MODIFY] Python `main.py` (file:///c:/Users/nefise/Desktop/BridgeApi/ai-service/main.py)
- Create a new optimized route: `POST /api/v1/semantic-match/startup-startup`
- **Payload Schema:** 
  ```json
  {
    "source_startup_id": 5,
    "target_startup_ids": [10, 11, 12]
  }
  ```

#### [MODIFY] Python `vector_store.py`
- Expose a fast retrieval method `get_embedding(startup_id)` to pull the Source Startup's 384-dimension vector directly from PostgreSQL `pgvector`, completely bypassing the heavy NLP `sentence-transformers` encoding phase during the matching event.

#### [MODIFY] Python `reranker.py`
- Intercept matches labeled under `mode: "startup_startup"`.
- Inject a specialized prompt to the LLM: *"You are a B2B Networking Expert. Evaluate if Startup A and Startup B can form a strategic partnership or client-vendor relationship..."*
