using System;
using System.Threading.Tasks;
using GPUInstancer;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "Seasons/SeasonsMaterialsGrass")]
public class SeasonsMaterialsGrass : ScriptableObject, GInterface43
{
	public GPUInstancerDetailPrototype Prototype;

	public SeasonsGrass SeasonsGrass;

	public static LoggerClass Logger => Class443.Logger;

	public virtual Task LoadSummer()
	{
		return SeasonsGrass.LoadSummer();
	}

	public virtual Task LoadWinter()
	{
		return SeasonsGrass.LoadWinter();
	}

	public virtual Task LoadSpring()
	{
		return SeasonsGrass.LoadSpring();
	}

	public virtual Task LoadSpringEarly()
	{
		return SeasonsGrass.LoadSpringEarly();
	}

	public virtual Task LoadAutumn()
	{
		return SeasonsGrass.LoadAutumn();
	}

	public virtual Task LoadAutumnLate()
	{
		return SeasonsGrass.LoadAutumnLate();
	}

	public virtual void Fix()
	{
		SeasonsGrass.Fix(Prototype);
	}

	public virtual void Unload()
	{
		SeasonsGrass.Unload();
	}
}
