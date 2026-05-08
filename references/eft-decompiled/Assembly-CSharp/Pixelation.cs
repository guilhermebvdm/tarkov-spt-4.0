using EFT.BlitDebug;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class Pixelation : MonoBehaviour
{
	public enum PixelationMode
	{
		Point = 0,
		CRT = 1,
		None = 10
	}

	public bool On;

	public PixelationMode Mode;

	[SerializeField]
	private Shader _pixelationShader;

	[SerializeField]
	[Range(2f, 2048f)]
	private float _blockCount = 256f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _alpha = 1f;

	[SerializeField]
	private Texture _pixelationMask;

	private Camera camera_0;

	private Material material_0;

	private static readonly int int_0 = Shader.PropertyToID("_PixelBlockCount");

	private static readonly int int_1 = Shader.PropertyToID("_PixelBlockSize");

	private static readonly int int_2 = Shader.PropertyToID("_PixelationMask");

	private static readonly int int_3 = Shader.PropertyToID("_Alpha");

	public void Awake()
	{
		camera_0 = GetComponent<Camera>();
	}

	public void OnRenderImage(RenderTexture source, RenderTexture destanation)
	{
		if (On && Mode != PixelationMode.None)
		{
			method_1();
			if (Mode != PixelationMode.None)
			{
				DebugGraphics.Blit(source, destanation, method_0(), (int)Mode);
			}
		}
		else
		{
			GClass860.BlitOrCopy(source, destanation);
		}
	}

	public Material method_0()
	{
		if (material_0 != null)
		{
			return material_0;
		}
		return material_0 = new Material(_pixelationShader);
	}

	public void method_1()
	{
		Material material = method_0();
		Vector2 vector = new Vector2(_blockCount, _blockCount / camera_0.aspect);
		Vector2 vector2 = new Vector2(1f / vector.x, 1f / vector.y);
		material.SetVector(int_0, vector);
		material.SetVector(int_1, vector2);
		material.SetTexture(int_2, _pixelationMask);
		material.SetFloat(int_3, _alpha);
	}
}
