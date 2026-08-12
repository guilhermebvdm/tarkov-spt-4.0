# 001 — Desmembramento de Perna em Bots Vivos · Spec Técnica

**Mod:** VisceralCombat
**Spec funcional:** [001-alive-leg-dismemberment-01-spec.md](001-alive-leg-dismemberment-01-spec.md)
**Criado:** 2026-08-11

> Fonte primária de verdade para qualquer assinatura, fórmula ou ponto de patch: [references/eft-decompiled/Assembly-CSharp/](../../../../references/eft-decompiled/Assembly-CSharp/). Toda referência ao código do EFT deve citar `arquivo.cs:linha`. Wiki SPT e fontes externas só como complemento.

## 1. Estratégia

Instanciar um `LivingDismembermentController` (`MonoBehaviour`) customizado no GameObject de qualquer bot IA (`Player.IsAI == true`) que sobreviver à amputação de perna (`EBodyPart.LeftLeg` ou `RightLeg`).
O controller forçará a postura de prone (`BotLay.IsLay = true`), bloqueará tentativas de levantar (`BotLay.NextPosibleGetUp = Time.time + 99999f`), aplicará dano contínuo de sangramento em vida (`ActiveHealthController.ApplyDamage`), emitirá poças visuais de sangramento nativas (`Effects.EmitBleeding`) a cada 0.2s e reproduzirá vozes de agonia (`Speaker.Play`).

## 2. Pontos de patch

| Alvo (Assembly) | Tipo | Motivo |
|---|---|---|
| [`Assembly-CSharp/BotLay.cs:34`](../../../../references/eft-decompiled/Assembly-CSharp/BotLay.cs#L34) | Direct API | Forçar e manter postura de bruços (`IsLay = true`) |
| [`Assembly-CSharp/EFT.HealthSystem/ActiveHealthController.cs:3721`](../../../../references/eft-decompiled/Assembly-CSharp/EFT.HealthSystem/ActiveHealthController.cs#L3721) | Direct API | Aplicar dano por sangramento de 10 HP/s em vida |
| [`Assembly-CSharp/Systems.Effects/Effects.cs:513`](../../../../references/eft-decompiled/Assembly-CSharp/Systems.Effects/Effects.cs#L513) | Direct API | Emitir poças nativas de sangue no chão via Raycast |

## 3. Novas propriedades F12 (BepInEx)

N/A. Usa a infraestrutura existente de configuração do VisceralCombat (`VisceralEntry`).

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `modded/VisceralCombat/VisceralCombat.Dismemberment.Classes/LivingDismembermentController.cs` | CRIAR | Controlador de bot vivo com perna amputada |
| `modded/VisceralCombat/VisceralCombat.Combined.Patches/KillPatch.cs` | MODIFICAR | Gatilho para anexo do `LivingDismembermentController` |
| `modded/VisceralCombat/VisceralCombat.Ragdolls.Classes/RagdollHelperClass.cs` | MODIFICAR | Ajustar `limbSize` para `(0.1f, 0.1f, 0.1f)` para evitar zero-vector warnings |
| `modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/BodiesImpulsePatch.cs` | MODIFICAR | Momentun físico universal $p = m \cdot v$ em tiros em cadaveres |

## 5. Stubs de código

```csharp
// modded/VisceralCombat/VisceralCombat.Dismemberment.Classes/LivingDismembermentController.cs
using EFT;
using EFT.HealthSystem;
using Comfort.Common;
using UnityEngine;

namespace VisceralCombat.Dismemberment.Classes;

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
		if (player == null || !player.IsAI || player.HealthController == null || !player.HealthController.IsAlive) return null;
		var controller = player.gameObject.AddComponent<LivingDismembermentController>();
		controller.Init(player, leg);
		return controller;
	}

	private void Init(Player player, EBodyPart leg)
	{
		_player = player;
		_dismemberedLeg = leg;
		_botOwner = player.AIData?.BotOwner;
		_isInitialized = true;
	}
}
```

## 6. Fluxo de dados

```
[Disparo] → [KillPatch.DismemberLimb] → [LivingDismembermentController.Attach] → [ForceProneLock + ApplyDamage(10f) + EmitBleeding]
```

## 7. Riscos e dependências

- **Sanidade de IA:** Bloquear get-up pode fazer a IA tentar rotacionar em prone (tratado pelo evento `OnCantRotate` nativo do `BotLay`).
- **Gated by FIKA:** Exige Handshake `VisceralEntry.AllPlayersHaveVisceralCombat`.

## 8. Checklist de implementação

- [x] Criar `LivingDismembermentController.cs` com prone lock e loop de sangramento.
- [x] Ajustar `limbSize` em `RagdollHelperClass.cs` para `Vector3(0.1f, 0.1f, 0.1f)`.
- [x] Ajustar `BodiesImpulsePatch.cs` para momento linear $p = m \cdot v$.

## 9. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid: start hook + stop hooks idempotentes (`GameWorld.OnDestroy` + `BaseLocalGame.Stop`) — AP-01 | ✅ | Destroy em `OnDestroy` do `LivingDismembermentController` |
| 2 | Filtro MainPlayer/Fika em todo patch que reage a ação de player — AP-02 | ✅ | `player.IsAI == true` e `VisceralEntry.AllPlayersHaveVisceralCombat` |
| 3 | Alvos ofuscados/virtuais resolvidos por assinatura — AP-03 | ✅ | `BotLay.IsLay` acessado via propriedade |
| 4 | Mudança de estado via API canônica do EFT — AP-04 | ✅ | `ActiveHealthController.ApplyDamage` |
| 5 | Estado entre raids cobertos | ✅ | Destruição automática via `MonoBehaviour.OnDestroy` ao limpar cena |
| 6 | Defaults de ConfigEntry sem ambiguidade — AP-05 | N/A | Sem novos ConfigEntry introduzidos |
| 7 | Re-invocação de método patcheado tem reentry-guard — AP-07 | N/A | Sem reentry em patches |
| 8 | Flags/caches de intercept validados contra o contexto atual — AP-08 | ✅ | `HealthController.IsAlive` checado a cada tick |
| 9 | Todo patch-point reconfirmado no .cs do dump — AP-09 | ✅ | `BotLay.cs:34` e `ActiveHealthController.cs:3721` |
| 10 | Skill EFT usada como lever confirmada não-inerte — AP-10 | N/A | Não modifica skills |
| 11 | Pacote FIKA próprio — AP-11 | N/A | Usa handshake de presença de mod existente |

## Histórico

| Data | Evento |
|---|---|
| 2026-08-11 | Spec técnica criada via `/create-technical-spec` |
