using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using BezierSplineTools;
using EFT;
using EFT.GlobalEvents;
using EFT.Vehicle;
using UnityEngine;

public class VehicleBase : MonoBehaviour, GInterface25
{
	[CompilerGenerated]
	private EBtrState ebtrState_0;

	[CompilerGenerated]
	private EVehicleRouteState evehicleRouteState_0;

	[CompilerGenerated]
	private bool bool_0;

	public BTRBodySpring bodySpring = new BTRBodySpring();

	public float currentSpeed;

	public float previousCurrentSpeed;

	private float float_0;

	private float float_1 = 15f;

	private int int_0;

	public float realAcceleration;

	private float float_2;

	private float float_3 = 15f;

	private int int_1;

	public float timeToEndPause;

	public MapPathConfig MapPathConfig;

	[SerializeField]
	public PathConfig _currentPathConfig;

	[CompilerGenerated]
	private MoveByPathType moveByPathType_0;

	private Dictionary<string, PathPartBase> dictionary_0 = new Dictionary<string, PathPartBase>();

	public GameObject turnCheckerObject;

	public GameObject surfaceCheckerCenter;

	public GameObject SurfaceCheckerFront;

	public GameObject SurfaceCheckerBack;

	public VehicleSuspension suspension;

	public bool lookForward;

	[CompilerGenerated]
	private EVehicleMoveDirection evehicleMoveDirection_0;

	public float moveSpeed;

	public float readyToDeparture = 10f;

	public float checkTurnDistanceTime = 10f;

	public float turnCheckSensitivity = 0.98f;

	public float decreaseSpeedOnTurnLimit = 0.5f;

	public float endSplineDecelerationDistance = 2f;

	public float accelerationSpeed = 0.1f;

	public float decelerationSpeed = 0.1f;

	public float horizontalRotateLerp = 0.5f;

	public float verticalRotateLerp = 0.2f;

	public float snappingThresholdDistance = 2f;

	private PathSpline pathSpline_0;

	private PathSpline pathSpline_1;

	private PathDestination pathDestination_0;

	private PathDestination pathDestination_1;

	private PathReverseData pathReverseData_0;

	private PathReversePart pathReversePart_0;

	private bool bool_1;

	private bool bool_2 = true;

	private bool bool_3;

	private bool bool_4;

	private bool bool_5 = true;

	private bool bool_6 = true;

	private bool bool_7;

	private float float_4;

	private float float_5;

	private float float_6;

	private float float_7;

	private float float_8;

	private float float_9;

	private float float_10;

	private float float_11;

	private int int_2;

	private int int_3;

	private int int_4 = 4;

	private float float_12 = 1f;

	private float float_13;

	private Vector3 vector3_0;

	private Vector3 vector3_1;

	private Vector3 vector3_2;

	private bool bool_8;

	public Vector2 pauseDurationRange = new Vector2(20f, 25f);

	private bool bool_9;

	private bool bool_10;

	private float float_14;

	private float float_15;

	public float sendSpawnEventTime = 15f;

	public GDelegate32 IncomingToDestinationEvent;

	public GDelegate31 ReadyToDepartureEvent;

	public EBtrState BtrState
	{
		[CompilerGenerated]
		get
		{
			return ebtrState_0;
		}
		[CompilerGenerated]
		set
		{
			ebtrState_0 = value;
		}
	}

	public EVehicleRouteState VehicleRouteState
	{
		[CompilerGenerated]
		get
		{
			return evehicleRouteState_0;
		}
		[CompilerGenerated]
		set
		{
			evehicleRouteState_0 = value;
		}
	}

	public bool IsPaid
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

	public PathConfig CurrentPathConfig
	{
		get
		{
			return _currentPathConfig;
		}
		set
		{
			_currentPathConfig = value;
		}
	}

	public MoveByPathType MoveByPathType
	{
		[CompilerGenerated]
		get
		{
			return moveByPathType_0;
		}
		[CompilerGenerated]
		set
		{
			moveByPathType_0 = value;
		}
	}

	public EVehicleMoveDirection VehicleMoveDirection
	{
		[CompilerGenerated]
		get
		{
			return evehicleMoveDirection_0;
		}
		[CompilerGenerated]
		set
		{
			evehicleMoveDirection_0 = value;
		}
	}

	public virtual void Start()
	{
		VehicleRouteState = EVehicleRouteState.OnDepot;
		base.gameObject.SetActive(value: false);
	}

	public virtual void Update()
	{
	}

	public void FixedUpdate()
	{
		float_10 = moveSpeed * Time.fixedDeltaTime;
		previousCurrentSpeed = currentSpeed;
		PauseProcess();
		MoveProcess();
		method_24(Time.fixedDeltaTime);
		if (bool_8)
		{
			method_20();
		}
		vector3_0 = base.transform.position;
	}

	public virtual bool IsDoorsClosed()
	{
		return false;
	}

	public virtual void HitFromPlayer(Player player)
	{
	}

	public void Initialization(MapPathConfig mapPathsConfig)
	{
		MapPathConfig = mapPathsConfig;
		float_7 = 0.1f;
		float_6 = 0.1f;
		dictionary_0 = MapPathConfig.GetPathParts(_currentPathConfig.GetAllPathPoints());
		turnCheckerObject.transform.parent = null;
	}

	public void SpawnInEnterPoint()
	{
		PathPartBase pathPartBase = dictionary_0[_currentPathConfig.enterPoint];
		base.transform.position = pathPartBase.transform.position;
		base.transform.rotation = pathPartBase.transform.rotation;
		pathDestination_1 = pathPartBase as PathDestination;
		pathDestination_0 = pathDestination_1;
		float_7 = 0.1f;
		GlobalEventHandlerClass.CreateEvent<BtrSpawnOnThePathEvent>().Invoke(pathPartBase.transform.position, pathPartBase.transform.rotation, string.Empty);
	}

	public void SpawnInPoint(int index)
	{
		string key = _currentPathConfig.pathPoints[index];
		PathPartBase pathPartBase = dictionary_0[key];
		base.transform.position = pathPartBase.transform.position;
		pathDestination_1 = pathPartBase as PathDestination;
		pathDestination_0 = pathDestination_1;
	}

	public void MoveEnable()
	{
		Reset();
		base.gameObject.SetActive(value: true);
		SpawnInEnterPoint();
		bool_1 = true;
		method_12();
		VehicleRouteState = EVehicleRouteState.OnRoute;
		StartCoroutine(method_0(state: true));
	}

	public void MoveDisable()
	{
		Reset();
		VehicleRouteState = EVehicleRouteState.OnDepot;
	}

	public void MoveToDestination(string destinationID)
	{
		PathSpline pathSplineByPoints = MapPathConfig.GetPathSplineByPoints(pathDestination_1.id, destinationID);
		method_2();
		method_17(pathSplineByPoints);
		BtrState = (IsPaid ? EBtrState.StopPaid : EBtrState.Stop);
		PathDestination pathDestination = MapPathConfig.GetPartPathByID(destinationID) as PathDestination;
		if ((bool)pathDestination)
		{
			pathDestination_0 = pathDestination;
		}
		bool_3 = true;
		bool_5 = false;
		method_4();
	}

	public IEnumerator method_0(bool state)
	{
		yield return new WaitForSeconds(1f);
		bool_8 = state;
	}

	public void MovePauseOn(float pauseDuration)
	{
		if (pauseDuration != 0f)
		{
			float_15 = 0f;
			float_14 = pauseDuration;
			bool_9 = true;
			timeToEndPause = float_14 - float_15;
			BtrState = (IsPaid ? EBtrState.StopPaid : EBtrState.Stop);
			GlobalEventHandlerClass.CreateEvent<BtrPauseMoveEvent>().Invoke(bool_9);
		}
		else
		{
			MovePauseOff();
		}
		method_22();
	}

	public void MovePauseOff()
	{
		if (!IsPaid)
		{
			ExtractPassengers();
		}
		BtrState = EBtrState.Running;
		float_15 = 0f;
		float_14 = 0f;
		bool_9 = false;
		bool_4 = false;
		bool_6 = true;
		if (bool_5)
		{
			method_12();
		}
		CheckNeedReverse(pathDestination_1, pathSpline_0);
		GlobalEventHandlerClass.CreateEvent<BtrPauseMoveEvent>().Invoke(bool_9);
	}

	public void method_1()
	{
		method_17(pathSpline_1);
		bool_3 = true;
	}

	public virtual void ExtractPassengers()
	{
	}

	public void MoveProcess()
	{
		if (!bool_9 && bool_3 && pathSpline_0 != null)
		{
			method_3();
		}
		else
		{
			currentSpeed = 0f;
		}
	}

	public void PauseProcess()
	{
		if (bool_9)
		{
			float_15 += Time.fixedDeltaTime;
			timeToEndPause = float_14 - float_15;
			if (timeToEndPause < readyToDeparture && !bool_10)
			{
				bool_10 = true;
				GlobalEventHandlerClass.CreateEvent<BtrReadyToDepartureEvent>().Invoke();
			}
			else if (timeToEndPause > readyToDeparture)
			{
				bool_10 = false;
			}
			if (!(float_15 <= float_14))
			{
				timeToEndPause = 0f;
				MovePauseOff();
			}
		}
	}

	public void Reset()
	{
		BtrState = EBtrState.Stop;
		bool_5 = true;
		pathDestination_1 = null;
		pathSpline_0 = null;
		pathDestination_0 = null;
		bool_3 = false;
		bool_1 = false;
		bool_9 = false;
		bool_8 = false;
		float_4 = 0f;
		int_2 = 0;
		float_14 = 0f;
		float_15 = 0f;
		float_7 = 0.1f;
		float_6 = 0.1f;
		float_10 = moveSpeed / (float)Application.targetFrameRate;
		method_4();
		base.transform.position = MapPathConfig.DepotPosition;
		base.gameObject.SetActive(value: false);
	}

	public void method_2()
	{
		float num = float_14 - float_15;
		float_15 += num - readyToDeparture;
		timeToEndPause = float_14 - float_15;
	}

	public void method_3()
	{
		VehicleMoveDirection = (method_21() ? EVehicleMoveDirection.Backward : EVehicleMoveDirection.Forward);
		if (float_13 < float_12)
		{
			float_13 += Time.fixedDeltaTime;
		}
		else
		{
			if (!IsDoorsClosed())
			{
				return;
			}
			PathSpline pathSpline = pathSpline_0;
			float num = method_11();
			float num2 = method_10();
			bool flag = num < turnCheckSensitivity;
			bool flag2 = num2 < endSplineDecelerationDistance && bool_6;
			if (flag && !flag2)
			{
				if (float_7 >= decreaseSpeedOnTurnLimit)
				{
					float_7 -= decelerationSpeed * Time.fixedDeltaTime;
					float_7 = Mathf.Clamp(float_7, decreaseSpeedOnTurnLimit, 1f);
					float_8 = 0f;
				}
			}
			else if ((flag2 && !flag) || (flag2 && flag))
			{
				Vector3 b = method_9();
				b.y = base.transform.position.y;
				if (float_8 == 0f)
				{
					float_8 = Vector3.Distance(base.transform.position, b);
				}
				float num3 = Vector3.Distance(base.transform.position, b);
				float num4 = float_8 / 100f;
				float value = num3 / num4 / 100f;
				float_7 = Mathf.Clamp(value, 0.05f, float_7);
			}
			else if (!flag2 && !flag)
			{
				float_7 += accelerationSpeed * Time.fixedDeltaTime;
				float_7 = Mathf.Clamp(float_7, 0.05f, 1f);
				float_8 = 0f;
			}
			Vector3 position = base.transform.position;
			Vector3 vector = default(Vector3);
			float num5 = (bool_2 ? 1f : (-1f));
			float num6 = float_4;
			int num7 = 0;
			while (true)
			{
				if (num7 < int_4)
				{
					num6 = float_4 + float_6 * float_7 * Time.fixedDeltaTime * num5;
					vector = pathSpline.spline.GetPointWithCurvesLengthCache(num6);
					vector.y = base.transform.position.y;
					position.y = base.transform.position.y;
					float_11 = Vector3.Distance(vector, position);
					if (float_11 < float_10 * float_7)
					{
						float_6 *= 1.1f;
					}
					else
					{
						float_6 *= 0.9f;
					}
					if (bool_2 ? (num6 > 1f) : (num6 < 0f))
					{
						break;
					}
					num7++;
					continue;
				}
				vector.y = base.transform.position.y;
				float num8 = Vector3.Distance(base.transform.position, vector) / Time.fixedDeltaTime;
				if (bool_7)
				{
					float num9 = currentSpeed / num8;
					float_6 *= num9;
					for (int i = 0; i < int_4; i++)
					{
						num6 = float_4 + float_6 * float_7 * Time.fixedDeltaTime * num5;
						vector = pathSpline.spline.GetPointWithCurvesLengthCache(num6);
						vector.y = base.transform.position.y;
						position.y = base.transform.position.y;
						float_11 = Vector3.Distance(vector, position);
						if (float_11 < float_10 * float_7)
						{
							float_6 *= 1.1f;
						}
						else
						{
							float_6 *= 0.9f;
						}
					}
					vector = pathSpline.spline.GetPointWithCurvesLengthCache(num6);
					vector.y = base.transform.position.y;
					num8 = Vector3.Distance(base.transform.position, vector) / Time.fixedDeltaTime;
				}
				currentSpeed = method_5(num8);
				base.transform.position = vector;
				if (lookForward)
				{
					Quaternion rotation = base.transform.rotation;
					Vector3 vector2 = default(Vector3);
					vector2 = ((!method_21()) ? (vector + (vector - position).normalized) : (vector - (vector - position).normalized));
					base.transform.LookAt(vector2);
					float num10 = Quaternion.Angle(rotation, base.transform.rotation);
					if (num10 < 90f && num10 != 0f)
					{
						Quaternion rotation2 = base.transform.rotation;
						Quaternion rotation3 = Quaternion.Lerp(rotation, rotation2, horizontalRotateLerp);
						base.transform.rotation = rotation3;
					}
					else
					{
						base.transform.rotation = rotation;
					}
				}
				float_4 = num6;
				bool_7 = false;
				return;
			}
			method_6();
		}
	}

	public void method_4()
	{
		float_13 = 0f;
	}

	public float method_5(float newSpeed)
	{
		int_0++;
		if ((float)int_0 == float_1)
		{
			method_22();
			int_0 = 1;
		}
		float_0 += newSpeed;
		return float_0 / (float)int_0;
	}

	public void method_6()
	{
		float_4 = 0f;
		bool_2 = true;
		bool_3 = false;
		if (bool_6)
		{
			float_7 = 0.1f;
			float_6 = 0.1f;
		}
		if (!bool_4)
		{
			method_13(pathDestination_0);
		}
		else
		{
			method_7();
		}
	}

	public void method_7()
	{
		int_3++;
		if (int_3 < pathReverseData_0.reverseParts.Count)
		{
			if (int_3 == pathReverseData_0.reverseParts.Count - 1)
			{
				bool_6 = false;
			}
			pathReversePart_0 = pathReverseData_0.reverseParts[int_3];
			method_17(pathReversePart_0.spline);
			bool_3 = true;
		}
		else
		{
			method_8();
			method_3();
		}
	}

	public void method_8()
	{
		method_1();
		bool_4 = false;
		bool_7 = true;
		int_3 = 0;
		bool_6 = true;
	}

	public Vector3 method_9()
	{
		Vector3 position = base.transform.position;
		if (bool_2)
		{
			return pathSpline_0.spline.GetPointWithCurvesLengthCache(1f);
		}
		return pathSpline_0.spline.GetPointWithCurvesLengthCache(0f);
	}

	public float method_10()
	{
		Vector3 position = base.transform.position;
		position = ((!bool_2) ? pathSpline_0.spline.GetPointWithCurvesLengthCache(0f) : pathSpline_0.spline.GetPointWithCurvesLengthCache(1f));
		position.y = base.transform.position.y;
		return Vector3.Distance(base.transform.position, position);
	}

	public float method_11()
	{
		float num = float_4;
		num = ((!bool_2) ? (float_4 - checkTurnDistanceTime * float_6 * float_7 * Time.fixedDeltaTime) : (float_4 + checkTurnDistanceTime * float_6 * float_7 * Time.fixedDeltaTime));
		Vector3 pointWithCurvesLengthCache = pathSpline_0.spline.GetPointWithCurvesLengthCache(num);
		turnCheckerObject.transform.position = pointWithCurvesLengthCache;
		Vector3 position = base.transform.position;
		position.y = pointWithCurvesLengthCache.y;
		Vector3 zero = Vector3.zero;
		zero = ((!method_21()) ? base.transform.forward : (base.transform.forward * -1f));
		return Vector3.Dot(zero, (pointWithCurvesLengthCache - position).normalized);
	}

	public void method_12()
	{
		if (!bool_1)
		{
			return;
		}
		if (_currentPathConfig.once)
		{
			int num = method_14(pathDestination_0) + 1;
			if (num >= dictionary_0.Count - 1)
			{
				bool_1 = false;
				return;
			}
			PathSpline pathSpline = dictionary_0.ElementAt(num).Value as PathSpline;
			PathDestination pathDestination = dictionary_0.ElementAt(num + 1).Value as PathDestination;
			if (pathDestination != null)
			{
				pathDestination_0 = pathDestination;
			}
			if (pathSpline != null)
			{
				method_17(pathSpline);
				bool_3 = true;
				method_4();
				BtrState = EBtrState.Running;
			}
		}
		else
		{
			if (!_currentPathConfig.circle)
			{
				return;
			}
			int num2 = method_14(pathDestination_0) + 1;
			if (num2 >= dictionary_0.Count - 1)
			{
				int_2++;
				MoveToDestination(_currentPathConfig.pathPoints[0]);
				BtrState = EBtrState.Running;
				return;
			}
			if (int_2 >= _currentPathConfig.circleCount)
			{
				bool_1 = false;
				MoveToDestination(_currentPathConfig.exitPoint);
				BtrState = EBtrState.Running;
				return;
			}
			PathSpline pathSpline2 = dictionary_0.ElementAt(num2).Value as PathSpline;
			PathDestination pathDestination2 = dictionary_0.ElementAt(num2 + 1).Value as PathDestination;
			if (pathDestination2 != null)
			{
				pathDestination_0 = pathDestination2;
			}
			if (pathSpline2 != null)
			{
				method_17(pathSpline2);
				method_4();
				bool_3 = true;
				BtrState = EBtrState.Running;
			}
		}
	}

	public void method_13(PathDestination pathDestination)
	{
		base.transform.position = pathDestination.transform.position;
		float_7 = 0.1f;
		float_6 = 0.1f;
		bool_5 = true;
		IsPaid = false;
		method_16(pathDestination);
		method_15();
		if (pathDestination.id == _currentPathConfig.exitPoint)
		{
			IncomingToDestinationEvent?.Invoke(pathDestination, isFirst: false, isFinal: true, isLastRotePoint: false);
			MoveDisable();
		}
		else if (pathDestination.id == _currentPathConfig.enterPoint)
		{
			IncomingToDestinationEvent?.Invoke(pathDestination, isFirst: true, isFinal: false, isLastRotePoint: false);
		}
		else
		{
			bool isLastRotePoint = (_currentPathConfig.circle && int_2 == _currentPathConfig.circleCount && pathDestination.id == _currentPathConfig.pathPoints[0]) | (_currentPathConfig.once && pathDestination.id == _currentPathConfig.pathPoints[_currentPathConfig.pathPoints.Count - 1]);
			IncomingToDestinationEvent?.Invoke(pathDestination, isFirst: false, isFinal: false, isLastRotePoint);
		}
		OnIncomingToDestination(pathDestination.id);
	}

	public virtual void OnIncomingToDestination(string pathID)
	{
	}

	public int method_14(PathPartBase pathPart)
	{
		return dictionary_0.Values.ToList().IndexOf(pathPart);
	}

	public void method_15()
	{
		float_14 = UnityEngine.Random.Range(pauseDurationRange.x, pauseDurationRange.y);
		MovePauseOn(float_14);
	}

	public void method_16(PathDestination pathDestination)
	{
		pathSpline_0 = null;
		pathDestination_1 = pathDestination;
	}

	public void method_17(PathSpline nextPathSpline)
	{
		pathSpline_0 = nextPathSpline;
		Vector3[] points = pathSpline_0.spline.Points;
		Vector3 position = points[^1];
		Vector3 position2 = points[0];
		float num = Vector3.Distance(base.transform.position, pathSpline_0.transform.TransformPoint(position));
		float num2 = Vector3.Distance(base.transform.position, pathSpline_0.transform.TransformPoint(position2));
		bool needReverse = num < num2;
		method_19(needReverse);
		method_18(pathSpline_0.spline);
	}

	public void method_18(BezierSpline spline)
	{
		spline.CreateCurvesLengthCache(0.01f);
		float_5 = spline.GetLengthFromChache();
	}

	public void method_19(bool needReverse)
	{
		if (needReverse)
		{
			float_4 = 1f;
			bool_2 = false;
		}
		else
		{
			float_4 = 0f;
			bool_2 = true;
		}
	}

	public void method_20()
	{
		Vector3 position = surfaceCheckerCenter.transform.position;
		Vector3 direction = surfaceCheckerCenter.transform.up * -1f;
		RaycastHit hitInfo = default(RaycastHit);
		if (Physics.Raycast(position, direction, out hitInfo, 30f, EFTHardSettings.Instance.MOVEMENT_MASK))
		{
			if (hitInfo.transform.CompareTag("Vehicle"))
			{
				base.transform.position = vector3_1;
				base.transform.forward = vector3_2;
				return;
			}
			if (Math.Abs(base.transform.position.y - hitInfo.point.y) > snappingThresholdDistance)
			{
				return;
			}
			base.transform.position = hitInfo.point;
			vector3_1 = hitInfo.point;
		}
		Vector3 position2 = SurfaceCheckerFront.transform.position;
		Vector3 direction2 = SurfaceCheckerFront.transform.up * -1f;
		RaycastHit hitInfo2 = default(RaycastHit);
		if (Physics.Raycast(position2, direction2, out hitInfo2, 30f, EFTHardSettings.Instance.MOVEMENT_MASK) && !hitInfo2.transform.CompareTag("Vehicle") && !(Math.Abs(SurfaceCheckerFront.transform.position.y - hitInfo2.point.y) > snappingThresholdDistance))
		{
			Vector3 forward = base.transform.forward;
			Vector3 vector = default(Vector3);
			vector = ((!method_21()) ? (hitInfo2.point + (hitInfo2.point - base.transform.position).normalized) : (hitInfo2.point - (hitInfo2.point - base.transform.position).normalized));
			base.transform.LookAt(vector);
			Vector3 forward2 = base.transform.forward;
			Vector3 forward3 = Vector3.Lerp(forward, forward2, verticalRotateLerp);
			base.transform.forward = forward3;
			vector3_2 = forward3;
		}
	}

	public void CheckNeedReverse(PathDestination pathDestination, PathSpline spline)
	{
		if (pathDestination_1 == null || !pathDestination_1.useReverse)
		{
			return;
		}
		ReverseDirection reverseDirection = ReverseDirection.front;
		Vector3 forward = base.transform.forward;
		Vector3 forward2 = pathDestination.transform.forward;
		Vector3 point = spline.spline.GetPoint(bool_2 ? 0.01f : 0.99f);
		point.y = base.transform.position.y;
		if (Vector3.Dot(forward, (point - base.transform.position).normalized) < 0f)
		{
			pathSpline_1 = spline;
			switch ((Vector3.Dot(forward, forward2) < 0f) ? ReverseDirection.back : ReverseDirection.front)
			{
			case ReverseDirection.back:
				pathReverseData_0 = pathDestination_1.backReverseData;
				break;
			case ReverseDirection.front:
				pathReverseData_0 = pathDestination_1.frontReverseData;
				break;
			}
			bool_4 = true;
			pathReversePart_0 = pathReverseData_0.reverseParts[0];
			method_17(pathReversePart_0.spline);
			if (pathReverseData_0.reverseParts.Count == 1)
			{
				bool_6 = false;
			}
			bool_3 = true;
		}
		else
		{
			bool_4 = false;
		}
	}

	public bool method_21()
	{
		if (bool_4)
		{
			return pathReversePart_0.moveType == ReverseMoveType.backward;
		}
		return false;
	}

	public void method_22()
	{
		currentSpeed = 0f;
		float_0 = 0f;
		int_0 = 0;
	}

	public void method_23()
	{
		realAcceleration = 0f;
		float_2 = 0f;
		int_1 = 0;
	}

	public void method_24(float deltaTime)
	{
		float val = previousCurrentSpeed - currentSpeed;
		bodySpring.AddAccelerationLimitless(0, val);
		bodySpring.Update(deltaTime);
		Vector3 localEulerAngles = base.transform.localEulerAngles;
		localEulerAngles.x = bodySpring.GetValue(useOffset: false).x;
		base.transform.localEulerAngles = localEulerAngles;
	}

	public float method_25(float newSpeed)
	{
		int_1++;
		if ((float)int_0 == float_1)
		{
			method_23();
			int_1 = 1;
		}
		float_2 += newSpeed;
		return float_2 / (float)int_1;
	}
}
