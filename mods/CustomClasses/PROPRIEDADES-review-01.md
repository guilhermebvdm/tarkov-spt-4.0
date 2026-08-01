# CustomClasses — Review de Propriedades F12 · 01

**Mod:** CustomClasses
**Fonte de verdade:** `modded/Client/PerksConfig.cs` (método `Bind`) + `modded/Client/Plugin.cs` (`Awake`) · **Doc espelho:** `PROPRIEDADES.md`
**Data:** 2026-08-01

> Revisão de **UX e organização** das propriedades do menu F12 (BepInEx ConfigurationManager). Cada achado recebe um ID `MP-01-MM` **permanente**. A aplicação acontece nos `Config.Bind` (`PerksConfig.cs`/`Plugin.cs`), não no `PROPRIEDADES.md` — este é regenerado depois.
>
> ⚠️ **Breaking change:** renomear uma **seção** ou **key** recria a `ConfigEntry` (BepInEx casa por `(seção, key)` literal) e **descarta o valor salvo** do usuário, voltando ao default. Todo achado que renomeia está marcado `⚠️ BREAKING` com estratégia de migração.

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 4 · 🟡 Médios: 6 · 🟢 Menores: 2 · Total: 12
> Props analisadas: 115 (105 em `PerksConfig.cs` — inclui 14 binds de cor — + 10 em `Plugin.cs`) · Seções: 10 · Props mortas: 0 · Breaking changes propostos: 4

## Critérios avaliados

| # | Critério | Categoria |
|---|---|---|
| 1 | A ordem das seções no F12 faz sentido? Seções relacionadas ficam próximas? | **ORD** |
| 2 | As seções estão bem distribuídas? Os nomes são intuitivos e consistentes? | **SEC** |
| 3 | Cada propriedade está na seção temática certa? | **LOC** |
| 4 | O nome (key) representa bem o que a propriedade faz? | **NAM** |
| 5 | O tipo/faixa dá boa experiência de edição? | **TYP** |
| 6 | O tooltip explica de forma clara e intuitiva? (idioma consistente) | **TIP** |
| 7 | A propriedade **faz algo**? (a `ConfigEntry` é lida, ou está morta?) | **DEAD** |
| 8 | A marcação **"Advanced"** é apropriada? | **ADV** |

## Impacto

- 🔴 **Bloqueador** — prop morta, range inválido, ou nome que induz o usuário a configurar errado.
- 🟠 **Forte** — organização/alocação que atrapalha encontrar; tooltip ausente/confuso; tipo com UX ruim.
- 🟡 **Médio** — nome subótimo, "Advanced" inadequado, idioma inconsistente, seção pouco clara.
- 🟢 **Menor** — polimento (capitalização, unidade no tooltip, ordem interna).

## Panorama

### Como o ConfigurationManager ordena (a raiz do problema)

O mod **não passa nenhum `Order`** — não existe `ConfigurationManagerAttributes` no client (confirmado por grep: os 4 hits de "Order" são `OrderBy` server-side). Sem `Order`, o ConfigurationManager:
- **Ordena as SEÇÕES** alfabeticamente pelo nome → por isso os prefixos `0 ·`, `1 ·` … `9 ·` funcionam (o número força a ordem).
- **Ordena os ITENS dentro de cada seção** alfabeticamente pela **key** → por isso o toggle `— Enabled` de cada perk cai no meio, sempre que o perk tem um valor cuja primeira letra é **< E** (Adrenaline `A`DS, Iron Lungs `B`reath, Quick Draw `D`raw-in, Rooted `A`DS, Bulwark `D`amage, Pack Mule `C`arry…).

### Seções na ordem do F12 (descoberta, pelo prefixo numérico)

`0 · General` → `1 · Interface & Position` → `2 · Combat Medic` → `3 · Rifleman` → `4 · Hunter` → `5 · Stealth` → `6 · Scavenger` → `7 · Tank` → `8 · Naked` → `9 · Vanilla Skill Fixes`. **A ordem das seções está correta e coerente** (sistema → 6 classes na ordem do roster → Peladão → fixes globais). O problema de ORD é **intra-seção**, não entre seções.

### Contagem por seção (rows no F12)

| Seção | Rows | Observação |
|---|---|---|
| `0 · General` | 10 | 6 de `Plugin.cs` (PascalCase legado) + 4 de `PerksConfig.cs` |
| `1 · Interface & Position` | 9 | 4 de `Plugin.cs` (PascalCase legado) + 5 de `PerksConfig.cs` |
| `2 · Combat Medic` | 12 | 5 perks (10) + cor (2) |
| `3 · Rifleman` | 13 | + Loud Looter (bind físico no bloco Scavenger, seção = Rifleman) |
| `4 · Hunter` | 19 | **inflada** — + Light Frame (2) e Quick Draw (3) compartilhados |
| `5 · Stealth` | 11 | Execution com keys inconsistentes |
| `6 · Scavenger` | 13 | perks próprios + cor |
| `7 · Tank` | 20 | maior seção; 7 perks |
| `8 · Naked` | 2 | só cor (minúscula, intencional) |
| `9 · Vanilla Skill Fixes` | 4 | master toggle cai por último |

### Props mortas

**Nenhuma.** Todas as 105 `ConfigEntry` de `PerksConfig.cs` são lidas em `.Value` fora do arquivo de bind (patches, `PerksCatalog`, `PerkDiagnostics`, `AdrenalineState`, `StancesArmStaminaBridge`, `ClassColorOverride`). As 10 de `Plugin.cs` são materializadas em campos estáticos no `Awake`. Evidência: grep de `PerksConfig.<campo>` cobre os 91 levers + os 14 binds de cor via `ClassColors`.

### Divergências código × `PROPRIEDADES.md` (doc DEFASADO — anterior aos itens 079–088)

| # | `PROPRIEDADES.md` | Código atual | Tipo |
|---|---|---|---|
| 1 | `Mobile Surgery — Enabled` (Médico) | **removido** (079) | key fantasma |
| 2 | `Overladen — Enabled` / `Inertia mult` (Scav) | **removido** (079, virou Lebre) | key fantasma |
| 3 | `Shaky Hands — …`, default `false` | renomeado → `Unskilled — …`, default **`true`** | rename + default |
| 4 | `Silent Knife` ausente | existe (`Silent Knife — Enabled`, 083) | key faltando |
| 5 | `Lebre` / `Medroso` ausentes | existem (081/082, 5 keys) | keys faltando |
| 6 | `Light Frame` / `Loud Looter` / `Quick Draw` / `Shotgun Reload` ausentes | existem (079/080/084/087/088) | keys faltando |
| 7 | `Swift Surgeon — Surgery time mult` = `0.5` | `0.75` | default |
| 8 | `Rapid Care — Use time mult` = `0.7` | `0.75` | default |
| 9 | `Adrenaline — Reload time mult` = `0.8` | `0.7` | default |
| 10 | `Adrenaline — ADS time mult` = `0.8` | `0.7` | default |
| 11 | `Iron Lungs — Breath drain mult` = `0.667` | `0.7` | default + tooltip |
| 12 | `Bunker — Heavy weapon recoil mult` = `0.85` | `0.7` | default |
| 13 | `Tireless Arms — Heavy arm drain mult` = `0.20` | `0.5` | default + tooltip |
| 14 | `Heavy Frame — Hunger/thirst drain` = `1.3` | `1.15` | default |

### Memória consultada

Snapshot `mods/CustomClasses/memory/sessions.md` (Sessão 16, 2026-07-15) + pendências. **Pendências que afetam esta review:**
- **P-14.2** (re-teste in-game do F12 reorganizado) — a migração do `.cfg` preserva valores só enquanto as keys não mudam; qualquer rename desta review **reseta** o lever (mesmo risco já mapeado). ⚠️ reescrever o `.cfg` exige jogo FECHADO.
- **P-14.1** (card fantasma do Shaky Hands) — desatualizada: o `Enabled` do "Unskilled" (ex-Shaky Hands) hoje é `true` no código, não `false` como a pendência assume.
- Regra `feedback_version_increment_on_release`: aplicar achados = subir versão (`0.15.0` → próxima) no `[BepInPlugin]`.

---

## Achados (ORD em primeiro — foco da review)

## Índice

| ID | Cat | Impacto | Título | Breaking? | Status |
|---|---|---|---|---|---|
| MP-01-01 | ORD | 🟠 | Toggle `— Enabled` cai no meio dos valores em ~14 perks | — | Pendente |
| MP-01-02 | ORD | 🟠 | Par de cor (`Override`/`Class color`) espalhado em 7 seções | — | Pendente |
| MP-01-03 | ORD·NAM | 🟠 | Vanilla Skill Fixes: master toggle por último + valores sem prefixo | ⚠️ | Pendente |
| MP-01-04 | ORD·NAM | 🟡 | Stealth: keys do Execution com `Enabled` como sufixo (ordem quebrada) | ⚠️ | Pendente |
| MP-01-05 | Doc | 🟠 | `PROPRIEDADES.md` defasado (14 divergências) | — | Pendente |
| MP-01-06 | TIP | 🟡 | Tooltips `PT / EN` mesma linha × padrão exigido `EN\n\nPT` | — | Pendente |
| MP-01-07 | NAM·SEC | 🟡 | General/Interface: keys PascalCase legadas (feias no F12) | ⚠️ | Pendente |
| MP-01-08 | NAM | 🟡 | `Lebre`/`Medroso` em PT fogem da convenção de keys EN | ⚠️ | Pendente |
| MP-01-09 | LOC·SEC | 🟡 | Perks compartilhados só aparecem sob UMA classe | ⚠️ | Pendente |
| MP-01-10 | ADV | 🟡 | `Perk Diagnostics overlay` (debug) deveria ser Advanced | — | Pendente |
| MP-01-11 | DEAD | 🟢 | Zero props mortas (nota positiva) | — | Informativo |
| MP-01-12 | SEC | 🟢 | Hunter inflada / Naked minúscula | — | Pendente |

---

### MP-01-01 · ORD — organização · 🟠 Forte

**Toggle `— Enabled` cai no MEIO dos valores em ~14 perks (ordenação alfabética por key)**

**Local:** todas as seções de classe · vários · `PerksConfig.cs:172-612` (todos os binds)

**Problema:** sem `Order`, o ConfigurationManager ordena cada seção **alfabeticamente pela key**. O toggle `<Perk> — Enabled` fica **abaixo** de qualquer valor cuja primeira palavra comece com letra `< E`. Perks afetados (o `— Enabled` NÃO é o 1º item do grupo):

| Perk | Seção | Ordem alfabética atual (F12) | Onde cai o `Enabled` |
|---|---|---|---|
| **Adrenaline** | Rifleman | `ADS time` · `Cooldown` · **`Enabled`** · `Recoil` · `Reload` · `Window` | 3º de 6 |
| **Iron Lungs** | Hunter | `Breath drain` · **`Enabled`** | 2º de 2 |
| **Sharpshooter** | Hunter | `ADS time` · **`Enabled`** | 2º de 2 |
| **Steady Arms** | Hunter | `ADS arm drain` · **`Enabled`** | 2º de 2 |
| **Rooted** | Hunter | `ADS move speed` · **`Enabled`** | 2º de 2 |
| **Light Frame** | Hunter | `Carry limit penalty` · **`Enabled`** | 2º de 2 |
| **Quick Draw** | Hunter | `Draw-in (phase 3)` · **`Enabled`** · `Put-away (phase 1)` | 2º de 3 |
| **Rattled** | Stealth | `Aim-punch` · **`Enabled`** | 2º de 2 |
| **Medroso** | Scavenger | `Cooldown` · **`Enabled`** · `Suppression` · `Tremor duration` | 2º de 4 |
| **Pack Mule** (Scav) | Scavenger | `Carry limit bonus` · **`Enabled`** | 2º de 2 |
| **Pack Mule** (Tank) | Tank | `Carry limit bonus` · **`Enabled`** | 2º de 2 |
| **Bulwark** | Tank | `Damage taken` · **`Enabled`** · `Min armor class` · `Require heavy armor` | 2º de 4 |
| **Weapon Mastery** | Vanilla Fixes | (ver MP-01-03) | último |
| **Execution** (move speed) | Stealth | (ver MP-01-04) | valor antes do toggle |

**Por que importa (UX):** o usuário abre o perk, vê primeiro um **slider de valor** e só depois o liga/desliga — exatamente a queixa relatada. Em seções grandes (Hunter, Tank) fica difícil parear cada toggle ao seu valor.

**Sugestão (recomendada — NÃO é breaking):** introduzir `Order` decrescente automático por ordem de registro. O código **já binda `Enabled` antes dos valores** em todos os perks, então basta fazer o F12 respeitar a ordem do código. Ver a proposta técnica no fim do relatório (helper `BindOrdered`). Isso resolve MP-01-01 e MP-01-02 de uma vez, sem tocar em `(seção, key)` → **valores salvos preservados**.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar: _________________

---

### MP-01-02 · ORD — organização · 🟠 Forte

**O par de cor (`Override color` + `Class color`) fica separado e espalhado em cada uma das 7 seções de classe**

**Local:** `2·Medic`…`8·Naked` · `Override color` / `Class color` · `PerksConfig.cs:623-628` (`BindClassColor`)

**Problema:** alfabeticamente, `Class color` (`C`) sobe para **perto do topo** da seção e `Override color` (`O`) cai para o **meio/fim** — os dois controles que formam UM par ficam desconectados, e a cor (item raramente mexido) se intromete entre os perks. Ex. Combat Medic: `Class color` é o 1º item da seção, antes de todos os perks; `Override color` cai entre Efficient Metabolism e Rapid Care.

**Por que importa (UX):** o toggle "ligar override" e o color-picker que ele controla deveriam estar juntos; hoje o usuário mexe na cor sem ver o toggle que a ativa (ou vice-versa). Além disso, empurram um controle cosmético para o topo de uma seção de gameplay.

**Sugestão (mesmo helper do MP-01-01):** com `Order` decrescente por ordem de registro e a `BindClassColor` sendo a **última** chamada de cada bloco de classe, o par de cor cai **naturalmente para o fundo** da seção, com `Override color` acima de `Class color` (ordem de bind). Sem rename → não breaking. (Se quiser garantir cor sempre por último mesmo com perks compartilhados no meio, dar à cor um `Order` fixo baixo.)

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar: _________________

---

### MP-01-03 · ORD·NAM — ⚠️ BREAKING · 🟠 Forte

**Vanilla Skill Fixes: o master toggle cai por último e os 3 valores não têm prefixo comum**

**Local:** `9 · Vanilla Skill Fixes` · `Weapon Mastery — Enabled` + 3 valores · `PerksConfig.cs:595-612`

**Problema:** a seção tem 4 keys: `Ergo bonus per level`, `Recoil bonus per level`, `Underbarrel XP per shot`, `Weapon Mastery — Enabled`. Ordenadas alfabeticamente, o **master toggle `Weapon Mastery — Enabled` (`W`) vem por ÚLTIMO**, com os 3 valores que ele governa acima. Pior: os 3 valores **não compartilham o prefixo** `Weapon Mastery — `, então não se agrupam visualmente com o toggle nem entre si.

**Por que importa (UX):** o usuário vê 3 sliders soltos e o liga/desliga geral no rodapé — invertido e desagrupado.

**Sugestão:** (a) via helper de `Order` o toggle sobe para o topo (resolve a ordem sem rename); (b) para agrupar de fato, renomear as 3 value-keys com prefixo — `Weapon Mastery — Underbarrel XP per shot`, `Weapon Mastery — Recoil bonus per level`, `Weapon Mastery — Ergo bonus per level`. ⚠️ **BREAKING** (reseta os 3 valores salvos ao default). Migração: nota no changelog "Vanilla Skill Fixes: 3 sliders resetados ao default (rename de key)". O toggle `Weapon Mastery — Enabled` **não** muda → preservado.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão (só (a), Order, sem rename)
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar: _________________

---

### MP-01-04 · ORD·NAM — ⚠️ BREAKING · 🟡 Médio

**Stealth/Execution: `Enabled` como SUFIXO da key quebra a ordem e o padrão**

**Local:** `5 · Stealth` · `Execution — Melee move speed Enabled` / `Execution — Melee move speed` / `Execution — Melee damage Enabled` / `Execution — Melee damage mult` · `PerksConfig.cs:378-395`

**Problema:** o Execution são 2 sub-perks (velocidade com melee, dano de melee). As keys fogem do padrão `<Perk> — Enabled` do resto do mod: aqui o `Enabled` é **sufixo** (`Execution — Melee move speed Enabled`). Como a value-key `Execution — Melee move speed` é **prefixo** da toggle-key `Execution — Melee move speed Enabled`, o **valor sorteia ANTES do próprio toggle**. Ordem no F12: `Melee damage Enabled` · `Melee damage mult` · `Melee move speed` · `Melee move speed Enabled` — o toggle de move-speed vira o 4º/último.

**Por que importa (UX):** o único perk do mod cujo toggle vem depois do valor por causa do prefixo; padrão de nome inconsistente confunde.

**Sugestão:** renomear para o padrão prefixo, tratando como 2 sub-perks: `Execution Speed — Enabled` / `Execution Speed — Move speed mult` e `Execution Melee — Enabled` / `Execution Melee — Damage mult`. ⚠️ **BREAKING** (reseta os 4). Migração: changelog. Alternativa não-breaking: só o helper de `Order` (a ordem de bind já é Enabled→valor→Enabled→valor), que corrige a ordem sem tocar nas keys — mas o padrão de nome inconsistente permanece.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão (rename)
- `[ ]` Aceitar com modificação (só Order, sem rename): _________________
- `[ ]` Rejeitar: _________________

---

### MP-01-05 · Doc — reconciliação · 🟠 Forte

**`PROPRIEDADES.md` está defasado — 14 divergências com o código (anterior aos itens 079–088)**

**Local:** `mods/CustomClasses/PROPRIEDADES.md` (todo) × `PerksConfig.cs`

**Problema:** ver tabela no Panorama. O doc lista keys que já não existem (`Mobile Surgery`, `Overladen`, `Shaky Hands`), não lista 7 perks novos (`Silent Knife`, `Lebre`, `Medroso`, `Light Frame`, `Loud Looter`, `Quick Draw`, `Shotgun Reload`) e tem 8 defaults divergentes.

**Por que importa (UX):** o doc é a referência que o usuário lê fora do jogo; hoje engana. (O `.md` **não** é editado neste command — mas a divergência é registrada aqui e o doc é **regenerado** depois que os achados de código forem aplicados.)

**Sugestão:** regenerar `PROPRIEDADES.md` **e** `PROPERTIES.md` a partir do código após aplicar MP-01-01..04. Não breaking (só doc).

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Rejeitar: _________________

---

### MP-01-06 · TIP — idioma · 🟡 Médio

**Tooltips usam `PT / EN` na mesma linha; o padrão exigido pelo command é `EN\n\nPT`**

**Local:** todos os `ConfigDescription` · `PerksConfig.cs` + `Plugin.cs`

**Problema:** o mod escreve `"<Português> / <English>"` numa linha só (ex.: `"Notificação no início da raid… / Raid-start notification…"`). O padrão mandatório do `/review-mod-properties` é **inglês na 1ª linha, linha em branco, português na 3ª** (`"<English>\n\n<Português>"`). Todos os ~115 tooltips divergem — porém isto é uma **convenção deliberada** do mod, documentada em `PROPRIEDADES.md` ("descrições bilíngues `PT / EN` na mesma linha").

**Por que importa (UX):** conflito entre a convenção do repo (command) e a do mod. É preciso decidir qual vale — não é um bug de tooltip individual.

**Sugestão:** decisão do usuário. Opção A: converter os tooltips para `EN\n\nPT` (não breaking — só a descrição muda; ~115 edições mecânicas). Opção B: manter a convenção do mod e registrar a exceção no command/na doc do repo. Recomendo **B** (menor churn, padrão já consolidado e documentado), a menos que o repo queira uniformizar todos os mods.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar (A — converter p/ EN\n\nPT)
- `[ ]` Aceitar (B — manter PT / EN e documentar exceção)
- `[ ]` Rejeitar: _________________

---

### MP-01-07 · NAM·SEC — ⚠️ BREAKING · 🟡 Médio

**General/Interface: keys PascalCase legadas aparecem cruas no F12**

**Local:** `0 · General` e `1 · Interface & Position` · `EnableSkillMultipliers`, `ShowMultiplierOnSkills`, `ShowClassOnPlayerName`, `ShowClassIdentity`, `ShowSkillsButton`, `ShowLevelUpFlavor`, `SkillsClassPosX`, `SkillsClassPosY`, `ClassIconRatio`, `DeployNameScale` · `Plugin.cs:36-82`

**Problema:** essas 10 keys são identificadores de código (PascalCase, sem espaço) e o ConfigurationManager as mostra **literalmente** como rótulo — `EnableSkillMultipliers`, `SkillsClassPosX` — quebrando a estética das keys em Title Case com espaços (`Raid-start perks notification`, `Weight Marker — X offset`) que convivem na MESMA seção.

**Por que importa (UX):** rótulos técnicos crus no meio de rótulos legíveis; a seção parece inacabada.

**Sugestão:** renomear para Title Case, ex.: `EnableSkillMultipliers`→`Skill XP scaling — Enabled`; `ShowMultiplierOnSkills`→`Skill multiplier highlight`; `ShowClassOnPlayerName`→`Class identity on player name`; `ShowClassIdentity`→`Class seal (menu + Skills)`; `ShowSkillsButton`→`SKILLS menu button`; `ShowLevelUpFlavor`→`Level-up flavor text`; `SkillsClassPosX/Y`→`Class seal — X/Y offset`; `ClassIconRatio`→`Class icon size ratio`; `DeployNameScale`→`Deploy name scale`. ⚠️ **BREAKING** (reseta os 10; a maioria é default, mas o usuário pode ter calibrado `SkillsClassPosX/Y`, `ClassIconRatio`, `DeployNameScale` — ver P-14.2). Migração: changelog + avisar que esses offsets voltam ao default.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (dívida): _________________

---

### MP-01-08 · NAM — ⚠️ BREAKING · 🟡 Médio

**`Lebre` e `Medroso` (português) fogem da convenção de keys em inglês**

**Local:** `6 · Scavenger` · `Lebre — …` / `Medroso — …` · `PerksConfig.cs:480-501`

**Problema:** todas as keys de perk estão em inglês (`Ghost Step`, `Iron Lungs`, `Pack Mule`), mas Lebre (Hare) e Medroso (Fearful) ficaram em PT. As keys do F12 são explicitamente EN por convenção do mod.

**Por que importa (UX):** inconsistência de idioma nas keys; um usuário EN não reconhece "Lebre"/"Medroso".

**Sugestão:** se quiser uniformizar, renomear para `Hare — …` / `Rattled Scav — …` (ou manter o nome-fantasia PT como decisão de flavor). ⚠️ **BREAKING** (reseta 7 keys). Baixa prioridade — flag de consistência, não de função. Alternativa: manter as keys e só garantir que os cards in-game mostrem o nome localizado.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (manter flavor PT): _________________

---

### MP-01-09 · LOC·SEC — ⚠️ BREAKING · 🟡 Médio

**Perks compartilhados só aparecem sob UMA classe — invisíveis para as outras que os têm**

**Local:** `Light Frame` (Caçador+Furtivo) e `Quick Draw` (Caçador+Fuzileiro+Furtivo) → seção **Hunter**; `Loud Looter` (Fuzileiro) → seção **Rifleman** · `PerksConfig.cs:446-478`

**Problema:** o bind físico está no bloco Scavenger mas o 1º arg manda para Hunter/Rifleman. Um lever compartilhado aplica a várias classes, mas só aparece sob **uma** delas. Quem joga de Furtivo procura `Light Frame`/`Quick Draw` na sua seção (Stealth) e não acha — estão em Hunter.

**Por que importa (UX):** o usuário não encontra a config de um perk que a sua classe tem. Escolha de seção é arbitrária (a 1ª classe do trio).

**Sugestão:** opções — (a) criar seção dedicada `X · Shared perks` para os levers multi-classe (mais claro, mas ⚠️ BREAKING pois muda a seção → reseta); (b) manter e adicionar no tooltip a lista de classes que compartilham (já parcialmente presente); (c) documentar no `PROPRIEDADES.md`. Recomendo (b)+(c) agora (não breaking) e considerar (a) num rework maior.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar (a — seção Shared, BREAKING)
- `[ ]` Aceitar (b/c — tooltip + doc, não breaking)
- `[ ]` Rejeitar: _________________

---

### MP-01-10 · ADV — exposição · 🟡 Médio

**`Perk Diagnostics overlay` (debug/validação) deveria ser `IsAdvanced`**

**Local:** `0 · General` · `Perk Diagnostics overlay` · `PerksConfig.cs:175-178`

**Problema:** o próprio tooltip diz "Só para validação". É um overlay de debug + log de peers no `LogOutput.log`, não um ajuste de jogo — mas está exposto no topo do F12 como qualquer setting comum. O mod não usa `Advanced` em lugar nenhum.

**Por que importa (UX):** polui a lista comum com uma ferramenta de dev; risco de o usuário ligar sem querer e ver overlay/spam de log.

**Sugestão:** marcar `IsAdvanced = true` (via o mesmo `ConfigurationManagerAttributes` local do helper de `Order`). Não breaking (só atributo). Candidatos secundários a `IsAdvanced`: `Recoil bonus per level` / `Ergo bonus per level` (knobs finos de balance).

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar: _________________

---

### MP-01-11 · DEAD — nota positiva · 🟢 Menor

**Zero propriedades mortas**

**Local:** todo `PerksConfig.cs` / `Plugin.cs`

**Problema:** nenhum — registrado como evidência. As 105 entries de `PerksConfig` têm `.Value` lido fora do bind (patches / `PerksCatalog` / `PerkDiagnostics` / `AdrenalineState` / `StancesArmStaminaBridge` / `ClassColorOverride`); as 10 de `Plugin.cs` viram campos estáticos no `Awake`.

**Sugestão:** nenhuma ação. Manter o padrão (todo lever novo deve ter um leitor).

**Decisão:**
- `[x]` Informativo (sem ação)

---

### MP-01-12 · SEC — distribuição · 🟢 Menor

**Hunter inflada (19 rows) / Naked minúscula (2 rows)**

**Local:** `4 · Hunter` e `8 · Naked`

**Problema:** Hunter carrega os compartilhados Light Frame + Quick Draw (ver MP-01-09), virando a maior seção depois de Tank; Naked tem só o par de cor. Ambos têm justificativa (compartilhados / Peladão sem perks), mas o desbalanço é perceptível.

**Por que importa (UX):** menor — navegação. Resolver MP-01-09 (mover compartilhados) reequilibraria Hunter.

**Sugestão:** tratar junto com MP-01-09; Naked fica como está (intencional, documentado).

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar (via MP-01-09)
- `[ ]` Rejeitar (aceitar como está): _________________

---

## Proposta técnica — ordenação por `Order` decrescente automático (resolve MP-01-01 e MP-01-02)

**Confirmações:**
- `ConfigDescription` aceita tags: assinatura `ConfigDescription(string description, AcceptableValueBase acceptableValues = null, params object[] tags)` — o 3º+ arg é `params object[] tags`. ✅
- O ConfigurationManager lê o campo `Order` (`int?`) por **duck-typing** de qualquer objeto nas tags cujo tipo tenha um campo/propriedade chamado `Order` (idem `IsAdvanced`, `bool?`). O mod define a classe localmente. ✅
- **`Order` MAIOR = mais no TOPO** da seção; o ConfigurationManager ordena por `Order` desc e, em empate, pela key.
- `Order` afeta **só a ordem intra-seção** — a ordem entre seções continua vindo do prefixo numérico do nome. ✅ (não mexe na ordem das 10 seções).

**Abordagem recomendada: helper com contador decrescente (opção b), NÃO per-bind manual.**

Para 115 binds, numerar `Order` à mão é frágil (renumerar a cada inserção). O helper injeta `Order` decrescente na **ordem em que os binds aparecem no código** → o F12 passa a espelhar o código. Como o código **já escreve `Enabled` antes dos valores** e chama `BindClassColor` no fim de cada bloco, o resultado sai logicamente ordenado **sem reordenar uma linha sequer**:

```csharp
// Espelho duck-typed dos atributos do ConfigurationManager (os NOMES dos campos têm que bater).
internal sealed class ConfigurationManagerAttributes
{
    public int? Order;
    public bool? IsAdvanced;
}

private static int _order = short.MaxValue;   // decrementa a cada bind → 1º bind = topo

// Overload com range
private static ConfigEntry<T> BindOrdered<T>(
    ConfigFile cfg, string section, string key, T def, string tooltip,
    AcceptableValueBase? range = null, bool advanced = false)
{
    var attr = new ConfigurationManagerAttributes { Order = _order--, IsAdvanced = advanced ? true : (bool?)null };
    return cfg.Bind(section, key, def, new ConfigDescription(tooltip, range, attr));
}
```

Aplicação: trocar cada `config.Bind(sec, key, def, "tip")` por `BindOrdered(config, sec, key, def, "tip")` e cada `config.Bind(sec, key, def, new ConfigDescription("tip", range))` por `BindOrdered(config, sec, key, def, "tip", range)`. `BindClassColor` e os binds do `Plugin.cs` também passam pelo helper (o contador é `internal static` compartilhado ou o `Plugin.cs` chama antes do `PerksConfig.Bind`, como já faz — General/Interface pegam os `Order` mais altos = topo, coerente).

**Custo/benefício:**
- **Prós:** zero rename → **não breaking** (valores salvos intactos); ~30 min mecânicos; a ordem vira "o que está escrito primeiro no código aparece primeiro" — intuitivo de manter; habilita `IsAdvanced` de brinde (MP-01-10).
- **Ressalvas:** (1) os compartilhados (Light Frame/Quick Draw) são bindados no bloco Scavenger mas renderizam em Hunter — como têm `Order` menor que os perks físicos do Hunter, caem no **fim** da seção Hunter (aceitável; se quiser cor sempre por último ali, dar `Order` fixo baixo à cor). (2) inserir bind novo desloca os `Order` seguintes, mas como nada é persistido em disco e recalcula no boot, é inofensivo.
- **Rejeitado:** `Order` manual por bind (115 números frágeis) — só vale se quisessem uma ordem que NÃO siga o código.

---

## Histórico

| Data | Evento |
|---|---|
| 2026-08-01 | Review de propriedades 01 criada via `/review-mod-properties` |
