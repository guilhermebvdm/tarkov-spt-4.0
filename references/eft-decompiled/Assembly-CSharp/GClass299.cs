using System;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

public class GClass299 : GClass177<GClass26>
{
	[CompilerGenerated]
	public class Class120
	{
		public BotWeaponManager weapons;

		public GClass299 gclass299_0;

		public void method_0()
		{
			// Found self-referencing delegate construction. Abort transformation to avoid stack overflow.
			IKnifeController knifeController = weapons.Melee.KnifeController;
			knifeController.ComboPlanning = (Action)Delegate.Remove(knifeController.ComboPlanning, new Action(method_0));
			if (UnityEngine.Random.Range(0, 100) > 50)
			{
				weapons.Melee.KnifeController.ContinueCombo();
			}
			else
			{
				weapons.Melee.KnifeController.BrakeCombo();
			}
		}

		public void method_1()
		{
			// Found self-referencing delegate construction. Abort transformation to avoid stack overflow.
			IKnifeController knifeController = weapons.Melee.KnifeController;
			knifeController.OnAttackEnd = (Action)Delegate.Remove(knifeController.OnAttackEnd, new Action(method_1));
			IKnifeController knifeController2 = weapons.Melee.KnifeController;
			knifeController2.ComboPlanning = (Action)Delegate.Remove(knifeController2.ComboPlanning, (Action)delegate
			{
				// Found self-referencing delegate construction. Abort transformation to avoid stack overflow.
				IKnifeController knifeController3 = weapons.Melee.KnifeController;
				knifeController3.ComboPlanning = (Action)Delegate.Remove(knifeController3.ComboPlanning, new Action(method_0));
				if (UnityEngine.Random.Range(0, 100) > 50)
				{
					weapons.Melee.KnifeController.ContinueCombo();
				}
				else
				{
					weapons.Melee.KnifeController.BrakeCombo();
				}
			});
			gclass299_0.Float_0 = Time.time + 2f;
		}
	}

	[NonSerialized]
	public float Float_0;

	public GClass299(BotOwner bot)
		: base(bot)
	{
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		BotOwner_0.GetPlayer.Physical.Stamina.Current = 10000f;
		BotOwner_0.WeaponManager.Melee.SetDebugCombo(state: true);
		if (BotOwner_0.WeaponManager.Selector.CanChangeToMeleeWeapons)
		{
			BotOwner_0.WeaponManager.Selector.ChangeToMelee();
		}
		method_0();
	}

	public void method_0()
	{
		Class120 CS_0024_003C_003E8__locals19 = new Class120();
		CS_0024_003C_003E8__locals19.gclass299_0 = this;
		if (Float_0 > Time.time)
		{
			return;
		}
		CS_0024_003C_003E8__locals19.weapons = BotOwner_0.WeaponManager;
		if (!CS_0024_003C_003E8__locals19.weapons.IsMelee || CS_0024_003C_003E8__locals19.weapons.Melee.KnifeController == null)
		{
			return;
		}
		Float_0 = float.MaxValue;
		IKnifeController knifeController = CS_0024_003C_003E8__locals19.weapons.Melee.KnifeController;
		knifeController.OnAttackEnd = (Action)Delegate.Combine(knifeController.OnAttackEnd, (Action)delegate
		{
			// Found self-referencing delegate construction. Abort transformation to avoid stack overflow.
			IKnifeController knifeController3 = CS_0024_003C_003E8__locals19.weapons.Melee.KnifeController;
			knifeController3.OnAttackEnd = (Action)Delegate.Remove(knifeController3.OnAttackEnd, new Action(CS_0024_003C_003E8__locals19.method_1));
			IKnifeController knifeController4 = CS_0024_003C_003E8__locals19.weapons.Melee.KnifeController;
			knifeController4.ComboPlanning = (Action)Delegate.Remove(knifeController4.ComboPlanning, (Action)delegate
			{
				// Found self-referencing delegate construction. Abort transformation to avoid stack overflow.
				IKnifeController knifeController5 = CS_0024_003C_003E8__locals19.weapons.Melee.KnifeController;
				knifeController5.ComboPlanning = (Action)Delegate.Remove(knifeController5.ComboPlanning, new Action(CS_0024_003C_003E8__locals19.method_0));
				if (UnityEngine.Random.Range(0, 100) > 50)
				{
					CS_0024_003C_003E8__locals19.weapons.Melee.KnifeController.ContinueCombo();
				}
				else
				{
					CS_0024_003C_003E8__locals19.weapons.Melee.KnifeController.BrakeCombo();
				}
			});
			CS_0024_003C_003E8__locals19.gclass299_0.Float_0 = Time.time + 2f;
		});
		IKnifeController knifeController2 = CS_0024_003C_003E8__locals19.weapons.Melee.KnifeController;
		knifeController2.ComboPlanning = (Action)Delegate.Combine(knifeController2.ComboPlanning, (Action)delegate
		{
			// Found self-referencing delegate construction. Abort transformation to avoid stack overflow.
			IKnifeController knifeController3 = CS_0024_003C_003E8__locals19.weapons.Melee.KnifeController;
			knifeController3.ComboPlanning = (Action)Delegate.Remove(knifeController3.ComboPlanning, new Action(CS_0024_003C_003E8__locals19.method_0));
			if (UnityEngine.Random.Range(0, 100) > 50)
			{
				CS_0024_003C_003E8__locals19.weapons.Melee.KnifeController.ContinueCombo();
			}
			else
			{
				CS_0024_003C_003E8__locals19.weapons.Melee.KnifeController.BrakeCombo();
			}
		});
		CS_0024_003C_003E8__locals19.weapons.Melee.KnifeController.MakeKnifeKick();
	}
}
