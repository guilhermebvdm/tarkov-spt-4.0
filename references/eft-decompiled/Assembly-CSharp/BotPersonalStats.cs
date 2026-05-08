using System;
using System.Collections.Generic;
using System.IO;
using EFT;
using EFT.InventoryLogic;
using UnityEngine;

public class BotPersonalStats
{
	public const float DIST1 = 13f;

	public const float DIST2 = 27f;

	public const float DIST3 = 43f;

	public const float DIST4 = 60f;

	public const float DIST5 = 86f;

	public const float DIST6 = 120f;

	public const float DIST7 = 160f;

	[NonSerialized]
	public const string NULL_ID = "Null_id";

	public static float[] DISTANCES = new float[7] { 13f, 27f, 43f, 60f, 86f, 120f, 160f };

	public static int STAT_INDEX = 1;

	public float LifeTime;

	public int CoverPointsTaken;

	public bool IsDead;

	public float TotaTimeInCover;

	public float middleGrendaThrowDist;

	public float MiddleTimeInCover;

	public string WeaponType;

	public string Zone;

	public string MySide;

	public string ProfileId;

	public string Difficulty;

	public float TotalDamage;

	public float BornTime;

	public int HaveGrenadesStart;

	public int GrenadeTrowed;

	public int KilledByGrenade;

	public Dictionary<string, GClass634> targetStats = new Dictionary<string, GClass634>();

	public Dictionary<string, GClass634> HitsGain = new Dictionary<string, GClass634>();

	public Dictionary<string, GClass627> Frags = new Dictionary<string, GClass627>();

	public Dictionary<BotStandByType, float> StandByTimes = new Dictionary<BotStandByType, float>();

	public GClass631 gainSightStats = new GClass631();

	public GClass625 AimingStats = new GClass625();

	[NonSerialized]
	public float TimeComeToCover;

	[NonSerialized]
	public BotOwner Owner;

	[NonSerialized]
	public bool IsWritten;

	[NonSerialized]
	public float CurStandByTime;

	public string WildType;

	[NonSerialized]
	public bool IsEnable_1;

	public bool IsEnable => IsEnable_1;

	public event Action<string, DamageInfoStruct> OnKillTarget;

	public event Action<IPlayer, DamageInfoStruct, EBodyPart> OnHitTarget;

	public static int GetIndex(float d)
	{
		int num = 0;
		while (true)
		{
			if (num < DISTANCES.Length)
			{
				if (!(d >= DISTANCES[num]))
				{
					break;
				}
				num++;
				continue;
			}
			return DISTANCES.Length - 1;
		}
		return num;
	}

	public void Init(BotOwner owner, string Zone)
	{
		if (owner == null)
		{
			IsEnable_1 = false;
			return;
		}
		HaveGrenadesStart = owner.WeaponManager.Grenades.GetGrenadeCount();
		Owner = owner;
		GClass628.AddBot(this);
		ProfileId = owner.Profile.Id;
		InfoClass info = owner.Profile.Info;
		object obj;
		if (info == null)
		{
			obj = null;
		}
		else
		{
			ProfileInfoSettingsClass settings = info.Settings;
			if (settings == null)
			{
				obj = null;
			}
			else
			{
				obj = settings.Role.ToString();
				if (obj != null)
				{
					goto IL_007d;
				}
			}
		}
		obj = "Unknown_Role_Profile.Info.Settings_is_null";
		goto IL_007d;
		IL_007d:
		WildType = (string)obj;
		this.Zone = Zone;
		Difficulty = ((owner.Profile.Info == null || owner.Profile.Info.Settings == null) ? "not set" : owner.Profile.Info.Settings.BotDifficulty.ToString());
		CurStandByTime = Time.time;
		MySide = owner.Side.ToString();
		BornTime = Time.time;
		foreach (BotStandByType value in Enum.GetValues(typeof(BotStandByType)))
		{
			StandByTimes.Add(value, 0f);
		}
	}

	public void Enable(bool val = true)
	{
		if (val && Owner == null)
		{
			Debug.LogError("can't enable stat without owner");
			return;
		}
		IsEnable_1 = val;
		if (IsEnable_1 && Owner.WeaponManager.CurrentWeapon != null)
		{
			SetWeapon(Owner.WeaponManager.CurrentWeapon);
		}
	}

	public void SetWeapon(Weapon weapon)
	{
		if (IsEnable && weapon != null)
		{
			WeaponType = weapon.TemplateId;
		}
	}

	public int GetShootByTarget(EnemyInfo person)
	{
		if (targetStats.TryGetValue(person.Person.ProfileId, out var value))
		{
			return value.TotalShots;
		}
		return 0;
	}

	public void ShootToTarget(EnemyInfo person, Vector3? pos = null)
	{
		GClass634 value;
		if (person != null && person.Person != null)
		{
			float magnitude = (Owner.Transform.position - person.Person.Transform.position).magnitude;
			if (targetStats.TryGetValue(person.Person.ProfileId, out value))
			{
				value.AddDistShot(magnitude);
			}
			else
			{
				value = new GClass634();
				value.AddDistShot(magnitude);
				targetStats.Add(person.Person.ProfileId, value);
			}
			Owner.Loyalty.MarkAsAggressor(person.Person);
		}
		else if (pos.HasValue)
		{
			float magnitude = (Owner.Transform.position - pos.Value).magnitude;
			if (targetStats.TryGetValue("Null_id", out value))
			{
				value.AddDistShot(magnitude);
				return;
			}
			value = new GClass634();
			value.AddDistShot(magnitude);
			targetStats.Add("Null_id", value);
		}
	}

	public void KillTarget(string killer, DamageInfoStruct info)
	{
		this.OnKillTarget?.Invoke(killer, info);
		if (IsEnable && !Frags.ContainsKey(killer))
		{
			Frags.Add(killer, new GClass627(info));
		}
	}

	public void HitTarget(IPlayer person, DamageInfoStruct damageInfo, EBodyPart bodyPart)
	{
		this.OnHitTarget?.Invoke(person, damageInfo, bodyPart);
		if (!IsEnable)
		{
			return;
		}
		try
		{
			TotalDamage += damageInfo.Damage;
			if (targetStats.TryGetValue(person.ProfileId, out var value))
			{
				float magnitude = (Owner.Transform.position - person.Transform.position).magnitude;
				value.AddDistHit(magnitude, (damageInfo.HitCollider != null) ? damageInfo.HitCollider.name : "Null", damageInfo.Damage, damageInfo.FireIndex, bodyPart, damageInfo.ArmorDamage);
			}
		}
		catch (Exception)
		{
			Debug.LogError(" Bot personal stat HitTarget NPE:" + damageInfo.ToString() + ";" + damageInfo.HitCollider?.ToString() + ";" + person?.ToString() + ";" + Owner?.ToString() + ";" + targetStats?.ToString() + ";");
		}
	}

	public void GrendateThrow(Vector3? trg)
	{
		if (IsEnable)
		{
			GrenadeTrowed++;
			if (trg.HasValue)
			{
				float magnitude = (Owner.Transform.position - trg.Value).magnitude;
				middleGrendaThrowDist = (middleGrendaThrowDist * (float)(GrenadeTrowed - 1) + magnitude) / (float)GrenadeTrowed;
			}
		}
	}

	public void Death(EDamageType damageType)
	{
		if (IsEnable)
		{
			if (damageType == EDamageType.GrenadeFragment)
			{
				KilledByGrenade++;
			}
			IsDead = true;
			LifeTime = Time.time - BornTime;
			SetStandByState(Owner.StandBy.StandByType);
		}
	}

	public void CoverTaken()
	{
		if (IsEnable)
		{
			TimeComeToCover = Time.time;
			CoverPointsTaken++;
		}
	}

	public void LeaveCover()
	{
		if (IsEnable)
		{
			float num = Time.time - TimeComeToCover;
			TotaTimeInCover += num;
			MiddleTimeInCover = (MiddleTimeInCover * (float)CoverPointsTaken + num) / (float)(CoverPointsTaken + 1);
		}
	}

	public void GetHit(DamageInfoStruct damageInfo, EBodyPart bodyType)
	{
		if (!IsEnable)
		{
			return;
		}
		if (damageInfo.Player != null && damageInfo.Player.iPlayer.Transform != null)
		{
			Vector3 position = damageInfo.Player.iPlayer.Transform.position;
			float magnitude = (Owner.Transform.position - position).magnitude;
			string profileId = damageInfo.Player.iPlayer.ProfileId;
			if (!HitsGain.TryGetValue(profileId, out var value))
			{
				value = new GClass634();
				HitsGain.Add(profileId, value);
			}
			string bone = ((damageInfo.HitCollider == null) ? "null" : damageInfo.HitCollider.name);
			value.AddDistHit(magnitude, bone, damageInfo.Damage, damageInfo.FireIndex, bodyType, damageInfo.ArmorDamage);
		}
		else
		{
			Debug.LogError("damage info without player or player transform " + (damageInfo.Player == null));
		}
	}

	public void AddGetVision(float distanceToEnemy, float visibilityK)
	{
		if (IsEnable)
		{
			gainSightStats.AddGainSIght(distanceToEnemy, visibilityK);
		}
	}

	public void Aim(Vector3 to, float time)
	{
		if (IsEnable)
		{
			float magnitude = (Owner.Transform.position - to).magnitude;
			AimingStats.AddAim(magnitude, time);
		}
	}

	public void SetStandByState(BotStandByType prevByType)
	{
		if (IsEnable)
		{
			float num = Time.time - CurStandByTime;
			StandByTimes[prevByType] += num;
			CurStandByTime = Time.time;
		}
	}

	public void method_0()
	{
		if (IsEnable && !IsWritten)
		{
			IsWritten = true;
			string contents = JsonParserClass.ToPrettyJson(this);
			string path = Owner.Profile.Id + STAT_INDEX + ".botStat";
			STAT_INDEX++;
			File.WriteAllText(path, contents);
		}
	}

	public void Dispose()
	{
		if (!IsEnable || IsDead)
		{
			return;
		}
		try
		{
			SetStandByState(Owner.StandBy.StandByType);
			LifeTime = Time.time - BornTime;
		}
		catch (Exception)
		{
			string text = "";
			if (Owner == null)
			{
				text = "owner is null";
			}
			else if (Owner.StandBy == null)
			{
				text = "standby is null. BotState:" + Owner.BotState;
			}
			Debug.LogError("DISPOSE bot stat error " + text);
		}
	}
}
