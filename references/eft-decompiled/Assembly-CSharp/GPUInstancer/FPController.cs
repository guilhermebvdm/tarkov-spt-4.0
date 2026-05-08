using UnityEngine;

namespace GPUInstancer;

[RequireComponent(typeof(CharacterController))]
public class FPController : MonoBehaviour
{
	[SerializeField]
	public float m_WalkSpeed;

	[SerializeField]
	public float m_RunSpeed;

	[SerializeField]
	public float m_JumpSpeed;

	private bool bool_0;

	private GClass1256 gclass1256_0;

	private Camera camera_0;

	private bool bool_1;

	private float float_0;

	private Vector2 vector2_0;

	private Vector3 vector3_0 = Vector3.zero;

	private CharacterController characterController_0;

	private CollisionFlags collisionFlags_0;

	private bool bool_2;

	private bool bool_3;

	private float float_1 = 10f;

	private float float_2 = 2f;

	public void Start()
	{
		characterController_0 = GetComponent<CharacterController>();
		camera_0 = Camera.main;
		bool_3 = false;
		gclass1256_0 = new GClass1256();
		gclass1256_0.Init(base.transform, camera_0.transform);
	}

	public void Update()
	{
		method_1();
		if (!bool_1 && Cursor.lockState == CursorLockMode.Locked)
		{
			bool_1 = Input.GetButtonDown("Jump");
		}
		if (!bool_2 && characterController_0.isGrounded)
		{
			vector3_0.y = 0f;
			bool_3 = false;
		}
		if (!characterController_0.isGrounded && !bool_3 && bool_2)
		{
			vector3_0.y = 0f;
		}
		bool_2 = characterController_0.isGrounded;
	}

	public void FixedUpdate()
	{
		method_0(out var speed);
		Vector3 vector = base.transform.forward * vector2_0.y + base.transform.right * vector2_0.x;
		Physics.SphereCast(base.transform.position, characterController_0.radius, Vector3.down, out var hitInfo, characterController_0.height / 2f, -1, QueryTriggerInteraction.Ignore);
		vector = Vector3.ProjectOnPlane(vector, hitInfo.normal).normalized;
		vector3_0.x = vector.x * speed;
		vector3_0.z = vector.z * speed;
		if (characterController_0.isGrounded)
		{
			vector3_0.y = 0f - float_1;
			if (bool_1)
			{
				vector3_0.y = m_JumpSpeed;
				bool_1 = false;
				bool_3 = true;
			}
		}
		else
		{
			vector3_0 += Physics.gravity * float_2 * Time.fixedDeltaTime;
		}
		collisionFlags_0 = characterController_0.Move(vector3_0 * Time.fixedDeltaTime);
		gclass1256_0.UpdateCursorLock();
	}

	public void method_0(out float speed)
	{
		float axis = Input.GetAxis("Horizontal");
		float axis2 = Input.GetAxis("Vertical");
		bool_0 = !Input.GetKey(KeyCode.LeftShift);
		speed = (bool_0 ? m_WalkSpeed : m_RunSpeed);
		vector2_0 = new Vector2(axis, axis2);
		if (vector2_0.sqrMagnitude > 1f)
		{
			vector2_0.Normalize();
		}
	}

	public void method_1()
	{
		gclass1256_0.LookRotation(base.transform, camera_0.transform);
	}

	public void OnControllerColliderHit(ControllerColliderHit hit)
	{
		Rigidbody attachedRigidbody = hit.collider.attachedRigidbody;
		if (collisionFlags_0 != CollisionFlags.Below && !(attachedRigidbody == null) && !attachedRigidbody.isKinematic)
		{
			attachedRigidbody.AddForceAtPosition(characterController_0.velocity * 0.1f, hit.point, ForceMode.Impulse);
		}
	}
}
