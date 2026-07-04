# 025 — Aposentar código morto + fechar shims Legacy · Spec técnica

> **Data:** 2026-07-04<br>
> **Status:** 🟢 Vivo<br>
> **Responsáveis:** Guilherme<br>
> **Referências:** [00-kickoff](./025-dead-code-legacy-shims-00-kickoff.md) · [AUDIT-2026-07-04](../../AUDIT-2026-07-04-code-product-ds.md) · [01-spec](./025-dead-code-legacy-shims-01-spec.md)<br>

---

## Abordagem (com file:line reais, conferidos)

Três frentes independentes num só item, ordenadas por acoplamento crescente:

### 1. Deletar código morto (sem dependência de migração)

Provas de morte (grep no estado atual em disco):

- **5 custom controls órfãos** — os nomes só aparecem na própria definição (`x:Class`, `AvaloniaProperty.Register<...>`) e em `build_*.txt`. Nenhuma view/`App.axaml`/`ViewLocator` os instancia:
  - `CustomControls/ProfileCard.axaml` + `ProfileCard.axaml.cs`
  - `CustomControls/DetailedProfileCard.axaml` + `.axaml.cs`
  - `CustomControls/TotalModsCard.axaml` + `.axaml.cs`
  - `CustomControls/GameLaunchBar.axaml` + `.axaml.cs`
  - `CustomControls/LoginBox.axaml` + `.axaml.cs`
- **Helpers sem call-site** (`WireGuardHelper.` e `FikaConfigHelper.` retornam 0 ocorrências fora das definições):
  - `Helpers/WireGuardHelper.cs` (classe estática; bypass TLS reportado em `:153`, `WaitForExit` bloqueante; ~274 linhas — remove risco próprio além de peso)
  - `Helpers/FikaConfigHelper.cs` (classe estática)
- **Método morto:** `ViewModels/ProfileViewModel.cs:388-410` `private async Task GameVersionCheck()` — nunca chamado (só a definição casa no grep). Remoção não afeta `InitializeAsync` (`:380-386`, só chama `CheckForUpdates`).

Nenhum desses arquivos está listado manualmente no `.csproj` (Avalonia usa glob) — apagar o arquivo basta; não há `<Compile Remove>`/`<AvaloniaResource>` explícito a limpar (confirmar no diff).

### 2. Migrar consumidores de `.card/.acc/.alt` e podar os shims

Consumidores restantes das classes legadas (grep `Classes="card|acc|alt"` após excluir os 5 órfãos):

- `Views/ModInfoView.axaml` — `Border Classes="card"` (`:22`, `:60`), `Label Classes="acc"` (`:28`, `:66`), `TextBlock Classes="alt"` (`:36`, `:74`).
- `CustomControls/ModInfoCard.axaml` — `Border Classes="card"` (`:8`), `Label Classes="acc"` (`:79`).

Migração:
- `Border Classes="card"` → `cc:TrlPanel` (padrão já usado em `ProfileView.axaml:109/123/143`) **ou** `Border` com `Background="{DynamicResource TrlBgPanelBrush}"`, `BorderBrush="{DynamicResource TrlEdgeBrush}"`, `BorderThickness="1"`, `CornerRadius="0"`, `BoxShadow="{DynamicResource TrlShadow2}"` (equivalência 1:1 ao shim `Legacy.axaml:19-26`).
- `Label/TextBlock Classes="acc"` → `Classes="trl-accent"` (ou `Foreground="{DynamicResource TrlAccentBrush}"`).
- `TextBlock Classes="alt"` → `Classes="trl-muted"` (o shim `.alt` aponta para `TrlAccentDimBrush`; `trl-muted` é o equivalente semântico de texto secundário — validar tom no GATE-INGAME; se precisar do mesmo tan-dim, usar `Foreground="{DynamicResource TrlAccentDimBrush}"`).

Após remover os consumidores, **podar `Legacy.axaml`** (deletar exatamente estes seletores):
- `:11-13` `TextBlock.alt`
- `:14-16` `TextBlock.acc`
- `:19-26` `Border.card`
- `:70-72` `Label.acc`
- `:73-75` `Label.alt`
- `:76-78` `Label.versionMismatch` (só era usado por `ProfileCard.axaml:18` e `DetailedProfileCard.axaml:36`, ambos deletados)

**Manter** (shims vivos, fora do escopo): `WindowNotificationManager` (`:29-32`), `NotificationCard` (`:34-52`), `cc|TitleBar` (`:55-64`), base `Label` (`:67-69`), `Separator` (`:81-84`, usado por Login/Register), `ProgressBar.error` (`:87-99`, ver 01-spec CC-2).

### 3. Correlatos DS

- **`SPTNotificationViewModel.cs:18-45`** — hoje um `switch(Type)` que faz `new SolidColorBrush(Colors.X)`. Refatorar em duas partes para ficar testável:
  - função pura `internal static string MapTypeToToken(NotificationType type)` → devolve a chave (`"TrlAccentBrush"` | `"TrlWarningBrush"` | `"TrlSuccessBrush"` | `"TrlDangerStrongBrush"` | `"TrlFgMutedBrush"`);
  - resolução do brush no ctor via `Application.Current?.TryFindResource(key, out var res)` (Avalonia 11: `ResourceNodeExtensions.TryFindResource`), com fallback `new SolidColorBrush(Color.Parse("#9B9A96"))` se `res` não for `IBrush` (design-time/headless — CC-4).
  Todos os tokens-alvo existem em `Tokens.axaml`: `TrlAccentBrush:39`, `TrlSuccessBrush:51`, `TrlWarningBrush:52`, `TrlDangerStrongBrush:48`, `TrlFgMutedBrush` (usado no tema).
- **`ModUpdateView.axaml:47`** — `CornerRadius="4"` → `CornerRadius="0"`. Alteração de 1 caractere.
- **`TrlFgOnDanger`** — adicionar em `Tokens.axaml` (bloco Status, junto de `:47-52`): `<SolidColorBrush x:Key="TrlFgOnDanger">#FFFFFF</SolidColorBrush>` (branco puro sobre `#D92C20`/`#F04438`/`#A8231A` mantém contraste AA para texto bold — mesma cor de hoje, agora tokenizada). Referenciar em `Button.axaml:70,76,81` e `TitleBar.axaml:90` (`Value="{DynamicResource TrlFgOnDanger}"`).
- **`ImageSourceConverter.cs:19-38`** — trocar o `return new Bitmap(rawUri)` (`:29`) por memoização:
  ```csharp
  private static readonly ConcurrentDictionary<string, Bitmap> _cache = new();
  ...
  return _cache.GetOrAdd(rawUri, p => new Bitmap(p));   // 1 decode por path
  ```
  Mantém o guard de `targetType` e o `try/catch` → `null`. Não há `Dispose` explícito: os bitmaps vivem enquanto o app vive, e o conjunto de paths é limitado (facção/ícones/poucos fundos). Trade-off documentado (01-spec CC-3). Opcional: aceitar `parameter` como largura-alvo e usar `Bitmap.DecodeToWidth` para cortar memória de fundos grandes (Taquila) — deixar como melhoria, não requisito.

## Arquivos a tocar

**Deletar:**
- `CustomControls/{ProfileCard,DetailedProfileCard,TotalModsCard,GameLaunchBar,LoginBox}.axaml` + `.axaml.cs` (10 arquivos)
- `Helpers/WireGuardHelper.cs`, `Helpers/FikaConfigHelper.cs`

**Editar:**
- `ViewModels/ProfileViewModel.cs` — remover `GameVersionCheck` (`:388-410`); decidir `OpenModsInfoCommand` (`:412-413`) conforme GATE-P1.
- `Views/ModInfoView.axaml` — migrar (ou deletar, GATE-P1(c)).
- `CustomControls/ModInfoCard.axaml` — migrar (ou deletar, GATE-P1(c)).
- `Assets/Theme/Controls/Legacy.axaml` — remover 6 seletores de shim.
- `Assets/Theme/Tokens.axaml` — `+ TrlFgOnDanger`.
- `Assets/Theme/Controls/Button.axaml` (`:70,76,81`), `CustomControls/TitleBar.axaml` (`:90`) — usar o token.
- `ViewModels/Notifications/SPTNotificationViewModel.cs` — tokens + seam de teste.
- `Views/ModUpdateView.axaml` (`:47`) — radius 0.
- `Converters/ImageSourceConverter.cs` — memoização.

**Condicional a GATE-P1(c):** também `Views/ModInfoView.axaml.cs`, `ViewModels/ModInfoViewModel.cs` e o botão desabilitado em `ProfileView.axaml:77-82`.

## Contratos / DTOs

- **`ImageSourceConverter.Convert`** — assinatura `IValueConverter` inalterada (`object Convert(object, Type, object, CultureInfo)`); muda só a implementação (memoização). Invariante nova: para o mesmo `rawUri`, retorna a mesma instância `Bitmap`. Continua devolvendo `null` para não-string, `targetType` incompatível ou path inválido.
- **`SPTNotificationViewModel`** — construtor público inalterado. `BarColor` (`IBrush`) passa a vir de token resolvido; novo membro `internal static string MapTypeToToken(NotificationType)` (seam de teste puro).
- **`TrlFgOnDanger`** — novo token `SolidColorBrush` em `Tokens.axaml`; consumido por `DynamicResource`. Aditivo — não altera tokens existentes.

## Riscos

- **R-1 — Binding quebra em runtime, não em build.** Podar um shim antes de migrar o consumidor compila mas quebra na tela. Mitigação: RN-2 (migrar→podar no mesmo diff) + GATE-INGAME abrindo todas as telas.
- **R-2 — `.alt` ≠ `trl-muted` exato.** `.alt` mapeia `TrlAccentDimBrush` (tan-dim); `trl-muted` é cinza-neutro. Se a diferença de tom incomodar, usar `TrlAccentDimBrush` direto. Validar no GATE-INGAME.
- **R-3 — Delete falso-positivo.** Um control poderia ser referenciado por string/reflection. Mitigação: os 5 não têm `ViewModel` correspondente (o `ViewLocator` mapeia por convenção de nome VM→View), e grep cobre `App.axaml`/resources. Baixo risco.
- **R-4 — Memoização servindo bitmap velho.** Só ocorreria se o **conteúdo** de um path mudasse mantendo o mesmo caminho. Fundos de Settings mudam o **path** ao trocar seleção; imagens de facção/ícone são estáticas → sem stale. Baixo risco.
- **R-5 — `TryFindResource` nulo em design-time.** Sem `Application.Current` no preview → NRE. Mitigação: fallback embutido (CC-4).
- **R-6 — Conflito de merge no `Legacy.axaml`/`Tokens.axaml`/`Button.axaml`** com o item 024 (ver Paralelismo).

## Plano de teste

**Unit (`SPT.Launcher.Tests`, xUnit — projeto já existe, ver `Sync/*Tests.cs`):**
- `Notifications/SPTNotificationMapTests.cs` (novo) — `MapTypeToToken` cobre os 5 casos (`Information/Warning/Success/Error/default`) → chave esperada. Função pura, sem Avalonia App → roda headless trivialmente.
- `Converters/ImageSourceConverterTests.cs` (novo) — `Convert` com não-string → `null`; `targetType` incompatível → `null`; path inexistente → `null` (try/catch); **memoização**: dois `Convert` do mesmo path retornam `ReferenceEquals`. Nota: instanciar `Bitmap` exige um PNG real de fixture e pode exigir `Avalonia.Headless` inicializado; se o setup headless for custoso, testar só o ramo de memoização com um seam (`Func<string,Bitmap>` injetável) e cobrir os ramos `null` sem Avalonia.

**Build/estático (GATE-BUILD):**
- `dotnet build SPT.Launcher.csproj -c Release` verde.
- `dotnet test SPT.Launcher.Tests.csproj -c Release` verde.
- Greps de aceite (AC-2, AC-3, AC-6): `WireGuardHelper|FikaConfigHelper|GameVersionCheck` = 0; `Classes="card"|"acc"|"alt"|Classes.acc|Classes.versionMismatch` = 0 em `project/`.

**Manual (GATE-INGAME):** roteiro do 01-spec (telas abrem sem binding error; 4 notificações com cor de tema; radius 0; troca de fundo sem hitch; texto `.danger` legível).

## Paralelismo — arquivos compartilhados com outros itens

| Arquivo | Compartilhado com | Natureza do toque neste 025 | Cuidado |
|---|---|---|---|
| `ViewModels/ProfileViewModel.cs` | **hub de 019-023** (StartGame, optional toggles, delete/wipe, edições) | cirúrgico: remove `GameVersionCheck:388-410`; talvez `OpenModsInfoCommand:412-413` | remoções localizadas; rebase frequente; não tocar os comandos de 019-023 |
| `Assets/Theme/Controls/Legacy.axaml` | **024** (e este 025) | remove 6 seletores de shim | 024 é **dep**: sequenciar 024→025 ou coordenar o diff; mudanças são deleções, alto risco de conflito textual |
| `Assets/Theme/Tokens.axaml` | 024 / DS | **aditivo** (`+TrlFgOnDanger`) | baixo conflito (linha nova no bloco Status) |
| `Assets/Theme/Controls/Button.axaml` | 024 / DS | troca literal→token em `:70,76,81` | conflito só se 024 mexer nas mesmas linhas `.danger` |
| `CustomControls/TitleBar.axaml` | tema/chrome | troca literal→token em `:90` | isolado |
| `Converters/ImageSourceConverter.cs` | **consumido por 004 (ClassSelection), Login, Register, 023/B3 (Settings), Profile, MainWindow** | reescrita interna (memoização) | mudança afeta TODAS as telas com imagem → validar cada uma no GATE-INGAME |
| `Views/ModUpdateView.axaml` | 007/016 (fluxo de sync) | 1 char (`:47` radius) | isolado |
| `SPTNotificationViewModel.cs` | — | tokens + seam | isolado |
| `OptionalModsHelper` | 019/021 | **não tocado aqui** | citado só para excluir do escopo |

## Gates

Ver [01-spec](./025-dead-code-legacy-shims-01-spec.md) (GATE-P1 produto, GATE-BUILD, GATE-INGAME, GATE-COOP). As-build/logs de execução vão para o `05-asbuild` quando o item for codificado.
