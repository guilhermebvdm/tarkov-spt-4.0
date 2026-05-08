using System;
using System.Collections.Generic;
using Audio.SpatialSystem;
using UnityEngine;

public class GClass1131 : GClass1130
{
	[NonSerialized]
	public List<SoundRay> List_0 = new List<SoundRay>(11);

	[NonSerialized]
	public GClass1136 Gclass1136_0;

	[NonSerialized]
	public Vector3 Vector3_0 = new Vector3(0f, 0.5f, 0f);

	[NonSerialized]
	public float Float_2;

	[NonSerialized]
	public bool Bool_1;

	public GClass1131(SpatialAudioSystem audioSystem)
		: base(audioSystem)
	{
		Float_2 = 0f;
		QueryParameters queryParameters = new QueryParameters
		{
			layerMask = GClass1130.Int32_0,
			hitTriggers = QueryTriggerInteraction.Ignore
		};
		Gclass1136_0 = new GClass1136(11, 1, queryParameters);
	}

	public override void OnUpdate(SourceContainerClass sourceContainer, Vector3 listenerPosition, bool forceUpdate = false)
	{
		if (Gclass1136_0.IsCompleted)
		{
			Vector3 currentPosition = sourceContainer.CurrentPosition;
			if (Vector3.SqrMagnitude(currentPosition - listenerPosition) > sourceContainer.SqrMaxDistance)
			{
				Float_2 = 1f;
				return;
			}
			Bool_1 = GClass1162.IsNeedVerticalOcclusion(currentPosition, listenerPosition, out var _);
			method_2(currentPosition, listenerPosition);
			method_1();
			method_3();
			base.OnUpdate(sourceContainer, listenerPosition, forceUpdate);
		}
	}

	public override float GetScore()
	{
		return Mathf.Clamp01((float)Math.Round(Float_2, 1, MidpointRounding.AwayFromZero));
	}

	public void method_1()
	{
		if (List_0.Count != 0)
		{
			for (int i = 0; i < List_0.Count; i++)
			{
				SoundRay soundRay = List_0[i];
				Vector3 normalized = (soundRay.EndPoint - soundRay.StartPoint).normalized;
				Gclass1136_0.AddRaycastCommand(soundRay.StartPoint, normalized, soundRay.Distance, i);
			}
		}
	}

	public void method_2(Vector3 soundPos, Vector3 listenerPos)
	{
		List_0.Clear();
		Vector3 startPoint = GClass1130.smethod_0(soundPos, listenerPos, 1f, positive: true);
		Vector3 startPoint2 = GClass1130.smethod_0(soundPos, listenerPos, 1f, positive: false);
		Vector3 endPoint = GClass1130.smethod_0(listenerPos, soundPos, 1f, positive: true);
		Vector3 endPoint2 = GClass1130.smethod_0(listenerPos, soundPos, 1f, positive: false);
		Vector3 startPoint3 = soundPos + Vector3_0;
		Vector3 endPoint3 = listenerPos + Vector3_0;
		Vector3 startPoint4 = soundPos - Vector3_0;
		Vector3 endPoint4 = listenerPos - Vector3_0;
		List_0.Add(new SoundRay(startPoint, listenerPos));
		List_0.Add(new SoundRay(soundPos, listenerPos));
		List_0.Add(new SoundRay(startPoint2, listenerPos));
		List_0.Add(new SoundRay(startPoint, endPoint));
		List_0.Add(new SoundRay(startPoint, endPoint2));
		List_0.Add(new SoundRay(soundPos, endPoint));
		List_0.Add(new SoundRay(soundPos, endPoint2));
		List_0.Add(new SoundRay(startPoint2, endPoint));
		List_0.Add(new SoundRay(startPoint2, endPoint2));
		List_0.Add(new SoundRay(startPoint3, endPoint3));
		List_0.Add(new SoundRay(startPoint4, endPoint4));
	}

	public override void OnLateUpdate()
	{
		Gclass1136_0.StoreResults();
		if (!Gclass1136_0.IsCompleted)
		{
			method_0();
			return;
		}
		method_4();
		method_0();
		base.OnLateUpdate();
	}

	public void method_3()
	{
		Gclass1136_0.Execute();
	}

	public void method_4()
	{
		if (Gclass1136_0.HitsData.Count == 0 || List_0.Count == 0 || Gclass1136_0.HitsData.Count < List_0.Count)
		{
			return;
		}
		Float_2 = 0f;
		for (int i = 0; i < List_0.Count; i++)
		{
			_ = List_0[i];
			RaycastHit[] array = Gclass1136_0.HitsData[i];
			if ((object)array[0].collider != null)
			{
				float num = method_5(array);
				Float_2 += num;
			}
		}
		Float_2 /= 11f;
	}

	public float method_5(RaycastHit[] hits)
	{
		return Mathf.Clamp(GClass950.FindObstaclesEffect(hits, Bool_1), 0f, 1f);
	}

	public override void OnDispose()
	{
		Gclass1136_0.Dispose();
		Gclass1136_0.ClearHitData();
		base.OnDispose();
	}
}
