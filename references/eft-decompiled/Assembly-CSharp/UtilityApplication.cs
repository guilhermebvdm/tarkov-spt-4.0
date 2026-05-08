using System;
using System.Diagnostics;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Comfort.Common;
using EFT;
using EFT.InputSystem;
using EFT.UI;
using Newtonsoft.Json;
using UnityEngine;

public class UtilityApplication : ClientApplication<BackendDummyClass.GClass2321>
{
	[SerializeField]
	private string _locale = "en";

	[SerializeField]
	private bool _loadTemplatesFromBackend = true;

	[SerializeField]
	private bool _dropCache;

	protected ItemFactoryClass ItemFactory;

	public override async Task StartTask()
	{
		await base.StartTask();
		await method_1();
	}

	public async Task method_1()
	{
		GClass2304 gClass = GClass2304.Execute(resetSettings: false, PlayerUpdateQueue, BundleLock);
		await gClass;
		if (gClass.Failed)
		{
			throw new NotImplementedException();
		}
		if (_dropCache)
		{
			new WebClient().DownloadString(BackendConfigAbstractClass.BackendUrl + "/api/cache/drop");
		}
		Result<IAssetsManager, InputTree> result = gClass.Result;
		ClientBackEnd = BackendDummyClass.smethod_0("localhost", 7777, _loadTemplatesFromBackend);
		Init(result.Value0, result.Value1);
		Result<GClass903> result2 = await ClientBackEnd.Login("", "");
		if (result2.Failed)
		{
			throw new NotImplementedException();
		}
		GClass903 value = result2.Value;
		LocaleManagerClass.LocaleManagerClass.String_0 = _locale;
		while (base.Session == null && value == null)
		{
			await Task.Yield();
		}
		await LocaleClass.ReloadLocale(base.Session);
		ItemFactory = await GClass1812.CreateItemFactory(base.Session);
		Singleton<ItemFactoryClass>.Create(ItemFactory);
		IEasyAssets instance = Singleton<IEasyAssets>.Instance;
		PoolManagerClass poolManagerClass = new PoolManagerClass(instance, ItemFactory.ItemTemplates, null);
		Singleton<PoolManagerClass>.Create(poolManagerClass);
		await base.gameObject.AddComponent<UiPools>().Init();
		GClass926 gClass2 = new GClass926(instance, poolManagerClass);
		await gClass2.Init();
		Singleton<GClass926>.Create(gClass2);
		await GClass1811.LoadCustomization(base.Session);
		if (Singleton<BackendConfigSettingsClass>.Instantiated)
		{
			Singleton<BackendConfigSettingsClass>.Release(Singleton<BackendConfigSettingsClass>.Instance);
		}
		Singleton<BackendConfigSettingsClass>.Create((await base.Session.GetGlobalConfig<BackendConfigClass>()).Config);
		if (Singleton<GClass1706>.Instantiated)
		{
			Singleton<GClass1706>.Release(Singleton<GClass1706>.Instance);
		}
		Singleton<GClass1706>.Create(JsonParserClass.ParseJsonTo<global::GenericParsedJsonResponseClass<GClass1441>>(GClass861.Load<TextAsset>("TestClientSettings").text, Array.Empty<JsonConverter>()).data.ClientSettings);
	}

	[CompilerGenerated]
	[DebuggerHidden]
	public Task method_2()
	{
		return base.StartTask();
	}
}
