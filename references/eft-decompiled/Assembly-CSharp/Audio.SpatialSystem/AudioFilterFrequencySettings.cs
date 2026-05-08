using System;
using UnityEngine;

namespace Audio.SpatialSystem;

[Serializable]
public class AudioFilterFrequencySettings
{
	[SerializeField]
	[Range(100f, 22000f)]
	public float _defaultIndoorOcclusionFrequency = 8000f;

	[SerializeField]
	[Range(100f, 22000f)]
	public float _defaultOutdoorOcclusionFrequency = 5000f;

	[SerializeField]
	[Range(100f, 22000f)]
	public float _lowerOcclusionFrequency = 450f;

	[SerializeField]
	[Range(100f, 22000f)]
	public float _upperOcclusionFrequency = 700f;

	[SerializeField]
	[Range(100f, 22000f)]
	public float _wallOcclusionFrequency = 1600f;

	[SerializeField]
	[Range(100f, 22000f)]
	public float _fullOcclusionFrequency = 100f;

	[SerializeField]
	[Range(100f, 22000f)]
	public float _differentEnvironmentsFrequency = 950f;

	public float GetLowerFrequency(ISpatialAudioRoom sourceRoom, Vector3 soundPosition, Vector3 listenerPosition, float obstruction, float propagation, bool isolated = false)
	{
		SpatialAudioSystem instance = MonoBehaviourSingleton<SpatialAudioSystem>.Instance;
		float t = ((propagation > 0f) ? propagation : obstruction);
		bool flag = instance.IsSourceAndListenerOutdoor(sourceRoom);
		bool flag2 = instance.IsSourceAndListenerInDiffEnvironment(sourceRoom);
		if (isolated)
		{
			return Mathf.Lerp((propagation > 0f) ? _wallOcclusionFrequency : _defaultOutdoorOcclusionFrequency, _fullOcclusionFrequency, t);
		}
		if (flag)
		{
			return _defaultOutdoorOcclusionFrequency;
		}
		if (flag2)
		{
			return _differentEnvironmentsFrequency;
		}
		float result = _defaultIndoorOcclusionFrequency;
		if (propagation > 0f)
		{
			result = _wallOcclusionFrequency;
		}
		if (!(propagation <= 0f) && GClass1162.IsNeedVerticalOcclusion(soundPosition, listenerPosition, out var verticalDot))
		{
			if (!(verticalDot > 0f))
			{
				return _lowerOcclusionFrequency;
			}
			return _upperOcclusionFrequency;
		}
		return result;
	}
}
