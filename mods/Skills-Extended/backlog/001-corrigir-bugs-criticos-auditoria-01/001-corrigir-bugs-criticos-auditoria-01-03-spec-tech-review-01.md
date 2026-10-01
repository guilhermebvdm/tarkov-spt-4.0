# 001 — Corrigir bugs críticos da auditoria 01 · Review Técnica 01

**Mod:** Skills-Extended
**Spec técnica revisada:** [001-corrigir-bugs-criticos-auditoria-01-02-spec-tech.md](001-corrigir-bugs-criticos-auditoria-01-02-spec-tech.md)
**Data:** 2026-09-07

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-01-MM`. Resolver até zerar bloqueadores antes de `/code-mod`.

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 3 · Total: 3

Memória consultada: sem memória prévia · pendências que afetam: nenhuma
Docs técnicos lidos (gatilho disparado): `spt-antipatterns.md` (só o obrigatório)

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | C — Erro de Lógica | ✅ Resolvido | `SmoothDoorOpenCoroutine` é `virtual` — checklist item 3 estava incorreto | Resolvido |
| PA-01-02 | A — Gap | ✅ Resolvido | Stub do Grupo C.1 (`CultistProductionPatch`) não resolvia o mecanismo — achei a resposta durante a review | Resolvido |
| PA-01-03 | B — Edge Case | ✅ Resolvido | Descrição do crescimento de `LastInteractionIsLocal` imprecisa (é por-processo, não por-mapa) | Resolvido |

## Categorias

- **A — Gaps de Especificação:** informações ausentes que ambiguam a implementação
- **B — Edge Cases:** cenários válidos não cobertos
- **C — Erros de Lógica:** pressupostos errados, contradições, código incompatível com SPT 4.0+

## Impacto

- 🔴 **Bloqueador** — impede implementar ou causa bug/crash garantido
- 🟡 **Importante** — pode causar comportamento errado em cenário relevante
- 🟢 **Menor** — qualidade/clareza, não bloqueia

---

## Pontos

### PA-01-01 · C — Erro de Lógica · ✅ Resolvido em 2026-09-07

**`SmoothDoorOpenCoroutine` é `virtual` — o check 3 da §9 (AP-03) afirma o contrário**

**Problema:** A spec técnica (§9, check 3) afirma: *"Nenhum dos 6 alvos patcheados ... é `virtual`/`abstract` no Assembly/spt-source consultado"*. Isso é **falso** para um dos alvos: [WorldInteractiveObject.cs:1088](../../../../references/eft-decompiled/Assembly-CSharp/EFT.Interactive/WorldInteractiveObject.cs#L1088) declara `public **virtual** IEnumerator SmoothDoorOpenCoroutine(EDoorState state, bool isLocalInteraction, float speed = 1f)` — confirmei lendo a linha diretamente nesta review. Isso é exatamente o cenário que a AP-03 (`docs/technical/spt-antipatterns.md`) exige auditar: métodos `virtual` podem ter overrides que não chamam `base.X()`, fazendo o Harmony patch (que intercepta a implementação da CLASSE BASE) nunca disparar pra esses overrides.

Investiguei os overrides durante esta review (a spec técnica não fez essa auditoria, daí o erro no checklist): existem **6 subclasses** de `WorldInteractiveObject` que sobrescrevem `SmoothDoorOpenCoroutine` — `Door.cs`, `KeycardDoor.cs`, `Switch.cs`, `LootableContainer.cs`, `DoorSwitch.cs`, `BufferGateSwitcher.cs`. Confirmei que as duas mais relevantes/comuns já chamam `base.SmoothDoorOpenCoroutine(...)`:
- [Door.cs:388-395](../../../../references/eft-decompiled/Assembly-CSharp/EFT.Interactive/Door.cs#L388-L395) — `yield return base.SmoothDoorOpenCoroutine(state, isLocalInteraction);` na linha 394. ✅ patch dispara normalmente.
- [KeycardDoor.cs:72-79](../../../../references/eft-decompiled/Assembly-CSharp/EFT.Interactive/KeycardDoor.cs#L72-L79) — `yield return base.SmoothDoorOpenCoroutine(state, isLocalInteraction, speed);` na linha 74. ✅ patch dispara normalmente.

**Não verifiquei** `Switch.cs`, `LootableContainer.cs`, `DoorSwitch.cs`, `BufferGateSwitcher.cs` por orçamento desta review.

**Por que importa:** Se algum dos 4 overrides não verificados NÃO chamar `base.SmoothDoorOpenCoroutine`, `DoorInteractionTrackerPatch` nunca vai gravar `isLocalInteraction` para interações com aquele tipo de objeto — mas isso **não quebra nada**, porque `DoorSoundPatch.Prefix` já tem o fail-safe correto (`return true` quando não há entrada no dicionário, §5 da spec técnica) — o resultado seria só "som nativo sem bônus de Silent Ops" pra esses 4 tipos específicos de objeto interativo, não um crash. É por isso que classifiquei como 🟡 Importante (comportamento degradado num cenário possivelmente raro), não 🔴 Bloqueador — mas o checklist da §9 precisa ser corrigido pra refletir a realidade, e o item 8 (checklist de implementação) deveria incluir essa verificação.

**Sugestão:**
1. Corrigir §9 check 3 de N/A para ✅ com a evidência acima (Door/KeycardDoor confirmados; os outros 4 marcados como não-verificados mas de baixo risco dado o fail-safe existente).
2. Adicionar ao §8 (Checklist de implementação) um item: "Confirmar se `Switch`/`LootableContainer`/`DoorSwitch`/`BufferGateSwitcher` chamam `base.SmoothDoorOpenCoroutine` — se algum não chamar, decidir se vale a pena patchear esse tipo especificamente ou aceitar o fail-safe (sem bônus) como comportamento esperado para esses casos raros."

**Resolução:** §9 check 3 corrigido de N/A para ✅ com a evidência de `Door`/`KeycardDoor`; item de checklist adicionado em §8 da spec técnica pros 4 tipos não verificados.

**Decisão:**
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

---

### PA-01-02 · A — Gap · ✅ Resolvido em 2026-09-07

**Stub do Grupo C.1 não resolve o mecanismo do desconto do altar cultista — resposta encontrada nesta review**

**Problema:** O stub de `CultistProductionPatch` (§5, Grupo C.1) da spec técnica termina com um `TODO confirmar` explícito, sem código funcional: *"localizar em `pmcData.Hideout.Production` a produção recém-criada pelo sacrifício (provável flag/campo `SptIsCultistCircle` — verificar o tipo Production/HideoutProduction em spt-source antes de escrever o Postfix definitivo)"*. Isso significa que, se alguém tentasse seguir a spec técnica literalmente hoje, não conseguiria implementar a correção mais crítica do grupo servidor (`AUD-01-05`) sem fazer investigação adicional no meio do `/code-mod` — o que é exatamente o que a spec técnica deveria evitar.

Fiz essa investigação durante a review e encontrei a resposta completa:
- `Hideout.Production` é `Dictionary<MongoId, Production?>?` — [BotBase.cs:708](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Models/Eft/Common/Tables/BotBase.cs#L708).
- O record `Production` ([BotBase.cs:754](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Models/Eft/Common/Tables/BotBase.cs#L754)) tem `SptIsCultistCircle` (`bool?`) na [linha 822-823](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Models/Eft/Common/Tables/BotBase.cs#L822-L823) — exatamente a flag hipotetizada — e `ProductionTime` (`double?`, segundos) na [linha 776](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Models/Eft/Common/Tables/BotBase.cs#L776), que é o campo de tempo a ajustar (equivalente ao `.Time` do `CircleCraftDetails` que o Postfix atual lê hoje).

**Por que importa:** Sem essa informação, a spec técnica não está pronta pra `/code-mod` na parte servidor — é a definição do próprio comando (`Toda referência ao EFT precisa vir do Assembly local com linha. Sem isso, a spec não é técnica — é palpite`), e aqui havia um palpite não resolvido.

**Sugestão:** Substituir o stub do Grupo C.1 na spec técnica pelo seguinte, eliminando a classe `CultistProductionPatch` (o patch em `GetCircleCraftingInfo` deixa de ser necessário):

```csharp
// modded/Server/Patches/CultistProductionPatch.cs (versão final — substitui a classe inteira)
public class StartSacrificePatch : AbstractPatch
{
    private static readonly ConfigController ConfigController = ServiceLocator.ServiceProvider.GetRequiredService<ConfigController>();
    private static readonly SkillUtil SkillUtil = ServiceLocator.ServiceProvider.GetRequiredService<SkillUtil>();

    protected override MethodBase? GetTargetMethod()
    {
        // ref: spt-source/Libraries/SPTarkov.Server.Core/Services/CircleOfCultistService.cs:58
        return AccessTools.Method(typeof(CircleOfCultistService), nameof(CircleOfCultistService.StartSacrifice));
    }

    [PatchPostfix]
    public static void Postfix(MongoId sessionId, PmcData pmcData, ItemEventRouterResponse __result)
    {
        if (pmcData.Hideout?.Production == null) return;

        // ref: spt-source/.../BotBase.cs:708 (Hideout.Production: Dictionary<MongoId, Production?>)
        // ref: spt-source/.../BotBase.cs:822-823 (Production.SptIsCultistCircle: bool?)
        var cultistProduction = pmcData.Hideout.Production.Values
            .FirstOrDefault(p => p?.SptIsCultistCircle == true);
        if (cultistProduction?.ProductionTime is null) return;

        if (!SkillUtil.TryGetSkillLevel(sessionId, SkillTypes.Shadowconnections, out var skillLevel))
        {
            return;
        }

        var timeBonusPerLevel = ConfigController.SkillsConfig.ShadowConnections.CultistCircleReturnTimeReduction;
        var buff = Math.Clamp(1f - timeBonusPerLevel * skillLevel, 0f, 1f);

        // ref: spt-source/.../BotBase.cs:776 (Production.ProductionTime: double?, segundos)
        cultistProduction.ProductionTime *= buff;
    }
}
// Classe CultistProductionPatch (GetCircleCraftingInfo) removida — não é mais necessária.
```

**Isso também simplifica o item de checklist já existente na spec** ("Resolver o TODO confirmar do Grupo C.1...") — pode ser marcado como resolvido assim que este código for adotado.

**Resolução:** stub do Grupo C.1 substituído na spec técnica pela versão final acima; item de checklist marcado como resolvido.

**Decisão:**
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

---

### PA-01-03 · B — Edge Case · ✅ Resolvido em 2026-09-07

**Descrição do crescimento de `LastInteractionIsLocal` é imprecisa**

**Problema:** §9 check 1 (AP-01, N/A) descreve `DoorInteractionTrackerPatch.LastInteractionIsLocal` como um dicionário que "só cresce até o número de portas **do mapa**" — mas como esse dicionário nunca é limpo (nem por raid, nem nunca), ele na verdade acumula uma entrada por porta **de todos os mapas visitados ao longo da vida do processo do jogo**, não só do mapa atual. O raciocínio de fundo (bounded, baixo risco, mesmo padrão já aceito pro `LockPickingHelpers.DoorAttempts` no relatório de auditoria) continua válido — só a frase "do mapa" está tecnicamten errada.

**Por que importa:** Não muda a conclusão (ainda é N/A, ainda é baixo risco — total de portas em TODOS os mapas do jogo é um número finito e pequeno, não um leak de verdade), mas um futuro leitor da spec pode interpretar errado o alcance do dicionário.

**Sugestão:** Trocar a frase por: *"só cresce até o número total de portas do jogo (todos os mapas, ao longo da vida do processo — não é limpo por raid, mas o total é finito e pequeno, mesmo padrão de risco já aceito para `LockPickingHelpers.DoorAttempts` no relatório de auditoria 01)"*.

**Resolução:** texto do check 1 (§9) corrigido na spec técnica.

**Decisão:**
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

## Histórico

| Data | Evento |
|---|---|
| 2026-09-07 | Review 01 criada via `/review-technical-spec` |
