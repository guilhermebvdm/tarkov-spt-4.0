using UnityEngine;

public class PlayerMotor : MonoBehaviour
{
	public abstract class GClass765
	{
		public const float PRONE_POSE = 0f;

		public const float CROUCH_POSE = 0.5f;

		public const float STAND_POSE = 1f;

		public const float SPEED_MIN = 0f;

		public const float SPEED_MAX = 1f;

		public const string POSE_LEVEL_PARAM_NAME = "Level";

		public const string DIRECTION_X_PARAM_NAME = "Direct_X";

		public const string DIRECTION_Y_PARAM_NAME = "Direct_Y";

		public const string SPEED_PARAM_NAME = "Speed";
	}

	public Vector3 LocalVelocity;

	private RaycastHit raycastHit_0;

	[SerializeField]
	private float _checkGroundedRayDistance = 1f;

	private Animator animator_0;

	private Vector2 vector2_0;

	private float float_0;

	private float float_1 = 1f;

	private Rigidbody rigidbody_0;

	private float float_2;

	public bool IsGrounded => raycastHit_0.collider != null;

	public Vector2 MovementDirection
	{
		get
		{
			return vector2_0;
		}
		set
		{
			vector2_0 = Vector2.ClampMagnitude(value, 1f);
			animator_0.SetFloat("Direct_X", vector2_0.x);
			animator_0.SetFloat("Direct_Y", vector2_0.y);
		}
	}

	public float PoseLevel
	{
		get
		{
			return float_1;
		}
		set
		{
			float_1 = Mathf.Clamp(value, 0f, 1f);
			animator_0.SetFloat("Level", float_1);
		}
	}

	public float Speed
	{
		get
		{
			return float_0;
		}
		set
		{
			float_0 = Mathf.Clamp(value, 0f, 1f);
			animator_0.SetFloat("Speed", float_0);
		}
	}

	public virtual void Awake()
	{
		animator_0 = GetComponent<Animator>();
		rigidbody_0 = GetComponent<Rigidbody>();
		PoseLevel = 1f;
		Speed = 0f;
	}

	public void CheckGrounded()
	{
		Physics.Raycast(new Ray(base.transform.position + Vector3.up * 0.5f, Vector3.down), out raycastHit_0, _checkGroundedRayDistance);
	}

	public virtual void Move(Vector2 direction, float speed)
	{
		MovementDirection = direction;
		Speed = speed;
	}

	public void FixedUpdate()
	{
		Speed = 0f;
	}

	public void SetYaw(float yaw)
	{
		float y = base.transform.eulerAngles.y;
		float value = Mathf.DeltaAngle(base.transform.eulerAngles.y, yaw);
		animator_0.SetFloat("DeltaRotation", value);
		float num = Mathf.DeltaAngle(y, base.transform.rotation.eulerAngles.y);
		float_2 = Mathf.Lerp(float_2, num / Time.deltaTime, Time.deltaTime);
		animator_0.SetFloat("RotateSpeed", float_2);
	}

	public Vector3 ConvertToWorldXZCoordintaes(Vector3 vector)
	{
		Vector3 forward = base.transform.forward;
		forward.y = 0f;
		forward.Normalize();
		return Quaternion.LookRotation(forward) * vector;
	}

	public void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawLine(base.transform.position, base.transform.position + rigidbody_0.velocity);
	}

	public void OnGUI()
	{
		GUI.Label(new Rect(10f, 10f, 200f, 40f), rigidbody_0.velocity.ToString());
		GUI.Label(new Rect(10f, 50f, 200f, 40f), (raycastHit_0.collider != null) ? raycastHit_0.collider.ToString() : "NO COLLISION");
	}
}
