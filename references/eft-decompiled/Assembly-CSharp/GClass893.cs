using UnityEngine;

public class GClass893 : GClass891
{
	public readonly AudioClip Clip2;

	public readonly float Balance;

	public GClass893(AudioClip clip1, AudioClip clip2, float balance, double start, double end)
		: base(clip1, start, end)
	{
		Clip2 = clip2;
		Balance = balance;
	}

	public void PlayOn(SuperSource source)
	{
		source.source1.clip = Clip1;
		source.source1.volume = Balance * Volume;
		source.source2.enabled = true;
		source.source2.clip = Clip2;
		source.source2.volume = (1f - Balance) * Volume;
		source.SetPitch(Pitch);
		source.PlayScheduled(Start);
	}
}
