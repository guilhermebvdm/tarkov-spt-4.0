# 005 — Multiplicadores de skill · As-Built

**Mod:** CustomClasses
**Spec funcional:** [005-skill-multipliers-01-spec.md](005-skill-multipliers-01-spec.md)
**Spec técnica:** [005-skill-multipliers-02-spec-tech.md](005-skill-multipliers-02-spec-tech.md)
**Review técnica:** [review-01](005-skill-multipliers-03-spec-tech-review-01.md)
**Build:** 2026-06-07

> Item **híbrido**, implementação **por fatias**. Esta entrega é a **Fatia 1a (SERVER)**. 🟡.

## Fatias

| Fatia | Conteúdo | Status |
|---|---|---|
| **1a** | Server: `skillMultipliers` no JSON + `SkillMultiplierRegistry` (singleton) + `SkillMultipliersRouter` (rota por edition) + populate das 10 classes | ✅ compilado/instalado |
| **1b** | Client BepInEx (1º projeto client → mod híbrido): `OnTrigger` Prefix escala XP (clamp ≥0) + cache lazy (fetch da rota) | ✅ compilado/instalado |
| **1c** | Client gym: `WorkoutBehaviour.method_18` Prefix/Postfix (snapshot/delta) escala XP de treino | ✅ compilado/instalado |
| **2** | Client UI: `SkillPanel` (seta verde/vermelha na linha) + `SkillTooltip` ("XP da classe: +X%") | ✅ compilado/instalado |

## Arquivos alterados (Fatia 1a)

| Ação | Path | Resumo |
| --- | --- | --- |
| MODIFICADO | `modded/Server/ClassDefinition.cs` | + `skillMultipliers` (skill→fator double). |
| CRIADO | `modded/Server/SkillMultiplierRegistry.cs` | Singleton (`InjectionType.Singleton`): edition→(skill→fator). |
| CRIADO | `modded/Server/SkillMultipliersRouter.cs` | `StaticRouter` `/customclasses/skill-multipliers` → resolve sessionId→edition→registry. |
| MODIFICADO | `modded/Server/CustomClassesMod.cs` | Injeta registry; no `RegisterClass` valida (SkillTypes + clamp ≥0) e grava por edition; log `skillMults=N`. |
| MODIFICADO | `scripts/build-class-jsons.js` | `SKILL_MULTIPLIERS` (buffs temáticos por classe) + emite `skillMultipliers`. |
| MODIFICADO | `modded/Server/config/classes/*.jsonc` | 10 classes com `skillMultipliers` temáticos. |

## PA resolvidos (review 01)

| ID | Resolução |
| --- | --- |
| PA-01-02 | StaticRouter/RouteAction/EmptyRequestData + Edition por sessionId confirmados; router feito. |
| PA-01-05 | `[Injectable]` default Scoped → registry = `InjectionType.Singleton`. |
| PA-01-01 | ⏭️ Fatia 2 (UI ofuscada). |
| PA-01-03 / PA-01-04 | ⬜ Fatia 1b (client): mapa `ESkillId` case-insensitive + hook de config pronta. |

## Pendências

1. ✅ `/compile-mod` server (0 warn/err, 54.3 KB) + 10 `.jsonc` com `skillMultipliers` instaladas. **Rota responde** (testável via client na Fatia 1b).
2. **Fatia 1b (client):** projeto BepInEx + `OnTrigger` Prefix (escala XP, clamp ≥0) + cache (fetch `/customclasses/skill-multipliers`) + hook de perfil pronto. Resolve PA-01-03/04.
3. **Fatia 2 (UI):** `+X%/−X%` na linha + tooltip (confirmar `SkillPanel`/`SkillTooltip` no decompilado — PA-01-01).
4. Playtest end-to-end → 🟢.

## Mudanças posteriores

**2026-06-07 — Fatia 1b (client):** 1º projeto client do mod → **híbrido**. `modded/Client/`: `CustomClasses.Client.csproj` (refs via `References/` do compile-mod), `Plugin.cs` (BepInPlugin + config `EnableSkillMultipliers`), `SkillMultipliers.cs` (cache `ESkillId→fator`, fetch lazy via `RequestHandler.GetJson` + map case-insensitive — PA-01-03/04), `Patches/OnTriggerPatch.cs` (Prefix em `AbstractSkillClass.OnTrigger`, `val *= fator`, clamp ≥0, try/catch). `+ Newtonsoft.Json.dll` no `resolve_references` do `compile-mod.sh`. Compila 0 warn/err; Client → `BepInEx/plugins/CustomClasses`, Server → `user/mods/CustomClasses`. Resta Fatia 2 (UI).

**2026-06-07 — fix UI lazy-load:** num perfil novo sem XP ganho, o cache (lazy no 1º `OnTrigger`) estava vazio → a UI não mostrava seta/tooltip. Adicionado `SkillMultipliers.EnsureLoaded()` no início de `SkillPanelPatch`/`SkillTooltipPatch` (carrega ao abrir a tela de Skills). Recompilado. **Lembrete:** o client é plugin BepInEx → exige **restart do JOGO** pra carregar o DLL novo.

## Histórico

| Data | Evento |
| --- | --- |
| 2026-06-07 | Fatia 1a (server) via `/code-mod` — DTO + registry + router + populate. Compilado 0 warn/err. PA-01-02/05 resolvidos. |
| 2026-06-07 | Fatia 1b (client) — projeto BepInEx + OnTrigger scaling + cache lazy. Híbrido compila 0 warn/err. PA-01-03/04 resolvidos. |
| 2026-06-07 | Fatia 1c (gym) + Fatia 2 (UI: seta na linha + texto no tooltip). `SkillPanel`/`SkillTooltip` em `EFT.UI` (não ofuscados — PA-01-01 resolvido). +refs Sirenix/TMP/UnityUI no compile-mod. Client 12 KB, 0 warn/err. **005 implementado por completo.** |
