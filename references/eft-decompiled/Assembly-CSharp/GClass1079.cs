using System;
using System.Threading.Tasks;
using Bsg.GameSettings;
using Diz.Binding;

public class GClass1079<T> : GInterface62<T>, IBindable<T>
{
	[NonSerialized]
	public GameSetting<T> GameSetting_0;

	[NonSerialized]
	public Func<T, Task<T>> Func_0;

	T IBindable<T>.Value => GameSetting_0.Value;

	public GClass1079(GameSetting<T> gameSetting, Func<T, Task<T>> setter = null)
	{
		GameSetting_0 = gameSetting;
		Func_0 = setter ?? new Func<T, Task<T>>(method_0);
	}

	public Action Bind(Action<T> handler)
	{
		return GameSetting_0.Bind(handler);
	}

	Action IBindable<T>.Bind(Action<T> handler)
	{
		//ILSpy generated this explicit interface implementation from .override directive in Bind
		return this.Bind(handler);
	}

	public Action Subscribe(Action<T> handler)
	{
		return GameSetting_0.Subscribe(handler);
	}

	Action IBindable<T>.Subscribe(Action<T> handler)
	{
		//ILSpy generated this explicit interface implementation from .override directive in Subscribe
		return this.Subscribe(handler);
	}

	public Action BindWithoutValue(IHandler handler)
	{
		return GameSetting_0.BindWithoutValue(handler);
	}

	Action IBindable<T>.BindWithoutValue(IHandler handler)
	{
		//ILSpy generated this explicit interface implementation from .override directive in BindWithoutValue
		return this.BindWithoutValue(handler);
	}

	public Task<T> Set(T t)
	{
		return Func_0(t);
	}

	Task<T> GInterface62<T>.Set(T t)
	{
		//ILSpy generated this explicit interface implementation from .override directive in Set
		return this.Set(t);
	}

	public Task<T> method_0(T t)
	{
		GameSetting_0.Value = t;
		return Task.FromResult(GameSetting_0.Value);
	}
}
