using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

public class GClass1080<T> where T : GClass1081<T>, new()
{
	[NonSerialized]
	public GInterface64 Ginterface64_0;

	[NonSerialized]
	[CompilerGenerated]
	public string String_0;

	public string Name
	{
		[CompilerGenerated]
		get
		{
			return String_0;
		}
	}

	public GClass1080(string name, GInterface64 provider)
	{
		Ginterface64_0 = provider;
		String_0 = name;
	}

	public Task<T> Load(T settingsGroup)
	{
		return Ginterface64_0.Load(settingsGroup);
	}

	public Task Save(T settingsGroup)
	{
		return Ginterface64_0.Save(settingsGroup);
	}
}
