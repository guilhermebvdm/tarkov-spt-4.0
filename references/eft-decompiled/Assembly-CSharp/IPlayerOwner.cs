using EFT;
using RootMotion.FinalIK;
using UnityEngine;

public interface IPlayerOwner
{
	IPlayer iPlayer { get; }

	IAIData AIData { get; }

	float AverageRotationSpeed { get; }

	float BotSoundCoef { get; }

	float SoundRadius { get; }

	Vector3 Velocity { get; }

	bool IsAI { get; }

	ICharacterController CharacterController { get; }

	EPlayerState CurrentStataName { get; }

	float CharacterSpeed { get; }

	string Nickname { get; }

	bool IsZombie { get; }

	WeaponSoundPlayer WeaponSoundPlayer { get; }

	HitReaction HitReaction { get; }

	GInterface270 MovementContextAdapter { get; }

	EPointOfView PointOfView { get; }

	string ItemInHandTemplateID { get; }

	bool HasBodyPartCollider(Collider col);

	void ShotReactions(DamageInfoStruct shot, EBodyPart bodyPart);

	ShotInfoClass ApplyShot(DamageInfoStruct damageInfo, EBodyPart bodyPartType, EBodyPartColliderType colliderType, EArmorPlateCollider armorPlateCollider, ShotIdStruct shotId);

	void Dispose();
}
