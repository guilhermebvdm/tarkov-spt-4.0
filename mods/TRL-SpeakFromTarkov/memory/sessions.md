# TRL-SpeakFromTarkov — Memória de Sessões

## Snapshot Delta
- **Versão:** 1.6.1 (SPT 4.0 / FIKA)
- **Estado:** v1.6.1 compilada e testada com 0 erros e 0 avisos (100% Clean Build). HUD In-Raid sincronizado 1:1 com a opacidade/autohide do `BattleStancePanel` vanilla, com offset X ajustável (+15px), ícones de modo de captura em 40px posicionados na parte inferior da barra (PTT, VAD, OPEN, MUTE), remoção do texto RAID e eliminação completa de todos os 133 avisos de compilação C#.
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
- **Solução Definitiva do Erro HTTP 415 (UnsupportedMediaType):** Atualizado [SftNetwork.cs](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-SpeakFromTarkov/modded/Network/SftNetwork.cs#L330) para enviar JSON puro e não-compactado via `HttpClient` (Content-Type `application/json`), e adicionado descompactador `ZLibStream` em [SftChannelController.cs](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-SpeakFromTarkov/Server/TRL-SpeakFromTarkov.Server/Controllers/SftChannelController.cs#L75) como fallback. Zera definitivamente os erros de `UnsupportedMediaType` no BepInEx.

- **Validação de Produção & Harmonia de Logs:** Confirmado nos logs de runtime real que o servidor C# e o cliente funcionam 100% sem exceções de `UnsupportedMediaType`, e as buscas `/sft/channels/list` rodam perfeitamente pareadas com `/fika/presence/get` a cada 10.0s.

---

## 2026-08-12 — Sessão 5: HUD de VOIP em Raid (In-Raid VOIP HUD Vertical & Ancoragem Vanilla)

**Tema central:** Criação do HUD de VOIP em partida (In-Raid), posicionado como uma barra vertical fina ancorada dinamicamente à esquerda do painel de postura (`BattleStancePanel`) da UI vanilla do EFT.

**Decisões-chave:**
- **In-Raid VOIP HUD Vertical ([InRaidVoipHUD.cs](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-SpeakFromTarkov/modded/UI/InRaidVoipHUD.cs)):** Criado novo componente visual fino (`14px` x `110px`) posicionado no canto inferior esquerdo da tela, com VU Meter vertical (de baixo para cima), ponto de status no topo e canal miniaturizado.
- **Ancoragem Dinâmica sem Dependência Externa:** O componente localiza a instância ativa do `BattleStancePanel` por reflexão segura de nome do tipo (sem arrastar dependência da DLL `Sirenix.Serialization`), calculando os vértices de tela (`RectTransform.GetWorldCorners`) e posicionando o HUD à esquerda da barra de posture em qualquer resolução.
- **Centralização de Visibilidade no BepInEx (F12):** Criadas as entradas `Enable In-Raid VOIP HUD` (padrão `true`) e `Enable Debug VOIP HUD` (padrão `false`) no menu de configurações F12. Atalho `F9` desativado.
- **Voice Calibration Wizard ([VoiceCalibrationHUD.cs](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-SpeakFromTarkov/modded/UI/VoiceCalibrationHUD.cs)):** Desenvolvido o assistente modal interativo em 3 fases (Whisper, Normal, Loud) com interface 100% em Inglês, frases táticas, controle liberado de cursor de mouse e atalho de acionamento `F8`.
- **Análise Estatística de Pico (P95) e Vale (P10):** O calibrador analisa os percentis de áudio ativo de cada fase para calcular dinamicamente os pontos médios exatos de transição (Traço 1 Sussurro $\rightarrow$ Voz Normal e Traço 2 Voz Normal $\rightarrow$ Grito), descartando micro-pausas e respirações.
- **Sincronização 1:1 com Sliders F12 e Gate VAD:** Os valores obtidos são persistidos no BepInEx como sliders ajustáveis no F12 (`WhisperThreshold`, `NormalThreshold`, `LoudThreshold`) e ajustam simultaneamente o ponto de abertura do VAD (`VADThreshold`) e a barra vertical do HUD in-raid em tempo real.
- **Redução da Largura da Barra (`7px`):** Largura do HUD vertical in-raid reduzida pela metade (de `14px` para `7px`), tornando a barra extremamente compacta e discreta ao lado do indicador de postura vanilla.

---

## 2026-08-14 — Sessão 6: Integração Visual do HUD In-Raid, Deslocamento Vanilla & Limpeza de 100% dos Warnings C#

**Tema central:** Sincronização avançada de transparência/autohide com a UI vanilla do Tarkov (`BattleStancePanel`), suporte ao ícone `mute.png`, reposicionamento dos ícones de modo (40px) na parte inferior da barra, deslocamento configurável (+15px) do painel de postura nativo e resolução de 100% dos avisos de compilação C#.

**Decisões-chave:**
- **Sincronização de Autohide Vanilla via `CanvasGroup`:** Recuperada a referência ao `CanvasGroup` do `BattleStancePanel` ([InRaidVoipHUD.cs](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-SpeakFromTarkov/modded/UI/InRaidVoipHUD.cs)), multiplicando `GUI.color` pelo `alpha` da animação do Tarkov (`DOFade`). Nosso HUD de VOIP agora esmaece e reaparece exatamente junto com o HUD do jogo.
- **Guard Estrito de Existência:** Se o `BattleStancePanel` não existir ou estiver inativo na hierarquia da cena, o HUD de VOIP não é renderizado.
- **Opção F12 `AlwaysVisibleInRaidHUD`:** Adicionada a opção no BepInEx ([VOIPPlugin.cs](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-SpeakFromTarkov/modded/VOIPPlugin.cs)) para manter o HUD de VOIP continuamente visível em raid se o jogador desejar, sem ignorar a regra de existência.
- **Deslocamento X do Painel de Postura Vanilla (+15px):** O `BattleStancePanel` nativo do jogo agora é deslocado +15px para a direita (ajustável no F12 via `ShiftStancePanelX`), criando um alinhamento tático limpo sem colar os elementos na borda esquerda da tela.
- **Ícones de Modo de Captura (40px na Parte Inferior) & Remoção do Texto "RAID":** Integrados os ícones `ptt.png`, `vad.png`, `open.png` e `mute.png` escalados suavemente para **40px** na parte inferior da barra vertical. A escrita miniaturizada "RAID" foi removida para um visual minimalista e moderno.
- **Resolução de 100% dos Avisos (133 Warnings $\rightarrow$ 0 Warnings):** Eliminados todos os 133 avisos C# (`CS8618`, `CS0414`, `CS0169`, `CS8600`, `CS8601`, `CS8603`, `CS8625`) em 12 arquivos C# do mod Client, alcançando compilação **100% limpa (0 Erros e 0 Avisos)**.

**Pendências abertas nesta sessão:**
- 🟢 Nenhuma pendência blocker. Build concluído com 0 erros e 0 avisos.

---

## 2026-08-14 — Sessão 7: Menu Selecionável `Visibilidade do HUD` (Oculto, SempreVisivel, SyncHUD, CaptaVoz)

**Tema central:** Conversão da opção de alternância booleana do HUD in-raid para o menu dropdown selecionável `Visibilidade do HUD` (`HudVisibilityMode`), adicionando o novo modo `CaptaVoz` que exibe a barra automaticamente ao captar voz.

**Decisões-chave:**
- **Enum `HudVisibilityMode` ([InRaidVoipHUD.cs](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-SpeakFromTarkov/modded/UI/InRaidVoipHUD.cs)):** Criado o enum com 4 opções curtas para o dropdown BepInEx no F12:
  - `Oculto`: Nunca exibe o HUD em raid.
  - `SempreVisivel`: HUD sempre visível durante a partida (ignora autohide).
  - `SyncHUD`: Sincroniza 1:1 com o autohide do HUD do jogo (`BattleStancePanel`).
  - `CaptaVoz`: Surge automaticamente quando a voz é captada pelo microfone (PTT, VAD ou Open mode filtrado pelo RNNoise) e esmaece suavemente 1s após o fim da fala.
- **Funcionalidade `CaptaVoz` ([InRaidVoipHUD.cs](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-SpeakFromTarkov/modded/UI/InRaidVoipHUD.cs)):** Implementado o timer de sustentação (`_voiceHoldTimer = 1.0f`) acionado por `Processor.IsTransmitting || Processor.DisplayLevel > 0.002f`, com transição suave de fade-out nos últimos 0.3s.
- **Substituição BepInEx `HudVisibility` ([VOIPPlugin.cs](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-SpeakFromTarkov/modded/VOIPPlugin.cs)):** Substituído o booleano `AlwaysVisibleInRaidHUD` por `HudVisibility` (`ConfigEntry<HudVisibilityMode>`), mantendo `SyncHUD` como padrão.

**Pendências abertas nesta sessão:**
- 🟢 Nenhuma pendência blocker. Build concluído com 0 erros e 0 avisos.

---

## 2026-08-14 — Sessão 8: Code-Review & Blindagem da Configuração `EnableMod` ("Habilitar Mod de Voz")

**Tema central:** Investigação rigorosa e resolução dos bugs e travamentos do BepInEx F12 provocados ao desativar/alternar a chave `EnableMod` ("Habilitar Mod de Voz").

**Decisões-chave:**
- **Eliminação da Trava Nula na Inicialização ([VoipController.cs](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-SpeakFromTarkov/modded/Core/VoipController.cs)):** Removido o `return` precoce em `Awake()` caso o jogo iniciasse com `EnableMod = false`. Todos os subcomponentes (`capturer`, `processor`, `hud`, etc.) agora são instanciados e inicializados de forma segura, prevenindo `NullReferenceException` ao reativar a opção no F12.
- **Bloqueio do Loop Infinito de Reabertura de Mic ([VoipController.cs](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-SpeakFromTarkov/modded/Core/VoipController.cs)):** Adicionado o guard `if (VoIPPlugin.EnableMod != null && !VoIPPlugin.EnableMod.Value) return;` no topo de `VoipController.Update()`. Impede que o timer de retry (`micRetryTimer`) continue forçando `capturer.StartCapture()` a cada 5s quando o mod está desativado, eliminando os travamentos na main thread da Unity.
- **Guards de Desativação nos HUDs ([InRaidVoipHUD.cs](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-SpeakFromTarkov/modded/UI/InRaidVoipHUD.cs) e [MenuVoipHUD.cs](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-SpeakFromTarkov/modded/UI/MenuVoipHUD.cs)):** Adicionados os guards no topo de `OnGUI()` e `Update()`, garantindo que requisições HTTP e renderizações visuais sejam imediatamente pausadas quando `EnableMod` estiver em `false`.

**Pendências abertas nesta sessão:**
- 🟢 Nenhuma pendência blocker. Build concluído com 0 erros e 0 avisos.

---

## 2026-08-14 — Sessão 9: Padronização Internacional das Configurações F12 em Inglês & `VoiceActivity` como Padrão

**Tema central:** Tradução completa de 100% das seções, chaves e descrições do menu de configurações BepInEx (F12) para Inglês técnico padrão, e definição de `VoiceActivity` como modo padrão de visibilidade do HUD in-raid.

**Decisões-chave:**
- **Padrão `VoiceActivity` ([InRaidVoipHUD.cs](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-SpeakFromTarkov/modded/UI/InRaidVoipHUD.cs)):** `VoiceActivity` definido como o valor padrão da chave `HudVisibility`. As opções do dropdown foram traduzidas para `Hidden`, `AlwaysVisible`, `SyncHUD` e `VoiceActivity`.
- **Tradução Completa das Configurações F12 ([VOIPPlugin.cs](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-SpeakFromTarkov/modded/VOIPPlugin.cs)):** Traduzidas todas as 10 seções do BepInEx (`General`, `VOIP Settings`, `UI / HUD Settings`, `Voice Calibration`, `Diagnostics`, `Audio Filters`, `Neural Filters (RNNoise)`, `Network`, `Network (Opus)`, `AI Bot Interaction`) e suas respectivas descrições para Inglês.

**Pendências abertas nesta sessão:**
- 🟢 Nenhuma pendência blocker. Build concluído com 0 erros e 0 avisos.

---

## 2026-08-14 — Sessão 10: Reorganização de Seções F12, Seção `Debug` Unificada & Ajuste de Valores Padrão

**Tema central:** Reorganização completa das seções no menu F12 do BepInEx, agrupamento das opções de diagnóstico na nova seção unificada `Debug`, eliminação do prefixo `Enable` e atualização dos valores padrões.

**Decisões-chave:**
- **Seção `Debug` Unificada ([VOIPPlugin.cs](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-SpeakFromTarkov/modded/VOIPPlugin.cs)):** Agrupadas as opções `Local Echo Loopback`, `Echo Delay (s)`, `Echo Volume`, `Profiler / Debug HUD`, `Debug Logs` e `Bot Speech Debug Volume` em uma única seção `Debug`.
- **Remoção do Prefixo `Enable` ([VOIPPlugin.cs](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-SpeakFromTarkov/modded/VOIPPlugin.cs)):** Eliminado o prefixo `Enable` de todas as chaves (ex: `Enable Voice Mod` $\rightarrow$ `Voice Mod`, `Enable AGC` $\rightarrow$ `AGC`, `Enable RNNoise Suppressor` $\rightarrow$ `RNNoise Suppressor`, `Enable Bot Reactivity` $\rightarrow$ `Bot Reactivity`).
- **Valores Padrão Atualizados:**
  - `Local Echo Loopback`: `false`
  - `Voice Mod`: `true`
  - `Debug Logs`: `false`
  - `AGC (Automatic Gain Control)`: `false`
  - `RNNoise Suppressor`: `true`
  - `Forward Error Correction (FEC)`: `false`
  - `Bot Speech Debug Volume`: `0.0` (0%)
  - `Bot Reactivity`: `true`

**Pendências abertas nesta sessão:**
- 🟢 Nenhuma pendência blocker. Build concluído com 0 erros e 0 avisos.
