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
///   3. Attaches continuous blood spray (limbSquirter) with ParticleFloorPainter — physical droplets paint floor decals as the bot crawls.
///   4. Plays agony audio phrases periodically.
///   5. Strictly gated by VisceralEntry.AllPlayersHaveVisceralCombat (FIKA handshake).
/// </summary>
public class LivingDismembermentController : MonoBehaviour
{
	private Player _player;
	private BotOwner _botOwner;
	private EBodyPart _dismemberedLeg;
	private float _nextBleedTick;
	private float _nextVoiceTick;
	private bool _isInitialized;
	private GameObject _bloodSprayInstance;
	private Transform _targetBone;

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
		_nextBleedTick = Time.time + 1.0f;
		_nextVoiceTick = Time.time + 2.0f;
		_isInitialized = true;

		// 1. Force Prone immediately
		ForceProneLock();

		// 2. Attach continuous arterial blood spray effect (Visceral Abordagem A: ParticleFloorPainter)
		AttachBloodSpray();

		// 3. Initial agony voice
		PlayAgonyVoice();

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
			var container = VisceralEntry.Instance?.effectContainer;
			if (container == null) return;

			GameObject prefabToUse = container.limbSquirter ?? container.heavyBleedEffect;
			if (prefabToUse == null) return;

			// Target bone for the amputated leg
			string targetBoneName = (_dismemberedLeg == EBodyPart.LeftLeg) ? "lthigh1" : "rthigh1";
			_targetBone = null;

			if (_player.Transform?.Original != null)
			{
				foreach (Transform t in VisceralCombat.Ragdolls.Classes.Utils.EnumerateHierarchyCore(_player.Transform.Original))
				{
					if (t != null && t.name.ToLower().Contains(targetBoneName))
					{
						_targetBone = t;
						break;
					}
				}
			}

			// Parent to root transform (scale 1.0, 1.0, 1.0) so particle velocity/scale is never 0.0001f
			Transform parentTransform = _player.Transform?.Original ?? _player.gameObject.transform;

			_bloodSprayInstance = Object.Instantiate(prefabToUse, parentTransform, false);
			_bloodSprayInstance.transform.position = _targetBone != null ? _targetBone.position : parentTransform.position;
			_bloodSprayInstance.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
			_bloodSprayInstance.transform.localScale = Vector3.one;

			var painter = _bloodSprayInstance.GetComponent<ParticleFloorPainter>() ?? _bloodSprayInstance.AddComponent<ParticleFloorPainter>();
			painter.CooldownSeconds = 1.0f; // 1 blood decal per second max

			ParticleSystem[] particleSystems = _bloodSprayInstance.GetComponentsInChildren<ParticleSystem>();
			foreach (ParticleSystem ps in particleSystems)
			{
				if (ps == null) continue;

				var main = ps.main;
				main.loop = true; // Continuous spray while alive

				var emission = ps.emission;
				emission.rateOverTime = 3f; // Moderate, clean squirt rate

				var collision = ps.collision;
				collision.enabled = true;
				collision.sendCollisionMessages = true;

				var childPainter = ps.gameObject.GetComponent<ParticleFloorPainter>() ?? ps.gameObject.AddComponent<ParticleFloorPainter>();
				childPainter.CooldownSeconds = 1.0f; // 1 blood decal per second max

				RagdollHelperClass.ApplyDarkCoagulatedBloodFx(ps);
				ps.Play();
			}

			QuickLogger.Log(ELogType.Log, $"[LivingDismemberment] Attached continuous blood spray with 1s decal cooldown to '{(_targetBone != null ? _targetBone.name : parentTransform.name)}'.");
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

		// 2. Keep blood spray positioned at leg coto without inheriting 0.0001 scale
		if (_bloodSprayInstance != null && _targetBone != null)
		{
			_bloodSprayInstance.transform.position = _targetBone.position;
		}

		// 3. Heavy Bleed damage loop (15 HP every 2.5s) — irremediable
		if (Time.time >= _nextBleedTick)
		{
			_nextBleedTick = Time.time + 2.5f;
			ApplyHeavyBleedDamage();
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
