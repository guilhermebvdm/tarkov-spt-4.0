using System;

public abstract class GClass1474
{
	public const int DefaultWindowSize = 64;

	public const int SocketBufferSize = 1048576;

	public const int SocketTTL = 255;

	public const int HeaderSize = 1;

	public const int ChanneledHeaderSize = 4;

	public const int FragmentHeaderSize = 6;

	public const int FragmentedHeaderTotalSize = 10;

	public const ushort MaxSequence = 32768;

	public const ushort HalfMaxSequence = 16384;

	[NonSerialized]
	public const int Int_0 = 11;

	[NonSerialized]
	public const int Int_1 = 68;

	[NonSerialized]
	public static int[] Int_2 = new int[7] { 508, 1024, 1164, 1392, 1404, 1424, 1432 };

	[NonSerialized]
	public static int Int_3 = Int_2[Int_2.Length - 1];

	public const byte MaxConnectionNumber = 4;

	public const int PacketPoolSize = 1000;
}
