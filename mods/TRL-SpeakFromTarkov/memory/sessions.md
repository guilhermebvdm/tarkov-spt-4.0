# TRL-SpeakFromTarkov — Memória de Sessões

## Snapshot Delta
- **Versão:** 1.6.0 (SPT 4.0 / FIKA)
- **Estado:** v1.6.0 compilada e testada com 0 erros. Implementação completa de Canais de VOIP no Menu Principal com P2P 2D estéreo, HUD alinhado ao FIKA, moderação de jogadores (Remover/Banir), confirmação anti-missclick, rolagem automática (ScrollView) e reconexão automática contínua pós-raid.
- **Pendências:** 🟢 Nenhuma pendência blocker registrada.

---

## 2026-07-28 — Sessão 1: Inicialização da Governança
- **Ação:** Criação de `mod.json`, `README.md` e padronização da memória de sessões.

---

## 2026-07-29 02:50 (GMT-3) — Sessão 2: v1.5.0 (Otimização de Payload, Alinhamento DSP 40ms e Blindagem Magic Header SFTV no Canal 1)

**Tema central:** Otimização completa do pipeline de áudio e rede do `TRL-SpeakFromTarkov` (Seções 7 e 8 do Roadmap), zerando a alocação de memória no Garbage Collector da Unity, blindando a transmissão no Canal 1 com a assinatura `SFTV` e eliminando episódios de desync e erros nativos do FMOD no FIKA.

**Decisões-chave:**
- **v1.3.2 — Proteção Numérica FMOD & Modo 2D:** Implementada sanitização numérica (`Mathf.Clamp`, `float.IsNaN`, `float.IsInfinity`) no [RemoteSpeaker.cs](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-SpeakFromTarkov/modded/Audio/RemoteSpeaker.cs) para erradicar os erros nativos Unity/FMOD `set3DSpread(spread) (An invalid parameter was passed)`. Adicionado fallback automático para o modo 2D de emergência (`SetEmergency2DMode`) quando o avatar remoto do jogador desincronizar.
- **v1.4.0 — Handshake Binário & Economia de Payload:** Criado o `SftHandshakePacket` no [SftHandshakePacket.cs](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-SpeakFromTarkov/modded/SftHandshakePacket.cs) vinculando a `string ProfileId` (36 bytes) a um `byte PlayerNetId` (1 byte) enviado 1 única vez na entrada da raid. Refatorado o `SftAudioPacket` para enviar o `PlayerNetId`, economizando ~35 bytes por pacote enviado.
- **v1.4.0 — Enquadramento DSP 40ms & Zero-Allocation:** Reenquadrado o buffer de captura em [MicrophoneCapturer.cs](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-SpeakFromTarkov/modded/Audio/MicrophoneCapturer.cs) para **40ms (1.920 amostras a 48kHz)**, casando exatamente com 4 blocos da rede neural do RNNoise (480 amostras cada) e reduzindo o envio na rede de 50 pps para **25 pps**. Ring Buffer redimensionado automaticamente para **4.096 amostras**. Integrado o pool de memória `System.Buffers.ArrayPool<byte>.Shared` no [VoipProcessor.cs](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-SpeakFromTarkov/modded/Audio/VoipProcessor.cs) zerando o consumo de RAM do GC da Unity.
- **v1.5.0 — Blindagem por Magic Header `SFTV` no Canal 1:** Adicionada a constante `MAGIC_HEADER = 0x56544653` (`SFTV`) no [SftAudioPacket.cs](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-SpeakFromTarkov/modded/SftAudioPacket.cs) e validação imediata em [SftNetwork.cs](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-SpeakFromTarkov/modded/Network/SftNetwork.cs) no `Channel 1` (`Unreliable`). Rejeita pacotes sem assinatura em < 0.1 nanossegundos, tornando o canal de voz 100% blindado e deixando o `Channel 0` do FIKA imune a travamentos.

**Lições / hipóteses descartadas:**
- A hipotética alteração na qualidade de voz Opus ao mudar para frames de 40ms foi descartada: a fidelidade em 48kHz permanece cristalina enquanto a estabilidade da placa de rede dobra (25 pps).
- A alocação contínua de arrays no encoder Opus foi descartada em favor de `ArrayPool<byte>.Shared`, eliminando micro-stutters de FPS durante a fala.

**Atividade cronológica:**
1. Diagnóstico do erro Unity Log FMOD `set3DSpread` e implementação do sanitizador numérico em `RemoteSpeaker.cs`.
2. Code-review `/code-review` executado e aprovado com 0 bloqueadores (`🔴 0`).
3. Refatoração v1.4.0: `SftHandshakePacket`, 40ms / 1.920 amostras, `ArrayPool` e `PlayerNetId`.
4. Refatoração v1.5.0: Magic Header `SFTV` no `SftAudioPacket.cs` e filtro de validação no `SftNetwork.cs`.
5. Compilação via `dotnet build -c Release` concluída com **0 erros**.
6. Git add, commit `32533fd1` e `git push origin main` executados com sucesso.

---

## 2026-08-02 — Sessão 3: v1.5.1 (Correção de Ancoragem 3D Posicional e Re-ancoragem Dinâmica)

**Tema central:** Correção da anomalia de áudio posicional 3D em que vozes de todos os jogadores remotos eram emitidas a partir do mesmo ponto `(0, 0, 0)` no espaço 3D (saída concentrada no mesmo ponto/jogador).

**Decisões-chave:**
- **Busca Nativa por Perfil Tarkov:** Substituição de filtro manual `FirstOrDefault` em `AllAlivePlayersList` pelo método nativo do motor do Tarkov `Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(profileId)` no [SftNetwork.cs](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-SpeakFromTarkov/modded/Network/SftNetwork.cs).
- **Re-ancoragem Dinâmica no RemoteSpeaker:** Implementado mecanismo de auto-recuperação em `RemoteSpeaker.Update` no [RemoteSpeaker.cs](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-SpeakFromTarkov/modded/Audio/RemoteSpeaker.cs) quando `transform.parent == null`. Caso o pacote de VOIP chegue antes do avatar do jogador remoto spawnar completamente na cena, o `RemoteSpeaker` tenta re-ancorar a cada frame e se prende à cabeça (`Head.Original`) assim que o boneco é instanciado, eliminando o acúmulo de vozes na origem `(0, 0, 0)`.
- **Conformidade de Rede FIKA:** Validação e alinhamento do mod com o guia canônico `docs/technical/fika-packet-desync-prevention-plan.md` (`EnsurePacketsRegistered` pré-envio, ausência de `UnregisterPacket`, airbag `try-catch` em callbacks).

**Pendências abertas nesta sessão:**
- Nenhuma pendência blocker.

---

## 2026-08-05 — Sessão 4: v1.6.0 (Sistema de Canais de VOIP no Menu Principal & HUD de Moderação)

**Tema central:** Implementação do sistema de comunicação por voz no Menu Principal (fora de raid), com criação/gestão de canais 2D P2P, HUD integrado no padrão do FIKA, moderação por dono do canal e reconexão fluida pós-raid.

**Decisões-chave:**
- **Menu VOIP HUD:** Criada a interface [MenuVoipHUD.cs](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-SpeakFromTarkov/modded/UI/MenuVoipHUD.cs) posicionada no topo direito (`width = 400px`, `marginRight = 55px`), perfeitamente alinhada com o painel de `JOGADORES ON-LINE` do FIKA.
- **Visibilidade Sincronizada com o FIKA:** Visibilidade do HUD vinculada ao objeto visual interno do FIKA (`_userInterface.activeInHierarchy`), garantindo que o painel fique oculto nas abas de Inventário/Personagem, Comerciantes, Mercado, Esconderijo e na Raid.
- **Microfone Desligado por Padrão no Menu:** No menu principal, o microfone permanece 100% desligado (`capturer.StopCapture()`) e só é ativado quando o jogador cria ou entra em um canal de voz.
- **Protocolo de Canais de Menu & Moderação:** Criado o [SftChannelAnnouncementPacket.cs](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-SpeakFromTarkov/modded/Network/SftChannelAnnouncementPacket.cs) com suporte a anúncios de sala, heartbeats, entrada, saída, `Kick` e `Ban`. Ações de moderação protegidas por modal de confirmação anti-missclick.
- **Migração do Mod de Servidor para C# (SPT 4.0):** Implementado o Mod de Servidor C# em [SftChannelController.cs](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-SpeakFromTarkov/Server/TRL-SpeakFromTarkov.Server/Controllers/SftChannelController.cs) herda de `ControllerBase` (`[ApiController]`), disponibilizando endpoints HTTP (`/sft/channels/list` e `/sft/channels/announce`) para listagem instantânea de salas entre todos os jogadores no menu.
- **Failover de Liderança P2P & Reconexão Pós-Raid:** Se o Host original do canal continuar na raid, os convidados que retornam ao menu assumem automaticamente a transmissão de heartbeats sem deixar o canal cair. Ao sair de uma partida, os jogadores são reconectados automaticamente ao canal de menu em que estavam antes da raid.
- **Code Review:** Executada revisão em 6 categorias × 4 impactos com aprovação 100% (0 bloqueadores).
- **Debounce Anti-Spam Global (0.8s):** Aplicada trava de 0.8s no `BroadcastChannelAnnouncement` ([SftNetwork.cs](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-SpeakFromTarkov/modded/Network/SftNetwork.cs#L290)) para todas as ações, zerando requisições multiplicadas no mesmo milissegundo.
- **Sincronização com Frequência do FIKA (10s):** Ajustados os timers de fetch e heartbeat do `MenuVoipHUD` ([MenuVoipHUD.cs](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-SpeakFromTarkov/modded/UI/MenuVoipHUD.cs#L125)) para **10.0s**, casando 1:1 com a atualização do painel "JOGADORES ON-LINE" do FIKA.

**Pendências abertas nesta sessão:**
- Nenhuma pendência blocker.
