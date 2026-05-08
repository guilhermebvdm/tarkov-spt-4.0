using EFT.CameraControl;
using UnityEngine;

public class BulletSoundPlayer : MonoBehaviour
{
	public virtual void Init(Camera camera, PlayerCameraController playerCameraController)
	{
	}

	public virtual void TryShot(EftBulletClass shot, Camera camera, string id, int currentIndex)
	{
	}
}
