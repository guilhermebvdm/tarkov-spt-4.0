# Design das classes

> **Data:** 2026-06-21<br>
> **Status:** 🟢 Vivo<br>
> **Responsáveis:** Guilherme<br>
> **Referências:** [class-matrix.mjs](../scripts/class-matrix.mjs)<br>

---

**Doc único e vivo do design das 6 classes** (consolidou `class-overview.md` + `class-levers.md`, agora arquivados em `.archived/`). Cobre todas as camadas: **perks 🔧 / drawbacks 🔻** (abaixo, por classe), a **matriz 🎯** (skills vanilla boostadas), **loadout 🎒** e **hideout 🏠**. **Decisão (2026-06-21): as signatures são tudo flat** — sem skills custom que escalam; a progressão/divergência vem da matriz. Fontes de verdade dos dados: [`class-matrix.mjs`](../scripts/class-matrix.mjs) (matriz reproduzível) + os `.jsonc` em `modded/Server/config/classes/` (implementação).

## Como funciona

- **🔧 Perk / 🔻 Drawback** = efeito aplicado por **patch Harmony keyed na classe** (`Info.GameVersion`), **configurável no F12**. **Não são skills** (não entram na lista/categoria de skills) — a **aba "Perks/Drawback"** (item 053) os exibe à parte.
  - **Passivo** — sempre ativo enquanto a classe está em uso.
  - **Condicional** — liga sob uma condição (ex.: **Adrenaline** no combate).
  - **(head-start)** — perk que libera *de cara* um efeito que a vanilla só entrega no topo da skill correspondente; a skill vanilla **continua subindo por cima** (via matriz 🎯). Sem redundância.
  - **🔻 Drawback** — penalidade proposital (a "sombra" da força da classe), criando counterplay. Mesmas regras de patch/F12; **1 por classe**.
- **Valores = medianos propostos**, todos ajustáveis no F12. *(Antes eram skills 0→51; viraram flat num ponto médio para acesso imediato — ver Histórico.)*

### Convenção de leitura dos números

- **`×k` = o valor vira `k×` o base.** Sempre digo **sobre o quê** incide + o resultado em linguagem clara entre parênteses.
- **Tempo** (uso de item, ADS, recarga, conserto): `×0.7` = leva 70% do tempo → **mais rápido (melhor)**; `×1.15` = leva 115% → **mais lento (pior)**.
- **Quantidade/intensidade** (dano, ruído, chance, peso): `×0.5` = metade; `×5` = quíntuplo. A frase diz se subir é bom ou ruim.

---

## 🩺 Médico

| Perk / Drawback | Tipo | Efeito | Obs / knob |
|---|---|---|---|
| **Combat Medic** | 🔧 head-start | • **cura/estabilização** (medkit, torniquete, tala, painkiller): tempo de uso `×0.7` → **~30% mais rápido**<br>• **cirurgia** (CMS/Surv12, restaura membro destruído): tempo de uso `×0.5` → **2× mais rápido** *(maior bônus por ser a ação mais lenta do jogo)*<br>• **cirurgia sem travar movimento**: pode **andar durante** a cirurgia (no vanilla o personagem fica imóvel) | FirstAid (cura) + Surgery (CMS/Surv12) — matriz já dá XP extra |
| **Shaky Hands** | 🔻 drawback | • **controle de recuo `×1.25`** → **recuo +25%** *(é suporte, não vence trocação sustentada)* | `RecoilControl` (client) |

---

## 🔫 Fuzileiro

| Perk / Drawback | Tipo | Efeito | Obs / knob |
|---|---|---|---|
| **Cool Under Fire** | 🔧 passivo | • **flinch ao levar dano** (tranco na mira ao ser **atingido**): intensidade `×0.5` → **metade do tranco** *(re-escopo 2026-06-23: o EFT 0.16.9 não tem efeito de supressão/near-miss no cliente; o perk atenua o flinch de hit)*<br>• **travamento da arma** (jam/misfire): chance `×0.5`<br>• **conserto do travamento**: tempo `×0.5` → **2× mais rápido** | flinch = `ForceEffector.AddForce` ✅ (050.2) · jam/conserto = 050.3 |
| **Adrenaline** | 🔧 condicional | • Ao **causar ou receber dano** em combate, liga uma janela de **25s** com:<br>  – recuo `×0.7` → **−30%**<br>  – tempo de recarga `×0.8` → **20% mais rápido**<br>  – tempo de ADS `×0.8` → **20% mais rápido**<br>• **renova** a cada novo evento · **cooldown 2 min** após expirar | duração/magnitudes tunáveis (F12) |
| **Loud Operator** | 🔻 drawback | • **volume de TODOS os sons do player `×1.3`** → **+30% mais alto** *(o assalto é barulhento; é ouvido chegando)* | `SoundVolume` (client) |

---

## 🎯 Caçador

| Perk / Drawback | Tipo | Efeito | Obs / knob |
|---|---|---|---|
| **Sharpshooter** | 🔧 passivo | • **tempo de ADS** (levantar a mira) `×0.85` → **15% mais rápido em TODAS as armas** *(2026-07-01: simplificado — o **saque de pistola ×0.5** e a **penalidade de AR ×1.15** do design original **não foram implementados**; ADS é flat em qualquer arma, sem downside. Decisão do usuário via CR-02-04)* | `AdsSpeedPatch` (client) |
| **Iron Lungs** | 🔧 passivo | • **tempo segurando a respiração** (steady aim) `×1.5` → **+50%**<br>• **sway** (oscilação da mira parado) `×0.7` → **−30%**<br>• ⚠️ **dreno de stamina do braço ao mirar** `×0.65` → **−35% (cansa menos)** | ⚠️ = zona stances (item 051) |
| **Rooted** | 🔻 drawback | • **velocidade de movimento enquanto em ADS `×0.85`** → **−15%** *(enraíza ao mirar; vulnerável a flanco/rush)* | move-while-ADS (client) |

---

## 👻 Furtivo *(Stealth)*

| Perk / Drawback | Tipo | Efeito | Obs / knob |
|---|---|---|---|
| **Ghost Step** | 🔧 head-start | • **volume de TODOS os sons que o player gera `×0.7` → −30%** (passos andando/correndo/na moita, comer, curar, saquear, recarregar) | vanilla CovertMovement só cobre movimento; aqui cobre tudo e é flat. Empilha com a CovertMovement (matriz) |
| **Execution** | 🔧 passivo | • **dano do golpe corpo-a-corpo** (faca/marreta) `×5`<br>• **velocidade de movimento com a melee equipada na mão** `+10%` *(só enquanto a arma branca está na mão)* | (era skill ×1→×20; flat ×5) |
| **Rattled** | 🔻 drawback | • **tranco na mira ao ser alvejado `×1.5`** → **+50%** *(o assassino se desmonta sob fogo aberto — espelho do Cool Under Fire)* | `AimPunchMagnitude` (client) |

---

## 🎒 Saqueador

| Perk / Drawback | Tipo | Efeito | Obs / knob |
|---|---|---|---|
| **Quick Hands** | 🔧 head-start | • ao revistar, **examina 2 itens de uma vez** (em vez de 1) | efeito `SearchDouble` (a vanilla só libera no elite da Search) |
| **Silent Looter** | 🔧 passivo | • **revistar containers/corpos não gera som** (ruído de saque = 0) | |
| **Pack Mule** | 🔧 head-start *(compartilhada c/ Tanque)* | • **+30% no limite de peso** que carrega antes de ficar *overweight* (aguenta mais; **não** deixa os itens mais leves) | = bônus máximo da Strength vanilla (`StrengthBuffLiftWeightInc.Max(0.3)`), liberado de cara |
| **Overladen** | 🔻 drawback | • **inércia de movimento escala com o peso** — mochila cheia deixa o movimento *clunky* (mais lento pra parar / trocar de direção) *(trade direto do "carregar tudo")* | `Inertia` (client) |

---

## 🛡️ Tanque

| Perk / Drawback | Tipo | Efeito | Obs / knob |
|---|---|---|---|
| **Pack Mule** | 🔧 head-start *(compartilhada c/ Saqueador)* | • **+30% no limite de peso** que carrega antes de ficar *overweight* (aguenta mais; **não** deixa os itens mais leves) | = bônus máximo da Strength vanilla (`StrengthBuffLiftWeightInc.Max(0.3)`) |
| **Bulwark** | 🔧 passivo | • **dano recebido na vida** (após a armadura) `×0.85` → **−15%** | (era skill 0→−25%; flat −15%) |
| **Bunker** | 🔧 passivo | • **armas pesadas na mão** (LMG/HMG/lança-granadas/underbarrel): **recuo `×0.85`** + **ergonomia `×1.15`** *(as skills de maestria dessas armas são **inertes** no jogo `[]` — o bônus vem por patch, não por skill)*<br>• **lança-granadas acoplado** (GP-25/M203): montar **não reduz a ergonomia** da arma<br>• ⚠️ **dreno de stamina do braço por segurar arma pesada** `×0` (braço não cansa) | ⚠️ = zona stances (item 051) |
| **Heavy Frame** | 🔻 drawback | • **velocidade de movimento `×0.9`** → **−10%**<br>• **fome/sede `×1.3`** → consome **30% mais rápido** *(o brutamontes gasta mais energia/hidratação)* | `Energy`/`Hydration` drain (client) |

---

## Matriz 🎯 (skills vanilla por classe)

> **Baseline = editor web (2026-06-22)** — re-sincronizado dos `.jsonc` (decisão do usuário: edições do editor = nova baseline; substitui a calibração da Fase 2). Camada **server-side** (criação de profile → restart/perfil novo). **Matriz completa por classe (~20–30 skills cada) vive em [`class-matrix.mjs`](../scripts/class-matrix.mjs)** (`node …/class-matrix.mjs` recalcula) + nos `.jsonc` — não duplicada aqui. Resumo:

| Classe | custo | netMult | flag |
|---|---|---|---|
| 🩺 Médico | 30.95 | +11.31 | 7 skills c/ nível (>6) |
| 🔫 Fuzileiro | 30.51 | +18.28 | — |
| 🎯 Caçador | 31.40 | +14.45 | ok |
| 👻 Furtivo | 29.74 | +12.43 | 8 skills c/ nível (>6) |
| 🎒 Saqueador | 30.45 | +11.65 | 8 skills c/ nível (>6) |
| 🛡️ Tanque | 35.28 | +18.89 | **custo >32** |

> ✅ **Balance aceito como está (decisão do usuário, 2026-06-22):** o netMult mais alto (≈+11 a +19) e os budgets estourados (Tanque custo 35.28>32; >6 skills de nível em alguns) são **intencionais** — o baseline do editor é o final. Os "flags" do `class-matrix.mjs` são informativos, não bloqueiam. *(`Shadowconnections` sem peso → custo/net preliminares, mas sem impacto na decisão.)*

## Loadout 🎒 e Hideout 🏠

> **Sincronizado com os `.jsonc` (2026-06-21).** **Loadout:** as **6 classes já têm kit completo** e **distinto em conteúdo** (`equipped`: armas/armaduras/coletes diferentes + `stash` de 23–70 itens). **Não há item-assinatura único** — todas usam a mesma `baseEdition` ("SPT Zero to hero"), então o contêiner seguro é igual pra todas (não é signature de ninguém). **Hideout:** cada classe tem **1 estação pré-construída**. *(O "2º station a −50% de tempo" do design antigo **não está nos configs** — decidir se ainda se quer.)*

| Classe | Hideout 🏠 (pré-construído) | Loadout 🎒 (kit distinto nos `.jsonc`) |
|---|---|---|
| 🩺 Médico | MedStation | kit médico |
| 🔫 Fuzileiro | Workbench | kit de assalto |
| 🎯 Caçador | Shooting Range | kit sniper/DMR |
| 👻 Furtivo | Lavatory | kit furtivo (pistola/suprimido) |
| 🎒 Saqueador | Scav Case | kit de loot |
| 🛡️ Tanque | Rest Station | kit pesado |

## Pontas soltas / coordenação (rastreadas no [mod-backlog](../backlog/mod-backlog.md))

- **Sync do `SkillWeights.cs`** (item 047) — o `.mjs` tem 3 categorias de gem (ShadowConnections→P, UsecArsystems/BearAksystems→C) que faltam no `.cs` (caem em `UnmappedFallback`). Mudança coordenada de `modded/Server/` (sessão paralela do editor).
- **⚠️ Zona stances** (item 051) — **Iron Lungs** (dreno de braço em ADS) e **Bunker** (stamina arma pesada) caem na zona que o stances mod toca (`GetHandsRestorationFunc`→0, `Priority.Low`, MainPlayer). Coordenar (mesmo repo) ou trocar o lever.
- **Bug do Círculo de Cultistas** (ShadowConnections, Saqueador, item 047) — server não chama `NormalizeToPercentage()` → efeito instantâneo desde nv1. Decisão aceitar vs corrigir.
- **Rename Ghost/Fantasma → Stealth/Furtivo na implementação** — **🔴 pré-requisito do 050.0** (ver Contrato de gating). `fantasma.jsonc`: `name` `Ghost`→`Stealth`, `displayName` {en `Ghost`→`Stealth`, pt `Fantasma`→`Furtivo`}, arquivo → `furtivo.jsonc` (opcional) + `ClassRegistrar`/viewer + **re-sync** + **validação in-game**. Mexe em `modded/Server/` → **coordenar** (sessão paralela). Sem isso, o gating do Furtivo cai na chave errada. *(Perfis "Ghost" órfãos não importam — server não está live.)*

## Pendências de produto (confirmar antes da spec técnica)

> **F ✅ Confirmado (2026-06-21):** Adrenaline liga ao causar/receber dano, **renova** a cada novo evento enquanto ativa, e ao expirar entra em **cooldown de 2 min** (não reativa nesse período).
>
> **I ✅ Resolvido (2026-06-21):** o fome/sede do Tanque é **drawback** = drain `×1.3` (consome 30% mais rápido), movido para o **Heavy Frame**.

| # | Pendência | Proposta |
|---|---|---|
| K ✅ | **Overlaps & stacking** (resolvido) — (a) **Pack Mule +30% × Strength**: Pack Mule é **piso**, garante o +30% máx da Strength desde o início e **não soma** (efetivo = o maior; teto **+30%**, respeita o cap vanilla `StrengthBuffLiftWeightInc.Max(0.3)`). (b) **Bulwark −15% × HeavyVests** e **Execution ×5 × Melee** (matriz): **multiplicam** (intencional, sem cap). | Piso p/ Pack Mule · multiplicativo nos demais. |

> **Sem skills custom:** a infra de skill custom (item **048**) fica **descopada** e o item **049** **funde no 050** (todas as signatures são perks/drawbacks flat). **Nenhuma skill nova** na lista → o `SkillMaster.cs` não precisa de categoria "Special"/"Ability" nova. *(A aba "Perks/Drawback" do item 053 é um display à parte, não uma skill.)*

## Implementação — patch-points + fatiamento (recon 2026-06-21)

**Infra (4 achados do recon):**
1. **O client já existe** — `modded/Client/Plugin.cs` (`[BepInPlugin "customclasses.mdj.client"]`, Awake habilita patches), **ConfigEntry/F12 montado** (sliders `AcceptableValueRange` + hook `SettingChanged`) e Harmony (`OnTriggerPatch` core + patches de UI). **Estende-se, não começa do zero.**
2. **Falta o gating per-classe** — hoje o gating é per-perfil; build no **050.0**: helper "lê `Info.GameVersion` → set da classe + checa player local".
3. **Zona stances confirmada** — o stances **neutraliza** `GClass774.Process/Consume` (Prefix→false) e escreve `hands.Current` via `StaminaController.Tick`. **Arm-stamina NÃO se patcha direto** — compor via `StaminaController.Multipliers`/`ArmStaminaCoordinator` (Iron Lungs braço, Bunker arma-pesada → item 051).
4. **Padrão SE** de leitura (`GameUtils.GetSkillManager()`) existe; como viramos perks flat, usamos **gating por classe + `ConfigEntry`**, não leitura de nível de skill.

**Contrato de gating** (pinado em `CustomClassesMod.cs:77` + `ClassRegistrar.cs:266`):
- `Info.GameVersion` = **nome localizado da edition** = `displayName[language]` (idioma do launcher: `"pt"` → `displayName.pt` · `"en"`/`"name"` → o `name`, que é inglês). **A chave muda com o idioma** → **não hardcodar** a string de um idioma só.
- Gate pela **chave estável = campo `name`** (inglês), mapeando do `GameVersion` via o registro `name`↔`displayName` que o server já mantém (`classVisualRegistry`, `ClassRegistrar.cs:277`) e **reaproveitando** a resolução de classe que o client já faz (rota `/customclasses/skill-multipliers`).

| Classe (doc) | `name` (chave estável) | `displayName.pt` (= GameVersion se lang=`pt`) | `displayName.en` |
|---|---|---|---|
| 🩺 Médico | `Combat Medic` | Médico de Combate | Combat Medic |
| 🔫 Fuzileiro | `Rifleman` | Fuzileiro | Rifleman |
| 🎯 Caçador | `Hunter` | Caçador | Hunter |
| 👻 Furtivo | **`Ghost`** ⚠️ | Fantasma | Ghost |
| 🎒 Saqueador | `Scavenger` | Saqueador | Scavenger |
| 🛡️ Tanque | `Tank` | Tanque | Tank |

> ⚠️ **O Furtivo ainda é `Ghost`/"Fantasma" no runtime** — o rename só ocorreu nos docs. **Pré-requisito do 050.0:** fazer o **rename de implementação antes** (`name` `Ghost`→`Stealth`, `displayName`→{en `Stealth`, pt `Furtivo`}), senão Ghost Step/Execution/Rattled gateiam na chave errada. *(As seções `[Médico]`/`[Furtivo]`/… na árvore F12 abaixo são rótulos de display; o gating real usa a chave estável.)*

**Patch-points por efeito** (C/S = client/server; conf. ✅ confirmado · 🟡 verificar · ⚠️ zona stances):

| Efeito | Alvo do patch | C/S | Conf. |
|---|---|---|---|
| **Bulwark** −15% dano | `Player.ApplyDamageInfo`→`ActiveHealthController.ApplyDamage` (mult. `damage`) | client | ✅ |
| **Pack Mule** +30% carga | `SkillManager.CarryingWeightRelativeModifier` / `StrengthBuffLiftWeightInc` (piso) | client | ✅ |
| **Quick Hands** 2-itens | `SkillManager.IsSearchDouble` (forçar `true`) | client | ✅ |
| **Execution** vel · **Rooted** · **Heavy Frame** vel | `MovementContext.MaxSpeed`/`SprintSpeed` (postfix-mult; **compõe** c/ stances) | client | ✅ *(Rooted: condicionar a "mirando")* |
| **Overladen** inércia | `BasePhysicalClass.OnWeightUpdated`→`Inertia` (postfix-mult; compõe) | client | ✅ |
| **Shaky Hands** · **Adrenaline** recuo | `WeaponRecoil.CalculateRecoil` / buff `RecoilControlImprove` | client | ✅ |
| **Iron Lungs** respiração | `GetOxygenCapacityFunc` + delta hold-breath | client | ✅ |
| **Adrenaline** janela/cooldown | state-machine própria (25s · cd 120s) ✅ · gatilho "receber dano" = `ApplyDamageInfo` ✅ · gatilho "**causar dano**" (acertar bot) = hook próprio 🟡 | client | ✅/🟡 |
| **Rattled** · **Cool Under Fire** supressão | `ForceEffector.AddForce`/`WiggleMagnitude` | client | 🟡 |
| **Iron Lungs** braço-ADS · **Bunker** arma-pesada | `StaminaController.Multipliers`/`ArmStaminaCoordinator` (**não** patchar `GClass774`) | client | ⚠️ |
| **Heavy Frame** fome/sede | tick de Energy/Hydration (método obfuscado) | client? | 🟡 |
| **Execution** melee ×5 | dano de arma branca (fora do decompile curado) | client | 🟡 |
| **Combat Medic** cura/cirurgia/sem-lock | uso de item médico (`GClass491`…) + flag de lock | client | 🟡 |
| **Cool Under Fire** travamento/conserto | `config.Malfunction` / roll | client | 🟡 |
| **Sharpshooter** ADS-por-arma / saque | `_props.Ergonomics`×`config.Aiming` / draw | client | 🟡 |
| **Ghost Step / Loud Operator / Silent Looter** som | passo (`MovementContext`) **+** sons de ação | client | 🟡 |
| **Bunker** GL + armas pesadas (LMG/HMG/GL/underbarrel) | ergo (`_props.Ergonomics`) + recuo (`WeaponRecoil`) condicionados ao `weapClass` da arma na mão + classe Tank. *(Maestrias LMG/HMG/Launcher/AttachedLauncher são inertes `[]` → bônus por patch.)* | client | 🟡 |

> **9 ✅ · ~7 🟡 · 2 ⚠️.** Quase tudo **client-side** → o **F12-live é viável** (ver seção F12).
>
> ⚠️ **Confiança = estimativa do recon (sub-agents).** Nomes `GClass*`/método são **version-specific** e **não** foram re-verificados no assembly carregado — **cada fatia re-confirma o alvo just-in-time** antes de patchar (decompilar de `D:/SPT` se preciso). Um `✅` aqui = "candidato forte", não "pinado".

**Em aberto — resolver na spec da fatia (🟡):**
- **"Todos os sons"** (Ghost Step/Loud/Silent) **não é 1 knob:** passos via `MovementContext` + sons de ação (cura/loot/reload) em pontos próprios → **multi-hook**.
- **Aim-punch** (Cool Under Fire/Rattled): **distinguir** o **tranco-de-hit** do **wiggle-de-supressão** — definir em qual(is) o multiplicador incide.
- **Energy/Hydration drain** (Heavy Frame): **confirmar que roda client-side na raid** (recon divergiu) — se for server, quebra o F12-live desse lever.
- **Gatilho "causar dano" da Adrenaline** — acertar um inimigo precisa de hook no dano aplicado ao **bot** (attacker = player local); o gatilho "receber dano" já é o `ApplyDamageInfo` do Bulwark.
- **Melee-dano / GL-ergo / uso-de-medkit / lock-de-cirurgia:** métodos fora do decompile curado → confirmar *just-in-time* (decompilar de `D:/SPT` se preciso).

**Fatiamento do 050** (por confiança/sistema; cada fatia = 1 ciclo SDD):
- **050.0 — Infra + 2 provas** *(✅)*: gating per-classe + framework F12-live + **Bulwark** (dano) + **Pack Mule** (carga) → valida ponta-a-ponta in-game.
- **050.1 — Movimento/inércia** *(✅)*: Execution vel · Rooted · Heavy Frame vel · Overladen.
- **050.2 — Recuo/aim-punch** *(✅/🟡)*: Shaky Hands · Adrenaline recuo · Cool Under Fire supressão · Rattled.
- **050.3 — Combate/saúde** *(🟡)*: Execution melee · Heavy Frame fome/sede · Combat Medic · malfunction · máquina-de-estado da Adrenaline.
- **050.4 — Som/arma/inventário** *(🟡+✅)*: Ghost Step/Loud/Silent · Sharpshooter · Bunker GL · Quick Hands · Iron Lungs respiração/sway.
- **051 — Zona stances**: Iron Lungs braço · Bunker arma-pesada (via `ArmStaminaCoordinator`).

> **DoD é por efeito, não por perk:** o fatiamento é por *sistema*, então perks divididos (Cool Under Fire, Iron Lungs, Adrenaline, Execution) só ficam **100% após a última fatia** que os toca. **Aceite por efeito = observação direta in-game** — ex.: Bulwark → hit conhecido perde −15% de HP; Loud Operator → bot detecta de mais longe; Pack Mule → +30% no limite de peso; Iron Lungs → segura o ar 1.5×. Cada **spec de fatia (SDD)** materializa o aceite testável do(s) seu(s) efeito(s); o **052** agrega a validação final das 6 classes.

## Configuração no F12 (BepInEx ConfigurationManager)

**É possível?** **Sim.** Os perks/drawbacks são **patches do CLIENTE** (recuo, ADS, dano, ruído, inércia, cura, peso, busca — simulados no cliente durante a raid; **exceção a confirmar:** o tick de fome/sede do Heavy Frame — se rodar server-side, vira restart, não F12-live), então o **F12 (`ConfigEntry` do BepInEx, client-side) é o lugar nativo** deles. O que é **server-side** é só a **matriz** (🎯 `skills`/`skillMultipliers` — seção **Matriz 🎯** abaixo) e a **definição da classe** (quais perks cada classe tem + gating), aplicadas na criação do profile — exige restart/profile novo, **não** estes perks. O gating (qual classe o player é) vem do profile (`Info.GameVersion`), setado no server mas **lido no cliente** na raid → o patch sabe a classe e aplica o valor do F12.

> ⚠️ **Validar na implementação — precisa de restart? (por entry):**
> - Patches **contínuos** que leem `ConfigEntry.Value` **no momento de aplicar** (recuo/ADS/dano/ruído/inércia/cura/busca/peso) → **ao vivo, sem restart**.
> - Efeitos aplicados **uma vez** (ex.: limite de carga setado no load da raid; inscrição em evento) → re-aplicar no `SettingChanged` (ou restart). Marcar caso a caso na spec técnica.

> 🔴 **Requisito (DoD):** todo valor que **pode** mudar ao vivo **deve** ser lido no *apply-time* (ou re-aplicado via `SettingChanged`) — **nada de cachear no boot/load**. Assim, mudanças no F12 **valem durante a raid** e os eventos/efeitos (incl. a janela e o cooldown da Adrenaline) passam a usar o novo valor já no próximo disparo.

Cada perk/drawback tem um **toggle `Enabled`** + os valores tunáveis. Layout proposto (seção por classe; 🔻 = drawback). *(Espelha os valores das tabelas por classe acima — serve de referência direta pras chaves de `ConfigEntry`; a redundância é proposital.)*

```
[Pack Mule]  (compartilhada Saqueador + Tanque — chave única)
  Enabled                         bool   true
  Carry limit bonus               float  0.30    # +30% no limite de peso

[Médico]
  Combat Medic — Enabled          bool   true
  Combat Medic — Heal use time    float  0.70    # ×tempo medkit/torniquete/tala/painkiller
  Combat Medic — Surgery time     float  0.50    # ×tempo CMS/Surv12
  Combat Medic — Surgery moving   bool   true    # cirurgia andando
  🔻 Shaky Hands — Enabled        bool   true
  🔻 Shaky Hands — Recoil control float  1.25    # ×recuo (+25%)

[Fuzileiro]
  Cool Under Fire — Enabled       bool   true
  Cool Under Fire — Suppression   float  0.50    # ×intensidade do tranco
  Cool Under Fire — Malf. chance  float  0.50    # ×chance de travamento
  Cool Under Fire — Fix time      float  0.50    # ×tempo de conserto
  Adrenaline — Enabled            bool   true
  Adrenaline — Window seconds     float  25
  Adrenaline — Recoil             float  0.70
  Adrenaline — Reload time        float  0.80
  Adrenaline — ADS time           float  0.80
  Adrenaline — Cooldown seconds   float  120
  Adrenaline — Trigger deal dmg   bool   true
  Adrenaline — Trigger take dmg   bool   true
  🔻 Loud Operator — Enabled      bool   true
  🔻 Loud Operator — Sound volume float  1.30    # ×volume de todos os sons do player

[Caçador]
  Sharpshooter — Enabled          bool   true
  Sharpshooter — Pistol draw time float  0.50
  Sharpshooter — ADS sniper/DMR   float  0.85
  Sharpshooter — ADS assault rifle float 1.15
  Iron Lungs — Enabled            bool   true
  Iron Lungs — Breath hold        float  1.50    # ×tempo segurando o ar
  Iron Lungs — Sway               float  0.70
  Iron Lungs — ADS arm stamina    float  0.65    # ×dreno do braço em ADS
  🔻 Rooted — Enabled             bool   true
  🔻 Rooted — ADS move speed      float  0.85    # ×velocidade enquanto em ADS

[Furtivo]
  Ghost Step — Enabled            bool   true
  Ghost Step — All noise          float  0.70    # ×volume de todos os sons do player
  Execution — Enabled             bool   true
  Execution — Melee damage        float  5.00
  Execution — Move speed w/ melee float  1.10
  🔻 Rattled — Enabled            bool   true
  🔻 Rattled — Aim punch when hit float  1.50    # ×tranco na mira ao ser alvejado

[Saqueador]
  Quick Hands — Enabled           bool   true
  Quick Hands — Double search     bool   true    # revista 2 itens de uma vez
  Silent Looter — Enabled         bool   true
  Silent Looter — Loot noise      float  0.00    # ×ruído de saque
  🔻 Overladen — Enabled          bool   true
  🔻 Overladen — Inertia by weight float 1.50    # inércia máx (no limite de carga); escala com o peso

[Tanque]
  Bulwark — Enabled               bool   true
  Bulwark — Damage taken          float  0.85    # ×dano recebido (−15%)
  Bunker — Enabled                bool   true
  Bunker — GL no ergo penalty     bool   true
  Bunker — Heavy wpn arm stamina  float  0.00    # ×dreno do braço c/ arma pesada
  Bunker — Heavy wpn recoil       float  0.85    # ×recuo c/ LMG/HMG/GL/underbarrel (maestrias inertes → patch)
  Bunker — Heavy wpn ergo         float  1.15    # ×ergonomia c/ armas pesadas
  🔻 Heavy Frame — Enabled        bool   true
  🔻 Heavy Frame — Move speed     float  0.90    # ×velocidade (−10%)
  🔻 Heavy Frame — Hunger/thirst  float  1.30    # ×taxa de fome/sede (+30%)
```

> Valores = medianos atuais (ver tabelas acima). Drawbacks finalizados em 2026-06-21.

## UI — aba "Perks/Drawback" na tela de Skills (escopo)

Adicionar uma **aba nova "Perks/Drawback"** na tela de **SKILLS** (ao lado das categorias de skill) que lista, **da classe ativa do player**, todos os seus **perks 🔧 e drawbacks 🔻** (os desta página).

- **Conteúdo:** por classe ativa, cada perk/drawback com nome + efeito + (head-start/condicional/passivo) + valor atual.
- **Bilíngue:** nome e efeito em **inglês** (canônico) **com tradução pt-br** — mostrar conforme o idioma do cliente (ou ambos).
- **UI fundada no design system do jogo:** reusar componentes/estilo da tela de Skills do EFT (fontes, cores, bordas, tooltips, ícones), como os `Skills*Patch` de identidade já fazem — **não** inventar visual do zero.
- **Client-side:** Harmony na tela de Skills, no mesmo padrão dos patches existentes (`SkillsNavButtonPatch`, `SkillsScreenIdentityPatch`). Fonte do conteúdo = esta página.
- **Pendente:** o **dicionário de strings EN/pt-br** por perk/drawback **ainda não existe** (deriva das tabelas por classe acima, que hoje têm nome em EN + efeito em pt) — precisa ser materializado pra esta aba (escopo do 053).
- **Backlog:** item próprio de UI (**053**), **depois do 050** (precisa dos perks existirem primeiro).

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-06-21 | Guilherme | Criação. Tabela das skills custom (🧪, máx 51) + perks (🔧) por classe. Máx corrigido 10→51; revelar ₽ removido do Saqueador. |
| 2026-06-21 | Guilherme | Modelo per-evento; Adrenaline ativa por causar dano; Fôlego ×2 + sway + dreno de braço. |
| 2026-06-21 | Guilherme | **Nomes EN** + 4 skills→perks (Mãos Rápidas/Passo Fantasma/Mula/Médico de Combate = head-start; redundantes com vanilla, confirmado no `SkillManager.cs`). |
| 2026-06-21 | Guilherme | Reorganizado por classe; review anti-ambiguidade (convenção de leitura, cada efeito detalhado); achados: magnitude da Adrenaline (faltava) e fome/sede do Bunker (contraditório). |
| 2026-06-21 | Guilherme | **Decisão: 100% perks.** As 4 skills restantes (Adrenaline, Iron Lungs, Bulwark, Execution) viram **perks flat** com valores medianos. **048 descopado · 049 funde no 050.** |
| 2026-07-01 | Guilherme | **Sharpshooter simplificado** (CR-02-04, review de negócio 053): impl = ADS ×0.85 flat em todas as armas; saque de pistola ×0.5 e penalidade de AR ×1.15 **não implementados** (decisão: manter simples). Design alinhado à realidade. |
| 2026-06-21 | Guilherme | **F ✅ confirmado.** Seção **Configuração no F12** (perks/drawbacks client-side; matriz server-side; nota de restart + requisito DoD de leitura ao vivo). |
| 2026-06-21 | Guilherme | **Drawbacks definidos (1 por classe):** Médico=Shaky Hands (recuo ×1.25) · Fuzileiro=Loud Operator (som ×1.3) · Caçador=Rooted (vel. ADS ×0.85) · Fantasma=Rattled (aim-punch ×1.5) · Saqueador=Overladen (inércia por peso) · Tanque=Heavy Frame (vel. ×0.9 + fome/sede ×1.3). **I ✅ resolvido.** |
| 2026-06-21 | Guilherme | **Classe Fantasma → Furtivo (Stealth).** Doc renomeado `class-custom-perks.md` → **`class-design.md`** e promovido a doc único: **consolidou** `class-overview.md` + `class-levers.md` (arquivados em `.archived/` com lápide) — absorveu **Matriz 🎯**, **Loadout/Hideout** e **Pontas soltas**. Rename na implementação (`.jsonc`/server) fica pendente (coordenar). |
| 2026-06-21 | Guilherme | **Sync com os `.jsonc`:** matriz conferida 1:1 (bate); Saqueador en `Looter`→`Scavenger`; loadout/hideout corrigidos (6 classes já têm kit completo; 1 estação pré-construída cada; sem 2º station a −50%). **Loadout assinatura do Furtivo/Tanque removido** ("placas laterais"/pendente — não vamos ter). **Escopo novo:** aba **"Perks/Drawback"** na tela de Skills (bilíngue, design system do jogo → item 053). |
| 2026-06-21 | Guilherme | **Review 1 endereçado (g-review-content):** seção **Implementação — patch-points + fatiamento** (mapa do recon: 9✅/7🟡/2⚠️ + 050.0–050.4); contradições "tela de Skills" reescritas; `[Fantasma]`→`[Furtivo]` no F12; Saqueador "contêiner 6 slots" corrigido (todas usam mesma `baseEdition`); **K resolvido** (Pack Mule = piso, sem somar c/ Strength); Quick Hands (`SearchDouble`) reescrito. |
| 2026-06-21 | Guilherme | **Review 2 endereçado:** Adrenaline confiança split (gatilho "causar dano" 🟡); ressalva F12 (Heavy Frame fome/sede client a confirmar); caveat de confiança do recon (re-confirmar alvo por fatia); **DoD por efeito** + aceite por efeito no fatiamento; 053 ganha pendência do dicionário de strings EN/pt-br. |
| 2026-06-21 | Guilherme | **Review 3 endereçado:** **Contrato de gating** pinado (`GameVersion`=`displayName[lang]`; chave estável=`name`; tabela por classe) + **Furtivo=`Ghost` no runtime** → rename de implementação vira **pré-requisito do 050.0**. Doc fechado pra `/g-autodev`. |
| 2026-06-22 | Guilherme | **Matriz re-sincronizada ao editor (novo baseline).** `sync-classes` (install→repo) + rename 054 re-aplicado (`furtivo.jsonc`) + `class-matrix.mjs` reescrito aos valores do editor. netMult subiu p/ ~+11–19 (vs +6/+4) e budgets estouraram (Tanque custo 35.28>32; Médico/Furtivo/Saqueador >6 skills c/ nível) → **pendência de balance**. `Shadowconnections` sem peso (preliminar). 050.0 (Bulwark/Pack Mule) compila ✅, pendente in-game. |
