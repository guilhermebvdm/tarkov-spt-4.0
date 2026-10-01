# 010 — Bot Não Ouve Convidado · Spec Técnica

**Mod:** TRL-SpeakFromTarkov
**Spec funcional:** [010-bot-nao-ouve-convidado-01-spec.md](010-bot-nao-ouve-convidado-01-spec.md)
**Criado:** 2026-09-09

> Fonte primária de verdade para qualquer assinatura, fórmula ou ponto de patch: [references/eft-decompiled/Assembly-CSharp/](../../../../references/eft-decompiled/Assembly-CSharp/) e [references/fika-plugin/Fika.Core/](../../../../references/fika-plugin/Fika.Core/).

## 0. Confirmação da hipótese de autoridade host/convidado (evidência de código, Tier 1)

A hipótese levantada na spec funcional — bots só são simulados de forma autoritativa no host — está **confirmada por evidência direta de código**, não só inferência estrutural:

- [`Fika.Core/Main/Players/FikaBot.cs:26-29`](../../../../references/fika-plugin/Fika.Core/Main/Players/FikaBot.cs#L26): o doc-comment da classe que carrega `AIData.IsAI = true` e é usada para acessar `BotOwner` diz literalmente: **"Used to simulate bots for the host."** É a fonte mais direta possível — bots com IA real só existem como tal na máquina do host.
- [`Fika.Core/Main/Components/BotStateManager.cs:13-18`](../../../../references/fika-plugin/Fika.Core/Main/Components/BotStateManager.cs#L13): só referencia `HostGameController`/`FikaServer` — é quem transmite o estado dos bots pra rede, exclusivamente do lado do host.
- [`Fika.Core/Main/GameMode/ClientGameController.cs`](../../../../references/fika-plugin/Fika.Core/Main/GameMode/ClientGameController.cs): usado por convidados, sem nenhuma lógica própria de `BotsController`/spawn de bot.
- [`Fika.Core/Main/Utils/FikaBackendUtils.cs:126-142`](../../../../references/fika-plugin/Fika.Core/Main/Utils/FikaBackendUtils.cs#L126): expõe `IsServer`/`IsClient`/`IsSinglePlayer` públicos — API oficial do FIKA pra essa distinção, usada no desenho abaixo.

**Conclusão:** quando `BotVoiceBridge.cs:96-141` (`TriggerBotVoiceEvent`) roda na máquina de um **convidado**, `Singleton<IBotGame>.Instance.BotsController.Bots.BotOwners` não é a lista autoritativa que o resto da raid observa — a notificação nunca chega ao bot real. **O teste em raid (host falando vs. convidado falando) descrito na spec funcional ainda deve ser feito antes do `/code-mod`**, como confirmação final e prática — mas a causa raiz já está estabelecida por evidência de código suficiente pra desenhar a correção.

## 1. Estratégia

Não há patch Harmony em código do EFT nesta correção — `BotVoiceBridge.cs` é 100% classe própria do mod. A estratégia é:

1. **Reverter** `AISoundType.gun` → `AISoundType.step` em `BotVoiceBridge.cs:108` (`AUD-03-01`).
2. **Extrair** a notificação ao sistema de bots (`PlaySound` + `ForceBotResponsesInRadius`, hoje dentro de `TriggerBotVoiceEvent`) para um método novo `NotifyBotsOfVoice(Player, float, EPhraseTrigger)`, chamável tanto localmente (host/singleplayer) quanto a partir de um pacote de rede recebido (convidado → host).
3. **Bifurcar** em `TriggerBotVoiceEvent`: se `FikaBackendUtils.IsClient`, enviar um pacote novo (`SftBotVoicePowerPacket`) pro host em vez de chamar `NotifyBotsOfVoice` localmente; caso contrário (host ou singleplayer), chamar `NotifyBotsOfVoice` como hoje.
4. **Registrar** o pacote novo no `SftNetwork.EnsurePacketsRegistered()`, no mesmo padrão dos pacotes existentes (`SftAudioPacketV2`, `SftChannelAnnouncementPacket`).
5. **Remover** a chamada a `SayPhrase()` (`AUD-03-10`) — sem efeito comprovado na IA vanilla (confirmado em `references/eft-decompiled/Assembly-CSharp/BotReceiver.cs:71-94`).

**Alternativa descartada:** usar `IFikaNetworkManager.SendVOIPData`/`EPacketType.VOIP` (API nativa de voz do FIKA) para transportar esse aviso. Descartada porque esse canal é hardcoded para alimentar exclusivamente `VOIPClient`/`VOIPServer` (Dissonance) — que o próprio mod já desliga via `GameSessionPatcher.cs:106-142` — e não tem como ser reaproveitado sem patchear `OnNetworkReceive` privado do FIKA (ver `docs/investigacao-canal-litenetlib-voip.md`, achado N1). O caminho de pacote customizado que o mod já usa pra `SftAudioPacketV2` é a opção pública, documentada e já validada em produção.

## 2. Pontos de patch

N/A — nenhum patch Harmony novo. A correção é código próprio do mod (`BotVoiceBridge.cs`, `Network/SftNetwork.cs`, novo arquivo `Network/SftBotVoicePowerPacket.cs`). O único ponto de patch existente e relevante (`GameSessionPatcher.cs:106-142`, `FikaClientInitializeVoipPatch`/`FikaServerInitializeVoipPatch`) não é alterado — é só a razão pela qual a alternativa do canal nativo de VOIP foi descartada (seção 1).

## 3. Novas propriedades F12 (BepInEx)

Nenhuma. O comportamento de `EnableBotInteraction` (já existente, seção "AI Bot Interaction") passa a ser checado **tanto no convidado que fala quanto no host que processa** — ver seção 6 (Fluxo de dados) — mas nenhum `ConfigEntry` novo é criado nem o texto/tooltip do existente muda.

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `Network/SftBotVoicePowerPacket.cs` | CRIAR | Pacote `INetSerializable` novo: `ProfileId`, `Power` (float), `Trigger` (byte = `EPhraseTrigger`). Enviado só convidado→host (`broadcast:false`). |
| `Audio/BotVoiceBridge.cs` | MODIFICAR | Revert `AISoundType.gun`→`.step`; extrai `NotifyBotsOfVoice`; bifurca por `FikaBackendUtils.IsClient`; remove `SayPhrase()`. |
| `Network/SftNetwork.cs` | MODIFICAR | Registra `SftBotVoicePowerPacket` em `EnsurePacketsRegistered()`; novo handler estático `OnReceiveBotVoicePower` que resolve o `Player` autoritativo via `GameWorld.GetAlivePlayerByProfileID` e chama `BotVoiceBridge.NotifyBotsOfVoice`. |

## 5. Stubs de código

```csharp
// Network/SftBotVoicePowerPacket.cs
using System;
using Fika.Core.Networking;
using Fika.Core.Networking.LiteNetLib.Utils;

namespace TRL_SpeakFromTarkov.Network
{
    /// <summary>
    /// Enviado por um CONVIDADO (nunca pelo host) pra avisar o host que atingiu o volume de fala
    /// necessário pra notificar os bots. O host resolve Player/posição pela própria referência
    /// autoritativa (GameWorld.GetAlivePlayerByProfileID) em vez de confiar em posição enviada
    /// pela rede — Power é o único dado sensível transportado, e é clampado no recebimento
    /// (ver SftNetwork.OnReceiveBotVoicePower) contra spoof/bug de cliente.
    /// </summary>
    public struct SftBotVoicePowerPacket : INetSerializable
    {
#pragma warning disable CS8618
        public string ProfileId;
        public float Power;
        public byte Trigger; // EPhraseTrigger, ver BotVoiceBridge.TriggerBotVoiceEvent

        [ThreadStatic] private static NetDataWriter? _innerWriter;
#pragma warning restore CS8618

        public void Serialize(NetDataWriter writer)
        {
            var inner = _innerWriter ??= new NetDataWriter(true, 128);
            inner.Reset();

            inner.Put(ProfileId ?? string.Empty);
            inner.Put(Power);
            inner.Put(Trigger);

            writer.PutBytesWithLength(inner.Data, 0, checked((ushort)inner.Length));
        }

        public void Deserialize(NetDataReader reader)
        {
            ProfileId = string.Empty;
            Power = 0f;
            Trigger = 0;

            if (!reader.TryGetBytesWithLength(out var payload) || payload == null) return;

            try
            {
                var inner = new NetDataReader(payload);
                if (!inner.TryGetString(out ProfileId)) return;
                if (!inner.TryGetFloat(out Power)) return;
                if (!inner.TryGetByte(out Trigger)) return;
            }
            catch (Exception ex)
            {
                ProfileId = string.Empty;
                Network.SftNetwork.LogErrorThrottled("SftBotVoicePowerPacket.Deserialize", ex);
            }
        }
    }
}
```

```csharp
// Audio/BotVoiceBridge.cs — trecho modificado de TriggerBotVoiceEvent (a partir da linha 62 atual)
private void TriggerBotVoiceEvent(Player player, float peakLevel)
{
    float power;
    EPhraseTrigger trigger;
    bool isAggressive = false;

    // ... cálculo de power/trigger/isAggressive por peakLevel — SEM MUDANÇA (linhas 68-87 atuais) ...

    lastTriggerTime = Time.time;

    // ref: Fika.Core/Main/Utils/FikaBackendUtils.cs:136 — IsClient é true só quando NÃO somos o host
    if (Fika.Core.Main.Utils.FikaBackendUtils.IsClient)
    {
        // Convidado: BotsController local não é autoritativo (ver seção 0) — reporta ao host
        // em vez de notificar uma cópia do bot que ninguém mais observa.
        SendVoicePowerToHost(player.ProfileId, power, trigger);
    }
    else
    {
        // Host ou singleplayer: comportamento idêntico ao de hoje, sem mudança de caminho.
        NotifyBotsOfVoice(player, power, trigger);
    }

    // Seção 2 original (reprodução audível local do próprio personagem) — SEM MUDANÇA,
    // sempre roda no cliente de quem fala, independente de ser host ou convidado.
    float debugVolume = VoIPPlugin.BotVoiceDebugVolume != null ? VoIPPlugin.BotVoiceDebugVolume.Value : 0.0f;
    if (debugVolume > 0.001f)
    {
        player.Say(trigger, demand: true, 0f, (ETagStatus)0, 100, aggressive: isAggressive);
    }
}

private void SendVoicePowerToHost(string profileId, float power, EPhraseTrigger trigger)
{
    if (!Comfort.Common.Singleton<Fika.Core.Networking.IFikaNetworkManager>.Instantiated) return;

    var packet = new Network.SftBotVoicePowerPacket
    {
        ProfileId = profileId,
        Power = power,
        Trigger = (byte)trigger
    };
    // broadcast:false — vai só pro host (ServerConnection), nunca é relayado a outros convidados.
    Comfort.Common.Singleton<Fika.Core.Networking.IFikaNetworkManager>.Instance.SendData(
        ref packet, Fika.Core.Networking.LiteNetLib.DeliveryMethod.ReliableOrdered, broadcast: false);
}

/// <summary>
/// Notifica o sistema de bots sobre a voz de <paramref name="player"/>. SÓ deve ser chamado
/// no host (localmente, quando quem fala é o próprio host, ou a partir do handler de rede
/// quando o aviso vem de um convidado) — nunca num convidado, que não tem BotsController
/// autoritativo (ver seção 0 da spec técnica).
/// </summary>
internal void NotifyBotsOfVoice(Player player, float power, EPhraseTrigger trigger)
{
    if (player == null) return;

    // Clamp defensivo: Power cruzou a rede quando reportado por um convidado — um cliente
    // bugado ou hostil poderia reportar um valor fora da faixa esperada (ver tiers em
    // TriggerBotVoiceEvent: sussurro 3-10m, normal 10-30m, grito 30-60m).
    power = Mathf.Clamp(power, 3.0f, 60.0f);

    Vector3 soundPos = player.PlayerBones != null && player.PlayerBones.Head != null
        ? player.PlayerBones.Head.Original.position
        : player.Transform.position;

    if (Comfort.Common.Singleton<BotEventHandler>.Instantiated)
    {
        try
        {
            // ref: Assembly-CSharp/BotEventHandler.cs:1277
            // AISoundType.step (revertido de .gun, AUD-03-01) — voz não é mais tratada como tiro real
            Comfort.Common.Singleton<BotEventHandler>.Instance.PlaySound(player, soundPos, power, AISoundType.step);
        }
        catch (Exception ex)
        {
            VoIPPlugin.Log.LogWarning($"[SFT] Erro ao notificar BotEventHandler: {ex.Message}");
        }
    }

    // SayPhrase() removido (AUD-03-10) — BotReceiver nativo ignora os triggers usados aqui
    // (ref: Assembly-CSharp/BotReceiver.cs:71-94), sem efeito comprovado na IA vanilla.

    ForceBotResponsesInRadius(player, soundPos, power, trigger); // método existente, sem mudança
}
```

```csharp
// Network/SftNetwork.cs — trecho modificado de EnsurePacketsRegistered() e novo handler
public static void EnsurePacketsRegistered()
{
    // ... guard clauses existentes sem mudança ...
    try
    {
        currentManager.RegisterPacket<SftAudioPacketV2>(OnReceiveVoipDataV2);
        currentManager.RegisterPacket<SftAudioPacket>(OnReceiveVoipDataLegacy);
        currentManager.RegisterPacket<SftChannelAnnouncementPacket>(OnReceiveChannelAnnouncement);
        currentManager.RegisterPacket<SftBotVoicePowerPacket>(OnReceiveBotVoicePower); // NOVO

        _lastRegisteredManager = currentManager;
        // ... log existente ...
    }
    catch (Exception ex) { /* sem mudança */ }
}

/// <summary>
/// Só tem efeito real no HOST — um convidado nunca recebe este pacote (é enviado com
/// broadcast:false, então o FIKA nunca o relaya a outros convidados; ver
/// references/fika-plugin/Fika.Core/Networking/FikaServer.cs:932-936).
/// </summary>
private static void OnReceiveBotVoicePower(SftBotVoicePowerPacket packet)
{
    try
    {
        if (VoIPPlugin.EnableBotInteraction != null && !VoIPPlugin.EnableBotInteraction.Value) return;
        if (string.IsNullOrEmpty(packet.ProfileId)) return;
        if (!Comfort.Common.Singleton<EFT.GameWorld>.Instantiated) return;

        // ref: Assembly-CSharp/EFT/GameWorld.cs:1238 — referência autoritativa do host, não a
        // posição enviada pelo convidado (que este pacote nem transporta).
        var player = Comfort.Common.Singleton<EFT.GameWorld>.Instance.GetAlivePlayerByProfileID(packet.ProfileId);
        if (player == null) return;

        // ref: Core/VoipController.cs:15,20 — Instance e botVoiceBridge já são públicos, sem mudança necessária aqui.
        Core.VoipController.Instance?.botVoiceBridge?.NotifyBotsOfVoice(player, packet.Power, (EPhraseTrigger)packet.Trigger);
    }
    catch (Exception ex)
    {
        LogErrorThrottled("Erro no callback OnReceiveBotVoicePower", ex);
    }
}
```

**Resolvido:** `VoipController.Instance` (`Core/VoipController.cs:15`) e `botVoiceBridge` (`Core/VoipController.cs:20`, getter público) já existem — não precisa de mudança estrutural nova, o stub acima usa esse caminho diretamente.

**Nota (PA-01-02 da review 01):** se `VoipController.Instance` ou `botVoiceBridge` ainda não estiverem prontos no exato momento em que o pacote chega (ex.: convidado fala nos primeiros frames da raid), a chamada acima é descartada silenciosamente pelo `?.` — isso é **aceitável de propósito**: a janela é de poucos frames, o efeito é só perder uma frase isolada (não a interação como um todo), e o convidado pode simplesmente falar de novo. Não é necessário log ou retry pra esse caso.

## 6. Fluxo de dados

```
[Convidado fala] → MicrophoneCapturer/VoipProcessor (thread de captura, sem mudança)
  → BotVoiceBridge.ProcessVoiceFrame → TriggerBotVoiceEvent (calcula power/trigger, sem mudança)
    → FikaBackendUtils.IsClient == true
      → SendVoicePowerToHost → SftBotVoicePowerPacket → IFikaNetworkManager.SendData(broadcast:false)
        → [rede] → FikaServer.OnNetworkReceive (host) → NetPacketProcessor.ReadAllPackets
          → SftNetwork.OnReceiveBotVoicePower (SÓ roda no host)
            → GameWorld.GetAlivePlayerByProfileID (Assembly-CSharp/EFT/GameWorld.cs:1238) — resolve Player autoritativo
            → BotVoiceBridge.NotifyBotsOfVoice
              → BotEventHandler.PlaySound(player, soundPos, power, AISoundType.step) (Assembly-CSharp/BotEventHandler.cs:1277)
              → ForceBotResponsesInRadius (BotVoiceBridge.cs:146-233, sem mudança) → bot.GetPlayer.Say(...)

[Host fala] → ... → TriggerBotVoiceEvent → FikaBackendUtils.IsClient == false
    → NotifyBotsOfVoice (chamado direto, mesmo caminho de sempre — nenhuma regressão pro caso já funcional)
```

## 7. Riscos e dependências

- **Teste em raid pendente:** a spec funcional exige confirmar host-vs-convidado numa raid real antes do `/code-mod`. O design acima assume a hipótese confirmada por evidência de código (seção 0), mas o teste prático ainda valida se não há um segundo fator em jogo.
- **Item `014-spatial-culling-host-side/`:** ambos os itens fazem o host processar dados vindos de convidados sobre "quem está falando, com que intensidade". Avaliar na implementação se compensa unificar em um único pacote/handler — não obrigatório, só oportunidade. Os dois itens tocam `Network/SftNetwork.cs` em métodos diferentes (`EnsurePacketsRegistered`/`OnReceiveBotVoicePower` aqui vs. `DrainSendQueue`/`OnReceiveVoipDataV2`/`RelayVoiceToNearbyPeers` no 014) — risco de conflito de merge baixo, mas implementar em sequência (não em paralelo) evita qualquer surpresa.
- **Nenhum patch Harmony existente é afetado** — `GameSessionPatcher.cs` permanece intocado por este item.
- **Config `EnableBotInteraction` dupla checagem** (convidado ao enviar, host ao receber) resolve o gap sinalizado na revisão da spec funcional — ambos precisam estar com a opção ligada pra a notificação ocorrer, decisão registrada aqui.

## 8. Checklist de implementação

- [ ] Fazer o teste em raid host-vs-convidado descrito na spec funcional (critério de aceite obrigatório) — se a hipótese não se confirmar, parar e reabrir a investigação antes de codar.
- [ ] Criar `Network/SftBotVoicePowerPacket.cs`.
- [ ] Extrair `NotifyBotsOfVoice` em `BotVoiceBridge.cs`, reverter `AISoundType.gun`→`.step`, remover `SayPhrase()`.
- [ ] Bifurcar `TriggerBotVoiceEvent` por `FikaBackendUtils.IsClient`.
- [ ] Registrar `SftBotVoicePowerPacket` e implementar `OnReceiveBotVoicePower` em `SftNetwork.cs`.
- [ ] Testar: host falando (comportamento idêntico a hoje, sem regressão); convidado falando (bot reage, confirmando a correção); convidado com `Bot Speech Debug Volume = 0%` (garantia de resposta independente de volume local); múltiplos convidados falando ao mesmo tempo.
- [ ] Compilar via `dotnet build` com 0 erros/0 avisos.

## 9. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid: start hook + stop hooks idempotentes — AP-01 | N/A | `SendVoicePowerToHost` não introduz recurso persistente — é uma chamada de envio pontual dentro de `TriggerBotVoiceEvent`, já executada na main thread a cada frame de `VoipController.Update()` (existente, sem novo hook de start/stop necessário). *(Correção PA-01-01 da review 01 — a evidência original citava `sendQueue`, que pertence ao pacote de áudio e não a este pacote novo.)* |
| 2 | Filtro MainPlayer/Fika em todo patch que reage a ação de player — AP-02 | ✅ | `OnReceiveBotVoicePower` guarda `Singleton<EFT.GameWorld>.Instantiated` antes de resolver o player (seção 5); `NotifyBotsOfVoice` guarda `player == null`. |
| 3 | Alvos ofuscados/virtuais resolvidos por assinatura; overrides auditados — AP-03 | N/A | Nenhum patch Harmony/override de método virtual do EFT neste item (seção 2). |
| 4 | Mudança de estado via API canônica do EFT; side-effects mapeados — AP-04 | ✅ | `PlaySound`/`ForceBotResponsesInRadius` são as mesmas APIs já usadas hoje (`BotEventHandler.cs:1277`); nenhuma reflection nova introduzida. |
| 5 | Estado entre raids: raid1→exit→raid2 e alt-F4/morte/MIA cobertos | ✅ | Ver spec funcional §Corner cases — nenhum estado novo persiste entre raids; `lastTriggerTime`/cooldown já existente não é alterado. |
| 6 | Semântica/defaults/faixas de cada ConfigEntry sem ambiguidade — AP-05 | N/A | Nenhum `ConfigEntry` novo (seção 3); comportamento de `EnableBotInteraction` existente só passa a ser checado nos dois lados (host + convidado), documentado na seção 7. |
| 7 | Re-invocação de método patcheado tem reentry-guard — AP-07 | N/A | Nenhum patch Harmony. |
| 8 | Flags/caches de intercept validados contra o contexto atual — AP-08 | N/A | Sem cache de intercept nesta correção. |
| 9 | Todo patch-point reconfirmado no `.cs` do dump — AP-09 | ✅ | `BotEventHandler.cs:1277,1282`, `BotReceiver.cs:71-94`, `GameWorld.cs:1238` lidos diretamente nesta sessão (seção 0 e stubs). |
| 10 | Skill EFT usada como lever confirmada não-inerte — AP-10 | N/A | Não se aplica (não é feature de skill do EFT). |
| 11 | Pacote FIKA próprio: envelope + `TryGet*` + campos resetados no Deserialize, envio só na main thread, registro por instância, zero `UnregisterPacket`, airbag com throttle — AP-11 | ✅ | `SftBotVoicePowerPacket` segue exatamente o padrão de `SftChannelAnnouncementPacket.cs`/`SftAudioPacketV2` já em produção (envelope `PutBytesWithLength`, `TryGet*`, reset no topo do `Deserialize`); não usa campo `Valid` explícito porque nenhum pacote existente do mod usa — os campos resetados fazem esse papel. Envio (`SendVoicePowerToHost`) roda de dentro de `TriggerBotVoiceEvent`, chamado de `VoipController.Update()` (main thread, confirmado na spec funcional). Registro via `EnsurePacketsRegistered()` por referência de instância, mesmo padrão dos demais pacotes. Handler `OnReceiveBotVoicePower` tem `try/catch` com `LogErrorThrottled`. |

## Histórico

| Data | Evento |
|---|---|
| 2026-09-09 | Spec técnica criada via `/create-technical-spec` |
| 2026-09-09 | Review 01 aplicada — 3 pontos resolvidos (evidência do check AP-01, nota de timing de nulo, nota de arquivo compartilhado com o item 014) |
