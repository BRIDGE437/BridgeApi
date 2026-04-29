"""
VectorStore
───────────
Persistent embedding cache backed by PostgreSQL + pgvector.
Reads and writes startup embeddings directly to the Startups table's
Embedding / EmbeddingHash columns — replaces ChromaDB.
"""

import logging

import numpy as np
from pgvector.psycopg import register_vector_async
from psycopg_pool import AsyncConnectionPool

from matching.utils import md5 as _md5

logger = logging.getLogger(__name__)


class VectorStore:
    """
    PostgreSQL-backed persistent cache for startup embeddings.

    Uses the Startups table's Embedding (vector(384)) and EmbeddingHash (MD5)
    columns. Stale detection: if the stored hash differs from the incoming
    text's MD5 the record is treated as a cache miss.
    """

    def __init__(
        self,
        pool: AsyncConnectionPool,
        embedding_model_name: str,
        dimensions: int,
    ) -> None:
        self._pool = pool
        self._model_name = embedding_model_name
        self._dimensions = dimensions
        logger.info(
            "VectorStore ready — model='%s', dimensions=%d",
            embedding_model_name,
            dimensions,
        )

    # ── Public API ─────────────────────────────────────────────────────────

    async def get_embeddings_status(
        self,
        startup_ids: list[int],
        texts: list[str],
    ) -> tuple[list[int], list[int]]:
        """
        Audit the existence and freshness of startup embeddings in PostgreSQL.
        OPTIMIZATION: Only fetches 'EmbeddingHash' (MD5), NOT the full 384-dim vector
        to save significant network bandwidth during the initial check.

        Returns:
            up_to_date_ids: IDs already correctly indexed in the DB.
            missing_ids: IDs that are empty or stale (text changed), requiring re-indexing.
        """
        # Step 1: Generate a 'Fingerprint' (MD5) for each incoming text.
        # If a startup's text changes by even 1 character, this hash will change.
        text_hashes = {sid: _md5(text) for sid, text in zip(startup_ids, texts)}

        try:
            async with self._pool.connection() as conn:
                await register_vector_async(conn)
                # Step 2: Query the DB for the current status of these IDs.
                # Notice we skip "Embedding" column here to keep the packet light.
                rows = await conn.execute(
                    'SELECT "Id", "EmbeddingHash" '
                    'FROM "Startups" WHERE "Id" = ANY(%s)',
                    (startup_ids,),
                )
                # Map the database response for fast O(1) lookup
                fetched = {
                    row[0]: row[1]  # {StartupId: StoredMD5Hash}
                    for row in await rows.fetchall()
                }
        except Exception as exc:
            logger.error(
                "pgvector status check failed: %s — falling back to re-indexing all", exc
            )
            return [], list(startup_ids)

        up_to_date_ids: list[int] = []
        missing_ids: list[int] = []

        # Step 3: Compare requested IDs against DB findings
        for sid in startup_ids:
            # Case A: ID not found in DB at all (New startup)
            if sid not in fetched:
                missing_ids.append(sid)
                continue

            stored_hash = fetched[sid]
            
            # Case B: ID exists but vector is empty (None) or text has changed (Stale)
            # stored_hash != text_hashes[sid] detects if the user updated their profile description.
            if stored_hash is None or stored_hash != text_hashes[sid]:
                logger.debug("Embedding missing or stale for startup_id=%d", sid)
                missing_ids.append(sid)
            else:
                # Case C: Exact match! We can use the existing vector in SQL matching.
                up_to_date_ids.append(sid)

        return up_to_date_ids, missing_ids

    async def get_similarities_sql(
        self,
        query_vector: np.ndarray,
        startup_ids: list[int],
    ) -> dict[int, float]:
        """
        Compute cosine similarity (1 - cosine distance) directly in PostgreSQL
        using pgvector's <=> operator.
        
        Returns: {startup_id: similarity_score}
        """
        if not startup_ids:
            return {}

        results: dict[int, float] = {}
        try:
            async with self._pool.connection() as conn:
                await register_vector_async(conn)
                # Cosine Similarity = 1 - Cosine Distance (<=> operator)
                rows = await conn.execute(
                    'SELECT "Id", (1 - ("Embedding" <=> %s)) AS similarity '
                    'FROM "Startups" WHERE "Id" = ANY(%s) '
                    'AND "Embedding" IS NOT NULL',
                    (query_vector.tolist(), startup_ids),
                )
                for row in await rows.fetchall():
                    results[row[0]] = float(row[1])
        except Exception as exc:
            logger.error("pgvector SQL scoring failed: %s", exc)
            return {}

        return results

    async def get_embedding(self, startup_id: int) -> np.ndarray | None:
        """
        Retrieves the 384-dimensional embedding for a specific startup from PostgreSQL.
        Used for B2B Startup-to-Startup matching where the source is already indexed.
        """
        try:
            async with self._pool.connection() as conn:
                await register_vector_async(conn)
                row = await (
                    await conn.execute(
                        'SELECT "Embedding" FROM "Startups" WHERE "Id" = %s',
                        (startup_id,)
                    )
                ).fetchone()
                
                if row and row[0] is not None:
                    # pgvector returns a numpy array or list natively depending on registration
                    return np.array(row[0], dtype=np.float32)
        except Exception as exc:
            logger.error("pgvector get_embedding failed for ID %d: %s", startup_id, exc)
        return None

    async def upsert_embeddings(
        self,
        startup_ids: list[int],
        texts: list[str],
        embeddings: list[np.ndarray],
    ) -> None:
        """
        Persist embeddings to the Startups table.
        Failures are non-fatal — logged as ERROR, never raised.
        """
        if not startup_ids:
            return

        try:
            async with self._pool.connection() as conn:
                await register_vector_async(conn)
                await conn.executemany(
                    'UPDATE "Startups" SET "Embedding" = %s, "EmbeddingHash" = %s '
                    'WHERE "Id" = %s',
                    [(emb.tolist(), _md5(text), sid)
                     for sid, text, emb in zip(startup_ids, texts, embeddings)],
                )
            logger.debug("Upserted %d embeddings into PostgreSQL", len(startup_ids))
        except Exception as exc:
            logger.error("pgvector upsert failed: %s", exc)

    async def get_stats(self) -> dict:
        """Return monitoring metadata."""
        return {
            "collection_name": "startup_embeddings",
            "persist_dir": "postgresql (pgvector)",
            "embedding_model": self._model_name,
            "dimensions": self._dimensions,
            "stored_embeddings": await self.count(),
        }

    async def count(self) -> int:
        """Number of startup embeddings stored in PostgreSQL."""
        try:
            async with self._pool.connection() as conn:
                row = await (
                    await conn.execute(
                        'SELECT COUNT(*) FROM "Startups" WHERE "Embedding" IS NOT NULL'
                    )
                ).fetchone()
                return int(row[0]) if row else 0
        except Exception:
            return 0


