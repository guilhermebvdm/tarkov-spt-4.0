using System;
using Audio.SpatialSystem;
using UnityEngine;

public abstract class GClass1128 : GInterface85, IDisposable
{
	[NonSerialized]
	public const float Float_0 = 1f;

	[NonSerialized]
	public const float Float_1 = 0f;

	[NonSerialized]
	public SpatialAudioSystem SpatialAudioSystem_0;

	[NonSerialized]
	public bool Bool_0;

	public GClass1128(SpatialAudioSystem audioSystem)
	{
		SpatialAudioSystem_0 = audioSystem;
	}

	public void ManualUpdate(SourceContainerClass sourceContainer, Vector3 listenerPosition, bool forceUpdate = false)
	{
		if (!Bool_0)
		{
			OnUpdate(sourceContainer, listenerPosition, forceUpdate);
		}
	}

	public void ManualLateUpdate()
	{
		if (!Bool_0)
		{
			OnLateUpdate();
		}
	}

	public abstract float GetScore();

	public virtual float CalculateScore(Vector3 listenerPosition, SourceContainerClass sourceContainer)
	{
		return 0f;
	}

	public void WarmUp()
	{
		OnWarmUp();
	}

	public virtual void OnWarmUp()
	{
	}

	public virtual void OnUpdate(SourceContainerClass sourceContainer, Vector3 listenerPosition, bool forceUpdate = false)
	{
	}

	public virtual void OnLateUpdate()
	{
	}

	public void Dispose()
	{
		if (!Bool_0)
		{
			Bool_0 = true;
			OnDispose();
		}
	}

	public void Reset()
	{
		Bool_0 = false;
		OnReset();
	}

	public virtual void OnReset()
	{
	}

	public virtual void OnDispose()
	{
	}

	~GClass1128()
	{
		Dispose();
	}
}
