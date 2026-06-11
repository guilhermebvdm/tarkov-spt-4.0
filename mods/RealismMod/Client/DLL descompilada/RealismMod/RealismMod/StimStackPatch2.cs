// Decompiled with JetBrains decompiler
// Type: RealismMod.StimStackPatch2
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT.HealthSystem;
using SPT.Reflection.Patching;
using System.Reflection;

#nullable disable
namespace RealismMod;

public class StimStackPatch2 : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (ActiveHealthController.Class2114<>).MakeGenericType(typeof (ActiveHealthController).GetNestedType("Stimulator", BindingFlags.Instance | BindingFlags.NonPublic)).GetMethod("method_0", BindingFlags.Instance | BindingFlags.Public);
  }

  [PatchPrefix]
  private static bool Prefix(ref bool __result)
  {
    __result = false;
    return false;
  }
}
