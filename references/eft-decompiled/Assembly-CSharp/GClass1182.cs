using System;
using System.Collections;
using System.Collections.Generic;
using Audio.AudioCulling;
using EFT;
using UnityEngine;

public class GClass1182 : GInterface93, IDisposable
{
	[NonSerialized]
	public int Int_0;

	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public Transform Transform_0;

	[NonSerialized]
	public List<GInterface94> List_0 = new List<GInterface94>();

	[NonSerialized]
	public Coroutine Coroutine_0;

	[NonSerialized]
	public bool Bool_0;

	public GClass1182(Transform listenerTransform, int maxUpdatePerFrame = 10, float cullDistanceOffset = 25f)
	{
		Transform_0 = listenerTransform;
		Float_0 = cullDistanceOffset * cullDistanceOffset;
		Int_0 = maxUpdatePerFrame;
	}

	public void Register(GInterface94 cullingObject)
	{
		if (List_0.Contains(cullingObject))
		{
			Debug.LogWarning("Object cullingObject already registered!");
			return;
		}
		List_0.Add(cullingObject);
		if (!Bool_0)
		{
			method_0();
		}
	}

	public void Unregister(GInterface94 cullingObject)
	{
		List_0.Remove(cullingObject);
	}

	public void method_0()
	{
		StaticManager instance = StaticManager.Instance;
		if (instance == null)
		{
			Debug.LogError("Can't start work coroutine, StaticManager is null!");
			return;
		}
		Bool_0 = true;
		GClass7.TryStopCoroutine(instance, ref Coroutine_0);
		Coroutine_0 = instance.StartCoroutine(method_1());
	}

	public IEnumerator method_1()
	{
		int num = 0;
		while (List_0.Count > 0)
		{
			for (int i = 0; i < List_0.Count; i++)
			{
				GInterface94 gInterface = List_0[i];
				bool flag = Vector3.SqrMagnitude(gInterface.CurrentPosition - Transform_0.position) <= gInterface.SqrMaxDistance + Float_0;
				gInterface.ChangeAudibleState(flag ? EAudibleState.Unmute : EAudibleState.Mute);
				num++;
				if (num > Int_0)
				{
					num = 0;
					yield return null;
				}
			}
			yield return null;
		}
		Bool_0 = false;
	}

	public void method_2()
	{
		Bool_0 = false;
		StaticManager instance = StaticManager.Instance;
		if (!(instance == null))
		{
			GClass7.TryStopCoroutine(instance, ref Coroutine_0);
		}
	}

	public void Dispose()
	{
		method_2();
	}
}
