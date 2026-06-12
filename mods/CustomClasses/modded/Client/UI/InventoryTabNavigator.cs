using System;
using System.Collections;
using System.Reflection;
using EFT.UI;       // InventoryScreen, EInventoryTab
using HarmonyLib;
using UnityEngine;

namespace CustomClasses.Client;

/// <summary>
///     (013) Seleciona a aba Skills no InventoryScreen recém-aberto. Espera a tela existir e chama
///     Tab.Select(true) via _tabDictionary[EInventoryTab.Skills] (reflection — Tab é tipo obfuscado).
///     ref: Assembly-CSharp.dll → EFT.UI.InventoryScreen { IReadOnlyDictionary&lt;EInventoryTab,Tab&gt; _tabDictionary; };
///          GClass3808.SelectTab → Tab.Select(bool sendCallback).
/// </summary>
internal static class InventoryTabNavigator
{
    private static readonly FieldInfo? TabDictField = AccessTools.Field(typeof(InventoryScreen), "_tabDictionary");

    public static IEnumerator SelectSkillsTabWhenReady()
    {
        InventoryScreen? inv = null;
        for (var i = 0; i < 120 && inv == null; i++)
        {
            inv = UnityEngine.Object.FindObjectOfType<InventoryScreen>();
            if (inv == null)
            {
                yield return null;
            }
        }

        if (inv == null)
        {
            yield break;
        }

        for (var i = 0; i < 5; i++)
        {
            yield return null;   // deixa as abas terminarem de inicializar
        }

        TrySelectSkills(inv);
    }

    private static void TrySelectSkills(InventoryScreen inv)
    {
        try
        {
            var dict = TabDictField?.GetValue(inv);
            var getItem = dict?.GetType().GetMethod("get_Item");
            var tab = getItem?.Invoke(dict, new object[] { EInventoryTab.Skills });
            var select = tab?.GetType().GetMethod("Select", new[] { typeof(bool) });
            select?.Invoke(tab, new object[] { true });
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] select skills tab falhou: {ex.Message}");
        }
    }
}
