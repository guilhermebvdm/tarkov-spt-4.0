using System;
using System.Runtime.CompilerServices;
using System.Threading;
using EFT;
using EFT.Interactive;
using UnityEngine;

public class GClass782 : ICharacterController
{
	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public CapsuleCollider CapsuleCollider_0;

	[NonSerialized]
	public IPlayer Iplayer_0;

	[NonSerialized]
	public Rigidbody Rigidbody_0;

	[NonSerialized]
	[CompilerGenerated]
	public CollisionFlags CollisionFlags_0;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_1;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_2;

	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_1;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_2;

	[NonSerialized]
	public Vector3 Vector3_0;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_3 = -1f;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_3;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_4;

	[CompilerGenerated]
	private Action<float> action_0;

	[NonSerialized]
	public LayerMask LayerMask_0;

	[NonSerialized]
	public GClass736.GClass737 Gclass737_0;

	[NonSerialized]
	public bool Bool_5 = true;

	[NonSerialized]
	public Vector3 Vector3_1;

	[NonSerialized]
	public Vector3 Vector3_2;

	public bool isEnabled
	{
		get
		{
			return CapsuleCollider_0.enabled;
		}
		set
		{
			CapsuleCollider_0.enabled = value;
		}
	}

	public Vector3 center
	{
		get
		{
			return CapsuleCollider_0.center;
		}
		set
		{
			CapsuleCollider_0.center = value;
		}
	}

	public CollisionFlags collisionFlags
	{
		[CompilerGenerated]
		get
		{
			return CollisionFlags_0;
		}
		[CompilerGenerated]
		set
		{
			CollisionFlags_0 = value;
		}
	}

	public bool detectCollisions
	{
		[CompilerGenerated]
		get
		{
			return Bool_1;
		}
		[CompilerGenerated]
		set
		{
			Bool_1 = value;
		}
	}

	public float height
	{
		get
		{
			return CapsuleCollider_0.height;
		}
		set
		{
			CapsuleCollider_0.height = value;
			if (action_0 != null)
			{
				action_0(value);
			}
		}
	}

	public bool isGrounded
	{
		[CompilerGenerated]
		get
		{
			return Bool_2;
		}
		[CompilerGenerated]
		set
		{
			Bool_2 = value;
		}
	}

	public float skinWidth
	{
		get
		{
			return Float_0;
		}
		set
		{
			Float_0 = value;
		}
	}

	public float radius
	{
		get
		{
			return CapsuleCollider_0.radius;
		}
		set
		{
			CapsuleCollider_0.radius = value;
		}
	}

	public float slopeLimit
	{
		[CompilerGenerated]
		get
		{
			return Float_1;
		}
		[CompilerGenerated]
		set
		{
			Float_1 = value;
		}
	}

	public float stepOffset
	{
		[CompilerGenerated]
		get
		{
			return Float_2;
		}
		[CompilerGenerated]
		set
		{
			Float_2 = value;
		}
	}

	public Vector3 velocity
	{
		get
		{
			return Vector3_0;
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	public Rigidbody attachedRigidbody
	{
		get
		{
			return Rigidbody_0;
		}
		set
		{
			Rigidbody_0 = value;
		}
	}

	public Bounds bounds
	{
		get
		{
			return CapsuleCollider_0.bounds;
		}
		set
		{
		}
	}

	public float contactOffset
	{
		get
		{
			return CapsuleCollider_0.contactOffset;
		}
		set
		{
			CapsuleCollider_0.contactOffset = value;
		}
	}

	public virtual float _distanceRaycast => 0f;

	public bool SupportDepenetration => false;

	public virtual bool UseAsyncMoveValidation => false;

	public float SpeedLimit
	{
		[CompilerGenerated]
		get
		{
			return Float_3;
		}
		[CompilerGenerated]
		set
		{
			Float_3 = value;
		}
	}

	public bool IsSpeedLimitWasEnabledAtTheFrame
	{
		[CompilerGenerated]
		get
		{
			return Bool_3;
		}
		[CompilerGenerated]
		set
		{
			Bool_3 = value;
		}
	}

	public bool IsMoveIgnored
	{
		[CompilerGenerated]
		get
		{
			return Bool_4;
		}
		[CompilerGenerated]
		set
		{
			Bool_4 = value;
		}
	}

	public virtual bool ShouldStickToGround => true;

	public event Action<float> OnHeightChanged
	{
		[CompilerGenerated]
		add
		{
			Action<float> action = action_0;
			Action<float> action2;
			do
			{
				action2 = action;
				Action<float> value2 = (Action<float>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<float> action = action_0;
			Action<float> action2;
			do
			{
				action2 = action;
				Action<float> value2 = (Action<float>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public virtual void SetSteerDirection(Vector3 steerDirection)
	{
	}

	public GClass782(bool performMovementValidation, CapsuleCollider collider, Rigidbody rigidbody, IPlayer player, float skinWidth, LayerMask maskToValidateMove)
	{
		LayerMask_0 = maskToValidateMove;
		PerformMovementValidation(performMovementValidation);
		CapsuleCollider_0 = collider;
		Rigidbody_0 = rigidbody;
		Iplayer_0 = player;
		Float_0 = skinWidth;
		Gclass737_0 = GClass736.Instance.CreateContainerForBatch("AnimationCallbackBatch");
	}

	public void Dispose()
	{
		Gclass737_0.Dispose();
	}

	public void PerformMovementValidation(bool performMovementValidation)
	{
		Bool_0 = performMovementValidation;
	}

	public virtual CollisionFlags Move(Vector3 motion, float deltaTime)
	{
		Vector3_1 = CapsuleCollider_0.transform.position;
		Vector3_2 = motion;
		_ = CapsuleCollider_0.transform.position;
		if (Bool_0 && !method_0(Vector3_1, motion))
		{
			return CollisionFlags.Sides;
		}
		return UpdateDelayedMoveLogic();
	}

	public bool method_0(Vector3 currentPoint, Vector3 motion)
	{
		if (Physics.Raycast(currentPoint + CapsuleCollider_0.center, maxDistance: motion.magnitude + _distanceRaycast, direction: motion, hitInfo: out var hitInfo, layerMask: LayerMask_0, queryTriggerInteraction: QueryTriggerInteraction.Ignore))
		{
			if (!method_2(hitInfo) && !EFTPhysicsClass.GetIgnoreCollision(CapsuleCollider_0, hitInfo.collider))
			{
				return false;
			}
			return true;
		}
		return true;
	}

	public void method_1(Vector3 currentPoint, Vector3 motion)
	{
		Vector3 vector = currentPoint + CapsuleCollider_0.center;
		float distance = motion.magnitude + _distanceRaycast;
		Gclass737_0.SetNewCommand(new RaycastCommand(vector, motion, distance, LayerMask_0));
		Bool_5 = true;
	}

	public CollisionFlags UpdateDelayedMoveLogic()
	{
		if (Bool_5)
		{
			Bool_5 = false;
			bool flag = true;
			if (Gclass737_0.ResultIsActual && Gclass737_0.Result.collider != null)
			{
				RaycastHit result = Gclass737_0.Result;
				if (!method_2(result) && !EFTPhysicsClass.GetIgnoreCollision(CapsuleCollider_0, result.collider))
				{
					flag = false;
				}
			}
			if (!flag)
			{
				return CollisionFlags.Sides;
			}
		}
		Vector3 vector = Vector3_1 + Vector3_2;
		if (method_3())
		{
			Rigidbody_0.MovePosition(vector);
		}
		else
		{
			Iplayer_0.PlayerBones.BodyTransform.position = vector;
		}
		method_4(Vector3_1, vector, Time.deltaTime);
		return CollisionFlags.None;
	}

	public bool method_2(RaycastHit hitInfo)
	{
		if (hitInfo.collider.gameObject.layer == LayerMaskClass.DoorLayer)
		{
			WorldInteractiveObject componentInParent = hitInfo.collider.gameObject.GetComponentInParent<WorldInteractiveObject>();
			if (componentInParent != null && (componentInParent.DoorState == EDoorState.Open || componentInParent.DoorState == EDoorState.Interacting))
			{
				return true;
			}
		}
		return false;
	}

	public bool method_3()
	{
		if (Rigidbody_0 != null)
		{
			return EFTPhysicsClass.SyncTransformsClass.UpdateControlledByUnity;
		}
		return false;
	}

	public bool SimpleMove(Vector3 motion)
	{
		throw new NotImplementedException();
	}

	public void CopyFields(ICharacterController characterController)
	{
		GClass779.CopyFields(this, characterController);
	}

	public void CopyFields(CharacterStruct footprint)
	{
		GClass779.CopyFields(this, footprint);
	}

	public CharacterStruct GetFootprint()
	{
		return GClass779.GetFootprint(this);
	}

	public Collider GetCollider()
	{
		return CapsuleCollider_0;
	}

	public void method_4(Vector3 currentPoint, Vector3 destinationPoint, float deltaTime)
	{
		if (method_3())
		{
			Vector3_0 = Rigidbody_0.velocity;
		}
		else
		{
			Vector3_0 = (destinationPoint - currentPoint) / deltaTime;
		}
	}

	public void SetEnableVerticalDepenetration(bool enable)
	{
	}
}
