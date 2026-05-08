#!/usr/bin/env bash
# compile-mod.sh — compila um mod (client BepInEx ou server TypeScript) e instala em D:\SPT.
# Uso: compile-mod.sh <mod-name> [--spt-path <path>] [--flat]

set -euo pipefail

# ---------- args ----------
MOD=""
SPT_PATH="${SPT_PATH:-D:/SPT}"
FLAT_INSTALL=0

while [[ $# -gt 0 ]]; do
  case "$1" in
    --spt-path) SPT_PATH="${2:-}"; shift 2 ;;
    --flat)     FLAT_INSTALL=1; shift ;;
    -h|--help)
      sed -n '2,4p' "$0" | sed 's|^# \{0,1\}||'
      exit 0 ;;
    -*) echo "Erro: flag desconhecida: $1" >&2; exit 1 ;;
    *)
      if [[ -z "$MOD" ]]; then MOD="$1"
      else echo "Erro: argumento extra: $1" >&2; exit 1
      fi
      shift ;;
  esac
done

[[ -n "$MOD" ]] || { echo "Erro: <mod-name> é obrigatório" >&2; exit 1; }

ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
MOD_DIR="$ROOT/mods/$MOD"
MODDED="$MOD_DIR/modded"
BUILDS="$MOD_DIR/builds"

[[ -d "$MOD_DIR" ]] || { echo "Erro: mod não encontrado: $MOD_DIR" >&2; exit 1; }
[[ -d "$MODDED"  ]] || { echo "Erro: pasta modded ausente: $MODDED" >&2; exit 1; }
[[ -d "$SPT_PATH" ]] || { echo "Aviso: SPT_PATH não existe ($SPT_PATH) — vai apenas gerar builds/" >&2; }

# ---------- detect mod type ----------
CSPROJ="$(find "$MODDED" -maxdepth 2 -name '*.csproj' 2>/dev/null | head -1)"
PACKAGE_JSON="$MODDED/package.json"

MOD_TYPE=""
if [[ -n "$CSPROJ" ]]; then
  if grep -qiE 'BepInEx|Assembly-CSharp' "$CSPROJ"; then
    MOD_TYPE="client-csharp"
  else
    MOD_TYPE="server-csharp"
  fi
elif [[ -f "$PACKAGE_JSON" ]]; then
  MOD_TYPE="server-typescript"
else
  echo "Erro: tipo de mod não detectado em $MODDED (sem .csproj nem package.json)" >&2
  exit 1
fi

echo "→ Mod: $MOD"
echo "→ Tipo: $MOD_TYPE"
echo "→ SPT path: $SPT_PATH"

# ---------- helper: resolve reference DLLs from SPT install ----------
resolve_references() {
  local refs_dir="$1"
  local spt="$2"
  [[ -d "$spt" ]] || return 0   # silent — sem SPT, build vai falhar com mensagem do msbuild

  mkdir -p "$refs_dir"

  # Mapa: <DLL> <candidate-paths separados por |>
  local map=(
    "BepInEx.dll|$spt/BepInEx/core/BepInEx.dll"
    "0Harmony.dll|$spt/BepInEx/core/0Harmony.dll"
    "Assembly-CSharp.dll|$spt/EscapeFromTarkov_Data/Managed/Assembly-CSharp.dll"
    "UnityEngine.dll|$spt/EscapeFromTarkov_Data/Managed/UnityEngine.dll"
    "UnityEngine.CoreModule.dll|$spt/EscapeFromTarkov_Data/Managed/UnityEngine.CoreModule.dll"
    "UnityEngine.InputLegacyModule.dll|$spt/EscapeFromTarkov_Data/Managed/UnityEngine.InputLegacyModule.dll"
    "UnityEngine.PhysicsModule.dll|$spt/EscapeFromTarkov_Data/Managed/UnityEngine.PhysicsModule.dll"
    "UnityEngine.AnimationModule.dll|$spt/EscapeFromTarkov_Data/Managed/UnityEngine.AnimationModule.dll"
    "Comfort.dll|$spt/EscapeFromTarkov_Data/Managed/Comfort.dll"
    "Sirenix.Serialization.dll|$spt/EscapeFromTarkov_Data/Managed/Sirenix.Serialization.dll"
    "AnimationSystem.Types.dll|$spt/EscapeFromTarkov_Data/Managed/AnimationSystem.Types.dll"
    "SPT.Reflection.dll|$spt/BepInEx/plugins/spt/spt-reflection.dll"
    "SPT.Common.dll|$spt/BepInEx/plugins/spt/spt-common.dll"
  )

  local copied=0
  for entry in "${map[@]}"; do
    local dll="${entry%%|*}"
    local src="${entry#*|}"
    local dst="$refs_dir/$dll"
    if [[ -f "$dst" ]]; then continue; fi
    if [[ -f "$src" ]]; then
      cp -f "$src" "$dst"
      copied=$((copied+1))
    fi
  done

  [[ $copied -gt 0 ]] && echo "→ Resolvidas $copied referências de $spt para $refs_dir" || true
}

# ---------- build ----------
mkdir -p "$BUILDS"

case "$MOD_TYPE" in
  client-csharp)
    command -v dotnet >/dev/null || { echo "Erro: dotnet SDK não instalado" >&2; exit 1; }

    ASSEMBLY_NAME="$(grep -oE '<AssemblyName>[^<]+' "$CSPROJ" | head -1 | sed 's|<AssemblyName>||')"
    [[ -n "$ASSEMBLY_NAME" ]] || ASSEMBLY_NAME="$(basename "$CSPROJ" .csproj)"

    # Resolver References/<*.dll> ausentes a partir do SPT install
    REFS_DIR="$(dirname "$CSPROJ")/References"
    resolve_references "$REFS_DIR" "$SPT_PATH"

    echo "→ Compilando $CSPROJ → $BUILDS"
    dotnet build "$CSPROJ" -c Release -o "$BUILDS" \
      -p:DebugType=portable \
      -p:DebugSymbols=true \
      --nologo \
      --verbosity minimal

    DLL="$BUILDS/$ASSEMBLY_NAME.dll"
    [[ -f "$DLL" ]] || { echo "Erro: DLL não gerada em $DLL" >&2; exit 1; }

    echo "✓ Build OK: $DLL ($(stat -c%s "$DLL" 2>/dev/null || stat -f%z "$DLL") bytes)"

    if [[ -d "$SPT_PATH" ]]; then
      if [[ "$FLAT_INSTALL" == "1" ]]; then
        DEST_DIR="$SPT_PATH/BepInEx/plugins"
        DEST_DLL="$DEST_DIR/$ASSEMBLY_NAME.dll"
      else
        DEST_DIR="$SPT_PATH/BepInEx/plugins/$ASSEMBLY_NAME"
        DEST_DLL="$DEST_DIR/$ASSEMBLY_NAME.dll"
      fi
      mkdir -p "$DEST_DIR"
      cp -f "$DLL" "$DEST_DLL"
      [[ -f "${DLL%.dll}.pdb" ]] && cp -f "${DLL%.dll}.pdb" "$DEST_DIR/" 2>/dev/null || true
      echo "✓ Instalado: $DEST_DLL"
    fi
    ;;

  server-typescript)
    command -v npm >/dev/null || { echo "Erro: npm não instalado" >&2; exit 1; }

    echo "→ npm install"
    (cd "$MODDED" && npm install --no-audit --no-fund --silent)

    if (cd "$MODDED" && npm run | grep -qE '^\s*build'); then
      echo "→ npm run build"
      (cd "$MODDED" && npm run build --silent)
    else
      echo "→ Sem script 'build' em package.json — pulando compilação TS"
    fi

    # Server TS: copia o pacote inteiro (sem node_modules nem build artifacts grandes)
    rm -rf "$BUILDS"
    mkdir -p "$BUILDS"
    rsync -a --exclude='node_modules' --exclude='.git' --exclude='*.log' \
          "$MODDED/" "$BUILDS/" 2>/dev/null || \
      cp -r "$MODDED/." "$BUILDS/"

    echo "✓ Build OK: $BUILDS"

    if [[ -d "$SPT_PATH" ]]; then
      DEST_DIR="$SPT_PATH/SPT/user/mods/$MOD"
      mkdir -p "$DEST_DIR"
      rsync -a --delete --exclude='node_modules' "$BUILDS/" "$DEST_DIR/" 2>/dev/null || \
        cp -r "$BUILDS/." "$DEST_DIR/"
      echo "✓ Instalado: $DEST_DIR"
    fi
    ;;

  server-csharp)
    echo "Erro: server-csharp ainda não suportado por este script" >&2
    exit 1
    ;;
esac

cat <<EOF

✓ Compilação concluída — $MOD ($MOD_TYPE)
  Build local:  $BUILDS
EOF

if [[ -d "$SPT_PATH" ]]; then
  case "$MOD_TYPE" in
    client-csharp) echo "  Instalado em: ${DEST_DLL:-?}" ;;
    server-*)      echo "  Instalado em: ${DEST_DIR:-?}" ;;
  esac
fi
