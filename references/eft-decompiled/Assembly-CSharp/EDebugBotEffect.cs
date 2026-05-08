using System;

[Flags]
public enum EDebugBotEffect : uint
{
	None = 0u,
	Stun = 1u,
	BurnEyes = 2u,
	Bleed = 4u,
	Fracture = 8u,
	Painkiller = 0x10u,
	Intoxication = 0x20u
}
