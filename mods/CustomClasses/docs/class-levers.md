# Design das classes — redesign 6 classes (skills + signatures)

> **Data:** 2026-06-20<br>
> **Status:** 🟢 Vivo<br>
> **Responsáveis:** mdj, Guilherme<br>
> **Referências:** [class-skill-catalog.md](./class-skill-catalog.md), [balance-model.md](./balance-model.md), [class-archetypes.md](./class-archetypes.md)<br>

---

Documento **consolidado** do redesign de **11→6 classes**. É a fonte de verdade do *design de papel + números* das classes: roster, arquitetura, a **matriz calibrada** (aprovada) e as **camadas** (skills, skills custom, patches, loadout, hideout) de cada classe.

A matriz vive de forma **reproduzível** em [`scripts/class-matrix.mjs`](../scripts/class-matrix.mjs) (espelha as tabelas abaixo; `node mods/CustomClasses/scripts/class-matrix.mjs` recalcula custo/netMult a partir de [`skill-weights.mjs`](../scripts/skill-weights.mjs) e faz cross-check contra os valores aprovados). **Os números deste doc e do `.mjs` são a verdade — não re-derivar de memória.**

## Glossário de camadas

Cada classe é definida por camadas, marcadas por emoji ao longo do doc:

| Emoji | Camada | Mecanismo |
|---|---|---|
| 🎯 | **skill** | `skills` (nível inicial) + `skillMultipliers` (XP-mult). É a **matriz**. Zero patch. |
| 🧪 | **skill custom** | revive um slot `ESkillId` morto como skill nova (padrão Skills-Extended), efeito lido de `mgr.<skill>.Level` num patch. Aparece no menu de Skills, sobe com XP, é gatilhável por `skillMultipliers`. |
| 🔧 | **patch per-player** | Harmony keyed na classe (`Info.GameVersion`), para o que skill não cobre (velocidade, ADS, cura instantânea, dano recebido). Flat **ou** escalando com uma skill existente lida no patch. |
| 🎒 | **loadout** | item/estação inicial no inventário. |
| 🏠 | **hideout** | estação com `−50%` de tempo. |
| 🌐 | **global** | afeta **todos** os players (ex.: revelar valor ₽). **NÃO** é lever de classe. |

---

## 1. Arquitetura "tudo-é-skill-real" (decisões travadas — não re-litigar)

1. **Tudo-é-skill-real** — abandonado o spawn-buff efêmero. Diferenciação = (a) **skills existentes** via `skills` (nível inicial) + `skillMultipliers` (XP-mult, `×0` desabilita), (b) **skills custom** (padrão SE) só para efeitos novos. **Persistem no perfil e aparecem na tela de Skills.**
2. **Skill custom SEM prepatcher** — reviver slot `ESkillId` morto + efeito via **patch lendo `mgr.<skill>.Level`** (estilo `UpdateWeaponsPatch` do SE). Evita `EBuffId` novo (prepatcher Mono.Cecil é frágil/ofuscado). Pipeline do SE mapeado na Fase 0.
3. **"Desabilitada por classe" = `skillMultipliers[skill]=0` + início 0** → `OnTriggerPatch.cs:33` (`val *= factor`, clamp ≥0) zera o XP → a skill congela em 0 → o efeito lê 0 → nada acontece. **Sem mecanismo novo.**
4. **Gating de classe em runtime** = ler `player.Profile.Info.GameVersion` (= edition = nome da classe CustomClasses). Funciona para qualquer player (`EFT/Profile.cs:239`).
5. **REVIVE morreu** — FIKA 2.2.6 e EFT 0.16.x **não têm sistema de "downed"/revive** (`FikaPlayer.cs:971`: morte é terminal). **NÃO construir do zero.** Por isso o Médico virou **Médico de Combate** (cura quase instantânea), não reanimador.
6. **Stances mod compõe** (multiplica) em velocidade/inércia/stamina-de-perna; **conflita em stamina de braço** (stances seta `GetHandsRestorationFunc`→0 para o MainPlayer, `Priority.Low`). 2 levers nossos caem nessa zona ⚠️ (ver §6).
7. **netMult ≠ poder real** — peso baixo + impacto alto (Strength 0.47) é subestimado; peso alto (Immunity/Vests/DMR 3.75) domina o número. netMult é **guia**; cruzar com velocidade de skill (§3) + impacto real ([class-skill-catalog.md](./class-skill-catalog.md)).

---

## 2. Roster (6 classes)

| pt (in-game) | en | Pilar | Signature |
|---|---|---|---|
| **Médico** | Medic | Suporte | 🔧 Médico de Combate (cura quase instantânea, +50% HP, cura andando/atirando) |
| **Fuzileiro** | Rifleman | Combate | 🧪 Adrenalina (pós-abate: −recuo/−recarga/−ADS por `3s + 0.5s/nv`) |
| **Caçador** | Hunter | Precisão | 🧪 Fôlego de Aço (prende respiração `×(1+0.1·nv) ≤ ×3`, −sway) |
| **Fantasma** | Ghost | Furtividade | 🔧 Execução (dano melee ×20) |
| **Saqueador** | Looter | Pilhagem | 🧪 Mãos Rápidas (busca/loot +rápido) + 🧪 Pack Mule |
| **Tanque** | Tank | Resistência | 🔧 Couraça (dano recebido `×(1−[0.05→0.25])`) + 🧪 Pack Mule |

`displayName` guarda os dois nomes; o launcher mostra conforme `config/settings.jsonc` (hoje `"pt"`).

---

## 3. Tiers de velocidade de skill

> Validado no `globals` `SkillsSettings`. Calibração do balance: **buff forte vale em 🐌** (o mult importa onde o grind é real); **debuff só morde em 🐇/🚶** (em 🐌 já é lento, o debuff é teatro).

- 🐇 **Sobe-fácil:** Perception, Metabolism, Attention, Search, Charisma, Intellect
- 🚶 **Média:** Assault, Pistol, SMG, Shotgun, DMR, AimDrills, MagDrills, StressResistance, **Endurance**, **CovertMovement** *(Endurance/CovertMovement não são fast: Endurance 0.04/ação, CovertMovement 0.025/passo)*
- 🐌 **Grind:** Strength, Sniper, Vitality, Health, Immunity, Melee, Throwing, Surgery, FirstAid, FieldMedicine, Light/HeavyVests, TroubleShooting, HideoutManagement, Crafting + gems (Lockpicking, SilentOps, ProneMovement, ShadowConnections, UsecArsystems, BearAksystems)

---

## 4. Matriz calibrada (camada 🎯 skill) — APROVADA

> `×` = multiplicador de XP · `Lv` = nível inicial (ausente = só multiplicador, sem pontos) · 🟢 buff 🔴 debuff. **Balance: topo (Méd/Fuz/Caç/Fan) ~+6 · base (Saq/Tan) ~+4** (Saq/Tan compensados pelas signatures 🔧🧪, fora do netMult). Reproduzível em [`scripts/class-matrix.mjs`](../scripts/class-matrix.mjs).

> **Recalibrada na Fase 4** (cross-check do `class-matrix.mjs` ✅, sem flags): (a) pesos das gems derivados por categoria — Lockpicking/SilentOps/ProneMovement/ShadowConnections → P (0.94), UsecArsystems/BearAksystems → C (1.50); (b) **SMG e AttachedLauncher removidos** (inertes no `globals` — ver §7); (c) custos aparados para caber em 28–32.

| Classe | custo | netMult | faixa |
|---|---|---|---|
| 🩺 Médico | 31.87 | +6.12 | topo |
| 🔫 Fuzileiro | 30.51 | +6.27 | topo |
| 🎯 Caçador | 31.40 | +6.21 | topo |
| 👻 Fantasma | 30.14 | +6.12 | topo |
| 🎒 Saqueador | 28.23 | +4.09 | base |
| 🛡️ Tanque | 30.29 | +4.28 | base |

### 🩺 Médico — netMult +6.12 · custo 31.87
- 🟢 FirstAid 🐌×2.5 Lv5 · FieldMedicine 🐌×2 Lv5 · Surgery 🐌×2 Lv4 · Vitality 🐌×2 Lv4 · HideoutManagement 🐌×1.5 Lv6 · Crafting 🐌×1.5 · Immunity 🐌×1.2 Lv1
- 🔴 Assault 🚶×0.6 · AimDrills 🚶×0.7 · CovertMovement 🚶×0.7 · Perception 🐇×0.8

### 🔫 Fuzileiro — netMult +6.27 · custo 30.51
- 🟢 Assault 🚶×2.5 Lv7 · UsecArsystems 🐌×2 Lv3 · BearAksystems 🐌×2 Lv3 · AimDrills 🚶×1.5 Lv5 · MagDrills 🚶×1.5 Lv4 · Endurance 🚶×1.5 Lv5 · StressResistance 🚶×1.3 · Pistol 🚶×1.2
- 🔴 CovertMovement 🚶×0.6 · Attention 🐇×0.7 · Search 🐇×0.8

### 🎯 Caçador — netMult +6.21 · custo 31.40
- 🟢 Sniper 🐌×2.5 Lv7 · DMR 🚶×1.5 Lv2 · AimDrills 🚶×1.5 · ProneMovement 🐌×1.5 Lv3 · Pistol 🚶×1.3 Lv2 · Perception 🐇×1.3 Lv2 · Metabolism 🐇×1.3 · CovertMovement 🚶×1.2 Lv3
- 🔴 Assault 🚶×0.6

### 👻 Fantasma — netMult +6.12 · custo 30.14
- 🟢 SilentOps 🐌×2.5 Lv6 · Pistol 🚶×1.8 Lv2 *(pistola silenciada — arma do Fantasma; SMG saiu, era inerte)* · CovertMovement 🚶×1.5 Lv6 · Perception 🐇×1.5 Lv5 *(exceção aceita: sentido-assinatura)* · Melee 🐌×1.5 Lv3 · LightVests 🐌×1.3 · ProneMovement 🐌×1.5 · Lockpicking 🐌×1.3 Lv3
- 🔴 Assault 🚶×0.6 · StressResistance 🚶×0.7 · Shotgun 🚶×0.7

### 🎒 Saqueador — netMult +4.09 · custo 28.23
- 🟢 Lockpicking 🐌×3 Lv8 · Strength 🐌×3 Lv7 · ShadowConnections 🐌×2.5 Lv6 · Attention 🐇×1.3 Lv8 · Perception 🐇×1.3 Lv5 · Search 🐇×1.3 Lv6 · HideoutManagement 🐌×1.2 · Intellect 🐇×1.2 · Charisma 🐇×1.2
- 🔴 Assault 🚶×0.6 · AimDrills 🚶×0.7 · StressResistance 🚶×0.7

### 🛡️ Tanque — netMult +4.28 · custo 30.29
- 🟢 StressResistance 🚶×2 · HeavyVests 🐌×1.5 Lv3 · Health 🐌×1.5 Lv4 · Vitality 🐌×1.5 Lv4 · Strength 🐌×1.5 Lv5 · Shotgun 🚶×1.5 Lv1 · Throwing 🐌×1.5 Lv1 · Melee 🐌×1.5 *(GL/AttachedLauncher saiu da matriz, era inerte → vira 🔧 signature)*
- 🔴 Metabolism 🐇×0.5 · CovertMovement 🚶×0.5 · AimDrills 🚶×0.7 · Pistol 🚶×0.7 · DMR 🚶×0.7

---

## 5. Camadas além do 🎯 skill (signatures 🔧/🧪 + 🎒 loadout + 🏠 hideout)

> Padrão hideout: cada classe = **1 estação inicial 🎒 + 1 estação −50% tempo 🏠**.

- **🩺 Médico** — 🔧 cura tempo ×0.3, +50% HP, sem lock de movimento/arma · 🔧 membro quebrado cura ×0.5 tempo · 🏠 MedStation −50% · 🎒 início MedStation
- **🔫 Fuzileiro** — 🧪 **Adrenalina** (pós-abate: −recuo/−recarga/−ADS por `3s + 0.5s/nv`) · 🔧 resist. supressão (aim-punch ×0.5) · 🔧 antitravamento (malfunction ×0.5, fix ×2) · 🎒🏠 Workbench −50%
- **🎯 Caçador** — 🧪 **Fôlego de Aço** (`×(1+0.1·nv) ≤ ×3`, −sway) · 🔧 saque de pistola ×0.5 · 🔧 ADS por arma (sniper/DMR ×0.85, AR ×1.15) · ⚠️ 🔧 resist. de braço em ADS (zona stances, §6) · 🎒🏠 Shooting Range + Intelligence Center −50%
- **👻 Fantasma** — 🔧 **Execução** (melee ×20) · 🔧 Passo Fantasma (ruído de todas as ações `×(1−0.5·nv/max)`, até −50%, **NÃO** silêncio total) · 🔧 MaxSpeed ×1.1 · 🎒🏠 Lavatory −50%
- **🎒 Saqueador** — 🧪 **Mãos Rápidas** (busca/loot mais rápido — 🟡 verificar se loot instantâneo já é vanilla, §7) · 🧪 **Pack Mule** (peso `×(1−[0.10→0.50])`) · 🔧 loot silencioso · 🎒 contêiner seguro 6 slots + Scav Case · 🏠 Scav Case −50% · 🌐 revelar valor ₽ (global, todos veem — não é lever de classe)
- **🛡️ Tanque** — 🔧 **Couraça** (dano recebido `×(1−[0.05→0.25])`) · 🧪 **Pack Mule** (compartilhada c/ Saqueador) · 🔧 GL mastery via patch (o slot `AttachedLauncher` é inerte no globals — §7) · 🔧 GL sem penalidade de ergo · ⚠️ 🔧 stamina segurando arma pesada ×0 (zona stances, §6) · 🔧 velocidade ×0.9 (debuff) · 🔧 −comida/bebida ×0.7 (debuff imediato = patch, não skill) · 🎒🏠 Rest Station + Kitchen + placas laterais · Kitchen −50%

---

## 6. Mecanismos de implementação

### 6.1 Pontos de patch per-player (🔧) confirmados
Per-player, ideais para `×fator` por classe (lendo a classe via `Info.GameVersion`):
- **Velocidade** — postfix em `MovementContext.MaxSpeed`/`SprintSpeed` (multiplicar). **Compõe** com o stances.
- **Inércia** — postfix em `BasePhysicalClass.OnWeightUpdated` → `__instance.Inertia *= fator`. **Compõe** com o stances.
- **Stamina (perna)** — `BasePhysicalClass.Get{Stamina}CapacityFunc`/`*RestorationFunc` + Delta de sprint em `PlayerPhysicalClass`.
- **Respiração** (Fôlego de Aço) — `GetOxygenCapacityFunc` + Delta de hold-breath.
- **ADS por classe de arma** — `_props.Ergonomics` (por arma) × curva `config.Aiming` — patch lendo `weapClass` + classe.
- **Velocidade de loot / lockpick** — `config.t_base_looting` / `t_base_lockpicking`.
- **Confiabilidade / overheat** — `config.Malfunction` / `config.Overheat`.

### 6.2 Coordenação com o stances mod ⚠️
`mods/stancesAndCameraPositionSPT4.0.11` toca velocidade, inércia e stamina de braço **para todos os players**. Como ambos os mods **multiplicam** (não setam), os efeitos **somam**:
- **Velocidade / Inércia / stamina de perna:** multiplicar → compõe sem conflito ✅.
- **Stamina de BRAÇO (mãos):** território do stances (`GetHandsRestorationFunc`→0/5, `Priority.Low`, MainPlayer) → multiplicar lá seria **zerado**. **Não patchar.** Os 2 levers ⚠️ (Caçador resist.-braço-ADS, Tanque stamina-arma-pesada) caem aqui → coordenar via estado compartilhado (mesmo repo) ou trocar o lever (§7).

### 6.3 `globals.json` ⚪
Muda o **baseline** — afeta bots. **Nunca** vira lever por-classe (só via patch per-player ou skill).

---

## 7. Pontas soltas

### ✅ Resolvidas na Fase 4
1. **Custo > 32 (Médico/Caçador)** — aparado: Médico FirstAid Lv6→5 (31.87), Caçador Perception Lv3→2 (31.40). As 6 classes ficam em [28,32].
2. **Pesos das gems** — derivados por categoria no `skill-weights.mjs`: Lockpicking/SilentOps/ProneMovement/ShadowConnections → P (0.94); UsecArsystems/BearAksystems → C (1.50). Matriz recalibrada e re-validada (`class-matrix.mjs` cross-check ✅).
3. **SMG mastery inerte** — confirmado no `globals.json` (`"SMG": []`, idem HMG/LMG). Fantasma trocou SMG ×1.8 por **Pistol ×1.8** (pistola silenciada, mastery funcional); Caçador removeu o debuff SMG (era teatro).
4. **AttachedLauncher inerte** — confirmado (`"AttachedLauncher": []` no globals, ausente do SE). Removido da matriz do Tanque; a identidade de lança-granadas vira **🔧 signature** (patch), não lever 🎯.

### ⏳ Em aberto
5. **Sincronizar `SkillWeights.cs`** — o `.mjs` ganhou 3 categorias de gem (ShadowConnections→P, UsecArsystems→C, BearAksystems→C) que **faltam no `.cs`** (caem em `UnmappedFallback` lá). Replicar numa mudança **coordenada** de `modded/Server/` (sessão paralela do editor).
6. **2 levers ⚠️ na zona stances** (§6.2): Caçador (resist. braço-ADS) e Tanque (stamina arma pesada). Decidir: coordenar (mesmo repo) ou trocar o lever.
7. **Loot instantâneo** — verificar no Assembly se já é vanilla (se for, Mãos Rápidas do Saqueador vira só velocidade de busca). Não bloqueia a matriz.
8. **Bug do Círculo de Cultistas** (ShadowConnections, [class-skill-catalog.md](./class-skill-catalog.md) §5.1) — o servidor não chama `NormalizeToPercentage()` → efeito instantâneo desde o nível 1. Afeta o Saqueador: o cooldown de scav funciona (−50% no L50), mas o círculo está bugado. Contar com isso ou corrigir antes.
9. **Review pendente do `class-levers.md` (9 itens)** — a maioria virou *moot* com "tudo-é-skill-real" (§1); itens vivos absorvidos. Re-revisar via `g-review-content` quando fechar.

---

## 8. Próximas fases

| Fase | O quê | Status |
|---|---|---|
| 0 | Viabilidade de skill custom (pipeline do SE) | ✅ |
| 1 | Travar roster (6 classes) | ✅ |
| 2 | Assinatura por classe + matriz skill calibrada | ✅ aprovado |
| 3 | Consolidar neste doc + `class-matrix.mjs` | ✅ **(este doc)** |
| 4 | Net-check final (pesos das gems por categoria + recalibração dos inertes) | ✅ |
| 4b | Épico no backlog (itens 047+) | ⏳ **PRÓXIMA** |
| 5 | Build das skills custom (padrão SE, sem prepatcher) | ⏳ |
| 6 | Aplicar nos `.jsonc` + validar (editor web + in-game) | ⏳ |

> Os `.jsonc` em `modded/Server/config/classes/` ainda têm o roster **antigo (11 classes)** com a matriz pré-redesign. A Fase 6 substitui (e renomeia para os 6 arquivos de `class-matrix.mjs`).

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-06-16 | mdj | Criação. Catálogo de levers (BuffType + patch per-player + skills/gems SE), régua de impacto, coordenação com o stances mod, rascunho dos 5-6 conjuntos. |
| 2026-06-20 | Guilherme | **Reescrita (Fase 3 do redesign 11→6).** Consolidado: arquitetura "tudo-é-skill-real" (7 decisões), roster 6 classes, tiers de velocidade, **matriz calibrada aprovada** (cards + tabela), camadas 🔧/🧪/🎒/🏠 por classe, mecanismos de patch, pontas soltas. Matriz materializada e validada em `scripts/class-matrix.mjs` (cross-check dos netMult ✅). |
| 2026-06-20 | Guilherme | **Net-check final (Fase 4).** Pesos das gems derivados por categoria (`skill-weights.mjs`); confirmado no `globals.json` que SMG/AttachedLauncher são inertes (`[]`) → removidos da matriz (Fantasma→Pistol ×1.8, Caçador dropa SMG, Tanque GL vira 🔧, Melee ×1.5). Custos aparados p/ [28,32]. Matriz recalibrada: topo +6.12/+6.27/+6.21/+6.12 · base +4.09/+4.28 (cross-check ✅, sem flags). Delta do `SkillWeights.cs` flagado p/ coordenação. |
