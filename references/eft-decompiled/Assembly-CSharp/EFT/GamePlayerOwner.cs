using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Comfort.Common;
using EFT.InputSystem;
using EFT.Interactive;
using EFT.InventoryLogic;
using EFT.UI;
using EFT.UI.Gestures;
using EFT.UI.Screens;
using JetBrains.Annotations;
using RootMotion.FinalIK;
using UnityEngine;

namespace EFT;

public abstract class GamePlayerOwner : PlayerOwner
{
	[Serializable]
	[CompilerGenerated]
	public class Class1712
	{
		public static readonly Class1712 class1712_0 = new Class1712();

		public static Func<UIInputNode, bool> func_0;

		public static Func<GClass3393, bool> func_1;

		public static Func<GClass3393, int> func_2;

		public static Func<TacticalComboVisualController, Transform> func_3;

		public static Func<Slot, Item> func_4;

		public static Func<LightComponent, FirearmLightStateStruct> func_5;

		public static Func<FirearmLightStateStruct, bool> func_6;

		public bool method_0(UIInputNode child)
		{
			if (child != null && child.gameObject.activeSelf)
			{
				return !(child is ConsoleScreen);
			}
			return false;
		}

		public bool method_1(GClass3393 g)
		{
			return g != null;
		}

		public int method_2(GClass3393 g)
		{
			return g.Grid.GridWidth * g.Grid.GridHeight;
		}

		public Transform method_3(TacticalComboVisualController x)
		{
			return x.transform;
		}

		public Item method_4(Slot x)
		{
			return x.ContainedItem;
		}

		public FirearmLightStateStruct method_5(LightComponent x)
		{
			return new FirearmLightStateStruct
			{
				Id = x.Item.Id,
				IsActive = x.IsActive,
				LightMode = x.SelectedMode
			};
		}

		public bool method_6(FirearmLightStateStruct x)
		{
			return x.IsActive;
		}
	}

	[CompilerGenerated]
	public class Class1713<T> where T : GamePlayerOwner
	{
		public T owner;

		public Player player;

		public void method_0(GEventArgs3 removeItemEventArgs)
		{
			owner.ClearInteractionState();
		}

		public void method_1()
		{
			player.OnPropVisibility -= owner.method_9;
			player.OnShowAmmoCountZeroingPanel -= owner.method_7;
			player.OnShowFireMode -= owner.method_6;
			player.OnShowAmmoDetails -= owner.method_8;
			player.PossibleInteractionsChanged -= owner.InteractionsChangedHandler;
			player.Speaker.OnPhraseTold -= owner.method_10;
			player.InventoryController.RemoveItemEvent -= delegate
			{
				owner.ClearInteractionState();
			};
		}
	}

	[CompilerGenerated]
	public class Class1714
	{
		public Item searchItem;

		public GClass3393 method_0(StashGridClass grid)
		{
			return grid.FindLocationForItem(searchItem);
		}
	}

	[CompilerGenerated]
	public class Class1715
	{
		public GamePlayerOwner gamePlayerOwner_0;

		public GClass2067 wishlistManager;

		public void method_0()
		{
			gamePlayerOwner_0.Player.InventoryController.AddItemEvent -= delegate(GEventArgs2 args)
			{
				if (args.Status == CommandStatus.Begin && args.Item.Owner == gamePlayerOwner_0.Player.InventoryController)
				{
					foreach (Item allItem in GClass3380.GetAllItems(args.Item))
					{
						gamePlayerOwner_0.hashSet_1.Add(allItem.Id);
					}
				}
				if (args.Status == CommandStatus.Failed && args.Item.Owner == gamePlayerOwner_0.Player.InventoryController)
				{
					foreach (Item allItem2 in GClass3380.GetAllItems(args.Item))
					{
						gamePlayerOwner_0.hashSet_1.Remove(allItem2.Id);
					}
				}
				if (args.Status != CommandStatus.Succeed)
				{
					return;
				}
				IEnumerable<Item> enumerable;
				if (!(args.Item is RandomLootContainerItemClass))
				{
					enumerable = GClass3380.GetAllItems(args.Item);
				}
				else
				{
					IEnumerable<Item> enumerable2 = new Item[1] { args.Item };
					enumerable = enumerable2;
				}
				foreach (Item item in enumerable)
				{
					if (GClass2067.IsNotificationsEnableInRaid() && gamePlayerOwner_0.Player.InventoryController.Examined(item) && !gamePlayerOwner_0.hashSet_1.Contains(item.Id) && wishlistManager.IsInWishlist(item.TemplateId, includeQol: true, out var group))
					{
						NotificationManagerClass.DisplayNotification(new GClass2561(item.TemplateId, group));
					}
					gamePlayerOwner_0.hashSet_1.Add(item.Id);
				}
			};
		}

		public void method_1(GEventArgs2 args)
		{
			if (args.Status == CommandStatus.Begin && args.Item.Owner == gamePlayerOwner_0.Player.InventoryController)
			{
				foreach (Item allItem in GClass3380.GetAllItems(args.Item))
				{
					gamePlayerOwner_0.hashSet_1.Add(allItem.Id);
				}
			}
			if (args.Status == CommandStatus.Failed && args.Item.Owner == gamePlayerOwner_0.Player.InventoryController)
			{
				foreach (Item allItem2 in GClass3380.GetAllItems(args.Item))
				{
					gamePlayerOwner_0.hashSet_1.Remove(allItem2.Id);
				}
			}
			if (args.Status != CommandStatus.Succeed)
			{
				return;
			}
			IEnumerable<Item> enumerable;
			if (!(args.Item is RandomLootContainerItemClass))
			{
				enumerable = GClass3380.GetAllItems(args.Item);
			}
			else
			{
				IEnumerable<Item> enumerable2 = new Item[1] { args.Item };
				enumerable = enumerable2;
			}
			foreach (Item item in enumerable)
			{
				if (GClass2067.IsNotificationsEnableInRaid() && gamePlayerOwner_0.Player.InventoryController.Examined(item) && !gamePlayerOwner_0.hashSet_1.Contains(item.Id) && wishlistManager.IsInWishlist(item.TemplateId, includeQol: true, out var group))
				{
					NotificationManagerClass.DisplayNotification(new GClass2561(item.TemplateId, group));
				}
				gamePlayerOwner_0.hashSet_1.Add(item.Id);
			}
		}
	}

	private static bool bool_0;

	[CompilerGenerated]
	private static bool bool_1;

	[CompilerGenerated]
	private static bool bool_2;

	private Coroutine coroutine_1;

	private bool bool_3 = true;

	protected LocationSettingsClass.Location _location;

	private BattleUIPanelExtraction battleUIPanelExtraction_0;

	protected ExtractionTimersPanel _timerPanel;

	private ConsoleScreen consoleScreen_0;

	private IFirearmHandsController ifirearmHandsController_0;

	private readonly CompositeDisposableClass compositeDisposableClass = new CompositeDisposableClass();

	private bool bool_4;

	protected InsuranceCompanyClass _insurance;

	private bool bool_5;

	[CompilerGenerated]
	private IBackEndSession iBackEndSession;

	protected GInterface472 BattleUIScreenController;

	protected GameDateTime _gameDateTime;

	private static Player player_1;

	private readonly HashSet<MongoID> hashSet_1 = new HashSet<MongoID>();

	protected GClass2073 _headLightsValidator;

	[CompilerGenerated]
	private Action action_2;

	public readonly global::BindableStateClass<ActionsReturnClass> AvailableInteractionState = new global::BindableStateClass<ActionsReturnClass>();

	public static bool IgnoreInputInNPCDialog
	{
		[CompilerGenerated]
		get
		{
			return bool_1;
		}
		[CompilerGenerated]
		set
		{
			bool_1 = value;
		}
	}

	public static bool IgnoreInputWithKeepResetLook
	{
		[CompilerGenerated]
		get
		{
			return bool_2;
		}
		[CompilerGenerated]
		set
		{
			bool_2 = value;
		}
	}

	public IBackEndSession Session
	{
		[CompilerGenerated]
		get
		{
			return iBackEndSession;
		}
		[CompilerGenerated]
		set
		{
			iBackEndSession = value;
		}
	}

	public static Player MyPlayer => player_1;

	public event Action OnLeave
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

	public static T smethod_2<T>(Player player, IInputTree inputTree, InsuranceCompanyClass insurance, IBackEndSession session, GameUI gameUI, GameDateTime gameDateTime, [CanBeNull] LocationSettingsClass.Location location) where T : GamePlayerOwner
	{
		T owner = PlayerOwner.smethod_0<T>(player, inputTree);
		owner._gameDateTime = gameDateTime;
		player_1 = player;
		player.HasGamePlayerOwner = true;
		player.POM.On();
		gameUI.UsingPanel.Init(player.HealthController, player.InventoryController);
		owner.battleUIPanelExtraction_0 = gameUI.BattleUiPanelExtraction;
		owner._timerPanel = gameUI.TimerPanel;
		owner._insurance = insurance;
		owner.Session = session;
		owner._location = location;
		owner._children.Add(gameUI.PostFXPreview);
		owner.consoleScreen_0 = MonoBehaviourSingleton<PreloaderUI>.Instance.Console;
		owner.CheckDuplicateChildren();
		owner.InitBattleUIScreen();
		player.OnPropVisibility += owner.method_9;
		player.OnShowAmmoCountZeroingPanel += owner.method_7;
		player.OnShowFireMode += owner.method_6;
		player.OnShowAmmoDetails += owner.method_8;
		player.PossibleInteractionsChanged += owner.InteractionsChangedHandler;
		player.Speaker.OnPhraseTold += owner.method_10;
		player.InventoryController.RemoveItemEvent += delegate
		{
			owner.ClearInteractionState();
		};
		owner.OnDestroyCompositeDisposable.AddDisposable(delegate
		{
			player.OnPropVisibility -= owner.method_9;
			player.OnShowAmmoCountZeroingPanel -= owner.method_7;
			player.OnShowFireMode -= owner.method_6;
			player.OnShowAmmoDetails -= owner.method_8;
			player.PossibleInteractionsChanged -= owner.InteractionsChangedHandler;
			player.Speaker.OnPhraseTold -= owner.method_10;
			player.InventoryController.RemoveItemEvent -= delegate
			{
				owner.ClearInteractionState();
			};
		});
		player.Grounder.solver.quality = Grounding.Quality.Fastest;
		if (Singleton<BetterAudio>.Instantiated)
		{
			Singleton<BetterAudio>.Instance.SetProtagonist(player);
		}
		owner._headLightsValidator = new GClass2073();
		owner.Init();
		owner.Player.BindSlotViewChangedAction(EquipmentSlot.Headwear, owner.method_14);
		owner.method_16();
		return owner;
	}

	public virtual void Init()
	{
	}

	public void OnLeaveHandler()
	{
		action_2?.Invoke();
	}

	public override void vmethod_0()
	{
		ShowBattleUIScreen();
		base.vmethod_0();
		if (!(this is HideoutPlayerOwner) && CameraClass.Exist)
		{
			CameraClass.Instance.ReflexController.OnOffReflexIfCan();
		}
	}

	public static void SetIgnoreInput(bool ignore)
	{
		bool_0 = ignore;
	}

	public static void SetIgnoreInputInNPCDialog(bool ignore)
	{
		IgnoreInputInNPCDialog = ignore;
	}

	public static void SetIgnoreInputWithKeepResetLook(bool ignore)
	{
		IgnoreInputWithKeepResetLook = ignore;
	}

	public static void AddIgnoreInputCommands(IEnumerable<ECommand> commands)
	{
		foreach (ECommand command in commands)
		{
			PlayerOwner.ignoredCommands.Add(command);
		}
	}

	public static void RemoveIgnoreInputCommands(IEnumerable<ECommand> commands)
	{
		foreach (ECommand command in commands)
		{
			PlayerOwner.ignoredCommands.Remove(command);
		}
	}

	public virtual void InitBattleUIScreen()
	{
	}

	public virtual void ShowBattleUIScreen()
	{
	}

	public override void CleanupOnDestroy()
	{
		player_1 = null;
		vmethod_1();
		base.CleanupOnDestroy();
	}

	public override void vmethod_1()
	{
		if (base.State != EState.Started)
		{
			return;
		}
		using (GClass4062.ReleaseBeginSampleWithToken("GamePlayerOwner:203.Stop", "Stop"))
		{
			foreach (UIInputNode item in from child in _children.OfType<UIInputNode>()
				where child != null && child.gameObject.activeSelf && !(child is ConsoleScreen)
				select child)
			{
				item.Close();
			}
			StaticManager.Instance.StartCoroutine(CloseBattleUi());
			SetIgnoreInput(ignore: false);
			SetIgnoreInputInNPCDialog(ignore: false);
			SetIgnoreInputWithKeepResetLook(ignore: false);
			base.vmethod_1();
		}
	}

	public abstract IEnumerator CloseBattleUi();

	public void CloseObjectivesPanel()
	{
		battleUIPanelExtraction_0.Close();
	}

	public void ShowObjectivesPanel(string text, float time)
	{
		battleUIPanelExtraction_0.Show(text, time);
	}

	public void LateUpdate()
	{
		base.Player.ExternalInteraction();
		base.Player.InteractionRaycast();
		method_12();
		method_5();
	}

	public void method_5()
	{
		if (_headLightsValidator != null)
		{
			Camera camera = CameraClass.Instance.Camera;
			if (!(camera == null))
			{
				Transform transform = camera.transform;
				_headLightsValidator.Update(transform.position, transform.eulerAngles);
			}
		}
	}

	public void method_6(Weapon.EFireMode fireMode)
	{
		BattleUIScreenController.ShowFireMode(fireMode);
	}

	public void method_7(string message)
	{
		BattleUIScreenController.ShowAmmoCountZeroingPanel(message);
	}

	public void method_8(int ammoCount, int maxAmmoCount, int mastering, string details, bool foldingMechanimWeapon)
	{
		BattleUIScreenController.ShowAmmoDetails(ammoCount, maxAmmoCount, mastering, details, foldingMechanimWeapon);
	}

	public void method_9(bool isVisible)
	{
		if (isVisible)
		{
			if (Singleton<BackendConfigSettingsClass>.Instance.AzimuthPanelShowsPlayerOrientation)
			{
				BattleUIScreenController.ShowAzimuthPanel(() => (int)Vector3.SignedAngle(Singleton<LevelSettings>.Instance.NorthVector, Quaternion.Euler(0f, base.Player.MovementContext.Yaw, 0f) * Vector3.forward, Vector3.up));
			}
			else if (base.Player.CompassValue != null)
			{
				BattleUIScreenController.ShowAzimuthPanel(base.Player.CompassValue);
			}
		}
		else
		{
			BattleUIScreenController?.HideAzimuthPanel();
		}
	}

	public void method_10(EPhraseTrigger trigger, TaggedClip match, TagBank bank, PhraseSpeakerClass baseSpeaker)
	{
		BattleUIScreenController.GesturesQuickPanel.TryNotifyPhrasePlayed(trigger, match.Clip.length).HandleExceptions();
	}

	public override void MumbleStatusChangedHandler(bool active)
	{
		base.MumbleStatusChangedHandler(active);
		if (active)
		{
			BattleUIScreenController.GesturesQuickPanel.ToggleBindPanel(base.Player);
		}
		else
		{
			BattleUIScreenController.GesturesQuickPanel.CloseBindPanel();
		}
	}

	public override void MakeBindActionHandler(int command, bool aggressive)
	{
		base.MakeBindActionHandler(command, aggressive);
		MakeBindAction(command, aggressive);
	}

	public override void QuickMumbleHandler()
	{
		base.QuickMumbleHandler();
		QuickMumbleStart();
	}

	public override void MumbleDropdownChangedHandler(bool active)
	{
		base.MumbleDropdownChangedHandler(active);
		if (active)
		{
			ToggleMumbleDropdown();
		}
		else
		{
			CloseMumbleDropdown();
		}
	}

	public void MakeBindAction(int index, bool aggressive)
	{
		GesturesMenu gesturesMenu = BattleUIScreenController.GesturesQuickPanel.GesturesMenu;
		if (gesturesMenu != null && gesturesMenu.Binds.TryGetValue((ECommand)index, out var value))
		{
			PlayPhraseOrGesture(value, aggressive);
		}
	}

	public void PlayPhraseOrGesture(int actionId, bool aggressive)
	{
		if (GClass3937.IsPlayerGesture(actionId))
		{
			base.Player.vmethod_7((EInteraction)actionId);
			return;
		}
		EPhraseTrigger phrase = (EPhraseTrigger)actionId;
		if (BattleUIScreenController.GesturesQuickPanel.IsPhraseAvailable(phrase))
		{
			base.Player.Say(phrase, demand: true, 0f, (ETagStatus)0, 100, aggressive);
		}
		else
		{
			Debug.Log("Unable to say phrase (" + phrase.ToString() + ")");
		}
	}

	public void QuickMumbleStart()
	{
		if (!BattleUIScreenController.GesturesQuickPanel.DropdownPanelActive && bool_3)
		{
			EPhraseTrigger ePhraseTrigger = BattleUIScreenController.GesturesQuickPanel.ActivateCommand();
			if (ePhraseTrigger != EPhraseTrigger.PhraseNone)
			{
				base.Player.Say(ePhraseTrigger, demand: true);
			}
		}
	}

	public void ToggleMumbleDropdown()
	{
		method_11(!BattleUIScreenController.GesturesQuickPanel.DropdownPanelActive);
	}

	public void CloseMumbleDropdown()
	{
		method_11(value: false);
	}

	public void method_11(bool value)
	{
		if (coroutine_1 != null)
		{
			StopCoroutine(coroutine_1);
			coroutine_1 = null;
		}
		if (value)
		{
			coroutine_1 = GClass855.WaitSeconds(this, 0.2f, delegate
			{
				bool_3 = false;
				BattleUIScreenController.GesturesQuickPanel.ShowDropdown();
			});
			return;
		}
		if (BattleUIScreenController.GesturesQuickPanel.DropdownPanelActive)
		{
			BattleUIScreenController.GesturesQuickPanel.CloseDropdown(delegate(EPhraseTrigger x)
			{
				base.Player.Say(x, demand: true);
			});
		}
		GClass855.WaitSeconds(this, 0.3f, delegate
		{
			bool_3 = true;
		});
	}

	public void method_12()
	{
		if (base.Player.InteractableObject is Door door && base.Player.HealthController.IsAlive)
		{
			bool flag;
			if ((flag = door.IsBreachAngle(base.Player.Position)) && !bool_5)
			{
				bool_5 = true;
				InteractionsChangedHandler();
			}
			else if (!flag && bool_5)
			{
				bool_5 = false;
				InteractionsChangedHandler();
			}
		}
	}

	public override void TranslateAxes(ref float[] axes)
	{
		if (bool_0)
		{
			return;
		}
		if (IgnoreInputWithKeepResetLook)
		{
			if (base.Player.IsResettingLook)
			{
				base.TranslateAxes(ref axes);
			}
		}
		else if (IgnoreInputInNPCDialog)
		{
			if (base.Player.IsLooking || base.Player.IsResettingLook || axes[4] != 0f || axes[5] != 0f)
			{
				axes[0] = 0f;
				axes[1] = 0f;
				base.TranslateAxes(ref axes);
			}
		}
		else
		{
			base.TranslateAxes(ref axes);
		}
	}

	public bool method_13(ECommand command)
	{
		if ((IgnoreInputInNPCDialog || IgnoreInputWithKeepResetLook) && command != ECommand.ResetLookDirection && command != ECommand.DisplayTimer && command != ECommand.DisplayTimerAndExits)
		{
			return false;
		}
		if (bool_0 && command != ECommand.ChangePointOfView && command != ECommand.ToggleTalk && command != ECommand.StopTalk)
		{
			return false;
		}
		return base.Player.HealthController.IsAlive;
	}

	public Weapon GetCurrentWeapon()
	{
		return ifirearmHandsController_0.Item;
	}

	public bool CanSwitchMagazine()
	{
		if (ifirearmHandsController_0 != null && !IgnoreInputInNPCDialog && !IgnoreInputWithKeepResetLook && !bool_0 && base.Player.HealthController.IsAlive)
		{
			return ifirearmHandsController_0.CanStartReload();
		}
		return false;
	}

	public void SwitchMagazine(Weapon weapon, Item item)
	{
		if (item == null || item.Owner != base.Player.InventoryController || !CanSwitchMagazine() || weapon != GetCurrentWeapon())
		{
			return;
		}
		InventoryController inventoryController = base.Player.InventoryController;
		if (item is MagazineItemClass magazineItemClass && (weapon.ReloadMode == Weapon.EReloadMode.ExternalMagazine || weapon.ReloadMode == Weapon.EReloadMode.ExternalMagazineWithInternalReloadSupport || (weapon.ReloadMode == Weapon.EReloadMode.InternalMagazine && weapon.GetCurrentMagazine() == null)))
		{
			MagazineItemClass currentMagazine = weapon.GetCurrentMagazine();
			Slot magazineSlot = weapon.GetMagazineSlot();
			if (InteractionsHandlerClass.CheckMoveIgnoringTargetItem(magazineItemClass, magazineSlot, inventoryController).Succeeded)
			{
				GClass3393 itemAddress = ((currentMagazine != null) ? method_21(currentMagazine) : null);
				ifirearmHandsController_0.ReloadMag(magazineItemClass, itemAddress, null);
			}
		}
		else
		{
			if (!(item is AmmoItemClass item2))
			{
				return;
			}
			if (weapon.IsUnderBarrelDeviceActive)
			{
				ifirearmHandsController_0.ReloadGrenadeLauncher(new AmmoPackReloadingClass(new List<AmmoItemClass> { item2 }), null);
			}
			else if (weapon.SupportsInternalReload)
			{
				if (weapon is RevolverItemClass)
				{
					ifirearmHandsController_0.ReloadCylinderMagazine(new AmmoPackReloadingClass(new List<AmmoItemClass> { item2 }), null, quickReload: false);
				}
				else
				{
					ifirearmHandsController_0.ReloadWithAmmo(new AmmoPackReloadingClass(new List<AmmoItemClass> { item2 }), null);
				}
			}
			else if (weapon.ReloadMode == Weapon.EReloadMode.OnlyBarrel)
			{
				Slot slot = (weapon.IsMultiBarrel ? weapon.FirstFreeChamberSlot : weapon.Chambers[0]);
				if (slot != null)
				{
					GClass3393 placeToPutContainedAmmoMagazine = ((!(slot.ContainedItem is AmmoItemClass { IsUsed: false } ammoItemClass)) ? null : method_21(ammoItemClass));
					ifirearmHandsController_0.ReloadBarrels(new AmmoPackReloadingClass(new List<AmmoItemClass> { item2 }), placeToPutContainedAmmoMagazine, null);
				}
			}
			else
			{
				Debug.LogError("This is strange: " + weapon.ReloadMode.ToString() + "\n" + item);
			}
		}
	}

	public bool CanUseMeds()
	{
		if (!IgnoreInputInNPCDialog && !IgnoreInputWithKeepResetLook && !bool_0)
		{
			return base.Player.HealthController.IsAlive;
		}
		return false;
	}

	public override ETranslateResult TranslateCommand(ECommand command)
	{
		if (!method_13(command))
		{
			return ETranslateResult.BlockAll;
		}
		base.Player.CurrentManagedState.Cancel();
		if (TranslateInventoryScreenInput(command))
		{
			return ETranslateResult.BlockAll;
		}
		if (TranslateExitScreenInput(command))
		{
			return ETranslateResult.BlockAll;
		}
		switch (command)
		{
		case ECommand.DisplayTimerAndExits:
			_timerPanel.ShowTimer(showExits: true, updateExits: true);
			break;
		case ECommand.DisplayTimer:
			_timerPanel.ShowTimer(showExits: false);
			BattleUIScreenController.ShowRaidTime();
			break;
		}
		base.TranslateCommand(command);
		return ETranslateResult.Ignore;
	}

	public virtual bool TranslateInventoryScreenInput(ECommand command)
	{
		if (!GClass2406.IsCommand(command, ECommand.ToggleInventory))
		{
			return false;
		}
		base.Player.SetInventoryOpened(opened: true);
		return true;
	}

	public virtual bool TranslateExitScreenInput(ECommand command)
	{
		return GClass2406.IsCommand(command, ECommand.Escape);
	}

	public abstract void CloseInventoryIfOpen();

	public virtual void ShowInventoryScreenLoot(CompoundItem loot, Action callback, bool isFakeContainer = false)
	{
	}

	public void method_14(GameObject model)
	{
		Transform[] tactical = (from x in GClass6.GetComponentsInChildrenActiveIgnoreFirstLevel<TacticalComboVisualController>(base.Player.PlayerBones.Head.Original)
			select x.transform).ToArray();
		Transform camTr = CameraClass.Instance.Camera?.transform;
		_headLightsValidator.ValidateAllLights(tactical, camTr);
	}

	public virtual void InteractionsChangedHandler()
	{
		GInterface177 interactableObject = base.Player.InteractableObject;
		object obj = interactableObject;
		if (obj == null)
		{
			interactableObject = base.Player.PlaceItemZone;
			obj = interactableObject;
			if (obj == null)
			{
				interactableObject = base.Player.BtrInteractionSide;
				obj = interactableObject;
				if (obj == null)
				{
					interactableObject = base.Player.TripwireInteractionTrigger;
					obj = interactableObject;
					if (obj == null)
					{
						interactableObject = base.Player.EventObjectInteractive;
						obj = interactableObject ?? base.Player.ExfiltrationPoint;
					}
				}
			}
		}
		ActionsReturnClass actionsReturnClass = GetActionsClass.GetAvailableActions(this, (GInterface177)obj);
		if (actionsReturnClass == null && TransitControllerAbstractClass.Exist<TransitInteractionControllerAbstractClass>(out var transitController))
		{
			actionsReturnClass = transitController.AvailableInteractionState;
		}
		actionsReturnClass?.InitSelected();
		AvailableInteractionState.Value = actionsReturnClass;
	}

	public void ClearInteractionState()
	{
		AvailableInteractionState.Value = null;
	}

	public void DisplayPreloaderUiNotification(string message)
	{
		NotificationManagerClass.DisplayMessageNotification(message);
	}

	public override void SetHandsController(IFirearmHandsController controller)
	{
		base.SetHandsController(controller);
		compositeDisposableClass.Dispose();
		ifirearmHandsController_0 = controller;
		compositeDisposableClass.BindState(ifirearmHandsController_0.CompassState, method_15);
		base.Player.SetInventoryOpened(CurrentScreenSingletonClass.Instance.CheckCurrentScreen(EEftScreenType.Inventory));
	}

	public void method_15(bool isActive)
	{
		if (!isActive && base.Player.PointOfView == EPointOfView.FirstPerson && BattleUIScreenController != null)
		{
			BattleUIScreenController.HideAzimuthPanel();
		}
	}

	public override void SetHandsController(IHandsThrowController grenadeController)
	{
		base.SetHandsController(grenadeController);
		base.Player.SetInventoryOpened(CurrentScreenSingletonClass.Instance.CheckCurrentScreen(EEftScreenType.Inventory));
	}

	public override void SetHandsController(GInterface198 controller)
	{
		base.SetHandsController(controller);
		ifirearmHandsController_0 = null;
		base.Player.SetInventoryOpened(CurrentScreenSingletonClass.Instance.CheckCurrentScreen(EEftScreenType.Inventory));
	}

	public override void ResetHands()
	{
		ifirearmHandsController_0 = null;
	}

	public void method_16()
	{
		foreach (Item allRealPlayerItem in base.Player.InventoryController.Inventory.AllRealPlayerItems)
		{
			hashSet_1.Add(allRealPlayerItem.Id);
		}
		GClass2067 wishlistManager = base.Player.Profile.WishlistManager;
		base.Player.InventoryController.AddItemEvent += delegate(GEventArgs2 args)
		{
			if (args.Status == CommandStatus.Begin && args.Item.Owner == base.Player.InventoryController)
			{
				foreach (Item allItem in GClass3380.GetAllItems(args.Item))
				{
					hashSet_1.Add(allItem.Id);
				}
			}
			if (args.Status == CommandStatus.Failed && args.Item.Owner == base.Player.InventoryController)
			{
				foreach (Item allItem2 in GClass3380.GetAllItems(args.Item))
				{
					hashSet_1.Remove(allItem2.Id);
				}
			}
			if (args.Status != CommandStatus.Succeed)
			{
				return;
			}
			IEnumerable<Item> enumerable;
			if (!(args.Item is RandomLootContainerItemClass))
			{
				enumerable = GClass3380.GetAllItems(args.Item);
			}
			else
			{
				IEnumerable<Item> enumerable2 = new Item[1] { args.Item };
				enumerable = enumerable2;
			}
			foreach (Item item in enumerable)
			{
				if (GClass2067.IsNotificationsEnableInRaid() && base.Player.InventoryController.Examined(item) && !hashSet_1.Contains(item.Id) && wishlistManager.IsInWishlist(item.TemplateId, includeQol: true, out var group))
				{
					NotificationManagerClass.DisplayNotification(new GClass2561(item.TemplateId, group));
				}
				hashSet_1.Add(item.Id);
			}
		};
		OnDestroyCompositeDisposable.AddDisposable(delegate
		{
			base.Player.InventoryController.AddItemEvent -= delegate(GEventArgs2 args)
			{
				if (args.Status == CommandStatus.Begin && args.Item.Owner == base.Player.InventoryController)
				{
					foreach (Item allItem3 in GClass3380.GetAllItems(args.Item))
					{
						hashSet_1.Add(allItem3.Id);
					}
				}
				if (args.Status == CommandStatus.Failed && args.Item.Owner == base.Player.InventoryController)
				{
					foreach (Item allItem4 in GClass3380.GetAllItems(args.Item))
					{
						hashSet_1.Remove(allItem4.Id);
					}
				}
				if (args.Status != CommandStatus.Succeed)
				{
					return;
				}
				IEnumerable<Item> enumerable;
				if (!(args.Item is RandomLootContainerItemClass))
				{
					enumerable = GClass3380.GetAllItems(args.Item);
				}
				else
				{
					IEnumerable<Item> enumerable2 = new Item[1] { args.Item };
					enumerable = enumerable2;
				}
				foreach (Item item2 in enumerable)
				{
					if (GClass2067.IsNotificationsEnableInRaid() && base.Player.InventoryController.Examined(item2) && !hashSet_1.Contains(item2.Id) && wishlistManager.IsInWishlist(item2.TemplateId, includeQol: true, out var group))
					{
						NotificationManagerClass.DisplayNotification(new GClass2561(item2.TemplateId, group));
					}
					hashSet_1.Add(item2.Id);
				}
			};
		});
	}

	public void ReleaseTactical()
	{
		if ((GClass1085.ETacticalInputMode)Singleton<SharedGameSettingsClass>.Instance.Game.Settings.TacticalInputMode != GClass1085.ETacticalInputMode.Hold || ifirearmHandsController_0 == null)
		{
			return;
		}
		FirearmLightStateStruct[] array = (from x in GClass3380.GetComponents<LightComponent>(ifirearmHandsController_0.Item.AllSlots.Select((Slot x) => x.ContainedItem))
			select new FirearmLightStateStruct
			{
				Id = x.Item.Id,
				IsActive = x.IsActive,
				LightMode = x.SelectedMode
			}).ToArray();
		if (!GClass856.IsNullOrEmpty(array) && array.Any((FirearmLightStateStruct x) => x.IsActive))
		{
			for (int num = 0; num < array.Length; num++)
			{
				array[num].IsActive = false;
			}
			if (ifirearmHandsController_0.SetLightsState(array, force: true, animated: false))
			{
				base.Player.PlayTacticalSound();
			}
		}
	}

	public void OnApplicationFocus(bool hasFocus)
	{
		if (!hasFocus)
		{
			ReleaseTactical();
		}
	}

	public void OnApplicationQuit()
	{
		ReleaseTactical();
	}

	public GamePlayerOwner()
	{
	}

	[CompilerGenerated]
	public int method_17()
	{
		return (int)Vector3.SignedAngle(Singleton<LevelSettings>.Instance.NorthVector, Quaternion.Euler(0f, base.Player.MovementContext.Yaw, 0f) * Vector3.forward, Vector3.up);
	}

	[CompilerGenerated]
	public void method_18()
	{
		bool_3 = false;
		BattleUIScreenController.GesturesQuickPanel.ShowDropdown();
	}

	[CompilerGenerated]
	public void method_19(EPhraseTrigger x)
	{
		base.Player.Say(x, demand: true);
	}

	[CompilerGenerated]
	public void method_20()
	{
		bool_3 = true;
	}

	[CompilerGenerated]
	public GClass3393 method_21(Item searchItem)
	{
		return (from grid in GClass3372.GetPrioritizedGridsForUnloadedObject(base.Player.InventoryController.Inventory.Equipment)
			select grid.FindLocationForItem(searchItem) into g
			where g != null
			orderby g.Grid.GridWidth * g.Grid.GridHeight
			select g).FirstOrDefault();
	}
}
