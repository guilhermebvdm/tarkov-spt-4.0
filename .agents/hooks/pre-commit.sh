#!/bin/bash
# Git pre-commit hook: auto-append histórico em docs/**/*.md modificados

set -eu

# Pula se for amend para evitar entradas duplicadas
if [ "${GIT_AMEND:-}" = "1" ]; then
  exit 0
fi

AUTHOR=$(git config user.name 2>/dev/null || echo "unknown")
DATE=$(date +%Y-%m-%d)

# Arquivos .md em docs/ que estão staged (qualquer profundidade)
STAGED=$(git diff --cached --name-only --diff-filter=ACM | grep -E '^docs/.+\.md$' | grep -v 'README\.md' || true)

[ -z "$STAGED" ] && exit 0

while IFS= read -r FILE; do
  [ -f "$FILE" ] || continue

  # Estatística de linhas alteradas (via diff staged)
  STATS=$(git diff --cached --numstat "$FILE" | awk '{printf "+%s / -%s linhas", $1, $2}')
  [ -z "$STATS" ] && STATS="alterado"

  if ! grep -q "^## Histórico" "$FILE"; then
    # Não existe seção — cria no final
    printf '\n## Histórico\n\n| Data | Autor | Descrição |\n|---|---|---|\n| %s | %s | %s |\n' \
      "$DATE" "$AUTHOR" "$STATS" >> "$FILE"
  else
    # Já existe — insere a nova linha logo após a última linha da tabela de histórico
    awk -v date="$DATE" -v author="$AUTHOR" -v stats="$STATS" '
      BEGIN { in_hist = 0; last_pipe = 0 }
      /^## Histórico/ { in_hist = 1 }
      in_hist && /^\|/ { last_pipe = NR }
      in_hist && !/^\|/ && !/^$/ && last_pipe > 0 {
        # Saiu da tabela — insere antes desta linha
        print "| " date " | " author " | " stats " |"
        in_hist = 0
        last_pipe = 0
      }
      { print }
      END {
        if (in_hist && last_pipe > 0) {
          print "| " date " | " author " | " stats " |"
        }
      }
    ' "$FILE" > "$FILE.tmp" && mv "$FILE.tmp" "$FILE"
  fi

  git add "$FILE"

done <<< "$STAGED"

exit 0
