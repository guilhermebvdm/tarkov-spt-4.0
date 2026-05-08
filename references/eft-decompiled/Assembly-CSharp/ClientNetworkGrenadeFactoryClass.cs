using System;
using Systems.Effects;
using Comfort.Common;
using EFT;
using EFT.MovingPlatforms;
using UnityEngine;

public class ClientNetworkGrenadeFactoryClass : GrenadeFactoryClass
{
	public override Grenade AddGrenade(GameObject gameObject)
	{
		return gameObject.AddComponent<ObservedGrenade>();
	}

	public override SmokeGrenade AddSmokeGrenade(GameObject gameObject)
	{
		return gameObject.AddComponent<ObservedSmokeGrenade>();
	}

	public override StunGrenade AddStunGrenade(GameObject gameObject)
	{
		return gameObject.AddComponent<ObservedStunGrenade>();
	}

	public override SmokeGrenade CreateStillSmokeGrenade(string itemId, string template, Vector3 position, Quaternion rotation, float time, MovingPlatform platform = null)
	{
		SmokeGrenade smokeGrenade = base.CreateStillSmokeGrenade(itemId, template, position, rotation, time);
		float num = smokeGrenade.WeaponSource.EmitTime - time;
		if (num > 0f)
		{
			GrenadeEmission emissionEffect = Singleton<Effects>.Instance.GetEmissionEffect(smokeGrenade.GrenadeSettings.EmmisionEffect);
			emissionEffect.transform.SetParent(smokeGrenade.transform, worldPositionStays: false);
			emissionEffect.SetFillParams(time, num);
			emissionEffect.StartEmission(time);
			emissionEffect.Precaution();
			smokeGrenade.EmissionEnd = (Action<Grenade>)Delegate.Combine(smokeGrenade.EmissionEnd, new Action<Grenade>(emissionEffect.StopEmission));
		}
		return smokeGrenade;
	}
}
