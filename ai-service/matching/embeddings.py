"""
Embedding Engine
─────────────────
Handles text-to-vector conversion and similarity computation.
Uses Sentence Transformers (free, local) for MVP.
Can be swapped to OpenAI/Voyage embeddings later.
"""

import numpy as np
from sentence_transformers import SentenceTransformer
import hashlib
import logging
import os
from typing import Optional

logger = logging.getLogger(__name__)


class EmbeddingEngine:
    """Manages embedding model and provides encode/similarity operations."""

    def __init__(self, model_name: str | None = None):
        # We always use the static semantic model (all-MiniLM-L6-v2) for embeddings.
        # Dynamic model switching is reserved exclusively for the LLM (Reranker).
        self.model_name = model_name or os.getenv("EMBEDDING_MODEL", "all-MiniLM-L6-v2")
        self._model = SentenceTransformer(self.model_name)
        self.dimensions = self._model.get_sentence_embedding_dimension()
        self._cache: dict[str, np.ndarray] = {}

        logger.info(
            f"Loaded model '{model_name}' with {self.dimensions} dimensions"
        )

    def encode(self, text: str) -> np.ndarray:
        """
        Encode a single text into an embedding vector.
        Uses MD5 hash for caching.
        """
        cache_key = hashlib.md5(text.encode()).hexdigest()

        if cache_key in self._cache:
            return self._cache[cache_key]

        embedding = self._model.encode(text, normalize_embeddings=True)
        self._cache[cache_key] = embedding
        return embedding

    def encode_batch(self, texts: list[str]) -> list[np.ndarray]:
        """
        Encode multiple texts. Uses cache for previously seen texts,
        batch-encodes new ones for efficiency.
        """
        results: list[Optional[np.ndarray]] = [None] * len(texts)
        uncached_indices: list[int] = []
        uncached_texts: list[str] = []

        # Check cache first
        for i, text in enumerate(texts):
            cache_key = hashlib.md5(text.encode()).hexdigest()
            if cache_key in self._cache:
                results[i] = self._cache[cache_key]
            else:
                uncached_indices.append(i)
                uncached_texts.append(text)

        # Batch encode uncached
        if uncached_texts:
            embeddings = self._model.encode(
                uncached_texts, normalize_embeddings=True, batch_size=32
            )
            for idx, embedding in zip(uncached_indices, embeddings):
                cache_key = hashlib.md5(texts[idx].encode()).hexdigest()
                self._cache[cache_key] = embedding
                results[idx] = embedding

        return results  # type: ignore

    def cosine_similarities(
        self, query: np.ndarray, candidates: list[np.ndarray]
    ) -> np.ndarray:
        """
        Compute cosine similarity between a query vector and candidate vectors.
        Since embeddings are normalized, dot product = cosine similarity.

        NOTE: In production, similarity computation should be performed at the 
        database level (SQL pgvector) via VectorStore.get_similarities_sql() 
        for better performance and lower network overhead.
        """
        if not candidates:
            return np.array([])

        candidate_matrix = np.vstack(candidates)
        similarities = np.dot(candidate_matrix, query)
        return similarities

    def clear_cache(self):
        """Clear the embedding cache."""
        self._cache.clear()
        logger.info("Embedding cache cleared")

    @property
    def cache_size(self) -> int:
        return len(self._cache)
