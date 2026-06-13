# 042 — Rodada de balance: Combate

> **Data:** 2026-06-13 · **Status:** 🟢 Aplicado (validação in-game pendente) · **Responsáveis:** mdj<br>
> **Ref:** [balance-model.md](../../docs/balance-model.md), [class-archetypes.md](../../docs/class-archetypes.md)

Fuzileiro + Operador Tático. Peso ok (skills ~1.0) → teto ×2.0.

## Pesquisa (agente web/wiki EFT 0.16.x)
- **Assault** = pacote de handling exclusivo de rifle de assalto (reload/troca/recoil/ergo). **AimDrills** = velocidade de ADS + mão firme nos 2s pós-mira (elite). **RecoilControl** = controle de spray, **mas REMOVIDA do EFT live em 0.14.5** → validar in-game se ainda aplica no SPT; não fazer dela a assinatura.
- **Anti-clone (ambos usam Assault/Endurance):** diferenciar por **magnitude** — Fuzileiro = pico balístico (Assault/AimDrills altos); Tático = espalhado físico (Endurance/Strength/StressResistance), Assault só moderado. Op. Tático "sem fraqueza" → **único sem debuff**.

## Aplicado
**Fuzileiro → +5.64** (era +2.26): Assault ×2 (+1.00), AimDrills ×2 (+1.15), RecoilControl ×2 (+1.00), MagDrills ×2 (+0.94), Endurance ×2 (+1.00), Throwing ×2 (+0.83), CovertMovement ×0.7 (−0.28).

**Operador Tático → +5.50** (era +1.61): Endurance ×2 (+1.00), Strength ×2 (+0.47), StressResistance ×2 (+0.88), MagDrills ×2 (+0.94), Throwing ×2 (+0.83), AimDrills ×1.5 (+0.575), Assault ×1.5 (+0.50), Attention ×1.5 (+0.30). **Sem debuff** (identidade "all-rounder sem fraqueza").

Custo inalterado (Fuzileiro 29.04, Tático 28.61). Sem debuff grátis. `.jsonc` editado direto, repo→install verificado.

**Pendência:** validar in-game se **RecoilControl** produz efeito no SPT 4.0 (removida do live 0.14.5). Se inerte, o recoil do Fuzileiro fica em Assault (−15%) + Weapon Mastery.

| Data | Autor | Alteração |
|---|---|---|
| 2026-06-13 | mdj | Aplicada. Fuzileiro +2.26→+5.64 (pico balístico); Tático +1.61→+5.50 (espalhado, sem debuff). |
