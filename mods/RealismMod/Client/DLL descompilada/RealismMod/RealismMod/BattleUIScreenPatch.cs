// Decompiled with JetBrains decompiler
// Type: RealismMod.BattleUIScreenPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT.UI;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace RealismMod;

public class BattleUIScreenPatch : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) ((IEnumerable<MethodInfo>) typeof (EftBattleUIScreen).GetMethods(BindingFlags.Instance | BindingFlags.Public)).First<MethodInfo>((Func<MethodInfo, bool>) (x => x.Name == "Show" && x.GetParameters()[0].Name == "owner"));
  }

  [PatchPostfix]
  private static void PatchPostFix(EftBattleUIScreen __instance)
  {
    MountingUI component = Plugin.MountingUIGameObject.GetComponent<MountingUI>();
    if (!Object.op_Inequality((Object) component, (Object) null) || Object.op_Equality((Object) component.ActiveUIScreen, (Object) ((Component) __instance).gameObject))
      return;
    component.ActiveUIScreen = ((Component) __instance).gameObject;
    component.DestroyGameObject();
    component.CreateGameObject(((Component) __instance).transform);
  }
}
