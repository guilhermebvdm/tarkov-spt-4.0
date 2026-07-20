#!/bin/bash
# remind-use-graph.sh — PreToolUse(Bash) do Claude Code.
#
# Objetivo: quando o decompile completo JÁ ESTÁ na máquina, lembrar de consultá-lo antes de
# descompilar tipo a tipo por reflexo. O `ilspycmd` era rotina porque o dump tinha 102 namespaces
# vazios; desde 2026-07-19 ele é completo (8.683 tipos) e a descompilação virou exceção.
#
# Semântica de saída (PreToolUse do Claude Code):
#   exit 0 → deixa passar (silencioso)
#   exit 2 → BLOQUEIA a chamada e injeta o stderr no contexto do agente  ← usado aqui
#
# Duas salvaguardas deliberadas:
#   1) Se o dump NÃO estiver em disco, sai 0 — quem não tem o dump PRECISA do ilspycmd; bloquear
#      prenderia justamente quem depende do fallback.
#   2) Escape explícito com o sufixo `# allow-ilspy`, que preserva o prefixo do comando (o comando
#      continua começando com `ilspycmd`, então segue casando a allowlist de permissões).

set -uo pipefail

DUMP="${CLAUDE_PROJECT_DIR:-.}/references/eft-decompiled/Assembly-CSharp"
INDEX="${CLAUDE_PROJECT_DIR:-.}/references/eft-decompiled/types-index.json"

# (1) sem dump em disco → ilspycmd é o caminho legítimo. Não atrapalhar.
[ -d "$DUMP" ] || exit 0

INPUT="$(cat 2>/dev/null || true)"
if command -v jq >/dev/null 2>&1; then
  CMD="$(printf '%s' "$INPUT" | jq -r '.tool_input.command // empty' 2>/dev/null || true)"
else
  # jq não está instalado no Git Bash desta máquina — extração best-effort sem ele.
  CMD="$(printf '%s' "$INPUT" | sed -n 's/.*"command"[[:space:]]*:[[:space:]]*"\(.*\)".*/\1/p' | head -1)"
fi
[ -n "${CMD:-}" ] || exit 0

# (2) escape explícito
case "$CMD" in *"# allow-ilspy"*) exit 0 ;; esac

# (3) só interessa o Assembly-CSharp do EFT. Descompilar OUTRAS DLLs (mods de terceiros,
# 0Harmony, Fika, SPT...) é trabalho legítimo que o dump do EFT não substitui — bloquear ali
# seria falso-positivo com mensagem enganosa. Casos reais já ocorridos neste repo:
# MoreBotsServer.dll, 0Harmony.dll, BringBackConcussion.dll.
case "$CMD" in *[Aa]ssembly-[Cc][Ss]harp*) ;; *) exit 0 ;; esac

# Match ANCORADO em posição-de-comando: só dispara quando ilspycmd é EXECUTADO, não quando a
# string aparece de passagem (ex.: `grep ilspycmd docs/`, um heredoc de commit citando a
# ferramenta, ou `cat` de um .md que fala dela).
if printf '%s' "$CMD" | grep -qE '(^|[;&|]|&&)[[:space:]]*([^[:space:]]*/)?ilspycmd([[:space:]]|$)'; then
  {
    echo "[harness] ilspycmd interceptado — o decompile COMPLETO já está nesta máquina."
    echo
    echo "  O dump em references/eft-decompiled/Assembly-CSharp/ tem 8.683 tipos e 0 namespaces"
    echo "  vazios (o buraco que tornava ilspycmd rotina foi fechado em 2026-07-19). Antes de"
    echo "  descompilar, tente nesta ordem:"
    echo
    echo "  1. Sei o NOME do tipo   -> grafo MCP (graphify-eft: query_graph / get_node)"
    echo "                             depois abra o .cs para provar a assinatura."
    echo "  2. Sei só o CONCEITO    -> grep no dump pelo alias 4.1, ou consulte"
    echo "                             references/eft-decompiled/types-index.json"
    echo "                             (ex.: 'Localization' -> GClass2348). O grafo NAO indexa"
    echo "                             os aliases, então essa busca passa pelo índice/grep."
    echo "  3. Existe mesmo?        -> types-index.json lista TODOS os tipos do assembly."
    echo "                             Ausência AQUI = não existe. Um grep vazio não prova nada."
    echo
    echo "  ilspycmd continua correto em 3 casos: o tipo está marcado // DECOMPILE-ERROR (são 8,"
    echo "  ex.: BackendAbstractClass), o tipo NAO está no índice, ou você precisa da DLL crua."
    echo "  Nesses casos, repita o comando com o sufixo:   # allow-ilspy"
    [ -f "$INDEX" ] || echo "  (aviso: types-index.json ausente — rode: bash scripts/decompile-eft.sh)"
  } >&2
  exit 2
fi

exit 0
