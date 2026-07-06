# 051 — Levers da zona stances · Spec Técnica

**Mod:** CustomClasses (+ toque coordenado no `mods/stancesAndCameraPositionSPT4.0.11` — liberação do usuário 2026-07-04)
**Spec funcional:** [051-stances-zone-levers-01-spec.md](051-stances-zone-levers-01-spec.md)
**Criado:** 2026-07-04

> Fontes: código dos DOIS mods (🥈 na hierarquia — ambos nossos). Nenhum ponto novo no Assembly do EFT: o stances
> já é a autoridade da stamina de braço; este item compõe COM ele.

## 1. Estratégia

**Hook de composição no dreno do `StaminaController` (stances) + provider por soft-detect (CustomClasses).**

O Tick do stances calcula, por frame: `delta = StanceManager.CachedAimDrainRate × (mult − 1) × dt` e escreve
`hands.Current` ([StaminaController.cs:70-77](../../../stancesAndCameraPositionSPT4.0.11/modded/StaminaController.cs#L70)).
O sinal de `delta` distingue DRENO (negativo) de RECUPERAÇÃO (positivo) — o fator de classe entra **só no ramo
negativo**, o que satisfaz "só o dreno, nunca a recuperação" por construção, agnóstico a cenário (hold-breath
incluso) e ao sinal do rate.

- **Stances (1 campo + 2 linhas no Tick):** `public static Func<float>? ExternalHandsDrainMult;` — aplicado como
  `if (delta < 0f && ExternalHandsDrainMult != null) delta *= Mathf.Clamp(ExternalHandsDrainMult(), 0f, 2f);`
  (clamp defensivo; provider null/ausente = comportamento byte-idêntico ao atual — critério de regressão zero).
- **CustomClasses (provider):** classe nova `StancesArmStaminaBridge` — resolve
  `AccessTools.TypeByName("CameraRotationMod.StaminaController")` + campo `ExternalHandsDrainMult` via reflection
  (LAZY, 1×; zero tipos do stances no IL — mesmo padrão FIKA/SAIN) e seta o delegate `Factor()`:
  - Caçador (`IsLocalClass("Hunter")`) **mirando** (`p.ProceduralWeaponAnimation.IsAiming` — mesmo sinal que o
    `Resolve()` do stances usa) → fator F12 (default **0.65**).
  - Tanque (`IsLocalClass("Tank")`) com **arma pesada em mãos** (`HeavyWeapon.InHand(p)` — reuso do gate do
    Bunker, [ClassWeaponPatches.cs:202-221](../../modded/Client/Patches/ClassWeaponPatches.cs#L202)) → fator F12
    (default **0**).
  - Senão → `1f` (neutro).

Alternativas descartadas: patch Harmony no Tick do stances a partir do CustomClasses (acoplamento hard + frágil a
rename); mexer nos `Multipliers[]` do stances (sobrescreveria config do usuário — viola "F12 dele soberano").

## 2. Pontos de mudança (nenhum patch novo no EFT)

| Onde | Tipo | Motivo |
|---|---|---|
| `stances/modded/StaminaController.cs` (Tick, pós-cálculo do delta) | campo público + 2 linhas | ponto único de composição do dreno |
| `CustomClasses/modded/Client/StancesArmStaminaBridge.cs` | CRIAR | provider soft-detect + `Factor()` por classe/contexto |
| `CustomClasses/modded/Client/Plugin.cs` | 1 linha | `StancesArmStaminaBridge.TryAttach()` no Awake (idempotente; re-tenta lazy se falhar) |
| `CustomClasses/modded/Client/PerksConfig.cs` + `PROPRIEDADES.md` | F12 §3 | toggles/fatores |
| `CustomClasses/modded/Client/PerksCatalog.cs` | 2 linhas | remover `pending: true` de Steady Arms (iron_lungs) e Tireless Arms (bunker) |

## 3. Novas propriedades F12 (CustomClasses)

| Seção | Nome (EN) | Tipo | Padrão | Faixa | Tooltip (pt-BR) |
|---|---|---|---|---|---|
| `Perks — Hunter` | `Steady Arms — Enabled` | bool | `true` | — | Caçador: braço cansa mais devagar ao mirar (compõe com o stances mod). |
| `Perks — Hunter` | `Steady Arms — ADS arm drain mult` | float | `0.65` | 0.2–1 | Multiplicador do dreno de braço do Caçador em ADS (0.65 = 35% mais lento). Requer o stances mod. |
| `Perks — Tank` | `Tireless Arms — Enabled` | bool | `true` | — | Tanque: braço não cansa segurando arma pesada (compõe com o stances mod). |
| `Perks — Tank` | `Tireless Arms — Heavy arm drain mult` | float | `0` | 0–1 | Multiplicador do dreno de braço do Tanque com LMG/HMG/GL em mãos (0 = não drena). Requer o stances mod. |

## 4. Fluxo de dados

```
[stances Tick, 1×/frame] Resolve(cenário) → mult do F12 do stances → delta = rate×(mult−1)×dt
        ↓ delta < 0 (DRENO)?
[hook]  delta *= Clamp(ExternalHandsDrainMult(), 0, 2)
        ↑ delegate setado pelo CustomClasses (soft-detect, lazy)
[CustomClasses Factor()] classe local + contexto (IsAiming / HeavyWeapon.InHand) → 0.65 / 0 / 1
        ↓
hands.Current = Clamp(prev + delta, 0, capacidade)  (eventos nativos re-disparados — inalterado)
```

## 5. Riscos e dependências

- **Coordenação de sessões:** o stances tem HANDOFF com edits NÃO commitados no tree principal (outra sessão) —
  este item NÃO toca o HANDOFF do stances; registro vai na memória do stances (`memory/sessions.md`) + CustomClasses.
- **Deploy do stances:** build via `/compile-mod stancesAndCameraPositionSPT4.0.11`; instalação final é MANUAL em
  `BepInEx/plugins/RealisticMobility/` (memória `reference_stances_canonical_build`) — conferir destino no deploy.
- **Delegate cross-assembly:** `Func<float>` é tipo do mscorlib — atribuição via reflection é estável; provider
  com try/catch interno (exceção no Factor não pode matar o Tick do stances → o hook clampa e o Tick já tem try).
- **AP-08:** fator lido por FRAME a partir do estado atual (classe/arma/mira) — sem cache; troca de arma/mira
  atualiza no tick seguinte (corner da 01-spec).
- **AP-01:** delegate estático sobrevive entre raids — correto (é função pura de estado atual); nada raid-scoped.
- **Regressão zero sem CustomClasses:** hook null → caminho idêntico ao atual (1 branch por frame de dreno).

## 6. Checklist de implementação

- [ ] stances: campo `ExternalHandsDrainMult` + composição no ramo `delta < 0` do Tick.
- [ ] CustomClasses: `StancesArmStaminaBridge` (soft-detect lazy + `Factor()`; 1 log info no attach, 1 warn se ausente).
- [ ] CustomClasses: F12 §3 + `PROPRIEDADES.md` + `Enable/TryAttach` no Plugin.
- [ ] CustomClasses: remover `pending` dos 2 cards (PerksCatalog).
- [ ] Compile dos DOIS mods 0/0; deploy do stances em `RealisticMobility/` (manual).
- [ ] Memória do stances: entrada curta registrando o hook (coordenação).

## 7. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle raid — AP-01 | ✅ | Sem estado raid-scoped: delegate estático puro; Tick do stances já gerencia o próprio lifecycle (`IsActiveContext`). |
| 2 | Filtro MainPlayer/Fika — AP-02 | ✅ | O Tick do stances já é MainPlayer-only (StaminaController.cs:53); `Factor()` lê a classe LOCAL (`IsLocalClass`). |
| 3 | Alvos ofuscados/virtuais — AP-03 | N/A | Nenhum alvo do EFT novo; composição entre os nossos 2 mods. |
| 4 | API canônica — AP-04 | ✅ | Nenhuma escrita nova de estado do EFT — o stances continua o único escritor de `hands.Current`. |
| 5 | Estado entre raids | ✅ | Fator re-avaliado por frame (§5); sem cache. |
| 6 | ConfigEntry — AP-05 | ✅ | §3: defaults = valores do catálogo (0.65/0); estado neutro = Enabled=false → Factor 1. |
| 7 | Reentry — AP-07 | ✅ | Sem re-invocação; delegate chamado 1×/frame pelo Tick. |
| 8 | Caches pós-troca — AP-08 | ✅ | §5 — leitura por frame do contexto atual. |

## Histórico

| Data | Evento |
|---|---|
| 2026-07-04 | Spec técnica criada (mecanismo evidenciado no Tick do stances; hook no ramo de dreno) |
