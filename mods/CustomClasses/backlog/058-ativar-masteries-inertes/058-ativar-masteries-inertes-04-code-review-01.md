# 058 — Ativar masteries inertes · Code Review 01

**Mod:** CustomClasses
**Spec técnica:** [058-ativar-masteries-inertes-02-spec-tech.md](058-ativar-masteries-inertes-02-spec-tech.md) (§10)
**Asbuild:** [058-ativar-masteries-inertes-05-asbuild.md](058-ativar-masteries-inertes-05-asbuild.md)
**Data:** 2026-07-04

> Review adversarial de contexto limpo sobre o commit `e6f3816` (10 verificações obrigatórias contra as fontes).
> Decisões autônomas (/g-autodev), todas aplicadas no mesmo dia.

## Resumo

> 🔴 0 · 🟠 1 · 🟡 3 · 🟢 3 · ✅ Aplicados: 7 · **Veredito do revisor: aprovado com ressalvas** (nenhum bloqueador)

## Índice

| ID | Cat | Impacto | Título | Status |
|---|---|---|---|---|
| CR-01-01 | C | 🟠 | XP cru bypassa `CalculateExpOnFirstLevels` — nível 0→1 ficaria 10× mais lento que a paridade prometida | ✅ Aplicado |
| CR-01-02 | B | 🟡 | `TryGet` sem `EnsureLoaded()` (único call-site do repo sem o par) | ✅ Aplicado |
| CR-01-03 | D | 🟡 | Ordem dos Prefixes (maestria antes do 050) era implícita e load-bearing pro PerkDiag | ✅ Aplicado |
| CR-01-04 | C | 🟡 | XP no shooting range do hideout (vanilla bloqueia via override vazio) | ✅ Aplicado |
| CR-01-05 | E | 🟢 | Clamps inconsistentes (0.5 no recuo × 0 no excesso do `float_5`) | ✅ Aplicado |
| CR-01-06 | C | 🟢 | Fator de classe ignorava o master switch `EnableSkillMultipliers` | ✅ Aplicado |
| CR-01-07 | F | 🟢 | Fator negativo sem clamp explícito | ✅ Aplicado |

## Resoluções aplicadas

- **CR-01-01:** `if (skill.Level < 9) xp = skill.CalculateExpOnFirstLevels(xp);` antes do `SetCurrent`
  (método público — SkillClass.cs:108; `WeaponSkillClass : SkillClass` confirmado via ilspycmd). O skip de
  fadiga (`UseEffectiveness`) e `BonusController` permanece como decisão documentada (PA-01-02/asbuild).
- **CR-01-02:** `SkillMultipliers.EnsureLoaded()` antes do `TryGet`.
- **CR-01-03:** `[HarmonyPriority(Priority.High)]` no Prefix da maestria + comentário de ordem intencional no
  `Plugin.cs`. Caveat de instrumento anotado no asbuild: com nível de maestria >0, o "Before" do overlay 052
  deixa de ser baseline vanilla puro (inclui a maestria — é o desenho).
- **CR-01-04:** no-op quando o MainPlayer é do hideout (detecção por nome, mesmo padrão do
  `RaidPerksNotificationPatch` — sem tipo hard no IL). Paridade: range não dá XP de weapon skill.
- **CR-01-05:** piso `0.5` também na escala do excesso do `float_5`.
- **CR-01-06:** fator de classe só com `Plugin.Enabled` (semântica do `OnTriggerPatch`).
- **CR-01-07:** `xp *= Mathf.Max(0f, f)`.

## Verificações que passaram limpas (evidência no relatório do revisor)

`method_57` único/não-virtual com caller único no OnFireEvent do lançador (bots cobertos pelo gate de controller) ·
`float_5` = exclusivamente força de recuo por tiro (set 14243/14298, consumo único 14479); escala do excesso segura;
sem double-dip (tiro do underbarrel passa no Prefix de recuo com a arma HOST → skill null) · APIs
`Skills`/`AttachedLauncher`/`SetCurrent`/`Current`/`Level` confirmadas · ergo comuta com Bunker (stacking
intencional) · zero estáticos novos (AP-01/08) · `SetCurrent(silent)` ainda notifica level-up sem spam ·
F12 = spec §10 = PROPRIEDADES; `Enabled=false` desliga tudo · **Fika por fonte:** `FikaClientFirearmController`
herda o caminho (patch pega no cliente); `ObservedFirearmController` não chama `method_57` (tiro remoto nunca
credita). Quirk vanilla anotado (flare consome `float_5` stale) — negligível.

## Histórico

| Data | Evento |
|---|---|
| 2026-07-04 | Review 01 criada (agente adversarial) e 7/7 achados aplicados via `/apply-code-review`; recompile 0/0 |
