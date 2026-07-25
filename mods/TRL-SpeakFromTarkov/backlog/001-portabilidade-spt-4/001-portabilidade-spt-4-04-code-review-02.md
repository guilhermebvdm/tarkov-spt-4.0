# 001 — portabilidade-spt-4 · Code Review 02 (Revisão Completa)

**Mod:** TRL-ImmersiveVoip
**Data:** 2026-07-16

> Revisão extensiva de todas as classes e lógicas do mod solicitada para garantir a estabilidade do MVP.

## Resumo

> 🔴 Bloqueadores: 1 · 🟠 Fortes: 3 · 🟡 Médios: 1 · 🟢 Menores: 0 

## Índice

| ID | Classe Alvo | Categoria | Impacto | Título |
| --- | --- | --- | --- | --- |
| CR-02-01 | VoiceChatManager | A — Crítico | 🔴 Bloqueador | Alocação de memória (GC) excessiva nos arrays a cada frame de Update |
| CR-02-02 | VoiceChatManager | B — Bug latente | 🟠 Forte | Glitch de aúdio a cada 1 segundo devido a quebra no buffer circular do microfone |
| CR-02-03 | VoiceChatManager | B — Bug latente | 🟠 Forte | VAD Auto-calibração não diminui, travando a HUD |
| CR-02-04 | NetworkManager | D — Arquitetura | 🟠 Forte | Reprodução de Opus via PlayOneShot gera áudio robótico/engasgado |
| CR-02-05 | NetworkManager | D — Arquitetura | 🟡 Médio | Vazamento de memória no dicionário de AudioSources |

---

## Pontos

### CR-02-01 · A — Crítico · 🔴 Bloqueador

**Alocação de memória (GC) excessiva nos arrays a cada frame**
**Local:** `VoiceChatManager.cs` (`UpdateAudioLevel` e `StartSpeaking`)
**Problema:** Você está usando `float[] samples = new float[FrameSize];` e `byte[] opusData = new byte[1275];` dentro do método `Update()`.
**Por que importa:** O `Update()` roda 60 a 144 vezes por segundo. Criar novos arrays o tempo todo enche a memória Lixo (Garbage) rapidamente, forçando a Unity a fazer uma limpeza agressiva (GC Spike), o que trava o jogo por frações de segundo (stuttering violento).
**Sugestão:** Declarar os arrays `samples` e `opusData` como variáveis privadas na classe e apenas reutilizá-los sobrescrevendo os dados dentro deles.

---

### CR-02-02 · B — Bug latente · 🟠 Forte

**Glitch de aúdio a cada 1 segundo (Buffer Wrap)**
**Local:** `VoiceChatManager.cs` (Leitura do microfone)
**Problema:** O `Microphone.Start` cria um clip de 1 segundo que fica gravando em loop circular (chega no final e volta ao 0). Quando a posição atual (`pos`) é menor que o `FrameSize` (ex: 200 < 960), a conta `Mathf.Max(0, pos - FrameSize)` dá 0. Em vez de ler os últimos 760 samples do final do clip + os 200 do começo, ele lê uma amostra velha e causa um "estalo" (glitch) robótico audível a cada 1 segundo de gravação.
**Sugestão:** Implementar uma lógica de leitura circular: se a posição for menor que o `FrameSize`, ler os dados antigos do final do AudioClip e juntar com os novos do começo.

---

### CR-02-03 · B — Bug latente · 🟠 Forte

**VAD Auto-calibração não diminui, travando a HUD**
**Local:** `VoiceChatManager.cs:143`
**Problema:** `if (rms > peakRMS) peakRMS = rms;`
**Por que importa:** O `peakRMS` sempre sobe, mas nunca cai. Se houver um espirro ou barulho forte, a HUD calibra o limite da barra lá pro alto e sua voz normal passará a preencher só 1% ou 2% da barra, fazendo parecer que o HUD "quebrou" ou travou.
**Sugestão:** Adicionar um multiplicador de decaimento: `peakRMS = Mathf.Lerp(peakRMS, 0f, Time.deltaTime * 0.1f);` para que o teto da HUD volte ao normal aos poucos.

---

### CR-02-04 · D — Arquitetura · 🟠 Forte

**Reprodução de Opus via PlayOneShot gera áudio engasgado**
**Local:** `NetworkManager.cs:93`
**Problema:** Cada pacote que chega (20ms de áudio) invoca `source.PlayOneShot(clip)`.
**Por que importa:** A Unity não foi feita para enfileirar milhares de clipes de 20 milissegundos sobrepostos. Eles vão perder a sincronia do sample-rate da placa de som, resultando num som mastigado, picotado ou com cliques e *poppings*.
**Sugestão:** A forma correta para VOIP na Unity é anexar um script com `OnAudioFilterRead(float[] data, int channels)` no `AudioSource`. O pacote de rede chega, joga o float decodificado dentro de um "Buffer/Fila", e o `OnAudioFilterRead` consome esse buffer de forma fluidamente colada à placa de áudio. *(Nota: Pode ser implementado numa V2 caso o PlayOneShot se prove aceitável no MVP).*

---

### CR-02-05 · D — Arquitetura · 🟡 Médio

**Vazamento de memória no dicionário de AudioSources**
**Local:** `NetworkManager.cs` (`audioSources`)
**Problema:** Os jogadores são adicionados ao `Dictionary<string, AudioSource>`, mas quando eles extraem da raid ou se desconectam, nunca são removidos.
**Por que importa:** O dicionário vai engordar até o `Cleanup()`. Em raids curtas não faz mal, mas é uma falha de design arquitetural.
**Sugestão:** Escutar o evento de desconexão de jogadores do Fika (ou checar de tempos em tempos se os AudioSources do dicionário ainda estão associados a um player vivo no `GameWorld`) e destrui-los.
