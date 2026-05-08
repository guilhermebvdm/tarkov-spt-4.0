using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class GClass1045
{
	public struct GStruct84(float startVelocity, float bulletMassGram, float bulletDiameterMilimeters, float ballisticCoefficientG1)
	{
		public float StartVelocity = startVelocity;

		public float BulletMassGram = bulletMassGram;

		public float BulletDiameterMilimeters = bulletDiameterMilimeters;

		public float BallisticCoefficientG1 = ballisticCoefficientG1;
	}

	public struct GStruct85(Vector3 position, Vector3 velocity)
	{
		public Vector3 Position = position;

		public Vector3 Velocity = velocity;
	}

	public struct GStruct86(float distance, float velocity, float drop)
	{
		public float Distance = distance;

		public float Velocity = velocity;

		public float Drop = drop;
	}

	[NonSerialized]
	public static GClass1045 Gclass1045_0;

	[NonSerialized]
	[CompilerGenerated]
	public List<GStruct85> List_0;

	[NonSerialized]
	[CompilerGenerated]
	public List<GStruct86> List_1;

	public List<GStruct85> currentBallisticData
	{
		[CompilerGenerated]
		get
		{
			return List_0;
		}
		[CompilerGenerated]
		set
		{
			List_0 = value;
		}
	}

	public List<GStruct86> currentMeasurementData
	{
		[CompilerGenerated]
		get
		{
			return List_1;
		}
		[CompilerGenerated]
		set
		{
			List_1 = value;
		}
	}

	public static GClass1045 getInstance()
	{
		if (Gclass1045_0 == null)
		{
			Gclass1045_0 = new GClass1045();
		}
		return Gclass1045_0;
	}

	public List<GStruct85> method_0(int shootingHeight, int shootingRangeLimit, GStruct84 ammo)
	{
		Vector3 vector = new Vector3(0f, shootingHeight, 0f);
		Vector3 vector2 = new Vector3(ammo.StartVelocity, 0f, 0f);
		Vector3 velocity = vector2;
		Vector3 currentPosition = vector;
		float num = Time.fixedDeltaTime;
		List<GStruct85> list = new List<GStruct85>();
		EftBulletClass.FormTrajectory(vector, vector2, ammo.BulletMassGram, ammo.BulletDiameterMilimeters, ammo.BallisticCoefficientG1, out var trajectoryInfo);
		list.Add(new GStruct85(vector, vector2));
		for (int i = 0; i < 100; i++)
		{
			EftBulletClass.PredictedTrajectoryCalculation(out currentPosition, out velocity, trajectoryInfo, num);
			list.Add(new GStruct85(currentPosition, velocity));
			num += Time.fixedDeltaTime;
		}
		return list;
	}

	public List<GStruct86> method_1(List<GStruct85> trajectory, int step, int height)
	{
		int i = 1;
		List<GStruct86> list = new List<GStruct86>();
		for (int j = 0; j < trajectory.Count - 1; j++)
		{
			for (; !(trajectory[j].Position.x > (float)(i * step)) && !(trajectory[j + 1].Position.x < (float)(i * step)); i++)
			{
				Vector3 vector = Vector3.Lerp(trajectory[j].Position, trajectory[j + 1].Position, ((float)(i * step) - trajectory[j].Position.x) / (trajectory[j + 1].Position.x - trajectory[j].Position.x));
				list.Add(new GStruct86(vector.x, trajectory[j].Velocity.magnitude, (float)height - vector.y));
			}
		}
		list.Add(new GStruct86(trajectory[trajectory.Count - 1].Position.x, trajectory[trajectory.Count - 1].Velocity.magnitude, (float)height - trajectory[trajectory.Count - 1].Position.y));
		return list;
	}

	public void Calculate(int shootingHeight, int shootingRangeLimit, int measurementStep, GStruct84 ammo)
	{
		currentBallisticData = method_0(shootingHeight, shootingRangeLimit, ammo);
		currentMeasurementData = method_1(currentBallisticData, measurementStep, shootingHeight);
	}
}
