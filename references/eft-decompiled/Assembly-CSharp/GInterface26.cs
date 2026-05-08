using UnityEngine;

public interface GInterface26
{
	Quaternion this[int index] { get; set; }

	bool IsAlternative { get; }

	bool IsCached { get; }

	Vector3 Position { get; }

	Quaternion Rotation { get; }
}
