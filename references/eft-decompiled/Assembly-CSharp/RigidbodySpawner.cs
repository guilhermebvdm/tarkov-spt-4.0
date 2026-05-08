using System;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

[DisallowMultipleComponent]
public class RigidbodySpawner : MonoBehaviour
{
	public float mass;

	public float drag;

	public float angularDrag;

	public bool useGravity;

	public bool isKinematic;

	public RigidbodyInterpolation interpolation;

	public CollisionDetectionMode collisionDetectionMode;

	[GAttribute10(typeof(RigidbodyConstraints))]
	public RigidbodyConstraints constraints;

	private Rigidbody rigidbody_0;

	[CompilerGenerated]
	private Action<RigidbodySpawner, Rigidbody> action_0;

	[CompilerGenerated]
	private Action<RigidbodySpawner> action_1;

	public Rigidbody Rigidbody => rigidbody_0;

	public event Action<RigidbodySpawner, Rigidbody> SpawnEvent
	{
		[CompilerGenerated]
		add
		{
			Action<RigidbodySpawner, Rigidbody> action = action_0;
			Action<RigidbodySpawner, Rigidbody> action2;
			do
			{
				action2 = action;
				Action<RigidbodySpawner, Rigidbody> value2 = (Action<RigidbodySpawner, Rigidbody>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<RigidbodySpawner, Rigidbody> action = action_0;
			Action<RigidbodySpawner, Rigidbody> action2;
			do
			{
				action2 = action;
				Action<RigidbodySpawner, Rigidbody> value2 = (Action<RigidbodySpawner, Rigidbody>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<RigidbodySpawner> RemoveEvent
	{
		[CompilerGenerated]
		add
		{
			Action<RigidbodySpawner> action = action_1;
			Action<RigidbodySpawner> action2;
			do
			{
				action2 = action;
				Action<RigidbodySpawner> value2 = (Action<RigidbodySpawner>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<RigidbodySpawner> action = action_1;
			Action<RigidbodySpawner> action2;
			do
			{
				action2 = action;
				Action<RigidbodySpawner> value2 = (Action<RigidbodySpawner>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public Rigidbody Create()
	{
		if (rigidbody_0 == null)
		{
			rigidbody_0 = base.gameObject.AddComponent<Rigidbody>();
			rigidbody_0.mass = mass;
			rigidbody_0.drag = drag;
			rigidbody_0.angularDrag = angularDrag;
			rigidbody_0.useGravity = useGravity;
			rigidbody_0.isKinematic = isKinematic;
			rigidbody_0.interpolation = interpolation;
			rigidbody_0.collisionDetectionMode = collisionDetectionMode;
			rigidbody_0.constraints = constraints;
		}
		if (action_0 != null)
		{
			action_0(this, rigidbody_0);
		}
		return rigidbody_0;
	}

	public void Remove()
	{
		if (rigidbody_0 != null)
		{
			UnityEngine.Object.Destroy(rigidbody_0);
			rigidbody_0 = null;
			if (action_1 != null)
			{
				action_1(this);
			}
		}
	}
}
