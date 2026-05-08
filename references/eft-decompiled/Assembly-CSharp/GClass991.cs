using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public abstract class GClass991
{
	[NonSerialized]
	public static HashSet<string> HashSet_0;

	public static void CopyTo(GameObject obj, Component component)
	{
		if (!(component == null))
		{
			Copy(component, obj.AddComponent(component.GetType()));
			UnityEngine.Object.Destroy(component);
		}
	}

	public static void CopyFromTo(GameObject from, GameObject to, Type[] types)
	{
		HashSet<Type> hashSet = new HashSet<Type>(types);
		Component[] components = from.GetComponents<Component>();
		foreach (Component component in components)
		{
			if (!(component == null) && hashSet.Contains(component.GetType()))
			{
				CopyTo(to, component);
			}
		}
	}

	public static void Copy(Component fromComponent, Component toComponent)
	{
		if (fromComponent.GetType() != toComponent.GetType())
		{
			return;
		}
		LinkedList<PropertyInfo> linkedList = smethod_0(fromComponent);
		LinkedList<FieldInfo> linkedList2 = smethod_1(fromComponent);
		Dictionary<PropertyInfo, object> dictionary = new Dictionary<PropertyInfo, object>();
		Dictionary<FieldInfo, object> dictionary2 = new Dictionary<FieldInfo, object>();
		foreach (PropertyInfo item in linkedList)
		{
			dictionary[item] = item.GetValue(fromComponent, null);
		}
		foreach (FieldInfo item2 in linkedList2)
		{
			dictionary2[item2] = item2.GetValue(fromComponent);
		}
		LinkedList<PropertyInfo> linkedList3 = smethod_0(toComponent);
		linkedList2 = smethod_1(toComponent);
		foreach (PropertyInfo item3 in linkedList3)
		{
			item3.SetValue(toComponent, dictionary[item3], null);
		}
		foreach (FieldInfo item4 in linkedList2)
		{
			item4.SetValue(toComponent, dictionary2[item4]);
		}
	}

	public static LinkedList<PropertyInfo> smethod_0(Component component)
	{
		PropertyInfo[] properties;
		if (HashSet_0 == null)
		{
			HashSet_0 = new HashSet<string>();
			properties = typeof(Component).GetProperties();
			foreach (PropertyInfo propertyInfo in properties)
			{
				HashSet_0.Add(propertyInfo.Name);
			}
		}
		LinkedList<PropertyInfo> linkedList = new LinkedList<PropertyInfo>();
		properties = component.GetType().GetProperties();
		foreach (PropertyInfo propertyInfo2 in properties)
		{
			if (propertyInfo2.CanWrite && propertyInfo2.CanRead && !propertyInfo2.GetGetMethod().IsStatic && !HashSet_0.Contains(propertyInfo2.Name))
			{
				linkedList.AddLast(propertyInfo2);
			}
		}
		return linkedList;
	}

	public static LinkedList<FieldInfo> smethod_1(Component component)
	{
		LinkedList<FieldInfo> linkedList = new LinkedList<FieldInfo>();
		FieldInfo[] fields = component.GetType().GetFields();
		foreach (FieldInfo fieldInfo in fields)
		{
			if (fieldInfo.IsPublic && !fieldInfo.IsStatic)
			{
				linkedList.AddLast(fieldInfo);
			}
		}
		return linkedList;
	}
}
