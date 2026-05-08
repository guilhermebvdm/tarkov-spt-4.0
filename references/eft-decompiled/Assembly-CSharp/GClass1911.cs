using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using EFT.InventoryLogic;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;

public abstract class GClass1911
{
	public abstract class Class1184
	{
		public readonly string Name;

		public readonly MemberInfo MemberInfo;

		public readonly JsonPropertyAttribute JsonPropertyAttribute;

		public readonly GAttribute26[] ComponentAtrributes;

		public readonly DefaultValueAttribute DefaultValueAttribute;

		public readonly GAttribute25 DiffableAttribute;

		public abstract Type MemberType { get; }

		public Class1184(MemberInfo memberInfo)
		{
			MemberInfo = memberInfo;
			Name = memberInfo.Name;
			JsonPropertyAttribute = memberInfo.GetCustomAttributes(typeof(JsonPropertyAttribute), inherit: true).FirstOrDefault() as JsonPropertyAttribute;
			DiffableAttribute = memberInfo.GetCustomAttributes(typeof(GAttribute25), inherit: true).FirstOrDefault() as GAttribute25;
			DefaultValueAttribute = memberInfo.GetCustomAttributes(typeof(DefaultValueAttribute), inherit: true).FirstOrDefault() as DefaultValueAttribute;
			ComponentAtrributes = (GAttribute26[])memberInfo.GetCustomAttributes(typeof(GAttribute26), inherit: true);
		}

		public abstract object GetValue(object obj);

		public abstract void SetValue(object obj, object value);
	}

	public abstract class Class1185<T> : Class1184 where T : MemberInfo
	{
		public new readonly T MemberInfo;

		public Class1185(T memberInfo)
			: base(memberInfo)
		{
			MemberInfo = memberInfo;
		}
	}

	public class Class1186 : Class1185<FieldInfo>
	{
		public override Type MemberType => MemberInfo.FieldType;

		public Class1186(FieldInfo memberInfo)
			: base(memberInfo)
		{
		}

		public override object GetValue(object obj)
		{
			return MemberInfo.GetValue(obj);
		}

		public override void SetValue(object obj, object value)
		{
			MemberInfo.SetValue(obj, value);
		}
	}

	public class Class1187 : Class1185<PropertyInfo>
	{
		public override Type MemberType => MemberInfo.PropertyType;

		public Class1187(PropertyInfo memberInfo)
			: base(memberInfo)
		{
		}

		public override object GetValue(object it)
		{
			return MemberInfo.GetValue(it, null);
		}

		public override void SetValue(object obj, object value)
		{
			MemberInfo.SetValue(obj, value);
		}
	}

	public class Class1188
	{
		[Serializable]
		[CompilerGenerated]
		public class Class1189
		{
			public static readonly Class1189 class1189_0 = new Class1189();

			public static Func<FieldInfo, Class1186> func_0;

			public static Func<PropertyInfo, Class1187> func_1;

			public Class1186 method_0(FieldInfo m)
			{
				return new Class1186(m);
			}

			public Class1187 method_1(PropertyInfo m)
			{
				return new Class1187(m);
			}
		}

		[NonSerialized]
		public Type Type_0;

		public readonly string Name;

		public readonly Class1186[] Fields;

		public readonly Class1187[] Properties;

		[NonSerialized]
		public Dictionary<string, Class1186> Dictionary_0;

		[NonSerialized]
		public Dictionary<string, Class1187> Dictionary_1;

		public Class1188(Type classType)
		{
			Type_0 = classType;
			Name = classType.Name;
			Fields = (from m in classType.GetFields()
				select new Class1186(m)).ToArray();
			Properties = (from m in classType.GetProperties()
				select new Class1187(m)).ToArray();
			Dictionary_0 = new Dictionary<string, Class1186>(Fields.Length);
			Dictionary_1 = new Dictionary<string, Class1187>(Properties.Length);
			try
			{
				Class1186[] fields = Fields;
				foreach (Class1186 @class in fields)
				{
					Dictionary_0.Add(@class.Name, @class);
				}
				Class1187[] properties = Properties;
				foreach (Class1187 class2 in properties)
				{
					Dictionary_1[class2.Name] = class2;
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				Class1186[] fields = Fields;
				foreach (Class1186 class3 in fields)
				{
					Debug.Log(class3.Name + " " + class3.MemberInfo);
				}
				Class1187[] properties = Properties;
				foreach (Class1187 class4 in properties)
				{
					Debug.Log(class4.Name + " " + class4.MemberInfo);
				}
				throw;
			}
		}

		public Class1184 GetMember(string name)
		{
			if (Dictionary_0.TryGetValue(name, out var value))
			{
				return value;
			}
			if (Dictionary_1.TryGetValue(name, out var value2))
			{
				return value2;
			}
			foreach (Class1184 item in ((IEnumerable<Class1184>)Dictionary_0.Values).Concat((IEnumerable<Class1184>)Dictionary_1.Values))
			{
				JsonPropertyAttribute jsonPropertyAttribute = item.JsonPropertyAttribute;
				if (jsonPropertyAttribute != null && jsonPropertyAttribute.PropertyName == name)
				{
					return item;
				}
			}
			return null;
		}
	}

	[Serializable]
	[CompilerGenerated]
	public class Class1190
	{
		public static readonly Class1190 class1190_0 = new Class1190();

		public bool method_0(Type t)
		{
			if (!t.IsSubclassOf(typeof(Item)))
			{
				return t.IsSubclassOf(typeof(GClass3379));
			}
			return true;
		}
	}

	[Serializable]
	[CompilerGenerated]
	public class Class1191<TObject>
	{
		public static readonly Class1191<TObject> class1191_0 = new Class1191<TObject>();

		public static Func<TObject, Class1186, object> func_0;

		public static Func<TObject, Class1187, object> func_1;

		public object method_0(TObject it, Class1186 member)
		{
			return member.GetValue(it);
		}

		public object method_1(TObject it, Class1187 member)
		{
			return member.GetValue(it);
		}
	}

	[CompilerGenerated]
	public class Class1192
	{
		public IItemComponent component;

		public bool method_0(Class1187 prop)
		{
			return prop.MemberInfo.PropertyType == component.GetType();
		}

		public bool method_1(Class1186 prop)
		{
			return prop.MemberType == component.GetType();
		}
	}

	[NonSerialized]
	public static Dictionary<Type, Class1188> Dictionary_0;

	public static Class1188 smethod_0(Type type)
	{
		Class1188 value;
		lock (Dictionary_0)
		{
			if (!Dictionary_0.TryGetValue(type, out value))
			{
				value = new Class1188(type);
				Dictionary_0.Add(type, value);
			}
		}
		return value;
	}

	static GClass1911()
	{
		Dictionary_0 = new Dictionary<Type, Class1188>();
		foreach (Type item in from t in Assembly.GetAssembly(typeof(Item)).GetTypes()
			where t.IsSubclassOf(typeof(Item)) || t.IsSubclassOf(typeof(GClass3379))
			select t)
		{
			smethod_0(item);
		}
	}

	public static Item CreateItem(Item item, GClass846 properties)
	{
		try
		{
			foreach (KeyValuePair<string, GClass846> item2 in properties.JToken.ToObject<Dictionary<string, GClass846>>())
			{
				item2.Deconstruct(out var key, out var value);
				string text = key;
				GClass846 gClass = value;
				Class1188 @class = smethod_0(item.GetType());
				Class1184 member = @class.GetMember(text);
				if (member == null)
				{
					Debug.LogWarning("No such field or property " + text + " in " + @class.Name);
				}
				else if (member.ComponentAtrributes.Length != 0)
				{
					object value2 = member.GetValue(item);
					if (value2 == null)
					{
						Debug.LogWarning($"Component {text} has probably been removed from {item.TemplateId} ({@class.Name}), but still exists in item {item.Id}");
					}
					else
					{
						JsonParserClass.ParseJsonTo(gClass, member.MemberType, value2);
					}
				}
				else
				{
					member.SetValue(item, gClass.JToken.ToObject(member.MemberType));
				}
			}
			return item;
		}
		catch (Exception ex)
		{
			Debug.LogError("Error while parsing item upd for " + item?.ToString() + ":\n" + ex);
			return item;
		}
	}

	public static Dictionary<string, object> GetItemProperties(Item item)
	{
		Dictionary<string, object> dictionary = smethod_1(item);
		foreach (IItemComponent component in item.Components)
		{
			if (!component.Serialized)
			{
				continue;
			}
			Dictionary<string, object> dictionary2 = smethod_1(component);
			Class1188 @class = smethod_0(item.GetType());
			Class1187 class2 = @class.Properties.FirstOrDefault((Class1187 prop) => prop.MemberInfo.PropertyType == component.GetType());
			Class1186 class3 = null;
			if (class2 == null)
			{
				class3 = @class.Fields.FirstOrDefault((Class1186 prop) => prop.MemberType == component.GetType());
			}
			if (class2 == null && class3 == null)
			{
				continue;
			}
			object obj;
			if (class2 != null)
			{
				JsonPropertyAttribute jsonPropertyAttribute = class2.JsonPropertyAttribute;
				if (jsonPropertyAttribute == null)
				{
					obj = null;
				}
				else
				{
					obj = jsonPropertyAttribute.PropertyName;
					if (obj != null)
					{
						goto IL_00ba;
					}
				}
				obj = class2.Name;
				goto IL_00ba;
			}
			JsonPropertyAttribute jsonPropertyAttribute2 = class3.JsonPropertyAttribute;
			object obj2;
			if (jsonPropertyAttribute2 == null)
			{
				obj2 = null;
			}
			else
			{
				obj2 = jsonPropertyAttribute2.PropertyName;
				if (obj2 != null)
				{
					goto IL_00dc;
				}
			}
			obj2 = class3.Name;
			goto IL_00dc;
			IL_00dc:
			string key = (string)obj2;
			goto IL_00de;
			IL_00de:
			if (dictionary2.Count > 0)
			{
				dictionary[key] = dictionary2;
			}
			continue;
			IL_00ba:
			key = (string)obj;
			goto IL_00de;
		}
		return dictionary;
	}

	public static Dictionary<string, object> smethod_1<T>(T obj)
	{
		Dictionary<string, object> result = new Dictionary<string, object>();
		Class1188 @class = smethod_0(obj.GetType());
		Class1186[] fields = @class.Fields;
		Class1187[] properties = @class.Properties;
		smethod_2<Class1186, FieldInfo, T>(obj, fields, result, (T it, Class1186 member) => member.GetValue(it));
		smethod_2<Class1187, PropertyInfo, T>(obj, properties, result, (T it, Class1187 member) => member.GetValue(it));
		return result;
	}

	public static void smethod_2<T, U, V>(V item, T[] members, IDictionary<string, object> result, Func<V, T, object> getValue) where T : Class1185<U> where U : MemberInfo
	{
		foreach (T val in members)
		{
			if (val.DiffableAttribute != null)
			{
				DefaultValueAttribute defaultValueAttribute = val.DefaultValueAttribute;
				object obj = getValue(item, val);
				if (defaultValueAttribute == null || !obj.Equals(defaultValueAttribute.Value))
				{
					result[val.Name] = obj;
				}
			}
		}
	}

	[CanBeNull]
	public static T CreateItemLocation<T>([CanBeNull] GClass846 locationData)
	{
		if (locationData != null)
		{
			return JsonParserClass.ParseJsonTo<T>(locationData, Array.Empty<JsonConverter>());
		}
		return default(T);
	}

	public static GClass846 SaveItemLocation(LocationInGrid itemLocation)
	{
		return JsonParserClass.ToUnparsedData(itemLocation);
	}

	public static GClass846 SaveItemLocation(int stackSlotPosition)
	{
		return JsonParserClass.ToUnparsedData(stackSlotPosition);
	}
}
