using EFT;
using UnityEngine;

public class ObservedGrenade : Grenade
{
	public override float PhysicsQuality => Grenade.PhysicsQualityForObserved;

	public override GClass833 GetVisibilityChecker()
	{
		return Grenade.GetVisibilityCheckerForObserved(this);
	}

	public override void StartTimer()
	{
		_behaviourTimerCoroutine = GClass7.StartBehaviourTimer(this, base.WeaponSource.GetExplDelay * 3f, method_7);
	}

	public void method_7()
	{
		Debug.LogWarning("Grenade expired by time");
		Object.Destroy(base.gameObject);
	}

	public override void OnDoneFromNet()
	{
		base.OnDoneFromNet();
		InvokeBlowUpEvent();
	}

	public override void ProcessContactExplodeCollision(float impulse)
	{
	}
}
