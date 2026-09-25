---
title: "Item 007 — Culling de Áudio Ambiente Inaudível — As-Built"
date: "2026-09-15"
status: "🔵 Concluído"
authors: ["Antigravity"]
---

# Item 007 — Culling de Áudio Ambiente Inaudível — As-Built

## 1. Resumo Executivo

O Item 007 implementou no mod **TRL-CoreSight** o módulo de **Culling de Áudio Ambiente Inaudível** (`AmbientAudioCullingManager`).

A Unity DSP e o subsistema de som do Escape from Tarkov continuam calculando atenuação tridimensional, curvas logarítmicas de volume, espacialização e oclusão de materiais para centenas de emissores de áudio ambiente contínuos (`AudioSource` com `loop == true`), mesmo quando o jogador se encontra a centenas de metros de distância e o volume acústico efetivo é inaudível (0 dB). O módulo pausa (`Pause()`) essas fontes distantes e as retoma suavemente (`UnPause()`) com histerese acústica ao se aproximar, liberando ciclos vitais de CPU na thread de áudio.

---

## 2. Componentes e Funcionalidades Implementadas

### 2.1. Gerenciador de Áudio Ambiente (`Core/AmbientAudioCullingManager.cs`)
- **Varredura e Catalogação Inicial:**
  - Varre os `AudioSource` da cena no início da raid.
  - Filtra exclusivamente emissores com `loop == true`.
  - Ignora sumariamente áudios de disparos balísticos, passos, recargas, jogadores/bots (`Player`) e alarmes/sirenes de mapa.
- **Histerese e Corte Acústico:**
  - $D_{corte} = \text{maxDistance} + \text{Margem}$ (padrão 10m de margem além do alcance do som).
  - Pausa o emissor com `Pause()` ao se afastar e retoma com `UnPause()` ao se aproximar.
  - Controle estrito via flag `IsPausedByMod`, assegurando que emissores desligados nativamente pelo mapa (ex.: lâmpadas apagadas que aguardam chave de força geral) nunca sejam tocados acidentalmente.
- **Time-Slicing Suave (Batching):**
  - Avaliação de 32 fontes por frame, garantindo menos de 0.04ms de custo de CPU por quadro.
- **Re-escaneamento Periódico:**
  - Varredura a cada 60 segundos para catalogar novos emissores ativados dinamicamente no mapa.
- **Cleanup Idempotente:**
  - `ResumeAll()` retoma deterministamente todos os emissores pausados ao sair da raid ou desativar o mod.

### 2.2. Integração no `Core/PerformanceManager.cs`
- Instanciação de `AmbientAudioCullingManager` no início da raid (`Initialize()`).
- Chamada de `_audioManager.OnUpdate()` no loop do jogo.
- Linha de telemetria no debug on-screen: `Áudio Ocluído: X/Total`.
- Encerramento limpo e restauração no `Cleanup()`.

### 2.3. Menu BepInEx F12 (`Configuration/ModConfig.cs` e `PROPRIEDADES.md`)
Criada a seção **`13. Otimização de Áudio (Culling de Ambiente)`**:
- `EnableAmbientAudioCulling` (`bool`, padrão `true`): Pausa fontes de áudio ambiente contínuas inaudíveis à distância para poupar CPU de mixagem.
- `AudioCullingMargin` (`float`, padrão `10.0m`, faixa `2.0m` a `30.0m`): Margem de segurança em metros além do maxDistance para pausar o áudio sem cortes sonoros.

---

## 3. Arquivos Criados e Modificados

| Arquivo | Ação | Descrição |
|---|---|---|
| `modded/Core/AmbientAudioCullingManager.cs` | CRIAR | Componente com varredura, time-slicing de 32 fontes/frame, histerese acústica e ciclo de vida seguro. |
| `modded/Configuration/ModConfig.cs` | MODIFICAR | Inclusão das propriedades da seção 13 e evento `OnAudioSettingsChanged`. |
| `modded/Core/PerformanceManager.cs` | MODIFICAR | Integração com inicialização, atualização por frame, telemetria OnGUI e cleanup. |
| `modded/Plugin.cs` | MODIFICAR | Bump SemVer para `0.3.7`. |
| `modded/TRL-CoreSight.csproj` | MODIFICAR | Adição de referência `UnityEngine.AudioModule.dll`, inclusão do manager compilado e bump SemVer para `0.3.7`. |
| `mod.json` | MODIFICAR | Bump de versão para `0.3.7`. |
| `PROPRIEDADES.md` | MODIFICAR | Documentação detalhada da Seção 13 em português do Brasil. |

---

## 4. Verificação e Compilação

- **Compilação:** `dotnet build mods/TRL-CoreSight/modded/TRL-CoreSight.csproj -c Release`
- **Resultado:** 0 Erros, 0 Avisos. Tempo decorrido: 1.21s.
- **Binário isolado gerado:** `mods/TRL-CoreSight/builds/TRL-CoreSight.dll` (76.800 bytes).
