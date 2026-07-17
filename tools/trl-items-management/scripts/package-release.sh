#!/usr/bin/env bash
# package-release.sh — gera o bundle ÚNICO de release pra VM (dev box).
#
# Produz trl-release-v<versão>.zip contendo (espelhando a estrutura real de pastas do SPT,
# relativa à raiz do jogo — não uma convenção própria "server/client"):
#   BepInEx/plugins/TRL-ItemsManagement/   (DLL+pdb do mod client)
#   SPT/user/mods/TRL-ItemsManagement/     (DLL+pdb+wwwroot do mod server, SEM config/ nem
#                                           data/ — user-data)
#   trl-items-management-pipeline/         (só os scripts Node que o mod chama via Process.Start
#                                           em runtime — sem viewer/, sem items.json, sem testes)
#   update-vm.ps1                          (o updater de 1 comando da VM)
#
# Os dois primeiros espelham exatamente <GameRoot>\BepInEx\plugins\... e <GameRoot>\SPT\user\
# mods\... — dá pra, em último caso, mesclar essas duas pastas direto na raiz do jogo na mão,
# sem depender do update-vm.ps1 (ele ainda é o jeito recomendado, por causa da migração e da
# preservação de config/dados).
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

# resolve o mesmo SPT_PATH que o compile-mod.sh usou (env > .spt-path > default D:/SPT).
# .Trim()/tr -d '\r': o arquivo é LF hoje, mas se algum dia for salvo por uma ferramenta Windows
# (Notepad etc.) e ganhar CRLF, um \r sobrando no path faz os checks de arquivo abaixo falharem
# com a mensagem genérica de "build falhou", escondendo a causa real.
SPT_INSTALL="${SPT_PATH:-}"
if [[ -z "$SPT_INSTALL" && -f "$ROOT/.spt-path" ]]; then
  SPT_INSTALL="$(grep -m1 '^SPT_PATH=' "$ROOT/.spt-path" | cut -d= -f2- | tr -d '\r')"
fi
SPT_INSTALL="${SPT_INSTALL:-D:/SPT}"

SERVER_SRC="$SPT_INSTALL/SPT/user/mods/TRL-ItemsManagement"
CLIENT_SRC="$SPT_INSTALL/BepInEx/plugins/TRL-ItemsManagement"
[[ -f "$SERVER_SRC/TRLItemsManagement-Server.dll" ]] || { echo "ERRO: build/instalação local falhou (ausente: $SERVER_SRC/TRLItemsManagement-Server.dll)"; exit 1; }
[[ -f "$CLIENT_SRC/TRLItemsManagement-Client.dll" ]] || { echo "ERRO: build/instalação local falhou (ausente: $CLIENT_SRC/TRLItemsManagement-Client.dll)"; exit 1; }
[[ -d "$SERVER_SRC/wwwroot" ]] || echo "  AVISO: $SERVER_SRC/wwwroot ausente — o bundle vai sair SEM a UI do mod."

# 2) staging do bundle — espelha <GameRoot>\BepInEx\plugins\... e <GameRoot>\SPT\user\mods\...
STAGE="$OUTDIR/.stage-v$VER"
BUN="$STAGE/trl-release-v$VER"
rm -rf "$STAGE"
mkdir -p "$BUN/BepInEx/plugins/TRL-ItemsManagement" \
         "$BUN/SPT/user/mods/TRL-ItemsManagement" \
         "$BUN/trl-items-management-pipeline/scripts" "$BUN/trl-items-management-pipeline/data"

# server: tudo MENOS config/, data/, logs/ (user-data — nunca vai no bundle; logs/ em
# particular é o audit.jsonl gerado em runtime pelo AuditLogService — bundlar o do install de
# DEV contaminaria a produção com entradas de teste) e *.bak (backup local de uma DLL
# sobrescrita no install de dev, sem sentido no bundle)
cp -r "$SERVER_SRC/." "$BUN/SPT/user/mods/TRL-ItemsManagement/"
rm -rf "$BUN/SPT/user/mods/TRL-ItemsManagement/config" "$BUN/SPT/user/mods/TRL-ItemsManagement/data" "$BUN/SPT/user/mods/TRL-ItemsManagement/logs"
rm -f "$BUN/SPT/user/mods/TRL-ItemsManagement/"*.bak

# client: DLL+pdb (sem user-data nesse lado; *.bak é backup local de um install anterior)
cp -r "$CLIENT_SRC/." "$BUN/BepInEx/plugins/TRL-ItemsManagement/"
rm -f "$BUN/BepInEx/plugins/TRL-ItemsManagement/"*.bak

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
echo "  conteúdo: trl-release-v$VER/{BepInEx/plugins/TRL-ItemsManagement, SPT/user/mods/TRL-ItemsManagement, trl-items-management-pipeline, update-vm.ps1}"
echo "  na VM: extraia e rode  .\\update-vm.ps1"
echo "  1ª vez na VM (setup antigo ainda presente): o próprio update-vm.ps1 migra TRLTraderPrices + retira o viewer Node."
