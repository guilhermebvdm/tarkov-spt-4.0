using System;
using System.Reflection;
using EFT;
using EFT.HealthSystem;
using Fika.Core.Main.ClientClasses;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace TarkovRedLine.PvpMode.Patches
{
    /// <summary>
    /// Captura o tipo do dano que está matando o jogador — a ÚNICA fonte confiável dele.
    ///
    /// Por que não dá para ler de outro lugar: <c>Player.LastDamageInfo</c> só é escrito no caminho
    /// de dano de combate (<c>Player.ApplyDamageInfo</c>, <c>Player.ManageAggressor</c>). Dano de
    /// desgaste — fome, sede, overdose de estimulante — não passa por nenhum dos dois: vai direto
    /// ao controlador de vida (<c>ActiveHealthController.cs:1353</c> e <c>:1186</c>) e chega à morte
    /// por <c>DestroyBodyPart → Kill(damageType)</c> (<c>:3883</c>). O tipo correto existe **só como
    /// argumento de Kill** — que é onde este prefixo o pega (code review 01, C-01).
    ///
    /// <c>Priority.First</c> garante que rodamos antes do prefixo do Fika
    /// (<c>ClientHealthController_Kill_Patch.cs:17</c>), que consulta
    /// <c>CheckIfDamageShouldInstantKill()</c> — nosso postfix precisa do valor já gravado.
    /// </summary>
    public class KillGatePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
            // ref: Assembly-CSharp/EFT.HealthSystem/ActiveHealthController.cs:3923
            // Kill não é virtual e não é redeclarado em ClientHealthController: o patch atinge
            // humanos e bots, por isso o filtro IsYourPlayer é a primeira linha (AP-02/AP-03).
            => AccessTools.Method(typeof(ActiveHealthController), nameof(ActiveHealthController.Kill));

        [PatchPrefix]
        [HarmonyPriority(Priority.First)]
        private static void Prefix(ActiveHealthController __instance, EDamageType damageType)
        {
            try
            {
                if (__instance is not ClientHealthController chc) return;
                if (chc.Player == null || !chc.Player.IsYourPlayer) return;

                RaidState.LastKillDamageType = damageType;
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"[TRL-PvpMode] KillGatePatch: {ex}");
            }
        }
    }
}
