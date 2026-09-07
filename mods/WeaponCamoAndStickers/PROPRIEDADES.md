# Propriedades de Configuração — WeaponCamoAndStickers

> Catálogo de opções expostas no menu F12 (BepInEx ConfigurationManager).
> **Plugin:** `7Bpencil.WeaponCamoAndStickers` (`7Bpencil.WeaponCamoAndStickers`) · **Versão:** `1.17.1` · **Fonte:** [`original/Client/WeaponCamoAndStickers/Plugin.cs`](original/Client/WeaponCamoAndStickers/Plugin.cs)
> *Nenhum `ConfigurationManagerAttributes` (Order/IsAdvanced) é usado no código-fonte — todas as entradas ficam na seção única `Main`, na ordem em que são registradas em `Awake()`. Nenhuma é marcada como Avançada.*

---

## Seções F12

### Main

| Propriedade | Nome em Inglês | Tipo | Padrão | Faixa | Tooltip (pt-BR) |
|---|---|---|---|---|---|
| Vídeo — Reproduzir Áudio | `Video \| Play Audio` | `bool` | `false` | — | *(sem tooltip no original)* |
| Editor de Camuflagem — Escala da Interface | `Camo Editor \| UI Scale` | `float` | `1.0` | 0.5 a 2 | *(sem tooltip no original)* |
| Editor de Camuflagem — Atalho: Mover | `Camo Editor \| Keybinds \| Move` | `KeyboardShortcut` | `G` | — | *(sem tooltip no original)* |
| Editor de Camuflagem — Atalho: Rotacionar | `Camo Editor \| Keybinds \| Rotate` | `KeyboardShortcut` | `R` | — | *(sem tooltip no original)* |
| Editor de Camuflagem — Atalho: Escalar | `Camo Editor \| Keybinds \| Scale` | `KeyboardShortcut` | `S` | — | *(sem tooltip no original)* |
| Raid — Chance de Camuflagem: Goons | `Raid \| Camo Spawn Chance \| Goons` | `int` | `100` | 0 a 100 | *(sem tooltip no original)* |
| Raid — Chance de Camuflagem: PMC | `Raid \| Camo Spawn Chance \| PMC` | `int` | `33` | 0 a 100 | *(sem tooltip no original)* |
| Raid — Chance de Camuflagem: Outros Chefes | `Raid \| Camo Spawn Chance \| Other Bosses` | `int` | `50` | 0 a 100 | *(sem tooltip no original)* |
| Raid — Chance de Camuflagem: Scavs | `Raid \| Camo Spawn Chance \| Scavs` | `int` | `0` | 0 a 100 | *(sem tooltip no original)* |

**Nota:** o autor não preencheu texto de tooltip (`ConfigDescription` vazio, `""`) para nenhuma destas entradas — o F12 exibirá apenas o nome, sem descrição ao passar o mouse.
