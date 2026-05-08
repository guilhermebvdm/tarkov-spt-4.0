using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class SSRReplacement : MonoBehaviour
{
	public Shader ssrReplacementShader;

	public RenderTexture tex;

	private GameObject gameObject_0;

	private Camera camera_0;

	public void method_0()
	{
		gameObject_0 = new GameObject("ShaderCamera", typeof(Camera));
		Camera component = GetComponent<Camera>();
		camera_0 = gameObject_0.GetComponent<Camera>();
		camera_0.CopyFrom(component);
		camera_0.aspect = component.aspect;
		camera_0.SetReplacementShader(ssrReplacementShader, "RenderType");
		camera_0.targetTexture = tex;
		camera_0.renderingPath = RenderingPath.Forward;
		camera_0.backgroundColor = Color.black;
		camera_0.clearFlags = CameraClearFlags.Color;
		gameObject_0.hideFlags = HideFlags.HideAndDontSave;
		gameObject_0.transform.parent = base.transform;
		gameObject_0.transform.localEulerAngles = Vector3.zero;
		gameObject_0.transform.localPosition = Vector3.zero;
	}

	public void OnDestroy()
	{
		Object.DestroyImmediate(camera_0);
	}

	public void OnPostRender()
	{
		if (camera_0 == null)
		{
			method_0();
		}
		camera_0.RenderWithShader(ssrReplacementShader, "RenderType");
		gameObject_0.transform.position = base.transform.position;
		gameObject_0.transform.rotation = base.transform.rotation;
	}
}
