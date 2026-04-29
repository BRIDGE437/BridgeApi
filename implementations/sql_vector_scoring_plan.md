# Scaling Vector Matching: SQL-Level Scoring (pgvector)

Transition the similarity calculation from Python's RAM (NumPy) to the database level (PostgreSQL/pgvector). This reduces network traffic and leverages the database's optimized vector operators (`<=>`).

## Proposed Changes

### 1. Vector Store Layer

#### [MODIFY] [vector_store.py](file:///c:/Users/nefise/Desktop/BridgeApi/ai-service/matching/vector_store.py)
Add a new method `get_similarities_sql` that computes similarities directly in PostgreSQL.
- **SQL Operator**: Use `1 - (Embedding <=> %s)`. 
  - `<=>` is the Cosine Distance operator in pgvector.
  - `1 - Distance` gives us the **Cosine Similarity** (0.0 to 1.0).
- **Filtering**: Still uses the `WHERE "Id" = ANY(%s)` clause to honor the candidates selected by the .NET Rule Engine.
- **Efficiency**: Only returns `(id, score)` pairs back to Python, instead of full 384-dimension arrays.

---

### 2. API Logic Layer

#### [MODIFY] [main.py](file:///c:/Users/nefise/Desktop/BridgeApi/ai-service/main.py)
Refactor the `semantic_match` endpoint to utilize the SQL-level scoring.
1. Encode the **Investor/Source text** into a single vector (Python).
2. Check if any candidate startups are missing embeddings (Stale check logic remains).
3. Call `vector_store.get_similarities_sql(...)` bypassing the `embedding_engine.cosine_similarities` NumPy call for indexed candidates.
4. Merge results and proceed to the optional LLM Reranking (Gemini).

---

### 3. Cleanup (Optional/Future Proofing)

#### [MODIFY] [embeddings.py](file:///c:/Users/nefise/Desktop/BridgeApi/ai-service/matching/embeddings.py)
- Keep `cosine_similarities` as a fallback or for local testing, but add a comment noting that production matching should happen in SQL.

## Benefits of This Approach
- **Zero Vector Transfer**: For a match with 1000 candidates, we avoid transferring 1.5MB of raw float data over the network.
- **Neon Optimization**: Neon's infrastructure can handle the vector math near the data storage, reducing CPU cycles on the FastAPI app server.
- **Future Search**: This sets the stage for "Global Search" (finding matches without a pre-filter) using `pgvector` indexes like `HNSW`.

## Verification Plan

### Manual Verification
1. Compare results between the old NumPy method and the new SQL method to ensure the scores are identical (or within a 0.001 tolerance).
2. Monitor memory usage in the FastAPI container during a batch match of 100+ startups.
3. Verify that the `pgvector` extension is correctly utilized in the PostgreSQL logs.
