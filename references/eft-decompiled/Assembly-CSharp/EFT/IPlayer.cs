using System;
using System.Collections.Generic;
using EFT.CameraControl;
using EFT.HealthSystem;
using EFT.InventoryLogic;
using UnityEngine;

namespace EFT;

public interface IPlayer
{
	int Id { get; }

	EPlayerSide Side { get; }

	string GroupId { get; }

	string TeamId { get; }

	Vector3 LookDirection { get; }

	Vector3 Position { get; }

	BifacialTransform Transform { get; }

	BifacialTransform WeaponRoot { get; }

	IHealthController HealthController { get; }

	Profile Profile { get; }

	IAIData AIData { get; }

	PlayerLoyaltyData Loyalty { get; }

	bool IsAI { get; }

	Dictionary<BodyPartType, EnemyPart> MainParts { get; }

	string ProfileId { get; }

	string AccountId { get; }

	InventoryController InventoryController { get; }

	IPlayerSearchController SearchController { get; }

	PlayerBones PlayerBones { get; }

	bool IsYourPlayer { get; }

	PlayerBody PlayerBody { get; }

	ICharacterController CharacterController { get; }

	bool IsInBufferZone { get; set; }

	EPlayerBtrState BtrState { get; set; }

	bool StateIsSuitableForHandInput { get; }

	Vector2 Rotation { get; }

	Vector3 Velocity { get; }

	Player.EUpdateMode ArmsUpdateMode { get; }

	EUpdateQueue ArmsUpdateQueue { get; }

	ECameraType VisibleToCameraType { get; }

	bool IsVisibleToCamera { get; }

	event Action<IPlayer> OnIPlayerDeadOrUnspawn;

	GStruct156<Item> FindItemById(MongoID itemId, bool checkDistance = true, bool checkOwnership = true);

	void OnDeserializeFromServer(byte channelId, IDataReader reader);

	RadioTransmitterRecodableComponent FindRadioTransmitter();

	CultistAmuletItemClass FindCultistAmulet();

	bool HasMarkOfUnknown(out MarkOfUnknownItemClass markOfUnknown);

	void SetInteractInHands(EInteraction interaction);

	void PlantItemLocalOnly(Item item, string zone);

	void UpdateInteractionCast();

	void HandleFlareSuccessEvent(Vector3 position, AmmoTemplate ammoTemplate);

	Vector3 PlayerColliderPointOnCenterAxis(float relativeHeight);

	IAnimator GetArmsAnimatorCommon();

	void SetArmsAnimatorCommon(IAnimator animator);
}
