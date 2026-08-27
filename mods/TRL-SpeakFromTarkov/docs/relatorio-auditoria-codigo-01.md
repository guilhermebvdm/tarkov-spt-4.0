---
title: "Relatório de Auditoria Técnica de Código — TRL-SpeakFromTarkov (Review 01)"
date: 2026-08-27
status: 🟢 Vivo
authors: Antigravity
---

# Relatório de Auditoria Técnica de Código — TRL-SpeakFromTarkov (Review 01)

Auditoria técnica estática profunda e minuciosa realizada na versão `1.5.1` (`modded-V3-audit`) do mod **TRL-SpeakFromTarkov**. A auditoria cobriu 16 arquivos C# em todas as 6 dimensões críticas: validação cruzada contra referências (`Assembly-CSharp`, `references/eft-decompiled`, `references/fika-plugin`), análise de ciclos `Update()` vs arquitetura reativa, alocação e pressão de Garbage Collector (GC), código morto/funções órfãs, conformidade com antipadrões do SPT e segurança de concorrência e threading na Unity.

---

## 1. Resumo Executivo da Auditoria

| Severidade | Quantidade | Descrição |
|---|---|---|
| 🔴 **Crítico** | 0 | Falhas que causam crash instantâneo, corrupção de save ou desync total de rede |
| 🟠 **Alto** | 2 | Funcionalidade anunciada não executada (AGC órfão) e patch mirando método inexistente no FIKA |
| 🟡 **Médio** | 5 | Varredura com `FindObjectsOfType`, alocações LINQ em loops, falta de unbind em eventos, fallback de câmera no coop |
| 🔵 **Baixo** | 3 | Variáveis declaradas sem uso com pragma suprimido, falta de OnDisable no bloqueador de input, task sem cancelamento |
| 💡 **Otimização** | 2 | Cache de busca de HUD no menu e substituição de alocações de array Opus por reciclagem |

---

## 2. Tabela de Achados

| ID | Severidade | Arquivo / Linha | Categoria | Descrição Resumida |
|---|---|---|---|---|
| `AUD-01-01` | 🟠 Alto | [`GameSessionPatcher.cs:L144-L162`](../modded-V3-audit/GameSessionPatcher.cs#L144-L162) | Referência Cruzada | Patch `FikaVoipControllerUpdatePatch` aponta para método `Update` inexistente em `FikaVOIPController` |
| `AUD-01-02` | 🟠 Alto | [`Audio/AudioFilter.cs:L261-L284`](../modded-V3-audit/Audio/AudioFilter.cs#L261-L284) | Código Morto | Método `ApplyAGC()` nunca é chamado no pipeline `Apply()`, tornando a opção F12 inoperante |
| `AUD-01-03` | 🟡 Médio | [`UI/PlayerVolumeMixerHUD.cs:L132`](../modded-V3-audit/UI/PlayerVolumeMixerHUD.cs#L132) | Performance & Unity | Uso de `FindObjectsOfType<RemoteSpeaker>()` ao alterar volume em vez de acessar dicionário do `SftNetwork` |
| `AUD-01-04` | 🟡 Médio | [`Audio/RemoteSpeaker.cs:L283-L290`](../modded-V3-audit/Audio/RemoteSpeaker.cs#L283-L290) | FIKA Coop / Listener | Dependência direta de `Camera.main` para cálculo acústico 3D sem fallback para `MainPlayer` do espectador |
| `AUD-01-05` | 🟡 Médio | [`Audio/RemoteSpeaker.cs:L246`](../modded-V3-audit/Audio/RemoteSpeaker.cs#L246) | GC Pressure | Uso de LINQ com delegate em loop periódico de re-ancoragem de áudio |
| `AUD-01-06` | 🟡 Médio | [`VOIPPlugin.cs:L339`](../modded-V3-audit/VOIPPlugin.cs#L339) | Antipadrões (AP-01) | Subscrição em `SceneManager.sceneLoaded` sem cancelamento correspondente no teardown |
| `AUD-01-07` | 🟡 Médio | [`UI/MenuVoipHUD.cs:L517-L542`](../modded-V3-audit/UI/MenuVoipHUD.cs#L517-L542) | Update & UI | Varredura de hierarquia de `Transform` a cada chamada de `OnGUI` para checar visibilidade do HUD FIKA |
| `AUD-01-08` | 🔵 Baixo | [`Audio/AudioFilter.cs:L59-L63`](../modded-V3-audit/Audio/AudioFilter.cs#L59-L63) | Variáveis Órfãs | Campos `_rnGateGain`, `_rnGateHoldTimer`, `_rnGateOpen` e `dspConfirmed` sem uso |
| `AUD-01-09` | 🔵 Baixo | [`UI/VoiceCalibrationHUD.cs:L106-L113`](../modded-V3-audit/UI/VoiceCalibrationHUD.cs#L106-L113) | Integridade de Input | Falta de garantia `OnDisable` para restaurar input do jogo se HUD for desativada |
| `AUD-01-10` | 🔵 Baixo | [`UI/MenuVoipHUD.cs:L151-L193`](../modded-V3-audit/UI/MenuVoipHUD.cs#L151-L193) | Concorrência | `Task.Run` com requisições HTTP sem `CancellationToken` ao descarregar cena |

---

## 3. Detalhamento dos Achados

### AUD-01-01 · ✅ Aplicado em 2026-08-27: Remoção de patch inativo `FikaVoipControllerUpdatePatch`
- **Severidade:** 🟠 Alto
- **Localização no Mod:** [`GameSessionPatcher.cs:L144-L162`](../modded-V3-audit/GameSessionPatcher.cs#L144-L162)
- **Referência Cruzada:** [`references/fika-plugin/Fika.Core/Networking/VOIP/FikaVOIPController.cs`](../../../references/fika-plugin/Fika.Core/Networking/VOIP/FikaVOIPController.cs)
- **Causa Raiz:** O patch tentava interceptar `Fika.Core.Networking.VOIP.FikaVOIPController.Update`. No entanto, na arquitetura do FIKA (0.16.9 / SPT 4.0), `FikaVOIPController` é uma classe C# pura que implementa `IPlayerVoipController` e não herda de `MonoBehaviour`, não contendo nenhum método `Update()`.
- **Impacto Técnico Real:** `AccessTools.Method(type, "Update")` retornava `null`, gerando uma tentativa de patch inválida no Harmony.
- **Resolução:** Classe de patch removida de `GameSessionPatcher.cs` e chamada removida do `Awake()` em `VOIPPlugin.cs`.
- **Decisão:**
  - `[ ]` Pendente
  - `[x]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-01-02 · ✅ Aplicado em 2026-08-27: Ativação do `ApplyAGC()` inteligente em canais 2D
- **Severidade:** 🟠 Alto
- **Localização no Mod:** [`Audio/AudioFilter.cs:L140-L156`](../modded-V3-audit/Audio/AudioFilter.cs#L140-L156) e [`Audio/AudioFilter.cs:L261-L284`](../modded-V3-audit/Audio/AudioFilter.cs#L261-L284)
- **Referência Cruzada:** [`VOIPPlugin.cs:L58`](../modded-V3-audit/VOIPPlugin.cs#L58) (`EnableAGC`)
- **Causa Raiz:** O método `private void ApplyAGC(float[] buf)` foi implementado, mas a invocação estava ausente no `Apply(buffer)`.
- **Resolução:** Invocação `if (EnableAGC && Is2DChannel) ApplyAGC(buffer);` adicionada antes do limiter em `AudioFilter.cs`, atuando exclusivamente em canais 2D para preservar a dinâmica acústica do 3D.
- **Decisão:**
  - `[ ]` Pendente
  - `[x]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-01-03 · ✅ Aplicado em 2026-08-27: Indexação O(1) de speakers no PlayerVolumeMixerHUD
- **Severidade:** 🟡 Médio
- **Localização no Mod:** [`UI/PlayerVolumeMixerHUD.cs:L132-L140`](../modded-V3-audit/UI/PlayerVolumeMixerHUD.cs#L132-L140)
- **Referência Cruzada:** [`Network/SftNetwork.cs:L30`](../modded-V3-audit/Network/SftNetwork.cs#L30) (`remoteSpeakers`)
- **Causa Raiz:** Ao mover o slider de volume, `FindObjectsOfType<RemoteSpeaker>()` percorria toda a hierarquia de cena.
- **Resolução:** Criado `SftNetwork.Instance.GetRemoteSpeaker(profileId)` e substituída a busca em `PlayerVolumeMixerHUD.cs`.
- **Decisão:**
  - `[ ]` Pendente
  - `[x]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-01-04 · ✅ Aplicado em 2026-08-27: Modo 2D Estéreo Global para Canal de Espectador (Mortos)
- **Severidade:** 🟡 Médio
- **Localização no Mod:** [`Audio/RemoteSpeaker.cs:L283-L290`](../modded-V3-audit/Audio/RemoteSpeaker.cs#L283-L290) e [`Network/SftNetwork.cs:L420-L450`](../modded-V3-audit/Network/SftNetwork.cs#L420-L450)
- **Referência Cruzada:** [`references/fika-plugin/Fika.Core`](../../../references/fika-plugin/Fika.Core)
- **Causa Raiz:** Pacotes do Canal 2 (Mortos) eram forçados para modo 3D e ancorados a cadáveres, atenuando com a distância da câmera do espectador.
- **Resolução:** Pacotes de Canal 2 agora configuram o `RemoteSpeaker` em modo 2D puro (`SetEmergency2DMode(true)`), desvinculados de cadáveres, com escuta dupla permitida para espectadores (Canal 2 em 2D + Canal 0 em 3D).
- **Decisão:**
  - `[ ]` Pendente
  - `[x]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-01-05 · ✅ Aplicado em 2026-08-27: Eliminação de LINQ em rotinas periódicas (Zero-Alloc)
- **Severidade:** 🟡 Médio
- **Localização no Mod:** [`Audio/RemoteSpeaker.cs:L246-L248`](../modded-V3-audit/Audio/RemoteSpeaker.cs#L246-L248) e [`Network/SftNetwork.cs`](../modded-V3-audit/Network/SftNetwork.cs)
- **Referência Cruzada:** [AP-03 / GC Pressure](../../../docs/technical/spt-antipatterns.md)
- **Causa Raiz:** `FirstOrDefault` com lambda capturando variáveis alocava closures no Heap a cada execução.
- **Resolução:** Substituído por laços `for` indexados sem alocação em `RemoteSpeaker.cs` e `SftNetwork.cs`.
- **Decisão:**
  - `[ ]` Pendente
  - `[x]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-01-06 · ✅ Aplicado em 2026-08-27: Unbind defensivo de eventos no plugin (AP-01)
- **Severidade:** 🟡 Médio
- **Localização no Mod:** [`VOIPPlugin.cs:L339`](../modded-V3-audit/VOIPPlugin.cs#L339) e [`VOIPPlugin.cs:L386`](../modded-V3-audit/VOIPPlugin.cs#L386)
- **Referência Cruzada:** [AP-01 — Falta de teardown](../../../docs/technical/spt-antipatterns.md)
- **Causa Raiz:** `SceneManager.sceneLoaded` inscrito sem cancelamento no `OnDestroy`.
- **Resolução:** `SceneManager.sceneLoaded -= OnSceneLoaded;` adicionado ao `OnDestroy()` de `VOIPPlugin.cs`.
- **Decisão:**
  - `[ ]` Pendente
  - `[x]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-01-07 · Varredura repetitiva de Transforms no `OnGUI` de `MenuVoipHUD`
- **Severidade:** 🟡 Médio
- **Localização no Mod:** [`UI/MenuVoipHUD.cs:L517-L542`](../modded-V3-audit/UI/MenuVoipHUD.cs#L517-L542)
- **Causa Raiz:** O método `IsFikaHUDVisible()` é chamado diretamente no topo de `OnGUI()`. O `OnGUI()` da Unity é executado de 2 a 4 vezes por frame para processar eventos de `Layout`, `Repaint` e input. A cada execução, ele realiza `tr.GetChild(0).GetChild(0)` e acessa propriedades da hierarquia da Unity.
- **Impacto Técnico Real:** Desperdício de tempo de CPU na main thread durante a navegação no menu principal do jogo.
- **Proposta de Correção:** Cachear o resultado de `IsFikaHUDVisible` no `Update()` com verificação a cada 0.2s, usando apenas a flag booleana em cache no `OnGUI()`. (Diferido para etapa isolada de menu).
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[x]` Rejeitar (deferir / aceitar como dívida): Diferido para etapa isolada do Menu

---

### AUD-01-08 · ✅ Aplicado em 2026-08-27: Limpeza de variáveis órfãs e pragmas CS0414
- **Severidade:** 🔵 Baixo
- **Localização no Mod:** [`Audio/AudioFilter.cs:L59-L63`](../modded-V3-audit/Audio/AudioFilter.cs#L59-L63) e [`Audio/MicrophoneCapturer.cs:L42-L44`](../modded-V3-audit/Audio/MicrophoneCapturer.cs#L42-L44)
- **Causa Raiz:** Campos declarados sem uso com pragmas de supressão.
- **Resolução:** Campos `_rnGateGain`, `_rnGateHoldTimer`, `_rnGateOpen`, `hasPlayed`, `dspConfirmed` e seus pragmas foram removidos.
- **Decisão:**
  - `[ ]` Pendente
  - `[x]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-01-09 · ✅ Aplicado em 2026-08-27: OnDisable defensivo no Wizard de Calibração
- **Severidade:** 🔵 Baixo
- **Localização no Mod:** [`UI/VoiceCalibrationHUD.cs:L102-L138`](../modded-V3-audit/UI/VoiceCalibrationHUD.cs#L102-L138)
- **Causa Raiz:** Possibilidade de input do jogo permanecer bloqueado caso o componente seja desativado enquanto aberto.
- **Resolução:** `OnDisable()` implementado chamando `CloseWizard()` para garantir desbloqueio do input do Tarkov.
- **Decisão:**
  - `[ ]` Pendente
  - `[x]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-01-10 · Requisições HTTP em `Task.Run` sem cancelamento em `MenuVoipHUD`
- **Severidade:** 🔵 Baixo
- **Localização no Mod:** [`UI/MenuVoipHUD.cs:L151-L194`](../modded-V3-audit/UI/MenuVoipHUD.cs#L151-L194)
- **Causa Raiz:** A consulta periódica de canais no servidor SPT (`/sft/channels/list`) roda em `Task.Run` a cada 10 segundos sem amarrar um `CancellationToken` ao ciclo de vida do componente.
- **Impacto Técnico Real:** Tarefas assíncronas podem continuar rodando por alguns segundos após a destruição do componente na transição para a raid.
- **Proposta de Correção:** Utilizar um `CancellationTokenSource` que seja cancelado no `OnDestroy()`.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

## 4. Plano de Ação e Recomendações

1. **Correções Imediatas de Lógica (Itens 🟠):**
   - Ativar a chamada de `ApplyAGC(buffer)` no pipeline de `AudioFilter.cs` para garantir que o ganho automático funcione conforme configurado no menu F12.
   - Remover a declaração morta de `FikaVoipControllerUpdatePatch` em `GameSessionPatcher.cs`.

2. **Otimizações de Desempenho e GC (Itens 🟡):**
   - Substituir `FindObjectsOfType<RemoteSpeaker>` em `PlayerVolumeMixerHUD.cs` pelo acesso indexado em `SftNetwork.remoteSpeakers`.
   - Eliminar os delegates LINQ em `RemoteSpeaker.cs` e cachear a verificação de hierarquia em `MenuVoipHUD.cs`.
   - Adicionar o cancelamento de subscrição de `SceneManager.sceneLoaded` no `OnDestroy` do plugin.

3. **Polimento e Robustez (Itens 🔵):**
   - Adicionar `OnDisable()` no wizard de calibração para garantir o desbloqueio do input do Tarkov em qualquer circunstância.
   - Remover variáveis mortas e pragmas CS0414 obsoletos.
