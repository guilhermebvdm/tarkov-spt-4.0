using System;

[Serializable]
public class IntFloat
{
	public int Index;

	public float Value;

	public IntFloat(int index, float val)
	{
		Index = index;
		Value = val;
	}
}
