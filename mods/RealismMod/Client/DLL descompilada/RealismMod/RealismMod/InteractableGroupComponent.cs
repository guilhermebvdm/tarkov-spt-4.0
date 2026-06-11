// Decompiled with JetBrains decompiler
// Type: RealismMod.InteractableGroupComponent
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using EFT.Interactive;
using EFT.UI;
using EFT.UI.BattleTimer;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
namespace RealismMod;

public class InteractableGroupComponent : MonoBehaviour
{
  private List<InteractionZone> _interactionZones = new List<InteractionZone>();
  private List<ExfiltrationPoint> _exfilsToBlock = new List<ExfiltrationPoint>();
  private int _completedSteps = 0;
  private bool _allValvesInCorrectState = false;

  public InteractableGroup GroupData { get; set; }

  public int ComplatedSteps => this._completedSteps;

  private void Start()
  {
    foreach (InteractionZone componentsInChild in ((Component) this).GetComponentsInChildren<InteractionZone>())
    {
      this._interactionZones.Add(componentsInChild);
      componentsInChild.OnInteractableStateChanged += new InteractionZone.OnStateChanged(this.HandleValveStateChanged);
    }
    this.EvaluateInteractableStates(true);
  }

  private void BlockExtracts()
  {
    if (this._exfilsToBlock.Count == 0)
    {
      this._exfilsToBlock = GameWorldController.ExfilsInLocation.Where<ExfiltrationPoint>((Func<ExfiltrationPoint, bool>) (exfil => ((IEnumerable<string>) this.GroupData.ExfilsToBlock).Any<string>((Func<string, bool>) (name => exfil.Settings.Name == name)))).ToList<ExfiltrationPoint>();
      if (PluginConfig.ZoneDebug.Value)
        Utils.Logger.LogWarning((object) ("extract count" + this._exfilsToBlock.Count<ExfiltrationPoint>().ToString()));
      foreach (ExfiltrationPoint exfiltrationPoint in this._exfilsToBlock)
      {
        if (PluginConfig.ZoneDebug.Value)
          Utils.Logger.LogWarning((object) ("extract " + exfiltrationPoint.Settings.Name));
      }
    }
    this.ToggleExtractAvailaibility(this._exfilsToBlock, false);
  }

  private void ToggleExtractAvailaibility(List<ExfiltrationPoint> exfils, bool enable)
  {
    foreach (ExfiltrationPoint exfil in exfils)
    {
      if (PluginConfig.ZoneDebug.Value)
        Utils.Logger.LogWarning((object) $"{exfil.Settings.Name} set to {enable}");
      foreach (Collider componentsInChild in ((Component) exfil).GetComponentsInChildren<BoxCollider>())
        componentsInChild.enabled = enable;
    }
    this.UpdateExtractPanel();
  }

  private void UpdateExtractPanel()
  {
    if (Object.op_Equality((Object) GameWorldController.GamePlayerOwner, (Object) null))
      return;
    ExtractionTimersPanel extractionTimersPanel = (ExtractionTimersPanel) AccessTools.Field(typeof (GamePlayerOwner), "_timerPanel").GetValue((object) GameWorldController.GamePlayerOwner);
    foreach (KeyValuePair<string, ExitTimerPanel> keyValuePair in (Dictionary<string, ExitTimerPanel>) AccessTools.Field(typeof (ExtractionTimersPanel), "dictionary_0").GetValue((object) extractionTimersPanel))
    {
      ExfiltrationPoint exfil = (ExfiltrationPoint) AccessTools.Field(typeof (ExitTimerPanel), "_point").GetValue((object) keyValuePair.Value);
      CustomTextMeshProUGUI customTextMeshProUgui1 = (CustomTextMeshProUGUI) AccessTools.Field(typeof (ExitTimerPanel), "_pointName").GetValue((object) keyValuePair.Value);
      Color color = (Color) AccessTools.Field(typeof (ExitTimerPanel), "_defaultTimerColor").GetValue((object) keyValuePair.Value);
      if (this._exfilsToBlock.Any<ExfiltrationPoint>((Func<ExfiltrationPoint, bool>) (e => e.Settings.Name == exfil.Settings.Name)))
      {
        ((TMP_Text) customTextMeshProUgui1).text = GClass2112.Localized(exfil.Settings.Name, (string) null);
        if (!this._allValvesInCorrectState)
        {
          CustomTextMeshProUGUI customTextMeshProUgui2 = customTextMeshProUgui1;
          ((TMP_Text) customTextMeshProUgui2).text = ((TMP_Text) customTextMeshProUgui2).text + " BLOCKED BY GAS";
        }
        ((Graphic) customTextMeshProUgui1).color = this._allValvesInCorrectState ? color : Color.red;
      }
    }
  }

  private void HandleValveStateChanged(InteractionZone interactable, EInteractableState newState)
  {
    if (PluginConfig.ZoneDebug.Value)
      Utils.Logger.LogWarning((object) $"==Valve {((Object) ((Component) interactable).gameObject).name} changed to {newState}");
    this.EvaluateInteractableStates();
  }

  private void EvaluateInteractableStates(bool isInit = false)
  {
    if (PluginConfig.ZoneDebug.Value)
      Utils.Logger.LogWarning((object) $"is init? {isInit}");
    Dictionary<GasZone, float> dictionary = new Dictionary<GasZone, float>();
    bool flag1 = true;
    this._completedSteps = 0;
    foreach (InteractionZone interactionZone in this._interactionZones)
    {
      bool flag2 = interactionZone.State == interactionZone.InteractableData.DesiredEndState;
      if (!flag2)
      {
        if (PluginConfig.ZoneDebug.Value)
          Utils.Logger.LogWarning((object) $"Valve {((Object) ((Component) interactionZone).gameObject).name} is NOT desired state");
        flag1 = false;
      }
      else
      {
        if (PluginConfig.ZoneDebug.Value)
          Utils.Logger.LogWarning((object) $"Valve {((Object) ((Component) interactionZone).gameObject).name} is in desired state");
        ++this._completedSteps;
      }
      foreach (IZone hazardZone in interactionZone.HazardZones)
      {
        GasZone key = hazardZone as GasZone;
        if (Object.op_Inequality((Object) key, (Object) null))
        {
          if (!dictionary.ContainsKey(key))
            dictionary.Add(key, interactionZone.InteractableData.FullCompletionModifer);
          key.ZoneStrengthTargetModi = !flag2 ? 1f : interactionZone.InteractableData.PartialCompletionModifier;
          if (PluginConfig.ZoneDebug.Value)
            Utils.Logger.LogWarning((object) $"Valve {((Object) ((Component) interactionZone).gameObject).name} is currently {interactionZone.State}, strength for {((Object) ((Component) key).gameObject).name} is {key.ZoneStrengthTargetModi}");
        }
      }
    }
    this._allValvesInCorrectState = flag1;
    if (flag1)
    {
      if (PluginConfig.ZoneDebug.Value)
        Utils.Logger.LogWarning((object) "All valves in desired state ");
      foreach (KeyValuePair<GasZone, float> keyValuePair in dictionary)
      {
        keyValuePair.Key.ZoneStrengthTargetModi = keyValuePair.Value;
        if (PluginConfig.ZoneDebug.Value)
          Utils.Logger.LogWarning((object) $"zone {((Object) ((Component) keyValuePair.Key).gameObject).name} strength is {keyValuePair.Key.ZoneStrengthTargetModi}");
      }
      this.ToggleExtractAvailaibility(this._exfilsToBlock, true);
    }
    else
      this.BlockExtracts();
  }
}
