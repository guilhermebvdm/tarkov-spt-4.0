#!/usr/bin/env bash
# package-release.sh — gera o zip de release do TRL-ImmersiveCombatMedicine (mod CLIENT-ONLY).
#
# Produz dist/trl-icm-release-v<versão>.zip contendo (espelhando a estrutura real de pastas do
# SPT, relativa à raiz do jogo):
#   BepInEx/plugins/TRL-ImmersiveCombatMedicine/   (DLL+pdb do mod client, já instalado em D:\SPT)
#
# Diferente do precedente tools/trl-items-management/scripts/package-release.sh (mod HÍBRIDO,
# client+server+pipeline Node): este mod não tem componente server nem pipeline externo — o
# bundle tem UM único diretório. Reaproveita o compile-mod.sh (build+install) em vez de duplicar
# essa lógica; a versão é lida do <Version> do único csproj do mod (fonte única — sem
# BepInPlugin/csproj a sincronizar entre 2 projetos, diferente do precedente).
#
# Uso:   bash mods/TRL-ImmersiveCombatMedicine/scripts/package-release.sh [OUTDIR]
#   OUTDIR padrão: <repo>/dist
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT="$(cd "$SCRIPT_DIR/../../.." && pwd)"
MOD_NAME="TRL-ImmersiveCombatMedicine"
ASSEMBLY="TRLImmersiveCombatMedicine"
MOD="$ROOT/mods/$MOD_NAME"
OUTDIR="${1:-$ROOT/dist}"

# /c/Repos/... -> c:/Repos/... (powershell aceita barra normal)
winpath() { echo "$1" | sed 's|^/\([a-z]\)/|\1:/|'; }

VER="$(grep -oE '<Version>[0-9][0-9.]*</Version>' "$MOD/modded/$MOD_NAME.csproj" | grep -oE '[0-9][0-9.]*' | head -1 || true)"
# PA-02-03 (review técnica 02): `|| true` evita que `set -e`+`pipefail` aborte esta linha
# SILENCIOSAMENTE antes do guard abaixo rodar, caso o formato do <Version> mude no futuro
# (o precedente tools/trl-items-management/scripts/package-release.sh:35 tem o mesmo bug —
# não herdado aqui por ser script novo).
[[ -n "$VER" ]] || { echo "ERRO: não achei <Version> no csproj do mod"; exit 1; }
echo "→ versão (csproj): $VER"

# avisa se há mudanças não commitadas no mod (o bundle reflete o working tree, não HEAD) —
# mesmo guard do precedente: evita releasar um build sem commit rastreável.
if ! git -C "$ROOT" diff --quiet -- "$MOD" || ! git -C "$ROOT" diff --cached --quiet -- "$MOD"; then
  echo "  AVISO: há mudanças não commitadas no mod — o bundle reflete o working tree atual, não HEAD."
fi

# 1) build + instala no SPT local — fonte de verdade do conteúdo do bundle (evita duplicar a
#    lógica de filtro de DLL própria que o compile-mod.sh já resolve).
echo "→ compilando e instalando localmente (compile-mod.sh)..."
bash "$ROOT/.agents/scripts/compile-mod.sh" "$MOD_NAME" >/dev/null

# resolve o mesmo SPT_PATH que o compile-mod.sh usou (env > .spt-path > default D:/SPT).
SPT_INSTALL="${SPT_PATH:-}"
if [[ -z "$SPT_INSTALL" && -f "$ROOT/.spt-path" ]]; then
  SPT_INSTALL="$(grep -m1 '^SPT_PATH=' "$ROOT/.spt-path" | cut -d= -f2- | tr -d '\r')"
fi
SPT_INSTALL="${SPT_INSTALL:-D:/SPT}"

CLIENT_SRC="$SPT_INSTALL/BepInEx/plugins/$MOD_NAME"
[[ -f "$CLIENT_SRC/$ASSEMBLY.dll" ]] || { echo "ERRO: build/instalação local falhou (ausente: $CLIENT_SRC/$ASSEMBLY.dll)"; exit 1; }

# 2) staging do bundle — espelha <GameRoot>\BepInEx\plugins\TRL-ImmersiveCombatMedicine\
STAGE="$OUTDIR/.stage-v$VER"
BUN="$STAGE/trl-icm-release-v$VER"
rm -rf "$STAGE"
mkdir -p "$BUN/BepInEx/plugins/$MOD_NAME"

# DLL+pdb apenas (sem config/user-data). Nota: BepInEx/plugins/<mod>/Silhueta/ (assets PNG
# opcionais carregados em runtime por ImageLoader.Init, ver Helpers/ImageLoader.cs) NÃO é
# rastreado no repo e fica FORA deste bundle por decisão explícita — é asset local do usuário,
# não artefato do mod; a spec funcional deste item só cobre "o DLL do client".
cp -f "$CLIENT_SRC/$ASSEMBLY.dll" "$BUN/BepInEx/plugins/$MOD_NAME/"
[[ -f "$CLIENT_SRC/$ASSEMBLY.pdb" ]] && cp -f "$CLIENT_SRC/$ASSEMBLY.pdb" "$BUN/BepInEx/plugins/$MOD_NAME/"

# 3) zip (Compress-Archive — sem depender de binário zip no Git Bash)
mkdir -p "$OUTDIR"
OUT="$OUTDIR/trl-icm-release-v$VER.zip"
rm -f "$OUT"
powershell.exe -NoProfile -Command "Compress-Archive -Path '$(winpath "$BUN")' -DestinationPath '$(winpath "$OUT")' -Force" >/dev/null
rm -rf "$STAGE"

SIZE="$(du -h "$OUT" | cut -f1)"
echo "✓ bundle: $OUT ($SIZE)"
echo "  conteúdo: trl-icm-release-v$VER/BepInEx/plugins/$MOD_NAME/{$ASSEMBLY.dll,$ASSEMBLY.pdb}"
echo "  instalação: extraia e mescle BepInEx/plugins/ na raiz do jogo (client-only — sem componente server)."
