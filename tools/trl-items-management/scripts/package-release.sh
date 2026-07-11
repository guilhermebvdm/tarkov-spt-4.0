#!/usr/bin/env bash
# package-release.sh — gera o bundle ÚNICO de release pra VM (dev box).
#
# Produz trl-release-v<versão>.zip contendo:
#   TRL-ItemsManagement/server/   (DLL+pdb+wwwroot do mod, SEM config/ nem data/ — user-data)
#   TRL-ItemsManagement/client/   (DLL+pdb do mod client)
#   trl-items-management-pipeline/  (só os scripts Node que o mod chama via Process.Start em
#                                    runtime — sem viewer/, sem items.json, sem testes/smoke)
#   update-vm.ps1                   (o updater de 1 comando da VM)
#
# A versão é lida do <Version> do csproj do server. O conteúdo do bundle vem do INSTALL LOCAL
# (D:\SPT por padrão, via .spt-path) — reaproveita o compile-mod.sh pra filtrar só as DLLs
# próprias do mod e copiar o wwwroot certo, em vez de duplicar essa lógica aqui.
#
# Uso:   bash tools/trl-items-management/scripts/package-release.sh [OUTDIR]
#   OUTDIR padrão: <repo>/dist  (passe D:/SPT/_vm-deploy p/ jogar direto no staging)
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT="$(cd "$SCRIPT_DIR/../../.." && pwd)"
TOOL="$ROOT/tools/trl-items-management"
MOD="$ROOT/mods/TRL-ItemsManagement"
OUTDIR="${1:-$ROOT/dist}"

# /c/Repos/... -> c:/Repos/... (powershell aceita barra normal)
winpath() { echo "$1" | sed 's|^/\([a-z]\)/|\1:/|'; }

VER="$(grep -oE '<Version>[0-9][0-9.]*</Version>' "$MOD/modded/Server/TRLItemsManagement.csproj" | grep -oE '[0-9][0-9.]*' | head -1)"
[[ -n "$VER" ]] || { echo "ERRO: não achei <Version> no csproj do server"; exit 1; }
echo "→ versão (csproj): $VER"

# avisa se há mudanças não commitadas no mod/pipeline (o bundle reflete o working tree, não HEAD)
if ! git -C "$ROOT" diff --quiet -- "$MOD" "$TOOL/scripts" "$TOOL/data" || ! git -C "$ROOT" diff --cached --quiet -- "$MOD" "$TOOL/scripts" "$TOOL/data"; then
  echo "  AVISO: há mudanças não commitadas no mod ou no pipeline — o bundle reflete o working tree atual, não HEAD."
fi

# 1) build + instala no SPT local — fonte de verdade do conteúdo do bundle (evita duplicar a
#    lógica de filtro de DLLs próprias / cópia de wwwroot que o compile-mod.sh já resolve).
echo "→ compilando e instalando localmente (compile-mod.sh)..."
bash "$ROOT/.agents/scripts/compile-mod.sh" TRL-ItemsManagement >/dev/null

# resolve o mesmo SPT_PATH que o compile-mod.sh usou (env > .spt-path > default D:/SPT)
SPT_INSTALL="${SPT_PATH:-}"
if [[ -z "$SPT_INSTALL" && -f "$ROOT/.spt-path" ]]; then
  SPT_INSTALL="$(grep -m1 '^SPT_PATH=' "$ROOT/.spt-path" | cut -d= -f2-)"
fi
SPT_INSTALL="${SPT_INSTALL:-D:/SPT}"

SERVER_SRC="$SPT_INSTALL/SPT/user/mods/TRL-ItemsManagement"
CLIENT_SRC="$SPT_INSTALL/BepInEx/plugins/TRL-ItemsManagement"
[[ -f "$SERVER_SRC/TRLItemsManagement-Server.dll" ]] || { echo "ERRO: build/instalação local falhou (ausente: $SERVER_SRC/TRLItemsManagement-Server.dll)"; exit 1; }
[[ -f "$CLIENT_SRC/TRLItemsManagement-Client.dll" ]] || { echo "ERRO: build/instalação local falhou (ausente: $CLIENT_SRC/TRLItemsManagement-Client.dll)"; exit 1; }

# 2) staging do bundle
STAGE="$OUTDIR/.stage-v$VER"
BUN="$STAGE/trl-release-v$VER"
rm -rf "$STAGE"
mkdir -p "$BUN/TRL-ItemsManagement/server" "$BUN/TRL-ItemsManagement/client" \
         "$BUN/trl-items-management-pipeline/scripts" "$BUN/trl-items-management-pipeline/data"

# server: tudo MENOS config/, data/ (user-data — nunca vai no bundle) e *.bak (backup local
# de uma DLL sobrescrita no install de dev, sem sentido no bundle)
cp -r "$SERVER_SRC/." "$BUN/TRL-ItemsManagement/server/"
rm -rf "$BUN/TRL-ItemsManagement/server/config" "$BUN/TRL-ItemsManagement/server/data"
rm -f "$BUN/TRL-ItemsManagement/server/"*.bak

# client: DLL+pdb (sem user-data nesse lado; *.bak é backup local de um install anterior)
cp -r "$CLIENT_SRC/." "$BUN/TRL-ItemsManagement/client/"
rm -f "$BUN/TRL-ItemsManagement/client/"*.bak

# pipeline Node: só os scripts que o mod invoca via Process.Start em runtime (ItemRefreshController/
# CatalogRebuildController) + dados-semente pro catálogo (sem items.json, regenerado na VM) — sem
# viewer/ (aposentado), sem scripts de teste/diagnóstico (action0-*, smoke*, verify-trader-*).
for f in load-env.js load-spt.js normalize.js fetch-tarkov-dev.js fetch-tarkov-market.js refresh-item.js; do
  cp "$TOOL/scripts/$f" "$BUN/trl-items-management-pipeline/scripts/"
done
for f in categories.json hideout-crafts.json meta.json traders.json handbook-prices-log.json; do
  [[ -f "$TOOL/data/$f" ]] && cp "$TOOL/data/$f" "$BUN/trl-items-management-pipeline/data/"
done

cp "$TOOL/scripts/update-vm.ps1" "$BUN/"

# 3) zip (Compress-Archive — sem depender de binário zip no Git Bash)
mkdir -p "$OUTDIR"
OUT="$OUTDIR/trl-release-v$VER.zip"
rm -f "$OUT"
powershell.exe -NoProfile -Command "Compress-Archive -Path '$(winpath "$BUN")' -DestinationPath '$(winpath "$OUT")' -Force" >/dev/null
rm -rf "$STAGE"

SIZE="$(du -h "$OUT" | cut -f1)"
echo "✓ bundle: $OUT ($SIZE)"
echo "  conteúdo: trl-release-v$VER/{TRL-ItemsManagement/{server,client}, trl-items-management-pipeline, update-vm.ps1}"
echo "  na VM: extraia e rode  .\\update-vm.ps1"
echo "  1ª vez na VM (setup antigo ainda presente): o próprio update-vm.ps1 migra TRLTraderPrices + retira o viewer Node."
