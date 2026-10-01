# 015 — Ambiente Acústico: Reverb e Curva de Distância · Spec Técnica

**Mod:** TRL-SpeakFromTarkov
**Spec funcional:** [015-ambiente-acustico-reverb-distancia-01-spec.md](015-ambiente-acustico-reverb-distancia-01-spec.md)
**Criado:** 2026-09-09

> Fonte primária de verdade: [references/eft-decompiled/Assembly-CSharp/](../../../../references/eft-decompiled/Assembly-CSharp/).

## 0. Resposta à investigação (a) — reverb nativo é viável, com evidência forte

Confirmado em `references/eft-decompiled/Assembly-CSharp/BetterAudio.cs`: o jogo já tem um `AudioMixer` (`Master`, campo público `BetterAudio.cs:495`) com grupos dedicados à voz, incluindo:

- [`BetterAudio.cs:543`](../../../../references/eft-decompiled/Assembly-CSharp/BetterAudio.cs#L543) — `public AudioMixerGroup ObservedPlayerSpeechMixer;`, resolvido em [`BetterAudio.cs:1003`](../../../../references/eft-decompiled/Assembly-CSharp/BetterAudio.cs#L1003) como `FindMixerGroup("ObservedPlayer/ObservedPlayerSpeech")` — o grupo de mixer **nativo** que o VOIP original do jogo (Dissonance/FikaVOIP) usava para a voz de OUTROS jogadores.
- [`BetterAudio.cs:521`](../../../../references/eft-decompiled/Assembly-CSharp/BetterAudio.cs#L521) — `public AudioMixerGroup VoipMixer;`, resolvido em [`:993`](../../../../references/eft-decompiled/Assembly-CSharp/BetterAudio.cs#L993) como `FindMixerGroup("Voip")`.
- [`BetterAudio.cs:497`](../../../../references/eft-decompiled/Assembly-CSharp/BetterAudio.cs#L497) — `public AudioMixerSnapshot[] Snapshots;`, um snapshot por `EnvironmentType` (indoor/outdoor, no mínimo — `:1233`), trocado via `TransitionTo` conforme o jogador entra/sai de áreas fechadas.
- `BetterAudio : MonoBehaviourSingleton<BetterAudio>` ([`:24`](../../../../references/eft-decompiled/Assembly-CSharp/BetterAudio.cs#L24)) — expõe `public static T Instance => Singleton<T>.Instance;`/`Instantiated` diretamente (`MonoBehaviourSingleton-1.cs:6-8`), então `BetterAudio.Instance`/`BetterAudio.Instantiated` já bastam, sem precisar do `Comfort.Common.Singleton<>` explícito.

**Conclusão:** o jogo já teria aplicado reverb ambiental na voz automaticamente — SE a voz passasse pelo `AudioMixer` do jogo. Confirmei em `Audio/RemoteSpeaker.cs:105` que o `AudioSource` do mod é criado via `gameObject.AddComponent<AudioSource>()` e **nunca** tem `outputAudioMixerGroup` atribuído — a voz sai direto pro `AudioListener`, sem passar por nenhum grupo/snapshot do jogo. **Essa é a causa raiz mais provável da ausência de reverb**, e a correção é de baixíssimo risco: atribuir `ObservedPlayerSpeechMixer` (ou `VoipMixer`) como saída do `AudioSource`. Zero DSP customizado necessário — reaproveita 100% a infraestrutura de áudio nativa do jogo.

**Nota (Review 01, PA-01-01):** `RemoteSpeaker.cs:115-117` já seta `bypassEffects`/`bypassListenerEffects`/`bypassReverbZones = true` no `AudioSource`. Esses 3 flags bypassam sistemas de áudio **diferentes** (efeitos por componente na própria GameObject, efeitos do `AudioListener`, e `AudioReverbZone` clássico do Unity, respectivamente) e **não afetam** o roteamento por `outputAudioMixerGroup`, que é um caminho de sinal independente. Essas 3 linhas permanecem inalteradas — a correção deste item não as remove nem depende de removê-las (aliás, é justamente por já bypassar o `AudioReverbZone` clássico que a alternativa descartada na seção 1 nunca funcionaria mesmo se implementada).

## 1. Estratégia

1. **Reverb (item a):** `RemoteSpeaker.Initialize()` passa a setar `audioSource.outputAudioMixerGroup = BetterAudio.Instance.ObservedPlayerSpeechMixer;` (com fallback defensivo se `BetterAudio` não estiver instanciado, ex.: menu/lobby fora de raid).
2. **Curva de distância (item b):** não há constante "correta" a inventar — a spec funcional já pede investigação, não uma fórmula pronta. A estratégia técnica é instrumentar temporariamente (log throttled, removível) os valores reais de `distanceAttenuation`/`airDampingAlpha` computados em `RemoteSpeaker.cs:403,406` durante uma sessão de raid real, comparar com a percepção reportada (200% necessário a ~10-20m), e então ajustar o expoente `1.2f` (linha 403) e/ou o piso `0.60f` do `airDampingAlpha` (linha 406) com base no dado medido — não no palpite.

**Alternativa descartada para o reverb:** implementar reverb via `UnityEngine.Audio.AudioReverbFilter`/`AudioReverbZone` (API genérica do Unity, não a infraestrutura própria do jogo). Descartada porque o jogo não usa esse caminho — usa `AudioMixer`/`AudioMixerSnapshot` (confirmado acima) — e reaproveitar o que já existe é estritamente mais simples e mais consistente com o resto do áudio do jogo do que introduzir um sistema paralelo.

## 2. Pontos de patch

N/A — nenhum patch Harmony. Leitura de campo público (`BetterAudio.ObservedPlayerSpeechMixer`) e atribuição de propriedade padrão do Unity (`AudioSource.outputAudioMixerGroup`), sem interceptar nenhum método do EFT.

## 3. Novas propriedades F12 (BepInEx)

| Seção | Nome (EN) | Tipo | Padrão | Faixa | Avançado | Tooltip (pt-BR) |
|---|---|---|---|---|---|---|
| `Network & 3D Audio` | `Enable Native Reverb` | bool | `true` | — | — | Roteia a voz dos outros jogadores pelo mixer de áudio nativo do jogo, aplicando o reverb ambiental de cada sala/corredor automaticamente. Desative se notar algum problema de volume ao ligar. |

> Config nova pra permitir desligar rápido se o roteamento pelo mixer nativo causar algum efeito colateral inesperado de volume/ducking em algum mapa específico — descoberto só em teste real, não em leitura de código.

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `Audio/RemoteSpeaker.cs` | MODIFICAR | `Initialize()` roteia `audioSource.outputAudioMixerGroup` pro grupo nativo de voz; instrumentação temporária de log em `OnAudioFilterRead` (removível após a calibração). |
| `VOIPPlugin.cs` | MODIFICAR | Novo `ConfigEntry<bool> EnableNativeReverb`. |

## 5. Stubs de código

```csharp
// Audio/RemoteSpeaker.cs — roteamento pro mixer nativo (Initialize())

void Initialize(/* assinatura existente */)
{
    // ... corpo existente até a criação do AudioSource (linha 105 atual) ...
    audioSource = gameObject.AddComponent<AudioSource>();

    // NOVO — reverb ambiental nativo via infraestrutura já existente do jogo.
    // ref: Assembly-CSharp/BetterAudio.cs:24,543 (MonoBehaviourSingleton, ObservedPlayerSpeechMixer)
    bool enableNativeReverb = VoIPPlugin.EnableNativeReverb == null || VoIPPlugin.EnableNativeReverb.Value;
    if (enableNativeReverb && BetterAudio.Instantiated)
    {
        audioSource.outputAudioMixerGroup = BetterAudio.Instance.ObservedPlayerSpeechMixer;
    }

    // ... resto do método sem mudança ...
}
```

```csharp
// Audio/RemoteSpeaker.cs — instrumentação temporária pra calibração da curva de distância
// (marcada claramente como removível — não é código de produção definitivo)

// PERF-INSTR AUD-015: remover após calibrar a curva de distância com dados reais de raid.
private static float _lastCurveLogTime = 0f;
// ... dentro de OnAudioFilterRead, após calcular distanceAttenuation/airDampingAlpha (linha ~407 atual) ...
if (VoIPPlugin.EnableDebugLogs != null && VoIPPlugin.EnableDebugLogs.Value && Time.unscaledTime - _lastCurveLogTime > 2f)
{
    _lastCurveLogTime = Time.unscaledTime;
    VoIPPlugin.Log.LogInfo($"[SFT-CURVE] dist={Vector3.Distance(listenerPos, transform.position):F1}m attn={distanceAttenuation:F2} airDamp={airDampingAlpha:F2}");
}
```

```csharp
// VOIPPlugin.cs — novo ConfigEntry (mesma seção "Network & 3D Audio" de MaxHearingDistance/OpusBitrate)
public static ConfigEntry<bool> EnableNativeReverb { get; private set; } = null!;
// ... no Awake()/Init de config, junto dos outros da mesma seção ...
EnableNativeReverb = Config.Bind("Network & 3D Audio", "Enable Native Reverb", true,
    new ConfigDescription("Roteia a voz dos outros jogadores pelo mixer de áudio nativo do jogo, aplicando o reverb ambiental de cada sala/corredor automaticamente. Desative se notar algum problema de volume ao ligar."));
```

## 6. Fluxo de dados

```
[RemoteSpeaker.Initialize()] → BetterAudio.Instance.ObservedPlayerSpeechMixer
  → audioSource.outputAudioMixerGroup (1x, na criação do speaker)

[Jogador remoto entra numa sala fechada] → o próprio jogo já transiciona BetterAudio.Snapshots
  (Assembly-CSharp/BetterAudio.cs:1228,1233) baseado no EnvironmentType do LISTENER (jogador local)
  → o reverb do snapshot ativo é aplicado a QUALQUER AudioSource roteado pelo ObservedPlayerSpeechMixer,
    incluindo a voz do RemoteSpeaker — sem o mod precisar calcular nada, é o mixer do jogo fazendo o trabalho.

[Calibração da curva de distância — processo manual, não automatizado]
  OnAudioFilterRead loga dist/attn/airDamp (throttled) → usuário testa em raid real a ~10-20m
  → compara com a percepção reportada → ajusta o expoente 1.2f / piso 0.60f em RemoteSpeaker.cs
  → repete até o volume de 100% no mixer ser suficiente pras distâncias de uso comum
```

## 7. Riscos e dependências

- **Ordem de execução:** implementar depois do item `011-threading-oclusao-voz/` (correção do bitmask de oclusão) — calibrar a curva de distância antes da oclusão estar correta arriscaria compensar um bug que está de saída.
- **Roteamento pelo mixer nativo pode interagir com o `_volumeGain`/mixer de volume por jogador (`PlayerVolumeMixerHUD`) de forma não testada** — o `ObservedPlayerSpeechMixer` pode ter seu próprio estágio de ganho/ducking dentro do `AudioMixer` do jogo, que somaria com o ganho que o mod já aplica manualmente em `OnAudioFilterRead` (`_volumeGain`, linha 419 atual). É exatamente por isso que este item introduz o toggle `EnableNativeReverb` — permite desligar rápido se a combinação dos dois ganhos ficar errada, sem precisar reverter código.
- **A calibração da curva (item b) não tem critério de sucesso automatizável** — depende de teste perceptual do usuário em raid real. O checklist de implementação reflete isso como etapa manual, não como assert de código.
- **Nenhum patch Harmony envolvido.**

## 8. Checklist de implementação

- [x] `VOIPPlugin.cs`: adicionar `EnableNativeReverb` (tooltip em inglês, seguindo a convenção já usada por todas as outras entradas da seção "Network & 3D Audio").
- [x] `RemoteSpeaker.cs`: rotear `outputAudioMixerGroup` pro `ObservedPlayerSpeechMixer` quando o toggle estiver ativo e `BetterAudio` estiver instanciado. **Achado durante a implementação:** `bypassReverbZones=true` já existia por outro motivo (bypassar o `AudioReverbZone` clássico) — comentário adicionado esclarecendo que não conflita com o roteamento novo.
- [x] Instrumentação temporária de log adicionada (`PERF-INSTR AUD-015`). **Correção durante a implementação:** o stub original da spec técnica usava `Time.unscaledTime` pro throttle — corrigido pra `Environment.TickCount`, porque `OnAudioFilterRead` roda na audio thread (`Time.*` não é seguro fora da main thread, mesmo motivo do padrão já usado em `SftNetwork.LogErrorThrottled`).
- [ ] Testar em pelo menos 2 ambientes bem diferentes do jogo (ex.: área aberta da Woods vs. corredor/bunker da Reserve) — confirmar que o reverb muda perceptivelmente entre os dois e que não introduz distorção/clipping. *(Requer teste em jogo.)*
- [ ] Testar com `EnableNativeReverb = false` — confirmar que o comportamento volta a ser idêntico ao de hoje (regressão zero quando desligado). *(Requer teste em jogo.)*
- [ ] Coletar dados reais de uma sessão de raid com squad a distâncias de 10-30m (via a instrumentação já adicionada), ajustar o expoente/piso de `RemoteSpeaker.cs` com base nesse dado. *(Requer teste em jogo.)*
- [ ] Remover a instrumentação temporária depois da calibração (ou mantê-la só sob `EnableDebugLogs`, já gated).
- [ ] Validar com o usuário que o volume de amigos no mixer pode voltar a 100% nas distâncias de uso comum. *(Requer teste em jogo.)*
- [ ] Compilar via `dotnet build` com 0 erros/0 avisos. *(Requer `/compile-mod`.)*

## 9. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid: start hook + stop hooks idempotentes — AP-01 | ✅ | `outputAudioMixerGroup` é atribuído 1x em `Initialize()`, mesmo ciclo de vida do `RemoteSpeaker` já existente — nenhum recurso novo a liberar (o `AudioMixerGroup` é um asset do jogo, não algo instanciado pelo mod). |
| 2 | Filtro MainPlayer/Fika em todo patch que reage a ação de player — AP-02 | ✅ | Guard `BetterAudio.Instantiated` antes de acessar `.Instance`. |
| 3 | Alvos ofuscados/virtuais resolvidos por assinatura; overrides auditados — AP-03 | N/A | Nenhum patch Harmony/override de método virtual do EFT — só leitura de campo público e propriedade padrão do `AudioSource`. |
| 4 | Mudança de estado via API canônica do EFT; side-effects mapeados — AP-04 | ✅ | `BetterAudio.ObservedPlayerSpeechMixer`/`AudioSource.outputAudioMixerGroup` são APIs públicas/nativas; side-effect mapeado na seção 7 (possível ganho duplicado, mitigado pelo toggle). |
| 5 | Estado entre raids: raid1→exit→raid2 e alt-F4/morte/MIA cobertos | ✅ | `outputAudioMixerGroup` é reatribuído a cada `Initialize()` (1 `RemoteSpeaker` novo por raid, por jogador remoto) — sem estado herdado. |
| 6 | Semântica/defaults/faixas de cada ConfigEntry sem ambiguidade — AP-05 | ✅ | `EnableNativeReverb` (bool, default `true`) documentado na seção 3, com tooltip explicando quando desligar. |
| 7 | Re-invocação de método patcheado tem reentry-guard — AP-07 | N/A | Nenhum patch Harmony. |
| 8 | Flags/caches de intercept validados contra o contexto atual — AP-08 | N/A | Nenhum cache de intercept neste item. |
| 9 | Todo patch-point reconfirmado no `.cs` do dump — AP-09 | ✅ | `BetterAudio.cs:24,495,497,521,543,982-1009,1228,1233` e `RemoteSpeaker.cs:105,403,406` lidos diretamente nesta sessão. |
| 10 | Skill EFT usada como lever confirmada não-inerte — AP-10 | ✅ | `BetterAudio.Snapshots`/`ObservedPlayerSpeechMixer` confirmados populados em `BetterAudio.cs:982-1009` (não é uma lista vazia/inerte) — a skill (mixer nativo) está ativa no jogo. |
| 11 | Pacote FIKA próprio: envelope + `TryGet*` + ... — AP-11 | N/A | Nenhum pacote de rede neste item. |

## Histórico

| Data | Evento |
|---|---|
| 2026-09-09 | Spec técnica criada via `/create-technical-spec` |
| 2026-09-09 | Review 01 aplicada — nota sobre `bypassReverbZones` (sistema diferente do proposto) adicionada; simplificado `Singleton<BetterAudio>` para `BetterAudio.Instance` direto |
