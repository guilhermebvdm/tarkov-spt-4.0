using System.Collections;
using Comfort.Common;
using EFT;
using UnityEngine;

namespace MultiFlare;

[HelpURL("https://eodproject.atlassian.net/wiki/spaces/FRONT/pages/1254981646/Unity.#Flare")]
public class FlareSceneSettings : MonoBehaviour
{
	private GClass1031 gclass1031_0;

	[SerializeField]
	private FlareSettings _settings;

	public FlareSettings Settings => _settings;

	public void Awake()
	{
		GClass1023.Instance.SetupRenderer(_settings);
	}

	public IEnumerator Start()
	{
		WaitForSeconds waitForSeconds = new WaitForSeconds(1f);
		while (!CameraClass.Instance.Camera)
		{
			yield return waitForSeconds;
		}
		GClass1026 camera = new GClass1026(CameraClass.Instance.Camera);
		GClass1023.Instance.AddCamera(camera);
		CameraClass.Instance.OpticCameraManager.OnOpticEnabled += smethod_0;
		CameraClass.Instance.OpticCameraManager.OnOpticDisabled += smethod_1;
		if ((bool)Singleton<GameWorld>.Instance)
		{
			Player mainPlayer = Singleton<GameWorld>.Instance.MainPlayer;
			if (mainPlayer != null)
			{
				float blindnessProtection = mainPlayer.GetBlindnessProtection();
				mainPlayer.OnBlindnessProtectionChanged += method_0;
				gclass1031_0 = new GClass1031(this, _settings, blindnessProtection);
			}
		}
	}

	public void OnDestroy()
	{
		GClass3687 opticCameraManager = CameraClass.Instance.OpticCameraManager;
		if (opticCameraManager != null)
		{
			opticCameraManager.OnOpticEnabled -= smethod_0;
			opticCameraManager.OnOpticDisabled -= smethod_1;
		}
		if ((bool)Singleton<GameWorld>.Instance)
		{
			Player mainPlayer = Singleton<GameWorld>.Instance.MainPlayer;
			if ((bool)mainPlayer)
			{
				mainPlayer.OnBlindnessProtectionChanged -= method_0;
			}
		}
		GClass1023.Instance.DisposeRenderer();
	}

	public void Update()
	{
		if (CameraClass.Exist && (bool)CameraClass.Instance.NightVision)
		{
			_settings.IsNvEnabled = CameraClass.Instance.NightVision.On;
		}
	}

	public void method_0(float blindnessProtection)
	{
		if (GClass855.IsZero(blindnessProtection))
		{
			gclass1031_0.Disable();
		}
		else
		{
			gclass1031_0.Enable(blindnessProtection);
		}
	}

	public static void smethod_0()
	{
		GClass1026 camera = new GClass1026(CameraClass.Instance.OpticCameraManager.Camera);
		GClass1023.Instance.AddCamera(camera);
	}

	public static void smethod_1()
	{
		GClass1023.Instance.RemoveCamera(CameraClass.Instance.OpticCameraManager.Camera);
	}
}
