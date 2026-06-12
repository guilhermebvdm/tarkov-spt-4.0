# 015 — Identidade da classe no nome do jogador · Code Review 01

**Mod:** CustomClasses
**Asbuild:** [015-identidade-deploy-05-asbuild.md](015-identidade-deploy-05-asbuild.md)
**Data:** 2026-06-08

> Revisão crítica pós-`/code-mod` (pedido do usuário). Foco no `ChatSpecialIconPatch` (widget compartilhado).

## Resumo

> 🔴 Bloqueadores: 0 (1 corrigido na hora) · 🟠 Fortes: 0 · 🟡 Médios: 1 · 🟢 Menores: 2 · ✅ Resolvidos: 1

## Pontos

### CR-01-01 · A — Crítico · ✅ Resolvido em 2026-06-08

**Gradiente vazava para outros jogadores em listas recicladas**

**Problema:** o `ChatSpecialIcon` é reciclado em listas (chat/online). O vanilla restaura `_icon.sprite` e `_specialLabel.color` (sólida) ao mostrar outro jogador, mas **não** desliga `enableVertexGradient`. Como o patch liga o vertex gradient para o jogador local, uma célula reaproveitada para **outro jogador** mantinha o gradiente da classe local.

**Resolução:** no ramo "não é o jogador local", `____specialLabel.enableVertexGradient = false` (restaura o comportamento vanilla). O `sprite`/cor já são restaurados pelo próprio vanilla. ([ChatSpecialIconPatch.cs](../../modded/Client/Patches/ChatSpecialIconPatch.cs))

### CR-01-02 · C — Gap vs. spec · 🟡 Médio

**Título em "2ª linha no character" não implementado (só sufixo mesma-linha)**

**Problema:** a decisão foi "2ª linha no character, sufixo nas listas". A 1ª versão usa **sufixo `[Classe]` mesma-linha em todos** (seguro, sem conflito). Falta detectar o contexto "character" para a 2ª linha.

**Sugestão:** validar in-game se o sufixo já satisfaz; se quiser a 2ª linha no character, detectar por `fontSize`/tamanho do rect (character tem fonte grande) — refinamento. Decidir após ver o resultado.

**Decisão:** `[ ]` Pendente · `[ ]` Aceitar (validar; refinar se necessário)

### CR-01-03 · F — Melhoria · 🟢 Menor

**`EnsureLoaded()` chamado em todo `ChatSpecialIcon.Show`**

Roda para qualquer jogador (chat/online), mas é **lazy** (só faz fetch 1x; depois é um `if (_loaded) return`). Custo desprezível. Aceitar.

### CR-01-04 · B — Bug latente · 🟢 Menor

**`GetTargetMethod` via `First(...)` lança se o EFT mudar `Show`**

Mesma fragilidade dos outros patches (entre versões do EFT). Baixa prioridade; `FirstOrDefault`+log como dívida opcional.

## Histórico

| Data | Evento |
| --- | --- |
| 2026-06-08 | Code review 01 — 1 🔴 (gradiente vazando) corrigido na hora; 1 🟡 (2ª linha) + 2 🟢. Recompilado 0 warn/err. |
