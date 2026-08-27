# Code Review 12 · Item 009 — Auditoria Técnica V3, Áudio 2D/3D e Otimizações Zero-Alloc

**Mod:** `TRL-SpeakFromTarkov`  
**Item:** `009-otimizacoes-arquiteturais-v2`  
**Data:** 2026-08-27  
**Fase Revisada:** Implementação e Refatoração das Rodadas de Auditoria Review 01 e Review 02 (Versão `1.5.3` / `modded-V3-audit`)

---

## 1. 📊 Resumo da Análise (6 Categorias × 4 Impactos)

| Categoria | Bloqueador 🔴 | Forte 🟠 | Médio 🟡 | Menor 🟢 | Total |
| :--- | :---: | :---: | :---: | :---: | :---: |
| **A — Crítico (Bug grave / Crash)** | 0 | 0 | 0 | 0 | **0** |
| **B — Bug Latente** | 0 | 0 | 0 | 0 | **0** |
| **C — Gap vs. Spec** | 0 | 0 | 0 | 0 | **0** |
| **D — Arquitetura / Padrões (AP-01..09)** | 0 | 0 | 0 | 0 | **0** |
| **E — Legibilidade / Manutenção** | 0 | 0 | 0 | 0 | **0** |
| **F — Melhoria Opcional** | 0 | 0 | 0 | 0 | **0** |
| **TOTAL** | **0** | **0** | **0** | **0** | **0** |

**Status Geral:** 🟢 **APROVADO — 0 Bloqueadores Pendentes (100% Clean Build).**

---

## 2. 🔍 Detalhamento das Implementações Validadas

### CR-12-01 · Cat B — Bug Latente / Áudio Posicional · 🔴 Bloqueador (Resolvido)
**Canal de Espectador / Mortos 2D Global Puro com Escuta Dupla**  
**Local:** [`Network/SftNetwork.cs:L415-L455`](../../modded-V3-audit/Network/SftNetwork.cs#L415-L455)
- **Problema Anterior:** Pacotes no Canal 2 (Mortos) eram forçados para modo 3D e ficavam ancorados fisicamente ao ragdoll/cadáver no chão. Ao assistir um amigo vivo que se afastava dos corpos, a voz dos mortos atenuava e sumia. Além disso, o filtro de canal impedia o espectador de escutar o Canal 0 dos vivos.
- **Implementação:** Pacotes de Canal 2 configuram o `RemoteSpeaker` em modo 2D estéreo global puro (`SetEmergency2DMode(true)`), desvinculados de qualquer Transform/cadáver (`transform.SetParent(null)`). Adicionado suporte a escuta dupla para espectadores (`myChannel == 2 && channel == 0`).
- **Por que importa:** Mortos conversam entre si com clareza cristalina e sem atenuação espacial 3D, enquanto continuam ouvindo a partida ao redor da câmera do amigo assistido.
- **Decisão:** `[x]` Aceito e aplicado.

---

### CR-12-02 · Cat D — Arquitetura DSP / Dinâmica Acústica · 🟠 Forte (Resolvido)
**Ativação do AGC Inteligente Restrita a Canais 2D**  
**Local:** [`Audio/AudioFilter.cs:L150-L156`](../../modded-V3-audit/Audio/AudioFilter.cs#L150-L156) e [`Audio/MicrophoneCapturer.cs:L250`](../../modded-V3-audit/Audio/MicrophoneCapturer.cs#L250)
- **Problema Anterior:** O método `ApplyAGC()` estava órfão no pipeline e nunca era chamado. A ativação global em raid distorceria a dinâmica natural de proximidade 3D (sussurros amplificados indevidamente).
- **Implementação:** Sincronizada a flag `Is2DChannel` com o canal ativo no microfone e invocada `if (EnableAGC && Is2DChannel) ApplyAGC(buffer);` antes do limiter.
- **Por que importa:** Nivelamento automático atua em canais 2D (menu e espectador) sem prejudicar a percepção espacial de distância do Canal 0.
- **Decisão:** `[x]` Aceito e aplicado.

---

### CR-12-03 · Cat D — Performance & DSP · 🟡 Médio (Resolvido)
**Indexação O(1) de Speakers e Soft-Limiter no Mixer de Volume**  
**Local:** [`Network/SftNetwork.cs:L494-L500`](../../modded-V3-audit/Network/SftNetwork.cs#L494-L500), [`UI/PlayerVolumeMixerHUD.cs:L126-L137`](../../modded-V3-audit/UI/PlayerVolumeMixerHUD.cs#L126-L137) e [`Audio/RemoteSpeaker.cs:L463-L467`](../../modded-V3-audit/Audio/RemoteSpeaker.cs#L463-L467)
- **Problema Anterior:** Ajustar o volume varria toda a cena com `FindObjectsOfType<RemoteSpeaker>()`. Volumes acima de 100% no mod legado zeravam o volume por divisão inteira indevida.
- **Implementação:** Criado `GetRemoteSpeaker(profileId)` indexado no dicionário e adicionado clamp suave (`Mathf.Clamp(targetSample, -0.98f, 0.98f)`) suportando ganhos de 0% a 200% sem estouro de buffer no FMOD.
- **Por que importa:** Zero hitches ao movimentar sliders in-raid e suporte seguro a amplificação de volume individual.
- **Decisão:** `[x]` Aceito e aplicado.

---

### CR-12-04 · Cat D — Antipadrões do SPT (AP-01 & AP-03) · 🟡 Médio (Resolvido)
**Teardown de Eventos, Limpeza de Patches e Zero-Alloc LINQ**  
**Local:** [`VOIPPlugin.cs:L384-L388`](../../modded-V3-audit/VOIPPlugin.cs#L384-L388), [`GameSessionPatcher.cs`](../../modded-V3-audit/GameSessionPatcher.cs) e [`Audio/RemoteSpeaker.cs:L243-L255`](../../modded-V3-audit/Audio/RemoteSpeaker.cs#L243-L255)
- **Implementação:**
  - `SceneManager.sceneLoaded -= OnSceneLoaded` implementado no `OnDestroy()` de `VOIPPlugin.cs` (AP-01).
  - Removido o patch inativo `FikaVoipControllerUpdatePatch`.
  - Substituído `FirstOrDefault` com lambda por laços `for` indexados em `RemoteSpeaker.cs` e `SftNetwork.cs` (AP-03).
  - Adicionado `OnDisable()` em `VoiceCalibrationHUD.cs` para liberação garantida do input do Tarkov.
- **Decisão:** `[x]` Aceito e aplicado.

---

### CR-12-05 · Cat D — GC Pressure & GPU Churn · 🟡 Médio (Resolvido)
**Eliminação de Alocações no OnGUI e Guard In-Raid no Retry de Microfone**  
**Local:** [`UI/InRaidVoipHUD.cs:L364-L375`](../../modded-V3-audit/UI/InRaidVoipHUD.cs#L364-L375), [`UI/VoipHUD.cs`](../../modded-V3-audit/UI/VoipHUD.cs) e [`Core/VoipController.cs:L288-L304`](../../modded-V3-audit/Core/VoipController.cs#L288-L304)
- **Implementação:**
  - Erradicadas chamadas a `MakeTex(color)` / `Destroy(fillTex)` em `OnGUI()`, consumindo texturas 1x1 estáticas pré-alocadas (`_greenTex`, `_yellowTex`, `_redTex`, `_grayTex`).
  - Cacheados todos os estilos `GUIStyle` em campos privados de classe.
  - Adicionado guard `if (!capturer.IsRecording && Singleton<EFT.GameWorld>.Instantiated)` no retry de microfone, mantendo-o desligado no menu principal.
- **Decisão:** `[x]` Aceito e aplicado.

---

## 3. 🎯 Validação de Build e Conformidade

- **Compilador:** `dotnet build -c Release`
- **Diagnóstico:** **0 Erro(s)** · **0 Aviso(s)** (100% Clean Build).
- **SemVer:** `1.5.3` (sincronizada em `VOIPPlugin.cs` e `TRL-SpeakFromTarkov.csproj`).
- **Isolamento de Build:** Binários mantidos exclusivamente dentro do repositório (`mods/TRL-SpeakFromTarkov/modded-V3-audit/bin/Release/`).

---

## 4. 📝 Conclusão e Próximos Passos

A versão `1.5.3` de **TRL-SpeakFromTarkov** consolida todas as exigências das duas auditorias técnicas estáticas com aprovação total de arquitetura, estabilidade acústica e desempenho.

**Próximo Passo:**
- Implementação e isolamento do transporte de canais de voz no Menu Principal (relay HTTP/WebSocket desacoplado do FIKA).
