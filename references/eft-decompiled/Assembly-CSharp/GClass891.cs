using UnityEngine;

public class GClass891
{
	public readonly AudioClip Clip1;

	public float Volume = 1f;

	public float Pitch = 1f;

	public readonly double Start;

	public readonly double End;

	public GClass891()
	{
	}

	public GClass891(AudioClip clip, double start, double end)
	{
		Clip1 = clip;
		Start = start;
		End = end;
	}
}
