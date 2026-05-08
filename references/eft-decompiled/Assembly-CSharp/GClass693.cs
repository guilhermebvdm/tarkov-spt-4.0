using System;
using System.Text;
using EFT.Game.Spawning;
using UnityEngine;

public class GClass693
{
	public class GClass694
	{
		[NonSerialized]
		public int Int_0;

		[NonSerialized]
		public Vector3 Vector3_0;

		[NonSerialized]
		public ISpawnPointCollider IspawnPointCollider_0;

		[NonSerialized]
		public bool Bool_0;

		public GClass694(int playerId, Vector3 playerPosition, ISpawnPointCollider spawnPointCollider, bool contain)
		{
			Int_0 = playerId;
			Vector3_0 = playerPosition;
			IspawnPointCollider_0 = spawnPointCollider;
			Bool_0 = contain;
		}

		public StringBuilder Debug()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append($"playerId:{Int_0} ");
			stringBuilder.Append($"playerPosition:{Vector3_0} ");
			stringBuilder.Append("collider:" + IspawnPointCollider_0.DebugInfo() + " ");
			stringBuilder.Append($"contain:{Bool_0} ");
			return stringBuilder;
		}
	}

	public class GClass695
	{
		[NonSerialized]
		public int Int_0;

		[NonSerialized]
		public Vector3 Vector3_0;

		[NonSerialized]
		public Vector3 Vector3_1;

		[NonSerialized]
		public float Float_0;

		[NonSerialized]
		public float Float_1;

		public GClass695(int playerId, Vector3 playerPosition, Vector3 spawnPointPosition, float sDist, float distanceSqr)
		{
			Int_0 = playerId;
			Vector3_0 = playerPosition;
			Vector3_1 = spawnPointPosition;
			Float_0 = sDist;
			Float_1 = distanceSqr;
		}

		public StringBuilder Debug()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append($"playerId:{Int_0} ");
			stringBuilder.Append($"playerPosition:{Vector3_0} ");
			stringBuilder.Append($"spawnPointPosition:{Vector3_1} ");
			stringBuilder.Append($"sDist:{Float_0} < distanceSqr:{Float_1} ");
			stringBuilder.Append($"isDistOk:{Float_0 < Float_1} ");
			return stringBuilder;
		}
	}

	[NonSerialized]
	public string String_0;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public bool Bool_1;

	[NonSerialized]
	public GClass695 Gclass695_0;

	[NonSerialized]
	public GClass694 Gclass694_0;

	public GClass693(string id)
	{
		String_0 = id;
	}

	public StringBuilder DebugInfo()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("_id:" + String_0 + " ");
		stringBuilder.Append($"_carePlayersIsNull:{Bool_0} ");
		stringBuilder.Append($"_carePlayersIsGood:{Bool_1} ");
		if (Gclass695_0 != null)
		{
			stringBuilder.AppendLine(Gclass695_0.Debug().ToString());
		}
		if (Gclass694_0 != null)
		{
			stringBuilder.AppendLine(Gclass694_0.Debug().ToString());
		}
		stringBuilder.Append("--------");
		return stringBuilder;
	}

	public void CarePlayersIsNull()
	{
		Bool_0 = true;
	}

	public void CheckPlayer(int playerId, Vector3 playerPosition, ISpawnPointCollider spawnPointCollider, bool contain)
	{
		Gclass694_0 = new GClass694(playerId, playerPosition, spawnPointCollider, contain);
	}

	public void CheckDist(int playerId, Vector3 playerPosition, Vector3 spawnPointPosition, float sDist, float distanceSqr)
	{
		Gclass695_0 = new GClass695(playerId, playerPosition, spawnPointPosition, sDist, distanceSqr);
	}

	public void CarePlayersIsGood()
	{
		Bool_1 = true;
	}
}
