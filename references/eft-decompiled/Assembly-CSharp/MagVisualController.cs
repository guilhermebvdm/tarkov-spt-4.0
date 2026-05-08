using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Comfort.Common;
using EFT;
using EFT.AssetsManager;
using JetBrains.Annotations;
using UnityEngine;

[DisallowMultipleComponent]
public class MagVisualController : WeaponModPoolObject
{
	[Serializable]
	[CompilerGenerated]
	public class Class430
	{
		public static readonly Class430 class430_0 = new Class430();

		public static Action<AmmoPoolObject> action_0;

		public void method_0(AmmoPoolObject ammo)
		{
			AssetPoolObject.ReturnToPool(ammo);
		}
	}

	private const string string_0 = "patron_";

	private GClass772[] gclass772_0;

	private MagazineItemClass magazineItemClass;

	private List<AmmoPoolObject> list_0 = new List<AmmoPoolObject>();

	private Animation animation_0;

	private int int_0 = -1;

	private bool bool_3;

	public void method_1()
	{
		if (gclass772_0 == null)
		{
			List<Transform> list = TransformHelperClass.smethod_4(base.transform, "patron_", false);
			gclass772_0 = new GClass772[list.Count];
			for (int i = 0; i < list.Count; i++)
			{
				Transform transform = list[i];
				int id = int.Parse(transform.name.Replace("patron_", ""));
				gclass772_0[i] = new GClass772(transform, id);
			}
		}
		animation_0 = GetComponent<Animation>();
	}

	public override void OnCreatePoolObject<TAssetPoolObject>([CanBeNull] global::AssetPoolAbstractClass<TAssetPoolObject> assetsPoolParent)
	{
		base.OnCreatePoolObject(assetsPoolParent);
		method_1();
	}

	public override void ReturnToPool()
	{
		ReturnAmmo();
		magazineItemClass = null;
		int_0 = -1;
		base.ReturnToPool();
	}

	public void ReturnAmmo()
	{
		list_0.ForEach(delegate(AmmoPoolObject ammo)
		{
			AssetPoolObject.ReturnToPool(ammo);
		});
		list_0.Clear();
	}

	public void InitMagazine(MagazineItemClass mag, IPlayer player, bool isAnimated = true)
	{
		magazineItemClass = mag;
		bool_3 = isAnimated;
		if (mag.Count > 0)
		{
			for (int i = 0; i < gclass772_0.Length; i++)
			{
				GClass772 gClass = gclass772_0[i];
				if (gClass.Id > magazineItemClass.Count)
				{
					gClass.SetActive(isActive: false);
					continue;
				}
				gClass.SetActive(isActive: true);
				AmmoItemClass bulletAtPosition = magazineItemClass.GetBulletAtPosition(magazineItemClass.Count - 1 - i);
				if (Singleton<PoolManagerClass>.Instantiated)
				{
					GameObject obj = Singleton<PoolManagerClass>.Instance.CreateItem(bulletAtPosition, Player.GetVisibleToCamera(player), player, isAnimated: false);
					AmmoPoolObject component = obj.GetComponent<AmmoPoolObject>();
					list_0.Add(component);
					if (component != null)
					{
						component.SetUsed(isUsed: false);
					}
					obj.transform.SetParent(gClass.Transform, worldPositionStays: false);
					obj.transform.localPosition = Vector3.zero;
					obj.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
					obj.SetActive(value: true);
					continue;
				}
				return;
			}
		}
		if (!bool_3)
		{
			method_3();
		}
		Update();
	}

	public void ObservedUpdate(int magCount)
	{
		if (magazineItemClass == null)
		{
			return;
		}
		bool flag;
		if ((flag = animation_0 != null && bool_3) && CameraClass.Instance.Distance(base.transform.position) > 10f)
		{
			flag = false;
		}
		if (animation_0 != null)
		{
			animation_0.enabled = flag;
		}
		if (flag)
		{
			method_2(magCount);
		}
		int num = gclass772_0.Length;
		for (int i = 0; i < num; i++)
		{
			GClass772 gClass = gclass772_0[i];
			if (gClass.Id > magCount && gClass.IsActive)
			{
				gClass.SetActive(isActive: false);
			}
		}
	}

	public virtual void Update()
	{
		if (magazineItemClass == null)
		{
			return;
		}
		int count = magazineItemClass.Count;
		bool flag;
		if ((flag = animation_0 != null && bool_3) && CameraClass.Instance.Distance(base.transform.position) > 10f)
		{
			flag = false;
		}
		if (animation_0 != null)
		{
			animation_0.enabled = flag;
		}
		if (flag)
		{
			method_3();
		}
		int num = gclass772_0.Length;
		for (int i = 0; i < num; i++)
		{
			GClass772 gClass = gclass772_0[i];
			if (gClass.Id > count && gClass.IsActive)
			{
				gClass.SetActive(isActive: false);
			}
		}
	}

	public void method_2(int magCount)
	{
		if (animation_0 == null || magazineItemClass == null || int_0 == magCount)
		{
			return;
		}
		int_0 = magCount;
		foreach (AnimationState item in animation_0)
		{
			item.speed = 0f;
			item.time = Mathf.Clamp((float)magCount * (1f / item.clip.frameRate), 0f, item.clip.length);
		}
	}

	public void method_3()
	{
		if (animation_0 == null || magazineItemClass == null || int_0 == magazineItemClass.Count)
		{
			return;
		}
		int_0 = magazineItemClass.Count;
		foreach (AnimationState item in animation_0)
		{
			item.speed = 0f;
			item.time = Mathf.Clamp((float)magazineItemClass.Count * (1f / item.clip.frameRate), 0f, item.clip.length);
		}
	}
}
