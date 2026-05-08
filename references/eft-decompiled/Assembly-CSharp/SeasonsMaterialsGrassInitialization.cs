using System.Threading.Tasks;
using GPUInstancer;
using UnityEngine;

public class SeasonsMaterialsGrassInitialization : MonoBehaviour, GInterface43
{
	private ESeason eseason_0;

	public GrassInitialization GrassInitialization;

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
		eseason_0 = ESeason.Summer;
		return Task.CompletedTask;
	}

	public Task LoadWinter()
	{
		eseason_0 = ESeason.Winter;
		return Task.CompletedTask;
	}

	public Task LoadSpring()
	{
		eseason_0 = ESeason.Spring;
		return Task.CompletedTask;
	}

	public Task LoadSpringEarly()
	{
		eseason_0 = ESeason.SpringEarly;
		return Task.CompletedTask;
	}

	public Task LoadAutumn()
	{
		eseason_0 = ESeason.Autumn;
		return Task.CompletedTask;
	}

	public Task LoadAutumnLate()
	{
		eseason_0 = ESeason.AutumnLate;
		return Task.CompletedTask;
	}

	public void Fix()
	{
		if (GrassInitialization != null)
		{
			GrassInitialization.method_0(eseason_0);
			return;
		}
		LoggerClass.LogError("Can't find GrassInitialization", this);
	}

	public void Unload()
	{
	}
}
