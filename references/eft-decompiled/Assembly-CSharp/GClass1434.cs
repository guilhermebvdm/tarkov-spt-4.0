using System;

public class GClass1434
{
	public double date;

	public int placed;

	public int sold;

	public int expired;

	public DateTime DateTime => EFTDateTimeClass.LocalDateTimeFromUnixTime(date);
}
