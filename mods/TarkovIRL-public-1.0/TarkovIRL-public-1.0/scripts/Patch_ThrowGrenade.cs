using EFT;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TarkovIRL
{
    public class Patch_ThrowGrenade : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(Player).GetMethod("ThrowGrenade", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void PatchPostfix(Player __instance, bool lowThrow)
        {
            if ((UnityEngine.Object)(object)__instance != (UnityEngine.Object)null && __instance.IsYourPlayer)
            {
                ThrowController.NewThrow(lowThrow);
            }
        }
    }
}
