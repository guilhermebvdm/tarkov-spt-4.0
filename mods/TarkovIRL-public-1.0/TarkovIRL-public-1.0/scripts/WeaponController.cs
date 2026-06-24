using EFT;
using RealismMod;
using UnityEngine;

namespace TarkovIRL
{
    public static class WeaponController
    {
        // public vars

        public static float CurrentWeaponErgoNorm = 0;
        public static float CurrentWeaponWeight = 0;

        public static bool IsStocked = false;
        public static bool IsStockFolded = false;
        public static bool IsPistol = false;
        public static bool SwayThisFrame = false;
        public static bool IsFoldable = false;
        public static bool IsUsingMounted = false;
        
        static int _currentWeaponHash = 0;
        static readonly int _MP5KHash = 25347301;

        public static void UpdateWpnStats(Player.FirearmController fc)
        {
            if (fc != null)
            {
                CurrentWeaponWeight = PrimeMover.Instance.WeightAttenuationCurve.Evaluate(fc.Weapon.TotalWeight);
                CurrentWeaponErgoNorm = PrimeMover.Instance.ErgoAttenuationCurve.Evaluate(fc.TotalErgonomics / 100f);
                IsStocked = CheckForStock(fc.Weapon);
            }
            else
            {
                CurrentWeaponWeight = 0;
                CurrentWeaponErgoNorm = 1f;
            }
        }

        public static void ToggleFolded()
        {
            if (!IsFoldable)
            {
                return;
            }
            if (AnimStateController.WeaponState != AnimStateController.EWeaponState.IDLE)
            {
                return;
            }

            IsStockFolded = !IsStockFolded;
        }

        static bool CheckForStock(EFT.InventoryLogic.Weapon weapon)
        {
            bool isPistol = weapon.WeapClass == "pistol";
            IsPistol = isPistol;
            if (isPistol)
            {
                return false;
            }

            else if (_currentWeaponHash == _MP5KHash)
            {
                return false;
            }

            if (weapon.GetFoldable() != null)
            {
                IsFoldable = true;
                IsStockFolded = weapon.Folded;
            }

            return true;
        }

        public static float GetWeaponMulti(bool getInverse)
        {
            float weaponMulti = CurrentWeaponWeight * (1f - CurrentWeaponErgoNorm);
            if (getInverse) return 1f / weaponMulti;
            return weaponMulti;
        }

        public static void SetCurrentWeaponHash(int weaponHash)
        {
            _currentWeaponHash = weaponHash;
            //UtilsTIRL.Log($"weapon hash set {_currentWeaponHash}");
        }

        public static int WeaponHash
        {
            get { return _currentWeaponHash; }
        }

        public static bool HasCheekWeld()
        {
            bool result;
            //result = IsStocked;
            //result &= !IsStockFolded;
            result = RealismWrapper.IsShoulderContact();
            return result;
        }
    }
}
