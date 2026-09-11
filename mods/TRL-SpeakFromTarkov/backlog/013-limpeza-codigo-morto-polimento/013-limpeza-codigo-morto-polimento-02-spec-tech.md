# 013 — Limpeza de Código Morto e Polimento · Spec Técnica

**Mod:** TRL-SpeakFromTarkov
**Spec funcional:** [013-limpeza-codigo-morto-polimento-01-spec.md](013-limpeza-codigo-morto-polimento-01-spec.md)
**Criado:** 2026-09-09

> Fonte primária de verdade para qualquer assinatura, fórmula ou ponto de patch: [references/eft-decompiled/Assembly-CSharp/](../../../../references/eft-decompiled/Assembly-CSharp/).

## 1. Estratégia

Nove correções pequenas e independentes, sem relação entre si além da severidade baixa/média. Uma delas (`AUD-03-11`) é a única que toca um patch Harmony existente; as demais são limpeza de código próprio do mod.

**`AUD-03-11` — mudança de estratégia (não é só "corrigir o Postfix"):** confirmei em `references/eft-decompiled/Assembly-CSharp/EFT/Player.cs:28592-28599` que `Init` é `async Task` e faz `await JobScheduler.Yield()` **antes** de `Profile = profile;` (linha 28599). Um Postfix Harmony dispara no primeiro `await` que efetivamente suspende — ou seja, antes de `Profile` ser atribuído. O guard atual (`__instance.IsYourPlayer`, `GameSessionPatcher.cs:32`) não depende de `Profile`, então **não há bug ativo hoje** — mas o padrão é frágil pra qualquer lógica futura. Duas opções técnicas, nesta ordem de preferência:
- **Opção A (preferida em teoria):** `[HarmonyPatch(typeof(EFT.Player), nameof(EFT.Player.Init), MethodType.Async)]` — o HarmonyX (usado neste projeto via `SPT.Reflection.Patching`) tem suporte dedicado a patchear o `MoveNext` gerado pelo compilador pra métodos `async`, fazendo o Postfix disparar de fato ao final da execução lógica.
- **Opção B (a implementada):** remover o Postfix de `Player.Init` e mover a chamada de `StartVoipCapture()`/`SetGameStateChannel(true)` para dentro do `Update()` já existente em `VoipController.cs`, reaproveitando o MESMO guard que o próprio `VoipController` já usa em outro ponto (`Singleton<EFT.GameWorld>.Instantiated && Singleton<EFT.GameWorld>.Instance.MainPlayer != null`, confirmado em `Core/VoipController.cs:277-279`) com uma flag one-shot (`_voipCaptureStarted`) pra disparar só uma vez por raid. Essa referência (`GameWorld.MainPlayer`) só é preenchida em `RegisterPlayer` (`Assembly-CSharp/EFT/GameWorld.cs:2277`, `MainPlayer = player;` quando `IsYourPlayer`), que roda depois de `Player.Init` estar logicamente completo — timing correto garantido por evidência de Assembly, sem depender de detalhes de implementação do HarmonyX.

**Decisão tomada na implementação (`/code-mod`):** a Opção A foi descartada sem tentativa — a sintaxe exata do `MethodType.Async` é documentação do HarmonyX (biblioteca de terceiros, fora da hierarquia de evidência EFT/SPT/FIKA deste repo) e não pôde ser verificada localmente; arriscar uma sintaxe não confirmada teria risco de erro de compilação sem benefício sobre a Opção B, que já tinha evidência de Assembly completa e comportamento idêntico ao original. Implementada a Opção B integralmente.

## 2. Pontos de patch

| Alvo (Assembly) | Tipo | Motivo |
|---|---|---|
| [`EFT/Player.cs:28592`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L28592) | Postfix (`MethodType.Async`, Opção A) ou removido (Opção B) | `AUD-03-11` — timing do Postfix em método `async Task`. |
| [`EFT.UI.DragAndDrop.BoundSlotView.RefreshSelectView`](../../../../references/eft-decompiled/Assembly-CSharp/) | Finalizer (já existente, `GameSessionPatcher.cs:207-225`) | `AUD-03-18` — restringir a supressão ao tipo de exceção correto. Tipo resolvido via `AccessTools.TypeByName` (não citado por linha porque o nome não é ofuscado — string literal no próprio patch). |

## 3. Novas propriedades F12 (BepInEx)

Nenhuma.

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `Audio/RemoteSpeaker.cs` | MODIFICAR | Destrói o `AudioClip` em `OnDestroy` (AUD-03-09); remove `packetQueue` morto e corrige o comentário (AUD-03-12); reutiliza array de `Vector3[4]` como campo (AUD-03-16, ver item 011/012 — na verdade este é o `InRaidVoipHUD`, corrigido abaixo). |
| `UI/InRaidVoipHUD.cs` | MODIFICAR | Reseta `_originalPosCaptured` quando `_battleStancePanel` volta a null (AUD-03-13); move `Vector3[4]` pra campo de instância (AUD-03-16). |
| `Network/SftNetwork.cs` | MODIFICAR | Troca os 2 `catch { }` de requisição HTTP por `LogErrorThrottled` (AUD-03-14, linha 338). |
| `UI/MenuVoipHUD.cs` | MODIFICAR | Mesma troca de `catch { }` por `LogErrorThrottled` (AUD-03-14, linha 192). |
| `GameSessionPatcher.cs` | MODIFICAR | Remove `FikaVoipSendPatch`/`FikaVoipReceivePatch` (AUD-03-15); ajusta `PlayerInitPatch` (AUD-03-11, ver seção 1); tipa o `Finalizer` de `BoundSlotViewRefreshSelectViewPatch` (AUD-03-18). |
| `Properties/AssemblyInfo.cs` | MODIFICAR | `AssemblyTitle`/`AssemblyProduct` → `TRL-SpeakFromTarkov`; `AssemblyVersion`/`AssemblyFileVersion` sincronizados com a versão atual do plugin (AUD-03-17). |

## 5. Stubs de código

```csharp
// Audio/RemoteSpeaker.cs — AUD-03-09: destruir o AudioClip

private AudioClip? _streamClip; // NOVO — guarda a referência criada na linha 128 atual

void Initialize(/* assinatura existente */)
{
    // ... corpo existente até a criação do clip (linha 128 atual) ...
    var clip = AudioClip.Create("SftStream", sampleRate, 1, sampleRate, false);
    _streamClip = clip; // NOVO
    audioSource.clip = clip;
    // ... resto do método sem mudança ...
}

void OnDestroy()
{
    decoder = null!;
    if (_streamClip != null) // NOVO
    {
        Destroy(_streamClip);
        _streamClip = null;
    }
}
```

```csharp
// Audio/RemoteSpeaker.cs — AUD-03-12: remover campo morto e corrigir comentário

// ANTES (linha 41 atual): private ConcurrentQueue<byte[]> packetQueue = new ConcurrentQueue<byte[]>();
// REMOVIDO — nunca usado; EnqueuePacket decodifica direto no streamBuffer (linha 170 atual).

// ANTES (linha 169 atual): // Decodificação Off-Thread imediata no callback de recepção da rede
// AGORA:
// Decodificação síncrona no callback do NetPacketProcessor. Roda na MAIN THREAD (PollEvents()
// só é chamado do Update() de FikaClient/FikaServer — não existe "thread de rede" separada
// neste ponto, confirmado em references/fika-plugin/Fika.Core/Networking/{FikaClient,FikaServer}.cs).
if (decoder != null && opusDecodeBuffer != null && streamBuffer != null)
{
    // ... corpo sem mudança ...
}
```

```csharp
// UI/InRaidVoipHUD.cs — AUD-03-13: reset de _originalPosCaptured entre raids

private void Update()
{
    if (Processor == null) return;

    if (_battleStancePanel == null && Comfort.Common.Singleton<EFT.GameWorld>.Instantiated && Comfort.Common.Singleton<EFT.GameWorld>.Instance.MainPlayer != null)
    {
        _originalPosCaptured = false; // NOVO — Unity retorna "== null" pra objeto destruído; ao perder
                                       // a referência da raid anterior, a próxima captura não deve herdar a posição antiga.
        _searchTimer += Time.deltaTime;
        // ... resto do bloco de busca sem mudança ...
    }
    // ... resto do método sem mudança ...
}
```

```csharp
// UI/InRaidVoipHUD.cs — AUD-03-16: Vector3[4] como campo, não alocado por chamada

private readonly Vector3[] _panelCorners = new Vector3[4]; // NOVO, substitui a linha 273 atual

// ... no método que hoje faz "Vector3[] corners = new Vector3[4]; rectTransform.GetWorldCorners(corners);" ...
rectTransform.GetWorldCorners(_panelCorners); // reusa o campo, zero alocação por chamada
float panelLeft = _panelCorners[0].x;
float panelBottomOnScreen = Screen.height - _panelCorners[0].y;
float panelTopOnScreen = Screen.height - _panelCorners[1].y;
```

```csharp
// Network/SftNetwork.cs — AUD-03-14 (linha 338 atual, dentro de BroadcastChannelAnnouncement)
// ANTES: catch { }
catch (Exception ex)
{
    LogErrorThrottled("Erro ao notificar servidor SPT sobre anúncio de canal (sft/channels/announce)", ex);
}
```

```csharp
// UI/MenuVoipHUD.cs — AUD-03-14 (linha 192 atual, dentro de FetchServerChannels)
// ANTES: catch { }
catch (Exception ex)
{
    Network.SftNetwork.LogErrorThrottled("Erro ao buscar canais do servidor SPT (sft/channels/list)", ex);
}
```

```csharp
// GameSessionPatcher.cs — AUD-03-15: remover os 2 patches nunca exercitados
// REMOVER inteiramente: internal class FikaVoipSendPatch (linhas 76-89 atuais)
// REMOVER inteiramente: internal class FikaVoipReceivePatch (linhas 91-104 atuais)

// Corrigido na Review 01 (PA-01-01): as chamadas .Enable() NÃO estão em GameSessionPatcher.Init()
// (que só tem PlayerInitPatch/PlayerOnDeadPatch/GameWorldDisposePatch) — estão em VOIPPlugin.cs.
// VOIPPlugin.cs — REMOVER estas 2 linhas:
// VOIPPlugin.cs:328 — new GameSessionPatcher.FikaVoipSendPatch().Enable();
// VOIPPlugin.cs:329 — new GameSessionPatcher.FikaVoipReceivePatch().Enable();
```

```csharp
// GameSessionPatcher.cs — AUD-03-18: Finalizer tipado (linhas 215-224 atuais)
[PatchFinalizer]
static System.Exception? Finalizer(System.Exception? __exception)
{
    if (__exception is System.NullReferenceException)
    {
        // Absorve NRE de BoundSlotView durante o spawn da raid no coop — motivo original do patch.
        return null;
    }
    return __exception; // qualquer outra exceção volta a subir normalmente, em vez de ser silenciada
}
```

```csharp
// GameSessionPatcher.cs — AUD-03-11, Opção A (preferida, ver seção 1 — TODO confirmar sintaxe HarmonyX)
internal class PlayerInitPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        // ref: Assembly-CSharp/EFT/Player.cs:28592
        return AccessTools.Method(typeof(EFT.Player), nameof(EFT.Player.Init));
    }

    // TODO confirmar: MethodType.Async no atributo [HarmonyPatch] ou equivalente do HarmonyX
    // pra mirar no MoveNext gerado pelo compilador em vez do método async "de fachada".

    [PatchPostfix]
    static void Postfix(EFT.Player __instance)
    {
        if (__instance.IsYourPlayer && Core.VoipController.Instance != null)
        {
            Core.VoipController.Instance.SetGameStateChannel(true);
            Core.VoipController.Instance.StartVoipCapture();
        }
    }
}
```

```csharp
// Core/VoipController.cs — AUD-03-11, Opção B (fallback caso a Opção A não seja viável)
private bool _voipCaptureStarted = false; // NOVO

void Update()
{
    // ... guards existentes (linha 260-261 atuais) ...

    // NOVO — substitui o Postfix de Player.Init inteiramente.
    // ref: Assembly-CSharp/EFT/GameWorld.cs:2277 — MainPlayer só é preenchido dentro de
    // RegisterPlayer quando IsYourPlayer, ou seja, depois do Init lógico estar completo.
    if (!_voipCaptureStarted && Singleton<EFT.GameWorld>.Instantiated && Singleton<EFT.GameWorld>.Instance.MainPlayer != null)
    {
        _voipCaptureStarted = true;
        SetGameStateChannel(true);
        StartVoipCapture();
    }

    // ... resto do Update() sem mudança ...
}

// NOVO — reseta a flag one-shot pra próxima raid poder chamar StartVoipCapture() de novo.
internal void ResetVoipCaptureStartedFlag() => _voipCaptureStarted = false;
```

```csharp
// GameSessionPatcher.cs — Opção B (fallback): reset do _voipCaptureStarted no fim da raid
// (corrigido na Review 01, PA-01-02 — sem isso, a captura de voz quebraria a partir da 2ª raid)
internal class GameWorldDisposePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(GameWorld), nameof(GameWorld.Dispose));
    }

    [PatchPrefix]
    static void Prefix()
    {
        if (Core.VoipController.Instance != null)
        {
            Core.VoipController.Instance.SetGameStateChannel(false);
            Core.VoipController.Instance.ResetVoipCaptureStartedFlag(); // NOVO — só necessário se a Opção B for a usada
        }
    }
}
```

## 6. Fluxo de dados

```
[AUD-03-09] RemoteSpeaker.Initialize() → AudioClip.Create() → _streamClip (novo campo)
  → RemoteSpeaker.OnDestroy() → Destroy(_streamClip)

[AUD-03-11, Opção B] GameWorld.RegisterPlayer (Assembly-CSharp/EFT/GameWorld.cs:2260-2278)
  → MainPlayer = player (linha 2277, só quando IsYourPlayer)
  → VoipController.Update() detecta MainPlayer != null pela 1ª vez → StartVoipCapture()/SetGameStateChannel(true)

[AUD-03-13] InRaidVoipHUD.Update() detecta _battleStancePanel == null (Unity fake-null pós-raid)
  → reseta _originalPosCaptured=false → próxima captura de posição não herda valor da raid anterior

[AUD-03-14] SftNetwork.BroadcastChannelAnnouncement / MenuVoipHUD.FetchServerChannels
  → falha HTTP → catch tipado → LogErrorThrottled (em vez de catch{} silencioso)
```

## 7. Riscos e dependências

- **AUD-03-11 é o item de maior risco desta rodada:** ambas as opções tocam o momento em que a captura de voz do jogador local começa. Testar cuidadosamente que `StartVoipCapture()` ainda dispara exatamente 1x por raid, sem atraso perceptível em relação ao comportamento atual.
- **`FikaVoipSendPatch`/`FikaVoipReceivePatch` (AUD-03-15):** confirmar antes de remover que nenhuma outra parte do mod ainda referencia essas classes (import/uso direto) — busca textual simples resolve.
- **Nenhuma dependência com os itens 010/011/012/014/015** — arquivos tocados aqui (`RemoteSpeaker.cs`, `InRaidVoipHUD.cs`, `SftNetwork.cs`, `MenuVoipHUD.cs`, `GameSessionPatcher.cs`, `AssemblyInfo.cs`) não se sobrepõem às mudanças dos outros itens desta rodada, **exceto** `RemoteSpeaker.cs`, que também é tocado pelo item `011` (campos de cache de listener) — aplicar os dois itens em sequência, não em paralelo, pra evitar conflito de merge no mesmo arquivo.

## 8. Checklist de implementação

- [x] `RemoteSpeaker.cs`: `AudioClip` destruído em `OnDestroy` (AUD-03-09).
- [x] `RemoteSpeaker.cs`: remover `packetQueue`, corrigir comentário da linha 169 (AUD-03-12).
- [x] `InRaidVoipHUD.cs`: resetar `_originalPosCaptured` (AUD-03-13); mover `Vector3[4]` pra campo (AUD-03-16).
- [x] `SftNetwork.cs` e `MenuVoipHUD.cs`: trocar os 2 `catch { }` por `LogErrorThrottled` (AUD-03-14).
- [x] `GameSessionPatcher.cs`: remover `FikaVoipSendPatch`/`FikaVoipReceivePatch` (linhas 76-104). `VOIPPlugin.cs`: remover as chamadas `.Enable()` correspondentes (linhas 328-329) (AUD-03-15).
- [x] `GameSessionPatcher.cs`: tipar o `Finalizer` de `BoundSlotViewRefreshSelectViewPatch` (AUD-03-18).
- [x] `GameSessionPatcher.cs`/`VoipController.cs`: aplicada a **Opção B** (poll em `VoipController.Update()` + `ResetVoipCaptureStartedFlag()` em `GameWorldDisposePatch`) — a Opção A (`MethodType.Async` do HarmonyX) foi descartada na implementação por depender de sintaxe de biblioteca de terceiros não verificável nas fontes deste repo (fora da hierarquia de evidência EFT/SPT/FIKA); a Opção B já tinha evidência de Assembly completa (AUD-03-11).
- [x] `AssemblyInfo.cs`: atualizado `AssemblyTitle`/`AssemblyProduct` para `TRL-SpeakFromTarkov`, `AssemblyVersion`/`AssemblyFileVersion` para `1.5.4.0` (sincronizado com `VOIPPlugin.cs:11`) (AUD-03-17).
- [ ] Testar: uma raid completa (captura de voz inicia corretamente 1x); uma exceção sintética injetada em `BoundSlotView.RefreshSelectView` que NÃO seja `NullReferenceException` aparece no log; falha de rede (SPT fora do ar) gera log throttled em vez de silêncio; `AudioClip`s de múltiplos `RemoteSpeaker` são destruídos sem erro ao fim da raid; **2ª raid em diante ainda inicia a captura de voz corretamente** (valida o reset da Opção B). *(Requer teste em jogo.)*
- [ ] Compilar via `dotnet build` com 0 erros/0 avisos. *(Requer `/compile-mod`.)*

## 9. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid: start hook + stop hooks idempotentes — AP-01 | ✅ | `AudioClip.Destroy` em `OnDestroy` fecha o ciclo de vida por `RemoteSpeaker`; `_voipCaptureStarted` (Opção B) é resetado no mesmo ponto onde `SetGameStateChannel(false)` já roda hoje (`GameWorldDisposePatch`). |
| 2 | Filtro MainPlayer/Fika em todo patch que reage a ação de player — AP-02 | ✅ | `PlayerInitPatch` já guarda `__instance.IsYourPlayer` (`GameSessionPatcher.cs:32`) — confirmado que o patch É local-only, resolvendo o `<!-- review -->` da spec funcional sobre efeito colateral em jogadores remotos. |
| 3 | Alvos ofuscados/virtuais resolvidos por assinatura; overrides auditados — AP-03 | ✅ | `EFT.Player.Init` (não ofuscado) confirmado em `Player.cs:28592`; `BoundSlotView.RefreshSelectView` resolvido por nome de string (não ofuscado, mesmo padrão já usado no patch existente). |
| 4 | Mudança de estado via API canônica do EFT; side-effects mapeados — AP-04 | ✅ | Opção B usa `GameWorld.MainPlayer` (API pública já lida em `VoipController.cs`), sem Reflection nova. |
| 5 | Estado entre raids: raid1→exit→raid2 e alt-F4/morte/MIA cobertos | ✅ | `_originalPosCaptured`/`_voipCaptureStarted` explicitamente resetados nos pontos de transição de raid — este é o próprio objetivo do item (AUD-03-13). |
| 6 | Semântica/defaults/faixas de cada ConfigEntry sem ambiguidade — AP-05 | N/A | Nenhum `ConfigEntry` novo ou alterado. |
| 7 | Re-invocação de método patcheado tem reentry-guard — AP-07 | ✅ | `_voipCaptureStarted` (Opção B) é exatamente esse guard — evita chamar `StartVoipCapture()` mais de 1x por raid. |
| 8 | Flags/caches de intercept validados contra o contexto atual — AP-08 | N/A | Nenhum cache de intercept novo neste item. |
| 9 | Todo patch-point reconfirmado no `.cs` do dump — AP-09 | ✅ | `Player.cs:28592-28599`, `GameWorld.cs:2260-2278,991` lidos diretamente nesta sessão. |
| 10 | Skill EFT usada como lever confirmada não-inerte — AP-10 | N/A | Não se aplica. |
| 11 | Pacote FIKA próprio: envelope + `TryGet*` + ... — AP-11 | N/A | Nenhum pacote de rede novo neste item. |

## Histórico

| Data | Evento |
|---|---|
| 2026-09-09 | Spec técnica criada via `/create-technical-spec` |
| 2026-09-09 | Review 01 aplicada — corrigido local real do `.Enable()` dos patches mortos (`VOIPPlugin.cs:328-329`, não `GameSessionPatcher.Init()`); adicionado reset de `_voipCaptureStarted` na Opção B do AUD-03-11 |
