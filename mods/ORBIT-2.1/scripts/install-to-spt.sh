#!/usr/bin/env bash
# install-to-spt.sh — instala o ORBIT 2.1 (port SPT 4.0) no install local do SPT, ou desfaz a instalação.
#
# O ORBIT 1.2.1 e o 2.1 têm o mesmo identificador de plugin (com.chazut.orbit): só um pode ficar em
# BepInEx/plugins. Este script move a pasta do 1.2.1 para fora de plugins antes de instalar o 2.1, e a traz
# de volta no --rollback. Nada é apagado.
#
# Uso:
#   bash mods/ORBIT-2.1/scripts/install-to-spt.sh              instala cliente, addon Fika e servidor
#   bash mods/ORBIT-2.1/scripts/install-to-spt.sh --rollback   remove o 2.1 e restaura o 1.2.1
#   [--spt-path <path>]                                        sobrescreve SPT_PATH / .spt-path
#
# O jogo e o servidor SPT precisam estar fechados (as DLLs ficam travadas enquanto rodam).

set -euo pipefail

HERE="$(cd "$(dirname "$0")" && pwd)"
MOD="$(cd "$HERE/.." && pwd)"
ROOT="$(cd "$MOD/../.." && pwd)"
MOD_NAME="$(basename "$MOD")"

ROLLBACK=0
SPT_PATH="${SPT_PATH:-}"
while [[ $# -gt 0 ]]; do
  case "$1" in
    --rollback) ROLLBACK=1; shift ;;
    --spt-path) SPT_PATH="${2:-}"; shift 2 ;;
    -h|--help) sed -n '2,14p' "$0" | sed 's|^# \{0,1\}||'; exit 0 ;;
    *) echo "Erro: argumento desconhecido: $1" >&2; exit 1 ;;
  esac
done
if [[ -z "$SPT_PATH" && -f "$ROOT/.spt-path" ]]; then
  SPT_PATH="$(grep -E '^SPT_PATH=' "$ROOT/.spt-path" | tail -1 | cut -d= -f2- | tr -d '\r')"
fi
SPT_PATH="${SPT_PATH:-D:/SPT}"
[[ -d "$SPT_PATH/BepInEx/plugins" ]] || { echo "Erro: $SPT_PATH não parece um install do SPT" >&2; exit 1; }

if tasklist 2>/dev/null | grep -qiE 'EscapeFromTarkov\.exe|SPT\.Server\.exe'; then
  echo "Erro: feche o jogo e o servidor SPT antes (EscapeFromTarkov.exe ou SPT.Server.exe em execução)." >&2
  exit 1
fi

PLUGINS="$SPT_PATH/BepInEx/plugins"
OLD_CLIENT="$PLUGINS/ORBIT"                               # ORBIT 1.2.1
PARKED="$SPT_PATH/BepInEx/plugins-disabled/ORBIT-1.2.1"   # fora de plugins: o BepInEx não carrega
NEW_CLIENT="$PLUGINS/$MOD_NAME"
NEW_SERVER="$SPT_PATH/SPT/user/mods/$MOD_NAME"

if [[ "$ROLLBACK" == "1" ]]; then
  if [[ -d "$NEW_CLIENT" ]]; then
    mkdir -p "$SPT_PATH/BepInEx/plugins-disabled"
    rm -rf "$SPT_PATH/BepInEx/plugins-disabled/$MOD_NAME"
    mv "$NEW_CLIENT" "$SPT_PATH/BepInEx/plugins-disabled/$MOD_NAME"
    echo "✓ cliente 2.1 movido para BepInEx/plugins-disabled/$MOD_NAME"
  fi
  if [[ -d "$NEW_SERVER" ]]; then
    mkdir -p "$SPT_PATH/SPT/user/mods-disabled"
    rm -rf "$SPT_PATH/SPT/user/mods-disabled/$MOD_NAME"
    mv "$NEW_SERVER" "$SPT_PATH/SPT/user/mods-disabled/$MOD_NAME"
    echo "✓ servidor 2.1 movido para SPT/user/mods-disabled/$MOD_NAME (config, presets e zonas vão junto)"
  fi
  if [[ -d "$PARKED" && ! -e "$OLD_CLIENT" ]]; then
    mv "$PARKED" "$OLD_CLIENT"
    echo "✓ ORBIT 1.2.1 restaurado em BepInEx/plugins/ORBIT"
  elif [[ -e "$OLD_CLIENT" ]]; then
    echo "i ORBIT 1.2.1 já está em BepInEx/plugins/ORBIT"
  else
    echo "! não encontrei o ORBIT 1.2.1 guardado em $PARKED" >&2
  fi
  exit 0
fi

[[ -f "$MOD/builds/client/ORBIT.dll" && -f "$MOD/builds/server/Orbit.Server.dll" ]] \
  || { echo "Erro: build ausente — rode /compile-mod $MOD_NAME --no-install antes" >&2; exit 1; }

if [[ -d "$OLD_CLIENT" ]]; then
  [[ ! -e "$PARKED" ]] || { echo "Erro: $PARKED já existe; remova ou renomeie antes de instalar" >&2; exit 1; }
  mkdir -p "$(dirname "$PARKED")"
  mv "$OLD_CLIENT" "$PARKED"
  echo "✓ ORBIT 1.2.1 movido para BepInEx/plugins-disabled/ORBIT-1.2.1 (mesmo GUID do 2.1)"
fi

install_dll() {  # $1 = origem, $2 = pasta destino
  mkdir -p "$2"
  cp -f "$1" "$2/"
  [[ -f "${1%.dll}.pdb" ]] && cp -f "${1%.dll}.pdb" "$2/" || true
  echo "  ✓ $(basename "$1") → $2"
}

echo "→ Cliente (reiniciar o JOGO para valer):"
install_dll "$MOD/builds/client/ORBIT.dll" "$NEW_CLIENT"
if [[ -f "$PLUGINS/Fika/Fika.Core.dll" ]]; then
  install_dll "$MOD/builds/client/Orbit.Fika.dll" "$NEW_CLIENT"
else
  echo "  i Fika não instalado: addon Orbit.Fika.dll não copiado"
fi
echo "→ Servidor (reiniciar o SPT.Server para valer):"
install_dll "$MOD/builds/server/Orbit.Server.dll" "$NEW_SERVER"

cat <<EOF

✓ ORBIT 2.1 instalado em $SPT_PATH
  Cliente:  BepInEx/plugins/$MOD_NAME/  (ORBIT.dll$( [[ -f "$PLUGINS/Fika/Fika.Core.dll" ]] && echo ', Orbit.Fika.dll'))
  Servidor: SPT/user/mods/$MOD_NAME/    (Orbit.Server.dll; o mod cria config.json, presets/, zones/ e addon/ ao lado)
  Painel:   https://127.0.0.1:6969/orbit depois de subir o servidor
  Desfazer: bash mods/$MOD_NAME/scripts/install-to-spt.sh --rollback
EOF
