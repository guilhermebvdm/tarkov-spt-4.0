using System;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "Seasons/SeasonsMaterialsGrassStub")]
public class SeasonsMaterialsGrassStub : SeasonsMaterialsGrass
{
	public override Task LoadSummer()
	{
		return Task.CompletedTask;
	}

	public override Task LoadWinter()
	{
		return Task.CompletedTask;
	}

	public override Task LoadSpringEarly()
	{
		return Task.CompletedTask;
	}

	public override Task LoadSpring()
	{
		return Task.CompletedTask;
	}

	public override Task LoadAutumn()
	{
		return Task.CompletedTask;
	}

	public override Task LoadAutumnLate()
	{
		return Task.CompletedTask;
	}

	public override void Fix()
	{
	}

	public override void Unload()
	{
	}
}
