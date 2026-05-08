using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Diz.Binding;

namespace Bsg.GameSettings;

[Serializable]
public abstract class GameSetting<T> : IGameSetting, IBindable<T>
{
	[CompilerGenerated]
	public class Class770
	{
		public Func<T, T> preProcessor;

		public Task<T> method_0(T value)
		{
			return Task.FromResult(preProcessor(value));
		}
	}

	[NonSerialized]
	public static Type SettingType = typeof(T);

	[NonSerialized]
	public static bool IsNullable = default(T) == null;

	[NonSerialized]
	public Func<T, Task<T>> AsyncPreProcessor;

	public string Key { get; }

	public bool IsAvailableToEdit { get; set; }

	public T Value
	{
		get
		{
			return GetValue();
		}
		set
		{
			SetValue(value).HandleExceptions();
		}
	}

	object IGameSetting.ObjectValue
	{
		get
		{
			return Value;
		}
		set
		{
			Value = (T)value;
		}
	}

	Type IGameSetting.ObjectType => SettingType;

	public abstract T GetValue();

	public abstract Task SetValue(T value);

	public bool HasSameValue_1(IGameSetting other)
	{
		if (!(other is GameSetting<T> other2))
		{
			throw new InvalidOperationException("Other IGameSetting should be type GameSetting with " + SettingType.FullName);
		}
		return HasSameValue(other2);
	}

	bool IGameSetting.HasSameValue(IGameSetting other)
	{
		//ILSpy generated this explicit interface implementation from .override directive in HasSameValue_1
		return this.HasSameValue_1(other);
	}

	public void TakeValueFrom_1(IGameSetting other)
	{
		if (!(other is GameSetting<T> other2))
		{
			throw new InvalidOperationException("Other IGameSetting should be type GameSetting with " + SettingType.FullName);
		}
		TakeValueFrom(other2);
	}

	void IGameSetting.TakeValueFrom(IGameSetting other)
	{
		//ILSpy generated this explicit interface implementation from .override directive in TakeValueFrom_1
		this.TakeValueFrom_1(other);
	}

	public GameSetting(string key, Func<T, T> preProcessor)
	{
		Key = key;
		IsAvailableToEdit = true;
		if (preProcessor != null)
		{
			AsyncPreProcessor = (T value) => Task.FromResult(preProcessor(value));
		}
	}

	public GameSetting(string key, Func<T, Task<T>> asyncPreProcessor)
	{
		Key = key;
		IsAvailableToEdit = true;
		AsyncPreProcessor = asyncPreProcessor;
	}

	public Task<T> PreProcessValue(T value)
	{
		if (!IsAvailableToEdit)
		{
			throw new Exception("Setting " + Key + " is not available to edit");
		}
		if (AsyncPreProcessor == null)
		{
			return Task.FromResult(value);
		}
		return AsyncPreProcessor(value);
	}

	public abstract bool HasSameValue(GameSetting<T> other);

	public abstract void TakeValueFrom(GameSetting<T> other);

	public abstract Action Bind(Action<T> handler);

	public abstract Action Subscribe(Action<T> handler);

	public abstract Action BindWithoutValue(IHandler handler);

	public abstract void ResetToDefault();

	public abstract void ForceApply();

	public override string ToString()
	{
		return "GameSetting " + Key + " with " + SettingType.Name + ":" + Value.ToString();
	}

	public static implicit operator T(GameSetting<T> setting)
	{
		return setting.Value;
	}
}
