#!/bin/bash
# update-graphs.sh — single source of truth for (re)generating code graphs via graphify.
#
# Usage:
#   bash scripts/update-graphs.sh             # all scopes (mods + references)
#   bash scripts/update-graphs.sh <mod>       # one mod (folder name under mods/)
#   bash scripts/update-graphs.sh <ref-id>    # one reference: eft-decompiled,
#                                             #   fika-plugin, fika-server, fika-headless, spt-source
#
# How it works:
#   1. Runs `graphify update <path>` per scope (AST-only, no LLM, incremental).
#      Working output lands at <path>/graphify-out/ (gitignored).
#   2. Copies graph.json / GRAPH_REPORT.md / graph.html to references/graphs/<id>/
#      — the VERSIONED location consumed by .mcp.json and the graph-code-navigation skill.
#   Ignore rules come from .graphifyignore at repo root (overrides .gitignore so the
#   vendored, gitignored references/spt-source and fika-* ARE extracted).
#
# Commit rule: regenerated graphs are committed together with the code change that
# motivated them (see references/graphs/README.md).

set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$REPO_ROOT"

# graphify lives in ~/.local/bin when installed via `uv tool install graphifyy`
export PATH="$HOME/.local/bin:$PATH"

if ! command -v graphify >/dev/null 2>&1; then
  echo "❌ graphify não encontrado no PATH."
  echo "   Instale com: python -m pip install --user uv && python -m uv tool install graphifyy"
  echo "   (binários vão para ~/.local/bin — rode 'python -m uv tool update-shell' uma vez)"
  exit 1
fi

GRAPHS_DIR="references/graphs"
FILTER="${1:-}"

# scope <id> <path> — extract one scope and publish artifacts to references/graphs/<id>/
scope() {
  local id="$1" path="$2"
  if [ ! -d "$path" ]; then
    echo "⏭️  $id — pasta ausente ($path), pulando. (references gitignored: rode 'node scripts/setup-references.js')"
    return 0
  fi
  echo "▶ $id ($path)"
  graphify update "$path" 2>&1 | grep -E "Rebuilt|No code" || true
  local out="$path/graphify-out"
  if [ ! -f "$out/graph.json" ]; then
    echo "⚠️  $id — extração não produziu graph.json, pulando publicação."
    return 0
  fi
  mkdir -p "$GRAPHS_DIR/$id"
  cp "$out/graph.json" "$GRAPHS_DIR/$id/graph.json"
  [ -f "$out/GRAPH_REPORT.md" ] && cp "$out/GRAPH_REPORT.md" "$GRAPHS_DIR/$id/GRAPH_REPORT.md"
  [ -f "$out/graph.html" ] && cp "$out/graph.html" "$GRAPHS_DIR/$id/graph.html"
  echo "  ✓ publicado em $GRAPHS_DIR/$id/"
}

matches() { [ -z "$FILTER" ] || [ "$FILTER" = "$1" ]; }

RAN=0

# Mods — auto-discovered by glob (no manual registration; /add-mod-repo-for-modding just works)
for modded in mods/*/modded; do
  [ -d "$modded" ] || continue
  mod="$(basename "$(dirname "$modded")")"
  if matches "$mod"; then
    scope "mods/$mod" "$modded"
    RAN=1
  fi
done

# References — fixed list (ids are stable for .mcp.json and the skill)
matches "eft-decompiled" && { scope "eft-decompiled" "references/eft-decompiled/Assembly-CSharp"; RAN=1; }
matches "fika-plugin"    && { scope "fika-plugin"    "references/fika-plugin";    RAN=1; }
matches "fika-server"    && { scope "fika-server"    "references/fika-server";    RAN=1; }
matches "fika-headless"  && { scope "fika-headless"  "references/fika-headless";  RAN=1; }
matches "spt-source"     && { scope "spt-source"     "references/spt-source";     RAN=1; }

if [ "$RAN" = "0" ]; then
  echo "❌ Escopo '$FILTER' não encontrado. Use um nome de mod (pasta em mods/) ou:"
  echo "   eft-decompiled · fika-plugin · fika-server · fika-headless · spt-source"
  exit 1
fi

echo "✓ Grafos atualizados. Commitar references/graphs/ junto com a mudança de código que motivou."
