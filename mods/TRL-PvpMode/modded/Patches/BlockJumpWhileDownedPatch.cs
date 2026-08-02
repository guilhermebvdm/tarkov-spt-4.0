using System;
using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace TarkovRedLine.PvpMode.Patches
{
    /// <summary>
    /// Ignora o comando de pular enquanto o jogador está caído.
    ///
    /// Existe para que a tecla de renascer possa ser o <b>espaço</b> sem efeito colateral. O Fika
    /// trava movimento e eixos ao derrubar o jogador, mas o pulo não passa por nenhum dos dois:
    /// <c>Player.Jump()</c> (EFT/Player.cs:25959) encaminha direto para o estado de movimento, e
    /// no Tarkov pular deitado é o comando de <b>levantar</b> — ou seja, segurar espaço no chão
    /// poderia tentar tirar o boneco da pose de caído.
    ///
    /// Em vez de apostar que não acontece, bloqueamos a origem. Efeito colateral nenhum: quem está
    /// caído não deveria pular mesmo.
    /// </summary>
    public class BlockJumpWhileDownedPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
            // ref: Assembly-CSharp/EFT/Player.cs:25959 — public void Jump()
            => AccessTools.Method(typeof(Player), nameof(Player.Jump));

        [PatchPrefix]
        private static bool Prefix(Player __instance)
        {
            try
            {
                // Roda para todo jogador e bot: filtrar primeiro (AP-02).
                if (!__instance.IsYourPlayer) return true;
                if (!RaidState.IsActive) return true;

                return __instance.ActiveHealthController is not
                    Fika.Core.Main.ClientClasses.ClientHealthController { Downed: true };
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"[TRL-PvpMode] BlockJumpWhileDownedPatch: {ex}");
                return true;
            }
        }
    }
}
