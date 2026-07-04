# 011 — Lista de mods · Kickoff

**Launcher:** Launcher4.0beta · **Data:** 2026-07-03 · **Origem:** Trello MTav8H5f item 5 ("Lista de Mods" — sem detalhe no card)

> Brief de kickoff — insumo para `/create-spec`. Não é a spec. **Escopo vago no card — refinar com o usuário antes da spec.**

## Objetivo (hipótese)

Exibir/organizar a lista de mods do servidor no launcher (quais mods a instalação roda, versão, talvez changelog) — confirmar a intenção do card com o usuário.

## Estado atual

- Já existem: [Models/Launcher/ModInfoCollection.cs](../../project/SPT.Launcher.Base/Models/Launcher/ModInfoCollection.cs), [Models/SPT/SPTServerModInfo.cs](../../project/SPT.Launcher.Base/Models/SPT/SPTServerModInfo.cs), [Views/ModInfoView.axaml](../../project/SPT.Launcher/Views/ModInfoView.axaml), [CustomControls/TotalModsCard.axaml](../../project/SPT.Launcher/CustomControls/TotalModsCard.axaml) e [ModInfoCard.axaml](../../project/SPT.Launcher/CustomControls/ModInfoCard.axaml) — mapear o que já funciona e o que o card quer além disso.
