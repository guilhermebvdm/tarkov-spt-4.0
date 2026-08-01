# Propriedades F12 — PlayerLives

> Todas as opções do menu **F12** (BepInEx ConfigurationManager). **4 seções · 10 opções.**
> Gerado de [original/Helpers/Settings.cs](original/Helpers/Settings.cs) em **2026-08-01**, para o clone upstream.
>
> **Plugin:** `com.somtam.playerLives` — "Player Lives" — **v1.2.5** (`BepInPlugin` em [original/Plugin.cs](original/Plugin.cs))
>
> Itens marcados **(Avançado)** só aparecem no F12 com a caixa **"Advanced settings"** ligada.
>
> **Ordem no menu F12:** nenhuma opção deste mod define `Order`, então o ConfigurationManager exibe cada
> seção na ordem de registro das chamadas `config.Bind` (a mesma ordem das tabelas abaixo). As seções
> seguem a ordem de descoberta: `General` → `Revive Conditions` → `On Revive` → `Development`.

---

## General

| Propriedade (EN) | Tradução (pt-BR) | Tipo | Padrão | Faixa | Tooltip (pt-BR) | Avançado |
|---|---|---|---|---|---|---|
| Player Lives | Vidas do jogador | int | `1` | — | Quantos revives por raid. | — |
| Invulnerability Duration (s) | Duração da invulnerabilidade (s) | float | `10` | — | Por quanto tempo você fica invulnerável depois do revive. | — |
| Revival Key | Tecla de revive | KeyCode | `F5` | — | *(sem tooltip)* | — |
| Give Up Key | Tecla de desistir | KeyCode | `F9` | — | *(sem tooltip)* | — |

## Revive Conditions

| Propriedade (EN) | Tradução (pt-BR) | Tipo | Padrão | Faixa | Tooltip (pt-BR) | Avançado |
|---|---|---|---|---|---|---|
| Require Active Buff | Exigir buff ativo | string (lista) | `None` | Lista fixa: `None`, `BuffsAdrenaline`, `BuffsPropital`, `BuffsSJ1TGLabs`, `BuffsSJ6TGLabs`, `BuffsZagustin`, `BuffseTGchange`, `Buffs_2A2bTG`, `Buffs_3bTG`, `Buffs_AHF1M`, `Buffs_Antidote`, `Buffs_L1`, `Buffs_MULE`, `Buffs_Meldonin`, `Buffs_Obdolbos`, `Buffs_Obdolbos2`, `Buffs_P22`, `Buffs_PNB`, `Buffs_Perfotoran`, `Buffs_SJ12_TGLabs`, `Buffs_Trimadol` | Escolhe o buff que precisa estar ativo para o revive funcionar. | — |
| Require Stim | Exigir estimulante | string (lista) | `None` | Lista fixa: `None`, `Any`, `Adrenaline`, `Propital`, `SJ1TGLabs`, `SJ6TGLabs`, `Zagustin`, `eTGchange`, `2A2bTG`, `3bTG`, `AHF1M`, `Antidote`, `L1`, `MULE`, `Meldonin`, `Obdolbos`, `Obdolbos2`, `P22`, `PNB`, `Perfotoran`, `SJ12_TGLabs`, `Trimadol` | Escolhe o estimulante que será consumido no revive. `Any` usa primeiro o estimulante mais barato que você estiver carregando. | — |
| Require Head Health > 0 | Exigir vida na cabeça > 0 | bool | `false` | — | Se a vida da sua cabeça estiver em 0, os revives deixam de funcionar. | — |

## On Revive

| Propriedade (EN) | Tradução (pt-BR) | Tipo | Padrão | Faixa | Tooltip (pt-BR) | Avançado |
|---|---|---|---|---|---|---|
| Restore destroyed body parts | Restaurar membros destruídos | bool | `true` | — | Membros enegrecidos (destruídos) são restaurados%. *(o `%` está no texto original do mod)* | — |
| Restore destroyed body parts healing | Cura ao restaurar membros destruídos | int | `25` | 1–100 | Quantidade de cura em %. | — |

## Development

| Propriedade (EN) | Tradução (pt-BR) | Tipo | Padrão | Faixa | Tooltip (pt-BR) | Avançado |
|---|---|---|---|---|---|---|
| Test Mode | Modo de teste | bool | `false` | — | *(sem tooltip)* | **(Avançado)** |

---

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-08-01 | Guilherme | Criação a partir do clone upstream (SHA `712ad338`, v1.2.5). |
