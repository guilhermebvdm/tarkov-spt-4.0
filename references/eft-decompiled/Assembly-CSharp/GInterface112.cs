using System.Collections.Generic;

public interface GInterface112
{
	int Count { get; }

	void Clear();

	bool ContainsIndex(ushort index);

	int GetNumPixels(ushort index);

	ushort[] ToIndexArray();

	List<KeyValuePair<ushort, int>> ToSamplesArray();

	void UnionWith(GStruct109 result);
}
