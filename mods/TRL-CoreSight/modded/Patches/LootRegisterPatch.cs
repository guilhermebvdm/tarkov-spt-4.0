using System;
using System.Reflection;
using EFT;
using EFT.Interactive;
using HarmonyLib;
using SPT.Reflection.Patching;
using TRLCoreSight.Core;

namespace TRLCoreSight.Patches
{
    /// <summary>
    /// Intercepta o registro de novos itens no GameWorld para cadastrá-los dinamicamente no LootCullingManager.
    /// ref: Assembly-CSharp/GameWorld.cs:1514
    /// </summary>
    public class LootRegisterPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            // ref: Assembly-CSharp/GameWorld.cs:1514 (RegisterLoot<T>(T loot))
            MethodInfo method = AccessTools.Method(typeof(GameWorld), nameof(GameWorld.RegisterLoot));
            if (method != null && method.IsGenericMethodDefinition)
            {
                return method.MakeGenericMethod(typeof(LootItem));
            }
            return method;
        }

        [PatchPostfix]
        private static void Postfix(object loot)
        {
            if (loot is LootItem lootItem && LootCullingManager.Instance != null)
            {
                LootCullingManager.Instance.RegisterLootItem(lootItem);
            }
        }
    }
}
