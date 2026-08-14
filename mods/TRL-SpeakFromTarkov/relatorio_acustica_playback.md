# 🔊 Relatório Técnico de Engenharia de Áudio — Reprodução & Acústica Ambiental

**Autor:** Engenheiro de Áudio Sênior / Lead Audio DSP Engineer  
**Projeto:** TRL-SpeakFromTarkov (SPT 4.0 / Tarkov Red Line / FIKA Coop)  
**Foco:** Resiliência de Jitter Buffer, Curva Físico-Acústica (-6dB), Panning de Potência Constante, Absorção Atmosférica e Oclusão

---

## 1. 🌐 RESILIÊNCIA DE REDE (JITTER BUFFER, CLOCK DRIFT & PLC)

### Jitter Buffer Adaptativo Dinâmico
- **Buffering Inicial:** Configurado via `VoIPPlugin.NetworkJitterBufferMs` (padrão **150ms** para VOIP 3D de proximidade; **100ms** para 2D no Menu).
- **Target de Recuperação:** Na ocorrência de uma falha de pacotes (*underrun*), o buffer alterna temporariamente para um alvo de recuperação reduzido (`jitterRecoverySamples = 50ms`), retomando a fala rapidamente sem acumular latência perceptível.

### Prevenção de Clock Drift & Overrun
- Se o relógio de transmissão do *Sender* for levemente mais rápido que o do *Receiver*, amostras se acumulam no Ring Buffer de 3 segundos (`streamBuffer`).
- **Mecanismo de Descarte:** O `RemoteSpeaker.cs` impõe um atraso máximo permitido (`maxAllowedDelay = jitterInitialSamples * 2`). Se o áudio acumulado superar 300ms, o ponteiro de leitura `streamReadPos` avança automaticamente para descarta o excesso antigo, restaurando o atraso para 150ms instantaneamente.

### Mitigação de Underrun (Fade-Out de 1ms sem Chiado)
- Quando o buffer seca durante a fala (`shouldPlay && rPos == wPos`), em vez de cortar o áudio para zero instantaneamente (o que produziria um estalo de estática por descontinuidade DC), a última amostra sofre um decaimento suave exponencial:
  ```csharp
  lastSample *= 0.95f; // Fade-out ultraleve de ~1ms
  ```

---

## 2. 🧱 OCLUSÃO E ABAFAMENTO AMBIENTAL (AIR DAMPING & GEOMETRIA)

### Absorção Atmosférica do Ar (Filtro Single-Pole LPF)
- O ar absorve frequências agudas mais rápido que graves conforme a distância aumenta.
- No `OnAudioFilterRead`, o mod calcula um coeficiente de amortecimento de ar (`airDampingAlpha`):
  - **De perto (< 2m):** `airDampingAlpha = 1.0f` (voz com brilho total e clareza).
  - **De longe (30m):** `airDampingAlpha = 0.15f` (agudos naturalmente abafados pelo ar).
- O filtro IIR de polo único é aplicado em tempo real sample a sample:
  ```csharp
  lpfState = lpfState + airDampingAlpha * (targetSample - lpfState);
  ```

### Oclusão por Geometria do Mapa (Oportunidade de Evolução)
- Atualmente, o `AudioSource` ignora o mixer padrão da Unity (`bypassEffects = true`) para prevenir que o motor do Tarkov multe o VOIP.
- **Proposta de Evolução:** Adicionar uma verificação periódica de Raycast físico (`Physics.Linecast` ou `SphereCast`) a cada ~200ms entre a câmera do jogador local e a cabeça do emissor remoto contra a camada de colisão do mapa (`HighPolyWithRaycast`). Se houver parede/teto no caminho, reduz-se o `airDampingAlpha` para `0.08f` e aplica-se uma atenuação de **-6dB a -12dB**, simulando a voz abafada através de concreto.

---

## 3. 🏛️ REVERBERAÇÃO E ECO (AMBIENTES E DISTÂNCIA)

### Estado Atual
- As `AudioReverbZone` nativas da Unity estão pausadas para o VOIP (`bypassReverbZones = true`) para evitar realimentação excessiva em espaços pequenos.
- Teste de Eco Local: Implementado com sucesso no painel F12 via `EnableEcho`, `EchoDelay` (em segundos) e `EchoVolume`.

---

## 4. 📐 CURVA DE ATENUAÇÃO FÍSICO-ACÚSTICA & PANNING ESTÉREO

### Curva de Atenuação Física (-6dB por Dobra de Distância)
- O volume do som no mundo real cai exponencialmente seguindo a lei do inverso do quadrado ($-\text{6dB}$ a cada dobra de distância).
- No `RemoteSpeaker.cs`, a distância normalizada ($\text{normD}$) é convertida por um expoente físico $2.2$:
  $$\text{distanceAttenuation} = (1.0 - \text{normD})^{2.2}$$
  - **1.5m:** 100% de volume
  - **5.0m:** ~70% de volume
  - **12.0m:** ~40% de volume
  - **20.0m:** ~12% de volume
  - **30.0m:** 0% (Silêncio absoluto configurável).

### Panning Estéreo de Potência Constante (-3dB Pan Law)
- O mod calcula a direção relativa do som no plano XZ em relação à câmera do jogador (`Camera.main.transform.right`):
  $$\text{pan} = \vec{\text{dirToSpeaker}} \cdot \vec{\text{listenerRight}} \quad \in [-1.0, 1.0]$$
- Aplica-se a Lei de Panning de Potência Constante ($-\text{3dB}$ Pan Law):
  $$\text{angle} = (\text{pan} + 1.0) \times \frac{\pi}{4}$$
  $$\text{gain}_{\text{left}} = \cos(\text{angle}), \quad \text{gain}_{\text{right}} = \sin(\text{angle})$$
- **Efeito:** Ao girar a cabeça do personagem, a energia total perceptível do som no fone de ouvido permanece **100% constante**, permitindo localização espacial exata (360°) de onde o colega ou inimigo está falando.
