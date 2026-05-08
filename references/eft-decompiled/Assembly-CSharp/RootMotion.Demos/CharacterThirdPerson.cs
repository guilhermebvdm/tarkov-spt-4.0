using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace RootMotion.Demos;

public class CharacterThirdPerson : CharacterBase
{
	[Serializable]
	public enum MoveMode
	{
		Directional,
		Strafe
	}

	public struct GStruct145
	{
		public Vector3 moveDirection;

		public bool jump;

		public bool crouch;

		public bool onGround;

		public bool isStrafing;

		public float yVelocity;
	}

	[Header("References")]
	[SerializeField]
	private CharacterAnimationBase characterAnimation;

	[SerializeField]
	private UserControlThirdPerson userControl;

	[SerializeField]
	private CameraController cam;

	[Header("Movement")]
	public MoveMode moveMode;

	public bool smoothPhysics = true;

	public float smoothAccelerationTime = 0.2f;

	public float linearAccelerationSpeed = 3f;

	public float platformFriction = 7f;

	public float groundStickyEffect = 4f;

	public float maxVerticalVelocityOnGround = 3f;

	public float velocityToGroundTangentWeight;

	[Header("Rotation")]
	public bool lookInCameraDirection;

	public float turnSpeed = 5f;

	public float stationaryTurnSpeedMlp = 1f;

	[Header("Jumping and Falling")]
	public float airSpeed = 6f;

	public float airControl = 2f;

	public float jumpPower = 12f;

	public float jumpRepeatDelayTime;

	[Header("Wall Running")]
	[SerializeField]
	private LayerMask wallRunLayers;

	public float wallRunMaxLength = 1f;

	public float wallRunMinMoveMag = 0.6f;

	public float wallRunMinVelocityY = -1f;

	public float wallRunRotationSpeed = 1.5f;

	public float wallRunMaxRotationAngle = 70f;

	public float wallRunWeightSpeed = 5f;

	[Header("Crouching")]
	public float crouchCapsuleScaleMlp = 0.6f;

	[CompilerGenerated]
	private bool bool_0;

	public GStruct145 animState;

	private Vector3 vector3_0;

	private Animator animator_0;

	private Vector3 vector3_1;

	private Vector3 vector3_2;

	private RaycastHit raycastHit_0;

	private float float_0;

	private float float_1;

	private float float_2;

	private float float_3;

	private float float_4;

	private float float_5;

	private Vector3 vector3_3 = Vector3.up;

	private Vector3 vector3_4;

	private float float_6;

	private float float_7;

	private Vector3 vector3_5;

	private bool bool_1;

	private float float_8;

	public bool onGround
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

	public override void Start()
	{
		base.Start();
		animator_0 = GetComponent<Animator>();
		vector3_3 = Vector3.up;
		onGround = true;
		animState.onGround = true;
		if (cam != null)
		{
			cam.enabled = false;
		}
	}

	public void OnAnimatorMove()
	{
		Move(animator_0.deltaPosition);
	}

	public override void Move(Vector3 deltaPosition)
	{
		vector3_5 += deltaPosition;
	}

	public void FixedUpdate()
	{
		r.interpolation = (smoothPhysics ? RigidbodyInterpolation.Interpolate : RigidbodyInterpolation.None);
		characterAnimation.smoothFollow = smoothPhysics;
		method_0(vector3_5);
		vector3_5 = Vector3.zero;
		Rotate();
		method_5();
		if (userControl.state.move == Vector3.zero && float_3 < airborneThreshold * 0.5f)
		{
			HighFriction();
		}
		else
		{
			ZeroFriction();
		}
		if (onGround)
		{
			animState.jump = Jump();
		}
		else
		{
			r.AddForce(Physics.gravity * gravityMultiplier - Physics.gravity);
		}
		ScaleCapsule(userControl.state.crouch ? crouchCapsuleScaleMlp : 1f);
		bool_1 = true;
	}

	public virtual void Update()
	{
		animState.onGround = onGround;
		animState.moveDirection = method_3();
		animState.yVelocity = r.velocity.y;
		animState.crouch = userControl.state.crouch;
		animState.isStrafing = moveMode == MoveMode.Strafe;
	}

	public virtual void LateUpdate()
	{
		if (!(cam == null))
		{
			cam.UpdateInput();
			if (bool_1 || r.interpolation != RigidbodyInterpolation.None)
			{
				cam.UpdateTransform((r.interpolation == RigidbodyInterpolation.None) ? Time.fixedDeltaTime : Time.deltaTime);
				bool_1 = false;
			}
		}
	}

	public void method_0(Vector3 deltaPosition)
	{
		method_1();
		Vector3 vector = deltaPosition / Time.deltaTime;
		vector += new Vector3(vector3_2.x, 0f, vector3_2.z);
		if (onGround)
		{
			if (velocityToGroundTangentWeight > 0f)
			{
				Quaternion b = Quaternion.FromToRotation(base.transform.up, vector3_1);
				vector = Quaternion.Lerp(Quaternion.identity, b, velocityToGroundTangentWeight) * vector;
			}
		}
		else
		{
			vector = Vector3.Lerp(b: new Vector3(userControl.state.move.x * airSpeed, 0f, userControl.state.move.z * airSpeed), a: r.velocity, t: Time.deltaTime * airControl);
		}
		if (onGround && Time.time > float_1)
		{
			r.velocity -= base.transform.up * float_5 * Time.deltaTime;
		}
		vector.y = Mathf.Clamp(r.velocity.y, r.velocity.y, onGround ? maxVerticalVelocityOnGround : r.velocity.y);
		r.velocity = vector;
		float b2 = ((!onGround) ? 1f : GetSlopeDamper(-deltaPosition / Time.deltaTime, vector3_1));
		float_2 = Mathf.Lerp(float_2, b2, Time.deltaTime * 5f);
	}

	public void method_1()
	{
		bool flag = method_2();
		if (float_6 > 0f && !flag)
		{
			float_8 = Time.time;
		}
		if (Time.time < float_8 + 0.5f)
		{
			flag = false;
		}
		float_6 = Mathf.MoveTowards(float_6, flag ? 1f : 0f, Time.deltaTime * wallRunWeightSpeed);
		if (float_6 <= 0f && float_7 > 0f)
		{
			base.transform.rotation = Quaternion.LookRotation(new Vector3(base.transform.forward.x, 0f, base.transform.forward.z), Vector3.up);
			vector3_3 = Vector3.up;
		}
		float_7 = float_6;
		if (!(float_6 <= 0f))
		{
			if (onGround && r.velocity.y < 0f)
			{
				r.velocity = new Vector3(r.velocity.x, 0f, r.velocity.z);
			}
			Vector3 forward = base.transform.forward;
			forward.y = 0f;
			RaycastHit hitInfo = new RaycastHit
			{
				normal = Vector3.up
			};
			Physics.Raycast(onGround ? base.transform.position : capsule.bounds.center, forward, out hitInfo, 3f, wallRunLayers);
			vector3_3 = Vector3.Lerp(vector3_3, hitInfo.normal, Time.deltaTime * wallRunRotationSpeed);
			vector3_3 = Vector3.RotateTowards(Vector3.up, vector3_3, wallRunMaxRotationAngle * (MathF.PI / 180f), 0f);
			Vector3 tangent = base.transform.forward;
			Vector3 normal = vector3_3;
			Vector3.OrthoNormalize(ref normal, ref tangent);
			base.transform.rotation = Quaternion.Slerp(Quaternion.LookRotation(forward, Vector3.up), Quaternion.LookRotation(tangent, vector3_3), float_6);
		}
	}

	public bool method_2()
	{
		if (Time.time < float_1 - 0.1f)
		{
			return false;
		}
		if (Time.time > float_1 - 0.1f + wallRunMaxLength)
		{
			return false;
		}
		if (r.velocity.y < wallRunMinVelocityY)
		{
			return false;
		}
		if (userControl.state.move.magnitude < wallRunMinMoveMag)
		{
			return false;
		}
		return true;
	}

	public Vector3 method_3()
	{
		switch (moveMode)
		{
		default:
			return Vector3.zero;
		case MoveMode.Strafe:
			vector3_0 = Vector3.SmoothDamp(vector3_0, userControl.state.move, ref vector3_4, smoothAccelerationTime);
			vector3_0 = Vector3.MoveTowards(vector3_0, userControl.state.move, Time.deltaTime * linearAccelerationSpeed);
			return base.transform.InverseTransformDirection(vector3_0);
		case MoveMode.Directional:
			vector3_0 = Vector3.SmoothDamp(vector3_0, new Vector3(0f, 0f, userControl.state.move.magnitude), ref vector3_4, smoothAccelerationTime);
			vector3_0 = Vector3.MoveTowards(vector3_0, new Vector3(0f, 0f, userControl.state.move.magnitude), Time.deltaTime * linearAccelerationSpeed);
			return vector3_0 * float_2;
		}
	}

	public virtual void Rotate()
	{
		float num = GetAngleFromForward(method_4());
		if (userControl.state.move == Vector3.zero)
		{
			num *= (1.01f - Mathf.Abs(num) / 180f) * stationaryTurnSpeedMlp;
		}
		RigidbodyRotateAround(characterAnimation.GetPivotPoint(), base.transform.up, num * Time.deltaTime * turnSpeed);
	}

	public Vector3 method_4()
	{
		bool flag = userControl.state.move != Vector3.zero;
		switch (moveMode)
		{
		default:
			return Vector3.zero;
		case MoveMode.Strafe:
			if (flag)
			{
				return userControl.state.lookPos - r.position;
			}
			if (!lookInCameraDirection)
			{
				return base.transform.forward;
			}
			return userControl.state.lookPos - r.position;
		case MoveMode.Directional:
			if (flag)
			{
				return userControl.state.move;
			}
			if (!lookInCameraDirection)
			{
				return base.transform.forward;
			}
			return userControl.state.lookPos - r.position;
		}
	}

	public virtual bool Jump()
	{
		if (!userControl.state.jump)
		{
			return false;
		}
		if (userControl.state.crouch)
		{
			return false;
		}
		if (!characterAnimation.animationGrounded)
		{
			return false;
		}
		if (Time.time < float_4 + jumpRepeatDelayTime)
		{
			return false;
		}
		onGround = false;
		float_1 = Time.time + 0.1f;
		Vector3 velocity = userControl.state.move * airSpeed;
		r.velocity = velocity;
		r.velocity += base.transform.up * jumpPower;
		return true;
	}

	public void method_5()
	{
		Vector3 b = Vector3.zero;
		float num = 0f;
		raycastHit_0 = GetSpherecastHit();
		vector3_1 = base.transform.up;
		float_3 = r.position.y - raycastHit_0.point.y;
		if (Time.time > float_1 && r.velocity.y < jumpPower * 0.5f)
		{
			bool num2 = onGround;
			onGround = false;
			float num3 = ((!num2) ? (airborneThreshold * 0.5f) : airborneThreshold);
			Vector3 velocity = r.velocity;
			velocity.y = 0f;
			float magnitude = velocity.magnitude;
			if (float_3 < num3)
			{
				num = groundStickyEffect * magnitude * num3;
				if (raycastHit_0.rigidbody != null)
				{
					b = raycastHit_0.rigidbody.GetPointVelocity(raycastHit_0.point);
				}
				onGround = true;
			}
		}
		vector3_2 = Vector3.Lerp(vector3_2, b, Time.deltaTime * platformFriction);
		float_5 = num;
		if (!onGround)
		{
			float_4 = Time.time;
		}
	}
}
