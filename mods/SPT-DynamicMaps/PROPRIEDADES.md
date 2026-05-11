# Propriedades — DynamicMaps

**Plugin:** `com.mpstark.DynamicMaps` | versão gerada no build (`BuildInfo.Version`)
**Arquivo:** [original/Config/Settings.cs](original/Config/Settings.cs) · [original/Plugin.cs](original/Plugin.cs)
**Dep:** `com.SPT.custom`

> Itens marcados **(Avançado)** só aparecem com "Advanced settings" ligado no F12.

---

## 1. General

| Nome EN | Nome PT-BR | Tipo | Padrão | Faixa | Tooltip PT-BR |
|---|---|---|---|---|---|
| Replace Map Screen | Substituir Tela de Mapa | `bool` | `true` | — | Se o mapa deve substituir a tela de mapa padrão da BSG (requer trocar de mapa para atualizar) |
| Center on Player Hotkey | Atalho: Centralizar no Player | `KeyboardShortcut` | `Semicolon` | — | Com o mapa aberto, centraliza a visão no jogador |
| Move Map Up Hotkey | Atalho: Mover Mapa para Cima | `KeyboardShortcut` | `UpArrow` | — | Atalho para mover o mapa para cima |
| Move Map Down Hotkey | Atalho: Mover Mapa para Baixo | `KeyboardShortcut` | `DownArrow` | — | Atalho para mover o mapa para baixo |
| Move Map Left Hotkey | Atalho: Mover Mapa para Esquerda | `KeyboardShortcut` | `LeftArrow` | — | Atalho para mover o mapa para a esquerda |
| Move Map Right Hotkey | Atalho: Mover Mapa para Direita | `KeyboardShortcut` | `RightArrow` | — | Atalho para mover o mapa para a direita |
| Move Map Hotkey Speed | Velocidade do Atalho de Mover | `float` | `0.25` | 0.05–2 | Velocidade de movimento do mapa; unidades são % do mapa por segundo |
| Change Map Level Up Hotkey | Atalho: Subir Nível do Mapa | `KeyboardShortcut` | `Period` | — | Atalho para subir o nível do mapa (shift+scroll-up também faz isso na tela do mapa) |
| Change Map Level Down Hotkey | Atalho: Descer Nível do Mapa | `KeyboardShortcut` | `Comma` | — | Atalho para descer o nível do mapa (shift+scroll-down também faz isso) |
| Zoom Map In Hotkey | Atalho: Zoom In | `KeyboardShortcut` | `Equals` | — | Atalho para dar zoom in (scroll-up também faz isso) |
| Zoom Map Out Hotkey | Atalho: Zoom Out | `KeyboardShortcut` | `Minus` | — | Atalho para dar zoom out (scroll-down também faz isso) |
| Zoom Map Hotkey Speed | Velocidade do Atalho de Zoom | `float` | `2.5` | 1–10 | Velocidade de zoom via atalho |
| Dump Info Hotkey **(Avançado)** | Atalho: Dump de Info | `KeyboardShortcut` | `LeftShift+LeftAlt+D` | — | Com o mapa aberto, salva JSONs de MarkerDefs para extrações, loot e switches na pasta raiz do plugin |

---

## 2. Dynamic Markers

| Nome EN | Nome PT-BR | Tipo | Padrão | Faixa | Tooltip PT-BR |
|---|---|---|---|---|---|
| Show Player Marker | Mostrar Marcador do Jogador | `bool` | `true` | — | Se o marcador do jogador deve ser exibido em raid |
| Show Friendly Player Markers | Mostrar Marcadores de Aliados | `bool` | `true` | — | Se marcadores de jogadores aliados devem ser exibidos em raid |
| Show Enemy Player Markers | Mostrar Marcadores de Inimigos | `bool` | `false` | — | Se marcadores de jogadores inimigos devem ser exibidos em raid (geralmente para debug) |
| Show Scav Markers | Mostrar Marcadores de Scavs | `bool` | `false` | — | Se marcadores de scavs inimigos devem ser exibidos em raid (geralmente para debug) |
| Show Boss Markers | Mostrar Marcadores de Bosses | `bool` | `false` | — | Se marcadores de bosses inimigos devem ser exibidos em raid |
| Show Locked Door Status | Mostrar Status de Portas Trancadas | `bool` | `true` | — | Se os marcadores de portas trancadas devem refletir o status com base na posse da chave |
| Show Quests In Raid | Mostrar Quests em Raid | `bool` | `true` | — | Se as quests devem ser exibidas no mapa em raid |
| Show Extracts In Raid | Mostrar Extrações em Raid | `bool` | `true` | — | Se as extrações devem ser exibidas em raid |
| Show Extracts Status In Raid | Mostrar Status das Extrações | `bool` | `true` | — | Se as extrações devem ser coloridas conforme seu status em raid |
| Show Dropped Backpack In Raid | Mostrar Mochila Largada | `bool` | `true` | — | Se as mochilas largadas pelo jogador (não de outros) devem ser exibidas em raid |
| Show BTR In Raid | Mostrar BTR em Raid | `bool` | `true` | — | Se o BTR deve ser exibido em raid |
| Show Airdrops In Raid | Mostrar Airdrops em Raid | `bool` | `true` | — | Se os airdrops devem ser exibidos quando pousarem em raid |
| Show Friendly Corpses In Raid | Mostrar Cadáveres de Aliados | `bool` | `true` | — | Se cadáveres de jogadores aliados devem ser exibidos em raid |
| Show Player-killed Corpses In Raid | Mostrar Cadáveres Mortos pelo Jogador | `bool` | `true` | — | Se cadáveres mortos pelo jogador devem ser exibidos em raid (bosses mortos aparecem em outra cor) |
| Show Friendly-killed Corpses In Raid | Mostrar Cadáveres Mortos por Aliados | `bool` | `true` | — | Se cadáveres mortos por jogadores aliados devem ser exibidos em raid (bosses mortos aparecem em outra cor) |
| Show Boss Corpses In Raid | Mostrar Cadáveres de Bosses | `bool` | `false` | — | Se cadáveres de bosses (exceto os mortos pelo jogador) devem ser exibidos em raid |
| Show Other Corpses In Raid | Mostrar Outros Cadáveres | `bool` | `false` | — | Se outros cadáveres (exceto aliados e mortos pelo jogador) devem ser exibidos em raid |

---

## 3. In-Raid

| Nome EN | Nome PT-BR | Tipo | Padrão | Faixa | Tooltip PT-BR |
|---|---|---|---|---|---|
| Auto Select Level | Seleção Automática de Nível | `bool` | `true` | — | Se o nível do mapa deve ser selecionado automaticamente com base na posição do jogador em raid |
| Auto Center On Player Marker | Centralizar Automaticamente no Jogador | `bool` | `false` | — | Se o marcador do jogador deve ser centralizado ao abrir o mapa em raid |
| Reset Zoom On Center | Resetar Zoom ao Abrir | `bool` | `true` | — | Se o zoom deve ser resetado cada vez que o mapa é aberto em raid |
| Centering On Player Zoom Level | Nível de Zoom ao Centralizar | `float` | `0.15` | 0–1 | Nível de zoom usado ao centralizar no jogador (0 = totalmente afastado, 1 = totalmente aproximado) |
| Peek at Map Shortcut | Atalho: Espreitar o Mapa | `KeyboardShortcut` | `M` | — | Atalho de teclado para espreitar o mapa sem abrir a tela completa |
| Hold for Peek | Segurar para Espreitar | `bool` | `true` | — | Se o atalho deve ser mantido pressionado para o mapa ficar aberto; se desabilitado, alterna ao pressionar |
