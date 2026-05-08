using EFT;
using UnityEngine;

public interface GInterface14
{
	Vector3 Position { get; }

	bool HaveLookSide { get; }

	int LookIndexes { get; }

	Vector3 LookDir(int index);

	void SetOwner(BotOwner owner);

	int GetOwnerId();
}
