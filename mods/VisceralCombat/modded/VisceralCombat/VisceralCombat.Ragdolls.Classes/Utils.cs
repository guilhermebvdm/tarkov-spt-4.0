using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using EFT;
using UnityEngine;
using Object = UnityEngine.Object;
using VisceralCombat.Ragdolls.Classes.RootMotion.Dynamics;

namespace VisceralCombat.Ragdolls.Classes;

public class Utils
{
	internal static IEnumerable<Transform> EnumerateHierarchyCore(Transform root)
	{
		if (root == null) yield break;
		Queue<Transform> queue = new Queue<Transform>();
		queue.Enqueue(root);
		while (queue.Count > 0)
		{
			Transform current = queue.Dequeue();
			if (current == null) continue;
			for (int i = 0; i < current.childCount; i++)
			{
				queue.Enqueue(current.GetChild(i));
			}
			yield return current;
		}
	}

	internal static void SetAnimation(AnimatorOverrideController overrideController, string clipName, AnimationClip clip)
	{
		typeof(AnimatorOverrideController).GetMethod("Internal_SetClipByName", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(overrideController, new object[2] { clipName, clip });
	}

	internal static IEnumerator LerpLayerWeight(Player __instance, int layer, float startValue, float endValue, float duration)
	{
		QuickLogger.Log(ELogType.Log, $"[SPY-AGONY] LerpLayerWeight START: Player='{__instance?.Profile?.Nickname}', layer={layer}, start={startValue} -> end={endValue}, duration={duration}");
		float timeElapsed = 0f;
		while (timeElapsed < duration)
		{
			if (__instance != null && __instance.BodyAnimatorCommon != null)
			{
				float currentWeight = Mathf.Lerp(startValue, endValue, timeElapsed / duration);
				__instance.BodyAnimatorCommon.SetLayerWeight(layer, currentWeight);
			}
			timeElapsed += Time.deltaTime;
			yield return null;
		}
		if (__instance != null && __instance.BodyAnimatorCommon != null)
		{
			__instance.BodyAnimatorCommon.SetLayerWeight(layer, endValue);
			QuickLogger.Log(ELogType.Log, $"[SPY-AGONY] LerpLayerWeight FINISHED: Player='{__instance?.Profile?.Nickname}', layer={layer}, finalWeight={endValue}");
		}
	}

	internal static void SetupPuppetMaster(Player p)
	{
		if ((Object)(object)p == (Object)null || (Object)(object)((Component)p).gameObject == (Object)null)
		{
			QuickLogger.Log(ELogType.Warn, "SetupPuppetMaster called with null Player or Player GameObject!");
			return;
		}
		if (p.GetComponentInChildren<PuppetMaster>() != null)
		{
			return; // Already setup!
		}
		if ((Object)(object)VisceralEntry.Instance?.effectContainer?.activeRagdollBase == (Object)null)
		{
			Profile profile = p.Profile;
			QuickLogger.Log(ELogType.Warn, "[SPY] Ragdoll Base Isn't Loaded in Effects yet! Skipping Player! " + ((profile != null) ? profile.Nickname : null));
			return;
		}
		try
		{
			GameObject val = Object.Instantiate<GameObject>(VisceralEntry.Instance.effectContainer.activeRagdollBase);
			val.SetActive(true);
			PuppetMaster puppetMaster = PuppetMaster.SetUp(((Component)p).gameObject.transform, val.transform, LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("TransparentFX"));
			if ((Object)(object)puppetMaster == (Object)null)
			{
				Profile profile2 = p.Profile;
				QuickLogger.Log(ELogType.Warn, "PuppetMaster setup failed for Player: " + ((profile2 != null) ? profile2.Nickname : null));
				return;
			}
			Transform transform = ((Component)puppetMaster).transform;
			if ((Object)(object)((transform != null) ? transform.parent : null) != (Object)null)
			{
				((Component)puppetMaster).transform.parent.parent = ((Component)p).gameObject.transform;
			}
			puppetMaster.mappingWeight = 0f;
			((Behaviour)puppetMaster).enabled = false;
			Profile profile3 = p.Profile;
			QuickLogger.Log(ELogType.Log, "PuppetMaster setup complete for Player: " + ((profile3 != null) ? profile3.Nickname : null));
		}
		catch (Exception arg)
		{
			Profile profile4 = p.Profile;
			QuickLogger.Log(ELogType.Error, $"Exception in SetupPuppetMaster for Player: {((profile4 != null) ? profile4.Nickname : null)}, {arg}");
		}
	}
}
