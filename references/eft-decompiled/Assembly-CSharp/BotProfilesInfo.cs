using TMPro;
using UnityEngine;

public class BotProfilesInfo : MonoBehaviour
{
	public TextMeshProUGUI field;

	private GClass406 gclass406_0;

	public void UpdateData(GClass406 group)
	{
		gclass406_0 = group;
		field.text = gclass406_0.Struct.MessageInfo();
		base.gameObject.SetActive(value: true);
	}
}
