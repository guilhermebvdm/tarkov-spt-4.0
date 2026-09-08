# 001 — Corrigir bugs críticos da auditoria 01 · Code Review 01

**Mod:** Skills-Extended
**Spec funcional:** [001-corrigir-bugs-criticos-auditoria-01-01-spec.md](001-corrigir-bugs-criticos-auditoria-01-01-spec.md)
**Spec técnica:** [001-corrigir-bugs-criticos-auditoria-01-02-spec-tech.md](001-corrigir-bugs-criticos-auditoria-01-02-spec-tech.md)
**Asbuild:** [001-corrigir-bugs-criticos-auditoria-01-05-asbuild.md](001-corrigir-bugs-criticos-auditoria-01-05-asbuild.md)
**Data:** 2026-09-07

> Análise crítica do código implementado por `/code-mod`. Memória consultada: sem memória prévia (mod não tem `memory/sessions.md`). Docs técnicos lidos: `spt-antipatterns.md` (gatilho obrigatório).

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 1 · ⏭️ Rejeitados: 1 · Total: 2

**Nota geral:** revi os 6 arquivos linha a linha contra o Assembly/spt-source real (não só contra a spec). Fui atrás especificamente de verificar se a reformulação do `CultistProductionPatch` (a mudança mais arriscada do item, sinalizada como tal na própria spec técnica) realmente preserva o comportamento pretendido — li o corpo completo de `CircleOfCultistService.StartSacrifice`/`RegisterCircleOfCultistProduction` no spt-source pra confirmar. O mecanismo escolhido (mutar `pmcData.Hideout.Production` depois que `StartSacrifice` roda por completo) está **correto** — `pmcData` e a `Production` criada são o mesmo objeto por referência em toda a cadeia, e a mutação acontece antes de qualquer serialização de resposta. Não é um bug; documentando aqui porque foi a parte que mais valia conferir.

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-01-01 | B — Bug latente | 🟡 Médio | `CultistProductionPatch` pode reaplicar o desconto numa produção antiga se `StartSacrifice` sair cedo | ✅ Aplicado |
| CR-01-02 | D — Arquitetura | 🟢 Menor | 4 dos 6 overrides de `SmoothDoorOpenCoroutine` continuam não verificados | ⏭️ Rejeitado (dívida técnica) |

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

### CR-01-01 · B — Bug latente · 🟡 Médio · ✅ Aplicado em 2026-09-07

**`StartSacrificePatch.Postfix` pode reaplicar o desconto de tempo numa produção que já existia antes desta chamada, se `StartSacrifice` sair por um caminho de erro**

**Local:** [`mods/Skills-Extended/modded/Server/Patches/CultistProductionPatch.cs:32-58`](../../modded/Server/Patches/CultistProductionPatch.cs#L32-L58)

**Problema:** o Postfix busca a produção do círculo cultista assim:
```csharp
var cultistProduction = pmcData.Hideout.Production.Values
    .FirstOrDefault(p => p?.SptIsCultistCircle == true);
if (cultistProduction?.ProductionTime is null) return;
...
cultistProduction.ProductionTime *= buff;
```
Ele roda **sempre** que `StartSacrifice` retorna — inclusive nos caminhos de saída antecipada do método original, como [`CircleOfCultistService.cs:69-74`](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Services/CircleOfCultistService.cs#L69-L74) (`cultistCircleStashId is null` → `return output;` **sem** chamar `RegisterCircleOfCultistProduction`). Se o jogador já tiver uma produção do círculo cultista em andamento de uma sacrifício anterior (`SptIsCultistCircle == true` já setado) e uma nova chamada a `StartSacrifice` cair nesse caminho de saída antecipada, o Postfix ainda encontra a produção ANTIGA e multiplica `ProductionTime` por `buff` de novo — aplicando o desconto uma segunda vez sobre um valor que já tinha sido descontado.

**Por que importa:** o comportamento correto é aplicar o desconto **uma vez**, só quando uma produção **nova** é de fato criada por aquela chamada. O código atual não distingue "acabei de criar uma produção" de "encontrei uma produção pré-existente qualquer no profile". Na prática exige um cenário de borda (produção já ativa + nova chamada que erra no caminho inicial) — pouco provável via UI normal do cliente (que deveria impedir isso), mas alcançável por retry de rede ou requisição fora do fluxo normal, já que o servidor não deveria confiar no cliente pra isso.

**Sugestão:** patchear `RegisterCircleOfCultistProduction` (método `protected`, acessível via `AccessTools.Method`) em vez de `StartSacrifice` — esse método só é chamado exatamente quando uma produção nova é de fato registrada, eliminando o problema pela raiz em vez de filtrar depois:
```csharp
protected override MethodBase? GetTargetMethod()
{
    return AccessTools.Method(typeof(CircleOfCultistService), "RegisterCircleOfCultistProduction");
}

[PatchPrefix]
public static void Prefix(MongoId sessionId, ref double craftingTime)
{
    if (!SkillUtil.TryGetSkillLevel(sessionId, SkillTypes.Shadowconnections, out var skillLevel))
    {
        return;
    }

    var timeBonusPerLevel = ConfigController.SkillsConfig.ShadowConnections.CultistCircleReturnTimeReduction;
    var buff = Math.Clamp(1f - timeBonusPerLevel * skillLevel, 0f, 1f);
    craftingTime *= buff;
}
```
Isso ajusta o tempo **antes** de `InitProduction` criar o objeto ([`CircleOfCultistService.cs:166`](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Services/CircleOfCultistService.cs#L166)), sem precisar buscar por `SptIsCultistCircle` depois — nunca toca numa produção que não acabou de ser criada por essa chamada específica. Continua sem estado `static` compartilhado (o `ref` é local à invocação, mesma garantia do design atual contra `AUD-01-05`).

**Decisão:**
- `[x]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

**Resolução:** Sugestão aplicada conforme proposto — classe renomeada de `StartSacrificePatch`/`CultistProductionPatch` pra `RegisterCircleOfCultistProductionPatch`, patcheando `RegisterCircleOfCultistProduction` com `Prefix(MongoId sessionId, ref double craftingTime)`. Compilado com 0 erros.
**Aplicação:** [`mods/Skills-Extended/modded/Server/Patches/CultistProductionPatch.cs`](../../modded/Server/Patches/CultistProductionPatch.cs) — reescrito por completo.

---

### CR-01-02 · D — Arquitetura · 🟢 Menor · ⏭️ Rejeitado em 2026-09-07

**4 dos 6 overrides de `SmoothDoorOpenCoroutine` continuam sem verificação de `base.*`**

**Local:** [`mods/Skills-Extended/modded/Plugin/Skills/SilentOps/Patches/DoorInteractionTrackerPatch.cs:19-25`](../../modded/Plugin/Skills/SilentOps/Patches/DoorInteractionTrackerPatch.cs#L19-L25)

**Problema:** a review técnica (`PA-01-01`) confirmou que `Door`/`KeycardDoor` chamam `base.SmoothDoorOpenCoroutine`, mas `Switch`, `LootableContainer`, `DoorSwitch` e `BufferGateSwitcher` continuam não verificados — item de checklist da spec técnica (§8) marcado como "não feito nesta rodada".

**Por que importa:** se algum desses 4 não chamar `base.*`, o bônus de Silent Ops simplesmente não se aplica a esse tipo específico de objeto (fail-safe de `DoorSoundPatch` já cobre com som nativo, sem quebrar nada) — impacto puramente de "feature incompleta para um tipo raro de objeto", não um bug funcional.

**Sugestão:** conferir os 4 arquivos (`grep -n "override IEnumerator SmoothDoorOpenCoroutine" -A 5` em cada um de `references/eft-decompiled/Assembly-CSharp/EFT.Interactive/{Switch,LootableContainer,DoorSwitch,BufferGateSwitcher}.cs`) antes de considerar a feature completa. Baixa prioridade — pode ficar como dívida técnica.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[x]` Rejeitar (deferir / aceitar como dívida): dívida técnica de baixo risco — fail-safe já cobre; verificar numa rodada futura se algum desses tipos raros de objeto interativo virar relevante.

**Resolução:** Rejeitado — dívida técnica de baixo risco, fail-safe já cobre; verificar numa rodada futura.

---

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-07 | Code review 01 criada via `/code-review` |
| 2026-09-07 | Aplicação automática via `/apply-code-review` — aplicado: CR-01-01; rejeitado: CR-01-02 |
