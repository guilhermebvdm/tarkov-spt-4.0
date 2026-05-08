using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace RootMotion.Demos;

[RequireComponent(typeof(Animator))]
public class SimpleLocomotion : MonoBehaviour
{
	[Serializable]
	public enum RotationMode
	{
		Smooth,
		Linear
	}

	[Tooltip("The component that updates the camera.")]
	[SerializeField]
	private CameraController cameraController;

	[Tooltip("Acceleration of movement.")]
	[SerializeField]
	private float accelerationTime = 0.2f;

	[Tooltip("Turning speed.")]
	[SerializeField]
	private float turnTime = 0.2f;

	[Tooltip("If true, will run on left shift, if not will walk on left shift.")]
	[SerializeField]
	private bool walkByDefault = true;

	[Tooltip("Smooth or linear rotation.")]
	[SerializeField]
	private RotationMode rotationMode;

	[Tooltip("Procedural motion speed (if not using root motion).")]
	[SerializeField]
	private float moveSpeed = 3f;

	[CompilerGenerated]
	private bool bool_0;

	private Animator animator_0;

	private float float_0;

	private float float_1;

	private float float_2;

	private Vector3 vector3_0;

	private CharacterController characterController_0;

	public bool isGrounded
	{
		[CompilerGenerated]
		get
		{
			return bool_0;
		}
		[CompilerGenerated]
		set
		{
			bool_0 = value;
		}
	}

	public void Start()
	{
		animator_0 = GetComponent<Animator>();
		characterController_0 = GetComponent<CharacterController>();
		cameraController.enabled = false;
	}

	public void Update()
	{
		isGrounded = base.transform.position.y < 0.1f;
		method_0();
		method_1();
	}

	public void LateUpdate()
	{
		cameraController.UpdateInput();
		cameraController.UpdateTransform();
	}

	public void method_0()
	{
		if (!isGrounded)
		{
			return;
		}
		Vector3 vector = method_2();
		if (vector == Vector3.zero)
		{
			return;
		}
		Vector3 forward = base.transform.forward;
		switch (rotationMode)
		{
		case RotationMode.Linear:
		{
			Vector3 vector3 = method_3();
			if (vector3 != Vector3.zero)
			{
				vector3_0 = cameraController.transform.rotation * vector3;
			}
			forward = Vector3.RotateTowards(forward, vector3_0, Time.deltaTime * (1f / turnTime), 1f);
			forward.y = 0f;
			base.transform.rotation = Quaternion.LookRotation(forward);
			break;
		}
		case RotationMode.Smooth:
		{
			Vector3 vector2 = cameraController.transform.rotation * vector;
			float current = Mathf.Atan2(forward.x, forward.z) * 57.29578f;
			float target = Mathf.Atan2(vector2.x, vector2.z) * 57.29578f;
			float angle = Mathf.SmoothDampAngle(current, target, ref float_1, turnTime);
			base.transform.rotation = Quaternion.AngleAxis(angle, Vector3.up);
			break;
		}
		}
	}

	public void method_1()
	{
		float target = ((!walkByDefault) ? (Input.GetKey(KeyCode.LeftShift) ? 0.5f : 1f) : (Input.GetKey(KeyCode.LeftShift) ? 1f : 0.5f));
		float_0 = Mathf.SmoothDamp(float_0, target, ref float_2, accelerationTime);
		float num = method_2().magnitude * float_0;
		animator_0.SetFloat("Speed", num);
		if (!animator_0.hasRootMotion && isGrounded)
		{
			Vector3 vector = base.transform.forward * num * moveSpeed;
			if (characterController_0 != null)
			{
				characterController_0.SimpleMove(vector);
			}
			else
			{
				base.transform.position += vector * Time.deltaTime;
			}
		}
	}

	public Vector3 method_2()
	{
		Vector3 result = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
		result.z += Mathf.Abs(result.x) * 0.05f;
		result.x -= Mathf.Abs(result.z) * 0.05f;
		return result;
	}

	public Vector3 method_3()
	{
		return new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
	}
}
