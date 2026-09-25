# 010 — afinidade-cpu-topologia-threads · Review Técnica 02

**Mod:** TRL-CoreSight
**Spec técnica revisada:** [010-afinidade-cpu-topologia-threads-02-spec-tech.md](010-afinidade-cpu-topologia-threads-02-spec-tech.md)
**Data:** 2026-09-19

> Análise crítica pós-implementação da afinidade de hardware e topologia de CPU. Cada ponto recebe um ID `PA-02-MM` (review 02, ponto MM).

## Resumo

> 🔴 Bloqueadores: 1 · 🟡 Importantes: 1 · 🟢 Menores: 2 · ✅ Resolvidos: 4 · Total: 4

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-02-01 | C — Lógica / Arquitetura | 🔴 Bloqueador | Afinidade de processo inteiro no PrimaryCluster asfixia threads paralelas e SAIN | ✅ Resolvido em 2026-09-19 |
| PA-02-02 | C — Lógica / Configuração | 🟡 Importante | Modo Auto seleciona PrimaryCluster automaticamente, cortando núcleos sem consentimento | ✅ Resolvido em 2026-09-19 |
| PA-02-03 | B — Ciclo de Vida | 🟢 Menor | Aplicação precoce de afinidade no menu principal (`Plugin.Awake`) | ✅ Resolvido em 2026-09-19 |
| PA-02-04 | A — Gap / Observabilidade | 🟢 Menor | Ausência de telemetria objetiva (frametimes e 1% lows) para validação empírica | ✅ Resolvido em 2026-09-19 |

---

## Pontos

### PA-02-01 · C — Lógica / Arquitetura · 🔴 Bloqueador

**Afinidade de processo inteiro no PrimaryCluster asfixia threads paralelas e SAIN**

**Problema:** A implementação inicial em `CpuTopologyManager.cs` aplicava a máscara de hardware via `SetProcessAffinityMask(GetCurrentProcess(), ...)`. Quando o modo `PrimaryCluster` é selecionado (ou escolhido pelo `Auto`), a máscara é restrita ao primeiro bloco de cache L3 (por exemplo, 4 núcleos físicos / 8 threads lógicas em processadores AMD Ryzen como 3700X, 3800X, 5800X ou configurações multi-CCX/CCD). Como `SetProcessAffinityMask` atinge todo o processo `EscapeFromTarkov.exe`, todas as threads auxiliares da Unity (Job System, PhysX, render worker threads) e, criticamente, as threads concorrentes de cálculo de decisão sensorial de IA do mod SAIN são forçadas a concorrer pelo mesmo conjunto limitado de núcleos, gerando severo afunilamento de escalabilidade.

**Por que importa:** O propósito da otimização é impedir a penalidade de latência de barramento inter-CCX (Infinity Fabric) na Main Thread da Unity (onde rodam os loops principais de gameplay e renderização). Ao confinar todo o processo a um único CCX, o ganho de cache da Main Thread é anulado pela sobrecarga de disputa de CPU com as threads de física e IA, podendo diminuir o desempenho em vez de aumentá-lo em cenários com múltiplos bots ativos.

**Sugestão:** Utilizar afinidade a nível de thread individual (`SetThreadAffinityMask`) exclusivamente para a Main Thread no modo `PrimaryCluster`. Manter a afinidade de processo livre (todas as threads permitidas pelo SO) para que o sistema operacional e o Job System continuem distribuindo as tarefas pesadas de fundo em todos os núcleos físicos do processador. Manter `SetProcessAffinityMask` apenas para os modos `PhysicalCoresOnly` e `PerformanceCores`, onde núcleos físicos reais não são excluídos da pool global de processamento do jogo.

**Decisão:**
- `[x]` Aceitar sugestão (aplicar `SetThreadAffinityMask` da Main Thread dentro do ciclo `Update()` do Unity)

**Resolução:** Adicionados P/Invokes `SetThreadAffinityMask` e `GetCurrentThread` na `CpuTopologyManager.cs`. Criado o método `PinCallingThreadToCluster()` a ser invocado no `Update()` do `PerformanceManager`. No modo `PrimaryCluster`, a afinidade do processo é mantida na totalidade de núcleos e apenas a thread principal é fixada ao cluster primário de L3.

---

### PA-02-02 · C — Lógica / Configuração · 🟡 Importante

**Modo Auto seleciona PrimaryCluster automaticamente, cortando núcleos sem consentimento**

**Problema:** No método `CpuTopologyManager.CalculateTargetMask()`, a regra do modo `ECpuAffinityMode.Auto` continha verificação para processadores com múltiplos blocos de cache L3 (`_primaryL3Mask != 0 && CountBits(_primaryL3Mask) >= 4`), elegendo automaticamente a máscara do CCX 0. Além disso, `CpuAffinityMode` tinha como valor padrão no `ModConfig.cs` a opção `Auto`. Isso fazia com que qualquer jogador utilizando CPUs AMD multi-CCX/CCD tivesse metade dos núcleos do processador amputados silenciosamente logo ao instalar o mod, sem aviso prévio.

**Por que importa:** O modo `Auto` deve representar a melhor prática universal segura, sem risco de regressão de desempenho. Confinar clusters de núcleos físicos deve ser uma opção de sintonia avançada opt-in (`PrimaryCluster`), e nunca uma decisão automática imposta pelo perfil padrão.

**Sugestão:** Alterar o valor padrão de fábrica de `CpuAffinityMode` para `ECpuAffinityMode.PhysicalCoresOnly` (que apenas desativa SMT secundário, mantendo todos os núcleos físicos disponíveis). No `CalculateTargetMask()`, remover a seleção de `_primaryL3Mask` dentro do ramo `Auto`, restringindo o `Auto` a escolher entre P-Cores em CPUs híbridas Intel ou desativação de SMT em CPUs com 6+ threads. Atualizar o tooltip do F12 documentando o comportamento de cada modo.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Padrão de fábrica do `CpuAffinityMode` redefinido para `PhysicalCoresOnly`. Lógica do `Auto` simplificada para não tocar em cortes de CCX/CCD. Tooltip no `ModConfig.cs` atualizado com orientações detalhadas de teste e recomendação de validação empírica.

---

### PA-02-03 · B — Ciclo de Vida · 🟢 Menor

**Aplicação precoce de afinidade no menu principal (`Plugin.Awake`)**

**Problema:** Em `Plugin.cs`, o método `Awake()` chamava `CpuTopologyManager.ApplyAffinity()` assim que o mod era carregado pelo BepInEx. No menu principal, trading e hideout, o jogo não apresenta sobrecarga pesada de IA ou física, e impor máscaras restritivas fora da partida pode concorrer com outros processos do jogador (streaming, navegadores, Discord). Além disso, ao término de uma raid, `PerformanceManager.Cleanup()` não chamava `CpuTopologyManager.RestoreOriginalAffinity()`, mantendo a afinidade retida indefinidamente no processo.

**Por que importa:** O escopo de intervenção de hardware de baixo nível deve ser estritamente restrito ao ciclo de vida da partida (em raid).

**Sugestão:** Remover `CpuTopologyManager.ApplyAffinity()` do `Plugin.Awake()` (mantendo apenas `InitializeTopology()` para introspecção de hardware). Assegurar que a afinidade seja ativada exclusivamente em `PerformanceManager.Initialize()` (início de raid) e que `RestoreOriginalAffinity()` seja invocado no `PerformanceManager.Cleanup()` (retorno ao menu / destruição do gerenciador).

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Removida chamada de aplicação em `Plugin.Awake()`. Integrada restauração de afinidade de processo e desvinculação de thread em `PerformanceManager.Cleanup()`.

---

### PA-02-04 · A — Gap / Observabilidade · 🟢 Menor

**Ausência de telemetria objetiva (frametimes e 1% lows) para validação empírica**

**Problema:** A avaliação de ganhos de performance de afinidade e topologia no SPT frequentemente sofre de viés de percepção subjetiva do jogador decorrente da flutuação imprevisível na quantidade de bots vivos e eventos dinâmicos entre diferentes raids. O mod carecia de uma ferramenta integrada para medição estatística concreta de estabilidade de taxa de quadros (frametime médio e 1% low).

**Por que importa:** Sem telemetria determinística, é impossível validar com precisão se a fixação da Main Thread ou corte de SMT gerou estabilização real de frame pacing em um hardware específico.

**Sugestão:** Desenvolver o componente `FrametimeBenchmark.cs` (MonoBehaviour acoplado a `PerformanceManager`), ativado opcionalmente via configuração no F12 (`EnableFrametimeBenchmark`). O componente deve registrar os deltas de frame sem escala (`Time.unscaledDeltaTime`) e, ao final da partida, persistir em arquivo CSV append (`BepInEx/plugins/TRL-CoreSight/benchmark_frametimes.csv`) a média de frametime, FPS médio, 1% low e o modo de afinidade utilizado.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Criado o componente `FrametimeBenchmark.cs` integrado ao ciclo do `PerformanceManager`. Adicionada a opção `EnableFrametimeBenchmark` em `ModConfig.cs` sob a categoria `0. Afinidade de CPU & Threads`.
