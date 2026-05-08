using JetBrains.Annotations;
using UnityEngine;

public class GClass612
{
	public string Name;

	public float PriorityScatter1meter = 0.2f;

	public float PriorityScatter10meter = 0.1f;

	public float PriorityScatter100meter = 0.03f;

	public GClass612()
	{
	}

	public GClass612(string Name, float PriorityScatter1meter, float PriorityScatter10meter, float PriorityScatter100meter)
	{
		this.Name = Name;
		this.PriorityScatter1meter = PriorityScatter1meter;
		this.PriorityScatter10meter = PriorityScatter10meter;
		this.PriorityScatter100meter = PriorityScatter100meter;
	}

	public GClass612(GClass612 sc, [CanBeNull] BotPresetClass botPreset)
	{
		float num = 1f;
		Name = sc.Name;
		PriorityScatter1meter = sc.PriorityScatter1meter * num;
		PriorityScatter10meter = sc.PriorityScatter10meter * num;
		PriorityScatter100meter = sc.PriorityScatter100meter * num;
	}

	public void Check(BotGlobalsScatteringSettings settings)
	{
		if (PriorityScatter1meter < settings.MinScatter)
		{
			Debug.LogError("PriorityScatter1meter is bad cause " + PriorityScatter1meter + "  settings.MinScatter:" + settings.MinScatter + "  name:" + Name);
		}
		if (PriorityScatter10meter < settings.MinScatter)
		{
			Debug.LogError("PriorityScatter10meter is bad cause " + PriorityScatter10meter + "  settings.MinScatter:" + settings.MinScatter + "  name:" + Name);
		}
		if (PriorityScatter100meter < settings.MinScatter)
		{
			Debug.LogError("PriorityScatter100meter is bad cause " + PriorityScatter100meter + "  settings.MinScatter:" + settings.MinScatter + "  name:" + Name);
		}
	}
}
