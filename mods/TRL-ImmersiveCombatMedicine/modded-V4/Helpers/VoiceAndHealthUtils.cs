using EFT;
using HarmonyLib;
using TRLImmersiveCombatMedicine;
using TRLImmersiveCombatMedicine.Trauma;

namespace TRLImmersiveCombatMedicine.Helpers
{
    // ref: item 019 — REMOVIDOS deste arquivo (código morto, zero call sites, confirmado por grep):
    //
    //   • VoiceHelper.SafePlayVoice(player, string triggerName)
    //   • VoiceHelper.TriggerTraumaVoice(player, string type)
    //   • HealthUtils.IsPartDestroyed(player, part)
    //
    // Os dois primeiros eram o sistema de voz do TrueTrauma 3.11 e resolviam o gatilho por
    // `Enum.Parse(typeof(EPhraseTrigger), nome)` dentro de um `catch {}` VAZIO. Três dos quatro nomes que ele
    // usava NÃO EXISTEM no enum — `OnLegBroken`, `OnHandBroken` e `OnPain` — então o Parse lançava, o catch
    // engolia, e o resultado era silêncio absoluto sem nada no log: metade das falas de perna e de braço
    // (o sorteio 50/50 com OnAgony) e TODAS as de estômago nunca tocaram uma única vez.
    //
    // A substituição é TraumaVoice (gatilhos TIPADOS, que não compilam se o nome estiver errado) +
    // TraumaPainVoice (mapa evento→fala). Os nomes corretos são `LegBroken` (49) e `HandBroken` (48), e para
    // dor de estômago o jogo não tem gatilho dedicado. Ver EPhraseTrigger.cs:47-48.
    //
    // HealthUtils.IsPartDestroyed saiu junto: o motor de estados (item 002) resolve estado de membro pelos
    // predicados do próprio HealthController, e este helper misturava "zerado" com "quebrado" numa só
    // resposta — a distinção que o item 019 precisa justamente preservar para escolher a fala certa.

    /// <summary>Mordaça de voz durante o desmaio: um jogador inconsciente não fala.</summary>
    [HarmonyPatch(typeof(Player), "Say")]
    class SilenceVoicePatch
    {
        static bool Prefix(Player __instance, EPhraseTrigger phrase)
        {
            if (!TRLImmersiveCombatMedicinePlugin.ConfigMasterEnabled.Value || !TRLImmersiveCombatMedicinePlugin.ConfigBlackoutEnabled.Value) return true;

            // ref: item 019 — a whitelist de OnYourOwn/OnBreath foi REMOVIDA. Ela existia porque o TrueTrauma
            // 3.11 não tinha pacote de rede e usava essas duas falas como CANAL DE SINALIZAÇÃO ("desmaiei" /
            // "acordei"), aproveitando o PhrasePacket nativo do Fika como transporte. O mod novo sincroniza o
            // desmaio por pacote próprio desde o CR-01-02, então a exceção ficou órfã — e vazava justamente
            // uma fala de dor (OnBreath, hoje usada pelo item 019 como ofego) de dentro do desmaio.
            if (__instance != null && TraumaState.BlackoutTimers.ContainsKey(__instance.ProfileId))
                return false;

            return true;
        }
    }
}
