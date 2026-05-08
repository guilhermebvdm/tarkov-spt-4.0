using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class BouncingObject : MonoBehaviour
{
	public struct Struct74
	{
		public int index;

		public Vector3 beginPoint;

		public Vector3 startVelocity;

		public Vector3 acceleration;

		public float endTime;

		public float currentTime;

		public Vector3 currentPoint;

		public Vector3 currentVelocity;

		public Vector3 nextJumpDirection;

		public bool useBezier;

		public Vector3 bezierRaisePoint;

		public float bezierStartTime;

		public Vector3 bezierStartPoint;

		public Vector3 bezierEndPoint;

		public Collider hitCollider;

		[NonSerialized]
		public const float Float_0 = 0.3f;

		public void Init(int index, Vector3 beginPoint, Vector3 startVelocity, Vector3 acceleration)
		{
			this.index = index;
			this.beginPoint = beginPoint;
			this.startVelocity = startVelocity;
			this.acceleration = acceleration;
		}

		public void SetUseBezier(float bezierStartTime, Vector3 startPoint, Vector3 endPoint, float distance)
		{
			Vector3 vector = (endPoint + startPoint) * 0.5f;
			SetUseBezier(bezierStartTime, startPoint, vector + Vector3.up * (distance * 0.3f), endPoint);
		}

		public void SetUseBezier(float bezierStartTime, Vector3 startPoint, Vector3 bezierRaisePoint, Vector3 endPoint)
		{
			useBezier = true;
			bezierStartPoint = startPoint;
			bezierEndPoint = endPoint;
			this.bezierRaisePoint = bezierRaisePoint;
			this.bezierStartTime = bezierStartTime;
		}

		public Vector3 CalculateVelocity(float time)
		{
			return startVelocity + acceleration * time;
		}

		public Vector3 CalculatePoint(float time, Vector3 currentVelocity)
		{
			return beginPoint + currentVelocity * time + 0.5f * acceleration * (time * time);
		}

		public bool Update(ref float deltaTime)
		{
			currentTime += deltaTime;
			float num = currentTime - endTime;
			bool result;
			if (num > 0f)
			{
				currentTime = endTime;
				deltaTime = num;
				result = true;
			}
			else
			{
				deltaTime = 0f;
				result = false;
			}
			currentVelocity = CalculateVelocity(currentTime);
			if (useBezier && currentTime >= bezierStartTime)
			{
				if (endTime > 0f)
				{
					float t = currentTime / endTime;
					currentPoint = GClass1280.GetPoint(bezierStartPoint, bezierRaisePoint, bezierEndPoint, t);
				}
				else
				{
					currentPoint = bezierEndPoint;
				}
			}
			else
			{
				currentPoint = CalculatePoint(currentTime, currentVelocity);
			}
			return result;
		}
	}

	private float float_0;

	private LayerMask layerMask_0;

	private RaycastHit[] raycastHit_0 = new RaycastHit[8];

	private int int_0;

	private Struct74 struct74_0;

	private float float_1;

	private float float_2;

	private float float_3;

	private float float_4;

	private int int_1;

	private const int int_2 = 3;

	private bool bool_0;

	private Vector3 vector3_0 = Physics.gravity;

	[CompilerGenerated]
	private bool bool_1;

	private Vector3 vector3_1;

	public bool Finished
	{
		[CompilerGenerated]
		get
		{
			return bool_1;
		}
		[CompilerGenerated]
		set
		{
			bool_1 = value;
		}
	}

	public float VelocitySqrMagnitude => struct74_0.currentVelocity.sqrMagnitude;

	public void Init(Vector3 beginPoint, Vector3 velocity, float radius, float playMult, LayerMask castMask, int maxCastCount, float deltaTimeStep, float randomReboundSpread, float bounceSpeedMult, bool showDebug)
	{
		base.transform.position = beginPoint;
		layerMask_0 = castMask;
		int_0 = maxCastCount;
		float_0 = deltaTimeStep;
		float_1 = playMult;
		float_2 = randomReboundSpread;
		bool_0 = showDebug;
		float_3 = bounceSpeedMult;
		float_4 = radius;
		Finished = false;
		struct74_0 = method_0(0, beginPoint, velocity);
		int_1 = 3;
	}

	public Struct74 method_0(int index, Vector3 beginPoint, Vector3 velocity)
	{
		Struct74 result = default(Struct74);
		result.Init(index, beginPoint, velocity, vector3_0);
		float num = 0f;
		float num2 = 0f;
		while (int_0 > 0)
		{
			num2 = num;
			num += float_0;
			int_0--;
			Vector3 currentVelocity = result.CalculateVelocity(num);
			Vector3 vector = result.CalculatePoint(num, currentVelocity);
			Vector3 direction = vector - beginPoint;
			float magnitude = direction.magnitude;
			if (!method_3(beginPoint, direction, magnitude, out var hit))
			{
				beginPoint = vector;
				continue;
			}
			result.hitCollider = hit.collider;
			Vector3 vector2;
			if (magnitude > 0f)
			{
				vector2 = direction.normalized;
				num -= (1f - hit.distance / magnitude) * float_0;
			}
			else
			{
				vector2 = Vector3.down;
			}
			result.nextJumpDirection = (vector2 + 2f * hit.normal + UnityEngine.Random.insideUnitSphere * float_2).normalized;
			Vector3 endPoint = hit.point + hit.normal * float_4;
			result.SetUseBezier(num2, beginPoint, endPoint, hit.distance);
			if (bool_0)
			{
				GClass810.DebugPoint(result.bezierRaisePoint, Color.red, 0.2f, 5f);
			}
			break;
		}
		result.endTime = num;
		return result;
	}

	[Conditional("UNITY_EDITOR")]
	public void method_1(Struct74 jump, float time)
	{
	}

	public void method_2(float time)
	{
		Collider collider = null;
		int num = 100;
		while (time > 0f)
		{
			bool num2 = struct74_0.Update(ref time);
			if (bool_0)
			{
				if (vector3_1 == Vector3.zero)
				{
					vector3_1 = base.transform.position;
				}
				UnityEngine.Debug.DrawLine(vector3_1, struct74_0.currentPoint, Color.blue, 5f);
				vector3_1 = struct74_0.currentPoint;
			}
			if (num2)
			{
				if (int_0 <= 0)
				{
					OnDone();
					break;
				}
				collider = struct74_0.hitCollider;
				float num3 = struct74_0.currentVelocity.magnitude * float_3;
				if (num3 < 0f)
				{
					num3 = 0f;
				}
				Vector3 velocity = struct74_0.nextJumpDirection * num3;
				struct74_0 = method_0(struct74_0.index + 1, struct74_0.currentPoint, velocity);
			}
			num--;
			if (num <= 0)
			{
				UnityEngine.Debug.LogError("wtf?");
				OnDone();
				break;
			}
		}
		if (collider != null && int_1 >= 3)
		{
			int_1 = 0;
			OnBounce(collider);
		}
	}

	public virtual void Update()
	{
		method_2(Time.deltaTime * float_1);
		int_1++;
		base.transform.position = struct74_0.currentPoint;
	}

	public bool method_3(Vector3 point, Vector3 direction, float maxDistance, out RaycastHit hit)
	{
		hit = default(RaycastHit);
		Ray ray = new Ray(point, direction);
		if (bool_0)
		{
			UnityEngine.Debug.DrawRay(ray.origin, ray.direction * maxDistance, new Color(1f, 1f, 0f, 0.25f), 5f);
		}
		int num = Physics.RaycastNonAlloc(ray, raycastHit_0, maxDistance, layerMask_0, QueryTriggerInteraction.Ignore);
		if (num > 0)
		{
			float num2 = float.MaxValue;
			for (int i = 0; i < num; i++)
			{
				RaycastHit raycastHit = raycastHit_0[i];
				if (raycastHit.distance < num2)
				{
					hit = raycastHit;
					num2 = raycastHit.distance;
				}
			}
			return true;
		}
		return false;
	}

	public virtual void OnDone()
	{
		Finished = true;
		base.enabled = false;
		if (bool_0)
		{
			UnityEngine.Debug.Log("done");
		}
	}

	public virtual void OnBounce(Collider collider)
	{
		if (bool_0)
		{
			UnityEngine.Debug.Log("OnBounce");
		}
	}

	public void OnDrawGizmos()
	{
		if (bool_0)
		{
			Gizmos.color = Color.red;
			Gizmos.DrawSphere(base.transform.position, 0.1f);
		}
	}
}
