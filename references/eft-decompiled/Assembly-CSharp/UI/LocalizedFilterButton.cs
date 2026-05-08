using EFT.UI;
using UnityEngine;
using UnityEngine.UI;

namespace UI;

public class LocalizedFilterButton : UIElement
{
	[SerializeField]
	private Button _button;

	[SerializeField]
	private string _localizationKey;

	public string LocalizationKey => _localizationKey;

	public Button.ButtonClickedEvent OnClick => _button.onClick;
}
