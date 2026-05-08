using System.Collections.Generic;
using Comfort.Common;
using EFT;
using EFT.CameraControl;
using UnityEngine;

public class BulletSoundPlayersController : MonoBehaviour
{
	[SerializeField]
	private List<BulletSoundPlayer> _bulletSoundPlayers;

	private Camera camera_0;

	private PlayerCameraController playerCameraController_0;

	public void Awake()
	{
		PlayerCameraController.OnPlayerCameraControllerCreated += method_0;
	}

	public void Update()
	{
		method_1();
	}

	public void OnDestroy()
	{
		PlayerCameraController.OnPlayerCameraControllerCreated -= method_0;
	}

	public void method_0(PlayerCameraController playerCameraController, Camera camera)
	{
		InitCamera(playerCameraController, camera);
	}

	public void InitCamera(PlayerCameraController playerCameraController, Camera camera)
	{
		camera_0 = camera;
		playerCameraController_0 = playerCameraController;
		foreach (BulletSoundPlayer bulletSoundPlayer in _bulletSoundPlayers)
		{
			bulletSoundPlayer.Init(camera_0, playerCameraController);
		}
	}

	public void method_1()
	{
		if (!(camera_0 == null) && !(playerCameraController_0 == null) && Singleton<GameWorld>.Instantiated && Singleton<GameWorld>.Instance.SharedBallisticsCalculator != null)
		{
			ISharedBallisticsCalculator sharedBallisticsCalculator = Singleton<GameWorld>.Instance.SharedBallisticsCalculator;
			for (int i = 0; i < sharedBallisticsCalculator.ActiveShotsCount; i++)
			{
				method_2(sharedBallisticsCalculator.GetActiveShot(i), camera_0);
			}
		}
	}

	public void method_2(EftBulletClass shot, Camera camera)
	{
		if (shot.Player == null || shot.Player.iPlayer == playerCameraController_0.Player)
		{
			return;
		}
		int currentIndex = shot.PositionHistory.Count - 1;
		string id = shot.Ammo.Id;
		if (shot.Player.WeaponSoundPlayer != null && shot.Player.WeaponSoundPlayer.IsOccludedToListener)
		{
			return;
		}
		foreach (BulletSoundPlayer bulletSoundPlayer in _bulletSoundPlayers)
		{
			bulletSoundPlayer.TryShot(shot, camera, id, currentIndex);
		}
	}
}
