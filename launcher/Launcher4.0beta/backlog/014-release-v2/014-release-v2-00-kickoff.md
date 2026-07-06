# 014 — Release launcher 2.0.0 · Kickoff

**Launcher:** Launcher4.0beta · **Data:** 2026-07-03 · **Origem:** Trello MTav8H5f item 6.3 · **Deps:** todos os anteriores (fecha o épico)

> Brief de kickoff — insumo para `/create-spec`. Não é a spec.

## Objetivo

Lançar o launcher TRL como **versão 2.0.0** com tudo do card entregue.

## Escopo previsto

- Bump de versão: [SPT.Launcher.csproj:13](../../project/SPT.Launcher/SPT.Launcher.csproj#L13) (`AssemblyVersion`, hoje `1.4.7.0`) + qualquer string de versão hardcoded nas views (ver item 013).
- Build release + empacotamento; distribuição aos jogadores (canal atual de update do launcher — ver [Helpers/LauncherUpdateHelper.cs](../../project/SPT.Launcher/Helpers/LauncherUpdateHelper.cs)).
- Regra da memória do repo: **toda versão oficial nova = incrementar versão (csproj+metadata, rebuild) e sufixar zips com `-v X.Y.Z`**.

## DoD (resumo)

- Launcher reporta 2.0.0 (assembly + UI) e o auto-update entrega a versão nova aos clientes.
