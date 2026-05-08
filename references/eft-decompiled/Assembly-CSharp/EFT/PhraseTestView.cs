using System.Runtime.CompilerServices;
using Comfort.Common;
using UnityEngine;
using UnityEngine.UI;

namespace EFT;

public class PhraseTestView : MonoBehaviour
{
	[CompilerGenerated]
	public class Class1813
	{
		public PhraseSpeakerClass tmpSpeaker;

		public PhraseTestView phraseTestView_0;

		public void method_0()
		{
			phraseTestView_0.UpdateSoundList(tmpSpeaker);
		}
	}

	[SerializeField]
	private GameObject _speakersPanel;

	[SerializeField]
	private GameObject _mainPanel;

	[SerializeField]
	private Button _showUi;

	[SerializeField]
	private Button _speakerLayoutButton;

	private int int_0;

	public void Awake()
	{
		if (_showUi != null)
		{
			_showUi.onClick.AddListener(delegate
			{
				_mainPanel.SetActive(!_mainPanel.activeSelf);
			});
		}
	}

	public void method_0()
	{
		method_1(_speakersPanel);
		foreach (PhraseSpeakerClass speaker in Singleton<GameWorld>.Instance.SpeakerManager.Speakers)
		{
			Button button = Object.Instantiate(_speakerLayoutButton);
			button.transform.SetParent(_speakersPanel.transform);
			button.GetComponent<RectTransform>().localScale = Vector3.one;
			button.gameObject.SetActive(value: true);
			button.GetComponent<Image>().color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
			PhraseSpeakerClass tmpSpeaker = speaker;
			button.onClick.AddListener(delegate
			{
				UpdateSoundList(tmpSpeaker);
			});
		}
	}

	public void UpdateSoundList(PhraseSpeakerClass speaker)
	{
	}

	public void method_1(GameObject panel)
	{
		Button[] componentsInChildren = panel.GetComponentsInChildren<Button>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Object.DestroyImmediate(componentsInChildren[i].gameObject);
		}
	}

	[CompilerGenerated]
	public void method_2()
	{
		_mainPanel.SetActive(!_mainPanel.activeSelf);
	}
}
