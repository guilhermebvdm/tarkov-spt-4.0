using Comfort.Common;
using EFT.MovingPlatforms;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshObstacle))]
public class TrainNavMeshCutter : MonoBehaviour
{
	public Locomotive.ETravelState CarvingOn = Locomotive.ETravelState.OnRouteToDestination;

	public Locomotive.ETravelState CarvingOff = Locomotive.ETravelState.OnRouteToDestination;

	private BotEventHandler botEventHandler;

	private NavMeshObstacle navMeshObstacle_0;

	public void Awake()
	{
		navMeshObstacle_0 = GetComponent<NavMeshObstacle>();
		navMeshObstacle_0.carving = false;
	}

	public void Start()
	{
		if (!Singleton<BotEventHandler>.Instantiated)
		{
			Singleton<BotEventHandler>.Instance = new BotEventHandler();
		}
		botEventHandler = Singleton<BotEventHandler>.Instance;
		botEventHandler.OnTrainCome += method_0;
	}

	public void OnDestroy()
	{
		botEventHandler.OnTrainCome -= method_0;
	}

	public void method_0(Locomotive.ETravelState obj)
	{
		if (obj == CarvingOn)
		{
			navMeshObstacle_0.carving = true;
		}
		else if (obj == CarvingOff)
		{
			navMeshObstacle_0.carving = false;
		}
	}
}
