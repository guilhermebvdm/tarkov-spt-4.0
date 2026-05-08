using System;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;
using UnityEngine.AI;

public class BotSettingsClass
{
	public Player Player;

	[NonSerialized]
	public Vector3 Vector3_0;

	[NonSerialized]
	public Vector3 Vector3_1;

	[NonSerialized]
	public BotsGroup BotsGroup_0;

	[NonSerialized]
	public Vector3[] Vector3_2 = new Vector3[4];

	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_0;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_1;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_2;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_3;

	[NonSerialized]
	[CompilerGenerated]
	public Vector3 Vector3_3;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_4;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_1;

	[NonSerialized]
	[CompilerGenerated]
	public Vector3 Vector3_4;

	[NonSerialized]
	[CompilerGenerated]
	public EBotEnemyCause EbotEnemyCause_0;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_5;

	public bool IsLastPositionChecked
	{
		[CompilerGenerated]
		get
		{
			return Bool_0;
		}
		[CompilerGenerated]
		set
		{
			Bool_0 = value;
		}
	}

	public float LastChangeVisionTime
	{
		[CompilerGenerated]
		get
		{
			return Float_1;
		}
		[CompilerGenerated]
		set
		{
			Float_1 = value;
		}
	}

	public float EnemyLastSeenTimeSense
	{
		[CompilerGenerated]
		get
		{
			return Float_2;
		}
		[CompilerGenerated]
		set
		{
			Float_2 = value;
		}
	}

	public float EnemyLastSeenTimeReal
	{
		[CompilerGenerated]
		get
		{
			return Float_3;
		}
		[CompilerGenerated]
		set
		{
			Float_3 = value;
		}
	}

	public Vector3 SuppressedPos
	{
		[CompilerGenerated]
		get
		{
			return Vector3_3;
		}
		[CompilerGenerated]
		set
		{
			Vector3_3 = value;
		}
	}

	public float Single_0
	{
		[CompilerGenerated]
		get
		{
			return Float_4;
		}
		[CompilerGenerated]
		set
		{
			Float_4 = value;
		}
	}

	public bool IsHaveSeen
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

	public Vector3 EnemyWeaponRootLastPos
	{
		[CompilerGenerated]
		get
		{
			return Vector3_4;
		}
		[CompilerGenerated]
		set
		{
			Vector3_4 = value;
		}
	}

	public EBotEnemyCause Cause
	{
		[CompilerGenerated]
		get
		{
			return EbotEnemyCause_0;
		}
		[CompilerGenerated]
		set
		{
			EbotEnemyCause_0 = value;
		}
	}

	public Vector3 EnemyLastVisiblePosition
	{
		get
		{
			return Vector3_1;
		}
		set
		{
			Vector3_1 = value;
			EnemyLastSeenTimeReal = Time.time;
			BotsGroup_0.SetLastVisionEnemyTimeReal(EnemyLastSeenTimeReal, Vector3_1);
		}
	}

	public Vector3 EnemyLastPosition
	{
		get
		{
			return Vector3_0;
		}
		set
		{
			Vector3_0 = value;
			EnemyLastSeenTimeSense = Time.time;
			BotsGroup_0.SetLastVisionEnemyTimeSence(EnemyLastSeenTimeSense, Vector3_0);
			IsHaveSeen = true;
		}
	}

	public float LastShootTime
	{
		[CompilerGenerated]
		get
		{
			return Float_5;
		}
		[CompilerGenerated]
		set
		{
			Float_5 = value;
		}
	}

	public BotSettingsClass(Player player, BotsGroup botGroup, EBotEnemyCause cause = EBotEnemyCause.Unknown)
	{
		Cause = cause;
		IsLastPositionChecked = true;
		Player = player;
		BotsGroup_0 = botGroup;
	}

	public void Clear()
	{
		IsLastPositionChecked = true;
		EnemyLastSeenTimeSense = Time.time;
	}

	public Vector3 GetPositionForSearch(int searchIndex)
	{
		if (searchIndex < Vector3_2.Length)
		{
			float num = Time.time - EnemyLastSeenTimeReal;
			float num2 = Time.time - Float_0;
			if (num > 5f || num2 > 5f)
			{
				Float_0 = Time.time;
				for (int i = 0; i < Vector3_2.Length; i++)
				{
					NavMeshHit hit;
					Vector3 vector = ((!NavMesh.SamplePosition(Vector3_0 + GClass369.BaseDirections[i], out hit, 2f, -1)) ? Vector3_0 : hit.position);
					Vector3_2[i] = vector;
				}
			}
			return Vector3_2[searchIndex];
		}
		return Vector3_0;
	}

	public void Sync(EnemyInfo info)
	{
		EnemyLastSeenTimeReal = info.TimeLastSeenReal;
		EnemyLastSeenTimeSense = info.TimeLastSeen;
	}

	public void SetSuppressEndTime(float suppressedEndTime)
	{
		Single_0 = suppressedEndTime;
		SuppressedPos = Vector3_0;
	}

	public bool IsSuppressed()
	{
		return Single_0 > Time.time;
	}

	public bool ShallISuppress()
	{
		if ((EnemyLastPosition - Player.Position).sqrMagnitude > LocalBotSettingsProviderClass.Core.DELTA_SUPRESS_DISTANCE_SQRT)
		{
			return false;
		}
		return true;
	}

	public void SetLastVisionChange(float lastChangeVisionTime)
	{
		LastChangeVisionTime = lastChangeVisionTime;
	}

	public void SetLastShootTime()
	{
		LastShootTime = Time.time;
	}
}
