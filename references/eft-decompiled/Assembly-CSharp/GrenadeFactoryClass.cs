using Comfort.Common;
using EFT;
using EFT.MovingPlatforms;
using UnityEngine;

public class GrenadeFactoryClass
{
	public Grenade Create(GrenadeSettings grenadeSettings, Vector3 position, Quaternion rotation, Vector3 force, ThrowWeapItemClass item, string profileId, float prewarm = 0f, bool withBallisticCalculator = true, bool isBeingPlanted = false)
	{
		grenadeSettings.transform.rotation = rotation;
		grenadeSettings.transform.position = position;
		grenadeSettings.gameObject.layer = LayerMask.NameToLayer("Default");
		ISharedBallisticsCalculator calculator = null;
		if (withBallisticCalculator)
		{
			calculator = Singleton<GInterface169>.Instance.CreateBallisticCalculator(0);
		}
		Grenade grenade = ((grenadeSettings is SmokeGrenadeSettings) ? AddSmokeGrenade(grenadeSettings.gameObject) : ((!(grenadeSettings is StunGrenadeSettings)) ? AddGrenade(grenadeSettings.gameObject) : AddStunGrenade(grenadeSettings.gameObject)));
		grenade.Init(grenadeSettings, profileId, item, prewarm, calculator, isBeingPlanted);
		grenade.SetThrowForce(force);
		return grenade;
	}

	public virtual SmokeGrenade CreateStillSmokeGrenade(string itemId, string template, Vector3 position, Quaternion rotation, float time, MovingPlatform platform = null)
	{
		ThrowWeapItemClass item = (ThrowWeapItemClass)Singleton<ItemFactoryClass>.Instance.CreateItem(itemId, template, null);
		SmokeGrenadeSettings smokeGrenadeSettings = (SmokeGrenadeSettings)Object.Instantiate(Singleton<PoolManagerClass>.Instance.CreateItem(item, isAnimated: true).GetComponent<GrenadePrefab>().GrenadeItself);
		smokeGrenadeSettings.Offset = Vector3.zero;
		if (platform != null)
		{
			position = platform.transform.TransformPoint(position);
			rotation = platform.transform.rotation * rotation;
		}
		SmokeGrenade obj = (SmokeGrenade)Create(smokeGrenadeSettings, position, rotation, Vector3.zero, item, null, time, withBallisticCalculator: false);
		obj.RemoveRigidbody();
		return obj;
	}

	public virtual SmokeGrenade AddSmokeGrenade(GameObject gameObject)
	{
		return gameObject.AddComponent<SmokeGrenade>();
	}

	public virtual StunGrenade AddStunGrenade(GameObject gameObject)
	{
		return gameObject.AddComponent<StunGrenade>();
	}

	public virtual Grenade AddGrenade(GameObject gameObject)
	{
		return gameObject.AddComponent<Grenade>();
	}
}
