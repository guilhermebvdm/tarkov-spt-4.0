using System;

public class Class1072
{
	[NonSerialized]
	public static DateTime DateTime_0 = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

	public static double DateToValue(DateTime dateTime)
	{
		return (dateTime - DateTime_0).TotalSeconds;
	}

	public static string DateToTimeString(DateTime dateTime)
	{
		return dateTime.ToString("t");
	}

	public static string DateToDateString(DateTime dateTime)
	{
		return dateTime.ToString("d");
	}

	public static string DateToDateTimeString(DateTime dateTime)
	{
		return string.Format("{0}{1}{2}", dateTime.ToString("d"), Environment.NewLine, dateTime.ToString("t"));
	}

	public static DateTime ValueToDate(double value)
	{
		DateTime dateTime_ = DateTime_0;
		return dateTime_.AddSeconds(value);
	}
}
