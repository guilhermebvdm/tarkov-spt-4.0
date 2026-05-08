using System;
using System.Collections;
using System.Runtime.CompilerServices;
using EFT.HealthSystem;
using EFT.InputSystem;
using EFT.InventoryLogic;
using EFT.UI;
using EFT.UI.Screens;
using JetBrains.Annotations;
using UnityEngine;

namespace EFT;

public class EftGamePlayerOwner : CommonPlayerOwner<ISession>
{
	[CompilerGenerated]
	public class Class1709
	{
		public EftGamePlayerOwner eftGamePlayerOwner_0;

		public Action callback;

		public void method_0()
		{
			eftGamePlayerOwner_0.Player.SetInventoryOpened(opened: false);
			callback();
		}
	}

	[CompilerGenerated]
	public class Class1710
	{
		public InventoryScreen.GClass3876 inventoryScreenController;

		public IHealthController healthController;

		public EftGamePlayerOwner eftGamePlayerOwner_0;

		public Action exitAction;

		public void method_0(EDamageType obj)
		{
			inventoryScreenController?.CloseScreen();
		}

		public void method_1()
		{
			healthController.DiedEvent -= delegate
			{
				inventoryScreenController?.CloseScreen();
			};
			eftGamePlayerOwner_0.Player.UpdateInteractionCast();
			exitAction();
		}
	}

	public static TPlayerOwner Create<TPlayerOwner>(Player player, IInputTree inputTree, InsuranceCompanyClass insurance, ISession session, GameUI gameUI, GameDateTime gameDateTime, [CanBeNull] LocationSettingsClass.Location location) where TPlayerOwner : EftGamePlayerOwner
	{
		TPlayerOwner val = GamePlayerOwner.smethod_2<TPlayerOwner>(player, inputTree, insurance, session, gameUI, gameDateTime, location);
		val.Session = session;
		return val;
	}

	public static EftGamePlayerOwner Create(Player player, IInputTree inputTree, InsuranceCompanyClass insurance, ISession session, GameUI gameUI, GameDateTime gameDateTime, [CanBeNull] LocationSettingsClass.Location location)
	{
		EftGamePlayerOwner eftGamePlayerOwner = GamePlayerOwner.smethod_2<EftGamePlayerOwner>(player, inputTree, insurance, session, gameUI, gameDateTime, location);
		eftGamePlayerOwner.Session = session;
		return eftGamePlayerOwner;
	}

	public override void Init()
	{
		base.Player.HealthController.DiedEvent += delegate
		{
			ReleaseTactical();
		};
		base.Player.HandsChangingEvent += base.ReleaseTactical;
		CurrentScreenSingletonClass.Instance.OnScreenChanged += delegate(EEftScreenType screenType)
		{
			if (screenType != EEftScreenType.BattleUI)
			{
				ReleaseTactical();
			}
		};
		OnDestroyCompositeDisposable.AddDisposable(delegate
		{
			base.Player.HealthController.DiedEvent -= delegate
			{
				ReleaseTactical();
			};
		});
		OnDestroyCompositeDisposable.AddDisposable(delegate
		{
			CurrentScreenSingletonClass.Instance.OnScreenChanged -= delegate(EEftScreenType screenType)
			{
				if (screenType != EEftScreenType.BattleUI)
				{
					ReleaseTactical();
				}
			};
		});
		OnDestroyCompositeDisposable.AddDisposable(delegate
		{
			base.Player.HandsChangingEvent -= base.ReleaseTactical;
		});
	}

	public override ETranslateResult TranslateCommand(ECommand command)
	{
		if (base.TranslateCommand(command) == ETranslateResult.BlockAll)
		{
			return ETranslateResult.BlockAll;
		}
		if (TranslateInventoryScreenInput(command))
		{
			return ETranslateResult.BlockAll;
		}
		if (TranslateExitScreenInput(command))
		{
			return ETranslateResult.BlockAll;
		}
		return ETranslateResult.Ignore;
	}

	public override bool TranslateInventoryScreenInput(ECommand command)
	{
		if (!CurrentScreenSingletonClass.Instance.CheckCurrentScreen(EEftScreenType.BattleUI))
		{
			return false;
		}
		if (!base.TranslateInventoryScreenInput(command))
		{
			return false;
		}
		ShowInventoryScreen(delegate
		{
			base.Player.SetInventoryOpened(opened: false);
		}, base.Player.HealthController, base.Player.InventoryController, base.Player.AbstractQuestControllerClass, base.Player.AbstractAchievementControllerClass, base.Player.AbstractPrestigeControllerClass, null, EInventoryTab.Unchanged);
		return true;
	}

	public override bool TranslateExitScreenInput(ECommand command)
	{
		if (!base.TranslateExitScreenInput(command))
		{
			return false;
		}
		MenuScreen.GClass3880 gClass = new MenuScreen.GClass3880();
		gClass.ShowScreen(EScreenState.Queued);
		gClass.OnLeave += base.OnLeaveHandler;
		return true;
	}

	public override void CloseInventoryIfOpen()
	{
		CurrentScreenSingletonClass.Instance.ToggleScreen(EEftScreenType.Inventory);
	}

	public override IEnumerator CloseBattleUi()
	{
		yield return null;
		yield return null;
		CurrentScreenSingletonClass.Instance.ToggleScreen(EEftScreenType.BattleUI);
	}

	public override void ShowInventoryScreenLoot(CompoundItem loot, Action callback, bool isFakeContainer = false)
	{
		base.Player.SetInventoryOpened(opened: true);
		ShowInventoryScreen(delegate
		{
			base.Player.SetInventoryOpened(opened: false);
			callback();
		}, base.Player.HealthController, base.Player.InventoryController, base.Player.AbstractQuestControllerClass, base.Player.AbstractAchievementControllerClass, base.Player.AbstractPrestigeControllerClass, loot, EInventoryTab.Gear, isFakeContainer);
	}

	public virtual void ShowInventoryScreen(Action exitAction, IHealthController healthController, InventoryController controller, AbstractQuestControllerClass questController, AbstractAchievementControllerClass achievementsController, AbstractPrestigeControllerClass prestigeController, [CanBeNull] CompoundItem lootItem, EInventoryTab tab, bool showAsGridContent = false)
	{
		if (!CurrentScreenSingletonClass.Instance.CheckCurrentScreen(EEftScreenType.BattleUI))
		{
			Debug.Log("<colo=red>Settings or menu screen is active. I can't search.</color>");
			exitAction();
			return;
		}
		EItemViewType viewType = ((base.Player.BtrState == EPlayerBtrState.Inside) ? EItemViewType.InventoryWithoutDiscard : EItemViewType.Inventory);
		InventoryScreen.GClass3876 inventoryScreenController = new InventoryScreen.GClass3876(Session, base.Player.Profile, healthController, controller, questController, achievementsController, prestigeController, lootItem, tab, showAsGridContent, viewType);
		inventoryScreenController.OnClose += delegate
		{
			healthController.DiedEvent -= delegate
			{
				inventoryScreenController?.CloseScreen();
			};
			base.Player.UpdateInteractionCast();
			exitAction();
		};
		healthController.DiedEvent += delegate
		{
			inventoryScreenController?.CloseScreen();
		};
		inventoryScreenController.ShowScreen(EScreenState.Queued);
	}

	public override void ShowBattleUIScreen()
	{
		BattleUIScreenController.OnClose += delegate
		{
			_timerPanel.Hide();
			MonoBehaviourSingleton<PreloaderUI>.Instance.RaidInfoVisibility = false;
		};
		BattleUIScreenController.OnShow += delegate
		{
			_timerPanel.Reveal();
			MonoBehaviourSingleton<PreloaderUI>.Instance.RaidInfoVisibility = true;
		};
		MonoBehaviourSingleton<PreloaderUI>.Instance.ResetTimersForShowRttAndLoss();
		BattleUIScreenController.ShowScreen(EScreenState.Root);
	}

	public override void InitBattleUIScreen()
	{
		BattleUIScreenController = new EftBattleUIScreen.GClass3866(this, _gameDateTime, _location, _insurance);
	}

	public override void MumbleStatusChangedHandler(bool active)
	{
		base.MumbleStatusChangedHandler(active);
		if (active)
		{
			ReleaseTactical();
		}
	}

	[CompilerGenerated]
	public void method_22()
	{
		base.Player.HealthController.DiedEvent -= delegate
		{
			ReleaseTactical();
		};
	}

	[CompilerGenerated]
	public void method_23()
	{
		CurrentScreenSingletonClass.Instance.OnScreenChanged -= delegate(EEftScreenType screenType)
		{
			if (screenType != EEftScreenType.BattleUI)
			{
				ReleaseTactical();
			}
		};
	}

	[CompilerGenerated]
	public void method_24()
	{
		base.Player.HandsChangingEvent -= base.ReleaseTactical;
	}

	[CompilerGenerated]
	public void method_25(EEftScreenType screenType)
	{
		if (screenType != EEftScreenType.BattleUI)
		{
			ReleaseTactical();
		}
	}

	[CompilerGenerated]
	public void method_26(EDamageType damage)
	{
		ReleaseTactical();
	}

	[CompilerGenerated]
	public void method_27()
	{
		base.Player.SetInventoryOpened(opened: false);
	}

	[CompilerGenerated]
	public void method_28()
	{
		_timerPanel.Hide();
		MonoBehaviourSingleton<PreloaderUI>.Instance.RaidInfoVisibility = false;
	}

	[CompilerGenerated]
	public void method_29()
	{
		_timerPanel.Reveal();
		MonoBehaviourSingleton<PreloaderUI>.Instance.RaidInfoVisibility = true;
	}
}
