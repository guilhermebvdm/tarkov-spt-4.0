using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BotInfoIcon : MonoBehaviour
{
	[SerializeField]
	private Image _icon;

	[SerializeField]
	private Color _trueColor;

	[SerializeField]
	private Color _falseColor;

	[SerializeField]
	private List<Color> _additionalColors = new List<Color>();

	private bool bool_0;

	private int int_0;

	public void SetBool(bool value)
	{
		bool_0 = value;
		_icon.color = (value ? _trueColor : _falseColor);
	}

	public void SetInt(int value)
	{
		int_0 = value;
		_icon.color = _additionalColors[value % _additionalColors.Count];
	}

	public void SetColor(Color color)
	{
		_icon.color = color;
	}
}
