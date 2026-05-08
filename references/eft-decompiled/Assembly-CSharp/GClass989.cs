using System;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

public abstract class GClass989
{
	[Serializable]
	[CompilerGenerated]
	public class Class681
	{
		public static readonly Class681 class681_0 = new Class681();

		public void method_0()
		{
		}
	}

	[NonSerialized]
	[CompilerGenerated]
	public static Vector2 Vector2_0;

	[NonSerialized]
	[CompilerGenerated]
	public static Rect Rect_0;

	[NonSerialized]
	[CompilerGenerated]
	public static int Int_0;

	[NonSerialized]
	[CompilerGenerated]
	public static int Int_1;

	[CompilerGenerated]
	private static Action action_0 = delegate
	{
	};

	public static Vector2 Size
	{
		[CompilerGenerated]
		get
		{
			return Vector2_0;
		}
		[CompilerGenerated]
		set
		{
			Vector2_0 = value;
		}
	}

	public static Rect Rect
	{
		[CompilerGenerated]
		get
		{
			return Rect_0;
		}
		[CompilerGenerated]
		set
		{
			Rect_0 = value;
		}
	}

	public static int Width
	{
		[CompilerGenerated]
		get
		{
			return Int_0;
		}
		[CompilerGenerated]
		set
		{
			Int_0 = value;
		}
	}

	public static int Height
	{
		[CompilerGenerated]
		get
		{
			return Int_1;
		}
		[CompilerGenerated]
		set
		{
			Int_1 = value;
		}
	}

	public static event Action OnScreenChange
	{
		[CompilerGenerated]
		add
		{
			Action action = action_0;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action action = action_0;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public static void Check()
	{
		if (Width != Screen.width || Height != Screen.height)
		{
			Width = Screen.width;
			Height = Screen.height;
			Size = new Vector2(Width, Height);
			Rect = new Rect(0f, 0f, Width, Height);
			action_0();
		}
	}
}
