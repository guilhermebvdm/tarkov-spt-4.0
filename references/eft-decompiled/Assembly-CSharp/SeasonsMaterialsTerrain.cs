using System.Threading.Tasks;
using JBooth.MicroSplat;
using Sirenix.OdinInspector;
using UnityEngine;

public class SeasonsMaterialsTerrain : SerializedMonoBehaviour, GInterface43
{
	[SerializeField]
	public MicroSplatTerrain[] _terrains;

	public static LoggerClass LoggerClass => Class443.Logger;

	public void Start()
	{
		SeasonsMaterialsFixer instance = MonoBehaviourSingleton<SeasonsMaterialsFixer>.Instance;
		if (instance != null)
		{
			instance.Add(this);
		}
		else
		{
			LoggerClass.LogError("Can't find SeasonsMaterialsFixer.Instance");
		}
	}

	public void OnDestroy()
	{
		SeasonsMaterialsFixer instance = MonoBehaviourSingleton<SeasonsMaterialsFixer>.Instance;
		if (instance != null)
		{
			instance.Remove(this);
		}
	}

	public Task LoadSummer()
	{
		LoggerClass.LogTrace("LoadSummer", this);
		MicroSplatObject.currentSeason = TextureArrayConfig.Season.Summer;
		return Task.CompletedTask;
	}

	public Task LoadWinter()
	{
		LoggerClass.LogTrace("LoadWinter", this);
		MicroSplatObject.currentSeason = TextureArrayConfig.Season.Winter;
		return Task.CompletedTask;
	}

	public Task LoadSpring()
	{
		LoggerClass.LogTrace("LoadSpring", this);
		MicroSplatObject.currentSeason = TextureArrayConfig.Season.Spring;
		return Task.CompletedTask;
	}

	public Task LoadSpringEarly()
	{
		LoggerClass.LogTrace("LoadSpringEarly", this);
		MicroSplatObject.currentSeason = TextureArrayConfig.Season.SpringEarly;
		return Task.CompletedTask;
	}

	public Task LoadAutumn()
	{
		LoggerClass.LogTrace("LoadAutumn", this);
		MicroSplatObject.currentSeason = TextureArrayConfig.Season.Autumn;
		return Task.CompletedTask;
	}

	public Task LoadAutumnLate()
	{
		LoggerClass.LogTrace("LoadAutumnLate", this);
		MicroSplatObject.currentSeason = TextureArrayConfig.Season.AutumnLate;
		return Task.CompletedTask;
	}

	public void Fix()
	{
		if (_terrains == null)
		{
			LoggerClass.LogError("_terrains is null", this);
			return;
		}
		for (int i = 0; i < _terrains.Length; i++)
		{
			MicroSplatTerrain microSplatTerrain = _terrains[i];
			LoggerClass.LogTrace("ApplySeasonMaterial to:" + microSplatTerrain.gameObject.name, this);
			microSplatTerrain.ApplySeasonMaterial();
			microSplatTerrain.Sync();
		}
		for (int j = 0; j < _terrains.Length; j++)
		{
			_terrains[j].UnloadUnusedSeasonMaterials();
		}
		Resources.UnloadUnusedAssets();
	}

	public void Unload()
	{
	}
}
