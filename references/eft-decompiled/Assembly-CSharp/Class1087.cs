using System;
using System.Runtime.CompilerServices;
using System.Threading;

public abstract class Class1087
{
	public class EventArgs0 : EventArgs
	{
		[NonSerialized]
		[CompilerGenerated]
		public Struct273 Struct273_0;

		[NonSerialized]
		[CompilerGenerated]
		public double Double_0;

		[NonSerialized]
		[CompilerGenerated]
		public double Double_1;

		[NonSerialized]
		[CompilerGenerated]
		public bool Bool_0;

		public Struct273 ItemIndex
		{
			[CompilerGenerated]
			get
			{
				return Struct273_0;
			}
			[CompilerGenerated]
			set
			{
				Struct273_0 = value;
			}
		}

		public double OldValue
		{
			[CompilerGenerated]
			get
			{
				return Double_0;
			}
			[CompilerGenerated]
			set
			{
				Double_0 = value;
			}
		}

		public double NewValue
		{
			[CompilerGenerated]
			get
			{
				return Double_1;
			}
			[CompilerGenerated]
			set
			{
				Double_1 = value;
			}
		}

		public bool MinMaxChanged
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

		public EventArgs0(int group, int category, double oldValue, double newValue, bool minMaxChanged)
		{
			ItemIndex = new Struct273(group, category);
			OldValue = oldValue;
			NewValue = newValue;
			MinMaxChanged = minMaxChanged;
		}
	}

	[CompilerGenerated]
	private EventHandler eventHandler_0;

	[CompilerGenerated]
	private EventHandler<EventArgs0> eventHandler_1;

	public abstract Class1073 Columns { get; }

	public abstract Class1074 Rows { get; }

	public event EventHandler DataStructureChanged
	{
		[CompilerGenerated]
		add
		{
			EventHandler eventHandler = eventHandler_0;
			EventHandler eventHandler2;
			do
			{
				eventHandler2 = eventHandler;
				EventHandler value2 = (EventHandler)Delegate.Combine(eventHandler2, value);
				eventHandler = Interlocked.CompareExchange(ref eventHandler_0, value2, eventHandler2);
			}
			while ((object)eventHandler != eventHandler2);
		}
		[CompilerGenerated]
		remove
		{
			EventHandler eventHandler = eventHandler_0;
			EventHandler eventHandler2;
			do
			{
				eventHandler2 = eventHandler;
				EventHandler value2 = (EventHandler)Delegate.Remove(eventHandler2, value);
				eventHandler = Interlocked.CompareExchange(ref eventHandler_0, value2, eventHandler2);
			}
			while ((object)eventHandler != eventHandler2);
		}
	}

	public event EventHandler<EventArgs0> DataValueChanged
	{
		[CompilerGenerated]
		add
		{
			EventHandler<EventArgs0> eventHandler = eventHandler_1;
			EventHandler<EventArgs0> eventHandler2;
			do
			{
				eventHandler2 = eventHandler;
				EventHandler<EventArgs0> value2 = (EventHandler<EventArgs0>)Delegate.Combine(eventHandler2, value);
				eventHandler = Interlocked.CompareExchange(ref eventHandler_1, value2, eventHandler2);
			}
			while ((object)eventHandler != eventHandler2);
		}
		[CompilerGenerated]
		remove
		{
			EventHandler<EventArgs0> eventHandler = eventHandler_1;
			EventHandler<EventArgs0> eventHandler2;
			do
			{
				eventHandler2 = eventHandler;
				EventHandler<EventArgs0> value2 = (EventHandler<EventArgs0>)Delegate.Remove(eventHandler2, value);
				eventHandler = Interlocked.CompareExchange(ref eventHandler_1, value2, eventHandler2);
			}
			while ((object)eventHandler != eventHandler2);
		}
	}

	public void method_0()
	{
		if (eventHandler_0 != null)
		{
			eventHandler_0(this, EventArgs.Empty);
		}
	}

	public void method_1(EventArgs0 data)
	{
		if (eventHandler_1 != null)
		{
			eventHandler_1(this, data);
		}
	}

	public abstract double[,] getRawData();

	public Class1087()
	{
	}
}
