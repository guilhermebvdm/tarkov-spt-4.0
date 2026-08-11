using System.Collections;
using EFT;
using EFT.HealthSystem;
using Systems.Effects;
using Comfort.Common;
using UnityEngine;
using VisceralCombat.Ragdolls.Classes;

namespace VisceralCombat.Dismemberment.Classes;

/// <summary>
/// Controls living AI bots that survive a leg dismemberment:
///   1. Forces immediate prone pose (via BotLay.IsLay = true) and prevents all get-up / crouch attempts.
///   2. Emits heavy bleeding damage continually until exanguination death.
///   3. Paints blood trails (Tarkov native deferred decals + VisceralCombat 3D blood pools) on the floor as the bot crawls.
///   4. Plays agony audio phrases periodically.
///   5. Strictly gated by VisceralEntry.AllPlayersHaveVisceralCombat (FIKA handshake).
/// </summary>
public class LivingDismembermentController : MonoBehaviour
{
	private Player _player;
	private BotOwner _botOwner;
	private EBodyPart _dismemberedLeg;
	private Vector3 _lastBloodPos;
	private float _nextBleedTick;
	private float _nextVoiceTick;
	private bool _isInitialized;
	private GameObject _bloodSprayInstance;

	public static LivingDismembermentController Attach(Player player, EBodyPart leg)
	{
		if (player == null || !player.IsAI) return null;
		if (player.HealthController == null || !player.HealthController.IsAlive) return null;

		// Gated by FIKA handshake — all players in raid must have VisceralCombat
		if (!VisceralEntry.AllPlayersHaveVisceralCombat)
		{
			QuickLogger.Log(ELogType.Log, "[LivingDismemberment] Gated out: Not all players have VisceralCombat installed.");
			return null;
		}

		var existing = player.gameObject.GetComponent<LivingDismembermentController>();
		if (existing != null) return existing;

		var controller = player.gameObject.AddComponent<LivingDismembermentController>();
		controller.Init(player, leg);
		return controller;
	}

	private void Init(Player player, EBodyPart leg)
	{
		_player = player;
		_dismemberedLeg = leg;
		_botOwner = player.AIData?.BotOwner;
		_lastBloodPos = player.Transform.position;
		_nextBleedTick = Time.time + 1.0f;
		_nextVoiceTick = Time.time + 2.0f;
		_isInitialized = true;

		// 1. Force Prone immediately
		ForceProneLock();

		// 2. Attach continuous arterial blood spray effect to coto
		AttachBloodSpray();

		// 3. Initial agony voice
		PlayAgonyVoice();

		// 4. Spawn initial blood puddle at amputation site
		EmitBloodDecal();

		QuickLogger.Log(ELogType.Log, $"[LivingDismemberment] Controller initialized on bot '{player.Profile?.Nickname}' ({leg}).");
	}

	private void ForceProneLock()
	{
		if (_botOwner != null && _botOwner.BotLay != null)
		{
			// Prevent GetUp by pushing NextPosibleGetUp into the far future
			_botOwner.BotLay.NextPosibleGetUp = Time.time + 99999f;

			// Only trigger IsLay = true if bot is not already in prone
			if (!_botOwner.BotLay.IsLay)
			{
				_botOwner.BotLay.IsLay = true;
			}
		}
	}

	private void AttachBloodSpray()
	{
		try
		{
			if (VisceralEntry.Instance?.effectContainer?.heavyBleedEffect == null) return;

			Transform parentBone = _player.PlayerBones?.Ribcage?.Original;
			if (parentBone == null) parentBone = _player.Transform.Original;

			_bloodSprayInstance = Object.Instantiate(VisceralEntry.Instance.effectContainer.heavyBleedEffect, parentBone, false);
			_bloodSprayInstance.transform.localPosition = Vector3.zero;
			_bloodSprayInstance.transform.localRotation = Quaternion.identity;

			// Dark coagulated blood styling (matching VisceralCombat aesthetics)
			ParticleSystem[] particleSystems = _bloodSprayInstance.GetComponentsInChildren<ParticleSystem>();
			foreach (ParticleSystem ps in particleSystems)
			{
				RagdollHelperClass.ApplyDarkCoagulatedBloodFx(ps);
			}
		}
		catch (System.Exception ex)
		{
			QuickLogger.Log(ELogType.Error, $"[LivingDismemberment] AttachBloodSpray failed: {ex.Message}");
		}
	}

	private void Update()
	{
		if (!_isInitialized || _player == null) return;

		// Self-destruct if bot dies
		if (_player.HealthController == null || !_player.HealthController.IsAlive)
		{
			Destroy(this);
			return;
		}

		// 1. Re-assert Prone Lock every frame
		ForceProneLock();

		// 2. Heavy Bleed damage loop (15 HP every 2.5s) — irremediable
		if (Time.time >= _nextBleedTick)
		{
			_nextBleedTick = Time.time + 2.5f;
			ApplyHeavyBleedDamage();
		}

		// 3. Blood trail decals & 3D pools as bot crawls (every 0.5m moved)
		if (Vector3.Distance(_player.Transform.position, _lastBloodPos) >= 0.5f)
		{
			_lastBloodPos = _player.Transform.position;
			EmitBloodDecal();
		}

		// 4. Periodic agony voice (every 8-14s)
		if (Time.time >= _nextVoiceTick)
		{
			_nextVoiceTick = Time.time + Random.Range(8.0f, 14.0f);
			PlayAgonyVoice();
		}
	}

	private void ApplyHeavyBleedDamage()
	{
		try
		{
			if (_player?.ActiveHealthController == null) return;
			_player.ActiveHealthController.ApplyDamage(_dismemberedLeg, 15f, GClass3051.HeavyBleedingDamage);
		}
		catch { }
	}

	private void EmitBloodDecal()
	{
		try
		{
			Vector3 origin = _player.Transform.position + Vector3.up * 0.5f;
			int layerMask = LayerMask.GetMask("Terrain", "HighPolyCollider", "LowPolyCollider", "Default");

			if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 3.0f, layerMask))
			{
				Vector3 normal = hit.normal.sqrMagnitude > 0.001f ? hit.normal : Vector3.up;

				// A. Tarkov Native Deferred Decals (Ground Blood)
				if (Singleton<Effects>.Instantiated && Singleton<Effects>.Instance != null)
				{
					Singleton<Effects>.Instance.EmitBleeding(hit.point, normal);
					Singleton<Effects>.Instance.EmitBloodOnEnvironment(hit.point, normal);
				}

				// B. VisceralCombat 3D Blood FX Puddle Prefabs from blood3dFxEffects
				var container = VisceralEntry.Instance?.effectContainer;
				if (container != null && container.blood3dFxEffects != null && container.blood3dFxEffects.Count > 0 && GoreObjectPool.Instance != null)
				{
					int maxIndex = Mathf.Min(12, container.blood3dFxEffects.Count);
					int randomIndex = Random.Range(0, maxIndex);
					GameObject prefab = container.blood3dFxEffects[randomIndex];

					if (prefab != null)
					{
						Quaternion rot = Quaternion.LookRotation(normal) * Quaternion.Euler(90f, Random.Range(0f, 360f), 0f);
						GameObject spawned = GoreObjectPool.Instance.Spawn(prefab, hit.point + normal * 0.015f, rot);

						if (spawned != null)
						{
							spawned.transform.localScale = Vector3.one * Random.Range(0.6f, 1.2f);

							// Dark coagulated blood styling
							ParticleSystem[] particleSystems = spawned.GetComponentsInChildren<ParticleSystem>();
							foreach (ParticleSystem ps in particleSystems)
							{
								RagdollHelperClass.ApplyDarkCoagulatedBloodFx(ps);
							}

							// Recycle pool asset after 30 seconds
							GoreObjectPool.Instance.Recycle(spawned, 30f);
						}
					}
				}

				QuickLogger.Log(ELogType.Log, $"[LivingDismemberment] Emitted blood trail puddle at {hit.point}");
			}
		}
		catch (System.Exception ex)
		{
			QuickLogger.Log(ELogType.Error, $"[LivingDismemberment] EmitBloodDecal failed: {ex.Message}");
		}
	}

	private void PlayAgonyVoice()
	{
		try
		{
			if (_player?.Speaker != null && _player.HealthController != null && _player.HealthController.IsAlive)
			{
				_player.Speaker.Play(EPhraseTrigger.OnAgony, ETagStatus.Dying, true);
			}
		}
		catch { }
	}

	private void OnDestroy()
	{
		_isInitialized = false;
		if (_bloodSprayInstance != null)
		{
			Destroy(_bloodSprayInstance);
			_bloodSprayInstance = null;
		}
	}
}
