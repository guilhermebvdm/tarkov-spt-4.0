using System;

public readonly struct GStruct117 : IEquatable<GStruct117>
{
	public readonly GClass3071 Connection;

	public GStruct117(GClass3071 connection)
	{
		this = default(GStruct117);
		Connection = connection;
	}

	public override int GetHashCode()
	{
		return Connection.GetHashCode();
	}

	public override string ToString()
	{
		return Connection.ToString();
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj is GStruct117)
		{
			return Equals((GStruct117)obj);
		}
		return false;
	}

	public bool Equals(GStruct117 other)
	{
		if (Connection == null)
		{
			if (other.Connection == null)
			{
				return true;
			}
			return false;
		}
		return Connection.Equals(other.Connection);
	}
}
