using System;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "Seasons/SeasonsMaterial")]
public class SeasonsMaterial : SerializedScriptableObject, GInterface43
{
	[OdinSerialize]
	private GInterface45 _summerMaterial;

	[OdinSerialize]
	private GInterface41 _autumnMaterial;

	[OdinSerialize]
	private GInterface41 _autumnLateMaterial;

	[OdinSerialize]
	private GInterface46 _winterMaterial;

	[OdinSerialize]
	private GInterface44 _springMaterial;

	[OdinSerialize]
	private GInterface44 _springEarlyMaterial;

	private GInterface42 _loadedMaterial;

	public static LoggerClass Logger => Class443.Logger;

	public Task LoadSummer()
	{
		_loadedMaterial = _summerMaterial;
		return Task.CompletedTask;
	}

	public Task LoadAutumn()
	{
		_loadedMaterial = _autumnMaterial;
		return Task.CompletedTask;
	}

	public Task LoadAutumnLate()
	{
		_loadedMaterial = _autumnLateMaterial;
		return Task.CompletedTask;
	}

	public Task LoadWinter()
	{
		_loadedMaterial = _winterMaterial;
		return Task.CompletedTask;
	}

	public Task LoadSpring()
	{
		_loadedMaterial = _springMaterial;
		return Task.CompletedTask;
	}

	public Task LoadSpringEarly()
	{
		_loadedMaterial = _springEarlyMaterial;
		return Task.CompletedTask;
	}

	public void Fix()
	{
		if (_loadedMaterial != null)
		{
			_loadedMaterial.Fix();
			return;
		}
		Logger.LogError("SeasonsMaterial name:" + base.name + " _loadedMaterial is null", this);
	}

	public void Unload()
	{
		_loadedMaterial = null;
	}
}
