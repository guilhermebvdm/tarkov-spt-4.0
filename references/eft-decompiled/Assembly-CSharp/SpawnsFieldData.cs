using TMPro;
using UnityEngine;

public class SpawnsFieldData : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI _aliveField;

	[SerializeField]
	private TextMeshProUGUI _groupsField;

	public void UpdateView(GStruct11 spawn)
	{
		string text = ((spawn.SpawnBundlesWait == 0) ? string.Empty : ("(" + spawn.SpawnBundlesWait + ")"));
		_aliveField.text = "Alive:" + spawn.Alive + " Delay:" + spawn.Delayed + " waitBE:" + spawn.ProfileWait + text + " Spawning:" + spawn.SpawnProcess + " Hour:" + spawn.Hour;
		_groupsField.text = ((spawn.DelayDebugModels == null || spawn.DelayDebugModels.Length == 0) ? "No delay" : string.Join("\n", spawn.DelayDebugModels));
	}
}
