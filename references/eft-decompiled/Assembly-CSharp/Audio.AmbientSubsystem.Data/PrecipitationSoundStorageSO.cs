using System;
using UnityEngine;

namespace Audio.AmbientSubsystem.Data;

[Serializable]
[CreateAssetMenu(menuName = "Scriptable objects/Audio/PrecipitationSoundStorageSO", fileName = "PrecipitationSoundStorageSO")]
public class PrecipitationSoundStorageSO : ScriptableObject, ISeasonSoundStorage<RainController.ERainIntensity>
{
	[SerializeField]
	private PrecipitationSoundStorage _storage = new PrecipitationSoundStorage();

	public bool TryGetClip(ESeasonStatus seasonStatus, RainController.ERainIntensity type, EnvironmentType environment, out AudioClip clip)
	{
		return _storage.TryGetClip(seasonStatus, type, environment, out clip);
	}
}
