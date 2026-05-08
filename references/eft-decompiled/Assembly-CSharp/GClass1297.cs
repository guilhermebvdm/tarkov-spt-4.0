using System.Runtime.CompilerServices;

public abstract class GClass1297
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Compress(EFTWriterClass writer, long last, long current)
	{
		GClass1295.CompressVarInt(writer, current - last);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static long Decompress(EFTReaderClass reader, long last)
	{
		return last + GClass1295.DecompressVarInt(reader);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Compress(EFTWriterClass writer, GStruct126 last, GStruct126 current)
	{
		Compress(writer, last.x, current.x);
		Compress(writer, last.y, current.y);
		Compress(writer, last.z, current.z);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static GStruct126 Decompress(EFTReaderClass reader, GStruct126 last)
	{
		long x = Decompress(reader, last.x);
		long y = Decompress(reader, last.y);
		long z = Decompress(reader, last.z);
		return new GStruct126(x, y, z);
	}
}
