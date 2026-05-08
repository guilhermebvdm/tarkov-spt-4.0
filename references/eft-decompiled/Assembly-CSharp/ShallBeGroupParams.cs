using System;

public class ShallBeGroupParams
{
	public bool Group;

	public int RemainCount;

	[field: NonSerialized]
	public int StartCount { get; set; }

	[field: NonSerialized]
	public bool IsBossSetted { get; set; }

	public ShallBeGroupParams(bool group, bool bossGroup, int groupCount = 4)
	{
		StartCount = (RemainCount = groupCount);
		Group = group;
	}

	public void DescreaseCount()
	{
		RemainCount--;
	}
}
