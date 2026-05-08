using System;
using UnityEngine;

namespace RootMotion.FinalIK;

[Serializable]
public class Grounding
{
	[Serializable]
	public enum Quality
	{
		Fastest,
		Simple,
		Best
	}

	public class Leg
	{
		[NonSerialized]
		public Grounding Grounding;

		[NonSerialized]
		public float LastTime;

		[NonSerialized]
		public float DeltaTime;

		[NonSerialized]
		public Vector3 LastPosition;

		[NonSerialized]
		public Quaternion ToHitNormal;

		[NonSerialized]
		public Quaternion r;

		[NonSerialized]
		public RaycastHit HeelHit;

		[NonSerialized]
		public Vector3 up = Vector3.up;

		[field: NonSerialized]
		public bool isGrounded { get; set; }

		[field: NonSerialized]
		public Vector3 IKPosition { get; set; }

		[field: NonSerialized]
		public Quaternion rotationOffset { get; set; }

		[field: NonSerialized]
		public bool initiated { get; set; }

		[field: NonSerialized]
		public float heightFromGround { get; set; }

		[field: NonSerialized]
		public Vector3 velocity { get; set; }

		[field: NonSerialized]
		public Transform transform { get; set; }

		[field: NonSerialized]
		public float IKOffset { get; set; }

		public float stepHeightFromGround => Mathf.Clamp(heightFromGround, 0f - Grounding.maxStep, Grounding.maxStep);

		public float Single_0 => Grounding.GetVerticalOffset(transform.position, Grounding.root.position - up * Grounding.heightOffset);

		public void Initiate(Grounding grounding, Transform transform)
		{
			initiated = false;
			Grounding = grounding;
			this.transform = transform;
			up = Vector3.up;
			IKPosition = transform.position;
			initiated = true;
			OnEnable();
		}

		public void OnEnable()
		{
			if (initiated)
			{
				LastPosition = transform.position;
				LastTime = Time.deltaTime;
			}
		}

		public void Reset()
		{
			LastPosition = transform.position;
			LastTime = Time.deltaTime;
			IKOffset = 0f;
			IKPosition = transform.position;
			rotationOffset = Quaternion.identity;
		}

		public void Process()
		{
			if (!initiated || Grounding.maxStep <= 0f)
			{
				return;
			}
			DeltaTime = Time.time - LastTime;
			LastTime = Time.time;
			if (DeltaTime == 0f)
			{
				return;
			}
			up = Grounding.up;
			heightFromGround = float.PositiveInfinity;
			velocity = (transform.position - LastPosition) / DeltaTime;
			velocity = Grounding.Flatten(velocity);
			LastPosition = transform.position;
			Vector3 vector = velocity * Grounding.prediction;
			if (Grounding.footRadius <= 0f)
			{
				Grounding.quality = Quality.Fastest;
			}
			switch (Grounding.quality)
			{
			case Quality.Fastest:
			{
				RaycastHit raycastHit4 = method_1(vector);
				method_3(raycastHit4.normal, raycastHit4.point);
				break;
			}
			case Quality.Simple:
			{
				HeelHit = method_1(Vector3.zero);
				RaycastHit raycastHit2 = method_1(Grounding.root.forward * Grounding.footRadius + vector);
				RaycastHit raycastHit3 = method_1(Grounding.root.right * Grounding.footRadius * 0.5f);
				Vector3 vector2 = Vector3.Cross(raycastHit2.point - HeelHit.point, raycastHit3.point - HeelHit.point).normalized;
				if (Vector3.Dot(vector2, up) < 0f)
				{
					vector2 = -vector2;
				}
				method_4(vector2, HeelHit.point, HeelHit.point);
				break;
			}
			case Quality.Best:
			{
				HeelHit = method_1(Vector3.zero);
				RaycastHit raycastHit = method_0(vector);
				method_4(raycastHit.normal, raycastHit.point, HeelHit.point);
				break;
			}
			}
			isGrounded = heightFromGround < Grounding.maxStep;
			float num = stepHeightFromGround;
			if (!Grounding.rootGrounded)
			{
				num = 0f;
			}
			IKOffset = GClass1462.LerpValue(IKOffset, num, Grounding.footSpeed, Grounding.footSpeed);
			IKOffset = Mathf.Lerp(IKOffset, num, DeltaTime * Grounding.footSpeed);
			float verticalOffset = Grounding.GetVerticalOffset(transform.position, Grounding.root.position);
			float num2 = Mathf.Clamp(Grounding.maxStep - verticalOffset, 0f, Grounding.maxStep);
			IKOffset = Mathf.Clamp(IKOffset, 0f - num2, IKOffset);
			method_6();
			IKPosition = transform.position - up * IKOffset;
			float footRotationWeight = Grounding.footRotationWeight;
			rotationOffset = ((footRotationWeight >= 1f) ? r : Quaternion.Slerp(Quaternion.identity, r, footRotationWeight));
		}

		public RaycastHit method_0(Vector3 offsetFromHeel)
		{
			RaycastHit hitInfo = default(RaycastHit);
			Vector3 vector = transform.position + Grounding.root.forward * Grounding.footRadius;
			hitInfo.point = vector - up * Grounding.maxStep * 2f;
			hitInfo.normal = up;
			Vector3 vector2 = vector + Grounding.maxStep * up;
			Vector3 point = vector2 + offsetFromHeel;
			Physics.CapsuleCast(vector2, point, Grounding.footRadius, -up, out hitInfo, Grounding.maxStep * 3f, Grounding.layers);
			return hitInfo;
		}

		public RaycastHit method_1(Vector3 offsetFromHeel)
		{
			RaycastHit hitInfo = default(RaycastHit);
			Vector3 vector = transform.position + offsetFromHeel;
			hitInfo.point = vector - up * Grounding.maxStep * 2f;
			hitInfo.normal = up;
			if (Grounding.maxStep <= 0f)
			{
				return hitInfo;
			}
			Physics.Raycast(vector + Grounding.maxStep * up, -up, out hitInfo, Grounding.maxStep * 3f, Grounding.layers);
			return hitInfo;
		}

		public Vector3 method_2(Vector3 normal)
		{
			if (Grounding.quality == Quality.Best)
			{
				return normal;
			}
			return Vector3.RotateTowards(up, normal, Grounding.maxFootRotationAngle * (MathF.PI / 180f), DeltaTime);
		}

		public void method_3(Vector3 normal, Vector3 point)
		{
			ToHitNormal = Quaternion.FromToRotation(up, method_2(normal));
			heightFromGround = method_5(point);
		}

		public void method_4(Vector3 planeNormal, Vector3 planePoint, Vector3 heelHitPoint)
		{
			planeNormal = method_2(planeNormal);
			ToHitNormal = Quaternion.FromToRotation(up, planeNormal);
			Vector3 hitPoint = GClass1464.LineToPlane(transform.position + up * Grounding.maxStep, -up, planeNormal, planePoint);
			heightFromGround = method_5(hitPoint);
			float max = method_5(heelHitPoint);
			heightFromGround = Mathf.Clamp(heightFromGround, float.NegativeInfinity, max);
		}

		public float method_5(Vector3 hitPoint)
		{
			return Grounding.GetVerticalOffset(transform.position, hitPoint) - Single_0;
		}

		public void method_6()
		{
			Quaternion b = method_7();
			r = Quaternion.Slerp(r, b, DeltaTime * Grounding.footRotationSpeed);
		}

		public Quaternion method_7()
		{
			if (Grounding.maxFootRotationAngle <= 0f)
			{
				return Quaternion.identity;
			}
			if (Grounding.maxFootRotationAngle >= 180f)
			{
				return ToHitNormal;
			}
			return Quaternion.RotateTowards(Quaternion.identity, ToHitNormal, Grounding.maxFootRotationAngle);
		}
	}

	public class Pelvis
	{
		[NonSerialized]
		public Grounding Grounding;

		[NonSerialized]
		public Vector3 LastRootPosition;

		[NonSerialized]
		public float DamperF;

		[NonSerialized]
		public bool Initiated;

		[NonSerialized]
		public float LastTime;

		[field: NonSerialized]
		public Vector3 IKOffset { get; set; }

		[field: NonSerialized]
		public float heightOffset { get; set; }

		public void Initiate(Grounding grounding)
		{
			Grounding = grounding;
			Initiated = true;
			OnEnable();
		}

		public void Reset()
		{
			LastRootPosition = Grounding.root.transform.position;
			LastTime = Time.deltaTime;
			IKOffset = Vector3.zero;
			heightOffset = 0f;
		}

		public void OnEnable()
		{
			if (Initiated)
			{
				LastRootPosition = Grounding.root.transform.position;
				LastTime = Time.time;
			}
		}

		public void Process(float lowestOffset, float highestOffset, bool isGrounded)
		{
			if (!Initiated)
			{
				return;
			}
			float num = Time.time - LastTime;
			LastTime = Time.time;
			if (!(num <= 0f))
			{
				float b = lowestOffset + highestOffset;
				if (!Grounding.rootGrounded)
				{
					b = 0f;
				}
				heightOffset = Mathf.Lerp(heightOffset, b, num * Grounding.pelvisSpeed);
				Vector3 p = Grounding.root.position - LastRootPosition;
				LastRootPosition = Grounding.root.position;
				DamperF = GClass1462.LerpValue(DamperF, isGrounded ? 1f : 0f, 1f, 10f);
				heightOffset -= Grounding.GetVerticalOffset(p, Vector3.zero) * Grounding.pelvisDamper * DamperF;
				IKOffset = Grounding.up * heightOffset;
			}
		}
	}

	[Tooltip("Layers to ground the character to. Make sure to exclude the layer of the character controller.")]
	public LayerMask layers;

	[Tooltip("Max step height. Maximum vertical distance of Grounding from the root of the character.")]
	public float maxStep = 0.5f;

	[Tooltip("The height offset of the root.")]
	public float heightOffset;

	[Tooltip("The speed of moving the feet up/down.")]
	public float footSpeed = 2.5f;

	[Tooltip("CapsuleCast radius. Should match approximately with the size of the feet.")]
	public float footRadius = 0.15f;

	[Tooltip("Amount of velocity based prediction of the foot positions.")]
	public float prediction = 0.05f;

	[Tooltip("Weight of rotating the feet to the ground normal offset.")]
	[Range(0f, 1f)]
	public float footRotationWeight = 1f;

	[Tooltip("Speed of slerping the feet to their grounded rotations.")]
	public float footRotationSpeed = 7f;

	[Tooltip("Max Foot Rotation Angle. Max angular offset from the foot's rotation.")]
	[Range(0f, 90f)]
	public float maxFootRotationAngle = 45f;

	[Tooltip("If true, solver will rotate with the character root so the character can be grounded for example to spherical planets. For performance reasons leave this off unless needed.")]
	public bool rotateSolver;

	[Tooltip("The speed of moving the character up/down.")]
	public float pelvisSpeed = 5f;

	[Tooltip("Used for smoothing out vertical pelvis movement (range 0 - 1).")]
	[Range(0f, 1f)]
	public float pelvisDamper;

	[Tooltip("The weight of lowering the pelvis to the lowest foot.")]
	public float lowerPelvisWeight = 1f;

	[Tooltip("The weight of lifting the pelvis to the highest foot. This is useful when you don't want the feet to go too high relative to the body when crouching.")]
	public float liftPelvisWeight;

	[Tooltip("The radius of the spherecast from the root that determines whether the character root is grounded.")]
	public float rootSphereCastRadius = 0.1f;

	[Tooltip("The raycasting quality. Fastest is a single raycast per foot, Simple is three raycasts, Best is one raycast and a capsule cast per foot.")]
	public Quality quality = Quality.Best;

	[NonSerialized]
	public bool Initiated;

	[field: NonSerialized]
	public Leg[] legs { get; set; }

	[field: NonSerialized]
	public Pelvis pelvis { get; set; }

	[field: NonSerialized]
	public bool isGrounded { get; set; }

	[field: NonSerialized]
	public Transform root { get; set; }

	[field: NonSerialized]
	public RaycastHit rootHit { get; set; }

	public bool rootGrounded => rootHit.distance < maxStep * 2f;

	public Vector3 up
	{
		get
		{
			if (!Boolean_0)
			{
				return Vector3.up;
			}
			return root.up;
		}
	}

	public bool Boolean_0
	{
		get
		{
			if (!rotateSolver)
			{
				return false;
			}
			if (root.up == Vector3.up)
			{
				return false;
			}
			return true;
		}
	}

	public RaycastHit GetRootHit(float maxDistanceMlp = 10f)
	{
		RaycastHit hitInfo = default(RaycastHit);
		Vector3 vector = up;
		Vector3 zero = Vector3.zero;
		Leg[] array = legs;
		foreach (Leg leg in array)
		{
			zero += leg.transform.position;
		}
		zero /= (float)legs.Length;
		hitInfo.point = zero - vector * maxStep * 10f;
		float num = maxDistanceMlp + 1f;
		hitInfo.distance = maxStep * num;
		if (maxStep <= 0f)
		{
			return hitInfo;
		}
		if (quality != Quality.Best)
		{
			Physics.Raycast(zero + vector * maxStep, -vector, out hitInfo, maxStep * num, layers);
		}
		else
		{
			Physics.SphereCast(zero + vector * maxStep, rootSphereCastRadius, -up, out hitInfo, maxStep * num, layers);
		}
		return hitInfo;
	}

	public bool IsValid(ref string errorMessage)
	{
		if (root == null)
		{
			errorMessage = "Root transform is null. Can't initiate Grounding.";
			return false;
		}
		if (legs == null)
		{
			errorMessage = "Grounding legs is null. Can't initiate Grounding.";
			return false;
		}
		if (pelvis == null)
		{
			errorMessage = "Grounding pelvis is null. Can't initiate Grounding.";
			return false;
		}
		if (legs.Length == 0)
		{
			errorMessage = "Grounding has 0 legs. Can't initiate Grounding.";
			return false;
		}
		return true;
	}

	public void Initiate(Transform root, Transform[] feet)
	{
		this.root = root;
		Initiated = false;
		rootHit = default(RaycastHit);
		if (legs == null)
		{
			legs = new Leg[feet.Length];
		}
		if (legs.Length != feet.Length)
		{
			legs = new Leg[feet.Length];
		}
		for (int i = 0; i < feet.Length; i++)
		{
			if (legs[i] == null)
			{
				legs[i] = new Leg();
			}
		}
		if (pelvis == null)
		{
			pelvis = new Pelvis();
		}
		string errorMessage = string.Empty;
		if (!IsValid(ref errorMessage))
		{
			GClass1465.Log(errorMessage, root);
		}
		else if (Application.isPlaying)
		{
			for (int j = 0; j < feet.Length; j++)
			{
				legs[j].Initiate(this, feet[j]);
			}
			pelvis.Initiate(this);
			Initiated = true;
		}
	}

	public void Update()
	{
		if (!Initiated)
		{
			return;
		}
		if ((int)layers == 0)
		{
			LogWarning("Grounding layers are set to nothing. Please add a ground layer.");
		}
		maxStep = Mathf.Clamp(maxStep, 0f, maxStep);
		footRadius = Mathf.Clamp(footRadius, 0.0001f, maxStep);
		pelvisDamper = Mathf.Clamp(pelvisDamper, 0f, 1f);
		rootSphereCastRadius = Mathf.Clamp(rootSphereCastRadius, 0.0001f, rootSphereCastRadius);
		maxFootRotationAngle = Mathf.Clamp(maxFootRotationAngle, 0f, 90f);
		prediction = Mathf.Clamp(prediction, 0f, prediction);
		footSpeed = Mathf.Clamp(footSpeed, 0f, footSpeed);
		rootHit = GetRootHit();
		float num = float.NegativeInfinity;
		float num2 = float.PositiveInfinity;
		isGrounded = false;
		Leg[] array = legs;
		foreach (Leg leg in array)
		{
			leg.Process();
			if (leg.IKOffset > num)
			{
				num = leg.IKOffset;
			}
			if (leg.IKOffset < num2)
			{
				num2 = leg.IKOffset;
			}
			if (leg.isGrounded)
			{
				isGrounded = true;
			}
		}
		pelvis.Process((0f - num) * lowerPelvisWeight, (0f - num2) * liftPelvisWeight, isGrounded);
	}

	public Vector3 GetLegsPlaneNormal()
	{
		if (!Initiated)
		{
			return Vector3.up;
		}
		Vector3 vector = up;
		Vector3 vector2 = vector;
		for (int i = 0; i < legs.Length; i++)
		{
			Vector3 vector3 = legs[i].IKPosition - root.position;
			Vector3 normal = vector;
			Vector3 tangent = vector3;
			Vector3.OrthoNormalize(ref normal, ref tangent);
			vector2 = Quaternion.FromToRotation(tangent, vector3) * vector2;
		}
		return vector2;
	}

	public void Reset()
	{
		if (Application.isPlaying)
		{
			pelvis.Reset();
			Leg[] array = legs;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Reset();
			}
		}
	}

	public void LogWarning(string message)
	{
		GClass1465.Log(message, root);
	}

	public float GetVerticalOffset(Vector3 p1, Vector3 p2)
	{
		if (Boolean_0)
		{
			return (Quaternion.Inverse(root.rotation) * (p1 - p2)).y;
		}
		return p1.y - p2.y;
	}

	public Vector3 Flatten(Vector3 v)
	{
		if (Boolean_0)
		{
			Vector3 tangent = v;
			Vector3 normal = root.up;
			Vector3.OrthoNormalize(ref normal, ref tangent);
			return Vector3.Project(v, tangent);
		}
		v.y = 0f;
		return v;
	}
}
