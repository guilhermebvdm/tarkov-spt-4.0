# 041 — Rodada de balance: Recon / Stealth

> **Data:** 2026-06-13 · **Status:** 🟢 Aplicado (validação in-game pendente) · **Responsáveis:** mdj<br>
> **Ref:** [balance-model.md](../../docs/balance-model.md), [class-archetypes.md](../../docs/class-archetypes.md)

Batedor + Operador Furtivo. **⚠ Peso baixo** → ressalva de viabilidade aplicada: teto de buff na **assinatura sobe para ×3.0** (na própria skill temática). Decisão tomada pelo agente sob o "continue" do usuário (meta ~+6 para todas).

## Pesquisa (agente web/wiki EFT 0.16.x)
- **Velocidade de loot = Attention** (+100% elite), não Search (Search só dá 2-containers no elite). Perception = detectar/raio de loot.
- **CovertMovement** = assinatura de ruído (−60% elite, todas as superfícies). **MagDrills** = manuseio de munição (recarga/descarga ±30%, check instantâneo elite) — "eficiente sob pressão".
- **Anti-clone:** ambos usam CovertMovement+Perception → diferenciar por **hierarquia invertida**: Batedor = Perception primário (sensor) + Endurance (mobilidade); Furtivo = CovertMovement primário (silêncio) + MagDrills (manuseio).

## Aplicado
**Batedor → +5.36** (era +1.70): Perception ×3 (+1.76), Endurance ×3 (+2.00), Attention ×2.5 (+0.90), CovertMovement ×1.5 (+0.47), Search ×2 (+0.43), RecoilControl ×0.8 (−0.20).

**Operador Furtivo → +5.33** (era +1.40): CovertMovement ×3 (+1.88), MagDrills ×3 (+1.88), Endurance ×2 (+1.00), Perception ×1.5 (+0.44), Search ×2 (+0.43), Assault ×0.7 (−0.30).

Custo inalterado (Batedor 30.00, Furtivo 28.71). Sem debuff grátis. `.jsonc` editado direto, repo→install verificado.

| Data | Autor | Alteração |
|---|---|---|
| 2026-06-13 | mdj | Aplicada. Batedor +1.70→+5.36; Furtivo +1.40→+5.33. Hierarquia Perception↔CovertMovement invertida (anti-clone). Teto ×3.0 na assinatura (peso baixo). |
