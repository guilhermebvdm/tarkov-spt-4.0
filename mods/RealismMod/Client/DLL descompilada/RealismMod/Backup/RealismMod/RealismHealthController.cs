// Decompiled with JetBrains decompiler
// Type: RealismMod.RealismHealthController
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using BepInEx.Configuration;
using Comfort.Common;
using EFT;
using EFT.Communications;
using EFT.HealthSystem;
using EFT.InventoryLogic;
using HarmonyLib;
using RealismMod.Health;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace RealismMod;

public class RealismHealthController
{
  public const float TOXIC_ITEM_FACTOR = 0.05f;
  public const float RAD_ITEM_FACTOR = 0.15f;
  public const float MIN_COUGH_THRESHOLD = 0.075f;
  public const float MIN_TOXICITY_THRESHOLD = 30f;
  public const float MIN_COUGH_DAMAGE_THRESHOLD = 0.14f;
  public const float OUT_OF_RAID_RAD_HEAL_FACTOR = 0.2f;
  public const float OUT_OF_RAID_RESOURCE_DEBUFF_MULTI = 10f;
  private HashSet<string> ToxicItems;
  private HashSet<string> RadioactiveItems;
  public Dictionary<string, EStimType> StimTypes;
  public List<EBodyPart> PossibleBodyParts;
  public EBodyPart[] BodyPartsArr;
  private List<ICustomHealthEffect> _activeHealthEffects;
  private List<EStimType> _activeStimOverdoses;
  private float _percentReources;
  private float _healthControllerTime;
  private float _effectsTime;
  private float _reliefWaitTime;
  private float _stimOverdoseWaitTime;
  private bool _doStimOverdoseTimer;
  private string _overdoseEffectToAdd;
  public const float DOUBLE_CLICK_TIME = 0.2f;
  private float _timeSinceLastClicked;
  private bool _clickTriggered;
  public const float ADRENALINE_BASE_VALUE = 70f;
  private float _adrenalineCooldownTime;
  public bool AdrenalineCooldownActive;
  private bool _reset1;
  private bool _reset2;
  private bool _reset3;
  private bool _reset4;
  private bool _reset5;
  private float _baseMaxHPRestore;
  private float _painInjuryStrength;
  public float PainEffectThreshold;
  public float PainReliefStrength;
  public float PainTunnelStrength;
  public int ReliefDuration;
  private bool _hasPKStims;
  public const float PAIN_RELIEF_INTERVAL = 15f;
  public const float PAIN_ARM_THRESHOLD = 30f;
  public const float PAIN_RELIEF_THRESHOLD = 30f;
  public const float BASE_OK_OVERDOSETHRESHOLD = 45f;
  public const float TOXICITY_THRESHOLD = 15f;
  public const float RADIATION_THRESHOLD = 30f;
  public const float RAD_TREATMENT_THRESHOLD = 40f;
  public const float BASE_TOX_RECOVERY_RATE = -0.05f;
  public const float HAZARD_INTERVAL = 10f;
  private float _hazardWaitTime;
  private bool _rightArmRuined;
  private bool _leftArmRuined;
  private bool _hasOverdosedStim;
  public float ResourcePerTick;
  private bool _haveNotifiedPKOverdose;
  private bool _addedCustomEffectsToDict;

  public DamageTracker DmgeTracker { get; }

  public PlayerZoneBridge PlayerHazardBridge { get; private set; }

  public bool HasPositiveAdrenalineEffect { get; set; }

  public bool HasNegativeAdrenalineEffect { get; set; }

  public bool IsCoughingInGas { get; private set; }

  public bool DoCoughingAudio { get; private set; }

  public int ToxicItemCount { get; private set; }

  public int RadItemCount { get; private set; }

  public bool IsPoisoned { get; set; }

  public bool CancelPassiveRegen { get; set; }

  public bool IsDehydrated { get; set; }

  public bool IsExhausted { get; set; }

  public float CurrentPassiveRegenBlockDuration { get; set; }

  public float BlockPassiveRegenBaseDuration
  {
    get => (float) (600.0 * (1.0 - (double) PlayerState.VitalityFactorStrong));
  }

  public bool BlockPassiveRegen
  {
    get
    {
      return PlayerState.IsSprinting || this.HasOverdosed || this.IsPoisoned || (double) HazardTracker.TotalToxicity > 15.0 || (double) HazardTracker.TotalRadiation > 15.0 || this.IsCoughingInGas;
    }
  }

  public float SurgeryPainFactor
  {
    get => (float) (15.0 * (1.0 - (double) PlayerState.StressResistanceFactor));
  }

  public float FracturePainFactor
  {
    get => (float) (15.0 * (1.0 - (double) PlayerState.StressResistanceFactor));
  }

  public float ZeroedPainFactor
  {
    get => (float) (20.0 * (1.0 - (double) PlayerState.StressResistanceFactor));
  }

  public float HpLossPainThreshold
  {
    get => (float) (0.800000011920929 * (1.0 - (double) PlayerState.StressResistanceFactor));
  }

  public float HpLossPainFactor
  {
    get => (float) (6.0 * (1.0 - (double) PlayerState.StressResistanceFactor));
  }

  public float AdrenalineMovementBonus
  {
    get
    {
      return this.HasPositiveAdrenalineEffect ? 0.9f + Mathf.Pow(PlayerState.StressResistanceFactor, 1.5f) : 1f;
    }
  }

  public float AdrenalineReloadBonus
  {
    get
    {
      return this.HasPositiveAdrenalineEffect ? 0.9f + Mathf.Pow(PlayerState.StressResistanceFactor, 1.25f) : 1f;
    }
  }

  public float AdrenalineStanceBonus
  {
    get
    {
      return this.HasPositiveAdrenalineEffect ? 0.9f + Mathf.Pow(PlayerState.StressResistanceFactor, 1.25f) : 1f;
    }
  }

  public float AdrenalineADSBonus
  {
    get
    {
      return this.HasPositiveAdrenalineEffect ? (float) (0.89999997615814209 + (double) PlayerState.StressResistanceFactor * 1.5) : 1f;
    }
  }

  public bool ArmsAreIncapacitated
  {
    get
    {
      return (this._rightArmRuined || this._leftArmRuined || (double) this.PainStrength > 30.0 && (double) this.PainStrength > (double) this.PainReliefStrength) && !this.IsOnPKStims;
    }
  }

  public bool HealthConditionPreventsTacSprint
  {
    get
    {
      return (double) HazardTracker.TotalToxicity > 20.0 || (double) HazardTracker.TotalRadiation > 40.0 || this.ArmsAreIncapacitated || this.HasOverdosed || this.IsPoisoned || this.IsCoughingInGas || this.IsDehydrated || this.IsExhausted;
    }
  }

  public bool HealthConditionForcedLowReady
  {
    get
    {
      return (double) HazardTracker.TotalToxicity > 40.0 || (double) HazardTracker.TotalRadiation > 60.0 || this.ArmsAreIncapacitated || this.HasOverdosed || this.IsPoisoned || this.IsCoughingInGas || this.IsDehydrated || this.IsExhausted;
    }
  }

  public bool HasOverdosedOnStim => this._hasOverdosedStim;

  public bool HasOverdosed
  {
    get
    {
      return (double) this.PainReliefStrength > (double) this.PKOverdoseThreshold || this.HasOverdosedOnStim;
    }
  }

  public float PKOverdoseThreshold
  {
    get => (float) (45.0 * (1.0 + (double) PlayerState.ImmuneSkillStrong));
  }

  public bool IsOnPKStims => this._hasPKStims && !this._hasOverdosedStim;

  public float PainSurgeryStrength { get; set; }

  public float PainStrength => this._painInjuryStrength + this.PainSurgeryStrength;

  public RealismHealthController(DamageTracker dmgTracker)
  {
    // ISSUE: unable to decompile the method.
  }

  public void ControllerUpdate()
  {
    if (!this._addedCustomEffectsToDict)
    {
      this.AddCustomEffectsToDict();
      this._addedCustomEffectsToDict = true;
    }
    if (!Utils.IsInHideout && Utils.PlayerIsReady)
    {
      this._healthControllerTime += Time.deltaTime;
      this._effectsTime += Time.deltaTime;
      this._reliefWaitTime += Time.deltaTime;
      this._hazardWaitTime += Time.deltaTime;
      this.HealthEffecTick();
      this.InRaidEffectDebuger();
      this.DropGearChecker();
      this.OverdoseTimer();
    }
    if (!GameWorldController.IsInRaid())
      this.OutOfRaidEffectDebugger();
    if (Utils.IsInHideout || !Utils.PlayerIsReady)
    {
      this.ResetAllEffects();
      this.DmgeTracker.ResetTracker();
    }
    this.AdrenalineTimer();
    this.RegenTimer();
    ScreenEffectsController.EffectsUpdate();
  }

  public void ResetAllEffects()
  {
    this._activeStimOverdoses.Clear();
    this._activeHealthEffects.Clear();
    this._painInjuryStrength = 0.0f;
    this.PainSurgeryStrength = 0.0f;
    this.PainReliefStrength = 0.0f;
    this.PainTunnelStrength = 0.0f;
    this.ReliefDuration = 0;
    this.HasNegativeAdrenalineEffect = false;
    this.HasPositiveAdrenalineEffect = false;
    this.CancelPassiveRegen = false;
    this.CurrentPassiveRegenBlockDuration = this.BlockPassiveRegenBaseDuration;
    this._hasOverdosedStim = false;
    this._leftArmRuined = false;
    this._rightArmRuined = false;
    this.ResetHealhPenalties();
  }

  private void InRaidEffectDebuger()
  {
    int num;
    if (PluginConfig.EnableMedicalLogging.Value)
    {
      KeyboardShortcut keyboardShortcut = PluginConfig.AddEffectKeybind.Value;
      num = Input.GetKeyDown(((KeyboardShortcut) ref keyboardShortcut).MainKey) ? 1 : 0;
    }
    else
      num = 0;
    if (num == 0)
      return;
    this.TestAddBaseEFTEffect(PluginConfig.AddEffectBodyPart.Value, Utils.GetYourPlayer(), PluginConfig.AddEffectType.Value);
    if (PluginConfig.EnableMedNotes.Value)
      NotificationManagerClass.DisplayMessageNotification($"Adding Health Effect {PluginConfig.AddEffectType.Value} To Part {((EBodyPart) PluginConfig.AddEffectBodyPart.Value).ToString()}", (ENotificationDurationType) 0, (ENotificationIconType) 0, new Color?());
  }

  private void OutOfRaidEffectDebugger()
  {
    int num;
    if (PluginConfig.EnableMedicalLogging.Value)
    {
      KeyboardShortcut keyboardShortcut = PluginConfig.AddEffectKeybind.Value;
      num = Input.GetKeyDown(((KeyboardShortcut) ref keyboardShortcut).MainKey) ? 1 : 0;
    }
    else
      num = 0;
    if (num == 0)
      return;
    this.TestAddOutOfRaidEffect();
  }

  private void DropGearChecker()
  {
    KeyboardShortcut keyboardShortcut = PluginConfig.DropGearKeybind.Value;
    if (Input.GetKeyDown(((KeyboardShortcut) ref keyboardShortcut).MainKey))
    {
      if (this._clickTriggered)
      {
        this.DropBlockingGear(Utils.GetYourPlayer());
        this._clickTriggered = false;
      }
      else
        this._clickTriggered = true;
      this._timeSinceLastClicked = 0.0f;
    }
    this._timeSinceLastClicked += Time.deltaTime;
    if ((double) this._timeSinceLastClicked <= 0.20000000298023224)
      return;
    this._clickTriggered = false;
  }

  private void OverdoseTimer()
  {
    if (!this._doStimOverdoseTimer)
      return;
    this._stimOverdoseWaitTime += Time.deltaTime;
    if ((double) this._stimOverdoseWaitTime >= 10.0)
    {
      this.AddStimDebuffs(Utils.GetYourPlayer(), this._overdoseEffectToAdd);
      this._doStimOverdoseTimer = false;
      this._stimOverdoseWaitTime = 0.0f;
    }
  }

  private void AdrenalineTimer()
  {
    if (!this.AdrenalineCooldownActive)
      return;
    this._adrenalineCooldownTime -= Time.deltaTime;
    if ((double) this._adrenalineCooldownTime <= 0.0)
    {
      this._adrenalineCooldownTime = (float) (70.0 * (1.0 - (double) PlayerState.StressResistanceFactor));
      this.AdrenalineCooldownActive = false;
    }
  }

  private void RegenTimer()
  {
    if (!this.CancelPassiveRegen)
      return;
    this.CurrentPassiveRegenBlockDuration -= Time.deltaTime;
    if ((double) this.CurrentPassiveRegenBlockDuration <= 0.0)
    {
      this.CancelPassiveRegen = false;
      this.CurrentPassiveRegenBlockDuration = this.BlockPassiveRegenBaseDuration;
    }
  }

  public void AddCustomEffectsToDict()
  {
    Type[] typeArray1 = new Type[5]
    {
      typeof (ResourceRateDrain),
      typeof (HealthChange),
      typeof (HealthDrain),
      typeof (ToxicityDamage),
      typeof (RadiationDamage)
    };
    Type type = typeof (GClass2862.GClass2863);
    FieldInfo field1 = type.GetField("type_0", BindingFlags.Public | BindingFlags.Static);
    Type[] typeArray2 = (Type[]) field1.GetValue((object) null);
    Type[] newTypeArr = new Type[typeArray2.Length + typeArray1.Length];
    typeArray2.CopyTo((Array) newTypeArr, 0);
    typeArray1.CopyTo((Array) newTypeArr, typeArray2.Length);
    field1.SetValue((object) null, (object) newTypeArr);
    FieldInfo field2 = type.GetField("dictionary_0", BindingFlags.Public | BindingFlags.Static);
    Dictionary<string, byte> dictionary = ((IEnumerable<Type>) newTypeArr).ToDictionary<Type, string, byte>((Func<Type, string>) (t => t.Name), (Func<Type, byte>) (t => (byte) Array.IndexOf<Type>(newTypeArr, t)));
    field2.SetValue((object) null, (object) dictionary);
    type.GetField("dictionary_1", BindingFlags.Public | BindingFlags.Static).SetValue((object) null, (object) dictionary.ToDictionary<KeyValuePair<string, byte>, byte, string>((Func<KeyValuePair<string, byte>, byte>) (kv => kv.Value), (Func<KeyValuePair<string, byte>, string>) (kv => kv.Key)));
  }

  private void TestAddBaseEFTEffect(int partIndex, Player player, string effect)
  {
    switch (effect)
    {
      case "":
        break;
      case "changeHp":
        player.ActiveHealthController.ChangeHealth((EBodyPart) partIndex, (float) (int) PluginConfig.test1.Value, GClass2855.Existence);
        break;
      default:
        Type nestedType = typeof (ActiveHealthController).GetNestedType(effect, BindingFlags.Instance | BindingFlags.NonPublic);
        if (nestedType == (Type) null)
        {
          Utils.Logger.LogError((object) ("nest type is null: " + effect));
          break;
        }
        MethodInfo methodInfo = this.GetAddBaseEFTEffectMethodInfo().MakeGenericMethod(nestedType);
        ActiveHealthController healthController = player.ActiveHealthController;
        object[] parameters = new object[6];
        parameters[0] = (object) (EBodyPart) partIndex;
        methodInfo.Invoke((object) healthController, parameters);
        break;
    }
  }

  private void TestAddOutOfRaidEffect()
  {
    HealthControllerClass healthController = Plugin.BSGHealthController;
    if (healthController == null)
      return;
    if (PluginConfig.AddEffectType.Value == "changeHp")
      healthController.ChangeHealth((EBodyPart) PluginConfig.AddEffectBodyPart.Value, (float) (int) PluginConfig.test1.Value, GClass2855.Existence);
    if (PluginConfig.AddEffectType.Value == "tox")
    {
      HazardTracker.TotalToxicity += PluginConfig.test1.Value;
      HazardTracker.UpdateHazardValues(ProfileData.PMCProfileId);
      HazardTracker.SaveHazardValues();
    }
    if (PluginConfig.AddEffectType.Value == "rad")
    {
      HazardTracker.TotalRadiation += PluginConfig.test1.Value;
      HazardTracker.UpdateHazardValues(ProfileData.PMCProfileId);
      HazardTracker.SaveHazardValues();
    }
    if (!PluginConfig.EnableMedNotes.Value)
      return;
    NotificationManagerClass.DisplayMessageNotification($"Adding Health Effect {PluginConfig.AddEffectType.Value} To Part {((EBodyPart) PluginConfig.AddEffectBodyPart.Value).ToString()}", (ENotificationDurationType) 0, (ENotificationIconType) 0, new Color?());
  }

  public void AddBasesEFTEffect(
    Player player,
    string effect,
    EBodyPart bodyPart,
    float? delayTime,
    float? duration,
    float? residueTime,
    float? strength)
  {
    this.GetAddBaseEFTEffectMethodInfo().MakeGenericMethod(typeof (ActiveHealthController).GetNestedType(effect, BindingFlags.Instance | BindingFlags.NonPublic)).Invoke((object) player.ActiveHealthController, new object[6]
    {
      (object) bodyPart,
      (object) delayTime,
      (object) duration,
      (object) residueTime,
      (object) strength,
      null
    });
  }

  public void AddBaseEFTEffectIfNoneExisting(
    Player player,
    string effect,
    EBodyPart bodyPart,
    float? delayTime,
    float? duration,
    float? residueTime,
    float? strength)
  {
    if (((GClass2814<ActiveHealthController.GClass2813>) player.ActiveHealthController).BodyPartEffects.Effects[(EBodyPart) 0].Any<KeyValuePair<string, float>>((Func<KeyValuePair<string, float>, bool>) (e => e.Key == effect)))
      return;
    this.AddBasesEFTEffect(player, effect, bodyPart, delayTime, duration, residueTime, strength);
  }

  public void AddToExistingBaseEFTEffect(
    Player player,
    string targetEffect,
    EBodyPart bodyPart,
    float delayTime,
    float? duration,
    float residueTime,
    float strength)
  {
    if (!((GClass2814<ActiveHealthController.GClass2813>) player.ActiveHealthController).BodyPartEffects.Effects[(EBodyPart) 0].Any<KeyValuePair<string, float>>((Func<KeyValuePair<string, float>, bool>) (e => e.Key == targetEffect)))
    {
      this.AddBasesEFTEffect(player, targetEffect, bodyPart, new float?(delayTime), duration, new float?(residueTime), new float?(strength));
    }
    else
    {
      IReadOnlyList<ActiveHealthController.GClass2813> ireadOnlyList0 = ((GClass2814<ActiveHealthController.GClass2813>) player.ActiveHealthController).IReadOnlyList_0;
      Type type1 = (Type) null;
      MedProperties.EffectTypes.TryGetValue(targetEffect, out type1);
      for (int index = ireadOnlyList0.Count - 1; index >= 0; --index)
      {
        ActiveHealthController.GClass2813 gclass2813 = ireadOnlyList0[index];
        Type type2 = gclass2813.Type;
        EBodyPart bodyPart1 = gclass2813.BodyPart;
        if (type2 == type1)
          gclass2813.AddWorkTime(duration, false);
      }
    }
  }

  public void TryAddAdrenaline(
    Player player,
    float painkillerDuration,
    float negativeEffectDuration,
    float negativeEffectStrength)
  {
    if (!PluginConfig.EnableAdrenaline.Value || this.AdrenalineCooldownActive)
      return;
    this.AdrenalineCooldownActive = true;
    AdrenalineEffect newEffect = new AdrenalineEffect(player, new int?((int) negativeEffectDuration + (int) painkillerDuration), 0, negativeEffectDuration, painkillerDuration, negativeEffectStrength, this);
    Plugin.RealHealthController.AddCustomEffect((ICustomHealthEffect) newEffect, true);
  }

  public MethodInfo GetAddBaseEFTEffectMethodInfo()
  {
    return ((IEnumerable<MethodInfo>) typeof (ActiveHealthController).GetMethods(BindingFlags.Instance | BindingFlags.Public)).First<MethodInfo>((Func<MethodInfo, bool>) (e => e.GetParameters().Length == 6 && e.GetParameters()[0].Name == "bodyPart" && e.GetParameters()[5].Name == "initCallback" && e.IsGenericMethod));
  }

  public void RemoveBaseEFTEffect(Player player, EBodyPart targetBodyPart, string targetEffect)
  {
    ((GClass2814<ActiveHealthController.GClass2813>) player.ActiveHealthController).GetAllActiveEffects(targetBodyPart);
    IReadOnlyList<ActiveHealthController.GClass2813> ireadOnlyList0 = ((GClass2814<ActiveHealthController.GClass2813>) player.ActiveHealthController).IReadOnlyList_0;
    Type type1 = (Type) null;
    if (!MedProperties.EffectTypes.TryGetValue(targetEffect, out type1))
      return;
    for (int index = ireadOnlyList0.Count - 1; index >= 0; --index)
    {
      ActiveHealthController.GClass2813 gclass2813 = ireadOnlyList0[index];
      Type type2 = gclass2813.Type;
      EBodyPart bodyPart = gclass2813.BodyPart;
      if (type2 == type1 && bodyPart == targetBodyPart)
        gclass2813.ForceResidue();
    }
  }

  public void AddCustomEffect(ICustomHealthEffect newEffect, bool canStack)
  {
    if (!canStack)
    {
      foreach (ICustomHealthEffect activeHealthEffect in this._activeHealthEffects)
      {
        if (activeHealthEffect.GetType() == newEffect.GetType() && activeHealthEffect.BodyPart == newEffect.BodyPart)
        {
          this.RemoveCustomEffectOfType(newEffect.GetType(), newEffect.BodyPart);
          break;
        }
      }
    }
    this._activeHealthEffects.Add(newEffect);
  }

  public bool AddCustomEffectIfNoneExisting(ICustomHealthEffect newEffect)
  {
    foreach (ICustomHealthEffect activeHealthEffect in this._activeHealthEffects)
    {
      if (activeHealthEffect.GetType() == newEffect.GetType() && activeHealthEffect.BodyPart == newEffect.BodyPart)
        return false;
    }
    this._activeHealthEffects.Add(newEffect);
    return true;
  }

  public bool RemoveCustomEffectOfType(Type effect, EBodyPart bodyPart)
  {
    bool flag = false;
    for (int index = this._activeHealthEffects.Count - 1; index >= 0; --index)
    {
      ICustomHealthEffect activeHealthEffect = this._activeHealthEffects[index];
      if (activeHealthEffect.GetType() == effect && activeHealthEffect.BodyPart == bodyPart)
      {
        this._activeHealthEffects.RemoveAt(index);
        flag = true;
      }
    }
    return flag;
  }

  public bool HasCustomEffectOfType(Type effect, EBodyPart bodyPart = 7)
  {
    bool flag1 = false;
    for (int index = this._activeHealthEffects.Count - 1; index >= 0; --index)
    {
      ICustomHealthEffect activeHealthEffect = this._activeHealthEffects[index];
      bool flag2 = bodyPart == 7 || activeHealthEffect.BodyPart == bodyPart;
      if (activeHealthEffect.GetType() == effect & flag2)
        flag1 = true;
    }
    return flag1;
  }

  public ICustomHealthEffect GetCustomEffectOfType<T>(EBodyPart bodyPart) where T : ICustomHealthEffect
  {
    ICustomHealthEffect customEffectOfType = (ICustomHealthEffect) null;
    for (int index = this._activeHealthEffects.Count - 1; index >= 0; --index)
    {
      ICustomHealthEffect activeHealthEffect = this._activeHealthEffects[index];
      if (activeHealthEffect.GetType() == typeof (T) && activeHealthEffect.BodyPart == bodyPart)
        customEffectOfType = activeHealthEffect;
    }
    return customEffectOfType;
  }

  public void CancelPendingEffects()
  {
    for (int index = this._activeHealthEffects.Count - 1; index >= 0; --index)
    {
      if ((double) this._activeHealthEffects[index].Delay > 0.0)
        this._activeHealthEffects.RemoveAt(index);
    }
  }

  public void RemoveRegenEffectsOfType(EDamageType damageType)
  {
    List<HealthRegenEffect> regenEffects = this._activeHealthEffects.OfType<HealthRegenEffect>().ToList<HealthRegenEffect>();
    regenEffects.RemoveAll((Predicate<HealthRegenEffect>) (r => r.DamageType == damageType));
    this._activeHealthEffects.RemoveAll((Predicate<ICustomHealthEffect>) (a => !((IEnumerable<ICustomHealthEffect>) regenEffects).Contains<ICustomHealthEffect>(a)));
  }

  public void RemoveEffectsOfType(EHealthEffectType effectType)
  {
    this._activeHealthEffects.RemoveAll((Predicate<ICustomHealthEffect>) (a => a.EffectType == effectType));
  }

  public void ResetBleedDamageRecord(Player player)
  {
    bool flag1 = false;
    bool flag2 = false;
    Type heavyBleedType;
    MedProperties.EffectTypes.TryGetValue("HeavyBleeding", out heavyBleedType);
    Type lightBleedType;
    MedProperties.EffectTypes.TryGetValue("LightBleeding", out lightBleedType);
    foreach (int num in this.BodyPartsArr)
    {
      EBodyPart ebodyPart = (EBodyPart) num;
      IEnumerable<IEffect> allActiveEffects = ((GClass2814<ActiveHealthController.GClass2813>) player.ActiveHealthController).GetAllActiveEffects(ebodyPart);
      if (heavyBleedType != (Type) null && allActiveEffects.Any<IEffect>((Func<IEffect, bool>) (h => h.Type == heavyBleedType)))
        flag1 = true;
      if (lightBleedType != (Type) null && allActiveEffects.Any<IEffect>((Func<IEffect, bool>) (l => l.Type == lightBleedType)))
        flag2 = true;
    }
    if (!flag1)
      this.DmgeTracker.TotalHeavyBleedDamage = 0.0f;
    if (flag2)
      return;
    this.DmgeTracker.TotalLightBleedDamage = 0.0f;
  }

  public bool HasBaseEFTEffect(Player player, string targetEffect)
  {
    ((GClass2814<ActiveHealthController.GClass2813>) player.ActiveHealthController).GetAllEffects();
    IReadOnlyList<ActiveHealthController.GClass2813> ireadOnlyList0 = ((GClass2814<ActiveHealthController.GClass2813>) player.ActiveHealthController).IReadOnlyList_0;
    Type type = (Type) null;
    if (MedProperties.EffectTypes.TryGetValue(targetEffect, out type))
    {
      for (int index = ireadOnlyList0.Count - 1; index >= 0; --index)
      {
        if (ireadOnlyList0[index].Type == type)
          return true;
      }
    }
    return false;
  }

  private void AddStimDebuffs(Player player, string debuffId)
  {
    MedsItemClass medsItemClass = (MedsItemClass) Singleton<ItemFactoryClass>.Instance.CreateItem(Utils.GenId(), debuffId, (GClass825) null);
    ((Item) medsItemClass).CurrentAddress = (ItemAddress) player.InventoryController.FindQuestGridToPickUp((Item) medsItemClass);
    player.ActiveHealthController.DoMedEffect((Item) medsItemClass, (EBodyPart) 0, new float?());
    if (!PluginConfig.EnableGeneralLogging.Value)
      return;
    Utils.Logger.LogWarning((object) ("is null " + (medsItemClass == null).ToString()));
    Utils.Logger.LogWarning((object) (((StimulatorBuffsComponent) medsItemClass.HealthEffectsComponent).StimulatorBuffs ?? ""));
    Utils.Logger.LogWarning((object) ("added " + debuffId));
  }

  private void EvaluateStimSingles(
    Player player,
    IEnumerable<IGrouping<EStimType, StimEffectShell>> stimGroups)
  {
    this._hasPKStims = false;
    foreach (IGrouping<EStimType, StimEffectShell> stimGroup in stimGroups)
    {
      switch (stimGroup.Key)
      {
        case EStimType.Regenerative:
          this._activeStimOverdoses.Remove(EStimType.Regenerative);
          break;
        case EStimType.Damage:
          this._activeStimOverdoses.Remove(EStimType.Damage);
          this._hasPKStims = true;
          break;
        case EStimType.Adrenal:
          this._activeStimOverdoses.Remove(EStimType.Adrenal);
          this._hasPKStims = true;
          break;
        case EStimType.Clotting:
          this._activeStimOverdoses.Remove(EStimType.Clotting);
          break;
        case EStimType.Performance:
          this._activeStimOverdoses.Remove(EStimType.Performance);
          break;
        case EStimType.Generic:
          this._activeStimOverdoses.Remove(EStimType.Generic);
          break;
        case EStimType.Weight:
          this._activeStimOverdoses.Remove(EStimType.Weight);
          break;
      }
    }
  }

  private void AddStimOverdose(EStimType stimType, string debuff, string message)
  {
    this._activeStimOverdoses.Add(stimType);
    this._doStimOverdoseTimer = true;
    this._overdoseEffectToAdd = debuff;
    if (!PluginConfig.EnableMedNotes.Value)
      return;
    NotificationManagerClass.DisplayWarningNotification(message, (ENotificationDurationType) 1);
  }

  private void EvaluateStimDuplicates(
    Player player,
    IEnumerable<IGrouping<EStimType, StimEffectShell>> stimGroups)
  {
    foreach (IGrouping<EStimType, StimEffectShell> stimGroup in stimGroups)
    {
      switch (stimGroup.Key)
      {
        case EStimType.Regenerative:
          if (!this._activeStimOverdoses.Contains(EStimType.Regenerative))
          {
            this.AddStimOverdose(EStimType.Regenerative, "6783ad5260cc8e9597065ec5", "Overdosed On Regenerative Stims");
            break;
          }
          break;
        case EStimType.Damage:
          if (!this._activeStimOverdoses.Contains(EStimType.Damage))
          {
            this.AddStimOverdose(EStimType.Damage, "6783adc3899d65035b52e21b", "Overdosed On Combat Stims");
            break;
          }
          break;
        case EStimType.Adrenal:
          if (!this._activeStimOverdoses.Contains(EStimType.Adrenal))
          {
            this.AddStimOverdose(EStimType.Adrenal, "6783ad365524129829f0099d", "Overdosed On Adrenal Stims");
            break;
          }
          break;
        case EStimType.Clotting:
          if (!this._activeStimOverdoses.Contains(EStimType.Clotting))
          {
            this.AddStimOverdose(EStimType.Clotting, "6783ad5fce6705d14a117b15", "Overdosed On Coagulating Stims");
            break;
          }
          break;
        case EStimType.Performance:
          if (!this._activeStimOverdoses.Contains(EStimType.Performance))
          {
            this.AddStimOverdose(EStimType.Performance, "6783ad9f56a70af01706bf5f", "Overdosed On Performance-Enhancing Stims");
            break;
          }
          break;
        case EStimType.Generic:
          if (!this._activeStimOverdoses.Contains(EStimType.Generic))
          {
            this.AddStimOverdose(EStimType.Generic, "6783adb2a43ec97b902c4080", "Overdosed On Stims");
            break;
          }
          break;
        case EStimType.Weight:
          if (!this._activeStimOverdoses.Contains(EStimType.Weight))
          {
            this.AddStimOverdose(EStimType.Weight, "6783ad886700d7d90daf548d", "Overdosed On Weight-Reducing Stims");
            break;
          }
          break;
      }
    }
  }

  public void EvaluateActiveStims(Player player)
  {
    IEnumerable<IGrouping<EStimType, StimEffectShell>> source = this._activeHealthEffects.OfType<StimEffectShell>().GroupBy<StimEffectShell, EStimType>((Func<StimEffectShell, EStimType>) (effect => effect.StimType));
    IEnumerable<IGrouping<EStimType, StimEffectShell>> groupings = source.Where<IGrouping<EStimType, StimEffectShell>>((Func<IGrouping<EStimType, StimEffectShell>, bool>) (group => group.Count<StimEffectShell>() > 1));
    IEnumerable<IGrouping<EStimType, StimEffectShell>> stimGroups = source.Where<IGrouping<EStimType, StimEffectShell>>((Func<IGrouping<EStimType, StimEffectShell>, bool>) (group => group.Count<StimEffectShell>() <= 1));
    int num = groupings.Sum<IGrouping<EStimType, StimEffectShell>>((Func<IGrouping<EStimType, StimEffectShell>, int>) (group => group.Count<StimEffectShell>()));
    this.EvaluateStimDuplicates(player, groupings);
    this.EvaluateStimSingles(player, stimGroups);
    if (num > 1)
      this._hasOverdosedStim = true;
    else
      this._hasOverdosedStim = false;
  }

  public EStimType GetStimType(string id)
  {
    EStimType estimType;
    return this.StimTypes.TryGetValue(id, out estimType) ? estimType : EStimType.Generic;
  }

  public void ResourceRegenCheck(Player player)
  {
    float num = player.Skills.VitalityBuffSurviobilityInc.Value;
    int delay = (int) Math.Round(15.0 * (1.0 - (double) num), 2);
    float tickRate = (float) Math.Round(0.2199999988079071 * (1.0 + (double) num), 2);
    this.IsDehydrated = this.HasBaseEFTEffect(player, "Dehydration");
    this.IsExhausted = this.HasBaseEFTEffect(player, "Exhaustion");
    if (this.IsDehydrated)
      this.RemoveRegenEffectsOfType((EDamageType) 65536 /*0x010000*/);
    if (!this.IsDehydrated && (double) this.DmgeTracker.TotalDehydrationDamage > 0.0)
    {
      this.RestoreHPArossBody(player, this.DmgeTracker.TotalDehydrationDamage, delay, (EDamageType) 65536 /*0x010000*/, tickRate);
      this.DmgeTracker.TotalDehydrationDamage = 0.0f;
    }
    if (this.IsExhausted)
      this.RemoveRegenEffectsOfType((EDamageType) 131072 /*0x020000*/);
    if (this.IsExhausted || (double) this.DmgeTracker.TotalExhaustionDamage <= 0.0)
      return;
    this.RestoreHPArossBody(player, this.DmgeTracker.TotalExhaustionDamage, delay, (EDamageType) 131072 /*0x020000*/, tickRate);
    this.DmgeTracker.TotalExhaustionDamage = 0.0f;
  }

  private void PainReliefCheck(Player player)
  {
    if ((double) this.PainStrength >= (double) this.PainEffectThreshold && !this.IsOnPKStims)
      this.AddBaseEFTEffectIfNoneExisting(player, "Pain", (EBodyPart) 1, new float?(0.0f), new float?(15f), new float?(1f), new float?(1f));
    this.ReliefDuration = Math.Max(this.ReliefDuration - 1, 0);
    if (this.ReliefDuration <= 0)
      return;
    if ((double) this.PainReliefStrength >= (double) this.PainStrength || this.IsOnPKStims)
      this.AddBaseEFTEffectIfNoneExisting(player, "PainKiller", (EBodyPart) 0, new float?(1f), new float?((float) this.ReliefDuration), new float?(5f), new float?(1f));
    else if ((double) this.PainStrength > (double) this.PainReliefStrength)
      this.RemoveBaseEFTEffect(player, (EBodyPart) 0, "PainKiller");
    if ((double) this._reliefWaitTime >= 15.0)
    {
      this.AddBasesEFTEffect(player, "TunnelVision", (EBodyPart) 0, new float?(1f), new float?(15f), new float?(5f), new float?(this.PainTunnelStrength));
      if ((double) this.PainReliefStrength > (double) this.PKOverdoseThreshold)
      {
        if (!this._haveNotifiedPKOverdose)
        {
          if (PluginConfig.EnableMedNotes.Value)
            NotificationManagerClass.DisplayWarningNotification("Overdosed On Pain Medication", (ENotificationDurationType) 1);
          this._haveNotifiedPKOverdose = true;
        }
        this.AddBasesEFTEffect(player, "Contusion", (EBodyPart) 0, new float?(1f), new float?(15f), new float?(5f), new float?(0.35f));
        this.AddToExistingBaseEFTEffect(player, "Tremor", (EBodyPart) 0, 1f, new float?(15f), 5f, 1f);
      }
      else
        this._haveNotifiedPKOverdose = false;
      this._reliefWaitTime = 0.0f;
    }
  }

  private void TickEffects()
  {
    for (int index = this._activeHealthEffects.Count - 1; index >= 0; --index)
    {
      ICustomHealthEffect activeHealthEffect = this._activeHealthEffects[index];
      activeHealthEffect.Delay = Math.Max(activeHealthEffect.Delay - 1, 0);
      int? duration = activeHealthEffect.Duration;
      int num1;
      if (duration.HasValue)
      {
        duration = activeHealthEffect.Duration;
        float? nullable = duration.HasValue ? new float?((float) duration.GetValueOrDefault()) : new float?();
        float num2 = 0.0f;
        num1 = (double) nullable.GetValueOrDefault() > (double) num2 & nullable.HasValue ? 1 : 0;
      }
      else
        num1 = 1;
      if (num1 != 0)
      {
        activeHealthEffect.Tick();
      }
      else
      {
        if (PluginConfig.EnableGeneralLogging.Value)
          Utils.Logger.LogWarning((object) "Removing Effect Due to Duration");
        this._activeHealthEffects.RemoveAt(index);
      }
    }
  }

  public void HealthEffecTick()
  {
    Player yourPlayer = Utils.GetYourPlayer();
    PlayerState.ImmuneSkillWeak = ((SkillManager.SkillBuffClass) yourPlayer.Skills.ImmunityPainKiller).Value;
    PlayerState.ImmuneSkillStrong = ((SkillManager.SkillBuffClass) yourPlayer.Skills.ImmunityMiscEffects).Value;
    PlayerState.StressResistanceFactor = yourPlayer.Skills.StressPain.Value;
    PlayerState.VitalityFactorStrong = yourPlayer.Skills.VitalityBuffBleedChanceRed.Value;
    if ((double) this._healthControllerTime >= 0.5 && !this._reset1)
    {
      this.ResetBleedDamageRecord(yourPlayer);
      this._reset1 = true;
    }
    if ((double) this._healthControllerTime >= 1.0 && !this._reset2)
    {
      this.ResourceRegenCheck(yourPlayer);
      this._reset2 = true;
    }
    if ((double) this._healthControllerTime >= 2.0 && !this._reset3)
    {
      this.DoubleBleedCheck(yourPlayer);
      this._reset3 = true;
    }
    if ((double) this._healthControllerTime >= 2.5 && !this._reset4)
    {
      this.EvaluateActiveStims(yourPlayer);
      this._reset4 = true;
    }
    if ((double) this._healthControllerTime >= 3.0 && !this._reset5)
    {
      this.PlayerInjuryStateCheck(yourPlayer);
      this._reset5 = true;
    }
    if ((double) this._effectsTime >= 1.0)
    {
      if (Plugin.ServerConfig.enable_hazard_zones)
        this.HazardZoneHealthTick(yourPlayer);
      this.PainReliefCheck(yourPlayer);
      this.TickEffects();
      this._effectsTime = 0.0f;
    }
    this.DoResourceDrain(yourPlayer.ActiveHealthController, Time.deltaTime);
    if (PluginConfig.PassiveRegen.Value && !this.HasCustomEffectOfType(typeof (PassiveHealthRegenEffect)))
      this.AddCustomEffect((ICustomHealthEffect) new PassiveHealthRegenEffect(yourPlayer, this), false);
    if ((double) this._healthControllerTime < 3.0)
      return;
    this._healthControllerTime = 0.0f;
    this._reset1 = false;
    this._reset2 = false;
    this._reset3 = false;
    this._reset4 = false;
    this._reset5 = false;
  }

  public void DropBlockingGear(Player player)
  {
    Player.ItemHandsController handsController = player.HandsController as Player.ItemHandsController;
    if (Object.op_Inequality((Object) handsController, (Object) null) && handsController.CurrentCompassState)
    {
      handsController.SetCompassState(false);
    }
    else
    {
      if (!Object.op_Equality((Object) player.MovementContext.StationaryWeapon, (Object) null) || player.HandsController.IsPlacingBeacon() || player.HandsController.IsInInteractionStrictCheck() || player.CurrentStateName == 9 || player.IsSprintEnabled)
        return;
      InventoryEquipment equipment = player.Equipment;
      List<Item> objList = new List<Item>();
      List<EquipmentSlot> equipmentSlotList = new List<EquipmentSlot>();
      if (this.BodyPartHasBleed(player, (EBodyPart) 0))
      {
        equipmentSlotList.Add((EquipmentSlot) 11);
        equipmentSlotList.Add((EquipmentSlot) 12);
        equipmentSlotList.Add((EquipmentSlot) 10);
      }
      if (this.BodyPartHasBleed(player, (EBodyPart) 2) || this.BodyPartHasBleed(player, (EBodyPart) 1))
      {
        equipmentSlotList.Add((EquipmentSlot) 6);
        equipmentSlotList.Add((EquipmentSlot) 4);
        equipmentSlotList.Add((EquipmentSlot) 7);
      }
      if (equipmentSlotList.Count < 1)
        return;
      foreach (EquipmentSlot equipmentSlot in equipmentSlotList)
      {
        Item containedItem = equipment.GetSlot(equipmentSlot).ContainedItem;
        if (containedItem != null)
          objList.Add(containedItem);
      }
      if (objList.Count < 1)
        return;
      foreach (Item obj in objList)
      {
        if (((TraderControllerClass) player.InventoryController).CanThrow(obj))
          ((TraderControllerClass) player.InventoryController).TryThrowItem(obj, (Callback) null, false);
      }
    }
  }

  private void GearBlocksMouth(ref bool blocksMouth, Item item)
  {
    if (item == null)
      return;
    Gear dataObj = TemplateStats.GetDataObj<Gear>(TemplateStats.GearStats, MongoID.op_Implicit(item.TemplateId));
    blocksMouth = dataObj.BlocksMouth;
  }

  private bool ToggleableItemBlocksMouth(Player player)
  {
    FaceShieldComponent component1 = player.FaceShieldObserver.Component;
    NightVisionComponent component2 = player.NightVisionObserver.Component;
    Gear dataObj = component1 == null ? (Gear) null : TemplateStats.GetDataObj<Gear>(TemplateStats.GearStats, MongoID.op_Implicit(((GClass3175) component1).Item.TemplateId));
    return (dataObj != null && (component1.Togglable == null || component1.Togglable.On) && dataObj.BlocksMouth) | (component2 != null && (component2.Togglable == null || component2.Togglable.On));
  }

  private bool CheckIfParentGearBlocksMouth(Item head, Item face)
  {
    bool blocksMouth1 = false;
    bool blocksMouth2 = false;
    this.GearBlocksMouth(ref blocksMouth2, head);
    this.GearBlocksMouth(ref blocksMouth1, face);
    return blocksMouth1 | blocksMouth2;
  }

  private bool CheckIfNestedGearBlocksMouth(InventoryEquipment equipment)
  {
    IEnumerable<Item> objs = equipment.GetSlot((EquipmentSlot) 11).ContainedItem is CompoundItem containedItem ? GClass3176.GetAllItemsFromCollection((GClass3050) containedItem).OfType<Item>() : (IEnumerable<Item>) null;
    if (objs != null)
    {
      foreach (Item obj in objs)
      {
        Gear dataObj = TemplateStats.GetDataObj<Gear>(TemplateStats.GearStats, MongoID.op_Implicit(obj.TemplateId));
        FaceShieldComponent itemComponent = obj.GetItemComponent<FaceShieldComponent>();
        if (dataObj != null && itemComponent == null && dataObj.BlocksMouth)
          return true;
      }
    }
    return false;
  }

  public bool MouthIsBlocked(Player player, Item head, Item face, InventoryEquipment equipment)
  {
    bool flag = this.CheckIfParentGearBlocksMouth(head, face) || this.ToggleableItemBlocksMouth(player) || this.CheckIfNestedGearBlocksMouth(equipment);
    return this.IsCoughingInGas || PluginConfig.GearBlocksEat.Value & flag;
  }

  public bool BodyPartHasBleed(Player player, EBodyPart part)
  {
    IEnumerable<IEffect> allActiveEffects = ((GClass2814<ActiveHealthController.GClass2813>) player.ActiveHealthController).GetAllActiveEffects(part);
    Type heavyBleedType;
    MedProperties.EffectTypes.TryGetValue("HeavyBleeding", out heavyBleedType);
    Type lightBleedType;
    MedProperties.EffectTypes.TryGetValue("LightBleeding", out lightBleedType);
    return (heavyBleedType != (Type) null && allActiveEffects.Any<IEffect>((Func<IEffect, bool>) (e => e.Type == heavyBleedType))) | (lightBleedType != (Type) null && allActiveEffects.Any<IEffect>((Func<IEffect, bool>) (e => e.Type == lightBleedType)));
  }

  public IEnumerable<IEffect> GetInjuriesOnBodyPart(
    Player player,
    EBodyPart part,
    ref bool hasHeavyBleed,
    ref bool hasLightBleed,
    ref bool hasFracture)
  {
    IEnumerable<IEffect> allActiveEffects = ((GClass2814<ActiveHealthController.GClass2813>) player.ActiveHealthController).GetAllActiveEffects(part);
    Type heavyBleedType;
    MedProperties.EffectTypes.TryGetValue("HeavyBleeding", out heavyBleedType);
    Type lightBleedType;
    MedProperties.EffectTypes.TryGetValue("LightBleeding", out lightBleedType);
    Type fractureType;
    MedProperties.EffectTypes.TryGetValue("BrokenBone", out fractureType);
    hasHeavyBleed = heavyBleedType != (Type) null && allActiveEffects.Any<IEffect>((Func<IEffect, bool>) (e => e.Type == heavyBleedType));
    hasLightBleed = lightBleedType != (Type) null && allActiveEffects.Any<IEffect>((Func<IEffect, bool>) (e => e.Type == lightBleedType));
    hasFracture = fractureType != (Type) null && allActiveEffects.Any<IEffect>((Func<IEffect, bool>) (e => e.Type == fractureType));
    return allActiveEffects;
  }

  public void GetBodyPartType(
    EBodyPart part,
    ref bool isNotLimb,
    ref bool isHead,
    ref bool isBody)
  {
    isHead = part == 0;
    isBody = part == 1 || part == 2;
    isNotLimb = part == 1 || part == 2 || part == 0;
  }

  public void CanConsume(Player player, Item item, ref bool canUse)
  {
    Consumable dataObj = TemplateStats.GetDataObj<Consumable>(TemplateStats.ConsumableStats, MongoID.op_Implicit(item.TemplateId));
    InventoryEquipment equipment = player.Equipment;
    Item containedItem1 = equipment.GetSlot((EquipmentSlot) 10).ContainedItem;
    Item containedItem2 = equipment.GetSlot((EquipmentSlot) 11).ContainedItem;
    if (this.MouthIsBlocked(player, containedItem2, containedItem1, equipment))
    {
      if (PluginConfig.EnableMedNotes.Value)
        NotificationManagerClass.DisplayWarningNotification(this.GetHealBlockMessage(this.IsCoughingInGas ? EHealBlockType.Gas : EHealBlockType.Food), (ENotificationDurationType) 1);
      canUse = false;
    }
    else
    {
      this.ConsumeAlcohol(player, item, dataObj);
      this.CheckIfReducesHazardInRaid(item, player, false);
      PlayerState.BlockFSWhileConsooming = true;
    }
  }

  private void ConsumeAlcohol(Player player, Item item, Consumable consumableStats)
  {
    if (consumableStats.ConsumableType != EConsumableType.Alcohol)
      return;
    this.AddPainkillerEffect(player, item, consumableStats);
  }

  public void DoPassiveRegen(
    float tickRate,
    EBodyPart bodyPart,
    Player player,
    int delay,
    float hpToRestore,
    EDamageType damageType)
  {
    if (this.HasCustomEffectOfType(typeof (TourniquetEffect), bodyPart) || (double) ((GInterface355) player.HealthController).GetBodyPartHealth(bodyPart, false).Current <= 0.0)
      return;
    this.AddCustomEffect((ICustomHealthEffect) new HealthRegenEffect(tickRate, new int?(), bodyPart, player, delay, hpToRestore, damageType, this), false);
  }

  public void RestoreHPArossBody(
    Player player,
    float hpToRestore,
    int delay,
    EDamageType damageType,
    float tickRate)
  {
    hpToRestore = (float) Mathf.RoundToInt(hpToRestore / (float) this.BodyPartsArr.Length);
    foreach (int num in this.BodyPartsArr)
    {
      EBodyPart ebodyPart = (EBodyPart) num;
      if (!this.HasCustomEffectOfType(typeof (TourniquetEffect), ebodyPart) && (double) ((GInterface355) player.HealthController).GetBodyPartHealth(ebodyPart, false).Current > 0.0)
        this.AddCustomEffect((ICustomHealthEffect) new HealthRegenEffect(tickRate, new int?(), ebodyPart, player, delay, hpToRestore, damageType, this), false);
    }
  }

  public void TrnqtRestoreHPArossBody(
    Player player,
    float hpToRestore,
    int delay,
    EBodyPart bodyPart,
    EDamageType damageType,
    float vitalitySkill)
  {
    hpToRestore = (float) Mathf.RoundToInt(hpToRestore / (float) (this.BodyPartsArr.Length - 1));
    float hpTick = (float) Math.Round(0.85000002384185791 * (1.0 + (double) vitalitySkill), 2);
    foreach (int num in this.BodyPartsArr)
    {
      EBodyPart ebodyPart = (EBodyPart) num;
      if (ebodyPart != bodyPart && !this.HasCustomEffectOfType(typeof (TourniquetEffect), ebodyPart) && (double) ((GInterface355) player.HealthController).GetBodyPartHealth(ebodyPart, false).Current > 0.0)
        this.AddCustomEffect((ICustomHealthEffect) new HealthRegenEffect(hpTick, new int?(), ebodyPart, player, delay, hpToRestore, damageType, this), false);
    }
  }

  private void HandleHeavyBleedHeal(
    MedsItemClass meds,
    Consumable medStats,
    EBodyPart bodyPart,
    Player player,
    bool isNotLimb,
    float vitalitySkill,
    float regenTickRate)
  {
    int useTime = (int) meds.HealthEffectsComponent.UseTime;
    if (PluginConfig.EnableMedNotes.Value)
      NotificationManagerClass.DisplayMessageNotification($"Heavy Bleed On {bodyPart.ToString()} Healed, Restoring HP.", (ENotificationDurationType) 1, (ENotificationIconType) 0, new Color?());
    float hpTick = (float) Math.Round((double) medStats.TrnqtDamage * (1.0 - (double) vitalitySkill), 2);
    float hpToRestore = Mathf.Min(this.DmgeTracker.TotalHeavyBleedDamage, Mathf.Round(this._baseMaxHPRestore * (1f + vitalitySkill)));
    if ((medStats.HeavyBleedHealType == EHeavyBleedHealType.Combo || medStats.HeavyBleedHealType == EHeavyBleedHealType.Tourniquet) && !isNotLimb)
    {
      this.AddCustomEffect((ICustomHealthEffect) new TourniquetEffect(hpTick, new int?(), bodyPart, player, useTime, this), false);
      if ((double) this.DmgeTracker.TotalHeavyBleedDamage > 0.0)
        this.TrnqtRestoreHPArossBody(player, hpToRestore, useTime, bodyPart, (EDamageType) 32768 /*0x8000*/, vitalitySkill);
    }
    else if ((double) this.DmgeTracker.TotalHeavyBleedDamage > 0.0)
      this.RestoreHPArossBody(player, hpToRestore, useTime, (EDamageType) 32768 /*0x8000*/, regenTickRate);
    this.DmgeTracker.TotalHeavyBleedDamage = Mathf.Max(this.DmgeTracker.TotalHeavyBleedDamage - hpToRestore, 0.0f);
  }

  private void HandleLightBleedHeal(
    MedsItemClass meds,
    Consumable medStats,
    EBodyPart bodyPart,
    Player player,
    bool isNotLimb,
    float vitalitySkill,
    float regenTickRate)
  {
    int useTime = (int) meds.HealthEffectsComponent.UseTime;
    if (PluginConfig.EnableMedNotes.Value)
      NotificationManagerClass.DisplayMessageNotification($"Light Bleed On {bodyPart.ToString()} Healed, Restoring HP.", (ENotificationDurationType) 1, (ENotificationIconType) 0, new Color?());
    float hpTick = (float) Math.Round((double) medStats.TrnqtDamage * (1.0 - (double) vitalitySkill), 2);
    float hpToRestore = Mathf.Min(this.DmgeTracker.TotalLightBleedDamage, Mathf.Round(this._baseMaxHPRestore * (1f + vitalitySkill)));
    if (medStats.ConsumableType == EConsumableType.Tourniquet && !isNotLimb)
    {
      this.AddCustomEffect((ICustomHealthEffect) new TourniquetEffect(hpTick, new int?(), bodyPart, player, useTime, this), false);
      if ((double) this.DmgeTracker.TotalLightBleedDamage > 0.0)
        this.TrnqtRestoreHPArossBody(player, hpToRestore, useTime, bodyPart, (EDamageType) 16384 /*0x4000*/, vitalitySkill);
    }
    else if ((double) this.DmgeTracker.TotalLightBleedDamage > 0.0)
      this.RestoreHPArossBody(player, hpToRestore, useTime, (EDamageType) 16384 /*0x4000*/, regenTickRate);
    this.DmgeTracker.TotalLightBleedDamage = Mathf.Max(this.DmgeTracker.TotalLightBleedDamage - hpToRestore, 0.0f);
  }

  private void HandleSurgery(
    MedsItemClass meds,
    Consumable medStats,
    EBodyPart bodyPart,
    Player player,
    float surgerySkill)
  {
    int useTime = (int) meds.HealthEffectsComponent.UseTime;
    float limitFactor = (float) (0.5 * (1.0 + (double) surgerySkill));
    this.AddCustomEffect((ICustomHealthEffect) new SurgeryEffect((float) Math.Round((double) medStats.HPRestoreTick * (1.0 + (double) surgerySkill), 2), new int?(), bodyPart, player, useTime, limitFactor, this), false);
  }

  private void HandleSplint(
    MedsItemClass meds,
    float regenTickRate,
    EBodyPart bodyPart,
    Player player)
  {
    if ((double) ((GInterface355) player.HealthController).GetBodyPartHealth(bodyPart, false).Current <= 0.0)
      return;
    if (PluginConfig.EnableMedNotes.Value)
      NotificationManagerClass.DisplayMessageNotification($"Fracture On {bodyPart.ToString()} Healed, Restoring HP.", (ENotificationDurationType) 1, (ENotificationIconType) 0, new Color?());
    int useTime = (int) meds.HealthEffectsComponent.UseTime;
    this.AddCustomEffect((ICustomHealthEffect) new HealthRegenEffect(regenTickRate, new int?(), bodyPart, player, useTime, 12f, (EDamageType) 64 /*0x40*/, this), false);
  }

  public void HandleHealthEffects(
    MedsItemClass meds,
    Consumable medStats,
    EBodyPart bodyPart,
    Player player,
    bool canHealHBleed,
    bool canHealLBleed,
    bool canHealFract)
  {
    float vitalitySkill = player.Skills.VitalityBuffBleedChanceRed.Value;
    float surgerySkill = player.Skills.SurgeryReducePenalty.Value;
    float regenTickRate = (float) Math.Round(0.40000000596046448 * (1.0 + (double) vitalitySkill), 2);
    bool hasHeavyBleed = false;
    bool hasLightBleed = false;
    bool hasFracture = false;
    this.GetInjuriesOnBodyPart(player, bodyPart, ref hasHeavyBleed, ref hasLightBleed, ref hasFracture);
    bool isHead = false;
    bool isBody = false;
    bool isNotLimb = false;
    this.GetBodyPartType(bodyPart, ref isNotLimb, ref isHead, ref isBody);
    if (PluginConfig.EnableTrnqtEffect.Value & hasHeavyBleed & canHealHBleed)
      this.HandleHeavyBleedHeal(meds, medStats, bodyPart, player, isNotLimb, vitalitySkill, regenTickRate);
    if (medStats.ConsumableType == EConsumableType.Surgical)
      this.HandleSurgery(meds, medStats, bodyPart, player, surgerySkill);
    if (canHealLBleed & hasLightBleed && !hasHeavyBleed && (medStats.ConsumableType == EConsumableType.Tourniquet && !isNotLimb || medStats.ConsumableType != EConsumableType.Tourniquet))
      this.HandleLightBleedHeal(meds, medStats, bodyPart, player, isNotLimb, vitalitySkill, regenTickRate);
    if (!(canHealFract & hasFracture) || medStats.ConsumableType != EConsumableType.Splint && (medStats.ConsumableType != EConsumableType.Medkit || hasHeavyBleed || hasLightBleed))
      return;
    this.HandleSplint(meds, regenTickRate, bodyPart, player);
  }

  private void AddPainkillerEffect(Player player, Item item, Consumable medStats)
  {
    int num = (int) ((double) medStats.Duration * (1.0 + (double) PlayerState.ImmuneSkillWeak));
    int delay = (int) Mathf.Round(medStats.Delay * (1f - player.Skills.HealthEnergy.Value));
    float tunnelStrength = medStats.TunnelVisionStrength * (1f - PlayerState.ImmuneSkillWeak);
    float strength = medStats.Strength;
    PainKillerEffect newEffect = new PainKillerEffect(new int?(num), player, delay, tunnelStrength, strength, this);
    Plugin.RealHealthController.AddCustomEffect((ICustomHealthEffect) newEffect, true);
  }

  private GClass1372 GetHazardTreatment(
    EDamageEffectType hazardType,
    HealthEffectsComponent healthEffectsComponent)
  {
    return healthEffectsComponent.DamageEffects.ContainsKey(hazardType) ? healthEffectsComponent.DamageEffects[hazardType] : (GClass1372) null;
  }

  private void ApplyDeHazardersOutOfRaid(GClass1372 details, EDamageEffectType type)
  {
    float fadeOut = details.FadeOut;
    int duration = (int) details.Duration;
    if (type == 4)
      HazardTracker.TotalToxicity -= fadeOut * (float) duration;
    if (type == 6)
    {
      if (HazardTracker.MedStationLevel >= 3)
      {
        HazardTracker.TotalRadiation -= fadeOut * 0.2f * (float) duration;
      }
      else
      {
        float num = (double) HazardTracker.TotalRadiation <= 40.0 ? 0.0f : (float) HazardTracker.GetNextLowestHazardLevel((int) HazardTracker.TotalRadiation);
        HazardTracker.TotalRadiation = Mathf.Clamp(HazardTracker.TotalRadiation - fadeOut * 0.2f * (float) duration, num, 100f);
      }
    }
    HazardTracker.UpdateHazardValues(ProfileData.PMCProfileId);
    HazardTracker.SaveHazardValues();
  }

  public bool TryHazardTreatmentOutOfRaid(MedsItemClass medClass)
  {
    GClass1372 hazardTreatment1 = this.GetHazardTreatment((EDamageEffectType) 4, medClass.HealthEffectsComponent);
    GClass1372 hazardTreatment2 = this.GetHazardTreatment((EDamageEffectType) 6, medClass.HealthEffectsComponent);
    bool flag1 = hazardTreatment1 != null && (double) HazardTracker.TotalToxicity > 0.0;
    bool flag2 = hazardTreatment2 != null && (double) HazardTracker.TotalRadiation > 0.0;
    if (flag1)
      this.ApplyDeHazardersOutOfRaid(hazardTreatment1, (EDamageEffectType) 4);
    if (flag2)
      this.ApplyDeHazardersOutOfRaid(hazardTreatment2, (EDamageEffectType) 6);
    return flag1 | flag2;
  }

  public bool TryReduceToxinInStashFood(Item item, bool isMed, HealthControllerClass hc)
  {
    if ((double) HazardTracker.TotalToxicity <= 0.0)
      return false;
    GClass1372 details = (GClass1372) null;
    MedsItemClass medsItemClass = item as MedsItemClass;
    FoodDrinkItemClass foodDrinkItemClass = item as FoodDrinkItemClass;
    if (isMed && medsItemClass != null)
      details = this.GetHazardTreatment((EDamageEffectType) 4, medsItemClass.HealthEffectsComponent);
    if (!isMed && foodDrinkItemClass != null)
      details = this.GetHazardTreatment((EDamageEffectType) 4, foodDrinkItemClass.HealthEffectsComponent);
    if (details == null)
      return false;
    this.ApplyDeHazardersOutOfRaid(details, (EDamageEffectType) 4);
    return true;
  }

  public void CheckIfReducesHazardInRaid(Item item, Player player, bool isMed)
  {
    GClass1372 gclass1372_1 = (GClass1372) null;
    GClass1372 gclass1372_2 = (GClass1372) null;
    if (isMed && item is MedsItemClass)
    {
      MedsItemClass medsItemClass = item as MedsItemClass;
      gclass1372_1 = medsItemClass.HealthEffectsComponent.DamageEffects.ContainsKey((EDamageEffectType) 4) ? medsItemClass.HealthEffectsComponent.DamageEffects[(EDamageEffectType) 4] : (GClass1372) null;
      gclass1372_2 = medsItemClass.HealthEffectsComponent.DamageEffects.ContainsKey((EDamageEffectType) 6) ? medsItemClass.HealthEffectsComponent.DamageEffects[(EDamageEffectType) 6] : (GClass1372) null;
    }
    if (!isMed && item is FoodDrinkItemClass)
    {
      FoodDrinkItemClass foodDrinkItemClass = item as FoodDrinkItemClass;
      gclass1372_1 = foodDrinkItemClass.HealthEffectsComponent.DamageEffects.ContainsKey((EDamageEffectType) 4) ? foodDrinkItemClass.HealthEffectsComponent.DamageEffects[(EDamageEffectType) 4] : (GClass1372) null;
      gclass1372_2 = foodDrinkItemClass.HealthEffectsComponent.DamageEffects.ContainsKey((EDamageEffectType) 6) ? foodDrinkItemClass.HealthEffectsComponent.DamageEffects[(EDamageEffectType) 6] : (GClass1372) null;
    }
    if (gclass1372_1 != null)
    {
      float rate = -gclass1372_1.FadeOut;
      int delay = (int) gclass1372_1.Delay;
      int duration = (int) gclass1372_1.Duration;
      DetoxificationEffect newEffect = new DetoxificationEffect(player, new int?(duration), delay, this, rate);
      Plugin.RealHealthController.AddCustomEffect((ICustomHealthEffect) newEffect, true);
    }
    if (gclass1372_2 == null)
      return;
    float rate1 = -gclass1372_2.FadeOut;
    int delay1 = (int) gclass1372_2.Delay;
    int duration1 = (int) gclass1372_2.Duration;
    RadationTreatmentEffect newEffect1 = new RadationTreatmentEffect(player, new int?(duration1), delay1, this, rate1);
    Plugin.RealHealthController.AddCustomEffect((ICustomHealthEffect) newEffect1, true);
  }

  private string GetHealBlockMessage(EHealBlockType blockType)
  {
    switch (blockType)
    {
      case EHealBlockType.Splint:
        return "Splints Can Only Fix Fractures On Limbs";
      case EHealBlockType.Trnqt:
        return "Tourniquets Can Only Stop Bleeds On Limbs";
      case EHealBlockType.Surgery:
        return "No Suitable Bodypart Was Found For Surgery Kit";
      case EHealBlockType.GearCommon:
        return "Gear Is Blocking Wound";
      case EHealBlockType.GearSpecific:
        return " Has Gear On, Remove Gear First To Be Able To Heal";
      case EHealBlockType.Unknown:
        return "No Suitable Bodypart Was Found For Healing";
      case EHealBlockType.Pills:
        return "Can't Take Pills, Mouth Is Blocked By Active Faceshield/NVGs Or Mask";
      case EHealBlockType.Food:
        return "Can't Eat/Drink, Mouth Is Blocked By Active Faceshield/NVGs Or Mask";
      case EHealBlockType.Gas:
        return "Can't Eat/Drink Due To Gas Exposure";
      default:
        return "No Suitable Bodypart Was Found For Healing";
    }
  }

  public bool ShouldAlwaysAllowOutOfRaid(Item item, Consumable medStats)
  {
    return (double) medStats.HPRestoreAmount > 0.0 || item is StimulatorItemClass || medStats.ConsumableType == EConsumableType.PainPills || medStats.ConsumableType == EConsumableType.Pills || medStats.ConsumableType == EConsumableType.Drug || medStats.ConsumableType == EConsumableType.PainDrug || medStats.ConsumableType == EConsumableType.Stimulator || medStats.ConsumableType == EConsumableType.Medkit || medStats.ConsumableType == EConsumableType.Surgical;
  }

  public bool CanUseInRaid(MedsItemClass meds, Consumable medStats)
  {
    if (medStats.CanBeUsedInRaid)
      return true;
    if (PluginConfig.EnableMedNotes.Value)
      NotificationManagerClass.DisplayWarningNotification("This Item Can Not Be Used In Raid", (ENotificationDurationType) 1);
    return false;
  }

  public (EBodyPart NewBodyPart, bool ShouldAllowHeal) ProcessSurgery(
    MedsItemClass meds,
    Consumable medStats,
    Player player,
    EBodyPart bodyPart,
    bool hasBodyGear,
    bool hasHeadGear)
  {
    bool isHead = false;
    bool isBody = false;
    bool isNotLimb = false;
    bodyPart = ((IEnumerable<EBodyPart>) Plugin.RealHealthController.BodyPartsArr).Where<EBodyPart>((Func<EBodyPart, bool>) (b => (double) ((GClass2814<ActiveHealthController.GClass2813>) player.ActiveHealthController).GetBodyPartHealth(b, false).Current / (double) ((GClass2814<ActiveHealthController.GClass2813>) player.ActiveHealthController).GetBodyPartHealth(b, false).Maximum < 1.0)).OrderBy<EBodyPart, float>((Func<EBodyPart, float>) (b => ((GClass2814<ActiveHealthController.GClass2813>) player.ActiveHealthController).GetBodyPartHealth(b, false).Current / ((GClass2814<ActiveHealthController.GClass2813>) player.ActiveHealthController).GetBodyPartHealth(b, false).Maximum)).FirstOrDefault<EBodyPart>();
    if (bodyPart == 7)
    {
      if (PluginConfig.EnableMedNotes.Value)
        NotificationManagerClass.DisplayWarningNotification(this.GetHealBlockMessage(EHealBlockType.Surgery), (ENotificationDurationType) 1);
      return (bodyPart, false);
    }
    Plugin.RealHealthController.GetBodyPartType(bodyPart, ref isNotLimb, ref isHead, ref isBody);
    if (PluginConfig.GearBlocksHeal.Value && (isBody & hasBodyGear || isHead & hasHeadGear))
    {
      if (PluginConfig.EnableMedNotes.Value)
        NotificationManagerClass.DisplayWarningNotification(this.GetHealBlockMessage(EHealBlockType.GearCommon), (ENotificationDurationType) 1);
      return (bodyPart, false);
    }
    Plugin.RealHealthController.HandleHealthEffects(meds, medStats, bodyPart, player, true, true, false);
    return (bodyPart, true);
  }

  public (EBodyPart NewBodyPart, bool ShouldAllowHeal) ProcessHealAttempt(
    MedsItemClass meds,
    Player player,
    EBodyPart bodyPart)
  {
    Consumable dataObj = TemplateStats.GetDataObj<Consumable>(TemplateStats.ConsumableStats, MongoID.op_Implicit(((Item) meds).TemplateId));
    if (!this.CanUseInRaid(meds, dataObj))
      return (bodyPart, false);
    this.CheckIfReducesHazardInRaid((Item) meds, player, true);
    MongoID? parentId = ((Item) meds).Template.ParentId;
    MongoID? nullable = MongoID.op_Implicit("5448f3a64bdc2d60728b456a");
    if (parentId.HasValue != nullable.HasValue || parentId.HasValue && !MongoID.op_Equality(parentId.GetValueOrDefault(), nullable.GetValueOrDefault()))
      return this.CanUseMedItemBodyPart(bodyPart, player, dataObj, meds);
    int num = (int) ((StimulatorBuffsComponent) meds.HealthEffectsComponent).BuffSettings[0].Duration * 2;
    int delay = Mathf.Max((int) ((StimulatorBuffsComponent) meds.HealthEffectsComponent).BuffSettings[0].Delay, 5);
    EStimType stimType = Plugin.RealHealthController.GetStimType(MongoID.op_Implicit(((Item) meds).Template._id));
    StimEffectShell newEffect = new StimEffectShell(player, new int?(num), delay, stimType, this);
    Plugin.RealHealthController.AddCustomEffect((ICustomHealthEffect) newEffect, true);
    return (bodyPart, true);
  }

  public (EBodyPart NewBodyPart, bool ShouldAllowHeal) CanUseMedItemBodyPart(
    EBodyPart bodyPart,
    Player player,
    Consumable medStats,
    MedsItemClass meds)
  {
    bool canHealLBleed = meds.HealthEffectsComponent.DamageEffects.ContainsKey((EDamageEffectType) 1) && (double) (meds.HealthEffectsComponent.DamageEffects[(EDamageEffectType) 1].Cost + 1) <= (double) meds.MedKitComponent.HpResource;
    bool canHealHBleed = meds.HealthEffectsComponent.DamageEffects.ContainsKey((EDamageEffectType) 0) && (double) (meds.HealthEffectsComponent.DamageEffects[(EDamageEffectType) 0].Cost + 1) <= (double) meds.MedKitComponent.HpResource;
    bool canHealFract = meds.HealthEffectsComponent.DamageEffects.ContainsKey((EDamageEffectType) 2) && (double) (meds.HealthEffectsComponent.DamageEffects[(EDamageEffectType) 2].Cost + 1) <= (double) meds.MedKitComponent.HpResource;
    if (bodyPart == 7)
    {
      int num1 = 0;
      InventoryEquipment equipment = (InventoryEquipment) AccessTools.Property(typeof (Player), "Equipment").GetValue((object) player);
      Item containedItem1 = equipment.GetSlot((EquipmentSlot) 11).ContainedItem;
      Item containedItem2 = equipment.GetSlot((EquipmentSlot) 12).ContainedItem;
      Item containedItem3 = equipment.GetSlot((EquipmentSlot) 9).ContainedItem;
      Item containedItem4 = equipment.GetSlot((EquipmentSlot) 10).ContainedItem;
      Item containedItem5 = equipment.GetSlot((EquipmentSlot) 7).ContainedItem;
      Item containedItem6 = equipment.GetSlot((EquipmentSlot) 6).ContainedItem;
      Item containedItem7 = equipment.GetSlot((EquipmentSlot) 4).ContainedItem;
      bool flag1 = this.MouthIsBlocked(player, containedItem1, containedItem4, equipment);
      bool hasBodyGear = containedItem5 != null || containedItem6 != null;
      bool hasHeadGear = containedItem1 != null || containedItem2 != null || containedItem4 != null;
      EHealBlockType blockType = EHealBlockType.Unknown;
      bool flag2 = medStats.ConsumableType == EConsumableType.Pills || medStats.ConsumableType == EConsumableType.PainPills;
      bool flag3 = medStats.ConsumableType == EConsumableType.PainDrug || medStats.ConsumableType == EConsumableType.Drug;
      if (PluginConfig.GearBlocksHeal.Value & flag2 & flag1)
      {
        if (PluginConfig.EnableMedNotes.Value)
          NotificationManagerClass.DisplayWarningNotification(this.GetHealBlockMessage(EHealBlockType.Pills), (ENotificationDurationType) 1);
        return (bodyPart, false);
      }
      if (medStats.ConsumableType == EConsumableType.PainPills || medStats.ConsumableType == EConsumableType.PainDrug)
      {
        this.AddPainkillerEffect(player, (Item) meds, medStats);
        return (bodyPart, true);
      }
      if (flag3 | flag2)
        return (bodyPart, true);
      Type type1;
      MedProperties.EffectTypes.TryGetValue("HeavyBleeding", out type1);
      Type type2;
      MedProperties.EffectTypes.TryGetValue("LightBleeding", out type2);
      Type type3;
      MedProperties.EffectTypes.TryGetValue("BrokenBone", out type3);
      if (medStats.ConsumableType == EConsumableType.Surgical)
        return this.ProcessSurgery(meds, medStats, player, bodyPart, hasBodyGear, hasHeadGear);
      foreach (int num2 in Plugin.RealHealthController.BodyPartsArr)
      {
        EBodyPart part = (EBodyPart) num2;
        bool isHead = false;
        bool isBody = false;
        bool isNotLimb = false;
        Plugin.RealHealthController.GetBodyPartType(part, ref isNotLimb, ref isHead, ref isBody);
        bool hasHeavyBleed = false;
        bool hasLightBleed = false;
        bool hasFracture = false;
        IEnumerable<IEffect> injuriesOnBodyPart = Plugin.RealHealthController.GetInjuriesOnBodyPart(player, part, ref hasHeavyBleed, ref hasLightBleed, ref hasFracture);
        float current = ((GClass2814<ActiveHealthController.GClass2813>) player.ActiveHealthController).GetBodyPartHealth(part, false).Current;
        float maximum = ((GClass2814<ActiveHealthController.GClass2813>) player.ActiveHealthController).GetBodyPartHealth(part, false).Maximum;
        foreach (IEffect ieffect in injuriesOnBodyPart)
        {
          if (PluginConfig.GearBlocksHeal.Value && (isBody & hasBodyGear || isHead & hasHeadGear))
            ++num1;
          else if (canHealHBleed && ieffect.Type == type1)
          {
            if (!isNotLimb)
            {
              bodyPart = part;
              break;
            }
            if (isBody | isHead && medStats.HeavyBleedHealType == EHeavyBleedHealType.Tourniquet)
            {
              blockType = EHealBlockType.Trnqt;
            }
            else
            {
              bodyPart = !(isBody | isHead) || medStats.HeavyBleedHealType != EHeavyBleedHealType.Clot && medStats.HeavyBleedHealType != EHeavyBleedHealType.Combo && medStats.HeavyBleedHealType != EHeavyBleedHealType.Surgical ? part : part;
              break;
            }
          }
          else if (canHealLBleed && ieffect.Type == type2)
          {
            if (!isNotLimb)
            {
              bodyPart = part;
              break;
            }
            if (isBody | isHead && medStats.HeavyBleedHealType == EHeavyBleedHealType.Tourniquet)
              blockType = EHealBlockType.Trnqt;
            else if (!((isBody | isHead) & hasHeavyBleed))
            {
              bodyPart = part;
              break;
            }
          }
          else if (canHealFract && ieffect.Type == type3)
          {
            if (!isNotLimb)
            {
              bodyPart = part;
              break;
            }
            if (isNotLimb)
            {
              blockType = EHealBlockType.Splint;
            }
            else
            {
              bodyPart = part;
              break;
            }
          }
        }
        if (bodyPart != 7)
          break;
      }
      if (bodyPart == 7)
      {
        if (medStats.ConsumableType == EConsumableType.Vaseline)
          return (bodyPart, true);
        if (blockType == EHealBlockType.Unknown && num1 > 0)
          blockType = EHealBlockType.GearCommon;
        if (PluginConfig.EnableMedNotes.Value)
          NotificationManagerClass.DisplayWarningNotification(this.GetHealBlockMessage(blockType), (ENotificationDurationType) 1);
        return (bodyPart, false);
      }
    }
    if (bodyPart != 7)
      Plugin.RealHealthController.HandleHealthEffects(meds, medStats, bodyPart, player, canHealHBleed, canHealLBleed, canHealFract);
    return (bodyPart, true);
  }

  public void CanUseMedItem(Player player, EBodyPart bodyPart, Item item, ref bool canUse)
  {
    Consumable dataObj = TemplateStats.GetDataObj<Consumable>(TemplateStats.ConsumableStats, MongoID.op_Implicit(item.TemplateId));
    if (MongoID.op_Equality(item.Template.Parent._id, MongoID.op_Implicit("5448f3a64bdc2d60728b456a")) || dataObj.ConsumableType == EConsumableType.Drug)
      return;
    if (!dataObj.CanBeUsedInRaid)
    {
      if (PluginConfig.EnableMedNotes.Value)
        NotificationManagerClass.DisplayWarningNotification("This Item Can Not Be Used In Raid", (ENotificationDurationType) 1);
      canUse = false;
    }
    else
    {
      MedsItemClass medsItemClass = item as MedsItemClass;
      InventoryEquipment equipment = player.Equipment;
      Item containedItem1 = equipment.GetSlot((EquipmentSlot) 11).ContainedItem;
      Item containedItem2 = equipment.GetSlot((EquipmentSlot) 12).ContainedItem;
      Item containedItem3 = equipment.GetSlot((EquipmentSlot) 9).ContainedItem;
      Item containedItem4 = equipment.GetSlot((EquipmentSlot) 10).ContainedItem;
      Item containedItem5 = equipment.GetSlot((EquipmentSlot) 7).ContainedItem;
      Item containedItem6 = equipment.GetSlot((EquipmentSlot) 6).ContainedItem;
      Item containedItem7 = equipment.GetSlot((EquipmentSlot) 4).ContainedItem;
      bool flag1 = this.MouthIsBlocked(player, containedItem1, containedItem4, equipment);
      bool flag2 = containedItem1 != null || containedItem2 != null || containedItem4 != null;
      bool flag3 = containedItem5 != null || containedItem6 != null;
      bool isHead = false;
      bool isBody = false;
      bool isNotLimb = false;
      this.GetBodyPartType(bodyPart, ref isNotLimb, ref isHead, ref isBody);
      float hpResource = medsItemClass.MedKitComponent.HpResource;
      if (dataObj.ConsumableType == EConsumableType.Vaseline)
        return;
      if (dataObj.ConsumableType == EConsumableType.Pills)
      {
        if (!(PluginConfig.GearBlocksEat.Value & flag1))
          return;
        if (PluginConfig.EnableMedNotes.Value)
          NotificationManagerClass.DisplayWarningNotification(this.GetHealBlockMessage(EHealBlockType.Pills), (ENotificationDurationType) 1);
        canUse = false;
      }
      else if (PluginConfig.GearBlocksHeal.Value && (isBody & flag3 || isHead & flag2))
      {
        if (PluginConfig.EnableMedNotes.Value)
          NotificationManagerClass.DisplayWarningNotification(bodyPart.ToString() + this.GetHealBlockMessage(EHealBlockType.GearSpecific), (ENotificationDurationType) 1);
        canUse = false;
      }
      else
      {
        bool hasHeavyBleed = false;
        bool hasLightBleed = false;
        bool hasFracture = false;
        this.GetInjuriesOnBodyPart(player, bodyPart, ref hasHeavyBleed, ref hasLightBleed, ref hasFracture);
        bool flag4 = ((!medsItemClass.HealthEffectsComponent.DamageEffects.ContainsKey((EDamageEffectType) 1) ? 0 : ((double) (medsItemClass.HealthEffectsComponent.DamageEffects[(EDamageEffectType) 1].Cost + 1) <= (double) medsItemClass.MedKitComponent.HpResource ? 1 : 0)) & (hasLightBleed ? 1 : 0)) != 0 | ((!medsItemClass.HealthEffectsComponent.DamageEffects.ContainsKey((EDamageEffectType) 0) ? 0 : ((double) (medsItemClass.HealthEffectsComponent.DamageEffects[(EDamageEffectType) 0].Cost + 1) <= (double) medsItemClass.MedKitComponent.HpResource ? 1 : 0)) & (hasHeavyBleed ? 1 : 0)) != 0 | ((!medsItemClass.HealthEffectsComponent.DamageEffects.ContainsKey((EDamageEffectType) 2) ? 0 : ((double) (medsItemClass.HealthEffectsComponent.DamageEffects[(EDamageEffectType) 2].Cost + 1) <= (double) medsItemClass.MedKitComponent.HpResource ? 1 : 0)) & (hasFracture ? 1 : 0)) != 0;
        if (dataObj.ConsumableType == EConsumableType.Medkit && !flag4)
          canUse = false;
        else if (isNotLimb && dataObj.HeavyBleedHealType == EHeavyBleedHealType.Tourniquet)
        {
          if (PluginConfig.EnableMedNotes.Value)
            NotificationManagerClass.DisplayWarningNotification(this.GetHealBlockMessage(EHealBlockType.Trnqt), (ENotificationDurationType) 1);
          canUse = false;
        }
        else if (((dataObj.ConsumableType != EConsumableType.Splint ? 0 : (medsItemClass.HealthEffectsComponent.DamageEffects.ContainsKey((EDamageEffectType) 2) ? 1 : 0)) & (isNotLimb ? 1 : 0)) != 0)
        {
          if (PluginConfig.EnableMedNotes.Value)
            NotificationManagerClass.DisplayWarningNotification(this.GetHealBlockMessage(EHealBlockType.Splint), (ENotificationDurationType) 1);
          canUse = false;
        }
        else
        {
          if (((dataObj.ConsumableType != EConsumableType.Medkit ? 0 : (medsItemClass.HealthEffectsComponent.DamageEffects.ContainsKey((EDamageEffectType) 2) ? 1 : 0)) & (hasFracture ? 1 : 0) & (isNotLimb ? 1 : 0)) == 0 || hasHeavyBleed || hasLightBleed)
            return;
          NotificationManagerClass.DisplayWarningNotification(this.GetHealBlockMessage(EHealBlockType.Splint), (ENotificationDurationType) 1);
          canUse = false;
        }
      }
    }
  }

  public void DoubleBleedCheck(Player player)
  {
    IEnumerable<IEffect> allActiveEffects1 = ((GClass2814<ActiveHealthController.GClass2813>) player.ActiveHealthController).GetAllActiveEffects((EBodyPart) 7);
    Type heavyBleedType;
    MedProperties.EffectTypes.TryGetValue("HeavyBleeding", out heavyBleedType);
    Type lightBleedType;
    MedProperties.EffectTypes.TryGetValue("LightBleeding", out lightBleedType);
    if (!((heavyBleedType != (Type) null && allActiveEffects1.Any<IEffect>((Func<IEffect, bool>) (e => e.Type == heavyBleedType))) & (lightBleedType != (Type) null && allActiveEffects1.Any<IEffect>((Func<IEffect, bool>) (e => e.Type == lightBleedType)))))
      return;
    IReadOnlyList<ActiveHealthController.GClass2813> ireadOnlyList0 = ((GClass2814<ActiveHealthController.GClass2813>) player.ActiveHealthController).IReadOnlyList_0;
    for (int index = ireadOnlyList0.Count - 1; index >= 0; --index)
    {
      ActiveHealthController.GClass2813 gclass2813 = ireadOnlyList0[index];
      Type type = gclass2813.Type;
      EBodyPart bodyPart = gclass2813.BodyPart;
      IEnumerable<IEffect> allActiveEffects2 = ((GClass2814<ActiveHealthController.GClass2813>) player.ActiveHealthController).GetAllActiveEffects(bodyPart);
      if ((heavyBleedType != (Type) null && allActiveEffects2.Any<IEffect>((Func<IEffect, bool>) (e => e.Type == heavyBleedType))) & (lightBleedType != (Type) null && allActiveEffects2.Any<IEffect>((Func<IEffect, bool>) (e => e.Type == lightBleedType))) && type == lightBleedType)
        gclass2813.ForceResidue();
    }
  }

  private void ResetHealhPenalties()
  {
    PlayerState.AimMoveSpeedInjuryMulti = 1f;
    PlayerState.ADSInjuryMulti = 1f;
    PlayerState.StanceInjuryMulti = 1f;
    PlayerState.ReloadInjuryMulti = 1f;
    PlayerState.HealthSprintSpeedFactor = 1f;
    PlayerState.HealthSprintAccelFactor = 1f;
    PlayerState.HealthWalkSpeedFactor = 1f;
    PlayerState.HealthStamRegenFactor = 1f;
    PlayerState.ErgoDeltaInjuryMulti = 1f;
    PlayerState.RecoilInjuryMulti = 1f;
  }

  private void DoResourceDrain(ActiveHealthController hc, float dt)
  {
    hc.ChangeEnergy(-this.ResourcePerTick * dt * PluginConfig.EnergyRateMulti.Value);
    hc.ChangeHydration(-this.ResourcePerTick * dt * PluginConfig.HydrationRateMulti.Value);
  }

  private float CalculateHPPenalty(float basePercent, float divisor)
  {
    return (float) (1.0 - (1.0 - (double) basePercent) / (double) divisor);
  }

  public void PlayerInjuryStateCheck(Player player)
  {
    player.MovementContext.PhysicalConditionIs((EPhysicalCondition) 32 /*0x20*/);
    player.MovementContext.PhysicalConditionIs((EPhysicalCondition) 16 /*0x10*/);
    float num1 = player.MovementContext.PhysicalConditionIs((EPhysicalCondition) 64 /*0x40*/) ? 0.95f : 1f;
    float num2 = 1f;
    float num3 = 1f;
    float num4 = 1f;
    float num5 = 1f;
    float num6 = 1f;
    float num7 = 1f;
    float num8 = 1f;
    float num9 = 1f;
    float num10 = 1f;
    float num11 = 1f;
    float num12 = (this._hasOverdosedStim ? 90f + this.PainReliefStrength : this.PainReliefStrength) / 200f;
    float num13 = ((GClass2814<ActiveHealthController.GClass2813>) player.ActiveHealthController).Energy.Current / ((GClass2814<ActiveHealthController.GClass2813>) player.ActiveHealthController).Energy.Maximum;
    float num14 = ((GClass2814<ActiveHealthController.GClass2813>) player.ActiveHealthController).Hydration.Current / ((GClass2814<ActiveHealthController.GClass2813>) player.ActiveHealthController).Hydration.Maximum;
    this._percentReources = Mathf.Clamp01((float) (((double) num13 + (double) num14) / 2.0));
    this.IsPoisoned = this.HasBaseEFTEffect(player, "LethalToxin");
    float num15 = 0.0f;
    float num16 = 0.0f;
    this._painInjuryStrength = 0.0f;
    Type fractureType;
    MedProperties.EffectTypes.TryGetValue("BrokenBone", out fractureType);
    foreach (int num17 in this.BodyPartsArr)
    {
      EBodyPart ebodyPart = (EBodyPart) num17;
      IEnumerable<IEffect> allActiveEffects = ((GClass2814<ActiveHealthController.GClass2813>) player.ActiveHealthController).GetAllActiveEffects(ebodyPart);
      bool flag1 = fractureType != (Type) null && allActiveEffects.Any<IEffect>((Func<IEffect, bool>) (e => e.Type == fractureType));
      float num18 = 1f - PlayerState.StressResistanceFactor;
      if (flag1)
        this._painInjuryStrength += this.FracturePainFactor;
      bool flag2 = ebodyPart == 3;
      bool flag3 = ebodyPart == 4;
      bool flag4 = flag2 | flag3;
      bool flag5 = ebodyPart == 5 || ebodyPart == 6;
      bool flag6 = ebodyPart == 1 || ebodyPart == 2;
      float current = ((GClass2814<ActiveHealthController.GClass2813>) player.ActiveHealthController).GetBodyPartHealth(ebodyPart, false).Current;
      float maximum = ((GClass2814<ActiveHealthController.GClass2813>) player.ActiveHealthController).GetBodyPartHealth(ebodyPart, false).Maximum;
      num15 += maximum;
      num16 += current;
      float basePercent = current / maximum;
      float hpPenalty1 = this.CalculateHPPenalty(basePercent, flag6 ? 10f : 5f);
      float hpPenalty2 = this.CalculateHPPenalty(basePercent, flag6 ? 15f : 7.5f);
      float hpPenalty3 = this.CalculateHPPenalty(basePercent, flag6 ? 8f : 4f);
      float hpPenalty4 = this.CalculateHPPenalty(basePercent, flag4 ? 20f : 14f);
      float hpPenalty5 = this.CalculateHPPenalty(basePercent, flag2 ? 2.5f : (flag3 ? 1.5f : 3f));
      float hpPenalty6 = this.CalculateHPPenalty(basePercent, flag2 ? 2.5f : (flag3 ? 1.5f : 3.5f));
      float hpPenalty7 = this.CalculateHPPenalty(basePercent, flag2 ? 3f : (flag3 ? 4f : 5f));
      float hpPenalty8 = this.CalculateHPPenalty(basePercent, flag2 ? 10f : 20f);
      if ((double) current <= 0.0)
        this._painInjuryStrength += this.ZeroedPainFactor;
      else if ((double) basePercent <= (double) this.HpLossPainThreshold)
        this._painInjuryStrength += this.HpLossPainFactor;
      if (flag5 | flag6)
      {
        num2 *= hpPenalty4;
        num8 *= hpPenalty3;
        num9 *= basePercent;
        num10 *= hpPenalty2;
        num11 *= hpPenalty1;
      }
      if (flag4)
      {
        bool flag7 = (double) current <= 0.0 | flag1 && !this.HasBaseEFTEffect(player, "PainKiller");
        if (flag2)
          this._leftArmRuined = flag7;
        if (flag3)
          this._rightArmRuined = flag7;
        float num19 = flag2 & flag1 ? 0.75f : (flag3 & flag1 ? 0.85f : 1f);
        num2 *= hpPenalty4 * num19;
        num4 *= hpPenalty5 * num19;
        num5 *= hpPenalty6 * num19;
        num6 *= hpPenalty7 * num19;
        num3 *= (float) (1.0 + (1.0 - (double) basePercent)) * num19;
        num7 *= (float) (1.0 + (1.0 - (double) hpPenalty8)) * num19;
      }
    }
    float num20 = Mathf.Clamp(1f - num16 / num15, 0.0f, 1f) * 0.25f;
    float num21 = Mathf.Max(num13 * 1.1f, 0.01f);
    float num22 = (float) (1.0 - (1.0 - (double) num21) / 8.0);
    float num23 = (float) (1.0 - (1.0 - (double) num21) / 12.0);
    float num24 = (float) (1.0 - (1.0 - (double) num21) / 20.0);
    float num25 = (float) (1.0 - (1.0 - (double) num21) / 5.0);
    float num26 = (float) (1.0 - (1.0 - (double) num21) / 2.0);
    float num27 = (float) (1.0 - (1.0 - (double) num21) / 10.0);
    float num28 = (float) (1.0 - (1.0 - (double) num21) / 40.0);
    float num29 = (float) (1.0 - (1.0 - (double) num21) / 2.0);
    float num30 = (float) (1.0 - (1.0 - (double) num21) / 10.0);
    float num31 = (float) (1.0 - (1.0 - (double) num14) / 4.0);
    float num32 = (float) (1.0 + (1.0 - (double) num14) / 20.0);
    float num33 = (float) (1.0 + (1.0 - (double) num14) / 4.0);
    float num34 = Mathf.Max(this._painInjuryStrength - this.PainReliefStrength, 0.0f);
    float num35 = this._hasOverdosedStim ? 90f + num34 : num34;
    float num36 = Mathf.Clamp((float) (1.0 - (double) num35 / 1000.0), 0.85f, 1f);
    float num37 = Mathf.Clamp((float) (1.0 + (double) num35 / 1000.0), 1f, 1.15f);
    float num38 = player.Skills.HealthEnergy.Value / 4f;
    float num39 = 1f + num38;
    float num40 = 1f - num38;
    float num41 = (float) (((double) HazardTracker.TotalToxicity + (this.IsCoughingInGas ? 100.0 : 0.0)) * (1.0 - (double) PlayerState.ImmuneSkillWeak) / 225.0);
    float num42 = 1f - num41;
    float num43 = 1f + num41;
    float num44 = (float) ((double) HazardTracker.TotalRadiation * (1.0 - (double) PlayerState.ImmuneSkillWeak) / 300.0);
    float num45 = 1f - num44;
    float num46 = 1f + num44;
    float num47 = this.IsPoisoned ? 0.8f : 1f;
    float num48 = this.IsPoisoned ? 1.2f : 1f;
    float num49 = num42 * num45 * num47;
    float num50 = num43 * num46 * num48;
    PlayerState.AimMoveSpeedInjuryMulti = Mathf.Clamp(num2 * num24 * num36 * num39 * num49, 0.6f * num31, 1f);
    PlayerState.ADSInjuryMulti = Mathf.Clamp(num4 * num25 * num36 * num39 * num49, 0.3f * num31, 1f);
    PlayerState.StanceInjuryMulti = Mathf.Clamp(num5 * num26 * num36 * num39 * num49, 0.65f * num31, 1f);
    PlayerState.ReloadInjuryMulti = Mathf.Clamp(num6 * num27 * num36 * num39 * num49, 0.75f * num31, 1f);
    PlayerState.HealthSprintSpeedFactor = Mathf.Clamp(num8 * num22 * num36 * num39 * num49, 0.4f * num31, 1f);
    PlayerState.HealthSprintAccelFactor = Mathf.Clamp(num9 * num22 * num36 * num39 * num49, 0.4f * num31, 1f);
    PlayerState.HealthWalkSpeedFactor = Mathf.Clamp(num10 * num23 * num36 * num39 * num49, 0.6f * num31, 1f);
    PlayerState.HealthStamRegenFactor = Mathf.Clamp(num11 * num30 * num36 * num39 * num49, 0.5f * num31, 1f);
    PlayerState.ErgoDeltaInjuryMulti = Mathf.Clamp(num3 * (float) (1.0 + (1.0 - (double) num29)) * num37 * num40 * num50, 1f, 1.3f * num33);
    PlayerState.RecoilInjuryMulti = Mathf.Clamp(num7 * (float) (1.0 + (1.0 - (double) num28)) * num37 * num40 * num50, 1f, 1.12f * num32);
    if (!PluginConfig.ResourceRateChanges.Value)
      return;
    if (!this.HasCustomEffectOfType(typeof (ResourceRateEffect), (EBodyPart) 1))
      this.AddCustomEffect((ICustomHealthEffect) new ResourceRateEffect(new int?(), player, 0, this), false);
    float num51 = PlayerState.TotalModifiedWeight * (1f - player.Skills.EnduranceBuffJumpCostRed.Value);
    float num52 = (double) (PlayerState.TotalModifiedWeight * (1f + player.Skills.EnduranceBuffJumpCostRed.Value)) >= 10.0 ? num51 / 500f : 0.0f;
    float num53 = PlayerState.IsSprinting ? 1.46f : 1f;
    float num54 = PlayerState.IsSprinting ? 0.11f : 0.0f;
    float num55 = this.IsPoisoned ? Mathf.Max((float) (1.5 * (1.0 - (double) PlayerState.ImmuneSkillWeak)), 1.1f) : 1f;
    float num56 = (float) (1.0 + (double) HazardTracker.TotalToxicity * (1.0 - (double) PlayerState.ImmuneSkillWeak) / 150.0);
    float num57 = (float) (1.0 + (double) HazardTracker.TotalRadiation * (1.0 - (double) PlayerState.ImmuneSkillWeak) / 190.0);
    this.ResourcePerTick = (float) (((double) num20 + (double) num12 + (double) num54 + (double) num52) * (double) num53 * (double) num56 * (double) num57 * (double) num55 * (1.0 - (double) player.Skills.HealthEnergy.Value));
  }

  private void HazardZoneHealthTick(Player player)
  {
    if (!GameWorldController.GameStarted || PluginConfig.DevMode.Value)
      return;
    if (Object.op_Equality((Object) this.PlayerHazardBridge, (Object) null))
      this.PlayerHazardBridge = ((Component) player).gameObject.GetComponent<PlayerZoneBridge>();
    if (!player.HealthController.IsAlive || (double) player.HealthController.DamageCoeff <= 0.0)
      return;
    if (GearController.HasGasMask && !this.PlayerHazardBridge.IsProtectedFromSafeZone && (this.PlayerHazardBridge.GasZoneCount > 0 || this.PlayerHazardBridge.RadZoneCount > 0 || this.ToxicItemCount > 0 || this.RadItemCount > 0 || GameWorldController.DoMapGasEvent || GameWorldController.DoMapRads))
    {
      GearController.UpdateFilterResource(player, this.PlayerHazardBridge);
      GearController.CalcGasMaskDuraFactor(player);
    }
    this.GasZoneTick(player);
    this.RadiationZoneTick(player);
    this.HazardEffectsTick(player);
    this.CoughController(player);
  }

  private void RadiationZoneTick(Player player)
  {
    bool flag1 = this.PlayerHazardBridge.RadZoneCount > 0 && !this.PlayerHazardBridge.IsProtectedFromSafeZone;
    bool flag2 = (flag1 || GameWorldController.DoMapRads) && (double) GearController.CurrentRadProtection <= 0.0;
    float num1 = PlayerState.IsSprinting ? 1.1f : 1f;
    float num2 = (float) this.RadItemCount * 0.15f;
    float num3 = GameWorldController.DoMapRads ? GameWorldController.CurrentMapRadStrength : 0.0f;
    float num4 = (float) ((1.0 - (double) GearController.CurrentRadProtection) * (1.0 - (double) PlayerState.ImmuneSkillWeak));
    float num5 = !flag2 ? HazardTracker.RadTreatmentRate : 0.0f;
    float num6 = flag1 ? num5 * 0.5f : num5;
    float num7 = this.PlayerHazardBridge.TotalRadRate + num2 + num3;
    float num8 = (this.PlayerHazardBridge.TotalRadRate * num1 + num2 + num3) * num4 + num6;
    float num9 = (double) num7 > 0.0 ? 10f : (this.PlayerHazardBridge.IsProtectedFromSafeZone ? 8f : 6f);
    float num10 = (double) num8 > 0.0 ? 10f : (this.PlayerHazardBridge.IsProtectedFromSafeZone ? 6f : 2f);
    HazardTracker.BaseTotalRadiationRate = Mathf.MoveTowards(HazardTracker.BaseTotalRadiationRate, num7, num9 * Time.deltaTime);
    HazardTracker.TotalRadiationRate = Mathf.MoveTowards(HazardTracker.TotalRadiationRate, num8, num10 * Time.deltaTime);
    HazardTracker.TotalRadiation = Mathf.Clamp(HazardTracker.TotalRadiation + HazardTracker.TotalRadiationRate, !flag1 || (double) GearController.CurrentRadProtection >= 1.0 ? (flag1 || (double) HazardTracker.TotalRadiation > 40.0 ? (float) HazardTracker.GetNextLowestHazardLevel((int) HazardTracker.TotalRadiation) : 0.0f) : HazardTracker.TotalRadiation, 100f);
  }

  private void GasZoneTick(Player player)
  {
    bool flag1 = this.PlayerHazardBridge.GasZoneCount > 0 && (double) this.PlayerHazardBridge.TotalGasRate > 0.0099999997764825821 && !this.PlayerHazardBridge.IsProtectedFromSafeZone;
    bool flag2 = flag1 && (double) GearController.CurrentGasProtection <= 0.0 || this.IsCoughingInGas;
    float num1 = PlayerState.IsSprinting ? 1.5f : 1f;
    float num2 = (float) this.ToxicItemCount * 0.05f;
    float num3 = GameWorldController.DoMapGasEvent ? GameWorldController.CurrentGasEventStrength : 0.0f;
    float num4 = (float) ((1.0 - (double) GearController.CurrentGasProtection) * (1.0 - (double) PlayerState.ImmuneSkillWeak));
    float num5 = (double) num3 > 0.0 || this.ToxicItemCount > 0 || flag1 || (double) HazardTracker.TotalToxicity <= 0.0 ? 0.0f : (float) (-0.05000000074505806 * (2.0 - (double) this._percentReources));
    float num6 = !flag2 ? HazardTracker.DetoxicationRate + num5 : 0.0f;
    float num7 = flag1 ? num6 * 0.5f : num6;
    float num8 = this.PlayerHazardBridge.TotalGasRate + num2 + num3;
    float num9 = (this.PlayerHazardBridge.TotalGasRate * num1 + num2 + num3) * num4 + num7;
    float num10 = (double) num8 > 0.0 ? 11f : (this.PlayerHazardBridge.IsProtectedFromSafeZone ? 7f : 5f);
    float num11 = (double) num9 > 0.0 ? 11f : (this.PlayerHazardBridge.IsProtectedFromSafeZone ? 6f : 1.5f);
    HazardTracker.BaseTotalToxicityRate = Mathf.MoveTowards(HazardTracker.BaseTotalToxicityRate, num8, num10 * Time.deltaTime);
    HazardTracker.TotalToxicityRate = Mathf.MoveTowards(HazardTracker.TotalToxicityRate, num9, num11 * Time.deltaTime);
    HazardTracker.TotalToxicity = Mathf.Clamp(HazardTracker.TotalToxicity + HazardTracker.TotalToxicityRate, !flag1 || (double) GearController.CurrentGasProtection >= 1.0 ? (flag2 || (double) HazardTracker.DetoxicationRate >= 0.0 ? (float) HazardTracker.GetNextLowestHazardLevel((int) HazardTracker.TotalToxicity) : 0.0f) : HazardTracker.TotalToxicity, 100f);
  }

  private void HazardEffectsTick(Player player)
  {
    if ((double) this._hazardWaitTime <= 10.0)
      return;
    if ((double) HazardTracker.TotalToxicity >= 10.0 || this.IsCoughingInGas)
    {
      if (((double) HazardTracker.TotalToxicity >= 15.0 || this.IsCoughingInGas) && !this.HasCustomEffectOfType(typeof (ToxicityEffect), (EBodyPart) 1))
        this.AddCustomEffect((ICustomHealthEffect) new ToxicityEffect(new int?(), player, 0, this), false);
      float num1 = HazardTracker.TotalToxicity / 100f;
      float num2 = this.IsCoughingInGas ? 1f : 0.0f;
      if ((double) HazardTracker.TotalToxicity >= 15.0 || this.IsCoughingInGas)
        this.AddToExistingBaseEFTEffect(player, "Contusion", (EBodyPart) 0, 1f, new float?(10f), 5f, num1 * 0.7f + num2);
    }
    if ((double) HazardTracker.TotalRadiation >= 10.0)
    {
      if ((double) HazardTracker.TotalRadiation >= 30.0 && !this.HasCustomEffectOfType(typeof (ToxicityEffect), (EBodyPart) 1))
        this.AddCustomEffect((ICustomHealthEffect) new RadiationEffect(new int?(), player, 0, this), false);
      float num = HazardTracker.TotalRadiation / 100f;
      this.AddBasesEFTEffect(player, "TunnelVision", (EBodyPart) 0, new float?(1f), new float?(10f), new float?(5f), new float?(Mathf.Min(num, 1f)));
    }
    this._hazardWaitTime = 0.0f;
  }

  private void InventoryCheckerHelper(IEnumerable<Item> items, bool areQuestItems = false)
  {
    if (areQuestItems && GearController.HasSafeContainer)
      return;
    foreach (Item obj in items)
    {
      int num;
      if (obj != null)
      {
        if (obj == null)
        {
          num = 1;
        }
        else
        {
          MongoID templateId = obj.TemplateId;
          num = 0;
        }
      }
      else
        num = 1;
      if (num == 0)
      {
        if (MongoID.op_Equality(obj.TemplateId, MongoID.op_Implicit("66fd588d397ed74159826cf0")))
          GearController.HasSafeContainer = true;
        if (obj?.Parent == null || obj?.Parent?.Container == null || !MongoID.op_Equality(obj.Parent.Container.ParentItem.TemplateId, MongoID.op_Implicit("66fd588d397ed74159826cf0")))
        {
          if (this.ToxicItems.Contains(MongoID.op_Implicit(obj.TemplateId)))
            ++this.ToxicItemCount;
          if (this.RadioactiveItems.Contains(MongoID.op_Implicit(obj.TemplateId)))
            ++this.RadItemCount;
        }
      }
    }
  }

  public void CheckInventoryForHazardousMaterials(Inventory inventory)
  {
    this.ToxicItemCount = 0;
    this.RadItemCount = 0;
    GearController.HasSafeContainer = false;
    IEnumerable<Item> allItems = GClass3176.GetAllItems((Item) inventory.QuestRaidItems);
    IEnumerable<Item> objs = Enumerable.Empty<Item>();
    foreach (int mainInventorySlot in GearController.MainInventorySlots)
    {
      EquipmentSlot equipmentSlot = (EquipmentSlot) mainInventorySlot;
      IEnumerable<Item> second = inventory.GetItemsInSlots((IEnumerable<EquipmentSlot>) new EquipmentSlot[1]
      {
        equipmentSlot
      }) ?? Enumerable.Empty<Item>();
      objs = objs.Concat<Item>(second);
    }
    this.InventoryCheckerHelper(objs);
    this.InventoryCheckerHelper(allItems, true);
  }

  private bool RollRadsCoughChance()
  {
    return (double) Random.Range(0, 10) < (double) HazardTracker.TotalRadiation / 10.0;
  }

  private void CoughController(Player player)
  {
    bool flag1 = (double) HazardTracker.TotalRadiation >= 30.0 && !Plugin.RealHealthController.HasBaseEFTEffect(player, "PainKiller");
    bool flag2 = (double) HazardTracker.TotalToxicity >= 30.0;
    bool flag3 = (double) HazardTracker.TotalToxicityRate >= 0.075000002980232239 * (1.0 + (double) PlayerState.ImmuneSkillStrong);
    bool flag4 = flag2 | flag1 | flag2;
    if (player.HealthController.IsAlive && (!GearController.HasGasMaskWithFilter || (double) GearController.GasMaskDurabilityFactor <= 0.0) && flag4 | flag3)
    {
      if (flag1 && !flag2)
      {
        if ((double) Time.time % (double) Mathf.RoundToInt(300f * (float) ((1.0 + (double) PlayerState.ImmuneSkillStrong) * (1.0 - (double) HazardTracker.TotalRadiation / 200.0))) < 1.0)
          this.DoCoughingAudio = true;
      }
      else
        this.DoCoughingAudio = true;
      if (flag3)
        this.IsCoughingInGas = true;
      else
        this.IsCoughingInGas = false;
    }
    else
    {
      this.IsCoughingInGas = false;
      this.DoCoughingAudio = false;
    }
  }
}
