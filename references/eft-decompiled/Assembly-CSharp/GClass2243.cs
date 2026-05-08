public abstract class GClass2243
{
	public static T? GetNested<T>(this T t) where T : struct, GInterface217<T>
	{
		return (T?)(object)t.Nested;
	}

	public static void AttachTo<T>(this T nested, ref T? t) where T : struct, GInterface217<T>
	{
		if (!t.HasValue)
		{
			t = nested;
			return;
		}
		T valueOrDefault = t.GetValueOrDefault();
		t = smethod_0(valueOrDefault, ref nested);
	}

	public static T smethod_0<T>(this T t, ref T nested) where T : GInterface217<T>
	{
		if (t.Nested == null)
		{
			object nested2 = nested;
			t.Nested = (GInterface217<T>)nested2;
		}
		else
		{
			smethod_1(t.Nested, ref nested);
		}
		return t;
	}

	public static void smethod_1<T>(this GInterface217<T> t, ref T nested) where T : GInterface217<T>
	{
		while (t.Nested != null)
		{
			t = t.Nested;
		}
		t.Nested = nested;
	}

	public static void AttachToOrUpdate<T>(this ref T child, ref T? root) where T : struct, GInterface216<T>
	{
		if (!root.HasValue)
		{
			root = child;
			return;
		}
		T root2 = root.GetValueOrDefault();
		root = smethod_2(ref root2, ref child);
	}

	public static T smethod_2<T>(this ref T root, ref T child) where T : struct, GInterface216<T>
	{
		if (root.TryUpdate(child))
		{
			return root;
		}
		if (root.Nested == null)
		{
			return smethod_0(root, ref child);
		}
		T root2 = GetNested(root).GetValueOrDefault();
		root.Nested = smethod_2(ref root2, ref child);
		return root;
	}
}
