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

                // Modo inativo (desligado no F12, pré-requisito faltando, fora de raid) ⇒ NÃO tocar
                // no resultado. Forçar false aqui deixaria o jogador pior do que sem o mod: morte
                // instantânea e nenhum aliado conseguindo levantar (code review 01, C-02).
                if (!RaidState.IsActive) return;

                __result = RaidState.HasLifeAvailable;
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"[TRL-PvpMode] CanBeRevivedByOtherPlayerPatch: {ex}");
            }
        }
    }

    /// <summary>
    /// Com o modo ativo, esta decisão passa a ser NOSSA por inteiro — não somamos à do Fika.
    ///
    /// Duas funções:
    ///
    /// (a) Morte por desgaste SEMPRE mata direto. Sem isto, o destravamento acima faz a morte por
    ///     fome/sede/estimulante ser bloqueada pelo prefixo do Fika e, logo depois, recusada por
    ///     TryProcessDownedState (:159-162) — o jogador ficaria IsAlive=false, sem estado de caído,
    ///     sem evento de morte e sem tela de fim de raid. É o defeito que este item existe para
    ///     corrigir (review 01, R-02).
    ///
    /// (b) A opção "tiro na cabeça mata direto" do nosso F12. Assumir a decisão inteira é o que
    ///     permite DESLIGAR o comportamento num servidor que tenha headshotKills ligado — um
    ///     postfix aditivo só conseguiria ligar, nunca desligar (code review 01, C-03).
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
                if (!RaidState.IsActive) return;

                // Fome / sede / estimulante são desgaste, não combate: matam direto.
                // O tipo vem do prefixo de Kill, gravado neste mesmo quadro — Player.LastDamageInfo
                // NÃO serve, pois só é escrito no caminho de dano de combate (C-01).
                var attrition = (RaidState.LastKillDamageType & ATTRITION) != 0;

                // ref: Assembly-CSharp/EFT/Player.cs:24329 — campo público
                var headshot = Settings.HEADSHOT_KILLS.Value
                            && __instance.Player.LastDamagedBodyPart == EBodyPart.Head;

                __result = attrition || headshot;
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"[TRL-PvpMode] InstantKillPatch: {ex}");
            }
        }
    }
}
