using System;
using EFT;
using UnityEngine;

public class GClass599 : GClass598
{
	[NonSerialized]
	public ThrowGrenadePlace ThrowGrenadePlace_0;

	[NonSerialized]
	public Action Action_0;

	[NonSerialized]
	public GClass596 Gclass596_0;

	[NonSerialized]
	public bool Bool_0 = true;

	public override Vector3 PlaceToThrow => ThrowGrenadePlace_0.From.transform.position;

	public GClass599(Player requester, ThrowGrenadePlace pos, GClass596 doorOpenRequest, Action callbackFinish)
		: base(requester)
	{
		Gclass596_0 = doorOpenRequest;
		if (Gclass596_0 != null)
		{
			Bool_0 = false;
			Gclass596_0.OnComplete += method_1;
		}
		Action_0 = callbackFinish;
		base.CanExecuteByMyself = true;
		ThrowGrenadePlace_0 = pos;
		BotRequestType = BotRequestType.throwGrenadeFromPlace;
	}

	public override void Take(BotOwner executor)
	{
		base.Take(executor);
		base.ThrowData = ThrowGrenadePlace_0.GetData();
		base.ThrowFromPos = base.ThrowData.Position;
		executor.WeaponManager.Grenades.OnGrenadeThrowComplete += base.method_0;
	}

	public override bool CanStartExecute(BotOwner executer)
	{
		if (ThrowGrenadePlace_0 == null)
		{
			return false;
		}
		return base.CanStartExecute(executer);
	}

	public override bool CanProceed()
	{
		if (!(Executor != null))
		{
			return true;
		}
		if (!Executor.WeaponManager.Grenades.HaveGrenade)
		{
			return false;
		}
		if (!base.ThrowFromPos.HasValue)
		{
			Debug.LogError("throw grenade request have not poition to throw");
			return false;
		}
		if (Executor.Memory.GoalEnemy != null && Executor.Memory.GoalEnemy.IsVisible)
		{
			_ = (Executor.Position - base.ThrowFromPos.Value).sqrMagnitude;
			_ = Executor.Settings.FileSettings.Grenade.REQUEST_DIST_MUST_THROW_SQRT;
			return true;
		}
		return true;
	}

	public override bool CanThrow()
	{
		return Bool_0;
	}

	public void method_1(bool obj)
	{
		Gclass596_0.OnComplete -= method_1;
		Bool_0 = true;
	}

	public override void Dispose()
	{
		if (Gclass596_0 != null)
		{
			Gclass596_0.OnComplete -= method_1;
		}
		if (Action_0 != null)
		{
			Action_0();
		}
		base.Dispose();
	}
}
