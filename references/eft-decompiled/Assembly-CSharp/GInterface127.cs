public interface GInterface127
{
	byte[] Buffer { get; }

	bool IsOverflow { get; }

	int BitsRead { get; }

	int BitsCount { get; }

	int BytesRead { get; }

	uint ReadBits(int bits);

	void ReadBytes(byte[] destination, int destinationStartIndex, int bytesCount);

	int GetAlignBits();

	void ReadAlign();

	void Reset();
}
