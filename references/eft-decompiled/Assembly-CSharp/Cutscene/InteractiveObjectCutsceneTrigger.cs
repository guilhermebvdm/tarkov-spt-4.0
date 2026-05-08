using System;
using EFT.Interactive;
using UnityEngine;

namespace Cutscene;

public class InteractiveObjectCutsceneTrigger : BaseCutsceneTrigger
{
	[Serializable]
	public class StartCutsceneCondition
	{
		public EDoorState prevObjectState;

		public EDoorState nextObjectState;

		public bool IsPassingCondition(EDoorState prevState, EDoorState nextState)
		{
			if (prevState == prevObjectState)
			{
				return nextState == nextObjectState;
			}
			return false;
		}
	}

	[SerializeField]
	private WorldInteractiveObject interactiveObject;

	[SerializeField]
	private StartCutsceneCondition condition;

	public override void Awake()
	{
		base.Awake();
		interactiveObject.OnDoorStateChanged += method_0;
	}

	public void method_0(WorldInteractiveObject obj, EDoorState prevState, EDoorState nextState)
	{
		if (condition.IsPassingCondition(prevState, nextState))
		{
			method_1();
		}
	}

	public void method_1()
	{
		if (interactiveObject.InteractingPlayer != null && interactiveObject.InteractingPlayer.IsYourPlayer)
		{
			CallStartCutscene(interactiveObject.InteractingPlayer);
		}
	}
}
