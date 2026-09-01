---
title: "Relatório de Auditoria Técnica de Código — TRL-Fixes (Review 01)"
date: 2026-08-27
status: 🟢 Vivo
authors: Antigravity
---

# Relatório de Auditoria Técnica de Código — TRL-Fixes (Review 01)

Este documento apresenta a **auditoria técnica estática profunda e minuciosa** de todos os patches e componentes do mod **TRL-Fixes** (escopo de código: [modded-V2-audit/](../modded-V2-audit/)), cruzando as referências do **EFT 0.16.9.1** (`references/eft-decompiled/`), **SPT 4.0** (`references/spt-source/`) e **FIKA Coop** (`references/fika-plugin/`).

---

## 1. Resumo Executivo da Auditoria

| Severidade | Quantidade | Descrição |
| :--- | :---: | :--- |
| 🔴 **Crítico** | 0 | Falhas graves, crashes iminentes ou corrupção de save |
| 🟠 **Alto** | 1 | Reflection sem cache em hot path de IA (AP-04 / GC Pressure massivo) |
| 🟡 **Médio** | 2 | Ausência de null-checks em ossos de players e duplicação de chamadas de animação |
| 🔵 **Baixo** | 3 | Fragilidade de tipos por string, fallback inoperante e inconsistência de logger |
| 💡 **Otimização** | 1 | Eliminação de boxing de enums em pacotes de rede e caching de LayerMasks |
| **Total** | **7** | **Oportunidades de refatoração identificadas e resolvidas** |

---

## 2. Tabela de Achados

| ID | Severidade | Arquivo / Linha | Categoria | Descrição Resumida | Status |
| :--- | :---: | :--- | :--- | :--- | :---: |
| `AUD-01-01` | 🟠 Alto | [FlashbangBotPatch.cs:L39-L50](../modded-V2-audit/Patches/FlashbangBotPatch.cs#L39-L50) | AP-04 / GC Pressure | Reflection sem cache em `SAINActivationClass.ManualUpdate` executada a cada frame por bot. | ✅ Aplicado |
| `AUD-01-02` | 🟡 Médio | [FlashbangRadiusPatch.cs:L51-L67](../modded-V2-audit/Patches/FlashbangRadiusPatch.cs#L51-L67) | Null Safety | Acesso a `player.PlayerBones.Head` e `botOwner.Settings.FileSettings` sem null-check defensivo. | ✅ Aplicado |
| `AUD-01-03` | 🟡 Médio | [BotMountWeaponFixPatch.cs:L149-L170](../modded-V2-audit/Patches/BotMountWeaponFixPatch.cs#L149-L170) | FSM / Redundância | `PlayerOperateStationaryWeaponPatch` executa setup e retorna `true`, duplicando animações. | ✅ Aplicado |
| `AUD-01-04` | 🔵 Baixo | [DynamicMapsSafetyPatch.cs:L34-L44](../modded-V2-audit/Patches/DynamicMapsSafetyPatch.cs#L34-L44) | AP-04 / Resiliência | Fallback para `GameWorldOnDestroyPatch` é inoperante para capturar NRE em `OnRaidEnd`. | ✅ Aplicado |
| `AUD-01-05` | 🔵 Baixo | [FikaMainThreadUISafetyPatch.cs:L32-L37](../modded-V2-audit/Patches/FikaMainThreadUISafetyPatch.cs#L32-L37) | Type Safety | Resolução de `PreloaderUI` por string em vez de `typeof(PreloaderUI)`. | ✅ Aplicado |
| `AUD-01-06` | 💡 Otimização | [FikaProceedEmptyHandsSafetyPatch.cs:L150](../modded-V2-audit/Patches/FikaProceedEmptyHandsSafetyPatch.cs#L150) | Zero-Alloc | Conversão `Enum.ToObject` e `new object[]` a cada pacote de mãos vazias. | ✅ Aplicado |
| `AUD-01-07` | 🔵 Baixo | [Múltiplos Patches](../modded-V2-audit/Patches/) | Logging Standard | Uso de `UnityEngine.Debug` em vez de `Plugin.Log` em 5 patches, perdendo tags BepInEx. | ✅ Aplicado |

---

## 3. Detalhamento dos Achados

---

### AUD-01-01 · Reflection sem Cache em Hot Path de Atualização do SAIN
- **Severidade:** 🟠 Alto
- **Evidência:** Forte
- **Localização no Mod:** [FlashbangBotPatch.cs:L39-L50](../modded-V2-audit/Patches/FlashbangBotPatch.cs#L39-L50)
- **Referência Cruzada:** [AP-04 (docs/technical/spt-antipatterns.md)](../../../docs/technical/spt-antipatterns.md)
- **Causa Raiz:** O método `Prefix` de `FlashbangBotPatch` intercepta `SAINActivationClass.ManualUpdate`, que é invocado **a cada frame para cada bot ativo no mapa** (20 a 40 bots simultâneos). Dentro do `Prefix`, o código executava `AccessTools.Property(__instance.GetType(), "BotOwner")` e `AccessTools.Method(__instance.GetType(), "SetActive")` dinamicamente **em todas as iterações**, além de alocar `new object[] { false }` para invocar o método.
- **Impacto Técnico Real:** Degradação contínua de ciclos de CPU em lookups de reflexão, alocação excessiva de memória temporária no Heap e pressão sobre o Garbage Collector (GC Spikes / micro-travamentos).
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - *Abordagem Otimizada:* Cachear estaticamente `PropertyInfo _botOwnerProp`, `MethodInfo _setActiveMethod` e o array de argumentos `_inactiveArgs = new object[] { false }` uma única vez no método `Enable()`.
- **Decisão:**
  - `[ ]` Pendente
  - `[x]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________
- **Resolução:** ✅ Aplicado em 2026-08-27 (v1.3.1).
- **Aplicação:** [FlashbangBotPatch.cs:L13-L58](../modded-V2-audit/Patches/FlashbangBotPatch.cs#L13-L58).

---

### AUD-01-02 · Ausência de Validação Defensiva em `player.PlayerBones.Head`
- **Severidade:** 🟡 Médio
- **Evidência:** Forte
- **Localização no Mod:** [FlashbangRadiusPatch.cs:L51-L67](../modded-V2-audit/Patches/FlashbangRadiusPatch.cs#L51-L67)
- **Referência Cruzada:** [GameWorld.cs:556](../../../references/eft-decompiled/Assembly-CSharp/EFT/GameWorld.cs#L556)
- **Causa Raiz:** Ao iterar sobre `gameWorld.AllAlivePlayersList`, o código acessava diretamente `player.PlayerBones.Head.position` e `botOwner.Settings.FileSettings.Grenade.FLASH_GRENADE_TIME_COEF` sem checar se `PlayerBones`, `Head` ou `FileSettings` são nulos. Se um bot estivesse em transição de inicialização ou morte, o acesso a `.position` disparava `NullReferenceException`, abortando o loop antes de processar os demais bots atingidos pela flashbang.
- **Impacto Técnico Real:** Interrupção do efeito de flashbang para os outros bots na raid caso um dos jogadores da lista estivesse com ossos descarregados.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Inserir verificações nulas explícitas antes de ler coordenadas e coeficientes.
- **Decisão:**
  - `[ ]` Pendente
  - `[x]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________
- **Resolução:** ✅ Aplicado em 2026-08-27 (v1.3.1).
- **Aplicação:** [FlashbangRadiusPatch.cs:L47-L70](../modded-V2-audit/Patches/FlashbangRadiusPatch.cs#L47-L70).

---

### AUD-01-03 · Duplicação de Configuração de Animação em `PlayerOperateStationaryWeaponPatch`
- **Severidade:** 🟡 Médio
- **Evidência:** Forte
- **Localização no Mod:** [BotMountWeaponFixPatch.cs:L149-L170](../modded-V2-audit/Patches/BotMountWeaponFixPatch.cs#L149-L170)
- **Referência Cruzada:** [Player.cs:26124](../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L26124)
- **Causa Raiz:** No patch `PlayerOperateStationaryWeaponPatch`, o `Prefix` executava `stationaryWeapon.SetOperator`, configurava `MovementContext.StationaryWeapon`, definia parâmetros e animações do jogador e em seguida retornava `true`. Como o método original `EFT.Player.OperateStationaryWeapon` era executado logo em seguida, o jogo executava as mesmas atribuições e comandos de animação uma segunda vez no mesmo frame.
- **Impacto Técnico Real:** Execução redundante de rotinas de animação e lógica de montagem.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Retornar `false` quando o comando for `Occupy`, alinhando o comportamento ao que já é feito no `FikaPlayerOperateStationaryWeaponPatch`.
- **Decisão:**
  - `[ ]` Pendente
  - `[x]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________
- **Resolução:** ✅ Aplicado em 2026-08-27 (v1.3.1).
- **Aplicação:** [BotMountWeaponFixPatch.cs:L166](../modded-V2-audit/Patches/BotMountWeaponFixPatch.cs#L166).

---

### AUD-01-04 · Fallback Inoperante em `DynamicMapsSafetyPatch`
- **Severidade:** 🔵 Baixo
- **Evidência:** Forte
- **Localização no Mod:** [DynamicMapsSafetyPatch.cs:L34-L44](../modded-V2-audit/Patches/DynamicMapsSafetyPatch.cs#L34-L44)
- **Referência Cruzada:** [Memória de Sessão 4 (CR-01-04)](../memory/sessions.md#L69)
- **Causa Raiz:** Se `DynamicMaps.UI.ModdedMapScreen` não fosse encontrado, o código tentava aplicar o finalizer em `DynamicMaps.Patches.GameWorldOnDestroyPatch.PatchPrefix`. Contudo, um finalizer no chamador não captura exceções geradas dentro de métodos chamados internamente por ele se a exceção já tiver desestabilizado o fluxo anterior.
- **Impacto Técnico Real:** Falsa expectativa de proteção caso a estrutura do DynamicMaps mudasse em versões futuras.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Remover o fallback ineficaz e emitir um aviso informativo claro desativando o patch graciosamente.
- **Decisão:**
  - `[ ]` Pendente
  - `[x]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________
- **Resolução:** ✅ Aplicado em 2026-08-27 (v1.3.1).
- **Aplicação:** [DynamicMapsSafetyPatch.cs:L20-L33](../modded-V2-audit/Patches/DynamicMapsSafetyPatch.cs#L20-L33).

---

### AUD-01-05 · Resolução por String de Tipo Referenciado em `FikaMainThreadUISafetyPatch`
- **Severidade:** 🔵 Baixo
- **Evidência:** Forte
- **Localização no Mod:** [FikaMainThreadUISafetyPatch.cs:L32-L37](../modded-V2-audit/Patches/FikaMainThreadUISafetyPatch.cs#L32-L37)
- **Referência Cruzada:** [Memória de Sessão 4 (CR-01-05)](../memory/sessions.md#L70)
- **Causa Raiz:** O tipo `EFT.UI.PreloaderUI` era resolvido em runtime via `AccessTools.TypeByName("EFT.UI.PreloaderUI")`, embora esteja disponível em tempo de compilação no assembly `Assembly-CSharp` já referenciado pelo projeto.
- **Impacto Técnico Real:** Perda de verificação de tipos em tempo de compilação e custo desnecessário de busca de tipos.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Utilizar diretamente `typeof(EFT.UI.PreloaderUI)`.
- **Decisão:**
  - `[ ]` Pendente
  - `[x]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________
- **Resolução:** ✅ Aplicado em 2026-08-27 (v1.3.1).
- **Aplicação:** [FikaMainThreadUISafetyPatch.cs:L32](../modded-V2-audit/Patches/FikaMainThreadUISafetyPatch.cs#L32).

---

### AUD-01-06 · Otimização Zero-Alloc em `FikaProceedEmptyHandsSafetyPatch`
- **Severidade:** 💡 Otimização
- **Evidência:** Forte
- **Localização no Mod:** [FikaProceedEmptyHandsSafetyPatch.cs:L150-L153](../modded-V2-audit/Patches/FikaProceedEmptyHandsSafetyPatch.cs#L150-L153)
- **Referência Cruzada:** [FikaProceedEmptyHandsSafetyPatch.cs](../modded-V2-audit/Patches/FikaProceedEmptyHandsSafetyPatch.cs)
- **Causa Raiz:** A cada pacote de mãos vazias interceptado, o método `Prefix` executava `Enum.ToObject(_deliveryMethodEnum, ReliableOrderedDeliveryMethod)` e alocava `new object[] { response, deliveryMethodVal, peer }`.
- **Impacto Técnico Real:** Alocação de memória temporária e boxing desnecessário de enums a cada troca de arma ou recarga fora do inventário.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Converter e armazenar o valor do enum `ReliableOrdered` em uma variável estática `_cachedDeliveryMethodVal` durante o `Enable()`.
- **Decisão:**
  - `[ ]` Pendente
  - `[x]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________
- **Resolução:** ✅ Aplicado em 2026-08-27 (v1.3.1).
- **Aplicação:** [FikaProceedEmptyHandsSafetyPatch.cs:L55-L151](../modded-V2-audit/Patches/FikaProceedEmptyHandsSafetyPatch.cs#L55-L151).

---

### AUD-01-07 · Padronização de Logging BepInEx (`Plugin.Log`)
- **Severidade:** 🔵 Baixo
- **Evidência:** Forte
- **Localização no Mod:** [BotWeaponManagerSafetyPatch.cs](../modded-V2-audit/Patches/BotWeaponManagerSafetyPatch.cs), [FixFikaReviveRagdollPatch.cs](../modded-V2-audit/Patches/FixFikaReviveRagdollPatch.cs), [FlashbangBotPatch.cs](../modded-V2-audit/Patches/FlashbangBotPatch.cs), [FlashbangRadiusPatch.cs](../modded-V2-audit/Patches/FlashbangRadiusPatch.cs), [PickupAimingSafetyPatch.cs](../modded-V2-audit/Patches/PickupAimingSafetyPatch.cs)
- **Referência Cruzada:** [Plugin.cs:16](../modded-V2-audit/Plugin.cs#L16)
- **Causa Raiz:** Vários patches utilizavam `UnityEngine.Debug.Log / LogWarning / LogError` diretamente em vez de `Plugin.Log?.LogInfo / LogWarning / LogError`.
- **Impacto Técnico Real:** Mensagens enviadas via `UnityEngine.Debug` não recebiam a tag de cabeçalho `[Info : TRL Fixes]` no console BepInEx e no arquivo `BepInEx/LogOutput.log`, dificultando a depuração e filtragem de logs por mod.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Substituir todas as chamadas de `Debug.Log*` por `Plugin.Log?.Log*`.
- **Decisão:**
  - `[ ]` Pendente
  - `[x]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________
- **Resolução:** ✅ Aplicado em 2026-08-27 (v1.3.1).
- **Aplicação:** Em todos os 5 arquivos de patch citados.

---

## 4. Plano de Ação e Recomendações

1. **Correções de Performance e Robustez:** Todos os achados de `AUD-01-01` a `AUD-01-07` foram aplicados na base `modded-V2-audit/`.
2. **Build e Teste:** Validado via compilação Release `1.3.1.0`.

---

**Memória consultada:** [mods/TRL-Fixes/memory/sessions.md](../memory/sessions.md) (Sessões 1 a 8 revisadas, sem pendências blockers ativas).
