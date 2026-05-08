using UnityEngine;

[ExecuteInEditMode]
public class OcclusionCullingSwitcher : DisablerCullingObjectBase
{
	public bool Boolean_0 => CameraClass.Instance.IsOcclusionCullingAllowed;

	public override void SetComponentsEnabled(bool hasEntered)
	{
		method_3();
	}

	public void method_3()
	{
		Camera camera = method_4();
		if (camera != null)
		{
			bool flag = Boolean_0 && HasEntered;
			if (camera.useOcclusionCulling != flag)
			{
				camera.useOcclusionCulling = flag;
				Debug.Log("camera.useOcclusionCulling = " + camera.useOcclusionCulling);
			}
		}
	}

	public Camera method_4()
	{
		return CameraClass.Instance.Camera;
	}
}
