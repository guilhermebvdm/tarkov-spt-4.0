using System.Text;
using Newtonsoft.Json;

public class GClass1443 : GInterface132
{
	[JsonProperty("delay")]
	public float Delay;

	[JsonProperty("duration")]
	public float Duration;

	[JsonProperty("fadeOut")]
	public float FadeOut;

	[JsonProperty("cost")]
	public int Cost;

	[JsonProperty("healthPenaltyMin")]
	public int HealthPenaltyMin;

	[JsonProperty("healthPenaltyMax")]
	public int HealthPenaltyMax;

	public float Value
	{
		get
		{
			return 1f;
		}
		set
		{
		}
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
		if (Cost > 0)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(" / ");
			}
			stringBuilder.Append(Cost + " HP");
		}
		return stringBuilder.ToString();
	}

	public string GetFullStringValue(string displayName)
	{
		if (GClass855.IsZero(Delay) && GClass855.IsZero(Duration) && Cost == 0)
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(GClass2348.Localized(displayName));
		if (Delay > 1f)
		{
			stringBuilder.Append(string.Format("\n{0} {1}{2}", GClass2348.Localized("Delay"), Delay, GClass2348.Localized("sec")));
		}
		if (Duration > 0f)
		{
			stringBuilder.Append(string.Format("\n{0} {1}{2}", GClass2348.Localized("Duration"), Duration, GClass2348.Localized("sec")));
		}
		if (Cost > 0)
		{
			stringBuilder.Append("\n" + Cost + " HP");
		}
		return stringBuilder.ToString();
	}
}
