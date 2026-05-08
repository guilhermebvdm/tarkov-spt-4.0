using Koenigz.PerfectCulling.EFT.Autotest;
using UnityEngine;

namespace Koenigz.PerfectCulling;

public class PerfectCullingResourcesLocator : ScriptableObject
{
	private static PerfectCullingResourcesLocator perfectCullingResourcesLocator_0;

	public Material UnlitTagMaterial;

	public PerfectCullingSettings Settings;

	public PerfectCullingColorTable ColorTable;

	public PerfectCullingVolumeBakeData tempVolumeBakeData;

	public Shader[] ExcludeTransparentShaders;

	public Shader[] ValidTransparentShaders;

	public Material ProxyLightMaterial;

	public Material RainbowProxyMaterial;

	public GameObject QuadPrefab;

	public Mesh[] LightSpheres;

	public Material AutotestRendererMaterialOpaque;

	public Material AutotestRendererMaterialTransparent;

	public Material AutotestProxyMaterialOpaque;

	public Material AutotestProxyMaterialTransparent;

	public PerfectCullingAutoTestCamera AutoTestCameraPrefab;

	public ComputeShader PointExtractorComputeShader;

	public GameObject AutotestProxyPrefab;

	public Material AutotestTerrainMaterial;

	public static PerfectCullingResourcesLocator Instance
	{
		get
		{
			if (perfectCullingResourcesLocator_0 == null)
			{
				PerfectCullingResourcesLocator[] array = Resources.LoadAll<PerfectCullingResourcesLocator>(GClass1224.ResourcesFolder);
				if (array.Length == 0)
				{
					return null;
				}
				perfectCullingResourcesLocator_0 = array[0];
			}
			return perfectCullingResourcesLocator_0;
		}
	}
}
