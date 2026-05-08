using EFT.GameTriggers;
using UnityEngine;

namespace CommonAssets.Scripts.Game.GameTriggers.Handlers;

public class HandlerGameObjectState : BaseTriggerHandler
{
	[SerializeField]
	private GameObject[] _controlledGameObjects;

	[SerializeField]
	private bool _targetState;

	[SerializeField]
	private bool _resetStateOnAwake;

	public void Awake()
	{
		if (_resetStateOnAwake && _controlledGameObjects != null && _controlledGameObjects.Length != 0)
		{
			GameObject[] controlledGameObjects = _controlledGameObjects;
			for (int i = 0; i < controlledGameObjects.Length; i++)
			{
				controlledGameObjects[i].SetActive(!_targetState);
			}
		}
	}

	public void Start()
	{
		GClass3592.Instance.Subscribe(_triggerId, method_0);
	}

	public void method_0(TriggerEvent triggerEvent)
	{
		if (_controlledGameObjects != null && _controlledGameObjects.Length != 0)
		{
			GameObject[] controlledGameObjects = _controlledGameObjects;
			for (int i = 0; i < controlledGameObjects.Length; i++)
			{
				controlledGameObjects[i].SetActive(_targetState);
			}
		}
	}
}
