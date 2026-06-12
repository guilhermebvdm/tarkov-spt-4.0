# 013 — Botão SKILLS na navegação · Spec Técnica

**Mod:** CustomClasses
**Spec funcional:** [013-botao-skills-navegacao-01-spec.md](013-botao-skills-navegacao-01-spec.md)
**Criado:** 2026-06-09

> Clona o botão CHARACTER do menu e abre a tela de personagem na **aba Skills**. Refs confirmadas via ilspycmd no DLL real.

## 1. Estratégia

Postfix em `MenuScreen.Show` (3 params) → clona o `_playerButton` (CHARACTER, `DefaultUIButton`) como `CC_SkillsButton`, logo abaixo. O onClick aciona `_playerButton.OnClick.Invoke()` (abre o inventário pelo mesmo fluxo do CHARACTER, sem depender do método obfuscado `method_8`) e dispara uma coroutine que espera o `InventoryScreen` abrir e seleciona a aba Skills.

## 2. Refs (Assembly-CSharp via ilspycmd)

| Símbolo | Observação |
|---|---|
| `EFT.UI.MenuScreen._playerButton` (`DefaultUIButton`) | botão CHARACTER; `OnClick` (UnityEvent) → `method_8(EMenuType.Player)` |
| `EFT.UI.DefaultUIButton.OnClick` (`UnityEvent`) | `AddListener`/`RemoveAllListeners`/`Invoke` |
| `EFT.UI.InventoryScreen._tabDictionary` (`IReadOnlyDictionary<EInventoryTab, Tab>`) | mapa aba→Tab |
| `EFT.UI.EInventoryTab.Skills` (=3) | aba alvo |
| `GClass3808.SelectTab(Tab)` → `Tab.Select(bool)` | seleção da aba (acessada via `Tab.Select` por reflection) |

## 3. Nova config F12

| Seção | Nome | Tipo | Padrão | Tooltip (pt-BR) |
|---|---|---|---|---|
| `General` | `ShowSkillsButton` | bool | `true` | Adiciona um botão SKILLS no menu (abaixo de CHARACTER) que abre a aba Skills. |

## 4. Arquivos

| Ação | Path | Resumo |
|---|---|---|
| CRIAR | `modded/Client/Patches/SkillsNavButtonPatch.cs` | postfix MenuScreen.Show; clona o _playerButton → "SKILLS"; onClick abre CHARACTER + coroutine. |
| CRIAR | `modded/Client/UI/InventoryTabNavigator.cs` | espera o InventoryScreen e chama `Tab.Select(true)` da aba Skills (reflection). |
| MODIFICAR | `modded/Client/Plugin.cs` | config `ShowSkillsButton` + registra o patch. |
| MODIFICAR | `mods/CustomClasses/PROPRIEDADES.md` | `ShowSkillsButton`. |

## 5. Riscos

- **Clone do botão:** o `OnClick` (UnityEvent runtime) não é clonado pelo Instantiate → `RemoveAllListeners` + nosso listener. O `_button` interno re-registra `OnClick.Invoke` no Awake do clone (clique funciona).
- **Menu-Overhaul:** o MO reposiciona os botões existentes; o `CC_SkillsButton` é novo (SetSiblingIndex após CHARACTER). **Validar alinhamento in-game** (pode precisar ajustar posição/estilo).
- **Ícone do clone:** herda o ícone do CHARACTER (caveira) — a definir se troca por um ícone de skills.
- **Seleção da aba:** reflection em `_tabDictionary` + `Tab.Select(bool)`; coroutine espera o `InventoryScreen` (timeout 120 frames). Se a API mudar, no-op (não quebra).
- **Lifecycle:** menu (fora de raid); try/catch + log; idempotente (Find por nome).

## 6. Checklist

- [x] `InventoryTabNavigator` (seleção da aba Skills).
- [x] `SkillsNavButtonPatch` (clone + onClick).
- [x] `Plugin`: `ShowSkillsButton` + registro.
- [x] `PROPRIEDADES.md`.
- [x] `/compile-mod` 0 warn/err.
- [ ] Playtest: botão SKILLS aparece abaixo de CHARACTER (com/sem MO); clicar abre a aba Skills; alinhamento ok.

## Histórico

| Data | Evento |
|---|---|
| 2026-06-09 | Spec técnica + implementação (autônoma). 0 warn/err. A validar in-game. |
