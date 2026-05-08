using UnityEngine;

public class MuzzleParticlePivot : MuzzleEffect
{
	[SerializeField]
	private EMuzzleParticlePivot _pivot;

	public void Play(GInterface53 system)
	{
		system.Play(_pivot, base.transform);
	}
}
