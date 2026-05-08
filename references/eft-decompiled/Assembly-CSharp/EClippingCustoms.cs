using System;

[Flags]
public enum EClippingCustoms : byte
{
	FaceCover = 1,
	Visor = 2,
	Hat = 4,
	Headphones = 8,
	Helmet = 0x10
}
