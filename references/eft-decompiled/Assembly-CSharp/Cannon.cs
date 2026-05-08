using System.Collections;
using UnityEngine;

public class Cannon : MonoBehaviour
{
	public float Distance = 16f;

	public float XSpeed = 250f;

	public float YSpeed = 120f;

	public float YMinLimit = -20f;

	public float YMaxLimit = 80f;

	[Range(0f, 50000f)]
	public float Force = 10000f;

	public Transform Target;

	private float float_0;

	private float float_1;

	public void Start()
	{
		Vector3 eulerAngles = base.transform.eulerAngles;
		float_0 = eulerAngles.y;
		float_1 = eulerAngles.x;
		method_0();
	}

	public void Update()
	{
		if (Input.GetKeyUp(KeyCode.Mouse0))
		{
			StartCoroutine(method_1());
		}
		if ((bool)Target && Input.GetKey(KeyCode.Mouse1))
		{
			method_0();
		}
	}

	public void method_0()
	{
		float_0 += (float)((double)Input.GetAxis("Mouse X") * (double)XSpeed * 0.0199999995529652);
		float_1 -= (float)((double)Input.GetAxis("Mouse Y") * (double)YSpeed * 0.0199999995529652);
		float_1 = smethod_0(float_1, YMinLimit, YMaxLimit);
		Quaternion quaternion = Quaternion.Euler(float_1, float_0, 0f);
		Vector3 position = quaternion * new Vector3(0f, 0f, 0f - Distance) + Target.position;
		base.transform.rotation = quaternion;
		base.transform.position = position;
	}

	public static float smethod_0(float angle, float min, float max)
	{
		if ((double)angle < -360.0)
		{
			angle += 360f;
		}
		if ((double)angle > 360.0)
		{
			angle -= 360f;
		}
		return Mathf.Clamp(angle, min, max);
	}

	public IEnumerator method_1()
	{
		Ray ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
		if (!Physics.Raycast(ray, out var hitInfo, 100f))
		{
			yield break;
		}
		Rigidbody attachedRigidbody = hitInfo.collider.attachedRigidbody;
		if (attachedRigidbody != null)
		{
			attachedRigidbody.AddForceAtPosition(Force * ray.direction, hitInfo.point);
		}
		yield return new WaitForSeconds(0.1f);
		if ((bool)hitInfo.collider)
		{
			Component component = hitInfo.collider.GetComponent(typeof(BoxFracture));
			if (component is BoxFracture)
			{
				(component as BoxFracture).Destroy(hitInfo.point, 1f);
			}
		}
	}
}
