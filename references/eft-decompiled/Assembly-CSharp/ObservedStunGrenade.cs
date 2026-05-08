using EFT;
using UnityEngine;

public class ObservedStunGrenade : StunGrenade
{
	public override float PhysicsQuality => Grenade.PhysicsQualityForObserved;

	public override GClass833 GetVisibilityChecker()
	{
		return Grenade.GetVisibilityCheckerForObserved(this);
	}

	public override void StartTimer()
	{
		_behaviourTimerCoroutine = GClass7.StartBehaviourTimer(this, base.WeaponSource.GetExplDelay * 2f, method_7);
	}

	public void method_7()
	{
		Object.Destroy(base.gameObject);
	}

	public override void OnDoneFromNet()
	{
		InvokeBlowUpEvent();
	}
}
