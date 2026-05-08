using Audio.Vehicles.BTR;
using UnityEngine;
using UnityEngine.Audio;

namespace Audio.Vehicles;

public class SoundSuspensionController : MonoBehaviour, GInterface76
{
	private const float float_0 = 10000f;

	[SerializeField]
	private float _volumeMult = 0.9f;

	[SerializeField]
	private float _playingTimeThreshold = 0.1f;

	[SerializeField]
	private SoundSuspensionImpactSoundPlayer[] _impactSoundPlayers;

	[SerializeField]
	private VehicleMovementSoundContainer _indoorSoundContainer;

	[SerializeField]
	private VehicleMovementSoundContainer _outdoorSoundContainer;

	[SerializeField]
	private Vector2 _randomPitchRange = new Vector2(0.8f, 1.1f);

	private EnvironmentType environmentType_0;

	private AudioMixerGroup audioMixerGroup_0;

	public float PlayingTimeThresholdSeconds => _playingTimeThreshold;

	public void Awake()
	{
		SoundSuspensionImpactSoundPlayer[] impactSoundPlayers = _impactSoundPlayers;
		for (int i = 0; i < impactSoundPlayers.Length; i++)
		{
			impactSoundPlayers[i].Init(this);
		}
		if (MonoBehaviourSingleton<BetterAudio>.Instantiated)
		{
			audioMixerGroup_0 = MonoBehaviourSingleton<BetterAudio>.Instance.VehicleOutMixer;
		}
	}

	public void SetEnvironment(EnvironmentType environment)
	{
		environmentType_0 = environment;
		audioMixerGroup_0 = method_0();
	}

	public AudioClip GetImpactClip()
	{
		if (environmentType_0 != EnvironmentType.Indoor)
		{
			return _outdoorSoundContainer.SuspensionImpactClips[Random.Range(0, _outdoorSoundContainer.SuspensionImpactClips.Length - 1)];
		}
		return _indoorSoundContainer.SuspensionImpactClips[Random.Range(0, _indoorSoundContainer.SuspensionImpactClips.Length - 1)];
	}

	public float CalculateVolume(float forceMagnitude)
	{
		return Mathf.Clamp01(forceMagnitude / 10000f) * _volumeMult;
	}

	public float GetRandomPitch()
	{
		return Random.Range(_randomPitchRange.x, _randomPitchRange.y);
	}

	public bool IsOutOfRange(Vector3 position, int sqrRolloff)
	{
		if (!CameraClass.Exist)
		{
			return true;
		}
		return CameraClass.Instance.SqrDistance(position) > (float)sqrRolloff;
	}

	public void UpdateImpactPlayers()
	{
		SoundSuspensionImpactSoundPlayer[] impactSoundPlayers = _impactSoundPlayers;
		for (int i = 0; i < impactSoundPlayers.Length; i++)
		{
			impactSoundPlayers[i].TryPlayImpactSound(audioMixerGroup_0);
		}
	}

	public AudioMixerGroup method_0()
	{
		if (environmentType_0 != EnvironmentType.Outdoor)
		{
			return MonoBehaviourSingleton<BetterAudio>.Instance.VehicleInMixer;
		}
		return MonoBehaviourSingleton<BetterAudio>.Instance.VehicleOutMixer;
	}
}
