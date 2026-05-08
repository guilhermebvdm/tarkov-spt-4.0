using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Bsg.GameSettings;

public class GClass1078<T, U> : GameSetting<U>
{
	[CompilerGenerated]
	public class Class771
	{
		public GClass1078<T, U> gclass1078_0;

		public Action<U> handler;

		public void method_0(T baseValue)
		{
			U obj = gclass1078_0.Func_0(baseValue);
			handler?.Invoke(obj);
		}
	}

	[NonSerialized]
	public Func<T, U> Func_0;

	[NonSerialized]
	public Func<U, Task> Func_1;

	[NonSerialized]
	public GameSetting<T> GameSetting_0;

	public GClass1078(GameSetting<T> baseSetting, Func<T, U> propertyGetter, Func<U, Task> propertySetter)
		: base(string.Empty, (Func<U, U>)null)
	{
		GameSetting_0 = baseSetting;
		Func_0 = propertyGetter;
		Func_1 = propertySetter;
	}

	public override U GetValue()
	{
		return Func_0(GameSetting_0);
	}

	public override Task SetValue(U value)
	{
		return Func_1(value);
	}

	public override void TakeValueFrom(GameSetting<U> other)
	{
		Func_1(other.Value).HandleExceptions();
	}

	public override Action Bind(Action<U> handler)
	{
		return GameSetting_0.Bind(method_0(handler));
	}

	public override Action Subscribe(Action<U> handler)
	{
		return GameSetting_0.Subscribe(method_0(handler));
	}

	public override Action BindWithoutValue(IHandler handler)
	{
		return GameSetting_0.BindWithoutValue(handler);
	}

	public override void ResetToDefault()
	{
	}

	public override void ForceApply()
	{
		GameSetting_0.ForceApply();
	}

	public override bool HasSameValue(GameSetting<U> other)
	{
		if (GameSetting<U>.IsNullable)
		{
			bool flag = base.Value == null;
			bool flag2 = other.Value == null;
			if (flag || flag2)
			{
				return flag == flag2;
			}
		}
		return other.Value.Equals(base.Value);
	}

	public Action<T> method_0(Action<U> handler)
	{
		return delegate(T baseValue)
		{
			U obj = Func_0(baseValue);
			handler?.Invoke(obj);
		};
	}
}
