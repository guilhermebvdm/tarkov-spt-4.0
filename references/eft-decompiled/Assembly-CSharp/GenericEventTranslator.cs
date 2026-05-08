using System;
using UnityEngine;

public class GenericEventTranslator : MonoBehaviour
{
	public Action<string> OnSoundBankPlay;

	public void PlaySoundBank(string bankType)
	{
		if (string.IsNullOrEmpty(bankType))
		{
			Debug.LogError("Soundbank is null: " + bankType);
		}
		else
		{
			OnSoundBankPlay?.Invoke(bankType);
		}
	}
}
