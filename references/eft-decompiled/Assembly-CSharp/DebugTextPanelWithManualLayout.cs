using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugTextPanelWithManualLayout : MonoBehaviour
{
	private static readonly char[] char_0 = new char[1] { '\n' };

	[SerializeField]
	private Text _dataLineTemplate;

	private readonly List<Text> list_0 = new List<Text>();

	private int int_0;

	private RectTransform rectTransform_0;

	public RectTransform RectTransform => rectTransform_0 ?? (rectTransform_0 = GClass949.RectTransform(base.transform));

	public float Single_0 => (float)int_0 * _dataLineTemplate.rectTransform.sizeDelta.y * _dataLineTemplate.rectTransform.localScale.y;

	public void Start()
	{
		_dataLineTemplate.gameObject.SetActive(value: false);
	}

	public void OnDestroy()
	{
		list_0.Clear();
	}

	public void UpdateText(string text)
	{
		object obj;
		if (text == null)
		{
			obj = null;
		}
		else
		{
			obj = text.Split(char_0, StringSplitOptions.RemoveEmptyEntries);
			if (obj != null)
			{
				goto IL_001b;
			}
		}
		obj = Array.Empty<string>();
		goto IL_001b;
		IL_001b:
		string[] array = (string[])obj;
		if (array != null)
		{
			int_0 = array.Length;
			for (int i = 0; i < array.Length; i++)
			{
				Text text2 = ((list_0.Count <= i) ? method_0() : list_0[i]);
				if (!text2.gameObject.activeSelf)
				{
					text2.gameObject.SetActive(value: true);
				}
				text2.text = array[i];
			}
		}
		else
		{
			int_0 = 0;
		}
		for (int j = int_0; j < list_0.Count; j++)
		{
			if (list_0[j].gameObject.activeSelf)
			{
				list_0[j].gameObject.SetActive(value: false);
			}
		}
		RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Single_0);
	}

	public Text method_0()
	{
		Text text = UnityEngine.Object.Instantiate(_dataLineTemplate, base.transform);
		text.rectTransform.anchoredPosition = GClass1675.WithY(text.rectTransform.anchoredPosition, (float)(-1 * list_0.Count) * text.rectTransform.sizeDelta.y * text.rectTransform.localScale.y);
		list_0.Add(text);
		return text;
	}
}
