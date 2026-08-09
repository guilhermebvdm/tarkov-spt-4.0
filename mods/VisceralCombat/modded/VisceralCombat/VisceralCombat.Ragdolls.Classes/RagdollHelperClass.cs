using System;
using System.Collections;
using System.Collections.Generic;
using Comfort.Common;
using EFT;
using Nexus.BundleLoader;
using UnityEngine;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;
using VisceralCombat.Ragdolls.Classes.RootMotion.Dynamics;

namespace VisceralCombat.Ragdolls.Classes;

public static class RagdollHelperClass
{
	internal static Dictionary<string, float> limb_chances = new Dictionary<string, float>();

	internal static List<Transform> limbsToCheck = new List<Transform>();

	private static float Anim_Neck1_Length = 9f;

	private static float Anim_Neck2_Length = 17f;

	private static float Anim_Thigh_Length = 13f;

	private static float Anim_Stomach1_Length = 13f;

	private static float Anim_Stomach2_Length = 7f;

	internal static Vector3 limbSize = new Vector3(0.001f, 0.001f, 0.001f);

	internal static void PlayDeathAnimation(Player p, PuppetMaster pm, EBodyPart eBodyPart)
	{
		VisceralCombat.Combined.Classes.DeathAudioController.HandleDeathAudio(p, eBodyPart);

		if ((int)eBodyPart > 0)
		{
			((MonoBehaviour)p).StartCoroutine(Utils.LerpLayerWeight(p, 18, 0f, 1f, VisceralEntry.Instance.AnimSwapDuration.Value));
		}
		else
		{
			if (p.BodyAnimatorCommon != null)
			{
				p.BodyAnimatorCommon.enabled = false;
			}
		}
		RuntimeAnimatorController runtimeAnimatorController = p.BodyAnimatorCommon.runtimeAnimatorController;
		AnimatorOverrideController overrideController = (AnimatorOverrideController)(object)((runtimeAnimatorController is AnimatorOverrideController) ? runtimeAnimatorController : null);
		AssetBundle assetBundle = BundleLoaderPlugin.Instance.GetAssetBundle("death_animations");
		switch ((int)eBodyPart)
		{
		case 0:
		{
			int num5 = Random.Range(0, 10);
			if (num5 > 8)
			{
				pm.muscleWeight = 0.55f;
				Muscle[] muscles = pm.muscles;
				foreach (Muscle muscle in muscles)
				{
					if (muscle.name.Contains("arm"))
					{
						muscle.props.muscleWeight = 0.002f;
					}
				}
				break;
			}
			int num6 = Random.Range(0, 10);
			if (num6 >= Random.Range(0, 10))
			{
				Object obj9 = assetBundle.LoadAsset("Death_Neck");
				Utils.SetAnimation(overrideController, "cultist_pray", (AnimationClip)(object)((obj9 is AnimationClip) ? obj9 : null));
				p.BodyAnimatorCommon.Play("cultist_pray", 18, 0f);
				pm.stateSettings.killDuration = Random.Range(Anim_Neck1_Length / 2f, Anim_Neck1_Length - 1f);
				pm.pinWeight = 0.3f;
			}
			else
			{
				Object obj10 = assetBundle.LoadAsset("Death_Neck2");
				Utils.SetAnimation(overrideController, "cultist_pray", (AnimationClip)(object)((obj10 is AnimationClip) ? obj10 : null));
				p.BodyAnimatorCommon.Play("cultist_pray", 18, 0f);
				pm.stateSettings.killDuration = Random.Range(Anim_Neck2_Length / 2f, Anim_Neck2_Length - 1f);
				pm.pinWeight = 0.3f;
			}
			pm.muscleWeight = 0.55f;
			break;
		}
		case 1:
		{
			int num2 = Random.Range(0, 10);
			if (num2 > 7)
			{
				int num3 = Random.Range(0, 10);
				if (num3 >= Random.Range(0, 10))
				{
					Object obj5 = assetBundle.LoadAsset("Death_Neck");
					Utils.SetAnimation(overrideController, "cultist_pray", (AnimationClip)(object)((obj5 is AnimationClip) ? obj5 : null));
					p.BodyAnimatorCommon.Play("cultist_pray", 18, 0f);
					pm.stateSettings.killDuration = Random.Range(Anim_Neck1_Length / 2f, Anim_Neck1_Length - 1f);
					pm.pinWeight = 0.3f;
				}
				else
				{
					Object obj6 = assetBundle.LoadAsset("Death_Neck2");
					Utils.SetAnimation(overrideController, "cultist_pray", (AnimationClip)(object)((obj6 is AnimationClip) ? obj6 : null));
					p.BodyAnimatorCommon.Play("cultist_pray", 18, 0f);
					pm.stateSettings.killDuration = Random.Range(Anim_Neck2_Length / 2f, Anim_Neck2_Length - 1f);
					pm.pinWeight = 0.3f;
				}
			}
			else
			{
				int num4 = Random.Range(0, 10);
				if (num4 >= Random.Range(0, 10))
				{
					Object obj7 = assetBundle.LoadAsset("Flail_Loop");
					Utils.SetAnimation(overrideController, "cultist_pray", (AnimationClip)(object)((obj7 is AnimationClip) ? obj7 : null));
					p.BodyAnimatorCommon.Play("cultist_pray", 18, 0f);
					pm.stateSettings.killDuration = Random.Range(5f, 10f);
				}
				else
				{
					Object obj8 = assetBundle.LoadAsset("Death_Stomach1");
					Utils.SetAnimation(overrideController, "cultist_pray", (AnimationClip)(object)((obj8 is AnimationClip) ? obj8 : null));
					p.BodyAnimatorCommon.Play("cultist_pray", 18, 0f);
					pm.stateSettings.killDuration = Random.Range(5f, 10f);
				}
			}
			break;
		}
		case 5:
		{
			Object obj12 = assetBundle.LoadAsset("Death_LThigh1");
			Utils.SetAnimation(overrideController, "cultist_pray", (AnimationClip)(object)((obj12 is AnimationClip) ? obj12 : null));
			p.BodyAnimatorCommon.Play("cultist_pray", 18, 0f);
			pm.stateSettings.killDuration = Random.Range(Anim_Thigh_Length / 2f, Anim_Thigh_Length - 1f);
			break;
		}
		case 6:
		{
			Object obj11 = assetBundle.LoadAsset("Death_RThigh1");
			Utils.SetAnimation(overrideController, "cultist_pray", (AnimationClip)(object)((obj11 is AnimationClip) ? obj11 : null));
			p.BodyAnimatorCommon.Play("cultist_pray", 18, 0f);
			pm.stateSettings.killDuration = Random.Range(Anim_Thigh_Length / 2f, Anim_Thigh_Length - 1f);
			break;
		}
		case 2:
		{
			int num = Random.Range(0, 10);
			if (num >= Random.Range(0, 10))
			{
				Object obj3 = assetBundle.LoadAsset("Death_Stomach1");
				Utils.SetAnimation(overrideController, "cultist_pray", (AnimationClip)(object)((obj3 is AnimationClip) ? obj3 : null));
				p.BodyAnimatorCommon.Play("cultist_pray", 18, 0f);
				pm.stateSettings.killDuration = Random.Range(Anim_Stomach1_Length / 2f, Anim_Stomach1_Length - 1f);
			}
			else
			{
				Object obj4 = assetBundle.LoadAsset("Death_Stomach2");
				Utils.SetAnimation(overrideController, "cultist_pray", (AnimationClip)(object)((obj4 is AnimationClip) ? obj4 : null));
				p.BodyAnimatorCommon.Play("cultist_pray", 18, 0f);
				pm.stateSettings.killDuration = Random.Range(Anim_Stomach2_Length / 2f, Anim_Stomach2_Length - 1f);
			}
			break;
		}
		case 3:
		{
			Object obj2 = assetBundle.LoadAsset("Flail_Loop");
			Utils.SetAnimation(overrideController, "cultist_pray", (AnimationClip)(object)((obj2 is AnimationClip) ? obj2 : null));
			p.BodyAnimatorCommon.Play("cultist_pray", 18, 0f);
			p.BodyAnimatorCommon.speed = Random.Range(0.2f, 1f);
			pm.stateSettings.killDuration = Random.Range(2.5f, 5f);
			break;
		}
		case 4:
		{
			Object obj = assetBundle.LoadAsset("Flail_Loop");
			Utils.SetAnimation(overrideController, "cultist_pray", (AnimationClip)(object)((obj is AnimationClip) ? obj : null));
			p.BodyAnimatorCommon.Play("cultist_pray", 18, 0f);
			p.BodyAnimatorCommon.speed = Random.Range(0.2f, 1f);
			pm.stateSettings.killDuration = Random.Range(2.5f, 5f);
			break;
		}
		default:
			break;
		}

		float totalDuration = Mathf.Max(3f, pm.stateSettings.killDuration + 1f);
		GClass855.WaitSeconds((MonoBehaviour)(object)StaticManager.Instance, totalDuration, (Action)delegate
		{
			if (Singleton<GameWorld>.Instantiated)
			{
				DisableLiveActiveRagdoll(p, pm);
			}
		});
	}

	internal static IEnumerator LerpMappingWeight(PuppetMaster pm, float startValue, float endValue, float duration)
	{
		float elapsedTime = 0f;
		while (elapsedTime < duration)
		{
			if (pm == null) yield break;
			pm.mappingWeight = Mathf.Lerp(startValue, endValue, elapsedTime / duration);
			elapsedTime += Time.deltaTime;
			yield return null;
		}
		if (pm != null)
		{
			pm.mappingWeight = endValue;
		}
	}

	internal static bool ShouldRagdoll(EBodyPart bodyPartType)
	{
		float num = Random.Range(0f, 1f);
		if ((int)bodyPartType switch
		{
			0 => limb_chances.TryGetValue("Head", out var value) ? value : 0f, 
			1 => limb_chances.TryGetValue("Chest", out var value2) ? value2 : 0f, 
			2 => limb_chances.TryGetValue("Stomach", out var value3) ? value3 : 0f, 
			5 => limb_chances.TryGetValue("Thighs", out var value4) ? value4 : 0f, 
			3 => limb_chances.TryGetValue("Arms", out var value5) ? value5 : 0f, 
			6 => limb_chances.TryGetValue("Thighs", out var value6) ? value6 : 0f, 
			4 => limb_chances.TryGetValue("Arms", out var value7) ? value7 : 0f, 
			_ => 0f, 
		} > num)
		{
			return true;
		}
		return false;
	}

	internal static void DisableLiveActiveRagdoll(Player p, PuppetMaster pm)
	{
		if (p != null && p.BodyAnimatorCommon != null)
		{
			((MonoBehaviour)p).StartCoroutine(Utils.LerpLayerWeight(p, 18, 1f, 0f, 1f));
		}

		if (pm != null && ((Component)pm).gameObject != null)
		{
			((MonoBehaviour)p).StartCoroutine(LerpMappingWeight(pm, pm.mappingWeight, 0f, 1f));

			GClass855.WaitSeconds((MonoBehaviour)(object)StaticManager.Instance, 1.2f, (Action)delegate
			{
				if (pm != null && ((Component)pm).gameObject != null && Singleton<GameWorld>.Instantiated)
				{
					if (p?.PlayerBody != null && ((Component)p.PlayerBody).gameObject != null)
					{
						Rigidbody[] componentsInChildren = ((Component)p.PlayerBody).gameObject.GetComponentsInChildren<Rigidbody>();
						foreach (Rigidbody rb in componentsInChildren)
						{
							if (rb == null) continue;
							rb.isKinematic = true;
							GClass855.WaitSeconds((MonoBehaviour)(object)StaticManager.Instance, 0.06f, (Action)delegate
							{
								if (rb != null && Singleton<GameWorld>.Instantiated)
								{
									rb.isKinematic = false;
								}
							});
						}
					}
					((Behaviour)pm).enabled = false;
					if (p != null && p.BodyAnimatorCommon != null)
					{
						p.BodyAnimatorCommon.enabled = false;
					}
				}
			});
		}
	}
}
