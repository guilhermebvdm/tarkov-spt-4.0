using System;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using UnityEngine;

namespace ChartAndGraph;

[Serializable]
public class TextFormatting : IInternalSettings
{
	[SerializeField]
	public string prefix = "";

	[SerializeField]
	public string suffix = "";

	[CompilerGenerated]
	private EventHandler OnDataUpdate;

	[CompilerGenerated]
	private EventHandler OnDataChanged;

	public string Suffix
	{
		get
		{
			return suffix;
		}
		set
		{
			suffix = value;
			RaiseOnUpdate();
		}
	}

	public string Prefix
	{
		get
		{
			return prefix;
		}
		set
		{
			prefix = value;
			RaiseOnUpdate();
		}
	}

	public event EventHandler Event_0
	{
		[CompilerGenerated]
		add
		{
			EventHandler eventHandler = OnDataUpdate;
			EventHandler eventHandler2;
			do
			{
				eventHandler2 = eventHandler;
				EventHandler value2 = (EventHandler)Delegate.Combine(eventHandler2, value);
				eventHandler = Interlocked.CompareExchange(ref OnDataUpdate, value2, eventHandler2);
			}
			while ((object)eventHandler != eventHandler2);
		}
		[CompilerGenerated]
		remove
		{
			EventHandler eventHandler = OnDataUpdate;
			EventHandler eventHandler2;
			do
			{
				eventHandler2 = eventHandler;
				EventHandler value2 = (EventHandler)Delegate.Remove(eventHandler2, value);
				eventHandler = Interlocked.CompareExchange(ref OnDataUpdate, value2, eventHandler2);
			}
			while ((object)eventHandler != eventHandler2);
		}
	}

	public event EventHandler Event_1
	{
		[CompilerGenerated]
		add
		{
			EventHandler eventHandler = OnDataChanged;
			EventHandler eventHandler2;
			do
			{
				eventHandler2 = eventHandler;
				EventHandler value2 = (EventHandler)Delegate.Combine(eventHandler2, value);
				eventHandler = Interlocked.CompareExchange(ref OnDataChanged, value2, eventHandler2);
			}
			while ((object)eventHandler != eventHandler2);
		}
		[CompilerGenerated]
		remove
		{
			EventHandler eventHandler = OnDataChanged;
			EventHandler eventHandler2;
			do
			{
				eventHandler2 = eventHandler;
				EventHandler value2 = (EventHandler)Delegate.Remove(eventHandler2, value);
				eventHandler = Interlocked.CompareExchange(ref OnDataChanged, value2, eventHandler2);
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

	public virtual void RaiseOnChanged()
	{
		if (OnDataChanged != null)
		{
			OnDataChanged(this, EventArgs.Empty);
		}
	}

	public virtual void RaiseOnUpdate()
	{
		if (OnDataUpdate != null)
		{
			OnDataUpdate(this, EventArgs.Empty);
		}
	}

	public static string smethod_0(string str, string category, string group)
	{
		if (!str.Contains("<?") && !str.Contains("\\n"))
		{
			return str;
		}
		return str.Replace("<?category>", category).Replace("<?group>", group).Replace("\\n", Environment.NewLine);
	}

	public static void smethod_1(StringBuilder builder, string category, string group)
	{
		builder.Replace("<?category>", category).Replace("<?group>", group).Replace("\\n", Environment.NewLine);
	}

	public static string smethod_2(string str)
	{
		if (!string.IsNullOrEmpty(str))
		{
			return str;
		}
		return "";
	}

	public void Format(StringBuilder builder, string data, string category, string group)
	{
		builder.Length = 0;
		builder.Append(smethod_2(Prefix));
		builder.Append(data);
		builder.Append(smethod_2(Suffix));
		smethod_1(builder, category, group);
	}

	public string Format(string data, string category, string group)
	{
		return smethod_0(smethod_2(Prefix) + data + smethod_2(Suffix), category, group);
	}
}
