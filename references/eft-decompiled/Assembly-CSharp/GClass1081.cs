using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Bsg.GameSettings;
using JetBrains.Annotations;

public abstract class GClass1081<T> where T : GClass1081<T>
{
	[CompilerGenerated]
	public class Class773<U>
	{
		public Func<U, U> preProcessor;

		public Task<U> method_0(U value)
		{
			return Task.FromResult(preProcessor(value));
		}
	}

	[NonSerialized]
	public List<IGameSetting> List_0 = new List<IGameSetting>();

	public IList<IGameSetting> GetSettingsList()
	{
		return List_0;
	}

	public bool HasSameSettings([NotNull] T other)
	{
		int num = 0;
		while (true)
		{
			if (num < List_0.Count)
			{
				if (!List_0[num].HasSameValue(other.List_0[num]))
				{
					break;
				}
				num++;
				continue;
			}
			return true;
		}
		return false;
	}

	public void TakeSettingsFrom([NotNull] T other)
	{
		for (int i = 0; i < List_0.Count; i++)
		{
			List_0[i].TakeValueFrom(other.List_0[i]);
		}
	}

	public void ForceApply()
	{
		foreach (IGameSetting item in List_0)
		{
			item.ForceApply();
		}
	}

	public bool TryForceApplyDiff([NotNull] T other)
	{
		bool result = false;
		for (int i = 0; i < List_0.Count; i++)
		{
			if (!List_0[i].HasSameValue(other.List_0[i]))
			{
				List_0[i].ForceApply();
				result = true;
			}
		}
		return result;
	}

	[NotNull]
	public abstract T Clone();

	public V method_0<U, V>([NotNull] V setting) where V : GameSetting<U>
	{
		List_0.Add(setting);
		return setting;
	}

	public GameSetting<U> method_1<U>([NotNull] GameSetting<U> setting)
	{
		return method_0<U, GameSetting<U>>(setting);
	}

	public GameSetting<U> method_2<U>(string key, U defaultValue = default(U), Func<U, U> preProcessor = null) where U : IEquatable<U>
	{
		return method_1(new EquatableGameSetting<U>(key, defaultValue, preProcessor));
	}

	public GameSetting<U> method_3<U>(string key, U defaultValue, Func<U, Task<U>> preProcessor) where U : IEquatable<U>
	{
		return method_1(new EquatableGameSetting<U>(key, defaultValue, preProcessor));
	}

	public GameSetting<U> method_4<U>(string key, U defaultValue = default(U), Func<U, U> preProcessor = null)
	{
		Func<U, Task<U>> preProcessor2 = ((preProcessor != null) ? ((Func<U, Task<U>>)((U value) => Task.FromResult(preProcessor(value)))) : null);
		return method_5(key, defaultValue, preProcessor2);
	}

	public GameSetting<U> method_5<U>(string key, U defaultValue, Func<U, Task<U>> preProcessor)
	{
		return method_1(new StateGameSetting<U>(key, defaultValue, preProcessor));
	}

	public ListGameSetting<U> method_6<U>(string key, ListGameSetting<U>.GDelegate37 copyItem = null, ListGameSetting<U>.GDelegate36 equalityCheck = null, Func<List<U>, List<U>> preProcessor = null)
	{
		return method_0<List<U>, ListGameSetting<U>>(new ListGameSetting<U>(key, copyItem, equalityCheck, preProcessor));
	}

	[Conditional("DEBUG")]
	public void method_7(T other)
	{
		if (other.GetType() != GetType())
		{
			throw new ArgumentException("Type mismatch!");
		}
		if (other.List_0.Count != List_0.Count)
		{
			throw new ArgumentException("Settings count mismatch!");
		}
		int num = 0;
		while (true)
		{
			if (num < List_0.Count)
			{
				IGameSetting gameSetting = List_0[num];
				IGameSetting gameSetting2 = other.List_0[num];
				if (gameSetting.ObjectType != gameSetting2.ObjectType)
				{
					break;
				}
				num++;
				continue;
			}
			return;
		}
		throw new ArgumentException($"Type of setting(index = {num}) mismatch!");
	}

	public GClass1081()
	{
	}
}
