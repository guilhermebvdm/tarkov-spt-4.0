using System.Runtime.CompilerServices;
using UnityEngine;

public class NavGraphVoxelCreatorTester : MonoBehaviour
{
	public bool WithLogs;

	private NavGraphContainer navGraphContainer_0;

	[CompilerGenerated]
	private bool bool_0;

	public int CurVoxelId;

	private NavGraphVoxel navGraphVoxel_0;

	public bool Inited
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

	public bool HaveVoxel => navGraphVoxel_0 != null;

	public void Refresh()
	{
		if (Object.FindObjectsOfType<BotZone>() != null)
		{
			navGraphContainer_0 = NavGraphContainer.CreateOfFind();
			Inited = true;
		}
	}

	public void TurnOff()
	{
		Inited = false;
	}

	public void UpdateTest()
	{
		if (Inited && !(navGraphContainer_0 == null))
		{
			Vector3 position = base.transform.position;
			NavGraphVoxel voxelSafe = navGraphContainer_0.GetVoxelSafe(position);
			if (voxelSafe.Id != CurVoxelId)
			{
				navGraphVoxel_0 = voxelSafe;
				CurVoxelId = voxelSafe.Id;
			}
		}
	}

	public void RefreshPosition()
	{
		Vector3 position = base.transform.position;
		CurVoxelId = (navGraphVoxel_0 = navGraphContainer_0.GetVoxelSafe(position)).Id;
	}

	public void DoTest()
	{
		if (navGraphVoxel_0 != null)
		{
			GClass417.CachePathesToVoxel(navGraphVoxel_0, withDebug: true);
		}
	}

	public void Update()
	{
		UpdateTest();
	}

	public void OnDrawGizmos()
	{
		if (Inited && !(navGraphContainer_0 == null))
		{
			if (navGraphVoxel_0 != null)
			{
				navGraphVoxel_0.OnDrawGizmos(Color.blue);
			}
			Gizmos.color = Color.green;
			Gizmos.DrawSphere(base.transform.position, 0.5f);
			Gizmos.DrawWireSphere(base.transform.position, 0.65f);
			Gizmos.DrawWireSphere(base.transform.position, 0.6f);
		}
		else
		{
			Gizmos.color = Color.red;
			Gizmos.DrawSphere(base.transform.position, 0.5f);
		}
	}
}
