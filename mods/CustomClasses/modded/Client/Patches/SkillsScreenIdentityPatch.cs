using System;
using System.Linq;
using System.Reflection;
using EFT.UI;       // SkillsAndMasteringScreen
using HarmonyLib;
using SPT.Reflection.Patching;
using TMPro;
using UnityEngine;

namespace CustomClasses.Client;

/// <summary>
///     (012) Selo da classe (ícone + nome colorido) no topo da tela de Skills.
///     ref: Assembly-CSharp.dll → EFT.UI.SkillsAndMasteringScreen.Show(Profile, InventoryController, IHealthController).
/// </summary>
internal class SkillsScreenIdentityPatch : ModulePatch
{
    private const string SealName = "CC_ClassSeal_Skills";

    protected override MethodBase GetTargetMethod()
    {
        // 3 params (Profile, InventoryController, IHealthController) — robusto a sobrecargas.
        return AccessTools.GetDeclaredMethods(typeof(SkillsAndMasteringScreen))
            .First(m => m.Name == nameof(SkillsAndMasteringScreen.Show) && m.GetParameters().Length == 3);
    }

    [PatchPostfix]
    private static void Postfix(SkillsAndMasteringScreen __instance)
    {
        if (!Plugin.ShowClassIdentity || __instance == null)
        {
            return;
        }

        try
        {
            SkillMultipliers.EnsureLoaded();
            if (string.IsNullOrEmpty(SkillMultipliers.ClassName))
            {
                return;
            }

            var font = __instance.GetComponentInChildren<TextMeshProUGUI>(true)?.font ?? TMP_Settings.defaultFontAsset;
            var color = ClassIdentityView.ResolveColor(SkillMultipliers.NameColor, Color.white);
            var go = ClassIdentityView.BuildOrRefresh(__instance.transform, SealName, SkillMultipliers.ClassName!,
                SkillMultipliers.IconFile, color, font, iconSize: 40f, fontSize: 24f);

            // topo-centro (centralizado horizontalmente); offset X/Y ajustável no F12.
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(Plugin.SkillsClassPosX?.Value ?? 0f, Plugin.SkillsClassPosY?.Value ?? 0f);
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] skills identity falhou: {ex.Message}");
        }
    }
}
