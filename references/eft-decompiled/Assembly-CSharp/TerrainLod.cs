using UnityEngine;

[ExecuteInEditMode]
public class TerrainLod : MonoBehaviour
{
	[SerializeField]
	private Terrain _terrain;

	[SerializeField]
	private GameObject _terrainLod;

	private bool bool_0 = true;

	private bool bool_1 = true;

	private bool bool_2 = true;

	public bool TerrainIsVisible
	{
		get
		{
			return bool_2;
		}
		set
		{
			if (bool_0)
			{
				_terrain.drawHeightmap = value;
			}
			if (bool_1)
			{
				_terrain.drawTreesAndFoliage = value;
			}
			_terrainLod.SetActive(!value);
			bool_2 = value;
		}
	}

	public void SetSettings(bool disableTerrainDraw, bool disableTerrainTreesDraw)
	{
		bool_0 = disableTerrainDraw;
		bool_1 = disableTerrainTreesDraw;
	}

	public void DestroyLod()
	{
		if (_terrainLod != null)
		{
			Object.DestroyImmediate(_terrainLod);
			_terrainLod = null;
		}
	}
}
