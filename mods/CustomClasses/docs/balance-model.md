# Modelo de balanceamento das classes

> **Data:** 2026-06-13<br>
> **Status:** 🟢 Vivo<br>
> **Responsáveis:** mdj<br>
> **Referências:** [class-schema.md](./class-schema.md)<br>

---

> ⚠️ **Superseded para o roster (2026-06-20):** o redesign 11→6 ([class-levers.md](./class-levers.md)) substituiu o roster de 11 classes e **revisou a meta para topo ~+6 / base ~+4** (base compensada por signatures 🔧/🧪 fora do netMult) — a meta "~+6 para todas" do §2 abaixo vale só para o histórico das rodadas 040–045. A **fórmula e o método** (custo/netMult ponderados por peso) seguem válidos e são usados pelo [`class-matrix.mjs`](../scripts/class-matrix.mjs).

Como balancear as classes do CustomClasses de forma sistemática e reutilizável (rodadas iterativas). Dois orçamentos por classe, **ambos ancorados no peso de skill** (`modded/Server/SkillWeights.cs`, espelhado em `scripts/skill-weights.mjs`).

> **Limitação assumida:** o peso mede **raridade de aquisição** (BASELINE/nível-esperado), **não poder de combate/utilidade**. Os orçamentos equilibram **investimento**, não poder real (Immunity 10 × Charisma 10 custam parecido, valem diferente em jogo). O ajuste fino de *poder percebido* é do **playtest**, não da fórmula. **Escopo:** só `skills` + `skillMultipliers`; loadout fica de fora (o ₽ é contexto).

## 1. Budget de CUSTO (capacidade inicial)

Governa as **skills iniciais** (`skills` no `.jsonc`).

```
custo = Σ (nível × peso_da_skill)        # alvo 28–32
```

Regras: alvo **28–32**; ≤ **6** skills com pontos; nível ≤ **10** por skill; cobrir as 4 categorias (Ph/M/C/P) quando fizer sentido. Validado por `scripts/check-skill-costs.mjs`.

## 2. Budget de MULTIPLICADOR (progressão) — buffs/debuffs de XP

Governa os `skillMultipliers`.

```
valor(skill)   = (fator − 1) × peso_da_skill      # buff > 0, debuff < 0
netMult(classe) = Σ valor                          # poder líquido de progressão
```

**Meta (decisão do usuário, 2026-06-13):** `netMult ≈ +6` para **todas** as classes — o **Médico de Combate (+6.17) é o padrão**, construído corretamente e **intacto**. Todas as classes são "edições especiais" igualmente **fortes**; a diferença entre elas vem de **QUAIS skills cada uma acelera** (a identidade do arquétipo), **não** do tamanho do net. Faixa de trabalho **~+5 a +6.5**. *(Substitui a meta anterior de +2.0 ±0.5 / net≈0 — o objetivo deixou de ser achatar e passou a ser **elevar todas ao nível do Médico** mantendo identidades distintas.)*

Regras de composição:
- **Skill-assinatura:** 1–2 skills no foco da classe, fator alto.
- **Debuffs:** 1–2, **temáticos** (skill que a classe "não treina"), fator **≥ 0.7** — para *contraste de identidade*, não para reduzir net (agora subimos, não cortamos).
- **Teto:** buff ≤ **2.0** (ver ressalva de viabilidade abaixo), debuff ≥ **0.7**; **~5–8** skills com multiplicador.
- **Regra anti-furo:** um debuff só "conta" para o net se for em skill **plausível de a classe treinar**. Debuff numa skill que a classe nunca toca é *grátis* na prática. O snapshot mostra `netMult` e `netMult(plausível)`; divergência entre eles = furo a corrigir.
  - **Critério objetivo de "plausível":** skills da(s) **categoria(s) do arquétipo** (ver `class-archetypes.md`) **+** skills exercitadas pelo loadout/playstyle da classe.

> **⚠ Ressalva de viabilidade (peso baixo não alcança +6 com teto ×2.0):** `netMult = Σ (fator−1)×peso`. Com buff ≤ ×2.0, cada skill contribui no máximo `1.0 × peso`. O Médico chega a +6 porque acelera skills de **peso alto** (Immunity 3.75, Vitality/Health 1.67). Classes cujas skills temáticas são de **peso baixo** **não atingem +6** nem buffando tudo no teto:
> - **Gerente** (Crafting 0.33, HideoutManagement 0.39, Charisma 0.40, Memory 0.50, Intellect 0.68): teto temático ≈ **+2.9**.
> - **Saqueador** (Search 0.43, Attention 0.60, Memory 0.50, Intellect 0.68, Perception 0.88): teto temático ≈ **+3.6**.
> - **Recon/Stealth** (CovertMovement 0.94, Search 0.43, Perception 0.88): teto temático ≈ **+3–4**.
>
> Classes de peso alto chegam fácil: **Caçador** (DMR 3.75 — hoje zerada!), **Combate** (Assault/RecoilControl ≈ 1.0 each), **Sobrevivencialista** (Immunity 3.75). Para as de peso baixo, a alavanca para chegar a ~+6 é uma **sub-decisão de cada rodada** (não decidida globalmente): (a) **subir o teto de buff** na assinatura (×2.5–3.0) só para a classe de peso baixo; (b) **aceitar um piso menor** documentado por arquétipo (a classe utilitária *é* mais fraca em progressão de raid — coerente com a fantasia); ou (c) **incluir uma skill temática de peso maior** no conjunto. Levar à decisão do usuário na rodada do grupo afetado.

## 3. Baseline (2026-06-13) — ponto de partida

`node scripts/class-balance-snapshot.mjs`:

| Classe | custo | netMult | b/d | gap até a meta (~+6) |
|---|---|---|---|---|
| **Médico de Combate** | 31.83 | **+6.17** | 7/1 | **PADRÃO — na meta, intacto** |
| Sobrevivencialista | 30.61 | +3.43 | 5/1 | subir ~+2.6 (Immunity já alta — viável) |
| Armeiro | 29.49 | +2.78 | 4/1 | subir ~+3.2 |
| Caçador | 29.38 | +2.29 | 4/1 | subir ~+3.7 (DMR 3.75 zerada → fácil) |
| Fuzileiro | 29.04 | +2.26 | 4/1 | subir ~+3.7 (skills peso ~1.0 → viável) |
| Batedor | 30.00 | +1.70 | 4/1 | subir ~+4.3 (peso baixo → ver ressalva) |
| Op. Tático | 28.61 | +1.61 | 5/1 | subir ~+4.4 |
| Op. Furtivo | 28.71 | +1.40 | 4/1 | subir ~+4.6 (peso baixo → ver ressalva) |
| Gerente | 29.88 | +1.37 | 5/1 | subir ~+4.6 (peso baixo → teto ≈+2.9!) |
| Saqueador | 29.98 | +1.36 | 5/1 | subir ~+4.6 (peso baixo → teto ≈+3.6!) |
| Peladão | — | isenta | — | `noBaseline` (classe-desafio) |

Leitura: o Médico (+6.17) é o **padrão alcançado**; **todas as outras estão abaixo** e precisam **subir** para ~+6, diferenciando-se por *quais* skills aceleram. **Nenhum debuff "grátis"** hoje (`netMult` = `netMult(plausível)` em todas). A barreira não é dispersão — é que **Gerente/Saqueador/Recon não chegam a +6 com teto ×2.0** (ver ressalva de viabilidade no §2): são justamente as que precisam de sub-decisão na rodada.

## 4. Estratégia de ajuste por rodada

1. **Cruzar com o arquétipo** (`class-archetypes.md`, refinado por `/deep-research` do papel/playstyle).
2. **Custo:** manter 28–32, ≤ 6 skills iniciais (não mexe nos multiplicadores).
3. **Multiplicador → meta ~+6 (subir, mantendo identidade):**
   - **Peso alto** (Sobreviv., Caçador c/ DMR, Fuzileiro, Médico-ok): subir a assinatura ao teto ×2.0 e completar com skills temáticas de peso médio/alto até ~+6. Geralmente alcançável.
   - **Peso baixo** (Gerente, Saqueador, Recon/Stealth): aplicar a **ressalva de viabilidade** — levar ao usuário a escolha (a) teto de buff maior na assinatura, (b) piso documentado < +6, ou (c) skill temática de peso maior.
   - **Diferenciação > número:** duas classes do mesmo grupo devem buffar **conjuntos de skills distintos** (anti-clones). O net pode ser parecido; as skills, não.
4. **Validar:** `class-balance-snapshot.mjs` (net ~+6, sem debuff grátis) + `check-skill-costs.mjs` (custo) + matriz/A×B no editor + smoke in-game.

## 5. Ferramentas

- `scripts/skill-weights.mjs` — **fonte JS única** da tabela de peso (espelha `SkillWeights.cs`). Mudou um, muda o outro.
- `scripts/check-skill-costs.mjs` — paridade do **custo** (28–32).
- `scripts/class-balance-snapshot.mjs` — **netMult** + flags (custo, debuff grátis, isenção do Peladão).

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-06-13 | mdj | Criação. Modelo de 2 orçamentos (custo + multiplicador ponderado por peso); meta de netMult +2.0 ±0.5 calibrada pela baseline; regra anti-furo. |
| 2026-06-13 | mdj | **Meta revista (decisão do usuário): netMult ~+6 para todas, Médico de Combate como padrão intacto** — elevar todas ao nível do Médico, diferenciando por *quais* skills (não por tamanho do net). Adicionada ressalva de viabilidade (peso baixo não chega a +6 com teto ×2.0 → sub-decisão por rodada). Reenquadrados §3 (gap até +6) e §4 (subir, não cortar). |
| 2026-06-20 | Guilherme | Nota de superseded no topo: redesign 11→6 mudou o roster e revisou a meta para topo ~+6 / base ~+4 (ver class-levers.md). Fórmula/método mantidos (usados pelo class-matrix.mjs). |
