using System;
using System.Reflection;
using EFT;
using Fika.Core.Main.ClientClasses;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace TarkovRedLine.PvpMode.Patches
{
    /// <summary>
    /// O destravamento central E o portão de vidas, no mesmo ponto.
    ///
    /// CanBeDowned (ClientHealthController.cs:22) exige
    ///   !_bledOut &amp;&amp; (max==0 || revives&lt;max) &amp;&amp; CanBeRevivedByOtherPlayer()
    ///
    /// Sem resgate por companheiro a última condição perde o sentido, mas continua bloqueando
    /// quem joga sozinho ou é o último vivo. Com vida disponível forçamos true; sem vida
    /// forçamos false, e aí o prefixo do Fika deixa a morte seguir normalmente.
    ///
    /// Por que aqui e não em CanBeDowned: preservar _bledOut. BleedOut() (:125) faz
    /// _bledOut = true; IsAlive = true; Kill() e CONTA com CanBeDowned==false para a morte
    /// passar — mexer nele viraria laço infinito (AP-07).
    /// </summary>
    public class CanBeRevivedByOtherPlayerPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
            // ref: fika-plugin/Fika.Core/Main/ClientClasses/ClientHealthController.cs:54
            => AccessTools.Method(typeof(ClientHealthController), "CanBeRevivedByOtherPlayer");

        [PatchPostfix]
        private static void Postfix(ClientHealthController __instance, ref bool __result)
        {
            try
            {
                // Kill/CanBeDowned rodam para todo mundo no mapa — só o jogador local nos interessa (AP-02).
                if (__instance?.Player == null || !__instance.Player.IsYourPlayer) return;
                if (!Settings.ENABLED.Value) return;

                __result = RaidState.HasLifeAvailable;
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"[TRL-PvpMode] CanBeRevivedByOtherPlayerPatch: {ex}");
            }
        }
    }

    /// <summary>
    /// Duas funções:
    ///
    /// (a) Morte por desgaste SEMPRE mata direto. Sem isto, o destravamento acima faz a morte por
    ///     fome/sede/estimulante ser bloqueada pelo prefixo do Fika e, logo depois, recusada por
    ///     TryProcessDownedState (:159-162) — o jogador ficaria IsAlive=false, sem estado de caído,
    ///     sem evento de morte e sem tela de fim de raid. É o defeito que este item existe para
    ///     corrigir (review 01, R-02).
    ///
    /// (b) A opção "tiro na cabeça mata direto" do nosso F12.
    /// </summary>
    public class InstantKillPatch : ModulePatch
    {
        private const EDamageType ATTRITION =
            EDamageType.Exhaustion | EDamageType.Dehydration | EDamageType.Stimulator;

        protected override MethodBase GetTargetMethod()
            // ref: fika-plugin/Fika.Core/Main/ClientClasses/ClientHealthController.cs:68
            => AccessTools.Method(typeof(ClientHealthController), nameof(ClientHealthController.CheckIfDamageShouldInstantKill));

        [PatchPostfix]
        private static void Postfix(ClientHealthController __instance, ref bool __result)
        {
            try
            {
                if (__instance?.Player == null || !__instance.Player.IsYourPlayer) return;
                if (!Settings.ENABLED.Value) return;
                if (__result) return;   // o Fika já decidiu matar; não desfazemos

                if ((FikaBridge.GetLastDamageType(__instance.Player) & ATTRITION) != 0)
                {
                    __result = true;
                    return;
                }

                // ref: Assembly-CSharp/EFT/Player.cs:24329 — campo público
                if (Settings.HEADSHOT_KILLS.Value && __instance.Player.LastDamagedBodyPart == EBodyPart.Head)
                    __result = true;
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"[TRL-PvpMode] InstantKillPatch: {ex}");
            }
        }
    }
}
