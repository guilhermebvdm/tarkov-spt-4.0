using System;
using UnityEngine;

namespace Audio.Data;

[Serializable]
[CreateAssetMenu(menuName = "Scriptable objects/Audio/BtrDriverSoundBankContainer", fileName = "BtrDriverSoundBankContainer")]
public class BtrDriverSoundBankContainer : SoundBankContainerBase<EBtrDriverPhraseTrigger>
{
	[Serializable]
	public class BtrDriverPhrasesByTrigger : SerializableEnumDictionary<EBtrDriverPhraseTrigger, SoundBankWithSettings>
	{
	}

	[SerializeField]
	private BtrDriverPhrasesByTrigger _phraseData;

	public override bool TryGetBank(EBtrDriverPhraseTrigger soundType, out SoundBankWithSettings bank)
	{
		return _phraseData.TryGetValue(soundType, out bank);
	}
}
