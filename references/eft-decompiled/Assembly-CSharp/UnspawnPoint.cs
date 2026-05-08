using UnityEngine;

public class UnspawnPoint : MonoBehaviour, IPositionPoint
{
	public Vector3 Position => base.transform.position;

	public void OnDrawGizmosSelected()
	{
		Gizmos.color = new Color(0.7f, 0.2f, 0.2f, 0.4f);
		GClass850.DrawCube(base.transform.position + Vector3.up * 60f * 0.5f, base.transform.rotation, new Vector3(1f, 60f, 1f));
	}
}
