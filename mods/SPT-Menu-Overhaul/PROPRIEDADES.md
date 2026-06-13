# MoxoPixel-MenuOverhaul — Propriedades F12

> **Plugin:** `MoxoPixel-MenuOverhaul` (`com.moxopixel.menuoverhaul`)<br>
> **Versão:** 1.2.2<br>
> **Fonte:** [original/Utils/Settings.cs](original/Utils/Settings.cs) · binds expostos via [original/Plugin.cs](original/Plugin.cs)<br>

Todas as propriedades abaixo aparecem no menu **F12** (BepInEx ConfigurationManager).

As seções são exibidas na ordem do prefixo numérico (`1.` → `4.`). Dentro de cada seção, a ordem segue o campo `Order` (decrescente), atribuído em `RecalcOrder()` na ordem em que os binds são adicionados no código.

Entradas marcadas **(Avançado)** (`IsAdvanced = true`) **só aparecem com "Advanced settings" ligado** no F12. Toda a seção **4. Advanced** é avançada.

---

## 1. General

| Propriedade (EN) | Tradução (pt-BR) | Tipo | Padrão | Faixa | Tooltip (pt-BR) |
|---|---|---|---|---|---|
| Enable Background | Habilitar plano de fundo | `bool` | `true` | — | Habilita ou desabilita o plano de fundo no menu principal |
| Enable Top Glow | Habilitar brilho superior | `bool` | `true` | — | Habilita ou desabilita o brilho azul/amarelo no topo do menu principal |
| Enable Extra Shadows | Habilitar sombras extras | `bool` | `false` | — | Habilita ou desabilita mais sombras para deixar o jogador no menu mais detalhado |
| Enable Larger Player Model | Habilitar modelo de jogador maior | `bool` | `false` | — | Habilite para deixar o modelo do jogador maior e mais próximo no menu principal |
| Enable High Quality Player Preview | Habilitar pré-visualização do jogador em alta qualidade | `bool` | `true` | — | Habilita renderização mais nítida da pré-visualização do jogador no menu principal (maior custo de GPU). Desabilite para melhor desempenho em sistemas mais fracos |
| Enable Default Player Animation | Habilitar animação padrão do jogador | `bool` | `false` | — | Usa o comportamento padrão de pré-visualização animada do jogador do EFT no menu principal. Desabilite para uma pose estática |
| Enable Menu Button Icons | Habilitar ícones dos botões do menu | `bool` | `true` | — | Mostra ou oculta os ícones dos botões do menu. Quando desabilitado, os ícones ficam ocultos nos estados padrão e de hover |

## 2. Adjustments

| Propriedade (EN) | Tradução (pt-BR) | Tipo | Padrão | Faixa | Tooltip (pt-BR) |
|---|---|---|---|---|---|
| Position Logotype Horizontal | Posição horizontal do logotipo | `float` | `-1.9` | `-10` a `2` | Ajusta a posição horizontal do logotipo |
| Position Logotype Vertical | Posição vertical do logotipo | `float` | `0` | `-2` a `2` | Ajusta o deslocamento vertical do logotipo em relação à sua posição padrão |
| Position Player Model Horizontal | Posição horizontal do modelo do jogador | `float` | `400` | `-600` a `1800` | Ajusta a posição horizontal do modelo do jogador no menu principal |
| Position Player Info Horizontal | Posição horizontal das informações do jogador | `float` | `250` | `-800` a `1200` | Ajusta a posição horizontal do texto de informações do jogador no menu principal |
| Position Player Info Vertical | Posição vertical das informações do jogador | `float` | `0` | `-300` a `300` | Ajusta a posição vertical do texto de informações do jogador no menu principal |
| Scale Background Horizontally | Escala horizontal do plano de fundo | `float` | `1.9` | `0` a `4` | Ajusta a escala horizontal da imagem de plano de fundo |
| Scale Background Vertically | Escala vertical do plano de fundo | `float` | `0.92` | `-1` a `3` | Ajusta a escala vertical da imagem de plano de fundo |
| Rotate Player Model | Rotacionar modelo do jogador | `float` | `180` | `0` a `360` | Ajusta a rotação horizontal do modelo do jogador no menu principal |

## 3. Colors

| Propriedade (EN) | Tradução (pt-BR) | Tipo | Padrão | Faixa | Tooltip (pt-BR) |
|---|---|---|---|---|---|
| Enable Logotype Light Accent Color | Habilitar cor de destaque na luz do logotipo | `bool` | `false` | — | Usa a Cor de Destaque para a luz do logotipo em vez de branco |
| Accent Color | Cor de destaque | `Color` | `RGBA(1, 0.75, 0.3, 1)` (dourado/laranja) | — | A cor de destaque usada para apelido, texto de experiência, botões destacados e tom do brilho superior (o alfa é sempre forçado para `1`) |

## 4. Advanced

> Toda esta seção é **(Avançado)** — só aparece com "Advanced settings" ligado no F12.

| Propriedade (EN) | Tradução (pt-BR) | Tipo | Padrão | Faixa | Tooltip (pt-BR) |
|---|---|---|---|---|---|
| Position Play Button Horizontal | Posição horizontal do botão Jogar | `float` | `250` | `-800` a `1200` | Ajusta a posição horizontal do grupo do botão Jogar (rótulo + ícone) |
| Position Character Button Horizontal | Posição horizontal do botão Personagem | `float` | `250` | `-800` a `1200` | Ajusta a posição horizontal do grupo do botão Personagem (rótulo + ícone) |
| Position Trade Button Horizontal | Posição horizontal do botão Comércio | `float` | `250` | `-800` a `1200` | Ajusta a posição horizontal do grupo do botão Comércio (rótulo + ícone) |
| Position Hideout Button Horizontal | Posição horizontal do botão Esconderijo | `float` | `250` | `-800` a `1200` | Ajusta a posição horizontal do grupo do botão Esconderijo (rótulo + ícone) |
| Position Exit Button Horizontal | Posição horizontal do botão Sair | `float` | `250` | `-800` a `1200` | Ajusta a posição horizontal do grupo do botão Sair (rótulo + ícone) |
| Camera Player Position X | Posição X da câmera do jogador | `float` | `0` | `-3` a `3` | Desloca o `localPosition` X do `Camera_inventory` para a pré-visualização do jogador no menu principal |
| Camera Player Position Y | Posição Y da câmera do jogador | `float` | `0` | `-3` a `3` | Desloca o `localPosition` Y do `Camera_inventory` para a pré-visualização do jogador no menu principal |
| Camera Player Position Z | Posição Z da câmera do jogador | `float` | `0` | `-3` a `3` | Desloca o `localPosition` Z do `Camera_inventory` para a pré-visualização do jogador no menu principal |
| Camera Player Rotation X | Rotação X da câmera do jogador | `float` | `0` | `-180` a `180` | Define o `localRotation` X do `Camera_inventory` para a pré-visualização do jogador no menu principal |
| Camera Player Rotation Y | Rotação Y da câmera do jogador | `float` | `0` | `-180` a `180` | Define o `localRotation` Y do `Camera_inventory` para a pré-visualização do jogador no menu principal |
| Camera Player Rotation Z | Rotação Z da câmera do jogador | `float` | `0` | `-180` a `180` | Define o `localRotation` Z do `Camera_inventory` para a pré-visualização do jogador no menu principal |

---

## Notas de comportamento

- **Accent Color** força o alfa para `1` sempre que alterado (`EnforceAccentColorOpaque`).
- **Position Logotype Vertical** converte valores de saves antigos (escala de mundo legada) para o novo deslocamento `-2`..`2` no carregamento (`NormalizeLegacyLogotypeVerticalSetting`).
