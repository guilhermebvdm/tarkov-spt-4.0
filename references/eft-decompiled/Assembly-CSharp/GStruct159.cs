using System;
using UnityEngine;

public struct GStruct159
{
	public double x;

	public double y;

	public double z;

	public double this[int index]
	{
		get
		{
			return index switch
			{
				0 => x, 
				1 => y, 
				2 => z, 
				_ => throw new IndexOutOfRangeException("Invalid DoubleVector3 index!"), 
			};
		}
		set
		{
			switch (index)
			{
			default:
				throw new IndexOutOfRangeException("Invalid DoubleVector3 index!");
			case 0:
				x = value;
				break;
			case 1:
				y = value;
				break;
			case 2:
				z = value;
				break;
			}
		}
	}

	public GStruct159 normalized => Normalize(this);

	public double magnitude => Math.Sqrt(x * x + y * y + z * z);

	public double sqrMagnitude => x * x + y * y + z * z;

	public static GStruct159 zero => new GStruct159(0.0, 0.0, 0.0);

	public static GStruct159 one => new GStruct159(1.0, 1.0, 1.0);

	public static GStruct159 forward => new GStruct159(0.0, 0.0, 1.0);

	public static GStruct159 back => new GStruct159(0.0, 0.0, -1.0);

	public static GStruct159 up => new GStruct159(0.0, 1.0, 0.0);

	public static GStruct159 down => new GStruct159(0.0, -1.0, 0.0);

	public static GStruct159 left => new GStruct159(-1.0, 0.0, 0.0);

	public static GStruct159 right => new GStruct159(1.0, 0.0, 0.0);

	[Obsolete("Use DoubleVector3.forward instead.")]
	public static GStruct159 fwd => new GStruct159(0.0, 0.0, 1.0);

	public Vector2 ToVector2()
	{
		return new Vector2((float)x, (float)y);
	}

	public Vector3 ToVector3()
	{
		return new Vector3((float)x, (float)y, (float)z);
	}

	public GClass1665 ToDoubleVector4()
	{
		return new GClass1665(x, y, z, 0.0);
	}

	public GStruct158 ToDoubleVector2()
	{
		return new GStruct158(x, y);
	}

	public GStruct159(double x, double y, double z)
	{
		this.x = x;
		this.y = y;
		this.z = z;
	}

	public GStruct159(double x, double y)
	{
		this.x = x;
		this.y = y;
		z = 0.0;
	}

	public static GStruct159 Lerp(GStruct159 a, GStruct159 b, double t)
	{
		t = Math.Max(0.0, Math.Min(1.0, t));
		return new GStruct159(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t, a.z + (b.z - a.z) * t);
	}

	public static GStruct159 LerpUnclamped(GStruct159 a, GStruct159 b, double t)
	{
		return new GStruct159(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t, a.z + (b.z - a.z) * t);
	}

	public static GStruct159 MoveTowards(GStruct159 current, GStruct159 target, double maxDistanceDelta)
	{
		GStruct159 gStruct = target - current;
		double num = gStruct.magnitude;
		if (!(num <= maxDistanceDelta) && num != 0.0)
		{
			return current + gStruct / num * maxDistanceDelta;
		}
		return target;
	}

	public void Set(double new_x, double new_y, double new_z)
	{
		x = new_x;
		y = new_y;
		z = new_z;
	}

	public static GStruct159 Scale(GStruct159 a, GStruct159 b)
	{
		return new GStruct159(a.x * b.x, a.y * b.y, a.z * b.z);
	}

	public void Scale(GStruct159 scale)
	{
		x *= scale.x;
		y *= scale.y;
		z *= scale.z;
	}

	public static GStruct159 Cross(GStruct159 lhs, GStruct159 rhs)
	{
		return new GStruct159(lhs.y * rhs.z - lhs.z * rhs.y, lhs.z * rhs.x - lhs.x * rhs.z, lhs.x * rhs.y - lhs.y * rhs.x);
	}

	public override int GetHashCode()
	{
		return x.GetHashCode() ^ (y.GetHashCode() << 2) ^ (z.GetHashCode() >> 2);
	}

	public override bool Equals(object other)
	{
		if (!(other is GStruct159 gStruct))
		{
			return false;
		}
		return x.Equals(gStruct.x) && y.Equals(gStruct.y) && z.Equals(gStruct.z);
	}

	public static GStruct159 Reflect(GStruct159 inDirection, GStruct159 inNormal)
	{
		return -2.0 * Dot(inNormal, inDirection) * inNormal + inDirection;
	}

	public static GStruct159 Normalize(GStruct159 value)
	{
		double num = Magnitude(value);
		if (num > 9.999999747378752E-06)
		{
			return value / num;
		}
		return zero;
	}

	public void Normalize()
	{
		double num = Magnitude(this);
		if (num > 9.999999747378752E-06)
		{
			this /= num;
		}
		else
		{
			this = zero;
		}
	}

	public static double Dot(GStruct159 lhs, GStruct159 rhs)
	{
		return lhs.x * rhs.x + lhs.y * rhs.y + lhs.z * rhs.z;
	}

	public static double Distance(GStruct159 a, GStruct159 b)
	{
		GStruct159 gStruct = new GStruct159(a.x - b.x, a.y - b.y, a.z - b.z);
		return Math.Sqrt(gStruct.x * gStruct.x + gStruct.y * gStruct.y + gStruct.z * gStruct.z);
	}

	public static GStruct159 ClampMagnitude(GStruct159 vector, double maxLength)
	{
		if (vector.sqrMagnitude > maxLength * maxLength)
		{
			return vector.normalized * maxLength;
		}
		return vector;
	}

	public static double Magnitude(GStruct159 a)
	{
		return Math.Sqrt(a.x * a.x + a.y * a.y + a.z * a.z);
	}

	public static double SqrMagnitude(GStruct159 a)
	{
		return a.x * a.x + a.y * a.y + a.z * a.z;
	}

	public static GStruct159 Min(GStruct159 lhs, GStruct159 rhs)
	{
		return new GStruct159(Math.Min(lhs.x, rhs.x), Math.Min(lhs.y, rhs.y), Math.Min(lhs.z, rhs.z));
	}

	public static GStruct159 Max(GStruct159 lhs, GStruct159 rhs)
	{
		return new GStruct159(Math.Max(lhs.x, rhs.x), Math.Max(lhs.y, rhs.y), Math.Max(lhs.z, rhs.z));
	}

	public static GStruct159 operator +(GStruct159 a, GStruct159 b)
	{
		return new GStruct159(a.x + b.x, a.y + b.y, a.z + b.z);
	}

	public static GStruct159 operator -(GStruct159 a, GStruct159 b)
	{
		return new GStruct159(a.x - b.x, a.y - b.y, a.z - b.z);
	}

	public static GStruct159 operator -(GStruct159 a)
	{
		return new GStruct159(0.0 - a.x, 0.0 - a.y, 0.0 - a.z);
	}

	public static GStruct159 operator *(GStruct159 a, double d)
	{
		return new GStruct159(a.x * d, a.y * d, a.z * d);
	}

	public static GStruct159 operator *(double d, GStruct159 a)
	{
		return new GStruct159(a.x * d, a.y * d, a.z * d);
	}

	public static GStruct159 operator /(GStruct159 a, double d)
	{
		return new GStruct159(a.x / d, a.y / d, a.z / d);
	}

	public static bool operator ==(GStruct159 lhs, GStruct159 rhs)
	{
		return SqrMagnitude(lhs - rhs) < 9.999999439624929E-11;
	}

	public static bool operator !=(GStruct159 lhs, GStruct159 rhs)
	{
		return !(lhs == rhs);
	}

	public override string ToString()
	{
		return string.Format("({0}, {1}, {2})", new object[3] { x, y, z });
	}

	public string ToString(string format)
	{
		return string.Format("({0}, {1}, {2})", new object[3]
		{
			x.ToString(format),
			y.ToString(format),
			z.ToString(format)
		});
	}
}
