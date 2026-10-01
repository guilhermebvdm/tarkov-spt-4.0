#!/usr/bin/env bash
# verify-port.sh — offline checks of the ORBIT 2.1 port to SPT 4.0. Nothing is written to the SPT install.
#
#   1. Harmony patches: runs every patch Enable() of builds/client/ORBIT.dll (and Orbit.Fika.dll) against the
#      real game assemblies of the install and checks target resolution, parameter binding and transpilers.
#   2. Type names compared as text: every 4.1 class name used as a string has a row in Spt40TypeNames.cs.
#
# Uso: bash mods/ORBIT-2.1/scripts/verify-port.sh [--spt-path <path>]
# Requer um build prévio: /compile-mod ORBIT-2.1 --no-install
# Código de saída: 0 = tudo passou, 1 = alguma verificação falhou.

set -uo pipefail

HERE="$(cd "$(dirname "$0")" && pwd)"
MOD="$(cd "$HERE/.." && pwd)"
ROOT="$(cd "$MOD/../.." && pwd)"

SPT_PATH="${SPT_PATH:-}"
while [[ $# -gt 0 ]]; do
  case "$1" in
    --spt-path) SPT_PATH="${2:-}"; shift 2 ;;
    -h|--help) sed -n '2,11p' "$0" | sed 's|^# \{0,1\}||'; exit 0 ;;
    *) echo "Erro: argumento desconhecido: $1" >&2; exit 1 ;;
  esac
done
if [[ -z "$SPT_PATH" && -f "$ROOT/.spt-path" ]]; then
  SPT_PATH="$(grep -E '^SPT_PATH=' "$ROOT/.spt-path" | tail -1 | cut -d= -f2- | tr -d '\r')"
fi
SPT_PATH="${SPT_PATH:-D:/SPT}"

BUILD="$MOD/builds/client"
[[ -f "$BUILD/ORBIT.dll" ]] || { echo "Erro: $BUILD/ORBIT.dll não existe — rode /compile-mod ORBIT-2.1 --no-install antes" >&2; exit 1; }
[[ -d "$SPT_PATH" ]] || { echo "Erro: SPT_PATH não existe: $SPT_PATH" >&2; exit 1; }

status=0

echo "=== 1/2 Patches Harmony contra o jogo em $SPT_PATH ==="
SPT_PATH="$SPT_PATH" dotnet build "$HERE/patch-dryrun/PatchDryRun.csproj" -c Release --nologo -v q >/dev/null \
  || { echo "Erro: não compilou scripts/patch-dryrun" >&2; exit 1; }
"$HERE/patch-dryrun/bin/Release/PatchDryRun.exe" "$(cygpath -w "$SPT_PATH" 2>/dev/null || echo "$SPT_PATH")" \
  "$(cygpath -w "$BUILD" 2>/dev/null || echo "$BUILD")" || status=1

echo
echo "=== 2/2 Nomes de tipo comparados como texto ==="
python "$HERE/check-type-name-literals.py" || status=1

echo
[[ $status -eq 0 ]] && echo "verify-port: TUDO PASSOU" || echo "verify-port: FALHOU"
exit $status
