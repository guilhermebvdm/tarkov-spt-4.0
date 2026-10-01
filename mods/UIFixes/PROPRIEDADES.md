# Propriedades de Configuração — UIFixes

> Catálogo de opções expostas no menu F12 (BepInEx ConfigurationManager).  
> **Plugin:** `UIFixes` (`Tyfon.UIFixes`) · **Versão:** `5.3.11` · **Fonte:** [`original/src/Settings/Settings.cs`](original/src/Settings/Settings.cs)  
> *Nota: Itens marcados como **(Avançado)** requerem a opção "Advanced Settings" ativada no ConfigurationManager.*

---

## Seções F12

### A. Interface

| Propriedade | Nome em Inglês | Tipo | Padrão | Faixa | Tooltip (pt-BR) | Avançado |
|---|---|---|---|---|---|---|
| Manter Janela de Mensagens Aberta | `Keep Messages Window Open` | `bool` | `true` | — | Após receber itens de uma transferência, reabre a janela de mensagens onde você parou. | Não |
| Preenchimento Automático de Missões | `Autofill Quest Item Turn-ins` | `bool` | `true` | — | Seleciona automaticamente itens correspondentes ao entregar itens de missão. Como pressionar o botão AUTO para você. | Não |
| Submenu de Contexto à Direita | `Context Menu Flyout on Right` | `bool` | `true` | — | Abre o submenu do menu de contexto para a direita, como a BSG planejou originalmente. | Não |
| Menu de Contexto Durante Busca | `Allow Context Menu While Searching` | `bool` | `false` | — | Permite que o menu de contexto funcione enquanto um contêiner está sendo pesquisado. | Não |
| Tamanho da Fonte do Menu de Contexto | `Context Menu Font Size` | `int` | `11` | 8 a 16 | Tamanho da fonte do menu de contexto. Também controla os botões em janelas de inspeção. | Não |
| Ocultar Atalhos Longos da Quickbar | `Hide Long Quickbar Keybinds` | `bool` | `true` | — | Atalhos com nomes maiores que 2 caracteres são abreviados para '...' com tooltip ao passar o mouse. | Não |
| Tempo de Fila de Operações do Servidor | `Server Operation Queue Time` | `int` | `15` | 0 a 60 | Tempo que o cliente espera para agrupar operações de inventário antes de enviar ao servidor (Vanilla é 60). | **Sim** |
| Tempo de Fila de Leitura de Correio | `Mail Read Queue Time` | `int` | `5` | 0 a 80 | Tempo que o cliente espera para marcar mensagens de correio como lidas no servidor. | **Sim** |
| Limitar Arrastes Não Padrão | `Limit Nonstandard Drags` | `bool` | `true` | — | Restringe o arrasto ao botão esquerdo do mouse quando Shift não está pressionado, minimizando conflitos de seleção múltipla. | **Sim** |
| Restaurar Posição de Rolagem Assíncrona | `Restore Async Scroll Positions` | `bool` | `true` | — | Restaura a posição de rolagem em telas carregadas de forma assíncrona. | **Sim** |
| Remover Nome Padrão de Preset de Mag | `Remove Default Mag Preset Name` | `bool` | `true` | — | Remove o texto padrão ao criar presets de carregador. | **Sim** |
| Carregar Preset de Mag em Balas | `Load Mag Preset On Bullets` | `bool` | `true` | — | Permite aplicar preset de carregador diretamente clicando nas balas. | **Sim** |
| Exibir Painel de Convite de Grupo | `Show Group Invite Panel` | `bool` | `true` | — | Controla a exibição do painel de convites de grupo no lobby. | **Sim** |

---

### B. Dialogs

| Propriedade | Nome em Inglês | Tipo | Padrão | Faixa | Tooltip (pt-BR) | Avançado |
|---|---|---|---|---|---|---|
| Confirmações de Preset de Arma | `Show Preset Confirmations` | `enum` | `Always` | — | Define quando exibir confirmações ao salvar ou substituir presets de arma. | Não |
| Salvar Preset com 1 Clique | `One-Click Preset Save` | `bool` | `false` | — | Salva o preset de arma imediatamente ao clicar no botão sem abrir diálogo de confirmação. | Não |
| Exibir Presets Padrão de Fábrica | `Show Stock Presets` | `bool` | `true` | — | Exibe os presets originais padrão de fábrica na lista de presets. | Não |
| Confirmações de Transferência | `Show Transfer Confirmations` | `enum` | `Always` | — | Controla alertas ao transferir itens com correios/comerciantes. | Não |
| Fechar Diálogos Clicando Fora | `Click Out of Dialogs` | `bool` | `true` | — | Permite fechar modais e caixas de diálogo clicando fora da janela. | **Sim** |

---

### C. Gameplay

| Propriedade | Nome em Inglês | Tipo | Padrão | Faixa | Tooltip (pt-BR) | Avançado |
|---|---|---|---|---|---|---|
| Fila de Inputs Segurados | `Queue Held Inputs` | `bool` | `true` | — | Enfileira comandos acionados enquanto outra ação está em execução para disparar assim que liberar. | Não |
| Alternar/Segurar Mira (ADS) | `Toggle or Hold Aim` | `bool` | `false` | — | Suporte híbrido: clique rápido alterna mira; segurar mantém a mira enquanto o botão estiver pressionado. | Não |
| Alternar/Segurar Interação | `Toggle or Hold Interact` | `bool` | `false` | — | Suporte híbrido para botão de interagir. | Não |
| Alternar/Segurar Corrida | `Toggle or Hold Sprint` | `bool` | `false` | — | Suporte híbrido para sprint. | Não |
| Alternar/Segurar Dispositivo Tático | `Toggle or Hold Tactical` | `bool` | `false` | — | Suporte híbrido para lanternas e lasers táticos. | Não |
| Alternar/Segurar Lanterna de Cabeça | `Toggle or Hold Headlight` | `bool` | `false` | — | Suporte híbrido para lanterna de capacete. | Não |
| Alternar/Segurar Óculos/NVG | `Toggle or Hold Goggles` | `bool` | `false` | — | Suporte híbrido para óculos de visão noturna e térmicos. | Não |
| Prevenir Zoom de Luneta no Inventário | `Prevent Scope Zoom From Inventory` | `bool` | `true` | — | Impede que a rolagem do mouse altere o zoom da luneta enquanto a tela de inventário estiver aberta. | Não |
| Correção de Lunetas Variáveis | `Variable Scope Fix` | `bool` | `true` | — | Corrige comportamentos inconsistentes de sensibilidade e zoom em lunetas ópticas variáveis. | **Sim** |
| Modificar Armas Equipadas | `Modify Equipped Weapons` | `bool` | `true` | — | Permite trocar miras e acessórios táticos em armas equipadas fora de raid sem precisar desequipá-las. | Não |
| Modificar Armas em Raid | `Modify Raid Weapons` | `enum` | `TacticalAndScopes` | — | Regula quais partes de arma podem ser modificadas dentro da raid (somente trilhos/miras ou livre). | Não |
| Modificar Placas de Blindagem Equipadas | `Modify Equipped Plates` | `bool` | `true` | — | Permite inserir e retirar placas balísticas de coletes equipados. | Não |
| Remover Ações Desabilitadas | `Remove Disabled Actions` | `bool` | `true` | — | Oculta opções cinzas/desabilitadas do menu de contexto de itens para economizar espaço. | Não |
| Permitir Carregar Munição em Raid | `Enable Load Ammo in Raid` | `bool` | `false` | — | Habilita animações de recarga rápida de munição solta em carregadores em raid. | Não |
| Reatribuir Teclas de Granadas | `Rebind Grenades` | `bool` | `true` | — | Permite mapear granadas individuais para slots de atalho rápido. | Não |
| Reatribuir Teclas de Consumíveis | `Rebind Consumables` | `bool` | `true` | — | Permite mapear consumíveis médicos e comida de forma direta. | Não |

---

### D. Mouse

| Propriedade | Nome em Inglês | Tipo | Padrão | Faixa | Tooltip (pt-BR) | Avançado |
|---|---|---|---|---|---|---|
| Destravar Cursor | `Unlock Cursor` | `bool` | `false` | — | Permite que o cursor do mouse saia da janela do jogo em configurações multi-monitor. | Não |
| Rolagem de Zoom na Arma | `Weapon Zoom Scroll` | `bool` | `true` | — | Habilita zoom na inspeção 3D de armas usando a roda do mouse. | Não |
| Pan e Arrastar Arma | `Weapon Pan Drag` | `bool` | `true` | — | Permite transladar a arma inspecionada clicando e arrastando com o botão do meio. | Não |
| Pan e Zoom de Personagem | `Character Pan and Zoom` | `bool` | `true` | — | Habilita rotação, pan e zoom completo no modelo 3D do personagem no menu principal. | Não |
| Multiplicador de Rolagem | `Mouse Scroll Multiplier` | `int` | `1` | 1 a 10 | Multiplicador de velocidade para rolagem de inventário fora de raid. | Não |
| Multiplicador Separado em Raid | `Use Raid Mouse Scroll Multiplier` | `bool` | `false` | — | Habilita um multiplicador independente de rolagem de inventário durante a raid. | **Sim** |
| Multiplicador de Rolagem em Raid | `Mouse Scroll Multiplier in Raid` | `int` | `1` | 1 a 10 | Velocidade de rolagem de inventário específica para uso dentro de raid. | **Sim** |

---

### E. Interface Keybinds & F. Item Keybinds

| Seção | Nome em Inglês | Tipo | Padrão | Descrição Resumida |
|---|---|---|---|---|
| Interface | `Use Home/End Keys` | `bool` | `true` | Teclas Home/End rolam o inventário instantaneamente para o topo ou fundo. |
| Interface | `Rebind Page Up/Down` | `bool` | `true` | Permite navegar páginas com Page Up e Page Down. |
| Interface | `Search Keybind` | `KeyboardShortcut` | `F` | Atalho para focar na barra de pesquisa de inventário e flea market. |
| Items | `Inspect Keybind` | `KeyboardShortcut` | `I` | Inspeciona o item selecionado. |
| Items | `Open Keybind` | `KeyboardShortcut` | `O` | Abre mochilas, contêineres e caixas de munição. |
| Items | `Examine Keybind` | `KeyboardShortcut` | `M` | Examina itens não identificados. |
| Items | `Top Up Keybind` | `KeyboardShortcut` | `T` | Completa pacotes de munição e pilhas de dinheiro. |
| Items | `Use / Use All Keybind` | `KeyboardShortcut` | `U` / `Shift+U` | Consome ou utiliza o item (meds, comida). |
| Items | `Unload / Reload Keybind` | `KeyboardShortcut` | `Alt+R` / `R` | Descarrega ou recarrega carregadores e armas. |
| Items | `Install / Uninstall Keybind` | `KeyboardShortcut` | — | Equipa ou desequipa mods de arma e equipamentos. |
| Items | `Filter By / Linked / Required` | `KeyboardShortcut` | `F` / `L` / `R` | Atalhos rápidos para pesquisas no Flea Market e comerciantes. |
| Items | `Pin / Lock Keybind` | `KeyboardShortcut` | — | Fixa itens no inventário para impedir venda ou movimento acidental. |

---

### H. Multiselect (Seleção Múltipla)

| Propriedade | Nome em Inglês | Tipo | Padrão | Faixa | Tooltip (pt-BR) | Avançado |
|---|---|---|---|---|---|---|
| Habilitar Seleção Múltipla | `Enable Multiselect` | `bool` | `true` | — | Permite selecionar múltiplos itens no stash desenhando caixas de seleção com o mouse. | Não |
| Multiselect em Raid | `Enable Multiselect in Raid` | `bool` | `false` | — | Habilita a seleção múltipla dentro de raid (padrão desligado para evitar desyncs acidentais). | **Sim** |
| Seleção Múltipla por Clique | `Enable Multi-Click` | `bool` | `true` | — | Permite adicionar itens à seleção segurando Ctrl e clicando individualmente. | **Sim** |
| Tecla da Caixa de Seleção | `Selection Box Key` | `KeyboardShortcut` | `None` | — | Tecla modificadora opcional para ativar o retângulo de seleção. | Não |
| Estratégia de Seleção | `Multi-Select Strategy` | `enum` | `Touch` | — | Define se o retângulo seleciona itens tocados (`Touch`) ou somente totalmente contidos (`Enclosed`). | Não |

---

### I a S. Sistemas Específicos (Stash, Flea, Trading, Containers)

- **Item Swapping:** Permite troca direta de itens ocupando o mesmo espaço físico no inventário arrastando um sobre o outro.
- **Item Stacking:** Empilha automaticamente itens idênticos com Fir (Found in Raid) e sem Fir de forma inteligente.
- **Containers:** Organização de contêineres e caixas de itens com atalhos de transferência direta.
- **Flea Market & Trading:** Foco automático na caixa de preço ao adicionar ofertas, memorização da última busca e navegação aprimorada com teclado.
