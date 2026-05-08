using UnityEngine;

public class HideoutGameWorld : ClientLocalGameWorld
{
	public override void BallisticsTick(float dt)
	{
		if (base.SharedBallisticsCalculator != null && base.SharedBallisticsCalculator.ActiveShotsCount > 0)
		{
			Physics.SyncTransforms();
		}
		base.BallisticsTick(dt);
	}

	public override void vmethod_1(float dt)
	{
	}
}
