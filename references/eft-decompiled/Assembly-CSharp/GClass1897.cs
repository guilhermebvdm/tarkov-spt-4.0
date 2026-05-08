using System;
using System.Collections.Generic;
using EFT;
using UnityEngine;

public class GClass1897
{
	public struct Struct302
	{
		public HashSet<GStruct176> Frontier;

		public HashSet<GStruct176> Checked;

		public HashSet<GStruct176> Buffer;

		public void Proceed()
		{
			if (Checked.Count > CellsLimit)
			{
				return;
			}
			Buffer.Clear();
			foreach (GStruct176 item in Frontier)
			{
				Buffer.Add(item);
			}
			Frontier.Clear();
			foreach (GStruct176 item2 in Buffer)
			{
				foreach (GStruct176 item3 in method_0(item2))
				{
					method_1(item2, item3);
				}
			}
		}

		public IEnumerable<GStruct176> method_0(GStruct176 cell)
		{
			Vector3 position = cell.Position;
			float cellSize = cell.CellSize;
			int stepsTo = cell.StepsTo + 1;
			yield return new GStruct176(new Vector3(position.x - 1f * cellSize, position.y, position.z), cellSize, stepsTo);
			yield return new GStruct176(new Vector3(position.x + 1f * cellSize, position.y, position.z), cellSize, stepsTo);
			yield return new GStruct176(new Vector3(position.x, position.y, position.z + 1f * cellSize), cellSize, stepsTo);
			yield return new GStruct176(new Vector3(position.x, position.y, position.z - 1f * cellSize), cellSize, stepsTo);
			yield return new GStruct176(new Vector3(position.x, position.y + 1f * cellSize, position.z), cellSize, stepsTo);
			yield return new GStruct176(new Vector3(position.x, position.y - 1f * cellSize, position.z), cellSize, stepsTo);
		}

		public void method_1(GStruct176 origin, GStruct176 target)
		{
			if (!Checked.Contains(target) && method_2(origin, target))
			{
				Checked.Add(target);
				Frontier.Add(target);
			}
		}

		public bool method_2(GStruct176 from, GStruct176 to)
		{
			LayerMask highPolyWithTerrainMask = LayerMaskClass.HighPolyWithTerrainMask;
			RaycastHit hitInfo;
			return !Physics.Raycast(from.Position, to.Position - from.Position, out hitInfo, from.CellSize * 2f, highPolyWithTerrainMask, QueryTriggerInteraction.Collide);
		}
	}

	public struct GStruct176(Vector3 position, float cellSize, int stepsTo) : IEquatable<GStruct176>
	{
		public bool NoWay = false;

		public Vector3 Position = position;

		public float CellSize = cellSize;

		public int StepsTo = stepsTo;

		[NonSerialized]
		public const int Int_0 = 16384;

		[NonSerialized]
		public const double Double_0 = 16384.0;

		public static int smethod_0(float f)
		{
			return (int)((double)f + 16384.0) - 16384;
		}

		public override int GetHashCode()
		{
			return (smethod_0(Position.x / CellSize) * 73856093) ^ (smethod_0(Position.y / CellSize) * 19349663) ^ (smethod_0(Position.z / CellSize) * 83492791);
		}

		public bool Equals(GStruct176 other)
		{
			return GetHashCode().Equals(other.GetHashCode());
		}

		public override bool Equals(object obj)
		{
			if (obj is GStruct176 other)
			{
				return Equals(other);
			}
			return false;
		}
	}

	public static GClass1897 Instance = new GClass1897();

	[NonSerialized]
	public const float Float_0 = 0.5f;

	[NonSerialized]
	public const float Float_1 = 1f;

	public static int Iterations = 30;

	public static int CellsLimit = 2000;

	[NonSerialized]
	public HashSet<GStruct176> HashSet_0 = new HashSet<GStruct176>();

	public void CalculateFrom(Vector3 origin, Vector3 forward)
	{
		Struct302 @struct = new Struct302
		{
			Frontier = new HashSet<GStruct176>(),
			Checked = new HashSet<GStruct176>(),
			Buffer = new HashSet<GStruct176>()
		};
		@struct.Frontier.Add(new GStruct176(origin, 0.5f, 0));
		for (int i = 0; i < Iterations; i++)
		{
			@struct.Proceed();
		}
		if (@struct.Checked.Count <= 0)
		{
			return;
		}
		HashSet_0.Clear();
		foreach (GStruct176 item in @struct.Checked)
		{
			HashSet_0.Add(new GStruct176(item.Position, 1f, item.StepsTo));
		}
		ThermobaricRenderer.Instance.Push(HashSet_0, origin, forward);
	}
}
