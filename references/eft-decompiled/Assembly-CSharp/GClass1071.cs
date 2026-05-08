using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Bsg.GameSettings;

public abstract class GClass1071<T> : GInterface65<T>, IDisposable where T : GClass1081<T>
{
	[NonSerialized]
	public T Gparam_0;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_1;

	[NonSerialized]
	public List<Action> List_0 = new List<Action>();

	public bool Boolean_0
	{
		[CompilerGenerated]
		get
		{
			return Bool_1;
		}
		[CompilerGenerated]
		set
		{
			Bool_1 = value;
		}
	}

	public bool IsBound => Gparam_0 != null;

	public void BindTo(T settingsGroup, bool previewMode = false)
	{
		if (IsBound)
		{
			throw new Exception("This BaseSettingsController for " + typeof(T).Name + " is already bound!");
		}
		Gparam_0 = settingsGroup;
		Bool_0 = previewMode;
		Boolean_0 = true;
		CreateBindings();
		Boolean_0 = false;
	}

	public abstract void CreateBindings();

	public abstract T SettingsGroupFactory();

	public void method_0<U>(GameSetting<U> gameSetting, Action<U> handler)
	{
		method_2(gameSetting.Bind(handler));
	}

	public void method_1<U>(GameSetting<U> gameSetting, Action<U> handler)
	{
		method_2(gameSetting.Subscribe(handler));
	}

	public void method_2(Action action)
	{
		List_0.Add(action);
	}

	public void Dispose()
	{
		foreach (Action item in List_0)
		{
			item();
		}
		Gparam_0 = null;
	}

	public GClass1071()
	{
	}
}
