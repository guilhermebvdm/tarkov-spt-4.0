using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EFT;
using EFT.Game.Spawning;
using UnityEngine;

public class GClass699
{
	[NonSerialized]
	public List<GClass698> List_0;

	[NonSerialized]
	public List<GClass698> List_1 = new List<GClass698>();

	[NonSerialized]
	public List<GClass698> List_2 = new List<GClass698>();

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public Dictionary<string, GClass693> Dictionary_0 = new Dictionary<string, GClass693>();

	[NonSerialized]
	public GClass693 Gclass693_0;

	[NonSerialized]
	public GClass696 Gclass696_0 = new GClass696();

	[NonSerialized]
	public GClass701 Gclass701_0 = new GClass701();

	[NonSerialized]
	public GClass698 Gclass698_0;

	[NonSerialized]
	public ISpawnPoint IspawnPoint_0;

	[NonSerialized]
	public IEnumerable<ISpawnPoint> Ienumerable_0;

	[NonSerialized]
	public string String_0;

	[NonSerialized]
	public string String_1;

	[NonSerialized]
	public ISpawnPoint IspawnPoint_1;

	public GClass699(string title)
	{
		String_0 = title;
		List_0 = List_1;
	}

	public void StartCheckCarePlayers(string spawnPointId)
	{
		Gclass693_0 = new GClass693(spawnPointId);
		if (!Dictionary_0.ContainsKey(spawnPointId))
		{
			Dictionary_0.Add(spawnPointId, Gclass693_0);
		}
		else
		{
			Dictionary_0[spawnPointId] = Gclass693_0;
		}
	}

	public void StartScavTogerther()
	{
		Gclass696_0.Inited = true;
	}

	public void StartCalcFarthestFromPlayers()
	{
		Gclass701_0.StartCalcFarthestFromPlayers();
	}

	public void Finish(string finishTitle)
	{
		String_1 = finishTitle;
		Task.Run((Action)method_0);
	}

	public void Finish(IEnumerable<ISpawnPoint> result, string finishTitle)
	{
		Ienumerable_0 = result;
		String_1 = finishTitle;
		Task.Run((Action)method_0);
	}

	public void Finish(ISpawnPoint result, string finishTitle)
	{
		IspawnPoint_0 = result;
		String_1 = finishTitle;
		Task.Run((Action)method_0);
	}

	public void NoValidPointsFirstIteration()
	{
		List_0 = List_2;
	}

	public void AddValidSpawnPoint(ISpawnPoint sp)
	{
		GClass698 gclass698_ = new GClass698(sp);
		Gclass698_0 = gclass698_;
		List_0.Add(Gclass698_0);
	}

	public void AddValidSpawnPointTime(ISpawnPoint sp, bool isValid, float time)
	{
		Gclass698_0.Time = time;
	}

	public void AddValidSpawnPointSide(ISpawnPoint sp, bool isValid, EPlayerSide? profileDataSide)
	{
		Gclass698_0.ProfileDataSide = profileDataSide;
	}

	public void CarePlayersIsNull(string spawnPointId)
	{
		Gclass693_0.CarePlayersIsNull();
	}

	public void CheckPlayer(string spawnPointId, int playerId, Vector3 playerPosition, ISpawnPointCollider spawnPointCollider, bool contain)
	{
		Gclass693_0.CheckPlayer(playerId, playerPosition, spawnPointCollider, contain);
	}

	public void CheckDist(string spawnPointId, int playerId, Vector3 playerPosition, Vector3 spawnPointPosition, float sDist, float distanceSqr)
	{
		Gclass693_0.CheckDist(playerId, playerPosition, spawnPointPosition, sDist, distanceSqr);
	}

	public void CarePlayersIsGood(string spawnPointId)
	{
		Gclass693_0.CarePlayersIsGood();
	}

	public void AddFarthestFromPlayersResult(ISpawnPoint rnd, float dist)
	{
		Gclass701_0.AddFarthestFromPlayersResult(rnd, dist);
	}

	public void AddFarthestFromPlayers(ISpawnPoint spawnPoint, float sqrDistance)
	{
		Gclass701_0.AddFarthestFromPlayers(spawnPoint, sqrDistance);
	}

	public void AddNotOccupied(ISpawnPoint point, bool isNotOccupied)
	{
		Gclass701_0.AddNotOccupied(point, isNotOccupied);
	}

	public void AddFinalNoValiedRandom(ISpawnPoint spawnPoint)
	{
		Gclass701_0.AddFinalNoValiedRandom(spawnPoint);
	}

	public void HaveValidPoints()
	{
		Bool_0 = true;
	}

	public void GroupPlayerIsNull(ISpawnPoint spawnPoint)
	{
		Gclass696_0.GroupPlayerIsNull(spawnPoint);
	}

	public void SetGroupCenter(Vector3 groupCenterPoint)
	{
		Gclass696_0.SetGroupCenter(groupCenterPoint);
	}

	public void CheckValidPoints(ISpawnPoint validPoint)
	{
		Gclass696_0.CheckValidPoints(validPoint);
	}

	public void CheckValidPointsIsOccupied(bool notOccupied)
	{
		Gclass696_0.CheckValidPointsIsOccupied(notOccupied);
	}

	public void CheckValidPointsSDist(float sDist)
	{
		Gclass696_0.CheckValidPointsSDist(sDist);
	}

	public void SetClosest(ISpawnPoint closest, float tmpDist)
	{
		Gclass696_0.SetClosest(closest, tmpDist);
	}

	public void SetValid(bool isValid)
	{
		Gclass698_0.IsValid = isValid;
	}

	public void LastRandom(ISpawnPoint lastRnd)
	{
		IspawnPoint_1 = lastRnd;
	}

	public void method_0()
	{
		if (GClass399.Instance.IsDebugEnable())
		{
			method_1();
		}
	}

	public void method_1()
	{
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("=======SpawnSystemDebugCollector " + String_0);
		stringBuilder.AppendLine($"----listFirst----:{List_1.Count}");
		foreach (GClass698 item in List_1)
		{
			stringBuilder.AppendLine(item.DebugInfo());
		}
		stringBuilder.AppendLine($"----listSecond----:{List_2.Count}");
		foreach (GClass698 item2 in List_2)
		{
			stringBuilder.AppendLine(item2.DebugInfo());
		}
		stringBuilder.AppendLine("-----dcCheckCarePlayers----");
		foreach (KeyValuePair<string, GClass693> item3 in Dictionary_0)
		{
			stringBuilder.AppendLine(item3.Value.DebugInfo().ToString());
		}
		stringBuilder.AppendLine($"_haveNotValidePoints:{Bool_0}");
		if (Gclass701_0.Inited)
		{
			stringBuilder.AppendLine(Gclass701_0.DebugInfo().ToString());
		}
		if (Gclass696_0.Inited)
		{
			stringBuilder.AppendLine(Gclass696_0.DebugInfo().ToString());
		}
		if (IspawnPoint_1 != null)
		{
			stringBuilder.AppendLine("Last random:" + IspawnPoint_1.Id);
		}
		if (IspawnPoint_0 != null)
		{
			stringBuilder.AppendLine($"_resultPoint:{IspawnPoint_0.Id}  Position:{IspawnPoint_0.Position}");
		}
		if (Ienumerable_0 != null)
		{
			stringBuilder.AppendLine($"_resultPoints:{Ienumerable_0.Count()} ");
			foreach (ISpawnPoint item4 in Ienumerable_0)
			{
				stringBuilder.AppendLine($"_resultPoint:{item4.Id}  Position:{item4.Position}");
			}
		}
		stringBuilder.AppendLine("===END===" + String_1 + "===END===");
		stopwatch.Stop();
	}
}
