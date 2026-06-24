using EFT;
using RealismMod;
using UnityEngine;

namespace TarkovIRL
{
    internal class RealismWrapper
    {
        static RealismHealthController _realHealth = null;
        static float _blurEffectStrength = 0;
        static float _chromaEffectStrength = 0;
        public static float GetRealismReloadSpeed()
        {
            float reloadSpeed = Mathf.Clamp(EfficiencyController.EfficiencyModifierInverse * WeaponStats.CurrentMagReloadSpeed * PlayerState.ReloadSkillMulti * PlayerState.GearErgoPenalty * StanceController.ActiveAimManipBuff, 0.65f, 1.35f);
            return reloadSpeed;
        }

        public static float GetRealismCheckMagSpeed()
        {
            float value = PluginConfig.GlobalCheckAmmoMulti.Value;
            if (WeaponStats._WeapClass == "pistol")
            {
                value = PluginConfig.GlobalCheckAmmoPistolSpeedMulti.Value;
            }
            float animationSpeed = Mathf.Clamp(EfficiencyController.EfficiencyModifierInverse * WeaponStats.CurrentMagReloadSpeed * PlayerState.ReloadSkillMulti * value, 0.7f, 1.35f);
            return animationSpeed;
        }

        public static bool IsShoulderContact()
        {
            return WeaponStats.HasShoulderContact;
        }

        public static bool IsAdrenaline
        { 
            get
            {
                if (_realHealth == null)
                {
                    _realHealth = Plugin.RealHealthController;
                }
                if (_realHealth != null)
                {
                    bool isAdrenal = _realHealth.HasNegativeAdrenalineEffect || _realHealth.HasPositiveAdrenalineEffect;
                    return isAdrenal;
                }
                else
                {
                    // do some error here
                    return false;
                }
            } 
        }

        public static float WeaponBalanceMulti
        {
            get
            {
                float weaponBalanceMulti = 1f + (Mathf.Pow(WeaponStats.Balance, 2f) * 0.001f);
                return weaponBalanceMulti;
            }
        }

        public static bool IsTacSprint 
        {
            get { return StanceController.IsDoingTacSprint; }
        }

        public static bool IsOverdose
        {
            get { return false; }
        }
    }
}