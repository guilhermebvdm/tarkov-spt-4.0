#!/bin/bash
# Pre-commit gate (HARD): block a commit that marks a backlog item 🟢 (Entregue)
# while one of its 06-fix-NN.md files still has an UNCHECKED in-raid validation box.
# Enforces AP-06 ("compila ≠ validado") — there is no legitimate reason to mark an
# item delivered with the in-raid box unticked. Only fires on fixes that use the
# fix.md.tmpl checklist signature, so legacy fixes (no checklist) are never blocked.
# Scoped: runs only when a mod-backlog.md or a 06-fix file is staged.
#
# Findings D4-03/D5-05 from the harness value review.

set -eu

[ "${GIT_AMEND:-}" = "1" ] && exit 0

TRIGGER=$(git diff --cached --name-only --diff-filter=ACMR \
  | grep -E '(mod-backlog\.md$|-06-fix-[0-9]+.*\.md$)' || true)
[ -z "$TRIGGER" ] && exit 0

VIOLATIONS=""

# For every mod backlog index, check its 🟢 items against their fix files.
for BACKLOG in mods/*/backlog/mod-backlog.md; do
  [ -f "$BACKLOG" ] || continue
  ITEM_DIR="$(dirname "$BACKLOG")"
  # Rows look like: | 004 | Title | ... | [004-slug/](./004-slug/) | 🟢 |
  # Extract item number of every row whose status cell is 🟢. Read the STAGED blob
  # (not the working tree) — the commit gate must judge what's being committed.
  GREEN_ITEMS=$(git show ":$BACKLOG" 2>/dev/null \
    | grep -E '^\|[[:space:]]*[0-9]{3}[[:space:]]*\|' \
    | grep '🟢' \
    | sed -E 's/^\|[[:space:]]*([0-9]{3}).*/\1/' || true)
  [ -z "$GREEN_ITEMS" ] && continue
  while IFS= read -r NNN; do
    [ -z "$NNN" ] && continue
    for FIX in "$ITEM_DIR/$NNN"-*/"$NNN"-*-06-fix-*.md; do
      [ -f "$FIX" ] || continue
      # Only fixes that carry the template checklist; unchecked in-raid box = violation.
      # Read the STAGED blob so a box ticked only in the working tree doesn't fool the gate.
      if git show ":$FIX" 2>/dev/null | grep -qE '^- \[ \] \*\*In-raid:'; then
        VIOLATIONS="$VIOLATIONS\n    • item $NNN (🟢) — $FIX tem caixa in-raid desmarcada"
      fi
    done
  done <<< "$GREEN_ITEMS"
done

if [ -n "$VIOLATIONS" ]; then
  echo "❌ AP-06: item marcado 🟢 'Entregue' com validação in-game pendente — commit abortado." >&2
  printf "%b\n" "$VIOLATIONS" >&2
  echo "  Valide in-game e marque a caixa, OU rebaixe o item para 🟡 no mod-backlog.md." >&2
  exit 1
fi

exit 0
