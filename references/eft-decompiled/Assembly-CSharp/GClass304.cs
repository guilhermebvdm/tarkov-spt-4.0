using System;
using EFT;
using UnityEngine;
using UnityEngine.AI;

public class GClass304 : GClass177<GClass26>
{
	[NonSerialized]
	public const float Float_0 = 1f;

	[NonSerialized]
	public Vector3 Vector3_0;

	[NonSerialized]
	public Vector3 Vector3_1;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public float Float_1;

	[NonSerialized]
	public GClass179 Gclass179_0;

	public GClass304(BotOwner bot)
		: base(bot)
	{
		Gclass179_0 = new GClass179(bot);
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		if (!Bool_0)
		{
			method_0();
			return;
		}
		BotOwner_0.DoorOpener.UpdateDoorInteractionStatus();
		BotOwner_0.SetPose(1f);
		BotOwner_0.SetTargetMoveSpeed(1f);
		Float_1 -= Time.deltaTime;
		if (Float_1 < 0f)
		{
			Float_1 = 1f;
			BotOwner_0.GoToPoint(Vector3_1);
			if (!BotOwner_0.WeaponManager.UnderbarrelLauncherController.IsActive)
			{
				BotOwner_0.WeaponManager.UnderbarrelLauncherController.TryEnable();
			}
		}
		Vector3 vector = Vector3_1 - BotOwner_0.Transform.position;
		vector.y = ((Mathf.Abs(vector.y) > 1f) ? vector.y : 0f);
		if (vector.magnitude > BotOwner_0.Settings.FileSettings.Move.REACH_DIST)
		{
			if (Float_1 < 0f)
			{
				Float_1 = 1f;
				if (BotOwner_0.GoToPoint(Vector3_1) != NavMeshPathStatus.PathComplete)
				{
					BotOwner_0.Mover.Teleport(Vector3_1);
				}
			}
		}
		else
		{
			method_1();
			Gclass179_0.UpdateNodeByBrain(data as GClass27);
			if (BotOwner_0.Memory.DangerData._place == null)
			{
				BotOwner_0.Memory.DangerData.SetTarget(new PlaceForCheck(Vector3_0, PlaceForCheckType.simple), BotOwner_0);
			}
		}
	}

	public void method_0()
	{
		DebugShootPractice debugShootPractice = UnityEngine.Object.FindObjectOfType<DebugShootPractice>();
		if (debugShootPractice != null)
		{
			Vector3_1 = debugShootPractice.ShootPracticePosition.position;
			Vector3_0 = debugShootPractice.ShootPracticeTarget.position;
			Debug.LogError($"DEBUG SHOOT NODE> OBJECT FOUND!  shootPoint:{Vector3_1}   targetPoint:{Vector3_0}");
		}
		else if (GameObject.Find("ShootPractice") != null)
		{
			GameObject gameObject = GameObject.Find("ShootPracticeTarget");
			Vector3_1 = gameObject.transform.position;
			GameObject gameObject2 = GameObject.Find("ShootPracticePosition");
			Vector3_0 = gameObject2.transform.position;
		}
		if (Vector3_1.sqrMagnitude > 0f && Vector3_0.sqrMagnitude > 0f)
		{
			Bool_0 = true;
		}
	}

	public void method_1()
	{
		BotOwner_0.Memory.ComeToPoint();
		BotOwner_0.SetPose(0f);
		BotOwner_0.StopMove();
	}
}
