using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Comfort.Common;
using EFT;
using UnityEngine;

public class TOD_Time : MonoBehaviour
{
	public bool LockCurrentTime;

	public GameDateTime GameDateTime;

	[Tooltip("Length of one day in minutes.")]
	[GAttribute0(1f)]
	public float DayLengthInMinutes = 30f;

	[Tooltip("Set the date to the current device date on start.")]
	public bool UseDeviceDate;

	[Tooltip("Set the time to the current device time on start.")]
	public bool UseDeviceTime;

	[Tooltip("Apply the time curve when progressing time.")]
	public bool UseTimeCurve;

	[Tooltip("Time progression curve.")]
	public AnimationCurve TimeCurve = AnimationCurve.Linear(0f, 0f, 24f, 24f);

	[CompilerGenerated]
	private Action action_0;

	[CompilerGenerated]
	private Action action_1;

	[CompilerGenerated]
	private Action action_2;

	[CompilerGenerated]
	private Action action_3;

	[CompilerGenerated]
	private Action action_4;

	[CompilerGenerated]
	private Action action_5;

	private bool bool_0;

	private AnimationCurve animationCurve_0;

	private AnimationCurve animationCurve_1;

	private DateTime dateTime_0;

	private const string string_0 = "d";

	public event Action OnGameTimeUpdated
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

	public event Action OnMinute
	{
		[CompilerGenerated]
		add
		{
			Action action = action_1;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action action = action_1;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action OnHour
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

	public event Action OnDay
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

	public event Action OnMonth
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

	public event Action OnYear
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

	public void RefreshTimeCurve()
	{
		TimeCurve.preWrapMode = WrapMode.Once;
		TimeCurve.postWrapMode = WrapMode.Once;
		method_1(TimeCurve, out animationCurve_0, out animationCurve_1);
		animationCurve_0.preWrapMode = WrapMode.Loop;
		animationCurve_0.postWrapMode = WrapMode.Loop;
		animationCurve_1.preWrapMode = WrapMode.Loop;
		animationCurve_1.postWrapMode = WrapMode.Loop;
	}

	public float ApplyTimeCurve(float deltaTime)
	{
		float num = animationCurve_1.Evaluate(GClass4.Instance.Cycle.Hour) + deltaTime;
		deltaTime = animationCurve_0.Evaluate(num) - GClass4.Instance.Cycle.Hour;
		if (num >= 24f)
		{
			deltaTime += (float)((int)num / 24 * 24);
		}
		else if (num < 0f)
		{
			deltaTime += (float)(((int)num / 24 - 1) * 24);
		}
		return deltaTime;
	}

	public void AddHours(float hours, bool adjust = true)
	{
		if (GameDateTime == null && UseTimeCurve && adjust)
		{
			hours = ApplyTimeCurve(hours);
		}
		DateTime dateTime = GClass4.Instance.Cycle.DateTime;
		DateTime dateTime2 = ((GameDateTime != null) ? GameDateTime.CalculateTaxonomyDate(dateTime_0) : dateTime.AddHours(hours));
		if (dateTime2.Year > dateTime.Year)
		{
			if (action_5 != null)
			{
				action_5();
			}
			if (action_4 != null)
			{
				action_4();
			}
			if (action_3 != null)
			{
				action_3();
			}
			if (action_2 != null)
			{
				action_2();
			}
			if (action_1 != null)
			{
				action_1();
			}
		}
		else if (dateTime2.Month > dateTime.Month)
		{
			if (action_4 != null)
			{
				action_4();
			}
			if (action_3 != null)
			{
				action_3();
			}
			if (action_2 != null)
			{
				action_2();
			}
			if (action_1 != null)
			{
				action_1();
			}
		}
		else if (dateTime2.Day > dateTime.Day)
		{
			if (action_3 != null)
			{
				action_3();
			}
			if (action_2 != null)
			{
				action_2();
			}
			if (action_1 != null)
			{
				action_1();
			}
		}
		else if (dateTime2.Hour > dateTime.Hour)
		{
			if (action_2 != null)
			{
				action_2();
			}
			if (action_1 != null)
			{
				action_1();
			}
		}
		else if (dateTime2.Minute > dateTime.Minute && action_1 != null)
		{
			action_1();
		}
		if (GameDateTime != null && !bool_0)
		{
			action_0?.Invoke();
			bool_0 = true;
		}
		GClass4.Instance.Cycle.DateTime = dateTime2;
	}

	public void AddSeconds(float seconds)
	{
		AddHours(seconds / 3600f);
	}

	public void method_0(Keyframe[] keys)
	{
		for (int i = 0; i < keys.Length; i++)
		{
			Keyframe keyframe = keys[i];
			if (i > 0)
			{
				Keyframe keyframe2 = keys[i - 1];
				keyframe.inTangent = (keyframe.value - keyframe2.value) / (keyframe.time - keyframe2.time);
			}
			if (i < keys.Length - 1)
			{
				Keyframe keyframe3 = keys[i + 1];
				keyframe.outTangent = (keyframe3.value - keyframe.value) / (keyframe3.time - keyframe.time);
			}
			keys[i] = keyframe;
		}
	}

	public void method_1(AnimationCurve source, out AnimationCurve approxCurve, out AnimationCurve approxInverse)
	{
		Keyframe[] array = new Keyframe[25];
		Keyframe[] array2 = new Keyframe[25];
		float num = -0.01f;
		for (int i = 0; i < 25; i++)
		{
			num = Mathf.Max(num + 0.01f, source.Evaluate(i));
			array[i] = new Keyframe(i, num);
			array2[i] = new Keyframe(num, i);
		}
		method_0(array);
		method_0(array2);
		approxCurve = new AnimationCurve(array);
		approxInverse = new AnimationCurve(array2);
	}

	public DateTime method_2()
	{
		try
		{
			return DateTime.Parse(DateTime.Parse(Singleton<BackendConfigSettingsClass>.Instance.TODSkyDate).ToString("d"));
		}
		catch (Exception)
		{
			return EFTDateTimeClass.Now;
		}
	}

	public void Start()
	{
		dateTime_0 = method_2();
		DateTime now = EFTDateTimeClass.Now;
		if (UseDeviceDate)
		{
			GClass4.Instance.Cycle.Year = now.Year;
			GClass4.Instance.Cycle.Month = now.Month;
			GClass4.Instance.Cycle.Day = now.Day;
		}
		if (UseDeviceTime)
		{
			GClass4.Instance.Cycle.Hour = (float)now.TimeOfDay.TotalHours;
		}
		RefreshTimeCurve();
	}

	public void FixedUpdate()
	{
		float num = 1440f / DayLengthInMinutes;
		if (!LockCurrentTime)
		{
			AddSeconds(Time.deltaTime * num);
		}
	}
}
