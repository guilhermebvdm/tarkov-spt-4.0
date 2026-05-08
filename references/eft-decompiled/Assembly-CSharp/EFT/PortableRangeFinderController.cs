using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Comfort.Common;
using EFT.CameraControl;

namespace EFT;

public class PortableRangeFinderController : Player.UsableItemController
{
	public class Class1297 : Class1293
	{
		public Class1297(PortableRangeFinderController controller)
			: base(controller)
		{
		}

		public override void SetAiming(bool isAiming)
		{
			base.SetAiming(isAiming);
			if (Player_0.PointOfView == EPointOfView.FirstPerson)
			{
				PortableRangeFinderController obj = UsableItemController_0 as PortableRangeFinderController;
				obj.opticSight_0.enabled = isAiming;
				obj.tacticalRangeFinderController_0.gameObject.SetActive(isAiming);
			}
		}

		public override void vmethod_0()
		{
			UsableItemController_0.InitiateOperation<Class1300>().Start();
		}
	}

	public class Class1295 : Class1294
	{
		public Class1295(PortableRangeFinderController controller)
			: base(controller)
		{
		}
	}

	public class Class1300 : Class1299
	{
		public Class1300(PortableRangeFinderController controller)
			: base(controller)
		{
		}

		public override void SetAiming(bool isAiming)
		{
			base.SetAiming(isAiming);
			if (Player_0.PointOfView == EPointOfView.FirstPerson)
			{
				PortableRangeFinderController obj = UsableItemController_0 as PortableRangeFinderController;
				obj.opticSight_0.enabled = isAiming;
				obj.tacticalRangeFinderController_0.gameObject.SetActive(isAiming);
			}
		}

		public override void HideWeapon(Action onHidden, bool fastDrop)
		{
			State = Player.EOperationState.Finished;
			UsableItemController_0.InitiateOperation<Class1303>().Start(onHidden, fastDrop);
		}

		public override void vmethod_0(GInterface443 oneItemOperation, Callback callback)
		{
			UsableItemController_0.InitiateOperation<Class1297>().Start(oneItemOperation.Item1, callback);
		}
	}

	public class Class1303 : Class1302
	{
		public Class1303(PortableRangeFinderController controller)
			: base(controller)
		{
		}

		public override void Start(Action onHidden, bool fastDrop)
		{
			base.Start(onHidden, fastDrop);
			PortableRangeFinderController obj = UsableItemController_0 as PortableRangeFinderController;
			obj.opticSight_0.enabled = false;
			obj.tacticalRangeFinderController_0.gameObject.SetActive(value: false);
		}
	}

	public class Class1306 : Class1305
	{
		public Class1306(PortableRangeFinderController controller)
			: base(controller)
		{
		}

		public override void vmethod_0()
		{
			Class1300 @class = UsableItemController_0.InitiateOperation<Class1300>();
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

	private const float float_3 = 2f;

	private OpticSight opticSight_0;

	private TacticalRangeFinderController tacticalRangeFinderController_0;

	public override Dictionary<Type, OperationFactoryDelegate> GetOperationFactoryDelegates()
	{
		return new Dictionary<Type, OperationFactoryDelegate>
		{
			{
				typeof(Class1306),
				() => new Class1306(this)
			},
			{
				typeof(Class1300),
				() => new Class1300(this)
			},
			{
				typeof(Class1303),
				() => new Class1303(this)
			},
			{
				typeof(Class1297),
				() => new Class1297(this)
			}
		};
	}

	public override void vmethod_0(Player player, WeaponPrefab weaponPrefab)
	{
		base.vmethod_0(player, weaponPrefab);
		opticSight_0 = weaponPrefab.GetComponentInChildren<OpticSight>();
		tacticalRangeFinderController_0 = opticSight_0.GetComponentInChildren<TacticalRangeFinderController>(includeInactive: true);
		player.ProceduralWeaponAnimation.ManualSetVariables(2f, 0f, 0f, 0f);
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

	public override void vmethod_1(Action callback)
	{
		InitiateOperation<Class1306>().Start(callback);
	}

	[CompilerGenerated]
	public Player.BaseAnimationOperationClass method_15()
	{
		return new Class1306(this);
	}

	[CompilerGenerated]
	public Player.BaseAnimationOperationClass method_16()
	{
		return new Class1300(this);
	}

	[CompilerGenerated]
	public Player.BaseAnimationOperationClass method_17()
	{
		return new Class1303(this);
	}

	[CompilerGenerated]
	public Player.BaseAnimationOperationClass method_18()
	{
		return new Class1297(this);
	}
}
