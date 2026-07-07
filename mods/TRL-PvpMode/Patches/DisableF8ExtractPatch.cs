using System.Reflection;
using Fika.Core.Main.Components;
using SPT.Reflection.Patching;
using BepInEx.Logging;

namespace ModoPVP.Patches
{
    public class DisableF8ExtractPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(CoopHandler).GetMethod("ProcessQuitting", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        [PatchPrefix]
        private static bool PatchPrefix(CoopHandler __instance)
        {
            // Se o estado for de morte, nós NÃO DEIXAMOS o Fika processar o Quit / Stop / Extração.
            // O nosso outro patch vai fazer o respawn e o jogo continuará ativo!
            if (__instance.QuitState == CoopHandler.EQuitState.Dead)
            {
                return false; 
            }
            return true;
        }
    }
}
