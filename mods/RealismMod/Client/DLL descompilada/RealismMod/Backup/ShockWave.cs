// Decompiled with JetBrains decompiler
// Type: ShockWave
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using EFT.HealthSystem;
using EFT.Interactive;
using RealismMod;
using UnityEngine;

#nullable disable
public class ShockWave : TriggerWithId
{
  public AudioSource shockwaveSound;
  private int _triggeredCount = 0;
  private Vector3 _maxScale = new Vector3(1000f, 1000f, 1000f);

  private void Start()
  {
    ((Component) this).gameObject.AddComponent<RadiationZone>().ZoneStrength = 700f;
    ((Component) this).gameObject.AddComponent<TriggerWithId>().SetId("nuke");
    ((Component) this).gameObject.layer = LayerMask.NameToLayer("Triggers");
    this.shockwaveSound.volume *= GameWorldController.GetGameVolumeAsFactor();
  }

  private void Update()
  {
    ((Component) this).transform.localScale = Vector3.Min(((Component) this).transform.localScale, this._maxScale);
  }

  private bool ApplyDamage(Player player, EBodyPart bodyPart, float damage)
  {
    double num = (double) player.ActiveHealthController.ApplyDamage(bodyPart, damage, GClass2855.RadiationDamage);
    return ((GClass2814<ActiveHealthController.GClass2813>) player.ActiveHealthController).IsAlive;
  }

  private void HandlePlayerEffects(Player player)
  {
    if (PlayerState.EnviroType == 1 || PlayerState.BtrState == 3)
      return;
    this.ApplyDamage(player, (EBodyPart) 0, 15f);
    this.ApplyDamage(player, (EBodyPart) 1, 110f);
    this.ApplyDamage(player, (EBodyPart) 2, 110f);
    this.ApplyDamage(player, (EBodyPart) 3, 60f);
    this.ApplyDamage(player, (EBodyPart) 4, 60f);
    this.ApplyDamage(player, (EBodyPart) 5, 60f);
    this.ApplyDamage(player, (EBodyPart) 6, 60f);
    if (player.IsInPronePose)
      return;
    player.ToggleProne();
  }

  private void HandleBotEffects(Player player)
  {
    if (player.Environment == 1 || !this.ApplyDamage(player, (EBodyPart) 1, 9001f))
      ;
  }

  public virtual void TriggerEnter(Player player)
  {
    if (Object.op_Equality((Object) player, (Object) null))
      return;
    if (!player.IsYourPlayer)
    {
      this.HandleBotEffects(player);
    }
    else
    {
      if (this._triggeredCount == 0)
      {
        this.shockwaveSound.Play();
        this.HandlePlayerEffects(player);
      }
      ++this._triggeredCount;
    }
  }
}
