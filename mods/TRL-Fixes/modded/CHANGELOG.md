# Changelog — TRL-Fixes

Versões mais recentes primeiro.

---

## v1.1.0 (2026-08-01)

### Nova correção — trava de controles ao pegar item do chão

- **`PickupAimingSafetyPatch`**: ao pegar ou equipar um item do chão pelo menu de ação nativo (mais comum com
  coletes e rigs), o corpo do personagem podia **congelar** — não anda, não agacha, não vira a visão — enquanto
  o inventário e a troca de arma continuavam respondendo. O patch impede a trava.
- O patch **já existia** neste mod, foi movido para `stancesAndCameraPositionSPT4.0.11` em 2026-07-25 sem
  registro, e voltou para cá: é remendo sobre bug do jogo base, e o mod de stances está sendo preparado para
  publicação pública.
- **Logging forense**: a primeira ocorrência sai no console com a **pilha de chamadas completa**; as seguintes
  saem com throttle de 5 s e contador acumulado. A causa raiz descrita no diagnóstico
  ([`docs/handoff-pickup-aiming-safety.md`](../docs/handoff-pickup-aiming-safety.md)) é coerente com o
  decompilado mas **nunca foi capturada em raid** — esse primeiro registro é o que confirma ou refuta.
- Reescrito no estilo do mod (Harmony direto, sem SPT.Reflection).

### Manutenção

- Versão passa a ser declarada também no `.csproj` (`Version`/`AssemblyVersion`/`FileVersion`). Sem isso a DLL
  saía marcada como `1.0.0.0` independentemente da versão do plugin.

---

## v1.0.0

Versão inicial: `FlashbangBotPatch`, `FlashbangRadiusPatch`, `Patch_PoolManagerCreateItem` e
`FixFikaReviveRagdollPatch`.
