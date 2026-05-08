using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class HighLightMesh : MonoBehaviour
{
	public class Class617
	{
		[NonSerialized]
		[CanBeNull]
		public Mesh Mesh_0;

		[NonSerialized]
		public Transform Transform_0;

		[NonSerialized]
		[CanBeNull]
		public SkinnedMeshRenderer SkinnedMeshRenderer_0;

		public Class617([CanBeNull] Mesh mesh, Transform transform, [CanBeNull] SkinnedMeshRenderer smr = null)
		{
			Mesh_0 = mesh;
			Transform_0 = transform;
			SkinnedMeshRenderer_0 = smr;
			if (!(SkinnedMeshRenderer_0 == null))
			{
				Mesh sharedMesh = SkinnedMeshRenderer_0.sharedMesh;
				Mesh_0 = new Mesh
				{
					vertices = sharedMesh.vertices,
					triangles = sharedMesh.triangles,
					bindposes = sharedMesh.bindposes,
					boneWeights = sharedMesh.boneWeights,
					name = "HighLightMesh _mesh"
				};
			}
		}

		public void Draw(CommandBuffer cmd, Material mat, int pass)
		{
			if (!(Mesh_0 == null) && !(Transform_0 == null))
			{
				if (SkinnedMeshRenderer_0 != null)
				{
					SkinnedMeshRenderer_0.BakeMesh(Mesh_0);
				}
				cmd.DrawMesh(Mesh_0, Transform_0.localToWorldMatrix, mat, 0, pass);
			}
		}
	}

	[SerializeField]
	private Material Mat;

	public Color Color;

	public float LineWidth;

	public bool Always;

	public Transform Target;

	private Transform transform_0;

	private float float_0;

	private Color color_0;

	private Class617[] class617_0;

	private int int_0;

	private int int_1;

	private static readonly int int_2 = Shader.PropertyToID("_Color");

	private static readonly int int_3 = Shader.PropertyToID("_Offset");

	private static readonly int int_4 = Shader.PropertyToID("_MaskRT");

	private static readonly int int_5 = Shader.PropertyToID("_FinalRT");

	private Camera camera_0;

	private CommandBuffer commandBuffer_0;

	private RenderTexture renderTexture_0;

	public void Awake()
	{
		camera_0 = GetComponent<Camera>();
		renderTexture_0 = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.R8);
		Mat.SetColor(int_2, Color);
		Mat.SetTexture(int_4, renderTexture_0);
		commandBuffer_0 = new CommandBuffer
		{
			name = "HighLight"
		};
		camera_0.RemoveCommandBuffer(CameraEvent.AfterImageEffectsOpaque, commandBuffer_0);
		camera_0.AddCommandBuffer(CameraEvent.AfterImageEffectsOpaque, commandBuffer_0);
	}

	public void Update()
	{
		if (color_0 != Color)
		{
			color_0 = Color;
			Mat.SetColor(int_2, Color);
		}
		if (int_0 != camera_0.pixelWidth || int_1 != camera_0.pixelHeight || Math.Abs(float_0 - LineWidth) > Mathf.Epsilon)
		{
			int_0 = camera_0.pixelWidth;
			int_1 = camera_0.pixelHeight;
			float_0 = LineWidth;
			Mat.SetVector(int_3, new Vector4(LineWidth / (float)camera_0.pixelWidth, LineWidth / (float)camera_0.pixelHeight, 0f, 0f));
		}
		if (transform_0 != Target)
		{
			transform_0 = Target;
			Renderer[] componentsInChildren = Target.GetComponentsInChildren<Renderer>(includeInactive: false);
			LinkedList<Class617> linkedList = new LinkedList<Class617>();
			Renderer[] array = componentsInChildren;
			foreach (Renderer renderer in array)
			{
				SkinnedMeshRenderer skinnedMeshRenderer = renderer as SkinnedMeshRenderer;
				if (skinnedMeshRenderer != null && skinnedMeshRenderer.enabled)
				{
					linkedList.AddLast(new Class617(null, skinnedMeshRenderer.transform, skinnedMeshRenderer));
				}
				else if (renderer is MeshRenderer && renderer.enabled)
				{
					linkedList.AddLast(new Class617(renderer.GetComponent<MeshFilter>().sharedMesh, renderer.transform));
				}
			}
			class617_0 = new Class617[linkedList.Count];
			linkedList.CopyTo(class617_0, 0);
		}
		commandBuffer_0.Clear();
		commandBuffer_0.SetRenderTarget(renderTexture_0);
		commandBuffer_0.ClearRenderTarget(clearDepth: false, clearColor: true, Color.black);
		if (Always)
		{
			method_0(3);
		}
		else
		{
			method_0(1);
			method_0(2);
		}
		commandBuffer_0.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
		commandBuffer_0.GetTemporaryRT(int_5, -1, -1);
		commandBuffer_0.Blit(BuiltinRenderTextureType.CameraTarget, int_5, Mat, 4);
		commandBuffer_0.Blit(int_5, BuiltinRenderTextureType.CameraTarget);
		commandBuffer_0.ReleaseTemporaryRT(int_5);
	}

	public void method_0(int pass)
	{
		if (class617_0 != null)
		{
			Class617[] array = class617_0;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Draw(commandBuffer_0, Mat, pass);
			}
		}
	}

	public void OnDisable()
	{
		commandBuffer_0.Clear();
		transform_0 = null;
	}

	public void OnDestroy()
	{
		if ((bool)renderTexture_0)
		{
			UnityEngine.Object.DestroyImmediate(renderTexture_0);
		}
		transform_0 = null;
	}
}
