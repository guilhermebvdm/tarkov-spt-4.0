using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using EFT;
using EFT.Interactive;
using Nexus.BundleLoader;
using SPT.Reflection.Patching;
using UnityEngine;

namespace VisceralCombat.Ragdolls.Patches;

public class GrenadeItemsPatch : ModulePatch
{
	// ref: otimização de desempenho (pedido do usuário 2026-09-20) — buffer estático reaproveitado
	// entre explosões, evitando alocar um array novo no heap a cada granada (Physics.OverlapSphere
	// aloca; OverlapSphereNonAlloc não). Tradeoff: se mais de 128 colliders na camada "Default"
	// estiverem no raio da explosão ao mesmo tempo (cenário raro — a camada "Default" também pega
	// geometria do mapa, não só itens), os excedentes são silenciosamente ignorados — diferente do
	// OverlapSphere original, que nunca teria esse limite. 128 é uma folga generosa pro uso comum.
	private const int MaxColliders = 128;
	private static readonly Collider[] _colliderBuffer = new Collider[MaxColliders];

	// ref: bug de code-review (2026-09-20) — PhysicalItemsPatch.cs reatribui itens largados pra
	// camada "Deadbody" assim que a corrotina de assentamento do item começa (quase imediatamente
	// após o drop, bem antes de "parar de se mexer"). A granada só buscava na camada "Default",
	// então nunca encontrava (nem empurrava) um item que já tinha passado por esse reassign - as
	// duas partes da mesma feature (ItemForce) ficavam inconsistentes entre si. Corrigido incluindo
	// as duas camadas na busca. Cacheados como campos estáticos (mesmo padrão já usado em
	// PhysicalItemsPatch._deadbodyLayer) pra não repetir o lookup por nome a cada explosão.
	private static int _layerMask = -1;

	// ref: fix pontual (2026-09-21, pedido do usuário) — AddExplosionForce, sem nenhum teto, podia
	// dar um pico de velocidade tão alto num item leve perto do centro da explosão que ele saía
	// voando a ponto de "desaparecer" do mapa. AddForce*/AddExplosionForce não atualiza
	// Rigidbody.velocity de forma síncrona (só na próxima simulação de física), então o clamp
	// precisa rodar 1 quadro fixo depois, não imediatamente após a chamada.
	private const float MaxItemLaunchSpeed = 15f; // m/s

	private static IEnumerator ClampLaunchSpeedNextFixedUpdate(Rigidbody rb)
	{
		yield return new WaitForFixedUpdate();
		if (rb != null && rb.velocity.magnitude > MaxItemLaunchSpeed)
		{
			rb.velocity = rb.velocity.normalized * MaxItemLaunchSpeed;
		}
	}

	protected override MethodBase GetTargetMethod()
	{
		return typeof(Grenade).GetMethod("Explosion", BindingFlags.Static | BindingFlags.Public);
	}

	[PatchPostfix]
	private static void Postfix(IExplosiveItem grenadeItem, Vector3 grenadePosition)
	{
		if (grenadeItem == null || VisceralEntry.Instance == null) return;
		if (VisceralEntry.Instance.ItemForce == null || !VisceralEntry.Instance.IsCategoryActive(VisceralEntry.Instance.ItemForce)) return; // ref: item 005

		if (_layerMask < 0)
		{
			_layerMask = (1 << LayerMask.NameToLayer("Default")) | (1 << LayerMask.NameToLayer("Deadbody"));
		}

		float maxDist = grenadeItem.MaxExplosionDistance;
		int count = Physics.OverlapSphereNonAlloc(grenadePosition, maxDist, _colliderBuffer, _layerMask);
		if (count == 0) return;

		HashSet<Rigidbody> processedItems = new HashSet<Rigidbody>();
		float forceMultiplier = grenadeItem.GetStrength * 0.5f * VisceralEntry.Instance.GrenadeExplIntensity.Value;

		for (int i = 0; i < count; i++)
		{
			Collider col = _colliderBuffer[i];
			_colliderBuffer[i] = null; // libera a referência do buffer reaproveitado
			if (col == null) continue;

			Rigidbody rb = col.attachedRigidbody ?? col.GetComponent<Rigidbody>();
			if (rb != null && processedItems.Add(rb))
			{
				if (rb.gameObject.GetComponent<ObservedLootItem>() != null)
				{
					rb.AddExplosionForce(forceMultiplier, grenadePosition, maxDist);
					if (StaticManager.Instance != null)
					{
						((MonoBehaviour)StaticManager.Instance).StartCoroutine(ClampLaunchSpeedNextFixedUpdate(rb));
					}
				}
			}
		}
	}
}
