using System;

[Serializable]
public class RundomRange
{
	public float Min;

	public float Range;

	[NonSerialized]
	public bool Random;

	public float Value
	{
		get
		{
			if (!Random)
			{
				return Min;
			}
			return Min + GClass2608.Float() * Range;
		}
	}

	public void Init()
	{
		Random = Range > float.Epsilon;
	}
}
