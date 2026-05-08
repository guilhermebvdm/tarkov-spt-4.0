using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Comfort.Common;
using EFT;
using UnityEngine;
using UnityEngine.AI;

public class GClass386 : IPositionPoint
{
	[NonSerialized]
	[CompilerGenerated]
	public Vector3 Vector3_0;

	public float CreatedTime;

	public Player Player;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_1;

	[NonSerialized]
	public List<BotsGroup> List_0;

	[NonSerialized]
	public bool Bool_2;

	[NonSerialized]
	public int Int_0 = -1;

	[NonSerialized]
	[CompilerGenerated]
	public EPlayerSide EplayerSide_0;

	[NonSerialized]
	[CompilerGenerated]
	public Vector3 Vector3_1;

	public Vector3 Position
	{
		[CompilerGenerated]
		get
		{
			return Vector3_0;
		}
		[CompilerGenerated]
		set
		{
			Vector3_0 = value;
		}
	}

	public bool IsOnNavMesh
	{
		[CompilerGenerated]
		get
		{
			return Bool_1;
		}
		[CompilerGenerated]
		set
		{
			Bool_1 = value;
		}
	}

	public EPlayerSide Side
	{
		[CompilerGenerated]
		get
		{
			return EplayerSide_0;
		}
	}

	public Vector3 BodyPosition
	{
		[CompilerGenerated]
		get
		{
			return Vector3_1;
		}
	}

	public GClass386(BotsGroup coreGroup, List<BotsGroup> groups, bool isAI, EPlayerSide side, Vector3 pos, IPlayer player)
	{
		Player = Singleton<GameWorld>.Instance.GetEverExistedPlayerByID(player.ProfileId);
		if (Player != null && Player.PlayerBones != null && Player.PlayerBones.Ribcage != null)
		{
			Vector3_1 = Player.PlayerBones.Ribcage.position;
		}
		List_0 = groups;
		EplayerSide_0 = side;
		Bool_2 = isAI;
		IsOnNavMesh = NavMesh.SamplePosition(pos, out var hit, 1f, -1);
		if (IsOnNavMesh)
		{
			Position = hit.position;
		}
		else
		{
			Position = pos;
		}
		CreatedTime = Time.time;
	}

	public bool IsEnemiesBody(EPlayerSide sideFounder)
	{
		if (!Bool_2)
		{
			return true;
		}
		if (Side == sideFounder)
		{
			return false;
		}
		return true;
	}

	public void ConnectedToGroups()
	{
	}

	public bool IsVisibleBy(BotOwner owner)
	{
		if (!Bool_0 && (owner.Transform.position - Position).sqrMagnitude < owner.Settings.FileSettings.Mind.DIST_TO_FOUND_SQRT && !Physics.Linecast(owner.MyHead.position, Position, LayerMaskClass.HighPolyWithTerrainMask))
		{
			Bool_0 = true;
			return true;
		}
		return false;
	}

	public void AddEnemyToGroups(IPlayer person, BotOwner bot, EBotEnemyCause cause)
	{
		Vector3 position = bot.Transform.position;
		Vector3 v = Position - position;
		Vector3 suspectedPoint = Position + GClass855.NormalizeFastSelf(v) * 50f;
		foreach (BotsGroup item in List_0)
		{
			item.AddPointToSearch(suspectedPoint, 350f, bot);
			if (person != null)
			{
				item.AddEnemy(person, cause);
			}
		}
	}

	public void InspectionComplete()
	{
	}

	public void SetUnderWork(BotOwner owner)
	{
		Int_0 = owner.Id;
	}

	public bool IsFreeFor(BotOwner owner)
	{
		if (Int_0 <= 0)
		{
			return true;
		}
		if (Int_0 == owner.Id)
		{
			return true;
		}
		return false;
	}
}
