using System;
using UnityEngine;

namespace Audio.AmbientSubsystem;

public class SoundPointsManager : MonoBehaviour
{
	[SerializeField]
	private SoundPoint[] _allSoundPoints = Array.Empty<SoundPoint>();

	public void Awake()
	{
		CollectPoints();
	}

	public void CollectPoints()
	{
		_allSoundPoints = GetComponentsInChildren<SoundPoint>();
	}
}
