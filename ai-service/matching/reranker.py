"""
LLM Reranker
─────────────
Optional module: sends top candidates to an LLM for detailed scoring
and natural language explanations.

Supports: OpenAI GPT-4o-mini, Google Gemini (1.5-flash/pro), or Anthropic Claude.
Configure via environment variables.

LLM results are cached in the PostgreSQL LlmScoreCache table using
MD5 hashes of investor and startup texts, so repeated calls for the
same pair never reach the LLM API.
"""

import json
import logging
import os
from typing import Any

import httpx
from psycopg_pool import AsyncConnectionPool

from matching.utils import md5 as _md5

logger = logging.getLogger(__name__)


class LlmReranker:
    """Handles LLM-based reranking of startup candidates with PostgreSQL cache."""

    def __init__(self, pool: AsyncConnectionPool | None = None):
        self.provider = os.getenv("LLM_PROVIDER", "openai")  # openai, google, anthropic
        self.api_key = os.getenv("LLM_API_KEY", "")
        self.model = os.getenv(
            "LLM_MODEL",
            "gpt-4o-mini" if self.provider == "openai" 
            else ("gemini-1.5-flash" if self.provider == "google" else "claude-sonnet-4-20250514"),
        )
        self._pool = pool

        if not self.api_key:
            logger.warning("No LLM_API_KEY set. LLM reranking will be unavailable.")

    async def rerank(
        self,
        investor_text: str,
        startups: list[Any],
        mode: str = "investor_startup",
    ) -> dict[int, dict[str, Any]]:
        """
        Score candidates using LLM, with PostgreSQL cache.

        mode: "investor_startup" | "startup_similarity"
        Cache key: (investor_text MD5, startup_text MD5).
        Returns: { startup_id: { "score": float, "reason": str } }
        """
        if not self.api_key:
            raise RuntimeError("LLM API key not configured")

        investor_hash = _md5(investor_text)

        # ── Check cache ──
        cached, misses = await self._check_cache(investor_hash, startups)

        # ── Call LLM for cache misses only ──
        llm_results: dict[int, dict] = {}
        if misses:
            prompt = self._build_prompt(investor_text, misses, mode)
            if self.provider == "openai":
                llm_results = await self._call_openai(prompt)
            elif self.provider == "anthropic":
                llm_results = await self._call_anthropic(prompt)
            elif self.provider == "google":
                llm_results = await self._call_gemini(prompt)
            else:
                raise ValueError(f"Unknown LLM provider: {self.provider}")

            # ── Store new results in cache ──
            await self._store_cache(investor_hash, misses, llm_results)

        return {**cached, **llm_results}

    async def rerank_by_ids(
        self,
        source_id: int,
        target_ids: list[int],
        mode: str = "startup_startup",
    ) -> dict[int, dict[str, Any]]:
        """
        Specialized rerank that fetches source and target texts from DB first.
        Used for B2B Networking where only IDs are provided.
        """
        if not self._pool:
            return {}

        async with self._pool.connection() as conn:
            # 1. Fetch Source
            source_row = await (await conn.execute(
                'SELECT "Name", "Tags", "Description", "BusinessModel", "HQ" '
                'FROM "Startups" WHERE "Id" = %s',
                (source_id,)
            )).fetchone()
            
            if not source_row:
                return {}
            
            source_text = f"{source_row[0]}. {source_row[1] or ''}. {source_row[2] or ''}. {source_row[3] or ''}. {source_row[4] or ''}"

            # 2. Fetch Targets
            rows = await conn.execute(
                'SELECT "Id", "Name", "Tags", "Description", "BusinessModel", "HQ" '
                'FROM "Startups" WHERE "Id" = ANY(%s)',
                (target_ids,)
            )
            
            from collections import namedtuple
            StartupStub = namedtuple("StartupStub", ["id", "text"])
            target_stubs = []
            
            for row in await rows.fetchall():
                text = f"{row[1]}. {row[2] or ''}. {row[3] or ''}. {row[4] or ''}. {row[5] or ''}"
                target_stubs.append(StartupStub(id=row[0], text=text))

        return await self.rerank(source_text, target_stubs, mode)

    # ── Cache helpers ────────────────────────────────────────────────────

    async def _check_cache(
        self, investor_hash: str, startups: list[Any]
    ) -> tuple[dict[int, dict], list[Any]]:
        """Returns (cache_hits_dict, cache_miss_startups)."""
        if not self._pool:
            return {}, startups

        startup_hashes = {s.id: _md5(s.text) for s in startups}
        startup_ids = list(startup_hashes.keys())

        try:
            async with self._pool.connection() as conn:
                rows = await conn.execute(
                    """
                    SELECT "StartupId", "StartupTextHash", "Score", "Reason"
                    FROM "LlmScoreCache"
                    WHERE "InvestorTextHash" = %s
                      AND "StartupId" = ANY(%s)
                    """,
                    (investor_hash, startup_ids),
                )
                db_rows = await rows.fetchall()
        except Exception as exc:
            logger.warning("LLM cache lookup failed: %s — skipping cache", exc)
            return {}, startups

        cached: dict[int, dict] = {}
        for row in db_rows:
            sid, stored_startup_hash, score, reason = row
            if stored_startup_hash == startup_hashes.get(sid):
                cached[sid] = {"score": float(score), "reason": reason or ""}

        misses = [s for s in startups if s.id not in cached]
        logger.debug(
            "LLM cache: %d hits, %d misses out of %d",
            len(cached), len(misses), len(startups),
        )
        return cached, misses

    async def _store_cache(
        self, investor_hash: str, startups: list[Any], results: dict[int, dict]
    ) -> None:
        """
        Upsert LLM results into LlmScoreCache table.

        Stale cleanup:
        - Per-row: deletes old entries for the same (InvestorTextHash, StartupId)
          when StartupTextHash has changed (startup text was updated).
        - Periodic: prunes entries older than 90 days to handle investor text changes
          and general cache hygiene (runs 1% of the time to avoid overhead).
        """
        if not self._pool or not results:
            return

        import random

        try:
            async with self._pool.connection() as conn:
                # Periodic TTL cleanup (1% chance per call)
                if random.random() < 0.01:
                    deleted = await conn.execute(
                        'DELETE FROM "LlmScoreCache" WHERE "CreatedAt" < NOW() - INTERVAL \'90 days\''
                    )
                    logger.info("LLM cache TTL cleanup: removed stale rows")

                for s in startups:
                    if s.id not in results:
                        continue
                    res = results[s.id]
                    startup_hash = _md5(s.text)

                    # Delete stale entry if startup text changed
                    await conn.execute(
                        """
                        DELETE FROM "LlmScoreCache"
                        WHERE "InvestorTextHash" = %s
                          AND "StartupId" = %s
                          AND "StartupTextHash" != %s
                        """,
                        (investor_hash, s.id, startup_hash),
                    )

                    await conn.execute(
                        """
                        INSERT INTO "LlmScoreCache"
                            ("InvestorTextHash", "StartupId", "StartupTextHash",
                             "Score", "Reason", "CreatedAt")
                        VALUES (%s, %s, %s, %s, %s, NOW())
                        ON CONFLICT ("InvestorTextHash", "StartupId", "StartupTextHash")
                        DO UPDATE SET "Score" = EXCLUDED."Score",
                                      "Reason" = EXCLUDED."Reason",
                                      "CreatedAt" = NOW()
                        """,
                        (investor_hash, s.id, startup_hash,
                         float(res.get("score", 0)), res.get("reason", "")),
                    )
            logger.debug("Stored %d LLM scores in cache", len(results))
        except Exception as exc:
            logger.error("LLM cache store failed: %s", exc)

    # ── Prompt & API calls ───────────────────────────────────────────────

    def _build_prompt(self, reference_text: str, startups: list[Any], mode: str = "investor_startup") -> str:
        startup_lines = [f"ID: {s.id} | {s.text}" for s in startups]
        startups_block = "\n".join(startup_lines)

        if mode == "startup_similarity":
            return f"""You are an expert startup analyst evaluating business similarity.

## Reference Startup
{reference_text}

## Candidate Startups
{startups_block}

## Task
Rate each candidate startup 0-10 for how similar it is to the reference startup.
Consider: same problem domain, overlapping technology stack, similar target market, comparable business model, and competitive/complementary positioning.

Return ONLY valid JSON array (no markdown, no explanation outside JSON):
[
  {{"id": 12345, "score": 8, "reason": "Both target SME financial management with SaaS B2B model..."}},
  ...
]"""

        if mode == "startup_startup":
            return f"""You are a B2B Networking Expert evaluating strategic partnerships.

## Source Startup (Initiator)
{reference_text}

## Candidate Startups
{startups_block}

## Task
Rate each candidate startup 0-10 on their potential to form a synergetic strategic partnership, client-vendor relationship, or joint venture with the Source Startup.
Consider: complementary technologies, target market overlap, potential B2B service exchange, and shared mission.

Return ONLY valid JSON array (no markdown, no explanation outside JSON):
[
  {{"id": 12345, "score": 8, "reason": "Excellent synergy: Candidate provides payment APIs needed by Source's E-commerce platform..."}},
  ...
]"""

        # Default: investor_startup mode
        return f"""You are an expert startup-investor matchmaker.

## Investor Profile
{reference_text}

## Candidate Startups
{startups_block}

## Task
Rate each startup 0-10 for how well it fits this investor's profile.
Consider sector alignment, business model fit, geographic preference, and growth stage.

Return ONLY valid JSON array (no markdown, no explanation outside JSON):
[
  {{"id": 12345, "score": 8, "reason": "Strong sector fit in fintech with B2B model..."}},
  ...
]"""

    async def _call_openai(self, prompt: str) -> dict[int, dict]:
        async with httpx.AsyncClient(timeout=30) as client:
            response = await client.post(
                "https://api.openai.com/v1/chat/completions",
                headers={
                    "Authorization": f"Bearer {self.api_key}",
                    "Content-Type": "application/json",
                },
                json={
                    "model": self.model,
                    "messages": [
                        {
                            "role": "system",
                            "content": "You are a startup-investor matching expert. Always respond with valid JSON only.",
                        },
                        {"role": "user", "content": prompt},
                    ],
                    "temperature": 0.3,
                    "max_tokens": 2000,
                },
            )
            response.raise_for_status()
            data = response.json()

        content = data["choices"][0]["message"]["content"]
        return self._parse_llm_response(content)

    async def _call_anthropic(self, prompt: str) -> dict[int, dict]:
        async with httpx.AsyncClient(timeout=30) as client:
            response = await client.post(
                "https://api.anthropic.com/v1/messages",
                headers={
                    "x-api-key": self.api_key,
                    "content-type": "application/json",
                    "anthropic-version": "2023-06-01",
                },
                json={
                    "model": self.model,
                    "max_tokens": 2000,
                    "messages": [{"role": "user", "content": prompt}],
                },
            )
            response.raise_for_status()
            data = response.json()

        content = data["content"][0]["text"]
        return self._parse_llm_response(content)

    async def _call_gemini(self, prompt: str) -> dict[int, dict]:
        """Call Google Gemini API."""
        url = f"https://generativelanguage.googleapis.com/v1beta/models/{self.model}:generateContent?key={self.api_key}"
        async with httpx.AsyncClient(timeout=30) as client:
            response = await client.post(
                url,
                json={
                    "contents": [{"parts": [{"text": prompt}]}]
                },
            )
            response.raise_for_status()
            data = response.json()

        try:
            content = data["candidates"][0]["content"]["parts"][0]["text"]
            return self._parse_llm_response(content)
        except (KeyError, IndexError) as e:
            logger.error(f"Gemini response parsing failed: {e}\nResponse: {data}")
            return {}

    def _parse_llm_response(self, content: str) -> dict[int, dict]:
        """Parse LLM JSON response into { id: { score, reason } } dict."""
        try:
            cleaned = content.strip()
            if cleaned.startswith("```"):
                cleaned = cleaned.split("\n", 1)[1]
            if cleaned.endswith("```"):
                cleaned = cleaned.rsplit("```", 1)[0]
            cleaned = cleaned.strip()

            items = json.loads(cleaned)
            result = {}
            for item in items:
                startup_id = item.get("id")
                score = float(item.get("score", 0))
                reason = item.get("reason", "")
                if startup_id is not None:
                    result[startup_id] = {"score": score, "reason": reason}
            return result

        except (json.JSONDecodeError, KeyError, TypeError) as e:
            logger.error(f"Failed to parse LLM response: {e}\nContent: {content}")
            return {}


