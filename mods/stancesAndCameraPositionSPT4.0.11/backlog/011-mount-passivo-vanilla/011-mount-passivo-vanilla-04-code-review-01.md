# 011 — Mount passivo sobre o vanilla · Code Review 01

**Mod:** stancesAndCameraPositionSPT4.0.11
**As-built:** [011-mount-passivo-vanilla-05-asbuild.md](011-mount-passivo-vanilla-05-asbuild.md)
**Data:** 2026-06-21

> Análise crítica do código implementado por `/code-mod`. IDs `CR-01-MM` permanentes. Resolver 🔴 antes de fechar.

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 1 · 🟡 Médios: 1 · 🟢 Menores: 1 · Total: 3

Memória: snapshot Sessão 5 · pendência P-5.2 (este item). Sem 🔴 — o item pode fechar após validação in-game; o 🟠 (AP-02 em Fika) é recomendado antes.

## Índice

| ID | Cat | Impacto | Título | Status |
|---|---|---|---|---|
| CR-01-01 | D/B | 🟠 | Buffs aplicam a qualquer player (AP-02) — falta gate MainPlayer | ✅ Aplicado |
| CR-01-02 | B | 🟡 | Patches de buff sem `try/catch` | ✅ Aplicado |
| CR-01-03 | F | 🟢 | `FieldInfo.GetValue` do `_player` por frame na detecção | Deferido (dívida menor) |

---

### CR-01-01 · D/B — AP-02 · 🟠 Forte

**Buffs de recoil/sway aplicam a qualquer player; o guard `IsBracing` é global (do local)**

**Local:** [`PassiveMountBuffPatches.cs`](../../modded/Patches/PassiveMountBuffPatches.cs)

**Problema:** `PassiveSwayPatch` faz Postfix em `ProceduralWeaponAnimation.ProcessEffectors`, que roda para **todas** as PWA processadas (peers/bots inclusos). O único guard é `PassiveMountState.IsBracing` — estado **global** que reflete o jogador local. Logo, enquanto o local está apoiado, o `Breath.Intensity` de um **peer** sendo processado no mesmo frame também é multiplicado:
```csharp
if (Plugin._EnablePassiveMount.Value && PassiveMountState.IsBracing && __instance.Breath != null)
    __instance.Breath.Intensity *= Plugin._PassiveSwayMultiplier.Value;   // __instance pode ser um peer
```
`PassiveRecoilPatch` (`NewRecoilShotEffect.AddRecoilForce`) tem o mesmo padrão; confirmar se `AddRecoilForce` roda para peers em Fika (recoil costuma ser 1ª pessoa/local, mas não garantido).

**Por que importa:** viola o AC Fika da spec funcional ("aplica somente ao seu jogador — nunca a bots nem outros players"). Em coop, reduz sway/recoil de peers durante a janela em que o local braça. (Em SP não há efeito.)

**Sugestão:** adicionar gate de MainPlayer no `PassiveSwayPatch`:
```csharp
if (__instance != Singleton<GameWorld>.Instance?.MainPlayer?.ProceduralWeaponAnimation) return;
```
e, no `PassiveRecoilPatch`, confirmar o escopo; se rodar para peers, derivar o player do `__instance` (ou gate equivalente) e aplicar só ao local.

**Decisão:**
- `[x]` Aceitar sugestão (gate MainPlayer no sway; confirmar/gate no recoil)
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir): _________________

---

### CR-01-02 · B — Robustez · 🟡 Médio

**`PassiveRecoilPatch`/`PassiveSwayPatch` sem `try/catch`**

**Local:** [`PassiveMountBuffPatches.cs`](../../modded/Patches/PassiveMountBuffPatches.cs)

**Problema:** os dois patches rodam no hot path de animação/recoil e não envolvem o corpo em `try/catch` (o `PassiveMountDetectPatch` envolve). Uma exceção inesperada (ex.: `Breath`/`MainPlayer` em estado atípico) propagaria do patch.

**Por que importa:** checklist `spt`/`csharp` exige patch body em `try/catch` + log; exceção no hot path pode afetar o frame.

**Sugestão:** envolver cada corpo em `try { ... } catch (Exception ex) { Plugin.Logger.LogError(...) }` (com throttle se necessário, dado o hot path).

**Decisão:**
- `[x]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir): _________________

---

### CR-01-03 · F — Performance · 🟢 Menor

**`_playerField.GetValue(__instance)` por frame na detecção**

**Local:** [`PassiveMountDetectPatch.cs`](../../modded/Patches/PassiveMountDetectPatch.cs)

**Problema:** o Postfix de `method_11` resolve o `_player` por `FieldInfo.GetValue` a cada chamada (com boxing). `FieldInfo` já é cacheado, mas a chamada roda por frame com a arma em mãos.

**Por que importa:** custo trivial isolado; soma no hot path. (Padrão herdado do item 004.)

**Sugestão:** opcional — usar `AccessTools.FieldRefAccess<Player.FirearmController, Player>("_player")` (delegate compilado) cacheado, ou verificar se há acessor público de `Player` no `FirearmController`. Resolve-se junto a uma futura passada de perf.

**Decisão:**
- `[ ]` Pendente
- `[x]` Rejeitar (deferir como dívida menor de perf)

---

## Histórico

| Data | Evento |
|---|---|
| 2026-06-21 | Code review 01 criada via `/code-review` — 0 🔴, 1 🟠, 1 🟡, 1 🟢 |
| 2026-06-21 | Aplicados CR-01-01 (gate `__instance == MainPlayer.PWA` no `PassiveSwayPatch`; `PassiveRecoilPatch` documentado como recoil 1ª-pessoa/local) e CR-01-02 (`try/catch` nos dois buffs). CR-01-03 deferido. Recompila 0 erros. |
