// Decompiled with JetBrains decompiler
// Type: RealismMod.DebugComponent
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.Ballistics;
using EFT.Interactive;
using EFT.UI;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace RealismMod;

public class DebugComponent : InteractableObject
{
  public List<ActionsTypesClass> DebugActions = new List<ActionsTypesClass>();

  public GameObject ParentGameObject { get; private set; }

  public GameObject Asset { get; private set; }

  public void Init()
  {
    ((Component) this).gameObject.layer = LayerMask.NameToLayer("Interactive");
    this.ParentGameObject = ((Component) ((Component) this).gameObject.transform.parent).gameObject;
    this.DebugActions.AddRange((IEnumerable<ActionsTypesClass>) new List<ActionsTypesClass>()
    {
      new ActionsTypesClass()
      {
        Name = "Select / Unselect",
        Action = (Action) (() =>
        {
          if (Object.op_Equality((Object) MoveDaCube.TargetInteractableComponent, (Object) null))
            MoveDaCube.SelectObject(((Component) this).gameObject);
          else if (Object.op_Equality((Object) MoveDaCube.TargetInteractableComponent, (Object) this))
          {
            MoveDaCube.UnselectObject();
          }
          else
          {
            MoveDaCube.UnselectObject(true);
            MoveDaCube.SelectObject(((Component) this).gameObject);
          }
        })
      },
      new ActionsTypesClass()
      {
        Name = ((Object) ((Component) this).transform.parent).name,
        Action = new Action(this.LogDetails)
      },
      new ActionsTypesClass()
      {
        Name = "Move To Player Feet",
        Action = new Action(this.ResetTranslation)
      },
      new ActionsTypesClass()
      {
        Name = "Reset Scale",
        Action = new Action(this.ResetScale)
      },
      new ActionsTypesClass()
      {
        Name = "Reset Rotation",
        Action = new Action(this.ResetRotation)
      },
      new ActionsTypesClass()
      {
        Name = "Face Camera Direction",
        Action = new Action(this.MatchPlayerYRotation)
      },
      new ActionsTypesClass()
      {
        Name = "Delete",
        Action = new Action(this.Delete)
      },
      new ActionsTypesClass()
      {
        Name = "LogObjectsInBounds",
        Action = new Action(this.LogObjectsInBounds)
      }
    });
  }

  public void ResetRotation()
  {
    this.ParentGameObject.transform.rotation = Quaternion.identity;
    ((Component) this).gameObject.transform.rotation = Quaternion.identity;
    Singleton<GUISounds>.Instance.PlayUISound((EUISoundType) 38);
  }

  public void MatchPlayerYRotation()
  {
    Quaternion rotation = Utils.GetYourPlayer().CameraPosition.rotation;
    Vector3 eulerAngles = ((Quaternion) ref rotation).eulerAngles;
    Vector3 vector3;
    // ISSUE: explicit constructor call
    ((Vector3) ref vector3).\u002Ector(0.0f, eulerAngles.y, 0.0f);
    this.ParentGameObject.transform.rotation = Quaternion.Euler(vector3);
    Singleton<GUISounds>.Instance.PlayUISound((EUISoundType) 38);
  }

  public void ResetTranslation()
  {
    this.ParentGameObject.transform.position = Utils.GetYourPlayer().Transform.position;
    Singleton<GUISounds>.Instance.PlayUISound((EUISoundType) 38);
  }

  public void LogDetails()
  {
    Transform transform = ((Component) this).transform;
    Utils.Logger.LogWarning((object) ("==== " + ((Object) ((Component) this).transform.parent).name));
    Utils.Logger.LogWarning((object) ("name " + ((Object) ((Component) this).transform.parent).name));
    Utils.Logger.LogWarning((object) $"\"position\": {{\"x\":{((Component) transform).transform.position.x.ToString()},\"y\":{((Component) transform).transform.position.y.ToString()},\"z\":{((Component) transform).transform.position.z.ToString()}}},");
    ManualLogSource logger = Utils.Logger;
    string[] strArray = new string[7];
    strArray[0] = "\"rotation\": {\"x\":";
    Quaternion rotation = ((Component) transform).transform.rotation;
    strArray[1] = ((Quaternion) ref rotation).eulerAngles.x.ToString();
    strArray[2] = ",\"y\":";
    strArray[3] = ((Component) transform).transform.eulerAngles.y.ToString();
    strArray[4] = ",\"z\":";
    strArray[5] = ((Component) transform).transform.eulerAngles.z.ToString();
    strArray[6] = "},";
    string str = string.Concat(strArray);
    logger.LogWarning((object) str);
    Utils.Logger.LogWarning((object) $"\"size\": {{\"x\":{((Component) transform).transform.localScale.x.ToString()},\"y\":{((Component) transform).transform.localScale.y.ToString()},\"z\":{((Component) transform).transform.localScale.z.ToString()}}}");
    Singleton<GUISounds>.Instance.PlayUISound((EUISoundType) 38);
  }

  public void ResetScale()
  {
    ((Component) this).gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
    Singleton<GUISounds>.Instance.PlayUISound((EUISoundType) 38);
    Singleton<GUISounds>.Instance.PlayUISound((EUISoundType) 38);
  }

  public void LogObjectsInBounds()
  {
    List<GameObject> gameObjectList = new List<GameObject>();
    BoxCollider boxCollider = ((Component) this).gameObject.AddComponent<BoxCollider>();
    boxCollider.size = ((Component) this).gameObject.transform.localScale;
    ((Component) boxCollider).transform.position = ((Component) this).gameObject.transform.position;
    ((Component) boxCollider).transform.rotation = ((Component) this).gameObject.transform.rotation;
    Bounds bounds = ((Collider) boxCollider).bounds;
    Collider[] colliderArray = Physics.OverlapBox(((Bounds) ref bounds).center, ((Bounds) ref bounds).extents, Quaternion.identity);
    Utils.Logger.LogWarning((object) $"Found {colliderArray.Length} objects in the box collider bounds:");
    foreach (Collider collider in colliderArray)
      Utils.Logger.LogWarning((object) ((Object) ((Component) collider).gameObject).name);
  }

  public void Delete()
  {
    MoveDaCube.UnselectObject(true);
    MoveDaCube.AllInteractableComponents.Remove(this);
    MoveDaCube.SelectedObjectName.Value = "";
    ((Component) this).gameObject.transform.position = new Vector3(0.0f, 0.0f, -9999f);
    Singleton<GUISounds>.Instance.PlayUISound((EUISoundType) 13);
    Object.Destroy((Object) ((Component) this).gameObject, 3f);
    Object.Destroy((Object) this.Asset, 3f);
  }

  public void TranslateMe(string axis, float amount)
  {
    Vector3 vector3;
    // ISSUE: explicit constructor call
    ((Vector3) ref vector3).\u002Ector(0.0f, 0.0f, 0.0f);
    switch (axis)
    {
      case "x":
        // ISSUE: explicit constructor call
        ((Vector3) ref vector3).\u002Ector(amount, 0.0f, 0.0f);
        break;
      case "y":
        // ISSUE: explicit constructor call
        ((Vector3) ref vector3).\u002Ector(0.0f, amount, 0.0f);
        break;
      case "z":
        // ISSUE: explicit constructor call
        ((Vector3) ref vector3).\u002Ector(0.0f, 0.0f, amount);
        break;
    }
    this.ParentGameObject.transform.Translate(vector3, (Space) 1);
  }

  public void ScaleMe(string axis, float amount)
  {
    Vector3 vector3;
    // ISSUE: explicit constructor call
    ((Vector3) ref vector3).\u002Ector(0.0f, 0.0f, 0.0f);
    switch (axis)
    {
      case "x":
        // ISSUE: explicit constructor call
        ((Vector3) ref vector3).\u002Ector(amount, 0.0f, 0.0f);
        break;
      case "y":
        // ISSUE: explicit constructor call
        ((Vector3) ref vector3).\u002Ector(0.0f, amount, 0.0f);
        break;
      case "z":
        // ISSUE: explicit constructor call
        ((Vector3) ref vector3).\u002Ector(0.0f, 0.0f, amount);
        break;
    }
    ((Component) this).gameObject.transform.localScale = Vector3.op_Addition(((Component) this).gameObject.transform.localScale, vector3);
  }

  public void RotateMe(string axis, float amount)
  {
    Vector3 vector3;
    // ISSUE: explicit constructor call
    ((Vector3) ref vector3).\u002Ector(0.0f, 0.0f, 0.0f);
    switch (axis)
    {
      case "x":
        // ISSUE: explicit constructor call
        ((Vector3) ref vector3).\u002Ector(0.0f, amount, 0.0f);
        break;
      case "y":
        // ISSUE: explicit constructor call
        ((Vector3) ref vector3).\u002Ector(0.0f, 0.0f, amount);
        break;
      case "z":
        // ISSUE: explicit constructor call
        ((Vector3) ref vector3).\u002Ector(amount, 0.0f, 0.0f);
        break;
    }
    this.ParentGameObject.transform.localRotation = Quaternion.op_Multiply(this.ParentGameObject.transform.localRotation, Quaternion.Euler(vector3));
  }

  public void SetColor(Color color)
  {
    ((Component) this).gameObject.GetComponent<Renderer>().material.color = color;
  }

  public void SetName(string name)
  {
    ((Object) ((Component) this).gameObject).name = name;
    ((Object) this.ParentGameObject).name = name + "_parent";
  }

  public string GetName() => ((Object) ((Component) this).gameObject).name;

  public Vector3 GetPosition() => this.ParentGameObject.transform.position;

  public Vector3 GetScale() => ((Component) this).gameObject.transform.localScale;

  public Quaternion GetRotation() => ((Component) this).gameObject.transform.rotation;

  public static void SpawnCube()
  {
    if (MoveDaCube.SelectedObjectName.Value == "")
    {
      ConsoleScreen.LogError("You must give your new zone object a name!");
      Singleton<GUISounds>.Instance.PlayUISound((EUISoundType) 6);
    }
    else
    {
      GameObject gameObject = new GameObject();
      GameObject primitive = GameObject.CreatePrimitive((PrimitiveType) 3);
      primitive.GetComponent<Renderer>().enabled = true;
      primitive.transform.parent = gameObject.transform;
      DebugComponent debugComponent = primitive.AddComponent<DebugComponent>();
      debugComponent.Init();
      debugComponent.ResetTranslation();
      debugComponent.MatchPlayerYRotation();
      debugComponent.SetName(MoveDaCube.SelectedObjectName.Value);
      MoveDaCube.UnselectObject();
      MoveDaCube.SelectObject(primitive, true);
      Singleton<GUISounds>.Instance.PlayUISound((EUISoundType) 7);
      MoveDaCube.AllInteractableComponents.Add(debugComponent);
    }
  }

  public static object GetFieldValue(string fieldName)
  {
    return typeof (Assets).GetProperty(fieldName, BindingFlags.Public | BindingFlags.Static).GetValue((object) null);
  }

  public static void SpawnAsset()
  {
    if (MoveDaCube.SelectedObjectName.Value == "")
    {
      ConsoleScreen.LogError("You must give your new zone object a name!");
      Singleton<GUISounds>.Instance.PlayUISound((EUISoundType) 6);
    }
    else
    {
      GameObject gameObject1 = new GameObject();
      GameObject primitive = GameObject.CreatePrimitive((PrimitiveType) 3);
      primitive.GetComponent<Renderer>().enabled = true;
      primitive.transform.parent = gameObject1.transform;
      DebugComponent debugComponent = primitive.AddComponent<DebugComponent>();
      debugComponent.Init();
      debugComponent.ResetTranslation();
      debugComponent.MatchPlayerYRotation();
      debugComponent.SetName(MoveDaCube.SelectedObjectName.Value);
      MoveDaCube.UnselectObject();
      MoveDaCube.SelectObject(primitive, true);
      Singleton<GUISounds>.Instance.PlayUISound((EUISoundType) 7);
      MoveDaCube.AllInteractableComponents.Add(debugComponent);
      BifacialTransform transform = Utils.GetYourPlayer().Transform;
      GameObject gameObject2 = Object.Instantiate<GameObject>(ZoneSpawner.GetAndLoadAsset(MoveDaCube.SelectedAssetName.Value), primitive.transform.position, primitive.transform.rotation);
      ((Object) gameObject2).name = MoveDaCube.SelectedAssetName.Value + Utils.GenId();
      gameObject2.transform.parent = gameObject1.transform;
      debugComponent.Asset = gameObject2;
      gameObject2.GetComponentInChildren<BallisticCollider>();
      MoveDaCube.LogObjects();
    }
  }

  public static Material GetTransparentMaterial(Color color, float transparency)
  {
    Color color1;
    // ISSUE: explicit constructor call
    ((Color) ref color1).\u002Ector(color.r, color.g, color.b, transparency);
    Material transparentMaterial = new Material(Shader.Find("Standard"));
    transparentMaterial.SetFloat("_Mode", 3f);
    transparentMaterial.SetInt("_SrcBlend", 5);
    transparentMaterial.SetInt("_DstBlend", 10);
    transparentMaterial.SetInt("_ZWrite", 0);
    transparentMaterial.DisableKeyword("_ALPHATEST_ON");
    transparentMaterial.EnableKeyword("_ALPHABLEND_ON");
    transparentMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
    transparentMaterial.renderQueue = 3000;
    transparentMaterial.SetFloat("_Glossiness", 0.0f);
    transparentMaterial.color = color1;
    return transparentMaterial;
  }

  public static Color GetTransparentColor(Color color, float transparency)
  {
    return new Color(color.r, color.g, color.b, transparency);
  }
}
