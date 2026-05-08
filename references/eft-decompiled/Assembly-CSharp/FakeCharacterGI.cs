using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class FakeCharacterGI : MonoBehaviour
{
	public Shader FakeCharacterGIShader;

	public Color GIColor;

	public Vector3 GIDirection;

	public float GIPower;

	public Color SkylightColor;

	public Vector3 SkylightDirection;

	public float SkylightPower;

	private Material material_0;

	private Camera camera_0;

	private static readonly int int_0 = Shader.PropertyToID("_InverseView");

	private static readonly int int_1 = Shader.PropertyToID("_GIColor");

	private static readonly int int_2 = Shader.PropertyToID("_GIDirection");

	private static readonly int int_3 = Shader.PropertyToID("_GIPower");

	private static readonly int int_4 = Shader.PropertyToID("_SkylightColor");

	private static readonly int int_5 = Shader.PropertyToID("_SkylightDirection");

	private static readonly int int_6 = Shader.PropertyToID("_SkylightPower");

	public void Awake()
	{
		camera_0 = GetComponent<Camera>();
		method_1();
	}

	public void OnValidate()
	{
		method_1();
	}

	[ImageEffectOpaque]
	public void OnRenderImage(RenderTexture src, RenderTexture dest)
	{
		Matrix4x4 cameraToWorldMatrix = camera_0.cameraToWorldMatrix;
		method_0().SetMatrix(int_0, cameraToWorldMatrix);
		Graphics.Blit(src, dest, method_0());
	}

	public Material method_0()
	{
		if (material_0 != null)
		{
			return material_0;
		}
		return material_0 = new Material(FakeCharacterGIShader);
	}

	public void method_1()
	{
		Material material = method_0();
		material.SetColor(int_1, GIColor);
		material.SetVector(int_2, GIDirection);
		material.SetFloat(int_3, GIPower);
		material.SetColor(int_4, SkylightColor);
		material.SetVector(int_5, SkylightDirection);
		material.SetFloat(int_6, SkylightPower);
	}
}
