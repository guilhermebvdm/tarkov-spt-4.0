# 025 — Fix 01 · Mãos travadas (`HandsController` nulo) ao repetir o drop de emergência rápido demais

> ⚠️ **SUPERADO por [Fix 02](025-emergencydrop-autocirurgia-06-fix-02.md) (2026-09-12).** O guard de reentrância abaixo (`_weaponEquipPending`) se mostrou **insuficiente**: o usuário reproduziu o mesmo travamento sem nenhuma repetição de ação (um único drop de emergência já causava a arma sendo puxada e guardada sozinha), provando que a causa raiz não era o nosso código reentrando — era a própria criação de um `Process<>` assíncrono nativo por `TrySetLastEquippedWeapon`, que colide com QUALQUER operação de mãos nativa (não só a nossa) durante a janela de risco. O Fix 02 remove a chamada por completo em vez de tentar proteger a janela. Este documento é mantido como registro histórico (append-only) — não editar, ver Fix 02 para a versão vigente.

**Mod:** TRL-ImmersiveCombatMedicine
**Item raiz:** [025-emergencydrop-autocirurgia-01-spec.md](025-emergencydrop-autocirurgia-01-spec.md) (também afeta [024-acelerar-fechamento-kit-emergencydrop](../024-acelerar-fechamento-kit-emergencydrop/), mesmo mecanismo)
**Asbuild:** [025-emergencydrop-autocirurgia-05-asbuild.md](025-emergencydrop-autocirurgia-05-asbuild.md)
**Criado:** 2026-09-12
**Disparado por:** Feedback in-raid do usuário durante validação (pendência P-13.2)

## Contexto

Usuário testou o drop de emergência em auto-cirurgia (item 025) repetindo o ciclo "usar CMS → apertar F pra dropar → pegar o item do chão → usar de novo" três vezes seguidas. Na 3ª repetição (feita mais rápido que as duas primeiras, a julgar pelo intervalo entre logs), o personagem ficou com as mãos permanentemente vazias — sem arma, sem item, impossível de recuperar apertando qualquer tecla — com este erro se repetindo a cada frame no console:

```
[Error : Unity Log] NullReferenceException: Object reference not set to an instance of an object
Stack trace:
EFT.MovementContext+Class1396.method_3 (EFT.Player player) (at <...>:0)
Fika.Core.Main.Players.FikaPlayer.MouseLook (...)
(wrapper dynamic-method) EFT.Player.DMD<EFT.Player::VisualPass>(EFT.Player)
(wrapper dynamic-method) EFT.Player.DMD<EFT.Player::LateUpdate>(EFT.Player)
```

Um sintoma menor relacionado também foi relatado antes deste crash: em um teste anterior, a arma aparentemente "foi puxada e removida logo em seguida", deixando as mãos vazias sem erro no console (recuperável reequipando manualmente) — mesmo mecanismo, versão mais branda.

## Causa raiz

`MovementContext.Class1396.method_3` ([`MovementContext.cs:54-62`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/MovementContext.cs#L54-L62)), chamado todo frame via `MouseLook`/`VisualPass`/`LateUpdate`, desreferencia `player.HandsController.ControllerGameObject.transform` **sem checar `null`**:
```csharp
public void method_3(Player player)
{
    if (!player.UsedSimplifiedSkeleton)
    {
        Quaternion handsRotation = player.HandsRotation;
        player.HandsController.ControllerGameObject.transform.SetPositionAndRotation(...);
        ...
    }
}
```
O `NullReferenceException` em loop confirma que `Player.HandsController` ficou `null` permanentemente. A causa: `Player.TrySetLastEquippedWeapon()` (chamado por `EmergencyDrop`/`EmergencyDropSelf` logo após `DestroyController()`) não é instantâneo por dentro — ele cria um `Process<TController,TResult>` cujo `CreateController()` ([`Player.cs:22540-22574`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L22540-L22574)) chama `Player.SpawnController(...)`, uma operação **assíncrona**. Enquanto essa operação está em voo, `Player.ProcessStatus` fica em `EProcessStatus.Scheduled` e só volta a `None` (ou dispara a próxima operação enfileirada) quando o callback do `SpawnController` finalmente confirma, via `ExecuteNext()` ([`Player.cs:22567`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L22567)).

Se `EmergencyDrop`/`EmergencyDropSelf` disparar de novo **antes** desse callback confirmar (repetição rápida demais), a nova tentativa de troca de mãos entra no `method_0` de agendamento ([`Player.cs:22462-22475`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L22462-L22475)) enquanto `ProcessStatus` ainda está `Scheduled` — nesse caso a operação é **enfileirada** (`Player.AbstractProcess_0 = this`) em vez de executar na hora, e dependendo do estado da operação anterior pode acabar não rodando nunca de forma limpa, deixando `HandsController` preso em `null`.

**Nota sobre uma hipótese alternativa descartada:** o usuário também levantou a hipótese de que o consumo de carga do item fosse aplicado com atraso (não na hora do cancelamento), e que isso pudesse causar o bug. Investigação direta em [`ActiveHealthController.cs:550-561`](../../../../references/eft-decompiled/Assembly-CSharp/EFT.HealthSystem/ActiveHealthController.cs#L550-L561) (`ForceResidue()` → `method_0()` → `Residue()`, todas chamadas **na mesma pilha de chamada, sem delay**) confirmou que o consumo é síncrono e gated apenas por um limiar de tempo mínimo de uso (`ItemRemoveAfterInterruptionTime`) — não uma espera de execução. Essa hipótese foi descartada; **não** é a causa deste bug.

## Mudanças aplicadas

| Arquivo | Mudança |
|---|---|
| `modded-V4/Patches/Medical/BandAidController.cs` | Novo campo `_weaponEquipPending` (+ `_weaponEquipPendingSince`, timeout de 5s): setado `true` logo antes de cada chamada a `TrySetLastEquippedWeapon` (em `EmergencyDrop` e `EmergencyDropSelf`), limpo `false` dentro do callback dela. Gate de `Update()` passa a exigir `!_weaponEquipPending` antes de permitir um novo drop de emergência — impede a colisão de processos que deixava `HandsController` preso em `null`. Reset em `ResetAllState()`. |
| `modded-V4/TRL-ImmersiveCombatMedicine.csproj` | Bump de versão `1.14.3` → `1.14.4`. |
| `modded-V4/TRLImmersiveCombatMedicinePlugin.cs` | `BepInPlugin` version string `1.14.3` → `1.14.4`. |

**Risco residual documentado (não resolvido por este fix):** a trava só impede que o **próprio mod** dispare um novo `EmergencyDrop(Self)` durante a janela de risco — não impede o jogador de disparar outra operação de mãos por uma via nativa diferente (ex.: trocar de arma manualmente, usar outro item) durante essa mesma janela de ~alguns frames. Esse risco é inerente ao design assíncrono nativo do jogo e exigiria patchear os handlers de input nativos pra fechar por completo — fora do escopo deste fix, que resolve o caminho que foi de fato reproduzido.

## Checklist de validação (obrigatório antes de marcar o fix como entregue)

- [x] Compila via `dotnet build -c Release` sem erros (0 Erros, 0 Warnings)
- [ ] **In-raid:** repetir o cenário exato do bug reportado (3 ciclos rápidos de usar CMS + drop de emergência) e confirmar que as mãos não travam mais
- [ ] **In-raid:** confirmar que apertar a tecla durante a janela de "troca em andamento" é silenciosamente ignorado (sem erro, sem travar), não gera comportamento estranho
- [ ] **Fika/multiplayer:** sem regressão com outros players — N/A ainda não testado, mas mudança é estritamente local (variável de instância do `MainPlayer`)
- [ ] **raid1 → exit → raid2:** `_weaponEquipPending` resetado em `ResetAllState()`, sem estado vazado
- [ ] **alt-F4 / morte / MIA:** teardown idempotente — N/A, guard é só uma flag local sem coroutine/recurso a liberar
- [ ] Memória do mod atualizada com a lição do fix

## Histórico

| Data | Evento |
|---|---|
| 2026-09-12 | Fix criado após bug reportado em raid real (mãos travadas, `NullReferenceException` em loop). Causa raiz identificada por leitura direta do Assembly (`Process<>.CreateController`/`SpawnController` assíncrono, `MovementContext.Class1396.method_3` sem null-check). Guard de reentrância implementado e buildado (v1.14.4). Validação in-game pendente. |
