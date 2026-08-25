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

	private static readonly Dictionary<Player, float> _agonyStartTime = new Dictionary<Player, float>();

	public static void ClearAgonyTimers()
	{
		_agonyStartTime.Clear();
	}

	/// <summary>
	/// Checks if a player is currently in FIKA's Downed / Coma / Bleedout state.
	/// When downed, FIKA temporarily sets IsAlive = false, but the player can be revived.
	/// VisceralCombat must NOT apply dismemberment, bone scaling, or ragdoll death setup
	/// while the player is downed.
	/// </summary>
	public static bool IsPlayerDowned(Player player)
	{
		if (player == null) return false;

		// 1. Direct check on FikaPlayer / ObservedPlayer / ClientHealthController if Fika is loaded
		try
		{
			if (player is Fika.Core.Main.Players.FikaPlayer fikaPlayer && fikaPlayer.Downed)
			{
				return true;
			}
			if (player.HealthController is Fika.Core.Main.ClientClasses.ClientHealthController clientHC && clientHC.Downed)
			{
				return true;
			}
		}
		catch { }

		// 2. Reflection fallback for any player or health controller with "Downed" property
		try
		{
			System.Reflection.PropertyInfo downedProp = player.GetType().GetProperty("Downed", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
			if (downedProp != null && downedProp.PropertyType == typeof(bool))
			{
				if ((bool)downedProp.GetValue(player)) return true;
			}

			if (player.HealthController != null)
			{
				System.Reflection.PropertyInfo hcDownedProp = player.HealthController.GetType().GetProperty("Downed", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
				if (hcDownedProp != null && hcDownedProp.PropertyType == typeof(bool))
				{
					if ((bool)hcDownedProp.GetValue(player.HealthController)) return true;
				}
			}
		}
		catch { }

		return false;
	}

	/// <summary>
	/// Finds any Player (human player or AI bot) in the active raid matching the given netId (player.Id).
	/// Valid across both Host and Client in FIKA coop sessions.
	/// </summary>
	public static Player FindPlayerByNetId(int netId)
	{
		if (!Singleton<GameWorld>.Instantiated || Singleton<GameWorld>.Instance == null) return null;

		var allPlayers = Singleton<GameWorld>.Instance.AllPlayersEverExisted;
		if (allPlayers != null)
		{
			foreach (var iPlayer in allPlayers)
			{
				if (iPlayer is Player p && p.Id == netId)
				{
					return p;
				}
			}
		}
		return null;
	}

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

		if (p != null)
		{
			_agonyStartTime[p] = Time.time;
		}

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
		bool isProne = p != null && (p.IsInPronePose || p.PoseLevel <= 0.1f);

		if (isProne)
		{
			// Prone bots: never play standing-up animations (which would teleport/snap the bot upright).
			// Either play ground flailing (Flail_Loop) or let PuppetMaster collapse directly into natural ragdoll.
			if (Random.value > 0.35f)
			{
				Object flailAsset = assetBundle.LoadAsset("Flail_Loop");
				if (flailAsset != null)
				{
					Utils.SetAnimation(overrideController, "cultist_pray", (AnimationClip)(object)((flailAsset is AnimationClip) ? flailAsset : null));
					p.BodyAnimatorCommon.Play("cultist_pray", 18, 0f);
					p.BodyAnimatorCommon.speed = Random.Range(0.4f, 0.9f);
					pm.stateSettings.killDuration = Random.Range(3.0f, 6.0f);
					pm.pinWeight = 0.02f;
					pm.muscleWeight = 0.5f;
				}
			}
			else
			{
				// Pure ragdoll collapse on the ground
				pm.stateSettings.killDuration = 0f;
				pm.pinWeight = 0f;
				pm.muscleWeight = 0f;
				if (p.BodyAnimatorCommon != null)
				{
					p.BodyAnimatorCommon.enabled = false;
				}
			}
		}
		else
		{
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
					pm.pinWeight = 0.02f;
				}
				else
				{
					Object obj10 = assetBundle.LoadAsset("Death_Neck2");
					Utils.SetAnimation(overrideController, "cultist_pray", (AnimationClip)(object)((obj10 is AnimationClip) ? obj10 : null));
					p.BodyAnimatorCommon.Play("cultist_pray", 18, 0f);
					pm.stateSettings.killDuration = Random.Range(Anim_Neck2_Length / 2f, Anim_Neck2_Length - 1f);
					pm.pinWeight = 0.02f;
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
						pm.pinWeight = 0.02f;
					}
					else
					{
						Object obj6 = assetBundle.LoadAsset("Death_Neck2");
						Utils.SetAnimation(overrideController, "cultist_pray", (AnimationClip)(object)((obj6 is AnimationClip) ? obj6 : null));
						p.BodyAnimatorCommon.Play("cultist_pray", 18, 0f);
						pm.stateSettings.killDuration = Random.Range(Anim_Neck2_Length / 2f, Anim_Neck2_Length - 1f);
						pm.pinWeight = 0.02f;
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
	internal static void InterruptAgony(Player p, PuppetMaster pm, bool forceInstant = false)
	{
		if (p == null || pm == null) return;
		if (!forceInstant && _agonyStartTime.TryGetValue(p, out float startTime))
		{
			// 1.2s grace period: ignore burst-fire bullets immediately following the fatal hit
			if (Time.time < startTime + 1.2f)
			{
				return;
			}
		}
		_agonyStartTime.Remove(p);

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
		// Restored to original mod behavior: materials and shaders are preserved untouched
	}

	public static void ApplyBloodCloudSettings()
	{
		if (VisceralEntry.Instance == null || !Comfort.Common.Singleton<Systems.Effects.Effects>.Instantiated) return;

		bool enabled = VisceralEntry.Instance.EnableImpactBloodCloud != null ? VisceralEntry.Instance.EnableImpactBloodCloud.Value : true;
		int particleCount = VisceralEntry.Instance.ImpactBloodCloudParticleCount != null ? VisceralEntry.Instance.ImpactBloodCloudParticleCount.Value : 10;
		float scaleMult = VisceralEntry.Instance.ImpactBloodCloudScale != null ? VisceralEntry.Instance.ImpactBloodCloudScale.Value : 1.0f;

		var effectsManager = Comfort.Common.Singleton<Systems.Effects.Effects>.Instance;
		if (effectsManager == null || effectsManager.EffectsArray == null) return;

		foreach (var effect in effectsManager.EffectsArray)
		{
			if (effect == null || effect.MaterialTypes == null) continue;
			if (System.Array.IndexOf(effect.MaterialTypes, EFT.Ballistics.MaterialType.Body) >= 0)
			{
				if (effect.Particles != null)
				{
					foreach (var ps in effect.Particles)
					{
						if (ps == null) continue;
						if (!enabled)
						{
							ps.MinCount = 0;
							ps.RandomCountRange = 0;
						}
						else
						{
							ps.MinCount = particleCount;
							ps.RandomCountRange = particleCount / 2;
							ps.UseRandomScale = true;
							ps.RandomScale = new Vector3(scaleMult, scaleMult, scaleMult);
						}
					}
				}
			}
		}
	}

	/// <summary>
	/// Configures a ParticleSystem's collision module to only collide with the static environment (floors, walls)
	/// and strictly ignore Player, HitCollider, Deadbody, and TransparentFX layers with zero physical impulse.
	/// Prevents micro-stuttering/stumbling when particles spray from the player's own body.
	/// </summary>
	public static void ConfigureBloodParticleCollision(ParticleSystem ps)
	{
		if (ps == null) return;
		var collision = ps.collision;
		collision.enabled = true;
		collision.type = ParticleSystemCollisionType.World;
		collision.mode = ParticleSystemCollisionMode.Collision3D;
		collision.sendCollisionMessages = true;
		collision.colliderForce = 0f;

		int playerLayer = LayerMask.NameToLayer("Player");
		int hitColliderLayer = LayerMask.NameToLayer("HitCollider");
		int deadbodyLayer = LayerMask.NameToLayer("Deadbody");
		int transparentFxLayer = LayerMask.NameToLayer("TransparentFX");

		int excludedMask = 0;
		if (playerLayer >= 0) excludedMask |= (1 << playerLayer);
		if (hitColliderLayer >= 0) excludedMask |= (1 << hitColliderLayer);
		if (deadbodyLayer >= 0) excludedMask |= (1 << deadbodyLayer);
		if (transparentFxLayer >= 0) excludedMask |= (1 << transparentFxLayer);

		collision.collidesWith = ~excludedMask;
	}

	private static readonly HashSet<Transform> _activeWakingCorpses = new HashSet<Transform>();

	/// <summary>
	/// Returns true if all active (non-kinematic) rigidbodies in the corpse have stopped moving
	/// (linear and angular velocity magnitude below threshold).
	/// </summary>
	public static bool IsCorpseAtRest(Rigidbody[] rbs, float maxLinearSpeed = 0.08f, float maxAngularSpeed = 0.08f)
	{
		if (rbs == null || rbs.Length == 0) return true;
		float maxLinSqr = maxLinearSpeed * maxLinearSpeed;
		float maxAngSqr = maxAngularSpeed * maxAngularSpeed;

		foreach (Rigidbody rb in rbs)
		{
			if (rb == null || rb.isKinematic) continue;
			if (rb.velocity.sqrMagnitude > maxLinSqr || rb.angularVelocity.sqrMagnitude > maxAngSqr)
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>
	/// Waits until the corpse has finished any agony animation and all bone rigidbodies have
	/// completely settled on the floor (velocity ~ 0) before putting rigidbodies into kinematic sleep (0% CPU).
	/// </summary>
	public static IEnumerator SleepCorpseWhenAtRest(Transform root, Rigidbody[] rbs, Player player = null, float minWait = 2.5f, float maxTimeout = 25.0f)
	{
		if (minWait > 0f)
		{
			yield return new WaitForSeconds(minWait);
		}

		float elapsed = minWait;
		PuppetMaster pm = root != null ? root.GetComponentInChildren<PuppetMaster>() : null;
		if (pm == null && player != null)
		{
			pm = ((Component)player).GetComponentInChildren<PuppetMaster>();
		}

		// 1. If an agony animation or active ragdoll is playing on this corpse, wait until it completely finishes!
		while (elapsed < maxTimeout)
		{
			bool isAgonizing = (player != null && VisceralEntry.Instance != null && VisceralEntry.Instance.dismemberedPlayers.Contains(player))
			                || (pm != null && ((Component)pm).gameObject.activeInHierarchy && pm.mappingWeight > 0.01f);

			if (!isAgonizing)
			{
				break;
			}

			yield return new WaitForSeconds(0.3f);
			elapsed += 0.3f;
		}

		// 2. Poll bone velocities until the corpse is completely stationary for 3 consecutive checks (0.6s)
		int stillChecks = 0;
		while (elapsed < maxTimeout)
		{
			yield return new WaitForSeconds(0.2f);
			elapsed += 0.2f;

			// Do not sleep if PuppetMaster was re-activated
			if (pm != null && ((Component)pm).gameObject.activeInHierarchy && pm.mappingWeight > 0.01f)
			{
				stillChecks = 0;
				continue;
			}

			if (IsCorpseAtRest(rbs))
			{
				stillChecks++;
				if (stillChecks >= 3)
				{
					break;
				}
			}
			else
			{
				stillChecks = 0;
			}
		}

		if (root != null)
		{
			_activeWakingCorpses.Remove(root);
		}

		// 3. Put rigidbodies to sleep in EFT physics
		if (rbs != null)
		{
			foreach (Rigidbody rb in rbs)
			{
				if (rb == null) continue;
				EFTPhysicsClass.GClass745.UnsupportRigidbody(rb);
				rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
				rb.isKinematic = true;
			}
		}
	}

	/// <summary>
	/// Temporarily wakes an inert corpse's rigidbodies from physics sleep upon bullet/grenade impact.
	/// Sets isKinematic = false, re-supports the rigidbodies in EFT physics simulation, and
	/// automatically returns the corpse to kinematic sleep after movement settles back to 0.
	/// </summary>
	public static Rigidbody[] WakeCorpse(Collider hitCollider, float duration = 2.0f)
	{
		if (hitCollider == null) return null;

		Player p = hitCollider.GetComponentInParent<Player>();
		if (p != null && p.HealthController != null && p.HealthController.IsAlive)
		{
			// Never dynamic-activate bone rigidbodies of living players or bots!
			return null;
		}

		Transform root = hitCollider.transform.root;
		Rigidbody[] rbs = root != null ? root.GetComponentsInChildren<Rigidbody>(true) : null;

		if ((rbs == null || rbs.Length == 0) && p?.PlayerBody != null)
		{
			rbs = ((Component)p.PlayerBody).gameObject.GetComponentsInChildren<Rigidbody>(true);
			if (root == null) root = ((Component)p.PlayerBody).transform;
		}

		if (rbs == null || rbs.Length == 0) return null;

		foreach (Rigidbody rb in rbs)
		{
			if (rb == null) continue;
			if (ParentIsDismembered(rb.transform))
			{
				rb.isKinematic = true;
				rb.detectCollisions = false;
				continue;
			}
			// Only call SupportRigidbody if the body was actually kinematic (sleeping)
			// This prevents duplicate entries in EFTPhysicsClass.List_0!
			if (rb.isKinematic)
			{
				rb.isKinematic = false;
				rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
				EFTPhysicsClass.GClass745.SupportRigidbody(rb, 0f);
			}
			rb.WakeUp();
		}

		if (root != null && _activeWakingCorpses.Add(root))
		{
			if (StaticManager.Instance != null)
			{
				((MonoBehaviour)StaticManager.Instance).StartCoroutine(SleepCorpseWhenAtRest(root, rbs, p, duration, 10.0f));
			}
		}

		return rbs;
	}

	public static void WakeCorpseTemporarily(Player player, float duration = 2.0f)
	{
		if (player == null || player.PlayerBody == null) return;
		if (player.HealthController != null && player.HealthController.IsAlive) return;

		GameObject bodyGo = ((Component)player.PlayerBody).gameObject;
		if (bodyGo == null) return;

		Transform root = ((Component)player).transform;
		Rigidbody[] rbs = bodyGo.GetComponentsInChildren<Rigidbody>(true);
		if (rbs == null || rbs.Length == 0) return;

		foreach (Rigidbody rb in rbs)
		{
			if (rb == null) continue;
			if (ParentIsDismembered(rb.transform))
			{
				rb.isKinematic = true;
				rb.detectCollisions = false;
				continue;
			}
			if (rb.isKinematic)
			{
				rb.isKinematic = false;
				rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
				EFTPhysicsClass.GClass745.SupportRigidbody(rb, 0f);
			}
			rb.WakeUp();
		}

		if (root != null && _activeWakingCorpses.Add(root))
		{
			if (StaticManager.Instance != null)
			{
				((MonoBehaviour)StaticManager.Instance).StartCoroutine(SleepCorpseWhenAtRest(root, rbs, player, duration, 10.0f));
			}
		}
	}
}

/// <summary>
/// Enforces RagdollHelperClass.limbSize in LateUpdate() on dismembered bone transforms.
/// </summary>
public class DismemberedLimbScaler : MonoBehaviour
{
	private void LateUpdate()
	{
		transform.localScale = RagdollHelperClass.limbSize;
	}
}
