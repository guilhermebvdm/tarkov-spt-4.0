using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

public abstract class AICoreStrategyAbstractClass<T>
{
	[NonSerialized]
	public Dictionary<int, global::AICoreLayerClass<T>> Dictionary_0 = new Dictionary<int, global::AICoreLayerClass<T>>();

	[NonSerialized]
	public List<global::AICoreLayerClass<T>> List_0 = new List<global::AICoreLayerClass<T>>();

	[NonSerialized]
	public GClass32 Gclass32_0;

	[NonSerialized]
	[CompilerGenerated]
	public Action<global::AICoreLayerClass<T>> Action_0;

	[NonSerialized]
	[CompilerGenerated]
	public global::AICoreLayerClass<T> Gclass35_0;

	public global::AICoreLayerClass<T> GClass35_0
	{
		[CompilerGenerated]
		get
		{
			return Gclass35_0;
		}
		[CompilerGenerated]
		set
		{
			Gclass35_0 = value;
		}
	}

	public event Action<global::AICoreLayerClass<T>> OnLayerChangedTo
	{
		[CompilerGenerated]
		add
		{
			Action<global::AICoreLayerClass<T>> action = Action_0;
			Action<global::AICoreLayerClass<T>> action2;
			do
			{
				action2 = action;
				Action<global::AICoreLayerClass<T>> value2 = (Action<global::AICoreLayerClass<T>>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref Action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<global::AICoreLayerClass<T>> action = Action_0;
			Action<global::AICoreLayerClass<T>> action2;
			do
			{
				action2 = action;
				Action<global::AICoreLayerClass<T>> value2 = (Action<global::AICoreLayerClass<T>>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref Action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public void Activate(GClass32 coreAgent)
	{
		Gclass32_0 = coreAgent;
		if (Dictionary_0.Count == 0)
		{
			Debug.LogError($"Blocker error. {this} have NO layers at strategy");
		}
		if (List_0.Count == 0)
		{
			Debug.LogError($"Blocker error. {this} have NO ACTIVE layers");
		}
		foreach (KeyValuePair<int, global::AICoreLayerClass<T>> item in Dictionary_0)
		{
			Gclass32_0.AddLayer(item.Key, item.Value.Name(), item.Value.Priority);
		}
		foreach (global::AICoreLayerClass<T> item2 in List_0)
		{
			Gclass32_0.ActivateLayer(item2.Name());
		}
	}

	public global::AICoreActionResultStruct<T, GClass26>? Update(global::AICoreActionResultStruct<T, GClass26> prevResult)
	{
		int num = 0;
		global::AICoreLayerClass<T> aICoreLayerClass;
		while (true)
		{
			if (num < List_0.Count)
			{
				aICoreLayerClass = List_0[num];
				if (aICoreLayerClass.ShallUseNow())
				{
					break;
				}
				num++;
				continue;
			}
			return null;
		}
		if (aICoreLayerClass != GClass35_0)
		{
			GClass35_0 = aICoreLayerClass;
			Action_0?.Invoke(aICoreLayerClass);
		}
		return aICoreLayerClass.Update(prevResult);
	}

	public void OnDrawGizmos()
	{
	}

	public string GetActiveLayerDebug()
	{
		return GClass35_0.Name();
	}

	public virtual void ManualUpdate()
	{
	}

	public bool method_0(int index, global::AICoreLayerClass<T> layer, bool activeOnStart)
	{
		if (Dictionary_0.ContainsKey(index))
		{
			Debug.LogError("Trying add layer with same id");
			return false;
		}
		Dictionary_0.Add(index, layer);
		if (activeOnStart)
		{
			method_4(layer);
		}
		return true;
	}

	public void method_1(int index)
	{
		if (Dictionary_0.TryGetValue(index, out var value))
		{
			method_4(value);
		}
	}

	public bool method_2(global::AICoreLayerClass<T> index)
	{
		return List_0.Contains(index);
	}

	public void method_3(int index)
	{
		if (Dictionary_0.TryGetValue(index, out var value))
		{
			List_0.Remove(value);
			Gclass32_0.Deactivate(value.Name());
		}
		else
		{
			Debug.LogError($"AHave no layer with index:{index}");
		}
	}

	public void method_4(global::AICoreLayerClass<T> layer)
	{
		layer.Activate();
		int num = 0;
		while (true)
		{
			if (num < List_0.Count)
			{
				global::AICoreLayerClass<T> aICoreLayerClass = List_0[num];
				if (layer.Priority > aICoreLayerClass.Priority)
				{
					break;
				}
				num++;
				continue;
			}
			List_0.Add(layer);
			return;
		}
		List_0.Insert(num, layer);
	}

	public bool HaveLayer<TLayer>() where TLayer : global::AICoreLayerClass<T>
	{
		foreach (KeyValuePair<int, global::AICoreLayerClass<T>> item in Dictionary_0)
		{
			if (item.Value is TLayer)
			{
				return true;
			}
		}
		return false;
	}

	public bool HaveLayer<TLayer>(out TLayer layerTarget) where TLayer : global::AICoreLayerClass<T>
	{
		foreach (KeyValuePair<int, global::AICoreLayerClass<T>> item in Dictionary_0)
		{
			if (item.Value is TLayer val)
			{
				layerTarget = val;
				return true;
			}
		}
		layerTarget = null;
		return false;
	}

	public bool IsLayerActive<TLayer>() where TLayer : global::AICoreLayerClass<T>
	{
		foreach (KeyValuePair<int, global::AICoreLayerClass<T>> item in Dictionary_0)
		{
			global::AICoreLayerClass<T> value = item.Value;
			if (value is TLayer && value.IsActive)
			{
				return true;
			}
		}
		return false;
	}

	public virtual void Dispose()
	{
		foreach (KeyValuePair<int, global::AICoreLayerClass<T>> item in Dictionary_0)
		{
			item.Value.Dispose();
		}
		Dictionary_0.Clear();
	}

	public AICoreStrategyAbstractClass()
	{
	}
}
