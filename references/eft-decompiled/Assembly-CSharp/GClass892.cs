using UnityEngine;

public class GClass892 : GClass891
{
	public GClass892(AudioClip clip, double start, double end)
		: base(clip, start, end)
	{
	}

	public void PlayOn(SimpleSource source)
	{
		source.source1.clip = Clip1;
		source.source1.volume = Volume;
		source.PlayScheduled(Start);
	}
}
