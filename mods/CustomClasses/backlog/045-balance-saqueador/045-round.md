# 045 — Rodada de balance: Saqueador (loot)

> **Data:** 2026-06-13 · **Status:** 🟢 Aplicado (validação in-game pendente) · **Responsáveis:** mdj<br>
> **Ref:** [balance-model.md](../../docs/balance-model.md), [class-archetypes.md](../../docs/class-archetypes.md)

Rodada solo. **⚠ Peso baixo** (Attention 0.60, Perception 0.88, Search 0.43) → teto ×3.0 na assinatura.

## Pesquisa (agente web/wiki EFT 0.16.x)
- **Velocidade de loot = Attention** (+100% elite), **não Search**. Search só dá buscar 2-containers no elite → buffar Search ×3 rende quase nada. Perception = detectar loot à distância (raio +100%, radar no elite).
- **Memory REMOVIDA** do EFT em 0.14.5 → buff de Memory é inerte. **Removida** da composição.
- Para chegar perto de +6 com skills leves: buff agressivo (×3) nas leves de detecção/velocidade (Attention/Perception) + **Strength/Endurance ×2** (peso maior) como eixo "carrega muito loot".

## Aplicado
**Saqueador → +5.24** (era +1.36): Attention ×3 (+1.20), Perception ×3 (+1.76), Intellect ×2 (+0.68 — identifica valiosos), Endurance ×2 (+1.00), Search ×2 (+0.43), Strength ×2 (+0.47 — carga), RecoilControl ×0.7 (−0.30). **Memory removida.**

Custo inalterado (29.98). Sem debuff grátis. `.jsonc` editado direto, repo→install verificado.

| Data | Autor | Alteração |
|---|---|---|
| 2026-06-13 | mdj | Aplicada. Saqueador +1.36→+5.24. Assinatura Attention/Perception ×3; +Strength/Endurance carga; Memory removida (skill morta). |
