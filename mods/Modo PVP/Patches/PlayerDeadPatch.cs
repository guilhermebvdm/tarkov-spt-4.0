using System.Reflection;
using EFT;
using Fika.Core.Main.Players;
using SPT.Reflection.Patching;
using BepInEx.Logging;
using Comfort.Common;
using Fika.Core.Main.GameMode;
using ModoPVP.Components;
using EFT.UI;

namespace ModoPVP.Patches
{
    public class PlayerDeadPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            // O alvo é a função de morte do jogador no Fika
            return typeof(FikaPlayer).GetMethod("OnDead", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }

        [PatchPostfix]
        private static void PatchPostfix(FikaPlayer __instance, EDamageType damageType)
        {
            if (__instance.IsYourPlayer)
            {
                // Fix do culling: força terceira pessoa antes do corpo morto ser gerado para restaurar a cabeça e peito
                __instance.PointOfView = EFT.EPointOfView.ThirdPerson;
                
                Logger.LogInfo("PlayerDeadPatch: Morte interceptada no ato! Escondendo Espectador e iniciando Respawn.");
                
                // Pegamos a instância do CoopGame ativa no momento
                CoopGame gameInstance = Singleton<IFikaGame>.Instance as CoopGame;

                // Chamamos a rotina diretamente para ver o que acontece sem a tela preta
                // PreloaderUI preloader = MonoBehaviourSingleton<PreloaderUI>.Instance;
                // preloader.StartBlackScreenShow(1f, 1f, () =>
                // {
                //     RespawnHandler.StartRespawnRoutine(gameInstance);
                // });

                RespawnHandler.StartRespawnRoutine(gameInstance);
            }
        }
    }
}
