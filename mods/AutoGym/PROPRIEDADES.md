# AutoGym — Propriedades (F12 / BepInEx ConfigurationManager)

> **Plugin:** `sweet.autogym` — AutoGym v1.0.0<br>
> **Fonte:** [original/Plugin.cs](original/Plugin.cs)<br>

Nenhuma propriedade usa `ConfigurationManagerAttributes` (`Order`/`IsAdvanced`) — todas aparecem com "Advanced settings" desligado, na ordenação padrão do ConfigurationManager.

## General

| Propriedade | Tradução (pt-BR) | Tipo | Padrão | Faixa | Tooltip (pt-BR) |
|---|---|---|---|---|---|
| Enabled | Habilitado | `bool` | `true` | — | Completa automaticamente o QTE da academia do esconderijo sem pressionar a tecla do QTE. |

## Visuals

| Propriedade | Tradução (pt-BR) | Tipo | Padrão | Faixa | Tooltip (pt-BR) |
|---|---|---|---|---|---|
| Hide Workout Gear | Ocultar Equipamento no Treino | `bool` | `true` | — | Oculta temporariamente mochila, colete tático, colete blindado, capacete, fone, máscara facial e óculos durante os treinos na academia do esconderijo. |
| Swap Workout Body Skin | Trocar Skin do Torso no Treino | `bool` | `true` | — | Troca temporariamente o torso do personagem para a skin de treino configurada durante exercícios na academia do esconderijo. Restaurado ao encerrar o treino. |
| Workout Body Skin Id | Id da Skin de Treino (Body) | `string` | `66a25a3af12f29d8a2599527` | — | Id do template de customização (parte Body) aplicado durante o treino. Padrão: "Tagilla's Chest" do mod AllTheClothes. Se o template não existir, nenhuma troca ocorre. |

## Timing

| Propriedade | Tradução (pt-BR) | Tipo | Padrão | Faixa | Tooltip (pt-BR) |
|---|---|---|---|---|---|
| Success Window Bias | Viés da Janela de Sucesso | `float` | `0.5` | `0.0`–`1.0` | Em que ponto dentro da janela de sucesso o AutoGym completa o QTE. 0 é cedo, 0.5 é o centro, 1 é tarde. |
| Extra Delay Ms | Atraso Extra (ms) | `int` | `0` | `0`–`250` | Atraso extra opcional após o timing de sucesso calculado. |
