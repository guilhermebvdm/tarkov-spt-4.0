# 001 — Desmembramento de Perna em Bots Vivos · Code Review 01

**Mod:** VisceralCombat
**Spec funcional:** [001-alive-leg-dismemberment-01-spec.md](001-alive-leg-dismemberment-01-spec.md)
**Spec técnica:** [001-alive-leg-dismemberment-02-spec-tech.md](001-alive-leg-dismemberment-02-spec-tech.md)
**Asbuild:** [001-alive-leg-dismemberment-05-asbuild.md](001-alive-leg-dismemberment-05-asbuild.md)
**Data:** 2026-08-11

> Análise crítica do código implementado por `/code-mod`. Cada achado recebe um ID `CR-01-MM` permanente. Resolver bloqueadores 🔴 via `/apply-code-review` antes de fechar o item.

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 0 · 🟢 Menores: 1 · ✅ Resolvidos: 0 · Total: 1

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-01-01 | F — Melhoria opcional | 🟢 Menor | Expor a taxa de dano por sangramento em vida como ConfigEntry no BepInEx | Rejeitado |

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

### CR-01-01 · F — Melhoria opcional · 🟢 Menor

**Expor a taxa de dano por sangramento em vida como ConfigEntry no BepInEx**

**Local:** [`mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Dismemberment.Classes/LivingDismembermentController.cs:128`](../../modded/VisceralCombat/VisceralCombat.Dismemberment.Classes/LivingDismembermentController.cs#L128)

**Problema:** A taxa de dano por sangramento de vida (`10f` HP/s) está atualmente codificada de forma rígida em `LivingDismembermentController.cs`.

```csharp
_player.ActiveHealthController.ApplyDamage(_dismemberedLeg, 10f, GClass3051.HeavyBleedingDamage);
```

**Por que importa:** Se o usuário quiser futuramente ajustar o tempo de agonia do bot (para aumentar ou diminuir a velocidade da morte), será necessário recompilar a DLL.

**Sugestão:** Vincular o valor `10f` a um `ConfigEntry<float>` no `VisceralEntry.cs` (ex: `LivingBleedDamageRate`), permitindo ajuste fino direto no menu F12 do BepInEx.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[x]` Rejeitar (deferir / aceitar como dívida): Mantido 10 HP/s direto no código como constante fixa.

**Resolução:** Ponto rejeitado pelo usuário em 2026-08-11. Valor mantido fixo em 10 HP/s no código sem expor no F12.

---

## Histórico

| Data | Evento |
| --- | --- |
| 2026-08-11 | Code review 01 criada via `/code-review` |
