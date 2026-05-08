using UnityEngine;

public class ThrowableSettings : MonoBehaviour
{
	[SerializeField]
	public Vector3 Offset;

	[SerializeField]
	[Header("Threshold sqrMagnitude")]
	public float VelocityTreshold = 0.005f;
}
