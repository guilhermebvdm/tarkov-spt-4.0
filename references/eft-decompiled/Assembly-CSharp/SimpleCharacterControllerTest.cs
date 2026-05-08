using UnityEngine;

public class SimpleCharacterControllerTest : MonoBehaviour
{
	private SimpleCharacterController simpleCharacterController_0;

	[SerializeField]
	private CapsuleCollider _mainCollider;

	private Vector3 vector3_0;

	public bool IsGrounded;

	public void Awake()
	{
		simpleCharacterController_0 = base.gameObject.AddComponent<SimpleCharacterController>();
		simpleCharacterController_0.Init(_mainCollider.transform, new Collider[1] { _mainCollider }, _mainCollider, EFTHardSettings.Instance.SIMPLE_CHARACTER_CONTROLLER_FIXED_DELTA_DISTANCE, 1, 0.25f, 60f);
	}

	public void Update()
	{
		Vector3 motion = base.transform.position - vector3_0;
		simpleCharacterController_0.Move(motion, Time.deltaTime);
		base.transform.position = simpleCharacterController_0.GetCollider().transform.position;
		vector3_0 = base.transform.position;
		IsGrounded = simpleCharacterController_0.isGrounded;
	}
}
