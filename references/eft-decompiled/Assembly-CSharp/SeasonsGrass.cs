using System;
using System.Threading.Tasks;
using Comfort.Common;
using EFT;
using GPUInstancer;
using JetBrains.Annotations;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "Seasons/SeasonsGrass")]
public class SeasonsGrass : ScriptableObject
{
	[SerializeField]
	private SeasonGrass SummerGrass;

	[SerializeField]
	private SeasonGrass AutumnGrass;

	[SerializeField]
	private SeasonGrass AutumnLateGrass;

	[SerializeField]
	private SeasonGrass SpringGrass;

	[SerializeField]
	private SeasonGrass SpringEarlyGrass;

	[SerializeField]
	private SeasonGrass WinterGrass;

	private bool _loaded;

	private ESeason _season;

	private Texture2D _texture;

	private Texture2D _bump;

	private Color _detailHealthyColor;

	private Color _detailDryColor;

	private Color _windWaveTintColor;

	public static LoggerClass Logger => Class443.Logger;

	public void Fix(GPUInstancerDetailPrototype prototype)
	{
		if (_season == ESeason.Winter)
		{
			prototype.detailDryColor = _detailDryColor;
			prototype.detailHealthyColor = _detailHealthyColor;
			prototype.windWaveTintColor = _windWaveTintColor;
		}
		else if (_season == ESeason.Spring)
		{
			prototype.detailDryColor = _detailDryColor;
			prototype.detailHealthyColor = _detailHealthyColor;
		}
		else if (_season == ESeason.SpringEarly)
		{
			prototype.detailDryColor = _detailDryColor;
			prototype.detailHealthyColor = _detailHealthyColor;
		}
		else if (_season == ESeason.Autumn)
		{
			prototype.detailDryColor = _detailDryColor;
			prototype.detailHealthyColor = _detailHealthyColor;
		}
		else if (_season == ESeason.AutumnLate)
		{
			prototype.detailDryColor = _detailDryColor;
			prototype.detailHealthyColor = _detailHealthyColor;
		}
		if (_texture != null)
		{
			prototype.prototypeTexture = _texture;
		}
		else
		{
			Logger.LogWarn("Texture is null", this);
		}
		if (_bump != null)
		{
			prototype.bumpMap = _bump;
		}
		else
		{
			Logger.LogWarn("BumpMap is null", this);
		}
		Logger.LogTrace("SeasonsGrass fixed", this);
	}

	public Task LoadSummer()
	{
		_season = ESeason.Summer;
		return Load(in SummerGrass);
	}

	public Task LoadWinter()
	{
		_season = ESeason.Winter;
		return Load(in WinterGrass);
	}

	public Task LoadSpring()
	{
		_season = ESeason.Spring;
		return Load(in SpringGrass);
	}

	public Task LoadSpringEarly()
	{
		_season = ESeason.SpringEarly;
		return Load(in SpringEarlyGrass);
	}

	public Task LoadAutumn()
	{
		_season = ESeason.Autumn;
		return Load(in AutumnGrass);
	}

	public Task LoadAutumnLate()
	{
		_season = ESeason.AutumnLate;
		return Load(in AutumnLateGrass);
	}

	public void Unload()
	{
		_texture = null;
		_bump = null;
	}

	public Task Load(in SeasonGrass seasonGrass)
	{
		if (_loaded)
		{
			return Task.CompletedTask;
		}
		_loaded = true;
		return LoadAsync(seasonGrass);
	}

	public async Task LoadAsync(SeasonGrass seasonGrass)
	{
		_detailHealthyColor = seasonGrass.DetailHealthyColor;
		_detailDryColor = seasonGrass.DetailDryColor;
		_windWaveTintColor = seasonGrass.WindWaveTintColor;
		Task<Texture2D> task = ((!string.IsNullOrEmpty(seasonGrass.TextureKey.path)) ? Load(seasonGrass.TextureKey) : null);
		Task<Texture2D> bumpTask = ((!string.IsNullOrEmpty(seasonGrass.BumpKey.path)) ? Load(seasonGrass.BumpKey) : null);
		if (task != null)
		{
			_texture = await task;
		}
		if (bumpTask != null)
		{
			_bump = await bumpTask;
		}
	}

	[ItemCanBeNull]
	public async Task<Texture2D> Load(ResourceKey resourceKey)
	{
		IEasyAssets assets = Singleton<IEasyAssets>.Instance;
		await GClass1857.LoadBundles(GClass1857.Retain(assets, new string[1] { resourceKey.path }));
		return GClass1857.GetAsset<Texture2D>(assets, resourceKey);
	}
}
