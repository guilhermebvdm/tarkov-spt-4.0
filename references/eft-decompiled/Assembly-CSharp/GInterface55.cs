using UnityEngine;

public interface GInterface55
{
	float Radius { get; }

	Vector3 Position { get; }

	Vector3 ClearTransformPosition { get; }

	Vector3 SafeMultithreadedPosition { get; }

	bool IsAutocullVisible { get; }

	int Index { get; }

	float CullDistanceSqr { get; }

	bool CullByDistanceOnly { get; set; }

	float SqrCameraDistance { get; set; }

	bool IsVisible { get; }

	bool IsVisibleByCamera { get; set; }

	void CustomUpdate();

	void Register();

	void SetVisibility(bool isVisible);
}
