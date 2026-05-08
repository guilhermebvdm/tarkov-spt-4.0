using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Systems.Effects;
using AnimationEventSystem;
using AnimationSystem.RootMotionTable;
using Audio.AmbientSubsystem;
using Audio.SpatialSystem;
using Bsg.GameSettings;
using Comfort.Common;
using CommonAssets.Scripts;
using CommonAssets.Scripts.Audio;
using Dissonance;
using Diz.Binding;
using Diz.Jobs;
using Diz.LanguageExtensions;
using EFT.Animations;
using EFT.AssetsManager;
using EFT.Ballistics;
using EFT.CameraControl;
using EFT.Character.Data;
using EFT.ClientItems.ClientSpecItems;
using EFT.EnvironmentEffect;
using EFT.Game.Spawning;
using EFT.GameRandoms;
using EFT.HealthSystem;
using EFT.Interactive;
using EFT.Interactive.SecretExfiltrations;
using EFT.InventoryLogic;
using EFT.InventoryLogic.Operations;
using EFT.ItemInHandSubsystem;
using EFT.MovingPlatforms;
using EFT.PrefabSettings;
using EFT.RocketLauncher;
using EFT.SynchronizableObjects;
using EFT.UI;
using EFT.Vaulting;
using EFT.Vehicle;
using EFT.WeaponMounting;
using JetBrains.Annotations;
using JsonType;
using NLog;
using RootMotion.FinalIK;
using UnityEngine;
using UnityEngine.Audio;

namespace EFT;

public class Player : MonoBehaviour, IPlayer, IOnItemAdded, GInterface179, IOnItemRemoved, IOnSetInHands, MovingPlatform.GInterface459, IDissonancePlayer
{
	public class GClass1537 : Error
	{
		[NonSerialized]
		public Item Item_0;

		[NonSerialized]
		public Type Type_0;

		public GClass1537(Item item, Type componentType)
		{
			Item_0 = item;
			Type_0 = componentType;
		}

		public override string ToString()
		{
			return Item_0?.ToString() + " doesn't have components of type " + Type_0.Name;
		}
	}

	public class GClass1538 : Error
	{
		[NonSerialized]
		public Item Item_0;

		[NonSerialized]
		public ItemAddress ItemAddress_0;

		public GClass1538(Item item, ItemAddress itemAddress)
		{
			Item_0 = item;
			ItemAddress_0 = itemAddress;
		}

		public override string ToString()
		{
			return Item_0?.ToString() + " is not located at " + ItemAddress_0?.ToString() + ". It's at " + Item_0.Parent;
		}
	}

	public class GClass1539 : Error
	{
		[NonSerialized]
		public GClass3379 Gclass3379_0;

		[NonSerialized]
		public bool Bool_0;

		public GClass1539(GClass3379 component, bool value)
		{
			Gclass3379_0 = component;
			Bool_0 = value;
		}

		public override string ToString()
		{
			string[] obj = new string[5]
			{
				Gclass3379_0.GetType().Name,
				" of ",
				Gclass3379_0.Item?.ToString(),
				" is already in state ",
				null
			};
			bool bool_ = Bool_0;
			obj[4] = bool_.ToString();
			return string.Concat(obj);
		}
	}

	public abstract class PlayerInventoryController : InventoryController, GInterface416
	{
		public class Class1202
		{
			[NonSerialized]
			public MagazineItemClass MagazineItemClass;

			[NonSerialized]
			public float Float_0;

			[NonSerialized]
			public Callback Callback_0;

			[NonSerialized]
			public bool Bool_0;

			[NonSerialized]
			public Coroutine Coroutine_0;

			public Class1202(MagazineItemClass magazine, float duration, Callback callback)
			{
				MagazineItemClass = magazine;
				Float_0 = duration;
				Callback_0 = callback;
				if (Coroutine_0 != null)
				{
					StaticManager.KillCoroutine(Coroutine_0);
				}
				Coroutine_0 = StaticManager.BeginCoroutine(method_0());
			}

			public void Proceed()
			{
				if (Coroutine_0 != null)
				{
					if (Bool_0)
					{
						Callback_0.Succeed();
					}
					else
					{
						Callback_0.Fail("Cancelled");
					}
					StaticManager.KillCoroutine(Coroutine_0);
					Coroutine_0 = null;
				}
			}

			public void TryProceedForItem(MagazineItemClass magazine)
			{
				if (MagazineItemClass == magazine)
				{
					Proceed();
				}
			}

			public IEnumerator method_0()
			{
				Bool_0 = false;
				yield return new WaitForSeconds(Float_0);
				Bool_0 = true;
				Proceed();
			}
		}

		public class Class1204 : Interface19
		{
			[CompilerGenerated]
			public class Class1205
			{
				public TaskCompletionSource<IResult> cancellationHandlerSource;

				public void method_0()
				{
					cancellationHandlerSource.Succeed();
				}
			}

			[CompilerGenerated]
			public class Class1206
			{
				public TaskCompletionSource<IResult> executionSource;

				public void method_0(IResult res)
				{
					executionSource.SetResult(res);
				}
			}

			[NonSerialized]
			public InventoryController InventoryController_0;

			[NonSerialized]
			public MagazineItemClass MagazineItemClass;

			[NonSerialized]
			public AmmoItemClass AmmoItemClass;

			[NonSerialized]
			public int Int_0;

			[NonSerialized]
			public bool Bool_0;

			[NonSerialized]
			public float Float_0;

			[NonSerialized]
			public IItemOwner IitemOwner_0;

			[NonSerialized]
			public IItemOwner IitemOwner_1;

			[NonSerialized]
			public CancellationTokenSource CancellationTokenSource_0;

			[NonSerialized]
			public float Float_1;

			public bool Boolean_0 => CancellationTokenSource_0?.IsCancellationRequested ?? true;

			public Class1204(InventoryController inventoryController, MagazineItemClass magazine, AmmoItemClass sourceAmmo, int count, bool elite, float loadOneAmmoSpeed)
			{
				InventoryController_0 = inventoryController;
				MagazineItemClass = magazine;
				AmmoItemClass = sourceAmmo;
				Int_0 = count;
				Bool_0 = elite;
				Float_0 = loadOneAmmoSpeed;
				Float_1 = loadOneAmmoSpeed;
				IitemOwner_0 = GClass3113.GetOwner(MagazineItemClass.Parent);
				IitemOwner_1 = GClass3113.GetOwner(AmmoItemClass.Parent);
			}

			public async Task<IResult> Start()
			{
				method_0();
				CancellationTokenSource_0 = new CancellationTokenSource();
				TaskCompletionSource<IResult> cancellationHandlerSource = new TaskCompletionSource<IResult>();
				CancellationTokenSource_0.Token.Register(delegate
				{
					cancellationHandlerSource.Succeed();
				});
				method_1(CommandStatus.Begin);
				IResult result = (await Task.WhenAny<IResult>(method_2(), cancellationHandlerSource.Task)).Result;
				Proceed(result.Succeed);
				return result;
			}

			public void Proceed(bool success = true)
			{
				CancellationTokenSource cancellationTokenSource_ = CancellationTokenSource_0;
				if (cancellationTokenSource_ != null && !cancellationTokenSource_.IsCancellationRequested)
				{
					method_0();
					method_1(success ? CommandStatus.Succeed : CommandStatus.Failed);
					method_3();
				}
			}

			public void method_0()
			{
				if (CancellationTokenSource_0 != null)
				{
					CancellationTokenSource_0.Cancel(throwOnFirstException: false);
					CancellationTokenSource_0.Dispose();
					CancellationTokenSource_0 = null;
				}
			}

			public void TryProceedForItem(Item item)
			{
				if (MagazineItemClass == item || AmmoItemClass == item)
				{
					Proceed();
				}
			}

			public void method_1(CommandStatus status)
			{
				GEventArgs7 args = new GEventArgs7(AmmoItemClass, MagazineItemClass, Int_0, Float_0, status, InventoryController_0);
				IitemOwner_0.RaiseLoadMagazineEvent(args);
				if (IitemOwner_0 != InventoryController_0)
				{
					InventoryController_0.RaiseLoadMagazineEvent(args);
				}
				if (IitemOwner_1 != IitemOwner_0)
				{
					IitemOwner_1.RaiseLoadMagazineEvent(args);
				}
			}

			public async Task<IResult> method_2()
			{
				Task task = method_5();
				for (int i = 0; i < Int_0; i++)
				{
					await task;
					if (Boolean_0)
					{
						break;
					}
					task = method_5();
					if (Bool_0)
					{
						Float_1 = Mathf.Clamp(Float_1 - Float_0 * Singleton<BackendConfigSettingsClass>.Instance.LoadTimeSpeedProgress / 100f, Float_0 * 40f / 100f, 10f);
					}
					GStruct153 gStruct = MagazineItemClass.ApplyWithoutRestrictions(InventoryController_0, AmmoItemClass, 1, simulate: true);
					if (!gStruct.Failed)
					{
						BaseInventoryOperationClass operation = InventoryController_0.ConvertOperationResultToOperation(gStruct.Value);
						TaskCompletionSource<IResult> executionSource = new TaskCompletionSource<IResult>();
						InventoryController_0.vmethod_1(operation, delegate(IResult res)
						{
							executionSource.SetResult(res);
						});
						IResult result = await executionSource.Task;
						if (!result.Failed)
						{
							method_4();
							method_3(i == Int_0 - 1);
							if (Boolean_0)
							{
								break;
							}
							continue;
						}
						return result;
					}
					return GClass1617.ToResult(gStruct);
				}
				return SuccessfulResult.New;
			}

			public void method_3(bool refreshIcon = false)
			{
				if (AmmoItemClass.CurrentAddress != null)
				{
					AmmoItemClass.RaiseRefreshEvent(refreshIcon);
				}
				MagazineItemClass.RaiseRefreshEvent(refreshIcon);
			}

			public void method_4()
			{
				if (Singleton<GUISounds>.Instantiated)
				{
					Singleton<GUISounds>.Instance.PlayUILoadSound();
				}
			}

			[CompilerGenerated]
			public Task method_5()
			{
				return Task.Delay(Mathf.CeilToInt(Float_1 * 1000f));
			}
		}

		public class Class1207 : Interface19
		{
			[Serializable]
			[CompilerGenerated]
			public class Class1208
			{
				public static readonly Class1208 class1208_0 = new Class1208();

				public static Func<Item, int> func_0;

				public int method_0(Item item)
				{
					return item.StackObjectsCount;
				}
			}

			[CompilerGenerated]
			public class Class1209
			{
				public TaskCompletionSource<IResult> cancellationHandlerSource;

				public void method_0()
				{
					cancellationHandlerSource.Succeed();
				}
			}

			[CompilerGenerated]
			public class Class1210
			{
				public TaskCompletionSource<IResult> executionSource;

				public void method_0(IResult executeResult)
				{
					executionSource.SetResult(executeResult);
				}
			}

			[NonSerialized]
			public InventoryController InventoryController_0;

			[NonSerialized]
			public MagazineItemClass MagazineItemClass;

			[NonSerialized]
			public bool Bool_0;

			[NonSerialized]
			public float Float_0;

			[NonSerialized]
			public int Int_0;

			[NonSerialized]
			public float Float_1;

			[NonSerialized]
			public int Int_1;

			[NonSerialized]
			public CancellationTokenSource CancellationTokenSource_0;

			[NonSerialized]
			public Item Item_0;

			[NonSerialized]
			public Item Item_1;

			public bool Boolean_0 => CancellationTokenSource_0?.IsCancellationRequested ?? true;

			public Class1207(InventoryController inventoryController, MagazineItemClass magazine, float loadOneAmmoSpeed, bool elite)
			{
				InventoryController_0 = inventoryController;
				MagazineItemClass = magazine;
				Float_0 = loadOneAmmoSpeed;
				Bool_0 = elite;
				Float_1 = loadOneAmmoSpeed;
				Int_0 = MagazineItemClass.Cartridges.Items.Sum((Item item) => item.StackObjectsCount);
				Int_1 = Int_0;
			}

			public async Task<IResult> Start()
			{
				method_0();
				if (Int_1 == 0)
				{
					return new GClass1562(MagazineItemClass).ToResult();
				}
				CancellationTokenSource_0 = new CancellationTokenSource();
				TaskCompletionSource<IResult> cancellationHandlerSource = new TaskCompletionSource<IResult>();
				CancellationTokenSource_0.Token.Register(delegate
				{
					cancellationHandlerSource.Succeed();
				});
				IResult result = (await Task.WhenAny<IResult>(method_2(), cancellationHandlerSource.Task)).Result;
				Proceed(result.Succeed);
				return result;
			}

			public void method_0()
			{
				if (CancellationTokenSource_0 != null)
				{
					CancellationTokenSource_0.Cancel(throwOnFirstException: false);
					CancellationTokenSource_0.Dispose();
					CancellationTokenSource_0 = null;
				}
			}

			public void Proceed(bool success)
			{
				CancellationTokenSource cancellationTokenSource_ = CancellationTokenSource_0;
				if (cancellationTokenSource_ != null && !cancellationTokenSource_.IsCancellationRequested)
				{
					method_0();
					method_3(success ? CommandStatus.Succeed : CommandStatus.Failed);
				}
			}

			public void TryProceedForItem(Item item)
			{
				if (MagazineItemClass == item || Item_0 == item || Item_1 == item)
				{
					Proceed(success: true);
				}
			}

			public void method_1()
			{
				if (Singleton<GUISounds>.Instantiated)
				{
					Singleton<GUISounds>.Instance.PlayUIUnloadSound();
				}
			}

			public async Task<IResult> method_2()
			{
				Task task = method_4();
				while (!Boolean_0)
				{
					AmmoItemClass ammoItemClass = (AmmoItemClass)MagazineItemClass.Cartridges.Items.LastOrDefault();
					if (ammoItemClass == null)
					{
						break;
					}
					GStruct154<GInterface424> gStruct = InteractionsHandlerClass.QuickFindAppropriatePlace(ammoItemClass, InventoryController_0, GClass1518.ToEnumerable(InventoryController_0.Inventory.Equipment), InteractionsHandlerClass.EMoveItemOrder.UnloadAmmo, simulate: true);
					if (!gStruct.Failed)
					{
						ItemAddress itemAddress = null;
						Item item = null;
						GInterface424 value = gStruct.Value;
						if (!(value is GInterface428 gInterface))
						{
							if (value is GInterface429 gInterface2)
							{
								item = gInterface2.TargetItem;
							}
						}
						else
						{
							itemAddress = gInterface.To;
						}
						if (itemAddress == null && item == null)
						{
							break;
						}
						if (Item_0 != ammoItemClass || item != Item_1)
						{
							if (Item_0 != null)
							{
								method_3(CommandStatus.Succeed);
							}
							Item_0 = ammoItemClass;
							Item_1 = item;
							method_3(CommandStatus.Begin);
						}
						await task;
						if (Boolean_0)
						{
							break;
						}
						task = method_4();
						if (Bool_0)
						{
							Float_1 = Mathf.Clamp(Float_1 - Float_0 * Singleton<BackendConfigSettingsClass>.Instance.LoadTimeSpeedProgress / 100f, Float_0 * 40f / 100f, 10f);
						}
						GStruct154<GInterface433> gStruct2 = ((Item_1 != null) ? AmmoItemClass.ApplyToAmmo(Item_0, Item_1, 1, InventoryController_0, simulate: true) : AmmoItemClass.ApplyToAddress(Item_0, itemAddress, 1, InventoryController_0, simulate: true));
						if (!gStruct2.Failed)
						{
							GInterface432 operationResult = new GClass3420(gStruct2.Value);
							GClass3514 operation = InventoryController_0.ConvertOperationResultToOperation(operationResult) as GClass3514;
							TaskCompletionSource<IResult> executionSource = new TaskCompletionSource<IResult>();
							InventoryController_0.vmethod_1(operation, delegate(IResult executeResult)
							{
								executionSource.SetResult(executeResult);
							});
							IResult result = await executionSource.Task;
							if (!result.Failed)
							{
								Int_1--;
								Item_0.RaiseRefreshEvent();
								MagazineItemClass.RaiseRefreshEvent(Int_1 == 0);
								Item_1?.RaiseRefreshEvent();
								method_1();
								continue;
							}
							return result;
						}
						return GClass1617.ToResult(gStruct2);
					}
					return GClass1617.ToResult(gStruct);
				}
				return SuccessfulResult.New;
			}

			public void method_3(CommandStatus status)
			{
				IItemOwner owner = GClass3113.GetOwner(MagazineItemClass.Parent);
				GEventArgs8 args = new GEventArgs8(Item_0, Item_1, MagazineItemClass, Int_0 - Int_1, Int_1, Float_0, status, InventoryController_0);
				owner.RaiseUnloadMagazineEvent(args);
				if (owner != InventoryController_0)
				{
					InventoryController_0.RaiseUnloadMagazineEvent(args);
				}
			}

			[CompilerGenerated]
			public Task method_4()
			{
				return Task.Delay(Mathf.CeilToInt(Float_1 * 1000f));
			}
		}

		[Serializable]
		[CompilerGenerated]
		public class Class1211
		{
			public static readonly Class1211 class1211_0 = new Class1211();

			public static Func<GClass3393, bool> func_0;

			public static Func<GClass3393, int> func_1;

			public static Func<GClass3393, bool> func_2;

			public static Func<EFT.InventoryLogic.IContainer, IEnumerable<Item>> func_3;

			public bool method_0(GClass3393 g)
			{
				return g != null;
			}

			public int method_1(GClass3393 g)
			{
				return g.Grid.GridWidth * g.Grid.GridHeight;
			}

			public bool method_2(GClass3393 x)
			{
				return x != null;
			}

			public IEnumerable<Item> method_3(EFT.InventoryLogic.IContainer x)
			{
				return x.Items;
			}
		}

		[CompilerGenerated]
		public class Class1212
		{
			public PlayerInventoryController playerInventoryController_0;

			public bool ignoreRestrictions;

			public bool method_0(GClass3248 localItem)
			{
				if (!playerInventoryController_0.List_3.Contains(localItem))
				{
					if (playerInventoryController_0.method_29(localItem, ignoreRestrictions))
					{
						return localItem.NotShownInSlot;
					}
					return true;
				}
				return false;
			}
		}

		[CompilerGenerated]
		public class Class1213
		{
			public PlayerInventoryController playerInventoryController_0;

			public IEnumerable<DestroyedItemsStruct> destroyedItems;

			public void method_0(Item itemToSubtract)
			{
				if (itemToSubtract.LimitedDiscard)
				{
					MongoID templateId = itemToSubtract.TemplateId;
					_ = playerInventoryController_0.DiscardLimits[templateId];
					int preservedNumber;
					int num = (method_1(itemToSubtract, out preservedNumber) ? preservedNumber : itemToSubtract.StackObjectsCount);
					if (num != 0)
					{
						playerInventoryController_0.DiscardLimits[templateId] -= num;
						playerInventoryController_0.LogDiscardLimitsChange(itemToSubtract.Template, -num);
					}
				}
			}

			public bool method_1(Item localItem, out int preservedNumber)
			{
				foreach (var (item2, _, num3) in destroyedItems)
				{
					if (localItem == item2)
					{
						preservedNumber = num3;
						return true;
					}
				}
				preservedNumber = 0;
				return false;
			}

			public bool method_2(Item localItem)
			{
				foreach (var (item2, _, num3) in destroyedItems)
				{
					if (localItem == item2)
					{
						return num3 > 0;
					}
				}
				return true;
			}
		}

		[CompilerGenerated]
		public class Class1214
		{
			public IEnumerable<DestroyedItemsStruct> destroyedItems;

			public PlayerInventoryController playerInventoryController_0;

			public void method_0(Item itemToAdd)
			{
				if (!itemToAdd.LimitedDiscard)
				{
					return;
				}
				int num = itemToAdd.StackObjectsCount;
				foreach (var (item2, _, num4) in destroyedItems)
				{
					if (item2 == itemToAdd)
					{
						num = num4;
						break;
					}
				}
				if (num != 0)
				{
					playerInventoryController_0.DiscardLimits[itemToAdd.TemplateId] += num;
					playerInventoryController_0.LogDiscardLimitsChange(itemToAdd.Template, num);
				}
			}

			public bool method_1(Item localItem)
			{
				foreach (var (item2, _, num3) in destroyedItems)
				{
					if (localItem == item2)
					{
						return num3 > 0;
					}
				}
				return true;
			}
		}

		[CompilerGenerated]
		public class Class1215
		{
			public MagazineItemClass magazine;

			public PlayerInventoryController playerInventoryController_0;

			public bool notify;

			public float speed;

			public void method_0(IResult result)
			{
				ItemAddress currentAddress = magazine.CurrentAddress;
				IItemOwner itemOwner = ((currentAddress != null) ? GClass3113.GetOwnerOrNull(currentAddress) : null);
				if (itemOwner == null)
				{
					return;
				}
				if (result.Succeed)
				{
					playerInventoryController_0.StrictCheckMagazine(magazine, status: true, playerInventoryController_0.Profile.MagDrillsMastering, notify);
					if (Singleton<GUISounds>.Instantiated)
					{
						Singleton<GUISounds>.Instance.PlayItemSound(magazine.ItemSound, EInventorySoundType.drop);
					}
				}
				else
				{
					UnityEngine.Debug.Log("<color=red>Check magazine operation has been cancelled</color>");
				}
				itemOwner.RaiseInventoryCheckMagazine(magazine, speed, status: false);
			}
		}

		[CompilerGenerated]
		public class Class1216
		{
			public AmmoItemClass containedAmmo;

			public TaskCompletionSource<IResult> taskSource;

			public GClass3393 method_0(StashGridClass grid)
			{
				return grid.FindLocationForItem(containedAmmo);
			}

			public void method_1(IResult result)
			{
				taskSource.SetResult(result);
			}
		}

		[CompilerGenerated]
		public class Class1217
		{
			public TaskCompletionSource<IResult> taskSource;

			public void method_0(IResult result)
			{
				taskSource.SetResult(result);
			}

			public void method_1(IResult result)
			{
				taskSource.SetResult(result);
			}
		}

		[CompilerGenerated]
		public class Class1218
		{
			public Callback callback;

			public void method_0(IResult result)
			{
				if (result.Failed)
				{
					UnityEngine.Debug.LogError(result.Error);
				}
				callback(result);
			}
		}

		[NonSerialized]
		public Player Player_0;

		[NonSerialized]
		public Interface19 Interface19_0;

		[NonSerialized]
		public Class1202 Class1202_0;

		[NonSerialized]
		public bool Bool_2 = true;

		[NonSerialized]
		public Dictionary<MongoID, int> Dictionary_0 = new Dictionary<MongoID, int>();

		[NonSerialized]
		public List<Item> List_3 = new List<Item>();

		[NonSerialized]
		[CompilerGenerated]
		public Dictionary<MongoID, int> Dictionary_1;

		[NonSerialized]
		[CompilerGenerated]
		public Profile Profile_0;

		public Dictionary<MongoID, int> DiscardLimits
		{
			[CompilerGenerated]
			get
			{
				return Dictionary_1;
			}
			[CompilerGenerated]
			set
			{
				Dictionary_1 = value;
			}
		}

		public virtual bool HasDiscardLimits => Profile.Side != EPlayerSide.Savage;

		public new Profile Profile
		{
			[CompilerGenerated]
			get
			{
				return Profile_0;
			}
		}

		public override Item ItemInHands
		{
			get
			{
				if (!(Player_0.HandsController != null))
				{
					return null;
				}
				return Player_0.HandsController.Item;
			}
		}

		public abstract IPlayerSearchController PlayerSearchController { get; }

		public override ISearchController SearchController => PlayerSearchController;

		public override bool Locked
		{
			get
			{
				return Player_0.ProcessStatus != EProcessStatus.None;
			}
			set
			{
				Player_0.ProcessStatus = (value ? EProcessStatus.Internal : EProcessStatus.None);
				UpdateLockedStatus();
			}
		}

		public PlayerInventoryController(Player player, Profile profile, bool examined)
			: base(profile, examined)
		{
			Player_0 = player;
			Profile_0 = profile;
			DiscardLimits = Profile.Inventory.DiscardLimits;
			base.RootItem.CurrentAddress = CreateItemAddress();
			if (base.QuestStashItem != null)
			{
				base.QuestStashItem.CurrentAddress = CreateItemAddress();
			}
			if (base.QuestRaidItem != null)
			{
				base.QuestRaidItem.CurrentAddress = CreateItemAddress();
			}
			if (base.Inventory.Stash != null)
			{
				base.Inventory.Stash.CurrentAddress = CreateItemAddress();
			}
		}

		public abstract SearchContentOperation vmethod_2(SearchableItemItemClass item);

		public override Task<IResult> TryRunNetworkTransaction(GStruct153 operationResult, Callback callback = null)
		{
			if (Player_0.HealthController.IsAlive)
			{
				return base.TryRunNetworkTransaction(operationResult, callback);
			}
			TaskCompletionSource<IResult> taskCompletionSource = new TaskCompletionSource<IResult>();
			taskCompletionSource.Fail("Player is dead");
			return taskCompletionSource.Task;
		}

		public virtual bool HasDiscardLimit(Item item, out int limit)
		{
			if (HasDiscardLimits && item.LimitedDiscard)
			{
				limit = DiscardLimits[item.TemplateId];
				return true;
			}
			limit = 0;
			return false;
		}

		public virtual void ResetDiscardLimits()
		{
			if (HasDiscardLimits)
			{
				DiscardLimits = Singleton<ItemFactoryClass>.Instance.GetDiscardLimits();
			}
		}

		public virtual IEnumerable<DestroyedItemsStruct> GetItemsOverDiscardLimit(Item item)
		{
			if (!HasDiscardLimits)
			{
				yield break;
			}
			foreach (var (key, value) in DiscardLimits)
			{
				Dictionary_0[key] = value;
			}
			bool ignoreRestrictions = Player_0.HealthController?.IsAlive ?? false;
			List_3.Clear();
			foreach (Item allItem in GClass3380.GetAllItems(item, (GClass3248 localItem) => !List_3.Contains(localItem) && (!method_29(localItem, ignoreRestrictions) || localItem.NotShownInSlot), (EFT.InventoryLogic.IContainer container) => !(container is Slot slot) || !slot.IsSpecial))
			{
				if (!method_29(allItem, ignoreRestrictions) && method_28(allItem, out var overLimit))
				{
					yield return overLimit;
				}
			}
		}

		public bool method_28(Item item, out DestroyedItemsStruct overLimit)
		{
			overLimit = default(DestroyedItemsStruct);
			if (!item.LimitedDiscard)
			{
				return false;
			}
			MongoID templateId = item.TemplateId;
			int stackObjectsCount = item.StackObjectsCount;
			if (!Dictionary_0.TryGetValue(templateId, out var value))
			{
				Logger.LogError($"Personal discard limits missing for {item}");
				return false;
			}
			int num = value - stackObjectsCount;
			if (num >= 0)
			{
				Dictionary_0[templateId] = num;
				return false;
			}
			Dictionary_0[templateId] = 0;
			List_3.Add(item);
			int num2 = Math.Min(-num, stackObjectsCount);
			overLimit = new DestroyedItemsStruct(item, num2, stackObjectsCount - num2);
			return true;
		}

		public bool method_29(Item item, bool ignoreRestrictions)
		{
			if (!ignoreRestrictions && item.CurrentAddress?.Container is Slot slot && slot.ParentItem is InventoryEquipment)
			{
				if (item.TryGetItemComponent<UnlootableComponent>(out var component) && component.IsUnlootableFrom(slot))
				{
					return true;
				}
				if (!(item is GClass3248))
				{
					return false;
				}
				if (item.TryGetItemComponent<CantRemoveFromSlotsDuringRaidComponent>(out var component2))
				{
					return !component2.CanRemoveFromSlotDuringRaid(slot.ID);
				}
				return false;
			}
			return false;
		}

		public virtual void SubtractFromDiscardLimits(Item rootItem, IEnumerable<DestroyedItemsStruct> destroyedItems)
		{
			Class1213 CS_0024_003C_003E8__locals4 = new Class1213();
			CS_0024_003C_003E8__locals4.playerInventoryController_0 = this;
			CS_0024_003C_003E8__locals4.destroyedItems = destroyedItems;
			if (!HasDiscardLimits)
			{
				return;
			}
			foreach (Item allItem in GClass3380.GetAllItems(rootItem, (Predicate<GClass3248>)delegate(Item localItem)
			{
				foreach (var (item2, _, num3) in CS_0024_003C_003E8__locals4.destroyedItems)
				{
					if (localItem == item2)
					{
						return num3 > 0;
					}
				}
				return true;
			}))
			{
				CS_0024_003C_003E8__locals4.method_0(allItem);
			}
		}

		public virtual void LogDiscardLimitsChange(ItemTemplate template, int delta)
		{
		}

		public virtual void AddDiscardLimits(Item rootItem, IEnumerable<DestroyedItemsStruct> destroyedItems)
		{
			Class1214 CS_0024_003C_003E8__locals4 = new Class1214();
			CS_0024_003C_003E8__locals4.destroyedItems = destroyedItems;
			CS_0024_003C_003E8__locals4.playerInventoryController_0 = this;
			if (!HasDiscardLimits)
			{
				return;
			}
			foreach (Item allItem in GClass3380.GetAllItems(rootItem, (Predicate<GClass3248>)delegate(Item localItem)
			{
				foreach (var (item2, _, num3) in CS_0024_003C_003E8__locals4.destroyedItems)
				{
					if (localItem == item2)
					{
						return num3 > 0;
					}
				}
				return true;
			}))
			{
				CS_0024_003C_003E8__locals4.method_0(allItem);
			}
		}

		public override GStruct156<Item> FindItemById(MongoID itemId, bool checkDistance = true, bool checkOwnership = true)
		{
			return Player_0.FindItemById(itemId, checkDistance, checkOwnership);
		}

		public override void OutProcess(TraderControllerClass executor, Item item, ItemAddress from, ItemAddress to, GInterface438 operation, Callback callback)
		{
			if (!executor.CheckTransferOwners(item, to, out var error))
			{
				callback.Fail(error.ToString());
			}
			else
			{
				method_34(item, from, to, operation, callback);
			}
		}

		public override void InProcess(TraderControllerClass executor, Item item, ItemAddress to, bool succeed, GInterface438 operation, Callback callback)
		{
			if (!succeed)
			{
				callback.Succeed();
				return;
			}
			if (!executor.CheckTransferOwners(item, to, out var error))
			{
				callback.Fail(error.ToString());
				return;
			}
			method_33(item, to, operation, callback);
			Player_0.StatisticsManager.OnGrabLoot(item);
		}

		public void SetNextProcessLocked(bool status)
		{
			Bool_2 = status;
		}

		public override void InventoryCheckMagazine(MagazineItemClass magazine, bool notify)
		{
			StopProcesses();
			float num = 100f - (float)Profile.Skills.MagDrillsInventoryCheckSpeed + magazine.CheckTimeModifier;
			float speed = Singleton<BackendConfigSettingsClass>.Instance.BaseCheckTime * num / 100f;
			UnityEngine.Debug.Log("<color=cyan>Perform CHECK with speed (" + speed + ")</color>");
			GClass3113.GetOwner(magazine.Parent).RaiseInventoryCheckMagazine(magazine, speed, status: true);
			Class1202_0 = new Class1202(magazine, speed, delegate(IResult result)
			{
				ItemAddress currentAddress = magazine.CurrentAddress;
				IItemOwner itemOwner = ((currentAddress != null) ? GClass3113.GetOwnerOrNull(currentAddress) : null);
				if (itemOwner != null)
				{
					if (result.Succeed)
					{
						StrictCheckMagazine(magazine, status: true, Profile.MagDrillsMastering, notify);
						if (Singleton<GUISounds>.Instantiated)
						{
							Singleton<GUISounds>.Instance.PlayItemSound(magazine.ItemSound, EInventorySoundType.drop);
						}
					}
					else
					{
						UnityEngine.Debug.Log("<color=red>Check magazine operation has been cancelled</color>");
					}
					itemOwner.RaiseInventoryCheckMagazine(magazine, speed, status: false);
				}
			});
		}

		public override async Task<IResult> LoadMultiBarrelWeapon(Weapon weapon, AmmoItemClass ammo, int ammoCount)
		{
			if (Player_0.HandsController is IFirearmHandsController firearmHandsController && firearmHandsController.Item == weapon)
			{
				if (!firearmHandsController.CanStartReload())
				{
					return new FailedResult("Can not load");
				}
				if (!weapon.IsMultiBarrel)
				{
					return new FailedResult("Can not load into not multi barrel weapon");
				}
				TaskCompletionSource<IResult> taskSource = new TaskCompletionSource<IResult>();
				Item containedItem = weapon.FirstFreeChamberSlot.ContainedItem;
				AmmoItemClass containedAmmo = containedItem as AmmoItemClass;
				GClass3393 placeToPutContainedAmmoMagazine = ((containedAmmo == null || containedAmmo.IsUsed) ? null : (from grid in GClass3372.GetPrioritizedGridsForUnloadedObject(base.Inventory.Equipment)
					select grid.FindLocationForItem(containedAmmo) into g
					where g != null
					orderby g.Grid.GridWidth * g.Grid.GridHeight
					select g).FirstOrDefault((GClass3393 x) => x != null));
				firearmHandsController.ReloadBarrels(new AmmoPackReloadingClass(new List<AmmoItemClass> { ammo }), placeToPutContainedAmmoMagazine, delegate(IResult result)
				{
					taskSource.SetResult(result);
				});
				return await taskSource.Task;
			}
			return await base.LoadMultiBarrelWeapon(weapon, ammo, ammoCount);
		}

		public override async Task<IResult> LoadWeaponWithAmmo(Weapon weapon, AmmoItemClass ammo, int ammoCount)
		{
			if (Player_0.HandsController is IFirearmHandsController firearmHandsController && firearmHandsController.Item == weapon)
			{
				if (!firearmHandsController.CanStartReload())
				{
					return new FailedResult("Can not load");
				}
				if (!weapon.SupportsInternalReload)
				{
					return new FailedResult("Can nol load into inserted magazine");
				}
				TaskCompletionSource<IResult> taskSource = new TaskCompletionSource<IResult>();
				if (weapon is RevolverItemClass)
				{
					firearmHandsController.ReloadCylinderMagazine(new AmmoPackReloadingClass(new List<AmmoItemClass> { ammo }), delegate(IResult result)
					{
						taskSource.SetResult(result);
					}, quickReload: false);
				}
				else
				{
					firearmHandsController.ReloadWithAmmo(new AmmoPackReloadingClass(new List<AmmoItemClass> { ammo }), delegate(IResult result)
					{
						taskSource.SetResult(result);
					});
				}
				return await taskSource.Task;
			}
			return await base.LoadWeaponWithAmmo(weapon, ammo, ammoCount);
		}

		public override void StrictCheckMagazine(MagazineItemClass magazine, bool status, int skill = 0, bool notify = false, bool useOperation = true)
		{
			if (status)
			{
				if (magazine.Count <= 0 || magazine.Count >= magazine.MaxCount)
				{
					skill = 2;
				}
				if (notify && !Profile.CheckedMagazines.ContainsKey(magazine.Id))
				{
					NotifyMagazineChecked(magazine.ShortName);
				}
			}
			SetMagazineCheckedStatus(magazine, status, skill, useOperation);
		}

		public override async Task<IResult> LoadMagazine(AmmoItemClass sourceAmmo, MagazineItemClass magazine, int loadCount, bool ignoreRestrictions)
		{
			if (loadCount <= 0)
			{
				return new FailedResult("Can not load 0 bullets.");
			}
			StopProcesses();
			float num = 100f - (float)Profile.Skills.MagDrillsLoadSpeed + magazine.LoadUnloadModifier;
			float num2 = Singleton<BackendConfigSettingsClass>.Instance.BaseLoadTime * num / 100f;
			UnityEngine.Debug.LogFormat("<color=cyan>Perform LOAD with speed ({0})</color>", num2);
			GStruct153 gStruct = (ignoreRestrictions ? magazine.ApplyWithoutRestrictions(this, sourceAmmo, 1, simulate: true) : magazine.Apply(this, sourceAmmo, 1, simulate: true));
			if (gStruct.Failed || !CanExecute(gStruct.Value))
			{
				return GClass1617.ToResult(gStruct);
			}
			IResult result = await method_30();
			if (result.Failed)
			{
				return result;
			}
			Interface19_0 = new Class1204(this, magazine, sourceAmmo, loadCount, Profile.Skills.MagDrillsLoadProgression, num2);
			IResult result2 = await Interface19_0.Start();
			Interface19_0 = null;
			return result2;
		}

		public override async Task<IResult> UnloadMagazine(MagazineItemClass magazine, bool equipmentBlocked)
		{
			StopProcesses();
			float num = 100f - (float)Profile.Skills.MagDrillsUnloadSpeed + magazine.LoadUnloadModifier;
			float num2 = Singleton<BackendConfigSettingsClass>.Instance.BaseUnloadTime * num / 100f;
			UnityEngine.Debug.LogFormat("<color=cyan>Perform UNLOAD with speed ({0})</color>", num2);
			IResult result = await method_30();
			if (result.Failed)
			{
				return result;
			}
			Interface19_0 = new Class1207(this, magazine, num2, elite: false);
			IResult result2 = await Interface19_0.Start();
			Interface19_0 = null;
			return result2;
		}

		public async Task<IResult> method_30()
		{
			await TasksExtensions.WaitUntil(() => Interface19_0 == null);
			IResult result;
			if (!Bool_2)
			{
				result = SuccessfulResult.New;
			}
			else
			{
				IResult result2 = new FailedResult("Next process is locked.");
				result = result2;
			}
			return result;
		}

		public override void StopProcesses()
		{
			Interface19_0?.Proceed();
			Class1202_0?.Proceed();
		}

		public void method_31(Item magazineOrAmmo)
		{
			Interface19_0?.TryProceedForItem(magazineOrAmmo);
		}

		public void method_32(MagazineItemClass magazine)
		{
			Class1202_0?.TryProceedForItem(magazine);
		}

		public override void ThrowItem(Item item, bool downDirection = false, Callback callback = null)
		{
			if (item is Weapon weapon)
			{
				CheckChamber(weapon, status: false);
				MagazineItemClass currentMagazine = weapon.GetCurrentMagazine();
				if (currentMagazine != null)
				{
					StrictCheckMagazine(currentMagazine, status: false);
					method_32(currentMagazine);
					method_31(currentMagazine);
				}
			}
			if (item is GClass3248 topLevelCollection)
			{
				foreach (Item item2 in GClass3380.GetAllItemsFromCollection(topLevelCollection))
				{
					if (!(item2 is MagazineItemClass magazineItemClass))
					{
						if (item2 is AmmoItemClass magazineOrAmmo)
						{
							method_31(magazineOrAmmo);
						}
					}
					else
					{
						method_32(magazineItemClass);
						method_31(magazineItemClass);
					}
				}
			}
			GStruct154<GClass3406> gStruct = InteractionsHandlerClass.Throw(item, this, simulate: true);
			if (gStruct.Failed)
			{
				callback?.Invoke(GClass1617.ToResult(gStruct));
			}
			else
			{
				vmethod_1(new ThrowOperationClass(method_12(), this, gStruct.Value, gStruct.Value.ItemsToDestroy, Player_0, downDirection), callback);
			}
		}

		public override ToggleOperationClass ToggleItem(GClass3430 toggleResult)
		{
			if (Player_0.HealthController.FindActiveEffect<GInterface376>() == null)
			{
				return new Class2500(method_12(), this, toggleResult, Player_0);
			}
			return base.ToggleItem(toggleResult);
		}

		public override void SetupItem(Item item, string zone, Vector3 position, Quaternion rotation, float setupTime, Callback callback = null)
		{
			GStruct154<GClass3408> gStruct = InteractionsHandlerClass.Discard(item, this, simulate: true);
			if (gStruct.Failed)
			{
				if (callback != null)
				{
					callback(GClass1617.ToResult(gStruct));
				}
				else
				{
					Logger.LogError(gStruct.Error.ToString());
				}
			}
			else
			{
				vmethod_1(new GClass3498(method_12(), this, gStruct.Value, zone, position, rotation, Player_0, setupTime), callback);
			}
		}

		public override void PlantTripwire(ThrowWeapItemClass grenade, PlantingKitsItemClass plantingKit, Vector3 fromPosition, Vector3 toPosition, Callback callback = null)
		{
			GStruct154<GClass3407> gStruct = InteractionsHandlerClass.SimulatePlantTripwire(this, grenade, plantingKit);
			if (gStruct.Failed)
			{
				if (callback != null)
				{
					callback(GClass1617.ToResult(gStruct));
				}
				else
				{
					Logger.LogError(gStruct.Error.ToString());
				}
			}
			else
			{
				vmethod_1(new GClass3492(method_12(), this, gStruct.Value, fromPosition, toPosition, Player_0), callback);
			}
		}

		public override void CheckMagazineAmmoDepend(MagazineItemClass magazine, Action callback, bool useOperation, bool allowUncheck = false)
		{
			if (magazine.Count > 0 && magazine.Count < magazine.MaxCount)
			{
				if (allowUncheck && Player_0.Profile.CheckedMagazines.ContainsKey(magazine.Id))
				{
					StrictCheckMagazine(magazine, status: false, 0, notify: false, useOperation);
				}
			}
			else if (!Player_0.Profile.CheckedMagazines.ContainsKey(magazine.Id))
			{
				StrictCheckMagazine(magazine, status: true, 2, notify: false, useOperation);
			}
			callback();
		}

		public override GStruct155 CheckItemAction(Item item, ItemAddress location)
		{
			if (IsInventoryBlocked())
			{
				return new GClass1569();
			}
			return base.CheckItemAction(item, location);
		}

		public void method_33(Item item, ItemAddress to, GInterface438 operation, Callback callback)
		{
			Player_0.TrySetInHands(item, to, operation, delegate(IResult result)
			{
				if (result.Failed)
				{
					UnityEngine.Debug.LogError(result.Error);
				}
				callback(result);
			});
		}

		public void method_34(Item item, ItemAddress from, ItemAddress to, GInterface438 abstractOperation, Callback callback)
		{
			Item item2 = method_35(item, from, to);
			if (item2 != null)
			{
				Player_0.TryRemoveFromHands(item2, abstractOperation, callback);
			}
			else
			{
				callback.Succeed();
			}
		}

		public override bool vmethod_0(BaseInventoryOperationClass operation)
		{
			return Player_0.HandsController.CanExecute(operation);
		}

		public Item method_35(Item item, ItemAddress from, ItemAddress to)
		{
			List<Item> list = new List<Item> { item };
			if (item is GClass3248 gClass)
			{
				list.AddRange(gClass.Containers.SelectMany((EFT.InventoryLogic.IContainer x) => x.Items));
			}
			Item item2 = list.FirstOrDefault((Item x) => ItemInHands == x);
			if (item2 == null && ItemInHands is Weapon rootItem)
			{
				if (GClass3380.IsChildOf(from, rootItem))
				{
					item2 = item;
				}
				if (GClass3380.IsChildOf(to, rootItem))
				{
					item2 = item;
				}
			}
			if (item2 != null)
			{
				return item2;
			}
			return (to != null || !Player_0.InventoryController.IsAnimatedSlot(from)) ? null : item;
		}

		[CompilerGenerated]
		public static bool smethod_1(EFT.InventoryLogic.IContainer container)
		{
			if (container is Slot slot)
			{
				return !slot.IsSpecial;
			}
			return true;
		}

		[CompilerGenerated]
		[DebuggerHidden]
		public Task<IResult> method_36(Weapon weapon, AmmoItemClass ammo, int ammoCount)
		{
			return base.LoadMultiBarrelWeapon(weapon, ammo, ammoCount);
		}

		[CompilerGenerated]
		[DebuggerHidden]
		public Task<IResult> method_37(Weapon weapon, AmmoItemClass ammo, int ammoCount)
		{
			return base.LoadWeaponWithAmmo(weapon, ammo, ammoCount);
		}

		[CompilerGenerated]
		public bool method_38()
		{
			return Interface19_0 == null;
		}

		[CompilerGenerated]
		public bool method_39(Item x)
		{
			return ItemInHands == x;
		}
	}

	public abstract class PlayerMovementConstantsClass
	{
		public const float SPEED_MIN = 0f;

		public const float SPEED_MAX = 0.7f;

		public const float MAX_SPRINTING_SPEED = 2f;

		public const float SPEED_MAX_DELTA = 0.3f;

		public static readonly int FIRST_PERSON_ACTION = Animator.StringToHash("FirstAction");

		public static readonly Vector2 STAND_POSE_ROTATION_PITCH_RANGE = new Vector2(-90f, 90f);

		public static readonly Vector2 PRONE_POSE_ROTATION_PITCH_RANGE = new Vector2(-16f, 25f);

		public static readonly Vector2 ROLL_POSE_ROTATION_PITCH_RANGE = new Vector2(-16f, 2f);

		public static readonly Vector2 FULL_YAW_RANGE = new Vector2(-360f, 360f);

		public const float POSE_RANGE_MIN = 0f;

		public const float POSE_RANGE_MAX = 1f;

		public const float POSE_THRESHOLD = 0.5f;

		public const float TILT_RANGE_MIN = -5f;

		public const float TILT_RANGE_MAX = 5f;

		public const int STEP_RANGE_MIN = -1;

		public const int STEP_RANGE_MAX = 1;

		public const float LEAN_SPEED = 5f;

		public const float SLOW_LEEN_SPEED = 0.1f;
	}

	public enum EMouseSensitivityModifier
	{
		Armor
	}

	public enum LeanType
	{
		NormalLean,
		SlowLean
	}

	public enum ESpeedLimit
	{
		BarbedWire,
		HealthCondition,
		Aiming,
		Weight,
		SurfaceNormal,
		Swamp,
		Shot,
		Armor,
		Fall
	}

	[Serializable]
	public class ValueBlender
	{
		[NonSerialized]
		public float Target_1;

		[NonSerialized]
		public float StartTime;

		[NonSerialized]
		public float StartValue;

		public float Speed = 1f;

		public virtual float Target
		{
			get
			{
				return Target_1;
			}
			set
			{
				if (Target != value)
				{
					StartValue = Value;
					StartTime = Time.time;
					Target_1 = value;
				}
			}
		}

		public virtual float Value
		{
			get
			{
				return Mathf.Clamp01(StartValue + (Time.time - StartTime) * Speed * Mathf.Sign(Target_1 - 0.5f));
			}
			set
			{
				StartTime = Time.time;
				StartValue = value;
			}
		}

		public ValueBlender(int defaultValue = 0)
		{
			Target_1 = defaultValue;
			StartValue = defaultValue;
		}
	}

	[Serializable]
	public class BetterValueBlender : ValueBlender
	{
		public override float Value
		{
			get
			{
				float num = Mathf.Max(0f, Time.time - StartTime);
				if (!(StartValue > Target))
				{
					return Mathf.Clamp(StartValue + num * Speed * Mathf.Sign(Target - StartValue), StartValue, Target);
				}
				return Mathf.Clamp(StartValue + num * Speed * Mathf.Sign(Target - StartValue), Target, StartValue);
			}
			set
			{
				StartValue = value;
				Target_1 = value;
				StartTime = Time.time;
			}
		}

		public void ChangeValue(float value, float delay)
		{
			StartTime = Time.time + delay;
			StartValue = value;
		}
	}

	[Serializable]
	public class ValueBlenderDelay : ValueBlender
	{
		public float Delay;

		public override float Target
		{
			get
			{
				return Target_1;
			}
			set
			{
				if (Target != value)
				{
					StartValue = Value;
					StartTime = Time.time + Delay;
					Target_1 = value;
				}
			}
		}
	}

	public delegate void GDelegate65(float damage, EBodyPart part, EDamageType type, float absorbed, MaterialType special);

	public class GClass2004
	{
		public Vector3 Shift;

		public Transform Transform;

		public CommonTransportee Transportee;

		public void RemovePhysics()
		{
			if ((bool)Transportee)
			{
				UnityEngine.Object.Destroy(Transportee);
			}
		}

		public void RestoreShift()
		{
			foreach (Transform item in Transform)
			{
				item.localPosition += Shift;
			}
		}

		public void Destroy()
		{
			RemovePhysics();
			RestoreShift();
			Transform = null;
			Transportee = null;
		}
	}

	public class EmptyHandsController : ItemHandsController, GInterface198, IHandsController, GInterface197
	{
		public class Class1259 : Class1258
		{
			[NonSerialized]
			public Callback Callback_0;

			public Class1259(EmptyHandsController controller)
				: base(controller)
			{
			}

			public virtual void Start(Item item, Callback callback)
			{
				Callback_0 = callback;
				Start();
				EmptyHandsController_0.SetInventoryOpened(opened: false);
				EmptyHandsController_0._player.SendHandsInteractionStateChanged(value: true, 300);
				Player_0.MovementContext.SetInteractInHands(EInteraction.DropBackpack);
			}

			public override void Reset()
			{
				Callback_0 = null;
				base.Reset();
			}

			public override void OnBackpackDrop()
			{
				State = EOperationState.Finished;
				EmptyHandsController_0._player.SendHandsInteractionStateChanged(value: false, 300);
				Player_0.MovementContext.SetInteractInHands(EInteraction.DropBackpack);
				EmptyHandsController_0.firearmsAnimator_0.SetInventory(EmptyHandsController_0.bool_0);
				WeaponAnimationSpeedControllerClass.ResetTriggerHandReady(EmptyHandsController_0.firearmsAnimator_0.Animator);
				EmptyHandsController_0.InitiateOperation<Class1261>().Start();
				Callback_0.Succeed();
			}

			public override void SetInventoryOpened(bool opened)
			{
				EmptyHandsController_0.bool_0 = opened;
			}
		}

		public abstract class Class1258 : BaseAnimationOperationClass
		{
			[NonSerialized]
			public Player Player_0;

			[NonSerialized]
			public EmptyHandsController EmptyHandsController_0;

			public Class1258(EmptyHandsController controller)
				: base(controller)
			{
				EmptyHandsController_0 = controller;
				Player_0 = EmptyHandsController_0._player;
			}

			public new void Start()
			{
				base.Start();
			}

			public virtual void HideWeaponComplete()
			{
				method_0();
			}

			public virtual void WeaponAppeared()
			{
				method_0();
			}

			public virtual void OnBackpackDrop()
			{
				method_0();
			}

			public virtual void HideWeapon(Action onHidden)
			{
				method_0();
			}

			public virtual void ExamineWeapon()
			{
				method_0();
			}

			public virtual void SetEmptyHandsCompassState(bool active)
			{
				method_0();
			}

			public virtual void FastForward()
			{
			}

			public virtual void SetInventoryOpened(bool opened)
			{
				EmptyHandsController_0.bool_0 = opened;
				EmptyHandsController_0.firearmsAnimator_0.SetInventory(opened);
			}

			public virtual bool CanExecute(GInterface438 operation)
			{
				if (!(operation is GInterface443 gInterface))
				{
					return true;
				}
				if (EmptyHandsController_0._player.InventoryController.IsAnimatedSlot(gInterface.From1))
				{
					return false;
				}
				return true;
			}

			public virtual void Execute(GInterface438 operation, Callback callback)
			{
				method_0();
				if (!(operation is GInterface443 gInterface))
				{
					callback.Succeed();
				}
				else if (EmptyHandsController_0._player.InventoryController.IsAnimatedSlot(gInterface.From1))
				{
					callback?.Fail($"Detach is not supported in current operation: {GetType()}");
				}
				else
				{
					callback.Succeed();
				}
			}
		}

		public class Class1261 : Class1258
		{
			[NonSerialized]
			public const float Float_0 = 300f;

			[NonSerialized]
			public float Float_1;

			public Class1261(EmptyHandsController controller)
				: base(controller)
			{
			}

			public new void Start()
			{
				base.Start();
				Float_1 = 0f;
			}

			public override void Reset()
			{
				Float_1 = 0f;
				base.Reset();
			}

			public override void HideWeapon(Action onHidden)
			{
				State = EOperationState.Finished;
				EmptyHandsController_0.InitiateOperation<Class1262>().Start(onHidden);
			}

			public override void OnEnd()
			{
				EmptyHandsController_0.SetCompassState(active: false);
			}

			public override bool CanExecute(GInterface438 operation)
			{
				return true;
			}

			public override void Execute(GInterface438 operation, Callback callback)
			{
				if (operation is GInterface443 gInterface && EmptyHandsController_0._player.InventoryController.IsAnimatedSlot(gInterface.From1) && !gInterface.From1.Equals(gInterface.To1))
				{
					State = EOperationState.Finished;
					EmptyHandsController_0.InitiateOperation<Class1259>().Start(gInterface.Item1, callback);
				}
				else
				{
					callback.Succeed();
				}
			}

			public override void Update(float deltaTime)
			{
				Float_1 += deltaTime;
				if (Float_1 > 300f)
				{
					EmptyHandsController_0.firearmsAnimator_0.Idle();
					Float_1 = 0f;
				}
			}

			public override void ExamineWeapon()
			{
				EmptyHandsController_0.firearmsAnimator_0.LookTrigger();
			}

			public override void SetEmptyHandsCompassState(bool active)
			{
				EmptyHandsController_0.CompassState.Value = active;
			}
		}

		public class Class1262 : Class1258
		{
			[NonSerialized]
			public Action Action_0;

			public Class1262(EmptyHandsController controller)
				: base(controller)
			{
			}

			public void Start(Action onHidden)
			{
				Action_0 = onHidden;
				Start();
				EmptyHandsController_0._player.BodyAnimatorCommon.SetFloat(PlayerAnimator.RELOAD_FLOAT_PARAM_HASH, 1f);
				HideWeaponComplete();
			}

			public override void Reset()
			{
				Action_0 = null;
				base.Reset();
			}

			public override bool CanExecute(GInterface438 operation)
			{
				return true;
			}

			public override void Execute(GInterface438 operation, Callback callback)
			{
				if (operation is GInterface443 gInterface && EmptyHandsController_0._player.InventoryController.IsAnimatedSlot(gInterface.From1) && !gInterface.From1.Equals(gInterface.To1))
				{
					State = EOperationState.Finished;
					EmptyHandsController_0.InitiateOperation<Class1259>().Start(gInterface.Item1, callback);
				}
				else
				{
					callback.Succeed();
				}
			}

			public override void HideWeaponComplete()
			{
				State = EOperationState.Finished;
				Action_0();
			}

			public override void HideWeapon(Action onHidden)
			{
				Action_0 = (Action)Delegate.Combine(Action_0, onHidden);
			}

			public override void FastForward()
			{
				if (State != EOperationState.Finished)
				{
					HideWeaponComplete();
				}
			}
		}

		public class Class1260 : Class1259
		{
			[NonSerialized]
			public const float Float_0 = 0.01f;

			[NonSerialized]
			public float Float_1;

			[NonSerialized]
			public bool Bool_0;

			public Class1260(EmptyHandsController controller)
				: base(controller)
			{
			}

			public override void Start(Item item, Callback callback)
			{
				Float_1 = 0f;
				Bool_0 = false;
				base.Start(item, callback);
			}

			public override void FastForward()
			{
				if (!Bool_0)
				{
					Bool_0 = true;
					OnBackpackDrop();
				}
			}

			public override void Update(float deltaTime)
			{
				base.Update(deltaTime);
				if (!Bool_0 && Float_1 > 0.01f)
				{
					Bool_0 = true;
					OnBackpackDrop();
				}
				Float_1 += deltaTime;
			}
		}

		public class Class1263 : Class1258
		{
			[NonSerialized]
			public Action Action_0;

			[NonSerialized]
			public bool Bool_0;

			[NonSerialized]
			public Action Action_1;

			public Class1263(EmptyHandsController controller)
				: base(controller)
			{
			}

			public void Start(Action callback)
			{
				Action_1 = callback;
				Bool_0 = false;
				Player_0.BodyAnimatorCommon.SetFloat(PlayerAnimator.WEAPON_SIZE_MODIFIER_PARAM_HASH, 1f);
				EmptyHandsController_0._player.BodyAnimatorCommon.SetFloat(PlayerAnimator.RELOAD_FLOAT_PARAM_HASH, 1f);
				Start();
				EmptyHandsController_0.firearmsAnimator_0.SetActiveParam(active: true);
			}

			public override void Reset()
			{
				base.Reset();
				Action_1 = null;
				Action_0 = null;
				Bool_0 = false;
			}

			public override void WeaponAppeared()
			{
				EmptyHandsController_0.SetupProp();
				State = EOperationState.Finished;
				EmptyHandsController_0._player.BodyAnimatorCommon.SetFloat(PlayerAnimator.RELOAD_FLOAT_PARAM_HASH, 0f);
				if (!Bool_0)
				{
					EmptyHandsController_0.InitiateOperation<Class1261>().Start();
				}
				else
				{
					EmptyHandsController_0.InitiateOperation<Class1262>().Start(Action_0);
				}
				Action action_ = Action_1;
				Action_1 = null;
				action_?.Invoke();
			}

			public override void HideWeapon(Action onHidden)
			{
				Action_0 = onHidden;
				Bool_0 = true;
				WeaponAppeared();
			}

			public override void FastForward()
			{
				if (State != EOperationState.Finished)
				{
					WeaponAppeared();
				}
			}

			public override void SetLeftStanceAnimOnStartOperation()
			{
				Player_0.MovementContext.LeftStanceController.DisableLeftStanceAnimFromHandsAction();
			}
		}

		[CompilerGenerated]
		public class Class1220<T> where T : EmptyHandsController
		{
			public T controller;

			public void method_0()
			{
				controller.firearmsAnimator_0.RemoveEventsConsumer(controller);
			}
		}

		[CompilerGenerated]
		public class Class1221
		{
			public EmptyHandsController emptyHandsController_0;

			public Action callback;

			public void method_0()
			{
				emptyHandsController_0._player.StartCoroutine(emptyHandsController_0.method_4(callback));
			}
		}

		[CompilerGenerated]
		public class Class1222
		{
			public Action callback;

			public void method_0()
			{
				callback();
			}
		}

		private GClass2086 gclass2086_0;

		private FirearmsAnimator firearmsAnimator_0;

		private bool bool_0;

		public Class1258 Class1258_0 => base.CurrentHandsOperation as Class1258;

		public virtual bool Boolean_0 => true;

		public override FirearmsAnimator FirearmsAnimator => firearmsAnimator_0;

		public override string LoggerDistinctId => $"{_player.ProfileId}|{_player.Profile.Info.Nickname}|{this}";

		public new GClass3365 Item => base.Item as GClass3365;

		public override float GetAnimatorFloatParam(int hash)
		{
			return firearmsAnimator_0.GetAnimatorParameter(hash);
		}

		public override bool SupportPickup()
		{
			return true;
		}

		public override void Pickup(bool p)
		{
			firearmsAnimator_0.SetPickup(p);
		}

		public override void Interact(bool isInteracting, int actionIndex)
		{
			_player.SendHandsInteractionStateChanged(isInteracting, actionIndex);
			firearmsAnimator_0.SetInteract(isInteracting, actionIndex);
		}

		public override void SetInventoryOpened(bool opened)
		{
			if (opened)
			{
				SetCompassState(active: false);
			}
			Class1258_0.SetInventoryOpened(opened);
			_player.CurrentManagedState?.OnInventory(opened);
		}

		public override void Loot(bool p)
		{
			firearmsAnimator_0.SetLooting(p);
		}

		public override bool IsInInteraction()
		{
			return firearmsAnimator_0.IsInInteraction;
		}

		public override bool IsInInteractionStrictCheck()
		{
			if (!IsInInteraction() && !(firearmsAnimator_0.GetLayerWeight(firearmsAnimator_0.LACTIONS_LAYER_INDEX) >= float.Epsilon))
			{
				return firearmsAnimator_0.Animator.IsInTransition(firearmsAnimator_0.LACTIONS_LAYER_INDEX);
			}
			return true;
		}

		public override void Destroy()
		{
			SetPropVisibility(isVisible: false);
			_player.ProceduralWeaponAnimation.ClearPreviousWeapon();
			base.Destroy();
			firearmsAnimator_0 = null;
			AssetPoolObject.ReturnToPool(_controllerObject.gameObject);
		}

		public override bool CanExecute(GInterface438 operation)
		{
			return Class1258_0.CanExecute(operation);
		}

		public override void Execute(GInterface438 operation, Callback callback)
		{
			Class1258_0.Execute(operation, callback);
		}

		public virtual void ExamineWeapon()
		{
			Class1258_0.ExamineWeapon();
		}

		public override void SetCompassState(bool active)
		{
			if (CanChangeCompassState(active))
			{
				Class1258_0.SetEmptyHandsCompassState(active);
			}
		}

		public override bool CanRemove()
		{
			return true;
		}

		public override void ShowGesture(EInteraction gesture)
		{
			if (gesture != EInteraction.None)
			{
				firearmsAnimator_0.Gesture(gesture);
			}
		}

		public static T smethod_6<T>(Player player) where T : EmptyHandsController
		{
			GClass3365 item = new GClass3365(MongoID.Generate(), new GClass3247());
			T val = ItemHandsController.smethod_0<T>(player, item);
			smethod_8(val, player);
			return val;
		}

		public static async Task<T> smethod_7<T>(Player player) where T : EmptyHandsController
		{
			GClass3365 item = new GClass3365(MongoID.Generate(), new GClass3247());
			T obj = await ItemHandsController.smethod_2<T>(player, item);
			smethod_8(obj, player);
			return obj;
		}

		public static void smethod_8<T>(T controller, Player player) where T : EmptyHandsController
		{
			WeaponPrefab componentInChildren = controller._controllerObject.GetComponentInChildren<WeaponPrefab>();
			controller.gclass2086_0 = componentInChildren.ObjectInHands;
			controller._controllerObject.transform.SetParent(player.PlayerBones.WeaponRoot.Original.parent);
			player.ProceduralWeaponAnimation.ClearPreviousWeapon();
			player.ProceduralWeaponAnimation.InitTransforms(controller.HandsHierarchy);
			controller.gclass2086_0.AfterGetFromPoolInit(controller._player.ProceduralWeaponAnimation, null, player.IsYourPlayer);
			controller.firearmsAnimator_0 = componentInChildren.FirearmsAnimator;
			controller.firearmsAnimator_0.AddEventsConsumer(controller);
			controller.CompositeDisposable.AddDisposable(delegate
			{
				controller.firearmsAnimator_0.RemoveEventsConsumer(controller);
			});
			controller._player.HandsAnimator = controller.firearmsAnimator_0;
		}

		public override void IEventsConsumerOnWeapIn()
		{
			method_2();
		}

		public override void IEventsConsumerOnWeapOut()
		{
			method_1();
		}

		public override void IEventsConsumerOnThirdAction(int intParam)
		{
			TranslateAnimatorParameter(intParam);
		}

		public override void IEventsOnBackpackDrop()
		{
			method_3();
		}

		public override void IEventsConsumerOnOnUseProp(bool boolParam)
		{
			SetPropVisibility(boolParam);
		}

		public override bool IsInventoryOpen()
		{
			return _objectInHandsAnimator.IsInInventory;
		}

		public override void FastForwardCurrentState()
		{
			Class1258_0.FastForward();
		}

		public void method_1()
		{
			Class1258_0.HideWeaponComplete();
		}

		public void method_2()
		{
			Class1258_0.WeaponAppeared();
		}

		public void method_3()
		{
			Class1258_0.OnBackpackDrop();
		}

		public override void Spawn(float animationSpeed, Action callback)
		{
			firearmsAnimator_0.SetAnimationSpeed(animationSpeed);
			Action callback2 = delegate
			{
				_player.StartCoroutine(method_4(callback));
			};
			InitiateOperation<Class1263>().Start(callback2);
		}

		public override void ManualUpdate(float deltaTime)
		{
			base.ManualUpdate(deltaTime);
			firearmsAnimator_0?.SetAimAngle(_player.Pitch);
		}

		public override void Drop(float animationSpeed, Action callback, bool fastDrop, Item nextControllerItem = null)
		{
			if (base.Destroyed)
			{
				Class1258_0.HideWeapon(callback);
				return;
			}
			base.Destroyed = true;
			Action onHidden = delegate
			{
				callback();
			};
			Class1258_0.HideWeapon(onHidden);
		}

		public IEnumerator method_4(Action callback)
		{
			while (!Boolean_0)
			{
				yield return null;
			}
			callback();
		}

		public override Dictionary<Type, OperationFactoryDelegate> GetOperationFactoryDelegates()
		{
			return new Dictionary<Type, OperationFactoryDelegate>
			{
				{
					typeof(Class1263),
					() => new Class1263(this)
				},
				{
					typeof(Class1261),
					() => new Class1261(this)
				},
				{
					typeof(Class1262),
					() => new Class1262(this)
				},
				{
					typeof(Class1259),
					() => new Class1259(this)
				}
			};
		}

		[CompilerGenerated]
		public BaseAnimationOperationClass method_5()
		{
			return new Class1263(this);
		}

		[CompilerGenerated]
		public BaseAnimationOperationClass method_6()
		{
			return new Class1261(this);
		}

		[CompilerGenerated]
		public BaseAnimationOperationClass method_7()
		{
			return new Class1262(this);
		}

		[CompilerGenerated]
		public BaseAnimationOperationClass method_8()
		{
			return new Class1259(this);
		}
	}

	public class FirearmController : ItemHandsController, IFirearmHandsController, GInterface199, IHandsController, GInterface197, GInterface200
	{
		public class GClass2014(FirearmController controller) : GClass2013(controller)
		{
			[NonSerialized]
			public AmmoItemClass AmmoItemClass;

			[NonSerialized]
			public ItemAddress ItemAddress_0;

			[NonSerialized]
			public Callback Callback_0;

			[NonSerialized]
			public bool Bool_0;

			[NonSerialized]
			public bool Bool_1;

			[NonSerialized]
			public int Int_0 = -1;

			public virtual void Start(AmmoItemClass item, int camoraIndex, ItemAddress itemAddress, Callback callback)
			{
				AmmoItemClass = item;
				Callback_0 = callback;
				Int_0 = camoraIndex;
				ItemAddress_0 = itemAddress;
				Start();
				FirearmController_0.IsAiming = false;
				FirearmsAnimator_0.Discharge(discharge: true);
				FirearmsAnimator_0.SetFire(fire: false);
				Player_0.MovementContext.SetBlindFire(0);
				Player_0.BodyAnimatorCommon.SetFloat(PlayerAnimator.RELOAD_FLOAT_PARAM_HASH, 1f);
			}

			public override void Reset()
			{
				base.Reset();
				AmmoItemClass = null;
				Callback_0 = null;
				Bool_0 = false;
				Bool_1 = false;
				Int_0 = -1;
				ItemAddress_0 = null;
			}

			public override void FastForward()
			{
				if (State != EOperationState.Finished)
				{
					OnMagAppeared();
					OnMagPuttedToRig();
				}
			}

			public override void RemoveAmmoFromChamber()
			{
				FirearmController_0.underbarrelManagerClass.DestroyPatronInWeapon();
			}

			public override void OnMagAppeared()
			{
				if (!Bool_0)
				{
					Bool_0 = true;
					FirearmsAnimator_0.Discharge(discharge: false);
					FirearmsAnimator_0.SetShellsInWeapon(FirearmController_0.UnderbarrelWeapon.ShellsInLauncherCount);
					FirearmsAnimator_0.SetCanReload(canReload: false);
				}
			}

			public override void OnMagPuttedToRig()
			{
				if (!Bool_1)
				{
					Bool_1 = true;
					method_5();
				}
			}

			public void method_5()
			{
				FirearmsAnimator_0.SetAmmoInChamber(0f);
				State = EOperationState.Finished;
				FirearmController_0.InitiateOperation<GClass2040>().Start();
				Callback_0.Succeed();
				FirearmsAnimator_0.SetAmmoInChamber(FirearmController_0.UnderbarrelWeapon.ChamberAmmoCount);
				FirearmsAnimator_0.SetShellsInWeapon(FirearmController_0.UnderbarrelWeapon.ShellsInLauncherCount);
			}

			public override void SetInventoryOpened(bool opened)
			{
				FirearmController_0.InventoryOpened = opened;
				FirearmsAnimator_0.SetInventory(opened);
			}
		}

		public class Class1264 : GClass2013
		{
			[NonSerialized]
			public Item Item_0;

			[NonSerialized]
			public Slot Slot_0;

			[NonSerialized]
			public Callback Callback_0;

			[NonSerialized]
			public bool Bool_0;

			public Class1264(FirearmController controller)
				: base(controller)
			{
			}

			public void Start(Item item, Slot slot, Callback callback)
			{
				Item_0 = item;
				Slot_0 = slot;
				Callback_0 = callback;
				Start();
				FirearmsAnimator_0.SetupMod(modSet: true);
				FirearmsAnimator_0.SetFire(fire: false);
				Player_0.MovementContext.SetBlindFire(0);
				FirearmController_0.SetAim(value: false);
			}

			public override void Reset()
			{
				Item_0 = null;
				Slot_0 = null;
				Callback_0 = null;
				Bool_0 = false;
				base.Reset();
			}

			public override void FastForward()
			{
				if (State != EOperationState.Finished)
				{
					OnModChanged();
				}
			}

			public override void SetAiming(bool isAiming)
			{
				if (!isAiming)
				{
					FirearmController_0.IsAiming = false;
				}
			}

			public override void OnModChanged()
			{
				if (!Bool_0)
				{
					Bool_0 = true;
					FirearmsAnimator_0.SetupMod(modSet: false);
					GameObject gameObject = Singleton<PoolManagerClass>.Instance.CreateItem(Item_0, isAnimated: true);
					WeaponManagerClass.SetupMod(Slot_0, gameObject);
					FirearmsAnimator_0.Fold(Weapon_0.Folded);
					State = EOperationState.Finished;
					Callback_0.Succeed();
					Player_0.BodyAnimatorCommon.SetFloat(PlayerAnimator.WEAPON_SIZE_MODIFIER_PARAM_HASH, Weapon_0.CalculateCellSize().X);
					Player_0.UpdateFirstPersonGrip(GripPose.EGripType.Common, FirearmController_0.HandsHierarchy);
					FirearmController_0.SetupModAudioController(gameObject.transform, FirearmController_0);
					if (Item_0 is Mod { HasLightComponent: not false })
					{
						Player_0.SendWeaponLightPacket();
					}
					FirearmController_0.WeaponModified();
					method_5(gameObject);
					FirearmController_0.InitiateOperation<GClass2037>().Start();
				}
			}

			public void method_5(GameObject createdItem)
			{
				if (Item_0 is LauncherItemClass underbarrelWeapon)
				{
					FirearmController_0.method_7(underbarrelWeapon, createdItem);
				}
			}

			public override void SetInventoryOpened(bool opened)
			{
				FirearmController_0.InventoryOpened = opened;
				FirearmsAnimator_0.SetInventory(opened);
			}

			public override bool CanChangeLightState(FirearmLightStateStruct[] lightsStates)
			{
				return false;
			}
		}

		public class GClass2029 : GClass2028
		{
			[NonSerialized]
			public const float Float_0 = 0f;

			[NonSerialized]
			public const float Float_1 = 0.25f;

			[NonSerialized]
			public const float Float_2 = 0.75f;

			[NonSerialized]
			public const float Float_3 = 0.75f;

			[NonSerialized]
			public const float Float_4 = 0.99f;

			[NonSerialized]
			public const int Int_0 = 3;

			[NonSerialized]
			public int Int_1;

			[NonSerialized]
			public float Float_5;

			[NonSerialized]
			public float Float_6;

			[NonSerialized]
			public float Float_7;

			[NonSerialized]
			public SingleShotData SingleShotData_0;

			[NonSerialized]
			public bool Bool_1;

			[NonSerialized]
			public int Int_2;

			[NonSerialized]
			public float Float_8;

			public float ShotsTime => Float_5;

			public GClass2029(FirearmController controller)
				: base(controller)
			{
				Float_5 = 60f / (float)controller.Item.FireRate;
				Int_2 = Weapon_0.GetItemComponent<FireModeComponent>()?.BurstShotsCount ?? 3;
			}

			public new virtual void Start()
			{
				base.Start();
				Float_7 = 0.0001f;
				Int_1 = 0;
				Float_6 = 0f;
				Bool_1 = false;
				FirearmController_0.autoFireOn = true;
				FirearmController_0.bool_3 = false;
				FirearmsAnimator_0.SetFire(FirearmController_0.IsTriggerPressed);
				FirearmsAnimator_0.Animator.Play(FirearmsAnimator_0.FullFireStateName, 1, 0.2f);
				InternalOnFireEvent();
			}

			public override void Reset()
			{
				base.Reset();
				Float_8 = -1f;
			}

			public override void SetTriggerPressed(bool pressed)
			{
				FirearmController_0.IsTriggerPressed = pressed;
			}

			public override void OnFireEvent()
			{
				Bool_1 = true;
			}

			public override void Update(float deltaTime)
			{
				if (!Bool_1 && !Player_0.IsAI)
				{
					return;
				}
				try
				{
					base.Update(deltaTime);
					float num = Float_7 % Float_5 / Float_5;
					float num2 = num + deltaTime / Float_5;
					if (num2 <= 1f)
					{
						method_10(num, num2);
					}
					else
					{
						method_10(num, 1f);
						float num3 = num2 - 1f;
						while (num3 > 0f)
						{
							if (num3 <= 1f)
							{
								method_10(0f, num3);
								num3 -= 1f;
								continue;
							}
							double num4 = Math.Floor(num3);
							for (int i = 0; (double)i < num4; i++)
							{
								method_10(0f, 1f);
								num3 -= 1f;
							}
						}
					}
					Weapon item = FirearmController_0.Item;
					float overheatFirerateMult = item.MalfState.OverheatFirerateMult;
					Float_7 += deltaTime;
					if (overheatFirerateMult > 0f)
					{
						if (Mathf.Abs(overheatFirerateMult - Float_6) > Mathf.Epsilon)
						{
							Float_7 = Float_5 * (num2 % 1f);
							Float_5 = 60f / ((float)item.FireRate * item.MalfState.OverheatFirerateMult);
						}
						Float_6 = overheatFirerateMult;
					}
				}
				catch (Exception exception)
				{
					UnityEngine.Debug.LogException(exception);
					InternalOnFireEndEvent();
				}
			}

			public void method_10(float normalizedStartFrameTime, float normalizedEndFrameTime)
			{
				if (State != EOperationState.Executing)
				{
					return;
				}
				if (normalizedStartFrameTime <= 0f && 0f < normalizedEndFrameTime)
				{
					InternalOnFireEvent();
				}
				if (State == EOperationState.Executing)
				{
					if (normalizedStartFrameTime <= 0.25f && 0.25f < normalizedEndFrameTime)
					{
						InternalOnShellEjectEvent();
					}
					if (normalizedStartFrameTime <= 0.75f && 0.75f < normalizedEndFrameTime && Weapon_0.IsBoltCatch && Weapon_0.ChamberAmmoCount == 0 && Weapon_0.GetCurrentMagazine() != null && Weapon_0.GetCurrentMagazineCount() == 0 && !Weapon_0.ManualBoltCatch)
					{
						FirearmsAnimator_0.SetBoltCatch(active: true);
					}
					if (normalizedStartFrameTime <= 0.75f && 0.75f < normalizedEndFrameTime)
					{
						InternalRemoveAmmoFromChamber();
						InternalOnAddAmmoInChamber();
					}
					if (normalizedStartFrameTime <= 0.99f && 0.99f < normalizedEndFrameTime)
					{
						InternalOnFireEndEvent();
					}
					Float_8 = normalizedEndFrameTime;
				}
			}

			public virtual void InternalOnFireEvent()
			{
				SingleShotData_0 = method_5(out var malfState, out var malfSource);
				ShowIncompatibleNotification();
				Weapon_0.MalfState.State = malfState;
				if (malfState != Weapon.EMalfunctionState.None)
				{
					Weapon_0.MalfState.LastMalfunctionTime = GClass1891.PastTime;
					if (Player_0.Skills.TroubleFixingDurElite.Value)
					{
						Weapon_0.MalfState.AddMalfReduceChance(Player_0.ProfileId, malfSource);
					}
					FirearmsAnimator_0.MisfireSlideUnknown(val: false);
					if (Weapon_0.MalfState.State == Weapon.EMalfunctionState.Misfire)
					{
						Player_0.InventoryController.ExamineMalfunction(Weapon_0, clearRest: true);
					}
				}
				if (malfState == Weapon.EMalfunctionState.Misfire)
				{
					FirearmController_0.bool_3 = true;
					method_13();
				}
				else
				{
					if (malfState == Weapon.EMalfunctionState.None)
					{
						FirearmController_0.bool_3 = false;
						FirearmsAnimator_0.SetFire(FirearmController_0.IsTriggerPressed);
					}
					else
					{
						FirearmController_0.bool_3 = true;
					}
					FireModeComponent fireMode = FirearmController_0.Item.FireMode;
					FirearmController_0.IsBirstOf2Start = fireMode.FireMode == Weapon.EFireMode.burst && Int_1 == 0 && fireMode.BurstShotsCount == 2;
					MakeShot(SingleShotData_0.AmmoToFire);
					Int_1++;
					FirearmController_0.IsBirstOf2Start = false;
					if (Weapon_0.HasChambers)
					{
						if (Weapon_0.MalfState.State == Weapon.EMalfunctionState.Feed)
						{
							FirearmController_0.weaponManagerClass.SetRoundIntoWeapon(SingleShotData_0.FedAmmo);
							FirearmController_0.weaponManagerClass.MoveAmmoFromChamberToShellPort(ammoIsUsed: false);
						}
						else
						{
							FirearmController_0.weaponManagerClass.MoveAmmoFromChamberToShellPort(SingleShotData_0.AmmoToFire.IsUsed);
						}
					}
					if (Weapon_0.MalfState.State == Weapon.EMalfunctionState.Jam || Weapon_0.MalfState.State == Weapon.EMalfunctionState.SoftSlide || Weapon_0.MalfState.State == Weapon.EMalfunctionState.HardSlide || Weapon_0.MalfState.State == Weapon.EMalfunctionState.Feed)
					{
						Player_0.InventoryController.ExamineMalfunction(Weapon_0, clearRest: true);
						method_13();
					}
					if (FirearmController_0.method_65())
					{
						Player_0.InventoryController.ProcessFastWeaponSwitchAvailability();
					}
				}
				FirearmController_0._player.MouseLook();
			}

			public void InternalOnShellEjectEvent()
			{
				if (!Weapon_0.HasChambers && SingleShotData_0.AmmoToFire != null)
				{
					FirearmController_0.weaponManagerClass.SetRoundIntoWeapon(SingleShotData_0.AmmoToFire);
					FirearmController_0.weaponManagerClass.MoveAmmoFromChamberToShellPort(SingleShotData_0.AmmoToFire.IsUsed);
				}
				method_8();
			}

			public override void OnShellEjectEvent()
			{
			}

			public override void OnAddAmmoInChamber()
			{
			}

			public override void RemoveAmmoFromChamber()
			{
			}

			public override void OnOnOffBoltCatchEvent(bool isCaught)
			{
			}

			public void InternalRemoveAmmoFromChamber()
			{
				if (Weapon_0.HasChambers)
				{
					FirearmsAnimator_0.SetAmmoInChamber(0f);
				}
				if (SingleShotData_0.AmmoWillBeLoadedToChamber == null || Weapon_0.MalfState.State != Weapon.EMalfunctionState.None)
				{
					FirearmController_0.bool_3 = true;
				}
			}

			public void InternalOnAddAmmoInChamber()
			{
				if (Weapon_0.HasChambers)
				{
					FirearmsAnimator_0.SetAmmoInChamber(SingleShotData_0.AmmoCountInChamberAfterShot);
				}
				FirearmsAnimator_0.SetAmmoOnMag(SingleShotData_0.AmmoCountInMagAfterShot);
				if (Weapon_0.HasChambers && SingleShotData_0.AmmoWillBeLoadedToChamber != null)
				{
					FirearmController_0.weaponManagerClass.SetRoundIntoWeapon(SingleShotData_0.AmmoWillBeLoadedToChamber);
				}
				FirearmsAnimator_0.SetFire(FirearmController_0.IsTriggerPressed);
			}

			public virtual void InternalOnFireEndEvent()
			{
				if (Weapon_0.HasChambers)
				{
					FirearmsAnimator_0.SetAmmoInChamber(SingleShotData_0.AmmoCountInChamberAfterShot);
				}
				FirearmsAnimator_0.SetAmmoOnMag(SingleShotData_0.AmmoCountInMagAfterShot);
				if (Weapon_0.SelectedFireMode == Weapon.EFireMode.burst)
				{
					if (Int_1 < Int_2 && SingleShotData_0.AmmoCountInChamberAfterShot > 0)
					{
						FirearmController_0.IsTriggerPressed = true;
						return;
					}
					FirearmController_0.IsTriggerPressed = false;
				}
				if (Action_0 != null)
				{
					FirearmController_0.IsTriggerPressed = false;
					FirearmsAnimator_0.SetFire(FirearmController_0.IsTriggerPressed);
					SetAiming(isAiming: false);
					method_12();
				}
				else if (FirearmController_0.IsTriggerPressed && !Bool_0)
				{
					if ((Weapon_0.HasChambers && SingleShotData_0.AmmoCountInChamberAfterShot == 0) || (!Weapon_0.HasChambers && SingleShotData_0.AmmoCountInMagAfterShot == 0))
					{
						if (Weapon_0.IsBoltCatch)
						{
							method_11();
							return;
						}
						method_11();
						FirearmController_0.SetTriggerPressed(pressed: true);
					}
				}
				else
				{
					method_11();
				}
			}

			public override void FastForward()
			{
				method_11();
			}

			public void method_11()
			{
				SetTriggerPressed(pressed: false);
				FirearmsAnimator_0.SetInventory(Bool_0);
				FirearmsAnimator_0.SetFire(FirearmController_0.IsTriggerPressed);
				State = EOperationState.Finished;
				FirearmsAnimator_0.Animator.Play(FirearmsAnimator_0.FullIdleStateName, 1, 0.1f);
				FirearmController_0.EmitEvents();
				State = EOperationState.Finished;
				FirearmController_0.autoFireOn = false;
				FirearmController_0.InitiateOperation<GClass2037>().Start();
			}

			public void method_12()
			{
				SetTriggerPressed(pressed: false);
				FirearmsAnimator_0.SetFire(FirearmController_0.IsTriggerPressed);
				FirearmsAnimator_0.Animator.Play(FirearmsAnimator_0.FullIdleStateName, 1, 0.1f);
				FirearmController_0.EmitEvents();
				SetAiming(isAiming: false);
				FirearmController_0.autoFireOn = false;
				State = EOperationState.Finished;
				Action_0();
			}

			public override void SetAiming(bool isAiming)
			{
				if (!Weapon_0.HasMagazineWithBelt())
				{
					FirearmController_0.IsAiming = isAiming;
				}
			}

			public void method_13()
			{
				SetTriggerPressed(pressed: false);
				FirearmsAnimator_0.SetFire(FirearmController_0.IsTriggerPressed);
				FirearmsAnimator_0.Malfunction((int)Weapon_0.MalfState.State);
				FirearmController_0.autoFireOn = false;
				State = EOperationState.Finished;
				switch (Weapon_0.MalfState.State)
				{
				case Weapon.EMalfunctionState.Misfire:
					FirearmsAnimator_0.Animator.Play("MISFIRE", 1, 0f);
					break;
				case Weapon.EMalfunctionState.Jam:
					FirearmsAnimator_0.Animator.Play("JAM", 1, 0f);
					break;
				case Weapon.EMalfunctionState.HardSlide:
					FirearmsAnimator_0.Animator.Play("HARD_SLIDE", 1, 0f);
					break;
				case Weapon.EMalfunctionState.SoftSlide:
					FirearmsAnimator_0.Animator.Play("SOFT_SLIDE", 1, 0f);
					break;
				case Weapon.EMalfunctionState.Feed:
					FirearmsAnimator_0.Animator.Play("FEED", 1, 0f);
					break;
				}
				FirearmController_0.EmitEvents();
				Weapon_0.MalfState.AmmoToFire = SingleShotData_0.AmmoToFire;
				Weapon_0.MalfState.AmmoWillBeLoadedToChamber = SingleShotData_0.AmmoWillBeLoadedToChamber;
				Weapon_0.MalfState.MalfunctionedAmmo = SingleShotData_0.FedAmmo ?? SingleShotData_0.AmmoToFire;
				FirearmController_0.InitiateOperation<GClass2049>().Start();
			}
		}

		public abstract class GClass2015 : GClass2013
		{
			[NonSerialized]
			public bool Bool_0;

			[NonSerialized]
			public Action Action_0;

			[NonSerialized]
			public Callback Callback_0;

			[NonSerialized]
			[CompilerGenerated]
			public Slot Slot_0_1;

			public Slot Slot_0
			{
				[CompilerGenerated]
				get
				{
					return Slot_0_1;
				}
				[CompilerGenerated]
				set
				{
					Slot_0_1 = value;
				}
			}

			public override EOperationState State
			{
				get
				{
					return base.State;
				}
				set
				{
					base.State = value;
					switch (value)
					{
					case EOperationState.Finished:
						Player_0.CurrentManagedState?.OnReload(enable: false);
						break;
					case EOperationState.Executing:
						Player_0.CurrentManagedState?.OnReload(enable: true);
						break;
					}
				}
			}

			public GClass2015(FirearmController controller)
				: base(controller)
			{
			}

			public void method_5(bool isAiming)
			{
				if (FirearmController_0.CurrentMasteringLevel < 2)
				{
					return;
				}
				Player_0.ProceduralWeaponAnimation.TacticalReload = isAiming;
				if (!isAiming || EFTHardSettings.Instance.CanAimInState(Player_0.CurrentState.Name))
				{
					if (FirearmController_0.float_2 > EFTHardSettings.Instance.STOP_AIMING_AT && isAiming)
					{
						FirearmController_0.AimingInterruptedByOverlap = false;
					}
					else
					{
						FirearmController_0.IsAiming = isAiming;
					}
				}
			}

			public void Start([CanBeNull] Callback callback)
			{
				Callback_0 = callback;
				Slot_0 = (Weapon_0.HasChambers ? Weapon_0.Chambers[0] : null);
				Player_0.ProceduralWeaponAnimation.TacticalReload = true;
				Player_0.ExecuteSkill((Action)delegate
				{
					Player_0.Skills.WeaponReloadAction.Complete(Weapon_0);
				});
				FirearmsAnimator_0.SetInventory(open: false);
				FirearmController_0.SetCompassState(active: false);
				Player_0.MovementContext.SetBlindFire(0);
				Player_0.BodyAnimatorCommon.SetFloat(PlayerAnimator.RELOAD_FLOAT_PARAM_HASH, 1f);
				Player_0.RemoveLeftHandItem(2.5f);
				Start();
			}

			public override void HideWeapon(Action onHidden, bool fastDrop, Item nextControllerItem = null)
			{
				Bool_0 = fastDrop;
				Action_0 = onHidden;
			}

			public override void Reset()
			{
				Action_0 = null;
				Bool_0 = false;
				Callback_0 = null;
				Slot_0 = null;
				base.Reset();
			}

			public void method_6()
			{
				Callback_0?.Succeed();
			}

			public override void SetInventoryOpened(bool opened)
			{
				FirearmController_0.InventoryOpened = opened;
			}

			public void method_7()
			{
				FirearmsAnimator_0.SetInventory(FirearmController_0.InventoryOpened);
			}

			[CompilerGenerated]
			public void method_8()
			{
				Player_0.Skills.WeaponReloadAction.Complete(Weapon_0);
			}
		}

		public class DefaultWeaponOperationClass : GenericFireOperationClass
		{
			public DefaultWeaponOperationClass(FirearmController controller)
				: base(controller)
			{
			}

			public override void Start()
			{
				base.Start();
				FirearmsAnimator_0.SetBoltActionReload(!FirearmController_0.IsTriggerPressed);
				Player_0.ProceduralWeaponAnimation.TacticalReload = Player_0.ProceduralWeaponAnimation.IsMountedState && !Player_0.ProceduralWeaponAnimation.IsVerticalMounting && Player_0.ProceduralWeaponAnimation.IsBipodUsed;
			}

			public override void SetTriggerPressed(bool pressed)
			{
				base.SetTriggerPressed(pressed);
				FirearmsAnimator_0.SetBoltActionReload(!FirearmController_0.IsTriggerPressed);
			}

			public override void SetInventoryOpened(bool opened)
			{
				base.SetInventoryOpened(opened);
				FirearmsAnimator_0.SetBoltActionReload(boltActionReload: true);
			}

			public override void ReloadMag(MagazineItemClass magazine, ItemAddress itemAddress, Callback finishCallback, Callback startCallback)
			{
				base.ReloadMag(magazine, itemAddress, finishCallback, startCallback);
				FirearmsAnimator_0.SetBoltActionReload(boltActionReload: true);
			}

			public override void QuickReloadMag(MagazineItemClass magazine, Callback finishCallback, Callback startCallback)
			{
				base.QuickReloadMag(magazine, finishCallback, startCallback);
				FirearmsAnimator_0.SetBoltActionReload(boltActionReload: true);
			}

			public override void ReloadWithAmmo(AmmoPackReloadingClass ammoPack, Callback finishCallback, Callback startCallback)
			{
				base.ReloadWithAmmo(ammoPack, finishCallback, startCallback);
				FirearmsAnimator_0.SetBoltActionReload(boltActionReload: true);
			}

			public override void ReloadCylinderMagazine(AmmoPackReloadingClass ammoPack, Callback finishCallback, Callback startCallback, bool quickReload = false)
			{
				base.ReloadCylinderMagazine(ammoPack, finishCallback, startCallback, quickReload);
				FirearmsAnimator_0.SetBoltActionReload(boltActionReload: true);
			}

			public override void HideWeapon(Action onHidden, bool fastDrop, Item nextControllerItem = null)
			{
				SetAiming(isAiming: false);
				SetTriggerPressed(pressed: false);
				base.HideWeapon(onHidden, fastDrop, (Item)null);
			}

			public override void OnAimingDisabled()
			{
				SetAiming(isAiming: false);
				SetTriggerPressed(pressed: false);
				base.OnAimingDisabled();
			}
		}

		public class GClass2023(FirearmController controller) : GClass2013(controller)
		{
			[NonSerialized]
			public AmmoItemClass AmmoItemClass;

			[NonSerialized]
			public CylinderMagazineItemClass CylinderMagazineItemClass;

			[NonSerialized]
			public ItemAddress ItemAddress_0;

			[NonSerialized]
			public Callback Callback_0;

			[NonSerialized]
			public bool Bool_0;

			[NonSerialized]
			public bool Bool_1;

			[NonSerialized]
			public int Int_0 = -1;

			public virtual void Start(AmmoItemClass item, int camoraIndex, ItemAddress itemAddress, Callback callback)
			{
				AmmoItemClass = item;
				Callback_0 = callback;
				Int_0 = camoraIndex;
				CylinderMagazineItemClass = Weapon_0.GetCurrentMagazine() as CylinderMagazineItemClass;
				ItemAddress_0 = itemAddress;
				Start();
				FirearmController_0.IsAiming = false;
				FirearmsAnimator_0.Discharge(discharge: true);
				FirearmsAnimator_0.SetFire(fire: false);
				Player_0.MovementContext.SetBlindFire(0);
				FirearmsAnimator_0.SetCamoraIndexForUnloadAmmo(camoraIndex);
				Player_0.BodyAnimatorCommon.SetFloat(PlayerAnimator.RELOAD_FLOAT_PARAM_HASH, 1f);
			}

			public override void Reset()
			{
				base.Reset();
				AmmoItemClass = null;
				Callback_0 = null;
				Bool_0 = false;
				Bool_1 = false;
				Int_0 = -1;
				CylinderMagazineItemClass = null;
				ItemAddress_0 = null;
			}

			public override void FastForward()
			{
				if (State != EOperationState.Finished)
				{
					OnMagAppeared();
					OnMagPuttedToRig();
				}
			}

			public override void OnMagAppeared()
			{
				if (!Bool_0)
				{
					Bool_0 = true;
					FirearmsAnimator_0.Discharge(discharge: false);
					FirearmsAnimator_0.SetShellsInWeapon(Weapon_0.ShellsInChamberCount);
					FirearmsAnimator_0.SetAmmoOnMag(CylinderMagazineItemClass.Count);
					WeaponManagerClass.DestroyPatronInWeapon(Int_0);
					FirearmsAnimator_0.SetCanReload(canReload: false);
					if (ItemAddress_0 != null)
					{
						method_5();
					}
				}
			}

			public override void OnMagPuttedToRig()
			{
				if (!Bool_1)
				{
					Bool_1 = true;
					if (ItemAddress_0 == null)
					{
						method_5();
					}
				}
			}

			public void method_5()
			{
				CylinderMagazineItemClass.ResetCamoraIndex();
				FirearmsAnimator_0.SetCamoraIndex(CylinderMagazineItemClass.CurrentCamoraIndex);
				State = EOperationState.Finished;
				method_6();
				FirearmController_0.InitiateOperation<GClass2037>().Start();
				Callback_0.Succeed();
			}

			public override void SetInventoryOpened(bool opened)
			{
				FirearmController_0.InventoryOpened = opened;
				FirearmsAnimator_0.SetInventory(opened);
			}

			public void method_6()
			{
				FirearmsAnimator_0.LoadOneTrigger(ItemAddress_0 != null);
			}
		}

		public class GClass2025 : GClass2024
		{
			public GClass2025(FirearmController controller)
				: base(controller)
			{
			}

			public override void RemoveAmmoFromChamber()
			{
				Callback_0.Succeed();
				FirearmsAnimator_0.SetAmmoInChamber(Weapon_0.ChamberAmmoCount);
				FirearmsAnimator_0.SetShellsInWeapon(Weapon_0.ShellsInWeaponCount);
				FirearmsAnimator_0.SetCanReload(canReload: false);
				if (Bool_0)
				{
					WeaponManagerClass.RemoveShellInWeapon();
				}
				else
				{
					WeaponManagerClass.DestroyPatronInWeapon(Int_0);
				}
			}

			public override void OnMagPuttedToRig()
			{
				if (!Bool_1)
				{
					Bool_1 = true;
					State = EOperationState.Finished;
					FirearmsAnimator_0.Discharge(discharge: false);
					FirearmController_0.InitiateOperation<GClass2037>().Start();
					if (base.Boolean_0)
					{
						RemoveAmmoFromChamber();
					}
				}
			}
		}

		public class GClass2024 : GClass2013
		{
			[NonSerialized]
			public Callback Callback_0;

			[NonSerialized]
			public bool Bool_0;

			[NonSerialized]
			public bool Bool_1;

			[NonSerialized]
			public int Int_0 = -1;

			[NonSerialized]
			public ItemAddress ItemAddress_0;

			public bool Boolean_0 => ItemAddress_0 != null;

			public GClass2024(FirearmController controller)
				: base(controller)
			{
			}

			public virtual void Start(AmmoItemClass ammo, int chamberIndex, ItemAddress destinationAddress, Callback callback)
			{
				if (Weapon_0.MalfState.State == Weapon.EMalfunctionState.Misfire)
				{
					method_2();
				}
				Callback_0 = callback;
				Bool_0 = ammo.IsUsed;
				Int_0 = chamberIndex;
				ItemAddress_0 = destinationAddress;
				Start();
				FirearmController_0.IsAiming = false;
				FirearmsAnimator_0.Discharge(discharge: true);
				FirearmsAnimator_0.SetFire(fire: false);
				Player_0.MovementContext.SetBlindFire(0);
				FirearmsAnimator_0.SetChamberIndexForLoadUnloadAmmo(Int_0);
				Player_0.BodyAnimatorCommon.SetFloat(PlayerAnimator.RELOAD_FLOAT_PARAM_HASH, 1f);
				method_5();
				method_3();
			}

			public override void Reset()
			{
				base.Reset();
				ItemAddress_0 = null;
				Callback_0 = null;
				Bool_0 = false;
				Bool_1 = false;
				Int_0 = -1;
			}

			public override void FastForward()
			{
				if (State != EOperationState.Finished)
				{
					OnMagPuttedToRig();
				}
			}

			public override void OnMagPuttedToRig()
			{
				if (!Bool_1)
				{
					Bool_1 = true;
					State = EOperationState.Finished;
					FirearmsAnimator_0.Discharge(discharge: false);
					FirearmController_0.InitiateOperation<GClass2037>().Start();
					Callback_0.Succeed();
					FirearmsAnimator_0.SetAmmoInChamber(Weapon_0.ChamberAmmoCount);
					FirearmsAnimator_0.SetShellsInWeapon(Weapon_0.ShellsInWeaponCount);
					FirearmsAnimator_0.SetCanReload(canReload: false);
					if (Bool_0)
					{
						WeaponManagerClass.RemoveShellInWeapon();
					}
					else
					{
						WeaponManagerClass.DestroyPatronInWeapon(Int_0);
					}
				}
			}

			public override void SetInventoryOpened(bool opened)
			{
				FirearmController_0.InventoryOpened = opened;
				FirearmsAnimator_0.SetInventory(opened);
			}

			public void method_5()
			{
				FirearmsAnimator_0.LoadOneTrigger(Boolean_0);
			}
		}

		public class GClass2026 : GClass2013
		{
			[NonSerialized]
			public Callback Callback_0;

			[NonSerialized]
			public bool Bool_0;

			public GClass2026(FirearmController controller)
				: base(controller)
			{
			}

			public virtual void Start(Item item, Callback callback)
			{
				Callback_0 = callback;
				Start();
				FirearmController_0.SetCompassState(active: false);
				FirearmsAnimator_0.SetFire(fire: false);
				Player_0.MovementContext.SetBlindFire(0);
				FirearmsAnimator_0.SetInventory(open: false);
				FirearmController_0.SetAim(value: false);
				FirearmController_0._player.SendHandsInteractionStateChanged(value: true, 300);
				Player_0.MovementContext.SetInteractInHands(EInteraction.DropBackpack);
			}

			public override void Reset()
			{
				Callback_0 = null;
				Bool_0 = false;
				base.Reset();
			}

			public override void SetAiming(bool isAiming)
			{
				if (!isAiming)
				{
					FirearmController_0.IsAiming = false;
				}
			}

			public override void FastForward()
			{
				if (State != EOperationState.Finished)
				{
					OnBackpackDropEvent();
				}
			}

			public override void OnBackpackDropEvent()
			{
				if (!Bool_0)
				{
					Bool_0 = true;
					State = EOperationState.Finished;
					FirearmController_0._player.SendHandsInteractionStateChanged(value: false, 300);
					WeaponAnimationSpeedControllerClass.ResetTriggerHandReady(FirearmController_0.firearmsAnimator_0.Animator);
					FirearmsAnimator_0.SetInventory(FirearmController_0.bool_2);
					FirearmController_0.InitiateOperation<GClass2037>().Start();
					Callback_0.Succeed();
				}
			}

			public override void SetInventoryOpened(bool opened)
			{
				FirearmController_0.bool_2 = opened;
			}

			public override bool CanChangeLightState(FirearmLightStateStruct[] lightsStates)
			{
				return false;
			}
		}

		public class GClass2047 : GClass2046
		{
			[NonSerialized]
			public bool Bool_0;

			public GClass2047(FirearmController controller)
				: base(controller)
			{
			}

			public new void Start()
			{
				FirearmController_0.IsAiming = false;
				FirearmsAnimator_0.SetFire(fire: false);
				Player_0.MovementContext.SetBlindFire(0);
				Player_0.BodyAnimatorCommon.SetFloat(PlayerAnimator.RELOAD_FLOAT_PARAM_HASH, 1f);
				base.Start();
			}

			public override void Reset()
			{
				Bool_0 = false;
				base.Reset();
			}

			public override void OnMalfunctionOffEvent()
			{
				if (!Bool_0)
				{
					Bool_0 = true;
					Player_0.InventoryController.ExamineMalfunctionType(Weapon_0);
					method_5();
				}
			}

			public override void FastForward()
			{
				OnMalfunctionOffEvent();
			}
		}

		public abstract class GClass2013 : BaseAnimationOperationClass
		{
			[NonSerialized]
			public FirearmController FirearmController_0;

			[NonSerialized]
			public Player Player_0;

			[NonSerialized]
			public FirearmsAnimator FirearmsAnimator_0;

			[NonSerialized]
			public WeaponManagerClass WeaponManagerClass;

			[NonSerialized]
			public Weapon Weapon_0;

			public GClass2013(FirearmController controller)
				: base(controller)
			{
				FirearmController_0 = controller;
				Player_0 = FirearmController_0._player;
				FirearmsAnimator_0 = FirearmController_0.firearmsAnimator_0;
				WeaponManagerClass = FirearmController_0.weaponManagerClass;
				Weapon_0 = FirearmController_0.Item;
			}

			public virtual bool CanChangeLightState(FirearmLightStateStruct[] lightsStates)
			{
				if (lightsStates != null)
				{
					return lightsStates.Length != 0;
				}
				return false;
			}

			public void SetLightsState(FirearmLightStateStruct[] lightsStates, bool force = false, bool animated = true)
			{
				if (force || CanChangeLightState(lightsStates))
				{
					if (animated)
					{
						FirearmsAnimator_0.ModToggleTrigger();
					}
					WeaponManagerClass.UpdateBeams();
				}
			}

			public override void SetLeftStanceAnimOnStartOperation()
			{
				Player_0.MovementContext.LeftStanceController.DisableLeftStanceAnimFromHandsAction();
			}

			public void method_2()
			{
				Player_0.ExecuteSkill((Action)delegate
				{
					Player_0.Skills.WeaponFixAction.Complete();
				});
				Player_0.InventoryController.CallUnknownMalfunctionStartRepair(Weapon_0);
				Player_0.InventoryController.CallMalfunctionRepaired(Weapon_0);
				Weapon_0.MalfState.Repair();
				FirearmsAnimator_0.MalfunctionRepair(val: false);
				FirearmsAnimator_0.Malfunction((int)Weapon_0.MalfState.State);
				FirearmsAnimator_0.MisfireSlideUnknown(val: false);
				FirearmsAnimator_0.SetLayerWeight(FirearmsAnimator_0.MALFUNCTION_LAYER_INDEX, 0);
				Weapon_0.MalfState.AmmoToFire = null;
				Weapon_0.MalfState.AmmoWillBeLoadedToChamber = null;
				Weapon_0.MalfState.MalfunctionedAmmo = null;
			}

			public void method_3()
			{
				int num = 0;
				for (int i = 0; i < Weapon_0.ShellsInChambers.Length; i++)
				{
					if (Weapon_0.ShellsInChambers[i] != null)
					{
						num++;
						FirearmsAnimator_0.SetChamberIndexWithShell(i);
					}
				}
				if (num == Weapon_0.ShellsInChambers.Length)
				{
					FirearmsAnimator_0.SetChamberIndexWithShell(num);
				}
				if (num == 0)
				{
					FirearmsAnimator_0.SetChamberIndexWithShell(-1f);
				}
			}

			public void SetScopeMode(FirearmScopeStateStruct[] scopeStates)
			{
				if (CanChangeScopeStates(scopeStates))
				{
					FirearmsAnimator_0.ModToggleTrigger();
					WeaponManagerClass.UpdateScopesMode();
				}
			}

			public virtual bool CanRemove()
			{
				return false;
			}

			public virtual void ShowGesture(EInteraction gesture)
			{
				method_0();
			}

			public virtual void BlindFire(int b)
			{
				BlindFire_Internal(0);
			}

			public virtual void OnFold(bool b)
			{
				method_0();
			}

			public void BlindFire_Internal(int b)
			{
				FirearmController_0.Blindfire = b != 0;
				if (b != 0 && FirearmController_0.IsAiming)
				{
					FirearmController_0.IsAiming = false;
				}
				Player_0.ProceduralWeaponAnimation.StartBlindFire(b);
			}

			public virtual void FastForward()
			{
			}

			public virtual bool CanChangeScopeStates(FirearmScopeStateStruct[] scopeStates)
			{
				if (scopeStates != null)
				{
					return scopeStates.Length != 0;
				}
				return false;
			}

			public virtual void SetFirearmCompassState(bool active)
			{
				method_0();
			}

			public virtual void OnMagPulledOutFromWeapon()
			{
				method_0();
			}

			public virtual void OnMagPuttedToRig()
			{
				method_0();
			}

			public virtual void OnMagAppeared()
			{
				method_0();
			}

			public virtual void OnMagInsertedToWeapon()
			{
				method_0();
			}

			public virtual void OnModChanged()
			{
				method_0();
			}

			public virtual void OnAddAmmoInChamber()
			{
				method_0();
			}

			public virtual void RemoveAmmoFromChamber()
			{
				method_0();
			}

			public virtual void OnOnOffBoltCatchEvent(bool isCaught)
			{
				method_0();
			}

			public virtual void OnBackpackDropEvent()
			{
				method_0();
			}

			public virtual void OnFireEvent()
			{
				method_0();
			}

			public virtual void OnFireEndEvent()
			{
				method_0();
			}

			public virtual void OnIdleStartEvent()
			{
				method_0();
			}

			public virtual void OnUtilityOperationStartEvent()
			{
				method_0();
			}

			public virtual void SetTriggerPressed(bool pressed)
			{
				method_0();
			}

			public virtual void ShowIncompatibleNotification()
			{
			}

			public virtual void SetInventoryOpened(bool opened)
			{
				method_0();
			}

			public virtual void SetAiming(bool isAiming)
			{
				method_0();
			}

			public virtual bool ChangeFireMode(Weapon.EFireMode fireMode)
			{
				method_0();
				return true;
			}

			public virtual bool CheckFireMode()
			{
				method_0();
				return false;
			}

			public virtual void OnSprintFinished()
			{
				method_0();
			}

			public virtual void OnSprintStart()
			{
				method_0();
			}

			public virtual void OnDropWeapon()
			{
				method_0();
			}

			public virtual void OnJumpOrFall()
			{
				method_0();
			}

			public virtual void OnAimingDisabled()
			{
				method_0();
			}

			public virtual bool ExamineWeapon()
			{
				method_0();
				return true;
			}

			public virtual void RollCylinder(Callback callback, bool rollToZeroCamora)
			{
				method_0();
			}

			public virtual void Execute(GInterface438 operation, Callback callback)
			{
				method_0();
				if (FirearmController_0.method_20(operation))
				{
					callback?.Succeed();
				}
				else
				{
					callback?.Fail($"Attach is not supported in current operation: {GetType()}");
				}
			}

			public virtual void ReloadMag(MagazineItemClass magazine, [CanBeNull] ItemAddress itemAddress, [CanBeNull] Callback finishCallback, [CanBeNull] Callback startCallback)
			{
				method_0();
				finishCallback?.Fail($"Reload is not supported in current operation: {GetType()}");
			}

			public virtual void QuickReloadMag(MagazineItemClass magazine, [CanBeNull] Callback finishCallback, [CanBeNull] Callback startCallback)
			{
				method_0();
				finishCallback?.Fail($"Quick reload is not supported in current operation: {GetType()}");
			}

			public virtual void ReloadGrenadeLauncher(AmmoPackReloadingClass ammoPack, [CanBeNull] Callback callback)
			{
				method_0();
				callback?.Fail($"Reload with ammo is not supported in current operation: {GetType()}");
			}

			public virtual void ReloadWithAmmo(AmmoPackReloadingClass ammoPack, [CanBeNull] Callback finishCallback, [CanBeNull] Callback startCallback)
			{
				method_0();
				finishCallback?.Fail($"Reload with ammo is not supported in current operation: {GetType()}");
			}

			public virtual void ReloadCylinderMagazine(AmmoPackReloadingClass ammoPack, [CanBeNull] Callback finishCallback, [CanBeNull] Callback startCallback, bool quickReload = false)
			{
				method_0();
				finishCallback?.Fail($"Reload revolver drum is not supported in current operation: {GetType()}");
			}

			public virtual void ReloadBarrels(AmmoPackReloadingClass ammoPack, ItemAddress placeToPutContainedAmmoMagazine, [CanBeNull] Callback finishCallback, [CanBeNull] Callback startCallback)
			{
				method_0();
				finishCallback?.Fail($"Reload with ammo is not supported in current operation: {GetType()}");
			}

			public virtual bool CanStartReload()
			{
				return false;
			}

			public virtual void OnRemoveShellEvent()
			{
				method_0();
			}

			public virtual void OnShellEjectEvent()
			{
				method_0();
			}

			public override void Update(float deltaTime)
			{
			}

			public virtual void AddAmmoToMag()
			{
				method_0();
			}

			public virtual void OnShowAmmo(bool value)
			{
				method_0();
			}

			public virtual void WeaponAppeared()
			{
				method_0();
			}

			public virtual void HideWeapon(Action onHidden, bool fastDrop, Item nextControllerItem = null)
			{
				method_0();
			}

			public virtual void HideWeaponComplete()
			{
				method_0();
			}

			public virtual bool CheckAmmo()
			{
				method_0();
				return false;
			}

			public virtual bool CheckChamber()
			{
				method_0();
				return false;
			}

			public virtual void OnMalfunctionOffEvent()
			{
				method_0();
			}

			public virtual void Pickup(bool p)
			{
				if (FirearmsAnimator_0.IsIdling())
				{
					Player_0.MovementContext.LeftStanceController.DisableLeftStanceAnimFromHandsAction();
					FirearmsAnimator_0.SetPickup(p);
				}
			}

			public virtual void Interact(bool isInteracting, int actionIndex)
			{
				if (FirearmsAnimator_0.IsIdling())
				{
					Player_0.MovementContext.LeftStanceController.DisableLeftStanceAnimFromHandsAction();
					Player_0.SendHandsInteractionStateChanged(isInteracting, actionIndex);
					FirearmsAnimator_0.SetInteract(isInteracting, actionIndex);
				}
			}

			public virtual void Loot(bool p)
			{
				if (FirearmsAnimator_0.IsIdling())
				{
					FirearmsAnimator_0.SetLooting(p);
				}
			}

			public virtual void UnderbarrelSightingRangeUp()
			{
				method_0();
			}

			public virtual void ForceSetUnderbarrelRangeIndex(int rangeIndex)
			{
				method_0();
			}

			public virtual void UnderbarrelSightingRangeDown()
			{
				method_0();
			}

			public virtual void UseSecondMagForReload()
			{
				method_0();
			}

			public virtual void ReplaceSecondMag()
			{
				method_0();
			}

			public virtual void PutMagToRig()
			{
				method_0();
			}

			public virtual bool ToggleLauncher(Action callback = null)
			{
				method_0();
				return false;
			}

			public virtual void ToggleLeftStance()
			{
				method_0();
			}

			public virtual bool CanNotBeInterrupted()
			{
				return false;
			}

			public virtual void LauncherAppeared()
			{
				method_0();
			}

			public virtual void LauncherDisappeared()
			{
				method_0();
			}

			public virtual void LauncherInventoryUnchamberFromMainWeapon(AmmoItemClass ammo, int camoraIndex, ItemAddress itemAddress, Callback callback)
			{
				method_0();
			}

			public virtual void LoadLauncherFromMainWeapon(AmmoItemClass ammo, int camoraIndex, ItemAddress itemAddress, Callback callback)
			{
				method_0();
			}

			public virtual void DropBackpackOperationInvoke(Item item, Callback callback)
			{
				method_0();
			}

			public virtual void SprintStateChanged(bool value)
			{
				method_0();
			}

			public virtual void OnBipodToggleEvent()
			{
				method_0();
			}

			public virtual bool ToggleBipod()
			{
				method_0();
				return false;
			}

			[CompilerGenerated]
			public void method_4()
			{
				Player_0.Skills.WeaponFixAction.Complete();
			}
		}

		public class RevolverFireOperationClass(FirearmController controller) : GenericFireOperationClass(controller)
		{
			[NonSerialized]
			public CylinderMagazineItemClass CylinderMagazineItemClass;

			[NonSerialized]
			public int Int_0 = -1;

			[NonSerialized]
			public bool Bool_5;

			public override void Start()
			{
				CylinderMagazineItemClass = Weapon_0.GetCurrentMagazine() as CylinderMagazineItemClass;
				Int_0 = CylinderMagazineItemClass.GetCamoraFireOrLoadStartIndex(!Weapon_0.CylinderHammerClosed);
				FirearmsAnimator_0.SetCamoraFireIndex(CylinderMagazineItemClass.CurrentCamoraIndex);
				if (CylinderMagazineItemClass.GetFirstAmmo(!Weapon_0.CylinderHammerClosed) == null)
				{
					State = EOperationState.Executing;
					method_14();
				}
				else
				{
					base.Start();
				}
			}

			public void method_14()
			{
				Bool_5 = true;
				FirearmsAnimator_0.SetCamoraFireIndex(CylinderMagazineItemClass.CurrentCamoraIndex);
				if ((Weapon_0.CylinderHammerClosed && Weapon_0.FireMode.FireMode == Weapon.EFireMode.doubleaction) || (!Weapon_0.CylinderHammerClosed && Weapon_0.FireMode.FireMode == Weapon.EFireMode.single))
				{
					CylinderMagazineItemClass.DryFireIncrementCamoraIndex();
				}
				FirearmsAnimator_0.SetDoubleAction(Convert.ToSingle(Weapon_0.CylinderHammerClosed));
				FirearmsAnimator_0.SetCamoraIndex(CylinderMagazineItemClass.CurrentCamoraIndex);
				Weapon_0.CylinderHammerClosed = Weapon_0.FireMode.FireMode == Weapon.EFireMode.doubleaction;
				FirearmsAnimator_0.SetFire(fire: true);
				Bool_1 = true;
				FirearmController_0.DryShot(Int_0);
			}

			public override void Reset()
			{
				base.Reset();
				Bool_5 = false;
				Int_0 = -1;
				CylinderMagazineItemClass = null;
				SingleShotData_0 = default(SingleShotData);
				Bool_1 = false;
			}

			public override void OnFireEvent()
			{
				if (!Bool_1)
				{
					Bool_1 = true;
					MakeShot(SingleShotData_0.AmmoToFire, Int_0);
					FirearmController_0.weaponManagerClass.MoveAmmoFromChamberToShellPort(SingleShotData_0.AmmoToFire.IsUsed, Int_0);
				}
			}

			public override void OnAddAmmoInChamber()
			{
				FirearmsAnimator_0.SetAmmoOnMag(CylinderMagazineItemClass.Count);
				Weapon_0.CylinderHammerClosed = Weapon_0.FireMode.FireMode == Weapon.EFireMode.doubleaction;
				FirearmsAnimator_0.SetDoubleAction(Convert.ToSingle(Weapon_0.CylinderHammerClosed));
			}

			public override void SetTriggerPressed(bool pressed)
			{
				FirearmController_0.IsTriggerPressed &= pressed;
			}

			public override void SetInventoryOpened(bool opened)
			{
				base.SetInventoryOpened(opened);
				FirearmController_0.InventoryOpened = opened;
			}

			public override void OnFireEndEvent()
			{
				if (Bool_5)
				{
					FirearmsAnimator_0.SetDoubleAction(Convert.ToSingle(Weapon_0.CylinderHammerClosed));
				}
			}

			public override void OnIdleStartEvent()
			{
				if (Bool_1)
				{
					FirearmController_0.IsTriggerPressed = false;
					FirearmsAnimator_0.SetFire(FirearmController_0.IsTriggerPressed);
					if (Action_0 != null)
					{
						FirearmsAnimator_0.SetFire(FirearmController_0.IsTriggerPressed);
						SetAiming(isAiming: false);
						method_17();
					}
					else
					{
						FirearmsAnimator_0.SetFire(FirearmController_0.IsTriggerPressed);
						method_15();
					}
				}
			}

			public override void OnShellEjectEvent()
			{
			}

			public override void FastForward()
			{
				OnFireEvent();
				method_15();
				FirearmsAnimator_0.Animator.Play(FirearmsAnimator_0.FullIdleStateName, 1, 0.2f);
			}

			public override void PrepareShot()
			{
				AmmoItemClass firstAmmo = CylinderMagazineItemClass.GetFirstAmmo(!Weapon_0.CylinderHammerClosed);
				SingleShotData_0 = default(SingleShotData);
				GStruct154<GInterface424> gStruct = CylinderMagazineItemClass.RemoveAmmoInCamora(firstAmmo, FirearmController_0._player.InventoryController);
				if (gStruct.Failed)
				{
					UnityEngine.Debug.LogError(gStruct.Error);
					return;
				}
				Player_0.InventoryController.CheckChamber(Weapon_0, status: false);
				if (firstAmmo == null)
				{
					UnityEngine.Debug.LogError("Fire operation can't start in case of no ammo");
					return;
				}
				SingleShotData_0.AmmoToFire = firstAmmo;
				SingleShotData_0.AmmoToFire.IsUsed = true;
				Weapon_0.ShellsInChambers[Int_0] = (AmmoTemplate)SingleShotData_0.AmmoToFire.Template;
				if (Weapon_0.CylinderHammerClosed || Weapon_0.FireMode.FireMode != Weapon.EFireMode.doubleaction)
				{
					CylinderMagazineItemClass.IncrementCamoraIndex();
				}
				FirearmsAnimator_0.SetCamoraIndex(CylinderMagazineItemClass.CurrentCamoraIndex);
				FirearmsAnimator_0.SetFire(FirearmController_0.IsTriggerPressed);
				ShowIncompatibleNotification();
				FirearmController_0._player.MouseLook();
			}

			public void method_15()
			{
				FirearmsAnimator_0.SetAmmoOnMag(CylinderMagazineItemClass.Count);
				FirearmsAnimator_0.SetInventory(Bool_0);
				FirearmsAnimator_0.SetFire(FirearmController_0.IsTriggerPressed);
				State = EOperationState.Finished;
				FirearmController_0.InitiateOperation<GClass2037>().Start();
			}

			public void method_16()
			{
				FirearmsAnimator_0.SetFire(FirearmController_0.IsTriggerPressed);
				FirearmsAnimator_0.Malfunction((int)Weapon_0.MalfState.State);
				State = EOperationState.Finished;
				Weapon_0.MalfState.AmmoToFire = SingleShotData_0.AmmoToFire;
				Weapon_0.MalfState.AmmoWillBeLoadedToChamber = SingleShotData_0.AmmoWillBeLoadedToChamber;
				Weapon_0.MalfState.MalfunctionedAmmo = SingleShotData_0.FedAmmo ?? SingleShotData_0.AmmoToFire;
				FirearmController_0.InitiateOperation<GClass2049>().Start();
			}

			public void method_17()
			{
				FirearmsAnimator_0.SetAmmoOnMag(CylinderMagazineItemClass.Count);
				FirearmsAnimator_0.SetFire(FirearmController_0.IsTriggerPressed);
				SetAiming(isAiming: false);
				State = EOperationState.Finished;
				Action_0();
			}
		}

		public class FireOnlyBarrelFireOperation(FirearmController controller) : GenericFireOperationClass(controller)
		{
			[NonSerialized]
			public List<int> List_0 = new List<int>();

			[NonSerialized]
			public List<SingleShotData> List_1 = new List<SingleShotData>();

			public override void PrepareShot()
			{
				if (Weapon_0.FireMode.FireMode == Weapon.EFireMode.single)
				{
					Slot firstLoadedChamberSlot = Weapon_0.FirstLoadedChamberSlot;
					int chamberIndex = Array.IndexOf(Weapon_0.Chambers, firstLoadedChamberSlot);
					method_15(chamberIndex);
				}
				else
				{
					method_14();
				}
				FirearmsAnimator_0.SetFire(FirearmController_0.IsTriggerPressed);
			}

			public void method_14()
			{
				for (int i = 0; i < Weapon_0.Chambers.Length; i++)
				{
					method_15(i);
				}
			}

			public void method_15(int chamberIndex)
			{
				SingleShotData item = new SingleShotData
				{
					AmmoCountInChamberBeforeShot = Weapon_0.ChamberAmmoCount
				};
				Slot slot = Weapon_0.Chambers[chamberIndex];
				if (slot.ContainedItem is AmmoItemClass { IsUsed: false } ammoItemClass)
				{
					ammoItemClass.IsUsed = true;
					_ = slot.RemoveItem().Succeeded;
					List_0.Add(chamberIndex);
					item.AmmoToFire = ammoItemClass;
					List_1.Add(item);
				}
			}

			public override void Reset()
			{
				base.Reset();
				List_0.Clear();
				List_1.Clear();
			}

			public override void RemoveAmmoFromChamber()
			{
				method_0();
			}

			public override void OnAddAmmoInChamber()
			{
				method_0();
			}

			public override void OnFireEvent()
			{
				Bool_1 = true;
				MakeMultiBarrelShot(List_1, List_0);
				for (int i = 0; i < List_1.Count; i++)
				{
					FirearmController_0.weaponManagerClass.MoveAmmoFromChamberToShellPort(List_1[i].AmmoToFire.IsUsed, List_0[i]);
					FirearmsAnimator_0.SetFire(FirearmController_0.IsTriggerPressed);
					SetTriggerPressed(pressed: false);
					Weapon_0.ShellsInChambers[List_0[i]] = (AmmoTemplate)List_1[i].AmmoToFire.Template;
				}
				FirearmsAnimator_0.SetAmmoInChamber(Weapon_0.ChamberAmmoCount);
				FirearmsAnimator_0.SetShellsInWeapon(Weapon_0.ShellsInWeaponCount);
			}
		}

		public class GenericFireOperationClass(FirearmController controller) : GClass2028(controller)
		{
			[NonSerialized]
			public SingleShotData SingleShotData_0;

			[NonSerialized]
			public bool Bool_1;

			[NonSerialized]
			public float Float_0;

			[NonSerialized]
			public bool Bool_2;

			[NonSerialized]
			public bool Bool_3;

			[NonSerialized]
			public bool Bool_4 = true;

			public new virtual void Start()
			{
				FirearmsAnimator_0.SetBoltActionReload(boltActionReload: true);
				base.Start();
				PrepareShot();
				Float_0 = 60f / (float)Weapon_0.SingleFireRate;
				Bool_4 = Weapon_0.CanQueueSecondShot;
				StartFireAnimation();
			}

			public virtual void StartFireAnimation()
			{
				if (Weapon_0.MalfState.State == Weapon.EMalfunctionState.None)
				{
					if (Weapon_0 is RevolverItemClass && Weapon_0.CylinderHammerClosed)
					{
						FirearmsAnimator_0.Animator.Play(FirearmsAnimator_0.FullDoubleActionFireStateName, 1, 0.2f);
					}
					else if (Weapon_0.FireMode.FireMode == Weapon.EFireMode.semiauto)
					{
						FirearmsAnimator_0.Animator.Play(FirearmsAnimator_0.FullSemiFireStateName, 1, 0.2f);
					}
					else
					{
						FirearmsAnimator_0.Animator.Play(FirearmsAnimator_0.FullFireStateName, 1, 0.2f);
					}
				}
			}

			public override void Reset()
			{
				base.Reset();
				SingleShotData_0 = default(SingleShotData);
				Bool_1 = false;
				Float_0 = 0f;
				Bool_2 = false;
				Bool_3 = false;
			}

			public override void OnFireEvent()
			{
				Bool_1 = true;
				MakeShot(SingleShotData_0.AmmoToFire);
				if (Weapon_0.HasChambers)
				{
					FirearmController_0.weaponManagerClass.MoveAmmoFromChamberToShellPort(SingleShotData_0.AmmoToFire.IsUsed);
				}
				FirearmController_0.IsTriggerPressed = false;
				FirearmsAnimator_0.SetFire(FirearmController_0.IsTriggerPressed);
				if (Weapon_0.MalfState.State == Weapon.EMalfunctionState.Jam || Weapon_0.MalfState.State == Weapon.EMalfunctionState.SoftSlide || Weapon_0.MalfState.State == Weapon.EMalfunctionState.HardSlide || Weapon_0.MalfState.State == Weapon.EMalfunctionState.Feed)
				{
					Player_0.InventoryController.ExamineMalfunction(Weapon_0, clearRest: true);
					method_11();
				}
				if (FirearmController_0.method_65())
				{
					Player_0.InventoryController.ProcessFastWeaponSwitchAvailability();
				}
			}

			public virtual void PrepareShot()
			{
				SingleShotData_0 = method_5(out var malfState, out var malfSource);
				Weapon_0.MalfState.State = malfState;
				if (malfState == Weapon.EMalfunctionState.None)
				{
					FirearmController_0.bool_3 = false;
					FirearmsAnimator_0.SetFire(FirearmController_0.IsTriggerPressed);
				}
				else
				{
					Weapon_0.MalfState.LastMalfunctionTime = GClass1891.PastTime;
					FirearmController_0.bool_3 = true;
					if (Player_0.Skills.TroubleFixingDurElite.Value)
					{
						Weapon_0.MalfState.AddMalfReduceChance(Player_0.ProfileId, malfSource);
					}
					FirearmsAnimator_0.MisfireSlideUnknown(val: false);
					if (Weapon_0.MalfState.State == Weapon.EMalfunctionState.Misfire)
					{
						Player_0.InventoryController.ExamineMalfunction(Weapon_0, clearRest: true);
					}
					if (malfState == Weapon.EMalfunctionState.Misfire)
					{
						method_11();
					}
					else
					{
						FirearmsAnimator_0.SetFire(FirearmController_0.IsTriggerPressed);
					}
				}
				ShowIncompatibleNotification();
				FirearmController_0._player.MouseLook();
			}

			public override void RemoveAmmoFromChamber()
			{
				FirearmsAnimator_0.SetAmmoInChamber(0f);
				if (SingleShotData_0.AmmoWillBeLoadedToChamber == null || Weapon_0.MalfState.State != Weapon.EMalfunctionState.None)
				{
					FirearmController_0.bool_3 = true;
				}
			}

			public override void OnAddAmmoInChamber()
			{
				FirearmsAnimator_0.SetAmmoInChamber(SingleShotData_0.AmmoCountInChamberAfterShot);
				FirearmsAnimator_0.SetAmmoOnMag(SingleShotData_0.AmmoCountInMagAfterShot);
				if (SingleShotData_0.AmmoWillBeLoadedToChamber != null)
				{
					FirearmController_0.weaponManagerClass.SetRoundIntoWeapon(SingleShotData_0.AmmoWillBeLoadedToChamber);
				}
				FirearmsAnimator_0.SetFire(FirearmController_0.IsTriggerPressed);
			}

			public override void OnShellEjectEvent()
			{
				if (!Weapon_0.HasChambers && SingleShotData_0.AmmoToFire != null)
				{
					FirearmController_0.weaponManagerClass.SetRoundIntoWeapon(SingleShotData_0.AmmoToFire);
					FirearmController_0.weaponManagerClass.MoveAmmoFromChamberToShellPort(SingleShotData_0.AmmoToFire.IsUsed);
				}
				method_8();
			}

			public override void Update(float deltaTime)
			{
				Float_0 -= deltaTime;
				if (Float_0 <= 0f && Bool_2)
				{
					method_13();
				}
			}

			public override void SetTriggerPressed(bool pressed)
			{
				FirearmController_0.IsTriggerPressed &= pressed;
				Bool_3 |= pressed && Bool_4;
			}

			public override void OnFireEndEvent()
			{
				FirearmController_0.IsTriggerPressed = false;
				if (!Weapon_0.HasChambers && SingleShotData_0.AmmoToFire != null)
				{
					FirearmController_0.weaponManagerClass.DestroyPatronInWeapon();
				}
			}

			public override void OnIdleStartEvent()
			{
				if (Bool_1)
				{
					Bool_2 = true;
				}
			}

			public void method_10()
			{
				if (Weapon_0.HasChambers)
				{
					FirearmsAnimator_0.SetAmmoInChamber(Weapon_0.ChamberAmmoCount);
				}
				FirearmsAnimator_0.SetAmmoOnMag(SingleShotData_0.AmmoCountInMagAfterShot);
				FirearmsAnimator_0.SetInventory(Bool_0);
				FirearmsAnimator_0.SetFire(FirearmController_0.IsTriggerPressed);
				State = EOperationState.Finished;
				FirearmController_0.InitiateOperation<GClass2037>().Start();
				if (Bool_3 && !Bool_0)
				{
					FirearmController_0.CurrentOperation.SetTriggerPressed(pressed: true);
					FirearmController_0.CurrentOperation.SetTriggerPressed(pressed: false);
				}
			}

			public void method_11()
			{
				FirearmsAnimator_0.SetFire(FirearmController_0.IsTriggerPressed);
				FirearmsAnimator_0.Malfunction((int)Weapon_0.MalfState.State);
				State = EOperationState.Finished;
				Weapon_0.MalfState.AmmoToFire = SingleShotData_0.AmmoToFire;
				Weapon_0.MalfState.AmmoWillBeLoadedToChamber = SingleShotData_0.AmmoWillBeLoadedToChamber;
				Weapon_0.MalfState.MalfunctionedAmmo = SingleShotData_0.FedAmmo ?? SingleShotData_0.AmmoToFire;
				FirearmController_0.InitiateOperation<GClass2049>().Start();
			}

			public void method_12()
			{
				FirearmsAnimator_0.SetAmmoInChamber(SingleShotData_0.AmmoCountInChamberAfterShot);
				FirearmsAnimator_0.SetAmmoOnMag(SingleShotData_0.AmmoCountInMagAfterShot);
				FirearmsAnimator_0.SetFire(FirearmController_0.IsTriggerPressed);
				SetAiming(isAiming: false);
				State = EOperationState.Finished;
				Action_0();
			}

			public void method_13()
			{
				if (Action_0 != null)
				{
					FirearmsAnimator_0.SetFire(FirearmController_0.IsTriggerPressed);
					SetAiming(isAiming: false);
					method_12();
				}
				else
				{
					FirearmsAnimator_0.SetFire(FirearmController_0.IsTriggerPressed);
					method_10();
				}
			}

			public override void FastForward()
			{
				base.FastForward();
				if (!Bool_1)
				{
					OnFireEvent();
				}
				Bool_3 = false;
				method_13();
				FirearmsAnimator_0.Animator.Play(FirearmsAnimator_0.FullIdleStateName, 1, 0.2f);
			}
		}

		public class GClass2028 : GClass2013
		{
			public struct SingleShotData
			{
				public AmmoItemClass AmmoToFire;

				[CanBeNull]
				public AmmoItemClass AmmoWillBeLoadedToChamber;

				public AmmoItemClass FedAmmo;

				public int AmmoCountInChamberBeforeShot;

				public int AmmoCountInChamberAfterShot;

				public int AmmoCountInMagBeforeShot;

				public int AmmoCountInMagAfterShot;

				public bool IsAmmoCompatible;
			}

			[CompilerGenerated]
			public class Class1224
			{
				public GClass2028 gclass2028_0;

				public MagazineItemClass magazine;

				public ItemAddress itemAddress;

				public Callback finishCallback;

				public Callback startCallback;

				public void method_0()
				{
					GStruct156<GClass2006> sourceOption = GClass2006.Run(gclass2028_0.FirearmController_0._player.InventoryController, gclass2028_0.FirearmController_0.Item, magazine, quickReload: false, gclass2028_0.FirearmController_0.Item.MalfState.IsKnownMalfunction(gclass2028_0.FirearmController_0._player.ProfileId), itemAddress);
					if (sourceOption.Succeeded)
					{
						gclass2028_0.State = EOperationState.Finished;
						gclass2028_0.FirearmController_0.InitiateOperation<GClass2016>().Start(sourceOption.Value, finishCallback);
						startCallback?.Succeed();
						return;
					}
					Callback callback = finishCallback;
					if (callback != null)
					{
						GClass1617.Invoke(callback, sourceOption);
					}
				}
			}

			[CompilerGenerated]
			public class Class1225
			{
				public GClass2028 gclass2028_0;

				public AmmoPackReloadingClass ammoPack;

				public Callback finishCallback;

				public bool quickReload;

				public Callback startCallback;

				public void method_0()
				{
					gclass2028_0.State = EOperationState.Finished;
					gclass2028_0.FirearmController_0.InitiateOperation<CylinderReloadOperationClass>().Start(ammoPack, finishCallback, quickReload);
					startCallback?.Succeed();
				}
			}

			[CompilerGenerated]
			public class Class1226
			{
				public GClass2028 gclass2028_0;

				public AmmoPackReloadingClass ammoPack;

				public Callback finishCallback;

				public Callback startCallback;

				public void method_0()
				{
					gclass2028_0.State = EOperationState.Finished;
					gclass2028_0.FirearmController_0.InitiateOperation<AmmoPackReloadOperationClass>().Start(ammoPack, finishCallback);
					startCallback?.Succeed();
				}
			}

			[CompilerGenerated]
			public class Class1227
			{
				public GClass2028 gclass2028_0;

				public MagazineItemClass magazine;

				public Callback finishCallback;

				public Callback startCallback;

				public void method_0()
				{
					GStruct156<GClass2006> sourceOption = GClass2006.Run(gclass2028_0.FirearmController_0._player.InventoryController, gclass2028_0.FirearmController_0.Item, magazine, quickReload: true, gclass2028_0.FirearmController_0.Item.MalfState.IsKnownMalfunction(gclass2028_0.FirearmController_0._player.ProfileId), null);
					if (sourceOption.Succeeded)
					{
						gclass2028_0.State = EOperationState.Finished;
						gclass2028_0.FirearmController_0.InitiateOperation<GClass2016>().Start(sourceOption.Value, finishCallback);
						startCallback?.Succeed();
						return;
					}
					Callback callback = finishCallback;
					if (callback != null)
					{
						GClass1617.Invoke(callback, sourceOption);
					}
				}
			}

			[CompilerGenerated]
			public class Class1228
			{
				public GClass2028 gclass2028_0;

				public Action onHidden;

				public bool fastDrop;

				public Item nextControllerItem;

				public void method_0()
				{
					gclass2028_0.State = EOperationState.Finished;
					gclass2028_0.FirearmController_0.InitiateOperation<GClass2053>().Start(onHidden, fastDrop, nextControllerItem);
				}
			}

			[NonSerialized]
			public bool Bool_0;

			[NonSerialized]
			public Action Action_0;

			public GClass2028(FirearmController controller)
				: base(controller)
			{
			}

			public override void SetLeftStanceAnimOnStartOperation()
			{
			}

			public SingleShotData method_5(out Weapon.EMalfunctionState malfState, out Weapon.EMalfunctionSource malfSource)
			{
				method_6(out var ammoToFire, out var _, out var ammoCountInMagBeforeShot);
				BackendConfigSettingsClass instance = Singleton<BackendConfigSettingsClass>.Instance;
				float modsCoolFactor;
				float currentOverheat = Weapon_0.GetCurrentOverheat(GClass1891.PastTime, instance.Overheat, out modsCoolFactor);
				malfState = FirearmController_0.GetMalfunctionState(ammoToFire, ammoCountInMagBeforeShot > 0, Weapon_0.IsBoltCatch, Weapon_0.GetCurrentMagazine() != null, currentOverheat, instance.Overheat.FixSlideOverheat, out malfSource);
				if (!Player_0.IsAI && Weapon_0.Template.AllowMisfire && Player_0.HealthController?.FindActiveEffect<GInterface369>() != null)
				{
					malfState = Weapon.EMalfunctionState.Misfire;
					malfSource = Weapon.EMalfunctionSource.Effect;
				}
				Weapon_0.MalfState.Source = malfSource;
				if (Player_0.IsAI && !instance.Malfunction.AllowMalfForBots)
				{
					malfState = Weapon.EMalfunctionState.None;
				}
				if (!Weapon_0.ValidateMalfunction(malfState))
				{
					malfState = Weapon.EMalfunctionState.None;
				}
				return method_7(malfState);
			}

			public void method_6(out AmmoItemClass ammoToFire, out AmmoItemClass ammoToChamber, out int ammoCountInMagBeforeShot)
			{
				Slot[] chambers = Weapon_0.Chambers;
				ammoToFire = (Weapon_0.HasChambers ? chambers[0] : null)?.ContainedItem as AmmoItemClass;
				ammoToChamber = null;
				MagazineItemClass currentMagazine = Weapon_0.GetCurrentMagazine();
				if (currentMagazine == null)
				{
					ammoCountInMagBeforeShot = 0;
					return;
				}
				ammoCountInMagBeforeShot = currentMagazine.Count;
				if (currentMagazine.IsAmmoCompatible(chambers) && ammoCountInMagBeforeShot > 0)
				{
					if (!Weapon_0.HasChambers)
					{
						ammoToFire = (AmmoItemClass)currentMagazine.Cartridges.Last;
					}
					else
					{
						ammoToChamber = (AmmoItemClass)currentMagazine.Cartridges.Last;
					}
				}
			}

			public SingleShotData method_7(Weapon.EMalfunctionState malfState)
			{
				bool flag = malfState == Weapon.EMalfunctionState.Feed;
				Slot[] chambers = Weapon_0.Chambers;
				SingleShotData result = new SingleShotData
				{
					AmmoCountInChamberBeforeShot = ((!Weapon_0.HasChambers) ? 1 : Weapon_0.ChamberAmmoCount)
				};
				Slot slot = (Weapon_0.HasChambers ? chambers[0] : null);
				AmmoItemClass ammoItemClass = slot?.ContainedItem as AmmoItemClass;
				MagazineItemClass currentMagazine = Weapon_0.GetCurrentMagazine();
				if (Weapon_0.HasChambers)
				{
					_ = slot.RemoveItem().Succeeded;
					ammoItemClass.IsUsed = true;
					result.AmmoToFire = ammoItemClass;
				}
				if (currentMagazine == null)
				{
					result.AmmoCountInChamberAfterShot = 0;
					result.AmmoWillBeLoadedToChamber = null;
					result.IsAmmoCompatible = true;
					return result;
				}
				result.AmmoCountInMagBeforeShot = currentMagazine.Count;
				result.IsAmmoCompatible = currentMagazine.IsAmmoCompatible(chambers);
				if (result.IsAmmoCompatible && result.AmmoCountInMagBeforeShot > 0 && (malfState == Weapon.EMalfunctionState.None || malfState == Weapon.EMalfunctionState.Feed))
				{
					AmmoItemClass ammoItemClass2 = (AmmoItemClass)((!Weapon_0.HasChambers || flag) ? currentMagazine.Cartridges.PopToNowhere(FirearmController_0._player.InventoryController).Value.ResultItem : currentMagazine.Cartridges.PopTo(FirearmController_0._player.InventoryController, Weapon_0.Chambers[0].CreateItemAddress()).Value.ResultItem);
					if (Weapon_0.HasChambers)
					{
						if (flag)
						{
							result.FedAmmo = ammoItemClass2;
						}
						else
						{
							result.AmmoWillBeLoadedToChamber = ammoItemClass2;
						}
					}
					else
					{
						result.AmmoToFire = ammoItemClass2;
						ammoItemClass = ammoItemClass2;
					}
					UncheckOnShot();
				}
				result.AmmoCountInChamberAfterShot = Weapon_0.ChamberAmmoCount;
				result.AmmoCountInMagAfterShot = Weapon_0.GetCurrentMagazineCount();
				return result;
			}

			public override void BlindFire(int b)
			{
				BlindFire_Internal(b);
			}

			public override void Reset()
			{
				Bool_0 = false;
				Action_0 = null;
				base.Reset();
			}

			public override void SetAiming(bool isAiming)
			{
				FirearmController_0.IsAiming = isAiming;
			}

			public override void ReloadMag(MagazineItemClass magazine, ItemAddress itemAddress, Callback finishCallback, Callback startCallback)
			{
				FirearmController_0.IsTriggerPressed = false;
				if (Action_0 == null)
				{
					Action_0 = delegate
					{
						GStruct156<GClass2006> sourceOption = GClass2006.Run(FirearmController_0._player.InventoryController, FirearmController_0.Item, magazine, quickReload: false, FirearmController_0.Item.MalfState.IsKnownMalfunction(FirearmController_0._player.ProfileId), itemAddress);
						if (sourceOption.Succeeded)
						{
							State = EOperationState.Finished;
							FirearmController_0.InitiateOperation<GClass2016>().Start(sourceOption.Value, finishCallback);
							startCallback?.Succeed();
						}
						else
						{
							Callback callback = finishCallback;
							if (callback != null)
							{
								GClass1617.Invoke(callback, sourceOption);
							}
						}
					};
				}
				else
				{
					finishCallback?.Fail("Action is already planned");
				}
			}

			public override void ReloadCylinderMagazine(AmmoPackReloadingClass ammoPack, Callback finishCallback, Callback startCallback, bool quickReload = false)
			{
				FirearmController_0.IsTriggerPressed = false;
				if (Weapon_0.GetCurrentMagazine() != null && Action_0 == null)
				{
					Action_0 = delegate
					{
						State = EOperationState.Finished;
						FirearmController_0.InitiateOperation<CylinderReloadOperationClass>().Start(ammoPack, finishCallback, quickReload);
						startCallback?.Succeed();
					};
				}
			}

			public override void ReloadWithAmmo(AmmoPackReloadingClass ammoPack, Callback finishCallback, Callback startCallback)
			{
				FirearmController_0.IsTriggerPressed = false;
				if (Weapon_0.GetCurrentMagazine() != null && Action_0 == null)
				{
					Action_0 = delegate
					{
						State = EOperationState.Finished;
						FirearmController_0.InitiateOperation<AmmoPackReloadOperationClass>().Start(ammoPack, finishCallback);
						startCallback?.Succeed();
					};
				}
			}

			public override void QuickReloadMag(MagazineItemClass magazine, Callback finishCallback, Callback startCallback)
			{
				FirearmController_0.IsTriggerPressed = false;
				if (Action_0 == null)
				{
					Action_0 = delegate
					{
						GStruct156<GClass2006> sourceOption = GClass2006.Run(FirearmController_0._player.InventoryController, FirearmController_0.Item, magazine, quickReload: true, FirearmController_0.Item.MalfState.IsKnownMalfunction(FirearmController_0._player.ProfileId), null);
						if (sourceOption.Succeeded)
						{
							State = EOperationState.Finished;
							FirearmController_0.InitiateOperation<GClass2016>().Start(sourceOption.Value, finishCallback);
							startCallback?.Succeed();
						}
						else
						{
							Callback callback = finishCallback;
							if (callback != null)
							{
								GClass1617.Invoke(callback, sourceOption);
							}
						}
					};
				}
				else
				{
					finishCallback?.Fail("Action is already planned");
				}
			}

			public override bool CanStartReload()
			{
				return Action_0 == null;
			}

			public override void HideWeapon(Action onHidden, bool fastDrop, Item nextControllerItem = null)
			{
				FirearmController_0.IsTriggerPressed = false;
				Action_0 = delegate
				{
					State = EOperationState.Finished;
					FirearmController_0.InitiateOperation<GClass2053>().Start(onHidden, fastDrop, nextControllerItem);
				};
			}

			public override void OnOnOffBoltCatchEvent(bool isCaught)
			{
				if (Weapon_0.IsBoltCatch)
				{
					FirearmsAnimator_0.SetBoltCatch(isCaught);
				}
			}

			public override void SetInventoryOpened(bool opened)
			{
				Bool_0 = opened;
			}

			public void method_8()
			{
				FirearmController_0.weaponManagerClass.StartSpawnShell(FirearmController_0._player.Velocity * 0.66f);
			}

			public virtual void MakeMultiBarrelShot(List<SingleShotData> singleShotDatas, List<int> chambersForFire)
			{
				bool multiBarrelShot = singleShotDatas.Count > 1;
				for (int i = 0; i < singleShotDatas.Count; i++)
				{
					MakeShot(singleShotDatas[i].AmmoToFire, chambersForFire[i], multiBarrelShot);
				}
			}

			public virtual void MakeShot(AmmoItemClass ammo, int chamberIndex = 0, bool multiBarrelShot = false)
			{
				FirearmController_0.method_58(FirearmController_0.Item, ammo, chamberIndex, multiBarrelShot);
			}

			public virtual void UncheckOnShot()
			{
				Weapon item = FirearmController_0.Item;
				MagazineItemClass currentMagazine = item.GetCurrentMagazine();
				if (currentMagazine != null)
				{
					FirearmController_0._player.InventoryController.CheckChamber(item, FirearmController_0._player.Profile.CheckedMagazines.ContainsKey(currentMagazine.Id));
					if (Singleton<BackendConfigSettingsClass>.Instance.UncheckOnShot)
					{
						FirearmController_0._player.InventoryController.CheckMagazineAmmoDepend(currentMagazine, method_9, useOperation: false, allowUncheck: true);
					}
				}
				else
				{
					FirearmController_0._player.InventoryController.CheckChamber(item, status: false);
				}
			}

			public void method_9()
			{
			}
		}

		public class FlareGunFireOperationClass : GenericFireOperationClass
		{
			[NonSerialized]
			public AmmoItemClass AmmoItemClass;

			public FlareGunFireOperationClass(FirearmController controller)
				: base(controller)
			{
			}

			public override void Start()
			{
				base.Start();
				FirearmsAnimator_0.SetFire(FirearmController_0.IsTriggerPressed);
				Player_0.InventoryController.RaiseEvent(new GEventArgs4(Weapon_0, CommandStatus.Begin, Player_0.InventoryController));
			}

			public override void Reset()
			{
				AmmoItemClass = null;
				base.Reset();
			}

			public override void PrepareShot()
			{
			}

			public override void OnFireEvent()
			{
				Bool_1 = true;
				AmmoItemClass = Weapon_0.FirstLoadedChamberSlot.ContainedItem as AmmoItemClass;
				if (AmmoItemClass != null && !AmmoItemClass.IsUsed)
				{
					AmmoItemClass.IsUsed = true;
					FirearmController_0.method_55(AmmoItemClass);
					FirearmController_0.weaponManagerClass.MoveAmmoFromChamberToShellPort(AmmoItemClass.IsUsed);
					Weapon_0.FirstLoadedChamberSlot.RemoveItem();
				}
			}

			public override void SetTriggerPressed(bool pressed)
			{
				FirearmController_0.IsTriggerPressed &= pressed;
			}

			public override void OnFireEndEvent()
			{
				Player_0.InventoryController.RaiseEvent(new GEventArgs4(Weapon_0, CommandStatus.Succeed, Player_0.InventoryController));
				SetTriggerPressed(pressed: false);
				FirearmsAnimator_0.SetFire(fire: false);
				Weapon_0.ShellsInChambers[0] = (AmmoTemplate)AmmoItemClass.Template;
				FirearmsAnimator_0.SetAmmoInChamber(Weapon_0.ChamberAmmoCount);
				FirearmsAnimator_0.SetShellsInWeapon(Weapon_0.ShellsInWeaponCount);
				State = EOperationState.Finished;
				FirearmController_0.InitiateOperation<GClass2037>().Start();
			}

			public override bool CanNotBeInterrupted()
			{
				return true;
			}
		}

		public class Class1269 : GClass2013
		{
			[NonSerialized]
			public Callback Callback_0;

			[NonSerialized]
			public FoldOperationClass FoldOperationClass;

			[NonSerialized]
			public bool Bool_0;

			[NonSerialized]
			public Action Action_0;

			[NonSerialized]
			public bool Bool_1;

			public Class1269(FirearmController controller)
				: base(controller)
			{
			}

			public void Start(FoldOperationClass foldOperation, Callback callback)
			{
				Bool_0 = false;
				FoldOperationClass = foldOperation;
				FoldableComponent foldable = FoldOperationClass.Foldable;
				FirearmsAnimator_0.SetInventory(open: false);
				Player_0.MovementContext.SetBlindFire(0);
				Player_0.BodyAnimatorCommon.SetFloat(PlayerAnimator.RELOAD_FLOAT_PARAM_HASH, 1f);
				if (foldable.CanBeFolded)
				{
					Callback_0 = callback;
					FirearmsAnimator_0.TriggerFold();
				}
				else
				{
					UnityEngine.Debug.LogError("FoldOperation can't fold anything");
					OnIdleStartEvent();
				}
			}

			public override void OnFold(bool b)
			{
				method_5();
			}

			public override void SetInventoryOpened(bool opened)
			{
				FirearmController_0.InventoryOpened = opened;
			}

			public void method_5()
			{
				State = EOperationState.Finished;
				FirearmController_0.RecalculateErgonomic();
				FirearmsAnimator_0.Fold(FoldOperationClass.NewValue);
				FirearmsAnimator_0.SetInventory(FirearmController_0.InventoryOpened);
				if (Bool_0)
				{
					FirearmController_0.InitiateOperation<GClass2037>().HideWeapon(Action_0, Bool_1);
				}
				else
				{
					FirearmController_0.InitiateOperation<GClass2037>().Start();
				}
				FoldOperationClass = null;
				if (Callback_0 != null)
				{
					Callback callback_ = Callback_0;
					Callback_0 = null;
					callback_.Succeed();
				}
				Player_0.BodyAnimatorCommon.SetFloat(PlayerAnimator.WEAPON_SIZE_MODIFIER_PARAM_HASH, Weapon_0.CalculateCellSize().X);
				Player_0.ProceduralWeaponAnimation.UpdateWeaponVariables();
			}

			public override void HideWeapon(Action onHidden, bool fastDrop, Item nextControllerItem = null)
			{
				Bool_0 = true;
				Action_0 = onHidden;
				Bool_1 = fastDrop;
			}

			public override void FastForward()
			{
				method_5();
				FirearmsAnimator_0.Animator.Play(FirearmsAnimator_0.FullIdleStateName, 1, 0.2f);
			}
		}

		public class GClass2037 : GClass2013
		{
			[CompilerGenerated]
			public class Class1229
			{
				public GClass2037 gclass2037_0;

				public AmmoItemClass ammo;

				public GInterface443 oneItemOperation;

				public Callback callback;

				public Action action_0;

				public Action action_1;

				public void method_0()
				{
					gclass2037_0.FirearmController_0.InitiateOperation<GClass2056>().Start(isLauncherEnabled: true, delegate
					{
						gclass2037_0.FirearmController_0.CurrentOperation.LauncherInventoryUnchamberFromMainWeapon(ammo, 0, oneItemOperation.To1, callback);
					});
				}

				public void method_1()
				{
					gclass2037_0.FirearmController_0.CurrentOperation.LauncherInventoryUnchamberFromMainWeapon(ammo, 0, oneItemOperation.To1, callback);
				}

				public void method_2()
				{
					gclass2037_0.FirearmController_0.InitiateOperation<GClass2056>().Start(isLauncherEnabled: true, delegate
					{
						gclass2037_0.FirearmController_0.CurrentOperation.LoadLauncherFromMainWeapon(ammo, 0, oneItemOperation.To1, callback);
					});
				}

				public void method_3()
				{
					gclass2037_0.FirearmController_0.CurrentOperation.LoadLauncherFromMainWeapon(ammo, 0, oneItemOperation.To1, callback);
				}
			}

			[NonSerialized]
			public float Float_0;

			[NonSerialized]
			public bool Bool_0;

			[NonSerialized]
			public float Float_1;

			[NonSerialized]
			public Action Action_0;

			[NonSerialized]
			public Action Action_1;

			[NonSerialized]
			public float Float_2;

			public GClass2037(FirearmController controller)
				: base(controller)
			{
			}

			public override void BlindFire(int b)
			{
				BlindFire_Internal(b);
			}

			public virtual void Start(Action callback = null)
			{
				base.Start();
				Player_0.ProceduralWeaponAnimation.TacticalReload = false;
				Action_0 = callback;
				Float_0 = 0f;
				Bool_0 = false;
				Float_1 = 0f;
				FirearmController_0.SetAnimatorAndProceduralValues();
				if (Weapon_0.IsUnderBarrelDeviceActive)
				{
					FirearmController_0.ToggleLauncher(callback);
				}
				method_5();
				FirearmController_0.method_64();
			}

			public void method_5()
			{
				if (Action_0 != null)
				{
					Action_0();
					Action_0 = null;
				}
			}

			public void method_6()
			{
				if (Action_1 != null)
				{
					Action_1();
					Action_1 = null;
				}
			}

			public override void OnIdleStartEvent()
			{
				SetLeftStanceAnimOnStartOperation();
				Player_0.BodyAnimatorCommon.SetFloat(PlayerAnimator.RELOAD_FLOAT_PARAM_HASH, 0f);
				FirearmController_0.bool_0 = false;
				FirearmController_0.bool_1 = false;
				method_6();
			}

			public override void OnEnd()
			{
				FirearmController_0.SetCompassState(active: false);
			}

			public override void Update(float deltaTime)
			{
				if (!FirearmController_0.IsAiming && !FirearmController_0.InventoryOpened && FirearmsAnimator_0.IsIdling())
				{
					Float_0 += deltaTime;
				}
				else
				{
					Float_0 = 0f;
				}
				if ((double)Float_0 > EFTHardSettings.Instance.IDLING_MAX_TIME && (double)Player_0.MovementIdlingTime > EFTHardSettings.Instance.IDLING_MAX_TIME)
				{
					FirearmsAnimator_0.Idle();
					Float_0 = 0f;
				}
				ProcessAutoshot();
				ProcessRemoveOneOffWeapon();
				method_7();
			}

			public override void OnDropWeapon()
			{
				Player_0.InventoryController.ThrowItem(Weapon_0);
			}

			public virtual void ProcessRemoveOneOffWeapon()
			{
				if (FirearmController_0.Item.IsOneOff && FirearmController_0.Item.Repairable.Durability == 0f && Player_0.InventoryController.CanThrow(Weapon_0))
				{
					Player_0.InventoryController.TryThrowItem(Weapon_0);
				}
			}

			public void method_7()
			{
				float layerWeight = FirearmsAnimator_0.GetLayerWeight(FirearmsAnimator_0.LACTIONS_LAYER_INDEX);
				if (Float_2 >= 1f && layerWeight < Float_2)
				{
					SetLeftStanceAnimOnStartOperation();
				}
				Float_2 = layerWeight;
			}

			public virtual void ProcessAutoshot()
			{
				if (!FirearmController_0.Item.MalfState.AutoshotChanceInited || FirearmController_0.Item.MalfState.AutoshotTime <= 0f)
				{
					return;
				}
				if (FirearmController_0.Item.MalfState.State != Weapon.EMalfunctionState.None)
				{
					FirearmController_0.Item.MalfState.AutoshotTime = -1f;
				}
				else if (GClass1891.PastTime >= FirearmController_0.Item.MalfState.AutoshotTime)
				{
					FirearmController_0.Item.MalfState.AutoshotTime = -1f;
					Weapon.EFireMode selectedFireMode = FirearmController_0.Item.SelectedFireMode;
					FirearmController_0.Item.FireMode.FireMode = Weapon.EFireMode.single;
					SetTriggerPressed(pressed: true);
					FirearmController_0.Item.FireMode.FireMode = selectedFireMode;
					if (selectedFireMode == Weapon.EFireMode.semiauto)
					{
						SetTriggerPressed(pressed: false);
					}
				}
			}

			public override void OnSprintStart()
			{
				SetAiming(isAiming: false);
			}

			public override void OnSprintFinished()
			{
			}

			public override void OnAimingDisabled()
			{
				SetAiming(isAiming: false);
			}

			public override void OnJumpOrFall()
			{
				SetAiming(isAiming: false);
			}

			public void DisableAimingOnReload()
			{
				if (FirearmController_0.CurrentMasteringLevel < 2 || Weapon_0.IsBeltMachineGun)
				{
					SetAiming(isAiming: false);
				}
			}

			public override void SetAiming(bool isAiming)
			{
				if ((!isAiming || EFTHardSettings.Instance.CanAimInState(Player_0.CurrentState.Name)) && (!isAiming || !FirearmController_0.Blindfire) && (!isAiming || !(FirearmController_0.float_2 > EFTHardSettings.Instance.STOP_AIMING_AT) || Player_0.MovementContext.IsInMountedState) && (!isAiming || FirearmController_0.FirearmsAnimator.IsIdling() || FirearmController_0.Item.Template.ReloadMode != Weapon.EReloadMode.OnlyBarrel || FirearmController_0.Item is RocketLauncherItemClass) && (!isAiming || FirearmController_0.FirearmsAnimator.IsIdling() || !Weapon_0.IsBeltMachineGun))
				{
					FirearmController_0.IsAiming = isAiming;
					Float_0 = 0f;
				}
			}

			public override void SetInventoryOpened(bool opened)
			{
				SetAiming(isAiming: false);
				if (Weapon_0 is RocketLauncherItemClass)
				{
					FirearmsAnimator_0.SetAimingFloat(0f);
				}
				SetTriggerPressed(pressed: false);
				FirearmController_0.InventoryOpened = opened;
				FirearmsAnimator_0.SetInventory(opened);
			}

			public override void SetTriggerPressed(bool pressed)
			{
				if ((pressed && !FirearmController_0.SuitableForHandInput) || (pressed && FirearmController_0.bool_1) || (Weapon_0 is RocketLauncherItemClass && !FirearmController_0.IsAiming && FirearmController_0.bool_0) || (Singleton<GameWorld>.Instance.LocationId == "hideout" && Weapon_0 is RocketLauncherItemClass))
				{
					return;
				}
				if (!FirearmController_0.CanPressTrigger())
				{
					if (!pressed)
					{
						FirearmController_0.IsTriggerPressed = pressed;
						FirearmsAnimator_0.SetFire(pressed);
					}
					UnityEngine.Debug.Log("SetTriggerPressed has been halted. Waiting for network callback...");
					return;
				}
				if (pressed)
				{
					ShowIncompatibleNotification();
				}
				if (Weapon_0.HasChambers)
				{
					FirearmsAnimator_0.SetAmmoInChamber(Weapon_0.ChamberAmmoCount);
				}
				FirearmsAnimator_0.SetAmmoOnMag(Weapon_0.GetCurrentMagazineCount());
				FirearmController_0.IsTriggerPressed = pressed;
				if (Weapon_0.HasMagazineWithBelt() && ((FirearmController_0.bool_0 && Weapon_0.WithAnimatorAiming && Weapon_0.GetCurrentMagazineCount() > 0 && !FirearmController_0.bool_7) || FirearmController_0.bool_1))
				{
					return;
				}
				if (FirearmController_0.IsTriggerPressed && Weapon_0.MalfState.State == Weapon.EMalfunctionState.None && (Weapon_0.ChamberAmmoCount > 0 || Weapon_0.IsOneOff || (!Weapon_0.HasChambers && Weapon_0.GetCurrentMagazineCount() > 0)) && !(Weapon_0 is RevolverItemClass))
				{
					State = EOperationState.Finished;
					if (FirearmController_0.Item.SelectedFireMode != Weapon.EFireMode.single && FirearmController_0.Item.SelectedFireMode != Weapon.EFireMode.doublet && FirearmController_0.Item.SelectedFireMode != Weapon.EFireMode.semiauto)
					{
						FirearmController_0.InitiateOperation<GClass2029>().Start();
					}
					else
					{
						FirearmController_0.InitiateOperation<GenericFireOperationClass>().Start();
					}
					return;
				}
				if (FirearmController_0.IsTriggerPressed && Weapon_0.MalfState.State == Weapon.EMalfunctionState.None && Weapon_0 is RevolverItemClass)
				{
					State = EOperationState.Finished;
					FirearmController_0.InitiateOperation<GenericFireOperationClass>().Start();
					return;
				}
				FirearmsAnimator_0.SetFire(pressed);
				if (Weapon_0.MalfState.State != Weapon.EMalfunctionState.None && pressed)
				{
					FirearmController_0.FirearmsAnimator.MisfireSlideUnknown(val: false);
					Player_0.InventoryController.ExamineMalfunction(Weapon_0);
				}
				if (pressed)
				{
					FirearmController_0.DryShot();
				}
			}

			public override void ShowIncompatibleNotification()
			{
				MagazineItemClass currentMagazine = Weapon_0.GetCurrentMagazine();
				bool flag = currentMagazine?.IsAmmoCompatible(Weapon_0.Chambers) ?? false;
				if (currentMagazine != null && !flag)
				{
					NotificationManagerClass.DisplaySingletonWarningNotification(string.Format(GClass2348.Localized("Ammo ({0}) is not compatible. Need: {1}"), GClass2348.Localized(currentMagazine.Cartridges.Last.Name), Weapon_0.AmmoCaliber));
				}
				Weapon_0.CompatibleAmmo = flag || currentMagazine == null;
			}

			public override bool ChangeFireMode(Weapon.EFireMode fireMode)
			{
				Weapon.EFireMode selectedFireMode = Weapon_0.SelectedFireMode;
				if (Weapon_0.IsBoltCatch && Weapon_0.NoFiremodeOnBoltcatch && FirearmsAnimator_0.GetBoltCatch())
				{
					return false;
				}
				if (selectedFireMode != fireMode)
				{
					method_8(fireMode);
					FirearmController_0.Item.FireMode.SetFireMode(fireMode);
					FirearmsAnimator_0.SetFireMode(FirearmController_0.Item.SelectedFireMode);
					FirearmController_0.SetCompassState(active: false);
					if (Player_0.ArmsAnimatorCommon.HasParameter(WeaponAnimationSpeedControllerClass.BOOL_FIREMODE_SPRINT))
					{
						Player_0.BodyAnimatorCommon.SetFloat(PlayerAnimator.RELOAD_FLOAT_PARAM_HASH, 1f);
					}
					method_9();
					return true;
				}
				method_9();
				return false;
			}

			public void method_8(Weapon.EFireMode fireMode)
			{
				if (Weapon_0 is RevolverItemClass && fireMode == Weapon.EFireMode.single)
				{
					if (Weapon_0.CylinderHammerClosed)
					{
						CylinderMagazineItemClass cylinderMagazineItemClass = Weapon_0.GetCurrentMagazine() as CylinderMagazineItemClass;
						cylinderMagazineItemClass.IncrementCamoraIndex();
						FirearmsAnimator_0.SetCamoraIndex(cylinderMagazineItemClass.CurrentCamoraIndex);
						FirearmsAnimator_0.HammerArmed();
					}
					FirearmsAnimator_0.SetDoubleAction(0f);
					Weapon_0.CylinderHammerClosed = false;
				}
			}

			public override bool CheckFireMode()
			{
				method_9();
				if (Weapon_0.FireMode.AvailableEFireModes.Length <= 1)
				{
					return false;
				}
				if (FirearmController_0._player.InventoryController.CheckItemAction(FirearmController_0.Item, FirearmController_0.Item.CurrentAddress).Failed)
				{
					return false;
				}
				SetAiming(isAiming: false);
				if (!(Weapon_0 is RevolverItemClass))
				{
					FirearmsAnimator_0.TriggerFiremodeCheck();
				}
				if (!(Weapon_0 is RevolverItemClass))
				{
					RunUtilityOperation(GClass2038.EUtilityType.CheckFireMode);
				}
				return true;
			}

			public void method_9()
			{
				if (Weapon_0 != null && Player_0.FirstPersonPointOfView)
				{
					Player_0.OnShowFireMode?.Invoke(Weapon_0.SelectedFireMode);
				}
			}

			public override void ShowGesture(EInteraction gesture)
			{
				Player_0.MovementContext.LeftStanceController.DisableLeftStanceAnimFromHandsAction();
				FirearmsAnimator_0.Gesture(gesture);
			}

			public override void SetFirearmCompassState(bool active)
			{
				if (!active || !FirearmController_0.Blindfire)
				{
					if (active)
					{
						Player_0.MovementContext.LeftStanceController.DisableLeftStanceAnimFromHandsAction();
					}
					FirearmController_0.CompassState.Value = active;
					if (active && FirearmController_0.IsAiming)
					{
						FirearmController_0.SetAim(value: false);
					}
				}
			}

			public override bool ExamineWeapon()
			{
				if (FirearmController_0.Item.MalfState.State != Weapon.EMalfunctionState.None)
				{
					if ((FirearmController_0.Item.MalfState.State == Weapon.EMalfunctionState.Misfire || FirearmController_0.Item.MalfState.State == Weapon.EMalfunctionState.SoftSlide || FirearmController_0.Item.MalfState.State == Weapon.EMalfunctionState.HardSlide) && !FirearmController_0.Item.MalfState.IsKnownMalfunction(Player_0.ProfileId))
					{
						if (FirearmController_0.IsAiming)
						{
							return false;
						}
						RunUtilityOperation(GClass2038.EUtilityType.ExamineWeapon);
					}
					else
					{
						State = EOperationState.Finished;
						FirearmController_0.InitiateOperation<GClass2047>().Start();
					}
				}
				else
				{
					if (FirearmController_0.IsAiming)
					{
						return false;
					}
					RunUtilityOperation(GClass2038.EUtilityType.ExamineWeapon);
				}
				FirearmsAnimator_0.LookTrigger();
				return true;
			}

			public override void RollCylinder(Callback finishCallback, bool rollToZeroCamora)
			{
				if (Weapon_0 is RevolverItemClass)
				{
					State = EOperationState.Finished;
					FirearmController_0.InitiateOperation<GClass2054>().Start(finishCallback, rollToZeroCamora);
				}
			}

			public override void HideWeapon(Action onHidden, bool fastDrop, Item nextControllerItem = null)
			{
				Player_0.PreviousWeaponAimState = FirearmController_0._isAiming;
				BlindFire(0);
				SetAiming(isAiming: false);
				SetTriggerPressed(pressed: false);
				State = EOperationState.Finished;
				FirearmController_0.InitiateOperation<GClass2053>().Start(onHidden, fastDrop, nextControllerItem);
			}

			public override bool CanRemove()
			{
				return true;
			}

			public override void ReloadMag(MagazineItemClass magazine, ItemAddress itemAddress, Callback finishCallback, Callback startCallback)
			{
				DisableAimingOnReload();
				SetTriggerPressed(pressed: false);
				GStruct156<GClass2006> sourceOption = GClass2006.Run(Player_0.InventoryController, Weapon_0, magazine, quickReload: false, Weapon_0.MalfState.IsKnownMalfunction(Player_0.ProfileId), itemAddress);
				if (sourceOption.Succeeded)
				{
					State = EOperationState.Finished;
					FirearmController_0.InitiateOperation<GClass2016>().Start(sourceOption.Value, finishCallback);
					startCallback?.Succeed();
				}
				else if (finishCallback != null)
				{
					GClass1617.Invoke(finishCallback, sourceOption);
				}
			}

			public override void QuickReloadMag(MagazineItemClass magazine, Callback finishCallback, Callback startCallback)
			{
				DisableAimingOnReload();
				SetTriggerPressed(pressed: false);
				GStruct156<GClass2006> sourceOption = GClass2006.Run(Player_0.InventoryController, Weapon_0, magazine, quickReload: true, Weapon_0.MalfState.IsKnownMalfunction(Player_0.ProfileId), null);
				if (sourceOption.Succeeded)
				{
					State = EOperationState.Finished;
					FirearmController_0.InitiateOperation<GClass2016>().Start(sourceOption.Value, finishCallback);
					startCallback?.Succeed();
				}
				else if (finishCallback != null)
				{
					GClass1617.Invoke(finishCallback, sourceOption);
				}
			}

			public override void ReloadWithAmmo(AmmoPackReloadingClass ammoPack, Callback finishCallback, Callback startCallback)
			{
				DisableAimingOnReload();
				SetTriggerPressed(pressed: false);
				MagazineItemClass currentMagazine = Weapon_0.GetCurrentMagazine();
				if (currentMagazine != null && currentMagazine.Count < currentMagazine.MaxCount)
				{
					State = EOperationState.Finished;
					FirearmController_0.InitiateOperation<AmmoPackReloadOperationClass>().Start(ammoPack, finishCallback);
					startCallback?.Succeed();
				}
				else
				{
					finishCallback?.Fail("Cant perform reload internal mag operation");
				}
			}

			public override void ReloadCylinderMagazine(AmmoPackReloadingClass ammoPack, Callback finishCallback, Callback startCallback, bool quickReload = false)
			{
				DisableAimingOnReload();
				SetTriggerPressed(pressed: false);
				MagazineItemClass currentMagazine = Weapon_0.GetCurrentMagazine();
				if (currentMagazine != null && (quickReload || currentMagazine.Count < currentMagazine.MaxCount))
				{
					State = EOperationState.Finished;
					FirearmController_0.InitiateOperation<CylinderReloadOperationClass>().Start(ammoPack, finishCallback, quickReload);
					startCallback?.Succeed();
				}
				else
				{
					finishCallback?.Fail("Cant perform reload internal mag operation");
				}
			}

			public override void ReloadBarrels(AmmoPackReloadingClass ammoPack, ItemAddress placeToPutContainedAmmoMagazine, Callback finishCallback, Callback startCallback)
			{
				DisableAimingOnReload();
				SetTriggerPressed(pressed: false);
				if (Weapon_0.Chambers.Length > 1)
				{
					GStruct156<ReloadMultiBarrelResultClass> sourceOption = ReloadMultiBarrelResultClass.Run(Player_0.InventoryController, Player_0.InventoryController, Weapon_0, ammoPack, placeToPutContainedAmmoMagazine);
					if (sourceOption.Error == null)
					{
						State = EOperationState.Finished;
						FirearmController_0.InitiateOperation<MutliBarrelReloadOperationClass>().Start(sourceOption.Value, finishCallback);
						startCallback?.Succeed();
					}
					else if (finishCallback != null)
					{
						GClass1617.Invoke(finishCallback, sourceOption);
					}
				}
				else
				{
					GStruct156<ReloadSingleBarrelResultClass> sourceOption2 = ReloadSingleBarrelResultClass.Run(Player_0.InventoryController, Player_0.InventoryController, Weapon_0, ammoPack.GetAmmoToReload(0), placeToPutContainedAmmoMagazine);
					if (sourceOption2.Error == null)
					{
						State = EOperationState.Finished;
						FirearmController_0.InitiateOperation<SingleBarrelReloadOperationClass>().Start(sourceOption2.Value, finishCallback);
						startCallback?.Succeed();
					}
					else if (finishCallback != null)
					{
						GClass1617.Invoke(finishCallback, sourceOption2);
					}
				}
			}

			public override bool CheckAmmo()
			{
				if (!(FirearmController_0 == null) && Weapon_0 != null)
				{
					MagazineItemClass currentMagazine = Weapon_0.GetCurrentMagazine();
					if (currentMagazine != null && !FirearmController_0._player.InventoryController.CheckItemAction(FirearmController_0.Item, FirearmController_0.Item.CurrentAddress).Failed && !FirearmController_0._player.InventoryController.CheckItemAction(currentMagazine, currentMagazine.CurrentAddress).Failed)
					{
						if (Weapon_0.MalfState.State != Weapon.EMalfunctionState.Feed && (Weapon_0.MalfState.State == Weapon.EMalfunctionState.None || !Weapon_0.IsBoltCatch))
						{
							if (Weapon_0 is RevolverItemClass || Weapon_0.Chambers.Length > 1)
							{
								Player_0.InventoryController.CheckChamber(Weapon_0, status: true);
							}
							if (Player_0.FirstPersonPointOfView)
							{
								AmmoItemClass ammoItemClass = ((!(Weapon_0 is RevolverItemClass)) ? (currentMagazine.Cartridges.Last as AmmoItemClass) : ((CylinderMagazineItemClass)currentMagazine)?.GetFirstAmmo(!Weapon_0.CylinderHammerClosed));
								Player_0.OnShowAmmoDetails?.Invoke(currentMagazine.Count, currentMagazine.MaxCount, Mathf.Max(Player_0.Profile.MagDrillsMastering, currentMagazine.CheckOverride), (ammoItemClass != null) ? GClass2348.Localized(ammoItemClass.Name) : null, Weapon_0 is RevolverItemClass || Weapon_0.Chambers.Length > 1);
								Player_0.InventoryController.StrictCheckMagazine(currentMagazine, status: true);
							}
							SetAiming(isAiming: false);
							FirearmsAnimator_0.CheckAmmo();
							bool isExternalMag = Weapon_0.ReloadMode == Weapon.EReloadMode.ExternalMagazine || Weapon_0.ReloadMode == Weapon.EReloadMode.ExternalMagazineWithInternalReloadSupport || (Weapon_0.ReloadMode == Weapon.EReloadMode.InternalMagazine && currentMagazine == null);
							FirearmsAnimator_0.SetIsExternalMag(isExternalMag);
							if (FirearmController_0._player.MovementContext.StationaryWeapon != null)
							{
								return true;
							}
							RunUtilityOperation(GClass2038.EUtilityType.CheckMagazine);
							return true;
						}
						SetAiming(isAiming: false);
						FirearmController_0.FirearmsAnimator.MisfireSlideUnknown(val: false);
						Player_0.InventoryController.ExamineMalfunction(FirearmController_0.Item);
						return false;
					}
					return false;
				}
				UnityEngine.Debug.LogError("Controller or Item in it is equal to null");
				return false;
			}

			public override bool CheckChamber()
			{
				if (FirearmController_0.IsTriggerPressed)
				{
					return false;
				}
				if (FirearmController_0._player.MovementContext.StationaryWeapon != null)
				{
					return false;
				}
				if (Weapon_0 is RevolverItemClass)
				{
					return false;
				}
				if (Weapon_0.MalfState.State == Weapon.EMalfunctionState.None)
				{
					if (FirearmController_0._player.InventoryController.CheckItemAction(FirearmController_0.Item, FirearmController_0.Item.CurrentAddress).Failed)
					{
						return false;
					}
					SetAiming(isAiming: false);
					FirearmsAnimator_0.CheckChamber();
					Player_0.InventoryController.CheckChamber(Weapon_0, status: true);
					RunUtilityOperation(GClass2038.EUtilityType.CheckChamber);
				}
				else
				{
					SetAiming(isAiming: false);
					if (Weapon_0.MalfState.IsKnownMalfType(Player_0.ProfileId))
					{
						State = EOperationState.Finished;
						FirearmController_0.InitiateOperation<FixMalfunctionOperationClass>().Start();
					}
					else
					{
						FirearmsAnimator_0.MisfireSlideUnknown(val: false);
						Player_0.InventoryController.ExamineMalfunction(FirearmController_0.Item);
					}
				}
				return true;
			}

			public override bool CanStartReload()
			{
				return true;
			}

			public virtual void RunUtilityOperation(GClass2038.EUtilityType utilityType)
			{
				Player_0.BodyAnimatorCommon.SetFloat(PlayerAnimator.RELOAD_FLOAT_PARAM_HASH, 1f);
				State = EOperationState.Finished;
				FirearmController_0.InitiateOperation<GClass2038>().Start(utilityType);
			}

			public override void Execute(GInterface438 operation, Callback callback)
			{
				if (operation is GClass3513 gClass)
				{
					if (gClass.InternalOperation is GInterface443 gInterface && Weapon_0.SupportsInternalReload && gInterface.Item1 is AmmoItemClass item)
					{
						FirearmController_0.ReloadWithAmmo(new AmmoPackReloadingClass(new List<AmmoItemClass> { item }), callback);
					}
					else
					{
						callback?.Fail($"Failed to load ammo into a weapon with \"{Weapon_0.ReloadMode}\" reload mode.");
					}
					return;
				}
				if (!(operation is GInterface443 gInterface2))
				{
					callback.Succeed();
					return;
				}
				if (!FirearmController_0.method_21(operation))
				{
					if (Player_0.InventoryController.IsAnimatedSlot(gInterface2.From1))
					{
						State = EOperationState.Finished;
						FirearmController_0.InitiateOperation<GClass2026>().Start(gInterface2.Item1, callback);
					}
					else
					{
						callback.Succeed();
					}
					return;
				}
				FirearmController_0.IsTriggerPressed = false;
				if (operation is FoldOperationClass foldOperation)
				{
					State = EOperationState.Finished;
					FirearmController_0.InitiateOperation<Class1269>().Start(foldOperation, callback);
				}
				else if (gInterface2.Item1 is AmmoItemClass ammo)
				{
					MagazineItemClass currentMagazine = Weapon_0.GetCurrentMagazine();
					bool flag = Weapon_0 is RevolverItemClass;
					if (!Weapon_0.HasChambers && !flag)
					{
						callback.Fail($"Can't perform chambers operation '{operation}' in the weapon without chambers");
						return;
					}
					LauncherItemClass underbarrelWeapon = FirearmController_0.UnderbarrelWeapon;
					if (underbarrelWeapon != null && ((gInterface2.From1 != null && gInterface2.From1.Container == underbarrelWeapon.Chamber) || (gInterface2.To1 != null && gInterface2.To1.Container == underbarrelWeapon.Chamber)))
					{
						method_10(gInterface2, ammo, callback);
						return;
					}
					if (flag)
					{
						method_11(gInterface2, ammo, callback);
						return;
					}
					if (operation is GClass3505 gClass2)
					{
						if (Weapon_0.ReloadMode != Weapon.EReloadMode.OnlyBarrel)
						{
							State = EOperationState.Finished;
							FirearmController_0.InitiateOperation<RechamberOperationClass>().Start(gClass2.AmmoInChamber, callback);
						}
						else
						{
							callback.Fail("This weapon can't perform RechamberOperation");
						}
						return;
					}
					string text = null;
					int num = 0;
					while (true)
					{
						if (num < Weapon_0.Chambers.Length)
						{
							Slot slot = Weapon_0.Chambers[num];
							Item containedItem = slot.ContainedItem;
							if (gInterface2.From1 != null && gInterface2.From1.Container == slot && containedItem == gInterface2.Item1)
							{
								if ((Weapon_0.ReloadMode == Weapon.EReloadMode.ExternalMagazine || Weapon_0.ReloadMode == Weapon.EReloadMode.ExternalMagazineWithInternalReloadSupport) && currentMagazine != null)
								{
									text = $"Can't perform operation {operation} while mag in the weapon";
								}
								else
								{
									if (Weapon_0.ReloadMode != Weapon.EReloadMode.InternalMagazine || Weapon_0.GetCurrentMagazineCount() <= 0)
									{
										ItemAddress destinationAddress = null;
										if (gInterface2 is GInterface445 { BaseInventoryOperation: MoveOperationClass baseInventoryOperation } && Weapon_0.Chambers.Contains(baseInventoryOperation.To.Container))
										{
											destinationAddress = baseInventoryOperation.To;
										}
										State = EOperationState.Finished;
										FirearmController_0.InitiateOperation<GClass2024>().Start((AmmoItemClass)gInterface2.Item1, num, destinationAddress, callback);
										Player_0.ExecuteSkill((Action)delegate
										{
											Player_0.Skills.WeaponChamberAction.Complete(Weapon_0);
										});
										return;
									}
									text = $"Can't perform operation {operation} while mag is not empty";
								}
							}
							else if (gInterface2.To1 != null && gInterface2.To1.Container == slot)
							{
								if (!Weapon_0.CanLoadAmmoToChamber)
								{
									text = $"Can't perform chambers operation in the weapon that can't load ammo to chamber, operation: {operation}";
								}
								else if ((Weapon_0.ReloadMode == Weapon.EReloadMode.ExternalMagazine || Weapon_0.ReloadMode == Weapon.EReloadMode.ExternalMagazineWithInternalReloadSupport) && currentMagazine != null)
								{
									text = $"Can't perform operation {operation} while mag in the weapon";
								}
								else
								{
									if (Weapon_0.ReloadMode != Weapon.EReloadMode.InternalMagazine || currentMagazine == null || currentMagazine.Cartridges.Count != currentMagazine.MaxCount || GClass3124.CanAccept(slot, currentMagazine.Cartridges.Last))
									{
										break;
									}
									text = $"Can't perform operation {operation} - mag is full and last patron is not valid for chamber";
								}
							}
							num++;
							continue;
						}
						callback.Fail(text ?? $"Can't perform operation {operation}: {gInterface2.Item1} {gInterface2.From1}->{gInterface2.To1}");
						return;
					}
					State = EOperationState.Finished;
					FirearmController_0.InitiateOperation<GClass2045>().Start((AmmoItemClass)gInterface2.Item1, num, callback);
					Player_0.ExecuteSkill((Action)delegate
					{
						Player_0.Skills.WeaponChamberAction.Complete(Weapon_0);
					});
				}
				else if (gInterface2.Item1 is MagazineItemClass)
				{
					if (Weapon_0.ReloadMode == Weapon.EReloadMode.ExternalMagazine || Weapon_0.ReloadMode == Weapon.EReloadMode.ExternalMagazineWithInternalReloadSupport)
					{
						if (gInterface2.To1 != null && GClass3380.IsChildOf(gInterface2.To1, Weapon_0))
						{
							GStruct156<GClass2005> sourceOption = GClass2005.Run(Player_0.InventoryController, Weapon_0, Player_0.ProfileId);
							if (sourceOption.Succeeded)
							{
								State = EOperationState.Finished;
								FirearmController_0.InitiateOperation<GClass2039>().Start(sourceOption.Value, callback);
							}
							else
							{
								GClass1617.Invoke(callback, sourceOption);
							}
							return;
						}
						if (gInterface2.From1 != null && GClass3380.IsChildOf(gInterface2.From1, Weapon_0))
						{
							State = EOperationState.Finished;
							FirearmController_0.InitiateOperation<GClass2050>().Start((MagazineItemClass)gInterface2.Item1, (Slot)gInterface2.From1.Container, callback);
							return;
						}
					}
					UnityEngine.Debug.LogErrorFormat("not implemented for operation {0}, just do it", operation);
					callback.Succeed();
				}
				else if (gInterface2.Item1 is Mod)
				{
					if (gInterface2.From1 != null && GClass3380.IsChildOf(gInterface2.From1, Weapon_0))
					{
						State = EOperationState.Finished;
						Slot slot2 = (Slot)gInterface2.From1.Container;
						FirearmController_0.InitiateOperation<GClass2052>().Start(gInterface2.Item1, slot2, callback);
					}
					else if (gInterface2.To1 != null && GClass3380.IsChildOf(gInterface2.To1, Weapon_0))
					{
						State = EOperationState.Finished;
						Slot slot3 = (Slot)gInterface2.To1.Container;
						FirearmController_0.InitiateOperation<Class1264>().Start(gInterface2.Item1, slot3, callback);
					}
					else
					{
						UnityEngine.Debug.LogErrorFormat("Mod operation for operation {0}, just do it", operation);
						callback.Succeed();
					}
				}
				else
				{
					UnityEngine.Debug.LogErrorFormat("not implemented for operation {0}, just do it", operation);
					callback.Succeed();
				}
			}

			public void method_10(GInterface443 oneItemOperation, AmmoItemClass ammo, Callback callback)
			{
				State = EOperationState.Finished;
				FirearmsAnimator_0.SetInventory(open: false);
				if (oneItemOperation.From1 != null)
				{
					Action_1 = delegate
					{
						FirearmController_0.InitiateOperation<GClass2056>().Start(isLauncherEnabled: true, delegate
						{
							FirearmController_0.CurrentOperation.LauncherInventoryUnchamberFromMainWeapon(ammo, 0, oneItemOperation.To1, callback);
						});
					};
				}
				else
				{
					Action_1 = delegate
					{
						FirearmController_0.InitiateOperation<GClass2056>().Start(isLauncherEnabled: true, delegate
						{
							FirearmController_0.CurrentOperation.LoadLauncherFromMainWeapon(ammo, 0, oneItemOperation.To1, callback);
						});
					};
				}
				method_6();
			}

			public void method_11(GInterface443 oneItemOperation, AmmoItemClass ammo, Callback callback)
			{
				CylinderMagazineItemClass cylinderMagazineItemClass = Weapon_0.GetCurrentMagazine() as CylinderMagazineItemClass;
				if (oneItemOperation is GClass3505 gClass)
				{
					if (Weapon_0.ReloadMode != Weapon.EReloadMode.OnlyBarrel)
					{
						State = EOperationState.Finished;
						FirearmController_0.InitiateOperation<RechamberOperationClass>().Start(gClass.AmmoInChamber, callback);
					}
					else
					{
						callback.Fail("This weapon can't perform RechamberOperation");
					}
					return;
				}
				ItemAddress itemAddress = null;
				if (oneItemOperation is GInterface445 { BaseInventoryOperation: MoveOperationClass baseInventoryOperation } && cylinderMagazineItemClass.Camoras.Contains(baseInventoryOperation.To.Container))
				{
					itemAddress = baseInventoryOperation.To;
				}
				int num = 0;
				while (true)
				{
					if (num < cylinderMagazineItemClass.Camoras.Length)
					{
						Slot slot = cylinderMagazineItemClass.Camoras[num];
						Item containedItem = slot.ContainedItem;
						if (oneItemOperation.From1 != null && oneItemOperation.From1.Container == slot && containedItem == oneItemOperation.Item1)
						{
							if ((Weapon_0.ReloadMode != Weapon.EReloadMode.ExternalMagazine && Weapon_0.ReloadMode != Weapon.EReloadMode.ExternalMagazineWithInternalReloadSupport) || Weapon_0.GetCurrentMagazine() == null)
							{
								State = EOperationState.Finished;
								FirearmController_0.InitiateOperation<GClass2023>().Start((AmmoItemClass)oneItemOperation.Item1, num, itemAddress, callback);
								Player_0.ExecuteSkill((Action)delegate
								{
									Player_0.Skills.WeaponChamberAction.Complete(Weapon_0);
								});
								return;
							}
						}
						else if (oneItemOperation.To1 != null && oneItemOperation.To1.Container == slot)
						{
							if (!Weapon_0.CanLoadAmmoToChamber)
							{
								callback.Fail("Can't perform chambers operation in the weapon that can't load ammo to chamber, operation: " + oneItemOperation);
							}
							else
							{
								if ((Weapon_0.ReloadMode != Weapon.EReloadMode.ExternalMagazine && Weapon_0.ReloadMode != Weapon.EReloadMode.ExternalMagazineWithInternalReloadSupport) || Weapon_0.GetCurrentMagazine() == null)
								{
									break;
								}
								callback.Fail($"Can't perform operation {oneItemOperation} while mag in the weapon");
							}
						}
						num++;
						continue;
					}
					callback.Fail($"Can't perform operation {oneItemOperation}");
					return;
				}
				State = EOperationState.Finished;
				FirearmController_0.InitiateOperation<GClass2044>().Start((AmmoItemClass)oneItemOperation.Item1, num, callback);
				Player_0.ExecuteSkill((Action)delegate
				{
					Player_0.Skills.WeaponChamberAction.Complete(Weapon_0);
				});
			}

			public override void SetLeftStanceAnimOnStartOperation()
			{
				if (!Weapon_0.IsStationaryWeapon && !Player_0._isInventoryOpened && !FirearmController_0.DisableLeftStanceByOverlap)
				{
					Player_0.MovementContext.LeftStanceController.SetAnimatorLeftStanceToCacheFromHandsAction();
				}
			}

			public override bool ToggleLauncher(Action callback)
			{
				if (FirearmController_0.UnderbarrelWeapon != null)
				{
					State = EOperationState.Finished;
					FirearmController_0.InitiateOperation<GClass2056>().Start(isLauncherEnabled: true, callback);
					Player_0.ProceduralWeaponAnimation.IsGrenadeLauncher = true;
					return true;
				}
				return false;
			}

			public override void OnOnOffBoltCatchEvent(bool isCatched)
			{
				FirearmsAnimator_0.SetBoltCatch(isCatched);
			}

			public override void DropBackpackOperationInvoke(Item item, Callback callback)
			{
				State = EOperationState.Finished;
				FirearmController_0.InitiateOperation<GClass2026>().Start(item, callback);
			}

			public override void ToggleLeftStance()
			{
				if (!Weapon_0.IsStationaryWeapon && !Weapon_0.BlockLeftStance && !FirearmController_0.CurrentCompassState && !Player_0.MovementContext.IsInPronePose)
				{
					Player_0.MovementContext.LeftStanceController.ToggleLeftStance();
				}
			}

			public override bool ToggleBipod()
			{
				State = EOperationState.Finished;
				FirearmController_0.InitiateOperation<Class1270>().Start();
				return true;
			}

			[CompilerGenerated]
			public void method_12()
			{
				Player_0.Skills.WeaponChamberAction.Complete(Weapon_0);
			}

			[CompilerGenerated]
			public void method_13()
			{
				Player_0.Skills.WeaponChamberAction.Complete(Weapon_0);
			}

			[CompilerGenerated]
			public void method_14()
			{
				Player_0.Skills.WeaponChamberAction.Complete(Weapon_0);
			}

			[CompilerGenerated]
			public void method_15()
			{
				Player_0.Skills.WeaponChamberAction.Complete(Weapon_0);
			}
		}

		public class GClass2039 : GClass2013
		{
			[NonSerialized]
			public Callback Callback_0;

			[NonSerialized]
			public GClass2005 Gclass2005_0;

			[NonSerialized]
			public bool Bool_0;

			[NonSerialized]
			public bool Bool_1;

			[NonSerialized]
			public bool Bool_2;

			public GClass2039(FirearmController controller)
				: base(controller)
			{
			}

			public virtual void Start(GClass2005 insertMagResult, Callback callback)
			{
				Gclass2005_0 = insertMagResult;
				Callback_0 = callback;
				Start();
				FirearmsAnimator_0.SetFire(fire: false);
				FirearmsAnimator_0.SetIsExternalMag(isExternalMag: true);
				FirearmsAnimator_0.SetCanReload(canReload: true);
				Player_0.MovementContext.SetBlindFire(0);
				FirearmsAnimator_0.SetMagTypeCurrent(Gclass2005_0.Magazine.magAnimationIndex);
				FirearmsAnimator_0.SetMagTypeNew(Gclass2005_0.Magazine.magAnimationIndex);
				FirearmsAnimator_0.InsertMagInInventoryMode();
				Player_0.BodyAnimatorCommon.SetFloat(PlayerAnimator.RELOAD_FLOAT_PARAM_HASH, 1f);
				FirearmController_0.bool_1 = true;
				if (Weapon_0.IsBoltCatch && Weapon_0.ChamberAmmoCount == 1 && !Gclass2005_0.HasNewAmmo && !Weapon_0.ManualBoltCatch && !Weapon_0.MustBoltBeOpennedForExternalReload && !Weapon_0.MustBoltBeOpennedForInternalReload)
				{
					FirearmsAnimator_0.SetBoltCatch(active: false);
				}
				if (Weapon_0.MalfState.State == Weapon.EMalfunctionState.Misfire && Weapon_0.MalfState.IsKnownMalfunction(Player_0.ProfileId) && Gclass2005_0.Magazine.Count > 0 && Gclass2005_0.AmmoCompatible)
				{
					FirearmsAnimator_0.SetAmmoInChamber(0f);
					FirearmsAnimator_0.SetLayerWeight(FirearmsAnimator_0.MALFUNCTION_LAYER_INDEX, 0);
				}
			}

			public override void Reset()
			{
				Callback_0 = null;
				Gclass2005_0 = null;
				Bool_0 = false;
				Bool_1 = false;
				Bool_2 = false;
				base.Reset();
			}

			public override void OnMagAppeared()
			{
				if (Bool_1)
				{
					return;
				}
				Bool_1 = true;
				WeaponManagerClass.SetupMod(Gclass2005_0.MagazineSlot.Slot, Singleton<PoolManagerClass>.Instance.CreateItem(Gclass2005_0.Magazine, isAnimated: true));
				if (Gclass2005_0.Magazine.IsMagazineWithBelt)
				{
					FirearmController_0.weaponPrefab_0.UpdateAnimatorHierarchy();
					if (FirearmController_0.HasBipod)
					{
						FirearmController_0.FirearmsAnimator.SetBipod(FirearmController_0.BipodState);
					}
				}
			}

			public override void OnMagInsertedToWeapon()
			{
				if (!Bool_2)
				{
					Bool_2 = true;
					FirearmsAnimator_0.SetAmmoOnMag(Gclass2005_0.MagazineAmmoCount + (Gclass2005_0.HasNewAmmo ? 1 : 0));
					FirearmsAnimator_0.SetMagInWeapon(ok: true);
					FirearmsAnimator_0.SetAmmoCompatible(Gclass2005_0.AmmoCompatible);
					if (!Gclass2005_0.HasNewAmmo && (Weapon_0.MalfState.State != Weapon.EMalfunctionState.Misfire || !Weapon_0.MalfState.IsKnownMalfunction(Player_0.ProfileId) || !Gclass2005_0.AmmoCompatible || Gclass2005_0.Magazine.Count <= 0))
					{
						method_5();
					}
					if (FirearmController_0.HasBipod)
					{
						FirearmController_0.FirearmsAnimator.SetBipod(FirearmController_0.BipodState);
					}
				}
			}

			public override void OnOnOffBoltCatchEvent(bool isCaught)
			{
				FirearmsAnimator_0.SetBoltCatch(isCaught);
			}

			public override void OnAddAmmoInChamber()
			{
				if (!Bool_0)
				{
					Bool_0 = true;
					FirearmsAnimator_0.SetAmmoOnMag(Gclass2005_0.Magazine.Count);
					if (Weapon_0.MalfState.State == Weapon.EMalfunctionState.Misfire)
					{
						method_2();
					}
					if (Gclass2005_0.HasNewAmmo)
					{
						WeaponManagerClass.SetRoundIntoWeapon(Gclass2005_0.NewAmmo);
					}
					FirearmsAnimator_0.SetAmmoInChamber(Gclass2005_0.Weapon.ChamberAmmoCount);
					method_5();
				}
			}

			public override void SetInventoryOpened(bool opened)
			{
				FirearmController_0.InventoryOpened = opened;
				FirearmsAnimator_0.SetInventory(opened);
			}

			public void method_5()
			{
				State = EOperationState.Finished;
				FirearmController_0.RecalculateErgonomic();
				FirearmController_0.InitiateOperation<GClass2037>().Start();
				Callback_0.Succeed();
				FirearmController_0.WeaponModified();
			}

			public override bool CanChangeLightState(FirearmLightStateStruct[] lightsStates)
			{
				return false;
			}

			public override void FastForward()
			{
				if (State != EOperationState.Finished)
				{
					OnMagAppeared();
					OnMagInsertedToWeapon();
					OnAddAmmoInChamber();
					if (State != EOperationState.Finished)
					{
						method_5();
					}
				}
			}

			public override void HideWeapon(Action onHidden, bool fastDrop, Item nextControllerItem = null)
			{
				State = EOperationState.Finished;
				FirearmController_0.RecalculateErgonomic();
				FirearmController_0.IsTriggerPressed = false;
				FirearmController_0.IsAiming = false;
				State = EOperationState.Finished;
				FirearmController_0.InitiateOperation<GClass2053>().Start(onHidden, fastDrop, nextControllerItem);
			}
		}

		public class GClass2005
		{
			public readonly Weapon Weapon;

			public readonly MagazineItemClass Magazine;

			public readonly int MagazineAmmoCount;

			public readonly GClass3391 MagazineSlot;

			public readonly bool AmmoCompatible;

			[NonSerialized]
			[CanBeNull]
			public GClass3410 Gclass3410_0;

			[NonSerialized]
			[CanBeNull]
			public GInterface424 Ginterface424_0;

			public AmmoItemClass NewAmmo => (AmmoItemClass)(Ginterface424_0?.ResultItem);

			public bool HasNewAmmo => Ginterface424_0 != null;

			public GClass2005(Weapon weapon, MagazineItemClass magazine, bool ammoCompatible, [CanBeNull] GClass3410 removeOldAmmoResult, [CanBeNull] GInterface424 addNewAmmoResult)
			{
				Weapon = weapon;
				Magazine = magazine;
				MagazineAmmoCount = magazine.Count;
				MagazineSlot = (GClass3391)magazine.Parent;
				AmmoCompatible = ammoCompatible;
				Gclass3410_0 = removeOldAmmoResult;
				Ginterface424_0 = addNewAmmoResult;
			}

			public void RollBack()
			{
				Ginterface424_0?.RollBack();
				Gclass3410_0?.RollBack();
			}

			public void RaiseEvents(TraderControllerClass controller, CommandStatus status)
			{
				Ginterface424_0?.RaiseEvents(controller, status);
				Gclass3410_0?.RaiseEvents(controller, status);
			}

			public static GStruct156<GClass2005> Run(InventoryController inventoryController, Weapon weapon, string playerId)
			{
				MagazineItemClass currentMagazine = weapon.GetCurrentMagazine();
				Slot[] chambers = weapon.Chambers;
				Slot slot = (weapon.HasChambers ? chambers[0] : null);
				bool flag = currentMagazine.IsAmmoCompatible(chambers);
				GStruct154<GClass3410> gStruct = ((slot == null || !weapon.MustBoltBeOpennedForExternalReload || slot.ContainedItem == null) ? default(GStruct154<GClass3410>) : InteractionsHandlerClass.Remove(slot.ContainedItem, inventoryController));
				if (gStruct.Failed)
				{
					return gStruct.Error;
				}
				bool flag2 = weapon.MalfState.State == Weapon.EMalfunctionState.None || (weapon.MalfState.State == Weapon.EMalfunctionState.Misfire && weapon.MalfState.IsKnownMalfunction(playerId));
				Weapon.EMalfunctionState state = weapon.MalfState.State;
				if (flag2 && weapon.MalfState.State != Weapon.EMalfunctionState.None)
				{
					weapon.MalfState.ChangeStateSilent(Weapon.EMalfunctionState.None);
				}
				GStruct154<GInterface424> gStruct2 = ((slot != null && slot.ContainedItem == null && currentMagazine.Count > 0 && flag && flag2) ? currentMagazine.Cartridges.PopTo(inventoryController, slot.CreateItemAddress()) : default(GStruct154<GInterface424>));
				if (flag2 && state != Weapon.EMalfunctionState.None)
				{
					weapon.MalfState.ChangeStateSilent(state);
				}
				if (gStruct2.Failed)
				{
					gStruct.Value?.RollBack();
					return gStruct2.Error;
				}
				return new GClass2005(weapon, currentMagazine, flag, gStruct.Value, gStruct2.Value);
			}

			public bool CanExecute(TraderControllerClass itemController)
			{
				return true;
			}
		}

		public class GClass2034 : GenericFireOperationClass
		{
			[NonSerialized]
			public bool Bool_5;

			public GClass2034(FirearmController controller)
				: base(controller)
			{
			}

			public new void Start()
			{
				base.Start();
				FirearmsAnimator_0.Animator.Play(FirearmsAnimator_0.FullFireStateName, 1, 0.2f);
			}

			public override void PrepareShot()
			{
				FirearmsAnimator_0.SetFire(FirearmController_0.IsTriggerPressed);
			}

			public override void Reset()
			{
				Bool_5 = false;
				base.Reset();
			}

			public void method_14()
			{
				FirearmController_0.IsTriggerPressed = false;
				FirearmsAnimator_0.SetFire(fire: false);
				State = EOperationState.Finished;
				FirearmController_0.InitiateOperation<GClass2040>().Start();
			}

			public override void OnFireEndEvent()
			{
				method_14();
			}

			public override void SetInventoryOpened(bool opened)
			{
				Bool_5 = opened;
			}

			public override void OnFireEvent()
			{
				if (!Bool_1)
				{
					FirearmsAnimator firearmsAnimator = FirearmController_0.weaponPrefab_0.FirearmsAnimator;
					firearmsAnimator.ResetGestureTrigger();
					firearmsAnimator.ResetHandReadyTrigger();
					IAnimator animator = firearmsAnimator.Animator;
					animator.SetLayerWeight(animator.GetLayerIndex("LActions"), 0f);
					animator.Play("Idle", animator.GetLayerIndex("LActions"), 0f);
					LauncherItemClass underbarrelWeapon = FirearmController_0.UnderbarrelWeapon;
					AmmoItemClass ammoItemClass = underbarrelWeapon.Chamber.ContainedItem as AmmoItemClass;
					FirearmController_0.method_57(underbarrelWeapon, ammoItemClass);
					underbarrelWeapon.Chamber.RemoveItem();
					ammoItemClass.IsUsed = true;
					Bool_1 = true;
					if (ammoItemClass.AmmoTemplate.RemoveShellAfterFire)
					{
						FirearmController_0.underbarrelManagerClass.DestroyPatronInWeapon();
					}
					else
					{
						FirearmController_0.underbarrelManagerClass.MoveAmmoFromChamberToShellPort(ammoItemClass.IsUsed);
					}
					if (!ammoItemClass.AmmoTemplate.RemoveShellAfterFire)
					{
						underbarrelWeapon.ShellsInChambers[0] = ammoItemClass.AmmoTemplate;
					}
					FirearmsAnimator_0.SetShellsInWeapon((!ammoItemClass.AmmoTemplate.RemoveShellAfterFire) ? 1 : 0);
					FirearmsAnimator_0.SetAmmoInChamber(0f);
				}
			}
		}

		public class GClass2040 : GClass2013
		{
			[CompilerGenerated]
			public class Class1230
			{
				public GClass2040 gclass2040_0;

				public Item item;

				public Callback callback;

				public void method_0()
				{
					gclass2040_0.FirearmController_0.CurrentOperation.DropBackpackOperationInvoke(item, callback);
				}
			}

			[CompilerGenerated]
			public class Class1231
			{
				public GClass2040 gclass2040_0;

				public Action onHidden;

				public bool fastDrop;

				public Item nextControllerItem;

				public void method_0()
				{
					gclass2040_0.FirearmController_0.CurrentOperation.HideWeapon(onHidden, fastDrop, nextControllerItem);
				}
			}

			[NonSerialized]
			public LauncherItemClass LauncherItemClass;

			[NonSerialized]
			public Action Action_0;

			[NonSerialized]
			public bool Bool_0;

			public GClass2040(FirearmController controller)
				: base(controller)
			{
			}

			public void Start(Action callback = null)
			{
				base.Start();
				Action_0 = callback;
				LauncherItemClass = FirearmController_0.UnderbarrelWeapon;
				WeaponAnimationSpeedControllerClass.SetUseLeftHand(FirearmsAnimator_0.Animator, useLeftHand: false);
				FirearmsAnimator_0.SetAmmoInChamber(LauncherItemClass.ChamberAmmoCount);
				FirearmsAnimator_0.SetInventory(FirearmController_0.bool_2);
				FirearmsAnimator_0.ResetCheckChamberTrigger();
				FirearmsAnimator_0.SetFire(fire: false);
				Action_0?.Invoke();
			}

			public override void SetLeftStanceAnimOnStartOperation()
			{
				if (!Weapon_0.IsStationaryWeapon)
				{
					Player_0.MovementContext.LeftStanceController.SetAnimatorLeftStanceToCacheFromHandsAction();
				}
			}

			public override bool ToggleLauncher(Action callback)
			{
				method_5(callback);
				return true;
			}

			public void method_5(Action callback)
			{
				State = EOperationState.Finished;
				FirearmController_0.InitiateOperation<GClass2056>().Start(isLauncherEnabled: false, callback);
			}

			public void method_6()
			{
				Player_0.UpdateLauncherBones(launcherEnable: false, FirearmController_0.underbarrelManagerClass.LauncherWeaponPrefab);
				Bool_0 = true;
			}

			public override void ForceSetUnderbarrelRangeIndex(int rangeIndex)
			{
				LauncherItemClass.ForceSetSightRangeIndex(rangeIndex);
			}

			public override void UnderbarrelSightingRangeDown()
			{
				LauncherItemClass.SightingRangeDown();
				method_7();
			}

			public override void UnderbarrelSightingRangeUp()
			{
				LauncherItemClass.SightingRangeUp();
				method_7();
			}

			public void method_7()
			{
				Player_0.ShowAmmoCountZeroingPanel(LauncherItemClass.RangeValue.ToString() ?? "");
			}

			public override void SprintStateChanged(bool value)
			{
				FirearmsAnimator_0.SetSprint(value);
			}

			public override void Update(float deltaTime)
			{
				if (Bool_0 && !FirearmsAnimator_0.IsIdling())
				{
					Player_0.UpdateLauncherBones(launcherEnable: true, FirearmController_0.underbarrelManagerClass.LauncherWeaponPrefab);
					Bool_0 = false;
				}
				SetSightingRange(deltaTime);
			}

			public virtual void SetSightingRange(float deltaTime)
			{
				int lastRangeValue = LauncherItemClass.LastRangeValue;
				if (lastRangeValue == LauncherItemClass.RangeValue)
				{
					return;
				}
				int num = 0;
				if (lastRangeValue > LauncherItemClass.RangeValue)
				{
					num = (int)Mathf.Clamp((float)lastRangeValue - 400f * deltaTime, LauncherItemClass.RangeValue, lastRangeValue);
				}
				else
				{
					if (lastRangeValue >= LauncherItemClass.RangeValue)
					{
						return;
					}
					num = (int)Mathf.Clamp((float)lastRangeValue + 400f * deltaTime, lastRangeValue, LauncherItemClass.RangeValue);
				}
				LauncherItemClass.LastRangeValue = num;
				FirearmsAnimator_0.SetUnderbarrelSightingRange(num);
			}

			public override void SetTriggerPressed(bool pressed)
			{
				FirearmController_0.IsTriggerPressed = pressed;
				if (pressed && !Player_0.StateIsSuitableForHandInput)
				{
					return;
				}
				if (pressed && LauncherItemClass.Chamber.ContainedItem != null)
				{
					State = EOperationState.Finished;
					FirearmController_0.InitiateOperation<GClass2034>().Start();
					return;
				}
				FirearmController_0.firearmsAnimator_0.SetFire(pressed);
				if (pressed)
				{
					FirearmController_0.DryShot(0, underbarrelShot: true);
				}
			}

			public override bool ExamineWeapon()
			{
				if (FirearmController_0.IsAiming)
				{
					return false;
				}
				RunUtilityOperation(GClass2038.EUtilityType.ExamineWeapon);
				FirearmsAnimator_0.LookTrigger();
				return true;
			}

			public override void OnIdleStartEvent()
			{
				SetLeftStanceAnimOnStartOperation();
				Player_0.BodyAnimatorCommon.SetFloat(PlayerAnimator.RELOAD_FLOAT_PARAM_HASH, 0f);
			}

			public override void ReloadGrenadeLauncher(AmmoPackReloadingClass ammoPack, Callback callback)
			{
				if (LauncherItemClass.Chamber.ContainedItem == null)
				{
					SetAiming(isAiming: false);
					State = EOperationState.Finished;
					FirearmController_0.InitiateOperation<GClass2042>().Start(ammoPack, callback);
				}
			}

			public override void SetInventoryOpened(bool opened)
			{
				SetAiming(isAiming: false);
				SetTriggerPressed(pressed: false);
				FirearmController_0.InventoryOpened = opened;
				FirearmsAnimator_0.SetInventory(opened);
			}

			public override bool CanStartReload()
			{
				return true;
			}

			public override void SetAiming(bool isAiming)
			{
				if (!isAiming || EFTHardSettings.Instance.CanAimInState(Player_0.CurrentState.Name))
				{
					if (FirearmController_0.float_2 > EFTHardSettings.Instance.STOP_AIMING_AT && isAiming)
					{
						FirearmController_0.AimingInterruptedByOverlap = false;
					}
					else
					{
						FirearmController_0.IsAiming = isAiming;
					}
				}
			}

			public override void Interact(bool isInteracting, int actionIndex)
			{
				if (FirearmsAnimator_0.IsIdling())
				{
					Player_0.MovementContext.LeftStanceController.DisableLeftStanceAnimFromHandsAction();
					Player_0.SendHandsInteractionStateChanged(isInteracting, actionIndex);
					FirearmController_0.weaponPrefab_0.FirearmsAnimator.SetInteract(isInteracting, actionIndex);
					method_6();
				}
			}

			public override void Pickup(bool pickup)
			{
				if (FirearmsAnimator_0.IsIdling())
				{
					Player_0.MovementContext.LeftStanceController.DisableLeftStanceAnimFromHandsAction();
					FirearmController_0.weaponPrefab_0.FirearmsAnimator.SetPickup(pickup);
					method_6();
				}
			}

			public override void ShowGesture(EInteraction gesture)
			{
				Player_0.MovementContext.LeftStanceController.DisableLeftStanceAnimFromHandsAction();
				FirearmController_0.weaponPrefab_0.FirearmsAnimator.Gesture(gesture);
				method_6();
			}

			public override void LoadLauncherFromMainWeapon(AmmoItemClass ammo, int camoraIndex, ItemAddress itemAddress, Callback callback)
			{
				SetAiming(isAiming: false);
				SetTriggerPressed(pressed: false);
				FirearmController_0.IsTriggerPressed = false;
				State = EOperationState.Finished;
				FirearmController_0.InitiateOperation<GClass2043>().Start(ammo, 0, callback);
			}

			public override void LauncherInventoryUnchamberFromMainWeapon(AmmoItemClass ammo, int camoraIndex, ItemAddress itemAddress, Callback callback)
			{
				SetAiming(isAiming: false);
				SetTriggerPressed(pressed: false);
				State = EOperationState.Finished;
				FirearmController_0.InitiateOperation<GClass2014>().Start(ammo, 0, itemAddress, callback);
			}

			public void method_8(Action onHidden, bool fastDrop, Item nextControllerItem = null)
			{
				SetAiming(isAiming: false);
				SetTriggerPressed(pressed: false);
				State = EOperationState.Finished;
				FirearmController_0.InitiateOperation<GClass2056>().Start(isLauncherEnabled: false);
				FirearmController_0.FirearmsAnimator.Animator.Play("ILDE GRIP", FirearmController_0.FirearmsAnimator.Animator.GetLayerIndex("LActions"), 0f);
				FirearmController_0.CurrentOperation.LauncherDisappeared();
				FirearmController_0.CurrentOperation.HideWeapon(onHidden, fastDrop, nextControllerItem);
			}

			public override void DropBackpackOperationInvoke(Item item, Callback callback)
			{
				State = EOperationState.Finished;
				FirearmController_0.InitiateOperation<GClass2056>().Start(isLauncherEnabled: false, delegate
				{
					FirearmController_0.CurrentOperation.DropBackpackOperationInvoke(item, callback);
				});
			}

			public override void HideWeapon(Action onHidden, bool fastDrop, Item nextControllerItem = null)
			{
				SetAiming(isAiming: false);
				SetTriggerPressed(pressed: false);
				State = EOperationState.Finished;
				FirearmController_0.InitiateOperation<GClass2056>().Start(isLauncherEnabled: false, delegate
				{
					FirearmController_0.CurrentOperation.HideWeapon(onHidden, fastDrop, nextControllerItem);
				});
			}

			public void method_9(GInterface443 oneItemOperation, AmmoItemClass ammo, Callback callback)
			{
				ItemAddress itemAddress = null;
				Slot chamber = LauncherItemClass.Chamber;
				AmmoItemClass ammoItemClass = chamber.ContainedItem as AmmoItemClass;
				if (oneItemOperation.From1 != null && oneItemOperation.From1.Container == chamber && ammoItemClass == oneItemOperation.Item1)
				{
					State = EOperationState.Finished;
					FirearmController_0.InitiateOperation<GClass2014>().Start((AmmoItemClass)oneItemOperation.Item1, 0, itemAddress, callback);
					Player_0.ExecuteSkill((Action)delegate
					{
						Player_0.Skills.WeaponChamberAction.Complete(Weapon_0);
					});
				}
				else if (oneItemOperation.To1 != null && oneItemOperation.To1.Container == chamber)
				{
					State = EOperationState.Finished;
					FirearmController_0.InitiateOperation<GClass2043>().Start((AmmoItemClass)oneItemOperation.Item1, 0, callback);
					Player_0.ExecuteSkill((Action)delegate
					{
						Player_0.Skills.WeaponChamberAction.Complete(Weapon_0);
					});
				}
				else
				{
					callback?.Fail("Failed operation in LauncherIdling");
				}
			}

			public override bool CheckChamber()
			{
				if (FirearmController_0.IsTriggerPressed)
				{
					return false;
				}
				if (FirearmController_0._player.MovementContext.StationaryWeapon != null)
				{
					return false;
				}
				if (LauncherItemClass.UseAmmoWithoutShell && LauncherItemClass.ChamberAmmoCount == 0)
				{
					return false;
				}
				if (FirearmController_0._player.InventoryController.CheckItemAction(FirearmController_0.Item, FirearmController_0.Item.CurrentAddress).Failed)
				{
					return false;
				}
				SetAiming(isAiming: false);
				FirearmsAnimator_0.CheckChamber();
				Player_0.InventoryController.CheckChamber(Weapon_0, status: true);
				RunUtilityOperation(GClass2038.EUtilityType.CheckChamber);
				return true;
			}

			public virtual void RunUtilityOperation(GClass2038.EUtilityType utilityType)
			{
				Player_0.BodyAnimatorCommon.SetFloat(PlayerAnimator.RELOAD_FLOAT_PARAM_HASH, 1f);
				State = EOperationState.Finished;
				FirearmController_0.InitiateOperation<GClass2041>().Start(utilityType, LauncherItemClass);
			}

			public override void Execute(GInterface438 operation, Callback callback)
			{
				if (!(operation is GInterface443 gInterface))
				{
					callback.Succeed();
					return;
				}
				if (!FirearmController_0.method_21(operation))
				{
					if (Player_0.InventoryController.IsAnimatedSlot(gInterface.From1))
					{
						DropBackpackOperationInvoke(gInterface.Item1, callback);
					}
					else
					{
						callback.Succeed();
					}
					return;
				}
				FirearmController_0.IsTriggerPressed = false;
				if (operation is FoldOperationClass foldOperation)
				{
					State = EOperationState.Finished;
					FirearmController_0.InitiateOperation<Class1269>().Start(foldOperation, callback);
				}
				else if (gInterface.Item1 is AmmoItemClass ammo && FirearmController_0.IsInLauncherMode())
				{
					method_9(gInterface, ammo, callback);
				}
				else
				{
					callback?.Fail("Failed operation in LauncherIdling");
				}
			}

			public override void ToggleLeftStance()
			{
				if (!Weapon_0.IsStationaryWeapon && !Weapon_0.BlockLeftStance && !FirearmController_0.CurrentCompassState)
				{
					Player_0.MovementContext.LeftStanceController.ToggleLeftStance();
				}
			}

			[CompilerGenerated]
			public void method_10()
			{
				Player_0.Skills.WeaponChamberAction.Complete(Weapon_0);
			}

			[CompilerGenerated]
			public void method_11()
			{
				Player_0.Skills.WeaponChamberAction.Complete(Weapon_0);
			}
		}

		public class GClass2042 : GClass2013
		{
			[NonSerialized]
			public Callback Callback_0;

			[NonSerialized]
			public AmmoPackReloadingClass AmmoPackReloadingClass;

			[NonSerialized]
			public bool Bool_0;

			[NonSerialized]
			public AmmoItemClass AmmoItemClass;

			[NonSerialized]
			public bool Bool_1;

			[NonSerialized]
			public Action Action_0;

			public GClass2042(FirearmController controller)
				: base(controller)
			{
			}

			public virtual void Start(AmmoPackReloadingClass ammoPack, Callback callback)
			{
				Start();
				AmmoPackReloadingClass = ammoPack;
				AmmoPackReloadingClass.LockItems();
				Callback_0 = callback;
				FirearmsAnimator_0.SetFire(fire: false);
				Player_0.MovementContext.SetBlindFire(0);
				FirearmsAnimator_0.Reload(b: true);
				method_5();
			}

			public override void Reset()
			{
				AmmoPackReloadingClass = null;
				Callback_0 = null;
				AmmoItemClass = null;
				Bool_0 = false;
				Action_0 = null;
				Bool_1 = false;
				base.Reset();
			}

			public override void SetInventoryOpened(bool opened)
			{
				Bool_0 = opened;
			}

			public override void OnMagAppeared()
			{
				FirearmController_0.underbarrelManagerClass.SetRoundIntoWeapon(AmmoItemClass);
			}

			public override void OnAddAmmoInChamber()
			{
				State = EOperationState.Finished;
				Action action_ = Action_0;
				bool bool_ = Bool_1;
				GClass2040 gClass = FirearmController_0.InitiateOperation<GClass2040>();
				gClass.Start();
				Callback_0?.Succeed();
				if (action_ != null)
				{
					gClass.HideWeapon(action_, bool_);
				}
			}

			public override void HideWeapon(Action onHidden, bool fastDrop, Item nextControllerItem = null)
			{
				Action_0 = onHidden;
				Bool_1 = fastDrop;
			}

			public void method_5()
			{
				AmmoPackReloadingClass.UnlockItems();
				AmmoItemClass = AmmoPackReloadingClass.GetAmmoToReload(1);
				Weapon_0.RaiseRefreshEvent();
				LauncherItemClass underbarrelWeapon = FirearmController_0.UnderbarrelWeapon;
				GStruct154<GInterface424> gStruct = AmmoPackReloadingClass.LoadAmmo(Player_0.InventoryController, Player_0.InventoryController, underbarrelWeapon.Chamber.CreateItemAddress());
				if (gStruct.Error != null)
				{
					UnityEngine.Debug.LogError("ReloadInternalMagOperation::Prepare --- Could not get ammo to load, error: " + gStruct.Error);
					return;
				}
				gStruct.Value.RaiseEvents(Player_0.InventoryController, CommandStatus.Begin);
				gStruct.Value.RaiseEvents(Player_0.InventoryController, CommandStatus.Succeed);
				AmmoItemClass = (AmmoItemClass)gStruct.Value.Item;
				if (AmmoItemClass == null)
				{
					UnityEngine.Debug.LogError("No ammo in ammo pack");
				}
			}

			public override void OnShellEjectEvent()
			{
				FirearmController_0.underbarrelManagerClass.StartSpawnShell(FirearmController_0._player.Velocity * 0.33f);
				for (int i = 0; i < FirearmController_0.UnderbarrelWeapon.ShellsInChambers.Length; i++)
				{
					FirearmController_0.UnderbarrelWeapon.ShellsInChambers[i] = null;
				}
				FirearmsAnimator_0.SetShellsInWeapon(FirearmController_0.UnderbarrelWeapon.ShellsInLauncherCount);
			}
		}

		public class GClass2041 : GClass2040
		{
			[NonSerialized]
			public const float Float_0 = 2.5f;

			[NonSerialized]
			public float Float_1;

			[NonSerialized]
			public bool Bool_1;

			[NonSerialized]
			public GClass2038.EUtilityType EutilityType_0;

			[NonSerialized]
			public new LauncherItemClass LauncherItemClass;

			public GClass2041(FirearmController firearmController)
				: base(firearmController)
			{
			}

			public void Start(GClass2038.EUtilityType utilityType, LauncherItemClass launcher)
			{
				EutilityType_0 = utilityType;
				LauncherItemClass = launcher;
				FirearmsAnimator_0.SetShellsInWeapon(LauncherItemClass.ShellsInLauncherCount);
				State = EOperationState.Executing;
				Float_1 = 0f;
			}

			public override void OnIdleStartEvent()
			{
				if (State == EOperationState.Ready)
				{
					base.OnIdleStartEvent();
					State = EOperationState.Finished;
					FirearmController_0.InitiateOperation<GClass2040>().Start();
				}
			}

			public override void OnUtilityOperationStartEvent()
			{
				State = EOperationState.Ready;
			}

			public override bool CanStartReload()
			{
				return false;
			}

			public override void Reset()
			{
				EutilityType_0 = GClass2038.EUtilityType.None;
				base.Reset();
			}

			public override bool CheckAmmo()
			{
				return false;
			}

			public override bool CheckChamber()
			{
				return false;
			}

			public override bool CheckFireMode()
			{
				return false;
			}

			public override void ReloadMag(MagazineItemClass magazine, ItemAddress itemAddress, Callback finishCallback, Callback startCallback)
			{
			}

			public override void ReloadWithAmmo(AmmoPackReloadingClass ammoPack, Callback finishCallback, Callback startCallback)
			{
			}

			public override void ReloadCylinderMagazine(AmmoPackReloadingClass ammoPack, Callback finishCallback, Callback startCallback, bool quickReload = false)
			{
			}

			public override void QuickReloadMag(MagazineItemClass magazine, Callback finishCallback, Callback startCallback)
			{
			}

			public override void ReloadGrenadeLauncher(AmmoPackReloadingClass ammoPack, Callback callback)
			{
			}

			public override void SetTriggerPressed(bool pressed)
			{
				if (EutilityType_0 == GClass2038.EUtilityType.ExamineWeapon)
				{
					OnUtilityOperationStartEvent();
					OnIdleStartEvent();
					FirearmController_0.CurrentOperation.SetTriggerPressed(pressed);
				}
			}

			public override void SetInventoryOpened(bool opened)
			{
				Bool_1 = opened;
				if (!Bool_1)
				{
					Float_1 = 0f;
				}
				base.SetInventoryOpened(opened);
			}

			public override void Update(float deltaTime)
			{
				base.Update(deltaTime);
				if (State != EOperationState.Executing || Bool_1)
				{
					return;
				}
				if (Float_1 > 2.5f)
				{
					if (FirearmsAnimator_0 != null)
					{
						UnityEngine.Debug.LogError("UtilityOperationEvent not found on " + FirearmsAnimator_0.Animator.name);
					}
					else
					{
						UnityEngine.Debug.LogError("UtilityOperationEvent not found. No animator!");
					}
					State = EOperationState.Ready;
					OnIdleStartEvent();
				}
				else
				{
					Float_1 += deltaTime;
				}
			}

			public override void SetSightingRange(float deltaTime)
			{
			}

			public override void SetAiming(bool isAiming)
			{
			}

			public override bool ExamineWeapon()
			{
				return true;
			}

			public override void OnShellEjectEvent()
			{
				FirearmController_0.underbarrelManagerClass.StartSpawnShell(FirearmController_0._player.Velocity * 0.33f);
				for (int i = 0; i < LauncherItemClass.ShellsInChambers.Length; i++)
				{
					LauncherItemClass.ShellsInChambers[i] = null;
				}
				FirearmsAnimator_0.SetShellsInWeapon(LauncherItemClass.ShellsInLauncherCount);
			}
		}

		public class GClass2043(FirearmController controller) : GClass2013(controller)
		{
			[NonSerialized]
			public AmmoItemClass AmmoItemClass;

			[NonSerialized]
			public int Int_0 = -1;

			[NonSerialized]
			public Callback Callback_0;

			public virtual void Start(AmmoItemClass ammo, int chamberIndex, Callback callback)
			{
				AmmoItemClass = ammo;
				Int_0 = chamberIndex;
				Callback_0 = callback;
				Start();
				FirearmController_0.IsAiming = false;
				FirearmsAnimator_0.SetFire(fire: false);
				FirearmsAnimator_0.LoadOneTrigger(loadOne: true);
				Player_0.MovementContext.SetBlindFire(0);
				FirearmsAnimator_0.SetChamberIndexForLoadUnloadAmmo(chamberIndex);
				Player_0.BodyAnimatorCommon.SetFloat(PlayerAnimator.RELOAD_FLOAT_PARAM_HASH, 1f);
			}

			public override void Reset()
			{
				AmmoItemClass = null;
				Int_0 = -1;
				Callback_0 = null;
				base.Reset();
			}

			public override void OnMagAppeared()
			{
				FirearmController_0.underbarrelManagerClass.SetRoundIntoWeapon(AmmoItemClass);
			}

			public override void OnRemoveShellEvent()
			{
				FirearmController_0.underbarrelManagerClass.RemoveShellInWeapon();
				FirearmsAnimator_0.SetShellsInWeapon(0);
			}

			public override void OnAddAmmoInChamber()
			{
				FirearmsAnimator_0.SetAmmoInChamber(1f);
				FirearmsAnimator_0.SetShellsInWeapon(0);
				FirearmsAnimator_0.SetCanReload(canReload: false);
				FirearmsAnimator_0.LoadOneTrigger(loadOne: false);
				State = EOperationState.Finished;
				FirearmController_0.InitiateOperation<GClass2040>().Start();
				Callback_0?.Succeed();
			}

			public override void OnOnOffBoltCatchEvent(bool isCaught)
			{
				FirearmsAnimator_0.SetBoltCatch(isCaught);
			}

			public override void SetInventoryOpened(bool opened)
			{
				FirearmController_0.InventoryOpened = opened;
				FirearmsAnimator_0.SetInventory(opened);
			}
		}

		public class GClass2044(FirearmController controller) : GClass2013(controller)
		{
			[NonSerialized]
			public AmmoItemClass AmmoItemClass;

			[NonSerialized]
			public int Int_0 = -1;

			[NonSerialized]
			public CylinderMagazineItemClass CylinderMagazineItemClass;

			[NonSerialized]
			public Callback Callback_0;

			public virtual void Start(AmmoItemClass ammo, int camoraIndex, Callback callback)
			{
				AmmoItemClass = ammo;
				CylinderMagazineItemClass = Weapon_0.GetCurrentMagazine() as CylinderMagazineItemClass;
				Callback_0 = callback;
				Int_0 = camoraIndex;
				Start();
				FirearmController_0.IsAiming = false;
				FirearmsAnimator_0.SetFire(fire: false);
				Player_0.MovementContext.SetBlindFire(0);
				FirearmsAnimator_0.LoadOneTrigger(loadOne: true);
				if (Weapon_0.ShellsInChambers[Int_0] != null)
				{
					FirearmsAnimator_0.SetCamoraIndexWithShellForRemove(Int_0);
				}
				else
				{
					FirearmsAnimator_0.SetCamoraIndexWithShellForRemove(-1);
				}
				FirearmsAnimator_0.SetShellsInWeapon(Weapon_0.GetShellsInWeaponCount());
				FirearmsAnimator_0.SetCamoraIndexForLoadAmmo(Int_0);
				FirearmsAnimator_0.SetAmmoOnMag(CylinderMagazineItemClass.Count - 1);
				FirearmsAnimator_0.SetCanReload(canReload: true);
				Player_0.BodyAnimatorCommon.SetFloat(PlayerAnimator.RELOAD_FLOAT_PARAM_HASH, 1f);
			}

			public override void Reset()
			{
				AmmoItemClass = null;
				CylinderMagazineItemClass = null;
				Callback_0 = null;
				Int_0 = -1;
				base.Reset();
			}

			public override void FastForward()
			{
				if (State != EOperationState.Finished)
				{
					OnRemoveShellEvent();
					OnAddAmmoInChamber();
					AddAmmoToMag();
				}
			}

			public override void OnMagPuttedToRig()
			{
				SwitchToIdle();
			}

			public override void OnRemoveShellEvent()
			{
				Weapon_0.ShellsInChambers[Int_0] = null;
				WeaponManagerClass.RemoveShellInWeapon(Int_0);
				FirearmsAnimator_0.SetShellsInWeapon(Weapon_0.GetShellsInWeaponCount());
			}

			public override void OnAddAmmoInChamber()
			{
				FirearmsAnimator_0.SetAmmoOnMag(CylinderMagazineItemClass.Count);
				FirearmsAnimator_0.SetShellsInWeapon(Weapon_0.GetShellsInWeaponCount());
				FirearmsAnimator_0.SetCanReload(canReload: true);
				FirearmsAnimator_0.LoadOneTrigger(loadOne: false);
				WeaponManagerClass.SetRoundIntoWeapon(AmmoItemClass, Int_0);
			}

			public override void AddAmmoToMag()
			{
				FirearmsAnimator_0.SetCanReload(canReload: false);
			}

			public override void OnOnOffBoltCatchEvent(bool isCaught)
			{
				FirearmsAnimator_0.SetBoltCatch(isCaught);
			}

			public override void SetInventoryOpened(bool opened)
			{
				FirearmController_0.InventoryOpened = opened;
				FirearmsAnimator_0.SetInventory(opened);
			}

			public virtual void SwitchToIdle()
			{
				CylinderMagazineItemClass.ResetCamoraIndex();
				FirearmsAnimator_0.SetCamoraIndex(CylinderMagazineItemClass.CurrentCamoraIndex);
				State = EOperationState.Finished;
				FirearmController_0.InitiateOperation<GClass2037>().Start();
				Callback_0.Succeed();
			}

			public override void HideWeapon(Action onHidden, bool fastDrop, Item nextControllerItem = null)
			{
				SwitchToIdle();
				FirearmController_0.InitiateOperation<GClass2053>().Start(onHidden, fastDrop, nextControllerItem);
			}
		}

		public class GClass2045(FirearmController controller) : GClass2013(controller)
		{
			[NonSerialized]
			public AmmoItemClass AmmoItemClass;

			[NonSerialized]
			public int Int_0 = -1;

			[NonSerialized]
			public Callback Callback_0;

			public virtual void Start(AmmoItemClass ammo, int chamberIndex, Callback callback)
			{
				AmmoItemClass = ammo;
				Int_0 = chamberIndex;
				Callback_0 = callback;
				Start();
				FirearmController_0.IsAiming = false;
				FirearmsAnimator_0.SetFire(fire: false);
				FirearmsAnimator_0.LoadOneTrigger(loadOne: true);
				Player_0.MovementContext.SetBlindFire(0);
				FirearmsAnimator_0.SetChamberIndexForLoadUnloadAmmo(chamberIndex);
				Player_0.BodyAnimatorCommon.SetFloat(PlayerAnimator.RELOAD_FLOAT_PARAM_HASH, 1f);
				method_3();
			}

			public override void Reset()
			{
				AmmoItemClass = null;
				Int_0 = -1;
				Callback_0 = null;
				base.Reset();
			}

			public override void OnRemoveShellEvent()
			{
				for (int i = 0; i < Weapon_0.ShellsInChambers.Length; i++)
				{
					Weapon_0.ShellsInChambers[i] = null;
				}
				WeaponManagerClass.RemoveAllShells();
				FirearmsAnimator_0.SetShellsInWeapon(Weapon_0.ShellsInWeaponCount);
			}

			public override void OnMagAppeared()
			{
				if (!AmmoItemClass.IsUsed && !WeaponManagerClass.HasPatronInWeapon(Int_0))
				{
					WeaponManagerClass.SetRoundIntoWeapon(AmmoItemClass, Int_0);
				}
				if (AmmoItemClass.IsUsed && !WeaponManagerClass.HasShellInWeapon(Int_0))
				{
					WeaponManagerClass.CreatePatronInShellPort(AmmoItemClass);
				}
			}

			public override void OnAddAmmoInChamber()
			{
				FirearmsAnimator_0.SetAmmoInChamber(Weapon_0.ChamberAmmoCount);
				FirearmsAnimator_0.SetShellsInWeapon(Weapon_0.ShellsInWeaponCount);
				Player_0.ExecuteSkill((Action)delegate
				{
					Player_0.Skills.RaidLoadedAmmoAction.Complete();
				});
				FirearmsAnimator_0.SetCanReload(canReload: false);
				FirearmsAnimator_0.LoadOneTrigger(loadOne: false);
				if (!AmmoItemClass.IsUsed && !WeaponManagerClass.HasPatronInWeapon(Int_0))
				{
					WeaponManagerClass.SetRoundIntoWeapon(AmmoItemClass, Int_0);
				}
				if (AmmoItemClass.IsUsed && !WeaponManagerClass.HasShellInWeapon(Int_0))
				{
					WeaponManagerClass.CreatePatronInShellPort(AmmoItemClass, Int_0);
				}
				SwitchToIdle();
			}

			public override void OnOnOffBoltCatchEvent(bool isCaught)
			{
				FirearmsAnimator_0.SetBoltCatch(isCaught);
			}

			public override void SetInventoryOpened(bool opened)
			{
				FirearmController_0.InventoryOpened = opened;
				FirearmsAnimator_0.SetInventory(opened);
			}

			public virtual void SwitchToIdle()
			{
				State = EOperationState.Finished;
				FirearmController_0.InitiateOperation<GClass2037>().Start();
				Callback_0.Succeed();
			}

			public override void HideWeapon(Action onHidden, bool fastDrop, Item nextControllerItem = null)
			{
				FirearmController_0.IsTriggerPressed = false;
				FirearmController_0.IsAiming = false;
				State = EOperationState.Finished;
				FirearmController_0.InitiateOperation<GClass2053>().Start(onHidden, fastDrop, nextControllerItem);
			}

			[CompilerGenerated]
			public void method_5()
			{
				Player_0.Skills.RaidLoadedAmmoAction.Complete();
			}
		}

		public abstract class GClass2046 : GClass2013
		{
			[CompilerGenerated]
			public class Class1232
			{
				public GClass2046 gclass2046_0;

				public Action onHidden;

				public bool fastDrop;

				public Item nextControllerItem;

				public void method_0()
				{
					gclass2046_0.State = EOperationState.Finished;
					gclass2046_0.FirearmController_0.InitiateOperation<GClass2053>().Start(onHidden, fastDrop, nextControllerItem);
				}
			}

			[NonSerialized]
			public AmmoItemClass AmmoItemClass;

			[NonSerialized]
			public AmmoItemClass AmmoItemClass_1;

			[NonSerialized]
			public Action Action_0;

			public GClass2046(FirearmController controller)
				: base(controller)
			{
			}

			public new virtual void Start()
			{
				base.Start();
				AmmoItemClass = Weapon_0.MalfState.AmmoToFire;
				AmmoItemClass_1 = Weapon_0.MalfState.AmmoWillBeLoadedToChamber;
				Player_0.MovementContext.SetBlindFire(0);
			}

			public override void Reset()
			{
				AmmoItemClass = null;
				AmmoItemClass_1 = null;
				Action_0 = null;
				base.Reset();
			}

			public override void RemoveAmmoFromChamber()
			{
				FirearmsAnimator_0.SetAmmoInChamber(Weapon_0.ChamberAmmoCount - ((AmmoItemClass_1 != null) ? 1 : 0));
				WeaponManagerClass.SetupPatronInWeaponForJam();
			}

			public void method_5()
			{
				FirearmController_0.IsTriggerPressed = false;
				FirearmsAnimator_0.SetFire(FirearmController_0.IsTriggerPressed);
				State = EOperationState.Finished;
				if (Action_0 != null)
				{
					Action_0();
				}
				else
				{
					FirearmController_0.InitiateOperation<GClass2037>().Start();
				}
			}

			public override void HideWeapon(Action onHidden, bool fastDrop, Item nextControllerItem = null)
			{
				FirearmController_0.IsTriggerPressed = false;
				FirearmController_0.IsAiming = false;
				Action_0 = delegate
				{
					State = EOperationState.Finished;
					FirearmController_0.InitiateOperation<GClass2053>().Start(onHidden, fastDrop, nextControllerItem);
				};
			}

			public override void SetLeftStanceAnimOnStartOperation()
			{
			}
		}

		public class IsOneOffFireOperationClass : GenericFireOperationClass
		{
			[NonSerialized]
			public AmmoItemClass AmmoItemClass;

			public IsOneOffFireOperationClass(FirearmController controller)
				: base(controller)
			{
			}

			public override void Start()
			{
				base.Start();
				FirearmsAnimator_0.SetFire(FirearmController_0.IsTriggerPressed);
				FirearmController_0.SendStartOneShotFire();
				Player_0.InventoryController.RaiseEvent(new GEventArgs4(Weapon_0, CommandStatus.Begin, Player_0.InventoryController));
			}

			public override void StartFireAnimation()
			{
				if (Weapon_0.MalfState.State == Weapon.EMalfunctionState.None)
				{
					if (Weapon_0 is RevolverItemClass && Weapon_0.CylinderHammerClosed)
					{
						FirearmsAnimator_0.Animator.Play(FirearmsAnimator_0.FullDoubleActionFireStateName, 1, 0f);
					}
					else
					{
						FirearmsAnimator_0.Animator.Play(FirearmsAnimator_0.FullFireStateName, 1, 0f);
					}
				}
			}

			public override void Reset()
			{
				AmmoItemClass = null;
				base.Reset();
			}

			public override void PrepareShot()
			{
			}

			public override void OnFireEvent()
			{
				AmmoItemClass = new AmmoItemClass(Guid.NewGuid().ToString(), Weapon_0.Template.DefAmmoTemplate);
				if (AmmoItemClass != null)
				{
					AmmoItemClass.IsUsed = true;
					FirearmController_0.method_55(AmmoItemClass);
					FirearmController_0.weaponManagerClass.MoveAmmoFromChamberToShellPort(AmmoItemClass.IsUsed);
					AmmoItemClass = null;
					Weapon_0.Repairable.Durability = 0f;
				}
			}

			public override void SetTriggerPressed(bool pressed)
			{
				FirearmController_0.IsTriggerPressed &= pressed;
			}

			public override void OnFireEndEvent()
			{
				Bool_1 = true;
				SetTriggerPressed(pressed: false);
				FirearmsAnimator_0.SetFire(fire: false);
				FirearmsAnimator_0.SetAmmoInChamber(Weapon_0.ChamberAmmoCount);
				FirearmsAnimator_0.SetShellsInWeapon(Weapon_0.ShellsInWeaponCount);
				Player_0.InventoryController.RaiseEvent(new GEventArgs4(Weapon_0, CommandStatus.Succeed, Player_0.InventoryController));
				SetAiming(isAiming: false);
				SetTriggerPressed(pressed: false);
				State = EOperationState.Finished;
				FirearmController_0.InitiateOperation<GClass2037>().Start();
			}

			public override void FastForward()
			{
				if (!Bool_1)
				{
					OnFireEvent();
				}
				OnFireEndEvent();
			}

			public override bool CanNotBeInterrupted()
			{
				return true;
			}
		}

		public class GClass2050 : GClass2013
		{
			[NonSerialized]
			public Slot Slot_0;

			[NonSerialized]
			public Callback Callback_0;

			[NonSerialized]
			public bool Bool_0;

			[NonSerialized]
			public bool Bool_1;

			[NonSerialized]
			public bool Bool_2;

			[NonSerialized]
			public bool Bool_3;

			[NonSerialized]
			public bool Bool_4;

			public GClass2050(FirearmController controller)
				: base(controller)
			{
			}

			public virtual void Start(MagazineItemClass magazine, Slot from, Callback callback)
			{
				Slot_0 = from;
				Callback_0 = callback;
				Bool_4 = magazine.IsMagazineWithBelt;
				Start();
				FirearmsAnimator_0.PullOutMagInInventoryMode();
				FirearmsAnimator_0.SetCanReload(canReload: false);
				FirearmsAnimator_0.ResetInsertMagInInventoryMode();
				FirearmsAnimator_0.SetFire(fire: false);
				Player_0.MovementContext.SetBlindFire(0);
				Player_0.BodyAnimatorCommon.SetFloat(PlayerAnimator.RELOAD_FLOAT_PARAM_HASH, 1f);
				FirearmController_0.bool_1 = true;
				if (Weapon_0.IsBeltMachineGun)
				{
					FirearmController_0.IsAiming = false;
				}
				if (!Weapon_0.MustBoltBeOpennedForExternalReload)
				{
					Bool_2 = true;
					Bool_3 = true;
					if (Weapon_0.MalfState.State == Weapon.EMalfunctionState.Misfire)
					{
						FirearmsAnimator_0.SetLayerWeight(FirearmsAnimator_0.MALFUNCTION_LAYER_INDEX, 0);
					}
				}
				else if (Weapon_0.MalfState.State == Weapon.EMalfunctionState.Misfire)
				{
					FirearmsAnimator_0.SetAmmoInChamber(1f);
					FirearmsAnimator_0.SetLayerWeight(FirearmsAnimator_0.MALFUNCTION_LAYER_INDEX, 0);
				}
				Bool_0 = false;
				Bool_1 = false;
				FirearmsAnimator_0.SetIsExternalMag(isExternalMag: true);
				MagazineItemClass currentMagazine = Weapon_0.GetCurrentMagazine();
				FirearmsAnimator_0.SetMagTypeCurrent(currentMagazine?.magAnimationIndex ?? 0);
				if (Weapon_0.IsBoltCatch && Weapon_0.ChamberAmmoCount == 1 && !Weapon_0.ManualBoltCatch && !Weapon_0.MustBoltBeOpennedForExternalReload && !Weapon_0.MustBoltBeOpennedForInternalReload)
				{
					FirearmsAnimator_0.SetBoltCatch(active: false);
				}
			}

			public override void Reset()
			{
				Slot_0 = null;
				Callback_0 = null;
				base.Reset();
			}

			public override void OnMagPulledOutFromWeapon()
			{
				Bool_0 = true;
				FirearmsAnimator_0.SetAmmoOnMag(0);
				FirearmsAnimator_0.SetMagInWeapon(ok: false);
				if (FirearmController_0.HasBipod)
				{
					FirearmController_0.FirearmsAnimator.SetBipod(FirearmController_0.BipodState);
				}
			}

			public override void OnMagPuttedToRig()
			{
				Bool_1 = true;
				WeaponManagerClass.RemoveMod(Slot_0);
				State = EOperationState.Finished;
				FirearmController_0.RecalculateErgonomic();
				FirearmController_0.InitiateOperation<GClass2037>().Start();
				Callback_0.Succeed();
				FirearmController_0.WeaponModified();
				if (Bool_4)
				{
					FirearmController_0.weaponPrefab_0.UpdateAnimatorHierarchy();
					if (FirearmController_0.HasBipod)
					{
						FirearmController_0.FirearmsAnimator.SetBipod(FirearmController_0.BipodState);
					}
				}
			}

			public override void SetInventoryOpened(bool opened)
			{
				FirearmController_0.InventoryOpened = opened;
				FirearmsAnimator_0.SetInventory(opened);
			}

			public override void OnShellEjectEvent()
			{
				Bool_2 = true;
				if (Weapon_0.MustBoltBeOpennedForExternalReload && Weapon_0.MalfState.State == Weapon.EMalfunctionState.Misfire)
				{
					WeaponManagerClass.CreatePatronInShellPort(Weapon_0.MalfState.MalfunctionedAmmo);
					WeaponManagerClass.StartSpawnMisfiredCartridge(Player_0.Velocity * 0.66f);
					return;
				}
				AmmoItemClass ammoItemClass = null;
				for (int i = 0; i < Weapon_0.Chambers.Length; i++)
				{
					ammoItemClass = (AmmoItemClass)Weapon_0.Chambers[i].ContainedItem;
					if (ammoItemClass != null && !ammoItemClass.IsUsed)
					{
						break;
					}
				}
				WeaponManagerClass.MoveAmmoFromChamberToShellPort(ammoItemClass.IsUsed);
				WeaponManagerClass.StartSpawnShell(Player_0.Velocity * 0.66f);
			}

			public override void RemoveAmmoFromChamber()
			{
				Bool_3 = true;
				if (Weapon_0.MustBoltBeOpennedForExternalReload && Weapon_0.MalfState.State == Weapon.EMalfunctionState.Misfire)
				{
					method_2();
					FirearmsAnimator_0.SetAmmoInChamber(Weapon_0.ChamberAmmoCount);
					return;
				}
				bool flag = false;
				Slot[] chambers = Weapon_0.Chambers;
				for (int i = 0; i < chambers.Length; i++)
				{
					if (flag)
					{
						break;
					}
					Slot slot = chambers[i];
					AmmoItemClass ammoItemClass = (AmmoItemClass)slot.ContainedItem;
					flag |= !ammoItemClass.IsUsed && slot.RemoveItem().Succeeded;
				}
				FirearmsAnimator_0.SetAmmoInChamber(Weapon_0.ChamberAmmoCount);
			}

			public override void OnOnOffBoltCatchEvent(bool isCaught)
			{
				FirearmsAnimator_0.SetBoltCatch(isCaught);
			}

			public override bool CanChangeLightState(FirearmLightStateStruct[] lightsStates)
			{
				return false;
			}

			public override void FastForward()
			{
				if (!Bool_3)
				{
					RemoveAmmoFromChamber();
				}
				if (!Bool_2)
				{
					OnShellEjectEvent();
				}
				if (!Bool_0)
				{
					OnMagPulledOutFromWeapon();
				}
				if (!Bool_1)
				{
					OnMagPuttedToRig();
				}
				FirearmsAnimator_0.Animator.Play(FirearmsAnimator_0.FullIdleStateName, 1, 0.1f);
			}
		}

		public class RechamberOperationClass : GClass2013
		{
			[CompilerGenerated]
			public class Class1233
			{
				public RechamberOperationClass RechamberOperationClass;

				public Action onHidden;

				public bool fastDrop;

				public Item nextControllerItem;

				public void method_0()
				{
					RechamberOperationClass.FirearmController_0.InitiateOperation<GClass2053>().Start(onHidden, fastDrop, nextControllerItem);
				}
			}

			[NonSerialized]
			public Item Item_0;

			[NonSerialized]
			public Item Item_1;

			[NonSerialized]
			public Action Action_0;

			[NonSerialized]
			public Callback Callback_0;

			[NonSerialized]
			public bool Bool_0;

			[NonSerialized]
			public bool Bool_1;

			[NonSerialized]
			public bool Bool_2;

			public RechamberOperationClass(FirearmController controller)
				: base(controller)
			{
			}

			public virtual void Start(AmmoItemClass ammo, Callback callback)
			{
				Start();
				Callback_0 = callback;
				if (Weapon_0.MalfState.State == Weapon.EMalfunctionState.Misfire)
				{
					method_2();
				}
				if (Weapon_0.ChamberAmmoCount == 0)
				{
					SwitchToIdle();
					return;
				}
				if (Weapon_0.HasChambers)
				{
					_ = Weapon_0.Chambers[0];
				}
				Item_0 = ammo;
				InteractionsHandlerClass.Remove(Item_0, Player_0.InventoryController, simulate: true);
				MagazineItemClass currentMagazine = Weapon_0.GetCurrentMagazine();
				if (currentMagazine != null && currentMagazine.Count > 0)
				{
					Item_1 = currentMagazine.Cartridges.Last;
					Player_0.ExecuteSkill((Action)delegate
					{
						Player_0.Skills.WeaponChamberAction.Complete(Weapon_0);
					});
				}
				FirearmController_0.SetAim(value: false);
				Player_0.MovementContext.SetBlindFire(0);
				FirearmsAnimator_0.SetAmmoInChamber(Weapon_0.ChamberAmmoCount);
				FirearmsAnimator_0.Rechamber(val: true);
				FirearmsAnimator_0.SetInventory(open: false);
				FirearmsAnimator_0.SetFire(fire: false);
				Player_0.BodyAnimatorCommon.SetFloat(PlayerAnimator.RELOAD_FLOAT_PARAM_HASH, 1f);
			}

			public override void Reset()
			{
				Item_0 = null;
				Item_1 = null;
				Action_0 = null;
				Bool_0 = false;
				Bool_1 = false;
				Bool_2 = false;
				base.Reset();
			}

			public override void RemoveAmmoFromChamber()
			{
				if (!Bool_0)
				{
					Bool_0 = true;
					WeaponManagerClass.RemovePatronInWeapon();
					FirearmsAnimator_0.SetAmmoInChamber(0f);
				}
			}

			public override void OnOnOffBoltCatchEvent(bool isCatched)
			{
				FirearmsAnimator_0.SetBoltCatch(isCatched);
			}

			public override void OnAddAmmoInChamber()
			{
				if (!Bool_1)
				{
					Bool_1 = true;
					if (Item_1 != null)
					{
						WeaponManagerClass.SetRoundIntoWeapon((AmmoItemClass)Item_1);
						SwitchToIdle();
					}
				}
			}

			public override void OnShellEjectEvent()
			{
				if (!Bool_2)
				{
					Bool_2 = true;
					Callback_0?.Succeed();
					WeaponManagerClass.ThrowPatronAsLoot(Item_0, Player_0, "RechamberOperation.OnShellEjectEvent");
					if (Item_1 == null)
					{
						SwitchToIdle();
					}
				}
			}

			public override void HideWeapon(Action onHidden, bool fastDrop, Item nextControllerItem = null)
			{
				Action_0 = delegate
				{
					FirearmController_0.InitiateOperation<GClass2053>().Start(onHidden, fastDrop, nextControllerItem);
				};
			}

			public override void FastForward()
			{
				if (State == EOperationState.Finished)
				{
					return;
				}
				RemoveAmmoFromChamber();
				MagazineItemClass currentMagazine = Weapon_0.GetCurrentMagazine();
				bool flag = Weapon_0.ReloadMode == Weapon.EReloadMode.ExternalMagazine || Weapon_0.ReloadMode == Weapon.EReloadMode.ExternalMagazineWithInternalReloadSupport;
				bool flag2 = currentMagazine != null && currentMagazine.Count > 0;
				if (Weapon_0.IsBoltCatch && Item_1 == null && !flag2 && ((flag && currentMagazine != null) || !flag))
				{
					OnOnOffBoltCatchEvent(true);
				}
				OnShellEjectEvent();
				if (State != EOperationState.Finished)
				{
					OnAddAmmoInChamber();
					if (State != EOperationState.Finished)
					{
						SwitchToIdle();
					}
				}
			}

			public virtual void SwitchToIdle()
			{
				FirearmsAnimator_0.SetAmmoInChamber(Weapon_0.ChamberAmmoCount);
				FirearmsAnimator_0.SetAmmoOnMag(Weapon_0.GetCurrentMagazineCount());
				FirearmsAnimator_0.Rechamber(val: false);
				FirearmsAnimator_0.SetAmmoInChamber(Weapon_0.ChamberAmmoCount);
				FirearmsAnimator_0.SetInventory(FirearmController_0.bool_2);
				State = EOperationState.Finished;
				if (Action_0 == null)
				{
					FirearmController_0.InitiateOperation<GClass2037>().Start();
				}
				else
				{
					Action_0();
				}
			}

			[CompilerGenerated]
			public void method_5()
			{
				Player_0.Skills.WeaponChamberAction.Complete(Weapon_0);
			}
		}

		public class CylinderReloadOperationClass(FirearmController controller) : AmmoPackReloadOperationClass(controller)
		{
			[NonSerialized]
			public bool Bool_3;

			[NonSerialized]
			public CylinderMagazineItemClass CylinderMagazineItemClass;

			[NonSerialized]
			public List<int> List_0 = new List<int>();

			[NonSerialized]
			public List<int> List_1 = new List<int>();

			[NonSerialized]
			public int Int_1 = -1;

			[NonSerialized]
			public bool Bool_4;

			[NonSerialized]
			public int Int_2;

			[NonSerialized]
			public bool Bool_5;

			public virtual void Start(AmmoPackReloadingClass ammoPack, Callback callback, bool quickReload = false)
			{
				base.Start(ammoPack, callback);
				Bool_4 = quickReload;
				method_9();
				base.Boolean_0 = false;
			}

			public override void OnMagPuttedToRig()
			{
				method_13();
				SwitchToIdle();
			}

			public override void Reset()
			{
				base.Reset();
				Bool_4 = false;
				Bool_5 = false;
				Bool_3 = false;
				CylinderMagazineItemClass = null;
				Int_1 = -1;
				Int_2 = 0;
				List_0.Clear();
				List_1.Clear();
			}

			public void method_9()
			{
				CylinderMagazineItemClass = Weapon_0.GetCurrentMagazine() as CylinderMagazineItemClass;
				Int_2 = CylinderMagazineItemClass.Count;
				Bool_3 = (Bool_4 || CylinderMagazineItemClass.Count == 0) && FirearmController_0.CurrentMasteringLevel > 0;
				FirearmsAnimator_0.SetWeaponLevel(Bool_3 ? 1 : 0);
				List_0 = (Bool_3 ? CylinderMagazineItemClass.GetCamorasIndexesList() : CylinderMagazineItemClass.GetFreeCamorasIndexesFromCurrentActiveIndex(Bool_4, !Weapon_0.CylinderHammerClosed));
				Weapon_0.GetShellsIndexes(List_1);
				FirearmsAnimator_0.SetShellsInWeapon(List_1.Count);
				FirearmsAnimator_0.SetAmmoOnMag(CylinderMagazineItemClass.Count);
				SendReloadCommand();
				if (CylinderMagazineItemClass.Count != 0)
				{
					method_16();
					FirearmsAnimator_0.SetCamoraIndexWithShellForRemove(Int_1);
				}
				if (Bool_4)
				{
					FirearmsAnimator_0.ReloadFast(Bool_4);
					FirearmsAnimator_0.SetAmmoCountForRemove(List_1.Count + CylinderMagazineItemClass.Count);
					FirearmsAnimator_0.ResetReload();
					Int_2 = 0;
				}
				else
				{
					FirearmsAnimator_0.SetAmmoCountForRemove(List_1.Count);
				}
				method_14();
			}

			public override void OnOnOffBoltCatchEvent(bool isCatched)
			{
				FirearmsAnimator_0.SetBoltCatch(isCatched);
			}

			public override void OnMagAppeared()
			{
				method_10();
			}

			public void method_10()
			{
				if (Bool_3)
				{
					CylinderMagazineItemClass.SetCurrentCamoraIndex(Weapon_0.CylinderHammerClosed ? (CylinderMagazineItemClass.MaxCount - 1) : 0);
					for (int i = 0; i < CylinderMagazineItemClass.MaxCount && i < AmmoPackReloadingClass.AmmoCount; i++)
					{
						AmmoItemClass ammoToReload = AmmoPackReloadingClass.GetAmmoToReload(i);
						WeaponManagerClass.SetRoundIntoWeapon(ammoToReload, i);
					}
				}
			}

			public void method_11()
			{
				if (Bool_3)
				{
					for (int i = Int_0; i < List_0.Count && i < AmmoPackReloadingClass.AmmoCount; i++)
					{
						WeaponManagerClass.DestroyPatronInWeapon(List_0[i]);
					}
				}
			}

			public override void RemoveAmmoFromChamber()
			{
				method_11();
			}

			public override void OnRemoveShellEvent()
			{
				if (CylinderMagazineItemClass.Count == 0)
				{
					method_17();
				}
				else if (Bool_4)
				{
					method_17();
					method_18();
				}
				else
				{
					Weapon_0.ShellsInChambers[Int_1] = null;
					WeaponManagerClass.StartSpawnShell(Player_0.Velocity * 0.33f, Int_1);
					method_16();
				}
			}

			public override void OnShellEjectEvent()
			{
				FirearmsAnimator_0.SetCamoraIndexWithShellForRemove(Int_1);
			}

			public override void OnAddAmmoInChamber()
			{
				if (Bool_3)
				{
					method_12();
				}
				else
				{
					method_15();
				}
				FirearmsAnimator_0.SetAmmoOnMag(CylinderMagazineItemClass.Count + Int_0);
			}

			public void method_12()
			{
				if (Int_0 < List_0.Count && Int_0 < AmmoPackReloadingClass.AmmoCount)
				{
					Int_0++;
					FirearmsAnimator_0.SetAmmoOnMag(Int_0);
					if (Int_0 >= List_0.Count || Int_0 >= AmmoPackReloadingClass.AmmoCount)
					{
						AddAmmoToMag();
					}
				}
			}

			public override void AddAmmoToMag()
			{
				method_14();
				base.Boolean_0 = true;
				FirearmsAnimator_0.SetAmmoOnMag(CylinderMagazineItemClass.Count + Int_0);
				if (!CanReload() || Bool_1)
				{
					FirearmsAnimator_0.SetMasteringReloadAborted(Bool_1);
					method_13();
				}
			}

			public static void smethod_0(int ammoToLoadIntoMag, AmmoPackReloadingClass ammoPack, Player player, CylinderMagazineItemClass magazine, Weapon weapon, List<int> camorasIndexesForLoadAmmo)
			{
				for (int i = 0; i < ammoToLoadIntoMag; i++)
				{
					GStruct154<GInterface424> gStruct = ammoPack.LoadAmmo(player.InventoryController, player.InventoryController, magazine.Camoras[camorasIndexesForLoadAmmo[i]].CreateItemAddress());
					if (gStruct.Error == null)
					{
						gStruct.Value.RaiseEvents(player.InventoryController, CommandStatus.Begin);
						gStruct.Value.RaiseEvents(player.InventoryController, CommandStatus.Succeed);
						continue;
					}
					UnityEngine.Debug.LogError("SwitchToIdle: Cannot load ammo. AmmoCount: " + ammoPack.AmmoCount + ", AmmoToLoadIntoMag: " + (ammoToLoadIntoMag - i) + ", Error: " + gStruct.Error);
				}
			}

			public override void HideWeapon(Action onHidden, bool fastDrop, Item nextControllerItem = null)
			{
				base.HideWeapon(onHidden, fastDrop, (Item)null);
				Bool_1 = true;
			}

			public override void FastForward()
			{
				if (State != EOperationState.Finished && State != EOperationState.Finished)
				{
					method_13();
					SwitchToIdle();
				}
			}

			public void method_13()
			{
				if (Bool_5)
				{
					return;
				}
				Bool_5 = true;
				if (Int_0 > 0)
				{
					Player_0.ExecuteSkill((Action)delegate
					{
						Player_0.Skills.WeaponReloadAction.Complete(Weapon_0);
					});
				}
				Weapon_0.Parent.RaiseRemoveEvent(Weapon_0, CommandStatus.Failed, Player_0.InventoryController);
				FirearmsAnimator_0.SetCamoraIndex(CylinderMagazineItemClass.CurrentCamoraIndex);
				FirearmsAnimator_0.SetCanReload(canReload: false);
				if (Weapon_0.MalfState.State == Weapon.EMalfunctionState.Misfire)
				{
					FirearmsAnimator_0.SetLayerWeight(FirearmsAnimator_0.MALFUNCTION_LAYER_INDEX, 1);
				}
			}

			public virtual void SwitchToIdle()
			{
				AmmoPackReloadingClass.UnlockItems();
				method_19();
				FirearmsAnimator_0.SetMasteringReloadAborted(value: false);
				FirearmsAnimator_0.SetInventory(FirearmController_0.InventoryOpened);
				Action action_ = Action_0;
				bool bool_ = Bool_0;
				Weapon_0.RaiseRefreshEvent();
				State = EOperationState.Finished;
				GClass2037 gClass = FirearmController_0.InitiateOperation<GClass2037>();
				gClass.Start();
				method_6();
				if (action_ != null)
				{
					gClass.HideWeapon(action_, bool_);
				}
			}

			public void method_14()
			{
				if (Int_0 < List_0.Count)
				{
					int camoraIndexForLoadAmmo = List_0[Int_0];
					FirearmsAnimator_0.SetCamoraIndexForLoadAmmo(camoraIndexForLoadAmmo);
				}
			}

			public void method_15()
			{
				if (Int_0 < List_0.Count && Int_0 < AmmoPackReloadingClass.AmmoCount)
				{
					FirearmsAnimator_0.SetCamoraIndex(CylinderMagazineItemClass.CurrentCamoraIndex);
					FirearmsAnimator_0.SetCamoraIndexForLoadAmmo(List_0[Int_0]);
					AmmoItemClass ammoToReload = AmmoPackReloadingClass.GetAmmoToReload(Int_0);
					WeaponManagerClass.SetRoundIntoWeapon(ammoToReload, List_0[Int_0]);
					Int_0++;
				}
			}

			public virtual void SendReloadCommand()
			{
			}

			public void method_16()
			{
				FirearmsAnimator_0.SetShellsInWeapon(List_1.Count);
				if (!Bool_4 && List_1.Count != 0)
				{
					Int_1 = List_1.First();
					List_1.RemoveAt(0);
				}
			}

			public override bool CanReload()
			{
				if (AmmoPackReloadingClass.AmmoCount - Int_0 > 0)
				{
					return Int_2 + Int_0 != MagazineItemClass.MaxCount;
				}
				return false;
			}

			public void method_17()
			{
				for (int i = 0; i < Weapon_0.ShellsInChambers.Length; i++)
				{
					Weapon_0.ShellsInChambers[i] = null;
				}
				WeaponManagerClass.StartSpawnAllShells(Vector3.down);
				FirearmsAnimator_0.SetShellsInWeapon(0);
			}

			public void method_18()
			{
				WeaponManagerClass.DestroyAllPatronsInWeapon();
				Slot[] camoras = CylinderMagazineItemClass.Camoras;
				int num = 0;
				GStruct154<GInterface424> gStruct;
				while (true)
				{
					if (num >= camoras.Length)
					{
						return;
					}
					Item containedItem = camoras[num].ContainedItem;
					if (containedItem != null)
					{
						gStruct = CylinderMagazineItemClass.RemoveAmmoInCamora(containedItem, Player_0.InventoryController);
						if (gStruct.Failed)
						{
							break;
						}
						WeaponManagerClass.ThrowPatronAsLoot(containedItem, Player_0, "ReloadCylinderMagOperation.RemoveAllAmmo");
					}
					num++;
				}
				UnityEngine.Debug.LogError(gStruct.Error);
			}

			public void method_19()
			{
				AmmoPackReloadingClass ammoPackReloadingClass = AmmoPackReloadingClass;
				CylinderMagazineItemClass cylinderMagazineItemClass = CylinderMagazineItemClass;
				Weapon weapon_ = Weapon_0;
				int int_ = Int_0;
				Player player_ = Player_0;
				List<int> list_ = List_0;
				smethod_0(int_, ammoPackReloadingClass, player_, cylinderMagazineItemClass, weapon_, list_);
				if (!weapon_.CylinderHammerClosed)
				{
					CylinderMagazineItemClass.SetNotEmptyCamoraAsActive();
				}
				FirearmsAnimator_0.SetAmmoOnMag(cylinderMagazineItemClass.Count);
				Player_0.InventoryController.CheckChamber(Weapon_0, status: true);
			}

			[CompilerGenerated]
			public void method_20()
			{
				Player_0.Skills.WeaponReloadAction.Complete(Weapon_0);
			}
		}

		public class GClass2016 : GClass2015
		{
			[NonSerialized]
			public Callback Callback_1;

			[NonSerialized]
			public bool Bool_1;

			[NonSerialized]
			public bool Bool_2;

			[NonSerialized]
			public bool Bool_3;

			[NonSerialized]
			public bool Bool_4;

			[NonSerialized]
			public bool Bool_5;

			[NonSerialized]
			public bool Bool_6;

			[NonSerialized]
			public bool Bool_7;

			[NonSerialized]
			public bool Bool_8;

			[NonSerialized]
			public GClass2006 Gclass2006_0;

			public GClass2016(FirearmController controller)
				: base(controller)
			{
			}

			public override void SetAiming(bool isAiming)
			{
				if (!Gclass2006_0.NextMagazine.IsMagazineWithBelt)
				{
					method_5(isAiming);
				}
			}

			public virtual void Start(GClass2006 reloadExternalMagResult, [CanBeNull] Callback callback)
			{
				Callback_1 = callback;
				Start(callback);
				FirearmController_0.bool_1 = true;
				Gclass2006_0 = reloadExternalMagResult;
				FirearmsAnimator_0.SetAmmoCompatible(Gclass2006_0.AmmoCompatible);
				FirearmsAnimator_0.SetIsExternalMag(isExternalMag: true);
				if (Gclass2006_0.RemoveFromChamberResult == null)
				{
					Bool_1 = true;
				}
				if (Gclass2006_0.PopNewAmmoResult == null)
				{
					Bool_6 = true;
				}
				FirearmsAnimator_0.SetCanReload(canReload: true);
				FirearmsAnimator_0.Reload((Gclass2006_0.OldMagazine != null) ? Gclass2006_0.OldMagazine.magAnimationIndex : (-1), Gclass2006_0.NextMagazine.magAnimationIndex, reloadExternalMagResult.QuickReload);
				Gclass2006_0.RaiseEvents(Player_0.InventoryController, CommandStatus.Begin);
				if (Weapon_0.IsBoltCatch && Weapon_0.ChamberAmmoCount == 1 && Gclass2006_0.PopNewAmmoResult == null && !Weapon_0.ManualBoltCatch && !Weapon_0.MustBoltBeOpennedForExternalReload && !Weapon_0.MustBoltBeOpennedForInternalReload)
				{
					FirearmsAnimator_0.SetBoltCatch(active: false);
				}
				Player_0.Say(EPhraseTrigger.OnWeaponReload);
				if (Weapon_0.MalfState.State == Weapon.EMalfunctionState.Misfire && Weapon_0.MalfState.IsKnownMalfunction(Player_0.ProfileId) && Gclass2006_0.NextMagazine.Count > 0 && Gclass2006_0.AmmoCompatible)
				{
					Bool_6 = false;
					FirearmsAnimator_0.SetAmmoInChamber(0f);
					FirearmsAnimator_0.SetLayerWeight(FirearmsAnimator_0.MALFUNCTION_LAYER_INDEX, 0);
				}
			}

			public override void Reset()
			{
				Callback_1 = null;
				Bool_1 = false;
				Bool_2 = false;
				Bool_3 = false;
				Bool_4 = false;
				Bool_5 = false;
				Bool_6 = false;
				Bool_8 = false;
				Gclass2006_0 = null;
				base.Reset();
			}

			public override void UseSecondMagForReload()
			{
				method_9();
			}

			public override void OnShellEjectEvent()
			{
				WeaponManagerClass.StartSpawnShell(Player_0.Velocity * 0.66f);
			}

			public override void RemoveAmmoFromChamber()
			{
				if (!Bool_1)
				{
					Bool_1 = true;
					if (Gclass2006_0.RemoveFromChamberResult != null)
					{
						WeaponManagerClass.RemovePatronInWeapon();
					}
					FirearmsAnimator_0.SetAmmoInChamber(0f);
					if (Gclass2006_0?.RemoveFromChamberResult != null)
					{
						WeaponManagerClass.ThrowPatronAsLoot(Gclass2006_0.RemoveFromChamberResult.Item, Player_0, "ReloadExternalMagOperation.RemoveAmmoFromChamber");
					}
				}
			}

			public override void OnMagPulledOutFromWeapon()
			{
				if (!Bool_2)
				{
					Bool_2 = true;
					method_7();
					FirearmsAnimator_0.SetMagInWeapon(ok: false);
				}
			}

			public override void OnMagPuttedToRig()
			{
				if (!Bool_3)
				{
					Bool_3 = true;
					if (Gclass2006_0.RemoveOldMagResult != null)
					{
						Item item = Gclass2006_0.RemoveOldMagResult.Item;
						DropMod(item, EWeaponModType.mod_magazine);
					}
					WeaponManagerClass.RemoveMod(Gclass2006_0.MagazineSlot);
				}
			}

			public virtual void DropMod(Item droppedMod, EWeaponModType modType)
			{
				BifacialTransform bodyTransform = Player_0.PlayerBones.BodyTransform;
				Transform original = bodyTransform.Original;
				Transform modTransform = WeaponManagerClass.GetModTransform(modType);
				Vector3 position = original.InverseTransformPoint(modTransform.position);
				Quaternion quaternion = Quaternion.Inverse(original.rotation) * modTransform.rotation;
				Vector3 position2 = bodyTransform.TransformPoint(position);
				Quaternion rotation = bodyTransform.rotation * quaternion;
				Vector3 velocity = Vector3.down / 100f;
				Vector3 angularVelocity = new Vector3(UnityEngine.Random.Range(-3f, 3f), UnityEngine.Random.Range(-3f, 3f), UnityEngine.Random.Range(-3f, 3f));
				Singleton<GameWorld>.Instance.ThrowItem(droppedMod, Player_0, position2, rotation, velocity, angularVelocity, syncable: true);
			}

			public override void OnMagAppeared()
			{
				if (Bool_4)
				{
					return;
				}
				Bool_4 = true;
				method_7();
				WeaponManagerClass.SetupMod(Gclass2006_0.MagazineSlot, Singleton<PoolManagerClass>.Instance.CreateItem(Gclass2006_0.NextMagazine, GetVisibleToCamera(Player_0), Player_0, isAnimated: true));
				if (Gclass2006_0.NextMagazine.IsMagazineWithBelt)
				{
					FirearmController_0.weaponPrefab_0.UpdateAnimatorHierarchy();
					if (FirearmController_0.HasBipod)
					{
						FirearmController_0.FirearmsAnimator.SetBipod(FirearmController_0.BipodState);
					}
				}
			}

			public override void OnMagInsertedToWeapon()
			{
				if (!Bool_5)
				{
					Bool_5 = true;
					FirearmsAnimator_0.SetMagInWeapon(ok: true);
					FirearmsAnimator_0.SetAmmoOnMag(Gclass2006_0.NextMagazine.Count + ((Gclass2006_0.PopNewAmmoResult != null) ? 1 : 0));
					if (Weapon_0.HasChambers && Gclass2006_0.PopNewAmmoResult == null && Weapon_0.MalfState.State != Weapon.EMalfunctionState.SoftSlide && Weapon_0.MalfState.State != Weapon.EMalfunctionState.HardSlide && Weapon_0.MalfState.State != Weapon.EMalfunctionState.Jam && (Weapon_0.MalfState.State != Weapon.EMalfunctionState.Misfire || !Weapon_0.MalfState.IsKnownMalfunction(Player_0.ProfileId) || !Gclass2006_0.AmmoCompatible))
					{
						SwitchToIdlingState();
					}
					FirearmsAnimator_0.SetMagTypeCurrent(Gclass2006_0.NextMagazine.magAnimationIndex);
				}
			}

			public override void OnOnOffBoltCatchEvent(bool isCatched)
			{
				if (isCatched || !Bool_8)
				{
					if (!isCatched)
					{
						Bool_8 = true;
					}
					FirearmsAnimator_0.SetBoltCatch(isCatched);
				}
			}

			public override void OnAddAmmoInChamber()
			{
				if (!Bool_6)
				{
					Bool_6 = true;
					if (Weapon_0.MalfState.State == Weapon.EMalfunctionState.Misfire)
					{
						method_2();
					}
					FirearmsAnimator_0.SetAmmoOnMag(Gclass2006_0.NextMagazine.Count);
					if (Weapon_0.HasChambers && Gclass2006_0.PopNewAmmoResult != null)
					{
						WeaponManagerClass.SetRoundIntoWeapon((AmmoItemClass)Gclass2006_0.PopNewAmmoResult.ResultItem);
					}
					FirearmsAnimator_0.SetAmmoInChamber(Gclass2006_0.Weapon.ChamberAmmoCount);
					if (!Bool_8)
					{
						OnOnOffBoltCatchEvent(false);
					}
					SwitchToIdlingState();
				}
			}

			public override void SetInventoryOpened(bool opened)
			{
				FirearmController_0.InventoryOpened = opened;
				if (Bool_2 || Bool_4)
				{
					FirearmsAnimator_0.SetInventory(opened);
				}
			}

			public virtual void SwitchToIdlingState()
			{
				if (State != EOperationState.Finished)
				{
					State = EOperationState.Finished;
					Action action_ = Action_0;
					bool bool_ = Bool_0;
					Gclass2006_0.RaiseEvents(Player_0.InventoryController, CommandStatus.Succeed);
					FirearmController_0.RecalculateErgonomic();
					GClass2037 gClass = FirearmController_0.InitiateOperation<GClass2037>();
					gClass.Start();
					Callback_1?.Succeed();
					FirearmController_0.WeaponModified();
					if (action_ != null)
					{
						gClass.HideWeapon(action_, bool_);
					}
				}
			}

			public void method_9()
			{
				UnityEngine.Debug.LogError("Not Implemented");
			}

			public override void OnIdleStartEvent()
			{
				if (Bool_4)
				{
					FirearmController_0.bool_1 = false;
					SwitchToIdlingState();
				}
			}

			public override void FastForward()
			{
				if (State != EOperationState.Finished)
				{
					RemoveAmmoFromChamber();
					OnMagPulledOutFromWeapon();
					OnMagPuttedToRig();
					OnMagAppeared();
					OnMagInsertedToWeapon();
					OnAddAmmoInChamber();
					OnOnOffBoltCatchEvent(false);
					if (State != EOperationState.Finished)
					{
						SwitchToIdlingState();
					}
					FirearmsAnimator_0.Animator.Play(FirearmsAnimator_0.FullIdleStateName, 1, 0.1f);
				}
			}
		}

		public class GClass2006
		{
			public readonly TraderControllerClass ItemController;

			public readonly Weapon Weapon;

			public readonly bool AmmoCompatible;

			[CanBeNull]
			public readonly MagazineItemClass OldMagazine;

			public readonly MagazineItemClass NextMagazine;

			public readonly Slot MagazineSlot;

			public readonly bool QuickReload;

			public readonly bool IsKnownMalfunction;

			[CanBeNull]
			public readonly GClass3410 RemoveFromChamberResult;

			[CanBeNull]
			public readonly GClass3410 RemoveOldMagResult;

			[CanBeNull]
			public readonly GClass3411 MoveOldMagResult;

			public readonly GClass3411 InsertNextMagResult;

			[CanBeNull]
			public readonly GInterface424 PopNewAmmoResult;

			public GClass2006(TraderControllerClass itemController, [CanBeNull] GClass3410 removeFromChamberResult, [CanBeNull] GClass3410 removeOldMagResult, [CanBeNull] GClass3411 moveOldMagResult, GClass3411 insertNextMagResult, [CanBeNull] GInterface424 popNewAmmoResult, Weapon weapon, bool ammoCompatible, bool quickReload, bool isKnownMalfunction)
			{
				ItemController = itemController;
				Weapon = weapon;
				AmmoCompatible = ammoCompatible;
				OldMagazine = ((removeOldMagResult != null) ? ((MagazineItemClass)removeOldMagResult.Item) : ((MagazineItemClass)(moveOldMagResult?.Item)));
				NextMagazine = (MagazineItemClass)insertNextMagResult.Item;
				MagazineSlot = ((GClass3391)insertNextMagResult.To).Slot;
				QuickReload = quickReload;
				IsKnownMalfunction = isKnownMalfunction;
				RemoveFromChamberResult = removeFromChamberResult;
				RemoveOldMagResult = removeOldMagResult;
				MoveOldMagResult = moveOldMagResult;
				InsertNextMagResult = insertNextMagResult;
				PopNewAmmoResult = popNewAmmoResult;
			}

			public void RollBack()
			{
				PopNewAmmoResult?.RollBack();
				InsertNextMagResult.RollBack();
				RemoveOldMagResult?.RollBack();
				MoveOldMagResult?.RollBack();
				RemoveFromChamberResult?.RollBack();
			}

			public void RaiseEvents(TraderControllerClass controller, CommandStatus status)
			{
				Weapon.Parent.RaiseRemoveEvent(Weapon, (status != CommandStatus.Begin) ? CommandStatus.Failed : CommandStatus.Begin, controller);
				RemoveFromChamberResult?.RaiseEvents(controller, status);
				RemoveOldMagResult?.RaiseEvents(controller, status);
				MoveOldMagResult?.RaiseEvents(controller, status);
				InsertNextMagResult.RaiseEvents(controller, status);
				PopNewAmmoResult?.RaiseEvents(controller, status);
			}

			public static GStruct156<GClass2006> Run(TraderControllerClass itemController, Weapon weapon, MagazineItemClass nextMagazine, bool quickReload, bool isKnownMalfunction, [CanBeNull] ItemAddress vestTargetAddress)
			{
				Slot slot = (weapon.HasChambers ? weapon.Chambers[0] : null);
				AmmoItemClass ammoItemClass = slot?.ContainedItem as AmmoItemClass;
				MagazineItemClass currentMagazine = weapon.GetCurrentMagazine();
				Slot magazineSlot = weapon.GetMagazineSlot();
				Weapon.EMalfunctionState state = weapon.MalfState.State;
				if (state == Weapon.EMalfunctionState.Misfire)
				{
					weapon.MalfState.ChangeStateSilent(Weapon.EMalfunctionState.None);
				}
				GStruct154<GClass3410> gStruct = ((ammoItemClass == null || !weapon.MustBoltBeOpennedForExternalReload) ? default(GStruct154<GClass3410>) : InteractionsHandlerClass.Remove(ammoItemClass, itemController));
				weapon.MalfState.ChangeStateSilent(state);
				if (gStruct.Failed)
				{
					return gStruct.Error;
				}
				GStruct154<GClass3410> gStruct2 = default(GStruct154<GClass3410>);
				GStruct154<GClass3411> gStruct3 = default(GStruct154<GClass3411>);
				if (currentMagazine != null)
				{
					if (vestTargetAddress != null)
					{
						gStruct3 = InteractionsHandlerClass.Move(currentMagazine, vestTargetAddress, itemController);
						if (gStruct3.Failed)
						{
							gStruct.Value?.RollBack();
							return gStruct3.Error;
						}
					}
					else
					{
						gStruct2 = InteractionsHandlerClass.Remove(currentMagazine, itemController);
						if (gStruct2.Failed)
						{
							gStruct.Value?.RollBack();
							return gStruct2.Error;
						}
					}
				}
				GStruct154<GClass3411> gStruct4 = InteractionsHandlerClass.Move(nextMagazine, magazineSlot.CreateItemAddress(), itemController);
				if (gStruct4.Failed)
				{
					gStruct2.Value?.RollBack();
					gStruct3.Value?.RollBack();
					gStruct.Value?.RollBack();
					return gStruct4.Error;
				}
				bool flag = nextMagazine.IsAmmoCompatible(weapon.Chambers);
				GStruct154<GInterface424> gStruct5 = default(GStruct154<GInterface424>);
				if (slot != null && slot.ContainedItem == null && nextMagazine.Count > 0 && flag)
				{
					bool num = weapon.MalfState.State == Weapon.EMalfunctionState.None || (weapon.MalfState.State == Weapon.EMalfunctionState.Misfire && isKnownMalfunction);
					Weapon.EMalfunctionState state2 = weapon.MalfState.State;
					if (num && weapon.MalfState.State != Weapon.EMalfunctionState.None)
					{
						weapon.MalfState.ChangeStateSilent(Weapon.EMalfunctionState.None);
					}
					if (num)
					{
						gStruct5 = nextMagazine.Cartridges.PopTo(itemController, slot.CreateItemAddress());
						if (state2 != Weapon.EMalfunctionState.None)
						{
							weapon.MalfState.ChangeStateSilent(state2);
						}
						if (gStruct5.Failed)
						{
							gStruct4.Value.RollBack();
							gStruct2.Value?.RollBack();
							gStruct3.Value?.RollBack();
							gStruct.Value?.RollBack();
							return gStruct5.Error;
						}
					}
				}
				return new GClass2006(itemController, gStruct.Value, gStruct2.Value, gStruct3.Value, gStruct4.Value, gStruct5.Value, weapon, flag, quickReload, isKnownMalfunction);
			}

			public bool CanExecute(TraderControllerClass itemController)
			{
				if (Weapon.CheckAction(null).Failed)
				{
					return false;
				}
				if (NextMagazine.CheckAction(null).Failed)
				{
					return false;
				}
				if (OldMagazine != null && OldMagazine.CheckAction(MagazineSlot.CreateItemAddress()).Failed)
				{
					return false;
				}
				if (MoveOldMagResult != null && MoveOldMagResult.Item.CheckAction(MoveOldMagResult.To).Failed)
				{
					return false;
				}
				return true;
			}
		}

		public class AmmoPackReloadOperationClass : GClass2015
		{
			[NonSerialized]
			public AmmoPackReloadingClass AmmoPackReloadingClass;

			[NonSerialized]
			public MagazineItemClass MagazineItemClass;

			[NonSerialized]
			public bool Bool_1;

			[NonSerialized]
			public int Int_0;

			[NonSerialized]
			public bool Bool_2;

			public bool Boolean_0
			{
				get
				{
					return Bool_2;
				}
				set
				{
					Bool_2 = value;
					if (!Bool_2 && value)
					{
						method_7();
					}
				}
			}

			public AmmoPackReloadOperationClass(FirearmController controller)
				: base(controller)
			{
			}

			public virtual void Start(AmmoPackReloadingClass ammoPack, [CanBeNull] Callback callback)
			{
				Start(callback);
				AmmoPackReloadingClass = ammoPack;
				FirearmsAnimator_0.SetInventory(open: false);
				FirearmsAnimator_0.SetCanReload(canReload: true);
				FirearmsAnimator_0.Reload(b: true);
				FirearmsAnimator_0.SetIsExternalMag(isExternalMag: false);
				AmmoPackReloadingClass.LockItems();
				Weapon_0.Parent.RaiseRemoveEvent(Weapon_0, CommandStatus.Begin, Player_0.InventoryController);
				MagazineItemClass = Weapon_0.GetCurrentMagazine();
			}

			public override void Reset()
			{
				AmmoPackReloadingClass = null;
				MagazineItemClass = null;
				Int_0 = 0;
				Bool_1 = false;
				Boolean_0 = false;
				base.Reset();
			}

			public virtual bool CanReload()
			{
				if (AmmoPackReloadingClass.AmmoCount - Int_0 > 0)
				{
					return MagazineItemClass.Count + Int_0 != MagazineItemClass.MaxCount;
				}
				return false;
			}

			public override void SetTriggerPressed(bool pressed)
			{
				Bool_1 |= pressed && Boolean_0;
			}

			public override void SetInventoryOpened(bool opened)
			{
				FirearmController_0.InventoryOpened = opened;
				Bool_1 = true;
				if (Boolean_0)
				{
					FirearmsAnimator_0.SetInventory(opened);
				}
			}
		}

		public class AmmoPackReloadInternalOneChamberOperationClass : AmmoPackReloadOperationClass
		{
			[CompilerGenerated]
			public class Class1234
			{
				public Player player;

				public Weapon weapon;

				public Action action_0;

				public void method_0()
				{
					player.Skills.WeaponReloadAction.Complete(weapon);
				}
			}

			[NonSerialized]
			public AmmoItemClass AmmoItemClass;

			[NonSerialized]
			public bool Bool_3;

			public AmmoPackReloadInternalOneChamberOperationClass(FirearmController controller)
				: base(controller)
			{
			}

			public override void Start(AmmoPackReloadingClass ammoPack, Callback callback)
			{
				base.Start(ammoPack, callback);
				method_9();
			}

			public void method_9()
			{
				bool flag = Weapon_0.MalfState.State == Weapon.EMalfunctionState.None || Weapon_0.MalfState.State == Weapon.EMalfunctionState.Misfire;
				if (Weapon_0.ChamberAmmoCount == 0 && flag)
				{
					Weapon.EMalfunctionState state = Weapon_0.MalfState.State;
					if (state == Weapon.EMalfunctionState.Misfire)
					{
						Weapon_0.MalfState.ChangeStateSilent(Weapon.EMalfunctionState.None);
					}
					GStruct154<GInterface424> gStruct = AmmoPackReloadingClass.LoadAmmo(Player_0.InventoryController, Player_0.InventoryController, base.Slot_0.CreateItemAddress());
					Weapon_0.MalfState.ChangeStateSilent(state);
					if (gStruct.Error != null)
					{
						UnityEngine.Debug.LogError("ReloadInternalMagOperation::Prepare --- Could not get ammo to load, error: " + gStruct.Error);
						return;
					}
					AmmoItemClass = (AmmoItemClass)gStruct.Value.ResultItem;
					if (gStruct.Value is GClass3411 gClass)
					{
						gClass.From.RaiseRemoveEvent(AmmoItemClass, CommandStatus.Succeed, Player_0.InventoryController);
					}
					if (gStruct.Value is GClass3417)
					{
						((GClass3417)gStruct.Value).From.RaiseRemoveEvent(AmmoItemClass, CommandStatus.Succeed, Player_0.InventoryController);
					}
				}
				if (Weapon_0.MalfState.State == Weapon.EMalfunctionState.Misfire)
				{
					FirearmsAnimator_0.SetLayerWeight(FirearmsAnimator_0.MALFUNCTION_LAYER_INDEX, 0);
				}
			}

			public override void Reset()
			{
				AmmoItemClass = null;
				Bool_3 = false;
				base.Reset();
			}

			public override void OnMagAppeared()
			{
				base.Boolean_0 = true;
			}

			public override void OnAddAmmoInChamber()
			{
				if (Bool_3)
				{
					return;
				}
				Bool_3 = true;
				if (Weapon_0.MalfState.State == Weapon.EMalfunctionState.Misfire && AmmoItemClass == null)
				{
					base.Boolean_0 = true;
					if (!CanReload() || Bool_1)
					{
						SwitchToIdle();
					}
					return;
				}
				if (Weapon_0.MalfState.State == Weapon.EMalfunctionState.Misfire)
				{
					method_2();
				}
				base.Boolean_0 = true;
				WeaponManagerClass.SetRoundIntoWeapon(AmmoItemClass);
				FirearmsAnimator_0.SetAmmoInChamber(Weapon_0.ChamberAmmoCount);
				FirearmsAnimator_0.SetCanReload(CanReload());
				Player_0.ExecuteSkill((Action)delegate
				{
					Player_0.Skills.WeaponChamberAction.Complete(Weapon_0);
				});
				if (!CanReload() || Bool_1)
				{
					SwitchToIdle();
				}
			}

			public override void OnOnOffBoltCatchEvent(bool isCatched)
			{
				FirearmsAnimator_0.SetBoltCatch(isCatched);
			}

			public override void AddAmmoToMag()
			{
				base.Boolean_0 = true;
				Int_0++;
				FirearmsAnimator_0.SetAmmoOnMag(MagazineItemClass.Count + Int_0);
				Player_0.ExecuteSkill((Action)delegate
				{
					Player_0.Skills.RaidLoadedAmmoAction.Complete();
				});
				if (!CanReload() || Bool_1)
				{
					SwitchToIdle();
				}
			}

			public virtual void SwitchToIdle()
			{
				FirearmsAnimator_0.SetCanReload(CanReload() && !Bool_1);
				if (Weapon_0.MalfState.State == Weapon.EMalfunctionState.Misfire)
				{
					FirearmsAnimator_0.SetLayerWeight(FirearmsAnimator_0.MALFUNCTION_LAYER_INDEX, 1);
				}
				method_10();
				Action action_ = Action_0;
				bool bool_ = Bool_0;
				Weapon_0.RaiseRefreshEvent();
				State = EOperationState.Finished;
				GClass2037 gClass = FirearmController_0.InitiateOperation<GClass2037>();
				gClass.Start();
				method_6();
				if (action_ != null)
				{
					gClass.HideWeapon(action_, bool_);
				}
			}

			public void method_10()
			{
				AmmoPackReloadingClass ammoPackReloadingClass = AmmoPackReloadingClass;
				MagazineItemClass magazineItemClass = MagazineItemClass;
				Weapon weapon_ = Weapon_0;
				int int_ = Int_0;
				Player player_ = Player_0;
				ammoPackReloadingClass.UnlockItems();
				CommitReloadWithAmmo(int_, ammoPackReloadingClass, player_, magazineItemClass, weapon_);
				FirearmsAnimator_0.SetAmmoOnMag(magazineItemClass.Count);
				weapon_.Parent.RaiseRemoveEvent(weapon_, CommandStatus.Failed, Player_0.InventoryController);
			}

			public static void CommitReloadWithAmmo(int ammoToLoadIntoMag, AmmoPackReloadingClass ammoPack, Player player, MagazineItemClass magazine, Weapon weapon)
			{
				for (int i = 0; i < ammoToLoadIntoMag; i++)
				{
					GStruct154<GInterface424> gStruct = ammoPack.LoadAmmo(player.InventoryController, player.InventoryController, magazine.Cartridges.CreateItemAddress());
					if (gStruct.Error == null)
					{
						gStruct.Value.RaiseEvents(player.InventoryController, CommandStatus.Begin);
						gStruct.Value.RaiseEvents(player.InventoryController, CommandStatus.Succeed);
					}
					else
					{
						UnityEngine.Debug.LogError("SwitchToIdle: Cannot load ammo. AmmoCount: " + ammoPack.AmmoCount + ", AmmoToLoadIntoMag: " + (ammoToLoadIntoMag - i) + ", Error: " + gStruct.Error);
					}
					player.ExecuteSkill((Action)delegate
					{
						player.Skills.WeaponReloadAction.Complete(weapon);
					});
				}
			}

			public override void HideWeapon(Action onHidden, bool fastDrop, Item nextControllerItem = null)
			{
				base.HideWeapon(onHidden, fastDrop, (Item)null);
				Bool_1 = true;
			}

			public override void FastForward()
			{
				if (State == EOperationState.Finished)
				{
					return;
				}
				Bool_1 = true;
				if (AmmoItemClass != null)
				{
					OnAddAmmoInChamber();
					if (Weapon_0.IsBoltCatch)
					{
						OnOnOffBoltCatchEvent(false);
					}
				}
				if (State != EOperationState.Finished)
				{
					SwitchToIdle();
				}
			}

			[CompilerGenerated]
			public void method_11()
			{
				Player_0.Skills.WeaponChamberAction.Complete(Weapon_0);
			}

			[CompilerGenerated]
			public void method_12()
			{
				Player_0.Skills.RaidLoadedAmmoAction.Complete();
			}
		}

		public class AmmoPackReloadInternalBoltOpenOperationClass : AmmoPackReloadOperationClass
		{
			[CompilerGenerated]
			public class Class1235
			{
				public Player player;

				public Weapon weapon;

				public Action action_0;

				public void method_0()
				{
					player.Skills.WeaponReloadAction.Complete(weapon);
				}

				public void method_1()
				{
					player.Skills.WeaponChamberAction.Complete(weapon);
				}
			}

			[NonSerialized]
			public Item Item_0;

			[NonSerialized]
			public bool Bool_3;

			[NonSerialized]
			public bool Bool_4;

			public AmmoPackReloadInternalBoltOpenOperationClass(FirearmController controller)
				: base(controller)
			{
			}

			public override void Start(AmmoPackReloadingClass ammoPack, Callback callback)
			{
				base.Start(ammoPack, callback);
				method_9();
				if (Weapon_0.MalfState.State == Weapon.EMalfunctionState.Misfire)
				{
					FirearmsAnimator_0.SetLayerWeight(FirearmsAnimator_0.MALFUNCTION_LAYER_INDEX, 0);
				}
			}

			public override void Reset()
			{
				Item_0 = null;
				Bool_3 = false;
				Bool_4 = false;
				base.Reset();
			}

			public void method_9()
			{
				if (Weapon_0.ChamberAmmoCount == 0)
				{
					return;
				}
				Item_0 = base.Slot_0.ContainedItem;
				if (Item_0 == null)
				{
					UnityEngine.Debug.LogError("ReloadInternalMagWithOpenBoltOperation::Prepare --- Could not get from chamber");
					return;
				}
				Weapon.EMalfunctionState state = Weapon_0.MalfState.State;
				if (Weapon_0.MalfState.State == Weapon.EMalfunctionState.Misfire)
				{
					Weapon_0.MalfState.ChangeStateSilent(Weapon.EMalfunctionState.None);
				}
				_ = InteractionsHandlerClass.Remove(Item_0, Player_0.InventoryController).Succeeded;
				if (state == Weapon.EMalfunctionState.Misfire)
				{
					Weapon_0.MalfState.ChangeStateSilent(state);
				}
			}

			public override void RemoveAmmoFromChamber()
			{
				if (!Bool_4)
				{
					Bool_4 = true;
					WeaponManagerClass.RemovePatronInWeapon();
					FirearmsAnimator_0.SetAmmoInChamber(0f);
					WeaponManagerClass.ThrowPatronAsLoot(Item_0, Player_0, "ReloadInternalMagWithOpenBoltOperation.RemoveAmmoFromChamber");
				}
			}

			public override void OnOnOffBoltCatchEvent(bool isCatched)
			{
				FirearmsAnimator_0.SetBoltCatch(isCatched);
			}

			public override void AddAmmoToMag()
			{
				WeaponManagerClass.DestroyPatronInWeapon();
				base.Boolean_0 = true;
				Int_0++;
				FirearmsAnimator_0.SetAmmoOnMag(MagazineItemClass.Count + Int_0);
				Player_0.ExecuteSkill((Action)delegate
				{
					Player_0.Skills.RaidLoadedAmmoAction.Complete();
				});
				FirearmsAnimator_0.SetCanReload(CanReload() && !Bool_1);
			}

			public override void OnAddAmmoInChamber()
			{
				if (!Bool_3)
				{
					Bool_3 = true;
					base.Boolean_0 = true;
					if (Weapon_0.MalfState.State == Weapon.EMalfunctionState.Misfire)
					{
						method_2();
						FirearmsAnimator_0.SetAmmoInChamber(Weapon_0.ChamberAmmoCount);
					}
					SwitchToIdle();
				}
			}

			public virtual void SwitchToIdle()
			{
				AmmoPackReloadingClass.UnlockItems();
				CommitReloadWithAmmo(Int_0, AmmoPackReloadingClass, Player_0, MagazineItemClass, Weapon_0);
				Player_0.InventoryController.RaiseRemoveEvent(new GEventArgs3(Weapon_0, Weapon_0.Parent, CommandStatus.Failed, Player_0.InventoryController));
				WeaponManagerClass.DestroyPatronInWeapon();
				if (base.Slot_0.ContainedItem != null)
				{
					WeaponManagerClass.SetRoundIntoWeapon((AmmoItemClass)base.Slot_0.ContainedItem);
				}
				FirearmsAnimator_0.SetAmmoOnMag(MagazineItemClass.Count);
				FirearmsAnimator_0.SetAmmoInChamber(Weapon_0.ChamberAmmoCount);
				FirearmsAnimator_0.SetCanReload(canReload: false);
				Action action_ = Action_0;
				bool bool_ = Bool_0;
				Weapon_0.RaiseRefreshEvent();
				State = EOperationState.Finished;
				GClass2037 gClass = FirearmController_0.InitiateOperation<GClass2037>();
				gClass.Start();
				method_6();
				if (action_ != null)
				{
					gClass.HideWeapon(action_, bool_);
				}
			}

			public static void CommitReloadWithAmmo(int ammoToLoadIntoMag, AmmoPackReloadingClass ammoPack, Player player, MagazineItemClass magazine, Weapon weapon)
			{
				for (int i = 0; i < ammoToLoadIntoMag; i++)
				{
					GStruct154<GInterface424> gStruct = ammoPack.LoadAmmo(player.InventoryController, player.InventoryController, magazine.Cartridges.CreateItemAddress());
					if (gStruct.Succeeded)
					{
						gStruct.Value.RaiseEvents(player.InventoryController, CommandStatus.Begin);
						gStruct.Value.RaiseEvents(player.InventoryController, CommandStatus.Succeed);
						player.ExecuteSkill((Action)delegate
						{
							player.Skills.WeaponReloadAction.Complete(weapon);
						});
					}
					else
					{
						UnityEngine.Debug.LogError("SwitchToIdle: Cannot load ammo. AmmoCount: " + ammoPack.AmmoCount + ", AmmoToLoadIntoMag: " + (ammoToLoadIntoMag - i) + ", Error: " + gStruct.Error);
					}
				}
				if (ammoToLoadIntoMag <= 0 || weapon.ChamberAmmoCount != 0)
				{
					return;
				}
				if (magazine.IsAmmoCompatible(weapon.Chambers))
				{
					GStruct154<GInterface424> gStruct2 = magazine.Cartridges.PopTo(player.InventoryController, weapon.Chambers[0].CreateItemAddress());
					if (gStruct2.Failed)
					{
						UnityEngine.Debug.LogError("CommitAmmoInChamber pop failed: " + gStruct2.Error);
					}
				}
				player.ExecuteSkill((Action)delegate
				{
					player.Skills.WeaponChamberAction.Complete(weapon);
				});
			}

			public override void HideWeapon(Action onHidden, bool fastDrop, Item nextControllerItem = null)
			{
				base.HideWeapon(onHidden, fastDrop, (Item)null);
				Bool_1 = true;
			}

			public override void OnShowAmmo(bool value)
			{
				if (value)
				{
					AmmoItemClass ammoToReload = AmmoPackReloadingClass.GetAmmoToReload(Int_0);
					if (ammoToReload != null)
					{
						WeaponManagerClass.SetRoundIntoWeapon(ammoToReload);
					}
				}
			}

			public override void FastForward()
			{
				if (State != EOperationState.Finished)
				{
					if (Item_0 != null)
					{
						RemoveAmmoFromChamber();
					}
					OnAddAmmoInChamber();
					if (Weapon_0.IsBoltCatch)
					{
						OnOnOffBoltCatchEvent(false);
					}
					if (State != EOperationState.Finished)
					{
						SwitchToIdle();
					}
				}
			}

			[CompilerGenerated]
			public void method_10()
			{
				Player_0.Skills.RaidLoadedAmmoAction.Complete();
			}
		}

		public class MutliBarrelReloadOperationClass : GClass2015
		{
			[NonSerialized]
			public ReloadMultiBarrelResultClass ReloadMultiBarrelResultClass;

			[NonSerialized]
			public bool Bool_1;

			[NonSerialized]
			public bool Bool_2;

			[NonSerialized]
			public int Int_0;

			public MutliBarrelReloadOperationClass(FirearmController controller)
				: base(controller)
			{
			}

			public virtual void Start(ReloadMultiBarrelResultClass reloadMultiBarrelResult, Callback callback)
			{
				Start(callback);
				ReloadMultiBarrelResultClass = reloadMultiBarrelResult;
				ReloadMultiBarrelResultClass.RaiseEvents(Player_0.InventoryController, CommandStatus.Begin);
				Int_0 = Weapon_0.ShellsInWeaponCount;
				method_9();
				method_3();
				FirearmsAnimator_0.SetCanReload(canReload: true);
				FirearmsAnimator_0.Reload(b: true);
				FirearmsAnimator_0.SetShellsInWeapon(Weapon_0.ShellsInWeaponCount);
				FirearmsAnimator_0.SetAmmoInChamber(Weapon_0.ChamberAmmoCount);
				Player_0.Say(EPhraseTrigger.OnWeaponReload);
			}

			public void method_9()
			{
				int num = 0;
				foreach (GClass2007 item in ReloadMultiBarrelResultClass.ChambersForReloading)
				{
					num++;
					FirearmsAnimator_0.SetChamberIndexForLoadUnloadAmmo(item.ChamberIndex);
				}
				if (num == ReloadMultiBarrelResultClass.ChambersInWeaponTotal)
				{
					FirearmsAnimator_0.SetChamberIndexForLoadUnloadAmmo(num);
				}
			}

			public override void Reset()
			{
				Int_0 = 0;
				Bool_2 = false;
				Bool_1 = false;
				ReloadMultiBarrelResultClass = null;
				base.Reset();
			}

			public override void FastForward()
			{
				if (!Bool_2)
				{
					OnRemoveShellEvent();
					OnShellEjectEvent();
					OnAddAmmoInChamber();
				}
			}

			public override void RemoveAmmoFromChamber()
			{
				FirearmsAnimator_0.SetAmmoInChamber(Weapon_0.ChamberAmmoCount);
			}

			public override void OnRemoveShellEvent()
			{
				for (int i = 0; i < Weapon_0.ShellsInChambers.Length; i++)
				{
					Weapon_0.ShellsInChambers[i] = null;
				}
				WeaponManagerClass.RemoveAllShells();
				FirearmsAnimator_0.SetShellsInWeapon(Weapon_0.ShellsInWeaponCount);
			}

			public override void OnShellEjectEvent()
			{
				FirearmsAnimator_0.SetCanReload(canReload: true);
				IReadOnlyCollection<GClass2007> chambersForReloading = ReloadMultiBarrelResultClass.ChambersForReloading;
				bool discardOldAmmo = ReloadMultiBarrelResultClass.DiscardOldAmmo;
				foreach (GClass2007 item in chambersForReloading)
				{
					if (item.OldAmmoResult == null)
					{
						continue;
					}
					if (discardOldAmmo)
					{
						AmmoItemClass ammoItemClass = (AmmoItemClass)item.OldAmmoResult.Item;
						if (!ammoItemClass.IsUsed)
						{
							WeaponManagerClass.RemovePatronInWeapon(item.ChamberIndex);
							WeaponManagerClass.ThrowPatronAsLoot(ammoItemClass, Player_0, "ReloadMultiBarrelOperation.OnShellEjectEvent");
						}
					}
					else
					{
						WeaponManagerClass.DestroyPatronInWeapon(item.ChamberIndex);
					}
				}
				if (Int_0 > 0)
				{
					WeaponManagerClass.StartSpawnAllShells(Player_0.Velocity * 0.33f);
				}
				FirearmsAnimator_0.SetAmmoInChamber(Weapon_0.ChamberAmmoCount);
			}

			public override void OnAddAmmoInChamber()
			{
				if (!Bool_1)
				{
					Bool_2 = true;
					Bool_1 = true;
					FirearmsAnimator_0.SetAmmoInChamber(Weapon_0.ChamberAmmoCount);
				}
			}

			public override void AddAmmoToMag()
			{
				ReloadMultiBarrelResultClass.RaiseEvents(Player_0.InventoryController, CommandStatus.Succeed);
				State = EOperationState.Finished;
				FirearmsAnimator_0.SetCanReload(canReload: false);
				Action action_ = Action_0;
				bool bool_ = Bool_0;
				Weapon_0.RaiseRefreshEvent();
				GClass2037 gClass = FirearmController_0.InitiateOperation<GClass2037>();
				gClass.Start();
				method_6();
				if (action_ != null)
				{
					gClass.HideWeapon(action_, bool_);
				}
			}

			public override void OnMagAppeared()
			{
				foreach (GClass2007 item in ReloadMultiBarrelResultClass.ChambersForReloading)
				{
					WeaponManagerClass.SetRoundIntoWeapon((AmmoItemClass)item.InsertResult.ResultItem, item.ChamberIndex);
				}
			}

			public override void HideWeapon(Action onHidden, bool fastDrop, Item nextControllerItem = null)
			{
				FastForward();
				FirearmController_0.InitiateOperation<GClass2053>().Start(onHidden, fastDrop, nextControllerItem);
			}
		}

		public class GClass2007
		{
			public readonly int ChamberIndex;

			public readonly GInterface424 OldAmmoResult;

			public readonly GInterface424 InsertResult;

			public GClass2007(int chamberIndex, GInterface424 oldAmmoResult, GInterface424 insertResult)
			{
				ChamberIndex = chamberIndex;
				OldAmmoResult = oldAmmoResult;
				InsertResult = insertResult;
			}

			public void RaiseEvents(TraderControllerClass controller, CommandStatus status)
			{
				OldAmmoResult?.RaiseEvents(controller, status);
				InsertResult.RaiseEvents(controller, status);
			}

			public bool CheckAction()
			{
				GInterface424 oldAmmoResult = OldAmmoResult;
				if (oldAmmoResult != null && oldAmmoResult.Item.CheckAction(null).Succeeded)
				{
					return InsertResult.ResultItem.CheckAction(null).Succeeded;
				}
				return false;
			}

			public void RollBack()
			{
				OldAmmoResult?.RollBack();
				InsertResult.RollBack();
			}
		}

		public class ReloadMultiBarrelResultClass
		{
			public readonly TraderControllerClass ItemController;

			public readonly AmmoPackReloadingClass AmmoPackToLoad;

			public readonly Weapon Weapon;

			public readonly ItemAddress PlaceToPutContainedAmmo;

			public readonly IReadOnlyCollection<GClass2007> ChambersForReloading;

			public readonly int ChambersInWeaponTotal;

			public readonly bool DiscardOldAmmo;

			public ReloadMultiBarrelResultClass(TraderControllerClass itemController, AmmoPackReloadingClass ammoPackToLoad, Weapon weapon, int chambersInWeaponTotal, ItemAddress placeToPutContainedAmmo, IReadOnlyCollection<GClass2007> chambersForReloading, bool discardOldAmmo)
			{
				ItemController = itemController;
				AmmoPackToLoad = ammoPackToLoad;
				Weapon = weapon;
				ChambersInWeaponTotal = chambersInWeaponTotal;
				PlaceToPutContainedAmmo = placeToPutContainedAmmo;
				ChambersForReloading = chambersForReloading;
				DiscardOldAmmo = discardOldAmmo;
			}

			public static GStruct156<ReloadMultiBarrelResultClass> Run(IIdGenerator idGenerator, TraderControllerClass itemController, Weapon weapon, AmmoPackReloadingClass ammoPack, ItemAddress placeToPutContainedAmmo)
			{
				int num = weapon.Chambers.Length;
				if (num == 0)
				{
					return default(GStruct156<ReloadMultiBarrelResultClass>).Error;
				}
				bool flag = placeToPutContainedAmmo == null;
				List<GClass2007> list = new List<GClass2007>();
				int num2 = 0;
				for (int i = 0; i < num; i++)
				{
					Slot slot = weapon.Chambers[i];
					AmmoItemClass ammoItemClass = slot.ContainedItem as AmmoItemClass;
					if (ammoItemClass != null && !ammoItemClass.IsUsed)
					{
						continue;
					}
					AmmoItemClass ammoToReload = ammoPack.GetAmmoToReload(num2);
					if (ammoToReload == null)
					{
						break;
					}
					int stackObjectsCount = ammoToReload.StackObjectsCount;
					ItemAddress to = slot.CreateItemAddress();
					GStruct154<GInterface424> gStruct = default(GStruct154<GInterface424>);
					if (ammoItemClass != null)
					{
						gStruct = (flag ? GClass1617.Cast<GClass3410, GInterface424>(InteractionsHandlerClass.Remove(ammoItemClass, itemController)) : GClass1617.Cast<GClass3411, GInterface424>(InteractionsHandlerClass.Move(ammoItemClass, placeToPutContainedAmmo, itemController)));
						if (gStruct.Failed)
						{
							continue;
						}
					}
					GStruct154<GInterface424> gStruct2 = InteractionsHandlerClass.ApplySingleItemToAddress(ammoToReload, idGenerator, itemController, to);
					if (gStruct2.Failed)
					{
						gStruct.Value?.RollBack();
						continue;
					}
					GClass2007 item = new GClass2007(i, gStruct.Value, gStruct2.Value);
					list.Add(item);
					if (stackObjectsCount <= 1)
					{
						num2++;
					}
				}
				return new ReloadMultiBarrelResultClass(itemController, ammoPack, weapon, num, placeToPutContainedAmmo, list, flag);
			}

			public void RollBack()
			{
				foreach (GClass2007 item in ChambersForReloading)
				{
					item.RollBack();
				}
			}

			public void RaiseEvents(TraderControllerClass controller, CommandStatus status)
			{
				if (ChambersForReloading.Count == 0)
				{
					return;
				}
				Weapon.Parent.RaiseRemoveEvent(Weapon, (status != CommandStatus.Begin) ? CommandStatus.Failed : CommandStatus.Begin, controller);
				foreach (GClass2007 item in ChambersForReloading)
				{
					item.RaiseEvents(controller, status);
				}
			}

			public bool CanExecute(TraderControllerClass itemController)
			{
				if (Weapon.CheckAction(null).Failed)
				{
					return false;
				}
				foreach (GClass2007 item in ChambersForReloading)
				{
					if (item.CheckAction())
					{
						return true;
					}
				}
				return false;
			}
		}

		public class SingleBarrelReloadOperationClass : GClass2015
		{
			[NonSerialized]
			public ReloadSingleBarrelResultClass ReloadSingleBarrelResultClass;

			public SingleBarrelReloadOperationClass(FirearmController controller)
				: base(controller)
			{
			}

			public virtual void Start(ReloadSingleBarrelResultClass reloadSingleBarrelResult, Callback callback)
			{
				Start(callback);
				ReloadSingleBarrelResultClass = reloadSingleBarrelResult;
				ReloadSingleBarrelResultClass.RaiseEvents(Player_0.InventoryController, CommandStatus.Begin);
				FirearmsAnimator_0.SetShellsInWeapon(Weapon_0.ShellsInWeaponCount);
				FirearmsAnimator_0.SetAmmoInChamber(reloadSingleBarrelResult.HasOldAmmoInChamber ? Weapon_0.ChamberAmmoCount : 0);
				FirearmsAnimator_0.SetCanReload(canReload: true);
				FirearmsAnimator_0.Reload(b: true);
				if (reloadSingleBarrelResult.HasOldAmmoInChamber)
				{
					for (int i = 0; i < Weapon_0.ShellsInChambers.Length; i++)
					{
						Weapon_0.ShellsInChambers[i] = null;
					}
				}
				Player_0.Say(EPhraseTrigger.OnWeaponReload);
			}

			public override void Reset()
			{
				ReloadSingleBarrelResultClass = null;
				base.Reset();
			}

			public override void RemoveAmmoFromChamber()
			{
				FirearmsAnimator_0.SetAmmoInChamber(Weapon_0.ChamberAmmoCount);
			}

			public override void OnRemoveShellEvent()
			{
				for (int i = 0; i < Weapon_0.ShellsInChambers.Length; i++)
				{
					Weapon_0.ShellsInChambers[i] = null;
				}
				WeaponManagerClass.RemoveAllShells();
				FirearmsAnimator_0.SetShellsInWeapon(Weapon_0.ShellsInWeaponCount);
			}

			public override void OnShellEjectEvent()
			{
				FirearmsAnimator_0.SetCanReload(canReload: true);
				if (ReloadSingleBarrelResultClass.HasOldAmmoInChamber)
				{
					if (ReloadSingleBarrelResultClass.DiscardOldAmmoToInventory)
					{
						WeaponManagerClass.DestroyPatronInWeapon();
					}
					else
					{
						AmmoItemClass ammoItemClass = (AmmoItemClass)ReloadSingleBarrelResultClass.OldAmmoResult.Item;
						if (!ammoItemClass.IsUsed)
						{
							WeaponManagerClass.RemovePatronInWeapon();
							WeaponManagerClass.ThrowPatronAsLoot(ammoItemClass, Player_0, "ReloadSingleBarrelOperation.OnShellEjectEvent");
						}
					}
				}
				else if (Weapon_0.HasShellsInChamberBarrelOnlyWeapon)
				{
					WeaponManagerClass.StartSpawnShell(Player_0.Velocity);
				}
				FirearmsAnimator_0.SetAmmoInChamber(0f);
			}

			public override void OnAddAmmoInChamber()
			{
				FirearmsAnimator_0.SetAmmoInChamber(Weapon_0.ChamberAmmoCount);
				ReloadSingleBarrelResultClass.RaiseEvents(Player_0.InventoryController, CommandStatus.Succeed);
				State = EOperationState.Finished;
				FirearmsAnimator_0.SetCanReload(canReload: false);
				Action action_ = Action_0;
				bool bool_ = Bool_0;
				Weapon_0.RaiseRefreshEvent();
				FirearmsAnimator_0.SetInventory(FirearmController_0.InventoryOpened);
				GClass2037 gClass = FirearmController_0.InitiateOperation<GClass2037>();
				gClass.Start();
				method_6();
				if (action_ != null)
				{
					gClass.HideWeapon(action_, bool_);
				}
			}

			public override void OnMagAppeared()
			{
				WeaponManagerClass.SetRoundIntoWeapon((AmmoItemClass)ReloadSingleBarrelResultClass.InsertNewAmmoResult.ResultItem);
			}

			public override void FastForward()
			{
				if (State != EOperationState.Finished)
				{
					OnRemoveShellEvent();
					OnShellEjectEvent();
					OnAddAmmoInChamber();
				}
			}

			public override void HideWeapon(Action onHidden, bool fastDrop, Item nextControllerItem = null)
			{
				base.HideWeapon(onHidden, fastDrop, nextControllerItem);
				FastForward();
			}
		}

		public class ReloadSingleBarrelResultClass
		{
			[CanBeNull]
			public readonly GInterface424 OldAmmoResult;

			[NotNull]
			public readonly GInterface424 InsertNewAmmoResult;

			public readonly ItemAddress PlaceToPutContainedAmmoMagazine;

			[NonSerialized]
			public Weapon Weapon_0;

			[NonSerialized]
			public AmmoItemClass AmmoItemClass;

			[NonSerialized]
			[CompilerGenerated]
			public MongoID MongoID_0;

			[NonSerialized]
			[CompilerGenerated]
			public MongoID? Nullable_0;

			public bool DiscardOldAmmoToInventory => PlaceToPutContainedAmmoMagazine != null;

			public bool HasOldAmmoInChamber => OldAmmoResult != null;

			public MongoID AmmoToLoadTemplateId
			{
				[CompilerGenerated]
				get
				{
					return MongoID_0;
				}
				[CompilerGenerated]
				set
				{
					MongoID_0 = value;
				}
			}

			public MongoID? AmmoToUnloadTemplateId
			{
				[CompilerGenerated]
				get
				{
					return Nullable_0;
				}
				[CompilerGenerated]
				set
				{
					Nullable_0 = value;
				}
			}

			public ReloadSingleBarrelResultClass(AmmoItemClass ammoToLoad, [CanBeNull] GInterface424 oldAmmoResult, [NotNull] GInterface424 insertNewAmmoResult, Weapon weapon, ItemAddress placeToPutContainedAmmoMagazine)
			{
				AmmoItemClass = ammoToLoad;
				Weapon_0 = weapon;
				PlaceToPutContainedAmmoMagazine = placeToPutContainedAmmoMagazine;
				OldAmmoResult = oldAmmoResult;
				InsertNewAmmoResult = insertNewAmmoResult;
				AmmoToLoadTemplateId = AmmoItemClass.TemplateId;
				AmmoToUnloadTemplateId = OldAmmoResult?.ResultItem.TemplateId;
			}

			public static GStruct156<ReloadSingleBarrelResultClass> Run(IIdGenerator idGenerator, TraderControllerClass itemController, Weapon weapon, AmmoItemClass ammo, ItemAddress placeToPutContainedAmmoMagazine)
			{
				Slot obj = weapon.Chambers[0];
				AmmoItemClass ammoItemClass = obj.ContainedItem as AmmoItemClass;
				ItemAddress to = obj.CreateItemAddress();
				GStruct154<GInterface424> gStruct = default(GStruct154<GInterface424>);
				if (ammoItemClass != null)
				{
					gStruct = ((placeToPutContainedAmmoMagazine == null) ? GClass1617.Cast<GClass3410, GInterface424>(InteractionsHandlerClass.Remove(ammoItemClass, itemController)) : GClass1617.Cast<GClass3411, GInterface424>(InteractionsHandlerClass.Move(ammoItemClass, placeToPutContainedAmmoMagazine, itemController)));
					if (gStruct.Failed)
					{
						return gStruct.Error;
					}
				}
				GStruct154<GInterface424> gStruct2 = InteractionsHandlerClass.ApplySingleItemToAddress(ammo, idGenerator, itemController, to);
				if (gStruct2.Failed)
				{
					gStruct.Value?.RollBack();
					return gStruct2.Error;
				}
				return new ReloadSingleBarrelResultClass(ammo, gStruct.Value, gStruct2.Value, weapon, placeToPutContainedAmmoMagazine);
			}

			public void RollBack()
			{
				OldAmmoResult?.RollBack();
				InsertNewAmmoResult.RollBack();
			}

			public void RaiseEvents(TraderControllerClass controller, CommandStatus status)
			{
				Weapon_0.Parent.RaiseRemoveEvent(Weapon_0, (status != CommandStatus.Begin) ? CommandStatus.Failed : CommandStatus.Begin, controller);
				OldAmmoResult?.RaiseEvents(controller, status);
				InsertNewAmmoResult.RaiseEvents(controller, status);
			}

			public bool CanExecute(TraderControllerClass itemController)
			{
				if (Weapon_0.CheckAction(null).Failed)
				{
					return false;
				}
				if (OldAmmoResult != null && OldAmmoResult.Item.CheckAction(null).Failed)
				{
					return false;
				}
				return AmmoItemClass.CheckAction(null).Succeeded;
			}
		}

		public class GClass2052 : GClass2013
		{
			[NonSerialized]
			public Slot Slot_0;

			[NonSerialized]
			public Callback Callback_0;

			[NonSerialized]
			public bool Bool_0;

			public GClass2052(FirearmController controller)
				: base(controller)
			{
			}

			public void Start(Item item, Slot slot, Callback callback)
			{
				Slot_0 = slot;
				Callback_0 = callback;
				Start();
				FirearmsAnimator_0.SetupMod(modSet: true);
				FirearmController_0.SetAim(value: false);
				FirearmsAnimator_0.SetFire(fire: false);
				Player_0.MovementContext.SetBlindFire(0);
			}

			public override void Reset()
			{
				Slot_0 = null;
				Callback_0 = null;
				Bool_0 = false;
				base.Reset();
			}

			public override void FastForward()
			{
				if (State != EOperationState.Finished)
				{
					OnModChanged();
				}
			}

			public override void SetAiming(bool isAiming)
			{
				if (!isAiming)
				{
					FirearmController_0.IsAiming = false;
				}
			}

			public override void OnModChanged()
			{
				if (!Bool_0)
				{
					Bool_0 = true;
					WeaponManagerClass.RemoveMod(Slot_0);
					FirearmsAnimator_0.SetupMod(modSet: false);
					State = EOperationState.Finished;
					FirearmController_0.InitiateOperation<GClass2037>().Start();
					method_5();
					Callback_0.Succeed();
					WeaponManagerClass.ModFinallyRemoved(Slot_0);
					Player_0.BodyAnimatorCommon.SetFloat(PlayerAnimator.WEAPON_SIZE_MODIFIER_PARAM_HASH, Weapon_0.CalculateCellSize().X);
					Player_0.UpdateFirstPersonGrip(GripPose.EGripType.Common, FirearmController_0.HandsHierarchy);
					FirearmController_0.ClearModAudioController(Slot_0);
					FirearmController_0.WeaponModified();
				}
			}

			public void method_5()
			{
				if (Slot_0.ContainedItem is LauncherItemClass)
				{
					FirearmController_0.method_8();
				}
			}

			public override void SetInventoryOpened(bool opened)
			{
				FirearmController_0.InventoryOpened = opened;
				FirearmsAnimator_0.SetInventory(opened);
			}

			public override bool CanChangeLightState(FirearmLightStateStruct[] lightsStates)
			{
				return false;
			}
		}

		public class GClass2053 : GClass2013
		{
			[NonSerialized]
			public Action Action_0;

			public GClass2053(FirearmController controller)
				: base(controller)
			{
			}

			public void Start(Action onHidden, bool fastDrop = false, Item nextControllerItem = null)
			{
				Action_0 = onHidden;
				if (Player_0.UsedSimplifiedSkeleton)
				{
					Start();
					FastForward();
					return;
				}
				Weapon_0.IsUnderBarrelDeviceActive = false;
				if (FirearmController_0.CheckForFastWeaponSwitch(nextControllerItem))
				{
					Player_0.Skills.WeaponSkills.TryGetValue(FirearmController_0.Item.GetType(), out var value);
					Player_0.Physical.OnWeaponSwitchFast(value?.Level ?? 0);
					FirearmsAnimator_0.SetSpeedParameters(1f, FirearmController_0.GetWeaponDrawSpeedMultiplier(FirearmController_0.Item, useFastDropAnimationSpeed: true));
					Player_0.QuickdrawWeaponFast = true;
					Player_0.QuickdrawTime = GClass1891.PastTime;
					Player_0.OnStartQuickdrawPistol?.Invoke();
				}
				Start();
				FirearmsAnimator_0.SetActiveParam(active: false);
				FirearmsAnimator_0.SetFastHide(fastDrop);
				Player_0.BodyAnimatorCommon.SetFloat(PlayerAnimator.RELOAD_FLOAT_PARAM_HASH, 1f);
			}

			public override void SetLeftStanceAnimOnStartOperation()
			{
				Player_0.MovementContext.LeftStanceController.DisableLeftStanceAnimFromHandsAction();
			}

			public override void Reset()
			{
				Action_0 = null;
				base.Reset();
			}

			public override void HideWeaponComplete()
			{
				State = EOperationState.Finished;
				Action_0?.Invoke();
			}

			public override void HideWeapon(Action onHidden, bool fastDrop, Item nextControllerItem = null)
			{
				Action_0 = (Action)Delegate.Combine(Action_0, onHidden);
			}

			public override void FastForward()
			{
				if (State != EOperationState.Finished)
				{
					HideWeaponComplete();
				}
			}

			public override void OnDropWeapon()
			{
				HideWeaponComplete();
			}

			public override bool CanChangeLightState(FirearmLightStateStruct[] lightsStates)
			{
				return false;
			}

			public override void BlindFire(int b)
			{
				base.BlindFire(0);
			}
		}

		public class GClass2010
		{
			public readonly TraderControllerClass ItemController;

			public readonly Weapon Weapon;

			public readonly bool AmmoCompatible;

			public readonly GInterface424 PopNewAmmoResult;

			public GClass2010(TraderControllerClass itemController, GInterface424 popNewAmmoResult, Weapon weapon, bool ammoCompatible)
			{
				AmmoCompatible = ammoCompatible;
				ItemController = itemController;
				Weapon = weapon;
				PopNewAmmoResult = popNewAmmoResult;
			}

			public void RollBack()
			{
				PopNewAmmoResult?.RollBack();
			}

			public void RaiseEvents(TraderControllerClass controller, CommandStatus status)
			{
				Weapon.Parent.RaiseRemoveEvent(Weapon, (status != CommandStatus.Begin) ? CommandStatus.Failed : CommandStatus.Begin, controller);
				PopNewAmmoResult?.RaiseEvents(controller, status);
			}

			public static GStruct156<GClass2010> Run(TraderControllerClass itemController, Weapon weapon)
			{
				Slot slot = (weapon.HasChambers ? weapon.Chambers[0] : null);
				MagazineItemClass currentMagazine = weapon.GetCurrentMagazine();
				bool flag = currentMagazine.IsAmmoCompatible(weapon.Chambers);
				if (slot != null && slot.ContainedItem == null && flag)
				{
					GStruct154<GInterface424> gStruct = currentMagazine.Cartridges.PopTo(itemController, slot.CreateItemAddress());
					if (gStruct.Failed)
					{
						return gStruct.Error;
					}
					return new GClass2010(itemController, gStruct.Value, weapon, flag);
				}
				return default(GStruct156<GClass3411>).Error;
			}

			public bool CanExecute(TraderControllerClass itemController)
			{
				return Weapon.CheckAction(null).Succeeded;
			}
		}

		public class FixMalfunctionOperationClass : GClass2046
		{
			[NonSerialized]
			public bool Bool_0;

			[NonSerialized]
			public bool Bool_1;

			[NonSerialized]
			public Weapon.EMalfunctionState EmalfunctionState_0;

			public FixMalfunctionOperationClass(FirearmController controller)
				: base(controller)
			{
			}

			public override void Start()
			{
				base.Start();
				Player_0.StopBlindFire();
				FirearmsAnimator_0.MalfunctionRepair(val: true);
				FirearmsAnimator_0.SetLayerWeight(FirearmsAnimator_0.MALFUNCTION_LAYER_INDEX, 0);
				FirearmsAnimator_0.Malfunction((int)Weapon_0.MalfState.State);
				Player_0.BodyAnimatorCommon.SetFloat(PlayerAnimator.RELOAD_FLOAT_PARAM_HASH, 1f);
				float num = 1f;
				float fixSpeed = FirearmController_0.gclass2250_0.FixSpeed;
				bool flag = FirearmController_0._player.MovementContext.PhysicalConditionIs(EPhysicalCondition.LeftArmDamaged);
				bool flag2 = FirearmController_0._player.MovementContext.PhysicalConditionIs(EPhysicalCondition.RightArmDamaged);
				BackendConfigSettingsClass.GClass1738 malfunction = Singleton<BackendConfigSettingsClass>.Instance.Malfunction;
				float num2 = ((Weapon_0.MalfState.State == Weapon.EMalfunctionState.HardSlide) ? malfunction.MalfRepairHardSlideMult : 1f);
				if (flag && flag2)
				{
					num = malfunction.MalfRepairTwoHandsBrokenMult;
				}
				else if (flag || flag2)
				{
					num = malfunction.MalfRepairOneHandBrokenMult;
				}
				FirearmsAnimator_0.SetMalfRepairSpeed(fixSpeed * num * num2);
				Player_0.MovementContext.PlayerAnimator.method_1(fixSpeed * num * num2);
				Player_0.ExecuteSkill((Action)delegate
				{
					Player_0.Skills.WeaponFixAction.Complete();
				});
				Player_0.InventoryController.RaiseEvent(new GEventArgs5(Weapon_0, CommandStatus.Begin, Player_0.InventoryController));
				EmalfunctionState_0 = Weapon_0.MalfState.State;
				Weapon_0.MalfState.Repair();
			}

			public override void FastForward()
			{
				OnAddAmmoInChamber();
				OnShellEjectEvent();
				OnMalfunctionOffEvent();
			}

			public override void SetTriggerPressed(bool pressed)
			{
				FirearmsAnimator_0.SetFire(pressed);
			}

			public override void Reset()
			{
				base.Reset();
				Bool_0 = false;
				Bool_1 = false;
			}

			public override void OnOnOffBoltCatchEvent(bool isCatched)
			{
				if (!isCatched)
				{
					UnityEngine.Debug.LogError($"OnOnOffBoltCatchEvent error: isCatched {isCatched}, must be true!");
				}
				if (!isCatched || Weapon_0.ChamberAmmoCount != 1)
				{
					FirearmsAnimator_0.SetBoltCatch(isCatched);
				}
			}

			public override void OnIdleStartEvent()
			{
			}

			public override void RemoveAmmoFromChamber()
			{
				FirearmsAnimator_0.SetAmmoInChamber(Weapon_0.ChamberAmmoCount);
				if (EmalfunctionState_0 == Weapon.EMalfunctionState.Jam)
				{
					WeaponManagerClass.SetupPatronInWeaponForJam();
					return;
				}
				if (EmalfunctionState_0 == Weapon.EMalfunctionState.Feed)
				{
					WeaponManagerClass.RemoveShellInWeapon();
				}
				WeaponManagerClass.CreatePatronInShellPort(AmmoItemClass);
			}

			public override void OnAddAmmoInChamber()
			{
				bool flag2;
				bool flag = (flag2 = Weapon_0.GetCurrentMagazine() != null) && Weapon_0.GetCurrentMagazine().Count > 0;
				if (flag2 && Weapon_0.GetCurrentMagazine().IsAmmoCompatible(Weapon_0.Chambers) && flag2 && flag && Weapon_0.HasChambers && Weapon_0.Chambers[0].ContainedItem == null)
				{
					GStruct156<GClass2010> gStruct = GClass2010.Run(FirearmController_0._player.InventoryController, FirearmController_0.Item);
					if (gStruct.Error != null)
					{
						UnityEngine.Debug.LogError("Failed move ammo to chamber: " + gStruct.Error);
					}
					if (gStruct.Value.PopNewAmmoResult == null)
					{
						Bool_0 = true;
					}
					else
					{
						AmmoItemClass_1 = gStruct.Value.PopNewAmmoResult.ResultItem as AmmoItemClass;
					}
				}
				if (!Bool_0)
				{
					Bool_0 = true;
					FirearmsAnimator_0.SetAmmoInChamber(Weapon_0.ChamberAmmoCount);
					FirearmsAnimator_0.SetAmmoOnMag(Weapon_0.GetCurrentMagazineCount());
					if (EmalfunctionState_0 == Weapon.EMalfunctionState.Feed)
					{
						WeaponManagerClass.RemoveShellInWeapon();
					}
					if (AmmoItemClass_1 != null)
					{
						WeaponManagerClass.SetRoundIntoWeapon(AmmoItemClass_1);
					}
					FirearmsAnimator_0.SetFire(FirearmController_0.IsTriggerPressed);
					FirearmsAnimator_0.SetAmmoInChamber(Weapon_0.ChamberAmmoCount);
				}
			}

			public override void OnMalfunctionOffEvent()
			{
				FirearmsAnimator_0.MalfunctionRepair(val: false);
				FirearmsAnimator_0.Malfunction((int)Weapon_0.MalfState.State);
				FirearmsAnimator_0.MisfireSlideUnknown(val: false);
				FirearmsAnimator_0.SetAmmoInChamber(Weapon_0.ChamberAmmoCount);
				FirearmsAnimator_0.SetAmmoOnMag(Weapon_0.GetCurrentMagazineCount());
				Weapon_0.MalfState.AmmoToFire = null;
				Weapon_0.MalfState.AmmoWillBeLoadedToChamber = null;
				Weapon_0.MalfState.MalfunctionedAmmo = null;
				Player_0.InventoryController.CallMalfunctionRepaired(Weapon_0);
				Player_0.InventoryController.RaiseEvent(new GEventArgs5(Weapon_0, CommandStatus.Succeed, Player_0.InventoryController));
				if (Weapon_0.HasChambers && Weapon_0.ChamberAmmoCount == 0)
				{
					FirearmController_0.weaponManagerClass.DestroyPatronInWeapon();
				}
				method_5();
			}

			public override bool CheckChamber()
			{
				return false;
			}

			public override void OnShellEjectEvent()
			{
				if (!Bool_1)
				{
					Bool_1 = true;
					switch (EmalfunctionState_0)
					{
					case Weapon.EMalfunctionState.Jam:
						WeaponManagerClass.SpawnShellAfterJam();
						break;
					case Weapon.EMalfunctionState.Misfire:
					case Weapon.EMalfunctionState.HardSlide:
					case Weapon.EMalfunctionState.SoftSlide:
						WeaponManagerClass.StartSpawnMisfiredCartridge(Player_0.Velocity);
						break;
					case Weapon.EMalfunctionState.Feed:
						WeaponManagerClass.ThrowPatronAsLoot(Weapon_0.MalfState.MalfunctionedAmmo, Player_0, "RepairMalfunction.OnShellEjectEvent");
						break;
					}
				}
			}

			[CompilerGenerated]
			public void method_6()
			{
				Player_0.Skills.WeaponFixAction.Complete();
			}
		}

		public class GClass2054 : GClass2013
		{
			[NonSerialized]
			public CylinderMagazineItemClass CylinderMagazineItemClass;

			[NonSerialized]
			public bool Bool_0;

			[NonSerialized]
			public Callback Callback_0;

			[NonSerialized]
			public bool Bool_1;

			public GClass2054(FirearmController controller)
				: base(controller)
			{
			}

			public virtual void Start(Callback finishCallback, bool rollToZeroCamora)
			{
				Callback_0 = finishCallback;
				CylinderMagazineItemClass = Weapon_0.GetCurrentMagazine() as CylinderMagazineItemClass;
				Bool_1 = rollToZeroCamora;
				Start();
				FirearmController_0.IsAiming = false;
				if (rollToZeroCamora)
				{
					FirearmsAnimator_0.RollToZeroCamora(roll: true);
				}
				else
				{
					FirearmsAnimator_0.SetRollCylinder(roll: true);
				}
				Player_0.BodyAnimatorCommon.SetFloat(PlayerAnimator.RELOAD_FLOAT_PARAM_HASH, 1f);
			}

			public override void Reset()
			{
				base.Reset();
				Bool_0 = false;
				CylinderMagazineItemClass = null;
			}

			public override void FastForward()
			{
				if (State != EOperationState.Finished)
				{
					OnMagPuttedToRig();
				}
			}

			public override void OnMagPuttedToRig()
			{
				if (!Bool_0)
				{
					Bool_0 = true;
					State = EOperationState.Finished;
					FirearmController_0.InitiateOperation<GClass2037>().Start();
					CylinderMagazineItemClass.IncrementCamoraIndex(Bool_1);
					FirearmsAnimator_0.SetCamoraIndex(CylinderMagazineItemClass.CurrentCamoraIndex);
					FirearmsAnimator_0.SetRollCylinder(roll: false);
					FirearmsAnimator_0.RollToZeroCamora(roll: false);
					Callback_0?.Succeed();
				}
			}

			public override void SetInventoryOpened(bool opened)
			{
				FirearmController_0.InventoryOpened = opened;
				FirearmsAnimator_0.SetInventory(opened);
			}
		}

		public class GClass2027 : GClass2026
		{
			[NonSerialized]
			public const float Float_0 = 0.35f;

			[NonSerialized]
			public float Float_1;

			[NonSerialized]
			public bool Bool_1;

			public GClass2027(FirearmController controller)
				: base(controller)
			{
			}

			public override void Start(Item item, Callback callback)
			{
				Float_1 = 0f;
				Bool_1 = false;
				base.Start(item, callback);
			}

			public override void FastForward()
			{
				if (!Bool_1)
				{
					Bool_1 = true;
					OnBackpackDropEvent();
				}
			}

			public override void Update(float deltaTime)
			{
				base.Update(deltaTime);
				if (!Bool_1 && Float_1 > 0.35f)
				{
					Bool_1 = true;
					OnBackpackDropEvent();
				}
				Float_1 += deltaTime;
			}
		}

		public class GClass2055 : GClass2013
		{
			[NonSerialized]
			public Action Action_0;

			[NonSerialized]
			public Action Action_1;

			[NonSerialized]
			public bool Bool_0;

			[NonSerialized]
			public AmmoItemClass AmmoItemClass;

			[NonSerialized]
			public MagazineItemClass MagazineItemClass;

			[NonSerialized]
			public bool Bool_1;

			public GClass2055(FirearmController controller)
				: base(controller)
			{
				controller._player.Logger.LogInfo("SpawnOperation");
			}

			public virtual void Start(Action onWeaponAppear)
			{
				FirearmController_0._player.Logger.LogInfo("SpawnOperation.Start()");
				Action_0 = onWeaponAppear;
				Start();
				FirearmsAnimator_0.SetActiveParam(active: true);
				FirearmsAnimator_0.SetLayerWeight(FirearmsAnimator_0.LACTIONS_LAYER_INDEX, 0);
				Player_0.BodyAnimatorCommon.SetFloat(PlayerAnimator.WEAPON_SIZE_MODIFIER_PARAM_HASH, Weapon_0.CalculateCellSize().X);
				int chamberAmmoCount = Weapon_0.ChamberAmmoCount;
				int currentMagazineCount = Weapon_0.GetCurrentMagazineCount();
				MagazineItemClass = Weapon_0.GetCurrentMagazine();
				FirearmController_0.AmmoInChamberOnSpawn = chamberAmmoCount;
				if (Weapon_0.HasChambers)
				{
					FirearmsAnimator_0.SetAmmoInChamber(chamberAmmoCount);
				}
				else
				{
					FirearmsAnimator_0.SetHammerArmed(Weapon_0.Armed);
				}
				if (Weapon_0.GetCurrentMagazine() is CylinderMagazineItemClass cylinderMagazineItemClass)
				{
					bool hammerArmed = !Weapon_0.CylinderHammerClosed;
					FirearmsAnimator_0.SetHammerArmed(hammerArmed);
					FirearmsAnimator_0.SetCamoraIndex(cylinderMagazineItemClass.CurrentCamoraIndex);
					for (int i = 0; i < cylinderMagazineItemClass.Count; i++)
					{
						if (cylinderMagazineItemClass.Camoras[i].ContainedItem != null)
						{
							Weapon_0.ShellsInChambers[i] = null;
							WeaponManagerClass.RemoveShellInWeapon(i);
						}
					}
				}
				if (Weapon_0.IsMultiBarrel)
				{
					for (int j = 0; j < Weapon_0.Chambers.Length; j++)
					{
						if (Weapon_0.Chambers[j].ContainedItem != null)
						{
							Weapon_0.ShellsInChambers[j] = null;
							WeaponManagerClass.RemoveShellInWeapon(j);
						}
					}
				}
				FirearmsAnimator_0.SetAmmoOnMag(currentMagazineCount);
				Player_0.BodyAnimatorCommon.SetFloat(PlayerAnimator.RELOAD_FLOAT_PARAM_HASH, 1f);
				Player_0.Skills.OnWeaponDraw(Weapon_0);
				bool flag = (Bool_1 = MagazineItemClass == null || MagazineItemClass.IsAmmoCompatible(Weapon_0.Chambers));
				FirearmsAnimator_0.SetAmmoCompatible(flag);
				if (Bool_1 && MagazineItemClass != null && MagazineItemClass.Count > 0 && FirearmController_0.Item.Chambers.Length != 0 && Weapon_0.MalfState.State == Weapon.EMalfunctionState.Misfire)
				{
					FirearmsAnimator_0.SetLayerWeight(FirearmsAnimator_0.MALFUNCTION_LAYER_INDEX, 0);
				}
				if (MagazineItemClass != null && chamberAmmoCount == 0 && currentMagazineCount > 0 && flag && FirearmController_0.Item.Chambers.Length != 0)
				{
					Weapon.EMalfunctionState state = FirearmController_0.Item.MalfState.State;
					if (state == Weapon.EMalfunctionState.Misfire)
					{
						FirearmController_0.Item.MalfState.ChangeStateSilent(Weapon.EMalfunctionState.None);
					}
					GStruct154<GInterface424> gStruct = MagazineItemClass.Cartridges.PopTo(FirearmController_0._player.InventoryController, FirearmController_0.Item.Chambers[0].CreateItemAddress());
					FirearmController_0.Item.MalfState.ChangeStateSilent(state);
					if (gStruct.Value != null)
					{
						WeaponManagerClass.RemoveAllShells();
						Player_0.UpdatePhones();
						AmmoItemClass = (AmmoItemClass)gStruct.Value.ResultItem;
					}
				}
			}

			public override void SetLeftStanceAnimOnStartOperation()
			{
				if (!Weapon_0.IsStationaryWeapon)
				{
					if (Weapon_0.BlockLeftStance)
					{
						Player_0.MovementContext.LeftStanceController.SetLeftStanceForce(value: false);
					}
					else
					{
						Player_0.MovementContext.LeftStanceController.SetAnimatorLeftStanceToCacheFromHandsAction();
					}
				}
			}

			public override void Reset()
			{
				base.Reset();
				Action_0 = null;
				Action_1 = null;
				MagazineItemClass = null;
				AmmoItemClass = null;
				Bool_1 = false;
			}

			public override void OnAddAmmoInChamber()
			{
				if (AmmoItemClass != null)
				{
					FirearmsAnimator_0.SetAmmoOnMag(MagazineItemClass.Count);
					FirearmsAnimator_0.SetAmmoInChamber(Weapon_0.ChamberAmmoCount);
					if (Weapon_0.HasChambers)
					{
						WeaponManagerClass.SetRoundIntoWeapon(AmmoItemClass);
					}
				}
			}

			public override void WeaponAppeared()
			{
				if (Bool_1 && MagazineItemClass != null && FirearmController_0.Item.Chambers.Length != 0 && Weapon_0.MalfState.State == Weapon.EMalfunctionState.Misfire)
				{
					method_2();
				}
				if (Weapon_0.MalfState.State != Weapon.EMalfunctionState.None)
				{
					Player_0.NeedRepairMalfPhraseSituation(Weapon_0.MalfState.State, Weapon_0.MalfState.IsKnownMalfType(FirearmController_0._player.ProfileId));
				}
				FirearmController_0.SetupProp();
				FirearmController_0._player.Logger.LogInfo("SpawnOperation.WeaponAppeared()");
				State = EOperationState.Finished;
				GClass2037 gClass = FirearmController_0.InitiateOperation<GClass2037>();
				gClass.Start();
				Action_0();
				Action<FirearmController> firearmController_ = FirearmController_0.action_1;
				FirearmController_0.action_1 = null;
				firearmController_?.Invoke(FirearmController_0);
				if (Action_1 != null)
				{
					gClass.HideWeapon(Action_1, Bool_0);
				}
				Action_1 = null;
				Player_0.BodyAnimatorCommon.SetFloat(PlayerAnimator.RELOAD_FLOAT_PARAM_HASH, 0f);
				FirearmController_0.method_10();
				if (Player_0.FastSlotSelection && Player_0.PreviousWeaponAimState)
				{
					FirearmController_0.ToggleAim();
				}
				if (Weapon_0.IsStationaryWeapon)
				{
					Player_0.MovementContext.LeftStanceController.DisableLeftStanceAnimFromHandsAction();
				}
			}

			public override void OnIdleStartEvent()
			{
				if (State != EOperationState.Finished)
				{
					WeaponAppeared();
				}
			}

			public override void HideWeapon(Action onHidden, bool fastDrop, Item nextControllerItem = null)
			{
				Bool_0 = fastDrop;
				Action_1 = onHidden;
			}

			public override void SetInventoryOpened(bool opened)
			{
			}

			public override void FastForward()
			{
				if (State != EOperationState.Finished)
				{
					FirearmsAnimator_0.Animator.Play(FirearmsAnimator_0.FullIdleStateName, 1, 0.1f);
					WeaponAppeared();
				}
			}

			public override bool CanChangeLightState(FirearmLightStateStruct[] lightsStates)
			{
				return false;
			}
		}

		public class GClass2049 : GClass2046
		{
			public GClass2049(FirearmController controller)
				: base(controller)
			{
			}

			public override void Start()
			{
				base.Start();
				if (Weapon_0.MalfState.State == Weapon.EMalfunctionState.Misfire)
				{
					AmmoItemClass ammoToFire = Weapon_0.MalfState.AmmoToFire;
					ammoToFire.IsUsed = false;
					BackendConfigSettingsClass instance = Singleton<BackendConfigSettingsClass>.Instance;
					float modsCoolFactor;
					float currentOverheat = Weapon_0.GetCurrentOverheat(GClass1891.PastTime, instance.Overheat, out modsCoolFactor);
					FirearmController_0.ShotMisfired(ammoToFire, Weapon_0.MalfState.State, currentOverheat);
				}
			}

			public override void OnShellEjectEvent()
			{
				FirearmController_0.weaponManagerClass.StartSpawnShell(FirearmController_0._player.Velocity * 0.66f);
				FirearmController_0.weaponManagerClass.SetRoundIntoWeapon(Weapon_0.MalfState.MalfunctionedAmmo);
				FirearmController_0.weaponManagerClass.MoveAmmoFromChamberToShellPort(ammoIsUsed: false);
			}

			public override void FastForward()
			{
				OnMalfunctionOffEvent();
			}

			public override void OnMalfunctionOffEvent()
			{
				FirearmsAnimator_0.SetAmmoInChamber(Weapon_0.ChamberAmmoCount);
				FirearmsAnimator_0.SetLayerWeight(FirearmsAnimator_0.MALFUNCTION_LAYER_INDEX, 1);
				FirearmsAnimator_0.Malfunction(-1);
				method_5();
			}
		}

		public class Class1270 : GClass2013
		{
			[NonSerialized]
			public Action Action_0;

			[NonSerialized]
			public Action Action_1;

			[NonSerialized]
			public bool Bool_0;

			[NonSerialized]
			public bool Bool_1;

			public Class1270(FirearmController controller)
				: base(controller)
			{
			}

			public void Start(Action callback = null)
			{
				Action_0 = callback;
				FirearmController_0.BipodState = !FirearmController_0.BipodState;
				FirearmsAnimator_0.SetInventory(open: false);
				FirearmsAnimator_0.SetBipod(FirearmController_0.BipodState);
				Player_0.MovementContext.SetBlindFire(0);
				EBipodToggleDirection bipodToggleDirection = WeaponManagerClass.BipodViewController.BipodToggleDirection;
				Player_0.MovementContext.SetInteractInHands((bipodToggleDirection != EBipodToggleDirection.Forward) ? (FirearmController_0.BipodState ? EInteraction.BipodBackwardOn : EInteraction.BipodBackwardOff) : (FirearmController_0.BipodState ? EInteraction.BipodForwardOn : EInteraction.BipodForwardOff));
				Player_0.ProceduralWeaponAnimation.IsBipodUsed = FirearmController_0.BipodState;
			}

			public override void Reset()
			{
				Action_0 = null;
				base.Reset();
			}

			public override void SetInventoryOpened(bool opened)
			{
				FirearmController_0.InventoryOpened = opened;
				FirearmsAnimator_0.SetInventory(opened);
			}

			public override void OnBipodToggleEvent()
			{
				method_5();
			}

			public void method_5()
			{
				State = EOperationState.Finished;
				FirearmsAnimator_0.SetInventory(FirearmController_0.InventoryOpened);
				if (Bool_0)
				{
					FirearmController_0.InitiateOperation<GClass2037>().HideWeapon(Action_1, Bool_1);
				}
				else
				{
					FirearmController_0.InitiateOperation<GClass2037>().Start();
				}
				if (Action_0 != null)
				{
					Action_0?.Invoke();
					Action_0 = null;
				}
			}

			public override void HideWeapon(Action onHidden, bool fastDrop, Item nextControllerItem = null)
			{
				Bool_0 = true;
				Action_1 = onHidden;
				Bool_1 = fastDrop;
			}

			public override void FastForward()
			{
				method_5();
				FirearmsAnimator_0.Animator.Play(FirearmsAnimator_0.FullIdleStateName, 1, 0.2f);
			}

			public override bool CanChangeLightState(FirearmLightStateStruct[] lightsStates)
			{
				return false;
			}
		}

		public class GClass2056 : GClass2013
		{
			[NonSerialized]
			public Action Action_0;

			[NonSerialized]
			public WeaponPrefab WeaponPrefab_0;

			[NonSerialized]
			public bool Bool_0;

			[NonSerialized]
			public bool Bool_1;

			[NonSerialized]
			public Action Action_1;

			public GClass2056(FirearmController controller)
				: base(controller)
			{
			}

			public void Start(bool isLauncherEnabled, Action callback = null)
			{
				Player_0.SetLauncherState(isLauncherEnabled);
				Action_0 = callback;
				Bool_0 = isLauncherEnabled;
				WeaponPrefab_0 = FirearmController_0.underbarrelManagerClass.LauncherWeaponPrefab;
				if (WeaponPrefab_0.FirearmsAnimator == null)
				{
					WeaponPrefab_0.Init(null, parent: true);
				}
				FirearmsAnimator_0.SetLauncher(isLauncherEnabled);
				if (isLauncherEnabled)
				{
					WeaponPrefab_0.SetUnderbarrelFastAnimator(Player_0);
					method_6(val: true);
				}
				else
				{
					WeaponPrefab_0.FirearmsAnimator.SetActiveParam(active: false);
				}
				Weapon_0.IsUnderBarrelDeviceActive = isLauncherEnabled;
				if (isLauncherEnabled)
				{
					FirearmController_0.method_9();
					Player_0.UpdateLauncherBones(isLauncherEnabled, WeaponPrefab_0);
				}
				method_5();
				Start();
			}

			public override void SetLeftStanceAnimOnStartOperation()
			{
			}

			public override void Reset()
			{
				base.Reset();
				Action_1 = null;
				Bool_1 = false;
				Action_0 = null;
				WeaponPrefab_0 = null;
				Bool_0 = false;
			}

			public override void SetInventoryOpened(bool opened)
			{
				SetAiming(isAiming: false);
				SetTriggerPressed(pressed: false);
				FirearmController_0.InventoryOpened = opened;
				FirearmsAnimator_0.SetInventory(opened);
			}

			public override void HideWeapon(Action onHidden, bool fastDrop, Item nextControllerItem = null)
			{
				Action_1 = onHidden;
				Bool_1 = fastDrop;
			}

			public override void LauncherAppeared()
			{
				State = EOperationState.Finished;
				Action action_ = Action_1;
				bool bool_ = Bool_1;
				Player_0.UpdateFirstPersonGrip(GripPose.EGripType.Common, FirearmController_0.HandsHierarchy);
				Player_0.ProceduralWeaponAnimation.method_8(FirearmController_0, FirearmController_0.UnderbarrelWeapon, null, FirearmController_0.gclass2250_0);
				Player_0.ProceduralWeaponAnimation.IsGrenadeLauncher = true;
				GClass2040 gClass = FirearmController_0.InitiateOperation<GClass2040>();
				if (action_ != null)
				{
					Action_0 = null;
				}
				gClass.Start(Action_0);
				if (action_ != null)
				{
					gClass.HideWeapon(action_, bool_);
				}
			}

			public override void OnEnd()
			{
				base.OnEnd();
				WeaponPrefab_0 = null;
			}

			public override void LauncherDisappeared()
			{
				Action action_ = Action_1;
				bool bool_ = Bool_1;
				State = EOperationState.Finished;
				Player_0.UpdateLauncherBones(launcherEnable: false, WeaponPrefab_0);
				Player_0.UpdateFirstPersonGrip(GripPose.EGripType.Common, FirearmController_0.HandsHierarchy);
				Player_0.ProceduralWeaponAnimation.method_8(FirearmController_0, FirearmController_0.Item, FirearmController_0.weaponPrefab_0, FirearmController_0.gclass2250_0);
				Player_0.ProceduralWeaponAnimation.IsGrenadeLauncher = false;
				Player_0.ProceduralWeaponAnimation.FindMountingPoint(FirearmController_0.HandsHierarchy);
				method_6(val: false);
				WeaponPrefab_0.ResetUnderbarrelFastAnimator(Player_0);
				GClass2037 gClass = FirearmController_0.InitiateOperation<GClass2037>();
				if (action_ != null)
				{
					Action_0 = null;
				}
				gClass.Start(Action_0);
				if (action_ != null)
				{
					gClass.HideWeapon(action_, bool_);
				}
			}

			public override void FastForward()
			{
				if (Bool_0)
				{
					LauncherAppeared();
				}
				else
				{
					LauncherDisappeared();
				}
			}

			public void method_5()
			{
				if (FirearmController_0.Blindfire)
				{
					FirearmController_0.Blindfire = false;
					Player_0.ProceduralWeaponAnimation.StartBlindFire(0);
				}
			}

			public void method_6(bool val)
			{
				WeaponPrefab_0.Animator.enabled = val;
				if (val)
				{
					WeaponPrefab_0.FirearmsAnimator.AddEventsConsumer(FirearmController_0);
					FirearmController_0.firearmsAnimator_0 = WeaponPrefab_0.FirearmsAnimator;
					FirearmController_0.AnimationEventsEmitter = WeaponPrefab_0.AnimationEventsEmitter;
					WeaponPrefab_0.FirearmsAnimator.SetActiveParam(active: true);
				}
				else
				{
					WeaponPrefab_0.FirearmsAnimator.RemoveEventsConsumer(FirearmController_0);
					FirearmController_0.firearmsAnimator_0 = FirearmController_0.weaponPrefab_0.FirearmsAnimator;
					FirearmController_0.AnimationEventsEmitter = FirearmController_0.weaponPrefab_0.AnimationEventsEmitter;
				}
			}
		}

		public class UnderbarrelManagerClass : GInterface210
		{
			public const string SHELLPORT_TRANSFORM_NAME = "shellport";

			public const string PATRON_IN_WEAPON_TRANSFORM_NAME = "patron_in_weapon";

			[NonSerialized]
			public Player Player_0;

			[NonSerialized]
			public FirearmController FirearmController_0;

			[NonSerialized]
			public WeaponPrefab WeaponPrefab_0;

			[NonSerialized]
			public LauncherItemClass LauncherItemClass;

			[NonSerialized]
			public bool Bool_0;

			[NonSerialized]
			public FirearmsEffects FirearmsEffects_0;

			[NonSerialized]
			public WeaponSoundPlayer WeaponSoundPlayer_0;

			[NonSerialized]
			public Transform Transform_0;

			[NonSerialized]
			public Vector3 Vector3_0;

			[NonSerialized]
			public Transform Transform_1;

			[NonSerialized]
			public AmmoPoolObject AmmoPoolObject_0;

			[NonSerialized]
			public AmmoPoolObject AmmoPoolObject_1;

			[NonSerialized]
			public ShellExtractionData ShellExtractionData_0;

			[NonSerialized]
			public BifacialTransform BifacialTransform_0 = new BifacialTransform();

			[NonSerialized]
			public WaitForEndOfFrame WaitForEndOfFrame_0 = new WaitForEndOfFrame();

			public FirearmsEffects FirearmsEffects => FirearmsEffects_0;

			public WeaponSoundPlayer WeaponSoundPlayer => WeaponSoundPlayer_0;

			public static WeaponSounds WeaponSounds_0 => Singleton<BetterAudio>.Instance.MiscCollisionSounds;

			public BifacialTransform Fireport => BifacialTransform_0;

			public WeaponPrefab LauncherWeaponPrefab => WeaponPrefab_0;

			public void Init(Player player, FirearmController controller, LauncherItemClass launcher)
			{
				WeaponPrefab_0 = controller.weaponPrefab_0._objectInstance.gameObject.GetComponentInChildren<WeaponPrefab>();
				method_0(player, controller, launcher);
			}

			public void Init(Player player, FirearmController controller, LauncherItemClass launcher, GameObject underbarrelPrefab)
			{
				WeaponPrefab_0 = underbarrelPrefab.GetComponent<WeaponPrefab>();
				method_0(player, controller, launcher);
			}

			public void InitWeaponSoundPlayer()
			{
				method_3();
			}

			public void method_0(Player player, FirearmController controller, LauncherItemClass launcher)
			{
				Player_0 = player;
				FirearmController_0 = controller;
				LauncherItemClass = launcher;
				LauncherItemClass.ResetRangeValueToDefault();
				ShellExtractionData_0 = WeaponPrefab_0.GetComponent<ShellExtractionData>();
				method_1();
				method_2();
				method_4();
				method_5();
			}

			public void method_1()
			{
				FirearmsEffects_0 = WeaponPrefab_0.gameObject.AddComponent<FirearmsEffects>();
				FirearmsEffects_0.Init(WeaponPrefab_0.transform);
			}

			public void method_2()
			{
				BifacialTransform_0.Original = TransformHelperClass.FindTransformRecursive(WeaponPrefab_0.transform, "fireport");
			}

			public void method_3()
			{
				if (!Bool_0)
				{
					WeaponSoundPlayer_0 = WeaponPrefab_0.transform.GetComponent<WeaponSoundPlayer>();
					WeaponSoundPlayer_0.Init(FirearmController_0, BifacialTransform_0, Player_0);
					Bool_0 = true;
				}
			}

			public void StartSpawnShell(Vector3 playerVelocity, int shellPortIndex = 0)
			{
				if (method_7())
				{
					WeaponPrefab_0.StartCoroutine(method_11(playerVelocity, shellPortIndex));
				}
			}

			public void method_4()
			{
				Transform_0 = TransformHelperClass.FindTransformRecursive(WeaponPrefab_0.transform, "shellport");
				Transform_1 = TransformHelperClass.FindTransformRecursive(WeaponPrefab_0.transform, "patron_in_weapon");
				if (Transform_0 != null)
				{
					Vector3_0 = Transform_0.localPosition;
				}
			}

			public void method_5()
			{
				if (LauncherItemClass.Chamber.ContainedItem != null)
				{
					SetRoundIntoWeapon((AmmoItemClass)LauncherItemClass.Chamber.ContainedItem);
				}
				if (LauncherItemClass.ShellsInChambers == null)
				{
					return;
				}
				for (int i = 0; i < LauncherItemClass.ShellsInChambers.Length; i++)
				{
					AmmoTemplate ammoTemplate = LauncherItemClass.ShellsInChambers[i];
					if (ammoTemplate != null)
					{
						SetPatronInShellPort(Singleton<PoolManagerClass>.Instance.CreateFromPool<AmmoPoolObject>(ammoTemplate.Prefab), i);
					}
				}
			}

			public bool method_6()
			{
				return CameraClass.Instance.Distance(WeaponPrefab_0.transform.position) < EFTHardSettings.Instance.PATRONS_MANIPULATIONS_VISIBLE_DISTANCE;
			}

			public bool method_7()
			{
				return CameraClass.Instance.Distance(WeaponPrefab_0.transform.position) < EFTHardSettings.Instance.FLYING_SHELLS_VISIBLE_DISTANCE;
			}

			public void MoveAmmoFromChamberToShellPort(bool ammoIsUsed, int chamberIndex = 0)
			{
				AmmoPoolObject ammoPoolObject_ = AmmoPoolObject_0;
				AmmoPoolObject_0 = null;
				if (!(ammoPoolObject_ == null))
				{
					SetPatronInShellPort(ammoPoolObject_, chamberIndex);
					if (method_6())
					{
						ammoPoolObject_.SetUsed(ammoIsUsed);
					}
				}
			}

			public void SetRoundIntoWeapon(AmmoItemClass ammo)
			{
				if (method_6())
				{
					if (AmmoPoolObject_0 != null)
					{
						UnityEngine.Debug.LogWarning("Already have an ammo in chamber");
						DestroyPatronInWeapon();
					}
					Transform transform_ = Transform_1;
					if (transform_.childCount > 0)
					{
						AssetPoolObject.ReturnToPool(transform_.GetChild(0).gameObject);
					}
					AmmoPoolObject ammoPoolObject = smethod_0(ammo);
					ammoPoolObject.SetUsed(ammo.IsUsed);
					ParentAmmoOrShellToTransform(ammoPoolObject.gameObject, transform_);
					AmmoPoolObject_0 = ammoPoolObject;
				}
			}

			public void SetPatronInShellPort(AmmoPoolObject ammoObject, int shellTransformIndex = 0)
			{
				if (AmmoPoolObject_1 != null)
				{
					UnityEngine.Debug.LogError("Error: already have a shell in shell port");
					AssetPoolObject.ReturnToPool(AmmoPoolObject_1.gameObject);
				}
				AmmoPoolObject_1 = ammoObject;
				ParentAmmoOrShellToTransform(ammoObject.gameObject, Transform_0);
			}

			public bool DestroyPatronInWeapon()
			{
				if (AmmoPoolObject_0 == null)
				{
					return false;
				}
				AssetPoolObject.ReturnToPool(AmmoPoolObject_0.gameObject);
				AmmoPoolObject_0 = null;
				return true;
			}

			public void RemoveShellInWeapon()
			{
				if (method_6())
				{
					method_8();
				}
			}

			public void method_8()
			{
				if (!(AmmoPoolObject_1 == null))
				{
					AssetPoolObject.ReturnToPool(AmmoPoolObject_1.gameObject);
					AmmoPoolObject_1 = null;
				}
			}

			public bool HasPatronInWeapon()
			{
				return AmmoPoolObject_0 != null;
			}

			public bool HasShellInWeapon()
			{
				return AmmoPoolObject_1 != null;
			}

			public static AmmoPoolObject smethod_0(Item ammo)
			{
				GameObject gameObject = Singleton<PoolManagerClass>.Instance.CreateItem(ammo, isAnimated: true);
				AmmoPoolObject component = gameObject.GetComponent<AmmoPoolObject>();
				if (component == null)
				{
					throw new Exception("Error: gameobject " + gameObject?.ToString() + " doesn't have AmmoPoolObject component");
				}
				return component;
			}

			public static void ParentAmmoOrShellToTransform(GameObject shell, Transform shellParent)
			{
				shell.transform.position = shellParent.position;
				shell.transform.rotation = shellParent.rotation;
				shell.transform.localRotation *= Quaternion.Euler(90f, 0f, 0f);
				shell.transform.SetParent(shellParent, worldPositionStays: true);
				shell.transform.localScale = Vector3.one;
				shell.transform.localPosition = Vector3.zero;
				shell.SetActive(value: true);
			}

			public AmmoPoolObject method_9(Vector3 playerVelocity, AmmoPoolObject shell)
			{
				AmmoPoolObject ammoPoolObject = Singleton<GameWorld>.Instance.SpawnShellInTheWorld(ref shell);
				ammoPoolObject.transform.parent = null;
				Vector3 shotRotationVector = ShellExtractionData_0.GetShotRotationVector();
				Vector3 shotAdditionalForce = ShellExtractionData_0.GetShotAdditionalForce();
				Vector3 force = (Transform_0.localPosition - Vector3_0) * ShellExtractionData_0.GetShotShellForceMultiplier() + shotAdditionalForce;
				method_10(force, shotRotationVector, ammoPoolObject, playerVelocity);
				ammoPoolObject.StartAutoDestroyCountDown();
				return ammoPoolObject;
			}

			public void method_10(Vector3 force, Vector3 torque, AmmoPoolObject shell, Vector3 parentForce)
			{
				shell.EnablePhysics(force, torque, parentForce, Transform_0.transform.forward);
				shell.gameObject.layer = LayerMaskClass.ShellsLayer;
				shell.Shell.CollisionListener = this;
			}

			public IEnumerator method_11(Vector3 playerVelocity, int shellPortIndex = 0)
			{
				if (!(AmmoPoolObject_1 == null))
				{
					AmmoPoolObject ammoPoolObject_ = AmmoPoolObject_1;
					AmmoPoolObject_1 = null;
					yield return WaitForEndOfFrame_0;
					if (!(ammoPoolObject_ == null))
					{
						method_9(playerVelocity, ammoPoolObject_).SetUsed(isUsed: true);
					}
				}
			}

			public void method_12(Vector3 position, BaseBallistic.ESurfaceSound material, ECaliber caliber)
			{
				SoundBank soundBank = null;
				float volume = 1f;
				switch (material)
				{
				case BaseBallistic.ESurfaceSound.Plastic:
					soundBank = caliber switch
					{
						ECaliber.ShellHeavy => WeaponSounds_0.ShellHeavyPlastic, 
						ECaliber.Shell12Cal => WeaponSounds_0.Shell12calPlastic, 
						ECaliber.Shell556Mm => WeaponSounds_0.Shell556mmPlastic, 
						ECaliber.Shell9Mm => WeaponSounds_0.Shell9mmPlastic, 
						_ => null, 
					};
					volume = 0.6f;
					break;
				case BaseBallistic.ESurfaceSound.Metal:
					soundBank = caliber switch
					{
						ECaliber.ShellHeavy => WeaponSounds_0.ShellHeavyMetal, 
						ECaliber.Shell12Cal => WeaponSounds_0.Shell12calMetal, 
						ECaliber.Shell556Mm => WeaponSounds_0.Shell556mmMetal, 
						ECaliber.Shell9Mm => WeaponSounds_0.Shell9mmMetal, 
						_ => null, 
					};
					break;
				case BaseBallistic.ESurfaceSound.Wood:
					soundBank = caliber switch
					{
						ECaliber.ShellHeavy => WeaponSounds_0.ShellHeavyWood, 
						ECaliber.Shell12Cal => WeaponSounds_0.Shell12calWood, 
						ECaliber.Shell556Mm => WeaponSounds_0.Shell556mmWood, 
						ECaliber.Shell9Mm => WeaponSounds_0.Shell9mmWood, 
						_ => null, 
					};
					break;
				case BaseBallistic.ESurfaceSound.Grass:
					soundBank = caliber switch
					{
						ECaliber.ShellHeavy => WeaponSounds_0.ShellHeavySoil, 
						ECaliber.Shell12Cal => WeaponSounds_0.Shell12calSoil, 
						ECaliber.Shell556Mm => WeaponSounds_0.Shell556mmSoil, 
						ECaliber.Shell9Mm => WeaponSounds_0.Shell9mmSoil, 
						_ => null, 
					};
					break;
				case BaseBallistic.ESurfaceSound.Concrete:
				case BaseBallistic.ESurfaceSound.Asphalt:
					soundBank = caliber switch
					{
						ECaliber.ShellHeavy => WeaponSounds_0.ShellHeavyConcrete, 
						ECaliber.Shell12Cal => WeaponSounds_0.Shell12calConcrete, 
						ECaliber.Shell556Mm => WeaponSounds_0.Shell556mmConcrete, 
						ECaliber.Shell9Mm => WeaponSounds_0.Shell9mmConcrete, 
						_ => null, 
					};
					break;
				case BaseBallistic.ESurfaceSound.Soil:
				case BaseBallistic.ESurfaceSound.Gravel:
					soundBank = caliber switch
					{
						ECaliber.ShellHeavy => WeaponSounds_0.ShellHeavySoil, 
						ECaliber.Shell12Cal => WeaponSounds_0.Shell12calSoil, 
						ECaliber.Shell556Mm => WeaponSounds_0.Shell556mmSoil, 
						ECaliber.Shell9Mm => WeaponSounds_0.Shell9mmSoil, 
						_ => null, 
					};
					break;
				}
				if (soundBank != null)
				{
					EOcclusionTest occlusionTest = ((Player_0 == null || Player_0.PointOfView == EPointOfView.ThirdPerson) ? EOcclusionTest.OneShotPropagation : EOcclusionTest.None);
					Singleton<BetterAudio>.Instance.PlayAtPoint(position + Vector3.up / 4f, soundBank, (int)soundBank.SourceType, CameraClass.Instance.Distance(position), volume, -1f, EnvironmentType.Outdoor, occlusionTest, oneShot: true, needUpdate: false);
				}
			}

			public void InvokeShellCollision(Vector3 position, BaseBallistic.ESurfaceSound material, ECaliber caliber)
			{
				method_12(position, material, caliber);
			}

			public void Clear()
			{
				RemoveShellInWeapon();
				DestroyPatronInWeapon();
			}
		}

		public class GClass2038 : GClass2037
		{
			public enum EUtilityType
			{
				None,
				ExamineWeapon,
				CheckChamber,
				CheckMagazine,
				CheckFireMode
			}

			[NonSerialized]
			public const float Float_3 = 2.5f;

			[NonSerialized]
			public float Float_4;

			[NonSerialized]
			public bool Bool_1;

			[NonSerialized]
			public EUtilityType EutilityType_0;

			public GClass2038(FirearmController controller)
				: base(controller)
			{
			}

			public void Start(EUtilityType utilityType)
			{
				EutilityType_0 = utilityType;
				FirearmsAnimator_0.SetShellsInWeapon(Weapon_0.ShellsInWeaponCount);
				State = EOperationState.Executing;
				Float_4 = 0f;
				SetLeftStanceAnimOnStartOperation();
			}

			public override void SetLeftStanceAnimOnStartOperation()
			{
				Player_0.MovementContext.LeftStanceController.DisableLeftStanceAnimFromHandsAction();
			}

			public override void OnIdleStartEvent()
			{
				if (State == EOperationState.Ready)
				{
					base.OnIdleStartEvent();
					State = EOperationState.Finished;
					FirearmController_0.InitiateOperation<GClass2037>().Start();
				}
			}

			public override void OnUtilityOperationStartEvent()
			{
				State = EOperationState.Ready;
			}

			public override bool CanStartReload()
			{
				return false;
			}

			public override void Reset()
			{
				EutilityType_0 = EUtilityType.None;
				base.Reset();
			}

			public override bool CheckAmmo()
			{
				return false;
			}

			public override bool CheckChamber()
			{
				return false;
			}

			public override bool CheckFireMode()
			{
				return false;
			}

			public override void ReloadMag(MagazineItemClass magazine, ItemAddress itemAddress, Callback finishCallback, Callback startCallback)
			{
			}

			public override void ReloadWithAmmo(AmmoPackReloadingClass ammoPack, Callback finishCallback, Callback startCallback)
			{
			}

			public override void ReloadCylinderMagazine(AmmoPackReloadingClass ammoPack, Callback finishCallback, Callback startCallback, bool quickReload = false)
			{
			}

			public override void QuickReloadMag(MagazineItemClass magazine, Callback finishCallback, Callback startCallback)
			{
			}

			public override void ReloadGrenadeLauncher(AmmoPackReloadingClass ammoPack, Callback callback)
			{
			}

			public override void SetTriggerPressed(bool pressed)
			{
				if (EutilityType_0 == EUtilityType.ExamineWeapon)
				{
					OnUtilityOperationStartEvent();
					OnIdleStartEvent();
					FirearmController_0.CurrentOperation.SetTriggerPressed(pressed);
				}
			}

			public override void SetInventoryOpened(bool opened)
			{
				Bool_1 = opened;
				if (!Bool_1)
				{
					Float_4 = 0f;
				}
				base.SetInventoryOpened(opened);
			}

			public override void Update(float deltaTime)
			{
				base.Update(deltaTime);
				if (State != EOperationState.Executing || Bool_1)
				{
					return;
				}
				if (Float_4 > 2.5f)
				{
					if (FirearmsAnimator_0 != null)
					{
						UnityEngine.Debug.LogError("UtilityOperationEvent not found on " + FirearmsAnimator_0.Animator.name);
					}
					else
					{
						UnityEngine.Debug.LogError("UtilityOperationEvent not found. No animator!");
					}
					State = EOperationState.Ready;
					OnIdleStartEvent();
				}
				else
				{
					Float_4 += deltaTime;
				}
			}

			public override void SetAiming(bool isAiming)
			{
			}

			public override bool ExamineWeapon()
			{
				return true;
			}

			public override void OnShellEjectEvent()
			{
				FirearmController_0.weaponManagerClass.StartSpawnShell(FirearmController_0._player.Velocity * 0.66f);
				for (int i = 0; i < Weapon_0.ShellsInChambers.Length; i++)
				{
					Weapon_0.ShellsInChambers[i] = null;
				}
				FirearmsAnimator_0.SetShellsInWeapon(Weapon_0.ShellsInWeaponCount);
			}
		}

		public class GClass2036 : GenericFireOperationClass
		{
			[NonSerialized]
			public AmmoItemClass AmmoItemClass;

			public GClass2036(FirearmController controller)
				: base(controller)
			{
			}

			public override void Start()
			{
				base.Start();
				FirearmsAnimator_0.SetFire(FirearmController_0.IsTriggerPressed);
				FirearmController_0.SetAim(value: false);
			}

			public override void PrepareShot()
			{
				FirearmsAnimator_0.SetFire(FirearmController_0.IsTriggerPressed);
			}

			public override void OnFireEvent()
			{
				Bool_1 = true;
				AmmoItemClass = Weapon_0.FirstLoadedChamberSlot.ContainedItem as AmmoItemClass;
				if (AmmoItemClass != null)
				{
					AmmoItemClass.IsUsed = true;
					FirearmController_0.method_56(AmmoItemClass);
					FirearmController_0.Weapon.FirstLoadedChamberSlot.RemoveItem();
					FirearmController_0.weaponManagerClass.MoveAmmoFromChamberToShellPort(AmmoItemClass.IsUsed);
				}
			}

			public override void OnFireEndEvent()
			{
				SetTriggerPressed(pressed: false);
				FirearmsAnimator_0.SetFire(fire: false);
				State = EOperationState.Finished;
				FirearmController_0.InitiateOperation<GClass2037>().Start();
			}

			public override void SetTriggerPressed(bool pressed)
			{
				FirearmController_0.IsTriggerPressed &= pressed;
			}

			public override bool CanNotBeInterrupted()
			{
				return true;
			}

			public override void Reset()
			{
				AmmoItemClass = null;
				base.Reset();
			}
		}

		[Serializable]
		[CompilerGenerated]
		public class Class1237
		{
			public static readonly Class1237 class1237_0 = new Class1237();

			public static Func<TacticalComboItemClass, bool> func_0;

			public static Func<Slot, bool> func_1;

			public static Func<Slot, BipodItemClass> func_2;

			public static Func<Slot, bool> func_3;

			public static Func<Slot, TacticalComboItemClass> func_4;

			public static Func<Slot, bool> func_5;

			public static Func<Slot, Mod> func_6;

			public static Func<Mod, int> func_7;

			public static Func<Slot, Item> func_8;

			public static Func<LightComponent, string> func_9;

			public static Func<LightComponent, LightComponent> func_10;

			public static Func<Slot, Item> func_11;

			public static Func<SightComponent, string> func_12;

			public static Func<SightComponent, SightComponent> func_13;

			public static Func<KeyValuePair<string, LightComponent>, bool> func_14;

			public static Func<Slot, Item> func_15;

			public bool method_0(TacticalComboItemClass x)
			{
				if (x.Light != null)
				{
					return x.Light.IsActive;
				}
				return false;
			}

			public bool method_1(Slot slot)
			{
				return slot.ContainedItem is BipodItemClass;
			}

			public BipodItemClass method_2(Slot x)
			{
				return x.ContainedItem as BipodItemClass;
			}

			public bool method_3(Slot slot)
			{
				return slot.ContainedItem is TacticalComboItemClass;
			}

			public TacticalComboItemClass method_4(Slot x)
			{
				return x.ContainedItem as TacticalComboItemClass;
			}

			public bool method_5(Slot slot)
			{
				return slot.ContainedItem is Mod;
			}

			public Mod method_6(Slot slot)
			{
				return slot.ContainedItem as Mod;
			}

			public int method_7(Mod mod)
			{
				return mod.UniqueAnimationModID;
			}

			public Item method_8(Slot x)
			{
				return x.ContainedItem;
			}

			public string method_9(LightComponent x)
			{
				return x.Item.Id;
			}

			public LightComponent method_10(LightComponent x)
			{
				return x;
			}

			public Item method_11(Slot x)
			{
				return x.ContainedItem;
			}

			public string method_12(SightComponent x)
			{
				return x.Item.Id;
			}

			public SightComponent method_13(SightComponent x)
			{
				return x;
			}

			public bool method_14(KeyValuePair<string, LightComponent> x)
			{
				return x.Value.IsActive;
			}

			public Item method_15(Slot slot)
			{
				return slot.ContainedItem;
			}
		}

		[Serializable]
		[CompilerGenerated]
		public class Class1238<T> where T : FirearmController
		{
			public static readonly Class1238<T> class1238_0 = new Class1238<T>();

			public static Func<Slot, bool> func_0;

			public static Func<Slot, BipodItemClass> func_1;

			public static Func<Slot, bool> func_2;

			public static Func<Slot, TacticalComboItemClass> func_3;

			public bool method_0(Slot slot)
			{
				return slot.ContainedItem is BipodItemClass;
			}

			public BipodItemClass method_1(Slot x)
			{
				return x.ContainedItem as BipodItemClass;
			}

			public bool method_2(Slot slot)
			{
				return slot.ContainedItem is TacticalComboItemClass;
			}

			public TacticalComboItemClass method_3(Slot x)
			{
				return x.ContainedItem as TacticalComboItemClass;
			}
		}

		[CompilerGenerated]
		public class Class1239<T> where T : FirearmController
		{
			public Player player;

			public T controller;

			public bool method_0()
			{
				if (player.AIData == null)
				{
					return false;
				}
				if (player.AIData.IsAI)
				{
					return player.AIData.BotOwner.LookSensor.ShootFromEyes;
				}
				return false;
			}

			public bool method_1()
			{
				if (player.AIData != null && player.IsAI)
				{
					return player.AIData.IsNoOffsetShooting;
				}
				return false;
			}

			public void method_2()
			{
				ProceduralWeaponAnimation proceduralWeaponAnimation = player.ProceduralWeaponAnimation;
				proceduralWeaponAnimation.AvailableScopesChanged = (Action)Delegate.Remove(proceduralWeaponAnimation.AvailableScopesChanged, new Action(controller.ValidateCurrentScopeIndex));
			}

			public void method_3()
			{
				controller.firearmsAnimator_0.RemoveEventsConsumer(controller);
			}

			public void method_4(bool visible)
			{
				controller.weaponManagerClass.SetVisiblePatronInWeapon(visible);
			}

			public void method_5()
			{
				controller.firearmsAnimator_0.SetPatronInWeaponVisibleEvent -= delegate(bool visible)
				{
					controller.weaponManagerClass.SetVisiblePatronInWeapon(visible);
				};
			}

			public void method_6()
			{
				controller._player.Skills.WeaponMastered -= controller.OnCurrentWeaponBeingMastered;
				controller._player.Skills.OnSkillLevelChanged -= controller.method_3;
				controller._player.MovementContext.PhysicalConditionChanged -= controller.method_4;
			}
		}

		[CompilerGenerated]
		public class Class1240
		{
			public FirearmController firearmController;

			public FirearmController firearmController_0;

			public GInterface73 modAudioController;

			public void method_0(AbstractHandsController oldController, AbstractHandsController newController)
			{
				// Found self-referencing delegate construction. Abort transformation to avoid stack overflow.
				if (!(oldController != firearmController))
				{
					firearmController_0._player.OnHandsControllerChanged -= method_0;
					modAudioController?.Clear();
				}
			}
		}

		[CompilerGenerated]
		public class Class1241
		{
			public Action callback;

			public FirearmController firearmController_0;

			public void method_0()
			{
				callback();
				firearmController_0._player.MovementContext.OnStateChanged += firearmController_0.method_17;
				firearmController_0._player.Physical.OnSprintStateChangedEvent += firearmController_0.method_16;
			}
		}

		[CompilerGenerated]
		public class Class1242
		{
			public Class1312 inventoryOperation;

			public Action callback;

			public void method_0()
			{
				inventoryOperation.Confirm();
				callback();
			}
		}

		public const int CALCULATOR_SEED = 0;

		private const string string_0 = "Cant StartReload";

		private const float float_0 = 1.5f;

		public const float OFFSET_FOR_OVERLAP_RAY_ON_LEFT_SHOULDER = 0.2f;

		public const float ADDITIONAL_LEFTSTANCE_OVERLAP_RAY_LENGTH = 0.2f;

		private const float float_1 = 0.5f;

		[CompilerGenerated]
		private Action action_0;

		[CompilerGenerated]
		private Action<FirearmController> action_1;

		[CompilerGenerated]
		private Action action_2;

		protected static readonly List<AmmoItemClass> _preallocatedAmmoList = new List<AmmoItemClass>(10);

		private static readonly List<EftBulletClass> list_0 = new List<EftBulletClass>(10);

		private static readonly RaycastHit[] raycastHit_0 = new RaycastHit[8];

		public GClass768 CCV;

		public Transform GunBaseTransform;

		public BifacialTransform Fireport;

		public BifacialTransform[] MultiBarrelsFireports;

		public bool _blindfire;

		public int CurrentChamberIndex;

		public float HipInaccuracy;

		public TacticalComboItemClass[] AimingDevices;

		public int AmmoInChamberOnSpawn;

		public bool autoFireOn;

		public BipodItemClass Bipod;

		internal Func<bool> func_0;

		internal Func<bool> func_1;

		internal WeaponManagerClass weaponManagerClass;

		internal FirearmsAnimator firearmsAnimator_0;

		protected float WeaponLn;

		protected bool AimingInterruptedByOverlap;

		protected bool _isAiming;

		private bool bool_0;

		private bool bool_1;

		protected float _aimingSens = -1f;

		protected ISharedBallisticsCalculator BallisticsCalculator;

		private WeaponPrefab weaponPrefab_0;

		private MalfunctionRandom malfunctionRandom_0;

		private bool bool_2;

		private bool bool_3;

		private float float_2;

		private float float_3 = 0.001f;

		private float float_4;

		private SkillManager.GClass2250 gclass2250_0;

		private bool bool_4;

		private bool bool_5;

		private bool bool_6;

		private int int_0;

		private GClass849<float> gclass849_0;

		private GClass849<float> gclass849_1;

		private Func<RaycastHit, bool> func_2;

		private OneOffWeaponSettings oneOffWeaponSettings_0;

		private bool bool_7;

		private bool bool_8;

		private float float_5 = 1f;

		private WeaponSoundPlayer weaponSoundPlayer_0;

		private int int_1;

		private UnderbarrelManagerClass underbarrelManagerClass;

		public LauncherItemClass UnderbarrelWeapon;

		[CompilerGenerated]
		private bool bool_9;

		[CompilerGenerated]
		private bool bool_10;

		private bool bool_11;

		private bool bool_12;

		private float float_6 = 1f;

		private float float_7;

		[CompilerGenerated]
		private Action<RocketProjectile> action_3;

		private List<GClass820<Weapon.EMalfunctionSource>.GStruct49<float, Weapon.EMalfunctionSource>> list_1 = new List<GClass820<Weapon.EMalfunctionSource>.GStruct49<float, Weapon.EMalfunctionSource>>(4);

		private List<GClass820<Weapon.EMalfunctionState>.GStruct49<float, Weapon.EMalfunctionState>> list_2 = new List<GClass820<Weapon.EMalfunctionState>.GStruct49<float, Weapon.EMalfunctionState>>(5);

		public new Weapon Item => base.Item as Weapon;

		public override FirearmsAnimator FirearmsAnimator => firearmsAnimator_0;

		public override string LoggerDistinctId => $"{_player.ProfileId}|{_player.Profile.Info.Nickname}|{this}";

		public SkillManager.GClass2250 BuffInfo => gclass2250_0;

		public bool IsOverlap => float_2 > 0f;

		public float OverlapValue => float_2;

		public bool IsSilenced
		{
			[CompilerGenerated]
			get
			{
				return bool_9;
			}
			[CompilerGenerated]
			set
			{
				bool_9 = value;
			}
		}

		public int CurrentMasteringLevel => _player.Skills.GetMastering(Item.TemplateId)?.Level ?? 0;

		public float TotalErgonomics => gclass849_1.Value;

		public float ErgonomicWeight => gclass849_0.Value;

		public BifacialTransform CurrentFireport
		{
			get
			{
				if (!Item.IsMultiBarrel)
				{
					return Fireport;
				}
				return MultiBarrelsFireports[CurrentChamberIndex];
			}
		}

		public override float AimingSensitivity
		{
			get
			{
				if (!IsAiming)
				{
					return _player.GetAimingSensitivity();
				}
				return _aimingSens;
			}
		}

		public virtual Vector3 WeaponDirection => CurrentFireport.Original.TransformDirection(_player.LocalShotDirection);

		public Weapon Weapon => Item;

		public Vector3 FireportPosition => CurrentFireport.position;

		public bool MouseLookControl => _player.MouseLookControl;

		public WeaponSoundPlayer WeaponSoundPlayer => weaponSoundPlayer_0;

		public bool IsBirstOf2Start
		{
			[CompilerGenerated]
			get
			{
				return bool_10;
			}
			[CompilerGenerated]
			set
			{
				bool_10 = value;
			}
		}

		public bool HasBipod => Bipod != null;

		public bool IsStationaryWeapon => _player.MovementContext.StationaryWeapon != null;

		public GClass2013 CurrentOperation => base.CurrentHandsOperation as GClass2013;

		public virtual bool IsTriggerPressed
		{
			get
			{
				return bool_4;
			}
			set
			{
				if (!value)
				{
					action_2?.Invoke();
				}
				bool_4 = value;
			}
		}

		public override bool IsAiming
		{
			get
			{
				return _isAiming;
			}
			set
			{
				if (!value)
				{
					_player.Physical.HoldBreath(enable: false);
				}
				if (_isAiming == value)
				{
					method_64();
					return;
				}
				_isAiming = value;
				_player.Skills.FastAimTimer.Target = (value ? 0f : 2f);
				_player.MovementContext.SetAimingSlowdown(IsAiming, 0.33f + gclass2250_0.AimMovementSpeed);
				_player.Physical.Aim((!_isAiming || !(_player.MovementContext.StationaryWeapon == null)) ? 0f : ErgonomicWeight);
				if (bool_7)
				{
					method_64();
				}
				else
				{
					method_63(_isAiming);
				}
				weaponManagerClass.SetAiming(_isAiming);
				UpdateSensitivity();
				AimingChanged(value);
			}
		}

		public bool Malfunction
		{
			get
			{
				return bool_3;
			}
			set
			{
				if (value)
				{
					action_2?.Invoke();
				}
				bool_3 = value;
			}
		}

		public bool InventoryOpened
		{
			get
			{
				return bool_2;
			}
			set
			{
				bool_2 = value;
				if (bool_2)
				{
					SetCompassState(active: false);
					BlindFire(0);
				}
			}
		}

		public bool Blindfire
		{
			get
			{
				return _blindfire;
			}
			set
			{
				_blindfire = value;
				if (_blindfire)
				{
					SetCompassState(active: false);
				}
			}
		}

		public bool BipodState
		{
			get
			{
				return bool_8;
			}
			set
			{
				bool_8 = value;
			}
		}

		public bool DisableLeftStanceByOverlap => bool_11;

		public bool CanSetBlindFire
		{
			get
			{
				if (!(Weapon is RocketLauncherItemClass) && !(CurrentOperation is GClass2038) && (CurrentOperation is GClass2037 || CurrentOperation is GClass2040))
				{
					return !InventoryOpened;
				}
				return false;
			}
		}

		public bool IsBipodsOperation => CurrentOperation is Class1270;

		public event Action OnShot
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

		public event Action<FirearmController> OnReadyToOperate
		{
			[CompilerGenerated]
			add
			{
				Action<FirearmController> action = action_1;
				Action<FirearmController> action2;
				do
				{
					action2 = action;
					Action<FirearmController> value2 = (Action<FirearmController>)Delegate.Combine(action2, value);
					action = Interlocked.CompareExchange(ref action_1, value2, action2);
				}
				while ((object)action != action2);
			}
			[CompilerGenerated]
			remove
			{
				Action<FirearmController> action = action_1;
				Action<FirearmController> action2;
				do
				{
					action2 = action;
					Action<FirearmController> value2 = (Action<FirearmController>)Delegate.Remove(action2, value);
					action = Interlocked.CompareExchange(ref action_1, value2, action2);
				}
				while ((object)action != action2);
			}
		}

		public event Action BreakLoop
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

		public event Action<RocketProjectile> OnRocketLaunchedEvent
		{
			[CompilerGenerated]
			add
			{
				Action<RocketProjectile> action = action_3;
				Action<RocketProjectile> action2;
				do
				{
					action2 = action;
					Action<RocketProjectile> value2 = (Action<RocketProjectile>)Delegate.Combine(action2, value);
					action = Interlocked.CompareExchange(ref action_3, value2, action2);
				}
				while ((object)action != action2);
			}
			[CompilerGenerated]
			remove
			{
				Action<RocketProjectile> action = action_3;
				Action<RocketProjectile> action2;
				do
				{
					action2 = action;
					Action<RocketProjectile> value2 = (Action<RocketProjectile>)Delegate.Remove(action2, value);
					action = Interlocked.CompareExchange(ref action_3, value2, action2);
				}
				while ((object)action != action2);
			}
		}

		public static T smethod_6<T>(Player player, Weapon weapon) where T : FirearmController
		{
			T val = ItemHandsController.smethod_0<T>(player, weapon);
			smethod_8(val, player, weapon, async: false).HandleExceptions();
			return val;
		}

		public static async Task<T> smethod_7<T>(Player player, Weapon weapon) where T : FirearmController
		{
			T val = await ItemHandsController.smethod_2<T>(player, weapon);
			await smethod_8(val, player, weapon);
			return val;
		}

		public static async Task smethod_8<T>(T controller, Player player, Weapon weapon, bool async = true) where T : FirearmController
		{
			WeaponPrefab component = controller.ControllerGameObject.GetComponent<WeaponPrefab>();
			AssetPoolObject component2 = controller._controllerObject.GetComponent<AssetPoolObject>();
			component.ResetStatesToDefault();
			controller.func_0 = delegate
			{
				if (player.AIData == null)
				{
					return false;
				}
				return player.AIData.IsAI && player.AIData.BotOwner.LookSensor.ShootFromEyes;
			};
			controller.func_1 = () => player.AIData != null && player.IsAI && player.AIData.IsNoOffsetShooting;
			controller.weaponManagerClass = component.ObjectInHands as WeaponManagerClass;
			controller.Fireport = player.PlayerBones.FindFireport();
			controller.func_2 = controller.method_15;
			controller.MultiBarrelsFireports = player.PlayerBones.FindMultiBarrelsFireports(controller.Item.IsMultiBarrel);
			controller.GunBaseTransform = controller.HandsHierarchy.GetTransform(ECharacterWeaponBones.Weapon_root_anim);
			controller.malfunctionRandom_0 = player.MalfRandoms;
			controller.CCV = component2.ContainerCollectionView;
			controller.gclass2250_0 = player.Skills.GetWeaponInfo(controller.Item);
			controller.gclass849_0 = new GClass849<float>(controller.method_13);
			controller.gclass849_1 = new GClass849<float>(controller.method_12);
			controller.weaponPrefab_0 = component;
			controller.Bipod = (from x in weapon.AllSlots
				where x.ContainedItem is BipodItemClass
				select x.ContainedItem as BipodItemClass).FirstOrDefault();
			controller.weaponManagerClass.Player = player;
			controller.weaponManagerClass.OnSmoothSensetivityChange += controller.method_1;
			controller.weaponManagerClass.OnSmoothScopeStateChanged += controller.method_2;
			if (async)
			{
				await JobScheduler.Yield();
			}
			controller.weaponManagerClass.AfterGetFromPoolInit(player.ProceduralWeaponAnimation, controller.CCV, player.IsYourPlayer);
			if (async)
			{
				await JobScheduler.Yield();
			}
			player.ProceduralWeaponAnimation.ClearPreviousWeapon();
			player.ProceduralWeaponAnimation.method_8(controller, controller.Item, component, controller.gclass2250_0);
			ProceduralWeaponAnimation proceduralWeaponAnimation = player.ProceduralWeaponAnimation;
			proceduralWeaponAnimation.AvailableScopesChanged = (Action)Delegate.Combine(proceduralWeaponAnimation.AvailableScopesChanged, new Action(controller.ValidateCurrentScopeIndex));
			controller.CompositeDisposable.AddDisposable(delegate
			{
				ProceduralWeaponAnimation proceduralWeaponAnimation2 = player.ProceduralWeaponAnimation;
				proceduralWeaponAnimation2.AvailableScopesChanged = (Action)Delegate.Remove(proceduralWeaponAnimation2.AvailableScopesChanged, new Action(controller.ValidateCurrentScopeIndex));
			});
			if (async)
			{
				await JobScheduler.Yield();
			}
			player.ProceduralWeaponAnimation.InitTransforms(controller.HandsHierarchy, controller.CCV);
			player.ProceduralWeaponAnimation.FindMountingPoint(controller.HandsHierarchy);
			if (controller.HasBipod)
			{
				player.ProceduralWeaponAnimation.FindBipodMountingPoint();
			}
			if (async)
			{
				await JobScheduler.Yield();
			}
			controller.weaponManagerClass.SetFireport(controller.CurrentFireport);
			if (async)
			{
				await JobScheduler.Yield();
			}
			controller.weaponManagerClass.SetPointOfView(player.PointOfView);
			controller.firearmsAnimator_0 = component.FirearmsAnimator;
			controller.firearmsAnimator_0.AddEventsConsumer(controller);
			controller.CompositeDisposable.AddDisposable(delegate
			{
				controller.firearmsAnimator_0.RemoveEventsConsumer(controller);
			});
			controller.firearmsAnimator_0.SetPatronInWeaponVisibleEvent += delegate(bool visible)
			{
				controller.weaponManagerClass.SetVisiblePatronInWeapon(visible);
			};
			controller.CompositeDisposable.AddDisposable(delegate
			{
				controller.firearmsAnimator_0.SetPatronInWeaponVisibleEvent -= delegate(bool visible)
				{
					controller.weaponManagerClass.SetVisiblePatronInWeapon(visible);
				};
			});
			controller._player.Skills.WeaponMastered += controller.OnCurrentWeaponBeingMastered;
			controller._player.Skills.OnSkillLevelChanged += controller.method_3;
			controller._player.MovementContext.PhysicalConditionChanged += controller.method_4;
			controller.CompositeDisposable.AddDisposable(delegate
			{
				controller._player.Skills.WeaponMastered -= controller.OnCurrentWeaponBeingMastered;
				controller._player.Skills.OnSkillLevelChanged -= controller.method_3;
				controller._player.MovementContext.PhysicalConditionChanged -= controller.method_4;
			});
			controller.float_3 = weapon.GetTotalCenterOfImpact(includeAmmo: false);
			controller.AimingDevices = (from x in weapon.AllSlots
				where x.ContainedItem is TacticalComboItemClass
				select x.ContainedItem as TacticalComboItemClass).ToArray();
			controller.UpdateHipInaccuracy();
			controller.float_4 = weapon.TotalShotgunDispersion;
			controller.method_18();
			if (async)
			{
				await JobScheduler.Yield();
			}
			controller.SyncWithCharacterSkills();
			controller.InitBallisticCalculator();
			if (async)
			{
				await JobScheduler.Yield();
			}
			controller._player.HandsAnimator = controller.firearmsAnimator_0;
			if (weapon.MalfState.State != Weapon.EMalfunctionState.None)
			{
				component.InitMalfunctionState(weapon, hasPlayer: true, weapon.MalfState.IsKnownMalfunction(player.ProfileId), out var ammoPoolObject);
				controller.weaponManagerClass.SetPatronInShellPort(ammoPoolObject);
			}
			controller.method_6();
			controller.firearmsAnimator_0.SetBoltCatch(active: false);
			controller.firearmsAnimator_0.SkipTime(Time.fixedDeltaTime);
			controller.method_10();
			controller.SetMaxUniqueAnimationModId();
			controller.method_14();
			controller.method_5();
			controller.InitModsAudioControllers(controller);
			for (int num = 0; num < weapon.Chambers.Length; num++)
			{
				Slot slot = weapon.Chambers[num];
				if (slot.ContainedItem != null)
				{
					controller.weaponManagerClass.SetRoundIntoWeapon((AmmoItemClass)slot.ContainedItem, num);
				}
			}
			if (weapon.GetCurrentMagazine() is CylinderMagazineItemClass cylinderMagazineItemClass)
			{
				for (int num2 = 0; num2 < cylinderMagazineItemClass.Camoras.Length; num2++)
				{
					Slot slot2 = cylinderMagazineItemClass.Camoras[num2];
					if (slot2?.ContainedItem != null)
					{
						controller.weaponManagerClass.SetRoundIntoWeapon((AmmoItemClass)slot2.ContainedItem, num2);
					}
				}
			}
			if (weapon.ShellsInChambers != null)
			{
				for (int num3 = 0; num3 < weapon.ShellsInChambers.Length; num3++)
				{
					AmmoTemplate ammoTemplate = weapon.ShellsInChambers[num3];
					if (ammoTemplate != null)
					{
						controller.weaponManagerClass.SetPatronInShellPort(Singleton<PoolManagerClass>.Instance.CreateFromPool<AmmoPoolObject>(ammoTemplate.Prefab), num3);
						controller.firearmsAnimator_0.SetShellsInWeapon(weapon.ShellsInWeaponCount);
					}
				}
			}
			if (async)
			{
				await JobScheduler.Yield();
			}
			controller.weaponManagerClass.InitStreamingAnimators();
		}

		public void ValidateCurrentScopeIndex()
		{
			if (Item.AimIndex.Value >= _player.ProceduralWeaponAnimation.ScopeAimTransforms.Count)
			{
				ChangeAimingMode();
			}
		}

		public void method_1(float sensitivity)
		{
			AimingSmoothSensitivity = sensitivity;
			UpdateSensitivity();
		}

		public void method_2(ESmoothScopeState scopeState)
		{
			_player.RaiseSmoothSightChangeEvent(_player.ProceduralWeaponAnimation.CurrentAimingMod, scopeState);
		}

		public void method_3(AbstractSkillClass obj)
		{
			if (obj.Id == ESkillId.BotReload || obj is MasterSkillClass || ((SkillClass)obj).Class == ESkillClass.Combat)
			{
				SyncWithCharacterSkills();
			}
		}

		public void method_4(EPhysicalCondition condition, EPhysicalCondition full)
		{
			if (condition == EPhysicalCondition.LeftArmDamaged || condition == EPhysicalCondition.RightArmDamaged)
			{
				SetAnimatorAndProceduralValues();
				RecalculateErgonomic();
				_player.ProceduralWeaponAnimation.UpdateWeaponVariables();
			}
		}

		public void OnCurrentWeaponBeingMastered(MasterSkillClass m)
		{
			if (m.MasteringGroup.Templates.Contains(Item.StringTemplateId))
			{
				SyncWithCharacterSkills();
			}
		}

		public float GetWeaponDrawSpeedMultiplier(Weapon weapon, bool useFastDropAnimationSpeed)
		{
			BackendConfigSettingsClass instance = Singleton<BackendConfigSettingsClass>.Instance;
			float num = 0f;
			float num2 = 0f;
			if (weapon.WeapClass.Equals("pistol"))
			{
				num = instance.WeaponFastDrawGlobalSettings.WeaponPistolFastSwitchMaxSpeedMult;
				num2 = instance.WeaponFastDrawGlobalSettings.WeaponPistolFastSwitchMinSpeedMult;
			}
			else
			{
				num = instance.WeaponFastDrawGlobalSettings.WeaponFastSwitchMaxSpeedMult;
				num2 = instance.WeaponFastDrawGlobalSettings.WeaponFastSwitchMinSpeedMult;
			}
			_player.Skills.WeaponSkills.TryGetValue(Item.GetType(), out var value);
			int num3 = value?.Level ?? 0;
			float num4 = num2 + (float)(num3 / 50) * (num - num2);
			if (useFastDropAnimationSpeed)
			{
				num4 /= 1.5f;
			}
			return num4;
		}

		public float GetWeaponReloadAnimationSpeed()
		{
			if (gclass2250_0 == null)
			{
				return 1f;
			}
			return gclass2250_0.ReloadSpeed;
		}

		public bool CheckForFastWeaponSwitch(Item nextControllerItem)
		{
			if (nextControllerItem is Weapon && nextControllerItem.Parent.Container == _player.Equipment.GetSlot(EquipmentSlot.Holster) && _player.FastSlotSelection)
			{
				return !_player.MovementContext.IsInPronePose;
			}
			return false;
		}

		public void SetAnimatorAndProceduralValues()
		{
			float num = GClass1891.PastTime - _player.QuickdrawTime;
			if (_player.Inventory.Equipment.GetSlot(EquipmentSlot.Holster).Equals(Item.Parent.Container) && _player.QuickdrawWeaponFast && num < 1f)
			{
				float fastWeaponSwitchStaminaLack = _player.Physical.FastWeaponSwitchStaminaLack;
				_player.Skills.WeaponSkills.TryGetValue(Item.GetType(), out var value);
				_player.Physical.OnWeaponSwitchFast(value?.Level ?? 0);
				float lackOfStamina = fastWeaponSwitchStaminaLack + _player.Physical.FastWeaponSwitchStaminaLack;
				float fullStaminaCost = Singleton<BackendConfigSettingsClass>.Instance.Stamina.WeaponFastSwitchConsumption * 2f;
				if (_player.Physical.HandsStamina.Current <= 0f)
				{
					_player.ProceduralWeaponAnimation.StartHandShake(lackOfStamina, fullStaminaCost);
				}
				firearmsAnimator_0.SetSpeedParameters(1f, GetWeaponDrawSpeedMultiplier(Item, useFastDropAnimationSpeed: false));
				_player.QuickdrawWeaponFast = false;
			}
			else
			{
				_player.QuickdrawWeaponFast = false;
				if (_player.MovementContext.PhysicalConditionIs(EPhysicalCondition.LeftArmDamaged) || _player.MovementContext.PhysicalConditionIs(EPhysicalCondition.RightArmDamaged))
				{
					firearmsAnimator_0.SetSpeedParameters();
					_player.MovementContext.PlayerAnimator.method_0();
				}
				else
				{
					firearmsAnimator_0.SetSpeedParameters(gclass2250_0.ReloadSpeed, gclass2250_0.SwapSpeed);
					_player.MovementContext.PlayerAnimator.method_0(gclass2250_0.ReloadSpeed, gclass2250_0.SwapSpeed);
				}
			}
		}

		public void SyncWithCharacterSkills()
		{
			firearmsAnimator_0.SetWeaponLevel(CurrentMasteringLevel);
			SkillManager.GClass2250 weaponInfo = _player.Skills.GetWeaponInfo(Item);
			gclass2250_0.AimMovementSpeed = weaponInfo.AimMovementSpeed;
			gclass2250_0.SwapSpeed = weaponInfo.SwapSpeed;
			gclass2250_0.DeltaErgonomics = weaponInfo.DeltaErgonomics;
			gclass2250_0.FixSpeed = weaponInfo.FixSpeed;
			gclass2250_0.RecoilSupression = weaponInfo.RecoilSupression;
			gclass2250_0.ReloadSpeed = weaponInfo.ReloadSpeed;
			gclass2250_0.Maxed = weaponInfo.Maxed;
			gclass2250_0.DoubleActionRecoilReduce = weaponInfo.DoubleActionRecoilReduce;
			SetAnimatorAndProceduralValues();
		}

		public void method_5()
		{
			weaponSoundPlayer_0 = _controllerObject.GetComponent<WeaponSoundPlayer>();
			weaponSoundPlayer_0.Init(this, CurrentFireport, _player);
			weaponSoundPlayer_0.IsSilenced = IsSilenced;
		}

		public void InitModsAudioControllers(FirearmController firearmController)
		{
			foreach (GClass768.GClass769 value in CCV.ContainerBones.Values)
			{
				SetupModAudioController(value.ItemView, firearmController);
			}
		}

		public void SetupModAudioController(Transform modObjectTransform, FirearmController firearmController)
		{
			Class1240 CS_0024_003C_003E8__locals9 = new Class1240();
			CS_0024_003C_003E8__locals9.firearmController = firearmController;
			CS_0024_003C_003E8__locals9.firearmController_0 = this;
			if (!(modObjectTransform != null) || !modObjectTransform.TryGetComponent<GInterface73>(out CS_0024_003C_003E8__locals9.modAudioController))
			{
				return;
			}
			CS_0024_003C_003E8__locals9.modAudioController.Clear();
			_player.OnHandsControllerChanged += delegate(AbstractHandsController oldController, AbstractHandsController newController)
			{
				// Found self-referencing delegate construction. Abort transformation to avoid stack overflow.
				if (!(oldController != CS_0024_003C_003E8__locals9.firearmController))
				{
					CS_0024_003C_003E8__locals9.firearmController_0._player.OnHandsControllerChanged -= CS_0024_003C_003E8__locals9.method_0;
					CS_0024_003C_003E8__locals9.modAudioController?.Clear();
				}
			};
			CS_0024_003C_003E8__locals9.modAudioController.Init(_player.ProfileId, firearmsAnimator_0);
		}

		public void ClearModAudioController(Slot slot)
		{
			GClass768.GClass769 viewForSlot = CCV.GetViewForSlot(slot);
			if (viewForSlot != null && viewForSlot.ItemView != null && viewForSlot.ItemView.TryGetComponent<GInterface73>(out var component))
			{
				component.Clear();
			}
		}

		public void UpdateSensitivity()
		{
			if (_isAiming)
			{
				float num = Item.Template.AimSensitivity;
				SightComponent currentAimingMod = _player.ProceduralWeaponAnimation.CurrentAimingMod;
				if (currentAimingMod != null)
				{
					num = ((!currentAimingMod.AdjustableOpticData.IsAdjustableOptic || !_player.ProceduralWeaponAnimation.CurrentScope.IsOptic) ? currentAimingMod.GetCurrentSensitivity : AimingSmoothSensitivity);
				}
				_aimingSens = num * _player.GetAimingSensitivity();
			}
		}

		public void method_6()
		{
			LauncherItemClass underbarrelWeapon = Item.GetUnderbarrelWeapon();
			if (underbarrelWeapon != null)
			{
				UnderbarrelWeapon = underbarrelWeapon;
				underbarrelManagerClass = new UnderbarrelManagerClass();
				underbarrelManagerClass.Init(_player, this, UnderbarrelWeapon);
			}
			else
			{
				UnderbarrelWeapon = null;
				underbarrelManagerClass = null;
			}
		}

		public void method_7(LauncherItemClass underbarrelWeapon, GameObject _weaponPrefab)
		{
			if (underbarrelWeapon != null)
			{
				UnderbarrelWeapon = underbarrelWeapon;
				underbarrelManagerClass = new UnderbarrelManagerClass();
				underbarrelManagerClass.Init(_player, this, UnderbarrelWeapon, _weaponPrefab);
			}
			else
			{
				UnderbarrelWeapon = null;
				underbarrelManagerClass = null;
			}
		}

		public void method_8()
		{
			ClearPreWarmOperationsDict();
			UnderbarrelWeapon = null;
			underbarrelManagerClass?.Clear();
			underbarrelManagerClass = null;
		}

		public void method_9()
		{
			underbarrelManagerClass?.InitWeaponSoundPlayer();
		}

		public void method_10()
		{
			float_2 = 0f;
			WeaponLn = 0f;
			if (_player.MovementContext.StationaryWeapon != null && _player.MovementContext.StationaryWeapon.Item == Item)
			{
				return;
			}
			if (!(base.WeaponRoot == null) && !(CurrentFireport.Original == null))
			{
				WeaponLn = Vector3.Distance(GunBaseTransform.position, CurrentFireport.Original.position);
				GameObject[] array = weaponManagerClass.MuzzleTransforms();
				foreach (GameObject gameObject in array)
				{
					WeaponLn = Mathf.Max(WeaponLn, Vector3.Distance(GunBaseTransform.position, gameObject.transform.position));
				}
				if (CurrentFireport.Original.lossyScale.y < 1f)
				{
					WeaponLn /= CurrentFireport.Original.lossyScale.y;
				}
				int_0 = LayerMask.NameToLayer("Player");
			}
			else
			{
				UnityEngine.Debug.LogError("No muzzle or Weapon_root. Overlapping disabled");
			}
		}

		public float method_11(Vector3 origin, float ln, ref bool overlapsWithPlayer, Vector3? weaponUp = null)
		{
			Vector3 vector = weaponUp ?? base.WeaponRoot.up;
			Vector3 end = origin - vector * ln;
			if (EFTPhysicsClass.Linecast(origin, end, out var bestHit, EFTHardSettings.Instance.WEAPON_OCCLUSION_LAYERS, reverseCheck: false, raycastHit_0, func_2))
			{
				overlapsWithPlayer = bestHit.collider.gameObject.layer == int_0;
				return ln - bestHit.distance;
			}
			Vector3 lhs = origin - _player.Position;
			Vector3 up = Vector3.up;
			float num = Vector3.Dot(lhs, up);
			if (EFTPhysicsClass.Linecast(_player.Position + num * up, origin, out bestHit, EFTHardSettings.Instance.WEAPON_OCCLUSION_LAYERS, reverseCheck: false, raycastHit_0, func_2))
			{
				overlapsWithPlayer = bestHit.collider.gameObject.layer == int_0;
				return ln;
			}
			return 0f;
		}

		public float method_12()
		{
			if (_player.MovementContext.PhysicalConditionContainsAny(EPhysicalCondition.LeftArmDamaged | EPhysicalCondition.RightArmDamaged))
			{
				return 0f;
			}
			float num = gclass2250_0.DeltaErgonomics;
			if (_player.MovementContext.IsInMountedState)
			{
				num += ((_player.MovementContext.PlayerMountingPointData.MountPointData.MountSideDirection != EMountSideDirection.Forward || !BipodState) ? gclass2250_0.MountingBonusErgo : gclass2250_0.BipodBonusErgo);
			}
			return Mathf.Max(0f, Item.ErgonomicsTotal * (1f + num + _player.ErgonomicsPenalty));
		}

		public float method_13()
		{
			return Item.TotalWeight * (1f - Mathf.Sqrt(TotalErgonomics) / 25f);
		}

		public void ReloadMagNotFound()
		{
			_player.PhraseSituation?.Invoke(EPhraseTrigger.NeedAmmo, 5);
		}

		public void method_14()
		{
			IsSilenced = GClass3380.GetItemComponentsInChildren<SilencerComponent>(Item).Any();
		}

		public void UpdateHipInaccuracy()
		{
			bool flag = AimingDevices.Length != 0 && AimingDevices.Any((TacticalComboItemClass x) => x.Light != null && x.Light.IsActive);
			HipInaccuracy = (flag ? (0.3f - gclass849_1.Value / 400f) : (1f - Mathf.Clamp01(gclass849_1.Value / 250f - 0.15f)));
			_player.ProceduralWeaponAnimation.Breath.HipPenalty = HipInaccuracy;
		}

		public void SetUnderbarrelWeapon()
		{
			method_6();
		}

		public void WeaponModified()
		{
			method_10();
			float_3 = Item.GetTotalCenterOfImpact(includeAmmo: false);
			method_14();
			RecalculateErgonomic();
			SetMaxUniqueAnimationModId();
			Bipod = (from x in Item.AllSlots
				where x.ContainedItem is BipodItemClass
				select x.ContainedItem as BipodItemClass).FirstOrDefault();
			BipodState = BipodState && HasBipod;
			weaponPrefab_0.InitHotObjects(Item);
			_controllerObject.GetComponent<WeaponSoundPlayer>().IsSilenced = IsSilenced;
			_player.ProceduralWeaponAnimation.UpdateWeaponVariables();
			AimingDevices = (from x in Item.AllSlots
				where x.ContainedItem is TacticalComboItemClass
				select x.ContainedItem as TacticalComboItemClass).ToArray();
			UpdateHipInaccuracy();
			_player.ProceduralWeaponAnimation.IsBipodUsed = BipodState;
			float_4 = Item.TotalShotgunDispersion;
		}

		public void RecalculateErgonomic()
		{
			gclass849_1.SetDirty();
			gclass849_0.SetDirty();
			_player.Physical.Aim((!_isAiming || !(_player.MovementContext.StationaryWeapon == null)) ? 0f : ErgonomicWeight);
		}

		public void SetMaxUniqueAnimationModId()
		{
			Mod[] array = (from slot in Item.AllSlots
				where slot.ContainedItem is Mod
				select slot.ContainedItem as Mod).ToArray();
			if (array.Length == 0)
			{
				bool_7 = false;
				return;
			}
			int num = array.Max((Mod mod) => mod.UniqueAnimationModID);
			bool_7 = num > 0;
			FirearmsAnimator.SetUniqueAnimationModId(num);
		}

		public override float GetAnimatorFloatParam(int hash)
		{
			return firearmsAnimator_0.GetAnimatorParameter(hash);
		}

		public bool method_15(RaycastHit overlapHit)
		{
			GameObject gameObject = overlapHit.collider.gameObject;
			if (gameObject.layer == int_0)
			{
				return gameObject == _player.gameObject;
			}
			return false;
		}

		public virtual void WeaponOverlapping()
		{
			if (WeaponLn <= 0f)
			{
				return;
			}
			try
			{
				float num = 1f;
				if (!_player.IsVisible || CurrentOperation is GClass2053)
				{
					return;
				}
				if (bool_12 && !_player._isInventoryOpened)
				{
					float_7 = -1f;
				}
				bool_12 = _player._isInventoryOpened;
				if (float_7 <= 1f)
				{
					float_7 += Time.deltaTime / float_6;
				}
				float weaponOverlapDistanceCulling = Singleton<GClass1706>.Instance.WeaponOverlapDistanceCulling;
				if (weaponOverlapDistanceCulling > 0f && !_player.IsVisibleByCullingObject(weaponOverlapDistanceCulling))
				{
					return;
				}
				Vector3 position = _player.ProceduralWeaponAnimation.HandsContainer.HandsPosition.Get();
				if (_player.ProceduralWeaponAnimation.BlindfireBlender.Value != 0f)
				{
					Vector3 position2 = (_player.ProceduralWeaponAnimation.BlindFireEndPosition + _player.ProceduralWeaponAnimation.PositionZeroSum) * 1.9f;
					position2 = _player.ProceduralWeaponAnimation.HandsContainer.WeaponRootAnim.parent.TransformPoint(position2);
					num = method_11(position2, WeaponLn, ref _player.ProceduralWeaponAnimation.TurnAway.OverlapsWithPlayer);
				}
				if (num >= 0.02f)
				{
					position = ((!_player.MovementContext.LeftStanceEnabled) ? _player.ProceduralWeaponAnimation.HandsContainer.WeaponRootAnim.parent.TransformPoint(position) : _player.ProceduralWeaponAnimation.HandsContainer.WeaponRootAnim.TransformPoint(position));
					num = method_11(position, WeaponLn, ref _player.ProceduralWeaponAnimation.TurnAway.OverlapsWithPlayer);
					if (num > 0f && _player.MovementContext.LeftStanceController.LeftStance && !_player.MovementContext.IsInPronePose && !_player._isInventoryOpened && float_7 > 1f && _player.method_25(PlayerAnimator.LEFT_STANCE_CURVE) >= 1f)
					{
						_player.MovementContext.LeftStanceController.DisableLeftStanceAnimFromHandsAction();
						bool_11 = true;
						float_7 = 0f;
					}
					if (!_player.MovementContext.LeftStanceController.LastAnimValue && _player.MovementContext.LeftStanceController.LeftStance && num <= 0f && bool_11 && !_player.MovementContext.IsInPronePose && !_player._isInventoryOpened && float_7 > 1f && _player.method_25(PlayerAnimator.LEFT_STANCE_CURVE) <= 0f)
					{
						position += -base.WeaponRoot.right * 0.2f;
						if (method_11(position, WeaponLn + 0.2f, ref _player.ProceduralWeaponAnimation.TurnAway.OverlapsWithPlayer) <= 0f)
						{
							_player.MovementContext.LeftStanceController.SetAnimatorLeftStanceToCacheFromHandsAction();
							bool_11 = false;
							float_7 = 0f;
						}
					}
				}
				SetWeaponOverlapValue(num);
				WeaponOverlapView();
			}
			finally
			{
			}
		}

		public void method_16(bool value)
		{
			if (IsAiming && value)
			{
				method_30();
			}
			if (IsBipodsOperation)
			{
				CurrentOperation.FastForward();
			}
			if (FirearmsAnimator != null)
			{
				FirearmsAnimator.SetSprint(value);
			}
			if (value)
			{
				SetTriggerPressed(pressed: false);
			}
		}

		public void method_17(EPlayerState previousstate, EPlayerState nextstate)
		{
			if (!EFTHardSettings.Instance.CanAimInState(nextstate))
			{
				method_30();
			}
		}

		public bool AudioDelegate()
		{
			if (IsTriggerPressed)
			{
				return !Malfunction;
			}
			return false;
		}

		public virtual void InitBallisticCalculator()
		{
			BallisticsCalculator = Singleton<GInterface169>.Instance.CreateBallisticCalculator(0);
		}

		public virtual void SetWeaponOverlapValue(float overlap)
		{
			float_2 = overlap;
		}

		public void WeaponOverlapView()
		{
			if (_player.MovementContext.IsInMountedState && _player.MovementContext.PlayerMountingPointData.TransitionMounting)
			{
				float_2 = 0f;
			}
			Vector3 vector = _player.ProceduralWeaponAnimation.HandsContainer.HandsPosition.Get();
			if (float_2 < 0.02f)
			{
				_player.ProceduralWeaponAnimation.TurnAway.OverlapDepth = float_2;
				_player.ProceduralWeaponAnimation.OverlappingAllowsBlindfire = true;
			}
			else
			{
				_player.ProceduralWeaponAnimation.OverlappingAllowsBlindfire = false;
				_player.ProceduralWeaponAnimation.TurnAway.OriginZShift = vector.y;
				_player.ProceduralWeaponAnimation.TurnAway.OverlapDepth = float_2;
			}
			if (float_2 > EFTHardSettings.Instance.STOP_AIMING_AT && IsAiming)
			{
				ToggleAim();
				AimingInterruptedByOverlap = true;
			}
			else if (float_2 < EFTHardSettings.Instance.STOP_AIMING_AT && _player.ProceduralWeaponAnimation.TurnAway.OverlapValue < 0.2f && AimingInterruptedByOverlap && !IsAiming)
			{
				ToggleAim();
				AimingInterruptedByOverlap = false;
			}
		}

		public void method_18()
		{
			if (Item.WeapFireType.Length > 1)
			{
				firearmsAnimator_0.SetFireMode(Item.SelectedFireMode, skipAnimation: true);
			}
			if (Item.HasChambers)
			{
				firearmsAnimator_0.SetAmmoInChamber(Item.ChamberAmmoCount);
			}
			MagazineItemClass currentMagazine = Item.GetCurrentMagazine();
			firearmsAnimator_0.SetMagInWeapon(currentMagazine != null);
			firearmsAnimator_0.SetAmmoOnMag(currentMagazine?.Count ?? 0);
			firearmsAnimator_0.SetMagTypeCurrent(currentMagazine?.magAnimationIndex ?? (-1));
			firearmsAnimator_0.Fold(Item.Folded);
			if (UnderbarrelWeapon != null)
			{
				firearmsAnimator_0.SetLauncher(isLauncherEnabled: false);
			}
		}

		public override void IEventsConsumerOnWeapIn()
		{
			method_32();
		}

		public override void IEventsConsumerOnWeapOut()
		{
			method_31();
		}

		public override void IEventsConsumerOnThirdAction(int intParam)
		{
			TranslateAnimatorParameter(intParam);
		}

		public override void IEventsConsumerOnAddAmmoInChamber()
		{
			method_34();
		}

		public override void IEventsConsumerOnRemoveShell()
		{
			method_35();
		}

		public override void IEventsConsumerOnShellEject()
		{
			method_36();
		}

		public override void IEventsConsumerOnAddAmmoInMag()
		{
			method_38();
		}

		public override void IEventsConsumerOnDelAmmoFromMag()
		{
			method_37();
		}

		public override void IEventsConsumerOnShowAmmo(bool boolParam)
		{
			method_39(boolParam);
		}

		public override void IEventsConsumerOnDelAmmoChamber()
		{
			method_33();
		}

		public override void IEventsConsumerOnMagIn()
		{
			method_50();
		}

		public override void IEventsConsumerOnMagOut()
		{
			method_47();
		}

		public override void IEventsConsumerOnMagShow()
		{
			method_49();
		}

		public override void IEventsConsumerOnMagHide()
		{
			method_48();
		}

		public override void IEventsConsumerOnOffBoltCatch()
		{
			method_40(isCatched: false);
		}

		public override void IEventsConsumerOnOnBoltCatch()
		{
			method_40(isCatched: true);
		}

		public override void IEventsConsumerOnMalfunctionOff()
		{
			method_41();
		}

		public override void IEventsConsumerOnFiringBullet()
		{
			method_42();
		}

		public override void IEventsConsumerOnFireEnd()
		{
			method_43();
		}

		public override void IEventsConsumerOnIdleStart()
		{
			method_45();
		}

		public override void IEventsConsumerOnUseSecondMagForReload()
		{
			method_24();
		}

		public override void IEventsConsumerOnReplaceSecondMag()
		{
			method_25();
		}

		public override void IEventsConsumerOnPutMagToRig()
		{
			method_26();
		}

		public override void IEventsConsumerOnModChanged()
		{
			method_46();
		}

		public override void IEventsConsumerOnLauncherAppeared()
		{
			method_23();
		}

		public override void IEventsConsumerOnLauncherDisappeared()
		{
			method_22();
		}

		public override void IEventsConsumerOnArm()
		{
			method_51(armed: true);
		}

		public override void IEventsConsumerOnDisarm()
		{
			method_51(armed: false);
		}

		public override void IEventsConsumerOnFoldOn()
		{
			method_19(b: true);
		}

		public override void IEventsConsumerOnFoldOff()
		{
			method_19(b: false);
		}

		public override void IEventsOnBackpackDrop()
		{
			method_44();
		}

		public override void IEventsConsumerOnStartUtilityOperation()
		{
			CurrentOperation?.OnUtilityOperationStartEvent();
		}

		public override void IEventsConsumerOnOnUseProp(bool boolParam)
		{
			SetPropVisibility(boolParam);
		}

		public override void IEventsOnBipodToggle()
		{
			method_52();
		}

		public void method_19(bool b)
		{
			SetCompassState(active: false);
			CurrentOperation?.OnFold(b);
		}

		public override bool CanExecute(GInterface438 operation)
		{
			if (method_20(operation))
			{
				return true;
			}
			if ((CurrentOperation is GClass2037 || CurrentOperation is GClass2040) && !(CurrentOperation is GClass2038))
			{
				return !(CurrentOperation is GClass2041);
			}
			return false;
		}

		public bool method_20(GInterface438 operation)
		{
			if (!(operation is GInterface443 gInterface))
			{
				return true;
			}
			if (!method_21(operation) && !_player.InventoryController.IsAnimatedSlot(gInterface.From1))
			{
				return true;
			}
			return false;
		}

		public override void Execute(GInterface438 operation, Callback callback)
		{
			CurrentOperation.Execute(operation, callback);
		}

		public bool method_21(GInterface438 operation)
		{
			if (!(operation is GInterface443 gInterface))
			{
				return false;
			}
			if (gInterface.Item1 == Item)
			{
				return true;
			}
			if (GClass3380.IsChildOf(gInterface.Item1, Item))
			{
				return true;
			}
			if ((gInterface.From1 != null && GClass3380.IsChildOf(gInterface.From1, Item)) || (gInterface.From1 == null && gInterface.Item1 == Item) || (gInterface.To1 != null && GClass3380.IsChildOf(gInterface.To1, Item)) || (gInterface.To1 == null && gInterface.Item1 == Item))
			{
				return true;
			}
			if (!(operation is GInterface444 gInterface2))
			{
				return false;
			}
			if (gInterface2.Item2 != null && GClass3380.IsChildOf(gInterface2.Item2, Item))
			{
				return true;
			}
			if (gInterface2.From2 != null && GClass3380.IsChildOf(gInterface2.From2, Item))
			{
				return true;
			}
			if (gInterface2.To2 != null && GClass3380.IsChildOf(gInterface2.To2, Item))
			{
				return true;
			}
			return false;
		}

		public void method_22()
		{
			CurrentOperation.LauncherDisappeared();
		}

		public void method_23()
		{
			CurrentOperation.LauncherAppeared();
		}

		public void method_24()
		{
			CurrentOperation.UseSecondMagForReload();
		}

		public void method_25()
		{
			CurrentOperation.ReplaceSecondMag();
		}

		public void method_26()
		{
			CurrentOperation.PutMagToRig();
		}

		public void method_27()
		{
			CurrentOperation.OnJumpOrFall();
		}

		public void method_28()
		{
			CurrentOperation.OnSprintFinished();
		}

		public void method_29()
		{
			CurrentOperation.OnSprintStart();
		}

		public void method_30()
		{
			CurrentOperation.OnAimingDisabled();
		}

		public void method_31()
		{
			CurrentOperation.HideWeaponComplete();
		}

		public void method_32()
		{
			CurrentOperation.WeaponAppeared();
		}

		public void method_33()
		{
			CurrentOperation.RemoveAmmoFromChamber();
		}

		public void method_34()
		{
			CurrentOperation.OnAddAmmoInChamber();
		}

		public void method_35()
		{
			CurrentOperation.OnRemoveShellEvent();
		}

		public void method_36()
		{
			CurrentOperation.OnShellEjectEvent();
		}

		public void method_37()
		{
			UnityEngine.Debug.LogError("Weapon has DelAmmoFromMag event");
		}

		public void method_38()
		{
			CurrentOperation.AddAmmoToMag();
		}

		public void method_39(bool b)
		{
			CurrentOperation.OnShowAmmo(b);
		}

		public void method_40(bool isCatched)
		{
			CurrentOperation.OnOnOffBoltCatchEvent(isCatched);
		}

		public void method_41()
		{
			CurrentOperation.OnMalfunctionOffEvent();
		}

		public void method_42()
		{
			CurrentOperation.OnFireEvent();
		}

		public void method_43()
		{
			CurrentOperation.OnFireEndEvent();
		}

		public void method_44()
		{
			CurrentOperation.OnBackpackDropEvent();
		}

		public void method_45()
		{
			CurrentOperation.OnIdleStartEvent();
		}

		public void method_46()
		{
			CurrentOperation.OnModChanged();
		}

		public void method_47()
		{
			CurrentOperation.OnMagPulledOutFromWeapon();
		}

		public void method_48()
		{
			CurrentOperation.OnMagPuttedToRig();
		}

		public void method_49()
		{
			CurrentOperation.OnMagAppeared();
		}

		public void method_50()
		{
			CurrentOperation.OnMagInsertedToWeapon();
		}

		public void method_51(bool armed)
		{
			firearmsAnimator_0.SetHammerArmed(armed);
			Item.Armed = armed;
		}

		public void method_52()
		{
			CurrentOperation.OnBipodToggleEvent();
		}

		public override void Spawn(float animationSpeed, Action callback)
		{
			firearmsAnimator_0.SetAnimationSpeed(animationSpeed);
			InitiateOperation<GClass2055>().Start(delegate
			{
				callback();
				_player.MovementContext.OnStateChanged += method_17;
				_player.Physical.OnSprintStateChangedEvent += method_16;
			});
		}

		public override void Drop(float animationSpeed, Action callback, bool fastDrop, Item nextControllerItem = null)
		{
			if (base.Destroyed)
			{
				CurrentOperation.HideWeapon(callback, fastDrop, nextControllerItem);
				return;
			}
			base.Destroyed = true;
			_player.MovementContext.OnStateChanged -= method_17;
			_player.Physical.OnSprintStateChangedEvent -= method_16;
			RemoveBallisticCalculator();
			Class1312 inventoryOperation = _player.method_138(Item);
			firearmsAnimator_0.SetAnimationSpeed(animationSpeed);
			Action onHidden = delegate
			{
				inventoryOperation.Confirm();
				callback();
			};
			CurrentOperation.HideWeapon(onHidden, fastDrop, nextControllerItem);
		}

		public virtual void RemoveBallisticCalculator()
		{
			Singleton<GInterface169>.Instance.RemoveBallisticCalculator(Item);
		}

		public override void Destroy()
		{
			action_2?.Invoke();
			action_2 = null;
			weaponPrefab_0 = null;
			CCV?.Dispose();
			CCV = null;
			_player.ProceduralWeaponAnimation.ClearPreviousWeapon();
			weaponManagerClass.OnSmoothSensetivityChange -= method_1;
			weaponManagerClass.OnSmoothScopeStateChanged -= method_2;
			underbarrelManagerClass?.Clear();
			if (firearmsAnimator_0 != null)
			{
				firearmsAnimator_0.SetBoltCatch(active: false);
			}
			base.Destroy();
			firearmsAnimator_0 = null;
			BallisticsCalculator = null;
			weaponManagerClass.ValidateScopeSmoothZoomUpdate(enableUpdate: false);
			AssetPoolObject.ReturnToPool(_controllerObject.gameObject);
		}

		public override bool SupportPickup()
		{
			return true;
		}

		public override void Pickup(bool p)
		{
			CurrentOperation.Pickup(p);
		}

		public override void Interact(bool isInteracting, int actionIndex)
		{
			CurrentOperation.Interact(isInteracting, actionIndex);
		}

		public override bool CanInteract()
		{
			if (!(CurrentOperation is GClass2037) && !(CurrentOperation is GClass2040))
			{
				return CurrentOperation is GClass2028;
			}
			return true;
		}

		public override bool InCanNotBeInterruptedOperation()
		{
			return CurrentOperation.CanNotBeInterrupted();
		}

		public override void Loot(bool p)
		{
			CurrentOperation.Loot(p);
		}

		public override bool IsInInteraction()
		{
			return firearmsAnimator_0.IsInInteraction;
		}

		public override bool IsInInteractionStrictCheck()
		{
			if (!IsInInteraction() && !(firearmsAnimator_0.GetLayerWeight(firearmsAnimator_0.LACTIONS_LAYER_INDEX) >= float.Epsilon))
			{
				return firearmsAnimator_0.Animator.IsInTransition(firearmsAnimator_0.LACTIONS_LAYER_INDEX);
			}
			return true;
		}

		public virtual void UnderbarrelSightingRangeDown()
		{
			CurrentOperation.UnderbarrelSightingRangeDown();
		}

		public virtual void UnderbarrelSightingRangeUp()
		{
			CurrentOperation.UnderbarrelSightingRangeUp();
		}

		public virtual bool IsInLauncherMode()
		{
			if (!(CurrentOperation is GClass2040) && !(CurrentOperation is GClass2034))
			{
				return CurrentOperation is GClass2042;
			}
			return true;
		}

		public virtual bool ToggleLauncher(Action callback = null)
		{
			return CurrentOperation.ToggleLauncher(callback);
		}

		public virtual void ChangeLeftStance()
		{
			if (!Blindfire && !_player.MovementContext.IsInMountedState)
			{
				_player.RemoveLeftHandItem();
				CurrentOperation.ToggleLeftStance();
			}
		}

		public virtual bool ToggleBipod()
		{
			if (!HasBipod)
			{
				return false;
			}
			if (_player.MovementContext.PlayerAnimator.AnimatedInteractions.IsScheduleDenied)
			{
				return false;
			}
			if (_player.MovementContext.IsInMountedState)
			{
				return false;
			}
			if (IsOverlap)
			{
				return false;
			}
			if (_player.IsSprintEnabled)
			{
				return false;
			}
			if (!_player.MovementContext.PlayerAnimator.AnimatedInteractions.CanInteract)
			{
				return false;
			}
			if (!_player.HandsController.FirearmsAnimator.IsIdling())
			{
				return false;
			}
			return CurrentOperation.ToggleBipod();
		}

		public virtual void SetTriggerPressed(bool pressed)
		{
			CurrentOperation.SetTriggerPressed(pressed && method_53());
		}

		public bool method_53()
		{
			bool flag = _player.MovementContext.PlayerAnimatorGetIsVaulting();
			BackendConfigSettingsClass.VaultingGlobalSettings vaultingSettings = Singleton<BackendConfigSettingsClass>.Instance.VaultingSettings;
			IVaultingParameters vaultingParameters = _player.VaultingParameters;
			EVaultingStrategy eVaultingStrategy = (flag ? (_player.MovementContext.PlayerAnimator.GetDoVault() ? EVaultingStrategy.Vault : EVaultingStrategy.Climb) : EVaultingStrategy.None);
			if (eVaultingStrategy == EVaultingStrategy.Vault && flag && vaultingParameters.VaultingHeight > vaultingSettings.MovesSettings.VaultSettings.MaxWithoutHandHeight)
			{
				return false;
			}
			if (eVaultingStrategy == EVaultingStrategy.Climb && flag && vaultingParameters.VaultingHeight > vaultingSettings.MovesSettings.ClimbSettings.MaxWithoutHandHeight)
			{
				return false;
			}
			return true;
		}

		public virtual bool CanPressTrigger()
		{
			return true;
		}

		public virtual void ToggleAim()
		{
			if (!Blindfire)
			{
				_player.RemoveLeftHandItem();
				SetCompassState(active: false);
				SetAim(!IsAiming);
			}
		}

		public virtual void SetAim(int scopeIndex)
		{
			Item.AimIndex.Value = Mathf.Max(0, scopeIndex);
			SetAim(scopeIndex >= 0);
		}

		public virtual void SetAim(bool value)
		{
			if (Blindfire)
			{
				return;
			}
			if (_player.UsedSimplifiedSkeleton)
			{
				_player.MovementContext.PlayerAnimator.SetAiming(value);
			}
			if (_player.MovementContext.IsInMountedState && Mathf.Abs(weaponManagerClass.ProceduralWeaponAnimation.CurrentScope.Rotation) >= EFTHardSettings.Instance.SCOPE_ROTATION_THRESHOLD)
			{
				value = false;
			}
			if (Item.IsOneOff)
			{
				value = false;
			}
			AimingInterruptedByOverlap = false;
			bool isAiming = IsAiming;
			CurrentOperation.SetAiming(value);
			_player.ProceduralWeaponAnimation.CheckShouldMoveWeaponCloser();
			_player.Boolean_0 &= !value;
			if (isAiming != IsAiming)
			{
				_player.ProceduralWeaponAnimation.Shootingg.CurrentRecoilEffect.WeaponRecoilEffect.SetAiming(value);
				_player.method_60(0.2f);
				if (value)
				{
					method_60();
				}
			}
		}

		public override void SetInventoryOpened(bool opened)
		{
			if (opened)
			{
				_player.MovementContext.LeftStanceController.DisableLeftStanceAnimFromOpenInventory();
			}
			else
			{
				_player.MovementContext.LeftStanceController.SetAnimatorLeftStanceToCacheFromCloseInventory();
			}
			CurrentOperation.SetInventoryOpened(opened);
			_player.CurrentManagedState?.OnInventory(opened);
			_player.InventoryOpenRaiseAction(opened);
		}

		public override bool IsInventoryOpen()
		{
			return InventoryOpened;
		}

		public virtual void ChangeAimingMode()
		{
			if (Blindfire || _player._leftHandController.InAction)
			{
				return;
			}
			_player.RemoveLeftHandItem(3f);
			int num = Item.AimIndex.Value + 1;
			if (num >= weaponManagerClass.ProceduralWeaponAnimation.ScopeAimTransforms.Count)
			{
				num = 0;
			}
			if (_player.MovementContext.IsInMountedState)
			{
				while (num != Item.AimIndex.Value && !(Mathf.Abs(weaponManagerClass.ProceduralWeaponAnimation.ScopeAimTransforms[num].Rotation) < EFTHardSettings.Instance.SCOPE_ROTATION_THRESHOLD))
				{
					num++;
					if (num >= weaponManagerClass.ProceduralWeaponAnimation.ScopeAimTransforms.Count)
					{
						num = 0;
					}
				}
				if (num == Item.AimIndex.Value || Mathf.Abs(weaponManagerClass.ProceduralWeaponAnimation.ScopeAimTransforms[num].Rotation) >= EFTHardSettings.Instance.SCOPE_ROTATION_THRESHOLD)
				{
					return;
				}
			}
			Item.AimIndex.Value = num;
			UpdateSensitivity();
			_player.RaiseSightChangedEvent(_player.ProceduralWeaponAnimation.CurrentAimingMod);
		}

		public virtual void ChangeAimingMode(int modeIndex)
		{
			if (!Blindfire)
			{
				Item.AimIndex.Value = modeIndex;
				UpdateSensitivity();
				_player.RaiseSightChangedEvent(_player.ProceduralWeaponAnimation.CurrentAimingMod);
			}
		}

		public virtual bool ChangeFireMode(Weapon.EFireMode fireMode)
		{
			if (Blindfire)
			{
				return false;
			}
			if (_player._leftHandController.InAction)
			{
				return false;
			}
			_player.RemoveLeftHandItem(3f);
			return CurrentOperation.ChangeFireMode(fireMode);
		}

		public virtual bool CheckFireMode()
		{
			if (Blindfire)
			{
				return false;
			}
			if (_player._leftHandController.InAction)
			{
				return false;
			}
			_player.RemoveLeftHandItem(3f);
			if (_player.MovementContext.IsInMountedState)
			{
				_player.MovementContext.StartExitingMountedState();
			}
			return CurrentOperation.CheckFireMode();
		}

		public virtual bool ExamineWeapon()
		{
			if (Blindfire)
			{
				return false;
			}
			if (_player._leftHandController.InAction)
			{
				return false;
			}
			_player.RemoveLeftHandItem(3f);
			if ((!(CurrentOperation is GClass2037) && !(CurrentOperation is GClass2040)) || _player.InventoryController.HasAnyHandsAction())
			{
				return false;
			}
			if (!CurrentOperation.ExamineWeapon())
			{
				return false;
			}
			if (_player.MovementContext.IsInMountedState)
			{
				_player.MovementContext.StartExitingMountedState();
			}
			if ((Item.MalfState.State == Weapon.EMalfunctionState.Jam || Item.MalfState.State == Weapon.EMalfunctionState.Feed) && !Item.MalfState.IsKnownMalfunction(_player.ProfileId))
			{
				firearmsAnimator_0.MisfireSlideUnknown(val: false);
				_player.InventoryController.ExamineMalfunction(Item);
			}
			return true;
		}

		public virtual void RollCylinder(bool rollToZeroCamora)
		{
			if (!Blindfire && !_player._leftHandController.InAction)
			{
				_player.RemoveLeftHandItem(3f);
				CurrentOperation.RollCylinder(null, rollToZeroCamora);
			}
		}

		public virtual bool CheckAmmo()
		{
			if (Blindfire)
			{
				return false;
			}
			if (_player._leftHandController.InAction)
			{
				return false;
			}
			_player.RemoveLeftHandItem(3f);
			_player.MovementContext.PlayerAnimator.AnimatedInteractions.ForceStopInteractions();
			if (_player.MovementContext.PlayerAnimator.AnimatedInteractions.IsInteractionPlaying)
			{
				return false;
			}
			if (_player.MovementContext.IsInMountedState)
			{
				_player.MovementContext.StartExitingMountedState();
			}
			return CurrentOperation.CheckAmmo();
		}

		public virtual bool CheckChamber()
		{
			if (Blindfire)
			{
				return false;
			}
			if (_player._leftHandController.InAction)
			{
				return false;
			}
			if (Item is RocketLauncherItemClass)
			{
				return false;
			}
			_player.RemoveLeftHandItem(3f);
			if (_player.MovementContext.IsInMountedState)
			{
				_player.MovementContext.StartExitingMountedState();
			}
			return CurrentOperation.CheckChamber();
		}

		public virtual void ReloadMag(MagazineItemClass magazine, ItemAddress itemAddress, Callback callback)
		{
			using (GClass4062.BeginSampleWithToken("FirearmController:1076.ReloadMag", "ReloadMag"))
			{
				if (Blindfire)
				{
					return;
				}
				_player.RemoveLeftHandItem(3f);
				_player.MovementContext.PlayerAnimator.AnimatedInteractions.ForceStopInteractions();
				if (!_player.MovementContext.PlayerAnimator.AnimatedInteractions.IsInteractionPlaying)
				{
					if (CanStartReload())
					{
						CurrentOperation.ReloadMag(magazine, itemAddress, callback, null);
					}
					else
					{
						callback?.Fail("Cant StartReload");
					}
				}
			}
		}

		public virtual void QuickReloadMag(MagazineItemClass magazine, Callback callback)
		{
			using (GClass4062.BeginSampleWithToken("FirearmController:1090.QuickReloadMag", "QuickReloadMag"))
			{
				if (!Blindfire)
				{
					_player.RemoveLeftHandItem(3f);
					if (CanStartReload())
					{
						CurrentOperation.QuickReloadMag(magazine, callback, null);
					}
					else
					{
						callback?.Fail("Cant StartReload");
					}
				}
			}
		}

		public virtual void ReloadGrenadeLauncher(AmmoPackReloadingClass foundItem, Callback callback)
		{
			if (!Blindfire)
			{
				_player.RemoveLeftHandItem(3f);
				if (CanStartReload())
				{
					CurrentOperation.ReloadGrenadeLauncher(foundItem, callback);
				}
				else
				{
					callback?.Fail("Cant StartReload");
				}
			}
		}

		public virtual void ReloadCylinderMagazine(AmmoPackReloadingClass ammoPack, Callback callback, bool quickReload = false)
		{
			if (Blindfire)
			{
				return;
			}
			_player.RemoveLeftHandItem(3f);
			if (Item.GetCurrentMagazine() != null)
			{
				if (CanStartReload())
				{
					CurrentOperation.ReloadCylinderMagazine(ammoPack, callback, null, quickReload);
				}
				else
				{
					callback?.Fail("Cant StartReload");
				}
			}
		}

		public virtual void ReloadWithAmmo(AmmoPackReloadingClass ammoPack, Callback callback)
		{
			if (Item.GetCurrentMagazine() == null || Blindfire)
			{
				return;
			}
			_player.RemoveLeftHandItem(3f);
			if (CanStartReload())
			{
				if (Item is RevolverItemClass)
				{
					CurrentOperation.ReloadCylinderMagazine(ammoPack, callback, null);
				}
				else
				{
					CurrentOperation.ReloadWithAmmo(ammoPack, callback, null);
				}
			}
			else
			{
				callback?.Fail("Cant StartReload");
			}
		}

		public virtual void ReloadBarrels(AmmoPackReloadingClass ammoPack, ItemAddress placeToPutContainedAmmoMagazine, Callback callback)
		{
			if (!Blindfire)
			{
				if (CanStartReload() && ammoPack.AmmoCount > 0)
				{
					CurrentOperation.ReloadBarrels(ammoPack, placeToPutContainedAmmoMagazine, callback, null);
				}
				else
				{
					callback?.Fail("Cant StartReload");
				}
			}
		}

		public virtual bool CanStartReload()
		{
			if (Blindfire)
			{
				return false;
			}
			if (_player._leftHandController.InAction)
			{
				return false;
			}
			MagazineItemClass currentMagazine = Item.GetCurrentMagazine();
			if (currentMagazine != null && !_player.InventoryController.Examined(currentMagazine))
			{
				NotificationManagerClass.DisplaySingletonWarningNotification(GClass2348.Localized("Attached magazine is not examined."));
				return false;
			}
			bool flag = (Item.MustBoltBeOpennedForExternalReload || Item.MustBoltBeOpennedForInternalReload) && Item.MalfState.IsAnyMalfExceptMisfire;
			if (Item.MalfState.State == Weapon.EMalfunctionState.Feed || flag)
			{
				_player.HandsController.FirearmsAnimator.MisfireSlideUnknown(val: false);
				_player.InventoryController.ExamineMalfunction(Item);
				return false;
			}
			return CurrentOperation.CanStartReload();
		}

		public virtual bool ShouldForceQuickReload()
		{
			return _player.HealthController?.FindActiveEffect<GInterface360>(EBodyPart.Head) != null;
		}

		public override void ManualUpdate(float deltaTime)
		{
			base.ManualUpdate(deltaTime);
			firearmsAnimator_0?.SetAimAngle(_player.Pitch);
			if (_player.MovementContext.FreefallTime > 0.5f)
			{
				SetAim(value: false);
			}
			bool_5 = true;
		}

		public Dictionary<string, LightComponent> GetCurrentLightStatus()
		{
			return GClass3380.GetComponents<LightComponent>(Item.AllSlots.Select((Slot x) => x.ContainedItem)).ToDictionary((LightComponent x) => x.Item.Id, (LightComponent x) => x);
		}

		public Dictionary<string, SightComponent> GetCurrentScopesStatus()
		{
			return GClass3380.GetComponents<SightComponent>(Item.AllSlots.Select((Slot x) => x.ContainedItem)).ToDictionary((SightComponent x) => x.Item.Id, (SightComponent x) => x);
		}

		public virtual bool SetLightsState(FirearmLightStateStruct[] lightsStates, bool force = false, bool animated = true)
		{
			if (!force && !CurrentOperation.CanChangeLightState(lightsStates))
			{
				return false;
			}
			Dictionary<string, LightComponent> currentLightStatus = GetCurrentLightStatus();
			for (int i = 0; i < lightsStates.Length; i++)
			{
				FirearmLightStateStruct firearmLightStateStruct = lightsStates[i];
				if (currentLightStatus.ContainsKey(firearmLightStateStruct.Id))
				{
					LightComponent lightComponent = currentLightStatus[firearmLightStateStruct.Id];
					lightComponent.IsActive = firearmLightStateStruct.IsActive;
					lightComponent.SelectedMode = firearmLightStateStruct.LightMode;
					if (lightComponent.IsActive && _player.AIData != null)
					{
						_player.AIData.TacticalModeChange(p0: true);
					}
				}
				else
				{
					UnityEngine.Debug.LogErrorFormat("Item {0} doesn't exist in current weapon", firearmLightStateStruct.Id);
				}
			}
			_player.AIData?.TacticalModeChange(currentLightStatus.Any((KeyValuePair<string, LightComponent> x) => x.Value.IsActive));
			CurrentOperation.SetLightsState(lightsStates, force, animated);
			if (BackendConfigAbstractClass.Config.UseSpiritPlayer)
			{
				_player.Spirit.RecheckSwitch();
			}
			UpdateHipInaccuracy();
			return true;
		}

		public override bool CanRemove()
		{
			return CurrentOperation.CanRemove();
		}

		public virtual void SetScopeMode(FirearmScopeStateStruct[] scopeStates)
		{
			if (!CurrentOperation.CanChangeScopeStates(scopeStates))
			{
				return;
			}
			Dictionary<string, SightComponent> currentScopesStatus = GetCurrentScopesStatus();
			for (int i = 0; i < scopeStates.Length; i++)
			{
				FirearmScopeStateStruct firearmScopeStateStruct = scopeStates[i];
				if (currentScopesStatus.ContainsKey(firearmScopeStateStruct.Id))
				{
					SightComponent sightComponent = currentScopesStatus[firearmScopeStateStruct.Id];
					sightComponent.SetScopeMode(firearmScopeStateStruct.ScopeIndexInsideSight, firearmScopeStateStruct.ScopeMode);
					sightComponent.SetSelectedOpticCalibrationPoint(firearmScopeStateStruct.ScopeIndexInsideSight, firearmScopeStateStruct.ScopeCalibrationIndex);
				}
				else
				{
					UnityEngine.Debug.LogError("Item " + firearmScopeStateStruct.Id + " doesn't exist in current weapon");
				}
			}
			_player.RaiseSightChangedEvent(_player.ProceduralWeaponAnimation.CurrentAimingMod);
			UpdateSensitivity();
			CurrentOperation.SetScopeMode(scopeStates);
		}

		public override bool IsHandsProcessing()
		{
			return firearmsAnimator_0.IsHandsProcessing();
		}

		public override void ShowGesture(EInteraction gesture)
		{
			CurrentOperation.ShowGesture(gesture);
			if (Singleton<BotEventHandler>.Instantiated)
			{
				Singleton<BotEventHandler>.Instance.ShowGesture(_player, gesture);
			}
		}

		public override void BlindFire(int b)
		{
			CurrentOperation.BlindFire(b);
		}

		public float method_54(Weapon weapon)
		{
			return weapon.GetBarrelDeviation();
		}

		public List<LightComponent> GetAllLightMods()
		{
			return GClass3380.GetComponents<LightComponent>(Item.AllSlots.Select((Slot slot) => slot.ContainedItem)).ToList();
		}

		public void method_55(AmmoItemClass flareItem)
		{
			Transform transform = TransformHelperClass.FindTransformRecursiveContains(base.WeaponRoot.transform, "fireport");
			InitiateFlare(flareItem, transform.position, -transform.up);
		}

		public void method_56(AmmoItemClass rocketItem)
		{
			Transform transform = TransformHelperClass.FindTransformRecursiveContains(base.WeaponRoot.transform, "fireport");
			Transform smokeport = TransformHelperClass.FindTransformRecursiveContains(base.WeaponRoot.transform, "smokeport");
			InitiateRocket(rocketItem, transform.position, -transform.up, smokeport);
		}

		public void InitiateFlare(AmmoItemClass flareItem, Vector3 shotPosition, Vector3 forward)
		{
			CreateFlareShot(flareItem, shotPosition, forward);
			method_59(weaponSoundPlayer_0, flareItem, shotPosition, forward, multiShot: false);
			weaponManagerClass.PlayShotEffects(_player.IsVisible, _player.SqrCameraDistance);
		}

		public void InitiateRocket(AmmoItemClass rocketItem, Vector3 shotPosition, Vector3 forward, Transform smokeport)
		{
			CreateRocketShot(rocketItem, shotPosition, forward, smokeport);
			method_59(weaponSoundPlayer_0, rocketItem, shotPosition, forward, multiShot: false);
			weaponManagerClass.PlayShotEffects(_player.IsVisible, _player.SqrCameraDistance);
		}

		public virtual void AdjustShotVectors(ref Vector3 position, ref Vector3 direction)
		{
			position -= direction * WeaponLn / 5f;
			if (_player.ProceduralWeaponAnimation.ShotNeedsFovAdjustments && _player.RibcageScaleCurrent < 1f)
			{
				Transform self = HandsHierarchy.Self;
				Vector3 position2 = self.InverseTransformPoint(position);
				Vector3 direction2 = self.InverseTransformDirection(direction);
				position2.z *= _player.RibcageScaleCurrent;
				direction2.z *= _player.RibcageScaleCurrent;
				position = self.TransformPoint(position2);
				direction = self.TransformDirection(direction2).normalized;
			}
		}

		public void method_57(LauncherItemClass launcher, AmmoItemClass ammo)
		{
			Vector3 direction = (func_0() ? _player.LookDirection : underbarrelManagerClass.Fireport.Original.TransformDirection(_player.LocalShotDirection));
			Vector3 position = (func_0() ? _player.AIData.BotOwner.LookSensor.ShootStartPos : underbarrelManagerClass.Fireport.position);
			float ammoFactor = ammo.AmmoFactor;
			float num = 1f;
			AdjustShotVectors(ref position, ref direction);
			ammo.buckshotDispersion = launcher.TotalShotgunDispersion;
			float barrelDeviation = launcher.GetBarrelDeviation();
			Vector3 shotDirection = direction * 100f + launcher.CenterOfImpact * ammoFactor * num * barrelDeviation * UnityEngine.Random.insideUnitSphere;
			InitiateShot(launcher, ammo, position, shotDirection.normalized, position, 0, 0f);
			float num2 = 1f;
			float_5 = num2 + (float)ammo.ammoRec / 100f;
			method_59(underbarrelManagerClass.WeaponSoundPlayer, ammo, position, shotDirection, multiShot: false);
			if (ammo.AmmoTemplate.IsLightAndSoundShot)
			{
				method_62(position, direction);
				LightAndSoundShot(position, direction, ammo.AmmoTemplate);
			}
		}

		public void method_58(Weapon weapon, AmmoItemClass ammo, int chamberIndex, bool multiShot = false)
		{
			Transform original = CurrentFireport.Original;
			Vector3 position = CurrentFireport.position;
			Vector3 direction = (func_0() ? _player.LookDirection : WeaponDirection);
			Vector3 position2 = (func_0() ? _player.AIData.BotOwner.LookSensor.ShootStartPos : position);
			float ammoFactor = ammo.AmmoFactor;
			float num = 1f;
			BackendConfigSettingsClass instance = Singleton<BackendConfigSettingsClass>.Instance;
			AdjustShotVectors(ref position2, ref direction);
			ammo.buckshotDispersion = float_4;
			CurrentChamberIndex = chamberIndex;
			weapon.OnShot(ammo.DurabilityBurnModificator, ammo.HeatFactor, _player.Skills.WeaponDurabilityLosOnShotReduce.Value, instance.Overheat, GClass1891.PastTime);
			float overheatProblemsStart = instance.Overheat.OverheatProblemsStart;
			if (weapon.MalfState.LastShotOverheat >= overheatProblemsStart)
			{
				num = Mathf.Lerp(1f, instance.Overheat.MaxCOIIncreaseMult, (weapon.MalfState.LastShotOverheat - overheatProblemsStart) / (instance.Overheat.MaxOverheat - overheatProblemsStart));
			}
			int num2;
			if (multiShot)
			{
				num2 = ((chamberIndex > 0) ? 1 : 0);
				if (num2 != 0)
				{
					float x = UnityEngine.Random.Range(weaponPrefab_0.DupletAccuracyPenaltyX.x, weaponPrefab_0.DupletAccuracyPenaltyX.y);
					float y = UnityEngine.Random.Range(weaponPrefab_0.DupletAccuracyPenaltyY.x, weaponPrefab_0.DupletAccuracyPenaltyY.y);
					Vector3 vector = new Vector3(x, y);
					float angle = vector.y * -1f;
					direction = Quaternion.AngleAxis(vector.x, original.forward) * direction;
					direction = Quaternion.AngleAxis(angle, original.right) * direction;
				}
			}
			else
			{
				num2 = 0;
			}
			float num3 = (weapon.CylinderHammerClosed ? (weapon.DoubleActionAccuracyPenalty * (1f - gclass2250_0.DoubleActionRecoilReduce) * weapon.StockDoubleActionAccuracyPenaltyMult) : 0f);
			float num4 = method_54(weapon);
			double num5 = weapon.GetItemComponent<BuffComponent>().WeaponSpread;
			if (GClass855.ApproxEquals(num5, 0.0))
			{
				num5 = 1.0;
			}
			Vector3 shotDirection = (func_1() ? direction : (direction * 100f + (float_3 + num3) * ammoFactor * num * num4 * (float)num5 * UnityEngine.Random.insideUnitSphere));
			InitiateShot(weapon, ammo, position2, shotDirection.normalized, position, chamberIndex, weapon.MalfState.LastShotOverheat);
			float num6 = ((num2 != 0) ? 1.5f : 1f);
			float_5 = num6 + (float)ammo.ammoRec / 100f;
			method_59(weaponSoundPlayer_0, ammo, position2, shotDirection, multiShot);
			if (ammo.AmmoTemplate.IsLightAndSoundShot)
			{
				method_62(position, direction);
				LightAndSoundShot(position, direction, ammo.AmmoTemplate);
			}
			if (_player.IsAI)
			{
				_player.AIData?.BotOwner?.Memory?.GoalEnemy?.SetLastShootTime();
			}
		}

		public void method_59(WeaponSoundPlayer weaponSoundPlayer, AmmoItemClass ammo, Vector3 shotPosition, Vector3 shotDirection, bool multiShot)
		{
			if (Item.FireMode.FireMode != Weapon.EFireMode.burst || Item.FireMode.BurstShotsCount != 2 || IsBirstOf2Start || Item.ChamberAmmoCount <= 0)
			{
				float pitchMult = method_61();
				weaponSoundPlayer.FireBullet(ammo, shotPosition, shotDirection.normalized, pitchMult, Malfunction, multiShot, IsBirstOf2Start);
			}
		}

		public void method_60()
		{
			float volume = CalculateAimingSoundVolume();
			weaponSoundPlayer_0.PlayAimingSound(volume);
		}

		public float CalculateAimingSoundVolume()
		{
			float num = TotalErgonomics / 100f - 1f;
			float num2 = Mathf.Clamp(num * num, 0.1f, 0.2f) * (1f - (float)_player.Skills.DrawSound);
			float num3 = 1f - (float)_player.Skills.BotSoundCoef;
			return num2 * _player.MovementContext.CovertEquipmentNoise * num3;
		}

		public float method_61()
		{
			float num = 1f;
			float overheatFirerateMult = Item.MalfState.OverheatFirerateMult;
			if (Item.FireMode.FireMode == Weapon.EFireMode.fullauto && overheatFirerateMult > 0f)
			{
				float num2 = 60f / (float)Item.FireRate;
				return 60f / ((float)Item.FireRate * overheatFirerateMult) / num2;
			}
			return 1f + UnityEngine.Random.Range(-0.03f, 0.03f);
		}

		public virtual void DryShot(int chamberIndex = 0, bool underbarrelShot = false)
		{
			SetCompassState(active: false);
		}

		public virtual void ShotMisfired(AmmoItemClass ammo, Weapon.EMalfunctionState malfunctionState, float overheat)
		{
		}

		public virtual void RegisterShot(Item weapon, EftBulletClass shot)
		{
		}

		public virtual void InitiateShot(IWeapon weapon, AmmoItemClass ammo, Vector3 shotPosition, Vector3 shotDirection, Vector3 fireportPosition, int chamberIndex, float overheat)
		{
			_player.OnMakingShot(weapon, _player.PlayerBones.WeaponRoot.position - shotPosition);
			if (ammo.InitialSpeed > 0f)
			{
				if (ammo.ProjectileCount == 1)
				{
					EftBulletClass shot = BallisticsCalculator.Shoot(ammo, shotPosition, shotDirection, _player.ProfileId, weapon.Item, weapon.SpeedFactor, 0);
					RegisterShot(weapon.Item, shot);
				}
				else
				{
					list_0.Clear();
					BallisticsCalculator.ShotMultiProjectileShot(ammo, shotPosition, shotDirection, weapon.SpeedFactor, list_0, _player.ProfileId, weapon.Item);
					foreach (EftBulletClass item in list_0)
					{
						RegisterShot(weapon.Item, item);
					}
					list_0.Clear();
				}
			}
			action_0?.Invoke();
			bool_6 = true;
			if (!_player.IsAI)
			{
				_player.OnStatisticsShot?.Invoke(weapon.Item, ammo);
			}
			if (weapon.IsUnderbarrelWeapon)
			{
				underbarrelManagerClass.FirearmsEffects.StartFireEffects(_player.IsVisible, _player.SqrCameraDistance);
			}
			else
			{
				weaponManagerClass.PlayShotEffects(_player.IsVisible, _player.SqrCameraDistance);
			}
		}

		public virtual void SendStartOneShotFire()
		{
		}

		public virtual void CreateFlareShot(AmmoItemClass flareItem, Vector3 shotPosition, Vector3 forward)
		{
			AmmoPoolObject ammoPoolObject = UnityEngine.Object.Instantiate(GClass1857.GetAsset<AmmoPoolObject>(Singleton<IEasyAssets>.Instance, flareItem.Template.Prefab));
			ammoPoolObject.transform.position = shotPosition;
			ammoPoolObject.transform.forward = forward;
			ammoPoolObject.gameObject.SetActive(value: true);
			FlareCartridge flareCartridge = ammoPoolObject.GetComponent<FlareCartridge>();
			if (flareCartridge == null)
			{
				flareCartridge = ammoPoolObject.gameObject.AddComponent<FlareCartridge>();
			}
			FlareCartridgeSettings flareCartridgeSettings = ammoPoolObject.GetComponent<FlareCartridgeSettings>();
			if (flareCartridgeSettings == null)
			{
				flareCartridgeSettings = ammoPoolObject.gameObject.AddComponent<FlareCartridgeSettings>();
			}
			flareCartridge.Init(flareCartridgeSettings, _player, flareItem, Item);
			flareCartridge.Launch();
			Singleton<GInterface169>.Instance.RegisterGrenade(flareCartridge);
			bool_6 = true;
		}

		public virtual void CreateRocketShot(AmmoItemClass rocketItem, Vector3 shotPosition, Vector3 forward, Transform smokeport = null)
		{
			AmmoPoolObject ammoPoolObject = Singleton<PoolManagerClass>.Instance.CreateFromPool<AmmoPoolObject>(rocketItem.Template.Prefab);
			ammoPoolObject.transform.position = shotPosition;
			ammoPoolObject.transform.forward = forward;
			ammoPoolObject.gameObject.SetActive(value: true);
			RocketProjectile rocketProjectile = ammoPoolObject.GetComponent<RocketProjectile>();
			if (rocketProjectile == null)
			{
				rocketProjectile = ammoPoolObject.gameObject.AddComponent<RocketProjectile>();
			}
			RocketSettings rocketSettings = ammoPoolObject.GetComponent<RocketSettings>();
			if (rocketSettings == null)
			{
				rocketSettings = ammoPoolObject.gameObject.AddComponent<RocketSettings>();
			}
			rocketProjectile.Initialize(rocketSettings, _player, rocketItem, Item, smokeport);
			rocketProjectile.Launch();
			action_3?.Invoke(rocketProjectile);
			bool_6 = true;
		}

		public override void OnAimReady()
		{
			base.OnAimReady();
			if (Weapon is RocketLauncherItemClass)
			{
				FirearmsAnimator.SetAimingFloat(1f);
			}
		}

		public override void OnIdleReady()
		{
			base.OnIdleReady();
			if (Weapon is RocketLauncherItemClass)
			{
				FirearmsAnimator.SetAimingFloat(0f);
			}
		}

		public override void OnDropWeapon()
		{
			CurrentOperation.OnDropWeapon();
		}

		public override void ManualLateUpdate(float deltaTime)
		{
			if (!BackendConfigAbstractClass.Config.UseSpiritPlayer || !_player.Spirit.IsActive)
			{
				if (bool_5)
				{
					WeaponOverlapping();
					bool_5 = false;
				}
				if (bool_6)
				{
					bool_6 = false;
					_player.ProceduralWeaponAnimation.Shoot(float_5);
				}
			}
		}

		public override void OnPlayerDead()
		{
			GClass4062.ReleaseBeginSample("FirearmController.OnPlayerDead", "OnPlayerDead");
			action_2?.Invoke();
			CurrentOperation.FastForward();
			RemoveBallisticCalculator();
			base.OnPlayerDead();
		}

		public override void FastForwardCurrentState()
		{
			CurrentOperation.FastForward();
			base.FastForwardCurrentState();
		}

		public bool IsInSpawnOperation()
		{
			if (CurrentOperation != null)
			{
				return CurrentOperation is GClass2055;
			}
			return true;
		}

		public bool IsInReloadOperation()
		{
			if (!(CurrentOperation is GClass2015) && !(CurrentOperation is GClass2039) && !(CurrentOperation is GClass2045) && !(CurrentOperation is GClass2050))
			{
				return CurrentOperation is GClass2044;
			}
			return true;
		}

		public bool IsInRemoveOperation()
		{
			if (CurrentOperation != null)
			{
				return CurrentOperation is GClass2053;
			}
			return true;
		}

		public virtual void OpticCalibrationSwitchUp(FirearmScopeStateStruct[] scopeStates)
		{
			weaponManagerClass.OpticCalibrationSwitchUp();
		}

		public virtual void OpticCalibrationSwitchDown(FirearmScopeStateStruct[] scopeStates)
		{
			weaponManagerClass.OpticCalibrationSwitchDown();
		}

		public bool HasScopeAimBone(SightComponent sightComp)
		{
			List<ProceduralWeaponAnimation.SightNBone> scopeAimTransforms = _player.ProceduralWeaponAnimation.ScopeAimTransforms;
			int num = 0;
			while (true)
			{
				if (num < scopeAimTransforms.Count)
				{
					if (scopeAimTransforms[num].Mod != null && scopeAimTransforms[num].Mod.Equals(sightComp))
					{
						break;
					}
					num++;
					continue;
				}
				return false;
			}
			return true;
		}

		public override void SetCompassState(bool active)
		{
			if (CanChangeCompassState(active))
			{
				CurrentOperation.SetFirearmCompassState(active);
			}
		}

		public void method_62(Vector3 point, Vector3 direction)
		{
			Singleton<Effects>.Instance.EmitGrenade("Flashbang", point, direction);
		}

		public void method_63(bool isAiming)
		{
			_player.MovementContext.PlayerAnimator.SetAiming(isAiming);
			if (!(Weapon is RocketLauncherItemClass))
			{
				FirearmsAnimator.SetAiming(isAiming);
			}
			FirearmsAnimator.SetAimingIn(isAiming);
			FirearmsAnimator.SetAimingOut(!isAiming);
			FirearmsAnimator.SetPrevAimingFloat(!isAiming);
			bool_0 = true;
		}

		public void method_64()
		{
			FirearmsAnimator.SetAimingIn(aimingIn: false);
			FirearmsAnimator.SetAimingOut(aimingOut: false);
			FirearmsAnimator.SetPrevAimingFloat(isAiming: false);
			bool_0 = false;
		}

		public virtual void LightAndSoundShot(Vector3 point, Vector3 direction, AmmoTemplate ammoTemplate)
		{
			_player.ActiveHealthController?.DoContusion(ammoTemplate.LightAndSoundShotSelfContusionTime, ammoTemplate.LightAndSoundShotSelfContusionStrength);
			Vector3 blindness = ammoTemplate.Blindness;
			float y = blindness.y;
			Collider[] array = Physics.OverlapSphere(point, y, LayerMaskClass.PlayerMask);
			List<IPlayerOwner> list = null;
			Dictionary<IPlayerOwner, GStruct230> dictionary = null;
			float num = Mathf.Cos(ammoTemplate.LightAndSoundShotAngle * 0.5f * (MathF.PI / 180f));
			Collider[] array2 = array;
			foreach (Collider col in array2)
			{
				IPlayerOwner alivePlayerBridgeByCollider = Singleton<GameWorld>.Instance.GetAlivePlayerBridgeByCollider(col);
				if (alivePlayerBridgeByCollider != null && !(alivePlayerBridgeByCollider.iPlayer.ProfileId == _player.ProfileId))
				{
					if (list == null)
					{
						list = new List<IPlayerOwner>();
						dictionary = new Dictionary<IPlayerOwner, GStruct230>();
					}
					Vector3 vector = alivePlayerBridgeByCollider.iPlayer.PlayerBones.Head.position - point;
					Vector3 normalized = vector.normalized;
					float magnitude = vector.magnitude;
					bool flag = Vector3.Dot(direction, normalized) >= num;
					list.Add(alivePlayerBridgeByCollider);
					dictionary.Add(alivePlayerBridgeByCollider, new GStruct230
					{
						Distance = magnitude,
						DirectionToEmitter = -normalized,
						TryToApplyStun = flag,
						TryToApplyBurnEyes = flag,
						TryToApplyContusion = true
					});
				}
			}
			if (list != null)
			{
				GClass2080.ApplyLightAndSoundHealthEffects(list, dictionary, point, blindness, ammoTemplate.Contusion);
			}
		}

		public bool method_65()
		{
			Slot slot = _player.Equipment.GetSlot(EquipmentSlot.Holster);
			if (Weapon.Parent.Container != slot && Weapon.GetCurrentMagazineCount() == 0 && Weapon.ChamberAmmoCount == 0 && slot.ContainedItem is Weapon weapon && weapon.MalfState.State == Weapon.EMalfunctionState.None)
			{
				return weapon.ChamberAmmoCount != 0;
			}
			return false;
		}

		public float GetTotalMalfunctionChance(AmmoItemClass ammoToFire, float overheat, out double durabilityMalfChance, out float magMalfChance, out float ammoMalfChance, out float overheatMalfChance, out float weaponDurability)
		{
			durabilityMalfChance = 0.0;
			magMalfChance = 0f;
			ammoMalfChance = 0f;
			overheatMalfChance = 0f;
			weaponDurability = 0f;
			if (!Item.AllowMalfunction)
			{
				return 0f;
			}
			BackendConfigSettingsClass instance = Singleton<BackendConfigSettingsClass>.Instance;
			BackendConfigSettingsClass.GClass1738 malfunction = instance.Malfunction;
			BackendConfigSettingsClass.GClass1739 overheat2 = instance.Overheat;
			BackendConfigSettingsClass.GClass1788 troubleShooting = instance.SkillsSettings.TroubleShooting;
			float ammoMalfChanceMult = malfunction.AmmoMalfChanceMult;
			float magazineMalfChanceMult = malfunction.MagazineMalfChanceMult;
			MagazineItemClass currentMagazine = Item.GetCurrentMagazine();
			magMalfChance = ((currentMagazine == null) ? 0f : (currentMagazine.MalfunctionChance * magazineMalfChanceMult));
			ammoMalfChance = ((ammoToFire != null) ? ((ammoToFire.MalfMisfireChance + ammoToFire.MalfFeedChance) * ammoMalfChanceMult) : 0f);
			float num = Item.Repairable.Durability / (float)Item.Repairable.TemplateDurability * 100f;
			weaponDurability = Mathf.Floor(num);
			if (overheat >= overheat2.OverheatProblemsStart)
			{
				overheatMalfChance = Mathf.Lerp(overheat2.MinMalfChance, overheat2.MaxMalfChance, (overheat - overheat2.OverheatProblemsStart) / (overheat2.MaxOverheat - overheat2.OverheatProblemsStart));
			}
			overheatMalfChance *= (float)Item.Buff.MalfunctionProtections;
			if (weaponDurability > 59f)
			{
				durabilityMalfChance = (Math.Pow(Item.BaseMalfunctionChance + 1f, 3.0 + (double)(100f - weaponDurability) / (20.0 - 10.0 / Math.Pow((double)Item.FireRate / 10.0, 0.322))) - 1.0) / 1000.0;
			}
			else
			{
				durabilityMalfChance = (Math.Pow(Item.BaseMalfunctionChance + 1f, Math.Log10(Math.Pow(101f - weaponDurability, (50.0 - Math.Pow(weaponDurability, 1.286) / 4.8) / (Math.Pow(Item.FireRate, 0.17) / 2.9815 + 2.1)))) - 1.0) / 1000.0;
			}
			durabilityMalfChance *= (float)Item.Buff.MalfunctionProtections;
			if (Item.MalfState.HasMalfReduceChance(_player.ProfileId, Weapon.EMalfunctionSource.Durability))
			{
				durabilityMalfChance *= troubleShooting.EliteDurabilityChanceReduceMult;
			}
			if (Item.MalfState.HasMalfReduceChance(_player.ProfileId, Weapon.EMalfunctionSource.Magazine))
			{
				magMalfChance *= troubleShooting.EliteMagChanceReduceMult;
			}
			if (Item.MalfState.HasMalfReduceChance(_player.ProfileId, Weapon.EMalfunctionSource.Ammo))
			{
				ammoMalfChance *= troubleShooting.EliteAmmoChanceReduceMult;
			}
			if (num >= malfunction.DurRangeToIgnoreMalfs.x && num <= malfunction.DurRangeToIgnoreMalfs.y)
			{
				durabilityMalfChance = 0.0;
				ammoMalfChance = 0f;
				magMalfChance = 0f;
			}
			durabilityMalfChance = Mathf.Clamp01((float)durabilityMalfChance);
			return Mathf.Clamp01((float)Math.Round(durabilityMalfChance + (double)((ammoMalfChance + magMalfChance + overheatMalfChance) / 1000f), 5));
		}

		public float GetNextMalfunctionRandom()
		{
			return malfunctionRandom_0.GetNextRandom();
		}

		public void GetMalfunctionSources(List<GClass820<Weapon.EMalfunctionSource>.GStruct49<float, Weapon.EMalfunctionSource>> result, double durabilityMalfChance, float magMalfChance, float ammoMalfChance, float overheatMalfChance, bool hasAmmoInMag, bool isMagazineInserted)
		{
			result.Clear();
			result.Add(new GClass820<Weapon.EMalfunctionSource>.GStruct49<float, Weapon.EMalfunctionSource>((float)durabilityMalfChance, Weapon.EMalfunctionSource.Durability));
			if (ammoMalfChance > 0f)
			{
				result.Add(new GClass820<Weapon.EMalfunctionSource>.GStruct49<float, Weapon.EMalfunctionSource>(ammoMalfChance / 1000f, Weapon.EMalfunctionSource.Ammo));
			}
			if (magMalfChance > 0f && hasAmmoInMag && isMagazineInserted)
			{
				result.Add(new GClass820<Weapon.EMalfunctionSource>.GStruct49<float, Weapon.EMalfunctionSource>(magMalfChance / 1000f, Weapon.EMalfunctionSource.Magazine));
			}
			if (overheatMalfChance > 0f)
			{
				result.Add(new GClass820<Weapon.EMalfunctionSource>.GStruct49<float, Weapon.EMalfunctionSource>(overheatMalfChance / 1000f, Weapon.EMalfunctionSource.Overheat));
			}
		}

		public void GetSpecificMalfunctionVariants(List<GClass820<Weapon.EMalfunctionState>.GStruct49<float, Weapon.EMalfunctionState>> result, AmmoItemClass ammo, Weapon.EMalfunctionSource malfunctionSource, float weaponDurability, bool hasAmmoInMag, bool isMagazineInserted, bool shouldCheckJam)
		{
			result.Clear();
			BackendConfigSettingsClass.GClass1738 malfunction = Singleton<BackendConfigSettingsClass>.Instance.Malfunction;
			switch (malfunctionSource)
			{
			case Weapon.EMalfunctionSource.Durability:
				if (hasAmmoInMag && isMagazineInserted && Item.AllowFeed)
				{
					result.Add(new GClass820<Weapon.EMalfunctionState>.GStruct49<float, Weapon.EMalfunctionState>(malfunction.DurFeedWt, Weapon.EMalfunctionState.Feed));
				}
				if (Item.AllowMisfire)
				{
					result.Add(new GClass820<Weapon.EMalfunctionState>.GStruct49<float, Weapon.EMalfunctionState>(malfunction.DurMisfireWt, Weapon.EMalfunctionState.Misfire));
				}
				if (shouldCheckJam && Item.AllowJam)
				{
					result.Add(new GClass820<Weapon.EMalfunctionState>.GStruct49<float, Weapon.EMalfunctionState>(malfunction.DurJamWt, Weapon.EMalfunctionState.Jam));
				}
				if (hasAmmoInMag && Item.AllowSlide)
				{
					result.Add(new GClass820<Weapon.EMalfunctionState>.GStruct49<float, Weapon.EMalfunctionState>(malfunction.DurSoftSlideWt, Weapon.EMalfunctionState.SoftSlide));
				}
				if (weaponDurability <= 5f && hasAmmoInMag && Item.AllowSlide)
				{
					float first3 = Mathf.Lerp(malfunction.DurHardSlideMinWt, malfunction.DurHardSlideMaxWt, 1f - weaponDurability / 5f);
					result.Add(new GClass820<Weapon.EMalfunctionState>.GStruct49<float, Weapon.EMalfunctionState>(first3, Weapon.EMalfunctionState.HardSlide));
				}
				break;
			case Weapon.EMalfunctionSource.Ammo:
				if (Item.AllowMisfire)
				{
					float first2 = malfunction.AmmoMisfireWt / (ammo.MalfMisfireChance + ammo.MalfFeedChance);
					result.Add(new GClass820<Weapon.EMalfunctionState>.GStruct49<float, Weapon.EMalfunctionState>(first2, Weapon.EMalfunctionState.Misfire));
				}
				if (hasAmmoInMag && isMagazineInserted && Item.AllowFeed)
				{
					result.Add(new GClass820<Weapon.EMalfunctionState>.GStruct49<float, Weapon.EMalfunctionState>(malfunction.AmmoFeedWt, Weapon.EMalfunctionState.Feed));
				}
				if (shouldCheckJam && Item.AllowJam)
				{
					result.Add(new GClass820<Weapon.EMalfunctionState>.GStruct49<float, Weapon.EMalfunctionState>(malfunction.AmmoJamWt, Weapon.EMalfunctionState.Jam));
				}
				break;
			case Weapon.EMalfunctionSource.Magazine:
				if (hasAmmoInMag && isMagazineInserted && Item.AllowFeed)
				{
					result.Add(new GClass820<Weapon.EMalfunctionState>.GStruct49<float, Weapon.EMalfunctionState>(1f, Weapon.EMalfunctionState.Feed));
				}
				break;
			case Weapon.EMalfunctionSource.Overheat:
				if (hasAmmoInMag && isMagazineInserted && Item.AllowFeed)
				{
					result.Add(new GClass820<Weapon.EMalfunctionState>.GStruct49<float, Weapon.EMalfunctionState>(malfunction.OverheatFeedWt, Weapon.EMalfunctionState.Feed));
				}
				if (shouldCheckJam && Item.AllowJam)
				{
					result.Add(new GClass820<Weapon.EMalfunctionState>.GStruct49<float, Weapon.EMalfunctionState>(malfunction.OverheatJamWt, Weapon.EMalfunctionState.Jam));
				}
				if (hasAmmoInMag && Item.AllowSlide)
				{
					result.Add(new GClass820<Weapon.EMalfunctionState>.GStruct49<float, Weapon.EMalfunctionState>(malfunction.OverheatSoftSlideWt, Weapon.EMalfunctionState.SoftSlide));
				}
				if (weaponDurability <= 5f && hasAmmoInMag && Item.AllowSlide)
				{
					float first = Mathf.Lerp(malfunction.OverheatHardSlideMinWt, malfunction.OverheatHardSlideMaxWt, 1f - weaponDurability / 5f);
					result.Add(new GClass820<Weapon.EMalfunctionState>.GStruct49<float, Weapon.EMalfunctionState>(first, Weapon.EMalfunctionState.HardSlide));
				}
				break;
			case Weapon.EMalfunctionSource.Ammo | Weapon.EMalfunctionSource.Magazine:
				break;
			}
		}

		public Weapon.EMalfunctionState GetMalfunctionState(AmmoItemClass ammoToFire, bool hasAmmoInMag, bool doesWeaponHaveBoltCatch, bool isMagazineInserted, float overheat, float fixSlideOverheat, out Weapon.EMalfunctionSource malfunctionSource)
		{
			malfunctionSource = Weapon.EMalfunctionSource.Durability;
			if (!Item.AllowMalfunction)
			{
				return Weapon.EMalfunctionState.None;
			}
			if (Item.MalfState.SlideOnOverheatReached && overheat > fixSlideOverheat && Item.AllowSlide && hasAmmoInMag)
			{
				malfunctionSource = Weapon.EMalfunctionSource.Overheat;
				return Weapon.EMalfunctionState.SoftSlide;
			}
			double durabilityMalfChance;
			float magMalfChance;
			float ammoMalfChance;
			float overheatMalfChance;
			float weaponDurability;
			float totalMalfunctionChance = GetTotalMalfunctionChance(ammoToFire, overheat, out durabilityMalfChance, out magMalfChance, out ammoMalfChance, out overheatMalfChance, out weaponDurability);
			float num = 0f;
			num = malfunctionRandom_0.GetRandomFloat();
			if (num > totalMalfunctionChance)
			{
				return Weapon.EMalfunctionState.None;
			}
			List<GClass820<Weapon.EMalfunctionSource>.GStruct49<float, Weapon.EMalfunctionSource>> list = list_1;
			GetMalfunctionSources(list, durabilityMalfChance, magMalfChance, ammoMalfChance, overheatMalfChance, hasAmmoInMag, isMagazineInserted);
			malfunctionSource = GClass820<Weapon.EMalfunctionSource>.GenerateDrop(list, num);
			bool shouldCheckJam = hasAmmoInMag || !doesWeaponHaveBoltCatch || !isMagazineInserted;
			List<GClass820<Weapon.EMalfunctionState>.GStruct49<float, Weapon.EMalfunctionState>> list2 = list_2;
			GetSpecificMalfunctionVariants(list2, ammoToFire, malfunctionSource, weaponDurability, hasAmmoInMag, isMagazineInserted, shouldCheckJam);
			if (list2.Count == 0)
			{
				return Weapon.EMalfunctionState.None;
			}
			return GClass820<Weapon.EMalfunctionState>.GenerateDrop(list2);
		}

		public override Dictionary<Type, OperationFactoryDelegate> GetOperationFactoryDelegates()
		{
			return new Dictionary<Type, OperationFactoryDelegate>
			{
				{
					typeof(GClass2055),
					() => new GClass2055(this)
				},
				{
					typeof(GClass2053),
					() => new GClass2053(this)
				},
				{
					typeof(GClass2037),
					() => new GClass2037(this)
				},
				{
					typeof(GClass2016),
					() => new GClass2016(this)
				},
				{
					typeof(AmmoPackReloadOperationClass),
					() => Item.MustBoltBeOpennedForInternalReload ? ((AmmoPackReloadOperationClass)new AmmoPackReloadInternalBoltOpenOperationClass(this)) : ((AmmoPackReloadOperationClass)new AmmoPackReloadInternalOneChamberOperationClass(this))
				},
				{
					typeof(CylinderReloadOperationClass),
					() => new CylinderReloadOperationClass(this)
				},
				{
					typeof(SingleBarrelReloadOperationClass),
					() => new SingleBarrelReloadOperationClass(this)
				},
				{
					typeof(MutliBarrelReloadOperationClass),
					() => new MutliBarrelReloadOperationClass(this)
				},
				{
					typeof(GClass2050),
					() => new GClass2050(this)
				},
				{
					typeof(GClass2039),
					() => new GClass2039(this)
				},
				{
					typeof(GClass2045),
					() => new GClass2045(this)
				},
				{
					typeof(GClass2044),
					() => new GClass2044(this)
				},
				{
					typeof(GClass2023),
					() => new GClass2023(this)
				},
				{
					typeof(GClass2052),
					() => new GClass2052(this)
				},
				{
					typeof(GClass2026),
					() => new GClass2026(this)
				},
				{
					typeof(Class1264),
					() => new Class1264(this)
				},
				{
					typeof(GClass2014),
					() => new GClass2014(this)
				},
				{
					typeof(GenericFireOperationClass),
					() => (!(Item is RocketLauncherItemClass)) ? ((!Item.IsFlareGun) ? ((!Item.IsOneOff) ? ((Item.ReloadMode != Weapon.EReloadMode.OnlyBarrel) ? ((!(Item is RevolverItemClass)) ? ((!Item.BoltAction) ? new GenericFireOperationClass(this) : new DefaultWeaponOperationClass(this)) : new RevolverFireOperationClass(this)) : new FireOnlyBarrelFireOperation(this)) : new IsOneOffFireOperationClass(this)) : new FlareGunFireOperationClass(this)) : new GClass2036(this)
				},
				{
					typeof(GClass2029),
					() => new GClass2029(this)
				},
				{
					typeof(GClass2049),
					() => new GClass2049(this)
				},
				{
					typeof(FixMalfunctionOperationClass),
					() => new FixMalfunctionOperationClass(this)
				},
				{
					typeof(RechamberOperationClass),
					() => new RechamberOperationClass(this)
				},
				{
					typeof(GClass2054),
					() => new GClass2054(this)
				},
				{
					typeof(GClass2056),
					() => new GClass2056(this)
				},
				{
					typeof(GClass2040),
					() => new GClass2040(this)
				},
				{
					typeof(GClass2034),
					() => new GClass2034(this)
				},
				{
					typeof(GClass2042),
					() => new GClass2042(this)
				},
				{
					typeof(GClass2024),
					() => (!Item.IsMultiBarrel) ? new GClass2024(this) : new GClass2025(this)
				},
				{
					typeof(GClass2043),
					() => new GClass2043(this)
				},
				{
					typeof(GClass2038),
					() => new GClass2038(this)
				},
				{
					typeof(GClass2041),
					() => new GClass2041(this)
				},
				{
					typeof(Class1269),
					() => new Class1269(this)
				},
				{
					typeof(GClass2047),
					() => new GClass2047(this)
				},
				{
					typeof(Class1270),
					() => new Class1270(this)
				}
			};
		}

		[CompilerGenerated]
		public BaseAnimationOperationClass method_66()
		{
			return new GClass2055(this);
		}

		[CompilerGenerated]
		public BaseAnimationOperationClass method_67()
		{
			return new GClass2053(this);
		}

		[CompilerGenerated]
		public BaseAnimationOperationClass method_68()
		{
			return new GClass2037(this);
		}

		[CompilerGenerated]
		public BaseAnimationOperationClass method_69()
		{
			return new GClass2016(this);
		}

		[CompilerGenerated]
		public BaseAnimationOperationClass method_70()
		{
			if (Item.MustBoltBeOpennedForInternalReload)
			{
				return new AmmoPackReloadInternalBoltOpenOperationClass(this);
			}
			return new AmmoPackReloadInternalOneChamberOperationClass(this);
		}

		[CompilerGenerated]
		public BaseAnimationOperationClass method_71()
		{
			return new CylinderReloadOperationClass(this);
		}

		[CompilerGenerated]
		public BaseAnimationOperationClass method_72()
		{
			return new SingleBarrelReloadOperationClass(this);
		}

		[CompilerGenerated]
		public BaseAnimationOperationClass method_73()
		{
			return new MutliBarrelReloadOperationClass(this);
		}

		[CompilerGenerated]
		public BaseAnimationOperationClass method_74()
		{
			return new GClass2050(this);
		}

		[CompilerGenerated]
		public BaseAnimationOperationClass method_75()
		{
			return new GClass2039(this);
		}

		[CompilerGenerated]
		public BaseAnimationOperationClass method_76()
		{
			return new GClass2045(this);
		}

		[CompilerGenerated]
		public BaseAnimationOperationClass method_77()
		{
			return new GClass2044(this);
		}

		[CompilerGenerated]
		public BaseAnimationOperationClass method_78()
		{
			return new GClass2023(this);
		}

		[CompilerGenerated]
		public BaseAnimationOperationClass method_79()
		{
			return new GClass2052(this);
		}

		[CompilerGenerated]
		public BaseAnimationOperationClass method_80()
		{
			return new GClass2026(this);
		}

		[CompilerGenerated]
		public BaseAnimationOperationClass method_81()
		{
			return new Class1264(this);
		}

		[CompilerGenerated]
		public BaseAnimationOperationClass method_82()
		{
			return new GClass2014(this);
		}

		[CompilerGenerated]
		public BaseAnimationOperationClass method_83()
		{
			if (!(Item is RocketLauncherItemClass))
			{
				if (!Item.IsFlareGun)
				{
					if (!Item.IsOneOff)
					{
						if (Item.ReloadMode != Weapon.EReloadMode.OnlyBarrel)
						{
							if (!(Item is RevolverItemClass))
							{
								if (!Item.BoltAction)
								{
									return new GenericFireOperationClass(this);
								}
								return new DefaultWeaponOperationClass(this);
							}
							return new RevolverFireOperationClass(this);
						}
						return new FireOnlyBarrelFireOperation(this);
					}
					return new IsOneOffFireOperationClass(this);
				}
				return new FlareGunFireOperationClass(this);
			}
			return new GClass2036(this);
		}

		[CompilerGenerated]
		public BaseAnimationOperationClass method_84()
		{
			return new GClass2029(this);
		}

		[CompilerGenerated]
		public BaseAnimationOperationClass method_85()
		{
			return new GClass2049(this);
		}

		[CompilerGenerated]
		public BaseAnimationOperationClass method_86()
		{
			return new FixMalfunctionOperationClass(this);
		}

		[CompilerGenerated]
		public BaseAnimationOperationClass method_87()
		{
			return new RechamberOperationClass(this);
		}

		[CompilerGenerated]
		public BaseAnimationOperationClass method_88()
		{
			return new GClass2054(this);
		}

		[CompilerGenerated]
		public BaseAnimationOperationClass method_89()
		{
			return new GClass2056(this);
		}

		[CompilerGenerated]
		public BaseAnimationOperationClass method_90()
		{
			return new GClass2040(this);
		}

		[CompilerGenerated]
		public BaseAnimationOperationClass method_91()
		{
			return new GClass2034(this);
		}

		[CompilerGenerated]
		public BaseAnimationOperationClass method_92()
		{
			return new GClass2042(this);
		}

		[CompilerGenerated]
		public BaseAnimationOperationClass method_93()
		{
			if (!Item.IsMultiBarrel)
			{
				return new GClass2024(this);
			}
			return new GClass2025(this);
		}

		[CompilerGenerated]
		public BaseAnimationOperationClass method_94()
		{
			return new GClass2043(this);
		}

		[CompilerGenerated]
		public BaseAnimationOperationClass method_95()
		{
			return new GClass2038(this);
		}

		[CompilerGenerated]
		public BaseAnimationOperationClass method_96()
		{
			return new GClass2041(this);
		}

		[CompilerGenerated]
		public BaseAnimationOperationClass method_97()
		{
			return new Class1269(this);
		}

		[CompilerGenerated]
		public BaseAnimationOperationClass method_98()
		{
			return new GClass2047(this);
		}

		[CompilerGenerated]
		public BaseAnimationOperationClass method_99()
		{
			return new Class1270(this);
		}
	}

	public interface GInterface193
	{
		void OnIdleStartAction();

		void OnDrawCompleteAction();

		void OnHideCompleteActionAction();

		void OnDropGrenadeAction();

		void OnDropFinishedAction();

		void StartCountdown();

		void HideGrenade(Action onHidden, bool fastHide);

		void PutGrenadeBack();

		void ShowGesture(EInteraction gesture);

		bool CanRemove();

		void FastForward();

		void OnBackpackDrop();

		void Execute(GInterface438 operation, Callback callback);
	}

	public abstract class BaseGrenadeHandsController : ItemHandsController
	{
		[Serializable]
		[CompilerGenerated]
		public class Class1243
		{
			public static readonly Class1243 class1243_0 = new Class1243();

			public bool method_0(RaycastHit raycastHit)
			{
				return false;
			}
		}

		[CompilerGenerated]
		public class Class1244<T> where T : BaseGrenadeHandsController
		{
			public T controller;

			public Player player;

			public void method_0()
			{
				controller.firearmsAnimator_0.RemoveEventsConsumer(controller);
			}

			public Transform method_1(string x)
			{
				return TransformHelperClass.FindTransformRecursive(player.PlayerBones.WeaponRoot.Original, x);
			}
		}

		[CompilerGenerated]
		public class Class1245
		{
			public BaseGrenadeHandsController baseGrenadeHandsController_0;

			public float animationSpeed;

			public Action callback;

			public bool fastDrop;

			public void method_0(Result<IHandsThrowController> result)
			{
				baseGrenadeHandsController_0.ActualDrop(result, animationSpeed, callback, fastDrop);
			}
		}

		[CompilerGenerated]
		public class Class1246
		{
			public Class1312 inventoryOperation;

			public Action callback;

			public void method_0()
			{
				inventoryOperation.Confirm();
				callback();
			}
		}

		public const float MASS = 0.6f;

		protected FirearmsAnimator firearmsAnimator_0;

		private const string string_0 = "fireport";

		protected Transform transform_0;

		protected Transform transform_1;

		protected Transform[] transform_2;

		protected GrenadePrefab grenadePrefab_0;

		protected GrenadeEmission grenadeEmission_0;

		private static readonly RaycastHit[] raycastHit_0 = new RaycastHit[8];

		private static Func<RaycastHit, bool> func_0 = (RaycastHit raycastHit) => false;

		public new ThrowWeapItemClass Item => base.Item as ThrowWeapItemClass;

		public override FirearmsAnimator FirearmsAnimator => firearmsAnimator_0;

		public GInterface193 GInterface193_0 => base.CurrentHandsOperation as GInterface193;

		public static T smethod_6<T>(Player player, ThrowWeapItemClass item, bool setQuickThrowParameters) where T : BaseGrenadeHandsController
		{
			T val = ItemHandsController.smethod_0<T>(player, item);
			smethod_8(val, player, setQuickThrowParameters);
			return val;
		}

		public static async Task<T> smethod_7<T>(Player player, ThrowWeapItemClass item, bool setQuickThrowParameters) where T : BaseGrenadeHandsController
		{
			T obj = await ItemHandsController.smethod_2<T>(player, item);
			smethod_8(obj, player, setQuickThrowParameters);
			return obj;
		}

		public static void smethod_8<T>(T controller, Player player, bool setQuickThrowParameters) where T : BaseGrenadeHandsController
		{
			WeaponPrefab componentInChildren = controller.ControllerGameObject.GetComponentInChildren<WeaponPrefab>();
			controller.firearmsAnimator_0 = componentInChildren.FirearmsAnimator;
			controller.firearmsAnimator_0.AddEventsConsumer(controller);
			controller.CompositeDisposable.AddDisposable(delegate
			{
				controller.firearmsAnimator_0.RemoveEventsConsumer(controller);
			});
			controller.transform_0 = player.PlayerBones.WeaponRoot.Original;
			if (setQuickThrowParameters)
			{
				controller.firearmsAnimator_0.SetQuickFire(quickFire: true);
				controller.firearmsAnimator_0.SetActiveParam(active: false);
			}
			controller.firearmsAnimator_0.SkipTime(1f / 60f);
			controller.grenadePrefab_0 = controller._controllerObject.GetComponent<GrenadePrefab>();
			controller.transform_1 = TransformHelperClass.FindTransformRecursive(player.PlayerBones.WeaponRoot.Original, controller.grenadePrefab_0.ThrowingParts[0]);
			controller.transform_1.gameObject.SetActive(value: true);
			controller.transform_2 = controller.grenadePrefab_0.ThrowingParts.Select((string x) => TransformHelperClass.FindTransformRecursive(player.PlayerBones.WeaponRoot.Original, x)).ToArray();
			for (int num = 0; num < controller.transform_2.Length; num++)
			{
				controller.transform_2[num].gameObject.SetActive(value: true);
			}
			controller._player.HandsAnimator = controller.firearmsAnimator_0;
			bool flag = controller._player.UpdateGrenadeAnimatorDuePoV();
			controller.firearmsAnimator_0.Animator.SetFloat("ThirdPersonAnimation", flag ? 1 : 0);
			player.ProceduralWeaponAnimation.ClearPreviousWeapon();
			player.ProceduralWeaponAnimation.InitTransforms(controller.HandsHierarchy);
			componentInChildren.ObjectInHands.AfterGetFromPoolInit(player.ProceduralWeaponAnimation, null, player.IsYourPlayer);
			controller._controllerObject.GetComponent<BaseSoundPlayer>().Init(controller, player.PlayerBones.WeaponRoot, player);
		}

		public override void IEventsConsumerOnWeapIn()
		{
			method_6();
		}

		public override void IEventsConsumerOnWeapOut()
		{
			method_5();
		}

		public override void IEventsConsumerOnFiringBullet()
		{
			method_3();
		}

		public override void IEventsConsumerOnIdleStart()
		{
			method_4();
		}

		public override void IEventsConsumerOnFireEnd()
		{
			method_2();
		}

		public override void IEventsConsumerOnAddAmmoInChamber()
		{
			method_1();
		}

		public override void IEventsConsumerOnDelAmmoChamber()
		{
			method_8();
		}

		public override void IEventsConsumerOnThirdAction(int intParam)
		{
			TranslateAnimatorParameter(intParam);
		}

		public override void IEventsConsumerOnCook()
		{
			vmethod_0();
		}

		public override void IEventsOnBackpackDrop()
		{
			method_7();
		}

		public override bool SupportPickup()
		{
			return true;
		}

		public override void IEventsConsumerOnOnUseProp(bool boolParam)
		{
			SetPropVisibility(boolParam);
		}

		public override void Pickup(bool p)
		{
			if (CanInteract())
			{
				firearmsAnimator_0.SetPickup(p);
			}
		}

		public override void Interact(bool isInteracting, int actionIndex)
		{
			if (CanInteract())
			{
				_player.SendHandsInteractionStateChanged(isInteracting, actionIndex);
				firearmsAnimator_0.SetInteract(isInteracting, actionIndex);
			}
		}

		public override bool CanInteract()
		{
			if (firearmsAnimator_0.IsIdling())
			{
				return firearmsAnimator_0.Animator.GetBool(WeaponAnimationSpeedControllerClass.BOOL_ACTIVE);
			}
			return false;
		}

		public override void Loot(bool p)
		{
			if (CanInteract())
			{
				firearmsAnimator_0.SetLooting(p);
			}
		}

		public override float GetAnimatorFloatParam(int hash)
		{
			return firearmsAnimator_0.GetAnimatorParameter(hash);
		}

		public override bool IsInInteraction()
		{
			return firearmsAnimator_0.IsInInteraction;
		}

		public override bool IsInInteractionStrictCheck()
		{
			if (!IsInInteraction() && !(firearmsAnimator_0.GetLayerWeight(firearmsAnimator_0.LACTIONS_LAYER_INDEX) >= float.Epsilon))
			{
				return firearmsAnimator_0.Animator.IsInTransition(firearmsAnimator_0.LACTIONS_LAYER_INDEX);
			}
			return true;
		}

		public override void Spawn(float animationSpeed, Action callback)
		{
			firearmsAnimator_0.SetAnimationSpeed(animationSpeed);
		}

		public override void Drop(float animationSpeed, Action callback, bool fastDrop, Item nextControllerItem = null)
		{
			Callback<IHandsThrowController> callback2 = delegate(Result<IHandsThrowController> result)
			{
				ActualDrop(result, animationSpeed, callback, fastDrop);
			};
			GrenadeHandsController grenadeHandsController = this as GrenadeHandsController;
			if (CanRemove())
			{
				callback2(grenadeHandsController);
			}
			else if (grenadeHandsController != null)
			{
				grenadeHandsController.SetOnUsedCallback(callback2);
			}
		}

		public virtual void ActualDrop(Result<IHandsThrowController> controller, float animationSpeed, Action callback, bool fastDrop)
		{
			if (base.Destroyed)
			{
				GInterface193_0.HideGrenade(callback, fastDrop);
				return;
			}
			base.Destroyed = true;
			firearmsAnimator_0.SetAnimationSpeed(animationSpeed);
			Class1312 inventoryOperation = _player.method_138(Item);
			Action onHidden = delegate
			{
				inventoryOperation.Confirm();
				callback();
			};
			GInterface193_0.HideGrenade(onHidden, fastDrop);
		}

		public override void Destroy()
		{
			_player.ProceduralWeaponAnimation.ClearPreviousWeapon();
			base.Destroy();
			firearmsAnimator_0 = null;
			AssetPoolObject.ReturnToPool(_controllerObject.gameObject);
		}

		public void method_1()
		{
			GInterface193_0.StartCountdown();
		}

		public virtual void vmethod_0()
		{
			if (!(grenadeEmission_0 != null))
			{
				grenadeEmission_0 = Singleton<Effects>.Instance.GetEmissionEffect(grenadePrefab_0.GrenadeItself.EmmisionEffect);
				grenadeEmission_0.AttachTo(transform_1, grenadePrefab_0.GrenadeItself.Offset);
				grenadeEmission_0.SetFillParams(0f, Item.EmitTime);
				grenadeEmission_0.StartEmission(0f);
			}
		}

		public void method_2()
		{
			GInterface193_0.OnDropFinishedAction();
		}

		public void method_3()
		{
			GInterface193_0.OnDropGrenadeAction();
		}

		public void method_4()
		{
			GInterface193_0.OnIdleStartAction();
		}

		public void method_5()
		{
			GInterface193_0.OnHideCompleteActionAction();
		}

		public void method_6()
		{
			GInterface193_0.OnDrawCompleteAction();
		}

		public override bool CanExecute(GInterface438 operation)
		{
			return true;
		}

		public override void Execute(GInterface438 operation, Callback callback)
		{
			GInterface193_0.Execute(operation, callback);
		}

		public override void ShowGesture(EInteraction gesture)
		{
			GInterface193_0.ShowGesture(gesture);
		}

		public void method_7()
		{
			GInterface193_0.OnBackpackDrop();
		}

		public void method_8()
		{
			GInterface193_0.PutGrenadeBack();
		}

		public virtual void vmethod_1(float timeSinceSafetyLevelRemoved, bool low = false)
		{
			if (BackendConfigAbstractClass.Config.UseSpiritPlayer && _player.Spirit.IsActive)
			{
				_player.Spirit.PlayerSync();
			}
			float num = 1f;
			Vector3 up = Vector3.up;
			Vector3? throwPosition = null;
			float num2 = 3f;
			bool flag = _player.IsAI && _player.AIData.BotOwner.BotState == EBotState.Active;
			if (!_player.HealthController.IsAlive)
			{
				return;
			}
			if (flag)
			{
				try
				{
					BotGrenadeController grenades = _player.AIData.BotOwner.WeaponManager.Grenades;
					float num3 = ((grenades.Mass <= 0.01f) ? 0.5f : grenades.Mass);
					num2 = grenades.AIGreanageThrowData.Force * num3;
					up = GClass855.NormalizeFastSelf(grenades.ToThrowDirection);
					_ = grenades.AIGreanageThrowData.Direction;
					throwPosition = FindThrowPosition();
				}
				catch (Exception)
				{
					return;
				}
			}
			else
			{
				num = (low ? 0.66f : (1f + (float)_player.Skills.StrengthBuffThrowDistanceInc));
				num2 = EFTHardSettings.Instance.GrenadeForce;
				if (!_player.Skills.ThrowingEliteBuff)
				{
					Vector3 vector = Mathf.Clamp01(0.5f - _player.Physical.HandsStamina.NormalValue) * UnityEngine.Random.onUnitSphere;
					up = (-transform_0.up * 5f + vector).normalized;
					num *= Mathf.Lerp(0.4f, 1f, _player.Physical.HandsStamina.NormalValue + 0.5f);
				}
				else
				{
					up = -transform_0.up;
				}
			}
			method_9(throwPosition, timeSinceSafetyLevelRemoved, num, up, num2, low, !flag);
		}

		public void method_9(Vector3? throwPosition, float timeSinceSafetyLevelRemoved, float lowHighThrow, Vector3 direction, float forcePower, bool lowThrow, bool withVelocity)
		{
			Vector3 force = direction * (forcePower * lowHighThrow);
			if (withVelocity)
			{
				force += _player.Velocity;
			}
			if (!throwPosition.HasValue)
			{
				throwPosition = FindThrowPosition();
			}
			vmethod_2(timeSinceSafetyLevelRemoved, throwPosition.Value, transform_1.rotation, force, lowThrow);
		}

		public Vector3 FindThrowPosition()
		{
			Vector3 vector = transform_1.position + transform_1.rotation * grenadePrefab_0.GrenadeItself.Offset;
			if (CheckHandsToBodyObstacles(_player, vector, out var _, out var correctedPoint))
			{
				vector = correctedPoint;
			}
			return vector;
		}

		public static bool CheckHandsToBodyObstacles(Player player, Vector3 point, out RaycastHit hit, out Vector3 correctedPoint)
		{
			Vector3 projectionOnRealForwardSurface = player.MovementContext.GetProjectionOnRealForwardSurface(point);
			bool isForwardHit;
			bool num = EFTPhysicsClass.LinecastInBothSides(projectionOnRealForwardSurface, point, out hit, out isForwardHit, LayerMasksDataAbstractClass.StaticObjectsHitMask, LayerMasksDataAbstractClass.StaticObjectsHitMask, raycastHit_0, func_0);
			if (num)
			{
				correctedPoint = hit.point - (point - projectionOnRealForwardSurface).normalized * 0.1f;
				return num;
			}
			correctedPoint = point;
			return num;
		}

		public Grenade method_10(Vector3 position, Quaternion rotation, Vector3 force, float prewarm = 0f)
		{
			GrenadeSettings grenadeSettings = UnityEngine.Object.Instantiate(grenadePrefab_0.GrenadeItself);
			Grenade grenade = Singleton<GInterface169>.Instance.GrenadeFactory.Create(grenadeSettings, position, rotation, force, Item, _player.ProfileId, prewarm);
			Singleton<GInterface169>.Instance.RegisterGrenade(grenade);
			return grenade;
		}

		public virtual void vmethod_2(float timeSinceSafetyLevelRemoved, Vector3 position, Quaternion rotation, Vector3 force, bool lowThrow)
		{
			_player.ExecuteSkill((Action)delegate
			{
				_player.Skills.ThrowAction.Complete();
			});
			GStruct154<GClass2060> gStruct = _player.method_139(Item, lowThrow, simulate: false);
			if (gStruct.Succeeded)
			{
				gStruct.Value.RaiseEvents(_player.InventoryController, CommandStatus.Begin);
				gStruct.Value.RaiseEvents(_player.InventoryController, CommandStatus.Succeed);
				Grenade grenade = method_10(position, rotation, force, timeSinceSafetyLevelRemoved);
				SmokeGrenade smokeGrenade = grenade as SmokeGrenade;
				if (smokeGrenade != null)
				{
					if (grenadeEmission_0 == null)
					{
						vmethod_0();
					}
					if (grenadeEmission_0 != null)
					{
						grenadeEmission_0.AttachTo(smokeGrenade.transform, Vector3.zero);
						smokeGrenade.EmissionEnd = (Action<Grenade>)Delegate.Combine(smokeGrenade.EmissionEnd, new Action<Grenade>(grenadeEmission_0.StopEmission));
						smokeGrenade.VelocityBelowThreshold += grenadeEmission_0.Stall;
					}
				}
				Transform[] array = transform_2;
				for (int num = 0; num < array.Length; num++)
				{
					array[num].gameObject.SetActive(value: false);
				}
				if (Singleton<BotEventHandler>.Instantiated)
				{
					if (_player.IsAI)
					{
						grenade.SetRigidbodyMass(0.5f);
						Singleton<BotEventHandler>.Instance.ThrowGrenade(grenade, position + Vector3.up, force, 0.5f);
					}
					else
					{
						Singleton<BotEventHandler>.Instance.ThrowGrenade(grenade, position + Vector3.up, force, 0.6f);
					}
				}
			}
			else
			{
				UnityEngine.Debug.LogError("Couldn't throw grenade: " + gStruct.Error);
			}
		}

		public bool method_11(out PlantingKitsItemClass plantingKit)
		{
			plantingKit = GClass3380.GetAllItems(_player.InventoryController.Inventory.Equipment).OfType<PlantingKitsItemClass>().FirstOrDefault();
			return plantingKit != null;
		}

		public override void FastForwardCurrentState()
		{
			GInterface193_0.FastForward();
		}

		public BaseGrenadeHandsController()
		{
		}

		[CompilerGenerated]
		public void method_12()
		{
			_player.Skills.ThrowAction.Complete();
		}
	}

	public class GrenadeHandsController : BaseGrenadeHandsController, IHandsThrowController, GInterface199, IHandsController, GInterface197
	{
		public class Class1273 : Class1272
		{
			public enum ESourceState
			{
				IdleState,
				PlantTripwireState
			}

			[NonSerialized]
			public Callback Callback_0;

			[NonSerialized]
			public ESourceState EsourceState_0;

			public Class1273(GrenadeHandsController controller)
				: base(controller)
			{
			}

			public virtual void Start(Item item, Callback callback)
			{
				Callback_0 = callback;
				Start();
				Gparam_0.FirearmsAnimator.SetInventory(open: false);
				Gparam_0._player.SendHandsInteractionStateChanged(value: true, 300);
				Gparam_0._player.MovementContext.SetInteractInHands(EInteraction.DropBackpack);
			}

			public void Setup(ESourceState sourceState)
			{
				EsourceState_0 = sourceState;
			}

			public override void Reset()
			{
				Callback_0 = null;
				base.Reset();
			}

			public override void OnBackpackDrop()
			{
				State = EOperationState.Finished;
				Gparam_0._player.SendHandsInteractionStateChanged(value: false, 300);
				Gparam_0.FirearmsAnimator.SetInteract(p: false, 300);
				Gparam_0._player.MovementContext.SetInteractInHands(EInteraction.DropBackpack);
				WeaponAnimationSpeedControllerClass.ResetTriggerHandReady(Gparam_0.FirearmsAnimator.Animator);
				Gparam_0.FirearmsAnimator.SetInventory(Gparam_0.bool_0);
				switch (EsourceState_0)
				{
				case ESourceState.PlantTripwireState:
					Gparam_0.InitiateOperation<TripwireStateManagerClass>().Start();
					break;
				case ESourceState.IdleState:
					Gparam_0.InitiateOperation<Class1277>().Start();
					break;
				}
				Callback_0.Succeed();
			}

			public override void SetInventoryOpened(bool opened)
			{
				Gparam_0.bool_0 = opened;
			}
		}

		public abstract class Class1272 : Class1271<GrenadeHandsController>
		{
			public Class1272(GrenadeHandsController controller)
				: base(controller)
			{
			}

			public virtual void ExamineWeapon()
			{
				method_0();
			}

			public virtual void PullRingForHighThrow()
			{
				method_0();
			}

			public virtual void HighThrow()
			{
				method_0();
			}

			public virtual void PullRingForLowThrow()
			{
				method_0();
			}

			public virtual void LowThrow()
			{
				method_0();
			}

			public virtual void ChangeFireMode(Weapon.EFireMode fireMode)
			{
				method_0();
			}

			public virtual void HandleFireInput()
			{
				method_0();
			}

			public virtual void HandleAltFireInput()
			{
				method_0();
			}

			public virtual void SetInventoryOpened(bool opened)
			{
				method_0();
			}

			public virtual void SetGrenadeCompassState(bool active)
			{
				method_0();
			}

			public virtual void PlantTripwire()
			{
				method_0();
			}

			public virtual bool CanChangeFireMode(Weapon.EFireMode fireMode)
			{
				method_0();
				return false;
			}
		}

		public class Class1275 : Class1272
		{
			public enum EThrowState
			{
				None,
				Idling,
				Throwing,
				Threw
			}

			[NonSerialized]
			public EThrowState EthrowState_0;

			[NonSerialized]
			public Action Action_0;

			public virtual bool WaitingHighThrow => EthrowState_0 == EThrowState.Idling;

			public Class1275(GrenadeHandsController controller)
				: base(controller)
			{
			}

			public new void Start()
			{
				Gparam_0._player.Say(EPhraseTrigger.OnGrenade);
				base.Start();
				vmethod_0();
				EthrowState_0 = EThrowState.Idling;
			}

			public override void Reset()
			{
				base.Reset();
				EthrowState_0 = EThrowState.None;
				Action_0 = null;
			}

			public virtual void vmethod_0()
			{
				Gparam_0.firearmsAnimator_0.SetGrenadeFire(FirearmsAnimator.EGrenadeFire.Hold);
			}

			public override void HighThrow()
			{
				if (EthrowState_0 != EThrowState.Throwing && EthrowState_0 != EThrowState.Threw)
				{
					EthrowState_0 = EThrowState.Throwing;
					Gparam_0.firearmsAnimator_0.SetGrenadeFire(FirearmsAnimator.EGrenadeFire.Throw);
				}
			}

			public override void OnDropGrenadeAction()
			{
				Gparam_0.firearmsAnimator_0.SetGrenadeFire(FirearmsAnimator.EGrenadeFire.Idle);
				method_2();
			}

			public void method_2(bool low = false)
			{
				Gparam_0.transform_1.gameObject.SetActive(value: false);
				Gparam_0.vmethod_1(0f, low);
			}

			public override void OnDropFinishedAction()
			{
				if (EthrowState_0 == EThrowState.Threw)
				{
					return;
				}
				EthrowState_0 = EThrowState.Threw;
				if (Gparam_0.Destroyed)
				{
					if (Action_0 != null)
					{
						Action_0();
					}
					Action_0 = null;
				}
				else if (Gparam_0.callback_0 != null)
				{
					Gparam_0.callback_0(Gparam_0);
				}
			}

			public override void HideGrenade(Action onHidden, bool fastHide)
			{
				if (EthrowState_0 == EThrowState.Threw)
				{
					onHidden();
				}
				else if (EthrowState_0 == EThrowState.Idling && Gparam_0.Item.CanBeHiddenDuringThrow)
				{
					Action_0 = onHidden;
					State = EOperationState.Finished;
					Gparam_0.InitiateOperation<Class1279>().Start(onHidden);
				}
				else
				{
					Action_0 = onHidden;
				}
			}

			public override void HandleFireInput()
			{
				if (WaitingHighThrow)
				{
					HighThrow();
				}
			}

			public override void SetInventoryOpened(bool opened)
			{
				if (EthrowState_0 == EThrowState.Idling && Gparam_0.Item.CanBeHiddenDuringThrow)
				{
					Gparam_0.firearmsAnimator_0.SetInventory(opened);
					PutGrenadeBack();
				}
			}

			public void method_3()
			{
				State = EOperationState.Finished;
				Gparam_0.InitiateOperation<Class1277>().Start();
			}

			public override bool CanRemove()
			{
				return EthrowState_0 != EThrowState.Throwing;
			}

			public override void PutGrenadeBack()
			{
				Gparam_0.firearmsAnimator_0.SetGrenadeAltFire(FirearmsAnimator.EGrenadeFire.Idle);
				Gparam_0.firearmsAnimator_0.SetGrenadeFire(FirearmsAnimator.EGrenadeFire.Idle);
				method_3();
				Gparam_0.vmethod_3();
			}

			public override void FastForward()
			{
				base.FastForward();
				if (EthrowState_0 == EThrowState.Idling)
				{
					PutGrenadeBack();
				}
				else if (EthrowState_0 != EThrowState.Threw)
				{
					UnityEngine.Debug.LogErrorFormat("Throw grenade operation: Fast Forward not implemented for _throwState == {0}", EthrowState_0);
				}
			}
		}

		public class Class1277 : Class1272
		{
			[NonSerialized]
			public bool Bool_0;

			public Class1277(GrenadeHandsController controller)
				: base(controller)
			{
			}

			public new void Start()
			{
				base.Start();
				Gparam_0.FirearmsAnimator.SetFireMode(Weapon.EFireMode.grenadeThrowing);
			}

			public override void HideGrenade(Action onHidden, bool fastHide)
			{
				State = EOperationState.Finished;
				Gparam_0.InitiateOperation<Class1279>().Start(onHidden);
			}

			public override bool CanRemove()
			{
				return true;
			}

			public override void OnEnd()
			{
				Gparam_0.SetCompassState(active: false);
				Bool_0 = false;
			}

			public override void ExamineWeapon()
			{
				Gparam_0.firearmsAnimator_0.LookTrigger();
			}

			public override void HandleFireInput()
			{
				PullRingForHighThrow();
			}

			public override void HandleAltFireInput()
			{
				PullRingForLowThrow();
			}

			public override void PullRingForHighThrow()
			{
				State = EOperationState.Finished;
				Gparam_0.InitiateOperation<Class1275>().Start();
			}

			public override void PullRingForLowThrow()
			{
				State = EOperationState.Finished;
				Gparam_0.InitiateOperation<Class1276>().Start();
			}

			public override void LowThrow()
			{
			}

			public override void OnIdleStartAction()
			{
				Bool_0 = true;
			}

			public override void ChangeFireMode(Weapon.EFireMode fireMode)
			{
				if (CanChangeFireMode(fireMode) && Gparam_0.method_11(out var _))
				{
					State = EOperationState.Finished;
					Gparam_0.SetFireModeVisual(fireMode);
					Gparam_0.InitiateOperation<TripwireStateManagerClass>().Start();
					Gparam_0.CurrentFireMode = fireMode;
				}
			}

			public override void HighThrow()
			{
			}

			public override void SetGrenadeCompassState(bool active)
			{
				Gparam_0.CompassState.Value = active;
			}

			public override bool CanChangeFireMode(Weapon.EFireMode fireMode)
			{
				bool flag = true;
				PlantingKitsItemClass plantingKit;
				if (fireMode != Weapon.EFireMode.greanadePlanting && fireMode != Weapon.EFireMode.grenadeThrowing)
				{
					flag = false;
				}
				else if (!Bool_0)
				{
					flag = false;
				}
				else if (!Gparam_0.Item.CanPlantOnGround)
				{
					NotificationManagerClass.DisplaySingletonWarningNotification(GClass2348.Localized("Tripwire/NoPlantFireMode"));
					flag = false;
				}
				else if (!Gparam_0.method_11(out plantingKit))
				{
					flag = false;
				}
				if (!flag)
				{
					Gparam_0.SetFireModeVisual(Weapon.EFireMode.grenadeThrowing);
				}
				return flag;
			}

			public override void Execute(GInterface438 operation, Callback callback)
			{
				if (!(operation is GInterface443 gInterface))
				{
					callback.Succeed();
				}
				else if (Gparam_0._player.InventoryController.IsAnimatedSlot(gInterface.From1))
				{
					State = EOperationState.Finished;
					Class1273 @class = Gparam_0.InitiateOperation<Class1273>();
					@class.Start(gInterface.Item1, callback);
					@class.Setup(Class1273.ESourceState.IdleState);
				}
				else
				{
					callback.Succeed();
				}
			}

			public override void SetInventoryOpened(bool opened)
			{
				Gparam_0.firearmsAnimator_0.SetInventory(opened);
			}

			public override void ShowGesture(EInteraction gesture)
			{
				Gparam_0.firearmsAnimator_0.Gesture(gesture);
			}
		}

		public class Class1276 : Class1275
		{
			public override bool WaitingHighThrow => false;

			public bool WaitingLowThrow => base.WaitingHighThrow;

			public Class1276(GrenadeHandsController controller)
				: base(controller)
			{
			}

			public override void vmethod_0()
			{
				Gparam_0.firearmsAnimator_0.SetGrenadeAltFire(FirearmsAnimator.EGrenadeFire.Hold);
			}

			public override void HighThrow()
			{
				method_0();
			}

			public override void LowThrow()
			{
				if (EthrowState_0 != EThrowState.Throwing && EthrowState_0 != EThrowState.Threw)
				{
					EthrowState_0 = EThrowState.Throwing;
					Gparam_0.firearmsAnimator_0.SetGrenadeAltFire(FirearmsAnimator.EGrenadeFire.Throw);
				}
			}

			public override void OnDropGrenadeAction()
			{
				Gparam_0.firearmsAnimator_0.SetGrenadeAltFire(FirearmsAnimator.EGrenadeFire.Idle);
				method_2(low: true);
			}

			public override void HandleAltFireInput()
			{
				if (WaitingLowThrow)
				{
					LowThrow();
				}
			}
		}

		public class TripwireStateManagerClass : Class1272
		{
			public enum EPlantOperationState
			{
				None,
				StateIn,
				Idling,
				Planting,
				Planted
			}

			[CompilerGenerated]
			public class Class1247
			{
				public Player player;

				public void method_0(IResult result)
				{
					player.ClearPlanting();
					if (result.Failed)
					{
						NotificationManagerClass.DisplaySingletonWarningNotification(GClass2348.Localized("Tripwire/PlantUnavailable"));
						UnityEngine.Debug.LogError("Failed to plant tripwire: " + result.Error);
					}
				}
			}

			[NonSerialized]
			[CompilerGenerated]
			public EPlantOperationState EplantOperationState_0;

			[NonSerialized]
			public Action Action_0;

			[NonSerialized]
			public PlantingKitsItemClass PlantingKitsItemClass;

			public EPlantOperationState EPlantOperationState_0
			{
				[CompilerGenerated]
				get
				{
					return EplantOperationState_0;
				}
				[CompilerGenerated]
				set
				{
					EplantOperationState_0 = value;
				}
			}

			public ETripwirePlanState ETripwirePlanState_0 => Gparam_0._player.TripwireVisualPlacer_0.TripwirePlanState;

			public TripwireStateManagerClass(GrenadeHandsController controller)
				: base(controller)
			{
			}

			public new void Start()
			{
				Player player = Gparam_0._player;
				if (player.TripwireVisualPlacer_0 == null || !player.TripwireVisualPlacer_0.PlantEnabled)
				{
					player.CreatePlantPlanner();
				}
				player.TripwireVisualPlacer_0.Init(Gparam_0.Item);
				Gparam_0.FirearmsAnimator.SetFireMode(Weapon.EFireMode.greanadePlanting);
				EPlantOperationState_0 = EPlantOperationState.StateIn;
				base.Start();
			}

			public override void OnIdleStartAction()
			{
				Gparam_0._player.InitFirstTripwirePoint();
				EPlantOperationState_0 = EPlantOperationState.Idling;
			}

			public override void OnEnd()
			{
				if (ETripwirePlanState_0 != ETripwirePlanState.Planned)
				{
					Gparam_0._player.ClearPlanting();
				}
				PlantingKitsItemClass = null;
			}

			public override void OnDropFinishedAction()
			{
				if (EPlantOperationState_0 == EPlantOperationState.Planted)
				{
					return;
				}
				if (EPlantOperationState_0 == EPlantOperationState.Planting)
				{
					if (PlantingKitsItemClass != null)
					{
						Player player = Gparam_0._player;
						TripwireVisualPlacer tripwireVisualPlacer_ = player.TripwireVisualPlacer_0;
						player.InventoryController.PlantTripwire(Gparam_0.Item, PlantingKitsItemClass, tripwireVisualPlacer_.FirstPlantPosition, tripwireVisualPlacer_.SecondPlantPosition, delegate(IResult result)
						{
							player.ClearPlanting();
							if (result.Failed)
							{
								NotificationManagerClass.DisplaySingletonWarningNotification(GClass2348.Localized("Tripwire/PlantUnavailable"));
								UnityEngine.Debug.LogError("Failed to plant tripwire: " + result.Error);
							}
						});
					}
					else
					{
						UnityEngine.Debug.LogError("Failed to plant tripwire: no kits");
					}
				}
				EPlantOperationState_0 = EPlantOperationState.Planted;
				if (Gparam_0.Destroyed)
				{
					Action_0?.Invoke();
					Action_0 = null;
				}
				else
				{
					Gparam_0.callback_0?.Invoke(Gparam_0);
				}
			}

			public override void HandleFireInput()
			{
				if (EPlantOperationState_0 != EPlantOperationState.Idling)
				{
					return;
				}
				Player player = Gparam_0._player;
				if (!Gparam_0.method_11(out var plantingKit))
				{
					NotificationManagerClass.DisplaySingletonWarningNotification(GClass2348.Localized("Tripwire/NoKit"));
					UnityEngine.Debug.LogError("Failed to plant tripwire: no kits");
					player.ClearPlanting();
					Gparam_0.ChangeFireMode(Weapon.EFireMode.grenadeThrowing);
					return;
				}
				PlantingKitsItemClass = plantingKit;
				TripwireVisualPlacer tripwireVisualPlacer_ = player.TripwireVisualPlacer_0;
				switch (tripwireVisualPlacer_.TripwirePlanState)
				{
				case ETripwirePlanState.None:
					player.InitFirstTripwirePoint();
					break;
				case ETripwirePlanState.FirstPlant:
					player.TripwireVisualPlacer_0.InitSecondPoint();
					break;
				case ETripwirePlanState.SecondPlant:
					if (tripwireVisualPlacer_.CanStartPlanting())
					{
						Gparam_0.PlantTripwire();
					}
					break;
				}
			}

			public override void HandleAltFireInput()
			{
				if (EPlantOperationState_0 == EPlantOperationState.Idling && ETripwirePlanState_0 == ETripwirePlanState.SecondPlant)
				{
					Gparam_0._player.InitFirstTripwirePoint();
				}
			}

			public override void Execute(GInterface438 operation, Callback callback)
			{
				if (EPlantOperationState_0 > EPlantOperationState.Idling)
				{
					callback.Fail("Can't execute operations in state " + EPlantOperationState_0);
				}
				else if (!(operation is GInterface443 gInterface))
				{
					callback.Succeed();
				}
				else if (Gparam_0._player.InventoryController.IsAnimatedSlot(gInterface.From1))
				{
					State = EOperationState.Finished;
					Class1273 @class = Gparam_0.InitiateOperation<Class1273>();
					@class.Start(gInterface.Item1, callback);
					@class.Setup(Class1273.ESourceState.PlantTripwireState);
				}
				else
				{
					callback.Succeed();
				}
			}

			public override void PlantTripwire()
			{
				if (EPlantOperationState_0 == EPlantOperationState.Idling)
				{
					EPlantOperationState_0 = EPlantOperationState.Planting;
					Gparam_0.FirearmsAnimator.SetGrenadeFire(FirearmsAnimator.EGrenadeFire.Throw);
				}
			}

			public override void HideGrenade(Action onHidden, bool fastHide)
			{
				switch (EPlantOperationState_0)
				{
				case EPlantOperationState.Idling:
					if (Gparam_0.Item.CanBeHiddenDuringThrow)
					{
						goto case EPlantOperationState.StateIn;
					}
					goto default;
				case EPlantOperationState.StateIn:
					Gparam_0.FirearmsAnimator.SetFireMode(Weapon.EFireMode.grenadeThrowing);
					Action_0 = onHidden;
					State = EOperationState.Finished;
					Gparam_0.InitiateOperation<Class1279>().Start(onHidden);
					break;
				default:
					Action_0 = onHidden;
					break;
				case EPlantOperationState.Planted:
					onHidden();
					break;
				}
			}

			public override void PutGrenadeBack()
			{
				Gparam_0.firearmsAnimator_0.SetGrenadeAltFire(FirearmsAnimator.EGrenadeFire.Idle);
				Gparam_0.firearmsAnimator_0.SetGrenadeFire(FirearmsAnimator.EGrenadeFire.Idle);
				method_2();
			}

			public override bool CanRemove()
			{
				return true;
			}

			public void method_2()
			{
				OnEnd();
				State = EOperationState.Finished;
				Gparam_0.InitiateOperation<Class1277>().Start();
			}

			public override bool CanChangeFireMode(Weapon.EFireMode fireMode)
			{
				if (fireMode != Weapon.EFireMode.greanadePlanting && fireMode != Weapon.EFireMode.grenadeThrowing)
				{
					return false;
				}
				if (EPlantOperationState_0 != EPlantOperationState.Idling)
				{
					return false;
				}
				return true;
			}

			public override void ChangeFireMode(Weapon.EFireMode fireMode)
			{
				if (CanChangeFireMode(fireMode))
				{
					Gparam_0.SetFireModeVisual(fireMode);
					method_2();
					Gparam_0.CurrentFireMode = fireMode;
				}
			}

			public override void SetInventoryOpened(bool opened)
			{
				if (EPlantOperationState_0 == EPlantOperationState.Idling)
				{
					Gparam_0.firearmsAnimator_0.SetInventory(opened);
				}
			}

			public override void ShowGesture(EInteraction gesture)
			{
				if (EPlantOperationState_0 == EPlantOperationState.Idling)
				{
					Gparam_0.firearmsAnimator_0.Gesture(gesture);
				}
			}

			public override void ExamineWeapon()
			{
				if (EPlantOperationState_0 == EPlantOperationState.Idling)
				{
					Gparam_0.firearmsAnimator_0.LookTrigger();
				}
			}

			public override void FastForward()
			{
				base.FastForward();
				if (EPlantOperationState_0 == EPlantOperationState.StateIn)
				{
					EPlantOperationState_0 = EPlantOperationState.Idling;
				}
				if (EPlantOperationState_0 == EPlantOperationState.Idling)
				{
					Gparam_0.ChangeFireMode(Weapon.EFireMode.grenadeThrowing);
				}
			}

			public override void Reset()
			{
				base.Reset();
				EPlantOperationState_0 = EPlantOperationState.None;
				Action_0 = null;
				PlantingKitsItemClass = null;
			}
		}

		public class Class1279 : Class1272
		{
			[NonSerialized]
			public Action Action_0;

			public Class1279(GrenadeHandsController controller)
				: base(controller)
			{
			}

			public void Start(Action callback)
			{
				Action_0 = callback;
				Start();
				Gparam_0.firearmsAnimator_0.SetActiveParam(active: false);
				Gparam_0._player.BodyAnimatorCommon.SetFloat(PlayerAnimator.RELOAD_FLOAT_PARAM_HASH, 1f);
			}

			public override void Reset()
			{
				Action_0 = null;
				base.Reset();
			}

			public override void OnHideCompleteActionAction()
			{
				State = EOperationState.Finished;
				Action_0();
			}

			public override void HideGrenade(Action onHidden, bool fastHide)
			{
				Action_0 = (Action)Delegate.Combine(Action_0, onHidden);
			}

			public override void FastForward()
			{
				if (State != EOperationState.Finished)
				{
					OnHideCompleteActionAction();
				}
			}
		}

		public class Class1274 : Class1273
		{
			[NonSerialized]
			public const float Float_0 = 0.25f;

			[NonSerialized]
			public float Float_1;

			[NonSerialized]
			public bool Bool_0;

			public Class1274(GrenadeHandsController controller)
				: base(controller)
			{
			}

			public override void Start(Item item, Callback callback)
			{
				Float_1 = 0f;
				Bool_0 = false;
				base.Start(item, callback);
			}

			public override void FastForward()
			{
				if (!Bool_0)
				{
					Bool_0 = true;
					OnBackpackDrop();
				}
			}

			public override void Update(float deltaTime)
			{
				base.Update(deltaTime);
				if (!Bool_0 && Float_1 > 0.25f)
				{
					Bool_0 = true;
					OnBackpackDrop();
				}
				Float_1 += deltaTime;
			}
		}

		public class Class1280 : Class1272
		{
			[NonSerialized]
			public Action Action_0;

			[NonSerialized]
			public Action Action_1;

			public Class1280(GrenadeHandsController controller)
				: base(controller)
			{
			}

			public void Start(Action callback)
			{
				Action_0 = callback;
				Start();
				Gparam_0.firearmsAnimator_0.SetActiveParam(active: true);
				Gparam_0._player.BodyAnimatorCommon.SetFloat(PlayerAnimator.WEAPON_SIZE_MODIFIER_PARAM_HASH, 1f);
				Gparam_0._player.BodyAnimatorCommon.SetFloat(PlayerAnimator.RELOAD_FLOAT_PARAM_HASH, 1f);
			}

			public override void Reset()
			{
				Action_0 = null;
				Action_1 = null;
				base.Reset();
			}

			public override void OnDrawCompleteAction()
			{
				Gparam_0.SetupProp();
				State = EOperationState.Finished;
				Class1277 @class = Gparam_0.InitiateOperation<Class1277>();
				@class.Start();
				Action_0();
				Gparam_0._player.BodyAnimatorCommon.SetFloat(PlayerAnimator.RELOAD_FLOAT_PARAM_HASH, 0f);
				if (Action_1 != null)
				{
					@class.HideGrenade(Action_1, fastHide: false);
				}
			}

			public override void SetInventoryOpened(bool opened)
			{
				Gparam_0.firearmsAnimator_0.SetInventory(opened);
			}

			public override void HideGrenade(Action onHidden, bool fastHide)
			{
				Action_1 = onHidden;
			}

			public override void FastForward()
			{
				if (State != EOperationState.Finished)
				{
					OnDrawCompleteAction();
				}
			}

			public override void SetLeftStanceAnimOnStartOperation()
			{
				Gparam_0._player.MovementContext.LeftStanceController.DisableLeftStanceAnimFromHandsAction();
			}
		}

		private bool bool_0;

		private Callback<IHandsThrowController> callback_0;

		[CompilerGenerated]
		private Weapon.EFireMode efireMode_0 = Weapon.EFireMode.grenadeThrowing;

		public Class1272 CurrentOperation => base.CurrentHandsOperation as Class1272;

		public bool WaitingForHighThrow
		{
			get
			{
				if (CurrentOperation is Class1275 @class)
				{
					return @class.WaitingHighThrow;
				}
				return false;
			}
		}

		public new ThrowWeapItemClass Item => base.Item;

		public Weapon.EFireMode CurrentFireMode
		{
			[CompilerGenerated]
			get
			{
				return efireMode_0;
			}
			[CompilerGenerated]
			set
			{
				efireMode_0 = value;
			}
		}

		public bool WaitingForLowThrow
		{
			get
			{
				if (CurrentOperation is Class1276 @class)
				{
					return @class.WaitingLowThrow;
				}
				return false;
			}
		}

		public static T smethod_9<T>(Player player, ThrowWeapItemClass item) where T : GrenadeHandsController
		{
			return BaseGrenadeHandsController.smethod_6<T>(player, item, setQuickThrowParameters: false);
		}

		public static Task<T> smethod_10<T>(Player player, ThrowWeapItemClass item) where T : GrenadeHandsController
		{
			return BaseGrenadeHandsController.smethod_7<T>(player, item, setQuickThrowParameters: false);
		}

		public override bool CanExecute(GInterface438 operation)
		{
			if (!(operation is GInterface443 gInterface))
			{
				return true;
			}
			if (_player.InventoryController.IsAnimatedSlot(gInterface.From1))
			{
				if (!(CurrentOperation is Class1277))
				{
					return CurrentOperation is TripwireStateManagerClass;
				}
				return true;
			}
			return true;
		}

		public override void Execute(GInterface438 operation, Callback callback)
		{
			base.GInterface193_0.Execute(operation, callback);
		}

		public override void Spawn(float animationSpeed, Action callback)
		{
			InitiateOperation<Class1280>().Start(callback);
			base.Spawn(animationSpeed, callback);
		}

		public virtual void ExamineWeapon()
		{
			CurrentOperation.ExamineWeapon();
		}

		public virtual void PullRingForHighThrow()
		{
			CurrentOperation.PullRingForHighThrow();
		}

		public virtual void HighThrow()
		{
			CurrentOperation.HighThrow();
		}

		public void SetFireModeVisual(Weapon.EFireMode fireMode)
		{
			_player.OnShowFireMode?.Invoke(fireMode);
		}

		public bool CanChangeFireMode(Weapon.EFireMode fireMode)
		{
			return CurrentOperation.CanChangeFireMode(fireMode);
		}

		public virtual void ChangeFireMode(Weapon.EFireMode fireMode)
		{
			CurrentOperation.ChangeFireMode(fireMode);
		}

		public virtual void HandleFireInput()
		{
			if (WaitingForHighThrow)
			{
				HighThrow();
				return;
			}
			Class1272 currentOperation = CurrentOperation;
			if (!(currentOperation is Class1277))
			{
				if (currentOperation is TripwireStateManagerClass tripwireStateManagerClass)
				{
					tripwireStateManagerClass.HandleFireInput();
				}
			}
			else
			{
				PullRingForHighThrow();
			}
		}

		public virtual void HandleAltFireInput()
		{
			if (WaitingForLowThrow)
			{
				LowThrow();
				return;
			}
			Class1272 currentOperation = CurrentOperation;
			if (!(currentOperation is Class1277))
			{
				if (currentOperation is TripwireStateManagerClass tripwireStateManagerClass)
				{
					tripwireStateManagerClass.HandleAltFireInput();
				}
			}
			else
			{
				PullRingForLowThrow();
			}
		}

		public virtual void PlantTripwire()
		{
			CurrentOperation.PlantTripwire();
		}

		public virtual void PullRingForLowThrow()
		{
			CurrentOperation.PullRingForLowThrow();
		}

		public virtual void LowThrow()
		{
			CurrentOperation.LowThrow();
		}

		public virtual void SetOnUsedCallback(Callback<IHandsThrowController> callback)
		{
			callback_0 = callback;
		}

		public override void SetInventoryOpened(bool opened)
		{
			if (opened)
			{
				SetCompassState(active: false);
			}
			CurrentOperation.SetInventoryOpened(opened);
		}

		public override bool IsInventoryOpen()
		{
			return firearmsAnimator_0.IsInInventory;
		}

		public override bool CanRemove()
		{
			return CurrentOperation.CanRemove();
		}

		public virtual void vmethod_3()
		{
		}

		public virtual bool CanThrow()
		{
			return _player.StateIsSuitableForHandInput;
		}

		public override void SetCompassState(bool active)
		{
			if (CanChangeCompassState(active))
			{
				CurrentOperation.SetGrenadeCompassState(active);
			}
		}

		public override Dictionary<Type, OperationFactoryDelegate> GetOperationFactoryDelegates()
		{
			return new Dictionary<Type, OperationFactoryDelegate>
			{
				{
					typeof(Class1280),
					() => new Class1280(this)
				},
				{
					typeof(Class1277),
					() => new Class1277(this)
				},
				{
					typeof(Class1279),
					() => new Class1279(this)
				},
				{
					typeof(Class1275),
					() => new Class1275(this)
				},
				{
					typeof(Class1276),
					() => new Class1276(this)
				},
				{
					typeof(TripwireStateManagerClass),
					() => new TripwireStateManagerClass(this)
				},
				{
					typeof(Class1273),
					() => new Class1273(this)
				}
			};
		}

		[CompilerGenerated]
		public BaseAnimationOperationClass method_13()
		{
			return new Class1280(this);
		}

		[CompilerGenerated]
		public BaseAnimationOperationClass method_14()
		{
			return new Class1277(this);
		}

		[CompilerGenerated]
		public BaseAnimationOperationClass method_15()
		{
			return new Class1279(this);
		}

		[CompilerGenerated]
		public BaseAnimationOperationClass method_16()
		{
			return new Class1275(this);
		}

		[CompilerGenerated]
		public BaseAnimationOperationClass method_17()
		{
			return new Class1276(this);
		}

		[CompilerGenerated]
		public BaseAnimationOperationClass method_18()
		{
			return new TripwireStateManagerClass(this);
		}

		[CompilerGenerated]
		public BaseAnimationOperationClass method_19()
		{
			return new Class1273(this);
		}
	}

	public abstract class Class1271<T> : BaseAnimationOperationClass, GInterface193 where T : BaseGrenadeHandsController
	{
		[NonSerialized]
		public T Gparam_0;

		public Class1271(T controller)
			: base(controller)
		{
			Gparam_0 = controller;
		}

		public virtual void OnIdleStartAction()
		{
			method_0();
		}

		public virtual void OnDrawCompleteAction()
		{
			method_0();
		}

		public virtual void OnHideCompleteActionAction()
		{
			method_0();
		}

		public virtual void OnDropGrenadeAction()
		{
			method_0();
		}

		public virtual void OnDropFinishedAction()
		{
			method_0();
		}

		public virtual void StartCountdown()
		{
			method_0();
		}

		public virtual void HideGrenade(Action onHidden, bool fastHide)
		{
			method_0();
		}

		public virtual void PutGrenadeBack()
		{
			method_0();
		}

		public virtual void ShowGesture(EInteraction gesture)
		{
			method_0();
		}

		public virtual bool CanRemove()
		{
			return false;
		}

		public virtual void FastForward()
		{
		}

		public virtual void OnBackpackDrop()
		{
			method_0();
		}

		public virtual void Execute(GInterface438 operation, Callback callback)
		{
			if (!(operation is GInterface443 gInterface))
			{
				callback.Succeed();
			}
			else if (Gparam_0._player.InventoryController.IsAnimatedSlot(gInterface.From1))
			{
				callback.Fail($"Detach is not supported in current operation: {GetType()}");
			}
			else
			{
				callback.Succeed();
			}
		}
	}

	public class QuickGrenadeThrowHandsController : BaseGrenadeHandsController, GInterface206, GInterface205<ThrowWeapItemClass>, GInterface204, IHandsController
	{
		public class Class1281(QuickGrenadeThrowHandsController controller) : Class1271<QuickGrenadeThrowHandsController>(controller)
		{
			[NonSerialized]
			public Action Action_0;

			[NonSerialized]
			public Action Action_1;

			[NonSerialized]
			public Callback<GInterface205<ThrowWeapItemClass>> Callback_0;

			[NonSerialized]
			public bool Bool_0;

			[NonSerialized]
			public bool Bool_1;

			[NonSerialized]
			public float Float_0 = -1f;

			public void Start(Action callback)
			{
				Gparam_0._player.Say(EPhraseTrigger.OnGrenade);
				Action_0 = callback;
				Start();
				Bool_1 = false;
				Bool_0 = false;
			}

			public void SetOnUsedCallback(Callback<GInterface205<ThrowWeapItemClass>> callback)
			{
				Callback_0 = callback;
			}

			public override void OnDropGrenadeAction()
			{
				Bool_1 = true;
				Gparam_0.vmethod_1(Float_0);
			}

			public override void OnDropFinishedAction()
			{
				Bool_0 = true;
				Gparam_0.firearmsAnimator_0.SetQuickFire(quickFire: false);
				if (Gparam_0.Destroyed)
				{
					Action_1();
				}
				else if (Callback_0 != null)
				{
					Callback_0(Gparam_0);
				}
			}

			public override void OnDrawCompleteAction()
			{
				Action_0();
			}

			public override void HideGrenade(Action onHidden, bool fastHide = false)
			{
				if (Bool_0)
				{
					onHidden();
				}
				else if (Action_1 == null)
				{
					Action_1 = onHidden;
				}
				else
				{
					Action_1 = (Action)Delegate.Combine(Action_1, onHidden);
				}
			}

			public override void StartCountdown()
			{
				Float_0 = 0f;
			}

			public override void Update(float deltaTime)
			{
				base.Update(deltaTime);
				if (Float_0 >= 0f)
				{
					Float_0 += deltaTime;
				}
			}

			public override void Reset()
			{
				base.Reset();
				Float_0 = -1f;
				Bool_1 = false;
				Bool_0 = false;
				Action_1 = null;
				Action_0 = null;
				Callback_0 = null;
			}

			public override void FastForward()
			{
				if (!Bool_0)
				{
					OnDropFinishedAction();
				}
			}

			public override void SetLeftStanceAnimOnStartOperation()
			{
				Gparam_0._player.MovementContext.LeftStanceController.DisableLeftStanceAnimFromHandsAction();
			}
		}

		[CompilerGenerated]
		public class Class1248
		{
			public Action callback;

			public void method_0()
			{
				callback();
			}
		}

		public Class1281 Class1281_0 => base.CurrentHandsOperation as Class1281;

		public static T smethod_9<T>(Player player, ThrowWeapItemClass item) where T : QuickGrenadeThrowHandsController
		{
			return BaseGrenadeHandsController.smethod_6<T>(player, item, setQuickThrowParameters: true);
		}

		public static Task<T> smethod_10<T>(Player player, ThrowWeapItemClass item) where T : QuickGrenadeThrowHandsController
		{
			return BaseGrenadeHandsController.smethod_7<T>(player, item, setQuickThrowParameters: true);
		}

		public void SetOnUsedCallback(Callback<GInterface205<ThrowWeapItemClass>> callback)
		{
			Class1281_0.SetOnUsedCallback(callback);
		}

		public override void Spawn(float animationSpeed, Action callback)
		{
			Action callback2 = delegate
			{
				callback();
			};
			InitiateOperation<Class1281>().Start(callback2);
			base.Spawn(animationSpeed, callback);
		}

		public override bool CanExecute(GInterface438 operation)
		{
			return false;
		}

		public override bool CanRemove()
		{
			return true;
		}

		public override Dictionary<Type, OperationFactoryDelegate> GetOperationFactoryDelegates()
		{
			return new Dictionary<Type, OperationFactoryDelegate> { 
			{
				typeof(Class1281),
				() => new Class1281(this)
			} };
		}

		[CompilerGenerated]
		public BaseAnimationOperationClass method_13()
		{
			return new Class1281(this);
		}
	}

	public interface Interface9
	{
		void OnDrawCompleteAction();

		void OnHideCompleteActionAction();

		void OnUseAction();

		void HideController(Action onHidden, bool fastHide);
	}

	public class QuickUseItemController : ItemHandsController, IOnHandsUseCallback, IHandsController
	{
		public abstract class GClass2057 : BaseAnimationOperationClass, Interface9
		{
			[NonSerialized]
			public QuickUseItemController QuickUseItemController_0;

			public GClass2057(QuickUseItemController controller)
				: base(controller)
			{
				QuickUseItemController_0 = controller;
			}

			public virtual void OnDrawCompleteAction()
			{
				method_0();
			}

			public virtual void OnHideCompleteActionAction()
			{
				method_0();
			}

			public virtual void OnUseAction()
			{
				method_0();
			}

			public virtual void HideController(Action onHidden, bool fastHide)
			{
				method_0();
			}
		}

		public class GClass2058 : GClass2057
		{
			[NonSerialized]
			public Action Action_0;

			[NonSerialized]
			public Action Action_1;

			[NonSerialized]
			public Callback<IOnHandsUseCallback> Callback_0;

			public GClass2058(QuickUseItemController controller)
				: base(controller)
			{
			}

			public void Start(Action callback)
			{
				Action_0 = callback;
				QuickUseItemController_0._objectInHandsAnimator.SetActiveParam(active: true);
				Start();
			}

			public void SetOnUsedCallback(Callback<IOnHandsUseCallback> callback)
			{
				Callback_0 = callback;
			}

			public Callback<IOnHandsUseCallback> GetOnUsedCallback()
			{
				return Callback_0;
			}

			public override void OnUseAction()
			{
				QuickUseItemController_0._objectInHandsAnimator.SetActiveParam(active: false);
				QuickUseItemController_0.method_4();
				if (QuickUseItemController_0.Destroyed)
				{
					Action action_ = Action_1;
					Action_1 = null;
					action_();
				}
				else if (Callback_0 != null)
				{
					Callback<IOnHandsUseCallback> callback_ = Callback_0;
					Callback_0 = null;
					callback_(QuickUseItemController_0);
				}
			}

			public override void OnDrawCompleteAction()
			{
				Action_0();
			}

			public override void Reset()
			{
				base.Reset();
				Action_1 = null;
				Action_0 = null;
				Callback_0 = null;
			}

			public virtual void FastForward()
			{
				method_0();
			}
		}

		[CompilerGenerated]
		public class Class1249
		{
			public Action callback;

			public void method_0()
			{
				callback();
			}
		}

		[CompilerGenerated]
		public class Class1250
		{
			public Action callback;

			public void method_0()
			{
				callback();
			}
		}

		[CompilerGenerated]
		public class Class1251<T> where T : QuickUseItemController
		{
			public T controller;

			public void method_0()
			{
				controller._objectInHandsAnimator.RemoveEventsConsumer(controller);
			}
		}

		protected GameObject _usableItemGameObject;

		protected new FirearmsAnimator _objectInHandsAnimator;

		public override FirearmsAnimator FirearmsAnimator => _objectInHandsAnimator;

		public override string LoggerDistinctId => $"{_player.ProfileId}|{_player.Profile.Info.Nickname}|{this}";

		public GClass2058 CurrentOperation => base.CurrentHandsOperation as GClass2058;

		public static T smethod_6<T>(Player player, Item item) where T : QuickUseItemController
		{
			T controller = ItemHandsController.smethod_1<T>(player, item, Singleton<PoolManagerClass>.Instance.CreateItemUsablePrefab);
			UsableHandsPrefab component = controller._controllerObject.GetComponent<UsableHandsPrefab>();
			GameObject gameObject = Singleton<PoolManagerClass>.Instance.CreateItem(item, isAnimated: true);
			gameObject.transform.SetParent(component.ItemSpawnTransform);
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localRotation = Quaternion.identity;
			gameObject.SetActive(value: true);
			controller._usableItemGameObject = gameObject;
			controller._objectInHandsAnimator = component.FirearmsAnimator;
			controller._objectInHandsAnimator.AddEventsConsumer(controller);
			controller.CompositeDisposable.AddDisposable(delegate
			{
				controller._objectInHandsAnimator.RemoveEventsConsumer(controller);
			});
			controller._objectInHandsAnimator.SkipTime(1f / 60f);
			controller._player.HandsAnimator = controller._objectInHandsAnimator;
			bool flag = controller._player.UpdateGrenadeAnimatorDuePoV();
			controller._objectInHandsAnimator.Animator.SetFloat("ThirdPersonAnimation", flag ? 1 : 0);
			player.ProceduralWeaponAnimation.ClearPreviousWeapon();
			player.ProceduralWeaponAnimation.InitTransforms(controller.HandsHierarchy);
			component.ObjectInHands.AfterGetFromPoolInit(player.ProceduralWeaponAnimation, null, player.IsYourPlayer);
			controller._controllerObject.GetComponent<BaseSoundPlayer>().Init(controller, player.PlayerBones.WeaponRoot, player);
			return controller;
		}

		public override void IEventsConsumerOnWeapIn()
		{
			method_1();
		}

		public override void IEventsConsumerOnWeapOut()
		{
			method_2();
		}

		public override void IEventsConsumerOnFiringBullet()
		{
			method_3();
		}

		public override void IEventsConsumerOnThirdAction(int i)
		{
			TranslateAnimatorParameter(i);
		}

		public override Dictionary<Type, OperationFactoryDelegate> GetOperationFactoryDelegates()
		{
			return new Dictionary<Type, OperationFactoryDelegate> { 
			{
				typeof(GClass2058),
				() => new GClass2058(this)
			} };
		}

		public override void Spawn(float animationSpeed, Action callback)
		{
			Action callback2 = delegate
			{
				callback();
			};
			InitiateOperation<GClass2058>().Start(callback2);
		}

		public override void Drop(float animationSpeed, Action callback, bool fastDrop, Item nextControllerItem = null)
		{
			if (!base.Destroyed)
			{
				base.Destroyed = true;
				_objectInHandsAnimator.SetAnimationSpeed(animationSpeed);
				((Action)delegate
				{
					callback();
				})();
			}
		}

		public override void Destroy()
		{
			_player.ProceduralWeaponAnimation.ClearPreviousWeapon();
			base.Destroy();
			_objectInHandsAnimator = null;
			AssetPoolObject.ReturnToPool(_controllerObject.gameObject);
		}

		public override bool CanExecute(GInterface438 operation)
		{
			if (operation is GClass3498 gClass)
			{
				return gClass.Item == base.Item;
			}
			if (operation is GClass3492 gClass2)
			{
				return gClass2.Tripwire == base.Item;
			}
			return false;
		}

		public override void Execute(GInterface438 operation, Callback callback)
		{
			callback.Succeed();
		}

		public override bool CanRemove()
		{
			return false;
		}

		public override bool CanInteract()
		{
			return false;
		}

		public override void Interact(bool isInteracting, int actionIndex)
		{
		}

		public void SetOnUsedCallback(Callback<IOnHandsUseCallback> callback)
		{
			CurrentOperation.SetOnUsedCallback(callback);
		}

		public Callback<IOnHandsUseCallback> GetOnUsedCallback()
		{
			return CurrentOperation.GetOnUsedCallback();
		}

		public void method_1()
		{
			CurrentOperation.OnDrawCompleteAction();
		}

		public void method_2()
		{
			CurrentOperation.OnHideCompleteActionAction();
		}

		public void method_3()
		{
			CurrentOperation.OnUseAction();
		}

		public void method_4()
		{
			AssetPoolObject.ReturnToPool(_usableItemGameObject);
			_usableItemGameObject = null;
		}

		public override void ShowGesture(EInteraction gesture)
		{
		}

		public override void FastForwardCurrentState()
		{
			CurrentOperation.FastForward();
		}

		[CompilerGenerated]
		public BaseAnimationOperationClass method_5()
		{
			return new GClass2058(this);
		}
	}

	public enum EOperationState
	{
		Ready,
		Executing,
		Finished
	}

	public abstract class ItemHandsController : AbstractHandsController, IHandsController
	{
		public delegate GameObject Delegate8(Item item, Player player);

		public delegate Task<GameObject> Delegate9(Item item, Player player);

		public delegate BaseAnimationOperationClass OperationFactoryDelegate();

		public class HandsControllerLogger : LoggerClass
		{
			[NonSerialized]
			public string String_0;

			public HandsControllerLogger(LoggerMode loggerMode, ItemHandsController controller)
				: base("hands-states", loggerMode)
			{
				String_0 = controller._player.ProfileId + "|" + controller._player.Profile.Nickname;
			}

			public void TraceStateChange(BaseAnimationOperationClass currentOperation, BaseAnimationOperationClass nextOperation)
			{
				if (IsEnabled(NLog.LogLevel.Trace))
				{
					Log("[{3}][{0}] -> [{1}] [{2}]", "<b>[{3}][{0}] -> [{1}]  [{2}]</b>", NLog.LogLevel.Trace, currentOperation, nextOperation, String_0, Time.frameCount);
				}
			}

			[Conditional("UNITY_EDITOR")]
			public void TraceMethodCall()
			{
				if (IsEnabled(NLog.LogLevel.Trace))
				{
					StackFrame stackFrame = new StackFrame(1);
					Log("[{2}] HANDS OPERATION CALL: {0}::{1} [{3}]", "<color=green>[{2}] HANDS OPERATION CALL:</color> <b>{0}::{1} [{3}]</b>", NLog.LogLevel.Trace, stackFrame.GetMethod().DeclaringType, stackFrame.GetMethod().Name, Time.frameCount, String_0);
				}
			}

			[Conditional("UNITY_EDITOR")]
			public void TraceMethodCall<T>(string argName, T argument1)
			{
				if (IsEnabled(NLog.LogLevel.Trace))
				{
					StackFrame stackFrame = new StackFrame(1);
					Log("[{4}] HANDS OPERATION CALL: {0}::{1}({2}:{3}) [{5}]", "<color=green>[{4}] HANDS OPERATION CALL:</color> <b>{0}::{1}({2}:{3}) [{5}]</b>", NLog.LogLevel.Trace, stackFrame.GetMethod().DeclaringType, stackFrame.GetMethod().Name, argName, argument1, Time.frameCount, String_0);
				}
			}
		}

		[Serializable]
		[CompilerGenerated]
		public class Class1252<T> where T : ItemHandsController
		{
			public static readonly Class1252<T> class1252_0 = new Class1252<T>();

			public static Delegate8 delegate8_0;

			public GameObject method_0(Item item1, Player player1)
			{
				return Singleton<PoolManagerClass>.Instance.CreateItem(item1, GetVisibleToCamera(player1), player1, isAnimated: true);
			}
		}

		[Serializable]
		[CompilerGenerated]
		public class Class1253<T> where T : ItemHandsController
		{
			public static readonly Class1253<T> class1253_0 = new Class1253<T>();

			public static Delegate9 delegate9_0;

			public Task<GameObject> method_0(Item item1, Player player1)
			{
				return Singleton<PoolManagerClass>.Instance.CreateItemAsync(item1, GetVisibleToCamera(player1), player1, isAnimated: true, JobPriorityClass.General);
			}
		}

		protected FirearmsAnimator _objectInHandsAnimator;

		private readonly Dictionary<Type, BaseAnimationOperationClass> dictionary_0 = new Dictionary<Type, BaseAnimationOperationClass>();

		protected GameObject _controllerObject;

		protected TransformLinks _handsHierarchy;

		private Item item_0;

		private Dictionary<Type, OperationFactoryDelegate> dictionary_1;

		protected internal Player _player;

		[CompilerGenerated]
		private readonly global::BindableStateClass<bool> gclass1643_0 = new global::BindableStateClass<bool>(initialValue: false);

		protected global::BindableStateClass<bool> RadioTransmitterState = new global::BindableStateClass<bool>(initialValue: false);

		protected HandsControllerLogger Logger;

		[CompilerGenerated]
		private BaseAnimationOperationClass baseAnimationOperationClass;

		public override FirearmsAnimator FirearmsAnimator => _objectInHandsAnimator;

		public virtual global::BindableStateClass<bool> CompassState
		{
			[CompilerGenerated]
			get
			{
				return gclass1643_0;
			}
		}

		public bool SuitableForHandInput => _player.StateIsSuitableForHandInput;

		public bool CurrentCompassState => CompassState.Value;

		public bool CurrentRadioTransmitterState => RadioTransmitterState.Value;

		public override GameObject ControllerGameObject => _controllerObject;

		public override float AimingSensitivity => _player.GetAimingSensitivity();

		public override TransformLinks HandsHierarchy => _handsHierarchy;

		public BaseAnimationOperationClass CurrentHandsOperation
		{
			[CompilerGenerated]
			get
			{
				return baseAnimationOperationClass;
			}
			[CompilerGenerated]
			set
			{
				baseAnimationOperationClass = value;
			}
		}

		public string CurrentHandsOperationName
		{
			get
			{
				if (CurrentHandsOperation == null)
				{
					return string.Empty;
				}
				string text = CurrentHandsOperation.GetType().ToString();
				if (!text.Contains("+"))
				{
					return text;
				}
				return text.Split('+')[^1];
			}
		}

		Item IHandsController.Item => item_0;

		public override void BlindFire(int b)
		{
		}

		public override Item GetItem()
		{
			return item_0;
		}

		public static T smethod_0<T>(Player player, Item item) where T : ItemHandsController
		{
			if (player.PlayerBody != null)
			{
				player.PlayerBody.GetSlotViewByItem(item)?.DestroyCurrentModel();
			}
			return smethod_1<T>(player, item, (Item item2, Player player2) => Singleton<PoolManagerClass>.Instance.CreateItem(item2, GetVisibleToCamera(player2), player2, isAnimated: true));
		}

		public static T smethod_1<T>(Player player, Item item, Delegate8 itemObjectFactoryDelegate) where T : ItemHandsController
		{
			T val = player.gameObject.AddComponent<T>();
			val._controllerObject = itemObjectFactoryDelegate(item, player);
			smethod_4(val, player, item);
			return val;
		}

		public static Task<T> smethod_2<T>(Player player, Item item) where T : ItemHandsController
		{
			return smethod_3<T>(player, item, (Item item2, Player player2) => Singleton<PoolManagerClass>.Instance.CreateItemAsync(item2, GetVisibleToCamera(player2), player2, isAnimated: true, JobPriorityClass.General));
		}

		public static async Task<T> smethod_3<T>(Player player, Item item, Delegate9 itemObjectAsyncFactoryDelegate) where T : ItemHandsController
		{
			T val = player.gameObject.AddComponent<T>();
			T val2 = val;
			val2._controllerObject = await itemObjectAsyncFactoryDelegate(item, player);
			smethod_4(val, player, item);
			return val;
		}

		public static void smethod_4<T>(T controller, Player player, Item item) where T : ItemHandsController
		{
			WeaponPrefab component = controller._controllerObject.GetComponent<WeaponPrefab>();
			controller._handsHierarchy = component.Hierarchy;
			controller._objectInHandsAnimator = component.FirearmsAnimator;
			controller.AnimationEventsEmitter = component.AnimationEventsEmitter;
			controller._player = player;
			controller._controllerObject.transform.SetPositionAndRotation(player.PlayerBones.Ribcage.Original.position, player.HandsRotation);
			player.UpdateBonesOnWeaponChange(controller._handsHierarchy);
			controller.item_0 = item;
			controller.WeaponRoot = player.PlayerBones.WeaponRoot.Original;
			smethod_5(controller, player);
			if (player.UsedSimplifiedSkeleton)
			{
				if (item.Id == "670ad7f1ad195290cd00da7a")
				{
					player.BodyAnimatorCommon.SetFloat(PlayerAnimator.ZOMBIE_WEAPON_TYPE_FLOAT_HASH, 0f);
				}
				else if (item is KnifeItemClass)
				{
					controller._controllerObject.transform.SetParent(player.PlayerBones.WeaponMeleeRoot);
					player.BodyAnimatorCommon.SetFloat(PlayerAnimator.ZOMBIE_WEAPON_TYPE_FLOAT_HASH, 1f);
				}
				else if (item is PistolItemClass)
				{
					controller._controllerObject.transform.SetParent(player.PlayerBones.WeaponPistolRoot);
					player.BodyAnimatorCommon.SetFloat(PlayerAnimator.ZOMBIE_WEAPON_TYPE_FLOAT_HASH, 2f);
				}
				controller._controllerObject.transform.localPosition = Vector3.zero;
				controller._controllerObject.transform.localRotation = Quaternion.identity;
			}
			controller.Logger = new HandsControllerLogger(LoggerMode.Add, controller);
		}

		public static void smethod_5<T>(T controller, Player player) where T : ItemHandsController
		{
			player.MovementContext.PlayerAnimator.EventsDispatcher.PlayerAnimatorEvents.DropBackpackEvents.OnBackpackDropEvent += controller.OnBackpackDrop;
			player.MovementContext.PlayerAnimator.EventsDispatcher.PlayerAnimatorEvents.BipodToggleEvents.OnBipodToggleEvent += controller.OnBipodToggle;
			player.MovementContext.PlayerAnimator.EventsDispatcher.PlayerAnimatorEvents.ZombieFireBulletEvents.OnZombieFireBulletEvent += controller.OnZombieFireBullet;
			player.MovementContext.PlayerAnimator.EventsDispatcher.PlayerAnimatorEvents.ZombieFireEndEvents.OnZombieFireEndEvent += controller.OnZombieFireEnd;
		}

		public void method_0()
		{
			_player.MovementContext.PlayerAnimator.EventsDispatcher.PlayerAnimatorEvents.DropBackpackEvents.OnBackpackDropEvent -= base.OnBackpackDrop;
			_player.MovementContext.PlayerAnimator.EventsDispatcher.PlayerAnimatorEvents.BipodToggleEvents.OnBipodToggleEvent -= base.OnBipodToggle;
			_player.MovementContext.PlayerAnimator.EventsDispatcher.PlayerAnimatorEvents.ZombieFireBulletEvents.OnZombieFireBulletEvent -= OnZombieFireBullet;
			_player.MovementContext.PlayerAnimator.EventsDispatcher.PlayerAnimatorEvents.ZombieFireEndEvents.OnZombieFireEndEvent -= OnZombieFireEnd;
		}

		public void TranslateAnimatorParameter(int actionIndex)
		{
			_player.BodyAnimatorCommon.SetInteger(PlayerAnimator.FIRST_PERSON_ACTION, actionIndex);
		}

		public override bool IsPlacingBeacon()
		{
			return CurrentHandsOperation is QuickUseItemController.GClass2058;
		}

		public void SetupProp()
		{
			if (_player.HealthController.IsAlive)
			{
				CompositeDisposable.BindState(CompassState, CompassStateHandler);
				CompositeDisposable.BindState(_player.MovementContext.CanUseProp, OnCanUsePropChanged);
			}
		}

		public virtual void OnCanUsePropChanged(bool canUse)
		{
			if (!canUse)
			{
				SetCompassState(active: false);
			}
		}

		public void SetPropVisibility(bool isVisible)
		{
			if (!isVisible)
			{
				SetCompassState(active: false);
			}
			_player.SetPropVisibility(CompassState.Value);
		}

		public override void ManualUpdate(float deltaTime)
		{
			CurrentHandsOperation.Update(deltaTime);
		}

		public void ToggleCompassState()
		{
			SetCompassState(!CompassState.Value);
		}

		public virtual void SetCompassState(bool active)
		{
			if (CanChangeCompassState(active))
			{
				CompassState.Value = active;
			}
		}

		public void ApplyCompassPacket(GStruct367 packet)
		{
			if (packet.Toggle)
			{
				CompassState.Value = packet.Status;
			}
		}

		public virtual bool CanChangeCompassState(bool newState)
		{
			if (newState)
			{
				if (_player.MovementContext.CanUseProp.Value)
				{
					return !IsInInteractionStrictCheck();
				}
				return false;
			}
			return true;
		}

		public virtual void CompassStateHandler(bool isActive)
		{
			_player.CreateCompass();
			_player.SetCompassState(isActive);
			_objectInHandsAnimator.ShowCompass(isActive);
		}

		public override void BallisticUpdate(float deltaTime)
		{
		}

		public override void EmitEvents()
		{
			base.AnimationEventsEmitter.EmitEvents();
		}

		public abstract Dictionary<Type, OperationFactoryDelegate> GetOperationFactoryDelegates();

		public void ClearPreWarmOperationsDict()
		{
			dictionary_0.Clear();
		}

		public TCreateOperation InitiateOperation<TCreateOperation>() where TCreateOperation : BaseAnimationOperationClass
		{
			if (dictionary_1 == null)
			{
				dictionary_1 = GetOperationFactoryDelegates();
			}
			Type typeFromHandle = typeof(TCreateOperation);
			if (!dictionary_0.ContainsKey(typeFromHandle))
			{
				dictionary_0[typeFromHandle] = dictionary_1[typeFromHandle]();
			}
			BaseAnimationOperationClass baseAnimationOperationClass = dictionary_0[typeFromHandle];
			baseAnimationOperationClass.UpdateLoggerController(this);
			Logger.TraceStateChange(CurrentHandsOperation, baseAnimationOperationClass);
			if (CurrentHandsOperation != null)
			{
				CurrentHandsOperation.OnEnd();
			}
			CurrentHandsOperation = baseAnimationOperationClass;
			CurrentHandsOperation.Reset();
			return (TCreateOperation)CurrentHandsOperation;
		}

		public override string ToString()
		{
			return $"{base.ToString()}, Item: {item_0}, CurrentHandsOperation: {CurrentHandsOperation}";
		}

		public override void Destroy()
		{
			method_0();
			base.Destroy();
		}

		public ItemHandsController()
		{
		}
	}

	public abstract class BaseKnifeController : ItemHandsController
	{
		[CompilerGenerated]
		public class Class1254<T> where T : BaseKnifeController
		{
			public T controller;

			public KnifeComponent knife;

			public PlayerBones method_0()
			{
				return controller._player.PlayerBones;
			}

			public void method_1()
			{
				controller.firearmsAnimator_0.RemoveEventsConsumer(controller);
			}

			public void method_2()
			{
				controller._player.BodyAnimatorCommon.SetLayerWeight(knife.Template.AdditionalAnimationLayer, 0f);
				controller._player.BodyAnimatorCommon.SetLayerWeight(knife.Template.AdditionalAnimationLayer + 1, 0f);
				controller.action_0 = null;
			}
		}

		[CompilerGenerated]
		public class Class1255
		{
			public BaseKnifeController baseKnifeController_0;

			public Class1312 inventoryOperation;

			public Action callback;

			public void method_0()
			{
				baseKnifeController_0._player.ProceduralWeaponAnimation.enabled = true;
				inventoryOperation.Confirm();
				callback();
				baseKnifeController_0.action_0?.Invoke();
			}
		}

		public EKickType LastKickType;

		private float float_0;

		protected Vector3 vector3_0 = Vector3.zero;

		protected Vector3 vector3_1 = Vector3.zero;

		public const float LERP_DIRECTION_T = 0.9f;

		private Action action_0;

		protected bool bool_0 = true;

		protected int int_0 = 1000;

		protected GClass2086 gclass2086_0;

		protected FirearmsAnimator firearmsAnimator_0;

		protected Transform transform_0;

		protected KnifeCollider knifeCollider_0;

		private int int_1;

		private int int_2;

		private int int_3;

		public KnifeComponent Knife => base.Item.GetItemComponent<KnifeComponent>();

		public override string LoggerDistinctId => $"{_player.ProfileId}|{_player.Profile.Info.Nickname}|{this}";

		public Interface10 Interface10_0 => base.CurrentHandsOperation as Interface10;

		public override FirearmsAnimator FirearmsAnimator => firearmsAnimator_0;

		public async Task method_1()
		{
			bool_0 = false;
			await Task.Delay(int_0);
			bool_0 = true;
		}

		public override float GetAnimatorFloatParam(int hash)
		{
			return firearmsAnimator_0.GetAnimatorParameter(hash);
		}

		public override bool SupportPickup()
		{
			return true;
		}

		public override void Pickup(bool p)
		{
			if (CanInteract())
			{
				firearmsAnimator_0.SetPickup(p);
			}
		}

		public override void Interact(bool isInteracting, int actionIndex)
		{
			if (CanInteract())
			{
				_player.SendHandsInteractionStateChanged(isInteracting, actionIndex);
				firearmsAnimator_0.SetInteract(isInteracting, actionIndex);
			}
		}

		public override void Loot(bool p)
		{
			if (CanInteract())
			{
				firearmsAnimator_0.SetLooting(p);
			}
		}

		public override bool CanInteract()
		{
			if (firearmsAnimator_0.IsIdling())
			{
				return firearmsAnimator_0.Animator.GetBool(WeaponAnimationSpeedControllerClass.BOOL_ACTIVE);
			}
			return false;
		}

		public override bool IsInInteraction()
		{
			return firearmsAnimator_0.IsInInteraction;
		}

		public override bool IsInInteractionStrictCheck()
		{
			if (!IsInInteraction() && !(firearmsAnimator_0.GetLayerWeight(firearmsAnimator_0.LACTIONS_LAYER_INDEX) >= float.Epsilon))
			{
				return firearmsAnimator_0.Animator.IsInTransition(firearmsAnimator_0.LACTIONS_LAYER_INDEX);
			}
			return true;
		}

		public Vector3 GetPlayerOrientation()
		{
			return _player.LookDirection;
		}

		public Vector3 GetPlayerCastOrigin()
		{
			return _player.MovementContext.RibcagePosition();
		}

		public override void ShowGesture(EInteraction gesture)
		{
			if (gesture != EInteraction.None)
			{
				firearmsAnimator_0.Gesture(gesture);
			}
		}

		public static T smethod_6<T>(Player player, KnifeComponent knife) where T : BaseKnifeController
		{
			T val = ItemHandsController.smethod_0<T>(player, knife.Item);
			smethod_8(val, player);
			return val;
		}

		public static async Task<T> smethod_7<T>(Player player, KnifeComponent knife) where T : BaseKnifeController
		{
			T obj = await ItemHandsController.smethod_2<T>(player, knife.Item);
			smethod_8(obj, player);
			return obj;
		}

		public static void smethod_8<T>(T controller, Player player) where T : BaseKnifeController
		{
			controller.int_1 = LayerMask.NameToLayer("HighPolyCollider");
			controller.int_2 = LayerMask.GetMask("HitCollider", "HighPolyCollider", "TransparentCollider", "Water");
			controller.int_3 = LayerMask.GetMask("Player");
			WeaponPrefab componentInChildren = controller._controllerObject.GetComponentInChildren<WeaponPrefab>();
			controller.gclass2086_0 = componentInChildren.ObjectInHands;
			controller.firearmsAnimator_0 = componentInChildren.FirearmsAnimator;
			controller.knifeCollider_0 = controller.HandsHierarchy.Self.GetComponentInChildren<KnifeCollider>();
			controller.knifeCollider_0.baseKnifeController_0 = controller;
			controller.knifeCollider_0.player_0 = controller._player;
			controller.knifeCollider_0.isInfectedPlayer = controller._player.UsedSimplifiedSkeleton;
			controller.knifeCollider_0._hitMask = controller.int_2;
			controller.knifeCollider_0._spiritMask = controller.int_3;
			controller.knifeCollider_0.GetPlayerOrientation = controller.GetPlayerOrientation;
			controller.knifeCollider_0.GetPlayerBones = () => controller._player.PlayerBones;
			controller.firearmsAnimator_0.AddEventsConsumer(controller);
			controller.CompositeDisposable.AddDisposable(delegate
			{
				controller.firearmsAnimator_0.RemoveEventsConsumer(controller);
			});
			controller.transform_0 = TransformHelperClass.FindTransformRecursive(player.PlayerBones.WeaponRoot.Original, "damage_collider");
			controller._player.HandsAnimator = controller.firearmsAnimator_0;
			KnifeComponent knife = controller.Item.GetItemComponent<KnifeComponent>();
			if (knife != null && knife.Template.AdditionalAnimationLayer > 0)
			{
				controller._player.BodyAnimatorCommon.SetLayerWeight(knife.Template.AdditionalAnimationLayer, 1f);
				controller._player.BodyAnimatorCommon.SetLayerWeight(knife.Template.AdditionalAnimationLayer + 1, 1f);
				controller.action_0 = delegate
				{
					controller._player.BodyAnimatorCommon.SetLayerWeight(knife.Template.AdditionalAnimationLayer, 0f);
					controller._player.BodyAnimatorCommon.SetLayerWeight(knife.Template.AdditionalAnimationLayer + 1, 0f);
					controller.action_0 = null;
				};
			}
			player.ProceduralWeaponAnimation.ClearPreviousWeapon();
			player.ProceduralWeaponAnimation.InitTransforms(controller.HandsHierarchy);
			controller.gclass2086_0.AfterGetFromPoolInit(player.ProceduralWeaponAnimation, null, player.IsYourPlayer);
			BaseSoundPlayer component = controller._controllerObject.GetComponent<BaseSoundPlayer>();
			if (component != null)
			{
				component.Init(controller, player.PlayerBones.WeaponRoot, player);
			}
		}

		public override void IEventsConsumerOnOnUseProp(bool boolParam)
		{
			SetPropVisibility(boolParam);
		}

		public override void IEventsConsumerOnWeapIn()
		{
			method_4();
		}

		public override void IEventsConsumerOnWeapOut()
		{
			method_3();
		}

		public override void IEventsConsumerOnThirdAction(int intParam)
		{
			TranslateAnimatorParameter(intParam);
		}

		public override void IEventsConsumerOnFireEnd()
		{
			if (!_player.UsedSimplifiedSkeleton)
			{
				OnFireEnd();
			}
		}

		public override void OnZombieFireEnd(IAnimatorEventParameter animatorEventParameter)
		{
			OnFireEnd();
		}

		public override void IEventsConsumerOnIdleStart()
		{
			Interface10_0.OnIdleStart();
		}

		public override void IEventsConsumerOnComboPlanning()
		{
			OnComboPlanning();
		}

		public override void IEventsConsumerOnFiringBullet()
		{
			if (!_player.UsedSimplifiedSkeleton)
			{
				method_2();
			}
		}

		public override void OnZombieFireBullet(IAnimatorEventParameter animatorEventParameter)
		{
			method_2();
		}

		public override void IEventsOnBackpackDrop()
		{
			method_5();
		}

		public override void Drop(float animationSpeed, Action callback, bool fastDrop, Item nextControllerItem = null)
		{
			SetDeflected(deflected: false);
			if (base.Destroyed)
			{
				Interface10_0.HideWeapon(callback, fastDrop);
				return;
			}
			base.Destroyed = true;
			Class1312 inventoryOperation = _player.method_138(Knife.Item);
			Action onHidden = delegate
			{
				_player.ProceduralWeaponAnimation.enabled = true;
				inventoryOperation.Confirm();
				callback();
				action_0?.Invoke();
			};
			Interface10_0.HideWeapon(onHidden, fastDrop);
		}

		public override void Destroy()
		{
			action_0?.Invoke();
			_player.ProceduralWeaponAnimation.ClearPreviousWeapon();
			base.Destroy();
			firearmsAnimator_0 = null;
			AssetPoolObject.ReturnToPool(_controllerObject.gameObject);
		}

		public void method_2()
		{
			Interface10_0.OnFire();
		}

		public new virtual void OnFireEnd()
		{
			Interface10_0.OnFireEnd();
		}

		public new virtual void OnComboPlanning()
		{
		}

		public void method_3()
		{
			Interface10_0.HideWeaponComplete();
		}

		public void method_4()
		{
			Interface10_0.WeaponAppeared();
		}

		public void method_5()
		{
			Interface10_0.OnBackpackDrop();
		}

		public override bool CanExecute(GInterface438 operation)
		{
			return true;
		}

		public override void Execute(GInterface438 operation, Callback callback)
		{
			Interface10_0.Execute(operation, callback);
		}

		public void method_6(GStruct182 other)
		{
			if (knifeCollider_0.OnHit != null)
			{
				KnifeCollider knifeCollider = knifeCollider_0;
				knifeCollider.OnHit = (Action<GStruct182>)Delegate.Remove(knifeCollider.OnHit, new Action<GStruct182>(method_6));
			}
			BallisticCollider component = other.collider.GetComponent<BallisticCollider>();
			if (component != null)
			{
				other.point = ((other.point.sqrMagnitude < 0.1f) ? other.collider.transform.position : other.point);
				vmethod_0(other, component);
				SetDeflected(deflected: true);
				if (other.collider.gameObject.layer == int_1)
				{
					_player.Physical.ConsumeAsMelee(Knife.Template.DeflectionConsumption);
				}
			}
		}

		public void SetDeflected(bool deflected)
		{
			firearmsAnimator_0.SetDeflected(deflected);
			_player.MovementContext.PlayerAnimatorSetDeflected(deflected);
		}

		public void SetMeleeSpeed(float speed)
		{
			firearmsAnimator_0.SetMeleeSpeed(speed);
			_player.MovementContext.PlayerAnimatorSetSwingSpeed(speed);
		}

		public bool method_7(out RaycastHit hitInfo, int layerMask)
		{
			return Physics.SphereCast(transform_0.position, 0.15f, _player.LookDirection, out hitInfo, 0.5f, layerMask);
		}

		public virtual ShotInfoClass vmethod_0(GStruct182 hit, BallisticCollider ballisticCollider)
		{
			int num = ((LastKickType == EKickType.Slash) ? Knife.Template.KnifeHitSlashDam : Knife.Template.KnifeHitStabDam);
			num = (_player.Physical.HandsStamina.Exhausted ? ((int)((float)num * Singleton<BackendConfigSettingsClass>.Instance.Stamina.ExhaustedMeleeDamageMultiplier)) : num);
			vector3_0 = Vector3.Lerp(vector3_0, (knifeCollider_0.transform.position - vector3_1).normalized, 0.9f);
			UnityEngine.Debug.DrawLine(hit.point, hit.point + vector3_0, Color.magenta, 10f);
			DamageInfoStruct damageInfo = new DamageInfoStruct
			{
				DamageType = EDamageType.Melee,
				Damage = (float)num * (1f + (float)_player.Skills.StrengthBuffMeleePowerInc),
				PenetrationPower = ((LastKickType == EKickType.Slash) ? Knife.Template.SlashPenetration : Knife.Template.StabPenetration),
				ArmorDamage = 1f,
				Direction = vector3_0,
				HitCollider = hit.collider,
				HitPoint = hit.point,
				Player = Singleton<GameWorld>.Instance.GetEverExistedBridgeByProfileID(_player.ProfileId),
				HittedBallisticCollider = ballisticCollider,
				HitNormal = hit.normal,
				Weapon = Knife.Item,
				IsForwardHit = true,
				StaminaBurnRate = Knife.Template.StaminaBurnRate
			};
			ShotInfoClass result = Singleton<GameWorld>.Instance.HackShot(damageInfo);
			if (ballisticCollider as BodyPartCollider != null)
			{
				_player.ExecuteSkill((Action)delegate
				{
					_player.Skills.FistfightAction.Complete();
				});
			}
			return result;
		}

		public override void FastForwardCurrentState()
		{
			Interface10_0.FastForward();
		}

		public BaseKnifeController()
		{
		}

		[CompilerGenerated]
		public void method_8()
		{
			_player.Skills.FistfightAction.Complete();
		}
	}

	public interface Interface10
	{
		void HideWeaponComplete();

		void WeaponAppeared();

		void HideWeapon(Action onHidden, bool fastDrop);

		void OnFireEnd();

		void OnFire();

		void OnBackpackDrop();

		void Execute<TInventoryOperation>(TInventoryOperation operation, Callback callback) where TInventoryOperation : GInterface438;

		void FastForward();

		void OnIdleStart();
	}

	public abstract class Class1282<T> : BaseAnimationOperationClass, Interface10 where T : BaseKnifeController
	{
		[NonSerialized]
		public T Gparam_0;

		public Class1282(T controller)
			: base(controller)
		{
			Gparam_0 = controller;
		}

		public virtual void HideWeaponComplete()
		{
			method_0();
		}

		public virtual void WeaponAppeared()
		{
			method_0();
		}

		public virtual void OnBackpackDrop()
		{
			method_0();
		}

		public virtual void Execute<TInventoryOperation>(TInventoryOperation operation, Callback callback) where TInventoryOperation : GInterface438
		{
			method_0();
			if (!(operation is GInterface443 gInterface))
			{
				callback.Succeed();
			}
			else if (Gparam_0._player.InventoryController.IsAnimatedSlot(gInterface.From1))
			{
				callback.Fail($"Detach is not supported in current operation: {GetType()}");
			}
			else
			{
				callback.Succeed();
			}
		}

		public virtual void HideWeapon(Action onHidden, bool fastDrop)
		{
			method_0();
		}

		public virtual void OnFireEnd()
		{
			method_0();
		}

		public virtual void OnFire()
		{
			method_0();
		}

		public virtual void FastForward()
		{
			method_0();
		}

		public virtual void OnIdleStart()
		{
		}
	}

	public enum EKickType : byte
	{
		Slash,
		Stab
	}

	public class KnifeController : BaseKnifeController, IKnifeController, GInterface199, IHandsController, GInterface197
	{
		public class Class1284 : Class1283
		{
			[NonSerialized]
			public Callback Callback_0;

			public Class1284(KnifeController controller)
				: base(controller)
			{
			}

			public virtual void Start(Item item, Callback callback)
			{
				Callback_0 = callback;
				Start();
				Gparam_0.FirearmsAnimator.SetInventory(open: false);
				Gparam_0._player.SendHandsInteractionStateChanged(value: true, 300);
				Player_0.MovementContext.SetInteractInHands(EInteraction.DropBackpack);
			}

			public override void Reset()
			{
				Callback_0 = null;
				base.Reset();
			}

			public override void OnBackpackDrop()
			{
				State = EOperationState.Finished;
				Gparam_0._player.SendHandsInteractionStateChanged(value: false, 300);
				Player_0.MovementContext.SetInteractInHands(EInteraction.DropBackpack);
				WeaponAnimationSpeedControllerClass.ResetTriggerHandReady(Gparam_0.FirearmsAnimator.Animator);
				Gparam_0.firearmsAnimator_0.SetInventory(Gparam_0.bool_1);
				Gparam_0.InitiateOperation<Class1286>().Start();
				Callback_0.Succeed();
			}

			public override void SetInventoryOpened(bool opened)
			{
				Gparam_0.bool_1 = opened;
			}
		}

		public class Class1286 : Class1283
		{
			[NonSerialized]
			public const float Float_0 = 300f;

			[NonSerialized]
			public float Float_1;

			public Class1286(KnifeController controller)
				: base(controller)
			{
			}

			public new void Start()
			{
				base.Start();
				Float_1 = 0f;
			}

			public override void Reset()
			{
				Float_1 = 0f;
				base.Reset();
			}

			public override void HideWeapon(Action onHidden, bool fastDrop)
			{
				State = EOperationState.Finished;
				Gparam_0.InitiateOperation<Class1288>().Start(onHidden, fastDrop);
			}

			public override bool CanRemove()
			{
				return true;
			}

			public override void Execute<TInventoryOperation>(TInventoryOperation operation, Callback callback)
			{
				if (!(operation is GInterface443 gInterface))
				{
					callback.Succeed();
				}
				else if (Player_0.InventoryController.IsAnimatedSlot(gInterface.From1))
				{
					State = EOperationState.Finished;
					Gparam_0.InitiateOperation<Class1284>().Start(gInterface.Item1, callback);
				}
				else
				{
					callback.Succeed();
				}
			}

			public override void Update(float deltaTime)
			{
				Float_1 += deltaTime;
				if (Float_1 > 300f)
				{
					Gparam_0.firearmsAnimator_0.Idle();
					Float_1 = 0f;
				}
			}

			public override void ExamineWeapon()
			{
				Gparam_0.firearmsAnimator_0.LookTrigger();
			}

			public override void SetInventoryOpened(bool opened)
			{
				Gparam_0.firearmsAnimator_0.SetInventory(opened);
			}

			public override bool MakeKnifeKick()
			{
				if (!Player_0.StateIsSuitableForHandInput)
				{
					return false;
				}
				if (Gparam_0._player.Physical.CanMeleeHit)
				{
					State = EOperationState.Finished;
					Gparam_0.InitiateOperation<Class1287>().Start(EKickType.Slash);
					if (Gparam_0._player.UsedSimplifiedSkeleton)
					{
						int value = UnityEngine.Random.Range(0, 3);
						Gparam_0._player.BodyAnimatorCommon.SetInteger(PlayerAnimator.ATTACK_VARIANT, value);
						Gparam_0._player.BodyAnimatorCommon.SetTrigger(PlayerAnimator.IS_ATTACKING);
					}
					return true;
				}
				Gparam_0._player.Physical.InvokeInsufficient();
				return false;
			}

			public override void OnEnd()
			{
				SetKnifeCompassState(active: false);
			}

			public override void SetKnifeCompassState(bool active)
			{
				Gparam_0.CompassState.Value = active;
			}

			public override bool MakeAlternativeKick()
			{
				if (!Player_0.StateIsSuitableForHandInput)
				{
					return false;
				}
				if (Gparam_0._player.Physical.CanMeleeHit)
				{
					State = EOperationState.Finished;
					Gparam_0.InitiateOperation<Class1287>().Start(EKickType.Stab);
					return true;
				}
				Gparam_0._player.Physical.InvokeInsufficient();
				return false;
			}

			public override void StopKnifeKick()
			{
			}

			public override void StopAlternativeKick()
			{
			}
		}

		public class Class1287 : Class1283
		{
			[CompilerGenerated]
			public class Class1256
			{
				public Class1287 class1287_0;

				public Action onHidden;

				public bool fastDrop;

				public void method_0()
				{
					class1287_0.Gparam_0.InitiateOperation<Class1288>().Start(onHidden, fastDrop);
				}
			}

			[NonSerialized]
			public Action Action_0;

			[NonSerialized]
			public bool Bool_0;

			public Class1287(KnifeController controller)
				: base(controller)
			{
			}

			public void Start(EKickType eKickType)
			{
				Start();
				Gparam_0.LastKickType = eKickType;
				if (eKickType == EKickType.Slash)
				{
					Gparam_0.firearmsAnimator_0.SetFire(fire: true);
				}
				else
				{
					Gparam_0.firearmsAnimator_0.SetAlternativeFire(fire: true);
				}
				Gparam_0.SetDeflected(deflected: false);
				Gparam_0.SetMeleeSpeed(Player_0.Physical.MeleeSpeed);
				Bool_0 = false;
			}

			public override void Update(float deltaTime)
			{
				base.Update(deltaTime);
				Gparam_0.knifeCollider_0.ManualUpdate();
				Vector3 position = Gparam_0.knifeCollider_0.transform.position;
				Vector3 normalized = (position - Gparam_0.vector3_1).normalized;
				UnityEngine.Debug.DrawLine(position, Gparam_0.vector3_1, Color.cyan, 10f);
				Gparam_0.vector3_0 = Vector3.Lerp(Gparam_0.vector3_0, normalized, 0.9f);
				Gparam_0.vector3_1 = position;
			}

			public override void Reset()
			{
				Action_0 = null;
				base.Reset();
			}

			public override void OnComboPlanning()
			{
			}

			public override void ContinueCombo()
			{
			}

			public override void BrakeCombo()
			{
				Bool_0 = true;
				Gparam_0.firearmsAnimator_0.SetFire(fire: false);
				Gparam_0.TranslateAnimatorParameter(0);
			}

			public override void OnIdleStart()
			{
				Gparam_0.OnFireEnd();
			}

			public override void StopKnifeKick()
			{
			}

			public override void StopAlternativeKick()
			{
			}

			public override void OnFireEnd()
			{
				Gparam_0.firearmsAnimator_0.SetFire(fire: false);
				Gparam_0.firearmsAnimator_0.SetAlternativeFire(fire: false);
				if (Action_0 != null)
				{
					method_2();
				}
				else
				{
					method_3();
				}
				if (Gparam_0.knifeCollider_0.OnHit != null)
				{
					KnifeCollider knifeCollider_ = Gparam_0.knifeCollider_0;
					knifeCollider_.OnHit = (Action<GStruct182>)Delegate.Remove(knifeCollider_.OnHit, new Action<GStruct182>(Gparam_0.method_6));
				}
				Gparam_0.knifeCollider_0.OnFireEnd();
			}

			public override void OnFire()
			{
				Gparam_0._player.Physical.ConsumeAsMelee((Gparam_0.LastKickType == EKickType.Slash) ? Gparam_0.Knife.Template.PrimaryConsumption : Gparam_0.Knife.Template.SecondaryConsumption);
				if (Gparam_0.knifeCollider_0.OnHit == null)
				{
					KnifeCollider knifeCollider_ = Gparam_0.knifeCollider_0;
					knifeCollider_.OnHit = (Action<GStruct182>)Delegate.Combine(knifeCollider_.OnHit, new Action<GStruct182>(Gparam_0.method_6));
				}
				Gparam_0.knifeCollider_0.MaxDistance = ((Gparam_0.LastKickType == EKickType.Slash) ? Gparam_0.Knife.Template.PrimaryDistance : Gparam_0.Knife.Template.SecondaryDistance);
				Gparam_0.knifeCollider_0.OnFire();
			}

			public override void HideWeapon(Action onHidden, bool fastDrop)
			{
				State = EOperationState.Finished;
				Action_0 = delegate
				{
					Gparam_0.InitiateOperation<Class1288>().Start(onHidden, fastDrop);
				};
			}

			public void method_2()
			{
				State = EOperationState.Finished;
				Action_0();
			}

			public void method_3()
			{
				State = EOperationState.Finished;
				Gparam_0.InitiateOperation<Class1286>().Start();
			}
		}

		public abstract class Class1283 : Class1282<KnifeController>
		{
			[NonSerialized]
			public Player Player_0;

			public Class1283(KnifeController controller)
				: base(controller)
			{
				Player_0 = controller._player;
			}

			public virtual void ExamineWeapon()
			{
				method_0();
			}

			public virtual void SetInventoryOpened(bool opened)
			{
				method_0();
			}

			public virtual bool MakeKnifeKick()
			{
				method_0();
				return false;
			}

			public virtual void OnComboPlanning()
			{
				method_0();
			}

			public virtual void BrakeCombo()
			{
				method_0();
			}

			public virtual void ContinueCombo()
			{
				method_0();
			}

			public virtual void StopKnifeKick()
			{
				method_0();
			}

			public virtual bool CanRemove()
			{
				return false;
			}

			public virtual bool MakeAlternativeKick()
			{
				method_0();
				return false;
			}

			public virtual void StopAlternativeKick()
			{
				method_0();
			}

			public virtual void SetKnifeCompassState(bool active)
			{
				method_0();
			}
		}

		public class Class1288 : Class1283
		{
			[NonSerialized]
			public Action Action_0;

			public Class1288(KnifeController controller)
				: base(controller)
			{
			}

			public void Start(Action onHidden, bool fastDrop)
			{
				Action_0 = onHidden;
				Start();
				Gparam_0.firearmsAnimator_0.SetActiveParam(active: false);
				Gparam_0.firearmsAnimator_0.SetFastHide(fastDrop);
				Gparam_0.SetMeleeSpeed(Player_0.Physical.MeleeSpeed);
				Gparam_0._player.BodyAnimatorCommon.SetFloat(PlayerAnimator.RELOAD_FLOAT_PARAM_HASH, 1f);
			}

			public override void Reset()
			{
				Action_0 = null;
				base.Reset();
			}

			public override void HideWeaponComplete()
			{
				State = EOperationState.Finished;
				Action_0();
			}

			public override void HideWeapon(Action onHidden, bool fastDrop)
			{
				Action_0 = (Action)Delegate.Combine(Action_0, onHidden);
			}

			public override void FastForward()
			{
				if (State != EOperationState.Finished)
				{
					HideWeaponComplete();
				}
			}
		}

		public class Class1285 : Class1284
		{
			[NonSerialized]
			public const float Float_0 = 0.25f;

			[NonSerialized]
			public float Float_1;

			[NonSerialized]
			public bool Bool_0;

			public Class1285(KnifeController controller)
				: base(controller)
			{
			}

			public override void Start(Item item, Callback callback)
			{
				Float_1 = 0f;
				Bool_0 = false;
				base.Start(item, callback);
			}

			public override void FastForward()
			{
				if (!Bool_0)
				{
					Bool_0 = true;
					OnBackpackDrop();
				}
			}

			public override void Update(float deltaTime)
			{
				base.Update(deltaTime);
				if (!Bool_0 && Float_1 > 0.25f)
				{
					Bool_0 = true;
					OnBackpackDrop();
				}
				Float_1 += deltaTime;
			}
		}

		public class Class1289 : Class1283
		{
			[NonSerialized]
			public Action Action_0;

			[NonSerialized]
			public Action Action_1;

			[NonSerialized]
			public bool Bool_0;

			public Class1289(KnifeController controller)
				: base(controller)
			{
			}

			public void Start(Action callback)
			{
				Action_1 = callback;
				Gparam_0._player.BodyAnimatorCommon.SetFloat(PlayerAnimator.WEAPON_SIZE_MODIFIER_PARAM_HASH, 1f);
				Gparam_0._player.BodyAnimatorCommon.SetFloat(PlayerAnimator.RELOAD_FLOAT_PARAM_HASH, 1f);
				Start();
				Gparam_0.firearmsAnimator_0.SetActiveParam(active: true);
				Gparam_0.firearmsAnimator_0.SetMeleeSpeed(Player_0.Physical.MeleeSpeed);
				Gparam_0.firearmsAnimator_0.SetInventory(Player_0._isInventoryOpened);
				if (Player_0.UsedSimplifiedSkeleton)
				{
					FastForward();
				}
			}

			public override void Reset()
			{
				base.Reset();
				Action_1 = null;
				Action_0 = null;
			}

			public override void WeaponAppeared()
			{
				Gparam_0.SetupProp();
				State = EOperationState.Finished;
				Gparam_0._player.BodyAnimatorCommon.SetFloat(PlayerAnimator.RELOAD_FLOAT_PARAM_HASH, 0f);
				Class1286 @class = Gparam_0.InitiateOperation<Class1286>();
				@class.Start();
				Action_1();
				if (Action_0 != null)
				{
					@class.HideWeapon(Action_0, Bool_0);
				}
			}

			public override void HideWeapon(Action onHidden, bool fastDrop)
			{
				Action_0 = onHidden;
				Bool_0 = fastDrop;
			}

			public override void FastForward()
			{
				if (State != EOperationState.Finished)
				{
					WeaponAppeared();
				}
			}

			public override void SetLeftStanceAnimOnStartOperation()
			{
				Player_0.MovementContext.LeftStanceController.DisableLeftStanceAnimFromHandsAction();
			}
		}

		protected bool bool_1;

		[CompilerGenerated]
		private Action action_1;

		[CompilerGenerated]
		private Action action_2;

		public Action ComboPlanning
		{
			[CompilerGenerated]
			get
			{
				return action_1;
			}
			[CompilerGenerated]
			set
			{
				action_1 = value;
			}
		}

		public Action OnAttackEnd
		{
			[CompilerGenerated]
			get
			{
				return action_2;
			}
			[CompilerGenerated]
			set
			{
				action_2 = value;
			}
		}

		public new KnifeComponent Knife => base.Knife;

		public Class1283 Class1283_0 => base.CurrentHandsOperation as Class1283;

		public static T smethod_9<T>(Player player, KnifeComponent knife) where T : KnifeController
		{
			return BaseKnifeController.smethod_6<T>(player, knife);
		}

		public static Task<T> smethod_10<T>(Player player, KnifeComponent knife) where T : KnifeController
		{
			return BaseKnifeController.smethod_7<T>(player, knife);
		}

		public virtual void ExamineWeapon()
		{
			Class1283_0.ExamineWeapon();
		}

		public virtual bool MakeKnifeKick()
		{
			return Class1283_0.MakeKnifeKick();
		}

		public override void OnComboPlanning()
		{
			if (knifeCollider_0.OnHit != null)
			{
				KnifeCollider knifeCollider = knifeCollider_0;
				knifeCollider.OnHit = (Action<GStruct182>)Delegate.Remove(knifeCollider.OnHit, new Action<GStruct182>(base.method_6));
			}
			Class1283_0.OnComboPlanning();
			ComboPlanning?.Invoke();
		}

		public override void OnFireEnd()
		{
			base.Interface10_0.OnFireEnd();
			OnAttackEnd?.Invoke();
		}

		public virtual bool MakeAlternativeKick()
		{
			return Class1283_0.MakeAlternativeKick();
		}

		public virtual void BrakeCombo()
		{
			Class1283_0.BrakeCombo();
		}

		public virtual void ContinueCombo()
		{
			Class1283_0.ContinueCombo();
		}

		public override void SetCompassState(bool active)
		{
			if (CanChangeCompassState(active))
			{
				Class1283_0.SetKnifeCompassState(active);
			}
		}

		public override bool CanRemove()
		{
			return Class1283_0.CanRemove();
		}

		public override void Spawn(float animationSpeed, Action callback)
		{
			firearmsAnimator_0.SetAnimationSpeed(animationSpeed);
			InitiateOperation<Class1289>().Start(callback);
			firearmsAnimator_0.SkipTime(Time.fixedDeltaTime);
		}

		public override void SetInventoryOpened(bool opened)
		{
			if (opened)
			{
				SetCompassState(active: false);
			}
			Class1283_0.SetInventoryOpened(opened);
			_player.CurrentManagedState?.OnInventory(opened);
		}

		public override bool IsInventoryOpen()
		{
			return _objectInHandsAnimator.IsInInventory;
		}

		public override bool CanExecute(GInterface438 operation)
		{
			if (!(operation is GInterface443 gInterface))
			{
				return true;
			}
			if (_player.InventoryController.IsAnimatedSlot(gInterface.From1))
			{
				return Class1283_0 is Class1286;
			}
			return true;
		}

		public void SetBotParameters()
		{
			knifeCollider_0.SetBotParameters(Knife.Template.ColliderScaleMultiplier);
		}

		public override Dictionary<Type, OperationFactoryDelegate> GetOperationFactoryDelegates()
		{
			return new Dictionary<Type, OperationFactoryDelegate>
			{
				{
					typeof(Class1289),
					() => new Class1289(this)
				},
				{
					typeof(Class1286),
					() => new Class1286(this)
				},
				{
					typeof(Class1288),
					() => new Class1288(this)
				},
				{
					typeof(Class1287),
					() => new Class1287(this)
				},
				{
					typeof(Class1284),
					() => new Class1284(this)
				}
			};
		}

		[CompilerGenerated]
		public BaseAnimationOperationClass method_9()
		{
			return new Class1289(this);
		}

		[CompilerGenerated]
		public BaseAnimationOperationClass method_10()
		{
			return new Class1286(this);
		}

		[CompilerGenerated]
		public BaseAnimationOperationClass method_11()
		{
			return new Class1288(this);
		}

		[CompilerGenerated]
		public BaseAnimationOperationClass method_12()
		{
			return new Class1287(this);
		}

		[CompilerGenerated]
		public BaseAnimationOperationClass method_13()
		{
			return new Class1284(this);
		}
	}

	public struct GStruct182(RaycastHit hit)
	{
		public Collider collider = hit.collider;

		public Vector3 point = hit.point;

		public Vector3 normal = hit.normal;
	}

	public class QuickKnifeKickController : BaseKnifeController, GInterface207, GInterface205<KnifeItemClass>, GInterface204, IHandsController
	{
		public class Class1290 : Class1282<QuickKnifeKickController>
		{
			[NonSerialized]
			public Action Action_0;

			[NonSerialized]
			public Action Action_1;

			[NonSerialized]
			public Callback<GInterface205<KnifeItemClass>> Callback_0;

			[NonSerialized]
			public bool Bool_0;

			public Class1290(QuickKnifeKickController controller)
				: base(controller)
			{
			}

			public void Start(Action callback)
			{
				Action_0 = callback;
				Gparam_0.firearmsAnimator_0.SetQuickFire(quickFire: true);
				Gparam_0.firearmsAnimator_0.SetActiveParam(active: false);
				Gparam_0.firearmsAnimator_0.SetMeleeSpeed(Gparam_0._player.Physical.MeleeSpeed);
				Gparam_0.SetDeflected(deflected: false);
				Gparam_0.SetMeleeSpeed(Gparam_0._player.Physical.MeleeSpeed);
			}

			public override void WeaponAppeared()
			{
				Action_0();
			}

			public override void HideWeapon(Action onHidden, bool fastHide)
			{
				onHidden();
				if (!GClass842.DisabledForNow)
				{
					if (Bool_0)
					{
						onHidden();
					}
					else
					{
						Action_1 = onHidden;
					}
				}
			}

			public void SetOnUsedCallback(Callback<GInterface205<KnifeItemClass>> callback)
			{
				Callback_0 = callback;
			}

			public override void OnFireEnd()
			{
				Bool_0 = true;
				Gparam_0.firearmsAnimator_0.SetQuickFire(quickFire: false);
				if (Gparam_0.Destroyed)
				{
					Action_1();
				}
				else if (Callback_0 != null)
				{
					Callback_0(Gparam_0);
				}
				if (Gparam_0.knifeCollider_0.OnHit != null)
				{
					KnifeCollider knifeCollider_ = Gparam_0.knifeCollider_0;
					knifeCollider_.OnHit = (Action<GStruct182>)Delegate.Remove(knifeCollider_.OnHit, new Action<GStruct182>(Gparam_0.method_6));
				}
				Gparam_0.knifeCollider_0.OnFireEnd();
			}

			public override void OnFire()
			{
				Gparam_0._player.Physical.ConsumeAsMelee(Gparam_0.Knife.Template.SecondaryConsumption);
				KnifeCollider knifeCollider_ = Gparam_0.knifeCollider_0;
				knifeCollider_.OnHit = (Action<GStruct182>)Delegate.Combine(knifeCollider_.OnHit, new Action<GStruct182>(Gparam_0.method_6));
				Gparam_0.knifeCollider_0.MaxDistance = Gparam_0.Knife.Template.PrimaryDistance;
				Gparam_0.knifeCollider_0.OnFire();
			}

			public override void Update(float deltaTime)
			{
				Gparam_0.knifeCollider_0.ManualUpdate();
				base.Update(deltaTime);
			}

			public override void SetLeftStanceAnimOnStartOperation()
			{
				Gparam_0._player.MovementContext.LeftStanceController.DisableLeftStanceAnimFromHandsAction();
			}
		}

		public new KnifeItemClass Item => (KnifeItemClass)base.Knife.Item;

		public Class1290 Class1290_0 => base.CurrentHandsOperation as Class1290;

		public static T smethod_9<T>(Player player, KnifeComponent knife) where T : QuickKnifeKickController
		{
			return BaseKnifeController.smethod_6<T>(player, knife);
		}

		public static Task<T> smethod_10<T>(Player player, KnifeComponent knife) where T : QuickKnifeKickController
		{
			return BaseKnifeController.smethod_7<T>(player, knife);
		}

		public void SetOnUsedCallback(Callback<GInterface205<KnifeItemClass>> callback)
		{
			Class1290_0.SetOnUsedCallback(callback);
		}

		public override void Spawn(float animationSpeed, Action callback)
		{
			firearmsAnimator_0.SetAnimationSpeed(animationSpeed);
			InitiateOperation<Class1290>().Start(callback);
			firearmsAnimator_0.SkipTime(Time.fixedDeltaTime);
		}

		public override bool CanExecute(GInterface438 operation)
		{
			return false;
		}

		public override bool CanRemove()
		{
			return true;
		}

		public override Dictionary<Type, OperationFactoryDelegate> GetOperationFactoryDelegates()
		{
			return new Dictionary<Type, OperationFactoryDelegate> { 
			{
				typeof(Class1290),
				() => new Class1290(this)
			} };
		}

		[CompilerGenerated]
		public BaseAnimationOperationClass method_9()
		{
			return new Class1290(this);
		}
	}

	public class MedsController : ItemHandsController, GInterface203, IOnHandsUseCallback, IHandsController
	{
		public class ObservedMedsControllerClass : BaseAnimationOperationClass
		{
			[NonSerialized]
			public MedsController MedsController_0;

			[NonSerialized]
			public Action Action_0;

			[NonSerialized]
			public Callback<IOnHandsUseCallback> Callback_0;

			[NonSerialized]
			public Queue<EBodyPart> Queue_0;

			[NonSerialized]
			public float Float_0;

			[NonSerialized]
			public int Int_0;

			[NonSerialized]
			public bool Bool_0;

			public ObservedMedsControllerClass(MedsController controller)
				: base(controller)
			{
				MedsController_0 = controller;
			}

			public override void Reset()
			{
				Action_0 = null;
				Callback_0 = null;
				base.Reset();
			}

			public void Start(GStruct382<EBodyPart> bodyParts, float amount, Action callback)
			{
				Start();
				callback();
				Queue_0 = new Queue<EBodyPart>();
				for (int i = 0; i < bodyParts.Length; i++)
				{
					Queue_0.Enqueue(bodyParts[i]);
				}
				if (MedsController_0.Item is MedsItemClass)
				{
					amount = 1f;
				}
				FoodDrinkComponent itemComponent = MedsController_0.Item.GetItemComponent<FoodDrinkComponent>();
				if (itemComponent != null)
				{
					amount = Mathf.Clamp(amount, 0f, itemComponent.HpPercent / itemComponent.MaxResource);
				}
				Float_0 = amount;
				if (MedsController_0.Item.TryGetItemComponent<AnimationVariantsComponent>(out var component))
				{
					Int_0 = UnityEngine.Random.Range(0, component.VariantsNumber);
				}
				method_5();
				MedsController_0._player.HealthController.EffectRemovedEvent += method_8;
				MedsController_0.firearmsAnimator_0?.SetActiveParam(active: true, resetLeftHand: false);
				MedsController_0.OnOutUseEvent += method_3;
				MedsController_0.Item.Owner.RemoveItemEvent += method_2;
			}

			public void method_2(GEventArgs3 args)
			{
				if (args.Item == MedsController_0.Item)
				{
					method_9();
				}
			}

			public void method_3()
			{
				MedsController_0.firearmsAnimator_0?.SetActiveParam(active: true, resetLeftHand: false);
				MedsController_0.firearmsAnimator_0?.SetNextLimb(value: false);
			}

			public bool method_4()
			{
				if (Queue_0.Count == 0)
				{
					return false;
				}
				if (MedsController_0.Item == null)
				{
					return false;
				}
				if (!MedsController_0.Item.TryGetItemComponent<MedKitComponent>(out var component))
				{
					return false;
				}
				if (component.HpResource < Mathf.Epsilon)
				{
					return false;
				}
				return true;
			}

			public void method_5()
			{
				EBodyPart bodyPart = EBodyPart.Common;
				if (Queue_0.TryPeek(out var result))
				{
					bodyPart = result;
				}
				if (MedsController_0._player.ActiveHealthController == null)
				{
					return;
				}
				if (MedsController_0._player.ActiveHealthController.DoMedEffect(MedsController_0.Item, bodyPart, Float_0) == null)
				{
					State = EOperationState.Finished;
					MedsController_0.FailedToApply = true;
					Callback<IOnHandsUseCallback> callback_ = Callback_0;
					Callback_0 = null;
					callback_?.Invoke(MedsController_0);
					return;
				}
				float num = MedsController_0._player.Skills.SurgerySpeed.Value / 100f;
				ValueStruct bodyPartHealth = MedsController_0._player.HealthController.GetBodyPartHealth(result);
				if (bodyPartHealth.Maximum - bodyPartHealth.Current < 10f)
				{
					num += 0.2f;
				}
				MedsController_0.firearmsAnimator_0?.SetUseTimeMultiplier(1f + num);
				method_6();
			}

			public void method_6()
			{
				Item item = MedsController_0.Item;
				Int_0++;
				int num = 0;
				if (item.TryGetItemComponent<AnimationVariantsComponent>(out var component))
				{
					num = component.VariantsNumber;
				}
				int animationVariant = (int)Mathf.Repeat(Int_0, num);
				MedsController_0._animationVariant = animationVariant;
				MedsController_0.firearmsAnimator_0?.SetAnimationVariant(animationVariant);
			}

			public void SetOnUsedCallback(Callback<IOnHandsUseCallback> callback)
			{
				Callback_0 = callback;
			}

			public Callback<IOnHandsUseCallback> GetOnUsedCallback()
			{
				return Callback_0;
			}

			public void Remove()
			{
				Queue_0.Clear();
				MedsController_0._player.HealthController.CancelApplyingItem();
			}

			public bool method_7()
			{
				if (!Queue_0.TryPeek(out var result))
				{
					return false;
				}
				if (result == EBodyPart.Common)
				{
					return true;
				}
				return !MedsController_0._player.HealthController.CanApplyItem(MedsController_0.Item, result);
			}

			public void method_8(IEffect effect)
			{
				if (effect is GInterface376)
				{
					while (method_7())
					{
						Queue_0.Dequeue();
					}
					bool flag = method_4();
					if (MedsController_0.firearmsAnimator_0 != null && MedsController_0.firearmsAnimator_0.HasNextLimb())
					{
						MedsController_0.firearmsAnimator_0.SetActiveParam(active: false, resetLeftHand: false);
						MedsController_0.firearmsAnimator_0.SetNextLimb(flag);
					}
					if (flag)
					{
						method_5();
					}
					else
					{
						method_9();
					}
				}
			}

			public void method_9()
			{
				MedsController_0.firearmsAnimator_0?.SetNextLimb(value: false);
				MedsController_0.firearmsAnimator_0?.SetActiveParam(active: false);
				if (!Bool_0)
				{
					Bool_0 = true;
					MedsController_0._player.HealthController.EffectRemovedEvent -= method_8;
					MedsController_0.OnOutUseEvent -= method_3;
					if (MedsController_0.Item.Owner != null)
					{
						MedsController_0.Item.Owner.RemoveItemEvent -= method_2;
					}
					if (!(MedsController_0 == null) && !MedsController_0.Equals(null))
					{
						Callback<IOnHandsUseCallback> callback_ = Callback_0;
						Callback_0 = null;
						callback_?.Invoke(MedsController_0);
					}
				}
			}

			public void HideWeapon(Action onHiddenCallback)
			{
				MedsController_0._player.ActiveHealthController?.RemoveMedEffect();
				MedsController_0._player.HealthController.EffectRemovedEvent -= method_8;
				Action_0 = onHiddenCallback;
				MedsController_0.firearmsAnimator_0?.SetActiveParam(active: false, resetLeftHand: false);
				if (State == EOperationState.Finished)
				{
					Action_0?.Invoke();
					Callback_0?.Invoke(MedsController_0);
					Callback_0 = null;
				}
			}

			public void HideWeaponComplete()
			{
				State = EOperationState.Finished;
				MedsController_0._player.method_138(MedsController_0.Item).Confirm();
				Action_0?.Invoke();
			}

			public void ClearQueue()
			{
				if (Queue_0.Count > 0)
				{
					EBodyPart item = Queue_0.Dequeue();
					Queue_0.Clear();
					Queue_0.Enqueue(item);
				}
			}

			public void FastForward()
			{
				if (State != EOperationState.Finished)
				{
					HideWeaponComplete();
				}
			}

			public override void SetLeftStanceAnimOnStartOperation()
			{
				MedsController_0._player.MovementContext.LeftStanceController.DisableLeftStanceAnimFromHandsAction();
			}
		}

		[CompilerGenerated]
		public class Class1257<T> where T : MedsController
		{
			public T controller;

			public void method_0()
			{
				controller.firearmsAnimator_0.RemoveEventsConsumer(controller);
			}

			public void method_1()
			{
				smethod_9(controller);
			}

			public void method_2()
			{
				controller.OnOutUseEvent -= delegate
				{
					smethod_9(controller);
				};
			}
		}

		private float float_0;

		public int _animationVariant;

		private GStruct382<EBodyPart> gstruct382_0;

		private FirearmsAnimator firearmsAnimator_0;

		[CompilerGenerated]
		private Action action_0;

		[CompilerGenerated]
		private bool bool_0;

		public override int AnimationVariant => _animationVariant;

		public ObservedMedsControllerClass ObservedMedsControllerClass => base.CurrentHandsOperation as ObservedMedsControllerClass;

		public override FirearmsAnimator FirearmsAnimator => firearmsAnimator_0;

		public override string LoggerDistinctId => $"{_player.ProfileId}|{_player.Profile.Info.Nickname}|{this}";

		public bool FailedToApply
		{
			[CompilerGenerated]
			get
			{
				return bool_0;
			}
			[CompilerGenerated]
			set
			{
				bool_0 = value;
			}
		}

		public event Action OnOutUseEvent
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

		public override void ShowGesture(EInteraction gesture)
		{
		}

		public override void Destroy()
		{
			_player.ProceduralWeaponAnimation.ClearPreviousWeapon();
			base.Destroy();
			firearmsAnimator_0 = null;
			AssetPoolObject.ReturnToPool(_controllerObject.gameObject);
		}

		public override void OnOutUse()
		{
			base.OnOutUse();
			action_0?.Invoke();
		}

		public void SetOnUsedCallback(Callback<IOnHandsUseCallback> callback)
		{
			this.ObservedMedsControllerClass.SetOnUsedCallback(callback);
		}

		public Callback<IOnHandsUseCallback> GetOnUsedCallback()
		{
			return this.ObservedMedsControllerClass.GetOnUsedCallback();
		}

		public void Remove()
		{
			method_1().HandleExceptions();
		}

		public async Task method_1()
		{
			await Task.Delay(600);
			this.ObservedMedsControllerClass.Remove();
		}

		public void ClearQueue()
		{
			this.ObservedMedsControllerClass.ClearQueue();
		}

		public override bool CanExecute(GInterface438 operation)
		{
			return true;
		}

		public override void Execute(GInterface438 operation, Callback callback)
		{
			callback.Succeed();
		}

		public override void Pickup(bool p)
		{
		}

		public override void Loot(bool p)
		{
		}

		public override void Interact(bool isInteracting, int actionIndex)
		{
		}

		public override bool CanInteract()
		{
			return false;
		}

		public override bool CanRemove()
		{
			return true;
		}

		public static T smethod_6<T>(Player player, Item item, GStruct382<EBodyPart> bodyParts, float amount, int animationVariant) where T : MedsController
		{
			T val = ItemHandsController.smethod_1<T>(player, item, Singleton<PoolManagerClass>.Instance.CreateItemUsablePrefab);
			smethod_8(val, player, item, animationVariant);
			val.gstruct382_0 = bodyParts;
			val.float_0 = amount;
			return val;
		}

		public static async Task<T> smethod_7<T>(Player player, Item item, GStruct382<EBodyPart> bodyParts, float amount, int animationVariant) where T : MedsController
		{
			T obj = await ItemHandsController.smethod_3<T>(player, item, Singleton<PoolManagerClass>.Instance.CreateItemUsablePrefabAsync);
			smethod_8(obj, player, item, animationVariant);
			obj.gstruct382_0 = bodyParts;
			obj.float_0 = amount;
			return obj;
		}

		public static void smethod_8<T>(T controller, Player player, Item item, int animationVariant) where T : MedsController
		{
			WeaponPrefab component = controller._controllerObject.GetComponent<WeaponPrefab>();
			GClass2086 objectInHands = component.ObjectInHands;
			player.ProceduralWeaponAnimation.ClearPreviousWeapon();
			player.ProceduralWeaponAnimation.InitTransforms(controller.HandsHierarchy);
			objectInHands.AfterGetFromPoolInit(player.ProceduralWeaponAnimation, null, player.IsYourPlayer);
			controller.firearmsAnimator_0 = component.FirearmsAnimator;
			controller.firearmsAnimator_0.AddEventsConsumer(controller);
			controller.CompositeDisposable.AddDisposable(delegate
			{
				controller.firearmsAnimator_0.RemoveEventsConsumer(controller);
			});
			controller.firearmsAnimator_0.SkipTime(0.0001f);
			controller._player.HandsAnimator = controller.firearmsAnimator_0;
			controller._controllerObject.GetComponent<BaseSoundPlayer>().Init(controller, player.PlayerBones.WeaponRoot, player);
			controller.firearmsAnimator_0.SetUseTimeMultiplier(1f + (float)player.Skills.SurgerySpeed);
			controller.OnOutUseEvent += delegate
			{
				smethod_9(controller);
			};
			controller.CompositeDisposable.AddDisposable(delegate
			{
				controller.OnOutUseEvent -= delegate
				{
					smethod_9(controller);
				};
			});
		}

		public static void smethod_9(MedsController controller)
		{
			if (!(controller == null))
			{
				controller.firearmsAnimator_0.SetActiveParam(active: true, resetLeftHand: false);
				controller.firearmsAnimator_0.SetNextLimb(value: false);
				smethod_10(controller);
			}
		}

		public static void smethod_10(MedsController controller)
		{
			Item item = controller.Item;
			controller._animationVariant++;
			int num = 0;
			if (item.TryGetItemComponent<AnimationVariantsComponent>(out var component))
			{
				num = component.VariantsNumber;
			}
			int animationVariant = (controller._animationVariant = (int)Mathf.Repeat(controller._animationVariant, num));
			controller.firearmsAnimator_0.SetAnimationVariant(animationVariant);
		}

		public override void ManualUpdate(float deltaTime)
		{
			base.ManualUpdate(deltaTime);
			firearmsAnimator_0?.SetAimAngle(_player.Pitch);
		}

		public override void IEventsConsumerOnWeapOut()
		{
			this.ObservedMedsControllerClass.HideWeaponComplete();
		}

		public override void IEventsConsumerOnThirdAction(int IntParam)
		{
			TranslateAnimatorParameter(IntParam);
		}

		public override void Spawn(float animationSpeed, Action callback)
		{
			firearmsAnimator_0.SetAnimationSpeed(animationSpeed);
			firearmsAnimator_0.SetPointOfViewOnSpawn(_player.PointOfView);
			InitiateOperation<ObservedMedsControllerClass>().Start(gstruct382_0, float_0, callback);
		}

		public override void Drop(float animationSpeed, Action callback, bool fastDrop = false, Item nextControllerItem = null)
		{
			method_2(callback).HandleExceptions();
		}

		public async Task method_2(Action callback)
		{
			await Task.Delay(600);
			base.Destroyed = true;
			this.ObservedMedsControllerClass.HideWeapon(callback);
		}

		public override void FastForwardCurrentState()
		{
			this.ObservedMedsControllerClass.FastForward();
		}

		public override Dictionary<Type, OperationFactoryDelegate> GetOperationFactoryDelegates()
		{
			return new Dictionary<Type, OperationFactoryDelegate> { 
			{
				typeof(ObservedMedsControllerClass),
				() => new ObservedMedsControllerClass(this)
			} };
		}

		[CompilerGenerated]
		public BaseAnimationOperationClass method_3()
		{
			return new ObservedMedsControllerClass(this);
		}
	}

	public abstract class AbstractHandsController : MonoBehaviour, IHandsController, IActorEvents
	{
		protected readonly CompositeDisposableClass CompositeDisposable = new CompositeDisposableClass();

		public Transform WeaponRoot { get; set; }

		public abstract GameObject ControllerGameObject { get; }

		public bool Destroyed { get; set; }

		public abstract TransformLinks HandsHierarchy { get; }

		public abstract FirearmsAnimator FirearmsAnimator { get; }

		public AnimationEventsEmitter AnimationEventsEmitter { get; set; }

		public virtual bool IsAiming
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		public virtual float AimingSensitivity => 1f;

		public virtual float AimingSmoothSensitivity { get; set; } = 1f;

		public virtual string LoggerDistinctId => "undefined";

		public Item Item => GetItem();

		public virtual int AnimationVariant => 0;

		public event Action<bool> OnAimingChanged;

		public virtual void ManualLateUpdate(float deltaTime)
		{
		}

		public virtual void Destroy()
		{
			CompositeDisposable.Dispose();
		}

		public abstract bool CanExecute(GInterface438 operation);

		public abstract void Execute(GInterface438 operation, Callback callback);

		public abstract bool CanRemove();

		public virtual bool IsHandsProcessing()
		{
			return false;
		}

		public virtual bool IsPlacingBeacon()
		{
			return false;
		}

		public virtual bool CanInteract()
		{
			return true;
		}

		public virtual bool InCanNotBeInterruptedOperation()
		{
			return false;
		}

		public abstract void ShowGesture(EInteraction gesture);

		public abstract void BlindFire(int b);

		public virtual bool IsInInteraction()
		{
			return false;
		}

		public virtual bool IsInInteractionStrictCheck()
		{
			return IsInInteraction();
		}

		public virtual float GetAnimatorFloatParam(int hash)
		{
			return 0f;
		}

		public virtual bool SupportPickup()
		{
			return false;
		}

		public virtual void Pickup(bool p)
		{
			throw new NotImplementedException();
		}

		public virtual void Interact(bool isInteracting, int actionIndex)
		{
			throw new NotImplementedException();
		}

		public virtual void Loot(bool p)
		{
			throw new NotImplementedException();
		}

		public virtual void SetInventoryOpened(bool opened)
		{
		}

		public virtual bool IsInventoryOpen()
		{
			return false;
		}

		public virtual void OnPlayerDead()
		{
		}

		public virtual void OnGameSessionEnd()
		{
		}

		public abstract Item GetItem();

		public abstract void ManualUpdate(float deltaTime);

		public abstract void BallisticUpdate(float deltaTime);

		public abstract void EmitEvents();

		public abstract void Spawn(float animationSpeed, Action callback);

		public abstract void Drop(float animationSpeed, Action callback, bool fastDrop, Item nextControllerItem = null);

		public virtual void FastForwardCurrentState()
		{
		}

		public virtual void AimingChanged(bool newValue)
		{
			this.OnAimingChanged?.Invoke(newValue);
		}

		public void OnAddAmmoInChamber()
		{
			IEventsConsumerOnAddAmmoInChamber();
		}

		void IActorEvents.OnAddAmmoInChamber()
		{
			//ILSpy generated this explicit interface implementation from .override directive in OnAddAmmoInChamber
			this.OnAddAmmoInChamber();
		}

		public virtual void IEventsConsumerOnAddAmmoInChamber()
		{
		}

		public void OnAddAmmoInMag()
		{
			IEventsConsumerOnAddAmmoInMag();
		}

		void IActorEvents.OnAddAmmoInMag()
		{
			//ILSpy generated this explicit interface implementation from .override directive in OnAddAmmoInMag
			this.OnAddAmmoInMag();
		}

		public virtual void IEventsConsumerOnAddAmmoInMag()
		{
		}

		public void OnArm()
		{
			IEventsConsumerOnArm();
		}

		void IActorEvents.OnArm()
		{
			//ILSpy generated this explicit interface implementation from .override directive in OnArm
			this.OnArm();
		}

		public virtual void IEventsConsumerOnArm()
		{
		}

		public void OnCook()
		{
			IEventsConsumerOnCook();
		}

		void IActorEvents.OnCook()
		{
			//ILSpy generated this explicit interface implementation from .override directive in OnCook
			this.OnCook();
		}

		public virtual void IEventsConsumerOnCook()
		{
		}

		public void OnDelAmmoChamber()
		{
			IEventsConsumerOnDelAmmoChamber();
		}

		void IActorEvents.OnDelAmmoChamber()
		{
			//ILSpy generated this explicit interface implementation from .override directive in OnDelAmmoChamber
			this.OnDelAmmoChamber();
		}

		public virtual void IEventsConsumerOnDelAmmoChamber()
		{
		}

		public void OnDelAmmoFromMag()
		{
			IEventsConsumerOnDelAmmoFromMag();
		}

		void IActorEvents.OnDelAmmoFromMag()
		{
			//ILSpy generated this explicit interface implementation from .override directive in OnDelAmmoFromMag
			this.OnDelAmmoFromMag();
		}

		public virtual void IEventsConsumerOnDelAmmoFromMag()
		{
		}

		public void OnDisarm()
		{
			IEventsConsumerOnDisarm();
		}

		void IActorEvents.OnDisarm()
		{
			//ILSpy generated this explicit interface implementation from .override directive in OnDisarm
			this.OnDisarm();
		}

		public virtual void IEventsConsumerOnDisarm()
		{
		}

		public void OnFireEnd()
		{
			IEventsConsumerOnFireEnd();
		}

		void IActorEvents.OnFireEnd()
		{
			//ILSpy generated this explicit interface implementation from .override directive in OnFireEnd
			this.OnFireEnd();
		}

		public virtual void IEventsConsumerOnFireEnd()
		{
		}

		public void OnComboPlanning()
		{
			IEventsConsumerOnComboPlanning();
		}

		void IActorEvents.OnComboPlanning()
		{
			//ILSpy generated this explicit interface implementation from .override directive in OnComboPlanning
			this.OnComboPlanning();
		}

		public virtual void IEventsConsumerOnComboPlanning()
		{
		}

		public void OnFiringBullet()
		{
			IEventsConsumerOnFiringBullet();
		}

		void IActorEvents.OnFiringBullet()
		{
			//ILSpy generated this explicit interface implementation from .override directive in OnFiringBullet
			this.OnFiringBullet();
		}

		public virtual void IEventsConsumerOnFiringBullet()
		{
		}

		public void OnFoldOff()
		{
			IEventsConsumerOnFoldOff();
		}

		void IActorEvents.OnFoldOff()
		{
			//ILSpy generated this explicit interface implementation from .override directive in OnFoldOff
			this.OnFoldOff();
		}

		public virtual void IEventsConsumerOnFoldOff()
		{
		}

		public void OnFoldOn()
		{
			IEventsConsumerOnFoldOn();
		}

		void IActorEvents.OnFoldOn()
		{
			//ILSpy generated this explicit interface implementation from .override directive in OnFoldOn
			this.OnFoldOn();
		}

		public virtual void IEventsConsumerOnFoldOn()
		{
		}

		public void OnIdleStart()
		{
			IEventsConsumerOnIdleStart();
		}

		void IActorEvents.OnIdleStart()
		{
			//ILSpy generated this explicit interface implementation from .override directive in OnIdleStart
			this.OnIdleStart();
		}

		public virtual void IEventsConsumerOnIdleStart()
		{
		}

		public void OnLauncherAppeared()
		{
			IEventsConsumerOnLauncherAppeared();
		}

		void IActorEvents.OnLauncherAppeared()
		{
			//ILSpy generated this explicit interface implementation from .override directive in OnLauncherAppeared
			this.OnLauncherAppeared();
		}

		public virtual void IEventsConsumerOnLauncherAppeared()
		{
		}

		public void OnLauncherDisappeared()
		{
			IEventsConsumerOnLauncherDisappeared();
		}

		void IActorEvents.OnLauncherDisappeared()
		{
			//ILSpy generated this explicit interface implementation from .override directive in OnLauncherDisappeared
			this.OnLauncherDisappeared();
		}

		public virtual void IEventsConsumerOnLauncherDisappeared()
		{
		}

		public void OnMagHide()
		{
			IEventsConsumerOnMagHide();
		}

		void IActorEvents.OnMagHide()
		{
			//ILSpy generated this explicit interface implementation from .override directive in OnMagHide
			this.OnMagHide();
		}

		public virtual void IEventsConsumerOnMagHide()
		{
		}

		public void OnMagIn()
		{
			IEventsConsumerOnMagIn();
		}

		void IActorEvents.OnMagIn()
		{
			//ILSpy generated this explicit interface implementation from .override directive in OnMagIn
			this.OnMagIn();
		}

		public virtual void IEventsConsumerOnMagIn()
		{
		}

		public void OnMagOut()
		{
			IEventsConsumerOnMagOut();
		}

		void IActorEvents.OnMagOut()
		{
			//ILSpy generated this explicit interface implementation from .override directive in OnMagOut
			this.OnMagOut();
		}

		public virtual void IEventsConsumerOnMagOut()
		{
		}

		public void OnMagShow()
		{
			IEventsConsumerOnMagShow();
		}

		void IActorEvents.OnMagShow()
		{
			//ILSpy generated this explicit interface implementation from .override directive in OnMagShow
			this.OnMagShow();
		}

		public virtual void IEventsConsumerOnMagShow()
		{
		}

		public void OnMessageName()
		{
			IEventsConsumerOnMessageName();
		}

		void IActorEvents.OnMessageName()
		{
			//ILSpy generated this explicit interface implementation from .override directive in OnMessageName
			this.OnMessageName();
		}

		public virtual void IEventsConsumerOnMessageName()
		{
		}

		public void OnMalfunctionOff()
		{
			IEventsConsumerOnMalfunctionOff();
		}

		void IActorEvents.OnMalfunctionOff()
		{
			//ILSpy generated this explicit interface implementation from .override directive in OnMalfunctionOff
			this.OnMalfunctionOff();
		}

		public virtual void IEventsConsumerOnMalfunctionOff()
		{
		}

		public void OnModChanged()
		{
			IEventsConsumerOnModChanged();
		}

		void IActorEvents.OnModChanged()
		{
			//ILSpy generated this explicit interface implementation from .override directive in OnModChanged
			this.OnModChanged();
		}

		public virtual void IEventsConsumerOnModChanged()
		{
		}

		public void OutUse()
		{
			OnOutUse();
		}

		void IActorEvents.OutUse()
		{
			//ILSpy generated this explicit interface implementation from .override directive in OutUse
			this.OutUse();
		}

		public virtual void OnOutUse()
		{
		}

		public void OnOffBoltCatch()
		{
			IEventsConsumerOnOffBoltCatch();
		}

		void IActorEvents.OnOffBoltCatch()
		{
			//ILSpy generated this explicit interface implementation from .override directive in OnOffBoltCatch
			this.OnOffBoltCatch();
		}

		public virtual void IEventsConsumerOnOffBoltCatch()
		{
		}

		public void OnOnBoltCatch()
		{
			IEventsConsumerOnOnBoltCatch();
		}

		void IActorEvents.OnOnBoltCatch()
		{
			//ILSpy generated this explicit interface implementation from .override directive in OnOnBoltCatch
			this.OnOnBoltCatch();
		}

		public virtual void IEventsConsumerOnOnBoltCatch()
		{
		}

		public void OnPutMagToRig()
		{
			IEventsConsumerOnPutMagToRig();
		}

		void IActorEvents.OnPutMagToRig()
		{
			//ILSpy generated this explicit interface implementation from .override directive in OnPutMagToRig
			this.OnPutMagToRig();
		}

		public virtual void IEventsConsumerOnPutMagToRig()
		{
		}

		public void OnRemoveShell()
		{
			IEventsConsumerOnRemoveShell();
		}

		void IActorEvents.OnRemoveShell()
		{
			//ILSpy generated this explicit interface implementation from .override directive in OnRemoveShell
			this.OnRemoveShell();
		}

		public virtual void IEventsConsumerOnRemoveShell()
		{
		}

		public void OnReplaceSecondMag()
		{
			IEventsConsumerOnReplaceSecondMag();
		}

		void IActorEvents.OnReplaceSecondMag()
		{
			//ILSpy generated this explicit interface implementation from .override directive in OnReplaceSecondMag
			this.OnReplaceSecondMag();
		}

		public virtual void IEventsConsumerOnReplaceSecondMag()
		{
		}

		public void OnShellEject()
		{
			IEventsConsumerOnShellEject();
		}

		void IActorEvents.OnShellEject()
		{
			//ILSpy generated this explicit interface implementation from .override directive in OnShellEject
			this.OnShellEject();
		}

		public virtual void IEventsConsumerOnShellEject()
		{
		}

		public void OnShowAmmo(bool BoolParam)
		{
			IEventsConsumerOnShowAmmo(BoolParam);
		}

		void IActorEvents.OnShowAmmo(bool BoolParam)
		{
			//ILSpy generated this explicit interface implementation from .override directive in OnShowAmmo
			this.OnShowAmmo(BoolParam);
		}

		public virtual void IEventsConsumerOnShowAmmo(bool BoolParam)
		{
		}

		public void OnShowMag()
		{
			IEventsConsumerOnShowMag();
		}

		void IActorEvents.OnShowMag()
		{
			//ILSpy generated this explicit interface implementation from .override directive in OnShowMag
			this.OnShowMag();
		}

		public void OnSliderOut()
		{
		}

		void IActorEvents.OnSliderOut()
		{
			//ILSpy generated this explicit interface implementation from .override directive in OnSliderOut
			this.OnSliderOut();
		}

		public virtual void IEventsConsumerOnShowMag()
		{
		}

		public void OnSound(string StringParam)
		{
			IEventsConsumerOnSound(StringParam);
		}

		void IActorEvents.OnSound(string StringParam)
		{
			//ILSpy generated this explicit interface implementation from .override directive in OnSound
			this.OnSound(StringParam);
		}

		public virtual void IEventsConsumerOnSound(string StringParam)
		{
		}

		public void OnSoundAtPoint(string StringParam)
		{
			IEventsConsumerOnSoundAtPoint(StringParam);
		}

		public virtual void IEventsConsumerOnSoundAtPoint(string StringParam)
		{
		}

		public void OnStartUtilityOperation()
		{
			IEventsConsumerOnStartUtilityOperation();
		}

		void IActorEvents.OnStartUtilityOperation()
		{
			//ILSpy generated this explicit interface implementation from .override directive in OnStartUtilityOperation
			this.OnStartUtilityOperation();
		}

		public virtual void IEventsConsumerOnStartUtilityOperation()
		{
		}

		public void OnThirdAction(int IntParam)
		{
			IEventsConsumerOnThirdAction(IntParam);
		}

		void IActorEvents.OnThirdAction(int IntParam)
		{
			//ILSpy generated this explicit interface implementation from .override directive in OnThirdAction
			this.OnThirdAction(IntParam);
		}

		public virtual void IEventsConsumerOnThirdAction(int IntParam)
		{
		}

		public void OnUseProp(bool BoolParam)
		{
			IEventsConsumerOnOnUseProp(BoolParam);
		}

		public virtual void IEventsConsumerOnOnUseProp(bool BoolParam)
		{
		}

		public void OnUseSecondMagForReload()
		{
			IEventsConsumerOnUseSecondMagForReload();
		}

		void IActorEvents.OnUseSecondMagForReload()
		{
			//ILSpy generated this explicit interface implementation from .override directive in OnUseSecondMagForReload
			this.OnUseSecondMagForReload();
		}

		public virtual void IEventsConsumerOnUseSecondMagForReload()
		{
		}

		public void OnWeapIn()
		{
			IEventsConsumerOnWeapIn();
		}

		void IActorEvents.OnWeapIn()
		{
			//ILSpy generated this explicit interface implementation from .override directive in OnWeapIn
			this.OnWeapIn();
		}

		public virtual void IEventsConsumerOnWeapIn()
		{
		}

		public void OnWeapOut()
		{
			IEventsConsumerOnWeapOut();
		}

		void IActorEvents.OnWeapOut()
		{
			//ILSpy generated this explicit interface implementation from .override directive in OnWeapOut
			this.OnWeapOut();
		}

		public virtual void IEventsConsumerOnWeapOut()
		{
		}

		public void OnBackpackDrop()
		{
		}

		public void OnBackpackDrop(IAnimatorEventParameter param)
		{
			IEventsOnBackpackDrop();
		}

		public virtual void IEventsOnBackpackDrop()
		{
		}

		public void OnCurrentAnimStateEnded()
		{
		}

		void IActorEvents.OnCurrentAnimStateEnded()
		{
			//ILSpy generated this explicit interface implementation from .override directive in OnCurrentAnimStateEnded
			this.OnCurrentAnimStateEnded();
		}

		public void OnSetActiveObject(int objectID)
		{
		}

		void IActorEvents.OnSetActiveObject(int objectID)
		{
			//ILSpy generated this explicit interface implementation from .override directive in OnSetActiveObject
			this.OnSetActiveObject(objectID);
		}

		public void OnDeactivateObject(int objectID)
		{
		}

		void IActorEvents.OnDeactivateObject(int objectID)
		{
			//ILSpy generated this explicit interface implementation from .override directive in OnDeactivateObject
			this.OnDeactivateObject(objectID);
		}

		public void OnBipodToggle(IAnimatorEventParameter param)
		{
			IEventsOnBipodToggle();
		}

		public virtual void IEventsOnBipodToggle()
		{
		}

		public void ReloadTest()
		{
		}

		public void BipodOpen()
		{
		}

		public void BipodClose()
		{
		}

		public virtual void OnZombieFireBullet(IAnimatorEventParameter animatorEventParameter)
		{
		}

		public virtual void OnZombieFireEnd(IAnimatorEventParameter animatorEventParameter)
		{
		}

		public void AimReady()
		{
			OnAimReady();
		}

		void IActorEvents.AimReady()
		{
			//ILSpy generated this explicit interface implementation from .override directive in AimReady
			this.AimReady();
		}

		public void IdleReady()
		{
			OnIdleReady();
		}

		void IActorEvents.IdleReady()
		{
			//ILSpy generated this explicit interface implementation from .override directive in IdleReady
			this.IdleReady();
		}

		public void DropWeapon()
		{
			OnDropWeapon();
		}

		void IActorEvents.DropWeapon()
		{
			//ILSpy generated this explicit interface implementation from .override directive in DropWeapon
			this.DropWeapon();
		}

		public virtual void OnAimReady()
		{
		}

		public virtual void OnIdleReady()
		{
		}

		public virtual void OnDropWeapon()
		{
		}

		public AbstractHandsController()
		{
		}
	}

	public abstract class BaseAnimationOperationClass
	{
		public class HandsControllerOperationLogger : LoggerClass
		{
			[NonSerialized]
			public AbstractHandsController AbstractHandsController_0;

			[NonSerialized]
			public BaseAnimationOperationClass BaseAnimationOperationClass;

			public HandsControllerOperationLogger(LoggerMode loggerMode, BaseAnimationOperationClass objectInHandsOperation)
				: base("hands-states", loggerMode)
			{
				BaseAnimationOperationClass = objectInHandsOperation;
			}

			public void SetHandsController(AbstractHandsController controller)
			{
				AbstractHandsController_0 = controller;
			}

			public void TraceProhibitedCall()
			{
				if (IsEnabled(NLog.LogLevel.Trace))
				{
					StackFrame stackFrame = new StackFrame(2);
					Log("[{0}][Prohibited method call][{1}::{2}][{3}]", "<color='red'>[{0}][Prohibited method call][{1}::{2}][{3}]</color>", NLog.LogLevel.Debug, Time.frameCount, BaseAnimationOperationClass.GetType().Name, stackFrame.GetMethod().Name, AbstractHandsController_0.LoggerDistinctId);
				}
			}

			[Conditional("UNITY_EDITOR")]
			public void TraceMethodCall()
			{
				if (IsEnabled(NLog.LogLevel.Trace))
				{
					StackFrame stackFrame = new StackFrame(2);
					LogTrace("method '{1}' operation: '{0}'", BaseAnimationOperationClass.GetType().Name, stackFrame.GetMethod().Name);
				}
			}

			public void OperationStart()
			{
				if (IsEnabled(NLog.LogLevel.Trace))
				{
					Log("[{0}][{1}::Start][{2}]", "<color=green><b>[{0}][{1}::Start][{2}]</b></color>", NLog.LogLevel.Trace, Time.frameCount, BaseAnimationOperationClass.GetType().Name, AbstractHandsController_0.LoggerDistinctId);
				}
			}
		}

		[NonSerialized]
		public HandsControllerOperationLogger HandsControllerOperationLogger_0;

		[NonSerialized]
		[CompilerGenerated]
		public EOperationState EoperationState_0;

		public virtual EOperationState State
		{
			[CompilerGenerated]
			get
			{
				return EoperationState_0;
			}
			[CompilerGenerated]
			set
			{
				EoperationState_0 = value;
			}
		}

		public BaseAnimationOperationClass(AbstractHandsController handsController)
		{
			HandsControllerOperationLogger_0 = new HandsControllerOperationLogger(LoggerMode.Add, this);
			UpdateLoggerController(handsController);
		}

		public void UpdateLoggerController(AbstractHandsController handsController)
		{
			HandsControllerOperationLogger_0.SetHandsController(handsController);
		}

		public void Start()
		{
			HandsControllerOperationLogger_0.OperationStart();
			State = EOperationState.Executing;
			SetLeftStanceAnimOnStartOperation();
		}

		public virtual void Reset()
		{
			State = EOperationState.Ready;
		}

		public virtual void Update(float deltaTime)
		{
		}

		public void method_0()
		{
			HandsControllerOperationLogger_0.TraceProhibitedCall();
		}

		[Conditional("UNITY_EDITOR")]
		public void method_1()
		{
		}

		public virtual void OnEnd()
		{
		}

		public virtual void SetLeftStanceAnimOnStartOperation()
		{
		}
	}

	public class UsableItemController : ItemHandsController, GInterface202, GInterface199, IHandsController, GInterface197
	{
		public abstract class Class1292 : BaseAnimationOperationClass, Interface11
		{
			[NonSerialized]
			public UsableItemController UsableItemController_0;

			[NonSerialized]
			public Player Player_0;

			public Class1292(UsableItemController controller)
				: base(controller)
			{
				UsableItemController_0 = controller;
				Player_0 = UsableItemController_0._player;
			}

			public virtual bool CanRemove()
			{
				return false;
			}

			public virtual void HideWeaponComplete()
			{
				method_0();
			}

			public virtual void WeaponAppeared()
			{
				method_0();
			}

			public virtual void ExamineWeapon()
			{
				method_0();
			}

			public virtual void OnBackpackDrop()
			{
				method_0();
			}

			public virtual void SetInventoryOpened(bool opened)
			{
				method_0();
			}

			public virtual void Execute(GInterface438 operation, Callback callback)
			{
				method_0();
				if (!(operation is GInterface443 gInterface))
				{
					callback.Succeed();
				}
				else if (UsableItemController_0._player.InventoryController.IsAnimatedSlot(gInterface.From1))
				{
					callback.Fail($"Detach is not supported in current operation: {GetType()}");
				}
				else
				{
					callback.Succeed();
				}
			}

			public virtual void HideWeapon(Action onHidden, bool fastDrop)
			{
				method_0();
			}

			public virtual void SetAiming(bool isAiming)
			{
				method_0();
			}

			public virtual void FastForward()
			{
				method_0();
			}

			public virtual void OnAimingDisabled()
			{
				method_0();
			}

			public virtual void SetCompassState(bool active)
			{
				method_0();
			}

			public virtual void OnIdleStart()
			{
			}
		}

		public class Class1293 : Class1292
		{
			[NonSerialized]
			public Callback Callback_0;

			public Class1293(UsableItemController controller)
				: base(controller)
			{
			}

			public virtual void Start(Item item, Callback callback)
			{
				Callback_0 = callback;
				Start();
				UsableItemController_0.SetAim(value: false);
				UsableItemController_0.FirearmsAnimator.SetInventory(open: false);
				UsableItemController_0.firearmsAnimator_0.Animator.SetLayerWeight(2, 1f);
				UsableItemController_0._player.SendHandsInteractionStateChanged(value: true, 300);
				Player_0.MovementContext.SetInteractInHands(EInteraction.DropBackpack);
			}

			public override void Reset()
			{
				Callback_0 = null;
				base.Reset();
			}

			public override void OnBackpackDrop()
			{
				State = EOperationState.Finished;
				UsableItemController_0._player.SendHandsInteractionStateChanged(value: false, 300);
				Player_0.MovementContext.SetInteractInHands(EInteraction.DropBackpack);
				WeaponAnimationSpeedControllerClass.ResetTriggerHandReady(UsableItemController_0.FirearmsAnimator.Animator);
				UsableItemController_0.firearmsAnimator_0.SetInventory(UsableItemController_0.bool_0);
				vmethod_0();
				Callback_0.Succeed();
			}

			public override void SetInventoryOpened(bool opened)
			{
				UsableItemController_0.bool_0 = opened;
			}

			public override void SetAiming(bool isAiming)
			{
				if (!isAiming || EFTHardSettings.Instance.CanAimInState(Player_0.CurrentState.Name))
				{
					UsableItemController_0.FirearmsAnimator.SetFire(isAiming);
					UsableItemController_0.IsAiming = isAiming;
				}
			}

			public virtual void vmethod_0()
			{
				UsableItemController_0.InitiateOperation<Class1299>().Start();
			}
		}

		public class Class1294 : Class1293
		{
			[NonSerialized]
			public const float Float_0 = 0.25f;

			[NonSerialized]
			public float Float_1;

			[NonSerialized]
			public bool Bool_0;

			public Class1294(UsableItemController controller)
				: base(controller)
			{
			}

			public override void Start(Item item, Callback callback)
			{
				Float_1 = 0f;
				Bool_0 = false;
				base.Start(item, callback);
			}

			public override void FastForward()
			{
				if (!Bool_0)
				{
					Bool_0 = true;
					OnBackpackDrop();
				}
			}

			public override void Update(float deltaTime)
			{
				base.Update(deltaTime);
				if (!Bool_0 && Float_1 > 0.25f)
				{
					Bool_0 = true;
					OnBackpackDrop();
				}
				Float_1 += deltaTime;
			}
		}

		public class Class1299 : Class1292
		{
			[NonSerialized]
			public const float Float_0 = 300f;

			[NonSerialized]
			public float Float_1;

			public Class1299(UsableItemController controller)
				: base(controller)
			{
			}

			public new void Start()
			{
				base.Start();
				Float_1 = 0f;
			}

			public override void Reset()
			{
				Float_1 = 0f;
				base.Reset();
			}

			public override void HideWeapon(Action onHidden, bool fastDrop)
			{
				State = EOperationState.Finished;
				UsableItemController_0.InitiateOperation<Class1302>().Start(onHidden, fastDrop);
				UsableItemController_0.Hide();
			}

			public override bool CanRemove()
			{
				return true;
			}

			public override void Execute(GInterface438 operation, Callback callback)
			{
				if (!(operation is GInterface443 gInterface))
				{
					callback.Succeed();
				}
				else if (Player_0.InventoryController.IsAnimatedSlot(gInterface.From1))
				{
					State = EOperationState.Finished;
					vmethod_0(gInterface, callback);
				}
				else
				{
					callback.Succeed();
				}
			}

			public override void Update(float deltaTime)
			{
				Float_1 += deltaTime;
				if (Float_1 > 300f)
				{
					UsableItemController_0.firearmsAnimator_0.Idle();
					Float_1 = 0f;
				}
			}

			public override void SetAiming(bool isAiming)
			{
				if ((!isAiming || EFTHardSettings.Instance.CanAimInState(Player_0.CurrentState.Name)) && (!isAiming || !(UsableItemController_0.float_1 > EFTHardSettings.Instance.STOP_AIMING_AT)))
				{
					UsableItemController_0.FirearmsAnimator.SetFire(isAiming);
					UsableItemController_0.IsAiming = isAiming;
					Float_1 = 0f;
				}
			}

			public override void ExamineWeapon()
			{
				UsableItemController_0.firearmsAnimator_0.LookTrigger();
			}

			public override void OnAimingDisabled()
			{
				SetAiming(isAiming: false);
			}

			public override void SetInventoryOpened(bool opened)
			{
				SetAiming(isAiming: false);
				UsableItemController_0.bool_0 = opened;
				UsableItemController_0.firearmsAnimator_0.SetInventory(opened);
			}

			public override void OnEnd()
			{
				SetCompassState(active: false);
			}

			public override void SetCompassState(bool active)
			{
				UsableItemController_0.CompassState.Value = active;
			}

			public virtual void vmethod_0(GInterface443 oneItemOperation, Callback callback)
			{
				UsableItemController_0.InitiateOperation<Class1293>().Start(oneItemOperation.Item1, callback);
			}
		}

		public class Class1302 : Class1292
		{
			[NonSerialized]
			public Action Action_0;

			public Class1302(UsableItemController controller)
				: base(controller)
			{
			}

			public virtual void Start(Action onHidden, bool fastDrop)
			{
				Action_0 = onHidden;
				Start();
				UsableItemController_0.firearmsAnimator_0.SetActiveParam(active: false);
				UsableItemController_0.firearmsAnimator_0.SetFastHide(fastDrop);
				UsableItemController_0._player.BodyAnimatorCommon.SetFloat(PlayerAnimator.RELOAD_FLOAT_PARAM_HASH, 1f);
				UsableItemController_0.IsAiming = false;
			}

			public override void Reset()
			{
				Action_0 = null;
				base.Reset();
			}

			public override void HideWeaponComplete()
			{
				State = EOperationState.Finished;
				Action_0?.Invoke();
			}

			public override void HideWeapon(Action onHidden, bool fastDrop)
			{
				Action_0 = (Action)Delegate.Combine(Action_0, onHidden);
			}

			public override void FastForward()
			{
				if (State != EOperationState.Finished)
				{
					HideWeaponComplete();
				}
			}
		}

		public class Class1305 : Class1292
		{
			[NonSerialized]
			public Action Action_0;

			[NonSerialized]
			public Action Action_1;

			[NonSerialized]
			public bool Bool_0;

			public Class1305(UsableItemController controller)
				: base(controller)
			{
			}

			public void Start(Action callback)
			{
				Action_1 = callback;
				Start();
				UsableItemController_0.firearmsAnimator_0.SetActiveParam(active: true);
				UsableItemController_0.firearmsAnimator_0.SetMeleeSpeed(Player_0.Physical.MeleeSpeed);
				Player_0.BodyAnimatorCommon.SetFloat(PlayerAnimator.WEAPON_SIZE_MODIFIER_PARAM_HASH, 1f);
				UsableItemController_0._player.BodyAnimatorCommon.SetFloat(PlayerAnimator.RELOAD_FLOAT_PARAM_HASH, 1f);
			}

			public override void Reset()
			{
				base.Reset();
				Action_1 = null;
				Action_0 = null;
			}

			public override void WeaponAppeared()
			{
				UsableItemController_0.SetupProp();
				State = EOperationState.Finished;
				UsableItemController_0._player.BodyAnimatorCommon.SetFloat(PlayerAnimator.RELOAD_FLOAT_PARAM_HASH, 0f);
				vmethod_0();
			}

			public override void HideWeapon(Action onHidden, bool fastDrop)
			{
				Action_0 = onHidden;
				Bool_0 = fastDrop;
			}

			public override void FastForward()
			{
				if (State != EOperationState.Finished)
				{
					WeaponAppeared();
				}
			}

			public virtual void vmethod_0()
			{
				Class1299 @class = UsableItemController_0.InitiateOperation<Class1299>();
				@class.Start();
				Action_1();
				if (Action_0 != null)
				{
					@class.HideWeapon(Action_0, Bool_0);
				}
			}

			public override void SetLeftStanceAnimOnStartOperation()
			{
				Player_0.MovementContext.LeftStanceController.DisableLeftStanceAnimFromHandsAction();
			}
		}

		[CompilerGenerated]
		public class Class1308
		{
			public UsableItemController usableItemController_0;

			public Class1312 inventoryOperation;

			public Action callback;

			public void method_0()
			{
				usableItemController_0._player.MovementContext.OnStateChanged -= usableItemController_0.vmethod_2;
				usableItemController_0._player.Physical.OnSprintStateChangedEvent -= usableItemController_0.method_4;
				usableItemController_0._player.ProceduralWeaponAnimation.enabled = true;
				inventoryOperation.Confirm();
				callback();
			}
		}

		private const float float_0 = 0.5f;

		private static readonly RaycastHit[] raycastHit_0 = new RaycastHit[8];

		private int int_0;

		private bool bool_0;

		private bool bool_1;

		protected float float_1;

		protected GClass2086 gclass2086_0;

		protected FirearmsAnimator firearmsAnimator_0;

		protected Func<RaycastHit, bool> func_0;

		protected bool bool_2;

		protected float float_2;

		protected bool bool_3;

		public Interface11 Interface11_0 => base.CurrentHandsOperation as Interface11;

		public override FirearmsAnimator FirearmsAnimator => firearmsAnimator_0;

		public override string LoggerDistinctId => $"{_player.ProfileId}|{_player.Profile.Info.Nickname}|{this}";

		public override bool IsAiming
		{
			get
			{
				return bool_3;
			}
			set
			{
				if (!value)
				{
					_player.Physical.HoldBreath(enable: false);
				}
				if (bool_3 != value)
				{
					bool_3 = value;
					_player.Skills.FastAimTimer.Target = (value ? 0f : 2f);
					_player.MovementContext.SetAimingSlowdown(IsAiming, 0.33f);
					_player.Physical.Aim(bool_3 ? 1 : 0);
					AimingChanged(value);
					_player.ProceduralWeaponAnimation.IsAiming = bool_3;
				}
			}
		}

		public static T smethod_6<T>(Player player, Item item) where T : UsableItemController
		{
			T val = ItemHandsController.smethod_0<T>(player, item);
			smethod_8(val, player);
			return val;
		}

		public static async Task<T> smethod_7<T>(Player player, Item item) where T : UsableItemController
		{
			T obj = await ItemHandsController.smethod_2<T>(player, item);
			smethod_8(obj, player);
			return obj;
		}

		public static void smethod_8<T>(T controller, Player player) where T : UsableItemController
		{
			WeaponPrefab weaponPrefab = smethod_9(controller);
			controller.vmethod_0(player, weaponPrefab);
		}

		public static WeaponPrefab smethod_9<T>(T controller) where T : UsableItemController
		{
			return controller._controllerObject.GetComponentInChildren<WeaponPrefab>();
		}

		public virtual void vmethod_0(Player player, WeaponPrefab weaponPrefab)
		{
			func_0 = method_8;
			gclass2086_0 = weaponPrefab.ObjectInHands;
			firearmsAnimator_0 = weaponPrefab.FirearmsAnimator;
			firearmsAnimator_0.AddEventsConsumer(this);
			CompositeDisposable.AddDisposable(delegate
			{
				firearmsAnimator_0.RemoveEventsConsumer(this);
			});
			_player.HandsAnimator = firearmsAnimator_0;
			player.ProceduralWeaponAnimation.ClearPreviousWeapon();
			player.ProceduralWeaponAnimation.InitTransforms(HandsHierarchy);
			player.ProceduralWeaponAnimation.method_9(weaponPrefab);
			player.ProceduralWeaponAnimation.FindAimTransformsWithoutSights();
			player.ProceduralWeaponAnimation.ResetScopeRotation();
		}

		public override void IEventsConsumerOnWeapIn()
		{
			method_2();
		}

		public override void IEventsConsumerOnWeapOut()
		{
			method_1();
		}

		public override void IEventsConsumerOnThirdAction(int intParam)
		{
			TranslateAnimatorParameter(intParam);
		}

		public override void IEventsOnBackpackDrop()
		{
			method_3();
		}

		public override void IEventsConsumerOnIdleStart()
		{
			Interface11_0.OnIdleStart();
		}

		public override void ManualLateUpdate(float deltaTime)
		{
			if ((!BackendConfigAbstractClass.Config.UseSpiritPlayer || !_player.Spirit.IsActive) && bool_1)
			{
				method_9();
				bool_1 = false;
			}
		}

		public override void ManualUpdate(float deltaTime)
		{
			base.ManualUpdate(deltaTime);
			bool_1 = true;
		}

		public override void IEventsConsumerOnOnUseProp(bool boolParam)
		{
			SetPropVisibility(boolParam);
		}

		public override bool SupportPickup()
		{
			return true;
		}

		public override bool IsInventoryOpen()
		{
			return bool_0;
		}

		public override void SetInventoryOpened(bool opened)
		{
			if (opened)
			{
				SetCompassState(active: false);
			}
			Interface11_0.SetInventoryOpened(opened);
			_player.CurrentManagedState?.OnInventory(opened);
		}

		public override void Pickup(bool p)
		{
			if (CanInteract())
			{
				firearmsAnimator_0.SetPickup(p);
			}
		}

		public override void Interact(bool isInteracting, int actionIndex)
		{
			if (CanInteract())
			{
				_player.SendHandsInteractionStateChanged(isInteracting, actionIndex);
				firearmsAnimator_0.SetInteract(isInteracting, actionIndex);
			}
		}

		public override void Loot(bool p)
		{
			if (CanInteract())
			{
				firearmsAnimator_0.SetLooting(p);
			}
		}

		public override bool CanRemove()
		{
			return true;
		}

		public override bool CanInteract()
		{
			if (firearmsAnimator_0.IsIdling())
			{
				return firearmsAnimator_0.Animator.GetBool(WeaponAnimationSpeedControllerClass.BOOL_ACTIVE);
			}
			return false;
		}

		public override void ShowGesture(EInteraction gesture)
		{
			SetAim(value: false);
			if (gesture != EInteraction.None)
			{
				firearmsAnimator_0.Gesture(gesture);
			}
		}

		public virtual bool ExamineWeapon()
		{
			if (Interface11_0 is Class1299 && !_player.InventoryController.HasAnyHandsAction())
			{
				Interface11_0.ExamineWeapon();
				return true;
			}
			return false;
		}

		public override bool IsInInteraction()
		{
			return firearmsAnimator_0.IsInInteraction;
		}

		public virtual void ToggleAim()
		{
			SetCompassState(active: false);
			SetAim(!IsAiming);
		}

		public virtual void SetAim(bool value)
		{
			bool isAiming = IsAiming;
			Interface11_0.SetAiming(value);
			_player.Boolean_0 &= !value;
			if (isAiming != IsAiming)
			{
				float value2 = 1f - (float)_player.Skills.DrawSound;
				value2 = Mathf.Clamp(value2, 0.1f, 0.2f);
				_player.method_60(value2);
			}
		}

		public virtual void Hide()
		{
		}

		public override bool IsInInteractionStrictCheck()
		{
			if (!IsInInteraction() && !(firearmsAnimator_0.GetLayerWeight(firearmsAnimator_0.LACTIONS_LAYER_INDEX) >= float.Epsilon))
			{
				return firearmsAnimator_0.Animator.IsInTransition(firearmsAnimator_0.LACTIONS_LAYER_INDEX);
			}
			return true;
		}

		public override void Spawn(float animationSpeed, Action callback)
		{
			firearmsAnimator_0.SetAnimationSpeed(animationSpeed);
			vmethod_1(callback);
			firearmsAnimator_0.SkipTime(Time.fixedDeltaTime);
			_player.MovementContext.OnStateChanged += vmethod_2;
			_player.Physical.OnSprintStateChangedEvent += method_4;
		}

		public override void Drop(float animationSpeed, Action callback, bool fastDrop, Item nextControllerItem = null)
		{
			if (base.Destroyed)
			{
				Interface11_0.HideWeapon(callback, fastDrop);
				return;
			}
			base.Destroyed = true;
			Class1312 inventoryOperation = _player.method_138(base.Item);
			Action onHidden = delegate
			{
				_player.MovementContext.OnStateChanged -= vmethod_2;
				_player.Physical.OnSprintStateChangedEvent -= method_4;
				_player.ProceduralWeaponAnimation.enabled = true;
				inventoryOperation.Confirm();
				callback();
			};
			Interface11_0.HideWeapon(onHidden, fastDrop);
		}

		public override void Destroy()
		{
			_player.ProceduralWeaponAnimation.ClearPreviousWeapon();
			base.Destroy();
			firearmsAnimator_0 = null;
			_player.MovementContext.OnStateChanged -= vmethod_2;
			_player.Physical.OnSprintStateChangedEvent -= method_4;
			AssetPoolObject.ReturnToPool(_controllerObject.gameObject);
		}

		public virtual void vmethod_1(Action callback)
		{
			InitiateOperation<Class1305>().Start(callback);
		}

		public void method_1()
		{
			Interface11_0.HideWeaponComplete();
		}

		public void method_2()
		{
			Interface11_0.WeaponAppeared();
		}

		public void method_3()
		{
			Interface11_0.OnBackpackDrop();
		}

		public void method_4(bool obj)
		{
			if (IsAiming && obj)
			{
				method_5();
			}
		}

		public virtual void vmethod_2(EPlayerState previousstate, EPlayerState nextstate)
		{
			if (!EFTHardSettings.Instance.CanAimInState(nextstate))
			{
				method_5();
			}
		}

		public void method_5()
		{
			Interface11_0.OnAimingDisabled();
		}

		public override bool CanExecute(GInterface438 operation)
		{
			return true;
		}

		public override void Execute(GInterface438 operation, Callback callback)
		{
			Interface11_0.Execute(operation, callback);
		}

		public override void FastForwardCurrentState()
		{
			Interface11_0.FastForward();
		}

		public void method_6()
		{
			float_1 = 0f;
			float_2 = 0f;
			if (!(_player.MovementContext.StationaryWeapon != null) || _player.MovementContext.StationaryWeapon.Item != base.Item)
			{
				if (base.WeaponRoot == null)
				{
					UnityEngine.Debug.LogError("No muzzle or Weapon_root. Overlapping disabled");
					return;
				}
				float_2 = 0.5f;
				int_0 = LayerMask.NameToLayer("Player");
			}
		}

		public float method_7(Vector3 origin, float ln, ref bool overlapsWithPlayer)
		{
			Vector3 end = origin - base.WeaponRoot.up * ln;
			if (EFTPhysicsClass.Linecast(origin, end, out var bestHit, EFTHardSettings.Instance.WEAPON_OCCLUSION_LAYERS, reverseCheck: false, raycastHit_0, func_0))
			{
				overlapsWithPlayer = bestHit.collider.gameObject.layer == int_0;
				return ln - bestHit.distance;
			}
			Vector3 lhs = origin - _player.Position;
			Vector3 up = Vector3.up;
			float num = Vector3.Dot(lhs, up);
			if (EFTPhysicsClass.Linecast(_player.Position + num * up, origin, out bestHit, EFTHardSettings.Instance.WEAPON_OCCLUSION_LAYERS, reverseCheck: false, raycastHit_0, func_0))
			{
				overlapsWithPlayer = bestHit.collider.gameObject.layer == int_0;
				return ln;
			}
			return 0f;
		}

		public bool method_8(RaycastHit overlapHit)
		{
			GameObject gameObject = overlapHit.collider.gameObject;
			if (gameObject.layer == int_0)
			{
				return gameObject == _player.gameObject;
			}
			return false;
		}

		public void method_9()
		{
			if (_player.IsVisible && float_2 > 0f && !(Interface11_0 is Class1302))
			{
				Vector3 position = _player.ProceduralWeaponAnimation.HandsContainer.HandsPosition.Get();
				float num = 1f;
				if (_player.ProceduralWeaponAnimation.BlindfireBlender.Value != 0f)
				{
					Vector3 position2 = (_player.ProceduralWeaponAnimation.BlindFireEndPosition + _player.ProceduralWeaponAnimation.PositionZeroSum) * 1.9f;
					position2 = _player.ProceduralWeaponAnimation.HandsContainer.WeaponRootAnim.parent.TransformPoint(position2);
					num = method_7(position2, float_2, ref _player.ProceduralWeaponAnimation.TurnAway.OverlapsWithPlayer);
				}
				if (num < 0.02f)
				{
					_player.ProceduralWeaponAnimation.TurnAway.OverlapDepth = num;
					_player.ProceduralWeaponAnimation.OverlappingAllowsBlindfire = true;
				}
				else
				{
					_player.ProceduralWeaponAnimation.OverlappingAllowsBlindfire = false;
					_player.ProceduralWeaponAnimation.TurnAway.OriginZShift = position.y;
					position = _player.ProceduralWeaponAnimation.HandsContainer.WeaponRootAnim.parent.TransformPoint(position);
					num = method_7(position, float_2, ref _player.ProceduralWeaponAnimation.TurnAway.OverlapsWithPlayer);
					_player.ProceduralWeaponAnimation.TurnAway.OverlapDepth = num;
				}
				float_1 = num;
				if (num > EFTHardSettings.Instance.STOP_AIMING_AT && IsAiming)
				{
					ToggleAim();
					bool_2 = true;
				}
				else if (num < EFTHardSettings.Instance.STOP_AIMING_AT && _player.ProceduralWeaponAnimation.TurnAway.OverlapValue < 0.2f && bool_2 && !IsAiming)
				{
					ToggleAim();
					bool_2 = false;
				}
			}
		}

		public override Dictionary<Type, OperationFactoryDelegate> GetOperationFactoryDelegates()
		{
			return new Dictionary<Type, OperationFactoryDelegate>
			{
				{
					typeof(Class1305),
					() => new Class1305(this)
				},
				{
					typeof(Class1299),
					() => new Class1299(this)
				},
				{
					typeof(Class1302),
					() => new Class1302(this)
				},
				{
					typeof(Class1293),
					() => new Class1293(this)
				}
			};
		}

		[CompilerGenerated]
		public void method_10()
		{
			firearmsAnimator_0.RemoveEventsConsumer(this);
		}

		[CompilerGenerated]
		public BaseAnimationOperationClass method_11()
		{
			return new Class1305(this);
		}

		[CompilerGenerated]
		public BaseAnimationOperationClass method_12()
		{
			return new Class1299(this);
		}

		[CompilerGenerated]
		public BaseAnimationOperationClass method_13()
		{
			return new Class1302(this);
		}

		[CompilerGenerated]
		public BaseAnimationOperationClass method_14()
		{
			return new Class1293(this);
		}
	}

	public interface Interface11
	{
		void HideWeaponComplete();

		void WeaponAppeared();

		void HideWeapon(Action onHidden, bool fastDrop);

		void OnBackpackDrop();

		void SetAiming(bool isAiming);

		void Execute(GInterface438 operation, Callback callback);

		void ExamineWeapon();

		void FastForward();

		void OnIdleStart();

		void OnAimingDisabled();

		void SetInventoryOpened(bool opened);
	}

	[Flags]
	public enum EAnimatorMask
	{
		Thirdperson = 1,
		Arms = 2,
		Procedural = 4,
		FBBIK = 8,
		IK = 0x10
	}

	public class GClass724 : LoggerClass
	{
		public GClass724(LoggerMode loggerMode)
			: base("player", loggerMode)
		{
		}
	}

	public class GClass2059<T> where T : class, IItemComponent
	{
		public readonly BindableEvent Changed = new BindableEvent();

		[NonSerialized]
		[CanBeNull]
		public T Gparam_0;

		[NonSerialized]
		public Slot Slot_0;

		[NonSerialized]
		public Func<T, Action, Action> Func_0;

		[NonSerialized]
		public Action Action_0;

		[CanBeNull]
		public virtual T Component => GetItemComponent();

		public GClass2059(Slot slot, Func<T, Action, Action> subscriber)
		{
			Slot_0 = slot;
			Func_0 = subscriber;
			Set(GetItemComponent());
		}

		public void Update()
		{
			Set(Component);
		}

		public void Set([CanBeNull] T value)
		{
			if (value != Gparam_0)
			{
				if (Gparam_0 != null)
				{
					Action_0();
				}
				Gparam_0 = value;
				if (Gparam_0 != null)
				{
					Action_0 = Func_0(Gparam_0, method_0);
				}
				method_0();
			}
		}

		public T GetItemComponent()
		{
			CompoundItem obj = Slot_0.ContainedItem as CompoundItem;
			if (obj == null)
			{
				return null;
			}
			return GClass3380.GetItemComponentsInChildren<T>(obj).FirstOrDefault();
		}

		public void method_0()
		{
			Changed.Invoke();
		}

		public virtual void Dispose()
		{
			if (Gparam_0 != null)
			{
				Action_0();
			}
			Gparam_0 = null;
		}
	}

	public enum EUpdateMode
	{
		Auto,
		Manual,
		None
	}

	public delegate float GDelegate66();

	public enum EVoipState : byte
	{
		NotAvailable,
		Available,
		Off,
		Banned,
		MicrophoneFail
	}

	public enum EProcessStatus
	{
		None,
		Scheduled,
		Internal
	}

	public class GClass2060 : IRaiseEvents, IRollback
	{
		[NonSerialized]
		public GClass3408 Gclass3408_0;

		[NonSerialized]
		public ThrowWeapItemClass ThrowWeapItemClass;

		public GClass2060(GClass3408 discardResult, ThrowWeapItemClass grenade)
		{
			Gclass3408_0 = discardResult;
			ThrowWeapItemClass = grenade;
		}

		public void RollBack()
		{
			Gclass3408_0.RollBack();
		}

		public void RaiseEvents(IItemOwner controller, CommandStatus status)
		{
			Gclass3408_0.RaiseEvents(controller, status);
		}

		public bool CanExecute(TraderControllerClass itemController)
		{
			return ThrowWeapItemClass.CheckAction(ThrowWeapItemClass.CurrentAddress).Succeeded;
		}
	}

	public abstract class Class1310 : IDisposable
	{
		public enum EInternalState
		{
			Creating,
			Executed,
			Confirmed,
			Disposed
		}

		[NonSerialized]
		[CompilerGenerated]
		public Player Player_0_1;

		[NonSerialized]
		[CompilerGenerated]
		public EInternalState EinternalState_0;

		[NonSerialized]
		[CompilerGenerated]
		public CommandStatus CommandStatus_0_1;

		[NonSerialized]
		[CompilerGenerated]
		public Item Item_0_1;

		public Player Player_0
		{
			[CompilerGenerated]
			get
			{
				return Player_0_1;
			}
			[CompilerGenerated]
			set
			{
				Player_0_1 = value;
			}
		}

		public EInternalState EInternalState_0
		{
			[CompilerGenerated]
			get
			{
				return EinternalState_0;
			}
			[CompilerGenerated]
			set
			{
				EinternalState_0 = value;
			}
		}

		public CommandStatus CommandStatus_0
		{
			[CompilerGenerated]
			get
			{
				return CommandStatus_0_1;
			}
			[CompilerGenerated]
			set
			{
				CommandStatus_0_1 = value;
			}
		}

		public Item Item_0
		{
			[CompilerGenerated]
			get
			{
				return Item_0_1;
			}
			[CompilerGenerated]
			set
			{
				Item_0_1 = value;
			}
		}

		public bool Boolean_0 => EInternalState_0 == EInternalState.Disposed;

		public Class1310(Player player, Item item)
		{
			Player_0 = player;
			Item_0 = item;
		}

		~Class1310()
		{
			Player_0 = null;
			Item_0 = null;
		}

		public void Execute()
		{
			if (EInternalState_0 == EInternalState.Creating)
			{
				EInternalState_0 = EInternalState.Executed;
				vmethod_0();
			}
		}

		public void Confirm(bool succeed = true)
		{
			if (!Boolean_0 && EInternalState_0 == EInternalState.Executed)
			{
				EInternalState_0 = EInternalState.Confirmed;
				vmethod_1(succeed);
			}
		}

		public abstract void vmethod_0();

		public abstract void vmethod_1(bool succeed);

		public void Dispose()
		{
			if (!Boolean_0)
			{
				EInternalState_0 = EInternalState.Disposed;
				if (CommandStatus_0 == CommandStatus.Begin)
				{
					CommandStatus_0 = CommandStatus.Failed;
				}
				Player_0 = null;
				Item_0 = null;
				GC.SuppressFinalize(this);
			}
		}
	}

	public class Class1311 : Class1310
	{
		public Class1311(Player player, Item item)
			: base(player, item)
		{
		}

		public override void vmethod_0()
		{
			base.Player_0.InventoryController.RaiseEvent(new GEventArgs9(base.Item_0, base.CommandStatus_0, base.Player_0.InventoryController));
		}

		public override void vmethod_1(bool succeed)
		{
			base.CommandStatus_0 = (succeed ? CommandStatus.Succeed : CommandStatus.Failed);
			base.Player_0.InventoryController.RaiseEvent(new GEventArgs9(base.Item_0, base.CommandStatus_0, base.Player_0.InventoryController));
		}
	}

	public class Class1312 : Class1310
	{
		public Class1312(Player player, Item item)
			: base(player, item)
		{
		}

		public override void vmethod_0()
		{
			base.Player_0.InventoryController.RaiseEvent(new GEventArgs10(base.Item_0, base.CommandStatus_0, base.Player_0.InventoryController));
		}

		public override void vmethod_1(bool succeed)
		{
			base.CommandStatus_0 = (succeed ? CommandStatus.Succeed : CommandStatus.Failed);
			base.Player_0.InventoryController.RaiseEvent(new GEventArgs10(base.Item_0, base.CommandStatus_0, base.Player_0.InventoryController));
		}
	}

	public abstract class AbstractProcess
	{
		public enum Completion
		{
			Sync,
			Async
		}

		public enum Confirmation
		{
			Unknown,
			Succeed,
			Failed
		}

		public static void Execute(AbstractProcess process)
		{
			process.Execute();
		}

		public abstract void Execute();

		public static bool TrySkip(AbstractProcess process)
		{
			return process.TrySkip();
		}

		public abstract bool TrySkip();

		public abstract void CreateController();

		public abstract void Skip(string error);

		public abstract void SkipToNext(string error);

		public abstract void Begin(string error = null);

		public abstract void Complete();

		public abstract void Complete(string error);

		public abstract void Abort();

		public abstract void AbortAfterCompletion();

		public abstract void ExecuteNext();

		public AbstractProcess()
		{
		}
	}

	public class Process<TController, TResult> : AbstractProcess where TController : AbstractHandsController, TResult
	{
		[Serializable]
		[CompilerGenerated]
		public class Class1313
		{
			public static readonly Class1313 class1313_0 = new Class1313();

			public static Callback callback_0;

			public static Callback<TResult> callback_1;

			public void method_0(IResult result)
			{
			}

			public void method_1(Result<TResult> result)
			{
			}
		}

		[CompilerGenerated]
		public class Class1314
		{
			public Process<TController, TResult> process_0;

			public Action execute;

			public void method_0()
			{
				process_0.Bool_0 = false;
				if (process_0.Item_0 != null)
				{
					process_0.Player_0.method_136(process_0.Item_0);
				}
				process_0.Begin();
				if (process_0.Confirmation_0 == Confirmation.Failed)
				{
					process_0.SkipToNext("not confirmed");
				}
				else
				{
					process_0.CreateController();
				}
			}

			public void method_1()
			{
				process_0.Player_0.Logger.LogInfo("[Player.Process] Execute, controller dropped");
				process_0.Player_0.DestroyController();
				if (process_0.Player_0.AbstractProcess_0 != null && process_0.Bool_0)
				{
					process_0.SkipToNext("skipped skippable");
				}
				else
				{
					execute();
				}
			}
		}

		[CompilerGenerated]
		public class Class1315
		{
			public Process<TController, TResult> process_0;

			public Class1311 setInHandsOperation;

			public void method_0()
			{
				process_0.Player_0.Logger.LogDebug("[Player.Process] CreateController, controller created, for item {0}, _confirmed {1}", (process_0.Item_0 != null) ? GClass2348.Localized(process_0.Item_0.ShortName) : "nullitem", process_0.Confirmation_0);
				process_0.Bool_3 = true;
				if (setInHandsOperation != null)
				{
					setInHandsOperation.Confirm();
				}
				switch (process_0.Confirmation_0)
				{
				default:
					throw new ArgumentException("Invalid enum");
				case Confirmation.Unknown:
					if (process_0.Completion_0 == Completion.Async)
					{
						process_0.Complete();
					}
					break;
				case Confirmation.Succeed:
					process_0.Complete();
					process_0.Player_0.Logger.LogInfo("SpawnController Confirmation.Succeed");
					process_0.ExecuteNext();
					break;
				case Confirmation.Failed:
					process_0.Abort();
					break;
				}
			}
		}

		[NonSerialized]
		public Player Player_0;

		[NonSerialized]
		public Func<TController> Func_0;

		[NonSerialized]
		[CanBeNull]
		public Item Item_0;

		[NonSerialized]
		public Completion Completion_0;

		[NonSerialized]
		public Confirmation Confirmation_0;

		[NonSerialized]
		public bool Bool_0;

		[NonSerialized]
		public bool Bool_1;

		[NonSerialized]
		public Callback Callback_0;

		[NonSerialized]
		public Callback<TResult> Callback_1;

		[NonSerialized]
		public TController Gparam_0;

		[NonSerialized]
		public bool Bool_2;

		[NonSerialized]
		public bool Bool_3 = true;

		public Process(Player player, Func<TController> controllerFactory, [CanBeNull] Item item, bool fastHide = false, Completion completion = Completion.Sync, Confirmation confirmation = Confirmation.Succeed, bool skippable = true)
		{
			Player_0 = player;
			Func_0 = controllerFactory;
			Item_0 = item;
			Completion_0 = completion;
			Confirmation_0 = confirmation;
			Bool_0 = skippable;
			Bool_2 = fastHide;
		}

		public void method_0([CanBeNull] Callback beginCallback, [CanBeNull] Callback<TResult> completeCallback, bool scheduled)
		{
			Player_0.Logger.LogInfo("Proceed: for {0}", Item_0);
			Callback_0 = beginCallback ?? ((Callback)delegate
			{
			});
			Callback_1 = completeCallback ?? ((Callback<TResult>)delegate
			{
			});
			if (Player_0.Profile.Info.Settings.Role == WildSpawnType.bossBoar)
			{
				Player_0.MovementContext.PlayerAnimator.EnableBoarPkm(Item_0.Id == "64cd089ceb496c0c707336e3");
			}
			if (Player_0._handsController != null && Player_0._handsController.Item == Item_0)
			{
				Skip("_player._handsController != null && ReferenceEquals. _item:" + Item_0?.ShortName + " _playerItems:" + Player_0?._handsController?.Item?.ShortName);
				return;
			}
			switch (Player_0.ProcessStatus)
			{
			default:
				throw new ArgumentException("Invalid enum");
			case EProcessStatus.None:
				Player_0.Logger.LogInfo("{0} {1} setting process status from {2} to Scheduled", this, Item_0, Player_0.ProcessStatus);
				Player_0.ProcessStatus = EProcessStatus.Scheduled;
				Execute();
				break;
			case EProcessStatus.Scheduled:
				if (Player_0.AbstractProcess_0 == null || AbstractProcess.TrySkip(Player_0.AbstractProcess_0))
				{
					Player_0.AbstractProcess_0 = this;
				}
				else
				{
					Skip("not scheduled EProcessStatus.Scheduled");
				}
				break;
			case EProcessStatus.Internal:
				if (scheduled)
				{
					Skip("not scheduled Internal");
				}
				else
				{
					Execute();
				}
				break;
			}
		}

		public override void Execute()
		{
			Player_0.Logger.LogInfo("[Player.Process] Execute for item: {0}", (Item_0 != null) ? GClass2348.Localized(Item_0.ShortName) : "nullitem");
			Action execute = delegate
			{
				Bool_0 = false;
				if (Item_0 != null)
				{
					Player_0.method_136(Item_0);
				}
				Begin();
				if (Confirmation_0 == Confirmation.Failed)
				{
					SkipToNext("not confirmed");
				}
				else
				{
					CreateController();
				}
			};
			if (Player_0.HandsController == null)
			{
				execute();
				return;
			}
			if (Player_0.HandsController.Item == Item_0 && !Player_0.HandsController.Destroyed)
			{
				SkipToNext("skipped same");
				return;
			}
			Player_0.Logger.LogInfo("[Player.Process] Execute, start controller drop");
			Player_0.DropCurrentController(delegate
			{
				Player_0.Logger.LogInfo("[Player.Process] Execute, controller dropped");
				Player_0.DestroyController();
				if (Player_0.AbstractProcess_0 != null && Bool_0)
				{
					SkipToNext("skipped skippable");
				}
				else
				{
					execute();
				}
			}, Bool_2, Item_0);
		}

		public override void CreateController()
		{
			Player_0.Logger.LogInfo("[Player.Process] CreateController, start controller create, for item {0}", (Item_0 != null) ? GClass2348.Localized(Item_0.ShortName) : "nullitem");
			Class1311 setInHandsOperation = ((Item_0 != null) ? Player_0.method_137(Item_0) : null);
			Gparam_0 = Func_0();
			Bool_3 = false;
			Player_0.SpawnController(Gparam_0, delegate
			{
				Player_0.Logger.LogDebug("[Player.Process] CreateController, controller created, for item {0}, _confirmed {1}", (Item_0 != null) ? GClass2348.Localized(Item_0.ShortName) : "nullitem", Confirmation_0);
				Bool_3 = true;
				if (setInHandsOperation != null)
				{
					setInHandsOperation.Confirm();
				}
				switch (Confirmation_0)
				{
				default:
					throw new ArgumentException("Invalid enum");
				case Confirmation.Unknown:
					if (Completion_0 == Completion.Async)
					{
						Complete();
					}
					break;
				case Confirmation.Succeed:
					Complete();
					Player_0.Logger.LogInfo("SpawnController Confirmation.Succeed");
					ExecuteNext();
					break;
				case Confirmation.Failed:
					Abort();
					break;
				}
			});
		}

		public void method_1(bool succeed)
		{
			Player_0.Logger.LogInfo("Confirm succeed:" + succeed + "  _spawned:" + Bool_3);
			if (Confirmation_0 != Confirmation.Unknown)
			{
				UnityEngine.Debug.LogWarning("Invalid confirmation on process");
				return;
			}
			Confirmation_0 = (succeed ? Confirmation.Succeed : Confirmation.Failed);
			if (Gparam_0 == null || !Bool_3)
			{
				return;
			}
			if (succeed)
			{
				if (Completion_0 == Completion.Sync)
				{
					Complete();
				}
				Player_0.Logger.LogInfo("Confirm succeed");
				ExecuteNext();
			}
			else if (Completion_0 == Completion.Sync)
			{
				Abort();
			}
			else
			{
				AbortAfterCompletion();
			}
		}

		public override bool TrySkip()
		{
			if (!Bool_1 && Bool_0)
			{
				Skip("skipped _completed");
				return true;
			}
			return false;
		}

		public override void Skip(string error)
		{
			Begin(error);
			Complete(error);
		}

		public override void SkipToNext(string error)
		{
			Skip(error);
			if (Confirmation_0 == Confirmation.Unknown)
			{
				Confirmation_0 = Confirmation.Failed;
			}
			Player_0.Logger.LogInfo("SkipToNext error:" + error);
			ExecuteNext();
		}

		public override void Begin(string error = null)
		{
			Bool_0 = false;
			if (string.IsNullOrEmpty(error))
			{
				Callback_0.Succeed();
			}
			else
			{
				Callback_0.Fail(error);
			}
		}

		public override void Complete()
		{
			Bool_1 = true;
			Callback_1((TResult)Gparam_0);
		}

		public override void Complete([CanBeNull] string error)
		{
			Bool_1 = true;
			Callback_1(new Result<TResult>
			{
				Error = error
			});
		}

		public override void Abort()
		{
			UnityEngine.Debug.LogError("Operation aborted");
			IOnHandsUseCallback onHandsUseCallback = Gparam_0 as IOnHandsUseCallback;
			Callback<IOnHandsUseCallback> callback = onHandsUseCallback?.GetOnUsedCallback();
			Player_0.DestroyController();
			Player_0._processStatus = EProcessStatus.None;
			if (callback != null)
			{
				Complete(null);
				callback(new Result<IOnHandsUseCallback>(onHandsUseCallback));
			}
			else
			{
				Player_0.SetEmptyHands(delegate
				{
					Complete(null);
				});
			}
			Player_0.Logger.LogInfo("Abort()");
			ExecuteNext();
		}

		public override void AbortAfterCompletion()
		{
			Player_0.DestroyController();
			Player_0.Logger.LogInfo("AbortAfterCompletion()");
			ExecuteNext();
		}

		public override void ExecuteNext()
		{
			if (Player_0.ProcessStatus == EProcessStatus.Scheduled)
			{
				AbstractProcess abstractProcess_ = Player_0.AbstractProcess_0;
				if (abstractProcess_ != null)
				{
					Player_0.Logger.LogInfo("{0} executing process to Next {1}", this, abstractProcess_.GetType());
					Player_0.AbstractProcess_0 = null;
					AbstractProcess.Execute(abstractProcess_);
				}
				else
				{
					Player_0.Logger.LogInfo("{0} {1} setting process status from {2} to None", this, Item_0, Player_0.ProcessStatus);
					Player_0.ProcessStatus = EProcessStatus.None;
				}
			}
		}

		[CompilerGenerated]
		public void method_2(Result<GInterface198> callback)
		{
			Complete(null);
		}
	}

	public abstract class PlayerOwnerInventoryController : PlayerInventoryController
	{
		[CompilerGenerated]
		public class Class1316<T> where T : Item
		{
			public Predicate<T> predicate;

			public Predicate<GClass3248> goDeeperPredicate;

			public PlayerOwnerInventoryController playerOwnerInventoryController_0;

			public bool method_0(T item)
			{
				Predicate<T> obj = predicate;
				if (obj != null && !obj(item))
				{
					return false;
				}
				return !(item is GClass3367);
			}

			public bool method_1(GClass3248 container)
			{
				Predicate<GClass3248> obj = goDeeperPredicate;
				if (obj != null && !obj(container))
				{
					return false;
				}
				if (container is SearchableItemItemClass item)
				{
					return playerOwnerInventoryController_0.SearchController.IsSearched(item);
				}
				return true;
			}
		}

		public PlayerOwnerInventoryController(Player player, Profile profile, bool examined)
			: base(player, profile, examined)
		{
		}

		public override void GetAcceptableItemsNonAlloc<TItem>(EquipmentSlot[] equipmentSlots, IList<TItem> preAllocatedList, Predicate<TItem> predicate = null, Predicate<GClass3248> goDeeperPredicate = null)
		{
			base.GetAcceptableItemsNonAlloc(equipmentSlots, preAllocatedList, delegate(TItem item)
			{
				Predicate<TItem> predicate2 = predicate;
				return (predicate2 == null || predicate2(item)) && !(item is GClass3367);
			}, delegate(GClass3248 container)
			{
				Predicate<GClass3248> predicate2 = goDeeperPredicate;
				if (predicate2 != null && !predicate2(container))
				{
					return false;
				}
				return !(container is SearchableItemItemClass item) || SearchController.IsSearched(item);
			});
		}

		public override void ExamineMalfunction(Weapon weapon, bool clearRest = false)
		{
			if (!Player_0.IsAI && weapon.MalfState.IsKnownMalfunction(base.Profile.Id))
			{
				GameSetting<bool> malfunctionVisability = Singleton<SharedGameSettingsClass>.Instance.Game.Settings.MalfunctionVisability;
				bool num = MonoBehaviourSingleton<PreloaderUI>.Instance.MalfunctionGlow.ShowGlow(BattleUIMalfunctionGlow.EGlowType.Examined, force: false, malfunctionVisability ? method_41() : 0f);
				Player_0.NeedRepairMalfPhraseSituation(weapon.MalfState.State, HasKnownMalfType(weapon));
				if (num)
				{
					if (HasKnownMalfType(weapon))
					{
						NotificationManagerClass.DisplayNotification(new GClass2540(weapon.MalfState.State));
					}
					else
					{
						NotificationManagerClass.DisplayNotification(new GClass2541());
					}
				}
			}
			else
			{
				base.ExamineMalfunction(weapon, clearRest);
				Player_0.NeedRepairMalfPhraseSituation(weapon.MalfState.State, HasKnownMalfType(weapon));
				if (!Player_0.IsAI)
				{
					method_40(weapon, 150).HandleExceptions();
				}
			}
		}

		public async Task method_40(Weapon weapon, int delayInMilliseconds)
		{
			await Task.Delay(delayInMilliseconds);
			if ((bool)Singleton<SharedGameSettingsClass>.Instance.Game.Settings.MalfunctionVisability)
			{
				MonoBehaviourSingleton<PreloaderUI>.Instance.MalfunctionGlow.ShowGlow(BattleUIMalfunctionGlow.EGlowType.Examined, force: true, method_41());
				Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.MalfunctionExamined);
			}
			bool isKnown = weapon.MalfState.IsKnownMalfType(Player_0.ProfileId);
			Player_0.NeedRepairMalfPhraseSituation(weapon.MalfState.State, isKnown);
		}

		public override void ExamineMalfunctionType(Weapon weapon)
		{
			if (weapon.MalfState.IsKnownMalfType(base.Profile.Id))
			{
				NotificationManagerClass.DisplayNotification(new GClass2542(weapon.MalfState.State));
				return;
			}
			base.ExamineMalfunctionType(weapon);
			if (!Player_0.IsAI)
			{
				if ((bool)Singleton<SharedGameSettingsClass>.Instance.Game.Settings.MalfunctionVisability)
				{
					MonoBehaviourSingleton<PreloaderUI>.Instance.MalfunctionGlow.ShowGlow(BattleUIMalfunctionGlow.EGlowType.TypeExamined, force: true, method_41());
				}
				NotificationManagerClass.DisplayNotification(new GClass2542(weapon.MalfState.State));
			}
		}

		public override void CallUnknownMalfunctionStartRepair(Weapon weapon)
		{
			base.CallUnknownMalfunctionStartRepair(weapon);
			if (!Player_0.IsAI)
			{
				if ((bool)Singleton<SharedGameSettingsClass>.Instance.Game.Settings.MalfunctionVisability)
				{
					MonoBehaviourSingleton<PreloaderUI>.Instance.MalfunctionGlow.ShowGlow(BattleUIMalfunctionGlow.EGlowType.Repaired, force: true, method_41());
				}
				NotificationManagerClass.DisplayNotification(new GClass2542(weapon.MalfState.State));
			}
		}

		public override void CallMalfunctionRepaired(Weapon weapon)
		{
			base.CallMalfunctionRepaired(weapon);
			if (!Player_0.IsAI && (bool)Singleton<SharedGameSettingsClass>.Instance.Game.Settings.MalfunctionVisability)
			{
				MonoBehaviourSingleton<PreloaderUI>.Instance.MalfunctionGlow.ShowGlow(BattleUIMalfunctionGlow.EGlowType.Repaired, force: true, method_41());
			}
		}

		public override bool CheckedMagazine(MagazineItemClass magazine)
		{
			return base.Profile.IsCheckedMagazines(magazine.Id);
		}

		public float method_41()
		{
			float result = 0.5f;
			if (Player_0.HealthController.FindActiveEffect<GInterface356>() != null)
			{
				result = 1f;
			}
			return result;
		}

		public override BaseInventoryOperationClass ConvertOperationResultToOperation(IRaiseEvents operationResult)
		{
			if (operationResult is GClass3406 gClass)
			{
				return new ThrowOperationClass(method_12(), this, gClass, gClass.ItemsToDestroy, Player_0);
			}
			return base.ConvertOperationResultToOperation(operationResult);
		}

		public override GStruct156<bool> TryThrowItem(Item item, Callback callback = null, bool silent = false)
		{
			if (item.Owner is GInterface416 gInterface)
			{
				List<DestroyedItemsStruct> list = gInterface.GetItemsOverDiscardLimit(item).ToList();
				if (list.Any())
				{
					GClass1583 gClass = new GClass1583(item, list);
					if (!silent)
					{
						NotificationManagerClass.DisplayWarningNotification(gClass.GetLocalizedDescription());
					}
					return gClass;
				}
			}
			ThrowItem(item, downDirection: false, callback);
			return true;
		}
	}

	public class SinglePlayerInventoryController : PlayerOwnerInventoryController
	{
		[NonSerialized]
		[CompilerGenerated]
		public IPlayerSearchController IplayerSearchController_0;

		public override IPlayerSearchController PlayerSearchController
		{
			[CompilerGenerated]
			get
			{
				return IplayerSearchController_0;
			}
		}

		public SinglePlayerInventoryController(Player player, Profile profile, bool isBot = false, bool examined = false)
			: base(player, profile, examined)
		{
			IPlayerSearchController iplayerSearchController_;
			if (!isBot && !examined)
			{
				IPlayerSearchController playerSearchController = new PlayerSearchControllerClass(profile, this);
				iplayerSearchController_ = playerSearchController;
			}
			else
			{
				IPlayerSearchController playerSearchController = new BotSearchControllerClass(profile);
				iplayerSearchController_ = playerSearchController;
			}
			IplayerSearchController_0 = iplayerSearchController_;
		}

		public override void vmethod_1(BaseInventoryOperationClass operation, Callback callback)
		{
			method_42(operation, callback).HandleExceptions();
		}

		public async Task method_42(BaseInventoryOperationClass operation, [CanBeNull] Callback callback)
		{
			if (Player_0._healthController.IsAlive)
			{
				await Task.Yield();
			}
			base.vmethod_1(operation, callback);
		}

		public override SearchContentOperation vmethod_2(SearchableItemItemClass item)
		{
			return new SearchContentOperationResultClass(method_12(), this, PlayerSearchController, base.Profile, item);
		}

		public override void GetTraderServicesDataFromServer(string traderId)
		{
			((LocalPlayer)Player_0).UpdateTradersServiceData(traderId).HandleExceptions();
		}

		public override bool HasCultistAmulet(out CultistAmuletItemClass amulet)
		{
			amulet = null;
			if (Player_0.IsAI)
			{
				return false;
			}
			foreach (Item itemsInSlot in base.Inventory.GetItemsInSlots(new EquipmentSlot[1] { EquipmentSlot.Pockets }))
			{
				if (itemsInSlot is CultistAmuletItemClass cultistAmuletItemClass)
				{
					amulet = cultistAmuletItemClass;
					return true;
				}
			}
			return false;
		}

		public void TryExpendCultistAmuletChargeOrDestroy(bool destroy, bool onGameEnd)
		{
			if (!HasCultistAmulet(out var amulet) || !(!amulet.TryExpendCharge() || destroy))
			{
				return;
			}
			if (onGameEnd)
			{
				GStruct154<GClass3410> gStruct = InteractionsHandlerClass.RemoveWithoutRestrictions(amulet, this);
				if (gStruct.Failed)
				{
					UnityEngine.Debug.LogError(gStruct.Error);
				}
				return;
			}
			GStruct154<GClass3408> gStruct2 = InteractionsHandlerClass.Discard(amulet, this, simulate: true);
			if (gStruct2.Failed)
			{
				UnityEngine.Debug.LogError(gStruct2.Error);
				return;
			}
			RemoveOperationClass operation = new RemoveOperationClass(method_12(), this, gStruct2.Value);
			vmethod_1(operation, null);
		}

		[CompilerGenerated]
		[DebuggerHidden]
		public void method_43(BaseInventoryOperationClass operation, Callback callback)
		{
			base.vmethod_1(operation, callback);
		}
	}

	[Serializable]
	[CompilerGenerated]
	public class Class1318
	{
		public static readonly Class1318 class1318_0 = new Class1318();

		public static Callback<IHandsController> callback_0;

		public static Callback<IHandsController> callback_1;

		public static Func<TacticalComboVisualController, bool> func_0;

		public static Func<GripPose, bool> func_1;

		public static Func<GripPose, bool> func_2;

		public static Func<GripPose, bool> func_3;

		public static Func<BetterPropagationVolume, bool> func_4;

		public static Func<CompositeArmorComponent, IEnumerable<EBodyPartColliderType>> func_5;

		public static Func<NightVisionComponent, Action, Action> func_6;

		public static Func<ThermalVisionComponent, Action, Action> func_7;

		public static Func<FaceShieldComponent, Action, Action> func_8;

		public static Func<FaceShieldComponent, Action, Action> func_9;

		public static Func<CompositeArmorComponent, EDeafStrength> func_10;

		public static Func<EDeafStrength, int> func_11;

		public static Func<CompositeArmorComponent, EDeafStrength> func_12;

		public static Func<EDeafStrength, int> func_13;

		public static Action action_0;

		public static Func<Renderer, bool> func_14;

		public static Func<Slot, bool> func_15;

		public static Func<Slot, IEnumerable<Item>> func_16;

		public static Func<Item, bool> func_17;

		public static Func<Item, bool> func_18;

		public static Callback<GInterface198> callback_2;

		public static Callback<IFirearmHandsController> callback_3;

		public static Callback<IOnHandsUseCallback> callback_4;

		public void method_0(Result<IHandsController> result)
		{
		}

		public void method_1(Result<IHandsController> result)
		{
		}

		public bool method_2(TacticalComboVisualController x)
		{
			return x.LightMod.IsActive;
		}

		public bool method_3(GripPose x)
		{
			return x.Hand == GripPose.EHand.Left;
		}

		public bool method_4(GripPose x)
		{
			return x.GripType == GripPose.EGripType.UnderbarrelWeapon;
		}

		public bool method_5(GripPose x)
		{
			return x.Hand == GripPose.EHand.Right;
		}

		public bool method_6(BetterPropagationVolume x)
		{
			return x.MutuallyExclusive;
		}

		public IEnumerable<EBodyPartColliderType> method_7(CompositeArmorComponent x)
		{
			return x.ArmorColliders;
		}

		public Action method_8(NightVisionComponent nv, Action handler)
		{
			return nv.Togglable.OnChanged.Subscribe(handler);
		}

		public Action method_9(ThermalVisionComponent tv, Action handler)
		{
			return tv.Togglable.OnChanged.Subscribe(handler);
		}

		public Action method_10(FaceShieldComponent fs, Action handler)
		{
			Action togglableSub = fs.Togglable?.OnChanged.Subscribe(handler);
			Action hitSub = fs.HitsChanged.Subscribe(handler);
			return delegate
			{
				togglableSub?.Invoke();
				hitSub();
			};
		}

		public Action method_11(FaceShieldComponent fs, Action handler)
		{
			Action togglableSub = fs.Togglable?.OnChanged.Subscribe(handler);
			Action hitSub = fs.HitsChanged.Subscribe(handler);
			return delegate
			{
				togglableSub?.Invoke();
				hitSub();
			};
		}

		public EDeafStrength method_12(CompositeArmorComponent x)
		{
			return x.Deaf;
		}

		public int method_13(EDeafStrength d)
		{
			return (int)d;
		}

		public EDeafStrength method_14(CompositeArmorComponent x)
		{
			return x.Deaf;
		}

		public int method_15(EDeafStrength d)
		{
			return (int)d;
		}

		public void method_16()
		{
			Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.PlayerIsDead);
		}

		public bool method_17(Renderer x)
		{
			return x.enabled;
		}

		public bool method_18(Slot slot)
		{
			return slot.ContainedItem != null;
		}

		public IEnumerable<Item> method_19(Slot slot)
		{
			return GClass3380.GetAllItems(slot.ContainedItem);
		}

		public bool method_20(Item item)
		{
			return item.IsSecretExitRequirement;
		}

		public bool method_21(Item item)
		{
			return item.IsSecretExitRequirement;
		}

		public void method_22(Result<GInterface198> result)
		{
		}

		public void method_23(Result<IFirearmHandsController> result)
		{
		}

		public void method_24(Result<IOnHandsUseCallback> _)
		{
		}

		public float method_25()
		{
			return Time.deltaTime;
		}
	}

	[CompilerGenerated]
	public class Class1319
	{
		public Player player_0;

		public float armorDamage;

		public void method_0()
		{
			player_0.Skills.LightArmorDamageTakenAction.Complete(armorDamage);
		}

		public void method_1()
		{
			player_0.Skills.HeavyArmorDamageTakenAction.Complete(armorDamage);
		}
	}

	[CompilerGenerated]
	public class Class1320
	{
		public EBodyPartColliderType colliderType;

		public bool method_0(BodyPartCollider hitCollider)
		{
			return hitCollider.BodyPartColliderType.Equals(colliderType);
		}
	}

	[CompilerGenerated]
	public class Class1321
	{
		public LootableContainer container;

		public Player player_0;

		public void method_0()
		{
			container.Interact(new InteractionResult(EInteractionType.Close));
			if (player_0.MovementContext.LevelOnApproachStart > 0f)
			{
				player_0.MovementContext.SetPoseLevel(player_0.MovementContext.LevelOnApproachStart);
				player_0.MovementContext.LevelOnApproachStart = -1f;
			}
		}
	}

	[CompilerGenerated]
	public class Class1322
	{
		public Player player_0;

		public bool onCorpse;

		public void method_0()
		{
			player_0.Skills.FindAction.Complete(onCorpse);
		}
	}

	[CompilerGenerated]
	public class Class1323
	{
		public Player player_0;

		public int count;

		public void method_0()
		{
			player_0.Skills.RaidLoadedAmmoAction.Complete(count);
		}
	}

	[CompilerGenerated]
	public class Class1324
	{
		public Player player_0;

		public int count;

		public void method_0()
		{
			player_0.Skills.RaidUnloadedAmmoAction.Complete(count);
		}
	}

	[CompilerGenerated]
	public class Class1325
	{
		public Player player_0;

		public float diff;

		public void method_0()
		{
			player_0.Skills.HydrationChanged.Complete(diff, diff);
		}
	}

	[CompilerGenerated]
	public class Class1326
	{
		public Player player_0;

		public float damage;

		public void method_0()
		{
			player_0.Skills.DamageTakenAction.Complete(damage);
		}
	}

	[CompilerGenerated]
	public class Class1327
	{
		public Player player_0;

		public float diff;

		public void method_0()
		{
			player_0.Skills.EnergyChanged.Complete(diff, diff);
		}
	}

	[CompilerGenerated]
	public class Class1328
	{
		public float distance;

		public Player player_0;

		public void method_0()
		{
			player_0.Skills.SprintAction.Complete(new SkillManager.GStruct279
			{
				Overweight = player_0.Physical.Overweight,
				Fatigue = (player_0.Fatigue?.Strength ?? 0f)
			}, distance);
		}
	}

	[CompilerGenerated]
	public class Class1329
	{
		public float distance;

		public Player player_0;

		public void method_0()
		{
			player_0.Skills.MovementAction.Complete(new SkillManager.GStruct279
			{
				Noise = player_0.MovementContext.CovertNoiseLevel,
				Overweight = player_0.Physical.Overweight,
				Fatigue = (player_0.Fatigue?.Strength ?? 0f)
			}, distance);
		}
	}

	[CompilerGenerated]
	public class Class1330
	{
		public VoiceBroadcastTrigger broadcastTrigger;

		public void method_0(int value)
		{
			float volume = (float)value / 100f;
			broadcastTrigger.ActivationFader.Volume = volume;
		}
	}

	[CompilerGenerated]
	public class Class1331
	{
		public Player player_0;

		public AbstractHandsController controller;

		public Action callback;

		public TaskCompletionSource onControllerAppeared;

		public void method_0()
		{
			player_0.method_127(controller);
			callback?.Invoke();
			onControllerAppeared.SetResult(result: true);
		}
	}

	[CompilerGenerated]
	public class Class1332
	{
		public Callback callback;

		public void method_0(Result<IHandsController> result)
		{
			callback?.Invoke(result);
		}

		public void method_1(Result<IHandsController> result)
		{
			callback?.Invoke(result);
		}
	}

	[CompilerGenerated]
	public class Class1333
	{
		public Player player_0;

		public Callback<IHandsController> completeCallback;

		public Item method_0(EquipmentSlot x)
		{
			return player_0.InventoryController.Inventory.Equipment.GetSlot(x).ContainedItem;
		}

		public bool method_1(Item x)
		{
			return player_0.method_129(x);
		}

		public void method_2(Result<GInterface198> result)
		{
			completeCallback(result.Complete ? new Result<IHandsController>(result.Value) : new Result<IHandsController>(null, result.Error));
		}
	}

	[CompilerGenerated]
	public class Class1334
	{
		public Callback<IHandsController> callback;

		public void method_0(Result<GInterface198> result)
		{
			callback((!string.IsNullOrEmpty(result.Error)) ? new Result<IHandsController>(null, result.Error) : new Result<IHandsController>(result.Value));
		}
	}

	[CompilerGenerated]
	public class Class1335
	{
		public Player player_0;

		public Weapon weapon;

		public AIFirearmController method_0()
		{
			return FirearmController.smethod_6<AIFirearmController>(player_0, weapon);
		}

		public FirearmController method_1()
		{
			return FirearmController.smethod_6<FirearmController>(player_0, weapon);
		}
	}

	[CompilerGenerated]
	public class Class1336
	{
		public Player player_0;

		public ThrowWeapItemClass throwWeap;

		public GrenadeHandsController method_0()
		{
			return GrenadeHandsController.smethod_9<GrenadeHandsController>(player_0, throwWeap);
		}
	}

	[CompilerGenerated]
	public class Class1337
	{
		public Player player_0;

		public MedsItemClass meds;

		public GStruct382<EBodyPart> bodyParts;

		public int animationVariant;

		public MedsController method_0()
		{
			return MedsController.smethod_6<MedsController>(player_0, meds, bodyParts, 1f, animationVariant);
		}
	}

	[CompilerGenerated]
	public class Class1338
	{
		public Player player_0;

		public FoodDrinkItemClass foodDrink;

		public GStruct382<EBodyPart> bodyParts;

		public float amount;

		public int animationVariant;

		public MedsController method_0()
		{
			return MedsController.smethod_6<MedsController>(player_0, foodDrink, bodyParts, amount, animationVariant);
		}
	}

	[CompilerGenerated]
	public class Class1339
	{
		public Player player_0;

		public KnifeComponent knife;

		public KnifeController method_0()
		{
			return KnifeController.smethod_9<KnifeController>(player_0, knife);
		}
	}

	[CompilerGenerated]
	public class Class1340<T> where T : UsableItemController
	{
		public Player player_0;

		public Item item;

		public T method_0()
		{
			return UsableItemController.smethod_6<T>(player_0, item);
		}
	}

	[CompilerGenerated]
	public class Class1341
	{
		public Player player_0;

		public Item item;

		public QuickUseItemController method_0()
		{
			return QuickUseItemController.smethod_6<QuickUseItemController>(player_0, item);
		}
	}

	[CompilerGenerated]
	public class Class1342
	{
		public Player player_0;

		public ThrowWeapItemClass throwWeap;

		public QuickGrenadeThrowHandsController method_0()
		{
			return QuickGrenadeThrowHandsController.smethod_9<QuickGrenadeThrowHandsController>(player_0, throwWeap);
		}
	}

	[CompilerGenerated]
	public class Class1343
	{
		public Player player_0;

		public KnifeComponent knife;

		public QuickKnifeKickController method_0()
		{
			return QuickKnifeKickController.smethod_9<QuickKnifeKickController>(player_0, knife);
		}
	}

	[CompilerGenerated]
	public class Class1344
	{
		public Callback<IHandsController> completeCallback;

		public Player player_0;

		public void method_0()
		{
			completeCallback?.Invoke(new Result<IHandsController>
			{
				Error = "can't find item controller"
			});
		}

		public void method_1(Result<IHandsThrowController> result)
		{
			smethod_1(result, completeCallback);
		}

		public void method_2(Result<GInterface203> result)
		{
			smethod_1(result, completeCallback);
		}

		public void method_3(Result<GInterface203> result)
		{
			smethod_1(result, completeCallback);
		}

		public void method_4(Result<IKnifeController> result)
		{
			smethod_1(result, completeCallback);
		}

		public void method_5(Result<IOnHandsUseCallback> result)
		{
			smethod_1(result, completeCallback);
		}
	}

	[CompilerGenerated]
	public class Class1345
	{
		public Weapon _003Cweapon_003E5__2;

		public Class1344 class1344_0;

		public void method_0(Result<IFirearmHandsController> result)
		{
			smethod_1(result, class1344_0.completeCallback);
			if (result.Complete && !_003Cweapon_003E5__2.IsOneOff)
			{
				class1344_0.player_0.LastEquippedWeaponOrKnifeItem = _003Cweapon_003E5__2;
			}
		}
	}

	[CompilerGenerated]
	public class Class1346
	{
		public Callback<IHandsController> completeCallback;

		public void method_0(Result<GInterface202> result)
		{
			smethod_1(result, completeCallback);
		}

		public void method_1(Result<GInterface202> result)
		{
			smethod_1(result, completeCallback);
		}
	}

	[CompilerGenerated]
	public class Class1347
	{
		public Callback<IHandsController> completeCallback;

		public void method_0(Result<GInterface202> result)
		{
			smethod_1(result, completeCallback);
		}

		public void method_1(Result<GInterface202> result)
		{
			smethod_1(result, completeCallback);
		}
	}

	[CompilerGenerated]
	public class Class1348
	{
		public Player player_0;

		public Callback callback;

		public void method_0(IResult result)
		{
			if ((object)player_0._removeFromHandsCallback == callback)
			{
				player_0._removeFromHandsCallback = null;
			}
			player_0.InventoryController.RaiseInOutProcessEvents(new GEventArgs17(player_0.HandsController.Item, CommandStatus.Succeed, player_0.InventoryController));
			callback(result);
		}
	}

	[CompilerGenerated]
	public class Class1349
	{
		public Player player_0;

		public Callback callback;

		public Item method_0(EquipmentSlot x)
		{
			return player_0.InventoryController.Inventory.Equipment.GetSlot(x).ContainedItem;
		}

		public bool method_1(Item x)
		{
			return player_0.method_129(x);
		}

		public void method_2(Result<IFirearmHandsController> result)
		{
			if ((object)player_0._removeFromHandsCallback == callback)
			{
				player_0._removeFromHandsCallback = null;
			}
			callback.Invoke(result);
		}

		public void method_3(IResult result)
		{
			if ((object)player_0._removeFromHandsCallback == callback)
			{
				player_0._removeFromHandsCallback = null;
			}
			callback(result);
		}
	}

	[CompilerGenerated]
	public class Class1350
	{
		public Player player_0;

		public Callback callback;

		public void method_0(Result<IHandsController> result)
		{
			if ((object)player_0._setInHandsCallback == callback)
			{
				player_0._setInHandsCallback = null;
			}
			callback.Invoke(result);
		}

		public void method_1(IResult error)
		{
			if ((object)player_0._setInHandsCallback == callback)
			{
				player_0._setInHandsCallback = null;
			}
			player_0.InventoryController.RaiseInOutProcessEvents(new GEventArgs17(player_0.HandsController.Item, CommandStatus.Succeed, player_0.InventoryController));
			callback(error);
		}
	}

	[CompilerGenerated]
	public class Class1351
	{
		public Player player_0;

		public IWeapon weapon;

		public void method_0()
		{
			player_0.Skills.RecoilAction.Complete(weapon.RecoilBase);
		}
	}

	[CompilerGenerated]
	public class Class1352
	{
		public GripPose.EGripType type;

		public Player player_0;

		public bool method_0(GripPose x)
		{
			if (x.GripType != type)
			{
				return x.GripType == GripPose.EGripType.UnderbarrelWeapon;
			}
			return true;
		}

		public int method_1(GripPose x)
		{
			return HandPoser.NumParents(x.transform, player_0.PlayerBones.WeaponRoot.Original);
		}

		public int method_2(GripPose x)
		{
			return HandPoser.NumParents(x.transform, player_0.PlayerBones.WeaponRoot.Original);
		}
	}

	[CompilerGenerated]
	public class Class1353
	{
		public Player player_0;

		public EPointOfView pointOfView;

		public void method_0(bool _)
		{
			player_0.PossibleInteractionsChanged?.Invoke();
		}

		public void method_1(EPlayerState prevState, EPlayerState nextState)
		{
			player_0.ProceduralWeaponAnimation.WalkEffectorEnabled = nextState == EPlayerState.Run;
			player_0.ProceduralWeaponAnimation.DrawEffectorEnabled = nextState != EPlayerState.ProneMove;
			player_0.ProceduralWeaponAnimation.TiltBlender.Target = ((nextState == EPlayerState.Idle || nextState == EPlayerState.ProneIdle) ? 1 : 0);
			if (prevState == EPlayerState.Stationary)
			{
				player_0.ProceduralWeaponAnimation.SetStrategy(pointOfView);
			}
		}
	}

	[CompilerGenerated]
	public class Class1354
	{
		public Action togglableSub;

		public Action hitSub;

		public void method_0()
		{
			togglableSub?.Invoke();
			hitSub();
		}
	}

	[CompilerGenerated]
	public class Class1355
	{
		public Action togglableSub;

		public Action hitSub;

		public void method_0()
		{
			togglableSub?.Invoke();
			hitSub();
		}
	}

	[CompilerGenerated]
	public class Class1356
	{
		public Player player_0;

		public IEffect effect;

		public void method_0()
		{
			player_0.Skills.LowHPDuration.Begin();
		}

		public void method_1()
		{
			player_0.Skills.HealthNegativeEffect.Complete(effect);
		}
	}

	[CompilerGenerated]
	public class Class1357
	{
		public Slot[] headSlots;

		public Slot[] armorSlots;

		public bool method_0(ItemAddress loc)
		{
			return headSlots.Contains(loc.Container);
		}

		public bool method_1(ItemAddress loc)
		{
			return armorSlots.Contains(loc.Container);
		}
	}

	public ICharacterController _characterController;

	protected TriggerColliderSearcher _triggerColliderSearcher;

	private bool _doorKick;

	private WorldInteractiveObject _currentInteractor;

	private float _horizontal;

	private float _vertical;

	private bool _resetLook;

	private bool _mouseLookControl;

	private bool _isResettingLook;

	private bool _setResetedLookNextFrame;

	private bool _isLooking;

	public PedometerClass Pedometer;

	public Vector3 HeadRotation;

	protected float _mouseSensitivityModifier;

	protected readonly Dictionary<EMouseSensitivityModifier, float> _mouseSensitivityModifiers = GClass866<EMouseSensitivityModifier>.GetDictWith<float>();

	private Vector2 _rotationPitchLimit = PlayerMovementConstantsClass.STAND_POSE_ROTATION_PITCH_RANGE;

	private Vector2 _targetRotationPitch = PlayerMovementConstantsClass.STAND_POSE_ROTATION_PITCH_RANGE;

	public float TrunkRotationLimit;

	public float PoseMemo = 1f;

	private float _speedMemo = 0.5f;

	private bool _lastSlowLean;

	public LeanType CurrentLeanType;

	private float _lastMovement;

	private bool _cachedMouseLookControl;

	private bool _isVaultingPressed;

	private float _vaultingTiming;

	protected float _prevHeight;

	public float HeightSmoothTime = 0.066f;

	private float _dampVelocity;

	private float _currentSmoothSpeed;

	private float _previousY;

	private const float ClampDeltaHeight = 0.2f;

	public const int GRIP_CULL_DISTANCE = 40;

	public const int IK_CULL_DISTANCE = 70;

	public const int MAX_IK_CULL_DISTANCE = 300;

	private const string COMPASS_RESOURCE_PATH = "assets/content/weapons/additional_hands/item_compass.bundle";

	private const string PLANT_TRIPWIRE_TEMPLATE_PATH = "Prefabs/tripwire_planner";

	public GripPose LeftHandInteractionTarget;

	public GrounderFBBIK Grounder;

	public HitReaction HitReaction;

	public float RibcageScaleCurrent = 1f;

	public float RibcageScaleCurrentTarget = 1f;

	public Transform[] _elbowBends;

	public HandPoser[] HandPosers;

	public Vector2 UtilityLayerRange = new Vector2(0.5f, 0.2f);

	public float UtilityLayerLerpSpeed = 3f;

	public ValueBlender LMarkerRawBlender = new ValueBlender
	{
		Speed = 4f,
		Target = 0f
	};

	public ValueBlender LayerWeight = new ValueBlender
	{
		Speed = 4f
	};

	public readonly BetterValueBlender ThirdIkWeight = new BetterValueBlender
	{
		Speed = 3f
	};

	public bool GripAutoAdjust;

	public bool CustomAnimationsAreProcessing;

	protected FullBodyBipedIK _fbbik;

	protected PlayerBody _playerBody;

	protected float ThirdPersonWeaponRootAuthority;

	public const float HAND_ANIMATION_BLEND_THRESHOLD = 0.1f;

	private float _ribcageScaleCompensated = 1f;

	private float _shoulderVel;

	private float _fbbikCooldown = 0.6f;

	private float _turnOffFbbikAt;

	private float _firstPersonRightHand;

	private float _firstPersonLeftHand;

	private float _utilityLayerWeight;

	private float _smoothLW;

	private float _rawWeight;

	private float _rawDampVelocity;

	private float _interactionLayerWeight;

	private bool _stored;

	private bool _pointOfViewUndecided = true;

	private bool _hasAnimatorPropBones;

	private bool _hasProp;

	private bool _propActive;

	private bool _compassInstantiated;

	private bool _radioTransmitterInstantiated;

	private Vector3[] _ribcageChildPositions;

	private Vector3 _ikPosition;

	private Vector3 _lMarkerRawPosition;

	private Vector3 _lElbowRawPosition;

	private Vector3 _rElbowRawPosition;

	private Vector3 _propRawPosition;

	private Quaternion _lMarkerRawRotation;

	private Quaternion _propRawRotation;

	private Quaternion _ikRotation;

	private Quaternion[] _ribcageChildRotations;

	private readonly Transform[] _markers = new Transform[2];

	private readonly Transform[] _gripReferences = new Transform[2];

	private Transform[] _ikTargets = new Transform[2];

	private Transform _vestMarker;

	private Transform _shoulderEffector;

	private Transform _propBone;

	private TwistRelax[] _twistBones;

	private LimbIK[] _limbs;

	private GameObject _beaconDummy;

	private Action _createBeaconAction;

	private PreviewMaterialSetter _beaconMaterialSetter;

	private BeaconPlacer _beaconPlacer;

	private FirearmsEffects _thirdWeaponEffects;

	private FirearmsEffects _firstWeaponEffects;

	private CompassArrow _compassArrow;

	private RadioTransmitterView _radioTransmitterView;

	private Transform[] _animatorPropTransforms = new Transform[3];

	private Transform[] _propTransforms = new Transform[3];

	private readonly List<BodyRendererDataStruct> _preAllocatedRenderersList = new List<BodyRendererDataStruct>(10);

	public readonly ValueBlender AuthorityBlender = new ValueBlender
	{
		Speed = 4f,
		Target = 0f
	};

	public readonly ValueBlender GrounderBlender = new ValueBlender
	{
		Speed = 4f,
		Target = 0f
	};

	private float _ergonomicsPenalty;

	private float _shotTime;

	protected bool _isDeadAlready;

	private bool _isGrenadeOrKnife;

	private ObjectInHandsAnimator _handsAnimator;

	private GameObject _spawnedKey;

	private Action _cacheBonesDelegate;

	protected bool IsHeadLightsAnimationActive;

	private bool _isInteractionPlayeingLastFrame;

	private Quaternion _currentHandsRotation;

	private GClass2004 _garbage;

	public BaseBallistic.ESurfaceSound CurrentSurface;

	private LayerMask _stepLayerMask;

	private const float TIME_BETWEEN_PRONE_SWEEPS = 0.5f;

	private const float MIN_ALLOWED_MOVEMENT_SPEED = 0f;

	private const float MAX_STEP_SOUND_SPEED_FACTOR = 1f;

	private const float SURFACE_CHECK_RAYCAST_OFFSET = 0.5f;

	private const float LANDING_VOLUME_MULT = 2.5f;

	private const float FP_GEAR_VOLUME = 0.85f;

	private const float LOCAL_AI_GEAR_VOLUME = 1f;

	private float CHECK_RANGE_BUFF = 1f;

	private float MIN_FALL_DAMAGE = 1f;

	private const float MIN_COMMON_DAMAGE = 4f;

	private const float FIRST_PERSON_REVERB_FACTOR = 3f;

	private const float DEFAULT_FP_ROLLOFF = 70f;

	public bool HeavyBreath;

	public bool Muffled;

	protected BetterSource NestedStepSoundSource;

	protected BetterSource _speechSource;

	protected bool OcclusionDirty;

	protected bool DistanceDirty;

	protected AudioClip FractureSound;

	public BaseSoundPlayer.SoundElement PropIn;

	public BaseSoundPlayer.SoundElement PropOut;

	protected bool PreviousFaceShield;

	protected bool PreviousNightVision;

	protected bool PreviousThermalVision;

	protected readonly Vector3 SpeechLocalPosition = new Vector3(0f, 1.2f, 0f);

	private readonly Vector3 _speechLocalPosition = new Vector3(0f, 0f, 0.3f);

	protected AudioClip FaceshieldOn;

	protected AudioClip FaceshieldOff;

	protected AudioClip NightVisionOn;

	protected AudioClip SwitchHeadlights;

	protected AudioClip NightVisionOff;

	protected AudioClip ThermalVisionOn;

	protected AudioClip ThermalVisionOff;

	private AudioClip _tinnitus;

	private Dictionary<BaseBallistic.ESurfaceSound, SurfaceSet> _soundBySurface;

	private SurfaceSet _currentSet;

	private SoundBank _gearSoundBank;

	private SoundBank _gearMediumSoundBank;

	private SoundBank _gearFastSoundBank;

	private SoundBank _backpackDropBank;

	private Sounds _playerSounds;

	private FirstPersonPlayerHearingSettings _hearingSettings;

	private Coroutine _idleCoroutine;

	private Coroutine _runCoroutine;

	private Coroutine _sprintCoroutine;

	private Coroutine _gearDelay;

	private Coroutine _outOfRangeSpeakingCoroutine;

	private Coroutine _currentSourceCoroutine;

	private bool _playedAtLeastOneStep;

	private float _nextJumpAfter;

	private BetterSource _searchSource;

	private float _searchCount;

	private AudioClip _lastClip;

	private readonly List<BetterPropagationVolume> _soundPropagationVolumes = new List<BetterPropagationVolume>();

	private readonly List<BetterPropagationVolume> _volumesBuffer = new List<BetterPropagationVolume>();

	private BetterPropagationVolume _mutuallyExclusive;

	private bool _exhaustionIsAudible;

	private Action _exhaustionAudibilityUnsub;

	private float _sprintSurfaceCheck = 60f;

	private float _runSurfaceCheck = 40f;

	private float _landSurfaceCheck = 40f;

	private float _proneSurfaceCheck = 30f;

	private float _sign;

	private float _lastStepTime;

	private float _lastTimeTurnSound;

	private float maxLengthTurnSound = 0.6f;

	private float _nextSurfaceCheck;

	private float _distance;

	private bool _enqueuedForRelease;

	private float _maxAllowedMovementSpeed;

	private GClass2681 _vaultAudioController;

	private GClass2681 _sprintVaultAudioController;

	private GClass2681 _climbAudioController;

	private AudioSource _voipAudioSource;

	private GInterface95 _specificStepAudioController = new GClass1183();

	private Action _voipSourceBinding;

	private BetterSource _gearSource;

	private GInterface268 _tripwireInteractionSoundController;

	private GClass885 _sourcePrewarmer;

	private IDropBackPackEvents _dropBackPackEvents;

	private GClass1180 _priorityCalculator;

	private readonly WaitForSeconds _gearWalkDelaySec = new WaitForSeconds(EFTHardSettings.Instance.GEAR_SOUND_DELAY);

	private int _animatorFootstepCurveHash;

	private readonly Dictionary<EAudioMovementState, float> _cachedMovementRolloff = new Dictionary<EAudioMovementState, float>();

	private GClass1096.GClass1104 _playerAudioSettings = new GClass1096.GClass1104();

	private BetterSource _interactionSource;

	private bool _useSimpleUnderRoofCheck;

	private readonly GClass1502 _damageThresholdAudioChecker = new GClass1502();

	private Action _soundUnsubscribeOnDestroy;

	public const EAnimatorMask EnabledAnimatorsPlayerDefault = EAnimatorMask.Thirdperson | EAnimatorMask.Arms | EAnimatorMask.Procedural | EAnimatorMask.FBBIK | EAnimatorMask.IK;

	private const EAnimatorMask FastAnimatorMask = EAnimatorMask.Thirdperson | EAnimatorMask.Arms | EAnimatorMask.FBBIK | EAnimatorMask.IK;

	public const EAnimatorMask EnabledAnimatorsSpiritDefault = EAnimatorMask.Thirdperson | EAnimatorMask.Arms;

	private const int SPRINT_DAMAGE = 2;

	private const int JUMP_DAMAGE = 3;

	public const string LAYER_NAME_PLAYER = "Player";

	public PlayerOverlapManager POM;

	public readonly List<string> TriggerZones = new List<string>();

	[NonSerialized]
	public BindableEvent OnExitTriggerVisited = new BindableEvent();

	public global::BindableStateClass<bool> InteractingWithExfiltrationPoint = new global::BindableStateClass<bool>();

	public EDamageType LastDamageType;

	public EBodyPart LastDamagedBodyPart;

	public bool Destroyed;

	public bool QuickdrawWeaponFast;

	public bool FastSlotSelection;

	public bool PreviousWeaponAimState;

	public float QuickdrawTime;

	public bool CanManipulateWithHandsInBufferZone;

	public IAnimator[] _animators;

	public IAnimator _underbarrelFastAnimator;

	public PhraseSpeakerClass Speaker;

	public PlayerSpirit Spirit;

	[GAttribute10(typeof(EAnimatorMask))]
	public EAnimatorMask EnabledAnimators = EAnimatorMask.Thirdperson | EAnimatorMask.Arms | EAnimatorMask.Procedural | EAnimatorMask.FBBIK | EAnimatorMask.IK;

	protected GClass724 Logger;

	protected Corpse Corpse;

	protected IPlayer LastAggressor;

	protected DamageInfoStruct LastDamageInfo;

	protected EBodyPart LastBodyPart;

	protected float _corpseAppliedForce;

	protected Func<float> GetSensitivity;

	protected Func<float> GetAimingSensitivity;

	protected Action<Action> _openAction;

	protected RecodableItemsHandler recodableItemsHandler;

	private float _countdownToSprintDamage = 1f;

	private float _lastHitTime;

	private int _lastHitDebuffFrame;

	private float _accumulatedDebuffDamage;

	private int _negativeBuffsCount;

	private bool _sense;

	private bool _isInventoryOpened;

	private bool _displaySense;

	private IEffect Fatigue;

	private Renderer[] _renderers = Array.Empty<Renderer>();

	private Camera _camera;

	private Coroutine _selfDamage;

	private readonly global::BindableStateClass<Item> _itemInHands = new global::BindableStateClass<Item>();

	protected readonly CompositeDisposableClass CompositeDisposable = new CompositeDisposableClass();

	private GClass3727 _heavyVestsDeflectRandoms;

	private Action _unsubscribeOnEndSession;

	protected IEnumerable<TacticalComboVisualController> _helmetLightControllers = new List<TacticalComboVisualController>();

	private Animator _createdAnimator;

	private RuntimeAnimatorController _createdRuntimeAnimatorController;

	private IVaultingComponent _vaultingComponent;

	private IVaultingComponentDebug _vaultingComponentDebug;

	private IVaultingParameters _vaultingParameters;

	private IVaultingGameplayRestrictions _vaultingGameplayRestrictions;

	protected ILeftHandController _leftHandController;

	private IWeaponMountingComponent _weaponMountingComponent;

	private float _currentBlindnessProtection;

	public Action<Item, AmmoItemClass> OnStatisticsShot;

	public BasePhysicalClass Physical;

	[SerializeField]
	private EUpdateQueue _updateQueue;

	[SerializeField]
	protected EUpdateQueue _armsUpdateQueue;

	[SerializeField]
	protected EUpdateMode _armsUpdateMode;

	[SerializeField]
	protected EUpdateMode _bodyUpdateMode;

	protected IHealthController _healthController;

	protected BodyPartCollider[] _hitColliders;

	protected ArmorPlateCollider[] _armorPlateColliders;

	protected PlayerInventoryController _inventoryController;

	protected AbstractHandsController _handsController;

	protected AbstractQuestControllerClass _questController;

	protected AbstractAchievementControllerClass _achievementsController;

	protected AbstractPrestigeControllerClass _prestigeController;

	protected GClass3617 _dialogController;

	public const string ARTA_MAN_PROFILE_ID = "66f3fad50ec64d74847d049d";

	public const string ARTA_MAN_NAME = "UI/Artillery/ArtaManName";

	private string _fullIdInfo;

	public Transform Tracking;

	private float _awareness;

	protected bool _armsupdated;

	protected float _armsTime;

	protected bool _bodyupdated;

	protected float _bodyTime;

	protected int _nFixedFrames;

	protected float _fixedTime;

	private static readonly GDelegate66 _defaultDeltaTimeDelegate = () => Time.deltaTime;

	private GDelegate66 _deltaTimeDelegate = _defaultDeltaTimeDelegate;

	private WaitForFixedUpdate _waitForFixedUpdate = new WaitForFixedUpdate();

	protected float LastDeltaTime;

	protected Transform _playerLookRaycastTransform;

	private EDoorState _lastInteractionState;

	private bool _nextCastHasForceEvent;

	private float _lastStateUpdateTime;

	private Coroutine _waitInventoryCoroutine;

	protected readonly List<ArmorComponent> _preAllocatedArmorComponents = new List<ArmorComponent>(20);

	protected EquipmentPenaltyComponent _preAllocatedBackpackPenaltyComponent;

	private bool _gameSessionEndWasCalled;

	protected Action ExfilUnsubscribe;

	protected List<Action> SessionEndUnsubscribe;

	protected bool AggressorFound;

	[SerializeField]
	public float MyHandsToBodyAngle;

	public byte MovementIteration;

	public List<SecretExfiltrationPoint> FoundSecretExits = new List<SecretExfiltrationPoint>();

	protected static readonly TimeSpan HearingDetectionTime = TimeSpan.FromSeconds(2.0);

	private GInterface211 _customHandRotator;

	private EPlayerBtrState _btrState;

	private EPlayerBtrState _lastBtrStateInteractionCheck;

	private EBtrState _lastBtrStateCheck;

	private bool _lastBtrCastResult;

	private bool _lastTripwireCastResult;

	private bool _lastEventObjectCastResult;

	private bool _isUsingLeftHand;

	private EProcessStatus _processStatus;

	private Item _lastEquippedWeaponOrKnifeItem;

	private readonly EquipmentSlot[] _slotPriority = new EquipmentSlot[4]
	{
		EquipmentSlot.FirstPrimaryWeapon,
		EquipmentSlot.SecondPrimaryWeapon,
		EquipmentSlot.Holster,
		EquipmentSlot.Scabbard
	};

	protected Callback _removeFromHandsCallback;

	private Callback _setInHandsCallback;

	private float _lastFaceshieldOperationTime;

	private int _faceshieldNumOperations;

	private const int MAX_FACESHIELD_OPERATIONS_PER_FRAME = 3;

	public IAnimator BodyAnimatorCommon => GetBodyAnimatorCommon();

	public IAnimator ArmsAnimatorCommon => GetArmsAnimatorCommon();

	public IAnimator UnderbarrelWeaponArmsAnimator => _underbarrelFastAnimator;

	public ICharacterController CharacterController => GetCharacterControllerCommon();

	public MovementContext MovementContext { get; set; }

	public bool IsResettingLook => _isResettingLook;

	public bool IsLooking => _isLooking;

	public bool MouseLookControl => _mouseLookControl;

	public EPlayerPose Pose
	{
		get
		{
			if (!IsInPronePose)
			{
				if (!(MovementContext.SmoothedPoseLevel < 0.11f))
				{
					return EPlayerPose.Stand;
				}
				return EPlayerPose.Duck;
			}
			return EPlayerPose.Prone;
		}
	}

	public float PoseLevel => MovementContext.PoseLevel;

	public float Speed => MovementContext.CharacterMovementSpeed;

	public Vector2 Rotation
	{
		get
		{
			return MovementContext.Rotation;
		}
		set
		{
			MovementContext.Rotation = value;
		}
	}

	public float Yaw => MovementContext.Yaw;

	public bool IsInPronePose => MovementContext.IsInPronePose;

	public float Pitch => MovementContext.Pitch;

	public Vector3 Velocity => MovementContext.Velocity;

	public Vector3 Motion => MovementContext.InputMotion;

	public Vector2 RotationPitchLimit
	{
		get
		{
			return _rotationPitchLimit;
		}
		set
		{
			_targetRotationPitch = value;
		}
	}

	public Vector2 InputDirection { get; set; }

	public BaseMovementState CurrentState => MovementContext.CurrentState;

	public MovementState CurrentManagedState => (MovementContext.OverridenControlsState ?? MovementContext.CurrentState) as MovementState;

	public int CurrentAnimatorStateIndex => MovementContext.CurrentAnimatorStateIndex;

	public Vector3 Position
	{
		get
		{
			return PlayerBones.BodyTransform.position;
		}
		set
		{
			PlayerBones.BodyTransform.position = value;
		}
	}

	public bool IsForwardInputDirection => InputDirection.y > 0f;

	public bool IsSprintEnabled => MovementContext.IsSprintEnabled;

	public EFTHardSettings EFTHardSettings_0 => EFTHardSettings.Instance;

	public float MovementIdlingTime => Time.time - _lastMovement;

	public bool IsVaultingPressed => _isVaultingPressed;

	public virtual bool OnHisWayToOperateStationaryWeapon => false;

	public bool OnScreen => _playerBody.IsVisible();

	public PlayerBody PlayerBody => _playerBody;

	public float HandsToBodyAngle => MovementContext.HandsToBodyAngle;

	public Func<int> CompassValue
	{
		get
		{
			if (_compassArrow != null)
			{
				return _compassArrow.PanelValue;
			}
			return null;
		}
	}

	public bool HasGamePlayerOwner { get; set; }

	public virtual EPointOfView PointOfView
	{
		get
		{
			return _playerBody.PointOfView.Value;
		}
		set
		{
			if (_playerBody.PointOfView.Value != value || _pointOfViewUndecided)
			{
				_pointOfViewUndecided = false;
				_playerBody.PointOfView.Value = value;
				CalculateScaleValueByFov((int)Singleton<SharedGameSettingsClass>.Instance.Game.Settings.FieldOfView);
				SetCompensationScale();
				if (value == EPointOfView.ThirdPerson)
				{
					PlayerBones.Ribcage.Original.localScale = new Vector3(1f, 1f, 1f);
				}
				MovementContext.PlayerAnimatorPointOfView(value);
				PointOfViewChanged?.Invoke();
				_playerBody.UpdatePlayerRenders(_playerBody.PointOfView.Value, Side);
				ProceduralWeaponAnimation.PointOfView = value;
			}
		}
	}

	public TripwireVisualPlacer TripwireVisualPlacer_0 { get; set; }

	public bool FirstPersonPointOfView => GClass2078.IsFirstPerson(PointOfView);

	public bool UsedSimplifiedSkeleton { get; set; }

	public BindableEvent PointOfViewChanged { get; } = new BindableEvent();

	public Vector3 BeaconPosition { get; set; }

	public Quaternion BeaconRotation { get; set; }

	public float ErgonomicsPenalty => _ergonomicsPenalty;

	public ObjectInHandsAnimator HandsAnimator
	{
		get
		{
			return _handsAnimator;
		}
		set
		{
			_handsAnimator = value;
		}
	}

	public GameObject CameraContainer { get; set; }

	public Transform CameraPosition { get; set; }

	public ProceduralWeaponAnimation ProceduralWeaponAnimation { get; set; }

	public bool AllowToPlantBeacon { get; set; }

	public Vector3 LookDirection => MovementContext.LookDirection;

	public BifacialTransform WeaponRoot => PlayerBones.WeaponRoot;

	public BifacialTransform Fireport => PlayerBones.Fireport;

	public BifacialTransform[] MultiBarrelFireports => PlayerBones.MultiBarrelsFireports;

	public Quaternion TargetHandsRotation => Quaternion.Euler(MovementContext.Pitch, MovementContext.Yaw, 0f);

	public Quaternion CurrentHandsRotaion
	{
		get
		{
			if ((FirstPersonPointOfView ? method_25(PlayerAnimator.FIRST_PERSON_CURVE_WEIGHT) : 1f) == 0f)
			{
				_currentHandsRotation = Quaternion.Euler(Mathf.LerpAngle(_currentHandsRotation.eulerAngles.x, TargetHandsRotation.eulerAngles.x, 0.9f), Mathf.LerpAngle(_currentHandsRotation.eulerAngles.y, TargetHandsRotation.eulerAngles.y, 0.9f), 0f);
			}
			else
			{
				_currentHandsRotation = Quaternion.Euler(Mathf.LerpAngle(_currentHandsRotation.eulerAngles.x, TargetHandsRotation.eulerAngles.x, 0.3f), Mathf.LerpAngle(_currentHandsRotation.eulerAngles.y, TargetHandsRotation.eulerAngles.y, 0.3f), 0f);
			}
			return _currentHandsRotation;
		}
	}

	public Quaternion HandsRotation
	{
		get
		{
			if (!_customHandRotator.IsValid)
			{
				return Quaternion.Euler(MovementContext.Pitch, MovementContext.Yaw, 0f);
			}
			return _customHandRotator.GetRotation();
		}
	}

	public EnvironmentType Environment { get; set; }

	public virtual float LandingThreshold => 0.3f;

	public float Single_0 => Mathf.Sign(BodyAnimatorCommon.GetFloat(_animatorFootstepCurveHash));

	public AudioSource VoipAudioSource
	{
		get
		{
			return _voipAudioSource;
		}
		set
		{
			_voipAudioSource = value;
			_voipSourceBinding?.Invoke();
			_voipSourceBinding = null;
			if (_voipAudioSource == null)
			{
				return;
			}
			SoundSettingsControllerClass settings = Singleton<SharedGameSettingsClass>.Instance.Sound.Settings;
			_voipSourceBinding = settings.VoipEnabled.Bind(delegate(bool enable)
			{
				if (_voipAudioSource != null)
				{
					_voipAudioSource.mute = !enable;
				}
			});
		}
	}

	public virtual float MINStepSoundSpeedFactor => 0.2f;

	public ETagStatus Fraction
	{
		get
		{
			if (Profile.Info.Side != EPlayerSide.Bear)
			{
				if (Profile.Info.Side != EPlayerSide.Usec)
				{
					return ETagStatus.Scav;
				}
				return ETagStatus.Usec;
			}
			return ETagStatus.Bear;
		}
	}

	public float SinceLastStep => Time.time - _lastStepTime;

	public virtual float ProtagonistHearing => 1f;

	public virtual float Distance
	{
		get
		{
			if (DistanceDirty)
			{
				_distance = (GClass2078.IsFirstPerson(PointOfView) ? 0f : CameraClass.Instance.Distance(Transform.position));
				DistanceDirty = false;
			}
			return _distance;
		}
	}

	public BetterSource SpeechSource
	{
		get
		{
			if (_speechSource == null)
			{
				CreateSpeechSource();
			}
			return _speechSource;
		}
		set
		{
			_speechSource = value;
		}
	}

	public bool IsUnderRoof
	{
		get
		{
			if (!_useSimpleUnderRoofCheck)
			{
				return !RainController.IsCameraUnderRain;
			}
			return Environment == EnvironmentType.Indoor;
		}
	}

	public float Single_1 => 1f - (float)Skills.BotSoundCoef;

	public GameWorld GameWorld { get; set; }

	public bool IsInBufferZone { get; set; }

	public GenericEventTranslator EventTranslator { get; set; }

	public IVaultingComponent VaultingComponent => _vaultingComponent;

	public IVaultingComponentDebug VaultingComponentDebug => _vaultingComponentDebug;

	public IVaultingParameters VaultingParameters => _vaultingParameters;

	public IVaultingGameplayRestrictions VaultingGameplayRestrictions => _vaultingGameplayRestrictions;

	public bool IsEnableVaulting => Singleton<BackendConfigSettingsClass>.Instance.VaultingSettings.IsActive;

	public IWeaponMountingComponent WeaponMountingComponent => _weaponMountingComponent;

	public ILeftHandController LeftHandController => _leftHandController;

	public GClass2059<NightVisionComponent> NightVisionObserver { get; set; }

	public GClass2059<ThermalVisionComponent> ThermalVisionObserver { get; set; }

	public GClass2059<FaceShieldComponent> FaceShieldObserver { get; set; }

	public GClass2059<FaceShieldComponent> FaceCoverObserver { get; set; }

	public string Location { get; set; }

	public ISpawnPoint SpawnPoint { get; set; }

	public float RayLength { get; set; }

	public InteractableObject InteractableObject { get; set; }

	public bool InteractableObjectIsProxy { get; set; }

	public bool IsAgressorInLighthouseTraderZone { get; set; }

	public Player InteractablePlayer { get; set; }

	public PlaceItemTrigger PlaceItemZone { get; set; }

	public ExfiltrationPoint ExfiltrationPoint { get; set; }

	public bool ExitTriggerZone { get; set; }

	public MalfunctionRandom MalfRandoms { get; set; } = new MalfunctionRandom(0);

	public string Infiltration => Profile.Info.EntryPoint;

	public string GroupId => Profile.Info.GroupId;

	public string TeamId => Profile.Info.TeamId;

	public float CarryingWeightRelativeModifier => Skills.CarryingWeightRelativeModifier * HealthController.CarryingWeightRelativeModifier;

	public float CarryingWeightAbsoluteModifier => HealthController.CarryingWeightAbsoluteModifier;

	public Inventory Inventory => InventoryController.Inventory;

	public InventoryEquipment Equipment => Inventory.Equipment;

	public bool IsInventoryOpened
	{
		get
		{
			return _isInventoryOpened;
		}
		set
		{
			_isInventoryOpened = value;
		}
	}

	public RecodableItemsHandler RecodableItemsHandler => recodableItemsHandler;

	public float BlindnessDuration
	{
		get
		{
			if (ThermalVisionObserver.Component?.Togglable != null && ThermalVisionObserver.Component.Togglable.On)
			{
				return 0f;
			}
			FaceShieldComponent component = FaceShieldObserver.Component;
			float num = ((component != null && (component.Togglable == null || component.Togglable.On)) ? (1f - component.BlindnessProtection) : 1f);
			float num2 = ((Equipment.GetSlot(EquipmentSlot.Eyewear).ContainedItem is VisorsItemClass visorsItemClass) ? (1f - visorsItemClass.BlindnessProtection) : 1f);
			if (!(Equipment.GetSlot(EquipmentSlot.FaceCover).ContainedItem is FaceCoverItemClass { FaceShield: var faceShield }))
			{
				return num * num2;
			}
			bool flag = faceShield != null && (faceShield.Togglable == null || faceShield.Togglable.On);
			return num * num2 * (flag ? (1f - faceShield.BlindnessProtection) : 1f);
		}
	}

	public int CurrentHour
	{
		get
		{
			if (GameWorld != null && GameWorld.GameDateTime != null)
			{
				return GameWorld.GameDateTime.Calculate().Hour;
			}
			return 0;
		}
	}

	public bool Boolean_0
	{
		get
		{
			return _sense;
		}
		set
		{
			_sense = value;
			bool flag = HandsController != null && HandsController.IsAiming;
			bool flag2 = _sense && !flag;
			if (_displaySense != flag2)
			{
				_displaySense = flag2;
				this.OnSenseChanged?.Invoke(_displaySense);
			}
		}
	}

	public string KillerId
	{
		get
		{
			IPlayerOwner player = LastDamageInfo.Player;
			if (player != null && this != player)
			{
				return player.iPlayer.ProfileId;
			}
			return null;
		}
	}

	public string KillerAccountId
	{
		get
		{
			IPlayerOwner player = LastDamageInfo.Player;
			if (player != null && this != player)
			{
				return player.iPlayer.AccountId;
			}
			return null;
		}
	}

	public bool HasGlasses
	{
		get
		{
			VisorsItemClass glasses;
			return TryFindGlasses(out glasses);
		}
	}

	public bool HandsIsEmpty => HandsController is EmptyHandsController;

	public bool IsWeaponOrKnifeInHands
	{
		get
		{
			if (!(HandsController is FirearmController))
			{
				return HandsController is BaseKnifeController;
			}
			return true;
		}
	}

	public virtual Vector3 LocalShotDirection => ProceduralWeaponAnimation.ShotDirection;

	public ETagStatus HealthStatus
	{
		get
		{
			float normalized = HealthController.GetBodyPartHealth(EBodyPart.Common).Normalized;
			HealthController.GetBodyPartsInCriticalCondition(0.15f, out var all, out var vital);
			if (vital <= 0 && normalized >= 0.2f)
			{
				if (all <= 2 && normalized >= 0.4f)
				{
					if (normalized < 0.9f)
					{
						return ETagStatus.Injured;
					}
					return ETagStatus.Healthy;
				}
				return ETagStatus.BadlyInjured;
			}
			return ETagStatus.Dying;
		}
	}

	public string TryGetId
	{
		get
		{
			Profile profile = Profile;
			object obj;
			if (profile == null)
			{
				obj = null;
			}
			else
			{
				obj = profile.Id;
				if (obj != null)
				{
					goto IL_0024;
				}
			}
			obj = PlayerId.ToString();
			goto IL_0024;
			IL_0024:
			return (string)obj;
		}
	}

	public int PlayerId { get; set; }

	public string ProfileId => Profile.Id;

	public string AccountId => Profile.AccountId;

	public Profile Profile { get; set; }

	public IStatisticsManager StatisticsManager { get; set; }

	public SkillManager Skills => Profile?.Skills;

	public EUpdateQueue UpdateQueue => _updateQueue;

	public EUpdateQueue ArmsUpdateQueue => _armsUpdateQueue;

	public ECameraType VisibleToCameraType { get; set; }

	public bool IsVisibleToCamera { get; set; } = true;

	public EUpdateQueue PhysicalUpdateQueue => EUpdateQueue.Update;

	public EUpdateMode ArmsUpdateMode => _armsUpdateMode;

	public EUpdateMode BodyUpdateMode => _bodyUpdateMode;

	public Player GetPlayer => this;

	public IAIData AIData { get; set; }

	public PlayerLoyaltyData Loyalty { get; set; }

	public bool IsAI
	{
		get
		{
			if (AIData != null)
			{
				return AIData.IsAI;
			}
			return false;
		}
	}

	public Dictionary<BodyPartType, EnemyPart> MainParts { get; set; }

	public virtual AbstractHandsController HandsController
	{
		get
		{
			return _handsController;
		}
		set
		{
			AbstractHandsController handsController = _handsController;
			_handsController = value;
			PlayerAnimator.EWeaponAnimationType weaponAnimationType = GetWeaponAnimationType(_handsController);
			MovementContext.PlayerAnimatorSetWeaponId(weaponAnimationType);
			_isGrenadeOrKnife = _handsController != null && weaponAnimationType != PlayerAnimator.EWeaponAnimationType.Rifle;
			if (_isGrenadeOrKnife)
			{
				method_17(MovementContext.CurrentState.Name, MovementContext.CurrentState.Name);
			}
			if ((object)handsController != _handsController && this.OnHandsControllerChanged != null)
			{
				this.OnHandsControllerChanged(handsController, _handsController);
			}
		}
	}

	public EPlayerState CurrentStateName => MovementContext.CurrentState.Name;

	public virtual InventoryController InventoryController => _inventoryController;

	public AbstractQuestControllerClass AbstractQuestControllerClass => _questController;

	public IPlayerSearchController SearchController => _inventoryController.PlayerSearchController;

	public AbstractAchievementControllerClass AbstractAchievementControllerClass => _achievementsController;

	public AbstractPrestigeControllerClass AbstractPrestigeControllerClass => _prestigeController;

	public GClass3617 GClass3617_0 => _dialogController;

	public IEnumerable<GInterface518> IEnumerable_0
	{
		get
		{
			if (_questController != null)
			{
				yield return _questController;
			}
			if (_achievementsController != null)
			{
				yield return _achievementsController;
			}
			if (_prestigeController != null)
			{
				yield return _prestigeController;
			}
		}
	}

	public int Id => PlayerId;

	public string FullIdInfoClean
	{
		get
		{
			string text = _fullIdInfo;
			string[] obj;
			object obj2;
			if (text == null)
			{
				obj = new string[7] { "[", null, null, null, null, null, null };
				Profile profile = Profile;
				if (profile == null)
				{
					obj2 = null;
				}
				else
				{
					obj2 = profile.Nickname;
					if (obj2 != null)
					{
						goto IL_0039;
					}
				}
				obj2 = "null";
				goto IL_0039;
			}
			goto IL_009b;
			IL_0039:
			obj[1] = (string)obj2;
			obj[2] = "|";
			Profile profile2 = Profile;
			object obj3;
			if (profile2 == null)
			{
				obj3 = null;
			}
			else
			{
				obj3 = profile2.AccountId;
				if (obj3 != null)
				{
					goto IL_005f;
				}
			}
			obj3 = "null";
			goto IL_005f;
			IL_005f:
			obj[3] = (string)obj3;
			obj[4] = "|";
			Profile profile3 = Profile;
			object obj4;
			if (profile3 == null)
			{
				obj4 = null;
			}
			else
			{
				obj4 = profile3.Id;
				if (obj4 != null)
				{
					goto IL_0085;
				}
			}
			obj4 = "null";
			goto IL_0085;
			IL_009b:
			return text;
			IL_0085:
			obj[5] = (string)obj4;
			obj[6] = "]";
			text = (_fullIdInfo = string.Concat(obj));
			goto IL_009b;
		}
	}

	public virtual string FullIdInfo => FullIdInfoClean;

	public EPlayerSide Side => Profile.Info.Side;

	public BifacialTransform Transform => PlayerBones.BodyTransform;

	public IHealthController HealthController => _healthController;

	public ActiveHealthController ActiveHealthController => _healthController as ActiveHealthController;

	public virtual float Awareness
	{
		get
		{
			return _awareness;
		}
		set
		{
			_awareness = value;
		}
	}

	public float DeltaTime => _deltaTimeDelegate();

	public virtual Ray InteractionRay => new Ray(_playerLookRaycastTransform.position - _playerLookRaycastTransform.forward * EFTHardSettings.Instance.BEHIND_CAST, _playerLookRaycastTransform.forward);

	public Vector3 InteractionRayOriginOnStartOperation { get; set; }

	public Vector3 InteractionRayDirectionOnStartOperation { get; set; }

	public int Int32_0
	{
		get
		{
			return _negativeBuffsCount;
		}
		set
		{
			if (_negativeBuffsCount < 1 && value > 0)
			{
				ExecuteSkill((Action)delegate
				{
					Skills.StimulatorNegativeBuff.Begin();
				});
			}
			if (_negativeBuffsCount > 0 && value < 1)
			{
				ExecuteSkill((Action)delegate
				{
					Skills.StimulatorNegativeBuff.Complete();
				});
			}
			_negativeBuffsCount = value;
		}
	}

	public virtual bool IsVisible
	{
		get
		{
			if (!FirstPersonPointOfView)
			{
				return OnScreen;
			}
			return true;
		}
		set
		{
		}
	}

	public virtual float SqrCameraDistance
	{
		get
		{
			if (FirstPersonPointOfView)
			{
				return 0f;
			}
			return CameraClass.Instance.SqrDistance(Transform.position);
		}
	}

	public PlayerBones PlayerBones { get; set; }

	public int OwnerId => PlayerId;

	public BotsGroup BotsGroup { get; set; }

	public bool IsYourPlayer { get; set; }

	[Obsolete("Use Player.Transform instead!", true)]
	public new Transform transform => base.transform;

	string IDissonancePlayer.PlayerId => ProfileId;

	Vector3 IDissonancePlayer.Position => Transform.Original.position;

	Quaternion IDissonancePlayer.Rotation => Transform.Original.rotation;

	public IPlayerVoipController VoipController { get; set; }

	NetworkPlayerType IDissonancePlayer.Type
	{
		get
		{
			if (!(this is ClientPlayer))
			{
				return NetworkPlayerType.Remote;
			}
			return NetworkPlayerType.Local;
		}
	}

	public bool IsTracking => !Destroyed;

	public DissonanceComms DissonanceComms { get; set; }

	public DateTime HearingDateTime { get; set; }

	public EVoipState VoipState { get; set; }

	public bool IgnoreCameraCollider { get; set; }

	public BTRSide BtrInteractionSide { get; set; }

	public TripwireInteractionTrigger TripwireInteractionTrigger { get; set; }

	public EventObjectInteractive EventObjectInteractive { get; set; }

	public EPlayerBtrState BtrState
	{
		get
		{
			return _btrState;
		}
		set
		{
			if (_btrState != value)
			{
				IgnoreCameraCollider = value >= EPlayerBtrState.Approach;
				switch (value)
				{
				default:
					throw new ArgumentOutOfRangeException();
				case EPlayerBtrState.Outside:
					BtrInteractionSide = null;
					_customHandRotator = new GClass2093();
					break;
				case EPlayerBtrState.GoIn:
					_customHandRotator = new GClass2094(this);
					break;
				case EPlayerBtrState.Inside:
					_customHandRotator = (_customHandRotator.IsValid ? new GClass2095(this, _customHandRotator.GetRotation()) : new GClass2095(this));
					break;
				case EPlayerBtrState.GoOut:
					_customHandRotator = new GClass2095(this);
					break;
				case EPlayerBtrState.Approach:
					break;
				}
				_btrState = value;
				this.OnBtrStateChanged?.Invoke(_btrState);
			}
		}
	}

	public virtual bool CanBeSnapped => true;

	public EProcessStatus ProcessStatus
	{
		get
		{
			return _processStatus;
		}
		set
		{
			_processStatus = value;
			InventoryController.UpdateLockedStatus();
		}
	}

	public AbstractProcess AbstractProcess_0 { get; set; }

	public Slot ActiveSlot { get; set; }

	public bool StateIsSuitableForHandInput => Array.IndexOf(EFTHardSettings.Instance.UnsuitableStates, CurrentState.Name) < 0;

	public Item LastEquippedWeaponOrKnifeItem
	{
		get
		{
			return _lastEquippedWeaponOrKnifeItem;
		}
		set
		{
			if (value is Weapon || value is KnifeItemClass)
			{
				_lastEquippedWeaponOrKnifeItem = value;
			}
		}
	}

	public event Action<float, float, int> OnSpeedChangedEvent;

	public event Action<SightComponent> OnSightChangedEvent;

	public event Action<bool> OnTacticalInteractionChanged;

	public event Action<SightComponent, ESmoothScopeState> OnSmoothSightChange;

	public event GDelegate65 OnDamageReceived;

	public event Action<DamageInfoStruct, EBodyPart, float> BeingHitAction;

	public event Action<bool> OnPropVisibility;

	public event Action<string> OnShowAmmoCountZeroingPanel;

	public event Action<Weapon.EFireMode> OnShowFireMode;

	public event Action<int, int, int, string, bool> OnShowAmmoDetails;

	public event GDelegate70 OnPlayerDead;

	public static event Action<Player, IPlayer, DamageInfoStruct, EBodyPart> OnPlayerDeadStatic;

	public event GDelegate71 OnPlayerDeadOrUnspawn;

	public event Action<bool> OnSenseChanged;

	public event Action PossibleInteractionsChanged;

	public event Action<EPhraseTrigger, int> PhraseSituation;

	public event Action<VisorsItemClass, bool> OnGlassesChanged;

	public event Action<float> OnBlindnessProtectionChanged;

	public event Action<Player, bool> OnInventoryOpened;

	public event Action OnStartInventoryOpen;

	public event Action OnStartQuickdrawPistol;

	public event Action<string, int> OnSpecialPlaceVisited;

	public event Action<IPlayer> OnIPlayerDeadOrUnspawn;

	public event Action<AbstractHandsController, AbstractHandsController> OnHandsControllerChanged;

	public event Action UpdateEvent;

	public event Action FixedUpdateEvent;

	public event Action<ExfiltrationPoint, bool> OnEpInteraction;

	public event Action<EPlayerBtrState> OnBtrStateChanged;

	public event Action HandsChangingEvent;

	public event Action<IHandsController> HandsChangedEvent;

	public static GStruct156<ItemAddress> ToItemAddress(GClass1950 descriptor)
	{
		return Singleton<GameWorld>.Instance.ToItemAddress(descriptor);
	}

	public virtual GStruct156<Item> FindItemById(MongoID itemId, bool checkDistance = true, bool checkOwnership = true)
	{
		GStruct156<(Item, GameWorld.GStruct162)> gStruct = GameWorld.FindItemWithWorldData(itemId);
		if (gStruct.Failed)
		{
			return gStruct.Error;
		}
		return gStruct.Value.Item1;
	}

	public IAnimator GetBodyAnimatorCommon()
	{
		if (BackendConfigAbstractClass.Config.UseSpiritPlayer && Spirit.IsActive)
		{
			return Spirit.BodyAnimatorWrapper;
		}
		return _animators[0];
	}

	public IAnimator GetArmsAnimatorCommon()
	{
		if (BackendConfigAbstractClass.Config.UseSpiritPlayer && Spirit.IsActive)
		{
			return Spirit.ArmsAnimator;
		}
		return _animators[1];
	}

	public void SetArmsAnimatorCommon(IAnimator animator)
	{
		_animators[1] = animator;
	}

	public RuntimeAnimatorController GetArmsAnimatorControllerCommon()
	{
		IAnimator armsAnimatorCommon = GetArmsAnimatorCommon();
		if (armsAnimatorCommon.runtimeAnimatorController is AnimatorOverrideController)
		{
			return ((AnimatorOverrideController)armsAnimatorCommon.runtimeAnimatorController).runtimeAnimatorController;
		}
		return armsAnimatorCommon.runtimeAnimatorController;
	}

	public ICharacterController GetCharacterControllerCommon()
	{
		if (BackendConfigAbstractClass.Config.UseSpiritPlayer && Spirit.IsActive)
		{
			return Spirit.CharacterController;
		}
		return _characterController;
	}

	public TriggerColliderSearcher GetTriggerColliderSearcher()
	{
		if (BackendConfigAbstractClass.Config.UseSpiritPlayer && Spirit.IsActive)
		{
			return Spirit.TriggerColliderSearcher;
		}
		return _triggerColliderSearcher;
	}

	public void AddMouseSensitivityModifier(EMouseSensitivityModifier type, float value)
	{
		_mouseSensitivityModifiers[type] = value;
		method_0();
	}

	public void RemoveMouseSensitivityModifier(EMouseSensitivityModifier type)
	{
		_mouseSensitivityModifiers.Remove(type);
		method_0();
	}

	public void method_0()
	{
		_mouseSensitivityModifier = 0f;
		foreach (float value in _mouseSensitivityModifiers.Values)
		{
			_mouseSensitivityModifier += value;
		}
	}

	public float GetRotationMultiplier()
	{
		if (HandsController != null && HandsController.IsAiming)
		{
			if (!(HandsController.AimingSensitivity >= float.Epsilon))
			{
				return GetAimingSensitivity();
			}
			return HandsController.AimingSensitivity;
		}
		return GetSensitivity() * (1f + _mouseSensitivityModifier);
	}

	public float GetCharacterSpeedMultiplier()
	{
		return 1f;
	}

	public void method_1()
	{
		CreateMovementContext();
		PoseMemo = MovementContext.PoseLevel;
		_speedMemo = MovementContext.CharacterMovementSpeed;
	}

	public virtual void CreateMovementContext()
	{
		LayerMask mOVEMENT_MASK = EFTHardSettings.Instance.MOVEMENT_MASK;
		MovementContext = MovementContext.Create(this, GetBodyAnimatorCommon, GetCharacterControllerCommon, mOVEMENT_MASK);
	}

	public bool method_2(float dir)
	{
		return GClass855.Positive(MovementContext.Tilt * dir);
	}

	public void method_3(float dir)
	{
		if (!_lastSlowLean)
		{
			CurrentLeanType = LeanType.NormalLean;
			MovementContext.SetBlindFire(0f);
			dir = (method_2(dir) ? 0f : (dir * 5f));
			CurrentManagedState.SetTilt(dir);
		}
	}

	public void method_4(float fromDir)
	{
		if (!_lastSlowLean && method_2(fromDir))
		{
			CurrentManagedState.SetTilt(0f);
		}
	}

	public void SlowLean(float dir)
	{
		_lastSlowLean = false;
		if (!(MovementContext.TiltDiff >= 1f) && Mathf.Abs(dir) > 0f)
		{
			CurrentLeanType = LeanType.SlowLean;
			dir *= 0.1f;
			CurrentManagedState.SetTilt(MovementContext.Tilt + dir);
			_lastSlowLean = true;
		}
	}

	public void ToggleBlindFire(float blindFireValue)
	{
		if (!MovementContext.LeftStanceEnabled || blindFireValue == 0f)
		{
			CurrentManagedState?.BlindFire(Math.Sign(blindFireValue));
		}
	}

	public void StopBlindFire()
	{
		CurrentManagedState?.BlindFire(0);
	}

	public void ToggleStep(int direction)
	{
		if (MovementContext.Step != direction)
		{
			CurrentManagedState.SetStep(direction);
		}
		else
		{
			ReturnFromStep(direction);
		}
	}

	public void ReturnFromStep(int direction)
	{
		if (MovementContext.Step == direction)
		{
			CurrentManagedState.SetStep(0);
		}
	}

	public void method_5(float fallHeight, float jumpHeight)
	{
		LandingAdjustments(fallHeight);
		PlayGroundedSound(fallHeight, jumpHeight);
	}

	public virtual void LandingAdjustments(float d)
	{
		if (d < Singleton<BackendConfigSettingsClass>.Instance.Inertia.FallThreshold)
		{
			return;
		}
		float num = EFTHardSettings.Instance.SpeedLimitDuration.Evaluate(d);
		if (!(num < float.Epsilon))
		{
			float num2 = EFTHardSettings_0.SpeedLimitAfterFall.Evaluate(d);
			if (!(num2 >= 1f))
			{
				MovementContext.ChangeSpeedLimit(num2, ESpeedLimit.Fall, num);
			}
		}
	}

	public virtual void Move(Vector2 direction)
	{
		CurrentManagedState.Move(direction);
		if (direction.sqrMagnitude >= float.Epsilon)
		{
			_lastMovement = Time.time;
		}
		InputDirection = direction;
	}

	public void ChangePose(float poseDelta)
	{
		CurrentManagedState.ChangePose(poseDelta);
		PoseMemo = PoseLevel;
		if (poseDelta != 0f)
		{
			_lastMovement = Time.time;
		}
	}

	public void method_6()
	{
		if (!IsInPronePose && PoseLevel < 0.5f)
		{
			float num = ((PoseMemo < 0.5f) ? 1f : PoseMemo);
			CurrentManagedState.ChangePose(num - PoseLevel);
		}
	}

	public void method_7()
	{
		float num = ((!(Math.Abs(PoseMemo - PoseLevel) < 1E-06f)) ? PoseMemo : ((PoseLevel >= 0.5f) ? 0f : 1f));
		if (IsInPronePose)
		{
			num = PoseLevel;
		}
		CurrentManagedState.ChangePose(num - PoseLevel);
	}

	public void ChangeSpeed(float speedDelta)
	{
		CurrentManagedState.ChangeSpeed(speedDelta);
	}

	public void RaiseChangeSpeedEvent()
	{
		this.OnSpeedChangedEvent?.Invoke(Physical.Sprinting ? MovementContext.CharacterMovementSpeed : MovementContext.ClampedSpeed, MovementContext.MaxSpeed, MovementContext.CovertNoiseLevel);
	}

	public void RaiseSightChangedEvent(SightComponent sightComp)
	{
		this.OnSightChangedEvent?.Invoke(sightComp);
	}

	public void RaiseTacticalInteractionChangedEvent(bool isPress)
	{
		this.OnTacticalInteractionChanged?.Invoke(isPress);
	}

	public void RaiseSmoothSightChangeEvent(SightComponent sightComp, ESmoothScopeState state)
	{
		this.OnSmoothSightChange?.Invoke(sightComp, state);
	}

	public virtual void AddStateSpeedLimit(float speedDelta, ESpeedLimit cause)
	{
		MovementContext?.AddStateSpeedLimit(speedDelta, cause);
	}

	public virtual void UpdateSpeedLimit(float speedDelta, ESpeedLimit cause)
	{
		MovementContext?.ChangeSpeedLimit(speedDelta, cause);
	}

	public virtual void UpdateSpeedLimit(float speedDelta, ESpeedLimit cause, float duration)
	{
		MovementContext?.ChangeSpeedLimit(speedDelta, cause, duration);
	}

	public virtual void RemoveStateSpeedLimit(ESpeedLimit cause)
	{
		MovementContext.RemoveStateSpeedLimit(cause);
	}

	public void method_8()
	{
		if (!GClass855.Positive(MovementContext.ClampedSpeed))
		{
			ChangeSpeed(_speedMemo - MovementContext.CharacterMovementSpeed);
		}
	}

	public void method_9()
	{
		if (GClass855.Positive(MovementContext.ClampedSpeed))
		{
			_speedMemo = MovementContext.CharacterMovementSpeed;
			ChangeSpeed(0f - _speedMemo);
		}
		else
		{
			method_8();
		}
	}

	public virtual void Rotate(Vector2 deltaRotation, bool ignoreClamp = false)
	{
		if (!GClass858.IsAnyComponentInfinity(deltaRotation) && !GClass858.IsAnyComponentNaN(deltaRotation))
		{
			CurrentManagedState.Rotate(deltaRotation, ignoreClamp);
			if (deltaRotation.sqrMagnitude >= float.Epsilon)
			{
				_lastMovement = Time.time;
			}
		}
		else
		{
			UnityEngine.Debug.LogErrorFormat("Attemption to set wrong deltaRotation: {0}", deltaRotation);
		}
	}

	public virtual void Look(float deltaLookY, float deltaLookX, bool withReturn = true)
	{
		bool num = HandsController != null && HandsController.IsAiming && !IsAI;
		EFTHardSettings instance = EFTHardSettings.Instance;
		Vector2 mOUSE_LOOK_HORIZONTAL_LIMIT = instance.MOUSE_LOOK_HORIZONTAL_LIMIT;
		Vector2 mOUSE_LOOK_VERTICAL_LIMIT = instance.MOUSE_LOOK_VERTICAL_LIMIT;
		if (num)
		{
			if (_cachedMouseLookControl != _mouseLookControl)
			{
				_cachedMouseLookControl = _mouseLookControl;
				int value = Singleton<SharedGameSettingsClass>.Instance.Game.Settings.FieldOfView.Value;
				float x = (_mouseLookControl ? ((float)value) : (ProceduralWeaponAnimation.CurrentScope.IsOptic ? 35f : ((float)value - 15f)));
				CameraClass.Instance.SetFov(x, 1f);
			}
			mOUSE_LOOK_HORIZONTAL_LIMIT *= instance.MOUSE_LOOK_LIMIT_IN_AIMING_COEF;
		}
		Vector3 eulerAngles = ProceduralWeaponAnimation.HandsContainer.CameraTransform.eulerAngles;
		if (eulerAngles.x >= 50f && eulerAngles.x <= 90f && MovementContext.IsSprintEnabled)
		{
			mOUSE_LOOK_VERTICAL_LIMIT.y = 0f;
		}
		_horizontal = Mathf.Clamp(_horizontal - deltaLookY, mOUSE_LOOK_HORIZONTAL_LIMIT.x, mOUSE_LOOK_HORIZONTAL_LIMIT.y);
		_vertical = Mathf.Clamp(_vertical + deltaLookX, mOUSE_LOOK_VERTICAL_LIMIT.x, mOUSE_LOOK_VERTICAL_LIMIT.y);
		float x2 = ((_vertical > 0f) ? (_vertical * (1f - _horizontal / mOUSE_LOOK_HORIZONTAL_LIMIT.y * (_horizontal / mOUSE_LOOK_HORIZONTAL_LIMIT.y))) : _vertical);
		if (_setResetedLookNextFrame)
		{
			_isResettingLook = false;
			_setResetedLookNextFrame = false;
		}
		if (_resetLook)
		{
			_mouseLookControl = false;
			_resetLook = false;
			_isResettingLook = true;
			deltaLookY = 0f;
			deltaLookX = 0f;
		}
		if (Math.Abs(deltaLookY) >= float.Epsilon && Math.Abs(deltaLookX) >= float.Epsilon)
		{
			_mouseLookControl = true;
		}
		if (!_mouseLookControl && withReturn)
		{
			if (Mathf.Abs(_horizontal) > 0.01f)
			{
				_horizontal = Mathf.Lerp(_horizontal, 0f, Time.deltaTime * 15f);
			}
			else
			{
				_horizontal = 0f;
			}
			if (Mathf.Abs(_vertical) > 0.01f)
			{
				_vertical = Mathf.Lerp(_vertical, 0f, Time.deltaTime * 15f);
			}
			else
			{
				_vertical = 0f;
			}
		}
		if (!_isResettingLook && _horizontal != 0f && _vertical != 0f)
		{
			_isLooking = true;
		}
		else
		{
			_isLooking = false;
		}
		if (_horizontal == 0f && _vertical == 0f)
		{
			_setResetedLookNextFrame = true;
		}
		HeadRotation = new Vector3(x2, _horizontal, 0f);
		ProceduralWeaponAnimation.SetHeadRotation(HeadRotation);
	}

	public void ResetLookDirection()
	{
		_resetLook = true;
	}

	public void Jump()
	{
		if (!(_vaultingTiming > Singleton<BackendConfigSettingsClass>.Instance.VaultingSettings.VaultingInputTime) && !MovementContext.PlayerAnimatorGetIsVaulting())
		{
			CurrentManagedState.Jump();
		}
	}

	public void Vaulting()
	{
		if (!_isVaultingPressed)
		{
			method_10();
		}
		else
		{
			method_11();
		}
	}

	public void method_10()
	{
		OnStartInventoryOpen += method_11;
		UpdateEvent += method_12;
		_isVaultingPressed = true;
	}

	public void method_11()
	{
		OnStartInventoryOpen -= method_11;
		UpdateEvent -= method_12;
		_vaultingTiming = 0f;
		_isVaultingPressed = false;
	}

	public void method_12()
	{
		CurrentManagedState.Vaulting();
		_vaultingTiming += Time.deltaTime;
	}

	public void EnableSprint(bool enable)
	{
		if (CurrentManagedState != null)
		{
			CurrentManagedState.EnableSprint(enable);
		}
	}

	public void ToggleSprint()
	{
		bool enable = !Physical.Sprinting;
		CurrentManagedState.EnableSprint(enable, isToggle: true);
	}

	public void ToggleHoldingBreath()
	{
		CurrentManagedState.EnableBreath(!Physical.HoldingBreath);
	}

	public void StopHoldingBreath()
	{
		if (Physical.HoldingBreath)
		{
			CurrentManagedState.EnableBreath(enable: false);
		}
	}

	public void method_13(float deltaTime)
	{
		MovementContext.ManualUpdate(deltaTime);
	}

	public void HeightInterpolation(float timeDeltatime)
	{
		if (!Mathf.Approximately(timeDeltatime, 0f))
		{
			float num = (IsInPronePose ? 0f : (Transform.position.y - _prevHeight));
			float num2 = Mathf.SmoothDamp(PlayerBones.AnimatedTransform.localPosition.y - num, 0f, ref _dampVelocity, _currentSmoothSpeed, 1000000f, timeDeltatime);
			PlayerBones.AnimatedTransform.localPosition = new Vector3(PlayerBones.AnimatedTransform.localPosition.x, Mathf.Clamp(num2, -0.2f, 0.2f), PlayerBones.AnimatedTransform.localPosition.z);
			_currentSmoothSpeed = Mathf.Lerp(_currentSmoothSpeed, (Mathf.Abs(num2) > _previousY) ? (HeightSmoothTime * 0.3f) : HeightSmoothTime, timeDeltatime);
			_previousY = Mathf.Abs(num2);
		}
	}

	public void method_14()
	{
		if (!Mathf.Approximately(PlayerBones.AnimatedTransform.localPosition.y, 0f))
		{
			PlayerBones.AnimatedTransform.localPosition = new Vector3(PlayerBones.AnimatedTransform.localPosition.x, 0f, PlayerBones.AnimatedTransform.localPosition.z);
		}
		_prevHeight = Transform.position.y;
		_previousY = 0f;
	}

	public virtual void ToggleProne()
	{
		if (!MovementContext.IsAnimatorInteractionOn)
		{
			CurrentManagedState.Prone();
		}
	}

	public void method_15()
	{
		if (InventoryController.Inventory.Equipment.GetSlot(EquipmentSlot.Headwear).ContainedItem is CompoundItem thisItem)
		{
			TogglableComponent togglableComponent = GClass3380.GetItemComponentsInChildren<TogglableComponent>(thisItem).FirstOrDefault();
			if (togglableComponent != null)
			{
				InventoryController.TryRunNetworkTransaction(togglableComponent.Set(!togglableComponent.On, simulate: true));
			}
		}
	}

	public virtual void vmethod_0(WorldInteractiveObject interactiveObject, InteractionResult interactionResult, Action callback)
	{
		EInteractionType interactionType = interactionResult.InteractionType;
		UnityEngine.Debug.LogFormat("<color=yellow>interact with door, interaction type '{0}'</color>", interactionType);
		CurrentManagedState.StartDoorInteraction(interactiveObject, interactionResult, callback);
		UpdateInteractionCast();
	}

	public virtual void vmethod_1(WorldInteractiveObject door, InteractionResult interactionResult)
	{
		if (!(door == null))
		{
			CurrentManagedState.ExecuteDoorInteraction(door, interactionResult, null, this);
		}
	}

	public virtual void vmethod_2(BTRSide btr, byte placeId, EInteractionType interaction)
	{
	}

	public virtual void vmethod_3(TransitControllerAbstractClass controller, int transitPointId, string keyId, EDateTime time)
	{
	}

	public virtual void vmethod_4(TripwireSynchronizableObject tripwire)
	{
	}

	public virtual void vmethod_5(GClass2282 controller, int objectId, EventObject.EInteraction interaction)
	{
	}

	public virtual void vmethod_6(string itemId, string zoneId, bool successful)
	{
		PlantItem(itemId, zoneId, successful);
	}

	public void PlantItem(string itemId, string zoneId, bool successful)
	{
		if (successful)
		{
			Profile.ItemDroppedAtPlace(itemId, zoneId);
		}
	}

	public virtual void PlantItemLocalOnly(Item item, string zone)
	{
		PlantItem(item.TemplateId, zone, successful: true);
	}

	public virtual void OperateStationaryWeapon(StationaryWeapon stationaryWeapon, StationaryPacketStruct.EStationaryCommand command)
	{
		switch (command)
		{
		case StationaryPacketStruct.EStationaryCommand.Occupy:
			if (Vector3.Distance(Position, stationaryWeapon.transform.position) > 2f)
			{
				UnityEngine.Debug.LogErrorFormat(GetPlayer, "Player [{0}] in position {1} attempts to occupy stationary weapon [{2}:{3}] in position {4} (threshold 2 meters)", GetPlayer.FullIdInfo, Position.ToString("F2"), stationaryWeapon.Item.ShortName, stationaryWeapon.Id, stationaryWeapon.transform.position);
			}
			if (stationaryWeapon.IsAvailable(ProfileId))
			{
				stationaryWeapon.SetOperator(ProfileId);
				method_126();
				MovementContext.StationaryWeapon = stationaryWeapon;
				MovementContext.InteractionParameters = stationaryWeapon.GetInteractionParameters();
				MovementContext.PlayerAnimatorSetApproached(b: false);
				MovementContext.PlayerAnimatorSetStationary(b: true);
				RemoveLeftHandItem();
				MovementContext.PlayerAnimatorSetStationaryAnimation((int)stationaryWeapon.Animation);
			}
			break;
		case StationaryPacketStruct.EStationaryCommand.Leave:
			if (ActiveSlot != null && ActiveSlot.ContainedItem != null)
			{
				SetInHands(ActiveSlot.ContainedItem, delegate
				{
				});
			}
			else
			{
				SetFirstAvailableItem(delegate
				{
				});
			}
			break;
		default:
			MovementContext.PlayerAnimatorSetStationary(b: false);
			if (MovementContext.StationaryWeapon != null)
			{
				MovementContext.StationaryWeapon.Unlock(ProfileId);
			}
			break;
		}
	}

	public void FastForwardToStationaryWeapon(Item item, Vector2 stationaryRotation, Quaternion playerRotation, Quaternion stationaryPlayerRotation)
	{
		StationaryWeapon stationaryWeapon = GameWorld.FindStationaryWeaponByItemId(item.Id);
		if (!(stationaryWeapon == null))
		{
			stationaryWeapon.SetOperator(ProfileId);
			MovementContext.StationaryWeapon = stationaryWeapon;
			Teleport(stationaryWeapon.GetInteractionParameters().InteractionPosition);
			bool flag = BodyAnimatorCommon.enabled;
			bool keepAnimatorControllerStateOnDisable = BodyAnimatorCommon.keepAnimatorControllerStateOnDisable;
			BodyAnimatorCommon.keepAnimatorControllerStateOnDisable = true;
			BodyAnimatorCommon.enabled = false;
			MovementContext.PlayerAnimatorSetApproached(b: true);
			MovementContext.PlayerAnimatorSetStationary(b: true);
			MovementContext.PlayerAnimatorSetStationaryAnimation((int)stationaryWeapon.Animation);
			Transform.rotation = stationaryPlayerRotation;
			MovementContext.Rotation = stationaryRotation;
			MovementContext.UpdateStationaryDeltaAngle();
			for (int i = 0; i < 150; i++)
			{
				BodyAnimatorCommon.Update(0.01f);
			}
			BodyAnimatorCommon.enabled = flag;
			BodyAnimatorCommon.keepAnimatorControllerStateOnDisable = keepAnimatorControllerStateOnDisable;
			Teleport(stationaryWeapon.GetInteractionParameters().InteractionPosition);
			if (stationaryWeapon.Animation == StationaryWeapon.EStationaryAnimationType.AGS_17)
			{
				Vector3 eulerAngles = playerRotation.eulerAngles;
				Transform.rotation = Quaternion.Euler(0f, eulerAngles.y, eulerAngles.z);
			}
			else
			{
				Transform.rotation = stationaryPlayerRotation;
			}
		}
	}

	public void Crutch_FinalizeStationaryWeapState(StationaryPacketStruct swPacket)
	{
		if (MovementContext.CurrentState.Name == EPlayerState.Stationary && !swPacket.IsStationaryFinal)
		{
			MovementContext.PlayerAnimatorSetStationary(b: false);
			if (MovementContext.StationaryWeapon != null)
			{
				MovementContext.StationaryWeapon.Unlock(ProfileId);
			}
		}
	}

	public void TryMountWeapon()
	{
		if (HasFirearmInHands() && HandsController is FirearmController firearmController && firearmController.Weapon.IsMountable && !MovementContext.IsStationaryWeaponInHands && MovementContext.IsGrounded && !firearmController.IsInReloadOperation() && !firearmController.IsInSpawnOperation() && !firearmController.IsInInteraction() && !firearmController.IsInRemoveOperation() && MovementContext.BlindFire == 0 && MovementContext.IsGrounded)
		{
			_weaponMountingComponent.TryMountWeapon(firearmController);
		}
	}

	public void SetRadioTransmitterView(RadioTransmitterView rtView)
	{
		_radioTransmitterView = rtView;
	}

	public void ReceiveDamage(float damage, EBodyPart part, EDamageType type, float absorbed, MaterialType special)
	{
		this.OnDamageReceived?.Invoke(damage, part, type, absorbed, special);
	}

	public void ShowAmmoCountZeroingPanel(string message)
	{
		this.OnShowAmmoCountZeroingPanel?.Invoke(message);
	}

	public void VisualPass()
	{
		if (CustomAnimationsAreProcessing)
		{
			return;
		}
		float num = 0f;
		if (!FirstPersonPointOfView)
		{
			num = CameraClass.Instance.Distance(Transform.position);
		}
		bool flag = FirstPersonPointOfView || (BackendConfigAbstractClass.Config.UseSpiritPlayer && !Spirit.IsActive) || (IsVisible && num <= EFTHardSettings.Instance.CULL_GROUNDER);
		if ((_armsupdated || ArmsUpdateMode == EUpdateMode.Auto) && flag && (EnabledAnimators & EAnimatorMask.Procedural) != 0 && !UsedSimplifiedSkeleton)
		{
			ProceduralWeaponAnimation.ProcessEffectors((_nFixedFrames > 0) ? _fixedTime : _armsTime, Mathf.Max(0, _nFixedFrames), Motion, Velocity);
			PlayerBones.Offset = ProceduralWeaponAnimation.HandsContainer.WeaponRootAnim.localPosition;
			PlayerBones.DeltaRotation = ProceduralWeaponAnimation.HandsContainer.WeaponRootAnim.localRotation;
		}
		if (_bodyupdated)
		{
			if (flag && !UsedSimplifiedSkeleton)
			{
				RestoreIKPos();
				HeightInterpolation(_bodyTime);
				FBBIKUpdate(num);
				MouseLook();
				if ((EnabledAnimators & EAnimatorMask.IK) != 0)
				{
					float num2 = (FirstPersonPointOfView ? method_25(PlayerAnimator.FIRST_PERSON_CURVE_WEIGHT) : 1f);
					float positionCacheValue = method_25(PlayerAnimator.POSITION_CACHE_FOR_WEAPON_PROCEDURAL) * num2;
					float num3 = method_25(PlayerAnimator.LEFT_STANCE_CURVE);
					ProceduralWeaponAnimation.GetLeftStanceCurrentCurveValue(num3);
					_firstPersonRightHand = 1f - method_25(PlayerAnimator.RIGHT_HAND_WEIGHT) * num2;
					_firstPersonLeftHand = 1f - method_25(PlayerAnimator.LEFT_HAND_WEIGHT) * num2;
					ThirdPersonWeaponRootAuthority = (MovementContext.IsInMountedState ? 0f : (method_25(PlayerAnimator.WEAPON_ROOT_3RD) * num2));
					if (FirstPersonPointOfView)
					{
						_smoothLW = ((_smoothLW > _firstPersonLeftHand) ? _firstPersonLeftHand : Mathf.SmoothDamp(_smoothLW, _firstPersonLeftHand, ref _shoulderVel, 0.2f));
						if (MovementContext.IsInMountedState && !IsInPronePose)
						{
							PlayerBones.SetShoulders(1f, 1f);
						}
						else
						{
							PlayerBones.SetShoulders(1f - method_25(PlayerAnimator.LEFT_SHOULDER_WEIGHT), 1f - method_25(PlayerAnimator.RIGHT_SHOULDER_WEIGHT));
						}
					}
					else
					{
						method_23(num);
					}
					if (_armsupdated || ArmsUpdateMode == EUpdateMode.Auto)
					{
						float thirdPersonAuthority = ThirdPersonWeaponRootAuthority;
						if (PointOfView == EPointOfView.ThirdPerson && MovementContext.StationaryWeapon != null)
						{
							thirdPersonAuthority = 0f;
						}
						bool inSprint = MovementContext.CurrentState.Name == EPlayerState.Sprint;
						bool lastAnimValue = MovementContext.LeftStanceController.LastAnimValue;
						bool leftStance = MovementContext.LeftStanceController.LeftStance;
						if (MovementContext.PlayerAnimator.AnimatedInteractions.IsInteractionPlaying)
						{
							MovementContext.LeftStanceController.DisableLeftStanceAnimFromBodyAction();
						}
						if (_isInteractionPlayeingLastFrame && !MovementContext.PlayerAnimator.AnimatedInteractions.IsInteractionPlaying)
						{
							MovementContext.LeftStanceController.SetAnimatorLeftStanceToCacheFromBodyAction();
						}
						_isInteractionPlayeingLastFrame = MovementContext.PlayerAnimator.AnimatedInteractions.IsInteractionPlaying;
						PlayerBones.ShiftWeaponRoot(_bodyTime, PointOfView, thirdPersonAuthority, armsupdated: false, positionCacheValue, num3, inSprint, lastAnimValue, leftStance, ProceduralWeaponAnimation.IsAiming, MovementContext.PlayerAnimator.AnimatedInteractions.IsInteractionPlaying, _leftHandController.IsUsing);
					}
					PlayerBones.RotateHead(0f, ProceduralWeaponAnimation.GetHeadRotation(), MovementContext.LeftStanceEnabled && HasFirearmInHands(), num3, ProceduralWeaponAnimation.IsAiming);
					HandPosers[0].weight = _firstPersonLeftHand;
					_limbs[0].solver.IKRotationWeight = (_limbs[0].solver.IKPositionWeight = _firstPersonLeftHand);
					_limbs[1].solver.IKRotationWeight = (_limbs[1].solver.IKPositionWeight = _firstPersonRightHand);
					method_20(num);
					method_24(num2);
					method_19(num);
					if (_firstPersonRightHand < 1f)
					{
						PlayerBones.Kinematics(_markers[1], _firstPersonRightHand);
					}
				}
				float num4 = method_25(PlayerAnimator.AIMING_LAYER_CURVE);
				MovementContext.PlayerAnimator.Animator.SetLayerWeight(6, 1f - num4);
				_prevHeight = Transform.position.y;
			}
			else
			{
				method_14();
				MouseLook();
			}
		}
		if (num > EFTHardSettings.Instance.AnimatorCullDistance)
		{
			BodyAnimatorCommon.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
			ArmsAnimatorCommon.cullingMode = ((!(_handsController is EmptyHandsController) && !(_handsController is KnifeController) && !(_handsController is UsableItemController)) ? AnimatorCullingMode.CullUpdateTransforms : AnimatorCullingMode.AlwaysAnimate);
		}
		else
		{
			BodyAnimatorCommon.cullingMode = AnimatorCullingMode.AlwaysAnimate;
			ArmsAnimatorCommon.cullingMode = AnimatorCullingMode.AlwaysAnimate;
		}
		if (_armsupdated || ArmsUpdateMode == EUpdateMode.Auto)
		{
			ProceduralWeaponAnimation.LateTransformations(Time.deltaTime);
			if (HandsController != null)
			{
				HandsController.ManualLateUpdate(Time.deltaTime);
			}
		}
		if (UsedSimplifiedSkeleton)
		{
			Transform child = PlayerBones.Weapon_Root_Anim.GetChild(0);
			child.localPosition = Vector3.zero;
			child.localRotation = Quaternion.identity;
		}
	}

	public virtual void LateUpdate()
	{
		MovementContext?.AnimatorStatesLateUpdate();
		DistanceDirty = true;
		OcclusionDirty = true;
		if (HealthController != null && HealthController.IsAlive)
		{
			Physical.LateUpdate();
			VisualPass();
			_armsupdated = false;
			_bodyupdated = false;
			if (_nFixedFrames > 0)
			{
				_nFixedFrames = 0;
				_fixedTime = 0f;
			}
			if (_beaconDummy != null)
			{
				Vector3 forward = _playerLookRaycastTransform.forward;
				if (Physics.Raycast(new Ray(_playerLookRaycastTransform.position + forward / 2f, forward), out var hitInfo, 1.5f, LayerMaskClass.HighPolyWithTerrainMask))
				{
					_beaconDummy.transform.position = hitInfo.point;
					_beaconDummy.transform.rotation = Quaternion.LookRotation(hitInfo.normal);
					_beaconMaterialSetter.SetAvailable(_beaconPlacer.Available);
					AllowToPlantBeacon = _beaconPlacer.Available;
					if (AllowToPlantBeacon)
					{
						BeaconPosition = _beaconDummy.transform.position;
						BeaconRotation = _beaconDummy.transform.rotation;
					}
				}
				else
				{
					_beaconDummy.transform.position = _playerLookRaycastTransform.position + _playerLookRaycastTransform.forward;
					_beaconDummy.transform.rotation = Quaternion.identity;
					_beaconMaterialSetter.SetAvailable(isAvailable: false);
					AllowToPlantBeacon = false;
				}
			}
			if (TripwireVisualPlacer_0 != null)
			{
				TripwireVisualPlacer_0.ProcessPlacement(InteractionRay, WeaponRoot.position);
			}
			ProceduralWeaponAnimation.StartFovCoroutine(this);
			PropUpdate();
		}
		ComplexLateUpdate(EUpdateQueue.Update, DeltaTime);
		if (POM != null && !IsAI)
		{
			POM.ExtrudeCamera();
		}
	}

	public void PropUpdate()
	{
		if (!_propActive)
		{
			return;
		}
		if (_hasAnimatorPropBones && _propActive)
		{
			for (int i = 0; i < _animatorPropTransforms.Length; i++)
			{
				_propTransforms[i].SetPositionAndRotation(_animatorPropTransforms[i].position, _animatorPropTransforms[i].rotation);
			}
		}
		if (_firstPersonLeftHand < 1f)
		{
			Quaternion quaternion = Quaternion.Inverse(_markers[0].rotation) * _animatorPropTransforms[0].rotation;
			Vector3 position = _markers[0].InverseTransformPoint(_animatorPropTransforms[0].position);
			_propTransforms[0].position = PlayerBones.LeftPalm.TransformPoint(position);
			_propTransforms[0].rotation = PlayerBones.LeftPalm.rotation * quaternion;
		}
	}

	public void CalculateScaleValueByFov(float fov)
	{
		float t = Mathf.InverseLerp(50f, 75f, fov);
		_ribcageScaleCompensated = Mathf.Lerp(1f, 0.65f, t);
	}

	public void RestoreRibcageScale()
	{
		RibcageScaleCurrentTarget = 1f;
	}

	public void SetCompensationScale(bool force = false)
	{
		RibcageScaleCurrentTarget = _ribcageScaleCompensated;
		if (force)
		{
			RibcageScaleCurrent = RibcageScaleCurrentTarget;
			ProceduralWeaponAnimation.ResetFovAdjustments(this);
		}
		ProceduralWeaponAnimation.SetFovParams(_ribcageScaleCompensated);
	}

	public void OnMakingShot([NotNull] IWeapon weapon, Vector3 force)
	{
		ExecuteSkill((Action)delegate
		{
			Skills.RecoilAction.Complete(weapon.RecoilBase);
		});
		IncreaseAwareness(15f);
		if (AIData != null)
		{
			FirearmController firearmController = HandsController as FirearmController;
			AISoundType spredPower = AISoundType.gun;
			if (firearmController != null && firearmController.IsSilenced)
			{
				spredPower = AISoundType.silencedGun;
			}
			AIData.TryPlayShootSound(GetPlayer, Position, spredPower);
			if (AIData.IsAI)
			{
				AIData.BotOwner.ShootData.ShootDoneWeapon();
			}
		}
		if (FirstPersonPointOfView && IsYourPlayer && GClass3692.IsReflexAvailable() && GClass3692.IsAutomaticReflexAnalyzerSupported())
		{
			CameraClass.Instance.ReflexController.DoReflexTriggerFlash();
		}
		if (FirstPersonPointOfView)
		{
			return;
		}
		_turnOffFbbikAt = Time.time + _fbbikCooldown;
		_fbbik.solver.Quick = false;
		float num = Mathf.Sqrt(weapon.TotalWeight) * weapon.RecoilForceBack / 2400f;
		force *= num;
		foreach (HitReaction.HitPoint item in HitReaction.Recoil)
		{
			item.Hit(force, PlayerBones.WeaponRoot.position);
		}
	}

	public IEnumerator HitDelay(Action callback)
	{
		yield return new WaitForEndOfFrame();
		callback();
	}

	public virtual void ShotReactions(DamageInfoStruct shot, EBodyPart bodyPart)
	{
		if (UsedSimplifiedSkeleton)
		{
			Vector3 normalized = shot.Direction.normalized;
			normalized.y = 0f;
			normalized = normalized.normalized;
			Vector3 vector = Transform.rotation * normalized;
			MovementContext.PlayerAnimator.SetHit((int)bodyPart, 0f - vector.z, 0f - vector.x);
		}
		else
		{
			Vector3 normalized2 = shot.Direction.normalized;
			if (PointOfView == EPointOfView.ThirdPerson)
			{
				_turnOffFbbikAt = Time.time + _fbbikCooldown;
				_fbbik.solver.Quick = false;
				if (shot.HittedBallisticCollider is BodyPartCollider bodyPartCollider)
				{
					HitReaction.Hit(bodyPartCollider.BodyPartColliderType, bodyPartCollider.BodyPartType, normalized2, shot.HitPoint);
				}
			}
			if (shot.Weapon is KnifeItemClass knifeItemClass)
			{
				KnifeComponent itemComponent = knifeItemClass.GetItemComponent<KnifeComponent>();
				Vector3 normalized3 = (shot.Player.iPlayer.Transform.position - Transform.position).normalized;
				Vector3 lhs = Vector3.Cross(normalized3, Vector3.up);
				float y = normalized2.y;
				float num = Vector3.Dot(lhs, normalized2);
				float num2 = 1f - Mathf.Abs(Vector3.Dot(normalized3, normalized2));
				num2 = ((bodyPart == EBodyPart.Head) ? num2 : Mathf.Sqrt(num2));
				Rotation += new Vector2(0f - num, 0f - y).normalized * (TransformHelperClass.Random(itemComponent.Template.AppliedTrunkRotation) * num2);
				ProceduralWeaponAnimation.ForceReact.AddForce(new Vector3(0f - y, num, 0f).normalized, num2, 1f, TransformHelperClass.Random(itemComponent.Template.AppliedHeadRotation));
			}
		}
		if (Singleton<Effects>.Instantiated)
		{
			_preAllocatedRenderersList.Clear();
			_playerBody.GetBodyRenderersNonAlloc(_preAllocatedRenderersList);
			Singleton<Effects>.Instance.EffectsCommutator.PlayerMeshesHit(_preAllocatedRenderersList, shot.HitPoint, -shot.HitNormal);
		}
	}

	public void method_16(EPointOfView pointOfView)
	{
		MovementContext.OnStateChanged += method_17;
		_ribcageChildRotations = new Quaternion[PlayerBones.FovSpecialTransforms.Length];
		_limbs = PlayerBones.Ribcage.Original.GetComponentsInChildren<LimbIK>(includeInactive: true);
		_limbs[0].enabled = false;
		_limbs[1].enabled = false;
		_twistBones = PlayerBones.Ribcage.Original.GetComponentsInChildren<TwistRelax>();
		HandPosers = PlayerBones.Ribcage.Original.GetComponentsInChildren<HandPoser>();
		method_18();
		PointOfView = pointOfView;
		ProceduralWeaponAnimation.PointOfView = pointOfView;
		_fbbik.enabled = false;
		_fbbik.solver.Quick = true;
		Grounder.ik = _fbbik;
		Grounder.enabled = false;
		CameraClass.Instance.FoVUpdateAction += OnFovUpdatedEvent;
		OnFovUpdatedEvent((int)CameraClass.Instance.Fov);
		SubscribeVisualEvents();
	}

	public void method_17(EPlayerState previousState, EPlayerState nextState)
	{
		if (!_isGrenadeOrKnife || HandsAnimator == null)
		{
			return;
		}
		if (FirstPersonPointOfView)
		{
			HandsAnimator.SetPlayerState(ObjectInHandsAnimator.PlayerState.None);
			return;
		}
		switch (nextState)
		{
		case EPlayerState.ProneIdle:
		case EPlayerState.ProneMove:
		case EPlayerState.Transit2Prone:
			HandsAnimator.SetPlayerState(ObjectInHandsAnimator.PlayerState.Prone);
			break;
		default:
			HandsAnimator.SetPlayerState(ObjectInHandsAnimator.PlayerState.Idle);
			break;
		case EPlayerState.Sprint:
			HandsAnimator.SetPlayerState(ObjectInHandsAnimator.PlayerState.Sprint);
			break;
		case EPlayerState.Jump:
			HandsAnimator.SetPlayerState(ObjectInHandsAnimator.PlayerState.Jump);
			break;
		}
	}

	public virtual void OnFovUpdatedEvent(int fov)
	{
		if (HealthController.IsAlive)
		{
			CalculateScaleValueByFov(fov);
			SetCompensationScale(force: true);
		}
	}

	public virtual void OnHealthEffectVisualAdded(IEffect effect)
	{
		if (effect is GInterface340 && Singleton<Effects>.Instantiated)
		{
			Singleton<Effects>.Instance.EffectsCommutator.StartBleedingForPlayer(this);
		}
	}

	public virtual void OnHealthEffectVisualRemoved(IEffect effect)
	{
		if (effect is GInterface340 && Singleton<Effects>.Instantiated)
		{
			Singleton<Effects>.Instance.EffectsCommutator.StopBleedingForPlayer(this);
		}
	}

	public virtual void OnPlayerVisualDied(EDamageType damageType)
	{
		if (Singleton<Effects>.Instantiated)
		{
			Singleton<Effects>.Instance.EffectsCommutator.StopBleedingForPlayer(this);
		}
	}

	public void SubscribeVisualEvents()
	{
		HealthController.EffectStartedEvent += OnHealthEffectVisualAdded;
		HealthController.EffectResidualEvent += OnHealthEffectVisualRemoved;
		HealthController.DiedEvent += OnPlayerVisualDied;
	}

	public void UnsubscribeVisualEvents()
	{
		HealthController.EffectStartedEvent -= OnHealthEffectVisualAdded;
		HealthController.EffectResidualEvent -= OnHealthEffectVisualRemoved;
		HealthController.DiedEvent -= OnPlayerVisualDied;
	}

	public void SwitchHeadLightsAnimation()
	{
		if (StateIsSuitableForHandInput && !IsHeadLightsAnimationActive)
		{
			MovementContext.SetInteractInHands(EInteraction.HelmetRailGear);
		}
		IsHeadLightsAnimationActive = true;
		StartCoroutine(method_140());
	}

	public virtual void MouseLook(bool forceApplyToOriginalRibcage = false)
	{
		if (!BackendConfigAbstractClass.Config.UseSpiritPlayer || !Spirit.IsActive || forceApplyToOriginalRibcage)
		{
			MovementContext.RotationAction?.Invoke(this);
		}
	}

	public void method_18()
	{
		CameraContainer = TransformHelperClass.FindTransform(Transform.Original, "CameraContainer").gameObject;
		CameraPosition = TransformHelperClass.FindTransform(Transform.Original, "Cam");
		ProceduralWeaponAnimation = PlayerBones.Ribcage.Original.GetComponent<ProceduralWeaponAnimation>();
		ProceduralWeaponAnimation.CameraContainer = CameraContainer;
		ProceduralWeaponAnimation.Walk.Speed = MovementContext.CharacterMovementSpeed;
		ProceduralWeaponAnimation.Breath.Physical = Physical;
		ProceduralWeaponAnimation.HandShakeEffector.Physical = Physical;
		ProceduralWeaponAnimation.OnPreCollision += IkStoreRaw;
		MovementContext.OnPoseChanged += delegate(int i)
		{
			ProceduralWeaponAnimation.Pose = i;
		};
		OnHealthEffectRemoved(null);
	}

	public void UpdateLauncherBones(bool launcherEnable, WeaponPrefab weaponPrefab)
	{
		if (launcherEnable)
		{
			Transform launcherRoot = weaponPrefab.transform;
			Transform propBone = PlayerBones.WeaponRoot.Original.GetComponentInChildren<AlternativePropBone>().transform;
			ProceduralWeaponAnimation.SetLauncherWeaponBone(weaponPrefab.transform, propBone);
			HandsController.HandsHierarchy.GatherUnderbarrelWeaponIK(launcherRoot, _elbowBends);
		}
		else
		{
			UpdateBonesOnWeaponChange(HandsController.HandsHierarchy);
		}
	}

	public void UpdateFirstPersonGrip(GripPose.EGripType type = GripPose.EGripType.Common, TransformLinks transforms = null)
	{
		HandPosers[0].GripWeight = 0f;
		if (transforms != null)
		{
			HandPosers[0].MapGrip(transforms.GetTransform(ECharacterWeaponBones.HumanLPalm));
			HandPosers[1].MapGrip(transforms.GetTransform(ECharacterWeaponBones.HumanRPalm));
			HandPoser obj = HandPosers[0];
			HandPoser obj2 = HandPosers[1];
			float weight = 1f;
			obj2.weight = 1f;
			obj.weight = weight;
			ProceduralWeaponAnimation.HandsContainer.CameraAnimatedFP = transforms.GetTransform(ECharacterWeaponBones.Camera_animated);
		}
		else
		{
			HandPoser obj3 = HandPosers[0];
			HandPoser obj4 = HandPosers[1];
			float weight = 0f;
			obj4.weight = 0f;
			obj3.weight = weight;
		}
		GripPose[] source = (from x in PlayerBones.WeaponRoot.Original.GetComponentsInChildren<GripPose>()
			where x.GripType == type || x.GripType == GripPose.EGripType.UnderbarrelWeapon
			select x).ToArray();
		GripPose gripPose = (from x in source
			where x.Hand == GripPose.EHand.Left
			orderby x.GripType == GripPose.EGripType.UnderbarrelWeapon descending, HandPoser.NumParents(x.transform, PlayerBones.WeaponRoot.Original) descending
			select x).FirstOrDefault();
		GripPose gripPose2 = (from x in source
			where x.Hand == GripPose.EHand.Right
			orderby HandPoser.NumParents(x.transform, PlayerBones.WeaponRoot.Original) descending
			select x).FirstOrDefault();
		HandPosers[0].SetGrip(gripPose);
		HandPosers[1].SetGrip(gripPose2);
		HandPosers[1].IgnoreIndexFinger = gripPose2 != null;
		_ikTargets = new Transform[2]
		{
			gripPose ? gripPose.transform : null,
			gripPose2 ? gripPose2.transform : null
		};
	}

	public void UpdateBonesOnWeaponChange(TransformLinks links)
	{
		PlayerBones.UpdateImportantBones(links);
		TransformHelperClass.SetLayersRecursively<MeshRenderer>(links.gameObject, LayerMask.NameToLayer("Player"), new string[1] { "Shells" });
		_elbowBends = new Transform[2];
		links.GatherIK(_markers, _gripReferences, _elbowBends);
		_propBone = links.GetTransformOutOfRangeSafe(ECharacterWeaponBones.prop);
		_hasAnimatorPropBones = _propBone != null;
		if (_hasAnimatorPropBones)
		{
			_animatorPropTransforms[0] = _propBone;
			_animatorPropTransforms[1] = _propBone.GetChild(0);
			_animatorPropTransforms[2] = _propBone.GetChild(1);
		}
		_vestMarker = ((links.Transforms.Length > 18) ? links.GetTransform(ECharacterWeaponBones.weapon_vest_IK_marker) : null);
		UpdateFirstPersonGrip(GripPose.EGripType.Common, links);
	}

	public void FBBIKUpdate(float distance)
	{
		if ((EnabledAnimators & EAnimatorMask.FBBIK) == 0)
		{
			return;
		}
		if (PointOfView == EPointOfView.ThirdPerson)
		{
			_fbbik.solver.iterations = (int)Mathf.Clamp(15f / distance, 0f, 2f);
			if (!_fbbik.solver.Quick && Time.time > _turnOffFbbikAt)
			{
				_fbbik.solver.Quick = true;
			}
		}
		_fbbik.solver.Update();
	}

	public void method_19(float d)
	{
		if (d > 300f)
		{
			return;
		}
		_limbs[0].solver.Update();
		if (d > 70f)
		{
			return;
		}
		_limbs[1].solver.Update();
		bool skip;
		if (!(skip = d > 40f))
		{
			TwistRelax[] twistBones = _twistBones;
			for (int i = 0; i < twistBones.Length; i++)
			{
				twistBones[i].Relax();
			}
		}
		HandPosers[0].ManualUpdate(skip);
		HandPosers[1].ManualUpdate(skip);
	}

	public Vector3 ProjectLocalPosition(Vector3 position)
	{
		return PlayerBones.WeaponRoot.TransformPoint(PlayerBones.Weapon_Root_Anim.InverseTransformPoint(position));
	}

	public void DropItemDead(Item item, GameObject prefab)
	{
		GClass4062.ReleaseBeginSample("Player.DropItemDead", "DropItemDead");
		int layer = LayerMask.NameToLayer("Deadbody");
		int num = LayerMask.NameToLayer("Shells");
		AssetPoolObject[] componentsInChildren = prefab.GetComponentsInChildren<AssetPoolObject>(includeInactive: true);
		Collider collider = null;
		AssetPoolObject[] array = componentsInChildren;
		foreach (AssetPoolObject assetPoolObject in array)
		{
			foreach (Collider collider2 in assetPoolObject.Colliders)
			{
				if (!collider2.isTrigger && collider2.gameObject.layer != num)
				{
					assetPoolObject.StoreCollider(collider2);
					collider2.enabled = true;
					collider2.gameObject.layer = layer;
					if (collider == null || collider.bounds.extents.sqrMagnitude < collider2.bounds.extents.sqrMagnitude)
					{
						collider = collider2;
					}
				}
			}
		}
		_garbage = new GClass2004
		{
			Transform = prefab.transform
		};
		Rigidbody rigidbody = prefab.AddComponent<Rigidbody>();
		LootItem component = prefab.GetComponent<LootItem>();
		if (component != null)
		{
			component.SetItemAndRigidbody(item, rigidbody);
		}
		_garbage.Shift = ((rigidbody != null) ? rigidbody.centerOfMass : Vector3.zero);
		if ((bool)collider)
		{
			_garbage.Transportee = collider.gameObject.AddComponent<CommonTransportee>();
			_garbage.Transportee.ParentTransform = prefab.transform;
		}
		foreach (Transform item2 in prefab.transform)
		{
			item2.localPosition -= _garbage.Shift;
		}
		TransformLinks componentInChildren = prefab.GetComponentInChildren<TransformLinks>();
		bool flag;
		if (!(flag = item is PistolItemClass || item is ThrowWeapItemClass || item.GetItemComponent<KnifeComponent>() != null))
		{
			HandPosers[1].Lerp2Target(EFTHardSettings.Instance.RIGHT_HAND_QTS, 5f);
		}
		RigidbodySpawner obj = (flag ? PlayerBones.Forearms[1].GetComponent<RigidbodySpawner>() : GetComponentInChildren<RigidbodySpawner>());
		Corpse.Ragdoll.AttachWeapon(rigidbody, base.gameObject, PlayerBones, componentInChildren, flag, Velocity);
		obj.RemoveEvent += RemoveAttachment;
	}

	public void RemoveAttachment(RigidbodySpawner spawner)
	{
		if (spawner != null)
		{
			spawner.RemoveEvent -= RemoveAttachment;
		}
		_garbage?.RemovePhysics();
	}

	public void ReleaseHand()
	{
		if (_garbage != null)
		{
			_garbage.Destroy();
			_garbage = null;
		}
		method_118();
		HandPosers[1].Lerp2Target(EFTHardSettings.Instance.RIGHT_HAND_QTS, 5f);
		ProceduralWeaponAnimation.OnPreCollision -= IkStoreRaw;
	}

	public void SpawnInHands(Item item, string parentBone)
	{
		_spawnedKey = Singleton<PoolManagerClass>.Instance.CreateItem(item, GetVisibleToCamera(this), this, isAnimated: true);
		Transform transform = TransformHelperClass.FindTransform(_spawnedKey.transform, "pivot");
		Transform transform2 = TransformHelperClass.FindTransform(_limbs[0].solver.bone3.transform, parentBone);
		_spawnedKey.transform.SetParent(transform2, worldPositionStays: false);
		_spawnedKey.transform.localRotation = Quaternion.identity;
		_spawnedKey.transform.localPosition = Vector3.zero;
		_spawnedKey.SetActive(value: true);
		if (transform != null)
		{
			Quaternion quaternion = Quaternion.Inverse(transform.rotation) * transform2.rotation;
			_spawnedKey.transform.localRotation *= quaternion;
			Vector3 vector = transform2.position - transform.position;
			_spawnedKey.transform.position += vector;
		}
		else
		{
			UnityEngine.Debug.LogError("pivot not found in " + _spawnedKey?.ToString() + " for keyId = " + item.Id);
		}
		AudioClip itemClip = Singleton<GUISounds>.Instance.GetItemClip(item.ItemSound, EInventorySoundType.pickup);
		if (itemClip != null)
		{
			BetterSource source = MonoBehaviourSingleton<BetterAudio>.Instance.PlayAtPoint(Transform.position, itemClip, BetterAudio.AudioSourceGroupType.Collisions, 30, 1f, EOcclusionTest.None, null, !FirstPersonPointOfView);
			if (!FirstPersonPointOfView && MonoBehaviourSingleton<SpatialAudioSystem>.Instantiated)
			{
				MonoBehaviourSingleton<SpatialAudioSystem>.Instance.ProcessSourceOcclusion(this, source);
			}
		}
	}

	public void ClearPlanting()
	{
		TripwireVisualPlacer_0.ClearPlanting();
		TripwireVisualPlacer_0.gameObject.SetActive(value: false);
	}

	public TripwireVisualPlacer CreatePlantPlanner()
	{
		if (TripwireVisualPlacer_0 == null)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/tripwire_planner") as GameObject);
			gameObject.gameObject.SetActive(value: true);
			gameObject.name = "PlantPlaner";
			TripwireVisualPlacer_0 = gameObject.GetComponent<TripwireVisualPlacer>();
		}
		TripwireVisualPlacer_0.gameObject.SetActive(value: true);
		TripwireVisualPlacer_0.transform.position = WeaponRoot.position;
		return TripwireVisualPlacer_0;
	}

	public void InitFirstTripwirePoint()
	{
		TripwireVisualPlacer_0.InitFirstPoint(InteractionRay);
	}

	public GameObject CreateBeacon(Item item, Vector3 position)
	{
		if (_beaconDummy == null)
		{
			GameObject original = Singleton<PoolManagerClass>.Instance.CreateLootPrefab(item, ECameraType.Default);
			_beaconDummy = UnityEngine.Object.Instantiate(original, position, Quaternion.identity);
			_beaconDummy.gameObject.SetActive(value: true);
			_beaconDummy.name = "BeaconDummy";
			AssetPoolObject component = _beaconDummy.GetComponent<AssetPoolObject>();
			foreach (Collider collider in component.Colliders)
			{
				collider.enabled = false;
			}
			BoxCollider boxCollider = _beaconDummy.AddComponent<BoxCollider>();
			boxCollider.enabled = false;
			component.RegisteredComponentsToClean.Add(boxCollider);
			_beaconMaterialSetter = _beaconDummy.AddComponent<PreviewMaterialSetter>();
			_beaconMaterialSetter.SetAvailable(isAvailable: true);
			component.RegisteredComponentsToClean.Add(_beaconMaterialSetter);
			_beaconPlacer = _beaconDummy.AddComponent<BeaconPlacer>();
		}
		return _beaconDummy;
	}

	public void DestroyBeacon()
	{
		if (!(_beaconDummy == null))
		{
			UnityEngine.Object.Destroy(_beaconDummy);
			_beaconDummy = null;
			_beaconMaterialSetter = null;
			_beaconPlacer = null;
		}
	}

	public void ClearHands()
	{
		if (!(_spawnedKey == null))
		{
			AssetPoolObject.ReturnToPool(_spawnedKey);
			_spawnedKey = null;
		}
	}

	public void RestoreIKPos()
	{
		if (!_stored)
		{
			return;
		}
		_interactionLayerWeight = HandsAnimator?.GetLayerWeight(HandsAnimator.LACTIONS_LAYER_INDEX) ?? 0f;
		_rawWeight = ((_rawWeight < _interactionLayerWeight) ? _interactionLayerWeight : Mathf.SmoothDamp(_rawWeight, _interactionLayerWeight, ref _rawDampVelocity, 0.2f));
		if (_rawWeight > 0.005f)
		{
			if (_hasAnimatorPropBones && _hasProp)
			{
				_propBone.position = _propRawPosition;
				_propBone.rotation = _propRawRotation;
			}
			_markers[0].position = Vector3.Lerp(_markers[0].position, _lMarkerRawPosition, _rawWeight);
			_markers[0].rotation = Quaternion.Slerp(_markers[0].rotation, _lMarkerRawRotation, _rawWeight);
			_elbowBends[0].position = _lElbowRawPosition;
		}
		_stored = false;
	}

	public void IkStoreRaw()
	{
		if (_hasAnimatorPropBones && _hasProp)
		{
			_propRawPosition = _propBone.position;
			_propRawRotation = _propBone.rotation;
		}
		if (!(_markers[0] == null))
		{
			_lMarkerRawPosition = _markers[0].position;
			_lMarkerRawRotation = _markers[0].rotation;
			if (_elbowBends != null && _elbowBends.Length != 0)
			{
				_lElbowRawPosition = _elbowBends[0].position;
				_rElbowRawPosition = _elbowBends[1].position;
				_stored = true;
			}
		}
	}

	public void method_20(float distance2Camera)
	{
		for (int i = 0; i < 2; i++)
		{
			if (!(_markers[i] == null) && !(Math.Abs(_limbs[i].solver.IKPositionWeight) < float.Epsilon))
			{
				if (_ikTargets[i] != null && distance2Camera < 40f)
				{
					float value = Vector3.Distance(_markers[i].position, _gripReferences[i].position);
					float num = Mathf.InverseLerp(0.1f, 0f, value);
					HandPosers[i].GripWeight = num;
					_ikPosition = Vector3.Lerp(_markers[i].position, _ikTargets[i].position, num);
					_ikRotation = Quaternion.Lerp(_markers[i].rotation, _ikTargets[i].rotation, num);
				}
				else
				{
					_ikPosition = _markers[i].position;
					_ikRotation = _markers[i].rotation;
				}
				if (LeftHandInteractionTarget != null && i == 0)
				{
					_ikPosition = Vector3.Lerp(_ikPosition, LeftHandInteractionTarget.transform.position, ThirdIkWeight.Value);
					_ikRotation = Quaternion.Slerp(_ikRotation, LeftHandInteractionTarget.transform.rotation, ThirdIkWeight.Value);
				}
				_limbs[i].solver.SetIKPosition(_ikPosition);
				_limbs[i].solver.SetIKRotation(_ikRotation);
			}
		}
	}

	public RuntimeAnimatorController CreateAnimatorController()
	{
		return GClass1857.GetAsset<RuntimeAnimatorController>(Singleton<IEasyAssets>.Instance, UsedSimplifiedSkeleton ? ResourceKeyManagerAbstractClass.ZOMBIE_ANIMATOR_CONTROLLER : ResourceKeyManagerAbstractClass.PLAYER_DEFAULT_ANIMATOR_CONTROLLER);
	}

	public void method_21()
	{
		if (Profile.Side == EPlayerSide.Savage)
		{
			_animators[0].SetLayerWeight(16, 0f);
			if (Profile.Info.Settings.Role == WildSpawnType.bossBoar)
			{
				_animators[0].runtimeAnimatorController = GClass1857.GetAsset<RuntimeAnimatorController>(Singleton<IEasyAssets>.Instance, ResourceKeyManagerAbstractClass.BOSS_KABAN_ANIMATOR_CONTROLLER);
				_animators[0].SetLayerWeight(16, 1f);
			}
		}
	}

	public void method_22()
	{
		if (!BackendConfigAbstractClass.Config.UseBodyFastAnimator && !BackendConfigAbstractClass.Config.UseSpiritPlayer && !UsedSimplifiedSkeleton)
		{
			method_21();
		}
		if (_animators[0].runtimeAnimatorController == null && !BackendConfigAbstractClass.Config.UseBodyFastAnimator)
		{
			RuntimeAnimatorController runtimeAnimatorController = CreateAnimatorController();
			_animators[0].runtimeAnimatorController = runtimeAnimatorController;
		}
		if (BackendConfigAbstractClass.Config.UseSpiritPlayer)
		{
			bool useFastAnimator = BotSettingsRepoClass.ShallUseFastAnimator(Profile.Info.Settings.Role) && BackendConfigAbstractClass.Config.UseSpiritFastAnimator;
			Spirit.InitBodyAnimator(_animators[0].updateMode, useFastAnimator);
		}
	}

	public virtual bool UpdateGrenadeAnimatorDuePoV()
	{
		return PointOfView == EPointOfView.ThirdPerson;
	}

	public void SetEnvironment(string profileID, EnvironmentType environmentType)
	{
		if (!(profileID != ProfileId))
		{
			Environment = environmentType;
		}
	}

	public void method_23(float distance)
	{
		if (!(distance > 70f))
		{
			if (_markers[0] != null && _vestMarker != null)
			{
				bool flag = ((_handsController is FirearmController firearmController && firearmController.IsInReloadOperation()) || _handsController.IsInventoryOpen()) && !IsSprintEnabled;
				_utilityLayerWeight = (flag ? Mathf.InverseLerp(UtilityLayerRange.x, UtilityLayerRange.y, Vector3.Distance(_markers[0].position, _vestMarker.position)) : Mathf.Lerp(_utilityLayerWeight, 0f, Time.deltaTime * UtilityLayerLerpSpeed));
				BodyAnimatorCommon.SetLayerWeight(2, _utilityLayerWeight);
			}
			else
			{
				BodyAnimatorCommon.SetLayerWeight(2, 0f);
			}
		}
	}

	public void method_24(float curveWeight)
	{
		if (_elbowBends != null && !(_elbowBends[0] == null))
		{
			Quaternion quaternion = Quaternion.Euler(0.65f * MovementContext.Pitch, MovementContext.Yaw, 0f);
			Vector3 vector = quaternion * ProceduralWeaponAnimation.TurnAway.RElbowShift;
			Vector3 vector2 = quaternion * ProceduralWeaponAnimation.TurnAway.LElbowShift;
			float num = curveWeight * method_25(PlayerAnimator.ELBOW_LEFT_WEIGHT);
			if (num < 1f)
			{
				PlayerBones.BendGoals[0].position = Vector3.Lerp(_elbowBends[0].position + vector2, PlayerBones.BendGoals[0].position, num);
			}
			float num2 = curveWeight * method_25(PlayerAnimator.ELBOW_RIGHT_WEIGHT);
			if (num2 < 1f)
			{
				PlayerBones.BendGoals[1].position = Vector3.Lerp(_elbowBends[1].position + vector, PlayerBones.BendGoals[1].position, num2);
			}
		}
	}

	public float method_25(int hash)
	{
		return Mathf.Min(_animators[0].GetFloat(hash), 1f);
	}

	public bool method_26()
	{
		foreach (KeyValuePair<EBoundItem, Item> boundItem in Inventory.FastAccess.BoundItems)
		{
			if (boundItem.Value is CompassItemClass)
			{
				return true;
			}
		}
		return false;
	}

	public void CreateCompass()
	{
		if (!_compassInstantiated && method_26())
		{
			Transform transform = Singleton<PoolManagerClass>.Instance.CreateFromPool<Transform>(new ResourceKey
			{
				path = "assets/content/weapons/additional_hands/item_compass.bundle"
			});
			transform.SetParent(PlayerBones.Ribcage.Original, worldPositionStays: false);
			transform.localRotation = Quaternion.identity;
			transform.localPosition = Vector3.zero;
			method_27(transform.gameObject);
			_compassInstantiated = true;
		}
	}

	public void method_27(GameObject obj)
	{
		_hasProp = obj != null;
		if (_hasProp)
		{
			_compassArrow = obj.GetComponentInChildren<CompassArrow>();
			_compassArrow.NorthDirection = Singleton<LevelSettings>.Instance.NorthVector;
			_compassArrow.enabled = true;
			_propTransforms[0] = obj.transform;
			_propTransforms[1] = TransformHelperClass.FindTransform(obj.transform, "prop_bone_001");
			_propTransforms[2] = TransformHelperClass.FindTransform(obj.transform, "prop_bone_002");
			obj.SetActive(value: false);
		}
		else
		{
			_propTransforms = new Transform[3];
			_propActive = false;
			if ((bool)_compassArrow)
			{
				_compassArrow.enabled = false;
				_compassArrow = null;
			}
		}
	}

	public void SetPropVisibility(bool isVisible)
	{
		if (_playerBody != null && FirstPersonPointOfView)
		{
			this.OnPropVisibility?.Invoke(isVisible);
		}
		if (_hasProp && _hasAnimatorPropBones)
		{
			_propTransforms[0].gameObject.SetActive(isVisible);
			_propTransforms[0].transform.SetPositionAndRotation(_animatorPropTransforms[0].position, _animatorPropTransforms[0].rotation);
			_compassArrow.enabled = isVisible;
			_propActive = isVisible;
		}
	}

	public void OnRadiolocationZoneEnter()
	{
		UnityEngine.Debug.Log(Profile.Nickname + "enter in radiolocation zone on client");
	}

	public void OnRadiolocationZoneExit()
	{
		UnityEngine.Debug.Log(Profile.Nickname + "exit from radiolocation zone on client");
	}

	public virtual bool IsVisibleByCullingObject(float cullingDistance)
	{
		return true;
	}

	public virtual void SetAlertedFloat(float alertedFloat)
	{
		if (UsedSimplifiedSkeleton)
		{
			if (Mathf.Approximately(alertedFloat, 1f))
			{
				MovementContext.PlayerAnimatorEnableInert(enabled: false);
			}
			MovementContext.PlayerAnimator.SetAlert(Mathf.Approximately(alertedFloat, 1f));
			MovementContext.PlayerAnimator.SetAlertFloat(alertedFloat);
		}
	}

	public virtual void TriggerZombieLost()
	{
		if (UsedSimplifiedSkeleton)
		{
			MovementContext.PlayerAnimatorEnableInert(enabled: false);
			MovementContext.PlayerAnimator.TriggerIsLost();
		}
	}

	public virtual void InitAudioController()
	{
		method_28();
		SetAudioProtagonist();
		_sourcePrewarmer = new GClass885();
		CompositeDisposable.AddDisposable(SearchController.SearchOperations.ItemsChanged.Bind(method_45));
		_stepLayerMask = LayerMaskClass.AudioControllerStepLayerMask;
		_soundBySurface = new Dictionary<BaseBallistic.ESurfaceSound, SurfaceSet>();
		IEasyAssets instance = Singleton<IEasyAssets>.Instance;
		_hearingSettings = GClass1857.GetAsset<FirstPersonPlayerHearingSettings>(instance, "assets/content/audio/prefabs/character/firstpersonplayerhearingsettings.bundle");
		_playerSounds = GClass1857.GetAsset<Sounds>(instance, "assets/content/audio/prefabs/movement/sounds.bundle");
		_gearSoundBank = _playerSounds.Gear;
		_gearMediumSoundBank = _playerSounds.GearMedium;
		_gearFastSoundBank = _playerSounds.GearFast;
		_backpackDropBank = _playerSounds.BackpackDrop;
		_tinnitus = _playerSounds.TinnitusSound;
		FaceshieldOn = _playerSounds.FaceShieldOn;
		FaceshieldOff = _playerSounds.FaceShieldOff;
		NightVisionOn = _playerSounds.NightVisionOn;
		NightVisionOff = _playerSounds.NightVisionOff;
		ThermalVisionOn = _playerSounds.ThermalVisionOn;
		ThermalVisionOff = _playerSounds.ThermalVisionOff;
		SwitchHeadlights = _playerSounds.SwitchHeadlights;
		FractureSound = _playerSounds.FractureSound;
		_animatorFootstepCurveHash = BodyAnimatorCommon.StringToHash("FootStep");
		PropIn = (_playerSounds.PropIn ? new BaseSoundPlayer.SoundElement
		{
			SoundClips = new AudioClip[1] { _playerSounds.PropIn },
			RollOff = 10
		} : null);
		PropOut = (_playerSounds.PropOut ? new BaseSoundPlayer.SoundElement
		{
			SoundClips = new AudioClip[1] { _playerSounds.PropOut },
			RollOff = 10
		} : null);
		SurfaceSet[] sets = _playerSounds.Sets;
		foreach (SurfaceSet surfaceSet in sets)
		{
			if (!_soundBySurface.ContainsKey(surfaceSet.Surface))
			{
				_soundBySurface.Add(surfaceSet.Surface, surfaceSet);
			}
			else
			{
				UnityEngine.Debug.LogError(surfaceSet.Surface.ToString() + " surface sounds are duplicated");
			}
		}
		_currentSet = _soundBySurface[BaseBallistic.ESurfaceSound.Concrete];
		MovementContext.OnStateChanged += method_55;
		MovementContext movementContext = MovementContext;
		movementContext.OnGrounded = (Action<float, float>)Delegate.Combine(movementContext.OnGrounded, new Action<float, float>(method_5));
		_healthController.ApplyDamageEvent += method_74;
		_healthController.DiedEvent += method_52;
		_healthController.EffectStartedEvent += method_53;
		_healthController.EffectRemovedEvent += method_54;
		InitAudioSources();
		_idleCoroutine = StartCoroutine(method_72());
		_exhaustionAudibilityUnsub = Physical.SubscribeToAudibleEffects(method_36);
		GenericEventTranslator eventTranslator = EventTranslator;
		eventTranslator.OnSoundBankPlay = (Action<string>)Delegate.Combine(eventTranslator.OnSoundBankPlay, new Action<string>(PlaySoundBank));
		foreach (KeyValuePair<BaseBallistic.ESurfaceSound, SurfaceSet> item in _soundBySurface)
		{
			_runSurfaceCheck = Math.Max(_runSurfaceCheck, item.Value.RunSoundBank.Rolloff);
			_sprintSurfaceCheck = Math.Max(_sprintSurfaceCheck, item.Value.SprintSoundBank.Rolloff);
			_landSurfaceCheck = Mathf.Max(_landSurfaceCheck, item.Value.LandingSoundBank.Rolloff);
			_proneSurfaceCheck = Mathf.Max(_proneSurfaceCheck, item.Value.ProneSoundBank.Rolloff);
		}
		Speaker.OnRelease += OnSpeakerRelease;
		FaceShieldObserver.Changed.Bind(PlayFaceShieldSound);
		NightVisionObserver.Changed.Bind(PlayNightVisionSound);
		ThermalVisionObserver.Changed.Bind(PlayThermalVisionSound);
		Muffled = false;
		if (FaceShieldObserver.Component != null)
		{
			method_38();
		}
		if (FaceCoverObserver.Component != null)
		{
			method_39();
		}
		method_29();
		CreateGearSource();
		method_41();
		PlayerMountingPointData playerMountingPointData = MovementContext.PlayerMountingPointData;
		playerMountingPointData.OnEnterMountedState = (Action<float>)Delegate.Combine(playerMountingPointData.OnEnterMountedState, new Action<float>(method_77));
		PlayerMountingPointData playerMountingPointData2 = MovementContext.PlayerMountingPointData;
		playerMountingPointData2.OnExitMountedState = (Action<float>)Delegate.Combine(playerMountingPointData2.OnExitMountedState, new Action<float>(method_78));
		method_42();
		_priorityCalculator = new GClass1180(200);
	}

	public void method_28()
	{
		GClass1706 instance = Singleton<GClass1706>.Instance;
		if (instance != null)
		{
			_playerAudioSettings = instance.AudioSettings.PlayerSettings;
		}
	}

	public void method_29()
	{
		Class443.OnInitialized -= method_30;
		if (Class443.Controller != null)
		{
			method_30(Class443.Controller);
			return;
		}
		UnityEngine.Debug.Log("winter controller initialization failed, try to subscribe");
		Class443.OnInitialized += method_30;
	}

	public void method_30(GInterface29 controller)
	{
		Class443.OnInitialized -= method_30;
		method_31(controller.Status);
		controller.StatusChangedEvent += method_31;
	}

	public void method_31(ESeasonStatus seasonStatus)
	{
		if (!_playerSounds.TryGetSeasonMovementSet(seasonStatus, out var set))
		{
			UnityEngine.Debug.Log($"Can't find movement sound set for season: {seasonStatus}");
			return;
		}
		if (MonoBehaviourSingleton<AmbientAudioSystem>.Instantiated)
		{
			_useSimpleUnderRoofCheck = MonoBehaviourSingleton<AmbientAudioSystem>.Instance.UseSimpleUnderRoofCheck;
		}
		else
		{
			UnityEngine.Debug.LogWarning("Ambient Audio System not init, use default under roof check for step layer");
		}
		bool useOcclusion = PointOfView == EPointOfView.ThirdPerson && this is LocalPlayer;
		_specificStepAudioController = new LocalPlayerStepAudioControllerClass(set, this, 0.1f, useOcclusion, seasonStatus);
		method_35();
	}

	public void InitVaultingAudioControllers(IVaultingParameters vaultingParams)
	{
		IPlayerAnimatorEvents playerAnimatorEvents = MovementContext.PlayerAnimator.EventsDispatcher.PlayerAnimatorEvents;
		Sounds asset = GClass1857.GetAsset<Sounds>(Singleton<IEasyAssets>.Instance, "assets/content/audio/prefabs/movement/sounds.bundle");
		_vaultAudioController = new GClass2682(asset, MonoBehaviourSingleton<BetterAudio>.Instance, MonoBehaviourSingleton<SpatialAudioSystem>.Instance, playerAnimatorEvents.VaultingSoundsEvents, vaultingParams, this, EVaultingSoundType.Vault);
		_sprintVaultAudioController = new GClass2682(asset, MonoBehaviourSingleton<BetterAudio>.Instance, MonoBehaviourSingleton<SpatialAudioSystem>.Instance, playerAnimatorEvents.SprintVaultSoundsEvents, vaultingParams, this, EVaultingSoundType.SprintVault);
		_climbAudioController = new GClass2682(asset, MonoBehaviourSingleton<BetterAudio>.Instance, MonoBehaviourSingleton<SpatialAudioSystem>.Instance, playerAnimatorEvents.ClimbSoundsEvents, vaultingParams, this, EVaultingSoundType.Climb);
	}

	public void method_32()
	{
		_vaultAudioController?.Dispose();
		_sprintVaultAudioController?.Dispose();
		_climbAudioController?.Dispose();
	}

	public void PlayTripwireInteractionSound(float plantTime, bool hasMultiTool)
	{
		_tripwireInteractionSoundController.PlayInteractionSound(hasMultiTool);
		SendTripwireInteractionSoundState(EInteractionStatus.Started, isSuccess: true, hasMultiTool);
	}

	public void StopTripwireInteractionSound(bool isSuccess, bool hasMultiTool)
	{
		_tripwireInteractionSoundController.StopInteractionSound(isSuccess, hasMultiTool);
		SendTripwireInteractionSoundState(EInteractionStatus.Finished, isSuccess, hasMultiTool);
	}

	public virtual void PlayToggleSound(ref bool previousState, bool isOn, AudioClip toggleOn, AudioClip toggleOff)
	{
		if (previousState != isOn)
		{
			Singleton<BetterAudio>.Instance.PlayAtPoint(Transform.Original.position + SpeechLocalPosition, isOn ? toggleOn : toggleOff, Distance, BetterAudio.AudioSourceGroupType.Character, 5);
		}
		previousState = isOn;
	}

	public void PlayTacticalSound()
	{
		Singleton<BetterAudio>.Instance.PlayAtPoint(Transform.Original.position + SpeechLocalPosition, SwitchHeadlights, Distance, BetterAudio.AudioSourceGroupType.Character, 5);
	}

	public void PlayFaceShieldSound()
	{
		FaceShieldComponent component = FaceShieldObserver.Component;
		bool isOn = component != null && (component.Togglable == null || component.Togglable.On);
		PlayToggleSound(ref PreviousFaceShield, isOn, FaceshieldOn, FaceshieldOff);
	}

	public void PlayNightVisionSound()
	{
		NightVisionComponent component = NightVisionObserver.Component;
		bool isOn = component != null && (component.Togglable == null || component.Togglable.On);
		PlayToggleSound(ref PreviousNightVision, isOn, NightVisionOn, NightVisionOff);
	}

	public void PlayThermalVisionSound()
	{
		ThermalVisionComponent component = ThermalVisionObserver.Component;
		bool isOn = component != null && (component.Togglable == null || component.Togglable.On);
		PlayToggleSound(ref PreviousThermalVision, isOn, ThermalVisionOn, ThermalVisionOff);
	}

	public virtual void SetAudioProtagonist()
	{
	}

	public void AddVolume(BetterPropagationVolume volume)
	{
		_soundPropagationVolumes.Add(volume);
		if (volume.MutuallyExclusive)
		{
			_mutuallyExclusive = volume;
		}
	}

	public void RemoveVolume(BetterPropagationVolume volume)
	{
		int num = _soundPropagationVolumes.IndexOf(volume);
		if (num >= 0)
		{
			_soundPropagationVolumes.RemoveAt(num);
		}
		_mutuallyExclusive = _soundPropagationVolumes.FirstOrDefault((BetterPropagationVolume x) => x.MutuallyExclusive);
	}

	public List<BetterPropagationVolume> GetPropagationVolume()
	{
		_volumesBuffer.Clear();
		if (_mutuallyExclusive != null)
		{
			_volumesBuffer.Add(_mutuallyExclusive);
		}
		else
		{
			_volumesBuffer.AddRange(_soundPropagationVolumes);
		}
		return _volumesBuffer;
	}

	public void ToggleMuteSpeechSource(bool muteSpeech)
	{
		SpeechSource.source1.mute = muteSpeech;
	}

	public void PlaySpeechFromTime(TaggedClip clip, float time)
	{
		method_68(SpeechSource, clip.Falloff);
		if (!method_34(SpeechSource.MaxDistance))
		{
			UpdateMuffledState();
			SpeechSource.SetActive(active: true);
			method_44(SpeechSource);
			SpeechSource.source1.spatialBlend = (GClass2078.IsFirstPerson(PointOfView) ? 0f : 1f);
			_speechSource.HrtfIntensity = (GClass2078.IsFirstPerson(PointOfView) ? 0f : 1f);
			_speechSource.DirectivityIntensity = (GClass2078.IsFirstPerson(PointOfView) ? 0f : 0.5f);
			SpeechSource.source1.time = time;
			float volume = clip.Volume;
			SpeechSource.Play(clip.Clip, null, 1f, volume, GClass2078.IsFirstPerson(PointOfView), oneShot: false);
		}
	}

	public void method_33(TaggedClip clip)
	{
		PlaySpeechFromTime(clip, 0f);
	}

	public virtual void UpdateMuffledState()
	{
		if (!OcclusionDirty || !MonoBehaviourSingleton<BetterAudio>.Instantiated)
		{
			return;
		}
		OcclusionDirty = false;
		BetterAudio instance = MonoBehaviourSingleton<BetterAudio>.Instance;
		AudioMixerGroup mixerGroup = ((PointOfView == EPointOfView.FirstPerson) ? instance.ClientPlayerSpeechMixer : instance.ObservedPlayerSpeechMixer);
		if (PointOfView == EPointOfView.ThirdPerson)
		{
			if (Muffled)
			{
				mixerGroup = instance.SimpleOccludedMixerGroup;
			}
		}
		else
		{
			mixerGroup = (Muffled ? instance.SelfSpeechReverb : instance.ClientPlayerSpeechMixer);
		}
		SpeechSource.SetMixerGroup(mixerGroup);
	}

	public virtual bool CheckSurface(float range)
	{
		if (method_34(range))
		{
			return false;
		}
		var (hit, surfaceSound) = method_75();
		method_76(hit, surfaceSound);
		if (Environment == EnvironmentType.Outdoor)
		{
			method_35();
		}
		return true;
	}

	public bool method_34(float spreadRange)
	{
		if (FirstPersonPointOfView)
		{
			return false;
		}
		float maxDistance = spreadRange * ProtagonistHearing + CHECK_RANGE_BUFF;
		return !GClass2313.IsInRange(Position, maxDistance);
	}

	public void method_35()
	{
		_specificStepAudioController.UpdateUnderRoofStatus(IsUnderRoof);
		SendUnderRoofStatus(IsUnderRoof);
	}

	public void method_36()
	{
		bool breathIsAudible;
		if ((breathIsAudible = Physical.BreathIsAudible) != _exhaustionIsAudible)
		{
			_exhaustionIsAudible = breathIsAudible;
			HeavyBreath = false;
			UpdateBreathStatus();
		}
	}

	public virtual void UpdateBreathStatus()
	{
		ETagStatus healthStatus = HealthStatus;
		bool flag2;
		bool flag = (flag2 = (healthStatus == ETagStatus.BadlyInjured || healthStatus == ETagStatus.Dying) && HealthController.FindActiveEffect<GInterface358>() == null) || _exhaustionIsAudible || Muffled;
		if (!HeavyBreath && flag)
		{
			ETagStatus eTagStatus = (flag2 ? healthStatus : ETagStatus.Healthy);
			ETagStatus eTagStatus2 = ((!_exhaustionIsAudible) ? ETagStatus.Unaware : ETagStatus.Aware);
			if (eTagStatus == ETagStatus.Healthy && eTagStatus2 == ETagStatus.Unaware)
			{
				Speaker.Play(EPhraseTrigger.OnBreath, eTagStatus | eTagStatus2, demand: true, -1);
			}
			else
			{
				Speaker.Play(EPhraseTrigger.OnBreath, eTagStatus | eTagStatus2, demand: true);
			}
		}
		HeavyBreath = flag;
	}

	public void OnSpeakerRelease(bool force)
	{
		HeavyBreath = false;
		if (_healthController.IsAlive)
		{
			UpdateBreathStatus();
		}
		if (!HeavyBreath || force)
		{
			method_37();
		}
	}

	public void method_37()
	{
		if (!(_speechSource == null))
		{
			_speechSource.SetParent(null);
			_speechSource.Release();
			_speechSource = null;
		}
	}

	public void method_38()
	{
		FaceShieldComponent component = FaceShieldObserver.Component;
		method_40(component, EquipmentSlot.Headwear);
		method_89();
	}

	public void method_39()
	{
		FaceShieldComponent component = FaceCoverObserver.Component;
		method_40(component, EquipmentSlot.FaceCover);
		method_89();
	}

	public void method_40(FaceShieldComponent fs, EquipmentSlot equipmentSlot)
	{
		Muffled = false;
		bool flag = false;
		bool flag2;
		if (flag2 = fs != null && (fs.Togglable == null || fs.Togglable.On))
		{
			flag = Equipment.GetSlot(equipmentSlot).ContainedItem is CompoundItem thisItem && GClass3380.GetItemComponentsInChildren<CompositeArmorComponent>(thisItem).SelectMany((CompositeArmorComponent x) => x.ArmorColliders).Contains(EBodyPartColliderType.Jaw);
		}
		Muffled = flag2 && flag;
		if (FirstPersonPointOfView)
		{
			UpdateBreathStatus();
			if (!flag2 && Speaker != null && !HeavyBreath && Speaker.Importance == 0)
			{
				Speaker.Shut();
			}
		}
		UpdateMuffledState();
		SendVoiceMuffledState(Muffled);
	}

	public virtual void SendVoiceMuffledState(bool isMuffled)
	{
	}

	public virtual void InitAudioSources()
	{
		CreateNestedSource();
	}

	public virtual void CreateNestedSource()
	{
		BetterAudio instance = MonoBehaviourSingleton<BetterAudio>.Instance;
		NestedStepSoundSource = instance.GetSource(BetterAudio.AudioSourceGroupType.Character);
		if ((object)NestedStepSoundSource != null)
		{
			bool flag = PointOfView == EPointOfView.FirstPerson;
			NestedStepSoundSource.SetMixerGroup(flag ? instance.ClientPlayerMovementMixer : instance.ObservedPlayerMovementMixer);
			NestedStepSoundSource.EnabledEQ(!flag);
			if (!flag && MonoBehaviourSingleton<SpatialAudioSystem>.Instantiated)
			{
				MonoBehaviourSingleton<SpatialAudioSystem>.Instance.ProcessSourceOcclusion(this, NestedStepSoundSource, method_80());
			}
			NestedStepSoundSource.EnableSpatialization = !flag;
			NestedStepSoundSource.HrtfIntensity = (flag ? 0f : 1f);
			NestedStepSoundSource.SetParent(Transform.Original, worldPositionStay: false);
			NestedStepSoundSource.LocalPosition = new Vector3(0f, 0.1f, 0f);
			method_66(EAudioMovementState.Run);
			method_44(NestedStepSoundSource);
		}
	}

	public virtual void CreateSpeechSource()
	{
		BetterAudio instance = MonoBehaviourSingleton<BetterAudio>.Instance;
		_speechSource = instance.GetSource(BetterAudio.AudioSourceGroupType.Speech, activateSource: false);
		if ((object)_speechSource != null)
		{
			bool flag = PointOfView == EPointOfView.FirstPerson;
			_speechSource.EnabledEQ(!flag);
			_speechSource.SetMixerGroup(flag ? instance.ClientPlayerSpeechMixer : instance.ObservedPlayerSpeechMixer);
			if (!flag && MonoBehaviourSingleton<SpatialAudioSystem>.Instantiated)
			{
				MonoBehaviourSingleton<SpatialAudioSystem>.Instance.ProcessSourceOcclusion(this, _speechSource, method_80());
			}
			_speechSource.EnableSpatialization = !flag;
			_speechSource.StartTrackingPosition(PlayerBones.Head.Original);
			_speechSource.LocalPosition = PlayerBones.Head.localPosition;
			_speechSource.transform.rotation = PlayerBones.Head.localRotation;
			method_44(_speechSource);
		}
	}

	public void CreateGearSource()
	{
		if (!(_gearSource != null))
		{
			BetterAudio instance = MonoBehaviourSingleton<BetterAudio>.Instance;
			_gearSource = instance.GetSource(BetterAudio.AudioSourceGroupType.Character, activateSource: false);
			bool flag = PointOfView == EPointOfView.FirstPerson;
			_gearSource.EnabledEQ(!flag);
			if (!flag && MonoBehaviourSingleton<SpatialAudioSystem>.Instantiated)
			{
				MonoBehaviourSingleton<SpatialAudioSystem>.Instance.ProcessSourceOcclusion(this, _gearSource, method_80());
			}
			_gearSource.SetBaseVolume(flag ? 0.85f : 1f);
			_gearSource.HrtfIntensity = (flag ? 0f : 1f);
			_gearSource.EnableSpatialization = !flag;
			_gearSource.SetParent(Transform.Original, worldPositionStay: false);
			_gearSource.LocalPosition = new Vector3(0f, 0.1f, 0f);
			_gearSource.SetMixerGroup(flag ? instance.ClientPlayerMovementMixer : instance.ObservedPlayerMovementMixer);
		}
	}

	public void method_41()
	{
		_tripwireInteractionSoundController = new GClass2580(this);
	}

	public void method_42()
	{
		IPlayerAnimatorEvents playerAnimatorEvents = MovementContext.PlayerAnimator.EventsDispatcher.PlayerAnimatorEvents;
		_dropBackPackEvents = playerAnimatorEvents.DropBackpackEvents;
		_dropBackPackEvents.OnBackpackDropEvent += method_79;
	}

	public void method_43(BetterSource source)
	{
		if (!GClass2078.IsFirstPerson(PointOfView))
		{
			_sourcePrewarmer.ProcessPlayPrewarmSound(source, Distance);
		}
	}

	public void method_44(BetterSource source)
	{
		if (!GClass2078.IsFirstPerson(PointOfView))
		{
			_sourcePrewarmer.PlayPrewarmSound(source);
		}
	}

	public IEnumerator SupportAudioSourceCoroutine()
	{
		while (_searchCount > 0f && _healthController.IsAlive)
		{
			float num = (GClass2078.IsFirstPerson(PointOfView) ? 0f : CameraClass.Instance.SqrDistance(Position));
			float num2 = method_69(EAudioMovementState.Search) * method_69(EAudioMovementState.Search);
			if (num <= num2)
			{
				BetterSource betterSource = method_46(_lastClip);
				AudioMixerGroup mixerGroup = (GClass2078.IsFirstPerson(PointOfView) ? MonoBehaviourSingleton<BetterAudio>.Instance.ClientPlayerMovementMixer : MonoBehaviourSingleton<BetterAudio>.Instance.ObservedPlayerMovementMixer);
				float volume = _playerAudioSettings.SearchSoundVolume.GetVolume(PointOfView);
				betterSource.SetMixerGroup(mixerGroup);
				betterSource.SetBaseVolume(volume);
				if (!betterSource.source1.isPlaying)
				{
					betterSource.SetActive(active: true);
					betterSource.Play(betterSource.GetClip(0), null, 1f, volume, forceStereo: true, oneShot: false);
				}
			}
			else
			{
				method_47();
			}
			yield return new WaitForSeconds(0.5f);
		}
		method_47();
	}

	public void method_45()
	{
		_searchCount = SearchController.SearchOperations.Count();
		if (_searchCount > 0f)
		{
			if (_currentSourceCoroutine != null)
			{
				StopCoroutine(_currentSourceCoroutine);
			}
			AudioClip lootingClip;
			try
			{
				string searchSound = SearchController.SearchOperations.Last().Item.SearchSound;
				lootingClip = Singleton<GUISounds>.Instance.GetLootingClip(searchSound);
			}
			catch (Exception)
			{
				GInterface155<SearchContentOperation> searchOperations = SearchController.SearchOperations;
				SearchableItemItemClass searchableItemItemClass = (searchOperations.Any() ? searchOperations.Last().Item : null);
				string text = searchableItemItemClass?.SearchSound;
				string text2 = searchableItemItemClass?.ShortName;
				UnityEngine.Debug.LogError($"SearchEventNRE1: searchOp {searchOperations.Count()} item is null {searchableItemItemClass == null} itemSound {text} itemName {text2}");
				return;
			}
			if (!(lootingClip == null))
			{
				BetterSource betterSource = method_46(lootingClip);
				try
				{
					betterSource.Loop = true;
					betterSource.Position = MovementContext.PlayerColliderCenter + MovementContext.TransformForwardVector / 4f;
					method_68(betterSource, method_67(EAudioMovementState.Search));
				}
				catch (Exception)
				{
					UnityEngine.Debug.LogError($"SearchEventNRE2: source is null {betterSource == null} Movement context is null {MovementContext == null}");
					return;
				}
				_currentSourceCoroutine = StartCoroutine(SupportAudioSourceCoroutine());
				_lastClip = lootingClip;
			}
		}
		else
		{
			method_47();
			if (_currentSourceCoroutine != null)
			{
				StopCoroutine(_currentSourceCoroutine);
				_currentSourceCoroutine = null;
			}
		}
	}

	public BetterSource method_46(AudioClip clip)
	{
		if (_searchSource == null)
		{
			_searchSource = MonoBehaviourSingleton<BetterAudio>.Instance.GetSource(BetterAudio.AudioSourceGroupType.Character, activateSource: false);
			_searchSource.EnabledEQ(!GClass2078.IsFirstPerson(PointOfView));
			if (_searchSource != null)
			{
				if (PointOfView == EPointOfView.ThirdPerson && MonoBehaviourSingleton<SpatialAudioSystem>.Instantiated)
				{
					MonoBehaviourSingleton<SpatialAudioSystem>.Instance.ProcessSourceOcclusion(this, _searchSource, method_80());
				}
				method_44(_searchSource);
				_searchSource.EnableSpatialization = PointOfView == EPointOfView.ThirdPerson;
				_searchSource.source1.clip = clip;
			}
		}
		return _searchSource;
	}

	public void method_47()
	{
		if (!(_searchSource == null))
		{
			_searchSource.Stop();
			_searchSource.Release();
			_searchSource = null;
		}
	}

	public void PlayInteractionSound(AudioClip clip, float volume = 1f, bool loop = true, bool stereo = true)
	{
		if (GClass2078.IsFirstPerson(PointOfView))
		{
			StopInteractionSound();
			_interactionSource = MonoBehaviourSingleton<BetterAudio>.Instance.GetSource(BetterAudio.AudioSourceGroupType.Nonspatial);
			_interactionSource.Position = Position;
			_interactionSource.Loop = loop;
			_interactionSource.source1.clip = clip;
			_interactionSource.SetBaseVolume(volume);
			_interactionSource.EnableStereo(stereo);
			_interactionSource.Play(clip, null, 1f, volume, stereo, oneShot: false);
		}
	}

	public void StopInteractionSound(float fadeTime = 0f)
	{
		if (_interactionSource == null)
		{
			return;
		}
		if (fadeTime > 0f && GClass2078.IsFirstPerson(PointOfView))
		{
			_interactionSource.VolumeFadeOut(fadeTime, delegate
			{
				_interactionSource.Stop();
				_interactionSource.Release();
				_interactionSource = null;
			});
		}
		else
		{
			method_143();
		}
	}

	public virtual void PlayGroundedSound(float fallHeight, float jumpHeight)
	{
		if (!(Time.realtimeSinceStartup < _nextJumpAfter) && method_48())
		{
			float num = fallHeight;
			if (!method_49())
			{
				num = Mathf.Max(fallHeight, jumpHeight);
			}
			if (num > LandingThreshold && CheckSurface(_landSurfaceCheck))
			{
				method_43(NestedStepSoundSource);
				float volume = Mathf.InverseLerp(0.1f, LandingThreshold * 2.5f, num);
				DefaultPlay(_currentSet.LandingSoundBank, volume, EAudioMovementState.Land);
				_nextJumpAfter = Time.realtimeSinceStartup + 0.5f;
			}
		}
	}

	public bool method_48()
	{
		EPlayerState ePlayerState = MovementContext.PreviousState?.Name ?? EPlayerState.None;
		EPlayerState ePlayerState2 = MovementContext.CurrentState?.Name ?? EPlayerState.None;
		if (ePlayerState2 != EPlayerState.Run && ePlayerState2 != EPlayerState.Sprint)
		{
			if (ePlayerState != EPlayerState.ClimbUp && ePlayerState2 != EPlayerState.ClimbUp)
			{
				return ePlayerState != EPlayerState.VaultingLanding;
			}
			return false;
		}
		return true;
	}

	public bool method_49()
	{
		return (MovementContext.PreviousState?.Name ?? EPlayerState.None) == EPlayerState.ClimbUp;
	}

	public void method_50()
	{
		if (NestedStepSoundSource != null && !_enqueuedForRelease)
		{
			_enqueuedForRelease = true;
			NestedStepSoundSource.SetParent(null);
			if (MonoBehaviourSingleton<BetterAudio>.Exist(out var component))
			{
				component.AddToAudioSourceQueue(NestedStepSoundSource, AudioSettings.dspTime + 1.0);
			}
			NestedStepSoundSource = null;
		}
		_exhaustionAudibilityUnsub?.Invoke();
		_exhaustionAudibilityUnsub = null;
		_specificStepAudioController.Dispose();
	}

	public void method_51()
	{
		if (_gearSource != null)
		{
			_gearSource.SetParent(null);
			_gearSource.Release();
			_gearSource = null;
		}
	}

	public void method_52(EDamageType damageType)
	{
		Coroutine[] array = new Coroutine[3] { _sprintCoroutine, _runCoroutine, _idleCoroutine };
		foreach (Coroutine coroutine in array)
		{
			if (coroutine != null)
			{
				StopCoroutine(coroutine);
			}
		}
		method_50();
		method_32();
		method_51();
		StopInteractionSound();
		Class443.OnInitialized -= method_30;
		if (Class443.Controller != null)
		{
			Class443.Controller.StatusChangedEvent -= method_31;
		}
	}

	public void method_53(IEffect healthEffect)
	{
		_damageThresholdAudioChecker.AddHealthEffect(healthEffect);
	}

	public void method_54(IEffect healthEffect)
	{
		_damageThresholdAudioChecker.RemoveHealthEffect(healthEffect);
	}

	public void method_55(EPlayerState previousState, EPlayerState nextstate)
	{
		method_43(NestedStepSoundSource);
		switch (previousState)
		{
		case EPlayerState.Idle:
		case EPlayerState.IdleZombieState:
		case EPlayerState.TurnZombieState:
			if (_idleCoroutine != null)
			{
				StopCoroutine(_idleCoroutine);
			}
			break;
		case EPlayerState.Run:
		case EPlayerState.MoveZombieState:
		case EPlayerState.StartMoveZombieState:
		case EPlayerState.EndMoveZombieState:
			if (_runCoroutine == null)
			{
				break;
			}
			StopCoroutine(_runCoroutine);
			if (!_playedAtLeastOneStep && SinceLastStep > 0.66f)
			{
				if (CheckSurface(_runSurfaceCheck))
				{
					PlayStepSound();
				}
				_lastStepTime = Time.time;
			}
			break;
		case EPlayerState.Sprint:
			if (_sprintCoroutine != null)
			{
				StopCoroutine(_sprintCoroutine);
				if (!_playedAtLeastOneStep && CheckSurface(_sprintSurfaceCheck))
				{
					DefaultPlay(_currentSet.SprintSoundBank, 1f, EAudioMovementState.Sprint);
				}
			}
			if (nextstate == EPlayerState.Transition || nextstate == EPlayerState.Idle)
			{
				float num = (FirstPersonPointOfView ? _currentSet.StopSoundBank.BaseVolume : 1f);
				DefaultPlay(_currentSet.StopSoundBank, num * MovementContext.CovertMovementVolume, EAudioMovementState.Stop);
			}
			break;
		}
		switch (nextstate)
		{
		case EPlayerState.Prone2Stand:
			method_60(0.7f, fast: true);
			break;
		case EPlayerState.Sprint:
			_sprintCoroutine = StartCoroutine(method_71(nextstate));
			break;
		case EPlayerState.Jump:
			DefaultPlay(_currentSet.JumpSoundBank, 1f, EAudioMovementState.Jump);
			method_60(MovementContext.CovertEquipmentNoise, fast: true);
			break;
		case EPlayerState.Idle:
		case EPlayerState.IdleZombieState:
		case EPlayerState.TurnZombieState:
			_idleCoroutine = StartCoroutine(method_72(nextstate));
			break;
		case EPlayerState.Run:
		case EPlayerState.MoveZombieState:
		case EPlayerState.StartMoveZombieState:
		case EPlayerState.EndMoveZombieState:
			_runCoroutine = StartCoroutine(method_73(nextstate));
			break;
		case EPlayerState.Transit2Prone:
		{
			EAudioMovementState movementState = ((previousState == EPlayerState.Sprint) ? EAudioMovementState.Drop : EAudioMovementState.None);
			float volume = 0.7f * MovementContext.CovertMovementVolume;
			if (previousState == EPlayerState.Sprint)
			{
				DefaultPlay(_currentSet.ProneDropSoundBank, volume, movementState);
			}
			else
			{
				method_60(volume, fast: true);
			}
			break;
		}
		}
	}

	public void DefaultPlay(SoundBank bank, float volume = 1f, EAudioMovementState movementState = EAudioMovementState.None)
	{
		if (bank == null)
		{
			string arg = ((_currentSet != null) ? _currentSet.ToString() : "None");
			UnityEngine.Debug.LogError($"Bank is null for state {movementState}. Current set is {arg}");
			return;
		}
		UpdateMuffledState();
		method_65(NestedStepSoundSource);
		method_66(movementState);
		volume *= method_64(movementState);
		bank.Play(NestedStepSoundSource, EnvironmentType.Outdoor, Distance, volume, Distance, FirstPersonPointOfView);
		_specificStepAudioController.Play(movementState, Environment, Distance, volume, Distance, FirstPersonPointOfView);
	}

	public void PlayStepSound()
	{
		UpdateMuffledState();
		SoundBank soundBank = ((Pose == EPlayerPose.Duck) ? _currentSet.DuckSoundBank : _currentSet.RunSoundBank);
		EAudioMovementState movementState = ((Pose != EPlayerPose.Duck) ? EAudioMovementState.Run : EAudioMovementState.Duck);
		float clampedSpeed = MovementContext.ClampedSpeed;
		float covertMovementVolumeBySpeed = MovementContext.CovertMovementVolumeBySpeed;
		clampedSpeed = Mathf.Max(Physical.MinStepSound, clampedSpeed) * covertMovementVolumeBySpeed;
		float num = method_57();
		float num2 = method_64(movementState);
		float num3 = ((FirstPersonPointOfView || method_80()) ? soundBank.RandomVolume : 1f);
		float num4 = covertMovementVolumeBySpeed * num * num2 * num3;
		method_66(movementState, includeSpeedMult: true);
		soundBank.Play(NestedStepSoundSource, EnvironmentType.Outdoor, Distance, num4, clampedSpeed, FirstPersonPointOfView);
		_specificStepAudioController.Play(movementState, Environment, Distance, num4, clampedSpeed, FirstPersonPointOfView);
		float num5 = method_56(clampedSpeed);
		if (num5 > 0f)
		{
			StartCoroutine(method_70(num5));
		}
	}

	public float method_56(float speed)
	{
		if (Pose != EPlayerPose.Duck)
		{
			return Mathf.Clamp(speed * 0.75f * Mathf.Sqrt(MovementContext.PoseLevel), 0.1f, 0.5f);
		}
		return Mathf.Clamp(speed, 0f, 0.3f);
	}

	public float method_57()
	{
		return Mathf.Clamp(method_59(), _playerAudioSettings.MinSpeedVolumeMult, 1f);
	}

	public float method_58()
	{
		float t = method_59();
		return Mathf.Lerp(_playerAudioSettings.MinSpeedRolloffMult, 1f, t);
	}

	public float method_59()
	{
		return Mathf.InverseLerp(0f, MovementContext.MaxSpeed, MovementContext.CharacterMovementSpeed);
	}

	public void method_60(float volume = 1f, bool fast = false)
	{
		SoundBank bank = (fast ? _gearFastSoundBank : _gearSoundBank);
		method_61(bank, volume);
	}

	public void method_61(SoundBank bank, float volume = 1f)
	{
		if (_healthController.IsAlive)
		{
			UpdateMuffledState();
			float rolloff = method_67(EAudioMovementState.Gear);
			volume *= method_64(EAudioMovementState.Gear) * MovementContext.CovertEquipmentNoise;
			_gearSource.SetActive(active: true);
			method_68(_gearSource, rolloff);
			bank.Play(_gearSource, EnvironmentType.Outdoor, Distance, volume, Distance, FirstPersonPointOfView);
		}
	}

	public void method_62(float speed = 55f)
	{
		if (Time.time - _lastTimeTurnSound >= maxLengthTurnSound)
		{
			float num = Mathf.InverseLerp(1f, 360f + (1f - MovementContext.PoseLevel) * 360f, method_63());
			SoundBank turnSoundBank = _currentSet.TurnSoundBank;
			float volume = num * MovementContext.CovertMovementVolume * turnSoundBank.BaseVolume;
			DefaultPlay(turnSoundBank, volume, EAudioMovementState.Turn);
			_lastTimeTurnSound = Time.time;
			if (num > 0.4f)
			{
				method_60(volume);
			}
		}
	}

	public float method_63()
	{
		return Mathf.Max(UsedSimplifiedSkeleton ? 45f : 0f, MovementContext.AverageRotationSpeed.Avarage);
	}

	public void PlaySoundBank(string soundBank)
	{
		if (soundBank == "Prone" && !(SinceLastStep < 0.5f) && CheckSurface(_proneSurfaceCheck))
		{
			UpdateMuffledState();
			method_65(NestedStepSoundSource);
			SoundBank proneSoundBank = _currentSet.ProneSoundBank;
			float b = MovementContext.CovertMovementVolume * MovementContext.ClampedSpeed * _currentSet.ProneSoundBank.BaseVolume;
			float num = Mathf.Max(0.4f, b);
			num *= method_64(EAudioMovementState.Prone);
			method_66(EAudioMovementState.Prone);
			proneSoundBank.Play(NestedStepSoundSource, EnvironmentType.Outdoor, Distance, num, Distance, FirstPersonPointOfView);
			_specificStepAudioController.Play(EAudioMovementState.Prone, Environment, Distance, num, Distance, FirstPersonPointOfView);
			_lastStepTime = Time.time;
		}
	}

	public float method_64(EAudioMovementState movementState)
	{
		float num = (GClass2078.IsFirstPerson(PointOfView) ? 70f : _playerAudioSettings.BaseMaxMovementRolloff);
		return method_69(movementState) / num * Single_1;
	}

	public void method_65(BetterSource source)
	{
		int priority = _priorityCalculator.CalculatePriority(Distance, source.MaxDistance);
		source.SetPriority(priority);
	}

	public void method_66(EAudioMovementState movementState = EAudioMovementState.None, bool includeSpeedMult = false)
	{
		method_68(NestedStepSoundSource, method_67(movementState, includeSpeedMult));
		_specificStepAudioController.UpdateSoundRolloff(NestedStepSoundSource.MaxDistance);
	}

	public float method_67(EAudioMovementState movementState = EAudioMovementState.None, bool includeSpeedMult = false)
	{
		float num = (includeSpeedMult ? method_58() : 1f);
		float num2 = (GClass2078.IsFirstPerson(PointOfView) ? 70f : _playerAudioSettings.BaseMaxMovementRolloff);
		float multByMovement = _playerAudioSettings.GetMultByMovement(movementState);
		float num3 = num2 * ProtagonistHearing * Physical.SoundRadius * multByMovement * num;
		_cachedMovementRolloff[movementState] = num3;
		return num3;
	}

	public void method_68(BetterSource source, float rolloff)
	{
		float rolloff2 = rolloff * _playerAudioSettings.GetRolloffMultByEnvironment(Environment) * ProtagonistHearing;
		source.SetRolloff(rolloff2);
	}

	public float method_69(EAudioMovementState movementState)
	{
		if (!_cachedMovementRolloff.TryGetValue(movementState, out var value))
		{
			return method_67(movementState);
		}
		return value;
	}

	public IEnumerator method_70(float volume = 1f)
	{
		yield return _gearWalkDelaySec;
		if (_gearSource != null)
		{
			method_60(volume);
		}
	}

	public IEnumerator method_71(EPlayerState state = EPlayerState.Sprint)
	{
		_playedAtLeastOneStep = false;
		while (CurrentState.Name == state)
		{
			float single_ = Single_0;
			if (Math.Abs(_sign - single_) >= float.Epsilon)
			{
				_sign = single_;
				float num = Time.time - _lastStepTime;
				if (num > 0.2f && MovementContext.FreefallTime < 0.6f)
				{
					_playedAtLeastOneStep = true;
					_lastStepTime = Time.time;
					method_65(NestedStepSoundSource);
					if (CheckSurface(_sprintSurfaceCheck))
					{
						UpdateMuffledState();
						SoundBank sprintSoundBank = _currentSet.SprintSoundBank;
						float num2 = (FirstPersonPointOfView ? sprintSoundBank.BaseVolume : 1f);
						float num3 = 0.5f + 3f * Physical.Overweight;
						float num4 = method_64(EAudioMovementState.Sprint) * num2 * num3;
						method_66(EAudioMovementState.Sprint);
						_currentSet.SprintSoundBank.Play(NestedStepSoundSource, EnvironmentType.Outdoor, Distance, num4, Distance, FirstPersonPointOfView);
						_specificStepAudioController.Play(EAudioMovementState.Sprint, Environment, Distance, num4, Distance, FirstPersonPointOfView);
						method_60();
						if (num < 1.2f && FirstPersonPointOfView)
						{
							ProceduralWeaponAnimation.Walk.StepFrequency = 0.5f / Mathf.Clamp(num, 0.3f, 0.8f);
						}
					}
				}
			}
			yield return null;
		}
	}

	public IEnumerator method_72(EPlayerState state = EPlayerState.Idle)
	{
		while (CurrentState.Name == state && HealthController.IsAlive)
		{
			float num = Math.Abs(HandsToBodyAngle);
			if (num > EFTHardSettings.Instance.TURN_ANGLE)
			{
				method_62(num);
				yield return new WaitForSeconds(EFTHardSettings.Instance.TURN_SOUND_DELAY);
			}
			yield return null;
		}
	}

	public IEnumerator method_73(EPlayerState state = EPlayerState.Run)
	{
		_playedAtLeastOneStep = false;
		while (CurrentState.Name == state)
		{
			float single_ = Single_0;
			if (Math.Abs(_sign - single_) >= float.Epsilon)
			{
				_sign = single_;
				float sinceLastStep = SinceLastStep;
				if (sinceLastStep > 0.2f && MovementContext.FreefallTime < 1f)
				{
					_lastStepTime = Time.time;
					_playedAtLeastOneStep = true;
					if (CheckSurface(_runSurfaceCheck))
					{
						if (sinceLastStep < 1.2f && FirstPersonPointOfView)
						{
							ProceduralWeaponAnimation.Walk.StepFrequency = 0.5f / Mathf.Clamp(sinceLastStep, 0.7f - MovementContext.SmoothedCharacterMovementSpeed / 2f, 1.2f);
						}
						PlayStepSound();
					}
				}
			}
			yield return null;
		}
	}

	public void method_74(EBodyPart bodyPart, float damage, DamageInfoStruct damageInfo)
	{
		EDamageType damageType = damageInfo.DamageType;
		if (!IsAI && damageType == EDamageType.Fall && damage > MIN_FALL_DAMAGE)
		{
			Say(EPhraseTrigger.OnBeingHurt, demand: true);
		}
		else
		{
			if (MovementContext.PhysicalConditionIs(EPhysicalCondition.OnPainkillers) && !(damage > 4f))
			{
				return;
			}
			if (GClass3051.IsSelfInflicted(damageType))
			{
				bool flag = HealthController.FindActiveEffect<GInterface358>() != null;
				if (((HealthStatus != ETagStatus.BadlyInjured && HealthStatus != ETagStatus.Dying) || flag) && _damageThresholdAudioChecker.TryReachThreshold(damageType))
				{
					Say(EPhraseTrigger.OnBeingHurt, demand: true);
				}
			}
			else
			{
				Say(EPhraseTrigger.OnBeingHurt, demand: true);
			}
		}
	}

	public (bool hit, BaseBallistic.ESurfaceSound surfaceSound) method_75()
	{
		Vector3 playerColliderCenter = MovementContext.PlayerColliderCenter;
		float num = MovementContext.CharacterController.height + 0.5f;
		Vector3 endPos = playerColliderCenter + Vector3.down * num;
		BaseBallistic.ESurfaceSound item = BaseBallistic.ESurfaceSound.Concrete;
		if (!GClass943.GetNearestHit(playerColliderCenter, endPos, out var hitInfo, num, _stepLayerMask))
		{
			return (hit: false, surfaceSound: item);
		}
		if (hitInfo.collider == null)
		{
			return (hit: false, surfaceSound: item);
		}
		BaseBallistic component = hitInfo.collider.GetComponent<BaseBallistic>();
		if (component != null)
		{
			item = component.GetSurfaceSound(hitInfo.point);
		}
		return (hit: true, surfaceSound: item);
	}

	public void method_76(bool hit, BaseBallistic.ESurfaceSound surfaceSound)
	{
		if (hit)
		{
			if (CurrentSurface != surfaceSound)
			{
				CurrentSurface = surfaceSound;
				_currentSet = _soundBySurface[surfaceSound];
				_specificStepAudioController.UpdateSurface(surfaceSound);
			}
			MovementContext.SoftSurface = CurrentSurface == BaseBallistic.ESurfaceSound.Asphalt || CurrentSurface == BaseBallistic.ESurfaceSound.Concrete || CurrentSurface == BaseBallistic.ESurfaceSound.Gravel || CurrentSurface == BaseBallistic.ESurfaceSound.Soil || CurrentSurface == BaseBallistic.ESurfaceSound.Wood || CurrentSurface == BaseBallistic.ESurfaceSound.WoodThick || CurrentSurface == BaseBallistic.ESurfaceSound.Puddle;
		}
	}

	public void method_77(float timeToMount)
	{
		method_61(_gearMediumSoundBank, _gearMediumSoundBank.RandomVolume);
	}

	public void method_78(float timeToUnmount)
	{
		method_61(_gearMediumSoundBank, _gearMediumSoundBank.RandomVolume);
	}

	public void method_79(IAnimatorEventParameter animatorEventParameter)
	{
		method_61(_backpackDropBank, _backpackDropBank.RandomVolume);
	}

	public bool method_80()
	{
		if (!FirstPersonPointOfView)
		{
			return UsedSimplifiedSkeleton;
		}
		return false;
	}

	public virtual void SendUnderRoofStatus(bool isUnderRoof)
	{
	}

	public virtual void SendTripwireInteractionSoundState(EInteractionStatus interactionStatus, bool isSuccess, bool hasMultiTool)
	{
	}

	public void method_81()
	{
		_soundUnsubscribeOnDestroy?.Invoke();
		_soundUnsubscribeOnDestroy = null;
		if (Class443.Controller != null)
		{
			Class443.Controller.StatusChangedEvent -= method_31;
		}
		_tripwireInteractionSoundController?.Dispose();
		if (MovementContext != null)
		{
			PlayerMountingPointData playerMountingPointData = MovementContext.PlayerMountingPointData;
			playerMountingPointData.OnEnterMountedState = (Action<float>)Delegate.Remove(playerMountingPointData.OnEnterMountedState, new Action<float>(method_77));
			PlayerMountingPointData playerMountingPointData2 = MovementContext.PlayerMountingPointData;
			playerMountingPointData2.OnExitMountedState = (Action<float>)Delegate.Remove(playerMountingPointData2.OnExitMountedState, new Action<float>(method_78));
		}
		if (_dropBackPackEvents != null)
		{
			_dropBackPackEvents.OnBackpackDropEvent -= method_79;
		}
		if (_healthController != null)
		{
			_healthController.ApplyDamageEvent -= method_74;
			_healthController.DiedEvent -= method_52;
			_healthController.EffectStartedEvent -= method_53;
			_healthController.EffectRemovedEvent -= method_54;
		}
	}

	public static TPlayer Create<TPlayer>(GameWorld gameWorld, ResourceKey assetName, int playerId, Vector3 position, EUpdateQueue updateQueue, EUpdateMode armsUpdateMode, EUpdateMode bodyUpdateMode, CharacterControllerSpawner.Mode characterControllerMode, Func<float> getSensitivity, Func<float> getAimingSensitivity, string prefix, bool isThirdPerson, bool useSimplifiedSkeleton = false) where TPlayer : Player
	{
		GameObject gameObject = Singleton<PoolManagerClass>.Instance.CreatePlayerObject(assetName);
		gameObject.name = prefix + gameObject.name;
		gameObject.transform.parent = null;
		Animator componentInChildren = gameObject.GetComponentInChildren<Animator>(includeInactive: true);
		gameObject.SetActive(value: true);
		return smethod_0<TPlayer>(gameWorld, gameObject, componentInChildren, playerId, position, updateQueue, armsUpdateMode, bodyUpdateMode, characterControllerMode, getSensitivity, getAimingSensitivity, isThirdPerson, useSimplifiedSkeleton);
	}

	public static T smethod_0<T>(GameWorld gameWorld, GameObject poolObject, Animator animator, int playerId, Vector3 position, EUpdateQueue updateQueue, EUpdateMode armsUpdateMode, EUpdateMode bodyUpdateMode, CharacterControllerSpawner.Mode characterControllerMode, Func<float> getSensitivity, Func<float> getAimingSensitivity, bool isThirdPerson, bool useSimplifiedSkeleton = false) where T : Player
	{
		PlayerPoolObject component = poolObject.GetComponent<PlayerPoolObject>();
		T val = poolObject.AddComponent<T>();
		val.PlayerId = playerId;
		component.RegisteredComponentsToClean.Add(val);
		val.GameWorld = gameWorld;
		val._updateQueue = updateQueue;
		val.GetSensitivity = getSensitivity;
		val.GetAimingSensitivity = getAimingSensitivity;
		val.MalfRandoms = new MalfunctionRandom(UnityEngine.Random.Range(int.MinValue, int.MaxValue));
		val._heavyVestsDeflectRandoms = new GClass3727(512, 0);
		val._armsUpdateMode = armsUpdateMode;
		val.TrunkRotationLimit = EFTHardSettings.Instance.HANDS_TO_BODY_MAX_ANGLE;
		val._bodyUpdateMode = bodyUpdateMode;
		val.PlayerBones = component.PlayerBones;
		val.PlayerBones.Player = val;
		val._animators = new IAnimator[2];
		val.CreateBodyAnimator(animator, updateQueue, useSimplifiedSkeleton);
		val.UsedSimplifiedSkeleton = useSimplifiedSkeleton;
		foreach (Collider collider in component.Colliders)
		{
			collider.enabled = true;
		}
		val.Transform.Original.position = position;
		val._characterController = component.CharacterControllerSpawner.Spawn(characterControllerMode, val, val.gameObject, isSpirit: false, isThirdPerson);
		val._triggerColliderSearcher = component.CharacterControllerSpawner.TriggerColliderSearcher;
		val._customHandRotator = new GClass2093();
		val.POM = component.PlayerOverlapManager;
		if (val.POM != null)
		{
			val.POM.Init(val._characterController.GetCollider());
		}
		IKAuthority[] behaviours = val.BodyAnimatorCommon.GetBehaviours<IKAuthority>();
		for (int i = 0; i < behaviours.Length; i++)
		{
			behaviours[i].PlayerBones = val.PlayerBones;
		}
		val.Grounder = component.GrounderFbbik;
		val.Grounder.enabled = true;
		val._fbbik = component.FullBodyBipedIk;
		val.HitReaction = component.HitReaction;
		val.HitReaction.enabled = false;
		val._limbs = component.LimbIks;
		LimbIK[] limbs = val._limbs;
		for (int i = 0; i < limbs.Length; i++)
		{
			limbs[i].enabled = true;
		}
		if (BackendConfigAbstractClass.Config.UseSpiritPlayer)
		{
			PlayerSpirit playerSpirit = Singleton<PoolManagerClass>.Instance.CreateFromPool<PlayerSpirit>(useSimplifiedSkeleton ? ResourceKeyManagerAbstractClass.ZOMBIE_SPIRIT_RESOURCE_KEY : ResourceKeyManagerAbstractClass.PLAYER_SPIRIT_RESOURCE_KEY);
			playerSpirit.transform.position = Vector3.zero;
			playerSpirit.gameObject.SetActive(value: true);
			val.Spirit = playerSpirit;
			val.Transform.Original.SetParent(playerSpirit.transform, worldPositionStays: false);
			playerSpirit.Init(val, position, val._bodyUpdateMode != EUpdateMode.None, characterControllerMode, null);
		}
		val.Logger = new GClass724(LoggerMode.Add);
		return val;
	}

	public virtual BasePhysicalClass CreatePhysical()
	{
		return new PlayerPhysicalClass();
	}

	public virtual void CreateBodyAnimator(Animator animator, EUpdateQueue updateQueue, bool useSimplifiedSkeleton)
	{
		if (!BackendConfigAbstractClass.Config.UseBodyFastAnimator)
		{
			_animators[0] = GClass1445.CreateAnimator(animator);
			_animators[0].cullingMode = AnimatorCullingMode.AlwaysAnimate;
			_animators[0].updateMode = ((updateQueue == EUpdateQueue.FixedUpdate) ? AnimatorUpdateMode.AnimatePhysics : AnimatorUpdateMode.Normal);
			if (!useSimplifiedSkeleton)
			{
				_animators[0].SetLayerWeight(17, 0f);
				_animators[0].SetLayerWeight(19, 0f);
				if (IsAI)
				{
					_animators[0].SetLayerWeight(15, 0f);
				}
			}
			return;
		}
		FastAnimatorControllerClass fastAnimatorController = GClass1346.Deserialize(GClass1857.GetAsset<TextAsset>(Singleton<IEasyAssets>.Instance, UsedSimplifiedSkeleton ? ResourceKeyManagerAbstractClass.ZOMBIE_FAST_ANIMATOR_CONTROLLER : ResourceKeyManagerAbstractClass.PLAYER_FAST_ANIMATOR_CONTROLLER).bytes);
		RootMotionBlendTable asset = GClass1857.GetAsset<RootMotionBlendTable>(Singleton<IEasyAssets>.Instance, UsedSimplifiedSkeleton ? ResourceKeyManagerAbstractClass.ZOMBIE_ROOTMOTION_TABLE : ResourceKeyManagerAbstractClass.PLAYER_ROOTMOTION_TABLE);
		asset.LoadNodes();
		_bodyUpdateMode = EUpdateMode.Manual;
		_animators[0] = GClass1445.CreateAnimator(fastAnimatorController, asset._loadedNodes, PlayerBones.BodyTransform.Original, PlayerBones.PlayableAnimator);
		_animators[0].cullingMode = AnimatorCullingMode.AlwaysAnimate;
		_animators[0].updateMode = ((updateQueue == EUpdateQueue.FixedUpdate) ? AnimatorUpdateMode.AnimatePhysics : AnimatorUpdateMode.Normal);
		CharacterClipsKeeper asset2 = GClass1857.GetAsset<CharacterClipsKeeper>(Singleton<IEasyAssets>.Instance, UsedSimplifiedSkeleton ? ResourceKeyManagerAbstractClass.ZOMBIE_ANIMATION_CLIPS_KEEPER : ResourceKeyManagerAbstractClass.PLAYER_ANIMATION_CLIPS_KEEPER);
		FastAnimatorProcessorClass fastAnimatorProcessorClass = _animators[0] as FastAnimatorProcessorClass;
		PlayerBones.PlayableAnimator.Init(_animators[0], fastAnimatorProcessorClass.GetParametersCache(), asset, asset2, manualUpdate: false);
		PlayerBones.PlayableAnimator.SetCuller(new GClass1340(PlayerBones.PlayableAnimator));
		PlayerBones.PlayableAnimator.Play();
		for (int i = 0; i < PlayerBones.PlayableAnimator.initialLayerInfo.Length; i++)
		{
			_animators[0].SetLayerWeight(i, PlayerBones.PlayableAnimator.initialLayerInfo[i].weight);
		}
	}

	public void method_82()
	{
		if (!(_createdAnimator == null) && !(_createdRuntimeAnimatorController == null) && !BackendConfigAbstractClass.Config.UseBodyFastAnimator)
		{
			_animators[0].runtimeAnimatorController = _createdRuntimeAnimatorController;
			if (_createdAnimator.runtimeAnimatorController == null)
			{
				_createdAnimator.runtimeAnimatorController = _createdRuntimeAnimatorController;
			}
			_createdAnimator = null;
			_createdRuntimeAnimatorController = null;
		}
	}

	public virtual async Task Init(Quaternion rotation, string layerName, EPointOfView pointOfView, Profile profile, PlayerInventoryController inventoryController, IHealthController healthController, IStatisticsManager statisticsManager, AbstractQuestControllerClass questController, AbstractAchievementControllerClass achievementsController, AbstractPrestigeControllerClass prestigeController, GClass3617 dialogController, IViewFilter filter, EVoipState voipState, bool aiControlled = false, bool async = true)
	{
		method_82();
		if (async)
		{
			await JobScheduler.Yield();
		}
		Profile = profile;
		method_22();
		if (async)
		{
			await JobScheduler.Yield();
		}
		StatisticsManager = statisticsManager;
		_inventoryController = inventoryController;
		MainParts = EnemyPart.Create(GetPlayer, PlayerBones);
		InventoryController.RegisterView(this);
		_itemInHands.Value = InventoryController.ItemInHands;
		ExfilUnsubscribe = InteractingWithExfiltrationPoint.Bind(delegate
		{
			this.PossibleInteractionsChanged?.Invoke();
		});
		CreateSlotObservers();
		DogtagComponent dogtagComponent = Equipment.GetSlot(EquipmentSlot.Dogtag).ContainedItem?.GetItemComponent<DogtagComponent>();
		if (dogtagComponent != null)
		{
			dogtagComponent.ProfileId = profile.Id;
			dogtagComponent.GroupId = Profile.Info.GroupId;
		}
		FaceShieldObserver.Changed.Subscribe(method_38);
		FaceCoverObserver.Changed.Subscribe(method_39);
		_questController = questController;
		_achievementsController = achievementsController;
		_prestigeController = prestigeController;
		_dialogController = dialogController;
		_playerBody = PlayerBones.AnimatedTransform.Original.gameObject.GetComponent<PlayerBody>();
		UpdatePhones();
		Task task = _playerBody.Init(filter.FilterCustomization(profile.Customization), Equipment, _itemInHands, LayerMask.NameToLayer("Player"), Side, "", null, IsYourPlayer);
		if (async)
		{
			await task;
		}
		_healthController = healthController;
		if (async)
		{
			await JobScheduler.Yield();
		}
		Physical = CreatePhysical();
		method_1();
		method_16(pointOfView);
		Physical.Init(this);
		Physical.EncumberedChanged += HealthController.SetEncumbered;
		Physical.OverEncumberedChanged += HealthController.SetOverEncumbered;
		Physical.OnWeightUpdated();
		TransformHelperClass.SetLayersRecursively(base.gameObject, LayerMask.NameToLayer(layerName));
		SetupHitColliders();
		EventTranslator = Transform.Original.GetChild(0).gameObject.AddComponent<GenericEventTranslator>();
		MovementContext.OnStateChanged += delegate(EPlayerState prevState, EPlayerState nextState)
		{
			ProceduralWeaponAnimation.WalkEffectorEnabled = nextState == EPlayerState.Run;
			ProceduralWeaponAnimation.DrawEffectorEnabled = nextState != EPlayerState.ProneMove;
			ProceduralWeaponAnimation.TiltBlender.Target = ((nextState == EPlayerState.Idle || nextState == EPlayerState.ProneIdle) ? 1 : 0);
			if (prevState == EPlayerState.Stationary)
			{
				ProceduralWeaponAnimation.SetStrategy(pointOfView);
			}
		};
		MovementContext.PhysicalConditionChanged += ProceduralWeaponAnimation.PhysicalConditionUpdated;
		HealthController.EffectStartedEvent += OnHealthEffectAdded;
		HealthController.EffectResidualEvent += OnHealthEffectRemoved;
		HealthController.HealthChangedEvent += method_87;
		HealthController.BodyPartDestroyedEvent += method_85;
		HealthController.BodyPartRestoredEvent += method_84;
		HealthController.PropagateAllEffects();
		profile.OnTraderStandingChanged += TraderStandingHandler;
		GClass3681 voice = Singleton<CustomizationSolverClass>.Instance.GetVoice(profile.Customization[EBodyModelPart.Voice]);
		Speaker = new PhraseSpeakerClass
		{
			OnDemandOnly = !aiControlled
		};
		Speaker.Init(profile.Info.Side, PlayerId, voice.Name);
		Speaker.TrackTransform = Transform;
		Speaker.OnPhraseTold += OnPhraseTold;
		GameWorld.SpeakerManager.AssignToGroup(profile.Info.Side, this);
		Environment = EnvironmentManager.Instance.GetPlayerCurrentEnvironmentType(ProfileId);
		EnvironmentManager.Instance.OnPlayerEnvironmentChanged += SetEnvironment;
		InitAudioController();
		MovementContext.Rotation = new Vector2(rotation.eulerAngles.y, Mathf.DeltaAngle(0f, rotation.eulerAngles.x));
		MovementContext.CachedRotation = new Vector2(rotation.eulerAngles.y, Mathf.DeltaAngle(0f, rotation.eulerAngles.x));
		_playerLookRaycastTransform = PlayerBones.LootRaycastOrigin;
		StartCoroutine(FakeCallbackCoroutine());
		Pedometer = new PedometerClass(this);
		ConnectSkillManager();
		float current = HealthController.Temperature.Current;
		PlayerBody.SetTemperatureForBody(current);
		RecalculateEquipmentParams();
		statisticsManager.Init(this);
		StartCoroutine(method_117());
		if (BackendConfigAbstractClass.Config.UseSpiritPlayer)
		{
			Spirit.InitAfterPlayerInit();
		}
		Loyalty = new PlayerLoyaltyData(this);
		method_99();
		_healthController.DiedEvent += OnDead;
		GameWorld.RegisterPlayer(this);
		if (_triggerColliderSearcher != null)
		{
			_triggerColliderSearcher.ConnectToCharacterController(_characterController);
			_triggerColliderSearcher.IsEnabled = true;
		}
		InitVoip(voipState);
		InitializeRecodableItemHandlers();
		StartCoroutine(method_90());
		BindSlotViewChangedAction(EquipmentSlot.Headwear, method_86);
		InitVaultingComponent(aiControlled);
		method_83(aiControlled);
		InitializeLeftHandController();
	}

	public virtual void InitializeLeftHandController()
	{
		_leftHandController = new GClass2725(MovementContext.PlayerAnimator.Animator, MovementContext.PlayerAnimator.EventsDispatcher.PlayerAnimatorEvents.LeftHandInteractionEvents, PlayerBones.LeftPalm);
	}

	public void method_83(bool aiControlled)
	{
		if (!aiControlled)
		{
			BackendConfigSettingsClass.GClass1753 mountingSettings = Singleton<BackendConfigSettingsClass>.Instance.MountingSettings;
			_weaponMountingComponent = new GClass2667(_playerBody.PlayerBones.weaponMountingView, MovementContext, ProceduralWeaponAnimation, mountingSettings.PointDetectionSettings, mountingSettings.MovementSettings, ProceduralWeaponAnimation.HandsContainer);
			HandsChangingEvent += _weaponMountingComponent.CancelFindingPoint;
		}
	}

	public virtual void InitVaultingComponent(bool aiControlled)
	{
		if (aiControlled)
		{
			return;
		}
		BackendConfigSettingsClass.VaultingGlobalSettings vaultingSettings = Singleton<BackendConfigSettingsClass>.Instance.VaultingSettings;
		if (vaultingSettings.IsActive)
		{
			GClass2679 parameters = (GClass2679)(_vaultingParameters = (IVaultingParameters)(_vaultingComponentDebug = (IVaultingComponentDebug)(_vaultingComponent = new GClass2679(MovementContext, vaultingSettings, () => MovementContext.AutoVaultingSettingEnabled, () => Physical.CanVault, () => Physical.CanClimb))));
			UpdateEvent += _vaultingComponent.DoVaultingTick;
			_vaultingGameplayRestrictions = new GClass2680(this, parameters);
			InitVaultingAudioControllers(_vaultingParameters);
		}
	}

	public virtual void CreateSlotObservers()
	{
		NightVisionObserver = new GClass2059<NightVisionComponent>(Equipment.GetSlot(EquipmentSlot.Headwear), (NightVisionComponent nv, Action handler) => nv.Togglable.OnChanged.Subscribe(handler));
		ThermalVisionObserver = new GClass2059<ThermalVisionComponent>(Equipment.GetSlot(EquipmentSlot.Headwear), (ThermalVisionComponent tv, Action handler) => tv.Togglable.OnChanged.Subscribe(handler));
		FaceShieldObserver = new GClass2059<FaceShieldComponent>(Equipment.GetSlot(EquipmentSlot.Headwear), delegate(FaceShieldComponent fs, Action handler)
		{
			Action togglableSub = fs.Togglable?.OnChanged.Subscribe(handler);
			Action hitSub = fs.HitsChanged.Subscribe(handler);
			return delegate
			{
				togglableSub?.Invoke();
				hitSub();
			};
		});
		FaceCoverObserver = new GClass2059<FaceShieldComponent>(Equipment.GetSlot(EquipmentSlot.FaceCover), delegate(FaceShieldComponent fs, Action handler)
		{
			Action togglableSub = fs.Togglable?.OnChanged.Subscribe(handler);
			Action hitSub = fs.HitsChanged.Subscribe(handler);
			return delegate
			{
				togglableSub?.Invoke();
				hitSub();
			};
		});
	}

	public void method_84(EBodyPart arg1, ValueStruct arg2)
	{
		ExecuteSkill(Skills.SurgeryAction.Complete);
		UpdateSpeedLimitByHealth();
	}

	public void method_85(EBodyPart arg1, EDamageType arg2)
	{
		if ((arg1 == EBodyPart.LeftLeg || arg1 == EBodyPart.RightLeg) && CurrentState.Name == EPlayerState.Sprint)
		{
			StartInflictSelfDamageCoroutine();
		}
		UpdateConditionsAfterBodyPartStateChanged(arg1);
	}

	public void BindSlotViewChangedAction(EquipmentSlot slot, Action<GameObject> action)
	{
		_playerBody.SlotViews.GetByKey(slot).ParentedModel.Bind(action);
	}

	public void method_86(GameObject _)
	{
		_helmetLightControllers = GClass6.GetComponentsInChildrenActiveIgnoreFirstLevel<TacticalComboVisualController>(PlayerBones.Head.Original);
		foreach (TacticalComboVisualController helmetLightController in _helmetLightControllers)
		{
			helmetLightController.UpdateBeams();
		}
		SendHeadlightsPacket(isSilent: true);
	}

	public virtual void Say(EPhraseTrigger phrase, bool demand = false, float delay = 0f, ETagStatus mask = (ETagStatus)0, int probability = 100, bool aggressive = false)
	{
		if (phrase == EPhraseTrigger.Cooperation)
		{
			vmethod_7(EInteraction.FriendlyGesture);
		}
		if (phrase == EPhraseTrigger.MumblePhrase)
		{
			phrase = ((aggressive || Time.time < Awareness) ? EPhraseTrigger.OnFight : EPhraseTrigger.OnMutter);
		}
		if (Speaker.OnDemandOnly && !demand)
		{
			this.PhraseSituation?.Invoke(phrase, 5);
			return;
		}
		if (Singleton<BotEventHandler>.Instantiated)
		{
			Singleton<BotEventHandler>.Instance.SayPhrase(this, phrase);
		}
		if (demand || probability > 99 || probability > UnityEngine.Random.Range(0, 100))
		{
			ETagStatus eTagStatus = ((!aggressive && !(Awareness > Time.time)) ? ETagStatus.Unaware : ETagStatus.Combat);
			if (delay > 0f)
			{
				Speaker.Queue(phrase, HealthStatus | mask | eTagStatus, delay, demand);
			}
			else
			{
				Speaker.Play(phrase, HealthStatus | mask | eTagStatus, demand);
			}
		}
	}

	public void NeedRepairMalfPhraseSituation(Weapon.EMalfunctionState malfState, bool isKnown)
	{
		if (isKnown && (malfState == Weapon.EMalfunctionState.SoftSlide || malfState == Weapon.EMalfunctionState.HardSlide))
		{
			this.PhraseSituation?.Invoke(EPhraseTrigger.OnWeaponJammed, 5);
		}
		else
		{
			this.PhraseSituation?.Invoke(EPhraseTrigger.WeaponBroken, 5);
		}
	}

	public virtual void OnPhraseTold(EPhraseTrigger @event, TaggedClip clip, TagBank bank, PhraseSpeakerClass speaker)
	{
		method_33(clip);
	}

	public void OnControllerColliderHit(ControllerColliderHit hit)
	{
		MovementContext.OnControllerColliderHit(hit);
	}

	public virtual void OnDestroy()
	{
		Destroyed = true;
		Class443.OnInitialized -= method_30;
		CameraClass.Instance.FoVUpdateAction -= OnFovUpdatedEvent;
		if (Speaker != null)
		{
			Speaker.OnPhraseTold -= OnPhraseTold;
			Speaker.OnDestroy();
		}
		method_50();
		method_32();
		method_51();
		StopInteractionSound();
		Dispose();
		if (BackendConfigAbstractClass.Config.UseSpiritPlayer && Spirit != null)
		{
			Spirit.IsStub = true;
			UnityEngine.Object.Destroy(Spirit.gameObject);
		}
	}

	public virtual void TraderStandingHandler(Profile.TraderInfo traderInfo)
	{
	}

	public virtual void OnInteractWithLightHouseTraderZone(GStruct434[] AllowedPlayers, GStruct434[] UnallowedPlayers)
	{
	}

	public virtual void OnLighthouseTraderZoneDebugToolSwitch(bool active)
	{
	}

	public virtual void ShowStringNotification(string message)
	{
	}

	public void method_87(EBodyPart bodyPart, float diff, DamageInfoStruct damageInfo)
	{
		if (!(Mathf.Abs(diff) < 0.01f))
		{
			UpdateConditionsAfterBodyPartStateChanged(bodyPart);
		}
	}

	public void UpdateConditionsAfterBodyPartStateChanged(EBodyPart bodyPart)
	{
		switch (bodyPart)
		{
		case EBodyPart.LeftArm:
		case EBodyPart.RightArm:
			UpdateArmsCondition();
			break;
		case EBodyPart.LeftLeg:
		case EBodyPart.RightLeg:
			UpdateSpeedLimitByHealth();
			break;
		}
		UpdateBreathStatus();
	}

	public virtual void UpdateArmsCondition()
	{
		if (HealthController.FindActiveEffect<GInterface358>() != null)
		{
			MovementContext.SetPhysicalCondition(EPhysicalCondition.LeftArmDamaged, val: false);
			MovementContext.SetPhysicalCondition(EPhysicalCondition.RightArmDamaged, val: false);
			return;
		}
		bool val = HealthController.IsBodyPartBroken(EBodyPart.LeftArm) || HealthController.IsBodyPartDestroyed(EBodyPart.LeftArm);
		bool val2 = HealthController.IsBodyPartBroken(EBodyPart.RightArm) || HealthController.IsBodyPartDestroyed(EBodyPart.RightArm);
		MovementContext.SetPhysicalCondition(EPhysicalCondition.LeftArmDamaged, val);
		MovementContext.SetPhysicalCondition(EPhysicalCondition.RightArmDamaged, val2);
	}

	public virtual void OnChangeRadioTransmitterState(bool isEncoded, RadioTransmitterStatus status, bool isAgressor)
	{
		if (RecodableItemsHandler.TryToGetRecodableComponent<RadioTransmitterRecodableComponent>(out var component))
		{
			component.SetStatus(status);
		}
		IsAgressorInLighthouseTraderZone = isAgressor;
	}

	public virtual void OnHealthEffectAdded(IEffect effect)
	{
		bool flag = true;
		if (effect is GInterface355)
		{
			MovementContext.SetPhysicalCondition(EPhysicalCondition.Tremor, val: true);
		}
		else if (effect is GInterface357)
		{
			if (MovementContext.PhysicalConditionIs(EPhysicalCondition.OnPainkillers))
			{
				flag = false;
			}
		}
		else if (!(effect is GInterface358) && !(effect is GInterface350) && !(effect is GInterface342))
		{
			if (effect is GInterface371)
			{
				MovementContext.SetPhysicalCondition(EPhysicalCondition.Panic, val: true);
			}
			else if (effect is GInterface361)
			{
				MovementContext.SetPhysicalCondition(EPhysicalCondition.Tremor, val: true);
			}
			else if (effect is GInterface376 gInterface)
			{
				MovementContext.SetPhysicalCondition(EPhysicalCondition.UsingMeds, val: true);
				if (gInterface.NoMove)
				{
					MovementContext.SetPhysicalCondition(EPhysicalCondition.HealingLegs, val: true);
				}
			}
			else if (effect is GInterface356)
			{
				ExecuteSkill((Action)delegate
				{
					Skills.LowHPDuration.Begin();
				});
			}
			else if (effect is GInterface366)
			{
				Fatigue = effect;
			}
			else if (effect is GInterface352)
			{
				method_88(effect.WorkStateTime);
			}
		}
		else
		{
			UpdateSpeedLimitByHealth();
			UpdateArmsCondition();
			if (effect is GInterface350)
			{
				Physical.BerserkRestorationFactor = true;
				Say(EPhraseTrigger.OnFight, demand: true);
			}
			if (effect is GInterface342 { WasPaused: false } && FractureSound != null && Singleton<BetterAudio>.Instantiated)
			{
				BetterSource betterSource = Singleton<BetterAudio>.Instance.PlayAtPoint(Position, FractureSound, CameraClass.Instance.Distance(Position), BetterAudio.AudioSourceGroupType.Character, 15);
				if (!GClass2078.IsFirstPerson(PointOfView) && betterSource != null && MonoBehaviourSingleton<SpatialAudioSystem>.Exist(out var component))
				{
					component.ProcessSourceOcclusion(this, betterSource);
				}
			}
		}
		if (flag)
		{
			ExecuteSkill((Action)delegate
			{
				Skills.HealthNegativeEffect.Complete(effect);
			});
		}
	}

	public void method_88(float time)
	{
		if (!IsAI && !(Equipment.GetSlot(EquipmentSlot.Earpiece).ContainedItem is HeadphonesItemClass))
		{
			Singleton<BetterAudio>.Instance.StartTinnitusEffect(time, _tinnitus);
		}
	}

	public virtual void OnHealthEffectRemoved(IEffect effect)
	{
		if (effect is GInterface355)
		{
			MovementContext.SetPhysicalCondition(EPhysicalCondition.Tremor, val: false);
		}
		else if (!(effect is GInterface358) && !(effect is GInterface350) && !(effect is GInterface342))
		{
			if (effect is GInterface361 && _healthController.FindActiveEffect<GInterface361>() == null)
			{
				MovementContext.SetPhysicalCondition(EPhysicalCondition.Tremor, val: false);
			}
			else if (effect is GInterface371 && _healthController.FindActiveEffect<GInterface371>() == null)
			{
				MovementContext.SetPhysicalCondition(EPhysicalCondition.Panic, val: false);
			}
			else if (effect is GInterface376 && _healthController.FindActiveEffect<GInterface376>() == null)
			{
				MovementContext.SetPhysicalCondition(EPhysicalCondition.UsingMeds, val: false);
				MovementContext.SetPhysicalCondition(EPhysicalCondition.HealingLegs, val: false);
				MovementContext.SetPhysicalCondition(EPhysicalCondition.RightLegDamaged, HealthController.IsBodyPartBroken(EBodyPart.RightLeg) || HealthController.IsBodyPartDestroyed(EBodyPart.RightLeg));
				MovementContext.SetPhysicalCondition(EPhysicalCondition.LeftLegDamaged, HealthController.IsBodyPartBroken(EBodyPart.LeftLeg) || HealthController.IsBodyPartDestroyed(EBodyPart.LeftLeg));
			}
			else if (effect is GInterface356)
			{
				ExecuteSkill((Action)delegate
				{
					Skills.LowHPDuration.Complete();
				});
			}
			else if (effect is GInterface366)
			{
				Fatigue = null;
			}
		}
		else
		{
			UpdateSpeedLimitByHealth();
			UpdateArmsCondition();
			if (effect is GInterface350)
			{
				Physical.BerserkRestorationFactor = false;
			}
		}
	}

	public virtual void UpdateSpeedLimitByHealth()
	{
		MovementContext.SetPhysicalCondition(EPhysicalCondition.OnPainkillers, HealthController.FindActiveEffect<GInterface358>() != null || HealthController.FindActiveEffect<GInterface350>() != null);
		MovementContext.SetPhysicalCondition(EPhysicalCondition.RightLegDamaged, HealthController.IsBodyPartBroken(EBodyPart.RightLeg) || HealthController.IsBodyPartDestroyed(EBodyPart.RightLeg));
		MovementContext.SetPhysicalCondition(EPhysicalCondition.LeftLegDamaged, HealthController.IsBodyPartBroken(EBodyPart.LeftLeg) || HealthController.IsBodyPartDestroyed(EBodyPart.LeftLeg));
		RemoveStateSpeedLimit(ESpeedLimit.HealthCondition);
		if (!MovementContext.PhysicalConditionIs(EPhysicalCondition.RightLegDamaged) && !MovementContext.PhysicalConditionIs(EPhysicalCondition.LeftLegDamaged))
		{
			return;
		}
		if (!MovementContext.PhysicalConditionIs(EPhysicalCondition.OnPainkillers))
		{
			MovementContext.EnableSprint(enable: false);
			if (MovementContext.PhysicalConditionIs(EPhysicalCondition.RightLegDamaged) && MovementContext.PhysicalConditionIs(EPhysicalCondition.LeftLegDamaged))
			{
				AddStateSpeedLimit(0.2f, ESpeedLimit.HealthCondition);
			}
			else
			{
				AddStateSpeedLimit(0.3f, ESpeedLimit.HealthCondition);
			}
		}
		if (CurrentState.Name == EPlayerState.Sprint)
		{
			StartInflictSelfDamageCoroutine();
		}
	}

	public void OnItemRemoved(GEventArgs3 eventArgs)
	{
		if (eventArgs.Status == CommandStatus.Succeed)
		{
			OnItemAddedOrRemoved(eventArgs.Item, eventArgs.From, added: false);
		}
	}

	void IOnItemRemoved.OnItemRemoved(GEventArgs3 eventArgs)
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnItemRemoved
		this.OnItemRemoved(eventArgs);
	}

	public void OnSetInHands(GEventArgs9 eventArgs)
	{
		if (eventArgs.Status == CommandStatus.Succeed)
		{
			_itemInHands.Value = eventArgs.Item;
		}
	}

	void IOnSetInHands.OnSetInHands(GEventArgs9 eventArgs)
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnSetInHands
		this.OnSetInHands(eventArgs);
	}

	public void OnItemAdded(GEventArgs2 eventArgs)
	{
		if (eventArgs.Status == CommandStatus.Succeed)
		{
			OnItemAddedOrRemoved(eventArgs.Item, eventArgs.To, added: true);
			ArmorComponent itemComponent = eventArgs.Item.GetItemComponent<ArmorComponent>();
			if (itemComponent != null)
			{
				OnArmorPointsChanged(itemComponent, children: true);
			}
			SideEffectComponent itemComponent2 = eventArgs.Item.GetItemComponent<SideEffectComponent>();
			if (itemComponent2 != null)
			{
				OnSideEffectApplied(itemComponent2);
			}
		}
	}

	void IOnItemAdded.OnItemAdded(GEventArgs2 eventArgs)
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnItemAdded
		this.OnItemAdded(eventArgs);
	}

	public void SwitchHeadLights(bool togglesActive, bool changesState)
	{
		if (IsHeadLightsAnimationActive || !StateIsSuitableForHandInput || !_helmetLightControllers.Any())
		{
			return;
		}
		foreach (TacticalComboVisualController helmetLightController in _helmetLightControllers)
		{
			FirearmLightStateStruct lightState = helmetLightController.LightMod.GetLightState(togglesActive, changesState);
			helmetLightController.LightMod.SetLightState(lightState);
		}
		SendHeadlightsPacket(isSilent: false);
		SwitchHeadLightsAnimation();
	}

	public virtual void SendHeadlightsPacket(bool isSilent)
	{
	}

	public virtual void SendWeaponLightPacket()
	{
	}

	public void UpdatePhonesReally()
	{
		if (!GClass3670.TryGetData<GClass2596>(out var dataContainer))
		{
			return;
		}
		CompoundItem compoundItem = Equipment.GetSlot(EquipmentSlot.Headwear).ContainedItem as CompoundItem;
		CompoundItem compoundItem2 = Equipment.GetSlot(EquipmentSlot.FaceCover).ContainedItem as CompoundItem;
		HeadphonesItemClass headphonesItemClass = (Equipment.GetSlot(EquipmentSlot.Earpiece).ContainedItem as HeadphonesItemClass) ?? ((compoundItem != null) ? GClass3380.GetAllItemsFromCollection(compoundItem).OfType<HeadphonesItemClass>().FirstOrDefault() : null);
		HeadphonesTemplateClass template = dataContainer.Default;
		if (headphonesItemClass != null)
		{
			template = headphonesItemClass.Template;
		}
		if (GClass2595.IsDefault(template) && compoundItem?.Template is ArmoredEquipmentTemplateClass armoredEquipmentTemplateClass)
		{
			EDeafStrength deafStrength = (from d in (from x in GClass3380.GetItemComponentsInChildren<CompositeArmorComponent>(compoundItem)
					select x.Deaf).Append(armoredEquipmentTemplateClass.DeafStrength)
				orderby (int)d descending
				select d).FirstOrDefault();
			template = dataContainer.GetHeadphonesTemplateByDeaf(deafStrength);
		}
		if (GClass2595.IsDefault(template) && compoundItem2 != null)
		{
			EDeafStrength deafStrength2 = (from x in GClass3380.GetItemComponentsInChildren<CompositeArmorComponent>(compoundItem2)
				select x.Deaf into d
				orderby (int)d descending
				select d).FirstOrDefault();
			template = dataContainer.GetHeadphonesTemplateByDeaf(deafStrength2);
		}
		Singleton<BetterAudio>.Instance.ApplyHeadphonesTemplate(template);
	}

	public virtual void UpdatePhones()
	{
		if (IsYourPlayer)
		{
			UpdatePhonesReally();
		}
	}

	public virtual void OnItemAddedOrRemoved(Item item, ItemAddress location, bool added)
	{
		if (location is GClass3393)
		{
			return;
		}
		Slot[] headSlots = new Slot[3]
		{
			Equipment.GetSlot(EquipmentSlot.Eyewear),
			Equipment.GetSlot(EquipmentSlot.Headwear),
			Equipment.GetSlot(EquipmentSlot.FaceCover)
		};
		IEnumerable<ItemAddress> allParentLocations = GClass3380.GetAllParentLocations(location, onlyMerged: true);
		if (allParentLocations.Any((ItemAddress loc) => headSlots.Contains(loc.Container)))
		{
			VisorsItemClass glasses;
			bool arg = TryFindGlasses(out glasses);
			this.OnGlassesChanged?.Invoke(glasses, arg);
			method_89();
			if (GClass3380.GetItemComponentsInChildren<NightVisionComponent>(item).Any())
			{
				NightVisionObserver.Update();
			}
			if (GClass3380.GetItemComponentsInChildren<ThermalVisionComponent>(item).Any())
			{
				ThermalVisionObserver.Update();
			}
			if (GClass3380.GetItemComponentsInChildren<FaceShieldComponent>(item).Any())
			{
				FaceShieldObserver.Update();
				FaceCoverObserver.Update();
			}
		}
		Slot[] armorSlots = Inventory.ArmorSlots.Select(Equipment.GetSlot).ToArray();
		if ((GClass3380.GetItemComponentsInChildren<ArmorComponent>(item).Any() && allParentLocations.Any((ItemAddress loc) => armorSlots.Contains(loc.Container))) || item.GetItemComponent<EquipmentPenaltyComponent>() != null)
		{
			RecalculateEquipmentParams();
		}
		UpdatePhones();
	}

	public bool TryFindGlasses(out VisorsItemClass glasses)
	{
		if (Equipment.GetSlot(EquipmentSlot.Eyewear).ContainedItem is VisorsItemClass visorsItemClass)
		{
			glasses = visorsItemClass;
			return true;
		}
		if (Equipment.GetSlot(EquipmentSlot.Headwear).ContainedItem is VisorsItemClass visorsItemClass2)
		{
			glasses = visorsItemClass2;
			return true;
		}
		glasses = null;
		return false;
	}

	public void method_89()
	{
		float blindnessProtection = GetBlindnessProtection();
		if (!GClass855.ApproxEquals(_currentBlindnessProtection, blindnessProtection))
		{
			_currentBlindnessProtection = blindnessProtection;
			this.OnBlindnessProtectionChanged?.Invoke(blindnessProtection);
		}
	}

	public float GetBlindnessProtection()
	{
		float num = 0f;
		EquipmentSlot[] array = new EquipmentSlot[3]
		{
			EquipmentSlot.Eyewear,
			EquipmentSlot.Headwear,
			EquipmentSlot.FaceCover
		};
		foreach (EquipmentSlot slotName in array)
		{
			if (Equipment.GetSlot(slotName).ContainedItem is ArmoredEquipmentItemClass armoredEquipmentItemClass)
			{
				num = ((armoredEquipmentItemClass.BlindnessProtection > num) ? armoredEquipmentItemClass.BlindnessProtection : num);
			}
		}
		Item containedItem = Equipment.GetSlot(EquipmentSlot.Headwear).ContainedItem;
		if (containedItem != null)
		{
			foreach (Item allItem in GClass3380.GetAllItems(containedItem))
			{
				if (allItem is ArmoredEquipmentItemClass armoredEquipmentItemClass2 && !(armoredEquipmentItemClass2.BlindnessProtection < num) && (!armoredEquipmentItemClass2.TryGetItemComponent<FaceShieldComponent>(out var component) || component.Togglable == null || component.Togglable.On))
				{
					num = armoredEquipmentItemClass2.BlindnessProtection;
				}
			}
		}
		return num;
	}

	public static ECameraType GetVisibleToCamera(IPlayer player)
	{
		return player?.VisibleToCameraType ?? ECameraType.Default;
	}

	[CanBeNull]
	public TItem TryGetItemInHands<TItem>() where TItem : Item
	{
		if (_handsController == null)
		{
			return null;
		}
		return _handsController.Item as TItem;
	}

	public virtual void OnDeserializeFromServer(byte channelId, IDataReader reader)
	{
	}

	public RadioTransmitterRecodableComponent FindRadioTransmitter()
	{
		if (RecodableItemsHandler.TryToGetRecodableComponent<RadioTransmitterRecodableComponent>(out var component))
		{
			return component;
		}
		return null;
	}

	public CultistAmuletItemClass FindCultistAmulet()
	{
		Slot[] slots = (InventoryController.Inventory.Equipment.GetSlot(EquipmentSlot.Pockets).ContainedItem as PocketsItemClass).Slots;
		int num = 0;
		CultistAmuletItemClass cultistAmuletItemClass;
		while (true)
		{
			if (num < slots.Length)
			{
				cultistAmuletItemClass = slots[num].ContainedItem as CultistAmuletItemClass;
				if (cultistAmuletItemClass != null)
				{
					break;
				}
				num++;
				continue;
			}
			return null;
		}
		return cultistAmuletItemClass;
	}

	public bool HasMarkOfUnknown(out MarkOfUnknownItemClass markOfUnknown)
	{
		PocketsItemClass obj = InventoryController.Inventory.Equipment.GetSlot(EquipmentSlot.Pockets).ContainedItem as PocketsItemClass;
		markOfUnknown = null;
		Slot[] slots = obj.Slots;
		int num = 0;
		MarkOfUnknownItemClass markOfUnknownItemClass;
		while (true)
		{
			if (num < slots.Length)
			{
				markOfUnknownItemClass = slots[num].ContainedItem as MarkOfUnknownItemClass;
				if (markOfUnknownItemClass != null)
				{
					break;
				}
				num++;
				continue;
			}
			return false;
		}
		markOfUnknown = markOfUnknownItemClass;
		return true;
	}

	public bool HasFirearmInHands()
	{
		if (HandsController is FirearmController firearmController)
		{
			return !firearmController.Weapon.IsStationaryWeapon;
		}
		return false;
	}

	public PlayerAnimator.EWeaponAnimationType GetWeaponAnimationType(AbstractHandsController handsController)
	{
		if (!(handsController == null) && handsController.Item != null && !(handsController is EmptyHandsController))
		{
			Item item = handsController.Item;
			if (!(item is PistolItemClass) && !(item is PortableRangeFinderItemClass) && !(item is RadioTransmitterItemClass))
			{
				if (item is RevolverItemClass revolverItemClass)
				{
					if (!revolverItemClass.WeapClass.Equals("pistol"))
					{
						return PlayerAnimator.EWeaponAnimationType.Rifle;
					}
					return PlayerAnimator.EWeaponAnimationType.Pistol;
				}
				if (item is GrenadeLauncherItemClass { IsFlareGun: not false })
				{
					return PlayerAnimator.EWeaponAnimationType.Pistol;
				}
				if (item.GetItemComponent<KnifeComponent>() != null)
				{
					return PlayerAnimator.EWeaponAnimationType.Knife;
				}
				if (item is ThrowWeapItemClass)
				{
					return PlayerAnimator.EWeaponAnimationType.ThrowWeapon;
				}
				if (item is RocketLauncherItemClass)
				{
					return PlayerAnimator.EWeaponAnimationType.RocketLauncher;
				}
				return PlayerAnimator.EWeaponAnimationType.Rifle;
			}
			return PlayerAnimator.EWeaponAnimationType.Pistol;
		}
		return PlayerAnimator.EWeaponAnimationType.EmptyHands;
	}

	public void SetDeltaTimeDelegate(GDelegate66 deltaTimeDelegate)
	{
		_deltaTimeDelegate = deltaTimeDelegate ?? _defaultDeltaTimeDelegate;
	}

	public void FixedUpdate()
	{
		_nFixedFrames++;
		_fixedTime += Time.fixedDeltaTime;
	}

	public virtual void FixedUpdateTick()
	{
		ComplexUpdate(EUpdateQueue.FixedUpdate, Time.fixedUnscaledDeltaTime);
	}

	public virtual void AfterMainTick()
	{
	}

	public virtual void UpdateTick()
	{
		float deltaTime = DeltaTime;
		ComplexUpdate(EUpdateQueue.Update, deltaTime);
	}

	public IEnumerator method_90()
	{
		while (true)
		{
			yield return _waitForFixedUpdate;
			ComplexLateUpdate(EUpdateQueue.FixedUpdate, Time.fixedUnscaledDeltaTime);
		}
	}

	public void ComplexUpdate(EUpdateQueue queue, float deltaTime)
	{
		if (HealthController != null && HealthController.IsAlive)
		{
			if (UpdateQueue == queue)
			{
				ManualUpdate(deltaTime);
				_bodyupdated = true;
				_bodyTime = deltaTime;
			}
			if (ArmsUpdateQueue == queue)
			{
				ArmsUpdate(deltaTime);
				_armsupdated = true;
				_armsTime = deltaTime;
			}
			if (PhysicalUpdateQueue == queue)
			{
				Physical.Update(deltaTime);
			}
			if (queue == EUpdateQueue.Update)
			{
				this.UpdateEvent?.Invoke();
			}
			if (queue == EUpdateQueue.FixedUpdate)
			{
				this.FixedUpdateEvent?.Invoke();
			}
		}
	}

	public virtual void ComplexLateUpdate(EUpdateQueue queue, float deltaTime)
	{
		if (ArmsUpdateQueue == queue && ArmsUpdateMode == EUpdateMode.Auto)
		{
			method_91(deltaTime);
		}
		MovementContext.LateFixedUpdate();
		AIData?.LateUpdate();
	}

	public virtual void ArmsUpdate(float deltaTime)
	{
		if (_handsController != null)
		{
			_handsController.ManualUpdate(deltaTime);
		}
		if (ArmsUpdateMode == EUpdateMode.Manual)
		{
			ArmsAnimatorCommon.Update(deltaTime);
			UnderbarrelWeaponArmsAnimator?.Update(deltaTime);
		}
		_armsupdated = true;
		_armsTime = deltaTime;
		if (ArmsUpdateMode == EUpdateMode.Manual)
		{
			method_91(deltaTime);
		}
	}

	public virtual void BodyUpdate(float deltaTime, int loop = 1)
	{
		if (BodyUpdateMode == EUpdateMode.Manual)
		{
			for (int i = 0; i < loop; i++)
			{
				float dt = deltaTime / (float)loop;
				BodyAnimatorCommon.Update(dt);
			}
		}
		if (BackendConfigAbstractClass.Config.UseBodyFastAnimator && HealthController != null && HealthController.IsAlive)
		{
			PlayerBones.PlayableAnimator.Process(IsVisible, deltaTime);
		}
	}

	public virtual void ManualUpdate(float deltaTime, float? platformDeltaTime = null, int loop = 1)
	{
		LastDeltaTime = deltaTime;
		if (Mathf.Approximately(deltaTime, 0f))
		{
			UnityEngine.Debug.LogErrorFormat("[ServerPlayer.ManualUpdate] deltaTime = {0}", deltaTime);
			return;
		}
		method_13(deltaTime);
		if (loop == 1)
		{
			HealthControllerUpdate(platformDeltaTime ?? deltaTime);
		}
		BodyUpdate(deltaTime);
		if (Vector2.Distance(_targetRotationPitch, _rotationPitchLimit) < 0.1f)
		{
			_rotationPitchLimit = _targetRotationPitch;
		}
		else
		{
			_rotationPitchLimit = Vector2.Lerp(_rotationPitchLimit, _targetRotationPitch, 0.1f);
		}
		UpdateTriggerColliderSearcher(deltaTime);
	}

	public virtual void HealthControllerUpdate(float deltaTime)
	{
		if (!(_healthController is HealthControllerClass))
		{
			_healthController.ManualUpdate(deltaTime);
		}
	}

	public virtual void UpdateTriggerColliderSearcher(float deltaTime, bool isCloseToCamera = true)
	{
		GetTriggerColliderSearcher().ManualUpdate(deltaTime, isCloseToCamera);
	}

	public void method_91(float deltaTime)
	{
		if (_handsController != null)
		{
			_handsController.EmitEvents();
			if (IsYourPlayer)
			{
				EFTPhysicsClass.SyncTransformsClass.SyncTransforms();
			}
			_handsController.BallisticUpdate(deltaTime);
		}
	}

	public void SetOwnerToAIData(BotOwner bot)
	{
		if (AIData == null)
		{
			AIData = new PlayerAIDataClass(bot, this);
		}
		else
		{
			AIData.BotOwner = bot;
		}
		Physical.EncumberDisabled = true;
	}

	public void SaveInteractionRayInfo()
	{
		InteractionRayOriginOnStartOperation = InteractionRay.origin;
		InteractionRayDirectionOnStartOperation = InteractionRay.direction;
	}

	public virtual void InteractionRaycast()
	{
		if (_playerLookRaycastTransform == null || !HealthController.IsAlive)
		{
			return;
		}
		InteractableObject interactableObject = null;
		InteractableObjectIsProxy = false;
		Player player = null;
		Ray interactionRay = InteractionRay;
		if (CurrentState.CanInteract && (bool)HandsController && HandsController.CanInteract())
		{
			RaycastHit hit;
			GameObject gameObject = GameWorld.FindInteractable(interactionRay, out hit);
			if (gameObject != null)
			{
				InteractiveProxy interactiveProxy = null;
				interactableObject = gameObject.GetComponentInParent<InteractableObject>();
				if (interactableObject == null)
				{
					interactiveProxy = gameObject.GetComponent<InteractiveProxy>();
					if (interactiveProxy != null && hit.distance < EFTHardSettings.Instance.DOOR_RAYCAST_DISTANCE + EFTHardSettings.Instance.BEHIND_CAST)
					{
						InteractableObjectIsProxy = true;
						interactableObject = interactiveProxy.Link;
					}
				}
				if (interactableObject != null && interactiveProxy == null)
				{
					if (interactableObject.InteractsFromAppropriateDirection(LookDirection))
					{
						if (!(hit.distance > EFTHardSettings.Instance.LOOT_RAYCAST_DISTANCE + EFTHardSettings.Instance.BEHIND_CAST) && interactableObject.isActiveAndEnabled)
						{
							if (hit.distance > EFTHardSettings.Instance.DOOR_RAYCAST_DISTANCE + EFTHardSettings.Instance.BEHIND_CAST && interactableObject is Door)
							{
								interactableObject = null;
							}
						}
						else
						{
							interactableObject = null;
						}
					}
					else
					{
						interactableObject = null;
					}
				}
				player = ((interactableObject == null) ? gameObject.GetComponent<Player>() : null);
			}
			RayLength = hit.distance;
		}
		if (interactableObject is WorldInteractiveObject worldInteractiveObject)
		{
			if (worldInteractiveObject is BufferGateSwitcher bufferGateSwitcher)
			{
				_ = bufferGateSwitcher.BufferGatesState;
				if (interactableObject == InteractableObject)
				{
					_nextCastHasForceEvent = true;
				}
			}
			else
			{
				EDoorState doorState = worldInteractiveObject.DoorState;
				if (doorState != EDoorState.Interacting && worldInteractiveObject.Operatable && (doorState != EDoorState.Locked || worldInteractiveObject.DoorKeyOpenInteraction != EInteraction.DoorCardOpen || InteractableObjectIsProxy) && (!InteractableObjectIsProxy || doorState == EDoorState.Locked))
				{
					if (interactableObject == InteractableObject && _lastInteractionState != doorState)
					{
						_nextCastHasForceEvent = true;
					}
				}
				else
				{
					interactableObject = null;
				}
			}
			if (worldInteractiveObject != null && worldInteractiveObject.NoInteractionsAllowed)
			{
				interactableObject = null;
			}
		}
		else if (interactableObject is LootItem lootItem)
		{
			if (lootItem.Item is Weapon { IsOneOff: not false } weapon && weapon.Repairable.Durability == 0f)
			{
				interactableObject = null;
			}
		}
		else if (interactableObject is StationaryWeapon stationaryWeapon)
		{
			if (stationaryWeapon.Locked)
			{
				interactableObject = null;
			}
			else if (interactableObject == InteractableObject && _lastInteractionState != stationaryWeapon.State)
			{
				_nextCastHasForceEvent = true;
			}
		}
		else if (interactableObject is LookAtProxy lookAtProxy)
		{
			lookAtProxy.Execute();
		}
		else if (interactableObject != null)
		{
			if (_lastStateUpdateTime != interactableObject.StateUpdateTime)
			{
				_nextCastHasForceEvent = true;
			}
			_lastStateUpdateTime = interactableObject.StateUpdateTime;
		}
		if (interactableObject != InteractableObject || _nextCastHasForceEvent)
		{
			_nextCastHasForceEvent = false;
			InteractableObject = interactableObject;
			if (InteractableObject is WorldInteractiveObject worldInteractiveObject2)
			{
				_lastInteractionState = worldInteractiveObject2.DoorState;
			}
			else if (InteractableObject is StationaryWeapon stationaryWeapon2)
			{
				_lastInteractionState = stationaryWeapon2.State;
			}
			this.PossibleInteractionsChanged?.Invoke();
		}
		if (player != InteractablePlayer || _nextCastHasForceEvent)
		{
			_nextCastHasForceEvent = false;
			InteractablePlayer = ((player != this) ? player : null);
			if (player == this)
			{
				UnityEngine.Debug.LogWarning(Profile.Nickname + " wants to interact to himself");
			}
			this.PossibleInteractionsChanged?.Invoke();
		}
		if (player == null && interactableObject == null)
		{
			float radius = 0.1f * (1f + (float)Skills.PerceptionLootDot);
			float distance = 1.5f;
			if ((bool)Skills.PerceptionEliteNoIdea)
			{
				distance = 2.35f;
				radius = 1.1f;
				interactionRay.origin = Transform.position + Vector3.up * 3f;
				interactionRay.direction = Vector3.down;
			}
			Boolean_0 = GameWorld.InteractionSense(interactionRay.origin, interactionRay.direction, radius, distance);
		}
		else
		{
			Boolean_0 = false;
		}
	}

	public virtual void PauseAllEffectsOnPlayer()
	{
	}

	public virtual void UnpauseAllEffectsOnPlayer()
	{
	}

	public virtual void ShowHelloNotification(string sender)
	{
		NotificationManagerClass.DisplayMessageNotification(string.Format(GClass2348.Localized("{0} ask to cooperate"), sender));
	}

	public void ResetInteractionRaycast(IKillableLootItem @object)
	{
		if (@object == InteractableObject)
		{
			InteractableObject = null;
			this.PossibleInteractionsChanged?.Invoke();
		}
	}

	public void OnPlaceItemTriggerChanged([CanBeNull] PlaceItemTrigger zone)
	{
		PlaceItemZone = zone;
		if (zone == null)
		{
			DestroyBeacon();
		}
		this.PossibleInteractionsChanged?.Invoke();
	}

	public void AddTriggerZone(TriggerWithId zone)
	{
		string id = zone.Id;
		if (!TriggerZones.Contains(id))
		{
			TriggerZones.Add(id);
		}
	}

	public void RemoveTriggerZone(TriggerWithId zone)
	{
		string id = zone.Id;
		if (TriggerZones.Contains(id))
		{
			TriggerZones.Remove(id);
		}
	}

	public void SetInteractInHands(EInteraction interaction)
	{
		MovementContext.SetInteractInHands(interaction);
	}

	public void UpdateInteractionCast()
	{
		_nextCastHasForceEvent = true;
	}

	public Vector3 PlayerColliderPointOnCenterAxis(float relativeHeight)
	{
		return MovementContext.PlayerColliderPointOnCenterAxis(relativeHeight);
	}

	public virtual void SetupHitColliders()
	{
		_hitColliders = GetComponentsInChildren<BodyPartCollider>();
		_armorPlateColliders = GetComponentsInChildren<ArmorPlateCollider>(includeInactive: true);
		BodyPartCollider[] hitColliders = _hitColliders;
		foreach (BodyPartCollider bodyPartCollider in hitColliders)
		{
			method_92(bodyPartCollider, includeChild: false);
		}
		ArmorPlateCollider[] armorPlateColliders = _armorPlateColliders;
		foreach (ArmorPlateCollider bodyPartCollider2 in armorPlateColliders)
		{
			method_92(bodyPartCollider2, includeChild: true);
		}
	}

	public void method_92(BodyPartCollider bodyPartCollider, bool includeChild)
	{
		int layer = LayerMask.NameToLayer("HitCollider");
		bodyPartCollider.SetUpPlayer(this);
		bodyPartCollider.PlayerProfileID = ProfileId;
		bodyPartCollider.gameObject.layer = layer;
		if (!includeChild)
		{
			return;
		}
		foreach (Transform item in bodyPartCollider.transform)
		{
			item.gameObject.layer = layer;
		}
	}

	public virtual void SetInventoryOpened(bool opened)
	{
		if (opened)
		{
			MovementContext.SetBlindFire(0);
			if (MovementContext.IsInMountedState)
			{
				MovementContext.ExitMountedState();
			}
		}
		else
		{
			if (_waitInventoryCoroutine != null)
			{
				StopCoroutine(_waitInventoryCoroutine);
			}
			MovementContext.PlayerAnimator.SetInventory(inventory: false);
			MovementContext.PlayerAnimator.SetInventoryOperation(isOperation: false);
			OnInventoryInteraction(inventory: false);
		}
		_isInventoryOpened = opened;
		InventoryOpenRaiseAction(opened);
		if (_handsController != null)
		{
			_handsController.SetInventoryOpened(opened);
			if (opened)
			{
				MovementContext.PlayerAnimator.AnimatedInteractions.ForceStopInteractions();
				_waitInventoryCoroutine = StartCoroutine(method_93());
			}
		}
	}

	public IEnumerator method_93()
	{
		if (_handsController.FirearmsAnimator.CurrentStateNameIs(1, "IN INVENTORY") || _handsController.FirearmsAnimator.CurrentStateNameIs(1, "OPEN INVENTORY"))
		{
			MovementContext.PlayerAnimator.SetInventory(inventory: true);
			OnInventoryInteraction(inventory: true);
			yield break;
		}
		while (!_handsController.FirearmsAnimator.CurrentStateNameIs(1, "IN INVENTORY") && !_handsController.FirearmsAnimator.CurrentStateNameIs(1, "OPEN INVENTORY") && IsInventoryOpened)
		{
			yield return null;
		}
		bool inventory = _handsController.FirearmsAnimator.CurrentStateNameIs(1, "IN INVENTORY") || _handsController.FirearmsAnimator.CurrentStateNameIs(1, "OPEN INVENTORY");
		MovementContext.PlayerAnimator.SetInventory(inventory);
		OnInventoryInteraction(inventory);
		while (_isInventoryOpened)
		{
			if (!_handsController.FirearmsAnimator.CurrentStateNameIs(1, "IN INVENTORY") && !_handsController.FirearmsAnimator.CurrentStateNameIs(1, "OPEN INVENTORY"))
			{
				OnInventoryInteraction(inventory, isOperation: true);
				MovementContext.PlayerAnimator.SetInventoryOperation(isOperation: true);
			}
			else
			{
				OnInventoryInteraction(inventory);
				MovementContext.PlayerAnimator.SetInventoryOperation(isOperation: false);
			}
			yield return null;
		}
	}

	public void InventoryOpenRaiseAction(bool opened)
	{
		this.OnStartInventoryOpen?.Invoke();
		this.OnInventoryOpened?.Invoke(this, opened);
	}

	public virtual void ExecuteShotSkill(Item weapon)
	{
		if (!(weapon is ThrowWeapItemClass) && !IsAI)
		{
			Type type = weapon.GetType();
			if (typeof(GClass3308).IsAssignableFrom(type))
			{
				type = typeof(GClass3308);
			}
			float val = (Skills.WeaponBuffs.ContainsKey(type) ? Skills.WeaponBuffs[type][EBuffId.WeaponDoubleMastering].Value : 1f);
			Skills.WeaponShotAction.Complete(weapon, val);
		}
	}

	public virtual void ManageAggressor(DamageInfoStruct damageInfo, EBodyPart bodyPart, EBodyPartColliderType colliderType)
	{
		if (_isDeadAlready)
		{
			return;
		}
		if (!HealthController.IsAlive)
		{
			_isDeadAlready = true;
		}
		Player player = ((damageInfo.Player == null) ? null : GameWorld.GetEverExistedPlayerByID(damageInfo.Player.iPlayer.ProfileId));
		if ((object)player == this)
		{
			return;
		}
		if (player == null)
		{
			if (damageInfo.Player != null && damageInfo.Player.iPlayer != null)
			{
				GClass788 aggressor = new GClass788(damageInfo.Player.iPlayer.AccountId, damageInfo.Player.iPlayer.ProfileId, damageInfo.Player.Nickname, damageInfo.Player.iPlayer.Profile.Info.MainProfileNickname, damageInfo.Player.iPlayer.Side, colliderType, (damageInfo.Weapon != null) ? damageInfo.Weapon.ShortName : string.Empty, damageInfo.Player.iPlayer.Profile.Info.SelectedMemberCategory, damageInfo.Player.iPlayer.Profile.Info.Settings.Role, damageInfo.Player.iPlayer.Profile.PrestigeLevel);
				Profile.EftStats.Aggressor = aggressor;
				LastAggressor = damageInfo.Player.iPlayer;
				LastDamageInfo = damageInfo;
				LastBodyPart = bodyPart;
			}
			else
			{
				Profile.EftStats.Aggressor = null;
				LastAggressor = null;
				LastDamageInfo = damageInfo;
				LastBodyPart = bodyPart;
			}
			if (damageInfo.DamageType == EDamageType.Artillery)
			{
				GClass788 aggressor2 = new GClass788("0", "66f3fad50ec64d74847d049d", GClass2348.Localized("UI/Artillery/ArtaManName"), string.Empty, EPlayerSide.Savage, colliderType, "UI/Artillery/ArtilleryWeaponName", EMemberCategory.Default, WildSpawnType.assault, 0);
				Profile.EftStats.Aggressor = aggressor2;
				LastAggressor = null;
				LastDamageInfo = damageInfo;
				LastBodyPart = bodyPart;
			}
			return;
		}
		if (damageInfo.Weapon != null && !GClass1673.EqualsAndNotNull(player.Profile.Info.GroupId, Profile.Info.GroupId))
		{
			player.ExecuteShotSkill(damageInfo.Weapon);
		}
		bool isHeavyDamage = damageInfo.DidBodyDamage / HealthController.GetBodyPartHealth(bodyPart).Maximum >= 0.6f && HealthController.FindExistingEffect<GInterface341>(bodyPart) != null;
		player.StatisticsManager.OnEnemyDamage(damageInfo, bodyPart, Profile.Id, Profile.Info.Side, Profile.Info.Settings.Role, Profile.Info.GroupId, HealthController.GetBodyPartHealth(EBodyPart.Common).Maximum, isHeavyDamage, Vector3.Distance(player.Transform.position, Transform.position), CurrentHour, Inventory.EquippedInSlotsTemplateIds, HealthController.BodyPartEffects, TriggerZones);
		if (string.IsNullOrEmpty(player.Profile.Info.Nickname) || player == this || player.ProfileId == ProfileId)
		{
			return;
		}
		method_94(damageInfo, bodyPart, player);
		LastAggressor = player;
		LastDamageInfo = damageInfo;
		LastBodyPart = bodyPart;
		IAIData aIData = player.AIData;
		Profile profile = ((aIData != null && aIData.IsAI) ? null : player.Profile);
		Profile.EftStats.Aggressor = new GClass788(profile?.AccountId, profile?.Id, player.Profile.Nickname, player.Profile.Info.MainProfileNickname, player.Profile.Info.Side, colliderType, (damageInfo.Weapon != null) ? damageInfo.Weapon.ShortName : string.Empty, player.Profile.Info.SelectedMemberCategory, player.Profile.Info.Settings.Role, player.Profile.Info.PrestigeLevel);
		if (!HealthController.IsAlive)
		{
			player.Say(EPhraseTrigger.OnEnemyDown, demand: false, UnityEngine.Random.Range(0f, 1f), Speaker.SideTag, 70);
			return;
		}
		bool flag = true;
		if (player.IsAI && damageInfo.Weapon != null && damageInfo.Weapon is KnifeItemClass)
		{
			flag = false;
		}
		if (flag)
		{
			player.Say(EPhraseTrigger.OnEnemyShot, demand: false, UnityEngine.Random.Range(0f, 1f), Speaker.SideTag, 30);
			GameWorld.SpeakerManager.GroupEvent(PlayerId, EPhraseTrigger.Hit, Transform.position, player.Speaker.SideTag, 30);
		}
	}

	public void method_94(DamageInfoStruct damageInfo, EBodyPart bodyPart, Player aggressor)
	{
		if (aggressor != null && aggressor.AIData.IsAI)
		{
			BotOwner botOwner = aggressor.AIData.BotOwner;
			botOwner.EnemiesController.HitTarget(this, damageInfo, bodyPart);
			botOwner.BotPersonalStats.HitTarget(this, damageInfo, bodyPart);
		}
	}

	public virtual void ApplyExplosionDamageToArmor(Dictionary<ExplosiveHitArmorColliderStruct, float> armorDamage, DamageInfoStruct damageInfo)
	{
		_preAllocatedArmorComponents.Clear();
		Inventory.GetPutOnArmorsNonAlloc(_preAllocatedArmorComponents);
		foreach (ArmorComponent preAllocatedArmorComponent in _preAllocatedArmorComponents)
		{
			float num = 0f;
			foreach (KeyValuePair<ExplosiveHitArmorColliderStruct, float> item in armorDamage)
			{
				if (preAllocatedArmorComponent.ShotMatches(item.Key.BodyPartColliderType, item.Key.ArmorPlateCollider))
				{
					num += item.Value;
				}
			}
			if (!(num <= 0f))
			{
				num = preAllocatedArmorComponent.ApplyExplosionDurabilityDamage(num, damageInfo, _preAllocatedArmorComponents);
				method_96(num, preAllocatedArmorComponent);
			}
		}
	}

	public bool IsShotDeflectedByHeavyArmor(EBodyPartColliderType colliderType, EArmorPlateCollider armorPlateCollider, int shotSeed)
	{
		if (!Skills.HeavyVestNoBodyDamageDeflectChance)
		{
			return false;
		}
		BackendConfigSettingsClass.GClass1790 heavyVests = Singleton<BackendConfigSettingsClass>.Instance.SkillsSettings.HeavyVests;
		_preAllocatedArmorComponents.Clear();
		Inventory.GetPutOnArmorsNonAlloc(_preAllocatedArmorComponents);
		foreach (ArmorComponent preAllocatedArmorComponent in _preAllocatedArmorComponents)
		{
			RepairableComponent repairable = preAllocatedArmorComponent.Repairable;
			if (preAllocatedArmorComponent.ArmorType == EArmorType.Heavy && !(repairable.Durability < heavyVests.RicochetChanceHVestsCurrentDurabilityThreshold * repairable.MaxDurability) && !(repairable.Durability < heavyVests.RicochetChanceHVestsMaxDurabilityThreshold * (float)repairable.TemplateDurability) && preAllocatedArmorComponent.ShotMatches(colliderType, armorPlateCollider) && _heavyVestsDeflectRandoms.GetRandom(shotSeed) < heavyVests.RicochetChanceHVestsEliteLevel)
			{
				return true;
			}
		}
		return false;
	}

	public bool method_95(EBodyPartColliderType colliderType)
	{
		if (!Skills.LightVestBleedingProtection)
		{
			return false;
		}
		_preAllocatedArmorComponents.Clear();
		Inventory.GetPutOnArmorsNonAlloc(_preAllocatedArmorComponents);
		foreach (ArmorComponent preAllocatedArmorComponent in _preAllocatedArmorComponents)
		{
			if (preAllocatedArmorComponent.IsDestroyed || preAllocatedArmorComponent.ArmorType != EArmorType.Light)
			{
				continue;
			}
			foreach (EBodyPartColliderType armorCollider in preAllocatedArmorComponent.ArmorColliders)
			{
				if (armorCollider == colliderType)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void method_96(float armorDamage, ArmorComponent armorComponent)
	{
		if (!(armorDamage > 0.1f) || armorComponent.IsDestroyed)
		{
			return;
		}
		switch (armorComponent.ArmorType)
		{
		case EArmorType.Heavy:
			ExecuteSkill((Action)delegate
			{
				Skills.HeavyArmorDamageTakenAction.Complete(armorDamage);
			});
			break;
		case EArmorType.Light:
			ExecuteSkill((Action)delegate
			{
				Skills.LightArmorDamageTakenAction.Complete(armorDamage);
			});
			break;
		}
	}

	[CanBeNull]
	public List<ArmorComponent> ProceedDamageThroughArmor(ref DamageInfoStruct damageInfo, EBodyPartColliderType colliderType, EArmorPlateCollider armorPlateCollider, bool damageInfoIsLocal = true)
	{
		_preAllocatedArmorComponents.Clear();
		Inventory.GetPutOnArmorsNonAlloc(_preAllocatedArmorComponents);
		List<ArmorComponent> list = null;
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		using (List<ArmorComponent>.Enumerator enumerator = _preAllocatedArmorComponents.GetEnumerator())
		{
			while (enumerator.MoveNext() && !(flag3 = enumerator.Current.Item.TemplateId == (MongoID)GClass3382.InvincibleBalaclava))
			{
			}
		}
		for (int i = 0; i < _preAllocatedArmorComponents.Count; i++)
		{
			ArmorComponent armorComponent = _preAllocatedArmorComponents[i];
			float num = 0f;
			if (armorComponent.ShotMatches(colliderType, armorPlateCollider))
			{
				if (flag || flag2)
				{
					float num2 = armorComponent.BluntThroughput;
					if (armorComponent.ArmorType == EArmorType.Heavy)
					{
						num2 *= 1f - (float)Skills.HeavyVestBluntThroughputDamageReduction;
					}
					damageInfo.Damage *= num2;
				}
				else
				{
					if (list == null)
					{
						list = new List<ArmorComponent>();
					}
					list.Add(armorComponent);
					if (_healthController.IsAlive)
					{
						num = armorComponent.ApplyDamage(ref damageInfo, colliderType, armorPlateCollider, damageInfoIsLocal, _preAllocatedArmorComponents, Skills.LightVestMeleeWeaponDamageReduction, Skills.HeavyVestBluntThroughputDamageReduction);
						method_96(num, armorComponent);
					}
					flag = (MongoID?)armorComponent.Item.Id == damageInfo.BlockedBy;
					flag2 = (MongoID?)armorComponent.Item.Id == damageInfo.DeflectedBy;
				}
			}
			if (num > 0.1f)
			{
				OnArmorPointsChanged(armorComponent);
			}
		}
		if (flag3)
		{
			damageInfo.Damage = 0f;
		}
		return list;
	}

	public virtual void OnArmorPointsChanged(ArmorComponent armor, bool children = false)
	{
	}

	public virtual void OnSideEffectApplied(SideEffectComponent sideEffect)
	{
	}

	public void SetDogtagInfo(GStruct368 deathPacket)
	{
		EPlayerSide ePlayerSide = (EPlayerSide)deathPacket.Side;
		switch ((WildSpawnType)deathPacket.Role)
		{
		case WildSpawnType.pmcUSEC:
			ePlayerSide = EPlayerSide.Usec;
			break;
		case WildSpawnType.pmcBEAR:
			ePlayerSide = EPlayerSide.Bear;
			break;
		}
		if (ePlayerSide == EPlayerSide.Savage)
		{
			return;
		}
		Item containedItem = Equipment.GetSlot(EquipmentSlot.Dogtag).ContainedItem;
		if (containedItem == null)
		{
			UnityEngine.Debug.LogErrorFormat("> DogTag slot item is null somehow. Side: {0}. Name {1}", ePlayerSide, FullIdInfo);
			return;
		}
		DogtagComponent itemComponent = containedItem.GetItemComponent<DogtagComponent>();
		if (itemComponent != null)
		{
			itemComponent.Item.SpawnedInSession = true;
			itemComponent.AccountId = deathPacket.AccountId;
			itemComponent.ProfileId = deathPacket.ProfileId;
			itemComponent.Nickname = deathPacket.Nickname;
			itemComponent.KillerAccountId = deathPacket.KillerAccountId;
			itemComponent.KillerProfileId = deathPacket.KillerProfileId;
			itemComponent.KillerName = deathPacket.KillerName;
			itemComponent.Side = ePlayerSide;
			itemComponent.Level = deathPacket.Level;
			itemComponent.Time = EFTDateTimeClass.UniversalDateTimeFromUnixTime(deathPacket.Time).ToLocalTime();
			itemComponent.Status = deathPacket.Status;
			itemComponent.WeaponName = deathPacket.WeaponName;
			itemComponent.GroupId = GroupId;
		}
		else
		{
			UnityEngine.Debug.LogError("> DogTagComponent on dog tag slot is null. Something went horrifically wrong!");
		}
	}

	public void RecalculateEquipmentParams()
	{
		float num = 0f;
		float num2 = 0f;
		float ergonomicsPenalty = ErgonomicsPenalty;
		_preAllocatedArmorComponents.Clear();
		Inventory.GetPutOnArmorsNonAlloc(_preAllocatedArmorComponents);
		EArmorPlateCollider eArmorPlateCollider = (EArmorPlateCollider)0;
		foreach (ArmorComponent preAllocatedArmorComponent in _preAllocatedArmorComponents)
		{
			if (!(preAllocatedArmorComponent.Repairable.Durability < Mathf.Epsilon))
			{
				eArmorPlateCollider |= preAllocatedArmorComponent.ArmorPlateCollidersMask;
			}
		}
		if ((bool)PlayerBones)
		{
			PlayerBones.SetArmorPlateCollidersState(eArmorPlateCollider);
		}
		_ergonomicsPenalty = 0f;
		for (int i = 0; i < _preAllocatedArmorComponents.Count; i++)
		{
			ArmorComponent armorComponent = _preAllocatedArmorComponents[i];
			float num3 = 0f;
			if (armorComponent.ArmorType == EArmorType.Light)
			{
				num3 = Skills.LightVestMoveSpeedPenaltyReduction;
			}
			if (armorComponent.ArmorType == EArmorType.Heavy)
			{
				num3 = Skills.HeavyVestMoveSpeedPenaltyReduction;
			}
			num += armorComponent.SpeedPenalty * (1f - num3);
			num2 += armorComponent.MousePenalty * (1f - num3);
			_ergonomicsPenalty += armorComponent.WeaponErgonomicPenalty;
		}
		_preAllocatedBackpackPenaltyComponent = Inventory.GetPutOnBackpack();
		if (_preAllocatedBackpackPenaltyComponent != null)
		{
			num += _preAllocatedBackpackPenaltyComponent.Template.SpeedPenaltyPercent;
			num2 += _preAllocatedBackpackPenaltyComponent.Template.MousePenalty;
			_ergonomicsPenalty += _preAllocatedBackpackPenaltyComponent.Template.WeaponErgonomicPenalty;
		}
		_preAllocatedArmorComponents.Clear();
		_ergonomicsPenalty /= 100f;
		RemoveStateSpeedLimit(ESpeedLimit.Armor);
		if (num < 0f)
		{
			AddStateSpeedLimit((100f + num) / 100f * MovementContext.MaxSpeed, ESpeedLimit.Armor);
		}
		if (Math.Abs(_ergonomicsPenalty - ergonomicsPenalty) > 0f)
		{
			ProceduralWeaponAnimation.UpdateWeaponVariables();
		}
		RemoveMouseSensitivityModifier(EMouseSensitivityModifier.Armor);
		if (num2 < 0f)
		{
			AddMouseSensitivityModifier(EMouseSensitivityModifier.Armor, num2 / 100f);
		}
	}

	public virtual void ApplyHitDebuff(float damage, float staminaBurnRate, EBodyPart bodyPartType, EDamageType damageType)
	{
		if (GClass3051.IsEnemyDamage(damageType))
		{
			IncreaseAwareness(20f);
		}
		if (HealthController.IsAlive && (!MovementContext.PhysicalConditionIs(EPhysicalCondition.OnPainkillers) || damage > 4f) && !IsAI)
		{
			if (Speaker != null)
			{
				Speaker.Play(EPhraseTrigger.OnBeingHurt, HealthStatus, demand: true);
			}
			else
			{
				UnityEngine.Debug.LogError("Player Speaker is null");
			}
		}
		if (GClass3051.IsWeaponInduced(damageType))
		{
			_accumulatedDebuffDamage = ((_lastHitDebuffFrame == Time.frameCount) ? (_accumulatedDebuffDamage + staminaBurnRate) : staminaBurnRate);
			float num = Mathf.InverseLerp(55f, 10f, _accumulatedDebuffDamage);
			if (num < 1f)
			{
				UpdateSpeedLimit(num, ESpeedLimit.Shot, 0.66f);
			}
			_lastHitDebuffFrame = Time.frameCount;
			Physical.BulletHit(staminaBurnRate);
			if ((bodyPartType == EBodyPart.LeftLeg || bodyPartType == EBodyPart.RightLeg) && !MovementContext.PhysicalConditionIs(EPhysicalCondition.OnPainkillers))
			{
				Physical.Sprint(target: false);
			}
		}
	}

	public virtual bool SetShotStatus(BodyPartCollider bodypart, EftBulletClass shot, Vector3 hitpoint, Vector3 shotNormal, Vector3 shotDirection)
	{
		_preAllocatedArmorComponents.Clear();
		Inventory.GetPutOnArmorsNonAlloc(_preAllocatedArmorComponents);
		ArmorPlateCollider armorPlateCollider = bodypart as ArmorPlateCollider;
		EArmorPlateCollider armorPlateCollider2 = ((!(armorPlateCollider == null)) ? armorPlateCollider.ArmorPlateColliderType : ((EArmorPlateCollider)0));
		int num = 0;
		while (true)
		{
			if (num < _preAllocatedArmorComponents.Count)
			{
				ArmorComponent armorComponent = _preAllocatedArmorComponents[num];
				if (armorComponent.ShotMatches(bodypart.BodyPartColliderType, armorPlateCollider2))
				{
					if (armorComponent.Deflects(shotDirection, shotNormal, shot))
					{
						break;
					}
					if (!shot.BlockedBy.HasValue)
					{
						armorComponent.SetPenetrationStatus(shot);
					}
				}
				num++;
				continue;
			}
			return false;
		}
		return true;
	}

	public bool CheckArmorHitByDirection(BodyPartCollider bodyPart)
	{
		_preAllocatedArmorComponents.Clear();
		Inventory.GetPutOnArmorsNonAlloc(_preAllocatedArmorComponents);
		ArmorPlateCollider armorPlateCollider = bodyPart as ArmorPlateCollider;
		EArmorPlateCollider armorPlateCollider2 = ((!(armorPlateCollider == null)) ? armorPlateCollider.ArmorPlateColliderType : ((EArmorPlateCollider)0));
		foreach (ArmorComponent preAllocatedArmorComponent in _preAllocatedArmorComponents)
		{
			if (preAllocatedArmorComponent.ShotMatches(bodyPart.BodyPartColliderType, armorPlateCollider2))
			{
				return true;
			}
		}
		return false;
	}

	public bool TryGetArmorResistData(BodyPartCollider bodyPart, float penetrationPower, out ArmorResistanceStruct armorResistanceData)
	{
		armorResistanceData = default(ArmorResistanceStruct);
		_preAllocatedArmorComponents.Clear();
		Inventory.GetPutOnArmorsNonAlloc(_preAllocatedArmorComponents);
		ArmorPlateCollider armorPlateCollider = bodyPart as ArmorPlateCollider;
		EArmorPlateCollider armorPlateCollider2 = ((!(armorPlateCollider == null)) ? armorPlateCollider.ArmorPlateColliderType : ((EArmorPlateCollider)0));
		int num = 0;
		ArmorComponent armorComponent;
		while (true)
		{
			if (num < _preAllocatedArmorComponents.Count)
			{
				armorComponent = _preAllocatedArmorComponents[num];
				if (armorComponent.ShotMatches(bodyPart.BodyPartColliderType, armorPlateCollider2))
				{
					break;
				}
				num++;
				continue;
			}
			return false;
		}
		armorResistanceData = GClass659.RealResistance(armorComponent.Repairable.Durability, armorComponent.Repairable.TemplateDurability, armorComponent.ArmorClass, penetrationPower);
		return true;
	}

	public virtual ShotInfoClass ApplyShot(DamageInfoStruct damageInfo, EBodyPart bodyPartType, EBodyPartColliderType colliderType, EArmorPlateCollider armorPlateCollider, ShotIdStruct shotId)
	{
		if (!_healthController.IsAlive)
		{
			return null;
		}
		IPlayerOwner player = damageInfo.Player;
		if (player != null && player.IsAI)
		{
			BotOwner botOwner = damageInfo.Player.AIData?.BotOwner;
			if (botOwner != null && !botOwner.ShouldApplyDamage(this, damageInfo, bodyPartType))
			{
				return null;
			}
		}
		bool hasValue = damageInfo.DeflectedBy.HasValue;
		float damage = damageInfo.Damage;
		List<ArmorComponent> list = ProceedDamageThroughArmor(ref damageInfo, colliderType, armorPlateCollider);
		method_97(list);
		MaterialType material = (hasValue ? MaterialType.HelmetRicochet : ((list == null || list.Count < 1) ? MaterialType.Body : list[0].Material));
		ShotInfoClass shotInfoClass = new ShotInfoClass
		{
			PoV = PointOfView,
			Penetrated = damageInfo.Penetrated,
			Material = material
		};
		float num = damage - damageInfo.Damage;
		ProceedLocalAbsorbedDamage(ref damageInfo, num);
		ApplyDamageInfo(damageInfo, bodyPartType, colliderType, 0f);
		ShotReactions(damageInfo, bodyPartType);
		ReceiveDamage(damageInfo.Damage, bodyPartType, damageInfo.DamageType, num, shotInfoClass.Material);
		return shotInfoClass;
	}

	public void method_97(List<ArmorComponent> damagedArmor)
	{
		if (damagedArmor == null)
		{
			return;
		}
		bool flag = false;
		foreach (ArmorComponent item in damagedArmor)
		{
			if (!(item.Repairable.Durability > Mathf.Epsilon))
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			RecalculateEquipmentParams();
		}
	}

	public virtual void ProceedLocalAbsorbedDamage(ref DamageInfoStruct damageInfo, float absorbedDamage)
	{
	}

	public virtual void ApplyDamageInfo(DamageInfoStruct damageInfo, EBodyPart bodyPartType, EBodyPartColliderType colliderType, float absorbed)
	{
		if (!_healthController.IsAlive)
		{
			return;
		}
		EDamageType damageType = damageInfo.DamageType;
		LastDamagedBodyPart = bodyPartType;
		IPlayerOwner player = damageInfo.Player;
		Player player2 = ((player != null) ? GameWorld.GetAlivePlayerByProfileID(player.iPlayer.ProfileId) : null);
		if (ActiveHealthController != null)
		{
			ActiveHealthController.DoWoundRelapse(damageInfo.Damage, bodyPartType);
			LastAggressor = player?.iPlayer;
			LastDamageInfo = damageInfo;
			LastBodyPart = bodyPartType;
			damageInfo.BleedBlock = method_95(colliderType);
			float value = (damageInfo.DidBodyDamage = ActiveHealthController.ApplyDamage(bodyPartType, damageInfo.Damage, damageInfo));
			ActiveHealthController.BluntContusion(bodyPartType, absorbed);
			if (GClass855.Positive(value) && ActiveHealthController.TryApplySideEffects(damageInfo, bodyPartType, out var sideEffectComponent) && player2 != null)
			{
				player2.OnSideEffectApplied(sideEffectComponent);
			}
		}
		else
		{
			damageInfo.DidBodyDamage = 0f;
		}
		player2?.Loyalty.MarkAsAggressor(this);
		ManageAggressor(damageInfo, bodyPartType, colliderType);
		ApplyHitDebuff(damageInfo.Damage, damageInfo.StaminaBurnRate * damageInfo.Damage, bodyPartType, damageType);
		if (!GClass3051.IsWeaponInduced(damageType))
		{
			ReceiveDamage(damageInfo.Damage, bodyPartType, damageType, 0f, MaterialType.None);
		}
		this.BeingHitAction?.Invoke(damageInfo, bodyPartType, 0f);
		if (Singleton<BotEventHandler>.Instantiated)
		{
			Singleton<BotEventHandler>.Instance.BeingHitAction(damageInfo, this);
		}
		if (player != null && !HealthController.IsAlive && Singleton<BotEventHandler>.Instantiated)
		{
			Singleton<BotEventHandler>.Instance.Kill(player.iPlayer, GetPlayer);
		}
	}

	public virtual void AddDetailedHitInfo(EDamageType damageType, int d, int absorbed, int staminaLoss, EBodyPart part, MaterialType special)
	{
	}

	public virtual bool ShouldVocalizeDeath(EBodyPart bodyPart)
	{
		return bodyPart != EBodyPart.Head;
	}

	public virtual void OnBeenKilledByAggressor(IPlayer aggressor, DamageInfoStruct damageInfo, EBodyPart bodyPart, EDamageType lethalDamageType)
	{
		if (AggressorFound || this == aggressor)
		{
			return;
		}
		AggressorFound = true;
		aggressor.AIData?.KillEnemy(this);
		Player alivePlayerByProfileID = GameWorld.GetAlivePlayerByProfileID(aggressor.ProfileId);
		if (!(alivePlayerByProfileID == null))
		{
			bool isFriendly = false;
			if (alivePlayerByProfileID.AIData != null && !alivePlayerByProfileID.AIData.IsAI && AIData != null && AIData.BotOwner != null)
			{
				isFriendly = (Profile.Info.Settings.Role == WildSpawnType.exUsec && AIData.BotOwner.BotsController.BotTradersServices.LighthouseKeeperServices.IsPlayerExUsecFriendly(alivePlayerByProfileID)) | (BotSettingsRepoClass.IsSectant(Profile.Info.Settings.Role) && _inventoryController.HasCultistAmulet(out var _));
			}
			float distance = Vector3.Distance(aggressor.Position, Position);
			alivePlayerByProfileID.StatisticsManager.OnEnemyKill(damageInfo, lethalDamageType, bodyPart, Profile.Info.Side, Profile.Info.Settings.Role, Profile.AccountId, Profile.Id, Profile.Nickname, Profile.Info.GroupId, Profile.Info.Level, Profile.Info.Settings.Experience, distance, CurrentHour, Inventory.EquippedInSlotsTemplateIds, HealthController.BodyPartEffects, TriggerZones, isFriendly, IsAI);
		}
	}

	public virtual void OnDead(EDamageType damageType)
	{
		GClass4062.ReleaseBeginSample("Player.OnDead", "OnDead");
		if (LastAggressor != null)
		{
			OnBeenKilledByAggressor(LastAggressor, LastDamageInfo, LastBodyPart, damageType);
		}
		if (BackendConfigAbstractClass.Config.UseSpiritPlayer)
		{
			Spirit.Die();
		}
		LastDamageType = damageType;
		GClass4062.ReleaseBeginSample("Player.OnDead.OnPlayerDeadInvoke", "OnDead");
		this.OnPlayerDead?.Invoke(this, LastAggressor, LastDamageInfo, LastBodyPart);
		Player.OnPlayerDeadStatic?.Invoke(this, LastAggressor, LastDamageInfo, LastBodyPart);
		this.OnPlayerDeadOrUnspawn?.Invoke(this);
		this.OnIPlayerDeadOrUnspawn?.Invoke(this);
		GClass4062.ReleaseBeginSample("Player.OnDead.SoundWork", "OnDead");
		if (ShouldVocalizeDeath(LastDamagedBodyPart))
		{
			EPhraseTrigger trigger = (GClass3051.IsWeaponInduced(LastDamageType) ? EPhraseTrigger.OnDeath : EPhraseTrigger.OnAgony);
			try
			{
				Speaker.Play(trigger, HealthStatus, demand: true);
			}
			catch (Exception ex)
			{
				UnityEngine.Debug.LogError(ex.Message);
			}
		}
		else
		{
			Speaker.Shut();
		}
		MovementContext.ReleaseDoorIfInteractingWithOne();
		PlayDeathSound();
		MovementContext.OnStateChanged -= method_17;
		MovementContext.PhysicalConditionChanged -= ProceduralWeaponAnimation.PhysicalConditionUpdated;
		InventoryController.UnregisterView(this);
		GClass4062.ReleaseBeginSample("Player.OnDead._exfilUnsubscribe", "OnDead");
		ExfilUnsubscribe();
		EnabledAnimators = (EAnimatorMask)0;
		GClass4062.ReleaseBeginSample("Player.OnDead.DisableAnimators", "OnDead");
		BodyAnimatorCommon.enabled = false;
		if (BackendConfigAbstractClass.Config.UseBodyFastAnimator)
		{
			PlayerBones.PlayableAnimator.Stop();
		}
		ArmsAnimatorCommon.enabled = false;
		GClass4062.ReleaseBeginSample("Player.OnDead.DisableCharacterController", "OnDead");
		_characterController.isEnabled = false;
		Skills?.Terminate();
		Physical.Unsubscribe();
		if (POM != null)
		{
			POM.Off();
		}
		try
		{
			if (HandsController != null)
			{
				HandsController.OnPlayerDead();
			}
		}
		catch (Exception arg)
		{
			UnityEngine.Debug.LogError($"Safe ex: {arg}");
		}
		FastForwardCurrentOperations();
		GClass4062.ReleaseBeginSample("Player.OnDead.InteractionInfoCallback", "OnDead");
		MovementContext.InteractionInfo.Callback?.Invoke();
		_healthController.DiedEvent -= OnDead;
		if (_propActive)
		{
			_propTransforms[0].parent = PlayerBones.LeftPalm;
			if ((bool)_compassArrow)
			{
				_compassArrow.enabled = false;
			}
			_propActive = false;
		}
		else
		{
			HandPosers[0].Lerp2Target(EFTHardSettings.Instance.LEFT_HAND_QTS, 5f);
		}
		bool usedSimplifiedSkeleton;
		if (usedSimplifiedSkeleton = UsedSimplifiedSkeleton)
		{
			List<Item> list = GClass3380.GetFirstLevelItems(_inventoryController.Inventory.Equipment).ToList();
			_ = list.Count;
			int num = 0;
			foreach (Item item in list)
			{
				GStruct154<GClass3410> gStruct = InteractionsHandlerClass.RemoveWithoutRestrictions(item, _inventoryController);
				if (gStruct.Failed)
				{
					UnityEngine.Debug.LogError($"Error during remove zombie loot: \n{gStruct.Error}");
					num++;
				}
			}
		}
		InteractionsHandlerClass.DestroyOverLimit(Equipment, InventoryController);
		Corpse = CreateCorpse();
		Corpse.IsZombieCorpse = usedSimplifiedSkeleton;
		ApplyCorpseImpulse();
		if (_triggerColliderSearcher != null)
		{
			_triggerColliderSearcher.IsEnabled = false;
		}
		GClass4062.ReleaseBeginSample("Player.OnDead.OnDeadCoroutine", "OnDead");
		if (MovementContext.StationaryWeapon != null)
		{
			MovementContext.StationaryWeapon.Unlock(ProfileId);
		}
		if (MovementContext.StationaryWeapon != null && MovementContext.StationaryWeapon.Item == _handsController.Item)
		{
			MovementContext.StationaryWeapon.Show();
			ReleaseHand();
		}
		else
		{
			Corpse.SetItemInHandsLootedCallback(ReleaseHand);
			StartCoroutine(method_98());
		}
	}

	public virtual void PlayDeathSound()
	{
		if (base.gameObject.GetComponent<GInterface209>() != null && this is ClientPlayer)
		{
			float time = 0f;
			if (Speaker != null && Speaker.Speaking)
			{
				time = Speaker.TimeLeft;
			}
			GClass855.WaitSeconds(StaticManager.Instance, time, delegate
			{
				Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.PlayerIsDead);
			});
		}
	}

	public IEnumerator method_98()
	{
		yield return null;
		if (_handsController != null)
		{
			DropItemDead(_handsController.Item, _handsController.ControllerGameObject);
		}
		if (GameWorld != null)
		{
			GameWorld.UnregisterPlayer(this);
		}
	}

	public virtual Corpse CreateCorpse()
	{
		return CreateCorpse<Corpse>(Velocity);
	}

	public T CreateCorpse<T>(Vector3 velocity) where T : Corpse
	{
		FirearmController firearmController = HandsController as FirearmController;
		GClass768 containerCollectionView = null;
		if (firearmController != null)
		{
			containerCollectionView = firearmController.CCV;
		}
		return Corpse.CreateCorpse<T>(base.gameObject, ProfileId, Equipment, Profile.Customization, reinitBody: false, GameWorld, Side, velocity, PlayerBones.Pelvis.Original, _itemInHands, foreStillCorpse: false, containerCollectionView, InventoryController.CurrentId);
	}

	public virtual void ApplyCorpseImpulse()
	{
		float hIT_FORCE = EFTHardSettings.Instance.HIT_FORCE;
		hIT_FORCE = (_corpseAppliedForce = hIT_FORCE * (0.3f + 0.7f * Mathf.InverseLerp(50f, 20f, LastDamageInfo.PenetrationPower)));
		Corpse.Ragdoll.ApplyImpulse(LastDamageInfo.HitCollider, LastDamageInfo.Direction, LastDamageInfo.HitPoint, hIT_FORCE);
	}

	public void ExitTriggerStatusChanged(bool status)
	{
		ExitTriggerZone = status;
		OnExitTriggerVisited.Invoke();
	}

	public virtual void SetExfiltrationPoint(ExfiltrationPoint point, bool entered)
	{
		this.OnEpInteraction?.Invoke(point, entered);
		ExfiltrationPoint = (entered ? point : null);
		this.PossibleInteractionsChanged?.Invoke();
	}

	public void SearchForInteractions()
	{
		_nextCastHasForceEvent = true;
		this.PossibleInteractionsChanged?.Invoke();
	}

	public void ForceInteractionsChanged()
	{
		this.PossibleInteractionsChanged?.Invoke();
	}

	public virtual void vmethod_7(EInteraction gesture)
	{
		MovementContext.SetInteractInHands(gesture);
	}

	public virtual void TriggerPhraseCommand(EPhraseTrigger phrase, int netPhraseId)
	{
	}

	public void AddAlly(IPlayer enemy)
	{
	}

	public void SetGroup(BotsGroup newBotsGroup)
	{
	}

	public virtual void KillMe(EBodyPartColliderType colliderType, float damage)
	{
		if (HealthController.IsAlive)
		{
			BodyPartCollider bodyPartCollider = _hitColliders.First((BodyPartCollider hitCollider) => hitCollider.BodyPartColliderType.Equals(colliderType));
			DamageInfoStruct damageInfo = new DamageInfoStruct
			{
				DamageType = EDamageType.Sniper,
				Damage = damage,
				Direction = Transform.forward,
				HitCollider = bodyPartCollider.Collider
			};
			ApplyShot(damageInfo, bodyPartCollider.BodyPartType, colliderType, (EArmorPlateCollider)0, ShotIdStruct.EMPTY_SHOT_ID);
		}
	}

	public virtual void DevelopResetDiscardLimits()
	{
		if (GClass867.Is(Profile.Info.MemberCategory, EMemberCategory.Developer) && InventoryController is GInterface416 gInterface)
		{
			gInterface.ResetDiscardLimits();
		}
	}

	public virtual void DevelopSetEncodedRadioTransmitter(bool value)
	{
	}

	public virtual void DevelopSetActiveLighthouseTraderZoneDebug(bool value)
	{
	}

	public virtual void GetRadioTransmitterStatusFromServer()
	{
	}

	public virtual void KillAIs()
	{
	}

	public virtual void SetEventState(EEventState value)
	{
	}

	public virtual void SpawnAI(int count)
	{
	}

	public virtual void DevelopUnlockDoors(bool openDoors)
	{
	}

	public virtual void Heal(EBodyPart bodyPart, float value)
	{
	}

	public virtual void DebugSnapshotAllPlayers()
	{
	}

	public virtual void DebugSpawnAirdrop(bool spawnNearPlayer, Vector3 playerPosition)
	{
	}

	public void SetCallbackForInteraction(Action<Action> cb)
	{
		_openAction = cb;
	}

	public virtual void TryInteractionCallback(LootableContainer container)
	{
		if (container != null)
		{
			_openAction?.Invoke(delegate
			{
				container.Interact(new InteractionResult(EInteractionType.Close));
				if (MovementContext.LevelOnApproachStart > 0f)
				{
					MovementContext.SetPoseLevel(MovementContext.LevelOnApproachStart);
					MovementContext.LevelOnApproachStart = -1f;
				}
			});
		}
		_openAction = null;
	}

	public void method_99()
	{
		Pedometer.Start();
		StatisticsManager.BeginStatisticsSession();
	}

	public virtual void OnGameSessionEnd(ExitStatus exitStatus, float pastTime, string locationId, string exitName)
	{
		if (_gameSessionEndWasCalled)
		{
			return;
		}
		try
		{
			_gameSessionEndWasCalled = true;
			Pedometer.Stop();
			Int32_0 = 0;
			ExecuteSkill((Action)delegate
			{
				Skills.LowHPDuration.Complete();
			});
			ExecuteSkill((Action)delegate
			{
				Skills.OnlineAction.Complete((float)StatisticsManager.CurrentSessionLength.TotalHours);
			});
			Profile.EftStats.LastPlayerState = null;
			StatisticsManager.EndStatisticsSession(exitStatus, pastTime);
			foreach (GInterface518 item in IEnumerable_0)
			{
				item.CheckExitConditionCounters(exitStatus, pastTime, locationId, exitName, HealthController.BodyPartEffects, TriggerZones);
			}
			if (AbstractQuestControllerClass is GClass4007 gClass)
			{
				gClass.ManageQuestStatusesForPveOfflineGameEnd();
			}
			if (exitStatus != ExitStatus.Transit)
			{
				_questController?.ResetCurrentNullableCounters();
			}
			_achievementsController?.ResetCurrentNullableCounters();
			method_118();
			if (exitStatus != ExitStatus.Killed && exitStatus != ExitStatus.Left)
			{
				method_119();
			}
			method_120();
			if (MovementContext.Platform != null)
			{
				GetOff(MovementContext.Platform);
			}
			Skills.OnSkillLevelChanged -= OnSkillLevelChanged;
			Skills.OnSkillExperienceChanged -= OnSkillExperienceChanged;
			Skills.WeaponMastered -= OnWeaponMastered;
			Skills.OnMasteringExperienceChanged -= OnMasteringExperienceChanged;
			Skills.ImmunityAvoidPoisonChance.OnResult -= method_100;
			Skills.ImmunityAvoidMiscEffectsChance.OnResult -= method_100;
			StatisticsManager.OnUniqueLoot -= method_110;
			MovementContext.OnStateChanged -= method_116;
			HealthController.ApplyDamageEvent -= method_112;
			HealthController.EnergyChangedEvent -= method_115;
			HealthController.HydrationChangedEvent -= method_111;
			HealthController.EffectResidualEvent -= method_113;
			HealthController.StimulatorBuffActivationEvent -= method_103;
			HealthController.TemperatureChangedEvent -= method_104;
			SearchController.OnItemFound -= method_105;
			SearchController.OnItemFullySearchedEvent -= method_106;
			InventoryController.OnAmmoLoaded -= method_107;
			InventoryController.OnAmmoUnloaded -= method_108;
			InventoryController.OnMagazineCheck -= method_109;
			HealthController.EffectStartedEvent -= OnHealthEffectAdded;
			HealthController.EffectResidualEvent -= OnHealthEffectRemoved;
			HealthController.HealthChangedEvent -= method_87;
			HealthController.BodyPartDestroyedEvent -= method_85;
			HealthController.BodyPartRestoredEvent -= method_84;
			Profile.OnTraderStandingChanged -= TraderStandingHandler;
			_unsubscribeOnEndSession?.Invoke();
			_unsubscribeOnEndSession = null;
			UnsubscribeVisualEvents();
		}
		catch (Exception arg)
		{
			UnityEngine.Debug.LogError($"Safe ex: {arg}");
		}
	}

	public virtual void ConnectSkillManager()
	{
		Skills.OnSkillLevelChanged += OnSkillLevelChanged;
		Skills.OnSkillExperienceChanged += OnSkillExperienceChanged;
		Skills.WeaponMastered += OnWeaponMastered;
		Skills.OnMasteringExperienceChanged += OnMasteringExperienceChanged;
		Skills.ImmunityAvoidPoisonChance.OnResult += method_100;
		Skills.ImmunityAvoidMiscEffectsChance.OnResult += method_100;
		StatisticsManager.OnUniqueLoot += method_110;
		MovementContext.OnStateChanged += method_116;
		HealthController.ApplyDamageEvent += method_112;
		HealthController.EnergyChangedEvent += method_115;
		HealthController.HydrationChangedEvent += method_111;
		HealthController.EffectResidualEvent += method_113;
		HealthController.StimulatorBuffActivationEvent += method_103;
		HealthController.TemperatureChangedEvent += method_104;
		SearchController.OnItemFound += method_105;
		SearchController.OnItemFullySearchedEvent += method_106;
		InventoryController.OnAmmoLoaded += method_107;
		InventoryController.OnAmmoUnloaded += method_108;
		InventoryController.OnMagazineCheck += method_109;
		_unsubscribeOnEndSession = GlobalEventHandlerClass.Instance.SubscribeOnEvent(delegate(GClass3552 invokedEvent)
		{
			method_114(invokedEvent);
		});
	}

	public void method_100(bool result)
	{
		HealthController.AddImmunityNotificationEffect();
	}

	public void method_101(IEffect obj)
	{
		if (obj is GInterface343)
		{
			Skills.Dehydration.Begin();
		}
		else if (obj is GInterface344)
		{
			Skills.Exhaustion.Begin();
		}
	}

	public void method_102(IEffect obj)
	{
		if (obj is GInterface343)
		{
			ExecuteSkill((Action)delegate
			{
				Skills.Dehydration.Complete();
			});
		}
		else if (obj is GInterface344)
		{
			ExecuteSkill((Action)delegate
			{
				Skills.Exhaustion.Complete();
			});
		}
	}

	public void method_103(IPlayerBuff buff)
	{
		if (!GClass3018.IsBuff(buff.Settings.BuffType, buff.Settings.Value))
		{
			Int32_0 += (buff.Active ? 1 : (-1));
		}
		Skills.Endurance.OnLevelUp?.Invoke();
	}

	public void method_104(float tempCelsio)
	{
		PlayerBody.SetTemperatureForBody(tempCelsio);
	}

	public void method_105(Item item)
	{
		if (!(item is StackableItemItemClass))
		{
			IItemOwner owner = GClass3113.GetOwner(item.Parent);
			bool onCorpse = owner.RootItem is InventoryEquipment;
			ExecuteSkill((Action)delegate
			{
				Skills.FindAction.Complete(onCorpse);
			});
		}
	}

	public void method_106()
	{
		ExecuteSkill(Skills.SearchAction.Complete);
	}

	public void method_107(int count)
	{
		ExecuteSkill((Action)delegate
		{
			Skills.RaidLoadedAmmoAction.Complete(count);
		});
	}

	public void method_108(int count)
	{
		ExecuteSkill((Action)delegate
		{
			Skills.RaidUnloadedAmmoAction.Complete(count);
		});
	}

	public void method_109()
	{
		ExecuteSkill((Action)delegate
		{
			Skills.MagazineCheckAction.Complete();
		});
	}

	public void method_110()
	{
		ExecuteSkill((Action)delegate
		{
			Skills.UniqueLoot.Complete();
		});
	}

	public void method_111(float diff)
	{
		ExecuteSkill((Action)delegate
		{
			Skills.HydrationChanged.Complete(diff, diff);
		});
	}

	public void method_112(EBodyPart bodyPart, float damage, DamageInfoStruct damageInfo)
	{
		if (!GClass3051.IsSelfInflicted(damageInfo.DamageType))
		{
			ExecuteSkill((Action)delegate
			{
				Skills.DamageTakenAction.Complete(damage);
			});
		}
	}

	public void method_113(IEffect healthEffect)
	{
		if (!(healthEffect is GInterface376 gInterface))
		{
			return;
		}
		foreach (GInterface518 item in IEnumerable_0)
		{
			item.CheckUseItemCounter(gInterface.MedItem.TemplateId, gInterface.Amount, Location, TriggerZones);
		}
	}

	public void method_114(GClass3552 flareEvent)
	{
		if (!IsYourPlayer || !(flareEvent.PlayerProfileID == ProfileId) || flareEvent.ZoneEventType != GClass3552.EZoneEventType.FiredPlayerAddedInShotList)
		{
			return;
		}
		foreach (GInterface518 item in IEnumerable_0)
		{
			item.CheckShootFlareCounter(flareEvent.ZoneID);
		}
	}

	public void method_115(float diff)
	{
		ExecuteSkill((Action)delegate
		{
			Skills.EnergyChanged.Complete(diff, diff);
		});
	}

	public virtual void ExecuteSkill(Action action)
	{
		action();
	}

	public virtual void ExecuteSkill(Action<float> action)
	{
		action(1f);
	}

	public async Task ManageGameQuests()
	{
		await GameWorld.method_12(this);
		TransitControllerAbstractClass transitController = GameWorld.TransitController;
		if (transitController == null || !transitController.IsTransitPlayer(ProfileId, out int _))
		{
			_questController?.ResetCurrentNullableCounters();
		}
		_questController?.CheckMapTransitConditions(GameWorld.TransitController);
	}

	public void InitializeRecodableItemHandlers()
	{
		recodableItemsHandler = new RecodableItemsHandler(this);
	}

	public virtual void StartInflictSelfDamageCoroutine()
	{
		if (_selfDamage == null)
		{
			_selfDamage = StartCoroutine(InflictSelfDamage());
		}
	}

	public IEnumerator InflictSelfDamage()
	{
		while (_healthController.IsAlive)
		{
			_countdownToSprintDamage -= Time.deltaTime;
			if (_countdownToSprintDamage <= 0f && CurrentState.Name == EPlayerState.Sprint)
			{
				_countdownToSprintDamage = UnityEngine.Random.Range(1f, 1.5f);
				if (MovementContext.PhysicalConditionIs(EPhysicalCondition.LeftLegDamaged))
				{
					ActiveHealthController.ApplyDamage(EBodyPart.LeftLeg, 2f, GClass3051.FallDamage);
				}
				if (MovementContext.PhysicalConditionIs(EPhysicalCondition.RightLegDamaged))
				{
					ActiveHealthController.ApplyDamage(EBodyPart.RightLeg, 2f, GClass3051.FallDamage);
				}
			}
			yield return null;
		}
	}

	public void method_116(EPlayerState previousState, EPlayerState nextState)
	{
		Pedometer.CurrentState = nextState;
		switch (nextState)
		{
		case EPlayerState.ProneMove:
			MovementContext.CheckGroundedRayDistance = 0.08f;
			Pedometer.MakeMark(EPlayerState.ProneMove);
			break;
		case EPlayerState.Run:
			MovementContext.CheckGroundedRayDistance = 0.08f;
			Pedometer.MakeMark(EPlayerState.Run);
			break;
		case EPlayerState.Sprint:
			MovementContext.CheckGroundedRayDistance = 0.15f;
			Pedometer.MakeMark(EPlayerState.Sprint);
			if (MovementContext.PhysicalConditionIs(EPhysicalCondition.LeftLegDamaged) || MovementContext.PhysicalConditionIs(EPhysicalCondition.RightLegDamaged))
			{
				StartInflictSelfDamageCoroutine();
			}
			break;
		case EPlayerState.Jump:
			MovementContext.CheckGroundedRayDistance = 0.03f;
			ActiveHealthController?.DoWoundRelapse(1f, EBodyPart.Common);
			break;
		}
		switch (previousState)
		{
		case EPlayerState.ProneMove:
			if (MovementContext.IsGrounded)
			{
				ExecuteSkill((Action)delegate
				{
					Skills.ProneAction.Complete(Pedometer.GetDistanceFromMark(EPlayerState.ProneMove));
				});
			}
			break;
		case EPlayerState.Run:
			if (MovementContext.IsGrounded)
			{
				float distance2 = Pedometer.GetDistanceFromMark(EPlayerState.Run);
				ExecuteSkill((Action)delegate
				{
					Skills.MovementAction.Complete(new SkillManager.GStruct279
					{
						Noise = MovementContext.CovertNoiseLevel,
						Overweight = Physical.Overweight,
						Fatigue = (Fatigue?.Strength ?? 0f)
					}, distance2);
				});
			}
			break;
		case EPlayerState.Sprint:
			if (MovementContext.IsGrounded)
			{
				float distance = Pedometer.GetDistanceFromMark(EPlayerState.Sprint);
				ExecuteSkill((Action)delegate
				{
					Skills.SprintAction.Complete(new SkillManager.GStruct279
					{
						Overweight = Physical.Overweight,
						Fatigue = (Fatigue?.Strength ?? 0f)
					}, distance);
				});
				ActiveHealthController?.DoWoundRelapse(distance / 10f, EBodyPart.Common);
			}
			break;
		case EPlayerState.Jump:
		{
			bool flag = false;
			if (MovementContext.PhysicalConditionIs(EPhysicalCondition.LeftLegDamaged))
			{
				ActiveHealthController?.ApplyDamage(EBodyPart.LeftLeg, 3f, GClass3051.FallDamage);
				flag = true;
			}
			if (MovementContext.PhysicalConditionIs(EPhysicalCondition.RightLegDamaged))
			{
				ActiveHealthController?.ApplyDamage(EBodyPart.RightLeg, 3f, GClass3051.FallDamage);
				flag = true;
			}
			if (flag && !MovementContext.PhysicalConditionIs(EPhysicalCondition.OnPainkillers) && !IsAI)
			{
				Say(EPhraseTrigger.OnBeingHurt, demand: true);
			}
			break;
		}
		}
	}

	public virtual void SendHandsInteractionStateChanged(bool value, int animationId)
	{
		if (value)
		{
			MovementContext.SetBlindFire(0);
		}
	}

	public virtual void SetCompassState(bool value)
	{
	}

	public virtual void SetLauncherState(bool value)
	{
	}

	public virtual void SetAnimatorLayerWeight(int layer, int weight)
	{
	}

	public virtual void OnSkillLevelChanged(AbstractSkillClass skill)
	{
	}

	public virtual void OnSkillExperienceChanged(AbstractSkillClass skill)
	{
	}

	public virtual void OnWeaponMastered(MasterSkillClass masterSkill)
	{
	}

	public virtual void OnMasteringExperienceChanged(MasterSkillClass masterSkill)
	{
	}

	public void SpecialPlaceVisited(string id, int experience)
	{
		this.OnSpecialPlaceVisited?.Invoke(id, experience);
	}

	public IEnumerator method_117()
	{
		yield return new WaitForSeconds(2f);
		_renderers = (from x in GetComponentsInChildren<Renderer>(includeInactive: true)
			where x.enabled
			select x).ToArray();
	}

	public void SwitchRenderer(bool @switch)
	{
		Renderer[] renderers = _renderers;
		for (int i = 0; i < renderers.Length; i++)
		{
			renderers[i].enabled = @switch;
		}
	}

	public virtual void Teleport(Vector3 position, bool onServerToo = false)
	{
		MovementContext.TransformPosition = position;
		method_14();
		_dampVelocity = 0f;
		MovementContext.ResetFlying();
		if ((bool)EnvironmentManager.Instance)
		{
			EnvironmentManager.Instance.UpdateEnvironmentForPlayer(this);
		}
	}

	public void IncreaseAwareness(float aware = 5f)
	{
		Awareness = Mathf.Max(Awareness, Time.time + aware);
	}

	public virtual void Sleep(bool value)
	{
	}

	public bool HasBodyPartCollider(Collider collider)
	{
		return PlayerBones.BodyPartCollidersHashSet.Contains(collider);
	}

	public virtual void OnVaulting()
	{
	}

	public virtual void OnAnimatedInteraction(EInteraction interaction)
	{
	}

	public virtual void OnInventoryInteraction(bool inventory, bool isOperation = false)
	{
	}

	public virtual void OnMounting(MountingPacketStruct.EMountingCommand command)
	{
	}

	public virtual void ToggleLeftHand(Item item)
	{
		_leftHandController.ToggleHand(item);
	}

	public virtual void RemoveLeftHandItem(float speed = 1f)
	{
		_leftHandController.RemoveItem(speed);
	}

	public virtual void Dispose()
	{
		if (EnvironmentManager.Instance != null)
		{
			EnvironmentManager.Instance.OnPlayerEnvironmentChanged -= SetEnvironment;
		}
		method_81();
		foreach (GInterface518 item in IEnumerable_0)
		{
			item?.Dispose();
		}
		_questController = null;
		_achievementsController = null;
		_prestigeController = null;
		if (GameWorld != null)
		{
			GameWorld.UnregisterPlayer(this);
			if (GameWorld.SpeakerManager != null)
			{
				GameWorld.SpeakerManager.RemoveFromGroup(this);
			}
		}
		_voipSourceBinding?.Invoke();
		_voipSourceBinding = null;
		if (_playerBody != null && Corpse == null)
		{
			_playerBody.Dispose();
			_playerBody = null;
		}
		_compassInstantiated = false;
		if (_vaultingComponent != null)
		{
			UpdateEvent -= _vaultingComponent.DoVaultingTick;
		}
		if (_weaponMountingComponent != null)
		{
			HandsChangingEvent -= _weaponMountingComponent.CancelFindingPoint;
			_weaponMountingComponent.Dispose();
		}
		Physical?.Unsubscribe();
		if (HandsController != null)
		{
			AbstractHandsController handsController = HandsController;
			method_118();
			UnityEngine.Object.Destroy(handsController);
		}
		MovementContext?.Dispose();
		if (ExfiltrationPoint != null)
		{
			ExfiltrationPoint.Entered.Remove(this);
		}
		if (MovementContext != null && MovementContext.Platform != null)
		{
			GetOff(MovementContext.Platform);
		}
		this.OnPlayerDeadOrUnspawn?.Invoke(this);
		this.OnIPlayerDeadOrUnspawn?.Invoke(this);
		CompositeDisposable.Dispose();
	}

	public void method_118()
	{
		if (!(_handsController == null))
		{
			_handsController.Destroy();
			_handsController = null;
		}
	}

	public void method_119()
	{
		foreach (Item item in (from item in _inventoryController.Inventory.Equipment.ContainerSlots.Where((Slot slot) => slot.ContainedItem != null).SelectMany((Slot slot) => GClass3380.GetAllItems(slot.ContainedItem))
			where item.IsSecretExitRequirement
			select item).ToList())
		{
			GStruct154<GClass3410> gStruct = InteractionsHandlerClass.RemoveWithoutRestrictions(item, _inventoryController);
			if (gStruct.Failed)
			{
				UnityEngine.Debug.LogError($"Error during remove secret exit requirement items: \n{gStruct.Error}");
			}
		}
	}

	public void method_120()
	{
		Slot slot = _inventoryController.Inventory.Equipment.GetSlot(EquipmentSlot.SecuredContainer);
		if (slot.ContainedItem == null)
		{
			return;
		}
		foreach (Item item in (from item in GClass3380.GetAllItems(slot.ContainedItem)
			where item.IsSecretExitRequirement
			select item).ToList())
		{
			GStruct154<GClass3410> gStruct = InteractionsHandlerClass.RemoveWithoutRestrictions(item, _inventoryController);
			if (gStruct.Failed)
			{
				UnityEngine.Debug.LogError($"Error during remove secret exit requirement items from container: \n{gStruct.Error}");
			}
		}
	}

	public void Board(MovingPlatform platform)
	{
		if (MovementContext.Platform != null && MovementContext.Platform != platform)
		{
			GetOff(MovementContext.Platform);
		}
		if (!platform.Passengers.Contains(this))
		{
			platform.Passengers.Add(this);
		}
		MovementContext.Platform = platform;
	}

	public void GetOff(MovingPlatform platform)
	{
		if (!(MovementContext.Platform != platform))
		{
			platform.Passengers.Remove(this);
			MovementContext.Platform = null;
		}
	}

	public void HandleFlareSuccessEvent(Vector3 position, AmmoTemplate ammoTemplate)
	{
		Singleton<BotEventHandler>.Instance.SuccessFlare(this, position, ammoTemplate);
	}

	public bool AddDiscoveredSecretExit(SecretExfiltrationPoint secretExfiltrationPoint)
	{
		bool num = !FoundSecretExits.Contains(secretExfiltrationPoint);
		if (num)
		{
			FoundSecretExits.Add(secretExfiltrationPoint);
		}
		return num;
	}

	public virtual void InitVoip(EVoipState voipState)
	{
		VoipState = voipState;
		if (voipState != EVoipState.NotAvailable)
		{
			DissonanceComms = DissonanceComms.Instance;
		}
	}

	public void method_121()
	{
		VoiceBroadcastTrigger broadcastTrigger = base.gameObject.AddComponent<VoiceBroadcastTrigger>();
		broadcastTrigger.ChannelType = CommTriggerTarget.Self;
		SoundSettingsControllerClass settings = Singleton<SharedGameSettingsClass>.Instance.Sound.Settings;
		CompositeDisposable.BindState(settings.VoiceChatVolume, delegate(int value)
		{
			float volume = (float)value / 100f;
			broadcastTrigger.ActivationFader.Volume = volume;
		});
	}

	public void TrackPlayerPosition()
	{
		DissonanceComms?.TrackPlayerPosition(this);
	}

	public (bool, bool) IsHeard(in Vector3 voicePos, float sqrDistance)
	{
		if (DissonanceComms == null)
		{
			return (false, false);
		}
		bool num = method_122(in voicePos, sqrDistance);
		return (num, num && (VoipState == EVoipState.Available || VoipState == EVoipState.MicrophoneFail));
	}

	public bool method_122(in Vector3 voicePos, float sqrDistance)
	{
		return (Position - voicePos).sqrMagnitude <= sqrDistance;
	}

	public virtual void ExternalInteraction()
	{
		RaycastHit hit;
		GameObject interactive = GameWorld.FindInteractable(InteractionRay, out hit);
		if (_playerLookRaycastTransform == null || !HealthController.IsAlive || !CurrentState.CanInteract || !HandsController || !HandsController.CanInteract())
		{
			return;
		}
		if (BTRControllerClass.Instance != null && BTRControllerClass.Instance.BtrView != null && BTRControllerClass.Instance.BtrView.IsMyPlayerInRange)
		{
			if (_btrState != EPlayerBtrState.Outside)
			{
				if (_lastBtrStateInteractionCheck != _btrState)
				{
					this.PossibleInteractionsChanged?.Invoke();
					_lastBtrStateInteractionCheck = _btrState;
				}
				if (_lastBtrStateCheck != BtrInteractionSide.BtrView.BtrState)
				{
					this.PossibleInteractionsChanged?.Invoke();
					_lastBtrStateCheck = BtrInteractionSide.BtrView.BtrState;
				}
				return;
			}
			method_123(interactive);
		}
		method_124(interactive);
		method_125(interactive);
	}

	public void method_123(GameObject interactive)
	{
		if (interactive == null)
		{
			method_158();
			return;
		}
		BTRSide componentInParent = interactive.GetComponentInParent<BTRSide>();
		if (!(componentInParent == null) && componentInParent.DistanceCheck(Position))
		{
			if (!_lastBtrCastResult)
			{
				BtrInteractionSide = componentInParent;
				this.PossibleInteractionsChanged?.Invoke();
			}
			_lastBtrCastResult = true;
		}
		else
		{
			method_158();
		}
	}

	public void method_124(GameObject interactive)
	{
		if (interactive == null)
		{
			method_159();
			return;
		}
		TripwireInteractionTrigger component = interactive.GetComponent<TripwireInteractionTrigger>();
		if (!(component == null) && component.DistanceCheck(Position))
		{
			if (!_lastTripwireCastResult)
			{
				TripwireInteractionTrigger = component;
				this.PossibleInteractionsChanged?.Invoke();
			}
			_lastTripwireCastResult = true;
		}
		else
		{
			method_159();
		}
	}

	public void method_125(GameObject interactive)
	{
		if (interactive == null)
		{
			method_160();
			return;
		}
		EventObjectInteractive component = interactive.GetComponent<EventObjectInteractive>();
		if (!(component == null) && component.DistanceCheck(Position))
		{
			if (!_lastEventObjectCastResult)
			{
				EventObjectInteractive = component;
				this.PossibleInteractionsChanged?.Invoke();
			}
			_lastEventObjectCastResult = true;
		}
		else
		{
			method_160();
		}
	}

	public void method_126()
	{
		this.HandsChangingEvent?.Invoke();
	}

	public void method_127(IHandsController controller)
	{
		this.HandsChangedEvent?.Invoke(controller);
	}

	public virtual Task SpawnController(AbstractHandsController controller, Action callback = null)
	{
		TaskCompletionSource onControllerAppeared = new TaskCompletionSource();
		HandsController = controller;
		controller.Spawn(1f, delegate
		{
			method_127(controller);
			callback?.Invoke();
			onControllerAppeared.SetResult(result: true);
		});
		return onControllerAppeared.Task;
	}

	public void FastForwardCurrentOperations()
	{
		if (!(HandsController == null))
		{
			AbstractHandsController handsController;
			do
			{
				handsController = HandsController;
				HandsController.FastForwardCurrentState();
			}
			while (!(handsController == HandsController));
		}
	}

	public void DestroyController()
	{
		Item item = HandsController.Item;
		FastForwardCurrentOperations();
		HandsController.Destroy();
		GEventArgs10[] array = InventoryController.SelectEvents<GEventArgs10>(item).ToArray();
		foreach (GEventArgs10 activeEvent in array)
		{
			InventoryController.RemoveActiveEvent(activeEvent);
		}
		UnityEngine.Object.Destroy(HandsController);
		HandsController = null;
	}

	public virtual void DropCurrentController(Action callback, bool fastDrop, Item nextControllerItem = null)
	{
		HandsController.Drop(1f, callback, fastDrop, nextControllerItem);
	}

	public void TrySaveLastItemInHands()
	{
		Item item = TryGetItemInHands<Item>();
		if (item != null)
		{
			LastEquippedWeaponOrKnifeItem = item;
		}
	}

	public void SetEmptyHands(Callback<GInterface198> callback)
	{
		callback = (Callback<GInterface198>)Delegate.Combine(callback, (Callback<GInterface198>)delegate
		{
			method_128(enable: false);
		});
		Proceed(withNetwork: true, callback);
	}

	public void HideWeapon()
	{
		TrySaveLastItemInHands();
		SetEmptyHands(delegate
		{
		});
		IsInBufferZone = true;
	}

	public void RevealWeapon()
	{
		IsInBufferZone = false;
		TrySetLastEquippedWeapon();
	}

	public void SetStationaryWeapon(Weapon weapon)
	{
		Proceed(weapon, delegate
		{
		}, scheduled: false);
	}

	public void SetInHands(Weapon weapon, Callback<IFirearmHandsController> callback)
	{
		Proceed(weapon, callback);
	}

	public void SetInHands(ThrowWeapItemClass throwWeap, Callback<IHandsThrowController> callback)
	{
		Proceed(throwWeap, callback);
	}

	public void SetInHands(MedsItemClass meds, GStruct382<EBodyPart> bodyParts, int animationVariant, Callback<GInterface203> callback)
	{
		method_128(enable: true);
		Proceed(meds, bodyParts, callback, animationVariant);
	}

	public void SetInHands(MedsItemClass meds, EBodyPart bodyPart, int animationVariant, Callback<GInterface203> callback)
	{
		method_128(enable: true);
		Proceed(meds, new GStruct382<EBodyPart>(bodyPart), callback, animationVariant);
	}

	public void SetInHands(FoodDrinkItemClass foodDrink, float amount, int animationVariant, Callback<GInterface203> callback)
	{
		method_128(enable: true);
		Proceed(foodDrink, amount, callback, animationVariant);
	}

	public void SetInHands(KnifeComponent knife, Callback<IKnifeController> callback)
	{
		Proceed(knife, callback);
	}

	public void SetInHandsUsableItem(Item item, Callback<GInterface202> callback)
	{
		if (item is PortableRangeFinderItemClass)
		{
			Proceed<PortableRangeFinderController>(item, callback);
		}
		if (item is RadioTransmitterItemClass)
		{
			Proceed<RadioTransmitterController>(item, callback);
		}
	}

	public void SetInHandsForQuickUse(Item quickUseItem, Callback<IOnHandsUseCallback> callback)
	{
		Proceed(quickUseItem, callback);
	}

	public void SetInHandsForQuickUse(ThrowWeapItemClass throwWeap, Callback<GInterface206> callback)
	{
		Proceed(throwWeap, callback);
	}

	public void SetInHandsForQuickUse(KnifeComponent knife, Callback<GInterface207> callback)
	{
		Proceed(knife, callback);
	}

	public void method_128(bool enable)
	{
		MovementContext.PlayerAnimator.SetItemUse(enable);
	}

	public void TrySetLastEquippedWeapon(bool equipFirstAvaliableOnFail = true, Callback callback = null)
	{
		if (method_129(LastEquippedWeaponOrKnifeItem))
		{
			TryProceed(LastEquippedWeaponOrKnifeItem, delegate(Result<IHandsController> result)
			{
				callback?.Invoke(result);
			});
		}
		else if (equipFirstAvaliableOnFail)
		{
			SetFirstAvailableItem(delegate(Result<IHandsController> result)
			{
				callback?.Invoke(result);
			});
		}
	}

	public void SetFirstAvailableItem(Callback<IHandsController> completeCallback)
	{
		Item item = _slotPriority.Select((EquipmentSlot x) => InventoryController.Inventory.Equipment.GetSlot(x).ContainedItem).FirstOrDefault((Item x) => method_129(x));
		if (item != null)
		{
			SetInHands(item, completeCallback);
			return;
		}
		SetEmptyHands(delegate(Result<GInterface198> result)
		{
			completeCallback(result.Complete ? new Result<IHandsController>(result.Value) : new Result<IHandsController>(null, result.Error));
		});
	}

	public bool method_129(Item itemToCheck)
	{
		if (itemToCheck != null && InventoryController.IsAtReachablePlace(itemToCheck))
		{
			if (itemToCheck.CurrentAddress != null)
			{
				return itemToCheck.CheckAction(null).Succeeded;
			}
			return true;
		}
		return false;
	}

	public void SetInHands(Item item, Callback<IHandsController> callback)
	{
		if (item.CurrentAddress != null && !item.CheckAction(null).Succeeded)
		{
			SetEmptyHands(delegate(Result<GInterface198> result)
			{
				callback((!string.IsNullOrEmpty(result.Error)) ? new Result<IHandsController>(null, result.Error) : new Result<IHandsController>(result.Value));
			});
		}
		else
		{
			TryProceed(item, callback);
		}
	}

	public void SetQuickSlotItem(EBoundItem quickSlot, Callback<IHandsController> callback)
	{
		Item boundItem = InventoryController.Inventory.FastAccess.GetBoundItem(quickSlot);
		if (boundItem != null && (!HealthController.IsItemForHealing(boundItem) || HealthController.CanApplyItem(boundItem, EBodyPart.Common)))
		{
			if (boundItem.CheckAction(null).Succeeded && !InventoryController.IsChangingWeapon && (!IsInBufferZone || CanManipulateWithHandsInBufferZone || (IsInBufferZone && HealthController.IsItemForHealing(boundItem))))
			{
				TryProceed(boundItem, callback);
			}
			else
			{
				callback(null);
			}
		}
		else
		{
			callback(null);
		}
	}

	public void SetSlotItem(EquipmentSlot equipmentSlot, Callback<IHandsController> callback)
	{
		Item containedItem = InventoryController.Inventory.Equipment.GetSlot(equipmentSlot).ContainedItem;
		if (containedItem != null)
		{
			SetItemInHands(containedItem, callback);
		}
	}

	public void DropBackpack()
	{
		Item containedItem = InventoryController.Inventory.Equipment.GetSlot(EquipmentSlot.Backpack).ContainedItem;
		if (containedItem != null)
		{
			if (MovementContext.IsInMountedState)
			{
				MovementContext.ExitMountedState();
			}
			ItemHandsController itemHandsController = HandsController as ItemHandsController;
			if (itemHandsController != null && itemHandsController.CurrentCompassState)
			{
				itemHandsController.SetCompassState(active: false);
			}
			else if (!(MovementContext.StationaryWeapon != null) && InventoryController.CanThrow(containedItem) && !HandsController.IsPlacingBeacon() && !HandsController.IsInInteractionStrictCheck() && CurrentStateName != EPlayerState.BreachDoor && !IsSprintEnabled && MovementContext.PlayerAnimator.AnimatedInteractions.CanInteract)
			{
				InventoryController.TryThrowItem(containedItem);
			}
		}
	}

	public void SetItemInHands(Item item, Callback<IHandsController> callback)
	{
		if (item != null && item.CheckAction(null).Succeeded && !InventoryController.IsChangingWeapon && (!IsInBufferZone || CanManipulateWithHandsInBufferZone))
		{
			TryProceed(item, callback);
			return;
		}
		UnityEngine.Debug.LogError("error null take to hands");
		callback(null);
	}

	public bool method_130(EquipmentSlot slot)
	{
		return InventoryController.Inventory.Equipment.GetSlot(slot).ContainedItem != null;
	}

	public bool method_131(EBoundItem slot)
	{
		return InventoryController.Inventory.FastAccess.GetBoundItem(slot) != null;
	}

	public virtual void Interact(IItemOwner loot, Callback callback)
	{
		callback.Succeed();
	}

	public virtual void Proceed(bool withNetwork, Callback<GInterface198> callback, bool scheduled = true)
	{
		Func<EmptyHandsController> controllerFactory = () => EmptyHandsController.smethod_6<EmptyHandsController>(this);
		new Process<EmptyHandsController, GInterface198>(this, controllerFactory, null).method_0(null, callback, scheduled);
	}

	public virtual void Proceed(Weapon weapon, Callback<IFirearmHandsController> callback, bool scheduled = true)
	{
		Func<FirearmController> controllerFactory = ((!IsAI) ? ((Func<FirearmController>)(() => FirearmController.smethod_6<FirearmController>(this, weapon))) : ((Func<AIFirearmController>)(() => FirearmController.smethod_6<AIFirearmController>(this, weapon))));
		bool fastHide = false;
		if (_handsController is FirearmController firearmController)
		{
			fastHide = firearmController.CheckForFastWeaponSwitch(weapon);
		}
		new Process<FirearmController, IFirearmHandsController>(this, controllerFactory, weapon, fastHide).method_0(null, callback, scheduled);
	}

	public virtual void Proceed(ThrowWeapItemClass throwWeap, Callback<IHandsThrowController> callback, bool scheduled = true)
	{
		Func<GrenadeHandsController> controllerFactory = () => GrenadeHandsController.smethod_9<GrenadeHandsController>(this, throwWeap);
		new Process<GrenadeHandsController, IHandsThrowController>(this, controllerFactory, throwWeap).method_0(null, callback, scheduled);
	}

	public virtual void Proceed(MedsItemClass meds, GStruct382<EBodyPart> bodyParts, Callback<GInterface203> callback, int animationVariant, bool scheduled = true)
	{
		Func<MedsController> controllerFactory = () => MedsController.smethod_6<MedsController>(this, meds, bodyParts, 1f, animationVariant);
		new Process<MedsController, GInterface203>(this, controllerFactory, meds).method_0(null, callback, scheduled);
	}

	public virtual void Proceed(FoodDrinkItemClass foodDrink, float amount, Callback<GInterface203> callback, int animationVariant, bool scheduled = true)
	{
		GStruct382<EBodyPart> bodyParts = default(GStruct382<EBodyPart>);
		bodyParts.Add(EBodyPart.Head);
		Func<MedsController> controllerFactory = () => MedsController.smethod_6<MedsController>(this, foodDrink, bodyParts, amount, animationVariant);
		new Process<MedsController, GInterface203>(this, controllerFactory, foodDrink).method_0(null, callback, scheduled);
	}

	public virtual void Proceed(KnifeComponent knife, Callback<IKnifeController> callback, bool scheduled = true)
	{
		Func<KnifeController> controllerFactory = () => KnifeController.smethod_9<KnifeController>(this, knife);
		new Process<KnifeController, IKnifeController>(this, controllerFactory, knife.Item, fastHide: true).method_0(null, callback, scheduled);
	}

	public virtual void Proceed<T>(Item item, Callback<GInterface202> callback, bool scheduled = true) where T : UsableItemController
	{
		Func<T> controllerFactory = () => UsableItemController.smethod_6<T>(this, item);
		new Process<T, GInterface202>(this, controllerFactory, item, fastHide: true).method_0(null, callback, scheduled);
	}

	public virtual void Proceed(Item item, Callback<IOnHandsUseCallback> callback, bool scheduled = true)
	{
		Func<QuickUseItemController> controllerFactory = () => QuickUseItemController.smethod_6<QuickUseItemController>(this, item);
		new Process<QuickUseItemController, IOnHandsUseCallback>(this, controllerFactory, item, fastHide: true, AbstractProcess.Completion.Sync, AbstractProcess.Confirmation.Succeed, skippable: false).method_0(null, callback, scheduled);
	}

	public virtual void Proceed(ThrowWeapItemClass throwWeap, Callback<GInterface206> callback, bool scheduled = true)
	{
		Func<QuickGrenadeThrowHandsController> controllerFactory = () => QuickGrenadeThrowHandsController.smethod_9<QuickGrenadeThrowHandsController>(this, throwWeap);
		new Process<QuickGrenadeThrowHandsController, GInterface206>(this, controllerFactory, throwWeap, fastHide: true, AbstractProcess.Completion.Sync, AbstractProcess.Confirmation.Succeed, skippable: false).method_0(null, callback, scheduled);
	}

	public virtual void Proceed(KnifeComponent knife, Callback<GInterface207> callback, bool scheduled = true)
	{
		Func<QuickKnifeKickController> controllerFactory = () => QuickKnifeKickController.smethod_9<QuickKnifeKickController>(this, knife);
		new Process<QuickKnifeKickController, GInterface207>(this, controllerFactory, knife.Item, fastHide: true, AbstractProcess.Completion.Sync, AbstractProcess.Confirmation.Succeed, skippable: false).method_0(null, callback, scheduled);
	}

	public void TryProceed(Item item, Callback<IHandsController> completeCallback, bool scheduled = true)
	{
		Class1344 @class = new Class1344();
		@class.completeCallback = completeCallback;
		@class.player_0 = this;
		StopBlindFire();
		if (item.LeftHandItem)
		{
			method_132(item);
		}
		else
		{
			if (_leftHandController.InAction)
			{
				return;
			}
			RemoveLeftHandItem(2f);
			method_126();
			if (item == null)
			{
				@class.method_0();
				return;
			}
			if (!InventoryController.IsAtReachablePlace(item))
			{
				SetFirstAvailableItem(@class.completeCallback);
				return;
			}
			if (HandsController is MedsController medsController && !IsAI)
			{
				medsController.SetOnUsedCallback(delegate
				{
				});
			}
			Class1344 class1344_0 = @class;
			Weapon weapon = item as Weapon;
			if (weapon == null)
			{
				if (!(item is ThrowWeapItemClass throwWeap))
				{
					if (!(item is MedsItemClass meds))
					{
						if (!(item is FoodDrinkItemClass foodDrink))
						{
							KnifeComponent itemComponent = item.GetItemComponent<KnifeComponent>();
							if (itemComponent != null)
							{
								Proceed(itemComponent, delegate(Result<IKnifeController> result)
								{
									smethod_1(result, class1344_0.completeCallback);
								}, scheduled);
							}
							else if (item is PortableRangeFinderItemClass)
							{
								method_134(item as PortableRangeFinderItemClass, class1344_0.completeCallback, scheduled);
							}
							else if (item is RadioTransmitterItemClass)
							{
								method_135(item as RadioTransmitterItemClass, class1344_0.completeCallback, scheduled);
							}
							else if (item.UsePrefab != null)
							{
								Proceed(item, delegate(Result<IOnHandsUseCallback> result)
								{
									smethod_1(result, class1344_0.completeCallback);
								}, scheduled);
							}
							else
							{
								class1344_0.method_0();
							}
						}
						else
						{
							Proceed(foodDrink, 1f, delegate(Result<GInterface203> result)
							{
								smethod_1(result, class1344_0.completeCallback);
							}, GClass3380.GetRandomAnimationVariant(item), scheduled);
						}
					}
					else
					{
						Proceed(meds, method_133(item), delegate(Result<GInterface203> result)
						{
							smethod_1(result, class1344_0.completeCallback);
						}, GClass3380.GetRandomAnimationVariant(item), scheduled);
					}
				}
				else
				{
					Proceed(throwWeap, delegate(Result<IHandsThrowController> result)
					{
						smethod_1(result, class1344_0.completeCallback);
					}, scheduled);
				}
				return;
			}
			Proceed(weapon, delegate(Result<IFirearmHandsController> result)
			{
				smethod_1(result, class1344_0.completeCallback);
				if (result.Complete && !weapon.IsOneOff)
				{
					class1344_0.player_0.LastEquippedWeaponOrKnifeItem = weapon;
				}
			}, scheduled);
		}
	}

	public void method_132(Item item)
	{
		if (!MovementContext.LeftStanceEnabled && !MovementContext.IsSprintEnabled && !MovementContext.PlayerAnimatorGetIsVaulting() && !MovementContext.IsStationaryWeaponInHands && !MovementContext.IsInMountedState && !HandsController.IsAiming && (!(HandsController is FirearmController firearmController) || (!firearmController.IsInReloadOperation() && !firearmController.IsInRemoveOperation() && !(firearmController.CurrentHandsOperation is FirearmController.GClass2038))))
		{
			ToggleLeftHand(item);
		}
	}

	public GStruct382<EBodyPart> method_133(Item item)
	{
		if ((GClass1085.EContinuousHealMode)Singleton<SharedGameSettingsClass>.Instance.Game.Settings.ContinuousHealMode != GClass1085.EContinuousHealMode.Disabled)
		{
			return HealthController.BodyPartsPriority(item, continuousHealEnabled: true);
		}
		return new GStruct382<EBodyPart>(EBodyPart.Common);
	}

	public void method_134(PortableRangeFinderItemClass item, Callback<IHandsController> completeCallback, bool scheduled = true)
	{
		if (this is ClientPlayer)
		{
			Proceed<ClientPortableRangeFinderController>(item, delegate(Result<GInterface202> result)
			{
				smethod_1(result, completeCallback);
			}, scheduled);
		}
		else
		{
			Proceed<PortableRangeFinderController>(item, delegate(Result<GInterface202> result)
			{
				smethod_1(result, completeCallback);
			}, scheduled);
		}
	}

	public void method_135(RadioTransmitterItemClass item, Callback<IHandsController> completeCallback, bool scheduled = true)
	{
		if (this is ClientPlayer)
		{
			Proceed<ClientRadioTransmitterController>(item, delegate(Result<GInterface202> result)
			{
				smethod_1(result, completeCallback);
			}, scheduled);
		}
		else
		{
			Proceed<RadioTransmitterController>(item, delegate(Result<GInterface202> result)
			{
				smethod_1(result, completeCallback);
			}, scheduled);
		}
	}

	public static void smethod_1<T>(Result<T> result, Callback<IHandsController> callback) where T : IHandsController
	{
		callback?.Invoke(new Result<IHandsController>(result.Value, result.Error));
	}

	public void method_136(Item item)
	{
		if (item == null)
		{
			return;
		}
		EquipmentSlot[] array = new EquipmentSlot[4]
		{
			EquipmentSlot.FirstPrimaryWeapon,
			EquipmentSlot.SecondPrimaryWeapon,
			EquipmentSlot.Holster,
			EquipmentSlot.Scabbard
		};
		InventoryEquipment equipment = InventoryController.Inventory.Equipment;
		int num = 0;
		EquipmentSlot slotName;
		while (true)
		{
			if (num < array.Length)
			{
				slotName = array[num];
				if (item == equipment.GetSlot(slotName).ContainedItem)
				{
					break;
				}
				num++;
				continue;
			}
			return;
		}
		ActiveSlot = equipment.GetSlot(slotName);
	}

	public bool CanPerformAnimatedOperation(Item item, BaseInventoryOperationClass operation)
	{
		if (!HealthController.IsAlive)
		{
			return true;
		}
		if (HandsController.CanExecute(operation))
		{
			return true;
		}
		if (HandsController.Item != item)
		{
			return true;
		}
		if (HandsController is BaseGrenadeHandsController && !HandsController.CanRemove())
		{
			return false;
		}
		return true;
	}

	public void TryRemoveFromHands(Item item, GInterface438 abstractOperation, Callback callback)
	{
		if (HandsController == null)
		{
			UnityEngine.Debug.LogFormat("Attempt to remove item '{0}' from hands, while HandsController == null", item);
			callback.Succeed();
		}
		else if (!HealthController.IsAlive)
		{
			callback.Succeed();
		}
		else if (HandsController.Item == item)
		{
			if (HandsController is BaseGrenadeHandsController && !HandsController.CanRemove())
			{
				callback.Fail("Cannot remove grenade while throwing it");
			}
			else
			{
				SetControllerInsteadRemovedOne(item, callback);
			}
		}
		else if (HandsController.CanExecute(abstractOperation))
		{
			_removeFromHandsCallback = callback;
			InventoryController.RaiseInOutProcessEvents(new GEventArgs17(HandsController.Item, CommandStatus.Begin, InventoryController));
			HandsController.Execute(abstractOperation, delegate(IResult result)
			{
				if ((object)_removeFromHandsCallback == callback)
				{
					_removeFromHandsCallback = null;
				}
				InventoryController.RaiseInOutProcessEvents(new GEventArgs17(HandsController.Item, CommandStatus.Succeed, InventoryController));
				callback(result);
			});
		}
		else
		{
			callback.Fail("hands controller can't perform this operation");
		}
	}

	public virtual void SetControllerInsteadRemovedOne(Item removingItem, Callback callback)
	{
		_removeFromHandsCallback = callback;
		if (removingItem is RocketLauncherItemClass)
		{
			Item item = _slotPriority.Select((EquipmentSlot x) => InventoryController.Inventory.Equipment.GetSlot(x).ContainedItem).FirstOrDefault((Item x) => method_129(x));
			if (item != null && item != removingItem && item is Weapon weapon)
			{
				Proceed(weapon, delegate(Result<IFirearmHandsController> result)
				{
					if ((object)_removeFromHandsCallback == callback)
					{
						_removeFromHandsCallback = null;
					}
					callback.Invoke(result);
				});
				return;
			}
		}
		TrySetLastEquippedWeapon(equipFirstAvaliableOnFail: true, delegate(IResult result)
		{
			if ((object)_removeFromHandsCallback == callback)
			{
				_removeFromHandsCallback = null;
			}
			callback(result);
		});
	}

	public void TrySetInHands(Item item, ItemAddress to, GInterface438 operation, Callback originalCallback)
	{
		if (HealthController.IsAlive && (!IsInBufferZone || CanManipulateWithHandsInBufferZone))
		{
			if (!(operation is FoldOperationClass) && ActiveSlot == to.Container)
			{
				_setInHandsCallback = originalCallback;
				TryProceed(item, delegate(Result<IHandsController> result)
				{
					if ((object)_setInHandsCallback == originalCallback)
					{
						_setInHandsCallback = null;
					}
					originalCallback.Invoke(result);
				}, scheduled: false);
			}
			else if ((item.Parent != to || operation is FoldOperationClass) && HandsController.CanExecute(operation))
			{
				_setInHandsCallback = originalCallback;
				InventoryController.RaiseInOutProcessEvents(new GEventArgs17(HandsController.Item, CommandStatus.Begin, InventoryController));
				HandsController.Execute(operation, delegate(IResult error)
				{
					if ((object)_setInHandsCallback == originalCallback)
					{
						_setInHandsCallback = null;
					}
					InventoryController.RaiseInOutProcessEvents(new GEventArgs17(HandsController.Item, CommandStatus.Succeed, InventoryController));
					originalCallback(error);
				});
			}
			else if (operation is FoldOperationClass && !HandsController.CanExecute(operation))
			{
				originalCallback.Fail("Can't perform operation");
			}
			else
			{
				originalCallback.Succeed();
			}
		}
		else
		{
			originalCallback.Succeed();
		}
	}

	public IEnumerator FakeCallbackCoroutine()
	{
		while (HealthController.IsAlive)
		{
			yield return null;
		}
		if (_removeFromHandsCallback != null)
		{
			_removeFromHandsCallback.Succeed();
			_removeFromHandsCallback = null;
		}
		if (_setInHandsCallback != null)
		{
			_setInHandsCallback.Succeed();
			_setInHandsCallback = null;
		}
	}

	public virtual void FaceshieldMarkOperation(FaceShieldComponent armor, bool hasServerOrigin)
	{
		if (Time.time > _lastFaceshieldOperationTime + Time.fixedDeltaTime)
		{
			_lastFaceshieldOperationTime = Time.time;
			_faceshieldNumOperations = 0;
		}
		if (_faceshieldNumOperations <= 3)
		{
			InventoryController.ExecuteFaceshieldMarkOperation(armor);
			_faceshieldNumOperations++;
		}
	}

	public Class1311 method_137(Item item)
	{
		if (item != null)
		{
			Class1311 @class = new Class1311(this, item);
			@class.Execute();
			return @class;
		}
		UnityEngine.Debug.LogWarning("<color=red>Invalid BeginSetInHands operation args</color>");
		return null;
	}

	public Class1312 method_138(Item item)
	{
		if (item != null)
		{
			Class1312 @class = new Class1312(this, item);
			@class.Execute();
			return @class;
		}
		UnityEngine.Debug.LogWarning("<color=red>Invalid BeginRemoveFromHands operation args</color>");
		return null;
	}

	public GStruct154<GClass2060> method_139(ThrowWeapItemClass grenade, bool lowThrow, bool simulate)
	{
		if (!GClass3380.IsChildOf(grenade, InventoryController.Inventory.Equipment))
		{
			return new GClass1522("Can't find container for item: " + grenade.Id);
		}
		GStruct154<GClass3408> gStruct = InteractionsHandlerClass.Discard(HandsController.Item, InventoryController, simulate);
		if (gStruct.Failed)
		{
			return gStruct.Error;
		}
		if (!simulate)
		{
			Physical.OnThrow(lowThrow);
		}
		return new GClass2060(gStruct.Value, grenade);
	}

	[CompilerGenerated]
	public IEnumerator method_140()
	{
		yield return new WaitForSeconds(0.8f);
		PlayTacticalSound();
		foreach (TacticalComboVisualController helmetLightController in _helmetLightControllers)
		{
			helmetLightController.UpdateBeams();
		}
		AIData?.TacticalModeChange(_helmetLightControllers.Any((TacticalComboVisualController x) => x.LightMod.IsActive));
		yield return new WaitForSeconds(0.6f);
		IsHeadLightsAnimationActive = false;
	}

	[CompilerGenerated]
	public void method_141(int i)
	{
		ProceduralWeaponAnimation.Pose = i;
	}

	[CompilerGenerated]
	public void method_142(bool enable)
	{
		if (_voipAudioSource != null)
		{
			_voipAudioSource.mute = !enable;
		}
	}

	[CompilerGenerated]
	public void method_143()
	{
		_interactionSource.Stop();
		_interactionSource.Release();
		_interactionSource = null;
	}

	[CompilerGenerated]
	public bool method_144()
	{
		return MovementContext.AutoVaultingSettingEnabled;
	}

	[CompilerGenerated]
	public bool method_145()
	{
		return Physical.CanVault;
	}

	[CompilerGenerated]
	public bool method_146()
	{
		return Physical.CanClimb;
	}

	[CompilerGenerated]
	public void method_147()
	{
		Skills.LowHPDuration.Complete();
	}

	[CompilerGenerated]
	public void method_148()
	{
		Skills.StimulatorNegativeBuff.Begin();
	}

	[CompilerGenerated]
	public void method_149()
	{
		Skills.StimulatorNegativeBuff.Complete();
	}

	[CompilerGenerated]
	public void method_150()
	{
		Skills.LowHPDuration.Complete();
	}

	[CompilerGenerated]
	public void method_151()
	{
		Skills.OnlineAction.Complete((float)StatisticsManager.CurrentSessionLength.TotalHours);
	}

	[CompilerGenerated]
	public void method_152(GClass3552 invokedEvent)
	{
		method_114(invokedEvent);
	}

	[CompilerGenerated]
	public void method_153()
	{
		Skills.Dehydration.Complete();
	}

	[CompilerGenerated]
	public void method_154()
	{
		Skills.Exhaustion.Complete();
	}

	[CompilerGenerated]
	public void method_155()
	{
		Skills.MagazineCheckAction.Complete();
	}

	[CompilerGenerated]
	public void method_156()
	{
		Skills.UniqueLoot.Complete();
	}

	[CompilerGenerated]
	public void method_157()
	{
		Skills.ProneAction.Complete(Pedometer.GetDistanceFromMark(EPlayerState.ProneMove));
	}

	[CompilerGenerated]
	public void method_158()
	{
		if (_lastBtrCastResult)
		{
			BtrInteractionSide = null;
			this.PossibleInteractionsChanged?.Invoke();
		}
		_lastBtrCastResult = false;
	}

	[CompilerGenerated]
	public void method_159()
	{
		if (_lastTripwireCastResult)
		{
			TripwireInteractionTrigger = null;
			this.PossibleInteractionsChanged?.Invoke();
		}
		_lastTripwireCastResult = false;
	}

	[CompilerGenerated]
	public void method_160()
	{
		if (_lastEventObjectCastResult)
		{
			EventObjectInteractive = null;
			this.PossibleInteractionsChanged?.Invoke();
		}
		_lastEventObjectCastResult = false;
	}

	[CompilerGenerated]
	public void method_161(Result<GInterface198> result)
	{
		method_128(enable: false);
	}

	[CompilerGenerated]
	public EmptyHandsController method_162()
	{
		return EmptyHandsController.smethod_6<EmptyHandsController>(this);
	}
}
