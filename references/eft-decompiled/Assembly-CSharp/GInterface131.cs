using System;
using BitPacking;
using UnityEngine;

public interface GInterface131 : ISerializer
{
	int BitsWritten { get; }

	int BytesWritten { get; }

	void WriteLimitedInt32(int value, int min, int max, BitPackingTag tag);

	void WriteUInt32UsingBitRange(uint value, int[] rangeBits, BitPackingTag tag);

	void WriteLimitedFloat(float value, float min, float max, float res, BitPackingTag tag);

	void WriteLimitedString(string value, char min, char max, BitPackingTag tag, uint? maxSize);

	void WriteBits(uint value, int bits);

	void WriteBytes(byte[] data, int startIndex, int length);

	void WriteBytesAndSize(byte[] data);

	void Write(char value);

	void Write(byte value);

	void Write(short value);

	void Write(ushort value);

	void Write(int value);

	void Write(uint value);

	void Write(long value);

	void Write(ulong value);

	void Write(float value);

	void Write(double value);

	void Write(string value);

	void Write(Vector3 value);

	void Write(Vector2 value);

	void Write(Quaternion value);

	void Write(bool value);

	void Write<[Attribute1] TEnum>(TEnum value) where TEnum : struct, Enum, IConvertible;

	void Write(GInterface131 value);

	void WriteCheck(uint checkNumber);

	void WriteAlign();

	void Flush();
}
