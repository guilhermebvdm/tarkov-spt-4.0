using UnityEngine;

public class GClass552
{
	public Vector3 BotPosAtStart;

	public BotCurrentPathAbstractClass Path;

	public float CreateTime;

	public string CallstackPathComeFrom;

	public void DebugDraw()
	{
		Debug.DrawRay(BotPosAtStart, Vector3.up * 10f, Color.red, 5f);
		Vector3 point = Path.GetPoint(0);
		Vector3 vector = Path.LastCorner();
		Debug.DrawRay(Path.GetPoint(0), Vector3.up * 15f, Color.yellow, 5f);
		Path.DrawDebugWay(0.7f);
		float num = Time.time - CreateTime;
		Debug.LogError($"Last period:{num}");
		Debug.LogError("Info:" + Path.DebugInfo);
		Debug.LogError("CallstackPathComeFrom:" + CallstackPathComeFrom);
		Debug.LogError($"From:{point} last:{vector}");
	}
}
