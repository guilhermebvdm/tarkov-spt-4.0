# 010 — afinidade-cpu-topologia-threads · As-Built

**Mod:** TRL-CoreSight
**Spec funcional:** [010-afinidade-cpu-topologia-threads-01-spec.md](010-afinidade-cpu-topologia-threads-01-spec.md)
**Spec técnica:** [010-afinidade-cpu-topologia-threads-02-spec-tech.md](010-afinidade-cpu-topologia-threads-02-spec-tech.md)
**Última review técnica:** [010-afinidade-cpu-topologia-threads-03-spec-tech-review-01.md](010-afinidade-cpu-topologia-threads-03-spec-tech-review-01.md)
**Build inicial:** 2026-09-19

> Documentação **pós-implementação**. Reflete o estado real do código entregue pelo `/code-mod` e atualizado por `/apply-code-review`.

## Arquivos alterados (build inicial)

| Ação | Path | Resumo |
| --- | --- | --- |
| CRIADO | `mods/TRL-CoreSight/modded/Core/CpuTopologyManager.cs` | Gerenciador nativo Win32 de topologia física, parser de `GetLogicalProcessorInformationEx`, cálculo de máscaras seguras e aplicação de afinidade/prioridade |
| MODIFICADO | `mods/TRL-CoreSight/modded/Configuration/ModConfig.cs` | Adicionados `CpuAffinityMode` e `ProcessPriorityHigh` na seção `0. Afinidade de CPU & Threads`, enum `ECpuAffinityMode` e evento reativo `OnCpuSettingsChanged` |
| MODIFICADO | `mods/TRL-CoreSight/modded/Core/PerformanceManager.cs` | Integrada aplicação no início de raid (`OnRaidStarted`), escuta de mudanças de configuração no F12 e revalidação de afinidade no foco da janela (`OnApplicationFocus`) |
| MODIFICADO | `mods/TRL-CoreSight/modded/Plugin.cs` | Inicialização da topologia no boot (`Awake`) e bump SemVer para `0.4.16` |
| MODIFICADO | `mods/TRL-CoreSight/modded/TRL-CoreSight.csproj` | Incluído `Core/CpuTopologyManager.cs` no item de compilação e bump de versão para `0.4.16` |

## PA-01-MM resolvidos durante o build

| ID | Categoria · Impacto | Resumo da resolução |
| --- | --- | --- |
| PA-01-01 | C — Lógica · 🟡 | Implementado parser com leitura de offsets 32 e 24 como fallback para `GROUP_AFFINITY` x64, garantindo que as máscaras físicas nunca sejam lidas como zero |
| PA-01-02 | A — Gap · 🟡 | Implementado `ApplyFallbackTopology()` baseado em `Environment.ProcessorCount` que gera máscara alternada padrão (01010101...) caso a chamada Win32 falhe |
| PA-01-03 | B — Edge Case · 🟢 | Adicionado callback `OnApplicationFocus(bool hasFocus)` em `PerformanceManager.cs` para revalidar a máscara caso o Windows Scheduler redistribua threads durante Alt+Tab |

## Arquivos alterados (rodada PA-02 — correção de afinidade de processo → thread)

| Ação | Path | Resumo |
| --- | --- | --- |
| MODIFICADO | `mods/TRL-CoreSight/modded/Core/CpuTopologyManager.cs` | `PrimaryCluster` deixou de usar `SetProcessAffinityMask` (processo inteiro) e passou a usar `SetThreadAffinityMask` só na Main Thread (`PinCallingThreadToCluster`/`UnpinCallingThread`, chamados a partir do `Update()`); `Auto` não escolhe mais `PrimaryCluster`; removido código morto do `CalculateTargetMask` |
| MODIFICADO | `mods/TRL-CoreSight/modded/Configuration/ModConfig.cs` | Padrão de `CpuAffinityMode` alterado de `Auto` para `PhysicalCoresOnly`; adicionada `EnableFrametimeBenchmark` |
| CRIADO | `mods/TRL-CoreSight/modded/Core/FrametimeBenchmark.cs` | Gravador de frametime médio e 1% low em CSV, para validação empírica do ganho de cada modo de afinidade |
| MODIFICADO | `mods/TRL-CoreSight/modded/Core/PerformanceManager.cs` | Integrado `FrametimeBenchmark` ao ciclo de vida; `Update()` drena `NeedsMainThreadPin`/`NeedsMainThreadUnpin`; `Cleanup()` chama `RestoreOriginalAffinity()` |
| MODIFICADO | `mods/TRL-CoreSight/modded/Plugin.cs` | Removida a chamada a `ApplyAffinity()` do `Awake()` (mantida só `InitializeTopology()`); afinidade agora só é aplicada dentro da raid |

## PA-02-MM resolvidos (ver [010-afinidade-cpu-topologia-threads-03-spec-tech-review-02.md](010-afinidade-cpu-topologia-threads-03-spec-tech-review-02.md))

| ID | Categoria · Impacto | Resumo da resolução |
| --- | --- | --- |
| PA-02-01 | C — Lógica/Arquitetura · 🔴 | `PrimaryCluster` não restringe mais o processo inteiro; usa afinidade de thread só na Main Thread |
| PA-02-02 | C — Lógica/Configuração · 🟡 | `Auto` nunca escolhe `PrimaryCluster`; padrão de fábrica virou `PhysicalCoresOnly` |
| PA-02-03 | B — Ciclo de Vida · 🟢 | Afinidade só é aplicada dentro da raid; restaurada no fim |
| PA-02-04 | A — Gap/Observabilidade · 🟢 | Criado `FrametimeBenchmark.cs` para medição objetiva (média + 1% low) |

Além dos PA-02, corrigidos manualmente (fora do fluxo automatizado, antes do `/code-review`): condição de corrida entre `_needsMainThreadPin`/`_needsMainThreadUnpin` ao trocar de `PrimaryCluster` rapidamente, e remoção do `case PrimaryCluster` morto em `CalculateTargetMask()`.

## CR-01-MM aplicados (ver [010-afinidade-cpu-topologia-threads-04-code-review-01.md](010-afinidade-cpu-topologia-threads-04-code-review-01.md))

| ID | Categoria · Impacto | Resumo da resolução |
| --- | --- | --- |
| CR-01-01 | B — Bug latente · 🟡 | `FrametimeBenchmark.cs`: aviso único (log) + coluna `Truncated` no CSV quando o buffer de amostras enche (~16-17 min) |
| CR-01-02 | C — Gap vs. processo · 🟠 | `PROPRIEDADES.md` atualizado: padrão `PhysicalCoresOnly`, tooltip completo, nova linha `EnableFrametimeBenchmark` |
| CR-01-03 | D — Arquitetura/processo · 🟡 | `mod-backlog.md`: status do item 010 atualizado de `⚪` para `🔵` |

## Fix 01 (ver [010-afinidade-cpu-topologia-threads-06-fix-01.md](010-afinidade-cpu-topologia-threads-06-fix-01.md))

| Arquivo | Ação | Resumo |
| --- | --- | --- |
| MODIFICADO | `mods/TRL-CoreSight/modded/Core/PerformanceManager.cs` | `Cleanup()` ganhou guard de reentrância (`_cleanedUp`) — `GameWorldOnDestroyPatch` e o `OnDestroy()` nativo da Unity no mesmo GameObject disparavam `Cleanup()` duas vezes sem proteção; `CpuTopologyManager.RestoreOriginalAffinity()` movido para a primeira linha do método, garantindo que a afinidade seja restaurada mesmo se algum sub-manager mais adiante lançar exceção durante o teardown |

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-19 | Build v0.4.16 concluído via `/code-mod` com 0 erros e 0 avisos |
| 2026-09-19 | Correção PA-02 (afinidade de processo → thread) aplicada via handoff externo + fix manual; recompilado com 0 erros e 0 avisos |
| 2026-09-19 | Code review 01 aplicada (CR-01-01 a CR-01-03) via `/apply-code-review`; recompilado com 0 erros e 0 avisos |
| 2026-09-19 | Fix 01 aplicado (Cleanup() idempotente + reordenação da restauração de afinidade), a partir de feedback in-raid de 2 raids reais; v0.4.17 → v0.4.18 instalado via `/compile-mod` |
