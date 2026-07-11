# Handoff — CustomClasses: redesign de 11→6 classes (skills/buffs/debuffs)

> **Data:** 2026-06-20 · **Mod:** `mods/CustomClasses` · **Sessão:** design (sem código novo ainda)
> **Idioma:** comunicação pt-BR · código/commits em inglês · `git push`/remote exigem aprovação

## ⭐ PRÓXIMA AÇÃO (a mais importante)
Iniciar a **Fase 3 — consolidar todo o design no `mods/CustomClasses/docs/class-levers.md`**: reescrever com (a) os 6 cards de classe, (b) a matriz skill×classe calibrada (embutida abaixo), (c) as signatures + camadas 🔧/🧪/🎒/🏠, (d) os fixes da review pendente, (e) a arquitetura "tudo-é-skill-real". **NÃO há código escrito ainda** — tudo é design. O usuário JÁ APROVOU o balance final; falta só persistir no doc e depois Fase 4/5.

⚠️ **Atenção (o usuário avisou que o agente começou a errar/esquecer):** a matriz calibrada e os tiers de velocidade só existem neste handoff e em scripts node **inline** (não foram salvos em arquivo). Use os números DESटE doc como verdade. Re-derivar de memória = erro.

---

## Plano (6 fases) e onde estamos

| Fase | O quê | Status |
|--|--|--|
| 0 | Viabilidade de skill custom (pipeline do SE) | ✅ feito |
| 1 | Travar roster (6 classes) | ✅ feito |
| 2 | Assinatura por classe + matriz skill (buff/debuff calibrada) | ✅ **feito e aprovado** |
| 3 | Consolidar no `class-levers.md` (cards+matriz+signatures+review fixes) | ⏳ **PRÓXIMA** |
| 4 | Materializar como épico no backlog (itens 047+) + net-check final | ⏳ |
| 5 | Build das skills custom (padrão SE, sem prepatcher) | ⏳ |
| 6 | Aplicar nos `.jsonc` + validar (editor web + in-game) | ⏳ |

---

## Decisões de arquitetura travadas (CRÍTICO — não re-litigar)

1. **"Tudo-é-skill-real"** — abandonar spawn-buffs efêmeros. Diferenciação = (a) **skills existentes** via `skills` (nível inicial) + `skillMultipliers` (XP-mult, ×0=desabilita), (b) **skills custom** (padrão SE) só para efeitos novos. Persistem no perfil, aparecem na tela de Skills.
2. **Skill custom SEM prepatcher** — reviver slot `ESkillId` morto + efeito via **patch lendo `mgr.<skill>.Level`** (estilo `UpdateWeaponsPatch` do SE). Evitar `EBuffId` novo (prepatcher Mono.Cecil é frágil/ofuscado). Pipeline completo do SE mapeado pelo agente na Fase 0.
3. **"Desabilitada por classe" = `skillMultipliers[skill]=0` + início 0** → `OnTriggerPatch.cs:33` (`val *= factor`, clamp ≥0) zera o XP → skill congela em 0 → efeito lê 0 → nada. **Sem mecanismo novo.**
4. **Gating por classe em runtime** = ler `player.Profile.Info.GameVersion` (= edition = nome da classe CustomClasses). Funciona p/ qualquer player (`EFT/Profile.cs:239`).
5. **REVIVE morreu** — FIKA 2.2.6 e EFT 0.16.x **não têm sistema de "downed"/revive** (agente confirmou: `FikaPlayer.cs:971` morte é terminal). NÃO construir do zero. Por isso o Médico mudou de "reanimar" para **Médico de Combate**.
6. **Stances mod compõe** (multiplica) em velocidade/inércia/stamina-de-perna; **conflita em stamina de braço** (stances seta `GetHandsRestorationFunc`→0 p/ MainPlayer, Priority.Low). 2 levers nossos caem nessa zona ⚠️ (ver pontas soltas).
7. **netMult ≠ poder real** — peso baixo+impacto alto (Strength 0.47) é subestimado; peso alto (Immunity/Vests/DMR 3.75) domina o número. netMult é guia, cruzar com velocidade+impacto.

---

## Roster (6 classes) — TRAVADO

| pt (in-game) | en | Pilar | Signature |
|--|--|--|--|
| **Médico** | Medic | Suporte | 🔧 Médico de Combate (cura quase instantânea, +50% HP, cura andando/atirando) |
| **Fuzileiro** | Rifleman | Combate | 🧪 Adrenalina (pós-abate: −recuo/−recarga/−ADS por 3s+0.5s/nv) |
| **Caçador** | Hunter | Precisão | 🧪 Fôlego de Aço (prende respiração ×(1+0.1·nv)≤×3, −sway) |
| **Fantasma** | Ghost | Furtividade | 🔧 Execução (dano melee ×20) |
| **Saqueador** | Looter | Pilhagem | 🧪 Mãos Rápidas (busca/loot +rápido) + 🧪 Pack Mule |
| **Tanque** | Tank | Resistência | 🔧 Couraça (dano recebido ×(1−[0.05→0.25])) + 🧪 Pack Mule |

`displayName` guarda os dois; launcher mostra conforme `config/settings.jsonc` (hoje `"pt"`).

---

## Tiers de velocidade de skill (validado no globals `SkillsSettings`)
> Calibração: **buff forte vale em 🐌** (mult importa onde o grind é real); **debuff só morde em 🐇/🚶** (em 🐌 é teatro). Usado em todo o balance.

- 🐇 **Sobe-fácil:** Perception, Metabolism, Attention, Search, Charisma, Intellect
- 🚶 **Média:** Assault, Pistol, SMG, Shotgun, DMR, AimDrills, MagDrills, StressResistance, **Endurance**, **CovertMovement** *(Endurance/CovertMovement foram CORRIGIDos de 🐇→🚶 — não são fast: Endurance 0.04/ação, CovertMovement 0.025/passo)*
- 🐌 **Grind:** Strength, Sniper, Vitality, Health, Immunity, Melee, Throwing, Surgery, FirstAid, FieldMedicine, Light/HeavyVests, TroubleShooting, HideoutManagement, Crafting + gems (Lockpicking, SilentOps, ProneMovement, ShadowConnections, UsecArsystems, BearAksystems, AttachedLauncher)

---

## MATRIZ FINAL CALIBRADA (camada 🎯 skill) — APROVADA
> `×` = multiplicador de XP · `Lv` = nível inicial · 🟢 buff 🔴 debuff. **Balance: topo (Méd/Fuz/Caç/Fan) ~+6 · base (Saq/Tan) ~+4** (Saq/Tan compensados pelas signatures 🔧🧪 fora do netMult).

### 🩺 Médico — netMult 6.12 · custo 32.8
- 🟢 FirstAid 🐌×2.5 Lv6 · FieldMedicine 🐌×2 Lv5 · Surgery 🐌×2 Lv4 · Vitality 🐌×2 Lv4 · HideoutManagement 🐌×1.5 Lv6 · Crafting 🐌×1.5 · Immunity 🐌×1.2 Lv1
- 🔴 Assault 🚶×0.6 · AimDrills 🚶×0.7 · CovertMovement 🚶×0.7 · Perception 🐇×0.8

### 🔫 Fuzileiro — netMult 6.27 · custo 29.5
- 🟢 Assault 🚶×2.5 Lv7 · UsecArsystems 🐌×2.5 Lv4 · BearAksystems 🐌×2.5 Lv4 · AimDrills 🚶×1.5 Lv5 · MagDrills 🚶×1.5 Lv4 · Endurance 🚶×1.5 Lv5 · StressResistance 🚶×1.3 · Pistol 🚶×1.2
- 🔴 CovertMovement 🚶×0.6 · Attention 🐇×0.7 · Search 🐇×0.8

### 🎯 Caçador — netMult 5.84 · custo 32.5
- 🟢 Sniper 🐌×2.5 Lv7 · DMR 🚶×1.5 Lv2 · AimDrills 🚶×1.5 · ProneMovement 🐌×1.5 Lv3 · Pistol 🚶×1.3 Lv2 · Perception 🐇×1.3 Lv3 · Metabolism 🐇×1.3 · CovertMovement 🚶×1.2 Lv3
- 🔴 Assault 🚶×0.6 · SMG 🚶×0.6
- *(cortados na limpeza: SilentOps/LightVests/Endurance ×1.2 flavor)*

### 👻 Fantasma — netMult 6.16 · custo 28.7
- 🟢 SilentOps 🐌×2.5 Lv6 · SMG 🚶×1.8 Lv4 · CovertMovement 🚶×1.5 Lv6 · Perception 🐇×1.5 Lv5 *(exceção aceita: sentido-assinatura)* · Pistol 🚶×1.5 · Melee 🐌×1.5 Lv3 · LightVests 🐌×1.3 · ProneMovement 🐌×1.5 · Lockpicking 🐌×1.3 Lv3
- 🔴 Assault 🚶×0.6 · StressResistance 🚶×0.7 · Shotgun 🚶×0.7

### 🎒 Saqueador — netMult 4.06 · custo 28.6
- 🟢 Lockpicking 🐌×3 Lv8 · ShadowConnections 🐌×2.5 Lv6 · Strength 🐌×2.5 Lv6 · Attention 🐇×1.3 Lv8 · Perception 🐇×1.3 Lv5 · Search 🐇×1.3 Lv6 · HideoutManagement 🐌×1.2 · Intellect 🐇×1.2 · Charisma 🐇×1.2
- 🔴 Assault 🚶×0.6 · AimDrills 🚶×0.7 · StressResistance 🚶×0.7

### 🛡️ Tanque — netMult 4.22 · custo 30.3
- 🟢 StressResistance 🚶×2 · HeavyVests 🐌×1.5 Lv3 · Health 🐌×1.5 Lv4 · Vitality 🐌×1.5 Lv4 · Strength 🐌×1.5 Lv5 · Shotgun 🚶×1.5 Lv1 · Throwing 🐌×1.5 Lv1 · AttachedLauncher 🐌×1.5 · Melee 🐌×1.2
- 🔴 Metabolism 🐇×0.5 · CovertMovement 🚶×0.5 · AimDrills 🚶×0.7 · Pistol 🚶×0.7 · DMR 🚶×0.7

---

## Camadas além do 🎯 skill (signatures 🔧/🧪 + 🎒 loadout + 🏠 hideout)

- **Médico** 🔧 cura tempo ×0.3, +50% HP, sem lock de movimento/arma · 🔧 membro quebrado cura ×0.5 tempo · 🏠 Medstation −50% tempo · 🎒 início Medstation
- **Fuzileiro** 🧪 Adrenalina · 🔧 resist. supressão (aim-punch ×0.5) · 🔧 antitravamento (malfunction ×0.5, fix ×2) · 🎒🏠 Workbench −50%
- **Caçador** 🧪 Fôlego de Aço · 🔧 saque pistola ×0.5 · 🔧 ADS por arma (sniper/DMR ×0.85, AR ×1.15) · ⚠️ 🔧 resist. braço em ADS (zona stances) · 🎒🏠 Shooting Range + Intelligence Center −50%
- **Fantasma** 🔧 Execução (melee ×20) · 🔧 Passo Fantasma (ruído todas ações ×(1−0.5·nv/max), até −50%, NÃO silêncio total) · 🔧 MaxSpeed ×1.1 · 🎒🏠 Lavatory −50%
- **Saqueador** 🧪 Mãos Rápidas (🟡 verificar se loot instantâneo já é vanilla) · 🧪 Pack Mule (peso ×(1−[0.10→0.50])) · 🔧 loot silencioso · 🎒 contêiner seguro 6 slots + Scav Case · 🏠 Scav Case −50% · 🌐 revelar valor ₽ (GLOBAL, todos veem — não é lever de classe)
- **Tanque** 🔧 Couraça · 🧪 Pack Mule (compartilhada c/ Saqueador) · 🧪/🔧 GL mastery (slot `AttachedLauncher`) · 🔧 GL sem penalidade de ergo · ⚠️ 🔧 stamina segurando arma pesada ×0 (zona stances) · 🔧 velocidade ×0.9 (debuff) · 🔧 −comida/bebida ×0.7 (debuff imediato = patch, não skill) · 🎒🏠 Rest Station + Kitchen + placas laterais · Kitchen −50%

**Padrão hideout:** cada classe = 1 estação inicial 🎒 + 1 estação −50% tempo 🏠.

---

## Pontas soltas (resolver na Fase 3/4)
1. **custo Médico (32.8) e Caçador (32.5) ~0.5 acima do teto 32** → aparar 1 nível inicial (trivial).
2. **Pesos reais das gems** — UsecAr/BearAk/Lockpicking/SilentOps/ProneMovement/ShadowConnections/AttachedLauncher caem em peso 1.0 (unmapped) no `SkillWeights.cs`/`skill-weights.mjs`. Definir pesos reais → re-rodar net.
3. **2 levers ⚠️ zona stances:** Caçador (resist. braço-ADS) e Tanque (stamina arma pesada) tocam stamina de braço que o stances sobrescreve. Decidir: coordenar (mesmo repo) ou trocar o lever.
4. **`AttachedLauncher` e loot-instantâneo** — verificar no Assembly (o usuário acha que loot instantâneo já é vanilla; se for, Mãos Rápidas vira só velocidade de busca).
5. **Review pendente do `class-levers.md`** (9 itens) — aplicar na reescrita da Fase 3 (a maioria virou moot com "tudo-é-skill-real"; ver doc).

---

## Artefatos (ler antes de continuar)
- `mods/CustomClasses/docs/class-levers.md` — doc a REESCREVER na Fase 3 (versão atual está desatualizada — pré-redesign).
- `mods/CustomClasses/docs/balance-model.md` e `class-archetypes.md` — modelo de custo/netMult.
- `mods/CustomClasses/scripts/skill-weights.mjs` — pesos (fonte única JS) + `BUDGET 28-32`, `MAX_SKILLS_WITH_POINTS=6`. Espelha `modded/Server/SkillWeights.cs`.
- `mods/CustomClasses/modded/Client/Patches/OnTriggerPatch.cs:33` — onde o XP-mult é aplicado (`val *= factor`, ×0=congela).
- `mods/CustomClasses/modded/Server/config/classes/*.jsonc` — onde a matriz será aplicada (Fase 6). Ex. atual `cacador.jsonc`: `skills{Sniper:8,...}` + `skillMultipliers{Sniper:2,...}`.
- Skills-Extended em `mods/Skills-Extended/modded` — referência do pipeline de skill custom (Fase 5).
- **A matriz calibrada NÃO está em nenhum arquivo** — só neste handoff. Materializar na Fase 3/4 (sugiro um `class-matrix.mjs` reproduzível).

## Skills sugeridas para a próxima sessão
- Nenhuma skill de fluxo ainda (estamos em design). Para Fase 3: edição direta do doc.
- Fase 4: `/add-backlog-item CustomClasses "..."` → `/create-spec` → `/create-technical-spec` → `/review-technical-spec` para cada skill custom / patch.
- Fase 5: `/code-mod` + `/compile-mod CustomClasses` + `/code-review`.
- `g-review-content` para revisar o `class-levers.md` reescrito.

## Restrições de sessão (preservar)
- Editor web do CustomClasses é sessão paralela — coordenar antes de mexer em `modded/Server/`.
- Escritas em arquivos SPT precisam de validação **in-game**, não só write+hash.
- `serve-inventory` pode clobberar edições manuais — commitar logo após sync.
- Builds locais de mod client são revertidas pelo sync do launcher (Dev Mod off) — subir build ao servidor.
