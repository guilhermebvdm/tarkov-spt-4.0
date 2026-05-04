// RemzDNB - 2026

using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using EFT.InventoryLogic;
using SPT.Reflection.Patching;

namespace RZEssentialsClient;

public class TraderGridSortingPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(GClass3381), nameof(GClass3381.Sort));
    }

    [PatchPostfix]
    public static void Postfix(ref List<Item> __result)
    {
        var cfg = ClientConfig.Instance;
        
        __result.Sort((a, b) =>
        {
            // Category
            if (cfg.EnableCategorySort)
            {
                var catA = GetIndex(cfg.CategoryOrder, a.Template?.ParentId?.ToString());
                var catB = GetIndex(cfg.CategoryOrder, b.Template?.ParentId?.ToString());
                var cat = catA.CompareTo(catB);
                if (cat != 0) return cat;
            }

            // Item
            if (cfg.EnableItemSort)
            {
                var prioA = GetIndex(cfg.ItemOrder, a.TemplateId.ToString());
                var prioB = GetIndex(cfg.ItemOrder, b.TemplateId.ToString());
                var prio = prioA.CompareTo(prioB);
                if (prio != 0) return prio;
            }

            // Alphabetical
            if (!cfg.EnableAlphabeticalSort) 
                return 0;
            
            return string.Compare(a.LocalizedName(), b.LocalizedName(), StringComparison.OrdinalIgnoreCase);
        });
    }

    private static int GetIndex(List<string> list, string value)
    {
        if (value == null) return int.MaxValue;
        var idx = list.IndexOf(value);
        return idx >= 0 ? idx : int.MaxValue;
    }
}