using System;
using System.Collections.Generic;


namespace CameraRotationMod
{
    /// <summary>
    /// Item 020 (F4) — log de erro com limite de repetição, para uso em caminho quente (por frame).
    ///
    /// O problema que resolve: sem limite, uma exceção que ocorre todo frame enche o console do BepInEx com
    /// um rastreamento de pilha por frame — o jogo engasga e o erro REAL fica soterrado. Sem log nenhum, o
    /// erro some. A saída é registrar a **primeira ocorrência de cada TIPO** com o rastreamento completo e,
    /// depois, no máximo uma linha a cada intervalo, dizendo quantas foram suprimidas.
    ///
    /// A chave é o **tipo** da exceção, não um booleano global: uma falha diferente que apareça raids depois
    /// ainda ganha o seu rastreamento — caso contrário o limite esconderia justamente o problema novo.
    ///
    /// Mecanismo extraído de `Networking.FikaSyncManager` (v2.11.0), onde nasceu para erros de rede; agora
    /// serve também o laço principal. O `FikaSyncManager.LogErrorThrottled` delega para cá, então rede e
    /// laço compartilham o mesmo balde de supressão — proposital: o que interessa é o console não afogar.
    /// </summary>
    internal static class ThrottledLog
    {
        private const int ErrorLogIntervalMs = 5000;

        private static int _lastErrorLogTick;
        private static int _suppressedErrors;
        private static readonly HashSet<Type> _tracedExceptionTypes = new HashSet<Type>();

        /// <summary>
        /// Primeira ocorrência de cada tipo sai completa; as seguintes, no máximo uma a cada 5 s, com a
        /// contagem do que foi suprimido. Nunca engole em silêncio.
        /// </summary>
        internal static void Error(string context, Exception ex)
        {
            // Sem `Initialize`/campo próprio de propósito: `Plugin.Logger` é atribuído no início do Awake,
            // antes de qualquer caminho que possa falhar. Um segundo campo aqui seria estado duplicado — e
            // um `Initialize` que ninguém chamasse seria exatamente o código morto que este item remove.
            var log = Plugin.Logger;
            var now = Environment.TickCount;

            if (ex != null && _tracedExceptionTypes.Add(ex.GetType()))
            {
                _lastErrorLogTick = now;
                log?.LogError($"[TRL-StancesAndMobility] {context}: {ex}");
                return;
            }

            // Subtração de ints trata o wrap-around de TickCount corretamente.
            if (now - _lastErrorLogTick < ErrorLogIntervalMs)
            {
                _suppressedErrors++;
                return;
            }

            _lastErrorLogTick = now;
            var suppressed = _suppressedErrors;
            _suppressedErrors = 0;
            log?.LogError($"[TRL-StancesAndMobility] {context}: {ex?.Message}"
                + (suppressed > 0 ? $" (+{suppressed} falhas suprimidas nos últimos {ErrorLogIntervalMs / 1000}s)" : string.Empty));
        }

        /// <summary>Reset de raid — o balde de tipos já vistos não deve atravessar partidas.</summary>
        internal static void Reset()
        {
            _tracedExceptionTypes.Clear();
            _suppressedErrors = 0;
            _lastErrorLogTick = 0;
        }
    }
}
