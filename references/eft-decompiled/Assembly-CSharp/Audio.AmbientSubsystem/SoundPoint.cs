using System;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

namespace Audio.AmbientSubsystem;

public class SoundPoint : MonoBehaviour, GInterface96
{
	[CompilerGenerated]
	private Action<int> action_0;

	[CompilerGenerated]
	private Action<int> action_1;

	[CompilerGenerated]
	private bool bool_0;

	public bool IsOccupied
	{
		[CompilerGenerated]
		get
		{
			return bool_0;
		}
		[CompilerGenerated]
		set
		{
			bool_0 = value;
		}
	}

	public event Action<int> Occupied
	{
		[CompilerGenerated]
		add
		{
			Action<int> action = action_0;
			Action<int> action2;
			do
			{
				action2 = action;
				Action<int> value2 = (Action<int>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<int> action = action_0;
			Action<int> action2;
			do
			{
				action2 = action;
				Action<int> value2 = (Action<int>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<int> Released
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

	public void Occupy()
	{
		action_0?.Invoke(GetInstanceID());
		IsOccupied = true;
	}

	public void Release()
	{
		IsOccupied = false;
		action_1?.Invoke(GetInstanceID());
	}
}
