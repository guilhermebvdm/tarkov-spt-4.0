using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Reflection;

namespace tarkin.ladders.bep
{
    internal class Patch_GameWorld_Dispose : ModulePatch
    {
        public static event Action OnPostfix;

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GameWorld), nameof(GameWorld.Dispose));
        }

        [PatchPostfix]
        private static void PatchPostfix(GameWorld __instance)
        {
            OnPostfix?.Invoke();
        }
    }
}
