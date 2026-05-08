using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Camera))]
[ExecuteAlways]
[DisallowMultipleComponent]
public class SSAOMask : MonoBehaviour
{
	[SerializeField]
	private Shader _maskShader;

	private static Mesh mesh_0;

	private static readonly Matrix4x4 matrix4x4_0 = Matrix4x4.identity;

	private Camera camera_0;

	private SSAA ssaa_0;

	private CameraEvent cameraEvent_0 = CameraEvent.BeforeLighting;

	private CommandBuffer commandBuffer_0;

	private RenderTexture renderTexture_0;

	private Material material_0;

	private int int_0;

	private int int_1;

	private static readonly int int_2 = Shader.PropertyToID("_SSAOMask");

	public void Start()
	{
		if (mesh_0 == null)
		{
			mesh_0 = GetQuadMesh();
		}
		if (_maskShader == null)
		{
			_maskShader = GClass872.Find("Hidden/SSAOMask");
		}
		commandBuffer_0 = new CommandBuffer();
		commandBuffer_0.name = "SSAO Mask";
		camera_0 = GetComponent<Camera>();
		ssaa_0 = GetComponent<SSAA>();
		camera_0.RemoveCommandBuffer(cameraEvent_0, commandBuffer_0);
		camera_0.AddCommandBuffer(cameraEvent_0, commandBuffer_0);
		material_0 = new Material(_maskShader);
		int num = (ssaa_0 ? ssaa_0.GetInputWidth() : camera_0.pixelWidth);
		int num2 = (ssaa_0 ? ssaa_0.GetInputHeight() : camera_0.pixelHeight);
		int_0 = num2;
		int_1 = num;
		method_0();
	}

	public void method_0()
	{
		if (commandBuffer_0 != null)
		{
			commandBuffer_0.Clear();
			if (renderTexture_0 != null)
			{
				Object.DestroyImmediate(renderTexture_0);
			}
			int width = (ssaa_0 ? ssaa_0.GetInputWidth() : camera_0.pixelWidth);
			int height = (ssaa_0 ? ssaa_0.GetInputHeight() : camera_0.pixelHeight);
			renderTexture_0 = new RenderTexture(width, height, 0, RenderTextureFormat.R8)
			{
				name = "SSAOMask _maskTexture " + camera_0.name
			};
			commandBuffer_0.SetRenderTarget(renderTexture_0, BuiltinRenderTextureType.CurrentActive);
			commandBuffer_0.ClearRenderTarget(clearDepth: false, clearColor: true, Color.white);
			commandBuffer_0.DrawMesh(mesh_0, matrix4x4_0, material_0);
			Shader.SetGlobalTexture(int_2, renderTexture_0);
		}
	}

	public void OnPreCull()
	{
		if (!(camera_0 == null))
		{
			int num = (ssaa_0 ? ssaa_0.GetInputWidth() : camera_0.pixelWidth);
			int num2 = (ssaa_0 ? ssaa_0.GetInputHeight() : camera_0.pixelHeight);
			if (num2 != int_0 || num != int_1)
			{
				int_0 = num2;
				int_1 = num;
				method_0();
			}
		}
	}

	public void OnDestroy()
	{
		if (commandBuffer_0 != null)
		{
			camera_0.RemoveCommandBuffer(cameraEvent_0, commandBuffer_0);
		}
		if (renderTexture_0 != null)
		{
			Object.DestroyImmediate(renderTexture_0);
		}
	}

	public static Mesh GetQuadMesh(float z = 0.1f)
	{
		Vector3[] vertices = new Vector3[4]
		{
			new Vector3(-1f, -1f, z),
			new Vector3(1f, -1f, z),
			new Vector3(-1f, 1f, z),
			new Vector3(1f, 1f, z)
		};
		Vector2[] uv = new Vector2[4]
		{
			new Vector2(0f, 0f),
			new Vector2(1f, 0f),
			new Vector2(0f, 1f),
			new Vector2(1f, 1f)
		};
		int[] triangles = new int[6] { 0, 1, 3, 3, 2, 0 };
		Mesh mesh = new Mesh();
		mesh.vertices = vertices;
		mesh.uv = uv;
		mesh.triangles = triangles;
		mesh.RecalculateBounds();
		mesh.name = "QuadMeshForAmbientLight";
		return mesh;
	}
}
