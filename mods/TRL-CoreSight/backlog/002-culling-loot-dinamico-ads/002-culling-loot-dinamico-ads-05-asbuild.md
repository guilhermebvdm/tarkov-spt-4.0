---
title: "Item 002 — Culling de Loose Loot Dinâmico & ADS Frustum Bypass — As-Built"
date: "2026-09-15"
status: "🔵 Em andamento"
authors: ["Antigravity"]
---

# Item 002 — Culling de Loose Loot Dinâmico & ADS Frustum Bypass — As-Built

## 1. Resumo Executivo

O Item 002 implementou no mod **TRL-CoreSight** o módulo de **Culling de Loose Loot Dinâmico com ADS Frustum Bypass**.

Cenários como Streets of Tarkov, Interchange e Customs contêm centenas de itens soltos (*loose loot*) espalhados pelo chão, prateleiras e mesas. Cada item solto possui componentes `LODGroup`, múltiplos sub-renderizadores, materiais e sombras ativas, gerando centenas de chamadas de desenho (*draw calls*) desnecessárias mesmo para itens minúsculos (cartelas de fósforo, caixas de munição) situados a 50 metros do jogador.

Este módulo categoriza o loot pelo tamanho em células do inventário (slots), aplicando culling de renderização proporcional à relevância visual, com restauração seletiva através do frustum da mira quando o jogador entra em ADS (*Aim Down Sights*), além de garantir que a colisão física e a interação por tecla permaneçam 100% ativas.

---

## 2. Componentes e Funcionalidades Implementadas

### 2.1. Monitor e Fila de Culling (`Core/LootCullingManager.cs`)
- **Cadastro Dinâmico de Loot:**
  - Registra instâncias de `LootItem` via patch em `GameWorld.RegisterLoot<LootItem>`.
  - Ignora contêineres de loot estáticos (caixas, casacos) ou corpos de jogadores (`Corpse`), focando exclusivamente em itens soltos.
- **Classificação por Células de Inventário (`Item.CalculateCellSize()`):**
  - **Pequeno ($\le 2$ slots):** Limiar padrão de **30 metros**.
  - **Médio ($3$ a $6$ slots):** Limiar padrão de **60 metros**.
  - **Grande ($> 6$ slots - rifles, bolsas grandes):** **Imune a culling** (visibilidade mantida sem corte artificial).
- **Processamento em Lotes Escalonados (Time-Slicing):**
  - Processa uma fatia máxima de 64 itens por frame, distribuindo a carga de CPU igualmente ao longo dos quadros.
- **ADS Frustum Filtering:**
  - Quando o jogador mira (`ProceduralWeaponAnimation.IsAiming == true`), os planos do frustum da câmera (`GeometryUtility.CalculateFrustumPlanes`) são calculados uma vez por frame.
  - Apenas os itens dentro do campo de visão da mira têm sua visibilidade ligada preventivamente (`GeometryUtility.TestPlanesAABB`), evitando o pico de reativação global de itens atrás de paredes ou fora de foco.
- **Isolamento de Renderização:**
  - Utiliza o método nativo de performance da BSG `LootItem.method_10(bool isVisible)`.
  - Liga e desliga exclusivamente os `LODGroup` e `Renderer`, mantendo o `_boundCollider` e a física intactos.

### 2.2. Patches Harmony (`Patches/LootRegisterPatch.cs`)
- Intercepta `GameWorld.RegisterLoot<LootItem>` no pós-instanciação para registrar qualquer novo item derrubado por bots ou jogadores ao longo da partida.

### 2.3. Menu BepInEx F12 (`Configuration/ModConfig.cs` e `PROPRIEDADES.md`)
Criada a seção **`9. Otimização de Loose Loot`**:
- `EnableLootCulling` (`bool`, padrão `true`): Ativa ou desativa o culling de itens no chão.
- `SmallLootDistance` (`float`, padrão `30.0m`, faixa `15.0m` a `80.0m`): Distância para corte de itens pequenos ($\le 2$ slots).
- `MediumLootDistance` (`float`, padrão `60.0m`, faixa `30.0m` a `120.0m`): Distância para corte de itens médios ($3$ a $6$ slots).
- `EnableADSLootBypass` (`bool`, padrão `true`): Reativa itens dentro da mira telescópica durante ADS.

### 2.4. Telemetria On-Screen (`Core/PerformanceManager.cs`)
- Adicionada a contagem em tempo real de itens monitorados e ocultos no painel de debug (`F11`):
  `Loot Culling: Ativo (X culled / Y total)`.

---

## 3. Arquivos Criados e Modificados

| Arquivo | Ação | Descrição |
|---|---|---|
| `modded/Core/LootCullingManager.cs` | CRIAR | Motor de verificação em lotes, classificação por tamanho de slots e ADS bypass. |
| `modded/Patches/LootRegisterPatch.cs` | CRIAR | Hook de registro de novos itens soltos em `GameWorld`. |
| `modded/Configuration/ModConfig.cs` | MODIFICAR | Definição das configurações da Seção 9 de F12. |
| `modded/Core/PerformanceManager.cs` | MODIFICAR | Integração do ciclo de vida e telemetria do `LootCullingManager`. |
| `modded/Plugin.cs` | MODIFICAR | Ativação do patch e bump de versão para `0.3.3`. |
| `modded/TRL-CoreSight.csproj` | MODIFICAR | Inclusão de arquivos de compilação e versão `0.3.3`. |
| `mod.json` | MODIFICAR | Bump de versão para `0.3.3`. |
| `PROPRIEDADES.md` | MODIFICAR | Documentação em pt-BR da Seção 9. |

---

## 4. Verificação e Compilação

- **Compilação:** `dotnet build mods/TRL-CoreSight/modded/TRL-CoreSight.csproj -c Release`
- **Resultado:** 0 Erros, 0 Avisos. Tempo decorrido: 1.11s.
- **Binário isolado gerado:** `mods/TRL-CoreSight/builds/TRL-CoreSight.dll` (54.272 bytes).
