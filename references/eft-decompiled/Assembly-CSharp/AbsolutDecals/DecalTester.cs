using System.Collections.Generic;
using UnityEngine;

namespace AbsolutDecals;

public class DecalTester : MonoBehaviour
{
	public LayerMask Mask = -1;

	public Material Mat;

	private List<Transform> list_0 = new List<Transform>();

	private List<Renderer> list_1 = new List<Renderer>();

	private int int_0;

	private int int_1;

	public void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var _, 1000f, Mask);
		}
		if (Input.GetKeyDown(KeyCode.P))
		{
			MonoBehaviourSingleton<DecalSystem>.Instance.UpdateNonStaticSceneData();
		}
	}

	public bool method_0(Renderer currentRenderer)
	{
		if (currentRenderer == null)
		{
			return false;
		}
		SkinnedMeshRenderer component = currentRenderer.GetComponent<SkinnedMeshRenderer>();
		if (component == null)
		{
			if (currentRenderer.GetComponent<MeshFilter>() == null)
			{
				return false;
			}
			list_0.Add(currentRenderer.transform);
			list_1.Add(currentRenderer.GetComponent<Renderer>());
			if (currentRenderer.gameObject.isStatic)
			{
				if (!Application.isPlaying)
				{
					int_0++;
					int_1++;
				}
				else
				{
					int_0++;
					int_1++;
				}
			}
			else
			{
				int_0++;
				int_1++;
			}
		}
		else if (component.sharedMesh == null)
		{
			return false;
		}
		return true;
	}
}
