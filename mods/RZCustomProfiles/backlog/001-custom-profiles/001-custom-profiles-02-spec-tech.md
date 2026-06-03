# 001 — Perfis customizados temáticos · Spec Técnica

**Mod:** RZCustomProfiles
**Spec funcional:** [001-custom-profiles-01-spec.md](001-custom-profiles-01-spec.md)
**Criado:** 2026-05-17

> **Atenção:** este item não envolve código C#, Harmony patches ou referências ao Assembly do EFT. RZCustomProfiles é um **mod server-side já compilado** (`modded/RZCustomProfiles.dll`) que lê JSONs declarativos em `profiles/`. A entrega é puramente **conteúdo declarativo** (10 arquivos `.json`). A "spec técnica" aqui documenta schema do JSON, mapeamento de skill keys e plano de geração — não há Assembly-CSharp a citar.

## 1. Estratégia

Gerar **10 arquivos `.json` em `modded/profiles/`** seguindo o schema completo de [exampleProfile.json](../../modded/profiles/exampleProfile.json) (formato JSONC — comentários `//` aceitos, confirmado também por [masterConfig.json](../../modded/config/masterConfig.json)). Cada arquivo varia em **cinco pontos**:

1. `Name` — nome em PT-BR da classe
2. `Description` — descrição do estilo de jogo
3. `SkillOverrides` — níveis das skills conforme [planejamento §Modelo de balanceamento](./001-custom-profiles-00-planejamento.md)
4. `HideoutStartingLevels` — `Stash: 1` (padrão) + **1 estação temática em nível 1** conforme [planejamento §Hideout inicial](./001-custom-profiles-00-planejamento.md); Gerente recebe 2 estações
5. `AdditionalStartingItems` — `Enabled: true` + `Items[]` com **todos os TPLs do loadout** (baseline + tema + primary + 3 backups) em formato plano `{ Tpl, Count }` conforme [planejamento §Inventário inicial](./001-custom-profiles-00-planejamento.md). TPLs resolvidos via [anchor-items.json](../anchor-items.json).

Todos os demais campos (`Enabled`, `BaseProfile`, `TradersLoyalty`, `ClearEquipment`, etc.) replicam exatamente o `exampleProfile.json` com valores neutros. Nenhum patch C# é necessário; o `RZCustomProfiles.dll` upstream faz a leitura e exposição ao launcher.

**Limitação aceita (Opção 1 simplificada para loadouts):** o schema do `AdditionalStartingItems.Items` é plano `{ Tpl, Count }` — não suporta itens equipados, aninhados (mag com munição carregada) nem posicionamento em slot. Resultado prático: todos os itens caem **no stash** ao criar o personagem. Jogador monta o loadout manualmente antes da primeira raid.

**Alternativas descartadas:**
- *Omitir campos não-modificados:* mais conciso, mas exigiria validar empiricamente que o parser do mod aceita JSONs parciais. Risco maior que o ganho.
- *Gerar via script:* template + parametrização por classe poderia evitar duplicação, mas 10 arquivos pequenos e revisão manual são mais auditáveis para esta entrega.

## 2. Fontes de schema (substituindo "Pontos de patch")

| Fonte | Uso |
|-------|-----|
| [modded/profiles/exampleProfile.json](../../modded/profiles/exampleProfile.json) | Schema canônico do mod (todos os campos e seus tipos). Copiar 1:1 e modificar os 5 pontos de variação. |
| [README.md §"Skills disponíveis"](../../README.md) | Lista oficial dos 53 nomes de skill aceitos. Mapeamento PT→EN obrigatório. |
| [modded/config/masterConfig.json](../../modded/config/masterConfig.json) | Confirma suporte a JSONC (comentários `//` e vírgulas pendentes). |
| [planejamento §Referência rápida](./001-custom-profiles-00-planejamento.md) | Composição final (skill + nível) das 10 classes após balanceamento ponderado. |
| [planejamento §Hideout inicial](./001-custom-profiles-00-planejamento.md) | Estação temática de hideout por classe. |
| [planejamento §Inventário inicial](./001-custom-profiles-00-planejamento.md) | Loadout completo (baseline + tema + primary + 3 backups) por classe com totais ~2M ₽. |
| [anchor-items.json](../anchor-items.json) | Mapa de IDs simbólicos (`AKM`, `MAG_AKM_30`, `AMMO_762x39_PS`, etc.) → TPL EFT (`bsgId`). Fonte primária para resolver IDs do planejamento. |
| [tools/tarkov-itemdb](../../../../tools/tarkov-itemdb/) | **Fonte autoritativa de metadados por TPL** — 5630 itens com `stackMaxSize`, dimensões, peso, categoria, preços. Necessária para resolver consolidação de itens (ver §7 Riscos e checklist). Acessar via `cache/spt-raw.json` (`items[tpl].stackMaxSize`) ou `data/items.json`. |

## 3. Novas propriedades F12 (BepInEx)

**N/A.** Mod server-side; toda configuração é via JSON. Nenhuma `ConfigEntry` introduzida.

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---------|------|--------|
| `modded/profiles/medicoDeCombate.json` | CRIAR | Skills médicas, `MedStation: 1`, loadout ~1.98M ₽ (AKM + PM + médica reforçada) |
| `modded/profiles/cacador.json` | CRIAR | Skills snipe/mobilidade, `Heating: 1`, loadout ~2.02M ₽ (SV-98 + sustento longo) |
| `modded/profiles/fuzileiro.json` | CRIAR | Skills AR/recoil, `Workbench: 1`, loadout ~1.97M ₽ (AKM + BP + MP-443) |
| `modded/profiles/batedor.json` | CRIAR | Skills recon/silêncio, `Security: 1`, loadout ~1.97M ₽ (AKS-74U + eTG-c) |
| `modded/profiles/operadorNoturno.json` | CRIAR | Skills NightOps/SilentOps, `Generator: 1`, loadout ~2.01M ₽ (AKMS + PBS-1 + PNV-10T) |
| `modded/profiles/armeiro.json` | CRIAR | Skills manutenção, `Workbench: 1`, loadout ~2.02M ₽ (AKM + Weapon repair kit + ferramentas) |
| `modded/profiles/operadorTatico.json` | CRIAR | Skills força/mira, `RestSpace: 1`, loadout ~1.99M ₽ (M4A1 + MP-443 + gear OTAN) |
| `modded/profiles/sobrevivencialista.json` | CRIAR | Skills metabolismo/vitalidade, `WaterCollector: 1`, loadout ~2.01M ₽ (Saiga-12K + sustento extra) |
| `modded/profiles/saqueador.json` | CRIAR | Skills loot/percepção, `Security: 1`, loadout ~1.98M ₽ (Saiga-9 + docs cases + barter) |
| `modded/profiles/gerenteDeOperacoes.json` | CRIAR | Skills hideout/intel, `Generator: 1` + `Heating: 1`, loadout ~1.99M ₽ (Saiga-12K + materiais de hideout) |

Nenhum arquivo `MODIFICAR` — `exampleProfile.json` permanece intocado como template de referência.

## 5. Skeleton do JSON (substituindo "Stubs de código")

Template a ser usado para cada um dos 10 arquivos. Substituir os 3 blocos marcados `[VARIA POR CLASSE]` pelos valores específicos. Demais campos copiados de `exampleProfile.json` sem alteração.

```jsonc
{
  "Enabled": true,
  "BaseProfile": 0,

  // [VARIA POR CLASSE] — Name em PT-BR (com acentos)
  "Name": "Médico de Combate",

  // [VARIA POR CLASSE] — Description em PT-BR (≤ 200 chars para evitar truncamento)
  "Description": "Combat Medic. Sobrevive a ferimentos que matariam outros. Trata dano severo rápido e continua operacional após levar dano.",

  "AllItemsExamined": false,
  "MaxLevel": false,
  "StartingLevel": null,
  "StartingPrestigeLevel": null,
  "MaxSkills": false,

  // [VARIA POR CLASSE] — apenas skills da classe têm valor > 0; demais = 0
  "SkillOverrides": {
    "Endurance": 0, "Strength": 0, "Vitality": 3, "Health": 2,
    "StressResistance": 0, "Metabolism": 0, "Immunity": 0,
    "Perception": 0, "Intellect": 0, "Attention": 0, "Charisma": 0, "Memory": 0,
    "Pistol": 0, "Revolver": 0, "SMG": 0, "Assault": 0, "Shotgun": 0,
    "Sniper": 0, "LMG": 0, "HMG": 0, "Launcher": 0, "AttachedLauncher": 0,
    "Throwing": 0, "Melee": 0, "DMR": 0, "RecoilControl": 0, "AimDrills": 0,
    "TroubleShooting": 0,
    "Surgery": 5, "CovertMovement": 0, "Search": 0, "MagDrills": 0,
    "Sniping": 0, "ProneMovement": 0, "FieldMedicine": 5, "FirstAid": 7,
    "LightVests": 0, "HeavyVests": 0, "WeaponModding": 0, "AdvancedModding": 0,
    "NightOps": 0, "SilentOps": 0, "Lockpicking": 0, "WeaponTreatment": 0,
    "Freetrading": 0, "Auctions": 0, "Cleanoperations": 0, "Barter": 0,
    "Shadowconnections": 0, "Taskperformance": 0,
    "Crafting": 0, "HideoutManagement": 0
  },

  "ClearEquipment": false,
  "ClearStash": false,
  "SecureContainer": 0,

  // [VARIA POR CLASSE] — loadout completo (baseline + tema + primary + 3 backups) em formato plano.
  // TPLs resolvidos via anchor-items.json. Total alvo ~2M ₽.
  "AdditionalStartingItems": {
    "Enabled": true,
    "Items": [
      // Baseline universal (todas as classes)
      { "Tpl": "5449016a4bdc2d6f028b456f", "Count": 100000 },     // Roubles
      { "Tpl": "544fb45d4bdc2dee738b4568", "Count": 1 },          // Salewa
      { "Tpl": "5751a25924597722c463c472", "Count": 2 },          // Army bandage
      { "Tpl": "5af0454c86f7746bf20992e8", "Count": 1 },          // Aluminum splint
      { "Tpl": "544fb37f4bdc2dee738b4567", "Count": 1 },          // Analgin
      { "Tpl": "590c5f0d86f77413997acfab", "Count": 1 },          // MRE
      { "Tpl": "5448ff904bdc2d6f028b456e", "Count": 1 },          // Army crackers
      { "Tpl": "5c0fa877d174af02a012e1cf", "Count": 1 },          // Aquamari
      { "Tpl": "5bffdc370db834001d23eca8", "Count": 1 },          // 6Kh5 Bayonet

      // Item-tema (varia por classe)
      // ...

      // Primary loadout (varia por classe) — arma + mags + munição + armadura + helmet + rig + mochila + meds
      // ...

      // Backup × 3 (varia por classe) — repetir cada item do backup unit 3×, ou usar Count×3
      // ...
    ]
  },

  "TradersLoyalty": {
    "54cb50c76803fa8b248b4571": { "Standing": 0.0, "SalesSum": 0 },
    "54cb57776803fa99248b456e": { "Standing": 0.0, "SalesSum": 0 },
    "579dc571d53a0658a154fbec": { "Standing": 0.0, "SalesSum": 0 },
    "58330581ace78e27b8b10cee": { "Standing": 0.0, "SalesSum": 0 },
    "5935c25fb3acc3127c3d8cd9": { "Standing": 0.0, "SalesSum": 0 },
    "5a7c2eca46aef81a7ca2145d": { "Standing": 0.0, "SalesSum": 0 },
    "5ac3b934156ae10c4430e83c": { "Standing": 0.0, "SalesSum": 0 },
    "5c0647fdd443bc2504c2d371": { "Standing": 0.0, "SalesSum": 0 },
    "6617beeaa9cfa777ca915b7c": { "Standing": 0.0, "SalesSum": 0 },
    "656f0f98d80a697f855d34b1": { "Standing": 0.0, "SalesSum": 0 },
    "638f541a29ffd1183d187f57": { "Standing": 0.0, "SalesSum": 0 }
  },

  // [VARIA POR CLASSE] — Stash: 1 (padrão) + 1 estação temática em nível 1.
  // Gerente de Operações recebe 2 estações.
  "HideoutStartingLevels": {
    "Stash": 1, "Generator": 0, "Vents": 0, "Security": 0, "WaterCloset": 0,
    "Heating": 0, "WaterCollector": 0, "MedStation": 1, "Kitchen": 0,
    "RestSpace": 0, "Workbench": 0, "IntelligenceCenter": 0, "ShootingRange": 0,
    "Library": 0, "ScavCase": 0, "Illumination": 0, "PlaceOfFame": 0,
    "AirFilteringUnit": 0, "SolarPower": 0, "BoozeGenerator": 0,
    "BitcoinFarm": 0, "ChristmasIllumination": 0, "EmergencyWall": 0,
    "Gym": 0, "WeaponStand": 0, "WeaponStandSecondary": 0,
    "EquipmentPresetsStand": 0, "CircleOfCultists": 0
  }
}
```

> Acima é o skeleton do **Médico de Combate**: SkillOverrides com Vitality 3 / Health 2 / Surgery 5 / FieldMedicine 5 / FirstAid 7, HideoutStartingLevels com `MedStation: 1`, e baseline universal de itens. Os blocos `Item-tema`, `Primary loadout` e `Backup × 3` devem ser preenchidos enumerando cada item da tabela §Inventário inicial do planejamento, resolvendo IDs simbólicos via [anchor-items.json](../anchor-items.json).
>
> **Regra de stack (obrigatória):** para cada item, consultar `stackMaxSize` em [tarkov-itemdb](../../../../tools/tarkov-itemdb/cache/spt-raw.json) (`items[tpl].stackMaxSize`):
> - **`stackMax == 1`** (meds, magazines, weapons, mochilas, coletes, capacetes, bayonet) — emitir **N entradas separadas** com `Count: 1` (não consolidar). Ex: 5 IFAKs no total → 5 linhas `{ "Tpl": "...IFAK...", "Count": 1 }`.
> - **`stackMax > 1`** (ammo geralmente 60; Roubles 1.000.000) — emitir **ceil(qty_total / stackMax)** entradas com `Count` igual ao stackMax (última pode ser menor). Ex: 180 rounds PS (stack 60) → 3 linhas `{ "Tpl": "...PS...", "Count": 60 }`. Para Roubles 100.000 (stack 1M) → 1 linha `{ "Tpl": "...rub...", "Count": 100000 }`.
>
> **Por quê:** comportamento do mod ao passar `Count > stackMaxSize` é indefinido — pode descartar o excedente silenciosamente. A regra acima é à prova de falha (worst case = mais linhas no JSON, mas resultado correto in-game).
>
> **Limite de design (`Description`):** ≤ 200 caracteres. Truncamento no launcher ainda não confirmado empiricamente; 200 é margem segura para a maioria dos widgets de seleção de perfil.

### Geração mecânica via script

Em vez de enumerar manualmente os ~50-90 itens de cada loadout (que com a regra de stack acima viraria erro humano garantido), a implementação **estende o [scripts/build-loadouts.js](../../scripts/build-loadouts.js)** existente:

- Esse script já contém as **recipes por classe** (`baseline`, `primary`, `backup × N`, `tema`) com IDs simbólicos e quantidades — fonte de verdade que já gerou as tabelas markdown da §Inventário inicial do planejamento.
- Para esta entrega, adicionar um novo modo `--emit-jsons` (ou criar `scripts/build-profile-jsons.js` derivado) que:
  1. Carrega as recipes de cada classe
  2. Resolve ID simbólico → TPL via [anchor-items.json](../anchor-items.json)
  3. Consulta `stackMaxSize` de cada TPL em [tools/tarkov-itemdb/cache/spt-raw.json](../../../../tools/tarkov-itemdb/cache/spt-raw.json)
  4. Aplica a regra de stack acima e emite `AdditionalStartingItems.Items[]`
  5. Combina com `Name`/`Description`/`SkillOverrides`/`HideoutStartingLevels` da classe
  6. Serializa em `modded/profiles/<classe>.json` (UTF-8 sem BOM, JSONC)
- **Pré-requisito:** atualizar as recipes do script para PT-BR atual (`Sanitarista` → `Médico de Combate`, `Franco-Atirador` → `Caçador`) e adicionar as 7 classes restantes que ainda não estão nele.

Vantagens: (a) recipes versionadas e revisáveis no diff, (b) regeneração trivial após mudanças no planejamento, (c) impossível introduzir erro de stack ou typo de TPL manualmente.

### Composições por classe

| Classe (arquivo) | Name (PT-BR) | SkillOverrides com valor > 0 | Hideout extra (lvl 1) | Loadout total |
|-----------------|--------------|------------------------------|----------------------|--------------:|
| medicoDeCombate | `Médico de Combate` | `FirstAid: 7, FieldMedicine: 5, Surgery: 5, Vitality: 3, Health: 2` | `MedStation` | ~1.977.163 ₽ |
| cacador | `Caçador` | `Sniper: 5, Sniping: 5, ProneMovement: 5, CovertMovement: 4, Perception: 4` | `Heating` | ~2.016.193 ₽ |
| fuzileiro | `Fuzileiro` | `Assault: 10, MagDrills: 8, RecoilControl: 5, AimDrills: 4, Endurance: 3` | `Workbench` | ~1.968.560 ₽ |
| batedor | `Batedor` | `CovertMovement: 8, Perception: 10, Endurance: 5, Search: 10, Attention: 7` | `Security` | ~1.971.098 ₽ |
| operadorNoturno | `Operador Noturno` | `NightOps: 4, SilentOps: 4, CovertMovement: 4, ProneMovement: 3, Perception: 2` | `Generator` | ~2.014.170 ₽ |
| armeiro | `Armeiro` | `WeaponTreatment: 8, TroubleShooting: 4, WeaponModding: 6, Intellect: 6` | `Workbench` | ~2.020.499 ₽ |
| operadorTatico | `Operador Tático` | `Strength: 10, Endurance: 7, AimDrills: 6, MagDrills: 6, LightVests: 2` | `RestSpace` | ~1.985.284 ₽ |
| sobrevivencialista | `Sobrevivencialista` | `Metabolism: 10, Vitality: 5, Immunity: 3, StressResistance: 5, Health: 3` | `WaterCollector` | ~2.009.967 ₽ |
| saqueador | `Saqueador` | `Attention: 10, Search: 10, Perception: 10, Intellect: 10, Memory: 8` | `Security` | ~1.977.305 ₽ |
| gerenteDeOperacoes | `Gerente de Operações` | `Crafting: 10, HideoutManagement: 10, Memory: 10, Intellect: 10, Charisma: 10, WeaponModding: 7` | `Generator` + `Heating` | ~1.991.918 ₽ |

> Loadout total = baseline universal + item-tema + primary + backup×3, conforme tabela do planejamento. Composição completa de itens (TPLs + Count) deve ser extraída por classe da [§Inventário inicial](./001-custom-profiles-00-planejamento.md) durante a implementação.

## 6. Fluxo de dados

```
[repo]                                   [deploy]                            [runtime]
modded/profiles/*.json   --copy-->   BepInEx/plugins/RZCustomProfiles/    --load-->   RZCustomProfiles.dll
                                     profiles/*.json                                  (server startup)
                                                                                              |
                                                                                              v
                                                                    SPT.Server expõe templates --HTTP--> SPT.Launcher
                                                                                                              |
                                                                                                              v
                                                                                  Usuário seleciona "Médico de Combate"
                                                                                                              |
                                                                                                              v
                                                                                  Server cria profile (cópia do Standard
                                                                                  + SkillOverrides aplicado a Profile.Skills.Common[i])
                                                                                                              |
                                                                                                              v
                                                                                  Personagem entra no Hideout com skills pré-elevadas
```

**Não há ponto de execução em raid** — toda aplicação é one-shot no momento da criação do personagem, lado servidor. Após criação, o profile vira um save normal e o RZCustomProfiles não tem mais influência sobre aquele personagem.

## 7. Riscos e dependências

> **Premissa fixa do schema:** todos os 10 perfis usam `BaseProfile: 0` (Standard). Os zeros explícitos em `TradersLoyalty`/`HideoutStartingLevels`/`SkillOverrides` são **identidade do Standard** (que já começa com tudo em zero). Mudar para `BaseProfile` Unheard (4) ou EOD (3) requer auditoria — esses base profiles dão hideout/traders/skills adiantados, e nossos zeros causariam **downgrade silencioso**. Esta premissa precisa ser revalidada se algum perfil futuro mudar `BaseProfile`.

- **Versão upstream do RZCustomProfiles:** `1.1.0 / SPT 4.0.13` (confirmado em [README.md](../../README.md)). Mudanças de schema em versões futuras quebrariam silenciosamente os 10 JSONs.
- **Encoding UTF-8 sem BOM obrigatório:** `Name`/`Description` têm acentos (Médico, Caçador, Operações). Editores Windows (Notepad) podem salvar com BOM ou em CP1252, corrompendo os caracteres. Validar com `file -i` ou hex viewer.
- **Hot-reload vs. restart:** comportamento ainda não validado (corner case na spec funcional). Padrão defensivo é avisar usuário para reiniciar servidor SPT após adicionar/editar perfis.
- **`Description` longa:** widget do launcher pode truncar — limite prático recomendado ≤ 200 caracteres (corner case na spec).
- **Convenção JSONC:** [`masterConfig.json`](../../modded/config/masterConfig.json) confirma suporte a comentários `//` e vírgulas pendentes, mas o parser exato (Newtonsoft.Json com `CommentHandling.Ignore`?) não foi inspecionado — comentários nos 10 JSONs podem ser removidos defensivamente se causarem erro.
- **Conflito com outros mods de perfil:** se outro mod (ex: `Profile Editor`, `SAIN`) também injetar `SkillOverrides`, ordem de carregamento define quem vence. Sem mods conflitantes vendorados em `mods/` no momento.
- **Stash inicial (10×28 slots) pode não comportar o loadout:** 1 primary + 3 backups com armas, mochilas, coletes e meds são ~150-200 slots equivalentes. Validar empiricamente — se transbordar, opções: (a) reduzir backups de 3 para 2, (b) elevar `HideoutStartingLevels.Stash` para 2 ou 3 (mais slots), (c) consolidar itens repetidos via `Count` (já recomendado no skeleton).
- **TPLs inválidos em `AdditionalStartingItems.Items`:** TPLs hex são frágeis a updates do EFT — itens podem ser renomeados/removidos entre patches. Validar a lista da [anchor-items.json](../anchor-items.json) contra a versão atual do EFT 0.16.x antes de gerar os JSONs.
- **Hideout — dependências entre estações:** **mitigado por design.** Apenas estações sem pré-requisitos foram selecionadas (`MedStation`, `Workbench`, `RestSpace`, `WaterCollector`, `Generator`, `Heating`, `Vents`, `Security`). Estações com cadeia de dependências (`ShootingRange`, `IntelligenceCenter`, `ScavCase`, `Library`) foram excluídas do design — ver [planejamento §Hideout inicial](./001-custom-profiles-00-planejamento.md) para detalhes.
- **Sandbox:** edits exclusivamente em `modded/profiles/`. `original/` permanece intocado por convenção do repo ([AGENTS.md §"Hierarquia"](../../../../AGENTS.md)).

## 8. Checklist de implementação

- [ ] Validar versão do mod em `modded/RZCustomProfiles.dll` vs README (confirmar 1.1.0 / SPT 4.0.13).
- [ ] Cross-check: extrair todos os IDs simbólicos usados nas recipes do `build-loadouts.js` e validar que cada um (a) existe como chave em [anchor-items.json](../anchor-items.json) e (b) o `bsgId` resolvido existe como TPL em [tools/tarkov-itemdb/cache/spt-raw.json](../../../../tools/tarkov-itemdb/cache/spt-raw.json) com `stackMaxSize` definido. Pode ser one-liner Node usando ambos JSONs como input.
- [ ] **Smoke test do comportamento `Count > stackMaxSize`:** criar um perfil dummy `_test.json` com `AdditionalStartingItems.Items: [{ Tpl: "<IFAK_TPL>", Count: 5 }]`. Criar personagem, abrir stash, contar IFAKs. Resultado define se a regra de stack do §5 é necessária (esperado: sim — confirmar empiricamente).
- [ ] Verificar pré-requisitos de cada estação de hideout temática (`MedStation`, `ShootingRange`, `Workbench`, `IntelligenceCenter`, `Generator`, `RestSpace`, `WaterCollector`, `ScavCase`). Listar quais exigem estação adicional em nível 1 e ajustar JSONs ou trocar estação.
- [ ] Estender [scripts/build-loadouts.js](../../scripts/build-loadouts.js) (ou criar `scripts/build-profile-jsons.js`): renomear recipes existentes para nomenclatura atual (Médico de Combate, Caçador), adicionar as 7 classes restantes, e implementar emissão de `.json` por classe aplicando a regra de stack do §5.
- [ ] Rodar o script e produzir os 10 arquivos em `modded/profiles/`.
- [ ] Garantir encoding **UTF-8 sem BOM** em todos os 10 arquivos. **Windows (PowerShell):**
  ```powershell
  Get-ChildItem mods/RZCustomProfiles/modded/profiles/*.json | ForEach-Object {
    $bytes = [System.IO.File]::ReadAllBytes($_.FullName)
    if ($bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF) {
      Write-Host "BOM detectado: $($_.Name)" -ForegroundColor Red
    } else { Write-Host "OK: $($_.Name)" }
  }
  ```
  Alternativa com Git Bash/WSL: `file -i mods/RZCustomProfiles/modded/profiles/*.json` deve retornar `charset=utf-8`.
- [ ] Validar sintaxe JSON de cada arquivo (parser tolerante a JSONC).
- [ ] Validar que cada `SkillOverrides` lista exatamente os mesmos 51 nomes do `exampleProfile.json` (sem typos, sem skills extras).
- [ ] Verificar custo ponderado de cada arquivo contra o planejamento (faixa `[28, 32]`). Script ou planilha.
- [ ] Verificar total ₽ de cada loadout contra o planejamento (faixa `[1.95M, 2.05M]`). Script ou planilha.
- [ ] Verificar limite de design: nenhum arquivo com mais de 6 skills > 0, nenhuma skill > 10.
- [ ] Deploy num ambiente de teste SPT 4.0.13 e validar critérios de aceite da spec funcional: (a) 10 perfis no launcher, (b) skills exatas in-game, (c) estação temática do hideout em nível 1, (d) loadout completo depositado no stash (medir slots ocupados — se transbordar, aplicar mitigação documentada em Riscos), (e) traders inalterados.
- [ ] Atualizar [memory/sessions.md](../../memory/sessions.md) com snapshot pós-entrega.

## Histórico

| Data | Evento |
|---|---|
| 2026-05-17 | Spec técnica criada via `/create-technical-spec` — adaptada para entrega de conteúdo declarativo (10 JSONs), sem código C# / Assembly refs / F12 |
| 2026-05-17 | Escopo expandido: incluídos `HideoutStartingLevels` temáticos (1 estação por classe, 2 no Gerente) e `AdditionalStartingItems` com loadout completo (Opção 1 simplificada — itens planos no stash). Skeleton, tabela de composições, arquivos, riscos (stash overflow, TPLs frágeis, dependências de hideout) e checklist atualizados. |
