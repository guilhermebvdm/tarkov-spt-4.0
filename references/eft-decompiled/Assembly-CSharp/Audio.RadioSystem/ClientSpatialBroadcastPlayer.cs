using Audio.AudioCulling;
using UnityEngine;

namespace Audio.RadioSystem;

public class ClientSpatialBroadcastPlayer : ClientBroadcastPlayer
{
	[Header("Spatial Settings")]
	[Space]
	[SerializeField]
	private bool _spatialize = true;

	[SerializeField]
	private float _volumetricRadius = 1f;

	[SerializeField]
	private MetaXRAudioSourceExperimentalFeatures.DirectivityPatternType _directivityPatternType = MetaXRAudioSourceExperimentalFeatures.DirectivityPatternType.HumanVoice;

	[SerializeField]
	[Range(0f, 1f)]
	private float _hrtfIntensity;

	private MetaXRAudioSource[] metaXRAudioSource_0;

	private MetaXRAudioSourceExperimentalFeatures[] metaXRAudioSourceExperimentalFeatures_0;

	public override void Initialize()
	{
		base.Initialize();
		method_2();
	}

	public override void ChangeAudibleState(EAudibleState state)
	{
		base.ChangeAudibleState(state);
		if (base.CurrentAudibleState != state)
		{
			MetaXRAudioSource[] array = metaXRAudioSource_0;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enabled = state == EAudibleState.Unmute;
			}
			MetaXRAudioSourceExperimentalFeatures[] array2 = metaXRAudioSourceExperimentalFeatures_0;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].enabled = state == EAudibleState.Unmute;
			}
		}
	}

	public override void SetupSource(AudioSource source)
	{
		source.spatialize = _spatialize;
		base.SetupSource(source);
	}

	public void method_2()
	{
		metaXRAudioSource_0 = GetComponentsInChildren<MetaXRAudioSource>();
		metaXRAudioSourceExperimentalFeatures_0 = GetComponentsInChildren<MetaXRAudioSourceExperimentalFeatures>();
		if (metaXRAudioSource_0.Length == 2 && metaXRAudioSourceExperimentalFeatures_0.Length == 2)
		{
			MetaXRAudioSource[] array = metaXRAudioSource_0;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].EnableSpatialization = _spatialize;
			}
			MetaXRAudioSourceExperimentalFeatures[] array2 = metaXRAudioSourceExperimentalFeatures_0;
			foreach (MetaXRAudioSourceExperimentalFeatures obj in array2)
			{
				obj.HrtfIntensity = _hrtfIntensity;
				obj.DirectivityPattern = _directivityPatternType;
				obj.VolumetricRadius = _volumetricRadius;
			}
		}
		else
		{
			Debug.LogError("The number of spatial components does not match, the required number of components = 2");
		}
	}
}
