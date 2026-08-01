# PROPRIEDADES.md — CustomClasses (client F12 / BepInEx)

Plugin: `customclasses.mdj.client` ("CustomClasses") — ver [modded/Client/Plugin.cs](modded/Client/Plugin.cs) e [modded/Client/PerksConfig.cs](modded/Client/PerksConfig.cs). Versão EN: [PROPERTIES.md](PROPERTIES.md).

Propriedades expostas no menu de configuração (F12 / ConfigurationManager). Uma é **(Avançado)**: `Perk Diagnostics overlay` (escondida atrás do filtro *Advanced* do F12).

> **Organização (reorg 2026-07-10):** uma **seção por classe** (perks + drawbacks juntos). O prefixo numérico (`0 ·`, `1 ·`…) força a ordem no F12 — o ConfigurationManager ordena as seções alfabeticamente. Ordem: sistema (`0`/`1`), depois as 6 classes na ordem do roster (`2`–`7`), depois o **Peladão** (`8`), e por fim os fixes globais (`9`).
>
> **Ordem DENTRO da seção (review de propriedades 2026-08-01):** agora segue a **ordem dos binds no código** — o helper `BindOrdered` injeta um `Order` decrescente, então o F12 espelha o código (o `— Enabled` de cada perk vem primeiro, a cor por último). Antes o ConfigurationManager ordenava alfabético pela key, jogando o `— Enabled` para o meio e separando o par de cor.
>
> **Idioma:** o F12 é do **BepInEx, não do EFT** — não segue o idioma do jogo (as strings são fixadas no `Awake`, antes de o EFT carregar o locale). Por isso os **nomes de seção/propriedade ficam em inglês** e as **descrições (tooltips) são bilíngues `PT / EN`** na mesma linha. Este arquivo (PT) e o [PROPERTIES.md](PROPERTIES.md) (EN) documentam por idioma.
>
> **Compartilhados desdobrados (config própria por classe):** Pack Mule (Saqueador + Tanque) e Loud Operator (Fuzileiro + Tanque) têm **config própria por classe** — cada seção mostra os seus, com valores independentes.
>
> **Compartilhados com config única:** Light Frame (Caçador + Furtivo) e Quick Draw (Caçador + Fuzileiro + Furtivo) têm **UMA config só**, na seção do Caçador (`4`), aplicada a todas as classes que os possuem. Aparecem **abaixo** do par de cor do Caçador porque são bindados depois no código (fisicamente no bloco do Saqueador). O Loud Looter (`3` Fuzileiro) tem o mesmo efeito colateral de ordem.

---

## Seção `0 · General`

| Propriedade (EN) | Tipo | Padrão | O que faz |
|---|---|---|---|
| `Skill XP scaling — Enabled` | bool | `true` | Liga/desliga a escala de ganho de XP de skill por classe. |
| `Skill multiplier highlight` | bool | `true` | Destaque do multiplicador nas skills (borda colorida + seta ±X% + tooltip da classe). |
| `Class identity on player name` | bool | `true` | Ícone + nome (gradiente) da classe no nome do jogador (deploy, character, lista online). (item 015) |
| `Class seal (menu + Skills)` | bool | `false` | Selo separado da classe no menu e no topo da tela de Skills. (item 012) |
| `SKILLS menu button` | bool | `true` | Botão SKILLS no menu (abaixo de CHARACTER) que abre a tela de Skills. (item 013) |
| `Level-up flavor text` | bool | `true` | Notificação de level-up `EASILY` (buff) / `FINALLY` (debuff) nas skills com multiplicador. (item 014) |
| `Raid-start perks notification` | bool | `true` | Notificação no início da raid listando os perks (verde) e drawbacks (vermelho) da classe. |
| `Perk Diagnostics overlay` | bool | `false` | **(Avançado)** Overlay ao vivo das propriedades afetadas pelos perks do seu player + log dos perks de SOM aplicados aos peers (coop; `LogOutput.log`). Só validação. |
| `Recoil floor — Enabled` | bool | `true` | **Piso do recuo COMBINADO** (maestria × perks). Os multiplicadores empilham por produto e o produto não tinha piso. (balance B15) |
| `Recoil floor — Min combined mult` | float | `0.60` | Recuo mínimo como fração do original (0.60 = nunca abaixo de −40% no total). Faixa 0.3..1. (balance B15) |

## Seção `1 · Interface & Position`

> Offsets (px) da UI da identidade da classe. `Class seal — X/Y offset` são **sliders** que aplicam em tempo real (com a tela de Skills aberta).

| Propriedade (EN) | Tipo | Padrão | Faixa | O que faz |
|---|---|---|---|---|
| `Class seal — X offset` | float | `0` | −1000..1000 | Selo da classe (tela de Skills) — offset horizontal do centro. |
| `Class seal — Y offset` | float | `-20` | −1000..1000 | Selo da classe (tela de Skills) — offset vertical do topo (negativo = para baixo). |
| `Class icon size ratio` | float | `1.35` | 0.8..2.5 | Tamanho do ícone = fonte do nome × ratio (mantém a proporção ícone:fonte em todas as telas). |
| `Deploy name scale` | float | `1.2` | 1.0..4.0 | Escala do ícone+nome do jogador na tela de deploy/loading (ícone e nome crescem juntos). |
| `Class Tab — X offset` | float | `0` | −400..400 | Ajuste fino da posição horizontal do botão da aba CLASS. |
| `Class Detail on Loading Screen` | bool | `true` | — | Detalhe da classe (perks/drawbacks) no seu nome na tela de loading da raid (FIKA). (item 055) |
| `Class Detail — Loading panel scale` | float | `0.75` | 0.5..1.0 | Escala (zoom-out) do popover de classe no loading (mesma área, conteúdo menor). |
| `Weight Marker — X offset` | float | `-107.0423` | −600..600 | Posição horizontal do marcador `▲ +X%` no peso (aba Health). Default calibrado in-game. (item 056) |
| `Weight Marker — Y offset` | float | `50.70423` | −600..600 | Posição vertical do marcador `▲ +X%` no peso (positivo = para cima). Default calibrado in-game. (item 056) |

## Seção `2 · Combat Medic`

| Propriedade (EN) | Tipo | Padrão | Faixa | O que faz |
|---|---|---|---|---|
| `Efficient Metabolism — Enabled` | bool | `true` | — | Perk: fome/sede drenam mais devagar. |
| `Efficient Metabolism — Hunger/thirst drain` | float | `0.85` | 0.5..1 | Dreno de fome/sede (0.85 = 15% mais devagar). |
| `Rapid Care — Enabled` | bool | `true` | — | Perk (**072**): curativos/estabilizações mais rápidos — **efeito E animação** juntos. |
| `Rapid Care — Use time mult` | float | `0.75` | 0.3..1 | Tempo de uso de itens médicos (0.75 = 25% mais rápido). **Não** vale para o kit de cirurgia (ver Swift Surgeon). |
| `Swift Surgeon — Enabled` | bool | `true` | — | Perk (**072**): cirurgia (CMS/Surv12) muito mais rápida. |
| `Swift Surgeon — Surgery time mult` | float | `0.75` | 0.3..1 | Tempo de cirurgia (0.75 = 25% mais rápido). A skill Surgery do jogador **continua** valendo por cima. |
| `Restorative Surgery — Enabled` | bool | `true` | — | Perk (**076**): a cirurgia restaura o membro a ~80% do HP máx (vanilla: CMS 25–45%, Surv12 60–72%). Vale na auto-cirurgia + aliado via ICM. |
| `Restorative Surgery — Restored max HP` | float | `0.80` | 0..1 | **Piso** da fração de HP máx retida (0.80 = 80%). Nunca pior que o vanilla; a skill Surgery empurra **além** deste piso. |
| `Unskilled — Enabled` | bool | `true` | — | Drawback (**079**, ex-`Shaky Hands`): +recuo por falta de habilidade. Agora Médico **E** Saqueador, e **LIGADO** por padrão (antes off). |
| `Unskilled — Recoil mult` | float | `1.25` | 1..2 | Recuo (1.25 = +25%). |
| `Override color` | bool | `false` | — | Sobrescreve a cor do nome/ícone desta classe pela 'Class color' (desligado = cor do server). (item 067) |
| `Class color` | Color | `#6f9455` | — | Cor do nome/ícone da classe — só vale com 'Override color' ligado; alpha ignorado (sempre opaca). (item 067) |

## Seção `3 · Rifleman`

> `Loud Looter` é bindado depois no código (bloco do Saqueador) → aparece **abaixo** do par de cor.

| Propriedade (EN) | Tipo | Padrão | Faixa | O que faz |
|---|---|---|---|---|
| `Cool Under Fire — Enabled` | bool | `true` | — | Perk: menos flinch ao levar dano. |
| `Cool Under Fire — Flinch mult` | float | `0.5` | 0..1 | Tranco de câmera ao levar dano (0.5 = −50%). |
| `Cool Under Fire — Malfunction chance mult` | float | `0.5` | 0..1 | Chance de travamento da arma (0.5 = −50%, anti-jam). |
| `Adrenaline — Enabled` | bool | `true` | — | Perk: causar/receber dano abre uma janela com recuo/recarga/ADS melhores. |
| `Adrenaline — Window (s)` | float | `25` | 5..120 | Duração da janela (renovável a cada novo dano). |
| `Adrenaline — Cooldown (s)` | float | `120` | 0..600 | Cooldown após a janela antes de reativar. |
| `Adrenaline — Recoil mult` | float | `0.7` | 0.3..1 | Recuo na janela (0.7 = −30%). |
| `Adrenaline — Reload time mult` | float | `0.7` | 0.3..1 | Recarga na janela (0.7 = 30% mais rápido). |
| `Adrenaline — ADS time mult` | float | `0.7` | 0.3..1 | ADS na janela (0.7 = 30% mais rápido). |
| `Loud Operator — Enabled` | bool | `true` | — | Drawback: +raio de audibilidade dos sons de movimento. |
| `Loud Operator — Sound radius mult` | float | `1.3` | 1..2 | Raio de som de movimento (1.3 = +30%). |
| `Override color` | bool | `false` | — | Sobrescreve a cor do nome/ícone desta classe pela 'Class color' (desligado = cor do server). (item 067) |
| `Class color` | Color | `#b0573a` | — | Cor do nome/ícone da classe — só vale com 'Override color' ligado; alpha ignorado (sempre opaca). (item 067) |
| `Loud Looter — Enabled` | bool | `true` | — | Drawback (**079**): som de interação/loot mais **ALTO** (a IA ouve mais; o canal de IA requer SAIN). |
| `Loud Looter — Volume mult` | float | `1.3` | 1..2 | Volume de interação/loot (1.3 = +30%). |

## Seção `4 · Hunter`

> `Light Frame` e `Quick Draw` são **compartilhados** (Caçador+Furtivo e Caçador+Fuzileiro+Furtivo) com **uma config só**, aqui. São bindados depois no código → aparecem **abaixo** do par de cor.

| Propriedade (EN) | Tipo | Padrão | Faixa | O que faz |
|---|---|---|---|---|
| `Stalker — Enabled` | bool | `true` | — | Perk: −raio de audibilidade dos sons de movimento (espreita). Irmão mais fraco do Ghost Step do Furtivo. |
| `Stalker — Sound radius mult` | float | `0.8` | 0.1..1 | Raio de som de movimento (0.80 = **−20%**; o Furtivo tem −30%). |
| `Sharpshooter — Enabled` | bool | `true` | — | Perk: mira (ADS) mais rápido. |
| `Sharpshooter — ADS time mult` | float | `0.85` | 0.5..1 | Tempo de ADS (0.85 = 15% mais rápido). |
| `Iron Lungs — Enabled` | bool | `true` | — | Perk: segura a respiração por mais tempo. |
| `Iron Lungs — Breath drain mult` | float | `0.7` | 0.2..1 | Dreno de O₂ ao prender a respiração (0.7 → +43% de duração). |
| `Steady Arms — Enabled` | bool | `true` | — | Perk: braço cansa mais devagar ao mirar (**requer o stances mod**). |
| `Steady Arms — ADS arm drain mult` | float | `0.65` | 0.2..1 | Dreno de braço em ADS (0.65 = 35% mais lento). |
| `Calm Sights — Enabled` | bool | `true` | — | Perk (**072**): a arma oscila menos. ⚠️ Afeta o sway de **mira/movimento**; o sway da **respiração** é outro sistema (esse é o Iron Lungs). |
| `Calm Sights — Sway mult` | float | `0.7` | 0.3..1 | Oscilação (sway) da arma (0.7 = 30% menos). |
| `Rooted — Enabled` | bool | `true` | — | Drawback: −velocidade enquanto mira. |
| `Rooted — ADS move speed` | float | `0.85` | 0.5..1 | Velocidade ao mirar (0.85 = −15%). |
| `Override color` | bool | `false` | — | Sobrescreve a cor do nome/ícone desta classe pela 'Class color' (desligado = cor do server). (item 067) |
| `Class color` | Color | `#c2973f` | — | Cor do nome/ícone da classe — só vale com 'Override color' ligado; alpha ignorado (sempre opaca). (item 067) |
| `Light Frame — Enabled` | bool | `true` | — | Drawback (**079**, Caçador + Furtivo): limite de carga reduzido (estrutura leve — leva menos loot). |
| `Light Frame — Carry limit penalty` | float | `-0.20` | −0.5..0 | Redução do limite de carga (−0.20 = −20%). Valor **negativo** (teto, não piso). |
| `Quick Draw — Enabled` | bool | `true` | — | Perk (**080/087/088**, Caçador + Fuzileiro + Furtivo): acelera a TROCA para a arma do coldre (guardar a anterior + sacar a do coldre). |
| `Quick Draw — Draw-in time mult (phase 3)` | float | `0.65` | 0.3..1 | Fase 3 — tempo de **SACAR** a arma do coldre (0.65 = 35% mais rápido; 1.0 = desliga). |
| `Quick Draw — Put-away time mult (phase 1)` | float | `0.75` | 0.3..1 | Fase 1 — tempo de **GUARDAR** a arma anterior (0.75 = 25% mais rápido; 1.0 = desliga). |

## Seção `5 · Stealth`

| Propriedade (EN) | Tipo | Padrão | Faixa | O que faz |
|---|---|---|---|---|
| `Execution Speed — Enabled` | bool | `true` | — | Perk: +velocidade com a melee na mão. |
| `Execution Speed — Move speed mult` | float | `1.1` | 1..1.5 | Velocidade com melee na mão (1.1 = +10%). |
| `Execution Melee — Enabled` | bool | `true` | — | Perk: multiplica o dano de golpe de faca. |
| `Execution Melee — Damage mult` | float | `3.5` | 1..10 | Dano de melee (3.5×, execução). Era `5` (one-shot trivial). (balance B7) |
| `Ghost Step — Enabled` | bool | `true` | — | Perk: −raio de audibilidade dos sons de movimento. |
| `Ghost Step — Sound radius mult` | float | `0.7` | 0.1..1 | Raio de som de movimento (0.7 = −30%). |
| `Rattled — Enabled` | bool | `true` | — | Drawback: +tranco de câmera ao levar dano. |
| `Rattled — Aim-punch mult` | float | `1.5` | 1..3 | Tranco ao levar dano (1.5 = +50%). |
| `Silent Knife — Enabled` | bool | `true` | — | Perk (**083**): a faca não faz barulho (sacar, golpear e acertar são silenciosos). |
| `Override color` | bool | `false` | — | Sobrescreve a cor do nome/ícone desta classe pela 'Class color' (desligado = cor do server). (item 067) |
| `Class color` | Color | `#8b8fa3` | — | Cor do nome/ícone da classe — só vale com 'Override color' ligado; alpha ignorado (sempre opaca). (item 067) |

## Seção `6 · Scavenger`

| Propriedade (EN) | Tipo | Padrão | Faixa | O que faz |
|---|---|---|---|---|
| `Quick Hands — Enabled` | bool | `true` | — | Perk: revista **2 contêineres ao mesmo tempo**, desde o início. É o bônus **elite** da skill Search (nível 51) antecipado — não é mecânica nova. Sem efeito duplo se a Search chegar a elite. (item 061) |
| `Silent Looter — Enabled` | bool | `true` | — | Perk: sons de interação/loot mais baixos. |
| `Silent Looter — Volume mult` | float | `0.4` | 0.1..1 | Volume de interação/loot (0.4 = −60%). |
| `Pack Mule — Enabled` | bool | `true` | — | Perk: +limite de carga (piso, não soma com a Strength). |
| `Pack Mule — Carry limit bonus` | float | `0.3` | 0..1 | Bônus de limite de carga (0.3 = +30%). |
| `Hare — Enabled` | bool | `true` | — | Perk (**081**, ex-`Lebre`): +velocidade de movimento enquanto **NÃO** estiver pesado (sem o ícone de sobrepeso/bigorna). |
| `Hare — Move speed mult` | float | `1.3` | 1..1.5 | Velocidade quando leve (1.3 = +30%). Desliga automaticamente ao ficar pesado. |
| `Nervous — Enabled` | bool | `true` | — | Drawback (**082**, ex-`Medroso`): mãos trêmulas (tremor) ao levar tiro **OU** sob supressão (bala passa perto). |
| `Nervous — Tremor duration (s)` | float | `6` | 1..20 | Duração do tremor (segundos). |
| `Nervous — Cooldown (s)` | float | `8` | 0..30 | Espera antes de o tremor poder re-disparar. |
| `Nervous — Suppression distance (m)` | float | `4` | 0..20 | Distância (m) que a bala passa perto p/ contar como supressão (0 = só ao levar tiro). |
| `Override color` | bool | `false` | — | Sobrescreve a cor do nome/ícone desta classe pela 'Class color' (desligado = cor do server). (item 067) |
| `Class color` | Color | `#c4ad45` | — | Cor do nome/ícone da classe — só vale com 'Override color' ligado; alpha ignorado (sempre opaca). (item 067) |

## Seção `7 · Tank`

| Propriedade (EN) | Tipo | Padrão | Faixa | O que faz |
|---|---|---|---|---|
| `Bulwark — Enabled` | bool | `true` | — | Perk: reduz o dano recebido na vida. |
| `Bulwark — Damage taken` | float | `0.85` | 0.5..1 | Dano recebido (0.85 = −15%). |
| `Bulwark — Require heavy armor` | bool | `true` | — | **Couraça CONDICIONAL**: só vale com armadura pesada equipada (antes era incondicional — valia até pelado). (balance B6) |
| `Bulwark — Min armor class` | int | `4` | 1..6 | Classe mínima da armadura equipada para a Couraça valer. (balance B6) |
| `Bunker — Enabled` | bool | `true` | — | Perk: com arma pesada (LMG/HMG/GL), menos recuo e mais ergonomia. |
| `Bunker — Heavy weapon recoil mult` | float | `0.7` | 0.5..1 | Recuo com arma pesada (0.7 = −30%). |
| `Bunker — Heavy weapon ergo mult` | float | `1.15` | 1..1.5 | Ergonomia com arma pesada (1.15 = +15%). |
| `Tireless Arms — Enabled` | bool | `true` | — | Perk: braço cansa muito devagar com arma pesada (**requer o stances mod**). |
| `Tireless Arms — Heavy arm drain mult` | float | `0.5` | 0..1 | Dreno de braço com arma pesada (0.5 = 2× mais lento; 0 = não drena). Era `0.20` → `0.5`. (balance B16) |
| `Heavy Frame — Enabled` | bool | `true` | — | Drawback: −velocidade de movimento (estrutura pesada). |
| `Heavy Frame — Move speed` | float | `0.9` | 0.5..1 | Velocidade (0.9 = −10%). |
| `Heavy Frame — Hunger/thirst drain` | float | `1.15` | 1..2 | Dreno de fome/sede (1.15 = +15% mais rápido). |
| `Pack Mule — Enabled` | bool | `true` | — | Perk: +limite de carga (piso, não soma com a Strength). |
| `Pack Mule — Carry limit bonus` | float | `0.3` | 0..1 | Bônus de limite de carga (0.3 = +30%). |
| `Loud Operator — Enabled` | bool | `true` | — | Drawback: +raio de audibilidade dos sons de movimento. |
| `Loud Operator — Sound radius mult` | float | `1.3` | 1..2 | Raio de som de movimento (1.3 = +30%). |
| `Shotgun Reload — Enabled` | bool | `true` | — | Perk (**084**): recarrega escopetas de tubo (shell-a-shell) mais rápido. Não afeta escopetas com carregador destacável (Saiga). |
| `Shotgun Reload — Reload time mult` | float | `0.6` | 0.4..1 | Tempo de recarga da escopeta (0.6 = 40% mais rápido). |
| `Override color` | bool | `false` | — | Sobrescreve a cor do nome/ícone desta classe pela 'Class color' (desligado = cor do server). (item 067) |
| `Class color` | Color | `#6b7280` | — | Cor do nome/ícone da classe — só vale com 'Override color' ligado; alpha ignorado (sempre opaca). (item 067) |

## Seção `8 · Naked`

> O Peladão (Naked) não tem perks — esta seção existe para a sua cor (item 067) e, mais tarde, o seu texto de mérito (item 068).

| Propriedade (EN) | Tipo | Padrão | Faixa | O que faz |
|---|---|---|---|---|
| `Override color` | bool | `false` | — | Sobrescreve a cor do nome/ícone desta classe pela 'Class color' (desligado = cor do server). (item 067) |
| `Class color` | Color | `#c28a60` | — | Cor do nome/ícone da classe — só vale com 'Override color' ligado; alpha ignorado (sempre opaca). (item 067) |

## Seção `9 · Vanilla Skill Fixes`

> Correções/ativações de mecânicas de skill vanilla que ficam inertes no EFT. Hoje: Weapon Mastery (item 058). As 4 keys agora têm o prefixo `Weapon Mastery — `.

| Propriedade (EN) | Tipo | Padrão | Faixa | O que faz |
|---|---|---|---|---|
| `Weapon Mastery — Enabled` | bool | `true` | — | Ativa as maestrias inertes: XP por disparo do underbarrel + bônus por nível (SMG/LMG/Launcher/Underbarrel). |
| `Weapon Mastery — Underbarrel XP per shot` | float | `0.5` | 0..1 | XP de Underbarrel Launchers por disparo do GP-25/M203 (0.5 = paridade de esforço com SMG). |
| `Weapon Mastery — Recoil bonus per level` | float | `0.004` | 0..0.02 | Redução de recuo por nível da maestria da arma na mão (0.004 = −0.4%/nível). |
| `Weapon Mastery — Ergo bonus per level` | float | `0.002` | 0..0.02 | Aumento de ergonomia por nível da maestria (0.002 = +0.2%/nível). |

---

> Notas (i18n — item 008):
> - Os textos **in-game** (nome da classe, tooltips, botão SKILLS, cards de perk) seguem o **idioma do EFT** (`"po"` = Português → pt; senão inglês). O **F12** é a exceção — não segue (ver nota no topo).
> - Trocar o `.dll` do client exige **reiniciar o jogo** (plugin BepInEx). Trocar um valor **default** no código NÃO altera um `.cfg` já existente — o BepInEx só grava o default quando a entry é criada pela 1ª vez. Renomear uma **key** (ex.: `Shaky Hands` → `Unskilled`) cria uma entry nova com o default → o valor antigo é "perdido" (reseta).

## Histórico

| Data | Alteração |
|---|---|
| 2026-06-07 | Criado (item 008). Documenta `EnableSkillMultipliers`, `ShowMultiplierOnSkills` (itens 005/010) e `Language` (008). |
| 2026-07-02 | Item 059 — seção `Perks — UI` com `Class Tab — X offset`. |
| 2026-07-02 | Item 055 — `Class Detail on Loading Screen`. |
| 2026-07-03 | Item 056 — `Weight Marker — X/Y offset`. |
| 2026-07-03 | Item 055 — `Class Detail — Loading panel scale`. |
| 2026-07-04 | Item 058 — seção `Weapon Mastery`. |
| 2026-07-04 | Item 051 — Steady Arms (Hunter) + Tireless Arms (Tank). |
| 2026-07-10 | Balance B17 — Metabolismo Eficiente (Médico). |
| 2026-07-10 | **Reorg completa do F12**: 9 seções (uma por classe, prefixo numérico EN), Pack Mule/Loud Operator desdobrados por classe, descrições padronizadas bilíngues `PT / EN`, 7 entries órfãs removidas. Doc espelhada em inglês: [PROPERTIES.md](PROPERTIES.md). |
| 2026-07-17 | Item 067 — `Override color` + `Class color` por classe (F12) nas 6 classes; nova seção `8 · Naked` (só cor); Vanilla Skill Fixes renumerado `8`→`9`. |
| 2026-08-01 | **Regeneração a partir do código (itens 079–088 + review de propriedades).** Perks novos: Silent Knife (083), Hare (081), Nervous (082), Light Frame (079), Loud Looter (079), Quick Draw (080/087/088, 2 sliders de fase), Shotgun Reload (084). Removidos: Mobile Surgery, Overladen (→ Hare), Shaky Hands (→ Unskilled). Keys renomeadas nas seções `0`/`1`/`5`/`9`; `Perk Diagnostics overlay` agora é **(Avançado)**; ordem intra-seção agora segue o código (`BindOrdered`). Defaults ajustados: Rapid Care 0.7→0.75, Swift Surgeon 0.5→0.75, Adrenaline reload/ADS 0.8→0.7, Iron Lungs 0.667→0.7, Bunker recuo 0.85→0.7, Tireless Arms 0.20→0.5, Heavy Frame fome/sede 1.3→1.15. |
