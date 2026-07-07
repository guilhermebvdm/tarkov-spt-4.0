#!/bin/bash
# Git pre-commit hook: auto-append histórico em docs/**/*.md modificados

set -eu

# Pula se for amend para evitar entradas duplicadas
if [ "${GIT_AMEND:-}" = "1" ]; then
  exit 0
fi

# Gates do harness (revisão de valor — converte invariantes de prosa em enforcement).
# Resolve o diretório deste script para chamar os gates vizinhos.
HOOK_DIR="$(cd "$(dirname "$0")" && pwd)"
# HARD: bloqueia item 🟢 com validação in-game pendente (AP-06).
bash "$HOOK_DIR/check-delivered-validation.sh" || exit 1
# WARN: grafo de mod defasado vs código mudado.
bash "$HOOK_DIR/check-graph-freshness.sh" || true
# WARN: pendência de memória sem [P-N.M].
bash "$HOOK_DIR/check-memory-ids.sh" || true
# WARN: .csproj com <HintPath> absoluto em vez de References\ (.spt-path).
bash "$HOOK_DIR/check-csproj-references.sh" || true
# HARD: frontmatter obrigatório em docs/**/*.md staged. Mesma regra do hook do Claude Code
# (.claude/settings.json), mas aqui vale para QUALQUER commit — inclusive fora do Claude Code.
DOCS_HEADER_STAGED=$(git diff --cached --name-only --diff-filter=ACM | grep -E '^docs/.+\.md$' | grep -v 'README\.md' || true)
DOCS_HEADER_FAIL=0
while IFS= read -r DOC; do
  [ -n "$DOC" ] || continue
  bash "$HOOK_DIR/validate-doc-header.sh" "$DOC" || DOCS_HEADER_FAIL=1
done <<< "$DOCS_HEADER_STAGED"
if [ "$DOCS_HEADER_FAIL" -ne 0 ]; then
  echo "❌ Frontmatter inválido em doc(s) staged — commit abortado (ver acima)." >&2
  exit 1
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
