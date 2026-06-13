#!/usr/bin/env bash
# compile-mod.sh — compila um mod (client BepInEx, server C#, server TypeScript, ou híbrido) e instala em D:\SPT.
# Uso: compile-mod.sh <mod-name> [--spt-path <path>] [--flat] [--clean] [--force-config]
#   --force-config: overwrite install config/ even when it diverges from the repo (guard rail, item 019)

set -euo pipefail

# ---------- args ----------
MOD=""
SPT_PATH="${SPT_PATH:-}"   # env tem precedência; senão .spt-path; senão default (resolvido após ROOT)
FLAT_INSTALL=0
CLEAN_BUILD=0
FORCE_CONFIG=0
CONFIG_SKIPPED=0

while [[ $# -gt 0 ]]; do
  case "$1" in
    --spt-path)     SPT_PATH="${2:-}"; shift 2 ;;
    --flat)         FLAT_INSTALL=1; shift ;;
    --clean)        CLEAN_BUILD=1; shift ;;
    --force-config) FORCE_CONFIG=1; shift ;;
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

# SPT_PATH precedence: env / --spt-path > .spt-path (gitignored, per-machine) > default D:/SPT.
# Parse ONLY the SPT_PATH= key (never `source` — avoids arbitrary shell execution).
# The .spt-path file lives at the repo root; value must be unquoted (e.g. SPT_PATH=D:/SPT).
if [[ -z "$SPT_PATH" && -f "$ROOT/.spt-path" ]]; then
  SPT_PATH="$(grep -E '^SPT_PATH=' "$ROOT/.spt-path" | tail -1 | cut -d= -f2- | tr -d '\r')"
fi
SPT_PATH="${SPT_PATH:-D:/SPT}"

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
    "UnityEngine.TextRenderingModule.dll|$spt/EscapeFromTarkov_Data/Managed/UnityEngine.TextRenderingModule.dll"
    "UnityEngine.ImageConversionModule.dll|$spt/EscapeFromTarkov_Data/Managed/UnityEngine.ImageConversionModule.dll"
    "UnityEngine.IMGUIModule.dll|$spt/EscapeFromTarkov_Data/Managed/UnityEngine.IMGUIModule.dll"
    "Fika.Core.dll|$spt/BepInEx/plugins/Fika/Fika.Core.dll"
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

# ---------- helpers: config guard rails (item 019 — anti-clobber) ----------
# The web editor (items 020+) writes class .jsonc files directly into the INSTALL
# (SPT/user/mods/<mod>/config/classes/). A blind repo→install copy on every build
# would destroy those edits, so the copy is guarded: if the install diverges from
# the repo, the config copy is skipped (the DLL install still proceeds).

# Normalize CRLF→LF and strip trailing whitespace so EOL-only differences never
# count as divergence (repo is checked out on Windows; editors may rewrite EOLs).
normalize_text() { sed -e 's/\r$//' -e 's/[ \t]*$//' "$1"; }

files_equal_normalized() { cmp -s <(normalize_text "$1") <(normalize_text "$2"); }

newer_side() {  # $1 = repo file, $2 = install file → which side has the newer mtime
  if   [[ "$1" -nt "$2" ]]; then echo "repo newer"
  elif [[ "$2" -nt "$1" ]]; then echo "install newer"
  else echo "same mtime"
  fi
}

# List guarded config data files (paths relative to the config dir):
# classes/**/*.json[c] plus root-level *.json[c] (e.g. hidden-editions.jsonc).
# CR-EP-09: root-level covers BOTH extensions, same as classes/.
list_config_files() {  # $1 = config dir
  local dir="$1"
  { if [[ -d "$dir/classes" ]]; then
      find "$dir/classes" -type f \( -name '*.json' -o -name '*.jsonc' \)
    fi
    find "$dir" -maxdepth 1 -type f \( -name '*.json' -o -name '*.jsonc' \)
  } | sed "s|^$dir/||" | sort
}

install_server_config() {  # $1 = repo config dir, $2 = server install mod dir
  local src="$1" dest_root="$2" dest="$2/config" rel
  if [[ "$FORCE_CONFIG" == "1" ]]; then
    cp -r "$src" "$dest_root/"
    echo "  ✓ config/ → $dest_root (--force-config: install overwritten with repo version)"
    return 0
  fi
  if [[ ! -d "$dest/classes" ]]; then
    cp -r "$src" "$dest_root/"
    echo "  ✓ config/ → $dest_root (fresh install — no classes/ in install yet)"
    return 0
  fi

  # CR-EP-02: a repo-only file at the FIRST level of classes/ is ambiguous — it may be a NEW
  # class added in the repo OR a class DELETED through the web editor in the install. Copying it
  # would silently resurrect the deleted class, so those files BLOCK the config copy.
  # CR2-EP-02: the editor (and the boot loader) only manage classes/<file> directly — SUBFOLDERS
  # (classes/_docs/ drafts etc.) have no editor delete flow, so a repo-only file there is always
  # a new file: copy-as-new, never a deletion candidate. Root-level files: same, copy-as-new.
  local divergent=() repo_only_classes=() repo_only_safe=() install_only=()
  while IFS= read -r rel; do
    if [[ -z "$rel" ]]; then continue; fi
    if [[ ! -f "$dest/$rel" ]]; then
      case "$rel" in
        classes/*/*) repo_only_safe+=("$rel") ;;     # subfolder draft — editor never deletes there
        classes/*)   repo_only_classes+=("$rel") ;;  # first level — possible editor deletion
        *)           repo_only_safe+=("$rel") ;;     # root-level config — no editor delete flow
      esac
    elif ! files_equal_normalized "$src/$rel" "$dest/$rel"; then
      divergent+=("$rel")
    fi
  done < <(list_config_files "$src")
  while IFS= read -r rel; do
    if [[ -n "$rel" && ! -f "$src/$rel" ]]; then install_only+=("$rel"); fi
  done < <(list_config_files "$dest")

  if [[ ${#divergent[@]} -eq 0 && ${#repo_only_classes[@]} -eq 0 ]]; then
    cp -r "$src" "$dest_root/"
    echo "  ✓ config/ → $dest_root (install matches repo — no divergence)"
    if [[ ${#install_only[@]} -gt 0 ]]; then
      echo "  i config: ${#install_only[@]} file(s) exist only in the install (kept untouched):"
      for rel in "${install_only[@]}"; do echo "      install-only: $rel"; done
      echo "    → run /sync-classes to bring them into the repo."
    fi
    return 0
  fi

  CONFIG_SKIPPED=1
  echo "  ✗ config/ copy SKIPPED — install diverges from repo (anti-clobber guard, item 019):"
  for rel in ${divergent[@]+"${divergent[@]}"}; do
    echo "      DIVERGES: $rel ($(newer_side "$src/$rel" "$dest/$rel"))"
  done
  for rel in ${repo_only_classes[@]+"${repo_only_classes[@]}"}; do
    echo "      repo-only: $rel — new class in repo OR deleted via editor in install;"
    echo "                 use --force-config to copy, or sync-classes to propagate the deletion"
  done
  if [[ ${#install_only[@]} -gt 0 ]]; then
    for rel in "${install_only[@]}"; do echo "      install-only: $rel"; done
  fi
  echo "    The DLL and the rest of the install proceeded normally; ONLY the config/ copy was skipped."
  echo "    → /sync-classes  : bring install edits back into the repo (then rebuild)"
  echo "    → --force-config : overwrite the install config with the repo version"
  # Repo-only root-level files and classes/ SUBFOLDER files are new — copying them clobbers nothing.
  if [[ ${#repo_only_safe[@]} -gt 0 ]]; then
    local n=0
    for rel in "${repo_only_safe[@]}"; do
      mkdir -p "$(dirname "$dest/$rel")"
      cp -f "$src/$rel" "$dest/$rel"
      n=$((n+1))
    done
    echo "  ✓ config: copied $n new repo-only file(s) (root-level/subfolder — no clobber risk)"
  fi
  return 0
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
  rsync -a --exclude='node_modules' --exclude='.git' --exclude='*.log' --exclude='graphify-out' \
        "$MODDED/" "$BUILDS/" 2>/dev/null || { cp -r "$MODDED/." "$BUILDS/"; rm -rf "$BUILDS/graphify-out"; }

  echo "✓ Build OK: $BUILDS"

  if [[ -d "$SPT_PATH" ]]; then
    DEST_DIR="$SPT_PATH/SPT/user/mods/$MOD"
    mkdir -p "$DEST_DIR"
    rsync -a --delete --exclude='node_modules' --exclude='graphify-out' "$BUILDS/" "$DEST_DIR/" 2>/dev/null \
      || { cp -r "$BUILDS/." "$DEST_DIR/"; rm -rf "$DEST_DIR/graphify-out"; }
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
    # Copiar assets de ícones do client (item 011), se existirem — simétrico ao config/ do server.
    if [[ -d "$(dirname "$CSPROJ")/icons" ]]; then
      mkdir -p "$CLIENT_DEST/icons"
      cp -f "$(dirname "$CSPROJ")"/icons/*.png "$CLIENT_DEST/icons/" 2>/dev/null || true
      echo "  ✓ icons/ → $CLIENT_DEST/icons"
    fi
    CLIENT_DEST_SHOWN="$CLIENT_DEST"; BUILT_CLIENT=1
  else  # server
    SERVER_DEST="$SPT_PATH/SPT/user/mods/$MOD"
    install_own_dlls "$OUT" "$SERVER_DEST"
    SERVER_DEST_SHOWN="$SERVER_DEST"; BUILT_SERVER=1
    # Copiar config/ do mod (JSONs de classe etc.) para o install do servidor.
    # item 019: guarded copy — never clobbers install-side edits without --force-config.
    if [[ -d "$(dirname "$CSPROJ")/config" ]]; then
      install_server_config "$(dirname "$CSPROJ")/config" "$SERVER_DEST"
    fi
    # item 019 (kickoff d): ship wwwroot/ (static web assets, items 020+).
    # wwwroot is code, not data — clobber is intentional (orphans removed).
    if [[ -d "$(dirname "$CSPROJ")/wwwroot" ]]; then
      rm -rf "$SERVER_DEST/wwwroot"
      cp -r "$(dirname "$CSPROJ")/wwwroot" "$SERVER_DEST/"
      echo "  ✓ wwwroot/ → $SERVER_DEST"
    fi
  fi
done

cat <<EOF

✓ Compilação concluída — $MOD (csharp)
  Build local:  $BUILDS
EOF
[[ "$BUILT_CLIENT" == "1" ]] && echo "  Client → ${CLIENT_DEST_SHOWN:-?}"
[[ "$BUILT_SERVER" == "1" ]] && echo "  Server → ${SERVER_DEST_SHOWN:-?}"
if [[ "$CONFIG_SKIPPED" == "1" ]]; then
  echo "  ⚠ config/ NOT copied to the install (diverges from repo) — use /sync-classes or --force-config"
fi
exit 0
