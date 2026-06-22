#!/usr/bin/env bash
# package-release.sh — gera o bundle ÚNICO de release pra VM (dev box).
#
# Produz trl-release-v<versão>.zip contendo:
#   trl-items-management/   (viewer; sem items.json nem scripts de teste — regerados na VM)
#   TRLTraderPrices/         (DLL do mod + .pdb)
#   update-vm.ps1            (o updater de 1 comando da VM)
#
# A versão é lida do <Version> do csproj do mod. O viewer vem de `git archive HEAD`
# (só versionado) — COMMITE antes de empacotar pra o bundle refletir o que está no git.
#
# Uso:   bash tools/trl-items-management/scripts/package-release.sh [OUTDIR]
#   OUTDIR padrão: <repo>/dist  (passe D:/SPT/_vm-deploy p/ jogar direto no staging)
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT="$(cd "$SCRIPT_DIR/../../.." && pwd)"
TOOL="$ROOT/tools/trl-items-management"
MOD="$ROOT/mods/TRLTraderPrices"
OUTDIR="${1:-$ROOT/dist}"

# /c/Repos/... -> c:/Repos/... (powershell aceita barra normal)
winpath() { echo "$1" | sed 's|^/\([a-z]\)/|\1:/|'; }

VER="$(grep -oE '<Version>[0-9][0-9.]*</Version>' "$MOD/modded/Server/TRLTraderPrices.csproj" | grep -oE '[0-9][0-9.]*' | head -1)"
[[ -n "$VER" ]] || { echo "ERRO: não achei <Version> no csproj"; exit 1; }
echo "→ versão (csproj): $VER"

# avisa se há mudanças não commitadas no viewer (o git archive não as inclui)
if ! git -C "$ROOT" diff --quiet -- "$TOOL" || ! git -C "$ROOT" diff --cached --quiet -- "$TOOL"; then
  echo "  AVISO: há mudanças do viewer NÃO commitadas — o bundle usa HEAD (git archive). Commite antes."
fi

# 1) build da DLL (build-only: SPT_PATH inexistente => não copia pro install, não trava nada)
echo "→ buildando a DLL..."
bash "$ROOT/.agents/scripts/compile-mod.sh" TRLTraderPrices --spt-path /nonexistent-build-only >/dev/null
DLL="$MOD/builds/server/TRLTraderPrices.dll"
[[ -f "$DLL" ]] || { echo "ERRO: build falhou ($DLL ausente)"; exit 1; }
DLLVER="$(powershell.exe -NoProfile -Command "[Diagnostics.FileVersionInfo]::GetVersionInfo('$(winpath "$DLL")').FileVersion" 2>/dev/null | tr -d '\r')"
echo "  DLL versão: $DLLVER"

# 2) staging do bundle
STAGE="$OUTDIR/.stage-v$VER"
BUN="$STAGE/trl-release-v$VER"
rm -rf "$STAGE"; mkdir -p "$BUN/trl-items-management" "$BUN/TRLTraderPrices"
git -C "$ROOT" archive --format=tar HEAD:tools/trl-items-management | tar -x -C "$BUN/trl-items-management"
( cd "$BUN/trl-items-management" && rm -f data/items.json scripts/*smoke*.js scripts/action0-*.js scripts/verify-trader-*.js )
cp "$DLL" "$BUN/TRLTraderPrices/"
cp "$MOD/builds/server/TRLTraderPrices.pdb" "$BUN/TRLTraderPrices/" 2>/dev/null || true
cp "$TOOL/scripts/update-vm.ps1" "$BUN/"

# 3) zip (Compress-Archive — sem depender de binário zip no Git Bash)
mkdir -p "$OUTDIR"
OUT="$OUTDIR/trl-release-v$VER.zip"
rm -f "$OUT"
powershell.exe -NoProfile -Command "Compress-Archive -Path '$(winpath "$BUN")' -DestinationPath '$(winpath "$OUT")' -Force" >/dev/null
rm -rf "$STAGE"

SIZE="$(du -h "$OUT" | cut -f1)"
echo "✓ bundle: $OUT ($SIZE)"
echo "  conteúdo: trl-release-v$VER/{trl-items-management, TRLTraderPrices, update-vm.ps1}"
echo "  na VM: extraia e rode  .\\update-vm.ps1"
