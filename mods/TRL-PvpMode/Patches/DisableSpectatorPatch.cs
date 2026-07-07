using System.Reflection;
using Fika.Core.Main.FreeCamera;
using SPT.Reflection.Patching;
using EFT;

namespace ModoPVP.Patches
{
    public class DisableSpectatorPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            // O alvo é o evento de morte capturado pela câmera livre do Fika
            return typeof(FreeCameraController).GetMethod("MainPlayer_DiedEvent", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        [PatchPrefix]
        private static bool PatchPrefix(EDamageType obj)
        {
            // Bloqueamos a execução da rotina de câmera de espectador (e da mensagem de F8)
            return false;
        }
    }
}
