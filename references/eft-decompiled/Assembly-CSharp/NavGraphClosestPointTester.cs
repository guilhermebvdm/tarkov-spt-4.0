using System.Runtime.CompilerServices;
using UnityEngine;

public class NavGraphClosestPointTester : MonoBehaviour
{
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

	public void Refresh()
	{
		if (Object.FindObjectsOfType<BotZone>() != null)
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
			gclass589_0.SetPos(position, navGraphContainer_0);
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
		}
	}
}
