using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using EFT.SynchronizableObjects;
using UnityEngine;

public class BotMinesData : GClass429
{
	[Serializable]
	[CompilerGenerated]
	public class Class186
	{
		public static readonly Class186 class186_0 = new Class186();

		public static Callback callback_0;

		public void method_0(IResult result)
		{
		}
	}

	[CompilerGenerated]
	public class Class187
	{
		public BotMinesData botMinesData_0;

		public AIMinePoint place;

		public AIMinePoint aiMinePoint;

		public void method_0(IResult result)
		{
			botMinesData_0.LastPlanted = place;
			botMinesData_0.MinePlantedComplete(aiMinePoint, succsec: true);
			botMinesData_0.Planting = false;
		}
	}

	[CompilerGenerated]
	public class Class188
	{
		public BotMinesData botMinesData_0;

		public AIMinePoint place;

		public void method_0(IResult result)
		{
			botMinesData_0.MinePlantedComplete(place, succsec: true);
		}
	}

	public BotMinesRealtimePlaceFinder MinesRealtimePlaceFinder;

	[NonSerialized]
	public HashSet<int> PlacesMineSetted = new HashSet<int>();

	[NonSerialized]
	public HashSet<AIMinePoint> PlacesMineInfo = new HashSet<AIMinePoint>();

	[NonSerialized]
	public List<AIMinePoint> PlacedMinesInfo = new List<AIMinePoint>();

	[NonSerialized]
	public int MaxMinesPerCach = 10;

	[NonSerialized]
	public const float DIST_BETWEEN_MINES = 10f;

	[NonSerialized]
	public const float SDIST_BETWEEN_MINES = 100f;

	[NonSerialized]
	public const float DIST_NOT_CLOSE_TO_ENEMY = 10f;

	[NonSerialized]
	public const float SDIST_NOT_CLOSE_TO_ENEMY = 100f;

	public const float SCLOSE_TO_MINE = 2.25f;

	[NonSerialized]
	public bool HaveUnplanted_1;

	public float _lastPlantTime = float.MinValue;

	[NonSerialized]
	public float StartPlaneTime;

	[field: NonSerialized]
	public Vector3 LastCacheCenter { get; set; }

	[field: NonSerialized]
	public bool Planting { get; set; }

	public bool HaveUnplanted => HaveUnplanted_1;

	[field: NonSerialized]
	public AIMinePoint LastPlanted { get; set; }

	[field: NonSerialized]
	public AIMinePoint CurrentActive { get; set; }

	public float LastPlantTime => _lastPlantTime;

	public event Action OnMinesStartCache;

	public event Action OnMinesCacheCompleted;

	public BotMinesData(BotOwner owner)
		: base(owner)
	{
		MinesRealtimePlaceFinder = new BotMinesRealtimePlaceFinder(owner);
	}

	public void Activate()
	{
		List<AIMinePoint> minesSimple = BotOwner_0.BotsGroup.BotGame.BotsController.CoversData.AIMinesPositions.MinesSimple;
		HaveUnplanted_1 = minesSimple.Count > 0;
	}

	public void MinePlantedComplete(AIMinePoint point, bool succsec)
	{
		if (point == null)
		{
			return;
		}
		PlacedMinesInfo.Remove(point);
		if (PlacesMineSetted.Add(point.Id))
		{
			PlacesMineInfo.Add(point);
			if (CurrentActive != null && CurrentActive.Id == point.Id)
			{
				SetActive(null);
			}
			if (PlacedMinesInfo.Count == 0)
			{
				this.OnMinesCacheCompleted?.Invoke();
			}
			_lastPlantTime = Time.time;
		}
	}

	public AIMinePoint FindClosestsUnplanted(Vector3 pos)
	{
		List<AIMinePoint> minesSimple = BotOwner_0.BotsGroup.BotGame.BotsController.CoversData.AIMinesPositions.MinesSimple;
		float num = float.MaxValue;
		AIMinePoint aIMinePoint = null;
		foreach (AIMinePoint item in minesSimple)
		{
			if (!PlacesMineSetted.Contains(item.Id) && item.CorePointInGame.ConnectionGroupId == BotOwner_0.Covers.ConnectionGroupId)
			{
				float sqrMagnitude = (pos - item.Position).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
					aIMinePoint = item;
					Debug.DrawLine(item.Position, item.Position + Vector3.up * 40f, Color.green, 15f);
				}
			}
		}
		HaveUnplanted_1 = aIMinePoint != null;
		return aIMinePoint;
	}

	public AIMinePoint GetClosestsPlanted(Vector3 position)
	{
		float num = float.MaxValue;
		AIMinePoint result = null;
		foreach (AIMinePoint item in PlacedMinesInfo)
		{
			float sqrMagnitude = (item.Position - position).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				num = sqrMagnitude;
				result = item;
			}
		}
		return result;
	}

	public void CacheAllInRadius(Vector3 pos, float rad)
	{
		MaxMinesPerCach = GClass856.RandomInclude(8, 10);
		List<AIMinePoint> minesSimple = BotOwner_0.BotsGroup.BotGame.BotsController.CoversData.AIMinesPositions.MinesSimple;
		float num = rad * rad;
		List<AIMinePoint> list = new List<AIMinePoint>();
		foreach (AIMinePoint item in minesSimple)
		{
			if (!PlacesMineSetted.Contains(item.Id) && item.CorePointInGame.ConnectionGroupId == BotOwner_0.StartCorePoint.ConnectionGroupId && (pos - item.Position).sqrMagnitude < num)
			{
				list.Add(item);
				Debug.DrawLine(item.Position, item.Position + Vector3.up * 40f, Color.green, 15f);
			}
		}
		list.Sort(method_0);
		List<AIMinePoint> list2 = new List<AIMinePoint>();
		foreach (AIMinePoint item2 in list)
		{
			bool flag = false;
			if (list2.Count > MaxMinesPerCach)
			{
				break;
			}
			if (item2.Priority < 1f)
			{
				foreach (AIMinePoint item3 in list2)
				{
					if (!(GClass369.SDistHorizontal(item3.Position - item2.Position) >= 100f))
					{
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				list2.Add(item2);
			}
		}
		this.OnMinesStartCache?.Invoke();
		LastCacheCenter = pos;
		PlacedMinesInfo = list2;
	}

	public int method_0(AIMinePoint x, AIMinePoint y)
	{
		if (x.Priority > y.Priority)
		{
			return -1;
		}
		if (x.Priority < y.Priority)
		{
			return 1;
		}
		return 0;
	}

	public void Dispose()
	{
	}

	public bool HaveAtCache()
	{
		return PlacedMinesInfo.Count > 0;
	}

	public AIMinePoint GetClosestsFromCache(Vector3 ownerPosition)
	{
		if (PlacedMinesInfo.Count == 0)
		{
			return null;
		}
		AIMinePoint result = null;
		float num = float.MaxValue;
		foreach (AIMinePoint item in PlacedMinesInfo)
		{
			if (PlacesMineInfo.Contains(item))
			{
				continue;
			}
			if (BotOwner_0.Memory.HaveEnemy)
			{
				float sqrMagnitude = (item.Position - BotOwner_0.Memory.GoalEnemy.CurrPosition).sqrMagnitude;
				float num2 = ((!item.HaveSafeRadius) ? 100f : item.SSafeRad);
				if (sqrMagnitude < num2)
				{
					continue;
				}
			}
			float sqrMagnitude2 = (item.Position - ownerPosition).sqrMagnitude;
			if (sqrMagnitude2 < num)
			{
				num = sqrMagnitude2;
				result = item;
			}
		}
		return result;
	}

	public void SetActive(AIMinePoint point)
	{
		CurrentActive = point;
	}

	public void ClearCache()
	{
		this.OnMinesCacheCompleted?.Invoke();
		PlacedMinesInfo.Clear();
	}

	public void ManualUpdate()
	{
		if (Planting && Time.time - StartPlaneTime > 20f)
		{
			if (CurrentActive != null)
			{
				PlacesMineInfo.Add(CurrentActive);
			}
			CurrentActive = null;
			Planting = false;
		}
	}

	public bool IsPlanted(AIMinePoint closests)
	{
		return PlacesMineInfo.Contains(closests);
	}

	public void StartPlant(AIMinePoint place)
	{
		if (GetFirstFragGrenade(out var grenade))
		{
			method_1(place, grenade);
		}
		else
		{
			Planting = false;
		}
	}

	public bool GetFirstFragGrenade(out ThrowWeapItemClass grenade)
	{
		foreach (ThrowWeapItemClass item in BotOwner_0.GetPlayer.InventoryController.Inventory.GetPlayerItems(EPlayerItems.Equipment).OfType<ThrowWeapItemClass>().ToList())
		{
			if (item.ThrowType == ThrowWeapType.frag_grenade)
			{
				grenade = item;
				return true;
			}
		}
		grenade = null;
		return false;
	}

	public void method_1(AIMinePoint place, ThrowWeapItemClass potentialGrenade)
	{
		if (Planting)
		{
			return;
		}
		AIMinePoint aiMinePoint = place;
		StartPlaneTime = Time.time;
		Tuple<Vector3, Vector3> sidePoints = place.GetSidePoints();
		if (TripwireSynchronizableObject.ValidateTripwirePosition(BotOwner_0.Position, sidePoints.Item1, sidePoints.Item2, isAI: true, withLogs: false, useLayerWithoutPlayer: true))
		{
			Planting = true;
			LastPlanted = place;
			BotOwner_0.GetPlayer.InventoryController.PlantTripwire(potentialGrenade, null, sidePoints.Item1, sidePoints.Item2, delegate
			{
				LastPlanted = place;
				MinePlantedComplete(aiMinePoint, succsec: true);
				Planting = false;
			});
		}
		else
		{
			MinePlantedComplete(place, succsec: false);
			Planting = false;
		}
	}

	public bool PlantHereNow(AIMinePoint place)
	{
		if (!GetFirstFragGrenade(out var grenade))
		{
			return false;
		}
		Tuple<Vector3, Vector3> sidePoints = place.GetSidePoints();
		BotOwner_0.GetPlayer.InventoryController.PlantTripwire(grenade, null, sidePoints.Item1, sidePoints.Item2, delegate
		{
			MinePlantedComplete(place, succsec: true);
		});
		return true;
	}

	public bool PlantHereNow(Vector3 p1, Vector3 p2)
	{
		if (!GetFirstFragGrenade(out var grenade))
		{
			return false;
		}
		BotOwner_0.GetPlayer.InventoryController.PlantTripwire(grenade, null, p1, p2, delegate
		{
		});
		return true;
	}
}
