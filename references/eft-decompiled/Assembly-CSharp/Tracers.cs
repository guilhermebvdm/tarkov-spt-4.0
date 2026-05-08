using System;
using System.Collections.Generic;
using Comfort.Common;
using EFT;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Tracers : MonoBehaviour
{
	private readonly HashSet<EftBulletClass> hashSet_0 = new HashSet<EftBulletClass>();

	public float MaxDistance = 20f;

	public float Probobility = 1f;

	public int Count = 250;

	private Vector3[] vector3_0;

	private Vector4[] vector4_0;

	private Mesh mesh_0;

	private Bounds bounds_0;

	private Material material_0;

	private bool bool_0;

	private int int_0;

	private float float_0;

	private static readonly int int_1 = Shader.PropertyToID("_MainTime");

	public void Start()
	{
	}

	public void OnValidate()
	{
		Count = Math.Max(Count, 1);
		Count = Math.Min(Count, 16383);
	}

	public void Awake()
	{
		int num = Count * 4;
		int num2 = Count * 6;
		vector3_0 = new Vector3[num];
		int[] triangles = new int[num2];
		Vector2[] array = new Vector2[num];
		Vector2[] array2 = new Vector2[num];
		vector4_0 = new Vector4[num];
		for (int i = 0; i < Count; i++)
		{
			Fill(i, array, array2, triangles);
		}
		bounds_0 = new Bounds(Vector3.zero, Vector3.zero);
		mesh_0 = new Mesh
		{
			vertices = vector3_0,
			triangles = triangles,
			uv = array,
			uv2 = array2,
			tangents = vector4_0,
			name = "Tracers _mesh"
		};
		GetComponent<MeshFilter>().mesh = mesh_0;
		material_0 = GetComponent<Renderer>().sharedMaterial;
	}

	public void Fill(int pos, Vector2[] uv0, Vector2[] uv1, int[] triangles)
	{
		int num = pos * 4;
		uv0[num] = new Vector2(-1f, 0f);
		uv0[num + 1] = new Vector2(1f, 0f);
		uv0[num + 2] = new Vector2(-1f, 1f);
		uv0[num + 3] = new Vector2(1f, 1f);
		uv1[num] = (uv1[num + 1] = (uv1[num + 2] = (uv1[num + 3] = new Vector2(UnityEngine.Random.value, UnityEngine.Random.value))));
		int num2 = pos * 6;
		for (int i = 0; i < 1; i++)
		{
			int num3 = num + (i << 1);
			int num4 = num3++;
			int num5 = num3++;
			int num6 = num3++;
			triangles[num2++] = num4;
			triangles[num2++] = num6;
			triangles[num2++] = num3;
			triangles[num2++] = num3;
			triangles[num2++] = num5;
			triangles[num2++] = num4;
		}
	}

	public void EmitSeg(Vector3 start, Vector3 direction, float time)
	{
		int i = int_0 * 4;
		Vector3 vector = start + direction;
		Vector4 vector2 = new Vector4(direction.x, direction.y, direction.z, time);
		int num = i + 4;
		int num2 = 0;
		for (; i < num; i++)
		{
			vector3_0[i] = start;
			vector4_0[i] = vector2;
			num2++;
		}
		int_0++;
		if (int_0 >= Count)
		{
			int_0 = 0;
		}
		bounds_0.Encapsulate(start);
		bounds_0.Encapsulate(vector);
		smethod_0(ref bounds_0, start, vector);
		bool_0 = true;
	}

	public static void smethod_0(ref Bounds bounds, Vector3 p0, Vector3 p1)
	{
		Abs(ref p0);
		Abs(ref p1);
		Vector3 vector = new Vector3(Math.Max(p0.x, p1.x) + 50f, Math.Max(p0.y, p1.y) + 50f, Math.Max(p0.z, p1.z) + 50f);
		Vector3 extents = bounds.extents;
		extents.x = ((extents.x > vector.x) ? extents.x : vector.x);
		extents.y = ((extents.y > vector.y) ? extents.y : vector.y);
		extents.z = ((extents.z > vector.z) ? extents.z : vector.z);
		bounds.extents = extents;
	}

	public static void Abs(ref Vector3 vec)
	{
		vec.x = ((vec.x > 0f) ? vec.x : (0f - vec.x));
		vec.y = ((vec.y > 0f) ? vec.y : (0f - vec.y));
		vec.z = ((vec.z > 0f) ? vec.z : (0f - vec.z));
	}

	public void Update()
	{
		try
		{
			method_0();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			Debug.LogError("traces are stoped");
			base.enabled = false;
		}
	}

	public void method_0()
	{
		GameWorld gameWorld = null;
		if (Singleton<GameWorld>.Instantiated)
		{
			gameWorld = Singleton<GameWorld>.Instance;
		}
		if (gameWorld == null || gameWorld.SharedBallisticsCalculator == null)
		{
			return;
		}
		float deltaTime = Time.deltaTime;
		float num = MaxDistance / deltaTime;
		ISharedBallisticsCalculator sharedBallisticsCalculator = gameWorld.SharedBallisticsCalculator;
		for (int i = 0; i < sharedBallisticsCalculator.ActiveShotsCount; i++)
		{
			EftBulletClass activeShot = sharedBallisticsCalculator.GetActiveShot(i);
			bool num2 = (float)activeShot.PositionHistory.Count * activeShot.Speed > num;
			float sqrMagnitude = (activeShot.PositionHistory[0] - activeShot.PositionHistory[activeShot.PositionHistory.Count - 1]).sqrMagnitude;
			if ((num2 || activeShot.BulletState != EftBulletClass.EBulletState.Flying) && sqrMagnitude > Mathf.Epsilon && UnityEngine.Random.value < Probobility)
			{
				if (!hashSet_0.Contains(activeShot))
				{
					method_1(activeShot);
					hashSet_0.Add(activeShot);
				}
				else if (activeShot.IsShotFinished)
				{
					hashSet_0.Remove(activeShot);
				}
			}
		}
	}

	public void LateUpdate()
	{
		material_0.SetFloat(int_1, Time.time);
		if (bool_0)
		{
			mesh_0.vertices = vector3_0;
			mesh_0.tangents = vector4_0;
			mesh_0.bounds = bounds_0;
			bool_0 = false;
		}
	}

	public void method_1(EftBulletClass shot)
	{
		if (shot.FragmentIndex == 0)
		{
			List<Vector3> positionHistory = shot.PositionHistory;
			Vector3 vector = positionHistory[0];
			Vector3 direction = positionHistory[positionHistory.Count - 1] - vector;
			float magnitude = direction.magnitude;
			direction *= Math.Min(magnitude, MaxDistance) / magnitude;
			EmitSeg(vector, direction, Time.time);
		}
	}
}
