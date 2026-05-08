using System;
using System.Runtime.CompilerServices;
using EFT;
using EFT.InventoryLogic;
using RootMotion.FinalIK;
using UnityEngine;

public class GClass1710 : IPlayerOwner
{
	[NonSerialized]
	[CompilerGenerated]
	public GInterface270 Ginterface270_0;

	[NonSerialized]
	public Player Player_0;

	public IPlayer iPlayer => Player_0;

	public IAIData AIData => Player_0.AIData;

	public float AverageRotationSpeed => Player_0.MovementContext.AverageRotationSpeed.Avarage;

	public float BotSoundCoef => Player_0.Skills.BotSoundCoef;

	public float SoundRadius => Player_0.Physical.SoundRadius;

	public Vector3 Velocity => Player_0._characterController.velocity;

	public bool IsAI => Player_0.IsAI;

	public bool IsZombie => iPlayer?.Profile.Info.Settings.UseSimpleAnimator ?? false;

	public ICharacterController CharacterController => Player_0.CharacterController;

	public EPlayerState CurrentStataName => Player_0.MovementContext.CurrentState.Name;

	public float CharacterSpeed => Player_0.MovementContext.ClampedSpeed;

	public string Nickname => Player_0.Profile.Nickname;

	public WeaponSoundPlayer WeaponSoundPlayer
	{
		get
		{
			if (Player_0.HandsController is Player.FirearmController firearmController)
			{
				return firearmController.WeaponSoundPlayer;
			}
			return null;
		}
	}

	public HitReaction HitReaction => Player_0.HitReaction;

	public GInterface270 MovementContextAdapter
	{
		[CompilerGenerated]
		get
		{
			return Ginterface270_0;
		}
	}

	public EPointOfView PointOfView => Player_0.PointOfView;

	public string ItemInHandTemplateID
	{
		get
		{
			MongoID? mongoID = Player_0.TryGetItemInHands<Item>()?.TemplateId;
			if (!mongoID.HasValue)
			{
				return null;
			}
			return mongoID.GetValueOrDefault();
		}
	}

	public bool HasBodyPartCollider(Collider col)
	{
		return Player_0.HasBodyPartCollider(col);
	}

	public void ShotReactions(DamageInfoStruct shot, EBodyPart bodyPart)
	{
		Player_0.ShotReactions(shot, bodyPart);
	}

	public ShotInfoClass ApplyShot(DamageInfoStruct damageInfo, EBodyPart bodyPartType, EBodyPartColliderType colliderType, EArmorPlateCollider armorPlateCollider, ShotIdStruct shotId)
	{
		return Player_0.ApplyShot(damageInfo, bodyPartType, colliderType, armorPlateCollider, shotId);
	}

	public GClass1710(Player player)
	{
		Player_0 = player;
		Ginterface270_0 = new GClass2583(Player_0.MovementContext);
	}

	public void Dispose()
	{
		MovementContextAdapter?.Dispose();
	}
}
