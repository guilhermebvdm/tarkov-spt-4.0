---
title: "Relatório de Auditoria Técnica de Código — FIKA (Review 02: Player Replication & Movement)"
date: 2026-09-02
status: 🟢 Vivo
authors: Antigravity
---

# Relatório de Auditoria Técnica de Código — FIKA (Review 02: Player Replication & Movement)

## 1. Resumo Executivo da Auditoria

Este relatório consolida o diagnóstico estático aprofundado e minucioso da **Partição 2 (Replicação de Jogadores, Entidades Observadas, Interpolação Temporal, Animações e Movimento)** do código original do mod **FIKA**, inspecionando ~8.000 linhas de código C# distribuídas nos módulos `Fika.Core/Main/Players/`, `ObservedClasses/` e `Networking/Snapshotting/`.

| Severidade | Quantidade | Descrição |
|---|:---:|---|
| 🔴 **Crítico** | 2 | Vazamento de memória e bindings reativos em `FikaPlayer.OnDestroy()` e vazamento permanente de instâncias `BetterSource` (AudioSource) na destruição de `ObservedPlayer`. |
| 🟠 **Alto** | 1 | Potencial divisão por zero (`motion / deltaTime`) em `ObservedMovementContext` gerando floats `NaN`/`Infinity` na física do `CharacterController`. |
| 🟡 **Médio** | 3 | Acesso não defensivo a `Singleton<IFikaNetworkManager>.Instance` em `ObservedPlayer.OnDestroy()` (AP-02), churn de GC em `ObservedFirearmController.GetOperationFactoryDelegates()` e retenção de delegates estáticos em `FikaPlayer`. |
| 💡 **Otimização** | 1 | Cálculos redundantes de interpolação em `ManualStateUpdate()` para entidades fora do campo de visão (*culled*). |

---

## 2. Tabela de Achados

| ID | Severidade | Arquivo / Linha | Categoria | Descrição Resumida |
|---|:---:|---|---|---|
| `AUD-02-01` | 🔴 Crítico | [`FikaPlayer.cs:L1586-1595`](../../original/Fika-Plugin/Fika.Core/Main/Players/FikaPlayer.cs#L1586-L1595) | Memory Leak | Falta de desinscrição de bindings de armadura (`_armorUnsubcribes`), limpeza de callbacks e buffers de snapshot no teardown de `FikaPlayer`. |
| `AUD-02-02` | 🔴 Crítico | [`ObservedPlayer.cs:L390, L1775-1810`](../../original/Fika-Plugin/Fika.Core/Main/Players/ObservedPlayer.cs#L1775-L1810) | Resource Leak | Fontes de áudio `VoipEftSource` (`BetterSource`) criadas no `BetterAudio` nunca são liberadas nem destruídas no `OnDestroy()`. |
| `AUD-02-03` | 🟠 Alto | [`ObservedMovementContext.cs:L65, L91`](../../original/Fika-Plugin/Fika.Core/Main/ObservedClasses/ObservedMovementContext.cs#L65) | Bug Latente | Divisão desprotegida por `deltaTime` (`motion / deltaTime`) podendo injetar `NaN` no `CharacterController`. |
| `AUD-02-04` | 🟡 Médio | [`ObservedPlayer.cs:L1805-1808`](../../original/Fika-Plugin/Fika.Core/Main/Players/ObservedPlayer.cs#L1805-L1808) | AP-02 (Defensiva) | Acesso direto a `Singleton<IFikaNetworkManager>.Instance` sem validação de `.Instantiated`, podendo interromper o `OnDestroy()`. |
| `AUD-02-05` | 🟡 Médio | [`ObservedFirearmController.cs:L79-90`](../../original/Fika-Plugin/Fika.Core/Main/ObservedClasses/HandsControllers/ObservedFirearmController.cs#L79-L90) | GC Pressure | `GetOperationFactoryDelegates()` aloca novo `Dictionary` e múltiplos delegates no Heap a cada invocação. |
| `AUD-02-06` | 🟡 Médio | [`FikaPlayer.cs:L147-159`](../../original/Fika-Plugin/Fika.Core/Main/Players/FikaPlayer.cs#L147-L159) | AP-01 (Teardown) | Delegates estáticos `OnPlayerSpawned`, `OnPlayerDestroyed`, etc., sem limpeza de referências entre sessões. |
| `AUD-02-07` | 💡 Otimização | [`ObservedPlayer.cs:L897-965`](../../original/Fika-Plugin/Fika.Core/Main/Players/ObservedPlayer.cs#L897-L965) | Desempenho | Execução de dezenas de `LerpUnclamped` em `ManualStateUpdate()` antes de checar se a entidade está culled (`!_cullingHandler.IsVisible`). |

---

## 3. Detalhamento dos Achados

### AUD-02-01 · Memory Leak no Teardown do Jogador Base (`FikaPlayer.OnDestroy`)
- **Severidade:** 🔴 Crítico
- **Localização:** [`FikaPlayer.cs:L1586-1595`](../../original/Fika-Plugin/Fika.Core/Main/Players/FikaPlayer.cs#L1586-L1595)
- **Referência Cruzada:** [`docs/technical/spt-antipatterns.md:AP-01`](../../../docs/technical/spt-antipatterns.md)
- **Causa Raiz:** O método `FikaPlayer.OnDestroy()` apenas limpa `CommonPacket`, dispara `OnPlayerDestroyed` e chama `base.OnDestroy()`. As coleções `OperationCallbacks`, `_proceedCallbacks`, o buffer `Snapshotter`, a lista `_preAllocatedArmorComponents` e as ações de desinscrição em `_armorUnsubcribes` (que só são invocadas no método `OnDead`) **não são executadas** quando o jogador extrai vivo ou a sessão é encerrada.
- **Impacto Técnico Real:** Inscrições reativas no inventário do EFT permanecem vivas no Heap apontando para o jogador destruído, retendo perfis e objetos do GameWorld.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - *Abordagem Atual:*
    ```csharp
    public override void OnDestroy()
    {
        if (IsAI || IsYourPlayer)
        {
            CommonPacket?.Clear();
            CommonPacket = null;
        }
        OnPlayerDestroyed?.Invoke(this);
        base.OnDestroy();
    }
    ```
  - *Abordagem Otimizada:*
    ```csharp
    public override void OnDestroy()
    {
        if (_armorUnsubcribes != null)
        {
            foreach (var unsubcribe in _armorUnsubcribes)
            {
                unsubcribe?.Invoke();
            }
            Array.Clear(_armorUnsubcribes, 0, _armorUnsubcribes.Length);
        }

        OperationCallbacks?.Clear();
        _proceedCallbacks?.Clear();
        _preAllocatedArmorComponents?.Clear();
        Snapshotter?.Clear();

        if (IsAI || IsYourPlayer)
        {
            CommonPacket?.Clear();
            CommonPacket = null;
        }
        OnPlayerDestroyed?.Invoke(this);
        base.OnDestroy();
    }
    ```
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-02-02 · Leak de AudioSource em `ObservedPlayer.OnDestroy` (`VoipEftSource`)
- **Severidade:** 🔴 Crítico
- **Localização:** [`ObservedPlayer.cs:L390, L1775-1810`](../../original/Fika-Plugin/Fika.Core/Main/Players/ObservedPlayer.cs#L1775-L1810)
- **Causa Raiz:** Em `ObservedPlayer.cs:390`, a fonte de áudio `VoipEftSource` é instanciada via `MonoBehaviourSingleton<BetterAudio>.Instance.CreateBetterSource<SimpleSource>(...)` e registrada no `SpatialAudioSystem`. No entanto, no método `OnDestroy()`, `VoipEftSource` nunca é liberado de volta para a pool do `BetterAudio` nem destruído (`Destroy(VoipEftSource.gameObject)` / `ReleaseSource`).
- **Impacto Técnico Real:** Cada operador remoto ou bot spawnado e despawnado durante a raid vaza permanentemente um componente `AudioSource` e `BetterSource` nativo do Unity na cena, degradando o desempenho do mixer de áudio e acumulando memória.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - No `ObservedPlayer.OnDestroy()`, liberar `VoipEftSource`:
    ```csharp
    if (VoipEftSource != null)
    {
        if (MonoBehaviourSingleton<BetterAudio>.Instantiated)
        {
            MonoBehaviourSingleton<BetterAudio>.Instance.ReleaseSource(VoipEftSource);
        }
        else
        {
            Destroy(VoipEftSource.gameObject);
        }
        VoipEftSource = null;
    }
    ```
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-02-03 · Divisão por Zero e Injeção de `NaN` em `ObservedMovementContext`
- **Severidade:** 🟠 Alto
- **Localização:** [`ObservedMovementContext.cs:L65, L91`](../../original/Fika-Plugin/Fika.Core/Main/ObservedClasses/ObservedMovementContext.cs#L65)
- **Causa Raiz:** Os métodos `DirectApplyMotion` e `LimitMotionXZ` executam divisões diretas por `deltaTime` (`InputMotion = motion / deltaTime;` e `InputMotionBeforeLimit = motion / deltaTime;`) sem checar se `deltaTime > 0`.
- **Impacto Técnico Real:** Em frames onde `deltaTime` seja zero ou extremamente próximo de zero (como em pausas de renderização ou transição), a divisão produz valores `NaN` ou `Infinity` nos vetores de movimento, corrompendo as matrizes de transformação do `CharacterController` da Unity e causando teletransporte ou invisibilidade do operador.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Substituir por:
    ```csharp
    InputMotion = deltaTime > 1E-05f ? motion / deltaTime : Vector3.zero;
    ```
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-02-04 · Acesso Não Defensivo a `IFikaNetworkManager` no Teardown
- **Severidade:** 🟡 Médio
- **Localização:** [`ObservedPlayer.cs:L1805-1808`](../../original/Fika-Plugin/Fika.Core/Main/Players/ObservedPlayer.cs#L1805-L1808)
- **Referência Cruzada:** [`docs/technical/spt-antipatterns.md:AP-02`](../../../docs/technical/spt-antipatterns.md)
- **Causa Raiz:** O bloco de remoção do jogador observado invoca `Singleton<IFikaNetworkManager>.Instance.ObservedPlayers.Remove(this)` diretamente. Se o network manager já tiver sido liberado na ordem de destruição de GameObjects, `Singleton<IFikaNetworkManager>.Instance` lança `NullReferenceException`.
- **Impacto Técnico Real:** A exceção interrompe a execução do método `OnDestroy()`, impedindo que `base.OnDestroy()` seja chamado e abortando a limpeza padrão de componentes da BSG.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Usar checagem defensiva:
    ```csharp
    if (Singleton<IFikaNetworkManager>.Instantiated && Singleton<IFikaNetworkManager>.Instance?.ObservedPlayers != null)
    {
        Singleton<IFikaNetworkManager>.Instance.ObservedPlayers.Remove(this);
    }
    ```
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-02-05 · Churn de GC em `GetOperationFactoryDelegates`
- **Severidade:** 🟡 Médio
- **Localização:** [`ObservedFirearmController.cs:L79-90`](../../original/Fika-Plugin/Fika.Core/Main/ObservedClasses/HandsControllers/ObservedFirearmController.cs#L79-L90)
- **Causa Raiz:** `GetOperationFactoryDelegates()` chama `base.GetOperationFactoryDelegates()`, que instancia um novo `Dictionary<Type, OperationFactoryDelegate>`, seguido por 5 novas instanciações de delegates `new OperationFactoryDelegate(...)` a cada chamada.
- **Impacto Técnico Real:** Alocações frequentes no Heap toda vez que o controlador de arma precisa resolver fábricas de operações de animação.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Cachear o dicionário de delegados ou pré-inicializar os delegates estaticamente.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-02-06 · Retenção de Delegates Estáticos em `FikaPlayer`
- **Severidade:** 🟡 Médio
- **Localização:** [`FikaPlayer.cs:L147-159`](../../original/Fika-Plugin/Fika.Core/Main/Players/FikaPlayer.cs#L147-L159)
- **Referência Cruzada:** [`docs/technical/spt-antipatterns.md:AP-01`](../../../docs/technical/spt-antipatterns.md)
- **Causa Raiz:** As propriedades estáticas `OnPlayerSpawned`, `OnPlayerDestroyed`, `OnPlayerDeath` e `OnPlayerDownedChanged` mantêm referências para delegates de mods terceiros que podem não desinscrever explicitamente.
- **Impacto Técnico Real:** Risco de retenção de instâncias e execução de callbacks órfãos após transições de raid.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Fornecer método de reset estático seguro invocado no ciclo de limpeza global de sessão (`Teardown`).
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-02-07 · Interpolação Redundante para Entidades Culled
- **Severidade:** 💡 Otimização
- **Localização:** [`ObservedPlayer.cs:L897-965`](../../original/Fika-Plugin/Fika.Core/Main/Players/ObservedPlayer.cs#L897-L965)
- **Causa Raiz:** `ManualStateUpdate()` calcula todos os `Mathf.LerpUnclamped` de rotação de cabeça, pose level, inclinação (tilt), velocidade e sobreposição de arma antes de verificar `if (!_cullingHandler.IsVisible)`.
- **Impacto Técnico Real:** Ciclos de CPU desperdiçados calculando interpolação de micro-estados cosméticos para operadores e bots que estão fora do campo de visão da câmera.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Para entidades onde `!_cullingHandler.IsVisible`, calcular apenas posição, rotação primária e velocidade, postergando interpolações detalhadas de ossos e mira para quando a entidade estiver visível.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

## 4. Salvaguarda de Contratos Públicos e Compatibilidade com Mods Terceiros

Para assegurar 100% de compatibilidade com mods de terceiros (*Speak From Tarkov*, *SAIN*, *Dynamic Maps*, *Questing Bots*), os seguintes campos, propriedades e métodos públicos foram validados e **devem permanecer originais e intactos**:

| Símbolo Público | Consumidores Externos | Diretriz Estrita |
|---|---|---|
| `FikaPlayer.IsAI` / `IsObservedAI` | *Speak From Tarkov*, *SAIN* | Preservar visibilidade pública e tipo booleano. |
| `FikaPlayer.NetId` / `ProfileId` | *Speak From Tarkov*, *Dynamic Maps* | Preservar assinaturas de identificação de rede. |
| `FikaPlayer.HandsController` | *Speak From Tarkov*, *SAIN* | Preservar acesso ao controlador de mãos. |
| `FikaPlayer.MovementContext` | *SAIN*, *Dynamic Maps* | Preservar herança e métodos de contexto de movimento. |
| `FikaPlayer.HealthController` | *Dynamic Maps*, *HUD mods* | Preservar propriedades de saúde e status corporal. |
| `FikaPlayer.Downed` | *HUD mods*, *Revival* | Preservar evento `OnPlayerDownedChanged` e flag de estado. |

---

## 5. Validação Automática

```bash
bash .agents/hooks/validate-doc-header.sh mods/FIKA/docs/original/relatorio-auditoria-codigo-02.md
```
