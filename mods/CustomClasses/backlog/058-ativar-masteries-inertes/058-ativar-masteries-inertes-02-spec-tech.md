# 058 — Ativar masteries inertes · Spec Técnica

**Mod:** CustomClasses
**Spec funcional:** [058-ativar-masteries-inertes-01-spec.md](058-ativar-masteries-inertes-01-spec.md)
**Recon:** [058-ativar-masteries-inertes-recon.md](058-ativar-masteries-inertes-recon.md)
**Criado:** 2026-07-03

> Fonte: recon (refs do decompile curado + confirmações). **Patch client nas 2 pernas** — server-globals descartado
> (sem binding, recon §2.2). **⚠️ O `/code-mod` desta spec está BLOQUEADO** até a validação prévia (V1/V2/V4 da spec
> funcional): os resultados definem o escopo real (quais skills recebem XP do mod, se precisa persistência server, se
> HMG/LMG/underbarrel são separáveis). Os stubs abaixo são o **plano condicional**.

## 1. Estratégia

Duas pernas, ambas **locais** (coop-safe):

- **Perna 1 — XP (nova):** Postfix em `Player.ExecuteShotSkill(Item weapon)` — o funil único do XP de weapon-skill, já no
  contexto "meu tiro acertou" (recon §2.1). Gate: `MainPlayer` local + `weapon.WeapClass` na categoria alvo. Credita
  `player.Skills.<Cat>.SetCurrent(Current + delta)`. **Só** para as categorias que **não** sobem no vanilla (V1).
- **Perna 2 — efeito (generaliza o 050):** os patches do 050 já tocam os pontos certos — recuo em
  `ProceduralWeaponAnimation.Shoot` (`ShootRecoilPatch`) e ergo em `FirearmController.TotalErgonomics`
  (`HeavyWeaponErgoPatch`). Adicionar uma camada que escala por **nível da skill** (`player.Skills.<Cat>.Level`),
  gateando por `WeapClass`. **Coexiste** com o Bunker (patches Harmony independentes no mesmo método — aditivo).

## 2. Pontos de patch

| Alvo | Tipo | Motivo | Bloqueio |
|---|---|---|---|
| `EFT.Player.ExecuteShotSkill(Item)` (`Player.cs:29934`) | **Postfix** | funil do XP ao acertar → creditar skill inerte | V1 (quais categorias) |
| `player.Skills.<SMG\|LMG\|HMG\|Launcher\|AttachedLauncher>` (`SkillManager.cs:1306-1320`) | escrita | `SetCurrent(Current+delta)` | V3 (propaga+persiste?) |
| `ProceduralWeaponAnimation.Shoot(ref float)` (`ClassWeaponPatches.cs:18`, molde 050) | Prefix (novo) | recuo × (1 − rec/lvl·Level) | V4 (categoria) |
| `FirearmController.TotalErgonomics` getter (`ClassWeaponPatches.cs:167`, molde 050) | Postfix (novo) | ergo × (1 + ergo/lvl·Level) | V4 |
| `HeavyWeapon.IsHeavy`/`WeapClass` (`ClassWeaponPatches.cs:201-221`) | reuso | detecção de categoria (estender p/ `smg`) | V4 (underbarrel, HMG≠LMG) |

## 3. Novas propriedades F12 (BepInEx)

| Seção | Nome (EN) | Tipo | Padrão | Faixa | Tooltip (pt-BR) |
|---|---|---|---|---|---|
| `Weapon Mastery` | `Weapon Mastery — Enabled` | bool | `true` | — | Ativa progressão + bônus das maestrias inertes (SMG/LMG/HMG/Launcher/underbarrel). |
| `Weapon Mastery` | `XP per hit` | float | `_a definir por paridade com as funcionais (V-tuning)_` | 0–50 | XP concedido por acerto de tiro. |
| `Weapon Mastery` | `Recoil bonus per level` | float | `0.004` | 0–0.02 | Redução de recuo por nível (paridade `WeaponSkillRecoilBonusPerLevel`). |
| `Weapon Mastery` | `Ergo bonus per level` | float | `0.002` | 0–0.02 | Aumento de ergo por nível (paridade com a curva vanilla). |

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `modded/Client/Patches/WeaponMasteryXpPatch.cs` | **CRIAR** | Perna 1 — Postfix em `ExecuteShotSkill`; gate `weapClass` + `MainPlayer`; `SetCurrent`. |
| `modded/Client/Patches/WeaponMasteryEffectPatches.cs` | **CRIAR** | Perna 2 — recuo/ergo escalando por nível (molde do 050), coexistindo com o Bunker. |
| `modded/Client/WeaponMastery.cs` | **CRIAR** | Helper: `weapClass → skill`, leitura de `Level`, gate de categoria (V4). |
| `modded/Client/PerksConfig.cs` + `Plugin.cs` + `PROPRIEDADES.md` | MODIFICAR | F12 da §3 + `Enable()`. |

## 5. Stubs de código (condicionais — dependem de V1/V4)

```csharp
// modded/Client/Patches/WeaponMasteryXpPatch.cs  (Perna 1)
[PatchPostfix]
private static void Postfix(Player __instance, Item weapon)   // ref: Player.cs:29934
{
    try
    {
        if (PerksConfig.WeaponMasteryEnabled?.Value != true) return;
        if (!ReferenceEquals(__instance, Singleton<GameWorld>.Instance?.MainPlayer)) return;   // AP-02
        var skill = WeaponMastery.SkillFor(__instance, weapon);   // null se categoria não-alvo OU já sobe no vanilla (V1)
        if (skill == null) return;
        skill.SetCurrent(skill.Current + (PerksConfig.MasteryXpPerHit?.Value ?? 0f));   // V3: confirmar propagação/persistência
    }
    catch (Exception ex) { Plugin.Log?.LogError($"[CustomClasses] (058) mastery xp: {ex.Message}"); }
}
```

```csharp
// modded/Client/Patches/WeaponMasteryEffectPatches.cs  (Perna 2 — molde do 050, escala por nível)
// Prefix ProceduralWeaponAnimation.Shoot(ref float str): str *= (1f - recPerLevel * level);
// Postfix FirearmController.TotalErgonomics getter: __result *= (1f + ergoPerLevel * level);
// gate por WeaponMastery.SkillFor(...).Level; coexiste com ShootRecoilPatch/HeavyWeaponErgoPatch (Bunker).
```

## 6. Fluxo de dados

```
[A] meu tiro acerta → ManageAggressor → Player.ExecuteShotSkill(weapon)   (Player.cs:29992/29934)
      ↓ Postfix (Perna 1)  gate: MainPlayer + weapClass ∈ alvo (V1)
[B] player.Skills.<Cat>.SetCurrent(+delta)   → sobe a barra (V3) → persiste? (V2)
      ⇒ Level ↑
[C] ao disparar/segurar a arma da categoria:
      Prefix Shoot → recuo × (1 − rec/lvl·Level)   |  Postfix TotalErgonomics → ergo × (1 + ergo/lvl·Level)
      (coexiste com Bunker flat do Tank)
```

## 7. Riscos e dependências (todos ligados à validação prévia)

- **V1 (XP duplo):** se SMG/LMG já sobem, `WeaponMastery.SkillFor` deve retornar null para elas → só HMG/Launcher/
  AttachedLauncher recebem XP do mod.
- **V2 (persistência):** se o server **não** salva o progresso dessas skills, a feature exige persistência server-side
  → **muda o escopo** (deixa de ser só client; coordenar server = colisão com editor). **Bloqueador potencial.**
- **V4 (categoria):** underbarrel acoplado sem flag simples (recon §5.2); HMG vs LMG ambas `machinegun` — pode forçar
  unificar. `WeaponMastery.SkillFor` encapsula essa decisão.
- **Empilhamento com Bunker:** coexistir multiplica skill × Bunker no Tank — validar balanceamento (config).
- **Conflito de patches:** os patches novos de efeito coexistem com os do 050 no mesmo método (Harmony compõe) — ordem
  não importa (multiplicadores comutam).

## 8. Checklist de implementação (DESBLOQUEIA após V1/V2/V4)

- [ ] **PRÉ:** rodar V1/V2/V4 in-game (spec funcional) e registrar os resultados aqui.
- [ ] `WeaponMastery.cs` (mapa categoria→skill conforme V4).
- [ ] `WeaponMasteryXpPatch` (Perna 1) — só categorias mortas (V1).
- [ ] `WeaponMasteryEffectPatches` (Perna 2) — escala por nível.
- [ ] F12 (§3) + `PROPRIEDADES.md` + `Enable()`.
- [ ] Compile 0/0.

## 9. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid — AP-01 | ✅ | Sem estado estático de raid; XP creditado por evento de tiro (raid-scoped natural). |
| 2 | Filtro MainPlayer/Fika — AP-02 | ✅ | Perna 1 gateia `ReferenceEquals(__instance, MainPlayer)`; efeito é por-arma local. §5. |
| 3 | Alvos ofuscados/virtuais — AP-03 | 🟡 | `ExecuteShotSkill`/`SetCurrent`/`ProceduralWeaponAnimation.Shoot` do recon (decompile curado); **reconfirmar linhas na versão atual antes de codar**. |
| 4 | Estado via API canônica — AP-04 | 🟡 | `SetCurrent` é a API de XP de skill, MAS **V3** confirma que propaga+persiste sem side-effect. |
| 5 | Estado entre raids | 🟡 | **V2** — depende do server persistir; se não, escopo muda. |
| 6 | ConfigEntry semântica/defaults — AP-05 | ✅ | F12 da §3 com defaults de paridade; `Weapon Mastery — Enabled` = estado neutro. |
| 7 | Reentry-guard — AP-07 | ✅ | Postfixes não re-invocam os métodos; efeito é multiplicador puro. |
| 8 | Flags/caches após troca — AP-08 | 🟡 | Detecção de categoria por `WeapClass` a cada disparo (sem cache); **V4** valida underbarrel/HMG≠LMG. |

> **Nota:** 4 checks em 🟡 apontam para a mesma raiz — as incógnitas V1/V2/V4. Por isso o code-mod aguarda a validação.

## 10. Redesenho pós-validação (2026-07-04 — resultados V + review 01 APLICADOS; este § manda sobre §1–§8)

**Resultados V** (01-spec, perfil zerado): SMG/LMG/Launcher-standalone **sobem vanilla** → FORA da Perna 1
(anti-XP-duplo, PA-01-01/riscos §7). **Underbarrel = única morta** (nem acerto de explosão credita — o Item do
hit é a munição). Extract persiste (V2 ✅). HMG intestável (NSV fixa de mapa).

**Perna 1 v2 — XP POR DISPARO do underbarrel** (o funil `ExecuteShotSkill` do §2 NÃO serve pro underbarrel):
- **Alvo:** `Player.FirearmController.method_57(LauncherItemClass launcher, AmmoItemClass ammo)` —
  [Player.cs:14231](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L14231) — roda **1× por
  disparo** do underbarrel (chamado pelo `OnFireEvent` do estado do lançador, Player.cs:6500-6512; faz o
  `InitiateShot`). Postfix.
- **Gate local (AP-02):** `ReferenceEquals(__instance, MainPlayer.HandsController)` (mesmo padrão do
  `HeavyWeaponErgoPatch` — cobre bots que usam FirearmController).
- **Crédito (PA-01-02/03):** `skills.AttachedLauncher` (campo público `WeaponSkillClass`,
  [SkillManager.cs:1320](../../../../references/eft-decompiled/Assembly-CSharp/SkillManager.cs#L1320)) →
  `SetCurrent(Current + xp × fatorClasse, silent: true)` — `fatorClasse` = `SkillMultipliers.TryGet(ESkillId.AttachedLauncher)`
  (consistência com o OnTriggerPatch; desvio da fadiga vanilla documentado como decisão).
- **Efeito por nível do próprio underbarrel (bônus):** `method_57` seta `float_5 = 1 + ammo.ammoRec/100`
  (coice pós-tiro) — o Postfix escala o EXCESSO: `float_5 = 1 + (float_5−1)·(1 − rec/lvl·Level)`.
- **HMG: DEFERIDA** (arma só existe estacionária — outro controller; sem como validar). Registrar no asbuild.

**Perna 2 v2 — efeito por nível (PA-01-04: por weapClass da ARMA EM MÃOS, não tipo C#):** categorias alcançáveis
com arma na mão: `smg` → `Skills.SMG` · `machinegun` → `Skills.LMG` (HMG inalcançável de mão) · `grenadeLauncher`
→ `Skills.Launcher`. Novos Prefix/Postfix nos MESMOS alvos do 050 (`ProceduralWeaponAnimation.Shoot` /
`FirearmController.TotalErgonomics` — coexistem com Shaky Hands/Adrenaline/Bunker; multiplicadores comutam):
`str × (1 − rec/lvl·Level)` e `ergo × (1 + ergo/lvl·Level)`. Aplica também às que sobem vanilla (o nível delas é
decorativo hoje — slots de buff vazios).

**F12 (fecha PA-01-07):** `Weapon Mastery — Enabled` (true) · `Underbarrel XP per shot` (**0.1**, 0–1 — paridade
`WeaponShotAction 0.1`) · `Recoil bonus per level` (0.004, 0–0.02) · `Ergo bonus per level` (0.002, 0–0.02).

**Resoluções da review 01 incorporadas:** PA-01-01 (protocolo estendido rodado; premissa corrigida) · PA-01-02
(fator de classe aplicado) · PA-01-03 (`silent: true`) · PA-01-04 (detecção pela arma em mãos/método dedicado —
sem depender de tipo do hit) · PA-01-05 (HMG deferida; discriminante estacionário anotado p/ futuro) · PA-01-06
(esta matriz) · PA-01-07 (defaults fixados) · PA-01-08 (morte: não testada, risco baixo — sistema nativo; V3
mod-side no checklist de validação) · PA-01-09 (friendly-fire: N/A no design por-disparo) · PA-01-10
(`HarmonyPriority` desnecessário: multiplicadores comutam; PerkDiag captura str0 no início do Prefix do 050 —
patch novo roda em Prefix separado) · PA-01-11 (refs corrigidas aqui).

**Validação pós-implementação (gate humano):** disparar GP-25 → barra de Underbarrel sobe (UI) → extract →
persistiu? (V3 mod-side) · efeito: `Recoil str` no overlay 052 cai com nível de SMG/LMG · sem XP duplo nas 3
vanilla · coop como cliente.

## Histórico

| Data | Evento |
|---|---|
| 2026-07-03 | Spec técnica via `/create-technical-spec` — plano client das 2 pernas; **code-mod bloqueado por V1/V2/V4** |
| 2026-07-04 | §10 — redesenho pós-V + review 01 aplicada (per-shot no `method_57`; anti-XP-duplo; HMG deferida; F12 fixado). Gate LIBERADO. |
