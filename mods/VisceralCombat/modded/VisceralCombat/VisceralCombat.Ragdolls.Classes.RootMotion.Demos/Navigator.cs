using UnityEngine;
using UnityEngine.AI;

namespace VisceralCombat.Ragdolls.Classes.RootMotion.Demos;

public class Navigator : MonoBehaviour
{
	public enum State
	{
		Idle,
		Seeking,
		OnPath
	}

	public Transform target;

	public float cornerRadius = 0.5f;

	public float maxSampleDistance = 5f;

	public bool activeTargetSeeking;

	public float nextPathInterval = 3f;

	private NavMeshPath path;

	private int cornerIndex;

	private Vector3[] corners = new Vector3[0];

	private Vector3 lastTargetPosition;

	private float nextPathTime;

	public State state { get; private set; }

	public Vector3 normalizedDeltaPosition { get; private set; }

	private void Start()
	{
		path = new NavMeshPath();
	}

	public void Update()
	{
		if (target == null)
		{
			Stop();
			return;
		}
		Vector3 position = target.position;

		switch (state)
		{
		case State.Seeking:
			if (path.status == NavMeshPathStatus.PathComplete)
			{
				corners = path.corners;
				if (corners.Length == 0)
				{
					Stop();
				}
				else
				{
					cornerIndex = 0;
					state = State.OnPath;
				}
			}
			if (path.status == NavMeshPathStatus.PathPartial)
			{
				Stop();
			}
			if (path.status == NavMeshPathStatus.PathInvalid)
			{
				Stop();
			}
			break;
		case State.OnPath:
			if (corners.Length == 0)
			{
				Stop();
				break;
			}
			normalizedDeltaPosition = (corners[cornerIndex] - transform.position).normalized;
			if (HorDistance(transform.position, corners[cornerIndex]) < cornerRadius)
			{
				cornerIndex++;
				if (cornerIndex >= corners.Length)
				{
					Stop();
				}
			}
			if (activeTargetSeeking && Time.time > nextPathTime && HorDistance(position, lastTargetPosition) > cornerRadius)
			{
				CalculatePath(position);
			}
			break;
		case State.Idle:
			if (activeTargetSeeking && Time.time > nextPathTime)
			{
				CalculatePath(position);
			}
			break;
		}
	}

	private void CalculatePath(Vector3 targetPosition)
	{
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
		if (HorDistance(transform.position, targetPosition) < cornerRadius * 2f)
		{
			return false;
		}
		if (NavMesh.CalculatePath(transform.position, targetPosition, -1, path))
		{
			return true;
		}
		if (NavMesh.SamplePosition(targetPosition, out NavMeshHit val, maxSampleDistance, -1) && NavMesh.CalculatePath(transform.position, val.position, -1, path))
		{
			return true;
		}
		return false;
	}

	private void Stop()
	{
		state = State.Idle;
		normalizedDeltaPosition = Vector3.zero;
	}

	private float HorDistance(Vector3 p1, Vector3 p2)
	{
		return Vector2.Distance(new Vector2(p1.x, p1.z), new Vector2(p2.x, p2.z));
	}

	public void Visualize()
	{
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
			if (i < corners.Length - 1)
			{
				Gizmos.DrawLine(corners[i], corners[i + 1]);
			}
		}
	}
}
