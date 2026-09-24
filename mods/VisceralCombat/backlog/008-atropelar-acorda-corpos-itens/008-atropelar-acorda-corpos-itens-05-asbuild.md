# 008 — Acordar/Empurrar Corpos e Itens por Contato Físico (Atropelar) · As-Built

**Mod:** VisceralCombat
**Spec funcional:** [008-atropelar-acorda-corpos-itens-01-spec.md](008-atropelar-acorda-corpos-itens-01-spec.md)
**Spec técnica:** [008-atropelar-acorda-corpos-itens-02-spec-tech.md](008-atropelar-acorda-corpos-itens-02-spec-tech.md)
**Última review técnica:** [008-atropelar-acorda-corpos-itens-03-spec-tech-review-01.md](008-atropelar-acorda-corpos-itens-03-spec-tech-review-01.md)
**Build inicial:** 2026-09-21

> Documentação **pós-implementação**. Reflete o estado real do código entregue pelo `/code-mod` e atualizado por `/apply-code-review`. Quando o conteúdo aqui diverge da spec técnica, este documento ganha — a spec é planejamento, o asbuild é o que foi feito.

## Arquivos alterados (build inicial)

| Ação | Path | Resumo |
| --- | --- | --- |
| CRIADO | `mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/PlayerContactPushPatch.cs` | Postfix em `Player.OnControllerColliderHit` — acorda/empurra corpos e itens físicos ao contato do jogador humano local, proporcional à velocidade horizontal, sem depender de tiro/granada prévios. |
| MODIFICADO | `mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/GameStartedPatch.cs` | Adicionado `PlayerContactPushPatch.ClearContactPushCooldowns();` ao início de raid, junto aos outros `.Clear()` de estado estático. |
| MODIFICADO | `mods/VisceralCombat/modded/VisceralCombat/VisceralCombat/VisceralEntry.cs` | Registrado `PlayerContactPushPatch` em `Awake()`; tooltips de "Player Body Collision" e "Item Physics" atualizados mencionando a reação por contato; versão `3.11.11` → `3.12.0`. |
| MODIFICADO | `mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.csproj` | `<Version>` `3.11.11` → `3.12.0`. |
| MODIFICADO | `mods/VisceralCombat/PROPRIEDADES.md` | Tooltips de "Player Body Collision" e "Item Physics" atualizados em pt-BR, espelhando o F12. |

## PA-NN-MM resolvidos durante o build

> Pontos da última review técnica que foram **aplicados como parte da implementação** (não como /apply-code-review posterior).

| ID | Categoria · Impacto | Resumo da resolução |
| --- | --- | --- |
| PA-01-01 | C — Erro de Lógica · 🟡 Importante | Já resolvido na spec técnica antes do build — guarda `ParentIsDismembered` antes de chamar `WakeCorpse`; código implementado segue exatamente esse stub. |
| PA-01-02 | B — Edge Case · 🟡 Importante | Já resolvido na spec técnica antes do build — velocidade horizontal (componente Y zerado) usada pra direção/magnitude do empurrão; código implementado segue exatamente esse stub. |
| PA-01-03 | A — Gap · 🟢 Menor | Documental — citações Fika (`FikaPlayer.cs:173`, `ObservedPlayer.cs:217`) já adicionadas à spec técnica antes do build; sem mudança de código necessária. |

## Mudanças posteriores

> Atualizado por `/apply-code-review` a cada rodada. Cada entrada lista os achados aplicados/rejeitados/pulados naquela rodada e os arquivos tocados.

- **2026-09-21 — [06-fix-01](008-atropelar-acorda-corpos-itens-06-fix-01.md):** `ForceMode.VelocityChange` (não-nativo do PhysX em `AddForceAtPosition`) causava empurrão de contato fraco/nulo — trocado por impulso escalado + `ForceMode.Impulse` (nativo). `PlayerContactPushPatch.cs` tocado. Build 3.12.1.
- **2026-09-21 — [06-fix-02](008-atropelar-acorda-corpos-itens-06-fix-02.md):** `Player.OnControllerColliderHit` nunca é chamado pelo jogo (jogador/bots usam `ControllerType.Simple`, não o `CharacterController` nativo da Unity) — reescrito de Harmony patch pra checagem periódica por proximidade (`Physics.OverlapSphere` a cada 0.2s perto do jogador, mesma técnica do `GrenadeItemsPatch`). `PlayerContactPushPatch.cs` reescrito, `GameStartedPatch.cs` e `VisceralEntry.cs` ajustados. Build 3.12.6.
- **2026-09-21 — [06-fix-03](008-atropelar-acorda-corpos-itens-06-fix-03.md):** corpo recém-morto perto do jogador era "ejetado" — múltiplos ossos do mesmo ragdoll (ainda próximos entre si logo após a morte) recebiam empurrão independente cada um. Corrigido deduplicando por corpo (`_processedCorpseRoots`), no máximo 1 empurrão por corpo por checagem. `PlayerContactPushPatch.cs` tocado. Build 3.12.7.
- **2026-09-21 — [06-fix-04](008-atropelar-acorda-corpos-itens-06-fix-04.md):** intensidade de chute agora configurável e separada por tipo de alvo — 2 `ConfigEntry<float>` novos (`CorpseKickIntensity`, `ItemKickIntensity`), substituindo a constante fixa compartilhada `PushIntensity`. `VisceralEntry.cs`, `PlayerContactPushPatch.cs` e `PROPRIEDADES.md` tocados. Build 3.12.8.

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-21 | Build concluído via `/code-mod` — 0 erros, mesmos 20 warnings pré-existentes do mod. Instalado automaticamente em `E:/Tarkov Red Line/BepInEx/plugins/VisceralCombat`; cópia para `E:\Tarkov Red Line - SERVER TEST` pendente de confirmação do usuário. |
| 2026-09-21 | [06-fix-01](008-atropelar-acorda-corpos-itens-06-fix-01.md) aplicado — `ForceMode.Impulse` (nativo) substitui `ForceMode.VelocityChange` (custom, torque imprevisível). Build 3.12.1, 0 erros. |
| 2026-09-21 | [06-fix-02](008-atropelar-acorda-corpos-itens-06-fix-02.md) aplicado — `PlayerContactPushPatch` reescrito de Harmony patch (evento nunca chamado pelo jogo) pra checagem periódica por proximidade. Build 3.12.6, 0 erros. |
| 2026-09-21 | [06-fix-03](008-atropelar-acorda-corpos-itens-06-fix-03.md) aplicado — dedupe por corpo (`_processedCorpseRoots`) evita empurrar múltiplos ossos do mesmo ragdoll na mesma checagem. Build 3.12.7, 0 erros. |
| 2026-09-21 | [06-fix-04](008-atropelar-acorda-corpos-itens-06-fix-04.md) aplicado — `CorpseKickIntensity`/`ItemKickIntensity` (ConfigEntry novos) substituem a constante `PushIntensity` fixa compartilhada. Build 3.12.8, 0 erros. |
