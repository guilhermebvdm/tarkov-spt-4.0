using System;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

[UsedImplicitly]
public class TimerText : MonoBehaviour
{
	[SerializeField]
	private TMP_Text _text;

	private DateTime dateTime_0;

	public void Show(DateTime startTime)
	{
		base.gameObject.SetActive(value: true);
		dateTime_0 = startTime;
	}

	[UsedImplicitly]
	public void Update()
	{
		TimeSpan timeSpan = EFTDateTimeClass.UtcNow - dateTime_0;
		_text.text = $"{(int)timeSpan.TotalMinutes:0}:{timeSpan.Seconds:00}";
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}
}
