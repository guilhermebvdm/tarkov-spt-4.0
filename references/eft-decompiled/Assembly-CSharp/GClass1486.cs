using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

public class GClass1486
{
	public enum CallType
	{
		Basic,
		Array,
		List
	}

	public abstract class Class956<T>
	{
		public CallType Type;

		public virtual void Init(MethodInfo getMethod, MethodInfo setMethod, CallType type)
		{
			Type = type;
		}

		public abstract void Read(T inf, GClass1482 r);

		public abstract void Write(T inf, GClass1484 w);

		public abstract void ReadArray(T inf, GClass1482 r);

		public abstract void WriteArray(T inf, GClass1484 w);

		public abstract void ReadList(T inf, GClass1482 r);

		public abstract void WriteList(T inf, GClass1484 w);

		public Class956()
		{
		}
	}

	public abstract class Class957<T, U> : Class956<T>
	{
		[NonSerialized]
		public Func<T, U> Func_0;

		[NonSerialized]
		public Action<T, U> Action_0;

		[NonSerialized]
		public Func<T, U[]> Func_1;

		[NonSerialized]
		public Action<T, U[]> Action_1;

		[NonSerialized]
		public Func<T, List<U>> Func_2;

		[NonSerialized]
		public Action<T, List<U>> Action_2;

		public override void ReadArray(T inf, GClass1482 r)
		{
			throw new GException13("Unsupported type: " + typeof(U)?.ToString() + "[]");
		}

		public override void WriteArray(T inf, GClass1484 w)
		{
			throw new GException13("Unsupported type: " + typeof(U)?.ToString() + "[]");
		}

		public override void ReadList(T inf, GClass1482 r)
		{
			throw new GException13("Unsupported type: List<" + typeof(U)?.ToString() + ">");
		}

		public override void WriteList(T inf, GClass1484 w)
		{
			throw new GException13("Unsupported type: List<" + typeof(U)?.ToString() + ">");
		}

		public U[] method_0(T inf, GClass1482 r)
		{
			ushort uShort = r.GetUShort();
			U[] array = Func_1(inf);
			array = ((array == null || array.Length != uShort) ? new U[uShort] : array);
			Action_1(inf, array);
			return array;
		}

		public U[] method_1(T inf, GClass1484 w)
		{
			U[] array = Func_1(inf);
			w.Put((ushort)array.Length);
			return array;
		}

		public List<U> method_2(T inf, GClass1482 r, out int len)
		{
			len = r.GetUShort();
			List<U> list = Func_2(inf);
			if (list == null)
			{
				list = new List<U>(len);
				Action_2(inf, list);
			}
			return list;
		}

		public List<U> method_3(T inf, GClass1484 w, out int len)
		{
			List<U> list = Func_2(inf);
			if (list == null)
			{
				len = 0;
				w.Put(0);
				return null;
			}
			len = list.Count;
			w.Put((ushort)len);
			return list;
		}

		public override void Init(MethodInfo getMethod, MethodInfo setMethod, CallType type)
		{
			base.Init(getMethod, setMethod, type);
			switch (type)
			{
			default:
				Func_0 = (Func<T, U>)Delegate.CreateDelegate(typeof(Func<T, U>), getMethod);
				Action_0 = (Action<T, U>)Delegate.CreateDelegate(typeof(Action<T, U>), setMethod);
				break;
			case CallType.List:
				Func_2 = (Func<T, List<U>>)Delegate.CreateDelegate(typeof(Func<T, List<U>>), getMethod);
				Action_2 = (Action<T, List<U>>)Delegate.CreateDelegate(typeof(Action<T, List<U>>), setMethod);
				break;
			case CallType.Array:
				Func_1 = (Func<T, U[]>)Delegate.CreateDelegate(typeof(Func<T, U[]>), getMethod);
				Action_1 = (Action<T, U[]>)Delegate.CreateDelegate(typeof(Action<T, U[]>), setMethod);
				break;
			}
		}

		public Class957()
		{
		}
	}

	public abstract class Class958<T, U> : Class957<T, U>
	{
		public abstract void vmethod_0(GClass1482 r, out U prop);

		public abstract void vmethod_1(GClass1484 w, ref U prop);

		public override void Read(T inf, GClass1482 r)
		{
			vmethod_0(r, out var prop);
			Action_0(inf, prop);
		}

		public override void Write(T inf, GClass1484 w)
		{
			U prop = Func_0(inf);
			vmethod_1(w, ref prop);
		}

		public override void ReadArray(T inf, GClass1482 r)
		{
			U[] array = method_0(inf, r);
			for (int i = 0; i < array.Length; i++)
			{
				vmethod_0(r, out array[i]);
			}
		}

		public override void WriteArray(T inf, GClass1484 w)
		{
			U[] array = method_1(inf, w);
			for (int i = 0; i < array.Length; i++)
			{
				vmethod_1(w, ref array[i]);
			}
		}

		public Class958()
		{
		}
	}

	public class Class961<T, U> : Class957<T, U>
	{
		[NonSerialized]
		public Action<GClass1484, U> Action_3;

		[NonSerialized]
		public Func<GClass1482, U> Func_3;

		public Class961(Action<GClass1484, U> write, Func<GClass1482, U> read)
		{
			Action_3 = write;
			Func_3 = read;
		}

		public override void Read(T inf, GClass1482 r)
		{
			Action_0(inf, Func_3(r));
		}

		public override void Write(T inf, GClass1484 w)
		{
			Action_3(w, Func_0(inf));
		}

		public override void ReadList(T inf, GClass1482 r)
		{
			int len;
			List<U> list = method_2(inf, r, out len);
			int count = list.Count;
			for (int i = 0; i < len; i++)
			{
				if (i < count)
				{
					list[i] = Func_3(r);
				}
				else
				{
					list.Add(Func_3(r));
				}
			}
			if (len < count)
			{
				list.RemoveRange(len, count - len);
			}
		}

		public override void WriteList(T inf, GClass1484 w)
		{
			int len;
			List<U> list = method_3(inf, w, out len);
			for (int i = 0; i < len; i++)
			{
				Action_3(w, list[i]);
			}
		}

		public override void ReadArray(T inf, GClass1482 r)
		{
			U[] array = method_0(inf, r);
			int num = array.Length;
			for (int i = 0; i < num; i++)
			{
				array[i] = Func_3(r);
			}
		}

		public override void WriteArray(T inf, GClass1484 w)
		{
			U[] array = method_1(inf, w);
			int num = array.Length;
			for (int i = 0; i < num; i++)
			{
				Action_3(w, array[i]);
			}
		}
	}

	public class Class962<T, U> : Class957<T, U> where U : struct, GInterface143
	{
		[NonSerialized]
		public U Gparam_0;

		public override void Read(T inf, GClass1482 r)
		{
			Gparam_0.Deserialize(r);
			Action_0(inf, Gparam_0);
		}

		public override void Write(T inf, GClass1484 w)
		{
			Gparam_0 = Func_0(inf);
			Gparam_0.Serialize(w);
		}

		public override void ReadList(T inf, GClass1482 r)
		{
			int len;
			List<U> list = method_2(inf, r, out len);
			int count = list.Count;
			for (int i = 0; i < len; i++)
			{
				U val = default(U);
				val.Deserialize(r);
				if (i < count)
				{
					list[i] = val;
				}
				else
				{
					list.Add(val);
				}
			}
			if (len < count)
			{
				list.RemoveRange(len, count - len);
			}
		}

		public override void WriteList(T inf, GClass1484 w)
		{
			int len;
			List<U> list = method_3(inf, w, out len);
			for (int i = 0; i < len; i++)
			{
				list[i].Serialize(w);
			}
		}

		public override void ReadArray(T inf, GClass1482 r)
		{
			U[] array = method_0(inf, r);
			int num = array.Length;
			for (int i = 0; i < num; i++)
			{
				array[i].Deserialize(r);
			}
		}

		public override void WriteArray(T inf, GClass1484 w)
		{
			U[] array = method_1(inf, w);
			int num = array.Length;
			for (int i = 0; i < num; i++)
			{
				array[i].Serialize(w);
			}
		}
	}

	public class Class963<T, U> : Class957<T, U> where U : class, GInterface143
	{
		[NonSerialized]
		public Func<U> Func_3;

		public Class963(Func<U> constructor)
		{
			Func_3 = constructor;
		}

		public override void Read(T inf, GClass1482 r)
		{
			U val = Func_3();
			val.Deserialize(r);
			Action_0(inf, val);
		}

		public override void Write(T inf, GClass1484 w)
		{
			Func_0(inf)?.Serialize(w);
		}

		public override void ReadList(T inf, GClass1482 r)
		{
			int len;
			List<U> list = method_2(inf, r, out len);
			int count = list.Count;
			for (int i = 0; i < len; i++)
			{
				if (i < count)
				{
					list[i].Deserialize(r);
					continue;
				}
				U val = Func_3();
				val.Deserialize(r);
				list.Add(val);
			}
			if (len < count)
			{
				list.RemoveRange(len, count - len);
			}
		}

		public override void WriteList(T inf, GClass1484 w)
		{
			int len;
			List<U> list = method_3(inf, w, out len);
			for (int i = 0; i < len; i++)
			{
				list[i].Serialize(w);
			}
		}

		public override void ReadArray(T inf, GClass1482 r)
		{
			U[] array = method_0(inf, r);
			int num = array.Length;
			for (int i = 0; i < num; i++)
			{
				array[i] = Func_3();
				array[i].Deserialize(r);
			}
		}

		public override void WriteArray(T inf, GClass1484 w)
		{
			U[] array = method_1(inf, w);
			int num = array.Length;
			for (int i = 0; i < num; i++)
			{
				array[i].Serialize(w);
			}
		}
	}

	public class Class964<T> : Class957<T, int>
	{
		public override void Read(T inf, GClass1482 r)
		{
			Action_0(inf, r.GetInt());
		}

		public override void Write(T inf, GClass1484 w)
		{
			w.Put(Func_0(inf));
		}

		public override void ReadArray(T inf, GClass1482 r)
		{
			Action_1(inf, r.GetIntArray());
		}

		public override void WriteArray(T inf, GClass1484 w)
		{
			w.PutArray(Func_1(inf));
		}
	}

	public class Class965<T> : Class957<T, uint>
	{
		public override void Read(T inf, GClass1482 r)
		{
			Action_0(inf, r.GetUInt());
		}

		public override void Write(T inf, GClass1484 w)
		{
			w.Put(Func_0(inf));
		}

		public override void ReadArray(T inf, GClass1482 r)
		{
			Action_1(inf, r.GetUIntArray());
		}

		public override void WriteArray(T inf, GClass1484 w)
		{
			w.PutArray(Func_1(inf));
		}
	}

	public class Class966<T> : Class957<T, short>
	{
		public override void Read(T inf, GClass1482 r)
		{
			Action_0(inf, r.GetShort());
		}

		public override void Write(T inf, GClass1484 w)
		{
			w.Put(Func_0(inf));
		}

		public override void ReadArray(T inf, GClass1482 r)
		{
			Action_1(inf, r.GetShortArray());
		}

		public override void WriteArray(T inf, GClass1484 w)
		{
			w.PutArray(Func_1(inf));
		}
	}

	public class Class967<T> : Class957<T, ushort>
	{
		public override void Read(T inf, GClass1482 r)
		{
			Action_0(inf, r.GetUShort());
		}

		public override void Write(T inf, GClass1484 w)
		{
			w.Put(Func_0(inf));
		}

		public override void ReadArray(T inf, GClass1482 r)
		{
			Action_1(inf, r.GetUShortArray());
		}

		public override void WriteArray(T inf, GClass1484 w)
		{
			w.PutArray(Func_1(inf));
		}
	}

	public class Class968<T> : Class957<T, long>
	{
		public override void Read(T inf, GClass1482 r)
		{
			Action_0(inf, r.GetLong());
		}

		public override void Write(T inf, GClass1484 w)
		{
			w.Put(Func_0(inf));
		}

		public override void ReadArray(T inf, GClass1482 r)
		{
			Action_1(inf, r.GetLongArray());
		}

		public override void WriteArray(T inf, GClass1484 w)
		{
			w.PutArray(Func_1(inf));
		}
	}

	public class Class969<T> : Class957<T, ulong>
	{
		public override void Read(T inf, GClass1482 r)
		{
			Action_0(inf, r.GetULong());
		}

		public override void Write(T inf, GClass1484 w)
		{
			w.Put(Func_0(inf));
		}

		public override void ReadArray(T inf, GClass1482 r)
		{
			Action_1(inf, r.GetULongArray());
		}

		public override void WriteArray(T inf, GClass1484 w)
		{
			w.PutArray(Func_1(inf));
		}
	}

	public class Class970<T> : Class957<T, byte>
	{
		public override void Read(T inf, GClass1482 r)
		{
			Action_0(inf, r.GetByte());
		}

		public override void Write(T inf, GClass1484 w)
		{
			w.Put(Func_0(inf));
		}

		public override void ReadArray(T inf, GClass1482 r)
		{
			Action_1(inf, r.GetBytesWithLength());
		}

		public override void WriteArray(T inf, GClass1484 w)
		{
			w.PutBytesWithLength(Func_1(inf));
		}
	}

	public class Class971<T> : Class957<T, sbyte>
	{
		public override void Read(T inf, GClass1482 r)
		{
			Action_0(inf, r.GetSByte());
		}

		public override void Write(T inf, GClass1484 w)
		{
			w.Put(Func_0(inf));
		}

		public override void ReadArray(T inf, GClass1482 r)
		{
			Action_1(inf, r.GetSBytesWithLength());
		}

		public override void WriteArray(T inf, GClass1484 w)
		{
			w.PutSBytesWithLength(Func_1(inf));
		}
	}

	public class Class972<T> : Class957<T, float>
	{
		public override void Read(T inf, GClass1482 r)
		{
			Action_0(inf, r.GetFloat());
		}

		public override void Write(T inf, GClass1484 w)
		{
			w.Put(Func_0(inf));
		}

		public override void ReadArray(T inf, GClass1482 r)
		{
			Action_1(inf, r.GetFloatArray());
		}

		public override void WriteArray(T inf, GClass1484 w)
		{
			w.PutArray(Func_1(inf));
		}
	}

	public class Class973<T> : Class957<T, double>
	{
		public override void Read(T inf, GClass1482 r)
		{
			Action_0(inf, r.GetDouble());
		}

		public override void Write(T inf, GClass1484 w)
		{
			w.Put(Func_0(inf));
		}

		public override void ReadArray(T inf, GClass1482 r)
		{
			Action_1(inf, r.GetDoubleArray());
		}

		public override void WriteArray(T inf, GClass1484 w)
		{
			w.PutArray(Func_1(inf));
		}
	}

	public class Class974<T> : Class957<T, bool>
	{
		public override void Read(T inf, GClass1482 r)
		{
			Action_0(inf, r.GetBool());
		}

		public override void Write(T inf, GClass1484 w)
		{
			w.Put(Func_0(inf));
		}

		public override void ReadArray(T inf, GClass1482 r)
		{
			Action_1(inf, r.GetBoolArray());
		}

		public override void WriteArray(T inf, GClass1484 w)
		{
			w.PutArray(Func_1(inf));
		}
	}

	public class Class959<T> : Class958<T, char>
	{
		public override void vmethod_1(GClass1484 w, ref char prop)
		{
			w.Put(prop);
		}

		public override void vmethod_0(GClass1482 r, out char prop)
		{
			prop = r.GetChar();
		}
	}

	public class Class960<T> : Class958<T, IPEndPoint>
	{
		public override void vmethod_1(GClass1484 w, ref IPEndPoint prop)
		{
			w.Put(prop);
		}

		public override void vmethod_0(GClass1482 r, out IPEndPoint prop)
		{
			prop = r.GetNetEndPoint();
		}
	}

	public class Class975<T> : Class957<T, string>
	{
		[NonSerialized]
		public int Int_0;

		public Class975(int maxLength)
		{
			Int_0 = ((maxLength > 0) ? maxLength : 32767);
		}

		public override void Read(T inf, GClass1482 r)
		{
			Action_0(inf, r.GetString(Int_0));
		}

		public override void Write(T inf, GClass1484 w)
		{
			w.Put(Func_0(inf), Int_0);
		}

		public override void ReadArray(T inf, GClass1482 r)
		{
			Action_1(inf, r.GetStringArray(Int_0));
		}

		public override void WriteArray(T inf, GClass1484 w)
		{
			w.PutArray(Func_1(inf), Int_0);
		}
	}

	public class Class976<T> : Class956<T>
	{
		[NonSerialized]
		public PropertyInfo PropertyInfo_0;

		[NonSerialized]
		public Type Type_0;

		public Class976(PropertyInfo property, Type propertyType)
		{
			PropertyInfo_0 = property;
			Type_0 = propertyType;
		}

		public override void Read(T inf, GClass1482 r)
		{
			PropertyInfo_0.SetValue(inf, Enum.ToObject(Type_0, r.GetByte()), null);
		}

		public override void Write(T inf, GClass1484 w)
		{
			w.Put((byte)PropertyInfo_0.GetValue(inf, null));
		}

		public override void ReadArray(T inf, GClass1482 r)
		{
			throw new GException13("Unsupported type: Enum[]");
		}

		public override void WriteArray(T inf, GClass1484 w)
		{
			throw new GException13("Unsupported type: Enum[]");
		}

		public override void ReadList(T inf, GClass1482 r)
		{
			throw new GException13("Unsupported type: List<Enum>");
		}

		public override void WriteList(T inf, GClass1484 w)
		{
			throw new GException13("Unsupported type: List<Enum>");
		}
	}

	public class Class977<T> : Class976<T>
	{
		public Class977(PropertyInfo property, Type propertyType)
			: base(property, propertyType)
		{
		}

		public override void Read(T inf, GClass1482 r)
		{
			PropertyInfo_0.SetValue(inf, Enum.ToObject(Type_0, r.GetInt()), null);
		}

		public override void Write(T inf, GClass1484 w)
		{
			w.Put((int)PropertyInfo_0.GetValue(inf, null));
		}
	}

	public class Class978<T>
	{
		public static Class978<T> Instance;

		[NonSerialized]
		public Class956<T>[] Class956_0;

		[NonSerialized]
		public int Int_0;

		public Class978(List<Class956<T>> serializers)
		{
			Int_0 = serializers.Count;
			Class956_0 = serializers.ToArray();
		}

		public void Write(T obj, GClass1484 writer)
		{
			for (int i = 0; i < Int_0; i++)
			{
				Class956<T> @class = Class956_0[i];
				if (@class.Type == CallType.Basic)
				{
					@class.Write(obj, writer);
				}
				else if (@class.Type == CallType.Array)
				{
					@class.WriteArray(obj, writer);
				}
				else
				{
					@class.WriteList(obj, writer);
				}
			}
		}

		public void Read(T obj, GClass1482 reader)
		{
			for (int i = 0; i < Int_0; i++)
			{
				Class956<T> @class = Class956_0[i];
				if (@class.Type == CallType.Basic)
				{
					@class.Read(obj, reader);
				}
				else if (@class.Type == CallType.Array)
				{
					@class.ReadArray(obj, reader);
				}
				else
				{
					@class.ReadList(obj, reader);
				}
			}
		}
	}

	public abstract class Class979
	{
		public abstract Class956<T> Get<T>();

		public Class979()
		{
		}
	}

	public class Class980<T> : Class979 where T : struct, GInterface143
	{
		public override Class956<U> Get<U>()
		{
			return new Class962<U, T>();
		}
	}

	public class Class981<T> : Class979 where T : class, GInterface143
	{
		[NonSerialized]
		public Func<T> Func_0;

		public Class981(Func<T> constructor)
		{
			Func_0 = constructor;
		}

		public override Class956<U> Get<U>()
		{
			return new Class963<U, T>(Func_0);
		}
	}

	public class Class982<T> : Class979
	{
		[NonSerialized]
		public Action<GClass1484, T> Action_0;

		[NonSerialized]
		public Func<GClass1482, T> Func_0;

		public Class982(Action<GClass1484, T> writer, Func<GClass1482, T> reader)
		{
			Action_0 = writer;
			Func_0 = reader;
		}

		public override Class956<U> Get<U>()
		{
			return new Class961<U, T>(Action_0, Func_0);
		}
	}

	[NonSerialized]
	public GClass1484 Gclass1484_0;

	[NonSerialized]
	public int Int_0;

	[NonSerialized]
	public Dictionary<Type, Class979> Dictionary_0 = new Dictionary<Type, Class979>();

	public void RegisterNestedType<T>() where T : struct, GInterface143
	{
		Dictionary_0.Add(typeof(T), new Class980<T>());
	}

	public void RegisterNestedType<T>(Func<T> constructor) where T : class, GInterface143
	{
		Dictionary_0.Add(typeof(T), new Class981<T>(constructor));
	}

	public void RegisterNestedType<T>(Action<GClass1484, T> writer, Func<GClass1482, T> reader)
	{
		Dictionary_0.Add(typeof(T), new Class982<T>(writer, reader));
	}

	public GClass1486()
		: this(0)
	{
	}

	public GClass1486(int maxStringLength)
	{
		Int_0 = maxStringLength;
	}

	public Class978<T> method_0<T>()
	{
		if (Class978<T>.Instance != null)
		{
			return Class978<T>.Instance;
		}
		PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty);
		List<Class956<T>> list = new List<Class956<T>>();
		foreach (PropertyInfo propertyInfo in properties)
		{
			Type propertyType = propertyInfo.PropertyType;
			Type type = (propertyType.IsArray ? propertyType.GetElementType() : propertyType);
			CallType type2 = (propertyType.IsArray ? CallType.Array : CallType.Basic);
			if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(List<>))
			{
				type = propertyType.GetGenericArguments()[0];
				type2 = CallType.List;
			}
			MethodInfo getMethod = propertyInfo.GetGetMethod();
			MethodInfo setMethod = propertyInfo.GetSetMethod();
			if (getMethod == null || setMethod == null)
			{
				continue;
			}
			Class956<T> @class = null;
			if (propertyType.IsEnum)
			{
				Type underlyingType = Enum.GetUnderlyingType(propertyType);
				if (underlyingType == typeof(byte))
				{
					@class = new Class976<T>(propertyInfo, propertyType);
				}
				else
				{
					if (!(underlyingType == typeof(int)))
					{
						throw new GException13("Not supported enum underlying type: " + underlyingType.Name);
					}
					@class = new Class977<T>(propertyInfo, propertyType);
				}
			}
			else if (type == typeof(string))
			{
				@class = new Class975<T>(Int_0);
			}
			else if (type == typeof(bool))
			{
				@class = new Class974<T>();
			}
			else if (type == typeof(byte))
			{
				@class = new Class970<T>();
			}
			else if (type == typeof(sbyte))
			{
				@class = new Class971<T>();
			}
			else if (type == typeof(short))
			{
				@class = new Class966<T>();
			}
			else if (type == typeof(ushort))
			{
				@class = new Class967<T>();
			}
			else if (type == typeof(int))
			{
				@class = new Class964<T>();
			}
			else if (type == typeof(uint))
			{
				@class = new Class965<T>();
			}
			else if (type == typeof(long))
			{
				@class = new Class968<T>();
			}
			else if (type == typeof(ulong))
			{
				@class = new Class969<T>();
			}
			else if (type == typeof(float))
			{
				@class = new Class972<T>();
			}
			else if (type == typeof(double))
			{
				@class = new Class973<T>();
			}
			else if (type == typeof(char))
			{
				@class = new Class959<T>();
			}
			else if (type == typeof(IPEndPoint))
			{
				@class = new Class960<T>();
			}
			else
			{
				Dictionary_0.TryGetValue(type, out var value);
				if (value != null)
				{
					@class = value.Get<T>();
				}
			}
			if (@class != null)
			{
				@class.Init(getMethod, setMethod, type2);
				list.Add(@class);
				continue;
			}
			throw new GException13("Unknown property type: " + propertyType.FullName);
		}
		Class978<T>.Instance = new Class978<T>(list);
		return Class978<T>.Instance;
	}

	public void Register<T>()
	{
		method_0<T>();
	}

	public T Deserialize<T>(GClass1482 reader) where T : class, new()
	{
		Class978<T> @class = method_0<T>();
		T val = new T();
		try
		{
			@class.Read(val, reader);
			return val;
		}
		catch
		{
			return null;
		}
	}

	public bool Deserialize<T>(GClass1482 reader, T target) where T : class, new()
	{
		Class978<T> @class = method_0<T>();
		try
		{
			@class.Read(target, reader);
		}
		catch
		{
			return false;
		}
		return true;
	}

	public void Serialize<T>(GClass1484 writer, T obj) where T : class, new()
	{
		method_0<T>().Write(obj, writer);
	}

	public byte[] Serialize<T>(T obj) where T : class, new()
	{
		if (Gclass1484_0 == null)
		{
			Gclass1484_0 = new GClass1484();
		}
		Gclass1484_0.Reset();
		Serialize(Gclass1484_0, obj);
		return Gclass1484_0.CopyData();
	}
}
