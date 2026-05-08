using System.Threading.Tasks;
using GPUInstancer;
using Sirenix.OdinInspector;
using UnityEngine;

public class SeasonsMaterialsSlice : SerializedMonoBehaviour, GInterface43
{
	[SerializeField]
	public SeasonsMaterialsGroup _group;

	[SerializeField]
	public GPUInstancerDetailManager _manager;

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

	public async Task LoadSummer()
	{
		if (_group != null)
		{
			await _group.LoadSummer();
			return;
		}
		LoggerClass.LogError("_group is null", this);
	}

	public async Task LoadWinter()
	{
		if (_group != null)
		{
			await _group.LoadWinter();
			return;
		}
		LoggerClass.LogError("_group is null", this);
	}

	public async Task LoadSpring()
	{
		if (_group != null)
		{
			await _group.LoadSpring();
			return;
		}
		LoggerClass.LogError("_group is null", this);
	}

	public async Task LoadSpringEarly()
	{
		if (_group != null)
		{
			await _group.LoadSpringEarly();
			return;
		}
		LoggerClass.LogError("_group is null", this);
	}

	public async Task LoadAutumn()
	{
		if (_group != null)
		{
			await _group.LoadAutumn();
			return;
		}
		LoggerClass.LogError("_group is null", this);
	}

	public async Task LoadAutumnLate()
	{
		if (_group != null)
		{
			await _group.LoadAutumnLate();
			return;
		}
		LoggerClass.LogError("_group is null", this);
	}

	public void Fix()
	{
		LoggerClass.LogTrace("SeasonsMaterialsSlice name:" + base.name + " fix begin", this);
		if (_group != null)
		{
			_group.Fix();
		}
		else
		{
			LoggerClass.LogError("_group is null", this);
		}
		if (_manager != null)
		{
			GClass1257.UpdateDetailInstances(_manager, updateMeshes: true);
			GClass1257.UpdateTerrainNormalMapDetailInstance(_manager);
			_manager.InitializeRuntimeDataAndBuffers();
			LoggerClass.LogTrace("GPUInstancerDetailManager fixed", _manager);
		}
		else
		{
			LoggerClass.LogError("GPUInstancerDetailManager is null", this);
		}
		LoggerClass.LogTrace("SeasonsMaterialsSlice name:" + base.name + " fix complete", this);
	}

	public void Unload()
	{
		if (_group != null)
		{
			_group.Unload();
			return;
		}
		LoggerClass.LogError("_group is null", this);
	}
}
