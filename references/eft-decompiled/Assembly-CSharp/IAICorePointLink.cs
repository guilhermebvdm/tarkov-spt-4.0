using UnityEngine;

public interface IAICorePointLink
{
	AICorePoint CorePointInGame { get; }

	Vector3 Position { get; }
}
