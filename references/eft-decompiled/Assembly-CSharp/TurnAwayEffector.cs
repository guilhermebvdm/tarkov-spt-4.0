using System;
using EFT;
using EFT.Animations;
using UnityEngine;

[Serializable]
public class TurnAwayEffector : IEffector
{
	[Serializable]
	public class AnimVal
	{
		public enum ComponentType
		{
			X,
			Y,
			Z
		}

		public float Intensity;

		public float AltIntensity;

		public float Intensity1;

		public float AltIntensity1;

		public AnimationCurve Curve;

		public ComponentType Component;

		[NonSerialized]
		public int Component_1;

		public void Initialize()
		{
			Component_1 = (int)Component;
		}

		public virtual void Process(ref Vector3 vec, float value, EPointOfView pov, float weight)
		{
			if (pov == EPointOfView.FirstPerson)
			{
				vec[Component_1] = Curve.Evaluate(value) * Mathf.Lerp(Intensity, Intensity1, weight);
			}
			else
			{
				vec[Component_1] = Curve.Evaluate(value) * Mathf.Lerp(AltIntensity, AltIntensity1, weight);
			}
		}
	}

	public float OverlapValue;

	[SerializeField]
	public TurnAwayPose _rifleShift;

	[SerializeField]
	public TurnAwayPose _pistolShift;

	[SerializeField]
	public TurnAwayPose _proneShift;

	[SerializeField]
	public TurnAwayPose[] _elbows;

	[NonSerialized]
	public TurnAwayPose CurrentShift;

	[SerializeField]
	public float _blendSpeed = 10f;

	[SerializeField]
	public float _turnAwayThreshold = 0.5f;

	[SerializeField]
	public float _turnAwayLeftStanceThreshold = 0.09f;

	[SerializeField]
	public float _playerOverlapThreshold = 0.6f;

	[SerializeField]
	public float _playerLeftStanceOverlapThreshold = 0.09f;

	[SerializeField]
	public float _inSmoothTime = 5f;

	[SerializeField]
	public float _outSmoothTime = 5f;

	[NonSerialized]
	public EPointOfView Pov = EPointOfView.ThirdPerson;

	[NonSerialized]
	public bool IsPistol_1;

	[NonSerialized]
	public bool IsInProne;

	[NonSerialized]
	public bool IsLeftStance;

	[NonSerialized]
	public bool IsInMountState;

	public bool IsDirty = true;

	[NonSerialized]
	public float FovInput = 1f;

	public bool OverlapsWithPlayer;

	public float OverlapDepth;

	public float OriginZShift;

	[NonSerialized]
	public float FovScale_1;

	[NonSerialized]
	public float NormalFovScale;

	public float Overlap;

	[NonSerialized]
	public float Blend;

	public AnimationCurve CameraShiftCurve;

	[NonSerialized]
	public Vector3 Position_1;

	[NonSerialized]
	public Vector3 Rotation_1;

	[NonSerialized]
	public Vector3 CameraShift_1;

	[NonSerialized]
	public Vector3 LElbowShift_1;

	[NonSerialized]
	public Vector3 RElbowShift_1;

	[NonSerialized]
	public float CameraZShift;

	[NonSerialized]
	public float MaxZShift = 0.4f;

	[SerializeField]
	public float _cameraShiftAtMaxOverlap = 0.02f;

	public bool IsInPronePose
	{
		set
		{
			IsInProne = value;
			UpdatePreset();
		}
	}

	public bool LeftStance
	{
		set
		{
			IsLeftStance = value;
		}
	}

	public bool InMountState
	{
		set
		{
			IsInMountState = value;
		}
	}

	public bool IsPistol
	{
		set
		{
			IsPistol_1 = value;
			UpdatePreset();
		}
	}

	public Vector3 Position => Position_1;

	public Vector3 Rotation => Rotation_1;

	public float FovScale
	{
		set
		{
			FovInput = value;
			FovScale_1 = ((Pov == EPointOfView.ThirdPerson) ? 1f : FovInput);
			NormalFovScale = Mathf.InverseLerp(0.65f, 1f, FovScale_1);
		}
	}

	public EPointOfView PointOfView
	{
		set
		{
			Pov = value;
			FovScale = FovInput;
			UpdatePreset();
		}
	}

	public Vector3 CameraShift => CameraShift_1;

	public Vector3 LElbowShift => LElbowShift_1;

	public Vector3 RElbowShift => RElbowShift_1;

	public void Initialize(PlayerSpring playerSpring)
	{
		TurnAwayPose[] array = new TurnAwayPose[3] { _rifleShift, _pistolShift, _proneShift };
		foreach (TurnAwayPose turnAwayPose in array)
		{
			AnimVal[] pos = turnAwayPose.Pos;
			for (int j = 0; j < pos.Length; j++)
			{
				pos[j].Initialize();
			}
			pos = turnAwayPose.Rot;
			for (int j = 0; j < pos.Length; j++)
			{
				pos[j].Initialize();
			}
		}
		array = _elbows;
		for (int i = 0; i < array.Length; i++)
		{
			AnimVal[] pos = array[i].Pos;
			for (int j = 0; j < pos.Length; j++)
			{
				pos[j].Initialize();
			}
		}
		UpdatePreset();
	}

	public void UpdatePreset()
	{
		CurrentShift = (IsInProne ? _proneShift : (IsPistol_1 ? _pistolShift : _rifleShift));
		MaxZShift = ((Pov == EPointOfView.FirstPerson) ? CurrentShift.Pos[1].Intensity : CurrentShift.Pos[1].AltIntensity);
	}

	public void Process(float deltaTime)
	{
		Position_1 = (Rotation_1 = (LElbowShift_1 = (RElbowShift_1 = (CameraShift_1 = Vector3.zero))));
		float num = (OverlapsWithPlayer ? (OverlapDepth / _playerOverlapThreshold) : (OverlapDepth / _turnAwayThreshold));
		if (IsInMountState)
		{
			num = Mathf.Clamp(num, 0f, 1f);
		}
		OverlapValue = Mathf.Lerp(OverlapValue, num, (num > OverlapValue) ? (_inSmoothTime * deltaTime) : (_outSmoothTime * deltaTime));
		if (!(OverlapValue <= 0.001f))
		{
			float b = ((OverlapValue > 1f) ? 1 : 0);
			Blend = Mathf.Lerp(Blend, b, _blendSpeed * deltaTime);
			AnimVal[] pos = CurrentShift.Pos;
			for (int i = 0; i < pos.Length; i++)
			{
				pos[i].Process(ref Position_1, OverlapValue, Pov, Blend);
			}
			Position_1.y = Mathf.Clamp(Position_1.y * FovScale_1, 0f, MaxZShift - OriginZShift);
			pos = CurrentShift.Rot;
			for (int i = 0; i < pos.Length; i++)
			{
				pos[i].Process(ref Rotation_1, OverlapValue, Pov, Blend);
			}
			pos = _elbows[0].Pos;
			for (int i = 0; i < pos.Length; i++)
			{
				pos[i].Process(ref LElbowShift_1, OverlapValue, Pov, Blend);
			}
			pos = _elbows[1].Pos;
			for (int i = 0; i < pos.Length; i++)
			{
				pos[i].Process(ref RElbowShift_1, OverlapValue, Pov, Blend);
			}
			if (Pov == EPointOfView.FirstPerson)
			{
				CameraZShift = Mathf.Lerp(CameraZShift, (OverlapValue > 1f) ? _cameraShiftAtMaxOverlap : (CameraShiftCurve.Evaluate(OverlapValue) * NormalFovScale), deltaTime * _blendSpeed);
				CameraShift_1 = new Vector3(0f, 0f, CameraZShift);
			}
		}
	}

	public string DebugOutput()
	{
		throw new NotImplementedException();
	}
}
