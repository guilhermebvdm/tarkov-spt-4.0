using System.Collections.Generic;
using UnityEngine;

namespace GPUInstancer;

public class GPUInstancerFloatingOriginHandler : MonoBehaviour
{
	public List<GPUInstancerManager> gPUIManagers;

	private Vector3 vector3_0;

	public void OnEnable()
	{
		vector3_0 = base.transform.position;
		base.transform.hasChanged = false;
	}

	public void Update()
	{
		if (!base.transform.hasChanged || !(base.transform.position != vector3_0))
		{
			return;
		}
		foreach (GPUInstancerManager gPUIManager in gPUIManagers)
		{
			if (gPUIManager != null)
			{
				GClass1257.SetGlobalPositionOffset(gPUIManager, base.transform.position - vector3_0);
			}
		}
		vector3_0 = base.transform.position;
		base.transform.hasChanged = false;
	}
}
