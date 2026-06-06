---
title: "SPT Realism Mod — Análise do canal de desenvolvimento (Discord SPT, ~90 dias)"
date: 2026-06-05
status: 🔵 Em andamento
authors: Guilherme
---

# SPT Realism Mod — Análise do canal de desenvolvimento (Discord)

> Análise do canal **"Realism Mod Development"** no Discord da comunidade SPT (SPT Pub), recorte de **07/03 → 04/06/2026**, **1.132 mensagens**, **102 participantes**. Baseada na [transcrição completa](./01-transcricao.md), nas [imagens/vídeos anexados](./assets/) e em fontes públicas ([Forge](https://forge.sp-tarkov.com/mod/416/spt-realism-mod), [GitHub `space-commits`](https://github.com/space-commits)).
>
> Objetivo: permitir **entrar na discussão já entendendo tudo**. Afirmações marcadas com _(inferência)_ não são literais do chat.

---

## 0. TL;DR (resumo executivo)

- O **SPT Realism Mod** é um **overhaul hardcore** do Tarkov singleplayer (balística, médico, recuo, anexos, stances, zonas de perigo, economia de traders/flea, loadouts de bots), do dev **Fontaine**. A versão estável atual é **1.6.4 para SPT 3.11.4** (445 mil downloads). Este canal é o **thread de desenvolvimento** — não há lançamento para SPT 4.0 **ainda**.
- **Decisão arquitetural central do período:** o Realism **não terá um mod monolítico para 4.0**. As features viram **mods standalone** lançados **um de cada vez**, mais uma **CommonLib própria** e (provavelmente) integração estilo "Voltron". Fontaine confirma isso várias vezes (16/04, 23/04).
- **Maior entrega técnica do período:** o **rework completo do sistema de stances** (28/04) — controller e input **reescritos do zero** para usar **animation curves com keyframes** e **integrar-se ao sistema de animação procedural da BSG** (em vez de brigar com ele). Ganhos: FPS-independência, menos stutter com drop de frame, blending suave com sway/inércia. Demonstrado em 2 vídeos.
- **Outras frentes:** rework do **sistema médico** (blood loss, inspirado em parte por discussões sobre Gray Zone Warfare), overhaul de **balística** (mais armor boxes, modelar órgãos, eliminar o sistema de fragmentação bugado da BSG), rework de **ADS speed**, **Fika sync** virando addon de stances.
- **Clima social:** muito **"when update?"** (pestering por data de release) → moderação ativa de **DrakiaXYZ**/**Badger**/**GrooveypenguinX**, macro anti-pestering, um banimento (Kushlungs, por slurs). Fontaine responde com o bordão **"+2 weeks / +2 months"** e o meme **"Realism tomorrow / it's Thursday somewhere"**.
- **Estado em 04/06:** stances em **WIP avançado** (test builds públicos prometidos quando a maioria das stances estiver portada); nenhum módulo 4.0 público ainda; a comunidade segue jogando **3.11/3.10.5 com Realism** enquanto espera.

---

## 1. O que é o SPT Realism Mod

| Campo | Valor |
|---|---|
| **Nome** | SPT Realism Mod (SPTRM) |
| **Autor** | **Fontaine** (conta GitHub: [`space-commits`](https://github.com/space-commits); também tem **Patreon**) |
| **Versão estável** | **1.6.4** (para **SPT 3.11.4**) — _Forge: "última atualização 14/05"_ |
| **Licença** | **CC BY-NC-ND 4.0** |
| **Downloads** | ~**445,2 mil** (Forge, mod #416) |
| **Repos** | Client (C#, BepInEx), Server (TypeScript), GUI (WinForms C#), + standalones: Recoil-Overhaul, Combat-Stances, Malfunctions, FOV-Fix |
| **Forge** | <https://forge.sp-tarkov.com/mod/416/spt-realism-mod> |
| **Hub** | <https://hub.sp-tarkov.com/files/file/606-spt-realism-mod/> |
| **Dependências** | Nenhuma listada hoje; **no 4.0 passará a depender da CommonLib própria** _(inferência a partir de 19/03 e 16/04)_ |
| **Estado p/ 4.0** | **Em desenvolvimento** — será **modular** (mods separados), sem release único |

O mod é um dos pilares do ecossistema SPT: tanto que boa parte da comunidade **se recusa a migrar para o SPT 4.0 enquanto o Realism não for portado** ("I refuse to wet my willy with 4.0 until RM is properly updated" — Dashwood Foxe, 11/03).

---

## 2. Quem é quem

| Participante | Msgs | Papel no canal |
|---|---:|---|
| **Fontaine** | 162 | **Autor/dev do Realism.** Define rumo técnico, responde dúvidas, demonstra WIP. Tom seco e sarcástico ("you'll get what you deserve"), visão hardcore declarada. |
| **Jpdarkone³** | 130 | **Superfã nº 1** e maior piadista do "when update". Vira alvo recorrente de moderação amigável; também puxa muito off-topic. |
| **Mout Duck** | 97 | Convertido ao **Gray Zone Warfare**; longos comparativos de movimento/feel GZW × Tarkov. |
| **Dashwood Foxe** | 76 | Superfã, nerd de armas/lore, memes; pressiona por features (bots com ergo real, IV anims). |
| **PrescriptionAdderall** | 68 | Player experiente (Kappa, balanceamento de munição, blood system do GZW); respostas ponderadas. |
| **Tebz** | 62 | Evangelista do **GZW**; trouxe o sistema médico de blood bag que inspirou discussão. |
| **Nosliw** | 49 | Voz medida; rebate argumentos fracos; joga 3.10.5 com Realism. |
| **GrooveypenguinX** | 31 | Modder/moderador ligado à **WTT**; autor da **CommonLib**; aconselha estratégia de release; emula mecânicas de quest do EFT 1.0. |
| **adishee** | 30 | Faz configs do **PTT** (Path to Tarkov); levantou conflito PTT × Interactable Exfils × Realism. |
| **J3RN3J** | 26 | Banter/wit. |
| **DrakiaXYZ** | 10 | **Dev core do SPT** + moderador. Aplica a política anti-pestering ("don't harass mod authors for updates"); ameaça fechar o thread. |
| **Qwertyalex** | 18 | Time **WTT**; lê código (Interactable Exfils, Realism); dá suporte técnico preciso. |
| **S41elite** | 19 | Respondedor prestativo (munição, zonas de dano). |
| **Twank bwattewy** | 18 | Player mais novo; dúvidas de config/munição (7.62×51, comfort modifier, gear de bots). |
| **Kojimbooo** | 17 | Criador de conteúdo; posta vídeos NVG/trailers/edits. |
| **Ser_G** | 14 | **Modelador de armas** (render "KillshotRifle" WIP). |
| **Kobe Thuy** | 13 | Banter; conhece o roteiro 4.0 → 4.1 → 1.0. |
| **BraveStarr** | 13 | "Sonhador" do SPT 5.0; piadas sobre Fika. |
| **Archangel(way)** | 15 | Postura semi-moderadora; fatos sobre Fika; piada "lançamos 4.1 quando o Realism atualizar". |
| **Badger** | 4 | Moderador (baniu Kushlungs). |
| **BoostedStache** | 7 | **Debugou** o bug de traders quebrados (isolou até Gunsmith). |
| **Bobby Renzobbi** | 2 | Postou o changelog do heartbeat SFX (adrenalina). |
| **gusy_gusy** | 2 | Reportou o "bug" de placa traseira destruída / dianteira intacta. |
| **Kushlungs** | 3 | **Banido** por assédio/slurs ao pressionar por updates. |

> Outros ~80 participantes com 1–8 msgs cada (suporte pontual, memes, perguntas de "when").

---

## 3. Linha do tempo do desenvolvimento (recorte de 90 dias)

| Data | Marco |
|---|---|
| **10–11/03** | Conflito **PTT × Interactable Exfils × Realism** (adishee/Qwertyalex). Fontaine: Realism altera exfils de Factory se o gás não estiver desligado. **Primeiro teaser do novo sistema de stances** (vídeo WIP "pre-pre-pre-alpha", ainda só com springs). |
| **12–13/03** | Crítica ao rework de ergonomia/shouldering da BSG no EFT live; Fontaine explica o plano (mover **arma→câmera**, não câmera→arma). **Drama de moderação**: Kushlungs assedia/usa slurs → banido do thread, da WTT e (sugerido) do Fika. |
| **19–20/03** | Discussão de **modularização** ("quebrar o mod em mods individuais + integração"). GrooveypenguinX descreve a **CommonLib** (salvage zones, secretQuest, group positions). Realism poderá depender dela. |
| **21–22/03** | **Sistema médico**: Tebz traz o blood-bag do **Gray Zone Warfare**; Fontaine confirma planos de **blood loss** no médico do Realism. |
| **26/03** | **Poll de estratégia do port 4.0**: (1) portar módulos **as-is** do 3.11 vs (2) **refatorar/melhorar/adicionar features**. Fontaine mapeia dificuldade por módulo. |
| **02/04** | Fontaine de **férias (2 semanas)**. Pico de memes de "Realism tomorrow". |
| **10/04** | Plano de **rework do ADS speed** (velocidade mínima bem mais rápida + mais movimento/desalinhamento durante a animação). |
| **16/04** | **Confirmação oficial:** "não haverá Realism para 4.0; as features serão **mods standalone**, lançadas uma a uma". |
| **22/04** | Bug do **heartbeat SFX da adrenalina** (loop infinito). Fontaine: "preciso corrigir o looping na próxima versão". DrakiaXYZ ameaça fechar o thread por pestering. |
| **28/04** | **Anúncio grande:** **rework de stances** com animation curves (controller/input reescritos do zero, integração com a animação procedural da BSG). **2 vídeos** demonstrativos. |
| **29/04** | Detalhes: primeiro reimplementar o 3.11, depois **test builds públicos**, depois novas features; heavy weapons trocam "forçar low-ready" por **dreno extra de stamina + camada de sway**. |
| **03/05** | Esclarecimento de uninstall (att-20: **GUI de config**, toggles "Revert Med Changes" + "Revert HP"). **Nenhuma feature do Realism exige profile wipe.** |
| **05/05** | BSG adiciona shouldering anim no test server; comparação favorável ao Realism. |
| **06/05** | Fontaine quer **refazer balística** e **eliminar o sistema de fragmentação da BSG**. |
| **15–16/05** | "Bug" de placas (att-22) explicado como **cobertura realista de placas** + pose da BSG. **BoostedStache isola** o bug de traders até as quests do **Gunsmith**. |
| **17/05** | Fontaine: provavelmente a modificação das quests de GS corrompe o estado; "**não será relevante para o 4.x, o server é completamente diferente agora**". |
| **28/05 – 04/06** | Conteúdo de comunidade (vídeos NVG/trailers do Kojimbooo). Sem release de módulo 4.0 no período. |

---

## 4. A grande decisão: modularização do Realism para 4.0

O ponto mais importante para "entrar na discussão". Fontaine vai **dissolver o mod monolítico** em peças:

- **Não existirá "Realism 4.0"** como um download único. Em vez disso, **cada feature** vira um **mod standalone**, **lançado individualmente** conforme fica pronta (16/04, 23/04).
- Haverá uma **CommonLib do Realism** (o próprio Realism "vira common lib", 19/03) — e possivelmente um **mod de integração estilo Voltron** que junta os módulos _(inferência, a partir do vocabulário de BraveStarr em 19/03)_.
- **Ordem provável de release:** **stances + CommonLib** primeiro; depois **balística, médico, traders/economia, bots, hazards, anexos, recoil** (Fontaine prevê, em 23/04, ser "inundado" com "where/when ballistics/meds/traders/bots" assim que stances sair).

**Dificuldade de separar cada módulo** (Fontaine, 26/03):

| Módulo | Isolável? |
|---|---|
| **Ballistics/ammo** | Razoavelmente **bem isolado** → port mais fácil |
| **Medical** | **Mais difícil** de separar |
| **Hazards** (gas zones) | **Mais difícil** de separar |
| **Stances** | Exige **muito trabalho** para virar standalone (controller reescrito) |
| **Recoil** | **Muito difícil** sem "janky stuff" |
| **Attachments** | **Muito difícil** sem "janky stuff" |

> Já existem **standalones publicados** que confirmam a estratégia em andamento: **SPT-Combat-Stances**, **SPT-Realism-Recoil-Overhaul-Standalone**, **SPT-Realism-Malfunctions** (Inspectionless Malfs) e **SPT-FOV-Fix** no GitHub do Fontaine.

**Por que não simplesmente portar as-is?** Fontaine quer a rota 2 (refatorar + novas features) porque "grande parte precisa ser reescrita de qualquer forma" e ele tem "planos grandes" (ex.: mais armor boxes, modelar órgãos). GrooveypenguinX reforça: "apressar o release só vai te dar dor de cabeça depois; lance quando estiver finalizado e provoque a fanbase" (26/03).

---

## 5. Stances — o rework que dominou o período

O destaque técnico. Antes (3.11): transição linear **0 → posição/rotação final** via springs, com muito trabalho manual "jank", difícil de manter. Agora (28/04):

- **Animation curves com keyframes** → animações de **enter/exit distintas** e transições suaves entre stances.
- **Controller e input system reescritos do zero**, mantendo o "fluid stance input" pelo qual o Realism é conhecido.
- **Integração com o sistema de animação procedural da BSG** — as stances **deixam de brigar** com sway/movimento/inércia pelo controle. Benefícios citados: **menos stutter com drop de FPS**, **independência de FPS**, blending muito mais suave.
- **Base** para adicionar mais camadas/profundidade depois.
- **Velocidade** ainda depende de **ergonomia + peso** da arma (como no 3.11).

**Detalhes de design (29/04):**
- Primeiro objetivo: **reimplementar a funcionalidade do 3.11**; durante o processo, **test builds públicos**; só depois novas features.
- Sem mudar balanceamento/restrições radicalmente — mas, em vez de **forçar low-ready** em armas pesadas, o **idle vai drenar stamina extra + uma camada de sway** por cima.
- **ADS speed** (10/04): velocidade mínima bem mais rápida, porém com **mais movimento/desalinhamento** da mira durante a animação (melhor para "reactive shooting").

**Active Aim System (AAS):** o "hipfire"/active-aim é parte das stances do Realism, com **keybind no F12** (BepInEx config). Existe também o standalone **SPT-Combat-Stances**.

**Recepção:** unânime ("smoooth as butter", "BSG could never"). SplatRash (05/05): "não acho que exista transição high/low-ready mais suave em nenhum jogo no mercado agora".

> Anexos relevantes: `att-04` (primeiro WIP, springs), `att-18` (rework com curvas) e `att-19` (mesma cena slowed-down). **São `.mp4` — ver nota sobre peso em [§14](#14-notas-de-captura).**

---

## 6. Sistema médico, balística, hazards e bots

### 6.1 Médico
- **Blood loss system planejado** — Fontaine confirma "mudanças significativas no médico do Realism, blood loss será parte disso" (21/03), inspirado em parte pela discussão do **Gray Zone Warfare** (blood bag + blood meter). Ressalva dele: o sistema do GZW "não é tão profundo quanto soa; TTK ainda parece baixo".
- **Heartbeat SFX** ao ter o efeito **Adrenaline** ativo (adicionado ~nov/2025; créditos: ViperWolf263 pelos arquivos, Niall pela sugestão — att-16/17).
- **XP de skills**: as mudanças médicas afetam como skills médicas sobem; também dá XP de **Metabolism** (comer/beber) e de **Troubleshooting** ao **chambar manualmente** (30/03).
- **Sem profile wipe**: nenhuma feature do Realism jamais exigiu wipe (02/05). Uninstall = marcar **"Revert Med Changes"** + **"Revert HP"** na GUI e rodar o server (att-20).

### 6.2 Balística / dano
- **Zonas de dano variáveis no tórax** (não só braço/perna/torso/cabeça) → **shot placement importa** (18/04).
- **Placas com cobertura realista**: não cobrem o corpo todo; é possível **a bala contornar a placa dianteira, penetrar o corpo e acertar a traseira** — explica o "bug" reportado por gusy_gusy (15/05). Agravado pela **pose da BSG** (player inclinado pra frente, expõe o lado esquerdo).
- **Gap meta×stock estreito**: munição ruim deixa de ser "dardo de nerf" contra placas altas; FMJ de rifle já serve; piores: **5.45 US** e **flechettes**; **5.56 SOST** entre as melhores; **leg meta** forte (tiro na perna derruba o inimigo).
- **Pólvora não queimada** modelada via **muzzle flash/sparks** conforme **comprimento do cano, calibre e muzzle devices** (supressor, gas block, charging handle afetam a quantidade de gás). Cano curto = **menos dano**.
- **Shotguns**: pellets limitados a **8** desde 3.10+ (problema de performance da BSG com muitos pellets). Bem avaliados no 3.11 ("more pellets in buck + accurate plate placement").
- **Plano:** overhaul de balística com **mais armor boxes**, **órgãos** modelados mais a fundo, e **substituir o sistema de fragmentação bugado da BSG** (06/05).

### 6.3 Hazards (gas / toxic fog)
- Zonas de perigo com **gás/névoa tóxica**; há uma **quest line** cujo texto explica como lidar (Furgan ignorou o texto e ficou preso, 27/04). Para desligar só a fog: completar a quest line **ou** editar a chance no **`mod.ts`** (arriscado).
- **Spawn seguro**: o player é movido para fora da zona ao spawnar; **futuro:** flag para impedir isso. Zonas definidas em **arquivos JSON** (20/03).
- **Labs**: filtros **GP-7/AI-2** ajudam; pode ser necessário um **stim** (23/05).

### 6.4 Bots
- Realism gerencia **loadouts noturnos** (NVG/lanterna) e **pools de gear/armas** de bots via **JSON no server folder** (pré-populado com gear da **WTT** — remova os IDs para excluir).
- **Bots não** estão sujeitos a ADS/handling/recoil do player — isso é **comportamento de bot**, **domínio do SAIN**; precisão/spread vem da definição do bot. Gear/comportamento ficam a cargo de **APBS/Adi Bots/Phobos**.
- Bug conhecido: PMCs com **rig/capacete armados mas sem placas/soft armor** — geralmente gear do **Artem**; remover do pool (20/04).

---

## 7. Economia de traders/flea, recoil, anexos e config

- **Economia**: estoques de trader **randomizados** + **flea tiered**. Muito elogiada e **sem réplica standalone boa no 4.0** (SVM + RZcustomEconomy só chegam perto; **Hardcore Rules** é outra coisa — settings de streamer). Loot decrescente em raids repetidos no mesmo mapa = baseado em **loot XP** (25/04).
- **Recoil**: o **Recoil Rework** (mod terceiro) é "basicamente o mesmo do Realism + camera recoil extra"; dá pra usar os dois ajustando configs, ou só usar o **advanced recoil config** do Realism. Existe o standalone **SPT-Realism-Recoil-Overhaul-Standalone**.
- **SVM não edita** features específicas do Realism (ex.: Comfort Modifier de mochila) — editar no **server folder** do mod (08/04).
- **Config GUI (att-20)** — `SPT Realism Mod Config (SPTRM v1.6.3)`, abas: **Home · Realism and Ballistics · Health & Player · Bots · Traders & Flea · Misc · Dev Tools**. Em *Health*: Medical Changes, **Revert Med Changes**, Stim Changes, Food Changes, Hazard Zones. Em *Movement*: Movement Changes, Fall Damage Changes, Weight Limits Changes, **Enable Stances**.

---

## 8. Bugs reportados e respostas do dev

| # | Reporte | Quem / data | Resposta do Fontaine | Status |
|---|---|---|---|---|
| 1 | **Heartbeat SFX da adrenalina** entra em **loop infinito** (dispara desidratado e não para mesmo após a adrenalina passar) | Twank bwattewy · 22/04 | "Preciso corrigir o **looping** na próxima versão" | **Reconhecido, fix planejado** |
| 2 | **Traders quebram** (retratos brancos/inclicáveis) ao sair da raid | BoostedStache · 30/04→16/05 | "User error"; causado por **mods de trader estranhos** (Artem? avatares cheat) — **BoostedStache isolou até as quests do Gunsmith**; a mod de GS do Realism pode **corromper o estado da quest**. "Não será relevante no 4.x, server é diferente" | **Workaround/contornado**; irrelevante p/ 4.0 |
| 3 | **Placa traseira destruída, dianteira intacta** após morrer | gusy_gusy · 15/05 (att-22) | **Não é bug**: placas não cobrem o corpo todo; tiro **contornou a dianteira, penou o corpo e bateu na traseira**; pose da BSG expõe o lado | **Working as intended** |
| 4 | **PMCs com rig/capacete armados sem placas/soft armor** | Twank bwattewy · 20/04 | Gear do **Artem** no pool; **remover os IDs** do JSON | **Config/dados** |
| 5 | **PTT + Interactable Exfils** conflita com Realism (extract conditions) | adishee · 10/03 | Realism **altera reqs de exfil de Factory** se o gás não estiver desligado | **Interação esperada** |
| 6 | **Recoil Rework + Realism** faz "funky stuff" | Twank/Kojimbooo · 04–05/04 | Ajustar ambos os configs ou só o **advanced recoil** do Realism | **Workaround** |

---

## 9. Performance

- O rework de stances foi motivado **em parte por performance**: o sistema antigo "brigava" com a animação da BSG e causava **stutter em drops de frame**; o novo é **FPS-independente** e tem blending mais suave (28/04).
- **Shotguns**: limite de 8 pellets foi imposto para evitar **perda de performance** da BSG com muitos impactos simultâneos (18/04).
- Discussão recorrente de que o **EFT base degradou** (texturas/áudio) e roda pior que o GZW — contexto, não atribuível ao mod.

---

## 10. Compatibilidade (o que combina / conflita)

| Categoria | Situação com Realism |
|---|---|
| **SAIN** | **Complementar** — SAIN faz o **comportamento/combate** dos bots; Realism faz **gear/loadouts** e as mecânicas do **player**. Não competem. |
| **APBS / Adi Bots / Phobos** | Mexem em **gear/spawn/comportamento** de bots; coexistem (Realism cede esse domínio). |
| **PTT (Path to Tarkov)** | **Conflito conhecido** envolvendo **Interactable Exfils** e os reqs de exfil de Factory (gas). |
| **Recoil Rework** | "Funky" junto; precisa ajuste de config (são sistemas redundantes). |
| **SVM** | **Não** edita features do Realism; serve para skill multipliers, economia parcial, etc. |
| **Fika** | Sync Realism×Fika foi suspeito de bugs no passado; **no 4.0 o Fika sync vira addon de stances** e "deve funcionar muito melhor". |
| **WTT / Artem / Painter / Badger (Epic's AIO)** | Usados em conjunto; **Artem** é o suspeito recorrente do bug de traders e de PMCs sem placa. |
| **Time/zone mods (OneTimeZone)** | Podem interferir na detecção de **noite** para loadouts de bots. |
| **FOV Fix** (do próprio Fontaine) | Combina; melhora o "feel" junto com stances. |

> Regra de ouro repetida pelo Fontaine: **para troubleshoot, rode só o Realism** e vá readicionando mods. "Realism não causa isso sozinho; precisa de outros mods para dar problema."

---

## 11. Roadmap (do que foi dito no período)

- **Curto prazo:** terminar o **rework de stances** (reimplementar o 3.11) → **test builds públicos** → lançar **stances standalone + CommonLib** como primeiros módulos 4.0.
- **Depois (um de cada vez):** **balística** (mais armor boxes, órgãos, fim do frag system da BSG), **médico** (blood loss), **traders/economia**, **bots**, **hazards**, **anexos**, **recoil**.
- **Stances:** mais camadas procedurais; heavy weapons com **dreno de stamina + sway** em vez de forçar low-ready; **ADS speed** reformulado.
- **Fika:** sync como **addon de stances**.
- **Sem release único; sem data.** "It'll be done when it's done" (pinned no mod page + Patreon).
- **Visão de versões:** Fontaine **não vê valor** em um hipotético "SPT 5.0" (paridade com EFT 1.0) se custar os client mods; prefere **reverse-engineer** de conteúdo/features. O **4.1** (último client antes do 1.0, com assembly **deobfuscado**) "não deve ser disruptivo" para client mods — ele aprendeu a **mexer o mínimo possível no server**.

---

## 12. Pontos legais e curiosos

- **Bordões/memes**: **"+2 weeks / +2 months"** (toda pressão por data adiciona tempo), **"Realism tomorrow / it's Thursday somewhere"**, **"fontainisyphus / Jobtaine"** (Fontaine como Sísifo/Jó do modding), o recorrente **"secret Fontaine sex mod"** (193 bytes, att-03).
- **Fontaine trollando**: "o mod simula cada partícula de pólvora não queimada considerando temperatura ambiente e altitude" — depois revela que é só o efeito líquido de mais flash em canos curtos (20/03). E **"I'm inside your walls"** quando alguém pensa numa feature que já existe.
- **Moderação**: **DrakiaXYZ** (dev core do SPT) aplica a regra **"não assedie autores por updates"**; há uma **macro/tag oficial** ("mod update — Ran by DrakiaXYZ"). Um usuário (**Kushlungs**) foi **banido** por slurs após o pestering.
- **GZW como espelho**: metade do período é a comunidade comparando Tarkov ao **Gray Zone Warfare** (movimento, áudio, blood system) — e usando isso como **fonte de ideias** para o médico do Realism.
- **"BSG copiou o Realism"**: Kojimbooo acha que a BSG copiou o **chamber check rápido**; Fontaine **discorda** ("as armas novas deles já têm anims melhores").
- **CommonLib (Groovey)**: **salvage zones** (LeaveItemAtLocation que entrega item, consumo opcional), uso do prop **`secretQuest`** (inutilizado pelo SPT) para esconder quests travadas, **group positions** (RNG escolhe a zona), e emulação das mecânicas do EFT 1.0 ("quest ao pegar item / ao entrar em zona").

---

## 13. Glossário

| Termo | Significado |
|---|---|
| **SPTRM / RM** | (SPT) Realism Mod |
| **Stances** | Sistema de posturas de arma (high/low/active-ready) do Realism; rework com animation curves |
| **AAS** | Active Aim System — hipfire/active-aim das stances (keybind no F12) |
| **CommonLib** | Biblioteca compartilhada (do Groovey e, no 4.0, do próprio Realism) com utilidades para vários mods |
| **Hazard zones** | Zonas de gás/névoa tóxica/radiação do Realism, com quest line |
| **ICO** | Inertia/recoil overhaul oficial da BSG (alvo de crítica) |
| **GZW** | Gray Zone Warfare (jogo concorrente, usado como referência) |
| **PTT** | (Trap's) Path to Tarkov — mod de extração/trânsito entre mapas |
| **Interactable Exfils** | API de exfils interativos (interage com PTT e com os reqs de exfil) |
| **SVM** | Server Value Modifier (config geral; Greed.exe) |
| **APBS / Phobos / SAIN / Adi Bots** | Mods de bots (spawn/gear/IA/comportamento) |
| **WTT** | "We The Tarkov"? — equipe/colab de gear; pool padrão de bots do Realism _(inferência sobre o nome)_ |
| **4.1 / 1.0** | 4.1 = último client SPT antes do EFT 1.0 (deobfuscado); 1.0 = release oficial do EFT |

---

## 14. Cheat-sheet — como entrar na discussão sabendo de tudo

1. **Não pergunte "when update".** É o tabu nº 1 do canal (moderação ativa, macro do DrakiaXYZ, um ban). A resposta canônica é **"it'll be done when it's done"** + Patreon/mod page.
2. **Saiba que não haverá "Realism 4.0" monolítico.** São **mods standalone**, um de cada vez, começando por **stances + CommonLib**.
3. **Stances** é o assunto quente: rework do zero, **animation curves**, integração com a animação procedural da BSG, FPS-independente. Heavy weapons mudam de "forçar low-ready" para **stamina drain + sway**.
4. **Médico** caminha para **blood loss** (inspiração parcial: GZW). **Sem profile wipe.** Uninstall = "Revert Med Changes" + "Revert HP" na GUI.
5. **Balística**: zonas de dano variáveis, placas não cobrem tudo (a bala pode contornar), gap meta×stock estreito, cano curto = menos dano; **frag system da BSG será substituído**.
6. **Troubleshooting** (regra do Fontaine): **rode só o Realism** e readicione mods. Suspeitos comuns: **Artem** (traders/PMCs sem placa), mods de tempo (loadout noturno), **Recoil Rework** (redundância).
7. **Compatibilidade mental**: SAIN = combate/IA; Realism = gear de bot + mecânicas do player; **eles não competem**. Fika sync vira **addon de stances** no 4.0.
8. **Tom**: humor seco, memes de "+2 weeks" e "Thursday". Elogie o trabalho, traga **feedback com contexto** (não só "stat X é fraca") — Fontaine responde bem a isso e mal a stats fora de contexto.

---

## Fontes

- **Transcrição:** [`01-transcricao.md`](./01-transcricao.md) (1.132 mensagens, 07/03→04/06/2026, GMT-3).
- **Anexos:** [`assets/`](./assets/) — 18 imagens/gifs + 6 vídeos (config GUI, stance reworks, health panel, armor stats, changelogs, memes).
- **Forge:** <https://forge.sp-tarkov.com/mod/416/spt-realism-mod> (v1.6.4, SPT 3.11.4, CC BY-NC-ND 4.0, ~445k downloads).
- **Hub:** <https://hub.sp-tarkov.com/files/file/606-spt-realism-mod/>
- **GitHub (Fontaine = `space-commits`):** [perfil](https://github.com/space-commits) · [Client](https://github.com/space-commits/SPT-Realism-Mod-Client) · [Server](https://github.com/space-commits/SPT-Realism-Mod-Server) · [GUI](https://github.com/space-commits/SPT-Realism-GUI) · [Combat-Stances](https://github.com/space-commits/SPT-Combat-Stances) · [Recoil-Overhaul-Standalone](https://github.com/space-commits/SPT-Realism-Recoil-Overhaul-Standalone) · [Malfunctions](https://github.com/space-commits/SPT-Realism-Malfunctions) · [FOV-Fix](https://github.com/space-commits/SPT-FOV-Fix).

## Histórico

| Data | Autor | Descrição |
|---|---|---|
| 2026-06-05 | Guilherme | feat(inventory): harden editing tool and drop UltraFika mod #0 |
