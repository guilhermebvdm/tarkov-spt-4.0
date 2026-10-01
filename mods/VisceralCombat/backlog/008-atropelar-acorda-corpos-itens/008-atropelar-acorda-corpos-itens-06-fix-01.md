# 008 — Fix 01 · `ForceMode.VelocityChange` produzia empurrão fraco/nulo por contato

**Mod:** VisceralCombat
**Item raiz:** [008-atropelar-acorda-corpos-itens-01-spec.md](008-atropelar-acorda-corpos-itens-01-spec.md)
**Asbuild:** [008-atropelar-acorda-corpos-itens-05-asbuild.md](008-atropelar-acorda-corpos-itens-05-asbuild.md)
**Criado:** 2026-09-21
**Disparado por:** feedback in-raid do usuário, testando a build 3.12.0 (itens 006+007+008 juntos)

## Contexto

Usuário reportou, após testar a build 3.12.0: "física continua não funcionando com tiros nem atropelo nos itens do chão (exceto máscaras)." O empurrão por contato deste item não estava reagindo perceptivelmente em nenhum item testado. Mesma causa raiz do item `007` — ver [007-piso-impulso-tiro-itens-06-fix-01.md](../007-piso-impulso-tiro-itens/007-piso-impulso-tiro-itens-06-fix-01.md) pro diagnóstico completo (pesquisa Unity Discussions confirmando que `AddForceAtPosition` + `ForceMode.VelocityChange` não é nativo do PhysX, com torque pouco previsível).

## Causa raiz

Idêntica ao item `007`: o stub original de `PlayerContactPushPatch.cs` aplicava `deltaV` via `AddForceAtPosition(deltaV, hit.point, ForceMode.VelocityChange)` — caminho custom da Unity (não-nativo do PhysX), que produz torque exagerado/pouco previsível quando o ponto de contato está deslocado do centro de massa (praticamente sempre), mascarando a componente linear visível do empurrão. Refuta a mesma premissa não-verificada da spec técnica original (`008-atropelar-acorda-corpos-itens-02-spec-tech.md` §1/§5).

## Mudanças aplicadas

| Arquivo | Mudança |
|---|---|
| `modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/PlayerContactPushPatch.cs` | `deltaV` (empurrão de contato) agora multiplicado pela massa real do `Rigidbody` alvo (`rb.mass`) antes de aplicar via `ForceMode.Impulse` (caminho nativo do PhysX) em vez de `ForceMode.VelocityChange` — PhysX divide de volta pela massa real, resultando na mesma `deltaV` pretendida, com torque calculado de forma nativa/consistente. |

## Checklist de validação (obrigatório antes de marcar o fix como entregue)

- [x] Compila via `/compile-mod` sem erros (build 3.12.1)
- [ ] **In-raid:** comportamento corrigido observado em raid real — pendente de novo teste do usuário (encostar leve vs. correr sobre corpo/item já acomodado, sem tiro prévio)
- [ ] **Fika/multiplayer:** sem regressão com outros players — ou `N/A: <razão>`
- [ ] **raid1 → exit → raid2:** sem estado vazado entre raids
- [ ] **alt-F4 / morte / MIA:** teardown idempotente, sem exceção no LogOutput.log
- [ ] Memória do mod atualizada (`/update-memory`) com a lição do fix

## Histórico

| Data | Evento |
|---|---|
| 2026-09-21 | Fix criado e aplicado — build 3.12.1 compilada, 0 erros. Aguardando validação em raid do usuário. |
