using UnityEngine;

namespace EFT;

public class GameWorldUnityTickListener : MonoBehaviour
{
	private GameWorld gameWorld_0;

	public static GameWorldUnityTickListener Create(GameObject gameObject, GameWorld gameWorld)
	{
		GameWorldUnityTickListener gameWorldUnityTickListener = gameObject.AddComponent<GameWorldUnityTickListener>();
		gameWorldUnityTickListener.gameWorld_0 = gameWorld;
		return gameWorldUnityTickListener;
	}

	public void Update()
	{
		if (!(CameraClass.Instance.Camera == null))
		{
			if (gameWorld_0.UpdateQueue == EUpdateQueue.Update)
			{
				gameWorld_0.DoWorldTick(gameWorld_0.DeltaTime);
			}
			else
			{
				gameWorld_0.DoOtherWorldTick(gameWorld_0.DeltaTime);
			}
		}
	}

	public void FixedUpdate()
	{
		if (!(CameraClass.Instance.Camera == null))
		{
			if (gameWorld_0.UpdateQueue == EUpdateQueue.FixedUpdate)
			{
				gameWorld_0.DoWorldTick(Time.fixedDeltaTime);
			}
			else
			{
				gameWorld_0.DoOtherWorldTick(Time.fixedDeltaTime);
			}
		}
	}

	public void LateUpdate()
	{
		gameWorld_0.LateUpdateWorld(Time.deltaTime);
	}
}
