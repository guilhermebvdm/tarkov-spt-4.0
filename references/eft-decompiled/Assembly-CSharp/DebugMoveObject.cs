using UnityEngine;

public class DebugMoveObject : MonoBehaviour
{
	public void Update()
	{
		base.transform.position = base.transform.position + Vector3.one * 0.01f;
	}
}
