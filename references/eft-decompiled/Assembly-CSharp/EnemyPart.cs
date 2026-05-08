using System;
using System.Collections.Generic;
using EFT;
using UnityEngine;

public class EnemyPart
{
	[NonSerialized]
	public bool CanAimToPart = true;

	[NonSerialized]
	public BifacialTransform BifacialTransform;

	[NonSerialized]
	public BodyPartCollider BodyPartCollider;

	[NonSerialized]
	public bool CanShoot_1;

	[NonSerialized]
	public int PosCachedForFrame;

	[NonSerialized]
	public Vector3 PositionCached;

	[NonSerialized]
	public bool CanShootLastValue;

	[NonSerialized]
	public bool VisCastForShootIsTried;

	[NonSerialized]
	public Vector3 LastVisibilityCastOffsetLocal;

	[NonSerialized]
	public Vector3 LastShootCastOffsetLocal;

	[field: NonSerialized]
	public IPlayer EnemyPlayer { get; }

	public Vector3 Position
	{
		get
		{
			if (Time.frameCount != PosCachedForFrame)
			{
				PositionCached = BifacialTransform.position;
				PosCachedForFrame = Time.frameCount;
			}
			return PositionCached;
		}
	}

	public bool CanShoot
	{
		get
		{
			return CanShoot_1;
		}
		set
		{
			if (CanAimToPart)
			{
				CanShoot_1 = value;
			}
		}
	}

	[field: NonSerialized]
	public BodyPartType BodyPartType { get; }

	public static Dictionary<BodyPartType, EnemyPart> Create(IPlayer person, PlayerBones bones)
	{
		return new Dictionary<BodyPartType, EnemyPart>
		{
			{
				BodyPartType.body,
				new EnemyPart(BodyPartType.body, person, bones.Spine3)
			},
			{
				BodyPartType.leftArm,
				new EnemyPart(BodyPartType.leftArm, person, bones.LeftShoulder)
			},
			{
				BodyPartType.rightArm,
				new EnemyPart(BodyPartType.rightArm, person, bones.RightShoulder)
			},
			{
				BodyPartType.head,
				new EnemyPart(BodyPartType.head, person, bones.Head, bones.BodyPartCollidersDictionary[EBodyPartColliderType.HeadCommon])
			},
			{
				BodyPartType.rightLeg,
				new EnemyPart(BodyPartType.rightLeg, person, bones.RightThigh1)
			},
			{
				BodyPartType.leftLeg,
				new EnemyPart(BodyPartType.leftLeg, person, bones.LeftThigh1)
			}
		};
	}

	public EnemyPart(BodyPartType bodyPartType, IPlayer enemy, BifacialTransform bifacialTransform)
	{
		EnemyPlayer = enemy;
		BodyPartType = bodyPartType;
		BifacialTransform = bifacialTransform;
		if (bodyPartType == BodyPartType.head)
		{
			CanAimToPart = LocalBotSettingsProviderClass.Core.CAN_SHOOT_TO_HEAD;
		}
	}

	public EnemyPart(BodyPartType bodyPartType, IPlayer enemy, BifacialTransform bifacialTransform, BodyPartCollider partCollider)
		: this(bodyPartType, enemy, bifacialTransform)
	{
		BodyPartCollider = partCollider;
	}

	public Vector3 GetPartPositionWithOffset()
	{
		return BifacialTransform.position + BifacialTransform.TransformVector(LastShootCastOffsetLocal);
	}

	public Vector3 GetPartRaycastTarget(bool hasLineOfSight, Vector3 sensorOrigin)
	{
		Vector3 position = Position;
		Vector3 vector = Vector3.zero;
		if (BodyPartCollider != null)
		{
			vector = (hasLineOfSight ? LastVisibilityCastOffsetLocal : BodyPartCollider.GetRandomPointToCastLocal(sensorOrigin));
			position += BodyPartCollider.transform.TransformVector(vector);
		}
		LastVisibilityCastOffsetLocal = vector;
		return position;
	}

	public void CheckCanShoot(BotOwner botOwner, GClass542 partVision, bool checkOnlyIfVisible = true)
	{
		if (checkOnlyIfVisible && !partVision.Visible)
		{
			CanShoot = false;
			return;
		}
		Vector3 shootStartPos = botOwner.LookSensor.ShootStartPos;
		Vector3 position = Position;
		Vector3 vector = Vector3.zero;
		if (BodyPartCollider != null)
		{
			if (CanShootLastValue)
			{
				vector = LastShootCastOffsetLocal;
			}
			else if (!VisCastForShootIsTried && partVision.HasLineOfSight)
			{
				vector = LastVisibilityCastOffsetLocal;
				VisCastForShootIsTried = true;
			}
			else
			{
				vector = BodyPartCollider.GetRandomPointToCastLocal(shootStartPos);
			}
			Vector3 vector2 = BodyPartCollider.TransformCached.TransformVector(vector);
			position += vector2;
		}
		float magnitude = (position - shootStartPos).magnitude;
		if (Physics.Linecast(shootStartPos, position, out var _, LayerMaskClass.HighPolyWithTerrainMask))
		{
			CanShoot = false;
		}
		else
		{
			bool flag = false;
			if (partVision.VisibleType == EEnemyPartVisibleType.Sence)
			{
				flag = partVision.IsFullDisappearForShoot(botOwner);
			}
			CanShoot = !flag && botOwner.LookSensor.MaxShootDist > magnitude;
		}
		CanShootLastValue = CanShoot;
		LastShootCastOffsetLocal = vector;
		if (CanShoot)
		{
			VisCastForShootIsTried = false;
		}
	}
}
