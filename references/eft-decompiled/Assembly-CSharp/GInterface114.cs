using System.Collections.Generic;

public interface GInterface114
{
	int NumCells { get; }

	List<ushort>[] RawCells { get; }

	List<int>[] RawPixels { get; }

	void Dispose();

	void DisposeAt(int cellIndex);

	List<ushort> GetCellData(int index);

	List<int> GetPixelData(int index);

	GStruct109 GetSample(int cellIndex);

	void Remap(GDelegate38 remapDelegate);
}
