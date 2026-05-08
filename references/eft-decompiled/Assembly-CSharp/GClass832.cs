using Unity.Collections;

public abstract class GClass832
{
	public static NativeArray<T> Create2D<T>(int l0, int l1, Allocator allocator, NativeArrayOptions options = NativeArrayOptions.ClearMemory) where T : struct
	{
		return new NativeArray<T>(l0 * l1, allocator, options);
	}

	public static T[] Create2DArray<T>(int l0, int l1) where T : struct
	{
		return new T[l0 * l1];
	}

	public static void Set2D<T>(this NativeArray<T> self, int l0, int index0, int index1, T value) where T : struct
	{
		self[index1 * l0 + index0] = value;
	}

	public static T Get2D<T>(this NativeArray<T> self, int l0, int index0, int index1) where T : struct
	{
		return self[index1 * l0 + index0];
	}

	public static void Set2D<T>(this T[] self, int l0, int index0, int index1, T value) where T : struct
	{
		self[index1 * l0 + index0] = value;
	}

	public static T Get2D<T>(this T[] self, int l0, int index0, int index1) where T : struct
	{
		return self[index1 * l0 + index0];
	}

	public static bool DisposeIfCreated<T>(this NativeArray<T> self) where T : struct
	{
		if (!self.IsCreated)
		{
			return false;
		}
		self.Dispose();
		return true;
	}
}
