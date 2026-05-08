using UnityEngine;

public class PrimitiveBillboard : MonoBehaviour
{
	public void Update()
	{
		if (CameraClass.Instance.Camera != null)
		{
			base.transform.forward = -CameraClass.Instance.Camera.transform.forward;
		}
	}
}
