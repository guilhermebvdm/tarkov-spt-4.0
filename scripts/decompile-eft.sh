#!/bin/bash
# decompile-eft.sh — regenera o decompile COMPLETO do Assembly-CSharp do EFT.
#
# Uso:
#   bash scripts/decompile-eft.sh                 # gera, valida e substitui o dump
#   bash scripts/decompile-eft.sh --dry-run       # gera em temp e valida, SEM substituir
#   DUMP_VERSION=2 bash scripts/decompile-eft.sh  # bump quando o HARNESS muda (mesma DLL)
#
# POR QUE ESTE SCRIPT EXISTE (não use `ilspycmd -p`):
#   o modo projeto do ilspycmd ABORTA no primeiro método indecompilável e descarta namespaces
#   INTEIROS em silêncio — foi assim que o dump anterior ficou com 102 pastas vazias
#   (EFT.Animations, EFT.HealthSystem, EFT.InventoryLogic, EFT.CameraControl, EFT.UI...).
#   O harness em .agents/tools/decompile-eft/ itera POR TIPO com try/catch: o que falha vira
#   um stub `// DECOMPILE-ERROR` visível e registrado no índice.
#
# REDE DE SEGURANÇA: gera em diretório TEMPORÁRIO, valida, e só então substitui. O dump
# funcional nunca é destruído sem um substituto validado.
#
# Saída versionada: references/eft-decompiled/{types-index.json,.provenance.json}
# Saída gitignored: references/eft-decompiled/Assembly-CSharp/ (ver .gitignore)

set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$REPO_ROOT"

DRY_RUN=0
[ "${1:-}" = "--dry-run" ] && DRY_RUN=1

GAME_DIR="${GAME_DIR:-D:/SPT}"
DLL="$GAME_DIR/EscapeFromTarkov_Data/Managed/Assembly-CSharp.dll"
MAPPINGS="docs/files-from-4.1/consolidated-mappings.txt"
DEST="references/eft-decompiled"
HARNESS=".agents/tools/decompile-eft"
DUMP_VERSION="${DUMP_VERSION:-1}"

# ── pré-requisitos ────────────────────────────────────────────────────────────
[ -f "$DLL" ] || { echo "❌ DLL não encontrada: $DLL"; echo "   (defina GAME_DIR se o jogo está em outro lugar)"; exit 1; }
command -v dotnet >/dev/null 2>&1 || { echo "❌ dotnet SDK não encontrado no PATH."; exit 1; }
[ -f "$MAPPINGS" ] && echo "▶ mapping 4.1: $MAPPINGS" || echo "⚠ mapping 4.1 ausente ($MAPPINGS) — dump sairá sem aliases"

# ── provenance ────────────────────────────────────────────────────────────────
echo "▶ coletando provenance…"
DLL_SHA="$(sha256sum "$DLL" | cut -d' ' -f1)"
BUILD_GUID="$(grep -a 'build-guid=' "$GAME_DIR/EscapeFromTarkov_Data/boot.config" 2>/dev/null | head -1 | cut -d= -f2 | tr -d '\r' || true)"
# Versões: o EFT não muda mais (SPT não sobe para a 1.0), mas o SPT RE-PATCHEIA a DLL a cada
# update dele — por isso sptVersion entra na provenance.
# A versão do EFT vem do FileVersion do executável: o boot.config só tem o build-guid, NÃO a
# versão (uma tentativa de grep ali cai sempre no fallback e grava "0.16.x" em silêncio).
EFT_VERSION="${EFT_VERSION:-$(powershell.exe -NoProfile -Command "(Get-Item '$GAME_DIR/EscapeFromTarkov.exe').VersionInfo.FileVersion" 2>/dev/null | tr -d '\r\n' || true)}"
case "${EFT_VERSION:-}" in
  0.*) : ;;                       # ok, formato esperado (ex.: 0.16.9.40087)
  *)   EFT_VERSION="0.16.x"; echo "  (aviso: não consegui ler a versão do EscapeFromTarkov.exe — usando '$EFT_VERSION')" ;;
esac
SPT_VERSION="${SPT_VERSION:-$(grep -oE '"sptVersion"\s*:\s*"[^"]+"' references/manifest.json 2>/dev/null | head -1 | sed -E 's/.*"([^"]+)"$/\1/' || true)}"
[ -n "${SPT_VERSION:-}" ] || SPT_VERSION="unknown"
echo "  sha256=${DLL_SHA:0:16}… buildGuid=${BUILD_GUID:-?} eft=$EFT_VERSION spt=$SPT_VERSION dumpVersion=$DUMP_VERSION"

# ── build do harness ──────────────────────────────────────────────────────────
echo "▶ compilando o harness…"
dotnet build "$HARNESS/DecompileEft.csproj" -c Release -v quiet --nologo >/dev/null
HARNESS_DLL="$HARNESS/bin/Release/net10.0/DecompileEft.dll"
[ -f "$HARNESS_DLL" ] || { echo "❌ harness não compilou: $HARNESS_DLL"; exit 1; }

# ── geração em TEMP (rede de segurança) ───────────────────────────────────────
TMP="$(mktemp -d)"
trap 'rm -rf "$TMP"' EXIT
echo "▶ descompilando para temp ($TMP)… isto leva alguns minutos"
dotnet "$HARNESS_DLL" "$DLL" "$TMP/Assembly-CSharp" \
  ${MAPPINGS:+--mappings "$MAPPINGS"} \
  --dump-version "$DUMP_VERSION" --spt-version "$SPT_VERSION" \
  --eft-version "$EFT_VERSION" --build-guid "$BUILD_GUID" --dll-sha256 "$DLL_SHA"

# ── validação (nada é substituído antes disso passar) ─────────────────────────
echo "▶ validando…"
CS_COUNT="$(find "$TMP/Assembly-CSharp" -name '*.cs' | wc -l)"
echo "  arquivos .cs: $CS_COUNT"
# Piso de sanidade: são ~8.683 tipos TOP-LEVEL nesta build (os aninhados saem inline no arquivo do
# pai, por isso o número é menor que os ~15 mil tipos totais da DLL). O dump anterior, parcial,
# tinha 4.561 — qualquer coisa abaixo de 8.000 indica que o harness falhou em massa.
[ "$CS_COUNT" -gt 8000 ] || { echo "❌ contagem suspeita (<8000, esperado ~8.7k) — abortando SEM substituir"; exit 1; }

FAIL=0
for CANARY in "EFT.Animations/ProceduralWeaponAnimation.cs" "EFT.HealthSystem/ActiveHealthController.cs" "GClass2348.cs"; do
  if [ -s "$TMP/Assembly-CSharp/$CANARY" ]; then echo "  ✓ canário: $CANARY"
  else echo "  ✗ canário AUSENTE/vazio: $CANARY"; FAIL=1; fi
done
[ "$FAIL" = "0" ] || { echo "❌ tipos-canário faltando — abortando SEM substituir"; exit 1; }

[ -s "$TMP/types-index.json" ] || { echo "❌ types-index.json ausente — abortando"; exit 1; }

# Lê as contagens do índice. jq NÃO está instalado no Git Bash desta máquina, então python é o
# caminho real aqui — sem esse fallback o GO/NO-GO dos stubs era pulado EM SILÊNCIO (o guard
# `command -v jq` fazia a validação sumir sem avisar, que é justamente o tipo de buraco que este
# plano existe para eliminar).
COUNTS=""
if command -v jq >/dev/null 2>&1; then
  COUNTS="$(jq -r '"\(.counts.total) \(.counts.stubError) \(.counts.withAlias41)"' "$TMP/types-index.json")"
elif command -v python >/dev/null 2>&1; then
  COUNTS="$(python -c "import json,sys;c=json.load(open(sys.argv[1],encoding='utf-8'))['counts'];print(c['total'],c['stubError'],c['withAlias41'])" "$TMP/types-index.json")"
else
  echo "❌ nem jq nem python disponíveis — não dá para validar o índice. Abortando."; exit 1
fi
read -r TOTAL STUBS ALIASES <<< "$COUNTS"
echo "  índice: total=$TOTAL stub-error=$STUBS aliases4.1=$ALIASES"
[ "$TOTAL" -gt 8000 ] || { echo "❌ índice inconsistente (total=$TOTAL) — abortando"; exit 1; }

# GO/NO-GO do plano: >100 stubs absolutos = investigar o harness antes de publicar. Sem isso
# estaríamos chamando de "completo" um dump furado — e a regra do harness manda confiar nele.
if [ "$STUBS" -gt 100 ]; then
  echo "⚠ ACIMA DO THRESHOLD ($STUBS stubs > 100). Revise antes de confiar no dump como 'completo'."
  [ "$DRY_RUN" = "1" ] || { echo "   Abortando. Rode com --dry-run para inspecionar."; exit 1; }
fi

if [ "$DRY_RUN" = "1" ]; then
  KEEP="$REPO_ROOT/.decompile-dryrun"
  rm -rf "$KEEP"; mkdir -p "$KEEP"; cp -r "$TMP/." "$KEEP/"
  trap - EXIT; rm -rf "$TMP"
  echo "✓ dry-run: resultado em $KEEP (NADA foi substituído)"
  exit 0
fi

# ── substituição atômica-ish ──────────────────────────────────────────────────
echo "▶ substituindo o dump…"
mkdir -p "$DEST"
rm -rf "$DEST/Assembly-CSharp"
mv "$TMP/Assembly-CSharp" "$DEST/Assembly-CSharp"
mv "$TMP/types-index.json" "$DEST/types-index.json"

cat > "$DEST/.provenance.json" <<EOF
{
  "_readme": "Origem deste dump. O EFT nao recebe mais updates compativeis com o SPT, mas o SPT RE-PATCHEIA a Assembly-CSharp a cada update dele — por isso sptVersion importa.",
  "dllSha256": "$DLL_SHA",
  "buildGuid": "$BUILD_GUID",
  "eftVersion": "$EFT_VERSION",
  "sptVersion": "$SPT_VERSION",
  "dumpVersion": $DUMP_VERSION,
  "generatedAtUtc": "$(date -u +%Y-%m-%dT%H:%M:%SZ)",
  "generatedBy": "scripts/decompile-eft.sh + .agents/tools/decompile-eft (ICSharpCode.Decompiler 10.1.0.8386)"
}
EOF

echo
echo "✓ dump completo em $DEST/Assembly-CSharp ($CS_COUNT arquivos)"
echo "  versionados: types-index.json + .provenance.json   |   gitignored: Assembly-CSharp/"
echo "▶ próximo passo: bash scripts/update-graphs.sh eft-decompiled"
