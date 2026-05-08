using System;
using System.Collections.Generic;
using Comfort.Common;
using UnityEngine;

public abstract class ComponentSystem<T, TS> : MonoBehaviour, GInterface32<T> where T : GInterface31 where TS : ComponentSystem<T, TS>
{
	protected List<T> Components;

	private int int_0;

	public abstract bool HasUpdate { get; }

	public abstract bool HasLateUpdate { get; }

	public static void Register(GameObject globalDotNotDestroy)
	{
		Singleton<GInterface32<T>>.Create(globalDotNotDestroy.AddComponent<TS>());
	}

	public virtual void Awake()
	{
		Components = new List<T>(64);
	}

	public virtual void Update()
	{
		if (!HasUpdate)
		{
			return;
		}
		for (int i = 0; i < int_0; i++)
		{
			try
			{
				UpdateComponent(Components[i]);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	public virtual void LateUpdate()
	{
		if (!HasLateUpdate)
		{
			return;
		}
		for (int i = 0; i < int_0; i++)
		{
			try
			{
				LateUpdateComponent(Components[i]);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	public abstract void UpdateComponent(T component);

	public abstract void LateUpdateComponent(T component);

	public virtual void Register(T component)
	{
		Components.Add(component);
		int_0 = Components.Count;
	}

	public virtual void Unregister(T component)
	{
		Components.Remove(component);
		int_0 = Components.Count;
	}

	public ComponentSystem()
	{
	}
}
