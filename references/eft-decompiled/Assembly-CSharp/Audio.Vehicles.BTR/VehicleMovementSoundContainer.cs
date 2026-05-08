using System;
using UnityEngine;

namespace Audio.Vehicles.BTR;

[Serializable]
[CreateAssetMenu(menuName = "Scriptable objects/Audio/VehicleMovementSoundContainer", fileName = "VehicleMovementSoundContainer")]
public class VehicleMovementSoundContainer : ScriptableObject
{
	public AudioClip EngineStartClip;

	public AudioClip EngineIdleAccelClip;

	public AudioClip EngineLowAccelClip;

	public AudioClip EngineHighAccelClip;

	public AudioClip LowRpmLoopClip;

	public AudioClip HighRpmLoopClip;

	public AudioClip LowSpeedMovementLoopClip;

	public AudioClip HighSpeedMovementLoopClip;

	public AudioClip MovementStopBreakClip;

	public AudioClip[] SkidClips;

	public AudioClip[] SuspensionImpactClips;
}
