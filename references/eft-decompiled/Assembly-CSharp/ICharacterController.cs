using System;
using UnityEngine;

public interface ICharacterController
{
	bool isEnabled { get; set; }

	Vector3 center { get; set; }

	CollisionFlags collisionFlags { get; }

	bool detectCollisions { get; set; }

	float height { get; set; }

	bool isGrounded { get; }

	float skinWidth { get; set; }

	float radius { get; set; }

	float slopeLimit { get; set; }

	float stepOffset { get; set; }

	Vector3 velocity { get; }

	Rigidbody attachedRigidbody { get; }

	Bounds bounds { get; }

	float contactOffset { get; set; }

	bool ShouldStickToGround { get; }

	bool SupportDepenetration { get; }

	float SpeedLimit { get; set; }

	bool IsSpeedLimitWasEnabledAtTheFrame { get; set; }

	event Action<float> OnHeightChanged;

	CollisionFlags Move(Vector3 motion, float deltaTime);

	bool SimpleMove(Vector3 motion);

	void CopyFields(ICharacterController characterController);

	void CopyFields(CharacterStruct footprint);

	CharacterStruct GetFootprint();

	Collider GetCollider();

	void SetSteerDirection(Vector3 steerDirection);

	void SetEnableVerticalDepenetration(bool enable);
}
