using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

public class GClass296(BotOwner bot) : GClass177<GClass26>(bot)
{
	[CompilerGenerated]
	public class Class119
	{
		public GameObject tr;

		public Vector3 method_0()
		{
			return tr.transform.position;
		}
	}

	[NonSerialized]
	public const float Float_0 = 1f;

	[NonSerialized]
	public Func<Vector3> Func_0;

	[NonSerialized]
	public Vector3 Vector3_0;

	[NonSerialized]
	public float Float_1;

	[NonSerialized]
	public float Float_2;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public bool Bool_1;

	[NonSerialized]
	public bool Bool_2;

	[NonSerialized]
	public List<AIGreandeAng> List_0 = new List<AIGreandeAng>
	{
		AIGreandeAng.ang15,
		AIGreandeAng.ang25,
		AIGreandeAng.ang35,
		AIGreandeAng.ang45,
		AIGreandeAng.ang55,
		AIGreandeAng.ang65
	};

	[NonSerialized]
	public float Float_3;

	public override void UpdateNodeByBrain(GClass26 data)
	{
		if (Bool_1)
		{
			method_0();
			return;
		}
		if (!BotOwner_0.WeaponManager.Grenades.HaveGrenade)
		{
			BotOwner_0.StopMove();
			return;
		}
		if (!Bool_0)
		{
			method_2();
			BotOwner_0.StopMove();
			if (Bool_2)
			{
				method_0();
			}
			return;
		}
		method_1();
		if (BotOwner_0.WeaponManager.Grenades.ReadyToThrow)
		{
			BotOwner_0.WeaponManager.Grenades.DoThrow();
			return;
		}
		BotGrenadeController grenades = BotOwner_0.WeaponManager.Grenades;
		if (!grenades.HaveGrenade)
		{
			return;
		}
		if (Bool_2 && Float_2 < Time.time)
		{
			Float_2 = Time.time + 5f;
			if (Func_0 != null)
			{
				Vector3 vector = Func_0();
				AIGreandeAng greandeAng = GClass856.RandomElement(List_0);
				AIGreanageThrowData aIGreanageThrowData = GClass577.CanThrowGrenade2(Vector3_0 + BotOwner.STAY_HEIGHT, vector, 100f, greandeAng);
				if (!aIGreanageThrowData.CanThrow)
				{
					Debug.LogError($"can' thow:{greandeAng.ToString()}  to:{vector}");
					return;
				}
				grenades.SetThrowData(aIGreanageThrowData);
				Debug.LogError($"TryThrowNow3 ang:{greandeAng.ToString()} to:{vector}");
				BotOwner_0.WeaponManager.Grenades.DoThrow();
			}
		}
		if (grenades.ReadyToThrow && grenades.AIGreanageThrowData.IsUpToDate() && BotOwner_0.Settings.FileSettings.Grenade.STOP_WHEN_THROW_GRENADE)
		{
			BotOwner_0.StopMove();
		}
	}

	public void method_0()
	{
		if (BotOwner_0.WeaponManager.Grenades.ReadyToThrow)
		{
			Debug.LogError("TryThrowNow");
			BotOwner_0.WeaponManager.Grenades.DoThrow();
			return;
		}
		BotGrenadeController grenades = BotOwner_0.WeaponManager.Grenades;
		if (!grenades.HaveGrenade)
		{
			return;
		}
		BotOwner_0.StopMove();
		if (Float_2 < Time.time)
		{
			Float_2 = Time.time + 5f;
			AIGreanageThrowData aIGreanageThrowData = GClass577.CanThrowGrenade2(BotOwner_0.Position, BotOwner_0.Position + Vector3.up * 5f + Vector3.forward * 10f, 100f, AIGreandeAng.ang45);
			if (aIGreanageThrowData.CanThrow)
			{
				grenades.SetThrowData(aIGreanageThrowData);
				Debug.LogError("TryThrowNow2");
				BotOwner_0.WeaponManager.Grenades.DoThrow();
			}
		}
	}

	public void method_1()
	{
		if (!(Float_3 > Time.time))
		{
			Float_3 = Time.time + 1f;
			if ((BotOwner_0.Position - Vector3_0).sqrMagnitude > 0.5f)
			{
				BotOwner_0.SetPose(1f);
				BotOwner_0.GoToPoint(Vector3_0);
			}
			else
			{
				Bool_2 = true;
				BotOwner_0.StopMove();
			}
		}
	}

	public void method_2()
	{
		if (GameObject.Find("ShootPractice") != null)
		{
			GameObject tr = GameObject.Find("ShootPracticeTarget");
			Func_0 = () => tr.transform.position;
			GameObject gameObject = GameObject.Find("ShootPracticePosition");
			Vector3_0 = gameObject.transform.position;
			BotOwner_0.Mover.Teleport(Vector3_0);
		}
		Bool_0 = true;
	}
}
