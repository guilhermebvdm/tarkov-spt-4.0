using System.Collections;
using Audio.SpatialSystem;
using Comfort.Common;
using EFT.Interactive;
using UnityEngine;

namespace EFT;

public class ExfiltrationTimerSoundPlayer : MonoBehaviour
{
	[SerializeField]
	private ExfiltrationPoint _exfiltrationPoint;

	[SerializeField]
	private EExfiltrationStatus _targetRequiredStatus;

	[SerializeField]
	private AudioSingleClipContainer _openSound;

	[SerializeField]
	private BetterAudio.AudioSourceGroupType _groupType = BetterAudio.AudioSourceGroupType.Environment;

	[SerializeField]
	private bool _spatialize;

	[SerializeField]
	private bool _enabledHighPassFilter;

	[SerializeField]
	private bool _enabledReverb;

	private bool bool_0;

	private bool bool_1;

	private IEnumerator ienumerator_0;

	public void Awake()
	{
		if (Singleton<GameWorld>.Instantiated && _exfiltrationPoint != null)
		{
			Singleton<GameWorld>.Instance.AfterGameStarted += method_0;
		}
	}

	public void method_0()
	{
		if (_exfiltrationPoint.Status == _targetRequiredStatus)
		{
			method_1(_exfiltrationPoint, _exfiltrationPoint.Status);
		}
		else
		{
			_exfiltrationPoint.OnStatusChanged += method_1;
		}
	}

	public void method_1(ExfiltrationPoint point, EExfiltrationStatus status)
	{
		if (!bool_0 && point.Status == _targetRequiredStatus && method_2())
		{
			float num = GClass1893.PastTimeSeconds(Singleton<AbstractGame>.Instance.GameTimer) - (float)_exfiltrationPoint.Settings.StartTime;
			if (!(num > 0f))
			{
				GClass7.StopBehaviourTimer(this, ref ienumerator_0);
				ienumerator_0 = GClass7.StartBehaviourTimer(this, Mathf.Abs(num), PlaySound);
				bool_0 = true;
			}
		}
	}

	public bool method_2()
	{
		if (!(_exfiltrationPoint == null) && _exfiltrationPoint.HasRequirements)
		{
			ExfiltrationRequirement[] requirements = _exfiltrationPoint.Requirements;
			int num = 0;
			while (true)
			{
				if (num < requirements.Length)
				{
					if (requirements[num].Requirement == ERequirementState.Timer)
					{
						break;
					}
					num++;
					continue;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public void PlaySound()
	{
		if (MonoBehaviourSingleton<BetterAudio>.Instantiated && MonoBehaviourSingleton<BetterAudio>.Instance.TryPlayAtPoint(out var source, base.transform.position, _openSound.GetClip(), _groupType, _openSound.GetMaxDistance(), _openSound.GetVolume(), EOcclusionTest.None, null, _spatialize, oneShot: false, autoReleaseSource: true, _enabledHighPassFilter))
		{
			source.EnableReverb = _enabledReverb;
			if (MonoBehaviourSingleton<SpatialAudioSystem>.Instantiated)
			{
				MonoBehaviourSingleton<SpatialAudioSystem>.Instance.ProcessSourceOcclusion(base.gameObject, source, EOcclusionTest.ContinuousPropagated);
			}
		}
	}

	public void OnDestroy()
	{
		GClass7.StopBehaviourTimer(this, ref ienumerator_0);
		if (Singleton<GameWorld>.Instantiated)
		{
			Singleton<GameWorld>.Instance.AfterGameStarted -= method_0;
		}
		if (_exfiltrationPoint != null)
		{
			_exfiltrationPoint.OnStatusChanged += method_1;
		}
	}
}
