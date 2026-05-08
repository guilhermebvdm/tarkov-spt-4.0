using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Camera))]
public class AreaLightManager : MonoBehaviour
{
	[SerializeField]
	private Shader ClearStencilShader;

	private const int int_0 = 192;

	private Material material_0;

	private CommandBuffer commandBuffer_0;

	private Camera camera_0;

	private Mesh mesh_0;

	public void Awake()
	{
		material_0 = new Material(ClearStencilShader);
		material_0.SetFloat("_StencilValue", 192f);
		camera_0 = GetComponent<Camera>();
		mesh_0 = AmbientLight.GetQuadMesh();
		commandBuffer_0 = new CommandBuffer
		{
			name = "ClearStencil For AreaLight"
		};
		commandBuffer_0.DrawMesh(mesh_0, Matrix4x4.identity, material_0);
		camera_0.AddCommandBuffer(CameraEvent.AfterImageEffectsOpaque, commandBuffer_0);
	}
}
