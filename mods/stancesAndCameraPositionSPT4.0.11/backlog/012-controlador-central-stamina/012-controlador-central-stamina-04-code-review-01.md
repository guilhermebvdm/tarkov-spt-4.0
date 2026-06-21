# 012 — Controlador central de stamina · Code Review 01

**Mod:** stancesAndCameraPositionSPT4.0.11
**As-built:** [012-controlador-central-stamina-05-asbuild.md](012-controlador-central-stamina-05-asbuild.md)
**Data:** 2026-06-21

> Análise crítica do código implementado, por **2 revisores independentes** (sub-agents de contexto limpo — anti-viés). IDs `CR-01-MM` permanentes.

## Resumo

> 🔴 Bloqueadores: 1 · 🟠 Fortes: 0 · 🟡 Médios: 1 · 🟢 Menores: 1 · Total: 3

Ambos os revisores convergiram nos mesmos 2 achados acionáveis (NRE + Reset). Demais dimensões (gate de perna, AP-02/Fika, neutralização, corner case, migração, eventos null-guard) confirmadas **corretas** pelos dois.

## Índice

| ID | Cat | Impacto | Título | Status |
|---|---|---|---|---|
| CR-01-01 | A | 🔴 | `Tick`/`Resolve` derefem `p` sem null-check (`p.HandsController`) | ✅ Aplicado |
| CR-01-02 | B | 🟡 | `StaminaController.Reset()` ausente no `OnRaidStart` | ✅ Aplicado |
| CR-01-03 | E | 🟢 | Gate `_PassiveStaminaSave` não documentado nas specs | ✅ Aplicado |

---

### CR-01-01 · A — Crítico · 🔴 Bloqueador

**`StaminaController.Tick` acessa `p.HandsController` sem checar `p == null`**

**Local:** [`StaminaController.cs:56-57`](../../modded-beta/StaminaController.cs)

**Problema:** `p?.Physical?.HandsStamina` é null-safe, mas a linha seguinte faz `p.HandsController` sem guard de `p`. Se `Singleton<GameWorld>.Instance.MainPlayer` for null (transição de cena/raid), lança `NRE`. Mitigado pelo `try/catch` do `Tick` (não crasha), **mas** o caminho de exceção **não** seta `ControllingHands = false` → o `Process` segue neutralizado e a stamina de braço congela por frames. `Resolve(p)` também assume `p != null`.

**Por que importa:** spam de `LogError` + stamina congelada na janela em que `MainPlayer` é null com a flag ainda `true`.

**Sugestão:** mover o guard de `p` para o topo:
```csharp
Player p = Singleton<GameWorld>.Instance?.MainPlayer;
if (p == null) { SetScenario(StaminaScenario.Inactive); ControllingHands = false; return; }
GClass774 hands = p.Physical?.HandsStamina;
```
e adicionar `if (p == null) return StaminaScenario.Inactive;` no topo de `Resolve` (defesa em camadas).

**Decisão:** `[x]` Aceitar sugestão · `[ ]` Aceitar com modificação · `[ ]` Rejeitar

---

### CR-01-02 · B — Bug latente · 🟡 Médio

**`StaminaController.Reset()` chamado só no `OnRaidEnd`, não no `OnRaidStart`**

**Local:** [`Patches/RaidLifecyclePatches.cs`](../../modded-beta/Patches/RaidLifecyclePatches.cs) (`GameWorldOnGameStartedPatch`)

**Problema:** a spec técnica (§9 check 1) prevê reset em start **e** end. Só o `OnRaidEnd` tem `StaminaController.Reset()`. Entre raid1→raid2, `Current`/`_prev`/`CurrentLabel`/`ControllingHands` carregam estado da raid anterior — o `_prev` desatualizado pode suprimir o primeiro log de transição e o debug mostra cenário incoerente por 1 frame.

**Por que importa:** invariante "estado limpo entre raids" (critério de aceite). `OnRaidEnd` já cobre a maioria, mas o reset no start é a garantia simétrica.

**Sugestão:** adicionar `StaminaController.Reset();` no `GameWorldOnGameStartedPatch` (junto de `StanceManager.OnRaidStart()`).

**Decisão:** `[x]` Aceitar sugestão · `[ ]` Aceitar com modificação · `[ ]` Rejeitar

---

### CR-01-03 · E — Legibilidade · 🟢 Menor

**O gate `_PassiveStaminaSave` no `Resolve` não está nas specs funcional/técnica**

**Local:** [`StaminaController.cs:99`](../../modded-beta/StaminaController.cs)

**Problema:** `Resolve` só entra em cenário Passive se `_PassiveStaminaSave.Value` — toggle introduzido no code-mod (reaproveitado do 06-fix-01) para não deixar a config órfã. É uma feature válida (off = passivo não mexe na stamina), mas a spec assumia que o passivo sempre captura. Já documentado em PROPRIEDADES + asbuild, mas não nas specs.

**Por que importa:** rastreabilidade — a próxima manutenção pode estranhar o gate.

**Sugestão:** nota de 1 linha na spec funcional (comportamento desejado) e técnica (§1) sobre o gate `_PassiveStaminaSave`.

**Decisão:** `[x]` Aceitar sugestão · `[ ]` Aceitar com modificação · `[ ]` Rejeitar

---

## Histórico

| Data | Evento |
|---|---|
| 2026-06-21 | Code review 01 (2 revisores independentes) — 1 🔴, 1 🟡, 1 🟢 |
| 2026-06-21 | Aplicados CR-01-01 (null-check `p` no Tick/Resolve), CR-01-02 (Reset no OnRaidStart), CR-01-03 (nota nas specs). Recompila 0 erros. |
