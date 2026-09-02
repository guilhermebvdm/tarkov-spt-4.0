---
title: "Relatório de Auditoria Técnica de Código — FIKA Modded (Partição 06: Sistemas Auxiliares & HUD)"
date: 2026-09-02
status: 🟢 Vivo
authors: Antigravity
---

# Relatório de Auditoria Técnica de Código — FIKA Modded (Partição 06: Sistemas Auxiliares & HUD)

## 1. Panorama da Partição & Diagnóstico da 2ª Rodada

Esta auditoria técnica da **2ª Rodada** reavalia os componentes de interface, reviver e câmera livre em [`mods/FIKA/modded/Fika-Plugin/Fika.Core/Main/FreeCamera/`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/FIKA/modded/Fika-Plugin/Fika.Core/Main/FreeCamera/) e [`Main/Components/ReviveInteractable.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/FIKA/modded/Fika-Plugin/Fika.Core/Main/Components/ReviveInteractable.cs).

### Quadro Comparativo de Diagnóstico

| Severidade | 1ª Rodada (Original) | Pós-Correção (Fase B) | 2ª Rodada (Novos Achados de Refino) |
| :--- | :---: | :---: | :---: |
| 🔴 **Crítico (Hitboxes Quebradas pós-Revive)** | 1 | 0 | **0** |
| 🟠 **Alto (Thread-Safety de Mensagens de UI)** | 1 | 0 | **0** |
| 🟡 **Médio (AP-02 / Hierarchy Scan na FreeCam)** | 1 | 0 | **2** |
| 🔵 **Baixo (Tipagem)** | 0 | 0 | **0** |
| 💡 **Sugestão de Otimização** | 1 | 0 | **1** |

---

## 2. Novos Achados Identificados na 2ª Rodada

### `AUD-MOD-06-01` — Acesso a `Singleton<GameWorld>.Instance` na FreeCam (AP-02)
- **Arquivo:** [`FreeCameraController.cs:L26-32`](../../modded/Fika-Plugin/Fika.Core/Main/FreeCamera/FreeCameraController.cs#L26-L32)
- **Severidade:** 🟡 Médio (Anti-Pattern AP-02)
- **Descrição:** O getter `Player` acessa `(FikaPlayer)Singleton<GameWorld>.Instance.MainPlayer` sem conferir `Singleton<GameWorld>.Instantiated`, gerando NRE caso a tecla de FreeCam seja acionada na transição de extração.
- **Correção Proposta:** Inserir guarda `Singleton<GameWorld>.Instantiated ? (FikaPlayer)Singleton<GameWorld>.Instance.MainPlayer : null`.

### `AUD-MOD-06-02` — Varredura de Hierarquia com `GameObject.Find` na Propriedade `BattleUI`
- **Arquivo:** [`FreeCameraController.cs:L48-60`](../../modded/Fika-Plugin/Fika.Core/Main/FreeCamera/FreeCameraController.cs#L48-L60)
- **Severidade:** 🟡 Médio (Performance / Frame Drop)
- **Descrição:** `BattleUI` chama `GameObject.Find("BattleUIScreen")` sempre que a propriedade é avaliada e o campo `_playerUI` estiver nulo, varrendo toda a árvore de GameObjects da cena do Unity em tempo de execução.
- **Correção Proposta:** Fazer cache único no `Start()` / `Awake()` ou usar `MonoBehaviourSingleton<CommonUI>.Instance`.

### `AUD-MOD-06-03` — Teardown de Câmera em `FreeCameraController.OnDestroy`
- **Arquivo:** [`FreeCameraController.cs`](../../modded/Fika-Plugin/Fika.Core/Main/FreeCamera/FreeCameraController.cs)
- **Severidade:** 💡 Sugestão de Otimização
- **Descrição:** Adicionar método `OnDestroy()` para garantir que qualquer estado de câmera livre seja desativado ao encerrar a sessão.

---

## 3. Status dos Achados da 1ª Rodada

- `TRL-Fixes #1` (Restauração de colliders Layer 12 pós-revive): ✅ **Integrado**
- `TRL-Fixes #5` (Despacho thread-safe `ShowFikaMessage` para a Main Thread): ✅ **Integrado**
