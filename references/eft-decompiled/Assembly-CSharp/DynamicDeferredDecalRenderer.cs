using System;
using System.Runtime.CompilerServices;
using EFT.Ballistics;
using UnityEngine;

public class DynamicDeferredDecalRenderer : MonoBehaviour
{
	[CompilerGenerated]
	private Material material_0;

	[CompilerGenerated]
	private Transform transform_0;

	[CompilerGenerated]
	private GameObject gameObject_0;

	[CompilerGenerated]
	private int int_0;

	private Action<BallisticCollider> action_0;

	private static readonly int int_1 = Shader.PropertyToID("_UvStartEnd");

	public Material DecalMaterial
	{
		[CompilerGenerated]
		get
		{
			return material_0;
		}
		[CompilerGenerated]
		set
		{
			material_0 = value;
		}
	}

	public Matrix4x4 ModelMatrix => base.transform.localToWorldMatrix;

	public Vector3 Position => base.transform.position;

	public Transform TransformHelper
	{
		[CompilerGenerated]
		get
		{
			return transform_0;
		}
		[CompilerGenerated]
		set
		{
			transform_0 = value;
		}
	}

	public GameObject GameObjectHelper
	{
		[CompilerGenerated]
		get
		{
			return gameObject_0;
		}
		[CompilerGenerated]
		set
		{
			gameObject_0 = value;
		}
	}

	public int CullingGroupSphereIndex
	{
		[CompilerGenerated]
		get
		{
			return int_0;
		}
		[CompilerGenerated]
		set
		{
			int_0 = value;
		}
	}

	public void Init(Material mat, Mesh mesh, Vector3 projectionDirection, Vector4 uvStartEnd, bool tiled, int cullingGroupIndex)
	{
		DecalMaterial = (tiled ? new Material(mat) : mat);
		CullingGroupSphereIndex = cullingGroupIndex;
		DecalMaterial.SetVector(int_1, uvStartEnd);
	}

	public bool ManualUpdate()
	{
		if (!(TransformHelper == null) && TransformHelper.hasChanged)
		{
			base.transform.position = TransformHelper.position;
			base.transform.rotation = TransformHelper.rotation;
			TransformHelper.hasChanged = false;
			return true;
		}
		return false;
	}
}
