// Decompiled with JetBrains decompiler
// Type: RealismMod.ChangePosePatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;

#nullable disable
namespace RealismMod;

public class ChangePosePatch : ModulePatch
{
  private static FieldInfo movementContextField;
  private static FieldInfo playerField;

  protected virtual MethodBase GetTargetMethod()
  {
    ChangePosePatch.movementContextField = AccessTools.Field(typeof (MovementState), "MovementContext");
    ChangePosePatch.playerField = AccessTools.Field(typeof (MovementContext), "_player");
    return (MethodBase) typeof (MovementState).GetMethod("ChangePose", BindingFlags.Instance | BindingFlags.Public);
  }

  [PatchPrefix]
  private static void Prefix(MovementState __instance)
  {
    MovementContext movementContext = (MovementContext) ChangePosePatch.movementContextField.GetValue((object) __instance);
    if (!((Player) ChangePosePatch.playerField.GetValue((object) movementContext)).IsYourPlayer)
      return;
    StanceController.IsMounting = false;
  }
}
