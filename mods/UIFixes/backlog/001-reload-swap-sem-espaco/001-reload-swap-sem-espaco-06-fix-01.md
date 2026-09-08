# 001 — Fix 01 · Swap não funcionava quando 0% de espaço livre em qualquer lugar (motor nativo colide consigo mesmo)

**Mod:** UIFixes
**Item raiz:** [001-reload-swap-sem-espaco-01-spec.md](001-reload-swap-sem-espaco-01-spec.md)
**Asbuild:** [001-reload-swap-sem-espaco-05-asbuild.md](001-reload-swap-sem-espaco-05-asbuild.md)
**Criado:** 2026-09-08
**Disparado por:** Feedback in-raid do usuário — "apertar R pra recarregar sendo que o inventário está 100% lotado, não faz o swap"

## Contexto

O fix CR-01-01 (fallback via `magAddress`) resolveu o caso "colete/bolsos cheios, mas a mochila tinha vaga". O usuário testou um cenário mais extremo — **inventário 100% cheio, nenhuma vaga livre em lugar nenhum** — e reportou que o swap continuava não acontecendo (nem o carregador antigo ia pro rig, nem nada visível acontecia).

## Causa raiz

Diagnóstico com logs `[UIFixes-Diag]` temporários (`ReloadInPlacePatches.cs`, ainda presentes no código a pedido do usuário, que está coletando mais relatos de terceiros) confirmou: o `SwapIfNoSpacePatch.Prefix` **encontrava corretamente** a única vaga possível (`candidateAddress`) — mas essa vaga é, por definição, exatamente o espaço que o **próprio carregador novo** ocupa (não sobra mais nenhuma outra vaga quando o inventário está 100% cheio).

O motor nativo do jogo (`Player.FirearmController.GClass2006.Run`, chamado de dentro de `ReloadMag`/`FikaClientFirearmController.ReloadMag`) processa a troca em duas etapas sequenciais:

```
Move(carregadorAntigo, vestTargetAddress, ...)   // roda PRIMEIRO
Move(carregadorNovo, slotDaArma, ...)            // só roda DEPOIS
```

Quando `vestTargetAddress` é exatamente a vaga que o carregador novo ainda ocupa (porque o primeiro `Move` roda antes do segundo tirar o carregador novo dali), o motor colide consigo mesmo (`GridSpaceTakenError`) e o reload inteiro aborta silenciosamente — sem nenhum erro visível pro jogador. Isso é uma limitação de ordenação do próprio motor do EFT, não um bug introduzido por nós; **refuta a premissa original da spec técnica** (`001-reload-swap-sem-espaco-02-spec-tech.md`), que assumia que bastava popular `itemAddress` corretamente para o motor nativo completar a troca em qualquer cenário.

Comparação com `mods/UIFixes/original/src/Patches/ReloadInPlacePatches.cs` mostrou que o upstream (Tyfon) já havia resolvido exatamente esse problema, usando `InteractionsHandlerClass.Swap(...)` + `controller.TryRunNetworkTransaction(...)` em vez de deixar o motor nativo (`ReloadMag`) processar a troca — uma troca atômica não sofre do problema de ordenação sequencial.

Esse padrão (Swap cru num item da arma empunhada) havia sido **deliberadamente abandonado** numa sessão anterior deste mesmo mod (`mods/UIFixes/memory/sessions.md`, sessões v5.3.14–v5.3.21) por causar rejeição `GClass1561` ("is currently being modified") e travamento de mãos no FIKA Headless. Só que essa sessão é **anterior** ao fix `003-magazine-swap-inplace-fix` do FIKA (`mods/FIKA/backlog/003-magazine-swap-inplace-fix/`), que resolveu a causa raiz desse `GClass1561` no lado do Headless (`ObservedInventoryController.IsSelfReferentialMagazineSwap`) de forma genérica — cobre qualquer troca entre dois `MagazineItemClass` na mesma arma, independente de qual ação da UI disparou. A prova de que o Swap cru já é seguro hoje: `WeaponApplyPatch` (`SwapPatches.cs:840`, drag-and-drop de carregador em arma equipada) já usa esse mesmo padrão com sucesso confirmado em Fika.

## Mudanças aplicadas

| Arquivo | Mudança |
|---|---|
| `modded/src/Patches/ReloadInPlacePatches.cs` | `SwapIfNoSpacePatch.Prefix`: quando `candidateAddress` (achado pela busca ampla ou pelo fallback CR-01-01) é exatamente igual a `magAddress` (a vaga do próprio carregador novo — o caso "0% de espaço livre"), usa `InteractionsHandlerClass.Swap(magazine, weaponMagSlotAddress, currentMagazine, candidateAddress, controller, true)` + `controller.TryRunNetworkTransaction(swapResult, callback)`, retornando `false` (pula o `ReloadMag` nativo). Para qualquer outro caso (vaga livre genuinamente diferente do slot do carregador novo), mantém o `Move`/`return true` já existente — inalterado. |
| `modded/src/Patches/ReloadInPlacePatches.cs` | Replicado manualmente `____player.RemoveLeftHandItem(3f)` + `AnimatedInteractions.ForceStopInteractions()` + checagem de `IsInteractionPlaying`/`CanStartReload()` antes do Swap — sem isso a troca acontecia instantânea, sem a animação/tempo de "guardar o carregador velho no colete com a mão" que o `ReloadMag` nativo dispara (via `GClass2016`/`OnMagPuttedToRig`) e que o `original` upstream também replica manualmente pelo mesmo motivo. |
| `modded/Shared.props`, `mod.json` | Bump `<Version>`/`version`: `5.3.24` → `5.3.26` (duas iterações: Swap atômico, depois pacing da animação) |

## Limitação conhecida (aceita)

O Swap atômico pula o `GClass2016` (state machine de animação nativa) por completo. A `RemoveLeftHandItem(3f)` restaura a sensação de tempo/pacing, mas a troca em si continua sendo aplicada de uma vez (sem os eventos intermediários do Animator como `OnMagPulledOutFromWeapon`/`OnMagAppeared` disparando efeitos visuais próprios). Investigado como alternativa: reescrever `GClass2006.Run` via transpiler pra fazer a troca dentro do próprio pipeline nativo — descartado por risco de regressão e fragilidade contra atualizações do jogo (decisão do usuário). Validado in-game pelo usuário como aceitável — a animação percebida de "recarga penalizada" está presente (confirmado 2026-09-08 após reteste com build atualizada).

## Checklist de validação

- [x] Compila via `dotnet build` sem erros (`mods/UIFixes/builds/Tyfon.UIFixes.dll`, v5.3.26)
- [x] **In-raid:** swap confirmado funcionando pelo usuário, com animação/tempo de recarga penalizada presente
- [x] **Fika/multiplayer:** testado em sessão Fika Host + Headless conectado — log do Headless sem `GClass1561`/"currently being modified"; `Swap result: Succeeded=True`
- [ ] **raid1 → exit → raid2:** não testado explicitamente nesta rodada
- [ ] **alt-F4 / morte / MIA:** não testado explicitamente nesta rodada
- [x] Memória do mod atualizada (`mods/UIFixes/memory/sessions.md`, Sessão 9)

## Histórico

| Data | Evento |
|---|---|
| 2026-09-08 | Fix criado, diagnosticado e validado in-game (Fika Host + Headless) |
