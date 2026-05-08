using System;
using UnityEngine;

public class GClass1256
{
	public float XSensitivity = 2f;

	public float YSensitivity = 2f;

	public bool clampVerticalRotation = true;

	public float MinimumX = -90f;

	public float MaximumX = 90f;

	public bool smooth;

	public float smoothTime = 5f;

	public bool lockCursor = true;

	[NonSerialized]
	public Quaternion Quaternion_0;

	[NonSerialized]
	public Quaternion Quaternion_1;

	[NonSerialized]
	public bool Bool_0 = true;

	public void Init(Transform character, Transform camera)
	{
		Quaternion_0 = character.localRotation;
		Quaternion_1 = camera.localRotation;
	}

	public void LookRotation(Transform character, Transform camera)
	{
		if (Bool_0)
		{
			float y = Input.GetAxis("Mouse X") * XSensitivity;
			float num = Input.GetAxis("Mouse Y") * YSensitivity;
			Quaternion_0 *= Quaternion.Euler(0f, y, 0f);
			Quaternion_1 *= Quaternion.Euler(0f - num, 0f, 0f);
			if (clampVerticalRotation)
			{
				Quaternion_1 = method_1(Quaternion_1);
			}
			if (smooth)
			{
				character.localRotation = Quaternion.Slerp(character.localRotation, Quaternion_0, smoothTime * Time.deltaTime);
				camera.localRotation = Quaternion.Slerp(camera.localRotation, Quaternion_1, smoothTime * Time.deltaTime);
			}
			else
			{
				character.localRotation = Quaternion_0;
				camera.localRotation = Quaternion_1;
			}
		}
		UpdateCursorLock();
	}

	public void SetCursorLock(bool value)
	{
		lockCursor = value;
		if (!lockCursor)
		{
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
	}

	public void UpdateCursorLock()
	{
		if (lockCursor)
		{
			method_0();
		}
	}

	public void method_0()
	{
		if (Input.GetKeyUp(KeyCode.Escape))
		{
			Bool_0 = false;
		}
		else if (Input.GetMouseButtonUp(0))
		{
			Bool_0 = true;
		}
		if (Bool_0)
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
		else
		{
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
	}

	public Quaternion method_1(Quaternion q)
	{
		q.x /= q.w;
		q.y /= q.w;
		q.z /= q.w;
		q.w = 1f;
		float value = 114.59156f * Mathf.Atan(q.x);
		value = Mathf.Clamp(value, MinimumX, MaximumX);
		q.x = Mathf.Tan(MathF.PI / 360f * value);
		return q;
	}
}
