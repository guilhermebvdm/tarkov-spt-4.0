# 002 — Corrigir cap de medicamento por instância · Review Técnica 01

**Mod:** Skills-Extended
**Spec técnica revisada:** [002-corrigir-cap-medicamento-instancia-02-spec-tech.md](002-corrigir-cap-medicamento-instancia-02-spec-tech.md)
**Data:** 2026-09-07

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-01-MM`. Resolver até zerar bloqueadores antes de `/code-mod`.

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 3 (2 mitigados, não eliminados — ver notas) · Total: 3

Memória consultada: sem memória prévia · pendências que afetam: nenhuma
Docs técnicos lidos (gatilho disparado): `spt-antipatterns.md` (só o obrigatório)

**Resultado geral:** a spec técnica original marcava os checks 3/9 da §9 como ❌/⚠️ por 3 pontos de incerteza genuína. Consegui fechar completamente 1 deles nesta review (com evidência forte) e proponho uma mitigação concreta pro 2º (que também resolve, na prática, sem precisar identificar a classe exata). O 3º (estabilidade do nome `method_6`) genuinamente não dá pra fechar sem uma ferramenta de symbol navigation que não tenho nesta sessão — mas já tem mitigação adequada na própria spec (resolver por assinatura). Por isso **rebaixei o item de 🔴/❌ para 🟡** — a spec pode avançar pro `/code-mod` com esses 2 pontos 🟡 como riscos monitorados, não bloqueadores.

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | C — Erro de Lógica | ✅ Resolvido | `HealthController` confirmado: property pública em `ActiveHealthController.GClass3008:361` | Resolvido |
| PA-01-02 | A — Gap | ✅ Resolvido (mitigado) | `SkillManager_0` não declarado em `ActiveHealthController.cs` — mitigado com reflection que sobe a hierarquia | Resolvido |
| PA-01-03 | A — Gap | ✅ Resolvido (aceito com mitigação) | Estabilidade de `method_6` não confirmável nesta sessão — mitigação já proposta aceita | Resolvido |

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

**`HealthController` confirmado: property pública, declarada em `ActiveHealthController.GClass3008`**

**Problema (original):** A spec técnica marcava como `TODO confirmar` se `HealthController` era uma property pública e em qual classe da hierarquia (`GClass3008` ou `Effect<TStore>`) ela era declarada.

**Investigação desta review:** Li [ActiveHealthController.cs:361-369](../../../../references/eft-decompiled/Assembly-CSharp/EFT.HealthSystem/ActiveHealthController.cs#L361-L369):
```csharp
public ActiveHealthController HealthController
{
    [CompilerGenerated] get { return ActiveHealthController_0; }
    [CompilerGenerated] set { ... }
}
```
Confirmado: **property pública** (não campo), declarada dentro de `ActiveHealthController.GClass3008` (a classe nested cujo corpo começa em [linha 29](../../../../references/eft-decompiled/Assembly-CSharp/EFT.HealthSystem/ActiveHealthController.cs#L29): `public abstract class GClass3008 : IEffect`) — a classe base comum de `Effect<TStore>` ([linha 14](../../../../references/eft-decompiled/Assembly-CSharp/EFT.HealthSystem/ActiveHealthController.cs#L14)) e, por herança, de `Stimulator` ([linha 2279](../../../../references/eft-decompiled/Assembly-CSharp/EFT.HealthSystem/ActiveHealthController.cs#L2279)).

**Resolução:** o stub de `StimulatorHealthControllerTrackerPatch` (§5 da spec técnica) pode manter `AccessTools.Property(...)` — funciona independente de passar o tipo `Stimulator` ou `Effect<GStruct394>` como base de busca, já que `Type.GetProperty` (usado internamente por `AccessTools.Property`) inclui membros públicos herdados por padrão. Remover o comentário `TODO confirmar` associado a este ponto específico e substituir pela citação acima.

**Aplicação:** §5 e §7 da spec técnica atualizados removendo a incerteza sobre este ponto — ✅ aplicado.

---

### PA-01-02 · A — Gap · ✅ Resolvido em 2026-09-07 (mitigado)

**`SkillManager_0` não tem declaração encontrável em `ActiveHealthController.cs` — provavelmente herdado de `GClass3009<T>`**

**Problema:** Tentei localizar a declaração de `SkillManager_0` (usado em `base.HealthController.SkillManager_0` em dezenas de pontos do arquivo, ex. [linha 2714](../../../../references/eft-decompiled/Assembly-CSharp/EFT.HealthSystem/ActiveHealthController.cs#L2714)) dentro de `ActiveHealthController.cs`. Só encontrei uma **atribuição** (`SkillManager_0 = skills;`, [linha 3440](../../../../references/eft-decompiled/Assembly-CSharp/EFT.HealthSystem/ActiveHealthController.cs#L3440)), nunca a declaração do campo/property em si. Como `ActiveHealthController : GClass3009<ActiveHealthController.GClass3008>` ([linha 12](../../../../references/eft-decompiled/Assembly-CSharp/EFT.HealthSystem/ActiveHealthController.cs#L12)), a hipótese mais provável é que `SkillManager_0` seja **herdado da classe base `GClass3009<T>`**, declarada em outro arquivo que não foi localizado nesta sessão (busca por `class GClass3009` na pasta `EFT.HealthSystem/` não encontrou nada — provavelmente vive em outro namespace do Assembly).

**Por que importa:** O stub atual de `StimulatorApplyBuffPatch` (§5) usa `AccessTools.Field(typeof(ActiveHealthController), "SkillManager_0")` — isso só encontra o campo se ele estiver declarado **diretamente** em `ActiveHealthController` (comportamento padrão de `Type.GetField` sem `BindingFlags.FlattenHierarchy` não inclui campos `private`/`protected` de classes base, só os `public`/`protected` — e mesmo esses só com as flags certas). Se `SkillManager_0` for `private` na classe base `GClass3009<T>`, a chamada atual pode retornar `null` silenciosamente, e o cast `as SkillManager` do resultado nulo não quebra, mas o `skillManager` final ficaria sempre nulo — caindo sempre no fallback `GameUtils.GetSkillManager()`, ou seja, **a correção não teria efeito nenhum** e ninguém perceberia sem testar em coop de verdade.

**Sugestão:** trocar a reflection por uma versão que sobe a hierarquia de tipos manualmente até achar o membro, independente de saber o nome exato da classe que o declara:

```csharp
// modded/Plugin/Skills/FieldMedicine/Patches/StimulatorApplyBuffPatch.cs
private static readonly FieldInfo SkillManagerAccessor = ResolveSkillManagerField();

private static FieldInfo ResolveSkillManagerField()
{
    for (var t = typeof(ActiveHealthController); t != null; t = t.BaseType)
    {
        var field = t.GetField("SkillManager_0", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null) return field;
    }
    return null; // se null, o Prefix cai no fallback GameUtils.GetSkillManager() já existente — degrada, não quebra
}
```
Isso resolve o problema **sem precisar saber em qual classe da hierarquia o campo realmente vive** — sobe `BaseType` até achar, e se não achar em lugar nenhum (churn de decompile, campo renomeado), cai no fallback já existente em vez de quebrar silenciosamente com `null` de um `GetField` que buscou só na classe errada.

**Resolução:** stub de `StimulatorApplyBuffPatch` atualizado com `ResolveSkillManagerField()`; guard de null adicionado no uso de `SkillManagerAccessor`.

**Decisão:**
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

---

### PA-01-03 · A — Gap · ✅ Resolvido em 2026-09-07 (aceito com mitigação)

**Estabilidade de `method_6` como nome de alvo — não verificável nesta sessão, aceitar mitigação já proposta**

**Problema:** A própria spec técnica (§7, §9 check 3/9) já identificou que `method_6` é um nome gerado pelo decompilador (diferente de `smethod_0`, que o mod já usa em produção há tempo e se provou estável) e propôs resolver por assinatura em vez de nome literal como mitigação. Tentei validar isso de forma independente nesta review, mas **não tenho acesso a uma ferramenta de symbol navigation** (dnSpy/ILSpy interativo) nesta sessão — só leitura de arquivo — então não consigo confirmar nem refutar a estabilidade do nome além do que a própria spec já documentou.

**Por que importa:** Se `method_6` mudar de nome numa geração futura do dump (ex.: após atualização do jogo), `GetTargetMethod()` retornaria `null` e o Harmony patch falharia ao registrar — o `ModulePatch.Enable()` da base `SPT.Reflection.Patching` normalmente loga um erro nesse caso em vez de travar o plugin inteiro (comportamento padrão do framework, não verificado especificamente aqui), mas o efeito prático seria "a correção deste item para de funcionar silenciosamente" até alguém notar.

**Sugestão:** não há nada de novo a resolver aqui além do que a spec já propõe — **aceitar a mitigação já documentada** (resolver por assinatura, ex. `AccessTools.GetDeclaredMethods(stimulatorType).First(m => m.Name.StartsWith("method_") && m.GetParameters().Length == 2 && m.GetParameters()[0].ParameterType == typeof(Class2222) && m.GetParameters()[1].ParameterType == typeof(bool))` — usando o tipo `Class2222` já visível na assinatura em [ActiveHealthController.cs:2666](../../../../references/eft-decompiled/Assembly-CSharp/EFT.HealthSystem/ActiveHealthController.cs#L2666)) durante o `/code-mod`, e documentar no as-build (`-05-asbuild.md`) qual dos dois caminhos (nome literal ou assinatura) foi efetivamente usado.

**Resolução:** mitigação aceita como está; item adicionado ao checklist §8 da spec técnica lembrando de resolver por assinatura no `/code-mod`.

**Decisão:**
- `[x]` Aceitar sugestão (usar resolução por assinatura, não por nome literal)
- `[ ]` Caminho alternativo: _________________

## Histórico

| Data | Evento |
|---|---|
| 2026-09-07 | Review 01 criada via `/review-technical-spec` — 1 ponto resolvido na própria review, item rebaixado de bloqueador pra 2 riscos 🟡 monitorados |
