# 004 — Tela de classes: dados reais · Code review 01

**Launcher:** Launcher4.0beta
**Commit revisado:** working tree (não commitado), branch `feat/launcher-2.0`
**Data:** 2026-07-04
**Reviewer:** adversarial, contexto limpo (não escreveu o código)
**Specs:** [01-spec](./004-classes-dados-reais-01-spec.md) · [02-spec-tech](./004-classes-dados-reais-02-spec-tech.md) · As-built: [05-asbuild](./004-classes-dados-reais-05-asbuild.md) · Rota server: as-built 058 do CustomClasses

**Arquivos revisados (escopo):** `SPT.Launcher.Base/Models/TRL/ClassInfo.cs` (novo), `SPT.Launcher.Base/Controllers/RequestHandler.cs` (só `RequestClassList`), `SPT.Launcher.Base/MiniCommon/ImageRequest.cs` (só `CacheServerImage`), `SPT.Launcher/ViewModels/ClassSelectionViewModel.cs`, `SPT.Launcher/Views/ClassSelectionView.axaml`.
**Fontes de verdade consultadas:** `MiniCommon/Request.cs`, `MiniCommon/Json.cs`, `Controllers/AccountManager.cs`, `Controllers/LogManager.cs`, `Models/SPT/ServerInfo.cs`, `ViewModels/ViewModelBase.cs`, `ViewModels/RegisterViewModel.cs`, `ViewModels/ConnectServerViewModel.cs`, `Views/ClassSelectionView.axaml.cs`, `Converters/ImageSourceConverter.cs`, `CustomControls/TrlPanel.cs·TrlScreenBar·TrlVersionFooter`, tema (`Tokens.axaml`, `Typography.axaml`, `Text.axaml`, `ListBox.axaml`, `Button.axaml`, `TrlCustomControls.axaml`), VM antigo em `HEAD` (diff de regressão), `zlib.net.dll` (reflexão + round-trip).

**Resultado do build (gate):** ✅ `dotnet build launcher/Launcher4.0beta/project/SPT.Launcher/SPT.Launcher.csproj -c Release` — **0 erro(s)**, 150 aviso(s); grep de warnings por `ClassInfo|ClassSelection|ImageRequest|RequestHandler` = **vazio** (zero warnings nos arquivos do escopo; a diferença vs 146 do as-built vem de arquivos de outros tracks em curso no working tree).

**Contagem:** 0 🔴 · 6 🟡 · 2 🟢 (notas)

---

### CR-01-01 [🟡] Trecho fallback→publicação roda fora de qualquer catch dentro de task fire-and-forget — exceção ali deixa "Carregando classes..." eterno SEM fallback

**Onde:** `ClassSelectionViewModel.LoadClassesAsync` (linhas 189-211). O `try` cobre só request+deserialize+`BuildFromServer`; o bloco `if (classes == null …) → LogManager.Warning → BuildFromEditionsFallback()` e o `Dispatcher.UIThread.Post` ficam fora. O chamador é `WhenActivated → Task.Run(async …)` sem observador — exceção vira unobserved task exception (engolida em silêncio).

**Cenário concreto:** `LogManager.Write` é `File.AppendAllLines` direto, sem lock e sem try (`LogManager.cs:59-67`), num singleton usado por N threads. O `Warning("[ClassSelection] Falling back…")` da linha 191 dispara exatamente no momento em que a rede caiu — o mesmo momento em que ConnectServer/ping/mod-sync também estão gravando erros no mesmo arquivo em threads paralelos. `AppendAllLines` abre com `FileShare.Read`; o segundo escritor concorrente leva `IOException` (sharing violation) → a exceção escapa do trecho sem catch → a task morre antes do `Post` → `IsLoading` fica `true` para sempre, lista vazia, sem mensagem de erro. Recuperável só via VOLTAR (o botão não é gated por `IsLoading`), mas o fallback vanilla — o requisito central do AC-8 — nunca acontece.

**Fix:** envolver o corpo inteiro de `LoadClassesAsync` num `try/catch` de última instância que garanta o `Dispatcher.UIThread.Post` final (com `IsLoading = false` + `RegisterErrorMsg` orientando retry) mesmo quando o próprio fallback falhar. Bônus barato: `lock` interno no `LogManager.Write` (fora do escopo deste item, mas é o vetor mais plausível).

---

### CR-01-02 [🟡] Clique em ESCOLHER CLASSE no estado vazio apaga a única orientação da tela

**Onde:** `FinalizeAccountCommand` (linhas 113-115): `RegisterErrorMsg = "";` executa ANTES do guard `if (SelectedClass == null) return;`.

**Cenário concreto:** rota falhou E `SelectedServer.editions` vazio → tela mostra "Nenhuma classe disponível. Verifique a conexão com o servidor e tente novamente." (única orientação visível). Usuário clica ESCOLHER CLASSE → mensagem some → comando retorna em silêncio → usuário fica com sidebar vazia, sem loading, sem erro, sem pista. A spec (corner case `SelectedServer == null` + rota falhou) pede "lista vazia + mensagem de erro na tela; botão não faz nada" — o botão faz algo: destrói a mensagem.

**Fix:** mover o guard para antes da limpeza (`if (SelectedClass == null) return; RegisterErrorMsg = "";`) — 1 linha trocada, sem efeito colateral (o comando não roda sem seleção de qualquer forma).

---

### CR-01-03 [🟡] Ícones baixados sequencialmente ANTES de publicar a lista — pior caso ~105 s de loading com os dados já em mãos

**Onde:** `BuildFromServer` → `CacheIcon` inline no loop (linha 247); a lista só chega à UI no `Post` após o loop completo. `Request.Send` tem `Timeout = 15000` (`Request.cs:37`).

**Cenário concreto:** rota de classes responde OK (payload pequeno), mas a rota de static files está lenta/pendurada (Tailscale reconectando, half-open — cenário citado no próprio comentário do timeout). 7 classes com ícone × até 15 s cada, em série = até ~105 s de "Carregando classes…" segurando nomes/descrições que já estão em memória. A spec-tech aceitou "download de ícones ainda no thread de fundo", mas não que a publicação da lista fique atrás deles.

**Fix:** publicar a lista primeiro (sem `IconPath`) e preencher os ícones num segundo passo (Post por item ao concluir — exigiria `ClassProfile` reativo para `IconPath/HasIcon`), OU baixar os 7 em paralelo (`Task.WhenAll`) para colapsar o pior caso em ~15 s. A segunda opção é menor e não mexe no shape do item.

---

### CR-01-04 [🟡] Duplo-load entre instâncias do VM: corrida real em `CacheServerImage`/`CachedRoutes` → ícones aleatoriamente nulos

**Onde:** guard `_loadStarted` é **por instância** (`ClassSelectionViewModel:62`); cada `NavigateTo(new ClassSelectionViewModel(…))` cria VM novo com load novo. `ImageRequest.CachedRoutes` é `List<string>` **estática** sem lock, e `File.Create(filePath)` não tolera escritor concorrente no mesmo path.

**Cenário concreto:** rede lenta (não morta): VM#1 está no meio do download dos 7 ícones; usuário clica VOLTAR → Register → avança de novo → VM#2 inicia `LoadClassesAsync` em paralelo (nada cancela o load do VM#1 — o `CompositeDisposable` do `WhenActivated` não é usado). Os dois threads chamam `CacheServerImage` para as mesmas rotas: (a) `File.Create` no mesmo `class_cacador.png` → `IOException` no perdedor → catch → retorna null → classe sem ícone na tela nova, aleatório por timing; (b) `List.Add`/`Contains` concorrentes em `CachedRoutes` (também em corrida com `CacheSideImage` do ProfileViewModel pós-login) podem corromper a lista interna. Dano contido pelos try/catch (degrada para ícone ausente), mas é corrida real introduzida por este item — primeiro consumidor multi-thread da infra.

**Fix:** trocar `CachedRoutes` por `ConcurrentDictionary<string, byte>` (ou `lock` nos dois métodos) e tratar "arquivo já existe e não-vazio" como cache hit antes do `File.Create`. Alternativa no VM: promover `_loadStarted` a algo compartilhado é errado (estado por navegação); a correção certa é na infra do `ImageRequest`.

---

### CR-01-05 [🟡] Cache de ícone não é isolado por servidor — troca de server na mesma sessão serve ícone stale

**Onde:** `CacheServerImage` — chave de cache = rota relativa (`CachedRoutes`) + arquivo em `Image_Cache/` global.

**Cenário concreto:** dev mode com switch de servidor na mesma sessão do launcher (server local ↔ produção): ambos servem `/CustomClasses-Server/icons/cacador.png`, mas com artes diferentes. Sessão conecta no server A → ícone cacheado e rota marcada; troca para server B → `CachedRoutes.Contains(route)` = hit → retorna o arquivo do server A. Entre sessões se autocorrige (`CachedRoutes` zera e o re-download sobrescreve). A assunção A5 do as-built cobre colisão de basename entre CLASSES do mesmo server — não este caso. Produção TRL é single-server (URL via gist), então o impacto real é dev-only.

**Fix (barato):** incluir o backend na chave de sessão (`CachedRoutes.Add($"{RequestHandler.GetBackendUrl()}{route}")`) — 1 linha; o arquivo em disco pode continuar compartilhado (o re-download sobrescreve). Ou apenas registrar como limitação conhecida no as-built (dev-only).

---

### CR-01-06 [🟡] `CacheServerImage` é API pública mas confia no caller para sanitizar `fileName` — defesa em profundidade ausente

**Onde:** `ImageRequest.CacheServerImage(route, fileName)` → `Path.Combine(ImageCacheFolder, fileName)` sem validação.

**Análise de traversal (caller atual):** NÃO há traversal explorável hoje — o único caller (`ClassSelectionViewModel.CacheIcon`) aplica `Path.GetFileName(info.IconUrl)` antes, o que remove separadores `/` e `\` (um `iconUrl` malicioso `../../evil` vira basename inofensivo; basename vazio é guardado). Um `iconUrl` com `:` no basename (`class_C:evil.png`) vira alternate data stream NTFS **contido dentro do Image_Cache** ou `IOException` capturada → null. `route` absoluto (`http://evil…`) quebra o `new Uri(RemoteEndPoint + url)` → exceção capturada → null.

**Cenário concreto do risco residual:** o método é público com nome genérico; um caller futuro (outro item passando um campo do server direto como `fileName`, ex. `displayName`) reabre `..\..\` sem ninguém notar — o contrato "quem chama sanitiza" não está nem no doc-comment.

**Fix:** dentro do método, `fileName = Path.GetFileName(fileName);` + rejeitar `Path.GetInvalidFileNameChars()` (ou trocar por hash da rota). Uma linha fecha a classe inteira de bug para sempre.

---

### CR-01-07 [🟢] 404/timeout vira `NullReferenceException` proposital no caminho do fallback — log mascara a causa raiz

`Request.Send` retorna `null` em erro de rede (logando o status real em `[Request]`), e `GetJson` faz `stream.CopyTo` sem null-check → NRE → capturada pelo catch do `LoadClassesAsync`, que loga `"Failed to load /customclasses/classes: Object reference not set…"`. O fallback funciona (verificado — é o caminho normal de 404), mas quem for depurar P-004.3 vai ver um NRE em vez de "404". Nota: padrão pré-existente da infra (todo `GetJson` da base se comporta assim); opcional logar `ex.GetType().Name` junto ou null-checkar em `RequestClassList`.

---

### CR-01-08 [🟢] Micro-notas (sem ação obrigatória)

1. `Task.Run` aninhado redundante: `WhenActivated → Task.Run(LoadClassesAsync)` e dentro `await Task.Run(() => RequestClassList())` — segundo salto de thread desnecessário; inofensivo.
2. Notificação do fix D1 com string PT hardcoded ("Não foi possível salvar sua senha agora…") em vez de `LocalizationProvider` — consistente com o restante do arquivo (que já mistura), mas destoa das chaves usadas em `profile_created`.
3. Ícone baixado corrompido (server responde 200 com corpo não-PNG): arquivo é gravado, `HasIcon=true`, `ImageSourceConverter` falha no `new Bitmap` → catch → `Source=null` → espaço vazio de 24px na linha. Autocorrige na próxima sessão (re-download sobrescreve). Cosmético.
4. `ClassProfile.Skills/SkillMultipliers` carregados e nunca lidos — intencional (kickoff: DTO para uso futuro), só registrando que não há dead-binding na view.

---

## Áreas verificadas e limpas

1. **Threading Avalonia (foco 1 do review):** ✅ todas as mutações de `AvailableClasses`, `SelectedClass`, `IsLoading` e `RegisterErrorMsg` do caminho de load acontecem dentro de um único `Dispatcher.UIThread.Post`; `ClassProfile` é imutável após construção (thread de fundo constrói, UI só lê); `ImmutableSolidColorBrush` correto para brush criado fora da UI thread; `FinalizeAccountCommand` roda no contexto da UI (continuations pós-await voltam ao sync context do Avalonia) — `SendNotification`/`RegisterErrorMsg` na thread certa. A view é `ReactiveUserControl<ClassSelectionViewModel>` com `WhenActivated` no code-behind — a ativação do VM dispara de verdade. Guard `_loadStarted` fecha reativação da MESMA instância (ativações sempre na UI thread — sem corrida). Navegar para fora no meio do load não crasha: o `Post` tardio muta um VM vivo na navigation stack, sem view — inócuo (o resíduo real do duplo-load é o CR-01-04, na infra de imagem).
2. **Contrato/parse (foco 2):** ✅ DTO 1:1 com o SP0 — `[JsonProperty]` camelCase explícito, `LocalizedPair {en,pt}`, `skills` int / `skillMultipliers` double; nulls omitidos pelo server (`WhenWritingNull`) viram propriedades null e TODO acesso é null-safe (`DisplayName?.Pt`, `Description?.En`, `Skills` sem render). Cadeias de fallback batem com AC-1/AC-2 (nome pt→en→editionKey; descrição pt→en→profileDescriptions→""). Dedupe defensivo por `editionKey` ordinal (P-058.4) com warning, primeira ocorrência vence, entry sem key é pulada. **UTF-8 verificado empiricamente:** round-trip na própria `zlib.net.dll` do projeto (`CompressToBytes(s, 9, UTF8)` → `Decompress(bytes, null)`) devolve `"Caçador"` byte-idêntico — o default de encoding do `SimpleZlib` com `null` é UTF-8; acentos da editionKey sobrevivem o `GetJson`. Lista vazia (0 classes habilitadas) → fallback vanilla; ambos vazios → mensagem + VOLTAR funcional (tela utilizável; ressalva CR-01-02).
3. **Fix D1 (foco 3):** ✅ usa a MESMA senha digitada (`_password` flui intocado de `RegisterViewModel.RegisterPassword` → ctor → `ChangePasswordAsync(_password)` → `LoginModel.Password`). Senha vazia é impossível no fluxo real (`RegisterViewModel` bloqueia `IsNullOrWhiteSpace` antes de navegar) e ainda assim há guard `!string.IsNullOrEmpty`. Falha na troca → Warning + notificação + fluxo segue = exatamente o AC-7 (usuário cai no dialog no próximo login, comportamento pré-fix). **A1 confirmada no código:** `AccountManager.Register` retorna `Login(...)` — só devolve `OK` depois de `SelectedAccount` populado e sessão setada (`AccountManager.cs:81-86`); falha parcial (register OK, `RequestAccount` falha) → `NoConnection`/`LoginFailed` → `registerResult != OK` → `ChangePassword` nem executa, sem NRE. Ressalva fora de escopo: nesse cenário parcial a UI mostra "Erro ao criar conta: …" com a conta JÁ criada (retry vira `RegisterFailed`) — comportamento byte-idêntico ao HEAD (verificado no diff), pré-existente ao item, não introduzido pelo D1.
4. **Fallback vanilla (foco 4):** ✅ 404/timeout → NRE capturado (CR-01-07) → fallback; JSON inválido/HTML → `JsonException` capturada → fallback; `GetJson` null → `Deserialize(null)` = null → fallback; `profileDescriptions` null → `?.TryGetValue` seguro; `editions` null → lista vazia + `RegisterErrorMsg`; `BuildFromEditionsFallback` cumpre o "never throws" do doc-comment (guards em cada acesso) — o furo residual do fire-and-forget é o CR-01-01, fora dessa função.
5. **XAML (foco 6):** ✅ zero binding órfão — `Advantages`/`Disadvantages`/`Skills`/`ImagePath` do layout antigo removidos da view junto com o VM; todos os bindings novos existem no VM (`AvailableClasses`, `SelectedClass(.NameUpper/.Description)`, `IsLoading`, `RegisterErrorMsg`, `HasIcon/IconPath/HasNameColor/NameBrush/Name`, commands). Todos os recursos referenciados existem: `TrlPhotoOverlayBrush`/`TrlPanelOverPhotoBrush`/`TrlEdgeBrush`/`TrlFgMutedBrush` (Tokens), `TrlTrackWider`/`TrlTextSm`/`TrlTextMd` (Typography), `trl-label`/`trl-muted`/`trl-danger` (Text), `ListBox.trl-nav` (ListBox), `Button.primary`/`.ghost` (Button), `ImageSourceConverter`, `bg-hero.jpg` (incluído via `AvaloniaResource Assets\**`). `TrlVersionFooter` com defaults (15.0/0.10 — 013L liga o dado); `TrlPanel` respeita `Background`/`Padding` via TemplateBinding (template verificado); `TrlScreenBar` recebe `Title` uppercase do consumidor (`NameUpper`) como o contrato do controle pede. Zero hex novo na view; vermelho só em `trl-danger` (contexto legítimo de erro); truque dos 2 TextBlocks preserva os foregrounds por estado do trl-nav para itens sem `nameColor` (item COM cor mantém a cor do server em selected/hover — decisão A3 da spec). Fluxo de registro intacto vs HEAD: guard, edition = `EditionKey` exato, notificação, auto-login → Profile, falha → Login, VOLTAR → Register, `[RequireServerConnected]` preservado.
6. **`RequestClassList` (foco lateral):** ✅ padrão idêntico aos vizinhos, `GetJson` com zlib default (exigência 058 §9 — nunca `HttpClient` cru); sessionId eventualmente presente no header é ignorado pela rota (as-built 058).

## Placar

| Sev | Qtd | IDs |
|---|---|---|
| 🔴 | 0 | — |
| 🟡 | 6 | CR-01-01 (fallback fora do catch em task engolidora), CR-01-02 (guard apaga mensagem do estado vazio), CR-01-03 (ícones seguram a lista, pior caso ~105 s), CR-01-04 (corrida duplo-load na infra de imagem), CR-01-05 (cache não isolado por server — dev-only), CR-01-06 (sanitização de `fileName` só no caller) |
| 🟢 | 2 | CR-01-07 (NRE como controle de fluxo no 404), CR-01-08 (micro-notas) |

Recomendação: aplicar CR-01-01 e CR-01-02 antes do gate humano P-004.1/P-004.3 (ambos tocam exatamente os cenários que os gates vão exercitar); CR-01-03/04/05/06 podem entrar no mesmo apply ou virar pendências registradas.

## Resoluções (apply 2026-07-04)

| ID | Resolução | Como |
|---|---|---|
| CR-01-01 | ✅ Aplicado | `LoadClassesAsync` reestruturado: try externo de última instância + **`finally` com o `Dispatcher.Post`** — a publicação (lista ou estado vazio com mensagem) nunca é pulada, mesmo com exceção no fallback/log; catch final loga com `try/catch` próprio (o log é o vetor plausível). `// ref: CR-01-01` |
| CR-01-02 | ✅ Aplicado | Guard `SelectedClass == null` movido para ANTES de `RegisterErrorMsg = ""`. `// ref: CR-01-02` |
| CR-01-03 | ✅ Aplicado | Opção de **menor risco = `Task.WhenAll`** (a indicada pela própria review): `BuildFromServer` → `BuildFromServerAsync`; perfis construídos sem ícone, downloads disparados em paralelo (`Task.Run` por ícone) e aguardados antes do publish — pior caso colapsa de ~7×15 s p/ ~1 timeout. Escolhida em vez de publicar-antes-e-preencher porque não exige `ClassProfile` reativo nem Post por item (perfis ainda não estão bound à UI; o await do `WhenAll` garante visibilidade de memória antes do Post). `// ref: CR-01-03` |
| CR-01-04 | ✅ Aplicado | `ImageRequest`: `CachedRoutes` `List<string>` → `ConcurrentDictionary<string, byte>` + `RouteLocks` (`ConcurrentDictionary<string, object>`) com **lock por rota** em `CacheServerImage` E `CacheImage` — nunca dois escritores no mesmo arquivo; perdedor espera e pega cache hit. Callers vanilla (`CacheBackgroundImage`/`CacheSideImage`) preservados: assinaturas, chave sem prefixo e semântica de sessão intactas. `// ref: CR-01-04` |
| CR-01-05 | ✅ Aplicado | Chave de sessão do `CacheServerImage` prefixada pelo backend (`GetBackendUrl() + route`); arquivo em disco segue compartilhado (re-download sobrescreve). `// ref: CR-01-05` |
| CR-01-06 | ✅ Aplicado | Dentro de `CacheServerImage`: `fileName = Path.GetFileName(fileName)` + rejeição de `Path.GetInvalidFileNameChars()` → null. Contrato não depende mais do caller. `// ref: CR-01-06` |
| CR-01-07 | ⏭️ Aceito sem ação estrutural | Padrão pré-existente da infra (`GetJson` sem null-check). De graça no retrofit do CR-01-01: o warning agora loga `ex.GetType().Name` junto da mensagem — quem depurar P-004.3 vê `NullReferenceException:` explícito em vez de só a mensagem. |
| CR-01-08 | ⏭️ Aceito | (1) `Task.Run` aninhado REMOVIDO de graça no retrofit do CR-01-01 (chamada direta — o método já roda em thread de fundo, `// ref: CR-01-08.1`); (2) string PT hardcoded mantida — segue o precedente registrado do arquivo/views; (3) ícone corrompido = cosmético, autocorrige na sessão seguinte; (4) Skills/multipliers sem render = intencional (kickoff). |

**Nota de build do apply:** warnings CS86xx (nullable) passaram a aparecer nos arquivos do item DEPOIS da review — `<Nullable>enable</Nullable>` entrou no `SPT.Launcher.csproj` por outro track entre a review e o apply; são ruído de contexto nullable idêntico ao dos arquivos irmãos (`RegisterViewModel`, `ProfileViewModel` etc.), não regressão deste item. Anotação de nullability do codebase = fora do escopo deste apply. Gate mantido: **0 Erro(s)**.

## Histórico

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-04 | Claude (reviewer) | Criação — review adversarial do item 004L (working tree). |
| 2026-07-04 | Claude | Apply: 6 🟡 aplicados (✅), 2 🟢 aceitos (⏭️ — 08.1 resolvido de graça); seção Resoluções + nota de build. |
