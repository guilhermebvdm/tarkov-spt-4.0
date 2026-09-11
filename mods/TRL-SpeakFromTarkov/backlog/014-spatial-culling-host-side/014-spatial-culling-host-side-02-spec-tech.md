# 014 — Spatial Culling Host-Side · Spec Técnica

**Mod:** TRL-SpeakFromTarkov
**Spec funcional:** [014-spatial-culling-host-side-01-spec.md](014-spatial-culling-host-side-01-spec.md)
**Criado:** 2026-09-09

> Fonte primária de verdade para rede/coop: [references/fika-plugin/Fika.Core/](../../../../references/fika-plugin/Fika.Core/). Para posição/estado de jogador: [references/eft-decompiled/Assembly-CSharp/](../../../../references/eft-decompiled/Assembly-CSharp/).

## 1. Estratégia

Nenhum patch Harmony — mudança inteiramente em `Network/SftNetwork.cs`, usando só API pública do `IFikaNetworkManager` já documentada e já usada em outros pontos do próprio mod.

**Restrição-chave confirmada por evidência (`references/fika-plugin/Fika.Core/Networking/FikaServer.cs:932-936`):** o relay automático do FIKA (quando um pacote chega no host com o byte de `broadcast=true`) faz `_netServer.SendToAll(...)` **antes** de qualquer parsing específico do mod — não dá pra interceptar esse relay pra aplicar filtro de distância. Por isso, a estratégia é: **parar de usar `broadcast:true` para o Canal 0** (raid 3D) e o host passa a decidir, pacote por pacote, pra quem retransmitir, usando `SendDataToPeer` (API pública, `IFikaNetworkManager.cs:108`) em vez do broadcast automático. Canais 1 (menu) e 2 (espectador) continuam com `broadcast:true` inalterado — não são o alvo deste item.

**Como o host mapeia jogador → `NetPeer`:** `FikaPlayer.NetId` (campo público, `Fika.Core/Main/Players/FikaPlayer.cs:55`) + `IFikaNetworkManager.GetPeerById(int id)` (`IFikaNetworkManager.cs:207`) — mesmo padrão já usado internamente pelo FIKA (`FikaBot.CreateBot` atribui `NetId` do mesmo jeito, `Fika.Core/Main/Players/FikaBot.cs:61`).

## 2. Pontos de patch

N/A — nenhum patch Harmony. Toda a mudança é em `Network/SftNetwork.cs`, usando APIs públicas do `Fika.Core.Networking.IFikaNetworkManager`.

## 3. Novas propriedades F12 (BepInEx)

Nenhuma. Reaproveita `VoIPPlugin.MaxHearingDistance` (já existente) com a mesma margem de tolerância de 10% já usada em `HandleVoipPacket` (`SftNetwork.cs:417-418`).

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `Network/SftNetwork.cs` | MODIFICAR | `DrainSendQueue()`: Canal 0 passa a usar `broadcast:false` (convidado) ou relay direto por distância (host); novo método `RelayVoiceToNearbyPeers`; `OnReceiveVoipDataV2` passa a também relayar (só no host) antes/depois de processar localmente. |

## 5. Stubs de código

```csharp
// Network/SftNetwork.cs — DrainSendQueue() modificado

private void DrainSendQueue()
{
    if (sendQueue.IsEmpty) return;
    if (!IsSessionActive || !Singleton<IFikaNetworkManager>.Instantiated)
    {
        while (sendQueue.TryDequeue(out _)) { }
        return;
    }

    string myProfileId = LocalSessionId;
    Player? myPlayer = null;
    if (Singleton<GameWorld>.Instantiated && Singleton<GameWorld>.Instance.MainPlayer != null)
    {
        myPlayer = Singleton<GameWorld>.Instance.MainPlayer;
        string pId = myPlayer.ProfileId;
        if (!string.IsNullOrEmpty(pId)) myProfileId = pId;
    }

    EnsurePacketsRegistered();

    while (sendQueue.TryDequeue(out var pending))
    {
        try
        {
            var packet = new SftAudioPacketV2
            {
                ProfileId = myProfileId,
                Channel = pending.Channel,
                AudioData = pending.Data,
                VoiceLevel = pending.VoiceLevel
            };

            // NOVO: só o Canal 0 (raid 3D) participa do culling por distância — Canais 1/2
            // continuam broadcast:true como hoje (espectador não tem noção de distância).
            if (pending.Channel == 0 && Fika.Core.Main.Utils.FikaBackendUtils.IsServer)
            {
                // Host falando: nunca cruza a rede pra "voltar" pro próprio host (FikaServer.SendData
                // ignora o parâmetro broadcast e sempre faz SendToAll — ver seção 1), então o host
                // já decide aqui, localmente, pra quem retransmitir.
                if (myPlayer != null) RelayVoiceToNearbyPeers(packet, myPlayer, excludePeer: null);
            }
            else if (pending.Channel == 0 && Fika.Core.Main.Utils.FikaBackendUtils.IsClient)
            {
                // Convidado: broadcast:false — vai só pro host, que decide o relay (ver OnReceiveVoipDataV2).
                Singleton<IFikaNetworkManager>.Instance.SendData(
                    ref packet, Fika.Core.Networking.LiteNetLib.DeliveryMethod.Unreliable, broadcast: false);
            }
            else
            {
                // Canal 1 (menu) / Canal 2 (espectador) — comportamento inalterado.
                Singleton<IFikaNetworkManager>.Instance.SendData(
                    ref packet, Fika.Core.Networking.LiteNetLib.DeliveryMethod.Unreliable, broadcast: true);
            }
        }
        catch (Exception ex)
        {
            LogErrorThrottled("Erro ao transmitir frame de áudio", ex);
        }
    }
}
```

```csharp
// Network/SftNetwork.cs — NOVO método de relay seletivo (só tem efeito real no host)

/// <summary>
/// Retransmite o pacote de voz só pra jogadores dentro do alcance de audição, em vez do
/// broadcast automático do FIKA pra todos. Chamado tanto quando o HOST fala (direto de
/// DrainSendQueue) quanto quando um CONVIDADO fala (via OnReceiveVoipDataV2, depois de o
/// pacote ter chegado só no host por broadcast:false).
/// </summary>
// Corrigido na Review 01 (PA-01-01): AllAlivePlayersList só contém jogadores VIVOS
// (Assembly-CSharp/EFT/GameWorld.cs:556, populado condicionalmente em RegisterPlayer:2260-2278).
// Jogadores mortos/espectadores têm "escuta dupla" documentada (Canal 0 em 3D dos vivos ao redor +
// Canal 2 em 2D) — iterar só AllAlivePlayersList quebraria essa feature. RegisteredPlayers
// (List<IPlayer>, GameWorld.cs:546) inclui todo mundo, vivo ou morto (RegisterPlayer:2262,
// incondicional) — é essa a lista certa pra decidir o relay.
private static void RelayVoiceToNearbyPeers(SftAudioPacketV2 packet, Player senderPlayer, Fika.Core.Networking.LiteNetLib.NetPeer? excludePeer)
{
    if (!Fika.Core.Main.Utils.FikaBackendUtils.IsServer) return; // defensivo — nunca deveria rodar num convidado
    if (!Singleton<IFikaNetworkManager>.Instantiated) return;
    if (!Singleton<GameWorld>.Instantiated) return;

    var manager = Singleton<IFikaNetworkManager>.Instance;
    float maxHearing = VoIPPlugin.MaxHearingDistance != null ? VoIPPlugin.MaxHearingDistance.Value : 60f;
    float maxCullDistance = maxHearing * 1.10f; // mesma margem de HandleVoipPacket (SftNetwork.cs:417-418)
    float sqrMaxCull = maxCullDistance * maxCullDistance;

    var allPlayers = Singleton<GameWorld>.Instance.RegisteredPlayers; // List<IPlayer> — vivos E mortos/espectadores
    if (allPlayers == null) return;

    for (int i = 0; i < allPlayers.Count; i++)
    {
        var candidate = allPlayers[i];
        if (candidate == null || candidate == senderPlayer) continue;
        if (candidate.ProfileId == packet.ProfileId) continue; // nunca retransmite pro próprio remetente

        float sqrDist = (candidate.Position - senderPlayer.Position).sqrMagnitude;
        if (sqrDist > sqrMaxCull) continue; // fora de alcance — nem gasta banda de upload do host com esse peer

        // ref: Fika.Core/Main/Players/FikaPlayer.cs:55 (NetId), IFikaNetworkManager.cs:207 (GetPeerById)
        if (candidate is not Fika.Core.Main.Players.FikaPlayer fikaPlayer) continue; // bot ou tipo inesperado — sem peer real
        var peer = manager.GetPeerById(fikaPlayer.NetId);
        if (peer == null || peer == excludePeer) continue;

        try
        {
            var outgoing = packet; // struct — cópia local por peer, SendDataToPeer pede ref
            manager.SendDataToPeer(ref outgoing, Fika.Core.Networking.LiteNetLib.DeliveryMethod.Unreliable, peer);
        }
        catch (Exception ex)
        {
            LogErrorThrottled("Erro ao retransmitir voz por distância (host)", ex);
        }
    }
}
```

```csharp
// Network/SftNetwork.cs — OnReceiveVoipDataV2 modificado (relay quando quem fala é um convidado)

private static void OnReceiveVoipDataV2(SftAudioPacketV2 packet)
{
    if (packet.AudioData == null) return;

    // NOVO: só no host, e só Canal 0 — retransmite pros peers dentro do alcance antes de
    // processar a audição local do próprio host (ordem não importa, os dois são independentes).
    if (packet.Channel == 0 && Fika.Core.Main.Utils.FikaBackendUtils.IsServer
        && Singleton<GameWorld>.Instantiated)
    {
        var senderPlayer = Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(packet.ProfileId);
        if (senderPlayer != null)
        {
            RelayVoiceToNearbyPeers(packet, senderPlayer, excludePeer: null);
        }
    }

    DispatchVoipPacket(packet.ProfileId, packet.Channel, packet.AudioData, packet.VoiceLevel,
        nameof(OnReceiveVoipDataV2));
}
```

## 6. Fluxo de dados

```
[Convidado fala, Canal 0] → DrainSendQueue → SendData(broadcast:false) → só chega no HOST
  → FikaServer.OnNetworkReceive: byte de broadcast é 0 → SEM relay automático (ver Fika.Core/Networking/FikaServer.cs:932-936)
  → _packetProcessor.ReadAllPackets → OnReceiveVoipDataV2 (SÓ dispara no host)
    → GameWorld.GetAlivePlayerByProfileID (Assembly-CSharp/EFT/GameWorld.cs:1238) — resolve posição autoritativa
    → RelayVoiceToNearbyPeers → itera RegisteredPlayers (vivos + mortos) → sqrMagnitude vs. MaxHearingDistance*1.10
      → dentro do alcance: FikaPlayer.NetId → GetPeerById → SendDataToPeer (só esses peers recebem)
      → fora do alcance: nunca recebe o pacote pela rede
    → DispatchVoipPacket (audição local do PRÓPRIO host, se ele estiver dentro do alcance de quem falou)

[Host fala, Canal 0] → DrainSendQueue detecta IsServer → RelayVoiceToNearbyPeers direto
  (nunca passa pelo caminho SendData/broadcast — o host decide localmente, sem round-trip de rede)
```

## 7. Riscos e dependências

- **Sinergia com o item `010-bot-nao-ouve-convidado/`:** ambos os itens fazem o host processar "quem está falando, onde, com que intensidade" vindo de um convidado. `RelayVoiceToNearbyPeers` (aqui) e `NotifyBotsOfVoice`/`SendVoicePowerToHost` (item 010) são fluxos paralelos e independentes — não compartilham pacote nem handler, mas ambos dependem da mesma referência autoritativa (`GameWorld.GetAlivePlayerByProfileID`). Não há conflito, mas revisar os dois itens juntos na implementação ajuda a manter o padrão consistente.
- **Canal 1/2 explicitamente fora deste item** — qualquer regressão nesses canais durante a implementação é bug, não é escopo aceito.
- **`candidate is FikaPlayer`** — confirmado (Review 01, PA-01-02): `Fika.Core/Main/Players/ObservedPlayer.cs:41` — `public sealed class ObservedPlayer : FikaPlayer`. O cast funciona pra qualquer jogador humano remoto observado pelo host, não só `MyPlayer` local.
- **Nenhum patch Harmony afetado.**

## 8. Checklist de implementação

- [x] `SftNetwork.cs`: modificar `DrainSendQueue()` pra bifurcar por canal + papel (host/convidado).
- [x] `SftNetwork.cs`: implementar `RelayVoiceToNearbyPeers` iterando `GameWorld.RegisteredPlayers` (não `AllAlivePlayersList` — corrigido na Review 01). Try/catch por peer dentro do loop (não em volta do loop inteiro) — falha isolada num peer não aborta o relay pros demais.
- [x] `SftNetwork.cs`: modificar `OnReceiveVoipDataV2` pra chamar o relay quando o host recebe voz de um convidado.
- [ ] Testar: convidado falando dentro do alcance de outro convidado — ambos se ouvem normalmente. Convidado falando fora do alcance de outro — o distante não recebe pacote nenhum (medir com contador de pacotes recebidos, não só ausência de som). Host falando — comportamento idêntico ao de hoje pra quem está no alcance. Canal 2 (espectador) — continua broadcast total, sem regressão. **Jogador morto/espectador continua recebendo a voz de jogadores vivos dentro do alcance, exatamente como hoje** (testar com pelo menos 1 jogador morto na raid — corrigido na Review 01). *(Requer teste em jogo com pelo menos 2 clientes/convidados.)*
- [ ] Compilar via `dotnet build` com 0 erros/0 avisos. *(Requer `/compile-mod`.)*

## 9. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid: start hook + stop hooks idempotentes — AP-01 | N/A | Nenhum estado novo persiste entre raids; a decisão de relay é por pacote, em tempo real. |
| 2 | Filtro MainPlayer/Fika em todo patch que reage a ação de player — AP-02 | ✅ | `RelayVoiceToNearbyPeers` guarda `Singleton<GameWorld>.Instantiated` e `allPlayers == null`; `OnReceiveVoipDataV2` guarda `Singleton<GameWorld>.Instantiated` antes de resolver o sender. |
| 3 | Alvos ofuscados/virtuais resolvidos por assinatura; overrides auditados — AP-03 | N/A | Nenhum patch Harmony/override de método virtual do EFT. |
| 4 | Mudança de estado via API canônica do EFT/FIKA; side-effects mapeados — AP-04 | ✅ | `SendDataToPeer`/`GetPeerById` são API pública documentada do `IFikaNetworkManager` (`IFikaNetworkManager.cs:108,207`), já citada como alternativa preferida na investigação de rede (`docs/investigacao-canal-litenetlib-voip.md`). |
| 5 | Estado entre raids: raid1→exit→raid2 e alt-F4/morte/MIA cobertos | ✅ | Nenhum campo estático/persistente novo — `RelayVoiceToNearbyPeers` só lê `GameWorld`/`IFikaNetworkManager` correntes a cada chamada. |
| 6 | Semântica/defaults/faixas de cada ConfigEntry sem ambiguidade — AP-05 | N/A | Nenhum `ConfigEntry` novo; reaproveita `MaxHearingDistance` existente sem alterar faixa/default. |
| 7 | Re-invocação de método patcheado tem reentry-guard — AP-07 | N/A | Nenhum patch Harmony. |
| 8 | Flags/caches de intercept validados contra o contexto atual — AP-08 | N/A | Nenhum cache de intercept neste item. |
| 9 | Todo patch-point reconfirmado no `.cs` do dump — AP-09 | ✅ | `FikaServer.cs:932-936`, `FikaPlayer.cs:55`, `FikaBot.cs:61`, `IFikaNetworkManager.cs:108,207`, `GameWorld.cs:1238` lidos diretamente nesta sessão. |
| 10 | Skill EFT usada como lever confirmada não-inerte — AP-10 | N/A | Não se aplica. |
| 11 | Pacote FIKA próprio: envelope + `TryGet*` + ... — AP-11 | N/A | Nenhum pacote novo — reaproveita `SftAudioPacketV2` já existente e validado em produção, só muda o método de envio (`SendData`→`SendDataToPeer`) e o valor de `broadcast`. |

## Histórico

| Data | Evento |
|---|---|
| 2026-09-09 | Spec técnica criada via `/create-technical-spec` |
| 2026-09-09 | Review 01 aplicada — bloqueador resolvido (relay agora usa `RegisteredPlayers`, não `AllAlivePlayersList`, preservando a escuta dupla do espectador); TODO da hierarquia `FikaPlayer` confirmado e removido |
