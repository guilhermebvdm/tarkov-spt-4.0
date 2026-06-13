#!/bin/bash
# Pre-commit gate: warn when a staged */memory/sessions.md or memory/repo-sessions.md
# has a bullet under the top "Pendências" block WITHOUT a [P-N.M] ID. The IDs are the
# cross-ref + GC primitive (memory-curation §7); a bare bullet can never enter the
# date-keyed GC. WARN-ONLY (exit 0) so legacy migration can be staged incrementally.
#
# Findings D1-01/D5-04 from the harness value review.

set -eu

[ "${GIT_AMEND:-}" = "1" ] && exit 0

STAGED=$(git diff --cached --name-only --diff-filter=ACM \
  | grep -E '(^memory/repo-sessions\.md$|/memory/sessions\.md$)' || true)
[ -z "$STAGED" ] && exit 0

WARNED=0
while IFS= read -r FILE; do
  [ -f "$FILE" ] || continue
  # Extract the lines between the "Pendências" heading and the next "## " heading,
  # then flag top-level bullets that have no [P- id.
  # Varre SÓ o primeiro bloco "Pendências" (snapshot do topo) e para no próximo
  # heading de qualquer nível — assim blocos "(ARQUIVADO)" e menções históricas
  # dentro de entradas de sessão não geram falso positivo.
  # NB: git-bash GNU awk roda em locale de byte único — o 'ê' de "Pendências" são 2
  # bytes (0xC3 0xAA), então um '.' não casa. Usar .* entre 'end' e 'ncias'.
  BAD=$(awk '
    done { next }
    inblk && /^#/ { done=1; next }
    inblk && /^- / && $0 !~ /\[P-[0-9]/ { print }
    !inblk && /^## .*[Pp]end.*ncias/ { inblk=1 }
  ' "$FILE" || true)
  if [ -n "$BAD" ]; then
    echo "⚠ $FILE — bullet(s) de pendência sem [P-N.M] (memory-curation §7):" >&2
    echo "$BAD" | sed 's/^/      /' >&2
    WARNED=1
  fi
done <<< "$STAGED"

[ "$WARNED" = "1" ] && echo "  (aviso — não bloqueia; atribua ID retroativo [P-N.M] + (aberta YYYY-MM-DD) na próxima gravação)" >&2

exit 0
