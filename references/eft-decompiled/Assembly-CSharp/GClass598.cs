using System;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

public abstract class GClass598 : BotRequest
{
	[NonSerialized]
	[CompilerGenerated]
	public Vector3? Nullable_0;

	[NonSerialized]
	[CompilerGenerated]
	public AIGreanageThrowData AigreanageThrowData_0;

	public Vector3? ThrowFromPos
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

	public AIGreanageThrowData ThrowData
	{
		[CompilerGenerated]
		get
		{
			return AigreanageThrowData_0;
		}
		[CompilerGenerated]
		set
		{
			AigreanageThrowData_0 = value;
		}
	}

	public abstract Vector3 PlaceToThrow { get; }

	public override EBotRequestMode RequestMode => EBotRequestMode.Fight;

	public GClass598(IPlayer requester)
		: base(requester, BotRequestType.throwGrenade)
	{
		base.CanExecuteByMyself = true;
	}

	public override bool CanStartExecute(BotOwner executer)
	{
		if (!executer.WeaponManager.Grenades.HaveGrenade)
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
		if (!ThrowFromPos.HasValue)
		{
			Debug.LogError("throw grenade request have not position to throw");
			return false;
		}
		if (Executor.Memory.GoalEnemy != null && Executor.Memory.GoalEnemy.IsVisible)
		{
			if ((Executor.Position - ThrowFromPos.Value).sqrMagnitude < Executor.Settings.FileSettings.Grenade.REQUEST_DIST_MUST_THROW_SQRT)
			{
				return true;
			}
			return false;
		}
		return true;
	}

	public override void PlayerDestroy(Player player)
	{
	}

	public virtual bool CanThrow()
	{
		return true;
	}

	public virtual bool IsComeToPlace()
	{
		Vector3 vector = Executor.Position - ThrowFromPos.Value;
		vector.y = 0f;
		if (vector.sqrMagnitude < 0.6f)
		{
			return true;
		}
		return false;
	}

	public override bool CanRequest(BotOwner requester)
	{
		return true;
	}

	public void method_0(ThrowWeapItemClass throwWeap)
	{
		Dispose();
	}

	public override void Dispose()
	{
		if (Executor != null && Executor.WeaponManager != null && Executor.WeaponManager.Grenades != null)
		{
			Executor.WeaponManager.Grenades.OnGrenadeThrowComplete -= method_0;
		}
		base.Dispose();
	}
}
