using EFT.InventoryLogic;
using EFT.UI.DragAndDrop;
using UnityEngine;
using UnityEngine.UI;

public class ItemViewStats : MonoBehaviour
{
	[SerializeField]
	private Image _modIcon;

	[SerializeField]
	private Image _modTypeIcon;

	[SerializeField]
	private Image _specialIcon;

	[SerializeField]
	private Image _armorClassIcon;

	[SerializeField]
	private GameObject _modTypeIconParent;

	public void SetStaticInfo(Item item, bool examined)
	{
		ArmorPlateItemClass armorPlateItemClass = item as ArmorPlateItemClass;
		bool flag = armorPlateItemClass != null && method_0(armorPlateItemClass);
		_modIcon.gameObject.SetActive(!flag);
		_modTypeIcon.gameObject.SetActive(!flag);
		_specialIcon.gameObject.SetActive(!flag);
		if (_modTypeIconParent != null)
		{
			_modTypeIconParent.SetActive(!flag);
		}
		if ((bool)_armorClassIcon)
		{
			_armorClassIcon.gameObject.SetActive(flag);
		}
		if ((bool)_armorClassIcon && flag)
		{
			method_1(armorPlateItemClass, examined);
		}
		else
		{
			method_2(item, examined);
		}
	}

	public bool method_0(ArmorPlateItemClass plate)
	{
		if (plate.CurrentAddress == null)
		{
			return true;
		}
		if (!(plate.CurrentAddress is GClass3391 gClass))
		{
			return true;
		}
		if (plate.Armor.ArmorClass == 0)
		{
			return false;
		}
		return !gClass.Slot.Locked;
	}

	public void method_1(ArmorPlateItemClass armorPlate, bool examined)
	{
		string key = smethod_0(armorPlate);
		_modIcon.color = new Color(_modTypeIcon.color.r, _modTypeIcon.color.g, _modTypeIcon.color.b, 0f);
		Sprite sprite = CacheResourcesPopAbstractClass.Pop<Sprite>(key);
		_armorClassIcon.sprite = sprite;
		_armorClassIcon.gameObject.SetActive(examined);
	}

	public void method_2(Item item, bool examined)
	{
		Sprite sprite = ItemViewFactory.LoadModIconSprite(item);
		Sprite sprite2 = ItemViewFactory.LoadModClassIconSprite(item);
		string specialIcon = ItemViewFactory.GetSpecialIcon(item);
		_modIcon.gameObject.SetActive(sprite != null && examined);
		_modIcon.color = GClass808.SetAlpha(_modIcon.color, 1f);
		_modTypeIcon.gameObject.SetActive(sprite2 != null && examined);
		_specialIcon.gameObject.SetActive(!string.IsNullOrEmpty(specialIcon) && examined);
		if (_modTypeIconParent != null)
		{
			_modTypeIconParent.SetActive(sprite2 != null && examined);
		}
		if (sprite != null)
		{
			_modIcon.sprite = sprite;
			_modIcon.SetNativeSize();
		}
		if (sprite2 != null)
		{
			_modTypeIcon.sprite = sprite2;
			_modTypeIcon.SetNativeSize();
		}
		if (!string.IsNullOrEmpty(specialIcon))
		{
			Sprite sprite3 = CacheResourcesPopAbstractClass.Pop<Sprite>("Special Icons/icon_" + specialIcon);
			_specialIcon.sprite = sprite3;
			LayoutElement orAddComponent = GClass6.GetOrAddComponent<LayoutElement>(_specialIcon.gameObject);
			Vector2 size = sprite3.textureRect.size;
			orAddComponent.minWidth = size.x;
			orAddComponent.preferredWidth = size.x;
			orAddComponent.minHeight = size.y;
			orAddComponent.preferredHeight = size.y;
			_specialIcon.SetNativeSize();
		}
	}

	public static string smethod_0(ArmorPlateItemClass item)
	{
		string arg = "Mod Types/icon_type_mod_armor_plate";
		if (item.Armor.ArmorClass == 0)
		{
			return null;
		}
		return $"{arg}_{item.Armor.ArmorClass}";
	}
}
