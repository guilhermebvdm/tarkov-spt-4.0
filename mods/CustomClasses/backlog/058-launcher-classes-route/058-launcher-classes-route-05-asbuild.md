# 058 — Rota pública de classes p/ launcher · As-built

**Mod:** CustomClasses
**Status:** Entregue + review 01 aplicada (build verde; validação com server rodando pendente — gate manual)
**Data:** 2026-07-03 (rev. 2 — pós /apply-code-review)
**Spec:** [058-launcher-classes-route-01-spec.md](058-launcher-classes-route-01-spec.md) · Review: [058-launcher-classes-route-04-code-review-01.md](058-launcher-classes-route-04-code-review-01.md) · Contrato SP0: [004-classes-dados-reais-00-kickoff.md](../../../../launcher/Launcher4.0beta/backlog/004-classes-dados-reais/004-classes-dados-reais-00-kickoff.md)

## Entregue

| Arquivo | Ação | Conteúdo |
|---|---|---|
| `modded/Server/ClassListRouter.cs` | **novo** | `[Injectable] StaticRouter`, rota `/customclasses/classes` (padrão `SkillMultipliersRouter`); sessionId ignorado; filtro `Enabled && Registered`; dedupe por editionKey (CR-01-03); skills normalizadas (CR-01-05) |
| `modded/Server/ClassListResponse.cs` | **novo** | DTOs `ClassListItem` + `LocalizedPair` no shape exato do contrato SP0 |
| `modded/Server/ClassEditionKeyRegistry.cs` | **novo** | Singleton `fileName → editionKey` efetiva, gravado no `Commit`, limpo no `Remove` |
| `modded/Server/LauncherLanguageConfig.cs` | **novo (review CR-01-01)** | Singleton com a resolução de língua da chave de edition (ex-privada do boot): `Language` (settings.jsonc, lazy) + `ResolveEditionKey(def)` |
| `modded/Server/ClassRegistrar.cs` | **editado** | +2 params de ctor; `ValidateAndBuild` resolve a chave EFETIVA via `LauncherLanguageConfig` (CR-01-01 — todos os callers ganham a mesma chave); `ResolveEditionKey(def)` público novo; `Commit` grava `SourceFileName → plan.Name`; `Remove` limpa o key registry |
| `modded/Server/CustomClassesMod.cs` | **editado (review CR-01-01)** | Removidos `LoadLauncherLanguage`/`ApplyLauncherLanguage`/`LauncherSettings` — boot sem lógica própria de língua (pipeline resolve) |
| `modded/Server/ClassEditorService.cs` | **editado (review CR-01-01/02)** | `BuildEntry`: flag `Registered` pela chave efetiva (fecha falso-negativo dos chips); `Delete`: hot-remove pela chave efetiva (fecha órfão no key registry) |

Nenhuma página Razor do editor web alterada; assinaturas públicas `ValidateAndBuild`/`Commit`/`Remove` intactas (sessão paralela segura). `mod-backlog.md` não tocado. Sem commits.

## Decisões e assunções

1. **editionKey via registro no `Commit` + resolução de língua NO PIPELINE (CR-01-01, fix estrutural).** A chave real da edition não é `def.name`: com `settings.jsonc language=pt` é `displayName.pt` ("Caçador" etc. — todas as 7 classes têm `name` EN). A resolução (`LauncherLanguageConfig.ResolveEditionKey`) agora vive em `ClassRegistrar.ValidateAndBuild` — **boot, Save/hotApply e Delete produzem a MESMA chave em qualquer `language`**. O `Commit` grava `fileName → plan.Name` no `ClassEditionKeyRegistry`; a rota junta por arquivo. O `name` dentro do arquivo de classe nunca é reescrito.
2. **Consequência do fix estrutural:** o cenário do CR-01-01 (Save+hotApply registrando edition EN transitória "Hunter" → perfis de jogadores com edition fantasma pós-restart) está eliminado — o Save agora comete sobre "Caçador", a mesma chave do boot. O antigo P-058.2 (Registered falso-negativo + hot-apply language-blind) está fechado na raiz.
3. **Gate "Registered" da rota** = mapeamento no key registry + `ClassVisualRegistry.Contains` (ownership canônico). Mais preciso que `entry.Registered` para colisão cross-file (distingue o arquivo dono). Dedupe adicional por editionKey no array (CR-01-03): primeiro arquivo vence (ordem determinística), warning no log. **O launcher (item 004) deve mesmo assim keyar a lista defensivamente** e o contrato SP0 merece a linha "editionKey é única no array" (a incorporar pela sessão do launcher — kickoff 004 não editado aqui).
4. **`skills` normalizadas na rota (CR-01-05):** enum-cased (TryParse ignoreCase + IsDefined) + clamp 0..51, desconhecidas fora — espelha `ClassRegistrar.ApplySkills`; o launcher nunca vê skill/nível que o pipeline não aplicaria. `skillMultipliers` vem do `SkillMultiplierRegistry.Get(editionKey)` (normalizados + clamp ≥ 0, em vigor).
5. **Proveniência mista aceita (CR-01-05 var. 1-2):** `displayName/description/skills` refletem o ARQUIVO atual (cache por mtime); `editionKey/skillMultipliers`, o último Commit. Edit externo sem hot-apply (ex.: `/sync-classes` com server up) pode divergir do template registrado até restart/hot-apply — janela rara, sem corrupção de perfil (o template é a verdade no registro); documentado no doc do router.
6. **DTO próprio `LocalizedPair`** (não `LocalizedText`: o converter colapsa `Pt==null` p/ string legada, quebraria o shape `{en,pt}`).
7. **Nulls omitidos:** `JsonUtil` usa `WhenWritingNull` — `iconUrl`/`nameColor`/`description.en|pt` nulos são **omitidos**. Launcher trata ausente = null.
8. **Performance:** fonte é `GetCachedEntries()` (cache 037) — por request: varredura de diretório + reads de dicionário, zero dry-run.
9. **Resposta comprimida (CR-01-04):** o SPT responde zlib deflate por default (`SptHttpListener.SendZlibJson`) com `Content-Type: application/json` — **o item 004 DEVE consumir via `Request`/`RequestHandler` existentes do launcher** (já descomprimem — `Request.GetJson` → `SimpleZlib.Decompress`), não `HttpClient` cru.

## Como testar manualmente (server rodando)

1. Deploy (build local não chega ao server sozinho): parar o `SPT.Server` → copiar `mods/CustomClasses/modded/Server/bin/Release/net9.0/CustomClasses-Server.dll` para a pasta do mod em `D:\SPT\SPT\user\mods\CustomClasses\` (mesmo processo do editor web) → reiniciar o server.
2. `curl -H "responsecompressed: 0" http://127.0.0.1:6969/customclasses/classes`
   ⚠️ **Sem o header a resposta sai zlib-comprimida (bytes binários no terminal — NÃO é a rota quebrada).** O header ativa o modo debug do SPT (`IsDebugRequest` → JSON plano). Alternativa: pipe para um descompressor zlib.
3. Verificar:
   - Array com as **7 classes**: `editionKey` = "Caçador", "Fuzileiro", "Furtivo", "Médico de Combate", "Peladão", "Saqueador", "Tanque" (ordem alfabética por nome de arquivo).
   - `editionKey` bate 1:1 com as keys do `ProfileTemplates` (as mesmas do `editions[]` de `/launcher/server/connect`).
   - `iconUrl` abre no browser: ex. `http://127.0.0.1:6969/CustomClasses-Server/icons/cacador.png`.
   - Editar um arquivo de classe para `"enabled": false` + restart → classe some da rota.
   - **Regressão CR-01-01:** com o server up, salvar `cacador.jsonc` no editor web (hot-apply) e re-curl → `editionKey` continua "Caçador" (não "Hunter"); `editions[]` do launcher não ganha edition nova.
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

## Build (gate — re-rodado pós-review)

```
dotnet build mods/CustomClasses/modded/Server/CustomClasses.Server.csproj -c Release
  CustomClasses.Server -> ...\modded\Server\bin\Release\net9.0\CustomClasses-Server.dll
  Compilação com êxito.  0 Aviso(s)  0 Erro(s)
```

## Pendências deixadas

- **P-058.1** — Validar a rota com o server rodando (curl com header + 7 classes + `enabled:false` some + regressão CR-01-01) — gate humano do DoD.
- ~~**P-058.2**~~ — **FECHADA** pelo fix estrutural do CR-01-01/02 (resolução de língua no pipeline; `Registered` e hot-remove pela chave efetiva).
- **P-058.3** — Guard de "arquivo dono" no `Save` do editor: `allowReplace=true` ainda perdoa colisão com edition do próprio mod vinda de OUTRO arquivo (Save de arquivo colidido/rename cross-file comete por cima). O dedupe da rota contém o dano no launcher; o guard é escopo do editor — item próprio.
- **P-058.4** — Sessão do launcher (item 004): incorporar ao contrato SP0 a linha "editionKey é única no array" e consumir a rota via `Request`/`RequestHandler` (zlib), não `HttpClient` cru.

## Histórico

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-03 | Claude | Criação (entrega inicial do item 058). |
| 2026-07-03 | Claude | Rev. 2 — review 01 aplicada: fix estrutural CR-01-01/02 (LauncherLanguageConfig no pipeline), dedupe CR-01-03, curl corrigido CR-01-04, skills normalizadas CR-01-05; claim "restart reconverge" removida (o cenário foi eliminado na raiz); P-058.2 fechada; +P-058.3/P-058.4. |
