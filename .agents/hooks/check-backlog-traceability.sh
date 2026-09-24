#!/bin/bash
# Pre-commit gate: warn when a mod's modded*/ source changed in this commit but no
# backlog artifact (mods/<mod>/backlog/**) was staged together with it. WARN-ONLY
# (exit 0) — mirrors check-graph-freshness.sh's tolerance (commit the backlog item
# together or in the immediately following commit); a hard block would fight
# legitimate iterative follow-up commits within an already-open item.
#
# Root cause: agents (Claude and Gemini) have shipped working code straight into
# mods/<mod>/modded*/ (including a second parallel fork, modded-V2/) without ever
# opening a backlog item — no spec, no review, no code-review — discovered only by
# manual diff audit after the fact (sessão 2026-09-15, FIKA AoI/threading work and
# TRL-DynamicSpawn stutter fixes). Only fires for mods that already use the backlog
# workflow (have a mods/<mod>/backlog/ directory); mods without one are untouched.

set -eu

[ "${GIT_AMEND:-}" = "1" ] && exit 0

# Source files whose change represents real mod logic (mirror check-graph-freshness.sh scope).
SRC_RE='\.(cs|ts|tsx|js|mjs|razor)$'

CHANGED_MODS=$(git diff --cached --name-only --diff-filter=ACMR \
  | grep -E '^mods/[^/]+/modded[^/]*/' \
  | grep -E "$SRC_RE" \
  | sed -E 's#^mods/([^/]+)/modded[^/]*/.*#\1#' \
  | sort -u || true)

[ -z "$CHANGED_MODS" ] && exit 0

STAGED_BACKLOG_MODS=$(git diff --cached --name-only --diff-filter=ACMR \
  | grep -E '^mods/[^/]+/backlog/' \
  | sed -E 's#^mods/([^/]+)/backlog/.*#\1#' \
  | sort -u || true)

MISSING=""
while IFS= read -r MOD; do
  [ -z "$MOD" ] && continue
  # Mod doesn't use the backlog workflow at all → nothing to compare against, skip.
  [ -d "mods/$MOD/backlog" ] || continue
  if ! echo "$STAGED_BACKLOG_MODS" | grep -qx "$MOD"; then
    MISSING="$MISSING $MOD"
  fi
done <<< "$CHANGED_MODS"

if [ -n "$MISSING" ]; then
  echo "⚠ Código de mod mudou sem item de backlog correspondente staged neste commit:" >&2
  for M in $MISSING; do echo "    • $M  →  abra/atualize mods/$M/backlog/NNN-slug/ (ver WORKFLOW.md)" >&2; done
  echo "  (aviso — não bloqueia; commit o item de backlog junto ou no commit seguinte)" >&2
fi

exit 0
