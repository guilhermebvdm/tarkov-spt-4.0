using UnityEngine;

[ExecuteInEditMode]
public class CheatShotLogVisualizer : MonoBehaviour
{
	private Vector3 vector3_0;

	private Vector3 vector3_1;

	private Vector3 vector3_2;

	private Vector3 vector3_3;

	private Vector3 vector3_4;

	private string string_0;

	private Vector3[] vector3_5;

	private Vector3[] vector3_6;

	private float float_0;

	[SerializeField]
	private float _radius1 = 0.025f;

	[SerializeField]
	private float _radius2 = 1f / 160f;

	[SerializeField]
	private float _radius3 = 1f / 160f;

	public static CheatShotLogVisualizer Create(string name, Vector3 aggressorPosition, Vector3 shotDirection, Vector3 shotStart, Vector3 shotEnd, Vector3 victimPosition, Vector3 victimPartPosition, string bodyPart, Vector3[] collisions, Vector3[] invalidCollisions)
	{
		GameObject obj = new GameObject("MISS_SHOT_INFO " + name);
		obj.transform.position = aggressorPosition;
		CheatShotLogVisualizer cheatShotLogVisualizer = obj.AddComponent<CheatShotLogVisualizer>();
		cheatShotLogVisualizer.Init(aggressorPosition, shotDirection, shotStart, shotEnd, victimPosition, victimPartPosition, bodyPart, collisions, invalidCollisions);
		return cheatShotLogVisualizer;
	}

	public void Init(Vector3 aggressorPosition, Vector3 shotDirection, Vector3 shotStart, Vector3 shotEnd, Vector3 victimPosition, Vector3 victimPartPosition, string bodyPart, Vector3[] collisions, Vector3[] invalidCollisions)
	{
		vector3_0 = aggressorPosition;
		vector3_1 = shotStart;
		vector3_2 = shotEnd;
		vector3_3 = victimPosition;
		vector3_4 = victimPartPosition;
		string_0 = bodyPart;
		vector3_5 = collisions;
		vector3_6 = invalidCollisions;
		float_0 = Vector3.Distance(vector3_1, vector3_2);
		Debug.Log("CheatShotLogVisualizer created, shot distance: " + float_0, base.gameObject);
	}
}
