# 007 — Desmaio 2.0: gatilhos percentuais · As-Built

**Mod:** TRL-ImmersiveCombatMedicine
**Spec funcional:** [007-desmaio-percentual-01-spec.md](007-desmaio-percentual-01-spec.md)
**Spec técnica:** [007-desmaio-percentual-02-spec-tech.md](007-desmaio-percentual-02-spec-tech.md)
**Última review técnica:** [007-desmaio-percentual-03-spec-tech-review-02.md](007-desmaio-percentual-03-spec-tech-review-02.md)
**Build inicial:** 2026-07-19

> Documentação **pós-implementação**. Reflete o estado real do código entregue pelo `/code-mod` e atualizado por `/apply-code-review`. Quando o conteúdo aqui diverge da spec técnica, este documento ganha — a spec é planejamento, o asbuild é o que foi feito.

## Arquivos alterados (build inicial)

| Ação | Path | Resumo |
| --- | --- | --- |
| CRIADO | `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Trauma/TraumaBlackoutTrigger.cs` | Helper estático stateless `Evaluate(Player, EBodyPart, float preHitHp)`: piso absoluto + percentual pré-tiro + gate de analgésico (imunidade total no tórax, redução 50%→25% na cabeça) + roll fixo + log `[Blackout2]`. |
| MODIFICADO | `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Trauma/HealthPatches.cs` | Prefix ganhou `EBodyPart bodyPartType, out float __state` (captura HP pré-hit só p/ Chest/Head, antes de `ApplyDamage` mutar); Postfix ganhou `float __state`, bloco `isChestTrauma`/`isHeadTrauma` (limiar fixo ≥35/≥10) removido e substituído por `shouldFaint` com filtro explícito `bodyPartType == Chest/Head` (PA-02-05) + `ConfigConsumerBlackout2.Value` + `TraumaBlackoutTrigger.Evaluate`. Restante do arquivo (blocos legados de pernas/braços/estômago, comentários) intocado. |
| MODIFICADO | `mods/TRL-ImmersiveCombatMedicine/modded/TRLImmersiveCombatMedicinePlugin.cs` | 4 `ConfigEntry<float>` novas (seção "11. Trauma 2.0 (Desmaio)": `ConfigBlackoutChestPercent`/`ConfigBlackoutHeadPercent`/`ConfigBlackoutChestAbsoluteFloor`/`ConfigBlackoutHeadAbsoluteFloor`); rename-at-delivery de `ConfigConsumerBlackout2` ("Blackout 2.0 (item 007)" → "Blackout 2.0", default `true`, tooltip real); bloco novo em `MigrateOrphanedConfigKeys` deletando o órfão sem copiar valor (padrão replicado de `:407-428`, bloco "Stomach Effects"); versão `1.7.0` → `1.8.0` no `[BepInPlugin]` e no log do Awake. |
| MODIFICADO | `mods/TRL-ImmersiveCombatMedicine/modded/TRL-ImmersiveCombatMedicine.csproj` | `<Version>1.7.0</Version>` → `<Version>1.8.0</Version>`. |
| MODIFICADO | `mods/TRL-ImmersiveCombatMedicine/PROPRIEDADES.md` | Seção "11. Trauma 2.0 (Desmaio)" nova (4 entries); linha JÁ EXISTENTE "Blackout 2.0 (item 007)" na seção 6 atualizada (nome → "Blackout 2.0", default `false`→`true`, tooltip placeholder → tooltip real); linha nova na tabela "Renomeadas"; entrada no Histórico de Alterações. |
| MODIFICADO | `mods/TRL-ImmersiveCombatMedicine/backlog/mod-backlog.md` | Status do item 007: ⚪ → 🟢. |
| MODIFICADO | `mods/TRL-ImmersiveCombatMedicine/backlog/007-desmaio-percentual/007-desmaio-percentual-02-spec-tech.md` | Checklist §8: 6 dos 8 itens marcados `[x]` (implementação); os 2 itens fora do escopo deste `/code-mod` (regenerar grafo, compilar) deixados `[ ]` com nota explícita. |

## PA-NN-MM resolvidos durante o build

> Nenhum achado da última review técnica (rodada 02) foi resolvido DURANTE este build — todos os 5 (PA-02-01 a PA-02-05) já haviam sido aplicados **na spec técnica antes do `/code-mod`** (ver histórico de `02-spec-tech.md`, entrada "Review técnica 02 ... aplicada"). A tabela abaixo lista os 5 só para rastreabilidade — a spec técnica consumida pelo `/code-mod` já refletia o estado pós-review, e o código foi escrito diretamente a partir dela sem re-derivar nenhuma decisão.

| ID | Categoria · Impacto | Resumo da resolução (na REVIEW, não neste build) |
| --- | --- | --- |
| PA-02-01 | Risco/dependência · 🟡 | Prova por decompile real (`ilspycmd`) de que `BringBackConcussion`/`VisceralCombat` não mutam HP antes do nosso `__state` — substituiu a garantia genérica do isolamento do Harmony. Código não muda por causa deste achado (já era o comportamento correto); apenas fortalece a evidência no §7. |
| PA-02-02 | Documentação · 🟢 | Tabela §3 ganhou coluna "Campo C#" explícita, separada do "Nome (EN)" exibido no F12 — evita mismatch entre nome de campo e nome de tela. Implementado literalmente: os 4 campos novos usam exatamente os nomes dessa coluna. |
| PA-02-03 | Documentação · 🟢 | Citação do bloco-molde de migração corrigida para `Plugin.cs:407-428` (bloco "Stomach Effects" do item 006) — o bloco de migração do 007 foi escrito replicando esse padrão exato (delete-antes-do-save, sem copiar valor, mesma mensagem de log). |
| PA-02-04 | Documentação · 🟢 | Identificada a linha JÁ EXISTENTE `Blackout 2.0 (item 007)` na seção 6 do `PROPRIEDADES.md` a atualizar (em vez de tratar como entrada nova) — aplicado literalmente nesta implementação. |
| PA-02-05 | Correção de código · 🟡 | Filtro explícito `(bodyPartType == EBodyPart.Chest \|\| bodyPartType == EBodyPart.Head)` adicionado ao `shouldFaint` do Postfix, antes de chamar `Evaluate` — a spec técnica já continha esse filtro no stub §5; implementado literalmente em `HealthPatches.cs`, mantendo o `else` de domínio dentro de `TraumaBlackoutTrigger.Evaluate` como defesa secundária (não removido). |

## Mudanças posteriores

> Atualizado por `/apply-code-review` a cada rodada. Cada entrada lista os achados aplicados/rejeitados/pulados naquela rodada e os arquivos tocados.

**Code-review rodada 01** (`007-desmaio-percentual-04-code-review-01.md`): CR-01-01 (🟠, aplicado) — spec técnica §7 corrigida: `Priority.High` = 600 (não 200) e a regra real do HarmonyX (maior valor executa primeiro entre Prefixes, confirmado por decompile de `0Harmony.dll`) substituiu uma regra invertida citada por engano; a conclusão prática ("nosso Prefix roda primeiro") já estava correta, só a prova estava errada. CR-01-02 (🟢, aplicado) — `HealthPatches.cs`: Prefix reordenado para capturar `__state` só após o gate `ConfigMasterEnabled`/`IsAlive` (otimização, sem mudança de comportamento). Build v1.8.0 recompilado após os fixes.

**Code-review rodada 02** (`007-desmaio-percentual-04-code-review-02.md`, última planejada): 0🔴 · 0🟠 · 0🟡 · 1🟢. CR-02-01 (🟢) — este próprio arquivo (`05-asbuild.md`) não listava os fixes da rodada 01; resolvido com esta entrada. Nenhum bug encontrado em nenhuma das duas rodadas. Item 007 FECHADO 🟢.

## Verificação de desvios

Nenhum desvio da spec técnica: os stubs §5 (`TraumaBlackoutTrigger.cs` e os trechos de `HealthPatches.cs`) foram implementados literalmente, incluindo o filtro explícito de `bodyPartType` (PA-02-05) e os nomes de campo C# exatos da tabela §3.

## Histórico

| Data | Evento |
| --- | --- |
| 2026-07-19 | Build concluído via `/code-mod` |
