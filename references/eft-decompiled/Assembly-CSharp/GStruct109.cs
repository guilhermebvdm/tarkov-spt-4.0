using System.Collections.Generic;

public readonly ref struct GStruct109(IReadOnlyList<ushort> Indices, IReadOnlyList<int> Pixels)
{
	public readonly IReadOnlyList<ushort> Indices = Indices;

	public readonly IReadOnlyList<int> Pixels = Pixels;

	public static readonly IReadOnlyList<int> Empty;
}
