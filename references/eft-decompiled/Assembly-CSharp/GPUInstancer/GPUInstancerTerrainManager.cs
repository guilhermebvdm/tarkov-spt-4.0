using UnityEngine;

namespace GPUInstancer;

public abstract class GPUInstancerTerrainManager : GPUInstancerManager
{
	[SerializeField]
	private Terrain _terrain;

	protected bool replacingInstances;

	protected bool initalizingInstances;

	public Terrain terrain => _terrain;

	public override bool isCulled => !_terrain.drawHeightmap;

	public override void Awake()
	{
		base.Awake();
		if (Application.isPlaying && useFloatingOriginHandler && terrain != null)
		{
			floatingOriginTransform = terrain.transform;
		}
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
	}

	public override void Reset()
	{
		base.Reset();
		if (terrain == null && base.gameObject.GetComponent<Terrain>() != null)
		{
			SetupManagerWithTerrain(base.gameObject.GetComponent<Terrain>());
		}
	}

	public virtual void SetupManagerWithTerrain(Terrain terrain, bool isOptic = false)
	{
		if (!(terrain == null) && !Application.isPlaying)
		{
			_terrain = terrain;
			IsOptic = isOptic;
			GeneratePrototypes();
			AddProxyToTerrain();
		}
	}

	public GPUInstancerTerrainProxy AddProxyToTerrain()
	{
		return null;
	}

	public static void smethod_2(GPUInstancerTerrainSettings terrainSettings)
	{
	}

	public GPUInstancerTerrainManager()
	{
	}
}
