using System;
using System.Collections.Generic;
using System.Reflection;
using EFT;
using EFT.Ballistics;
using EFT.InventoryLogic;
using SPT.Reflection.Patching;
using UnityEngine;

namespace VisceralCombat.Ragdolls.Patches;

public class BodiesImpulsePatch : ModulePatch
{
	private static Dictionary<string, float> _dictionary = new Dictionary<string, float>
	{
		{ "545x39", 5f },
		{ "556x45", 5.5f },
		{ "762x39", 8f },
		{ "762x51", 12f },
		{ "762x54R", 15f },
		{ "127x55", 25f },
		{ "9x19", 2.5f },
		{ "9x18PM", 2f },
		{ "45ACP", 4f },
		{ "357", 6f },
		{ "44Mag", 8f },
		{ "50AE", 15f },
		{ "12g", 20f },
		{ "20g", 15f },
		{ "23x75", 35f },
		{ "300BLK", 7f },
		{ "338LM", 30f },
		{ "366TKM", 9f },
		{ "46x30", 3.5f },
		{ "57x28", 4f },
		{ "9x21", 3.5f },
		{ "9x39", 7.5f }
	};

	private static Dictionary<string, float> _bonedictionary = new Dictionary<string, float>
	{
		{ "Base HumanPelvis", 3.5f },
		{ "Base HumanSpine1", 3.5f },
		{ "Base HumanSpine2", 3.5f },
		{ "Base HumanSpine3", 3.5f },
		{ "Base HumanRibcage", 3.5f },
		{ "Base HumanHead", 3.5f },
		{ "Base HumanNeck", 3.5f },
		{ "Base HumanLUpperarm", 0.75f },
		{ "Base HumanLForearm1", 1f },
		{ "Base HumanRUpperarm", 0.75f },
		{ "Base HumanRForearm1", 1f },
		{ "Base HumanLThigh1", 0.75f },
		{ "Base HumanLThigh2", 0.8f },
		{ "Base HumanLCalf", 1f },
		{ "Base HumanLFoot", 1f },
		{ "Base HumanLToe", 1f },
		{ "Base HumanRThigh1", 0.75f },
		{ "Base HumanRThigh2", 0.8f },
		{ "Base HumanRCalf", 1f },
		{ "Base HumanRFoot", 1f },
		{ "Base HumanRToe", 1f }
	};

	protected override MethodBase GetTargetMethod()
	{
		return typeof(BallisticsCalculator).GetMethod("Shoot", BindingFlags.Instance | BindingFlags.Public, null, new Type[1] { typeof(EftBulletClass) }, null);
	}

	[PatchPostfix]
	private static void Postfix(EftBulletClass shot)
	{
		if (shot == null) return;

		if (shot.IsShotFinished)
		{
			ProcessImpulse(shot);
		}
	}

	public static void ProcessImpulse(EftBulletClass shot)
	{
		if (shot == null || shot.HitCollider == null) return;

		Transform rootTransform = shot.HitCollider.transform.root;
		if (rootTransform == null) return;

		VisceralCombat.Ragdolls.Classes.RootMotion.Dynamics.PuppetMaster puppet = rootTransform.GetComponentInChildren<VisceralCombat.Ragdolls.Classes.RootMotion.Dynamics.PuppetMaster>();
		if (puppet != null && puppet.state == VisceralCombat.Ragdolls.Classes.RootMotion.Dynamics.PuppetMaster.State.Dead)
		{
			Rigidbody body = shot.HitCollider.GetComponent<Rigidbody>();
			if (body != null && shot.Ammo != null)
			{
				string caliber = (shot.Ammo is AmmoItemClass ammoItem) ? ammoItem.Caliber : string.Empty;
				float baseForce = _dictionary.TryGetValue(caliber, out float val) ? val : 25f;
				float boneMult = _bonedictionary.TryGetValue(shot.HitCollider.name, out float mult) ? mult : 1f;

				string hitName = shot.HitCollider.name.ToLower();
				float bodyPartMult = 1.0f;
				if (VisceralEntry.Instance != null)
				{
					if (hitName.Contains("head")) bodyPartMult = VisceralEntry.Instance.headForceIntensity.Value;
					else if (hitName.Contains("pelvis") || hitName.Contains("spine") || hitName.Contains("rib")) bodyPartMult = VisceralEntry.Instance.TorsoForceIntensity.Value;
					else if (hitName.Contains("arm")) bodyPartMult = VisceralEntry.Instance.ArmsForceIntensity.Value;
					else if (hitName.Contains("thigh") || hitName.Contains("calf") || hitName.Contains("foot")) bodyPartMult = VisceralEntry.Instance.LegsForceIntensity.Value;
				}

				float totalIntensity = (VisceralEntry.Instance != null) ? VisceralEntry.Instance.ShotIntensity.Value : 1f;
				Vector3 impulse = shot.Direction * (baseForce * boneMult * bodyPartMult * totalIntensity);
				body.AddForceAtPosition(impulse, shot.HitPoint, ForceMode.Impulse);
			}
		}
	}
}
