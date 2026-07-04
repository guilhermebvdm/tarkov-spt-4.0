# 009 — Mods opcionais com descrição · Spec (funcional + técnica)

**Launcher:** Launcher4.0beta · **Data:** 2026-07-04 · **Kickoff:** [009-mods-opcionais-descricao-00-kickoff.md](./009-mods-opcionais-descricao-00-kickoff.md) · **Contrato SP0:** congelado 2026-07-03 (descriptor por pasta)

> Spec fundida (funcional + técnica) — sessão autônoma, instrução do coordenador.

## Funcional

Cada mod opcional da seção **MODS OPCIONAIS** da tela logada (`ProfileView`) exibe, abaixo do nome, uma **descrição curta** (PT ou EN conforme o idioma do launcher). Fonte das descrições = **server** (nada hardcoded no launcher): `description.json` em `Launcher-Updater/Opcionais/<grupo>/`, exposto pelo `optionals-list`.

## Estado atual mapeado (pergunta do kickoff: "como o toggle age")

- Toggles são populados do **manifesto principal** (`optionalGroups` do `config.json` do server, pass-through). Ativar = `DownloadOptionalGroupAsync` (baixa os arquivos do manifesto marcados `optionalGroup` via `/download`); desativar = `RemoveOptionalGroupAsync` (deleta os arquivos, ou baixa `offFolders` de `Opcionais/` quando o grupo os define).
- ⚠️ Achado (fora de escopo, registrado): o `GenerateManifestAsync` deste repo **não marca** nenhum arquivo com `optional`/`optionalGroup` — a ativação via `_cachedGroupFiles` só funciona se o config/manifesto de produção tagear os arquivos. Este item não mexe no pipeline de instalação; só em **descrições**. → P-009.2.
- O endpoint `optionals-list` existia mas **nenhum client o consumia** — mudar o shape do response não quebra launcher antigo (retrocompat exigida é a inversa: launcher novo × server antigo).

## Contrato (SP0 congelado + S2)

**Descriptor** — `Launcher-Updater/Opcionais/<grupo>/description.json`:

```json
{ "name": "PiP Disable", "description": { "pt": "…", "en": "…" } }
```

**Response novo do `GET /launcher/mods/optionals-list`:**

```json
{ "folders": [ { "id": "PiPDisable", "name": "PiP Disable", "description": { "pt": "…", "en": "…" } } ] }
```

- `id` = nome da pasta (sempre). Retrocompat server: pasta **sem** descriptor ⇒ `name` = nome da pasta, `description` = `null`. Descriptor inválido (JSON quebrado) ⇒ tratado como ausente + log.
- Retrocompat client: launcher novo × server antigo ⇒ parser tolera o shape antigo `{ "folders": ["A","B"] }` (vira descriptor `{id, name=id, description=null}`) e resposta de erro/timeout ⇒ lista vazia ⇒ descrições ficam como estão (fallback = `description` string do `optionalGroups`, comportamento atual).

## Decisões e assunções

- **D1 — enriquecimento, não substituição.** Os toggles continuam nascendo do `optionalGroups` do manifesto (id/name/estado); o `optionals-list` só **enriquece** `Description` (e `Name` apenas quando o grupo não tem nome). Motivo: o `name` do `config.json` é curado pelo operador e é o vínculo com o estado salvo (`EnabledOptionals[id]`); trocar a fonte do toggle mudaria semântica de ativação — fora do escopo.
- **D2 — join grupo×descriptor por heurística tolerante** (case-insensitive): `descriptor.id == group.id` OU `group.folders` contém `descriptor.id` OU `descriptor.name == group.name`. O layout real (ids "gore"/"hollywood" × pastas "Visceral"/"Hollywood") não é 1:1 garantido — a heurística cobre os três vínculos plausíveis sem exigir migração no server.
- **D3 — idioma**: `DefaultLocale` começa com "Portuguese" ⇒ `pt`, senão `en`; fallback pro outro idioma quando o preferido está vazio.
- **D4 — fetch assíncrono pós-manifesto**: o `optionals-list` é buscado depois de popular os toggles (não bloqueia o fluxo de login/verificação); falha é silenciosa (log warning). `OptionalModToggle.Name/Description` viraram reativos p/ o enriquecimento tardio refletir na UI.
- **D5 — `description.json` não é sincronizado pro jogo**: o `optionals-manifest` (usado pelos `offFolders`) passa a **excluir** o `description.json` da raiz do grupo — descriptor é metadado, não arquivo do jogo.
- **D6 — transporte** = `RequestHandler.RequestOptionalsList()` (mesmo canal do manifesto, `request.RemoteEndPoint`), não o `GetServerBaseUrl()` do helper (que derruba a porta — canal menos confiável).
- **A-009.1 — textos dos 4 descriptors redigidos nesta sessão** (PT+EN, 1-2 frases) em `Launcher-Updater-templates/Opcionais/` — pasta template no repo; **o operador copia** para `Launcher-Updater/Opcionais/<grupo>/` no server e ajusta nomes de pasta se o layout real diferir (Hollywood/PiPDisable/IRL/Visceral são os nomes do card).

## Interação com o sync 007 (pergunta do kickoff)

Sem mudança necessária: opcional desligado não é re-baixado (filtro `IsOptionalGroupEnabled` do planner) nem deletado como extra (paths de TODOS os grupos ficam no set de proteção `manifestPaths`/`GetAllKnownOptionalPaths` — CC3 do 007). O descriptor não entra no manifesto principal, então não afeta hash/sync.

## Análise técnica — PiP Disable × ExternalResolution

Evidência coletada (instalação real `D:\SPT`, ambos os mods presentes):

- **PiP-Disabler v1.4.1** (`com.fiodor.pipdisabler.cfg`): remove a renderização picture-in-picture das miras telescópicas — a visão da luneta passa a ser a câmera principal em tela cheia (mesh surgery + zoom de FOV). Hoje está `Mod Enabled = false`.
- **Dynamic External Resolution v1.1.1 (DERP)** (`com.Shibatsu.DynamicExternalResolution.cfg`): "reduz a resolução da renderização **externa** quando mirando através de mira telescópica" (sampling 50% ou DLSS/FSR UltraPerformance). Hoje `Enable Mod = false`.
- **Conflito teórico claro**: o DERP assume que, mirando com PiP, a imagem externa é só o fundo desfocado atrás da luneta — degradá-la é ganho de FPS barato. Com o PiP-Disabler ativo, a "renderização externa" **é a própria visão da luneta em tela cheia**; se o gatilho do DERP ("aiming through telescopic sight") continuar disparando, a imagem inteira da mira cai pra 50% de resolução (ou UltraPerformance) — borrão exatamente onde o jogador precisa de nitidez.
- **Inconclusivo sem jogo**: não dá pra afirmar por análise estática se o hook do DERP ainda dispara quando o PiP-Disabler suprime a câmera óptica (depende de qual evento o DERP patcheia). → **P-009.1**.
- Mitigação aplicada agora: a descrição do PiPDisable avisa "não recomendado junto com o Dynamic External Resolution".

## Mudanças

| Onde | O quê |
|---|---|
| Server `ModUpdater.cs` | `optionals-list` lê `description.json` por grupo → `{folders:[{id,name,description}]}` (retrocompat: sem descriptor ⇒ name=pasta, description=null); `optionals-manifest` exclui o descriptor |
| Repo (templates) | `TarkovRedLine.Server/Launcher-Updater-templates/Opcionais/{Hollywood,PiPDisable,IRL,Visceral}/description.json` (PT+EN) |
| `RequestHandler.cs` | `RequestOptionalsList()` (já entregue no lote do 008) |
| `Helpers/OptionalModsHelper.cs` | `OptionalFolderDescriptor` + `FetchOptionalsListAsync()` (parse tolerante aos 2 shapes) + `ResolveDescription(descriptor, preferPt)` |
| `ViewModels/OptionalModToggle.cs` | `Name`/`Description` reativos |
| `ViewModels/ProfileViewModel.cs` | `EnrichOptionalDescriptionsAsync()` após popular toggles (join D2, idioma D3) |
| `Views/ProfileView.axaml` | **Só** a seção MODS OPCIONAIS: cada item vira CheckBox + `TextBlock` `.trl-muted` `TrlTextXs` com wrap abaixo do nome (visível só com descrição); tooltip redundante removido |

## Gates

- `dotnet build SPT.Launcher.csproj -c Release` · `dotnet test SPT.Launcher.Tests.csproj -c Release` · `dotnet build TarkovRedLine.Server.csproj -c Release` — verdes. Nunca rodar o exe.

## Pendências

- **P-009.1 — verificar in-game PiP Disable × DERP**: ativar ambos, mirar com luneta telescópica e observar (a) se a imagem em tela cheia perde resolução (borrão/ganho de FPS anômalo), (b) log do BepInEx confirmando ativação do DERP durante ADS com PiP suprimido. Se confirmar: ativar o grupo PiPDisable deve desabilitar `Enable Mod` do DERP (candidato: off-file no próprio grupo ou config no performance pack). Não trava este item.
- **P-009.2 — pipeline de instalação dos opcionais**: manifesto deste repo nunca marca `optional`/`optionalGroup` ⇒ `DownloadOptionalGroupAsync` depende de tagueamento que o `GenerateManifestAsync` não faz. Verificar config de produção e, se preciso, tagear no scan por prefixo das pastas dos grupos.
