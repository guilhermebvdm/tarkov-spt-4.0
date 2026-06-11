// Decompiled with JetBrains decompiler
// Type: RealismMod.MountingUI
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using UnityEngine;
using UnityEngine.UI;

#nullable disable
namespace RealismMod;

public class MountingUI : MonoBehaviour
{
  public GameObject ActiveUIScreen;
  private GameObject _mountingUIGameObject;
  private Image _mountingUIImage;
  private RectTransform _mountingUIRect;
  private Vector2 _iconSize = new Vector2(80f, 80f);

  public void DestroyGameObject()
  {
    if (!Object.op_Inequality((Object) this._mountingUIGameObject, (Object) null))
      return;
    Object.Destroy((Object) this._mountingUIGameObject);
  }

  public void CreateGameObject(Transform parent)
  {
    this._mountingUIGameObject = new GameObject(nameof (MountingUI));
    this._mountingUIRect = this._mountingUIGameObject.AddComponent<RectTransform>();
    this._mountingUIRect.anchoredPosition = new Vector2(100f, 100f);
    this._mountingUIImage = this._mountingUIGameObject.AddComponent<Image>();
    this._mountingUIGameObject.transform.SetParent(parent);
    this._mountingUIImage.sprite = Plugin.LoadedSprites["mounting.png"];
    ((Graphic) this._mountingUIImage).raycastTarget = false;
    ((Graphic) this._mountingUIImage).color = Color.clear;
    this._mountingUIRect.sizeDelta = this._iconSize;
  }

  public void Update()
  {
    if (!Object.op_Inequality((Object) this.ActiveUIScreen, (Object) null) || !PluginConfig.EnableMountUI.Value)
      return;
    switch (StanceController.BracingDirection)
    {
      case EBracingDirection.Left:
        this._mountingUIImage.sprite = Plugin.LoadedSprites["mountingleft.png"];
        break;
      case EBracingDirection.Right:
        this._mountingUIImage.sprite = Plugin.LoadedSprites["mountingright.png"];
        break;
      default:
        this._mountingUIImage.sprite = Plugin.LoadedSprites["mounting.png"];
        break;
    }
    if (StanceController.IsMounting)
    {
      ((Graphic) this._mountingUIImage).color = Color.white;
      this._mountingUIRect.sizeDelta = Vector2.op_Multiply(this._iconSize, Mathf.Lerp(1f, 1.15f, Mathf.PingPong(Time.time * 0.9f, 1f)));
    }
    else if (StanceController.IsBracing && !PlayerState.IsSprinting)
    {
      this._mountingUIRect.sizeDelta = this._iconSize;
      float num = Mathf.Lerp(0.2f, 1f, Mathf.PingPong(Time.time, 1f));
      Color color;
      // ISSUE: explicit constructor call
      ((Color) ref color).\u002Ector(1f, 1f, 1f, num);
      ((Graphic) this._mountingUIImage).color = color;
    }
    else
      ((Graphic) this._mountingUIImage).color = Color.clear;
    ((Transform) this._mountingUIRect).localPosition = new Vector3(650f, -460f, 0.0f);
  }
}
