using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

[ExecuteInEditMode]
public class SimpleCharacterController : MonoBehaviour, ICharacterController
{
	public class GClass755
	{
		public struct GStruct36
		{
			public bool IsOverlapped;
		}

		public Collider[] colliders;

		public GStruct36[] collisionInfos;

		public int count;

		public List<Collider> seepColliders = new List<Collider>(2);

		public GClass755(int capacity)
		{
			colliders = new Collider[capacity];
			collisionInfos = new GStruct36[capacity];
		}
	}

	[SerializeField]
	private Transform _movementTransform;

	[SerializeField]
	private CapsuleCollider _mainCollider;

	[SerializeField]
	private Collider[] _colliders;

	[SerializeField]
	private LayerMask _collisionMask;

	private const int int_0 = 64;

	[Header("Settings")]
	[SerializeField]
	private float _castHalo = 0.05f;

	[SerializeField]
	private float _groundDotThreshold = 0.0871558f;

	[SerializeField]
	private float _maxDepenetrationDistance = 0.2f;

	[SerializeField]
	private float _maxDepenetrationDistanceResolve = 0.05f;

	[SerializeField]
	private float _snapGroundDenepentrationToUp = 0.64f;

	[SerializeField]
	private float _fixedDeltaDistance;

	[Header("Speed Limit")]
	[SerializeField]
	private float _speedLimit = -1f;

	[SerializeField]
	private float _sqrSpeedLimit = -1f;

	[Header("Resolve Tightness")]
	[SerializeField]
	private float _tightDotThreshold = -0.6f;

	[SerializeField]
	private int _maxIterationsToResolveTightness = 3;

	[SerializeField]
	private float _feelingTightMaxMotion = 0.2f;

	[SerializeField]
	private int _finePositionsToRemember = 8;

	[SerializeField]
	private bool _isSeepingActive = true;

	[Header("Stepping")]
	[SerializeField]
	private float _canStandUpRising = 0.005f;

	[SerializeField]
	private float _canStepUpExpanse = 0.005f;

	[SerializeField]
	private float _stepUpDumpPercent = 0.5f;

	[SerializeField]
	private float _canStandUpObstacleHeight = 0.15f;

	private const float float_0 = 0.17f;

	[Header("Huunity Depenetration Bug")]
	[SerializeField]
	private bool _resolveHuunityDepenetrationBug = true;

	[SerializeField]
	private float _depenetrationBugRayAddition = -0.001f;

	[Header("Alerts")]
	[SerializeField]
	private int _maxCycleCountToHalt = 1000;

	private Vector3 vector3_0;

	private int int_1;

	private bool bool_0;

	private bool bool_1 = true;

	private HashSet<Collider> hashSet_0 = new HashSet<Collider>();

	[CompilerGenerated]
	private bool bool_2;

	[CompilerGenerated]
	private bool bool_3;

	private GClass755[] gclass755_0;

	private RaycastHit[] raycastHit_0 = new RaycastHit[64];

	private Vector3 vector3_1;

	[CompilerGenerated]
	private Action<float> action_0;

	private bool bool_4;

	private bool bool_5;

	private float float_1;

	private float float_2;

	[Header("Other")]
	[SerializeField]
	private float _slopLimitDot;

	private float float_3;

	[SerializeField]
	private float _stepOffsetInternal;

	private Vector3 vector3_2;

	private Vector3 vector3_3;

	private Func<Collider, int> func_0;

	public bool IsSpeedLimitWasEnabledAtTheFrame
	{
		[CompilerGenerated]
		get
		{
			return bool_2;
		}
		[CompilerGenerated]
		set
		{
			bool_2 = value;
		}
	}

	public bool IsMoveIgnored
	{
		[CompilerGenerated]
		get
		{
			return bool_3;
		}
		[CompilerGenerated]
		set
		{
			bool_3 = value;
		}
	}

	public bool SupportDepenetration => true;

	public float SpeedLimit
	{
		get
		{
			return _speedLimit;
		}
		set
		{
			_speedLimit = value;
			_sqrSpeedLimit = _speedLimit * _speedLimit;
		}
	}

	public bool isEnabled
	{
		get
		{
			return _mainCollider.enabled;
		}
		set
		{
			_mainCollider.enabled = value;
		}
	}

	public Vector3 center
	{
		get
		{
			return _mainCollider.center;
		}
		set
		{
			_mainCollider.center = value;
		}
	}

	public CollisionFlags collisionFlags
	{
		get
		{
			throw new NotSupportedException();
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	public bool detectCollisions
	{
		get
		{
			throw new NotSupportedException();
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	public float height
	{
		get
		{
			return _mainCollider.height;
		}
		set
		{
			_mainCollider.height = value;
			if (action_0 != null)
			{
				action_0(value);
			}
		}
	}

	public bool isGrounded
	{
		get
		{
			return bool_4;
		}
		set
		{
			bool_4 = value;
		}
	}

	public float skinWidth
	{
		get
		{
			return float_1;
		}
		set
		{
			float_1 = value;
		}
	}

	public float radius
	{
		get
		{
			return _mainCollider.radius;
		}
		set
		{
			_mainCollider.radius = value;
		}
	}

	public float slopeLimit
	{
		get
		{
			return float_2;
		}
		set
		{
			float_2 = value;
			_slopLimitDot = Mathf.Cos(float_2 * (MathF.PI / 180f));
		}
	}

	public float stepOffset
	{
		get
		{
			return float_3;
		}
		set
		{
			float_3 = value;
			_stepOffsetInternal = float_3 + 0.17f;
		}
	}

	public Vector3 velocity
	{
		get
		{
			return vector3_2;
		}
		set
		{
			vector3_2 = value;
		}
	}

	public Rigidbody attachedRigidbody
	{
		get
		{
			throw new NotSupportedException();
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	public Bounds bounds
	{
		get
		{
			return _mainCollider.bounds;
		}
		set
		{
		}
	}

	public float contactOffset
	{
		get
		{
			return _mainCollider.contactOffset;
		}
		set
		{
			_mainCollider.contactOffset = value;
		}
	}

	public Vector3 surfaceNormalAccordingToCapsule
	{
		get
		{
			return vector3_3;
		}
		set
		{
			vector3_3 = value;
		}
	}

	public bool Boolean_0
	{
		get
		{
			if (_isSeepingActive)
			{
				return func_0 != null;
			}
			return false;
		}
	}

	public bool ShouldStickToGround => true;

	public event Action<float> OnHeightChanged
	{
		[CompilerGenerated]
		add
		{
			Action<float> action = action_0;
			Action<float> action2;
			do
			{
				action2 = action;
				Action<float> value2 = (Action<float>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<float> action = action_0;
			Action<float> action2;
			do
			{
				action2 = action;
				Action<float> value2 = (Action<float>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public void SetSteerDirection(Vector3 steerDirection)
	{
	}

	public void Init(Transform movementTransform, Collider[] colliders, CapsuleCollider mainCollider, float fixedDeltaDistance, LayerMask collisionMask, float stepOffset, float slopeLimit)
	{
		_movementTransform = movementTransform;
		_mainCollider = mainCollider;
		_fixedDeltaDistance = fixedDeltaDistance;
		_collisionMask = collisionMask;
		this.stepOffset = stepOffset;
		this.slopeLimit = slopeLimit;
		method_0(colliders);
	}

	public void RegisterCanSeepThroughDelegate(Func<Collider, int> canSeepThroughDelegate)
	{
		func_0 = canSeepThroughDelegate;
	}

	public void AddNoSpeedLimitCollisionColliders(IEnumerable<Collider> colliders)
	{
		foreach (Collider collider in colliders)
		{
			hashSet_0.Add(collider);
		}
	}

	public void RemoveNoSpeedLimitCollisionColliders(IEnumerable<Collider> colliders)
	{
		foreach (Collider collider in colliders)
		{
			hashSet_0.Remove(collider);
		}
	}

	public void method_0(Collider[] colliders)
	{
		_colliders = colliders;
		gclass755_0 = new GClass755[_colliders.Length];
		for (int i = 0; i < _colliders.Length; i++)
		{
			gclass755_0[i] = new GClass755(64);
		}
	}

	public CollisionFlags Move(Vector3 motion, float deltaTime)
	{
		if (IsMoveIgnored)
		{
			return CollisionFlags.None;
		}
		float magnitude = motion.magnitude;
		if (magnitude > 0f)
		{
			Vector3 vector = (vector3_1 = _movementTransform.position);
			int num = (int)(magnitude / _fixedDeltaDistance);
			float num2 = magnitude % _fixedDeltaDistance;
			if (num > 0)
			{
				if (num >= _maxCycleCountToHalt)
				{
					return CollisionFlags.None;
				}
				Vector3 motion2 = _fixedDeltaDistance / magnitude * motion;
				for (int i = 0; i < num; i++)
				{
					method_4(motion2);
				}
			}
			Vector3 motion3 = num2 / magnitude * motion;
			method_4(motion3);
			if (_resolveHuunityDepenetrationBug)
			{
				vector3_1 = method_18(vector3_1);
			}
			method_13(vector, vector3_1, deltaTime);
			method_1(vector, deltaTime);
			if (vector != vector3_1)
			{
				_movementTransform.position = vector3_1;
			}
			bool flag = Physics.Raycast(vector3_1 + Vector3.up * 0.05f, Vector3.down, 0.2f, _collisionMask, QueryTriggerInteraction.Ignore);
			bool_5 = isGrounded || flag;
		}
		return CollisionFlags.None;
	}

	public void method_1(Vector3 startPosition, float deltaTime)
	{
		IsSpeedLimitWasEnabledAtTheFrame = false;
		if (_speedLimit < 0f || method_2())
		{
			return;
		}
		Vector3 rhs = vector3_2;
		if (rhs.y < 0f)
		{
			rhs.y = 0f;
		}
		float sqrMagnitude = rhs.sqrMagnitude;
		if (sqrMagnitude > _sqrSpeedLimit)
		{
			Vector3 normalized = vector3_2.normalized;
			float num2;
			if (vector3_2.y < 0f)
			{
				float num = Vector3.Dot(normalized, rhs) / Mathf.Sqrt(sqrMagnitude);
				num2 = _speedLimit / num;
			}
			else
			{
				num2 = _speedLimit;
			}
			vector3_2 = normalized * num2;
			vector3_1 = startPosition + vector3_2 * deltaTime;
		}
		IsSpeedLimitWasEnabledAtTheFrame = true;
	}

	public bool method_2()
	{
		for (int i = 0; i < gclass755_0.Length; i++)
		{
			GClass755 gClass = gclass755_0[i];
			for (int j = 0; j < gClass.count; j++)
			{
				Collider item = gClass.colliders[j];
				if (gClass.collisionInfos[j].IsOverlapped && hashSet_0.Contains(item))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool method_3(LayerMask layerMask)
	{
		for (int i = 0; i < gclass755_0.Length; i++)
		{
			GClass755 gClass = gclass755_0[i];
			for (int j = 0; j < gClass.count; j++)
			{
				Collider collider = gClass.colliders[j];
				if (gClass.collisionInfos[j].IsOverlapped && ((1 << collider.gameObject.layer) & (int)layerMask) != 0)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void method_4(Vector3 motion)
	{
		Vector3 resolvedPoint = vector3_1 + motion;
		isGrounded = false;
		bool feelingTightMain = false;
		bool flag = false;
		for (int i = 0; i < 1 + _maxIterationsToResolveTightness; i++)
		{
			bool updateNeighbours = i == 0;
			method_5(resolvedPoint, updateNeighbours, !feelingTightMain, out resolvedPoint, out feelingTightMain);
			if (!feelingTightMain)
			{
				break;
			}
			if (!flag && Boolean_0)
			{
				method_10();
				flag = true;
			}
		}
		bool flag2 = true;
		Vector3 vector = resolvedPoint - vector3_1;
		float magnitude = vector.magnitude;
		if (magnitude > 0f)
		{
			if (magnitude >= _maxDepenetrationDistance)
			{
				resolvedPoint = vector3_1 + vector * (_maxDepenetrationDistanceResolve / magnitude);
				flag2 = false;
			}
			else if (feelingTightMain)
			{
				float num = magnitude / _feelingTightMaxMotion;
				if (num > 1f)
				{
					num = 1f;
				}
				num = 1f - num;
				if (num > 0f)
				{
					resolvedPoint = vector3_1 + vector * num;
				}
				else if (bool_0)
				{
					resolvedPoint = vector3_0;
				}
				flag2 = false;
			}
		}
		if (flag2)
		{
			int_1++;
			if (int_1 >= _finePositionsToRemember)
			{
				vector3_0 = resolvedPoint;
				int_1 = 0;
				bool_0 = true;
			}
		}
		else
		{
			int_1 = 0;
		}
		vector3_1 = resolvedPoint;
	}

	public int method_5(Vector3 desiredPoint, bool updateNeighbours, bool doSnapToGround, out Vector3 resolvedPoint, out bool feelingTightMain)
	{
		int num = 0;
		feelingTightMain = false;
		resolvedPoint = desiredPoint;
		for (int i = 0; i < _colliders.Length; i++)
		{
			Collider collider = _colliders[i];
			Vector3 desiredPoint2 = resolvedPoint + (collider.transform.position - _movementTransform.position);
			float snapGroundDenepentrationToUp = 0f;
			if (doSnapToGround && collider == _mainCollider)
			{
				snapGroundDenepentrationToUp = _snapGroundDenepentrationToUp;
			}
			GClass755 neighboursColliders = gclass755_0[i];
			Vector3 surfaceNormal;
			bool feelingTight;
			int num2 = method_6(collider, neighboursColliders, desiredPoint2, snapGroundDenepentrationToUp, updateNeighbours, out resolvedPoint, out surfaceNormal, out feelingTight);
			num += num2;
			if (collider == _mainCollider)
			{
				vector3_3 = surfaceNormal;
				if (feelingTight)
				{
					feelingTightMain = true;
				}
			}
		}
		if (num > 0)
		{
			vector3_3 = vector3_3.normalized;
		}
		else
		{
			vector3_3 = Vector3.up;
		}
		return num;
	}

	public int method_6(Collider collider, GClass755 neighboursColliders, Vector3 desiredPoint, float snapGroundDenepentrationToUp, bool updateNeighbours, out Vector3 resolvedPoint, out Vector3 surfaceNormal, out bool feelingTight)
	{
		int num = 0;
		Vector3 zero = Vector3.zero;
		feelingTight = false;
		resolvedPoint = desiredPoint;
		if (updateNeighbours)
		{
			method_7(collider, neighboursColliders, desiredPoint);
			method_9(neighboursColliders);
		}
		surfaceNormal = Vector3.zero;
		for (int i = 0; i < neighboursColliders.count; i++)
		{
			Collider collider2 = neighboursColliders.colliders[i];
			if (method_8(collider2))
			{
				continue;
			}
			Vector3 direction;
			float distance;
			bool flag = Physics.ComputePenetration(collider, resolvedPoint, collider.transform.rotation, collider2, collider2.transform.position, collider2.transform.rotation, out direction, out distance);
			neighboursColliders.collisionInfos[i] = new GClass755.GStruct36
			{
				IsOverlapped = flag
			};
			bool flag2 = neighboursColliders.seepColliders.Contains(collider2);
			if (flag)
			{
				if (flag2)
				{
					continue;
				}
				float num2 = Vector3.Dot(direction, Vector3.up);
				if (num2 >= _groundDotThreshold)
				{
					if (direction.y > surfaceNormal.y)
					{
						surfaceNormal = direction;
					}
					if (collider2.gameObject.layer == LayerMaskClass.PlayerLayer)
					{
						distance *= EFTHardSettings.Instance.HumanPyramidExtraDepenetration;
					}
					else if (snapGroundDenepentrationToUp > 0f && num2 >= snapGroundDenepentrationToUp)
					{
						direction = Vector3.up;
						if (num2 > 0f)
						{
							distance /= num2;
						}
					}
					isGrounded = true;
				}
				if (num > 0 && Vector3.Dot(zero.normalized, direction) < _tightDotThreshold)
				{
					feelingTight = true;
				}
				num++;
				Vector3 vector = distance * direction;
				zero += direction;
				if (!bool_1)
				{
					vector.y = 0f;
				}
				resolvedPoint += vector;
			}
			else if (flag2)
			{
				neighboursColliders.seepColliders.Remove(collider2);
			}
		}
		return num;
	}

	public void method_7(Collider collider, GClass755 neighboursColliders, Vector3 desiredPoint)
	{
		Vector3 position = desiredPoint + (collider.bounds.center - collider.transform.position);
		float num = Mathf.Max(Mathf.Max(collider.bounds.extents.x, collider.bounds.extents.y), collider.bounds.extents.z) + _castHalo;
		neighboursColliders.count = Physics.OverlapSphereNonAlloc(position, num, neighboursColliders.colliders, _collisionMask, QueryTriggerInteraction.Ignore);
	}

	public bool method_8(Collider collider)
	{
		int num = 0;
		while (true)
		{
			if (num < _colliders.Length)
			{
				Collider collider2 = _colliders[num];
				if (collider2 == collider || EFTPhysicsClass.GetIgnoreCollision(collider2, collider))
				{
					break;
				}
				num++;
				continue;
			}
			return false;
		}
		return true;
	}

	public void method_9(GClass755 neighboursColliders)
	{
		for (int num = neighboursColliders.seepColliders.Count - 1; num >= 0; num--)
		{
			Collider collider = neighboursColliders.seepColliders[num];
			bool flag = false;
			for (int i = 0; i < neighboursColliders.count; i++)
			{
				Collider collider2 = neighboursColliders.colliders[i];
				if (collider == collider2)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				neighboursColliders.seepColliders.RemoveAt(num);
			}
		}
	}

	public void method_10()
	{
		for (int i = 0; i < gclass755_0.Length; i++)
		{
			GClass755 collidersArray = gclass755_0[i];
			method_11(collidersArray);
		}
	}

	public void method_11(GClass755 collidersArray)
	{
		int num = int.MaxValue;
		bool flag = false;
		for (int i = 0; i < collidersArray.count; i++)
		{
			Collider collider = collidersArray.colliders[i];
			if (method_8(collider))
			{
				continue;
			}
			int num2 = method_12(collider);
			if (num2 >= 0 && num2 <= num)
			{
				if (!collidersArray.seepColliders.Contains(collider))
				{
					collidersArray.seepColliders.Add(collider);
				}
				if (num2 < num)
				{
					flag = true;
				}
				num = num2;
			}
		}
		if (!flag)
		{
			return;
		}
		for (int num3 = collidersArray.seepColliders.Count - 1; num3 >= 0; num3--)
		{
			Collider collider2 = collidersArray.seepColliders[num3];
			if (method_12(collider2) > num)
			{
				collidersArray.seepColliders.RemoveAt(num3);
			}
		}
	}

	public int method_12(Collider collider)
	{
		return func_0(collider);
	}

	public void method_13(Vector3 currentPoint, Vector3 destinationPoint, float deltaTime)
	{
		vector3_2 = (destinationPoint - currentPoint) / deltaTime;
	}

	public Vector3 method_14(Vector3 startPoint, Vector3 motion, Vector3 desiredPoint)
	{
		Vector3 result = desiredPoint;
		Vector3 vector = new Vector3(motion.x, 0f, motion.z);
		Vector3 normalized = vector.normalized;
		float magnitude = vector.magnitude;
		if (magnitude > 0f && method_15(startPoint, normalized, magnitude, out var bestHit))
		{
			float num = 1f - Vector3.Dot(-normalized, bestHit.normal);
			if (_slopLimitDot <= num && num <= 1f)
			{
				float num2 = bestHit.point.y - result.y;
				if (num2 > 0f && method_17(bestHit.point))
				{
					result.y += num2 * _stepUpDumpPercent;
				}
			}
		}
		return result;
	}

	public bool method_15(Vector3 startPoint, Vector3 forwardDirection, float forwardDistance, out RaycastHit bestHit)
	{
		float num = radius / 2f;
		bool flag = false;
		if (!bool_5)
		{
			flag = true;
		}
		else if (Physics.Raycast(new Ray(startPoint + Vector3.up * (height * _canStandUpObstacleHeight), forwardDirection), radius * 1.1f, _collisionMask, QueryTriggerInteraction.Ignore))
		{
			flag = true;
		}
		bool flag2 = false;
		if (flag)
		{
			Vector3 vector = startPoint + forwardDirection * (forwardDistance + _canStepUpExpanse + radius) + Vector3.up * _stepOffsetInternal;
			Ray ray = new Ray(vector, Vector3.down);
			if ((flag2 = method_16(ray, raycastHit_0, _stepOffsetInternal, _collisionMask, out bestHit)) && !bool_5)
			{
				float num2 = radius * 0.5f;
				Ray ray2 = new Ray(vector + Vector3.up * num2, forwardDirection.normalized * num2);
				RaycastHit[] results = new RaycastHit[5];
				flag2 = !method_16(ray2, results, num2, _collisionMask, out var _);
			}
		}
		else
		{
			Vector3 vector2 = startPoint + forwardDirection * forwardDistance + Vector3.up * (num + _canStandUpRising + _stepOffsetInternal);
			flag2 = Physics.CapsuleCast(vector2, vector2, num, Vector3.down, out bestHit, _stepOffsetInternal, _collisionMask, QueryTriggerInteraction.Ignore);
		}
		return flag2;
	}

	public bool method_16(Ray ray, RaycastHit[] results, float maxDistance, int layerMask, out RaycastHit bestHit)
	{
		int num = Physics.RaycastNonAlloc(ray, results, maxDistance, layerMask, QueryTriggerInteraction.Ignore);
		float num2 = float.MaxValue;
		bestHit = default(RaycastHit);
		bool result = false;
		for (int i = 0; i < num; i++)
		{
			RaycastHit raycastHit = results[i];
			if (!method_8(raycastHit.collider) && raycastHit.distance < num2)
			{
				num2 = raycastHit.distance;
				bestHit = raycastHit;
				result = true;
			}
		}
		return result;
	}

	public bool method_17(Vector3 point)
	{
		int count = Physics.RaycastNonAlloc(new Ray(point + Vector3.up * _canStandUpRising, Vector3.up), raycastHit_0, height, _collisionMask, QueryTriggerInteraction.Ignore);
		return !method_19(count, raycastHit_0);
	}

	public Vector3 method_18(Vector3 point)
	{
		Vector3 result = point;
		float num = radius;
		Vector3 vector = _mainCollider.bounds.center;
		vector.y = vector.y - height / 2f + radius;
		Ray ray = new Ray(vector, Vector3.down);
		RaycastHit bestHit;
		bool num2 = method_16(ray, raycastHit_0, num + _depenetrationBugRayAddition, _collisionMask, out bestHit);
		UnityEngine.Debug.DrawRay(vector, Vector3.down * (num + _depenetrationBugRayAddition), Color.blue);
		if (num2)
		{
			result = bestHit.point;
		}
		return result;
	}

	public bool method_19(int count, RaycastHit[] colliders)
	{
		bool result = false;
		for (int i = 0; i < count; i++)
		{
			RaycastHit raycastHit = colliders[i];
			if (!method_8(raycastHit.collider))
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public bool method_20(int count, Collider[] colliders)
	{
		bool result = false;
		for (int i = 0; i < count; i++)
		{
			Collider collider = colliders[i];
			if (!method_8(collider))
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public void SetEnableVerticalDepenetration(bool enable)
	{
		bool_1 = enable;
	}

	public bool SimpleMove(Vector3 motion)
	{
		throw new NotImplementedException();
	}

	public void CopyFields(ICharacterController characterController)
	{
		GClass779.CopyFields(this, characterController);
	}

	public void CopyFields(CharacterStruct footprint)
	{
		GClass779.CopyFields(this, footprint);
	}

	public CharacterStruct GetFootprint()
	{
		return GClass779.GetFootprint(this);
	}

	public Collider GetCollider()
	{
		return _mainCollider;
	}

	[Conditional("UNITY_EDITOR")]
	public void method_21(string message, params object[] args)
	{
	}

	[Conditional("UNITY_EDITOR")]
	public void method_22<T>(string name, T msg)
	{
	}

	public void SeTightDotThreshold(float degrees)
	{
		_tightDotThreshold = Mathf.Cos(degrees * (MathF.PI / 180f));
	}

	public void SetSnapGroundDenepentrationToUpInDegrees(float degrees)
	{
		_snapGroundDenepentrationToUp = Mathf.Cos(degrees * (MathF.PI / 180f));
	}

	public void SeGroundDotThreshold(float degrees)
	{
		_groundDotThreshold = Mathf.Cos(degrees * (MathF.PI / 180f));
	}

	public void SeSlotLimitDot(float degrees)
	{
		slopeLimit = degrees;
	}

	public void OnDrawGizmos()
	{
		Color green = Color.green;
		for (int i = 0; i < _colliders.Length; i++)
		{
			Collider collider = _colliders[i];
			Matrix4x4 matrix4x = Matrix4x4.TRS(collider.transform.position, collider.transform.rotation, collider.transform.lossyScale);
			Matrix4x4 matrix = Gizmos.matrix;
			Gizmos.matrix *= matrix4x;
			CapsuleCollider capsuleCollider = collider as CapsuleCollider;
			SphereCollider sphereCollider = collider as SphereCollider;
			if (capsuleCollider != null)
			{
				Vector3 vector = new Vector3(0f, capsuleCollider.height / 2f, 0f);
				GClass810.DrawCapsule(capsuleCollider.center - vector, capsuleCollider.center + vector, green, capsuleCollider.radius);
			}
			else if (sphereCollider != null)
			{
				Gizmos.color = green;
				Gizmos.DrawWireSphere(sphereCollider.center, sphereCollider.radius);
			}
			else
			{
				Gizmos.color = green;
				Gizmos.DrawWireCube(collider.transform.InverseTransformPoint(collider.bounds.center), collider.bounds.size);
			}
			Gizmos.matrix = matrix;
		}
	}
}
