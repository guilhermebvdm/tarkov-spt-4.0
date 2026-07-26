using System.Collections.Generic;
using EFT;
using UnityEngine;

namespace TRLImmersiveCombatMedicine.Trauma
{
    /// <summary>Voz de dor TIPADA do ciclo de queda (spec 004 §5 — substitui o caminho reflection do VoiceHelper
    /// legado, rec. P5). Peers ouvem o MESMO clipe via PhrasePacket do Fika (fika FikaPlayer.cs:1093-1103; sem
    /// filtro LocalPhrases). Anti-spam próprio ≥2s por (player, tipo). Reusável pelo 005 (P9).</summary>
    internal static class TraumaVoice
    {
        private static readonly Dictionary<(string, bool), float> _nextAllowed = new Dictionary<(string, bool), float>();

        /// <summary>ref: item 020 — janelas de anti-spam de voz sobreviventes.</summary>
        internal static int ResidualCount => _nextAllowed.Count;
        private const float SpamCooldown = 2f;

        /// <summary>FORTE (queda executada + tentativa negada): OnAgony com importance explícita — fura o Busy do
        /// Speaker em tiroteio (demand só fura OnDemandOnly+roll — correção P5).
        /// ref: PhraseSpeakerClass.cs:175/206-227; EPhraseTrigger.cs:6.
        /// DECISÃO A3 (009, 2026-07-20): compete pelo MESMO Speaker/importance:100 com TryPlayStrong (005/lockout
        /// de re-ADS). Investigado e ACEITO sem arbitragem — precondição estreita (pernas em ciclo de queda E braços
        /// em lockout no MESMO player, sobrepostos no tempo), sem sintoma documentado (memory/sessions.md P-3.6/P-4.1),
        /// e o lado 005 já tolera a perda via retry 0,3s + log voice=skipped (ArmsConsumer.TryBlockReAds). O motor
        /// vanilla (PhraseSpeakerClass.Play, Busy && importance<=Int_0 → :207-211) já arbitra "primeiro chega, leva" —
        /// nenhuma camada adicional foi criada por cima dessa garantia (spec funcional 009 corner A3).</summary>
        internal static void PlayStrong(Player p)
        {
            if (!Allowed(p, strong: true)) return;
            p.Speaker?.Play(EPhraseTrigger.OnAgony, ETagStatus.Combat | ETagStatus.Dying, demand: true, importance: 100);
        }

        /// <summary>FORTE accept-gated (item 005 — lockout de re-ADS): mesmo trigger/tags do PlayStrong, SEM o
        /// anti-spam interno (o throttle por janela + piso 0,3s vivem no TraumaArmsConsumer — canal separado por
        /// consumidor, comportamento do 004 preservado). Retorno true = o Speaker ACEITOU (TagBank != null) —
        /// contrato da PA-01-02 (janela só consumida se tocou). ref: PhraseSpeakerClass.cs:206-227.</summary>
        internal static bool TryPlayStrong(Player p)
        {
            if (p is null) return false;
            return p.Speaker?.Play(EPhraseTrigger.OnAgony, ETagStatus.Combat | ETagStatus.Dying, demand: true, importance: 100) != null;
        }

        /// <summary>LEVE (liberação): OnBeingHurt demand:true — humano local tem OnDemandOnly=true no inicializador
        /// do new PhraseSpeakerClass (Player.cs:28670; Init é chamada separada :28672 — PA-01-13).
        /// ref: Player.cs:28799-28829; EPhraseTrigger.cs:12.</summary>
        internal static void PlayLight(Player p)
        {
            if (!Allowed(p, strong: false)) return;
            p.Say(EPhraseTrigger.OnBeingHurt, demand: true);
        }

        private static bool Allowed(Player p, bool strong)
        {
            if (p is null || p.ProfileId == null) return false;
            var key = (p.ProfileId, strong);
            if (_nextAllowed.TryGetValue(key, out float next) && Time.time < next) return false;
            _nextAllowed[key] = Time.time + SpamCooldown;
            return true;
        }

        /// <summary>Ponto de limpeza DECLARADO do dict estático (PA-02-08 — skill csharp §2): chamado no sweep de
        /// raid-end/world-swap do consumidor, junto de TraumaBotFall.ClearAll(). Sem ele, entradas de ProfileIds
        /// de raids mortas acumulam a sessão inteira.</summary>
        internal static void Clear() => _nextAllowed.Clear();
    }
}
