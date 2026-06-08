#!/usr/bin/env bash
# compile-mod.sh — compila um mod (client BepInEx, server C#, server TypeScript, ou híbrido) e instala em D:\SPT.
# Uso: compile-mod.sh <mod-name> [--spt-path <path>] [--flat] [--clean]

set -euo pipefail

# ---------- args ----------
MOD=""
SPT_PATH="${SPT_PATH:-D:/SPT}"
FLAT_INSTALL=0
CLEAN_BUILD=0

while [[ $# -gt 0 ]]; do
  case "$1" in
    --spt-path) SPT_PATH="${2:-}"; shift 2 ;;
    --flat)     FLAT_INSTALL=1; shift ;;
    --clean)    CLEAN_BUILD=1; shift ;;
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

# ---------- helpers: classificar csproj + nomes de assembly próprios ----------
csproj_assembly_name() {  # $1 = caminho do csproj → nome do assembly (ou basename)
  local n
  n="$(grep -oE '<AssemblyName>[^<]+' "$1" | head -1 | sed 's|<AssemblyName>||' | tr -d '\r ')"
  [[ -n "$n" ]] && echo "$n" || basename "$1" .csproj
}

csproj_kind() {  # $1 = caminho do csproj → client | server | lib
  if grep -qiE 'BepInEx|Assembly-CSharp' "$1"; then echo client
  elif grep -qiE 'SPTarkov\.' "$1"; then echo server
  else echo lib
  fi
}

# ---------- detect mod type ----------
mapfile -t CSPROJS < <(find "$MODDED" -maxdepth 3 -name '*.csproj' 2>/dev/null \
  | grep -vE '/(obj|bin)/' | sort)
PACKAGE_JSON="$MODDED/package.json"

MOD_TYPE=""
if [[ ${#CSPROJS[@]} -gt 0 ]]; then
  MOD_TYPE="csharp"
elif [[ -f "$PACKAGE_JSON" ]]; then
  MOD_TYPE="server-typescript"
else
  echo "Erro: tipo de mod não detectado em $MODDED (sem .csproj nem package.json)" >&2
  exit 1
fi

# Assemblies próprios (DLLs produzidas pelos projetos DESTE mod) — usados para filtrar a
# instalação, garantindo que nunca distribuímos DLLs de terceiros (SPTarkov.*, Unity, BepInEx, NuGet).
declare -a OWN_ASSEMBLIES=()
if [[ "$MOD_TYPE" == "csharp" ]]; then
  for _f in "${CSPROJS[@]}"; do OWN_ASSEMBLIES+=("$(csproj_assembly_name "$_f")"); done
fi

echo "→ Mod: $MOD"
echo "→ Tipo: $MOD_TYPE"
echo "→ SPT path: $SPT_PATH"
[[ "$MOD_TYPE" == "csharp" ]] && echo "→ Projetos: ${#CSPROJS[@]} (assemblies próprios: ${OWN_ASSEMBLIES[*]})"

# ---------- helper: resolver DLLs de referência do client a partir do SPT install ----------
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
    "Newtonsoft.Json.dll|$spt/EscapeFromTarkov_Data/Managed/Newtonsoft.Json.dll"
    "Unity.TextMeshPro.dll|$spt/EscapeFromTarkov_Data/Managed/Unity.TextMeshPro.dll"
    "UnityEngine.UI.dll|$spt/EscapeFromTarkov_Data/Managed/UnityEngine.UI.dll"
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

  [[ $copied -gt 0 ]] && echo "  → Resolvidas $copied referências de $spt para $refs_dir" || true
}

# ---------- helper: instalar DLLs próprias de um diretório de build no destino ----------
install_own_dlls() {  # $1 = dir de build, $2 = dir de destino
  local out="$1" dest="$2" name src n=0
  mkdir -p "$dest"
  for name in "${OWN_ASSEMBLIES[@]}"; do
    src="$out/$name.dll"
    [[ -f "$src" ]] || continue
    [[ -f "$dest/$name.dll" ]] && cp -f "$dest/$name.dll" "$dest/$name.dll.bak"
    cp -f "$src" "$dest/$name.dll"
    [[ -f "$out/$name.pdb" ]] && cp -f "$out/$name.pdb" "$dest/" 2>/dev/null || true
    echo "  ✓ $name.dll → $dest"
    n=$((n+1))
  done
  [[ $n -gt 0 ]] || echo "  ! nenhuma DLL própria encontrada em $out" >&2
}

# ---------- build ----------
if [[ "$CLEAN_BUILD" == "1" && -d "$BUILDS" ]]; then
  echo "→ --clean: removendo $BUILDS"
  rm -rf "$BUILDS"
fi
mkdir -p "$BUILDS"

TS="$(date '+%y%m%d-%H%M')"

# ===== server-typescript =====
if [[ "$MOD_TYPE" == "server-typescript" ]]; then
  command -v npm >/dev/null || { echo "Erro: npm não instalado" >&2; exit 1; }

  echo "→ npm install"
  (cd "$MODDED" && npm install --no-audit --no-fund --silent)

  if (cd "$MODDED" && npm run | grep -qE '^\s*build'); then
    echo "→ npm run build"
    (cd "$MODDED" && npm run build --silent)
  else
    echo "→ Sem script 'build' em package.json — pulando compilação TS"
  fi

  rm -rf "$BUILDS"; mkdir -p "$BUILDS"
  rsync -a --exclude='node_modules' --exclude='.git' --exclude='*.log' \
        "$MODDED/" "$BUILDS/" 2>/dev/null || cp -r "$MODDED/." "$BUILDS/"

  echo "✓ Build OK: $BUILDS"

  if [[ -d "$SPT_PATH" ]]; then
    DEST_DIR="$SPT_PATH/SPT/user/mods/$MOD"
    mkdir -p "$DEST_DIR"
    rsync -a --delete --exclude='node_modules' "$BUILDS/" "$DEST_DIR/" 2>/dev/null \
      || cp -r "$BUILDS/." "$DEST_DIR/"
    echo "✓ Instalado: $DEST_DIR"
  fi

  echo; echo "✓ Compilação concluída — $MOD ($MOD_TYPE)"; echo "  Build local: $BUILDS"
  [[ -d "$SPT_PATH" ]] && echo "  Instalado em: ${DEST_DIR:-?}"
  exit 0
fi

# ===== csharp (client e/ou server; suporta mod híbrido com vários projetos) =====
command -v dotnet >/dev/null || { echo "Erro: dotnet SDK não instalado" >&2; exit 1; }

BUILT_CLIENT=0; BUILT_SERVER=0
CLIENT_DEST_SHOWN=""; SERVER_DEST_SHOWN=""

for CSPROJ in "${CSPROJS[@]}"; do
  KIND="$(csproj_kind "$CSPROJ")"
  # libs (ex.: Common) são compiladas como dependência transitiva dos entry projects;
  # suas DLLs próprias são instaladas junto via install_own_dlls (filtro OWN_ASSEMBLIES).
  [[ "$KIND" == "lib" ]] && { echo "→ [lib] $(basename "$CSPROJ") — pulando build direto (dependência transitiva)"; continue; }

  ASM="$(csproj_assembly_name "$CSPROJ")"
  OUT="$BUILDS/$KIND"
  echo "→ [$KIND] Compilando $(basename "$CSPROJ") (assembly: $ASM)"

  [[ "$KIND" == "client" ]] && resolve_references "$(dirname "$CSPROJ")/References" "$SPT_PATH"

  dotnet build "$CSPROJ" -c Release -o "$OUT" \
    -p:DebugType=portable \
    -p:DebugSymbols=true \
    --nologo \
    --verbosity minimal

  MAIN_DLL="$OUT/$ASM.dll"
  [[ -f "$MAIN_DLL" ]] || { echo "Erro: DLL não gerada em $MAIN_DLL" >&2; exit 1; }

  # Cópia versionada com timestamp (arquivo histórico em builds/)
  cp -f "$MAIN_DLL" "$BUILDS/$ASM-$TS.dll"
  echo "  ✓ Build OK: $MAIN_DLL ($(stat -c%s "$MAIN_DLL" 2>/dev/null || stat -f%z "$MAIN_DLL") bytes); arquivada $ASM-$TS.dll"

  [[ -d "$SPT_PATH" ]] || continue

  if [[ "$KIND" == "client" ]]; then
    if [[ "$FLAT_INSTALL" == "1" ]]; then
      CLIENT_DEST="$SPT_PATH/BepInEx/plugins"
      CONFLICT_DIR="$SPT_PATH/BepInEx/plugins/$ASM"
      [[ -d "$CONFLICT_DIR" ]] && { echo "  → Removendo subfolder conflitante: $CONFLICT_DIR"; rm -rf "$CONFLICT_DIR"; }
    else
      CLIENT_DEST="$SPT_PATH/BepInEx/plugins/$MOD"
      CONFLICT_FLAT="$SPT_PATH/BepInEx/plugins/$ASM.dll"
      [[ -f "$CONFLICT_FLAT" ]] && { echo "  → Removendo DLL flat conflitante: $CONFLICT_FLAT"; rm -f "$CONFLICT_FLAT" "${CONFLICT_FLAT}.bak" "${CONFLICT_FLAT%.dll}.pdb"; }
    fi
    install_own_dlls "$OUT" "$CLIENT_DEST"
    CLIENT_DEST_SHOWN="$CLIENT_DEST"; BUILT_CLIENT=1
  else  # server
    SERVER_DEST="$SPT_PATH/SPT/user/mods/$MOD"
    install_own_dlls "$OUT" "$SERVER_DEST"
    SERVER_DEST_SHOWN="$SERVER_DEST"; BUILT_SERVER=1
    # Copiar config/ do mod (JSONs de classe etc.) para o install do servidor
    if [[ -d "$(dirname "$CSPROJ")/config" ]]; then
      cp -r "$(dirname "$CSPROJ")/config" "$SERVER_DEST/"
      echo "  ✓ config/ → $SERVER_DEST"
    fi
  fi
done

cat <<EOF

✓ Compilação concluída — $MOD (csharp)
  Build local:  $BUILDS
EOF
[[ "$BUILT_CLIENT" == "1" ]] && echo "  Client → ${CLIENT_DEST_SHOWN:-?}"
[[ "$BUILT_SERVER" == "1" ]] && echo "  Server → ${SERVER_DEST_SHOWN:-?}"
exit 0
