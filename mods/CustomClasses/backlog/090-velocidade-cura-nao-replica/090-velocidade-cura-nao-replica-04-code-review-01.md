# 090 — Velocidade da animação de cura não replica pros outros jogadores (Fika) · Code Review 01

**Mod:** CustomClasses
**Spec funcional:** [090-velocidade-cura-nao-replica-01-spec.md](090-velocidade-cura-nao-replica-01-spec.md)
**Spec técnica:** [090-velocidade-cura-nao-replica-02-spec-tech.md](090-velocidade-cura-nao-replica-02-spec-tech.md)
**Asbuild:** [090-velocidade-cura-nao-replica-05-asbuild.md](090-velocidade-cura-nao-replica-05-asbuild.md)
**Data:** 2026-09-09

> Análise crítica do código implementado por `/code-mod`. Cada achado recebe um ID `CR-01-MM` permanente. Resolver bloqueadores 🔴 via `/apply-code-review` antes de fechar o item.

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 1 · Total: 1

Memória consultada: snapshot 2026-07-15 (Sessão 16, `mods/CustomClasses/memory/sessions.md`) · pendências que afetam este item: nenhuma. Doc técnico lido (gatilho sempre): `spt-antipatterns.md` — nenhuma violação de AP-NN encontrada.

Todos os 3 pontos da [spec-tech-review-01](090-velocidade-cura-nao-replica-03-spec-tech-review-01.md) (PA-01-01 a PA-01-03) foram confirmados implementados 1:1 no código: `[BepInDependency("com.fika.core", SoftDependency)]` presente em `Plugin.cs`, logs de sucesso/aviso em `Register()`, comportamento fail-open coerente com o cenário "hook ausente" — nenhum precisa ser reaberto.

Assembly/mod-evidence reconferida: `ClassIdentities.cs:143-151` (`ClassIdOf`, gate `IsAI` e local-vs-peer) ✅, `ClassMedicPatches.cs:86-91` (`MedicTiming.IsSurgery`) e `:114-131` (`MedicTiming.FactorFor`) ✅, `EClassId.CombatMedic` confirmado em `SkillMultipliers.cs:32` ✅ — todos batem com o citado na spec técnica e são usados corretamente no código novo. `Player.MedsController` (base de `ObservedMedsController` no FIKA) e `FirearmsAnimator`/`AbstractHandsController` não são tocados diretamente por este item (a resolução delega inteiramente ao hook do FIKA/005, já revisado e fechado separadamente) — nada a reconferir aqui além da assinatura do delegate, que bate (`Func<Player, Item, float>`, mesmos tipos `EFT.Player`/`EFT.InventoryLogic.Item` dos dois lados).

Verificação de fluxo (leitura linha a linha, sem bug encontrado):
- `Register()` só é chamado 1× no `Awake()`, dentro de `try/catch`, na ordem certa em relação ao `[BepInDependency]` novo — a resolução `static readonly` de `HookType`/`ExtraSpeedMultiplierField` acontece no primeiro toque à classe, que é exatamente essa chamada.
- `AccessTools.TypeByName`/`AccessTools.Field` (HarmonyLib) retornam `null` em vez de lançar quando o alvo não existe — o fail-open funciona mesmo sem o `try/catch` externo; o `try/catch` em `Plugin.cs` é uma rede de segurança adicional (consistente com o padrão do `ExecutionSpeedCapPatch`), não uma dependência real.
- `ResolveFactor` reusa `MedicTiming._disabled` (via `FactorFor`) implicitamente — se o trio local do 072 (`MedsOperationScopePatch`/`MedUseTimePatch`/`MedAnimSpeedPatch`) falhar ao aplicar NESTE cliente, a réplica de velocidade pra peers TAMBÉM desliga (retorna 1f). Isso é uma consequência correta e intencional do reuso: o kill-switch do 072 (CR-F6, "meio-perk é pior que nenhum") se propaga coerentemente pro 090, sem código extra.
- `SetValue(null, del)` com tipo de delegate incompatível (se um Fika futuro mudar a assinatura do hook) lançaria `ArgumentException` — já capturado pelo `try/catch` do `Plugin.cs` (linha 269-276), sem risco de crash no boot.

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-01-01 | F | 🟢 Menor | `Register()` não avisa se estiver sobrescrevendo um hook já assinado por outro mod | ✅ Aplicado 2026-09-09 |

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

### CR-01-01 · F — Melhoria opcional · 🟢 Menor · ✅ Aplicado em 2026-09-09

**`Register()` não avisa se estiver sobrescrevendo um hook já assinado por outro mod**

**Local:** [`mods/CustomClasses/modded/Client/Patches/ClassMedicReplicationHook.cs:31-41`](../../modded/Client/Patches/ClassMedicReplicationHook.cs#L31-L41)

**Problema:** `ObservedMedsSpeedHook.ExtraSpeedMultiplier` (item FIKA/005) é documentado como `last-write-wins` — se outro mod já tiver assinado o campo antes do `Awake()` do CustomClasses rodar, `Register()` sobrescreve silenciosamente sem log nenhum.

```csharp
Func<Player, Item, float> del = ResolveFactor;
ExtraSpeedMultiplierField.SetValue(null, del);
Plugin.Log?.LogInfo("[CustomClasses] (090) hook de velocidade de cura (Fika) assinado com sucesso.");
```

**Por que importa:** Hoje não há nenhum outro mod conhecido usando esse hook, então isso é puramente preventivo. Mas se um 2º mod aparecer no futuro (o próprio FIKA/005 já previu essa limitação na spec), o sintoma seria "um dos dois perks simplesmente não funciona" sem nenhuma pista no log de qual dos dois "ganhou" — dificultando o diagnóstico.

**Sugestão:** Antes do `SetValue`, ler o valor atual do campo (`ExtraSpeedMultiplierField.GetValue(null)`) e, se já não for `null`, logar um `LogWarning` do tipo `"[CustomClasses] (090) sobrescrevendo um ExtraSpeedMultiplier já assinado por outro mod (last-write-wins) — comportamento pode não ser o esperado."` antes de prosseguir com a atribuição normalmente.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Sugestão aplicada conforme proposto.
**Aplicação:** `ClassMedicReplicationHook.cs` — guard `if (ExtraSpeedMultiplierField.GetValue(null) != null)` com `LogWarning` adicionado antes do `SetValue`, comentado com `ref: CR-01-01`.

---

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-09 | Code review 01 criada via `/code-review` |
| 2026-09-09 | Aplicação automática de 1 achado via `/apply-code-review` — ID aplicado: CR-01-01 |
