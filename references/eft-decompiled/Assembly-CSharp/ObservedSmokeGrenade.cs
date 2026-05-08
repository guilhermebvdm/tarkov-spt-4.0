using EFT;

public class ObservedSmokeGrenade : SmokeGrenade
{
	public override float PhysicsQuality => Grenade.PhysicsQualityForObserved;

	public override GClass833 GetVisibilityChecker()
	{
		return Grenade.GetVisibilityCheckerForObserved(this);
	}

	public override void OnDoneFromNet()
	{
		RemoveRigidbody();
	}

	public override void RemoveRigidBodyOnVelocityDrop(Throwable grenade)
	{
	}
}
