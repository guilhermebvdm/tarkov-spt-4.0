# 008 — Acordar/Empurrar Corpos e Itens por Contato Físico (Atropelar) · Review Técnica 01

**Mod:** VisceralCombat
**Spec técnica revisada:** [008-atropelar-acorda-corpos-itens-02-spec-tech.md](008-atropelar-acorda-corpos-itens-02-spec-tech.md)
**Data:** 2026-09-21

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-01-MM` (review 01, ponto MM). Resolver até zerar bloqueadores antes de `/code-mod`.

**Memória consultada:** snapshot de 2026-09-21 (Sessão 14), `mods/VisceralCombat/memory/sessions.md`. **Pendências que afetam esta revisão:** `[P-10.2]` (histórico de bugs sutis no sistema de ragdoll ativo) — motivou o achado PA-01-01 abaixo, que toca exatamente esse subsistema (`WakeCorpse`/dismembramento). **Docs técnicos conferidos:** `spt-antipatterns.md` (sempre) — nenhuma contradição direta com a taxonomia; a hierarquia de evidência (`.agents/resources.md`) foi conferida especificamente pro achado PA-01-03 (claim de Fika sem citação de fonte Fika).

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 3 · Total: 3

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | C — Erro de Lógica | 🟡 Importante | `WakeCorpse` em parte desmembrada pode travar ela em `isKinematic=true`/`detectCollisions=false` permanentemente | ✅ Resolvido em 2026-09-21 |
| PA-01-02 | B — Edge Case | 🟡 Importante | Componente vertical de `Velocity` (queda/pouso sobre corpo) não é filtrada — pode empurrar o alvo pra baixo/através do chão | ✅ Resolvido em 2026-09-21 |
| PA-01-03 | A — Gap | 🟢 Menor | Claim sobre `IsYourPlayer` em Fika/coop sem citação de fonte Fika (hierarquia de evidência) | ✅ Resolvido em 2026-09-21 |

## Categorias

- **A — Gaps de Especificação:** informações ausentes que ambiguam a implementação
- **B — Edge Cases:** cenários válidos não cobertos
- **C — Erros de Lógica:** pressupostos errados, contradições, código incompatível com SPT 4.0+

## Impacto

- 🔴 **Bloqueador** — impede implementar ou causa bug/crash garantido
- 🟡 **Importante** — pode causar comportamento errado em cenário relevante
- 🟢 **Menor** — qualidade/clareza, não bloqueia

---

## Pontos

### PA-01-01 · C — Erro de Lógica · 🟡 Importante

**`WakeCorpse` em parte desmembrada pode travar ela em `isKinematic=true`/`detectCollisions=false` permanentemente**

**Problema:** O stub de §5 chama `RagdollHelperClass.WakeCorpse(hit.collider, CorpseWakeDuration)` sempre que `isCorpseBone == true` (detectado via `GetComponentInParent<Player>()` + `!HealthController.IsAlive`), sem nenhuma checagem adicional. Mas `WakeCorpse` já tem, internamente, uma exclusão explícita pra ossos desmembrados ([RagdollHelperClass.cs:775-783](../../../../mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Classes/RagdollHelperClass.cs#L775-L783)): `if (ParentIsDismembered(rb.transform)) { rb.isKinematic = true; rb.detectCollisions = false; continue; }` — se a colisão pertencer a uma parte já desmembrada (mas ainda hierarquicamente sob o `Player` original, cenário que a própria spec marca como incerto em §7 "TODO confirmar"), chamar `WakeCorpse` faz essa parte ser posta em `isKinematic = true` **e** `detectCollisions = false` incondicionalmente — mesmo que ela já estivesse fisicamente ativa e reagindo normalmente antes desse toque. `detectCollisions = false` é mais grave que "sem reação": a partir desse ponto, aquele `Rigidbody` **para de gerar qualquer evento de colisão**, inclusive o próprio `OnControllerColliderHit` que dispara este patch — ou seja, um único toque acidental numa parte desmembrada pode deixá-la permanentemente impossível de tocar/empurrar pelo resto da raid.

**Por que importa:** O mod já tem um histórico de bugs sutis exatamente no sistema de ragdoll ativo/desmembramento nesta sessão (`[P-10.2]` da memória do mod). A §7 da spec técnica atual já reconhece incerteza sobre "parte desmembrada resolve pra `Player`?", mas trata isso como "não vai reagir" (neutro) — na prática, se resolver, o resultado é pior: reage uma vez (ou nem isso) e desliga a própria colisão da parte pro resto da raid, uma regressão visível se o jogador notar que uma parte que "reagia bem antes de eu encostar" para de reagir a qualquer coisa depois.

**Sugestão:** Antes de chamar `WakeCorpse` no branch de corpo, adicionar uma checagem explícita: `if (RagdollHelperClass.ParentIsDismembered(rb.transform)) return;` (usando o mesmo método `internal` já existente em `RagdollHelperClass.cs:508` — acessível de dentro do mesmo assembly `VisceralCombat.dll`) — pula o empurrão inteiro pra esse caso, em vez de acionar a exclusão interna do `WakeCorpse` de forma incidental. Complementar validando em jogo (já presente no checklist §8) se uma parte desmembrada aparece como `ObservedLootItem` (aí cai no branch de item, que não tem esse problema) ou continua sob o `Player` original (aí cai nesse novo early-return, sem reação nenhuma — mais seguro que o comportamento atual do `WakeCorpse`, mesmo que não seja o ideal do critério de aceite).

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Sugestão aplicada — guarda `if (isCorpseBone && RagdollHelperClass.ParentIsDismembered(rb.transform)) return;` adicionada em `008-atropelar-acorda-corpos-itens-02-spec-tech.md` §5 (antes da checagem de toggle/chamada a `WakeCorpse`), com nota atualizada em §7 e item de validação em §8.

---

### PA-01-02 · B — Edge Case · 🟡 Importante

**Componente vertical de `Velocity` (queda/pouso sobre corpo) não é filtrada — pode empurrar o alvo pra baixo/através do chão**

**Problema:** O stub de §5 calcula `Vector3 deltaV = __instance.Velocity.normalized * (clampedSpeed * PushIntensity);` usando `__instance.Velocity` ([Player.cs:24613](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L24613)) em 3 dimensões, incluindo o componente Y (vertical). `MovementContext.Velocity` ([MovementContext.cs:1110](../../../../references/eft-decompiled/Assembly-CSharp/EFT/MovementContext.cs#L1110), `CharacterController.velocity`) reflete a velocidade real do personagem, incluindo queda livre. Um jogador pulando de uma escada/telhado e pousando sobre um corpo ou item vai gerar uma `Velocity` com componente Y fortemente negativa — o `deltaV` resultante empurraria o alvo majoritariamente **pra baixo**, não lateralmente, podendo visualmente cravar o corpo/item no chão ou atravessar geometria, em vez do empurrão horizontal "atropelar" que a spec funcional descreve (esbarrão leve/correndo, não "aterrissagem").

**Por que importa:** Pousar em cima de um corpo/item ao descer de um lugar alto é um cenário bem plausível em mapas com verticalidade (escadas, telhados, janelas) — não é um caso raro. Nenhum critério de aceite ou corner case da spec funcional cobre esse cenário, e a spec técnica atual não filtra a componente vertical antes de calcular a direção/magnitude do empurrão.

**Sugestão:** Calcular a velocidade só com o componente horizontal antes de usar pra direção/magnitude do empurrão: substituir, no stub de §5, `float speed = __instance.Velocity.magnitude;` e o cálculo de `deltaV` por uma versão que zera o eixo Y primeiro, por exemplo:
```csharp
Vector3 horizontalVelocity = __instance.Velocity;
horizontalVelocity.y = 0f;
float speed = horizontalVelocity.magnitude;
// ... (checks de MinSpeedForPush/cooldown/categoria inalterados, usando este `speed`)
Vector3 deltaV = horizontalVelocity.normalized * (Mathf.Min(speed, MaxSpeedForPush) * PushIntensity);
```
Isso faz "atropelar" significar literalmente movimento horizontal, sem interferência de quedas/pulos verticais.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Sugestão aplicada — `horizontalVelocity` (componente Y zerado) usado tanto pra `speed`/`MinSpeedForPush` quanto pra direção de `deltaV` em `008-atropelar-acorda-corpos-itens-02-spec-tech.md` §5, com §6 e §7 atualizados e item de validação em §8.

---

### PA-01-03 · A — Gap · 🟢 Menor

**Claim sobre `IsYourPlayer` em Fika/coop sem citação de fonte Fika (hierarquia de evidência)**

**Problema:** §1 afirma: "`IsYourPlayer`... verdadeiro só pra o personagem controlado localmente nesta máquina (funciona igual em host e em peer)" — uma afirmação sobre comportamento em **coop**, que segundo a hierarquia de evidência (`.agents/resources.md`) deveria vir de `references/fika-plugin/`/`references/fika-server/` (fonte 🥇 pra lógica cooperativa), não só de convenção geral do EFT. A spec técnica original não cita nenhum arquivo do Fika pra essa afirmação.

**Por que importa:** Sem essa confirmação, a garantia central de "só jogador humano local dispara, igual em host e peer" (decisão explícita do usuário, §1) fica sem lastro verificável — um leitor futuro (ou o `/code-review`) não teria como confirmar rapidamente se isso é fato ou suposição.

**Sugestão:** Adicionar as 2 referências já confirmadas nesta revisão a §1 e à evidência do check 2 de §9: `FikaPlayer.cs:173` (`player.IsYourPlayer = true;` — setado pra o personagem controlado localmente, seja host ou peer) e `ObservedPlayer.cs:217` (`player.IsYourPlayer = false;` — setado explicitamente pra peers remotos observados no seu client). Isso confirma exatamente o comportamento assumido, sem precisar de investigação nova — só falta a citação.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Sugestão aplicada — citações `FikaPlayer.cs:173` e `ObservedPlayer.cs:217` adicionadas em `008-atropelar-acorda-corpos-itens-02-spec-tech.md` §1 e na evidência do check 2 de §9.

## Histórico

| Data | Evento |
|---|---|
| 2026-09-21 | Review 01 criada — 0 bloqueadores, 2 importantes, 1 menor |
| 2026-09-21 | Aplicação dos 3 achados via decisão do usuário ("concordo") — IDs: PA-01-01, PA-01-02, PA-01-03. Spec técnica atualizada. |
