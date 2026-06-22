# Catálogo quantitativo de skills (fórmula real por skill)

> **Data:** 2026-06-20<br>
> **Status:** 🟢 Vivo<br>
> **Responsáveis:** Guilherme<br>
> **Referências:** [class-levers.md](./class-levers.md)<br>

---

Detalhamento **quantitativo** das skills que o [class-levers.md](./class-levers.md) lista qualitativamente. Para cada skill: o que faz (qualitativo) e a **fórmula real** (efeito no nível 50 e no elite). Serve de base para escolher fatores por classe.

**Fontes de verdade** (cruzadas para montar este catálogo):
- `mods/Skills-Extended/modded/Server/Resources/Configs/SkillsConfig.json` — magnitudes das físicas, médicas e das 6 gems (o **SE manda** nessas no setup do CustomClasses).
- `mods/Skills-Extended/modded/Plugin/Skills/**` — semântica de como cada valor vira efeito.
- `references/eft-decompiled/Assembly-CSharp/**` — efeito das skills **nativas** não reimplementadas pelo SE.
- `D:\SPT\SPT\SPT_Data\database\globals.json` → bloco `SkillsSettings` (linha ~35250) — taxas de XP e constantes nativas (autoritativo sobre os defaults hardcoded do `BackendConfigSettingsClass.cs`).

## 1. Regra-mestra (como ler todo número abaixo)

Confirmada no código (os patches de servidor reescrevem a curva à mão e batem com os builders nativos):

- Valor do `SkillsConfig.json` = **pontos percentuais** (`0.6` → 0,6%). Normalizado por `MathExtensions.NormalizeToPercentage()` (`/100`).
- `PerLevel(p)` → efeito **linear** `p% × nível` → no **nível 50** vale `p×50 %`.
- `Max(m)` → `m` **é** o valor no nível 50 (curva `m × nível/50`).
- `Elite(e)` → bônus **plano**, somado **só no nível 51**.
- Consumo: buff positivo `×(1+valor)`; redução `×(1−valor)`.
- Nível máx = 50; **elite = 51** (`AbstractSkillClass.cs`: `Level = Current/100`).

**Marcação de confiança:** `[cód]` lido do decompilado/plugin · `[globals]` lido do `globals.json` · `[mec]` mecânica EFT estabelecida cujo ponto de consumo está fora do dump `Assembly-CSharp` (outra assembly), mas é consistente com os parâmetros do item · `[inf]` inferido (getter compilado de `SkillManager` **não** extraível do dump — padrão EFT).

> **Atenção às magnitudes nativas.** A regra-mestra do `/100` (`NormalizeToPercentage`) vale para o `SkillsConfig.json` (SE). Os buffs **nativos** montados no `SkillManager` (ex.: Surgery) já recebem **frações cruas** no builder (`Max(0.2f)`, `PerLevel(0.01f)`) — **não** divida por 100. Ver §4.1.

## 2. Quem manda em cada skill

| Grupo | Fonte de verdade do efeito |
|---|---|
| Físicas, médicas (FirstAid/FieldMedicine), as 6 gems | **Skills-Extended** (`SkillsConfig.json` + plugin) |
| Mastering de arma, AimDrills, MagDrills, RecoilControl, CovertMovement, Attention, Perception, Search, Throwing, Vests, Surgery | **EFT nativo** (decompilado + `globals.json`) |

## 3. Físicas — reimplementadas pelo SE

Substituem as nativas via `Core/Patches/CreateSkillPatches.cs`; cada buff liga a um BuffType nativo do EFT consumido em `PlayerPhysicalClass.cs`.

| Skill | O que faz | Efeito no L50 / elite |
|---|---|---|
| **Endurance** | Stamina, custo de pulo, hold-breath, recuperação de stamina, mãos | Capacidade de stamina **+50%** (elite +70%); restauração **+50%** (elite +75%); custo de pulo **−30%**; hold-breath **+100%**; mãos elite +50%; elite: não treme sem stamina |
| **Strength** | Pulo, carga, melee, sprint, arremesso, fadiga de mira, mato/água | JumpHeight **+20%**, LiftWeight **+30%**, MeleePower **+30%**, SprintSpeed **+20%**, ThrowDist **+20%**, AimFatigue **+20%**; penalidade de mato/água **−75%**; elite: crit melee +50%, atravessa mato |
| **Health** | Menos fratura/quebra; +energia/hidratação máx | BreakChance **−60%**; Energy máx **+30%**; Hydration máx **+30%**; elite: absorve parte do dano |
| **Vitality** | Menos sangramento; mais "sobrevivência" | BleedChance **−60%**; Survivability **+20%**; elite: regen + para sangramento |
| **Metabolism** | Recuperação de energia/sede; debuffs metabólicos | Ratio de recuperação **+50%**; duração de debuff **−50%**; elite: não desidrata |
| **StressResistance** | Dor e tremor sob HP baixo/estresse | Pain **−50%**; Tremor **−60%**; elite: berserk |
| **Immunity** | Veneno/intoxicação, debuff de stim, painkiller | MiscEffects **−50%**; Poison **−50%**; PainKiller **+30%**; elite: **90%** de evitar veneno e efeitos diversos |

`[cód: SkillClasses/Physical/*.cs + PlayerPhysicalClass.cs]` — magnitudes confirmadas cruzando `SkillsConfig.json` (SE, pontos percentuais) com o builder nativo `method_3` `[cód: SkillManager.cs:2400-2486]`: ambos coincidem (ex.: `BuffEnduranceIncMax 50` = `EnduranceBuffEnduranceInc.Max(0.5f)`; `BleedChanceRedPerLevel 1.2` = `VitalityBuffBleedChanceRed.PerLevel(0.012f)`).

## 4. Médicas

| Skill | Fonte | O que faz | Efeito no L50 / elite |
|---|---|---|---|
| **FirstAid** | SE | Med-kits mais rápidos e baratos | Velocidade de uso **+30% no L50** (`tempo × (1 − 0,006·nível)`, config `ItemSpeedBonus 0.6`) `[cód: HealthEffectUseTimePatch.cs:36]`; custo de recurso **−25% no L50** (`× (1 − 0,005·nível)`, config `MedkitUsageReduction 0.5`) `[cód: HealthEffectComponentPatch.cs:105-166]`; **elite:** anda com perna quebrada `[cód: CanWalkPatch.cs:34]` |
| **FieldMedicine** | SE | Injetores mais potentes | Cap de efeito do stim sobe `60·(1 + 0,005·nível)` → **75 no L50** `[cód: StimulatorApplyBuffPatch.cs:29]`; duração do buff `× (1 + 0,01·nível)` → **+50% no L50**; chance de efeito positivo `× (1 + 0,005·nível)` → **+25% no L50** `[cód: SkillManagerExt.cs:277,280]` |
| **Surgery** | EFT nativo | Cirurgia (CMS/Surv12) mais rápida + mais HP restaurado na parte destruída | **Velocidade:** `tempo = base ÷ (1 + 0,2·nível/50)` → L50 **−16,7%**, elite **−28,6%** `[cód: SkillManager.cs:2537 + Player.cs:19907]`. **Penalidade de HP:** `×(1 − 0,01·nível)` → L50 **−50% da penalidade**, **elite anula** (restaura 100%) `[cód: SkillManager.cs:2538]`. Fórmula + exemplos → **§4.1** |

### 4.1 Surgery em detalhe (fórmula real + exemplos)

A cirurgia só age em **parte destruída** (blacked-out, HP atual = 0): braços (60), pernas (65), estômago (70). Cabeça (35) e tórax (85) destruídos = morte, sem cirurgia. Os dois kits do jogo (`[banco: templates/items.json]`):

| Kit | ID | `medUseTime` base | Penalidade HP (min–max) | `MaxHpResource` | Extra |
|---|---|---|---|---|---|
| **CMS** (`core_medical_surgical_kit`) | `5d02778e86f774203e7dedbe` | **16 s** | **25–45%** | 3 | — |
| **Surv12** (`survival_first_aid_rollup_kit`) | `5d02797c86f774203f38e30a` | **20 s** | **60–72%** | 9 | também cura **Fratura** |

A skill tem **dois buffs nativos** (`[cód: SkillManager.cs:2531-2539]`):

- **`SurgerySpeed.Max(0.2f).Elite(0.4f)`** → `Value = 0,2 · nível/50` (L50 = `0,2`; elite = `0,4`). Aplicado como **multiplicador de velocidade do animator**: `SetUseTimeMultiplier(1 + Value)` `[cód: Player.cs:19907]`. Logo o tempo efetivo é `base ÷ (1 + Value)` (multiplicador >1 = animação mais rápida = menos tempo).
- **`SurgeryReducePenalty.PerLevel(0.01f).Elite(1f)`** → `Value = 0,01 · nível` (L50 = `0,5`; elite = `1,0`). Reduz a penalidade de HP da parte destruída de forma **multiplicativa**: `penalidade_efetiva = penalidade_item × (1 − Value)`. Elite (`1,0`) zera a penalidade → restaura **100%** do HP máximo.

> A **forma de consumo** da penalidade (`maxHP × (1 − penalidade_efetiva)`) é a mecânica EFT estabelecida `[mec]` — o `DoMedEffect` que a aplica está em outra assembly, fora do dump `Assembly-CSharp`. As **magnitudes dos buffs** (`0,2` e `0,01/nível`), essas, são `[cód]`. Há ainda um caminho secundário de fila multi-parte (`Player.cs:19562`) que usa `SurgerySpeed.Value / 100f` (≈ irrelevante) + `0,2` se a parte está quase cheia — não dispara em cirurgia (parte destruída tem HP=0). ⚠️

#### Velocidade — tempo de cirurgia por nível

`tempo = medUseTime ÷ (1 + 0,2·nível/50)`

| Nível | Multiplicador | CMS (base 16 s) | Surv12 (base 20 s) |
|---|---|---|---|
| 0 | ×1,00 | 16,0 s | 20,0 s |
| 25 | ×1,10 | 14,5 s | 18,2 s |
| **50** | **×1,20** | **13,3 s** (−16,7%) | **16,7 s** (−16,7%) |
| **Elite (51)** | **×1,40** | **11,4 s** (−28,6%) | **14,3 s** (−28,6%) |

#### HP recuperado — restauração na parte destruída

`HP_restaurado = maxHP × (1 − penalidade_item × (1 − 0,01·nível))`

A penalidade do item é uma faixa (min–max), então o HP restaurado também é faixa. Exemplo numérico numa **perna (maxHP = 65)** e num **braço (maxHP = 60)**:

| Nível | Penalidade efetiva | % restaurado | **CMS** perna 65 / braço 60 | **Surv12** perna 65 / braço 60 |
|---|---|---|---|---|
| 0 | CMS 25–45% · Surv12 60–72% | CMS 55–75% · Surv12 28–40% | **36–49 / 33–45 HP** | **18–26 / 17–24 HP** |
| **50** (−50%) | CMS 12,5–22,5% · Surv12 30–36% | CMS 77,5–87,5% · Surv12 64–70% | **50–57 / 47–53 HP** | **42–46 / 38–42 HP** |
| **Elite** (−100%) | 0% (ambos) | 100% | **65 / 60 HP** (cheio) | **65 / 60 HP** (cheio) |

Escala linear por `maxHP` (estômago 70 etc.). **Leitura prática:** o CMS restaura **muito mais HP por cirurgia** (penalidade 25–45% vs 60–72%); o Surv12 compensa com **mais usos (9 vs 3)** e cura fratura. Com Surgery no elite, qualquer kit devolve o membro **cheio** — a vantagem do CMS por uso desaparece e o Surv12 vira o melhor custo-benefício.

## 5. As 6 "gems" — reativadas pelo SE

| Gem | O que faz | Efeito no L50 / elite |
|---|---|---|
| **UsecArsystems** (=NATO) | Em armas NATO: +ergo, −recuo | Ergo **+30%**; recuo **−20%**; +10% do XP cruza p/ Eastern `[cód: SkillManagerExt.cs:201-202]` |
| **BearAksystems** (=Eastern) | Idem armas do Leste (menor — batem mais forte) | Ergo **+20%**; recuo **−15%** `[cód: SkillManagerExt.cs:210-211]` |
| **ProneMovement** | Rastejar mais rápido e silencioso | Velocidade prone **+30%**; volume **−40%** `[cód: ProneMoveStatePatch.cs:33,72]` |
| **SilentOps** | Melee rápido, portas/loot silenciosos, supressor barato | Melee speed **+25%**; volume de porta/loot **−50%**; preço de supressor **−25%** `[cód: SkillManagerExt.cs:238-240]` |
| **LockPicking** | Minigame de arrombar fechadura | "Força do pick" **+125%**; tolerância do sweet-spot **+75%**; quebra na 3ª falha; **elite: não consome lockpick** `[cód: LockPickingGame.cs:405-430]` |
| **ShadowConnections** | Cooldown de scav, Círculo de Cultistas, scav cultista | ver §5.1 ⚠️ |

### 5.1 ShadowConnections em detalhe

XP matando cultistas (`sectantWarrior`/Priest). Três efeitos — **cliente e servidor divergem**:

- **Cooldown do scav:** `buff = clamp(1 − 0,01×nível, 0.05, 1)` → **−50% no L50** (teto −95%); **elite: cooldown zerado** (5s). `[cód: Server/Patches/ScavCooldownTimerPatch.cs:52-55]`
- **Tempo de retorno do Círculo de Cultistas:** config `1`. **Pretendido** −50% no L50, **mas há bug** — o servidor não chama `NormalizeToPercentage()` (`CultistProductionPatch.cs:69`), usa o `1` cru → tempo **zerado já no nível 1**. ⚠️
- **Scav nascer cultista:** **0,5 ponto%/nível → 25% no L50** (`GeneratePlayerScavPatch.cs:48`) — troca o role para `sectantWarrior` e copia vida/aparência.

## 6. Combate nativas EFT

| Skill | O que faz | Efeito |
|---|---|---|
| **Weapon mastering** (Assault, Shotgun, Sniper, DMR, Pistol, Revolver) | +ergo e −recuo da *classe* de arma | Recuo **−0,3%/nível → −15% no L50** `[globals:35640]`; ergo +1/nível **[inf]**. **SMG/LMG/HMG = `[]`** (sem XP/efeito nesta build) |
| **AimDrills** | Velocidade de saque/mira (ADS) e silêncio do saque | Velocidade de saque/ADS `DrawSpeed.Max(0.5f)` → **+50% no L50** `[cód: SkillManager.cs:2051]` (consumo de tempo na animação fora do dump); som do saque **−50%** `× (1 − DrawSound)`, `DrawSound.Max(0.5f)` `[cód: Player.cs:14329]`; **elite:** saque sem tremor (`DrawElite`/`DrawTremor`) |
| **MagDrills** | Load/unload/check de magazine | Fórmula `tempo × (100 − Skills.MagDrills*)/100`. Load `PerLevel(0.6)` → **−30% no L50** `[cód: Player.cs:1257]`; unload `PerLevel(0.6)` → **−30%** `[cód: Player.cs:1279]`; check `PerLevel(0.8)` → **−40%** `[cód: Player.cs:1146]`; precisão do check `clamp(nível/10, 0, 2)`; **elite:** check de munição instantâneo `[cód: SkillManager.cs:2588-2592]` |
| **RecoilControl** | Recuo global de toda arma | `RecoilControlImprove.PerLevel(0.003)` → **−0,3%/nível → −15% no L50** `[cód: SkillManager.cs:2048 + globals:35551]`. **Continua viva** — hipótese de "removida em 0.14.5" **não se confirma** no dump |
| **Throwing** | Custo de stamina e distância de granada | Custo de stamina do arremesso `× (1 − ThrowingEnergyExpenses)`, `PerLevel(0.01)` → **−50% no L50** `[cód: SkillManager.cs:2014,2045]`. A **distância** usa o buff de **Strength** (`StrengthBuffThrowDistanceInc`), não o de Throwing `[cód: Player.cs:15638]`; **elite:** arremesso sem tremor de stamina (`ThrowingEliteBuff`) |

## 7. Movimento / percepção nativas EFT

| Skill | O que faz | Efeito |
|---|---|---|
| **CovertMovement** | Reduz volume de passo/equipamento ao mover devagar + velocidade encoberta | Fórmula `1 − Skills.CovertMovement*`. Volume de passo `SoundVolume.PerLevel(0.012)` → **−60% no L50** `[cód: MovementContext.cs:969]`; ruído de equipamento `Equipment.PerLevel(0.012)` → **−60%** `[cód: MovementContext.cs:1009]`; velocidade de movimento encoberto `Custom(1 + 0,01·nível)` → **×1,5 no L50** `[cód: SkillManager.cs:2527]`; **elite:** quase silêncio agachado |
| **Perception** | Raio de destaque de loot + som | raio `0,1 × (1 + PerceptionLootDot)`, `PerceptionLootDot.PerLevel(0.02)` → **1,0 no L50** → raio **dobra (0,1→0,2)** `[cód: Player.cs:29750 + SkillManager.cs:2344]`; **elite:** distância de destaque **1,5→2,35** |
| **Attention** | Exame de itens / identificação de loot | `AttentionExamine.PerLevel(0.02)` e `AttentionLootSpeed.PerLevel(0.02)` → ambos **1,0 no L50** `[cód: SkillManager.cs:2378-2379]` (forma de consumo do tempo fora do dump → efeito % final **[inf]**); **elite:** lucky-search `0,5` + examinar instantâneo |
| **Search** | Velocidade de busca em container | `SearchBuffSpeed.PerLevel(0.01)` → **+50% no L50** `[cód: SkillManager.cs:2547]` (consumo fora do dump); **elite:** busca 2 containers ao mesmo tempo (`SearchDouble`) `[cód: GClass2235.cs:84]` |

## 8. Armadura nativas EFT

| Skill | Efeito (lido do globals) |
|---|---|
| **LightVests** | Penalidade de movimento do colete **−0,6%/nível → −30% no L50** `[globals:35501]`; dano melee atravessado **−0,6%/nível → −30%** `[globals:35500]`; desgaste no reparo **−0,8%/nível → −40%** `[globals:35502]`; proteção a sangramento (flag); **elite: −50% chance** de deterioração no reparo `[globals:35503]` |
| **HeavyVests** | Penalidade de movimento do colete **−0,5%/nível → −25% no L50** `[globals:35397]`; blunt atravessado **−0,4%/nível → −20%** `[globals:35381]`; desgaste no reparo **−1%/nível → −50%** `[globals:35401]`; **elite: +5% ricochete** + **−50% chance** de deterioração `[globals:35402]` |

## 9. Mortas / meta — **não viram lever de classe**

Efeito é fora de raid (hideout/loja/bancada).

| Skill | Por que não vale | O que faz (meta) — valores reais |
|---|---|---|
| **Intellect** | bancada/exame | Custo de pontos de reparo **−0,4%/nível → −20% L50** `[globals:35477]`; desgaste ao reparar −0,4%/nível; velocidade de aprendizado e manutenção de arma +2%/nível `[cód: SkillManager.cs:2365-2367]`; **elite:** contador de munição, scope em container, aprendiz natural |
| **Charisma** | loja | Descontos de cura/seguro/saída-paga/reroll de daily **−0,1%/nível → −5% L50** cada `[globals:35291-35296]`; **elite:** scav case **−10%**, perda de Fence **−50%**, **+1** daily quest `[globals:35286-35288]` |
| **Crafting** | hideout | Tempo de craft único **−0,75%/nível → −37,5% L50** `[globals:35330]`; tempo de produção contínua **−0,75%/nível** `[globals:35342]`; **elite: +1** produção |
| **HideoutManagement** | hideout | Consumo de recurso **−0,5%/nível → −25% L50** `[globals:35406]`; boost de bônus de zona **+1%/nível** `[globals:35425]`; **elite:** slots extras |
| **WeaponTreatment** | bancada | Perda de durabilidade ao atirar **−0,5%/nível → −25% L50** `[globals:35657]`; desgaste ao reparar arma **−1%/nível → −50% L50** `[globals:35660]`; **elite: −50% chance** de deterioração |
| **TroubleShooting** | passivo/raro | Tempo de reparo de malfunction **−0,5%/nível → −25% L50** `[globals:35626]`; **elite: −30% chance** de falha de munição/durabilidade/magazine `[globals:35623-35625]` + examinar malfunction |
| **Memory** | meta | segura skills contra decay; **elite:** não decaem |

**FactionLocked (levers fracos):**
- **BearRawPower** (só BEAR): desconto Prapor **−0,5%/nível**, +25% XP de quest no L50, **elite −7%** em todos os traders. `[cód: Server/Patches/GetTraderAssortPatch.cs:81-99]`
- **UsecNegotiations** (só USEC): análogo com Peacekeeper e dinheiro de quest.

## 10. Achados que afetam "tratar como verdade"

1. **Bug do Círculo de Cultistas** (ShadowConnections §5.1): falta `NormalizeToPercentage()` no servidor → efeito real é instantâneo desde o nível 1, não −1%/nível. Contar com isso ou corrigir antes de usar como lever.
2. **Endurance** tem aparente copy/paste em `EnduranceSkill.cs:26` usando `BuffBreathTimeIncMax`(100) onde devia ser `BuffEnduranceIncMax`(50). Validar in-game.
3. Os números "X%/nível" do `modpage.md` do SE estão **desatualizados** (config antiga). Os valores deste catálogo seguem o `SkillsConfig.json` vigente.
4. **Magnitudes nativas agora extraídas do builder** (`SkillManager.cs:2042-2601`): AimDrills (`DrawSpeed.Max(0.5f)`), MagDrills (`PerLevel 0.6/0.6/0.8`), Search (`SearchBuffSpeed.PerLevel(0.01)`), CovertMovement, Perception, Attention, Throwing — todos passam a **[cód]**. O que **ainda** fica **[inf]** é só a *forma de consumo* de alguns valores (tempo de saque do AimDrills, tempo de exame do Attention, velocidade de busca do Search), consumida em assembly **fora** do dump `Assembly-CSharp` — e o **ergo por nível do mastering**. Para fechar: tooltip em jogo ou dnSpy nos getters `SkillManager.get_*`.
5. **Duas convenções de escala convivem:** buffs **nativos** no builder usam **frações cruas** (`Max(0.2f)`, `PerLevel(0.012f)`) aplicadas direto; as skills do **SE** (físicas, médicas, gems) guardam **pontos percentuais** no `SkillsConfig.json` e o plugin **normaliza `/100`** antes de escalar por nível (ex.: FirstAid `ItemSpeedBonus 0.6` → `0,006·nível` → 0,30 no L50). Não confundir os dois ao ler um número cru.

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-06-20 | Guilherme | Criação. Catálogo quantitativo cruzando SkillsConfig.json (SE), EFT decompilado e globals.json: regra-mestra de leitura, físicas/médicas/gems (SE) + nativas (mastering, AimDrills, vests, etc.), mortas/meta e achados (bug do Círculo de Cultistas, copy/paste em Endurance). |
| 2026-06-21 | Guilherme | Surgery deixa de ser `[inf]`: fórmula real extraída do `SkillManager.cs:2531-2539` (`SurgerySpeed.Max(0.2f)` + `SurgeryReducePenalty.PerLevel(0.01f).Elite(1f)`). Nova §4.1 com mecânica de buff (frações cruas, sem `/100`), tabela de tempo e HP recuperado por nível, e exemplos para CMS e Surv12 (params reais de `items.json`). Adicionado marcador de confiança `[mec]` e nota sobre o caminho `/100f` em `Player.cs:19562`. |
| 2026-06-21 | Guilherme | Passagem por **todas** as skills cruzando builder nativo (`SkillManager.cs:2042-2601`), `SkillsConfig.json` (SE), plugins SE e `globals.json`. Resolvidos vários `[inf]` → `[cód]`/`[globals]`: AimDrills **+50%** (DrawSpeed.Max 0.5), MagDrills **−30/−30/−40%**, Search **+50%**, CovertMovement **−60%** volume, Perception raio dobra, Throwing energia **−50%**. §4 (FirstAid/FieldMedicine) com fórmula por nível e citações corretas (CanWalkPatch, SkillManagerExt). §8 vests e §9 mortas/meta (Intellect, Charisma, Crafting, Hideout, WeaponTreatment, TroubleShooting) com valores/nível + L50 do globals. §3 confirmada (SE = builder). §10: item 4 atualizado + item 5 (duas convenções de escala). |
