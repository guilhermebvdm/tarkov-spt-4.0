using System;
using Comfort.Common;
using EFT;
using UnityEngine;

public class Watch : MonoBehaviour
{
	[SerializeField]
	private Transform _hourHand;

	[SerializeField]
	private Transform _minuteHand;

	[SerializeField]
	private Transform _secondHand;

	[SerializeField]
	private AnimationCurve _curve;

	private float float_0 = 1f;

	private DateTime dateTime_0;

	private TimeSpan timeSpan_0;

	public DateTime DateTime_0
	{
		get
		{
			TimeSpan timeSpan = TimeSpan.FromTicks((long)((float)(EFTDateTimeClass.Now - dateTime_0).Ticks * float_0));
			return dateTime_0 + timeSpan + timeSpan_0;
		}
	}

	public void Init(TimeSpan timeDifference)
	{
		timeSpan_0 = timeDifference;
		if (Singleton<GameWorld>.Instantiated && Singleton<GameWorld>.Instance.GameDateTime != null)
		{
			GameDateTime gameDateTime = Singleton<GameWorld>.Instance.GameDateTime;
			float_0 = gameDateTime.TimeFactor;
			dateTime_0 = EFTDateTimeClass.Now;
			timeSpan_0 = gameDateTime.Calculate().TimeOfDay - dateTime_0.TimeOfDay;
		}
	}

	public void Update()
	{
		TimeSpan timeOfDay = DateTime_0.TimeOfDay;
		double num = timeOfDay.TotalSeconds % 60.0;
		float time = (float)(num - (double)(int)num);
		_hourHand.localRotation = Quaternion.Euler(0f, 0f, (float)timeOfDay.TotalHours * 30f);
		_minuteHand.localRotation = Quaternion.Euler(0f, 0f, (float)timeOfDay.TotalMinutes % 60f * 6f);
		_secondHand.localRotation = Quaternion.Euler(0f, 0f, ((float)(int)num + _curve.Evaluate(time)) * 6f);
	}
}
