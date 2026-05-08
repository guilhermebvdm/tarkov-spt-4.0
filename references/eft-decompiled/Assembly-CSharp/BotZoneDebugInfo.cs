using System.Text;
using EFT;
using TMPro;
using UnityEngine;

public class BotZoneDebugInfo : MonoBehaviour
{
	public TextMeshProUGUI field;

	public void UpdateData(string nameZone, int[] blocks)
	{
		if (blocks == null)
		{
			return;
		}
		base.gameObject.SetActive(value: true);
		StringBuilder stringBuilder = new StringBuilder(nameZone);
		stringBuilder.Append(":  ");
		foreach (int num in blocks)
		{
			if (num > 0 && num < 40)
			{
				stringBuilder.Append(((WildSpawnType)(num - 1)/*cast due to constrained. prefix*/).ToString());
				stringBuilder.Append(',');
			}
		}
		string text = stringBuilder.ToString();
		if (field != null)
		{
			field.text = text;
		}
	}
}
