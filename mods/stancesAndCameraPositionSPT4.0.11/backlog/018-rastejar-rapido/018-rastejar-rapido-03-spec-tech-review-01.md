# 018 — Correr agachado e rastejar rápido (crouch-run + high-crawl) · Review Técnica 01

**Mod:** stancesAndCameraPositionSPT4.0.11
**Spec técnica revisada:** [018-rastejar-rapido-02-spec-tech.md](018-rastejar-rapido-02-spec-tech.md)
**Data:** 2026-09-09

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-01-MM`. Resolver até zerar bloqueadores antes de `/code-mod`.

**Memória consultada:** snapshot de 2026-08-30 (Sessão 16) · pendências que afetam: nenhuma nova.
**Gatilho desta rodada:** o usuário apontou um cenário concreto de bug antes mesmo de eu rodar a review — a mesma tecla usada para "correr" costuma ser reaproveitada (pelo bind do jogador) para "prender a respiração" enquanto mira parado. Se o boost de velocidade/sobretaxa de stamina reagir só a "tecla de sprint pressionada" sem confirmar movimento real, seguraria a respiração e de repente o personagem ganharia velocidade/dreno extra parado. Esse ponto foi investigado a fundo (PA-01-03 abaixo) e já é corrigido nesta rodada.

## Resumo

> 🔴 Bloqueadores: 3 · 🟠 Fortes: 0 · 🟡 Médios: 1 · 🟢 Menores: 0 · ✅ Resolvidos: 3 · Total: 4

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | AP-02 — Filtro Fika/local ausente | 🔴 Bloqueador | Patches reagem a bots e peers Fika, não só ao jogador local | ✅ Resolvido 2026-09-09 |
| PA-01-02 | AP-01 — Lifecycle de raid | 🔴 Bloqueador | Flags estáticas sem reset entre raids | ✅ Resolvido 2026-09-09 |
| PA-01-03 | C — Erro de lógica | 🔴 Bloqueador | Boost/sobretaxa não re-verificam movimento real a cada frame — vulnerável ao cenário "segurar sprint parado pra prender a respiração" | ✅ Resolvido 2026-09-09 |
| PA-01-04 | B — Edge case | 🟡 Médio | Incerteza sobre se `EnableSprint` é rechamado ao começar a mover com a tecla já segurada (mecanismo `Bool_1`/latch) | Pendente |

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

### PA-01-01 · AP-02 — Filtro Fika/local ausente · 🔴 Bloqueador · ✅ Resolvido em 2026-09-09

**Patches reagem a bots e jogadores observados em Fika, não só ao jogador local**

**Problema:** Os 4 stubs da §5 (`CrouchRunEnableSprintPatch`, `CrouchRunMaxSpeedPatch`, `ProneRunObservePatch`, `ProneRunMaxSpeedPatch`) interceptam `RunStateClass.EnableSprint`, `ProneMoveStateClass.EnableSprint` e `MovementContext.MaxSpeed` sem checar se o `MovementContext`/`Player` dono da chamada é o jogador local. A própria spec técnica já tinha auto-marcado isso como 🔴 pendente no check 2 da §9.

**Por que importa:** Sem o filtro, bots e outros jogadores num raid Fika ganhariam a mesma supressão de stand-up e o mesmo boost de velocidade — exatamente o bug já catalogado como AP-02 neste repo (item 002, CR-01-01: snap de stance disparando com o tiro de outro jogador).

**Sugestão:** Adicionar guarda `___MovementContext.Player.IsYourPlayer` (mesmo padrão usado em `ApplyComplexRotationPatch.cs:209`) logo no início dos 4 Prefixes/Postfixes, retornando cedo (`return true` nos Prefix, `return` nos Postfix sem tocar `__result`) quando `!IsYourPlayer`. Precisa confirmar o nome exato do campo que expõe o `Player` a partir de `MovementContext` (candidato: `MovementContext._player`, privado — usar `Traverse.Create(__instance).Field<Player>("_player").Value`, mesmo padrão já usado em `ApplyComplexRotationPatch.cs:207` para obter `Player` a partir do `FirearmController`).

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

**Resolução:** Aplicado nos 4 stubs da §5 (ver spec técnica atualizada) — guarda `IsYourPlayer` via `Traverse.Create(__instance).Field<Player>("_player").Value` logo após resolver o `MovementContext`, antes de qualquer outra lógica.

---

### PA-01-02 · AP-01 — Lifecycle de raid · 🔴 Bloqueador · ✅ Resolvido em 2026-09-09

**Flags estáticas (`Active`/`SprintKeyHeld`/`Held`) sem reset entre raids**

**Problema:** `CrouchRunEnableSprintPatch.Active` e `ProneRunObservePatch.Held` são campos estáticos que só são escritos dentro dos próprios Prefix/Postfix — nenhum hook de início/fim de raid os zera. Se uma raid terminar com a flag em `true` (jogador saiu segurando sprint agachado), o próximo `MaxSpeed` lido antes do primeiro `EnableSprint` da raid nova herdaria o valor antigo.

**Por que importa:** Mesma classe de bug do AP-01 (item 001 do stances, 3 ocorrências 🔴 nas reviews técnicas) — estado "fantasma" entre raids.

**Sugestão:** Adicionar um método estático `ResetState()` em cada uma das duas classes de patch (zerando `SprintKeyHeld`/`Held` para `false`), chamado a partir do mesmo hook que já reseta os outros estados estáticos do mod (`StanceManager.ResetState`, citado em `ApplyComplexRotationPatch.ResetSpeedTracker()`/`ResetMetrics()`/`ResetWaypoint()`).

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

**Resolução:** Adicionado `ResetState()` em `CrouchRunEnableSprintPatch` e `ProneRunObservePatch` (ver stubs atualizados na spec técnica) + item novo no checklist §8 lembrando de conectar ao hook de reset de raid existente.

---

### PA-01-03 · C — Erro de lógica · 🔴 Bloqueador · ✅ Resolvido em 2026-09-09

**Boost de velocidade e sobretaxa de stamina não re-verificam movimento real a cada frame — vulnerável a "segurar sprint parado" (ex.: prender a respiração)**

**Problema:** Nos stubs originais, `CrouchRunMaxSpeedPatch.Postfix` e `ProneRunMaxSpeedPatch.Postfix` só checam a flag `Active`/`Held` (atualizada dentro do Prefix/Postfix de `EnableSprint`, ou seja, só nos EVENTOS de apertar/soltar a tecla) — nenhum dos dois reconfirma `MovementContext.MovementDirection` no momento em que o boost é de fato aplicado (`MaxSpeed` é lido TODO FRAME por `UpdateCharacterControllerSpeedLimit`, [MovementContext.cs:4173-4182](../../../../references/eft-decompiled/Assembly-CSharp/EFT/MovementContext.cs#L4173-L4182), independente de quando `EnableSprint` foi chamado pela última vez). Cenário concreto trazido pelo usuário: o jogador está parado mirando (ADS) e segura a MESMA tecla configurada para sprint — mas nesse contexto ela serve pra "prender a respiração", não pra correr. Se `SprintKeyHeld` ficar `true` (porque a tecla física está pressionada) e o boost não reconfirmar `MovementDirection`, o personagem ganharia velocidade extra/dreno de stamina extra **parado**, sem ter apertado WASD.

**Por que importa:** É um bug de gameplay visível e incômodo — o jogador tentando ficar parado e quieto (prendendo a respiração pra estabilizar a mira) de repente vê a barra de stamina cair ou sente o personagem "querer" se mover. Também é o motivo pelo qual o design original com uma flag só event-driven é frágil: `EnableSprint` só é chamado nas transições de tecla (down/up), não a cada frame — então qualquer suposição de que a flag reflete o estado "agora" é falsa enquanto a tecla continua pressionada sem soltar.

**Sugestão:** Separar os dois conceitos que a spec técnica original misturou numa única flag:
1. **`SprintKeyHeld`** (renomear de `Active`/`Held`): só significa "o jogo está pedindo sprint agora" — continua sendo atualizado no Prefix/Postfix de `EnableSprint`, eventando corretamente em down/up.
2. **A condição de boost/sobretaxa, recalculada em TODO frame** dentro dos próprios Postfix de `MaxSpeed` (e futuramente, dentro do tick que ativa/desativa a sobretaxa de stamina): `featureOn && SprintKeyHeld && <pose certa> && __instance.MovementDirection.sqrMagnitude > 0.0001f`. Ler `MovementDirection` **direto do `__instance`** (o próprio `MovementContext` recebido no Postfix de `MaxSpeed`), nunca cacheado — isso garante que soltar o WASD (sem soltar a tecla de sprint) desliga o boost no MESMO frame, e resolve de quebra a incerteza do PA-01-04 (não importa quando `EnableSprint` foi chamado por último, o Postfix sempre olha o estado de movimento atual).

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

**Resolução:** Stubs reescritos na spec técnica (§5): `Active`→`SprintKeyHeld` (crouch) e `Held`→`SprintKeyHeld` (prone), e os dois Postfix de `MaxSpeed` agora recomputam `MovementDirection.sqrMagnitude > 0.0001f` a cada chamada, direto do `__instance`. Adicionada nota explícita de que a (futura) ativação/desativação da sobretaxa de stamina precisa usar a MESMA condição recalculada por frame — nunca só a flag de tecla.

---

### PA-01-04 · B — Edge case · 🟡 Médio

**Incerteza sobre se `EnableSprint` é rechamado ao começar a mover com a tecla de sprint já segurada (mecanismo `Bool_1` em `RunStateClass`)**

**Problema:** O método original de `RunStateClass.EnableSprint` ([RunStateClass.cs:401-420](../../../../references/eft-decompiled/Assembly-CSharp/RunStateClass.cs#L401-L420)) tem um branch `else if (!isToggle) { Bool_1 = enabled; }` para quando `MovementDirection.y <= 0.1f` — sugerindo que existe um mecanismo de "latch": segurar sprint parado marca a intenção (`Bool_1`), e alguma lógica não localizada nesta investigação (provavelmente em `ManualAnimatorMoveUpdate` ou equivalente) verifica esse latch quando o jogador começa a andar, sem que `EnableSprint` seja chamado de novo. Se isso for verdade, o `Prefix` de `CrouchRunEnableSprintPatch` **nunca rodaria** nesse caminho específico (ele só existe em cima do método `EnableSprint`), e o `SetPoseLevel(1f)` do latch levantaria o personagem sem o mod conseguir interceptar.

**Por que importa:** Se confirmado, o crouch-run falharia especificamente no fluxo "segura sprint parado, depois anda pra frente" (em vez de "anda pra frente, depois segura sprint") — um jeito plausível e comum de jogar.

**Sugestão:** Validar em `/code-mod`/in-game especificamente esse fluxo (segurar sprint primeiro, depois apertar W). Se o personagem levantar mesmo com o Prefix aplicado, o `Bool_1`/latch precisa de um segundo ponto de patch (provavelmente em `RunStateClass.ManualAnimatorMoveUpdate`, não lido nesta investigação). Isso não muda a correção do PA-01-03 (a condição de boost recalculada por frame continua correta e necessária de qualquer forma) — é um ponto de patch ADICIONAL, só se a validação in-game confirmar a lacuna.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

---

## Histórico

| Data | Evento |
|---|---|
| 2026-09-09 | Review 01 criada via `/review-technical-spec`, motivada por um cenário de bug descrito pelo usuário (tecla de sprint compartilhada com "prender a respiração" parado). 3 de 4 pontos já resolvidos na mesma rodada. |
