using UnityEngine;

namespace Audio.AmbientSubsystem.AmbientSplineEmitter.SplineSoundEmitter;

public abstract class SplineTriggerAbstract : MonoBehaviour
{
	[Range(0f, 100f)]
	[SerializeField]
	protected int _triggerChance = 100;

	[Range(0f, 600f)]
	[SerializeField]
	protected float _cooldown = 1f;

	[SerializeField]
	private bool _triggerOnce;

	private float float_0;

	private bool bool_0;

	public void Awake()
	{
		OnAwake();
	}

	public virtual void OnAwake()
	{
	}

	public void PushTrigger()
	{
		if ((!_triggerOnce || !bool_0) && !(Time.realtimeSinceStartup < float_0) && Random.Range(0, 100) <= _triggerChance)
		{
			OnTriggered();
			float_0 = Time.realtimeSinceStartup + _cooldown;
			if (_triggerOnce)
			{
				bool_0 = true;
			}
		}
	}

	public abstract void OnTriggered();

	public SplineTriggerAbstract()
	{
	}
}
