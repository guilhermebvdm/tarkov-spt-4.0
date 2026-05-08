using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Comfort.Common;
using UnityEngine;

public class SonicBulletSoundPlayer : BulletSoundPlayer
{
	public class Class522 : GClass952.GInterface48
	{
		[NonSerialized]
		public GClass944<Class522> Gclass944_0;

		[NonSerialized]
		public Vector3 Vector3_0;

		[NonSerialized]
		public AudioClip AudioClip_0;

		[NonSerialized]
		public float Float_0;

		[NonSerialized]
		public int Int_0;

		[NonSerialized]
		public float Float_1;

		public void Initialize(GClass944<Class522> pool, Vector3 start, AudioClip clip, float distance, int rollOff, float volume)
		{
			Gclass944_0 = pool;
			Vector3_0 = start;
			AudioClip_0 = clip;
			Float_0 = distance;
			Int_0 = rollOff;
			Float_1 = volume;
		}

		public void Release()
		{
			Singleton<BetterAudio>.Instance.PlayAtPoint(Vector3_0, AudioClip_0, Float_0, BetterAudio.AudioSourceGroupType.Weaponry, Int_0, Float_1);
			AudioClip_0 = null;
			Gclass944_0.PutObject(this);
		}
	}

	public enum SonicType
	{
		Sonic9,
		Sonic545,
		Sonic762,
		SonicShotgun
	}

	public class GClass898
	{
		public AmmoItemClass Ammo;

		public Vector3 ShotPosition;

		public Vector3 ShotDirection;

		public Camera Camera;

		public float Rolloff;

		public float Delay;

		public bool IsOccluded;

		public GClass898(AmmoItemClass ammo, Vector3 shotPosition, Vector3 shotDirection, Camera camera, float rolloff, float delay, bool isOccluded)
		{
			Ammo = ammo;
			ShotPosition = shotPosition;
			ShotDirection = shotDirection;
			Camera = camera;
			Rolloff = rolloff;
			Delay = delay;
			IsOccluded = isOccluded;
		}
	}

	[Serializable]
	public class SonicAudio
	{
		public SonicType Type;

		public AudioClip Clip;
	}

	[CompilerGenerated]
	public class Class523
	{
		public SonicType type;

		public bool method_0(SonicAudio x)
		{
			return x.Type == type;
		}
	}

	private readonly float float_0 = 340.29f;

	[SerializeField]
	private List<SonicAudio> _sources;

	[SerializeField]
	private AnimationCurve _soundCurve;

	[SerializeField]
	private float _minDistance = 1.5f;

	private readonly GClass944<Class522> gclass944_0 = new GClass944<Class522>(5, 2);

	public void Awake()
	{
		GClass897.OnShoot += FireBullet;
	}

	public void OnDestroy()
	{
		GClass897.OnShoot -= FireBullet;
	}

	public void FireBullet(GClass898 sonicInfo)
	{
		if (sonicInfo.IsOccluded || sonicInfo.Ammo.InitialSpeed < float_0)
		{
			return;
		}
		Vector3 shotPosition = sonicInfo.ShotPosition;
		Vector3 end = shotPosition + sonicInfo.ShotDirection * 1000f;
		Vector3 vector = GClass897.CalculateNormalFromPoint(shotPosition, end, sonicInfo.Camera.transform.position);
		if (vector == Vector3.zero)
		{
			return;
		}
		float num = CameraClass.Instance.Distance(shotPosition);
		if (!(num < _minDistance))
		{
			int rollOff = method_1(sonicInfo, num);
			float magnitude = vector.magnitude;
			float num2 = num * 0.125f;
			if (!(magnitude > num2))
			{
				SonicType sonicType = sonicInfo.Ammo.SonicType;
				SonicAudio sonicAudio = method_0(sonicType);
				float time = magnitude / num2;
				float volume = _soundCurve.Evaluate(time);
				Class522 @class = gclass944_0.GetObject();
				@class.Initialize(gclass944_0, shotPosition, sonicAudio.Clip, num, rollOff, volume);
				double dspTime = AudioSettings.dspTime;
				Singleton<BetterAudio>.Instance.AddToAudioSourceQueue(@class, dspTime + (double)sonicInfo.Delay);
			}
		}
	}

	public SonicAudio method_0(SonicType type)
	{
		return _sources.First((SonicAudio x) => x.Type == type);
	}

	public int method_1(GClass898 sonicInfo, float distance)
	{
		float t = distance / sonicInfo.Rolloff;
		return (int)Mathf.Lerp(2000f, sonicInfo.Rolloff, t);
	}
}
