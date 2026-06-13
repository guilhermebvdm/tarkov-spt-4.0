#!/usr/bin/env bash
# sync-classes.sh — bring CustomClasses config edits from the SPT install back into the repo.
# Direction: INSTALL → REPO. Covers config/classes/**/*.json[c] and root-level config/*.json[c]
# (e.g. hidden-editions.jsonc). Also propagates DELETIONS: a classes/ file that exists only in
# the repo was deleted in the install (web editor) and is removed from the repo too (CR-EP-02).
# Shows a per-file diff preview and asks for confirmation.
# Usage: sync-classes.sh [--spt-path <path>] [--dry-run] [--yes]
#   --spt-path <path>  SPT install root (default: env SPT_PATH or D:/SPT — same as compile-mod.sh)
#   --dry-run          only show what would change; copy nothing
#   --yes              skip the confirmation prompt (required for non-interactive runs)
set -euo pipefail

SPT_PATH="${SPT_PATH:-D:/SPT}"
DRY_RUN=0
ASSUME_YES=0

while [[ $# -gt 0 ]]; do
  case "$1" in
    --spt-path) SPT_PATH="${2:-}"; shift 2 ;;
    --dry-run)  DRY_RUN=1; shift ;;
    --yes)      ASSUME_YES=1; shift ;;
    -h|--help)  sed -n '2,9p' "$0" | sed 's|^# \{0,1\}||'; exit 0 ;;
    *) echo "Error: unknown argument: $1" >&2; exit 1 ;;
  esac
done

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
REPO_CONFIG="$(cd "$SCRIPT_DIR/../modded/Server/config" && pwd)"
INSTALL_CONFIG="$SPT_PATH/SPT/user/mods/CustomClasses/config"

[[ -d "$INSTALL_CONFIG" ]] || { echo "Error: install config not found: $INSTALL_CONFIG" >&2; exit 1; }

# CR2-EP-03: a config/ without classes/ means a foreign/broken install — treating every repo
# class as "deleted in install" would mass-delete the repo (catastrophic with --yes). Abort.
[[ -d "$INSTALL_CONFIG/classes" ]] || {
  echo "Error: install has no classes/ folder ($INSTALL_CONFIG/classes) — wrong SPT path?" >&2
  echo "       Aborting: no deletions proposed. Run /compile-mod first if this is a fresh install." >&2
  exit 1
}

# Same normalization as compile-mod.sh's guard: ignore CRLF/LF and trailing whitespace.
normalize_text() { sed -e 's/\r$//' -e 's/[ \t]*$//' "$1"; }
files_equal_normalized() { cmp -s <(normalize_text "$1") <(normalize_text "$2"); }

# CR-EP-09: root-level covers BOTH extensions (*.json + *.jsonc), same as classes/.
list_config_files() {  # $1 = config dir → relative paths of class jsons + root-level *.json[c]
  local dir="$1"
  { if [[ -d "$dir/classes" ]]; then
      find "$dir/classes" -type f \( -name '*.json' -o -name '*.jsonc' \)
    fi
    find "$dir" -maxdepth 1 -type f \( -name '*.json' -o -name '*.jsonc' \)
  } | sed "s|^$dir/||" | sort
}

CHANGED=()
NEW=()
while IFS= read -r rel; do
  if [[ -z "$rel" ]]; then continue; fi
  if [[ ! -f "$REPO_CONFIG/$rel" ]]; then
    NEW+=("$rel")
  elif ! files_equal_normalized "$INSTALL_CONFIG/$rel" "$REPO_CONFIG/$rel"; then
    CHANGED+=("$rel")
  fi
done < <(list_config_files "$INSTALL_CONFIG")

# CR-EP-02: FIRST-LEVEL classes/ files that exist only in the REPO were deleted in the install
# (the web editor deletes only there) — propagate the deletion by removing the repo copy.
# CR2-EP-02: the editor/boot loader only manage classes/<file> directly — files in SUBFOLDERS
# (classes/_docs/ drafts etc.) and root-level files are never deletable through the editor, so
# they are left alone (a repo-only draft is a new file, not a deletion).
DELETED=()
while IFS= read -r rel; do
  if [[ -z "$rel" ]]; then continue; fi
  case "$rel" in
    classes/*/*) ;;   # subfolder (draft) — out of the editor's scope, never a deletion
    classes/*)
      if [[ ! -f "$INSTALL_CONFIG/$rel" ]]; then DELETED+=("$rel"); fi ;;
  esac
done < <(list_config_files "$REPO_CONFIG")

TOTAL=$(( ${#CHANGED[@]} + ${#NEW[@]} + ${#DELETED[@]} ))
if [[ $TOTAL -eq 0 ]]; then
  echo "✓ Repo config already in sync with the install — nothing to do."
  exit 0
fi

echo "→ Install: $INSTALL_CONFIG"
echo "→ Repo:    $REPO_CONFIG"
echo "→ $TOTAL file(s) to sync install → repo (${#CHANGED[@]} changed, ${#NEW[@]} new, ${#DELETED[@]} deleted)"

for rel in ${CHANGED[@]+"${CHANGED[@]}"}; do
  echo
  echo "── CHANGED: $rel (-repo / +install) ──"
  diff -u --label "repo/$rel" --label "install/$rel" \
    "$REPO_CONFIG/$rel" "$INSTALL_CONFIG/$rel" | head -30 || true
done
for rel in ${NEW[@]+"${NEW[@]}"}; do
  echo
  echo "── NEW (not in repo yet): $rel ──"
done
for rel in ${DELETED[@]+"${DELETED[@]}"}; do
  echo
  echo "── DELETED in install (repo copy will be removed): $rel ──"
done
echo

# Guard against silently clobbering uncommitted manual edits in the repo
# (see memory: feedback_serve_inventory_clobber).
if command -v git >/dev/null 2>&1 \
   && git -C "$REPO_CONFIG" rev-parse --is-inside-work-tree >/dev/null 2>&1; then
  DIRTY="$(git -C "$REPO_CONFIG" status --porcelain -- . 2>/dev/null || true)"
  if [[ -n "$DIRTY" ]]; then
    echo "⚠ Repo config dir has UNCOMMITTED changes — syncing will overwrite them:"
    echo "$DIRTY" | sed 's/^/    /'
    echo "  Commit/stash them first if they must be preserved."
    echo
  fi
fi

if [[ "$DRY_RUN" == "1" ]]; then
  echo "(dry-run: nothing copied)"
  exit 0
fi

if [[ "$ASSUME_YES" != "1" ]]; then
  if [[ -t 0 ]]; then
    read -r -p "Apply these $TOTAL change(s) (copy/delete) install → repo? [y/N] " ans
    if [[ ! "$ans" =~ ^[Yy]([Ee][Ss])?$ ]]; then
      echo "Aborted — nothing copied."
      exit 1
    fi
  else
    echo "Error: divergence found but running non-interactively without --yes." >&2
    echo "Re-run with --yes to apply, or --dry-run to inspect first." >&2
    exit 2
  fi
fi

n=0
for rel in ${CHANGED[@]+"${CHANGED[@]}"} ${NEW[@]+"${NEW[@]}"}; do
  mkdir -p "$(dirname "$REPO_CONFIG/$rel")"
  cp -f "$INSTALL_CONFIG/$rel" "$REPO_CONFIG/$rel"
  echo "  ✓ $rel → repo"
  n=$((n+1))
done
for rel in ${DELETED[@]+"${DELETED[@]}"}; do
  rm -f "$REPO_CONFIG/$rel"
  echo "  ✗ $rel removed from repo (deleted in install)"
  n=$((n+1))
done

echo
echo "✓ Synced $n file(s) install → repo. Review with 'git diff'/'git status' and commit."
