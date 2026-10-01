# 002 — Corrigir cap de medicamento por instância · Code Review 01

**Mod:** Skills-Extended
**Spec funcional:** [002-corrigir-cap-medicamento-instancia-01-spec.md](002-corrigir-cap-medicamento-instancia-01-spec.md)
**Spec técnica:** [002-corrigir-cap-medicamento-instancia-02-spec-tech.md](002-corrigir-cap-medicamento-instancia-02-spec-tech.md)
**Asbuild:** [002-corrigir-cap-medicamento-instancia-05-asbuild.md](002-corrigir-cap-medicamento-instancia-05-asbuild.md)
**Data:** 2026-09-07

> Análise crítica do código implementado por `/code-mod`. Memória consultada: sem memória prévia. Docs técnicos lidos: `spt-antipatterns.md` (gatilho obrigatório).

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 1 · Total: 1

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-01-01 | D — Arquitetura | 🟠 Forte | Reflection nova sem guard de null — mesmo padrão que este item existe pra corrigir | ✅ Aplicado |

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

### CR-01-01 · D — Arquitetura · 🟠 Forte · ✅ Aplicado em 2026-09-07

**`StimulatorHealthControllerTrackerPatch` usa reflection sem os mesmos guards de null que o resto do item 001/002 introduziu em outros lugares**

**Local:** [`mods/Skills-Extended/modded/Plugin/Skills/FieldMedicine/Patches/StimulatorHealthControllerTrackerPatch.cs:18-21,33-37,41-44`](../../modded/Plugin/Skills/FieldMedicine/Patches/StimulatorHealthControllerTrackerPatch.cs#L18-L44)

**Problema:** dois pontos de reflection nova neste arquivo não têm proteção contra falha, ao contrário de `StimulatorApplyBuffPatch.SkillManagerAccessor` (mesmo item, mesma sessão), que checa `!= null` antes de usar:

1. `HealthControllerProperty` é resolvida uma vez (`static readonly`) e usada direto no Prefix sem checagem:
```csharp
private static readonly PropertyInfo HealthControllerProperty =
    AccessTools.Property(typeof(ActiveHealthController).GetNestedType("Stimulator", BindingFlags.NonPublic), "HealthController");
...
[PatchPrefix]
private static void Prefix(object __instance)
{
    CurrentEffectOwner = HealthControllerProperty.GetValue(__instance) as ActiveHealthController;
}
```
Se `AccessTools.Property` não achar o membro (ex.: um patch futuro do jogo renomear/mover a property), `HealthControllerProperty` fica `null`, e `HealthControllerProperty.GetValue(...)` lança `NullReferenceException` **dentro do Prefix** — o que a skill `csharp-mod-best-practices` §3 marca explicitamente como proibido sem try/catch ("Never throw out of a Harmony prefix/postfix... Wrap the body in try/catch").

2. `GetTargetMethod()` usa `.First(...)` em vez de `.FirstOrDefault(...)`:
```csharp
return AccessTools.GetDeclaredMethods(stimulatorType)
    .First(m => m.Name.StartsWith("method_") && ... );
```
Se a resolução por assinatura não encontrar nenhum método compatível (o próprio risco que a spec técnica já reconhece em `PA-01-03` — nomes `method_N` podem mudar entre gerações do decompile), isso lança `InvalidOperationException` durante o registro do patch no boot do plugin, em vez de falhar de forma diagnosticável.

**Por que importa:** este item inteiro existe porque padrões de reflection sem checagem de null (`GameUtils.GetSkillManager()!`) causavam `NullReferenceException` em produção (o crash original do `MeleeSpeedPatch`, e os 5+ achados equivalentes da auditoria). Introduzir o mesmo padrão de risco num arquivo novo do mesmo item é inconsistente com o resto do trabalho — se uma atualização futura do jogo mudar a estrutura de `Stimulator`/`HealthController`, o sintoma seria exatamente igual ao bug original que motivou toda essa rodada de correções.

**Sugestão:**
```csharp
private static readonly PropertyInfo HealthControllerProperty = ResolveHealthControllerProperty();

private static PropertyInfo ResolveHealthControllerProperty()
{
    var stimulatorType = typeof(ActiveHealthController).GetNestedType("Stimulator", BindingFlags.NonPublic);
    return AccessTools.Property(stimulatorType, "HealthController");
}

protected override MethodBase GetTargetMethod()
{
    var stimulatorType = typeof(ActiveHealthController).GetNestedType("Stimulator", BindingFlags.NonPublic);
    var method = AccessTools.GetDeclaredMethods(stimulatorType)
        .FirstOrDefault(m => m.Name.StartsWith("method_")
                              && m.GetParameters().Length == 2
                              && m.GetParameters()[0].ParameterType.Name == "Class2222"
                              && m.GetParameters()[1].ParameterType == typeof(bool));
    if (method == null)
    {
        Plugin.Log.LogError("[Skills Extended] StimulatorHealthControllerTrackerPatch: não achou method_6 por assinatura — patch não será aplicado.");
    }
    return method;
}

[PatchPrefix]
private static void Prefix(object __instance)
{
    if (HealthControllerProperty == null) return;
    CurrentEffectOwner = HealthControllerProperty.GetValue(__instance) as ActiveHealthController;
}
```
Se `GetTargetMethod()` retornar `null`, o framework de patch (`SPT.Reflection.Patching`) trata isso como "patch não aplicável" e loga, em vez de lançar — mesmo comportamento defensivo que o resto do mod já assume em outros lugares. Isso também mantém consistência: se este patch não conseguir se registrar, `StimulatorApplyBuffPatch` já tem o fallback pra `GameUtils.GetSkillManager()` (comportamento anterior, não regressivo) — o sistema degrada graciosamente em vez de travar o boot do plugin.

**Decisão:**
- `[x]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

**Resolução:** Sugestão aplicada conforme proposto — `HealthControllerProperty` resolvida via método com nome próprio, checada antes de usar no Prefix; `GetTargetMethod()` trocado de `.First(...)` pra `.FirstOrDefault(...)` com log de erro + `null` se não achar. Compilado com 0 erros.
**Aplicação:** [`mods/Skills-Extended/modded/Plugin/Skills/FieldMedicine/Patches/StimulatorHealthControllerTrackerPatch.cs`](../../modded/Plugin/Skills/FieldMedicine/Patches/StimulatorHealthControllerTrackerPatch.cs)

---

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-07 | Code review 01 criada via `/code-review` |
| 2026-09-07 | Aplicação automática via `/apply-code-review` — aplicado: CR-01-01 |
