using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Comfort.Common;
using EFT.AssetsManager;
using UnityEngine;

public class GClass2091 : GClass2088
{
	[Serializable]
	[CompilerGenerated]
	public class Class1393
	{
		public static readonly Class1393 class1393_0 = new Class1393();

		public static Action<AmmoPoolObject> action_0;

		public void method_0(AmmoPoolObject ammo)
		{
			AssetPoolObject.ReturnToPool(ammo);
		}
	}

	[NonSerialized]
	public const float Float_0 = 100f;

	[NonSerialized]
	public GClass772[] Gclass772_0;

	[NonSerialized]
	public List<AmmoPoolObject> List_0 = new List<AmmoPoolObject>();

	[NonSerialized]
	public Animation Animation_0;

	[NonSerialized]
	public int Int_0 = -1;

	[NonSerialized]
	public static int Int_1;

	[NonSerialized]
	public int Int_2;

	public override void Update()
	{
		method_1();
	}

	public override void CacheDataTransforms()
	{
		if (Gclass772_0 == null)
		{
			List<Transform> list = TransformHelperClass.smethod_4(GameObject_0.transform, "patron_", false);
			Gclass772_0 = new GClass772[list.Count];
			for (int i = 0; i < list.Count; i++)
			{
				Transform transform = list[i];
				int id = int.Parse(transform.name.Replace("patron_", ""));
				Gclass772_0[i] = new GClass772(transform, id);
			}
		}
	}

	public override void InitializeVisualOnStart()
	{
		if (Int_1 == 0)
		{
			Int_1 = 1;
			Int_2 = 1;
		}
		else
		{
			Int_1 = 0;
			Int_2 = 0;
		}
		method_0();
		if (!Bool_0)
		{
			RefreshVisualParts();
		}
		Update();
	}

	public override void ReturnVisualPartsToPool()
	{
		List_0.ForEach(delegate(AmmoPoolObject ammo)
		{
			AssetPoolObject.ReturnToPool(ammo);
		});
		List_0.Clear();
	}

	public override void Reset()
	{
		MagazineItemClass = null;
		Int_0 = -1;
		List_0.Clear();
		Animation_0 = null;
		base.Reset();
	}

	public override void RefreshVisualParts()
	{
		if (Animation_0 == null || MagazineItemClass == null || Int_0 == MagazineItemClass.Count)
		{
			return;
		}
		Int_0 = MagazineItemClass.Count;
		foreach (AnimationState item in Animation_0)
		{
			item.speed = 0f;
			item.time = Mathf.Clamp((float)MagazineItemClass.Count * (1f / item.clip.frameRate), 0f, item.clip.length);
		}
	}

	public void method_0()
	{
		if (MagazineItemClass.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < Gclass772_0.Length; i++)
		{
			GClass772 gClass = Gclass772_0[i];
			if (gClass.Id > MagazineItemClass.Count)
			{
				gClass.SetActive(isActive: false);
				continue;
			}
			gClass.SetActive(isActive: true);
			AmmoItemClass bulletAtPosition = MagazineItemClass.GetBulletAtPosition(MagazineItemClass.Count - 1 - i);
			if (Singleton<PoolManagerClass>.Instantiated)
			{
				GameObject gameObject = Singleton<PoolManagerClass>.Instance.CreateItem(bulletAtPosition, isAnimated: false);
				AmmoPoolObject component = gameObject.GetComponent<AmmoPoolObject>();
				List_0.Add(component);
				if (component != null)
				{
					component.SetUsed(isUsed: false);
				}
				gameObject.transform.SetParent(gClass.Transform, worldPositionStays: false);
				gameObject.transform.localPosition = Vector3.zero;
				gameObject.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
				gameObject.SetActive(value: true);
				continue;
			}
			break;
		}
	}

	public void method_1()
	{
		if (MagazineItemClass != null)
		{
			method_2();
			method_3();
		}
	}

	public void method_2()
	{
	}

	public void method_3()
	{
		if (Time.frameCount % 2 != Int_2)
		{
			return;
		}
		int num = Gclass772_0.Length;
		int count = MagazineItemClass.Count;
		for (int i = 0; i < num; i++)
		{
			GClass772 gClass = Gclass772_0[i];
			if (gClass.Id > count && gClass.IsActive)
			{
				gClass.SetActive(isActive: false);
			}
		}
	}
}
