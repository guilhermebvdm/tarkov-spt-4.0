# 002 — Redesign de skills com budget por categoria · As-Built

**Mod:** RZCustomProfiles
**Spec funcional:** [002-custom-profiles-01-spec.md](002-custom-profiles-01-spec.md)
**Multiplicadores:** [002-custom-profiles-00-multiplicadores.md](002-custom-profiles-00-multiplicadores.md)
**Última review técnica:** *(N/A — etapas review/spec-técnica puladas a pedido do usuário)*
**Build inicial:** 2026-05-17

> Documentação **pós-implementação**. Item 002 pulou review/technical-spec/code-review porque a mudança é apenas dados declarativos no script existente (`build-profile-jsons.js`) e JSONs gerados — sem código C# nem nova lógica.

## Arquivos alterados (build inicial)

| Ação | Path | Resumo |
| --- | --- | --- |
| MODIFICADO | `mods/RZCustomProfiles/scripts/build-profile-jsons.js` | `SKILL_MULTS` reduzido às 32 skills vivas + clamp `[0.25, 5.00]` (4 valores subiram pra 3.75); novo `SKILL_CATEGORIES` para validação Ph/M/C/P; cap de 6 skills removido; validação de cobertura adicionada; cleanup de variáveis órfãs (`REPO_ROOT`, `nameOf`). |
| MODIFICADO | `mods/RZCustomProfiles/scripts/build-profile-jsons.js` | Composições das 10 classes atualizadas com skills vivas conforme [01-spec](002-custom-profiles-01-spec.md). |
| RENOMEADO | `operadorNoturno.json` → `operadorFurtivo.json` | NightOps/SilentOps/ProneMovement mortas no SPT 4.0.13 — tema reposicionado pra "stealth diurno" usando CovertMovement+Search+Perception. |
| REGENERADO | `mods/RZCustomProfiles/modded/profiles/*.json` (10 arquivos) | Skills atualizadas. Loadouts/hideout/traders inalterados (fora de escopo do 002). |
| DEPLOYADO | `D:/SPT/SPT/user/mods/RZCustomProfiles/profiles/*.json` | 9 sobrescritos + 1 novo (operadorFurtivo). `operadorNoturno.json` removido do install. |

## Composições finais (skill por classe + custo ponderado)

| Classe | C | Ph | M | P | Custo |
|--------|---|----|----|----|------:|
| Médico de Combate | Assault 5 | Vitality 5, Health 3, StressResistance 4 | Attention 2 | Surgery 7 | 31.83 |
| Caçador | Sniper 8 | Endurance 5 | Perception 6, Attention 4 | CovertMovement 5 | 29.38 |
| Fuzileiro | Assault 10, RecoilControl 4, AimDrills 4 | Endurance 3 | Attention 3 | MagDrills 6 | 29.04 |
| Batedor | Assault 4 | Endurance 5 | Perception 8, Attention 5 | CovertMovement 8, Search 8 | 30.00 |
| Operador Furtivo | Assault 5 | Endurance 5 | Perception 6 | CovertMovement 8, Search 5, MagDrills 4 | 28.71 |
| Armeiro | Assault 4 | Strength 3 | Intellect 6 | WeaponTreatment 8, TroubleShooting 4 | 29.49 |
| Operador Tático | Assault 5, AimDrills 5 | Strength 10, Endurance 7 | Attention 4 | MagDrills 4 | 28.61 |
| Sobrevivencialista | Shotgun 3 | Metabolism 10, Vitality 4, Immunity 2, Health 1 | Perception 3 | Search 4 | 30.61 |
| Saqueador | Assault 2 | Strength 2 | Attention 10, Perception 10, Intellect 8, Memory 5 | Search 10 | 29.98 |
| Gerente de Operações | Shotgun 2 | Strength 4 | Memory 10, Intellect 10, Charisma 10 | Crafting 10, HideoutManagement 10 | 29.88 |

## Decisões importantes durante o build

| Achado | Resolução |
|--------|-----------|
| **StressResistance é Ph (não M)** | Confirmado via screenshot in-game: tag `Ph` no UI. Script rejeitou Médico de Combate inicial (M sem cobertura). Adicionado `Attention 2` ao Médico (cost 30.63 → 31.83, ainda dentro de [28, 32]). Spec 01 e budget table atualizados. |
| **Cap de 6 skills removido** | Usuário rejeitou explicitamente a herança do 001. Saqueador/Gerente mantêm 7 skills; Sobrevivencialista mantém 7. Cobertura mínima Ph/M/C/P + total [28, 32] são as únicas restrições de design vigentes. |
| **20 skills mortas removidas do SKILL_MULTS** | Validação automática: qualquer override de skill morta dispara erro de "Multiplicador não encontrado". Resistência ativa contra regressão. |

## Validações automatizadas

| Validação | Resultado |
|-----------|-----------|
| Custo ponderado ∈ [28, 32] | ✓ todas (28.61 → 31.83) |
| Skill ≤ 10 | ✓ todas |
| Cobertura mínima Ph/M/C/P | ✓ todas |
| Sem skills mortas em uso | ✓ (validação via SKILL_MULTS) |
| Encoding UTF-8 sem BOM | ✓ |
| Slots ≤ 280 | ✓ (213–257, 4 com ⚠️ packing) |

## Validações pendentes (playtest in-game)

- **RecoilControl, Sniper, Memory, Charisma** — multiplicadores ainda por **premissa**, não observação. Em playtest, observar se sobem com taxa esperada.
- **Operador Furtivo** novo no launcher — confirmar visualmente (10 perfis total).
- **Categorização de cada skill in-game** — confirmar que skills caem nas abas que `SKILL_CATEGORIES` espera (especialmente RecoilControl/AimDrills/TroubleShooting na aba C).
- **Sobrevivencialista** — confirmar Immunity 2 aplicado (skill que mais subiu de custo com novo clamp 5.00).

## Histórico

| Data | Evento |
| --- | --- |
| 2026-05-17 | Build concluído via `/code-mod` (etapas review/technical-spec/code-review puladas a pedido — mod é apenas dados declarativos). 10 JSONs regenerados com skills vivas, operadorFurtivo substituiu operadorNoturno. |
