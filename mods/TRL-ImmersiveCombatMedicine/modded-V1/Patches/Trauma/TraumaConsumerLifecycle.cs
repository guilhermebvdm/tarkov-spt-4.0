using System;
using Comfort.Common;
using EFT;

namespace TRLImmersiveCombatMedicine.Trauma
{
    /// <summary>Helper de DETECÇÃO compartilhado do esqueleto de Update() dos 4 consumidores de estado contínuo
    /// (003/004/005/006) — extraído no item 009 (A4; débito registrado em 006 code-review-01 CR-01-02 e
    /// 008 code-review-01 CR-01-01, ver memory/sessions.md P-4.1). Cobre SÓ os 4 eventos de lifecycle
    /// (mundo nulo, world-swap/transit, toggle ON→OFF, toggle OFF→ON); a AÇÃO de cada evento continua
    /// 100% no consumidor via callback — NUNCA generalizada aqui (mandato da spec funcional A4).
    /// `struct` mutável deliberada (não readonly) — bookkeeping de 2 campos, sem alocação de heap por
    /// consumidor (csharp-mod-best-practices §5, exceção documentada à regra "prefira readonly struct").</summary>
    internal struct TraumaConsumerLifecycle
    {
        // PA-01-03 (review 1): o campo `_lifecycle` em cada consumidor NUNCA pode ser declarado `readonly` —
        // Tick() muta o struct em-place; `readonly` faria o C# operar sobre uma cópia defensiva silenciosa
        // a cada chamada, quebrando a detecção de mundo/toggle SEM erro de compilação (bug silencioso).
        private GameWorld _trackedWorld;
        private bool _wasActive;

        /// <summary>Chamado 1x por Update() do consumidor, com os MESMOS 5 delegates cacheados em Awake()
        /// (nunca recriados por frame — csharp-mod-best-practices §1). Retorna o `active` corrente: o
        /// consumidor decide rodar (true) ou não (false, já tratado como early-return) sua lógica per-tick.
        /// Qualquer callback pode ser null (no-op) — ex.: TraumaStomachConsumer não tem ação de toggle-on.</summary>
        internal bool Tick(
            Func<bool> isActive,
            Action onWorldGone,
            Action onWorldSwap,
            Action onToggleOff,
            Action onToggleOn)
        {
            // ref: Comfort.Common.Singleton<T> — spt-mod-best-practices §2 (único Singleton correto)
            GameWorld gw = Singleton<GameWorld>.Instance;
            if (gw == null)
            {
                onWorldGone?.Invoke();
                _trackedWorld = null;
                _wasActive = isActive();
                return false;
            }
            if (!ReferenceEquals(gw, _trackedWorld))
            {
                onWorldSwap?.Invoke();
                _trackedWorld = gw;
            }

            bool active = isActive();
            if (_wasActive && !active) onToggleOff?.Invoke();
            else if (!_wasActive && active) onToggleOn?.Invoke();
            _wasActive = active;
            return active;
        }
    }
}
