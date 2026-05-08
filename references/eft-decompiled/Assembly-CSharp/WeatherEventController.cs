using System;
using System.Runtime.CompilerServices;
using EFT;
using EFT.Weather;
using UnityEngine;

public class WeatherEventController : MonoBehaviour
{
	public static WeatherEventController Instance;

	public const string WEATHER_EVENT = "weatherEvent";

	public const int PRAY_ANIMATION_LAYER = 17;

	[CompilerGenerated]
	private bool bool_0;

	[CompilerGenerated]
	private static bool bool_1;

	private float float_0;

	[CompilerGenerated]
	private int int_0 = 21;

	[CompilerGenerated]
	private int int_1 = 40;

	public float SecToChange = 45f;

	private DateTime dateTime_0;

	[CompilerGenerated]
	private readonly WeatherClass weatherClass = new WeatherClass();

	public float currentPercantage;

	private long long_0;

	private float float_1;

	public bool EventActive
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

	public static bool ChangeTimeInProgress
	{
		[CompilerGenerated]
		get
		{
			return bool_1;
		}
		[CompilerGenerated]
		set
		{
			bool_1 = value;
		}
	}

	public int DesiredHour
	{
		[CompilerGenerated]
		get
		{
			return int_0;
		}
		[CompilerGenerated]
		set
		{
			int_0 = value;
		}
	}

	public int DesiredMinute
	{
		[CompilerGenerated]
		get
		{
			return int_1;
		}
		[CompilerGenerated]
		set
		{
			int_1 = value;
		}
	}

	public WeatherClass DestWeatherNode
	{
		[CompilerGenerated]
		get
		{
			return weatherClass;
		}
	}

	public GameDateTime GameDateTime => GClass4.Instance.CurrentTime.GameDateTime;

	public void Awake()
	{
		Instance = this;
	}

	public void OnDestroy()
	{
		Instance = null;
	}

	public void Update()
	{
		if (EventActive || ChangeTimeInProgress)
		{
			DateTime gameDateTime = GameDateTime.Calculate();
			currentPercantage = Mathf.Clamp01((float)(gameDateTime.Ticks - long_0) / (float)(dateTime_0.Ticks - long_0));
			if (currentPercantage >= 1f && ChangeTimeInProgress)
			{
				ChangeTimeInProgress = false;
				GameDateTime.ResetForce(gameDateTime);
				GameDateTime.TimeFactorMod = 1f;
				GClass4.Instance.UpdateInterval = float_1;
				GClass4.Instance.CurrentTime.LockCurrentTime = true;
			}
		}
	}

	public void SetWeather(GClass1908 weatherSettings)
	{
		SetWeather(weatherSettings.Hour, weatherSettings.Minute, weatherSettings.Cloudness, weatherSettings.WindDirection, weatherSettings.Wind, weatherSettings.Rain, weatherSettings.RainRandomness, weatherSettings.ScaterringFogDensity, weatherSettings.TopWindDirection);
	}

	public void SetWeather(int hours, int minutes, float cloudness, int windDirection, float wind, float rain, float rainRandomness, float scaterringFogDensity, Vector2 topWindDirection)
	{
		DesiredHour = hours;
		DesiredMinute = minutes;
		DestWeatherNode.Cloudness = cloudness;
		DestWeatherNode.WindDirection = windDirection;
		DestWeatherNode.Wind = wind;
		DestWeatherNode.Rain = rain;
		DestWeatherNode.RainRandomness = rainRandomness;
		DestWeatherNode.Temperature = 20f;
		DestWeatherNode.AtmospherePressure = 760f;
		DestWeatherNode.ScaterringFogDensity = scaterringFogDensity;
		DestWeatherNode.TopWindDirection = topWindDirection;
	}

	public void Activate(bool val, int fromPercent = 0, bool force = false)
	{
		EventActive = val;
		if (EventActive)
		{
			float num = (float)fromPercent / 100f;
			float_0 = GameDateTime.TimeFactor;
			DateTime dateTime = GameDateTime.Calculate();
			dateTime_0 = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, DesiredHour, DesiredMinute, dateTime.Second, DateTimeKind.Utc);
			if (dateTime_0 < dateTime)
			{
				dateTime_0 = dateTime_0.AddDays(1.0);
			}
			int num2 = Mathf.CeilToInt(SecToChange * (1f - num));
			TimeSpan timeSpan = dateTime_0.Subtract(dateTime);
			TimeSpan timeSpan2 = EFTDateTimeClass.Now.AddSeconds(num2).Subtract(DateTime.Now);
			if (num2 > 0 && !force)
			{
				float timeFactorMod = (float)timeSpan.Ticks / (float)timeSpan2.Ticks / float_0;
				GameDateTime.TimeFactorMod = timeFactorMod;
			}
			long value = (long)((float)timeSpan.Ticks * num);
			DestWeatherNode.Time = EFTDateTimeClass.MoscowNow.AddSeconds(Mathf.Max(num2, 1)).Ticks;
			WeatherController.Instance.SetWeatherForce(DestWeatherNode);
			float_1 = GClass4.Instance.UpdateInterval;
			GClass4.Instance.UpdateInterval = 0f;
			GameDateTime.Lock();
			if (force)
			{
				GClass4.Instance.CurrentTime.LockCurrentTime = false;
				GameDateTime.ResetForce(dateTime_0);
				GClass4.Instance.CurrentTime.AddSeconds(0f);
				GameDateTime.TimeFactorMod = 1f;
				GClass4.Instance.UpdateInterval = float_1;
				GClass4.Instance.CurrentTime.LockCurrentTime = true;
				ChangeTimeInProgress = false;
			}
			else
			{
				GameDateTime.ResetForce(dateTime.AddTicks(value));
				ChangeTimeInProgress = true;
			}
			long_0 = dateTime.Ticks;
		}
		else
		{
			float num3 = (float)fromPercent / 100f;
			int a = Mathf.CeilToInt(SecToChange * (1f - num3));
			DestWeatherNode.Time = EFTDateTimeClass.MoscowNow.AddSeconds(Mathf.Max(a, 1)).Ticks;
		}
	}

	public void method_0()
	{
		SetWeather(new GClass1908());
		Activate(val: true);
	}
}
