# 002 — Drop de Arma na Morte + Capacete/Óculos no Desmembramento de Cabeça · Code Review 03

**Mod:** VisceralCombat
**Spec funcional:** [002-drop-arma-capacete-oculos-cabeca-01-spec.md](002-drop-arma-capacete-oculos-cabeca-01-spec.md)
**Spec técnica:** [002-drop-arma-capacete-oculos-cabeca-02-spec-tech.md](002-drop-arma-capacete-oculos-cabeca-02-spec-tech.md)
**Asbuild:** [002-drop-arma-capacete-oculos-cabeca-05-asbuild.md](002-drop-arma-capacete-oculos-cabeca-05-asbuild.md)
**Data:** 2026-09-20

> Análise crítica do código implementado por `/code-mod`. Cada achado recebe um ID `CR-03-MM` permanente. Resolver bloqueadores 🔴 via `/apply-code-review` antes de fechar o item.
>
> Terceira rodada — cobre a arquitetura unificada de drop síncrono (`DeathInventoryDropPatch`, `ActiveHealthController.method_35`) já revisada e corrigida nas rodadas 01/02, mais o trabalho feito **fora** do fluxo formal desde então (reformulação completa via `CR-NET-LOCK-01`, correções de "objeto visual órfão" `CR-HEAD-PROP-GHOST-01`/`CR-WEAPON-GHOST-01`/`CR-HANDS-DISPOSE-01`, todas já documentadas no asbuild — ver pendência `[P-10.1]`) e as mudanças aplicadas pela rodada 02 (`CR-02-01` a `CR-02-06`, 2026-09-14, sessão paralela).

**Memória consultada:** snapshot de 2026-09-11 (Sessão 11), `mods/VisceralCombat/memory/sessions.md`. **Pendências que afetam esta revisão:** `[P-10.1]` (🔴 — motivo desta rodada existir), `[P-10.2]` (🟡 — hardening do ragdoll, não relacionado ao diff revisado aqui). **Docs técnicos conferidos:** `spt-antipatterns.md` (sempre — AP-02, AP-04, AP-09 relevantes), `fika-packet-desync-prevention-plan.md` (nenhum pacote novo introduzido neste diff).

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 2 · Total: 2

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-03-01 | A — Crítico | 🔴 Bloqueador | `LimbKillPatch.DropCorpseHeadEquipment` reproduz o mecanismo já comprovado quebrado (`item.Owner` sobre corpo já criado) e roda sem gate de autoridade em coop | ✅ Aplicado em 2026-09-20 |
| CR-03-02 | F — Melhoria opcional | 🟡 Médio | `ResolveAndDropHeadEquipment` grava uma entrada em `PendingHeadOutcome` mesmo para mortes sem relação com a cabeça | ✅ Aplicado em 2026-09-20 |

## Categorias

- **A — Crítico** — bug grave, crash garantido, corrupção de estado, security issue.
- **B — Bug latente** — comportamento errado em cenário plausível, não acionado pelo caminho golden.
- **C — Gap vs. spec** — código não implementa critério de aceite, corner case, ou AC da spec.
- **D — Arquitetura** — viola padrões do repo, duplica código, leak de estado, abuso de reflection.
- **E — Legibilidade/manutenção** — nomes ruins, comentário "porquê" ausente, código morto, complexidade desnecessária.
- **F — Melhoria opcional** — refactor de qualidade, micro-otimização, simplificação.

## Impacto

- 🔴 **Bloqueador** — fix obrigatório antes de fechar o item.
- 🟠 **Forte** — fix recomendado; pode ser deferido para `06-fix-NN.md` futuro.
- 🟡 **Médio** — anotar, decidir caso a caso.
- 🟢 **Menor** — opcional.

---

## Pontos

### CR-03-01 · A — Crítico · 🔴 Bloqueador · ✅ Aplicado em 2026-09-20

**`LimbKillPatch.DropCorpseHeadEquipment` reproduz o mecanismo já comprovado quebrado (`item.Owner` sobre corpo já criado) e roda sem gate de autoridade em coop**

**Local:** [`mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/LimbKillPatch.cs:283-311`](../../modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/LimbKillPatch.cs#L283-L311) (chamado em [:264](../../modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/LimbKillPatch.cs#L264) e [:270](../../modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/LimbKillPatch.cs#L270), adicionado por `CR-02-04`)

**Problema:**
```csharp
// Cadáveres usam GClass3385 (TraderControllerClass) como dono do equipamento.
TraderControllerClass controller = (helmet?.Owner as TraderControllerClass)
                                ?? (eyewear?.Owner as TraderControllerClass)
                                ?? (player.InventoryController as TraderControllerClass);

if (controller != null)
{
    if (helmet != null) controller.ThrowItem(helmet, false, null);
    if (eyewear != null) controller.ThrowItem(eyewear, false, null);
}
```
Dois problemas compostos, ambos com precedente **já comprovado real nesta mesma sessão** (ver `mods/VisceralCombat/memory/sessions.md`, Sessão 10, achado `CR-NET-LOCK-01`):

1. **Mesmo padrão `item.Owner` que já quebrou uma vez.** Esse método roda em resposta a um tiro **pós-morte** (a "Dead corpses branch" de `ProcessLimbKill`, chamada quando o cadáver já existe há tempo) — ou seja, `helmet?.Owner`/`eyewear?.Owner` **sempre** resolve para o `GClass3385` do cadáver (`TraderControllerClass` sem override de `vmethod_1`, sem integração com o Fika). A investigação desta sessão provou, com log real de usuário, que chamar `ThrowItem` sobre esse controller específico deixa o item travado num estado "is currently being modified"/`PlayerIsBusyError` — inlootiável, sem completar a operação de rede — exatamente a razão pela qual `DeathInventoryDropPatch` (arma/capacete **na morte**) foi reformulado pra rodar em `ActiveHealthController.method_35`, **antes** do cadáver existir, evitando esse controller por completo. `DropCorpseHeadEquipment` não tem esse recurso disponível — o cadáver já existe muito antes de um tiro pós-morte acontecer — então precisa de um mecanismo **diferente**, ainda não investigado, não de uma reaplicação do padrão já descartado.
2. **Nenhum gate de autoridade.** `ProcessLimbKill` (chamado por `VisceralShotProcessor.WatchShotCoroutine`, [`VisceralShotProcessor.cs:51`](../../modded/VisceralCombat/VisceralCombat.Combined.Classes/VisceralShotProcessor.cs#L51)) não tem, em nenhum ponto do caminho até `DropCorpseHeadEquipment`, uma checagem `FikaBackendUtils.IsServer || FikaBackendUtils.IsSinglePlayer` — ao contrário de **todo** outro ponto de mutação de inventário deste mod (`DeathInventoryDropPatch.cs:75`, `ShootOffHelmetPatch.cs:27`). Isso é apropriado pros outros efeitos dessa branch (`DismemberLimb` pra torso/braço/perna — puramente visual, cada peer aplica localmente e não há problema em rodar em todos) mas **não** para esta chamada especificamente, que é a única mutação real de inventário no meio de efeitos cosméticos. Em coop, **cada peer** que processa o tiro localmente chamaria `ThrowItem` pro mesmo item, simultaneamente — mesmo classe de corrida que causou os bugs de duplicação/travamento documentados nesta sessão, agora multiplicada por N peers em vez de 1.

**Por que importa:** Qualquer tiro de cabeça em um cadáver já morto, em coop, com `DropHeadEquipmentOnDismemberment` ligado (padrão `true`), é um caminho plausível e comum neste mod focado em desmembramento — não é um edge case raro. A combinação dos dois problemas tem alta chance de reproduzir, pro capacete/óculos, exatamente os sintomas reais já relatados pelo usuário pra arma antes do `CR-NET-LOCK-01`: item preso no cadáver, inlootiável, ou capturado por múltiplos peers ao mesmo tempo.

**Sugestão:** Duas ações, uma imediata e barata, outra que precisa de investigação antes de codar (mesmo padrão que resolveu `CR-NET-LOCK-01` — não reagir com outro patch às cegas):
1. **Imediato:** adicionar o gate de autoridade que falta, consistente com todo o resto do mod:
   ```csharp
   if (!(FikaBackendUtils.IsServer || FikaBackendUtils.IsSinglePlayer)) return;
   ```
   no início de `DropCorpseHeadEquipment` (precisa de `using Fika.Core.Main.Utils;` no arquivo, ainda não importado — conferir). Isso sozinho elimina a corrida entre múltiplos peers, mas **não** resolve o problema 1.
2. **Precisa de investigação (não aplicar às cegas):** o mecanismo de remoção segura de item de um cadáver **já existente** (diferente do caso "antes da morte" que `DeathInventoryDropPatch` resolve) continua sem solução comprovada neste repo — a investigação original desta sessão sobre isso foi interrompida quando o usuário pediu pra focar no caso "na morte" primeiro. Até essa investigação acontecer, a opção mais segura é **desativar a chamada** (comentar/remover as duas linhas `DropCorpseHeadEquipment(player);` em `LimbKillPatch.cs:264` e `:270`, mantendo o resto do efeito visual de `DismemberLimb` intacto) — o capacete/óculos simplesmente continuam no cadáver quando ele é desmembrado post-mortem (comportamento pré-`CR-02-04`, conhecido e seguro), até uma spec técnica dedicada decidir o mecanismo certo.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Sugestão aplicada — as duas partes.
**Aplicação:**
- `mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/LimbKillPatch.cs:264-278` — as duas chamadas `DropCorpseHeadEquipment(player);` (ramos `HeadOff`/`HeadBurst`) comentadas, com explicação inline do motivo (mesmo mecanismo `item.Owner` já comprovado quebrado, sem truque de "antes da criação do cadáver" disponível pra este caso). O resto do efeito visual (`DismemberLimb`) continua intacto.
- `mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/LimbKillPatch.cs:283-296` — `using Fika.Core.Main.Utils;` adicionado; `DropCorpseHeadEquipment` ganhou o gate `FikaBackendUtils.IsServer || FikaBackendUtils.IsSinglePlayer` que faltava, mantido implementado (mas não chamado) pra quando um mecanismo seguro for investigado.
- Build confirmado (`dotnet build`, Release — Build succeeded).

---

### CR-03-02 · F — Melhoria opcional · 🟡 Médio · ✅ Aplicado em 2026-09-20

**`ResolveAndDropHeadEquipment` grava uma entrada em `PendingHeadOutcome` mesmo para mortes sem relação com a cabeça**

**Local:** [`mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Combined.Patches/DeathInventoryDropPatch.cs:117-129`](../../modded/VisceralCombat/VisceralCombat.Combined.Patches/DeathInventoryDropPatch.cs#L117-L129)

**Problema:**
```csharp
if (!IsBallisticOrExplosiveDamage(damageType))
{
    KillPatch.PendingHeadOutcome[player.Id] = KillPatch.HeadDismemberOutcome.None;
    return;
}

EBodyPart lastBodyPart = (EBodyPart)_lastBodyPartField.GetValue(player);
if (lastBodyPart != EBodyPart.Head) return;
```
O early-return de `IsBallisticOrExplosiveDamage` grava `None` no dicionário **antes** de checar se `LastBodyPart` sequer é `Head`. Ou seja, toda morte por dano não-balístico (fome, desidratação, sangramento residual, queda) grava uma entrada em `PendingHeadOutcome` — mesmo quando a morte não teve nada a ver com a cabeça. A entrada nunca é lida (o Postfix só consulta o cache quando `bodyPartType == 0`), então fica ocupando espaço no dicionário até `ClearPendingHeadOutcomes()` (`GameStartedPatch`) limpar no início da próxima raid — sem leak entre raids, só desperdício dentro da raid atual.

**Por que importa:** Puramente de eficiência/higiene — em uma raid com muitas mortes por causas não-balísticas (bots morrendo de sangramento após combate, por exemplo), o dicionário acumula entradas nunca lidas. Não causa bug nem comportamento incorreto observável.

**Sugestão:** Mover a checagem de `LastBodyPart != Head` para antes do early-return de tipo de dano (ou combinar as duas condições num único `if`), gravando `None` só quando a morte de fato envolveu a cabeça:
```csharp
EBodyPart lastBodyPart = (EBodyPart)_lastBodyPartField.GetValue(player);
if (lastBodyPart != EBodyPart.Head) return;

if (!IsBallisticOrExplosiveDamage(damageType))
{
    KillPatch.PendingHeadOutcome[player.Id] = KillPatch.HeadDismemberOutcome.None;
    return;
}
```

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Sugestão aplicada conforme proposto.
**Aplicação:** `mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Combined.Patches/DeathInventoryDropPatch.cs:117-129` — checagem de `LastBodyPart != Head` movida pra antes da checagem de tipo de dano. Build confirmado (`dotnet build`, Release — Build succeeded).

---

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-20 | Code review 03 criada via `/code-review` — resolve a pendência `[P-10.1]` (🔴), primeira revisão formal desde a reformulação `CR-NET-LOCK-01` e desde a rodada 02 (aplicada fora deste fluxo em 2026-09-14). |
| 2026-09-20 | Aplicação de 2 achados via `/apply-code-review` — IDs: CR-03-01, CR-03-02. Rebuild confirmado. |
