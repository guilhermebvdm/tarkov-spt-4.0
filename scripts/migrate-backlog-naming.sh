#!/bin/bash
# migrate-backlog-naming.sh — renomeia artefatos de backlog para nova convenção numerada.
#
# Antes:  NNN-<slug>-spec.md, -technical-spec.md, -technical-review-NN.md, -z-asbuild.md, -z-fix-NN.md
# Depois: NNN-<slug>-01-spec.md, -02-spec-tech.md, -03-spec-tech-review-NN.md, -05-asbuild.md, -06-fix-NN.md
#
# Atualiza também referências markdown ([...](old.md)) em .md de mods/, docs/, .claude/, .agents/.
#
# Uso:
#   bash scripts/migrate-backlog-naming.sh           # roda de verdade
#   bash scripts/migrate-backlog-naming.sh --dry     # só lista o que faria
#
# Idempotente: re-rodar não causa dano (arquivos já renomeados não dão match no padrão antigo).
# Git Bash no Windows: usa sintaxe GNU `sed -i ...` (sem '').

set -euo pipefail

DRY=0
if [ "${1:-}" = "--dry" ]; then
  DRY=1
  echo "🔍 DRY RUN — nenhum arquivo será modificado."
fi

REPO_ROOT="$(git rev-parse --show-toplevel 2>/dev/null || pwd)"
cd "$REPO_ROOT"

# ----------------------------------------------------------------------
# Etapa 1: renomear arquivos em mods/*/backlog/NNN-<slug>/
# ----------------------------------------------------------------------

declare -a RENAMES=()   # pares "old_path|new_path"

# Iterar todas as pastas de itens de backlog
while IFS= read -r -d '' DIR; do
  # DIR = mods/<mod>/backlog/NNN-<slug>
  SLUG_DIR="$(basename "$DIR")"        # ex.: 001-stamina-e-velocidade
  # Slug item = todo o nome da pasta (NNN-<slug>)

  for OLD in "$DIR"/*.md; do
    [ -f "$OLD" ] || continue
    OLD_NAME="$(basename "$OLD")"
    NEW_NAME=""

    case "$OLD_NAME" in
      # Já no novo padrão — pular
      "${SLUG_DIR}-01-spec.md" \
      | "${SLUG_DIR}-02-spec-tech.md" \
      | "${SLUG_DIR}"-03-spec-tech-review-[0-9][0-9].md \
      | "${SLUG_DIR}"-04-code-review-[0-9][0-9].md \
      | "${SLUG_DIR}-05-asbuild.md" \
      | "${SLUG_DIR}"-06-fix-[0-9][0-9].md)
        continue
        ;;
      # spec funcional
      "${SLUG_DIR}-spec.md")
        NEW_NAME="${SLUG_DIR}-01-spec.md"
        ;;
      # spec técnica
      "${SLUG_DIR}-technical-spec.md")
        NEW_NAME="${SLUG_DIR}-02-spec-tech.md"
        ;;
      # review da spec técnica (com NN de 2 dígitos)
      "${SLUG_DIR}"-technical-review-[0-9][0-9].md)
        NN="${OLD_NAME##*-technical-review-}"
        NN="${NN%.md}"
        NEW_NAME="${SLUG_DIR}-03-spec-tech-review-${NN}.md"
        ;;
      # asbuild
      "${SLUG_DIR}-z-asbuild.md")
        NEW_NAME="${SLUG_DIR}-05-asbuild.md"
        ;;
      # fix-NN
      "${SLUG_DIR}"-z-fix-[0-9][0-9].md)
        NN="${OLD_NAME##*-z-fix-}"
        NN="${NN%.md}"
        NEW_NAME="${SLUG_DIR}-06-fix-${NN}.md"
        ;;
      *)
        # Padrão não-reconhecido — log e pular
        echo "  [skip] $OLD_NAME (padrão não-reconhecido)"
        continue
        ;;
    esac

    NEW_PATH="$DIR/$NEW_NAME"
    RENAMES+=( "$OLD|$NEW_PATH" )
  done
done < <(find mods -type d -path 'mods/*/backlog/[0-9][0-9][0-9]-*' -print0)

if [ ${#RENAMES[@]} -eq 0 ]; then
  echo "✓ Nada a renomear — pasta já no padrão novo."
  exit 0
fi

echo ""
echo "📋 Renames planejados (${#RENAMES[@]} arquivos):"
for PAIR in "${RENAMES[@]}"; do
  OLD="${PAIR%%|*}"
  NEW="${PAIR##*|}"
  echo "  $(basename "$OLD")  →  $(basename "$NEW")"
done

if [ "$DRY" = "1" ]; then
  echo ""
  echo "(dry-run — saindo sem modificar)"
  exit 0
fi

# ----------------------------------------------------------------------
# Etapa 2: executar renames
# ----------------------------------------------------------------------

echo ""
echo "🔨 Renomeando arquivos..."
for PAIR in "${RENAMES[@]}"; do
  OLD="${PAIR%%|*}"
  NEW="${PAIR##*|}"
  git mv "$OLD" "$NEW" 2>/dev/null || mv "$OLD" "$NEW"
done
echo "✓ ${#RENAMES[@]} arquivos renomeados."

# ----------------------------------------------------------------------
# Etapa 3: atualizar referências markdown ([...](old.md), old.md em texto)
# ----------------------------------------------------------------------

echo ""
echo "🔧 Atualizando referências em .md..."

# Escopo: arquivos .md de mods/, docs/, .claude/, .agents/
# Skip: node_modules, .git, original/ (upstream intocado)
mapfile -t MD_FILES < <(find mods docs .claude .agents -type f -name '*.md' \
  -not -path '*/node_modules/*' \
  -not -path '*/.git/*' \
  -not -path '*/original/*' \
  2>/dev/null)

UPDATED=0
for FILE in "${MD_FILES[@]}"; do
  [ -f "$FILE" ] || continue

  # Cada rename gera 1 substituição. Construir todos os pares e aplicar via sed.
  # Para cada NNN-<slug>-X.md → NNN-<slug>-Y.md, substituir literal.
  # Uso o nome BASE em vez de path completo, porque links em markdown costumam ser relativos.

  CHANGED=0
  for PAIR in "${RENAMES[@]}"; do
    OLD_BASE="$(basename "${PAIR%%|*}")"
    NEW_BASE="$(basename "${PAIR##*|}")"
    # Escapar dots e hyphens para regex seguro
    OLD_ESC=$(printf '%s\n' "$OLD_BASE" | sed 's/[.[\*^$()+?{|]/\\&/g')
    if grep -q "$OLD_ESC" "$FILE" 2>/dev/null; then
      sed -i "s|$OLD_ESC|$NEW_BASE|g" "$FILE"
      CHANGED=1
    fi
  done

  if [ "$CHANGED" = "1" ]; then
    UPDATED=$((UPDATED + 1))
    echo "  $FILE"
  fi
done

echo ""
echo "✓ Migração concluída."
echo "   Arquivos renomeados: ${#RENAMES[@]}"
echo "   Arquivos .md com refs atualizadas: $UPDATED"
echo ""
echo "Próximo: revisar diff com 'git diff --stat' e 'git diff -- ...specific.md' antes de commit."
