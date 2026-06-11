// Decompiled with JetBrains decompiler
// Type: RealismMod.MoveDaCube
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using BepInEx.Configuration;
using BepInEx.Logging;
using Comfort.Common;
using EFT.Interactive;
using EFT.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace RealismMod;

public static class MoveDaCube
{
  public static ConfigEntry<float> ChangeSpeed;
  public static ConfigEntry<KeyboardShortcut> PositiveXKey;
  public static ConfigEntry<KeyboardShortcut> NegativeXKey;
  public static ConfigEntry<KeyboardShortcut> PositiveYKey;
  public static ConfigEntry<KeyboardShortcut> NegativeYKey;
  public static ConfigEntry<KeyboardShortcut> PositiveZKey;
  public static ConfigEntry<KeyboardShortcut> NegativeZKey;
  public static ConfigEntry<KeyboardShortcut> TranslateKey;
  public static ConfigEntry<KeyboardShortcut> ScaleKey;
  public static ConfigEntry<KeyboardShortcut> RotateKey;
  public static ConfigEntry<bool> LockXAndZRotation;
  public static ConfigEntry<string> SelectedObjectName;
  public static ConfigEntry<string> SelectedAssetName;
  public static DebugComponent TargetInteractableComponent;
  public static List<DebugComponent> AllInteractableComponents = new List<DebugComponent>();
  public static NoteWindow NoteUIPanel;
  public static InventoryScreen InventoryUI;
  public static TasksScreen TasksUI;
  public static List<ExfiltrationPoint> ExfiltrationPointList = new List<ExfiltrationPoint>();
  public static MoveDaCube.EInputMode Mode = MoveDaCube.EInputMode.Translate;

  private static void DrawerMatchPlayerYRotation(ConfigEntryBase entry)
  {
    if (Object.op_Equality((Object) Utils.GetYourPlayer(), (Object) null) || Object.op_Equality((Object) MoveDaCube.TargetInteractableComponent, (Object) null))
      return;
    MoveDaCube.InitializeButton(new Action(MoveDaCube.TargetInteractableComponent.MatchPlayerYRotation), "Object Faces Camera Direction");
  }

  private static void InitializeButton(Action callable, string buttonName)
  {
    if (!GUILayout.Button(buttonName, new GUILayoutOption[1]
    {
      GUILayout.ExpandWidth(true)
    }))
      return;
    callable();
  }

  private static void DrawerUnselectObject(ConfigEntryBase entry)
  {
    if (Object.op_Equality((Object) Utils.GetYourPlayer(), (Object) null) || Object.op_Equality((Object) MoveDaCube.TargetInteractableComponent, (Object) null))
      return;
    MoveDaCube.InitializeButton((Action) (() => MoveDaCube.UnselectObject()), "Unselect Object");
  }

  private static void DrawerSpawnObject(ConfigEntryBase entry)
  {
    if (Object.op_Equality((Object) Utils.GetYourPlayer(), (Object) null))
      return;
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    MoveDaCube.InitializeButton(MoveDaCube.\u003C\u003EO.\u003C0\u003E__SpawnCube ?? (MoveDaCube.\u003C\u003EO.\u003C0\u003E__SpawnCube = new Action(DebugComponent.SpawnCube)), "Spawn Object");
  }

  private static void DrawerSpawnAsset(ConfigEntryBase entry)
  {
    if (Object.op_Equality((Object) Utils.GetYourPlayer(), (Object) null))
      return;
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    MoveDaCube.InitializeButton(MoveDaCube.\u003C\u003EO.\u003C1\u003E__SpawnAsset ?? (MoveDaCube.\u003C\u003EO.\u003C1\u003E__SpawnAsset = new Action(DebugComponent.SpawnAsset)), "Spawn Asset");
  }

  private static void DrawerLogObjects(ConfigEntryBase entry)
  {
    MoveDaCube.InitializeButton((Action) (() => MoveDaCube.LogObjects()), "Log Object");
  }

  private static void DrawerResetTranslation(ConfigEntryBase entry)
  {
    if (Object.op_Equality((Object) Utils.GetYourPlayer(), (Object) null) || Object.op_Equality((Object) MoveDaCube.TargetInteractableComponent, (Object) null))
      return;
    MoveDaCube.InitializeButton(new Action(MoveDaCube.TargetInteractableComponent.ResetTranslation), "Move Object To Player Feet");
  }

  public static void InitTempBindings(ConfigFile config)
  {
    MoveDaCube.SelectedObjectName = config.Bind<string>("41.0: Object Control", "Selected Object Name", "", new ConfigDescription("", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_hazard_zones)
      }
    }));
    MoveDaCube.SelectedAssetName = config.Bind<string>("41.0: Object Control", "Selected Asset Name", "", new ConfigDescription("", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_hazard_zones)
      }
    }));
    MoveDaCube.ChangeSpeed = config.Bind<float>("41.0: Object Control", "Change Speed", 3f, (ConfigDescription) null);
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    config.Bind<string>("41.0: Object Control", "Spawn Object (name required)", "", new ConfigDescription("Creates a new zone object", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        CustomDrawer = (MoveDaCube.\u003C\u003EO.\u003C2\u003E__DrawerSpawnObject ?? (MoveDaCube.\u003C\u003EO.\u003C2\u003E__DrawerSpawnObject = new Action<ConfigEntryBase>(MoveDaCube.DrawerSpawnObject))),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_hazard_zones)
      }
    }));
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    config.Bind<string>("41.0: Object Control", "Spawn Asset (name required)", "", new ConfigDescription("Creates a new zone object", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        CustomDrawer = (MoveDaCube.\u003C\u003EO.\u003C3\u003E__DrawerSpawnAsset ?? (MoveDaCube.\u003C\u003EO.\u003C3\u003E__DrawerSpawnAsset = new Action<ConfigEntryBase>(MoveDaCube.DrawerSpawnAsset))),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_hazard_zones)
      }
    }));
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    config.Bind<string>("41.1: Object Control", "Move Object To Player Feet", "", new ConfigDescription("Moves object to player", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        CustomDrawer = (MoveDaCube.\u003C\u003EO.\u003C4\u003E__DrawerResetTranslation ?? (MoveDaCube.\u003C\u003EO.\u003C4\u003E__DrawerResetTranslation = new Action<ConfigEntryBase>(MoveDaCube.DrawerResetTranslation))),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_hazard_zones)
      }
    }));
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    config.Bind<string>("41.1: Object Control", "Face Player Camera Direction", "", new ConfigDescription("Creates a new zone object", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        CustomDrawer = (MoveDaCube.\u003C\u003EO.\u003C5\u003E__DrawerMatchPlayerYRotation ?? (MoveDaCube.\u003C\u003EO.\u003C5\u003E__DrawerMatchPlayerYRotation = new Action<ConfigEntryBase>(MoveDaCube.DrawerMatchPlayerYRotation))),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_hazard_zones)
      }
    }));
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    config.Bind<string>("41.1: Object Control", "Unselect Object", "", new ConfigDescription("Unselects currently selected object", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        CustomDrawer = (MoveDaCube.\u003C\u003EO.\u003C6\u003E__DrawerUnselectObject ?? (MoveDaCube.\u003C\u003EO.\u003C6\u003E__DrawerUnselectObject = new Action<ConfigEntryBase>(MoveDaCube.DrawerUnselectObject))),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_hazard_zones)
      }
    }));
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    config.Bind<string>("41.1: Object Control", "Log Objects", "", new ConfigDescription("Logs Objects", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        CustomDrawer = (MoveDaCube.\u003C\u003EO.\u003C7\u003E__DrawerLogObjects ?? (MoveDaCube.\u003C\u003EO.\u003C7\u003E__DrawerLogObjects = new Action<ConfigEntryBase>(MoveDaCube.DrawerLogObjects))),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_hazard_zones)
      }
    }));
    MoveDaCube.PositiveZKey = config.Bind<KeyboardShortcut>("20.0: Object Movement Keybinds", "1. Forward", new KeyboardShortcut((KeyCode) 117, Array.Empty<KeyCode>()), new ConfigDescription("", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_hazard_zones)
      }
    }));
    MoveDaCube.NegativeZKey = config.Bind<KeyboardShortcut>("20.0: Object Movement Keybinds", "2. Backward", new KeyboardShortcut((KeyCode) 101, Array.Empty<KeyCode>()), new ConfigDescription("", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_hazard_zones)
      }
    }));
    MoveDaCube.NegativeXKey = config.Bind<KeyboardShortcut>("20.0: Object Movement Keybinds", "3. Left", new KeyboardShortcut((KeyCode) 110, Array.Empty<KeyCode>()), new ConfigDescription("", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_hazard_zones)
      }
    }));
    MoveDaCube.PositiveXKey = config.Bind<KeyboardShortcut>("20.0: Object Movement Keybinds", "4. Right", new KeyboardShortcut((KeyCode) 105, Array.Empty<KeyCode>()), new ConfigDescription("", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_hazard_zones)
      }
    }));
    MoveDaCube.PositiveYKey = config.Bind<KeyboardShortcut>("20.0: Object Movement Keybinds", "5. Up", new KeyboardShortcut((KeyCode) 121, Array.Empty<KeyCode>()), new ConfigDescription("", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_hazard_zones)
      }
    }));
    MoveDaCube.NegativeYKey = config.Bind<KeyboardShortcut>("20.0: Object Movement Keybinds", "6. Down", new KeyboardShortcut((KeyCode) 108, Array.Empty<KeyCode>()), new ConfigDescription("", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_hazard_zones)
      }
    }));
    MoveDaCube.TranslateKey = config.Bind<KeyboardShortcut>("30.0: Mode Keybinds", "Translate", new KeyboardShortcut((KeyCode) 106, Array.Empty<KeyCode>()), new ConfigDescription("", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_hazard_zones)
      }
    }));
    MoveDaCube.ScaleKey = config.Bind<KeyboardShortcut>("30.0: Mode Keybinds", "Scale", new KeyboardShortcut((KeyCode) 104, Array.Empty<KeyCode>()), new ConfigDescription("", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_hazard_zones)
      }
    }));
    MoveDaCube.RotateKey = config.Bind<KeyboardShortcut>("30.0: Mode Keybinds", "Rotate", new KeyboardShortcut((KeyCode) 44, Array.Empty<KeyCode>()), (ConfigDescription) null);
    MoveDaCube.LockXAndZRotation = config.Bind<bool>("30.0: Mode Keybinds", "Lock X And Z Rotation Axes", true, new ConfigDescription("", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_hazard_zones)
      }
    }));
  }

  public static void AddComponentToExistingGO(GameObject box, string name)
  {
    GameObject gameObject = new GameObject();
    GameObject primitive = GameObject.CreatePrimitive((PrimitiveType) 3);
    primitive.transform.parent = gameObject.transform;
    ((Object) gameObject).name = name + "_parent";
    gameObject.transform.position = box.transform.position;
    gameObject.transform.rotation = box.transform.rotation;
    primitive.transform.rotation = box.transform.rotation;
    primitive.transform.localScale = box.transform.localScale;
    primitive.AddComponent<DebugComponent>();
    DebugComponent component1 = primitive.GetComponent<DebugComponent>();
    component1.Init();
    component1.SetName(name);
    Renderer component2 = ((Component) component1).GetComponent<Renderer>();
    component2.enabled = true;
    component2.material.color = Color.magenta;
    MoveDaCube.AllInteractableComponents.Add(component1);
    box.transform.localScale = new Vector3(0.0f, 0.0f, 0.0f);
  }

  public static void Update()
  {
    if (Object.op_Equality((Object) Utils.GetYourPlayer(), (Object) null) || Object.op_Equality((Object) MoveDaCube.TargetInteractableComponent, (Object) null))
      return;
    KeyboardShortcut keyboardShortcut1 = MoveDaCube.TranslateKey.Value;
    if (((KeyboardShortcut) ref keyboardShortcut1).IsDown())
    {
      MoveDaCube.Mode = MoveDaCube.EInputMode.Translate;
      MoveDaCube.TargetInteractableComponent.SetColor(new Color(0.0f, 1f, 0.0f, 0.1f));
      Singleton<GUISounds>.Instance.PlayUISound((EUISoundType) 17);
    }
    KeyboardShortcut keyboardShortcut2 = MoveDaCube.ScaleKey.Value;
    if (((KeyboardShortcut) ref keyboardShortcut2).IsDown())
    {
      MoveDaCube.Mode = MoveDaCube.EInputMode.Scale;
      MoveDaCube.TargetInteractableComponent.SetColor(new Color(0.0f, 0.0f, 1f, 0.1f));
      Singleton<GUISounds>.Instance.PlayUISound((EUISoundType) 18);
    }
    KeyboardShortcut keyboardShortcut3 = MoveDaCube.RotateKey.Value;
    if (((KeyboardShortcut) ref keyboardShortcut3).IsDown())
    {
      MoveDaCube.Mode = MoveDaCube.EInputMode.Rotate;
      MoveDaCube.TargetInteractableComponent.SetColor(new Color(1f, 0.0f, 0.0f, 0.1f));
      Singleton<GUISounds>.Instance.PlayUISound((EUISoundType) 19);
    }
    float deltaTime = Time.deltaTime;
    float speed = MoveDaCube.ChangeSpeed.Value;
    switch (MoveDaCube.Mode)
    {
      case MoveDaCube.EInputMode.Translate:
        MoveDaCube.HandleTranslation(speed, deltaTime);
        break;
      case MoveDaCube.EInputMode.Scale:
        MoveDaCube.HandleScaling(speed, deltaTime);
        break;
      case MoveDaCube.EInputMode.Rotate:
        MoveDaCube.HandleRotation(speed, deltaTime);
        break;
    }
  }

  public static void LogObjects(bool mute = false)
  {
    Utils.Logger.LogWarning((object) "=======================");
    foreach (DebugComponent interactableComponent in MoveDaCube.AllInteractableComponents)
    {
      Utils.Logger.LogWarning((object) "==");
      Utils.Logger.LogWarning((object) ("zone name " + ((Object) interactableComponent).name));
      if (Object.op_Inequality((Object) interactableComponent.Asset, (Object) null))
        Utils.Logger.LogWarning((object) ("asset name " + ((Object) interactableComponent.Asset).name));
      ManualLogSource logger1 = Utils.Logger;
      string[] strArray1 = new string[7];
      strArray1[0] = "\"position\": {\"x\":";
      Vector3 vector3 = ((Component) interactableComponent).transform.position;
      strArray1[1] = vector3.x.ToString();
      strArray1[2] = ",\"y\":";
      vector3 = ((Component) interactableComponent).transform.position;
      strArray1[3] = vector3.y.ToString();
      strArray1[4] = ",\"z\":";
      vector3 = ((Component) interactableComponent).transform.position;
      strArray1[5] = vector3.z.ToString();
      strArray1[6] = "},";
      string str1 = string.Concat(strArray1);
      logger1.LogWarning((object) str1);
      ManualLogSource logger2 = Utils.Logger;
      string[] strArray2 = new string[7];
      strArray2[0] = "\"rotation\": {\"x\":";
      Quaternion rotation = ((Component) interactableComponent).transform.rotation;
      vector3 = ((Quaternion) ref rotation).eulerAngles;
      strArray2[1] = vector3.x.ToString();
      strArray2[2] = ",\"y\":";
      vector3 = ((Component) interactableComponent).transform.eulerAngles;
      strArray2[3] = vector3.y.ToString();
      strArray2[4] = ",\"z\":";
      vector3 = ((Component) interactableComponent).transform.eulerAngles;
      strArray2[5] = vector3.z.ToString();
      strArray2[6] = "},";
      string str2 = string.Concat(strArray2);
      logger2.LogWarning((object) str2);
      ManualLogSource logger3 = Utils.Logger;
      string[] strArray3 = new string[7];
      strArray3[0] = "\"size\": {\"x\":";
      vector3 = ((Component) interactableComponent).transform.localScale;
      strArray3[1] = vector3.x.ToString();
      strArray3[2] = ",\"y\":";
      vector3 = ((Component) interactableComponent).transform.localScale;
      strArray3[3] = vector3.y.ToString();
      strArray3[4] = ",\"z\":";
      vector3 = ((Component) interactableComponent).transform.localScale;
      strArray3[5] = vector3.z.ToString();
      strArray3[6] = "}";
      string str3 = string.Concat(strArray3);
      logger3.LogWarning((object) str3);
      Utils.Logger.LogWarning((object) "==");
    }
  }

  public static void SelectObject(GameObject obj, bool mute = false)
  {
    MoveDaCube.TargetInteractableComponent = obj.GetComponent<DebugComponent>();
    MoveDaCube.Mode = MoveDaCube.EInputMode.Translate;
    MoveDaCube.TargetInteractableComponent.SetColor(new Color(0.0f, 1f, 0.0f, 0.1f));
    MoveDaCube.SelectedObjectName.Value = MoveDaCube.TargetInteractableComponent.GetName();
    if (mute)
      return;
    Singleton<GUISounds>.Instance.PlayUISound((EUISoundType) 33);
  }

  public static void UnselectObject(bool mute = false)
  {
    if (Object.op_Equality((Object) MoveDaCube.TargetInteractableComponent, (Object) null))
      return;
    MoveDaCube.TargetInteractableComponent.GetName();
    MoveDaCube.TargetInteractableComponent.SetColor(new Color(1f, 1f, 1f, 0.1f));
    MoveDaCube.TargetInteractableComponent = (DebugComponent) null;
    MoveDaCube.SelectedObjectName.Value = "";
    if (mute)
      return;
    Singleton<GUISounds>.Instance.PlayUISound((EUISoundType) 34);
  }

  public static bool NameTaken()
  {
    if (!(MoveDaCube.SelectedObjectName.Value != MoveDaCube.TargetInteractableComponent.GetName()))
      return false;
    ConsoleScreen.LogError($"An object by the name of {MoveDaCube.SelectedObjectName.Value} already exists!");
    Singleton<GUISounds>.Instance.PlayUISound((EUISoundType) 6);
    return true;
  }

  public static bool NameFieldEmpty()
  {
    if (!(MoveDaCube.SelectedObjectName.Value == ""))
      return false;
    ConsoleScreen.LogError("Your object has an empty name! SOMETHING WILL EXPLODE AHHH FIX IT NOW (jk just fix it and you'll be fine)");
    Singleton<GUISounds>.Instance.PlayUISound((EUISoundType) 6);
    return true;
  }

  public static void HandleScaling(float speed, float delta)
  {
    KeyboardShortcut keyboardShortcut1 = MoveDaCube.PositiveXKey.Value;
    if (((KeyboardShortcut) ref keyboardShortcut1).IsPressed())
      MoveDaCube.TargetInteractableComponent.ScaleMe("x", speed * delta);
    KeyboardShortcut keyboardShortcut2 = MoveDaCube.NegativeXKey.Value;
    if (((KeyboardShortcut) ref keyboardShortcut2).IsPressed())
      MoveDaCube.TargetInteractableComponent.ScaleMe("x", (float) -((double) speed * (double) delta));
    keyboardShortcut2 = MoveDaCube.PositiveYKey.Value;
    if (((KeyboardShortcut) ref keyboardShortcut2).IsPressed())
      MoveDaCube.TargetInteractableComponent.ScaleMe("y", speed * delta);
    keyboardShortcut2 = MoveDaCube.NegativeYKey.Value;
    if (((KeyboardShortcut) ref keyboardShortcut2).IsPressed())
      MoveDaCube.TargetInteractableComponent.ScaleMe("y", (float) -((double) speed * (double) delta));
    keyboardShortcut2 = MoveDaCube.PositiveZKey.Value;
    if (((KeyboardShortcut) ref keyboardShortcut2).IsPressed())
      MoveDaCube.TargetInteractableComponent.ScaleMe("z", speed * delta);
    keyboardShortcut2 = MoveDaCube.NegativeZKey.Value;
    if (!((KeyboardShortcut) ref keyboardShortcut2).IsPressed())
      return;
    MoveDaCube.TargetInteractableComponent.ScaleMe("z", (float) -((double) speed * (double) delta));
  }

  public static void HandleRotation(float speed, float delta)
  {
    float num = speed * 25f;
    KeyboardShortcut keyboardShortcut1 = MoveDaCube.PositiveXKey.Value;
    if (((KeyboardShortcut) ref keyboardShortcut1).IsPressed())
      MoveDaCube.TargetInteractableComponent.RotateMe("x", num * delta);
    KeyboardShortcut keyboardShortcut2 = MoveDaCube.NegativeXKey.Value;
    if (((KeyboardShortcut) ref keyboardShortcut2).IsPressed())
      MoveDaCube.TargetInteractableComponent.RotateMe("x", -num * delta);
    KeyboardShortcut keyboardShortcut3 = MoveDaCube.PositiveYKey.Value;
    if (((KeyboardShortcut) ref keyboardShortcut3).IsPressed())
      MoveDaCube.TargetInteractableComponent.RotateMe("y", num * delta);
    KeyboardShortcut keyboardShortcut4 = MoveDaCube.NegativeYKey.Value;
    if (((KeyboardShortcut) ref keyboardShortcut4).IsPressed())
      MoveDaCube.TargetInteractableComponent.RotateMe("y", -num * delta);
    KeyboardShortcut keyboardShortcut5 = MoveDaCube.PositiveZKey.Value;
    if (((KeyboardShortcut) ref keyboardShortcut5).IsPressed())
      MoveDaCube.TargetInteractableComponent.RotateMe("z", num * delta);
    KeyboardShortcut keyboardShortcut6 = MoveDaCube.NegativeZKey.Value;
    if (!((KeyboardShortcut) ref keyboardShortcut6).IsPressed())
      return;
    MoveDaCube.TargetInteractableComponent.RotateMe("z", -num * delta);
  }

  public static void HandleTranslation(float speed, float delta)
  {
    KeyboardShortcut keyboardShortcut1 = MoveDaCube.PositiveXKey.Value;
    if (((KeyboardShortcut) ref keyboardShortcut1).IsPressed())
      MoveDaCube.TargetInteractableComponent.TranslateMe("x", speed * delta);
    KeyboardShortcut keyboardShortcut2 = MoveDaCube.NegativeXKey.Value;
    if (((KeyboardShortcut) ref keyboardShortcut2).IsPressed())
      MoveDaCube.TargetInteractableComponent.TranslateMe("x", (float) -((double) speed * (double) delta));
    keyboardShortcut2 = MoveDaCube.PositiveYKey.Value;
    if (((KeyboardShortcut) ref keyboardShortcut2).IsPressed())
      MoveDaCube.TargetInteractableComponent.TranslateMe("y", speed * delta);
    keyboardShortcut2 = MoveDaCube.NegativeYKey.Value;
    if (((KeyboardShortcut) ref keyboardShortcut2).IsPressed())
      MoveDaCube.TargetInteractableComponent.TranslateMe("y", (float) -((double) speed * (double) delta));
    keyboardShortcut2 = MoveDaCube.PositiveZKey.Value;
    if (((KeyboardShortcut) ref keyboardShortcut2).IsPressed())
      MoveDaCube.TargetInteractableComponent.TranslateMe("z", speed * delta);
    keyboardShortcut2 = MoveDaCube.NegativeZKey.Value;
    if (!((KeyboardShortcut) ref keyboardShortcut2).IsPressed())
      return;
    MoveDaCube.TargetInteractableComponent.TranslateMe("z", (float) -((double) speed * (double) delta));
  }

  public enum EInputMode
  {
    Translate,
    Scale,
    Rotate,
  }
}
