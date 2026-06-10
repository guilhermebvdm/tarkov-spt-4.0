using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Comfort.Common;
using EFT.HealthSystem;
using EFT.Hideout;
using EFT.InputSystem;
using EFT.InventoryLogic;
using EFT.UI;
using EFT.UI.Screens;
using JetBrains.Annotations;
using UI.Hideout;
using UnityEngine;

namespace EFT;

public class HideoutPlayerOwner : EftGamePlayerOwner
{
	[CompilerGenerated]
	public class Class1723
	{
		public HideoutPlayerOwner hideoutPlayerOwner_0;

		public Action exitAction;

		public void method_0()
		{
			hideoutPlayerOwner_0.Player.UpdateInteractionCast();
			exitAction();
		}
	}

	[CompilerGenerated]
	public class Class1724
	{
		public HideoutPlayerOwner hideoutPlayerOwner_0;

		public Action exitAction;

		public void method_0()
		{
			hideoutPlayerOwner_0.Player.UpdateInteractionCast();
			exitAction();
		}
	}

	private const int int_0 = 50;

	[CompilerGenerated]
	private Action<bool> action_3;

	[CompilerGenerated]
	private Action<HideoutArea> action_4;

	[CompilerGenerated]
	private Action<HideoutArea> action_5;

	[CompilerGenerated]
	private Action action_6;

	[CompilerGenerated]
	private Action action_7;

	private bool bool_6;

	private GInterface232 ginterface232_1;

	private GInterface231 ginterface231_1;

	private bool bool_7 = true;

	private readonly Vector3 vector3_0 = new Vector3(0f, -500f, 0f);

	private Vector3 vector3_1;

	private Quaternion quaternion_0;

	private Vector3 vector3_2;

	private Quaternion quaternion_1;

	private RuntimeAnimatorController runtimeAnimatorController_0;

	[CompilerGenerated]
	private bool bool_8 = true;

	[CompilerGenerated]
	private HideoutPlayer hideoutPlayer_0;

	[CompilerGenerated]
	private bool bool_9;

	[CompilerGenerated]
	private bool bool_10;

	[CompilerGenerated]
	private HideoutArea hideoutArea_0;

	[CompilerGenerated]
	private bool bool_11;

	[CompilerGenerated]
	private string string_0;

	public bool FirstPersonMode
	{
		[CompilerGenerated]
		get
		{
			return bool_8;
		}
		[CompilerGenerated]
		set
		{
			bool_8 = value;
		}
	}

	public HideoutPlayer HideoutPlayer
	{
		[CompilerGenerated]
		get
		{
			return hideoutPlayer_0;
		}
		[CompilerGenerated]
		set
		{
			hideoutPlayer_0 = value;
		}
	}

	public bool InShootingRange
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

	public bool AvailableForInteractions
	{
		get
		{
			if (!InShootingRange)
			{
				return !HideoutPlayer.IsUpdateHideoutPlayerInventoryInProgress;
			}
			return false;
		}
	}

	public bool FlashLightState
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

	public HideoutArea HideoutArea
	{
		[CompilerGenerated]
		get
		{
			return hideoutArea_0;
		}
		[CompilerGenerated]
		set
		{
			hideoutArea_0 = value;
		}
	}

	public override bool SetItemInHandsImmediately => false;

	public bool IsGuest
	{
		[CompilerGenerated]
		get
		{
			return bool_11;
		}
		[CompilerGenerated]
		set
		{
			bool_11 = value;
		}
	}

	public string HideoutOwnerAccountId
	{
		[CompilerGenerated]
		get
		{
			return string_0;
		}
		[CompilerGenerated]
		set
		{
			string_0 = value;
		}
	}

	public override GInterface231 PlayerInputTranslator
	{
		get
		{
			if (!InShootingRange)
			{
				return ginterface231_1;
			}
			return base.PlayerInputTranslator;
		}
		set
		{
			base.PlayerInputTranslator = value;
		}
	}

	public override GInterface232 HandsInputTranslator
	{
		get
		{
			if (!InShootingRange)
			{
				return ginterface232_1;
			}
			return base.HandsInputTranslator;
		}
		set
		{
			base.HandsInputTranslator = value;
		}
	}

	public event Action<bool> OnShootingRangeStatusChange
	{
		[CompilerGenerated]
		add
		{
			Action<bool> action = action_3;
			Action<bool> action2;
			do
			{
				action2 = action;
				Action<bool> value2 = (Action<bool>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_3, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<bool> action = action_3;
			Action<bool> action2;
			do
			{
				action2 = action;
				Action<bool> value2 = (Action<bool>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_3, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<HideoutArea> OnSelectArea
	{
		[CompilerGenerated]
		add
		{
			Action<HideoutArea> action = action_4;
			Action<HideoutArea> action2;
			do
			{
				action2 = action;
				Action<HideoutArea> value2 = (Action<HideoutArea>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_4, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<HideoutArea> action = action_4;
			Action<HideoutArea> action2;
			do
			{
				action2 = action;
				Action<HideoutArea> value2 = (Action<HideoutArea>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_4, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<HideoutArea> OnSpecialAreaActionSelection
	{
		[CompilerGenerated]
		add
		{
			Action<HideoutArea> action = action_5;
			Action<HideoutArea> action2;
			do
			{
				action2 = action;
				Action<HideoutArea> value2 = (Action<HideoutArea>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_5, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<HideoutArea> action = action_5;
			Action<HideoutArea> action2;
			do
			{
				action2 = action;
				Action<HideoutArea> value2 = (Action<HideoutArea>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_5, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action OnToggleInfoIcons
	{
		[CompilerGenerated]
		add
		{
			Action action = action_6;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_6, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action action = action_6;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_6, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action OnExitQte
	{
		[CompilerGenerated]
		add
		{
			Action action = action_7;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_7, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action action = action_7;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_7, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public void EnterShootingRange()
	{
		if (!HideoutPlayer.IsUpdateHideoutPlayerInventoryInProgress)
		{
			if (FlashLightState)
			{
				method_31();
			}
			method_30(status: true).HandleExceptions();
		}
	}

	public async Task ExitShootingRange()
	{
		while (HideoutPlayer.IsUpdateHideoutPlayerInventoryInProgress)
		{
			await Task.Yield();
		}
		await method_30(status: false);
		method_32();
	}

	public void SetPointOfView(bool firstPerson)
	{
		if (FirstPersonMode != firstPerson || bool_7)
		{
			if (firstPerson)
			{
				GClass3681 voice = Singleton<CustomizationSolverClass>.Instance.GetVoice(base.Player.Profile.Customization[EBodyModelPart.Voice]);
				base.Player.Speaker.ReplaceVoice(base.Player.Profile.Info.Side, voice.Name);
			}
			bool_7 = false;
			FirstPersonMode = firstPerson;
			HideoutPlayer.VisorVisibility = FirstPersonMode;
			base.Player.BodyAnimatorCommon.enabled = FirstPersonMode;
			if (FirstPersonMode)
			{
				BattleUIScreenController.ShowScreen(EScreenState.Queued);
			}
			else if (!BattleUIScreenController.Closed)
			{
				BattleUIScreenController.CloseScreen();
			}
			GamePlayerOwner.SetIgnoreInput(!FirstPersonMode);
			base.Player.PointOfView = ((!FirstPersonMode) ? EPointOfView.FreeCamera : EPointOfView.FirstPerson);
			method_32();
			_headLightsValidator.SetPause(!FirstPersonMode);
			if (vector3_1 == Vector3.zero)
			{
				vector3_2 = base.Player.Transform.position;
				quaternion_1 = base.Player.TargetHandsRotation;
			}
			if (FirstPersonMode)
			{
				base.Player.Teleport(vector3_1);
				base.Player.MovementContext.Rotation = new Vector2(quaternion_0.eulerAngles.y, Mathf.DeltaAngle(0f, quaternion_0.eulerAngles.x));
				base.Player.MovementContext.CachedRotation = new Vector2(quaternion_0.eulerAngles.y, Mathf.DeltaAngle(0f, quaternion_0.eulerAngles.x));
				HideoutPlayer.SetPatrol(patrol: true);
			}
			else
			{
				vector3_1 = base.Player.Transform.position;
				quaternion_0 = base.Player.TargetHandsRotation;
				base.Player.Teleport(vector3_0);
			}
			Singleton<HideoutClass>.Instance.IsClientBusy = false;
		}
	}

	public void DecidePatrolStatus(bool force = false)
	{
		if (force || !HideoutPlayer.IsUpdateHideoutPlayerInventoryInProgress)
		{
			Vector2 rotation = base.Player.MovementContext.Rotation;
			bool flag = rotation.x < -50f || rotation.x > 50f;
			if (force || HideoutPlayer.IsInPatrol != flag)
			{
				HideoutPlayer.SetPatrol(flag);
			}
		}
	}

	public void SelectArea(HideoutArea area)
	{
		action_4?.Invoke(area);
	}

	public void SelectSpecialAreaAction(HideoutArea area)
	{
		action_5?.Invoke(area);
	}

	public void ExitQte()
	{
		action_7?.Invoke();
	}

	public void HideoutAreaInteraction([CanBeNull] HideoutArea area, bool inSight)
	{
		if (area == HideoutArea || inSight)
		{
			HideoutArea = (inSight ? area : null);
			base.Player.SearchForInteractions();
		}
	}

	public override void vmethod_1()
	{
		if (base.State == EState.Started)
		{
			base.vmethod_1();
		}
	}

	public override void Init()
	{
		base.Init();
		HideoutPlayer = (HideoutPlayer)base.Player;
		HideoutPlayer.SetEnvironment(HideoutPlayer.ProfileId, EnvironmentType.Indoor);
		method_30(status: false, forced: true).HandleExceptions();
		base.Player.MovementContext.BlockFirearms = true;
	}

	public override void CreateInputTranslators()
	{
		ginterface232_1 = new Class1729();
		ginterface231_1 = new GClass2353();
		base.CreateInputTranslators();
	}

	public override void InitBattleUIScreen()
	{
		BattleUIScreenController = new EftBattleUIScreen.GClass3867(this);
	}

	public override void ShowBattleUIScreen()
	{
	}

	public override void InteractionsChangedHandler()
	{
		GInterface177 interactableObject = base.Player.InteractableObject;
		object obj = interactableObject;
		if (obj == null)
		{
			interactableObject = base.Player.PlaceItemZone;
			obj = interactableObject;
			if (obj == null)
			{
				interactableObject = base.Player.ExfiltrationPoint;
				obj = interactableObject ?? HideoutArea;
			}
		}
		ActionsReturnClass availableHideoutActions = GetActionsClass.GetAvailableHideoutActions(this, (GInterface177)obj);
		availableHideoutActions?.DefaultSelected();
		AvailableInteractionState.Value = availableHideoutActions;
	}

	public override ETranslateResult TranslateCommand(ECommand command)
	{
		if (command <= ECommand.DisplayTimerAndExits)
		{
			if (command != ECommand.ToggleShooting)
			{
				if (command == ECommand.ToggleProne || (uint)(command - 62) <= 1u)
				{
					goto IL_0039;
				}
			}
			else if (HideoutPlayer.IsInPatrol)
			{
				goto IL_0039;
			}
		}
		else if (command <= ECommand.DropBackpack)
		{
			if (command == ECommand.ToggleGoggles)
			{
				HideoutPlayer.ToggleNightVision();
				return ETranslateResult.Ignore;
			}
			if (command == ECommand.DropBackpack)
			{
				goto IL_0039;
			}
		}
		else
		{
			switch (command)
			{
			case ECommand.SwitchHeadLight:
				HideoutPlayer.SwitchHeadLights(togglesActive: false, changesState: true);
				return ETranslateResult.Ignore;
			case ECommand.ToggleHeadLight:
				HideoutPlayer.SwitchHeadLights(togglesActive: true, changesState: false);
				return ETranslateResult.Ignore;
			}
		}
		ETranslateResult eTranslateResult = base.TranslateCommand(command);
		if (eTranslateResult != ETranslateResult.Ignore)
		{
			return eTranslateResult;
		}
		switch (command)
		{
		default:
			return ETranslateResult.Ignore;
		case ECommand.ToggleInfo:
			if (!InShootingRange)
			{
				action_6?.Invoke();
			}
			return ETranslateResult.Block;
		case ECommand.EndAlternativeShooting:
			return ETranslateResult.Block;
		case ECommand.ToggleAlternativeShooting:
			if (!InShootingRange)
			{
				method_31();
			}
			return ETranslateResult.Block;
		}
		IL_0039:
		return ETranslateResult.Ignore;
	}

	public override bool TranslateInventoryScreenInput(ECommand command)
	{
		if (!GClass2406.IsCommand(command, ECommand.ToggleInventory))
		{
			return false;
		}
		if (InShootingRange)
		{
			return false;
		}
		ShowInventoryScreen(delegate
		{
			HideoutPlayer.SetInventoryOpened(opened: false);
		}, HideoutPlayer.HealthController, HideoutPlayer.OriginalInventory, HideoutPlayer.AbstractQuestControllerClass, HideoutPlayer.AbstractAchievementControllerClass, HideoutPlayer.AbstractPrestigeControllerClass, HideoutPlayer.OriginalInventory.Inventory.Stash, EInventoryTab.Unchanged);
		base.Player.SetInventoryOpened(opened: true);
		return true;
	}

	public override void ShowInventoryScreen(Action exitAction, IHealthController healthController, InventoryController controller, AbstractQuestControllerClass questController, AbstractAchievementControllerClass achievementsController, AbstractPrestigeControllerClass prestigeController, CompoundItem lootItem, EInventoryTab tab, bool showAsGridContent = false)
	{
		Singleton<HideoutClass>.Instance.ShowInventoryScreen(delegate
		{
			base.Player.UpdateInteractionCast();
			exitAction();
		});
	}

	public void ShowAreaItemsTransferScreen(EAreaType areaType, Action exitAction)
	{
		Singleton<HideoutClass>.Instance.ShowAreaItemsTransferScreen(areaType, delegate
		{
			base.Player.UpdateInteractionCast();
			exitAction();
		});
	}

	public override bool TranslateExitScreenInput(ECommand command)
	{
		if (!GClass2406.IsCommand(command, ECommand.Escape))
		{
			return false;
		}
		if (base.Player.PointOfView != EPointOfView.FirstPerson)
		{
			return true;
		}
		if (InShootingRange)
		{
			ExitShootingRange().HandleExceptions();
			return true;
		}
		if (!BattleUIScreenController.Closed)
		{
			BattleUIScreenController.CloseScreen();
			SelectArea(null);
			return true;
		}
		return base.TranslateExitScreenInput(command);
	}

	public override GInterface231 PlayerInputTranslatorFactory(Player player)
	{
		return new Class1726(player);
	}

	public override GInterface232 GrenadeInputTranslatorFactory(IHandsThrowController grenadeController)
	{
		return new Class1716(grenadeController);
	}

	public async Task method_30(bool status, bool forced = false)
	{
		if (InShootingRange != status || forced)
		{
			InShootingRange = status;
			if (!InShootingRange)
			{
				await HideoutPlayer.ReleaseShootingRangeInventory();
			}
			else
			{
				await HideoutPlayer.UpdateHideoutPlayerInventory();
			}
			DecidePatrolStatus(force: true);
			action_3?.Invoke(InShootingRange);
			base.Player.SearchForInteractions();
		}
	}

	public void method_31()
	{
		if (!(CameraClass.Instance.Flashlight == null))
		{
			FlashLightState = !FlashLightState;
			method_32();
		}
	}

	public void method_32()
	{
		if (!(CameraClass.Instance.Flashlight == null))
		{
			CameraClass.Instance.Flashlight.SetState(method_33());
		}
	}

	public bool method_33()
	{
		if (InShootingRange)
		{
			return false;
		}
		return FlashLightState;
	}

	public void method_34(bool hide)
	{
		HideoutPlayer.PlayerBones.HolsterPrimary.gameObject.SetActive(!hide);
		HideoutPlayer.PlayerBones.HolsterSecondary.gameObject.SetActive(!hide);
		Item containedItem = HideoutPlayer.InventoryController.Inventory.Equipment.GetSlot(EquipmentSlot.Backpack).ContainedItem;
		PlayerBody.EquipmentSlotClass slotViewByItem = HideoutPlayer.PlayerBody.GetSlotViewByItem(containedItem);
		if (slotViewByItem.ParentedModel.Value != null)
		{
			slotViewByItem.ParentedModel.Value.SetActive(!hide);
		}
	}

	public void PrepareWorkout(RuntimeAnimatorController animatorController, Vector3 playerPosition, Vector3 playerRotation, Transform lDumbbell, Transform rDumbbell, QteHandleData.PropsData leftDumbbellData, QteHandleData.PropsData rightDumbbellData)
	{
		HideoutPlayer.CustomAnimationsAreProcessing = true;
		method_34(hide: true);
		runtimeAnimatorController_0 = HideoutPlayer._animators[0].runtimeAnimatorController;
		HideoutPlayer._animators[0].runtimeAnimatorController = animatorController;
		HideoutPlayer._animators[0].enabled = true;
		HideoutPlayer.PlayerBody.UpdatePlayerRenders(EPointOfView.ThirdPerson, HideoutPlayer.Side);
		HideoutPlayer.PlayerBody.PointOfView.Value = EPointOfView.FreeCamera;
		base.Player.Teleport(playerPosition);
		base.Player.Transform.rotation = Quaternion.Euler(playerRotation);
		smethod_4(lDumbbell, base.Player.PlayerBones.LeftPalm, leftDumbbellData);
		smethod_4(rDumbbell, base.Player.PlayerBones.RightPalm, rightDumbbellData);
		HideoutPlayer.SetPatrol(patrol: true);
	}

	public void StopWorkout()
	{
		HideoutPlayer.CustomAnimationsAreProcessing = false;
		method_34(hide: false);
		base.Player.Teleport(vector3_1);
		HideoutPlayer._animators[0].runtimeAnimatorController = runtimeAnimatorController_0;
		HideoutPlayer._animators[0].Reset();
		IPlayerStateContainerBehaviour[] playerStates = (BackendConfigAbstractClass.Config.UseSpiritPlayer ? HideoutPlayer.Spirit.BodyAnimatorWrapper.GetBehaviours<IPlayerStateContainerBehaviour>() : HideoutPlayer._animators[0].GetBehaviours<IPlayerStateContainerBehaviour>());
		HideoutPlayer.MovementContext.InitMovementStates(playerStates);
		DecidePatrolStatus(force: true);
	}

	public void ResetBackupPosition()
	{
		vector3_1 = vector3_2;
		quaternion_0 = quaternion_1;
		base.Player.CameraContainer.transform.rotation = quaternion_0;
		base.Player.MovementContext.Rotation = new Vector2(quaternion_0.eulerAngles.y, Mathf.DeltaAngle(0f, quaternion_0.eulerAngles.x));
		base.Player.MovementContext.CachedRotation = new Vector2(quaternion_0.eulerAngles.y, Mathf.DeltaAngle(0f, quaternion_0.eulerAngles.x));
	}

	public (Vector3, Quaternion) GetBackupPositionAndRotation()
	{
		return (vector3_1, quaternion_0);
	}

	public void SetGuest(bool isGuest, string ownerAccountId)
	{
		IsGuest = isGuest;
		HideoutOwnerAccountId = ownerAccountId;
	}

	[CompilerGenerated]
	public void method_35()
	{
		HideoutPlayer.SetInventoryOpened(opened: false);
	}

	[CompilerGenerated]
	public static void smethod_4(Transform dumbbell, Transform parent, QteHandleData.PropsData transformData)
	{
		dumbbell.SetParent(parent);
		dumbbell.localPosition = transformData.Position;
		dumbbell.localRotation = Quaternion.Euler(transformData.Rotation);
	}
}
You are not using the latest version of the tool, please update.
Latest version is '10.1.0.8386' (yours is '10.0.1.8346')
