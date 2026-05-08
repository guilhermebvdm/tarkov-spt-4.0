using System;
using System.Runtime.CompilerServices;
using System.Text;
using Newtonsoft.Json;

public class GClass1444 : GInterface132
{
	[JsonProperty("delay")]
	public float Delay;

	[JsonProperty("duration")]
	public float Duration;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_0;

	[JsonProperty("value")]
	public float Value
	{
		[CompilerGenerated]
		get
		{
			return Float_0;
		}
		[CompilerGenerated]
		set
		{
			Float_0 = value;
		}
	}

	public GClass1444 CutPiece(float piece)
	{
		GClass1444 obj = (GClass1444)MemberwiseClone();
		obj.Value *= piece;
		obj.Duration *= piece;
		return obj;
	}

	public string GetStringValue(string postfix = "")
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (Delay > 1f)
		{
			stringBuilder.Append(string.Format("{0} {1}{2}", GClass2348.Localized("Del."), Delay, GClass2348.Localized("sec")));
		}
		if (Duration > 0f)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(" / ");
			}
			stringBuilder.Append(string.Format("{0} {1}{2}", GClass2348.Localized("Dur."), Duration, GClass2348.Localized("sec")));
		}
		if (!GClass855.IsZero(Value))
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(" / ");
			}
			stringBuilder.Append(Math.Abs(Value) + postfix);
		}
		return stringBuilder.ToString();
	}

	public string GetFullStringValue(string displayName)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(GClass2348.Localized(displayName));
		if (!GClass855.IsZero(Value))
		{
			stringBuilder.AppendFormat("\n{0} {1}", (Value > 0f) ? GClass2348.Localized("increase") : GClass2348.Localized("decrease"), Math.Abs(Value));
		}
		if (Delay > 1f)
		{
			stringBuilder.Append(string.Format("\n{0} {1}{2}", GClass2348.Localized("Delay"), Delay, GClass2348.Localized("sec")));
		}
		if (Duration > 0f)
		{
			stringBuilder.Append(string.Format("\n{0} {1}{2}", GClass2348.Localized("Duration"), Duration, GClass2348.Localized("sec")));
		}
		return stringBuilder.ToString();
	}
}
