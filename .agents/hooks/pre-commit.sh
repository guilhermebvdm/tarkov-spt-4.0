#!/bin/bash
# Git pre-commit hook: auto-append histórico em docs/**/*.md modificados

set -eu

# Pula se for amend para evitar entradas duplicadas
if [ "${GIT_AMEND:-}" = "1" ]; then
  exit 0
fi

# Valida o manifesto de referencias quando ele estiver staged (abortar commit se invalido)
MANIFEST_STAGED=$(git diff --cached --name-only --diff-filter=ACM | grep -E '^references/manifest\.json$' || true)
if [ -n "$MANIFEST_STAGED" ]; then
  if command -v node >/dev/null 2>&1; then
    node scripts/setup-references.js --check-manifest || {
      echo "❌ references/manifest.json invalido — commit abortado." >&2
      exit 1
    }
  else
    echo "⚠ node nao encontrado — pulando validacao de references/manifest.json" >&2
  fi
fi

AUTHOR=$(git config user.name 2>/dev/null || echo "unknown")
DATE=$(date +%Y-%m-%d)

# Lê resumo da mensagem do commit (.git/COMMIT_EDITMSG)
# Funciona com `git commit -m "..."` (mensagem já está no arquivo).
# Para `git commit` interativo, o arquivo só tem o template — fallback abaixo.
COMMIT_MSG_FILE="$(git rev-parse --git-path COMMIT_EDITMSG)"
SUMMARY="(sem mensagem)"
if [ -f "$COMMIT_MSG_FILE" ]; then
  CANDIDATE=$(grep -v '^#' "$COMMIT_MSG_FILE" 2>/dev/null | grep -v '^[[:space:]]*$' | head -1 || true)
  if [ -n "$CANDIDATE" ]; then
    # Escapa pipes para não quebrar a tabela markdown
    SUMMARY=$(echo "$CANDIDATE" | sed 's/|/\\|/g')
    # Trunca se passar de 120 caracteres
    if [ ${#SUMMARY} -gt 120 ]; then
      SUMMARY="${SUMMARY:0:117}..."
    fi
  fi
fi

# Arquivos .md em docs/ que estão staged (qualquer profundidade)
STAGED=$(git diff --cached --name-only --diff-filter=ACM | grep -E '^docs/.+\.md$' | grep -v 'README\.md' || true)

[ -z "$STAGED" ] && exit 0

while IFS= read -r FILE; do
  [ -f "$FILE" ] || continue

  if ! grep -q "^## Histórico" "$FILE"; then
    # Não existe seção — cria no final
    printf '\n## Histórico\n\n| Data | Autor | Descrição |\n|---|---|---|\n| %s | %s | %s |\n' \
      "$DATE" "$AUTHOR" "$SUMMARY" >> "$FILE"
  else
    # Já existe — insere a nova linha logo após a última linha da tabela de histórico
    awk -v date="$DATE" -v author="$AUTHOR" -v summary="$SUMMARY" '
      BEGIN { in_hist = 0; last_pipe = 0 }
      /^## Histórico/ { in_hist = 1 }
      in_hist && /^\|/ { last_pipe = NR }
      in_hist && !/^\|/ && !/^$/ && last_pipe > 0 {
        # Saiu da tabela — insere antes desta linha
        print "| " date " | " author " | " summary " |"
        in_hist = 0
        last_pipe = 0
      }
      { print }
      END {
        if (in_hist && last_pipe > 0) {
          print "| " date " | " author " | " summary " |"
        }
      }
    ' "$FILE" > "$FILE.tmp" && mv "$FILE.tmp" "$FILE"
  fi

  git add "$FILE"

done <<< "$STAGED"

exit 0
