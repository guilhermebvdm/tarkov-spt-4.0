using System;
using Audio.Data;
using EFT;
using UnityEngine;

namespace Audio.AmbientSubsystem.Data;

[Serializable]
[CreateAssetMenu(menuName = "Scriptable objects/Audio/SeasonSurfaceSetContainer", fileName = "SeasonSurfaceSetContainer")]
public class SeasonSurfaceSetContainer : SoundContainerBase<ESeasonStatus, SurfaceSet>
{
	[Serializable]
	public class SeasonMovementSounds : SerializableEnumDictionary<ESeasonStatus, SurfaceSet>
	{
	}

	[SerializeField]
	private SeasonMovementSounds SeasonPlayerMovementSounds;

	public override bool TryGetContent(ESeasonStatus soundType, out SurfaceSet set)
	{
		return SeasonPlayerMovementSounds.TryGetValue(soundType, out set);
	}
}
