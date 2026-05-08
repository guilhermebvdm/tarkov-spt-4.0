using System;
using EFT;
using EFT.HealthSystem;
using EFT.InventoryLogic;

public class GClass487 : BotFirstAidClass
{
	[NonSerialized]
	public const string String_0 = "590c657e86f77412b013051d";

	[NonSerialized]
	public const string String_1 = "5e99711486f7744bfc4af328";

	public override float min_percent => -1f;

	public static void LogToAllPArts(string info, ActiveHealthController hc)
	{
		string text = "";
		EBodyPart[] array = (EBodyPart[])Enum.GetValues(typeof(EBodyPart));
		for (int i = 0; i < array.Length; i++)
		{
			EBodyPart bodyPart = array[i];
			ValueStruct bodyPartHealth = hc.GetBodyPartHealth(bodyPart);
			text += $" {bodyPart.ToString()}:({bodyPartHealth.Current})";
		}
	}

	public GClass487(BotOwner owner, Action<bool> callback)
		: base(owner, callback)
	{
	}

	public override void SetRandomPartToHeal()
	{
		method_5("590c657e86f77412b013051d");
		if (base.CurUsingMeds == null)
		{
			method_5("5e99711486f7744bfc4af328");
		}
		if (base.CurUsingMeds != null && !Nullable_0.HasValue)
		{
			Nullable_0 = EBodyPart.LeftArm;
			((ActiveHealthController)BotOwner_0.HealthController)?.ApplyDamage(Nullable_0.Value, 1f, default(DamageInfoStruct));
		}
	}

	public override void RefreshMeds()
	{
		method_5("5e99711486f7744bfc4af328");
	}

	public override void FirstAidApplied()
	{
		BotOwner_0.GetPlayer.ActiveHealthController.RestoreFullHealth();
	}

	public void method_5(string id)
	{
		Player getPlayer = BotOwner_0.GetPlayer;
		EquipmentSlot[] secureSlots = BotMedecine.secureSlots;
		List_0.Clear();
		getPlayer.InventoryController.GetAcceptableItemsNonAlloc(secureSlots, List_0);
		foreach (MedsItemClass item in List_0)
		{
			if (item.TemplateId == (MongoID)id)
			{
				base.CurUsingMeds = item;
				return;
			}
		}
		base.CurUsingMeds = null;
	}
}
