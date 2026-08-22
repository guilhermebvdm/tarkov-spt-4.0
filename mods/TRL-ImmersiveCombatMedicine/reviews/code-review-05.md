# TRL-ImmersiveCombatMedicine — Code Review 05 (Consolidação e Refatoração Geral v1.13.5)

**Mod:** TRL-ImmersiveCombatMedicine  
**Data:** 2026-08-15  
**Status:** 🟢 Vivo  
**Responsáveis:** Antigravity (Gemini) + Guilherme  
**Workspace:** [modded-V3(review)](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-V3%28review%29)  
**Versão Alvo:** SPT 4.0.13 / EFT 0.16.9 / Fika 2.3.4  
**Referências:** [revisao-geral-consolidada.md](../revisao-geral-consolidada.md), [code-review-04.md](./code-review-04.md), [walkthrough.md](file:///C:/Users/Saraiva/.gemini/antigravity-ide/brain/4176e280-06fc-4a96-993e-353889506499/walkthrough.md)

> Análise crítica do código consolidado e corrigido nos 16 itens do mod (excluindo a nova feature de necrose do Item 07, reservada para discussão dedicada). Cada achado recebe um ID `CR-05-MM` permanente.

---

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 0 · 🟢 Menores: 6 · ✅ Resolvidos: 6 · Total: 6

---

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-05-01 | D — Arquitetura | 🟢 Menor | Unificação de namespaces sob `TRLImmersiveCombatMedicine.*` e desambiguação `global::Fika.Core` | ✅ Resolvido |
| CR-05-02 | F — Melhoria opcional | 🟢 Menor | Otimização de Garbage Collector (GC Alloc = 0) em `MedicalLogic` e `BandAidUI` | ✅ Resolvido |
| CR-05-03 | E — Legibilidade/Manutenção | 🟢 Menor | Governança de logs diagnósticos condicionada a toggles de debug no BepInEx F12 | ✅ Resolvido |
| CR-05-04 | D — Arquitetura | 🟢 Menor | Harmony003 analyzer warning em `HealthPatches.cs` (`DamageInfoStruct` não-ref) | ✅ Resolvido |
| CR-05-05 | B — Bug latente | 🟢 Menor | Integridade de verificação de mãos em `HandsStateGuard.cs` | ✅ Resolvido |
| CR-05-06 | D — Arquitetura | 🟢 Menor | Bump de versão SemVer 1.13.5 e isolamento de builds no `.csproj` | ✅ Resolvido |

---

## Categorias

- **A — Crítico** — bug grave, crash garantido, corrupção de estado, security issue.
- **B — Bug latente** — comportamento errado em cenário plausível, não acionado pelo caminho golden.
- **C — Gap vs. spec** — código não implementa critério de aceite, corner case, ou AC da spec.
- **D — Arquitetura** — viola padrões do repo, duplica código, leak de estado, abuso de reflection.
- **E — Legibilidade/manutenção** — nomes ruins, comentário "porquê" ausente, código morto, complexidade desnecessária.
- **F — Melhoria opcional** — refactor de qualidade, micro-otimização, simplificação.

## Impacto

- 🔴 **Bloqueador** — fix obrigatório antes de fechar o item.
- 🟠 **Forte** — fix recomendado; pode ser deferido para fix futuro.
- 🟡 **Médio** — anotar, decidir caso a caso.
- 🟢 **Menor** — opcional / informativo.

---

## Pontos

### CR-05-01 · D — Arquitetura · 🟢 Menor

**Unificação de namespaces sob `TRLImmersiveCombatMedicine.*` e desambiguação `global::Fika.Core`**

**Local:** [`mods/TRL-ImmersiveCombatMedicine/modded-V3(review)/TRLImmersiveCombatMedicinePlugin.cs:597`](../../modded-V3(review)/TRLImmersiveCombatMedicinePlugin.cs#L597), [`Patches/Medical/*`](../../modded-V3(review)/Patches/Medical/), [`Patches/Trauma/*`](../../modded-V3(review)/Patches/Trauma/)

**Problema:** O mod continha legados fragmentados de namespaces (`Band_Aid`, `TrueTrauma`), criando confusão estrutural e potenciais colisões de nomes quando a pasta `Fika` foi introduzida no namespace `TRLImmersiveCombatMedicine.Fika` (fazendo referências a `Fika.Core` colidirem com o namespace interno).

**Por que importa:** Semântica desorganizada dificulta a manutenção, gera acoplamento espúrio e quebra a compilação por ambiguidade de tipos entre o assembly de bridge local e o assembly oficial do Fika.

**Sugestão:** Todos os arquivos foram organizados sob `TRLImmersiveCombatMedicine`, `TRLImmersiveCombatMedicine.Medical`, `TRLImmersiveCombatMedicine.Trauma`, `TRLImmersiveCombatMedicine.Helpers` e `TRLImmersiveCombatMedicine.Fika`. As referências ao player de rede oficial do Fika foram qualificadas com `global::Fika.Core.Main.Players.FikaPlayer`.

**Decisão:**
- [x] Aceitar sugestão

**Resolução:** ✅ Aplicado na v1.13.5. Compilação limpa e todos os namespaces 100% harmonizados.

---

### CR-05-02 · F — Melhoria opcional · 🟢 Menor

**Otimização de Garbage Collector (GC Alloc = 0) em `MedicalLogic` e `BandAidUI`**

**Local:** [`mods/TRL-ImmersiveCombatMedicine/modded-V3(review)/Patches/Medical/MedicalLogic.cs:19`](../../modded-V3(review)/Patches/Medical/MedicalLogic.cs#L19), [`mods/TRL-ImmersiveCombatMedicine/modded-V3(review)/Patches/Medical/BandAidUI.cs:257`](../../modded-V3(review)/Patches/Medical/BandAidUI.cs#L257)

**Problema:** No fluxo médico, `MedicalLogic` chamava repetidamente `Enum.GetValues(typeof(EBodyPart))`, alocando arrays dinâmicos na Heap. Em `BandAidUI`, a cada frame do `Update()`, a checagem de efeitos dos 7 membros executava `method.MakeGenericMethod()` e `new object[] { bodyPart }`, gerando dezenas de alocações por segundo e causando micro-stutters pelo Garbage Collector do Unity Mono.

**Por que importa:** Conforme `csharp-mod-best-practices` §1 (Allocations), rotinas de HUD e hot-paths não devem gerar alocações na Heap por frame para evitar pressões de GC e perda de FPS durante tiroteios intensos.

**Sugestão:** 
1. Criar array estático `AllBodyParts` em `MedicalLogic`.
2. Criar dicionário de reflexão antecipada `_genericFindMethods` e cache de argumentos `_bodyPartArgsCache` por `EBodyPart` em `BandAidUI`.

**Decisão:**
- [x] Aceitar sugestão

**Resolução:** ✅ Aplicado na v1.13.5. Alocações de GC reduzidas a zero nos loops de verificação de status corporal e de efeitos.

---

### CR-05-03 · E — Legibilidade/Manutenção · 🟢 Menor

**Governança de logs diagnósticos condicionada a toggles de debug no BepInEx F12**

**Local:** [`mods/TRL-ImmersiveCombatMedicine/modded-V3(review)/TRLImmersiveCombatMedicinePlugin.cs:255`](../../modded-V3(review)/TRLImmersiveCombatMedicinePlugin.cs#L255), [`Patches/Medical/MedicHealPatch.cs:120`](../../modded-V3(review)/Patches/Medical/MedicHealPatch.cs#L120), [`Patches/Trauma/SpeedLimitPatches.cs:50`](../../modded-V3(review)/Patches/Trauma/SpeedLimitPatches.cs#L50)

**Problema:** Diversos métodos e patches emitiam `LogInfo` em ações repetitivas (recálculo de velocidade, recompute de limites de física, pacotes de rede e bridge de animação), poluindo o console do BepInEx durante o jogo regular.

**Por que importa:** Conforme `csharp-mod-best-practices` §8 (Logging discipline), logs verbose em tempo de execução devem ser estritamente controlados por opções de configuração, permitindo análise apenas sob demanda do desenvolvedor.

**Sugestão:** Criada a categoria `"12. Debug (Dev)"` no arquivo de configuração do BepInEx com dois novos toggles:
- `ConfigDebugMedicLogs` (`Debug Medic Logs`, default `false`): logs de tratamento, handshake e rede médica.
- `ConfigDebugPhysicsLogs` (`Debug Physics Logs`, default `false`): logs de física, clamp de velocidade e bloqueio de postura.

**Decisão:**
- [x] Aceitar sugestão

**Resolução:** ✅ Aplicado na v1.13.5. Console limpo em gameplay padrão; depuração acionável em tempo real no menu F12.

---

### CR-05-04 · D — Arquitetura · 🟢 Menor

**Harmony003 analyzer warning em `HealthPatches.cs` (`DamageInfoStruct` não-ref)**

**Local:** [`mods/TRL-ImmersiveCombatMedicine/modded-V3(review)/Patches/Trauma/HealthPatches.cs:15`](../../modded-V3(review)/Patches/Trauma/HealthPatches.cs#L15), [`Patches/Trauma/HealthPatches.cs:51`](../../modded-V3(review)/Patches/Trauma/HealthPatches.cs#L51)

**Problema:** O compilador emitia 10 avisos `Harmony003: Harmony non-ref patch parameter damageInfo.DamageType modified. This assignment have no effect.` nas verificações condicionais `damageInfo.DamageType == EDamageType.Bullet`.

**Por que importa:** O analisador do BepInEx interpreta a leitura de propriedades em instâncias de struct passadas por valor como uma potencial tentativa de mutação ineficaz.

**Sugestão:** Ajustar a assinatura do Prefix e Postfix para receber `ref DamageInfoStruct damageInfo`, eliminando os falsos-positivos do analisador e mantendo a integridade da assinatura Harmony.

**Decisão:**
- [x] Aceitar sugestão

**Resolução:** ✅ Aplicado na v1.13.5. Compilação finalizada com **0 Erros e 0 Avisos**.

---

### CR-05-05 · B — Bug latente · 🟢 Menor

**Integridade de verificação de mãos em `HandsStateGuard.cs`**

**Local:** [`mods/TRL-ImmersiveCombatMedicine/modded-V3(review)/Helpers/HandsStateGuard.cs:13`](../../modded-V3(review)/Helpers/HandsStateGuard.cs#L13)

**Problema:** A checagem de estado das mãos precisa garantir que o jogador não inicie nova operação médica enquanto consome itens como comida/bebida ou outros medicamentos já em mãos.

**Por que importa:** Iniciar transições de itens médicos com as mãos ocupadas sem o devido guard pode causar rejeição de operação no cliente Fika e travar as mãos do jogador no estado `HandsController can't perform this operation`.

**Sugestão:** A classe `HandsStateGuard` intercepta e valida `player.HandsController.Item` contra `MedsItemClass` e `FoodDrinkItemClass` de forma defensiva antes de despachar o heal.

**Decisão:**
- [x] Aceitar sugestão

**Resolução:** ✅ Aplicado e validado.

---

### CR-05-06 · D — Arquitetura · 🟢 Menor

**Bump de versão SemVer 1.13.5 e isolamento de builds no `.csproj`**

**Local:** [`mods/TRL-ImmersiveCombatMedicine/modded-V3(review)/TRL-ImmersiveCombatMedicine.csproj:7`](../../modded-V3(review)/TRL-ImmersiveCombatMedicine.csproj#L7), [`TRLImmersiveCombatMedicinePlugin.cs:22`](../../modded-V3(review)/TRLImmersiveCombatMedicinePlugin.cs#L22)

**Problema:** De acordo com as diretrizes do `GEMINI.md` e convenções do repositório, cada rodada de modificações de código exige incremento rigoroso de SemVer (`1.13.4` $\to$ `1.13.5`) e isolamento dos binários exclusivamente dentro do workspace do mod (`modded-V3(review)/bin/Release/`).

**Por que importa:** Previne que binários de testes sobrescrevam o ambiente de jogo real do usuário (`D:/SPT`) e mantém rastreabilidade histórica completa no versionamento.

**Sugestão:** Atualizado `Version` no `.csproj` e a anotação `[BepInPlugin("com.trl.immersivecombatmedicine", "TRL-ImmersiveCombatMedicine", "1.13.5")]` no `Plugin.cs`.

**Decisão:**
- [x] Aceitar sugestão

**Resolução:** ✅ Aplicado. Compilação gera `TRLImmersiveCombatMedicine.dll` v1.13.5 estritamente em `modded-V3(review)/bin/Release/netstandard2.1/`.

---

## Histórico

| Data | Evento |
| --- | --- |
| 2026-08-15 | Code review 05 criada cobrindo as revisões consolidadas dos itens 01 a 16 e compilação v1.13.5 no workspace `modded-V3(review)`. |
