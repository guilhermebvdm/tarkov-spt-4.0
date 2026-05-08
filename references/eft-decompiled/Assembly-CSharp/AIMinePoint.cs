using System;
using UnityEngine;

public class AIMinePoint : MonoBehaviour, IAICorePointLink
{
	[SerializeField]
	private int _idCore = -1;

	public int Id = -1;

	public float Delta = 1f;

	public static Vector3 BaseLook = Vector3.forward;

	private static Color AUTO_POINT = new Color(0.85f, 0.3f, 0f, 1f);

	private static Color PREWARM_POINT = new Color(0.25f, 0.1f, 0.7f, 1f);

	private static Color PREWARM_POINT_AUTO = new Color(0.15f, 0.1f, 0.9f, 1f);

	public bool IsAuto;

	public bool IsForPrewarm;

	public float SafeRad = -1f;

	public float Priority;

	private AICorePoint _corePointInGame;

	public AICorePoint CorePointInGame => _corePointInGame;

	public Vector3 Position => base.transform.position;

	public float SSafeRad { get; set; }

	public bool HaveSafeRadius { get; set; }

	public void Restore(AICorePointHolder cores)
	{
		_corePointInGame = cores.GetCorePoint(_idCore);
	}

	public void SetCore(AICorePoint coreP)
	{
		HaveSafeRadius = SafeRad > 0f;
		SSafeRad = SafeRad * SafeRad;
		_corePointInGame = coreP;
		_idCore = _corePointInGame.Id;
	}

	public void DrawGizmos()
	{
		float radius = 0.1f;
		if (IsForPrewarm)
		{
			if (IsAuto)
			{
				Gizmos.color = PREWARM_POINT_AUTO;
			}
			else
			{
				Gizmos.color = PREWARM_POINT;
			}
		}
		else if (IsAuto)
		{
			Gizmos.color = AUTO_POINT;
		}
		else
		{
			Gizmos.color = Color.red;
		}
		Gizmos.DrawSphere(Position, 0.2f);
		Gizmos.DrawWireSphere(Position, 0.21f);
		Tuple<Vector3, Vector3> sidePoints = GetSidePoints();
		Vector3 item = sidePoints.Item1;
		Vector3 item2 = sidePoints.Item2;
		Gizmos.DrawSphere(item, radius);
		Gizmos.DrawSphere(item2, radius);
		Gizmos.DrawLine(item2, item);
		Gizmos.DrawLine(Position, Position + Vector3.up * Priority);
	}

	public Tuple<Vector3, Vector3> GetSidePoints()
	{
		float y = base.transform.rotation.eulerAngles.y;
		Vector3 vector = GClass855.RotateOnAngUp(BaseLook, 0f - y) * Delta;
		Vector3 item = Position + vector;
		Vector3 item2 = Position - vector;
		return new Tuple<Vector3, Vector3>(item, item2);
	}

	public void OnDrawGizmosSelected()
	{
		DrawGizmos();
	}

	public void OnDrawGizmos()
	{
		if (GClass806.AIMine)
		{
			DrawGizmos();
		}
	}
}
