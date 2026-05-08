using EFT.PrefabSettings;
using UnityEngine;

namespace Systems.Effects;

public class FireworkEffectSelector : FlareShotEffectSelector
{
	[SerializeField]
	private ParticleSystem _fireworkParticleSystem;

	public override void SetFlareEffect(FlareColorType flareColorType, float lifetime)
	{
		_flarePatronSound.Init(lifetime);
		_fireworkParticleSystem.transform.parent = null;
		_fireworkParticleSystem.Play();
	}
}
