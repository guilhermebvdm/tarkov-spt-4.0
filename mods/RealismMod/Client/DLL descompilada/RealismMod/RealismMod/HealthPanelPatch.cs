// Decompiled with JetBrains decompiler
// Type: RealismMod.HealthPanelPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT.UI.Health;
using SPT.Reflection.Patching;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
namespace RealismMod;

public class HealthPanelPatch : ModulePatch
{
  public const float MAIN_FONT_SIZE = 14f;
  public const float SECONDARY_FONT_SIZE = 30f;
  public const float FONT_CHANGE_SPEED = 1f;

  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (HealthParametersPanel).GetMethod("method_0", BindingFlags.Instance | BindingFlags.Public);
  }

  private static Color GetCurrentGasColor(float level)
  {
    float num = level;
    if ((double) num == 0.0)
      return Color.white;
    if ((double) num <= 25.0)
      return Color.yellow;
    if ((double) num <= 50.0)
      return new Color(1f, 0.647f, 0.0f);
    if ((double) num <= 75.0)
      return new Color(1f, 0.25f, 0.0f);
    return (double) num > 75.0 ? Color.red : Color.white;
  }

  private static Color GetCurrentRadColor(float level)
  {
    float num = level;
    if ((double) num == 0.0)
      return Color.white;
    if ((double) num <= 15.0)
      return Color.yellow;
    if ((double) num <= 25.0)
      return new Color(1f, 0.647f, 0.0f);
    if ((double) num <= 50.0)
      return new Color(1f, 0.25f, 0.0f);
    return (double) num > 50.0 ? Color.red : Color.white;
  }

  private static Color GetGasRateColor(float level)
  {
    float num = level;
    if ((double) num < 0.0)
      return Color.green;
    if ((double) num == 0.0)
      return new Color(0.4549f, 0.4824f, 0.4941f, 1f);
    if ((double) num <= 0.15000000596046448)
      return Color.yellow;
    if ((double) num <= 0.25)
      return new Color(1f, 0.647f, 0.0f);
    if ((double) num <= 0.40000000596046448)
      return new Color(1f, 0.25f, 0.0f);
    return (double) num > 0.40000000596046448 ? Color.red : Color.white;
  }

  private static Color GetRadRateColor(float level)
  {
    float num = level;
    if ((double) num < 0.0)
      return Color.green;
    if ((double) num == 0.0)
      return new Color(0.4549f, 0.4824f, 0.4941f, 1f);
    if ((double) num <= 0.05000000074505806)
      return Color.yellow;
    if ((double) num <= 0.15000000596046448)
      return new Color(1f, 0.647f, 0.0f);
    if ((double) num <= 0.25)
      return new Color(1f, 0.25f, 0.0f);
    return (double) num > 0.25 ? Color.red : Color.white;
  }

  [PatchPostfix]
  private static void Postfix(HealthParametersPanel __instance)
  {
    GameObject gameObject1 = ((Component) __instance).gameObject;
    if (gameObject1.transform.childCount <= 0)
      return;
    GameObject gameObject2 = ((Component) gameObject1.transform.Find("Poisoning"))?.gameObject;
    if (Object.op_Inequality((Object) gameObject2, (Object) null))
    {
      GameObject gameObject3 = ((Component) gameObject2.transform.Find("Buff"))?.gameObject;
      GameObject gameObject4 = ((Component) gameObject2.transform.Find("Current"))?.gameObject;
      if (Object.op_Inequality((Object) gameObject3, (Object) null))
      {
        float level = PluginConfig.EnableTrueHazardRates.Value ? HazardTracker.BaseTotalToxicityRate : HazardTracker.TotalToxicityRate;
        CustomTextMeshProUGUI component = gameObject3.GetComponent<CustomTextMeshProUGUI>();
        ((TMP_Text) component).text = ((double) level > 0.0 ? "+" : "") + level.ToString("0.00");
        ((Graphic) component).color = HealthPanelPatch.GetGasRateColor(level);
        ((TMP_Text) component).fontSize = 14f;
      }
      if (Object.op_Inequality((Object) gameObject4, (Object) null))
      {
        float level = Mathf.Round(HazardTracker.TotalToxicity);
        CustomTextMeshProUGUI component = gameObject4.GetComponent<CustomTextMeshProUGUI>();
        ((TMP_Text) component).text = level.ToString();
        ((Graphic) component).color = HealthPanelPatch.GetCurrentGasColor(level);
        ((TMP_Text) component).fontSize = 30f;
      }
    }
    GameObject gameObject5 = ((Component) gameObject1.transform.Find("Radiation"))?.gameObject;
    if (Object.op_Inequality((Object) gameObject5, (Object) null))
    {
      GameObject gameObject6 = ((Component) gameObject5.transform.Find("Buff"))?.gameObject;
      GameObject gameObject7 = ((Component) gameObject5.transform.Find("Current"))?.gameObject;
      if (Object.op_Inequality((Object) gameObject6, (Object) null))
      {
        float level = PluginConfig.EnableTrueHazardRates.Value ? HazardTracker.BaseTotalRadiationRate : HazardTracker.TotalRadiationRate;
        CustomTextMeshProUGUI component = gameObject6.GetComponent<CustomTextMeshProUGUI>();
        ((TMP_Text) component).text = ((double) level > 0.0 ? "+" : "") + level.ToString("0.00");
        ((Graphic) component).color = HealthPanelPatch.GetRadRateColor(level);
        ((TMP_Text) component).fontSize = 14f;
      }
      if (Object.op_Inequality((Object) gameObject7, (Object) null))
      {
        float level = Mathf.Round(HazardTracker.TotalRadiation);
        CustomTextMeshProUGUI component = gameObject7.GetComponent<CustomTextMeshProUGUI>();
        ((TMP_Text) component).text = level.ToString();
        ((Graphic) component).color = HealthPanelPatch.GetCurrentRadColor(level);
        ((TMP_Text) component).fontSize = 30f;
      }
    }
  }
}
