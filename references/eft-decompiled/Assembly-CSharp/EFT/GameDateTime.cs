using System;
using UnityEngine;

namespace EFT;

public class GameDateTime
{
	public float TimeFactorMod = 1f;

	[NonSerialized]
	public float RealtimeSinceStartup;

	[NonSerialized]
	public const string DateFormat = "d";

	[NonSerialized]
	public const string TimeFormat = "HH:mm:ss";

	[NonSerialized]
	public bool Locked_1;

	[field: NonSerialized]
	public bool Boolean_0 { get; set; }

	[field: NonSerialized]
	public DateTime DateTime_0 { get; set; }

	[field: NonSerialized]
	public DateTime DateTime_1 { get; set; }

	[field: NonSerialized]
	public float TimeFactor { get; set; }

	public bool Locked => Locked_1;

	public static GameDateTime smethod_0(DateTime realDateTime, ref string gameDate, ref string gameTime, ref float timeFactor, bool debug = false)
	{
		Validate(ref gameDate, ref gameTime, ref timeFactor, init: true);
		DateTime gameDateTime = smethod_1(gameDate, gameTime);
		return new GameDateTime(realDateTime, gameDateTime, timeFactor, debug);
	}

	public static DateTime smethod_1(string date, string time)
	{
		long ticks = DateTime.Parse(DateTime.Parse(date).ToString("d") + " " + DateTime.Parse(time).ToString("HH:mm:ss")).Ticks;
		return new DateTime(ticks, DateTimeKind.Utc);
	}

	public GameDateTime(DateTime realDateTime, DateTime gameDateTime, float timeFactor, bool debug = false)
	{
		RealtimeSinceStartup = Time.realtimeSinceStartup;
		DateTime_0 = realDateTime;
		DateTime_1 = gameDateTime;
		TimeFactor = timeFactor;
		Boolean_0 = debug;
		Debug.Log("RealDateTime:" + DateTime_0.ToString() + "  GameDateTime:" + DateTime_1.ToString() + "  factor:" + TimeFactor + " debug:" + debug);
	}

	public DateTime method_0()
	{
		float num = Time.realtimeSinceStartup - RealtimeSinceStartup;
		return DateTime_0 + TimeSpan.FromSeconds(num);
	}

	public static GameDateTime CreateDebug()
	{
		string gameDate = "";
		string gameTime = "";
		float timeFactor = 0f;
		return smethod_0(EFTDateTimeClass.UtcNow, ref gameDate, ref gameTime, ref timeFactor, debug: true);
	}

	public DateTime Calculate()
	{
		TimeSpan timeSpan = method_0() - DateTime_0;
		return DateTime_1 + TimeSpan.FromTicks((long)((float)timeSpan.Ticks * TimeFactor * TimeFactorMod));
	}

	public DateTime CalculateTaxonomyDate(DateTime todSkyDateTime)
	{
		DateTime dateTime = Calculate();
		return new DateTime(todSkyDateTime.Year, todSkyDateTime.Month, todSkyDateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second, DateTimeKind.Utc);
	}

	public void Reset(DateTime gameDateTime)
	{
		Reset(EFTDateTimeClass.UtcNow, gameDateTime, TimeFactor);
	}

	public void Reset(DateTime realDateTime, string gameDate, string gameTime, float timeFactor)
	{
		DateTime gameDateTime = smethod_1(gameDate, gameTime);
		Reset(EFTDateTimeClass.UtcNow, gameDateTime, timeFactor);
	}

	public void ResetForce(DateTime gameDateTime)
	{
		Reset(EFTDateTimeClass.UtcNow, gameDateTime, TimeFactor, force: true);
	}

	public void Reset(DateTime realDateTime, DateTime gameDateTime, float timeFactor)
	{
		Reset(realDateTime, gameDateTime, timeFactor, force: false);
	}

	public void Reset(DateTime realDateTime, DateTime gameDateTime, float timeFactor, bool force)
	{
		if (!Locked || force)
		{
			RealtimeSinceStartup = Time.realtimeSinceStartup;
			DateTime_0 = realDateTime;
			DateTime_1 = gameDateTime;
			TimeFactor = timeFactor;
		}
	}

	public static GameDateTime Deserialize(EFTReaderClass reader)
	{
		DateTime realDateTime = (GClass1285.ReadBool(reader) ? EFTDateTimeClass.UtcNow : DateTime.FromBinary(GClass1285.ReadLong(reader)));
		DateTime gameDateTime = DateTime.FromBinary(GClass1285.ReadLong(reader));
		float timeFactor = GClass1285.ReadFloat(reader);
		return new GameDateTime(realDateTime, gameDateTime, timeFactor);
	}

	public void Serialize(EFTWriterClass writer, bool gameOnly)
	{
		GClass1290.WriteBool(writer, gameOnly);
		if (gameOnly)
		{
			GClass1290.WriteLong(writer, Calculate().ToBinary());
		}
		else
		{
			GClass1290.WriteLong(writer, DateTime_0.ToBinary());
			GClass1290.WriteLong(writer, DateTime_1.ToBinary());
		}
		GClass1290.WriteFloat(writer, TimeFactor);
	}

	public static void Validate(ref string date, ref string time, ref float timeFactor, bool init)
	{
		try
		{
			if (init || !string.IsNullOrEmpty(date))
			{
				date = DateTime.Parse(date).ToString("d");
			}
		}
		catch (Exception)
		{
			date = EFTDateTimeClass.UtcNow.Date.ToString("d");
		}
		try
		{
			if (init || !string.IsNullOrEmpty(time))
			{
				time = DateTime.Parse(time).ToString("HH:mm:ss");
			}
		}
		catch (Exception)
		{
			time = EFTDateTimeClass.UtcNow.ToString("HH:mm:ss");
		}
		if (timeFactor <= 0f || timeFactor > 86400f)
		{
			Debug.LogError("Invalid Time Factor:" + timeFactor);
			timeFactor = 1f;
		}
	}

	public void Lock()
	{
		Locked_1 = true;
	}

	public void Unlock()
	{
		Locked_1 = false;
	}
}
