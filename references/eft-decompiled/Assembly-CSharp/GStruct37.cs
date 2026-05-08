using System.Collections.Generic;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct GStruct37 : IEqualityComparer<EPlayerState>
{
	public bool Equals(EPlayerState x, EPlayerState y)
	{
		return x == y;
	}

	public int GetHashCode(EPlayerState obj)
	{
		return (int)obj;
	}
}
