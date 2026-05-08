using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

public class GClass875
{
	public struct Struct92
	{
		public GameObject go;

		[CompilerGenerated]
		private Action<GameObject> action_0;

		[CompilerGenerated]
		private Action action_1;

		public event Action<GameObject> OnAdd
		{
			[CompilerGenerated]
			add
			{
				Action<GameObject> action = action_0;
				Action<GameObject> action2;
				do
				{
					action2 = action;
					Action<GameObject> value2 = (Action<GameObject>)Delegate.Combine(action2, value);
					action = Interlocked.CompareExchange(ref action_0, value2, action2);
				}
				while ((object)action != action2);
			}
			[CompilerGenerated]
			remove
			{
				Action<GameObject> action = action_0;
				Action<GameObject> action2;
				do
				{
					action2 = action;
					Action<GameObject> value2 = (Action<GameObject>)Delegate.Remove(action2, value);
					action = Interlocked.CompareExchange(ref action_0, value2, action2);
				}
				while ((object)action != action2);
			}
		}

		public event Action OnRemove
		{
			[CompilerGenerated]
			add
			{
				Action action = action_1;
				Action action2;
				do
				{
					action2 = action;
					Action value2 = (Action)Delegate.Combine(action2, value);
					action = Interlocked.CompareExchange(ref action_1, value2, action2);
				}
				while ((object)action != action2);
			}
			[CompilerGenerated]
			remove
			{
				Action action = action_1;
				Action action2;
				do
				{
					action2 = action;
					Action value2 = (Action)Delegate.Remove(action2, value);
					action = Interlocked.CompareExchange(ref action_1, value2, action2);
				}
				while ((object)action != action2);
			}
		}

		public Struct92(GuidComponent comp)
		{
			go = comp.gameObject;
			action_1 = null;
			action_0 = null;
		}

		public void HandleAddCallback()
		{
			if (action_0 != null)
			{
				action_0(go);
			}
		}

		public void HandleRemoveCallback()
		{
			if (action_1 != null)
			{
				action_1();
			}
		}
	}

	[NonSerialized]
	public static GClass875 Gclass875_0;

	[NonSerialized]
	public Dictionary<Guid, Struct92> Dictionary_0;

	public static bool Add(GuidComponent guidComponent)
	{
		if (Gclass875_0 == null)
		{
			Gclass875_0 = new GClass875();
		}
		return Gclass875_0.method_0(guidComponent);
	}

	public static void Remove(Guid guid)
	{
		if (Gclass875_0 == null)
		{
			Gclass875_0 = new GClass875();
		}
		Gclass875_0.method_1(guid);
	}

	public static GameObject ResolveGuid(Guid guid, Action<GameObject> onAddCallback, Action onRemoveCallback)
	{
		if (Gclass875_0 == null)
		{
			Gclass875_0 = new GClass875();
		}
		return Gclass875_0.method_2(guid, onAddCallback, onRemoveCallback);
	}

	public static GameObject ResolveGuid(Guid guid, Action onDestroyCallback)
	{
		if (Gclass875_0 == null)
		{
			Gclass875_0 = new GClass875();
		}
		return Gclass875_0.method_2(guid, null, onDestroyCallback);
	}

	public static GameObject ResolveGuid(Guid guid)
	{
		if (Gclass875_0 == null)
		{
			Gclass875_0 = new GClass875();
		}
		return Gclass875_0.method_2(guid, null, null);
	}

	public GClass875()
	{
		Dictionary_0 = new Dictionary<Guid, Struct92>();
	}

	public bool method_0(GuidComponent guidComponent)
	{
		Guid guid = guidComponent.GetGuid();
		Struct92 value = new Struct92(guidComponent);
		if (!Dictionary_0.ContainsKey(guid))
		{
			Dictionary_0.Add(guid, value);
			return true;
		}
		Struct92 value2 = Dictionary_0[guid];
		if (value2.go != null && value2.go != guidComponent.gameObject)
		{
			if (!Application.isPlaying)
			{
				Debug.LogWarningFormat(guidComponent, "Guid Collision Detected while creating {0}.\nAssigning new Guid.", (guidComponent != null) ? guidComponent.name : "NULL");
			}
			return false;
		}
		value2.go = value.go;
		value2.HandleAddCallback();
		Dictionary_0[guid] = value2;
		return true;
	}

	public void method_1(Guid guid)
	{
		if (Dictionary_0.TryGetValue(guid, out var value))
		{
			value.HandleRemoveCallback();
		}
		Dictionary_0.Remove(guid);
	}

	public GameObject method_2(Guid guid, Action<GameObject> onAddCallback, Action onRemoveCallback)
	{
		if (Dictionary_0.TryGetValue(guid, out var value))
		{
			if (onAddCallback != null)
			{
				value.OnAdd += onAddCallback;
			}
			if (onRemoveCallback != null)
			{
				value.OnRemove += onRemoveCallback;
			}
			Dictionary_0[guid] = value;
			return value.go;
		}
		if (onAddCallback != null)
		{
			value.OnAdd += onAddCallback;
		}
		if (onRemoveCallback != null)
		{
			value.OnRemove += onRemoveCallback;
		}
		Dictionary_0.Add(guid, value);
		return null;
	}
}
