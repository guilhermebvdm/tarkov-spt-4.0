#!/bin/bash
# Pre-commit gate: warn when a mod's modded/ source changed in this commit but its
# published code graph (references/graphs/mods/<mod>/graph.json) was NOT staged too.
# WARN-ONLY (exit 0) — the rule is "commit the graph together or in the immediately
# following commit"; a hard block would fight that legitimate flow. Run regen with
# /update-mod-graph <mod> (or bash scripts/update-graphs.sh <mod>).
#
# Findings D3-01/D3-05/D5-03/DC-07 from the harness value review.

set -eu

[ "${GIT_AMEND:-}" = "1" ] && exit 0

# Source files whose change shifts graph topology (mirror update-graphs.sh scope).
SRC_RE='\.(cs|ts|tsx|js|mjs|razor)$'

CHANGED_MODS=$(git diff --cached --name-only --diff-filter=ACMR \
  | grep -E "^mods/[^/]+/modded/" \
  | grep -E "$SRC_RE" \
  | sed -E 's#^mods/([^/]+)/modded/.*#\1#' \
  | sort -u || true)

[ -z "$CHANGED_MODS" ] && exit 0

STALE=""
while IFS= read -r MOD; do
  [ -z "$MOD" ] && continue
  GRAPH="references/graphs/mods/$MOD/graph.json"
  # No published graph at all → mod is binary/config-only (intentional skip) → ignore.
  [ -f "$GRAPH" ] || continue
  # Graph exists but was not staged in this commit → potentially stale.
  if ! git diff --cached --name-only --diff-filter=ACMR | grep -qx "$GRAPH"; then
    STALE="$STALE $MOD"
  fi
done <<< "$CHANGED_MODS"

if [ -n "$STALE" ]; then
  echo "⚠ Grafo possivelmente defasado — código de mod mudou sem regenerar o grafo:" >&2
  for M in $STALE; do echo "    • $M  →  /update-mod-graph $M" >&2; done
  echo "  (aviso — não bloqueia; commit o grafo junto ou no commit seguinte. Ver references/graphs/README.md)" >&2
fi

exit 0
