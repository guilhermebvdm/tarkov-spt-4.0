using System.Collections.Generic;

public class GClass625
{
	public Dictionary<int, float> timeAim = new Dictionary<int, float>();

	public Dictionary<int, int> times = new Dictionary<int, int>();

	public GClass625()
	{
		int num = 0;
		float[] dISTANCES = BotPersonalStats.DISTANCES;
		for (int i = 0; i < dISTANCES.Length; i++)
		{
			timeAim.Add(num, 0f);
			times.Add(num, 0);
			num++;
		}
	}

	public void AddAim(float dist, float time)
	{
		int index = BotPersonalStats.GetIndex(dist);
		times[index]++;
		float num = timeAim[index];
		timeAim[index] = (num * (float)(times[index] - 1) + time) / (float)times[index];
	}
}
