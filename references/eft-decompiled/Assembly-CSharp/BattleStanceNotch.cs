using UnityEngine;
using UnityEngine.UI;

public class BattleStanceNotch : MonoBehaviour
{
	[SerializeField]
	private Color _availableColor;

	[SerializeField]
	private Color _unavailableColor;

	[SerializeField]
	private Image _notchImage;

	public void Init(RectTransform rect)
	{
		base.transform.SetParent(rect);
		GClass949.ResetTransform(base.transform);
		Show(available: true);
	}

	public void Show(bool available)
	{
		base.gameObject.SetActive(value: true);
		_notchImage.color = (available ? _availableColor : _unavailableColor);
	}
}
