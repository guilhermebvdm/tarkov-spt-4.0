using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using TRLCoreSight.Configuration;

namespace TRLCoreSight.Core
{
    public enum ECpuAffinityMode
    {
        Disabled = 0,
        Auto = 1,
        PhysicalCoresOnly = 2,
        PerformanceCores = 3,
        PrimaryCluster = 4
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

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern UIntPtr SetThreadAffinityMask(IntPtr hThread, UIntPtr dwThreadAffinityMask);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetCurrentThread();

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
        private static bool _needsMainThreadPin = false;
        private static bool _needsMainThreadUnpin = false;
        private static bool _mainThreadPinned = false;

        public static bool NeedsMainThreadPin => _needsMainThreadPin;
        public static bool NeedsMainThreadUnpin => _needsMainThreadUnpin;

        public static void InitializeTopology()
        {
            if (_topologyParsed) return;

            try
            {
                _originalProcessMask = (ulong)Process.GetCurrentProcess().ProcessorAffinity.ToInt64();
                _logicalCoreCount = Environment.ProcessorCount;

                bool success = ParseTopology();
                if (!success)
                {
                    ApplyFallbackTopology();
                }

                _topologyParsed = true;
                Plugin.LogSource?.LogInfo($"[TRL-CoreSight][CPU] Topologia detectada com sucesso: {_physicalCoreCount} núcleos físicos, {_logicalCoreCount} threads lógicas.");
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogWarning($"[TRL-CoreSight][CPU] Erro na inicialização da topologia: {ex.Message}. Aplicando fallback seguro.");
                ApplyFallbackTopology();
                _topologyParsed = true;
            }
        }

        public static void ApplyAffinity()
        {
            if (!_topologyParsed) InitializeTopology();

            try
            {
                // 1. Elevação de Prioridade de Processo
                if (ModConfig.ProcessPriorityHigh != null && ModConfig.ProcessPriorityHigh.Value)
                {
                    try
                    {
                        var process = Process.GetCurrentProcess();
                        if (process.PriorityClass != ProcessPriorityClass.High)
                        {
                            process.PriorityClass = ProcessPriorityClass.High;
                            Plugin.LogSource?.LogInfo("[TRL-CoreSight][CPU] Prioridade do processo ajustada para High.");
                        }
                    }
                    catch { }
                }

                // 2. Estratégia de Afinidade
                ECpuAffinityMode mode = ModConfig.CpuAffinityMode != null ? ModConfig.CpuAffinityMode.Value : ECpuAffinityMode.PhysicalCoresOnly;
                if (mode == ECpuAffinityMode.Disabled)
                {
                    RestoreOriginalAffinity();
                    return;
                }

                // Modo PrimaryCluster: não restringe o processo inteiro para não estrangular worker threads / SAIN.
                // Mantém a afinidade global do processo livre e sinaliza para afixar apenas a Main Thread no Update().
                if (mode == ECpuAffinityMode.PrimaryCluster)
                {
                    if (_originalProcessMask != 0)
                    {
                        SetProcessAffinityMask(GetCurrentProcess(), (UIntPtr)_originalProcessMask);
                    }
                    _needsMainThreadPin = true;
                    // Cancela qualquer "unpin" pendente de uma troca de modo anterior ainda não drenada
                    // pelo Update() — sem isso, uma troca rápida (PrimaryCluster → outro → PrimaryCluster de
                    // novo antes do próximo frame) fixaria a thread e a liberaria de volta um frame depois.
                    _needsMainThreadUnpin = false;
                    Plugin.LogSource?.LogInfo($"[TRL-CoreSight][CPU] Modo PrimaryCluster ativado. Processo mantido livre (0x{_originalProcessMask:X}); Main Thread será afixada no ciclo de atualização da partida.");
                    return;
                }

                // Demais modos (PhysicalCoresOnly e PerformanceCores): usam afinidade de processo
                _needsMainThreadPin = false;
                if (_mainThreadPinned)
                {
                    // Saindo do PrimaryCluster: a Main Thread ainda está presa na máscara estreita
                    // anterior (SetProcessAffinityMask não afeta afinidade de thread já fixada).
                    // Sinaliza para o Update() (thread principal) liberá-la de volta ao total.
                    _mainThreadPinned = false;
                    _needsMainThreadUnpin = true;
                }
                ulong targetMask = CalculateTargetMask(mode);

                // Salvaguarda: Nunca permitir menos de 4 threads ativas em processadores modernos
                int activeThreads = CountBits(targetMask);
                if (activeThreads < 4 && _logicalCoreCount >= 4)
                {
                    Plugin.LogSource?.LogWarning($"[TRL-CoreSight][CPU] Máscara calculada (0x{targetMask:X}) resultou em apenas {activeThreads} threads. Mantendo afinidade original para segurança.");
                    targetMask = _originalProcessMask;
                    activeThreads = CountBits(targetMask);
                }

                if (targetMask != 0)
                {
                    SetProcessAffinityMask(GetCurrentProcess(), (UIntPtr)targetMask);
                    Plugin.LogSource?.LogInfo($"[TRL-CoreSight][CPU] Afinidade de processo aplicada! Modo: {mode}, Máscara: 0x{targetMask:X}, Threads Ativas: {activeThreads}/{_logicalCoreCount}.");
                }
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogError($"[TRL-CoreSight][CPU] Falha ao aplicar máscara de afinidade: {ex.Message}");
            }
        }

        public static void PinCallingThreadToCluster()
        {
            if (!_needsMainThreadPin) return;
            _needsMainThreadPin = false;

            if (_primaryL3Mask == 0 || CountBits(_primaryL3Mask) < 4)
            {
                Plugin.LogSource?.LogWarning($"[TRL-CoreSight][CPU] PrimaryCluster não pôde ser aplicado à Main Thread: máscara inválida (0x{_primaryL3Mask:X}).");
                return;
            }

            try
            {
                IntPtr hThread = GetCurrentThread();
                UIntPtr result = SetThreadAffinityMask(hThread, (UIntPtr)_primaryL3Mask);
                if (result == UIntPtr.Zero)
                {
                    int err = Marshal.GetLastWin32Error();
                    Plugin.LogSource?.LogWarning($"[TRL-CoreSight][CPU] Falha ao afixar Main Thread ao cluster L3 (Win32 Error: {err}).");
                }
                else
                {
                    _mainThreadPinned = true;
                    Plugin.LogSource?.LogInfo($"[TRL-CoreSight][CPU] Main Thread afixada com sucesso ao PrimaryCluster (L3 Cache Mask: 0x{_primaryL3Mask:X}, {CountBits(_primaryL3Mask)} threads). Worker threads do processo permanecem livres.");
                }
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogError($"[TRL-CoreSight][CPU] Exceção ao afixar Main Thread: {ex.Message}");
            }
        }

        public static void UnpinCallingThread()
        {
            if (!_needsMainThreadUnpin) return;
            _needsMainThreadUnpin = false;

            try
            {
                SetThreadAffinityMask(GetCurrentThread(), (UIntPtr)_originalProcessMask);
                Plugin.LogSource?.LogInfo("[TRL-CoreSight][CPU] Main Thread liberada de volta ao conjunto total de núcleos do processo.");
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogError($"[TRL-CoreSight][CPU] Exceção ao liberar afinidade da Main Thread: {ex.Message}");
            }
        }

        public static void RestoreOriginalAffinity()
        {
            try
            {
                _needsMainThreadPin = false;
                _needsMainThreadUnpin = false;
                _mainThreadPinned = false;
                if (_originalProcessMask != 0)
                {
                    SetProcessAffinityMask(GetCurrentProcess(), (UIntPtr)_originalProcessMask);
                    try
                    {
                        SetThreadAffinityMask(GetCurrentThread(), (UIntPtr)_originalProcessMask);
                    }
                    catch { }
                    Plugin.LogSource?.LogInfo("[TRL-CoreSight][CPU] Afinidade restaurada para o padrão do Windows.");
                }
            }
            catch { }
        }

        private static ulong CalculateTargetMask(ECpuAffinityMode mode)
        {
            switch (mode)
            {
                case ECpuAffinityMode.PhysicalCoresOnly:
                    return _physicalCoresMask != 0 ? _physicalCoresMask : _originalProcessMask;

                case ECpuAffinityMode.PerformanceCores:
                    return _pCoresMask != 0 ? _pCoresMask : (_physicalCoresMask != 0 ? _physicalCoresMask : _originalProcessMask);

                // PrimaryCluster não passa por aqui: ApplyAffinity() intercepta esse modo antes
                // e usa afinidade de thread (PinCallingThreadToCluster), não de processo.

                case ECpuAffinityMode.Auto:
                default:
                    // Se for Intel Híbrida com P-Cores identificados
                    if (_pCoresMask != 0 && _pCoresMask != _originalProcessMask && CountBits(_pCoresMask) >= 4)
                        return _pCoresMask;

                    // Caso geral: Se tiver 6 ou mais threads lógicas, prioriza núcleos físicos (corta SMT)
                    if (_physicalCoresMask != 0 && _logicalCoreCount >= 6)
                        return _physicalCoresMask;

                    return _originalProcessMask;
            }
        }

        private static bool ParseTopology()
        {
            uint length = 0;
            GetLogicalProcessorInformationEx(LOGICAL_PROCESSOR_RELATIONSHIP.RelationAll, IntPtr.Zero, ref length);
            if (length == 0) return false;

            IntPtr buffer = Marshal.AllocHGlobal((int)length);
            try
            {
                if (!GetLogicalProcessorInformationEx(LOGICAL_PROCESSOR_RELATIONSHIP.RelationAll, buffer, ref length))
                    return false;

                IntPtr current = buffer;
                int offset = 0;
                _physicalCoreCount = 0;
                _physicalCoresMask = 0;
                _pCoresMask = 0;
                _primaryL3Mask = 0;

                while (offset < length)
                {
                    LOGICAL_PROCESSOR_RELATIONSHIP relation = (LOGICAL_PROCESSOR_RELATIONSHIP)Marshal.ReadInt32(current, 0);
                    int size = Marshal.ReadInt32(current, 4);
                    if (size <= 0) break;

                    if (relation == LOGICAL_PROCESSOR_RELATIONSHIP.RelationProcessorCore)
                    {
                        _physicalCoreCount++;
                        byte efficiency = Marshal.ReadByte(current, 9); // EfficiencyClass em offset 9

                        // GROUP_AFFINITY inicia em offset 32 em arquiteturas x64
                        ulong coreMask = (ulong)Marshal.ReadInt64(current, 32);
                        if (coreMask == 0)
                        {
                            // Fallback se offset em 64-bit divergir
                            coreMask = (ulong)Marshal.ReadInt64(current, 24);
                        }

                        if (coreMask != 0)
                        {
                            // A primeira thread do conjunto representa o núcleo físico
                            ulong primaryLogicalThread = coreMask & (~coreMask + 1);
                            _physicalCoresMask |= primaryLogicalThread;

                            if (efficiency > 0)
                            {
                                _pCoresMask |= coreMask;
                            }
                        }
                    }
                    else if (relation == LOGICAL_PROCESSOR_RELATIONSHIP.RelationCache)
                    {
                        byte level = Marshal.ReadByte(current, 8);
                        if (level == 3 && _primaryL3Mask == 0)
                        {
                            // GROUP_AFFINITY da relação de cache em offset 32 / 24
                            ulong cacheMask = (ulong)Marshal.ReadInt64(current, 32);
                            if (cacheMask == 0)
                                cacheMask = (ulong)Marshal.ReadInt64(current, 24);

                            if (cacheMask != 0)
                            {
                                _primaryL3Mask = cacheMask;
                            }
                        }
                    }

                    offset += size;
                    current = new IntPtr(current.ToInt64() + size);
                }

                return _physicalCoresMask != 0;
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        private static void ApplyFallbackTopology()
        {
            _physicalCoreCount = _logicalCoreCount > 1 ? _logicalCoreCount / 2 : 1;

            // Gera máscara alternada (01010101...) representando núcleos físicos comuns em SMT
            ulong mask = 0;
            for (int i = 0; i < _logicalCoreCount; i += 2)
            {
                mask |= (1UL << i);
            }

            _physicalCoresMask = mask;
            _pCoresMask = 0;
            _primaryL3Mask = 0;
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
