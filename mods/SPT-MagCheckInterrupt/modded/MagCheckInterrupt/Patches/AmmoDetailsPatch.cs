using System.Reflection;
using Comfort.Common;
using EFT;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace MagCheckInterrupt.Patches;

/// <summary>
/// This patch is for delaying the display of the ammo count panel (bottom right).
/// </summary>
public class AmmoDetailsPatch : ModulePatch
{
    private static readonly AccessTools.FieldRef<EftBattleUIScreen, AmmoCountPanel> _ammoCountPanelField =
        AccessTools.FieldRefAccess<EftBattleUIScreen, AmmoCountPanel>("_ammoCountPanel");

    private static AmmoDetails _lastAmmoDetail;

    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(GamePlayerOwner), nameof(GamePlayerOwner.method_8));
    }

    [PatchPrefix]
    public static bool Prefix(
        GamePlayerOwner __instance,
        int ammoCount,
        int maxAmmoCount,
        int mastering,
        string details,
        bool foldingMechanimWeapon
    )
    {
        _lastAmmoDetail = new AmmoDetails(ammoCount, maxAmmoCount, mastering, details, foldingMechanimWeapon);
        return false;
    }

    public static void ShowLastAmmoDetail()
    {
        Singleton<CommonUI>.Instance.EftBattleUIScreen.ShowAmmoDetails(
            _lastAmmoDetail.AmmoCount,
            _lastAmmoDetail.MaxAmmoCount,
            _lastAmmoDetail.Mastering,
            _lastAmmoDetail.Details,
            _lastAmmoDetail.FoldingMechanimWeapon
        );
    }

    public static void HideAmmoCount()
    {
        var ammoCountPanel = _ammoCountPanelField(Singleton<CommonUI>.Instance.EftBattleUIScreen);
        ammoCountPanel.Hide();
    }

    private readonly struct AmmoDetails(int ammoCount, int maxAmmoCount, int mastering, string details, bool foldingMechanimWeapon)
    {
        public readonly int AmmoCount = ammoCount;
        public readonly int MaxAmmoCount = maxAmmoCount;
        public readonly int Mastering = mastering;
        public readonly string Details = details;
        public readonly bool FoldingMechanimWeapon = foldingMechanimWeapon;
    }
}
