using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EFT;
using EFT.Ballistics;
using EFT.NextObservedPlayer;
using RootMotion.FinalIK;
using UnityEngine;

public class GClass1711 : IPlayerOwner
{
	[NonSerialized]
	[CompilerGenerated]
	public GInterface270 Ginterface270_0;

	[NonSerialized]
	public ObservedPlayerView ObservedPlayerView_0;

	public IPlayer iPlayer => ObservedPlayerView_0;

	public IAIData AIData => ObservedPlayerView_0.AIData;

	public float AverageRotationSpeed => ObservedPlayerView_0.ObservedPlayerController.StateContext.AverageRotationSpeed;

	public float BotSoundCoef => ObservedPlayerView_0.ObservedPlayerController.InfoContainer.BotSoundCoef;

	public float SoundRadius => ObservedPlayerView_0.ObservedPlayerController.InfoContainer.SoundRadius;

	public Vector3 Velocity => ObservedPlayerView_0.ObservedPlayerController.StateContext.Velocity;

	public bool IsAI => ObservedPlayerView_0.IsAI;

	public string Nickname => ObservedPlayerView_0.ObservedPlayerController.InfoContainer.NickName;

	public bool IsZombie => iPlayer?.Profile.Info.Settings.UseSimpleAnimator ?? false;

	public ICharacterController CharacterController => ObservedPlayerView_0.CharacterController;

	public EPlayerState CurrentStataName => ObservedPlayerView_0.ObservedPlayerController.MovementController.ObservedPlayerStateContext.CurrentStateName;

	public float CharacterSpeed => ObservedPlayerView_0.ObservedPlayerController.MovementController.ObservedPlayerStateContext.CharacterMovementSpeed;

	public WeaponSoundPlayer WeaponSoundPlayer => ObservedPlayerView_0.ObservedPlayerController?.HandsController?.WeaponSoundPlayer;

	public string ItemInHandTemplateID
	{
		get
		{
			MongoID? mongoID = ObservedPlayerView_0.ObservedPlayerController.HandsController.ItemInHands?.TemplateId;
			if (!mongoID.HasValue)
			{
				return null;
			}
			return mongoID.GetValueOrDefault();
		}
	}

	public GInterface270 MovementContextAdapter
	{
		[CompilerGenerated]
		get
		{
			return Ginterface270_0;
		}
	}

	public EPointOfView PointOfView => EPointOfView.ThirdPerson;

	public HitReaction HitReaction => ObservedPlayerView_0.HitReaction;

	public IEnumerable<GClass2903> IEnumerable_0 => ObservedPlayerView_0.ObservedPlayerController.ArmorInfoController.ObservedPlayerArmors.Values;

	public bool HasBodyPartCollider(Collider col)
	{
		return ObservedPlayerView_0.PlayerBones.BodyPartCollidersHashSet.Contains(col);
	}

	public void ShotReactions(DamageInfoStruct shot, EBodyPart bodyPart)
	{
		ObservedPlayerView_0.ReactionOnShot(shot, bodyPart);
	}

	public ShotInfoClass ApplyShot(DamageInfoStruct damageInfo, EBodyPart bodyPartType, EBodyPartColliderType colliderType, EArmorPlateCollider armorPlateCollider, ShotIdStruct shotId)
	{
		if (!ObservedPlayerView_0.HealthController.IsAlive)
		{
			return null;
		}
		bool hasValue = damageInfo.DeflectedBy.HasValue;
		MaterialType material = MaterialType.Body;
		MongoID? hitArmorID = null;
		if (hasValue)
		{
			material = MaterialType.HelmetRicochet;
		}
		else
		{
			foreach (GClass2903 item in IEnumerable_0)
			{
				if (item.ShotMatches(colliderType, armorPlateCollider))
				{
					material = item.material;
					hitArmorID = item.itemID;
					break;
				}
			}
		}
		ShotInfoClass result = new ShotInfoClass
		{
			PoV = EPointOfView.ThirdPerson,
			Penetrated = damageInfo.Penetrated,
			Material = material,
			HitArmorID = hitArmorID
		};
		ObservedPlayerView_0.ReactionOnShot(damageInfo, bodyPartType);
		return result;
	}

	public GClass1711(ObservedPlayerView player)
	{
		ObservedPlayerView_0 = player;
		Ginterface270_0 = new GClass2584(ObservedPlayerView_0.ObservedPlayerController.StateContext);
	}

	public void Dispose()
	{
		MovementContextAdapter?.Dispose();
	}
}
