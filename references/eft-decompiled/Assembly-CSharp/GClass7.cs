using System;
using System.Collections;
using UnityEngine;

public abstract class GClass7
{
	public static IEnumerator StartBehaviourTimer(this MonoBehaviour monoBehaviour, float delay, Action action)
	{
		return StartBehaviourTimer(monoBehaviour, delay, repeatable: false, ignoreTimeScale: false, action);
	}

	public static IEnumerator StartBehaviourTimer(this MonoBehaviour monoBehaviour, float? delay, Action action)
	{
		return StartBehaviourTimer(monoBehaviour, delay, repeatable: false, ignoreTimeScale: false, action);
	}

	public static IEnumerator StartBehaviourTimer(this MonoBehaviour monoBehaviour, float delay, bool repeatable, Action action)
	{
		return StartBehaviourTimer(monoBehaviour, delay, repeatable, ignoreTimeScale: false, action);
	}

	public static IEnumerator StartBehaviourTimer(this MonoBehaviour monoBehaviour, float? delay, bool repeatable, bool ignoreTimeScale, Action action)
	{
		IEnumerator enumerator = TimerBehaviourCoroutine(monoBehaviour, delay, repeatable, ignoreTimeScale, action);
		monoBehaviour.StartCoroutine(enumerator);
		return enumerator;
	}

	public static void StopBehaviourTimer(this MonoBehaviour monoBehaviour, ref IEnumerator timerCoroutine)
	{
		if (timerCoroutine != null)
		{
			if (monoBehaviour != null)
			{
				monoBehaviour.StopCoroutine(timerCoroutine);
			}
			timerCoroutine = null;
		}
	}

	public static void TryStopCoroutine(this MonoBehaviour monoBehaviour, ref Coroutine coroutine)
	{
		if (coroutine != null)
		{
			if (monoBehaviour != null)
			{
				monoBehaviour.StopCoroutine(coroutine);
			}
			coroutine = null;
		}
	}

	public static IEnumerator TimerBehaviourCoroutine(this MonoBehaviour monoBehaviour, float? delay, bool repeatable, bool ignoreTimeScale, Action action)
	{
		do
		{
			if (ignoreTimeScale)
			{
				float realtimeSinceStartup = Time.realtimeSinceStartup;
				while (Time.realtimeSinceStartup < realtimeSinceStartup + delay)
				{
					yield return null;
				}
			}
			else if (!delay.HasValue)
			{
				yield return null;
			}
			else
			{
				yield return new WaitForSeconds(delay.Value);
			}
			action();
		}
		while (repeatable);
	}

	public static bool IsNullOrDestroyed(this MonoBehaviour monoBehaviour)
	{
		if (!(monoBehaviour == null))
		{
			return monoBehaviour == null;
		}
		return true;
	}
}
