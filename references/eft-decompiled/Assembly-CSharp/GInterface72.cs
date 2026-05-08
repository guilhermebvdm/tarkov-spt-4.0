using UnityEngine;

public interface GInterface72
{
	bool IsPositionTracked { get; }

	Vector3 Position { get; }

	void StartTracking(Transform tracker, Vector3 positionOffset = default(Vector3));

	void StopTracking();

	void UpdatePosition();
}
