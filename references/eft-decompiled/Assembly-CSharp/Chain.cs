using System;

[Serializable]
public class Chain
{
	public enum ESpeaker
	{
		Self,
		Group,
		Other
	}

	public EPhraseTrigger Event = EPhraseTrigger.PhraseNone;

	public int Probability;

	public ESpeaker Who;
}
