using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Koenigz.PerfectCulling.EFT;

[RequireComponent(typeof(PerfectCullingLightGroupPreProcess))]
public class LightVolumeSettings : MonoBehaviour
{
	[Serializable]
	public class LightSettings
	{
		public CullingObject LightObject;

		public Vector3 Offset;

		public Quaternion Rotation;

		public LightMeshQuality MeshQuality;

		public float RangeModifier;

		public float CollisionMargin;

		public float SpotAngleModifier;

		public LayerMask RaycastMask = 264192;

		public SerializedMesh SerializedLightVolumeMesh;
	}

	[Serializable]
	public class SerializedMesh
	{
		public Vector3[] Vertices;

		public int[] Triangles;

		public Mesh CreateMesh()
		{
			Mesh mesh = new Mesh();
			mesh.vertices = Vertices;
			mesh.triangles = Triangles;
			mesh.RecalculateNormals();
			mesh.RecalculateBounds();
			return mesh;
		}
	}

	[CompilerGenerated]
	public class Class840
	{
		public CullingObject light;

		public bool method_0(LightSettings x)
		{
			return x.LightObject == light;
		}
	}

	public const int DEFAULT_VOLUME_MESH_RAYCAST_MASK = 264192;

	[SerializeField]
	private LayerMask _raycastMask = 264192;

	[SerializeField]
	private List<LightSettings> _lightParameters = new List<LightSettings>();

	public IReadOnlyCollection<LightSettings> LightBakeParameters => _lightParameters;

	public static LightVolumeSettings Get(CullingObject cullingObject)
	{
		return cullingObject.transform.root.GetComponent<LightVolumeSettings>();
	}

	public LightSettings GetLightSettings(CullingObject light)
	{
		LightSettings lightSettings = _lightParameters.Find((LightSettings x) => x.LightObject == light);
		if (lightSettings == null)
		{
			lightSettings = new LightSettings();
			lightSettings.LightObject = light;
			lightSettings.Rotation = Quaternion.identity;
			lightSettings.MeshQuality = LightMeshQuality.Medium;
			lightSettings.RangeModifier = 1f;
			lightSettings.SpotAngleModifier = 1f;
			lightSettings.RaycastMask = _raycastMask;
			_lightParameters.Add(lightSettings);
		}
		return lightSettings;
	}
}
