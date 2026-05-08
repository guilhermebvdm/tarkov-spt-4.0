using Audio.AmbientSubsystem;
using UnityEngine;

namespace Audio.SpatialSystem;

public interface ISpatialAudioRoom : ISpatialRoom
{
	EAudioRoomTypeMask Type { get; }

	string Name { get; }

	BoxCollider[] Colliders { get; }

	bool IsOutdoor { get; }

	bool IsIsolated { get; }

	float RoomSize { get; }

	RoomAmbientData RoomAmbientData { get; }

	Bounds Bounds { get; }

	bool IsValid { get; }

	bool CheckIsolationRelation(ISpatialAudioRoom roomToCheck);

	void PlayRoomToneSound();

	void StopRoomToneSound();
}
