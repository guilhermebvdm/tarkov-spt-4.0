# 001 — Auto-desdobrar fone só no próprio jogador · As-Built

**Mod:** SPT-Foldables
**Spec funcional:** [001-desdobrar-fone-proprio-jogador-01-spec.md](001-desdobrar-fone-proprio-jogador-01-spec.md)
**Spec técnica:** [001-desdobrar-fone-proprio-jogador-02-spec-tech.md](001-desdobrar-fone-proprio-jogador-02-spec-tech.md)
**Última review técnica:** [001-desdobrar-fone-proprio-jogador-03-spec-tech-review-01.md](001-desdobrar-fone-proprio-jogador-03-spec-tech-review-01.md)
**Build inicial:** 2026-09-24

> Documentação **pós-implementação**. Reflete o estado real do código entregue pelo `/code-mod` e atualizado por `/apply-code-review`. Quando o conteúdo aqui diverge da spec técnica, este documento ganha — a spec é planejamento, o asbuild é o que foi feito.

## Arquivos alterados (build inicial)

| Ação | Path | Resumo |
| --- | --- | --- |
| MODIFICADO | `mods/SPT-Foldables/modded/Foldables/Patches/Operations/InRaid/InventoryScreenShowPatch.cs` | `Postfix` passa a capturar o parâmetro `CompoundItem lootItem` de `ItemsPanel.Show` e retorna cedo quando `lootItem != null` (há um container externo — corpo, bot, outro jogador, baú — sendo mostrado junto), evitando a reentrância que corrompia o painel de loot em coop. |
| MODIFICADO | `mods/SPT-Foldables/modded/Foldables/Foldables.cs` | Bump de versão `BepInPlugin` 1.0.3 → 1.0.4 (patch, fix de bug). |
| MODIFICADO | `mods/SPT-Foldables/modded/Foldables/Foldables.csproj` | `<Version>` acompanha o bump acima (1.0.4). |

## PA-NN-MM resolvidos durante o build

> Pontos da última review técnica que foram **aplicados como parte da implementação** (não como /apply-code-review posterior).

| ID | Categoria · Impacto | Resumo da resolução |
| --- | --- | --- |
| PA-01-01 | C — Erro de Lógica · 🔴 | O design original (checar `Player.PlayerInventoryController`/`IsYourPlayer` em `inventoryController`) foi refutado antes de codar — esse parâmetro é sempre o do jogador local em `ItemsPanel.Show`, nunca distinguiria corpo de jogador. Reescrito pra usar `lootItem` (não nulo quando há container externo aberto), que é o parâmetro real que representa a entidade sendo lootada. |

## Mudanças posteriores

> Atualizado por `/apply-code-review` a cada rodada. Cada entrada lista os achados aplicados/rejeitados/pulados naquela rodada e os arquivos tocados.

(vazio inicialmente — preenchido por `/apply-code-review`)

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-24 | Build concluído via `/code-mod` — `Foldables.dll` v1.0.4 (fork `mods/SPT-Foldables/modded/`) |
