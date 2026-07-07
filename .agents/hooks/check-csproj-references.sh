#!/bin/bash
# Pre-commit gate: warn when a staged *.csproj has a <HintPath> pointing to an
# absolute Windows path (drive letter) instead of the References\ folder that
# /compile-mod resolves automatically from .spt-path (see AGENTS.md § Workspace,
# csharp-mod-best-practices §9). WARN-ONLY (exit 0) — legacy vendored mods
# (original/, frozen SPT 3.x) keep their own machine path and are not being
# actively migrated.
#
# Root cause: an agent nearly hand-copied game DLLs into a mod's References/
# folder instead of running /compile-mod — nothing enforced the convention,
# only prose. 8 pre-existing mods had the same hardcoded-path mistake
# (sessão 2026-07-07: TRL-PvpMode, UnderFire, TarkovIRL x2, TRL-ImmersiveScopes,
# StanceSync, TarkovRedLine4.0 client x2).

set -eu

[ "${GIT_AMEND:-}" = "1" ] && exit 0

STAGED=$(git diff --cached --name-only --diff-filter=ACM -- '*.csproj' || true)
[ -z "$STAGED" ] && exit 0

WARNED=0
while IFS= read -r FILE; do
  [ -n "$FILE" ] || continue
  # Read the STAGED blob, not the working tree — a fix applied only in the
  # working tree must not fool the gate (memory-curation lesson, sessão 5b).
  BAD=$(git show ":$FILE" 2>/dev/null | grep -nE '<HintPath>[A-Za-z]:\\' || true)
  if [ -n "$BAD" ]; then
    echo "⚠ $FILE — <HintPath> com caminho absoluto de máquina (deveria ser References\\<dll>, resolvido por /compile-mod via .spt-path):" >&2
    echo "$BAD" | sed 's/^/      /' >&2
    WARNED=1
  fi
done <<< "$STAGED"

[ "$WARNED" = "1" ] && echo "  (aviso — não bloqueia; mods vendorizados em original/ ou SPT 3.x legado podem manter path próprio)" >&2

exit 0
