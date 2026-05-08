using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public abstract class GClass809
{
	public static bool SetEnabledUniversal(this Component component, bool isEnabled)
	{
		if (component is Behaviour)
		{
			Behaviour behaviour = (Behaviour)component;
			if (behaviour.enabled != isEnabled)
			{
				behaviour.enabled = isEnabled;
				return true;
			}
			return false;
		}
		if (component is Renderer)
		{
			Renderer renderer = (Renderer)component;
			if (renderer.enabled != isEnabled)
			{
				renderer.enabled = isEnabled;
				return true;
			}
			return false;
		}
		if (component is LODGroup)
		{
			LODGroup lODGroup = (LODGroup)component;
			if (lODGroup.enabled != isEnabled)
			{
				lODGroup.enabled = isEnabled;
				return true;
			}
			return false;
		}
		if (component is Collider)
		{
			Collider collider = (Collider)component;
			if (collider.enabled != isEnabled)
			{
				collider.enabled = isEnabled;
				return true;
			}
			return false;
		}
		if (component is ParticleSystem)
		{
			ParticleSystem particleSystem = (ParticleSystem)component;
			if (particleSystem.isPlaying != isEnabled)
			{
				if (isEnabled)
				{
					particleSystem.Play(withChildren: true);
				}
				else
				{
					particleSystem.Pause(withChildren: true);
				}
				return true;
			}
			return false;
		}
		return false;
	}

	public static bool IsEnabledUniversal(this Component component)
	{
		if (component is Behaviour)
		{
			return ((Behaviour)component).enabled;
		}
		if (component is Renderer)
		{
			return ((Renderer)component).enabled;
		}
		if (component is LODGroup)
		{
			return ((LODGroup)component).enabled;
		}
		if (component is Collider)
		{
			return ((Collider)component).enabled;
		}
		if (component is ParticleSystem)
		{
			return ((ParticleSystem)component).isPlaying;
		}
		return false;
	}

	public static void SetEnabledUniversal(this List<Component> components, bool isEnabled)
	{
		for (int i = 0; i < components.Count; i++)
		{
			SetEnabledUniversal(components[i], isEnabled);
		}
	}

	public static void CopyComponentValues<T>(this Component target, T source) where T : Component
	{
		PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (PropertyInfo propertyInfo in properties)
		{
			if (propertyInfo.CanWrite)
			{
				try
				{
					propertyInfo.SetValue(target, propertyInfo.GetValue(source, null), null);
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
			}
		}
		FieldInfo[] fields = typeof(T).GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (FieldInfo fieldInfo in fields)
		{
			fieldInfo.SetValue(target, fieldInfo.GetValue(source));
		}
	}
}
