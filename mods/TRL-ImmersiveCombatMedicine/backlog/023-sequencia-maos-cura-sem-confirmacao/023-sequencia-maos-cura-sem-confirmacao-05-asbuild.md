# 023 — Sequência de mãos da cura sem confirmação de operação · As-Built

**Mod:** TRL-ImmersiveCombatMedicine
**Spec funcional:** [023-sequencia-maos-cura-sem-confirmacao-01-spec.md](023-sequencia-maos-cura-sem-confirmacao-01-spec.md)
**Spec técnica:** [023-sequencia-maos-cura-sem-confirmacao-02-spec-tech.md](023-sequencia-maos-cura-sem-confirmacao-02-spec-tech.md)
**Última review técnica:** [023-sequencia-maos-cura-sem-confirmacao-03-spec-tech-review-01.md](023-sequencia-maos-cura-sem-confirmacao-03-spec-tech-review-01.md)
**Build inicial:** 2026-09-12

> Documentação **pós-implementação**. Reflete o estado real do código entregue pelo `/code-mod`. Quando o conteúdo aqui diverge da spec técnica, este documento ganha — a spec é planejamento, o asbuild é o que foi feito.

## Arquivos alterados (build inicial)

| Ação | Path | Resumo |
| --- | --- | --- |
| MODIFICADO | `mods/TRL-ImmersiveCombatMedicine/modded-V4/Patches/Medical/BandAidController.cs` | `HealRoutine`: `SetInHands` passa a usar o callback nativo (`setInHandsConfirmed`) em vez de lambda vazio; loga se não confirmar dentro do `UseTime`. `EmergencyDrop` (PASSO 3): `TrySetLastEquippedWeapon` ganhou callback de log (antes `null`) — sequenciamento síncrono com o PASSO 4 (`ThrowItem`) mantido idêntico ao original. |

Nenhum arquivo criado, nenhum novo Harmony patch, nenhuma nova classe — conforme §1/§4 da spec técnica.

## PA-NN-MM resolvidos durante o build

> Pontos da review técnica 01 que já tinham sido aplicados **na própria spec técnica** antes deste build (não corrigidos aqui pela primeira vez — o código já nasceu implementando a versão revisada).

| ID | Categoria · Impacto | Resumo da resolução |
| --- | --- | --- |
| PA-01-01 | C — Erro de Lógica · 🔴 | Stubs implementados sem acessar nenhum membro de `result` (`Callback<T>`/`Result<T>` não decompilados) — só usam a invocação do callback como sinal. Compilação real confirmou: 0 erros, 0 avisos. |
| PA-01-02 | B — Edge Case · 🟡 | `EmergencyDrop` implementado sem coroutine nova — `ThrowItem` continua síncrono e imediato, exatamente como antes. Callback de `TrySetLastEquippedWeapon` só loga, não gateia nada. |

## Mudanças posteriores

> Atualizado por `/apply-code-review` a cada rodada.

### Rodada de code-review 01 — 2026-09-12

| ID | Status | Resumo |
| --- | --- | --- |
| CR-01-01 | ✅ Aplicado | `result.Error` (string) restaurado nos dois callbacks — falha real agora é logada, não só timeout. Achado extra na aplicação: `Result<T>` é struct (`result != null` não compila) e o lambda implícito ficava ambíguo entre as sobrecargas de `SetInHands(Item, Callback<T>)` — resolvido com delegates de tipo explícito (`Callback<IHandsController>` e `Callback`/`IResult`). |

Arquivo tocado: `mods/TRL-ImmersiveCombatMedicine/modded-V4/Patches/Medical/BandAidController.cs` (mesmo arquivo do build inicial, sem arquivo novo). Build Release: 0 erros, 0 avisos.

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-12 | Build concluído via `/code-mod`. Compilação Release local em `modded-V4` verificada: 0 erros, 0 avisos. Validação in-game (tempo de cura percebido, raid coop sob rajada de spawn de bot cenário item 007 do FIKA, `EmergencyDrop` sob latência) ainda pendente — ver checklist §8 da spec técnica. |
| 2026-09-12 | Versão bumpada `1.14.0 → 1.14.1` (patch — fix de sequenciamento) em `TRLImmersiveCombatMedicinePlugin.cs` e `.csproj`. `/compile-mod` **não roda para este mod** — o script espera literalmente uma pasta `modded/`, mas este mod usa a convenção versionada `modded-V1`/`V2`/`V3`/`V4` (falha com "pasta modded ausente"). Build local final feito via `dotnet build -c Release` direto em `modded-V4/`: 0 erros, 0 avisos. `.dll` gerado em `modded-V4/bin/Release/netstandard2.1/TRLImmersiveCombatMedicine.dll` — **não instalado em `D:\SPT`/no jogo**, conforme pedido. |
| 2026-09-12 | Aplicação de 1 achado de code-review 01 via `/apply-code-review` — ID: `CR-01-01`. Ver "Mudanças posteriores" acima. Build Release final: 0 erros, 0 avisos. |
