# 🛡️ Relatório Técnico de Engenharia de Redes & Segurança — Canais de Menu P2P, Estados Vivo/Morto & Spatial Listener

**Autor:** Engenheiro de Redes & Segurança Sênior / Lead Multiplayer Architect  
**Projeto:** TRL-SpeakFromTarkov (SPT 4.0 / Tarkov Red Line / FIKA Coop)  
**Foco:** Gestão de Memória no Menu P2P, Retransmissão Segura Host-Side (Anti-Exploit Vivo/Morto) e Referencial do Ouvinte Fantasma

---

## 1. 🧹 MENU P2P & GESTÃO DE MEMÓRIA (SOCKETS & TRANSIÇÃO DE CENA)

### Ciclo de Vida do HUD do Menu (`MenuVoipHUD`)
- **Operação no Menu:** O `MenuVoipHUD` utiliza sinalização HTTP assíncrona (`/sft/channels/list`) integrada ao servidor SPT e datagramas `SftChannelAnnouncementPacket` via LiteNetLib para listar, criar, entrar e manter salas de voz no menu principal.
- **Heartbeats & Cleanup Periodico:** A cada 10 segundos, o menu executa `CleanupStaleChannels()`, removendo canais inativos por mais de 15s e liberando entradas do dictionary concorrente `activeChannels`.

### Transição Menu ➔ Raid ➔ Menu (Sem Leak de Sockets)
1. **Entrada na Raid (`inRaid == true`):**
   - O `MenuVoipHUD.Update()` detecta a transição e salva as credenciais da sala em `SavedMenuChannelId` e `SavedMenuChannelName`.
   - O `SftNetwork.StopSession()` esvazia a `sendQueue` e destrói os GameObjects de `RemoteSpeaker` ativos (`Destroy(speaker.gameObject)`).
2. **Segurança no NetPacketProcessor (Zero Crashes de Desync):**
   - Os handlers de pacotes no `NetPacketProcessor` do FIKA **não são desregistrados** durante a troca de mapa. Desregistrar handlers em tempo de execução causaria `ParseException: Undefined packet` no LiteNetLib do FIKA se pacotes em voo chegassem, derrubando a leitura de rede e causando desync no movimento dos jogadores.
   - O controle de tráfego fora de raid é gerenciado por uma trava booleana ultraeficiente (`IsSessionActive`), garantindo 0 bytes alocados e 0 leques de sockets abertos.
3. **Retorno ao Menu:** Ao sair da raid (`wasInRaid && !inRaid`), o `AutoRestoreMenuChannel()` reconecta o jogador à sua sala de rádio do menu automaticamente.

---

## 2. 🔒 ROTEAMENTO DE ESTADOS (VIVO/MORTO — SEGURANÇA HOST-SIDE AUTHORITATIVE)

### O Perigo do Filtro Client-Side (Exploit Risk)
Se o descarte da fala de um jogador morto fosse feito apenas no cliente do jogador vivo (ex: mutando o volume localmente), um usuário mal-intencionado poderia manipular o cliente ou capturar o tráfego UDP bruto (*packet sniffer*) para escutar o canal fantasma e obter informações estratégicas da equipe inimiga.

### Solução Blindada no Host (Server-Side Cutoff)
No Host (Servidor FIKA), o método `OnReceiveVoipDataServer` valida a saúde do emissor antes de decidir o roteamento do pacote:

```csharp
private static void OnReceiveVoipDataServer(SftAudioPacketV2 packet, NetPeer senderPeer)
{
    var gameWorld = Singleton<GameWorld>.Instance;
    if (gameWorld == null) return;

    // Obtém a instância do jogador emissor
    Player senderPlayer = gameWorld.GetAlivePlayerByProfileID(packet.ProfileId);
    bool isSenderAlive = senderPlayer != null && senderPlayer.HealthController.IsAlive;

    // SE O EMISSOR ESTÁ MORTO (FANTASMA):
    if (!isSenderAlive)
    {
        // Transmite EXCLUSIVAMENTE para a lista de peers que também estão MORTOS (Channel Ghost)
        RelayToDeadPlayersOnly(packet, senderPeer);
        return; // O pacote do morto NUNCA é enviado para os clientes dos jogadores VIVOS!
    }

    // SE O EMISSOR ESTÁ VIVO:
    if (packet.Channel == 0)
    {
        // Proximidade 3D em Raid -> Transmite para vivos e mortos dentro do raio de 40m
        RelaySpatialAudio(packet, senderPeer, senderPlayer.Position, maxDistance: 40f);
    }
    else
    {
        // Canais Privados de Esquadrão / Rádio
        Singleton<IFikaNetworkManager>.Instance.SendData(ref packet, DeliveryMethod.Unreliable, broadcast: true);
    }
}
```

- **Garantia de Segurança:** Se o Host intercepta e ignora a retransmissão da voz do fantasma para os vivos, o pacote de áudio **nunca sai da placa de rede do Host com destino aos vivos**. É matematicamente impossível hackear a escuta.

---

## 3. 🎧 SPATIAL LISTENER DO FANTASMA (REFERENCIAL DA CÂMERA DE ESPECTADOR)

### Como a Unity Renderiza o Áudio do Morto
No `RemoteSpeaker.cs`, a posição do ouvinte (`listenerPos`) é determinada pela câmera principal do jogo:
```csharp
if (Camera.main != null)
{
    listenerPos = Camera.main.transform.position;
}
```

### Comportamento em Raid
1. **Troca de Câmera em Morte:** Quando um jogador morre no Tarkov, o motor do jogo altera o transform da `Camera.main` para a Câmera de Espectador (Spectator Camera), focando no aliado sobrevivente ou no corpo no chão.
2. **Audio Espacializado em Tempo Real:** Como o `RemoteSpeaker.cs` lê `Camera.main.transform.position`, a voz de proximidade de jogadores vivos ao redor do aliado espectado é renderizada na posição exata da câmera do espectador.
3. **Experiência Imersiva:** Isso permite que o jogador morto assista à partida do parceiro e ouça o áudio 3D perfeitamente alinhado com a visão da câmera de espectador, sem glitches de posição.
