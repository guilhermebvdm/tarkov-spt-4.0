# 007 — Fix 01 · `ForceMode.VelocityChange` produzia reação fraca/nula em vez do piso pretendido

**Mod:** VisceralCombat
**Item raiz:** [007-piso-impulso-tiro-itens-01-spec.md](007-piso-impulso-tiro-itens-01-spec.md)
**Asbuild:** [007-piso-impulso-tiro-itens-05-asbuild.md](007-piso-impulso-tiro-itens-05-asbuild.md)
**Criado:** 2026-09-21
**Disparado por:** feedback in-raid do usuário, testando a build 3.12.0 (itens 006+007+008 juntos)

## Contexto

Usuário reportou, após testar a build 3.12.0: "física continua não funcionando com tiros nem atropelo nos itens do chão (exceto máscaras), a máscara se move bem pouco como se o tiro não fosse forte o suficiente pra mover." Ou seja: nem o piso de massa do item 007 (tiro) nem o empurrão por contato do item 008 (atropelar) produziam a reação esperada — só a máscara reagia, e ainda assim fraco.

Build confirmada como sendo a correta antes de investigar mais (mesmo hash/tamanho/timestamp do `.dll` em `E:\Tarkov Red Line` e `E:\Tarkov Red Line - SERVER TEST`, header do `.cfg` mostrando `v3.12.0`) — não era um caso de build desatualizada.

## Causa raiz

O stub original de ambos os itens (`BodiesImpulsePatch.cs`, ramo de item largado, e `PlayerContactPushPatch.cs`) calculava a mudança de velocidade desejada manualmente e aplicava via `Rigidbody.AddForceAtPosition(deltaV, point, ForceMode.VelocityChange)`. Pesquisa (Unity Discussions, thread ["Why does ForceMode change how angular velocity is calculated in AddForceAtPosition"](https://discussions.unity.com/t/why-does-forcemode-change-how-angular-velocity-is-calculated-in-addforceatposition/940069)) confirma que **PhysX não suporta nativamente `ForceMode.Acceleration`/`ForceMode.VelocityChange` na função `addForceAtPos`** — só `Force`/`Impulse` têm implementação nativa. Pra `VelocityChange`, a Unity usa um caminho **custom**, cujo comportamento de torque fica pouco previsível quando o ponto de aplicação (`shot.HitPoint`/`hit.point`) está deslocado do centro de massa (praticamente sempre o caso aqui) — na prática, a maior parte da "energia" aplicada acabava indo pra rotação de forma exagerada/inconsistente em vez de deslocamento visível, fazendo o item parecer não reagir (ou reagir fraco, caso da máscara).

Isso **refuta uma premissa da spec técnica original de ambos os itens** (`007-piso-impulso-tiro-itens-02-spec-tech.md` §1/§5 e `008-atropelar-acorda-corpos-itens-02-spec-tech.md` §1/§5), que assumiam `ForceMode.VelocityChange` seria um substituto direto e prevcircível de `ForceMode.Impulse` pra componente linear, sem afetar o torque. Essa suposição não foi verificada contra documentação/comportamento real da Unity antes do `/code-mod` — lição registrada na memória do mod.

## Mudanças aplicadas

| Arquivo | Mudança |
|---|---|
| `modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/BodiesImpulsePatch.cs` | Ramo de item largado: em vez de calcular `deltaV` e aplicar via `ForceMode.VelocityChange`, agora escala o impulso pela razão `massa real / massa efetiva` (`Mathf.Clamp` inalterado) e aplica via `ForceMode.Impulse` (caminho nativo do PhysX) — PhysX divide de volta pela massa real, produzindo a mesma velocidade final pretendida, com torque calculado do jeito nativo de sempre (idêntico ao comportamento pré-item-007 pra itens dentro da faixa não-grampeada). |
| `modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/PlayerContactPushPatch.cs` | Mesma técnica: `deltaV` (empurrão de contato) multiplicado pela massa real do alvo antes de aplicar via `ForceMode.Impulse`. Fix compartilhado com o item `008` — ver [008-atropelar-acorda-corpos-itens-06-fix-01.md](../008-atropelar-acorda-corpos-itens/008-atropelar-acorda-corpos-itens-06-fix-01.md). |

## Checklist de validação (obrigatório antes de marcar o fix como entregue)

- [x] Compila via `/compile-mod` sem erros (build 3.12.1)
- [ ] **In-raid:** comportamento corrigido observado em raid real — pendente de novo teste do usuário
- [ ] **Fika/multiplayer:** sem regressão com outros players — ou `N/A: <razão>`
- [ ] **raid1 → exit → raid2:** sem estado vazado entre raids
- [ ] **alt-F4 / morte / MIA:** teardown idempotente, sem exceção no LogOutput.log
- [ ] Memória do mod atualizada (`/update-memory`) com a lição do fix

## Histórico

| Data | Evento |
|---|---|
| 2026-09-21 | Fix criado e aplicado — build 3.12.1 compilada, 0 erros. Aguardando validação em raid do usuário. |
