using System;
using System.Diagnostics;

public abstract class EFTDateTimeClass
{
	public const int SECONDS_IN_MINUTE = 60;

	public const int SECONDS_IN_HOUR = 3600;

	public const int SECONDS_IN_DAY = 86400;

	public static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

	[NonSerialized]
	public static DateTime DateTime_0 = DateTime.Now;

	[NonSerialized]
	public static DateTime DateTime_1 = DateTime.UtcNow;

	[NonSerialized]
	public static Stopwatch Stopwatch_0 = Stopwatch.StartNew();

	public static DateTime UtcNow => DateTime_1.AddMilliseconds(Stopwatch_0.ElapsedMilliseconds);

	public static DateTime Now => DateTime_0.AddMilliseconds(Stopwatch_0.ElapsedMilliseconds);

	public static double UtcNowUnix => ToUnixTime(UtcNow);

	public static int UtcNowUnixInt => (int)ToUnixTime(UtcNow);

	public static double NowUnix => ToUnixTime(Now);

	public static DateTime MoscowNow => UtcNow.AddHours(3.0);

	public static DateTime LocalDateTimeFromUnixTime(double milliseconds)
	{
		return UniversalDateTimeFromUnixTime(milliseconds).ToLocalTime();
	}

	public static DateTime UniversalDateTimeFromUnixTime(double seconds)
	{
		DateTime unixEpoch = UnixEpoch;
		return unixEpoch.AddMilliseconds(seconds * 1000.0);
	}

	public static double ToUnixTime(this DateTime dateTime)
	{
		return (dateTime - UnixEpoch).TotalSeconds;
	}

	public static void SetServerTime(double milliseconds)
	{
		Stopwatch_0 = Stopwatch.StartNew();
		DateTime_1 = UniversalDateTimeFromUnixTime(milliseconds);
		DateTime_0 = DateTime_1.ToLocalTime();
	}

	public static DateTime FromHour(int hour)
	{
		DateTime now = DateTime.Now;
		return new DateTime(now.Year, now.Month, now.Day, hour, 0, 0, DateTimeKind.Utc);
	}

	public static DateTime StartOfDay()
	{
		DateTime now = DateTime.Now;
		return new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, 0);
	}
}
