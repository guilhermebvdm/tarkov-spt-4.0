using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using JetBrains.Annotations;
using UnityEngine;

public abstract class GClass856
{
	public enum ESideTurn
	{
		Left,
		Right
	}

	[Serializable]
	[CompilerGenerated]
	public class Class478
	{
		public static readonly Class478 class478_0 = new Class478();

		public static Func<ParameterInfo, Type> func_0;

		public Type method_0(ParameterInfo p)
		{
			return p.ParameterType;
		}
	}

	[Serializable]
	[CompilerGenerated]
	public class Class479<T>
	{
		public static readonly Class479<T> class479_0 = new Class479<T>();

		public static Func<T, float> func_0;

		public float method_0(T x)
		{
			return Random(0f, 5f);
		}
	}

	[Serializable]
	[CompilerGenerated]
	public class Class480<T>
	{
		public static readonly Class480<T> class480_0 = new Class480<T>();

		public static Func<T, int> func_0;

		public int method_0(T item)
		{
			return Random_0.Next(0, 1000);
		}
	}

	[CompilerGenerated]
	public class Class481<T, U> where T : IComparable<T>
	{
		public T key;

		public bool method_0(T k)
		{
			return k.CompareTo(key) < 0;
		}

		public bool method_1(T k)
		{
			return k.CompareTo(key) > 0;
		}
	}

	[CompilerGenerated]
	public class Class482<T>
	{
		public int offset;

		public int index;

		public int length;

		public int method_0(T item)
		{
			int num = offset + index;
			int num2 = index + 1;
			index = num2;
			if (num >= length)
			{
				num -= length;
			}
			else if (num < 0)
			{
				num += length;
			}
			return num;
		}
	}

	[CompilerGenerated]
	public abstract class Class483<T, U> where T : IComparable<T>
	{
		public static CallSite<Func<CallSite, object, object, object>> callSite_0;

		public static CallSite<Func<CallSite, Type, object, object>> callSite_1;

		public static CallSite<Func<CallSite, object, double>> callSite_2;

		public static CallSite<Func<CallSite, object, object, object>> callSite_3;

		public static CallSite<Func<CallSite, Type, object, object>> callSite_4;

		public static CallSite<Func<CallSite, object, double>> callSite_5;
	}

	[NonSerialized]
	public static System.Random Random_0;

	[NonSerialized]
	public static Color32 Color32_0;

	[NonSerialized]
	public static Color32 Color32_1;

	[NonSerialized]
	public static Dictionary<EEnemyPartVisibleType, string> Dictionary_0;

	[NonSerialized]
	public static Dictionary<BodyPartType, string> Dictionary_1;

	static GClass856()
	{
		Color32_0 = new Color32(84, 193, byte.MaxValue, byte.MaxValue);
		Color32_1 = new Color32(196, 0, 0, byte.MaxValue);
		Dictionary_0 = new Dictionary<EEnemyPartVisibleType, string>
		{
			{
				EEnemyPartVisibleType.NotVisible,
				"NV"
			},
			{
				EEnemyPartVisibleType.GreenSence,
				"GS"
			},
			{
				EEnemyPartVisibleType.Sence,
				"SC"
			},
			{
				EEnemyPartVisibleType.Visible,
				"VS"
			}
		};
		Dictionary_1 = new Dictionary<BodyPartType, string>
		{
			{
				BodyPartType.head,
				"HEAD"
			},
			{
				BodyPartType.body,
				"BODY"
			},
			{
				BodyPartType.leftArm,
				"LARM"
			},
			{
				BodyPartType.rightArm,
				"RARM"
			},
			{
				BodyPartType.leftLeg,
				"LLEG"
			},
			{
				BodyPartType.rightLeg,
				"RLEG"
			}
		};
		Random_0 = new System.Random((int)EFTDateTimeClass.Now.Ticks);
	}

	public static long smethod_0(long value, long min, long max)
	{
		if (value >= min)
		{
			if (value <= max)
			{
				return value;
			}
			return max;
		}
		return min;
	}

	public static string ToTimeString(this int seconds)
	{
		return smethod_1(seconds);
	}

	public static string ToTimeString(this float seconds)
	{
		return smethod_1(seconds);
	}

	public static string smethod_1(float seconds)
	{
		TimeSpan timeSpan = TimeSpan.FromSeconds(seconds);
		return $"{(int)timeSpan.TotalHours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
	}

	public static int RandomInclude(int a, int b)
	{
		b++;
		if (a > b)
		{
			return Random_0.Next(b, a);
		}
		return Random_0.Next(a, b);
	}

	public static int RandomSing()
	{
		if (Random(0f, 100f) < 50f)
		{
			return 1;
		}
		return -1;
	}

	public static float Random(float a, float b)
	{
		float num = (float)Random_0.NextDouble();
		return a + (b - a) * num;
	}

	public static bool IsTrue100(float v)
	{
		return Random(0f, 100f) < v;
	}

	public static bool IsTrue01(float v)
	{
		return Random(0f, 1f) < v;
	}

	public static Vector3 RandomHorizontal(float min, float max)
	{
		return new Vector3(Random(min, max), 0f, Random(min, max));
	}

	public static bool RandomBool(float chanceInPercent = 50f)
	{
		return IsTrue100(chanceInPercent);
	}

	public static T ParseEnum<T>(this string value)
	{
		return (T)Enum.Parse(typeof(T), value, ignoreCase: true);
	}

	public static List<T> Shuffle<T>(this List<T> l)
	{
		return l.OrderBy((T x) => Random(0f, 5f)).ToList();
	}

	public static bool InBounds(Vector3 pos, BoxCollider[] colliders)
	{
		int num = 0;
		while (true)
		{
			if (num < colliders.Length)
			{
				BoxCollider box = colliders[num];
				if (GClass369.PointInOABB(pos, box))
				{
					break;
				}
				num++;
				continue;
			}
			return false;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float SqrDistance(this Vector3 a, Vector3 b)
	{
		Vector3 vector = new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
		return vector.x * vector.x + vector.y * vector.y + vector.z * vector.z;
	}

	public static Vector3 RotateAroundPivot(this Vector3 Point, Vector3 Pivot, Quaternion Angle)
	{
		return Angle * (Point - Pivot) + Pivot;
	}

	public static Vector3 RotateAroundPivot(this Vector3 Point, Vector3 Pivot, Vector3 Euler)
	{
		return RotateAroundPivot(Point, Pivot, Quaternion.Euler(Euler));
	}

	public static void RotateAroundPivot(this Transform Me, Vector3 Pivot, Quaternion Angle)
	{
		Me.position = RotateAroundPivot(Me.position, Pivot, Angle);
	}

	public static void RotateAroundPivot(this Transform Me, Vector3 Pivot, Vector3 Euler)
	{
		Me.position = RotateAroundPivot(Me.position, Pivot, Quaternion.Euler(Euler));
	}

	public static float NextFloat(this System.Random random, int min, int max)
	{
		double num = random.NextDouble() * 2.0 - 1.0;
		double num2 = Math.Pow(2.0, random.Next(min, max));
		return (float)(num * num2);
	}

	public static void SetUnlockStatus(this CanvasGroup group, bool value, bool setRaycast = true)
	{
		group.alpha = (value ? 1f : 0.3f);
		group.interactable = value;
		if (setRaycast)
		{
			group.blocksRaycasts = value;
		}
	}

	public static string SubstringIfNecessary(this string @string, int count)
	{
		if (string.IsNullOrEmpty(@string))
		{
			return string.Empty;
		}
		if (@string.Length <= count)
		{
			return @string;
		}
		return @string.Substring(0, count - 2) + "...";
	}

	public static string Prefix(this float value)
	{
		if (!GClass855.Positive(value))
		{
			return string.Empty;
		}
		return "+";
	}

	public static string LevelFormat(this int level)
	{
		if (level <= 10)
		{
			return "0" + level;
		}
		return level.ToString();
	}

	public static string WithPrefix(this float value, string ending = "")
	{
		return Prefix(value) + value.ToString(CultureInfo.InvariantCulture) + ending;
	}

	public static string ColoredWithPrefix(this float value, bool positive, string ending = "")
	{
		string text = WithPrefix(value, ending);
		if (!GClass855.IsZero(value))
		{
			Color32 color = (positive ? Color32_0 : Color32_1);
			text = "<color=#" + ColorUtility.ToHtmlStringRGBA(color) + ">" + text + "</color>";
		}
		return text;
	}

	public static string SubstringIfNecessary(this int @int)
	{
		if (@int <= 99)
		{
			return @int.ToString();
		}
		return "> 99";
	}

	public static string RemoveAllNewLines(this string @string)
	{
		return @string.Replace('\n', '\0');
	}

	public static void ForceAddValue<T, W>(this Dictionary<T, W> d, T k, W v)
	{
		if (d.ContainsKey(k))
		{
			d[k] = v;
		}
		else
		{
			d.Add(k, v);
		}
	}

	public static TValue GetValueByClosestKey<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) where TKey : IComparable<TKey>
	{
		if (dictionary != null && dictionary.Count != 0)
		{
			if (dictionary.TryGetValue(key, out var value))
			{
				return value;
			}
			TKey val = dictionary.Keys.Where((TKey k) => k.CompareTo(key) < 0).Max();
			TKey val2 = dictionary.Keys.Where((TKey k) => k.CompareTo(key) > 0).Min();
			double num = Math.Abs((dynamic)key - (dynamic)val);
			double num2 = Math.Abs((dynamic)val2 - (dynamic)key);
			if (num <= num2)
			{
				return dictionary[val];
			}
			return dictionary[val2];
		}
		return default(TValue);
	}

	public static bool TryGetKey<TKey, TValue>(this IDictionary<TKey, TValue> instance, TValue value, out TKey key)
	{
		foreach (KeyValuePair<TKey, TValue> item in instance)
		{
			if (item.Value.Equals(value))
			{
				key = item.Key;
				return true;
			}
		}
		key = default(TKey);
		return false;
	}

	public static IEnumerable<T> Randomize<T>(this IEnumerable<T> source)
	{
		return source.OrderBy((T item) => Random_0.Next(0, 1000));
	}

	public static IEnumerable<T> Shift<T>(this IEnumerable<T> source, int offset)
	{
		int index = 0;
		int length = source.Count();
		offset %= length;
		return source.OrderBy(delegate
		{
			int num = offset + index;
			int num2 = index + 1;
			index = num2;
			if (num >= length)
			{
				num -= length;
			}
			else if (num < 0)
			{
				num += length;
			}
			return num;
		});
	}

	public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
	{
		HashSet<TKey> hashSet = new HashSet<TKey>();
		foreach (TSource item in source)
		{
			if (hashSet.Add(keySelector(item)))
			{
				yield return item;
			}
		}
	}

	public static List<T> ApplyFilter<T>(this List<T> list, Func<T, bool> predicate)
	{
		if (list == null)
		{
			return null;
		}
		int num = 0;
		while (num < list.Count)
		{
			T arg = list[num];
			if (predicate(arg))
			{
				num++;
			}
			else
			{
				list.RemoveAt(num);
			}
		}
		return list;
	}

	public static List<T> ApplyFilter<T>(this List<T> list, int maxCount, Func<T, bool> predicate)
	{
		if (list == null)
		{
			return null;
		}
		int num = 0;
		int num2 = 0;
		while (num2 < list.Count)
		{
			T arg = list[num2];
			bool flag = false;
			if (num < maxCount)
			{
				flag = predicate(arg);
			}
			if (flag)
			{
				num++;
				num2++;
			}
			else
			{
				list.RemoveAt(num2);
			}
		}
		return list;
	}

	[CanBeNull]
	public static T RandomElement<T>(this IReadOnlyList<T> list)
	{
		if (list.Count == 0)
		{
			return default(T);
		}
		int index = Random_0.Next(0, list.Count);
		return list[index];
	}

	public static IList<T> RandomElement<T>(this IReadOnlyList<T> list, int count)
	{
		List<T> result = new List<T>();
		if (list.Count == 0)
		{
			return result;
		}
		if (list.Count > count)
		{
			result = list.ToList();
			int num = result.Count - count;
			for (int i = 0; i < num; i++)
			{
				T item = RandomElement(result);
				result.Remove(item);
			}
			return result;
		}
		if (list.Count == count)
		{
			return list.ToList();
		}
		Debug.LogError("not implemented yet");
		return list.ToList();
	}

	public static IList<T> RandomElement<T>(this IReadOnlyList<T> list, Func<T, bool> condition, int count)
	{
		List<T> result = new List<T>();
		if (list.Count == 0)
		{
			return result;
		}
		if (list.Count > count)
		{
			result = list.ToList();
			int num = result.Count - count;
			for (int i = 0; i < num; i++)
			{
				T item = RandomElement(result.Where(condition));
				result.Remove(item);
			}
			return result;
		}
		if (list.Count == count)
		{
			return list.ToList();
		}
		Debug.LogError("not implemented yet");
		return list.ToList();
	}

	public static IList<T> ShakeElements<T>(this IReadOnlyList<T> list, int count)
	{
		List<T> list2 = new List<T>();
		System.Random random = new System.Random();
		for (int i = 0; i < count; i++)
		{
			int num = random.Next(list2.Count + 1);
			if (num == list2.Count)
			{
				list2.Add(list[i]);
				continue;
			}
			list2.Add(list2[num]);
			list2[num] = list[i];
		}
		return list2;
	}

	public static IList<T> ShakeElements<T>(this List<T> list)
	{
		return ShakeElements(list, list.Count);
	}

	public static Vector3 Rotate90(Vector3 n, ESideTurn side)
	{
		if (side == ESideTurn.Left)
		{
			return new Vector3(0f - n.z, n.y, n.x);
		}
		return new Vector3(n.z, n.y, 0f - n.x);
	}

	public static Vector3 Rotate90(Vector3 n, int side)
	{
		if (side != 1)
		{
			return new Vector3(n.z, n.y, 0f - n.x);
		}
		return new Vector3(0f - n.z, n.y, n.x);
	}

	[CanBeNull]
	public static T RandomElement<T>(this IEnumerable<T> list)
	{
		return RandomElement(list.ToList());
	}

	public static void ForceAddValue<T>(this List<T> l, T v)
	{
		if (!l.Contains(v))
		{
			l.Add(v);
		}
	}

	public static string SplitCurrencyNumber(this int number, string delimiter = " ")
	{
		return SplitCurrencyNumber((float)number, delimiter);
	}

	public static string SplitCurrencyNumber(this float number, string delimiter = " ")
	{
		NumberFormatInfo numberFormatInfo = new NumberFormatInfo
		{
			NumberGroupSeparator = delimiter
		};
		return number.ToString("N0", numberFormatInfo);
	}

	public static string SplitNumber(long number, string delimiter = " ", int count = 3)
	{
		StringBuilder stringBuilder = new StringBuilder(number.ToString());
		if (stringBuilder.Length > count)
		{
			for (int num = stringBuilder.Length - count; num > 0; num -= count)
			{
				stringBuilder = stringBuilder.Insert(num, delimiter);
			}
		}
		return stringBuilder.ToString();
	}

	public static void SearchTransform<T>(Transform transform, List<T> findObjects) where T : Behaviour
	{
		if (transform == null)
		{
			return;
		}
		T component = transform.gameObject.GetComponent<T>();
		if (component != null)
		{
			findObjects.Add(component);
		}
		foreach (Transform item in transform)
		{
			SearchTransform(item, findObjects);
		}
	}

	public static float GreateRandom(float val)
	{
		return val * Random(0.8f, 1.2f);
	}

	public static float GreateRandom(float val, float fraction)
	{
		return val * Random(1f - fraction, 1f + fraction);
	}

	public static float GreateRandom(int val)
	{
		return (int)((float)val * Random(0.8f, 1.2f));
	}

	public static Delegate CreateDelegate(this MethodInfo methodInfo, object target)
	{
		bool num = methodInfo.ReturnType.Equals(typeof(void));
		IEnumerable<Type> enumerable = from p in methodInfo.GetParameters()
			select p.ParameterType;
		Func<Type[], Type> func;
		if (num)
		{
			func = Expression.GetActionType;
		}
		else
		{
			func = Expression.GetFuncType;
			enumerable = enumerable.Concat(new Type[1] { methodInfo.ReturnType });
		}
		if (methodInfo.IsStatic)
		{
			return Delegate.CreateDelegate(func(enumerable.ToArray()), methodInfo);
		}
		return Delegate.CreateDelegate(func(enumerable.ToArray()), target, methodInfo.Name);
	}

	public static int ParseToInt(this string str)
	{
		if (str == "∞")
		{
			return 0;
		}
		int result = 0;
		if (!int.TryParse(str, out result))
		{
			return 0;
		}
		return result;
	}

	public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> kvp, out TKey key, out TValue value)
	{
		key = kvp.Key;
		value = kvp.Value;
	}

	public static bool StartsWith(this string test, IEnumerable<string> variations)
	{
		foreach (string variation in variations)
		{
			if (test.StartsWith(variation, StringComparison.InvariantCulture))
			{
				return true;
			}
		}
		return false;
	}

	public static void ExecuteForEach<TKey, TValue>([NotNull] this IReadOnlyDictionary<TKey, TValue> dictionary, [NotNull] Action<TKey, TValue> action)
	{
		foreach (var (arg, arg2) in dictionary)
		{
			action(arg, arg2);
		}
	}

	public static void ExecuteForEach<T>([NotNull] this IEnumerable<T> collection, [NotNull] Action<T> action)
	{
		foreach (T item in collection)
		{
			action(item);
		}
	}

	public static bool IsNullOrEmpty<T>(this IEnumerable<T> collection)
	{
		if (collection == null)
		{
			return true;
		}
		return !collection.Any();
	}

	public static void SetCount<T>(this List<T> list, int count)
	{
		int count2 = list.Count;
		int num = count - count2;
		if (num == 0)
		{
			return;
		}
		if (num > 0)
		{
			if (list.Capacity < count)
			{
				list.Capacity = count;
			}
			for (int i = 0; i < num; i++)
			{
				list.Add(default(T));
			}
		}
		else
		{
			num = -num;
			list.RemoveRange(count2 - num, num);
		}
	}

	public static bool IsEqual(this Vector3 a, Vector3 b, float sqrThreshold)
	{
		return (a - b).sqrMagnitude <= sqrThreshold;
	}

	public static string GetSymbol(this EEnemyPartVisibleType type)
	{
		return Dictionary_0.GetValueOrDefault(type, "??");
	}

	public static string GetSymbol(this BodyPartType type)
	{
		return Dictionary_1.GetValueOrDefault(type, "??");
	}
}
