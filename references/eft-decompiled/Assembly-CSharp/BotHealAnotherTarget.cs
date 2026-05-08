using System;
using Comfort.Common;
using EFT;
using UnityEngine;

public class BotHealAnotherTarget : GClass429
{
	[NonSerialized]
	public const float MAX_SDIST_TO_START_HEAL = 4f;

	public IPlayer Target;

	[NonSerialized]
	public float NextPosible;

	[NonSerialized]
	public float AskTime;

	[field: NonSerialized]
	public bool IsInProcess { get; set; }

	public event Action<IPlayer> OnHealAsked;

	public BotHealAnotherTarget(BotOwner owner)
		: base(owner)
	{
	}

	public void HealAsk(IPlayer target)
	{
		if (!IsInProcess)
		{
			this.OnHealAsked?.Invoke(target);
			Target = target;
			AskTime = Time.time;
		}
	}

	public bool ShallDoHeal()
	{
		if (IsInProcess)
		{
			return true;
		}
		if (Time.time - AskTime < 2f && Target != null && (Target.Position - BotOwner_0.Position).sqrMagnitude < 4f)
		{
			return true;
		}
		return false;
	}

	public void ManualUpdate()
	{
		if (Target == null)
		{
			return;
		}
		Vector3 dir = Target.Position - BotOwner_0.Position;
		dir.y = 0f;
		BotOwner_0.Steering.LookToDirection(dir);
		if (NextPosible < Time.time && !IsInProcess)
		{
			if (ShallDoHeal())
			{
				IsInProcess = true;
				Debug.LogError($"START Heal another {Time.time}");
				BotOwner_0.Medecine.Stimulators.StartApplyToTarget(method_0);
				NextPosible = Time.time + 15f;
			}
			else
			{
				NextPosible = Time.time + 1f;
			}
		}
	}

	public void method_0(bool isOok)
	{
		Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(Target.ProfileId).ActiveHealthController.RestoreFullHealth();
		Debug.LogError($"Heal another completed {Time.time}");
		IsInProcess = false;
		Target = null;
	}
}
