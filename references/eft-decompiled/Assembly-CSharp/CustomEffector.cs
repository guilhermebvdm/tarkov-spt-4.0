using System;
using System.Linq;
using System.Runtime.CompilerServices;
using EFT;
using EFT.Animations;
using UnityEngine;

[Serializable]
public class CustomEffector : IEffector
{
	[Serializable]
	[CompilerGenerated]
	public class Class543
	{
		public static readonly Class543 class543_0 = new Class543();

		public static Func<AnimVal, GClass2793> func_0;

		public GClass2793 method_0(AnimVal c)
		{
			return new GClass2793(c);
		}
	}

	[NonSerialized]
	public bool Inited;

	public WalkPreset AimPose;

	public float RestTransitionSpeed = 2f;

	public float RestDelay = 10f;

	public AnimVal RestValues;

	public GClass2793[] _aimPose;

	[NonSerialized]
	public GClass2793 RestProcessor;

	[NonSerialized]
	public float LastActivityTime;

	[NonSerialized]
	public float InitAimRotation;

	public Player.ValueBlender PoseBlender = new Player.ValueBlender
	{
		Speed = 4f
	};

	[NonSerialized]
	public float PoseNormalTime;

	[NonSerialized]
	public float AimTarget;

	public Vector3 Offset => new Vector3(-0.002f, 0f, 0.02f) * (1f - PoseBlender.Value);

	public bool Aim
	{
		set
		{
			if (Inited)
			{
				PoseBlender.Target = (value ? 1 : 0);
				_aimPose[0].Processors[1].Intensity = ((PoseBlender.Target == 1f) ? InitAimRotation : (0f - InitAimRotation));
			}
		}
	}

	public void UpdateActivityTime()
	{
		LastActivityTime = Time.time;
	}

	public void Initialize(PlayerSpring playerSpring)
	{
		UpdateActivityTime();
		_aimPose = AimPose.Curves.Select((AnimVal c) => new GClass2793(c)).ToArray();
		GClass2793[] aimPose = _aimPose;
		for (int num = 0; num < aimPose.Length; num++)
		{
			aimPose[num].Initialize(null, playerSpring.HandsPosition, playerSpring.HandsRotation);
		}
		if (RestValues != null)
		{
			RestProcessor = new GClass2793(RestValues);
			RestProcessor.Initialize(null, playerSpring.HandsPosition, playerSpring.HandsRotation);
		}
		InitAimRotation = AimPose[0].Vals[1].Intensity;
		Inited = true;
	}

	public void Process(float intensity)
	{
		if (!(Math.Abs(PoseBlender.Value) < float.Epsilon))
		{
			GClass2793[] aimPose = _aimPose;
			for (int i = 0; i < aimPose.Length; i++)
			{
				aimPose[i].ProcessAtTime(PoseBlender.Value, intensity);
			}
		}
	}

	public void ProcessRestPose()
	{
		if (RestProcessor != null)
		{
			float num = Time.time - LastActivityTime - RestDelay;
			if (num > 0f)
			{
				num = Mathf.Clamp01(num * RestTransitionSpeed);
				RestProcessor.ProcessAtTime(num);
			}
		}
	}

	public string DebugOutput()
	{
		throw new NotImplementedException();
	}

	public void Clear()
	{
		RestValues = null;
		RestProcessor = null;
	}
}
