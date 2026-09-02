---
title: "Relatório de Auditoria Técnica de Código — FIKA (Review 06: Auxiliary Systems & HUD)"
date: 2026-09-02
status: 🟢 Vivo
authors: Antigravity
---

# Relatório de Auditoria Técnica de Código — FIKA (Review 06: Auxiliary Systems & HUD)

## 1. Resumo Executivo da Auditoria

Este relatório consolida o diagnóstico estático aprofundado e minucioso da **Partição 6 (Sistemas Auxiliares, Reanimação/Revival, Bleedout, HealthBars, NamePlates, Pings 3D, FreeCamera e Chat In-Game)** do código original do mod **FIKA**, inspecionando ~5.500 linhas de código C# distribuídas nos módulos `Fika.Core/Main/Components/`, `FreeCamera/` e `UI/Custom/`.

| Severidade | Quantidade | Descrição |
|---|:---:|---|
| 🔴 **Crítico** | 2 | `FikaChatUIScript` não remove nó do `InputTree` nem desinscreve listeners de botões no teardown; `FikaHealthBar` não implementa `OnDestroy()`, retendo estruturas de UI. |
| 🟠 **Alto** | 1 | `ReviveInteractable` não restaura animadores e layers de ragdoll ao ser destruído prematuramente por morte ou término de raid. |
| 🟡 **Médio** | 2 | Chamada redundante `Destroy(this)` dentro de `OnDestroy()` e risco de NRE em `Destroy(_freecamUI.gameObject)` no `FreeCamera.cs`; importação não utilizada de `DG.Tweening`. |
| 💡 **Otimização** | 1 | Oportunidade de desacoplar raycasts periódicos de oclusão de visão em `FikaHealthBar.CheckForOcclusion()`. |

---

## 2. Tabela de Achados

| ID | Severidade | Arquivo / Linha | Categoria | Descrição Resumida |
|---|:---:|---|---|---|
| `AUD-06-01` | 🔴 Crítico | [`FikaChatUIScript.cs:L67-74`](../../original/Fika-Plugin/Fika.Core/UI/Custom/FikaChatUIScript.cs#L67-L74) | Input / Leak | Falta de método `OnDestroy()` para remover o nó de input de `FikaGlobals.InputTree` e desinscrever eventos de botões. |
| `AUD-06-02` | 🔴 Crítico | [`FikaHealthBar.cs:L22-75`](../../original/Fika-Plugin/Fika.Core/Main/Components/FikaHealthBar.cs#L22-L75) | Memory Leak | Ausência de `OnDestroy()` em `FikaHealthBar` para limpar GameObjects de Canvas e ícones de efeitos no descarte do jogador. |
| `AUD-06-03` | 🟠 Alto | [`ReviveInteractable.cs:L49-53`](../../original/Fika-Plugin/Fika.Core/Main/Components/ReviveInteractable.cs#L49-L53) | Bug Latente | Destruição sem `RemoveRagdoll()` deixa animadores, layers e colliders do jogador em estado desativado. |
| `AUD-06-04` | 🟡 Médio | [`FreeCamera.cs:L809-810`](../../original/Fika-Plugin/Fika.Core/Main/FreeCamera/FreeCamera.cs#L809-L810) | Antipadrão / NRE | `Destroy(this)` dentro de `OnDestroy()` e descarte direto de `_freecamUI.gameObject` sem checagem de nulo. |
| `AUD-06-05` | 🔵 Baixo | [`FikaHealthBar.cs:L6`](../../original/Fika-Plugin/Fika.Core/Main/Components/FikaHealthBar.cs#L6) | Código Morto | Importação de namespace `using DG.Tweening;` sem nenhum uso ativo no arquivo. |
| `AUD-06-06` | 💡 Otimização | [`FikaHealthBar.cs:L84-92`](../../original/Fika-Plugin/Fika.Core/Main/Components/FikaHealthBar.cs#L84-L92) | Desempenho | Raycasts periódicos de oclusão de visão (`Physics.Raycast`) executados individualmente por cada jogador observado. |

---

## 3. Detalhamento dos Achados

### AUD-06-01 · InputNode Órfão no Teardown do Chat (`FikaChatUIScript`)
- **Severidade:** 🔴 Crítico
- **Localização:** [`FikaChatUIScript.cs:L67-74`](../../original/Fika-Plugin/Fika.Core/UI/Custom/FikaChatUIScript.cs#L67-L74)
- **Referência Cruzada:** [`docs/technical/spt-antipatterns.md:AP-01`](../../../docs/technical/spt-antipatterns.md)
- **Causa Raiz:** O componente `FikaChatUIScript` herda de `InputNode` e executa `FikaGlobals.InputTree.Add(this)` no método `Start()`. No entanto, a classe **não implementa `OnDestroy()`**, deixando a referência do nó presa na árvore de input global do EFT quando a cena é descarregada.
- **Impacto Técnico Real:** A árvore de input continua disparando comandos para uma instância destruída de MonoBehaviour, gerando erros silenciosos ou bloqueando comandos de teclado (Enter, Escape) nos menus após a partida.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - *Abordagem Otimizada:*
    ```csharp
    protected void OnDestroy()
    {
        if (FikaGlobals.InputTree != null)
        {
            FikaGlobals.InputTree.Remove(this);
        }

        if (_fikaChatUI != null)
        {
            _fikaChatUI.InputField?.onSubmit?.RemoveListener(OnSubmit);
            _fikaChatUI.SendButton?.onClick?.RemoveListener(SendMessage);
            _fikaChatUI.CloseButton?.onClick?.RemoveListener(CloseChat);
        }

        _chatMessages?.Clear();
        _stringBuilder?.Clear();
    }
    ```
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-06-02 · Ausência de Teardown de UI em `FikaHealthBar`
- **Severidade:** 🔴 Crítico
- **Localização:** [`FikaHealthBar.cs:L22-75`](../../original/Fika-Plugin/Fika.Core/Main/Components/FikaHealthBar.cs#L22-L75)
- **Referência Cruzada:** [`docs/technical/spt-antipatterns.md:AP-01`](../../../docs/technical/spt-antipatterns.md)
- **Causa Raiz:** `FikaHealthBar` instancia dinamicamente componentes de UI e placas de identificação sobre os operadores, mas não possui rotina de `OnDestroy()` para limpar as listas de efeitos, dicionários de partes do corpo (`_bodyParts`) e referências de renderização.
- **Impacto Técnico Real:** Retenção de referências de GameObjects de Canvas e sprites na memória após a destruição dos jogadores observados.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Implementar `OnDestroy()` com `.Clear()` em `_effects`, `_bodyParts` e destruição segura de `_playerPlate.gameObject`.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-06-03 · Estado Incompleto de Ragdoll na Destruição Prematura de `ReviveInteractable`
- **Severidade:** 🟠 Alto
- **Localização:** [`ReviveInteractable.cs:L49-53`](../../original/Fika-Plugin/Fika.Core/Main/Components/ReviveInteractable.cs#L49-L53)
- **Causa Raiz:** Durante o nocaute, `Init()` desativa animadores (`BodyAnimatorCommon`, `ArmsAnimatorCommon`) e muda as layers para Deadbody. O método `OnDestroy()` apenas cancela o áudio de agonia (`CancelInvoke(nameof(AgonySFX))`), mas não restaura os componentes de animação e colisão caso o jogador seja destruído enquanto nocauteado.
- **Impacto Técnico Real:** Se o jogador nocauteado desconectar ou a raid for finalizada, o objeto é retornado à pool com componentes desabilitados, corrompendo futuras instanciações do mesmo modelo na pool de objetos (`PlayerPoolObject`).
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Garantir a restauração de flags e animadores no `OnDestroy()` caso `RemoveRagdoll()` não tenha sido chamado.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-06-04 · `Destroy(this)` Redundante e Risco de NRE em `FreeCamera`
- **Severidade:** 🟡 Médio
- **Localização:** [`FreeCamera.cs:L809-810`](../../original/Fika-Plugin/Fika.Core/Main/FreeCamera/FreeCamera.cs#L809-L810)
- **Causa Raiz:** `FreeCamera.OnDestroy()` invoca `Destroy(this)` (chamada desnecessária e potencialmente recursiva durante o ciclo de destruição do Unity) e acessa `_freecamUI.gameObject` diretamente sem checagem de nulo.
- **Impacto Técnico Real:** Disparo de `NullReferenceException` durante o teardown caso `_freecamUI` não tenha sido instanciado.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Remover `Destroy(this)` e proteger o descarte da UI com `if (_freecamUI != null) Destroy(_freecamUI.gameObject);`.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-06-05 · Importação Não Utilizada de `DG.Tweening` em `FikaHealthBar`
- **Severidade:** 🔵 Baixo
- **Localização:** [`FikaHealthBar.cs:L6`](../../original/Fika-Plugin/Fika.Core/Main/Components/FikaHealthBar.cs#L6)
- **Causa Raiz:** O arquivo contém `using DG.Tweening;`, mas todas as interpolações são calculadas manualmente sem uso do motor DOTween.
- **Impacto Técnico Real:** Poluição de cabeçalho e importação morta.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Remover a linha `using DG.Tweening;`.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-06-06 · Otimização de Raycast de Oclusão em `FikaHealthBar`
- **Severidade:** 💡 Otimização
- **Localização:** [`FikaHealthBar.cs:L84-92`](../../original/Fika-Plugin/Fika.Core/Main/Components/FikaHealthBar.cs#L84-L92)
- **Causa Raiz:** `CheckForOcclusion()` roda `Physics.Raycast` a cada 1 segundo por operador aliado individualmente.
- **Impacto Técnico Real:** Oportunidade de intercalar ou agrupar os testes de oclusão de múltiplos aliados para suavizar o consumo de CPU.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Adicionar um deslocamento temporal aleatório na inicialização do contador (`_counter = UnityEngine.Random.Range(0f, 1f);`) para distribuir os raycasts ao longo dos frames.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

## 4. Salvaguarda de Contratos Públicos e Compatibilidade com Mods Terceiros

Para assegurar 100% de compatibilidade com mods de terceiros (*Speak From Tarkov*, *SAIN*, *Dynamic Maps*, *Amands Graphics*, *Custom HUD mods*):

| Símbolo Público | Consumidores Externos | Diretriz Estrita |
|---|---|---|
| `FikaHealthBar.Create` | *Custom UI*, *NamePlate mods* | Preservar método de criação estático e assinatura. |
| `ReviveInteractable.CanInteract` / `BeingRevived` | *HUD mods*, *Revival mods* | Preservar propriedades de interação. |
| `FreeCamera.IsActive` / `Instance` | *Spectator mods*, *Cinematic mods* | Preservar propriedades e estado da câmera. |
| `FikaChatUIScript.ChatMessages` | *Chat addons*, *Overlay mods* | Preservar lista de mensagens públicas. |

---

## 5. Validação Automática

```bash
bash .agents/hooks/validate-doc-header.sh mods/FIKA/docs/original/relatorio-auditoria-codigo-06.md
```
