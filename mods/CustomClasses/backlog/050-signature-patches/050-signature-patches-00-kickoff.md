# 050 — Perks + drawbacks de signature (🔧🔻) · Kickoff

**Mod:** CustomClasses · **Data:** 2026-06-21 · **Origem:** redesign 11→6 → "tudo é perk flat" ([class-design.md](../../docs/class-design.md))
**Wave:** R-W1 · **Deps:** **054 (rename Furtivo) é pré-requisito do 050.0** · 047 (soft)

> Brief de kickoff — insumo para `/create-spec 050`. Não é a spec. **Fonte autoritativa: [class-design.md](../../docs/class-design.md)** (perks/drawbacks por classe + Contrato de gating + patch-points + fatiamento).

## Objetivo

Patches Harmony **per-player keyed na classe** (gating pela **chave estável `name`**, ver Contrato de gating no doc), **client-side**, **F12-live** (ler `ConfigEntry` no apply-time). **Todas as signatures são flat** — não há skill custom que escala. Estende a infra client que **já existe** (`modded/Client/Plugin.cs` + ConfigEntry + Harmony).

**12 perks 🔧 + 6 drawbacks 🔻** (1 drawback/classe): Combat Medic · Cool Under Fire + Adrenaline · Sharpshooter + Iron Lungs · Ghost Step + Execution · Quick Hands + Silent Looter + Pack Mule · Bulwark + Bunker; drawbacks Shaky Hands · Loud Operator · Rooted · Rattled · Overladen · Heavy Frame.

## Fatiamento (cada fatia = 1 ciclo SDD; ver doc §Implementação)

- **050.0 — Infra + 2 provas** *(✅)*: gating per-classe (`name`) + framework F12-live + **Bulwark** (dano) + **Pack Mule** (carga, piso). Valida ponta-a-ponta in-game.
- **050.1 — Movimento/inércia** *(✅)*: Execution vel · Rooted · Heavy Frame vel · Overladen.
- **050.2 — Recuo/aim-punch** *(✅/🟡)*: Shaky Hands · Adrenaline recuo · Cool Under Fire supressão · Rattled.
- **050.3 — Combate/saúde** *(🟡)*: Execution melee · Heavy Frame fome/sede · Combat Medic · malfunction · máquina-de-estado da Adrenaline.
- **050.4 — Som/arma/inventário** *(🟡+✅)*: Ghost Step/Loud/Silent · Sharpshooter · Bunker GL · Quick Hands · Iron Lungs respiração/sway.

## Escopo / Riscos

- **Gating:** `Info.GameVersion` = `displayName[lang]` (muda com idioma) → **gatear pela chave estável `name`** (mapear via `classVisualRegistry`), **não** hardcodar idioma. Furtivo = `Ghost` no runtime até o **054**.
- **Confiança dos patch-points = estimativa do recon** (nomes `GClass*`/método são version-specific) → **re-confirmar o alvo no assembly carregado just-in-time** em cada fatia (decompilar de `D:/SPT` se preciso). 9 ✅ · ~7 🟡 · 2 ⚠️.
- **Em aberto (resolver na spec da fatia):** som "todos os sons" = **multi-hook** (não 1 knob); aim-punch **hit vs supressão**; Energy/Hydration **client vs server** (se server, Heavy Frame vira restart); melee-dano/GL-ergo/uso-de-medkit/lock-de-cirurgia + gatilho "causar dano" da Adrenaline = métodos a confirmar.
- **ZONA STANCES (vai pro 051, não aqui):** Iron Lungs braço-ADS + Bunker arma-pesada → compor via `StaminaController.Multipliers`/`ArmStaminaCoordinator`; **NÃO patchar `GClass774`** (o stances neutraliza).
- Velocidade/inércia **compõem** com o stances (postfix-mult) ✅.

## Refs

- **[../../docs/class-design.md](../../docs/class-design.md)** (autoridade) · [../../scripts/class-matrix.mjs](../../scripts/class-matrix.mjs) (matriz)
- Skills `spt-mod-best-practices`, `csharp-mod-best-practices`, `graph-code-navigation`

## DoD (resumo)

- **Aceite por EFEITO** (não por perk — perks divididos só ficam 100% após a última fatia): cada efeito observável in-game na classe certa, **sem efeito nas outras** (gating validado). Ex.: Bulwark → hit conhecido perde −15% HP; Pack Mule → +30% no limite de peso.
- Toda constante exposta no **F12** (`ConfigEntry`), **lida no apply-time** (muda durante a raid) — exceto o que a spec marcar como restart.
