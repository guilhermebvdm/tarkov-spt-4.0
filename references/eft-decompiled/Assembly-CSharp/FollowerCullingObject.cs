using System;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

[ExecuteInEditMode]
public class FollowerCullingObject : CullingObject
{
	[CompilerGenerated]
	private Action<bool> action_0;

	private Func<Transform> func_0;

	public event Action<bool> OnVisibilityChanged
	{
		[CompilerGenerated]
		add
		{
			Action<bool> action = action_0;
			Action<bool> action2;
			do
			{
				action2 = action;
				Action<bool> value2 = (Action<bool>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<bool> action = action_0;
			Action<bool> action2;
			do
			{
				action2 = action;
				Action<bool> value2 = (Action<bool>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public void Init(Func<Transform> transformGetter)
	{
		func_0 = transformGetter;
	}

	public void SetParams(float radius, Vector3 shift, float cullDistance)
	{
		base.Radius = radius;
		base.Shift = shift;
		CullDistance = cullDistance;
		if (CullingManager.Instance != null)
		{
			CullingManager.Instance.UpdateSphere(this);
		}
	}

	public override Transform GetTransform()
	{
		return func_0();
	}

	public override void SetVisibility(bool isVisible)
	{
		if (base.IsVisible != isVisible)
		{
			base.SetVisibility(isVisible);
			action_0?.Invoke(isVisible);
			if (CullingManager.Instance != null)
			{
				CullingManager.Instance.UpdateSphere(this);
			}
		}
	}
}
