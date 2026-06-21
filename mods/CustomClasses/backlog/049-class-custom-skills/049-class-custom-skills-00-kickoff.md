# 049 — Skills custom de classe (🧪) · Kickoff

**Mod:** CustomClasses · **Data:** 2026-06-20 · **Origem:** redesign 11→6, Fase 5 ([class-levers.md](../../docs/class-levers.md) §5)
**Wave:** R-W2 · **Deps:** 048

> Brief de kickoff — insumo para `/create-spec 049`. Não é a spec.

## Objetivo

Implementar as signatures/levers **🧪** sobre a infra do 048 — cada uma = slot `ESkillId` revivido + efeito lido do nível:

- **Pack Mule** (Saqueador + Tanque) — peso `×(1−[0.10→0.50])` por nível.
- **Adrenalina** (Fuzileiro) — pós-abate: −recuo/−recarga/−ADS por `3s + 0.5s/nv`.
- **Fôlego de Aço** (Caçador) — prende respiração `×(1+0.1·nv) ≤ ×3`, −sway.
- **Mãos Rápidas** (Saqueador) — busca/loot mais rápido.

## Escopo / Riscos

- **Pack Mule** compartilhada entre 2 classes (mesma skill, gating por `GameVersion`).
- **Fôlego de Aço** toca `GetOxygenCapacityFunc` + Delta de hold-breath (per-player, compõe com o stances).
- ⚠️ **Mãos Rápidas:** verificar se loot instantâneo já é vanilla (ponta solta #7) — se for, vira só velocidade de Search.
- **Adrenalina** é buff temporário pós-evento — confirmar gancho de "abate" (kill event) per-player.

## Refs

- [../../docs/class-levers.md](../../docs/class-levers.md) §5 · [../../docs/class-skill-catalog.md](../../docs/class-skill-catalog.md) (fórmulas reais)
- Item 048 (infra) · [../../../Skills-Extended/modded](../../../Skills-Extended/modded)

## DoD (resumo)

- 4 skills funcionais in-game, escalando com o nível e gated por classe (Pack Mule em 2). Aparecem no menu de Skills.
- Parâmetros de cada efeito expostos no **F12** (defaults da [tabela §6.4](../../docs/class-levers.md); valores `TBD` fixados no playtest) — decisão #8.
- Cada skill custom nova **adicionada ao `SkillMaster.cs`** (aparece no viewer do editor, como a seção "Gems (SE)" do 047) + documentada em class-levers/overview.
