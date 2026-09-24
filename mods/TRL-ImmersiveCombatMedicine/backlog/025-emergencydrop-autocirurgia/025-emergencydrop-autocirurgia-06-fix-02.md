# 025 — Fix 02 · Remover `TrySetLastEquippedWeapon()` do EmergencyDrop (fix-01 insuficiente)

**Mod:** TRL-ImmersiveCombatMedicine
**Item raiz:** [025-emergencydrop-autocirurgia-01-spec.md](025-emergencydrop-autocirurgia-01-spec.md) (também afeta [024-acelerar-fechamento-kit-emergencydrop](../024-acelerar-fechamento-kit-emergencydrop/) e a confirmação de callback do [023-sequencia-maos-cura-sem-confirmacao](../023-sequencia-maos-cura-sem-confirmacao/))
**Asbuild:** [025-emergencydrop-autocirurgia-05-asbuild.md](025-emergencydrop-autocirurgia-05-asbuild.md)
**Criado:** 2026-09-12
**Disparado por:** Feedback in-raid do usuário — o guard de reentrância do Fix 01 não resolveu o problema; usuário reproduziu **dois sintomas distintos** e propôs remover a chamada por completo como experimento.

## Contexto

Depois do Fix 01 (guard `_weaponEquipPending`), o usuário reportou que o problema **continuava**, em duas situações distintas:

- **Situação 1 (sem repetição nenhuma):** um único acionamento do drop de emergência — CMS cai no chão, a arma é puxada pra frente, e **sozinha** (sem nenhuma ação do jogador no meio) é guardada de novo, deixando o personagem sem nada nas mãos por mais de 1 minuto (o usuário esperou de propósito pra descartar a hipótese de "tempo invisível" pendente). Só voltou ao normal apertando manualmente a tecla de arma.
- **Situação 2 (repetição rápida):** repetir o ciclo "drop de emergência → pegar o item do chão → usar de novo → drop de emergência de novo" **rápido demais** reproduz o travamento total de mãos (`HandsController` nulo, `NullReferenceException` em loop) já documentado no Fix 01.

A Situação 1 é a mais reveladora: como não há repetição de ação nenhuma, o guard de reentrância do Fix 01 (que só protege contra o **nosso próprio** código disparar `EmergencyDrop`/`EmergencyDropSelf` de novo) nunca poderia ter prevenido isso — o problema está na própria sequência de uma única chamada.

## Causa raiz

`Player.TrySetLastEquippedWeapon()` não é um simples "setter" — ele cria um `Process<TController,TResult>` de verdade ([`Player.cs:31800-31816`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L31800-L31816) → `TryProceed` → `CreateController()`, [`Player.cs:22540-22574`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L22540-L22574)), cujo passo final (`Player.SpawnController(...)`) é **assíncrono**. Enquanto essa arma está sendo instanciada de verdade, `Player.ProcessStatus` fica em `EProcessStatus.Scheduled` — e só volta a `None` (ou dispara a próxima operação enfileirada) quando o callback do `SpawnController` finalmente confirma, via `ExecuteNext()` ([`Player.cs:22693-22710`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L22693-L22710)):

```csharp
public override void ExecuteNext()
{
    if (Player_0.ProcessStatus == EProcessStatus.Scheduled)
    {
        AbstractProcess abstractProcess_ = Player_0.AbstractProcess_0;
        if (abstractProcess_ != null)
        {
            Player_0.AbstractProcess_0 = null;
            AbstractProcess.Execute(abstractProcess_);   // dispara processo ENFILEIRADO automaticamente
        }
        else { Player_0.ProcessStatus = EProcessStatus.None; }
    }
}
```

Se **qualquer** operação de mãos nativa (não necessariamente vinda do nosso mod — pode ser o próprio jogador reusando o item médico) for iniciada enquanto `ProcessStatus` ainda está `Scheduled`, ela entra no ramo de enfileiramento do agendador ([`method_0`, `Player.cs:22462-22475`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L22462-L22475)) em vez de rodar na hora: `Player.AbstractProcess_0 = <nova operação>`. Quando o `Process` da nossa arma finalmente confirma e chama `ExecuteNext()`, essa operação enfileirada dispara **sozinha**, automaticamente — e como as mãos nesse momento têm a ARMA (recém-criada por nós), essa operação enfileirada a **desmonta** pra montar o que quer que tivesse sido enfileirado, deixando as mãos vazias sem nenhuma ação explícita do jogador. Isso explica a Situação 1 sem precisar de nenhuma repetição visível: o "algo" que fica enfileirado pode vir do próprio ciclo de vida da cirurgia (ex.: o corpo residual da operação original de `SetInHands` do item médico, que só termina de se resolver depois do nosso `DestroyController()`/`TrySetLastEquippedWeapon()` já estarem em andamento).

**Fix 01 (guard `_weaponEquipPending`) era insuficiente porque** ele só impedia o **nosso próprio código** de chamar `EmergencyDrop`/`EmergencyDropSelf` de novo durante a janela de risco — não impedia (nem poderia impedir facilmente) qualquer outra operação nativa de mãos, iniciada por qualquer caminho, de colidir com o `Process` pendente da nossa arma. A raiz do problema é o próprio **ato de criar esse `Process`** — a solução mais robusta é não criá-lo.

## Mudanças aplicadas

| Arquivo | Mudança |
|---|---|
| `modded-V4/Patches/Medical/BandAidController.cs` | `TrySetLastEquippedWeapon()` **removido** de `EmergencyDrop()` e `EmergencyDropSelf()`. Campos do Fix 01 (`_weaponEquipPending`, `_weaponEquipPendingSince`, `WEAPON_EQUIP_PENDING_TIMEOUT`) removidos por não terem mais função (nenhum `Process` assíncrono é mais criado por esses métodos). Comentários dos dois métodos atualizados explicando a causa raiz e a troca de comportamento. |
| `modded-V4/TRL-ImmersiveCombatMedicine.csproj` | Bump de versão `1.14.4` → `1.14.5`. |
| `modded-V4/TRLImmersiveCombatMedicinePlugin.cs` | `BepInPlugin` version string `1.14.4` → `1.14.5`. |

## Mudança de comportamento (aceita pelo usuário, experimento proposto por ele mesmo)

Depois do drop de emergência (aliado ou auto-cirurgia), as mãos ficam **livres** instantaneamente, mas **sem reequipar a arma automaticamente**. O jogador precisa apertar a própria tecla de arma se quiser puxá-la de volta — troca deliberada de "arma pronta sozinha" (promessa original dos itens 023/024/025) por "mãos livres, sem risco de travar", já que a auto-restauração da arma era a fonte direta dos dois bugs reproduzidos.

**Impacto nas specs anteriores:** os itens 023/024/025 prometiam "a arma está pronta pra atirar no mesmo instante" como critério de aceite — isso deixa de ser verdade a partir deste fix. Ver Histórico das respectivas specs funcionais para a nota de revisão.

## Checklist de validação (obrigatório antes de marcar o fix como entregue)

- [x] Compila via `dotnet build -c Release` sem erros (0 Erros, 0 Warnings)
- [ ] **In-raid — Situação 1:** único drop de emergência (aliado e auto-cirurgia), confirmar que as mãos ficam vazias e **estáveis** (sem puxar/guardar arma sozinha), esperando pelo menos 1 minuto sem agir
- [ ] **In-raid — Situação 2:** repetir o ciclo rápido (drop → pegar → usar → drop de novo) e confirmar que não trava mais, mesmo repetindo várias vezes seguidas
- [ ] **Fika/multiplayer:** sem regressão com outros players — mudança é estritamente local
- [ ] **raid1 → exit → raid2:** sem estado novo introduzido (campos do fix-01 removidos, nada a vazar)
- [ ] **alt-F4 / morte / MIA:** N/A — sem coroutine/recurso a liberar
- [ ] Memória do mod atualizada com a lição do fix

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-12 | Fix 02 criado depois do Fix 01 se mostrar insuficiente (usuário reproduziu o bug de duas formas distintas, incluindo sem nenhuma repetição). Causa raiz revisada: o problema não era reentrância do NOSSO código, era a própria criação de um `Process<>` assíncrono nativo pelo `TrySetLastEquippedWeapon`. Removida a chamada por completo — experimento proposto pelo próprio usuário. Build verde, v1.14.5. Validação in-game pendente. |
