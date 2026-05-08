using System;

namespace CommonAssets.Scripts.Audio;

[Serializable]
public enum EAudioMovementState : byte
{
	None,
	Run,
	Sprint,
	Stop,
	Land,
	Turn,
	Duck,
	Prone,
	Drop,
	Jump,
	Gear,
	VaultLand,
	VaultGeneric,
	Search
}
