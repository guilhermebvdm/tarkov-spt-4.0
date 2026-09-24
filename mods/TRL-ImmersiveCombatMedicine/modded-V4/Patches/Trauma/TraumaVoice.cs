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
        /// <summary>ref: item 019 — canal de anti-spam. Era `bool strong`, o que dava DOIS canais; o mod antigo
        /// era pior ainda (canal ÚNICO por jogador, então grito de perna calava grito de braço por 2s). Com um
        /// canal por tipo, uma perna quebrada e um braço quebrado no mesmo instante produzem as DUAS falas.</summary>
        internal enum Kind
        {
            Strong,    // OnAgony — queda executada, tentativa de levantar negada
            Light,     // OnBeingHurt — liberação do bloqueio
            Fracture,  // LegBroken / HandBroken — membro FRATURADO
            Zeroed,    // OnAgony — membro zerado sem fratura (canal próprio p/ não competir com Strong)
            Effort     // OnBreath — esforço/ofego ao perder a mira
        }

        private static readonly Dictionary<(string, Kind), float> _nextAllowed = new Dictionary<(string, Kind), float>();

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
            if (!Allowed(p, Kind.Strong)) return;
            p.Speaker?.Play(EPhraseTrigger.OnAgony, ETagStatus.Combat | ETagStatus.Dying, demand: true, importance: 100);
        }

        /// <summary>ref: item 019 — FRATURA de membro. `LegBroken`/`HandBroken` são exatamente os gatilhos que o
        /// jogo usa para o mesmo evento: BotMemoryClass.cs:640-654 reage ao efeito de fratura falando LegBroken
        /// para perna e HandBroken para braço. Os dois também estão no menu de voz do jogador, grupo
        /// "HEALTH STATUS" (GesturesMenu.cs:469), o que garante que existe clipe de voz de jogador.
        ///
        /// ATENÇÃO HISTÓRICA: o mod antigo tentava `OnLegBroken`/`OnHandBroken` — nomes que NÃO EXISTEM no enum.
        /// Ele resolvia por `Enum.Parse` de string dentro de um `catch {}` vazio (VoiceAndHealthUtils.cs:25-36),
        /// então metade das falas de perna/braço e TODAS as de estômago (`OnPain`, também inexistente) eram
        /// silêncio absoluto, sem nada no log. Só o OnAgony do sorteio 50/50 tocava. Usar o enum tipado aqui é
        /// o que impede o bug de voltar.</summary>
        internal static void PlayFracture(Player p, TraumaRegion region)
        {
            if (!Allowed(p, Kind.Fracture)) return;
            EPhraseTrigger trigger = region == TraumaRegion.Arms
                ? EPhraseTrigger.HandBroken   // ref: EPhraseTrigger.cs:47 (48)
                : EPhraseTrigger.LegBroken;   // ref: EPhraseTrigger.cs:48 (49)
            // Sem importance: fala de status, não de agonia — não deve furar a fila de combate como o OnAgony.
            p.Say(trigger, demand: true);
        }

        /// <summary>ref: item 019 — membro ZERADO sem fratura. O jogo não tem gatilho dedicado para "membro
        /// destruído" (só para fratura), então reusa OnAgony — mas em canal de anti-spam próprio, para não
        /// consumir a janela do Strong (queda) nem ser consumido por ela.</summary>
        internal static void PlayZeroed(Player p)
        {
            if (!Allowed(p, Kind.Zeroed, consume: false)) return;
            // ref: review 019 CR-01-03 — readback antes de consumir a janela, mesmo contrato do TryPlayStrong
            // (PA-01-02 do item 005): o Speaker devolve null quando engole a fala por Busy/importance, e
            // consumir a janela nesse caso custaria 2s de silêncio por uma fala que nunca saiu.
            if (p.Speaker?.Play(EPhraseTrigger.OnAgony, ETagStatus.Combat | ETagStatus.Dying, demand: true, importance: 100) == null) return;
            Consume(p, Kind.Zeroed);
        }

        /// <summary>ref: item 019 — ESFORÇO/ofego, não agonia: usado quando a mira cai por fraqueza do braço.
        /// É a fala que o mod antigo tocava nesse momento (o único dos quatro nomes dele que existia de fato).
        /// ref: EPhraseTrigger.cs:26 (29); Player.cs:27588 usa o mesmo trigger p/ respiração pesada.</summary>
        internal static void PlayEffort(Player p)
        {
            if (!Allowed(p, Kind.Effort)) return;
            p.Say(EPhraseTrigger.OnBreath, demand: true);
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
            if (!Allowed(p, Kind.Light)) return;
            p.Say(EPhraseTrigger.OnBeingHurt, demand: true);
        }

        private static bool Allowed(Player p, Kind kind, bool consume = true)
        {
            if (p is null || p.ProfileId == null) return false;
            // ref: item 019 — bots são MUDOS por decisão do usuário (2026-07-26): o mod antigo não filtrava IA e
            // os bots gritavam, o que polui o áudio e atrapalha identificar quem está ferido de verdade. Hoje
            // nenhum call site passa bot, então é guard defensivo — e é o ponto único onde a decisão vive.
            if (p.IsAI) return false;

            // ref: review 019 CR-01-02 — mordaça de desmaio AQUI, e não só no SilenceVoicePatch. Aquele patch
            // intercepta `Player.Say`, mas PlayStrong/PlayZeroed usam `Speaker.Play` DIRETO e furavam a mordaça:
            // um desmaiado que levasse um tiro capaz de zerar um membro gritaria de dentro do apagão. Centralizar
            // no gate cobre os dois caminhos de emissão de uma vez.
            if (TraumaState.BlackoutTimers.ContainsKey(p.ProfileId)) return false;

            var key = (p.ProfileId, kind);
            if (_nextAllowed.TryGetValue(key, out float next) && Time.time < next) return false;
            if (consume) _nextAllowed[key] = Time.time + SpamCooldown;
            return true;
        }

        /// <summary>Consome a janela de anti-spam depois de confirmar que o Speaker aceitou a fala
        /// (par de `Allowed(..., consume: false)`).</summary>
        private static void Consume(Player p, Kind kind)
        {
            if (p is null || p.ProfileId == null) return;
            _nextAllowed[(p.ProfileId, kind)] = Time.time + SpamCooldown;
        }

        /// <summary>Ponto de limpeza DECLARADO do dict estático (PA-02-08 — skill csharp §2): chamado no sweep de
        /// raid-end/world-swap do consumidor, junto de TraumaBotFall.ClearAll(). Sem ele, entradas de ProfileIds
        /// de raids mortas acumulam a sessão inteira.</summary>
        internal static void Clear() => _nextAllowed.Clear();
    }
}
