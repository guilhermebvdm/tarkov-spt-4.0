using UnityEngine;

public class AlwaysUp : MonoBehaviour
{
	public void Update()
	{
		base.transform.LookAt(base.transform.position + base.transform.parent.forward, Vector3.up);
	}
}
