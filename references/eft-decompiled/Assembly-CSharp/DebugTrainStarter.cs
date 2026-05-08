using System.Runtime.CompilerServices;
using Comfort.Common;
using EFT;
using EFT.MovingPlatforms;
using UnityEngine;

public class DebugTrainStarter : MonoBehaviour, IPhysicsTrigger
{
	public bool TrainComeAction = true;

	public GameObject GameObject;

	public GameObject TrainEffect;

	[CompilerGenerated]
	private readonly string string_0;

	public string Description
	{
		[CompilerGenerated]
		get
		{
			return string_0;
		}
	}

	bool IPhysicsTrigger.enabled
	{
		get
		{
			return base.enabled;
		}
		set
		{
			base.enabled = value;
		}
	}

	public void Awake()
	{
		GameObject.SetActive(TrainComeAction);
		base.gameObject.layer = LayerMask.NameToLayer("Triggers");
	}

	public void OnTriggerEnter(Collider other)
	{
		Player playerByCollider = Singleton<GameWorld>.Instance.GetPlayerByCollider(other);
		if (!(playerByCollider == null) && playerByCollider.AIData != null && !playerByCollider.AIData.IsAI)
		{
			BotEventHandler instance = Singleton<BotEventHandler>.Instance;
			if (instance != null)
			{
				Debug.LogError("DEBUG EVENT: TrainCome:" + TrainComeAction);
				Locomotive.ETravelState val = (TrainComeAction ? Locomotive.ETravelState.OnRouteToDestination : Locomotive.ETravelState.OnRouteBack);
				instance.TrainCome(val);
				TrainEffect.SetActive(!TrainEffect.activeInHierarchy);
			}
		}
	}

	public void OnTriggerExit(Collider col)
	{
	}
}
