# 003 — Corrigir achados altos da auditoria 01 · As-Built

**Mod:** Skills-Extended
**Spec funcional:** [003-corrigir-achados-altos-auditoria-01-01-spec.md](003-corrigir-achados-altos-auditoria-01-01-spec.md)
**Spec técnica:** [003-corrigir-achados-altos-auditoria-01-02-spec-tech.md](003-corrigir-achados-altos-auditoria-01-02-spec-tech.md)
**Última review técnica:** [003-corrigir-achados-altos-auditoria-01-03-spec-tech-review-01.md](003-corrigir-achados-altos-auditoria-01-03-spec-tech-review-01.md)
**Build inicial:** 2026-09-07

> Documentação pós-implementação. Reflete o estado real do código entregue pelo `/code-mod`.

## Arquivos alterados (build inicial)

| Ação | Path | Resumo |
| --- | --- | --- |
| MODIFICADO | `mods/Skills-Extended/modded/Plugin/Skills/FirstAid/Patches/HealthEffectComponentPatch.cs` | Reescrito: `PerInstanceHealthEffect` (novo wrapper `IHealthEffect`) substitui a mutação em `template.DamageEffects[x].Cost`; tooltip ressincronizado via `Item.AddOrReplaceAttribute`/`Item.Replacements`; bookkeeping antigo (`OriginalCosts`/`InstanceIdsChangedAtLevel`/`ResetLevelChangedAt`) removido por não ser mais necessário |
| MODIFICADO | `mods/Skills-Extended/modded/Plugin/Skills/WeaponSkills/Patches/UpdateWeaponsPatch.cs` | 4 dicionários viram `internal`; `Prefix` gateado por `GameUtils.IsInRaid()`; `.Clear()` incondicional removido; mutação de `Template.Ergonomics` removida (resolvida via `ErgonomicsTotalPatch`); `Template.RecoilForceUp`/`RecoilForceBack` preservados com comentário de débito técnico; novos métodos `TriggerRaidStart()` e `ClearRaidState()` |
| CRIADO | `mods/Skills-Extended/modded/Plugin/Skills/WeaponSkills/Patches/ErgonomicsTotalPatch.cs` | Postfix em `Weapon.ErgonomicsTotal` — aplica o bônus de Ergonomia por-instância, gateado por `UsecWeaponInstanceIds`/`EasternWeaponInstanceIds` |
| MODIFICADO | `mods/Skills-Extended/modded/Plugin/Models/WeaponClasses.cs` | Campo `ergo` removido de `OrigWeaponValues` (ficou órfão após a mutação de Ergonomics ser removida) |
| MODIFICADO | `mods/Skills-Extended/modded/Plugin/Skills/Core/Patches/SkillManagerConstructorPatch.cs` | Adicionado `SkillOwners` (`ConditionalWeakTable<AbstractSkillClass, SkillManager>`) e registro de `FieldMedicine` no Postfix |
| MODIFICADO | `mods/Skills-Extended/modded/Plugin/Skills/FieldMedicine/Patches/AbstractSkillClassSummaryLevelPatch.cs` | Reescrito: resolve o `SkillManager` via `SkillManagerConstructorPatch.SkillOwners` em vez de `GameUtils.GetSkillManager()` |
| MODIFICADO | `mods/Skills-Extended/modded/Plugin/Skills/SilentOps/Patches/GetBarterPricePatch.cs` | Null-guard adicionado em `GetBarterPricePatch.Postfix` e `RequiredItemsCountPatch.Postfix` |
| MODIFICADO | `mods/Skills-Extended/modded/Plugin/Skills/Shared/Patches/OnGameStarted.cs` | `Player` e os 3 handlers (`ApplyMedicalXp`/`ApplyNatoRifleXp`/`ApplyEasternRifleXp`) viram `internal`; `ApplyMedicalXp` ganha guard `if (Player == null) return;`; `Postfix` chama `UpdateWeaponsPatch.TriggerRaidStart()`; nova classe `OnGameEndedPatch` (Postfix em `GameWorld.OnDestroy`) desinscrevendo os 3 eventos e chamando `UpdateWeaponsPatch.ClearRaidState()` |
| MODIFICADO | `mods/Skills-Extended/modded/Plugin/Helpers/ReflectionHelper.cs` | Removidos os 7 campos de reflection não utilizados (incluindo os 2 com bug de copy-paste) e a lógica de resolução correspondente; `GetOldMovementTypes()`/`OldMovementIdleState` preservados |
| MODIFICADO | `mods/Skills-Extended/modded/FikaSync/Patches/OnGameStartedPatch.cs` | Adicionado `LockPickingHelpers.DoorAttempts.Clear()` no início do Postfix (roda em client e headless) |
| MODIFICADO | `mods/Skills-Extended/modded/Common/SkillsExtendedInfo.cs` | Bump de versão 2.2.5 → 2.2.6 |
| MODIFICADO | `mods/Skills-Extended/modded/Plugin/Plugin.csproj` | Bump de versão 2.2.5 → 2.2.6 |
| MODIFICADO | `mods/Skills-Extended/modded/Server/Server.csproj` | Bump de versão 2.2.5 → 2.2.6 (sincronizado, embora nenhum código de servidor tenha sido tocado neste item) |

## PA-NN-MM resolvidos durante o build

> Todos os 4 pontos da review 01 foram resolvidos na própria spec técnica antes do build (ver histórico da review) — a implementação seguiu os stubs já corrigidos, sem retrabalho.

| ID | Categoria · Impacto | Resumo da resolução |
| --- | --- | --- |
| PA-01-01 | B — Edge Case · 🟡 | `UpdateWeaponsPatch.TriggerRaidStart()` chamado por `OnGameStartedPatch.Postfix` — bônus de arma ativo desde o 1º frame da raid |
| PA-01-02 | A — Gap · 🟡 | `ApplyMedicalXp` ganhou `if (Player == null) return;` — corner case de janela de transição de cena coberto |
| PA-01-03 | C — Erro de Lógica · 🟢 | `OnGameEndedPatch` referencia os nomes reais dos 3 handlers (`internal static`), sem placeholder |
| PA-01-04 | B — Edge Case · 🟢 | Grep por `.IHealthEffect ==`/`!=` em `EFT.InventoryLogic`/`EFT.HealthSystem` — sem ocorrências |

## Achado durante o build (não estava em nenhuma review)

Ao remover a mutação de `weapon.Template.Ergonomics` (resolvida via `ErgonomicsTotalPatch`), o campo `ergo` de `OrigWeaponValues` (`Plugin/Models/WeaponClasses.cs`) ficou órfão — capturado mas nunca mais lido. Removido por consistência (mesma classe de problema do `AUD-01-13`, mas descoberto aqui, não na auditoria original). Não afeta nenhum comportamento — `RecoilForceUp`/`RecoilForceBack` continuam usando seus próprios campos (`weaponUp`/`weaponBack`), inalterados.

Também durante o build, a verificação residual do `AUD-01-14` (overrides de `GameWorld.OnDestroy` além de `ClientGameWorld`) foi **fechada por completo**, não só mitigada como a spec técnica antecipava: a cadeia de herança tem só duas subclasses concretas (`ClientLocalGameWorld`, solo, não sobrescreve `OnDestroy`; `ClientNetworkGameWorld`, coop/Fika/headless, sobrescreve e chama `base.OnDestroy()`) — os dois caminhos reais de raid chegam confirmadamente em `GameWorld.OnDestroy()`. Spec técnica §1.8/§9 atualizada para refletir o achado fechado (sem débito técnico residual, ao contrário do que o item 001 aceitou para o caso análogo de `SmoothDoorOpenCoroutine`).

## Mudanças posteriores

### 2026-09-07 — `/apply-code-review` (rodada 01)

| ID | Resultado | Arquivo tocado |
| --- | --- | --- |
| CR-01-01 | ⏭️ Rejeitado | Nenhum — débito técnico de Recoil aceito por decisão explícita do usuário, sem editar a spec funcional |

Nenhum arquivo de código alterado nesta rodada (o único achado da code review 01 foi rejeitado).

## Pendências não-bloqueadoras para validação manual

- Validar in-game que o tooltip do item médico mostra o custo já reduzido pela skill FirstAid (`AUD-01-07`).
- Validar in-game que o bônus de Ergonomia/Recuo da arma já está ativo no primeiro frame da raid, sem precisar abrir nenhuma tela antes (`PA-01-01`).
- Compilação efetiva (`.dll`) ainda não realizada nesta rodada — fora do escopo do `/code-mod`; pendente de `/compile-mod` (ou build manual redirecionado, conforme preferência já estabelecida neste mod).

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-07 | Build concluído via `/code-mod` — 9 achados (`AUD-01-07` a `AUD-01-15`) implementados em `mods/Skills-Extended/modded/`; 4 pontos da review técnica 01 (`PA-01-01` a `PA-01-04`) já vinham resolvidos na spec técnica antes do build |
| 2026-09-07 | Compilação Release (Plugin + Common) via `dotnet build` redirecionado — 0 erros de código; nada instalado em `E:/Tarkov Red Line` |
| 2026-09-07 | `/code-review` rodada 01 — 1 achado (`CR-01-01`, gap entre spec funcional e débito técnico documentado de Recoil); `/apply-code-review` rodada 01 — CR-01-01 rejeitado (débito aceito por decisão explícita do usuário) |
