# 004 — Tela de classes: dados reais · Spec técnica

**Launcher:** Launcher4.0beta · **Data:** 2026-07-04 · **Spec funcional:** [004-classes-dados-reais-01-spec.md](./004-classes-dados-reais-01-spec.md)

> **Desvio de processo registrado:** a review de spec-tech (artefato 03) foi fundida na code-review pós-código (04), por velocidade do run autônomo. Specs escritas e implementadas na mesma sessão.

## Arquivos

| Arquivo | Ação |
|---|---|
| `SPT.Launcher.Base/Models/TRL/ClassInfo.cs` | **novo** — DTOs `ClassInfo` + `LocalizedPair` (contrato SP0) |
| `SPT.Launcher.Base/Controllers/RequestHandler.cs` | **editar** — `RequestClassList()` |
| `SPT.Launcher.Base/MiniCommon/ImageRequest.cs` | **editar** — `CacheServerImage(route, fileName)` público |
| `SPT.Launcher/ViewModels/ClassSelectionViewModel.cs` | **reescrever** — load real + fallback + fix D1 |
| `SPT.Launcher/Views/ClassSelectionView.axaml` | **reescrever** — restyle TRL |

## 1. DTO client (`SPT.Launcher.Base/Models/TRL/ClassInfo.cs`)

Namespace `SPT.Launcher.Models.TRL`. Propriedades PascalCase + `[JsonProperty]` camelCase (Newtonsoft, mesma lib do resto do Base). Shape 1:1 do contrato SP0: `EditionKey`, `DisplayName`/`Description` (`LocalizedPair {En,Pt}`), `IconUrl`, `NameColor`, `Skills` (`Dictionary<string,int>`), `SkillMultipliers` (`Dictionary<string,double>`). Campos ausentes → null (server omite nulls — as-built 058 §7). Skills/multipliers **sem render** nesta tela (kickoff), ficam no DTO p/ uso futuro.

**Linha incorporada ao contrato (P-058.4):** `editionKey` é única no array (dedupe server-side); o client ainda deduplica defensivamente.

## 2. Rota (`RequestHandler.RequestClassList`)

```csharp
public static string RequestClassList() => request.GetJson("/customclasses/classes");
```

Padrão idêntico aos vizinhos (`RequestConnect` etc.). `Request.GetJson` já descomprime zlib (`SimpleZlib.Decompress`) — exigência do as-built 058 §9 (resposta vem zlib por default; **nunca** `HttpClient` cru).

## 3. Ícones (`ImageRequest.CacheServerImage`)

Método público novo, mesmo shape do `CacheImage` privado: GET raw (`Request.Send`, sem decompress — static files saem crus) → grava em `Image_Cache/<fileName>` → retorna path local ou `null`. Diferenças do `CacheImage`: endpoint = `RequestHandler.GetBackendUrl()` (o backend CONECTADO, e não `Server.Url` de settings) e retorno do path p/ o binding. Cache por rota+arquivo por sessão (reusa `CachedRoutes`).

View liga `IconPath` (path local) via `ImageSourceConverter` existente → `Bitmap`. Nada de bitmap decodificado em thread de fundo segurando handle — o converter roda no measure da UI a partir do arquivo local.

## 4. ViewModel (`ClassSelectionViewModel`)

- `ClassProfile` (item de UI): `EditionKey`, `Name` (pt→en→editionKey), `Description` (pt→en→profileDescriptions→""), `IconPath`, `NameColor`, `NameBrush` (`SolidColorBrush` parseado; null se ausente/inválido), `HasNameColor`, `NameUpper` (p/ `TrlScreenBar`, que exige uppercase do consumidor), `Skills`/`SkillMultipliers` (futuro).
- **Load async:** `WhenActivated` → `Task.Run(LoadClassesAsync)` (padrão do `ConnectServerViewModel:30-36`); guard `_loadStarted` contra reativação. Mutação de `AvailableClasses`/`SelectedClass` via `Dispatcher.UIThread.Post` (ObservableCollection não é thread-safe p/ binding).
- `LoadClassesAsync`: `RequestClassList` → `Json.Deserialize<List<ClassInfo>>` → build (dedupe por `EditionKey`, skip key vazia, download de ícones ainda no thread de fundo). Qualquer exceção/null/vazio → **fallback**: `ServerManager.SelectedServer.editions` + `profileDescriptions` (sem ícone/cor) + `LogManager.Warning`. Ambos vazios → `RegisterErrorMsg` orientando checar conexão.
- `SelectedClass` default = `AvailableClasses[0]`. Remove `LoadMockClasses()` e o índice `[3]`.
- `FinalizeAccountCommand`: envia `SelectedClass.EditionKey`. Remove o bloco morto de "MOCK fallback".
- **Fix D1 (`// ref: 005-D1`):** após `RegisterAsync == OK` e antes do auto-login, `await AccountManager.ChangePasswordAsync(_password)` (senha não-vazia). O `Register` OK já logou e populou `SelectedAccount` (`AccountManager.Register:147` → `Login`), pré-condição do `ChangePassword`. Falha → Warning + notificação, segue (comportamento pré-fix no próximo login).

## 5. View (restyle TRL)

Layout 2 colunas sobre `bg-hero.jpg` + `TrlPhotoOverlayBrush` (padrão pilotos Login/Register do 015/8fa0190):

- **Col 0 (300px, `TrlPanelOverPhotoBrush` + hairline direita `TrlEdgeBrush`):** header `trl-label` "SELEÇÃO DE CLASSE"; `ListBox.trl-nav` (template: ícone 24px quando `IconPath` + nome; nome com `Foreground={Binding NameBrush}` **somente** quando `HasNameColor` — dois TextBlocks alternados por visibilidade, para não sobrescrever com null o foreground por estado do trl-nav); "Carregando classes..." enquanto `IsLoading`; erro `Classes="trl-danger"`; botões `ESCOLHER CLASSE` `.primary` + `VOLTAR` `.ghost`; `TrlVersionFooter` (defaults — 013L liga o dado).
- **Col 1:** `TrlPanel` (`ShowHeader=False`, `Padding=0`, fundo `TrlPanelOverPhotoBrush`) com `TrlScreenBar` no topo (`Title={Binding SelectedClass.NameUpper}`) + `ScrollViewer` com label "DESCRIÇÃO DA CLASSE" e a descrição PT com wrap. Painel invisível com `SelectedClass == null` (`ObjectConverters.IsNotNull`).
- **REMOVIDO:** Vantagens/Desvantagens/Habilidades, painel "[Imagem do Personagem]", footer hardcoded, estilos locais de ListBoxItem com hex, `bg2.png`.
- Zero hex novo; só `{DynamicResource Trl*}` e classes do tema.

## 6. Riscos/assunções

- **A1:** `Register` OK implica `SelectedAccount` setado (via `Login` interno) — verdade no código atual; se `Login` interno falhar o registerResult já não é OK.
- **A2:** rota de static files serve PNG cru (as-built 058: `iconUrl` abre no browser) — `Send` sem decompress é o correto.
- **A3:** `nameColor` legível sobre o tema grafite é responsabilidade do autor da classe (server); client só aplica.
- **A4:** build gate = `dotnet build SPT.Launcher.csproj -c Release`; lock transitório de agente paralelo → retry (3× / 20s).
- **A5:** E2E com server real (7 classes, ícones, registro com edition correta) = gate humano (P-058.1).
