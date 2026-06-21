# 012 — Controlador central de stamina · Spec Tech Review 01

**Mod:** stancesAndCameraPositionSPT4.0.11
**Spec técnica:** [012-controlador-central-stamina-02-spec-tech.md](012-controlador-central-stamina-02-spec-tech.md)
**Data:** 2026-06-21

> Análise crítica adversarial da spec técnica. IDs `PA-01-MM` permanentes.

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 3 · 🟢 Menores: 1 · Total: 4

Memória: snapshot Sessão 5 (2026-06-21) · pendências que afetam: P-5.3 (reset de estado estático / try-catch — a spec cobre via `StaminaController.Reset()` + `try/catch` nos Prefixes e no `Tick`). Sem 🔴 — pode iniciar `/code-mod`; os 🟡 são resolvidos durante.

## Índice

| ID | Cat | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | B | 🟡 | Ordem `Tick` × `Process` no frame não garantida | ✅ Resolvido (spec §7 + comentário no code-mod) |
| PA-01-02 | A | 🟡 | `FieldInfo` dos eventos pode resolver `null` (sem guarda) | ✅ Resolvido (null-guard no stub §5) |
| PA-01-03 | B | 🟡 | Buffs/skills de stamina de braço ignorados ao neutralizar `Process` | ✅ Resolvido (aceito como assunção, spec §7) |
| PA-01-04 | A | 🟢 | `StanceStaminaState.Multiplier` fica órfão | ✅ Resolvido (limpar no code-mod, spec §7) |

---

### PA-01-01 · B — Edge Case · 🟡 Importante

**Ordem `Tick` (Plugin.Update) × `Process` (Player.Update) no frame não é garantida**

**Problema:** o `Prefix` de `GClass774.Process` decide neutralizar lendo `StaminaController.ControllingHands`, que é setado no `Tick` (`Plugin.Update`). A ordem entre `Plugin.Update` e `Player.Update`/`ComplexUpdate` (Player.cs:1111) não é determinística no Unity sem Script Execution Order — no 1º frame de uma transição, o `Process` pode rodar com o `ControllingHands` do frame anterior.

**Por que importa:** ≤1 frame de defasagem na borda (vanilla roda 1 frame, ou o controller pula 1 frame). Imperceptível, mas convém estar consciente para não diagnosticar como bug.

**Sugestão:** aceitar a defasagem (o gate por flag é seguro — nunca neutraliza a perna nem corrompe estado) e **documentar** no comentário do Prefix. Alternativa, se incomodar: no Prefix, em vez de só o flag, re-derivar `StanceManager.IsActiveContext() && tem-arma` (mesma decisão do `Tick`, sem depender da ordem) — custo de algumas leituras a mais por frame.

**Decisão:** `[x]` Aceitar sugestão (aceitar defasagem + comentar) · `[ ]` Caminho alternativo

---

### PA-01-02 · A — Gap · 🟡 Importante

**`AccessTools.Field(typeof(GClass774), "action_1"/"action_3")` pode retornar `null` — `Tick` lançaria `NRE` ao cruzar o threshold**

**Problema:** os backing fields `action_1`/`action_3` são `[CompilerGenerated]` (GClass774.cs:47,53). Se o nome variar numa versão futura do EFT, `_onThreshold`/`_onValueChanged` resolvem `null`, e `_onThreshold.GetValue(hands)` lança `NRE` (capturada pelo `try/catch` do `Tick`, mas com log a cada cruzamento de 15 de stamina e **sem** o tremor/barra).

**Por que importa:** robustez — degradar limpo é melhor que spam de erro + perda silenciosa do feedback de exaustão.

**Sugestão:** no stub do `StaminaController`, guardar os disparos: `if (_onThreshold != null && ...)`. Validar não-null uma vez no init (ou no primeiro `Tick`) e logar **uma** vez se faltarem, seguindo sem o evento. Adicionar o guarda ao §5.

**Decisão:** `[x]` Aceitar sugestão · `[ ]` Caminho alternativo

---

### PA-01-03 · B — Edge Case · 🟡 Importante

**`BuffRestoration`/`Overuse`/`DisableRestoration` do vanilla deixam de atuar no braço ao neutralizar `Process`**

**Problema:** o `Process` vanilla (GClass774.cs:351) aplica `BuffRestoration` (buffs de restauração de stamina — stims como SJ6/Obdolbos, skill Endurance) além da `SelfRestoration`. Neutralizando o `Process` e escrevendo `Current` só pelo multiplicador, esses **buffs/skills não afetam mais a stamina de braço**.

**Por que importa:** muda comportamento — um stim/skill que o jogador espera que recupere o braço mais rápido não terá efeito no braço (a perna continua normal). Pode surpreender.

**Sugestão:** **aceitar** como consequência intencional do "controle 100%" pedido pela spec funcional (a régua passa a ser o multiplicador por cenário) e **registrar na entrega/PROPRIEDADES** que stims/skills de stamina de braço não se somam ao controlador. Alternativa (mais trabalho): ler `hands.BuffRestoration` e somar ao delta quando o cenário recupera.

**Decisão:** `[x]` Aceitar sugestão (aceitar + documentar como assunção) · `[ ]` Caminho alternativo (somar BuffRestoration)

---

### PA-01-04 · A — Gap · 🟢 Menor

**`StanceStaminaState.Multiplier` fica sem leitor após aposentar `ShouldApplyStamina`**

**Problema:** o controller lê `Multipliers[]` diretamente; `ApplyStaminaStance` ainda seta `StanceStaminaState.Multiplier`, mas ninguém mais o lê (o `TickStanceStamina` é esvaziado). Sobra só `IsSuspendedByProne` (usado pelo speed-limit).

**Por que importa:** estado morto confunde a próxima manutenção (parece que alguém lê).

**Sugestão:** durante o `/code-mod`, remover `StanceStaminaState.Multiplier` e o `ShouldApplyStamina` (manter só `IsSuspendedByProne` + `Reset`), ou comentar que ficou só para o speed-limit. Não bloqueia.

**Decisão:** `[x]` Aceitar sugestão (limpar no code-mod) · `[ ]` Caminho alternativo

---

## Histórico

| Data | Evento |
|---|---|
| 2026-06-21 | Review 01 criada via `/review-technical-spec` — 0 🔴, 3 🟡, 1 🟢 |
