using EFT;
using UnityEngine;

public class Gramophone : MonoBehaviour
{
	public SoundBank Bank;

	public float BlendOverride = -1f;

	public EnvironmentType Environment;

	private SoundBank soundBank_0;

	public void Update()
	{
		if (!(Bank == null) && Input.GetKeyDown(KeyCode.KeypadEnter))
		{
			MonoBehaviourSingleton<BetterAudio>.Instance.PlayAtPoint(base.transform.position, Bank, CameraClass.Instance.Distance(base.transform.position), Bank.RandomVolume, BlendOverride, Environment);
		}
	}
}
