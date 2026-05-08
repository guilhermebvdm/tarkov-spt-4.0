using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Audio.AmbientSubsystem;
using Audio.SpatialSystem;
using EFT;
using UnityEngine;

[Serializable]
public class SpatialAudioRoom : MonoBehaviour, ISpatialAudioRoom, ISpatialRoom
{
	[Serializable]
	public class RoomConnection
	{
		public SpatialAudioRoom connectedRoom;

		public BaseSpatialAudioPortal[] connectingPortals;
	}

	[Serializable]
	[CompilerGenerated]
	public class Class524
	{
		public static readonly Class524 class524_0 = new Class524();

		public static Func<BaseSpatialAudioPortal, bool> func_0;

		public bool method_0(BaseSpatialAudioPortal connectingPortal)
		{
			return (object)connectingPortal == null;
		}
	}

	private const short INVALID_ID = -1;

	private readonly GClass1151 _logger = new GClass1151();

	[HideInInspector]
	public int priority;

	[Range(0f, 1f)]
	public float WallOcclusion = 0.5f;

	public List<AudioTriggerArea> Areas;

	[SerializeField]
	private bool Outdoor;

	[SerializeField]
	private bool Isolated;

	[SerializeField]
	private EAudioRoomTypeMask _type = EAudioRoomTypeMask.IndoorGeneric;

	[Tooltip("experimental options to fit room in scene geometry")]
	public bool FitToGeometry;

	public List<RoomConnection> roomConnections = new List<RoomConnection>();

	public List<ISpatialPortal> allPortals = new List<ISpatialPortal>();

	[SerializeField]
	private short _iD;

	[SerializeField]
	[HideInInspector]
	private float _roomSize;

	[SerializeField]
	[HideInInspector]
	private Bounds _bounds;

	public RoomAmbientData AmbientData;

	private readonly Dictionary<int, uint> _playersEnteredAreasCount = new Dictionary<int, uint>();

	private ISoundPlayer _roomAmbientSoundPlayer;

	private HashSet<short> _connectedRoomsIDs = new HashSet<short>();

	public bool OnlyWired = true;

	public EAudioRoomTypeMask Type => _type;

	public string Name { get; set; } = string.Empty;

	public BoxCollider[] Colliders { get; set; }

	public bool IsOutdoor => Outdoor;

	public bool IsIsolated => Isolated;

	public RoomAmbientData RoomAmbientData => AmbientData;

	public bool IsValid
	{
		get
		{
			if (ID != -1)
			{
				return IsInitialized;
			}
			return false;
		}
	}

	public bool IsInitialized { get; set; }

	public short ID => _iD;

	public float RoomSize => _roomSize;

	public Vector3 Position => Bounds.center;

	public Bounds Bounds => _bounds;

	public bool CheckIsolationRelation(ISpatialAudioRoom roomToCheck)
	{
		if (!Isolated)
		{
			return false;
		}
		foreach (RoomConnection roomConnection in roomConnections)
		{
			if (roomConnection.connectedRoom.ID == roomToCheck.ID)
			{
				return false;
			}
		}
		return true;
	}

	public virtual void Awake()
	{
		IsInitialized = false;
		method_1();
	}

	public void method_0()
	{
		foreach (AudioTriggerArea area in Areas)
		{
			area.OnTriggerArea += method_7;
		}
	}

	public bool IsRoomConnected(short roomID)
	{
		if (_connectedRoomsIDs.Count == 0)
		{
			method_1();
		}
		return _connectedRoomsIDs.Contains(roomID);
	}

	public void Initialize()
	{
		method_6();
		if (method_3())
		{
			method_2();
			_bounds = method_10();
			method_4();
			method_5();
			method_0();
			if (_roomSize <= 0f)
			{
				_roomSize = method_9();
			}
			_roomAmbientSoundPlayer = new RoomAmbientSoundPlayer(AmbientData);
			IsInitialized = true;
		}
	}

	public void PlayRoomToneSound()
	{
		_roomAmbientSoundPlayer.Play();
	}

	public void StopRoomToneSound()
	{
		_roomAmbientSoundPlayer.Stop();
	}

	public void method_1()
	{
		_connectedRoomsIDs.Clear();
		foreach (RoomConnection roomConnection in roomConnections)
		{
			_connectedRoomsIDs.Add(roomConnection.connectedRoom.ID);
		}
	}

	public void method_2()
	{
		if (Areas.Count == 0)
		{
			return;
		}
		try
		{
			Colliders = new BoxCollider[Areas.Count];
			for (int i = 0; i < Areas.Count; i++)
			{
				Colliders[i] = Areas[i].GetCollider();
			}
		}
		catch (Exception)
		{
			_logger.LogError(ESpatialAudioRoomError.NoValidTriggerCollider, Name);
		}
	}

	public bool method_3()
	{
		if (Areas == null)
		{
			_logger.LogError(ESpatialAudioRoomError.AudioTriggerAreaIsNull, Name);
			return false;
		}
		foreach (RoomConnection roomConnection in roomConnections)
		{
			if (roomConnection != null)
			{
				if ((object)roomConnection.connectedRoom != null)
				{
					if (roomConnection.connectingPortals != null)
					{
						if (roomConnection.connectingPortals.Any((BaseSpatialAudioPortal connectingPortal) => (object)connectingPortal == null))
						{
							_logger.LogError(ESpatialAudioRoomError.ConnectedPortalIsNull, Name);
							return false;
						}
						continue;
					}
					_logger.LogError(ESpatialAudioRoomError.ConnectedPortalIsNull, Name);
					return false;
				}
				_logger.LogError(ESpatialAudioRoomError.ConnectedRoomIsNull, Name);
				return false;
			}
			_logger.LogError(ESpatialAudioRoomError.RoomConnectionIsNull, Name);
			return false;
		}
		return true;
	}

	public void method_4()
	{
		foreach (RoomConnection roomConnection in roomConnections)
		{
			BaseSpatialAudioPortal[] connectingPortals = roomConnection.connectingPortals;
			for (int i = 0; i < connectingPortals.Length; i++)
			{
				connectingPortals[i].SetConnectedRoom(this);
			}
		}
	}

	public void method_5()
	{
		foreach (RoomConnection roomConnection in roomConnections)
		{
			BaseSpatialAudioPortal[] connectingPortals = roomConnection.connectingPortals;
			foreach (BaseSpatialAudioPortal item in connectingPortals)
			{
				allPortals.Add(item);
			}
		}
	}

	public void method_6()
	{
	}

	public void method_7(object sender, GEventArgs0 e)
	{
		if (!MonoBehaviourSingleton<SpatialAudioSystem>.Instantiated)
		{
			return;
		}
		IPlayer player = e.Player;
		if (player == null || !player.HealthController.IsAlive)
		{
			return;
		}
		OnTriggerEvent(e);
		int id = player.Id;
		if (e.TriggerEventType == GEventArgs0.ETriggerEventType.TriggerEnter)
		{
			if (_playersEnteredAreasCount.TryGetValue(id, out var value))
			{
				if (value == 0)
				{
					MonoBehaviourSingleton<SpatialAudioSystem>.Instance.AddPlayerCurrentRoom(this, player);
				}
				_playersEnteredAreasCount[id]++;
			}
			else
			{
				_playersEnteredAreasCount.Add(id, 1u);
				MonoBehaviourSingleton<SpatialAudioSystem>.Instance.AddPlayerCurrentRoom(this, player);
			}
		}
		if (e.TriggerEventType == GEventArgs0.ETriggerEventType.TriggerExit && _playersEnteredAreasCount.TryGetValue(id, out var value2))
		{
			value2--;
			_playersEnteredAreasCount[id] = value2;
			if (value2 == 0)
			{
				MonoBehaviourSingleton<SpatialAudioSystem>.Instance.RemovePlayerCurrentRoom(this, player);
				_playersEnteredAreasCount.Remove(id);
			}
		}
	}

	public virtual void OnTriggerEvent(GEventArgs0 eventArgs)
	{
	}

	public void OnDestroy()
	{
		method_8();
		_roomAmbientSoundPlayer?.Stop();
	}

	public void method_8()
	{
		foreach (AudioTriggerArea area in Areas)
		{
			area.OnTriggerArea -= method_7;
		}
	}

	public float method_9()
	{
		if (Colliders != null && Colliders.Length != 0)
		{
			Vector3 size = _bounds.size;
			float num = size.x * size.y * size.z;
			_roomSize = (float)Math.Round(num, 1, MidpointRounding.AwayFromZero);
			return _roomSize;
		}
		return _roomSize;
	}

	public bool IsConnected(int targetRoomID)
	{
		foreach (RoomConnection roomConnection in roomConnections)
		{
			if (roomConnection.connectedRoom.ID == targetRoomID)
			{
				return true;
			}
		}
		return false;
	}

	public List<ISpatialPortal> GetPortals()
	{
		return allPortals;
	}

	public Bounds method_10()
	{
		method_2();
		if (Colliders != null && Colliders.Length != 0)
		{
			Bounds bounds = Colliders[0].bounds;
			for (int i = 1; i < Colliders.Length; i++)
			{
				bounds.Encapsulate(Colliders[i].bounds.min);
				bounds.Encapsulate(Colliders[i].bounds.max);
			}
			_bounds = bounds;
			return bounds;
		}
		return new Bounds(base.transform.position, Vector3.zero);
	}
}
