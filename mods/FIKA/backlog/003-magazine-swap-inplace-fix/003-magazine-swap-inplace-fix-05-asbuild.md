# 003 — Fix rejeição de swap de magazine in-place no Headless (GClass1561) · As-Built

**Mod:** FIKA
**Spec funcional:** [003-magazine-swap-inplace-fix-01-spec.md](003-magazine-swap-inplace-fix-01-spec.md)
**Spec técnica:** [003-magazine-swap-inplace-fix-02-spec-tech.md](003-magazine-swap-inplace-fix-02-spec-tech.md)
**Última review técnica:** [003-magazine-swap-inplace-fix-03-spec-tech-review-01.md](003-magazine-swap-inplace-fix-03-spec-tech-review-01.md)
**Build inicial:** 2026-09-06

> Documentação **pós-implementação**. Reflete o estado real do código entregue pelo `/code-mod` e atualizado por `/apply-code-review`. Quando o conteúdo aqui diverge da spec técnica, este documento ganha — a spec é planejamento, o asbuild é o que foi feito.

## Desvio confirmado durante o build (importante)

A spec técnica (§1.4/§5) previa dois Harmony patches simétricos capturando o item efetivamente movido: um em `Player.TryRemoveFromHands` e outro em `Player.TrySetInHands`. Ao reler `ObservedInventoryController.cs` por completo antes de editar (passo obrigatório do `/code-mod`), confirmou-se que **`InProcess` é sobrescrito nessa classe** (linhas 260-274, delega para `HandleInProcess` 276-305) e **chama `RaiseInOutProcessEvents` diretamente**, sem nunca passar por `Player.TrySetInHands` — ao contrário do que a spec assumia (que só auditou o caminho herdado de `Player.PlayerInventoryController.InProcess`).

Correção aplicada: o patch em `Player.TrySetInHands` (`CaptureMovedItemOnSet`) foi **removido do design** — seria código morto para o cenário Headless que este item resolve. No lugar, `ObservedInventoryController.HandleInProcess` chama diretamente `InOutHandsProcessTimestampPatch.SetPendingMovedItem(item)` antes de `RaiseInOutProcessEvents` (é código do próprio mod, não precisa de Harmony). O lado `OutProcess`/`TryRemoveFromHands` (não sobrescrito por `ObservedInventoryController`) continua coberto pelo Harmony patch original (`CaptureMovedItemOnRemove`). O mecanismo central de correlação (`RecordBeginSucceed`, Postfix em `TraderControllerClass.RaiseInOutProcessEvents`) não mudou.

Resultado: design final mais simples que o planejado (1 Harmony patch a menos), e estruturalmente correto para o caminho real que o Headless executa — o que a versão original da spec técnica não cobriria de fato.

## Arquivos alterados (build inicial)

| Ação | Path | Resumo |
| --- | --- | --- |
| CRIADO | `mods/FIKA/modded/Fika-Plugin/Fika.Core/Main/Patches/InventoryPatches/InOutHandsProcessTimestampPatch.cs` | Correlaciona cada janela Begin/Succeed de "entrar/sair das mãos" com o item que a abriu (arma ou item aninhado), via 1 Harmony patch (`Player.TryRemoveFromHands`) + 1 Postfix (`TraderControllerClass.RaiseInOutProcessEvents`) + 1 setter público chamado diretamente por `ObservedInventoryController` |
| MODIFICADO | `mods/FIKA/modded/Fika-Plugin/Fika.Core/Main/ObservedClasses/ObservedInventoryController.cs` | `HandleInProcess` registra o item efetivamente movido antes do `Begin`; `CheckItemAction` ganha a exceção escopada `IsSelfReferentialMagazineSwap` (método de instância) no bloco `inOutHandsProcess`, sem alterar as demais checagens de colisão do laço |
| MODIFICADO | `mods/FIKA/modded/Fika-Plugin/Fika.Core/FikaPlugin.cs` | Bump `FikaVersion`: `2.3.13` → `2.3.14`; (CR-01-01) registro explícito dos 2 novos `ModulePatch` em `EnableModulePatches()`, sem depender de auto-discovery de tipos aninhados |
| MODIFICADO | `mods/FIKA/mod.json` | Bump do componente `plugin`: `2.3.11` → `2.3.14` (alinha drift de versão pré-existente, não relacionado a este item) |

## PA-NN-MM resolvidos durante o build

> Pontos da review técnica 01 que foram confirmados/refinados como parte da implementação (a resolução formal de cada um já está registrada em `003-magazine-swap-inplace-fix-03-spec-tech-review-01.md`; aqui só o que o código realmente fez).

| ID | Categoria · Impacto | Resumo da resolução |
| --- | --- | --- |
| PA-01-01 | C — Erro de lógica · 🔴 | `IsSelfReferentialMagazineSwap` implementado como método de instância (sem `static`) |
| PA-01-02 | A — Gap · 🔴 | Exceção agora exige `movedItem is MagazineItemClass`; um saque/guarda de arma real grava a própria arma como item movido e nunca satisfaz a condição — confirmado adicionalmente que, no caminho `InProcess` do Headless, o item vem direto do parâmetro de `InProcess`, não de `TrySetInHands` (ver "Desvio confirmado" acima) |
| PA-01-03 | A — Gap · 🟡 | Não exigiu mudança de código — a checagem `item == geventArgs2.Item` (`ObservedInventoryController.cs`) permanece intocada pela edição, cobrindo o corner case "dois jogadores no mesmo item" |
| PA-01-04 | C — Erro de lógica · 🟢 | Linhas corrigidas na spec; no código, os dois logs `#if DEBUG` do bloco `inOutHandsProcess` foram consolidados em um único log (a decisão agora avalia as duas checagens juntas via `collidesViaHands`) — desvio menor documentado no checklist da spec técnica |
| PA-01-05 | B — Edge case · 🟢 | Risco aceito sem mitigação de código adicional (documentado em §7 da spec técnica) |

## Pendências para fechar o item (fora do escopo do `/code-mod`)

- [ ] Instrumentação temporária + calibração de `GraceWindowSeconds` (hoje `0.35f`, marcado `TODO confirmar` no código) via teste real em Headless dedicado.
- [ ] `/compile-mod FIKA` — compilar `Fika.Core.dll`, propagar para `Fika-Headless/References/`, recompilar `Fika.Headless.dll`.
- [ ] Validação in-game dos 4 cenários da spec funcional + os 2 corner cases adicionais do checklist técnico (saque de arma real / dois jogadores no mesmo item).
- [ ] `/code-review` deste item.

## Mudanças posteriores

> Atualizado por `/apply-code-review` a cada rodada. Cada entrada lista os achados aplicados/rejeitados/pulados naquela rodada e os arquivos tocados.

**Rodada 01 (2026-09-06):** 2 achados aplicados, 0 rejeitados.
- **CR-01-01** (🟠, Arquitetura): `FikaPlugin.cs` — registro explícito via `_patchManager.EnablePatch(...)` para os 2 `ModulePatch` aninhados de `InOutHandsProcessTimestampPatch`, eliminando a dependência de auto-discovery não verificado para tipos aninhados.
- **CR-01-02** (🟢, Legibilidade): `003-magazine-swap-inplace-fix-02-spec-tech.md` §2/§5 — bloco `CaptureMovedItemOnSet` (descartado) anotado com nota explícita apontando para este as-built, em vez de deixar o stub divergente sem aviso.

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-06 | Build concluído via `/code-mod` |
| 2026-09-06 | Aplicação de 2 achados de code-review 01 via `/apply-code-review` — IDs: CR-01-01, CR-01-02 |
