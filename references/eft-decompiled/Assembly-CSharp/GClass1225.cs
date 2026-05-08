using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public abstract class GClass1225
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int FlattenXYZ(int x, int y, int z, Vector3 cellCount)
	{
		return Mathf.CeilToInt((float)z * cellCount.x * cellCount.y + (float)y * cellCount.x + (float)x);
	}

	public static int FlattenXYZDouble(int x, int y, int z, Vector3 cellCount)
	{
		return (int)Math.Ceiling((double)z * (double)cellCount.x * (double)cellCount.y + (double)y * (double)cellCount.x + (double)x);
	}

	public static void UnflattenToXYZ(int index, out int x, out int y, out int z, Vector3 cellCount)
	{
		x = index % (int)cellCount.x;
		y = index / (int)cellCount.x % (int)cellCount.y;
		z = index / ((int)cellCount.x * (int)cellCount.y);
	}

	public static bool IsXYZInBounds(int x, int y, int z, Vector3 cellCount)
	{
		if (x >= 0 && y >= 0 && z >= 0)
		{
			if (!((float)x >= cellCount.x) && !((float)y >= cellCount.y) && (float)z < cellCount.z)
			{
				return true;
			}
			return false;
		}
		return false;
	}

	public static int CalculateNumberOfCells(Vector3 scale, Vector3 cellSize)
	{
		return Mathf.CeilToInt(scale.x / cellSize.x) * Mathf.CeilToInt(scale.y / cellSize.y) * Mathf.CeilToInt(scale.z / cellSize.z);
	}

	public static Vector3 CalculateCellCount(Vector3 scale, Vector3 cellSize)
	{
		return new Vector3(Mathf.CeilToInt(scale.x / cellSize.x), Mathf.CeilToInt(scale.y / cellSize.y), Mathf.CeilToInt(scale.z / cellSize.z));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int GetIndexForWorldPosNoOrientation(Vector3 worldPos, Vector3 gridOrigin, Vector3 gridScale, Vector3 cellCount, Vector3 cellSize, out bool isOutOfBounds)
	{
		worldPos = worldPos - gridOrigin + gridOrigin;
		Vector3 vector = worldPos - gridOrigin + 0.5f * gridScale;
		int num = (int)(vector.x / cellSize.x);
		int num2 = (int)(vector.y / cellSize.y);
		int num3 = (int)(vector.z / cellSize.z);
		int num4 = (int)Mathf.Clamp(num, 0f, cellCount.x - 1f);
		int num5 = (int)Mathf.Clamp(num2, 0f, cellCount.y - 1f);
		int num6 = (int)Mathf.Clamp(num3, 0f, cellCount.z - 1f);
		if (Mathf.Abs(num - num4) <= 2 && Mathf.Abs(num2 - num5) <= 2 && Mathf.Abs(num3 - num6) <= 2)
		{
			isOutOfBounds = false;
		}
		else
		{
			isOutOfBounds = true;
		}
		return FlattenXYZ(num4, num5, num6, cellCount);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int GetIndexForWorldPos(Vector3 worldPos, Vector3 gridOrigin, Quaternion gridCurrentOrientation, Vector3 gridScale, Quaternion gridBakeOrientation, Vector3 cellCount, Vector3 cellSize, out bool isOutOfBounds)
	{
		worldPos = Quaternion.Inverse(gridCurrentOrientation) * gridBakeOrientation * (worldPos - gridOrigin) + gridOrigin;
		worldPos = Quaternion.Inverse(gridBakeOrientation) * worldPos;
		Vector3 vector = worldPos - gridOrigin + 0.5f * gridScale;
		int num = (int)(vector.x / cellSize.x);
		int num2 = (int)(vector.y / cellSize.y);
		int num3 = (int)(vector.z / cellSize.z);
		int num4 = (int)Mathf.Clamp(num, 0f, cellCount.x - 1f);
		int num5 = (int)Mathf.Clamp(num2, 0f, cellCount.y - 1f);
		int num6 = (int)Mathf.Clamp(num3, 0f, cellCount.z - 1f);
		if (Mathf.Abs(num - num4) <= 2 && Mathf.Abs(num2 - num5) <= 2 && Mathf.Abs(num3 - num6) <= 2)
		{
			isOutOfBounds = false;
		}
		else
		{
			isOutOfBounds = true;
		}
		return FlattenXYZ(num4, num5, num6, cellCount);
	}
}
