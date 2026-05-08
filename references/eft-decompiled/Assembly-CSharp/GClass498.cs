using EFT;
using JetBrains.Annotations;
using UnityEngine;

public class GClass498 : BotSteering
{
	public GClass498([NotNull] BotOwner owner)
		: base(owner)
	{
	}

	public override void SetXAngle(float degPerSec)
	{
		if (!BlockSteering)
		{
			if (BotOwner_0.HasPathAndNotComplete)
			{
				Vector3.Angle(BotOwner_0.Mover.DirCurPoint, LookDirection_1);
			}
			float target;
			if (BotOwner_0.LookedTransform != null)
			{
				Vector3 normalized = (BotOwner_0.LookedTransform.position - BotOwner_0.WeaponRoot.position).normalized;
				target = 57.29578f * Mathf.Atan2(normalized.x, normalized.z);
			}
			else
			{
				target = 57.29578f * Mathf.Atan2(LookDirection_1.x, LookDirection_1.z);
			}
			float num = Mathf.DeltaAngle(Player.Rotation.x, target);
			if (BotOwner_0.BotLay.IsLay && num > BotOwner_0.Settings.FileSettings.Look.ANGLE_FOR_GETUP)
			{
				BotOwner_0.BotLay.GetUp(withCheck: true);
			}
			float num2 = 180f;
			float num3 = ((!(num > 0f)) ? Mathf.Clamp(num, 0f - num2, 0f) : Mathf.Clamp(num, 0f, num2));
			BotOwner_0.AimingManager.CurrentAiming.RotateX(num3);
			Player.Rotate(new Vector2(num3, 0f), ignoreClamp: true);
		}
	}

	public override bool CanSteerToMovingDirection()
	{
		return true;
	}
}
