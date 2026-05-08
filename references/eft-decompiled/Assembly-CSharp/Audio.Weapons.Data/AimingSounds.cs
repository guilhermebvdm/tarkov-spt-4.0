using System;
using Audio.Data;
using EFT.Weapons.Data;

namespace Audio.Weapons.Data;

[Serializable]
public class AimingSounds : SerializableEnumDictionary<ESoundWeaponType, SoundBankWithSettings>
{
}
