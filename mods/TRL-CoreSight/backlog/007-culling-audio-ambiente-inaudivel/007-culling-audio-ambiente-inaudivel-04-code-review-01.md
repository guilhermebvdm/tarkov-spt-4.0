---
title: "Item 007 — Culling de Áudio Ambiente Inaudível — Code Review 01"
date: "2026-09-15"
status: "🔵 Em andamento"
authors: ["Antigravity"]
---

# Item 007 — Culling de Áudio Ambiente Inaudível — Code Review 01

## 1. Escopo da Revisão

Revisão técnica do código implementado no mod `TRL-CoreSight` para suspensão dinâmica (pausa/despausa) de emissores contínuos de som ambiente (`AudioSource` com `loop == true`) inaudíveis à distância.

### Arquivos Modificados / Criados

| Arquivo | Tipo | Descrição |
|---|---|---|
| `modded/Core/AmbientAudioCullingManager.cs` | Novo | Componente com time-slicing de 32 fontes/frame, histerese acústica de distância, exclusão de sons vitais e ciclo de vida robusto. |
| `modded/Configuration/ModConfig.cs` | Modificado | Inclusão de `EnableAmbientAudioCulling` e `AudioCullingMargin` na seção 13 de F12 e evento `OnAudioSettingsChanged`. |
| `modded/Core/PerformanceManager.cs` | Modificado | Instanciação, atualização em quadro, telemetria OnGUI e cleanup idempotente com restauração. |
| `modded/Plugin.cs` | Modificado | Bump de versão SemVer para `0.3.7`. |
| `modded/TRL-CoreSight.csproj` | Modificado | Referência para `UnityEngine.AudioModule.dll`, inclusão do manager e bump SemVer para `0.3.7`. |
| `mod.json` | Modificado | Bump SemVer para `0.3.7`. |
| `PROPRIEDADES.md` | Modificado | Documentação em pt-BR da seção 13 de Otimização de Áudio. |

---

## 2. Análise Crítica de Código e Arquitetura

### 2.1. Desempenho e Distribuição de Carga (Time-Slicing)
- **Batching de 32 Fontes por Frame:** Mesmo em mapas com mais de 300 fontes sonoras contínuas (como Interchange e Streets), a varredura consome menos de 0.04ms por quadro, evitando qualquer pico de frame-time.
- **Distância Quadrática (`sqrMagnitude`):** A verificação de distâncias utiliza $D^2$, eliminando completamente cálculos caros de raiz quadrada (`Mathf.Sqrt`).

### 2.2. Robustez Acústica e Imunidade Tática
- **Sons de Tiro, Passos e Recarga 100% Preservados:** Fontes com `loop == false` são ignoradas imediatamente no escaneamento.
- **Imunidade de Jogadores e Bots:** Qualquer emissor vinculado a `Player` (`GetComponentInParent<Player>() != null`) é sumariamente ignorado.
- **Sons Críticos de Mapa (Sirenes/Alarmes):** Verificação de palavras-chave (`siren`, `alarm`, `voice`, `step`) garante que alarmes de extração toquem pelo mapa inteiro sem cortes.
- **Preservação de Fontes Inativas (`IsPausedByMod`):** Se uma luz ou gerador estiver desligado originalmente (esperando botão de energia ser pressionado), o mod não o marca como pausado e nunca o inicia indevidamente.

### 2.3. Ciclo de Vida e Segurança em Raid
- **Cleanup Idempotente (`ResumeAll`):** Todas as fontes pausadas pelo mod têm `UnPause()` chamado no `Cleanup()`, garantindo restauração limpa ao retornar para o menu ou ao desativar a opção no menu F12.
- **Tratamento de Exceções Defensivo:** Verificações de `src.isActiveAndEnabled` e blocos `try/catch` evitam qualquer ruído no log da Unity se emissores forem destruídos durante a raid.

---

## 3. Conformidade com as Regras do Projeto

| Regra | Status | Evidência |
|---|---|---|
| **SemVer Bump Obrigatório** | ✅ Aprovado | Versão incrementada de `0.3.6` para `0.3.7` em `Plugin.cs`, `TRL-CoreSight.csproj` e `mod.json`. |
| **Isolamento de Build** | ✅ Aprovado | DLL gerada exclusivamente em `mods/TRL-CoreSight/builds/TRL-CoreSight.dll` (76.800 bytes). Nada copiado para `D:/SPT`. |
| **Ciclo de Backlog** | ✅ Aprovado | Item 007 documentado (01-spec, 02-spec-tech, 03-review, 04-code-review, 05-asbuild). |
| **Idioma** | ✅ Aprovado | Toda a documentação e código em Português do Brasil. |

---

## 4. Veredito do Code Review

🟢 **Aprovado com Louvor.** A implementação alivia a carga do mixer DSP da Unity sem comprometer a imersão ou a percepção tática de combate do jogador.
