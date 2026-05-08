using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Comfort.Common;
using EFT;
using EFT.AssetsManager;
using UnityEngine;

public class GClass2089 : GClass2088
{
	[Serializable]
	[CompilerGenerated]
	public class Class1392
	{
		public static readonly Class1392 class1392_0 = new Class1392();

		public static Action<AmmoPoolObject> action_0;

		public void method_0(AmmoPoolObject ammo)
		{
			AssetPoolObject.ReturnToPool(ammo);
		}
	}

	[NonSerialized]
	public GClass772[] Gclass772_0;

	[NonSerialized]
	public GClass772[] Gclass772_1;

	[NonSerialized]
	public List<AmmoPoolObject> List_0 = new List<AmmoPoolObject>();

	[NonSerialized]
	public int Int_0 = -1;

	[NonSerialized]
	public int Int_1;

	[NonSerialized]
	public int Int_2;

	[NonSerialized]
	public int Int_3;

	[NonSerialized]
	public BeltDetachablePartSpawner BeltDetachablePartSpawner_0;

	[NonSerialized]
	public bool Bool_1;

	[NonSerialized]
	public const string String_1 = "empty_link_";

	public virtual int MagazineCount => MagazineItemClass.Count;

	public override void Update()
	{
		method_1();
	}

	public override void CacheDataTransforms()
	{
		if (Gclass772_0 == null)
		{
			List<Transform> list = TransformHelperClass.smethod_4(GameObject_0.transform, "patron_", false);
			list.Reverse();
			Gclass772_0 = new GClass772[list.Count];
			for (int i = 0; i < list.Count; i++)
			{
				Transform transform = list[i];
				int id = int.Parse(transform.name.Replace("patron_", ""));
				Gclass772_0[i] = new GClass772(transform, id);
			}
		}
		if (Gclass772_1 == null)
		{
			List<Transform> list2 = TransformHelperClass.smethod_4(GameObject_0.transform, "empty_link_", false);
			Gclass772_1 = new GClass772[list2.Count];
			for (int j = 0; j < Gclass772_1.Length; j++)
			{
				Transform transform2 = list2[j];
				int id2 = int.Parse(transform2.name.Replace("empty_link_", ""));
				Gclass772_1[j] = new GClass772(transform2, id2);
			}
		}
	}

	public override void InitializeVisualOnStart()
	{
		BeltDetachablePartSpawner_0 = GameObject_0.GetComponent<BeltDetachablePartSpawner>();
		Bool_1 = BeltDetachablePartSpawner_0 != null;
		method_0();
		Int_1 = MagazineItemClass.Count;
		Int_3 = MagazineItemClass.BeltMagazineRefreshCount;
		Int_2 = 0;
		RefreshVisualParts();
		Update();
	}

	public override void RefreshVisualParts()
	{
		for (int i = 0; i < Gclass772_1.Length; i++)
		{
			Gclass772_1[i].SetActive(isActive: false);
		}
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
		base.Reset();
	}

	public void method_0()
	{
		if (MagazineItemClass == null || MagazineCount <= 0)
		{
			return;
		}
		for (int i = 0; i < Gclass772_0.Length; i++)
		{
			GClass772 gClass = Gclass772_0[i];
			if (gClass.Id > MagazineCount)
			{
				gClass.SetActive(isActive: false);
				continue;
			}
			gClass.SetActive(isActive: true);
			AmmoItemClass bulletAtPosition = MagazineItemClass.GetBulletAtPosition(MagazineCount - 1 - i);
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
			int num = Int_1 - MagazineCount;
			if (num > Int_2)
			{
				int updatesCount = num - Int_2;
				method_2(updatesCount);
				method_3();
			}
		}
	}

	public void method_2(int updatesCount)
	{
		for (int i = 0; i < updatesCount; i++)
		{
			method_4(Int_2);
			method_6();
			Int_2++;
		}
	}

	public void method_3()
	{
		if (Int_2 >= Int_3)
		{
			RefreshVisualParts();
			method_5();
			Int_1 = MagazineCount;
			Int_2 = 0;
		}
	}

	public void method_4(int index)
	{
		if (index < Gclass772_1.Length)
		{
			Gclass772_1[index].SetActive(isActive: true);
		}
	}

	public void method_5()
	{
		if (Bool_1)
		{
			BeltDetachablePartSpawner_0.SpawnBeltDetachablePart();
		}
	}

	public void method_6()
	{
		AssetPoolObject.ReturnToPool(List_0[0]);
		List_0.RemoveAt(0);
		for (int i = 0; i < List_0.Count; i++)
		{
			List_0[i].transform.SetParent(Gclass772_0[i].Transform, worldPositionStays: false);
			List_0[i].transform.localPosition = Vector3.zero;
		}
		int num = List_0.Count - 1;
		if (List_0.Count < MagazineCount)
		{
			AmmoItemClass bulletAtPosition = MagazineItemClass.GetBulletAtPosition(GetBulletPositionForSpawnInBelt());
			Transform transform = Gclass772_0[num].Transform;
			GameObject gameObject = Singleton<PoolManagerClass>.Instance.CreateItem(bulletAtPosition, isAnimated: false);
			AmmoPoolObject component = gameObject.GetComponent<AmmoPoolObject>();
			List_0.Add(component);
			if (component != null)
			{
				component.SetUsed(isUsed: false);
			}
			gameObject.transform.SetParent(transform, worldPositionStays: false);
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
			gameObject.SetActive(value: true);
		}
	}

	public virtual int GetBulletPositionForSpawnInBelt()
	{
		return MagazineItemClass.Count - List_0.Count;
	}
}
