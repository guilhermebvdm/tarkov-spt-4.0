# Propriedades F12 — MegaMod (CWX-MegaMod)

Configurações expostas no menu F12 (BepInEx ConfigurationManager).  
Plugin: `com.cwx.megamod` · Versão: `4.0.1`  
Arquivo de origem: [`original/src/CWX-MegaMod/MegaMod.cs`](original/src/CWX-MegaMod/MegaMod.cs)

---

## 1- All Mods

| Nome | Tradução pt-BR | Tipo | Padrão | Faixa | Tooltip (pt-BR) |
|---|---|---|---|---|---|
| `ReserveAlarmChanger - On/Off` | Alterador de Alarme da Reserve | `bool` | `false` | - | Ativa o alterador de som de alarme da Reserve para o arquivo na pasta do plugin (roda apenas no início da raid). |
| `BushWhacker - On/Off` | Desativador de Lentidão em Arbustos | `bool` | `false` | - | Remove a desaceleração ao passar por dentro de arbustos/moitas. |
| `GrassCutter - On/Off` | Cortador de Grama | `bool` | `false` | - | Remove toda a grama do mapa via GPUInstancerDetailManager. |
| `MasterKey - On/Off` | Chave Mestra | `bool` | `false` | - | Altera portas trancadas para aceitarem a chave configurada em MasterKeyToUse. |
| `EnvironmentEnjoyer - On/Off` | Desativador de Ambiente | `bool` | `false` | - | Desativa árvores e arbustos do mapa. |
| `SpaceUser - On/Off` | Usar Barra de Espaço | `bool` | `false` | - | Permite usar a barra de espaço para aceitar no Flea Market e dividir pilhas de itens. |
| `TradingPlayerView - On/Off` | Visualizador de Jogador nos Traders | `bool` | `false` | - | Altera a visualização do personagem nas telas de negociação dos traders. |
| `PainkillerDesat - On/Off` | Desativador de Efeito de Analgésico | `bool` | `false` | - | Remove a distorção visual e dessaturação causada pelo uso de analgésicos. |

---

## 2- Debug Mods

| Nome | Tradução pt-BR | Tipo | Padrão | Faixa | Tooltip (pt-BR) |
|---|---|---|---|---|---|
| `InstantSearch - On/Off` | Busca Instantânea | `bool` | `false` | - | Ativa busca e exame instantâneo de contêineres e bolsos. |
| `FoodWater - On/Off` | Hidratação e Energia Infinita | `bool` | `false` | - | Remove o dreno de hidratação e energia do personagem. |
| `LootLoss - On/Off` | Remoção de Loot | `bool` | `false` | - | Remove o loot do mapa durante o carregamento da raid. |
| `InventoryViewer - On/Off` | Visualizador de Inventário | `bool` | `false` | - | Altera a visualização do inventário para exibir todos os contêineres abertos. |
| `GodMode - On/Off` | Modo Deus | `bool` | `false` | - | Torna o jogador imortal a dano de combate. |
| `CameraShake - On/Off` | Desativar Tremor de Câmera | `bool` | `false` | - | Remove o tremor e balanço de câmera. |
| `ThermalMode - On/Off` | Modo Térmico | `bool` | `false` | - | Ativa visão térmica na visão normal. |
| `BetterThermalMode - On/Off` | Modo Térmico Aprimorado | `bool` | `false` | - | Desativa desfoque, ruído e granulação da visão térmica. |
| `NightVisionMode - On/Off` | Modo Visão Noturna | `bool` | `false` | - | Ativa visão noturna na visão normal. |

---

## 3- MasterKey

| Nome | Tradução pt-BR | Tipo | Padrão | Faixa | Tooltip (pt-BR) |
|---|---|---|---|---|---|
| `MasterKeyToUse` | Chave Mestra a Utilizar | `EMasterKeys` | `Yellow` | - | Define qual cartão/chave abre todas as portas trancadas. |
