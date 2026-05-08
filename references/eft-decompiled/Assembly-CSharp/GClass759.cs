using System;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

public class GClass759 : ObjectInHandsAnimator
{
	[CompilerGenerated]
	private Action action_0;

	public event Action FireEvent
	{
		[CompilerGenerated]
		add
		{
			Action action = action_0;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action action = action_0;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public void SetFire(bool fire)
	{
		WeaponAnimationSpeedControllerClass.SetFire(base.Animator, fire);
	}

	public void SetAltFire(bool altFire)
	{
		WeaponAnimationSpeedControllerClass.SetAltFire(base.Animator, altFire);
	}

	public void SetIdleVar(float idleVar)
	{
		WeaponAnimationSpeedControllerClass.SetIdleVar(base.Animator, idleVar);
	}

	public void SetFireVar(int fireVarCount)
	{
		WeaponAnimationSpeedControllerClass.SetFireVar(base.Animator, fireVarCount);
	}

	public void SetActive(bool active)
	{
		WeaponAnimationSpeedControllerClass.SetActive(base.Animator, active);
	}

	public void Fire()
	{
		Debug.Log("FUCK");
		if (action_0 != null)
		{
			action_0();
		}
	}

	public void AltFire()
	{
		if (action_0 != null)
		{
			action_0();
		}
	}

	public void QuickFire()
	{
		if (action_0 != null)
		{
			action_0();
		}
	}
}
