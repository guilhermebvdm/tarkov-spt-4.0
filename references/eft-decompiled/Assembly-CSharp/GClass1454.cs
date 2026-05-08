using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

public class GClass1454
{
	public class Class924
	{
		[Serializable]
		[CompilerGenerated]
		public class Class925
		{
			public static readonly Class925 class925_0 = new Class925();

			public void method_0(bool s, bool e)
			{
			}
		}

		public Enum My;

		public GDelegate44 Action;

		public float Duration;

		public Dictionary<int, Class926> Events = new Dictionary<int, Class926>();

		public int OnTimeEndState;

		[NonSerialized]
		public static GDelegate44 Gdelegate44_0 = delegate
		{
		};

		public void Initialize(Enum my, GDelegate44 action, float duration, int onTimeEndState)
		{
			Duration = duration;
			OnTimeEndState = onTimeEndState;
			My = my;
			Action = action ?? Gdelegate44_0;
		}
	}

	public class Class926
	{
		public class Class927
		{
			public Func<bool> _action;

			public int _state;
		}

		[NonSerialized]
		public List<Class927> List_0 = new List<Class927>();

		[NonSerialized]
		public Func<Enum> Func_0;

		public int DefaultState = -1;

		public int GetNextState()
		{
			if (Func_0 != null)
			{
				return Convert.ToInt32(Func_0());
			}
			foreach (Class927 item in List_0)
			{
				if (item._action())
				{
					return item._state;
				}
			}
			return DefaultState;
		}

		public void AddCase(Func<bool> action, int state)
		{
			List_0.Add(new Class927
			{
				_action = action,
				_state = state
			});
		}

		public void SetFunc(Func<Enum> func)
		{
			Func_0 = func;
		}
	}

	public delegate void GDelegate44(bool start, bool end);

	[Serializable]
	[CompilerGenerated]
	public class Class928
	{
		public static readonly Class928 class928_0 = new Class928();

		public static Action<Enum, float> action_0;

		public void method_0(Enum e, float f)
		{
		}
	}

	[NonSerialized]
	public Class924[] Class924_0;

	[CompilerGenerated]
	private Action<Enum, float> action_0 = delegate
	{
	};

	[NonSerialized]
	public Class924 Class924_1;

	[NonSerialized]
	public Class924 Class924_2;

	[NonSerialized]
	public Class924 Class924_3;

	[NonSerialized]
	public float Float_0;

	public Enum CurrentState => Class924_1.My;

	public event Action<Enum, float> OnStartState
	{
		[CompilerGenerated]
		add
		{
			Action<Enum, float> action = action_0;
			Action<Enum, float> action2;
			do
			{
				action2 = action;
				Action<Enum, float> value2 = (Action<Enum, float>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<Enum, float> action = action_0;
			Action<Enum, float> action2;
			do
			{
				action2 = action;
				Action<Enum, float> value2 = (Action<Enum, float>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public GClass1454(Type stateEnumType, Enum defaultEvent)
	{
		int length = Enum.GetValues(stateEnumType).Length;
		Class924_0 = new Class924[length];
		for (int num = 0; num < length; num++)
		{
			Class924_0[num] = new Class924();
		}
		Class924_1 = Class924_0[Convert.ToInt32(defaultEvent)];
	}

	public void Update()
	{
		bool flag = Class924_2 != null;
		if (Class924_1 != null)
		{
			bool start = false;
			bool end = false;
			if (flag && (start = Class924_1.Duration > 0f || Class924_1 != Class924_2))
			{
				Class924_1 = Class924_2;
				Class924_2 = null;
				Class924_3 = null;
				method_1(Class924_1.My, Class924_1.Duration);
			}
			if (method_2())
			{
				Class924_2 = Class924_3 ?? Class924_0[Class924_1.OnTimeEndState];
				Class924_3 = null;
				end = true;
			}
			if (Class924_1.Action != null)
			{
				Class924_1.Action(start, end);
			}
		}
	}

	public void DoEvent(Enum @event)
	{
		int key = Convert.ToInt32(@event);
		if (Class924_1.Events.TryGetValue(key, out var value))
		{
			int nextState = value.GetNextState();
			if (nextState != -1)
			{
				Class924_3 = Class924_0[nextState];
			}
		}
	}

	public void SetNextState(Enum state)
	{
		Class924_2 = Class924_0[Convert.ToInt32(state)];
	}

	public void SetStateImmediate(Enum state)
	{
		Class924_2 = Class924_0[Convert.ToInt32(state)];
		Class924_3 = null;
		Float_0 = 0f;
	}

	public void AddState(Enum state, float duration, Enum onTimeEndState, GDelegate44 action = null)
	{
		Class924_0[Convert.ToInt32(state)].Initialize(state, action, duration, Convert.ToInt32(onTimeEndState));
	}

	public void AddState(Enum state, GDelegate44 action = null)
	{
		int num = Convert.ToInt32(state);
		Class924_0[num].Initialize(state, action, -1f, num);
	}

	public void AddTransition(Enum fromState, Enum @event, Func<Enum> action)
	{
		Dictionary<int, Class926> events = Class924_0[Convert.ToInt32(fromState)].Events;
		int key = Convert.ToInt32(@event);
		if (!events.TryGetValue(key, out var value))
		{
			events.Add(key, value = new Class926());
		}
		value.SetFunc(action);
	}

	public void AddTransition(Enum fromState, Enum @event, Func<bool> action, Enum toState)
	{
		method_0(fromState, @event, toState, action);
	}

	public void AddTransition(Enum fromState, Enum @event, Enum toState)
	{
		method_0(fromState, @event, toState, null);
	}

	public void method_0(Enum fromState, Enum @event, Enum toState, Func<bool> action)
	{
		Dictionary<int, Class926> events = Class924_0[Convert.ToInt32(fromState)].Events;
		int key = Convert.ToInt32(@event);
		if (!events.TryGetValue(key, out var value))
		{
			events.Add(key, value = new Class926());
		}
		if (action == null)
		{
			value.DefaultState = Convert.ToInt32(toState);
		}
		else
		{
			value.AddCase(action, Convert.ToInt32(toState));
		}
	}

	public void method_1(Enum state, float duration)
	{
		float num = ((Time.time < Float_0) ? Float_0 : Time.time);
		Float_0 = num + duration;
		action_0(state, num);
	}

	public void FixCurrentStateDuration(float newDuration)
	{
		Float_0 = Float_0 - Class924_1.Duration + newDuration;
	}

	public void SetStateDuration(Enum state, float newDuration)
	{
		Class924_0[Convert.ToInt32(state)].Duration = newDuration;
	}

	public bool method_2()
	{
		return Time.time + 0.0625f >= Float_0;
	}
}
