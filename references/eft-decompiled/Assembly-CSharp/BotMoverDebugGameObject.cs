using System;
using EFT;
using UnityEngine;

public class BotMoverDebugGameObject : MonoBehaviour
{
	private BotOwner botOwner_0;

	private Vector3 vector3_0;

	private Vector3 vector3_1;

	private Action<Vector3> action_0;

	private float float_0;

	private bool bool_0;

	private bool bool_1;

	public void Init(BotOwner holder, Action<Vector3> setPostion)
	{
		botOwner_0 = holder;
		action_0 = setPostion;
		botOwner_0.OnIPlayerDeadOrUnspawn += method_0;
	}

	public void method_0(IPlayer obj)
	{
		bool_1 = true;
		if (botOwner_0 != null)
		{
			botOwner_0.OnIPlayerDeadOrUnspawn -= method_0;
		}
	}

	public void Update()
	{
		if (bool_1)
		{
			return;
		}
		if (bool_0)
		{
			if (float_0 < Time.time)
			{
				action_0(vector3_1);
				bool_0 = false;
			}
		}
		else if (!botOwner_0.Mover.HasPathAndNoComplete && !((vector3_0 - base.transform.position).sqrMagnitude < 0.1f))
		{
			vector3_0 = base.transform.position;
			vector3_1 = base.transform.position;
			float_0 = Time.time + 0.1f;
			bool_0 = true;
		}
	}

	public void OnDestroy()
	{
		if (botOwner_0 != null)
		{
			botOwner_0.OnIPlayerDeadOrUnspawn -= method_0;
		}
	}

	public void OnDrawGizmosSelected()
	{
		botOwner_0.OnDrawGizmosSelected();
	}
}
