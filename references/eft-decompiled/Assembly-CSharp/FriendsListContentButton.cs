using UnityEngine;
using UnityEngine.UI;

public class FriendsListContentButton : Button
{
	[SerializeField]
	private GameObject _connectedGameObject;

	[SerializeField]
	private CustomTextMeshProUGUI _buttonLabel;

	public void UpdateStatus(bool status)
	{
		_connectedGameObject.SetActive(status);
		_buttonLabel.text = (status ? "-" : "+");
	}
}
