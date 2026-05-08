using System;
using Audio.Data;
using EFT.Weapons.Data;
using UnityEngine;

namespace Audio.Weapons.Data;

[Serializable]
[CreateAssetMenu(menuName = "Scriptable objects/Audio/WeaponAimingSoundsSO", fileName = "WeaponAimingSounds")]
public class WeaponAimingSoundsSO : ScriptableObject
{
	[SerializeField]
	private AimingSounds _aimingSounds = new AimingSounds();

	public bool TryGetAimingBank(ESoundWeaponType soundWeaponType, out SoundBankWithSettings bank)
	{
		return _aimingSounds.TryGetValue(soundWeaponType, out bank);
	}
}
