using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Comfort.Common;
using EFT;
using EFT.UI.Ragfair;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.UI;

public abstract class GClass855
{
	public enum SideTurn
	{
		left,
		right
	}

	public class Class468
	{
		public int Val;
	}

	[CompilerGenerated]
	public class Class469<T>
	{
		public Action<T> action;

		public T value;

		public void method_0()
		{
			action(value);
		}
	}

	[CompilerGenerated]
	public class Class470<T, U>
	{
		public Action<T, U> action;

		public T value1;

		public void method_0(U value2)
		{
			action(value1, value2);
		}
	}

	[CompilerGenerated]
	public class Class471<T, U>
	{
		public Action<T, U> action;

		public T value1;

		public U value2;

		public void method_0()
		{
			action(value1, value2);
		}
	}

	[CompilerGenerated]
	public class Class472<T>
	{
		public Action<T> action;

		public T value;

		public void method_0()
		{
			action(value);
		}
	}

	[CompilerGenerated]
	public class Class473<T, U>
	{
		public Action<T, U> action;

		public T value1;

		public U value2;

		public void method_0()
		{
			action(value1, value2);
		}
	}

	public const float LOW_ACCURACY_DELTA = 0.001f;

	[NonSerialized]
	public static bool Bool_0;

	[NonSerialized]
	public static float Float_0;

	[NonSerialized]
	public static System.Random Random_0 = new System.Random();

	[NonSerialized]
	public static bool Bool_1 = true;

	[NonSerialized]
	public static double Double_0;

	public const float MAX_NAVMESH_HIT_OFFSET = 0.04f;

	[NonSerialized]
	public static Dictionary<string, Task<Sprite>> Dictionary_0 = new Dictionary<string, Task<Sprite>>();

	public static bool ApproxEquals(this float value, float value2)
	{
		return Math.Abs(value - value2) < float.Epsilon;
	}

	public static bool ApproxEquals(this double value, double value2)
	{
		return Math.Abs(value - value2) < 1.401298464324817E-45;
	}

	public static bool LowAccuracyApprox(this float value, float value2)
	{
		return Math.Abs(value - value2) < 0.001f;
	}

	public static bool IsZero(this float value)
	{
		return Math.Abs(value) < float.Epsilon;
	}

	public static bool IsZero(this Vector2 vector)
	{
		if (IsZero(vector.x))
		{
			return IsZero(vector.y);
		}
		return false;
	}

	public static bool IsZero(this Vector3 vector)
	{
		if (IsZero(vector.x) && IsZero(vector.y))
		{
			return IsZero(vector.z);
		}
		return false;
	}

	public static float Lerp(this Vector2 vector, float t)
	{
		return Mathf.Lerp(vector.x, vector.y, t);
	}

	public static bool IsZero(this double value)
	{
		return Math.Abs(value) < 1.401298464324817E-45;
	}

	public static bool Positive(this double value)
	{
		return value >= 1.401298464324817E-45;
	}

	public static bool Positive(this float value)
	{
		return value >= float.Epsilon;
	}

	public static bool Negative(this double value)
	{
		return value <= -1.401298464324817E-45;
	}

	public static bool Negative(this float value)
	{
		return value <= -1E-45f;
	}

	public static bool ZeroOrNegative(this float value)
	{
		return value < float.Epsilon;
	}

	public static bool ZeroOrPositive(this float value)
	{
		return value > -1E-45f;
	}

	public static double Clamp01(this double value)
	{
		if (value < 0.0)
		{
			return 0.0;
		}
		if (!(value > 1.0))
		{
			return value;
		}
		return 1.0;
	}

	public static double Clamp(this double value, double limit1, double limit2)
	{
		if (limit1 < limit2)
		{
			value = Math.Max(value, limit1);
			value = Math.Min(value, limit2);
		}
		else
		{
			value = Math.Max(value, limit2);
			value = Math.Min(value, limit1);
		}
		return value;
	}

	public static Vector3 Multiply(this Vector3 multiplier1, Vector3 multiplier2)
	{
		return new Vector3(multiplier1.x * multiplier2.x, multiplier1.y * multiplier2.y, multiplier1.z * multiplier2.z);
	}

	public static Vector2 Multiply(this Vector2 multiplier1, Vector2 multiplier2)
	{
		return new Vector2(multiplier1.x * multiplier2.x, multiplier1.y * multiplier2.y);
	}

	public static Vector3 Divide(this Vector3 divisible, Vector3 divisor)
	{
		return new Vector3(divisible.x / divisor.x, divisible.y / divisor.y, divisible.z / divisor.z);
	}

	public static Vector2 Divide(this Vector2 divisible, Vector2 divisor)
	{
		return new Vector2(divisible.x / divisor.x, divisible.y / divisor.y);
	}

	public static Rect Scale(this Rect rect, Vector2 scale)
	{
		return new Rect(rect.x * scale.x, rect.y * scale.y, rect.width * scale.x, rect.height * scale.y);
	}

	public static Vector3 Clamp(this Vector3 vector, Vector3 min, Vector3 max)
	{
		return new Vector3(Mathf.Clamp(vector.x, Mathf.Min(min.x, max.x), Mathf.Max(min.x, max.x)), Mathf.Clamp(vector.y, Mathf.Min(min.y, max.y), Mathf.Max(min.y, max.y)), Mathf.Clamp(vector.z, Mathf.Min(min.z, max.z), Mathf.Max(min.z, max.z)));
	}

	public static Vector3 DeltaAngle(this Vector3 from, Vector3 to)
	{
		return new Vector3(Mathf.DeltaAngle(from.x, to.x), Mathf.DeltaAngle(from.y, to.y), Mathf.DeltaAngle(from.z, to.z));
	}

	public static Vector3 Ceil(this Vector3 vector)
	{
		return new Vector3(Mathf.Ceil(vector.x), Mathf.Ceil(vector.y), Mathf.Ceil(vector.z));
	}

	public static Vector2 Ceil(this Vector2 vector)
	{
		return Ceil((Vector3)vector);
	}

	public static T GetRandomItem<T>(this List<T> list, T excludedItem)
	{
		if (list == null)
		{
			return default(T);
		}
		int count = list.Count;
		switch (count)
		{
		case 0:
			return default(T);
		case 1:
			return list[0];
		default:
		{
			int num = 0;
			T result;
			do
			{
				int index = UnityEngine.Random.Range(0, count);
				result = list[index];
				num++;
			}
			while (!result.Equals(excludedItem) || num == 100);
			return result;
		}
		}
	}

	public static T GetRandomItem<T>(this List<T> list)
	{
		if (list != null && list.Count != 0)
		{
			int index = UnityEngine.Random.Range(0, list.Count);
			return list[index];
		}
		return default(T);
	}

	public static double ExactLength(this AudioClip clip)
	{
		return (double)clip.samples / (double)clip.frequency;
	}

	public static Func<object, T> smethod_0<T>(MethodInfo methodInfo)
	{
		ParameterExpression parameterExpression = Expression.Parameter(typeof(object), "obj");
		UnaryExpression arg = Expression.Convert(parameterExpression, methodInfo.GetParameters().First().ParameterType);
		return Expression.Lambda<Func<object, T>>(Expression.Call(methodInfo, arg), new ParameterExpression[1] { parameterExpression }).Compile();
	}

	public static Action<object, T> smethod_1<T>(MethodInfo methodInfo)
	{
		ParameterExpression parameterExpression = Expression.Parameter(typeof(object), "obj");
		ParameterExpression parameterExpression2 = Expression.Parameter(typeof(T), "value");
		UnaryExpression arg = Expression.Convert(parameterExpression, methodInfo.GetParameters().First().ParameterType);
		UnaryExpression arg2 = Expression.Convert(parameterExpression2, methodInfo.GetParameters().Last().ParameterType);
		return Expression.Lambda<Action<object, T>>(Expression.Call(methodInfo, arg, arg2), new ParameterExpression[2] { parameterExpression, parameterExpression2 }).Compile();
	}

	public static IEnumerable<T> TakeLast<T>(this IEnumerable<T> collection, int n)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		if (n < 0)
		{
			throw new ArgumentOutOfRangeException("n", "n must be 0 or greater");
		}
		LinkedList<T> linkedList = new LinkedList<T>();
		foreach (T item in collection)
		{
			linkedList.AddLast(item);
			if (linkedList.Count > n)
			{
				linkedList.RemoveFirst();
			}
		}
		return linkedList;
	}

	public static Func<TOBjectType, TValueType> CreateGetter<TOBjectType, TValueType>(FieldInfo fieldInfo)
	{
		Type typeFromHandle = typeof(TValueType);
		Type typeFromHandle2 = typeof(TOBjectType);
		return (Func<TOBjectType, TValueType>)CreateGetterFieldDynamicMethod(fieldInfo, typeFromHandle2, typeFromHandle).CreateDelegate(typeof(Func<TOBjectType, TValueType>));
	}

	public static Func<object, TValueType> CreateGetter<TValueType>(FieldInfo fieldInfo, Type objectType)
	{
		Type typeFromHandle = typeof(TValueType);
		return smethod_0<TValueType>(CreateGetterFieldDynamicMethod(fieldInfo, objectType, typeFromHandle).GetBaseDefinition());
	}

	public static DynamicMethod CreateGetterFieldDynamicMethod(FieldInfo fieldInfo, Type objectType, Type valueType)
	{
		DynamicMethod dynamicMethod = new DynamicMethod(fieldInfo.ReflectedType.FullName + ".get_" + fieldInfo.Name, valueType, new Type[1] { objectType }, restrictedSkipVisibility: true);
		ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
		if (fieldInfo.IsStatic)
		{
			iLGenerator.Emit(OpCodes.Ldsfld, fieldInfo);
		}
		else
		{
			iLGenerator.Emit(OpCodes.Ldarg_0);
			iLGenerator.Emit(OpCodes.Ldfld, fieldInfo);
		}
		iLGenerator.Emit(OpCodes.Ret);
		return dynamicMethod;
	}

	public static List<Transform> GetChildsName(Transform transform, string name, bool onlyActive = true)
	{
		List<Transform> list = new List<Transform>();
		foreach (Transform item in transform)
		{
			if (!item.name.Contains(name))
			{
				continue;
			}
			if (onlyActive)
			{
				if (item.gameObject.activeSelf)
				{
					list.Add(item);
				}
			}
			else
			{
				list.Add(item);
			}
		}
		return list;
	}

	public static Transform GetChildName(Transform transform, string name, string nocontains = "")
	{
		foreach (Transform item in transform)
		{
			if (item.name.Contains(name) && item.gameObject.activeSelf)
			{
				if (nocontains.Length <= 0)
				{
					return item;
				}
				if (!item.name.Contains(nocontains))
				{
					return item;
				}
			}
		}
		return null;
	}

	public static bool IsCloseDebug(Vector3 v, float x, float z)
	{
		float num = 0.1f;
		if (v.x > x - num && v.x < x + num && v.z > z - num && v.z < z + num)
		{
			return true;
		}
		return false;
	}

	public static bool IsCloseDebug(Vector3 v, float x, float y, float z)
	{
		float num = 0.1f;
		if (v.x > x - num && v.x < x + num && v.z > z - num && v.z < z + num && v.y > y - num && v.y < y + num)
		{
			return true;
		}
		return false;
	}

	public static Action<TOBjectType, TValueType> CreateSetter<TOBjectType, TValueType>(FieldInfo field)
	{
		Type typeFromHandle = typeof(TOBjectType);
		Type typeFromHandle2 = typeof(TValueType);
		return (Action<TOBjectType, TValueType>)smethod_2(field, typeFromHandle, typeFromHandle2).CreateDelegate(typeof(Action<TOBjectType, TValueType>));
	}

	public static Action<object, TValueType> CreateSetter<TValueType>(FieldInfo field, Type objectType)
	{
		Type typeFromHandle = typeof(TValueType);
		return smethod_1<TValueType>(smethod_2(field, objectType, typeFromHandle).GetBaseDefinition());
	}

	public static DynamicMethod smethod_2(FieldInfo field, Type objType, Type valueType)
	{
		DynamicMethod dynamicMethod = new DynamicMethod(field.ReflectedType.FullName + ".set_" + field.Name, null, new Type[2] { objType, valueType }, restrictedSkipVisibility: true);
		ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
		if (field.IsStatic)
		{
			iLGenerator.Emit(OpCodes.Ldarg_1);
			iLGenerator.Emit(OpCodes.Stsfld, field);
		}
		else
		{
			iLGenerator.Emit(OpCodes.Ldarg_0);
			iLGenerator.Emit(OpCodes.Ldarg_1);
			iLGenerator.Emit(OpCodes.Stfld, field);
		}
		iLGenerator.Emit(OpCodes.Ret);
		return dynamicMethod;
	}

	public static IEnumerable<T> SingleItemAsEnumerable<T>(this T item)
	{
		yield return item;
	}

	public static float RandomNormal(float min, float max)
	{
		double num = 3.5;
		double num2;
		while ((num2 = BoxMuller((double)min + (double)(max - min) / 2.0, (double)(max - min) / 2.0 / num)) > (double)max || !(num2 >= (double)min))
		{
		}
		return (float)num2;
	}

	public static double BoxMuller(double mean, double standard_deviation)
	{
		return mean + BoxMuller() * standard_deviation;
	}

	public static double BoxMuller()
	{
		if (Bool_1)
		{
			Bool_1 = false;
			return Double_0;
		}
		double num;
		double num2;
		double num3;
		do
		{
			num = 2.0 * Random_0.NextDouble() - 1.0;
			num2 = 2.0 * Random_0.NextDouble() - 1.0;
			num3 = num * num + num2 * num2;
		}
		while (num3 >= 1.0 || num3 == 0.0);
		num3 = Math.Sqrt(-2.0 * Math.Log(num3) / num3);
		Double_0 = num2 * num3;
		Bool_1 = true;
		return num * num3;
	}

	public static bool RemoveFromQueue<T>(T item, Queue<T> q)
	{
		bool result = false;
		Queue<T> queue = new Queue<T>();
		while (q.Count > 0)
		{
			T item2 = q.Dequeue();
			if (item2.Equals(item))
			{
				result = true;
			}
			else
			{
				queue.Enqueue(item2);
			}
		}
		while (queue.Count > 0)
		{
			q.Enqueue(queue.Dequeue());
		}
		return result;
	}

	public static Mesh MakeFullScreenMesh(Camera cam)
	{
		Mesh mesh = new Mesh
		{
			name = "Utils MakeFullScreenMesh"
		};
		cam.ViewportToWorldPoint(new Vector3(0f, 0f, 0f));
		Vector3[] vertices = new Vector3[4]
		{
			cam.ViewportToWorldPoint(new Vector3(0f, 0f, 0f)),
			cam.ViewportToWorldPoint(new Vector3(1f, 0f, 0f)),
			cam.ViewportToWorldPoint(new Vector3(0f, 1f, 0f)),
			cam.ViewportToWorldPoint(new Vector3(1f, 1f, 0f))
		};
		mesh.vertices = vertices;
		Vector2[] uv = new Vector2[4]
		{
			new Vector2(0f, 0f),
			new Vector2(1f, 0f),
			new Vector2(0f, 1f),
			new Vector2(1f, 1f)
		};
		mesh.uv = uv;
		int[] triangles = new int[6] { 2, 1, 0, 2, 3, 1 };
		mesh.triangles = triangles;
		return mesh;
	}

	public static void ProcessException(Exception exception)
	{
		Debug.LogException(exception);
	}

	public static string LoadTextResource(string name)
	{
		TextAsset textAsset = GClass861.Load<TextAsset>(name);
		if (textAsset == null)
		{
			return null;
		}
		string text = textAsset.text;
		Resources.UnloadAsset(textAsset);
		return text;
	}

	public static bool IsOnNavMesh(Vector3 v, float dist = 0.04f)
	{
		NavMesh.SamplePosition(v, out var hit, dist, -1);
		return hit.hit;
	}

	public static bool IsDisplayChildCount(this EViewListType type)
	{
		if (type != EViewListType.RequirementsWindow && type != EViewListType.Handbook && type != EViewListType.WishList)
		{
			return type == EViewListType.WeaponBuild;
		}
		return true;
	}

	public static bool IsUpdateChildStatus(this EViewListType type)
	{
		if (type != EViewListType.AllOffers && type != EViewListType.MyOffers && type != EViewListType.WishList)
		{
			return type == EViewListType.WeaponBuild;
		}
		return true;
	}

	public static void SetCanvasRestriction(this CanvasScaler scaler, float scaleFactor)
	{
		if (!(scaler == null))
		{
			scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
			scaler.referencePixelsPerUnit = 100f;
			scaler.scaleFactor = scaleFactor;
		}
	}

	public static bool IsIdBased(this EFilterType type)
	{
		if (type != EFilterType.FilterSearch && type != EFilterType.LinkedSearch && type != EFilterType.NeededSearch && type != EFilterType.BuildItems)
		{
			return type == EFilterType.OfferId;
		}
		return true;
	}

	public static async Task<IResult> LoadAndAssignSprite(this GInterface221 session, string url, Image image, bool resetImage, CancellationToken cancellationToken)
	{
		if (resetImage)
		{
			image.enabled = false;
		}
		if (url == null)
		{
			Debug.LogError("Empty image URL");
			return new FailedResult("Empty image URL");
		}
		if (!Dictionary_0.TryGetValue(url, out var value))
		{
			value = LoadIconSprite(session, url);
			Dictionary_0[url] = value;
		}
		Sprite sprite = await value;
		if (sprite == null)
		{
			Dictionary_0.Remove(url);
			return new FailedResult("Sprite is null.");
		}
		if (!cancellationToken.IsCancellationRequested && !(image == null))
		{
			image.sprite = sprite;
			image.enabled = true;
			return new SuccessfulResult();
		}
		return new FailedResult("Image is null or cancelled.");
	}

	public static async Task<Sprite> LoadIconSprite(GInterface221 session, string imageUrl)
	{
		Texture2D texture2D = await session.LoadTextureMain(imageUrl);
		Sprite result = null;
		if (texture2D != null)
		{
			result = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), Vector2.one * 0.5f, 100f);
		}
		else
		{
			Debug.LogError("Warning! Downloaded sprite is null: " + imageUrl + "!");
		}
		return result;
	}

	public static void UnloadAssetFromBundle(AssetBundle bundle)
	{
		bundle.Unload(unloadAllLoadedObjects: true);
	}

	public static void ClearTransform(this Transform t)
	{
		foreach (Transform item in t)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
	}

	public static void ClearTransformImmediate(this Transform t)
	{
		List<Transform> list = new List<Transform>();
		foreach (Transform item in t)
		{
			list.Add(item);
		}
		Transform[] array = list.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			UnityEngine.Object.DestroyImmediate(array[i].gameObject);
		}
	}

	public static float AngOfNormazedVectors(Vector3 a, Vector3 b)
	{
		return Mathf.Acos(a.x * b.x + a.y * b.y + a.z * b.z) * 57.29578f;
	}

	public static float AngOfNormazedVectorsCoef(Vector3 a, Vector3 b)
	{
		return a.x * b.x + a.y * b.y + a.z * b.z;
	}

	public static bool IsAngLessNormalized(Vector3 a, Vector3 b, float cos)
	{
		return a.x * b.x + a.y * b.y + a.z * b.z > cos;
	}

	public static Vector3 NormalizeFast(Vector3 v)
	{
		float num = (float)Math.Sqrt(v.x * v.x + v.y * v.y + v.z * v.z);
		return new Vector3(v.x / num, v.y / num, v.z / num);
	}

	public static Vector3 NormalizeFastSelf(Vector3 v)
	{
		float num = (float)Math.Sqrt(v.x * v.x + v.y * v.y + v.z * v.z);
		v.x /= num;
		v.y /= num;
		v.z /= num;
		return v;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void NormalizeFastSelf(ref Vector3 v)
	{
		float num = Mathf.Sqrt(v.x * v.x + v.y * v.y + v.z * v.z);
		v *= 1f / num;
	}

	public static bool IsOdd(int value)
	{
		return value % 2 != 0;
	}

	public static Vector3 Rotate90(Vector3 n, SideTurn side)
	{
		if (side == SideTurn.left)
		{
			return new Vector3(0f - n.z, n.y, n.x);
		}
		return new Vector3(n.z, n.y, 0f - n.x);
	}

	public static Vector3 RotateVectorOnAngToZ(Vector3 d, float angDegree)
	{
		Vector3 vector = NormalizeFastSelf(d);
		float f = MathF.PI / 180f * angDegree;
		float num = Mathf.Cos(f);
		float y = Mathf.Sin(f);
		return new Vector3(vector.x * num, y, vector.z * num);
	}

	public static Vector3 RotateOnAngUp(Vector3 b, float angDegree)
	{
		float f = angDegree * (MathF.PI / 180f);
		float num = Mathf.Sin(f);
		float num2 = Mathf.Cos(f);
		float x = b.x * num2 - b.z * num;
		float z = b.z * num2 + b.x * num;
		return new Vector3(x, 0f, z);
	}

	public static Vector2 RotateOnAng(Vector2 b, float a)
	{
		float f = a * (MathF.PI / 180f);
		float num = Mathf.Sin(f);
		float num2 = Mathf.Cos(f);
		float x = b.x * num2 - b.y * num;
		float y = b.y * num2 + b.x * num;
		return new Vector2(x, y);
	}

	public static Action Bind<T>(Action<T> action, T value)
	{
		return delegate
		{
			action(value);
		};
	}

	public static Action<T2> Bind<T1, T2>(Action<T1, T2> action, T1 value1)
	{
		return delegate(T2 arg)
		{
			action(value1, arg);
		};
	}

	public static Action Bind<T1, T2>(Action<T1, T2> action, T1 value1, T2 value2)
	{
		return delegate
		{
			action(value1, value2);
		};
	}

	public static UnityAction BindToUnityAction<T>(Action<T> action, T value)
	{
		return delegate
		{
			action(value);
		};
	}

	public static UnityAction BindToUnityAction<T1, T2>(Action<T1, T2> action, T1 value1, T2 value2)
	{
		return delegate
		{
			action(value1, value2);
		};
	}

	public static float Length(this Quaternion quaternion)
	{
		return Mathf.Sqrt(quaternion.x * quaternion.x + quaternion.y * quaternion.y + quaternion.z * quaternion.z + quaternion.w * quaternion.w);
	}

	public static void Normalize(this Quaternion quaternion)
	{
		float num = Length(quaternion);
		if (!Mathf.Approximately(num, 1f))
		{
			if (Mathf.Approximately(num, 0f))
			{
				quaternion.Set(0f, 0f, 0f, 1f);
			}
			else
			{
				quaternion.Set(quaternion.x / num, quaternion.y / num, quaternion.z / num, quaternion.w / num);
			}
		}
	}

	public static Coroutine WaitOneFrame(this MonoBehaviour component, Action callback)
	{
		if (!component.isActiveAndEnabled)
		{
			return null;
		}
		return component.StartCoroutine(Co_Wait(0f, callback));
	}

	public static Coroutine WaitFrames(this MonoBehaviour component, int frames, Action callback)
	{
		if (!component.isActiveAndEnabled)
		{
			return null;
		}
		return component.StartCoroutine(smethod_3(frames, callback));
	}

	public static Coroutine WaitSeconds(this MonoBehaviour component, float time, Action action)
	{
		if (!component.isActiveAndEnabled)
		{
			return null;
		}
		return component.StartCoroutine(Co_Wait(time, action));
	}

	public static Coroutine WaitForEndOfFrame(this MonoBehaviour component, Action action)
	{
		if (!component.isActiveAndEnabled)
		{
			return null;
		}
		return component.StartCoroutine(smethod_4(action));
	}

	public static IEnumerator Co_Wait(float seconds, Action callback)
	{
		yield return new WaitForSeconds(seconds);
		callback();
	}

	public static IEnumerator smethod_3(int framesCount, [CanBeNull] Action callback)
	{
		while (framesCount > 0)
		{
			yield return null;
			framesCount--;
		}
		callback?.Invoke();
	}

	public static IEnumerator smethod_4([CanBeNull] Action callback)
	{
		yield return new WaitForEndOfFrame();
		callback?.Invoke();
	}

	public static List<WaveInfoClass> ServerOptimizeCountWaves(List<WaveInfoClass> wavesProfiles)
	{
		GClass624<WildSpawnType, BotDifficulty, Class468> gClass = new GClass624<WildSpawnType, BotDifficulty, Class468>();
		foreach (WaveInfoClass wavesProfile in wavesProfiles)
		{
			int limit = wavesProfile.Limit;
			if (gClass.TryGet(wavesProfile.Role, wavesProfile.Difficulty, out var val))
			{
				val.Val += limit;
				continue;
			}
			Class468 @class = new Class468();
			@class.Val += limit;
			gClass.Add(wavesProfile.Role, wavesProfile.Difficulty, @class);
		}
		IEnumerable<GClass623<WildSpawnType, BotDifficulty, Class468>> allVals = gClass.GetAllVals();
		List<WaveInfoClass> list = new List<WaveInfoClass>();
		foreach (GClass623<WildSpawnType, BotDifficulty, Class468> item in allVals)
		{
			list.Add(new WaveInfoClass(item.Value.Val, item.PrimaryKey, item.SecondaryKey));
		}
		return list;
	}

	public static void DecomposeSwingTwist(this Quaternion quaternion, Vector3 twistNormal, out Quaternion swing, out Quaternion twist)
	{
		Vector3 vector = new Vector3(quaternion.x, quaternion.y, quaternion.z);
		if (vector.sqrMagnitude < Mathf.Epsilon)
		{
			Vector3 vector2 = quaternion * twistNormal;
			Vector3 axis = Vector3.Cross(twistNormal, vector2);
			if (axis.sqrMagnitude > Mathf.Epsilon)
			{
				float angle = Vector3.Angle(twistNormal, vector2);
				swing = Quaternion.AngleAxis(angle, axis);
			}
			else
			{
				swing = Quaternion.identity;
			}
			twist = Quaternion.AngleAxis(180f, twistNormal);
		}
		else
		{
			Vector3 vector3 = Vector3.Project(vector, twistNormal);
			twist = new Quaternion(vector3.x, vector3.y, vector3.z, quaternion.w);
			twist = Quaternion.Normalize(twist);
			swing = quaternion * Quaternion.Inverse(twist);
		}
	}

	public static float SqrDistHorizontal(Vector3 a, Vector3 b)
	{
		float num = a.x - b.x;
		float num2 = a.z - b.z;
		return num * num + num2 * num2;
	}

	public static EHysteresisState Hysteresis(float input, float threshold, float negativeDeviation, float positiveDeviation)
	{
		if (!(input > threshold + positiveDeviation))
		{
			if (!(input < threshold - negativeDeviation))
			{
				return EHysteresisState.BetweenThresholds;
			}
			return EHysteresisState.LessThanTreshold;
		}
		return EHysteresisState.AboveTreshold;
	}

	public static EHysteresisState Hysteresis(float input, float threshold, float deviation)
	{
		if (!(input > threshold + deviation))
		{
			if (!(input < threshold - deviation))
			{
				return EHysteresisState.BetweenThresholds;
			}
			return EHysteresisState.LessThanTreshold;
		}
		return EHysteresisState.AboveTreshold;
	}
}
