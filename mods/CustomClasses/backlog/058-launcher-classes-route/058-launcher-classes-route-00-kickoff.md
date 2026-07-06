# 058 — Rota pública de classes p/ launcher · Kickoff

**Mod:** CustomClasses · **Data:** 2026-07-03 · **Origem:** item 004 do backlog do launcher ([004-classes-dados-reais](../../../../launcher/Launcher4.0beta/backlog/004-classes-dados-reais/)) — card Trello MTav8H5f

> Brief de kickoff — insumo para `/create-spec 058`. Não é a spec.

## Objetivo

Rota HTTP pública `GET /customclasses/classes` listando as classes registradas com metadados de display, para o launcher TRL exibir na tela de seleção de classe (pré-registro, sem sessionId). Desenhada para servir também o item **057** (identidade de classe per-player em coop) — registry completo displayName en+pt / ícone / cor.

## Contrato (SP0, congelado 2026-07-03)

Ver contrato completo no kickoff do launcher [004](../../../../launcher/Launcher4.0beta/backlog/004-classes-dados-reais/004-classes-dados-reais-00-kickoff.md): array de `{editionKey, displayName{en,pt}, description{en,pt}, iconUrl, nameColor, skills{}, skillMultipliers{}}`. `editionKey` = chave EXATA em `ProfileTemplates` (com `settings.jsonc language=pt` → `displayName.pt`, ex. "Caçador").

## Abordagem

- Novo `ClassListRouter` (`[Injectable] StaticRouter`), modelo em [SkillMultipliersRouter.cs](../../modded/Server/SkillMultipliersRouter.cs).
- Fonte: `ClassEditorService.GetCachedEntries()` filtrando `Enabled && Registered` (cache por mtime — zero dry-run no hot path).
- `iconUrl` relativo `/CustomClasses-Server/icons/<iconFile>` — os static files do `IModWebMetadata` já servem `wwwroot/icons/` nessa URL (nada a criar).
- ⚠️ Toca `modded/Server/` — coordenar com trabalho paralelo do editor web se houver.

## DoD

- Build do `CustomClasses.Server.csproj` verde; rota responde o contrato com as 7 classes atuais; classes `enabled:false` ou não registradas NÃO aparecem; `editionKey` bate 1:1 com as keys do `ProfileTemplates`.
