# 043 — Rodada de balance: Utilitário / Hideout

> **Data:** 2026-06-13 · **Status:** 🟢 Aplicado (validação in-game pendente) · **Responsáveis:** mdj<br>
> **Ref:** [balance-model.md](../../docs/balance-model.md), [class-archetypes.md](../../docs/class-archetypes.md)

Armeiro + Gerente de Operações.

## Pesquisa (agente web/wiki + globals.json SPT 4.0)
- **WeaponTreatment** = durabilidade/desgaste/reparo (≠ jams). **TroubleShooting** = malfunctions/jams (conserto rápido, −recorrência). Dupla coesa e in-raid do Armeiro.
- **Intellect NÃO dá buff de XP a outras skills** (crença invertida — no globals, *outras* skills alimentam Intellect: Crafting→Intellect 6.6%). Intellect = velocidade de exame + qualidade de reparo.
- **Crafting** elite dobra produção paralela; **HideoutManagement** corta consumo + amplia bônus de zona. Skills do Gerente são **peso baixo** e grind lento por design (fora-de-raid).
- Recomendação da pesquisa: buff grande na **taxa de XP** das skills de hideout (cumpre a fantasia) e **aceitar o Gerente mais fraco em raid**; NÃO inflar o efeito mecânico de produção (vira impressora de dinheiro).

## Aplicado
**Armeiro → +5.33** (era +2.78): WeaponTreatment ×2 (+1.25), TroubleShooting ×2 (+2.50 — peso 2.5, maior alavanca), Intellect ×2 (+0.68), Crafting ×2 (+0.33 — modifica gear), Strength ×2 (+0.47), Assault ×1.3 (+0.30), Endurance ×0.8 (−0.20). Peso ok → teto ×2.0.

**Gerente → +4.23** (era +1.37): Crafting ×3 (+0.66), HideoutManagement ×3 (+0.78), Intellect ×2 (+0.68), Charisma ×2 (+0.40), Attention ×2 (+0.60), Shotgun ×1.5 (+1.25 — autodefesa que ele já porta), Strength ×0.7 (−0.16). **Memory removida** (morta no 0.14.5).
- **Decisão de viabilidade (usuário):** "teto temático ~+4.5". O Gerente fica **abaixo do padrão de propósito** — é a classe "fora de raid"; o buff grande é na **taxa de XP** das skills de hideout, não no efeito mecânico (evita economia descontrolada).

Custo inalterado (Armeiro 29.49, Gerente 29.88). Sem debuff grátis. `.jsonc` editado direto, repo→install verificado.

| Data | Autor | Alteração |
|---|---|---|
| 2026-06-13 | mdj | Aplicada. Armeiro +2.78→+5.33; Gerente +1.37→+4.23 (piso temático por decisão do usuário; Memory removida). |
