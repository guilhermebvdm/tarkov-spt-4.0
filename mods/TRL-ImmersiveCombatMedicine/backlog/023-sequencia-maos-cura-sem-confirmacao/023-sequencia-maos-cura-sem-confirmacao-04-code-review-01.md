# 023 — Sequência de mãos da cura sem confirmação de operação · Code Review 01

**Mod:** TRL-ImmersiveCombatMedicine
**Spec funcional:** [023-sequencia-maos-cura-sem-confirmacao-01-spec.md](023-sequencia-maos-cura-sem-confirmacao-01-spec.md)
**Spec técnica:** [023-sequencia-maos-cura-sem-confirmacao-02-spec-tech.md](023-sequencia-maos-cura-sem-confirmacao-02-spec-tech.md)
**Asbuild:** [023-sequencia-maos-cura-sem-confirmacao-05-asbuild.md](023-sequencia-maos-cura-sem-confirmacao-05-asbuild.md)
**Data:** 2026-09-12

> Análise crítica do código implementado por `/code-mod`. Cada achado recebe um ID `CR-01-MM` permanente. Resolver bloqueadores 🔴 via `/apply-code-review` antes de fechar o item.

**Memória consultada:** topo de `mods/TRL-ImmersiveCombatMedicine/memory/sessions.md` (Sessão 12) · pendências que afetam este item: nenhuma.
**Docs técnicos:** só o obrigatório (`spt-antipatterns.md`) — nenhum outro gatilho do roteamento se aplica.

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 1 · Total: 1

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-01-01 | C — Gap vs. spec | 🟠 Forte | `.Error` do callback existe (confirmado agora por compilador) mas não é usado — AC de "logar falha real" só é parcialmente atendido | ✅ Aplicado em 2026-09-12 |

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

### CR-01-01 · C — Gap vs. spec · 🟠 Forte · ✅ Aplicado em 2026-09-12

**`.Error` do callback existe de verdade (confirmado agora por compilador) mas não é usado — AC-3 da spec funcional só parcialmente atendido**

**Local:** [`mods/TRL-ImmersiveCombatMedicine/modded-V4/Patches/Medical/BandAidController.cs:591-605`](../../modded-V4/Patches/Medical/BandAidController.cs#L591-L605) (`HealRoutine`) e [`:494-513`](../../modded-V4/Patches/Medical/BandAidController.cs#L494-L513) (`EmergencyDrop`)

**Problema:** A spec técnica (§7, `PA-01-01`) deixou registrado como incerteza: `Callback<T>`/`Result<T>` não são decompilados, então os stubs foram simplificados pra **não** acessar `result.Error`, só usar a invocação do callback como sinal — com a nota explícita "Detalhe de sucesso/erro fica como melhoria futura, adicionada só depois que o compilador do `/code-mod` confirmar os membros reais."

O `/code-mod` rodou, compilou limpo, e essa confirmação **nunca foi feita** — o código implementado ficou exatamente como o stub simplificado, sem revisitar a questão. Testei agora, nesta review, com um experimento de compilação isolado (adicionar `result.Error` nos dois callbacks e observar o erro/sucesso do `dotnet build`):

```csharp
// HealRoutine (linha ~600) — testado, compila:
doctor.SetInHands(itemUsed, (result) => { var x = result.Error; });

// EmergencyDrop (linha ~503) — testado, compila:
doctor.TrySetLastEquippedWeapon(true, (result) => { var x = result.Error; });
```

Forçando um erro de tipo (`int x = result.Error;`) o compilador confirmou, nos dois casos: `error CS0029: Não é possível converter implicitamente tipo "string" em "int"` — ou seja, **`result.Error` existe e é `string` nos dois callbacks**, exatamente a forma que a primeira versão (pré-review) da spec técnica já assumia antes do `PA-01-01` pedir cautela.

**Por que importa:** o critério de aceite AC-3 da spec funcional diz: *"O callback de `SetInHands` deixa de ser ignorado — uma falha real da operação (ex: hands controller rejeitando) é logada e tratada, não silenciosamente descartada."* O código atual só loga em `HealRoutine` quando o callback **nunca dispara** dentro do `UseTime` (um proxy de timeout) — mas se o callback disparar **rápido, com uma falha real** (`result.Error` não-vazio), `setInHandsConfirmed` vira `true` do mesmo jeito e **nenhum aviso é logado**. Uma falha real de verdade passa batido — exatamente o "silenciosamente descartada" que o AC-3 pede pra evitar. Em `EmergencyDrop`, o log fixo `"...confirmou conclusão..."` dispara mesmo que `result.Error` indique falha, o que é uma mensagem enganosa nesse cenário.

**Sugestão:** já que a incerteza que motivou `PA-01-01` está resolvida (evidência de compilador, a fonte de maior confiança), voltar a inspecionar `result.Error` nos dois callbacks:

```csharp
// HealRoutine:
doctor.SetInHands(itemUsed, (result) =>
{
    setInHandsConfirmed = true;
    if (result != null && !string.IsNullOrEmpty(result.Error))
    {
        TRLImmersiveCombatMedicinePlugin.ModLogger.LogWarning(
            $"HealRoutine: SetInHands falhou ({result.Error}) — item={itemUsed.ShortName.Localized()}, patient={patient.Profile.Nickname}");
    }
});
```

```csharp
// EmergencyDrop:
doctor.TrySetLastEquippedWeapon(true, (result) =>
{
    if (result != null && !string.IsNullOrEmpty(result.Error))
        TRLImmersiveCombatMedicinePlugin.ModLogger.LogWarning($"EmergencyDrop: TrySetLastEquippedWeapon falhou ({result.Error}).");
    else
        TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo("EmergencyDrop: TrySetLastEquippedWeapon confirmou conclusão (log assíncrono, não bloqueia o drop).");
});
```

Também atualizar a spec técnica §5/§7 (remover a nota de incerteza, registrar a confirmação) já que essa é justamente a "melhoria futura" que ela mesma previu.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Sugestão aplicada, com um ajuste descoberto só na hora de compilar de verdade (não previsto no texto original do achado): `Result<T>` é **struct** (tipo valor), não classe — `result != null` não compila (`CS0019`). Foi só descoberto porque o primeiro reflexo (reaplicar exatamente a sugestão com lambda implícito `(result) => {...}`) deu um erro de compilação **diferente e confuso** (`Result<IHandsThrowController>` em vez de `Result<IHandsController>` — a inferência de tipo entre as várias sobrecargas de `SetInHands(Item, Callback<T>)` fica ambígua quando o lambda não referencia nenhum membro do tipo genérico). Resolvido trocando os dois lambdas implícitos por delegates com **tipo explícito** (`Callback<IHandsController>` em `HealRoutine`, `Callback` não-genérico com `IResult` em `EmergencyDrop`) e removendo a checagem `!= null` do `Result<T>` (struct, nunca é null) — mantida só em `EmergencyDrop`, onde o parâmetro é a interface `IResult` (tipo referência, `!= null` válido ali).

**Aplicação:** `mods/TRL-ImmersiveCombatMedicine/modded-V4/Patches/Medical/BandAidController.cs` — `HealRoutine` (~linha 594-622) e `EmergencyDrop` (~linha 494-516). Build Release confirmado: 0 erros, 0 avisos.

---

## Verificações sem achado

- **Refs ao Assembly:** `Player.cs:31845` (`SetInHands`), `:31800` (`TrySetLastEquippedWeapon`), `:32003` (`TryProceed`) conferidas contra o dump atual — batem com o que a spec técnica cita.
- **Sandbox:** só `modded-V4/` tocado, `original/` intocado.
- **AP-02 (MainPlayer/Fika):** sequência só roda pro médico local (Ownership Guard já existente em `MedicHealPatch.cs`), não alterado por este item.
- **AP-08 (flags stale):** `setInHandsConfirmed` é variável local por execução da coroutine — sem risco de vazamento entre curas.
- **Build:** `dotnet build -c Release` em `modded-V4` — 0 erros, 0 avisos, consistente com o asbuild.
- **Sem código morto, sem duplicação, sem patch novo** introduzido pelo diff.

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-12 | Code review 01 criada via `/code-review`. 1 achado (`CR-01-01`, 🟠 Forte) — confirmado por experimento de compilação que `result.Error` existe e funciona nos dois callbacks; a spec técnica já havia previsto essa checagem como "melhoria futura" pendente de confirmação do compilador. |
| 2026-09-12 | Aplicação automática de 1 achado via `/apply-code-review` — ID aplicado: `CR-01-01`. Achado extra descoberto na aplicação (não estava no texto original): `Result<T>` é struct, exigiu tipagem explícita dos callbacks em vez de lambda implícito. Build Release: 0 erros, 0 avisos. |
