using System;
using System.Diagnostics;
using EFT;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;

public class BotSettingsComponents
{
	public BotGlobalLayData Lay = new BotGlobalLayData();

	public BotGlobalAimingSettings Aiming = new BotGlobalAimingSettings();

	public BotGlobalLookData Look = new BotGlobalLookData();

	public BotGlobalShootData Shoot = new BotGlobalShootData();

	public BotGlobalsMoveSettings Move = new BotGlobalsMoveSettings();

	public BotGlobalsGrenadeSettings Grenade = new BotGlobalsGrenadeSettings();

	public BotGlobalsChangeSettings Change = new BotGlobalsChangeSettings();

	public BotGlobalsCoverSettings Cover = new BotGlobalsCoverSettings();

	public BotGlobalPatrolSettings Patrol = new BotGlobalPatrolSettings();

	public BotGlobasHearingSettings Hearing = new BotGlobasHearingSettings();

	public BotGlobalsMindSettings Mind = new BotGlobalsMindSettings();

	public BotGlobalsBossSettings Boss = new BotGlobalsBossSettings();

	public BotGlobalsCoreSettingsClass Core = new BotGlobalsCoreSettingsClass();

	public BotGlobalsScatteringSettings Scattering = new BotGlobalsScatteringSettings();

	public static BotSettingsComponents Create(string pvp, [CanBeNull] string pve, BotDifficulty d, WildSpawnType role)
	{
		string json = pvp;
		if (pve != null)
		{
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			pve = pve.Replace("\n", "");
			pve = pve.Replace("\\", "");
			pvp = GClass619.DoMerge(pve, pvp, "Mind");
			pvp = GClass619.DoMerge(pve, pvp, "Lay");
			pvp = GClass619.DoMerge(pve, pvp, "Aiming");
			pvp = GClass619.DoMerge(pve, pvp, "Look");
			pvp = GClass619.DoMerge(pve, pvp, "Shoot");
			pvp = GClass619.DoMerge(pve, pvp, "Move");
			pvp = GClass619.DoMerge(pve, pvp, "Grenade");
			pvp = GClass619.DoMerge(pve, pvp, "Cover");
			pvp = GClass619.DoMerge(pve, pvp, "Patrol");
			pvp = GClass619.DoMerge(pve, pvp, "Hearing");
			pvp = GClass619.DoMerge(pve, pvp, "Boss");
			pvp = GClass619.DoMerge(pve, pvp, "Change");
			pvp = GClass619.DoMerge(pve, pvp, "Scattering");
			stopwatch.Stop();
		}
		Stopwatch stopwatch2 = new Stopwatch();
		stopwatch2.Start();
		BotSettingsComponents botSettingsComponents;
		try
		{
			botSettingsComponents = JsonParserClass.ParseJsonTo<BotSettingsComponents>(pvp, Array.Empty<JsonConverter>());
			if (!Application.isPlaying)
			{
				UnityEngine.Debug.LogError("Merge complete:" + d.ToString() + "  role:" + role);
			}
		}
		catch (Exception)
		{
			botSettingsComponents = JsonParserClass.ParseJsonTo<BotSettingsComponents>(json, Array.Empty<JsonConverter>());
		}
		stopwatch2.Stop();
		botSettingsComponents.Aiming.Update();
		botSettingsComponents.Look.Update();
		botSettingsComponents.Shoot.Update();
		botSettingsComponents.Move.Update();
		botSettingsComponents.Grenade.Update();
		botSettingsComponents.Cover.Update();
		botSettingsComponents.Patrol.Update();
		botSettingsComponents.Hearing.Update();
		botSettingsComponents.Mind.Update();
		botSettingsComponents.Lay.Update();
		botSettingsComponents.Boss.Update();
		botSettingsComponents.Change.Update();
		botSettingsComponents.Scattering.Update();
		return botSettingsComponents;
	}

	public BotSettingsComponents Copy()
	{
		return new BotSettingsComponents
		{
			Aiming = Aiming,
			Look = Look,
			Shoot = Shoot,
			Move = Move,
			Grenade = Grenade,
			Cover = Cover,
			Patrol = Patrol,
			Hearing = Hearing,
			Mind = Mind,
			Lay = Lay,
			Boss = Boss,
			Change = Change,
			Scattering = Scattering,
			Core = new BotGlobalsCoreSettingsClass(Core)
		};
	}
}
