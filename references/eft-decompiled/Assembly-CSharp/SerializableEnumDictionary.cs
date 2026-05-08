using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

[Serializable]
public abstract class SerializableEnumDictionary<TEnum, T> : Dictionary<TEnum, T>, ISerializationCallbackReceiver where TEnum : struct, Enum
{
	[SerializeField]
	[HideInInspector]
	public List<string> _keys = new List<string>();

	[SerializeField]
	[HideInInspector]
	public List<T> _values = new List<T>();

	public SerializableEnumDictionary()
		: base(GClass866<TEnum>.Count, GClass866<TEnum>.EqualityComparer)
	{
	}

	[CanBeNull]
	public T GetValueOrDefault(TEnum key)
	{
		if (!TryGetValue(key, out var value))
		{
			return default(T);
		}
		return value;
	}

	public void OnAfterDeserialize()
	{
		Clear();
		for (int i = 0; i < _keys.Count; i++)
		{
			if (GClass866<TEnum>.TryDeserializeValue(_keys[i], out var value))
			{
				base[value] = _values[i];
			}
		}
	}

	void ISerializationCallbackReceiver.OnAfterDeserialize()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnAfterDeserialize
		this.OnAfterDeserialize();
	}

	public void OnBeforeSerialize()
	{
		_keys.Clear();
		_values.Clear();
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			var (value, item) = (KeyValuePair<TEnum, T>)(ref enumerator.Current);
			_keys.Add(GClass866<TEnum>.SerializeValue(value));
			_values.Add(item);
		}
	}

	void ISerializationCallbackReceiver.OnBeforeSerialize()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnBeforeSerialize
		this.OnBeforeSerialize();
	}
}
