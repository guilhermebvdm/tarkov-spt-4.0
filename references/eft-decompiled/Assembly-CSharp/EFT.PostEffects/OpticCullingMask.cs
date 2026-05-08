using UnityEngine;
using UnityEngine.Rendering;

namespace EFT.PostEffects;

[RequireComponent(typeof(Camera))]
[DisallowMultipleComponent]
public class OpticCullingMask : MonoBehaviour
{
	[SerializeField]
	private Shader _cullingMaskShader;

	[SerializeField]
	[Range(0f, 2f)]
	public float _maskScale = 1f;

	private static Mesh mesh_0;

	private static readonly Matrix4x4 matrix4x4_0 = Matrix4x4.identity;

	private Camera camera_0;

	private Material material_0;

	private CameraEvent cameraEvent_0 = CameraEvent.BeforeGBuffer;

	private CommandBuffer commandBuffer_0;

	private int int_0;

	public void Awake()
	{
		if (mesh_0 == null)
		{
			mesh_0 = SSAOMask.GetQuadMesh(0f);
		}
		if (_cullingMaskShader == null)
		{
			_cullingMaskShader = GClass872.Find("Hidden/OpticCullingMask");
		}
		material_0 = new Material(_cullingMaskShader);
		int_0 = Shader.PropertyToID("_MaskScale");
		UpdateParameters();
		commandBuffer_0 = new CommandBuffer
		{
			name = "Optic Culling Mask"
		};
		commandBuffer_0.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
		commandBuffer_0.DrawMesh(mesh_0, matrix4x4_0, material_0);
		camera_0 = GetComponent<Camera>();
		camera_0.AddCommandBuffer(cameraEvent_0, commandBuffer_0);
	}

	public void OnDestroy()
	{
		commandBuffer_0?.Release();
	}

	public void UpdateParameters()
	{
		if (material_0 != null)
		{
			material_0.SetFloat(int_0, _maskScale);
		}
	}
}
