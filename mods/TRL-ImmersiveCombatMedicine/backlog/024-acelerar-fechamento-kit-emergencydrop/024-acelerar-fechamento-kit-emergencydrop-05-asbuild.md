# 024 — Descarte instantâneo das mãos no EmergencyDrop · As-Built

**Mod:** TRL-ImmersiveCombatMedicine
**Spec funcional:** [024-acelerar-fechamento-kit-emergencydrop-01-spec.md](024-acelerar-fechamento-kit-emergencydrop-01-spec.md)
**Spec técnica:** [024-acelerar-fechamento-kit-emergencydrop-02-spec-tech.md](024-acelerar-fechamento-kit-emergencydrop-02-spec-tech.md)
**Última review técnica:** [024-acelerar-fechamento-kit-emergencydrop-03-spec-tech-review-01.md](024-acelerar-fechamento-kit-emergencydrop-03-spec-tech-review-01.md)
**Build inicial:** 2026-09-12

> Documentação **pós-implementação**. Reflete o estado real do código entregue pelo `/code-mod`. Quando o conteúdo aqui diverge da spec técnica, este documento ganha — a spec é planejamento, o asbuild é o que foi feito.

## Arquivos alterados (build inicial)

| Ação | Path | Resumo |
| --- | --- | --- |
| MODIFICADO | `mods/TRL-ImmersiveCombatMedicine/modded-V4/Patches/Medical/BandAidController.cs` | Novo campo `_currentHealIsSurgery` (espelha `_itemBeingUsed` em todos os 6 pontos de set/reset); gate de `Update()` restringindo a tecla de EmergencyDrop a itens de cirurgia; `EmergencyDrop()` reescrito: PASSO 1 agora captura `elapsed`/`savedStats`/`isSurgeryItem` e reseta `_healStartTime`; novo PASSO 2b chama `doctor.DestroyController()` (guardado por `isSurgeryItem`) entre `ForceFinishAnimation` e `TrySetLastEquippedWeapon`; PASSO 4 aplica penalidade de carga (`ConsumeSafe`, ≥1s) e notificação diferenciada (`TreatmentCancelledWithItemLoss`/`ItemDropped`). |
| MODIFICADO | `mods/TRL-ImmersiveCombatMedicine/modded-V4/TRL-ImmersiveCombatMedicine.csproj` | Bump de versão `1.14.1` → `1.14.2`. |
| MODIFICADO | `mods/TRL-ImmersiveCombatMedicine/modded-V4/TRLImmersiveCombatMedicinePlugin.cs` | `BepInPlugin` version string `1.14.1` → `1.14.2`. |

Nenhum arquivo criado — sem novo Harmony patch, sem novo pacote de rede.

## PA-NN-MM resolvidos durante o build

> Todos os 3 pontos da review 01 já haviam sido resolvidos na própria spec técnica (§5) antes do `/code-mod` — o build só materializou o que a spec definiu.

| ID | Categoria · Impacto | Resumo da resolução |
| --- | --- | --- |
| PA-01-01 | C — Erro de Lógica · 🟡 | Auditoria AP-03 fechada: `EmergencyDrop()` só é alcançável com `HandsController` sendo `null` ou `Player.MedsController` (única classe usada por qualquer `MedsItemClass`) — os outros 7 overrides de `Destroy()` nunca são atingidos neste call site. Reforçado pelo escopo exclusivo a cirurgia. |
| PA-01-02 | A — Gap · 🟡 | `EmergencyDrop` agora mostra `TreatmentCancelledWithItemLoss` quando a penalidade é aplicada e `ItemDropped` quando não, mesmo padrão de `CancelHealInProgress`. |
| PA-01-03 | B — Edge Case · 🟢 | `_healStartTime = -1f;` adicionado logo após capturar `elapsed`, no início de `EmergencyDrop`. |

## Decisão de escopo pós-review (antes do `/code-mod`)

Depois da review 01 (sem bloqueadores), o usuário restringiu o escopo do item: **`EmergencyDrop` passa a ser exclusivo a itens de cirurgia (CMS/Surv12)** — para bandagem/tala/torniquete/medkit comum, a tecla não produz nenhum efeito, porque o encerramento vanilla desses itens já é curto o bastante pra ser confortável (só CMS/Surv12 têm a animação longa de "fechar o kit" que motivou este item). Isso foi propagado às specs (`01-spec.md`, `02-spec-tech.md`) e ao código via:
- Novo campo `_currentHealIsSurgery`, setado em `HealRoutine` (`stats.IsSurgery`) e resetado em todos os pontos onde `_itemBeingUsed` é limpo.
- Gate em `Update()`: `_isHealingInProgress && _currentHealIsSurgery && CheckPressMode(...)`.
- Re-checagem defensiva (`if (isSurgeryItem)`) dentro do próprio `EmergencyDrop`, envolvendo só o novo PASSO 2b (`DestroyController`) — não depende exclusivamente do gate de entrada.

## Mudanças posteriores

### `/apply-code-review` — rodada 01 (2026-09-12)

| ID | Categoria · Impacto | Resolução |
| --- | --- | --- |
| CR-01-01 | B — Bug latente · 🟠 | Bloco de penalidade de carga (`MedicalLogic.ConsumeSafe`) movido pra ANTES do PASSO 4 (`ThrowItem`), mesma ordem de `CancelHealInProgress`. `ThrowItem` agora condicionado a `savedItem.CurrentAddress != null` (evita jogar um item que `ConsumeSafe` já pode ter descartado via `DiscardItemNetworked` ao chegar a zero carga). |
| CR-01-02 | E — Legibilidade · 🟢 | Não aplicado nesta rodada — usuário não marcou a decisão; permanece pendente. |

Arquivos tocados nesta rodada: `mods/TRL-ImmersiveCombatMedicine/modded-V4/Patches/Medical/BandAidController.cs` (modificado). Build `dotnet build -c Release`: 0 Erros, 0 Warnings.

### Fix compartilhado com o item 025 (2026-09-12) — ver [025-emergencydrop-autocirurgia-06-fix-01.md](../025-emergencydrop-autocirurgia/025-emergencydrop-autocirurgia-06-fix-01.md) e [Fix 02](../025-emergencydrop-autocirurgia/025-emergencydrop-autocirurgia-06-fix-02.md)

Bug reportado em raid durante a validação do item 025 (mãos travadas, `NullReferenceException` em loop) afeta igualmente `EmergencyDrop` deste item, já que ambos chamam `TrySetLastEquippedWeapon` (assíncrono por dentro, `Player.cs:22540-22574`) logo após `DestroyController()`. Fix 01 (guard `_weaponEquipPending`, v1.14.4) se mostrou insuficiente — usuário reproduziu o mesmo travamento sem repetição de ação. Fix 02 (v1.14.5) **removeu `TrySetLastEquippedWeapon()` por completo** de `EmergencyDrop()`: o drop de emergência agora deixa as mãos livres instantaneamente, mas **sem reequipar a arma automaticamente** — o jogador precisa apertar a própria tecla de arma. Isso muda o critério de aceite original ("a arma está pronta pra atirar no mesmo instante") — ver nota na spec funcional deste item.

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-12 | Build concluído via `/code-mod`. `dotnet build -c Release` em `modded-V4`: 0 Erros, 0 Warnings. Versão `1.14.2`. Binário gerado só em `modded-V4/bin/Release/netstandard2.1/` (pasta do jogo não tocada). |
| 2026-09-12 | Aplicação de 1 achado de code-review 01 via `/apply-code-review` — ID: CR-01-01 (ordem `ConsumeSafe`/`ThrowItem` corrigida). Build verde, 0 Erros/0 Warnings. |
| 2026-09-12 | Fix compartilhado (documentado no item 025): guard `_weaponEquipPending` evita colisão com o processo assíncrono nativo de `TrySetLastEquippedWeapon`, corrigindo travamento de mãos reportado em raid — mesmo call site usado por este item. Versão `1.14.4`, build verde. Insuficiente — ver próxima entrada. |
| 2026-09-12 | Fix 02 compartilhado (documentado no item 025): `TrySetLastEquippedWeapon()` removido por completo de `EmergencyDrop` — mãos livres sem reequipar arma automaticamente. Versão `1.14.5`, build verde. Validação in-game pendente. |
