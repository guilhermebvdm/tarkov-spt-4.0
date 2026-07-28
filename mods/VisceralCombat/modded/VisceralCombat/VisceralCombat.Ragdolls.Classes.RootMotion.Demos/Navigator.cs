using System;
using UnityEngine;
using UnityEngine.AI;

namespace VisceralCombat.Ragdolls.Classes.RootMotion.Demos;

[Serializable]
public class Navigator
{
	public enum State
	{
		Idle,
		Seeking,
		OnPath
	}

	[Tooltip("Should this Navigator be actively seeking a path.")]
	public bool activeTargetSeeking;

	[Tooltip("Increase this value if the character starts running in a circle, not able to reach the corner because of a too large turning radius.")]
	public float cornerRadius = 0.5f;

	[Tooltip("Recalculate path if target position has moved by this distance from the position it was at when the path was originally calculated")]
	public float recalculateOnPathDistance = 1f;

	[Tooltip("Sample within this distance from sourcePosition.")]
	public float maxSampleDistance = 5f;

	[Tooltip("Interval of updating the path")]
	public float nextPathInterval = 3f;

	private Transform transform;

	private int cornerIndex;

	private Vector3[] corners = (Vector3[])(object)new Vector3[0];

	private NavMeshPath path;

	private Vector3 lastTargetPosition;

	private bool initiated;

	private float nextPathTime;

	public Vector3 normalizedDeltaPosition { get; private set; }

	public State state { get; private set; }

	public void Initiate(Transform transform)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Expected O, but got Unknown
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		this.transform = transform;
		path = new NavMeshPath();
		initiated = true;
		cornerIndex = 0;
		corners = (Vector3[])(object)new Vector3[0];
		state = State.Idle;
		lastTargetPosition = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
	}

	public void Update(Vector3 targetPosition)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Invalid comparison between Unknown and I4
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Invalid comparison between Unknown and I4
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Invalid comparison between Unknown and I4
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		if (!initiated)
		{
			return;
		}
		switch (state)
		{
		case State.Seeking:
			normalizedDeltaPosition = Vector3.zero;
			if ((int)path.status == 0)
			{
				corners = path.corners;
				cornerIndex = 0;
				if (corners.Length == 0)
				{
					Stop();
				}
				else
				{
					state = State.OnPath;
				}
			}
			if ((int)path.status == 1)
			{
			}
			if ((int)path.status != 2)
			{
			}
			break;
		case State.OnPath:
			if (activeTargetSeeking && Time.time > nextPathTime && HorDistance(targetPosition, lastTargetPosition) > recalculateOnPathDistance)
			{
				CalculatePath(targetPosition);
			}
			else
			{
				if (cornerIndex >= corners.Length)
				{
					break;
				}
				Vector3 val = corners[cornerIndex] - transform.position;
				val.y = 0f;
				float magnitude = ((Vector3)(ref val)).magnitude;
				if (magnitude > 0f)
				{
					normalizedDeltaPosition = val / ((Vector3)(ref val)).magnitude;
				}
				else
				{
					normalizedDeltaPosition = Vector3.zero;
				}
				if (magnitude < cornerRadius)
				{
					cornerIndex++;
					if (cornerIndex >= corners.Length)
					{
						Stop();
					}
				}
			}
			break;
		case State.Idle:
			if (activeTargetSeeking && Time.time > nextPathTime)
			{
				CalculatePath(targetPosition);
			}
			break;
		}
	}

	private void CalculatePath(Vector3 targetPosition)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (Find(targetPosition))
		{
			lastTargetPosition = targetPosition;
			state = State.Seeking;
		}
		else
		{
			Stop();
		}
		nextPathTime = Time.time + nextPathInterval;
	}

	private bool Find(Vector3 targetPosition)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		if (HorDistance(transform.position, targetPosition) < cornerRadius * 2f)
		{
			return false;
		}
		if (NavMesh.CalculatePath(transform.position, targetPosition, -1, path))
		{
			return true;
		}
		NavMeshHit val = default(NavMeshHit);
		if (NavMesh.SamplePosition(targetPosition, ref val, maxSampleDistance, -1) && NavMesh.CalculatePath(transform.position, ((NavMeshHit)(ref val)).position, -1, path))
		{
			return true;
		}
		return false;
	}

	private void Stop()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		state = State.Idle;
		normalizedDeltaPosition = Vector3.zero;
	}

	private float HorDistance(Vector3 p1, Vector3 p2)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		return Vector2.Distance(new Vector2(p1.x, p1.z), new Vector2(p2.x, p2.z));
	}

	public void Visualize()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		if (state == State.Idle)
		{
			Gizmos.color = Color.gray;
		}
		if (state == State.Seeking)
		{
			Gizmos.color = Color.red;
		}
		if (state == State.OnPath)
		{
			Gizmos.color = Color.green;
		}
		if (corners.Length != 0 && state == State.OnPath && cornerIndex == 0)
		{
			Gizmos.DrawLine(transform.position, corners[0]);
		}
		for (int i = 0; i < corners.Length; i++)
		{
			Gizmos.DrawSphere(corners[i], 0.1f);
		}
		if (corners.Length > 1)
		{
			for (int j = 0; j < corners.Length - 1; j++)
			{
				Gizmos.DrawLine(corners[j], corners[j + 1]);
			}
		}
		Gizmos.color = Color.white;
	}
}
