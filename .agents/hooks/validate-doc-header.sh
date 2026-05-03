#!/bin/bash
# Valida frontmatter obrigatório em docs/**/*.md
# Claude Code: recebe JSON via stdin com tool_input.file_path
# Manual:      bash validate-doc-header.sh <caminho-do-arquivo>

set -euo pipefail

# Prioriza $1 (chamada manual). Só tenta parsear stdin se $1 não vier.
FILE="${1:-}"

if [ -z "$FILE" ]; then
  # Sem argumento — provavelmente Claude Code passando JSON via stdin
  if command -v jq >/dev/null 2>&1; then
    INPUT=$(cat 2>/dev/null || true)
    if [ -n "$INPUT" ]; then
      FILE=$(echo "$INPUT" | jq -r '.tool_input.file_path // empty' 2>/dev/null || true)
    fi
  else
    echo "⚠️  jq não está instalado. Hook do Claude Code não pode extrair file_path do stdin."
    echo "   Instale com: winget install jqlang.jq (Windows) ou apt install jq (Linux)"
    exit 0  # Não bloqueia, apenas avisa
  fi
fi

# Sem arquivo → encerra silenciosamente
[ -z "$FILE" ] && exit 0

# Normaliza path: pode vir absoluto do Claude Code
# Converte para path relativo ao repo se aplicável
REPO_ROOT=$(git rev-parse --show-toplevel 2>/dev/null || pwd)
case "$FILE" in
  "$REPO_ROOT"/*) FILE="${FILE#$REPO_ROOT/}" ;;
esac

# Só valida .md dentro de docs/ (qualquer profundidade)
[[ "$FILE" =~ ^docs/.+\.md$ ]] || exit 0

# Pula README.md (índices não precisam de frontmatter)
[[ "$(basename "$FILE")" == "README.md" ]] && exit 0

# Arquivo deve existir
[ -f "$FILE" ] || exit 0

# Extrai APENAS o frontmatter (entre os dois ---) — não confunde com conteúdo
FRONTMATTER=$(awk '/^---$/{c++; if(c==2) exit; next} c==1' "$FILE")

if [ -z "$FRONTMATTER" ]; then
  echo "❌ $FILE — frontmatter YAML ausente. Adicione bloco --- ... --- no topo."
  exit 1
fi

# Valida campos DENTRO do frontmatter
MISSING=()
echo "$FRONTMATTER" | grep -q "^title:[[:space:]]*\S"   || MISSING+=("title")
echo "$FRONTMATTER" | grep -q "^date:[[:space:]]*\S"    || MISSING+=("date")
echo "$FRONTMATTER" | grep -q "^status:[[:space:]]*\S"  || MISSING+=("status")
echo "$FRONTMATTER" | grep -q "^authors:[[:space:]]*\S" || MISSING+=("authors")

if [ ${#MISSING[@]} -gt 0 ]; then
  echo "❌ Header inválido em $FILE — campos faltando ou vazios: ${MISSING[*]}"
  echo "   Formato esperado:"
  echo "   ---"
  echo "   title: ..."
  echo "   date: YYYY-MM-DD"
  echo "   status: 🟢 Vivo"
  echo "   authors: nome"
  echo "   ---"
  exit 1
fi

echo "✅ Header válido: $FILE"
exit 0
