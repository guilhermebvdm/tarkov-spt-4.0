# 010 — Fix 01 · Afinidade de CPU não é restaurada de forma confiável ao sair da raid

**Mod:** TRL-CoreSight
**Item raiz:** [010-afinidade-cpu-topologia-threads-01-spec.md](010-afinidade-cpu-topologia-threads-01-spec.md)
**Asbuild:** [010-afinidade-cpu-topologia-threads-05-asbuild.md](010-afinidade-cpu-topologia-threads-05-asbuild.md)
**Criado:** 2026-09-19
**Disparado por:** feedback in-raid do usuário (log real de duas raids, `E:\Tarkov Red Line - SERVER TEST\BepInEx\LogOutput.log`)

## Contexto

Após fechar o item 010 (code review 01 aplicada, compilado e instalado como v0.4.17), o usuário rodou duas raids reais para validar:

1. **Raid 1 — Alt+F4 no meio da raid.** A mensagem `"[TRL-CoreSight][CPU] Afinidade restaurada para o padrão do Windows."` não apareceu no log. Hipótese inicial (registrada na conversa): explicável por interrupção abrupta do processo.
2. **Raid 2 — raid completa até o fim, retorno normal ao menu.** A mesma mensagem **continuou ausente**, mesmo com o log seguindo por mais de 1000 linhas após o `"Cleanup de raid concluído."` (carregamento de missões no menu, etc.) e **zero exceções** em qualquer lugar do arquivo. Isso descartou a hipótese do Alt+F4 — o problema é estrutural, não um artefato de captura de log.

`Afinidade de processo aplicada!` apareceu corretamente 4 vezes ao longo da raid 2 (início + 3 reaplicações), com a máscara certa (`0x5555`, 8 físicos de 16 lógicos) — ou seja, a aplicação da afinidade sempre funcionou; só a confirmação de restauração no fim nunca apareceu, em nenhuma das duas raids.

## Causa raiz

`GameWorld.OnDestroy` é patcheado com `[PatchPrefix]` em [`RaidLifecyclePatches.cs:54-66`](../../modded/Patches/RaidLifecyclePatches.cs#L54), que chama manualmente `PerformanceManager.Instance?.Cleanup()`. Só que `PerformanceManager` também é um `MonoBehaviour` anexado ao **mesmo GameObject** do `GameWorld`, e por isso a própria Unity também dispara seu `OnDestroy()` nativo (`PerformanceManager.cs:365-368`: `private void OnDestroy() { Cleanup(); }`) como parte do mesmo processo de destruição — **sem nenhuma garantia de ordem entre os dois disparos, e sem proteção de reentrância**.

Isso significa que `Cleanup()` roda **duas vezes** a cada fim de raid. A segunda execução repete a sequência inteira — incluindo `_shadowWatcher.Restore(); Destroy(_shadowWatcher);`, `_botLimiter.RestoreAll(); Destroy(_botLimiter);` etc. — só que agora sobre sub-managers que já foram destruídos pela primeira passagem. Diferente do `Initialize()` (que embrulha cada sub-manager em `try/catch`), o `Cleanup()` original não tinha nenhuma proteção por etapa: uma exceção do tipo `MissingReferenceException`/`NullReferenceException` ao acessar um objeto Unity já destruído interrompe o método imediatamente — e como esse `OnDestroy()` nativo do Unity não tem try/catch nenhum ao redor, a exceção é tratada internamente pelo motor da Unity, que a registra no log de player da própria engine (não necessariamente no `LogOutput.log` do BepInEx, dependendo da configuração do `UnityLogListener`). Como `CpuTopologyManager.RestoreOriginalAffinity()` estava posicionado **depois** de todos os `Destroy()` de sub-managers no método, essa exceção silenciosa impedia a linha de restauração de ser alcançada em uma das duas execuções — e, dependendo de qual delas falhava, também na outra (a ordem de disparo entre os dois `OnDestroy()` não é determinística).

Este é exatamente o antipadrão descrito em `spt-mod-best-practices` §2 ("Stop hook — Make `RaidSession.End()` idempotent — guard with a `bool _ended`"), que não foi aplicado quando o `Cleanup()` original deste mod foi escrito (antes do item 010) nem quando o item 010 adicionou a chamada de restauração de afinidade.

## Mudanças aplicadas

| Arquivo | Mudança |
|---|---|
| `modded/Core/PerformanceManager.cs` | Adicionado guard de reentrância `_cleanedUp` no topo de `Cleanup()` — segunda chamada agora é no-op. Movido `CpuTopologyManager.RestoreOriginalAffinity()` para a primeira linha executável do método (antes de qualquer sub-manager que possa lançar exceção), garantindo que a afinidade do processo/thread seja sempre restaurada independente do que aconteça no resto do teardown. |

## Checklist de validação (obrigatório antes de marcar o fix como entregue)

- [x] Compila via `/compile-mod` sem erros (verificado via `dotnet build` direto; instalação formal pendente do próximo `/compile-mod`)
- [ ] **In-raid:** comportamento corrigido observado em raid real — pendente: usuário validar que `"Afinidade restaurada"` aparece agora ao sair de uma raid completa
- [ ] **Fika/multiplayer:** sem regressão com outros players — N/A: afinidade de CPU é 100% local ao processo do cliente, não depende de outros peers
- [ ] **raid1 → exit → raid2:** sem estado vazado entre raids — pendente validação
- [ ] **alt-F4 / morte / MIA:** teardown idempotente, sem exceção no LogOutput.log — pendente validação (o cenário original que expôs o bug)
- [ ] Memória do mod atualizada (`/update-memory`) com a lição do fix

## Histórico

| Data | Evento |
|---|---|
| 2026-09-19 | Fix criado a partir de feedback in-raid (2 raids reais, log analisado) |
| 2026-09-19 | Correção aplicada (guard de reentrância + reordenação); recompilado localmente com 0 erros e 0 avisos |
