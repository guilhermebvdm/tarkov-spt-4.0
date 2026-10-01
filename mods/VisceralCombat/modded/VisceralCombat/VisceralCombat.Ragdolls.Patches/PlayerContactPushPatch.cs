using System;
using System.Collections;
using System.Collections.Generic;
using Comfort.Common;
using EFT;
using EFT.Interactive;
using Fika.Core.Main.Utils;
using Nexus.BundleLoader;
using UnityEngine;
using VisceralCombat.Ragdolls.Classes;

namespace VisceralCombat.Ragdolls.Patches;

/// <summary>
/// Acorda/empurra corpos e itens físicos perto dos pés do jogador humano local ("atropelar"),
/// sem depender de tiro/granada prévios.
///
/// ref: item 008, fix 02 — reescrito de zero. A versão original patcheava
/// Player.OnControllerColliderHit (Assembly-CSharp/EFT/Player.cs:28849), mas esse evento nunca é
/// chamado pelo jogo: nem o jogador local nem os bots usam o CharacterController nativo da Unity
/// (ApplicationConfigClass.cs — ClientPlayerMode/BotPlayerMode usam ControllerType.Simple, que
/// cria um SimpleCharacterController com colisão 100% customizada, sem disparar esse callback).
/// Confirmado em teste real: log incondicional no Postfix nunca imprimiu uma linha sequer.
///
/// Nova estratégia: checagem periódica por proximidade (Physics.OverlapSphere perto da posição do
/// jogador), mesma técnica já usada por GrenadeItemsPatch — funciona independente de qual sistema
/// de movimento o jogo usa por baixo.
/// </summary>
public static class PlayerContactPushPatch
{
	private const float CheckIntervalSeconds = 0.2f; // ref: pedido do usuário — equilíbrio custo/responsividade
	private const float CheckRadius = 0.6f; // m — raio de detecção perto do jogador
	private const float MinSpeedForPush = 0.3f; // m/s — abaixo disso, considera "parado", sem empurrão
	private const float MaxSpeedForPush = 6f; // m/s — teto pra não deixar sprint gerar empurrão exagerado
	private const float PushIntensity = 0.5f; // ref: item 008, fix 04 — só usado como fallback se VisceralEntry.Instance ainda não montou os ConfigEntry (CorpseKickIntensity/ItemKickIntensity, ver VisceralEntry.cs)
	private const float CorpseWakeDuration = 2.5f; // mesma duração default já usada por BodiesImpulsePatch.cs:62
	private const float ContactPushCooldownSeconds = 0.35f; // evita reempurrar o mesmo Rigidbody a cada checagem

	private const int MaxColliders = 32; // folga generosa pro raio pequeno usado aqui
	private static readonly Collider[] _colliderBuffer = new Collider[MaxColliders];

	private static readonly Dictionary<Rigidbody, float> _lastPushTime = new Dictionary<Rigidbody, float>();
	private static int _layerMask = -1;
	private static Coroutine _checkCoroutine;

	// ref: item 008, fix 03 — um corpo recém-morto tem vários ossos, todos muito próximos entre si
	// logo após a morte (ainda não se espalharam no chão). Sem essa deduplicação, cada osso dentro
	// do raio recebia um empurrão INDEPENDENTE na mesma checagem — vários "chutes" simultâneos em
	// pontos diferentes do mesmo corpo, fazendo ele disparar de forma descontrolada (bug reportado
	// pelo usuário: bot morrendo perto dele "ejetava" e sumia). Corpo já assentado no chão não tinha
	// esse problema porque os ossos já estão espalhados (poucos caem no raio pequeno de cada vez).
	// Reaproveitado (não realocado) a cada checagem — mesmo raciocínio do buffer de colliders.
	private static readonly HashSet<Transform> _processedCorpseRoots = new HashSet<Transform>();

	// ref: item 006 — mesmo padrão de limpeza de estado raid-scoped (GameStartedPatch.Postfix)
	public static void ClearContactPushCooldowns()
	{
		_lastPushTime.Clear();
	}

	// ref: item 008, fix 02 — chamado 1x por raid a partir de GameStartedPatch.Postfix
	public static void StartPeriodicCheck()
	{
		if (StaticManager.Instance == null) return;
		StopPeriodicCheck();
		_checkCoroutine = ((MonoBehaviour)StaticManager.Instance).StartCoroutine(PeriodicCheckLoop());
	}

	public static void StopPeriodicCheck()
	{
		if (_checkCoroutine != null && StaticManager.Instance != null)
		{
			((MonoBehaviour)StaticManager.Instance).StopCoroutine(_checkCoroutine);
		}
		_checkCoroutine = null;
	}

	private static IEnumerator PeriodicCheckLoop()
	{
		WaitForSeconds wait = new WaitForSeconds(CheckIntervalSeconds);
		while (true)
		{
			yield return wait;
			try
			{
				CheckOnce();
			}
			catch (Exception ex)
			{
				QuickLogger.Log(ELogType.Error, $"[PlayerContactPushPatch] {ex}");
			}
		}
	}

	private static void CheckOnce()
	{
		if (!Singleton<GameWorld>.Instantiated) return;
		Player player = Singleton<GameWorld>.Instance.MainPlayer;
		if (player == null || player.HealthController == null || !player.HealthController.IsAlive) return;

		Vector3 horizontalVelocity = player.Velocity; // ref: Player.cs:24613
		horizontalVelocity.y = 0f;
		float speed = horizontalVelocity.magnitude;
		if (speed < MinSpeedForPush) return; // parado não empurra

		if (_layerMask < 0)
		{
			_layerMask = (1 << LayerMask.NameToLayer("Default")) | (1 << LayerMask.NameToLayer("Deadbody"));
		}

		Vector3 checkCenter = player.Position; // ref: Player.cs:24637 — PlayerBones.BodyTransform.position
		int count = Physics.OverlapSphereNonAlloc(checkCenter, CheckRadius, _colliderBuffer, _layerMask);
		if (count == 0) return;

		float clampedSpeed = Mathf.Min(speed, MaxSpeedForPush);
		Vector3 pushDirection = horizontalVelocity.normalized;

		_processedCorpseRoots.Clear(); // ref: item 008, fix 03 — reinicia a dedupe por corpo a cada checagem

		for (int i = 0; i < count; i++)
		{
			Collider col = _colliderBuffer[i];
			_colliderBuffer[i] = null; // libera a referência do buffer reaproveitado
			if (col == null) continue;

			Rigidbody rb = col.attachedRigidbody ?? col.GetComponent<Rigidbody>();
			if (rb == null) continue;

			if (_lastPushTime.TryGetValue(rb, out float lastTime) && Time.time - lastTime < ContactPushCooldownSeconds)
			{
				continue; // já empurrado recentemente — evita reaplicar a cada checagem enquanto parado perto
			}

			bool isLootItem = rb.gameObject.GetComponent<ObservedLootItem>() != null;
			bool isCorpseBone = false;
			if (!isLootItem)
			{
				Player corpseOwner = col.GetComponentInParent<Player>();
				isCorpseBone = corpseOwner != null && corpseOwner.HealthController != null && !corpseOwner.HealthController.IsAlive;
			}

			if (!isLootItem && !isCorpseBone) continue; // colisor irrelevante (ex.: outro player vivo, geometria)

			// ref: item 008, fix 03 — só empurra UM osso por corpo por checagem (ver comentário no
			// campo _processedCorpseRoots acima). Itens largados não têm esse problema (1 Rigidbody
			// cada, tipicamente), então a dedupe só se aplica ao ramo de corpo.
			if (isCorpseBone && !_processedCorpseRoots.Add(col.transform.root)) continue;

			// ref: PA-01-01 (item 008) — parte já desmembrada: WakeCorpse trava isKinematic=true e
			// detectCollisions=false pra ela (RagdollHelperClass.cs:775-783) — pular em vez de acionar.
			if (isCorpseBone && RagdollHelperClass.ParentIsDismembered(rb.transform)) continue;

			float intensity;
			if (isLootItem)
			{
				if (VisceralEntry.Instance == null || !VisceralEntry.Instance.IsCategoryActive(VisceralEntry.Instance.ItemForce)) continue; // ref: item 005
				intensity = VisceralEntry.Instance.ItemKickIntensity?.Value ?? PushIntensity;
			}
			else
			{
				if (VisceralEntry.Instance == null || !VisceralEntry.Instance.IsCategoryActive(VisceralEntry.Instance.BodyCollision)) continue; // ref: item 005
				RagdollHelperClass.WakeCorpse(col, CorpseWakeDuration); // ref: RagdollHelperClass.cs:753
				intensity = VisceralEntry.Instance.CorpseKickIntensity?.Value ?? PushIntensity;
			}

			// ref: item 008, fix 04 — intensidade configurável separada por tipo de alvo (pedido do
			// usuário), em vez de um único PushIntensity fixo compartilhado.
			Vector3 deltaV = pushDirection * (clampedSpeed * intensity);

			// ref: item 008, fix 01 — ForceMode.Impulse (nativo do PhysX) em vez de VelocityChange;
			// escala pela massa real do alvo pra obter a mesma deltaV pretendida, com torque nativo.
			Vector3 scaledImpulse = deltaV * rb.mass;
			Vector3 applyPoint = col.ClosestPoint(checkCenter);
			rb.AddForceAtPosition(scaledImpulse, applyPoint, ForceMode.Impulse);

			_lastPushTime[rb] = Time.time;
		}
	}
}
