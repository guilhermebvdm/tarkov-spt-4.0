using EFT;
using SPT.Reflection.Patching;
using System.Reflection;

namespace TarkovIRL
{
    public class Patch_PlayStepSound : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(Player).GetMethod("PlayStepSound", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void PatchPostfix(Player __instance)
        {
            if ((UnityEngine.Object)(object)__instance != (UnityEngine.Object)null && __instance.IsYourPlayer)
            {
                FootstepController.NewStep(__instance);
            }
        }
    }
}