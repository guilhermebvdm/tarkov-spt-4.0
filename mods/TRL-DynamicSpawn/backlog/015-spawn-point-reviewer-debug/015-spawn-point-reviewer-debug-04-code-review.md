---
title: "Item 015 — Spawn Point Reviewer (Debug/Validação Visual 3D) — Code Review"
date: "2026-09-15"
status: "🔵 Em andamento"
authors: ["Antigravity"]
---

# Item 015 — Spawn Point Reviewer (Debug/Validação Visual 3D) — Code Review

## 1. Escopo da Revisão

Revisão técnica do código implementado no Client do mod `TRL-DynamicSpawn` para auditoria e ajuste fino interativo de coordenadas de spawn (Nativas da BSG e coordenadas importadas do MOAR).

### Arquivos Modificados / Criados

| Arquivo | Tipo | Descrição |
|---|---|---|
| `Client/Components/SpawnMarkerVisual.cs` | Novo | Componente visual de cada marcador 3D (poste `LineRenderer`, plano `LevelQuad`, billboard `TextMeshPro`, collider e materiais compartilhados `ZTest Always`). |
| `Client/Components/SpawnPointReviewerManager.cs` | Novo | Orquestrador da sessão de review, varredura de pontos nativos e MOAR, raycast hover/lock, binds de movimentação 3D, snap no solo, persistência JSON e HUD `OnGUI`. |
| `Client/Helpers/Settings.cs` | Modificado | Configurações BepInEx F12 para ativar o reviewer e definir atalhos de seleção, snap, aprovação e reprovação. |
| `Client/Helpers/RaidLifecycle.cs` | Modificado | Início e parada de sessão idempotentes vinculados a `OnRaidStart` e `OnRaidEnd`. |
| `Client/Plugin.cs` | Modificado | Registro de `SpawnPointReviewerManager.Enable()` e bump de versão SemVer para `3.7.9`. |
| `Client/TRL-DynamicSpawn-Client.csproj` | Modificado | Referência a `Unity.TextMeshPro.dll` e bump de versão SemVer para `3.7.9`. |
| `Server/TRL-DynamicSpawn-Server.csproj` | Modificado | Bump de versão SemVer para `3.7.9`. |
| `Server/ModMetadata.cs` | Modificado | Bump de versão SemVer para `3.7.9`. |

---

## 2. Análise Crítica de Código e Arquitetura

### 2.1. Desempenho e Eficiência de Renderização
- **Materiais Compartilhados (`SpawnMarkerVisual.InitMaterials`):** A implementação utiliza instâncias estáticas únicas para os materiais coloridos (`_matRed`, `_matMagenta`, `_matGreen`, `_matBlack`, `_matYellow`) com shader `Hidden/Internal-Colored` (e fallback seguro `UI/Default`). Isso evita que cada um dos 100~300 pontos da raid instancie seu próprio material em memória, prevenindo vazamento de VRAM e mantendo draw calls agrupadas.
- **Raycast de Hover:** O raycast é disparado apenas 1 vez por frame no `Update` a partir do centro da câmera ativa (`Physics.Raycast(ray, 150f, ~0, QueryTriggerInteraction.Collide)`). Como os colliders dos postes são `CapsuleCollider` leves com `isTrigger = true`, a sobrecarga física na Unity é desprezível.

### 2.2. Robustez com Câmera Livre (FreeCam)
- **Fallback da Câmera Ativa (`SpawnPointReviewerManager.GetActiveCamera`):** Em EFT com mods como FreeCam ou câmeras de depuração do Unity, `Camera.main` pode se tornar `null` ou perder a tag. O método realiza fallback hierárquico: `Camera.main` -> `Camera.current` -> `Camera.allCameras.FirstOrDefault(enabled && active)`. Isso assegura que o HUD, o billboard e a seleção continuem funcionando perfeitamente em modo de voo livre.

### 2.3. Resiliência do Pipeline de Persistência JSON
- **Atomicidade e Rastreabilidade:** Cada alteração de coordenadas (translação ou snap) ou mudança de status (aprovação/reprovação) atualiza o dicionário em memória e salva imediatamente o arquivo `<ModDir>/SpawnReviews/<map>.json`. O arquivo contém a posição original, a posição ajustada, o indicador `wasMoved`, o timestamp UTC e o status (`Pending`, `Approved`, `Rejected`), permitindo versionamento git limpo do progresso de auditoria.

### 2.4. Isolamento Estrito de Gameplay
- **Desligado por Padrão:** `Settings.enableSpawnPointReviewer` é falso por padrão no F12. Nenhum objeto 3D ou collider é gerado caso a opção esteja desligada.
- **Destruição Completa em `StopReviewSession`:** No término da raid ou ao desmarcar a opção no F12, todos os GameObjects instanciados são destruídos via `Destroy()`, listas e dicionários são limpos, garantindo zero retenção de memória entre raids.

---

## 3. Conformidade com as Regras do Projeto

| Regra | Status | Evidência |
|---|---|---|
| **SemVer Bump Obrigatório** | ✅ Aprovado | Versão incrementada de `3.7.8` para `3.7.9` em `Plugin.cs`, ambos `.csproj` e `ModMetadata.cs`. |
| **Isolamento de Build** | ✅ Aprovado | Binários compilados estritamente em `Client/bin/Release` e `Server/bin/Release`; nada copiado para `D:/SPT`. |
| **Ciclo de Backlog** | ✅ Aprovado | Item `015-spawn-point-reviewer-debug` documentado de 01 a 05. |
| **Idioma** | ✅ Aprovado | Respostas, documentação e comentários em Português do Brasil. |

---

## 4. Veredito do Code Review

🟢 **Aprovado para Homologação em Raid Real.**
O código está limpo, bem documentado, modular e compilou com 0 erros e 0 avisos no Client.
