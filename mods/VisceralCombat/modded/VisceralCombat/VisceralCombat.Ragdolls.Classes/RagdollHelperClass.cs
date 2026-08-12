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

	internal static Vector3 limbSize = new Vector3(0.1f, 0.1f, 0.1f);

	/// <summary>
	/// Zeroes the muscle weight of muscles belonging to a dismembered limb so that
	/// PuppetMaster does not attempt to animate a bone scaled to 0.001f during agony,
	/// which would cause the "giant bot" physics explosion.
	/// Keywords are lower-case substrings matched against muscle.name (same pattern
	/// already used in PlayDeathAnimation case 0 and LimbKillPatch).
	/// </summary>
	internal static void DisableDismemberedMuscles(PuppetMaster pm, EBodyPart dismemberedPart)
	{
		if (pm?.muscles == null) return;

		string[] muscleKeywords = dismemberedPart switch
		{
			(EBodyPart)3 => new[] { "humanlupperarm", "humanlforearm", "humanlpalm", "humanlhand", "humanldigit", "lupperarm", "lforearm", "lpalm", "lhand" }, // LeftArm
			(EBodyPart)4 => new[] { "humanrupperarm", "humanrforearm", "humanrpalm", "humanrhand", "humanrdigit", "rupperarm", "rforearm", "rpalm", "rhand" }, // RightArm
			(EBodyPart)5 => new[] { "humanlthigh", "humanlleg", "humanlcalf", "humanlfoot", "humanltoe", "lthigh", "lleg", "lcalf", "lfoot" },                // LeftLeg
			(EBodyPart)6 => new[] { "humanrthigh", "humanrleg", "humanrcalf", "humanrfoot", "humanrtoe", "rthigh", "rleg", "rcalf", "rfoot" },                // RightLeg
			_ => Array.Empty<string>()
		};

		if (muscleKeywords.Length == 0) return;

		foreach (Muscle muscle in pm.muscles)
		{
			if (muscle == null) continue;
			string mNameLow = muscle.name?.ToLower() ?? "";
			string tNameLow = muscle.target?.name?.ToLower() ?? "";

			foreach (string kw in muscleKeywords)
			{
				if ((mNameLow.Length > 0 && mNameLow.Contains(kw)) || (tNameLow.Length > 0 && tNameLow.Contains(kw)))
				{
					muscle.state.isDisconnected = true;
					muscle.props.muscleWeight = 0f;
					muscle.props.pinWeight    = 0f;
					muscle.props.mappingWeight = 0f;
					muscle.state.muscleWeightMlp = 0f;
					muscle.state.pinWeightMlp    = 0f;
					muscle.state.mappingWeightMlp = 0f;
					QuickLogger.Log(ELogType.Log, $"DisableDismemberedMuscles: zeroed muscle '{muscle.name}' (target: {muscle.target?.name}) for {dismemberedPart}");
					break;
				}
			}
		}
	}

	internal static void PlayDeathAnimation(Player p, PuppetMaster pm, EBodyPart eBodyPart)
	{
		VisceralCombat.Combined.Classes.DeathAudioController.HandleDeathAudio(p, eBodyPart);

		if ((int)eBodyPart > 0)
		{
			if (VisceralEntry.Instance != null && VisceralEntry.Instance.dismemberedPlayers.Contains(p))
			{
				if (p.BodyAnimatorCommon != null)
				{
					p.BodyAnimatorCommon.SetLayerWeight(18, 1f);
				}
			}
			else
			{
				((MonoBehaviour)p).StartCoroutine(Utils.LerpLayerWeight(p, 18, 0f, 1f, VisceralEntry.Instance.AnimSwapDuration.Value));
			}
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
		if (p != null)
		{
			DismemberedLimbScaler[] scalers = p.GetComponentsInChildren<DismemberedLimbScaler>(true);
			foreach (DismemberedLimbScaler scaler in scalers)
			{
				if (scaler != null) scaler.transform.localScale = limbSize;
			}
		}

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
			if (pm == null || ((Component)pm).gameObject == null || !((Component)pm).gameObject.activeInHierarchy) yield break;
			pm.mappingWeight = Mathf.Lerp(startValue, endValue, elapsedTime / duration);
			elapsedTime += Time.deltaTime;
			yield return null;
		}
		if (pm != null && ((Component)pm).gameObject != null && ((Component)pm).gameObject.activeInHierarchy)
		{
			pm.mappingWeight = endValue;
		}
	}

	/// <summary>
	/// Immediately interrupts an active agony animation when a bot is shot.
	/// Disables the animator immediately (avoiding T-pose / idle pose reset),
	/// zeroes muscle/pin weights, sets rigidbodies to physical ragdoll, and deactivates PuppetMaster.
	/// </summary>
	internal static void InterruptAgony(Player p, PuppetMaster pm)
	{
		if (p == null || pm == null) return;
		if (VisceralEntry.Instance != null) VisceralEntry.Instance.dismemberedPlayers.Remove(p);

		// 1. Instantly drop all animation pin and muscle spring stiffness to 0 so the body collapses under gravity
		pm.stateSettings.killDuration = 0f;
		pm.pinWeight = 0f;
		pm.muscleWeight = 0f;
		pm.muscleSpring = 0f;
		pm.mappingWeight = 1f; // KEEP mappingWeight = 1 so PuppetMaster continuously maps ragdoll physics onto PlayerBody
		pm.state = PuppetMaster.State.Dead;

		// 2. Disable animator component completely so Layer 0 never evaluates scale 1.0f keyframes on bones
		if (p.BodyAnimatorCommon != null)
		{
			p.BodyAnimatorCommon.enabled = false;
		}

		DismemberedLimbScaler[] scalers = p.GetComponentsInChildren<DismemberedLimbScaler>(true);
		foreach (DismemberedLimbScaler scaler in scalers)
		{
			if (scaler != null) scaler.transform.localScale = limbSize;
		}

		// 3. Release non-dismembered rigidbodies to physical ragdoll while disconnecting dismembered muscles
		Muscle[] muscles = pm.muscles;
		if (muscles != null)
		{
			foreach (Muscle m in muscles)
			{
				if (m == null) continue;

				bool isDismembered = (m.rigidbody != null && ParentIsDismembered(m.rigidbody.transform))
				                     || (m.target != null && ParentIsDismembered(m.target))
				                     || (m.joint != null && ParentIsDismembered(m.joint.transform));

				if (isDismembered)
				{
					m.state.isDisconnected = true;
					m.props.muscleWeight = 0f;
					m.props.pinWeight = 0f;
					m.props.mappingWeight = 0f;
					m.state.muscleWeightMlp = 0f;
					m.state.pinWeightMlp = 0f;
					m.state.mappingWeightMlp = 0f;
					if (m.rigidbody != null)
					{
						m.rigidbody.isKinematic = true;
						m.rigidbody.detectCollisions = false;
					}
				}
				else
				{
					if (m.rigidbody != null)
					{
						m.rigidbody.isKinematic = false;
						m.rigidbody.detectCollisions = true;
					}
				}
			}
		}

		GClass855.WaitSeconds((MonoBehaviour)(object)StaticManager.Instance, 3f, (Action)delegate
		{
			if ((UnityEngine.Object)pm != null && pm.gameObject != null && Singleton<GameWorld>.Instantiated)
			{
				pm.gameObject.SetActive(false);
			}
		});
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

	internal static bool ParentIsDismembered(Transform t)
	{
		Transform curr = t;
		while (curr != null)
		{
			if (curr.localScale == limbSize) return true;
			if (curr.GetComponent<DismemberedLimbScaler>() != null) return true;
			curr = curr.parent;
		}
		return false;
	}

	internal static void DisableLiveActiveRagdoll(Player p, PuppetMaster pm)
	{
		if (VisceralEntry.Instance != null && p != null) VisceralEntry.Instance.dismemberedPlayers.Remove(p);

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
							if (ParentIsDismembered(rb.transform))
							{
								rb.isKinematic = true;
								rb.detectCollisions = false;
								continue;
							}
							rb.isKinematic = true;
							GClass855.WaitSeconds((MonoBehaviour)(object)StaticManager.Instance, 0.06f, (Action)delegate
							{
								if (rb != null && Singleton<GameWorld>.Instantiated && !ParentIsDismembered(rb.transform))
								{
									rb.isKinematic = false;
								}
							});
						}
					}
					((Component)pm).gameObject.SetActive(false);
				}
			});
		}
	}

	/// <summary>
	/// Configures a blood particle system to display realistic dark coagulated blood
	/// and removes white glow / emission overdraw.
	/// Scope: only the particle prefab subtree (ps.gameObject), never the character root.
	/// Two shader paths handled:
	///   - "Particles/VD 3D Blood Shader V14" (custom mod shader): _TintColor + _Color
	///   - "Legacy Shaders/Particles/Alpha Blended Premultiply" (Unity built-in): low-alpha _Color kills white premultiply glow
	/// </summary>
	public static void ApplyDarkCoagulatedBloodFx(ParticleSystem ps)
	{
		if (ps == null) return;

		float r = VisceralEntry.Instance != null && VisceralEntry.Instance.BloodColorR != null ? VisceralEntry.Instance.BloodColorR.Value : 0.22f;
		float g = VisceralEntry.Instance != null && VisceralEntry.Instance.BloodColorG != null ? VisceralEntry.Instance.BloodColorG.Value : 0.02f;
		float b = VisceralEntry.Instance != null && VisceralEntry.Instance.BloodColorB != null ? VisceralEntry.Instance.BloodColorB.Value : 0.02f;
		float a = VisceralEntry.Instance != null && VisceralEntry.Instance.BloodColorA != null ? VisceralEntry.Instance.BloodColorA.Value : 0.95f;
		float legacyA = VisceralEntry.Instance != null && VisceralEntry.Instance.BloodLegacyAlpha != null ? VisceralEntry.Instance.BloodLegacyAlpha.Value : 0.35f;
		float emission = VisceralEntry.Instance != null && VisceralEntry.Instance.BloodEmission != null ? VisceralEntry.Instance.BloodEmission.Value : 0.0f;
		float smoothness = VisceralEntry.Instance != null && VisceralEntry.Instance.BloodSmoothness != null ? VisceralEntry.Instance.BloodSmoothness.Value : 0.0f;
		float specR = VisceralEntry.Instance != null && VisceralEntry.Instance.BloodSpecularR != null ? VisceralEntry.Instance.BloodSpecularR.Value : 0.0f;
		float specG = VisceralEntry.Instance != null && VisceralEntry.Instance.BloodSpecularG != null ? VisceralEntry.Instance.BloodSpecularG.Value : 0.0f;
		float specB = VisceralEntry.Instance != null && VisceralEntry.Instance.BloodSpecularB != null ? VisceralEntry.Instance.BloodSpecularB.Value : 0.0f;

		Color darkBlood = new Color(r, g, b, a);
		Color darkBloodPremultiply = new Color(r, g, b, legacyA);
		Color emissionColor = new Color(emission, emission, emission, 1f);
		Color specColor = new Color(specR, specG, specB, 1f);

		var main = ps.main;
		main.startColor = new ParticleSystem.MinMaxGradient(darkBlood);

		var lights = ps.lights;
		lights.enabled = false;

		Renderer[] renderers = ps.gameObject.GetComponentsInChildren<Renderer>(true);
		foreach (Renderer rend in renderers)
		{
			if (rend == null || rend.material == null) continue;
			Material mat = rend.material;
			string shaderName = mat.shader != null ? mat.shader.name : string.Empty;

			if (mat.HasProperty("_EmissionColor"))
			{
				mat.SetColor("_EmissionColor", emissionColor);
			}
			if (emission > 0.001f)
			{
				mat.EnableKeyword("_EMISSION");
			}
			else
			{
				mat.DisableKeyword("_EMISSION");
			}

			if (shaderName.Contains("Premultiply") || shaderName.Contains("Alpha Blended"))
			{
				if (mat.HasProperty("_Color")) mat.SetColor("_Color", darkBloodPremultiply);
			}
			else
			{
				if (mat.HasProperty("_Color")) mat.SetColor("_Color", darkBlood);
				if (mat.HasProperty("_TintColor")) mat.SetColor("_TintColor", darkBlood);
				if (mat.HasProperty("_SpecColor")) mat.SetColor("_SpecColor", specColor);
				if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", smoothness);
				if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
			}
		}
	}

	public static void UpdateAllActiveBloodFxInScene()
	{
		ParticleSystem[] allPs = UnityEngine.Object.FindObjectsOfType<ParticleSystem>();
		if (allPs == null) return;
		foreach (ParticleSystem ps in allPs)
		{
			if (ps != null && ps.gameObject != null && (ps.gameObject.name.Contains("blood") || ps.gameObject.name.Contains("Blood") || ps.gameObject.name.Contains("squirt") || ps.gameObject.name.Contains("Bleed") || ps.gameObject.name.Contains("gore")))
			{
				ApplyDarkCoagulatedBloodFx(ps);
			}
		}
	}
}

/// <summary>
/// Enforces RagdollHelperClass.limbSize in Update(), OnAnimatorMove(), and LateUpdate()
/// on dismembered bone transforms.
/// OnAnimatorMove() runs IMMEDIATELY after Unity's Animator updates bone transforms in internal animation pass,
/// preventing the Animator from displaying or overriding the bone scale back to 1.0 during agony.
/// </summary>
public class DismemberedLimbScaler : MonoBehaviour
{
	private void Update()
	{
		transform.localScale = RagdollHelperClass.limbSize;
	}

	private void OnAnimatorMove()
	{
		transform.localScale = RagdollHelperClass.limbSize;
	}

	private void LateUpdate()
	{
		transform.localScale = RagdollHelperClass.limbSize;
	}
}
