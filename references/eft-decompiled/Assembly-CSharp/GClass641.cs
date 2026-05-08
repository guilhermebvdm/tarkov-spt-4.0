using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

public class GClass641
{
	public interface IBotTimer
	{
		float Duration { get; }

		float StartTime { get; }

		float EndTime { get; }

		float TimeLeft { get; }

		bool IsLoopped { get; }

		bool IsActive { get; }

		event Action OnTimer;

		void Stop(bool shallFire = false);

		void Restart(float endTime);
	}

	public class Class297 : IBotTimer
	{
		[NonSerialized]
		public Action Action_0;

		[NonSerialized]
		public Action Action_1;

		[CompilerGenerated]
		private Action action_2;

		[NonSerialized]
		[CompilerGenerated]
		public float Float_0;

		[NonSerialized]
		[CompilerGenerated]
		public float Float_1;

		[NonSerialized]
		[CompilerGenerated]
		public bool Bool_0;

		[NonSerialized]
		[CompilerGenerated]
		public bool Bool_1;

		public float EndTime
		{
			[CompilerGenerated]
			get
			{
				return Float_0;
			}
			[CompilerGenerated]
			set
			{
				Float_0 = value;
			}
		}

		public float Duration
		{
			[CompilerGenerated]
			get
			{
				return Float_1;
			}
			[CompilerGenerated]
			set
			{
				Float_1 = value;
			}
		}

		public float TimeLeft => EndTime - Time.time;

		public float StartTime => EndTime - Duration;

		public bool IsLoopped
		{
			[CompilerGenerated]
			get
			{
				return Bool_0;
			}
			[CompilerGenerated]
			set
			{
				Bool_0 = value;
			}
		}

		public bool IsActive
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

		public event Action OnTimer
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

		public void Init(Action onStop, Action onStart)
		{
			Action_0 = onStop;
			Action_1 = onStart;
		}

		public void Start(float endTime, float duration, bool isLopped = false)
		{
			EndTime = endTime;
			Duration = duration;
			IsLoopped = isLopped;
			Action_1();
			IsActive = true;
		}

		public void Stop(bool shallFire = false)
		{
			if (!IsActive)
			{
				Debug.LogError("Trying to stop inactive timer.");
				return;
			}
			Action_0();
			IsActive = false;
			if (shallFire)
			{
				Fire();
			}
		}

		public void Fire()
		{
			try
			{
				if (action_2 != null)
				{
					action_2();
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				if (IsActive)
				{
					Stop();
				}
			}
		}

		public void Restart(float endTime)
		{
			Action_0();
			EndTime = endTime;
			Action_1();
		}
	}

	[CompilerGenerated]
	public class Class298
	{
		public GClass641 gclass641_0;

		public Class297 timer;

		public void method_0()
		{
			gclass641_0.LinkedList_0.Remove(timer);
		}

		public void method_1()
		{
			gclass641_0.method_0(timer);
		}
	}

	[NonSerialized]
	public LinkedList<Class297> LinkedList_0 = new LinkedList<Class297>();

	public IBotTimer MakeTimer(float endTime, float duration, bool isLopped = false)
	{
		Class297 timer = new Class297();
		timer.Init(delegate
		{
			LinkedList_0.Remove(timer);
		}, delegate
		{
			method_0(timer);
		});
		timer.Start(endTime, duration, isLopped);
		return timer;
	}

	public IBotTimer MakeTimer(TimeSpan duration, bool isLopped = false)
	{
		float num = (float)duration.TotalSeconds;
		return MakeTimer(Time.time + num, num, isLopped);
	}

	public void Update()
	{
		LinkedListNode<Class297> linkedListNode = LinkedList_0.First;
		while (linkedListNode != null)
		{
			LinkedListNode<Class297> next = linkedListNode.Next;
			if (Time.time > linkedListNode.Value.EndTime)
			{
				Class297 value = linkedListNode.Value;
				if (value.IsLoopped)
				{
					value.Restart(value.EndTime + value.Duration);
				}
				else
				{
					value.Stop();
				}
				value.Fire();
				linkedListNode = next;
				continue;
			}
			break;
		}
	}

	public void method_0(Class297 timer)
	{
		LinkedListNode<Class297> linkedListNode = LinkedList_0.First;
		if (linkedListNode == null)
		{
			LinkedList_0.AddFirst(timer);
			return;
		}
		while (true)
		{
			if (linkedListNode != null)
			{
				LinkedListNode<Class297> next = linkedListNode.Next;
				if (!(linkedListNode.Value.EndTime <= timer.EndTime))
				{
					break;
				}
				linkedListNode = next;
				continue;
			}
			LinkedList_0.AddLast(timer);
			return;
		}
		LinkedList_0.AddBefore(linkedListNode, timer);
	}

	public void StopAllTimers()
	{
		foreach (Class297 item in LinkedList_0.ToList())
		{
			item.Stop();
		}
		LinkedList_0.Clear();
	}
}
