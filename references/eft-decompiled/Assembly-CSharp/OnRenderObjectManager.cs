using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class OnRenderObjectManager : MonoBehaviour
{
	private List<OnRenderObjectConnector> list_0 = new List<OnRenderObjectConnector>();

	public void Start()
	{
		list_0 = new List<OnRenderObjectConnector>();
		OnRenderObjectConnector.OnRenderObjectConnectorAdd += method_0;
		OnRenderObjectConnector.OnRenderObjectConnectorRemove += method_1;
		method_2();
	}

	public void OnDestroy()
	{
		list_0.Clear();
		OnRenderObjectConnector.OnRenderObjectConnectorAdd -= method_0;
		OnRenderObjectConnector.OnRenderObjectConnectorRemove -= method_1;
	}

	public void OnRenderObject()
	{
		Camera current = Camera.current;
		if (!current)
		{
			return;
		}
		CameraClass instance = CameraClass.Instance;
		if (current == instance.Camera || current == instance.OpticCameraManager.Camera)
		{
			for (int i = 0; i < list_0.Count; i++)
			{
				list_0[i].ManualOnRenderObject(current);
			}
		}
	}

	public void method_0(OnRenderObjectConnector connector)
	{
		if (!list_0.Contains(connector))
		{
			list_0.Add(connector);
		}
	}

	public void method_1(OnRenderObjectConnector connector)
	{
		if (list_0.Contains(connector))
		{
			list_0.Remove(connector);
		}
	}

	public void method_2()
	{
		OnRenderObjectConnector[] array = Object.FindObjectsOfType<OnRenderObjectConnector>();
		for (int i = 0; i < array.Length; i++)
		{
			method_0(array[i]);
		}
	}
}
