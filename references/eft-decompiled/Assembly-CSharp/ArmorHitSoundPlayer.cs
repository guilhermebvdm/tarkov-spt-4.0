using Comfort.Common;
using EFT;
using EFT.CameraControl;
using UnityEngine;

public class ArmorHitSoundPlayer : BulletSoundPlayer
{
	[SerializeField]
	private AudioClip[] _fpSounds;

	[SerializeField]
	private SoundBank _closeDistandeSound;

	private PlayerCameraController playerCameraController_0;

	public override void Init(Camera camera, PlayerCameraController playerCameraController)
	{
		playerCameraController_0 = playerCameraController;
	}

	public void Start()
	{
		GClass897.OnArmorHit += method_0;
	}

	public void OnDestroy()
	{
		GClass897.OnArmorHit -= method_0;
	}

	public void OnDisable()
	{
		GClass897.OnArmorHit -= method_0;
	}

	public void method_0(IPlayer player, IPlayer enemy, Vector3 directionFromPlayer)
	{
		if (!(playerCameraController_0 == null))
		{
			if (playerCameraController_0.Player == player)
			{
				method_1(playerCameraController_0.Camera.transform.forward, directionFromPlayer);
				return;
			}
			float magnitude = (playerCameraController_0.Player.Position - player.Position).magnitude;
			method_2(player, magnitude);
		}
	}

	public void method_1(Vector3 forward, Vector3 normal)
	{
		int num = Random.Range(0, _fpSounds.Length - 1);
		float panStereo = GClass897.CalculatePan(forward, normal);
		Singleton<BetterAudio>.Instance.PlayNonspatial(_fpSounds[num], BetterAudio.AudioSourceGroupType.Impacts, panStereo);
	}

	public void method_2(IPlayer player, float distance)
	{
		Singleton<BetterAudio>.Instance.PlayAtPoint(player.Position, _closeDistandeSound, distance);
	}
}
