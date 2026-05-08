using EFT;
using UnityEngine;

public class ProneAIMoveStateClass : ProneMoveStateClass
{
	public ProneAIMoveStateClass(MovementContext movementContext)
		: base(movementContext)
	{
	}

	public override void LimitMotion(ref Vector3 motion, float deltaTime)
	{
		MovementContext.LimitMotionXZ(ref motion, deltaTime);
	}

	public override void Rotate(Vector2 deltaRotation, bool ignoreClamp = false)
	{
		if (!ignoreClamp)
		{
			deltaRotation = ClampRotation(deltaRotation);
		}
		MovementContext.Rotation += deltaRotation;
	}
}
