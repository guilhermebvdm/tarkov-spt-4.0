using System;
using System.Collections;
using MultiFlare;
using UnityEngine;

public class GClass1031
{
	[NonSerialized]
	public MonoBehaviour MonoBehaviour_0;

	[NonSerialized]
	public FlareSettings FlareSettings_0;

	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public float Float_1;

	[NonSerialized]
	public Coroutine Coroutine_0;

	public GClass1031(MonoBehaviour owner, FlareSettings settings, float initValue)
	{
		MonoBehaviour_0 = owner;
		FlareSettings_0 = settings;
		if (!GClass855.IsZero(initValue))
		{
			Float_0 = initValue;
			FlareSettings_0.SetBlindnessProtectionFactor(initValue);
		}
	}

	public void Enable(float value)
	{
		method_1();
		Float_1 = value;
		Coroutine_0 = MonoBehaviour_0.StartCoroutine(method_0());
	}

	public void Disable()
	{
		method_1();
		Float_1 = 0f;
		Coroutine_0 = MonoBehaviour_0.StartCoroutine(method_0());
	}

	public IEnumerator method_0()
	{
		yield return new WaitForSeconds(0.6f);
		while (!Mathf.Approximately(Float_0, Float_1))
		{
			Float_0 = Mathf.MoveTowards(Float_0, Float_1, Time.deltaTime * 4f);
			FlareSettings_0.SetBlindnessProtectionFactor(Float_0);
			yield return null;
		}
		Coroutine_0 = null;
	}

	public void method_1()
	{
		if (Coroutine_0 != null)
		{
			MonoBehaviour_0.StopCoroutine(Coroutine_0);
		}
	}
}
