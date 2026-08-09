using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Comfort.Common;
using EFT;
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
		TarkovApplication obj = (TarkovApplication)Singleton<ClientApplication<ISession>>.Instance;
		RaidSettings val = (RaidSettings)typeof(TarkovApplication).GetField("_raidSettings", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(obj);
		IEnumerable<GameObject> enumerable = from go in Object.FindObjectsOfType<GameObject>()
			where go.layer == LayerMask.NameToLayer("Grass")
			select go;
		IEnumerable<GameObject> enumerable2 = from go in Object.FindObjectsOfType<GameObject>()
			where go.layer == LayerMask.NameToLayer("Foliage")
			select go;
		GameObject val2 = GameObject.Find("TerrainsAI");
		LayerMasksDataAbstractClass.HitMask = VisceralEntry.Instance.LayerMaskConstructor(VisceralEntry.WorldLayers.Concat(VisceralEntry.DeadBodyLayers.Concat(VisceralEntry.HitColliderLayers)).ToArray());
		
		SetupNewLayer(LayerMask.NameToLayer("TransparentFX"));
		
		VisceralEntry.Instance.dismemberedPlayers.Clear();
		VisceralEntry.Instance.deadPlayers.Clear();
		GoreObjectPool.Instance?.ClearPool();
		QuickLogger.Log(ELogType.Log, "GameStartedPatch: Cleaned deadPlayers, dismemberedPlayers, and GoreObjectPool for new raid.");

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
