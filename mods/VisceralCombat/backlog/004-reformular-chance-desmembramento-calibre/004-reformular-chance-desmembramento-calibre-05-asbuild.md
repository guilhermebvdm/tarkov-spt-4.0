# 004 — Reformular Chance de Desmembramento por Calibre e Parte do Corpo · As-Built

**Mod:** VisceralCombat
**Spec funcional:** [004-reformular-chance-desmembramento-calibre-01-spec.md](004-reformular-chance-desmembramento-calibre-01-spec.md)
**Spec técnica:** [004-reformular-chance-desmembramento-calibre-02-spec-tech.md](004-reformular-chance-desmembramento-calibre-02-spec-tech.md)
**Última review técnica:** [004-reformular-chance-desmembramento-calibre-03-spec-tech-review-01.md](004-reformular-chance-desmembramento-calibre-03-spec-tech-review-01.md)
**Build inicial:** 2026-09-10

## Arquivos alterados (build inicial)

| Ação | Path | Resumo |
| --- | --- | --- |
| MODIFICADO | `mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Combined.Patches/KillPatch.cs` | `DismemberChances` (struct), `ResolveDismemberChance`/`ResolveHeadOutcome` (3 mecanismos C→A→B→0), acumulador de momento multi-projétil, `BurstHead` (novo, efeito "cabeça estourada"), `Postfix` reestruturado pra usar os novos resolvers |
| MODIFICADO | `mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/LimbKillPatch.cs` | Ramo "Dead corpses branch" usa `ResolveDismemberChance`/`ResolveHeadOutcome`; corrigido bug pré-existente de normalização de calibre (`AmmoItemClass.Caliber` sem prefixo vs tabela com prefixo) |
| MODIFICADO | `mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/GameStartedPatch.cs` | Limpa o acumulador de momento no início de cada raid |
| MODIFICADO | `mods/VisceralCombat/modded/VisceralCombat/VisceralCombat/VisceralEntry.cs` | `ParseDismembermentJson` lê o schema aninhado (`dismember_calibers`, `dismember_exceptions`, `dismember_multiprojectile_curve`); `OnDismembermentPacketClient` despacha "head_burst" pra `BurstHead`; versão 3.9.12 → 3.9.13 |
| MODIFICADO | `mods/VisceralCombat/assets/ssh/VD_Calibers.json` | Novo schema: `dismember_calibers` aninhado (29 calibres × 4 partes do corpo, da planilha de referência), `dismember_exceptions` (Barrikada/Zvezda do 23x75), `dismember_multiprojectile_curve` (thresholds placeholder). `bleed_calibers`/`ragdoll_limb_chances`/`light_bleed_calibers`/`heavy_bleed_calibers` mantidos intocados |

## Achados corrigidos durante a implementação (além do previsto na spec técnica)

1. **`AmmoTemplate.Caliber` vs `AmmoItemClass.Caliber`:** o primeiro mantém o prefixo `"Caliber"`, o segundo já vem sem ele (`AmmoItemClass.cs:32`). Isso é um **bug pré-existente real**: o código antigo de `LimbKillPatch.cs` tentava as duas formas mas nunca batia com as chaves `"Caliber..."` da tabela — a chance de desmembramento pós-morte por bala provavelmente **sempre caiu no default `0.5f`**, nunca usou os valores calibrados por calibre. Corrigido com `KillPatch.NormalizeCaliber` aplicado consistentemente.
2. **`AmmoItemClass.Name` é chave de localização, não nome interno** (`Item.cs:392`) — `LimbKillPatch.cs` usa `ammo.AmmoTemplate.Name` pra bater com as chaves de `dismember_exceptions`.
3. **`SkeletonRootJoint` é `Diz.Skinning.Skeleton`, não `Transform`** — `BurstHead` corrigido pra usar `player.PlayerBones.Head.Original` (bone real da cabeça) nos efeitos de sangue.
4. **`OnDismembermentPacketClient` sempre chamava `DismemberLimb`** — corrigido pra despachar pra `BurstHead` quando `packet.bone == "head_burst"`, evitando que um peer remoto encolha a cabeça real do observado por engano.

## PA-NN-MM / CR-NN-MM resolvidos durante o build

| ID | Categoria · Impacto | Resumo da resolução |
| --- | --- | --- |
| PA-01-01 (spec-tech) | C — Erro de Lógica | Contagem dupla de momento na rolagem de cabeça — corrigido no próprio design antes do `/code-mod` |
| PA-01-02 (spec-tech) | C — Erro de Lógica | `FireIndex` em `KillPatch.Postfix` — confirmado disponível via `damageInfo.FireIndex` |
| PA-01-03 (spec-tech) | A — Gap · 🟡 | Thresholds do mecanismo A movidos pro JSON (não hardcoded) |
| PA-01-04 (spec-tech) | B — Edge Case · 🟢 | Ancoragem visual de `BurstHead` — resolvida com `PlayerBones.Head.Original` (melhor que o `TODO confirmar` original) |
| CR-01-01 (code-review) | B — Bug latente · 🟠 | Possível dupla contagem de momento entre `KillPatch.Postfix`/`LimbKillPatch.ProcessLimbKill` em corpos já mortos — **pendente de decisão/validação**, não aplicado nesta rodada |

## Pendências antes de marcar 🟢 Entregue

- [ ] **`CR-01-01` (🟠):** decidir se aplica a guarda de idempotência no acumulador de momento, ou se valida empiricamente que o risco não se concretiza (testar buckshot repetido no mesmo membro de um corpo já morto e comparar a taxa de desmembramento observada contra a curva calibrada).
- [ ] Calibração final: `momentum_min_ns`/`momentum_max_ns`/`head_burst_multiplier` em `VD_Calibers.json` são placeholders (3.0/15.0/1.5) — ajustar com base em teste em jogo.
- [ ] Validação solo completa (ver checklist §9 da spec técnica): calibre fraco nunca desmembra; calibre 12 buckshot acumula corretamente; `.50 BMG` desmembra com chance alta incluindo cabeça; cabeça "estoura" visualmente diferente de "arranca" (confirma `Head_1/2` vs `Head_3`); bloqueio de Boss (item 003) sem regressão; drop de capacete/óculos (item 002) dispara em ambos os efeitos de cabeça.
- [ ] Validação de rede: peer remoto vendo "estourar" corretamente via `BurstHead` (não `DismemberLimb`) através do pacote.
- [ ] `/code-review` rodada 02 (opcional) depois de decidir o `CR-01-01`.

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-10 | Build concluído via `/code-mod` — código implementado e compilado, 4 achados corrigidos durante a implementação (além do previsto na spec técnica) |
| 2026-09-10 | `/code-review` rodada 01: 0 🔴 + 1 🟠 (`CR-01-01`, dupla contagem de momento) + 0 🟡 + 0 🟢 |
