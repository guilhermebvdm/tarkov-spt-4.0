using System;
using System.Collections.Generic;
using EFT;
using UnityEngine;

public abstract class GClass484 : GClass429
{
	[NonSerialized]
	public List<MedsItemClass> List_0 = new List<MedsItemClass>();

	[NonSerialized]
	public Action<bool> Action_0;

	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public bool Bool_1;

	public bool Using
	{
		get
		{
			if (Bool_0)
			{
				return !Bool_1;
			}
			return false;
		}
		set
		{
			Bool_0 = value;
			if (Bool_0)
			{
				Bool_1 = false;
			}
			Action_0?.Invoke(Bool_0);
		}
	}

	public GClass484(BotOwner owner, Action<bool> callback)
		: base(owner)
	{
		Action_0 = callback;
	}

	public virtual void Activate()
	{
		BotOwner_0.GetPlayer.BeingHitAction += method_0;
	}

	public void CancelCurrent()
	{
		if (!Bool_1 && Bool_0)
		{
			if (Float_0 < Time.time + 3f)
			{
				Float_0 += 5f;
			}
			Bool_1 = true;
			BotOwner_0.WeaponManager.Selector.TakePrevWeapon();
		}
	}

	public void method_0(DamageInfoStruct arg1, EBodyPart arg2, float arg3)
	{
		CancelCurrent();
	}

	public virtual void Dispose()
	{
		BotOwner_0.GetPlayer.BeingHitAction -= method_0;
	}
}
