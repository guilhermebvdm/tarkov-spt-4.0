# RZCustomProfiles

**Versão:** 1.1.0 · **Compatível com:** SPT 4.0.13 · **Autor:** RemzDNB
**Forge:** https://forge.sp-tarkov.com/mod/2614/rzcustomprofiles · **Licença:** MIT

---

## O que é

Gerenciador de templates de perfil para SPT. Permite criar perfis de personagem customizados que aparecem como opção de seleção no launcher, cada um com suas próprias configurações de skills, nível inicial, hideout, traders e inventário.

---

## Como funciona

### Estrutura de arquivos

```
RZCustomProfiles/
├── config/
│   └── masterConfig.json       ← configurações globais do servidor
└── profiles/
    ├── exampleProfile.json     ← template de referência
    └── *.json                  ← um arquivo = um perfil disponível no launcher
```

### Criando um perfil

Crie um `.json` em `profiles/` baseado no `exampleProfile.json`. O perfil aparece no launcher automaticamente quando `"Enabled": true`.

```json
{
  "Enabled": true,
  "BaseProfile": 0,
  "Name": "NomeDoPerfil",
  "Description": "Descrição exibida no launcher",

  "SkillOverrides": {
    "NomeDaSkill": 5
  }
}
```

### Perfis base disponíveis (`BaseProfile`)

| ID | Nome |
|----|------|
| 0 | Standard |
| 1 | Left Behind |
| 2 | Prepare To Escape |
| 3 | Edge Of Darkness |
| 4 | Unheard |
| 5 | Tournament |
| 6 | SPT Developer |
| 7 | SPT Easy Start |
| 8 | SPT Zero to Hero |

---

## Skills disponíveis para override

Todas as skills aceitam valores de **0 a 51** (máximo do jogo).

### Físicas
| Skill | Efeito principal |
|-------|-----------------|
| `Endurance` | Estamina de sprint e caminhada |
| `Strength` | Capacidade de carga e alcance de arremesso |
| `Vitality` | HP máximo e resistência a sangramento |
| `Health` | Regeneração passiva de HP |
| `StressResistance` | Reduz tremores e efeitos de dor |
| `Metabolism` | Dreno de energia e hidratação mais lento |
| `Immunity` | Resistência a veneno e toxinas |

### Mentais
| Skill | Efeito principal |
|-------|-----------------|
| `Perception` | Raio de escuta e detecção de loot |
| `Intellect` | Velocidade de exame e qualidade de reparo |
| `Attention` | Velocidade de saque e itens extras encontrados |
| `Memory` | Velocidade de progressão de todas as skills |
| `Charisma` | Desconto em traders |

### Combate
| Skill | Efeito principal |
|-------|-----------------|
| `Assault` | Maestria com rifles de assalto |
| `Pistol` | Maestria com pistolas |
| `SMG` | Maestria com submetralhadoras |
| `Sniper` | Maestria com rifles de precisão |
| `DMR` | Maestria com DMRs |
| `Shotgun` | Maestria com escopetas |
| `Melee` | Maestria com armas brancas |
| `Throwing` | Maestria com granadas |
| `RecoilControl` | Redução de recuo |
| `AimDrills` | Velocidade de mira (ADS) |
| `TroubleShooting` | Limpeza de encravamentos mais rápida |

### Práticas
| Skill | Efeito principal |
|-------|-----------------|
| `FirstAid` | Uso mais rápido de bandagens e curativos |
| `Surgery` | Uso mais rápido de kits cirúrgicos |
| `FieldMedicine` | Usar meds em movimento |
| `MagDrills` | Velocidade de troca de carregador |
| `CovertMovement` | Redução de ruído ao se mover |
| `Search` | Velocidade de busca em containers |
| `Sniping` | Redução de oscilação de mira em scope |
| `ProneMovement` | Movimento mais rápido de bruços |
| `LightVests` | Bônus com armaduras leves |
| `HeavyVests` | Bônus com armaduras pesadas |
| `WeaponModding` | Velocidade de modificação de armas |
| `AdvancedModding` | Modificações avançadas |
| `WeaponTreatment` | Degradação de armas mais lenta |
| `NightOps` | Desempenho com visão noturna |
| `SilentOps` | Bônus com armas silenciadas |
| `Lockpicking` | Velocidade de lockpick |

### Escambo e Hideout
| Skill | Efeito principal |
|-------|-----------------|
| `Crafting` | Crafting no hideout mais rápido |
| `HideoutManagement` | Produção do hideout mais eficiente |
| `Barter` | Melhores condições de troca |
| `Taskperformance` | Melhores recompensas de quests |

---

**Workflow de desenvolvimento:** ver [WORKFLOW.md](../../WORKFLOW.md).
