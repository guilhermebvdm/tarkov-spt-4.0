using System;
using EFT;
using EFT.Interactive;
using UnityEngine;

public class ApproachStateClass(MovementContext movementContext) : MovementState(movementContext)
{
	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public float Float_1;

	[NonSerialized]
	public float Float_2 = 0.33f;

	public Vector2 ApproachSpeed = new Vector2(2.5f, 0.01f);

	public float ApproachAngularSpeed = 90f;

	public float TMinus;

	public AnimationCurve ApproachCurve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 0f), new Keyframe(1f, 1f, 0f, 0f));

	[NonSerialized]
	public Vector3 Vector3_0;

	[NonSerialized]
	public Vector3 Vector3_1;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public Vector2 Vector2_0;

	[NonSerialized]
	public float Float_3;

	[NonSerialized]
	public float Float_4;

	[NonSerialized]
	public Vector3 Vector3_2;

	[NonSerialized]
	public float Float_5;

	[NonSerialized]
	public bool Bool_1;

	[NonSerialized]
	public bool Bool_2;

	[NonSerialized]
	public bool Bool_3;

	[NonSerialized]
	public Vector2 Vector2_1;

	[NonSerialized]
	public Vector3 Vector3_3;

	public bool ApproachDone => Float_0 > Float_2;

	public float ApproachProgress => Float_0 / Float_2;

	public override void Enter(bool isFromSameState)
	{
		MovementContext.StateLocksInventory = true;
		MovementContext.SetTilt(0f);
		MovementContext.SetBlindFire(0);
		base.Enter(isFromSameState);
		Bool_3 = false;
		MovementContext.IsInPronePose = false;
		Float_1 = Time.time;
		WorldInteractiveObject.GStruct436 interactionParameters = MovementContext.InteractionParameters;
		Vector3_0 = interactionParameters.InteractionPosition;
		Vector3_1 = interactionParameters.ViewTarget;
		Bool_0 = interactionParameters.Snap;
		Vector2_0 = MovementContext.Rotation;
		Vector3_2 = MovementContext.TransformPosition;
		Float_4 = MovementContext.PoseLevel;
		Float_3 = MovementContext.TransformRotation.eulerAngles.y;
		if (interactionParameters.RotationMode == WorldInteractiveObject.ERotationInterpolationMode.ViewTarget)
		{
			Bool_1 = Vector3_1.y > MovementContext.RibcagePosition().y;
			if (interactionParameters.BlockChangePosLevel)
			{
				Bool_1 = false;
			}
			if (!Bool_1)
			{
				float num = ((interactionParameters.Grip == null) ? (MovementContext.RibcagePosition().y - Vector3_1.y) : (MovementContext.RibcagePosition().y - interactionParameters.Grip.transform.position.y));
				Bool_2 = num > 0.65f;
			}
		}
		else
		{
			Bool_2 = interactionParameters.Sit;
		}
		if (interactionParameters.BlockChangePosLevel)
		{
			Bool_2 = false;
		}
		if (Bool_0)
		{
			float num2 = Vector3.Distance(MovementContext.TransformPosition, Vector3_0);
			Float_2 = num2 / GClass2298.Evaluate(ApproachSpeed, MovementContext.Overweight);
			switch (interactionParameters.RotationMode)
			{
			default:
				throw new ArgumentOutOfRangeException();
			case WorldInteractiveObject.ERotationInterpolationMode.ViewTarget:
				Vector2_1 = MovementContext.CalculateLookAtDirection(Vector3_1, Vector3_0, Bool_1 ? 1.5f : (Bool_2 ? 1.2f : 0f));
				break;
			case WorldInteractiveObject.ERotationInterpolationMode.ViewTargetWithZeroPitch:
				Vector2_1 = MovementContext.CalculateLookAtDirection(Vector3_1, Vector3_0, Bool_1 ? 1.5f : (Bool_2 ? 1.2f : 0f));
				Vector2_1.y = 0f;
				break;
			case WorldInteractiveObject.ERotationInterpolationMode.ViewTargetAsOrientation:
				Vector2_1 = new Vector2(interactionParameters.ViewTarget.x, interactionParameters.ViewTarget.y);
				break;
			}
			Vector2_1 = MovementContext.ClampRotation(Vector2_1);
			float num3 = Mathf.Abs(Mathf.DeltaAngle(Vector2_1.x, MovementContext.Rotation.x));
			float num4 = Mathf.Abs(Mathf.DeltaAngle(Vector2_1.y, MovementContext.Rotation.y));
			if (num3 / ApproachAngularSpeed > Float_2)
			{
				Float_2 = num3 / ApproachAngularSpeed;
			}
			if (num4 / ApproachAngularSpeed > Float_2)
			{
				Float_2 = num4 / ApproachAngularSpeed;
			}
			Float_2 = Mathf.Min(Float_2, 1.5f);
		}
		else
		{
			MovementContext.PlayerAnimatorSetApproached(b: true);
			Bool_3 = true;
		}
		if (!Bool_3)
		{
			if (Bool_1 || Bool_2)
			{
				MovementContext.LevelOnApproachStart = MovementContext.PoseLevel;
			}
			Float_0 = 0f;
		}
	}

	public void method_0(float stateTime, float dt)
	{
		float t = ((Float_2 > 0f) ? Mathf.Clamp01(ApproachCurve.Evaluate(stateTime / Float_2)) : 1f);
		MovementContext.ApplyRotation(Quaternion.Euler(0f, Mathf.LerpAngle(Float_3, Vector2_1.x, t), 0f));
		if (Bool_1)
		{
			MovementContext.SetPoseLevel(Mathf.Lerp(Float_4, 1f, t), force: true);
		}
		else if (Bool_2)
		{
			MovementContext.SetPoseLevel(Mathf.Lerp(Float_4, 0f, t), force: true);
		}
		Vector2 rotation = new Vector2(Mathf.LerpAngle(Vector2_0.x, Vector2_1.x, t), Mathf.LerpAngle(Vector2_0.y, Vector2_1.y, t));
		MovementContext.Rotation = rotation;
		Vector3 motion = Vector3.Lerp(Vector3_2, Vector3_0, t) - MovementContext.TransformPosition;
		if (motion.sqrMagnitude > 0f)
		{
			MovementContext.ApplyApproachMotion(motion, dt);
		}
		Vector3_3 = Vector3.Lerp(Vector3_3, MovementContext.InverseTransformVector(motion.normalized), 0.1f);
		MovementContext.MovementDirection = new Vector2(Vector3_3.x, Vector3_3.z);
		MovementContext.LeftStanceController.DisableLeftStanceAnimFromBodyAction();
	}

	public override void Prone()
	{
	}

	public override void ManualAnimatorMoveUpdate(float deltaTime)
	{
		Float_0 = Time.time - Float_1;
		if (Bool_0)
		{
			method_0(Float_0, deltaTime);
		}
		else if (Bool_1)
		{
			MovementContext.SetPoseLevel(1f);
		}
		else if (Bool_2)
		{
			MovementContext.SetPoseLevel(0f);
		}
		if (Float_0 > Float_2)
		{
			MovementContext.PlayerAnimatorSetApproached(b: true);
			Bool_3 = true;
		}
	}

	public override void Exit(bool toSameState)
	{
		base.Exit(toSameState);
		MovementContext.StateLocksInventory = false;
		Bool_3 = false;
		MovementContext.PlayerAnimatorSetApproached(b: false);
	}

	public override void BlindFire(int b)
	{
	}
}
