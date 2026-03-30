"""
AI Microservice for Startup-Investor Matching
──────────────────────────────────────────────
Handles embedding computation, semantic similarity, and optional LLM reranking.
This runs alongside the .NET backend as a separate service.

Embedding cache: PostgreSQL (pgvector) — replaces ChromaDB.
LLM result cache: PostgreSQL LlmScoreCache table.
"""

import logging
import os
import time
from contextlib import asynccontextmanager

from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from psycopg_pool import AsyncConnectionPool
from pydantic import BaseModel

from matching.embeddings import EmbeddingEngine
from matching.reranker import LlmReranker
from matching.vector_store import VectorStore

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

# ── Global instances ──
embedding_engine: EmbeddingEngine = None  # type: ignore
llm_reranker: LlmReranker = None  # type: ignore
vector_store: VectorStore = None  # type: ignore
db_pool: AsyncConnectionPool = None  # type: ignore


@asynccontextmanager
async def lifespan(app: FastAPI):
    """Initialize models and DB connection pool on startup."""
    global embedding_engine, llm_reranker, vector_store, db_pool

    logger.info("Loading embedding model...")
    embedding_engine = EmbeddingEngine()

    logger.info("Connecting to PostgreSQL (pgvector)...")
    db_url = os.getenv(
        "PGVECTOR_DATABASE_URL",
        "postgresql://postgres:postgres@localhost:5432/matching_db",
    )
    db_pool = AsyncConnectionPool(db_url, open=False)
    await db_pool.open()

    vector_store = VectorStore(
        pool=db_pool,
        embedding_model_name=embedding_engine.model_name,
        dimensions=embedding_engine.dimensions,
    )

    llm_reranker = LlmReranker(pool=db_pool)
    logger.info("AI Service ready!")
    yield

    logger.info("Shutting down AI Service...")
    await db_pool.close()


app = FastAPI(
    title="AI Matching Microservice",
    version="1.0.0",
    lifespan=lifespan,
)

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_methods=["*"],
    allow_headers=["*"],
)


# ══════════════════════════════════════
# REQUEST / RESPONSE MODELS
# ══════════════════════════════════════


class StartupInput(BaseModel):
    id: int
    text: str


class SemanticMatchRequest(BaseModel):
    investor_text: str
    startups: list[StartupInput]
    use_llm: bool = False
    mode: str = "investor_startup"  # "investor_startup" | "startup_similarity"


class SemanticResultItem(BaseModel):
    startup_id: int
    similarity_score: float
    llm_score: float = 0.0
    reason: str | None = None


class SemanticMatchResponse(BaseModel):
    results: list[SemanticResultItem]
    model: str
    processing_time_ms: int


class EmbedRequest(BaseModel):
    texts: list[str]


class EmbedResponse(BaseModel):
    embeddings: list[list[float]]
    model: str
    dimensions: int


class IndexStartupsRequest(BaseModel):
    startups: list[StartupInput]


class IndexStartupsResponse(BaseModel):
    indexed: int
    already_cached: int
    model: str
    processing_time_ms: int


class VectorStoreStatsResponse(BaseModel):
    collection_name: str
    persist_dir: str
    embedding_model: str
    dimensions: int
    stored_embeddings: int
    in_memory_cache_size: int


# ══════════════════════════════════════
# ENDPOINTS
# ══════════════════════════════════════


@app.post("/api/v1/semantic-match", response_model=SemanticMatchResponse)
async def semantic_match(request: SemanticMatchRequest):
    """
    Core endpoint: compute semantic similarity between investor and startups.
    Uses PostgreSQL (pgvector) as persistent embedding cache.
    Optionally applies LLM reranking on the top 20 results (with cache).
    """
    start = time.time()

    if not request.startups:
        raise HTTPException(status_code=400, detail="No startups provided")

    # Step 1: Encode investor text (L1 in-memory cache)
    investor_embedding = embedding_engine.encode(request.investor_text)

    # Step 2: Resolve startup embeddings via PostgreSQL (L2), then encode misses
    startup_ids = [s.id for s in request.startups]
    startup_texts = [s.text for s in request.startups]

    cached_embeddings, missing_ids = await vector_store.get_embeddings(startup_ids, startup_texts)

    if missing_ids:
        missing_id_set = set(missing_ids)
        missing_startups = [s for s in request.startups if s.id in missing_id_set]
        new_embeddings = embedding_engine.encode_batch([s.text for s in missing_startups])
        await vector_store.upsert_embeddings(
            [s.id for s in missing_startups],
            [s.text for s in missing_startups],
            new_embeddings,
        )
        for startup, emb in zip(missing_startups, new_embeddings):
            cached_embeddings[startup.id] = emb

    # Step 3: Restore original order
    startup_embeddings = [cached_embeddings[sid] for sid in startup_ids]

    # Step 4: Compute cosine similarities
    similarities = embedding_engine.cosine_similarities(investor_embedding, startup_embeddings)

    # Step 5: Build results
    results: list[SemanticResultItem] = []
    for i, startup in enumerate(request.startups):
        sim_score = float(similarities[i])
        results.append(
            SemanticResultItem(
                startup_id=startup.id,
                similarity_score=max(0.0, min(1.0, sim_score)),
            )
        )

    # Step 6: Optional LLM reranking (top 20 only, with PostgreSQL cache)
    if request.use_llm:
        try:
            top_20 = sorted(results, key=lambda r: r.similarity_score, reverse=True)[:20]
            top_20_ids = {r.startup_id for r in top_20}

            llm_scores = await llm_reranker.rerank(
                investor_text=request.investor_text,
                startups=[s for s in request.startups if s.id in top_20_ids],
                mode=request.mode,
            )

            for result in results:
                if result.startup_id in llm_scores:
                    result.llm_score = llm_scores[result.startup_id]["score"]
                    result.reason = llm_scores[result.startup_id]["reason"]

        except Exception as e:
            logger.warning(f"LLM reranking failed: {e}")

    elapsed_ms = int((time.time() - start) * 1000)

    return SemanticMatchResponse(
        results=results,
        model=embedding_engine.model_name,
        processing_time_ms=elapsed_ms,
    )


@app.post("/api/v1/index-startups", response_model=IndexStartupsResponse)
async def index_startups(request: IndexStartupsRequest):
    """
    Bulk pre-indexing endpoint.
    Encodes and persists embeddings for all provided startups into PostgreSQL.
    """
    start = time.time()

    if not request.startups:
        raise HTTPException(status_code=400, detail="No startups provided")

    startup_ids = [s.id for s in request.startups]
    startup_texts = [s.text for s in request.startups]

    cached_embeddings, missing_ids = await vector_store.get_embeddings(startup_ids, startup_texts)
    already_cached = len(cached_embeddings)

    if missing_ids:
        missing_id_set = set(missing_ids)
        missing_startups = [s for s in request.startups if s.id in missing_id_set]
        new_embeddings = embedding_engine.encode_batch([s.text for s in missing_startups])
        await vector_store.upsert_embeddings(
            [s.id for s in missing_startups],
            [s.text for s in missing_startups],
            new_embeddings,
        )

    elapsed_ms = int((time.time() - start) * 1000)

    return IndexStartupsResponse(
        indexed=len(missing_ids),
        already_cached=already_cached,
        model=embedding_engine.model_name,
        processing_time_ms=elapsed_ms,
    )


@app.get("/api/v1/vector-store/stats", response_model=VectorStoreStatsResponse)
async def vector_store_stats():
    """Return monitoring metadata for the PostgreSQL vector store."""
    stats = await vector_store.get_stats()
    stats["in_memory_cache_size"] = embedding_engine.cache_size
    return VectorStoreStatsResponse(**stats)


@app.post("/api/v1/embed", response_model=EmbedResponse)
async def embed_texts(request: EmbedRequest):
    """Generate embeddings for given texts."""
    if not request.texts:
        raise HTTPException(status_code=400, detail="No texts provided")

    embeddings = embedding_engine.encode_batch(request.texts)

    return EmbedResponse(
        embeddings=[emb.tolist() for emb in embeddings],
        model=embedding_engine.model_name,
        dimensions=embedding_engine.dimensions,
    )


@app.get("/health")
async def health():
    return {
        "status": "ok",
        "model": embedding_engine.model_name if embedding_engine else "not loaded",
        "vector_store_embeddings": await vector_store.count() if vector_store else 0,
    }
