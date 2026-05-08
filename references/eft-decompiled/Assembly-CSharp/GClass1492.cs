using System.Text;
using AnimationEventSystem;
using EFT;
using EFT.InventoryLogic;
using UnityEngine;

public abstract class GClass1492
{
	public static string CreateStateSnapshot(Player player)
	{
		StringBuilder stringBuilder = new StringBuilder();
		smethod_0(stringBuilder, player);
		if (player.HandsController is Player.FirearmController firearmController)
		{
			smethod_1(stringBuilder, player.ProfileId, firearmController);
		}
		smethod_3(stringBuilder, player.HandsController);
		smethod_4(stringBuilder, "Hands animator", player.HandsAnimator.Animator);
		smethod_2(stringBuilder, player.MovementContext);
		smethod_4(stringBuilder, "Body animator", player.BodyAnimatorCommon);
		return stringBuilder.ToString();
	}

	public static void smethod_0(StringBuilder output, Player player)
	{
		smethod_5(output, $"Snapshot of player {player.Profile.Nickname}, IsAI {player.IsAI}");
		string currentHandsOperationName = (player.HandsController as Player.ItemHandsController).CurrentHandsOperationName;
		output.AppendFormat("HandsOperation: {0}", currentHandsOperationName);
		output.AppendLine();
	}

	public static void smethod_1(StringBuilder output, string playerProfileId, Player.FirearmController firearmController)
	{
		Weapon item = firearmController.Item;
		bool num = item.GetCurrentMagazine() != null;
		smethod_5(output, " ==== Weapon logical state ==== ");
		output.Append(GClass2348.Localized(item.ShortName));
		output.AppendLine();
		output.AppendFormat("IsTriggerPressed: {0}", firearmController.IsTriggerPressed);
		output.AppendLine();
		if (num)
		{
			MagazineItemClass currentMagazine = item.GetCurrentMagazine();
			output.AppendFormat("Mag inserted, ammo: {0}/{1}", currentMagazine.Count, currentMagazine.MaxCount);
			output.AppendLine();
		}
		else
		{
			output.Append("Mag not inserted");
			output.AppendLine();
		}
		if (item.HasChambers)
		{
			for (int i = 0; i < item.Chambers.Length; i++)
			{
				string arg = ((!(item.Chambers[i].ContainedItem is AmmoItemClass ammoItemClass)) ? "empty" : ("full (" + GClass2348.Localized(ammoItemClass.ShortName) + ")"));
				output.AppendFormat("Chamber #{0} is {1}", i, arg);
				output.AppendLine();
			}
		}
		else
		{
			output.AppendLine("No chambers");
			output.AppendLine();
		}
		bool flag = item.MalfState.IsKnownMalfunction(playerProfileId);
		output.AppendFormat("Malfunction: {0}, source: {1}, isKnown: {2}", item.MalfState.State, item.MalfState.Source, flag);
		output.AppendLine();
	}

	public static void smethod_2(StringBuilder output, MovementContext movementContext)
	{
		smethod_5(output, " ==== Movement Context logical state ==== ");
		output.AppendLine($"CurrentState: {movementContext.CurrentState.Name} {movementContext.CurrentState.Type}");
		output.AppendLine();
		output.AppendLine($"CharacterMovementSpeed: {movementContext.CharacterMovementSpeed}");
		output.AppendLine();
		output.AppendLine($"SmoothedCharacterMovementSpeed: {movementContext.SmoothedCharacterMovementSpeed}");
		output.AppendLine();
		output.AppendLine($"SprintSpeed: {movementContext.SprintSpeed}");
		output.AppendLine();
		output.AppendLine($"IsSprintEnabled: {movementContext.IsSprintEnabled}");
		output.AppendLine();
		output.AppendLine($"StateSpeedLimit: {movementContext.StateSpeedLimit}");
		output.AppendLine();
		output.AppendLine($"StateSprintSpeedLimit: {movementContext.StateSprintSpeedLimit}");
		output.AppendLine();
		output.AppendLine($"MaxSpeed: {movementContext.MaxSpeed}");
		output.AppendLine();
	}

	public static void smethod_3(StringBuilder output, Player.AbstractHandsController firearmController)
	{
		smethod_5(output, " ==== Animation events history ====");
		int num = 0;
		foreach (AnimationEventsSequenceData.GStruct142 item in firearmController.AnimationEventsEmitter.EventsSequenceData.AnimationEventsDebugQueue)
		{
			string animStateByNameHash = GClass758.GetAnimStateByNameHash(item.StateNameShortHash);
			output.AppendFormat("Event #{0}: {1}.{2}, conditionPassed: {3}", num++, animStateByNameHash, item.EventName, item.ConditionPassed);
			output.AppendLine();
		}
	}

	public static void smethod_4(StringBuilder output, string caption, IAnimator animator)
	{
		smethod_5(output, " ==== " + caption + " ====");
		for (int i = 0; i < animator.layerCount; i++)
		{
			AnimatorStateInfoWrapper currentAnimatorStateInfo = animator.GetCurrentAnimatorStateInfo(i);
			string animStateByNameHash = GClass758.GetAnimStateByNameHash(currentAnimatorStateInfo.shortNameHash);
			output.AppendFormat("Layer #{0} ", i);
			output.AppendFormat("currentState: {0} ({1}), normalizedTime: {2}", animStateByNameHash, currentAnimatorStateInfo.shortNameHash, currentAnimatorStateInfo.normalizedTime);
			if (animator.IsInTransition(i))
			{
				AnimatorStateInfoWrapper nextAnimatorStateInfo = animator.GetNextAnimatorStateInfo(i);
				string animStateByNameHash2 = GClass758.GetAnimStateByNameHash(nextAnimatorStateInfo.shortNameHash);
				output.AppendFormat(", nextState: {0} ({1}), normalizedTime: {2}", animStateByNameHash2, nextAnimatorStateInfo.shortNameHash, nextAnimatorStateInfo.normalizedTime);
			}
			output.AppendLine();
		}
		output.AppendLine();
		output.AppendFormat("Parameters");
		output.AppendLine();
		for (int j = 0; j < animator.parameterCount; j++)
		{
			AnimatorParameterInfo parameter = animator.GetParameter(j);
			string text = string.Empty;
			switch (parameter.type)
			{
			case AnimatorControllerParameterType.Float:
				text = animator.GetFloat(parameter.nameHash).ToString();
				break;
			case AnimatorControllerParameterType.Int:
				text = animator.GetInteger(parameter.nameHash).ToString();
				break;
			case AnimatorControllerParameterType.Bool:
			case AnimatorControllerParameterType.Trigger:
				text = animator.GetBool(parameter.nameHash).ToString();
				break;
			}
			string text2 = WeaponAnimationSpeedControllerClass.ParameterHashToName(parameter.nameHash);
			output.AppendFormat("{0} = {1}   ({2}, {3})", text2, text, parameter.nameHash, parameter.type);
			output.AppendLine();
		}
	}

	public static void smethod_5(StringBuilder output, string caption)
	{
		output.AppendLine();
		output.Append(caption);
		output.AppendLine();
	}
}
