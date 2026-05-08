using System;
using System.Runtime.CompilerServices;

public abstract class TalkClass
{
	[NonSerialized]
	[CompilerGenerated]
	public static DateTime DateTime_0;

	[NonSerialized]
	[CompilerGenerated]
	public static bool Bool_0;

	public static DateTime DateTime
	{
		[CompilerGenerated]
		get
		{
			return DateTime_0;
		}
		[CompilerGenerated]
		set
		{
			DateTime_0 = value;
		}
	}

	public static bool Blocked
	{
		[CompilerGenerated]
		get
		{
			return Bool_0;
		}
		[CompilerGenerated]
		set
		{
			Bool_0 = value;
		}
	}

	public static bool IsTalkDetected()
	{
		return (DateTime.UtcNow - DateTime).TotalMilliseconds <= 200.0;
	}

	public static void SetTalkDateTime()
	{
		DateTime = DateTime.UtcNow;
	}
}
