using System;
using System.Linq;
using System.Runtime.CompilerServices;
using EFT.InventoryLogic;
using UnityEngine;

namespace EFT;

public abstract class NetworkPlayer : Player
{
	[Serializable]
	public struct ClientShot
	{
		public bool Approved;

		public int BodyPart;

		public float Damage;

		public float LeftBodyPartHealth;

		public string TargetName;

		public override string ToString()
		{
			return $"Approved: {Approved}, BodyPart: {(EBodyPart)BodyPart}, Damage: {Damage}, LeftBodyPartHealth: {LeftBodyPartHealth}, TargetName: {TargetName}";
		}
	}

	[Serializable]
	[CompilerGenerated]
	public class Class1379
	{
		public static readonly Class1379 class1379_0 = new Class1379();

		public static Func<Slot, Item> func_0;

		public Item method_0(Slot slot)
		{
			return slot.ContainedItem;
		}
	}

	[CompilerGenerated]
	public class Class1380
	{
		public string id;

		public bool method_0(Item item)
		{
			return item?.Id == id;
		}

		public bool method_1(Item item)
		{
			return item.Id == id;
		}
	}

	public static readonly EPhraseTrigger[] LocalPhrases = new EPhraseTrigger[4]
	{
		EPhraseTrigger.OnAgony,
		EPhraseTrigger.OnBeingHurt,
		EPhraseTrigger.OnBreath,
		EPhraseTrigger.OnDeath
	};

	[CompilerGenerated]
	private bool bool_0;

	[CompilerGenerated]
	private IFrameIndexer iFrameIndexer;

	public override bool IsVisible
	{
		[CompilerGenerated]
		get
		{
			return bool_0;
		}
		[CompilerGenerated]
		set
		{
			bool_0 = value;
		}
	}

	public override AbstractHandsController HandsController
	{
		get
		{
			return _handsController;
		}
		set
		{
			if ((object)value != _handsController)
			{
				base.HandsController = value;
			}
		}
	}

	public new abstract PlayerInventoryController InventoryController { get; }

	public IFrameIndexer FrameIndexer
	{
		[CompilerGenerated]
		get
		{
			return iFrameIndexer;
		}
		[CompilerGenerated]
		set
		{
			iFrameIndexer = value;
		}
	}

	public static T smethod_2<T>(GameWorld gameWorld, ResourceKey assetName, int playerId, Vector3 position, string prefix, IFrameIndexer frameIndexer, EUpdateQueue updateQueue, EUpdateMode armsUpdateMode, EUpdateMode bodyUpdateMode, CharacterControllerSpawner.Mode characterControllerMode, Func<float> getSensitivity, Func<float> getAimingSensitivity, bool isThirdPerson, bool useSimplifiedSkeleton = false) where T : NetworkPlayer
	{
		T val = Player.Create<T>(gameWorld, assetName, playerId, position, updateQueue, armsUpdateMode, bodyUpdateMode, characterControllerMode, getSensitivity, getAimingSensitivity, prefix, isThirdPerson, useSimplifiedSkeleton);
		val.FrameIndexer = frameIndexer;
		return val;
	}

	public override void OnDeserializeFromServer(byte channelId, IDataReader reader)
	{
		throw new Exception("Invalid operation");
	}

	public override void Dispose()
	{
		base.Dispose();
		FrameIndexer = null;
	}

	public void ProcessPoisonResourceChange(string id, float value)
	{
		using (GClass4062.BeginSampleWithToken("NetworkPlayer.ProcessPoisonResourceChange", "ProcessPoisonResourceChange"))
		{
			Item item = base.Inventory.Equipment.Slots.Select((Slot slot) => slot.ContainedItem).FirstOrDefault((Item item2) => item2?.Id == id) ?? base.Inventory.GetPlayerItems().FirstOrDefault((Item item2) => item2.Id == id);
			if (item == null)
			{
				GStruct156<Item> gStruct = base.GameWorld.FindItemById(id);
				if (gStruct.Failed)
				{
					Debug.LogError("Error: ProcessRpcPoisonResourceChange: " + gStruct.Error);
					return;
				}
				item = gStruct.Value;
			}
			if (item.TryGetItemComponent<SideEffectComponent>(out var component))
			{
				component.Value = value;
				item.RaiseRefreshEvent(refreshIcon: false, checkMagazine: false);
			}
		}
	}

	public void ProcessArmorPointsChange(string id, float value)
	{
		_preAllocatedArmorComponents.Clear();
		base.Inventory.GetPutOnArmorsNonAlloc(_preAllocatedArmorComponents);
		foreach (ArmorComponent preAllocatedArmorComponent in _preAllocatedArmorComponents)
		{
			if (preAllocatedArmorComponent.Item.Id == id)
			{
				preAllocatedArmorComponent.Repairable.Durability = value;
				preAllocatedArmorComponent.Buff.TryDisableComponent(preAllocatedArmorComponent.Repairable.Durability);
				preAllocatedArmorComponent.Item.RaiseRefreshEvent(refreshIcon: false, checkMagazine: false);
				return;
			}
		}
		GStruct156<Item> gStruct = base.GameWorld.FindItemById(id);
		if (gStruct.Failed)
		{
			Debug.LogError("Error: ProcessRpcArmorPointsChange: " + gStruct.Error);
			return;
		}
		ArmorComponent itemComponent = gStruct.Value.GetItemComponent<ArmorComponent>();
		if (itemComponent != null)
		{
			itemComponent.Repairable.Durability = value;
			itemComponent.Buff.TryDisableComponent(itemComponent.Repairable.Durability);
			itemComponent.Item.RaiseRefreshEvent(refreshIcon: false, checkMagazine: false);
		}
	}

	public void ProcessChangeSkillExperience(GStruct261? packet, bool silent)
	{
		if (base.Skills == null)
		{
			return;
		}
		while (packet.HasValue)
		{
			GStruct261 valueOrDefault = packet.GetValueOrDefault();
			try
			{
				ESkillId skillId = (ESkillId)valueOrDefault.SkillId;
				(base.Skills.GetSkill(skillId) ?? throw new Exception("cant find skill: " + skillId)).UpdateFromServer(valueOrDefault, silent);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			packet = GClass2243.GetNested(valueOrDefault);
		}
	}

	public void ProcessChangeMasteringExperience(GStruct262? packet, bool silent)
	{
		while (packet.HasValue)
		{
			GStruct262 valueOrDefault = packet.GetValueOrDefault();
			try
			{
				base.Skills?.ChangeMasteringLevel(valueOrDefault.GroupName, valueOrDefault.Value, silent);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			packet = GClass2243.GetNested(valueOrDefault);
		}
	}

	public NetworkPlayer()
	{
	}
}
