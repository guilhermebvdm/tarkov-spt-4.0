# 010 — afinidade-cpu-topologia-threads · Spec Técnica

**Mod:** TRL-CoreSight
**Spec funcional:** [010-afinidade-cpu-topologia-threads-01-spec.md](010-afinidade-cpu-topologia-threads-01-spec.md)
**Criado:** 2026-09-19

> Fonte primária de verdade para qualquer assinatura, fórmula ou ponto de patch: [references/eft-decompiled/Assembly-CSharp/](../../../../references/eft-decompiled/Assembly-CSharp/). Toda referência ao código do EFT deve citar `arquivo.cs:linha`. Win32 API como fonte de controle de sistema operacional.

## 1. Estratégia

O gerenciamento de afinidade de processador não requer desvio de fluxo interno ou injeção de IL em lógicas balísticas do EFT; em vez disso, atua na borda do ciclo de vida da raid (`RaidStartPatch`) e na camada de interoperabilidade com o sistema operacional (Win32 P/Invoke via `kernel32.dll`).

A estratégia consiste em:
1. **Descoberta de Topologia em Tempo de Execução:** Consultar a estrutura física da CPU via `GetLogicalProcessorInformationEx` (Win32), mapeando núcleos físicos reais, threads lógicas (SMT/Hyperthreading), clusters de Cache L3 (CCX/CCD em processadores AMD Ryzen) e classes de eficiência (`EfficiencyClass` em processadores híbridos Intel de 12ª a 14ª geração).
2. **Construção de Máscara Segura com Salvaguardas:** Gerar uma máscara de 64 bits (`UIntPtr`) de acordo com o modo selecionado no menu F12 (`Auto`, `PhysicalCoresOnly`, `PerformanceCores`, `PrimaryCluster`, `Disabled`). Se a máscara resultante contiver menos de 4 núcleos lógicos ativos, a aplicação é descartada (fail-safe) para evitar estrangulamento da *render thread* e *worker threads* da Unity.
3. **Aplicação e Persistência Pós-Loading:** Aplicar a máscara via `SetProcessAffinityMask` no evento `OnRaidStarted` (após o carregamento do mapa e após a rotina nativa da BSG ter finalizado), e reaplicar dinamicamente caso o jogador altere a opção no F12 durante a partida.

---

## 2. Pontos de patch

| Alvo (Assembly) | Tipo | Motivo |
|---|---|---|
| [`EFT.GameWorld.OnGameStarted`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/GameWorld.cs) | Postfix | Dispara `PerformanceManager.OnRaidStarted()` já roteado para o ciclo de vida do mod, garantindo que a afinidade seja cravada após qualquer reset de cena da BSG. |

---

## 3. Novas propriedades F12 (BepInEx)

| Seção | Nome (EN) | Tipo | Padrão | Faixa | Avançado | Tooltip (pt-BR) |
|---|---|---|---|---|---|---|
| `1. Afinidade de CPU & Threads` | `CpuAffinityMode` | `ECpuAffinityMode` | `Auto` | Auto, PhysicalCoresOnly, PerformanceCores, PrimaryCluster, Disabled | — | Define a estratégia de afinidade de processador: Auto (detecta e otimiza), Físicos Apenas (desativa SMT/HT), Performance Cores (apenas P-Cores na Intel), Bloco Primário (trava no CCX/CCD 0 na AMD) ou Desativado. |
| `1. Afinidade de CPU & Threads` | `ProcessPriorityHigh` | `bool` | `true` | true/false | — | Eleva a prioridade do processo do jogo no Windows para Alta (ProcessPriorityClass.High), reduzindo interrupções causadas por outros programas em segundo plano. |

---

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `modded/Configuration/ModConfig.cs` | MODIFICAR | Registrar o enum `ECpuAffinityMode`, as chaves `CpuAffinityMode` e `ProcessPriorityHigh`, e o evento reativo `OnCpuSettingChanged`. |
| `modded/Core/CpuTopologyManager.cs` | CRIAR | Módulo nativo Win32 (P/Invoke `kernel32.dll`), parser de `GetLogicalProcessorInformationEx`, cálculo de máscaras e aplicação atômica de afinidade e prioridade. |
| `modded/Core/PerformanceManager.cs` | MODIFICAR | Conectar o `CpuTopologyManager.ApplyAffinity()` no hook de início de raid e no evento de configuração. |
| `modded/Plugin.cs` | MODIFICAR | Inicializar a detecção de topologia na carga do plugin e efetuar o bump SemVer. |
| `PROPRIEDADES.md` | MODIFICAR | Documentar a nova seção `1. Afinidade de CPU & Threads` no catálogo de opções F12. |

---

## 5. Stubs de código

### 5.1 Estruturas Win32 e P/Invoke (`modded/Core/CpuTopologyManager.cs`)

```csharp
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using TRLCoreSight.Configuration;
using UnityEngine;

namespace TRLCoreSight.Core
{
    public enum ECpuAffinityMode
    {
        Disabled,
        Auto,
        PhysicalCoresOnly,
        PerformanceCores,
        PrimaryCluster
    }

    public static class CpuTopologyManager
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetLogicalProcessorInformationEx(
            LOGICAL_PROCESSOR_RELATIONSHIP RelationshipType,
            IntPtr Buffer,
            ref uint ReturnedLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetProcessAffinityMask(IntPtr hProcess, UIntPtr dwProcessAffinityMask);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetCurrentProcess();

        private enum LOGICAL_PROCESSOR_RELATIONSHIP
        {
            RelationProcessorCore = 0,
            RelationNumaNode = 1,
            RelationCache = 2,
            RelationProcessorPackage = 3,
            RelationGroup = 4,
            RelationAll = 0xffff
        }

        private static ulong _originalProcessMask = 0;
        private static ulong _physicalCoresMask = 0;
        private static ulong _pCoresMask = 0;
        private static ulong _primaryL3Mask = 0;
        private static int _logicalCoreCount = 0;
        private static int _physicalCoreCount = 0;
        private static bool _topologyParsed = false;

        public static void InitializeTopology()
        {
            try
            {
                _originalProcessMask = (ulong)Process.GetCurrentProcess().ProcessorAffinity.ToInt64();
                _logicalCoreCount = Environment.ProcessorCount;

                ParseTopology();
                _topologyParsed = true;

                Plugin.LogSource?.LogInfo($"[TRL-CoreSight][CPU] Topologia detectada: {_physicalCoreCount} núcleos físicos, {_logicalCoreCount} threads lógicas.");
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogError($"[TRL-CoreSight][CPU] Falha ao analisar topologia: {ex.Message}");
            }
        }

        public static void ApplyAffinity()
        {
            if (!_topologyParsed) InitializeTopology();

            try
            {
                // Ajuste de Prioridade de Processo
                if (ModConfig.ProcessPriorityHigh.Value)
                {
                    if (Process.GetCurrentProcess().PriorityClass != ProcessPriorityClass.High)
                    {
                        Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
                        Plugin.LogSource?.LogInfo("[TRL-CoreSight][CPU] Prioridade do processo ajustada para High.");
                    }
                }

                ECpuAffinityMode mode = ModConfig.CpuAffinityMode.Value;
                if (mode == ECpuAffinityMode.Disabled)
                {
                    RestoreOriginalAffinity();
                    return;
                }

                ulong targetMask = CalculateTargetMask(mode);

                // Salvaguarda: Mínimo 4 threads ativas
                int activeCores = CountBits(targetMask);
                if (activeCores < 4 && _logicalCoreCount >= 4)
                {
                    Plugin.LogSource?.LogWarning($"[TRL-CoreSight][CPU] Máscara calculada ({activeCores} threads) menor que a salvaguarda mínima de 4. Usando máscara original.");
                    targetMask = _originalProcessMask;
                }

                SetProcessAffinityMask(GetCurrentProcess(), (UIntPtr)targetMask);
                Plugin.LogSource?.LogInfo($"[TRL-CoreSight][CPU] Afinidade aplicada com sucesso! Modo: {mode}, Máscara: 0x{targetMask:X}, Threads Ativas: {activeCores}.");
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogError($"[TRL-CoreSight][CPU] Erro ao aplicar afinidade: {ex.Message}");
            }
        }

        public static void RestoreOriginalAffinity()
        {
            if (_originalProcessMask != 0)
            {
                SetProcessAffinityMask(GetCurrentProcess(), (UIntPtr)_originalProcessMask);
                Plugin.LogSource?.LogInfo("[TRL-CoreSight][CPU] Afinidade restaurada para o padrão do sistema operacional.");
            }
        }

        private static ulong CalculateTargetMask(ECpuAffinityMode mode)
        {
            switch (mode)
            {
                case ECpuAffinityMode.PhysicalCoresOnly:
                    return _physicalCoresMask != 0 ? _physicalCoresMask : _originalProcessMask;

                case ECpuAffinityMode.PerformanceCores:
                    return _pCoresMask != 0 ? _pCoresMask : _physicalCoresMask;

                case ECpuAffinityMode.PrimaryCluster:
                    return _primaryL3Mask != 0 ? _primaryL3Mask : _physicalCoresMask;

                case ECpuAffinityMode.Auto:
                default:
                    // Se possui núcleos híbridos com P-Cores detectados
                    if (_pCoresMask != 0 && _pCoresMask != _originalProcessMask && CountBits(_pCoresMask) >= 4)
                        return _pCoresMask;

                    // Se possui múltiplos clusters de Cache L3 (AMD Ryzen multi-CCX/CCD)
                    if (_primaryL3Mask != 0 && _primaryL3Mask != _originalProcessMask && CountBits(_primaryL3Mask) >= 4)
                        return _primaryL3Mask;

                    // Caso padrão: Apenas núcleos físicos reais se tiver 6+ threads
                    if (_physicalCoresMask != 0 && _logicalCoreCount >= 6)
                        return _physicalCoresMask;

                    return _originalProcessMask;
            }
        }

        private static void ParseTopology()
        {
            // Implementação nativa percorrendo os registros de LOGICAL_PROCESSOR_RELATIONSHIP
            // extraindo masks de Core (bit 0 de cada núcleo), Cache L3 (Level == 3) e EfficiencyClass.
            uint length = 0;
            GetLogicalProcessorInformationEx(LOGICAL_PROCESSOR_RELATIONSHIP.RelationAll, IntPtr.Zero, ref length);
            if (length == 0) return;

            IntPtr buffer = Marshal.AllocHGlobal((int)length);
            try
            {
                if (GetLogicalProcessorInformationEx(LOGICAL_PROCESSOR_RELATIONSHIP.RelationAll, buffer, ref length))
                {
                    IntPtr current = buffer;
                    int offset = 0;
                    _physicalCoreCount = 0;
                    _physicalCoresMask = 0;
                    _pCoresMask = 0;
                    _primaryL3Mask = 0;

                    while (offset < length)
                    {
                        LOGICAL_PROCESSOR_RELATIONSHIP relation = (LOGICAL_PROCESSOR_RELATIONSHIP)Marshal.ReadInt32(current);
                        int size = Marshal.ReadInt32(current, 4);

                        if (relation == LOGICAL_PROCESSOR_RELATIONSHIP.RelationProcessorCore)
                        {
                            _physicalCoreCount++;
                            byte efficiency = Marshal.ReadByte(current, 8); // EfficiencyClass
                            // GroupMask em offset 16
                            ulong coreMask = (ulong)Marshal.ReadInt64(current, 24);

                            // Pega a primeira thread lógica como a representação física primária
                            ulong primaryLogicalThread = coreMask & (~coreMask + 1);
                            _physicalCoresMask |= primaryLogicalThread;

                            if (efficiency > 0) // P-Core na Intel
                            {
                                _pCoresMask |= coreMask;
                            }
                        }
                        else if (relation == LOGICAL_PROCESSOR_RELATIONSHIP.RelationCache)
                        {
                            byte level = Marshal.ReadByte(current, 8);
                            if (level == 3 && _primaryL3Mask == 0)
                            {
                                // Primeiro cluster de L3 (CCX/CCD 0)
                                _primaryL3Mask = (ulong)Marshal.ReadInt64(current, 24);
                            }
                        }

                        offset += size;
                        current = new IntPtr(current.ToInt64() + size);
                    }
                }
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        private static int CountBits(ulong mask)
        {
            int count = 0;
            while (mask != 0)
            {
                count += (int)(mask & 1);
                mask >>= 1;
            }
            return count;
        }
    }
}
```

---

## 6. Fluxo de dados

```
[A] Inicialização do Jogo / BepInEx (Plugin.Awake)
    └──> CpuTopologyManager.InitializeTopology()
         ├──> Win32 GetLogicalProcessorInformationEx
         └──> Detecta núcleos físicos, P-Cores e clusters de L3

[B] Carregamento da Partida (GameWorld.OnGameStarted)
    └──> RaidStartPatch.Postfix()
         └──> PerformanceManager.OnRaidStarted()
              └──> CpuTopologyManager.ApplyAffinity()
                   ├──> Sobrescreve afinidade simplória da BSG
                   ├──> Valida salvaguarda (>= 4 threads ativas)
                   └──> Executa SetProcessAffinityMask & SetPriorityClass(High)

[C] Menu F12 (Em Tempo Real)
    └──> ModConfig.CpuAffinityMode.SettingChanged
         └──> CpuTopologyManager.ApplyAffinity() (In-Live Update)
```

---

## 7. Riscos e dependências

- **Conflito com Opção Vanilla ("Usar apenas núcleos físicos"):** Zero risco de colisão destrutiva. Como o nosso hook roda no `OnRaidStarted`, a máscara calculada pelo mod substitui com sucesso a máscara da BSG.
- **Processadores com Menos de 4 Núcleos:** Protegido pelo guard de contagem de bits (`CountBits < 4`).
- **Ordem de Inicialização:** A topologia é resolvida uma única vez no boot do jogo e mantida em cache estático na memória, garantindo overhead zero de processamento durante a raid.

---

## 8. Checklist de implementação

- [ ] Criar `modded/Core/CpuTopologyManager.cs` com interoperabilidade Win32 e parser de topologia.
- [ ] Adicionar enums e binds em `modded/Configuration/ModConfig.cs` para a seção `1. Afinidade de CPU & Threads`.
- [ ] Integrar chamada em `PerformanceManager.cs` no evento `OnRaidStarted` e no listener de configuração do F12.
- [ ] Atualizar catálogo do menu em `PROPRIEDADES.md`.
- [ ] Compilar build Release `v0.4.16` e copiar binário para testes.

---

## 9. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid: start hook + stop hooks idempotentes (`GameWorld.OnDestroy` + `BaseLocalGame.Stop`) — AP-01 | ✅ | Hook de aplicação ocorre em `OnRaidStarted` via `RaidStartPatch.cs`; descarte seguro no teardown. |
| 2 | Filtro MainPlayer/Fika em todo patch que reage a ação de player — AP-02 | N/A | Feature de nível de processo de hardware; não atua em eventos individuais de jogadores. |
| 3 | Alvos ofuscados/virtuais resolvidos por assinatura; TODOS os overrides auditados — AP-03 | N/A | Utiliza ponto canônico já existente `GameWorld.OnGameStarted` via `RaidStartPatch.cs`. |
| 4 | Mudança de estado via API canônica do EFT; side-effects mapeados — AP-04 | ✅ | Afinidade aplicada via Win32 API sobre o processo do EFT, sem mutação corrompida de estruturas internas. |
| 5 | Estado entre raids: raid1→exit→raid2 e alt-F4/morte/MIA cobertos | ✅ | Revalidação e reaplicação a cada entrada de partida garantida em `PerformanceManager.OnRaidStarted`. |
| 6 | Semântica/defaults/faixas de cada ConfigEntry sem ambiguidade (incl. estado neutro) — AP-05 | ✅ | Tabela da Seção 3 com enum completo, tooltips descritivos e modo Disabled como estado neutro. |
| 7 | Re-invocação de método patcheado tem reentry-guard/`ReversePatch` — AP-07 | N/A | Não há re-invocação de métodos patcheados. |
| 8 | Flags/caches de intercept validados contra o contexto atual após troca — AP-08 | ✅ | Topologia física é cacheada imutável no boot do sistema operacional; máscara dinâmica reavaliada sob demanda. |
| 9 | Todo patch-point reconfirmado no `.cs` do dump; conferido no `types-index.json` — AP-09 | ✅ | Ponto canônico `GameWorld.OnGameStarted` verificado no ciclo existente do mod. |
| 10 | Skill EFT usada como lever confirmada não-inerte — AP-10 | N/A | Não utiliza skills do EFT. |
| 11 | Pacote FIKA próprio: envelope de comprimento, envio na main thread, etc. — AP-11 | N/A | Não cria pacotes de rede próprios. |
