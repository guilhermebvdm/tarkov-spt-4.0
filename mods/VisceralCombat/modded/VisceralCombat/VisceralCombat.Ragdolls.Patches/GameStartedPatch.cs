using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Comfort.Common;
using EFT;
using Fika.Core.Main.Utils;
using Fika.Core.Networking;
using Nexus.BundleLoader;
using SPT.Reflection.Patching;
using UnityEngine;
using Object = UnityEngine.Object;
using VisceralCombat.Dismemberment.Classes;

namespace VisceralCombat.Ragdolls.Patches;

public class GameStartedPatch : ModulePatch
{
	protected override MethodBase GetTargetMethod()
	{
		return typeof(GameWorld).GetMethod("OnGameStarted");
	}

	[PatchPostfix]
	private static void Postfix(GameWorld __instance)
	{
		LayerMasksDataAbstractClass.HitMask = VisceralEntry.Instance.LayerMaskConstructor(VisceralEntry.WorldLayers.Concat(VisceralEntry.DeadBodyLayers.Concat(VisceralEntry.HitColliderLayers)).ToArray());
		
		SetupNewLayer(LayerMask.NameToLayer("TransparentFX"));
		
		VisceralEntry.Instance.dismemberedPlayers.Clear();
		VisceralEntry.Instance.deadPlayers.Clear();
		VisceralCombat.Ragdolls.Patches.LimbKillPatch.ClearLivingVolleys();
		VisceralCombat.Ragdolls.Classes.RagdollHelperClass.ClearAgonyTimers();
		VisceralCombat.Combined.Classes.VisceralShotProcessor.ClearShots();
		VisceralCombat.Combat.Patches.ShellCasingPatch.ClearCasings();
		GoreObjectPool.Instance?.ClearPool();
		QuickLogger.Log(ELogType.Log, "GameStartedPatch: Cleaned deadPlayers, dismemberedPlayers, GoreObjectPool, living volleys, agony timers, and casings for new raid.");

		// Only the host (FikaServer) initiates the handshake; solo SPT also triggers immediately.
		// Clients skip — they respond to the host's ping via VisceralEntry registered packets.
		if (!FikaBackendUtils.IsClient)
		{
			VisceralEntry.Instance.StartVisceralHandshake();
		}

		if (VisceralEntry.Instance.BodyCollision.Value)
		{
			Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Deadbody"), LayerMask.NameToLayer("HitCollider"), false);
			Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Deadbody"), LayerMask.NameToLayer("Player"), false);
			if (VisceralEntry.Instance.UseActiveRagdolls.Value)
			{
				Physics.IgnoreLayerCollision(LayerMask.NameToLayer("TransparentFX"), LayerMask.NameToLayer("HitCollider"), false);
				Physics.IgnoreLayerCollision(LayerMask.NameToLayer("TransparentFX"), LayerMask.NameToLayer("Player"), false);
			}
		}
		else
		{
			Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Deadbody"), LayerMask.NameToLayer("HitCollider"), true);
			Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Deadbody"), LayerMask.NameToLayer("Player"), true);
			if (VisceralEntry.Instance.UseActiveRagdolls.Value)
			{
				Physics.IgnoreLayerCollision(LayerMask.NameToLayer("TransparentFX"), LayerMask.NameToLayer("HitCollider"), true);
				Physics.IgnoreLayerCollision(LayerMask.NameToLayer("TransparentFX"), LayerMask.NameToLayer("Player"), true);
			}
		}
	}

	internal static void SetupNewLayer(LayerMask layer)
	{
		Physics.IgnoreLayerCollision(layer, layer, false);
		Physics.IgnoreLayerCollision(layer, 3, true);
		Physics.IgnoreLayerCollision(layer, 6, true);
		Physics.IgnoreLayerCollision(layer, 7, true);
		Physics.IgnoreLayerCollision(layer, LayerMask.NameToLayer("DoorLowPolyCollider"), true);
		Physics.IgnoreLayerCollision(layer, LayerMask.NameToLayer("LowPolyCollider"), true);
		Physics.IgnoreLayerCollision(layer, LayerMask.NameToLayer("Terrain"), false);
		Physics.IgnoreLayerCollision(layer, LayerMask.NameToLayer("HighPolyCollider"), false);
		Physics.IgnoreLayerCollision(layer, LayerMask.NameToLayer("Shells"), false);
		Physics.IgnoreLayerCollision(layer, LayerMask.NameToLayer("TransparentCollider"), false);
	}
}
