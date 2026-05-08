using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Audio.AuxiliaryAudioUtils;
using Audio.Data;
using Audio.SpatialSystem.Data;
using Audio.SpatialSystem.Utils;
using Comfort.Common;
using EFT;
using EFT.DataProviding;
using JetBrains.Annotations;
using UnityEngine;

namespace Audio.SpatialSystem;

public class SpatialAudioSystem : MonoBehaviourSingleton<SpatialAudioSystem>, IDisposable
{
	private const uint uint_0 = 4u;

	private const byte byte_0 = 1;

	public SpatialAudioPoolsConfig poolsConfig;

	public AudioOcclusionSettings OcclusionSettings;

	[SerializeField]
	private SpatialAudioLocationInfo _locationInfo;

	private readonly List<SourceContainerClass> list_0 = new List<SourceContainerClass>();

	private readonly Dictionary<int, SourceContainerClass> dictionary_0 = new Dictionary<int, SourceContainerClass>();

	private readonly List<GClass1126> list_1 = new List<GClass1126>();

	private readonly Dictionary<int, GClass1126> dictionary_1 = new Dictionary<int, GClass1126>();

	private readonly List<GClass1126> list_2 = new List<GClass1126>();

	private GClass1126 gclass1126_0;

	private readonly Dictionary<int, IPlayer> dictionary_2 = new Dictionary<int, IPlayer>();

	private readonly Dictionary<int, SourceContainerClass> dictionary_3 = new Dictionary<int, SourceContainerClass>();

	private readonly Dictionary<int, GClass1126> dictionary_4 = new Dictionary<int, GClass1126>();

	private readonly Dictionary<int, GClass1126> dictionary_5 = new Dictionary<int, GClass1126>();

	private readonly Dictionary<int, SourceContainerClass> dictionary_6 = new Dictionary<int, SourceContainerClass>();

	private readonly Dictionary<int, int> dictionary_7 = new Dictionary<int, int>();

	private readonly Dictionary<int, Coroutine> dictionary_8 = new Dictionary<int, Coroutine>();

	private readonly Dictionary<int, GClass1126> dictionary_9 = new Dictionary<int, GClass1126>();

	private readonly Dictionary<int, SourceContainerClass> dictionary_10 = new Dictionary<int, SourceContainerClass>();

	private GClass1164 gclass1164_0;

	private Transform transform_0;

	private GClass1122 gclass1122_0;

	private GInterface87 ginterface87_0;

	private int int_0;

	private int int_1;

	private GClass1173 gclass1173_0;

	private bool bool_0;

	private GClass1139 gclass1139_0;

	private GClass1141 gclass1141_0;

	private GClass1140 gclass1140_0;

	private readonly GClass1180 gclass1180_0 = new GClass1180();

	private GClass2585 gclass2585_0;

	private Coroutine coroutine_0;

	private Action action_0;

	[CompilerGenerated]
	private static bool bool_1;

	public Transform Transform_0
	{
		get
		{
			if (transform_0 == null)
			{
				transform_0 = method_31();
			}
			return transform_0;
		}
	}

	public bool Boolean_0
	{
		get
		{
			if (Initialized && list_1.Count > 0 && ListenerCurrentRoom.IsValid && Transform_0 != null)
			{
				return GamePlayerOwner.MyPlayer != null;
			}
			return false;
		}
	}

	public ISpatialAudioRoom ListenerCurrentRoom => gclass1122_0?.ListenerCurrentRoom;

	public int ListenerCurrentOutdoorRoomID => gclass1122_0.ListenerCurrentOutdoorRoomID;

	public uint PropagationDepth
	{
		get
		{
			if (!GClass3670.TryGetData<GClass1138>(out var _))
			{
				return 4u;
			}
			return (uint)_locationInfo.bakeSettings.maxDepthIndoorToIndoor;
		}
	}

	public SpatialAudioLocationInfo LocationInfo => _locationInfo;

	public static bool Initialized
	{
		[CompilerGenerated]
		get
		{
			return bool_1;
		}
		[CompilerGenerated]
		set
		{
			bool_1 = value;
		}
	}

	public async Task Initialize(CancellationToken token, IProgress<float> progress = null)
	{
		smethod_0();
		method_9();
		method_0();
		gclass1173_0 = GClass3670.GetData<GClass1173>();
		transform_0 = Singleton<AudioListenerConsistencyManager>.Instance.transform;
		ginterface87_0 = GClass1141.Create(transform_0, EOcclusionTest.Fast, GClass722.Instance);
		if (_locationInfo == null)
		{
			Debug.LogError("Spatial Audio Location info is missing, can't full initialized");
			return;
		}
		gclass1139_0 = new GClass1139(_locationInfo.relativeBakeDataPath, this);
		Task task = gclass1139_0.LoadDataAsync(token);
		if (progress != null && _locationInfo != null)
		{
			method_5(task, progress);
		}
		await task;
		gclass1122_0 = new GClass1122();
		await Task.Yield();
		gclass1164_0 = new GClass1164(this, poolsConfig, OcclusionSettings);
		gclass1164_0.PreCreatePools();
		if (token.IsCancellationRequested)
		{
			method_8();
			return;
		}
		method_18();
		Initialized = true;
		bool_0 = false;
		action_0 = GlobalEventHandlerClass.Instance.SubscribeOnEvent<GClass3568>(method_10);
		gclass2585_0 = new GClass2585(this, Transform_0);
		gclass1140_0 = new GClass1140(this);
		method_1();
		GClass722.Instance.LogInfo("Target audio quality = " + method_3());
		GClass722.Instance.LogInfo("SpatialAudioSystem Initialized");
		if (Singleton<GameWorld>.Instantiated)
		{
			Singleton<GameWorld>.Instance.AfterGameStarted += method_4;
		}
		GlobalEventHandlerClass.CreateEvent<GClass3576>().Invoke();
	}

	public void method_0()
	{
		if (Singleton<GClass1706>.Instance?.AudioSettings != null)
		{
			GClass1096 audioSettings = Singleton<GClass1706>.Instance.AudioSettings;
			OcclusionSettings.Apply(audioSettings.OcclusionSettings.locationOcclusionSettings);
			if (MonoBehaviourSingleton<BetterAudio>.Exist(out var component))
			{
				component.ApplySpatialSettingsFromBackend(audioSettings.OcclusionSettings);
			}
		}
	}

	public void method_1()
	{
		method_2<GClass1129>();
		method_2<GClass1134>();
	}

	public void method_2<T>() where T : GClass1128
	{
		T val = gclass1164_0.WithdrawCalculator<T>();
		val.WarmUp();
		gclass1164_0.ReturnCalculator(val);
	}

	public string method_3()
	{
		EAudioQuality qualityByLogicCores = GClass2313.GetQualityByLogicCores();
		int processorCount = SystemInfo.processorCount;
		return qualityByLogicCores switch
		{
			EAudioQuality.High => $"high {processorCount}", 
			EAudioQuality.Medium => $"medium {processorCount}", 
			_ => $"low {processorCount}", 
		};
	}

	public TCalculator WithdrawCalculator<TCalculator>()
	{
		return gclass1164_0.WithdrawCalculator<TCalculator>();
	}

	public void ReturnCalculator<TCalculator>(TCalculator calculator)
	{
		gclass1164_0.ReturnCalculator(calculator);
	}

	public void method_4()
	{
		Singleton<GameWorld>.Instance.AfterGameStarted -= method_4;
		GClass3670.GetData<GClass1138>().UpdateInitialPortalsData();
	}

	public void method_5(Task loadTask, IProgress<float> progress)
	{
		method_8();
		coroutine_0 = StartCoroutine(method_6(loadTask, progress));
	}

	public IEnumerator method_6(Task loadTask, IProgress<float> progress)
	{
		while (!loadTask.IsCompleted && !loadTask.IsFaulted && !loadTask.IsCanceled)
		{
			progress.Report(method_7());
			yield return null;
		}
	}

	public float method_7()
	{
		GClass1138 data = GClass3670.GetData<GClass1138>();
		float num = (float)data.RelevantRoomPairsByRoomID.Count / (float)_locationInfo.roomsCount;
		float num2 = (float)data.RoomPairsByID.Count / (float)_locationInfo.roomPairsCount;
		float num3 = (float)data.RoutesDataCount / (float)_locationInfo.roomPairsCount;
		return Mathf.Clamp01((num + num2 + num3) / 3f);
	}

	public void method_8()
	{
		if (coroutine_0 != null)
		{
			StopCoroutine(coroutine_0);
			coroutine_0 = null;
		}
	}

	public void method_9()
	{
		GClass3670.CreateData<GClass1138>(EDataLifeTime.Raid);
		GClass3670.CreateData<GClass1173>(EDataLifeTime.Raid);
	}

	public void method_10(GClass3568 stateChangedEvent)
	{
		GClass1138 data = GClass3670.GetData<GClass1138>();
		if (stateChangedEvent.PortalState == BaseSpatialAudioPortal.PortalState.Open || stateChangedEvent.PortalState == BaseSpatialAudioPortal.PortalState.Closed)
		{
			data.UpdatePortalData(stateChangedEvent.PortalID, stateChangedEvent.PortalState, stateChangedEvent.ClosureLevel, stateChangedEvent.Depth, stateChangedEvent.TraversalCost);
		}
	}

	public static SpatialAudioSystem CreateSpatialAudioSystem(Transform parentTransform = null)
	{
		GameObject obj = new GameObject("SpatialAudioSystem");
		obj.transform.SetParent(parentTransform);
		SpatialAudioSystem spatialAudioSystem = obj.AddComponent<SpatialAudioSystem>();
		Singleton<SpatialAudioSystem>.Create(spatialAudioSystem);
		SpatialAudioLocationInfo locationInfo = Resources.Load<SpatialAudioLocationInfo>("SpatialAudioLocationInfo/empty_info");
		spatialAudioSystem._locationInfo = locationInfo;
		GClass722.Instance.LogError("New spatial system created, some modules may not work properly!");
		return spatialAudioSystem;
	}

	public static void smethod_0(bool newSystemEnabled = true)
	{
		MonoBehaviourSingleton<BetterAudio>.Instance.Master.SetFloat("GunsLowPassFilter", newSystemEnabled ? 22000f : 900f);
	}

	public bool TryGetPlayerAudioContainer(int playerID, out SourceContainerClass container)
	{
		return dictionary_3.TryGetValue(playerID, out container);
	}

	public int ProcessSourceOcclusion([NotNull] IPlayer player, [NotNull] BetterSource source, bool allowedLimiter = false)
	{
		int id = player.Id;
		if (!dictionary_2.ContainsValue(player))
		{
			player.OnIPlayerDeadOrUnspawn += method_14;
		}
		dictionary_2[player.Id] = player;
		if (dictionary_3.TryGetValue(id, out var value))
		{
			value.AddSource(source, allowedLimiter);
			dictionary_4[id].UpdateOcclusionEffects(updateImmediately: true);
		}
		else
		{
			Transform spine = player.PlayerBones.Spine1;
			value = gclass1173_0.GetContainer(spine, EOcclusionTest.Combined);
			value.AddSource(source, allowedLimiter);
			GClass1126 gClass = new GClass1126(gclass1180_0, value, Transform_0, GClass722.Instance);
			dictionary_4[id] = gClass;
			if (gClass.Priority == EAudioSourcePriority.OutOfRange)
			{
				list_2.Add(gClass);
			}
			else
			{
				list_1.Add(gClass);
			}
			dictionary_1[value.ID] = gClass;
			dictionary_0[value.ID] = value;
			gClass.UpdateOcclusionEffects(updateImmediately: true);
			dictionary_3.Add(id, value);
			if (allowedLimiter)
			{
				gclass2585_0.RegisterPlayer(player);
			}
		}
		return value.ID;
	}

	public int ProcessSourceOcclusion([NotNull] GameObject go, [NotNull] BetterSource source, EOcclusionTest occlusionTest = EOcclusionTest.Regular, float emptyContainerLifeTimeMS = 30000f, Vector3 positionOffset = default(Vector3), bool staticPosition = false)
	{
		if (go == null)
		{
			GClass722.Instance.LogError("GO can't be null, can't start occlusion process");
			return -1;
		}
		if (source == null)
		{
			GClass722.Instance.LogError("Audio source can't be null, can't start occlusion process");
			return -1;
		}
		if (occlusionTest == EOcclusionTest.None)
		{
			return -1;
		}
		int instanceID = go.GetInstanceID();
		if (dictionary_6.TryGetValue(instanceID, out var value))
		{
			method_11(value, source, positionOffset);
		}
		else
		{
			value = gclass1173_0.GetContainer(go.transform, occlusionTest);
			method_11(value, source, positionOffset);
			GClass1126 gClass = new GClass1126(gclass1180_0, value, Transform_0, GClass722.Instance);
			dictionary_5[instanceID] = gClass;
			if (gClass.Priority == EAudioSourcePriority.OutOfRange)
			{
				list_2.Add(gClass);
			}
			else
			{
				list_1.Insert(method_12(), gClass);
			}
			dictionary_1[value.ID] = gClass;
			dictionary_6[instanceID] = value;
			if (!staticPosition)
			{
				list_0.Add(value);
				dictionary_0[value.ID] = value;
			}
		}
		dictionary_5[instanceID].ForceUpdate();
		dictionary_5[instanceID].UpdateOcclusionEffects(updateImmediately: true);
		dictionary_7[value.ID] = instanceID;
		value.OnContainerEmpty -= method_13;
		value.OnContainerEmpty += method_13;
		return value.ID;
	}

	public void UnregisterContainer(int containerID)
	{
		method_13(containerID);
	}

	public void method_11(SourceContainerClass container, BetterSource newSource, Vector3 posOffset)
	{
		container.SetPositionOffset(posOffset);
		container.AddSource(newSource);
		method_30(container);
	}

	public int method_12()
	{
		int count = list_1.Count;
		int num = ((count != 0) ? (int_0 + 1) : 0);
		if (num > count)
		{
			num = count - 1;
		}
		return num;
	}

	public void method_13(int containerID)
	{
		if (dictionary_7.TryGetValue(containerID, out var value))
		{
			dictionary_7.Remove(containerID);
			if (dictionary_6.TryGetValue(value, out var value2))
			{
				value2.OnContainerEmpty -= method_13;
				dictionary_6.Remove(value);
				method_27(value2);
			}
			if (dictionary_5.TryGetValue(value, out var value3))
			{
				value3?.Dispose();
				list_1.Remove(value3);
				list_2.Remove(value3);
				dictionary_5.Remove(value);
				dictionary_1.Remove(containerID);
			}
		}
	}

	public void PauseContainerUpdate(int containerID)
	{
		if (!dictionary_10.TryGetValue(containerID, out var _) && dictionary_0.TryGetValue(containerID, out var value2) && value2.IsValid)
		{
			dictionary_10[containerID] = value2;
			list_0.Remove(value2);
			if (dictionary_1.TryGetValue(containerID, out var value3) && value3.IsValid)
			{
				value3.ForceUpdate();
				value3.ManualLateUpdate();
				dictionary_9[containerID] = value3;
				list_1.Remove(value3);
				list_2.Remove(value3);
			}
		}
	}

	public void ResumeContainerUpdate(int containerID)
	{
		if (dictionary_10.TryGetValue(containerID, out var value))
		{
			list_0.Add(value);
			dictionary_10.Remove(containerID);
			if (dictionary_9.TryGetValue(containerID, out var value2))
			{
				value2.ForceUpdate();
				value2.ManualLateUpdate();
				list_2.Remove(value2);
				list_1.Add(value2);
				dictionary_9.Remove(containerID);
			}
		}
	}

	public void method_14(IPlayer player)
	{
		player.OnIPlayerDeadOrUnspawn -= method_14;
		int id = player.Id;
		method_16(id);
		dictionary_8[id] = StartCoroutine(method_15(player));
	}

	public IEnumerator method_15(IPlayer player)
	{
		int id = player.Id;
		dictionary_2.Remove(id);
		if (!dictionary_4.TryGetValue(id, out var value))
		{
			yield break;
		}
		value.UpdateOcclusionEffects(updateImmediately: true);
		SpeakerManager speakerManager = Singleton<GameWorld>.Instance.SpeakerManager;
		object obj;
		float num;
		if ((object)speakerManager == null)
		{
			obj = null;
		}
		else
		{
			obj = speakerManager.GetSpeaker(id);
			if (obj != null)
			{
				num = ((PhraseSpeakerClass)obj).TimeLeft;
				goto IL_014b;
			}
		}
		num = 1f;
		goto IL_014b;
		IL_014b:
		float seconds = num;
		yield return new WaitForSeconds(seconds);
		value.Dispose();
		dictionary_4.Remove(id);
		list_1.Remove(value);
		list_2.Remove(value);
		if (dictionary_3.TryGetValue(id, out var value2))
		{
			dictionary_3.Remove(id);
			dictionary_1.Remove(value2.ID);
			gclass2585_0.UnregisterPlayer(player);
			gclass1173_0.ReturnContainer(value2);
			dictionary_8.Remove(id);
		}
	}

	public void method_16(int playerID)
	{
		if (dictionary_8.TryGetValue(playerID, out var value) && value != null)
		{
			StopCoroutine(value);
		}
	}

	public int ProcessSourceOcclusion([NotNull] BetterSource source, EOcclusionTest test, Vector3 positionShift = default(Vector3))
	{
		switch (test)
		{
		case EOcclusionTest.None:
			return -1;
		default:
			return method_20(source, test);
		case EOcclusionTest.Fast:
			return method_17(source, test);
		case EOcclusionTest.OneShotPropagation:
		case EOcclusionTest.OneShotFullOcclusion:
			return method_19(source, test, positionShift);
		}
	}

	public void ProcessInteractiveObjectOcclusion(string targetID, BetterSource source, Vector3 position, EOcclusionTest test)
	{
		gclass1140_0.CheckOcclusion(targetID, source, position, test);
	}

	public int method_17([NotNull] BetterSource source, EOcclusionTest test = EOcclusionTest.Fast)
	{
		SourceContainerClass container = gclass1173_0.GetContainer(source.transform, test);
		container.AddSource(source);
		method_30(container);
		ginterface87_0.SetAudioSourceContainer(container);
		ginterface87_0.UpdateOcclusionEffects(container, updateImmediately: true);
		list_0.Add(container);
		dictionary_0[container.ID] = container;
		return container.ID;
	}

	public void method_18()
	{
		gclass1141_0 = GClass1141.Create(transform_0, EOcclusionTest.OneShotPropagation, GClass722.Instance);
	}

	public int method_19([NotNull] BetterSource source, EOcclusionTest test = EOcclusionTest.Fast, Vector3 positionShift = default(Vector3))
	{
		SourceContainerClass container = gclass1173_0.GetContainer(source.transform, test);
		container.UseQualityCompression = source.SpatialSettings.useQualityCompression;
		container.MaxOcclusionQualityFactor = source.SpatialSettings.maxQualityFactor;
		container.AddSource(source);
		container.SetPositionOffset(positionShift);
		method_30(container);
		gclass1141_0.SetAudioSourceContainer(container);
		list_0.Add(container);
		dictionary_0[container.ID] = container;
		return container.ID;
	}

	public int method_20([NotNull] BetterSource source, EOcclusionTest test)
	{
		SourceContainerClass container = gclass1173_0.GetContainer(source.transform, test);
		container.AddSource(source);
		method_30(container);
		GClass1126 gClass = new GClass1126(gclass1180_0, container, Transform_0, GClass722.Instance);
		if (gClass.Priority == EAudioSourcePriority.OutOfRange)
		{
			list_2.Add(gClass);
		}
		else
		{
			list_1.Add(gClass);
		}
		dictionary_1[container.ID] = gClass;
		gClass.UpdateOcclusionEffects(updateImmediately: true);
		list_0.Add(container);
		dictionary_0[container.ID] = container;
		return container.ID;
	}

	public void AddPlayerCurrentRoom(SpatialAudioRoom room, IPlayer player)
	{
		gclass1122_0?.AddPlayerCurrentRoom(room, player);
		method_21(room, player.Id);
	}

	public void method_21(SpatialAudioRoom newRoom, int playerId)
	{
		if (dictionary_3.TryGetValue(playerId, out var value) && value.CurrentAudioRoom.ID != newRoom.ID)
		{
			value.CurrentAudioRoom = newRoom;
			if (dictionary_4.TryGetValue(playerId, out var value2))
			{
				value2.UpdateOcclusionEffects(updateImmediately: true);
			}
		}
	}

	public void RemovePlayerCurrentRoom(SpatialAudioRoom room, IPlayer player)
	{
		gclass1122_0?.RemovePlayerCurrentRoom(room, player);
	}

	public bool IsSourceInListenerRoom(SourceContainerClass sourceContainer)
	{
		return sourceContainer.CurrentAudioRoom.ID == ListenerCurrentRoom.ID;
	}

	public bool IsSourceAndListenerOutdoor(ISpatialAudioRoom sourceRoom)
	{
		if (ListenerCurrentRoom.IsOutdoor)
		{
			return sourceRoom.IsOutdoor;
		}
		return false;
	}

	public bool IsSourceAndListenerInCommonOutdoor(ISpatialAudioRoom sourceRoom)
	{
		if (ListenerCurrentRoom.Type == EAudioRoomTypeMask.OutdoorCommon)
		{
			return sourceRoom.Type == EAudioRoomTypeMask.OutdoorCommon;
		}
		return false;
	}

	public bool IsSourceAndListenerInDiffEnvironment(ISpatialAudioRoom sourceRoom)
	{
		return ListenerCurrentRoom.IsOutdoor != sourceRoom.IsOutdoor;
	}

	public float GetEnvironmentFactor(ISpatialAudioRoom sourceRoom, AudioGroupOcclusionSettings settings)
	{
		if (!IsSourceAndListenerInDiffEnvironment(sourceRoom))
		{
			return 1f;
		}
		if (!ListenerCurrentRoom.IsOutdoor)
		{
			return settings.indoorToOutdoorFactor;
		}
		return settings.outdoorToIndoorFactor;
	}

	public float GetIsolationFactor(ISpatialAudioRoom sourceRoom)
	{
		if (sourceRoom.IsValid && ListenerCurrentRoom.IsValid)
		{
			if (sourceRoom.ID == ListenerCurrentRoom.ID)
			{
				return 0f;
			}
			if (!sourceRoom.IsIsolated && !ListenerCurrentRoom.IsIsolated)
			{
				return 0f;
			}
			if (!TryGetMatchingRoomPair(sourceRoom, out var roomPair))
			{
				return 1f;
			}
			return Mathf.Clamp01((float)(int)roomPair.ShortestRouteLength / (float)PropagationDepth);
		}
		return 0f;
	}

	public bool IsListenerAndSourceInSameRoom(ISpatialAudioRoom sourceRoom, EAudioRoomTypeMask roomMask)
	{
		ISpatialAudioRoom listenerCurrentRoom = ListenerCurrentRoom;
		if (sourceRoom.IsValid && listenerCurrentRoom.IsValid)
		{
			if (sourceRoom.ID != listenerCurrentRoom.ID)
			{
				return false;
			}
			return (sourceRoom.Type & roomMask) != 0;
		}
		return false;
	}

	public bool TryGetMatchingRoomPair(ISpatialAudioRoom emitterRoom, out RoomPair roomPair, out bool isReversed)
	{
		return gclass1122_0.TryGetMatchingRoomPair(emitterRoom, ListenerCurrentRoom, out roomPair, out isReversed);
	}

	public bool TryGetMatchingRoomPair(ISpatialAudioRoom emitterRoom, out RoomPair roomPair)
	{
		bool isReversed;
		return TryGetMatchingRoomPair(emitterRoom, out roomPair, out isReversed);
	}

	public void Update()
	{
		if (GClass3670.TryGetData<GClass1138>(out var dataContainer) && dataContainer.HasPendingPortalUpdates)
		{
			dataContainer.SwapPortalBuffers();
		}
		if (Initialized)
		{
			gclass1141_0.ManualUpdate();
		}
		method_35();
		if (Boolean_0)
		{
			method_24();
			method_25();
		}
	}

	public int method_22()
	{
		return Mathf.Min(int_0 + 1, list_1.Count);
	}

	public void method_23()
	{
		int_0++;
		if (int_0 >= list_1.Count)
		{
			int_0 = 0;
		}
	}

	public void LateUpdate()
	{
		if (Initialized)
		{
			gclass1141_0.ManualLateUpdate();
		}
		if (!Boolean_0)
		{
			return;
		}
		method_26();
		if (!ListenerCurrentRoom.IsValid)
		{
			if (gclass1126_0 != null)
			{
				gclass1126_0.ManualLateUpdate();
				gclass1126_0.UpdateOcclusionEffects();
				gclass1126_0 = null;
				method_23();
			}
			return;
		}
		method_29();
		if (gclass1126_0 != null)
		{
			gclass1126_0.ManualLateUpdate();
			gclass1126_0.UpdateOcclusionEffects();
			gclass1126_0 = null;
			method_23();
		}
		gclass1164_0.UpdatePendingCalculators(poolsConfig.pendingCompleteCalculatorsCount);
	}

	public void method_24()
	{
		for (int num = method_22() - 1; num >= int_0; num--)
		{
			GClass1126 gClass = list_1[num];
			if (!gClass.IsValid)
			{
				gClass.Dispose();
				list_1.RemoveAt(num);
				if (num < int_0)
				{
					int_0--;
				}
			}
		}
		if (!(Transform_0 != null))
		{
			method_33();
		}
	}

	public void method_25()
	{
		if (list_1.Count == 0)
		{
			return;
		}
		int num = list_1.Count;
		int num2 = 0;
		int num3;
		GClass1126 gClass;
		while (true)
		{
			if (num2 >= num)
			{
				return;
			}
			num3 = (int_0 + num2) % num;
			gClass = list_1[num3];
			gClass.UpdatePriority();
			if (gClass.Priority == EAudioSourcePriority.OutOfRange)
			{
				list_1.RemoveAt(num3);
				list_2.Add(gClass);
				if (num3 < int_0)
				{
					int_0--;
				}
				num--;
				num2--;
			}
			else
			{
				if (gClass.SkippedFrames >= gClass.FramesToSkip)
				{
					break;
				}
				gClass.SkippedFrames++;
			}
			num2++;
		}
		gClass.SkippedFrames = 0;
		gclass1126_0 = gClass;
		int_0 = num3;
		gClass.ManualUpdate();
	}

	public void method_26()
	{
		for (int i = 0; i < list_0.Count; i++)
		{
			SourceContainerClass sourceContainerClass = list_0[i];
			if (!sourceContainerClass.IsValid)
			{
				method_27(sourceContainerClass);
			}
		}
	}

	public void method_27(SourceContainerClass sourceContainer)
	{
		list_0.Remove(sourceContainer);
		dictionary_0.Remove(sourceContainer.ID);
		gclass1173_0.ReturnContainer(sourceContainer);
	}

	public void method_28()
	{
		foreach (var (_, container) in dictionary_3)
		{
			gclass1173_0.ReturnContainer(container);
		}
	}

	public void method_29()
	{
		foreach (KeyValuePair<int, SourceContainerClass> item in dictionary_3)
		{
			item.Deconstruct(out var key, out var value);
			int playerId = key;
			SourceContainerClass sourceContainerClass = value;
			ISpatialAudioRoom otherPlayerCurrentRoom = gclass1122_0.GetOtherPlayerCurrentRoom(playerId);
			sourceContainerClass.CurrentAudioRoom = otherPlayerCurrentRoom;
		}
		foreach (SourceContainerClass item2 in list_0)
		{
			method_30(item2);
		}
	}

	public void method_30(SourceContainerClass sourceContainer)
	{
		bool flag = GClass940.IsPositionIdentical(sourceContainer.CurrentPosition, sourceContainer.GetCachedPosition());
		Bounds sourceBounds = sourceContainer.SourceBounds;
		if (!sourceContainer.CurrentAudioRoom.IsValid)
		{
			if (!flag)
			{
				sourceContainer.UpdateCachedPosition();
			}
		}
		else
		{
			if (flag)
			{
				return;
			}
			sourceContainer.UpdateCachedPosition();
		}
		sourceContainer.CurrentAudioRoom = gclass1122_0.FindActualCurrentRoom(sourceContainer.CurrentAudioRoom, sourceBounds);
	}

	public Transform method_31()
	{
		if (!Singleton<AudioListenerConsistencyManager>.Instantiated)
		{
			return null;
		}
		return Singleton<AudioListenerConsistencyManager>.Instance.transform;
	}

	public override void OnDestroy()
	{
		Dispose();
		base.OnDestroy();
	}

	public void Dispose()
	{
		if (!Initialized || bool_0)
		{
			return;
		}
		bool_0 = true;
		if (Singleton<GameWorld>.Instantiated)
		{
			Singleton<GameWorld>.Instance.AfterGameStarted -= method_4;
		}
		Initialized = false;
		method_8();
		action_0?.Invoke();
		action_0 = null;
		foreach (var (_, player2) in dictionary_2)
		{
			if (player2 != null)
			{
				player2.OnIPlayerDeadOrUnspawn -= method_14;
			}
		}
		method_32();
		dictionary_2.Clear();
		method_33();
		for (int i = 0; i < list_0.Count; i++)
		{
			SourceContainerClass sourceContainer = list_0[i];
			method_27(sourceContainer);
		}
		method_28();
		dictionary_4.Clear();
		list_0.Clear();
		dictionary_3.Clear();
		gclass1122_0?.Clear();
		gclass1122_0 = null;
		ginterface87_0 = null;
		int_0 = 0;
		GClass950.Clear();
		gclass2585_0?.Dispose();
		gclass1140_0?.Dispose();
		gclass1141_0?.Dispose();
		gclass1164_0.DisposePools();
		if (GClass3670.TryGetData<GClass1138>(out var dataContainer))
		{
			try
			{
				gclass1139_0?.StopLoadCoroutine();
			}
			catch (Exception arg)
			{
				GClass722.Instance.LogWarn($"Failed to stop spatial audio data loader coroutine: {arg}");
			}
			dataContainer.Dispose();
		}
	}

	public void method_32()
	{
		foreach (var (playerID, _) in dictionary_8)
		{
			method_16(playerID);
		}
		dictionary_8.Clear();
	}

	public void method_33()
	{
		foreach (GClass1126 item in list_1)
		{
			item.Dispose();
		}
		list_1.Clear();
		foreach (GClass1126 item2 in list_2)
		{
			item2.Dispose();
		}
		list_2.Clear();
	}

	public void method_34()
	{
		for (int num = list_2.Count - 1; num >= 0; num--)
		{
			GClass1126 gClass = list_2[num];
			if (!gClass.IsValid)
			{
				gClass.Dispose();
				list_2.RemoveAt(num);
			}
		}
	}

	public void method_35()
	{
		method_34();
		for (int num = list_2.Count - 1; num >= 0; num--)
		{
			GClass1126 gClass = list_2[num];
			gClass.UpdatePriority();
			if (gClass.Priority != EAudioSourcePriority.OutOfRange)
			{
				list_2.RemoveAt(num);
				list_1.Insert(method_12(), gClass);
			}
		}
	}
}
