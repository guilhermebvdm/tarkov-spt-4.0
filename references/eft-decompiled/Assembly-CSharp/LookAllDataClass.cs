using System.Collections.Generic;

public class LookAllDataClass
{
	public bool ShallRecalcGoal;

	public float MinDistance = float.MaxValue;

	public readonly List<BotReportsDataClass> ReportsData = new List<BotReportsDataClass>();

	public void Reset()
	{
		ShallRecalcGoal = false;
		MinDistance = float.MaxValue;
		ReportsData.Clear();
	}
}
