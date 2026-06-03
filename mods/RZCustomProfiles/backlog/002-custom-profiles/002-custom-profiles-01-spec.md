# 002 — Redesign de skills com budget por categoria

**Mod:** RZCustomProfiles
**Status:** Backlog
**Criado:** 2026-05-17
**Continua:** [001-custom-profiles](../001-custom-profiles/) (10 JSONs já entregues; este item refina as skills)

## Visão geral

Redesenhar a distribuição de skills das 10 classes do item [001](../001-custom-profiles/) usando dois novos princípios de design descobertos via playtest do item 001:

1. **Eliminar 20 skills mortas** — o SPT 4.0.13 mantém várias skills no schema do `globals.json` como arrays vazios (`[]`), o que significa que o jogo não as expõe no UI nem as aplica. Esse subset inclui skills críticas que tínhamos no 001: `FirstAid`, `FieldMedicine`, `Sniping`, `ProneMovement`, `NightOps`, `SilentOps`, `WeaponModding`, entre outras.
2. **Budget por categoria (Ph/M/C/P)** — em vez de "até 6 skills com teto 10" do 001, cada classe agora tem um **orçamento por categoria** que define seu "DNA" (ex: Sobrevivencialista = Ph-heavy, Saqueador = M-heavy, Armeiro = P-heavy). Combat (C) é obrigatório com pontos na mastery da arma primária da classe.

Como side-effect: a classe "Operador Noturno" precisa ser renomeada para **"Operador Furtivo"** porque suas 3 skills core (NightOps/SilentOps/ProneMovement) estão todas mortas — o tema "noturno" não pode ser representado mecanicamente. A nova identidade é "stealth diurno" usando CovertMovement + Search + Perception.

## Comportamento atual

Os 10 JSONs de [001](../001-custom-profiles/) já estão em produção (deploy validado in-game). Problemas observados:

- **5 de 10 classes** têm skills mortas no `SkillOverrides`: Médico de Combate (FirstAid+FieldMedicine), Caçador (Sniping+ProneMovement), Operador Noturno (NightOps+SilentOps+ProneMovement), Armeiro (WeaponModding), Gerente de Operações (WeaponModding).
- Pontos "perdidos" por classe: 6 a 12 dos ~30 do budget — efetivamente **20-40% das skills setadas não fazem nada**.
- Operador Noturno é caso extremo: ~11 dos 30 pontos em skills mortas, sem substituto vivo que preserve o tema "noturno".
- Distribuição entre categorias era ad-hoc: algumas classes só tinham skills em 1-2 categorias (Fuzileiro 100% C, Saqueador 100% M+P), sem "shape" intencional.

## Comportamento desejado

### Tabela de skills mortas (referência)

Skills em `globals.json` com configuração `[]` (não aplicam no jogo SPT 4.0.13):

| Categoria | Skills mortas |
|-----------|--------------|
| Combat | `SMG`, `LMG`, `HMG`, `Launcher`, `AttachedLauncher` |
| Practical | `Sniping`, `ProneMovement`, `FieldMedicine`, `FirstAid`, `WeaponModding`, `AdvancedModding`, `NightOps`, `SilentOps`, `Lockpicking` |
| Trading | `Freetrading`, `Auctions`, `Cleanoperations`, `Barter`, `Shadowconnections`, `Taskperformance` |

### Skills vivas categorizadas

| Categoria (in-game tab) | Skills usáveis |
|-------------------------|----------------|
| **Ph — Physical** | `Endurance`, `Strength`, `Vitality`, `Health`, `StressResistance`, `Metabolism`, `Immunity` |
| **M — Mental** | `Perception`, `Intellect`, `Attention`, `Charisma`, `Memory` |
| **C — Combat** | `Pistol`, `Revolver`, `Assault`, `Shotgun`, `Sniper`, `DMR`, `Throwing`, `Melee`, `RecoilControl`, `AimDrills`, `TroubleShooting` |
| **P — Practical** | `Surgery`, `CovertMovement`, `Search`, `MagDrills`, `LightVests`, `HeavyVests`, `WeaponTreatment`, `Crafting`, `HideoutManagement` |

### Nova regra de design

1. **Budget total: 28–32 pontos ponderados** (mantido do 001)
2. **Budget por categoria** — cada classe tem `{ Ph, M, C, P }` somando ~30 que define seu "shape"
3. **Cobertura mínima** — todas as 4 categorias têm ≥ 1 ponto na classe
4. **Combat obrigatório alinhado à arma** — pontos em C devem incluir a mastery da arma primária do loadout (ex: Caçador → `Sniper`, Sobrevivencialista → `Shotgun`)
5. **Skills mortas proibidas** — sistema de validação no script rejeita qualquer skill marcada `[]` no `globals.json`

### Per-class category budgets (shape do DNA)

> Valores calculados a partir dos multiplicadores de [002-custom-profiles-00-multiplicadores.md](./002-custom-profiles-00-multiplicadores.md) (BASELINE=15, clamp ampliado para `[0.25, 5.00]`). Os valores aqui são **custos ponderados reais** (não níveis brutos de skill) — definem o "DNA" de cada classe.

| Classe | Ph | M | C | P | Total | Shape |
|--------|---:|---:|---:|---:|------:|-------|
| Médico de Combate | 16.88 | 1.20 | 5.00 | 8.75 | 31.83 | Resistência + medicina |
| Caçador | 5.00 | 7.68 | 12.00 | 4.70 | 29.38 | Combat puro (Sniper) |
| Fuzileiro | 3.00 | 1.80 | 18.60 | 5.64 | 29.04 | Combat extremo |
| Batedor | 5.00 | 10.04 | 4.00 | 10.96 | 30.00 | Recon equilibrado |
| **Operador Furtivo** *(era Op. Noturno)* | 5.00 | 5.28 | 5.00 | 13.43 | 28.71 | Furtividade diurna |
| Armeiro | 1.41 | 4.08 | 4.00 | 20.00 | 29.49 | Especialista P |
| Operador Tático | 11.70 | 2.40 | 10.75 | 3.76 | 28.61 | Físico + Combat |
| Sobrevivencialista | 18.10 | 2.64 | 7.50 | 1.72 | 30.61 | Tank Ph extremo |
| Saqueador | 0.94 | 22.74 | 2.00 | 4.30 | 29.98 | Genius M extremo |
| Gerente de Operações | 1.88 | 15.80 | 5.00 | 7.20 | 29.88 | M + Hideout |

### Composição de skills sugerida (a refinar durante review)

> Cada célula referencia os multiplicadores de [002-custom-profiles-00-multiplicadores.md](./002-custom-profiles-00-multiplicadores.md). **Sem cap de número de skills.** Regras vigentes: cobertura mínima (≥ 1 ponto em cada categoria Ph/M/C/P), C alinhado com a mastery da arma primária, total ∈ [28, 32].

| Classe | C | Ph | M | P | Custo |
|--------|---|----|----|----|------:|
| Médico de Combate | Assault 5 | Vitality 5, Health 3, StressResistance 4 | Attention 2 | Surgery 7 | 31.83 |
| Caçador | Sniper 8 | Endurance 5 | Perception 6, Attention 4 | CovertMovement 5 | 29.38 |
| Fuzileiro | Assault 10, RecoilControl 4, AimDrills 4 | Endurance 3 | Attention 3 | MagDrills 6 | 29.04 |
| Batedor | Assault 4 | Endurance 5 | Perception 8, Attention 5 | CovertMovement 8, Search 8 | 30.00 |
| Operador Furtivo | Assault 5 | Endurance 5 | Perception 6 | CovertMovement 8, Search 5, MagDrills 4 | 28.71 |
| Armeiro | Assault 4 | Strength 3 | Intellect 6 | WeaponTreatment 8, TroubleShooting 4 | 29.49 |
| Operador Tático | Assault 5, AimDrills 5 | Strength 10, Endurance 7 | Attention 4 | MagDrills 4 | 28.61 |
| Sobrevivencialista | Shotgun 3 | Metabolism 10, Vitality 4, Immunity 2, **Health 1** | Perception 3 | Search 4 | 30.61 |
| Saqueador | Assault 2 | Strength 2 | Attention 10, Perception 10, Intellect 8, Memory 5 | Search 10 | 29.98 |
| Gerente de Operações | Shotgun 2 | Strength 4 | Memory 10, Intellect 10, Charisma 10 | Crafting 10, HideoutManagement 10 | 29.88 |

**Único ajuste pelo recálculo com clamp `[0.25, 5.00]`:**

- **Sobrevivencialista** — Immunity ficou 25% mais caro (3.00→3.75). `Health 2→1` apenas, para encaixar no teto de 32 (passaria de 32.28 → ficou em 30.61). Design original de 7 skills preservado.
- **Saqueador, Gerente** — design original intocado (não usam nenhuma das 4 skills afetadas pelo clamp).
- **Outras 7 classes** — sem mudança (nenhuma usa Immunity/LightVests/HeavyVests/DMR).

## Critérios de aceite

- [ ] **Tabela de skills mortas confirmada empiricamente.** Lista das 20 skills com `[]` em `globals.json` é a fonte de verdade. Validar contra a versão exata do SPT em uso (4.0.13 em `D:/SPT/SPT/SPT_Data/database/globals.json`) e marcar `[fonte externa]` no planejamento.
- [ ] **Operador Noturno renomeado para Operador Furtivo** em todos os artefatos: planejamento principal ([../001-custom-profiles/001-custom-profiles-00-planejamento.md](../001-custom-profiles/001-custom-profiles-00-planejamento.md)), script ([../../scripts/build-profile-jsons.js](../../scripts/build-profile-jsons.js)), JSON (`modded/profiles/operadorFurtivo.json` substituindo `operadorNoturno.json`) e memory.
- [ ] **Cada um dos 10 perfis tem skills em todas as 4 categorias** (Ph/M/C/P), com ≥ 1 ponto cada.
- [ ] **Cada perfil tem mastery da arma primária do loadout** em C (ex: Caçador com SV-98 tem `Sniper`, não `Pistol`).
- [ ] **Custo ponderado por categoria bate com a tabela alvo** (±2 pontos por categoria) — validado por script.
- [ ] **Custo ponderado total ∈ [28, 32]** mantido do 001.
- [ ] **Nenhuma skill morta** (das 20 listadas) presente com nível > 0 em qualquer JSON.
- [ ] **Multiplicadores das skills mortas removidos** do `SKILL_MULTS` no script (com comentário explicando). Tabela limpa documentada em [002-custom-profiles-00-multiplicadores.md](./002-custom-profiles-00-multiplicadores.md).
- [ ] **Clamp ampliado para `[0.25, 5.00]`** no script (era `[0.25, 3.00]`). 5 multiplicadores afetados (`Immunity`, `LightVests`, `HeavyVests`, `DMR` sobem para 3.75; `Pistol` permanece 3.00). Ver [02-multiplicadores §Impacto](./002-custom-profiles-00-multiplicadores.md).
- [ ] **Script valida budget por categoria** ao gerar JSONs (issue se categoria ultrapassa orçamento da classe).

## Corner cases

- [ ] **Skill aparentemente viva mas sem efeito visível** — algumas skills no `globals.json` podem ter configuração não-vazia mas hidden no UI. Validar empiricamente in-game antes de confiar (ex: criar personagem com `Throwing: 5` e ver se aparece na aba Skills).
- [ ] **Saiga-9 mastery** — o carbine 9mm do Saqueador. Verificar empiricamente se mastery é `Assault`, `Pistol`, ou outro (SMG está morta).
- [ ] **Mudança de arma primária por classe no futuro** — se Caçador trocar de SV-98 para Mosin, a mastery em C (Sniper vs DMR) precisa acompanhar. Documentar no script para que recipe de loadout e skill C fiquem em sync.
- [ ] **Atualização do SPT** — quando o SPT-AKI patcher liberar uma versão nova (4.0.14+), reauditar skills mortas. A lista pode mudar (skills podem ser ressuscitadas ou outras desativadas).
- [ ] **Rebalanceamento empírico via novo personagem de referência** — o modelo `BASELINE(15)/nivel_observado` foi calibrado com personagem lvl 43. Skills agora restritas ao subset vivo podem ter custos diferentes em comportamento real — observar drift se nova referência aparecer.
- [ ] **Classe-shape extrema** — Saqueador (M 23/30) e Sobrevivencialista (Ph 19/31) são quase mono-categoria. Validar se isso fica balanceado em playtest ou se causa desbalanceamento (ex: Saqueador sem armadura realmente sobrevive?).
- [ ] **Rename de classe quebra saves existentes** — qualquer usuário que já criou um personagem "Operador Noturno" terá inconsistência na referência. Documentar isso no comunicado de mudança.

## Fora de escopo

- **Mudanças no loadout** (baseline, tema, primary, backup) — segue como entregue no item 001. Esta entrega é apenas sobre `SkillOverrides`.
- **Mudanças em HideoutStartingLevels** — segue como item 001.
- **Mudanças em TradersLoyalty** — segue como item 001 (todos em LL1).
- **Re-derivação do modelo de balanceamento** (fórmula `mult = BASELINE/nivel_observado`) — mantido. Apenas a lista de skills aceita é restrita.
- **Adição de novas classes** — limitado às 10 existentes. Renomeação de uma (Op. Noturno → Op. Furtivo) é permitida.

## Referências

- [001-custom-profiles/](../001-custom-profiles/) — item anterior que entregou os JSONs originais e o gerador
- [../001-custom-profiles/001-custom-profiles-00-planejamento.md](../001-custom-profiles/001-custom-profiles-00-planejamento.md) — planejamento principal (modelo de balanceamento + skills + hideout + loadouts)
- [../../scripts/build-profile-jsons.js](../../scripts/build-profile-jsons.js) — gerador que será estendido com validação por categoria
- `D:/SPT/SPT/SPT_Data/database/globals.json` — fonte autoritativa de skills vivas/mortas (`[fonte externa]`)
- [../../modded/profiles/](../../modded/profiles/) — JSONs em produção, a serem regenerados

## Histórico

| Data | Evento |
|---|---|
| 2026-05-17 | Item criado a partir de playtest do 001 — descoberta de 20 skills mortas no SPT 4.0.13 + proposta de budget por categoria |
