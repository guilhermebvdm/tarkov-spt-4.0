using System;
using UnityEngine;

namespace EFT.Rendering.Clouds;

public class CloudDataDefaultResources : ScriptableObject
{
	[SerializeField]
	private Texture2D _defaultCloudMap;

	[SerializeField]
	private ComputeShader _bakeCloudTextureCs;

	[SerializeField]
	private ComputeShader _bakeCloudShadowsCs;

	[SerializeField]
	private Shader _cloudLayerPS;

	private static CloudDataDefaultResources cloudDataDefaultResources_0;

	public Texture2D DefaultCloudMap => _defaultCloudMap;

	public ComputeShader BakeCloudTextureCs => _bakeCloudTextureCs;

	public ComputeShader BakeCloudShadowsCs => _bakeCloudShadowsCs;

	public Shader CloudLayerPS => _cloudLayerPS;

	public static CloudDataDefaultResources Instance
	{
		get
		{
			if ((bool)cloudDataDefaultResources_0)
			{
				return cloudDataDefaultResources_0;
			}
			cloudDataDefaultResources_0 = Resources.Load<CloudDataDefaultResources>("CloudDataDefaultResources");
			if (cloudDataDefaultResources_0 == null)
			{
				throw new NullReferenceException("There is no CloudDataDefaultResources!");
			}
			return cloudDataDefaultResources_0;
		}
	}
}
