---
title: "Relatório de Auditoria Técnica de Código — TRL-SpeakFromTarkov (Review 03)"
date: 2026-09-09
status: 🟢 Vivo
authors: Claude
---

# Relatório de Auditoria Técnica de Código — TRL-SpeakFromTarkov (Review 03)

Terceira rodada de auditoria técnica estática profunda e minuciosa, realizada em `mods/TRL-SpeakFromTarkov/modded-V4/` — fork novo criado a partir de `modded-V3-audit/` (v1.5.4) especificamente para este ciclo de auditoria, sem tocar na V3 (que permanece intacta como versão estável de referência). Cobriu as 6 dimensões críticas nos 17 arquivos C# do mod (~5.850 linhas), com validação cruzada contra `references/eft-decompiled/`, `references/fika-plugin/Fika.Core/` e os relatórios anteriores (`relatorio-auditoria-codigo-01.md`, `-02.md`) para evitar duplicar achados já ✅ Aplicado.

**Atenção especial:** `VOIPPlugin.cs` e `TRL-SpeakFromTarkov.csproj` foram modificados em commit não documentado em `memory/sessions.md` (bump de versão 1.5.3→1.5.4 + refator de `Audio/BotVoiceBridge.cs`). Essa mudança não auditada continha o achado mais grave desta rodada (AUD-03-01).

---

## 1. Resumo Executivo da Auditoria

| Severidade | Quantidade | Descrição |
|---|---|---|
| 🔴 **Crítico** | 1 | IA de bots reage à voz do jogador como se fosse tiro real (pânico/cobertura) |
| 🟠 **Alto** | 3 | Acesso a API Unity fora da main thread (audio thread), oclusão física por porta/parede quebrada, race condition sem sincronização no pipeline de voz |
| 🟡 **Médio** | 8 | GC pressure em 3 telas, I/O síncrono em disco no drag do slider, crescimento sem limite de dados persistidos, vazamento lento de `AudioClip`, chamada de IA sem efeito, timing incorreto de patch Harmony em método async |
| 🔵 **Baixo** | 6 | Campo morto com comentário enganoso, gap de lifecycle entre raids, catches silenciosos sem log, patches mortos, alocação mínima por frame, metadados de assembly desatualizados |
| 💡 **Otimização** | 1 | Finalizer de patch Harmony suprime toda exceção de um método usado por toda a UI de drag-and-drop |

---

## 2. Tabela de Achados

| ID | Severidade | Arquivo / Linha | Categoria | Descrição Resumida |
|---|---|---|---|---|
| `AUD-03-01` | 🔴 Crítico | [`Audio/BotVoiceBridge.cs:108`](../modded-V4/Audio/BotVoiceBridge.cs#L108) | Referência Cruzada (Dim. 1) | `AISoundType.gun` no lugar de `.step` faz bots entrarem em pânico de combate ao ouvir a voz do jogador |
| `AUD-03-02` | 🟠 Alto | [`Audio/RemoteSpeaker.cs:373-379,432-439`](../modded-V4/Audio/RemoteSpeaker.cs#L373-L439) | Threading (Dim. 6) | `Camera.main`/`Singleton<GameWorld>` acessados dentro de `OnAudioFilterRead` (audio thread do Unity, não a main thread) — confirmado por 2 auditorias independentes |
| `AUD-03-03` | 🟠 Alto | [`Audio/RemoteSpeaker.cs:282`](../modded-V4/Audio/RemoteSpeaker.cs#L282), [`Audio/BotVoiceBridge.cs:159`](../modded-V4/Audio/BotVoiceBridge.cs#L159) | Referência Cruzada (Dim. 1) | Composição de `LayerMask` sem shift de bits quebra silenciosamente a oclusão de voz por portas/objetos interativos |
| `AUD-03-04` | 🟠 Alto | [`Audio/VoipProcessor.cs`](../modded-V4/Audio/VoipProcessor.cs) (propriedades públicas) | Threading (Dim. 6) | Estado do pipeline de voz (modo, mute, PTT, níveis) lido/escrito por 2 threads sem `volatile`/lock |
| `AUD-03-05` | 🟡 Médio | [`Audio/AudioFilter.cs`](../modded-V4/Audio/AudioFilter.cs) (config runtime) | Threading (Dim. 6) | Config do filtro de ruído atualizada na main thread e lida na thread de captura sem sincronização |
| `AUD-03-06` | 🟡 Médio | [`UI/PlayerVolumeMixerHUD.cs:275-431`](../modded-V4/UI/PlayerVolumeMixerHUD.cs#L275-L431), [`UI/VoiceCalibrationHUD.cs:199-424`](../modded-V4/UI/VoiceCalibrationHUD.cs#L199-L424), [`UI/MenuVoipHUD.cs:615,650`](../modded-V4/UI/MenuVoipHUD.cs#L615) | GC Pressure (Dim. 2/3) | `GUIStyle`/`List` alocados a cada `OnGUI()` em 3 telas não cobertas pelo fix AUD-02-01 — confirmado por 2 auditorias independentes |
| `AUD-03-07` | 🟡 Médio | [`UI/PlayerVolumeMixerHUD.cs:414-417`](../modded-V4/UI/PlayerVolumeMixerHUD.cs#L414-L417) | GC/IO (Dim. 3) | `File.WriteAllText` síncrono na main thread a cada movimento do slider de volume |
| `AUD-03-08` | 🟡 Médio | [`UI/PlayerVolumeMixerHUD.cs:35-37,171-200`](../modded-V4/UI/PlayerVolumeMixerHUD.cs#L35-L200) | Memory Leak (Dim. 3) | Dicionários estáticos de volume por `profileId` crescem para sempre, sem TTL/eviction |
| `AUD-03-09` | 🟡 Médio | [`Audio/RemoteSpeaker.cs:128-133,504-507`](../modded-V4/Audio/RemoteSpeaker.cs#L128-L507) | Memory Leak (Dim. 3) | `AudioClip` runtime criado por `RemoteSpeaker` nunca é `Destroy()`ado |
| `AUD-03-10` | 🟡 Médio | [`Audio/BotVoiceBridge.cs:101`](../modded-V4/Audio/BotVoiceBridge.cs#L101) | Referência Cruzada (Dim. 1) | `SayPhrase()` com triggers que `BotReceiver` nativo ignora — no-op para IA vanilla |
| `AUD-03-11` | 🟡 Médio | [`GameSessionPatcher.cs:22-38`](../modded-V4/GameSessionPatcher.cs#L22-L38) | Referência Cruzada (Dim. 1) | Postfix Harmony em `EFT.Player.Init` (método `async Task`) dispara antes da inicialização lógica completar |
| `AUD-03-12` | 🔵 Baixo | [`Audio/RemoteSpeaker.cs:41`](../modded-V4/Audio/RemoteSpeaker.cs#L41) | Código Morto (Dim. 4) | Campo `packetQueue` declarado e nunca usado; comentário adjacente descreve um modelo de threading que não existe |
| `AUD-03-13` | 🔵 Baixo | [`UI/InRaidVoipHUD.cs:171-175`](../modded-V4/UI/InRaidVoipHUD.cs#L171-L175) | Antipadrões (Dim. 5 / AP-01) | `_originalPosCaptured` não é resetado entre raids |
| `AUD-03-14` | 🔵 Baixo | [`Network/SftNetwork.cs:338`](../modded-V4/Network/SftNetwork.cs#L338), [`UI/MenuVoipHUD.cs:192`](../modded-V4/UI/MenuVoipHUD.cs#L192) | Código Morto (Dim. 4) | `catch { }` sem log em requisições HTTP — falhas de rede ficam invisíveis |
| `AUD-03-15` | 🔵 Baixo | [`GameSessionPatcher.cs:76-104`](../modded-V4/GameSessionPatcher.cs#L76-L104) | Código Morto (Dim. 4) | `FikaVoipSendPatch`/`FikaVoipReceivePatch` nunca são exercitados (alvo nunca instanciado quando `EnableMod=true`) |
| `AUD-03-16` | 🔵 Baixo | [`UI/InRaidVoipHUD.cs:273`](../modded-V4/UI/InRaidVoipHUD.cs#L273) | GC Pressure (Dim. 3) | `new Vector3[4]` alocado a cada `OnGUI` Repaint |
| `AUD-03-17` | 🔵 Baixo | [`Properties/AssemblyInfo.cs:8,12`](../modded-V4/Properties/AssemblyInfo.cs#L8) | Manutenibilidade | Metadados do assembly com nome de projeto antigo (`VoipUnlimited`) e versão travada em `1.0.0.0` |
| `AUD-03-18` | 💡 Otimização | [`GameSessionPatcher.cs:207-225`](../modded-V4/GameSessionPatcher.cs#L207-L225) | Antipadrões (Dim. 5) | `[PatchFinalizer]` de `BoundSlotView.RefreshSelectView` suprime incondicionalmente qualquer exceção de um método usado por toda a UI de drag-and-drop do jogo |

---

## 3. Detalhamento dos Achados

### AUD-03-01 · IA de bots reage à voz do jogador como tiro real
- **Severidade:** 🔴 Crítico
- **Localização no Mod:** [`Audio/BotVoiceBridge.cs:108`](../modded-V4/Audio/BotVoiceBridge.cs#L108)
- **Referência Cruzada:** [`references/eft-decompiled/Assembly-CSharp/BotHearingSensor.cs:90-117,150-174`](../../../references/eft-decompiled/Assembly-CSharp/BotHearingSensor.cs)
- **Causa Raiz:** Um commit não documentado (início de setembro, sem entrada em `memory/sessions.md`) trocou `AISoundType.step` por `AISoundType.gun` na chamada `Singleton<BotEventHandler>.Instance.PlaySound(player, soundPos, power, AISoundType.gun)`. Para `type == AISoundType.gun`, `BotHearingSensor` **pula** a checagem probabilística de distância normal, aplica `Memory.Spotted(byHit:false)` se dentro do raio de detecção, e — se dentro de `BULLET_FEEL_DIST` — chama `Memory.SetPanicPoint(...)`, `Memory.SetUnderFire(enemy)` e pode disparar `BotTalk.Say(EPhraseTrigger.SniperPhrase)`.
- **Impacto Técnico Real:** Qualquer fala do jogador perto de bots hostis (inclusive um sussurro via `OnMutter`, raio 3-10m) os faz reagir como se tivessem sido baleados — pânico, procura de cobertura, "under fire" — sem nenhum combate real ocorrendo. Reproduzível sempre, não intermitente. `EnableBotInteraction` é `true` por padrão, então todo jogador com o recurso ativo é afetado.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - *Abordagem Atual:* `AISoundType.gun` (aciona ramo de pânico de combate)
  - *Abordagem Otimizada:* `AISoundType.step` (o valor original, antes do commit não auditado)
  - *Código Refatorado:*
```csharp
Singleton<BotEventHandler>.Instance.PlaySound(player, soundPos, power, AISoundType.step);
```
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-03-02 · Acesso a API Unity fora da main thread dentro de `OnAudioFilterRead`
- **Severidade:** 🟠 Alto
- **Evidência:** Forte — confirmada por 2 leituras independentes do código-fonte (não são a mesma passada duplicada), ambas convergindo para a mesma causa raiz.
- **Localização no Mod:** [`Audio/RemoteSpeaker.cs:373-379,432-439`](../modded-V4/Audio/RemoteSpeaker.cs#L373-L439)
- **Causa Raiz:** `OnAudioFilterRead()` é documentadamente executado pela Unity na **audio thread**, distinta da main thread e da thread de captura de microfone do mod. Dentro dela, o código chama `Camera.main` (que pode disparar `GameObject.FindGameObjectWithTag` internamente) e `Singleton<GameWorld>.Instance.MainPlayer`/`transform.position` — APIs que a Unity só garante seguras na main thread. O mesmo padrão em `Update()` (linha ~292, main thread) está correto; o problema é exclusivo da cópia dentro do callback de áudio.
- **Impacto Técnico Real:** Pode lançar `UnityException` ou corromper o cache interno da tag `MainCamera`, principalmente em trocas de câmera (virar espectador, morrer, trocar de corpo). É a explicação mais provável para crashes "aleatórios" sem padrão claro relatados por jogadores, já que dispara múltiplas vezes por segundo, por `RemoteSpeaker` ativo, durante toda a raid.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - *Abordagem Atual:* `Camera.main`/`Singleton<GameWorld>` resolvidos dentro do callback de áudio.
  - *Abordagem Otimizada:* Cachear `listenerPos`/`listenerRight` uma vez por frame em `Update()` (main thread) em campos simples; `OnAudioFilterRead` apenas lê esses campos já resolvidos.
  - *Código Refatorado:*
```csharp
// Em Update() (main thread) — uma vez por frame:
private Vector3 _cachedListenerPos;
private Vector3 _cachedListenerRight;

void Update()
{
    var cam = Camera.main;
    if (cam != null)
    {
        _cachedListenerPos = cam.transform.position;
        _cachedListenerRight = cam.transform.right;
    }
    else if (Singleton<GameWorld>.Instantiated && Singleton<GameWorld>.Instance.MainPlayer != null)
    {
        var mp = Singleton<GameWorld>.Instance.MainPlayer;
        _cachedListenerPos = mp.Transform.position;
        _cachedListenerRight = mp.Transform.right;
    }
    // ... resto do Update
}

// Em OnAudioFilterRead() (audio thread) — só leitura, zero chamada de API Unity:
// usar _cachedListenerPos / _cachedListenerRight diretamente
```
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-03-03 · Oclusão de voz por porta/parede quebrada (bitmask sem shift)
- **Severidade:** 🟠 Alto
- **Localização no Mod:** [`Audio/RemoteSpeaker.cs:282`](../modded-V4/Audio/RemoteSpeaker.cs#L282), [`Audio/BotVoiceBridge.cs:159`](../modded-V4/Audio/BotVoiceBridge.cs#L159)
- **Referência Cruzada:** [`references/eft-decompiled/Assembly-CSharp/LayerMaskClass.cs:82,125,131-132`](../../../references/eft-decompiled/Assembly-CSharp/LayerMaskClass.cs)
- **Causa Raiz:** `_occlusionLayerMask = LayerMaskClass.HighPolyWithTerrainMask | LayerMaskClass.DoorLayer | LayerMaskClass.InteractiveLayer;` — `DoorLayer` é um **índice de layer bruto** (0-31), não uma bitmask; o código nativo do próprio EFT só usa a versão corretamente deslocada (`1 << DoorLayer`, ou `InteractiveMask` já deslocado) ao montar máscaras. ORar o índice cru corrompe a máscara resultante.
- **Impacto Técnico Real:** O `Physics.Linecast` de oclusão física não testa contra portas/objetos interativos como pretendido — testa bits arbitrários que podem coincidir com layers não relacionadas. A feature de abafamento de voz atrás de portas fechadas, documentada como funcionando desde a V2, na prática não funciona como descrito, sem gerar nenhum erro visível.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - *Abordagem Atual:* `HighPolyWithTerrainMask | DoorLayer | InteractiveLayer`
  - *Abordagem Otimizada:* `HighPolyWithTerrainMask | (1 << DoorLayer) | InteractiveMask`
  - *Código Refatorado:*
```csharp
_occlusionLayerMask = LayerMaskClass.HighPolyWithTerrainMask
    | (1 << LayerMaskClass.DoorLayer)
    | LayerMaskClass.InteractiveMask;
```
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-03-04 · Race condition: estado do `VoipProcessor` sem sincronização entre threads
- **Severidade:** 🟠 Alto
- **Localização no Mod:** [`Audio/VoipProcessor.cs`](../modded-V4/Audio/VoipProcessor.cs) (propriedades `RawLevel`, `DisplayLevel`, `PeakLevel`, `IsTransmitting`, `LastOpusBytes`, `CurrentMode`, `IsMuted`, `IsPTTActive`)
- **Causa Raiz:** `VoipProcessor.ProcessAudio()` roda inteiro na **thread de captura** (disparado por `MicrophoneCapturer.ProcessFrame()`), escrevendo essas propriedades sem `volatile`/lock. Simultaneamente, a **main thread** escreve `CurrentMode`/`IsMuted`/`IsPTTActive` (via `VoipController.cs`, `MenuVoipHUD.cs`, `VOIPPlugin.cs`) e lê `RawLevel`/`DisplayLevel`/etc. para os HUDs (`VoipHUD.cs`, `InRaidVoipHUD.cs`, `VoiceCalibrationHUD.cs`) — um cruzamento bidirecional sem nenhuma barreira de memória.
- **Impacto Técnico Real:** Sem `volatile`/`Interlocked`, não há garantia de visibilidade entre threads pela especificação do C#. Na prática (x86/x64, JIT atual) o sintoma mais provável é o toggle de mute/PTT/modo "atrasar" alguns ciclos da thread de captura — não é um crash hoje, mas é comportamento indefinido concentrado exatamente na classe central do pipeline de voz.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - *Abordagem Atual:* Campos simples lidos/escritos livremente por 2 threads.
  - *Abordagem Otimizada:* Marcar os campos-backing como `volatile` (válido para `bool`/`int`/`float`) ou centralizar mute/PTT/modo num snapshot trocado via `Interlocked.Exchange`, no mesmo padrão já documentado em `SftNetwork.cs` para a fila de envio.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-03-05 · Race condition: config do `AudioFilter` sem sincronização
- **Severidade:** 🟡 Médio
- **Localização no Mod:** [`Audio/AudioFilter.cs`](../modded-V4/Audio/AudioFilter.cs) (`OpenThreshold`, `CloseThreshold`, `HoldTime`, `UseRNNoise`, `EnableAGC`, `EnableLimiter`, `LPFCutoffHz`/`_lpfAlpha`)
- **Causa Raiz:** `MicrophoneCapturer.Update()` (main thread) reescreve essas propriedades todo frame a partir de `ConfigEntry<T>.Value`; `AudioFilter.Apply()` só é chamado da thread de captura. Nenhuma é `volatile` — `OpenThreshold`/`CloseThreshold` podem ser observadas parcialmente atualizadas (uma nova, outra antiga) pela thread de captura.
- **Impacto Técnico Real:** Descompasso momentâneo no noise gate ao mudar configuração no F12 durante transmissão ativa. Baixo impacto perceptível, mesma família de problema do AUD-03-04.
- **Alternativa de Melhor Lógica / Proposta de Correção:** Mesma solução do AUD-03-04 — idealmente um snapshot imutável trocado por referência (`Interlocked.Exchange`), lido uma vez por frame pela thread de captura em vez de N campos soltos.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-03-06 · GC pressure em `OnGUI` não coberta pelo fix AUD-02-01
- **Severidade:** 🟡 Médio
- **Evidência:** Forte — confirmada por 2 auditorias independentes para `PlayerVolumeMixerHUD.cs` e `VoiceCalibrationHUD.cs`.
- **Localização no Mod:** [`UI/PlayerVolumeMixerHUD.cs:275-431`](../modded-V4/UI/PlayerVolumeMixerHUD.cs#L275-L431) (6 estilos no escopo da janela + 3 por jogador em `DrawPlayerRow`), [`UI/VoiceCalibrationHUD.cs:199-424`](../modded-V4/UI/VoiceCalibrationHUD.cs#L199-L424) (8 estilos), [`UI/MenuVoipHUD.cs:615,650`](../modded-V4/UI/MenuVoipHUD.cs#L615) (`.ToList()` em coleções a cada `OnGUI`)
- **Causa Raiz:** O antipadrão corrigido em `InRaidVoipHUD.cs`/`VoipHUD.cs` na Review 02 (AUD-02-01 — `new GUIStyle`/texturas a cada `OnGUI`) não foi estendido a estas 3 telas.
- **Impacto Técnico Real:** Micro-engasgos enquanto essas telas estão visíveis. `MenuVoipHUD` é o caso mais sério: ao contrário das outras duas (modais on-demand), fica ativo continuamente sempre que o jogador está no menu principal do FIKA — não é uma alocação ocasional.
- **Alternativa de Melhor Lógica / Proposta de Correção:** Mover a construção dos `GUIStyle` para campos cacheados em `Awake()`/`Initialize()` (mesmo padrão já aplicado em `InRaidVoipHUD`/`VoipHUD`); em `MenuVoipHUD`, iterar a coleção diretamente ou cachear a lista, invalidando só quando a composição do canal mudar.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-03-07 · I/O de disco síncrono na main thread durante o drag do slider de volume
- **Severidade:** 🟡 Médio
- **Localização no Mod:** [`UI/PlayerVolumeMixerHUD.cs:414-417`](../modded-V4/UI/PlayerVolumeMixerHUD.cs#L414-L417) (`SetPlayerVolume` → `SaveConfig()`)
- **Causa Raiz:** `DrawPlayerRow` chama `SetPlayerVolume` (que faz `File.WriteAllText` síncrono, serializando a lista inteira) toda vez que o slider muda mais de `0.01`. `OnGUI` roda 2-4x por frame — durante o arraste, isso dispara múltiplas escritas em disco síncronas por frame.
- **Impacto Técnico Real:** Hitch real num modal que pode ser aberto dentro de raid (atalho Alt+P).
- **Alternativa de Melhor Lógica / Proposta de Correção:** Debounce — só persistir ao soltar o slider (`GUI.changed` + `Event.current.type == EventType.MouseUp`) ou throttle a 1x/segundo.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-03-08 · Dicionários de volume por jogador crescem sem limite
- **Severidade:** 🟡 Médio
- **Localização no Mod:** [`UI/PlayerVolumeMixerHUD.cs:35-37,171-200`](../modded-V4/UI/PlayerVolumeMixerHUD.cs#L35-L200)
- **Causa Raiz:** `_playerVolumes`/`_mutedPlayers`/`_playerNicknames` são `static` e nunca depuradas — todo `profileId` já visto em qualquer squad fica gravado para sempre em `TRL-SpeakFromTarkov-PlayersVolume.json`, sem TTL, limite ou eviction.
- **Impacto Técnico Real:** Ao longo de meses de coop com squads variados, o arquivo e o dicionário em memória crescem indefinidamente. Não quebra nada hoje, mas é uma dívida técnica de crescimento sem teto.
- **Alternativa de Melhor Lógica / Proposta de Correção:** Guardar `LastSeen` por entrada e podar no load (ex.: descartar entradas com mais de 90 dias) ou aplicar um teto de N entradas com política LRU.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-03-09 · `AudioClip` runtime nunca destruído no `RemoteSpeaker`
- **Severidade:** 🟡 Médio
- **Localização no Mod:** [`Audio/RemoteSpeaker.cs:128-133,504-507`](../modded-V4/Audio/RemoteSpeaker.cs#L128-L507)
- **Causa Raiz:** `Initialize()` cria `AudioClip.Create("SftStream", ...)` e atribui a `audioSource.clip`. `OnDestroy()` só faz `decoder = null!` — o `AudioClip` runtime nunca recebe `Destroy()` explícito.
- **Impacto Técnico Real:** Destruir o `GameObject`/`AudioSource` não libera o asset `AudioClip` em si — fica órfão até o próximo `Resources.UnloadUnusedAssets()` (no headless, roda esporadicamente fora de raid). Cada raid recria um `RemoteSpeaker` por jogador remoto, então o lixo de clips (~192KB cada a 48kHz mono) acumula entre varreduras.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
```csharp
private AudioClip _streamClip;
// ... em Initialize(): _streamClip = audioSource.clip = AudioClip.Create(...);

void OnDestroy()
{
    decoder = null!;
    if (_streamClip != null)
    {
        Destroy(_streamClip);
        _streamClip = null;
    }
}
```
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-03-10 · `SayPhrase()` sem efeito na IA nativa (no-op)
- **Severidade:** 🟡 Médio
- **Localização no Mod:** [`Audio/BotVoiceBridge.cs:101`](../modded-V4/Audio/BotVoiceBridge.cs#L101)
- **Referência Cruzada:** [`references/eft-decompiled/Assembly-CSharp/BotReceiver.cs:71-94`](../../../references/eft-decompiled/Assembly-CSharp/BotReceiver.cs)
- **Causa Raiz:** `Singleton<BotEventHandler>.Instance.SayPhrase(player, trigger)` é chamado com `trigger ∈ {OnMutter, NoisePhrase, OnFight}`. O único assinante nativo de `OnPhraseSay` (`BotReceiver.method_0`) só trata `{Silence, Stop, FollowMe, NeedHelp, GetInCover, Spreadout}` — nenhum dos triggers usados pelo mod está nessa lista.
- **Impacto Técnico Real:** A chamada é 100% no-op para a IA vanilla, apesar do comentário do código sugerir que módulos nativos reagiriam. Se o alvo real é um mod de IA de terceiros (ex.: SAIN, não vendorizado neste repo), não há como confirmar aqui se ele trata esses triggers de forma diferente.
- **Alternativa de Melhor Lógica / Proposta de Correção:** Remover a chamada (sem efeito comprovado) ou documentar explicitamente como "best-effort para mods de terceiros", sem depender dela para comportamento de IA vanilla.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-03-11 · Timing incorreto de Postfix Harmony em método `async Task`
- **Severidade:** 🟡 Médio
- **Localização no Mod:** [`GameSessionPatcher.cs:22-38`](../modded-V4/GameSessionPatcher.cs#L22-L38) (`PlayerInitPatch`)
- **Referência Cruzada:** [`references/eft-decompiled/Assembly-CSharp/EFT/Player.cs:28592`](../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs) — `public virtual async Task Init(...)`
- **Causa Raiz:** Um Postfix Harmony num método `async Task` executa quando a parte **síncrona** do método retorna (no primeiro `await` suspenso), não quando toda a lógica de inicialização termina. `await JobScheduler.Yield()` suspende antes de `Profile = profile;` e do restante do corpo rodarem.
- **Impacto Técnico Real:** `VoipController.Instance.StartVoipCapture()`/`SetGameStateChannel(true)` disparam antes do player estar totalmente inicializado. Impacto hoje é baixo (o Postfix só lê `IsYourPlayer`, setado independentemente), mas é um padrão frágil para qualquer lógica futura que dependa de `Profile`/`HealthController` já resolvidos.
- **Alternativa de Melhor Lógica / Proposta de Correção:** Usar `[HarmonyPatch(..., MethodType.Async)]` (mira no `MoveNext` gerado pelo compilador) ou mover a lógica para um evento pós-init genuíno (ex.: `GameWorld.OnPersonAdd`).
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-03-12 · Campo morto `packetQueue` com comentário enganoso sobre threading
- **Severidade:** 🔵 Baixo
- **Localização no Mod:** [`Audio/RemoteSpeaker.cs:41`](../modded-V4/Audio/RemoteSpeaker.cs#L41) (declaração), comentário em `RemoteSpeaker.cs:169`
- **Causa Raiz:** `packetQueue` (`ConcurrentQueue<byte[]>`) é declarado mas nunca usado — `EnqueuePacket()` decodifica Opus diretamente e de forma síncrona no buffer circular (`streamBuffer`). O comentário adjacente ("Decodificação Off-Thread imediata no callback de recepção da rede") descreve um modelo que não corresponde à realidade: a recepção roda na **main thread** via `PollEvents()` do FikaClient/FikaServer (confirmado em `references/fika-plugin/Fika.Core/Networking/{FikaClient,FikaServer}.cs` — `NetManager.UnsyncedEvents` é `false` por padrão e nunca é ligado pelo FIKA).
- **Impacto Técnico Real:** Não é um bug ativo, mas é uma armadilha de manutenção: um refactor futuro que "aproveite" essa fila, ou confie no comentário para mover o recebimento pra uma thread de rede real, reintroduziria uma escrita cross-thread em `streamBuffer` concorrendo com a leitura em `OnAudioFilterRead()`.
- **Alternativa de Melhor Lógica / Proposta de Correção:** Remover o campo morto e corrigir o comentário para refletir a realidade (decode síncrono no callback do `NetPacketProcessor`, disparado na main thread).
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-03-13 · Gap de lifecycle entre raids no `InRaidVoipHUD`
- **Severidade:** 🔵 Baixo
- **Localização no Mod:** [`UI/InRaidVoipHUD.cs:171-175`](../modded-V4/UI/InRaidVoipHUD.cs#L171-L175) (campo `_originalPosCaptured`, declarado na linha 30)
- **Causa Raiz:** `_originalStanceAnchoredPos`/`_originalPosCaptured` são capturados uma única vez (primeira raid) e nunca resetados quando `_battleStancePanel` volta a `null` entre raids.
- **Impacto Técnico Real:** Se a posição-base do `BattleStancePanel` variar entre instanciações (resolução, HUD scale), o deslocamento X aplicado usaria a base da raid 1 em raids seguintes. Impacto visual provavelmente baixo (mesmo prefab, mesma posição default), mas é um gap de lifecycle real (adjacente ao AP-01).
- **Alternativa de Melhor Lógica / Proposta de Correção:** Resetar `_originalPosCaptured = false` no mesmo bloco que zera `_battleStancePanel`.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-03-14 · Catches silenciosos escondem falhas de rede HTTP
- **Severidade:** 🔵 Baixo
- **Localização no Mod:** [`Network/SftNetwork.cs:338`](../modded-V4/Network/SftNetwork.cs#L338) (POST `sft/channels/announce`), [`UI/MenuVoipHUD.cs:192`](../modded-V4/UI/MenuVoipHUD.cs#L192) (GET `sft/channels/list`)
- **Causa Raiz:** `catch { }` puro dentro de `Task.Run`, sem log. Diferente dos demais `catch { }` do mod (defensivos, em torno de acesso a hierarquia Unity/config — aceitáveis e não sinalizados aqui), estes dois escondem falhas de rede/servidor SPT sem qualquer rastro.
- **Impacto Técnico Real:** Se o endpoint HTTP do SPT cair, o sintoma vira "canais de menu simplesmente não funcionam" sem nenhuma pista no log para investigar.
- **Alternativa de Melhor Lógica / Proposta de Correção:** Trocar por `SftNetwork.LogErrorThrottled(...)`, já existente e usado em outros pontos do próprio arquivo.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-03-15 · Patches Harmony mortos (`FikaVoipSendPatch`/`FikaVoipReceivePatch`)
- **Severidade:** 🔵 Baixo
- **Localização no Mod:** [`GameSessionPatcher.cs:76-104`](../modded-V4/GameSessionPatcher.cs#L76-L104)
- **Causa Raiz:** Os alvos (`FikaVOIPClient.SendVoiceData`/`NetworkReceivedPacket`) existem, mas `FikaClientInitializeVoipPatch`/`FikaServerInitializeVoipPatch` (linhas 106-142) já bloqueiam `InitializeVOIP()` inteiro quando `EnableMod=true` — e é `InitializeVOIP()` que instancia `FikaVOIPClient`. Quando `EnableMod=true`, `FikaVOIPClient` nunca é criado, então esses 2 patches nunca são exercitados. Quando `EnableMod=false`, os patches se auto-desativam (retornam `true`).
- **Impacto Técnico Real:** Código morto em ambos os estados do mod.
- **Alternativa de Melhor Lógica / Proposta de Correção:** Remover os dois patches, ou documentar explicitamente como redundância defensiva intencional (caso a intenção seja blindagem contra mudança futura no FIKA).
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-03-16 · Alocação mínima por `OnGUI` em `InRaidVoipHUD`
- **Severidade:** 🔵 Baixo
- **Localização no Mod:** [`UI/InRaidVoipHUD.cs:273`](../modded-V4/UI/InRaidVoipHUD.cs#L273)
- **Causa Raiz:** `Vector3[] corners = new Vector3[4]` alocado a cada `OnGUI` Repaint enquanto o HUD in-raid está visível — o resto do arquivo já é zero-alloc (pós AUD-02-01).
- **Impacto Técnico Real:** Trivial, mas fecha o arquivo em 100% zero-alloc se corrigido.
- **Alternativa de Melhor Lógica / Proposta de Correção:** Mover para campo de instância reutilizado.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-03-17 · Metadados de assembly desatualizados
- **Severidade:** 🔵 Baixo
- **Localização no Mod:** [`Properties/AssemblyInfo.cs:8,12`](../modded-V4/Properties/AssemblyInfo.cs#L8)
- **Causa Raiz:** `AssemblyTitle("VoipUnlimited")`/`AssemblyProduct("VoipUnlimited")` — nome de projeto antigo, pré-rebrand para `TRL-SpeakFromTarkov`. `AssemblyVersion`/`AssemblyFileVersion` travados em `1.0.0.0`, dessincronizados da versão real (`1.5.4`).
- **Impacto Técnico Real:** Nenhum funcional — só inconsistência administrativa/identidade do mod (GUID e `BepInPlugin` já estão corretos em `VOIPPlugin.cs`).
- **Alternativa de Melhor Lógica / Proposta de Correção:** Atualizar `AssemblyTitle`/`AssemblyProduct` para `TRL-SpeakFromTarkov` e sincronizar `AssemblyVersion`/`AssemblyFileVersion` com a versão do plugin a cada bump.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-03-18 · Finalizer Harmony suprime toda exceção de método de UI genérico
- **Severidade:** 💡 Otimização
- **Localização no Mod:** [`GameSessionPatcher.cs:207-225`](../modded-V4/GameSessionPatcher.cs#L207-L225) (`BoundSlotViewRefreshSelectViewPatch`)
- **Causa Raiz:** `[PatchFinalizer]` retorna `null` incondicionalmente em ambos os ramos, suprimindo **qualquer** exceção de `BoundSlotView.RefreshSelectView` — método usado por toda a UI de drag-and-drop do jogo, não só no fluxo de spawn de arma do FIKA que motivou o patch originalmente.
- **Impacto Técnico Real:** Mascara potenciais bugs futuros não relacionados ao propósito original do patch, em qualquer tela de inventário/drag-and-drop.
- **Alternativa de Melhor Lógica / Proposta de Correção:** Restringir a supressão ao tipo de exceção específico que motivou o patch (ex.: `catch` tipado no corpo do finalizer, relançando qualquer outra exceção).
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

## 4. Verificações Sem Achado (confirmado correto)

- **Fila de envio de rede (`SftNetwork.cs`):** `Singleton<IFikaNetworkManager>.Instance.SendData(...)` só é chamado de `DrainSendQueue()` (exclusivamente a partir de `Update()`, main thread) e de `BroadcastChannelAnnouncement()` (sempre a partir de contextos main-thread do `MenuVoipHUD`). O único site em thread de captura, `SftNetwork.Broadcast()`, corretamente só enfileira e nunca toca `IFikaNetworkManager`/`_dataWriter`. Nenhum risco de corrupção de pacote.
- **Teardown entre raids:** `SftNetwork.remoteSpeakers` é corretamente limpo em `StopSession()` (`Destroy(gameObject)` por speaker + `.Clear()`). Sem leak de `Player`/`GameWorld` por raid inteira.
- **Validade de patches Harmony:** todos os alvos citados (`EFT.Player.Init`/`OnDead`, `GameWorld.Dispose`, métodos do `Fika.Core`, `BoundSlotView.RefreshSelectView`) foram confirmados linha a linha nos dumps decompilados/FIKA — nenhum patch mira em assinatura inexistente.
- **`AUD-01-06`/`AUD-02-01`/`AUD-02-02`/`AUD-02-03`:** reconfirmados presentes e corretos em `modded-V4` (unbind de `SceneManager.sceneLoaded`, zero-alloc `OnGUI` em `InRaidVoipHUD`/`VoipHUD`, guard in-raid no retry de microfone, busca condicional do `BattleStancePanel`).

**Pendências conhecidas reforçadas (não re-numeradas):**
- `AUD-01-07` — varredura de `Transform` em `MenuVoipHUD.IsFikaHUDVisible()` a cada `OnGUI`. Ainda deferida.
- `AUD-01-10` — `Task.Run` sem `CancellationToken` em `MenuVoipHUD.FetchServerChannels()` (`MenuVoipHUD.cs:151-194`). Ainda pendente, não decidida.

---

## 5. Plano de Ação e Recomendações

1. **Corrigir imediatamente (🔴):** `AUD-03-01` — reverter `AISoundType.gun` para `.step`. Trivial, 1 linha, quebra gameplay para todo mundo hoje.
2. **Priorizar em seguida (🟠):** `AUD-03-02` (Camera.main fora da main thread — risco de crash), `AUD-03-03` (oclusão de porta quebrada — feature anunciada não funciona), `AUD-03-04` (race condition no pipeline de voz).
3. **Agrupar como item de GC/UX (🟡):** `AUD-03-06`, `AUD-03-07`, `AUD-03-08` — mesma família de telas (`PlayerVolumeMixerHUD`, `VoiceCalibrationHUD`, `MenuVoipHUD`), pode ser um único item de backlog.
4. **Agrupar como item de limpeza (🔵):** `AUD-03-12` a `AUD-03-17` — baixo risco, podem entrar num item de "polimento" único.
5. **Ver seção dedicada** (`docs/investigacao-canal-litenetlib-voip.md`) para a resposta ao Passo 3 (canal de rede LiteNetLib) — não gera item de backlog para o canal (não recomendado), mas recomenda avançar com Spatial Culling Host-Side.

---

## 6. Memória Consultada
- Memória consultada: `mods/TRL-SpeakFromTarkov/memory/sessions.md` (Sessões 1 a 16, v1.5.3) + estado do fork `modded-V4` em v1.5.4 (commit não documentado em sessions.md, cujo conteúdo é coberto por `AUD-03-01`).
- Relatórios anteriores consultados: `relatorio-auditoria-codigo-01.md`, `relatorio-auditoria-codigo-02.md` — nenhum achado ✅ Aplicado foi re-reportado; `AUD-01-07` e `AUD-01-10` reforçados por referência (ainda pendentes).
