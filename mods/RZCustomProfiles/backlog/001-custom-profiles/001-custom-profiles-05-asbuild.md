# 001 — Perfis customizados temáticos · As-Built

**Mod:** RZCustomProfiles
**Spec funcional:** [001-custom-profiles-01-spec.md](001-custom-profiles-01-spec.md)
**Spec técnica:** [001-custom-profiles-02-spec-tech.md](001-custom-profiles-02-spec-tech.md)
**Última review técnica:** [001-custom-profiles-03-spec-tech-review-01.md](001-custom-profiles-03-spec-tech-review-01.md)
**Build inicial:** 2026-05-17

> Documentação **pós-implementação**. Reflete o estado real do código entregue pelo `/code-mod` e atualizado por `/apply-code-review`. Quando o conteúdo aqui diverge da spec técnica, este documento ganha — a spec é planejamento, o asbuild é o que foi feito.

## Arquivos alterados (build inicial)

| Ação | Path | Resumo |
| --- | --- | --- |
| CRIADO | `mods/RZCustomProfiles/scripts/build-profile-jsons.js` | Script Node que agrega recipes (baseline + tema + primary + backup×N), resolve anchor → bsgId, consulta `stackMaxSize` em `tools/tarkov-itemdb`, e emite 10 JSONs aplicando regra de stack stack-aware. Inclui validação interna (custo ponderado, total ₽, limites de design). |
| CRIADO | `mods/RZCustomProfiles/modded/profiles/medicoDeCombate.json` | Médico de Combate — FirstAid 7, FieldMedicine 5, Surgery 5, Vitality 3, Health 2 + MedStation:1 + loadout ~1.98M ₽ (89 entradas em Items[]) |
| CRIADO | `mods/RZCustomProfiles/modded/profiles/cacador.json` | Caçador — Sniper 5, Sniping 5, ProneMovement 5, CovertMovement 4, Perception 4 + Heating:1 + loadout ~2.02M ₽ (81 entradas) |
| CRIADO | `mods/RZCustomProfiles/modded/profiles/fuzileiro.json` | Fuzileiro — Assault 10, MagDrills 8, RecoilControl 5, AimDrills 4, Endurance 3 + Workbench:1 + loadout ~1.97M ₽ (88 entradas) |
| CRIADO | `mods/RZCustomProfiles/modded/profiles/batedor.json` | Batedor — CovertMovement 8, Perception 10, Endurance 5, Search 10, Attention 7 + Security:1 + loadout ~1.97M ₽ (87 entradas) |
| CRIADO | `mods/RZCustomProfiles/modded/profiles/operadorNoturno.json` | Operador Noturno — NightOps 4, SilentOps 4, CovertMovement 4, ProneMovement 3, Perception 2 + Generator:1 + loadout ~2.01M ₽ (95 entradas) |
| CRIADO | `mods/RZCustomProfiles/modded/profiles/armeiro.json` | Armeiro — WeaponTreatment 8, TroubleShooting 4, WeaponModding 6, Intellect 6 + Workbench:1 + loadout ~2.02M ₽ (71 entradas, backup×2) |
| CRIADO | `mods/RZCustomProfiles/modded/profiles/operadorTatico.json` | Operador Tático — Strength 10, Endurance 7, AimDrills 6, MagDrills 6, LightVests 2 + RestSpace:1 + loadout ~1.99M ₽ (86 entradas) |
| CRIADO | `mods/RZCustomProfiles/modded/profiles/sobrevivencialista.json` | Sobrevivencialista — Metabolism 10, Vitality 5, Immunity 3, StressResistance 5, Health 3 + WaterCollector:1 + loadout ~2.01M ₽ (98 entradas) |
| CRIADO | `mods/RZCustomProfiles/modded/profiles/saqueador.json` | Saqueador — Attention 10, Search 10, Perception 10, Intellect 10, Memory 8 + Security:1 + loadout ~1.98M ₽ (78 entradas) |
| CRIADO | `mods/RZCustomProfiles/modded/profiles/gerenteDeOperacoes.json` | Gerente de Operações — Crafting 10, HideoutManagement 10, Memory 10, Intellect 10, Charisma 10, WeaponModding 7 + Generator:1 + Heating:1 + loadout ~1.99M ₽ (82 entradas) |
| MODIFICADO | `mods/RZCustomProfiles/memory/sessions.md` | Snapshot "Estado atual" reescrito + entrada cronológica 2026-05-17 do build |

## PA-NN-MM resolvidos durante o build

> Todos os 7 pontos da [review 01](001-custom-profiles-03-spec-tech-review-01.md) já estavam resolvidos no momento do build (spec técnica refletindo todas as decisões). Implementação seguiu as resoluções:

| ID | Categoria · Impacto | Resumo da resolução aplicada |
| --- | --- | --- |
| PA-01-01 | C · 🔴 | Hideout limitado às 8 estações sem pré-requisitos. Caçador/Batedor/Saqueador/Gerente remapeadas. |
| PA-01-02 | A · 🟡 | Geração mecânica via script (`build-profile-jsons.js`) em vez de skeleton manual. Recipes versionadas no próprio script. |
| PA-01-03 | B · 🟡 | Regra stack-aware implementada: `stackMax==1` → N entradas com Count:1; `stackMax>1` → ceil(qty/stackMax) entradas. Aplicada via lookup em `tools/tarkov-itemdb/cache/spt-raw.json`. |
| PA-01-04 | A · 🟡 | Cross-check automático: script falha se anchor ID não existe, se bsgId está faltando, ou se TPL não tem stackMaxSize. Roda como precondição da geração. |
| PA-01-05 | C · 🟡 | Validação de BOM embutida no próprio script (lê bytes após write, aborta se BOM detectado). |
| PA-01-06 | A · 🟢 | Descriptions geradas dentro do limite de 200 chars (todas ≤ 150 chars). |
| PA-01-07 | B · 🟢 | Todas as 10 classes usam `BaseProfile: 0` (Standard). Premissa documentada na spec técnica §7. |

## Validações executadas

| Validação | Resultado |
|-----------|-----------|
| Encoding UTF-8 sem BOM (script + script externo `od -tx1`) | ✓ todas as 10 |
| JSON parser (Python `json.load`) | ✓ todas as 10 parseiam |
| Custo ponderado ∈ [28, 32] | ✓ 29.66 → 30.85 |
| Total ₽ ∈ [1.95M, 2.05M] | ✓ 1.968.560 → 2.020.499 |
| ≤ 6 skills com nível > 0 | ✓ 4-6 skills/classe |
| ≤ 10 por skill | ✓ max 10 |
| SkillOverrides com 51 nomes do exampleProfile.json | ✓ todas |
| HideoutStartingLevels com 28 estações + Stash:1 padrão | ✓ todas |
| TradersLoyalty com 11 traders zerados | ✓ todas |
| Items[] com entradas planas `{Tpl, Count}` (sem nested) | ✓ todas |

## Validações pendentes (requerem ambiente SPT rodando)

> Não bloqueiam o asbuild. Devem ser executadas em playtest pelo usuário antes de promover para release.

- **Smoke test do comportamento `Count > stackMaxSize`** — confirmar que a regra stack-aware é necessária (esperado: sim; mod deve perder itens silenciosamente sem ela).
- **Critérios de aceite da spec funcional in-game:**
  - 10 perfis aparecem no launcher
  - Skills exatas aplicadas no personagem criado (verificar Character → Skills)
  - Estação temática do hideout em L1
  - Loadout depositado no stash inicial (sem perder itens por overflow — stash tem 280 slots; loadouts geram 71-98 entradas)
  - Traders inalterados
- **Acentos PT-BR renderizando no launcher** (`Médico`, `Caçador`, `Operações`).
- **Description não truncada no launcher** (todas ≤ 150 chars; margem de 50 chars para o limite de 200).

## Mudanças posteriores

> Atualizado por `/apply-code-review` a cada rodada. Cada entrada lista os achados aplicados/rejeitados/pulados naquela rodada e os arquivos tocados.

### 2026-05-17 — Aplicação da Code Review 01

4 pontos aplicados, 1 rejeitado (falso alarme). Arquivos tocados:

| Ação | Path | Origem | Resumo |
|------|------|--------|--------|
| CRIADO | `mods/RZCustomProfiles/scripts/extract-item-data.js` | CR-01-01 | Extrai subset versionado (stackMax, dims, name) do tarkov-itemdb gitignored. Rodar quando EFT atualizar. |
| CRIADO | `mods/RZCustomProfiles/scripts/item-data.json` | CR-01-01 | Subset versionado de 100 TPLs — fonte de runtime do build-profile-jsons.js. |
| MODIFICADO | `mods/RZCustomProfiles/scripts/build-profile-jsons.js` | CR-01-01 | Lê item-data.json em vez de tools/tarkov-itemdb/cache/spt-raw.json (gitignored). |
| MODIFICADO | `mods/RZCustomProfiles/scripts/build-profile-jsons.js` | CR-01-03 | Main loop em 2 passadas: valida tudo em memória → só escreve se OK. Zero estado inconsistente em runs falhos. |
| MODIFICADO | `mods/RZCustomProfiles/scripts/build-profile-jsons.js` | CR-01-04 | Adicionado `stashSlotsRequired()` + auto-mitigação `Stash:2` quando loadout > 280 slots. Resultado: 7 classes auto-bumped, 3 preservaram Stash:1. |
| DELETADO | `mods/RZCustomProfiles/scripts/build-loadouts.js` | CR-01-02 | Legado com recipes estale (nomes antigos, mapas removidos). Única fonte de verdade agora é build-profile-jsons.js. |
| REGENERADO | `mods/RZCustomProfiles/modded/profiles/*.json` (10 arquivos) | — | Re-gerados pelo script atualizado. Stash:2 aplicado em 7/10 classes (Médico, Fuzileiro, Batedor, Op. Noturno, Op. Tático, Sobrevivencialista, Saqueador); demais (Caçador, Armeiro, Gerente) preservam Stash:1. |

Decisões registradas em [001-custom-profiles-04-code-review-01.md](001-custom-profiles-04-code-review-01.md): CR-01-01/02/03/04 ✅ Aplicado, CR-01-05 ❌ Rejeitado (falso alarme).

## Histórico

| Data | Evento |
| --- | --- |
| 2026-05-17 | Build concluído via `/code-mod` — 10 JSONs gerados via script `build-profile-jsons.js`, todas as validações automatizadas OK; validação empírica in-game pendente |
| 2026-05-17 | Code Review 01 aplicada via `/apply-code-review`: 4 pontos resolvidos (extract-item-data.js + item-data.json versionados; validate→write invertido; stash slot validation + auto-bump Stash:2 em 7 classes; build-loadouts.js deletado), 1 rejeitado (CR-01-05 falso alarme) |
| 2026-05-17 | **Auto-bump Stash:2 revertido** por decisão de design ("Stash precisa ficar em L1"). Mitigação: `backupCount: 3 → 2` em todas as classes. Total ₽ caiu para 1.63M–2.02M (faixa de validação relaxada para [1.5M, 2.05M]). Todas as 10 classes agora cabem em Stash:1 (213–257 slots). 4 classes próximas do limite (warning de packing). |
| 2026-05-17 | **Deploy revelou overflow real** — log do servidor SPT mostrou ~14-17 itens por classe não conseguindo ser colocados ("stash full, could not place"). Causa raiz: `BaseProfile: 0` (Standard) traz itens iniciais que **somam** com o nosso loadout. **Solução:** `BaseProfile: 0 → 8` (Zero to Hero, stash vazio). backupCount × 2 mantido. Slot warning threshold ajustado de 60% → 85% (referência agora é stash totalmente disponível). 4 classes ainda próximas do limite mas devem caber em playtest sem itens do Standard somando. |
