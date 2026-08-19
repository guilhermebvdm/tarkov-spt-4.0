using System.Reflection;
using System.Runtime.CompilerServices;
using EFT;
using EFT.Animations;
using HarmonyLib;
using UnityEngine;

#nullable disable
namespace ActionPOV.Core
{
    public static class EFTBindings
    {
        public static readonly FieldInfo FC_PlayerField = 
            AccessTools.Field(typeof(Player.FirearmController), "_player");
            
        public static readonly FieldInfo PWA_FCField = 
            AccessTools.Field(typeof(ProceduralWeaponAnimation), "_firearmController");
            
        public static readonly FieldInfo PWA_HeadRotVecField = 
            AccessTools.Field(typeof(ProceduralWeaponAnimation), "_headRotationVec");

        private static readonly ConditionalWeakTable<ProceduralWeaponAnimation, Player> _playerCache = 
            new ConditionalWeakTable<ProceduralWeaponAnimation, Player>();

        public static Player GetPlayer(ProceduralWeaponAnimation pwa)
        {
            if (pwa == null) return null;
            if (!_playerCache.TryGetValue(pwa, out var player))
            {
                var fc = PWA_FCField?.GetValue(pwa) as Player.FirearmController;
                if (fc != null)
                {
                    player = FC_PlayerField?.GetValue(fc) as Player;
                    if (player != null)
                    {
                        _playerCache.Add(pwa, player);
                    }
                }
            }
            return player;
        }

        public static void SetHeadRotationVec(ProceduralWeaponAnimation pwa, Vector3 rot)
        {
            if (pwa != null && PWA_HeadRotVecField != null)
            {
                PWA_HeadRotVecField.SetValue(pwa, rot);
            }
        }
    }
}
