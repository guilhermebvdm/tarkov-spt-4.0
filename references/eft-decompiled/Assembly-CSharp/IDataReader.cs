using System;
using UnityEngine;

public interface IDataReader : ISerializer
{
	int BitsRead { get; }

	int BytesRead { get; }

	int ReadLimitedInt32(int min, int max);

	uint ReadUInt32UsingBitRange(int[] rangeBits);

	float ReadLimitedFloat(float min, float max, float res);

	string ReadLimitedString(char min, char max, uint? maxSize = 1600u);

	uint ReadBits(int bits);

	void ReadBytes(byte[] destinationBytes, int destinationStartIndex, int length);

	byte[] ReadBytesAlloc(uint? maxSize = 1600u);

	char ReadChar();

	byte ReadByte();

	short ReadInt16();

	ushort ReadUInt16();

	int ReadInt32();

	uint ReadUInt32();

	long ReadInt64();

	ulong ReadUInt64();

	float ReadFloat();

	double ReadDouble();

	string ReadString(uint? maxSize = 1600u);

	Vector3 ReadVector3();

	Vector2 ReadVector2();

	Quaternion ReadQuaternion();

	bool ReadBool();

	TEnum ReadEnum<[Attribute1] TEnum>() where TEnum : struct, Enum, IConvertible;

	bool ReadCheck(uint magic, string message = null);

	void ReadAlign();
}
