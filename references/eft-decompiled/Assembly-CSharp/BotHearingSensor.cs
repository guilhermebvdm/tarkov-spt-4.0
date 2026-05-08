using System;
using System.Runtime.CompilerServices;
using Comfort.Common;
using EFT;
using UnityEngine;

public class BotHearingSensor
{
	[CompilerGenerated]
	public class Class286
	{
		public BotHearingSensor botHearingSensor_0;

		public IPlayer player;

		public Vector3 position;

		public float power;

		public AISoundType type;

		public void method_0()
		{
			if (!botHearingSensor_0.BotOwner.IsDead)
			{
				botHearingSensor_0.method_1(player, position, power, type);
			}
		}
	}

	[NonSerialized]
	public BotOwner BotOwner;

	[NonSerialized]
	public BotDifficultySettingsClass BotSettings;

	public GDelegate7 OnEnemySounHearded;

	public BotHearingSensor(BotOwner onwer)
	{
		BotOwner = onwer;
		BotSettings = BotOwner.Settings;
	}

	public void Init()
	{
		Singleton<BotEventHandler>.Instance.OnSoundPlayed += method_0;
	}

	public void method_0(IPlayer player, Vector3 position, float power, AISoundType type)
	{
		if (player.AIData.IsMute)
		{
			return;
		}
		if (BotOwner.Memory.GoalEnemy != null)
		{
			method_1(player, position, power, type);
			return;
		}
		float num = ((!BotOwner.Memory.GoalTarget.HavePlaceTarget()) ? BotOwner.Settings.FileSettings.Hearing.HEAR_DELAY_WHEN_PEACE : BotOwner.Settings.FileSettings.Hearing.HEAR_DELAY_WHEN_HAVE_SMT);
		if (num > 0f)
		{
			BotOwner.AITaskManager.RegisterDelayedTask(BotOwner, num, delegate
			{
				if (!BotOwner.IsDead)
				{
					method_1(player, position, power, type);
				}
			});
		}
		else
		{
			method_1(player, position, power, type);
		}
	}

	public void method_1(IPlayer player, Vector3 position, float power, AISoundType type)
	{
		if (player != null && BotOwner.GetPlayer == Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(player.ProfileId))
		{
			return;
		}
		float dist = 0f;
		bool flag = false;
		if (player != null)
		{
			if (!BotOwner.BotsGroup.IsEnemy(player))
			{
				if ((type == AISoundType.gun || type == AISoundType.silencedGun) && BotOwner.BotsGroup.Neutrals.ContainsKey(player))
				{
					flag = true;
				}
				if (!flag && !BotOwner.BotsGroup.Enemies.ContainsKey(player))
				{
					return;
				}
			}
			if (flag)
			{
				BotOwner.BotsGroup.LastSoundsController.AddNeutralSound(player, position);
				return;
			}
			bool wasHeard = method_6(position, power, out dist);
			if (type == AISoundType.step)
			{
				if (method_7(dist))
				{
					method_2(player, position, power, wasHeard, type);
				}
				return;
			}
			if (dist < BotOwner.Settings.FileSettings.Cover.SOUND_TO_GET_SPOTTED)
			{
				BotOwner.Memory.Spotted(byHit: false);
			}
			method_2(player, position, power, wasHeard, type);
		}
		else
		{
			bool wasHeard2 = method_6(position, power, out dist);
			method_2(null, position, power, wasHeard2, type);
		}
	}

	public void method_2(IPlayer enemy, Vector3 pos, float power, bool wasHeard, AISoundType type)
	{
		if (enemy != null && enemy.AIData.IsAI && BotOwner.BotsGroup.Contains(enemy.AIData.BotOwner))
		{
			return;
		}
		Vector3 bDir = BotOwner.Transform.position - pos;
		float magnitude = bDir.magnitude;
		if (wasHeard)
		{
			float num = ((type == AISoundType.step || (uint)(type - 1) > 1u) ? (magnitude / BotOwner.Settings.FileSettings.Hearing.DISPERSION_COEF) : (magnitude / BotOwner.Settings.FileSettings.Hearing.DISPERSION_COEF_GUN));
			float num2 = GClass856.Random(0f - num, num);
			float num3 = GClass856.Random(0f - num, num);
			Vector3 vector = new Vector3(pos.x + num2, pos.y, pos.z + num3);
			PlaceForCheck placeForCheck = BotOwner.BotsGroup.AddPointToSearch(vector, power, BotOwner);
			if (placeForCheck == null)
			{
				return;
			}
			if (magnitude < BotOwner.Settings.FileSettings.Hearing.RESET_TIMER_DIST)
			{
				BotOwner.LookData.ResetUpdateTime();
			}
			OnEnemySounHearded?.Invoke(vector, magnitude, type);
			if (enemy != null && magnitude < BotOwner.Settings.FileSettings.Mind.BULLET_FEEL_DIST && (type == AISoundType.gun || type == AISoundType.silencedGun))
			{
				if (!(magnitude < BotOwner.Settings.FileSettings.Hearing.BOT_CLOSE_PANIC_DIST) && !method_5(BotOwner, bDir.magnitude, enemy.LookDirection, bDir))
				{
					BotOwner.Memory.SetPanicPoint(placeForCheck, realDanger: false);
				}
				else
				{
					Vector3 to = pos + enemy.LookDirection;
					bool flag = method_3(pos, to, BotOwner.Settings.FileSettings.Mind.BULLET_FEEL_CLOSE_SDIST);
					BotOwner.Memory.SetPanicPoint(placeForCheck, flag);
					if (flag)
					{
						if (BotOwner.Memory.IsInCover)
						{
							BotOwner.Memory.BotCurrentCoverInfo.AddShootDirection();
						}
						BotOwner.Memory.SetUnderFire(enemy);
						if (magnitude > BotOwner.Settings.FileSettings.Hearing.ENEMY_SNIPER_SHOOT_DIST)
						{
							BotOwner.BotTalk.Say(EPhraseTrigger.SniperPhrase);
						}
					}
				}
			}
			if (!BotOwner.Memory.GoalTarget.HavePlaceTarget() && BotOwner.Memory.GoalEnemy == null)
			{
				BotOwner.BotsGroup.CalcGoalForBot(BotOwner);
			}
		}
		else
		{
			if ((type != AISoundType.gun && type != AISoundType.silencedGun) || !(magnitude < BotOwner.Settings.FileSettings.Mind.BULLET_FEEL_DIST) || enemy == null)
			{
				return;
			}
			Vector3 to2 = pos + enemy.LookDirection;
			if (method_3(pos, to2, BotOwner.Settings.FileSettings.Mind.BULLET_FEEL_CLOSE_SDIST))
			{
				if (BotOwner.Memory.IsInCover)
				{
					BotOwner.Memory.Spotted(byHit: false);
				}
				PlaceForCheck placeForCheck2 = BotOwner.BotsGroup.AddPointToSearch(pos, power, BotOwner);
				if (placeForCheck2 != null)
				{
					BotOwner.Memory.SetPanicPoint(placeForCheck2, realDanger: false);
				}
			}
		}
	}

	public bool method_3(Vector3 from, Vector3 to, float maxSDist)
	{
		return (GClass369.GetProjectionPoint(BotOwner.Position + Vector3.up, from, to) - BotOwner.Position).sqrMagnitude < maxSDist;
	}

	public bool method_4(Vector3 from, Vector3 to, Vector3 dir, float dist, float maxSDist)
	{
		from += BotOwner.STAY_HEIGHT;
		to += BotOwner.STAY_HEIGHT;
		if (Physics.Raycast(new Ray(from, dir), out var hitInfo, dist, LayerMaskClass.HighPolyWithTerrainMask))
		{
			return (hitInfo.point - to).sqrMagnitude < maxSDist;
		}
		return true;
	}

	public bool method_5(BotOwner bot, float bDist, Vector3 dir, Vector3 bDir)
	{
		if (Vector3.Dot(dir, bDir) < 0f)
		{
			return false;
		}
		float num = Vector3.Angle(dir, bDir);
		if (num < bot.Settings.FileSettings.Hearing.SOUND_DIR_DEEFREE)
		{
			return bDist * Mathf.Tan(num * (MathF.PI / 180f)) < 23f;
		}
		return false;
	}

	public bool method_6(Vector3 position, float power, out float dist)
	{
		if (BotOwner.IsDead)
		{
			dist = 0f;
			return false;
		}
		dist = (BotOwner.Transform.position - position).magnitude;
		float num = BotSettings.Current.CurrentHearingSense * power;
		return dist < num;
	}

	public bool method_7(float d)
	{
		if (d < BotOwner.Settings.Current.CloseHearingSense)
		{
			return true;
		}
		if (d > BotOwner.Settings.Current.FarHearingSense)
		{
			return false;
		}
		float num = BotOwner.Settings.Current.FarHearingSense - BotOwner.Settings.Current.CloseHearingSense;
		float num2 = d - BotOwner.Settings.Current.CloseHearingSense;
		float num3 = 1f - num2 / num;
		return GClass856.Random(0f, 1f) < num3;
	}

	public void Dispose()
	{
		Singleton<BotEventHandler>.Instance.OnSoundPlayed -= method_0;
	}
}
