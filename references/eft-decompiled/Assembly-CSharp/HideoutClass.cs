using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Comfort.Common;
using EFT;
using EFT.Communications;
using EFT.Hideout;
using EFT.InventoryLogic;
using EFT.UI;
using EFT.UI.Ragfair;
using EFT.UI.Screens;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UI.Hideout;
using UnityEngine;

public class HideoutClass : GInterface229
{
	public class Class404 : LoggerClass
	{
		[NonSerialized]
		public const string String_0 = "hideout";

		public static Class404 Instance
		{
			get
			{
				if (Singleton<Class404>.Instantiated)
				{
					return Singleton<Class404>.Instance;
				}
				Class404 @class = new Class404();
				Singleton<Class404>.Create(@class);
				return @class;
			}
		}

		public Class404()
			: base("hideout", LoggerMode.Add)
		{
		}
	}

	[Serializable]
	[CompilerGenerated]
	public class Class1907
	{
		public static readonly Class1907 class1907_0 = new Class1907();

		public static Func<GClass2204, bool> func_0;

		public static Func<QteHandleData, bool> func_1;

		public static Func<GClass2427, Requirement[]> func_2;

		public static Func<GStruct299, GClass1433> func_3;

		public static Func<TraderClass, Profile.TraderInfo> func_4;

		public static Func<GClass2362, IEnumerable<GClass2358>> func_5;

		public static Func<AreaData, bool> func_6;

		public bool method_0(GClass2204 x)
		{
			return x.AreaType == EAreaType.Generator;
		}

		public bool method_1(QteHandleData qteData)
		{
			return qteData.Requirements != null;
		}

		public Requirement[] method_2(GClass2427 v)
		{
			return v.Requirements;
		}

		public GClass1433 method_3(GStruct299 reference)
		{
			return new GClass1433(reference.Item, reference.Count);
		}

		public Profile.TraderInfo method_4(TraderClass trader)
		{
			return trader.Info;
		}

		public IEnumerable<GClass2358> method_5(GClass2362 x)
		{
			return x.Items;
		}

		public bool method_6(AreaData v)
		{
			return v.Template.Type == EAreaType.ShootingRange;
		}
	}

	[Serializable]
	[CompilerGenerated]
	public class Class1908<T> where T : Item
	{
		public static readonly Class1908<T> class1908_0 = new Class1908<T>();

		public static Func<Item, MongoID> func_0;

		public static Func<Item, bool> func_1;

		public MongoID method_0(Item item)
		{
			return item.TemplateId;
		}

		public bool method_1(Item item)
		{
			return item.SpawnedInSession;
		}
	}

	[CompilerGenerated]
	public class Class1909
	{
		public AreaTemplate areaTemplate;

		public bool method_0(GClass2204 areaInfo)
		{
			return areaInfo.AreaType == areaTemplate.Type;
		}

		public bool method_1(AreaData data)
		{
			return data.Template.Type == areaTemplate.Type;
		}
	}

	[CompilerGenerated]
	public class Class1910
	{
		public GClass2204 areaInfo;

		public bool method_0(GClass2204 a)
		{
			return a.AreaType == areaInfo.AreaType;
		}
	}

	[CompilerGenerated]
	public class Class1911
	{
		public InventoryController inventoryController;

		public HideoutClass HideoutClass;

		public Profile profile;

		public EnergyControllerClass energyController;

		public void method_0()
		{
			inventoryController.AddItemEvent -= HideoutClass.method_26;
			inventoryController.RemoveItemEvent -= HideoutClass.method_27;
			inventoryController.RefreshItemEvent -= HideoutClass.method_28;
			profile.Skills.OnSkillLevelChanged -= HideoutClass.method_10;
			profile.OnTraderLoyaltyChanged -= HideoutClass.method_11;
		}

		public void method_1()
		{
			energyController.OnEnergyGenerationChanged -= HideoutClass.ProductionController.EnergySupplyChanged;
			energyController.OnEnergyGenerationChanged -= HideoutClass.method_20;
		}
	}

	[CompilerGenerated]
	public class Class1912
	{
		public HideoutClass HideoutClass;

		public AreaData areaData;

		public void method_0()
		{
			HideoutClass.method_9(areaData);
		}

		public bool method_1(GClass2204 info)
		{
			return info.AreaType == areaData.Template.Type;
		}

		public void method_2(bool silent)
		{
			HideoutClass.method_37();
		}
	}

	[CompilerGenerated]
	public class Class1913
	{
		public AreaData areaData;

		public bool method_0(AreaData x)
		{
			return x.Template.Id == areaData.ParentAreaId;
		}
	}

	[CompilerGenerated]
	public class Class1914
	{
		public AreaData areaData;

		public bool method_0(Requirement requirement)
		{
			return requirement.SourceAreaType != areaData.Template.Type;
		}
	}

	[CompilerGenerated]
	public class Class1915
	{
		public AreaTemplate areaTemplate;

		public HideoutClass HideoutClass;

		public bool method_0(AreaTemplate v)
		{
			return v.Type == areaTemplate.Type;
		}

		public bool method_1(QteData qteData)
		{
			return qteData.AreaType == areaTemplate.Type;
		}
	}

	[CompilerGenerated]
	public class Class1916
	{
		public AreaData data;

		public Class1915 class1915_0;

		public void method_0(bool selected)
		{
			class1915_0.HideoutClass.method_16(data.Template.Type, selected);
		}
	}

	[CompilerGenerated]
	public class Class1917
	{
		public int level;

		public bool method_0(QteData qteData)
		{
			return qteData.AreaLevel == level;
		}
	}

	[CompilerGenerated]
	public class Class1918
	{
		public EAreaType type;

		public bool method_0(GClass2440 scheme)
		{
			return scheme.areaType == (int)type;
		}
	}

	[CompilerGenerated]
	public class Class1919
	{
		public GClass1433 itemReference;

		public bool method_0(Item playerItem)
		{
			return playerItem.Id == itemReference.id;
		}
	}

	[CompilerGenerated]
	public class Class1920
	{
		public int type;

		public EAreaType areaType;

		public bool method_0(GClass2204 a)
		{
			return a.Type == type;
		}

		public bool method_1(AreaData data)
		{
			return data.Template.Type == areaType;
		}
	}

	[CompilerGenerated]
	public class Class1921<T> where T : Item
	{
		public IEnumerable<MongoID> templateIdFilter;

		public HideoutClass HideoutClass;

		public bool method_0(Item item)
		{
			return templateIdFilter.Contains(item.TemplateId);
		}

		public bool method_1(Item item)
		{
			return HideoutClass.ISession.Profile.Examined(item);
		}
	}

	[CompilerGenerated]
	public class Class1922
	{
		public HideoutClass HideoutClass;

		public Item templateItem;

		public Action onComplete;

		public void method_0(IResult result)
		{
			HideoutClass.List_4.Remove(templateItem);
			if (result.Succeed)
			{
				HideoutClass.method_24();
			}
			onComplete?.Invoke();
		}
	}

	[CompilerGenerated]
	public class Class1923
	{
		public string stashId;

		public List<Item> producedItems;

		public GClass2447 behaviour;

		public void method_0(GEventArgs2 args)
		{
			if (args.Status == CommandStatus.Succeed)
			{
				Item item = args.Item;
				EFT.InventoryLogic.IContainer container = item.CurrentAddress?.Container;
				if (item.SpawnedInSession && container != null && !GClass3113.MergesWithChildren(container) && !(container.ParentItem.Id != stashId))
				{
					producedItems.Add(item);
					behaviour.ShowNotification(EHideoutNotificationType.ItemCollected, item);
				}
			}
		}
	}

	[CompilerGenerated]
	public class Class1924
	{
		public EAreaType areaType;

		public bool method_0(AreaData data)
		{
			return data.Template.Type == areaType;
		}

		public bool method_1(AreaData data)
		{
			return data.Template.Type == areaType;
		}

		public bool method_2(AreaData data)
		{
			return data.Template.Type == areaType;
		}
	}

	[CompilerGenerated]
	public class Class1925
	{
		public EAreaType type;

		public bool method_0(AreaData data)
		{
			return data.Template.Type == type;
		}
	}

	public const string NO_FREE_SPACE_TITLE = "no_free_space_title";

	public const string NO_FREE_SPACE_MESSAGE = "no_free_space_message";

	[NonSerialized]
	public const float Float_0 = 0.5f;

	[NonSerialized]
	public static GClass3751 Gclass3751_0 = new GClass3751();

	[NonSerialized]
	public static List<Item> List_0 = new List<Item>(8);

	[CompilerGenerated]
	private Action action_0;

	[CompilerGenerated]
	private Action<Item> action_1;

	[CompilerGenerated]
	private Action action_2;

	[CompilerGenerated]
	private Action<GClass2431, string> action_3;

	[CompilerGenerated]
	private Action action_4;

	[NonSerialized]
	public AreaTemplate[] AreaTemplate_0_1;

	[NonSerialized]
	public InventoryController InventoryController_0;

	[NonSerialized]
	public ISession ISession;

	[NonSerialized]
	public GClass2425 Gclass2425_0;

	[NonSerialized]
	public BonusController BonusController_0;

	[NonSerialized]
	public GClass4005 Gclass4005_0;

	[NonSerialized]
	public AbstractAchievementControllerClass AbstractAchievementControllerClass;

	[NonSerialized]
	public AbstractPrestigeControllerClass AbstractPrestigeControllerClass;

	[NonSerialized]
	public GClass3617 Gclass3617_0;

	[NonSerialized]
	public List<GInterface229> List_1 = new List<GInterface229>();

	[NonSerialized]
	public List<Item> List_2 = new List<Item>();

	[NonSerialized]
	public List<Item> List_3 = new List<Item>();

	[NonSerialized]
	public List<Item> List_4 = new List<Item>();

	[NonSerialized]
	public CompositeDisposableClass CompositeDisposableClass = new CompositeDisposableClass();

	[NonSerialized]
	public List<Requirement> List_5 = new List<Requirement>();

	[NonSerialized]
	public Dictionary<EAreaType, AreaData> Dictionary_0;

	[NonSerialized]
	public List<AreaData> List_6;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public bool Bool_1;

	[NonSerialized]
	public GClass2204 Gclass2204_0;

	[NonSerialized]
	public AreaData AreaData_0;

	[NonSerialized]
	public bool Bool_2;

	[NonSerialized]
	public QteData[] QteData_0;

	[NonSerialized]
	public HideoutSeedDataStruct HideoutSeedDataStruct;

	[NonSerialized]
	public GClass2659 Gclass2659_0;

	[NonSerialized]
	[CompilerGenerated]
	public EnergyControllerClass EnergyControllerClass;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_3;

	[NonSerialized]
	[CompilerGenerated]
	public GClass2443 Gclass2443_0;

	[NonSerialized]
	[CompilerGenerated]
	public GClass2437 Gclass2437_0;

	[NonSerialized]
	[CompilerGenerated]
	public ImprovementControllerClass ImprovementControllerClass;

	[NonSerialized]
	[CompilerGenerated]
	public HideoutSettingsClass HideoutSettingsClass;

	[NonSerialized]
	[CompilerGenerated]
	public GClass2204[] Gclass2204_1;

	[NonSerialized]
	[CompilerGenerated]
	public AreaTemplate[] AreaTemplate_1;

	[NonSerialized]
	[CompilerGenerated]
	public GClass2421 Gclass2421_0;

	[NonSerialized]
	public HealthControllerClass HealthControllerClass;

	[NonSerialized]
	public EEventType[] EeventType_0 = Array.Empty<EEventType>();

	[NonSerialized]
	public HashSet<GClass2440> HashSet_0 = new HashSet<GClass2440>();

	[NonSerialized]
	public HideoutCustomizationOffersCollection HideoutCustomizationOffersCollection_0;

	public IEnumerable<Item> AllStashItems => List_2;

	public EnergyControllerClass EnergyController
	{
		[CompilerGenerated]
		get
		{
			return EnergyControllerClass;
		}
		[CompilerGenerated]
		set
		{
			EnergyControllerClass = value;
		}
	}

	public bool IsClientBusy
	{
		[CompilerGenerated]
		get
		{
			return Bool_3;
		}
		[CompilerGenerated]
		set
		{
			Bool_3 = value;
		}
	}

	public GClass2443 ProductionSchemesCollection
	{
		[CompilerGenerated]
		get
		{
			return Gclass2443_0;
		}
		[CompilerGenerated]
		set
		{
			Gclass2443_0 = value;
		}
	}

	public GClass2437 ProductionController
	{
		[CompilerGenerated]
		get
		{
			return Gclass2437_0;
		}
		[CompilerGenerated]
		set
		{
			Gclass2437_0 = value;
		}
	}

	public ImprovementControllerClass ImprovementController
	{
		[CompilerGenerated]
		get
		{
			return ImprovementControllerClass;
		}
		[CompilerGenerated]
		set
		{
			ImprovementControllerClass = value;
		}
	}

	public HideoutSettingsClass Settings
	{
		[CompilerGenerated]
		get
		{
			return HideoutSettingsClass;
		}
		[CompilerGenerated]
		set
		{
			HideoutSettingsClass = value;
		}
	}

	public IEnumerable<GClass2440> FavoriteProductionSchemes => HashSet_0;

	public GClass2204[] GClass2204_0
	{
		[CompilerGenerated]
		get
		{
			return Gclass2204_1;
		}
		[CompilerGenerated]
		set
		{
			Gclass2204_1 = value;
		}
	}

	public AreaTemplate[] AreaTemplate_0
	{
		[CompilerGenerated]
		get
		{
			return AreaTemplate_1;
		}
		[CompilerGenerated]
		set
		{
			AreaTemplate_1 = value;
		}
	}

	public List<AreaData> AreaDatas => List_6 ?? (List_6 = new List<AreaData>());

	public GClass2421 CustomizationController
	{
		[CompilerGenerated]
		get
		{
			return Gclass2421_0;
		}
		[CompilerGenerated]
		set
		{
			Gclass2421_0 = value;
		}
	}

	public EEventType[] Events
	{
		get
		{
			return EeventType_0;
		}
		set
		{
			EeventType_0 = value;
			GClass2452.SaveGlobalEvents(EeventType_0);
		}
	}

	public event Action OnItemsChanged
	{
		[CompilerGenerated]
		add
		{
			Action action = action_0;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action action = action_0;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<Item> OnItemChanged
	{
		[CompilerGenerated]
		add
		{
			Action<Item> action = action_1;
			Action<Item> action2;
			do
			{
				action2 = action;
				Action<Item> value2 = (Action<Item>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<Item> action = action_1;
			Action<Item> action2;
			do
			{
				action2 = action;
				Action<Item> value2 = (Action<Item>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action OnAreaUpdated
	{
		[CompilerGenerated]
		add
		{
			Action action = action_2;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_2, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action action = action_2;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_2, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<GClass2431, string> OnProducedItemsReceived
	{
		[CompilerGenerated]
		add
		{
			Action<GClass2431, string> action = action_3;
			Action<GClass2431, string> action2;
			do
			{
				action2 = action;
				Action<GClass2431, string> value2 = (Action<GClass2431, string>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_3, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<GClass2431, string> action = action_3;
			Action<GClass2431, string> action2;
			do
			{
				action2 = action;
				Action<GClass2431, string> value2 = (Action<GClass2431, string>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_3, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action OnInited
	{
		[CompilerGenerated]
		add
		{
			Action action = action_4;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_4, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action action = action_4;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_4, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public async Task Init(ISession session)
	{
		ISession = session;
		Bool_0 = false;
		if (Bool_1)
		{
			method_5();
		}
		AreaTemplate_0_1 = (await HideoutAreasInfo.LoadAsync()).Templates.ToArray();
		GClass2204_0 = ISession.GetHideoutAreasInfo();
		Gclass2204_0 = GClass2204_0.FirstOrDefault((GClass2204 x) => x.AreaType == EAreaType.Generator);
		await Task.WhenAll(method_0(), method_1(), method_2(), method_4(), method_3());
		Bool_1 = true;
		action_4?.Invoke();
	}

	public async Task method_0()
	{
		AreaTemplate_0 = JsonParserClass.ParseJsonTo<AreaTemplate[]>(await ISession.GetAreaTemplatesUnparsed(), Array.Empty<JsonConverter>());
	}

	public async Task method_1()
	{
		QteData_0 = await ISession.GetAreasQte();
	}

	public async Task method_2()
	{
		Settings = await ISession.GetHideoutSettings();
		Class404.Instance.LogInfo("Generator consumption from backend: {0}", Settings.generatorFuelFlowRate);
		Class404.Instance.LogInfo("Air filter consumption from backend: {0}", Settings.airFilterUnitFlowRate);
	}

	public async Task method_3()
	{
		HideoutCustomizationOffersCollection_0 = await ISession.GetHideoutCustomizationOffers();
	}

	public async Task method_4()
	{
		ProductionSchemesCollection = await ISession.GetProductionRecipes();
		GClass2440[] productionSchemes = ProductionSchemesCollection.ProductionSchemes;
		foreach (GClass2440 gClass in productionSchemes)
		{
			if (gClass.FavoriteScheme)
			{
				HashSet_0.Add(gClass);
			}
			gClass.OnAddToFavorite += method_38;
			gClass.OnRemoveFromFavorite += method_39;
		}
	}

	public void method_5()
	{
		Bool_1 = false;
		List_1.Clear();
		foreach (AreaData areaData in AreaDatas)
		{
			areaData.ReleaseSubscriptions();
		}
		HashSet_0.Clear();
		GClass2440[] productionSchemes = ProductionSchemesCollection.ProductionSchemes;
		foreach (GClass2440 obj in productionSchemes)
		{
			obj.OnAddToFavorite -= method_38;
			obj.OnRemoveFromFavorite -= method_39;
		}
		if (Singleton<EnergyControllerClass>.Instantiated)
		{
			EnergyControllerClass instance = Singleton<EnergyControllerClass>.Instance;
			instance.OnEnergyGenerationChanged -= ProductionController.EnergySupplyChanged;
			instance.OnEnergyGenerationChanged -= method_20;
			instance.Dispose();
			Singleton<EnergyControllerClass>.Release(instance);
		}
	}

	public void UpdateProduction(Dictionary<string, HideoutProductionsDataClass> productionDataChanges)
	{
		if (productionDataChanges != null && productionDataChanges.Count != 0)
		{
			ProductionController.UpdateProductions(productionDataChanges);
		}
	}

	public void UpdateImprovements(Dictionary<string, HideoutImprovementsDataClass> improvements)
	{
		if (improvements != null && improvements.Count != 0)
		{
			ImprovementController.UpdateImprovementDataset(improvements);
		}
	}

	public async Task SetHideoutData(GClass2212 hideoutData, bool updateVisual)
	{
		AreaTemplate[] areaTemplate_0_ = AreaTemplate_0_1;
		foreach (AreaTemplate areaTemplate in areaTemplate_0_)
		{
			GClass2204 gClass = hideoutData.Info.Areas.FirstOrDefault((GClass2204 areaInfo) => areaInfo.AreaType == areaTemplate.Type);
			if (gClass == null)
			{
				continue;
			}
			AreaData areaData = AreaDatas.First((AreaData data) => data.Template.Type == areaTemplate.Type);
			areaData.SetDataOwner(hideoutData.IsGuest);
			if (hideoutData.IsGuest)
			{
				areaData.SetSelectedStatus(isSelected: false);
			}
			if (updateVisual)
			{
				areaData.StashController?.Dispose();
			}
			hideoutData.AreasStashes.TryGetValue(areaData.Template.Type, out var value);
			if (value != null)
			{
				areaData.SetAreaStash(value, hideoutData.Info.MannequinPoses);
			}
			if (updateVisual)
			{
				if (hideoutData.IsGuest)
				{
					method_6(gClass, areaData);
					areaData.SetCurrentLevelDumb(gClass.Level, silent: true);
				}
				else
				{
					areaData.SetCurrentLevelDumb(gClass.Level, silent: true);
					areaData.InitProfileData(gClass, InventoryController_0, ISession, silent: true);
				}
			}
		}
		EnergyController.SetInfinityGeneration(hideoutData.IsGuest);
		if (hideoutData.IsGuest)
		{
			EnergyController.SetSwitchedStatus(isOn: true);
		}
		await CustomizationController.InstallCustomization(hideoutData.Info.GlobalCustomization, hideoutData.CustomizationStash);
	}

	public void method_6(GClass2204 areaInfo, AreaData areaData)
	{
		GClass2204 gClass = ISession.Profile.Hideout.Areas.FirstOrDefault((GClass2204 a) => a.AreaType == areaInfo.AreaType);
		if (gClass != null)
		{
			gClass.Constructing = areaData.CurrentStage.ActionReady || areaData.CurrentStage.ActionGoing;
			DateTime startTime = areaData.CurrentStage.StartTime;
			float data = areaData.NextStage.ConstructionTime.Data;
			int value = (int)EFTDateTimeClass.ToUnixTime(startTime.AddSeconds(data));
			gClass.CompleteTime = value;
		}
	}

	public void SetInventoryController(InventoryController inventoryController, HealthControllerClass healthController, LocalQuestControllerClass questController, AbstractAchievementControllerClass achievementsController, AbstractPrestigeControllerClass prestigeController, GClass3617 dialogController)
	{
		CompositeDisposableClass.Dispose();
		HideoutSeedDataStruct = ISession.Profile.Hideout.Seed;
		Profile profile = ISession.Profile;
		method_13();
		InventoryController_0 = inventoryController;
		HealthControllerClass = healthController;
		Gclass4005_0 = questController;
		AbstractAchievementControllerClass = achievementsController;
		AbstractPrestigeControllerClass = prestigeController;
		Gclass3617_0 = dialogController;
		profile.Skills.OnSkillLevelChanged += method_10;
		profile.OnTraderLoyaltyChanged += method_11;
		inventoryController.AddItemEvent += method_26;
		inventoryController.RemoveItemEvent += method_27;
		inventoryController.RefreshItemEvent += method_28;
		CompositeDisposableClass.AddDisposable(delegate
		{
			inventoryController.AddItemEvent -= method_26;
			inventoryController.RemoveItemEvent -= method_27;
			inventoryController.RefreshItemEvent -= method_28;
			profile.Skills.OnSkillLevelChanged -= method_10;
			profile.OnTraderLoyaltyChanged -= method_11;
		});
		ProductionController = new GClass2437(Settings.generatorSpeedWithoutFuel);
		List_1.Add(ProductionController);
		ImprovementController = new ImprovementControllerClass();
		List_1.Add(ImprovementController);
		EnergyControllerClass energyController = new EnergyControllerClass();
		EnergyController = energyController;
		Singleton<EnergyControllerClass>.Create(EnergyController);
		List_1.Add(EnergyController);
		energyController.OnEnergyGenerationChanged += ProductionController.EnergySupplyChanged;
		energyController.OnEnergyGenerationChanged += method_20;
		CompositeDisposableClass.AddDisposable(delegate
		{
			energyController.OnEnergyGenerationChanged -= ProductionController.EnergySupplyChanged;
			energyController.OnEnergyGenerationChanged -= method_20;
		});
		method_14();
		if (Gclass2425_0 == null)
		{
			Gclass2425_0 = new GClass2425();
		}
		else
		{
			Gclass2425_0.DeInit();
		}
		Gclass2425_0.Init(profile, this);
		method_25();
		method_12();
		method_37();
		method_20(EnergyController.IsEnergyGenerationOn);
		CustomizationController = new GClass2421(HideoutCustomizationOffersCollection_0, InventoryController_0.Inventory.HideoutCustomizationStash, InventoryController_0, ISession.Profile, Gclass4005_0.Quests, ISession);
		MonoBehaviourSingleton<PreloaderUI>.Instance.MenuTaskBar.InitHideout(this);
		GClass2447.CanShowReadyNotifications = true;
	}

	public void method_7(AreaData areaData)
	{
		CompositeDisposableClass.SubscribeEvent(areaData.RecalculateAreas, delegate
		{
			method_9(areaData);
		});
		GClass2204 areaInfo = GClass2204_0.FirstOrDefault((GClass2204 info) => info.AreaType == areaData.Template.Type) ?? new GClass2204
		{
			Type = (int)areaData.Template.Type,
			Active = false
		};
		areaData.InitProfileData(areaInfo, InventoryController_0, ISession);
		CompositeDisposableClass.SubscribeEvent(areaData.RecalculateAreas, method_12);
		CompositeDisposableClass.SubscribeEvent(areaData.LevelUpdated, delegate
		{
			method_37();
		});
		method_8(areaData);
		if (areaData.Enabled)
		{
			method_19(areaData, areaInfo, areaData.Template.Type, ISession.Profile);
		}
	}

	public void method_8(AreaData areaData)
	{
		if (!GClass1673.IsNullOrEmpty(areaData.ParentAreaId))
		{
			List_6.First((AreaData x) => x.Template.Id == areaData.ParentAreaId).AddChildArea(areaData);
		}
	}

	public void method_9(AreaData areaData)
	{
		List_5 = List_5.Where((Requirement requirement) => requirement.SourceAreaType != areaData.Template.Type).ToList();
		Stage nextStage = areaData.NextStage;
		if (nextStage != null)
		{
			List_5.AddRange(nextStage.Requirements);
		}
		if (areaData.CurrentStage.QteData != null && !GClass856.IsNullOrEmpty(areaData.CurrentStage.QteData.Data))
		{
			foreach (QteHandleData item in areaData.CurrentStage.QteData.Data.Where((QteHandleData qteData) => qteData.Requirements != null))
			{
				List_5.AddRange(item.Requirements);
			}
		}
		List_5.AddRange(areaData.Template.Requirements);
		IEnumerable<Requirement> collection = GClass1518.Flatten(areaData.CurrentStage.Improvements.Select((GClass2427 v) => v.Requirements));
		List_5.AddRange(collection);
	}

	public double GetMaxBonus(EBonusType bonusType, double input)
	{
		return BonusController_0.Calculate(bonusType, input);
	}

	public void ApplyChanges()
	{
		ISession.FlushOperationQueue();
	}

	public void method_10(AbstractSkillClass skill)
	{
		method_30(decide: true);
	}

	public void method_11(Profile.TraderInfo traderInfo)
	{
		method_31(decide: true);
	}

	public void method_12()
	{
		method_29();
		method_30(decide: false);
		method_31(decide: false);
		method_34(decide: false);
		method_32(HealthControllerClass);
		method_33(HealthControllerClass);
		foreach (AreaData item in List_6)
		{
			item.CheckVisibility();
		}
	}

	public void method_13()
	{
		if (BonusController_0 == null)
		{
			BonusController_0 = new BonusController(Array.Empty<SkillBonusAbstractClass>(), ISession.Profile.Skills);
		}
		else
		{
			BonusController_0.Reset();
		}
		AreaTemplate[] areaTemplate_ = AreaTemplate_0;
		for (int i = 0; i < areaTemplate_.Length; i++)
		{
			foreach (KeyValuePair<int, Stage> stage in areaTemplate_[i].Stages)
			{
				stage.Deconstruct(out var _, out var value);
				foreach (SkillBonusAbstractClass bonuse in value.Bonuses)
				{
					BonusController_0.AddBonus(bonuse.Clone());
				}
			}
		}
	}

	public void method_14()
	{
		if (AreaTemplate_0 == null)
		{
			return;
		}
		GClass2447.CanShowReadyNotifications = false;
		Dictionary_0 = new Dictionary<EAreaType, AreaData>();
		Events = Singleton<BackendConfigSettingsClass>.Instance.EventType ?? Array.Empty<EEventType>();
		Profile profile = ISession.Profile;
		bool flag = false;
		if (!GClass856.IsNullOrEmpty(List_6))
		{
			flag = true;
			foreach (AreaData item2 in List_6)
			{
				item2.Dispose();
			}
		}
		AreaDatas.Clear();
		AreaTemplate[] areaTemplate_0_ = AreaTemplate_0_1;
		foreach (AreaTemplate areaTemplate in areaTemplate_0_)
		{
			AreaTemplate areaTemplate2 = AreaTemplate_0.FirstOrDefault((AreaTemplate v) => v.Type == areaTemplate.Type);
			IEnumerable<QteData> areaQteDatas = QteData_0.Where((QteData qteData) => qteData.AreaType == areaTemplate.Type);
			if (areaTemplate2 != null)
			{
				areaTemplate.UpdateData(areaTemplate2);
				method_15(areaTemplate2.Stages, areaTemplate, areaQteDatas);
			}
			if (areaTemplate.AreaBehaviour is GInterface229 item)
			{
				List_1.Add(item);
			}
			AreaData data = new AreaData(areaTemplate);
			switch (areaTemplate.Type)
			{
			case EAreaType.ChristmasIllumination:
				data.Template.Enabled = Events.Contains(EEventType.Christmas);
				break;
			case EAreaType.Generator:
				AreaData_0 = data;
				break;
			}
			AreaDatas.Add(data);
			Dictionary_0.Add(data.Template.Type, data);
			if (areaTemplate.AreaBehaviour is GInterface244 generator)
			{
				EnergyController.AddGenerator(generator, Gclass2204_0.Active);
			}
			ImprovementController.SetImprovements(data);
			if (data.Template.AreaBehaviour is GInterface248 gInterface)
			{
				gInterface.EnergyGenerationChanged(EnergyController.IsEnergyGenerationOn);
				EnergyController.OnEnergyGenerationChanged += gInterface.EnergyGenerationChanged;
			}
			data.OnSelected.Subscribe(delegate(bool selected)
			{
				method_16(data.Template.Type, selected);
			});
			method_7(data);
			if (flag)
			{
				data.TryContinueWaitUpgrade();
			}
		}
		ProductionController.EnergySupplyChanged(EnergyController.IsEnergyGenerationOn);
		ImprovementController.SetImprovementDataset(profile.Hideout.Improvements);
	}

	public void method_15(Dictionary<int, Stage> stages, AreaTemplate areaTemplate, IEnumerable<QteData> areaQteDatas)
	{
		foreach (KeyValuePair<int, Stage> stage3 in stages)
		{
			var (level, stage2) = (KeyValuePair<int, Stage>)(ref stage3);
			if (!areaTemplate.Stages.TryGetValue(level, out var value))
			{
				Debug.LogErrorFormat("There is no stage level {1} for area type {0} on client", areaTemplate.Type, level);
				continue;
			}
			QteData[] backendQteData = areaQteDatas.Where((QteData qteData) => qteData.AreaLevel == level).ToArray();
			stage2.QteData.UpdateBackend(backendQteData);
			value.UpdateData(stage2);
			method_17(value, level, areaTemplate.Type);
		}
	}

	public void method_16(EAreaType type, bool selected)
	{
		if (!selected)
		{
			return;
		}
		foreach (AreaData areaData in AreaDatas)
		{
			if (areaData.Template.Type != type)
			{
				areaData.SetSelectedStatus(isSelected: false);
			}
		}
	}

	public void method_17(Stage stage, int level, EAreaType type)
	{
		if (level > 0)
		{
			switch (type)
			{
			case EAreaType.AirFilteringUnit:
				stage.FuelSupply.Data = new List<HideoutControllerClass>
				{
					new HideoutControllerClass
					{
						AmountPerSecond = Settings.airFilterUnitFlowRate
					}
				};
				break;
			case EAreaType.Generator:
				stage.FuelSupply.Data = new List<HideoutControllerClass>
				{
					new HideoutControllerClass
					{
						AmountPerSecond = Settings.generatorFuelFlowRate
					}
				};
				break;
			}
		}
	}

	public List<ProductionBuildAbstractClass> method_18(EAreaType type)
	{
		if (EAreaType.ScavCase == type)
		{
			return ((IEnumerable<ProductionBuildAbstractClass>)ProductionSchemesCollection.ScavCaseSchemes).ToList();
		}
		if (EAreaType.CircleOfCultists == type)
		{
			return ((IEnumerable<ProductionBuildAbstractClass>)ProductionSchemesCollection.CultistsSchemes).ToList();
		}
		return ((IEnumerable<ProductionBuildAbstractClass>)ProductionSchemesCollection.ProductionSchemes.Where((GClass2440 scheme) => scheme.areaType == (int)type)).ToList();
	}

	public void method_19(AreaData data, GClass2204 areaInfo, EAreaType type, Profile profile)
	{
		List<ProductionBuildAbstractClass> list = method_18(type);
		if (!list.Any())
		{
			return;
		}
		List<MongoID> unlockedSchemeList = profile.UnlockedRecipeInfo.unlockedSchemeList;
		foreach (ProductionBuildAbstractClass item in list)
		{
			if (unlockedSchemeList.Contains(item._id))
			{
				item.locked = false;
			}
		}
		data.SetProductionSchemes(list, profile.Info);
		List<HideoutProductionsDataClass> list2 = new List<HideoutProductionsDataClass>();
		Dictionary<string, HideoutProductionsDataClass> production = profile.Hideout.Production;
		if (production != null && production.Count > 0)
		{
			foreach (ProductionBuildAbstractClass item2 in list)
			{
				if (profile.Hideout.Production.TryGetValue(item2._id, out var value))
				{
					list2.Add(value);
				}
			}
		}
		ProductionController.SetProducer(data, GClass2431.GetProducer(data, areaInfo, list2, list, profile.Skills, profile.FenceInfo));
	}

	public void method_20(bool isPowerOn)
	{
		ELightStatus lightStatus = (isPowerOn ? ELightStatus.Working : ELightStatus.OutOfFuel);
		foreach (AreaData areaData in AreaDatas)
		{
			areaData.LightStatus = lightStatus;
		}
	}

	public List<GStruct299> method_21(ItemRequirement[] requirements)
	{
		List<GStruct299> list = new List<GStruct299>(requirements.Length);
		foreach (ItemRequirement itemRequirement in requirements)
		{
			bool isTool = itemRequirement is ToolRequirement;
			int num = itemRequirement.IntCount;
			method_40(List_0, itemRequirement);
			foreach (Item item2 in List_0)
			{
				StackableItemItemClass stackableItemItemClass = item2 as StackableItemItemClass;
				GStruct299 gStruct = new GStruct299
				{
					Item = item2,
					IsTool = isTool,
					Count = ((stackableItemItemClass == null) ? 1 : Mathf.Min(num, stackableItemItemClass.StackObjectsCount)),
					RemoveReferenceItem = (stackableItemItemClass == null || num >= stackableItemItemClass.StackObjectsCount)
				};
				gStruct.Requirements = requirements;
				GStruct299 item = gStruct;
				num -= item.Count;
				list.Add(item);
				if (num <= 0)
				{
					break;
				}
			}
		}
		List_0.Clear();
		return list;
	}

	[CanBeNull]
	public IEnumerable<GInterface424> method_22(IEnumerable<GStruct299> references)
	{
		return method_23(references.Select((GStruct299 reference) => new GClass1433(reference.Item, reference.Count)));
	}

	[CanBeNull]
	public IEnumerable<GInterface424> method_23(IEnumerable<GClass1433> items)
	{
		List<GInterface424> list = null;
		foreach (GClass1433 itemReference in items)
		{
			Item item = List_2.FirstOrDefault((Item playerItem) => playerItem.Id == itemReference.id);
			object obj;
			if (item == null)
			{
				obj = null;
			}
			else
			{
				ItemAddress currentAddress = item.CurrentAddress;
				obj = ((currentAddress != null) ? GClass3113.GetOwnerOrNull(currentAddress) : null);
			}
			if (obj != null)
			{
				if (list == null)
				{
					list = new List<GInterface424>();
				}
				GStruct154<GInterface424> gStruct = default(GStruct154<GInterface424>);
				gStruct = ((!(item is StackableItemItemClass stackableItemItemClass) || stackableItemItemClass.StackObjectsCount <= itemReference.count) ? GClass1617.Cast<GClass3410, GInterface424>(InteractionsHandlerClass.Remove(item, InventoryController_0)) : GClass1617.Cast<GClass3422, GInterface424>(InteractionsHandlerClass.SplitToNowhere(stackableItemItemClass, itemReference.count, InventoryController_0, InventoryController_0, simulate: false)));
				if (gStruct.Succeeded)
				{
					list.Add(gStruct.Value);
				}
				else
				{
					Debug.LogError(gStruct.Error);
				}
			}
		}
		method_24();
		return list;
	}

	public void method_24()
	{
		method_25();
		method_34(decide: true);
	}

	public void method_25()
	{
		Inventory inventory = InventoryController_0.Inventory;
		List_2.Clear();
		GClass3380.GetAllAssembledItemsNonAlloc(inventory.Stash, List_2);
		GClass3380.GetAllAssembledItemsNonAlloc(inventory.QuestStashItems, List_2);
		GClass3380.GetAllAssembledItemsNonAlloc(inventory.QuestRaidItems, List_2);
		action_0?.Invoke();
	}

	public void method_26(GEventArgs2 args)
	{
		if (args.Status != CommandStatus.Succeed)
		{
			return;
		}
		Item item = args.Item;
		if (!List_2.Contains(item))
		{
			ItemAddress currentAddress = item.CurrentAddress;
			Inventory inventory = InventoryController_0.Inventory;
			if (GClass3380.IsChildOf(currentAddress, inventory.Stash, notMergedWithThisItem: true) || GClass3380.IsChildOf(currentAddress, inventory.QuestStashItems, notMergedWithThisItem: true) || GClass3380.IsChildOf(currentAddress, inventory.QuestRaidItems, notMergedWithThisItem: true))
			{
				GClass3380.GetAllAssembledItemsNonAlloc(item, List_2);
				method_34(decide: true);
				action_0?.Invoke();
				action_1?.Invoke(item);
			}
		}
	}

	public void method_27(GEventArgs3 args)
	{
		if (args.Status != CommandStatus.Succeed)
		{
			return;
		}
		Item item = args.Item;
		if (!List_2.Remove(item))
		{
			return;
		}
		List_3.Clear();
		GClass3380.GetAllAssembledItemsNonAlloc(item, List_3);
		foreach (Item item2 in List_3)
		{
			List_2.Remove(item2);
		}
		method_34(decide: true);
		action_0?.Invoke();
		action_1?.Invoke(item);
	}

	public void method_28(GEventArgs18 args)
	{
		Item item = args.Item;
		if (GClass3380.IsChildOf(item.CurrentAddress, InventoryController_0.Inventory.Stash, notMergedWithThisItem: true))
		{
			List_3.Clear();
			GClass3380.GetAllAssembledItemsNonAlloc(item, List_3);
			foreach (Item item2 in List_3)
			{
				if (!List_2.Contains(item2))
				{
					List_2.Add(item2);
				}
			}
		}
		method_34(decide: true);
		action_0?.Invoke();
		action_1?.Invoke(item);
	}

	public async Task UpgradeZone(EAreaType areaType, RelatedRequirements requirements)
	{
		List<GStruct299> list = method_21(requirements.OfType<ItemRequirement>().ToArray());
		IEnumerable<GInterface424> operationsList = method_22(list);
		IResult result = await ISession.UpgradeZone((int)areaType, list);
		if (result.Failed)
		{
			Debug.LogError(result.Error);
			GClass3399.RollBack(operationsList);
			method_24();
		}
	}

	public async Task CompleteUpgradeZone(EAreaType areaType)
	{
		int type = (int)areaType;
		IResult result = await ISession.CompleteUpgradeZone(type);
		if (result.Failed)
		{
			Debug.LogError(result.Error);
			return;
		}
		GClass2204 gClass = GClass2204_0.FirstOrDefault((GClass2204 a) => a.Type == type);
		if (gClass == null)
		{
			throw new Exception("Found area is null");
		}
		gClass.Level++;
		foreach (SkillBonusAbstractClass datum in (AreaDatas.FirstOrDefault((AreaData data) => data.Template.Type == areaType) ?? throw new Exception("Found areaData area is null")).StageAt(gClass.Level).Bonuses.Data)
		{
			if (datum.Passive && !datum.Production && datum is GClass1834 gClass2)
			{
				float maximum = ISession.Profile.Health.Energy.Maximum;
				ISession.Profile.Health.Energy.Maximum = (float)gClass2.CalculateValue(maximum);
			}
		}
		action_2?.Invoke();
	}

	public IEnumerable<T> GetAvailableItemsByFilter<T>(IEnumerable<MongoID> templateIdFilter = null, Func<Item, bool> predicate = null, bool onlyExamined = false) where T : Item
	{
		IEnumerable<Item> source = List_2;
		if (templateIdFilter != null)
		{
			source = source.Where((Item item) => templateIdFilter.Contains(item.TemplateId));
		}
		if (predicate != null)
		{
			source = source.Where(predicate);
		}
		if (onlyExamined)
		{
			source = source.Where((Item item) => ISession.Profile.Examined(item));
		}
		source = from item in source
			orderby item.TemplateId, item.SpawnedInSession
			select item;
		return source.OfType<T>();
	}

	public void PutItemInAreaSlot(EAreaType areaType, int slotIndex, Item item, int count = 1)
	{
		Dictionary<int, GStruct299> items = new Dictionary<int, GStruct299> { 
		{
			slotIndex,
			new GStruct299
			{
				Item = item,
				Count = count
			}
		} };
		ISession.PutItemsInAreaSlots(areaType, items, null);
		method_24();
	}

	public void PutManyInAreaSlots(EAreaType areaType, Dictionary<int, GStruct299> slotItems, Action onComplete)
	{
		ISession.PutItemsInAreaSlots(areaType, slotItems, onComplete);
		method_24();
	}

	public bool TakeItemsFromAreaSlot(EAreaType areaType, int slotIndex, MongoID itemTemplate, Action onComplete = null)
	{
		Item templateItem = Singleton<ItemFactoryClass>.Instance.GetPresetItem(itemTemplate);
		List_4.Add(templateItem);
		if (!InventoryController_0.Inventory.Stash.Grid.HasFreeSpaceForItems(List_4))
		{
			List_4.Remove(templateItem);
			if (MonoBehaviourSingleton<PreloaderUI>.Instantiated)
			{
				MonoBehaviourSingleton<PreloaderUI>.Instance.ShowErrorScreen(GClass2348.Localized("no_free_space_title"), GClass2348.Localized("no_free_space_message"));
			}
			onComplete?.Invoke();
			return false;
		}
		ISession.TakeItemsFromAreaSlots(areaType, new int[1] { slotIndex }, delegate(IResult result)
		{
			List_4.Remove(templateItem);
			if (result.Succeed)
			{
				method_24();
			}
			onComplete?.Invoke();
		});
		return true;
	}

	public async Task<IResult> RestartProduction(ProductionBuildAbstractClass scheme)
	{
		return await ISession.HideoutProductionReset(scheme._id);
	}

	public async Task<IResult> CancelProduction(ProductionBuildAbstractClass scheme)
	{
		return await ISession.HideoutProductionCancel(scheme._id);
	}

	public async Task StartSingleProduction(ProductionBuildAbstractClass scheme, Action callback)
	{
		ItemRequirement[] array = scheme.RequiredItems.ToArray();
		IEnumerable<GClass2358> enumerable = Array.Empty<GClass2358>();
		if (array.Length != 0)
		{
			enumerable = await method_35(array);
		}
		if (enumerable == null)
		{
			Class404.Instance.LogInfo("Items selection canceled");
			return;
		}
		HashSet<string> hashSet = new HashSet<string>();
		ItemRequirement[] array2 = array;
		foreach (ItemRequirement itemRequirement in array2)
		{
			if (itemRequirement is ToolRequirement)
			{
				hashSet.Add(itemRequirement.TemplateId);
			}
		}
		List<GClass1433> list = new List<GClass1433>();
		List<GClass1433> list2 = new List<GClass1433>();
		foreach (GClass2358 item2 in enumerable)
		{
			GClass1433 item = new GClass1433(item2.Id, item2.ExistingCount);
			if (hashSet.Contains(item2.TemplateId))
			{
				list2.Add(item);
			}
			else
			{
				list.Add(item);
			}
		}
		Class404.Instance.LogInfo("Items for StartProduction chosen | scheme " + scheme._id);
		ISession.StartSingleProduction(scheme._id, list, list2);
		callback?.Invoke();
	}

	public async Task StartScavCaseProduction(string schemeId, ItemRequirement[] requirements)
	{
		List<GStruct299> list = method_21(requirements);
		List<GClass1433> list2 = new List<GClass1433>();
		List<GClass1433> list3 = new List<GClass1433>();
		foreach (GStruct299 item2 in list)
		{
			GClass1433 item = new GClass1433(item2.Id, item2.Count);
			if (item2.IsTool)
			{
				list3.Add(item);
			}
			else
			{
				list2.Add(item);
			}
		}
		IEnumerable<GInterface424> operationsList = method_22(list);
		IResult result = await ISession.StartScavCaseProduction(schemeId, list2, list3);
		if (result.Failed)
		{
			Debug.LogError(result.Error);
			GClass3399.RollBack(operationsList);
			method_24();
		}
	}

	public async Task StartCultistsProduction()
	{
		await ISession.StartCultistsProduction();
	}

	public async Task<bool> GetProducedItems(GClass2431 producer, string schemeId, bool showItemsListWindow)
	{
		GClass2447 behaviour = Dictionary_0[producer.AreaType].Template.AreaBehaviour;
		List<Item> producedItems = new List<Item>();
		string stashId = InventoryController_0.Inventory.Stash.Id;
		InventoryController_0.AddItemEvent += delegate(GEventArgs2 args)
		{
			if (args.Status == CommandStatus.Succeed)
			{
				Item item = args.Item;
				EFT.InventoryLogic.IContainer container = item.CurrentAddress?.Container;
				if (item.SpawnedInSession && container != null && !GClass3113.MergesWithChildren(container) && !(container.ParentItem.Id != stashId))
				{
					producedItems.Add(item);
					behaviour.ShowNotification(EHideoutNotificationType.ItemCollected, item);
				}
			}
		};
		IResult obj = await ISession.TakeHideoutProduction(schemeId);
		InventoryController_0.AddItemEvent -= delegate(GEventArgs2 args)
		{
			if (args.Status == CommandStatus.Succeed)
			{
				Item item = args.Item;
				EFT.InventoryLogic.IContainer container = item.CurrentAddress?.Container;
				if (item.SpawnedInSession && container != null && !GClass3113.MergesWithChildren(container) && !(container.ParentItem.Id != stashId))
				{
					producedItems.Add(item);
					behaviour.ShowNotification(EHideoutNotificationType.ItemCollected, item);
				}
			}
		};
		if (obj.Failed)
		{
			return false;
		}
		method_24();
		action_3?.Invoke(producer, schemeId);
		if (!showItemsListWindow)
		{
			return true;
		}
		ItemUiContext instance = ItemUiContext.Instance;
		instance.ItemsListWindow.Show(producedItems, Singleton<HandbookClass>.Instance, InventoryController_0, instance, ISession.InsuranceCompany);
		return true;
	}

	public void StartContinuousProduction(string schemeId)
	{
		ISession.StartContinuousProduction(schemeId);
	}

	public void Update(float deltaTime)
	{
		if (!Bool_0)
		{
			if (!Bool_1)
			{
				return;
			}
			float profileDecayTime = Time.time - ISession.ProfilesUpdateTime;
			foreach (GInterface229 item in List_1)
			{
				if (item is GInterface252 gInterface)
				{
					gInterface.Start(profileDecayTime);
				}
			}
			Bool_0 = true;
		}
		foreach (GInterface229 item2 in List_1)
		{
			item2.Update(deltaTime);
		}
	}

	public void method_29()
	{
		foreach (AreaRequirement item in List_5.OfType<AreaRequirement>())
		{
			item.Test(Dictionary_0);
		}
		method_36();
	}

	public void method_30(bool decide)
	{
		SkillManager skills = ISession.Profile.Skills;
		foreach (SkillRequirement item in List_5.OfType<SkillRequirement>())
		{
			item.Test(skills);
		}
		if (decide)
		{
			method_37();
		}
	}

	public void method_31(bool decide)
	{
		List<Profile.TraderInfo> tradersList = ISession.Traders.Select((TraderClass trader) => trader.Info).ToList();
		foreach (GClass2455 item in List_5.OfType<GClass2455>())
		{
			item.Test(tradersList);
		}
		if (decide)
		{
			method_37();
		}
	}

	public void method_32(HealthControllerClass healthController)
	{
		foreach (HealthRequirement item in List_5.OfType<HealthRequirement>())
		{
			item.Test(healthController);
		}
	}

	public void method_33(HealthControllerClass healthController)
	{
		foreach (BodyPartBuffRequirement item in List_5.OfType<BodyPartBuffRequirement>())
		{
			item.Test(healthController);
		}
	}

	public async void method_34(bool decide)
	{
		if (GClass2340.InRaid)
		{
			return;
		}
		if (decide)
		{
			if (Bool_2)
			{
				return;
			}
			Bool_2 = true;
			await TasksExtensions.Delay(0.5f);
		}
		Bool_2 = false;
		foreach (ItemRequirement item in List_5.OfType<ItemRequirement>())
		{
			item.Test(List_2);
		}
		if (decide)
		{
			method_37();
		}
	}

	public async Task StartImprovement(GClass2427 improvement, EAreaType areaType)
	{
		ItemRequirement[] array = improvement.Requirements.OfType<ItemRequirement>().ToArray();
		IEnumerable<GClass2358> enumerable = Array.Empty<GClass2358>();
		if (array.Any())
		{
			enumerable = await method_35(array);
		}
		if (enumerable == null)
		{
			Class404.Instance.LogInfo("Items selection canceled");
			return;
		}
		List<GClass1433> list = new List<GClass1433>();
		foreach (GClass2358 item2 in enumerable)
		{
			GClass1433 item = new GClass1433(item2.Id, item2.ExistingCount);
			list.Add(item);
		}
		Class404.Instance.LogInfo("Items for StartProduction chosen | scheme " + improvement.Id);
		ISession.StartImprovement(improvement.Id, areaType, list);
	}

	public async Task<IEnumerable<GClass2358>> method_35(ItemRequirement[] requirements)
	{
		CustomizationClass customizationClass = await ItemUiContext.Instance.HandoverItemsWindow.SelectItemsAsync(EExchangeableWindowType.Hideout, new Dictionary<GInterface234, int>(1) { 
		{
			method_21(requirements).First(),
			1
		} }, displayInputField: false, ISession.Profile, InventoryController_0);
		return (customizationClass == null || customizationClass.Count <= 0) ? null : customizationClass.SelectMany((GClass2362 x) => x.Items);
	}

	public void method_36()
	{
		foreach (AreaData areaData in AreaDatas)
		{
			areaData.IconStatusUpdated.Invoke();
		}
	}

	public void method_37()
	{
		foreach (AreaData areaData in AreaDatas)
		{
			areaData.DecideStatus(areaData.CurrentLevel);
		}
	}

	public void ActivateWeapon(HideoutPlayerOwner owner)
	{
		if (List_6.FirstOrDefault((AreaData v) => v.Template.Type == EAreaType.ShootingRange)?.Template.AreaBehaviour is ShootingRangeBehaviour shootingRangeBehaviour)
		{
			shootingRangeBehaviour.ManualEnterLocation(owner);
		}
	}

	public int GetShootingRangeScore()
	{
		return ISession.GetHideoutShootingRangeScore();
	}

	public void SetShootingRangeScore(int value)
	{
		ISession.SetHideoutShootingRangeScore(value);
	}

	public void WorkoutOperation(IEnumerable<bool> qtesResult, string qteId, string accountId)
	{
		ISession.WorkoutEnded(qtesResult, qteId, accountId, delegate(IResult result)
		{
			if (result.Succeed)
			{
				if (Gclass2659_0 != null)
				{
					HideoutSeedDataStruct = Gclass2659_0.GetNewSeed();
				}
				Gclass2659_0 = null;
			}
		});
	}

	public bool InQteRandomChance(float chance)
	{
		return (float)QteRandomNext(0, 101) <= chance;
	}

	public int QteRandomNext(int min, int max)
	{
		if (Gclass2659_0 == null)
		{
			Gclass2659_0 = new GClass2659(HideoutSeedDataStruct);
		}
		return Gclass2659_0.Random(min, max);
	}

	public void ShowAreaItemsTransferScreen(EAreaType areaType, Action exitAction = null)
	{
		if (!InventoryController_0.Inventory.HideoutAreaStashes.TryGetValue(areaType, out var _))
		{
			Debug.LogError($"Area {areaType} does not have a stash.");
			return;
		}
		switch (areaType)
		{
		default:
		{
			HideoutAreaTransferItemsScreen.GClass3902 gClass2 = new HideoutAreaTransferItemsScreen.GClass3902(AreaDatas.First((AreaData data) => data.Template.Type == areaType), InventoryController_0, ISession);
			if (exitAction != null)
			{
				gClass2.OnClose += exitAction;
			}
			gClass2.ShowScreen(EScreenState.Queued);
			break;
		}
		case EAreaType.CircleOfCultists:
		{
			HideoutCircleOfCultistsScreen.GClass3903 gClass3 = new HideoutCircleOfCultistsScreen.GClass3903(AreaDatas.First((AreaData data) => data.Template.Type == areaType), ISession, InventoryController_0);
			if (exitAction != null)
			{
				gClass3.OnClose += exitAction;
			}
			gClass3.ShowScreen(EScreenState.Queued);
			break;
		}
		case EAreaType.EquipmentPresetsStand:
		{
			HideoutMannequinEquipmentScreen.GClass3904 gClass = new HideoutMannequinEquipmentScreen.GClass3904(AreaDatas.First((AreaData data) => data.Template.Type == areaType), ISession, InventoryController_0, HealthControllerClass);
			if (exitAction != null)
			{
				gClass.OnClose += exitAction;
			}
			gClass.ShowScreen(EScreenState.Queued);
			break;
		}
		}
	}

	public void ShowInventoryScreen(Action exitAction = null)
	{
		InventoryScreen.GClass3873 gClass = new InventoryScreen.GClass3873(ISession, HealthControllerClass, InventoryController_0, Gclass4005_0, AbstractAchievementControllerClass, AbstractPrestigeControllerClass, InventoryController_0.Inventory.Stash, EInventoryTab.Gear);
		if (exitAction != null)
		{
			gClass.OnClose += exitAction;
		}
		gClass.ShowScreen(EScreenState.Queued);
	}

	public AreaData InitHideoutArea(EAreaType type, HideoutArea area)
	{
		AreaData areaData = AreaDatas.First((AreaData data) => data.Template.Type == type);
		area.Init(areaData, InventoryController_0, ISession.Profile, CustomizationController);
		return areaData;
	}

	public void method_38(ProductionBuildAbstractClass scheme)
	{
		HashSet_0.Add(scheme as GClass2440);
	}

	public void method_39(ProductionBuildAbstractClass scheme)
	{
		HashSet_0.Remove(scheme as GClass2440);
	}

	public EventDialogScreen.GClass3863 StartGameScreen(MongoID dialogId)
	{
		EventDialogScreen.GClass3863 gClass = new EventDialogScreen.GClass3863(ISession.Profile, ISession, Gclass3617_0, dialogId);
		gClass.ShowScreen(EScreenState.Queued);
		return gClass;
	}

	[CompilerGenerated]
	public void method_40(List<Item> itemsBuffer, ItemRequirement requirement)
	{
		itemsBuffer.Clear();
		foreach (Item item in List_2)
		{
			if (requirement.IsSuitableItem(item) && GClass3380.IsEmpty(item))
			{
				itemsBuffer.Add(item);
			}
		}
		itemsBuffer.Sort(Gclass3751_0);
	}

	[CompilerGenerated]
	public void method_41(IResult result)
	{
		if (result.Succeed)
		{
			if (Gclass2659_0 != null)
			{
				HideoutSeedDataStruct = Gclass2659_0.GetNewSeed();
			}
			Gclass2659_0 = null;
		}
	}
}
