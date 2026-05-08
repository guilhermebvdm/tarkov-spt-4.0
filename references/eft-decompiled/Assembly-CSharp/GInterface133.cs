using System.Collections.Generic;
using UnityEngine;

public interface GInterface133
{
	string Name { get; }

	int ParameterXHash { get; }

	int ParameterYHash { get; }

	float MaxClipTime { get; }

	bool IsLooping { get; }

	Vector3 PosMotion { get; }

	Quaternion PosRotation { get; }

	Vector3 ComputeRootPosition(float deltaTime, float stateNormalizedTime, GInterface134 parametersCache);

	Quaternion ComputeDeltaRotation(float deltaTime, GInterface134 parametersCache);

	float ComputeClipTime(GInterface134 parametersCache);

	void ComputeActiveClips(List<GStruct141> nodes, GInterface134 parametersCache);
}
