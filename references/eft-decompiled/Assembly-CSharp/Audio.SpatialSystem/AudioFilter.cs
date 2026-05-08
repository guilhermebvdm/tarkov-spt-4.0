using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

namespace Audio.SpatialSystem;

public abstract class AudioFilter : MonoBehaviour, GInterface69
{
	[CompilerGenerated]
	private float float_0;

	[CompilerGenerated]
	private bool bool_0;

	protected bool _isActive;

	public float FilterLevel
	{
		[CompilerGenerated]
		get
		{
			return float_0;
		}
		[CompilerGenerated]
		set
		{
			float_0 = value;
		}
	}

	public bool Initialized
	{
		[CompilerGenerated]
		get
		{
			return bool_0;
		}
		[CompilerGenerated]
		set
		{
			bool_0 = value;
		}
	}

	public abstract void ResetFilter();

	public abstract void SetFilterParams(float value, bool applyImmediately = false, ESoundOcclusionType occlusionType = ESoundOcclusionType.FullOcclusion);

	public abstract void SetResonance(float value);

	public abstract void CalculateFrequency(float dt);

	public virtual void ManualUpdate()
	{
		if (_isActive && base.enabled)
		{
			CalculateFrequency(Time.deltaTime);
		}
	}

	public void Awake()
	{
		OnAwake();
		ResetFilter();
		Initialized = true;
	}

	public virtual void SetActive(bool active)
	{
		if (_isActive != active)
		{
			_isActive = active;
			if (_isActive)
			{
				Enable();
			}
			else
			{
				Disable();
			}
		}
	}

	public virtual void OnAwake()
	{
	}

	public virtual void Enable()
	{
		Subscribe();
	}

	public virtual void Subscribe()
	{
		StaticManager instance = StaticManager.Instance;
		if (instance != null)
		{
			instance.StaticUpdate += ManualUpdate;
		}
	}

	public virtual void Unsubscribe()
	{
		StaticManager instance = StaticManager.Instance;
		if (instance != null)
		{
			instance.StaticUpdate -= ManualUpdate;
		}
	}

	public virtual void Disable()
	{
		ResetFilter();
		Unsubscribe();
	}

	public virtual void OnDestroy()
	{
		ResetFilter();
		Unsubscribe();
	}

	public AudioFilter()
	{
	}
}
