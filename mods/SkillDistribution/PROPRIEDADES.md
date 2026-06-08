# Propriedades F12 — SkillDistribution (cliente)

> **Plugin:** `com.zgfuedkx.skilldistribution` (display: `ZGFueDkx-SkillDistribution`) · **Versão:** 1.2.2<br>
> **Fonte:** [original/SkillDistributionClient/Plugin.cs](original/SkillDistributionClient/Plugin.cs) · binds em [original/SkillDistributionClient/Helpers/Settings.cs](original/SkillDistributionClient/Helpers/Settings.cs)<br>
> **Aba no F12:** `SkillDistribution`

> ⚠️ **Nota técnica:** este mod **não** chama `Config.Bind(...)` diretamente. Usa um wrapper da biblioteca externa `ZGFueDkxCommonLibrary` (`ZGFueDkx.ZGCLib.Config`, em [ZGFueDkxCommonLibrary/Config/](ZGFueDkxCommonLibrary/Config/)): `config.MakeCategory(order, nome)` cria a seção e `category.BindConfig(nome, default, descrição)` / `category.BindButton(...)` registram as entradas. O nome real da seção exposto ao BepInEx é **`"{order}. {nome}"`** (ex.: `Config.Bind("1. General config", ...)`) — é assim que aparece no F12 e no arquivo `BepInEx/config/com.zgfuedkx.skilldistribution.cfg`. Dentro de cada seção, `Order = order*1000` decrescente garante que a ordem de bind = ordem de exibição (topo → base).
>
> Nenhuma entrada está marcada como **(Avançado)** — o parâmetro `hide` (→ `IsAdvanced`) é `false` em todas as chamadas, então todas aparecem no F12 sem precisar ligar "Advanced settings".

---

## Seção: `1. General config`

| # | Nome (EN) | Tradução (pt-BR) | Tipo | Padrão | Faixa | Tooltip (pt-BR) |
|---|-----------|------------------|------|--------|-------|-----------------|
| 1 | Experience distribution mode | Modo de distribuição de experiência | enum `EDistributionMode` | `WeightedRandomMax` | `Equal`, `RoundRobin`, `Random`, `WeightedRandomMin`, `WeightedRandomMax`, `Min`, `Max` | Determina como a experiência das skills é distribuída.<br>**Equal** — toda a XP é distribuída igualmente entre as skills (se não houver XP suficiente, usa Random).<br>**RoundRobin** — distribui XP para uma skill após a outra, de forma cíclica.<br>**Random** — distribui XP para skill(s) aleatória(s).<br>**WeightedRandomMin** — skill(s) aleatória(s), skills de nível mais **baixo** têm maior chance.<br>**WeightedRandomMax** — skill(s) aleatória(s), skills de nível mais **alto** têm maior chance.<br>**Min** — distribui para a(s) skill(s) de nível mais baixo.<br>**Max** — distribui para a(s) skill(s) de nível mais alto. |
| 2 | Skills count | Quantidade de skills | `int` | `3` | — | Número de skills para as quais a experiência será distribuída. |
| 3 | Allow gym | Permitir academia | `bool` | `true` | — | Se a XP da academia também deve ser distribuída quando força/endurance estiverem no máximo. |
| 4 | Use bonuses | Usar bônus | `bool` | `true` | — | Se a XP distribuída deve usar os bônus da skill de destino. |
| 5 | Use effectiveness | Usar efetividade | `bool` | `true` | — | Se a XP distribuída deve usar e causar fadiga (effectiveness) na skill de destino. |
| 6 | Cause fatigue | Causar fadiga | `bool` | `true` | — | Se a XP distribuída deve causar fadiga na skill de destino quando `use_effectiveness` for false. Esta opção não tem efeito se "Use effectiveness" estiver ativo. |
| 7 | Experience multiplier | Multiplicador de experiência | `float` | `1.0` | — | Multiplicador da XP distribuída. Use para aumentar ou diminuir a XP que é distribuída. Acumula com o multiplicador por skill. |
| 8 | Experience multiplier (gym) | Multiplicador de experiência (academia) | `float` | `1.0` | — | Multiplicador da XP distribuída a partir do treino na academia. |
| — | Reset to server values | Restaurar valores do servidor | botão (`Reset`) | — | — | Puxa as configurações do servidor e as aplica. (Sem efeito se `allow_override` do servidor for false.) |

## Seção: `2. Skill Multipliers`

| # | Nome (EN) | Tradução (pt-BR) | Tipo | Padrão | Faixa | Tooltip (pt-BR) |
|---|-----------|------------------|------|--------|-------|-----------------|
| 1 | Enable multipliers | Ativar multiplicadores | `bool` | `false` | — | Se deve aplicar os multiplicadores listados abaixo (não se aplica ao treino de academia — use "Experience multiplier (gym)"). |
| 2 | `[skill]` multiplier | Multiplicador de `[skill]` | `float` | `1.0` | — | Define o multiplicador de XP distribuída quando a skill `[skill]` (em nível elite) é a fonte da XP. Acumula com o multiplicador global de experiência. |

> A linha **`[skill]` multiplier** é **dinâmica**: gerada em runtime por `Settings.BuildMultipliers()` para cada skill não-bloqueada do `DisplayList` do perfil do jogador (uma entrada por skill).

## Seção: `9. Debug`

| # | Nome (EN) | Tradução (pt-BR) | Tipo | Padrão | Faixa | Tooltip (pt-BR) |
|---|-----------|------------------|------|--------|-------|-----------------|
| 1 | Debug logs | Logs de debug | `bool` | `false` | — | Registra informações de debug no `Player.log`. |

---

## Config do servidor (não-F12)

Além do F12, há config server-side editável em `SPT/user/mods/SkillDistribution/config/config.jsonc` (fonte: [original/SkillDistributionServer/config/config.jsonc](original/SkillDistributionServer/config/config.jsonc)). Espelha as opções acima em snake_case e adiciona:

- `allow_override` (bool, default `true`) — se o cliente pode sobrescrever a config do servidor. Recomendado `true` para vanilla, `false` para Fika. Quando `true`, mudanças no `config.jsonc` **não afetam** os jogadores.
- `skill_multipliers` (objeto) — multiplicadores por skill (35 skills listadas, todas default `1.0`).
