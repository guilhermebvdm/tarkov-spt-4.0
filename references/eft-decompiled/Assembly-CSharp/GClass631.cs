using System.Collections.Generic;

public class GClass631
{
	public Dictionary<int, GClass630> stat = new Dictionary<int, GClass630>();

	public GClass631()
	{
		int num = 0;
		float[] dISTANCES = BotPersonalStats.DISTANCES;
		foreach (float dist in dISTANCES)
		{
			GClass630 gClass = new GClass630();
			stat.Add(num, gClass);
			gClass.Dist = dist;
			num++;
		}
	}

	public void AddGainSIght(float d, float seenCoef)
	{
		int index = BotPersonalStats.GetIndex(d);
		if (stat.TryGetValue(index, out var value))
		{
			value.Times++;
			float midCoef = (value.MidCoef * (float)(value.Times - 1) + seenCoef) / (float)value.Times;
			value.MidCoef = midCoef;
		}
	}
}
