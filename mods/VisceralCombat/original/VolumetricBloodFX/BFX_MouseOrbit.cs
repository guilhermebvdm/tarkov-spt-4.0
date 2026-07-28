using System;
using UnityEngine;

public class BFX_MouseOrbit : MonoBehaviour
{
	public GameObject target;

	public float distance = 10f;

	public float xSpeed = 250f;

	public float ySpeed = 120f;

	public float yMinLimit = -20f;

	public float yMaxLimit = 80f;

	private float x = 0f;

	private float y = 0f;

	private float prevDistance;

	private void Start()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		Vector3 eulerAngles = ((Component)this).transform.eulerAngles;
		x = eulerAngles.y;
		y = eulerAngles.x;
	}

	private void LateUpdate()
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_0245: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		//IL_025f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_026c: Unknown result type (might be due to invalid IL or missing references)
		//IL_027a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03db: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0402: Unknown result type (might be due to invalid IL or missing references)
		//IL_0410: Unknown result type (might be due to invalid IL or missing references)
		//IL_02eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0305: Unknown result type (might be due to invalid IL or missing references)
		//IL_030a: Unknown result type (might be due to invalid IL or missing references)
		//IL_031a: Unknown result type (might be due to invalid IL or missing references)
		//IL_031f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0324: Unknown result type (might be due to invalid IL or missing references)
		//IL_032c: Unknown result type (might be due to invalid IL or missing references)
		//IL_033a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		if (distance < 2f)
		{
			distance = 2f;
		}
		distance -= Input.GetAxis("Mouse ScrollWheel") * 2f;
		if (Object.op_Implicit((Object)(object)target) && Input.GetMouseButton(1))
		{
			Vector3 mousePosition = Input.mousePosition;
			float num = 1f;
			if (Screen.dpi < 1f)
			{
				num = 1f;
			}
			num = ((!(Screen.dpi < 200f)) ? (Screen.dpi / 200f) : 1f);
			if (mousePosition.x < 380f * num && (float)Screen.height - mousePosition.y < 250f * num)
			{
				return;
			}
			Cursor.visible = false;
			Cursor.lockState = (CursorLockMode)1;
			x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
			y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
			y = ClampAngle(y, yMinLimit, yMaxLimit);
			Quaternion val = Quaternion.Euler(y, x, 0f);
			Vector3 position = val * new Vector3(0f, 0f, 0f - distance) + target.transform.position;
			((Component)this).transform.rotation = val;
			((Component)this).transform.position = position;
		}
		else
		{
			Cursor.visible = true;
			Cursor.lockState = (CursorLockMode)0;
		}
		if (Input.GetKey((KeyCode)275))
		{
			if ((double)Time.timeScale < 0.5)
			{
				x += 1.6666667f;
			}
			else
			{
				x += Time.deltaTime * 3f;
			}
			Quaternion val2 = Quaternion.Euler(y, x, 0f);
			Vector3 position2 = val2 * new Vector3(0f, 0f, 0f - distance) + target.transform.position;
			((Component)this).transform.rotation = val2;
			((Component)this).transform.position = position2;
		}
		if (Input.GetKey((KeyCode)276))
		{
			if ((double)Time.timeScale < 0.5)
			{
				x -= 1.6666667f;
			}
			else
			{
				x -= Time.deltaTime * 3f;
			}
			Quaternion val3 = Quaternion.Euler(y, x, 0f);
			Vector3 position3 = val3 * new Vector3(0f, 0f, 0f - distance) + target.transform.position;
			((Component)this).transform.rotation = val3;
			((Component)this).transform.position = position3;
		}
		if (Input.GetKeyDown((KeyCode)273))
		{
			if ((double)Time.timeScale > 0.5)
			{
				Time.timeScale = 0f;
			}
			else
			{
				Time.timeScale = 1f;
			}
		}
		if (Math.Abs(prevDistance - distance) > 0.001f)
		{
			prevDistance = distance;
			Quaternion val4 = Quaternion.Euler(y, x, 0f);
			Vector3 position4 = val4 * new Vector3(0f, 0f, 0f - distance) + target.transform.position;
			((Component)this).transform.rotation = val4;
			((Component)this).transform.position = position4;
		}
	}

	private static float ClampAngle(float angle, float min, float max)
	{
		if (angle < -360f)
		{
			angle += 360f;
		}
		if (angle > 360f)
		{
			angle -= 360f;
		}
		return Mathf.Clamp(angle, min, max);
	}
}
