using System;
using UnityEngine;

namespace Audio.AmbientSubsystem.AmbientSplineEmitter;

public abstract class AbstractSplineMappedEmitter : MonoBehaviour, GInterface110
{
	public abstract Vector3 EmitterPosition { get; set; }

	public abstract event Action StartEmit;

	public abstract event Action Emitted;

	public abstract event Action Stopped;

	public void Awake()
	{
		OnAwake();
	}

	public virtual void OnAwake()
	{
	}

	public abstract void Translate(Vector3 position);

	public abstract void SetSpreadRange(Vector2 spreadRange);

	public abstract void UpdateSpread(float value);

	public abstract void UpdateSpatialBlend(float value);

	public abstract void ScaleMaxDistance(float value);

	public abstract void FadeOut(float fadeOutSec);

	public abstract void FadeIn(float fadeInSec);

	public virtual void OnDestroy()
	{
	}

	public AbstractSplineMappedEmitter()
	{
	}
}
