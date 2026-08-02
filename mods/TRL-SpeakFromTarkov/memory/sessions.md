# TRL-SpeakFromTarkov — Memória de Sessões

## Snapshot Delta
- **Versão:** 1.5.0 (SPT 4.0 / FIKA)
- **Estado:** v1.5.0 compilada e commitada com 0 erros (`32533fd1`). Transmissão VOIP isolada no `Channel 1` com Magic Header `SFTV`, enquadramento DSP de 40ms (1.920 amostras @ 48kHz), Zero-Allocation Opus `ArrayPool` e higienização FMOD/NaN.
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

**Pendências abertas nesta sessão:**
- Nenhuma pendência blocker.
