using System;
using Audio.Vehicles.BTR;
using UnityEngine;

namespace Audio.Vehicles;

public class BtrTurretSoundPlayerController : MonoBehaviour
{
	[SerializeField]
	private Transform _turretTransform;

	[SerializeField]
	private VehicleRotationSoundPlayer[] _turretHorizontalRotationPlayers = Array.Empty<VehicleRotationSoundPlayer>();

	private bool bool_0;

	public void Awake()
	{
		if (!(_turretTransform == null) && _turretHorizontalRotationPlayers.Length != 0)
		{
			VehicleRotationSoundPlayer[] turretHorizontalRotationPlayers = _turretHorizontalRotationPlayers;
			foreach (VehicleRotationSoundPlayer obj in turretHorizontalRotationPlayers)
			{
				obj.transform.SetParent(_turretTransform, worldPositionStays: false);
				obj.transform.localRotation = Quaternion.identity;
				obj.transform.localPosition = Vector3.zero;
				obj.UpdateRotation(_turretTransform.localRotation.y);
			}
			bool_0 = true;
		}
		else
		{
			Debug.LogError("Btr Turret Sound Controller not initialized correct!");
		}
	}

	public void Update()
	{
		if (bool_0)
		{
			VehicleRotationSoundPlayer[] turretHorizontalRotationPlayers = _turretHorizontalRotationPlayers;
			for (int i = 0; i < turretHorizontalRotationPlayers.Length; i++)
			{
				turretHorizontalRotationPlayers[i].UpdateRotation(_turretTransform.localRotation.y);
			}
		}
	}
}
