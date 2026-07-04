# 004 — Tela de classes: dados reais (CustomClasses) · Kickoff

**Launcher:** Launcher4.0beta · **Data:** 2026-07-03 · **Origem:** Trello MTav8H5f itens 3 e 3.2 ("Imagem e textos das Classes → [CustomClass]")
**Deps:** 003 (UI entregue) · **Item irmão:** a parte server vive no backlog do CustomClasses (criar via `/add-backlog-item CustomClasses` — ⚠️ toca `mods/CustomClasses/modded/Server/`)

> Brief de kickoff — insumo para `/create-spec`. Não é a spec. Núcleo do card do Trello.

## Objetivo

Substituir o mock da tela de classes por dados reais servidos pelo CustomClasses: lista, imagem/ícone, descrição PT, e (a decidir) vantagens/desvantagens e habilidades — fechando o loop registro→edition correta.

## Estado atual (mapeado 2026-07-03)

- **Launcher** só recebe `editions[]` + `profileDescriptions{}` do endpoint vanilla `/launcher/server/connect` ([RequestHandler.cs:31-34](../../project/SPT.Launcher.Base/Controllers/RequestHandler.cs#L31), [ServerInfo.cs](../../project/SPT.Launcher.Base/Models/SPT/ServerInfo.cs)). Registro: `POST /launcher/profile/register` com `{username, password, edition}`.
- **CustomClasses** registra 7 classes como editions pela chave `displayName.pt` (ex.: "Caçador", "Tanque") — `config/settings.jsonc` com `"language": "pt"`. Schema já tem `description` (PT/EN), `iconFile`, `nameColor`, `skills`, `skillMultipliers` (doc canônica: `mods/CustomClasses/docs/class-schema.md`).
- **Não existe endpoint público de lista de classes.** Só `POST /customclasses/skill-multipliers` (`SkillMultipliersRouter.cs`), que exige sessionId logado — não serve pré-registro. O item **057 do backlog do CustomClasses** (registry completo p/ identidade coop) precisa de rota parecida — avaliar unificar.
- **Ícones**: 13 PNGs em `mods/CustomClasses/modded/Server/wwwroot/icons/` — rota HTTP estática p/ consumo externo a confirmar/criar.
- **Vantagens/desvantagens não existem estruturadas** no schema — decisão de design: derivar de `skillMultipliers` (>1 = vantagem, <1 = desvantagem) vs campos novos `advantages`/`disadvantages` (PT/EN) vs híbrido (deriva + override).

## Escopo previsto

1. **Server (item irmão no CustomClasses):** rota pública `GET /customclasses/classes` — lista de classes habilitadas com metadados de display (displayName en/pt, description en/pt, iconFile, nameColor, skills, skillMultipliers) + acesso HTTP aos ícones.
2. **Launcher:** client HTTP da rota; popular `AvailableClasses` no `ClassSelectionViewModel` (remover `LoadMockClasses`); carregar imagem por ícone; render das vantagens/desvantagens conforme decisão de design.
3. **Fallback:** servidor sem CustomClasses → degradar para `editions[]`+`profileDescriptions{}` vanilla (sem imagem/atributos).

## DoD (resumo)

- Tela lista **exatamente** as classes servidas pelo server (nada de classe fantasma), com imagem e descrição PT.
- Escolher classe → perfil criado com a edition correta (validar no server).
- Sem o mod no server, a tela degrada sem crash.
