# Rebalance v2 — épico da planilha `classes-perks-draws`

> **Data:** 2026-07-25<br>
> **Status:** 🟢 Vivo<br>
> **Responsáveis:** Guilherme<br>
> **Referências:** [balance-review-2026-07-05.md](./balance-review-2026-07-05.md), [../docs/perk-ideas.md](../docs/perk-ideas.md), [../docs/balance-perks-by-class.csv](../docs/balance-perks-by-class.csv)<br>

---

Fonte: o usuário calibrou a aba **`classes-perks-draws`** de `docs/classes-perks-draws.xlsx` (colunas `Classe · Efeito atual · Nome PT · Modificador · Valor · Ação`). Este doc estrutura o desenvolvimento. Itens no `mod-backlog.md`: **078–084**.

## Decisões travadas (respostas do usuário, 2026-07-25)

1. **Saque rápido pistola** = acelera o *deploy* de **qualquer arma no slot HOLSTER** (não só pistola) — gate pelo slot, não pelo tipo de arma.
2. **Lebre** (Scav) e **Saque Barulhento** (Rifleman) — ambos **entram** (vieram sem marca na planilha).
3. **Medroso** (Scav) — **portar o mod `UnderFire-2.0.1`** (detecção de "sob fogo" por dano + near-miss `GClass897.OnShoot`; efeito `Tremor`), gateado só p/ Scav, e **desativar o UnderFire global** (hoje aplica a todos, sempre).
4. **Recarga rápida escopeta** (Tank) — usar a **mecânica elite do EFT** (Mag Drills — 2 cartuchos por vez); **spike** confirma antes.

## Categorias de mudança

### A) Recalibrar valor (novo default no F12 + config) — item 078
| Classe | Modificador | De → Para |
|---|---|---|
| Combat Medic | tempo de cirurgia (Swift Surgeon) | 0.5 → **0.75** |
| Combat Medic | tempo de cura (Rapid Care) | 0.7 → **0.75** |
| Hunter | dreno de respiração (Iron Lungs) | 0.667 → **0.7** |
| Rifleman | ADS na janela (Adrenaline Focus) | 0.80 → **0.7** |
| Rifleman | recarga na janela (Adrenaline Reload) | 0.80 → **0.7** |
| Tank | recuo arma pesada (Steady Mount) | 0.85 → **0.7** |
| Tank | fadiga braço pesado (Tireless Arms) | 0.20 → **0.5** |
| Tank | fome/sede (Heavy Appetite) | 1.30 → **1.15** |

### B) Realocar levers existentes (REMOVER/ADD/rename) — item 079
- **REMOVER da classe:** Combat Medic → Mobile Surgery · Scavenger → Overladen (inércia).
- **ADD (lever que já existe, nova classe, como drawback):** Combat Medic → Rattled (tranco ×1.5) · Hunter → Light Frame (carga −0.2) · Stealth → Light Frame (carga −0.2) · Scavenger → Falta de habilidade (recuo 1.25) · Rifleman → Saque Barulhento (volume de loot ×1.3, = Silent Looter invertido).
- **RENAME + ligar:** `Shaky Hands / Mãos Trêmulas` → **`Falta de habilidade`** (recuo); sai de OFF → **ativa** (Combat Medic + Scavenger).

### C) Criar do zero (NOVO) — itens 080–084
| # | Nome | Modificador | Classe(s) | Mecânica |
|---|---|---|---|---|
| 080 | Saque Rápido | deploy de arma no slot HOLSTER (×0.8) | Hunter, Rifleman, Stealth | patch novo (velocidade de saque) |
| 081 | Lebre | +velocidade quando leve (×1.3) | Scavenger | patch novo (condicional ao peso) |
| 082 | Medroso | tremor de mãos sob fogo | Scavenger | portar UnderFire + gate de classe |
| 083 | Morte Silenciosa | abate com faca + puxar faca sem som | Stealth | patch novo (som) — **coop + AI** |
| 084 | Recarga Rápida Escopeta | recarrega 2 cartuchos por vez | Tank | spike mecânica elite EFT |

## Mapa de sincronização (coop Fika / AI vanilla+SAIN)

> Regra 075: todo efeito gateia por classe do dono e barra IsAI. Regra coop: som que a IA/peers percebem precisa do pipeline host-side (065) + rolloff de peer (066).

| Modificador | Precisa sync? | Por quê |
|---|---|---|
| Valores A (recuo/ADS/sway/tempo/HP/fome/carga) | ❌ local | efeito no próprio jogador; carga/velocidade replicam via Fika nativo |
| Rattled, Falta de habilidade, Light Frame (item 079) | ❌ local | recuo/tranco/carga são do próprio jogador |
| Saque Rápido (080) | ❌ local | animação de saque local; peers veem via replicação nativa |
| Lebre (081) | ❌ local | velocidade replica via Fika nativo (molde Heavy Frame) |
| Medroso (082) | ❌ local | tanto o gatilho (dano/near-miss, calculado no cliente do Scav) quanto o efeito (Tremor visual) são do próprio jogador |
| **Saque Barulhento (079)** | ✅ **coop + AI** | volume de loot que a IA e os peers ouvem → mesmo pipeline do Silent Looter (065/066/B14), lever invertido (>1) |
| **Morte Silenciosa (083)** | ✅ **coop + AI (crítico)** | o som do abate/faca **não pode alertar bots (SAIN/vanilla) nem peers (Fika)** — precisa suprimir na origem e não notificar a IA |

## Corners gerais (valem p/ todos os itens)

- **Bots/scav-player:** gate por `IsLocalClass` + identidade de instância (regra 075) — nenhum efeito vaza p/ bot do host nem p/ o próprio jogador entrando de SCAV (a classe é do PMC).
- **Estado entre raids:** efeitos reativos (Medroso/tremor, janelas) e flags precisam ser limpos no fim da raid (não vazar p/ o menu/próxima raid).
- **Toggle F12 mid-raid:** todo lever lê `.Value` no apply-time (DoD do 050) — mudança vale sem restart.
- **F12 renomeado (item 079, Shaky Hands→Falta de habilidade):** renomear a key **reseta** o valor salvo — comunicar no changelog (mesmo caso do 067).
- **Config idêntica p/ todos (decisão 2026-07-12):** os valores viajam iguais a todos os jogadores; sem sync de config.

## Sequência sugerida (menor risco → maior)

1. **078** (valores) — trivial, sem código novo, valida o pipeline de recalibração.
2. **079** (realocações) — mexe em gates de classe + catálogo + F12; médio.
3. **080 · 081** (Saque Rápido · Lebre) — perks novos locais, patches diretos.
4. **082** (Medroso) — porta UnderFire; reconfirmar nomes ofuscados no decompile atual.
5. **084** (spike escopeta) — confirma a mecânica elite antes de comprometer.
6. **083** (Morte Silenciosa) — o mais caro (coop + AI); por último, com gate in-game entre os anteriores.

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-25 | Guilherme | Criação — épico do rebalance v2 a partir da planilha `classes-perks-draws`; 4 decisões travadas; achado UnderFire (derisca efeitos visuais) |
