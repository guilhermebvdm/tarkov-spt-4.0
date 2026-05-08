using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class NavGraphVoxelAffectorTester : MonoBehaviour
{
	public bool WithLogs;

	private NavGraphContainer navGraphContainer_0;

	private GClass589 gclass589_0;

	[CompilerGenerated]
	private bool bool_0;

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

	public int AffectedElements()
	{
		if (gclass589_0 == null)
		{
			return -1;
		}
		return gclass589_0.AffectedElements();
	}

	public int AffectedDoorElements()
	{
		if (gclass589_0 == null)
		{
			return -1;
		}
		return gclass589_0.AffectedDoorElements();
	}

	public void Refresh()
	{
		if (navGraphContainer_0 == null)
		{
			navGraphContainer_0 = NavGraphContainer.CreateOfFind();
			gclass589_0 = new GClass589();
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
			gclass589_0.SetPos(position, WithLogs);
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
			Gizmos.color = Color.green;
			Gizmos.DrawSphere(base.transform.position, 0.5f);
			try
			{
				gclass589_0.TryDrawAffectionGizmos();
			}
			catch (Exception)
			{
				Gizmos.DrawLine(base.transform.position, gclass589_0.GetVoxelPos());
			}
		}
	}
}
