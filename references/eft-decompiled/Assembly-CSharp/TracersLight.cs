using System;
using System.Collections.Generic;
using Comfort.Common;
using EFT;
using JsonType;
using UnityEngine;

public class TracersLight : MonoBehaviour
{
	public class Class654
	{
		public readonly Color BackCurrentTracerColor;

		public readonly Color CurrentTracerColor;

		public Vector3 Direction;

		public readonly Vector3 PositionShift;

		public readonly float SizePercent;

		public readonly float Speed;

		public Class654(Vector3 positionShift, Vector3 direction, Color currentTracerColor, Color backCurrentTracerColor, float sizePercent, float speed)
		{
			PositionShift = positionShift;
			Direction = direction;
			CurrentTracerColor = currentTracerColor;
			BackCurrentTracerColor = backCurrentTracerColor;
			SizePercent = sizePercent;
			Speed = speed;
		}
	}

	[ColorUsage(true, true, 0f, 8f, 0.125f, 3f)]
	public Color Color;

	[SerializeField]
	private float _maxFlyingTime = 7f;

	[SerializeField]
	private AnimationCurve _sizeTimeCurve;

	[SerializeField]
	private AnimationCurve _colorAlphaCurve;

	[SerializeField]
	private float _grenadeFlyingMaxTime = 0.25f;

	[SerializeField]
	private float _grenadeSizeCoef = 0.15f;

	[SerializeField]
	private MeshFilter _tracersMF;

	[SerializeField]
	private float _speedModifier = 0.005f;

	[SerializeField]
	private float _grenadeSpeedModifier = 0.02f;

	private List<ISharedBallisticsCalculator> list_0;

	private List<Class654> list_1;

	private bool bool_0;

	private bool bool_1;

	private static readonly float float_0 = 100f;

	public void Awake()
	{
		list_1 = new List<Class654>();
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
			Debug.LogError("traces light are stoped");
			base.enabled = false;
		}
	}

	public void method_0()
	{
		if (list_1 == null)
		{
			list_1 = new List<Class654>();
		}
		if (list_1.Count > 0)
		{
			list_1.Clear();
		}
		if (Singleton<GameWorld>.Instantiated)
		{
			if (Singleton<GameWorld>.Instance.SharedBallisticsCalculator != null)
			{
				method_1(Singleton<GameWorld>.Instance.SharedBallisticsCalculator);
			}
			if (Singleton<GameWorld>.Instance.ClientBallisticCalculator != null)
			{
				method_1(Singleton<GameWorld>.Instance.ClientBallisticCalculator);
			}
			if (list_1.Count > 0)
			{
				method_5();
				bool_0 = true;
			}
			else if (bool_0)
			{
				method_6();
				bool_0 = false;
			}
		}
	}

	public void method_1(ISharedBallisticsCalculator ballisticsCalculator)
	{
		for (int i = 0; i < ballisticsCalculator.ActiveShotsCount; i++)
		{
			EftBulletClass activeShot = ballisticsCalculator.GetActiveShot(i);
			if (activeShot.GetTimeSinceShot < _maxFlyingTime && !activeShot.ShotProcessed)
			{
				method_2(activeShot);
			}
		}
	}

	public void method_2(EftBulletClass shot)
	{
		AmmoItemClass ammoItemClass = (AmmoItemClass)shot.Ammo;
		if (!ammoItemClass.Tracer)
		{
			return;
		}
		List<Vector3> positionHistory = shot.PositionHistory;
		if (positionHistory.Count < 2)
		{
			return;
		}
		Vector3 vector = positionHistory[positionHistory.Count - 2];
		Vector3 vector2 = positionHistory[positionHistory.Count - 1];
		float sqrMagnitude = (vector2 - vector).sqrMagnitude;
		if (!(sqrMagnitude < 0.001f) && (positionHistory.Count != 2 || !(sqrMagnitude < float_0)))
		{
			Color = GClass1409.ToColor(ammoItemClass.TracerColor);
			if (ammoItemClass.TracerColor == TaxonomyColor.tracerYellow)
			{
				method_4(shot, vector, vector2);
			}
			else
			{
				method_3(shot, vector, vector2);
			}
		}
	}

	public void method_3(EftBulletClass shot, Vector3 p0, Vector3 p1)
	{
		Vector3 normalized = (p1 - p0).normalized;
		float time = shot.GetTimeSinceShot / _maxFlyingTime;
		float sizePercent = _sizeTimeCurve.Evaluate(time);
		Color color = Color;
		float num = _colorAlphaCurve.Evaluate(time);
		color.a *= num;
		Color backCurrentTracerColor = color;
		float speed = shot.VelocityMagnitude * _speedModifier;
		list_1.Add(new Class654(p1, normalized, color, backCurrentTracerColor, sizePercent, speed));
	}

	public void method_4(EftBulletClass shot, Vector3 p0, Vector3 p1)
	{
		if (!(shot.GetTimeSinceShot > _grenadeFlyingMaxTime))
		{
			Vector3 normalized = (p1 - p0).normalized;
			float num = shot.GetTimeSinceShot / _grenadeFlyingMaxTime;
			float num2 = _sizeTimeCurve.Evaluate(num);
			Color color = Color;
			float num3 = 1f - num;
			color.a *= num3;
			float speed = shot.VelocityMagnitude * _grenadeSpeedModifier;
			num2 *= _grenadeSizeCoef;
			list_1.Add(new Class654(p1, normalized, color, Color.clear, num2, speed));
		}
	}

	public void method_5()
	{
		Vector3[] array = new Vector3[12 * list_1.Count];
		int[] array2 = new int[18 * list_1.Count];
		Vector2[] array3 = new Vector2[12 * list_1.Count];
		Color[] array4 = new Color[array.Length];
		for (int i = 0; i < list_1.Count; i++)
		{
			float speed = list_1[i].Speed;
			float num = 0.5f * list_1[i].SizePercent;
			array[12 * i] = new Vector3(0f - num, 0f - num, -0.03f * speed);
			array[1 + 12 * i] = new Vector3(num, 0f - num, -0.03f * speed);
			array[2 + 12 * i] = new Vector3(num, num, -0.03f * speed);
			array[3 + 12 * i] = new Vector3(0f - num, num, -0.03f * speed);
			array[4 + 12 * i] = new Vector3(0f, 0f - num, 0f);
			array[5 + 12 * i] = new Vector3(0f, 0f - num, 0f - speed);
			array[6 + 12 * i] = new Vector3(0f, num, 0f - speed);
			array[7 + 12 * i] = new Vector3(0f, num, 0f);
			array[8 + 12 * i] = new Vector3(0f - num, 0f, 0f);
			array[9 + 12 * i] = new Vector3(0f - num, 0f, 0f - speed);
			array[10 + 12 * i] = new Vector3(num, 0f, 0f - speed);
			array[11 + 12 * i] = new Vector3(num, 0f, 0f);
			Vector3 tangent = Vector3.zero;
			Vector3 binormal = Vector3.zero;
			Vector3.OrthoNormalize(ref list_1[i].Direction, ref tangent, ref binormal);
			Matrix4x4 matrix4x = Matrix4x4.LookAt(Vector3.zero, list_1[i].Direction, tangent);
			for (int j = 0; j < 12; j++)
			{
				array[i * 12 + j] = matrix4x.MultiplyPoint3x4(array[i * 12 + j]);
				array[i * 12 + j] += list_1[i].PositionShift;
			}
			array2[18 * i] = 12 * i;
			array2[1 + 18 * i] = 1 + 12 * i;
			array2[2 + 18 * i] = 2 + 12 * i;
			array2[3 + 18 * i] = 12 * i;
			array2[4 + 18 * i] = 2 + 12 * i;
			array2[5 + 18 * i] = 3 + 12 * i;
			array2[6 + 18 * i] = 4 + 12 * i;
			array2[7 + 18 * i] = 5 + 12 * i;
			array2[8 + 18 * i] = 6 + 12 * i;
			array2[9 + 18 * i] = 4 + 12 * i;
			array2[10 + 18 * i] = 6 + 12 * i;
			array2[11 + 18 * i] = 7 + 12 * i;
			array2[12 + 18 * i] = 8 + 12 * i;
			array2[13 + 18 * i] = 9 + 12 * i;
			array2[14 + 18 * i] = 10 + 12 * i;
			array2[15 + 18 * i] = 8 + 12 * i;
			array2[16 + 18 * i] = 10 + 12 * i;
			array2[17 + 18 * i] = 11 + 12 * i;
			array3[12 * i] = new Vector2(0.5f, 0.5f);
			array3[1 + 12 * i] = new Vector2(1f, 0.5f);
			array3[2 + 12 * i] = new Vector2(1f, 1f);
			array3[3 + 12 * i] = new Vector2(0.5f, 1f);
			array3[4 + 12 * i] = new Vector2(0f, 0f);
			array3[5 + 12 * i] = new Vector2(1f, 0f);
			array3[6 + 12 * i] = new Vector2(1f, 0.5f);
			array3[7 + 12 * i] = new Vector2(0f, 0.5f);
			array3[8 + 12 * i] = new Vector2(0f, 0f);
			array3[9 + 12 * i] = new Vector2(1f, 0f);
			array3[10 + 12 * i] = new Vector2(1f, 0.5f);
			array3[11 + 12 * i] = new Vector2(0f, 0.5f);
			for (int k = 0; k < 12; k++)
			{
				array4[k + 12 * i] = ((k < 4) ? list_1[i].BackCurrentTracerColor : list_1[i].CurrentTracerColor);
			}
		}
		Mesh mesh = _tracersMF.mesh;
		mesh.Clear();
		mesh.vertices = array;
		mesh.triangles = array2;
		mesh.uv = array3;
		mesh.colors = array4;
		mesh.RecalculateNormals();
	}

	public void method_6()
	{
		_tracersMF.mesh.Clear();
	}
}
