using System.Collections;
using UnityEngine;

public abstract class HalloweenEffect : MonoBehaviour
{
	private IEnumerator ienumerator_0;

	public void Process()
	{
		Abort();
		ienumerator_0 = RunProcess();
		StartCoroutine(ienumerator_0);
	}

	public abstract IEnumerator RunProcess();

	public void Abort()
	{
		if (ienumerator_0 != null)
		{
			StopCoroutine(ienumerator_0);
			ienumerator_0 = null;
		}
	}

	public HalloweenEffect()
	{
	}
}
