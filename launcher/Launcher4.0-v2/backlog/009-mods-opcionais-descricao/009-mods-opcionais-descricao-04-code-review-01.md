# 009 — Mods opcionais com descrição · Code Review 01 (adversarial)

**Launcher:** Launcher4.0beta · **Data:** 2026-07-04 · **Revisor:** agente adversarial (contexto limpo) · **Commit revisado:** `6bd5138` (feature 008/009) + fiação do `ProfileViewModel`/`ProfileView.axaml` que entrou em `d18994f` ("VMs carry W3 planner wiring")

> Escopo lido na íntegra pós-mudança: `Helpers/OptionalModsHelper.cs`, `ViewModels/{ProfileViewModel,OptionalModToggle}.cs`, `Views/ProfileView.axaml` (seção MODS OPCIONAIS), `Controllers/RequestHandler.cs`, server `ModUpdater.cs` (`optionals-list`, `optionals-manifest`), 4 templates `description.json`.

## Placar

| Severidade | Qtd | IDs |
|---|---|---|
| 🔴 Bloqueante | 0 | — |
| 🟡 Atenção | 1 | CR-01-01 |
| 🟢 Menor/observação | 3 | CR-01-02, CR-01-03, CR-01-04 |

## Gates

```
dotnet build SPT.Launcher/SPT.Launcher.csproj -c Release              → 0 Erro(s), 168 Aviso(s) (pré-existentes)
dotnet test  SPT.Launcher.Tests/SPT.Launcher.Tests.csproj -c Release  → Aprovado! 52/52, 0 falhas
dotnet build TarkovRedLine.Server.csproj -c Release                   → 0 Erro(s), 35 Aviso(s)
```

Todos verdes. Exe do launcher NÃO executado, conforme instrução.

---

### CR-01-01 [🟡] Join heurístico é first-match por DESCRIPTOR (ordem alfabética do server) — match fraco anterior vence match exato posterior; grupos multi-pasta herdam a descrição da primeira pasta alfabética

**Arquivo:** `ViewModels/ProfileViewModel.cs` (`FindOptionalDescriptor`, linhas 310–330)

**Defeito estrutural:** o loop itera os descriptors (ordem = `Directory.GetDirectories`, alfabética) e retorna o **primeiro** que satisfaça QUALQUER uma das 3 regras da D2 (join por id → pastas → nome). A precedência é por posição na lista, não por força da regra: um descriptor anterior casado pela regra fraca (nome, ou pasta compartilhada) **sombreia** um descriptor posterior que casaria pela regra forte (id exato).

**Cenário concreto (layout real citado na própria spec — ids `gore`/`hollywood` × pastas `Visceral`/`Hollywood`):**

1. `config.json` do server: `{ id: "gore", name: "Gore Total", folders: ["Visceral", "Hollywood"] }` (grupo multi-pasta) e `{ id: "hollywood", name: "Hollywood", folders: ["Hollywood"] }`.
2. `optionals-list` devolve descriptors em ordem alfabética: `[Hollywood, IRL, PiPDisable, Visceral]`.
3. Toggle `gore`: o descriptor `Hollywood` é testado primeiro → regra 2 (`group.folders ∋ "Hollywood"`) casa → **retorna Hollywood** — o grupo "Gore Total" exibe *"Efeitos cinematográficos de sangue e impacto…"* em vez da descrição do Visceral (a pasta principal do grupo). O descriptor `Visceral` nunca é considerado.
4. Variante do sombreamento id-exato: se existisse pasta `Opcionais/Gore/` com descriptor próprio, o toggle `gore` ainda receberia o do `Hollywood` — a regra 1 (`descriptor.id == "gore"`) do descriptor `Gore` (posterior na ordem alfabética a `Hollywood`? não — "Gore" < "Hollywood"… mas "Efeitos" < "Gore" sombrearia via regra 3 se um descriptor anterior tivesse `name == group.name`). O ponto: a correção do resultado depende da **ordem alfabética das pastas no server**, que o operador não controla como contrato.

**Fix (~6 linhas):** três passadas com precedência global sobre a lista inteira:

```csharp
return descriptors.FirstOrDefault(d => Eq(d.Id, toggle.Id))
    ?? descriptors.FirstOrDefault(d => group?.folders?.Any(f => Eq(f, d.Id)) == true)
    ?? descriptors.FirstOrDefault(d => !string.IsNullOrWhiteSpace(group?.name) && Eq(d.Name, group.name));
```

Para multi-pasta continua pegando a primeira pasta da lista `folders` (ordem do OPERADOR no config.json, que é curável) — melhor contrato do que ordem alfabética do disco.

---

### CR-01-02 [🟢] Troca de idioma em Settings não re-enriquece — descrição fica no idioma antigo até a próxima verificação

**Arquivos:** `ProfileViewModel.cs` (`EnrichOptionalDescriptionsAsync` roda uma vez por `CheckForUpdates`) · `SettingsViewModel.GoBackCommand`

`preferPt` é resolvido no momento do enriquecimento. Usuário troca o idioma em Settings e volta: sem `PendingOptionalChanges`, o `GoBackCommand` faz `NavigateBack` para a **instância existente** do `ProfileViewModel` — nenhum re-check, descrições permanecem no idioma anterior até re-login ou VERIFICAR ARQUIVOS. Cosmético e auto-corrigível; se quiser fechar: guardar os descriptors buscados e re-aplicar `ResolveDescription` num handler de `DefaultLocale` changed.

---

### CR-01-03 [🟢] (pré-existente, fora do diff) Fluxos legados de opcionais usam `GetServerBaseUrl()` que derruba a porta — a D6 evitou o canal ruim para o call novo, mas o OFF/ON dos toggles continua nele

**Arquivo:** `OptionalModsHelper.cs` (`GetServerBaseUrl` → `http://{uri.Host}` sem porta; usado por `DownloadOptionalGroupAsync` e `DownloadFromOpcionaisFolder`)

O item 009 acertou em rotear `RequestOptionalsList()` pelo `request.RemoteEndPoint` (D6). Mas os downloads de ativação/offFolders continuam montando URL sem porta (`http://<host>/launcher/mods/...` → porta 80) — se funcionam em produção, é por proxy/port-forward não documentado neste repo. Não é regressão deste commit (nenhuma linha desses fluxos mudou); registrado porque conecta com o P-009.2 (pipeline de instalação dos opcionais precisa de verificação de produção). Fix quando atacar o P-009.2: migrar os dois métodos para `RequestHandler`.

---

### CR-01-04 [🟢] Descriptor herdado no disco: cliente que baixou um off-folder ANTES do descriptor existir não recebe cleanup

Se no futuro um operador colocar `description.json` na raiz de uma pasta que também é usada como `offFolder` (layout onde o off-folder É a raiz do grupo), clientes que sincronizaram ANTES da exclusão D5 existir podem ter um `description.json` órfão no `GamePath`. Hoje é vazio (descriptors são novos, nunca foram servidos pelo `optionals-manifest`). Nenhuma ação — registrado para não redescobrir.

---

## Respostas diretas às perguntas do encargo

**Parse tolerante (2 shapes) — matriz de falhas auditada, sem caminho de crash:**

| Entrada | Resultado |
|---|---|
| Shape novo `{folders:[{id,name,description:{pt,en}}]}` | parse completo |
| Shape legado `{folders:["A","B"]}` | descriptor `{Id=Name=pasta, descrições null}` → enriquecimento vira no-op de descrição (regra "só seta se não-vazio") |
| Elemento nem string nem objeto (número, null, array) | `continue` — item ignorado |
| `description` string/null/array em vez de objeto | ignorado → descrições null |
| `id` e `name` ausentes | descriptor descartado (`Id` vazio) |
| Response não-JSON (HTML de erro), vazio, timeout | exceção capturada / early-return → **lista vazia** → toggles mantêm o `description` do `optionalGroups` (fallback D4) |
| Fallback de erro do transporte (`GetFromHwidManager` devolve `{"version":"?"}`) | sem prop `folders` → lista vazia ✔ |

`RequestOptionalsList` nunca lança (o transporte captura internamente); o `Task.Run` + try/catch do helper cobrem o resto. **Nenhum caso vira crash.**

**description.json malformado num grupo derruba a lista?** Não — o server parseia POR grupo dentro de try/catch (`ModUpdater.GetOptionalsList` linhas 151–182): grupo inválido degrada para `{id, name=pasta, description=null}` + log; os demais seguem intactos. ✔

**`ResolveDescription` com ambos null:** retorna `null` → `IsNullOrWhiteSpace` → descrição do toggle não é tocada (fallback preservado). ✔ Fallback cruzado pt↔en correto nos dois sentidos.

**Enriquecimento assíncrono × thread (pergunta 5):** limpo. `OptionalMods` é populado via `Dispatcher.UIThread.Post`; o enriquecimento muta `toggle.Name/Description` (reativos) **dentro de outro `Post`** — toda mutação de coleção/props bindadas na UI thread, serializada pelo dispatcher. A lista `optionalGroups` capturada é local do parse (nunca mutada depois). Corrida entre enriquecimento velho e repopulação nova (retry do manifesto) aplica dados idênticos — benigno. Lambda postada é null-safe em todos os acessos (sem risco de exceção não-observada no dispatcher).

**Regressão offFolders × exclusão do descriptor (pergunta 6):** limpo. A exclusão no `optionals-manifest` filtra apenas `relPath == "description.json"` na **raiz** da pasta pedida — remove da lista um arquivo que não deve ir pro jogo (D5) e nada mais; `optional-download` não é afetado; `DownloadOptionalGroupAsync` (ON) usa arquivos taggeados do manifesto principal, que nunca contêm o descriptor; `GetAllKnownOptionalPaths` (proteção CC3 do motor) idem. Toggle OFF com offFolders funciona exatamente como antes (ver CR-01-03 para o problema pré-existente de porta, que não é desta mudança).

**Retrocompat bidirecional:** launcher novo × server antigo ✔ (shape legado + erro→vazio); launcher antigo × server novo ✔ (endpoint não era consumido; manifesto não mudou para o 009 — o descriptor não entra em `files`).

## Áreas auditadas e limpas (além das acima)

- **D1 (enriquecimento, não substituição):** `Name` do descriptor só entra quando `group.name` é vazio; `Description` do `optionalGroups` só é sobrescrita quando o descriptor resolve texto não-vazio. ✔
- **D3 (idioma):** `DefaultLocale` usa nomes romanos ("Portuguese", "English" — `LocalizationProvider`); `StartsWith("Portuguese", OrdinalIgnoreCase)` cobre as variantes plausíveis. ✔
- **UI (`ProfileView.axaml` 121–139):** `TextBlock` com `IsVisible={Binding Description, Converter=IsNotNullOrEmpty}` — grupo sem descrição não ocupa espaço; wrap ativo; classes do tema, zero hex; resto do restyle intocado. XAML válido (o `<\cc:...>` visto em grep era artefato de exibição). ✔
- **Templates:** 4 descriptors JSON válidos, shape do contrato SP0, PT+EN presentes; aviso do conflito PiP×DERP (Dynamic External Resolution) presente no PiPDisable (mitigação do P-009.1). ✔

## Observação cruzada

A mitigação sugerida no **P-009.1** ("config no performance pack do 008" para desligar o DERP quando PiPDisable ativo) colide com o **CR-01-04 do review do 008** (pack cobrindo arquivo de grupo opcional ativo gera ping-pong entre o motor e o fluxo legado). Se P-009.1 confirmar o conflito in-game, preferir a alternativa **off-file no próprio grupo** — não o pack.

---

## Resoluções

**Data:** 2026-07-04 · **Executor:** Wave 3 (/apply-code-review) · Gates pós-apply: build launcher 0 erros · build server 0 erros · testes **55/55** verdes.

| ID | Resolução | Como |
|---|---|---|
| CR-01-01 🟡 | **APLICADO** | `FindOptionalDescriptor` (`ProfileViewModel.cs`, edit cirúrgico) reescrito em **três passadas com precedência global**: id exato → pastas do grupo (na ordem de `group.folders`, curada pelo operador — não mais a ordem alfabética do disco) → nome. Match fraco anterior não sombreia mais match forte posterior; grupo multi-pasta herda a descrição da primeira pasta da lista do config.json |
| CR-01-02 🟢 | **REGISTRADO, sem código** | Troca de idioma re-enriquece na próxima verificação/re-login (auto-corrigível, cosmético) — anotado no asbuild |
| CR-01-03 🟢 | **REGISTRADO** | Pré-existente e fora do diff; migração dos fluxos legados p/ `RequestHandler` fica com o **P-009.2** (pipeline de instalação dos opcionais) |
| CR-01-04 🟢 | **REGISTRADO** | Sem ação (cenário futuro vazio hoje) — mantido aqui para não redescobrir |

A **observação cruzada** foi acatada na resolução do CR-01-04 do review do 008: se P-009.1 confirmar o conflito PiP×DERP in-game, a mitigação será **off-file no próprio grupo**, não o performance pack.
