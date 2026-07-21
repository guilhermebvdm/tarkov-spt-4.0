# Propriedades F12 — Discord Raid Map

> **Plugin:** Discord Raid Map (`com.fiodor.discordraidmap`)<br>
> **Versão:** 1.0.0<br>
> **Fonte:** [original/Settings.cs](original/Settings.cs) · [original/Plugin.cs](original/Plugin.cs)<br>

Propriedades expostas no menu F12 (BepInEx ConfigurationManager), agrupadas pela `section` (1º argumento de `Config.Bind`) e ordenadas por `Order` decrescente (ordem de exibição real na tela).

Nenhuma propriedade é marcada como **(Avançado)** (`IsAdvanced = true`) neste mod — todas aparecem sem precisar ligar "Advanced settings" no F12.

## Seção: Discord

| Nome (EN) | Tradução (pt-BR) | Tipo | Padrão | Faixa | Tooltip (pt-BR) |
|---|---|---|---|---|---|
| Webhook Url | URL do Webhook | `string` | `""` (vazio) | — | URL do webhook do Discord usada para a mensagem do mapa de raid. |
| Message Name | Nome da Mensagem | `string` | `Raid Map` | — | Nome usado para a mensagem de mapa do Discord. |
| Update Interval Seconds | Intervalo de Atualização (segundos) | `int` | `5` | 2 – 120 | Com que frequência editar a mensagem de mapa do Discord. |

## Seção: Map Text (Texto do Mapa)

| Nome (EN) | Tradução (pt-BR) | Tipo | Padrão | Faixa | Tooltip (pt-BR) |
|---|---|---|---|---|---|
| Map Text Font | Fonte do Texto do Mapa | `string` | `DelaGothicOne-Regular.ttf` | Lista dinâmica: `Default` + arquivos `.ttf`/`.otf` em `Assets\Fonts` | Arquivo de fonte usado para o texto do mapa. Adicione arquivos `.ttf` ou `.otf` em `Assets\Fonts` e reinicie o jogo para atualizar esta lista. |
| Map Text Font Size | Tamanho da Fonte do Texto do Mapa | `int` | `36` | 1 – 100 | Tamanho da fonte usada para o texto do mapa. |

## Seção: Markers (Marcadores)

| Nome (EN) | Tradução (pt-BR) | Tipo | Padrão | Faixa | Tooltip (pt-BR) |
|---|---|---|---|---|---|
| Marker Display Size | Tamanho de Exibição dos Marcadores | `int` | `60` | 16 – 256 | Tamanho em pixels usado para desenhar os ícones de marcador no mapa. Os PNGs de origem podem ser maiores para uma redução de escala mais nítida. |

---

**Notas:**

- A lista de opções de **Map Text Font** é montada em runtime por `GetFontChoices()`: sempre inclui `Default` (fonte bitmap embutida) mais qualquer `.ttf`/`.otf` presente em `Assets\Fonts`. Novos arquivos só aparecem após reiniciar o jogo.
- Alterações em **Update Interval Seconds**, **Map Text Font**, **Map Text Font Size** e **Marker Display Size** disparam `SettingChanged` e são aplicadas ao vivo durante a raid (ver `Plugin.cs`).
