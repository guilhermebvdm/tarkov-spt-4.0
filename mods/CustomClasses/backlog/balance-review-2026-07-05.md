# Balance Review — CustomClasses

> **Data:** 2026-07-05<br>
> **Status:** 🔵 Análise — **nenhuma mudança aplicada**; decisões são marcadas no §2<br>
> **Pedido:** "o que adicionar/remover/editar para deixar tudo mais equilibrado?"<br>

---

## 1. Como ler (30 segundos)

A análise mede **3 camadas** independentes de poder de uma classe:

| Camada | O que é | Métrica | Alvo |
|---|---|---|---|
| **Skills iniciais** | Níveis de skill que a classe já nasce tendo | `custo` = Σ nível×peso | 28–32 |
| **XP de skills** | O quanto a classe treina mais rápido/devagar | `netMult` = Σ (fator−1)×peso | todas próximas (hoje: 11↔19) |
| **Perks/Drawbacks** | Efeitos de combate ligados (patches) | sem número — avaliação qualitativa | cada perk tem preço/counterplay |

Vocabulário:
- **vivo** = efeito implementado e rodando · **pendente ⏳** = aparece esmaecido no painel, não faz nada.
- **debuff "grátis"** = penalidade de XP numa skill que a classe **nem treina** → parece custo, não custa nada.
- **card × F12** = o card do painel CLASS mostra um valor fixo do catálogo; o efeito real usa o default do F12 — quando divergem, o jogador é enganado.

Fontes: `scripts/class-balance-snapshot.mjs` · `PerksCatalog.cs` · `PerksConfig.cs` · 058 (mastery). Números crus no **Anexo A**.

---

## 2. Painel de decisões

Cada linha é UMA mudança atômica. Marque a coluna **Decisão** (⬜ pendente · ✅ aprovada · ❌ rejeitada) e **Aplicada** quando commitada.

| # | Prio | Classe | Mudança proposta | Esforço | Decisão | Aplicada |
|---|---|---|---|---|---|---|
| B1 | 🔴 | Médico | `Shaky Hands — Enabled` default **OFF** até os perks do Médico existirem | 1 linha | ⬜ | ⬜ |
| B2 | 🔴 | Furtivo | Ghost Step: alinhar real ao anunciado — F12 `0.40 → 0.65` (card diz −30%, aplica −60%) | 1 linha | ⬜ | ⬜ |
| B3 | 🔴 | Caçador | Iron Lungs: alinhar real ao anunciado — F12 `0.50 → 0.65` (card diz +50%, aplica ≈×2) | 1 linha | ⬜ | ⬜ |
| B4 | 🔴 | (UI) | Cards do painel CLASS lerem o **valor vivo do F12** (precedente: footer 060) — nunca mais mentir | médio | ⬜ | ⬜ |
| B5 | 🟠 | Tanque | Skills iniciais: `Shotgun 5→4` + `Vitality 5→4` → custo 35.3 → ≈31.5 (volta ao alvo) | jsonc | ⬜ | ⬜ |
| B6 | 🟠 | Tanque | **Couraça condicional**: dano ×0.85 só com colete pesado (classe 4+); sem colete ×1.0. Alternativa simples: `0.85 → 0.88` | patch | ⬜ | ⬜ |
| B7 | 🟠 | Furtivo | Melee `×5 → ×3.5` (ou manter ×5 só em golpe por trás, se o lever distinguir ângulo) | 1 linha / patch | ⬜ | ⬜ |
| B8 | 🟡 | Médico | Subir XP: ex. `Vitality ×1.5` e/ou `Health ×1.5` (tirar do piso do netMult) | jsonc | ⬜ | ⬜ |
| B9 | 🟡 | Saqueador | XP `Search ×2 → ×3` (sinergia com Quick Hands, item 061) | jsonc | ⬜ | ⬜ |
| B10 | 🟡 | Fuzileiro | Cortar buffs derived `BearRawpower ×1.5` + `UsecNegotiations ×1.5` (netMult 18.6 → ≈16.5) | jsonc | ⬜ | ⬜ |
| B11 | 🟡 | Furtivo | Trocar os 5 debuffs "grátis" por debuffs que mordem (ex.: `Endurance ×0.8`, `Throwing ×0.8`) | jsonc | ⬜ | ⬜ |
| B12 | 🟡 | Médico | Implementar os perks (050 — perna transpiler: Rapid Care/Swift Surgeon) | item 050 | ⬜ | ⬜ |
| B13 | ℹ️ | Tanque | RN-03 (mastery por classe): Tanque **×1.5, não ×2** — não amplificar quem já está no teto | decisão | ⬜ | ⬜ |

**Ordem sugerida:** B1 → B2/B3 (quick wins, 3 linhas) → B5/B6 (Tanque) → B7 → B8–B11 (passada nos .jsonc, rodar o snapshot antes/depois) → B12 → B13 junto do RN-03.

---

## 3. Diagnóstico por classe

### 🛡️ Tanque — *acima do orçamento em TUDO* → B5, B6, B13

| Camada | Situação | Veredito |
|---|---|---|
| Skills iniciais | custo **35.3** — única classe fora do alvo 28–32 | ✗ estourado |
| XP | netMult **19.2** — o maior do mod | ✗ teto |
| Perks vivos | dano recebido ×0.85 **incondicional** + carga +30% + pacote pesado completo (recuo ×0.85, ergo ×1.15, GL sem penalidade, braço não cansa) | ✗ pacote mais forte |
| Drawbacks vivos | veloc ×0.9 · fome/sede ×1.3 · **ruído +30% (novo, 2026-07-05)** | ajuda, não fecha a conta |

O ruído novo foi na direção certa. Ainda assim, é a única classe forte nas **três** camadas ao mesmo tempo. A proposta B6 (Couraça só com colete pesado) é a mais elegante: temática ("Couraça"), counterável, e casa com o HeavyVests ×2 que a classe já treina.

### 🩺 Médico de Combate — *hoje é estritamente NEGATIVO* → B1, B8, B12

| Camada | Situação | Veredito |
|---|---|---|
| Skills iniciais | custo 31.0 | ✓ |
| XP | netMult **11.3 — o menor** | ✗ piso |
| Perks vivos | **NENHUM** (os 3 são pendentes ⏳) | ✗ |
| Drawbacks vivos | recuo ×1.25 **ATIVO** | ✗ só o ônus roda |

Quem joga de Médico hoje só perde: paga o drawback de um kit que não existe. B1 (desligar Shaky Hands por default até os perks nascerem) corrige a injustiça com 1 linha.

### 👻 Furtivo — *mais forte do que anuncia* → B2, B7, B11

| Camada | Situação | Veredito |
|---|---|---|
| Skills iniciais | custo 29.7 | ✓ |
| XP | netMult 14.4 plausível, **mas 5 debuffs são "grátis"** (StressResistance, BearRawpower, UsecNegotiations, Vitality, Metabolism — skills que ele não treina) | ⚠ custo inflado |
| Perks vivos | ruído **×0.40 real** (card anuncia ×0.7!) · melee **×5** · +10% veloc c/ melee | ⚠ 2 outliers |
| Drawbacks vivos | aim punch ×1.5 | ✓ morde |

O combo real é mais forte que o exibido: chega nas costas com −60% de ruído e mata com ×5 garantido. B2 alinha o ruído (e é nerf merecido); B7 tira o one-shot trivial.

### 🔫 Fuzileiro — *ok em combate, XP gordo* → B10

| Camada | Situação | Veredito |
|---|---|---|
| Skills iniciais | custo 30.5 | ✓ |
| XP | netMult **18.6** — 2º maior | ⚠ alto |
| Perks vivos | flinch ×0.5 · anti-jam ×0.5 · Adrenalina (recuo ×0.7, recarga ×0.8, ADS ×0.8 — **só na janela de combate**) | ✓ condicional, bem desenhado |
| Drawbacks vivos | ruído +30% | ✓ |

O pacote de combate é o modelo a seguir (forte, mas condicional). Só o XP precisa de dieta (B10).

### 🎯 Caçador — *equilibrado; 1 card mente* → B3

| Camada | Situação | Veredito |
|---|---|---|
| Skills iniciais | custo 31.4 | ✓ |
| XP | netMult 14.5 (≈ média) | ✓ |
| Perks vivos | ADS −15% · fôlego **dreno ×0.5 real** (card anuncia +50%, real ≈ ×2) · braço mirando ×0.65 · Mira Serena ⏳ | ⚠ 1 card mente |
| Drawbacks vivos | veloc mirando −15% (só durante ADS — leve) | ✓ aceitável p/ identidade sniper |

Classe de referência. Só alinhar o Iron Lungs (B3). Se ficar forte demais quando Mira Serena nascer, endurecer o Rooted pra 0.75 (não agora).

### 🎒 Saqueador — *utilidade pura, XP no piso* → B9 (+ item 061)

| Camada | Situação | Veredito |
|---|---|---|
| Skills iniciais | custo 30.5 | ✓ |
| XP | netMult 11.6 — 2º menor | ⚠ baixo |
| Perks vivos | saque silencioso ×0.4 · carga +30% · Quick Hands ⏳ (**061** — antecipa o bônus elite vanilla da Search) | ✓ identidade looter |
| Drawbacks vivos | inércia ∝ peso (×1.5) | ✓ morde de verdade |

Classe de economia, não de combate — ok por design. B9 (+Search ×3) reforça a identidade e tira do piso; o 061 completa o kit.

---

## 4. Temas transversais

1. **Transparência = balance percebido (B4).** Dois cards mentem hoje (Ghost Step, Iron Lungs). Mesmo com B2/B3 alinhando os valores, o certo é o card ler o F12 vivo — o footer do 060 já faz isso; estender aos cards elimina a classe inteira de bug.
2. **Amplitude do netMult: 7.9 → alvo ≈4.** Hoje 11.3 (Médico) ↔ 19.2 (Tanque). Aplicando B5+B8+B9+B10, todas caem na faixa **13–17**. Rodar `node scripts/class-balance-snapshot.mjs` antes/depois de cada mexida nos .jsonc.
3. **Mastery (058) não é neutra (B13).** A camada é igual pra todos hoje; o RN-03 propõe multiplicadores por classe — qualquer ×2 pro Tanque amplifica a classe que já está no teto.

---

## Anexo A — números crus (snapshot 2026-07-05)

| Classe | custo (alvo 28–32) | netMult | netMult plausível | buffs/debuffs | flags |
|---|---|---|---|---|---|
| Hunter | 31.40 ✓ | 14.45 | 14.45 | 18/3 | — |
| Stealth | 29.74 ✓ | 12.43 | 14.35 | 11/7 | 5 debuffs grátis |
| Rifleman | 30.51 ✓ | 18.28 | 18.63 | 21/14 | 1 debuff grátis (Shadowconnections) |
| Combat Medic | 30.95 ✓ | 11.31 | 11.31 | 15/7 | — |
| Scavenger | 30.45 ✓ | 11.56 | 11.56 | 19/2 | — |
| **Tank** | **35.28 ✗** | **18.89** | **19.19** | 12/20 | custo fora do alvo · 1 debuff grátis |

netMult plausível: min 11.31 · máx 19.19 · média 14.92 · **amplitude 7.88**.

Divergências card × F12 encontradas (auditoria completa dos 20 efeitos numéricos — só estas 2):

| Efeito | Card (catálogo) | F12 real | Efeito prático |
|---|---|---|---|
| Ghost Step (Furtivo) | ×0.7 (−30% ruído) | **0.40** | −60% ruído |
| Iron Lungs (Caçador) | +50% duração de fôlego | dreno **×0.50** | ≈ ×2 duração |

## Anexo B — perks/drawbacks vivos por classe (referência rápida)

| Classe | Perks 🟢 vivos | Pendentes ⏳ | Drawbacks 🔴 vivos |
|---|---|---|---|
| Tanque | carga +30% · dano ×0.85 · recuo pesado ×0.85 · ergo pesado ×1.15 · GL sem penalidade · braço não cansa (peso) | — | veloc ×0.9 · fome/sede ×1.3 · ruído +30% |
| Fuzileiro | flinch ×0.5 · anti-jam ×0.5 · Adrenalina (recuo ×0.7 · recarga ×0.8 · ADS ×0.8, janela) | — | ruído +30% |
| Caçador | ADS −15% · fôlego dreno ×0.5 · braço mirando ×0.65 | Mira Serena | veloc mirando −15% |
| Furtivo | ruído ×0.40 · melee ×5 · +10% veloc c/ melee | — | aim punch ×1.5 |
| Saqueador | saque silencioso ×0.4 · carga +30% | Quick Hands (061) | inércia ∝ peso |
| Médico | — | Cuidado Rápido · Cirurgião Ágil · Cirurgia em Movimento | recuo ×1.25 |

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-05 | Guilherme/Claude | Análise criada. |
| 2026-07-05 | Guilherme/Claude | Reorganizado (UX): painel de decisões rastreável (B1–B13), diagnóstico por classe, glossário, anexos com números crus. |
