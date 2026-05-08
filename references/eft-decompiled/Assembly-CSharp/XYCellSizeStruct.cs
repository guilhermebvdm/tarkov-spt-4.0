using System;
using JetBrains.Annotations;
using UnityEngine;

public struct XYCellSizeStruct
{
	public int X;

	public int Y;

	public float Length => (float)Math.Sqrt(X * X + Y * Y);

	public static float Distance(XYCellSizeStruct a, XYCellSizeStruct b)
	{
		return (a - b).Length;
	}

	public static XYCellSizeStruct operator +(XYCellSizeStruct a, XYCellSizeStruct b)
	{
		return new XYCellSizeStruct(a.X + b.X, a.Y + b.Y);
	}

	public static XYCellSizeStruct operator -(XYCellSizeStruct a, XYCellSizeStruct b)
	{
		return new XYCellSizeStruct(a.X - b.X, a.Y - b.Y);
	}

	public static XYCellSizeStruct operator -(XYCellSizeStruct a)
	{
		return new XYCellSizeStruct(-a.X, -a.Y);
	}

	public static XYCellSizeStruct operator *(XYCellSizeStruct a, float d)
	{
		return new XYCellSizeStruct((int)((float)a.X * d), (int)((float)a.Y * d));
	}

	public static XYCellSizeStruct operator *(XYCellSizeStruct a, int d)
	{
		return new XYCellSizeStruct(a.X * d, a.Y * d);
	}

	public static XYCellSizeStruct operator *(float d, XYCellSizeStruct a)
	{
		return a * d;
	}

	public static XYCellSizeStruct operator *(int d, XYCellSizeStruct a)
	{
		return a * d;
	}

	public static XYCellSizeStruct operator *(XYCellSizeStruct a, XYCellSizeStruct b)
	{
		return new XYCellSizeStruct(a.X * b.X, a.Y * b.Y);
	}

	public static XYCellSizeStruct operator /(XYCellSizeStruct a, float d)
	{
		return new XYCellSizeStruct((int)((float)a.X / d), (int)((float)a.Y / d));
	}

	public static XYCellSizeStruct operator /(XYCellSizeStruct a, int d)
	{
		return new XYCellSizeStruct(a.X / d, a.Y / d);
	}

	public static bool operator ==(XYCellSizeStruct lhs, XYCellSizeStruct rhs)
	{
		if (lhs.X == rhs.X)
		{
			return lhs.Y == rhs.Y;
		}
		return false;
	}

	public static bool operator !=(XYCellSizeStruct lhs, XYCellSizeStruct rhs)
	{
		if (lhs.X == rhs.X)
		{
			return lhs.Y != rhs.Y;
		}
		return true;
	}

	public XYCellSizeStruct(int x, int y)
	{
		X = x;
		Y = y;
	}

	public override bool Equals([CanBeNull] object other)
	{
		if (!(other is XYCellSizeStruct xYCellSizeStruct))
		{
			return false;
		}
		if (X.Equals(xYCellSizeStruct.X))
		{
			return Y.Equals(xYCellSizeStruct.Y);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return X.GetHashCode() ^ (Y.GetHashCode() << 2);
	}

	public override string ToString()
	{
		return $"({X}, {Y})";
	}

	public string ToString(string format)
	{
		return $"({X.ToString(format)}, {Y.ToString(format)})";
	}

	public static implicit operator Vector2(XYCellSizeStruct intVec2)
	{
		return new Vector2(intVec2.X, intVec2.Y);
	}
}
