# 044 — Rodada de balance: Caçador (sniper)

> **Data:** 2026-06-13 · **Status:** 🟢 Aplicado (validação in-game pendente) · **Responsáveis:** mdj<br>
> **Ref:** [balance-model.md](../../docs/balance-model.md), [class-archetypes.md](../../docs/class-archetypes.md)

Rodada solo. Peso ok (Sniper 1.50, AimDrills 1.15) → teto ×2.0.

## Pesquisa (agente web/wiki EFT 0.16.x) — correção importante
- **Sniper (bolt-action) ≠ DMR** são skills SEPARADAS e **não intercambiáveis** (dar XP numa não sobe a outra). Bolt-action = ferrolho (Mosin/T-5000); DMR = marksman semi-auto (SVD/SR-25). A skill "Sniping" (sway/respiração) **não existe** no jogo.
- → O Caçador é **bolt-action puro** ("um tiro, uma morte"); **DMR NÃO é ativada** (contraria a suposição do plano de "ativar DMR" — DMR seria um sub-arquétipo marksman, não o sniper paciente).
- Quem **segura a respiração** para estabilizar a mira é **Endurance** (não há skill de sniper para isso); quem firma a mão é **AimDrills** (elite: sem tremor 2s).
- Perception perdeu o bônus de audição (~jul/2024); virou skill de loot — centralidade baixa para sniper, mas mantida leve.

## Aplicado
**Caçador → +5.47** (era +2.29): Sniper ×2 (+1.50), AimDrills ×2 (+1.15), Endurance ×2 (+1.00), CovertMovement ×2 (+0.94), Perception ×2 (+0.88), Attention ×1.5 (+0.30), Assault ×0.7 (−0.30).

Custo inalterado (29.38). Sem debuff grátis. `.jsonc` editado direto, repo→install verificado.

**Nota:** se no futuro quisermos um sub-arquétipo "marksman" (semi-auto), DMR (peso 3.75) é a skill — mas é outra fantasia, não o Caçador.

| Data | Autor | Alteração |
|---|---|---|
| 2026-06-13 | mdj | Aplicada. Caçador +2.29→+5.47. DMR descartada (≠ bolt-action). AimDrills/Endurance/CovertMovement ×2. |
