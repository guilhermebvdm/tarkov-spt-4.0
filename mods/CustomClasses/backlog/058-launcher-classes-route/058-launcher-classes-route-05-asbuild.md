# 058 — Rota pública de classes p/ launcher · As-built

**Mod:** CustomClasses
**Status:** Entregue (build verde; validação com server rodando pendente — gate manual)
**Data:** 2026-07-03
**Spec:** [058-launcher-classes-route-01-spec.md](058-launcher-classes-route-01-spec.md) · Contrato SP0: [004-classes-dados-reais-00-kickoff.md](../../../../launcher/Launcher4.0beta/backlog/004-classes-dados-reais/004-classes-dados-reais-00-kickoff.md)

## Entregue

| Arquivo | Ação | Conteúdo |
|---|---|---|
| `modded/Server/ClassListRouter.cs` | **novo** | `[Injectable] StaticRouter` com rota `/customclasses/classes` (padrão do `SkillMultipliersRouter`); sessionId ignorado (pré-registro); filtro `Enabled && Registered` |
| `modded/Server/ClassListResponse.cs` | **novo** | DTOs `ClassListItem` + `LocalizedPair` no shape exato do contrato SP0 |
| `modded/Server/ClassEditionKeyRegistry.cs` | **novo** | Singleton `fileName → editionKey` efetiva, gravado no `Commit` |
| `modded/Server/ClassRegistrar.cs` | **editado (3 micro-edições)** | +1 parâmetro de ctor; `Commit` grava `SourceFileName → plan.Name` no registry novo; `Remove` limpa por edition |

Nenhum arquivo do editor web (Razor/páginas) foi alterado. `mod-backlog.md` não tocado (proibição do processo). Sem commits.

## Decisões e assunções

1. **editionKey via registro no `Commit`, não re-derivação.** A chave real da edition não é `def.name`: com `settings.jsonc language=pt` o boot re-chaveia para `displayName.pt` (`CustomClassesMod.ApplyLauncherLanguage`, **privado**). Todas as 7 classes atuais têm `name` EN ("Hunter") ≠ `displayName.pt` ("Caçador"). Em vez de duplicar a lógica de língua no router, o `ClassRegistrar.Commit` (que conhece `plan.SourceFileName` + `plan.Name` = chave efetiva) grava o mapa `fileName → editionKey` no novo `ClassEditionKeyRegistry`. Garantia por construção: a rota serve exatamente a chave que está no `ProfileTemplates`, em qualquer `language` (`pt`/`en`/`name`).
2. **`entry.Registered` NÃO foi usado como gate** — ele checa `templates.ContainsKey(def.Name)` (`ClassEditorService.cs:200`) e é falso-negativo sob `language=pt` (bug latente pré-existente que também afeta os chips "Registered" do editor web — registrado na spec como fora de escopo, candidato a item próprio). Gate da rota = mapeamento presente no key registry + `ClassVisualRegistry.Contains` (ownership canônico, mesmo critério do `SkillMultipliersRouter`).
3. **`skillMultipliers` vem do `SkillMultiplierRegistry.Get(editionKey)`** (normalizados enum-cased + clamp ≥ 0, em vigor), não do dict cru do arquivo. `skills` vem cru do arquivo (níveis iniciais não existem em registry).
4. **DTO próprio `LocalizedPair`** em vez de `LocalizedText`: o `LocalizedTextConverter` serializa `Pt==null` como string simples (forma legada), o que quebraria o shape `{en,pt}` do contrato.
5. **Nulls omitidos**: o `JsonUtil` do SPT usa `WhenWritingNull` — `iconUrl`/`nameColor`/`description.en|pt` nulos são **omitidos** do JSON. Launcher deve tratar ausente = null.
6. **Performance**: fonte é `GetCachedEntries()` (cache por `(mtime, length)` do item 037) — por request: varredura de diretório + reads de dicionário, **zero dry-run**.
7. **Edge herdado, não corrigido (fora de escopo)**: o hot-apply do editor (`Save`) usa `def.Name` cru sem transform de língua — sob `language=pt` um save+hotApply registraria a edition EN ao lado da PT e re-mapearia o arquivo. A rota serve a chave do último `Commit` do arquivo (consistente com o registrado); restart reconverge.

## Como testar manualmente (server rodando)

1. Deploy (build local não chega ao server sozinho): parar o `SPT.Server` → copiar `mods/CustomClasses/modded/Server/bin/Release/net9.0/CustomClasses-Server.dll` para a pasta do mod em `D:\SPT\SPT\user\mods\CustomClasses\` (mesmo processo do editor web) → reiniciar o server.
2. `curl http://127.0.0.1:6969/customclasses/classes`
3. Verificar:
   - Array com as **7 classes**: `editionKey` = "Caçador", "Fuzileiro", "Furtivo", "Médico de Combate", "Peladão", "Saqueador", "Tanque" (ordem alfabética por nome de arquivo).
   - `editionKey` bate 1:1 com as keys do `ProfileTemplates` (as mesmas que o launcher já recebe em `/launcher/server/connect` → `editions[]`).
   - `iconUrl` abre no browser: ex. `http://127.0.0.1:6969/CustomClasses-Server/icons/cacador.png`.
   - Editar um arquivo de classe para `"enabled": false` + restart → classe some da rota.
   - Sem sessão/login: o curl acima já roda sem cookie/sessionId.

## JSON esperado (exemplo — elemento "Caçador")

```json
{
  "editionKey": "Caçador",
  "displayName": { "en": "Hunter", "pt": "Caçador" },
  "description": {
    "en": "Sniper. Patient and precise. Owns elevated positions, minimises movement and eliminates before being spotted.",
    "pt": "Sniper. Paciente e preciso. Domina posições elevadas, minimiza movimento e elimina antes de ser detectado."
  },
  "iconUrl": "/CustomClasses-Server/icons/cacador.png",
  "nameColor": "#c2973f",
  "skills": { "Sniper": 7, "DMR": 2, "ProneMovement": 3, "Pistol": 2, "Perception": 2, "CovertMovement": 3 },
  "skillMultipliers": { "Sniper": 2.5, "DMR": 1.5, "AimDrills": 1.5, "...": 0.0 }
}
```

## Build (gate)

```
dotnet build mods/CustomClasses/modded/Server/CustomClasses.Server.csproj -c Release
  CustomClasses.Server -> ...\modded\Server\bin\Release\net9.0\CustomClasses-Server.dll
  Compilação com êxito.  0 Aviso(s)  0 Erro(s)
```

## Pendências deixadas

- **P-058.1** — Validar a rota com o server rodando (curl + 7 classes + `enabled:false` some) — gate humano do DoD.
- **P-058.2** — Bug latente pré-existente: `ClassFileEntry.Registered` falso-negativo sob `language=pt` (chips do editor web) + hot-apply language-blind do `Save`/`Remove` — item próprio a criar.
