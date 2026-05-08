using System;
using JetBrains.Annotations;

[Serializable]
public class LocationInGrid
{
	public int x;

	public int y;

	public ItemRotation r;

	public LocationInGrid()
	{
	}

	public LocationInGrid(int x, int y, ItemRotation r)
	{
		this.x = x;
		this.y = y;
		this.r = r;
	}

	public LocationInGrid Clone()
	{
		return new LocationInGrid(x, y, r);
	}

	public static bool operator ==([CanBeNull] LocationInGrid from, [CanBeNull] LocationInGrid to)
	{
		if ((object)from == to)
		{
			return true;
		}
		if ((object)from != null && (object)to != null)
		{
			if (from.x == to.x && from.y == to.y)
			{
				return from.r == to.r;
			}
			return false;
		}
		return false;
	}

	public static bool operator !=(LocationInGrid from, LocationInGrid to)
	{
		return !(from == to);
	}

	public bool method_0([CanBeNull] LocationInGrid other)
	{
		if ((object)other != null && x == other.x && y == other.y)
		{
			return r == other.r;
		}
		return false;
	}

	public override bool Equals([CanBeNull] object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (this == obj)
		{
			return true;
		}
		if (obj.GetType() == GetType())
		{
			return method_0((LocationInGrid)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (int)((x * 397 + y) * 397 + r);
	}

	public override string ToString()
	{
		return "(x: " + x + ", y: " + y + ", r: " + r.ToString() + ")";
	}
}
