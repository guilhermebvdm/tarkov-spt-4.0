# 031 — Skills em ordem canônica (componente compartilhado) · Kickoff

**Mod:** CustomClasses · **Data:** 2026-06-10 · **Origem:** comparação de UX com o viewer de perfis do RZ
**Épico:** UX do editor (030–035) · **Wave:** UX-W1 (paralelo ao 030) · **Deps:** —

> Brief de kickoff — insumo para `/create-spec 031`. Não é a spec.

## Problema (UX)

No viewer antigo, **todas as skills da tabela canônica aparecem sempre, na MESMA ordem fixa** (Ph→M→C→P, `SKILL_MASTER` em `profiles.js:10-45`), com nível 0 esmaecido (opacity 0.3) e barras coloridas por categoria — trocar de classe mantém cada skill na mesma posição visual, então a comparação é instantânea. No editor atual, o detalhe lista **só as skills definidas, na ordem do JSON**, sem categorias — impossível comparar de cabeça.

> **Sem números mágicos:** a contagem de skills NÃO deve ser hardcoded em spec/UI (kickoff anterior dizia "31", a enumeração soma 32 — divergência real). Fonte da verdade = `SkillWeights.cs` (tabela + categorias já existem); o `SkillMaster` deriva dele e a spec valida a contagem real uma única vez.

## Escopo

- **`SkillMaster.cs`** (server): ordem canônica portada do viewer antigo — Physical (Endurance, Strength, Vitality, Health, StressResistance, Metabolism, Immunity) → Mental (Perception, Intellect, Attention, Charisma, Memory) → Combat (Pistol, Revolver, Assault, Shotgun, Sniper, DMR, Throwing, Melee, RecoilControl, AimDrills, TroubleShooting) → Practical (Surgery, CovertMovement, Search, MagDrills, LightVests, HeavyVests, WeaponTreatment, Crafting, HideoutManagement) + seção extra **Skills-Extended** (FirstAid, FieldMedicine, UsecNegotiations, BearRawpower) no fim. Cores por categoria (port: `#c87c50` Ph / `#7090c8` M / accent C / `#6e9a3f` P).
- **`SkillCanonicalList.razor`** (componente compartilhado): renderiza TODAS as skills na ordem canônica — nível 0 esmaecido com "—", barra de progresso proporcional (0–10 visual; >10 satura) na cor da categoria, separadores de categoria com label, custo por skill inline (nível×peso) e multiplicador XP como chip ±% quando existir. Modo **read-only** (detalhe) e modo **edit** (campo numérico inline por linha — substitui o fluxo "Add skill" por dropdown: **0 = não definida**; menos cliques).
- **Adoção:** `ClassDetail.razor` (painel Skills) e `ClassEdit.razor` (aba Skills; aba Multiplicadores pode aderir ao mesmo layout — decidir na spec).
- **Semântica do nível 0 no round-trip (decidir na spec; recomendação abaixo):** hoje `"Skill": 0` explícito no JSON **registra** a skill com progress 0 + `LastAccess` (≠ ausente — `CustomClassesMod.ApplySkills`). A lista canônica com "0 = não definida" não pode derrubar zeros pré-existentes em silêncio. Recomendação: **preservar zeros que já estavam no arquivo** (a linha nasce "definida em 0", distinguível visualmente) e só não CRIAR zeros novos ao editar. Documentar a decisão no `docs/class-schema.md`.

## Refs

- `tools/tarkov-itemdb/viewer/profiles.js:10-45` (SKILL_MASTER), `:143-177` (renderSkills), `profiles.css:152-214`
- `SkillWeights.cs` (pesos/categorias já existem — reusar, não duplicar), `SkillsExtendedCompat.cs`
- Território: `Web/Shared/Skill*.razor` (novo), `ClassDetail.razor`, `ClassEdit.razor` (030 NÃO toca estes)

## DoD (resumo)

- Detalhe e edição mostram TODAS as skills da tabela canônica (contagem = `SkillWeights.cs`, validada na spec) sempre na mesma ordem/posição, zeros esmaecidos, categorias separadas.
- Editar nível direto na linha (sem dropdown de add); salvar não CRIA zeros novos no JSON (zeros pré-existentes preservados conforme decisão da spec).
