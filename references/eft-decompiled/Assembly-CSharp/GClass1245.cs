using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

public abstract class GClass1245
{
	public class GClass1246
	{
		public global::VisibilityIndicesClass<ushort> buffer;
	}

	public const int MAX_SAMPLE_ARRAY_SIZE = 65535;

	public const int INDEX_SIZE = 2;

	[NonSerialized]
	public static ConcurrentQueue<GClass1246> ConcurrentQueue_0 = new ConcurrentQueue<GClass1246>();

	[NonSerialized]
	public static ConcurrentQueue<global::VisibilityIndicesClass<ushort>> ConcurrentQueue_1 = new ConcurrentQueue<global::VisibilityIndicesClass<ushort>>();

	public static IReadOnlyCollection<GClass1246> DebugAllocation => ConcurrentQueue_0;

	public static int NumDebugAllocs => ConcurrentQueue_0.Count;

	public static int RuntimeMemoryUsage => ConcurrentQueue_0.Count * 65535 * 2;

	public static int NumFreeBuffers => ConcurrentQueue_1.Count;

	public static global::VisibilityIndicesClass<ushort> AllocateCellArray()
	{
		if (ConcurrentQueue_1.TryDequeue(out var result))
		{
			result.Clear();
			return result;
		}
		global::VisibilityIndicesClass<ushort> visibilityIndicesClass = new global::VisibilityIndicesClass<ushort>(65535);
		GClass1246 gClass = new GClass1246();
		gClass.buffer = visibilityIndicesClass;
		ConcurrentQueue_0.Enqueue(gClass);
		return visibilityIndicesClass;
	}

	public static void ReuseCellArray(global::VisibilityIndicesClass<ushort> e)
	{
		if (e.Buffer == null || e.Buffer.Length == 0)
		{
			throw new InvalidOperationException("Attempt to reuse empty buffer reference");
		}
		ConcurrentQueue_1.Enqueue(e);
	}
}
