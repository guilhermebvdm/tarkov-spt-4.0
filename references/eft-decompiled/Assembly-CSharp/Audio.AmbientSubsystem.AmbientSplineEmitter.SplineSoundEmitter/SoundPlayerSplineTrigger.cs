using UnityEngine;

namespace Audio.AmbientSubsystem.AmbientSplineEmitter.SplineSoundEmitter;

[RequireComponent(typeof(BaseAmbientSoundPlayer))]
public class SoundPlayerSplineTrigger : SplineTriggerAbstract
{
	[SerializeField]
	private BaseAmbientSoundPlayer _ambientSoundPlayer;

	public override void OnAwake()
	{
		base.OnAwake();
		if (_ambientSoundPlayer == null)
		{
			_ambientSoundPlayer = GetComponent<BaseAmbientSoundPlayer>();
		}
	}

	public override void OnTriggered()
	{
		_ambientSoundPlayer.Play();
	}
}
