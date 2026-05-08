using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Comfort.Common;
using EFT;
using UnityEngine;

public class BotCreatorClass : IBotCreator
{
	[Serializable]
	[CompilerGenerated]
	public class Class561
	{
		public static readonly Class561 class561_0 = new Class561();

		public static Func<Renderer, bool> func_0;

		public bool method_0(Renderer x)
		{
			return x.enabled;
		}
	}

	[CompilerGenerated]
	public class Class562
	{
		public BotCreationDataClass data;

		public BotCreatorClass BotCreatorClass;

		public BotZone zone;

		public Action<BotOwner> callback;

		public Func<BotOwner, BotZone, BotsGroup> groupAction;

		public void method_0(BotOwner bot)
		{
			bot.SpawnProfileData = data._profileData;
			BotCreatorClass.method_3(zone, bot, callback, groupAction);
		}
	}

	[CompilerGenerated]
	public class Class563
	{
		public BotCreatorClass BotCreatorClass;

		public BotZone zone;

		public Action<BotOwner> callback;

		public Func<BotOwner, BotZone, BotsGroup> groupAction;

		public void method_0(BotOwner bot)
		{
			BotCreatorClass.method_3(zone, bot, callback, groupAction);
		}
	}

	[NonSerialized]
	public IBotGame IbotGame_0;

	[NonSerialized]
	public GInterface21 Ginterface21_0;

	[NonSerialized]
	public Func<GameWorld, Profile, Vector3, Task<LocalPlayer>> Func_0;

	[NonSerialized]
	public Dictionary<Player, List<Renderer>> Dictionary_0 = new Dictionary<Player, List<Renderer>>();

	[NonSerialized]
	[CompilerGenerated]
	public int Int_0;

	public int BotsLoading
	{
		[CompilerGenerated]
		get
		{
			return Int_0;
		}
		[CompilerGenerated]
		set
		{
			Int_0 = value;
		}
	}

	public bool StartProfilesLoaded => Ginterface21_0.StartProfilesLoaded;

	public int BundlesLoading => Ginterface21_0.BundlesLoading;

	public BotCreatorClass(IBotGame game, GInterface21 profileCreator, Func<GameWorld, Profile, Vector3, Task<LocalPlayer>> playerCreator)
	{
		BotsLoading = 0;
		IbotGame_0 = game;
		Ginterface21_0 = profileCreator;
		Func_0 = playerCreator;
	}

	public async Task method_0(BotCreationDataClass data, bool shallBeGroup, Action<BotOwner> callback, CancellationToken cancellationToken, bool withDelete)
	{
		foreach (Profile profile in data.Profiles)
		{
			if (shallBeGroup)
			{
				profile.Info.Settings.TryChangeRoleToAssaultGroup();
			}
			if (cancellationToken.IsCancellationRequested || data.SpawnStopped)
			{
				return;
			}
			if (profile != null)
			{
				if (shallBeGroup)
				{
					profile.Info.Settings.TryChangeRoleToAssaultGroup();
				}
				GClass682 position = data.GetPosition();
				if (position != null)
				{
					await method_2(profile, position, callback, isLocalGame: true, cancellationToken);
				}
			}
		}
	}

	public async Task method_1(Profile profile, GClass682 position, bool shallBeGroup, Action<BotOwner> callback, CancellationToken cancellationToken)
	{
		await Task.Yield();
		if (profile != null)
		{
			if (shallBeGroup)
			{
				profile.Info.Settings.TryChangeRoleToAssaultGroup();
			}
			method_2(profile, position, callback, isLocalGame: true, cancellationToken).HandleExceptions();
		}
		else
		{
			Debug.LogError("Can't SpawnBot cause bad profile");
		}
	}

	public async Task method_2(Profile profile, GClass682 bornInfo, Action<BotOwner> callback, bool isLocalGame, CancellationToken cancellationToken)
	{
		LocalPlayer localPlayer = await Func_0(Singleton<GameWorld>.Instance, profile, bornInfo.position);
		if (!(localPlayer == null) && !cancellationToken.IsCancellationRequested)
		{
			AICorePoint corePoint = IbotGame_0.BotsController.CoversData.AICorePointsHolder.GetCorePoint(bornInfo.CorePointId);
			BotOwner botOwner = BotOwner.Create(localPlayer, null, IbotGame_0.GameDateTime, IbotGame_0.BotsController, isLocalGame, corePoint);
			method_4(botOwner.GetPlayer);
			method_5(botOwner, @switch: false);
			botOwner.GetComponentsInChildren<Collider>();
			botOwner.GetPlayer.CharacterController.isEnabled = false;
			callback(botOwner);
		}
	}

	public async Task ActivateBot(BotCreationDataClass data, BotZone zone, bool shallBeGroup, Func<BotOwner, BotZone, BotsGroup> groupAction, Action<BotOwner> callback, CancellationToken cancellationToken)
	{
		await method_0(data, shallBeGroup, delegate(BotOwner bot)
		{
			bot.SpawnProfileData = data._profileData;
			method_3(zone, bot, callback, groupAction);
		}, cancellationToken, withDelete: true);
	}

	public Task ActivateBot(Profile profile, GClass682 position, BotZone zone, bool shallBeGroup, Func<BotOwner, BotZone, BotsGroup> groupAction, Action<BotOwner> callback, CancellationToken cancellationToken)
	{
		return method_1(profile, position, shallBeGroup, delegate(BotOwner bot)
		{
			method_3(zone, bot, callback, groupAction);
		}, cancellationToken);
	}

	public void FillBackupProfilesData(GClass406 resultCache)
	{
		Ginterface21_0.FillBackupProfilesData(resultCache);
	}

	public void AddToTargetBackup(BotDifficulty difficulty, WildSpawnType role, int count)
	{
		Ginterface21_0.AddToTargetBackup(difficulty, role, count);
	}

	public void method_3(BotZone zone, BotOwner bot, Action<BotOwner> callback, Func<BotOwner, BotZone, BotsGroup> groupAction)
	{
		Player getPlayer = bot.GetPlayer;
		if (getPlayer != null && getPlayer.CharacterController != null)
		{
			getPlayer.CharacterController.isEnabled = true;
		}
		bot.GetPlayer.MovementContext.ResetFlying();
		AICoversData coversData = IbotGame_0.BotsController.CoversData;
		bool autoActivate = IbotGame_0.Status == GameStatus.Starting || IbotGame_0.Status == GameStatus.Started;
		bot.PreActivate(zone, IbotGame_0.GameDateTime, groupAction(bot, zone), coversData, autoActivate);
		method_5(bot, @switch: true);
		callback(bot);
	}

	public void method_4(Player player)
	{
		Dictionary_0.Add(player, (from x in player.GetComponentsInChildren<Renderer>()
			where x.enabled
			select x).ToList());
	}

	public void method_5(BotOwner owner, bool @switch)
	{
		if (!Dictionary_0.TryGetValue(owner.GetPlayer, out var value) || value == null)
		{
			return;
		}
		foreach (Renderer item in value)
		{
			item.enabled = @switch;
		}
	}

	public async Task<Profile> GenerateProfile(BotCreationDataClass data, CancellationToken cancellationToken, bool withDelete)
	{
		int num = 1;
		BotsLoading += num;
		Profile result = await Ginterface21_0.CreateProfile(data, cancellationToken, withDelete);
		BotsLoading -= num;
		return result;
	}
}
