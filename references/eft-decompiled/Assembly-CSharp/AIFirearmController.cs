using EFT;
using UnityEngine;

public class AIFirearmController : Player.FirearmController
{
	public override Vector3 WeaponDirection => _player.LookDirection;
}
