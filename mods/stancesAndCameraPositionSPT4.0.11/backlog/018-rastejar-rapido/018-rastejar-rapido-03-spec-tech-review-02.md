# 018 — Correr agachado e rastejar rápido (crouch-run + high-crawl) · Review Técnica 02

**Mod:** stancesAndCameraPositionSPT4.0.11
**Spec técnica revisada:** [018-rastejar-rapido-02-spec-tech.md](018-rastejar-rapido-02-spec-tech.md)
**Data:** 2026-09-09

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-02-MM`. Resolver até zerar bloqueadores antes de `/code-mod`.

**Memória consultada:** snapshot de 2026-08-30 (Sessão 16) · pendências que afetam: nenhuma nova.
**Escopo desta rodada:** a review 01 (`018-rastejar-rapido-03-spec-tech-review-01.md`) zerou os 3 bloqueadores originais; esta rodada foca no que foi adicionado depois (§1.5 rampa de velocidade, §1.6 animação de sprint, §1.7 piso de postura) e cruza com código real do mod (`item 005`, `POSE_CHANGING_SPEED`) para achar interações não documentadas. PA-01-04 (🟡, review 01) segue pendente — só valida in-game, sem mudança de código proposta.
**✅ PA-01-04 (review 01) segue aberto** — nenhuma informação nova nesta rodada; continua no checklist §8 como validação in-game.

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 1 · 🟡 Médios: 1 · 🟢 Menores: 0 · ✅ Resolvidos: 2 · Total: 2

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-02-01 | C — Erro de lógica | 🟠 Forte | Piso de postura usa `force: true` — produz um "pop" visual em vez de transição suave | ✅ Resolvido 2026-09-09 |
| PA-02-02 | D — Arquitetura | 🟡 Médio | Side-effects (`SetPoseLevel`, `PlayerAnimatorEnableSprint`) dentro do Postfix de uma property getter | ✅ Resolvido 2026-09-09 (documentado, não refatorado) |

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

### PA-02-01 · C — Erro de lógica · 🟠 Forte · ✅ Resolvido em 2026-09-09

**Piso de postura (§1.7) usa `SetPoseLevel(..., force: true)` — isso faz a postura "teleportar" em vez de subir/descer suavemente**

**Problema:** O stub de `CrouchRunMaxSpeedPatch` (§5) chama `SetPoseLevel(target, force: true)` tanto para subir a postura ao entrar no piso quanto para restaurar ao sair. Olhando `SetPoseLevel` ([MovementContext.cs:2139-2174](../../../../references/eft-decompiled/Assembly-CSharp/EFT/MovementContext.cs#L2139-L2174)): quando `force` é `true`, a linha `if (force) { SmoothedPoseLevel = PoseLevel_1; }` (linha 2171) faz o valor SUAVIZADO (o que realmente anima a pose visível, via `PlayerAnimatorSetPoseLevel(SmoothedPoseLevel)`, linha 822) saltar direto pro valor final, **sem passar pela interpolação normal**. Cruzando com o código REAL do mod (não o spec-tech desatualizado do item 005): `Plugin.ApplyMovementSpeeds()` ([Plugin.cs:1431-1439](../../modded/Plugin.cs#L1431-L1439)) já ajusta `EFTHardSettings.Instance.POSE_CHANGING_SPEED` — a taxa que rege exatamente essa suavização — via o slider `Crouch Speed Multiplier` do item 005. Ou seja, o mod já tem a peça pra fazer essa transição suave e configurável, e o stub atual a ignora com `force: true`.

**Por que importa:** Diferente da perda de velocidade (um número que fica invisível ao "cortar" — só se sente a desaceleração), uma mudança de `PoseLevel` afeta a ALTURA do personagem/câmera diretamente. Um `force: true` faria o personagem "pular" de agachado-baixo pra agachado-médio (e de volta) instantaneamente — um pop visual bem mais perceptível e feio do que qualquer corte de velocidade, e destoa exatamente do cuidado que esta spec já teve com transições graduais (§1.5).

**Sugestão:** Trocar `force: true` por `force: false` nas duas chamadas de `SetPoseLevel` do piso de postura (subir E restaurar). Conferido que isso não introduz risco novo: o único efeito colateral do branch `!force` em `SetPoseLevel` (linha 2155-2163, disparar `Skills.PushUp.Complete`) só dispara ao alcançar `_player.Physical.MaxPoseLevel` (praticamente em pé) — irrelevante para um alvo intermediário como `0.5`. A checagem de colisão (`CanStandAt`) roda igual com `force` em qualquer valor, então a segurança contra teto baixo não muda. Com `force: false`, a transição herda automaticamente o `POSE_CHANGING_SPEED` (já configurável pelo usuário via `Crouch Speed Multiplier` do item 005) — sem precisar de nenhuma config nova.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

**Resolução:** Stub de `CrouchRunMaxSpeedPatch` (§5) atualizado — as duas chamadas de `SetPoseLevel` agora usam `force: false`. Nota adicionada em §1.7 documentando a dependência nova com `POSE_CHANGING_SPEED`/item 005.

---

### PA-02-02 · D — Arquitetura · 🟡 Médio · ✅ Resolvido em 2026-09-09 (documentado, não refatorado)

**Side-effects (`SetPoseLevel`, `PlayerAnimatorEnableSprint`) dentro do Postfix de uma property getter (`MaxSpeed`)**

**Problema:** `CrouchRunMaxSpeedPatch.Postfix` patcheia o GETTER de `MovementContext.MaxSpeed` — uma property que, por convenção, deveria ser um cálculo puro (só leitura). O stub atual usa esse ponto pra, além de multiplicar `__result`, também **disparar efeitos colaterais**: ligar/desligar a animação de sprint (§1.6) e mudar `PoseLevel` via `SetPoseLevel` (§1.7). Isso funciona hoje graças a uma coincidência: o guard `Time.frameCount != _lastFrame` (pensado originalmente só pra não avançar a rampa mais de uma vez por frame) também acaba protegendo contra reentrância caso `SetPoseLevel`/eventos do Animator disparem uma leitura recursiva de `MaxSpeed` no mesmo frame — mas essa proteção é um efeito colateral do design, não algo garantido por contrato.

**Por que importa:** Se no futuro `MaxSpeed` passar a ser lido de um contexto onde `Time.frameCount` não avança da forma esperada (ex.: chamado de uma coroutine, de um editor tool, ou em `FixedUpdate` vs `Update` misturados), a proteção contra reentrância deixa de valer silenciosamente. Um Postfix num getter que muda estado do jogo (pose, animator) é uma arquitetura mais frágil a mudanças futuras do Assembly do que um Postfix num método de tick genuíno.

**Por que não bloqueia:** Não há nenhum bug demonstrado — é uma preocupação de robustez a prazo, não uma falha observada ou dedutível com certeza nas leituras feitas. Refatorar agora (mover os efeitos colaterais para um novo Postfix em `RunStateClass.ManualAnimatorMoveUpdate`, um método de tick genuíno já mapeado em §1.6) adicionaria escopo/risco a uma spec que já passou por 3 rodadas de mudança nesta sessão.

**Sugestão:** Documentar a dependência explicitamente (feito — ver Resolução) e **não bloquear `/code-mod`** por isso. Se `/code-review` (depois do código pronto) achar sinais reais de reentrância ou comportamento inconsistente, é o momento certo de migrar os efeitos colaterais para `RunStateClass.ManualAnimatorMoveUpdate` — nesse ponto já com código real pra comparar antes/depois, em vez de uma refatoração especulativa agora.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

**Resolução:** Nota de arquitetura adicionada na spec técnica (§1.5, junto ao guard de frame) explicando que a proteção contra reentrância é incidental ao guard de rampa, não uma garantia própria — para o `/code-review` reavaliar se aparecer sinal real de problema.

---

## Histórico

| Data | Evento |
|---|---|
| 2026-09-09 | Review 02 criada via `/review-technical-spec`, focada nas seções adicionadas após a review 01 (§1.5-1.7). 2 pontos, ambos resolvidos na mesma rodada (1 fix de código, 1 documentação). |
