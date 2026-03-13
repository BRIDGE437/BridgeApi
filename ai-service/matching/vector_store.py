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

    async def get_embeddings(
        self,
        startup_ids: list[int],
        texts: list[str],
    ) -> tuple[dict[int, np.ndarray], list[int]]:
        """
        Fetch embeddings from the Startups table for the given IDs.

        Performs stale detection via MD5 hash comparison.

        Returns:
            cached_dict  – {startup_id: np.ndarray} for cache hits
            missing_ids  – startup_ids that must be encoded and upserted
        """
        text_hashes = {sid: _md5(text) for sid, text in zip(startup_ids, texts)}

        try:
            async with self._pool.connection() as conn:
                await register_vector_async(conn)
                rows = await conn.execute(
                    'SELECT "Id", "Embedding", "EmbeddingHash" '
                    'FROM "Startups" WHERE "Id" = ANY(%s)',
                    (startup_ids,),
                )
                fetched = {
                    row[0]: (row[1], row[2])
                    for row in await rows.fetchall()
                }
        except Exception as exc:
            logger.error(
                "pgvector get failed: %s — treating all as cache miss", exc
            )
            return {}, list(startup_ids)

        cached_dict: dict[int, np.ndarray] = {}
        missing_ids: list[int] = []

        for sid in startup_ids:
            if sid not in fetched:
                missing_ids.append(sid)
                continue

            emb, stored_hash = fetched[sid]
            if emb is None or stored_hash != text_hashes[sid]:
                logger.debug("Cache stale for startup_id=%d", sid)
                missing_ids.append(sid)
            else:
                cached_dict[sid] = np.array(emb, dtype=np.float32)

        return cached_dict, missing_ids

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
                for sid, text, emb in zip(startup_ids, texts, embeddings):
                    await conn.execute(
                        'UPDATE "Startups" SET "Embedding" = %s, "EmbeddingHash" = %s '
                        'WHERE "Id" = %s',
                        (emb.tolist(), _md5(text), sid),
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


