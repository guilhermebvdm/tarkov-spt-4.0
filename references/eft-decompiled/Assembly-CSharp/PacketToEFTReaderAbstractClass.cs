using System;
using System.Runtime.CompilerServices;

public abstract class PacketToEFTReaderAbstractClass
{
	[Serializable]
	[CompilerGenerated]
	public class Class903
	{
		public static readonly Class903 class903_0 = new Class903();

		public GClass1283 method_0()
		{
			return new GClass1283(new byte[0]);
		}
	}

	[NonSerialized]
	public static GClass1296<GClass1283> Gclass1296_0 = new GClass1296<GClass1283>(() => new GClass1283(new byte[0]), 1000);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static GClass1283 Get(byte[] bytes)
	{
		GClass1283 gClass = Gclass1296_0.Get();
		gClass.SetBuffer(bytes);
		return gClass;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static GClass1283 Get(ArraySegment<byte> segment)
	{
		GClass1283 gClass = Gclass1296_0.Get();
		gClass.SetBuffer(segment);
		return gClass;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Return(GClass1283 reader)
	{
		Gclass1296_0.Return(reader);
	}
}
