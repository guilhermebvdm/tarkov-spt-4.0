using System;
using EFT.Animations;
using UnityEngine;

[Serializable]
public class MotionEffector : IEffector
{
	public Vector3 Motion;

	public Vector3 Velocity;

	public float RotationInputClamp = 300f;

	[NonSerialized]
	public Vector3 LastPosition;

	[NonSerialized]
	public Vector3 LastForward;

	public Vector3 PositionVelocity;

	public Vector2 RotationVelocity;

	public Vector3 PositionAcceleration;

	public Vector2 RotationAcceleration;

	public Vector3 SwayFactors = Vector3.one;

	[NonSerialized]
	public Vector3 LastPositionVelocity;

	[NonSerialized]
	public Vector2 LastRotationVelocity;

	[NonSerialized]
	public Vector2 RotVelSum;

	[NonSerialized]
	public Vector2 RotAccSum;

	[NonSerialized]
	public Vector3 LastRotation;

	public float Intensity = 0.45f;

	public float BipodModifier = 0.15f;

	[NonSerialized]
	public Vector3 PlatformMovement;

	[NonSerialized]
	public Vector2 v;

	[NonSerialized]
	public Vector2 v4;

	[NonSerialized]
	public GClass908[] MouseProcessors;

	[NonSerialized]
	public GClass907[] MovementProcessors;

	public MotionEffectorParameters MouseParameters;

	public MotionEffectorParameters MovementParameters;

	[NonSerialized]
	public bool IsMounted;

	[NonSerialized]
	public bool IsBipodUsed;

	[NonSerialized]
	public bool Initialized;

	[NonSerialized]
	public Vector3 v2;

	[NonSerialized]
	public Vector3 v3;

	[NonSerialized]
	public bool NeedReset;

	[field: NonSerialized]
	public Transform Transform { get; set; }

	public void Initialize(PlayerSpring playerSpring)
	{
		Transform = playerSpring.TrackingTransform;
		MovementProcessors = GClass907.CreateInstance(MovementParameters);
		MouseProcessors = GClass908.CreateInstance(MouseParameters);
		GClass908[] mouseProcessors = MouseProcessors;
		for (int i = 0; i < mouseProcessors.Length; i++)
		{
			mouseProcessors[i].Initialize(playerSpring.SwaySpring);
		}
		GClass907[] movementProcessors = MovementProcessors;
		for (int i = 0; i < movementProcessors.Length; i++)
		{
			movementProcessors[i].Initialize(playerSpring.CameraRotation, playerSpring.HandsPosition, playerSpring.HandsRotation);
		}
	}

	public void AddPlatformMovement(Vector3 movement)
	{
		PlatformMovement += movement;
	}

	public void FixedTracking(float deltaTime)
	{
		if (NeedReset)
		{
			method_1();
			NeedReset = false;
		}
		Vector3 motion = Motion;
		float num = Mathf.Abs(Velocity.y);
		motion.y = Mathf.Clamp(motion.y, 0f - num, num);
		PositionVelocity = Vector3.SmoothDamp(PositionVelocity, Transform.InverseTransformDirection(motion), ref v2, MovementParameters.VelSmooth);
		LastPosition = Transform.position;
		PositionAcceleration = Vector3.SmoothDamp(PositionAcceleration, PositionVelocity - LastPositionVelocity, ref v3, MovementParameters.AccSmooth);
		LastPositionVelocity = PositionVelocity;
		Vector2 b = method_0() / deltaTime;
		RotationVelocity.x = Mathf.Clamp(RotationVelocity.x, 0f - RotationInputClamp, RotationInputClamp);
		RotationVelocity.y = Mathf.Clamp(RotationVelocity.y, 0f - RotationInputClamp, RotationInputClamp);
		RotAccSum = Vector2.Lerp(RotAccSum, RotationVelocity - LastRotationVelocity, 1f / 3f);
		LastRotationVelocity = RotationVelocity;
		RotVelSum = Vector2.Lerp(RotVelSum, b, 0.2f);
		RotationVelocity = Vector2.SmoothDamp(RotationVelocity, RotVelSum, ref v4, MouseParameters.VelSmooth, float.MaxValue, deltaTime);
		RotationAcceleration = Vector2.SmoothDamp(RotationAcceleration, RotAccSum, ref v, MouseParameters.AccSmooth, float.MaxValue, deltaTime);
	}

	public Vector2 method_0()
	{
		Vector3 vector = Transform.InverseTransformDirection(LastForward);
		Vector2 result = new Vector2
		{
			y = Mathf.Atan2(vector.z, vector.y) * 57.29578f - 90f,
			x = (0f - Mathf.Atan2(vector.x, vector.z)) * 57.29578f
		};
		LastForward = Transform.forward;
		return result;
	}

	public void Process(float deltaTime)
	{
		GClass907[] movementProcessors = MovementProcessors;
		for (int i = 0; i < movementProcessors.Length; i++)
		{
			movementProcessors[i].Process(this, deltaTime * Intensity * ((!IsMounted || !IsBipodUsed) ? 1f : BipodModifier));
		}
		GClass908[] mouseProcessors = MouseProcessors;
		for (int i = 0; i < mouseProcessors.Length; i++)
		{
			mouseProcessors[i].Process(this, deltaTime, SwayFactors * ((!IsMounted || !IsBipodUsed) ? 1f : BipodModifier));
		}
	}

	public void SetMounting(bool isMounted)
	{
		IsMounted = isMounted;
	}

	public void SetBipod(bool isBipodUsed)
	{
		IsBipodUsed = isBipodUsed;
	}

	public string DebugOutput()
	{
		return $"_rotationVelocity:{RotationVelocity}\n_rotationAcceleration:{RotationAcceleration}\n_positionAcceleration{PositionAcceleration * 100f}\n_positionVelocity{PositionVelocity * 100f}";
	}

	public void Reset()
	{
		NeedReset = true;
	}

	public void method_1()
	{
		LastPosition = Transform.position;
		PositionVelocity = Transform.InverseTransformDirection(Vector3.zero);
		LastPositionVelocity = Vector3.zero;
		PositionAcceleration = Vector3.zero;
		LastForward = Transform.forward;
		LastRotationVelocity = Vector2.zero;
		RotationAcceleration = Vector2.zero;
		v = Vector2.zero;
		v2 = Vector3.zero;
		v3 = Vector3.zero;
		RotAccSum = Vector2.zero;
		RotVelSum = Vector2.zero;
	}
}
