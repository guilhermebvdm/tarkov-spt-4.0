// Decompiled with JetBrains decompiler
// Type: RealismMod.RigidLootSpawnPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using EFT.InventoryLogic;
using SPT.Reflection.Patching;
using System.Reflection;

#nullable disable
namespace RealismMod;

public class RigidLootSpawnPatch : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (GameWorld).GetMethod("CreateLootWithRigidbody");
  }

  [SPT.Reflection.Patching.PatchPostfix]
  public static void PatchPostfix(Item item) => GameWorldController.RandomizeLootResources(item);
}
