using UnityEngine;

public class RotateObject : MonoBehaviour
{
	public void Start()
	{
	}

	public void Update()
	{
		base.transform.Rotate(Vector3.up * Time.deltaTime * 15f, Space.World);
	}
}
