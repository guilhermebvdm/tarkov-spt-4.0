public interface IMeasureStatistics
{
	byte[] Buffer { get; }

	bool IsOverflow { get; }

	int BitsWritten { get; }

	int BitsCount { get; }

	int BytesWritten { get; }

	int TotalBytes { get; }

	void WriteBits(uint value, int bits);

	void WriteBytes(byte[] bytes, int startIndex, int length);

	void WriteBytes(byte[] bytes);

	void FlushBits();

	void WriteByteAlign();

	void Reset();
}
