using UnityEngine;

public class AutoDoorSound : MonoBehaviour
{
	[SerializeField]
	private AudioClip _openClip;

	[SerializeField]
	private AudioClip _closeClip;

	[SerializeField]
	[Range(0f, 1f)]
	private float _volume;

	public void OnStartOpen()
	{
		if (!(_openClip == null))
		{
			Vector3 position = base.transform.position;
			float distance = CameraClass.Instance.Distance(position);
			BetterSource betterSource = MonoBehaviourSingleton<BetterAudio>.Instance.PlayAtPoint(position, _openClip, distance, BetterAudio.AudioSourceGroupType.InteractiveObjects, 15);
			if (!(betterSource == null))
			{
				betterSource.SetBaseVolume(_volume);
			}
		}
	}

	public void OnStartClose()
	{
		if (!(_closeClip == null))
		{
			Vector3 position = base.transform.position;
			float distance = CameraClass.Instance.Distance(position);
			BetterSource betterSource = MonoBehaviourSingleton<BetterAudio>.Instance.PlayAtPoint(position, _closeClip, distance, BetterAudio.AudioSourceGroupType.InteractiveObjects, 15);
			if (!(betterSource == null))
			{
				betterSource.SetBaseVolume(_volume);
			}
		}
	}
}
