using System;
using System.Runtime.CompilerServices;
using EFT;
using NLog;

public class GClass710 : LoggerClass
{
	[NonSerialized]
	[CompilerGenerated]
	public static GClass710 Gclass710_0 = new GClass710();

	public static GClass710 Instance
	{
		[CompilerGenerated]
		get
		{
			return Gclass710_0;
		}
	}

	public GClass710()
		: base("botProfiles", LoggerMode.Add)
	{
	}

	public void method_3(Profile profile)
	{
		if (IsEnabled(LogLevel.Info))
		{
			LogInfo("Profile Activate:{0}", new CompleteProfileDescriptorClass(profile, GClass2240.Instance));
		}
	}

	public void method_4(Profile profile)
	{
		if (IsEnabled(LogLevel.Info))
		{
			LogInfo("ProfileAddedFromBackend:{0}", new CompleteProfileDescriptorClass(profile, GClass2240.Instance));
		}
	}
}
