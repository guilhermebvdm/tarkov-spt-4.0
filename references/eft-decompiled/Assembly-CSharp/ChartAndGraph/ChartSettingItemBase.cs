using System;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

namespace ChartAndGraph;

[RequireComponent(typeof(AnyChart))]
[ExecuteInEditMode]
public abstract class ChartSettingItemBase : MonoBehaviour, IInternalSettings
{
	[CompilerGenerated]
	private EventHandler eventHandler_0;

	[CompilerGenerated]
	private EventHandler eventHandler_1;

	private AnyChart anyChart_0;

	public abstract Action<IInternalUse, bool> Assign { get; }

	public AnyChart AnyChart_0
	{
		get
		{
			if (anyChart_0 == null)
			{
				anyChart_0 = GetComponent<AnyChart>();
			}
			return anyChart_0;
		}
	}

	public event EventHandler Event_0
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

	public event EventHandler Event_1
	{
		[CompilerGenerated]
		add
		{
			EventHandler eventHandler = eventHandler_1;
			EventHandler eventHandler2;
			do
			{
				eventHandler2 = eventHandler;
				EventHandler value2 = (EventHandler)Delegate.Combine(eventHandler2, value);
				eventHandler = Interlocked.CompareExchange(ref eventHandler_1, value2, eventHandler2);
			}
			while ((object)eventHandler != eventHandler2);
		}
		[CompilerGenerated]
		remove
		{
			EventHandler eventHandler = eventHandler_1;
			EventHandler eventHandler2;
			do
			{
				eventHandler2 = eventHandler;
				EventHandler value2 = (EventHandler)Delegate.Remove(eventHandler2, value);
				eventHandler = Interlocked.CompareExchange(ref eventHandler_1, value2, eventHandler2);
			}
			while ((object)eventHandler != eventHandler2);
		}
	}

	public event EventHandler ChartAndGraph_002EIInternalSettings_002EInternalOnDataUpdate
	{
		add
		{
			Event_0 += value;
		}
		remove
		{
			Event_0 -= value;
		}
	}

	public event EventHandler ChartAndGraph_002EIInternalSettings_002EInternalOnDataChanged
	{
		add
		{
			Event_1 += value;
		}
		remove
		{
			Event_1 -= value;
		}
	}

	public void AddInnerItem(IInternalSettings item)
	{
		item.InternalOnDataChanged += method_1;
		item.InternalOnDataUpdate += method_0;
	}

	public void method_0(object sender, EventArgs e)
	{
		RaiseOnUpdate();
	}

	public void method_1(object sender, EventArgs e)
	{
		RaiseOnChanged();
	}

	public virtual void RaiseOnChanged()
	{
		if (eventHandler_1 != null)
		{
			eventHandler_1(this, EventArgs.Empty);
		}
	}

	public virtual void RaiseOnUpdate()
	{
		if (eventHandler_0 != null)
		{
			eventHandler_0(this, EventArgs.Empty);
		}
	}

	public void method_2(bool clear)
	{
		AnyChart anyChart = AnyChart_0;
		if (anyChart != null)
		{
			Assign(anyChart, clear);
		}
	}

	public virtual void OnEnable()
	{
		method_2(clear: false);
	}

	public virtual void OnDisable()
	{
		method_2(clear: true);
	}

	public virtual void OnDestory()
	{
		method_2(clear: true);
	}

	public virtual void OnValidate()
	{
		AnyChart anyChart = AnyChart_0;
		if (anyChart != null)
		{
			((IInternalUse)anyChart).CallOnValidate();
		}
	}

	public virtual void Start()
	{
		method_2(clear: false);
	}

	public ChartSettingItemBase()
	{
	}
}
