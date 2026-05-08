#!/usr/bin/env bash
# add-mod.sh — clona um repo de mod, vendoriza em mods/<Nome>/ e gera estrutura padrão.
# Uso: add-mod.sh <git-url> [--name <ModName>] [--forge <forge-url>]

set -euo pipefail

URL=""
NAME=""
FORGE=""

while [[ $# -gt 0 ]]; do
  case "$1" in
    --name)  NAME="${2:-}"; shift 2 ;;
    --forge) FORGE="${2:-}"; shift 2 ;;
    -h|--help)
      sed -n '2,4p' "$0" | sed 's|^# \{0,1\}||'
      exit 0 ;;
    -*) echo "Erro: flag desconhecida: $1" >&2; exit 1 ;;
    *)
      if [[ -z "$URL" ]]; then URL="$1"
      else echo "Erro: argumento extra: $1" >&2; exit 1
      fi
      shift ;;
  esac
done

[[ -n "$URL" ]] || { echo "Erro: <git-url> é obrigatório" >&2; exit 1; }

# Inferir nome do repositório (basename sem .git)
if [[ -z "$NAME" ]]; then
  NAME="$(basename "$URL" .git)"
fi

ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
MOD_DIR="$ROOT/mods/$NAME"
TPL="$ROOT/.agents/templates"

[[ ! -e "$MOD_DIR" ]] || { echo "Erro: $MOD_DIR já existe" >&2; exit 1; }
[[ -f "$TPL/mod.json.tmpl" && -f "$TPL/mod-readme.md.tmpl" ]] \
  || { echo "Erro: templates ausentes em $TPL" >&2; exit 1; }

TMP="$(mktemp -d)"
trap 'rm -rf "$TMP"' EXIT

echo "→ Clonando $URL ..."
git clone --depth=1 "$URL" "$TMP/clone" >/dev/null 2>&1 \
  || { echo "Erro: clone falhou ($URL)" >&2; exit 1; }

SHA="$(git -C "$TMP/clone" rev-parse HEAD)"
BRANCH="$(git -C "$TMP/clone" rev-parse --abbrev-ref HEAD)"
[[ "$BRANCH" == "HEAD" ]] && BRANCH="(detached)"

# Heurística de licença: lê o primeiro LICENSE* encontrado
LICENSE="Unknown"
for f in LICENSE LICENSE.md LICENSE.txt LICENCE COPYING; do
  if [[ -f "$TMP/clone/$f" ]]; then
    if   grep -qi "MIT License" "$TMP/clone/$f"; then LICENSE="MIT"
    elif grep -qi "Apache License" "$TMP/clone/$f"; then LICENSE="Apache-2.0"
    elif grep -qi "GNU General Public License" "$TMP/clone/$f"; then
      if   grep -qi "Version 3" "$TMP/clone/$f"; then LICENSE="GPL-3.0"
      elif grep -qi "Version 2" "$TMP/clone/$f"; then LICENSE="GPL-2.0"
      else LICENSE="GPL"
      fi
    elif grep -qi "BSD" "$TMP/clone/$f"; then LICENSE="BSD"
    else LICENSE="see $f"
    fi
    break
  fi
done

# Heurística de versão: package.json → *.csproj
VERSION_BASE=""
if [[ -f "$TMP/clone/package.json" ]]; then
  VERSION_BASE="$(grep -oE '"version"[[:space:]]*:[[:space:]]*"[^"]+"' "$TMP/clone/package.json" \
                  | head -1 | sed -E 's/.*"([^"]+)"$/\1/' || true)"
fi
if [[ -z "$VERSION_BASE" ]]; then
  CSPROJ="$(find "$TMP/clone" -maxdepth 3 -name '*.csproj' 2>/dev/null | head -1)"
  if [[ -n "$CSPROJ" ]]; then
    VERSION_BASE="$(grep -oE '<Version>[^<]+' "$CSPROJ" | head -1 | sed 's|<Version>||' || true)"
  fi
fi
[[ -n "$VERSION_BASE" ]] || VERSION_BASE="unknown"

CREATED_AT="$(date -u +%Y-%m-%dT%H:%M:%SZ)"

# Remove .git — desvincula do upstream
rm -rf "$TMP/clone/.git"

# Monta estrutura
echo "→ Criando $MOD_DIR ..."
mkdir -p "$MOD_DIR"/{assets,backlog,builds,scripts}
mv "$TMP/clone" "$MOD_DIR/original"
cp -r "$MOD_DIR/original" "$MOD_DIR/modded"

# Renderiza templates
render() {
  sed \
    -e "s|{{NAME}}|$NAME|g" \
    -e "s|{{UPSTREAM_URL}}|$URL|g" \
    -e "s|{{UPSTREAM_SHA}}|$SHA|g" \
    -e "s|{{UPSTREAM_BRANCH}}|$BRANCH|g" \
    -e "s|{{FORGE_URL}}|$FORGE|g" \
    -e "s|{{LICENSE}}|$LICENSE|g" \
    -e "s|{{VERSION_BASE}}|$VERSION_BASE|g" \
    -e "s|{{CREATED_AT}}|$CREATED_AT|g" \
    "$1"
}

render "$TPL/mod.json.tmpl"        > "$MOD_DIR/mod.json"
render "$TPL/mod-readme.md.tmpl"   > "$MOD_DIR/README.md"

cat <<EOF

✓ Mod adicionado: $NAME
  Pasta:    mods/$NAME/
  Upstream: $URL
  SHA:      $SHA (branch: $BRANCH)
  Licença:  $LICENSE
  Versão:   $VERSION_BASE

Próximos passos:
  1. Edite mods/$NAME/mod.json — preencha "spt_version"
  2. Edite mods/$NAME/README.md — descrição e build
  3. Modifique mods/$NAME/modded/ (NUNCA mods/$NAME/original/)
  4. Ver diff: diff -r mods/$NAME/original/ mods/$NAME/modded/
EOF
