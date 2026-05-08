using EFT.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LabelContentRow : UIElement
{
	[SerializeField]
	private Image _icon;

	[SerializeField]
	private TMP_Text _titleText;

	[SerializeField]
	private TMP_Text _mainText;

	public void Show(string mainText, string titleText = null, Sprite sprite = null)
	{
		if (_titleText != null)
		{
			_titleText.gameObject.SetActive(value: false);
		}
		if (_icon != null)
		{
			_icon.gameObject.SetActive(value: false);
		}
		_mainText.text = mainText;
		_mainText.gameObject.SetActive(value: true);
		if (!GClass856.IsNullOrEmpty(titleText))
		{
			if (_titleText != null)
			{
				_titleText.text = titleText;
				_titleText.gameObject.SetActive(value: true);
			}
			else
			{
				Debug.LogError("LabelContentRow title text gameobject is null!");
			}
		}
		if (sprite != null)
		{
			if (_titleText != null)
			{
				_icon.sprite = sprite;
				_icon.gameObject.SetActive(value: true);
			}
			else
			{
				Debug.LogError("LabelContentRow icon gameobject is null!");
			}
		}
		ShowGameObject();
	}
}
