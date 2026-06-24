# 050 — Spec técnica · Fatia 050.0 (Infra + Bulwark + Pack Mule)

**Mod:** CustomClasses · **Data:** 2026-06-22 · **Status:** 🔵 Em andamento
**Refs:** [01-spec](./050-signature-patches-01-spec.md) · [class-design.md §Implementação](../../docs/class-design.md) · deps: **054 (rename Furtivo)**

> Escopo desta spec-tech = **fatia 050.0** (a infra + 2 provas). As fatias 050.1–050.4 ganham suas próprias spec-tech quando chegarem (cada uma re-confirma seus patch-points). **Confiança do recon = candidato; aqui re-confirmado no decompile.**

## Infra (reusa o que já existe no client)

- **Identidade da classe local — JÁ EXISTE:** `SkillMultipliers` (client) carrega `/customclasses/skill-multipliers` → `classNameEn`/`classNamePt` do perfil local ([SkillMultipliers.cs:79-83](../../modded/Client/SkillMultipliers.cs#L79)). Para os nossos 6, **`name` == `displayName.en`** (ambos inglês) → **`classNameEn` é a chave estável de gating** (idioma-independente). 
  - **Adicionar:** `public static string? ClassNameEn => _classNameEn;` + helper `public static bool IsLocalClass(string nameEn)` (compara case-insensitive). *(Hoje só expõe `ClassName`, resolvido por idioma — não serve p/ gating.)*
- **"É o player local?":** `Player.IsYourPlayer` (Bulwark) / comparar `__instance == mainPlayer.Skills` (Pack Mule) — **nunca** aplicar a bots/remotos.
- **F12 framework — JÁ EXISTE:** padrão `Config.Bind(...)` + `AcceptableValueRange` (slider) + `SettingChanged` em [Plugin.cs:35-82](../../modded/Client/Plugin.cs#L35). Novo arquivo `PerksConfig.cs` com as entries; **lidas no apply-time** (sem cache).

## Patch-points (re-confirmados no decompile)

### Bulwark — dano recebido ×0.85 (Tanque)
- **Alvo:** `Player.ApplyDamageInfo(DamageInfoStruct, EBodyPart, EBodyPartColliderType, float)` — [Player.cs:~30163](../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L30163) aplica armadura→vida dentro do método.
- **Patch:** `Prefix` — se `__instance.IsYourPlayer` **e** `SkillMultipliers.IsLocalClass("Tank")` **e** `Bulwark.Enabled` → `damageInfo.Damage *= Bulwark.DamageTaken` (0.85). `DamageInfoStruct` é struct → receber por `ref`.
- **Nota:** escala o dano de entrada (a armadura absorve do valor já escalado → a perda de HP final cai ~15%, proporcional). Refino "estritamente pós-armadura" fica p/ 050.3 se medirmos desvio. **Não** mexer no caminho de bots (gate `IsYourPlayer`).

### Pack Mule — +30% limite de carga (piso) (Saqueador + Tanque)
- **Alvo:** `SkillManager.CarryingWeightRelativeModifier => 1f + (float)StrengthBuffLiftWeightInc` — [SkillManager.cs:1836](../../../references/eft-decompiled/Assembly-CSharp/SkillManager.cs#L1836).
- **Patch:** `Postfix` — se `__instance` é o `Skills` do MainPlayer local **e** `IsLocalClass("Scavenger"|"Tank")` **e** `PackMule.Enabled` → `__result = Math.Max(__result, 1f + PackMule.CarryLimitBonus)` (piso 1.30). **Piso, não soma** (respeita o cap vanilla `Max(0.3)` e a decisão K).

## §9 Conformidade (antipatterns)

- **AP-06 (compilar ≠ funcionar):** o "entregue" é validação **in-game** — gate humano; esta fatia para no `compile-mod`.
- **Per-player / sem vazar p/ bots:** todo patch gateia em player local (`IsYourPlayer` / `mainPlayer.Skills`). Coop: efeito é local de cada cliente.
- **Lifecycle/leaks:** patches habilitados no `Awake` (persistem); **sem** estado em-raid (Bulwark/Pack Mule são stateless) → nada a limpar no fim da raid.
- **Config no apply-time:** ler `ConfigEntry.Value` dentro do patch (não cachear) → F12-live (DoD).
- **Gating idioma-independente:** comparar `classNameEn` (inglês), não a string localizada do `GameVersion`.
- **Dep 054:** o gate do Furtivo só casa após o rename (`name`=`Stealth`); Bulwark/Pack Mule (Tank/Scavenger) não dependem do 054.

## Arquivos (modded/Client)

| Arquivo | Mudança |
|---|---|
| `SkillMultipliers.cs` | + `ClassNameEn` getter + `IsLocalClass(nameEn)` |
| `PerksConfig.cs` *(novo)* | `ConfigEntry` de Bulwark (Enabled, DamageTaken=0.85) e Pack Mule (Enabled, CarryLimitBonus=0.30) |
| `Patches/BulwarkPatch.cs` *(novo)* | Prefix `Player.ApplyDamageInfo` |
| `Patches/PackMulePatch.cs` *(novo)* | Postfix `SkillManager.CarryingWeightRelativeModifier` |
| `Plugin.cs` | bind do `PerksConfig` + `.Enable()` dos 2 patches |

## DoD (050.0)

- Tanque perde ~15% menos HP num hit conhecido; Saqueador/Tanque com +30% no limite de peso; **zero efeito** em outras classes/bots; F12 muda ao vivo. (Validação in-game = gate.)

## Histórico

| Data | Autor | Alteração |
|---|---|---|
| 2026-06-22 | Guilherme | Criação. Spec-tech da fatia 050.0: patch-points re-confirmados (`Player.ApplyDamageInfo`, `SkillManager.CarryingWeightRelativeModifier`), gating via `SkillMultipliers.classNameEn` (chave estável), F12 framework, §9. |
