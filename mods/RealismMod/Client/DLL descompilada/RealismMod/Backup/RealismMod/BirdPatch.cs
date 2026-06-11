// Decompiled with JetBrains decompiler
// Type: RealismMod.BirdPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT.Animals;
using EFT.Ballistics;
using SPT.Reflection.Patching;
using System;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace RealismMod;

public class BirdPatch : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (BirdsSpawner).GetMethod("Awake", BindingFlags.Instance | BindingFlags.Public);
  }

  [SPT.Reflection.Patching.PatchPostfix]
  private static void PatchPostfix(BirdsSpawner __instance)
  {
    GameWorldController.RunEarlyGameCheck();
    if (Plugin.FikaPresent)
      return;
    foreach (Bird componentsInChild in ((Component) __instance).gameObject.GetComponentsInChildren<Bird>())
    {
      SphereCollider sphereCollider = ((Component) componentsInChild).gameObject.AddComponent<SphereCollider>();
      sphereCollider.radius = 0.35f;
      BallisticCollider ballisticCollider = ((Component) componentsInChild).gameObject.AddComponent<BallisticCollider>();
      ((Component) ballisticCollider).gameObject.layer = 12;
      ballisticCollider.TypeOfMaterial = (MaterialType) 2;
      Birb birb = ((Component) componentsInChild).gameObject.AddComponent<Birb>();
      ballisticCollider.OnHitAction += new Action<DamageInfoStruct>(birb.OnHit);
      if (PluginConfig.ZoneDebug.Value)
      {
        GameObject primitive = GameObject.CreatePrimitive((PrimitiveType) 0);
        primitive.transform.SetParent(((Component) componentsInChild).transform);
        primitive.transform.localPosition = sphereCollider.center;
        primitive.transform.localScale = Vector3.op_Multiply(Vector3.op_Multiply(Vector3.one, sphereCollider.radius), 2f);
        primitive.GetComponent<Renderer>().material.color = new Color(1f, 0.0f, 0.0f, 1f);
        primitive.GetComponent<Collider>().enabled = false;
      }
    }
  }
}
