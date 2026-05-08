using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class GClass1674
{
	[NonSerialized]
	public const string String_0 = "HOURS";

	[NonSerialized]
	public const string String_1 = "MIN";

	[NonSerialized]
	public const string String_2 = " d. ";

	[NonSerialized]
	public const string String_3 = ":";

	[NonSerialized]
	public static List<string> List_0 = new List<string>(3);

	public static string TraderFormat(this TimeSpan timeLeft)
	{
		string text = ((timeLeft.Days > 0) ? (timeLeft.Days + " d. ") : string.Empty);
		string text2 = timeLeft.Hours.ToString("D2");
		string text3 = timeLeft.Minutes.ToString("D2");
		string text4 = timeLeft.Seconds.ToString("D2");
		return text + text2 + ":" + text3 + ":" + text4;
	}

	public static string EftArenaTransferFormat(this TimeSpan timeLeft)
	{
		string text = timeLeft.Minutes.ToString("D2");
		string text2 = timeLeft.Seconds.ToString("D2");
		return text + ":" + text2;
	}

	public static void Deconstruct(this TimeSpan span, out int hours, out int minutes, out int seconds)
	{
		(hours, minutes, seconds) = Decompose(span);
	}

	public static (int hours, int minutes, int seconds) Decompose(this TimeSpan span)
	{
		int num = (int)span.TotalHours;
		if (num < 0)
		{
			num = 0;
		}
		int num2 = span.Minutes;
		if (num2 < 0)
		{
			num2 = 0;
		}
		int num3 = span.Seconds;
		if (num3 < 0)
		{
			num3 = 0;
		}
		return (hours: num, minutes: num2, seconds: num3);
	}

	public static TimeSpan Clamp(this TimeSpan span, TimeSpan from, TimeSpan to)
	{
		long ticks = from.Ticks;
		long ticks2 = to.Ticks;
		return TimeSpan.FromTicks(smethod_0(span.Ticks, ticks, ticks2));
	}

	public static long smethod_0(long value, long min, long max)
	{
		if (value >= min)
		{
			if (value <= max)
			{
				return value;
			}
			return max;
		}
		return min;
	}

	public static TimeSpan MultiplyByPercent(this TimeSpan multiplicand, int percent)
	{
		return Clamp(TimeSpan.FromTicks(multiplicand.Ticks + multiplicand.Ticks * percent / 100L), TimeSpan.FromDays(0.0), TimeSpan.FromDays(30.0));
	}

	public static TimeSpan Multiply(this TimeSpan multiplicand, float multiplier)
	{
		return Clamp(TimeSpan.FromTicks((long)((float)multiplicand.Ticks * multiplier)), TimeSpan.FromDays(0.0), TimeSpan.FromDays(30.0));
	}

	public static int RoundedSeconds(this TimeSpan timeSpan)
	{
		return (int)Mathf.Round((float)timeSpan.Ticks / 10000000f % 60f);
	}

	public static string TimeLeftShortFormat(this TimeSpan timeSpan, char separator = ' ')
	{
		int num = (int)timeSpan.TotalHours;
		int minutes = timeSpan.Minutes;
		List_0.Clear();
		if (num > 0)
		{
			List_0.Add(num + " " + GClass2348.Localized("HOURS"));
		}
		if (minutes > 0)
		{
			List_0.Add(minutes + " " + GClass2348.Localized("MIN"));
		}
		if (num < 1)
		{
			List_0.Add(timeSpan.Seconds + " " + GClass2348.Localized("SEC"));
		}
		return string.Join(separator.ToString(), List_0);
	}

	public static string RagfairDateFormatLong(this TimeSpan timeSpan)
	{
		if (timeSpan.Days > 0)
		{
			return $"{Mathf.Max(0, timeSpan.Days):D2}d {Mathf.Max(0, timeSpan.Hours):D2}:{Mathf.Max(0, timeSpan.Minutes):D2}:{Mathf.Max(0, timeSpan.Seconds):D2}";
		}
		return $"{Mathf.Max(0, timeSpan.Hours):D2}:{Mathf.Max(0, timeSpan.Minutes):D2}:{Mathf.Max(0, timeSpan.Seconds):D2}";
	}

	public static string TimePassedFormat(this TimeSpan timeSpan)
	{
		string format = GClass2348.Localized("ragfair/Created {0} ago");
		if (timeSpan.TotalSeconds <= 5.0)
		{
			return string.Format(format, "< 5 s");
		}
		if (timeSpan.TotalMinutes <= 1.0)
		{
			return string.Format(format, "< 1 m");
		}
		if (timeSpan.TotalMinutes <= 10.0)
		{
			return string.Format(format, "< 10 m");
		}
		if (timeSpan.TotalMinutes <= 30.0)
		{
			return string.Format(format, "< 30 m");
		}
		if (timeSpan.TotalHours <= 1.0)
		{
			return string.Format(format, "< 1 h");
		}
		if (timeSpan.TotalHours <= 12.0)
		{
			return string.Format(format, "< 12 h");
		}
		if (timeSpan.TotalHours <= 24.0)
		{
			return string.Format(format, "< 1 d");
		}
		return string.Format(format, (int)timeSpan.TotalDays + " d");
	}

	public static string RagfairDateFormatShort(this TimeSpan timeSpan)
	{
		int num = (int)timeSpan.TotalHours;
		if (num >= 24)
		{
			return $"{timeSpan.Days:D2}d {timeSpan.Hours:D2}h";
		}
		if (num >= 5)
		{
			return $"{timeSpan.Hours:D2}h";
		}
		if (num >= 1)
		{
			return $"{timeSpan.Hours:D2}h:{timeSpan.Minutes:D2}m";
		}
		return $"{timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
	}

	public static string DailyQuestFormat(this TimeSpan timeSpan)
	{
		string text = string.Empty;
		if (timeSpan.Days > 0)
		{
			text += string.Format("{0} {1} ", timeSpan.Days, GClass2348.Localized("TimeDaysShort"));
		}
		return text + $"{timeSpan.Hours:00}:{timeSpan.Minutes:00}:{timeSpan.Seconds:00}";
	}

	public static string DailyStatFormat(this TimeSpan timeSpan)
	{
		string text = string.Empty;
		int num = (int)Math.Floor(timeSpan.TotalHours);
		if (num > 0)
		{
			text += string.Format("{0:00} {1} ", num, GClass2348.Localized("HOURS"));
		}
		return text + string.Format("{0} {1}", timeSpan.Minutes, GClass2348.Localized("MIN"));
	}
}
