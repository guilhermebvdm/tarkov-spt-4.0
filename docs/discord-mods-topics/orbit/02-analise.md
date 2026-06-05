---
title: "ORBIT — Análise do thread no Discord (mod de IA para SPT 4.0)"
date: 2026-06-04
status: 🔵 Em andamento
authors: Guilherme
---

# ORBIT — Análise do thread no Discord

> Análise do thread **"ORBIT"** no canal **#mods-development** do Discord da comunidade SPT (27/05 → 04/06/2026, **699 mensagens**). Baseada na [transcrição completa](./01-transcricao.md), nas [imagens/logs anexados](./assets/) e em fontes públicas ([Forge](https://forge.sp-tarkov.com/mod/2706/orbit), [GitHub](https://github.com/Chazut/ORBIT)).
>
> Objetivo: permitir **entrar na discussão já entendendo tudo** o que foi conversado. Afirmações marcadas com _(inferência)_ não são literais do chat.

---

## 0. TL;DR (resumo executivo)

- **ORBIT** (*Objective-driven Raid Bot Intelligence Tactics*) é um **overhaul de IA de bots** para SPT 4.0, criado pelo dev **Chazut**. Não mexe em **como** os bots atiram (isso é o **SAIN**); muda **o que eles fazem fora de combate**: squads de PMC/PlayerScav recebem **objetivos** (caçar PvP, lootear zona valiosa, fazer quest) e **extraem** quando terminam — "como um player de verdade".
- É construído **sobre o Phobos** (com **permissão** do autor, Janky) e adiciona uma **camada de looting própria** + sistema de **objetivos por squad** + **lógica de extract**.
- A grande novela do thread: Chazut **publicou o mod antes de pedir permissão** para usar código do **LootingBots (LB)**, **removeu voluntariamente** da Hub no mesmo dia, **reescreveu o looting do zero** e **relançou** a v1.0.0 em **04/06/2026**.
- Estado atual (04/06): **lançado e estável** (~795 downloads), **MVP do looting próprio pronto**, em **fase de testes pesados**, com roadmap extenso.

---

## 1. O que é o ORBIT

| Campo | Valor |
|---|---|
| **Nome** | ORBIT — *Objective-driven Raid Bot Intelligence Tactics* |
| **Autor** | **Chazut** (projeto **solo**; 10+ anos de dev profissional, por declaração própria) |
| **Versão** | 1.0.0 |
| **SPT alvo** | 4.0.13 |
| **Licença** | MIT |
| **Base** | **Phobos** (advection field + cell dispatch + squad movement) — usado **com permissão** |
| **Categoria** | Bots / AI overhaul |
| **Repo** | <https://github.com/Chazut/ORBIT> (33 commits, branches `main`/`dev`) |
| **GUID** | `com.chazut.orbit` |

**A frase-síntese do próprio Chazut** (transcrição, 01/06): *"ORBIT is an AI overhaul, it doesn't change how bots fight (SAIN handles that), it changes **what bots do when they're not fighting**."* E o slogan da página: *"Bots with a mission. They loot, they fight, they extract — like real players, not wandering AI."*

---

## 2. Quem é quem na conversa — e o que cada integrante faz

> **Importante:** ORBIT é um **projeto solo do Chazut**. Não existe uma "equipe de desenvolvimento". Os demais participantes são **testers, consultores técnicos e a comunidade** girando em torno do lançamento. Abaixo, os papéis efetivos observados no thread.

| Pessoa | Papel no thread | O que faz / contribuiu |
|---|---|---|
| **Chazut** | **Autor/dev** (180 msgs, o mais ativo) | Cria e mantém o ORBIT; responde dúvidas, coleta bug reports, anuncia decisões, explica arquitetura, conduz a resolução das permissões. |
| **Shynd** | **"Suporte não-oficial" / consultor técnico** (42 msgs) | Troubleshooting (crashes de memória, pagefile), evangeliza boas práticas da comunidade, e dá ideias de design fortes — destaque para o esquema **arma → arquétipo de comportamento** (CQB vs longo alcance) e o atalho via **MOA** da arma. Alerta o Chazut sobre edge-cases de **Fika/multi-client**. |
| **Archangelway** | **Mantenedor atual do LootingBots** | Dá o **"veredito"** sobre o uso do código do LB no ORBIT (ver §11). Posição: ok republicar dado que já há builds soltos, mas quer o **código do LB totalmente removido até o SPT 4.1**, senão pediria unpublish. |
| **Cosmin** | **Playtester intensivo** (121 msgs, 2º mais ativo) | Testa exaustivamente (caçada de cultists, FPS, comportamento em Customs/Dorms), sugere features (cultists estáticos, roamer "gank", snipers em janelas), reporta sensações de jogo. |
| **Klinical** | **Tester headless + gerador de ideias** | Roda setups pesados/headless; reportou o **spam de log "RequestNear bee-lining to own-kill corpse"**; propôs ideias de **looting com personalidade** (Timmy distraído, Chad agarra corpo e disengage, prone ao lootear). |
| **Fums** | **Comunidade / IT** | Testa, ajuda em troubleshooting de hardware (RAM, pagefile, headless), contextualiza o histórico (mod "TAB"). |
| **Baconism** | **Tester** | Feedback "Phobos raids com mochilas cheias do LootingBots"; compartilha config do **SAIN AILimit**; contexto histórico (TAB = *ThePasch's Autonomous Bots*). |
| **Ika** | **Tester veterano** (desde SPT 3.8) | Elogios fortes ("melhor IA de PvE que experimentei") + nitpicks (bots estáticos no regroup são fáceis de matar). |
| **die mitze / Recker / Scootis_McPootis / S41elite / MauwMa / Bloody / ValFin / Lega / SligarTheTiger / Bourne** | **Testers / bug reporters** | Reportam exfils travados, transit camping, minefield, freeze com ABPS, crash de memória (Bloody), drops de FPS, dúvidas de setup. |
| **LifeBosses (Серёга)** | **Tester / figura da comunidade** | Reportou o bug das **portas sempre fechadas / "phantom walk"**; entusiasta do retorno ao Tarkov. |
| **DeW4VE** | **Usuário de Fika Headless** | Feedback de performance: ORBIT roda **melhor que Questing+Looting Bots** no headless. |
| **Trinagan's Alt** | **Apoio de suporte** | Instrui como coletar o `Player.log` para debugar o crash do Bloody. |
| **Pandito / ArchaicFink / Slum_K1ng / Vendth / Zybergeris / RetroLogic / TheSunGod / Kobe Thuy / harmony 👾** | **Comunidade / debate** | Discutem licenciamento/permissões, etiqueta da comunidade, hype, sugestões pontuais (ex.: Kobe sugere referenciar o config do Progressive Bots para CQB/range). |

---

## 3. Em que etapa está o mod (linha do tempo)

| Data (GMT-3) | Evento |
|---|---|
| **27/05** | **Primeiro lançamento público** na Hub/Forge. Chazut anuncia no thread. |
| **27/05 (mesmo dia)** | **Removido voluntariamente** da Hub — ele publicou **antes** de pedir permissão formal aos autores das bases (Phobos e **LootingBots**). |
| **28/05 → 02/06** | **Dev continua** (só o *release* na Hub está pausado). Correções e features pequenas (toggle de cultists vanilla, chain-loot no mesmo andar, filtro de exfil). Quem quer testar **compila do GitHub**. |
| **02–03/06** | Chazut conclui a **remoção total do código do LootingBots** e reescreve o looting **do zero** sobre APIs vanilla da BSG ("Plan B" para voltar à Hub). |
| **03/06** | **Archangelway** (mantenedor do LB) dá o veredito; ambos ficam **alinhados**. |
| **04/06** | **Relançado na Forge** como **v1.0.0** (SPT 4.0.13). Surge um **bug de version-constraint do SAIN** na Forge (ver §4). ~795 downloads ao fim do dia. |

**Estado em 04/06:** **publicado e jogável**, **MVP do looting próprio concluído**, **fase de testes pesados** ("then a few new looting features on top, and ORBIT will be officially back"), com bugs conhecidos sendo tratados e roadmap longo (§16).

---

## 4. Como o ORBIT interage com o SAIN

Essa é a relação central do mod. Resumo:

- **Divisão de responsabilidades:** **SAIN = combate** (visão, audição, mira, cover, granadas). **ORBIT = fora de combate** (objetivos, deslocamento, looting, extract). Citação direta de Chazut: *"ORBIT explicitly hands the bot back to SAIN for combat (15s grace after combat ends before ORBIT picks it up again)."* No instante em que o bot detecta passos/tiro/inimigo, **ORBIT perde o controle** até o SAIN soltar.
- **Sistema de personalidade:** ORBIT **se pluga nos arquétipos do SAIN** (Rat, Chad, GigaChad, etc.) e os mapeia em comportamento (um **Rat** loota e extrai cedo; um **Chad** caça PvP e fica mais tempo). O arquétipo é **travado no spawn**, a partir da personalidade que o SAIN sorteou.
- **Dependência rígida:** SAIN é **hard dependency no nível do BepInEx** — *"if SAIN isn't installed, ORBIT just won't load (BepInEx skips it cleanly)"*. Sem SAIN, o ORBIT some silenciosamente (sem crash), nem aparece no F12.
- **Desabilitar a camada de extract do SAIN:** passo de instalação **obrigatório** (aba INSTALLATION, [att-26](./assets/att-26-2026-06-04-Lega.png)) — editar `BepInEx/plugins/SAIN/Presets/<seu_preset>/GlobalSettings.json` e setar:
  ```json
  "Extract": { "SAIN_EXTRACT_TOGGLE": false }
  ```
  Motivo: a lógica de extract do SAIN **brigaria** com a do ORBIT.
- **Sliders de combate do SAIN não precisam mudar:** Chazut confirma — *"no recommended SAIN combat sliders, ORBIT only handles objectives, dispatch and looting, all the hearing/vision/aim is 100% SAIN's domain."* Pode ajustar visão/audição à vontade que **não conflita**.
- **Bug de publicação na Forge (04/06):** a Forge **não aceitava declarar o SAIN como dependência** — o version-constraint (`~4.4.3`, `~4.4`, `1.0.0`, `*`) retornava *"No matching versions found"* ([att-19](./assets/att-19-2026-06-04-Chazut.png), [att-20](./assets/att-20-2026-06-04-Chazut.png), [att-21](./assets/att-21-2026-06-04-Archangelway.png)). Causa provável _(inferência, levantada por Archangelway)_: SAIN é antigo, anterior à Forge, e tem "muitas versões escondidas" bagunçando o constraint. Workaround do Chazut: deixou **BigBrain** e **Waypoints** como dependências visíveis (que **já são dependências do SAIN**) e o SAIN fica garantido pelo hard-require do BepInEx.

---

## 5. Existe um preset ideal de SAIN?

**Sim** — Chazut publica um **preset recomendado de personalidades** (aba *"Personalities (Recommended SAIN Config)"*, [att-13](./assets/att-13-2026-06-03-Lemoireal.png) / [att-14](./assets/att-14-2026-06-03-Chazut.png)). No F12 do SAIN, em **Personality → Assignment**:

| Personalidade | Chance |
|---|---|
| Rat | 10 |
| Wreckless | 5 |
| SnappingTurtle | 5 |
| Coward | 5 |
| Chad | 5 |
| Timmy | 3 |
| GigaChad | 3 |

- Marcar **"Can be randomly assigned" = True** em cada uma.
- **Normal** é o default — **não precisa setar** (Lemoireal perguntou; Chazut confirmou que qualquer bot que não sorteie uma das listadas vira Normal).
- Resultado: *"roughly a third of your PMCs interesting personalities"* — a distribuição em que o ORBIT foi balanceado.

**Outros ajustes recomendados de SAIN (não de combate):**
1. **Desabilitar o extract do SAIN** (`SAIN_EXTRACT_TOGGLE: false`) — ver §4.
2. **Não mexer** em sliders de visão/audição por causa do ORBIT — eles são domínio do SAIN e não conflitam.

**Dicas de "tunagem por estilo"** (do chat):
- Quer **mais PvP/ação**? Suba o **Kills %**, e/ou restrinja as personalidades a **Wreckless / Chad / GigaChad** (são as que caçam kills). Custo: perde a diversidade (sem Rats espreitando).
- Quer **mais looting silencioso**? Mais **Rats**.

> ⚠️ **Observação (discrepância de fonte):** a descrição da Forge fala em *"five SAIN personality archetypes"*, mas o preset recomendado lista **7** personalidades. O resumo do README no GitHub lista **5** (Rat/Chad/Coward/GigaChad/Timmy). Provavelmente "5 arquétipos com tuning distinto" se refere ao núcleo, com o config recomendado incluindo extras. _(inferência)_

---

## 6. Arquitetura e dependências (como tudo se encaixa)

**Pilha de mods (load order observada nos logs [att-27](./assets/att-27-2026-06-04-Bloody.log) / [att-30](./assets/att-30-2026-06-04-Shynd.log)):**

```
BigBrain 1.4.0  →  SAIN 4.4.3  →  ORBIT 1.0.0  →  Waypoints 1.8.2
(DrakiaXYZ)        (Solarint…)     (Chazut)         (DrakiaXYZ)
```

| Camada | Responsabilidade |
|---|---|
| **Phobos** (embutido, base) | *Advection field* + *cell dispatch* — espalha bots organicamente pelo mapa; movimento de squad. ORBIT herda o **núcleo de dispatch** do Phobos. |
| **BigBrain** | Framework de camadas de cérebro/decisão (o `BrainManager` que o ORBIT "fia" — log: *"ORBIT 1.0.0 fully loaded — BrainManager wired"*). |
| **SAIN** | Combate + sistema de personalidades. |
| **Waypoints – Expanded Navmesh** | Navmesh estendido (pathfinding). |
| **ORBIT** | **Camada de ação reescrita**: objetivos por squad, looting próprio, extract. |

**Detalhes técnicos colhidos dos logs:**
- Patches Harmony do ORBIT: `OrbitInitPatch`, `OrbitTickPatch`, `OrbitDisposePatch`.
- `LootConfig.Init: DONE — looting=Scav, Pmc, PlayerScav, detectDist=80m` → o looting cobre **Scav, PMC e PlayerScav**, com distância de detecção de **80 m**.
- **Shims de compatibilidade por GUID:** o ORBIT detecta plugins de facção e fica "inert" se ausentes — ex.: `UNTAR`, `RUAF`, `BlackDivision` (e suporte planejado a **ISB**). Isso permite o toggle "manter o cérebro do mod de facção" vs "fazer roam com ORBIT".
- **Roda sob Fika** (os logs mostram `FikaModHandler` carregando os plugins) → **compatível com multiplayer/headless** (confirmado também por DeW4VE e Klinical rodando headless).

---

## 7. O sistema de looting (o grande diferencial)

Reescrito **do zero** sobre APIs vanilla da BSG (depois da remoção do LB). Características:

- **Thresholds de valor por personalidade** (README): **Chad ~15–20k**/slot, **médio ~10k**, **Rat ~5k**, **Scav ~30% de chance aleatória**. → *"a Chad walks past trash while a Rat grabs everything."*
- **Dirigido por POI + dispatch de squad** (determinístico), **não** por visão por-bot como o LB. Chazut: *"it's deterministic, driven by world-indexed POI and squad-level dispatch, not by per-bot vision."*
- **Modelo "go here, loot this and check other items in proximity"** — diferente do LB (que loota o que **vê**).
- **Corpse drainage em dois trilhos** (itens visíveis vs. track de busca); **cash antes de wallets**; **só mods removíveis**.
- **Chain-loot com viés de mesmo andar** (evita o "elevator yo-yo" no Resort).
- **Squad memory** — membros não lootam o que o squad já pegou; espalham por POIs próximos em vez de empilhar.
- **Anima ajoelhar para lootar** (pickup animation da BSG); **prone ao lootar exposto** está na lista de ideias.
- **Sistema de "weapon swap scoring" em construção** (04/06): pontuar troca de arma por mapa (CQB vs longo alcance). Discussão rica com Shynd → ideia de classificar por **classe de arma + óptica** ou até por **MOA** (≤2.5 MOA → range; >2.5 MOA → CQB; com regras de cruzamento).
- **Tuning ao vivo:** Klinical pediu **expor mais knobs de looting** no F12 — Chazut topou para o próximo patch.

**Bug relacionado (nos logs):** `F2447: HandbookClass never became available; ItemPriceLookup will return 0 (bots treat all loot as worthless)` — se o handbook de preços não carrega, os bots tratam **todo loot como sem valor**. Apareceu no log do crash do Bloody.

---

## 8. Objetivos por squad + lógica de extract

**Objetivos (rolados no spawn, com pesos normalizados):**
- **3 tipos:** **Kills** (caçar PvP em hotzones), **LootValue** (ir às zonas mais ricas, com thresholds por personalidade), **Quest**.
- **Viés por personalidade:** Rat → loot; Wreckless/Chad/GigaChad → kills.
- **Quests deliberadamente simples:** o bot **vai até o ponto da quest e espera um pouco**; se a quest tem vários objetivos, faz **só um**. Não roda a questline completa (isso é o **QuestingBots**). É só "mais um tipo de objetivo para mandar bots a lugares interessantes".
- **Roam livre entre objetivos** via advection field (herdado do Phobos). **Convergência para o player foi removida** por design (Chazut achou "artificial") — pode ser readicionada como **toggle opcional** se a comunidade pedir.

**Extract:**
- Escolhe o **exfil elegível mais próximo** e **se compromete** com ele (ainda **sem fallback** para o próximo se não conseguir chegar — está no roadmap).
- Só permite exfils **no lado oposto ao spawn** (mesma restrição do player) — feature **exclusiva do ORBIT**.
- **Car Extract:** o membro do squad **espera os companheiros** antes de iniciar o timer (evita extrair sem o ferido lento).
- **Reserve:** exfils desabilitados por enquanto (exigem ligar switches/levers — complexo).
- **Squad extrai quando:** atingiu o threshold de loot, completou objetivos, ou o tempo de raid está acabando.

---

## 9. ORBIT vs Phobos vs LootingBots vs QuestingBots

| | Relação com o ORBIT |
|---|---|
| **Phobos** (Janky) | **Base** do ORBIT (advection + dispatch), com permissão. ORBIT **removeu a convergência ao player** e **reescreveu a camada de ação** (loot/extract/objetivos). Filosofia: Phobos = "caos / nowhere is safe"; ORBIT = "comportamento de squad crível, como live EFT" (raid variance). |
| **LootingBots** (Skwizzy / mant. Archangelway) | Looting **inspirado**, mas **reescrito do zero** (sem código do LB na versão final). LB é **mais profundo** (corpse looting com troca inteligente de arma comparando gun+ammo+mods, rig swap, compat. de magazine). |
| **QuestingBots** (dwesterwick/DanW) | **Inspiração**, **sem reuso de código**. QB é **muito mais profundo** em quests (faz a quest de verdade, timers, favorece quests do player, Labyrinth). ORBIT é simples de propósito. |
| **Incompatibilidades** | ORBIT **conflita** com LB, QB e Phobos standalone (e qualquer outro "AI overhaul") — *"layer fight"*. Rodar LB junto = bugs (foi o caso do bug das Goons da die mitze). |

---

## 10. Integração com o Raid Review (RR)

- Chazut **recomenda fortemente** instalar o **Raid Review** junto: ele **visualiza** os objetivos principais de cada squad + o movimento no mapa pós-raid + o que cada bot lootou. *"grab last Raid Review and click on a bot dot, it shows each squad's main objectives + movement."*
- O ORBIT tem **API de integração com o RR**.
- Exemplos nas imagens: [att-24](./assets/att-24-2026-06-04-MauwMa.png) (bot **Chad "svastonoleg"** que extraiu), [att-31](./assets/att-31-2026-06-04-Chazut.png) (paths no Woods) e [att-32](./assets/att-32-2026-06-04-Chazut.png) (bot **Rat "btdc00"** que lootou **13 itens / ₽223.251**).
- **Caveat (Shynd):** rodar o RR **sobre Fika / múltiplos clientes ao mesmo tempo** pode **aumentar a busca por containers** dos bots — a própria página do RR desaconselha esse uso.

---

## 11. A "novela" das permissões (contexto social que move o thread)

Boa parte do thread gira em torno disso — vale entender para não chegar perdido:

1. Chazut **lançou o ORBIT empolgado, antes de pedir permissão** formal aos autores das bases (Phobos e **LootingBots**), pois a primeira versão **integrava um pedaço do código do LB** no looting.
2. Percebeu o erro e **removeu voluntariamente** da Hub no mesmo dia. Assumiu publicamente o erro ("feeling pretty guilty and stupid about that one").
3. **Pediu para a comunidade NÃO pressionar** os autores do LB (Arch e Skwizzy) — pressão poderia empurrar um "talvez" para um "não". Houve debate sobre etiqueta da comunidade (Vendth x Shynd x S41elite x Slum_K1ng).
4. Sem resposta clara (só uma **reação de sapinho 🐸** = *"we're watching"*), Chazut tocou um **"Plan B"**: **remover 100% do código do LB** e reescrever o looting do zero (branch `refactor/lb-removal`, [att-14](./assets/att-14-2026-06-03-Chazut.png)). Chegou a **deletar o clone local do LB e desligar a busca de repos do assistente de IA** para garantir zero vazamento.
5. **Archangelway** (mantenedor do LB) deu o veredito (03–04/06): ok republicar, mas quer **o código do LB totalmente fora até o SPT 4.1**, senão pediria unpublish. Chazut respondeu que **a remoção já estava feita** → **ficaram alinhados**, sem ressentimentos.
6. **Relançou** com o looting próprio.

**Lição que o próprio Chazut tira:** *"should have asked first. My bad."* — e a comunidade majoritariamente apoiou.

---

## 12. Bugs reportados e respostas do dev

| Bug | Quem reportou | Resposta / status |
|---|---|---|
| Bots travam tentando exfil **"Hole in Fence"** no Interchange (2º andar do mall) | Scootis_McPootis | **Desabilitou os exfils "backpack"** no dev branch; vai revisar exfils condicionais. |
| **PMC parado num transit** (Streets), sem se mover o resto da raid | Recker, die mitze | **Não é camping de transit.** É um **quirk vanilla de spawn**: o bot nasce num **pedaço isolado de navmesh** (mesmo bug do silo em Factory). Fix planejado: **teleportar** o bot preso para local válido. |
| PMC atravessa **minefield** em Lighthouse e "extrai" | MauwMa | **Bug real** — ao reconstruir a geração de POIs (Phobos faltava POIs), **esqueceu de excluir as zonas de minefield**. Vai corrigir. |
| **Spam de log** `RequestNear … bee-lining to own-kill corpse` (megabytes/raid) em Labs | Klinical | **Bug real mas inofensivo**; corrige no próximo update. |
| **Portas sempre fechadas / "phantom walk"** (bots atravessam sem abrir) | LifeBosses, Klinical | ORBIT **não fecha portas** (só abre se precisar passar/lootear, com roll em trancadas). O **trigger de abrir porta** pode estar falhando às vezes — investigando. |
| **Crash de memória (OOM)** ao carregar **Shoreline** | Bloody | **Não é do ORBIT** — `System out of memory` no `Player.log`. Diagnóstico do Shynd: pagefile/virtual memory mexidos ou pouco espaço em disco. (Bloody resolveu **fechando o Discord**.) |
| **Bots "burros"/freeze com ABPS** | ValFin | **Provavelmente não é o ORBIT** (ABPS só mexe em spawns). Único a reportar; pediram clipe. |
| **Bots estáticos no regroup** (fáceis de 1-tap) | Ika | Nitpick reconhecido; ligado ao "movimento muito reto" (roadmap: smarter movement). |
| **Drop de FPS** após instalar ORBIT | Bourne | Em apuração; Chazut/Fums lembram que ORBIT é leve mas **não invisível** à performance, e que **LB+Phobos são incompatíveis**. |

> Padrão das respostas do Chazut: **agradece o report, reproduz/explica a causa raiz, diz se é do ORBIT ou vanilla/outro mod, e registra o fix.** Postura técnica e transparente.

---

## 13. Performance

- **Vs Phobos:** ligeiramente **mais pesado** ("does more per frame"), pois adiciona objetivos + looting. O custo real continua sendo o **pathfinding da BSG**.
- **Vs LB + QB juntos:** vários relatos (inclusive **headless**: DeW4VE, Klinical) dizem que o **ORBIT roda melhor**.
- **Tuning de perf já feito:** intervalo do "opportunistic corpse scan" mudou de **0.5s → 2.5s** no default (configs antigos precisam ajustar manualmente no F12 ou deletar `com.chazut.orbit.cfg`).
- **Mods de "AI limit"** (que congelam bots distantes): **desaconselhados** por Chazut/Shynd/Archangelway — são "antitéticos" ao ORBIT, porque o efeito "raid viva" depende dos bots **agirem em background**. Se congelar, você não acha zonas pré-lootadas, corpos esvaziados, e extrações ficam raras. Se for usar limiter, preferir o **Fika Dynamic AI** (do lacy) a "AI Limit". O **limiter do próprio SAIN** (AI vs AI vision/hearing) é **ok** com ORBIT.

---

## 14. Guia de instalação, ordem de carregamento e compatibilidade

> Seção prática e autocontida. Resume "o que instalar, em que ordem, como carrega e o que combina/conflita". Os passos de config do SAIN estão detalhados em [§4](#4-como-o-orbit-interage-com-o-sain) e [§5](#5-existe-um-preset-ideal-de-sain).

### 14.1 A regra de ouro — existem 3 tipos de "mod de IA"

O que pode ou não rodar junto **depende da categoria** do mod, não do nome:

| Categoria | O que faz | Exemplos | Com ORBIT? |
|---|---|---|---|
| **Overhaul de comportamento** | **Dirige o cérebro** do bot fora de combate (movimento/objetivos/loot) | **Phobos, LootingBots (LB), QuestingBots (QB)** | ❌ **Incompatível** — *layer fight*, só um pode dirigir |
| **Combate** | Visão, audição, mira, cover | **SAIN** | ✅ **Obrigatório** (camada separada) |
| **Facção / novos tipos de bot** | **Adiciona** inimigos/facções novas | **Black Division, UNTAR, RUAF, ISB**, Miyako Carry Service | ✅ **Compatível, com toggle** (ver 14.6) |
| **Spawn / loadout / loot-table** | **Quem** aparece, **com o quê**, **o que** dropa | **ABPS** (spawns), **APBS** (loadouts), Fontaine RM | ✅ **Compatível e recomendado** |
| **AI limit** | Congela bots distantes p/ poupar CPU | AI Limit, Fika Dynamic AI | ⚠️ Funciona, mas **degrada** a experiência (ver [§13](#13-performance)) |

> **Teste rápido para classificar qualquer mod de IA:** ele **muda como os bots existentes se movem/decidem fora de combate** (→ incompatível) ou **adiciona facção / spawn / loadout** (→ compatível)?

### 14.2 Não confundir siglas parecidas

| Mod | Nome real | Camada |
|---|---|---|
| **Waypoints** | Waypoints – Expanded Navmesh (DrakiaXYZ) | **Pathfinding** — estende o navmesh ("onde o bot *pode* andar"). Dependência. |
| **ABPS** | [Acid's **Bot Placement** System](https://forge.sp-tarkov.com/mod/2097/abps-acids-bot-placement-system) | **Spawn** — waves, nº de bots, chance de boss ("*quais* bots aparecem"). |
| **APBS** | [Acid's **Progressive** Bot System](https://forge.sp-tarkov.com/mod/1594/apbs-acids-progressive-bot-system) | **Loadout** — gear/arma/munição por tier ("*com o quê* aparecem"). |

→ **Waypoints e ABPS não fazem a mesma coisa e não conflitam** (pathfinding vs spawn). E **ABPS NÃO é "AI overhaul"** — a frase "any other AI overhaul mod" na modpage confundiu gente no thread (o Filipe chegou a desinstalar ABPS achando que era incompatível; Chazut esclareceu que é compatível e recomendado).

### 14.3 Dependências obrigatórias (com versões)

| Mod | Versão mínima | Autor |
|---|---|---|
| **BigBrain** | ≥ 1.4.0 | DrakiaXYZ |
| **Waypoints – Expanded Navmesh** | ≥ 1.8.2 | DrakiaXYZ |
| **SAIN** | 4.4.3 (testado) | Solarint / ArchangelWTF / DrakiaXYZ |

> Sem **SAIN**, o ORBIT **não carrega** (hard dependency no BepInEx — some silenciosamente, sem crash, e nem aparece no F12). BigBrain e Waypoints **também são dependências do próprio SAIN**.

### 14.4 Ordem de **instalação** (passo a passo)

Dois "trilhos" diferentes:

- **Plugins de cliente** → vão para `BepInEx/plugins/` (o zip do ORBIT é só **extrair na raiz do SPT**).
- **Mods de servidor** → vão para `user/mods/`. **APBS** (loadouts) é server-side — por isso **nem aparece** na lista de plugins do log; só o **ABPS** (spawn) tem componente de cliente.

Sequência recomendada (dependências primeiro — **boa prática**, não obrigatória, já que o BepInEx reordena no load):

1. **BigBrain** (≥1.4.0)
2. **Waypoints – Expanded Navmesh** (≥1.8.2)
3. **SAIN** (4.4.3)
4. **ORBIT** (extrair o zip na raiz do SPT)
5. **ABPS** (spawns) + **APBS** (loadouts) — recomendados
6. *(opcional)* **Raid Review** — fortemente recomendado para enxergar o que os bots fazem
7. *(co-op)* **Fika** — ORBIT roda sob Fika/headless

### 14.5 Ordem de **carregamento** (automática) — cenário máximo

**Você não define a ordem de load manualmente.** Para plugins de cliente, o **BepInEx resolve sozinho** pelo grafo de dependências (`[BepInDependency]`). A tabela abaixo é a **pilha lógica / de dependências** (a ordem que *importa*), **não** a ordem literal que o BepInEx imprime no log (essa pode parecer aleatória — ver a ressalva mais abaixo).

> 🔴 Obrigatório · 🟢 Opcional recomendado · ⚪ Opcional (gosto) — `[C]` cliente (`BepInEx/plugins`) · `[S]` servidor (`user/mods`)

| # | Camada | Mod | | Trilho |
|---|---|---|---|---|
| 0 | **Base** | BepInEx + Configuration Manager + SPT.Core/Custom/Singleplayer | 🔴 (vem com o SPT) | `[C]`/`[S]` |
| 1 | **Frameworks de IA** | **BigBrain** ≥1.4.0 | 🔴 | `[C]` |
| 1 | **Frameworks de IA** | **Waypoints – Expanded Navmesh** ≥1.8.2 | 🔴 | `[C]` |
| 2 | **Spawn** | **ABPS** (Bot Placement System) | 🟢 | `[C]`+`[S]` |
| 2 | **Loadout** | **APBS** (Progressive Bot System) | 🟢 | `[S]` |
| 3 | **Facções / novos bots** | **Black Division · UNTAR · RUAF · ISB** | ⚪ | `[C]`+`[S]` |
| 4 | **Combate** | **SAIN** 4.4.3 | 🔴 | `[C]` |
| 5 | **Overhaul de comportamento** | **ORBIT** | 🔴 (o mod) | `[C]` |
| 6 | **Co-op** | **Fika** | 🟢 (só multiplayer) | `[C]`+`[S]` |
| 7 | **Diagnóstico** | **Raid Review** | 🟢 | `[C]`+`[S]` |

**Por que essa ordem (lógica):**
- As **camadas 1–4 vêm "antes" do ORBIT** porque ele depende delas: precisa do **BigBrain** (camadas de cérebro), do **Waypoints** (navmesh), do **SAIN** (personalidades + combate), e que **spawns (ABPS) / loadouts (APBS) / facções** já tenham **gerado os bots** para ele assumir.
- **ORBIT (camada 5) é o topo da cadeia de IA** — é a relação **garantida** pelas dependências e confirmada no log.
- **Onde entram as facções (camada 3):** elas *adicionam tipos de bot*. O ORBIT as detecta **por presença de GUID no início da raid** (não por posição de load), então **a posição exata é indiferente** — basta estarem instaladas que o toggle aparece no F12. Como o ORBIT é **opt-OUT** hoje (ver [§14.7](#147-mods-de-facção-black-division--untar--ruaf--isb)), ele captura esses bots automaticamente, a menos que você escolha "manter o cérebro do autor".
- **Fika e Raid Review** ficam por cima — não influenciam a IA, só multiplayer e visualização.

**A realidade do BepInEx (importante):** a ordem **literal** de instanciação dos plugins é resolvida automaticamente e **pode não bater** com a pilha lógica. No log do thread ([att-30](./assets/att-30-2026-06-04-Shynd.log)) a ordem impressa foi, por ex.:

```
ABPS(73) → BigBrain(111) → SAIN(120) → ORBIT(230) → Fika(367) → Raid Review(1139) → Waypoints(1193)
```

Repare que o **Waypoints carregou por último** e mesmo assim o ORBIT inicializou certo (`BrainManager wired`) — porque esses mods **se ligam a eventos do ciclo de raid**, não à ordem de instanciação do plugin. A única relação **garantida** e que importa é a cadeia **`BigBrain → SAIN → ORBIT`**.

→ **Você não precisa (nem deve) ordenar plugins de cliente na mão.** Para mods de **servidor** (`user/mods`), a ordem default também resolve (via `loadAfter` nos `package.json`). O que importa **não é a ordem**, e sim **ter as versões certas** + a **config** abaixo.

### 14.6 Config obrigatória / recomendada

1. **Desligar o extract do SAIN** (senão briga com o do ORBIT) — editar `BepInEx/plugins/SAIN/Presets/<seu_preset>/GlobalSettings.json`:
   ```json
   "Extract": { "SAIN_EXTRACT_TOGGLE": false }
   ```
2. **Aplicar o preset de personalidades** do SAIN (F12 → Personality → Assignment) — ver tabela em [§5](#5-existe-um-preset-ideal-de-sain).
3. **Sliders de combate do SAIN** (visão/audição/mira): mexer à vontade, **não conflitam** com o ORBIT.
4. Tudo do ORBIT se ajusta **ao vivo no F12** in-game.

### 14.7 Mods de facção (Black Division / UNTAR / RUAF / ISB)

São **facções novas** ([Black Division](https://welcometotarkov.wiki.gg/wiki/Black_Division_(Custom)) caça PMCs em matilha; [UNTAR](https://forge.sp-tarkov.com/mod/2342/untar-go-home) = ONU que só atira se ameaçada; [RUAF](https://forge.sp-tarkov.com/mod/2427/ruaf-come-home) = forças russas), **não** overhauls. Chazut confirmou no thread: *"is this mod compatible with black division? yes, and RUAF/UNTAR as well... there is a toggle to keep their brain from the mod author, or to make them roam with ORBIT."*

- O ORBIT **detecta cada facção por GUID** e expõe um **toggle no F12** (logs: `UNTAR/RUAF/BlackDivision: plugin '…' not present — toggle inert` quando ausentes). Por facção você escolhe: **manter o cérebro do autor da facção**, ou **fazê-la roam com o ORBIT**.
- ⚠️ **Comportamento default = opt-OUT:** hoje o ORBIT **controla todo tipo de bot por default** (qualquer bot novo de qualquer mod é capturado automaticamente). Facções **sem toggle dedicado** (ex.: **ISB Special Force**, Miyako Carry Service) passam a roam com objetivos do ORBIT, o que pode atropelar comportamentos muito custom. Chazut planeja **inverter para opt-IN por tipo de bot** (mais seguro) e há suporte a **ISB** no todo dele.

### 14.8 Tabela de compatibilidade (resumo)

| ✅ Recomendado / compatível | ❌ Incompatível |
|---|---|
| **SAIN** (obrigatório, combate) | **LootingBots** (overhaul) |
| **BigBrain + Waypoints** (dependências) | **QuestingBots** (overhaul) |
| **ABPS** (spawns) · **APBS** (loadouts) | **Phobos** standalone (overhaul) |
| **Black Division / UNTAR / RUAF / ISB** (facção, com toggle) | Qualquer outro **"AI overhaul"** de comportamento |
| **Fontaine's RM** · mods de loot-table | ⚠️ **AI limit mods** (rodam, mas degradam) |
| **Raid Review** · **Fika** (headless/multiplayer) | |

### 14.9 Checklist final

- [ ] BigBrain, Waypoints, SAIN instalados (versões certas) e ORBIT extraído na raiz
- [ ] `SAIN_EXTRACT_TOGGLE` = `false`
- [ ] Preset de personalidades do SAIN aplicado ([§5](#5-existe-um-preset-ideal-de-sain))
- [ ] Spawns/loadouts via ABPS/APBS (opcional, recomendado)
- [ ] **Nenhum** outro overhaul de comportamento (LB/QB/Phobos) instalado
- [ ] Facções (Black Division/UNTAR/RUAF) com o toggle decidido no F12
- [ ] *(opcional)* Raid Review para diagnosticar comportamento

### 14.10 E no servidor? Load order de mods server-side (SPT 4.0 = C#)

> O ORBIT é **client** (BepInEx). Mas a pilha recomendada tem peças **de servidor**: **APBS** (loadouts) e o componente server do **ABPS** (spawns). No SPT 4.0 o servidor virou **C#** e o modelo de load order **mudou** em relação ao 3.x.

Como o servidor 4.0 carrega (verificado no source [server-csharp](https://github.com/sp-tarkov/server-csharp)):

- **Descoberta:** cada subpasta de `user/mods/` é um mod (com DLL); importadas em ordem **alfabética** de pasta (`Directory.GetDirectories`).
- **Metadata por classe** (`AbstractModMetadata`, **não mais** `package.json`): `ModGuid`, `Version`, **`SptVersion`** (constraint semver, ex. `~4.0.0`), **`ModDependencies`** (`GUID → faixa de versão`) e **`Incompatibilities`**. → **Não existe `LoadBefore`/`LoadAfter`** (isso era 3.x).
- **Validação** (`ModValidator`): dependências presentes/compatíveis + incompatibilidades + `SptVersion`. Se **qualquer** mod falha, **nenhum** carrega (tudo-ou-nada).
- **Execução = fases de DI** (`OnLoadOrder`): `PreSptModLoader → Database → PostDBModLoader → … → PostSptModLoader`. **A ordem que importa é a fase, não a pasta.**

**Existe "ordem ideal" no servidor?** Praticamente **não se gerencia à mão** no 4.0 — o framework resolve via `ModDependencies` + fases OnLoad. O que importa é **ter as versões certas** e **sem conflitos**. Tiebreak cru (raro): renomear a pasta força a ordem alfabética de import.

> ⚠️ **Obsoleto no 4.0:** `loadBefore`/`loadAfter` no `package.json`, `loadorder.json` e o **Load Order Editor (LOE)** são do **SPT 3.x** — não se aplicam ao server C# do 4.0.

---

## 15. Pontos legais e curiosos

- **O nome é um trocadilho.** "ORBIT" / "went into orbit": quando uma **explosão arremessa os bots para o alto**, Chazut brincou — *"the Rat arrived and went into orbit as well"* ([att-31](./assets/att-31-2026-06-04-Chazut.png) / [att-32](./assets/att-32-2026-06-04-Chazut.png)). Bug virou meme do próprio mod.
- **Transparência sobre IA.** Aba "About AI": *"Claude was used as an implementation assistant. Architecture, design, debugging, and all in-game testing are mine."* Chazut também contou que **desligou a busca de repos do assistente de IA** para não vazar código do LB, e brincou que "**graças a Deus a IA mastiga logs de centenas de milhares de linhas**" para análise.
- **Dev não-nativo em inglês.** Chazut usa **DeepL** para polir o texto e **estourou a cota grátis** respondendo o thread ("google trad isn't great, same level as me").
- **Meme dos 666 downloads** ([att-25](./assets/att-25-2026-06-04-Chazut.png)) e o meme do Palpatine *"we will watch your mod progress with great interest"* ([att-01](./assets/att-01-2026-05-28-Baconism.jpg)).
- **A ideia do MOA** (Shynd): classificar o "estilo" do bot pela **precisão da arma** (≤2.5 MOA → objetivos de range; >2.5 MOA → CQB), com regras de cruzamento (gun preciso de alto RPM ainda serve em CQB; arma imprecisa com luneta ainda serve à distância). Chazut adorou ("way cleaner than a hardcoded weapon-class table").
- **"A cola que faltava no SPT"** (Ika): *"the glue that SPT has needed to bring AI mods together."* Vários veem o ORBIT como candidato a **mod "obrigatório"** como SAIN/APBS (Fums).
- **Determinístico, não por visão.** Diferente do LB (reativo à visão do bot), o looting do ORBIT é **determinístico por POI indexado no mundo** — escolha arquitetural deliberada.
- **Detalhe de imersão:** o bot **só extrai pelo lado oposto ao spawn**, igual ao player; e o **Car Extract espera o squad**.
- **Cultists viram subplot cômico:** o Cosmin passou **3 dias** caçando cultists para uma quest ("niddle in the haysack"), e um **priest se suicidou com granada** ao ver os guardas mortos.

---

## 16. Roadmap (do que foi falado no thread + página)

**Confirmado "na lista" (sem ETA):**
- Bots preferem loot no **mesmo andar** (já em teste).
- Membros podem **extrair sozinhos** se baterem o próprio threshold de loot.
- Squads podem **campar + emboscar** em vez de só roam.
- Objetivo **"Marked-key loot rush"** (high-tier).
- Objetivo **"Spawn rush"** (personalidades agressivas).
- **"Boss hunting"** (roam entre spawns de boss).
- **Movimento mais inteligente** (checar cantos, escanear a retaguarda, menos dash em linha reta).
- **Reserve exfils** (hoje desabilitados — precisam de switches).
- **Squad splitting** com radio comms.
- **Novas personalidades** (ex.: **"Camper"** para sniping estático).
- Objetivos de **airdrop / heli crash / BTR**.
- Objetivos **multi-step** (ex.: KIBA alarm disarm, ULTRA power).
- **Fix do bot preso em navmesh isolado** (teleporte leve).
- **Flip da lógica de controle** de bots: de **opt-OUT** (controla todo bot por default) para **opt-IN por tipo de bot** (mais seguro).
- **Toggle de convergência ao player** de volta (opcional), se a comunidade pedir.
- **Expor mais knobs de looting** no F12.
- **Weapon-aware behaviour** (CQB vs range por arma/MOA).
- Suporte a facções (**ISB**, etc.).

---

## 17. Glossário rápido

| Termo | Significado |
|---|---|
| **SAIN** | *Solarint's AI Modifications* — overhaul de **combate** dos bots (visão, audição, mira, cover). Dependência obrigatória. |
| **BigBrain** | Framework (DrakiaXYZ) de **camadas de cérebro** dos bots; o `BrainManager` orquestra qual lógica controla o bot. |
| **Waypoints – Expanded Navmesh** | Navmesh estendido (DrakiaXYZ) para pathfinding em áreas que a BSG não cobre. |
| **Phobos** | Mod de IA do **Janky** (pré-release, só no Discord dele); base do ORBIT — espalha bots via **advection field**. |
| **Advection field** | Campo de "forças" que empurra os bots organicamente pelo mapa (em vez de rotas fixas). |
| **POI** | *Point of Interest* — pontos do mapa que o ORBIT indexa (loot, exfil, quest, etc.) e usa para mandar squads. |
| **Convergência** | Força (do Phobos) que puxa bots **na direção do player** ("nowhere is safe"). **Removida** no ORBIT. |
| **Raid Review (RR)** | Mod que mostra, pós-raid, o que cada bot fez no mapa. Recomendado com ORBIT. |
| **ABPS / APBS** | Mods de **spawn** / **loadouts** (não são AI overhaul). Compatíveis e recomendados. |
| **Fika** | Mod de **co-op/multiplayer** para SPT; "headless" = cliente dedicado que roda a raid. ORBIT funciona sob Fika. |
| **LB / QB** | LootingBots / QuestingBots — mods de looting / questing. Inspirações; **incompatíveis** com ORBIT. |
| **Personalidades (Rat/Chad/GigaChad/Wreckless/Coward/Timmy/SnappingTurtle/Normal)** | Arquétipos do **SAIN** que o ORBIT mapeia em comportamento de objetivo/loot. |

---

## 18. Cheat-sheet — como entrar na discussão já sabendo de tudo

- **O mod já está no ar** (v1.0.0, 04/06), com **looting próprio** (sem LB), **estável**, em **testes pesados**.
- A **briga de permissões acabou bem** (alinhado com o mantenedor do LB; código do LB 100% removido).
- ORBIT = **objetivos + looting + extract**; **SAIN faz o combate**. Instale **SAIN + BigBrain + Waypoints**, ative o **preset de personalidades** (§5) e **desligue o extract do SAIN**.
- Use **Raid Review** para enxergar o que os bots fazem.
- Reportes úteis = **map + situação + screenshot do Raid Review** (e `Player.log` para crashes).
- **Não** use junto: LB, QB, Phobos, outros AI overhauls; evite **AI limit mods**.

**Perguntas em aberto / boas para puxar no thread:**
- O fix do **bot preso em navmesh** e a **exclusão dos minefields** já entraram em release ou ainda estão no dev?
- O **weapon-aware behaviour** (MOA / classe de arma) virou feature concreta?
- Vão expor mais **knobs de looting** no F12 (pedido do Klinical)?
- Há plano de **toggle de convergência** opcional?
- Compatibilidade futura com **Pit Fireteam / Miyako Carry Service / ISB**?

---

## Fontes

- **Thread no Discord (primário):** <https://discord.com/channels/875684761291599922/1509314495019745451> — capturado em 2026-06-04.
- **Transcrição local:** [01-transcricao.md](./01-transcricao.md) (699 mensagens) + [imagens/logs](./assets/).
- **Forge:** <https://forge.sp-tarkov.com/mod/2706/orbit>
- **GitHub:** <https://github.com/Chazut/ORBIT>
- Dependências citadas: [SAIN](https://forge.sp-tarkov.com/mod/791), BigBrain e Waypoints (DrakiaXYZ).

## Histórico

| Data | Autor | Descrição |
|---|---|---|
| 2026-06-04 | Guilherme | docs(technical): move modding guides out of backlog, add frontmatter and index |
