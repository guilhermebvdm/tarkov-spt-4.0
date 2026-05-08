using System;
using Audio.AudioCulling;
using UnityEngine;

public interface GInterface94
{
	EAudibleState CurrentAudibleState { get; }

	Vector3 CurrentPosition { get; }

	float SqrMaxDistance { get; }

	event Action<EAudibleState> AudibleStateChanged;

	void ChangeAudibleState(EAudibleState state);
}
