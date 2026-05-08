using System.Collections;
using EFT;
using UnityEngine;

namespace Cutscene;

public class CutsceneFakePlayerSteps : MonoBehaviour
{
	[SerializeField]
	private float _timeBetweenSteps;

	[SerializeField]
	private int _maxStepsCount = -1;

	private Coroutine coroutine_0;

	private Player player_0;

	private int int_0;

	public void OnDestroy()
	{
		ToggleFakeStepsSound(null, toggle: false);
	}

	public void ToggleFakeStepsSound(Player onPlayer, bool toggle)
	{
		if (toggle)
		{
			if (coroutine_0 == null)
			{
				int_0 = 0;
				player_0 = onPlayer;
				coroutine_0 = StartCoroutine(method_1());
			}
		}
		else if (coroutine_0 != null)
		{
			StopCoroutine(coroutine_0);
			coroutine_0 = null;
		}
	}

	public void method_0()
	{
		int_0++;
		if (_maxStepsCount == -1 || _maxStepsCount >= int_0)
		{
			player_0.PlayStepSound();
		}
	}

	public IEnumerator method_1()
	{
		while (true)
		{
			method_0();
			float num = 0f;
			while (num <= 1f)
			{
				num += Time.deltaTime / _timeBetweenSteps;
				yield return null;
			}
			yield return null;
		}
	}
}
