# 056 — Indicador de perk no peso (Pack Mule)

**Mod:** CustomClasses
**Status:** Backlog
**Criado:** 2026-07-03

## Visão geral

Fecha a **"Feature 1"** (peso ↔ perk **Pack Mule**): dar ao limite de carga na aba **Health** o mesmo indicador
"▲ +X% · via Classe" que a aba de Skills já tem (`SkillPanelPatch`). **Escopo mínimo** decidido a partir do
[recon](056-perk-stat-indicators-recon.md): dos ~12 candidatos, o **peso é o único** onde o número exibido **já reflete
o efeito do perk** (os stats de arma/ADS mostram o valor do item, não o bufado, ou não têm número). Só exibição.

## Comportamento atual

- O limite de carga no painel de saúde **já incorpora o +30%** do Pack Mule: o `Max` sai de
  `UpperOverweightLimit × skillManager.CarryingWeightRelativeModifier`, e o `PackMulePatch` postfixa exatamente esse
  `CarryingWeightRelativeModifier` ([PackMulePatch.cs:26](../../modded/Client/Patches/PackMulePatch.cs#L22)). O número
  está certo, mas **nada indica** que o bônus vem da classe.
- O único indicador "▲ +X%" do mod hoje é na aba **Skills** (`SkillPanelPatch`, ao lado do nome da skill).

## Comportamento desejado

Ao lado do valor de peso na aba Health, um marcador **"▲ +X%"** (verde) + **tooltip** de atribuição à classe, quando a
classe local tem **Pack Mule** ativo (Saqueador/Tanque). Reusa `MultiplierFormat.Marker` + `HoverTooltipArea` (mesmo
molde do `SkillPanelPatch`).

## Critérios de aceite

- [ ] Na aba **Health** (fora da raid: stash/inventário), com classe local **Scavenger** ou **Tank** e Pack Mule
      ligado, aparece o marcador **"▲ +X%"** ao lado do peso, com **X = `PackMuleCarryBonus`** (default 30).
- [ ] **Hover** no marcador mostra um tooltip atribuindo o bônus à classe (nome da classe).
- [ ] Classe **sem** Pack Mule, perk **desligado** (`PackMuleEnabled=false`), ou indicador de UI desligado
      (`ShowOnUi=false`) → **sem** marcador.
- [ ] O marcador **não altera** o número do peso (só anota) e **não quebra** o painel nativo (valor, cor de warning,
      layout dos outros parâmetros intactos).
- [ ] **Fika/multiplayer:** `N/A` — UI de menu, **fora da raid** (sem `MainPlayer`); gateia só pela **classe local**
      (mesmo padrão do `PackMulePatch` fora da raid). Cada client vê o seu próprio marcador.
- [ ] **Estado entre raids:** `N/A` — UI de menu recriada a cada abertura do painel (`Show`); sem estado persistente.

## Corner cases

- [ ] **Strength alta** (o piso de +30% não "morde" porque a Strength já dá mais): o marcador indica o **piso garantido
      pela classe** — o tooltip esclarece que é piso. Aceito (não recalcular o efetivo real neste escopo).
- [ ] **Painel reaberto** (Show chamado de novo) / refresh periódico (`Update`): **não duplicar** o marcador
      (idempotência — reusar/reescrever um único marcador).
- [ ] **`_weight` ou o TMP do valor nulo** (painel sem sub-painel de peso): não fazer nada, sem exceção.
- [ ] **Peso em overweight** (valor em cor de warning): o marcador é independente da cor/valor — não interfere.

## Fora de escopo

- [x] **Energy/Hydration ↔ Heavy Frame**: o recon classificou como "talvez" (o jogo já tem seta de taxa nativa) — no
      máximo um tooltip de atribuição, **deferido** (baixo ganho, esforço médio).
- [x] **Stats de arma / ADS / aim-punch / ruído / velocidade**: descartados no recon (número do item, não bufado, ou
      não exibido). O lugar desses sinalizadores é a **aba Perks (053/059)**, não os painéis vanilla.
- [x] Alterar a mecânica do Pack Mule (o efeito já existe no `PackMulePatch`).

## Referências

- [056-perk-stat-indicators-recon.md](056-perk-stat-indicators-recon.md) — recon completo (por que só o peso vale).
- [SkillPanelPatch.cs](../../modded/Client/Patches/SkillPanelPatch.cs) — molde do marcador + tooltip.
- [PackMulePatch.cs](../../modded/Client/Patches/PackMulePatch.cs) — o efeito mecânico + o gate de classe.

## Histórico

| Data | Evento |
|---|---|
| 2026-07-03 | Item criado via `/create-spec` (escopo mínimo = só o peso ↔ Pack Mule, derivado do recon) |
