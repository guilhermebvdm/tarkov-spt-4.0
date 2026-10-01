# 011 — Threading e Oclusão de Voz · Spec Técnica

**Mod:** TRL-SpeakFromTarkov
**Spec funcional:** [011-threading-oclusao-voz-01-spec.md](011-threading-oclusao-voz-01-spec.md)
**Criado:** 2026-09-09

> Fonte primária de verdade para qualquer assinatura, fórmula ou ponto de patch: [references/eft-decompiled/Assembly-CSharp/](../../../../references/eft-decompiled/Assembly-CSharp/).

## 1. Estratégia

Nenhum patch Harmony — todo o código tocado é próprio do mod (`RemoteSpeaker.cs`, `BotVoiceBridge.cs`, `VoipProcessor.cs`, `AudioFilter.cs`, `MicrophoneCapturer.cs`). Quatro correções independentes, agrupadas por serem todas do domínio threading/física de áudio:

1. **AUD-03-02:** cachear posição/direção do listener em `Update()` (main thread) e só ler esses campos em `OnAudioFilterRead` (audio thread) — elimina toda chamada a `Camera.main`/`Singleton<GameWorld>` fora da main thread.
2. **AUD-03-03:** corrigir a composição da máscara de oclusão (bitshift faltante).
3. **AUD-03-04:** marcar como `volatile` os campos de estado do `VoipProcessor` compartilhados entre a thread de captura e a main thread.
4. **AUD-03-05:** trocar a escrita campo-a-campo da config do `AudioFilter` por um snapshot imutável trocado por referência (`Interlocked.Exchange`), evitando leitura de config parcialmente atualizada.

**Alternativa descartada para AUD-03-02:** mover o cálculo de distância/panning inteiro para dentro do `Update()` e só passar o resultado final (`finalAttenuation`/`panLeftGain`/`panRightGain`) pro `OnAudioFilterRead`. Descartada porque `Update()` roda 1x/frame (~60-144 Hz) enquanto `OnAudioFilterRead` roda no ritmo do buffer de áudio do Unity — calcular só a posição/direção do listener (2 `Vector3`) no `Update()` e deixar o resto (distância, atenuação, panning) em `OnAudioFilterRead` preserva a granularidade atual da suavização (`Mathf.Lerp` de oclusão, decaimento de `lastSample`) sem reescrever a lógica de DSP, e é a mudança mínima que resolve o problema real (chamada de API Unity fora da main thread).

## 2. Pontos de patch

N/A — nenhum patch Harmony. Todas as mudanças são em métodos próprios do mod, citados na seção 5.

## 3. Novas propriedades F12 (BepInEx)

Nenhuma.

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `Audio/RemoteSpeaker.cs` | MODIFICAR | Cacheia listener pos/right em `Update()`; `OnAudioFilterRead` passa a só ler os campos cacheados; corrige bitmask de oclusão (linha 282). |
| `Audio/BotVoiceBridge.cs` | MODIFICAR | Corrige o mesmo bug de bitmask de oclusão (linha 159) — instância separada do mesmo `_botOcclusionMask`/`_occlusionLayerMask` local a esta classe. |
| `Audio/VoipProcessor.cs` | MODIFICAR | Campos de estado (`RawLevel`, `DisplayLevel`, `PeakLevel`, `IsTransmitting`, `LastOpusBytes`, `IsMuted`, `IsPTTActive`, `CurrentMode`) passam a ter backing field `volatile`. |
| `Audio/AudioFilter.cs` | MODIFICAR | Substitui as propriedades de config individuais por um snapshot imutável (`NoiseGateConfig`) trocado via `Interlocked.Exchange`. |
| `Audio/MicrophoneCapturer.cs` | MODIFICAR | `Update()` passa a montar um `NoiseGateConfig` novo e trocar de uma vez, em vez de escrever campo a campo; `isThreadRunning` passa a `volatile`. |

## 5. Stubs de código

```csharp
// Audio/RemoteSpeaker.cs — novos campos e mudança em Update()/OnAudioFilterRead

private Vector3 _cachedListenerPos = Vector3.zero;
private Vector3 _cachedListenerRight = Vector3.right;

void Update()
{
    try
    {
        // ── NOVO: resolvido 1x por frame na main thread, único lugar onde Camera.main/
        // Singleton<GameWorld> são tocados neste componente a partir de agora. ──
        if (Camera.main != null)
        {
            var camTransform = Camera.main.transform;
            _cachedListenerPos = camTransform.position;
            _cachedListenerRight = camTransform.right;
        }
        else if (Singleton<GameWorld>.Instantiated && Singleton<GameWorld>.Instance.MainPlayer != null)
        {
            var mp = Singleton<GameWorld>.Instance.MainPlayer;
            _cachedListenerPos = mp.Position + Vector3.up * 1.6f;
            _cachedListenerRight = mp.Transform != null ? mp.Transform.Original.right : Vector3.right;
        }

        // ... lógica de re-ancoragem existente (linhas 240-268 atuais), sem mudança ...

        // ── OCLUSÃO FÍSICA (linhas 270-317 atuais) — reaproveita _cachedListenerPos
        // em vez de ler Camera.main de novo; sem mudança na cadência de 200ms ──
        if (Time.time - _lastOcclusionCheckTime >= 0.20f)
        {
            _lastOcclusionCheckTime = Time.time;
            bool occlusionEnabled = VoIPPlugin.EnableOcclusion == null || VoIPPlugin.EnableOcclusion.Value;

            if (occlusionEnabled && currentSpatialBlend > 0f && !isEmergency2DMode)
            {
                if (_occlusionLayerMask == -1)
                {
                    try
                    {
                        // ref: Assembly-CSharp/LayerMaskClass.cs:82,125 (DoorLayer = índice cru, não bitmask)
                        // ref: Assembly-CSharp/LayerMaskClass.cs:58,132 (InteractiveMask = 1 << InteractiveLayer, já correto)
                        // ref: Assembly-CSharp/LayerMaskClass.cs:135 (uso nativo confirma o padrão "1 << DoorLayer")
                        _occlusionLayerMask = LayerMaskClass.HighPolyWithTerrainMask
                            | (1 << LayerMaskClass.DoorLayer)
                            | LayerMaskClass.InteractiveMask;
                    }
                    catch
                    {
                        _occlusionLayerMask = LayerMask.GetMask("HighPolyWithRaycast", "Terrain", "LowPoly", "Interactive", "Doors", "Default");
                    }
                }

                Vector3 targetHeadPos = transform.position;
                Vector3 listenerPos = _cachedListenerPos; // era Camera.main.transform.position

                if (listenerPos != Vector3.zero && Vector3.Distance(listenerPos, targetHeadPos) > 1.2f)
                {
                    _isOccluded = Physics.Linecast(listenerPos, targetHeadPos, out RaycastHit hit, _occlusionLayerMask, QueryTriggerInteraction.Ignore);
                }
                else
                {
                    _isOccluded = false;
                }
            }
            else
            {
                _isOccluded = false;
            }

            _targetOcclusionFactor = _isOccluded ? 0.50f : 1.0f;
            _targetOcclusionAirDamping = _isOccluded ? 0.25f : 1.0f;
        }
    }
    catch (Exception ex)
    {
        VoIPPlugin.Log.LogError($"[SFT] Erro no Update de RemoteSpeaker: {ex.Message}");
    }
}

void OnAudioFilterRead(float[] data, int channels)
{
    // ... cabeçalho de jitter buffer (linhas 327-364 atuais), sem mudança ...

    float distanceAttenuation = 1.0f;
    float airDampingAlpha = 1.0f;
    // ANTES: Vector3 listenerPos = Vector3.zero; + if (Camera.main != null) ...
    // AGORA: leitura pura de campo, zero chamada de API Unity nesta thread.
    Vector3 listenerPos = currentSpatialBlend > 0f ? _cachedListenerPos : Vector3.zero;

    if (currentSpatialBlend > 0f && listenerPos != Vector3.zero)
    {
        float dist = Vector3.Distance(listenerPos, transform.position);
        // ... resto do cálculo de distanceAttenuation/airDampingAlpha (linhas 385-407 atuais), sem mudança ...
    }

    // ... oclusão (linhas 411-423 atuais), sem mudança ...

    float panLeftGain = 1.0f;
    float panRightGain = 1.0f;

    if (currentSpatialBlend > 0f && listenerPos != Vector3.zero)
    {
        // ANTES: Vector3 listenerRight = Vector3.right; + if (Camera.main != null) ...
        Vector3 listenerRight = _cachedListenerRight;
        // ... resto do cálculo de panning (linhas 442-454 atuais), sem mudança ...
    }

    // ... resto do método (linhas 457-501 atuais), sem mudança ...
}
```

```csharp
// Audio/BotVoiceBridge.cs — mesma correção de bitmask, campo local desta classe (linha 159 atual)
if (_botOcclusionMask == -1)
{
    try
    {
        // ref: Assembly-CSharp/LayerMaskClass.cs:82,125,58,132,135 (mesma justificativa do RemoteSpeaker.cs)
        _botOcclusionMask = LayerMaskClass.HighPolyWithTerrainMask
            | (1 << LayerMaskClass.DoorLayer)
            | LayerMaskClass.InteractiveMask;
    }
    catch
    {
        _botOcclusionMask = LayerMask.GetMask("HighPolyWithRaycast", "Terrain", "LowPoly", "Interactive", "Doors", "Default");
    }
}
```

```csharp
// Audio/VoipProcessor.cs — campos convertidos para volatile (float/bool/enum são permitidos
// pela especificação do C# para 'volatile'; RawLevel/DisplayLevel/PeakLevel/LastOpusBytes
// e o modo/mute/PTT cruzam entre a thread de captura e a main thread)

private volatile float _rawLevel;
public float RawLevel { get => _rawLevel; private set => _rawLevel = value; }

private volatile float _displayLevel;
public float DisplayLevel { get => _displayLevel; private set => _displayLevel = value; }

private volatile float _peakLevel;
public float PeakLevel { get => _peakLevel; private set => _peakLevel = value; }

private volatile int _lastOpusBytes;
public int LastOpusBytes { get => _lastOpusBytes; private set => _lastOpusBytes = value; }

private volatile bool _isTransmitting;
public bool IsTransmitting { get => _isTransmitting; private set => _isTransmitting = value; }

// Escritos pela MAIN THREAD (VoipController.cs:361,371,391), lidos pela THREAD DE CAPTURA
// dentro de UpdateTransmittingState() — mesmo motivo, direção invertida.
private volatile bool _isMuted;
public bool IsMuted { get => _isMuted; set => _isMuted = value; }

private volatile bool _isPTTActive;
public bool IsPTTActive { get => _isPTTActive; set => _isPTTActive = value; }

// VoipMode é enum com base int — permitido por volatile pela especificação do C#.
private volatile VoipMode _currentMode = VoipMode.VAD;
public VoipMode CurrentMode { get => _currentMode; set => _currentMode = value; }
```

```csharp
// Audio/AudioFilter.cs — snapshot imutável trocado por referência em vez de N propriedades soltas

/// <summary>Imutável — cada troca de config cria uma instância nova, nunca modifica uma existente
/// em uso. Isso garante que a thread de captura sempre vê um conjunto de valores coerente entre
/// si (nunca metade nova, metade antiga), o que campos volatile individuais não garantiriam.</summary>
public sealed class NoiseGateConfig
{
    public readonly float OpenThreshold;
    public readonly float CloseThreshold;
    public readonly float HoldTime;
    public readonly bool UseRNNoise;
    public readonly float RNNoiseVADThreshold;
    public readonly float RNNoiseGateHold;
    public readonly bool EnableAGC;
    public readonly bool EnableLimiter;
    public readonly bool Is2DChannel;
    public readonly float LpfAlpha; // já pré-calculado (era o setter de LPFCutoffHz)

    public NoiseGateConfig(float openThreshold, float closeThreshold, float holdTime,
        bool useRNNoise, float rnNoiseVadThreshold, float rnNoiseGateHold,
        bool enableAGC, bool enableLimiter, bool is2DChannel, float lpfCutoffHz, int sampleRate)
    {
        OpenThreshold = openThreshold;
        CloseThreshold = closeThreshold;
        HoldTime = holdTime;
        UseRNNoise = useRNNoise;
        RNNoiseVADThreshold = rnNoiseVadThreshold;
        RNNoiseGateHold = rnNoiseGateHold;
        EnableAGC = enableAGC;
        EnableLimiter = enableLimiter;
        Is2DChannel = is2DChannel;

        float dt = 1f / sampleRate;
        float rc = 1f / (2f * Mathf.PI * lpfCutoffHz);
        LpfAlpha = dt / (rc + dt);
    }
}

// Campo trocado por referência — leitura/escrita de referência já é atômica em .NET,
// sem precisar de volatile explícito no campo em si (Interlocked.Exchange já dá a barreira de memória).
private NoiseGateConfig _config = new NoiseGateConfig(0.008f, 0.005f, 0.15f, true, 0.85f, 0.15f, false, true, false, 4000f, 48000);

public void UpdateConfig(NoiseGateConfig newConfig) => System.Threading.Interlocked.Exchange(ref _config, newConfig);

// Corrigido na Review 01 (PA-01-01): o Apply() real (AudioFilter.cs:134-155) tem duas etapas
// depois do bloco RNNoise/Fallback — ApplyAGC (canais 2D) e ApplyLimiter (proteção final contra
// clipping) — que o stub original omitia. Nenhuma das duas lê campos de NoiseGateConfig, então
// só precisam ler config.EnableAGC/Is2DChannel/EnableLimiter pra decidir se disparam, sem mudar
// de assinatura. ApplyRNNoise também voltou à assinatura original (sem `config`) — ela não lê
// nenhum campo do NoiseGateConfig (ver PA-01-02).
public void Apply(float[] buffer)
{
    var config = _config; // snapshot único, coerente, lido 1x no topo do método (thread de captura)

    if (_rnAvailable && config.UseRNNoise)
    {
        ApplyHPF(buffer);
        if (config.LpfAlpha > 0) ApplyLPF(buffer, config.LpfAlpha);
        ApplyRNNoise(buffer);
    }
    else
    {
        ApplyFallback(buffer, config);
    }

    // AGC é aplicado exclusivamente em canais 2D (menu ou spectator) para não distorcer proximidade 3D
    if (config.EnableAGC && config.Is2DChannel)
    {
        ApplyAGC(buffer); // não lê NoiseGateConfig — sem mudança de assinatura
    }

    // O limiter é a última barreira de proteção de áudio
    if (config.EnableLimiter) ApplyLimiter(buffer); // não lê NoiseGateConfig — sem mudança de assinatura
}

private void ApplyFallback(float[] buf, NoiseGateConfig config)
{
    ApplyHPF(buf);
    ApplyNoiseGate(buf, config); // ApplyNoiseGate é quem de fato lê OpenThreshold/CloseThreshold/HoldTime
}
```

```csharp
// Audio/MicrophoneCapturer.cs — Update() monta o snapshot de uma vez em vez de escrever campo a campo
// (trecho que substitui as linhas 237-246 atuais)
if (audioFilter != null)
{
    var newConfig = new AudioFilter.NoiseGateConfig(
        VoIPPlugin.NoiseGateThreshold.Value,
        VoIPPlugin.NoiseGateThreshold.Value * 0.6f,
        VoIPPlugin.NoiseGateHoldMs.Value / 1000f,
        VoIPPlugin.UseRNNoise.Value,
        VoIPPlugin.RNNoiseVADThreshold.Value,
        VoIPPlugin.RNNoiseGateHoldMs.Value / 1000f,
        VoIPPlugin.EnableAGC.Value,
        VoIPPlugin.EnableLimiter.Value,
        Core.VoipController.Instance != null && Core.VoipController.Instance.CurrentChannel != 0,
        VoIPPlugin.LPFCutoff.Value,
        targetSampleRate);
    audioFilter.UpdateConfig(newConfig);
}

// ── AUD-03-11 (corner case do review /review-spec): teardown de thread ──
// isThreadRunning JÁ é o mecanismo de shutdown limpo (StopCapture seta false e faz
// captureThread.Join(500)); só precisa de 'volatile' pra garantir que a thread de captura
// veja a mudança sem depender de timing/cache de registrador.
private volatile bool isThreadRunning;
```

## 6. Fluxo de dados

```
[Main thread — Update() de RemoteSpeaker, 1x/frame]
  Camera.main / Singleton<GameWorld> → _cachedListenerPos, _cachedListenerRight (campos)

[Unity audio thread — OnAudioFilterRead(), ritmo do buffer de áudio]
  lê _cachedListenerPos/_cachedListenerRight (zero chamada de API Unity aqui)
  → distanceAttenuation, panLeftGain/panRightGain → data[] de saída

[Main thread — MicrophoneCapturer.Update(), 1x/frame]
  VoIPPlugin.*.Value (ConfigEntry) → new AudioFilter.NoiseGateConfig(...) → audioFilter.UpdateConfig()
    (Interlocked.Exchange troca a referência _config)

[Thread de captura — AudioFilter.Apply(), por frame de 20-40ms de áudio]
  var config = _config; (snapshot único e coerente) → ApplyHPF/ApplyRNNoise/ApplyFallback(buffer, config)

[Main thread — VoipController.HandleKeys(), 1x/frame]
  processor.CurrentMode = ... / processor.IsMuted = ... / processor.IsPTTActive = ...  (campos volatile)

[Thread de captura — VoipProcessor.ProcessAudio()/UpdateTransmittingState(), por frame de captura]
  lê CurrentMode/IsMuted/IsPTTActive (volatile) → decide IsTransmitting (volatile, escrito aqui)

[Main thread — HUDs (VoipHUD, InRaidVoipHUD, VoiceCalibrationHUD), a cada OnGUI/Update]
  leem RawLevel/DisplayLevel/PeakLevel/IsTransmitting/LastOpusBytes (volatile, escritos pela thread de captura)
```

## 7. Riscos e dependências

- **`ApplyRNNoise`/`ApplyFallback`/`ApplyLPF` precisam de assinatura nova** (recebem `NoiseGateConfig` em vez de ler propriedades da própria instância) — mudança mecânica, mas toca várias assinaturas internas de `AudioFilter.cs`; revisar todos os call sites internos ao implementar.
- **Nenhum patch Harmony afetado.**
- **Ordem de implementação:** aplicar este item (`011`) antes do `015-ambiente-acustico-reverb-distancia/`, que reajusta a mesma curva de distância em `OnAudioFilterRead` — já documentado na spec funcional do `015`.
- **`isThreadRunning` volatile** é uma correção pequena e isolada, mas compartilha arquivo (`MicrophoneCapturer.cs`) com a troca de `NoiseGateConfig` — aplicar as duas mudanças na mesma revisão evita dois diffs no mesmo método.
- **`RNNoiseVADThreshold`/`RNNoiseGateHold` aparentam não ter consumidor ativo** em `ApplyRNNoise` hoje (ela usa constantes hardcoded — `0.0003f`, `0.20f`, `0.01f` — em vez dessas duas propriedades). Incluí-las no `NoiseGateConfig` pela mesma razão de threading dos demais campos ainda é correto (corrige a race condition de qualquer forma), mas é possível candidato a achado de código morto numa auditoria futura — fora do escopo deste item (Review 01, PA-01-02).

## 8. Checklist de implementação

- [x] `RemoteSpeaker.cs`: adicionar `_cachedListenerPos`/`_cachedListenerRight`, preenchê-los no topo do `Update()`, substituir os 2 blocos `Camera.main`/`Singleton<GameWorld>` dentro de `OnAudioFilterRead` por leitura desses campos.
- [x] `RemoteSpeaker.cs`: corrigir `_occlusionLayerMask` (linha 282) para `HighPolyWithTerrainMask | (1 << DoorLayer) | InteractiveMask`.
- [x] `BotVoiceBridge.cs`: mesma correção em `_botOcclusionMask` (linha 159).
- [x] `VoipProcessor.cs`: converter `RawLevel`, `DisplayLevel`, `PeakLevel`, `LastOpusBytes`, `IsTransmitting`, `IsMuted`, `IsPTTActive`, `CurrentMode` para backing field `volatile`.
- [x] `AudioFilter.cs`: criar `NoiseGateConfig`, adicionar `UpdateConfig()`. `ApplyLPF` (recebe `config.LpfAlpha`), `ApplyFallback`/`ApplyNoiseGate` (recebem `config` inteiro) e o próprio `Apply()` (lê `config.EnableAGC`/`Is2DChannel`/`EnableLimiter` pras 2 chamadas finais) mudaram de assinatura — `ApplyHPF`, `ApplyRNNoise`, `ApplyAGC` e `ApplyLimiter` mantiveram a assinatura atual (corrigido na Review 01, PA-01-01).
- [x] `MicrophoneCapturer.cs`: `Initialize()` e `Update()` montam `NoiseGateConfig` via `BuildNoiseGateConfig()` (helper novo, evita duplicar a construção nos 2 pontos) e chamam `UpdateConfig()`. `isThreadRunning` já era `volatile` no código atual (`MicrophoneCapturer.cs:34`) — nenhuma mudança necessária aí.
- [ ] Testar: falar atrás de porta fechada (oclusão deve funcionar); trocar mute/PTT/modo durante fala ativa (sem atraso perceptível); trocar limiar do RNNoise no F12 durante transmissão (sem corte); múltiplos `RemoteSpeaker` simultâneos com câmera trocando de espectador pra jogador vivo (sem `UnityException` no log). *(Requer teste em jogo — fora do escopo de `/code-mod`.)*
- [ ] Compilar via `dotnet build` com 0 erros/0 avisos. *(Requer `/compile-mod`.)*

## 9. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid: start hook + stop hooks idempotentes — AP-01 | ✅ | `isThreadRunning`/`StopCapture()` já implementam start/stop idempotente (`MicrophoneCapturer.cs:151-168`); este item só adiciona `volatile` pra garantir visibilidade cross-thread do flag, sem mudar o fluxo de start/stop. |
| 2 | Filtro MainPlayer/Fika em todo patch que reage a ação de player — AP-02 | N/A | Nenhum patch Harmony; `RemoteSpeaker`/`VoipProcessor`/`AudioFilter` já guardam `Singleton<GameWorld>.Instantiated`/`MainPlayer != null` nos pontos tocados (sem mudança nesse guard). |
| 3 | Alvos ofuscados/virtuais resolvidos por assinatura; overrides auditados — AP-03 | N/A | Nenhum patch Harmony/override de método virtual do EFT. |
| 4 | Mudança de estado via API canônica do EFT; side-effects mapeados — AP-04 | ✅ | Nenhuma API do EFT nova é chamada; `Camera.main`/`Physics.Linecast` já eram usados, só mudou de qual thread são chamados. |
| 5 | Estado entre raids: raid1→exit→raid2 e alt-F4/morte/MIA cobertos | ✅ | `_cachedListenerPos`/`_cachedListenerRight` são campos de instância de `RemoteSpeaker`, recriado por raid (um `RemoteSpeaker` por jogador remoto, por raid) — não há herança de valor entre raids. `NoiseGateConfig` é recriado no primeiro `Update()` de cada raid. |
| 6 | Semântica/defaults/faixas de cada ConfigEntry sem ambiguidade — AP-05 | N/A | Nenhum `ConfigEntry` novo; os já existentes (`NoiseGateThreshold`, `LPFCutoff`, etc.) só passam a alimentar o snapshot em vez de propriedades soltas, sem mudar faixa/default. |
| 7 | Re-invocação de método patcheado tem reentry-guard — AP-07 | N/A | Nenhum patch Harmony. |
| 8 | Flags/caches de intercept validados contra o contexto atual após troca — AP-08 | ✅ | `_occlusionLayerMask`/`_botOcclusionMask` continuam com o cache lazy (`if (_occlusionLayerMask == -1)`) intacto — só a fórmula de composição muda, não a estratégia de cache. |
| 9 | Todo patch-point reconfirmado no `.cs` do dump — AP-09 | ✅ | `LayerMaskClass.cs:58,60,82,102-103,111,125,131-132,135` lidos diretamente nesta sessão (seção 1 e stubs). |
| 10 | Skill EFT usada como lever confirmada não-inerte — AP-10 | N/A | Não se aplica. |
| 11 | Pacote FIKA próprio: envelope + `TryGet*` + ... — AP-11 | N/A | Este item não introduz nenhum pacote de rede novo. |

## Histórico

| Data | Evento |
|---|---|
| 2026-09-09 | Spec técnica criada via `/create-technical-spec` |
| 2026-09-09 | Review 01 aplicada — bloqueador resolvido (stub de `Apply()` corrigido pra incluir `ApplyAGC`/`ApplyLimiter`), nota sobre código morto em `RNNoiseVADThreshold`/`RNNoiseGateHold` adicionada |
