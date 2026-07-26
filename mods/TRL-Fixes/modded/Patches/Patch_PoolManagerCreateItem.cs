using HarmonyLib;
using EFT;
using EFT.InventoryLogic;
using EFT.CameraControl;
using UnityEngine;
using System.Reflection;
using System;
using Comfort.Common;

namespace TRLFixes.Patches
{
    public class Patch_PoolManagerCreateItem
    {
        public void Enable()
        {
            try
            {
                var harmony = new Harmony("com.trl.fixes.poolmanagercreateitem");
                var targetMethod = AccessTools.Method(
                    typeof(PoolManagerClass),
                    "CreateItem",
                    new Type[] { typeof(Item), typeof(ECameraType), typeof(IPlayer), typeof(bool) }
                );

                if (targetMethod != null)
                {
                    var prefixMethod = AccessTools.Method(typeof(Patch_PoolManagerCreateItem), nameof(Prefix));
                    harmony.Patch(targetMethod, prefix: new HarmonyMethod(prefixMethod));
                    Plugin.Log?.LogInfo("TRL-Fixes: Hook no PoolManagerClass.CreateItem aplicado com sucesso!");
                }
                else
                {
                    Plugin.Log?.LogError("TRL-Fixes: Metodo PoolManagerClass.CreateItem nao encontrado!");
                }
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"TRL-Fixes: Erro ao aplicar patch PoolManagerClass.CreateItem: {ex}");
            }
        }

        private static bool Prefix(ref GameObject __result, Item item, ECameraType cameraType, IPlayer player, bool isAnimated)
        {
            if (player != null && !player.IsYourPlayer)
            {
                try
                {
                    // Para jogadores remotos (Fika/Coop), redireciona para a sobrecarga mais simples
                    // que nao exige referencias de camera do jogador local (evitando NullReferenceException).
                    if (Singleton<PoolManagerClass>.Instantiated)
                    {
                        __result = Singleton<PoolManagerClass>.Instance.CreateItem(item, isAnimated);
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    Plugin.Log?.LogError($"[TRL-Fixes] Erro ao instanciar item de recarga para jogador remoto {player.Profile?.Nickname}: {ex}");
                    __result = null;
                    return false;
                }
            }
            return true;
        }
    }
}
