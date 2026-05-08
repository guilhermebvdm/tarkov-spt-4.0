using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Comfort.Common;
using EFT;
using EFT.CameraControl;
using EFT.InputSystem;
using EFT.InventoryLogic;
using UnityEngine;

public class LootTestApplication : ClientApplication<BackendDummyClass.GClass2321>
{
	[Serializable]
	[CompilerGenerated]
	public class Class505
	{
		public static readonly Class505 class505_0 = new Class505();

		public static Func<KeyValuePair<MongoID, ItemTemplate>, bool> func_0;

		public static Func<KeyValuePair<MongoID, ItemTemplate>, IEnumerable<ResourceKey>> func_1;

		public static Func<ResourceKey, string> func_2;

		public bool method_0(KeyValuePair<MongoID, ItemTemplate> x)
		{
			return x.Value.Prefab != null;
		}

		public IEnumerable<ResourceKey> method_1(KeyValuePair<MongoID, ItemTemplate> x)
		{
			return x.Value.AllResources;
		}

		public string method_2(ResourceKey x)
		{
			return x.path;
		}
	}

	public override async Task StartTask()
	{
		await base.StartTask();
		IOperation<IAssetsManager, InputTree> obj = await GClass2304.Execute(resetSettings: false, PlayerUpdateQueue, BundleLock);
		if (obj.Failed)
		{
			UnityEngine.Debug.LogError("Some freaking operation failed");
		}
		Result<IAssetsManager, InputTree> result = obj.Result;
		ClientBackEnd = BackendDummyClass.smethod_0();
		Init(result.Value0, result.Value1);
		await ClientBackEnd.Login("bugaga", "bugaga");
		ItemFactoryClass itemFactoryClass = await GClass1812.CreateItemFactory(base.Session);
		Singleton<ItemFactoryClass>.Create(itemFactoryClass);
		await GClass1811.LoadCustomization(base.Session);
		Singleton<PoolManagerClass>.Create(new PoolManagerClass(Singleton<IEasyAssets>.Instance, itemFactoryClass.ItemTemplates, null));
		await AssetsManagerSingletonClass.Manager.LoadBundlesAsync((from x in itemFactoryClass.ItemTemplates.Where((KeyValuePair<MongoID, ItemTemplate> x) => x.Value.Prefab != null).SelectMany((KeyValuePair<MongoID, ItemTemplate> x) => x.Value.AllResources)
			select x.path).ToArray());
		int num = 0;
		foreach (Item item in itemFactoryClass.CreateAllItemsEver())
		{
			await Task.Yield();
			GameObject obj2 = Singleton<PoolManagerClass>.Instance.CreateLootPrefab(item, ECameraType.Default);
			obj2.transform.position = new Vector3(num % 16, 0f, (float)num / 16f);
			obj2.SetActive(value: true);
			num++;
		}
	}

	[CompilerGenerated]
	[DebuggerHidden]
	public Task method_1()
	{
		return base.StartTask();
	}
}
