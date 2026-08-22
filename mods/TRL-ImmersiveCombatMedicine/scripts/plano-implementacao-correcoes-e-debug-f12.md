# Plano de Implementação — Correções, Otimizações de GC, Limpeza Arquitetural & Sistema de Debug Logs no F12

> **Módulo:** `TRL-ImmersiveCombatMedicine`  
> **Workspace:** [`modded-testchannel`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel)  
> **Versão Alvo:** `1.13.5` (Bump SemVer Patch)  
> **Status:** 🔵 Em Planejamento (Histórico Versionado)  
> **Data:** 2026-08-15  
> **Escopo:** Aplicação de todas as melhorias e correções dos Itens 01 a 06 e 08 a 16 (O **Item 07: Torniquetes Realistas e Necrose** fica reservado para discussão dedicada posterior).  

---

## 1. Visão Geral das Alterações

Durante a auditoria sistemática das 16 funcionalidades, identificamos oportunidades de refinamento para unificar namespaces, otimizar o GC, desacoplar os logs diagnósticos por trás de toggles de Debug no menu F12 do BepInEx e padronizar toda a arquitetura sob a família `TRLImmersiveCombatMedicine`.

---

## 2. Detalhamento das Correções por Grupo

### Grupo A · Padronização de Namespaces & Limpeza de Usings
- **Objetivo:** Eliminar namespaces legados (`TrueTrauma`, `Band_Aid`) e uniformizar todo o mod sob:
  - `TRLImmersiveCombatMedicine` (Core & Plugin)
  - `TRLImmersiveCombatMedicine.Medical` (Lógica médica, UI, Handlers de rede)
  - `TRLImmersiveCombatMedicine.Trauma` (Motor 2.0, Consumidores de membros, Queda, Desmaio)
  - `TRLImmersiveCombatMedicine.Helpers` (Bancos de dados, carregadores de imagem, utilitários de IA)
  - `TRLImmersiveCombatMedicine.Fika` (Bridges e adaptadores multiplayer)
- **Arquivos:** Todos os arquivos em `Patches/Medical/`, `Patches/Trauma/`, `Helpers/` e `Fika/`.

---

### Grupo B · Otimizações de GC e Desempenho
1. **Cache de Enum em Membros (`MedicalLogic.cs`):**
   - Substituir chamadas repetidas de `Enum.GetValues(typeof(EBodyPart))` por um array estático constante `static readonly EBodyPart[] AllBodyParts = { ... }`.
2. **Cache de Reflexão Genérica no HUD (`BandAidUI.cs`):**
   - Pré-instanciar delegates/métodos especializados para os 7 tipos de efeito no `CacheTypes()`, eliminando chamadas repetidas de `MakeGenericMethod` e alocações de array `new object[] { bodyPart }` a cada 250ms.
3. **Cache do Animador de Armas (`MedicHealPatch.cs`):**
   - Adicionar `_fiFirearmsAnimator` ao `EnsureFieldCache`, evitando lookups de reflexão dinâmicos durante o disparo da animação.
4. **Otimização de Lookup (`MedicInteractable.cs` & `BandAidController.cs`):**
   - Fazer `MedicInteractable.Ensure(target)` retornar o próprio componente injetado, evitando `target.GetComponent` duplicado no mesmo frame.

---

### Grupo C · Sistema de Debug Logs no Menu F12 (BepInEx Config)
> [!IMPORTANT]
> **Situação Atual no Mod:**
> - O mod já possui `ConfigVerboseEngineLog` (para detalhes de polling do motor Trauma 2.0) e `ConfigDebugTestConsumer`.
> - **Porém, o Módulo Médico (Band-Aid, Rede, Animações e Handshakes) e os patches de Físicas não possuíam um toggle de Debug no F12**, fazendo com que logs diagnósticos de `method_5`, recomputes de velocidade e pacotes de rede fossem emitidos incondicionalmente no console.

#### Novas Configurações no F12 (`TRLImmersiveCombatMedicinePlugin.cs`):
1. **`ConfigDebugMedicLogs` (Seção `"4. Keybinds (Medic)"` ou `"99. Diagnóstico & Debug"`):**
   - *Default:* `false` (Desligado em produção).
   - *Descrição:* Controla a exibição de logs de diagnóstico de ativação de modo médico, animação `method_5`, chamadas de bridge e handshakes de rede.
2. **`ConfigDebugPhysicsLogs` (Seção `"5. Trauma 2.0 (Motor)"`):**
   - *Default:* `false` (Desligado em produção).
   - *Descrição:* Controla a exibição de logs de recompute de `SpeedLimits` e clamps de velocidade.

#### Gateamento nos Arquivos:
- `MedicHealPatch.cs`: Log diagnóstico de `method_5` só emite quando `ConfigDebugMedicLogs.Value == true`.
- `SpeedLimitPatches.cs`: Re-logs de `UpdateSpeedLimitByHealth` só emitem quando `ConfigDebugPhysicsLogs.Value == true`.
- `BandAidNetworkHandler.cs`: Logs de fluxo de pacote rotineiro só emitem quando `ConfigDebugMedicLogs.Value == true` (erros de rede reais continuam sempre visíveis).
- `BandAidController.cs`: Logs de polling e distâncias só emitem com Debug ativo.

---

### Grupo D · Versionamento e SemVer (GEMINI.md)
- Atualizar versão para `1.13.5` em:
  - `TRLImmersiveCombatMedicinePlugin.cs` (`[BepInPlugin(..., "1.13.5")]`)
  - `TRL-ImmersiveCombatMedicine.csproj` (`<Version>1.13.5</Version>`)

---

## 3. Arquivos Propostos para Modificação

### [Componente: Plugin & Configuração]
#### [MODIFY] [`TRLImmersiveCombatMedicinePlugin.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/TRLImmersiveCombatMedicinePlugin.cs)
- Adicionar `ConfigDebugMedicLogs` e `ConfigDebugPhysicsLogs`.
- Atualizar SemVer para `1.13.5` e ajustar imports de namespaces.

#### [MODIFY] [`TRL-ImmersiveCombatMedicine.csproj`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/TRL-ImmersiveCombatMedicine.csproj)
- Atualizar `<Version>1.13.5</Version>`.

---

### [Componente: Medical & Helpers]
#### [MODIFY] [`Helpers/ItemDatabase.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Helpers/ItemDatabase.cs)
- Atualizar namespace para `TRLImmersiveCombatMedicine.Helpers`.

#### [MODIFY] [`Helpers/ImageLoader.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Helpers/ImageLoader.cs)
- Atualizar namespace para `TRLImmersiveCombatMedicine.Helpers`.

#### [MODIFY] [`Helpers/HandsStateGuard.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Helpers/HandsStateGuard.cs)
- Atualizar namespace para `TRLImmersiveCombatMedicine.Helpers`.

#### [MODIFY] [`Patches/Medical/MedicalLogic.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Patches/Medical/MedicalLogic.cs)
- Adicionar `static readonly EBodyPart[] AllBodyParts` para otimização de GC.
- Atualizar namespace para `TRLImmersiveCombatMedicine.Medical`.

#### [MODIFY] [`Patches/Medical/BandAidUI.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Patches/Medical/BandAidUI.cs)
- Cachear métodos genéricos especializados de `FindActiveEffect` para eliminar micro-alocações per-frame.
- Atualizar namespace para `TRLImmersiveCombatMedicine.Medical`.

#### [MODIFY] [`Patches/Medical/MedicHealPatch.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Patches/Medical/MedicHealPatch.cs)
- Cachear `_fiFirearmsAnimator` em `EnsureFieldCache`.
- Gatear log diagnóstico de `method_5` com `ConfigDebugMedicLogs.Value`.
- Atualizar namespace para `TRLImmersiveCombatMedicine.Medical`.

#### [MODIFY] [`Patches/Medical/BandAidController.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Patches/Medical/BandAidController.cs)
- Gatear logs diagnósticos com `ConfigDebugMedicLogs.Value`.
- Atualizar namespace para `TRLImmersiveCombatMedicine.Medical`.

#### [MODIFY] [`Patches/Medical/BandAidNetworkHandler.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Patches/Medical/BandAidNetworkHandler.cs)
- Gatear logs informativos de rede com `ConfigDebugMedicLogs.Value`.
- Atualizar namespace para `TRLImmersiveCombatMedicine.Medical`.

#### [MODIFY] [`Patches/Medical/CustomClassesBridge.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Patches/Medical/CustomClassesBridge.cs)
- Atualizar namespace para `TRLImmersiveCombatMedicine.Medical`.

#### [MODIFY] [`Patches/Medical/MedicInteractable.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Patches/Medical/MedicInteractable.cs)
- Otimizar retorno do método `Ensure(Player target)`.
- Atualizar namespace para `TRLImmersiveCombatMedicine.Medical`.

---

### [Componente: Trauma & Fika]
#### [MODIFY] [`Patches/Trauma/HealthPatches.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Patches/Trauma/HealthPatches.cs)
- Atualizar namespace para `TRLImmersiveCombatMedicine.Trauma`.

#### [MODIFY] [`Patches/Trauma/SpeedLimitPatches.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Patches/Trauma/SpeedLimitPatches.cs)
- Gatear re-logs de calibração de velocidade com `ConfigDebugPhysicsLogs.Value`.
- Limpar `using TrueTrauma;`.

#### [MODIFY] [`Patches/Trauma/MovementPatches.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Patches/Trauma/MovementPatches.cs)
- Atualizar namespace para `TRLImmersiveCombatMedicine.Trauma`.

#### [MODIFY] [`Patches/Trauma/InputPatches.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Patches/Trauma/InputPatches.cs)
- Atualizar namespace para `TRLImmersiveCombatMedicine.Trauma`.

#### [MODIFY] [`Patches/Trauma/BotPatches.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Patches/Trauma/BotPatches.cs)
- Atualizar namespace para `TRLImmersiveCombatMedicine.Trauma`.

#### [MODIFY] [`Patches/Trauma/FikaRevivePatch.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Patches/Trauma/FikaRevivePatch.cs)
- Atualizar namespace para `TRLImmersiveCombatMedicine.Trauma`.

#### [MODIFY] [`Patches/Trauma/TraumaPurge.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Patches/Trauma/TraumaPurge.cs)
- Limpar `using TrueTrauma;`.

#### [MODIFY] [`Helpers/AggroHelper.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Helpers/AggroHelper.cs)
- Atualizar namespace para `TRLImmersiveCombatMedicine.Helpers`.

#### [MODIFY] [`Helpers/VoiceAndHealthUtils.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Helpers/VoiceAndHealthUtils.cs)
- Atualizar namespace para `TRLImmersiveCombatMedicine.Helpers`.

#### [MODIFY] [`Fika/FikaBridge.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Fika/FikaBridge.cs)
- Atualizar namespace para `TRLImmersiveCombatMedicine.Fika`.

---

## 4. Plano de Verificação

### 4.1. Compilação Automatizada
- Executar compilação local com `dotnet build` em `modded-testchannel/` garantindo:
  - **Zero Erros** de compilação C#.
  - **Zero Warnings** de ambiguidade de tipos ou referências.
  - Artefato binário gerado exclusivamente em `modded-testchannel/bin/Release/netstandard2.0/`.

### 4.2. Verificação de Comportamento de Logs
- Testar que com as novas opções de debug desligadas (`false`), o console permanece totalmente silencioso durante transições de mira, animações de cura e recomputes de mancar.
- Confirmar que ao ligar as opções no F12, os logs diagnósticos voltam a ser impressos para suporte.
