using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Comfort.Common;
using CommonAssets.Scripts.ArtilleryShelling;
using EFT;
using EFT.GlobalEvents;
using EFT.GlobalEvents.ArtilleryShellingEcents;
using EFT.InventoryLogic;
using UnityEngine;

public class ServerShellingControllerClass : IDisposable
{
	[Serializable]
	[CompilerGenerated]
	public class Class299
	{
		public static readonly Class299 class299_0 = new Class299();

		public static Func<ArtilleryShellingZone, bool> func_0;

		public bool method_0(ArtilleryShellingZone x)
		{
			return x.UseInCalledShelling;
		}
	}

	[CompilerGenerated]
	public class Class300
	{
		public string zoneId;

		public bool method_0(ArtilleryShellingZone x)
		{
			return x.ID == zoneId;
		}
	}

	[CompilerGenerated]
	public class Class301
	{
		public GClass644 process;

		public bool method_0(ArtilleryBrigade x)
		{
			return x.ID == process.shellingBrigadeID;
		}
	}

	[CompilerGenerated]
	public class Class302
	{
		public ArtilleryServerProjectileClass serverProjectile;

		public bool method_0(ArtilleryShellingZone x)
		{
			return x.ID == serverProjectile.zoneID;
		}

		public bool method_1(KeyValuePair<ArtilleryShellingZone, GClass644> x)
		{
			return x.Key.ID == serverProjectile.zoneID;
		}

		public bool method_2(ArtilleryShellingZone x)
		{
			return x.ID == serverProjectile.zoneID;
		}

		public bool method_3(KeyValuePair<ArtilleryShellingZone, GClass645> x)
		{
			return x.Key.ID == serverProjectile.zoneID;
		}
	}

	[NonSerialized]
	public BackendConfigSettingsClass.ArtilleryShellingGlobalSettings ArtilleryShellingGlobalSettings_0;

	[NonSerialized]
	public ArtilleryShellingMapConfiguration ArtilleryShellingMapConfiguration_0;

	[NonSerialized]
	public List<ArtilleryBrigade> List_0;

	[NonSerialized]
	public ArtilleryBrigade ArtilleryBrigade_0;

	[NonSerialized]
	public Dictionary<ArtilleryShellingZone, GClass644> Dictionary_0 = new Dictionary<ArtilleryShellingZone, GClass644>();

	[NonSerialized]
	public Dictionary<ArtilleryShellingZone, GClass645> Dictionary_1 = new Dictionary<ArtilleryShellingZone, GClass645>();

	[NonSerialized]
	public List<ArtilleryShellingZone> List_1 = new List<ArtilleryShellingZone>();

	[NonSerialized]
	public List<(ArtilleryShellingZone, GClass644)> List_2 = new List<(ArtilleryShellingZone, GClass644)>();

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_0;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_1;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_2;

	[NonSerialized]
	public bool Bool_3;

	[NonSerialized]
	public GameWorld GameWorld_0;

	[NonSerialized]
	public GClass1507 Gclass1507_0;

	[NonSerialized]
	public const int Int_0 = 10;

	public GClass818<int, ArtilleryServerProjectileClass> ActiveProjectiles = new GClass818<int, ArtilleryServerProjectileClass>();

	[NonSerialized]
	public const float Float_0 = 300f;

	[NonSerialized]
	public const float Float_1 = 400f;

	[NonSerialized]
	public float Float_2;

	[NonSerialized]
	public int Int_1 = 60;

	[NonSerialized]
	public Vector3 Vector3_0;

	[NonSerialized]
	public float Float_3 = 3f;

	[NonSerialized]
	public float Float_4;

	[NonSerialized]
	public bool Bool_4;

	[NonSerialized]
	public List<int> List_3 = new List<int>();

	[NonSerialized]
	public Queue<string> Queue_0 = new Queue<string>();

	[NonSerialized]
	public int Int_2;

	[NonSerialized]
	public float Float_5;

	[NonSerialized]
	public float Float_6;

	[NonSerialized]
	public float Float_7;

	[NonSerialized]
	public float Float_8;

	[NonSerialized]
	public CancellationTokenSource CancellationTokenSource_0 = new CancellationTokenSource();

	[NonSerialized]
	public CancellationToken CancellationToken_0;

	[NonSerialized]
	public List<ArtilleryPacketStruct> List_4 = new List<ArtilleryPacketStruct>();

	[NonSerialized]
	public ArtilleryPacketStruct ArtilleryPacketStruct;

	[NonSerialized]
	public Action Action_0;

	[NonSerialized]
	public MongoID MongoID_0 = new MongoID("6655e35b6bc645cb7b059912");

	[NonSerialized]
	public bool Bool_5;

	[NonSerialized]
	public HashSet<int> HashSet_0 = new HashSet<int>();

	[NonSerialized]
	public HashSet<int> HashSet_1 = new HashSet<int>();

	[NonSerialized]
	public static IEnumerable<EquipmentSlot> Ienumerable_0 = new EquipmentSlot[2]
	{
		EquipmentSlot.ArmorVest,
		EquipmentSlot.Pockets
	};

	public ArtilleryShellingMapConfiguration CurrentMapConfiguration => ArtilleryShellingMapConfiguration_0;

	public bool ShellingOn
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

	public bool TargetShellingOn
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

	public bool PauseOn
	{
		[CompilerGenerated]
		get
		{
			return Bool_2;
		}
		[CompilerGenerated]
		set
		{
			Bool_2 = value;
		}
	}

	public static ServerShellingControllerClass Instance
	{
		get
		{
			if (!(Singleton<GameWorld>.Instance == null))
			{
				return Singleton<GameWorld>.Instance.ServerShellingController;
			}
			return null;
		}
	}

	public ServerShellingControllerClass()
	{
		method_0(CancellationTokenSource_0.Token).HandleExceptions();
	}

	public async Task method_0(CancellationToken cancellationToken)
	{
		GameWorld_0 = Singleton<GameWorld>.Instance;
		if (GameWorld_0.LocationId != "develop")
		{
			while (Singleton<AbstractGame>.Instance.Status != GameStatus.Started)
			{
				await Task.Yield();
			}
		}
		ArtilleryShellingGlobalSettings_0 = Singleton<BackendConfigSettingsClass>.Instance.ArtilleryShelling;
		if (ArtilleryShellingGlobalSettings_0.ArtilleryMapsConfigs.Keys.Contains(GameWorld_0.LocationId))
		{
			ArtilleryShellingMapConfiguration_0 = ArtilleryShellingGlobalSettings_0.ArtilleryMapsConfigs[GameWorld_0.LocationId];
		}
		method_23(10);
		method_5();
		method_1();
		LocalGame localGame = Singleton<AbstractGame>.Instance as LocalGame;
		if (localGame != null)
		{
			Bool_3 = true;
			localGame.UpdateByUnity += OnUpdate;
		}
		if (ArtilleryShellingMapConfiguration_0.PlanedShellingOn && ArtilleryShellingMapConfiguration_0.ShellingZones.Length != 0)
		{
			Int_1 = ArtilleryShellingMapConfiguration_0.ArtilleryShellingAirDropSettings.AirDropTime;
			Vector3_0 = ArtilleryShellingMapConfiguration_0.ArtilleryShellingAirDropSettings.AirDropPosition;
			if (ArtilleryShellingMapConfiguration_0.ArtilleryShellingAirDropSettings.UseAirDrop)
			{
				InitAirdrop(Int_1, Vector3_0).HandleExceptions();
			}
			await method_8(ArtilleryShellingMapConfiguration_0.InitShellingTimer);
		}
	}

	public void method_1()
	{
		Action_0 = (Action)Delegate.Combine(Action_0, GlobalEventHandlerClass.Instance.SubscribeOnEvent<GClass3553>(method_4));
	}

	public void method_2()
	{
		GameWorld instance = Singleton<GameWorld>.Instance;
		if (instance == null)
		{
			return;
		}
		foreach (Player allAlivePlayers in instance.AllAlivePlayersList)
		{
			if (!(allAlivePlayers == null) && allAlivePlayers.AbstractQuestControllerClass != null && allAlivePlayers.IsYourPlayer && method_3(allAlivePlayers, out var position))
			{
				PositionInShellingZoneCheck(position, out var inShellingPos, out var _);
				if (inShellingPos)
				{
					allAlivePlayers.AbstractQuestControllerClass?.CheckUnderArtillery();
				}
			}
		}
	}

	public bool method_3(IPlayer player, out Vector3 position)
	{
		position = default(Vector3);
		if (player == null)
		{
			return false;
		}
		if (!player.HealthController.IsAlive)
		{
			return false;
		}
		if (player.PlayerBones == null)
		{
			return false;
		}
		if (player.PlayerBones.BodyTransform == null)
		{
			return false;
		}
		position = player.Position;
		return true;
	}

	public void method_4(GClass3553 flareSuccessEvent)
	{
		if (flareSuccessEvent.AmmoTemplate.ContainsType(FlareEventType.CallArtilleryOnMyself) && !(flareSuccessEvent.FiredPlayer.PlayerBones == null))
		{
			if (Dictionary_0.Count + Dictionary_1.Count >= ArtilleryShellingGlobalSettings_0.MaxCalledShellingCount)
			{
				NotifyEvent notifyEvent = GlobalEventHandlerClass.CreateEvent<NotifyEvent>();
				notifyEvent.MessageType = NotifyEvent.EMessageType.ArtilleryBusy;
				notifyEvent.ReceiverId = flareSuccessEvent.FiredPlayer.Id;
				notifyEvent.Invoke();
			}
			else
			{
				Vector3 position = flareSuccessEvent.FiredPlayer.Transform.position;
				StartShellingPosition(position);
			}
		}
	}

	public void method_5()
	{
	}

	public void method_6()
	{
		Action_0?.Invoke();
		Action_0 = null;
	}

	public void OnUpdate()
	{
		if (ShellingOn && !PauseOn)
		{
			method_10();
		}
		method_11();
		method_35();
		if (Bool_3)
		{
			method_9();
		}
		method_34();
		if (ActiveProjectiles.Count == 0)
		{
			method_7();
		}
	}

	public void method_7()
	{
		if (!ShellingOn && Bool_5)
		{
			method_15();
		}
		if (!TargetShellingOn && Dictionary_1.Count == 0)
		{
			method_16();
		}
	}

	public void StartShellingPosition(Vector3 position)
	{
		ArtilleryShellingZone zone = ArtilleryShellingMapConfiguration_0.ShellingZones.FirstOrDefault((ArtilleryShellingZone x) => x.UseInCalledShelling);
		if (!zone.Equals(null))
		{
			ref string iD = ref zone.ID;
			iD = iD + "_" + UnityEngine.Random.Range(0, int.MaxValue);
			zone.Center = position;
			method_25(ref zone);
			StartEventShelling(zone, ArtilleryShellingMapConfiguration_0.InitCalledShellingTime).HandleExceptions();
		}
	}

	public void StartImmediateShellingZone(string zoneId)
	{
		bool flag = false;
		foreach (KeyValuePair<ArtilleryShellingZone, GClass645> item in Dictionary_1)
		{
			if (item.Key.ID == zoneId)
			{
				flag = true;
			}
		}
		if (!flag)
		{
			ArtilleryShellingZone zone = ArtilleryShellingMapConfiguration_0.ShellingZones.First((ArtilleryShellingZone x) => x.ID == zoneId);
			StartEventShelling(zone, ArtilleryShellingMapConfiguration_0.InitShellingTimer).HandleExceptions();
		}
	}

	public async Task method_8(float secondsDelay)
	{
		Float_5 = 0f;
		Float_6 = secondsDelay;
		while (!(Float_5 >= Float_6 - ArtilleryShellingMapConfiguration_0.BeforeShellingSignalTime))
		{
			Float_5 += Time.deltaTime;
			await Task.Yield();
		}
		Dictionary_0.Clear();
		method_24();
		if (ArtilleryShellingMapConfiguration_0.NewZonesForEachShelling || List_1.Count == 0)
		{
			method_21(ArtilleryShellingMapConfiguration_0.ZonesInShelling, ref List_1);
		}
		foreach (ArtilleryShellingZone item in List_1)
		{
			ArtilleryBrigade artilleryBrigade = method_20(item);
			Dictionary_0.Add(item, new GClass644
			{
				roundsCountTarget = item.ShellingRounds,
				shotsCountTarget = item.ShotCount,
				shellingBrigadeID = artilleryBrigade.ID,
				ShellingPoints = method_19(item),
				BoundsPoints = item.ZoneBounds,
				pauseBetweenRoundOn = true
			});
		}
		foreach (KeyValuePair<ArtilleryShellingZone, GClass644> item2 in Dictionary_0)
		{
			method_33(item2.Key.ID);
		}
		while (Float_5 < Float_6)
		{
			Float_5 += Time.deltaTime;
			await Task.Yield();
		}
		Float_5 = 0f;
		ShellingOn = true;
	}

	public async Task StartEventShelling(ArtilleryShellingZone zone, float initShellingTime)
	{
		ArtilleryBrigade artilleryBrigade = method_20(zone);
		GClass645 gClass = new GClass645
		{
			roundsCountTarget = zone.ShellingRounds,
			shotsCountTarget = zone.ShotCount,
			shellingBrigadeID = artilleryBrigade.ID,
			ShellingPoints = method_19(zone),
			BoundsPoints = zone.ZoneBounds,
			pauseBetweenRoundOn = true
		};
		gClass.waitStartEventShellingTimeElapsed = 0f;
		gClass.waitStartEventShellingTimeTarget = initShellingTime;
		while (!(gClass.waitStartEventShellingTimeElapsed >= gClass.waitStartEventShellingTimeTarget - ArtilleryShellingMapConfiguration_0.BeforeShellingSignalTime))
		{
			gClass.waitStartEventShellingTimeElapsed += Time.deltaTime;
			await Task.Yield();
		}
		method_33(zone.ID);
		while (gClass.waitStartEventShellingTimeElapsed < gClass.waitStartEventShellingTimeTarget)
		{
			gClass.waitStartEventShellingTimeElapsed += Time.deltaTime;
			await Task.Yield();
		}
		Dictionary_1.Add(zone, gClass);
		gClass.waitStartEventShellingTimeElapsed = 0f;
		TargetShellingOn = true;
	}

	public void StopShelling()
	{
		foreach (GClass644 value in Dictionary_0.Values)
		{
			foreach (int key in value.PlayersInAlertZone.Keys)
			{
				HashSet_0.Add(key);
			}
			value.RefreshProcess();
		}
		ShellingOn = false;
	}

	public void SetPauseState(bool pauseOn)
	{
		PauseOn = pauseOn;
	}

	public void method_9()
	{
		ClientShellingControllerClass clientShellingController = Singleton<GameWorld>.Instance.ClientShellingController;
		for (int i = 0; i < ActiveProjectiles.Count; i++)
		{
			ArtilleryServerProjectileClass byIndex = ActiveProjectiles.GetByIndex(i);
			int id = byIndex.id;
			if (!clientShellingController.ActiveClientProjectiles.ContainsKey(id))
			{
				clientShellingController.CreateActiveProjectile(id, byIndex.startPosition);
				continue;
			}
			ArtilleryPacketStruct.id = id;
			ArtilleryPacketStruct.position = byIndex.currentPosition;
			ArtilleryPacketStruct.explosion = byIndex.explosion;
			clientShellingController.ActiveClientProjectiles[id].SyncProjectileState(ref ArtilleryPacketStruct);
		}
	}

	public void method_10()
	{
		if (Int_2 == ArtilleryShellingMapConfiguration_0.ShellingCount)
		{
			return;
		}
		Bool_5 = true;
		foreach (ArtilleryShellingZone key in Dictionary_0.Keys)
		{
			GClass644 gClass = Dictionary_0[key];
			if (gClass.roundsCountCurrent != gClass.roundsCountTarget)
			{
				Bool_5 = false;
				if (gClass.pauseBetweenRoundOn)
				{
					gClass.CalculateRoundsPause();
					Bool_5 = false;
				}
				else if (gClass.pauseBetweenShotOn)
				{
					gClass.CalculateShotsPause();
				}
				else if (gClass.shotsCountCurrent == gClass.shotsCountTarget)
				{
					float time = UnityEngine.Random.Range(key.PauseBetweenRounds.x, key.PauseBetweenRounds.y);
					gClass.InitRoundsPause(time);
				}
				else
				{
					method_18(key, gClass);
					float time2 = UnityEngine.Random.Range(key.PauseBetweenShots.x, key.PauseBetweenShots.y);
					gClass.InitShotPause(time2);
				}
			}
		}
		if (Bool_4)
		{
			List_2.Clear();
			foreach (KeyValuePair<ArtilleryShellingZone, GClass644> item in Dictionary_0)
			{
				List_2.Add((item.Key, item.Value));
			}
			foreach (KeyValuePair<ArtilleryShellingZone, GClass645> item2 in Dictionary_1)
			{
				List_2.Add((item2.Key, item2.Value));
			}
			method_13(List_2);
			Bool_4 = false;
		}
		if (Bool_5)
		{
			StopShelling();
			Int_2++;
			if (Int_2 != ArtilleryShellingMapConfiguration_0.ShellingCount)
			{
				method_8(UnityEngine.Random.Range(ArtilleryShellingMapConfiguration_0.PauseBetweenShellings.x, ArtilleryShellingMapConfiguration_0.PauseBetweenShellings.y)).HandleExceptions();
			}
		}
	}

	public void method_11()
	{
		if (Dictionary_1.Count == 0)
		{
			return;
		}
		List<ArtilleryShellingZone> list = new List<ArtilleryShellingZone>();
		foreach (ArtilleryShellingZone key in Dictionary_1.Keys)
		{
			GClass645 gClass = Dictionary_1[key];
			if (gClass.roundsCountCurrent == gClass.roundsCountTarget)
			{
				list.Add(key);
				continue;
			}
			if (gClass.pauseBetweenRoundOn)
			{
				gClass.CalculateRoundsPause();
				continue;
			}
			if (gClass.pauseBetweenShotOn)
			{
				gClass.CalculateShotsPause();
				continue;
			}
			if (gClass.shotsCountCurrent == gClass.shotsCountTarget)
			{
				float time = UnityEngine.Random.Range(key.PauseBetweenRounds.x, key.PauseBetweenRounds.y);
				gClass.InitRoundsPause(time);
			}
			else
			{
				method_18(key, gClass);
				float time2 = UnityEngine.Random.Range(key.PauseBetweenShots.x, key.PauseBetweenShots.y);
				gClass.InitShotPause(time2);
			}
			if (!Bool_4)
			{
				continue;
			}
			List_2.Clear();
			foreach (KeyValuePair<ArtilleryShellingZone, GClass644> item in Dictionary_0)
			{
				List_2.Add((item.Key, item.Value));
			}
			foreach (KeyValuePair<ArtilleryShellingZone, GClass645> item2 in Dictionary_1)
			{
				List_2.Add((item2.Key, item2.Value));
			}
			method_13(List_2);
			Bool_4 = false;
		}
		foreach (ArtilleryShellingZone item3 in list)
		{
			if (!Dictionary_1.TryGetValue(item3, out var value))
			{
				continue;
			}
			foreach (int key2 in value.PlayersInAlertZone.Keys)
			{
				HashSet_1.Add(key2);
			}
			Dictionary_1.Remove(item3);
		}
		if (TargetShellingOn && Dictionary_1.Count == 0)
		{
			TargetShellingOn = false;
		}
		list.Clear();
	}

	public bool method_12(Player player)
	{
		foreach (CompoundItem collection in GClass3372.GetCollections(player.InventoryController.Inventory.Equipment, Ienumerable_0))
		{
			foreach (Item notMergedItem in GClass3380.GetNotMergedItems(collection))
			{
				if (notMergedItem.TemplateId == MongoID_0)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void method_13(List<(ArtilleryShellingZone, GClass644)> zonesAndProcesses)
	{
		foreach (Player allAlivePlayers in GameWorld_0.AllAlivePlayersList)
		{
			bool num = method_12(allAlivePlayers);
			int playerId = allAlivePlayers.PlayerId;
			if (num && allAlivePlayers.HealthController.IsAlive)
			{
				bool flag = false;
				Vector3 position = allAlivePlayers.Position;
				(ArtilleryShellingZone, GClass644, Vector3, float) tuple = default((ArtilleryShellingZone, GClass644, Vector3, float));
				foreach (var zonesAndProcess in zonesAndProcesses)
				{
					ArtilleryShellingZone item = zonesAndProcess.Item1;
					GClass644 item2 = zonesAndProcess.Item2;
					for (int i = 0; i < item2.ShellingPoints.Count; i++)
					{
						position.y = item2.ShellingPoints[i].y;
						float num2 = GClass856.SqrDistance(item2.ShellingPoints[i], position);
						if (!(num2 > tuple.Item4) || tuple.Equals(default((ArtilleryShellingZone, GClass644, Vector3, float))))
						{
							tuple = (item, item2, item2.ShellingPoints[i], num2);
						}
					}
				}
				if (tuple.Equals(default((ArtilleryShellingZone, GClass644, Vector3, float))))
				{
					method_14(zonesAndProcesses, playerId);
					continue;
				}
				float x = tuple.Item1.AlarmStages[^1].Value.x;
				if (Mathf.Sqrt(tuple.Item4) < x)
				{
					flag = true;
				}
				if (flag && (ShellingOn || TargetShellingOn))
				{
					if (!tuple.Item2.PlayersInAlertZone.ContainsKey(playerId))
					{
						tuple.Item2.PlayersInAlertZone.Add(playerId, new GClass642());
					}
					GClass642 gClass = tuple.Item2.PlayersInAlertZone[playerId];
					gClass.point = tuple.Item3;
					gClass.zoneId = tuple.Item1.ID;
					method_17(playerId, gClass.zoneId, gClass.point);
				}
				else
				{
					method_14(zonesAndProcesses, playerId);
				}
			}
			else
			{
				method_14(zonesAndProcesses, playerId);
			}
		}
	}

	public void method_14(List<(ArtilleryShellingZone, GClass644)> zonesAndProcesses, int playerID)
	{
		foreach (var zonesAndProcess in zonesAndProcesses)
		{
			zonesAndProcess.Item2.PlayersInAlertZone.Remove(playerID);
		}
		method_17(playerID, "", Vector3.zero);
	}

	public void method_15()
	{
		foreach (int item in HashSet_0)
		{
			method_17(item, "", Vector3.zero);
		}
		HashSet_0.Clear();
		Bool_5 = false;
	}

	public void method_16()
	{
		if (HashSet_1.Count == 0)
		{
			return;
		}
		foreach (int item in HashSet_1)
		{
			method_17(item, "", Vector3.zero);
		}
		HashSet_1.Clear();
	}

	public void method_17(int playerId, string zoneId, Vector3 nearestPoint)
	{
		GlobalEventHandlerClass.CreateEvent<ChangePlayerShellingAlertStateEvent>().Invoke(playerId, zoneId, nearestPoint);
	}

	public void method_18(ArtilleryShellingZone zone, GClass644 process)
	{
		ArtilleryBrigade artilleryBrigade = ArtilleryShellingMapConfiguration_0.Brigades.First((ArtilleryBrigade x) => x.ID == process.shellingBrigadeID);
		int num = UnityEngine.Random.Range(0, artilleryBrigade.ArtilleryGuns.Length - 1);
		ArtilleryGun artilleryGun = artilleryBrigade.ArtilleryGuns[num];
		ArtilleryServerProjectileClass projectile = Gclass1507_0.GetProjectile();
		int index = UnityEngine.Random.Range(0, process.ShellingPoints.Count);
		Vector3 vector = process.ShellingPoints[index];
		Vector2 vector2 = zone.PointRadius * UnityEngine.Random.insideUnitCircle;
		Vector3 shellingZonePoint = vector + new Vector3(vector2.x, 0f, vector2.y);
		Vector3 geometryPoint = Vector3.zero;
		method_26(shellingZonePoint, out geometryPoint);
		RegisterArtilleryProjectile(projectile);
		ActiveProjectiles.Add(projectile.id, projectile);
		bool firstInShellingZone = process.shotsCountCurrent == 0 && process.roundsCountCurrent == 0;
		bool lastInShellingZone = process.shotsCountCurrent == process.shotsCountTarget - 1 && process.roundsCountCurrent == process.roundsCountTarget - 1;
		bool firstInRound = process.shotsCountCurrent == 0;
		bool lastInRound = process.shotsCountCurrent == process.shotsCountTarget - 1;
		projectile.InitFly(artilleryGun.Position, geometryPoint, zone.ExplosionDistanceRange, zone.ID, firstInShellingZone, lastInShellingZone, firstInRound, lastInRound);
		GlobalEventHandlerClass.CreateEvent<InitShellingProjectileFlyEvent>().Invoke(projectile.id, projectile.speed, projectile.startPosition, projectile.finalPosition);
	}

	public List<Vector3> method_19(ArtilleryShellingZone zone)
	{
		UnityEngine.Random.InitState(DateTime.Now.Millisecond);
		List<Vector3> l = zone.PointsWorldPositions.ToList();
		int num = UnityEngine.Random.Range((int)zone.PointsInShellings.x, (int)zone.PointsInShellings.y);
		if (num > zone.PointsWorldPositions.Length)
		{
			num = zone.PointsWorldPositions.Length;
		}
		if (num < 0)
		{
			num = 0;
		}
		l = GClass856.Shuffle(l);
		l.RemoveRange(num, l.Count - num);
		return l;
	}

	public ArtilleryBrigade method_20(ArtilleryShellingZone shellingZone)
	{
		ArtilleryBrigade[] brigades = ArtilleryShellingMapConfiguration_0.Brigades;
		float num = 10000f;
		ArtilleryBrigade result = default(ArtilleryBrigade);
		for (int i = 0; i < brigades.Length; i++)
		{
			ArtilleryBrigade artilleryBrigade = brigades[i];
			float num2 = Vector3.Distance(shellingZone.Center, artilleryBrigade.ArtilleryGuns[1].Position);
			if (num2 < num)
			{
				num = num2;
				result = artilleryBrigade;
			}
		}
		return result;
	}

	public void method_21(int zonesToGet, ref List<ArtilleryShellingZone> buffer)
	{
		buffer.Clear();
		List<ArtilleryShellingZone> list = GClass856.Shuffle(method_22().ToList());
		for (int i = 0; i < zonesToGet && i < list.Count; i++)
		{
			buffer.Add(list[i]);
		}
	}

	public IEnumerable<ArtilleryShellingZone> method_22()
	{
		ArtilleryShellingZone[] shellingZones = ArtilleryShellingMapConfiguration_0.ShellingZones;
		for (int i = 0; i < shellingZones.Length; i++)
		{
			ArtilleryShellingZone artilleryShellingZone = shellingZones[i];
			if (artilleryShellingZone.UsedInPlanedShelling && artilleryShellingZone.IsActive)
			{
				yield return artilleryShellingZone;
			}
		}
	}

	public void method_23(int capacity)
	{
		Gclass1507_0 = new GClass1507();
		Gclass1507_0.InitPool(capacity, ArtilleryShellingGlobalSettings_0);
	}

	public void method_24()
	{
		for (int i = 0; i < ArtilleryShellingMapConfiguration_0.ShellingZones.Length; i++)
		{
			ArtilleryShellingZone zone = ArtilleryShellingMapConfiguration_0.ShellingZones[i];
			method_25(ref zone);
			ArtilleryShellingMapConfiguration_0.ShellingZones[i] = zone;
		}
	}

	public void method_25(ref ArtilleryShellingZone zone)
	{
		float num = zone.GridStep.x * zone.Points.x;
		float num2 = zone.GridStep.y * zone.Points.y;
		Vector3 s = new Vector3(num, 0.01f, num2);
		zone.ZoneBounds = new List<Vector2>();
		Vector2[] array = new Vector2[4];
		List<Vector3> list = new List<Vector3>();
		Matrix4x4 matrix4x = Matrix4x4.TRS(zone.Center, Quaternion.Euler(0f, zone.Rotate, 0f), s);
		for (int i = 0; (float)i < zone.Points.y; i++)
		{
			float num3 = 0f - (num2 / 2f + zone.GridStep.y / 2f) + zone.GridStep.y * (float)(i + 1);
			for (int j = 0; (float)j < zone.Points.x; j++)
			{
				float num4 = 0f - (num / 2f + zone.GridStep.x / 2f) + zone.GridStep.x * (float)(j + 1);
				Matrix4x4 matrix4x2 = Matrix4x4.TRS(new Vector3(num4 / num, 0f, num3 / num2), s: new Vector3(zone.PointRadius, 0.01f, zone.PointRadius), q: Quaternion.identity);
				Matrix4x4 matrix = matrix4x * matrix4x2;
				Vector3 item = method_32(matrix);
				list.Add(item);
				if (i == 0 && j == 0)
				{
					array[0] = new Vector2(item.x, item.z);
				}
				if (i == 0 && (float)j == zone.Points.x - 1f)
				{
					array[1] = new Vector2(item.x, item.z);
				}
				if ((float)i == zone.Points.y - 1f && j == 0)
				{
					array[3] = new Vector2(item.x, item.z);
				}
				if ((float)i == zone.Points.y - 1f && (float)j == zone.Points.x - 1f)
				{
					array[2] = new Vector2(item.x, item.z);
				}
			}
		}
		zone.ZoneBounds = array.ToList();
		zone.PointsWorldPositions = list.ToArray();
	}

	public bool method_26(Vector3 shellingZonePoint, out Vector3 geometryPoint)
	{
		shellingZonePoint.y = 300f;
		RaycastHit hitInfo = default(RaycastHit);
		if (Physics.Raycast(shellingZonePoint, Vector3.down, out hitInfo, 400f, EFTHardSettings.Instance.MOVEMENT_MASK))
		{
			geometryPoint = hitInfo.point;
			return true;
		}
		geometryPoint = Vector3.zero;
		return false;
	}

	public void RegisterArtilleryProjectile(ArtilleryServerProjectileClass serverProjectile)
	{
		ArtilleryPacketStruct netPacket = serverProjectile.GetNetPacket();
		serverProjectile.ExplosionEvent += method_28;
		GameWorld_0.ArtilleryProjectilesStates.Add(netPacket);
	}

	public void method_27(ArtilleryServerProjectileClass serverProjectile)
	{
		serverProjectile.ExplosionEvent -= method_28;
	}

	public void method_28(ArtilleryServerProjectileClass serverProjectile)
	{
		try
		{
			serverProjectile.explosion = true;
			if (Bool_3)
			{
				method_30(serverProjectile);
			}
			else
			{
				method_31(serverProjectile);
			}
			method_27(serverProjectile);
			method_29(serverProjectile);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		serverProjectile.Deactivate();
		Gclass1507_0.ReturnProjectile(serverProjectile);
		ActiveProjectiles.Remove(serverProjectile.id);
	}

	public void method_29(ArtilleryServerProjectileClass serverProjectile)
	{
		ArtilleryShellingZone zone = default(ArtilleryShellingZone);
		if (Dictionary_0.Keys.Any((ArtilleryShellingZone x) => x.ID == serverProjectile.zoneID))
		{
			zone = Dictionary_0.First((KeyValuePair<ArtilleryShellingZone, GClass644> x) => x.Key.ID == serverProjectile.zoneID).Key;
		}
		else if (Dictionary_1.Keys.Any((ArtilleryShellingZone x) => x.ID == serverProjectile.zoneID))
		{
			zone = Dictionary_1.First((KeyValuePair<ArtilleryShellingZone, GClass645> x) => x.Key.ID == serverProjectile.zoneID).Key;
		}
		float maxLenghtZoneSide = method_36(zone);
		float radius = method_37(maxLenghtZoneSide);
		GlobalEventHandlerClass.CreateEvent<ShellingProjectileExplosionEvent>().Invoke(serverProjectile.zoneID, radius, zone.Center);
		if (serverProjectile.firstInRoundForZone)
		{
			GlobalEventHandlerClass.CreateEvent<FirstShellingExplosionInRoundEvent>().Invoke(serverProjectile.zoneID, radius, zone.Center);
			method_2();
		}
		if (serverProjectile.lastInRoundForZone)
		{
			GlobalEventHandlerClass.CreateEvent<LastShellingExplosionInRoundEvent>().Invoke(serverProjectile.zoneID, radius, zone.Center);
			method_2();
		}
		if (serverProjectile.firstInShellingForZone)
		{
			GlobalEventHandlerClass.CreateEvent<FirstShellingExplosionZoneEvent>().Invoke(serverProjectile.zoneID, radius, zone.Center);
			method_2();
		}
		if (serverProjectile.lastInShellingForZone)
		{
			GlobalEventHandlerClass.CreateEvent<LastShellingExplosionZoneEvent>().Invoke(serverProjectile.zoneID, radius, zone.Center);
			method_2();
		}
	}

	public void method_30(ArtilleryServerProjectileClass serverProjectile)
	{
		ArtilleryPacketStruct.position = serverProjectile.currentPosition;
		ArtilleryPacketStruct.explosion = true;
		Singleton<GameWorld>.Instance.ClientShellingController.ActiveClientProjectiles[serverProjectile.id].SyncProjectileState(ref ArtilleryPacketStruct);
	}

	public void method_31(ArtilleryServerProjectileClass projectileServer)
	{
		ArtilleryPacketStruct netPacket = projectileServer.GetNetPacket();
		netPacket.explosion = true;
		GameWorld_0.ArtilleryProjectilesStates.Add(netPacket);
	}

	public Vector3 method_32(Matrix4x4 matrix)
	{
		Vector3 result = default(Vector3);
		result.x = matrix.m03;
		result.y = matrix.m13;
		result.z = matrix.m23;
		return result;
	}

	public async Task InitAirdrop(int time, Vector3 position)
	{
		await Task.Delay(time * 1000, CancellationToken_0);
		GameWorld_0.InitAirdrop(ArtilleryShellingMapConfiguration_0.ArtilleryShellingAirDropSettings.LootTemplateId, takeNearbyPoint: true, position);
	}

	public void method_33(string zoneID)
	{
		Queue_0.Enqueue(zoneID);
	}

	public void method_34()
	{
		if (Queue_0.Count == 0)
		{
			return;
		}
		List<Player> allAlivePlayersList = GameWorld_0.AllAlivePlayersList;
		List_3.Clear();
		foreach (Player item in allAlivePlayersList)
		{
			if (method_12(item))
			{
				List_3.Add(item.PlayerId);
			}
		}
		GlobalEventHandlerClass.CreateEvent<ShellingNotifyEvent>().Invoke(List_3.ToArray(), Queue_0.Dequeue());
	}

	public void method_35()
	{
		Float_4 += Time.fixedDeltaTime;
		if (!(Float_4 < Float_3))
		{
			Float_4 = 0f;
			Bool_4 = true;
		}
	}

	public float method_36(ArtilleryShellingZone zone)
	{
		float maxLenghtZoneSide = ((zone.Points.x >= zone.Points.y) ? (zone.Points.x * zone.GridStep.x) : (zone.Points.y * zone.GridStep.y));
		return method_37(maxLenghtZoneSide);
	}

	public float method_37(float maxLenghtZoneSide)
	{
		return maxLenghtZoneSide / Mathf.Sqrt(2f);
	}

	public void Dispose()
	{
		method_6();
	}

	public void PositionInShellingZoneCheck(Vector3 position, out bool inShellingPos, out string zoneID)
	{
		inShellingPos = false;
		zoneID = "";
		foreach (ArtilleryShellingZone key in Dictionary_0.Keys)
		{
			inShellingPos = method_38(position, key.ZoneBounds, key.Center);
			if (inShellingPos)
			{
				zoneID = key.ID;
				return;
			}
		}
		foreach (ArtilleryShellingZone key2 in Dictionary_1.Keys)
		{
			inShellingPos = method_38(position, key2.ZoneBounds, key2.Center);
			if (inShellingPos)
			{
				zoneID = key2.ID;
				break;
			}
		}
	}

	public bool method_38(Vector3 p, List<Vector2> bounds, Vector3 center)
	{
		Vector3 vector = new Vector3(bounds[0].x, p.y, bounds[0].y);
		Vector3 vector2 = new Vector3(bounds[1].x, p.y, bounds[1].y);
		Vector3 vector3 = new Vector3(bounds[2].x, p.y, bounds[2].y);
		Vector3 vector4 = new Vector3(bounds[3].x, p.y, bounds[3].y);
		Vector3 vector5 = p - vector;
		Vector3 vector6 = p - vector2;
		Vector3 vector7 = p - vector3;
		Vector3 vector8 = p - vector4;
		Vector3 vector9 = vector4 - vector;
		Vector3 vector10 = vector - vector2;
		Vector3 vector11 = vector2 - vector3;
		Vector3 vector12 = vector3 - vector4;
		float num = Vector3.Dot(vector5.normalized, vector9.normalized);
		float num2 = Vector3.Dot(vector6.normalized, vector10.normalized);
		float num3 = Vector3.Dot(vector7.normalized, vector11.normalized);
		float num4 = Vector3.Dot(vector8.normalized, vector12.normalized);
		if (num > 0f && num2 > 0f && num3 > 0f)
		{
			return num4 > 0f;
		}
		return false;
	}
}
