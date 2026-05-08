using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Comfort.Common;
using EFT;
using EFT.MovingPlatforms;
using UnityEngine;

public class SmokeGrenade : Grenade, MovingPlatform.GInterface459
{
	private float float_7;

	private float float_8;

	private float float_9;

	private Vector3 vector3_3 = new Vector3(1f, 1f, 1f);

	public Action<Grenade> EmissionEnd;

	private const float float_10 = 60f;

	[CompilerGenerated]
	private MovingPlatform movingPlatform_0;

	public float EmitTime => float_9;

	public SmokeGrenadeSettings SmokeGrenadeSettings_0 => _grenadeSettings as SmokeGrenadeSettings;

	public SphereCollider Area => SmokeGrenadeSettings_0._emissionArea;

	public float Radius => float_8;

	public MovingPlatform Platform
	{
		[CompilerGenerated]
		get
		{
			return movingPlatform_0;
		}
		[CompilerGenerated]
		set
		{
			movingPlatform_0 = value;
		}
	}

	public SmokeGrenadeDataPacketStruct NetworkData => new SmokeGrenadeDataPacketStruct
	{
		PlatformId = (short)((Platform == null) ? (-1) : ((short)Array.IndexOf(Singleton<GameWorld>.Instance.Platforms, Platform))),
		Position = ((Platform == null) ? base.transform.position : base.transform.localPosition),
		Template = base.WeaponSource.TemplateId,
		Id = base.WeaponSource.Id,
		Time = (int)(Time.time - float_7),
		Orientation = ((Platform == null) ? base.transform.rotation : base.transform.localRotation)
	};

	public Vector3 Vector3_0 => base.transform.position;

	public float LifeTime
	{
		get
		{
			Keyframe keyframe = SmokeGrenadeSettings_0._sizeOverTime[SmokeGrenadeSettings_0._sizeOverTime.length - 1];
			return EmitTime * keyframe.time;
		}
	}

	public override void Init(GrenadeSettings settings, string profileId, ThrowWeapItemClass throwWeap, float timeSpent, ISharedBallisticsCalculator calculator, bool isBeingPlanted)
	{
		float_9 = throwWeap.EmitTime;
		if (float_9 <= 0f)
		{
			Debug.LogError(GClass2348.Localized(throwWeap.ShortName) + " has 0 EmitTime. Update parameters or download templates. Set to 30");
			float_9 = 30f;
		}
		float_9 -= timeSpent;
		base.Init(settings, profileId, throwWeap, timeSpent, calculator, isBeingPlanted);
		StartEmissionBehaviour();
		base.VelocityBelowThreshold += RemoveRigidBodyOnVelocityDrop;
		GClass7.StartBehaviourTimer(this, 60f, RemoveRigidbody);
	}

	public void method_7()
	{
		for (int i = 0; i < SmokeGrenadeSettings_0._sizeOverTime.length; i++)
		{
			Keyframe keyframe = SmokeGrenadeSettings_0._sizeOverTime[i];
			if (keyframe.value > float_8)
			{
				float_8 = keyframe.value;
			}
		}
	}

	public void SetEmission(float time)
	{
		method_7();
		float_7 = time;
	}

	[ContextMenu("Update Emission Area")]
	public void UpdateEmissionArea(float normalTime)
	{
		float num = Mathf.Clamp01(SmokeGrenadeSettings_0._sizeOverTime.Evaluate(normalTime));
		bool flag = num > 0.1f;
		SmokeGrenadeSettings_0._emissionArea.gameObject.SetActive(flag);
		SmokeGrenadeSettings_0._emissionArea.transform.parent = (flag ? null : base.transform);
		float num2 = ((normalTime < 0.5f) ? Mathf.Lerp(SmokeGrenadeSettings_0._areaStartPosNorm, 1f, num) : 1f);
		if (flag)
		{
			SmokeGrenadeSettings_0._emissionArea.transform.position = base.transform.position + base.transform.rotation * SmokeGrenadeSettings_0._pivot * num2;
			SmokeGrenadeSettings_0._emissionArea.transform.localScale = vector3_3 * (SmokeGrenadeSettings_0._initialRadius * num * SmokeGrenadeSettings_0._radiusMultiplier);
			SmokeGrenadeSettings_0._emissionArea.transform.rotation = Quaternion.identity;
		}
	}

	public override void OnExplosion()
	{
		SetEmission(Time.time);
		StartCoroutine(EmissionAreaHandler(2f));
		StartCoroutine(DelayForEmitEvent(3f));
	}

	public override void SetThrowForce(Vector3 force)
	{
		base.SetThrowForce(force);
		Rigidbody.AddTorque(SmokeGrenadeSettings_0._torque + SmokeGrenadeSettings_0._torque * UnityEngine.Random.Range(0f - SmokeGrenadeSettings_0._torqueDelta, SmokeGrenadeSettings_0._torqueDelta), ForceMode.Acceleration);
	}

	public void StartEmissionBehaviour()
	{
		GClass7.StartBehaviourTimer(this, EmitTime, InvokeEmissionEndEvent);
	}

	public virtual void RemoveRigidBodyOnVelocityDrop(Throwable grenade)
	{
		RemoveRigidbody();
	}

	public void RemoveRigidbody()
	{
		if (Rigidbody != null)
		{
			UnityEngine.Object.Destroy(Rigidbody);
			Rigidbody = null;
		}
		if (Platform == null)
		{
			Singleton<GameWorld>.Instance.BoardIfOnPlatform(this, base.transform.position);
		}
	}

	[ContextMenu("EmissionEnd")]
	public void InvokeEmissionEndEvent()
	{
		if (EmissionEnd != null)
		{
			EmissionEnd(this);
		}
	}

	public IEnumerator DelayForEmitEvent(float time)
	{
		yield return new WaitForSeconds(time);
		if (Singleton<BotEventHandler>.Instantiated)
		{
			SmokeGrenade smokeGrenade = this;
			float smokeRadius = smokeGrenade?.Radius ?? 0f;
			float smokeLifeTime = smokeGrenade?.LifeTime ?? 0f;
			Singleton<BotEventHandler>.Instance.GrenadeExplosion(Vector3_0, base.ProfileId, isSmoke: true, smokeRadius, smokeLifeTime, Id);
		}
	}

	public IEnumerator EmissionAreaHandler(float rate)
	{
		float num = 0f;
		while (num < SmokeGrenadeSettings_0._sizeOverTime[SmokeGrenadeSettings_0._sizeOverTime.length - 1].time)
		{
			num = (Time.time - float_7) / EmitTime;
			UpdateEmissionArea(num);
			yield return new WaitForSeconds(rate);
		}
		UpdateEmissionArea(0f);
		base.enabled = false;
	}

	public void Board(MovingPlatform platform)
	{
		Platform = platform;
		base.transform.parent = platform.transform;
	}

	public void GetOff(MovingPlatform platform)
	{
		if (!(Platform != platform))
		{
			Platform = null;
			base.transform.parent = null;
		}
	}
}
