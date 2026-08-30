---
title: "Relatório de Auditoria Técnica de Código — Tarkov Red Line Launcher (Review 01)"
date: 2026-08-29
status: 🟢 Vivo
authors: Antigravity
---

# Relatório de Auditoria Técnica de Código — Tarkov Red Line Launcher (Review 01)

## 1. Resumo Executivo da Auditoria

Auditoria estática profunda realizada em todo o código-fonte do **Tarkov Red Line Launcher (v2.10.2)** nos projetos `SPT.Launcher`, `SPT.Launcher.Base` e `SPT.ByteBanger` (.NET 9.0 / Avalonia UI). A inspeção cruzou contratos com o mod de servidor `mods/TarkovRedLine4.0`, examinou o ciclo de vida de ViewModels, a retenção de recursos nativos (bitmaps e sockets), loops de monitoramento e padrões de concorrência.

| Severidade | Quantidade | Descrição |
|---|---|---|
| 🔴 **Crítico** | 0 | Nenhum crash fatal ou corrupção crítica ativa |
| 🟠 **Alto** | 2 | Retenção de ViewModels por singleton e vazamento de handles nativos de Bitmaps |
| 🟡 **Médio** | 3 | Polling contínuo de processos com churn de handles, APIs de rede legadas e WMI unmanaged |
| 🔵 **Baixo** | 1 | Guard silencioso sem feedback visual no seletor de classes |
| 💡 **Otimização** | 1 | Otimização de despacho no Dispatcher da UI via ReactiveUI Schedulers |

---

## 2. Tabela de Achados

| ID | Severidade | Arquivo / Linha | Categoria | Descrição Resumida |
|---|---|---|---|---|
| `AUD-01-01` | 🟠 Alto | [ProfileViewModel.cs:L309](../project/SPT.Launcher/ViewModels/ProfileViewModel.cs#L309) | Memory Leak | Inscrição estática sem unsubscribe retém instâncias de ViewModel no GC |
| `AUD-01-02` | 🟠 Alto | [BackgroundCarousel.cs:L225](../project/SPT.Launcher/Models/BackgroundCarousel.cs#L225) | Resource Leak | `Bitmap` do Avalonia não é descartado no `Reload()` e `Dispose()` do carrossel |
| `AUD-01-03` | 🟡 Médio | [ProcessMonitor.cs:L45](../project/SPT.Launcher.Base/MiniCommon/ProcessMonitor.cs#L45) | Polling / Handles | `Process.GetProcessesByName` a cada 1s aloca instâncias e handles sem `Dispose` |
| `AUD-01-04` | 🟡 Médio | [Request.cs:L30](../project/SPT.Launcher.Base/MiniCommon/Request.cs#L30) | Conectividade & TLS | APIs legadas `WebRequest`/`ServicePointManager` (SYSLIB0014) com bypass TLS global |
| `AUD-01-05` | 🟡 Médio | [HwidHelper.cs:L64](../project/SPT.Launcher.Base/Helpers/HwidHelper.cs#L64) | COM/WMI Resource | `ManagementObjectCollection` e `ManagementBaseObject` sem bloco `using` |
| `AUD-01-06` | 🔵 Baixo | [ClassSelectionViewModel.cs:L184](../project/SPT.Launcher/ViewModels/ClassSelectionViewModel.cs#L184) | UX / Resiliência | Retorno silencioso sem aviso visual quando `SelectedClass == null` |
| `AUD-01-07` | 💡 Otimização | [ProfileViewModel.cs:L348](../project/SPT.Launcher/ViewModels/ProfileViewModel.cs#L348) | Reatividade | Uso disperso de `Dispatcher.UIThread.Post` em vez de `ObserveOn` reativo |

---

## 3. Detalhamento dos Achados

### AUD-01-01 · Retenção de ViewModels por Inscrição Estática Não Desinscrita
- **Severidade:** 🟠 Alto
- **Localização:** [ProfileViewModel.cs:L309](../project/SPT.Launcher/ViewModels/ProfileViewModel.cs#L309) e [LoginViewModel.cs:L283](../project/SPT.Launcher/ViewModels/LoginViewModel.cs#L283)
- **Causa Raiz:** O construtor registra um delegate anônimo no evento `LauncherSettingsProvider.Instance.PropertyChanged`. Como `LauncherSettingsProvider.Instance` é um singleton que vive durante toda a execução do aplicativo, ele retém uma referência forte para o ViewModel.
- **Impacto Técnico Real:** Sempre que o usuário faz logout e novo login, uma nova instância de `ProfileViewModel` (com todo o seu grafo de objetos: `Carousel`, `ModInfoCollection`, `GameStarter`) é criada, enquanto a instância anterior nunca é liberada pelo Garbage Collector.
- **Proposta de Correção:**
  Mover a escuta para o ciclo de vida reativo `WhenActivated` e gerenciar via `CompositeDisposable`:

```csharp
// ProfileViewModel.cs
this.WhenActivated((CompositeDisposable disposables) =>
{
    PropertyChangedEventHandler onSettingsChanged = (s, e) =>
    {
        if (e.PropertyName == nameof(LauncherSettingsProvider.Instance.CanStartGame))
        {
            this.RaisePropertyChanged(nameof(CanStartGame));
            this.RaisePropertyChanged(nameof(CanVerifyFiles));
        }
    };

    LauncherSettingsProvider.Instance.PropertyChanged += onSettingsChanged;
    Disposable.Create(() => LauncherSettingsProvider.Instance.PropertyChanged -= onSettingsChanged)
        .DisposeWith(disposables);
});
```

- **Decisão:**
  - `[x]` Aceitar sugestão (✅ Aplicado em 2026-08-29 em `ProfileViewModel.cs:L335` e `LoginViewModel.cs:L283`)
  - **Resolução:** Handlers movidos para dentro do `WhenActivated` com `Disposable.Create().DisposeWith(disposables)`.

---

### AUD-01-02 · Ausência de Dispose em `Avalonia.Media.Imaging.Bitmap` no Carrossel
- **Severidade:** 🟠 Alto
- **Localização:** [BackgroundCarousel.cs:L225-250](../project/SPT.Launcher/Models/BackgroundCarousel.cs#L225-L250) e [L298-315](../project/SPT.Launcher/Models/BackgroundCarousel.cs#L298-L315)
- **Causa Raiz:** `Bitmap` implementa `IDisposable` e mantém ponteiros para buffers gráficos nativos (Skia/GPU). Ao chamar `Reload()` para trocar as imagens do servidor ou ao chamar `Dispose()`, o array anterior é substituído sem invocar `Dispose()` nas instâncias existentes.
- **Impacto Técnico Real:** Retenção desnecessária de 20MB a 60MB de memória gráfica não gerenciada a cada recarga de fundos do carrossel até a passagem indeterminada do GC.
- **Proposta de Correção:**

```csharp
// BackgroundCarousel.cs
private void DisposeDecodedBitmaps()
{
    lock (_lock)
    {
        if (_decoded != null)
        {
            for (int i = 0; i < _decoded.Length; i++)
            {
                _decoded[i]?.Dispose();
                _decoded[i] = null;
            }
        }
    }
}

public void Reload()
{
    DisposeDecodedBitmaps();
    var newDescriptors = BuildDescriptors();
    // ...
}

public void Dispose()
{
    if (_disposed) return;
    _disposed = true;
    DisposeDecodedBitmaps();
    // ...
}
```

- **Decisão:**
  - `[x]` Aceitar sugestão (✅ Aplicado em 2026-08-29 em `BackgroundCarousel.cs:L221`)
  - **Resolução:** `DisposeDecodedBitmaps()` implementado com `lock (_lock)` e invocado em `Reload()` e `Dispose()`.

---

### AUD-01-03 · Polling Contínuo de Processos com Churn de Handles Win32
- **Severidade:** 🟡 Médio
- **Localização:** [ProcessMonitor.cs:L43-56](../project/SPT.Launcher.Base/MiniCommon/ProcessMonitor.cs#L43-L56)
- **Causa Raiz:** A cada segundo (1000ms), `Process.GetProcessesByName(processName)` varre a tabela de processos do Windows, instanciando novos objetos `Process` e alocando handles Win32 internos que não são explicitamente liberados.
- **Impacto Técnico Real:** Alocações contínuas de Heap e de descritores de sistema operacional durante todo o tempo em que o jogo está em execução.
- **Proposta de Correção:**
  Garantir descarte dos processos temporários e utilizar eventos quando o processo é encontrado:

```csharp
// ProcessMonitor.cs
private void OnPollEvent(object source, ElapsedEventArgs e)
{
    Process[] clientProcesses = Process.GetProcessesByName(processName);
    try
    {
        if (clientProcesses.Length > 0)
        {
            aliveCallback(this);
            return;
        }
        exitCallback(this);
    }
    finally
    {
        foreach (var p in clientProcesses)
        {
            p.Dispose();
        }
    }
}
```

- **Decisão:**
  - `[x]` Aceitar sugestão (✅ Aplicado em 2026-08-29 em `ProcessMonitor.cs:L43`)
  - **Resolução:** Bloco `try/finally` adicionado garantindo o `Dispose()` imediato de todos os objetos `Process`.

---

### AUD-01-04 · Uso de Transporte Legado (`WebRequest`) e Bypass Global de Certificado TLS
- **Severidade:** 🟡 Médio
- **Localização:** [Request.cs:L30-36, L70-75](../project/SPT.Launcher.Base/MiniCommon/Request.cs#L30-L36)
- **Causa Raiz:** O módulo `Request.cs` utiliza `HttpWebRequest` e `ServicePointManager` (obsoletos no .NET 9 gerando avisos `SYSLIB0014`). O callback `ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };` afeta todas as conexões do `AppDomain`. Além disso, `response.GetResponseStream()` não garante o descarte de `WebResponse`.
- **Impacto Técnico Real:** Avisos de compilação contínuos, ineficiência de pool de conexões e brecha de conformidade de segurança (já catalogada como pendência `[P-1.5]` na memória do projeto).
- **Proposta de Correção:**
  Migrar a infraestrutura de requisição para `HttpClient` singleton com suporte a compressão deflate/zlib nativa, espelhando a arquitetura moderna já implementada em `ServerHeartbeatMonitor.cs`.
- **Decisão:**
  - `[ ]` Pendente (Agendado para sprint de segurança / item 027)

---

### AUD-01-05 · Ausência de Dispose em Coleções e Objetos COM do WMI
- **Severidade:** 🟡 Médio
- **Localização:** [HwidHelper.cs:L62-70](../project/SPT.Launcher.Base/Helpers/HwidHelper.cs#L62-L70)
- **Causa Raiz:** `searcher.Get()` retorna um `ManagementObjectCollection` que implementa `IDisposable`. A iteração sem `using` não libera os enumeradores COM imediatamente.
- **Impacto Técnico Real:** Retenção de interfaces COM nativas do subsistema WMI do Windows até o GC finalizer.
- **Proposta de Correção:**

```csharp
// HwidHelper.cs
using (var searcher = new ManagementObjectSearcher(query))
using (var collection = searcher.Get())
{
    foreach (ManagementObject obj in collection)
    {
        using (obj)
        {
            var value = obj[property]?.ToString();
            if (!string.IsNullOrEmpty(value))
                return value;
        }
    }
}
```

- **Decisão:**
  - `[x]` Aceitar sugestão (✅ Aplicado em 2026-08-29 em `HwidHelper.cs:L62`)
  - **Resolução:** `ManagementObjectCollection` e `ManagementObject` encapsulados em blocos `using`.

---

### AUD-01-06 · Guard Silencioso no Comando de Finalização de Cadastro
- **Severidade:** 🔵 Baixo
- **Localização:** [ClassSelectionViewModel.cs:L184](../project/SPT.Launcher/ViewModels/ClassSelectionViewModel.cs#L184)
- **Causa Raiz:** A linha `if (SelectedClass == null) return;` aborta o comando sem exibir nenhuma mensagem para o usuário.
- **Impacto Técnico Real:** Se a lista de classes demorar a responder e o usuário clicar no botão "ESCOLHER", a UI não responde nem fornece feedback visual explicativo.
- **Proposta de Correção:**
  Adicionar mensagem explicativa ou condicionar a propriedade `CanExecute` do `FinalizeAccountCommand` ao estado de `SelectedClass != null && !IsLoading`.
- **Decisão:**
  - `[x]` Aceitar sugestão (✅ Aplicado em 2026-08-29 em `ClassSelectionViewModel.cs:L182`)
  - **Resolução:** Atribuição dinâmica de `RegisterErrorMsg` com `class_selection_loading` ou `class_selection_none_available`.

---

### AUD-01-07 · Otimização de Despacho de UI via ReactiveUI Schedulers
- **Severidade:** 💡 Otimização
- **Localização:** [ProfileViewModel.cs:L348, L359](../project/SPT.Launcher/ViewModels/ProfileViewModel.cs#L348)
- **Causa Raiz:** Múltiplas chamadas explícitas a `Dispatcher.UIThread.Post(...)` quando a arquitetura já dispõe de `RxApp.MainThreadScheduler` e observáveis reativos.
- **Impacto Técnico Real:** Sobrecarga mínima de enfileiramento assíncrono.
- **Proposta de Correção:**
  Padronizar eventos do servidor e providers para observáveis com `.ObserveOn(RxApp.MainThreadScheduler).Subscribe(...)`.
- **Decisão:**
  - `[ ]` Pendente (Melhoria de qualidade de vida para próxima rodada)

---

## 4. Plano de Ação e Recomendações

1. **Sprint Concluída (Refatoração de Ciclo de Vida e Leaks):**
   - ✅ Corrigido `AUD-01-01` (Unsubscribe em `LauncherSettingsProvider.Instance.PropertyChanged` via `WhenActivated`).
   - ✅ Corrigido `AUD-01-02` (Implementado `DisposeDecodedBitmaps` em `BackgroundCarousel`).
   - ✅ Corrigido `AUD-01-03` (Adicionado descarte explícito de `Process` em `ProcessMonitor.cs`).
   - ✅ Corrigido `AUD-01-05` (Wrapping `using` no WMI de `HwidHelper`).
   - ✅ Corrigido `AUD-01-06` (Feedback explícito quando `SelectedClass == null`).
2. **Próximos Passos (Segurança e .NET 9):**
   - Agendar a modernização de `Request.cs` para `HttpClient` singleton (`AUD-01-04` / `[P-1.5]`).

---

## 5. Memória Consultada
- `launcher/Launcher4.0-v2/memory/sessions.md` (Delta 2026-07-26, `[P-1.1]` a `[P-1.7]`).
