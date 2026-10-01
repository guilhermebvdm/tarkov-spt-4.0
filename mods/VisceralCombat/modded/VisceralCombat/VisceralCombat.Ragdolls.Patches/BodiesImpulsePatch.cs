using System;
using System.Linq;
using System.Reflection;
using EFT;
using EFT.Ballistics;
using EFT.Interactive;
using EFT.InventoryLogic;
using SPT.Reflection.Patching;
using UnityEngine;
using VisceralCombat.Ragdolls.Classes;

namespace VisceralCombat.Ragdolls.Patches;

public class BodiesImpulsePatch : ModulePatch
{
	// Teto e piso de massa pra conversão impulso→velocidade no ramo de item largado: itens mais
	// pesados que isso são tratados, só pra esta conta, como se pesassem MassCapKg — dá um piso de
	// reação perceptível pra itens pesados (arma, capacete) sem alterar a reação de itens já mais
	// leves que o teto (máscara, óculos, fone continuam usando a massa real, comportamento idêntico
	// ao de hoje). MinMassKg protege contra item com peso zero/quase-zero cadastrado (ex.: chave,
	// documento, item de quest) — sem ele, a divisão manual poderia gerar Infinity/NaN e quebrar a
	// física daquele item (ref: item 007, PA-01-01).
	private const float MassCapKg = 0.5f; // ref: item 007 — calibrar em jogo se necessário
	private const float MinMassKg = 0.05f; // ref: item 007, PA-01-01 — piso de segurança, não deve afetar item real

	protected override MethodBase GetTargetMethod()
	{
		return typeof(BallisticsCalculator).GetMethod("Shoot", BindingFlags.Instance | BindingFlags.Public, null, new Type[1] { typeof(EftBulletClass) }, null);
	}

	[PatchPostfix]
	private static void Postfix(EftBulletClass shot)
	{
		if (shot == null) return;
		VisceralCombat.Combined.Classes.VisceralShotProcessor.RegisterShot(shot);
	}

	public static void ProcessImpulse(EftBulletClass shot)
	{
		if (shot == null || shot.HitCollider == null) return;

		Collider hitCollider = shot.HitCollider;

		// Calculate realistic physical momentum: p = m * v (mass in kg * speed in m/s) with 0.25f scale
		float massKg = (shot.BulletMassGram > 0f) ? (shot.BulletMassGram / 1000f) : 0.008f;
		float speed = (shot.Speed > 0f) ? shot.Speed : 400f;
		float physicalImpulse = (massKg * speed) * 0.25f;

		// Check if hitting dropped loot item
		Rigidbody lootRb = hitCollider.attachedRigidbody ?? hitCollider.GetComponentInParent<Rigidbody>();
		if (lootRb != null && lootRb.gameObject.GetComponent<ObservedLootItem>() != null)
		{
			if (VisceralEntry.Instance != null && VisceralEntry.Instance.IsCategoryActive(VisceralEntry.Instance.ItemForce)) // ref: item 005
			{
				physicalImpulse *= VisceralEntry.Instance.objectIntensity.Value;

				// ref: item 007 (fix 01) — teto/piso de massa efetiva. lootRb.mass é o peso real do
				// item (LootItem.cs:334 — _rigidBody.mass = item_0.TotalWeight). Clamp() garante que
				// itens ACIMA do teto usem o teto, itens com peso ~zero usem o piso (PA-01-01), e
				// qualquer item com peso realista entre os dois use a massa real, sem mudança.
				float effectiveMass = Mathf.Clamp(lootRb.mass, MinMassKg, MassCapKg);

				// ref: item 007, fix 01 — PhysX não suporta ForceMode.VelocityChange nativamente em
				// AddForceAtPosition (só Force/Impulse têm implementação nativa; VelocityChange usa
				// um caminho custom da Unity, com torque/rotação pouco previsíveis quando o ponto de
				// impacto está longe do centro de massa — foi o que causou o item "reagir fraco
				// demais" em testes reais). Em vez de dividir manualmente e trocar o ForceMode,
				// escalamos o impulso pela razão massa-real/massa-efetiva e deixamos ForceMode.Impulse
				// (nativo, mesmo caminho de antes do item 007) fazer a divisão pela massa REAL — o
				// resultado da velocidade final é idêntico ao pretendido (physicalImpulse/effectiveMass),
				// mas o torque volta a usar o tensor de inércia real do jeito que sempre usou.
				Vector3 scaledImpulse = shot.Direction * (physicalImpulse * (lootRb.mass / effectiveMass));
				lootRb.AddForceAtPosition(scaledImpulse, shot.HitPoint, ForceMode.Impulse);
			}
			return;
		}

		// Never apply ragdoll impulse or wake rigidbodies on a living player/bot!
		Player targetPlayer = hitCollider.GetComponentInParent<Player>();
		if (targetPlayer != null && targetPlayer.HealthController != null && targetPlayer.HealthController.IsAlive)
		{
			return;
		}

		// ref: item 005 (§5.5) — sem toggle mestre, corpo usa física vanilla (sem impulso/wake customizado)
		if (VisceralEntry.Instance == null || !VisceralEntry.Instance.VisceralCombatEnabled.Value) return;

		// Wake the corpse's rigidbodies and re-support them in EFT physics
		Rigidbody[] corpseRbs = RagdollHelperClass.WakeCorpse(hitCollider, 2.5f);

		// Resolve the target Rigidbody to apply impulse
		Rigidbody targetRb = hitCollider.attachedRigidbody ?? hitCollider.GetComponentInParent<Rigidbody>();
		if (targetRb == null && corpseRbs != null && corpseRbs.Length > 0)
		{
			targetRb = corpseRbs.FirstOrDefault(r => r != null && !RagdollHelperClass.ParentIsDismembered(r.transform));
		}

		if (targetRb == null) return;

		string hitName = hitCollider.name.ToLower();
		float bodyPartMult = 1.0f;
		if (VisceralEntry.Instance != null)
		{
			if (hitName.Contains("head")) bodyPartMult = VisceralEntry.Instance.headForceIntensity?.Value ?? 1f;
			else if (hitName.Contains("pelvis") || hitName.Contains("spine") || hitName.Contains("rib")) bodyPartMult = VisceralEntry.Instance.TorsoForceIntensity?.Value ?? 1f;
			else if (hitName.Contains("arm")) bodyPartMult = VisceralEntry.Instance.ArmsForceIntensity?.Value ?? 1f;
			else if (hitName.Contains("thigh") || hitName.Contains("calf") || hitName.Contains("foot")) bodyPartMult = VisceralEntry.Instance.LegsForceIntensity?.Value ?? 1f;
		}

		float totalIntensity = (VisceralEntry.Instance != null && VisceralEntry.Instance.ShotIntensity != null) ? VisceralEntry.Instance.ShotIntensity.Value : 1f;
		Vector3 impulse = shot.Direction * (physicalImpulse * bodyPartMult * totalIntensity);

		targetRb.AddForceAtPosition(impulse, shot.HitPoint, ForceMode.Impulse);
	}
}
