using UnityEngine;
using UnityEngine.AI;

public class NavMeshCutElement : MonoBehaviour
{
	public NavMeshCutGroup Group;

	public NavMeshObstacle Obstacle;

	public BoxCollider Collider;

	private float float_0;

	public float CutPeriodSec = 300f;

	public bool IsCut => Obstacle.carving;

	public Vector3 Position => base.transform.position;

	public bool TryCut()
	{
		if (HaveFreeToCut())
		{
			if (Obstacle.carving)
			{
				return true;
			}
			Group.Cut(this);
			return true;
		}
		if (Group.UnCutOldest())
		{
			Group.Cut(this);
			return true;
		}
		return false;
	}

	public void Update()
	{
		if (Obstacle.carving && float_0 < Time.time)
		{
			Group.UnCut(this);
		}
	}

	public bool HaveFreeToCut()
	{
		return Group.HaveFreeToCut(this);
	}

	public void OnDrawGizmosSelected()
	{
		if (Group != null)
		{
			Group.DrawGizmo();
		}
	}

	public bool IsInside(Vector3 pos)
	{
		return GClass369.PointInOABB(pos, Collider);
	}

	public void SetUncutTime()
	{
		float_0 = Time.time + CutPeriodSec;
	}
}
