
# 002 — Corrigir cap de medicamento por instância · Spec Técnica

**Mod:** Skills-Extended
**Spec funcional:** [002-corrigir-cap-medicamento-instancia-01-spec.md](002-corrigir-cap-medicamento-instancia-01-spec.md)
**Criado:** 2026-09-07

> Fonte primária de verdade: [references/eft-decompiled/Assembly-CSharp/EFT.HealthSystem/ActiveHealthController.cs](../../../../references/eft-decompiled/Assembly-CSharp/EFT.HealthSystem/ActiveHealthController.cs). Todas as linhas citadas foram lidas diretamente nesta sessão.

> ⚠️ Esta spec tem **mais incerteza técnica** que a do item 001 — o próprio relatório de auditoria já sinalizava isso como o achado arquiteturalmente mais trabalhoso do grupo. Os pontos marcados `TODO confirmar` precisam ser resolvidos durante o `/code-mod`, idealmente com o projeto aberto num IDE com acesso ao dump completo (nesta sessão só tive acesso via leitura de arquivo, sem navegação de symbol/typeinfo interativa).

## 1. Estratégia

O patch atual (`StimulatorApplyBuffPatch`, alvo `ActiveHealthController.Stimulator.smethod_0`, um método **estático**) não tem acesso a nenhuma informação de qual entidade está usando o estimulante — por isso hoje cai em `GameUtils.GetSkillManager()`, que sempre resolve o `MainPlayer`. Achei o ponto exato de onde vem o contexto correto: `Stimulator.method_6` ([ActiveHealthController.cs:2666-2731](../../../../references/eft-decompiled/Assembly-CSharp/EFT.HealthSystem/ActiveHealthController.cs#L2666-L2731)) é o método de **instância** que chama `smethod_0` para cada tipo de buff, e no caso `EStimulatorBuffType.SkillRate` ([ActiveHealthController.cs:2712-2719](../../../../references/eft-decompiled/Assembly-CSharp/EFT.HealthSystem/ActiveHealthController.cs#L2712-L2719) — que é literalmente o cap de Field Medicine que este item corrige) usa `base.HealthController.SkillManager_0` — o `SkillManager` do **dono real do efeito**, não do `MainPlayer`.

`base.HealthController` é a propriedade `HealthController` (tipo `ActiveHealthController`), definida na classe base comum de todos os efeitos (atribuída em `Init`/`SetEffectInfo`, [ActiveHealthController.cs:457,473](../../../../references/eft-decompiled/Assembly-CSharp/EFT.HealthSystem/ActiveHealthController.cs#L457)) — `Stimulator` ([ActiveHealthController.cs:2279](../../../../references/eft-decompiled/Assembly-CSharp/EFT.HealthSystem/ActiveHealthController.cs#L2279): `protected class Stimulator : Effect<GStruct394>`) herda essa propriedade através de `Effect<TStore>` ([ActiveHealthController.cs:14](../../../../references/eft-decompiled/Assembly-CSharp/EFT.HealthSystem/ActiveHealthController.cs#L14): `public abstract class Effect<TStore> : GClass3008`).

Como `smethod_0` continua sendo o método patcheado hoje (mudar o alvo do patch pra `method_6` exigiria reescrever a lógica de cálculo inteira, non-trivial), a estratégia é o mesmo padrão de "patch cooperando com patch" desenhado no item 001 (Grupo A.1, porta): um **novo patch em `method_6`** captura `base.HealthController` do efeito em execução antes de `smethod_0` ser chamado; o `StimulatorApplyBuffPatch` existente passa a consultar esse valor capturado em vez de `GameUtils.GetSkillManager()`.

## 2. Pontos de patch

| Alvo (Assembly) | Tipo | Motivo |
|---|---|---|
| [`ActiveHealthController.Stimulator.method_6`](../../../../references/eft-decompiled/Assembly-CSharp/EFT.HealthSystem/ActiveHealthController.cs#L2666) | Prefix + Postfix (**novo**) | Capturar `base.HealthController` do efeito antes de `smethod_0` rodar, liberar depois |
| [`ActiveHealthController.Stimulator.smethod_0`](../../../../references/eft-decompiled/Assembly-CSharp/EFT.HealthSystem/ActiveHealthController.cs#L3064) | Prefix (existente, **modificar**) | Resolver `SkillManager` a partir do `HealthController` capturado acima, com fallback seguro |

## 3. Novas propriedades F12 (BepInEx)

N/A — nenhuma `ConfigEntry` nova.

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `modded/Plugin/Skills/FieldMedicine/Patches/StimulatorHealthControllerTrackerPatch.cs` | CRIAR | Novo patch em `Stimulator.method_6` que captura o `HealthController` (dono real) do efeito em execução |
| `modded/Plugin/Skills/FieldMedicine/Patches/StimulatorApplyBuffPatch.cs` | MODIFICAR | Resolver `SkillManager` via o tracker acima em vez de `GameUtils.GetSkillManager()` |

## 5. Stubs de código

```csharp
// modded/Plugin/Skills/FieldMedicine/Patches/StimulatorHealthControllerTrackerPatch.cs (novo)
using System.Reflection;
using EFT.HealthSystem;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace SkillsExtended.Skills.FieldMedicine.Patches;

// smethod_0 (patcheado por StimulatorApplyBuffPatch) é estático e não recebe nenhum dado de qual
// entidade está usando o estimulante. method_6 (instância, dentro de Stimulator) TEM esse contexto
// via base.HealthController — este patch captura esse valor antes de smethod_0 rodar.
internal class StimulatorHealthControllerTrackerPatch : ModulePatch
{
    // Resolvido na review 01 (PA-01-01): "HealthController" é property pública, declarada em
    // ActiveHealthController.GClass3008 (ActiveHealthController.cs:361-369). Type.GetProperty
    // inclui membros públicos herdados por padrão, então buscar a partir de "Stimulator" já basta.
    private static readonly PropertyInfo HealthControllerProperty =
        AccessTools.Property(
            typeof(ActiveHealthController).GetNestedType("Stimulator", BindingFlags.NonPublic),
            "HealthController");

    // Válido só durante a execução síncrona de method_6 (Unity/Harmony rodam no mesmo thread aqui,
    // então não há concorrência real entre um Prefix e o Postfix da mesma chamada).
    internal static ActiveHealthController CurrentEffectOwner;

    protected override MethodBase GetTargetMethod()
    {
        // ref: Assembly-CSharp/EFT.HealthSystem/ActiveHealthController.cs:2666
        // Resolvido por assinatura (não por nome literal "method_6", conforme PA-01-03) — nomes
        // gerados (method_N) podem mudar entre gerações do decompile mesmo pra mesma versão do jogo.
        var stimulatorType = typeof(ActiveHealthController).GetNestedType("Stimulator", BindingFlags.NonPublic);
        return AccessTools.GetDeclaredMethods(stimulatorType)
            .First(m => m.Name.StartsWith("method_")
                        && m.GetParameters().Length == 2
                        && m.GetParameters()[0].ParameterType.Name == "Class2222"
                        && m.GetParameters()[1].ParameterType == typeof(bool));
    }

    [PatchPrefix]
    private static void Prefix(object __instance)
    {
        CurrentEffectOwner = HealthControllerProperty.GetValue(__instance) as ActiveHealthController;
    }

    [PatchPostfix]
    private static void Postfix()
    {
        CurrentEffectOwner = null;
    }
}
```

```csharp
// modded/Plugin/Skills/FieldMedicine/Patches/StimulatorApplyBuffPatch.cs (diff)
[PatchPrefix]
public static bool Prefix(InjectorBuff buffSettings, float refValue, Vector2? limits, ref float __result)
{
    if (!SkillsExtendedPlugin.SkillData.FieldMedicine.Enabled || limits is null)
    {
        return true;
    }

    var value = buffSettings.AbsoluteValue ? buffSettings.Value : (buffSettings.Value + 1f) * refValue;

    // Resolve o SkillManager do DONO REAL do efeito (capturado por
    // StimulatorHealthControllerTrackerPatch a partir de method_6), em vez de sempre o MainPlayer.
    var ownerHealthController = StimulatorHealthControllerTrackerPatch.CurrentEffectOwner;
    var skillManager = ownerHealthController != null && SkillManagerAccessor != null
        ? SkillManagerAccessor.GetValue(ownerHealthController) as SkillManager
        : GameUtils.GetSkillManager(); // fallback — não deveria ser atingido no fluxo normal (ver §7)

    var newSkillCap = 60 * (1 + skillManager?.SkillManagerExtended.FieldMedicineSkillCap);

    __result = Mathf.CeilToInt(Mathf.Clamp(value, limits.Value.x, newSkillCap));

    return false;
}

// Campo adicionado no topo da classe. Resolvido na review 01 (PA-01-02): a declaração de
// "SkillManager_0" não foi encontrada diretamente em ActiveHealthController.cs — provavelmente
// herdada da classe base GClass3009<T> (outro arquivo). Em vez de arriscar um GetField que só
// busca na classe exata (retornaria null silenciosamente se herdado), sobe a cadeia de BaseType
// até achar — se não achar em lugar nenhum, cai no fallback GameUtils.GetSkillManager() já
// existente no Prefix acima, degradando em vez de quebrar.
private static readonly FieldInfo SkillManagerAccessor = ResolveSkillManagerField();

private static FieldInfo ResolveSkillManagerField()
{
    for (var t = typeof(ActiveHealthController); t != null; t = t.BaseType)
    {
        var field = t.GetField("SkillManager_0", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null) return field;
    }
    return null;
}
```

## 6. Fluxo de dados

```
[A] Jogador/bot/peer usa um estimulante que afeta a skill Field Medicine (SkillRate)
  → [B] EFT chama ActiveHealthController.Stimulator.method_6(buff, activate: true)
        (ActiveHealthController.cs:2666, case EStimulatorBuffType.SkillRate na linha 2712)
  → [C] StimulatorHealthControllerTrackerPatch.Prefix captura base.HealthController (dono real do efeito)
  → [D] method_6 chama smethod_0(buff.Settings, skillClass.Level, limits) (linha 2719)
  → [E] StimulatorApplyBuffPatch.Prefix intercepta smethod_0, lê o HealthController capturado em [C],
        resolve o SkillManager do DONO REAL (não mais sempre o MainPlayer), calcula o cap
  → [F] StimulatorHealthControllerTrackerPatch.Postfix libera CurrentEffectOwner
```

## 7. Riscos e dependências

- **Incerteza técnica original, resolvida/mitigada na review 01:** (1) `HealthController` confirmado — property pública em `ActiveHealthController.GClass3008:361` (`PA-01-01`, ✅ Resolvido); (2) `SkillManager_0` não localizado diretamente em `ActiveHealthController.cs` (provavelmente herdado de `GClass3009<T>`) — mitigado com reflection que sobe `BaseType` até achar, com fallback seguro se não achar (`PA-01-02`, ✅ Resolvido); (3) estabilidade do nome `method_6` entre gerações do dump — **não verificável sem symbol navigation**, aceito como risco monitorado com a mitigação já proposta (resolver por assinatura durante o `/code-mod` em vez de nome literal) (`PA-01-03`, ✅ Resolvido — aceito com mitigação, não eliminado).
- **Correção pós-compilação (esta suposição da spec original estava errada):** eu tinha suposto que `var newSkillCap = 60 * (1 + skillManager?.SkillManagerExtended.FieldMedicineSkillCap);` produzia `float?` por causa do `?.`, e o stub original em §5 adicionava um `?? limits.Value.y` pra "proteger" isso. **O compilador provou que isso é desnecessário**: `FieldMedicineSkillCap` é do tipo `SkillManager.SkillBuffClass` (uma classe wrapper, não um `float` puro — confirmado em `Plugin/Skills/Core/SkillManagerExt.cs:29`), com conversão implícita pra `float`. Como o membro final da cadeia (`FieldMedicineSkillCap`) é um **tipo referência** (classe), `?.` não produz `Nullable<T>` (isso só acontece quando o membro final é um tipo valor) — o resultado da expressão é `float` normal, exatamente como o código original já esperava. O `?? limits.Value.y` foi removido do código final (não compilava: `CS0019`). Mantido aqui como nota de correção, não como achado real.
- **Fallback `GameUtils.GetSkillManager()`** no stub (quando `CurrentEffectOwner` é nulo) preserva o comportamento ATUAL (buggy, mas não regressivo) como rede de segurança — se por algum motivo `method_6` não rodar antes de `smethod_0` num caminho não mapeado, o cálculo continua funcionando (com o mesmo viés de MainPlayer que já existe hoje) em vez de quebrar.
- **Sem dependência de outros patches do mod** — `StimulatorApplyBuffPatch`/`StimulatorHealthControllerTrackerPatch` são isolados dentro do subsistema Field Medicine.

## 8. Checklist de implementação

- [x] `GetTargetMethod()` de `StimulatorHealthControllerTrackerPatch` resolvido por assinatura (`Class2222` + `bool`), não por nome literal `"method_6"`, conforme mitigação aceita em `PA-01-03`.
- [x] Criar `StimulatorHealthControllerTrackerPatch.cs`.
- [x] Modificar `StimulatorApplyBuffPatch.cs` conforme §5.
- [x] Registro automático via `PatchManager` (auto-discovery, mesmo mecanismo do item 001) — sem necessidade de registro manual.
- [x] Compilar e confirmar 0 erros/warnings novos — compilou limpo após 2 ajustes: `using EFT;` faltante pro tipo `SkillManager`, e remoção do `?? limits.Value.y` que não compilava (ver §7 — `newSkillCap` é `float` não-nulo, suposição original da spec estava errada).
- [ ] Validar em raid — pendente de teste manual do usuário (usar estimulante com efeito `SkillRate` no jogador local, e se possível em bot/outro jogador).

## 9. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid: start/stop hooks — AP-01 | N/A | `CurrentEffectOwner` é limpo pelo próprio Postfix da mesma chamada síncrona (não é estado raid-scoped que precise de teardown de raid). |
| 2 | Filtro MainPlayer/Fika em todo patch que reage a ação de player — AP-02 | ✅ | É o objetivo central deste item — resolver o `SkillManager` do dono real em vez do `MainPlayer` (§1, §5). |
| 3 | Alvos ofuscados/virtuais resolvidos por assinatura; overrides auditados — AP-03 | ✅ (mitigado) | `method_6` não é `virtual` (chamado diretamente dentro da própria classe `Stimulator`, sem overrides a auditar), mas o nome é gerado — mitigação aceita em `PA-01-03` (review 01): resolver por assinatura (`Class2222` + `bool`) no `/code-mod`, item adicionado ao checklist §8. |
| 4 | Mudança de estado via API canônica; side-effects mapeados — AP-04 | ✅ | Nenhuma escrita de campo interno novo — só leitura de `HealthController`/`SkillManager_0` já existentes, através de reflection em vez de acesso direto (porque os tipos são `protected`/aninhados). `HealthController` confirmado público (`PA-01-01`); `SkillManager_0` acessado via reflection que sobe a hierarquia (`PA-01-02`), com fallback seguro. |
| 5 | Estado entre raids | N/A | Efeito de estimulante é escopo de uma única raid; `CurrentEffectOwner` não persiste entre chamadas, muito menos entre raids. |
| 6 | ConfigEntry sem ambiguidade — AP-05 | N/A | Nenhuma `ConfigEntry` nova. |
| 7 | Reentry-guard — AP-07 | N/A | Nenhum patch invoca o próprio método patcheado. |
| 8 | Flags/caches validados contra o contexto atual — AP-08 | ✅ | `CurrentEffectOwner` é setado e limpo a cada chamada de `method_6` — não é um cache de longa duração, não pode ficar stale entre trocas de contexto. |
| 9 | Patch-point reconfirmado no `.cs` do dump — AP-09 | ✅ (com ressalva) | `smethod_0`, linha 2712-2719 (contexto de uso) e `HealthController` (`ActiveHealthController.GClass3008:361`) foram lidos diretamente e confirmados. `method_6` como nome exato não tem a mesma garantia de estabilidade que `smethod_0` já demonstrou em produção — mitigado (não eliminado) resolvendo por assinatura no `/code-mod`, conforme `PA-01-03`. |
| 10 | Skill EFT usada como lever confirmada não-inerte — AP-10 | N/A | Field Medicine já é funcional hoje (bug é sobre A QUEM o cap é aplicado). |
| 11 | Pacote FIKA próprio — AP-11 | N/A | Nenhum pacote `INetSerializable` envolvido. |

## Histórico

| Data | Evento |
|---|---|
| 2026-09-07 | Spec técnica criada via `/create-technical-spec` |
| 2026-09-07 | Review 01 aplicada — `PA-01-01` (HealthController confirmado), `PA-01-02` (SkillManager_0 via reflection com fallback), `PA-01-03` (method_6 — mitigação aceita) incorporados; checklist §9 atualizado |
| 2026-09-07 | Build concluído via `/code-mod` — compilou limpo após corrigir `using EFT;` faltante e remover o `?? limits.Value.y` (não compilava; suposição de `float?` estava errada, confirmado pelo compilador) |
