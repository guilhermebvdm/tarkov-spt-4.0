using EFT;
using HarmonyLib;
using RealismMod.Weapons;
using RealismMod;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TarkovIRL
{
    public class Patch_QuickReloadMag : ModulePatch
    {
        private static FieldInfo playerField;

        protected override MethodBase GetTargetMethod()
        {
            playerField = AccessTools.Field(typeof(Player.FirearmController), "_player");
            return typeof(Player.FirearmController).GetMethod("QuickReloadMag", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void PatchPostfix(Player.FirearmController __instance)
        {
            Player player = (Player)playerField.GetValue(__instance);
            if (player.IsYourPlayer)
            {
                AugmentedReloadController.RefreshAnimator(player.HandsAnimator);
            }
        }
    }
}
