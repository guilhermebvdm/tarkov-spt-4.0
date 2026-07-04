# 008 — Configs performance (toggle + overlay) · Code Review 01 (adversarial)

**Launcher:** Launcher4.0beta · **Data:** 2026-07-04 · **Revisor:** agente adversarial (contexto limpo) · **Commit revisado:** `6bd5138` (feature 008/009) + fiação dos VMs que entrou em `d18994f` (CR-01-06 do 007: "VMs carry W3 planner wiring") · **Motor avaliado:** pós-fix `d18994f`

> Escopo lido na íntegra pós-mudança: `SPT.Launcher.Base/Sync/*` (SyncManifestOverlay, SyncPlanner, SyncEngine, SyncBaseline, SyncRuleResolver, SyncPathUtil, SyncPlannerOptions), `Controllers/RequestHandler.cs`, `Helpers/LauncherSettingsProvider.cs`, `ViewModels/{ProfileViewModel,ModUpdateViewModel,SettingsViewModel}.cs`, `Views/SettingsView.axaml`, `SPT.Launcher.Tests/Sync/{SyncOverlayTests,SyncTestFixture}.cs`, server `ModUpdater.cs`.

## Placar

| Severidade | Qtd | IDs |
|---|---|---|
| 🔴 Bloqueante | 1 | CR-01-01 (pré-existente no arquivo tocado — não é regressão deste commit) |
| 🟡 Atenção | 4 | CR-01-02, CR-01-03, CR-01-04, CR-01-05 |
| 🟢 Menor/observação | 4 | CR-01-06, CR-01-07, CR-01-08, CR-01-09 |

## Gates

```
dotnet build SPT.Launcher/SPT.Launcher.csproj -c Release              → 0 Erro(s), 168 Aviso(s) (pré-existentes: nullability/CA1416)
dotnet test  SPT.Launcher.Tests/SPT.Launcher.Tests.csproj -c Release  → Aprovado! 52/52, 0 falhas (108 ms)
dotnet build TarkovRedLine.Server.csproj -c Release                   → 0 Erro(s), 35 Aviso(s)
```

Todos verdes, sem retry necessário (nenhum lock transitório). Exe do launcher NÃO executado, conforme instrução.

---

### CR-01-01 [🔴] `/launcher/mods/download` serve path absoluto arbitrário — o guard do novo `performance-download` expõe a assimetria

**Arquivo:** `mods/TarkovRedLine4.0/Server/TarkovRedLine.Server/Controllers/ModUpdater.cs` (`DownloadFile`, linhas 71–96)

**Pré-existente** — não foi introduzido por este commit, mas o commit adicionou o endpoint irmão `performance-download` com o guard CORRETO (`Path.GetFullPath` + `StartsWith`) no mesmo arquivo, e a comparação pedida por esta review revela que o `/download` original **não tem guard de contenção nenhum**:

```csharp
var fallbackPath = Path.Combine(GetModsRepoPath(), file.Replace("..", ""));
if (System.IO.File.Exists(fallbackPath)) return PhysicalFile(fallbackPath, ...);
```

**Cenário concreto:** `Path.Combine` com segundo argumento **enraizado** descarta o primeiro. `GET /launcher/mods/performance-download` está protegido, mas:

```
GET http://<server>:7075/launcher/mods/download?file=D:/SPT/SPT/user/profiles/<id>.json
```

`"D:/SPT/...".Replace("..","")` é no-op → `Path.Combine(modsRepo, "D:/SPT/...")` retorna o path enraizado → `File.Exists` → `PhysicalFile` **serve o profile do jogador** (ou qualquer arquivo legível pelo processo: config do server com segredos, `hwid.json`, etc.). O cache `_fileMapCache` não salva: o fallback roda em qualquer miss. O server é alcançável pela rede Tailscale de todos os jogadores, sem autenticação nesse endpoint. `optional-download` e `optionals-manifest` têm `StartsWith` e ficam protegidos contra o vetor enraizado; só o `/download` está aberto.

**Fix:** replicar o padrão do `performance-download` (que este commit acertou):

```csharp
var modsPath = GetModsRepoPath();
var fallbackPath = Path.GetFullPath(Path.Combine(modsPath, file.Replace("..", "")));
if (!fallbackPath.StartsWith(modsPath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) return NotFound(...);
```

(ver CR-01-06 para o detalhe do separador final, que vale para os quatro endpoints).

---

### CR-01-02 [🟡] Overlay ON sem baseline ⇒ pack **nunca aplica**, silenciosamente (R1.5) — e não se auto-recupera enquanto ON

**Arquivos:** `SyncPlanner.cs` (ramo `PreserveDivergent`, linhas 105–124) · `SyncManifestOverlay.cs` (`Merge` descarta o hash base) · `SyncOverlayTests.cs` (gap)

**Cenário concreto:** o merge substitui o hash base pelo hash do pack, e o planner só aplica em pasta `preserve-divergent` quando `local == baseline` (R1.3 — "não customizado, server evoluiu"). Quando **não há entrada de baseline** para o path, R1.5 preserva conservadoramente. Sequências reais que chegam nesse estado:

1. `sync-state.json` corrompido/apagado (`SyncBaseline.Load` engole exceção → baseline vazio) com o toggle já ON;
2. usuário cancela o auto-check do login (botão CANCELAR existe), liga o toggle e clica VERIFICAR — primeira sync da instalação já roda com o manifesto merged;
3. server adiciona um cfg novo em base+pack simultaneamente e o arquivo local já existe (instalado por mod, nunca sincronizado).

Resultado: `local (default) ≠ efetivo (pack)`, sem baseline → `PreserveCustomized` → o pack **não aplica**; e como ações preservadas **não semeiam baseline** (CC7 só semeia `local == hash efetivo`, que é o do pack), TODA verificação futura com ON repete o mesmo resultado. Só destrava com o ciclo não-óbvio OFF → verificar (CC7 semeia `local == default`) → ON → verificar. A UI mostra apenas "(N preservados)" — o usuário acha que o pack está ativo.

**Confronto com os testes:** os 5 testes semeiam baseline via `PlanAndRunAsync(baseManifest)` antes de qualquer merge — nenhum cobre o caminho sem baseline.

**Fix proposto:** o merge conhece o hash base e o joga fora. Expor no `SyncManifestOverlay` um `IReadOnlyDictionary<string,string> BaseHashByPath` e, no ramo R1.5 do planner (via `SyncPlannerOptions`, ex. `Func<string,string> FallbackBaselineHash`), tratar `local == hashBase` como "equals baseline" (é a mesma prova do CC7: o arquivo é byte-idêntico ao default do server ⇒ não é customização). Alternativa mínima: teste documentando o comportamento + aviso na UI quando overlay ON e `Preserved > 0` em paths do pack.

---

### CR-01-03 [🟡] OFF não restaura arquivo tocado após o apply (BepInEx reescreve cfgs) — e o texto da UI promete restauração incondicional

**Arquivos:** `Views/SettingsView.axaml` (texto do toggle) · semântica D1 (spec 008) · `SyncOverlayTests.cs` (gap)

**Cenário concreto:** ON → verificação aplica o pack (baseline := hash pack) → usuário **joga**. BepInEx `ConfigFile` re-serializa cfgs no boot do plugin (acrescenta keys ausentes, remove órfãs, normaliza ordem/whitespace) — hash MD5 byte a byte muda mesmo sem o usuário tocar em nada. Agora `local ≠ baseline(pack)` → OFF → verificação: `PreserveCustomized` → o arquivo **não volta ao padrão do server**, contrariando o texto do toggle: *"Ao desligar, a próxima verificação restaura as configs padrão do servidor."* O mesmo mecanismo impede updates futuros do pack com ON (server publica pack v2 → local ≠ baseline → preservado).

É a semântica R1 do 007 funcionando como projetada (conservadora, correta para customização humana) — mas para um pack de **cfgs de BepInEx**, arquivo-reescrito-pelo-jogo é o caso comum, não a exceção. O teste 4 (`TurningOverlayOff_...`) prova o revert só no cenário "ninguém tocou entre as syncs" — a versão simplificada do fluxo real ON→**jogar**→OFF.

**Fix:** (a) corrigir o texto da UI para não prometer o que R1 não entrega: "restaura as configs **que você não modificou** desde a última sincronização"; (b) orientação ao operador (doc do pack): distribuir cfgs completos e estáveis (todas as keys, na ordem que o plugin serializa) para minimizar reescrita; (c) incluir o cenário ON→tocar arquivo→OFF no P-008.1 (E2E in-game) e num 6º teste de unidade documentando o comportamento esperado (Preserve, não revert).

---

### CR-01-04 [🟡] Pack cobrindo arquivo de grupo opcional ATIVO ⇒ ping-pong entre o motor e o fluxo legado de opcionais

**Arquivos:** `SyncManifestOverlay.cs` (A-008.3 só preserva flags) · `OptionalModsHelper.cs` (`DownloadOptionalGroupAsync`, linhas 226–261) · spec 009 §P-009.1 (agrava)

**Cenário concreto:** A-008.3 garante que grupo **desligado** não é forçado (flags preservadas → planner exclui). Mas para grupo **ligado** os dois fluxos brigam:

1. Manifesto base tagueia `BepInEx/config/com.Shibatsu.DynamicExternalResolution.cfg` como `optionalGroup="pip"` e o operador põe um override desse cfg no pack — **exatamente** a mitigação candidata registrada em P-009.1 ("ativar PiPDisable deve desligar Enable Mod do DERP… candidato: config no performance pack do 008").
2. Grupo ativo + toggle ON: verificação baixa a versão do **pack** (hash efetivo = pack, roteado pro `performance-download`); baseline := hash pack.
3. Usuário desliga/religa o opcional (ou qualquer chamada a `DownloadOptionalGroupAsync`): o helper compara hash local (pack) contra `file.hash` do **manifesto base** (o cache `_cachedGroupFiles` é montado de `allFiles`, pré-merge) → mismatch → re-baixa a versão **base** via `/download`, escrevendo **fora do motor, sem atualizar baseline**.
4. Próxima verificação: `local (base) == baseline?` Não — baseline tem hash pack… `local ≠ baseline` → em pasta config → `PreserveCustomized` → **estado preso na versão base com o toggle ON** (variante do CR-01-02); em pasta mirror, re-baixa o pack → loop de rede a cada interação.

**Fix:** curto prazo, regra de operação validada no server: `GenerateManifestAsync` loga warning se um path de `config-performance/` colide com path taggeado `optionalGroup` (e o doc do operador proíbe). Médio prazo: `Merge` receber o set de paths opcionais e não sobrepor entradas de grupos (a colisão vira warning no client). Longo prazo é o P-009.2 (rotear opcionais pelo motor/baseline).

---

### CR-01-05 [🟡] Baseline grava o hash do MANIFESTO, não dos bytes baixados — manifesto stale do pack envenena o baseline

**Arquivos:** `SyncEngine.cs` (linha 98: `_baseline.SetHash(action.RelativePath, action.ServerHash)`) · server `ModUpdater.cs` (manifesto estático, regenerado só em `/refresh`/restart)

**Cenário concreto:** o manifesto do server é gerado uma vez e cacheado (`_manifestCache` estático). O operador edita um cfg em `config-performance/` **sem** chamar `/refresh`:

1. Client verifica com ON: manifesto entrega o hash **antigo** do arquivo do pack; o `performance-download` serve os bytes **novos** (o `_performanceFileMapCache` aponta pro mesmo path físico, e o fallback lê o disco).
2. Engine grava `baseline := hash antigo` (do manifesto), mas o arquivo em disco tem hash novo → `local ≠ baseline` já na próxima verificação → "customizado" → wedge do CR-01-02: pack não re-aplica, OFF não reverte.

O mesmo desalinhamento existe no `mods_repo` desde o 007, mas o pack agrava: é a pasta que o operador vai editar ao vivo com frequência (presets de FPS), e a vítima é justamente o mecanismo de baseline de que o D1 depende.

**Fix:** no `SyncEngine`, após `ApplyAtomic`, registrar o MD5 **dos bytes efetivamente gravados** (`SyncPathUtil.ComputeMd5` no destino, ou hash do buffer `data` em memória — barato) em vez de `action.ServerHash`; opcionalmente logar warning quando divergir do hash do manifesto (download inconsistente detectado). + Doc do operador: `/refresh` obrigatório após mexer no pack (vale registrar no P-008.1).

---

### CR-01-06 [🟢] Hardening do guard do `performance-download`: `StartsWith` sem separador final + lookup case-sensitive

**Arquivo:** server `ModUpdater.cs` (linhas 117–122)

`fallbackPath.StartsWith(perfPath)` sem `+ Path.DirectorySeparatorChar` aceita irmãos por prefixo (`Launcher-Updater/config-performance-bak/...` passaria, se alcançável via input enraizado que reproduza o prefixo — exige conhecer o path absoluto do server; risco baixo). O lookup `_performanceFileMapCache.TryGetValue(normalizedFile)` é case-sensitive — casing do path do manifesto base (que o merge preserva) diferente do casing em disco do pack cai no fallback (funciona no Windows; num host Linux daria 404). Fix de 2 linhas: sufixar o separador no `StartsWith` e criar o dicionário com `StringComparer.OrdinalIgnoreCase`. Mesmo hardening de separador vale para `optionals-manifest`/`optional-download` (pré-existentes).

---

### CR-01-07 [🟢] `Merge`: resolução de duplicatas inconsistente (override usa a ÚLTIMA ocorrência do pack, append usa a PRIMEIRA)

**Arquivo:** `SyncManifestOverlay.cs` — `overlayByPath[...] = overlay` (última vence) vs loop de append que adiciona a primeira não-consumida. O scan de disco do server não produz paths duplicados, então é inalcançável hoje — registrar para quando o pack tiver outra origem. Fix: deduplicar `overlayFiles` (primeira vence) antes dos dois usos.

---

### CR-01-08 [🟢] Plano stale no `ModUpdateViewModel`: toggle alterado entre CHECK e APPLY aplica o plano antigo

**Arquivo:** `ModUpdateViewModel.cs` — `_plan`/`_performanceOverlay` são capturados no `CheckForUpdates`; se o usuário for a Settings, mudar `UsePerformanceConfigs` e voltar a clicar ATUALIZAR sem re-checar, aplica o plano da configuração anterior (par plano+downloader consistente entre si, então nada corrompe; a próxima verificação corrige). Aceitável; se quiser blindar: invalidar `_plan` ao detectar mudança do setting.

---

### CR-01-09 [🟢] (recorte 007, herdado) Gate de reentrância é por instância — navegação ainda permite 2 motores

**Arquivo:** `ProfileViewModel.cs` (`_syncGate` é campo de instância). Navegar para `ModUpdateView` (auto-check no ctor) e voltar (`GoBackCommand` → **novo** `ProfileViewModel` → novo auto-check), ou Settings→voltar com `PendingOptionalChanges` (também instancia novo `ProfileViewModel`), pode rodar dois engines sobre o mesmo baseline — o fix CR-01-01 do 007 fechou a reentrância **dentro** da instância, não entre instâncias/telas. O 008 não muda o risco, mas o herda (dois merges concorrentes). Registrado aqui como referência cruzada para o follow-up do 007; fix natural: gate `static`.

---

## Confronto com os 5 testes do `SyncOverlayTests` (pergunta central do encargo)

Os testes usam o planner+engine **reais** com filesystem temp (não mocks) — os fluxos que cobrem, cobrem de verdade:

| Teste | Fluxo provado | Limite |
|---|---|---|
| 1 `Merge_OverridesByPath...` | override case/separator-insensitive, casing base preservado, append, flags | puro, ok |
| 2 `Overlay_Applies...` | ON aplica sobre estado convergido; roteia pro endpoint do pack; baseline := pack | exige baseline pré-semeado |
| 3 `Overlay_PreservesUserCustomizedConfig` | customização sobrevive ao ligar o pack (D2) ✔ | — |
| 4 `TurningOverlayOff_Reverts...` | OFF re-baixa o padrão e re-semeia baseline (D1) ✔ | só se ninguém tocou o arquivo entre as syncs (CR-01-03) |
| 5 `SteadyStateOn_SecondRun...` | regime ON converge, zero churn (D3) ✔ | — |

**Não provados (gaps):** ON sem baseline (CR-01-02); ON→arquivo-modificado→OFF (CR-01-03); ciclo de vida do arquivo só-do-pack no OFF (A-008.1: sob `config` fica no disco; sob `plugins` vai para `-disabled` — comportamento correto e **não testado**); exclusão de grupo opcional desligado no nível do **planner** (A-008.3 — o teste 1 só verifica as flags no merge); roteamento com downloads base+pack no MESMO plano (teste 2 tem 1 arquivo só).

**Resposta direta ao trace ON→jogar→OFF:** com o arquivo intocado entre as syncs, o OFF reverte corretamente (baseline registra o hash do pack ⇒ R1.3 re-baixa o padrão — provado pelo teste 4 no motor real). Com o arquivo tocado (pelo jogo ou pelo usuário), fica preso como "customizado" — CR-01-03. Customização prévia sobrevive a ON e a OFF (testes 3 + R1.4), sem caminho que a sobrescreva.

## Áreas auditadas e limpas

- **Merge single-pass (D3):** semanticamente correto contra o motor pós-fix; anti-churn confirmado; entradas vazias/nulas filtradas; `IsOverlayPath` opera no set normalizado.
- **Roteador de downloader (pergunta 2 do encargo):** nos dois VMs, path do pack → `DownloadPerformanceFile`, resto → `DownloadModFile`; com OFF o overlay é `null` e nada roteia pro endpoint do pack; não existe caso em que um path presente nos dois manifestos precise dos bytes BASE com ON (hash efetivo é o do pack). Sem endpoint errado.
- **Guard GameRoot (CR-01-05 do 007):** paths do pack passam pelo mesmo `ResolveUnderRoot` do engine ANTES do download — pack malicioso/typado com `..` vira erro por-arquivo, sem escrita fora da raiz. ✔
- **CR-01-02 do 007 (ignoredFiles):** overlay não interage — ignored só filtra extras, downloads do pack não são bloqueados. ✔
- **Server (pergunta 3):** `performanceOverlay` entra no objeto serializado ANTES do MD5 ⇒ pack muda → hash do manifesto muda (D4 confirmado; ressalva CR-01-05: só após regeneração). Pack ausente → pasta criada + seção `[]` → client (`?.ToObject ?? new List`) tolera; server antigo sem a seção → idem. Launcher antigo ignora o campo extra (JObject). ✔
- **Persistência:** `UsePerformanceConfigs` como prop pública do `Settings` (Newtonsoft serializa), save imediato no setter do `SettingsViewModel`, `EXPECTED_CONFIG_VERSION` 2→3 re-grava com default `false`. ✔
- **XAML:** `SettingsView.axaml` válido, toggle com classes do tema, zero hex novo. ✔

## Pendências sugeridas (a fundir no asbuild pelo /apply-code-review)

- Somar ao **P-008.1** (E2E): cenário ON→jogar→OFF (CR-01-03), pack editado sem `/refresh` (CR-01-05) e primeira sync com ON (CR-01-02).

---

## Resoluções

**Data:** 2026-07-04 · **Executor:** Wave 3 (/apply-code-review) · Gates pós-apply: build launcher 0 erros · build server 0 erros · testes **55/55** verdes.

| ID | Resolução | Como |
|---|---|---|
| CR-01-01 🔴 | **APLICADO** | Guard de contenção compartilhado `TryResolveUnder(baseDir, input)` no `ModUpdater.cs` (`Path.GetFullPath` + `StartsWith(prefixo + separador, OrdinalIgnoreCase)` + catch de path inválido) aplicado ao fallback do **`/download`** e replicado em **`optionals-manifest`** e **`optional-download`** (que só tinham `StartsWith` sem `GetFullPath`/separador). `performance-download` migrado pro mesmo helper. Input enraizado agora resolve e é rejeitado pelo prefixo |
| CR-01-02 🟡 | **DECIDIDO: documentado, não corrigido nesta passada** | O fix do CR-01-05 (hash dos bytes) **não** fecha este caso — o wedge nasce no planner (R1.5 preserva sem baseline), não no hash gravado. Virou teste de comportamento `OverlayOn_WithoutBaseline_DoesNotApplyPack_KnownWedge` (documenta o wedge + o destravamento OFF→verificar→ON) + cenário no P-008.1. O fix estrutural proposto (fallback `local == hash base` ⇒ trata como "equals baseline" via `SyncPlannerOptions`) fica registrado como **P-008.3** — mexe em semântica do planner, fora do apply cirúrgico |
| CR-01-03 🟡 | **DECIDIDO: texto honesto (menor risco) + teste** | Rastrear paths do pack p/ revert forçado no OFF sobrescreveria exatamente o que o preserve-divergent protege (cfg reescrita pelo jogo é indistinguível de customização) — risco maior. Aplicado: descrição do toggle na `SettingsView` agora diz que o OFF restaura só o que **não foi modificado desde a última sincronização**; teste `Off_AfterFileTouchedPostApply_PreservesInsteadOfReverting` documenta o comportamento; cenário ON→jogar→OFF somado ao P-008.1; orientação ao operador (cfgs completos/estáveis no pack) no asbuild |
| CR-01-04 🟡 | **DECIDIDO: regra de operação + pendência** | Não fechável por unit test (o ping-pong envolve o fluxo legado HTTP do `OptionalModsHelper`) e o warning server-side sugerido seria código morto neste repo (o manifesto nunca tagueia `optionalGroup` — P-009.2). Registrado: regra de operação "pack NÃO pode cobrir arquivo de grupo opcional" no asbuild, cenário no P-008.1, e a observação cruzada do review do 009 acatada — se P-009.1 confirmar o conflito PiP×DERP, a mitigação preferida é **off-file no grupo**, não o pack |
| CR-01-05 🟡 | **APLICADO** | `SyncEngine` agora grava no baseline o **MD5 dos bytes gravados** (`SyncPathUtil.ComputeMd5(byte[])` novo) e loga warning quando diverge do hash do manifesto (manifesto stale — `/refresh` esquecido). Teste `Baseline_RecordsHashOfWrittenBytes_NotStaleManifestHash` |
| CR-01-06 🟢 | **APLICADO** | Coberto pelo helper compartilhado (separador final em todos os endpoints) + `_fileMapCache`/`_performanceFileMapCache` com `StringComparer.OrdinalIgnoreCase` |
| CR-01-07 🟢 | **APLICADO** | `Merge` deduplica o pack com `TryAdd` (primeira ocorrência vence nos dois usos — consistente com o append) |
| CR-01-08 🟢 | **REGISTRADO, sem código** | Aceito como o reviewer classificou: par plano+downloader é consistente entre si, nada corrompe e a próxima verificação corrige; a tela nem tem navegação apontando pra ela hoje |
| CR-01-09 🟢 | **REGISTRADO** | Follow-up do 007 (gate de reentrância entre instâncias), fora do escopo deste apply — referência cruzada mantida |
