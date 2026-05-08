using System;
using System.Runtime.CompilerServices;

public abstract class GClass1291
{
	[Serializable]
	[CompilerGenerated]
	public class Class904
	{
		public static readonly Class904 class904_0 = new Class904();

		public GClass1288 method_0()
		{
			return new GClass1288();
		}
	}

	[NonSerialized]
	public static GClass1300<GClass1288> Gclass1300_0 = new GClass1300<GClass1288>(() => new GClass1288(), 1000);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static GClass1288 Get()
	{
		GClass1288 gClass = Gclass1300_0.Get();
		gClass.Reset();
		return gClass;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Return(GClass1288 writer)
	{
		Gclass1300_0.Return(writer);
	}
}
