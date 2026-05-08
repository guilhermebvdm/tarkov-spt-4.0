using System;
using EFT;
using UnityEngine;

public class GClass581
{
	public Grenade Grenade;

	public float StartTime;

	public float EndTime;

	[NonSerialized]
	public float Float_0;

	public bool SafePlaceFound;

	[NonSerialized]
	public Vector3 Vector3_0;

	[NonSerialized]
	public BotOwner BotOwner_0;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public bool Bool_1;

	public Vector3 DangerPoint
	{
		get
		{
			if (Grenade != null && Grenade.gameObject != null)
			{
				return BotOwner_0.Settings.FileSettings.Grenade.BEWARE_TYPE switch
				{
					3 => Vector3_0, 
					2 => Grenade.transform.position, 
					_ => (Vector3_0 + Grenade.transform.position) / 2f, 
				};
			}
			Bool_0 = true;
			return Vector3_0;
		}
	}

	public GClass581(Vector3 point, Grenade grenade, BotOwner owner, float startDelay)
	{
		BotOwner_0 = owner;
		Grenade = grenade;
		grenade.DestroyEvent += method_0;
		Vector3_0 = point;
		StartTime = Time.time + startDelay;
		EndTime = Time.time + LocalBotSettingsProviderClass.Core.DELTA_GRENADE_END_TIME;
	}

	public bool ShallDestroy()
	{
		if (Bool_0)
		{
			method_1();
			return true;
		}
		if (Time.time > EndTime)
		{
			method_1();
			return true;
		}
		if (Grenade == null)
		{
			method_1();
			return true;
		}
		return false;
	}

	public bool ShallRunAway()
	{
		if (IsActive() && (BotOwner_0.Transform.position - DangerPoint).sqrMagnitude < LocalBotSettingsProviderClass.Core.DELTA_GRENADE_SAFE_DIST_SQRT)
		{
			return true;
		}
		return false;
	}

	public bool IsActive()
	{
		return Time.time > StartTime;
	}

	public void method_0(Throwable arg3)
	{
		arg3.DestroyEvent -= method_0;
		Bool_0 = true;
		if (!(Grenade != null) || !(Grenade is SmokeGrenade))
		{
			Grenade = null;
		}
	}

	public bool IsCoverSafe(Vector3 posOnFloor)
	{
		if (Float_0 > Time.time)
		{
			return Bool_1;
		}
		Float_0 = Time.time + 2f;
		Vector3 direction = posOnFloor + Vector3.up - DangerPoint;
		float magnitude = direction.magnitude;
		Ray ray = new Ray(DangerPoint, direction);
		Bool_1 = Physics.Raycast(ray, out var _, magnitude, LayerMaskClass.HighPolyWithTerrainMask);
		return Bool_1;
	}

	public void method_1()
	{
		if (Grenade != null)
		{
			Grenade.DestroyEvent -= method_0;
		}
	}
}
