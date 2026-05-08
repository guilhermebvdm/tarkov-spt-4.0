using System;
using System.Runtime.CompilerServices;
using System.Threading;
using ChartAndGraph;

public abstract class Class1084 : GInterface163
{
	[NonSerialized]
	public string String_0;

	[NonSerialized]
	public string String_1;

	[NonSerialized]
	[CompilerGenerated]
	public object Object_0;

	[NonSerialized]
	[CompilerGenerated]
	public ChartDynamicMaterial ChartDynamicMaterial_0;

	[CompilerGenerated]
	private Action<string, GInterface163> action_0;

	public string Name
	{
		get
		{
			return String_0;
		}
		set
		{
			String_1 = String_0;
			String_0 = value;
			if (action_0 != null)
			{
				action_0(String_1, this);
			}
		}
	}

	public object UserData
	{
		[CompilerGenerated]
		get
		{
			return Object_0;
		}
		[CompilerGenerated]
		set
		{
			Object_0 = value;
		}
	}

	public ChartDynamicMaterial Material
	{
		[CompilerGenerated]
		get
		{
			return ChartDynamicMaterial_0;
		}
		[CompilerGenerated]
		set
		{
			ChartDynamicMaterial_0 = value;
		}
	}

	public event Action<string, GInterface163> NameChanged
	{
		[CompilerGenerated]
		add
		{
			Action<string, GInterface163> action = action_0;
			Action<string, GInterface163> action2;
			do
			{
				action2 = action;
				Action<string, GInterface163> value2 = (Action<string, GInterface163>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<string, GInterface163> action = action_0;
			Action<string, GInterface163> action2;
			do
			{
				action2 = action;
				Action<string, GInterface163> value2 = (Action<string, GInterface163>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public Class1084(string name)
	{
		String_0 = name;
	}

	public void CancelNameChange()
	{
		String_0 = String_1;
	}
}
