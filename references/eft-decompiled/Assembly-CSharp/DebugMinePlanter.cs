using System.Collections.Generic;
using Comfort.Common;
using EFT;
using EFT.SynchronizableObjects;
using UnityEngine;
using UnityEngine.AI;

public class DebugMinePlanter : MonoBehaviour
{
	private IBotGame ibotGame_0;

	public int MassPlantCount = 10;

	public float MassPlantRad = 20f;

	private Dictionary<TripwireSynchronizableObject, PlantedMineAIInfo> dictionary_0 = new Dictionary<TripwireSynchronizableObject, PlantedMineAIInfo>();

	public void DoPlant()
	{
		method_0(base.transform.position);
	}

	public void method_0(Vector3 pos)
	{
		if (Singleton<BotEventHandler>.Instance != null && NavMesh.SamplePosition(pos, out var hit, 3f, -1))
		{
			Vector3 position = hit.position;
			GameWorld instance = Singleton<GameWorld>.Instance;
			if (instance != null)
			{
				TripwireSynchronizableObject throwWeap = (TripwireSynchronizableObject)instance.SynchronizableObjectLogicProcessor.TakeFromPool(SynchronizableObjectType.Tripwire);
				Singleton<BotEventHandler>.Instance.PlantTripwire(throwWeap, position);
				Debug.DrawLine(position, position + Vector3.up * 20f, Color.green, 10f);
				return;
			}
		}
		Debug.DrawLine(pos, pos + Vector3.up * 20f, Color.red, 10f);
	}

	public void MassRndPlant()
	{
		Vector3 pos = base.transform.position;
		for (int i = 0; i < MassPlantCount; i++)
		{
			float b = MassPlantRad * 0.5f;
			float num = GClass856.Random(0f, b) * (float)GClass856.RandomSing();
			float num2 = GClass856.Random(0f, b) * (float)GClass856.RandomSing();
			pos = new Vector3(num + pos.x, 0f, num2 + pos.z);
			method_0(pos);
		}
	}

	public void Awake()
	{
		ibotGame_0 = null;
	}

	public void Update()
	{
		if (Application.isPlaying && ibotGame_0 == null)
		{
			ibotGame_0 = Singleton<IBotGame>.Instance;
			if (ibotGame_0 != null)
			{
				ibotGame_0.BotsController.PlantedMines.OnAdd += method_2;
				ibotGame_0.BotsController.PlantedMines.OnRemove += method_1;
			}
		}
	}

	public void method_1(TripwireSynchronizableObject objmine, PlantedMineAIInfo aimineinfo)
	{
		dictionary_0.Remove(objmine);
	}

	public void method_2(TripwireSynchronizableObject objmine, PlantedMineAIInfo aimineinfo)
	{
		dictionary_0.Add(objmine, aimineinfo);
	}

	public void Clear()
	{
		dictionary_0 = new Dictionary<TripwireSynchronizableObject, PlantedMineAIInfo>();
	}

	public void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireCube(base.transform.position, Vector3.one * 1f);
		Gizmos.DrawWireSphere(base.transform.position, MassPlantRad);
		Gizmos.color = Color.white;
		foreach (KeyValuePair<TripwireSynchronizableObject, PlantedMineAIInfo> item in dictionary_0)
		{
			Vector3 pos = item.Value.Pos;
			Gizmos.DrawSphere(pos, 0.1f);
			Gizmos.DrawWireSphere(pos, 0.12f);
			Gizmos.DrawWireSphere(pos, 0.13f);
		}
	}
}
