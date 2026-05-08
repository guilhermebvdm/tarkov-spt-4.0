using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Comfort.Common;
using EFT;
using JetBrains.Annotations;
using NLog;

public class Class443
{
	public class Class388 : LoggerClass
	{
		public Class388(LoggerMode mode)
			: base(LogManager.GetLogger("seasons"), mode)
		{
		}
	}

	[CompilerGenerated]
	private static Action<GInterface29> action_0;

	[NonSerialized]
	[CompilerGenerated]
	public static LoggerClass LoggerClass = new Class388(LoggerMode.Add);

	[CanBeNull]
	public static GInterface29 Controller
	{
		get
		{
			if (!(Singleton<GameWorld>.Instance != null))
			{
				return null;
			}
			return Singleton<GameWorld>.Instance.GInterface29_0;
		}
	}

	public static LoggerClass Logger
	{
		[CompilerGenerated]
		get
		{
			return LoggerClass;
		}
	}

	public static event Action<GInterface29> OnInitialized
	{
		[CompilerGenerated]
		add
		{
			Action<GInterface29> action = action_0;
			Action<GInterface29> action2;
			do
			{
				action2 = action;
				Action<GInterface29> value2 = (Action<GInterface29>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<GInterface29> action = action_0;
			Action<GInterface29> action2;
			do
			{
				action2 = action;
				Action<GInterface29> value2 = (Action<GInterface29>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public static void smethod_0(GInterface29 controller)
	{
		action_0?.Invoke(controller);
	}
}
