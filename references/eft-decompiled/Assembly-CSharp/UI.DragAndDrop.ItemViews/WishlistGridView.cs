using System.Runtime.CompilerServices;
using EFT;
using EFT.UI;
using UnityEngine;
using UnityEngine.UI;

namespace UI.DragAndDrop.ItemViews;

public class WishlistGridView : UIElement
{
	[CompilerGenerated]
	public class Class735
	{
		public GClass2067 manager;

		public WishlistGridView wishlistGridView_0;

		public void method_0()
		{
			manager.OnAddInWishlist -= wishlistGridView_0.method_2;
			manager.OnRemoveFromWishlist -= wishlistGridView_0.method_3;
			manager.OnChangeGroup -= wishlistGridView_0.method_4;
		}
	}

	[SerializeField]
	private GameObject _glow;

	[SerializeField]
	private GameObject _border;

	[SerializeField]
	private Image _icon;

	private MongoID mongoID_0;

	private GClass2067 gclass2067_0;

	public void Show(GClass2067 manager, MongoID templateId, bool examined)
	{
		if (manager == null)
		{
			method_0(active: false);
			return;
		}
		gclass2067_0 = manager;
		mongoID_0 = templateId;
		SetExamined(examined);
		manager.OnAddInWishlist += method_2;
		manager.OnRemoveFromWishlist += method_3;
		manager.OnChangeGroup += method_4;
		UI.AddDisposable(delegate
		{
			manager.OnAddInWishlist -= method_2;
			manager.OnRemoveFromWishlist -= method_3;
			manager.OnChangeGroup -= method_4;
		});
	}

	public void SetExamined(bool examined)
	{
		if (gclass2067_0 != null)
		{
			if (examined && gclass2067_0.IsInWishlist(mongoID_0, includeQol: true, out var group))
			{
				method_0(active: true);
				method_1(group);
			}
			else
			{
				method_0(active: false);
			}
		}
	}

	public void method_0(bool active)
	{
		if (_glow != null)
		{
			_glow.SetActive(active);
		}
		if (_border != null)
		{
			_border.SetActive(active);
		}
		if (_icon != null)
		{
			_icon.gameObject.SetActive(active);
		}
	}

	public void method_1(EWishlistGroup groupType)
	{
		if (_icon != null)
		{
			_icon.sprite = EFTHardSettings.Instance.StaticIcons.WishlistSpritesOutline[groupType];
		}
	}

	public void method_2(MongoID templateId, EWishlistGroup group)
	{
		if (!(templateId != mongoID_0))
		{
			method_0(active: true);
			method_1(group);
		}
	}

	public void method_3(MongoID templateId, EWishlistGroup group)
	{
		if (!(templateId != mongoID_0))
		{
			if (gclass2067_0.IsInWishlist(templateId, includeQol: true, out var group2))
			{
				method_1(group2);
			}
			else
			{
				method_0(active: false);
			}
		}
	}

	public void method_4(MongoID templateId, EWishlistGroup group)
	{
		if (!(templateId != mongoID_0))
		{
			method_1(group);
		}
	}
}
