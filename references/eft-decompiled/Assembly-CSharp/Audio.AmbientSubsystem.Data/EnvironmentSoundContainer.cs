using System;
using UnityEngine;

namespace Audio.AmbientSubsystem.Data;

[Serializable]
public class EnvironmentSoundContainer
{
	[SerializeField]
	public AudioClip _indoorClip;

	[SerializeField]
	public AudioClip _outdoorClip;

	public AudioClip GetClip(EnvironmentType environment)
	{
		if (environment != EnvironmentType.Indoor)
		{
			return _outdoorClip;
		}
		return _indoorClip;
	}
}
