using TMPro;
using UnityEngine;

public class BotGroupInfo : MonoBehaviour
{
	public TextMeshProUGUI field;

	private GStruct21 gstruct21_0;

	public void UpdateData(GStruct21 group)
	{
		gstruct21_0 = group;
		field.text = gstruct21_0.MessageInfo();
		base.gameObject.SetActive(value: true);
	}
}
