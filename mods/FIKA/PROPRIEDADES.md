# Catálogo de Propriedades — Fika Plugin (BepInEx Configuration)

Este documento cataloga **todas as propriedades configuráveis** expostas pelo **Fika.Core** via BepInEx ConfigurationManager (Menu F12 in-game), organizadas por seção e dispostas rigorosamente na ordem de exibição (`Order` decrescente).

- **Plugin:** `com.fika.core` (`Fika.Core.dll`)
- **Versão Base:** `2.3.9`
- **Código Fonte:** [`original/Fika-Plugin/Fika.Core/FikaConfig.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/FIKA/original/Fika-Plugin/Fika.Core/FikaConfig.cs)

> [!NOTE]
> Itens marcados como **(Avançado)** possuem `IsAdvanced = true` e só são visíveis no menu F12 quando a opção *"Show advanced settings"* estiver marcada.

---

## 🛠️ 1. Advanced (Avançado)

Configurações técnicas para depuração, carregamento de assets paralelos e controle de ciclo de vida.

| Nome (Inglês) | Tradução pt-BR | Tipo | Padrão | Faixa / Opções | Avançado | Tooltip (pt-BR) |
| :--- | :--- | :--- | :--- | :--- | :---: | :--- |
| `Show Official Version` | Versão Oficial | `bool` | `false` | `true` / `false` | **Sim** | Mostra a versão oficial do Tarkov no canto da tela em vez da string de versão do Fika. |
| `Developer Mode` | Modo Desenvolvedor | `bool` | `false` | `true` / `false` | **Sim** | Ativa funcionalidades e comandos extras voltados para depuração e testes. |
| `No AI` | Sem IA | `bool` | `false` | `true` / `false` | **Sim** | Impede totalmente a instanciação e o spawn de qualquer bot de IA na raid. |
| `No Loot` | Sem Loot | `bool` | `false` | `true` / `false` | **Sim** | Impede a geração de loot dinâmico/loose no mapa para acelerar o carregamento em sessões de debug. |
| `Player Load Priority` | Prioridade de Carregamento | `ELoadPriority` | `Low` | `Low`, `Normal`, `High` | **Sim** | Define a prioridade do thread de carregamento de outros jogadores e bots no cliente. |
| `Max Bundle Lock` | Bloqueio Máximo de Bundles | `int` | `5` | `1` a `10` | **Sim** | Quantidade máxima de bundles carregados em paralelo. Aumente se o carregamento de modelos de bots demorar no cliente. (Padrão EFT é 1). |

---

## 👥 2. Coop (Configurações Gerais Cooperativas)

Opções principais de interface, atalhos de raid e notificações cooperativas.

| Nome (Inglês) | Tradução pt-BR | Tipo | Padrão | Faixa / Opções | Avançado | Tooltip (pt-BR) |
| :--- | :--- | :--- | :--- | :--- | :---: | :--- |
| `Auto Use Headless` | Usar Auto Headless | `bool` | `false` | `true` / `false` | Não | Marca automaticamente a opção de hospedar no cliente dedicado Headless se disponível. |
| `Show Feed` | Exibir Feed de Eventos | `bool` | `true` | `true` / `false` | Não | Ativa notificações no topo da tela quando aliados morrem, extraem ou abatem chefes. |
| `Auto Extract` | Auto Extrair | `bool` | `false` | `true` / `false` | Não | Extrai o jogador automaticamente após a contagem regressiva de extração (como host, apenas se não houver clientes ativos). |
| `Show Extract Message` | Mostrar Mensagem de Extração | `bool` | `true` | `true` / `false` | Não | Exibe a mensagem de confirmação de extração após concluir ou morrer na raid. |
| `Extract Key` | Tecla de Extração Manual | `KeyboardShortcut` | `F8` | Tecla de Teclado | Não | Tecla utilizada para forçar a extração da raid a qualquer momento. |
| `Show In-Game Player List` | Exibir Lista de Jogadores | `bool` | `true` | `true` / `false` | Não | Define se a relação de jogadores e bots deve ser exibida ao morrer ou extrair. |
| `Enable Chat` | Habilitar Chat | `bool` | `false` | `true` / `false` | Não | Ativa a caixa de texto de chat in-game. Não pode ser alterado durante a raid. |
| `Chat Key` | Tecla do Chat | `KeyboardShortcut` | `RightControl` | Tecla de Teclado | Não | Tecla de atalho usada para abrir a interface de envio de mensagens do chat. |
| `Enable Online Players` | Habilitar Jogadores Online | `bool` | `true` | `true` / `false` | Não | Exibe a lista e status de outros jogadores conectados no menu principal do jogo. |
| `Online Players Scale` | Escala da Janela Online | `float` | `1.0` | `0.5` a `1.5` | Não | Escala da interface da lista de jogadores online. Requer reabrir o menu para atualizar. |

---

## 🏷️ 3. Coop \| Name Plates (Placas de Nome e Indicadores)

Customização visual das placas de identificação de equipe, oclusão e barras de vida.

| Nome (Inglês) | Tradução pt-BR | Tipo | Padrão | Faixa / Opções | Avançado | Tooltip (pt-BR) |
| :--- | :--- | :--- | :--- | :--- | :---: | :--- |
| `Show Player Name Plates` | Exibir Placas de Nome | `bool` | `true` | `true` / `false` | Não | Exibe as placas flutuantes de identificação sobre a cabeça dos companheiros de equipe. |
| `Hide Health Bar` | Ocultar Barra de Vida | `bool` | `false` | `true` / `false` | Não | Oculta a barra de integridade física, mantendo apenas o nome. |
| `Show HP% instead of bar` | Mostrar HP em % | `bool` | `false` | `true` / `false` | Não | Exibe a porcentagem numérica de vida em vez de uma barra gráfica colorida. |
| `Show Effects` | Exibir Efeitos de Status | `bool` | `true` | `true` / `false` | Não | Exibe ícones de status (sangramento, fratura, dor) abaixo da barra de vida do aliado. |
| `Show Player Faction Icon` | Mostrar Ícone de Facção | `bool` | `true` | `true` / `false` | Não | Exibe o ícone distintivo da facção do jogador (USEC, BEAR, Scav) ao lado do nome. |
| `Hide Name Plate in Optic` | Ocultar em Miras Ópticas | `bool` | `true` | `true` / `false` | Não | Desativa a renderização das placas ao mirar através de miras com lentes PiP (Picture-in-Picture). |
| `Name Plates Use Optic Zoom` | Zoom Óptico nas Placas | `bool` | `true` | `true` / `false` | **Sim** | Projeta a posição correta da placa no campo de visão ampliado da ótica PiP. |
| `Decrease Opacity In Peripheral` | Diminuir Opacidade Periférica | `bool` | `true` | `true` / `false` | Não | Reduz a opacidade das placas de aliados que estiverem fora do centro de visão direta. |
| `Name Plate Scale` | Escala da Placa | `float` | `1.0` | `0.5` a `1.5` | Não | Multiplicador de escala do tamanho das placas de nome. |
| `Opacity in ADS` | Opacidade ao Mirar (ADS) | `float` | `0.75` | `0.1` a `1.0` | Não | Nível de transparência das placas de aliados ao mirar pela alça de mira. |
| `Max Distance to Show` | Distância Máxima de Exibição | `float` | `500.0` | `10.0` a `1000.0`m | Não | Distância máxima (metros) em que as placas são renderizadas (fade inicia na metade). |
| `Minimum Opacity` | Opacidade Mínima | `float` | `0.1` | `0.0` a `1.0` | Não | Limite mínimo de transparência das placas na visão periférica. |
| `Use Occlusion` | Usar Oclusão Física | `bool` | `false` | `true` / `false` | Não | Oculta a placa se o aliado estiver totalmente encoberto por paredes ou obstáculos sólidos. |
| `Full Health Color` | Cor de Vida Máxima | `Color` | `Verde` | Seletor de Cor | Não | Cor exibida na barra de vida quando o jogador está com 100% de HP. |
| `Low Health Color` | Cor de Vida Crítica | `Color` | `Vermelho` | Seletor de Cor | Não | Cor exibida na barra de vida quando o jogador está à beira da morte. |
| `Name Plate Text Color` | Cor do Texto | `Color` | `Branco` | Seletor de Cor | Não | Cor da tipografia com o apelido do jogador. |
| `Show Broken Limbs` | Mostrar Membros Fraturados | `bool` | `false` | `true` / `false` | Não | Mostra os membros do corpo fraturados de outros jogadores ao lado da barra de saúde. |

---

## 📜 4. Coop \| Quest Sharing (Compartilhamento de Missões)

Sincronização de objetivos, recompensas de experiência e progressão mútua.

| Nome (Inglês) | Tradução pt-BR | Tipo | Padrão | Faixa / Opções | Avançado | Tooltip (pt-BR) |
| :--- | :--- | :--- | :--- | :--- | :---: | :--- |
| `Quest Types` | Tipos de Missão | `EQuestSharingTypes` | `All` | `All`, `FindItem`, `Kill`, `Beacon` | Não | Define quais tipos de etapas de quests serão compartilhadas e recebidas entre os membros do grupo. |
| `Show Notifications` | Notificações de Compartilhamento | `bool` | `true` | `true` / `false` | Não | Emite notificação na tela quando um aliado avança ou conclui um objetivo compartilhado com você. |
| `Easy Kill Conditions` | Condições de Morte Facilitada | `bool` | `false` | `true` / `false` | Não | Permite que eliminações de inimigos feitas por seus aliados contem para suas próprias quests se as condições de mapa/arma forem satisfeitas. |
| `Shared Kill Experience` | XP de Eliminação Compartilhada | `bool` | `false` | `true` / `false` | Não | Concede 50% de experiência de combate para você quando um aliado abater um inimigo comum. |
| `Shared Boss Experience` | XP de Chefes Compartilhada | `bool` | `false` | `true` / `false` | Não | Concede 50% de experiência de combate para você quando um aliado abater um Chefe. |

---

## 📍 5. Coop \| Pinging (Sistema de Marcações Táticas)

Sinalizadores 3D no espaço do mundo para orientação de esquadrão.

| Nome (Inglês) | Tradução pt-BR | Tipo | Padrão | Faixa / Opções | Avançado | Tooltip (pt-BR) |
| :--- | :--- | :--- | :--- | :--- | :---: | :--- |
| `Ping System` | Sistema de Marcação | `bool` | `true` | `true` / `false` | Não | Ativa o recurso de emissão e visualização de marcações tridimensionais no mapa. |
| `Ping Button` | Tecla de Marcação | `KeyboardShortcut` | `Semicolon` (`;`) | Tecla de Teclado | Não | Tecla utilizada para colocar um marcador na posição onde a retícula estiver apontando. |
| `Ping Color` | Cor da Marcação | `Color` | `Branco` | Seletor de Cor | Não | Cor visual do marcador que você transmite para os demais membros do grupo. |
| `Ping Size` | Tamanho do Marcador | `float` | `1.0` | `0.1` a `2.0` | Não | Multiplicador de escala da geometria do marcador visual. |
| `Ping Time` | Duração do Ping | `int` | `3` | `2` a `10`s | Não | Tempo de permanência do marcador na tela antes de desaparecer. |
| `Play Ping Animation` | Animar Gesto de Apontar | `bool` | `false` | `true` / `false` | Não | Executa o gesto tático de apontar com o braço esquerdo do operador ao criar um ping. |
| `Show Ping During Optics` | Mostrar Ping em Ópticas | `bool` | `false` | `true` / `false` | Não | Mantém o marcador visível mesmo quando o operador estiver em visada com mira telescópica. |
| `Ping Use Optic Zoom` | Zoom Óptico no Marcador | `bool` | `true` | `true` / `false` | **Sim** | Projeta o marcador na escala e posição da lente PiP ao mirar. |
| `Ping Scale With Distance` | Escala por Distância | `bool` | `true` | `true` / `false` | **Sim** | Ajusta o tamanho relativo do ícone para manter boa visibilidade a longas distâncias. |
| `Ping Minimum Opacity` | Opacidade Mínima | `float` | `0.05` | `0.0` a `0.5` | **Sim** | Limite mínimo de transparência do marcador quando olhado no centro da mira. |
| `Show Ping Range` | Exibir Distância Métrica | `bool` | `false` | `true` / `false` | Não | Exibe a distância exata em metros entre você e o ponto marcado. |
| `Ping Sound` | Som da Marcação | `EPingSound` | `SubQuestComplete` | Efeitos de Áudio | Não | Áudio reproduzido para a equipe ao ser transmitido um novo ping. |

---

## 🩹 6. Coop \| Revival (Sistema de Reanimação)

Controles do estado de incapacitação (downed) e reanimação de operadores.

| Nome (Inglês) | Tradução pt-BR | Tipo | Padrão | Faixa / Opções | Avançado | Tooltip (pt-BR) |
| :--- | :--- | :--- | :--- | :--- | :---: | :--- |
| `Give Up Key` | Tecla de Desistir | `KeyboardShortcut` | `End` | Tecla de Teclado | Não | Tecla que o jogador incapacitado deve segurar para desistir e encerrar a sangria imediatamente. |

---

## 📹 7. Coop \| Debug (Câmera Livre e Ferramentas)

Recursos de câmera livre e inspeção de raid.

| Nome (Inglês) | Tradução pt-BR | Tipo | Padrão | Faixa / Opções | Avançado | Tooltip (pt-BR) |
| :--- | :--- | :--- | :--- | :--- | :---: | :--- |
| `Free Camera Button` | Botão Câmera Livre | `KeyboardShortcut` | `F9` | Tecla de Teclado | Não | Tecla de atalho para alternar entre o controle do personagem e o voo da FreeCam. |
| `AZERTY Mode` | Modo Teclado AZERTY | `bool` | `false` | `true` / `false` | Não | Adapta os direcionais de movimentação da FreeCam para teclados de padrão francês AZERTY. |
| `Drone Mode` | Modo Drone | `bool` | `false` | `true` / `false` | Não | Restringe a movimentação vertical e horizontal simulando o voo estilizado de um drone. |
| `Keybind Overlay` | Overlay de Atalhos | `bool` | `true` | `true` / `false` | Não | Exibe uma lista com todos os comandos da FreeCam na lateral da tela durante o uso. |

---

## 🌐 8. Network (Configuração de Rede e Conectividade UDP)

Parâmetros de transporte UDP LiteNetLib, perfuração NAT, UPnP e endereçamento.

| Nome (Inglês) | Tradução pt-BR | Tipo | Padrão | Faixa / Opções | Avançado | Tooltip (pt-BR) |
| :--- | :--- | :--- | :--- | :--- | :---: | :--- |
| `Force IP` | Forçar IP Público | `string` | `""` | Endereço IP / Hostname | Não | Força o servidor ao hospedar a anunciar este IP específico para o backend SPT. Deixe vazio para autodetectar. |
| `Force Bind IP` | Forçar Interface Local | `string` | `"Disabled"` | Lista de Adaptadores | Não | Força o socket do servidor a escutar em uma placa de rede/VPN específica (ex: Radmin/Tailscale). |
| `UDP Port` | Porta UDP | `ushort` | `25565` | `0` a `65535` | Não | Porta de rede UDP utilizada para transmissão em tempo real dos pacotes de gameplay e física. |
| `Use Port Mapping` | Usar Mapeamento UPnP | `bool` | `false` | `true` / `false` | **Sim** | Tenta configurar regras de redirecionamento de porta automaticamente via UPnP no roteador. |
| `Use NAT Punching` | Usar NAT Punching | `bool` | `false` | `true` / `false` | **Sim** | Utiliza sinalização NAT Punch para estabelecer conexão direta mesmo com portas fechadas no host. |
| `Use Fika NAT Punch Server` | Usar Servidor NAT Fika | `bool` | `false` | `true` / `false` | **Sim** | Conecta ao servidor público de perfuração NAT do projeto Fika (requer `Use NAT Punching` ativo). |
| `Connection Timeout` | Tempo Limite de Conexão | `int` | `30` | `5` a `60`s | Não | Tempo em segundos sem resposta antes de desconectar um cliente ou host. |
| `Send Rate` | Taxa de Envio de Posição | `ESendRate` | `Medium` | `Low` (10Hz), `Medium` (20Hz), `High` (30Hz) | Não | Frequência de despacho dos pacotes de sincronização de movimento. Afeta host e todos os clientes. |
| `Allow VOIP` | Permitir Chat de Voz (VOIP) | `bool` | `false` | `true` / `false` | Não | Ativa o transporte de áudio posicional integrado via UDP quando você hospedar a raid. |

---

## ⚙️ 9. Gameplay (Jogabilidade e Integridade de Estado)

Regras de simulação de inventário e sobrevivência de bots.

| Nome (Inglês) | Tradução pt-BR | Tipo | Padrão | Faixa / Opções | Avançado | Tooltip (pt-BR) |
| :--- | :--- | :--- | :--- | :--- | :---: | :--- |
| `Strict Inventory Sync` | Sincronização Estrita de Inventário | `bool` | `true` | `true` / `false` | Não | Exige confirmação do servidor para todas as transições de itens/armas antes de permitir novos disparos/ações, eliminando desyncs graves. |
| `Disable Bot Metabolism` | Desativar Metabolismo de Bots | `bool` | `false` | `true` / `false` | Não | Impede que a IA sofra perda contínua de hidratação/energia, prevenindo mortes involuntárias de chefes e bots em raids extensas. |
