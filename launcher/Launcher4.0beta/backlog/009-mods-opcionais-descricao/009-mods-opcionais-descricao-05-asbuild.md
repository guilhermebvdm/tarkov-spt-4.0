# 009 — Mods opcionais com descrição · As-built

**Launcher:** Launcher4.0beta · **Data:** 2026-07-04 · **Specs:** [00-kickoff](./009-mods-opcionais-descricao-00-kickoff.md) · [01-spec (fundida)](./009-mods-opcionais-descricao-01-spec.md)

> Desvio de processo registrado: sessão autônoma (Wave 3) — spec fundida e reviews dispensadas por instrução do coordenador. Mudanças server saíram em lote coordenado com o item 008 (mesmo `ModUpdater.cs`).

## O que foi construído

### Server — `mods/TarkovRedLine4.0/Server/.../Controllers/ModUpdater.cs`

- **`GET /launcher/mods/optionals-list`** (contrato SP0/S2): lê `description.json` de cada pasta em `Launcher-Updater/Opcionais/` e devolve `{ folders: [ { id, name, description: { pt, en } } ] }`. Retrocompat server: pasta sem descriptor ⇒ `name` = nome da pasta, `description = null`; descriptor com JSON inválido ⇒ tratado como ausente + log (nunca derruba o endpoint).
- **`optionals-manifest`** passou a **excluir** o `description.json` da raiz do grupo — descriptor é metadado, não pode ser baixado pro jogo pelos fluxos de `offFolders`.

### Templates dos descriptors (pasta template no repo — operador copia pro server)

`mods/TarkovRedLine4.0/Server/TarkovRedLine.Server/Launcher-Updater-templates/Opcionais/<grupo>/description.json`, textos PT+EN redigidos nesta sessão (A-009.1):

| Grupo | name | Resumo PT |
|---|---|---|
| `Hollywood/` | Hollywood Effects | Efeitos cinematográficos de sangue e impacto, estilo filme de ação |
| `PiPDisable/` | PiP Disable | Desativa o picture-in-picture das miras telescópicas p/ ganhar FPS; avisa "não recomendado junto com o Dynamic External Resolution" |
| `IRL/` | IRL | TarkovIRL — imersão realista (movimentação, câmera, som) |
| `Visceral/` | Visceral | Visceral Combat — gore e desmembramento |

### Client

| Arquivo | Mudança |
|---|---|
| `SPT.Launcher.Base/Controllers/RequestHandler.cs` | `RequestOptionalsList()` (entregue no lote do 008 — decisão D6: mesmo canal do manifesto, não o `GetServerBaseUrl()` do helper que derruba a porta) |
| `Helpers/OptionalModsHelper.cs` | `OptionalFolderDescriptor` (Id/Name/DescriptionPt/DescriptionEn) + `FetchOptionalsListAsync()` com **parse tolerante aos 2 shapes** (novo com objetos; antigo `folders: ["A"]` ⇒ descriptor sem descrição — retrocompat com server antigo; erro/timeout ⇒ lista vazia) + `ResolveDescription(descriptor, preferPt)` (D3: pt se `DefaultLocale` começa com "Portuguese", fallback cruzado) |
| `ViewModels/OptionalModToggle.cs` | `Name` e `Description` **reativos** (`RaiseAndSetIfChanged`) — o enriquecimento tardio reflete na UI |
| `ViewModels/ProfileViewModel.cs` | `EnrichOptionalDescriptionsAsync()` disparado após popular os toggles (não bloqueia login/verificação); join grupo×descriptor com heurística D2 (`descriptor.id==group.id` ∨ `group.folders∋descriptor.id` ∨ `descriptor.name==group.name`, case-insensitive); `name` do descriptor só entra quando o grupo não tem nome (D1 — o `name` do `optionalGroups` é curado pelo operador) |
| `Views/ProfileView.axaml` | **Só a seção MODS OPCIONAIS** (restyle do UI-pack intocado no resto): cada item virou `StackPanel` com o `CheckBox` (nome) + `TextBlock` `.trl-muted` `TrlTextXs` com `TextWrapping="Wrap"` abaixo, visível apenas quando há descrição (`StringConverters.IsNotNullOrEmpty`); tooltip redundante removido. Zero hex |

## Análise PiP Disable × ExternalResolution (pergunta do card)

Evidência da instalação real `D:\SPT` (ambos os mods presentes, ambos **desligados** hoje):

- **PiP-Disabler v1.4.1** (`com.fiodor.pipdisabler.cfg`) elimina a renderização picture-in-picture — a visão da luneta vira a câmera principal em tela cheia (mesh surgery + zoom de FOV).
- **Dynamic External Resolution v1.1.1 / DERP** (`com.Shibatsu.DynamicExternalResolution.cfg`) reduz a resolução da renderização **externa** "when aiming through the telescopic sight" (50% sampling ou DLSS/FSR UltraPerformance).
- **Conflito teórico claro**: com PiP desativado, a renderização "externa" É a visão da luneta em tela cheia — se o gatilho do DERP continuar disparando, a mira inteira perde resolução exatamente quando o jogador precisa de nitidez. **Inconclusivo sem teste in-game** (depende de qual evento o DERP patcheia e se ele ainda dispara com a câmera óptica suprimida) ⇒ **P-009.1**, item não travado. Mitigação imediata: aviso na descrição do PiPDisable.

## Decisões e assunções (detalhe na spec)

1. **D1 — enriquecimento, não substituição**: toggles continuam nascendo do `optionalGroups` do manifesto; `optionals-list` só agrega descrição.
2. **D2 — join heurístico tolerante** (ids de grupo × nomes de pasta não são 1:1 garantidos no layout real).
3. **D3 — idioma pt/en pelo `DefaultLocale`** com fallback cruzado.
4. **D4 — fetch assíncrono e falha silenciosa** (launcher novo × server antigo funciona: descrições ficam como estão).
5. **D5 — `description.json` excluído do `optionals-manifest`** (metadado).
6. **A-009.1 — nomes das pastas template** = os 4 do card (Hollywood, PiPDisable, IRL, Visceral); o operador ajusta se o layout real de `Opcionais/` diferir.
7. **Achado registrado (fora de escopo)**: o `GenerateManifestAsync` deste repo nunca marca arquivos com `optional`/`optionalGroup`, então a ativação via manifesto depende de tagueamento que não existe aqui ⇒ **P-009.2**.

## Gates

```
dotnet build SPT.Launcher.csproj -c Release            → 0 Erro(s)
dotnet build TarkovRedLine.Server.csproj -c Release    → 0 Erro(s)
dotnet test  SPT.Launcher.Tests.csproj -c Release      → Aprovado! 48/48, 0 falhas
```

Obs.: uma rodada intermediária ficou vermelha (4 falhas em testes do 007) por **corrida com edição concorrente** de outra wave (review CR-01-03: `config-server` saiu do fallback do resolver e os testes estavam sendo ajustados no mesmo momento); re-rodada após a convergência ⇒ 48/48. Nenhuma falha relacionada aos itens 008/009.

## Pendências

- **P-009.1 — verificar in-game PiP Disable × DERP** (ver spec §análise): ativar ambos, ADS com luneta, observar perda de resolução em tela cheia + log BepInEx do DERP. Se confirmar: ativar PiPDisable deve desligar `Enable Mod` do DERP (candidatos: off-file do grupo ou config no performance pack do 008).
- **P-009.2 — pipeline de instalação dos opcionais**: verificar no server de produção como os arquivos ganham `optional`/`optionalGroup` no manifesto; se não ganham, tagear no scan por prefixo das pastas dos grupos (sem isso `DownloadOptionalGroupAsync` ativa "nada").
- **P-009.3 — E2E**: copiar os 4 templates pro server real, `GET /launcher/mods/optionals-list` e validar as descrições renderizadas na ProfileView (wrap, idioma, grupo sem descriptor).
