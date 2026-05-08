using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using EFT;
using EFT.Game.Spawning;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

public class BotZone : MonoBehaviour
{
	[Serializable]
	[CompilerGenerated]
	public class Class267
	{
		public static readonly Class267 class267_0 = new Class267();

		public static Func<SpawnPointMarker, ISpawnPoint> func_0;

		public ISpawnPoint method_0(SpawnPointMarker marker)
		{
			return marker.SpawnPoint;
		}
	}

	public float DistanceCoef = 1f;

	public int Id;

	private const int NORMAL_SNIPERS_COUNT_PER_ZONE = 2;

	[SerializeField]
	[FormerlySerializedAs("MaxPersons")]
	private int _maxPersons = -1;

	public bool CanSpawnBoss;

	public bool SnipeZone;

	public PatrolWay[] PatrolWays;

	public List<SpawnPointMarker> SpawnPointMarkers;

	public UnspawnPoint[] UnSpawnPoints;

	private ISpawnPoint[] _spawnPoints;

	[HideInInspector]
	public bool HasPmcBotSpawns;

	public BotLocationModifier Modifier;

	private BotsController _botsController;

	public BotZoneNavMeshCutter ZoneNavMeshCutters;

	[HideInInspector]
	public string CoverPointCreatorPresetName;

	public const int NORMAL_BOT_COUNT = 10;

	public int MaxPersons => _maxPersons;

	public Vector3 CenterOfSpawnPoints { get; set; }

	public ISpawnPoint[] SpawnPoints
	{
		get
		{
			ISpawnPoint[] array = _spawnPoints;
			object obj;
			if (array == null)
			{
				List<SpawnPointMarker> spawnPointMarkers = SpawnPointMarkers;
				if (spawnPointMarkers == null)
				{
					obj = null;
				}
				else
				{
					obj = spawnPointMarkers.Select((SpawnPointMarker marker) => marker.SpawnPoint).ToArray();
					if (obj != null)
					{
						goto IL_004a;
					}
				}
				obj = Array.Empty<ISpawnPoint>();
				goto IL_004a;
			}
			goto IL_0052;
			IL_0052:
			return array;
			IL_004a:
			ISpawnPoint[] array2 = (ISpawnPoint[])obj;
			_spawnPoints = (ISpawnPoint[])obj;
			array = array2;
			goto IL_0052;
		}
	}

	public string NameZone => base.name;

	public BotZoneDangerAreas ZoneDangerAreas { get; set; }

	public string ShortName { get; set; }

	public void Awake()
	{
		method_0();
	}

	public void Init(BotLocationModifier modifier, BotsController botsController)
	{
		_botsController = botsController;
		ShortName = base.name;
		ShortName = ShortName.Replace("Bot", "");
		ShortName = ShortName.Replace("Zone", "");
		if (ShortName.Length < 2)
		{
			ShortName = base.name;
		}
		else
		{
			int value = Mathf.Min(10, ShortName.Length);
			value = Mathf.Clamp(value, 1, 20);
			ShortName = ShortName.Substring(0, value);
			if (ShortName.Length == 0)
			{
				ShortName = base.name;
			}
		}
		if (SpawnPoints != null)
		{
			_ = SpawnPoints.LongLength;
		}
		ZoneDangerAreas = new BotZoneDangerAreas();
		PatrolWay[] patrolWays = PatrolWays;
		for (int i = 0; i < patrolWays.Length; i++)
		{
			foreach (PatrolPoint point in patrolWays[i].Points)
			{
				point.Restore(botsController.CoversData.AICorePointsHolder);
			}
		}
		_ = base.gameObject.scene.name;
		Modifier = modifier;
		Vector3 zero = Vector3.zero;
		if (SpawnPointMarkers != null && SpawnPointMarkers.Count > 0)
		{
			foreach (SpawnPointMarker spawnPointMarker in SpawnPointMarkers)
			{
				spawnPointMarker.BotZone = this;
				if (!smethod_0(spawnPointMarker.transform.position))
				{
					GClass3583.Ground(spawnPointMarker.transform);
				}
				zero += spawnPointMarker.Position;
			}
			CenterOfSpawnPoints = zero / SpawnPointMarkers.Count;
		}
		patrolWays = PatrolWays;
		for (int i = 0; i < patrolWays.Length; i++)
		{
			patrolWays[i].InitPoints();
		}
		if (SnipeZone && MaxPersons <= 0)
		{
			_maxPersons = 2;
		}
	}

	public static bool smethod_0(Vector3 position)
	{
		NavMesh.SamplePosition(position, out var hit, 0.6f, -1);
		return hit.hit;
	}

	public void method_0()
	{
		method_1(base.gameObject);
		foreach (Transform item in base.transform)
		{
			if (item.gameObject.GetComponent<SpawnPointMarker>() == null)
			{
				method_1(item.gameObject);
			}
		}
	}

	public void method_1(GameObject obj)
	{
		Collider component = obj.GetComponent<Collider>();
		if (component != null)
		{
			component.enabled = false;
		}
	}

	public bool IsValid()
	{
		if (SpawnPointMarkers.Count > 0)
		{
			return PatrolWays.Length != 0;
		}
		return false;
	}

	public void OnDrawGizmosSelected()
	{
		if (Application.isPlaying && ZoneDangerAreas != null)
		{
			ZoneDangerAreas.OnDrawGizmosSelected();
		}
	}

	public void ClearPools()
	{
	}

	public bool HaveFreeSpace(int requestedCount)
	{
		if (_maxPersons > 0 && _botsController != null)
		{
			int count = _botsController.Bots.GetListByZone(this).Count;
			return _maxPersons - count >= requestedCount;
		}
		return true;
	}
}
