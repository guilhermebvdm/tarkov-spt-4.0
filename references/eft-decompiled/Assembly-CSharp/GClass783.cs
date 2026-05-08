using System;
using EFT;
using UnityEngine;

public class GClass783 : GClass782
{
	[NonSerialized]
	public Vector3 Vector3_3;

	[NonSerialized]
	public const float Float_4 = 0.3f;

	public override float _distanceRaycast => 0.3f;

	public override bool ShouldStickToGround => false;

	public override bool UseAsyncMoveValidation => false;

	public GClass783(bool performMovementValidation, CapsuleCollider collider, Rigidbody rigidbody, IPlayer player, float skinWidth, LayerMask collisionMask)
		: base(performMovementValidation, collider, rigidbody, player, skinWidth, collisionMask)
	{
	}

	public override CollisionFlags Move(Vector3 motion, float deltaTime)
	{
		float num = Vector3.Dot(motion, Vector3_3);
		motion = ((!(num > 0f)) ? Vector3.zero : (Vector3_3 * num));
		return base.Move(motion, deltaTime);
	}

	public override void SetSteerDirection(Vector3 steerDirection)
	{
		base.SetSteerDirection(steerDirection);
		Vector3_3 = steerDirection;
	}
}
