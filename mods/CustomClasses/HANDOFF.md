# CustomClasses — Handoff (2026-07-03)

> Contexto pra continuar o trabalho de UI/perks do mod em outra sessão. Foco da sessão: **059** (aba CLASS +
> catálogo atômico), **055** (detalhe no loading FIKA), **056** (marcador de peso), specs do **058**.

## Estado do repositório

- **Branch:** `feat/053-perks-property-model` · **nada em push** (só commits locais).
- **Commits da sessão:** `a57b48c` (baseline 053) → `76904b9` (HEAD). Sequência relevante:
  `a1a24d5` 059 · `663a38a`+`529c433` 055 · `ef8b8d8`+`7f3db04` 056 · `6153adf` specs 058 · `76904b9` fixes pós-teste.
- **Reverter tudo da sessão:** `git reset --hard a57b48c`.
- **DLL client instalada:** `D:/SPT/BepInEx/plugins/CustomClasses/CustomClasses-Client.dll` (109056 bytes).
- Servidor: **FIKA Coop PVE** (SPT 4.0.13 / EFT 0.16.9 / FIKA 2.3.3 / SAIN 4.4.3).

## Como buildar / rodar

- **Compilar+instalar:** `bash .agents/scripts/compile-mod.sh CustomClasses` (client → `BepInEx/plugins/`, server → `SPT/user/mods/`).
- **F12 novos só aparecem após REINICIAR o EFT** (plugin BepInEx recarrega só no boot).
- **Refs de UI do EFT:** o decompile curado (`references/eft-decompiled/`) **NÃO tem `EFT.UI`** → usar
  `ilspycmd "D:/SPT/EscapeFromTarkov_Data/Managed/Assembly-CSharp.dll" -t <FQN>`. Ex.: `EFT.UI.Health.HealthParametersPanel`.
- Validação in-game = **gate humano** (compilar ≠ funcionar, AP-06). Usuário testa no servidor (via AnyDesk).

## O que foi entregue nesta sessão (validado in-game ✅)

| Item | Estado | Notas |
|---|---|---|
| **059** aba CLASS + catálogo atômico (2 colunas) | ✅ funciona | perk/drawback + valor **derivados** do multiplicador+polaridade. `PerksCatalog` (PerkGroup/PerkLine). Painel extraído p/ `PerksPanelView` (reusado por 055). |
| **055** detalhe da classe no loading FIKA | ✅ funciona | Só classe **local**. `76904b9` mudou p/ **hover-only** (era auto-visível, cobria o carrossel). Per-player = item 057. |
| **056** marcador ▲ +30% no peso (Pack Mule) | ✅ funciona | Postfix em `HealthParametersPanel.method_0`; molde do `SkillPanelPatch`. `7f3db04` add F12 `Weight Marker — X/Y offset` (o marcador nasce mal posicionado — usuário calibra). |
| **058** ativar masteries inertes | 📄 specs only | Recon: globals **não pega** → patch client. **Code-mod BLOQUEADO** por validação prévia (ver pendências). |

## PENDÊNCIAS (priorizadas)

### 1. 🟡 059 CLASS #3 — "1 card por efeito" (IMPLEMENTADO 2026-07-03, validar in-game)
Implementado conforme a decisão do usuário + requisito novo: **ícone por efeito reusa os quadradinhos de buff da
tela SKILLS** (`EFT.UI.BuffIcon.smethod_0` → `StaticIcons.BuffIdSprites[EBuffId]` — irmão do `SkillIdSprites`).
- `PerksCatalog.PerkLine` ganhou `EBuffId Icon` (mapeado nas 18 entradas da Library) + `PerksCatalog.BuffSprite(line)`.
- `PerksPanelView`: `BuildGroupCard` → **`BuildEffectCard`** (1 card por `PerkLine`): frame 40px com ícone do efeito
  (fallback = ícone do grupo), nome do GRUPO esmaecido (UpperCase, cor do acento a 75%) + chip `ValueToken` +
  `Label` em destaque; acento/bg por `line.IsPerk`/`line.Pending`; coluna continua por `group.IsPerk`.
- **Validar in-game:** (a) os sprites de `BuffIdSprites` aparecem? (b) mapeamentos semânticos fazem sentido visual?
  (ex.: flinch/aim punch → `AimMasterWiggle`, dano recebido → `HealthEliteAbsorbDamage`); (c) altura dos cards no
  Tanque (6 cards na coluna de perks) cabe sem scroll.

### 2. 🟠 059 CLASS #1 — título da aba cortado ("ASS" em vez de "CLASS")
A aba CLASS fica muito à esquerda e o "CL" é cortado pela margem da tela.
- **Falta:** o log `[CustomClasses][053-tabs]` do console BepInEx (dá as posições X reais de CLASS/SKILLS/MASTERING) — o usuário vai colar. Com ele, corrigir o cálculo em [`SkillsClassTabPatch.cs`](modded/Client/Patches/SkillsClassTabPatch.cs) (`cRt.anchoredPosition = sRt.x - classW - gap + offsetX`).
- **Melhoria:** o F12 `Class Tab — X offset` **só aplica no boot** (a aba é montada 1× — idempotência bloqueia recalcular). Tornar "live" (reposicionar no `SettingChanged`) OU recalcular a cada `Show`.

### 3. 🟡 059 CLASS #2 — chip ✓/✗ nas flags (implementado `76904b9`, VERIFICAR)
Linhas Flag (`no ergo penalty`, `no arm fatigue`) agora têm chip **✓** (perk) / **✗** (drawback) — `MultiplierFormat.ValueToken`.
- **Verificar in-game:** o glyph ✓ (U+2713) / ✗ (U+2717) renderiza na fonte do EFT? Se aparecer **□**, trocar por `ON`/`OFF` ou `+`/`−` no `ValueToken` (Flag). **▲▼ sabidamente funcionam** (marcador de peso).

### 4. 🟡 055 — validar hover-only + zoom-out + pendências do code-review 02
- Confirmar in-game que o **hover** dispara na tela de deploy (EventSystem/GraphicRaycaster ativo?). Se não disparar, o popover não aparece.
- **Zoom-out do popover (2026-07-03):** `LoadingClassHover.ApplyScale()` — escala default **0.75** com rect
  compensado (÷ escala) → mesma pegada visual ~600×460, +33% de espaço interno pros cards por efeito. F12
  `Class Detail — Loading panel scale` (0.5–1.0), lido **a cada hover** (live). Validar se cabe tudo (Tanque = pior caso).
- `04-code-review-02`: **CR-02-03** (re-add do painel em mapa com **trânsito**, ex. Streets), **CR-02-04** (posição do painel 600×460 em 1280×720).

### 5. 🟡 056 — calibrar F12 e fixar default
Usuário ajusta `Weight Marker — X/Y offset` (F12 → `Perks — UI`) até posicionar bem (chute inicial X≈−70, Y≈+30) e passa os valores → fixar como **default** no `PerksConfig` (aí dispensa o F12).

### 6. 🟢 057 — identidade de classe **per-player** em coop (o que o usuário quer no loading)
Fazer o popover do 055 funcionar no nome de **cada** player (não só o local) — o caso coop mais útil. Exige **rota no server** expondo o registry de classes (resolver por `Profile.Info.GameVersion` → displayName en+pt) + `ClassIconCache` de todas as classes. **⚠️ toca `modded/Server`** → coordenar com a sessão do editor. Backlog item 057. **Desbloqueia o per-player do 055.**

### 7. 🟢 058 — rodar validação prévia in-game (destrava o code-mod)
Protocolo V1–V4 na [`058-...-01-spec.md`](backlog/058-ativar-masteries-inertes/058-ativar-masteries-inertes-01-spec.md):
V1 (SMG/LMG já sobem?) · V2 (persiste entre raids? — se não, precisa server) · V4 (underbarrel detectável? HMG≠LMG?).
Sem esses resultados, codar a mecânica é chute. Assunção registrada: coexistir com Bunker.

### 8. 🟢 051 — decisão de design (stances zone)
Iron Lungs (braço-ADS) + Bunker (arm stamina) caem na zona do stances mod. Decidir **(a) coordenar** (mexer nos 2 mods) vs **(b) trocar o lever** (fora da zona). Só kickoff, sem spec. Detalhe já explicado ao usuário nesta sessão.

## Arquitetura / arquivos-chave (client)

- [`PerksCatalog.cs`](modded/Client/PerksCatalog.cs) — `PerkGroup`/`PerkLine`; `IsPerk`+`ValueToken` **derivados** de `Multiplier`+`Polarity`+`Format`. `Library` (18 grupos) + `ByClass`. `LocalGroups()`, `IconSprite()`, `BuildNotificationText()`.
- [`PerksPanelView.cs`](modded/Client/PerksPanelView.cs) — painel reutilizável (header + 2 colunas + cards + `CardHover`/`FadeIn`). **Alvo da pendência #1.**
- [`MultiplierFormat.cs`](modded/Client/MultiplierFormat.cs) — `Marker()` (▲/▼), `ValueToken()` (chips), `TooltipText()`, `CarryTooltip()` (056). i18n aqui.
- [`Patches/SkillsClassTabPatch.cs`](modded/Client/Patches/SkillsClassTabPatch.cs) — só a **aba** (clone tab, toggle-group, overlay [ícone]CLASS, posição). **Pendência #2.**
- [`Patches/ClassDetailLoadingPatch.cs`](modded/Client/Patches/ClassDetailLoadingPatch.cs) — 055; soft-detect FIKA (`TypeByName("LoadingScreenUI")`), `LoadingClassHover` (hover-only).
- [`Patches/WeightMarkerPatch.cs`](modded/Client/Patches/WeightMarkerPatch.cs) — 056; molde do [`SkillPanelPatch.cs`](modded/Client/Patches/SkillPanelPatch.cs).
- [`Patches/PackMulePatch.cs`](modded/Client/Patches/PackMulePatch.cs) — gate reusado: `SkillMultipliers.IsLocalClass("Scavenger"|"Tank")` + `PerksConfig.PackMuleEnabled`.

## Constraints (invioláveis)

- Commits locais **livres**; **push/PR/deploy exigem aprovação** (menu interativo).
- **Não tocar `modded/Server`** sem coordenar (sessão paralela do editor web, server-side) — vale p/ 057.
- Idioma: comunicação **pt-BR**, código/commits em **inglês**.
- Padrão SDD do repo (`WORKFLOW.md`): backlog → spec → review-spec → spec-tech → review-tech → code-mod → asbuild → code-review → compile → **gate humano**. Artefatos em `backlog/NNN-<slug>/`.

## Memória relevante (`~/.claude/projects/.../memory/`)

- `feedback_coop_multiplayer_sync` — servidor é FIKA coop; sinalizar gaps de sync (solo mascara bugs de cliente).
- `feedback_spt_validation` — escritas SPT precisam validação in-game, não só compile.
- `reference_spt_localedb_per_call_cost`, `reference_spt_helper_cache_timing` — perf/timing de helpers SPT.
