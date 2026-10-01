# 011 — remocao-otimizador-lunetas-smart-pip · Spec Funcional

**Mod:** TRL-CoreSight  
**Data:** 2026-09-20  
**Status:** 🔵 Em andamento  

## 1. Contexto e Motivação

O subsistema "Smart PiP" foi introduzido com a hipótese de aliviar a GPU e a taxa de quadros (FPS) ao engajar miras telescópicas de aumento (Picture-in-Picture), reduzindo forçadamente o `lodBias` periférico (`PiPExternalLODBias = 0.75`) e a distância de sombras (`PiPShadowDistance = 35m`), além de desativar a luz volumétrica interna da luneta via Harmony (`StripOpticVolumetrics`).

Entretanto, a validação empírica e a análise arquitetural demonstraram que:
1. **Ausência de ganho real de FPS:** Ocultar o render de malhas ou forçar níveis de LOD simplificados durante a visada não descarrega a geometria da memória de vídeo (VRAM) nem interrompe as matrizes de transformação de cena. O gargalo do SPT em PiP reside na necessidade de desenhar a cena duas vezes com duas câmeras ativas (`BaseOpticCamera` e `FPS Camera`), e a redução superficial de LOD periférico não compensa o overhead de draw calls adicionais.
2. **Degradação e Bug Visual:** A agressiva redução de LOD externo e distância de sombras ao mirar causava pop-in gritante e desaparecimento súbito de geometrias do cenário e vegetação ao redor do tubo da mira, gerando uma experiência de jogo artificial e prejudicial à imersão tática.
3. **Decisão:** Remover integralmente o subsistema Smart PiP, eliminando as opções obsoletas de F12 e simplificando o pipeline de LOD e sombras do mod.

## 2. Escopo

### O que entra
- Remoção do patch `OpticVolumetricsPatch.cs` e de sua referência no `.csproj` e no `Plugin.cs`.
- Remoção das propriedades e configurações de F12 em `ModConfig.cs` (`EnableSmartPiP`, `PiPShadowDistance`, `PiPExternalLODBias`, `StripOpticVolumetrics`).
- Desacoplamento da lógica de PiP em `InteriorOcclusionWatcher.cs`, que volta a ser 100% dedicada a interiores/exteriores.
- Desacoplamento da lógica de PiP em `PerformanceManager.cs`: `HandleDynamicLOD` volta a alternar suavemente apenas entre `AimLODBias` e `BaseLODBias`.
- Atualização da tabela de configurações em `PROPRIEDADES.md`.

### O que NÃO entra
- Nenhuma alteração nos outros subsistemas do CoreSight:
  - LOD Dinâmico geral (visada padrão vs navegação normal).
  - Otimização de Sombras em Interiores (`InteriorOcclusionWatcher`).
  - Otimização de Bots, Declutter, Loose Loot, Sombras Secundárias, GC Optimizer, Camuflagem Zonal e Afinidade de CPU.

## 3. Critérios de Aceite

1. **Ausência de Menus Órfãos:** O menu de configuração F12 não deve mais exibir a seção `"7. Otimizador de Lunetas (Smart PiP)"`.
2. **Estabilidade Visual em Miras:** Ao mirar com lunetas de aumento (ex: Vudu, Razor HD, Schmidt & Bender), o cenário periférico não deve sofrer redução forçada de LOD ou corte de sombras, preservando a coerência visual.
3. **LOD Dinâmico Preservado:** A mira de ferro, red-dots e lunetas continuam ativando o aumento suave para `AimLODBias` (melhor nitidez do alvo mirado) e retornando para `BaseLODBias` ao abaixar a arma.
4. **Isolamento de Sombras de Interior:** Entrar e sair de edifícios continua alternando a distância de sombras normalmente sem interferência de luneta.
5. **Compilação Limpa:** Projeto compila com 0 erros e 0 avisos.
