# Balance review — 2026-07-05 (análise; nenhuma mudança aplicada)

**Mod:** CustomClasses
**Criado:** 2026-07-05
**Pedido:** "O que podemos adicionar/remover/editar para deixar tudo mais equilibrado?"
**Fontes:** `scripts/class-balance-snapshot.mjs` (orçamento de custo + netMult de XP, regra anti-furo) ·
`PerksCatalog.cs` (perks/drawbacks vivos e pendentes) · `PerksConfig.cs` (defaults F12 realmente aplicados) ·
058 (mastery) · inclui as mudanças de hoje (Tanque +30% ruído; 061 Quick Hands no backlog).

## Estado medido

### Eixo 1 — economia de XP (snapshot; alvo custo 28–32)

| Classe | Custo inicial | netMult (plausível) | Flags |
|---|---|---|---|
| Médico de Combate | 30.95 ✓ | **11.31 (mín)** | — |
| Saqueador | 30.45 ✓ | 11.56 | — |
| Furtivo (Stealth) | 29.74 ✓ | 14.35 | ⚠ 5 debuffs "grátis" (não mordem) |
| Caçador (Hunter) | 31.40 ✓ | 14.45 | — |
| Fuzileiro (Rifleman) | 30.51 ✓ | 18.63 | ⚠ 1 debuff grátis |
| **Tanque (Tank)** | **35.28 ✗ (única fora)** | **19.19 (máx)** | ⚠ 1 debuff grátis |

Amplitude do netMult: **7.88** (média 14.92) — grande.

### Eixo 2 — perks/drawbacks VIVOS (o que realmente roda hoje)

| Classe | Perks vivos | Pendentes ⏳ | Drawbacks vivos |
|---|---|---|---|
| Tanque | Carga +30% · dano recebido ×0.85 · pacote pesado (recuo ×0.85, ergo ×1.15, GL sem penalidade, braço não cansa) | — | velocidade ×0.9 · fome/sede ×1.3 · **ruído +30% (novo)** |
| Fuzileiro | flinch ×0.5 · anti-jam ×0.5 · Adrenalina (recuo ×0.7/recarga ×0.8/ADS ×0.8, janela) | — | ruído +30% |
| Caçador | ADS −15% · fôlego (dreno ×0.5) · braço mirando ×0.65 | Mira Serena | velocidade mirando −15% |
| Furtivo | ruído ×0.40 (F12) · melee ×5 · +10% veloc. c/ melee | — | aim punch ×1.5 |
| Saqueador | saque silencioso ×0.4 · carga +30% | Quick Hands (061) | inércia ∝ peso (×1.5) |
| **Médico** | **NENHUM** | Cuidado Rápido · Cirurgião Ágil · Cirurgia em Movimento | **recuo ×1.25 (ativo!)** |

### Eixo 3 — mastery (058, camada igual p/ todos)
−0.4% recuo · +0.2% ergo por nível (SMG/LMG/Launcher/Underbarrel em mãos). RN-03 (multiplicadores de XP de
maestria POR CLASSE) segue pendente de decisão.

## Findings e propostas (BAL-NN, por prioridade)

**BAL-01 · ALTA — Tanque acima do orçamento nos DOIS eixos + melhor pacote vivo.**
Único fora do custo (35.28), maior netMult (19.19) e o perk individual mais forte do mod (dano recebido ×0.85,
incondicional). O +30% de ruído de hoje ajuda, mas não fecha a conta.
→ Propostas: (a) baixar níveis iniciais: `Shotgun 5→4` + `Vitality 5→4` (custo ≈ 31.5, volta ao range);
(b) **Couraça condicional**: ×0.85 só com colete pesado (classe 4+) equipado — temático ("Couraça"), counterável
e casa com HeavyVests ×2; sem colete → ×1.0; alternativa mais simples: 0.85→0.88;
(c) no RN-03, dar mastery ×1.5 (não ×2) pro Tanque.

**BAL-02 · ALTA — Médico de Combate é estritamente NEGATIVO no build vivo.**
Os 3 perks são pendentes; o drawback (recuo ×1.25) está ativo; e é o menor netMult (11.31). Quem joga de Médico
hoje só perde.
→ Propostas: (a) prioridade de implementação pros perks do 050-medic (Rapid Care/Swift Surgeon — a perna
transpiler); (b) **até lá, `Shaky Hands — Enabled` default OFF** (1 linha no PerksConfig; religa quando os perks
existirem); (c) subir 1–2 buffs de XP (ex.: Vitality/Health ×1.5) pra tirar do piso do netMult.

**BAL-03 · MÉDIA — Melee ×5 do Furtivo é o pico de spike do mod.**
Com ruído ×0.40 (ver BAL-04) o Furtivo chega nas costas com facilidade; ×5 transforma qualquer faca em execução
garantida sem counterplay.
→ Proposta: ×3.5 (ainda mata com 1–2 golpes por trás) OU manter ×5 condicionado a golpe pelas costas
(se o lever atual não distinguir ângulo, ficar no ×3.5).

**BAL-04 · MÉDIA — Cards mentem em 2 efeitos (transparência = balance percebido).**
(a) **Ghost Step**: card mostra ×0.7 (−30%), F12 aplica **0.40 (−60%)**; (b) **Iron Lungs**: card mostra
"+50% duração", F12 aplica dreno ×0.5 (**≈ ×2 duração**). Ambos mais fortes do que o anunciado.
→ Propostas: alinhar o real ao anunciado (Ghost Step 0.40→0.65 — também é nerf merecido; Iron Lungs
0.5→0.65 ≈ +50% real) e/ou fazer os cards lerem o F12 vivo (precedente: footer do 060). Recomendo AMBOS:
valor alinhado + card vivo.

**BAL-05 · MÉDIA — Debuffs "grátis" inflam o custo aparente (regra anti-furo).**
Furtivo: StressResistance/BearRawpower/UsecNegotiations/Vitality/Metabolism não mordem (skills que ele não
treina) — netMult real 12.43 vs 14.35 plausível. Fuzileiro/Tanque: Shadowconnections idem.
→ Proposta: trocar por debuffs em skills que a classe treina (ex.: Furtivo — `Endurance ×0.8`,
`Throwing ×0.8`) mantendo o netMult plausível ~14.

**BAL-06 · MÉDIA — Comprimir a amplitude do netMult (7.88 → ~4).**
Alvo sugerido: todas em 13–17. Baixar Fuzileiro ~18.6→~16.5 (cortar os derived BearRawpower/UsecNegotiations
×1.5, −2.1) e Tanque via BAL-01; subir Médico via BAL-02 e Saqueador ~11.6→~13 (ex.: `Search ×2→×3` — sinergia
com Quick Hands 061, identidade de looter).

**BAL-07 · BAIXA — Pack Mule compartilhado agora está justo.**
Saqueador paga com Overladen (inércia ∝ peso), Tanque passa a pagar com ruído+velocidade. Sem ação.

**BAL-08 · BAIXA — Rooted (Caçador) quase não morde.**
−15% de velocidade só enquanto MIRA. Aceitável como identidade (sniper parado); se o Caçador subir de winrate
após Mira Serena, endurecer pra 0.75.

**BAL-09 · INFO — RN-03 (mastery por classe) deve respeitar o teto do Tanque.**
Proposta original (Tanque LMG/Launcher/Attached ×2, Fuzileiro SMG ×1.5, Furtivo SMG ×2) amplifica a classe já
no teto — ver BAL-01(c).

## Ordem sugerida de execução
1. BAL-02(b) — Shaky Hands OFF por default (1 linha, tira o Médico do negativo hoje).
2. BAL-04 — alinhar Ghost Step/Iron Lungs + cards vivos.
3. BAL-01(a/b) — custo do Tanque + Couraça condicional.
4. BAL-03 — melee do Furtivo.
5. BAL-05/06 — passada fina nos .jsonc de XP (rodar o snapshot antes/depois).
6. BAL-02(a) — perks do Médico (item 050, perna transpiler).

## Histórico

| Data | Evento |
|---|---|
| 2026-07-05 | Análise criada (snapshot + catálogo + F12 + mastery). Nenhuma mudança aplicada. |
