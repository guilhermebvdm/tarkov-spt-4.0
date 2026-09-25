---
title: "Item 002 — Culling de Loose Loot Dinâmico & ADS Frustum Bypass — Code Review 01"
date: "2026-09-15"
status: "🔵 Em andamento"
authors: ["Antigravity"]
---

# Item 002 — Culling de Loose Loot Dinâmico & ADS Frustum Bypass — Code Review 01

## 1. Escopo da Revisão

Revisão técnica do código implementado no mod `TRL-CoreSight` para o gerenciamento e culling inteligente de loose loot no mundo da raid, incluindo escalonamento por tamanho de grid e bypass via frustum de câmera ao mirar (ADS).

### Arquivos Criados / Modificados

| Arquivo | Tipo | Descrição |
|---|---|---|
| `modded/Core/LootCullingManager.cs` | Novo | Gerenciador dinâmico de loose loot com particionamento em lotes de 64 itens/frame, classificação por tamanho de células e frustum culling seletivo em ADS. |
| `modded/Patches/LootRegisterPatch.cs` | Novo | Patch Harmony Postfix para interceptar `GameWorld.RegisterLoot<LootItem>` e cadastrar os itens instanciados no mundo. |
| `modded/Configuration/ModConfig.cs` | Modificado | Inclusão de configurações da Seção 9 de F12 (`EnableLootCulling`, `SmallLootDistance`, `MediumLootDistance`, `EnableADSLootBypass`). |
| `modded/Core/PerformanceManager.cs` | Modificado | Instanciação e controle de ciclo de vida (`Initialize`, `OnUpdate`, `OnGUI`, `Cleanup`) do `LootCullingManager`. |
| `modded/Plugin.cs` | Modificado | Ativação do `LootRegisterPatch` e bump SemVer para `0.3.3`. |
| `modded/TRL-CoreSight.csproj` | Modificado | Inclusão dos novos arquivos compilados e bump SemVer para `0.3.3`. |
| `mod.json` | Modificado | Bump SemVer para `0.3.3`. |
| `PROPRIEDADES.md` | Modificado | Documentação em pt-BR da seção 9 no F12. |

---

## 2. Análise Crítica de Código e Arquitetura

### 2.1. Desempenho e Distribuição em Fatias de Tempo (Time-Slicing)
- **Zero Spikes no Frame Rate:** Ao iterar um lote estrito de no máximo 64 itens por frame (`_batchIndex = (_batchIndex + ProcessBatchSize) % _lootItems.Count`), o gerenciador evita inspecionar centenas ou milhares de itens de uma só vez, eliminando micro-congelamentos de CPU.
- **Checagem Quadrática Leve (`sqrMagnitude`):** As distâncias são calculadas comparando magnitudes ao quadrado com os raios pré-calculados ao quadrado (`smallDistSqr` e `medDistSqr`).

### 2.2. Preservação de Jogabilidade e Imunidade de Itens Críticos
- **Sem Invisibilidade Indesejada em Armas e Mochilas:** O cálculo de tamanho de slots (`Item.CalculateCellSize()`) classifica itens em:
  - *Small* ($\le 2$ slots: munições soltas, moedas, itens pequenos de cura/comida) $\to$ culling além de 30m.
  - *Medium* ($3$ a $6$ slots: capacetes, coletes médios, remédios médios) $\to$ culling além de 60m.
  - *Large* ($> 6$ slots: rifles, fuzis de assalto, mochilas grandes) $\to$ **imunes a culling** (`maxDist = float.MaxValue`). Isso elimina completamente o estranhamento visual de ver uma arma grande sumir a curta/média distância.
- **Preservação de Colisão e Interação Física:** O método nativo invocado `LootItem.method_10(bool isVisible)` desativa exclusivamente os `LODGroup` e `Renderer` do modelo 3D, sem mexer no `_boundCollider`. Dessa forma, o jogador continua conseguindo passar por cima, pegar com o prompt 'F' e o item mantém colisão física natural.

### 2.3. Prevenção do "Mega-Stutter" em ADS (Frustum Filtering)
- **Bypass Seletivo AABB:** Se o jogador mira através de uma luneta de longa distância, o sistema não acorda todos os itens do mapa simultaneamente. Em vez disso, se `EnableADSLootBypass` estiver ligado e o jogador estiver em mira aproximada (`ProceduralWeaponAnimation.IsAiming`), apenas os itens contidos dentro da pirâmide de visão da câmera (`GeometryUtility.TestPlanesAABB(planes, bounds)`) recebem override de visibilidade.

### 2.4. Resiliência a Morte e Fim de Raid
- **Detecção de Estado Vital do Jogador:** Se o jogador morrer (`!player.HealthController.IsAlive`), o sistema desativa o culling forçado e restaura a visibilidade normal dos itens para telas de pós-morte ou modo espectador coop FIKA.
- **Limpeza no `Cleanup()`:** Toda a lista interna `_lootItems` é percorrida, garantindo que qualquer item que estivesse culled seja restaurado antes de descarregar a cena.

---

## 3. Conformidade com as Regras do Projeto

| Regra | Status | Evidência |
|---|---|---|
| **SemVer Bump Obrigatório** | ✅ Aprovado | Versão incrementada de `0.3.2` para `0.3.3` em `Plugin.cs`, `TRL-CoreSight.csproj` e `mod.json`. |
| **Isolamento de Build** | ✅ Aprovado | DLL final gerada exclusivamente em `mods/TRL-CoreSight/builds/TRL-CoreSight.dll` (54.272 bytes). Nada copiado para `D:/SPT`. |
| **Ciclo de Backlog** | ✅ Aprovado | Item 002 documentado de ponta a ponta (01-spec, 02-spec-tech, 03-review, 04-code-review, 05-asbuild). |
| **Idioma** | ✅ Aprovado | Toda a documentação e código em Português do Brasil. |

---

## 4. Veredito do Code Review

🟢 **Aprovado com Louvor para Homologação em Raid.** A implementação é robusta, limpa, protege o frame rate e respeita a integridade física dos itens do jogo.
