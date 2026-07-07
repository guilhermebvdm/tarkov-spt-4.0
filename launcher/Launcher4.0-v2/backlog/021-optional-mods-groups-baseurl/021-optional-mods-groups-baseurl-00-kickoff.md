# 021 — Mods opcionais: grupos faltantes + base-URL + descrição + I/O · Kickoff

**Launcher:** Launcher4.0beta · **Data:** 2026-07-04 · **Origem:** [AUDIT-2026-07-04](../../AUDIT-2026-07-04-code-product-ds.md) (009 + GetServerBaseUrl) · **Severidade:** 🟡 (aceite parcial do 009) · **Deps:** 009

> Brief de kickoff — insumo para `/create-spec`. Não é a spec.

## Objetivo
Completar o aceite do item 009 (4 toggles + descrição em todos) e corrigir o download que falha em silêncio.

## Achados
- **Toggles PiP Disable e IRL não existem:** `optionalGroups` do server tem só `gore`/`grass`/`hollywood`; os templates `PiPDisable/` e `IRL/description.json` ficam órfãos (nunca renderizam). O card exige os 4.
- **Descrição nova só alcança `hollywood`:** o join por nome de pasta casa só `hollywood`; `gore` cai no fallback do `config.json` antigo, `grass` nem tem template. "Descrição em todos" só passa via fallback legado.
- **`GetServerBaseUrl` derruba porta e força http** (`OptionalModsHelper.cs:45-57`): GET em `http://host:80/...` → exceção engolida como Warning → mod aparece "ativado" mas nada baixa. **Gap de coop:** assets divergem entre clientes sem erro visível.
- **I/O + MD5 na UI thread** (`OptionalModsHelper.cs:255,354,368`): trava a UI em grupos grandes.
- **PiP × ExternalResolution** é só texto na descrição, não lógica (deferido P-009.1, exige teste in-game).

## Critérios de aceite (seed)
- Grupos `PiPDisable` e `IRL` presentes e renderizando toggle; descrição de **todos** via a fonte nova (nomes de pasta alinhados aos templates).
- `GetServerBaseUrl` preserva esquema+porta reais (como o `RequestHandler`); download falho → erro **visível**, não silencioso.
- Download/gravação/hash **off-thread**.
