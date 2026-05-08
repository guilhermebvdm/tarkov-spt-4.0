using System.Collections.Generic;
using Comfort.Common;
using EFT;
using EFT.Ballistics;
using UnityEngine;

namespace Systems.Effects;

public class EffectsCommutator : MonoBehaviour
{
	[SerializeField]
	private Vector2 _minMaxBleedingSpawnDelta = new Vector2(1f, 3f);

	private Effects.Effect[] effect_0;

	private List<Vector3> list_0 = new List<Vector3>(16);

	private readonly List<KeyValuePair<IPlayer, float>> list_1 = new List<KeyValuePair<IPlayer, float>>(3);

	private const int int_0 = 3;

	private const int int_1 = 4;

	public void PlayerMeshesHit(List<BodyRendererDataStruct> renderers, Vector3 point, Vector3 direction)
	{
		Singleton<Effects>.Instance.PlayerMeshesHit(renderers, point, direction);
	}

	public void PlayHitEffect(EftBulletClass info, ShotInfoClass playerHitInfo)
	{
		if (IsHitPointAlreadyProcessed(info.HitPoint))
		{
			return;
		}
		list_0.Add(info.HitPoint);
		float num = ((info.FragmentIndex > 0) ? (0.5f / (float)info.FragmentIndex) : 1f);
		if (info.HittedBallisticCollider == null)
		{
			return;
		}
		Vector3 position = info.HitPoint + info.HitNormal * EFTHardSettings.Instance.DECAL_SHIFT;
		float num2 = Mathf.InverseLerp(64f, 256f, info.VelocitySqrMagnitude);
		MaterialType material = info.HittedBallisticCollider.TypeOfMaterial;
		EPointOfView pov = EPointOfView.ThirdPerson;
		if (playerHitInfo != null)
		{
			material = playerHitInfo.Material;
			pov = playerHitInfo.PoV;
			if (playerHitInfo.Penetrated)
			{
				CheckEnvironmentHitAfterBodyHit(info);
			}
			else if (playerHitInfo.Silent)
			{
				num2 = 0f;
			}
		}
		else if (info.BulletState == EftBulletClass.EBulletState.RicochetHit)
		{
			Singleton<BetterAudio>.Instance.PlayAtPoint(position, Singleton<Effects>.Instance.AdditionalSoundEffects[0], CameraClass.Instance.Distance(position), 1f, -1f, EnvironmentType.Outdoor, EOcclusionTest.OneShotPropagation);
		}
		bool isHitPointVisible;
		if (!(isHitPointVisible = info.FragmentIndex == 0 && info.Player != null && info.Player.iPlayer.IsYourPlayer))
		{
			Vector3 vector = CameraClass.Instance.Camera.WorldToViewportPoint(info.HitPoint);
			isHitPointVisible = vector.z > 0f && vector.x > -0.01f && vector.x < 1.01f && vector.y > -0.01f && vector.y < 1.01f;
		}
		Singleton<Effects>.Instance.Emit(material, info.HittedBallisticCollider, position, info.HitNormal, info.IsForwardHit ? (num * num2) : 0f, isKnife: false, isHitPointVisible, pov);
	}

	public bool IsHitPointAlreadyProcessed(Vector3 hitPoint)
	{
		int num = 0;
		while (true)
		{
			if (num < list_0.Count)
			{
				Vector3 vector = list_0[num];
				if (Mathf.Abs(hitPoint.x - vector.x) < 0.025f && Mathf.Abs(hitPoint.y - vector.y) < 0.025f && !(Mathf.Abs(hitPoint.z - vector.z) >= 0.025f))
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

	public void Update()
	{
		UpdatePlayersBleedings();
	}

	public void LateUpdate()
	{
		list_0.Clear();
	}

	public static void CheckEnvironmentHitAfterBodyHit(EftBulletClass fragment)
	{
		if (Physics.Raycast(fragment.HitPoint, fragment.Direction, out var hitInfo, EFTHardSettings.Instance.DRAW_BLOOD_ON_WALLS_MAX_DISTANCE, EFTHardSettings.Instance.ENVIRONMENT_HIT_MASK))
		{
			BallisticCollider component = hitInfo.collider.gameObject.GetComponent<BallisticCollider>();
			if (component != null && !(component is BodyPartCollider))
			{
				Singleton<Effects>.Instance.EmitBloodOnEnvironment(hitInfo.point, hitInfo.normal);
			}
		}
	}

	public void PlayKnifeHitEffect(DamageInfoStruct info)
	{
		if (info.HittedBallisticCollider == null)
		{
			return;
		}
		float volume = 1f;
		Vector3 position = info.HitPoint + info.HitNormal * EFTHardSettings.Instance.DECAL_SHIFT;
		if (info.Weapon is KnifeItemClass knifeItemClass && knifeItemClass.KnifeComponent.Template.DisplayOnModel)
		{
			volume = 0f;
			int num = 4;
			if (info.HittedBallisticCollider.TypeOfMaterial == MaterialType.Body || info.HittedBallisticCollider.TypeOfMaterial == MaterialType.BodyArmor)
			{
				num = 3;
			}
			Singleton<BetterAudio>.Instance.PlayAtPoint(position, Singleton<Effects>.Instance.AdditionalSoundEffects[num], CameraClass.Instance.Distance(position), 1f, -1f, EnvironmentType.Outdoor, EOcclusionTest.OneShotPropagation);
		}
		Singleton<Effects>.Instance.Emit(info.HittedBallisticCollider.TypeOfMaterial, info.HittedBallisticCollider, position, info.HitNormal, volume, isKnife: true);
	}

	public void PlayKnifeHitEffect(BallisticCollider collider, Vector3 hitPoint, Vector3 hitNormal, KnifeItemClass knife)
	{
		if (collider == null)
		{
			return;
		}
		float volume = 1f;
		Vector3 position = hitPoint + hitNormal * EFTHardSettings.Instance.DECAL_SHIFT;
		if (knife != null && knife.KnifeComponent.Template.DisplayOnModel)
		{
			volume = 0f;
			int num = 4;
			if (collider.TypeOfMaterial == MaterialType.Body || collider.TypeOfMaterial == MaterialType.BodyArmor)
			{
				num = 3;
			}
			Singleton<BetterAudio>.Instance.PlayAtPoint(position, Singleton<Effects>.Instance.AdditionalSoundEffects[num], CameraClass.Instance.Distance(position), 1f, -1f, EnvironmentType.Outdoor, EOcclusionTest.OneShotPropagation);
		}
		Singleton<Effects>.Instance.Emit(collider.TypeOfMaterial, collider, position, hitNormal, volume, isKnife: true);
	}

	public void StartBleedingForPlayer(IPlayer player)
	{
		bool flag = false;
		using (List<KeyValuePair<IPlayer, float>>.Enumerator enumerator = list_1.GetEnumerator())
		{
			while (enumerator.MoveNext() && !(flag = enumerator.Current.Key == player))
			{
			}
		}
		if (!flag)
		{
			list_1.Add(new KeyValuePair<IPlayer, float>(player, 0f));
		}
	}

	public void StopBleedingForPlayer(IPlayer player)
	{
		int num = -1;
		for (int i = 0; i < list_1.Count; i++)
		{
			if (list_1[i].Key == player)
			{
				num = i;
				break;
			}
		}
		if (num >= 0)
		{
			list_1.RemoveAt(num);
		}
	}

	public void UpdatePlayersBleedings()
	{
		float time = Time.time;
		for (int i = 0; i < list_1.Count; i++)
		{
			KeyValuePair<IPlayer, float> keyValuePair = list_1[i];
			float value = keyValuePair.Value;
			if (time > value && Physics.Raycast(keyValuePair.Key.Position, Vector3.down, out var hitInfo, EFTHardSettings.Instance.DRAW_BLEEDING_MAX_DISTANCE, EFTHardSettings.Instance.ENVIRONMENT_HIT_MASK))
			{
				Singleton<Effects>.Instance.EmitBleeding(hitInfo.point, hitInfo.normal);
				float value2 = time + Random.Range(_minMaxBleedingSpawnDelta.x, _minMaxBleedingSpawnDelta.y);
				list_1[i] = new KeyValuePair<IPlayer, float>(keyValuePair.Key, value2);
			}
		}
	}
}
