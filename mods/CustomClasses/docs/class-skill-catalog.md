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

**Marcação de confiança:** `[cód]` lido do decompilado/plugin · `[globals]` lido do `globals.json` · `[inf]` inferido (getter compilado de `SkillManager` **não** extraível do dump — padrão EFT).

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

`[cód: SkillClasses/Physical/*.cs + PlayerPhysicalClass.cs]`

## 4. Médicas

| Skill | Fonte | O que faz | Efeito no L50 / elite |
|---|---|---|---|
| **FirstAid** | SE | Med-kits mais rápidos e baratos | Velocidade de uso **+30%**; custo de recurso **−25%**; elite: anda com perna quebrada `[cód: SkillManagerExt.cs:181-182]` |
| **FieldMedicine** | SE | Injetores mais potentes | Eleva cap de skill p/ **75** (+25%); duração de stim **+50%**; chance de efeito positivo **+25%** `[cód: StimulatorApplyBuffPatch.cs:29]` |
| **Surgery** | EFT nativo | Cirurgia (CMS/Surv12) mais rápida + mais HP | Multiplicador `1+SurgerySpeed` `[cód: Player.cs:19907]`; valor/nível **[inf]**; elite reduz tempo forte |

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
| **AimDrills** | Velocidade de ADS | ~+0,2%/nível → **~+10% no L50** **[inf]** |
| **MagDrills** | Load/unload/check de magazine | `100 − Skills.MagDrills*` `[cód: Player.cs:1146+]`; valores/nível **[inf]**; elite: check instantâneo |
| **RecoilControl** | Recuo global de toda arma | **−0,3%/nível → −15% no L50** `[globals:35549]`. **Continua viva** — hipótese de "removida em 0.14.5" **não se confirma** no dump |
| **Throwing** | Distância de granada | distância usa o buff de **Strength** `[cód: Player.cs:15638]`; elite: arremesso sem tremor de stamina |

## 7. Movimento / percepção nativas EFT

| Skill | O que faz | Efeito |
|---|---|---|
| **CovertMovement** | Reduz volume de passo/equipamento ao mover devagar | `1 − Skills.CovertMovement*` `[cód: MovementContext.cs:965]`; ~−0,3%/nível **[inf]**; elite: quase silêncio agachado |
| **Perception** | Raio de destaque de loot + som | raio `0,1×(1+PerceptionLootDot)` `[cód: Player.cs:29750]`; **elite: distância 1,5→2,35** |
| **Attention** | Exame de itens / identificação de loot | tempo de exame (getter compilado) **[inf]**; elite: examinar instantâneo |
| **Search** | Velocidade de busca em container | ~+33% no L50 **[inf]**; **elite: busca 2 containers ao mesmo tempo** `[cód: GClass2235.cs:84]` |

## 8. Armadura nativas EFT

| Skill | Efeito (lido do globals) |
|---|---|
| **LightVests** | Penalidade de movimento **−0,6%/nível (−30% L50)**; dano melee via colete **−0,6%/nível**; elite: **−50%** chance de desgaste no reparo `[globals:35484]` |
| **HeavyVests** | Penalidade de movimento **−0,5%/nível (−25% L50)**; blunt atravessado **−0,4%/nível**; **elite: +5% ricochete**; elite −50% desgaste `[globals:35380]` |

## 9. Mortas / meta — **não viram lever de classe**

Efeito é fora de raid (hideout/loja/bancada).

| Skill | Por que não vale | O que faz (meta) |
|---|---|---|
| **Intellect** | bancada/exame | menos desgaste e custo de reparo, exame |
| **Charisma** | loja | descontos de trader/seguro/scav case |
| **Crafting** | hideout | −tempo de craft; elite +1 produção |
| **HideoutManagement** | hideout | −consumo; slots elite |
| **WeaponTreatment** | bancada | menos perda de durabilidade ao reparar arma |
| **TroubleShooting** | passivo/raro | −malfunctions; elite −30% chance de cada falha (algum efeito in-raid) |
| **Memory** | meta | segura skills contra decay; elite: não decaem |

**FactionLocked (levers fracos):**
- **BearRawPower** (só BEAR): desconto Prapor **−0,5%/nível**, +25% XP de quest no L50, **elite −7%** em todos os traders. `[cód: Server/Patches/GetTraderAssortPatch.cs:81-99]`
- **UsecNegotiations** (só USEC): análogo com Peacekeeper e dinheiro de quest.

## 10. Achados que afetam "tratar como verdade"

1. **Bug do Círculo de Cultistas** (ShadowConnections §5.1): falta `NormalizeToPercentage()` no servidor → efeito real é instantâneo desde o nível 1, não −1%/nível. Contar com isso ou corrigir antes de usar como lever.
2. **Endurance** tem aparente copy/paste em `EnduranceSkill.cs:26` usando `BuffBreathTimeIncMax`(100) onde devia ser `BuffEnduranceIncMax`(50). Validar in-game.
3. Os números "X%/nível" do `modpage.md` do SE estão **desatualizados** (config antiga). Os valores deste catálogo seguem o `SkillsConfig.json` vigente.
4. Multiplicadores por nível das nativas (AimDrills, MagDrills, masterings, Search, Attention) são getters compilados **não extraíveis** do dump → marcados **[inf]**. Para 100% de certeza: ler no tooltip de skill em jogo ou inspecionar a DLL com dnSpy nos getters `SkillManager.get_*`.

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-06-20 | Guilherme | Criação. Catálogo quantitativo cruzando SkillsConfig.json (SE), EFT decompilado e globals.json: regra-mestra de leitura, físicas/médicas/gems (SE) + nativas (mastering, AimDrills, vests, etc.), mortas/meta e achados (bug do Círculo de Cultistas, copy/paste em Endurance). |
