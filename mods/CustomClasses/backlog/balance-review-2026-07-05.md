# Balance Review — CustomClasses

> **Data:** 2026-07-05<br>
> **Status:** 🟢 Ondas 0, 1 e 2 APLICADAS (2026-07-10/11) — restam só as ESTRUTURAIS (B12·B13·B14·B19); decisões marcadas no §2<br>
> **Pedido:** "o que adicionar/remover/editar para deixar tudo mais equilibrado?"<br>
> **Revisões:** /g-review-content ×2 (2026-07-05) — rodada 1: 9 itens (B14–B18, números validados); rodada 2: 8 itens (ondas de aplicação, B14 concreto, antes→depois, Anexo C corrigido, B19)<br>

---

## 1. Como ler (30 segundos)

A análise mede **3 camadas** independentes de poder de uma classe:

| Camada | O que é | Métrica | Alvo |
|---|---|---|---|
| **Skills iniciais** | Níveis de skill que a classe já nasce tendo | `custo` = Σ nível×peso | 28–32 |
| **XP de skills** | O quanto a classe treina mais rápido/devagar | `netMult` = Σ (fator−1)×peso | todas em ~13–17 |
| **Perks/Drawbacks** | Efeitos de combate ligados (patches) | sem número — avaliação qualitativa | cada perk tem preço/counterplay |

Vocabulário:
- **vivo** = efeito implementado e rodando · **pendente ⏳** = aparece esmaecido no painel, não faz nada.
- **debuff "grátis"** = penalidade de XP numa skill que a classe **nem treina** → parece custo, não custa nada.
- **card × F12** = o card do painel CLASS mostra um valor fixo do catálogo; o efeito real usa o default do F12 — quando divergem, o jogador é enganado.
- **host-only** = efeito com gate no player local; em coop FIKA, **não afeta a percepção dos bots quando quem joga é um CLIENTE** (bots vivem no processo do host) — ver B14.

Fontes: `scripts/class-balance-snapshot.mjs` (fórmula confirmada: `custo = Σ nível×peso`, linear) · `PerksCatalog.cs` · `PerksConfig.cs` · 058 (mastery). Números crus no **Anexo A**; stacking no **Anexo C**.

---

## 2. Painel de decisões

Cada linha é UMA mudança atômica. Marque a coluna **Decisão** (⬜ pendente · ✅ aprovada · ❌ rejeitada) e **Aplicada** quando commitada.

| # | Prio | Classe | Mudança proposta | Esforço | Depende de | Decisão | Aplicada |
|---|---|---|---|---|---|---|---|
| B1 | 🔴 | Médico | `Shaky Hands — Enabled` default **OFF** até os perks do Médico existirem | 1 linha | — | ✅ | ✅ 2026-07-10 |
| B2 | 🔴 | Furtivo | Ghost Step: F12 `0.40 → 0.70` — **exatamente** o que o card anuncia (−30%) · no mesmo commit: corrigir o **label** do card "todo o ruído do player" → "ruído de movimento/ações" (**tiros ficam de fora** dos 3 pipelines — rótulo atual mente) | 2 linhas | ideal c/ B4 | ✅ | ✅ 2026-07-10 |
| B3 | 🔴 | Caçador | Iron Lungs: F12 `0.50 → 0.667` — dreno ×0.667 = **+50% de duração exatos** (o card) | 1 linha | ideal c/ B4 | ✅ | ✅ 2026-07-10 |
| B4 | 🔴 | (UI) | Cards do painel CLASS lerem o **valor vivo do F12** (precedente: footer 060) — pré-requisito de VERDADE para qualquer recalibrada futura; por isso entra na Onda 0 | médio | — | ✅ | ✅ 2026-07-10 |
| B5 | 🟠 | Tanque | Custo inicial (35.28 → alvo): **opção A** `HeavyVests 3→2` (−3.75 → **31.53**, 1 mexida; a classe treina HeavyVests ×2, recupera rápido) · opção B `Shotgun 5→4` + `Vitality 5→4` (−4.17 → **31.11**) | jsonc | — | ✅ | ✅ 2026-07-11 |
| B6 | 🟠 | Tanque | **Couraça condicional**: dano ×0.85 só com colete pesado (classe 4+) equipado; sem colete ×1.0. Alternativa simples: `0.85 → 0.88`. Viável: `BulwarkPatch` é Prefix em `Player.ApplyDamageInfo` com acesso ao `__instance` (equipment). **Onda 2** (ver ondas abaixo) | patch | Onda 1 testada | ✅ | ✅ 2026-07-11 |
| B7 | 🟠 | Furtivo | Melee `×5 → ×3.5` (1 linha no default) · variante: manter ×5 só em golpe por trás (checar ângulo do `damageInfo` — médio). **Onda 2** | 1 linha / médio | Onda 1 testada | ✅ | ✅ 2026-07-11 |
| B8 | 🟡 | Médico | Subir XP (⚠ **Health já é ×2 — não tocar**): `Vitality ×1→×1.5` (+0.84) + `FirstAid ×2.5→×3` (+0.47) + `Charisma ×1→×1.5` (+0.20) → netMult 11.31 → **≈12.8** (borda da faixa) | jsonc | — | ✅ | ✅ 2026-07-11 |
| B9 | 🟡 | Saqueador | Subir XP (pacote — `Search ×3` sozinho rende só +0.43): `Search ×2→×3` (+0.43) + `Endurance ×1.5→×2` (+0.50) + `Attention ×1.5→×2` (+0.30) + `Charisma ×1.5→×2` (+0.20) → 11.56 → **≈13.0**; sinergia c/ 061 | jsonc | — | ✅ | ✅ 2026-07-11 |
| B10 | 🟡 | Fuzileiro | Cortar buffs derived `BearRawpower ×1.5` + `UsecNegotiations ×1.5` (−2.50) → 18.63 → **16.13** | jsonc | — | ✅ | ✅ 2026-07-11 |
| B11 | 🟡 | Furtivo | Trocar os 5 debuffs "grátis" por debuffs que mordem (ex.: `Endurance ×0.8`, `Throwing ×0.8`) mantendo netMult ≈14 | jsonc | — | ✅ | ✅ 2026-07-11 |
| B12 | 🟡 | Médico | Implementar os perks (050 — perna transpiler: Rapid Care/Swift Surgeon) | item 050 | — | ⬜ | ⬜ |
| B13 | ℹ️ | Tanque | RN-03 (mastery por classe): Tanque **×1.5, não ×2** — não amplificar quem já está no teto | decisão | B15 | ⬜ | ⬜ |
| B14 | 🟠 | (coop) | **Som host-side para remotos** — implementação DIRETA, sem protocolo novo: bots ouvem no HOST e o host já tem o mapa nickname→classe (rota 057) → `AiSoundPatch`/`SainSoundPatch` resolvem a classe do player REMOTO (`ClassIdentities.TryResolve(Profile.Nickname)`) e aplicam o multiplicador DELE. Única parte que exigiria sync real: o rolloff audível (`method_67`) para OUTROS humanos — fica de fora. Enquanto não aplicado: som = host-only nas avaliações | pequeno/médio | rota 057 (já existe) | ✅ | ✅ 2026-07-11 |
| B15 | 🟡 | Tanque/Fuzileiro | **Piso COMBINADO de recuo**: Bunker ×0.85 × mastery 51 ≈ **×0.68**; Adrenalina ×0.7 × mastery ≈ **×0.56** na janela. Mastery tem piso próprio (0.5 — inalcançável no cap 51), o PRODUTO não tem → definir piso combinado (ex.: 0.6, que só morde a janela de Adrenalina). Decidir ANTES do B13 | pequeno | — | ✅ | ✅ 2026-07-11 |
| B16 | 🟡 | Tanque | Tireless Arms `0 → 0.2–0.25`: imunidade ABSOLUTA é outlier — o especialista em mira (Caçador) tem ×0.65; fadiga 4–5× mais lenta preserva a fantasia sem imunidade. **Onda 2** | trivial | decidir c/ B6 | ✅ | ✅ 2026-07-11 |
| B17 | 🟡 | Médico | **Perk vivo hoje**: "Metabolismo Eficiente" — fome/sede `×0.85` reutilizando o lever do Heavy Frame (`ClassCombatHealthPatches` branch por classe) + card no catálogo | pequeno | — | ✅ | ✅ 2026-07-10 |
| B18 | 🟡 | Tanque | Baixar XP (§4.2 não tocava o netMult do Tanque): `Shotgun ×3→×2.5` (−1.25) + `LightVests ×2→×1.75` (−0.94) → 19.19 → **≈17.0** (teto da faixa) | jsonc | — | ✅ | ✅ 2026-07-11 |
| B19 | 🔵 | (UI) | Cards **Flag** exibirem a magnitude quando houver F12 numérico: Silent Looter (real ×0.4) e Overladen (real ×1.5) hoje não mostram número — coerência com a transparência do B4 | pequeno | B4 | ⬜ | ⬜ |

### Ondas de aplicação (anti-overshooting)

O Tanque recebe **5 nerfs** neste painel (B5, B6, B16, B18 + o ruído já aplicado em 2026-07-05) — aplicados de uma vez, o risco clássico é overshooting (Tanque vira o novo Médico). Sequência com re-teste entre ondas:

1. **Onda 0 — justiça imediata (sem risco):** B1 · B17 · B2/B3 (alinhamento + label) · **B4** (antes de qualquer recalibrada futura — sem ele, B2/B3 voltam a mentir na primeira calibrada).
2. **Onda 1 — orçamentos (.jsonc):** B5 · B8 · B9 · B10 · B11 · B18 — rodar `node scripts/class-balance-snapshot.mjs` antes/depois e conferir com a tabela projetada do §4.2.
3. **Gate in-game** — raids coop de teste; sensação de TTK/progressão por classe.
4. **Onda 2 — combate:** B6 · B16 · B7 · B15 (só se a Onda 1 não tiver corrigido a percepção sozinha).
5. **Estruturais (qualquer momento):** B14 (host-side) · B12 (item 050) · B19 (com B4) · B13 (com RN-03, após B15).

---

## 3. Diagnóstico por classe

### 🛡️ Tanque — *acima do orçamento em TUDO* → B5, B6, B13, B15, B16, B18 (em ondas!)

| Camada | Situação | Veredito |
|---|---|---|
| Skills iniciais | custo **35.3** — única classe fora do alvo 28–32 | ✗ estourado |
| XP | netMult **19.2** — o maior do mod (nenhuma linha original tocava isso → B18) | ✗ teto |
| Perks vivos | dano recebido ×0.85 **incondicional** + carga +30% + pacote pesado completo; recuo pesado empilha com mastery (**×0.68 combinado** — Anexo C) e Tireless Arms é imunidade absoluta (×0) | ✗ pacote mais forte |
| Drawbacks vivos | veloc ×0.9 · fome/sede ×1.3 · **ruído +30% (novo, 2026-07-05 — host-only vs bots, ver B14)** | ajuda, não fecha a conta |

O ruído novo foi na direção certa (mas só morde a IA quando o Tanque é o host — B14). A B6 (Couraça só com colete pesado) segue sendo a proposta mais elegante: temática, counterável, e casa com o HeavyVests ×2 que a classe já treina. **Não aplicar B5/B6/B16/B18 de uma vez** — ver Ondas no §2.

### 🩺 Médico de Combate — *hoje é estritamente NEGATIVO* → B1, B8, B12, B17

| Camada | Situação | Veredito |
|---|---|---|
| Skills iniciais | custo 31.0 | ✓ |
| XP | netMult **11.3 — o menor** (obs.: `Health ×2` e `FirstAid ×2.5` já existem; o gap real é `Vitality ×1`) | ✗ piso |
| Perks vivos | **NENHUM** (os 3 são pendentes ⏳) — mas B17 entrega 1 perk vivo barato (fome/sede ×0.85, lever já existe) | ✗ |
| Drawbacks vivos | recuo ×1.25 **ATIVO** (integral fora de SMG/LMG/GL mesmo no endgame — Anexo C) | ✗ só o ônus roda |

Quem joga de Médico hoje só perde. B1 (1 linha) corrige a injustiça; B17 dá o primeiro perk vivo sem esperar a perna transpiler (B12).

### 👻 Furtivo — *mais forte do que anuncia* → B2, B7, B11

| Camada | Situação | Veredito |
|---|---|---|
| Skills iniciais | custo 29.7 | ✓ |
| XP | netMult 14.4 plausível, **mas 5 debuffs são "grátis"** (StressResistance, BearRawpower, UsecNegotiations, Vitality, Metabolism) | ⚠ custo inflado |
| Perks vivos | ruído **×0.40 real** (card anuncia ×0.70 → B2 = 0.70 exato; label também mente — tiros ficam de fora) · melee **×5** · +10% veloc c/ melee | ⚠ 2 outliers |
| Drawbacks vivos | aim punch ×1.5 | ✓ morde |

O combo real é mais forte que o exibido: −60% de ruído (anunciado −30%) + kill garantido de ×5. B2 alinha exato; B7 tira o one-shot trivial (Onda 2 — são 2 nerfs na mesma classe). Coop: o ruído reduzido também é host-only vs bots (B14).

### 🔫 Fuzileiro — *ok em combate, XP gordo* → B10, B15

| Camada | Situação | Veredito |
|---|---|---|
| Skills iniciais | custo 30.5 | ✓ |
| XP | netMult **18.6** — 2º maior | ⚠ alto |
| Perks vivos | flinch ×0.5 · anti-jam ×0.5 · Adrenalina (janela) — que empilha com mastery: **×0.56 combinado** no pico (Anexo C → B15) | ✓ condicional, bem desenhado |
| Drawbacks vivos | ruído +30% (host-only vs bots — B14) | ✓ |

O pacote de combate é o modelo a seguir. B10 corta o XP pra 16.13; B15 põe piso no produto recuo×mastery.

### 🎯 Caçador — *equilibrado; 1 card mente* → B3

| Camada | Situação | Veredito |
|---|---|---|
| Skills iniciais | custo 31.4 | ✓ |
| XP | netMult 14.5 (≈ média) | ✓ |
| Perks vivos | ADS −15% · fôlego **dreno ×0.50 real ≈ ×2 duração** (card anuncia +50% → B3 = 0.667 exato) · braço mirando ×0.65 · Mira Serena ⏳ | ⚠ 1 card mente |
| Drawbacks vivos | veloc mirando −15% (só durante ADS — leve) | ✓ aceitável p/ identidade sniper |

Classe de referência. Só alinhar o Iron Lungs (B3). Se ficar forte demais quando Mira Serena nascer, endurecer o Rooted pra 0.75 (não agora).

### 🎒 Saqueador — *utilidade pura, XP no piso* → B9 (+ item 061)

| Camada | Situação | Veredito |
|---|---|---|
| Skills iniciais | custo 30.5 | ✓ |
| XP | netMult 11.6 — 2º menor (`Search ×3` sozinho só rende +0.43 → B9 é PACOTE) | ⚠ baixo |
| Perks vivos | saque silencioso ×0.4 (host-only vs bots no pipeline SAIN — B14) · carga +30% · Quick Hands ⏳ (**061**) | ✓ identidade looter |
| Drawbacks vivos | inércia ∝ peso (×1.5) | ✓ morde de verdade |

Classe de economia por design. B9 (pacote +1.43 → ≈13.0) + 061 completam o kit.

### 🏃 Peladão (Naked) — isenta

Sem skills iniciais nem multiplicadores (`noBaseline`) — o snapshot a exclui do balance por design. Sem ação.

---

## 4. Temas transversais

1. **Transparência = balance percebido (B2/B3/B4/B19).** Dois cards mentem no número, um no rótulo (Ghost Step promete "todo o ruído"; tiros ficam de fora). B2/B3 alinham nos valores EXATOS anunciados (0.70 / 0.667); B4 (cards lendo o F12 vivo) é pré-requisito de verdade permanente — por isso está na **Onda 0**, antes de qualquer recalibrada.
2. **Amplitude do netMult: 7.9 → ≈4.2.** Estado projetado com a Onda 1 aplicada:

   | Classe | netMult hoje | → projetado | Mudança |
   |---|---|---|---|
   | Médico | 11.31 | **≈12.8** | B8 |
   | Saqueador | 11.56 | **≈13.0** | B9 |
   | Furtivo | 14.35 | 14.35 (≈14 c/ B11) | B11 neutro |
   | Caçador | 14.45 | 14.45 | — |
   | Fuzileiro | 18.63 | **16.13** | B10 |
   | Tanque | 19.19 | **≈17.0** | B18 (custo: B5 → 31.1–31.5) |

   Regenerar via `node scripts/class-balance-snapshot.mjs` após cada onda e conferir contra esta tabela.
3. **Coop muda o veredito do som (B14).** Os 3 pipelines têm gate no player local e os bots vivem no host — pra um CLIENTE Fika, Ghost Step/Loud Operator/ruído do Tanque não afetam a IA. O fix é direto (host resolve a classe do remoto via mapa da rota 057 — sem protocolo novo); até lá, tratar identidade sonora como **host-only**.
4. **Mastery (058) não é neutra (B13/B15).** A camada empilha multiplicativamente com os perks de recuo (Anexo C) **nas 3 categorias com maestria** (SMG/LMG/Lançador); o RN-03 deve considerar o PRODUTO, não o fator isolado. B15 decide-se antes de B13.

---

## Anexo A — números crus (snapshot 2026-07-05)

| Classe | custo (alvo 28–32) | netMult | netMult plausível | buffs/debuffs | flags |
|---|---|---|---|---|---|
| Hunter | 31.40 ✓ | 14.45 | 14.45 | 18/3 | — |
| Stealth | 29.74 ✓ | 12.43 | 14.35 | 11/7 | 5 debuffs grátis |
| Rifleman | 30.51 ✓ | 18.28 | 18.63 | 21/14 | 1 debuff grátis (Shadowconnections) |
| Combat Medic | 30.95 ✓ | 11.31 | 11.31 | 15/7 | — |
| Naked | — | — | — | — | ISENTA (sem skills/multiplicadores) |
| Scavenger | 30.45 ✓ | 11.56 | 11.56 | 19/2 | — |
| **Tank** | **35.28 ✗** | **18.89** | **19.19** | 12/20 | custo fora do alvo · 1 debuff grátis |

netMult plausível: min 11.31 · máx 19.19 · média 14.92 · **amplitude 7.88**.
Fórmula confirmada no script: `custo = Σ nível_inicial × peso(skill)` (linear) · `netMult = Σ (fator−1) × peso`.

Divergências card × F12 (auditoria dos efeitos numéricos — 2 números + 1 rótulo):

| Efeito | Card (catálogo) | F12/real | Efeito prático | Fix |
|---|---|---|---|---|
| Ghost Step (Furtivo) | ×0.70 (−30% ruído) | **0.40** | −60% ruído | B2: F12 → 0.70 |
| Ghost Step (rótulo) | "todo o ruído do player" | pipelines cobrem movimento/passo/ações — **tiros ficam de fora** | promete mais do que faz | B2: label → "ruído de movimento/ações" |
| Iron Lungs (Caçador) | +50% duração de fôlego | dreno **×0.50** | ≈ ×2 duração | B3: F12 → 0.667 |

## Anexo B — perks/drawbacks vivos por classe (referência rápida)

| Classe | Perks 🟢 vivos | Pendentes ⏳ | Drawbacks 🔴 vivos |
|---|---|---|---|
| Tanque | carga +30% · dano ×0.85 · recuo pesado ×0.85 · ergo pesado ×1.15 · GL sem penalidade · braço não cansa (×0) | — | veloc ×0.9 · fome/sede ×1.3 · ruído +30%¹ |
| Fuzileiro | flinch ×0.5 · anti-jam ×0.5 · Adrenalina (recuo ×0.7 · recarga ×0.8 · ADS ×0.8, janela) | — | ruído +30%¹ |
| Caçador | ADS −15% · fôlego dreno ×0.667 · braço mirando ×0.65 · **ruído ×0.80 (Stalker — NOVO 2026-07-11)**¹ | Mira Serena | veloc mirando −15% |
| Furtivo | ruído ×0.40¹ · melee ×5 · +10% veloc c/ melee | — | aim punch ×1.5 |
| Saqueador | saque silencioso ×0.4¹ · carga +30% | Quick Hands (061) | inércia ∝ peso |
| Médico | — (B17 propõe fome/sede ×0.85 vivo) | Cuidado Rápido · Cirurgião Ágil · Cirurgia em Movimento | recuo ×1.25 |

¹ Efeito de som: **host-only vs bots** em coop (B14).

## Anexo C — stacking de recuo (por que B15 existe)

Multiplicadores de recuo se COMBINAM por produto. A mastery (058) existe **só para SMG/LMG/Lançador**
(`WeaponMastery.SkillForHeld`) e tem piso próprio 0.5 — que **nunca é atingido** no cap de nível 51
(1 − 0.004×51 = 0.796; o piso exigiria nível 125). O PRODUTO com os perks não tem piso nenhum:

| Cenário | Fatores | Produto |
|---|---|---|
| Tanque, LMG, maestria nível 51 | Bunker ×0.85 · mastery ×0.796 | **×0.68** |
| Fuzileiro, SMG, janela de Adrenalina, maestria 51 | Adrenalina ×0.70 · mastery ×0.796 | **×0.56** |
| Médico **com SMG**, maestria 51 | Shaky Hands ×1.25 · mastery ×0.796 | **×0.995** |

Nota (efeito colateral não-óbvio): com SMG/LMG/GL no endgame, a mastery **anula o drawback de recuo do
Médico** — mas o Shaky Hands segue **integral (×1.25)** em AR/DMR/pistola/etc., que não têm maestria.
O B15 (piso combinado) deve considerar tanto o CHÃO dos buffs quanto esse TETO de anulação de drawbacks
nas 3 categorias.

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-05 | Guilherme/Claude | Análise criada. |
| 2026-07-05 | Guilherme/Claude | Reorganizado (UX): painel de decisões rastreável, diagnóstico por classe, glossário, anexos. |
| 2026-07-05 | Guilherme/Claude | Review rodada 1 endereçada: B8/B9 corrigidos, B5 exato, B2/B3 exatos, novos B14–B18, Peladão no Anexo A. |
| 2026-07-05 | Guilherme/Claude | Review rodada 2 endereçada: Anexo C corrigido (mastery só SMG/LMG/GL; piso 0.5 inalcançável; "anula drawback" qualificado), dependência circular B13↔B15 desfeita, **Ondas de aplicação** (B4 na Onda 0; nerfs do Tanque divididos c/ gate entre ondas), B14 reescrito como implementação host-side direta, tabela netMult antes→depois no §4.2, label do Ghost Step no B2, novo B19 (magnitude nos cards Flag). |
| 2026-07-10 | Guilherme/Claude | **Onda 0 aplicada** (aprovada pelo usuário): B1 (Shaky Hands default OFF), B2 (Ghost Step F12 0.40→0.70 + label "ruído de movimento/ações"), B3 (Iron Lungs F12 0.50→0.667), B4 (cards do painel CLASS lendo o F12 vivo via `PerkLine.Live` — 22 linhas fiadas, Iron Lungs=1/dreno e Pack Mule=1+bônus transformados), B17 (Metabolismo Eficiente — 1º perk vivo do Médico, fome/sede ×0.85 no lever do Heavy Frame). Build limpo + review adversarial dos mapeamentos (0 problemas). |
| 2026-07-11 | Guilherme/Claude | ⚠️ **Conteúdo novo FORA do board** (pedido do usuário): perk **Stalker/Espreita** no Caçador — ruído de movimento ×0.80 (−20%), irmão mais fraco do Ghost Step (−30%). Anexo B atualizado, mas **a análise deste board — que classificava o Caçador como "equilibrado, classe de referência" — NÃO considerou este perk**: ele ganhou poder sem contrapartida. Reavaliar o Caçador antes/depois das Ondas 1–2 (alavanca de custo já sugerida no §3: endurecer o Rooted p/ 0.75). Idem item 061 (Quick Hands) — perk que era ⏳ e virou vivo no Saqueador. |
