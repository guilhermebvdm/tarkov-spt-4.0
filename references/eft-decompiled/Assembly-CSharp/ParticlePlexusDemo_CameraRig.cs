using UnityEngine;

public class ParticlePlexusDemo_CameraRig : MonoBehaviour
{
	public Transform pivot;

	private Vector3 vector3_0;

	[Range(0f, 90f)]
	public float rotationLimit = 90f;

	public float rotationSpeed = 2f;

	public float rotationLerpSpeed = 4f;

	private Vector3 vector3_1;

	public void Start()
	{
		vector3_1 = pivot.localEulerAngles;
		vector3_0 = vector3_1;
	}

	public void Update()
	{
		float axis = Input.GetAxis("Horizontal");
		float axis2 = Input.GetAxis("Vertical");
		if (Input.GetKeyDown(KeyCode.R))
		{
			vector3_0 = vector3_1;
		}
		axis *= rotationSpeed;
		axis2 *= rotationSpeed;
		vector3_0.y += axis;
		vector3_0.x += axis2;
		vector3_0.x = Mathf.Clamp(vector3_0.x, 0f - rotationLimit, rotationLimit);
		vector3_0.y = Mathf.Clamp(vector3_0.y, 0f - rotationLimit, rotationLimit);
		Vector3 localEulerAngles = pivot.localEulerAngles;
		localEulerAngles.x = Mathf.LerpAngle(localEulerAngles.x, vector3_0.x, Time.deltaTime * rotationLerpSpeed);
		localEulerAngles.y = Mathf.LerpAngle(localEulerAngles.y, vector3_0.y, Time.deltaTime * rotationLerpSpeed);
		pivot.localEulerAngles = localEulerAngles;
	}
}
