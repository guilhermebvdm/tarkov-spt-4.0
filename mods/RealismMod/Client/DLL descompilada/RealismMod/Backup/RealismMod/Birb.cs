// Decompiled with JetBrains decompiler
// Type: RealismMod.Birb
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace RealismMod;

public class Birb : MonoBehaviour
{
  private bool _wasDestroyed = false;
  private Dictionary<string, int> _birbLoot = new Dictionary<string, int>()
  {
    {
      "5751487e245977207e26a315",
      100
    },
    {
      "57347d3d245977448f7b7f61",
      100
    },
    {
      "5448ff904bdc2d6f028b456e",
      100
    },
    {
      "5c06779c86f77426e00dd782",
      100
    },
    {
      "573474f924597738002c6174",
      100
    },
    {
      "5c1267ee86f77416ec610f72",
      20
    },
    {
      "62a09cfe4f842e1bd12da3e4",
      30
    },
    {
      "5734758f24597738025ee253",
      50
    },
    {
      "59faff1d86f7746c51718c9c",
      10
    },
    {
      "59faf7ca86f7740dbe19f6c2",
      40
    },
    {
      "5780cf7f2459777de4559322",
      30
    },
    {
      "5d80c60f86f77440373c4ece",
      30
    },
    {
      "5ede7a8229445733cb4c18e2",
      30
    },
    {
      "5d80c62a86f7744036212b3f",
      30
    },
    {
      "62987dfc402c7f69bf010923",
      30
    },
    {
      "63a3a93f8a56922e82001f5d",
      30
    },
    {
      "64ccc25f95763a1ae376e447",
      30
    },
    {
      "64d4b23dc1b37504b41ac2b6",
      30
    },
    {
      "5c94bbff86f7747ee735c08f",
      15
    },
    {
      "5c1d0d6d86f7744bb2683e1f",
      5
    },
    {
      "5c1e495a86f7743109743dfb",
      5
    },
    {
      "5c1d0c5f86f7744bb2683cf0",
      5
    }
  };

  private void Update()
  {
    if (this._wasDestroyed || !Plugin.ModInfo.DoGasEvent && (!Plugin.ModInfo.IsPreExplosion || !GameWorldController.IsRightDateForExp) && !GameWorldController.DoMapRads && !Plugin.ModInfo.HasExploded && !GameWorldController.DidExplosionClientSide || Object.op_Equality((Object) ((Component) this).gameObject, (Object) null))
      return;
    Object.Destroy((Object) ((Component) this).gameObject, 20f);
    this._wasDestroyed = true;
  }

  private IEnumerator HandleHitAsync()
  {
    if (Utils.SystemRandom.Next(10) <= 5)
      yield return (object) Utils.LoadLoot(((Component) this).transform.position, ((Component) this).transform.rotation, Utils.GetRandomWeightedKey(this._birbLoot)).AsCoroutine();
    if (Utils.SystemRandom.Next(10) <= 3)
      yield return (object) Utils.LoadLoot(((Component) this).transform.position, ((Component) this).transform.rotation, Utils.GetRandomWeightedKey(this._birbLoot)).AsCoroutine();
    if (Utils.SystemRandom.Next(10) <= 2)
      yield return (object) Utils.LoadLoot(((Component) this).transform.position, ((Component) this).transform.rotation, Utils.GetRandomWeightedKey(this._birbLoot)).AsCoroutine();
    Object.Destroy((Object) ((Component) this).gameObject);
  }

  public void OnHit(DamageInfoStruct di) => this.StartCoroutine(this.HandleHitAsync());
}
