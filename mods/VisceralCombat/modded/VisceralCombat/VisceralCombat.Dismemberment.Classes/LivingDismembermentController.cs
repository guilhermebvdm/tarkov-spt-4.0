using EFT;
using EFT.HealthSystem;
using Systems.Effects;
using Comfort.Common;
using UnityEngine;

namespace VisceralCombat.Dismemberment.Classes;

/// <summary>
/// Controls living AI bots that survive a leg dismemberment:
///   1. Forces immediate prone pose (BotLay.IsLay = true) and prevents all get-up attempts.
///   2. Emits heavy bleeding damage continually until exsanguination death.
///   3. Creates blood decals on the floor via vanilla-identical Raycast (player.Position -> Vector3.down)
///      — no ParticleSystem parented to any bone, so LookRotation warnings are impossible.
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
	private float _nextDecalTick;

	private bool _isInitialized;

	// Vanilla-identical blood decal interval (0.5s — tighter than vanilla 1-3s for a crawling trail feel)
	private const float DecalInterval = 0.5f;

	// Vanilla bleeding: max Raycast distance downward from player.Position
	private static int? _cachedEnvMask;

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

		_nextBleedTick = Time.time + 1.0f;
		_nextVoiceTick = Time.time + 2.0f;
		_nextDecalTick = Time.time + DecalInterval;
		_isInitialized = true;

		// 1. Force Prone immediately
		ForceProneLock();

		// 2. Initial agony voice
		PlayAgonyVoice();

		QuickLogger.Log(ELogType.Log, $"[LivingDismemberment] Controller initialized on bot '{player.Profile?.Nickname}' ({leg}).");
	}

	private void ForceProneLock()
	{
		if (_botOwner?.BotLay == null) return;

		// Push NextPossibleGetUp very far into the future so the bot never stands up
		_botOwner.BotLay.NextPosibleGetUp = Time.time + 99999f;

		if (!_botOwner.BotLay.IsLay)
			_botOwner.BotLay.IsLay = true;
	}

	/// <summary>
	/// Emits a blood decal on the floor using the exact same method as vanilla EffectsCommutator:
	/// Raycast from player.Position straight down, then call EmitBleeding at the hit point.
	/// No ParticleSystem, no bone attachment, no LookRotation warnings.
	/// </summary>
	private void EmitFloorBloodDecal()
	{
		if (Singleton<Effects>.Instance == null) return;

		// Cache env mask once — same mask used by EffectsCommutator.UpdatePlayersBleedings()
		if (!_cachedEnvMask.HasValue)
		{
			_cachedEnvMask = EFTHardSettings.Instance != null
				? (int)EFTHardSettings.Instance.ENVIRONMENT_HIT_MASK
				: ~(LayerMask.GetMask("Player", "HitCollider", "Deadbody"));
		}

		Vector3 origin = _player.Position;
		float maxDist = EFTHardSettings.Instance != null
			? EFTHardSettings.Instance.DRAW_BLEEDING_MAX_DISTANCE
			: 10f;

		if (Physics.Raycast(origin, Vector3.down, out var hit, maxDist, _cachedEnvMask.Value))
		{
			Singleton<Effects>.Instance.EmitBleeding(hit.point, hit.normal);
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

		// 2. Blood decal on floor — vanilla Raycast method (0.5s cadence → crawling trail)
		if (Time.time >= _nextDecalTick)
		{
			_nextDecalTick = Time.time + DecalInterval;
			EmitFloorBloodDecal();
		}

		// 3. Heavy bleed damage tick (15 HP every 2.5s — irremediable, bleeds to death)
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
				_player.Speaker.Play(EPhraseTrigger.OnAgony, ETagStatus.Dying, true);
		}
		catch { }
	}

	private void OnDestroy()
	{
		_isInitialized = false;
	}
}
