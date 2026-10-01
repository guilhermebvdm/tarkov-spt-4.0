# 004 — Corrigir achados restantes da auditoria 01 · Code Review 01

**Mod:** Skills-Extended
**Spec funcional:** [004-corrigir-achados-restantes-auditoria-01-01-spec.md](004-corrigir-achados-restantes-auditoria-01-01-spec.md)
**Spec técnica:** [004-corrigir-achados-restantes-auditoria-01-02-spec-tech.md](004-corrigir-achados-restantes-auditoria-01-02-spec-tech.md)
**Asbuild:** [004-corrigir-achados-restantes-auditoria-01-05-asbuild.md](004-corrigir-achados-restantes-auditoria-01-05-asbuild.md)
**Data:** 2026-09-07

> Análise crítica do código implementado por `/code-mod`. Memória consultada: snapshot de 2026-09-07 (Sessão 2) · pendências que afetam: nenhuma. Docs técnicos lidos: `spt-antipatterns.md` (gatilho obrigatório).

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 1 · Total: 1

**Nota geral:** revi os 11 arquivos de código linha a linha contra o Assembly e a spec técnica. Confirmei que Plugin, Common e Server compilaram de fato (0 erros de código — ver asbuild). O único achado é uma consequência colateral pequena da própria correção de `AUD-01-19` (a alocação de closure) — não um bug funcional, e a correção é trivial e já reaproveita infraestrutura que o item 003 deixou pronta no mesmo mod.

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-01-01 | D — Arquitetura | 🟡 Médio | Cache estático de `ProneMoveStatePatch` retém o `Player` da raid anterior até a próxima raid sobrescrever | ✅ Aplicado |

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

### CR-01-01 · D — Arquitetura · 🟡 Médio · ✅ Aplicado em 2026-09-07

**Cache estático de `ProneMoveStatePatch` (`AUD-01-19`) retém o `Player` da raid anterior até a próxima raid sobrescrever**

**Local:** [`mods/Skills-Extended/modded/Plugin/Skills/ProneMovement/Patches/ProneMoveStatePatch.cs:18-19,48-52`](../../modded/Plugin/Skills/ProneMovement/Patches/ProneMoveStatePatch.cs#L18-L52)

**Problema:** a correção de `AUD-01-19` (evitar alocar uma closure nova a cada quadro de bruços) introduz dois campos `private static`:

```csharp
private static Player _cachedPlayer;
private static Action _cachedProneAction;
```

Eles só são sobrescritos quando `!ReferenceEquals(player, _cachedPlayer)` (linha 48) — ou seja, na primeira vez que o Prefix roda numa raid nova. Entre o fim de uma raid e o início da próxima, `_cachedPlayer`/`_cachedProneAction` continuam segurando a referência do `Player` da raid **anterior**, sem nenhum ponto de limpeza explícito.

**Por que importa:** é exatamente a mesma classe de achado que motivou `AUD-01-14` no item 003 (leak de referência de `Player` entre raids) — mas numa escala bem menor aqui (um campo `Player` + um `Action`, não uma cadeia de assinaturas de evento). O item 003 já criou `OnGameEndedPatch` (Postfix em `GameWorld.OnDestroy`, [`OnGameStarted.cs:243-263`](../../modded/Plugin/Skills/Shared/Patches/OnGameStarted.cs#L243-L263)) especificamente para centralizar esse tipo de limpeza de fim de raid — deixar este cache novo fora dele é inconsistente com o padrão que o próprio mod acabou de estabelecer duas rodadas atrás.

**Sugestão:** em `OnGameEndedPatch.Postfix` (`mods/Skills-Extended/modded/Plugin/Skills/Shared/Patches/OnGameStarted.cs`), adicionar a limpeza deste cache junto com o resto:

```csharp
[PatchPostfix]
private static void Postfix()
{
    if (OnGameStartedPatch.Player != null)
    {
        OnGameStartedPatch.Player.ActiveHealthController.EffectStartedEvent -= OnGameStartedPatch.ApplyMedicalXp;
        OnGameStartedPatch.Player.Skills.OnMasteringExperienceChanged -= OnGameStartedPatch.ApplyNatoRifleXp;
        OnGameStartedPatch.Player.Skills.OnMasteringExperienceChanged -= OnGameStartedPatch.ApplyEasternRifleXp;
        OnGameStartedPatch.Player = null;
    }

    UpdateWeaponsPatch.ClearRaidState();
    ProneMoveStatePatch.ClearCachedAction(); // novo
}
```
E em `ProneMoveStatePatch.cs`, expor um método `internal static void ClearCachedAction() { _cachedPlayer = null; _cachedProneAction = null; }`. Isso mantém a limpeza de fim de raid centralizada num único lugar, em vez de espalhar responsabilidade de teardown por vários patches sem coordenação.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

**Resolução:** Sugestão aplicada conforme proposto — `ProneMoveStatePatch.ClearCachedAction()` criado e chamado por `OnGameEndedPatch.Postfix` junto com o resto da limpeza de fim de raid.
**Aplicação:** [`mods/Skills-Extended/modded/Plugin/Skills/ProneMovement/Patches/ProneMoveStatePatch.cs`](../../modded/Plugin/Skills/ProneMovement/Patches/ProneMoveStatePatch.cs) (novo método `ClearCachedAction`) e [`mods/Skills-Extended/modded/Plugin/Skills/Shared/Patches/OnGameStarted.cs`](../../modded/Plugin/Skills/Shared/Patches/OnGameStarted.cs) (chamada adicionada em `OnGameEndedPatch.Postfix` + `using` novo).

---

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-07 | Code review 01 criada via `/code-review` |
| 2026-09-07 | Aplicação automática via `/apply-code-review` — aplicado: CR-01-01 |
