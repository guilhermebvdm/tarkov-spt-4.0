# Roadmap — TRL-SpeakFromTarkov

> Lista de funcionalidades futuras planejadas além do MVP inicial (portabilidade para SPT 4).

## 1. Canais de Comunicação & Regras de Acesso
- **Menu/Lobby:** Jogadores no menu do jogo podem se comunicar (canal global da party).
- **Raid-Vivo:** Comunicação posicional 3D restrita a jogadores vivos dentro da mesma raid.
- **Raid-Morto (Spectator):** Canal exclusivo para os jogadores mortos na raid.
  - *Regra de Permissão:* Espectadores (mortos) conversam entre si e conseguem ouvir os vivos. Os jogadores vivos **JAMAIS** conseguem ouvir os mortos.
- **Efeito Espectral no Canal de Espectadores:** Adicionar um sutil efeito de áudio (reverb espectral suave) para identificar o canal dos mortos, sem prejudicar a inteligibilidade da comunicação. *(Necessário testar em áudio real para garantir que fique natural)*.

## 2. Alcance Dinâmico & Validação de Calibração (Sussurro / Normal / Grito)
- **Alcance Dinâmico por Volume (`VoiceLevel`):** *(Já implementado no código)* O mod calcula o volume do microfone em tempo real e ajusta a distância:
  - **Sussurro:** ~10m de alcance máximo.
  - **Voz Normal:** ~30m de alcance padrão.
  - **Grito:** Até 60m de alcance máximo.
- **Validação & Debug de Calibração:** Adicionar indicadores no painel de Profiler (`F9`) para testar se o microfone está calibrando com precisão os limiares entre sussurro, voz normal e grito.

## 3. Imersão por Equipamento do Personagem
- **Efeito de Voz em Máscaras de Gás e Balaclavas (*Equipment Voice Muffle*):** Detectar se o jogador está usando máscaras (GP-5, Respirador, Balaclava pesada) e aplicar filtro abafador de frequência (LPF) na transmissão do microfone, fazendo a voz soar abafada de verdade dentro da máscara.
- **Reverberação Interna Instantânea da Própria Voz (*Gas Mask / Helmet Self-Reverb*):** Ao falar usando Máscara de Gás ou Capacete Fechado com Visor (Altyn/Maska), alimentar uma linha de retorno local de ultrabaixa latência (1ms a 5ms, utilizando o `SelfSpeechReverb` do Tarkov ou filtro Comb local) para que o próprio jogador escute o eco/ressonância interna da sua fala reverberando dentro do capacete em tempo real.
- **Isolamento Acústico de Capacetes Fechados (Altyn / Maska):** Validação prévia no `Assembly-CSharp` / `VoipMixer`. Como o Tarkov utiliza o `SpatialLowPassFilter` nativo no canal de VOIP, validar se capacetes fechados já abafam o VOIP nativamente antes de criar qualquer código adicional.
- **Efeito de Dor Grave e Agonia na Voz:** Avaliar e testar se injetar micro-tremores ou alterações de tom quando o jogador estiver com HP crítico (<25%) ou sangramento pesado entrega um resultado realista sem soar artificial.

## 4. Interação com IA (Bots & SAIN)
- **Detecção de Voz pelos Bots (Reatividade Posicional 3D):** Fazer com que o som do microfone do jogador chame a atenção dos bots próximos no mundo 3D.
- **Gatilho de Voz Silencioso para Bots (`EPhraseTrigger`):** Ao falar alto no microfone (`IsTransmitting == true`), o mod dispara `Player.Speaker.Play(EPhraseTrigger.OnMutter)` silenciado para ouvidos humanos (volume 0% local), mas captado a 100% pelo `BotReceiver.cs` da IA.
- **Diálogo Dinâmico (Bots Respondendo o Jogador):** Ao captar a voz no mundo 3D, o bot vira na direção do jogador e **responde verbalmente em 3D** (ex: Scavs gritando *"Cheki Breki!"*, *"Opachki!"* ou *"Who's there?!"*).

## 5. Walkie-Talkie / Rádio & Servidor no Menu
- **Objeto Equipável:** Necessário ter um item "Walkie-Talkie" no inventário/slot específico para se comunicar à distância.
- **Efeitos de Áudio:** Adição de chiados de rádio, estática e efeitos de *squelch* (abertura e fechamento de transmissão).
- **HUD P2P de Rádio no Menu (Independente do FIKA):** Interface no menu do jogo para abrir uma transmissão P2P enquanto arruma o Stash.
- **Requisito do Hideout (Integração RPG):** Acesso ao rádio no menu vinculado à construção do **Centro de Inteligência (Intelligence Center)** no Hideout.

## 6. Otimizações & Interface Visual (HUD)
- **Decodificação Multithread:** Deslocar o `OpusDecoder.Decode` para *ThreadPool* paralela.
- **HUD Minimalista de Gameplay:** Além do painel completo de debug (`F9`), criar um indicador visual sutil e discreto no canto da tela (pequeno ícone de microfone/alto-falante + sigla do canal e modo PTT/VAD ativo) para usar em partidas normais.
- **Controle Individual de Volume:** Slider no menu F12 para ajustar o volume de cada parceiro da party individualmente.

## 7. Otimizações de Rede, Desempenho e Coexistência (LiteNetLib & GC)
- **Otimização de Serialização e Redução de Overhead UDP (`SftAudioPacket.cs`)**:
  - *Diagnóstico:* O pacote `SftAudioPacket` serializa uma `string ProfileId` (GUID de ~36 caracteres) 50 vezes por segundo para cada jogador falando, gerando overhead no tamanho do pacote e alocação de memória no Garbage Collector (GC).
  - *Detalhamento de Implementação:*
    - Criar um mapeamento de handshake no início da raid (`SftHandshakePacket`) que vincula o `ProfileId (string)` do jogador a um `PlayerNetId (byte)` único (0 a 255).
    - No `SftAudioPacket`, substituir `public string ProfileId` por `public byte PlayerNetId`.
    - Economia: Redução de ~35 bytes por pacote e eliminação total da alocação/deserialização de strings no loop de alta frequência.
- **Zero-Allocation no Transmissor de Áudio Opus (`VoipProcessor.cs`)**:
  - *Diagnóstico:* Na função `Transmit()`, a chamada `byte[] finalData = new byte[len]; Array.Copy(opusBuffer, finalData, len);` aloca um novo `byte[]` na memória a cada 20ms de áudio.
  - *Detalhamento de Implementação:*
    - Substituir a alocação direta pelo uso do pool de arrays do .NET (`System.Buffers.ArrayPool<byte>.Shared.Rent(len)`) ou por uma estrutura de buffer rotativo prealocado.
    - Atualizar o evento `OnOpusDataEncoded` para passar `ArraySegment<byte>` ou devolver o buffer ao pool assim que o `SendData` do LiteNetLib concluir o envio.
    - Benefício: Eliminação da pressão sobre o Garbage Collector da Unity no envio de voz.
- **Agrupamento Dinâmico de Frames de Áudio (Frame Sizing / Packing)**:
  - *Diagnóstico:* O mod transmite quadros de 20ms (50 pacotes UDP por segundo). Em conexões P2P instáveis ou com perdas de pacotes, 50 pps pode ser excessivo para o LiteNetLib do FIKA.
  - *Detalhamento de Implementação:*
    - Adicionar suporte configurável no F12 para quadros de **40ms** (25 pacotes/s) ou **60ms** (~16 pacotes/s).
    - Como a biblioteca Concentus (Opus) suporta nativamente pacotes de 40ms/60ms alterando `frameSize = sampleRate * (frameMs / 1000)`, ajustar o `MicrophoneCapturer` para acumular a quantidade exata de amostras antes do enquadramento.
    - Benefício: Redução de até 68% no volume total de pacotes por segundo na rede sem perda perceptível de clareza de áudio.

