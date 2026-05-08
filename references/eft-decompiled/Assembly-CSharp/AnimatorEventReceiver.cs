using System;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

[Obsolete]
public class AnimatorEventReceiver : MonoBehaviour
{
	[CompilerGenerated]
	private Action action_0;

	[CompilerGenerated]
	private Action<int> action_1;

	[CompilerGenerated]
	private Action action_2;

	[CompilerGenerated]
	private Action action_3;

	[CompilerGenerated]
	private Action action_4;

	[CompilerGenerated]
	private Action action_5;

	[CompilerGenerated]
	private Action action_6;

	[CompilerGenerated]
	private Action action_7;

	[CompilerGenerated]
	private Action action_8;

	[CompilerGenerated]
	private Action action_9;

	public event Action OnFire
	{
		[CompilerGenerated]
		add
		{
			Action action = action_0;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action action = action_0;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<int> OnChangeFireMode
	{
		[CompilerGenerated]
		add
		{
			Action<int> action = action_1;
			Action<int> action2;
			do
			{
				action2 = action;
				Action<int> value2 = (Action<int>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<int> action = action_1;
			Action<int> action2;
			do
			{
				action2 = action;
				Action<int> value2 = (Action<int>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action OnDelAmmoChamber
	{
		[CompilerGenerated]
		add
		{
			Action action = action_2;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_2, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action action = action_2;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_2, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action OnDelAmmoFromMag
	{
		[CompilerGenerated]
		add
		{
			Action action = action_3;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_3, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action action = action_3;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_3, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action OnShellEject
	{
		[CompilerGenerated]
		add
		{
			Action action = action_4;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_4, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action action = action_4;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_4, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action OnAddAmmoInChamber
	{
		[CompilerGenerated]
		add
		{
			Action action = action_5;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_5, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action action = action_5;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_5, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action OnOnBoltCatch
	{
		[CompilerGenerated]
		add
		{
			Action action = action_6;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_6, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action action = action_6;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_6, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action OnOffBoltCatch
	{
		[CompilerGenerated]
		add
		{
			Action action = action_7;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_7, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action action = action_7;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_7, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action OnMagOut
	{
		[CompilerGenerated]
		add
		{
			Action action = action_8;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_8, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action action = action_8;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_8, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action OnMagIn
	{
		[CompilerGenerated]
		add
		{
			Action action = action_9;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_9, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action action = action_9;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_9, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public void FiringBullet()
	{
		if (action_0 != null)
		{
			action_0();
		}
		Debug.Log("FIRE");
	}

	public void DelAmmoChamber()
	{
		if (action_2 != null)
		{
			action_2();
		}
	}

	public void ShellEject()
	{
		if (action_4 != null)
		{
			action_4();
		}
	}

	public void DelAmmoFromMag()
	{
		if (action_3 != null)
		{
			action_3();
		}
	}

	public void AddAmmoInChamber()
	{
		if (action_5 != null)
		{
			action_5();
		}
	}

	public void OnBoltCatch()
	{
		if (action_6 != null)
		{
			action_6();
		}
	}

	public void OffBoltCatch()
	{
		if (action_7 != null)
		{
			action_7();
		}
	}

	public void SetFiremode1()
	{
		if (action_1 != null)
		{
			action_1(1);
		}
	}

	public void SetFiremode0()
	{
		if (action_1 != null)
		{
			action_1(0);
		}
	}

	public void MagOut()
	{
		if (action_8 != null)
		{
			action_8();
		}
	}

	public void MagIn()
	{
		if (action_9 != null)
		{
			action_9();
		}
	}
}
