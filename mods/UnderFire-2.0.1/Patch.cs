using BepInEx.Logging;
using EFT;
using EFT.Ballistics;
using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UnderFire.Patch
{
    internal class UnderFirePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player), nameof(Player.ReceiveDamage));
        }

        [PatchPostfix]
        static void Postfix(ref Player __instance, float damage, EBodyPart part, EDamageType type, float absorbed, MaterialType special)
        {
            if (!__instance.IsYourPlayer) return;
            if (!Plugin.HitEnabled.Value) return;
            if (Plugin.isAdrenalineActive) return;
            if (damage > 0)
            {
                if (type == EDamageType.Bullet || type == EDamageType.GrenadeFragment)
                {
                    //Plugin.logSource.LogWarning($"Starting Adrenaline from OnHit Patch");
                    Plugin.StartAdrenaline();
                }
            }
            return;
        }
    }
}
