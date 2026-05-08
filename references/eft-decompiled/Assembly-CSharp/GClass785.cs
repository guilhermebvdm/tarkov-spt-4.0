using System;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

public class GClass785 : ICharacterController
{
	[NonSerialized]
	public CharacterController CharacterController_0;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_0;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_0;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_1;

	[CompilerGenerated]
	private Action<float> action_0;

	public bool SupportDepenetration => true;

	public float SpeedLimit
	{
		[CompilerGenerated]
		get
		{
			return Float_0;
		}
		[CompilerGenerated]
		set
		{
			Float_0 = value;
		}
	}

	public bool IsSpeedLimitWasEnabledAtTheFrame
	{
		[CompilerGenerated]
		get
		{
			return Bool_0;
		}
		[CompilerGenerated]
		set
		{
			Bool_0 = value;
		}
	}

	public bool IsMoveIgnored
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

	public bool isEnabled
	{
		get
		{
			return CharacterController_0.enabled;
		}
		set
		{
			CharacterController_0.enabled = value;
		}
	}

	public Vector3 center
	{
		get
		{
			return CharacterController_0.center;
		}
		set
		{
			CharacterController_0.center = value;
		}
	}

	public CollisionFlags collisionFlags => CharacterController_0.collisionFlags;

	public bool detectCollisions
	{
		get
		{
			return CharacterController_0.detectCollisions;
		}
		set
		{
			CharacterController_0.detectCollisions = value;
		}
	}

	public float height
	{
		get
		{
			return CharacterController_0.height;
		}
		set
		{
			CharacterController_0.height = value;
			if (action_0 != null)
			{
				action_0(value);
			}
		}
	}

	public bool isGrounded => CharacterController_0.isGrounded;

	public float skinWidth
	{
		get
		{
			return CharacterController_0.skinWidth;
		}
		set
		{
			CharacterController_0.skinWidth = value;
		}
	}

	public float radius
	{
		get
		{
			return CharacterController_0.radius;
		}
		set
		{
			CharacterController_0.radius = value;
		}
	}

	public float slopeLimit
	{
		get
		{
			return CharacterController_0.slopeLimit;
		}
		set
		{
			CharacterController_0.slopeLimit = value;
		}
	}

	public float stepOffset
	{
		get
		{
			return CharacterController_0.stepOffset;
		}
		set
		{
			CharacterController_0.stepOffset = value;
		}
	}

	public Vector3 velocity => CharacterController_0.velocity;

	public Rigidbody attachedRigidbody => CharacterController_0.attachedRigidbody;

	public Bounds bounds => CharacterController_0.bounds;

	public float contactOffset
	{
		get
		{
			return CharacterController_0.stepOffset;
		}
		set
		{
			CharacterController_0.stepOffset = value;
		}
	}

	public bool ShouldStickToGround => true;

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

	public void SetSteerDirection(Vector3 steerDirection)
	{
	}

	public GClass785(CharacterController cc)
	{
		CharacterController_0 = cc;
	}

	public CollisionFlags Move(Vector3 motion, float deltaTime)
	{
		return CharacterController_0.Move(motion);
	}

	public bool SimpleMove(Vector3 motion)
	{
		return CharacterController_0.SimpleMove(motion);
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
		return CharacterController_0;
	}

	public void SetEnableVerticalDepenetration(bool enable)
	{
	}
}
