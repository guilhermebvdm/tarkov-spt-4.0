using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

public class GClass705
{
	[CompilerGenerated]
	private Action<MineDirectional> action_0;

	public List<MineDirectional> Mines => MineDirectional.Mines;

	public event Action<MineDirectional> OnExplosion
	{
		[CompilerGenerated]
		add
		{
			Action<MineDirectional> action = action_0;
			Action<MineDirectional> action2;
			do
			{
				action2 = action;
				Action<MineDirectional> value2 = (Action<MineDirectional>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<MineDirectional> action = action_0;
			Action<MineDirectional> action2;
			do
			{
				action2 = action;
				Action<MineDirectional> value2 = (Action<MineDirectional>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public GClass705()
	{
		MineDirectional.Mines.Clear();
	}

	public void OnMineExplosion(MineDirectional mineDirectional)
	{
		action_0?.Invoke(mineDirectional);
	}

	public IEnumerable<MineDirectional> method_0(string tag)
	{
		if (string.IsNullOrEmpty(tag))
		{
			yield break;
		}
		foreach (MineDirectional mine in Mines)
		{
			if (mine != null && !string.IsNullOrEmpty(mine.Tag) && mine.Tag.Equals(tag, StringComparison.InvariantCultureIgnoreCase))
			{
				yield return mine;
			}
		}
	}

	public void SetArmedState(string tag, bool state)
	{
		foreach (MineDirectional item in method_0(tag))
		{
			item.SetArmed(state);
		}
	}
}
