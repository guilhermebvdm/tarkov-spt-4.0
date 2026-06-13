# 040 — Rodada de balance: Suporte / Sobrevivência

> **Data:** 2026-06-13<br>
> **Status:** 🟢 Aplicado (validação in-game pendente)<br>
> **Responsáveis:** mdj<br>
> **Referências:** [balance-model.md](../../docs/balance-model.md), [class-archetypes.md](../../docs/class-archetypes.md)<br>

---

Primeira rodada do épico de balance (039–045). Grupo **Suporte/Sobrevivência**: Médico de Combate + Sobrevivencialista. O **Médico (+6.17) é o padrão intacto** (decisão do usuário) — esta rodada ajusta **só o Sobrevivencialista**, usando o Médico como referência de ~+6 e como contraste de identidade.

## Pesquisa (substituiu `/deep-research`, indisponível neste ambiente)

Agente de pesquisa web + wiki + source do Skills-Extended. Achados-chave (EFT 0.16.x / SPT 4.0):

- **Núcleo de resiliência passiva:** Immunity (imune a veneno/efeitos negativos), Metabolism (elite: imune a fome/sede), Vitality (sangramento autopara), Health (−fratura/fome/sede), Endurance (aguenta o raid em movimento). StressResistance é tangente (funciona machucado).
- **Skill-assinatura distintiva:** **Immunity** — nenhum médico precisa dela; encapsula "meu corpo resiste" vs. "eu me curo".
- **Search é periférico** ao arquétipo (looting; velocidade real vem de Attention, não Search).
- **FirstAid/FieldMedicine (SE) são ATIVAS** (disparam ao usar item médico/stim) → núcleo do **Médico**, não do survivalist passivo. Confirmado no código do SE.
- Tensão notada: Metabolism e Immunity competem por XP (Metabolism reduz duração de debuffs, que é o que treina Immunity).

## Mudança aplicada (`skillMultipliers`)

| Skill | antes | depois | valor | nota |
|---|---|---|---|---|
| Immunity | ×1.5 | **×2.0** | +3.75 | 🔑 assinatura |
| Metabolism | ×2.0 | ×2.0 | +0.29 | núcleo |
| Vitality | ×1.5 | ×1.5 | +0.84 | núcleo |
| Health | ×1.3 | ×1.3 | +0.50 | núcleo |
| Endurance | — | **×1.5** | +0.50 | ativada (faltava) |
| StressResistance | — | **×1.3** | +0.26 | ativada |
| Search | ×1.3 | **removida** | — | periférico |
| RecoilControl | ×0.8 | ×0.8 | −0.20 | debuff temático (plausível via Shotgun inicial) |

**netMult: +3.43 → +5.94** (meta ~+6 ✓). Custo inalterado (30.61; skills iniciais não tocadas). Sem debuff "grátis" (`netMult` = `netMult(plausível)`).

## Diferenciação vs. Médico de Combate

| | Médico (ativo) | Sobrevivencialista (passivo) |
|---|---|---|
| Assinatura | Surgery ×2.0 | **Immunity ×2.0** |
| Exclusivo | FirstAid + FieldMedicine (SE) | Metabolism ×2.0 + Endurance |
| Fantasia | "eu conserto o dano" | "meu corpo não desiste" |

## Aplicação

- Editado `skillMultipliers` direto no `.jsonc` (decisão do usuário — gerador congelado, install = fonte de verdade).
- Diff repo×install pré-aplicação: só `skillMultipliers` divergia (sem risco de clobber de loadout/outfit da sessão do editor).
- Copiado repo→install (`D:/SPT/SPT/user/mods/CustomClasses/config/classes/sobrevivencialista.jsonc`); verificado `install == repo`.
- Validado: `class-balance-snapshot.mjs` (+5.94, sem flag) + `check-skill-costs.mjs` (custo OK, paridade mantida).

## Pendências

- **Validação in-game** (regra `feedback_spt_validation`): perfil novo de Sobrevivencialista → conferir os ±% no tooltip das skills (Immunity +100%, Endurance/StressResistance presentes, Search sem buff). Requer reload do server (edição direta do `.jsonc`, não hot-apply).
- **Nota de custo (fora do escopo):** 7 skills iniciais com pontos (teto do modelo = 6). Custo OK; limpeza opcional adiada.

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-06-13 | mdj | Rodada aplicada. Sobrevivencialista net +3.43→+5.94; assinatura Immunity ×2.0; Endurance/StressResistance ativadas; Search removida. |
