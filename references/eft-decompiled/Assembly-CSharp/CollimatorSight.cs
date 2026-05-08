using System;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

public class CollimatorSight : MonoBehaviour
{
	[CompilerGenerated]
	private static Action<CollimatorSight> action_0;

	[CompilerGenerated]
	private static Action<CollimatorSight> action_1;

	[CompilerGenerated]
	private static Action<CollimatorSight> action_2;

	private static readonly Quaternion quaternion_0 = Quaternion.Euler(-90f, 0f, 0f);

	public MeshRenderer CollimatorMeshRenderer;

	public Material CollimatorMaterial;

	public static event Action<CollimatorSight> OnCollimatorEnabled
	{
		[CompilerGenerated]
		add
		{
			Action<CollimatorSight> action = action_0;
			Action<CollimatorSight> action2;
			do
			{
				action2 = action;
				Action<CollimatorSight> value2 = (Action<CollimatorSight>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<CollimatorSight> action = action_0;
			Action<CollimatorSight> action2;
			do
			{
				action2 = action;
				Action<CollimatorSight> value2 = (Action<CollimatorSight>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public static event Action<CollimatorSight> OnCollimatorDisabled
	{
		[CompilerGenerated]
		add
		{
			Action<CollimatorSight> action = action_1;
			Action<CollimatorSight> action2;
			do
			{
				action2 = action;
				Action<CollimatorSight> value2 = (Action<CollimatorSight>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<CollimatorSight> action = action_1;
			Action<CollimatorSight> action2;
			do
			{
				action2 = action;
				Action<CollimatorSight> value2 = (Action<CollimatorSight>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public static event Action<CollimatorSight> OnCollimatorUpdated
	{
		[CompilerGenerated]
		add
		{
			Action<CollimatorSight> action = action_2;
			Action<CollimatorSight> action2;
			do
			{
				action2 = action;
				Action<CollimatorSight> value2 = (Action<CollimatorSight>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_2, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<CollimatorSight> action = action_2;
			Action<CollimatorSight> action2;
			do
			{
				action2 = action;
				Action<CollimatorSight> value2 = (Action<CollimatorSight>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_2, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public void Awake()
	{
		CollimatorMeshRenderer = GetComponent<MeshRenderer>();
		CollimatorMaterial = CollimatorMeshRenderer.sharedMaterial;
	}

	public void OnEnable()
	{
		action_0?.Invoke(this);
	}

	public void OnDisable()
	{
		action_1?.Invoke(this);
	}

	public void LookAt(Vector3 point, Vector3 worldUp)
	{
		base.transform.LookAt(point, worldUp);
		base.transform.localRotation *= quaternion_0;
		action_2?.Invoke(this);
	}
}
