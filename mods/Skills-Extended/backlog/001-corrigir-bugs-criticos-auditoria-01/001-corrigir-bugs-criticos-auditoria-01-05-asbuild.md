# 001 — Corrigir bugs críticos da auditoria 01 · As-Built

**Mod:** Skills-Extended
**Spec funcional:** [001-corrigir-bugs-criticos-auditoria-01-01-spec.md](001-corrigir-bugs-criticos-auditoria-01-01-spec.md)
**Spec técnica:** [001-corrigir-bugs-criticos-auditoria-01-02-spec-tech.md](001-corrigir-bugs-criticos-auditoria-01-02-spec-tech.md)
**Última review técnica:** [001-corrigir-bugs-criticos-auditoria-01-03-spec-tech-review-01.md](001-corrigir-bugs-criticos-auditoria-01-03-spec-tech-review-01.md)
**Build inicial:** 2026-09-07

> Documentação pós-implementação. Reflete o estado real do código entregue pelo `/code-mod`.

## Arquivos alterados (build inicial)

| Ação | Path | Resumo |
| --- | --- | --- |
| CRIADO | `mods/Skills-Extended/modded/Plugin/Skills/SilentOps/Patches/DoorInteractionTrackerPatch.cs` | Captura `isLocalInteraction` por porta a partir de `SmoothDoorOpenCoroutine`, pra `DoorSoundPatch` saber de quem é a interação |
| MODIFICADO | `mods/Skills-Extended/modded/Plugin/Skills/SilentOps/Patches/DoorSoundPatch.cs` | Só aplica o bônus de Silent Ops quando a interação é do jogador local; `return true` (som nativo) caso contrário ou se `SkillManager` for nulo |
| MODIFICADO | `mods/Skills-Extended/modded/Plugin/Skills/Strength/Patches/MovementContextSetSpeedLimitPatch.cs` | Injeta `Player ____player`, filtra por `IsYourPlayer`, checa null do `SkillManager` antes de usar |
| MODIFICADO | `mods/Skills-Extended/modded/Plugin/Skills/WeaponSkills/Patches/UpdateWeaponsPatch.cs` | `SkillManager`/`Profile` resolvidos dentro do loop de `UpdateUsecWeapons`/`UpdateEasternWeapons`, com `yield break` a cada iteração se nulo |
| MODIFICADO | `mods/Skills-Extended/modded/Server/Patches/CultistProductionPatch.cs` | Reescrito: desconto de tempo do sacrifício cultista aplicado no Postfix de `StartSacrifice` via `Production.SptIsCultistCircle`/`ProductionTime`, sem campo `static` compartilhado |
| MODIFICADO | `mods/Skills-Extended/modded/Server/Patches/GeneratePlayerScavPatch.cs` | `_generateAsCultist` estático substituído por `__state` do Harmony |
| MODIFICADO | `mods/Skills-Extended/modded/Common/SkillsExtendedInfo.cs` | Bump de versão 2.2.3 → 2.2.4 |
| MODIFICADO | `mods/Skills-Extended/modded/Plugin/Plugin.csproj` | Bump de versão 2.2.3 → 2.2.4 |
| MODIFICADO | `mods/Skills-Extended/modded/Server/Server.csproj` | Bump de versão 2.2.1 → 2.2.4 (estava desalinhado do `SkillsExtendedInfo.VERSION` antes desta sessão) |

## PA-NN-MM resolvidos durante o build

> Todos os pontos da review 01 foram resolvidos durante a própria spec técnica (não durante o build) — ver histórico da review. Nenhum ponto novo surgiu durante a implementação.

| ID | Categoria · Impacto | Resumo da resolução |
| --- | --- | --- |
| PA-01-01 | C — Erro de Lógica · (era 🟡) | `SmoothDoorOpenCoroutine` confirmado `virtual`; overrides de `Door`/`KeycardDoor` auditados (chamam `base.*`) |
| PA-01-02 | A — Gap · (era 🔴) | `CultistProductionPatch` reescrito usando `Production.SptIsCultistCircle`/`ProductionTime` do modelo real do hideout |
| PA-01-03 | B — Edge Case · 🟢 | Texto do check 1 (§9) corrigido |

## Pendências não-bloqueadoras levadas para o `/code-review`

- Confirmar se `Switch`/`LootableContainer`/`DoorSwitch`/`BufferGateSwitcher` chamam `base.SmoothDoorOpenCoroutine` (mitigado pelo fail-safe de `DoorSoundPatch`, mas não verificado nesta rodada).
- Validação em raid e em Fika coop ainda não realizada (pendente de teste manual do usuário).

## Mudanças posteriores

### 2026-09-07 — `/apply-code-review` (rodada 01)

| ID | Resultado | Arquivo tocado |
| --- | --- | --- |
| CR-01-01 | ✅ Aplicado | `mods/Skills-Extended/modded/Server/Patches/CultistProductionPatch.cs` (reescrito — patch movido de `StartSacrifice`/`GetCircleCraftingInfo` para `RegisterCircleOfCultistProduction`, elimina risco de reaplicar o desconto numa produção pré-existente) |
| CR-01-02 | ⏭️ Rejeitado | Nenhum — dívida técnica aceita (verificar overrides de `Switch`/`LootableContainer`/`DoorSwitch`/`BufferGateSwitcher` numa rodada futura) |

Versão bump: 2.2.4 → 2.2.5.

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-07 | Build concluído via `/code-mod` — Plugin e Server compilados com 0 erros de código |
| 2026-09-07 | `/apply-code-review` rodada 01 — CR-01-01 aplicado, CR-01-02 rejeitado (dívida técnica); recompilado com 0 erros |
