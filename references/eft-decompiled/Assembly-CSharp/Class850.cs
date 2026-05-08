using System;
using ComponentAce.Compression.Libs.zlib;

public abstract class Class850
{
	public static byte[] EditorCompressWithAlloc(byte[] inputData)
	{
		byte[] array = new byte[inputData.Length * 2];
		int avail_in = inputData.Length;
		ZStream zStream = new ZStream();
		zStream.deflateInit(9);
		zStream.next_in = inputData;
		zStream.next_in_index = 0;
		zStream.avail_in = avail_in;
		zStream.next_out_index = 0;
		zStream.avail_out = array.Length;
		zStream.next_out = array;
		zStream.deflate(4);
		int next_out_index = zStream.next_out_index;
		zStream.deflateReset();
		zStream.free();
		return array.AsSpan(0, next_out_index).ToArray();
	}
}
