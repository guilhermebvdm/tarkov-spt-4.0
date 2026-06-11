// Decompiled with JetBrains decompiler
// Type: RealismMod.SetPenetrationStatusPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace RealismMod;

public class SetPenetrationStatusPatch : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (ArmorComponent).GetMethod("SetPenetrationStatus", BindingFlags.Instance | BindingFlags.Public);
  }

  [PatchPrefix]
  private static bool Prefix(EftBulletClass shot, ArmorComponent __instance)
  {
    bool flag1 = __instance.Template.ArmorMaterial == 5 && !BallisticsController.ArmorHasSpecificColliders(__instance, BallisticsController.HeadCollidors);
    if ((double) __instance.Repairable.Durability <= 0.0 && !flag1)
      return false;
    float penetrationPower = shot.PenetrationPower;
    float num1 = (float) ((double) __instance.Repairable.Durability / (double) __instance.Repairable.TemplateDurability * 100.0);
    float num2 = !(__instance.Template.ArmorMaterial == 5 & flag1) ? (__instance.Template.ArmorMaterial != 3 && __instance.Template.ArmorMaterial != 4 && (flag1 || __instance.Template.ArmorMaterial != 5) ? Mathf.Min(100f, num1 * 1.1f) : Mathf.Min(100f, num1 * 1.5f)) : 100f;
    float resistance = (float) Singleton<BackendConfigSettingsClass>.Instance.Armor.GetArmorClass(__instance.ArmorClass).Resistance;
    float num3 = (float) ((121.0 - 5000.0 / (45.0 + (double) num2 * 2.0)) * (double) resistance * 0.0099999997764825821);
    bool flag2 = ((double) num3 >= (double) penetrationPower + 15.0 ? 0.0 : ((double) num3 >= (double) penetrationPower ? 0.40000000596046448 * ((double) num3 - (double) penetrationPower - 15.0) * ((double) num3 - (double) penetrationPower - 15.0) : 100.0 + (double) penetrationPower / (0.89999997615814209 * (double) num3 - (double) penetrationPower))) - (double) shot.Randoms.GetRandomFloat(shot.RandomSeed) * 100.0 < 0.0;
    bool flag3 = (double) num2 >= 90.0 && (double) resistance - (double) shot.PenetrationPower >= 5.0;
    if (flag3 | flag2)
    {
      shot.BlockedBy = MongoID.op_Implicit(((GClass3175) __instance).Item.Id);
      if (PluginConfig.EnableBallisticsLogging.Value)
      {
        Debug.Log((object) ">>> Shot blocked by armor piece");
        ModulePatch.Logger.LogWarning((object) "===========PEN STATUS=============== ");
        ModulePatch.Logger.LogWarning((object) "Blocked");
        ModulePatch.Logger.LogWarning((object) ("shouldBeBlocked " + flag3.ToString()));
        ModulePatch.Logger.LogWarning((object) "========================== ");
      }
    }
    else if (PluginConfig.EnableBallisticsLogging.Value)
    {
      ModulePatch.Logger.LogWarning((object) "============PEN STATUS============== ");
      ModulePatch.Logger.LogWarning((object) "Penetrated");
      ModulePatch.Logger.LogWarning((object) ("shouldBeBlocked " + flag3.ToString()));
      ModulePatch.Logger.LogWarning((object) "========================== ");
    }
    return false;
  }
}
