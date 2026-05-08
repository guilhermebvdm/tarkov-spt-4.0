using EFT;
using UnityEngine;

public class AudioTester : MonoBehaviour
{
	private Coroutine coroutine_0;

	public SoundBank SoundBank;

	public TagBank Phrases;

	public EOcclusionTest EOcclusionTest = EOcclusionTest.Regular;

	public bool JustTest;

	public bool Talking;

	private float float_0;

	public float Freq = 3f;

	public float Times = 1f;

	public void Update()
	{
		if (!Talking || Time.time < float_0 + Freq)
		{
			return;
		}
		if (!JustTest)
		{
			if (Phrases != null)
			{
				TaggedClip taggedClip = Phrases.Clips[Random.Range(0, Phrases.Clips.Length)];
				for (int i = 0; (float)i < Times; i++)
				{
					MonoBehaviourSingleton<BetterAudio>.Instance.PlayAtPoint(base.transform.position, taggedClip.Clip, CameraClass.Instance.Distance(base.transform.position), BetterAudio.AudioSourceGroupType.Speech, taggedClip.Falloff, taggedClip.Volume, EOcclusionTest);
				}
			}
			else if (SoundBank != null)
			{
				for (int j = 0; (float)j < Times; j++)
				{
					MonoBehaviourSingleton<BetterAudio>.Instance.PlayAtPoint(base.transform.position, SoundBank, CameraClass.Instance.Distance(base.transform.position), 1f, -1f, EnvironmentType.Outdoor, EOcclusionTest);
				}
			}
		}
		float_0 = Time.time;
	}
}
