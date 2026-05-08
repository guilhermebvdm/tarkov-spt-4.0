using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class GClass807
{
	public struct Struct83
	{
		public FieldInfo fieldInfo;

		public object defaultValue;
	}

	[NonSerialized]
	public List<Struct83> List_0 = new List<Struct83>();

	public void StoreState(object host, Type[] excludedBaseClasses = null)
	{
		Type type = host.GetType();
		List<FieldInfo> list = new List<FieldInfo>();
		Type type2 = type;
		while (!method_0(excludedBaseClasses, type2))
		{
			FieldInfo[] fields = type2.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			list.AddRange(fields);
			if (!((type2 = type2.BaseType) != null))
			{
				break;
			}
		}
		List_0.Clear();
		foreach (FieldInfo item in list)
		{
			object value = item.GetValue(host);
			if (item.GetCustomAttribute<GAttribute9>() == null && (item.FieldType.IsValueType || value == null || (value is UnityEngine.Object obj && obj == null) || item.FieldType == typeof(string)))
			{
				List_0.Add(new Struct83
				{
					fieldInfo = item,
					defaultValue = value
				});
			}
		}
	}

	public void RestoreState(object host)
	{
		foreach (Struct83 item in List_0)
		{
			item.fieldInfo.SetValue(host, item.defaultValue);
		}
	}

	public bool method_0(Type[] types, Type type)
	{
		if (types != null)
		{
			for (int i = 0; i < types.Length; i++)
			{
				if (types[i] == type)
				{
					return true;
				}
			}
		}
		return false;
	}
}
