using System;
using Comfort.Common;
using EFT.ClientItems.ClientSpecItems;

namespace EFT;

public class RadioTransmitterController : Player.UsableItemController
{
	public class Class1298 : Class1293
	{
		public Class1298(RadioTransmitterController controller)
			: base(controller)
		{
		}

		public override void SetAiming(bool isAiming)
		{
		}

		public override void vmethod_0()
		{
			UsableItemController_0.InitiateOperation<Class1301>().Start();
		}
	}

	public class Class1296 : Class1294
	{
		public Class1296(RadioTransmitterController controller)
			: base(controller)
		{
		}
	}

	public class Class1301 : Class1299
	{
		public Class1301(RadioTransmitterController controller)
			: base(controller)
		{
		}

		public override void SetAiming(bool isAiming)
		{
		}

		public override void HideWeapon(Action onHidden, bool fastDrop)
		{
			State = Player.EOperationState.Finished;
			UsableItemController_0.InitiateOperation<Class1304>().Start(onHidden, fastDrop);
		}

		public override void vmethod_0(GInterface443 oneItemOperation, Callback callback)
		{
			UsableItemController_0.InitiateOperation<Class1298>().Start(oneItemOperation.Item1, callback);
		}
	}

	public class Class1304 : Class1302
	{
		public Class1304(RadioTransmitterController controller)
			: base(controller)
		{
		}
	}

	public class Class1307 : Class1305
	{
		public Class1307(RadioTransmitterController controller)
			: base(controller)
		{
		}

		public override void vmethod_0()
		{
			Class1301 @class = UsableItemController_0.InitiateOperation<Class1301>();
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

	private RadioTransmitterView radioTransmitterView_0;

	public override void vmethod_0(Player player, WeaponPrefab weaponPrefab)
	{
		base.vmethod_0(player, weaponPrefab);
		if (radioTransmitterView_0 == null)
		{
			radioTransmitterView_0 = weaponPrefab.GetComponentInChildren<RadioTransmitterView>();
			radioTransmitterView_0.Initialiaze(player);
		}
		method_6();
		gclass2086_0.AfterGetFromPoolInit(player.ProceduralWeaponAnimation, null, player.IsYourPlayer);
		BaseSoundPlayer component = _controllerObject.GetComponent<BaseSoundPlayer>();
		if (component != null)
		{
			component.Init(this, player.PlayerBones.WeaponRoot, player);
		}
	}

	public override void vmethod_2(EPlayerState previousstate, EPlayerState nextstate)
	{
	}

	public override void SetAim(bool value)
	{
	}
}
