using EFT;
using EFT.HealthSystem;
using Comfort.Common;
using UnityEngine;

namespace VisceralCombat.Dismemberment.Classes;

/// <summary>
/// Controls living AI bots that survive a leg dismemberment:
///   1. Forces immediate prone pose (BotLay.IsLay = true) and prevents all get-up attempts.
///   2. Applies native HeavyBleeding to the amputated leg via ActiveHealthController.DoBleed(true, leg).
///      EFT's native health controller automatically drains HP until exsanguination death,
///      and EFT's native EffectsCommutator automatically handles floor blood decals.
///   3. Re-asserts HeavyBleeding every 1s so the bot can NEVER cure or stop the bleeding.
///   4. Plays agony audio phrases periodically.
///   5. Strictly gated by VisceralEntry.AllPlayersHaveVisceralCombat (FIKA handshake).
/// </summary>
public class LivingDismembermentController : MonoBehaviour
{
	private Player _player;
	private BotOwner _botOwner;
	private EBodyPart _dismemberedLeg;

	private float _nextBleedCheck;
	private float _nextDecalTick;
	private float _nextVoiceTick;
	private bool _isInitialized;

	public static LivingDismembermentController Attach(Player player, EBodyPart leg)
	{
		if (player == null || !player.IsAI) return null;
		if (player.HealthController == null || !player.HealthController.IsAlive) return null;

		// Gated by FIKA handshake — all players in raid must have VisceralCombat
		if (!VisceralEntry.AllPlayersHaveVisceralCombat)
		{
			QuickLogger.Log(ELogType.Log, "[LivingDismemberment] Gated out: Not all players have VisceralCombat.");
			return null;
		}

		// One controller per bot
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

		_nextBleedCheck = Time.time;
		_nextDecalTick = Time.time;
		_nextVoiceTick = Time.time + 2.0f;
		_isInitialized = true;

		// 1. Force Prone immediately
		ForceProneLock();

		// 2. Apply initial native Heavy Bleeding
		EnsureNativeHeavyBleeding();

		// 3. Play agony voice
		PlayAgonyVoice();

		QuickLogger.Log(ELogType.Log, $"[LivingDismemberment] Controller initialized on bot '{player.Profile?.Nickname}' ({leg}).");
	}

	private void ForceProneLock()
	{
		if (_botOwner?.BotLay == null) return;

		// Push NextPossibleGetUp very far into the future so the bot never stands up
		_botOwner.BotLay.NextPosibleGetUp = Time.time + 99999f;

		if (!_botOwner.BotLay.IsLay)
		{
			_botOwner.BotLay.IsLay = true;
		}
	}

	/// <summary>
	/// Applies native HeavyBleeding to the amputated leg.
	/// Native EFT HealthController drains HP across body parts and native EffectsCommutator creates blood decals.
	/// </summary>
	private void EnsureNativeHeavyBleeding()
	{
		try
		{
			if (_player?.ActiveHealthController != null && _player.HealthController.IsAlive)
			{
				_player.ActiveHealthController.DoBleed(true, _dismemberedLeg);
			}
		}
		catch (System.Exception ex)
		{
			QuickLogger.Log(ELogType.Warn, $"[LivingDismemberment] Failed to apply native HeavyBleeding: {ex.Message}");
		}
	}

	private void Update()
	{
		if (!_isInitialized || _player == null) return;

		// Self-destruct when bot dies
		if (_player.HealthController == null || !_player.HealthController.IsAlive)
		{
			Destroy(this);
			return;
		}

		// 1. Re-assert prone lock every frame
		ForceProneLock();

		// 2. Ensure HeavyBleeding is active and apply 35 HP bleed damage per second to accelerate death (making AI healing useless)
		if (Time.time >= _nextBleedCheck)
		{
			_nextBleedCheck = Time.time + 1.0f;
			EnsureNativeHeavyBleeding();
			try
			{
				if (_player?.ActiveHealthController != null && _player.HealthController.IsAlive)
				{
					_player.ActiveHealthController.ApplyDamage(_dismemberedLeg, 35f, GClass3051.HeavyBleedingDamage);
				}
			}
			catch { }
		}

		// 3. 5x faster visual floor blood decal trail (every 0.2s) without increasing HP damage
		if (Time.time >= _nextDecalTick)
		{
			_nextDecalTick = Time.time + 0.2f;
			EmitVisualFloorDecal();
		}

		// 4. Periodic agony voice (every 8-14s)
		if (Time.time >= _nextVoiceTick)
		{
			_nextVoiceTick = Time.time + Random.Range(8.0f, 14.0f);
			PlayAgonyVoice();
		}
	}

	private void EmitVisualFloorDecal()
	{
		try
		{
			if (Singleton<Systems.Effects.Effects>.Instantiated && _player != null)
			{
				Vector3 origin = _player.Position + Vector3.up * 0.5f;
				if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 3f, EFTHardSettings.Instance.ENVIRONMENT_HIT_MASK))
				{
					Singleton<Systems.Effects.Effects>.Instance.EmitBleeding(hit.point, hit.normal);
				}
			}
		}
		catch { }
	}

	private void PlayAgonyVoice()
	{
		try
		{
			if (_player?.Speaker != null && (_player.HealthController?.IsAlive ?? false))
			{
				_player.Speaker.Play(EPhraseTrigger.OnAgony, ETagStatus.Dying, true);
			}
		}
		catch { }
	}

	private void OnDestroy()
	{
		_isInitialized = false;
	}
}
