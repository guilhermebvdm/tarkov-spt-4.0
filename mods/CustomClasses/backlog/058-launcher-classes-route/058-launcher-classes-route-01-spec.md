# 058 — Rota pública de classes p/ launcher · Spec (fundida)

**Mod:** CustomClasses
**Status:** Entregue (ver adendo pós-review no fim)
**Criado:** 2026-07-03
**Kickoff:** [058-launcher-classes-route-00-kickoff.md](058-launcher-classes-route-00-kickoff.md) · Contrato SP0: [004-classes-dados-reais-00-kickoff.md](../../../../launcher/Launcher4.0beta/backlog/004-classes-dados-reais/004-classes-dados-reais-00-kickoff.md) §"Contrato SP0"

> Régua SDD "médio": spec única fundida (funcional + técnica), implementação, as-built. Execução autônoma — decisões registradas aqui e no as-built.

## Objetivo

`GET /customclasses/classes` — rota HTTP pública (sem sessionId; pré-registro) que devolve o array de classes registradas com metadados de display, no contrato SP0 congelado 2026-07-03. Consumidores: launcher TRL (tela de seleção de classe) e, futuramente, item 057 (identidade de classe per-player em coop).

## Contrato (SP0, congelado — não alterar)

```json
[
  {
    "editionKey": "Caçador",
    "displayName": { "en": "Hunter", "pt": "Caçador" },
    "description": { "en": "...", "pt": "..." },
    "iconUrl": "/CustomClasses-Server/icons/cacador.png",
    "nameColor": "#c2973f",
    "skills": { "Sniper": 7 },
    "skillMultipliers": { "Sniper": 2.5 }
  }
]
```

- `editionKey` = chave **EXATA** da edition no `ProfileTemplates` (com `settings.jsonc language=pt` → `displayName.pt`).
- Só classes `Enabled && Registered`. `iconUrl` relativa (`/CustomClasses-Server/icons/<iconFile>`; os static files do `IModWebMetadata` já servem `wwwroot/` nesse mount — nada a criar); `null`/omitida sem `iconFile`.
- Nota de serialização: o `JsonUtil` do SPT usa `DefaultIgnoreCondition = WhenWritingNull` (`JsonUtil.cs:22`) — campos `null` são **omitidos** do JSON. O launcher deve tratar campo ausente como `null` (ex.: `iconUrl`, `description.en`).

## Fonte de dados

| Campo | Fonte | Nota |
|---|---|---|
| `editionKey` | **`ClassEditionKeyRegistry`** (novo) — mapa `fileName → chave registrada`, gravado em `ClassRegistrar.Commit` | Ver "Resolução da editionKey" abaixo |
| `displayName.en/pt` | `ClassFileEntry.Definition.DisplayName` (fallback: `name`) | via `ClassEditorService.GetCachedEntries()` |
| `description.en/pt` | `Definition.Description` | não existe em nenhum registry — só no arquivo |
| `iconUrl`, `nameColor` | `Definition.IconFile` / `Definition.NameColor` | mesma montagem de URL das páginas web (`Classes.razor:250`) |
| `skills` | `Definition.Skills` (cru; `{}` se ausente) | níveis iniciais; não existe em registry |
| `skillMultipliers` | `SkillMultiplierRegistry.Get(editionKey)` | fatores **normalizados** (enum-cased, clamp ≥ 0) em vigor — preferido ao dict cru do arquivo |

Performance: `GetCachedEntries()` é o hot path do item 037 (cache por `(mtime, length)` — varredura de diretório + reads de dicionário, **zero dry-run** por request).

## Resolução da editionKey (decisão central)

**Problema.** A chave registrada NÃO é `def.name`: o boot (`CustomClassesMod.OnLoad`) lê `config/settings.jsonc → language` e, em `pt`/`en`, re-chaveia a edition para `displayName[lang]` (`ApplyLauncherLanguage`, privado). Estado atual: `language=pt` e **todas as 7 classes** têm `name` EN (ex. `"Hunter"`) ≠ `displayName.pt` (ex. `"Caçador"`) → editions registradas são as PT. Consequência conhecida: `ClassFileEntry.Registered` (que checa `templates.ContainsKey(def.Name)`, `ClassEditorService.cs:200`) é **falso-negativo** nesse cenário — não serve de gate aqui.

**Decisão.** Não re-derivar a língua na rota (frágil; a resolução é privada do boot). Em vez disso, **gravar a verdade no momento do registro**: novo singleton `ClassEditionKeyRegistry` (`fileName → editionKey`), alimentado por `ClassRegistrar.Commit` (que conhece `plan.SourceFileName` + `plan.Name` = chave efetiva) e limpo por `ClassRegistrar.Remove`. A rota junta `entry.FileName → editionKey`; ausência de mapeamento = não registrado (boot skip por colisão/erro, ou removido).

Gate "Registered" da rota = `keyRegistry.GetEditionKey(fileName) != null && visualRegistry.Contains(key)` (o visual registry é o ownership canônico, mesmo critério do `SkillMultipliersRouter`).

**Mudança em arquivo existente (mínima, registrada):** `ClassRegistrar.cs` ganha 1 parâmetro de ctor + 1 chamada em `Commit` + 1 chamada em `Remove`. Nenhum arquivo do editor web (Razor/serviços de página) é tocado. Sancionado pelo kickoff ("prefira método público novo mínimo e registre").

## Edge cases

- **Classe `enabled:false`**: entry filtrada por `Enabled` — não aparece, mesmo que a edition ainda esteja registrada (ex.: desabilitada à mão no disco sem restart). Conforme contrato.
- **Arquivo unparseable**: `Definition == null` → filtrada (e nunca teria mapeamento de Commit).
- **editionKey com acento** ("Caçador", "Médico de Combate", "Peladão"): serializada como string JSON UTF-8 normal — nenhum encoding especial; o launcher usa a chave verbatim no `POST /launcher/profile/register` (mesma forma que `ProfilesUsingEdition` compara Ordinal).
- **`language` ≠ `pt`** (`en`/`name`/settings ausente): nada muda na rota — o Commit grava a chave que o boot resolveu, qualquer que seja a língua. `editionKey` continua batendo 1:1 com `ProfileTemplates`.
- **Colisão cross-file** (dois arquivos com o mesmo `name`): boot registra só o primeiro; o segundo arquivo não tem mapeamento no key registry → filtrado (sem classe fantasma).
- **Hot-apply do editor com `language=pt`** (bug latente PRE-existente, fora de escopo): `ClassEditorService.Save` chama `ValidateAndBuild` com o `def.Name` cru (sem transform de língua) — um save+hotApply registraria "Hunter" como edition NOVA ao lado de "Caçador" e re-mapearia o arquivo p/ "Hunter". A rota passa a servir a chave do último Commit do arquivo (consistente com o que está registrado); um restart reconverge. Registrado como pendência para item futuro, não corrigido aqui.
- **Ícone com nome não-ASCII**: `iconUrl` monta a URL sem encode, idêntico às páginas web existentes — ícones atuais são slugs ASCII; consistência > robustez especulativa.
- **GET sem body**: `StaticRouter.HandleStatic` materializa `EmptyRequestData` quando o body é vazio (`Router.cs`) — GET funciona; `sessionId` é ignorado pelo handler.

## Critérios de aceite (DoD)

- [ ] `dotnet build mods/CustomClasses/modded/Server/CustomClasses.Server.csproj -c Release` verde.
- [ ] Rota responde o contrato com as 7 classes atuais; `editionKey` = chaves PT ("Caçador", "Tanque", "Saqueador", "Peladão", "Médico de Combate", "Fuzileiro", "Furtivo").
- [ ] Classes `enabled:false` ou não registradas não aparecem.
- [ ] Zero dry-run por request (hot path do 037 preservado).
- [ ] Nenhum arquivo do editor web alterado; `ClassRegistrar.cs` só com as 3 micro-edições registradas acima.

## Fora de escopo

- Corrigir o falso-negativo de `ClassFileEntry.Registered` sob `language=pt` (afeta chips do editor web — item próprio).
- Corrigir o hot-apply language-blind do editor (`Save`/`Remove` com `def.Name` cru).
- Vantagens/desvantagens estruturadas (descopado no 004 do launcher); cache HTTP/ETag (server local).

## Arquivos

| Arquivo | Ação |
|---|---|
| `modded/Server/ClassEditionKeyRegistry.cs` | novo — singleton `fileName → editionKey` |
| `modded/Server/ClassListResponse.cs` | novo — DTOs `ClassListItem` + `LocalizedPair` (não usar `LocalizedText`: o converter colapsa `Pt==null` p/ string legada, quebrando o shape) |
| `modded/Server/ClassListRouter.cs` | novo — `[Injectable] StaticRouter`, rota `/customclasses/classes` |
| `modded/Server/ClassRegistrar.cs` | editado — ctor + `Commit` + `Remove` (3 micro-edições) |

## Adendo (pós-review 01, 2026-07-03)

A review adversarial ([04-code-review-01](058-launcher-classes-route-04-code-review-01.md), 1 🔴 + 4 🟡) mudou duas posições desta spec — o as-built rev. 2 é a descrição vigente:

- **§Edge cases "hot-apply do editor" e §Fora de escopo (itens 1-2) SUPERADOS:** o CR-01-01 reclassificou o bug como bloqueador (perfis de jogadores criados na janela ficam com edition fantasma — a claim "restart reconverge" não vale para perfis) e o fix ESTRUTURAL foi aplicado: a resolução de língua saiu do boot para o singleton novo `LauncherLanguageConfig`, consumido por `ClassRegistrar.ValidateAndBuild` — boot, Save/hotApply e Delete produzem a MESMA editionKey; `Registered`/hot-remove do `ClassEditorService` usam a chave efetiva. P-058.2 fechada na raiz.
- **Arquivos adicionais tocados** (além da tabela acima): `LauncherLanguageConfig.cs` (novo), `CustomClassesMod.cs` (lógica privada de língua removida), `ClassEditorService.cs` (`BuildEntry` + `Delete`), `ClassListRouter.cs` (dedupe CR-01-03 + normalização de skills CR-01-05).
- **Teste manual:** curl exige `-H "responsecompressed: 0"` (resposta default é zlib — CR-01-04).
