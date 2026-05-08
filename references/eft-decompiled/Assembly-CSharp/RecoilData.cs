using System;
using EFT;
using UnityEngine;

public class RecoilData
{
	[NonSerialized]
	public float RemainRecoilTime;

	[NonSerialized]
	public float LastRecoilSetTime;

	[NonSerialized]
	public Vector3 RecoilOffset_1;

	[NonSerialized]
	public Vector3 LastRecoilFullOffset;

	[NonSerialized]
	public BotOwner Owner;

	[NonSerialized]
	public float EndRecoilTime;

	[NonSerialized]
	public bool EndRecoilCheck;

	public Vector3 RecoilOffset => RecoilOffset_1;

	public RecoilData(BotOwner owner)
	{
		Owner = owner;
	}

	public void LosingRecoil()
	{
		if (RemainRecoilTime > 0f)
		{
			RemainRecoilTime -= Time.deltaTime;
			RemainRecoilTime = Mathf.Clamp(RemainRecoilTime, 0f, RemainRecoilTime);
			float num = RemainRecoilTime / LastRecoilSetTime;
			RecoilOffset_1 = LastRecoilFullOffset * num;
		}
	}

	public void CheckEndRecoil()
	{
		if (EndRecoilCheck && EndRecoilTime < Time.time)
		{
			RemainRecoilTime = 0f;
			RecoilOffset_1 = Vector3.zero;
		}
	}

	public void Recoil()
	{
		if (Owner.WeaponManager.IsNowAutomatic)
		{
			float num = Owner.Settings.FileSettings.Shoot.RECOIL_PER_METER * Owner.AimingManager.CurrentAiming.LastDist2Target;
			float num2 = num * Owner.Settings.FileSettings.Shoot.HORIZONT_RECOIL_COEF;
			float num3 = num * Owner.Settings.FileSettings.Shoot.MAX_RECOIL_PER_METER;
			float num4 = GClass856.Random(0f - num2, num2);
			float num5 = GClass856.Random(0f - num2, num2);
			float num6 = GClass856.Random(0f, num);
			Vector3 recoilOffset_ = new Vector3(RecoilOffset_1.x + num4, Mathf.Clamp(RecoilOffset_1.y + num6, 0f - num3, num3), RecoilOffset_1.z + num5);
			RecoilOffset_1 = recoilOffset_;
			RemainRecoilTime += Owner.Settings.FileSettings.Shoot.RECOIL_TIME_NORMALIZE;
			LastRecoilFullOffset = RecoilOffset_1;
			LastRecoilSetTime = RemainRecoilTime;
			EndRecoilTime = Time.time + RemainRecoilTime;
		}
	}

	public string Info()
	{
		if (RemainRecoilTime > 0f)
		{
			return "-";
		}
		return RecoilOffset_1.magnitude.ToString("0.00");
	}
}
