using System;
using System.Collections.Generic;
using System.Text;
using EFT;
using JetBrains.Annotations;
using UnityEngine;

public class GClass475([NotNull] BotOwner owner) : BotEnemyChooser(owner)
{
	public const float PERIOD_OF_BOSS_ATTACK_TO_START_FIGHT = 15f;

	[NonSerialized]
	public HashSet<int> HashSet_0 = new HashSet<int>();

	[NonSerialized]
	public StringBuilder StringBuilder_0;

	[NonSerialized]
	public int Int_0;

	public override EnemyInfo FindDangerEnemy()
	{
		if (!BotOwner_0.BotFollower.HaveBoss)
		{
			return base.FindDangerEnemy();
		}
		if (!BotOwner_0.BotFollower.BossToFollow.Player().HealthController.IsAlive)
		{
			return base.FindDangerEnemy();
		}
		if (HashSet_0.Count > 0)
		{
			HashSet_0.Clear();
		}
		if (BotOwner_0.BotFollower.BossToFollow.IsAI)
		{
			BotOwner botOwner = BotOwner_0.BotFollower.BossToFollow.Player().AIData.BotOwner;
			if (botOwner.Boss.BossLogic is ZyriachyBossLogicClass zyriachyBossLogicClass)
			{
				if (botOwner.Memory.HaveEnemy && !zyriachyBossLogicClass.HaveHitFrom(botOwner.Memory.GoalEnemy.Person.Id))
				{
					method_2(botOwner.Memory.GoalEnemy);
				}
				if (method_1())
				{
					foreach (BotOwner follower in botOwner.Boss.Followers)
					{
						if (follower.Id != BotOwner_0.Id && follower.Memory.HaveEnemy)
						{
							IPlayer person = follower.Memory.GoalEnemy.Person;
							if (!zyriachyBossLogicClass.HaveHitFrom(botOwner.Memory.GoalEnemy.Person.Id))
							{
								HashSet_0.Add(person.Id);
							}
						}
					}
				}
			}
		}
		bool flag = GClass398.Instance.IsTraceEnable();
		StringBuilder_0 = new StringBuilder();
		if (flag)
		{
			StringBuilder_0.Append($"prepare find enemy for zryachiy:{BotOwner_0.Id} ");
		}
		List<EnemyInfo> list = new List<EnemyInfo>();
		foreach (EnemyInfo value in BotOwner_0.EnemiesController.EnemyInfos.Values)
		{
			if (flag)
			{
				StringBuilder_0.AppendLine();
				StringBuilder_0.Append(" Enemy " + value.Person.Profile.Nickname + " ");
			}
			if (!value.Person.HealthController.IsAlive)
			{
				if (flag)
				{
					StringBuilder_0.Append("isDead");
				}
			}
			else if (!value.HaveSeen)
			{
				if (flag)
				{
					StringBuilder_0.Append("not HaveSeen");
				}
			}
			else if (!value.ShallKnowEnemy())
			{
				if (flag)
				{
					StringBuilder_0.Append("not ShallKnowEnemy");
				}
			}
			else if (HashSet_0.Contains(value.Person.Id))
			{
				if (flag)
				{
					StringBuilder_0.Append(" _ignoreId");
				}
			}
			else if (!method_0(value))
			{
				if (flag)
				{
					StringBuilder_0.Append(" not SpendEnought");
				}
			}
			else
			{
				list.Add(value);
			}
		}
		if (flag)
		{
			int hashCode = StringBuilder_0.ToString().GetHashCode();
			Int_0 = hashCode;
		}
		return BetterEnemy(list, flag);
	}

	public override float CalcWeight(float dist, EnemyInfo enemyInfo)
	{
		float num = dist;
		if (!enemyInfo.IsVisible)
		{
			num += 1000000f;
		}
		if (!enemyInfo.CanShoot)
		{
			num += 1000000f;
		}
		return num;
	}

	public bool method_0(EnemyInfo info)
	{
		if (info is GClass541 gClass)
		{
			if (BotOwner_0.BotFollower.BossToFollow.Player().AIData.BotOwner.Boss.BossLogic is ZyriachyBossLogicClass zyriachyBossLogicClass && zyriachyBossLogicClass.HaveHitFrom(info.Person.Id))
			{
				return true;
			}
			return gClass.SpendedAtAttackZone(3f);
		}
		return true;
	}

	public bool method_1()
	{
		return false;
	}

	public void method_2(EnemyInfo bossEnemyInfo)
	{
		if (bossEnemyInfo is GClass541 gClass && (gClass.CloseZone || gClass.CanShootCauseTime()))
		{
			return;
		}
		IPlayer person = bossEnemyInfo.Person;
		if (!BotOwner_0.EnemiesController.EnemyInfos.TryGetValue(person, out var value))
		{
			return;
		}
		if (value.Distance > 15f && bossEnemyInfo.CanShoot && bossEnemyInfo.IsVisible)
		{
			if (bossEnemyInfo.PersonalLastShootTime < 0f)
			{
				if (Time.time - bossEnemyInfo.LastChangeVisionTime < 15f)
				{
					HashSet_0.Add(person.Id);
				}
			}
			else
			{
				method_3(bossEnemyInfo, person);
			}
		}
		else if (bossEnemyInfo.PersonalLastShootTime < 0f)
		{
			HashSet_0.Add(person.Id);
		}
		else
		{
			method_3(bossEnemyInfo, person);
		}
	}

	public void method_3(EnemyInfo bossEnemyInfo, IPlayer person)
	{
		if (bossEnemyInfo.FirstTimeShoot > 0f)
		{
			if (Time.time - bossEnemyInfo.FirstTimeShoot < 15f)
			{
				HashSet_0.Add(person.Id);
			}
		}
		else if (bossEnemyInfo is GClass541 gClass && gClass.SpendedAtAttackZone(3f))
		{
			HashSet_0.Add(person.Id);
		}
	}
}
