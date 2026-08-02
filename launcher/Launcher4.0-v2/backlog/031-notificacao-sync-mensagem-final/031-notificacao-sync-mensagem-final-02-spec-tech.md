# 031 — Notificações de sync · Spec técnica

**Mod:** Launcher4.0-v2
**Criado:** 2026-08-02
**Spec funcional:** [031-notificacao-sync-mensagem-final-01-spec.md](./031-notificacao-sync-mensagem-final-01-spec.md)
**Irmão:** [032 (velocidade)](../032-velocidade-download-nunca-funcionou/032-velocidade-download-nunca-funcionou-02-spec-tech.md) — reset e UI coordenados; esta spec traz a arquitetura compartilhada.

> Fonte primária = código do launcher (`project/`). Sem Assembly EFT/Harmony/F12. Verificação = `dotnet build` + `dotnet test` (novos testes do `SyncMessages`/`SyncResult`); o resto (fechamento visual, idioma) é gate in-game.

## 1. Estratégia

Cinco mudanças, uma central (propagar o tipo de ação) e as outras em cima dela:

1. **Propagar o `SyncActionKind` no `SyncProgress`** — hoje o motor reporta só a fase `"applying"` ([SyncEngine.cs:86](../../project/SPT.Launcher.Base/Sync/SyncEngine.cs#L86)), então a UI não sabe se é download/move/delete. Adicionar um 5º parâmetro **opcional** ao ctor (default `null`, a fase "checking" não passa) — não quebra os 2 call-sites.
2. **Um helper `SyncMessages`** (novo, em `SPT.Launcher/Helpers/`) com **duas** funções puras que os **dois** VMs chamam (consolida o achado F): `ProgressText(kind, path, cur, total)` escolhe a chave de progresso por ação; `BuildSummary(result)` compõe a frase final a partir de **chaves de locale** (mata o `Summary` PT-hardcoded, achado B). O `SyncResult.Summary` PT fica **só para logs internos**.
3. **Fechar o ciclo** — separar a visibilidade da **barra de progresso** (só enquanto o sync roda) da **área de status/link** (persiste com o resultado); e um **guard de geração** que impede um `Progress<T>` atrasado de sobrescrever a mensagem final.
4. **Reset único** — o bloco de início do run ([ProfileViewModel.cs:452-458](../../project/SPT.Launcher/ViewModels/ProfileViewModel.cs#L452)) passa a limpar **também** `LastUpdateText`/`HasLastUpdate` (hoje não limpa). Cobre os 2 gatilhos (ambos entram por `CheckForUpdatesCore`).
5. **Link por total de ações** — `SetLastUpdate` passa a receber o `SyncResult` e ligar o link por `Updated + MovedToDisabled + Deleted + Forced + Seeded + OptionalConfigApplied > 0`, não só `Updated`.

## 2. Pontos de extensão

| Ponto | Local | Papel |
|---|---|---|
| `SyncProgress` ctor/campo | [SyncPlan.cs:7-21](../../project/SPT.Launcher.Base/Sync/SyncPlan.cs#L7) | +`SyncActionKind? Kind` (5º param opcional, default null). |
| `progress?.Report` | [SyncEngine.cs:86](../../project/SPT.Launcher.Base/Sync/SyncEngine.cs#L86) | passa `action.Kind`. |
| `SyncMessages` (novo) | `SPT.Launcher/Helpers/SyncMessages.cs` | `ProgressText` + `BuildSummary` — as 2 funções i18n compartilhadas. |
| `applyProgress` | [ProfileViewModel.cs:679-683](../../project/SPT.Launcher/ViewModels/ProfileViewModel.cs#L679) | usa `SyncMessages.ProgressText(p.Kind, …)` em vez de `update_downloading` fixo. |
| mensagem final | [ProfileViewModel.cs:711-724](../../project/SPT.Launcher/ViewModels/ProfileViewModel.cs#L711) | ramo IoActionCount>0 usa `SyncMessages.BuildSummary(result)`. |
| reset | [ProfileViewModel.cs:452-458](../../project/SPT.Launcher/ViewModels/ProfileViewModel.cs#L452) | +limpar `LastUpdateText`/`HasLastUpdate`. |
| `SetLastUpdate` | [ProfileViewModel.cs:883-887](../../project/SPT.Launcher/ViewModels/ProfileViewModel.cs#L883) | recebe `SyncResult`; link por total de ações. |
| fechamento | [ProfileViewModel.cs:108-113](../../project/SPT.Launcher/ViewModels/ProfileViewModel.cs#L108) (`IsUpdateVisible`) + [ProfileView.axaml:215-229](../../project/SPT.Launcher/Views/ProfileView.axaml#L215) | barra some ao concluir; status+link ficam. |
| ModUpdateViewModel | [ModUpdateViewModel.cs:310,339](../../project/SPT.Launcher/ViewModels/ModUpdateViewModel.cs#L310) | apply e sucesso passam a usar `SyncMessages` (mesmas strings do Profile). |
| i18n | `LocalizationProvider.cs` (LocaleData + GenerateDefaultLocale) + `English.json`/`Portuguese.json` | novas chaves em **4 lugares** cada (parity all-or-nothing). |

## 3. F12 · Harmony
`N/A` — launcher Avalonia.

## 4. Arquivos

### CRIAR
| Arquivo | Conteúdo |
|---|---|
| `SPT.Launcher/Helpers/SyncMessages.cs` | `ProgressText` + `BuildSummary` (i18n, compartilhado pelos 2 VMs). |
| `SPT.Launcher.Tests/Sync/SyncMessagesTests.cs` | testes: cada Kind → chave certa; BuildSummary só inclui segmentos > 0; frase vazia de ação = "tudo atualizado". |

### MODIFICAR
| Arquivo | Mudança |
|---|---|
| `Sync/SyncPlan.cs` | `SyncProgress` +`Kind`. |
| `Sync/SyncEngine.cs` | passa `action.Kind` no Report (:86). |
| `ViewModels/ProfileViewModel.cs` | applyProgress por ação; mensagem final via `BuildSummary`; reset limpa link; `SetLastUpdate(result)`; guard de geração; barra/`IsUpdateVisible`. |
| `ViewModels/ModUpdateViewModel.cs` | apply e sucesso via `SyncMessages`. |
| `Views/ProfileView.axaml` | separar visibilidade barra × status/link (§5.5). |
| `Views/ModUpdateView.axaml` | idem, onde aplicável. |
| `Helpers/LocalizationProvider.cs` | props novas em `LocaleData` + `GenerateDefaultLocale`. |
| locales `English.json` / `Portuguese.json` | chaves novas (§5.6). |

## 5. Stubs

### 5.1 SyncProgress ganha o Kind

```csharp
// SyncPlan.cs
public sealed class SyncProgress
{
    public SyncProgress(string phase, string currentPath, int current, int total, SyncActionKind? kind = null)
    {
        Phase = phase; CurrentPath = currentPath; Current = current; Total = total; Kind = kind;
    }
    public string Phase { get; }
    public string CurrentPath { get; }
    public int Current { get; }
    public int Total { get; }
    /// <summary>Item 031: tipo da ação em curso (null na fase "checking"). Deixa a UI escolher a frase.</summary>
    public SyncActionKind? Kind { get; }
}

// SyncEngine.cs:86 — passa o Kind:
progress?.Report(new SyncProgress("applying", action.RelativePath, ioDone + 1, ioTotal, action.Kind));
```

### 5.2 SyncMessages — as duas funções i18n compartilhadas

```csharp
// SPT.Launcher/Helpers/SyncMessages.cs
using System.Collections.Generic;
using SPT.Launcher.Sync;

namespace SPT.Launcher.Helpers
{
    /// <summary>Item 031/F: fonte ÚNICA das mensagens de sync, usada pelos dois VMs (Profile e ModUpdate).
    /// Tudo i18n — nada de string montada em código.</summary>
    public static class SyncMessages
    {
        /// <summary>Frase de progresso fiel à ação (não mais tudo "Baixando").</summary>
        public static string ProgressText(SyncActionKind? kind, string path, int current, int total)
        {
            var L = LocalizationProvider.Instance;
            string fmt = kind switch
            {
                SyncActionKind.DeleteExtra        => L.update_deleting,                 // "Removendo: …"
                SyncActionKind.MoveToDisabled     => L.update_archiving,                // "Arquivando (saiu do servidor): …"
                SyncActionKind.MoveDirToDisabled  => L.update_archiving,
                SyncActionKind.SeedCopy           => L.update_seeding,                  // "Instalando padrão: …"
                SyncActionKind.ForceCopy          => L.update_forcing_config,           // "Aplicando config obrigatória: …"
                SyncActionKind.OptionalConfigCopy => L.update_applying_optional_config, // "Aplicando config opcional: …"
                _                                 => L.update_downloading,              // Download (e fallback)
            };
            return string.Format(fmt, path, current, total);
        }

        /// <summary>Frase final composta de segmentos traduzidos (só os &gt; 0). Vazia de ação → "tudo atualizado".</summary>
        public static string BuildSummary(SyncResult r)
        {
            var L = LocalizationProvider.Instance;
            var segs = new List<string>();
            void Add(int n, string fmt) { if (n > 0) segs.Add(string.Format(fmt, n)); }

            Add(r.Updated, L.sync_seg_downloaded);
            Add(r.MovedToDisabled, L.sync_seg_archived);
            Add(r.Deleted, L.sync_seg_removed);
            Add(r.Seeded, L.sync_seg_seeded);
            Add(r.Forced, L.sync_seg_forced);
            Add(r.OptionalConfigApplied, L.sync_seg_optional_config);
            Add(r.ConfigsBackedUp, L.sync_seg_backed_up);
            Add(r.Preserved + r.PreservedDevMode, L.sync_seg_kept);
            Add(r.Errors, L.sync_seg_errors);

            if (segs.Count == 0) return L.update_up_to_date;
            return string.Format(L.sync_completed_prefix, string.Join(" · ", segs)); // "Concluído: a · b · c"
        }
    }
}
```

### 5.3 ProfileViewModel — applyProgress por ação + mensagem final + guard de geração

```csharp
// campo novo — guard contra Progress<T> atrasado (CC-7):
private int _syncRunId;

// no início do run (junto do reset): int myRun = ++_syncRunId;  (capturado no closure)

// applyProgress (substitui :679-683):
var applyProgress = new Progress<SyncProgress>(p =>
{
    if (myRun != _syncRunId) return;            // report de um run já encerrado → ignora (CC-7)
    UpdateProgress = p.Current;
    UpdateStatusText = SyncMessages.ProgressText(p.Kind, p.CurrentPath, p.Current, p.Total);
});

// ... após result = await engine.ExecuteAsync(...):
_syncRunId++;                                    // invalida qualquer report ainda em voo ANTES da msg final

// mensagem final — ramo IoActionCount>0 (:711-715):
UpdateStatusText = SyncMessages.BuildSummary(result);   // i18n, não result.Summary
```

> `result.Summary` (PT) permanece **só** nos `LogManager.Instance.Info/Warning` ([:709,:714](../../project/SPT.Launcher/ViewModels/ProfileViewModel.cs#L709)) — log é interno.

### 5.4 Reset limpa o link + SetLastUpdate por total de ações

```csharp
// reset (:452-458) — adicionar:
LastUpdateText = "";
HasLastUpdate = false;

// SetLastUpdate: assinatura passa a receber o result (chamada :698 vira SetLastUpdate(result)):
private void SetLastUpdate(SyncResult r)
{
    int total = r.Updated + r.MovedToDisabled + r.Deleted + r.Forced + r.Seeded + r.OptionalConfigApplied;
    LastUpdateText = string.Format(LocalizationProvider.Instance.last_update_files_updated, total);
    HasLastUpdate = total > 0;
}
// A outra chamada (LoadLastUpdateInfo, :875, que lê do last-update.json no load da tela) passa a somar
// os counts do relatório (updated+moved+deleted+forced+seeded+optionalConfig), não só counts.updated.
```

### 5.5 Fechar o ciclo — barra some, status/link ficam (ProfileView.axaml)

Hoje o `StackPanel` inteiro (barra + status + link) tem `IsVisible="{Binding IsUpdateVisible}"` ([:215](../../project/SPT.Launcher/Views/ProfileView.axaml#L215)). Separar:

```xml
<!-- StackPanel externo: visível quando há QUALQUER coisa (status OU link) -->
<StackPanel Grid.Row="1" ... IsVisible="{Binding IsUpdateVisible}" ...>
    <TextBlock Text="{Binding UpdateStatusText}" .../>            <!-- fica: mensagem final -->
    <!-- a BARRA (ProgressBar + cancelar) some ao concluir -->
    <Grid ColumnDefinitions="*,Auto,Auto" IsVisible="{Binding IsSyncRunning}">
        <ProgressBar .../>
        <!-- (velocidade rebinda aqui — item 032) -->
        <Button Grid.Column="2" .../>                             <!-- cancelar -->
    </Grid>
    <Button Content="{Binding LastUpdateText}" ... IsVisible="{Binding HasLastUpdate}"/> <!-- fica -->
</StackPanel>
```

- **ProfileView:** `IsSyncRunning` (bool já existente, [ProfileViewModel.cs:187](../../project/SPT.Launcher/ViewModels/ProfileViewModel.cs#L187)) governa só a **barra**; ao concluir vira `false` — garantido no `finally` do run (CR-01-05, fecha a barra mesmo em erro/exceção/cancelamento) —, a barrinha+cancelar somem, e o **texto final**+**link** permanecem no `StackPanel`.
- **ModUpdateView:** o `ModUpdateViewModel` **não tem** `IsSyncRunning` — usa `IsBusy` (= `IsChecking || IsUpdating`, [ModUpdateViewModel.cs:56](../../project/SPT.Launcher/ViewModels/ModUpdateViewModel.cs#L56)). A barra dessa tela (hoje sem gate) passa a `IsVisible="{Binding IsBusy}"`. **CR-01-03:** NÃO copiar `IsSyncRunning` literal pro ModUpdateView (binding a prop inexistente → falha silenciosa → barra some pra sempre).
- `IsUpdateVisible` (Profile) já **não** é desligado no sucesso hoje — é por isso que a barra fica pendurada (o defeito). Passa a significar "há área de update": fica `true` após concluir (mostra resumo+link); some só quando não há nada a mostrar (os `false` de Dev Mode em :520/:653 continuam válidos).

### 5.6 Chaves i18n (todas em PT + EN, parity exata)

**Progresso por ação** — `{0}`=nome, `{1}/{2}`=posição:

| chave | PT | EN | nota |
|---|---|---|---|
| `update_downloading` | Baixando: {0} ({1}/{2}) | Downloading: {0} ({1}/{2}) | existe |
| `update_deleting` | Removendo: {0} ({1}/{2}) | Removing: {0} ({1}/{2}) | existe (órfã → passa a ser usada) |
| `update_archiving` | Arquivando (saiu do servidor): {0} ({1}/{2}) | Archiving (removed from server): {0} ({1}/{2}) | nova |
| `update_seeding` | Instalando padrão: {0} ({1}/{2}) | Installing default: {0} ({1}/{2}) | nova |
| `update_forcing_config` | Aplicando config obrigatória: {0} ({1}/{2}) | Applying required config: {0} ({1}/{2}) | nova |
| `update_applying_optional_config` | Aplicando config opcional: {0} ({1}/{2}) | Applying optional config: {0} ({1}/{2}) | nova |

**Segmentos do resumo final** — `{0}`=contador:

| chave | PT | EN |
|---|---|---|
| `sync_completed_prefix` | Concluído: {0} | Done: {0} |
| `sync_seg_downloaded` | {0} baixados | {0} downloaded |
| `sync_seg_archived` | {0} arquivados | {0} archived |
| `sync_seg_removed` | {0} removidos | {0} removed |
| `sync_seg_seeded` | {0} padrões instalados | {0} defaults installed |
| `sync_seg_forced` | {0} configs obrigatórias | {0} required configs |
| `sync_seg_optional_config` | {0} configs opcionais | {0} optional configs |
| `sync_seg_backed_up` | {0} config(s) sua(s) preservada(s) | {0} of your config(s) kept |
| `sync_seg_kept` | {0} mantidos | {0} kept |
| `sync_seg_errors` | {0} erros | {0} errors |

Cada chave: property `string` no `LocaleData` + linha em `GenerateDefaultLocale` + entrada nos 2 JSON (4 lugares — senão o locale inteiro falha no load, `Json.cs:98-103`). `update_completed` (duplicata de sucesso) é aposentada quando o ModUpdateViewModel passar a usar `BuildSummary`.

## 6. Fluxo de dados

```
SyncEngine, por ação → progress.Report(SyncProgress(phase, path, cur, total, action.Kind))   [SyncEngine.cs:86]
   → applyProgress (ProfileViewModel/ModUpdateViewModel): if(myRun==_syncRunId)
        UpdateStatusText = SyncMessages.ProgressText(p.Kind, …)   ← frase por ação
await ExecuteAsync → _syncRunId++  (invalida reports em voo)
   → UpdateStatusText = SyncMessages.BuildSummary(result)  ← frase final i18n
   → IsSyncRunning = false  → a BARRA some; status final + link ficam (ProfileView.axaml §5.5)
   → SetLastUpdate(result)  → HasLastUpdate por total de ações
início do próximo run → reset zera status+progresso+taxa+LastUpdateText (um ponto só)
```

## 7. Riscos e dependências

- **R-1 (parity i18n):** 16 chaves novas/tocadas × 2 idiomas × 4 lugares. Omitir uma no JSON derruba o locale inteiro (`Json.cs:98-103`). Mitigar: um teste que carrega os 2 JSON e afirma que toda property de `LocaleData` está não-nula. **CR-01-05:** o teste deve desserializar com `AllowNullValues: true` (senão `LoadClassWithoutSaving` devolve o objeto **inteiro** null ao 1º campo faltante → não aponta qual chave falta). E `update_completed` (aposentada) sai nos **4 lugares juntos** — remoção parcial ou vira lixo (property sem uso) ou quebra o load (JSON sem a property que o `LocaleData` espera). `GenerateDefaultLocale` já é subconjunto incompleto — a garantia real são os 2 JSON completos + este teste.
- **R-2 (guard de geração — defensivo, CR-01-02):** a review confirmou que o **fechamento do ciclo** vem da **separação barra×status** + `IsSyncRunning=false` no `finally` (§5.5) — **não** do `_syncRunId`. Como `Progress<T>` posta no mesmo `SynchronizationContext` em FIFO e a continuação (que faz `BuildSummary`) só é postada quando o `ExecuteAsync` **retorna**, todo report já foi drenado antes do resumo → o guard **nunca dispara** na arquitetura atual. Mantido como cinto-e-suspensório (inócuo; protege se alguém introduzir `ConfigureAwait(false)`/report de background). ⚠️ A causa exata do print (barra presa em "Downloading" sem resumo) **não foi reproduzida** — pode ser exceção/travamento antes da msg final (hip. 2 do kickoff), não a corrida; o `IsSyncRunning=false` no `finally` fecha a barra em qualquer caso, mas **confirmar a causa é gate in-game** (`/g-diagnose` com repro).
- **R-3 (IsUpdateVisible muda de semântica):** deixa de significar "sync rodando" e passa a "há área de update". Auditar todos os usos ([:453,:520,:653](../../project/SPT.Launcher/ViewModels/ProfileViewModel.cs#L453)) — o `false` dos caminhos Dev Mode/manifesto-falhou deve continuar escondendo a área quando não há resultado a mostrar.
- **R-4 (coordenação com 032):** o reset da taxa (§5.4 do 032) e o rebinding da velocidade (Grid.Column=1 de §5.5) vivem nos mesmos blocos — implementar os dois juntos para não colidir.
- **R-5 (ModUpdateViewModel):** tem lista visual por-arquivo (`UpdateFileStatus`) que não muda; só a mensagem de sucesso/progresso passa a usar `SyncMessages`.

## 8. Checklist de implementação

1. `SyncPlan.cs`: `SyncProgress` +`Kind`. `SyncEngine.cs:86`: passar `action.Kind`.
2. `SyncMessages.cs` (novo) + `SyncMessagesTests.cs`.
3. i18n: 4 novas + reusar `update_deleting` + 10 segmentos — em `LocaleData`, `GenerateDefaultLocale`, `English.json`, `Portuguese.json`.
4. `ProfileViewModel`: applyProgress por ação; `_syncRunId` guard; `BuildSummary` na msg final; reset limpa link; `SetLastUpdate(result)`; `IsSyncRunning`/`IsUpdateVisible` no fechamento.
5. `ProfileView.axaml`: separar barra × status/link (§5.5).
6. `ModUpdateViewModel` + `ModUpdateView.axaml`: usar `SyncMessages`; mesma separação.
7. Teste de parity i18n (toda property de `LocaleData` não-nula nos 2 locales).
8. `dotnet build` + `dotnet test` verdes. `/code-review`. Gate in-game: desligar um mod-pasta e ver "Arquivando…" + resumo final traduzido + barra que fecha.

## 9. Conformidade com skills

| Item | Status | Evidência |
|---|---|---|
| Lifecycle de raid / Harmony / leak | N/A | Launcher pré-jogo; UI (spec §CA N/A). |
| Coop/Fika | ✅ | Só texto de UI; um mod coop-safe preservado é rotulado "mantido", não "arquivado" (SyncMessages via Kind; spec CA-031.1). |
| Thread-safety | ✅ | `Progress<T>` já posta na UI thread; guard `_syncRunId` evita sobrescrita por report atrasado (§5.3, R-2). |
| i18n parity | ✅ | Teste de parity (R-1); 4 lugares por chave; loader all-or-nothing respeitado. |
| Sem string hardcoded na UI | ✅ | `BuildSummary` compõe de chaves; `Summary` PT fica só em log (§5.3). |

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-08-02 | Guilherme | Spec técnica via `/create-technical-spec` (mapeamento por 2 exploradores). Arquitetura: `SyncProgress`+`Kind`, helper `SyncMessages` (i18n, compartilhado pelos 2 VMs — mata o `Summary` PT e consolida o achado F), guard de geração `_syncRunId`, separação barra×status/link. 9 pontos de extensão. Coordenada com o 032 (reset + UI). |
| 2026-08-02 | Guilherme | `/review-technical-spec` review 01 (sub-agent adversarial) — 0 🔴 · 2 🟡 · 3 🟢 aplicados: ModUpdateView usa `IsBusy` (não `IsSyncRunning`, inexistente lá); `IsSyncRunning=false` no `finally`; guard `_syncRunId` reclassificado como defensivo (fechamento vem da separação barra×status); teste de parity com `AllowNullValues`; `Errors` redundante documentado. |
