# 025 — EmergencyDrop na cirurgia própria (self-heal) · As-Built

**Mod:** TRL-ImmersiveCombatMedicine
**Spec funcional:** [025-emergencydrop-autocirurgia-01-spec.md](025-emergencydrop-autocirurgia-01-spec.md)
**Spec técnica:** [025-emergencydrop-autocirurgia-02-spec-tech.md](025-emergencydrop-autocirurgia-02-spec-tech.md)
**Última review técnica:** [025-emergencydrop-autocirurgia-03-spec-tech-review-01.md](025-emergencydrop-autocirurgia-03-spec-tech-review-01.md)
**Build inicial:** 2026-09-12

> Documentação **pós-implementação**. Reflete o estado real do código entregue pelo `/code-mod`. Quando o conteúdo aqui diverge da spec técnica, este documento ganha — a spec é planejamento, o asbuild é o que foi feito.

## Arquivos alterados (build inicial)

| Ação | Path | Resumo |
| --- | --- | --- |
| MODIFICADO | `mods/TRL-ImmersiveCombatMedicine/modded-V4/Patches/Medical/BandAidController.cs` | Gate de `Update()` estendido com `selfSurgeryInProgress` (mutuamente exclusivo com `_isHealingInProgress`); novo método privado `EmergencyDropSelf(Player, Player.MedsController)` — cancela a auto-cirurgia via `ClearQueue()`+`CancelApplyingItem()` (dispara a punição nativa de carga sem reimplementar), pula o fechamento do kit via `DestroyController()`, reequipa a arma via `TrySetLastEquippedWeapon`, e dropa o item (guard `CurrentAddress != null`). |
| MODIFICADO | `mods/TRL-ImmersiveCombatMedicine/modded-V4/TRL-ImmersiveCombatMedicine.csproj` | Bump de versão `1.14.2` → `1.14.3`. |
| MODIFICADO | `mods/TRL-ImmersiveCombatMedicine/modded-V4/TRLImmersiveCombatMedicinePlugin.cs` | `BepInPlugin` version string `1.14.2` → `1.14.3`. |

Nenhum arquivo criado — sem novo Harmony patch, sem novo pacote de rede.

## PA-NN-MM resolvidos durante o build

> Os 3 pontos da review 01 já haviam sido resolvidos na própria spec técnica (§5/§7) antes do `/code-mod` — o build só materializou o que a spec definiu.

| ID | Categoria · Impacto | Resumo da resolução |
| --- | --- | --- |
| PA-01-01 | A — Gap · 🟢 | Comentário inline explicando por que `SetPhysicalCondition(UsingMeds, false)` é mantida (gate de movimento puro, confirmado seguro; precaução contra o clear nativo que pode viver dentro do trecho que `DestroyController()` pula). |
| PA-01-02 | A — Gap · 🟢 | Comentário inline confirmando que `ReleaseSurgeryImmobilize` é um no-op neste contexto, mantido só por consistência. |
| PA-01-03 | A — Gap · 🟢 | §7 da spec técnica reescrito de "não confirmado" pra "confirmado" quanto à ausência de auto-descarte nativo — guard mantido por defesa, não por necessidade comprovada. |

## Decisões de design carregadas da spec

- **Isolamento aliado × self:** já garantido pelo guard G5 existente em `MedicHealPatch.cs:340-347` (bloqueia self-heal vanilla do médico enquanto `BandAidHealActive`) — nenhuma mudança necessária nesse patch, só a garantia de que `EmergencyDropSelf` nunca dispara com `_isHealingInProgress` ativo.
- **Sem reimplementação de consumo de carga:** confirmado por investigação (subagent + verificação manual) que a punição nativa de perda de carga por cancelamento tardio roda de forma síncrona dentro de `ActiveHealthController.CancelApplyingItem()` (`ActiveHealthController.cs:1944-1949`), desacoplada do fechamento do kit — chamar essa API já é suficiente, sem duplicar nem pular a punição.
- **`ForceFinishAnimation()` (item 024) deliberadamente NÃO reusado** — `MedicHealPatch._currentObservedMedsControllerClass` só é populado durante redirect de cura de aliado (`MedicHealPatch.cs:338`); irrelevante pra auto-cirurgia.

## Mudanças posteriores

### `/apply-code-review` — rodada 01 (2026-09-12)

| ID | Categoria · Impacto | Resolução |
| --- | --- | --- |
| CR-01-01 | F — Melhoria opcional · 🟡 | `Update()` passou a cachear o resultado de `IsSurgery` por referência de `Item` (`_lastSelfMedsItemChecked`/`_lastSelfMedsIsSurgery`), recalculando `ItemDatabase.GetStats` só quando o item nas mãos muda, não todo frame. Cache resetado em `ResetAllState()`. |
| CR-01-02 | D — Arquitetura · 🟢 | Comentário adicionado ao gate de `Update()` documentando a invariante entre `_isHealingInProgress` e `MedicHealPatch.BandAidHealActive` (guard G5). |

Arquivos tocados nesta rodada: `mods/TRL-ImmersiveCombatMedicine/modded-V4/Patches/Medical/BandAidController.cs` (modificado). Build `dotnet build -c Release`: 0 Erros, 0 Warnings.

### Fix 01 (2026-09-12, SUPERADO pelo Fix 02) — ver [025-emergencydrop-autocirurgia-06-fix-01.md](025-emergencydrop-autocirurgia-06-fix-01.md)

Bug reportado em raid real: mãos travadas (`HandsController` nulo, `NullReferenceException` em loop em `MovementContext.Class1396.method_3`) ao repetir o drop de emergência rápido demais — `TrySetLastEquippedWeapon` dispara um processo nativo assíncrono (`SpawnController`) que pode colidir com uma nova tentativa disparada antes de confirmar. Corrigido com guard de reentrância `_weaponEquipPending` (`BandAidController.cs`). Afeta também o item 024 (mesmo call site). Versão `1.14.4`. **Insuficiente** — ver Fix 02.

### Fix 02 (2026-09-12) — ver [025-emergencydrop-autocirurgia-06-fix-02.md](025-emergencydrop-autocirurgia-06-fix-02.md)

Usuário reproduziu o mesmo travamento SEM repetição de ação (um único drop já causava "arma puxada e guardada sozinha"), provando que o Fix 01 não resolvia a causa raiz. `TrySetLastEquippedWeapon()` **removido por completo** de `EmergencyDrop()`/`EmergencyDropSelf()` — mãos ficam livres após o drop, mas SEM reequipar a arma automaticamente (o jogador precisa apertar a própria tecla de arma). Campos do Fix 01 (`_weaponEquipPending` etc.) removidos por não terem mais função. Versão `1.14.5`. **Muda um critério de aceite das specs funcionais dos itens 023/024/025** — ver nota nas respectivas specs.

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-12 | Build concluído via `/code-mod`. `dotnet build -c Release` em `modded-V4`: 0 Erros, 0 Warnings. Versão `1.14.3`. Binário gerado só em `modded-V4/bin/Release/netstandard2.1/` (pasta do jogo não tocada). |
| 2026-09-12 | Aplicação de 2 achados de code-review 01 via `/apply-code-review` — IDs: CR-01-01 (cache por item), CR-01-02 (comentário de invariante). Build verde, 0 Erros/0 Warnings. |
| 2026-09-12 | Fix 01: guard de reentrância `_weaponEquipPending` pra evitar colisão com o processo assíncrono nativo de `TrySetLastEquippedWeapon`, corrigindo travamento de mãos reportado em raid. Versão `1.14.4`, build verde. |
| 2026-09-12 | Fix 02: Fix 01 se mostrou insuficiente (bug reproduzido sem repetição). `TrySetLastEquippedWeapon()` removido por completo de `EmergencyDrop`/`EmergencyDropSelf` — mãos ficam livres sem reequipar arma automaticamente. Versão `1.14.5`, build verde. Validação in-game pendente. |
