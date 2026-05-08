using System;
using UnityEngine;

namespace Audio.AmbientSubsystem.Data;

public interface ISeasonSoundStorage<in T> where T : Enum
{
	bool TryGetClip(ESeasonStatus seasonStatus, T type, EnvironmentType environment, out AudioClip clip);
}
