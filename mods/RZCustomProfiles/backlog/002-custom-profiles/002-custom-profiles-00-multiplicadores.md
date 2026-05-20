# 002 — Modelo de balanceamento (multiplicadores por skill)

> Artefato de planejamento dedicado ao **modelo de custo ponderado** usado pelo item 002. Evolução da seção "Modelo de balanceamento" do [planejamento histórico do item 001](../001-custom-profiles/001-custom-profiles-00-planejamento.md), aplicando:
>
> - Remoção das 20 skills mortas no SPT 4.0.13 (`globals.json` com `[]`).
> - Categorização explícita por aba in-game (Ph / M / C / P).
> - **Clamp ampliado de `[0.25, 3.00]` para `[0.25, 5.00]`** — skills muito raras (Immunity, LightVests, HeavyVests etc.) tinham seu custo subestimado pelo teto antigo. O personagem-referência lvl 43 tinha essas em nível 4 → mult real = `15/4 = 3.75`, dentro do novo clamp.

## Fórmula

```text
multiplicador_skill = BASELINE / nivel_observado_no_lvl_43
custo_skill          = nivel_atribuído × multiplicador_skill
custo_classe         = Σ custo_skill
```

- `BASELINE = 15` (mediana das skills neutras — Assault e Endurance ficaram exatamente nesse valor).
- Resultado em **`[0.25, 5.00]`** por clamp (faixa ampliada vs. item 001).
- Arredondado a 2 casas decimais.

## Multiplicadores (apenas skills vivas no SPT 4.0.13)

Personagem de referência: lvl 43 do usuário, screenshots em [../../assets/](../../assets/).

### Ph — Physical

| Skill | Nível @ lvl 43 | Mult |
|-------|---------------:|-----:|
| `Metabolism` | 51 (ELITE) | 0.29 |
| `Strength` | 32 | 0.47 |
| `StressResistance` | 17 | 0.88 |
| `Endurance` | 15 | 1.00 |
| `Vitality` | 9 | 1.67 |
| `Health` | 9 | 1.67 |
| `Immunity` | 4 | **3.75** *(antes clamp 3.00)* |

### M — Mental

| Skill | Nível @ lvl 43 | Mult |
|-------|---------------:|-----:|
| `Attention` | 25 | 0.60 |
| `Intellect` | 22 | 0.68 |
| `Perception` | 17 | 0.88 |
| `Memory` | — | 0.50 *(premissa: sobe junto com todas as outras skills)* |
| `Charisma` | — | 0.40 *(premissa: passiva em trades/quests)* |

### C — Combat

| Skill | Nível @ lvl 43 | Mult |
|-------|---------------:|-----:|
| `Revolver` | 21 | 0.71 |
| `Throwing` | 18 | 0.83 |
| `Assault` | 15 | 1.00 *(baseline)* |
| `AimDrills` | 13 | 1.15 |
| `Melee` | 8 | 1.88 |
| `Shotgun` | 6 | 2.50 |
| `TroubleShooting` | 6 | 2.50 |
| `Pistol` | 5 | 3.00 |
| `DMR` | 4 | **3.75** *(antes clamp 3.00)* |
| `Sniper` | — | 1.50 *(premissa: mastering raro de bolt-action longo)* |
| `RecoilControl` | — | 1.00 *(premissa: sobe atirando — baseline)* |

### P — Practical

| Skill | Nível @ lvl 43 | Mult |
|-------|---------------:|-----:|
| `Crafting` | 45 | 0.33 |
| `HideoutManagement` | 38 | 0.39 |
| `Search` | 35 | 0.43 |
| `CovertMovement` | 16 | 0.94 |
| `MagDrills` | 16 | 0.94 |
| `Surgery` | 12 | 1.25 |
| `WeaponTreatment` | 12 | 1.25 |
| `LightVests` | 4 | **3.75** *(antes clamp 3.00)* |
| `HeavyVests` | 4 | **3.75** *(antes clamp 3.00)* |

## Skills mortas em SPT 4.0.13 (referência negativa)

Não usar. Em `D:/SPT/SPT/SPT_Data/database/globals.json` aparecem como `[]` (configuração vazia, sem efeito no jogo) — `[fonte externa: SPT_Data/globals.json]`.

| Categoria | Skills mortas |
|-----------|--------------|
| Combat | `SMG`, `LMG`, `HMG`, `Launcher`, `AttachedLauncher` |
| Practical | `Sniping`, `ProneMovement`, `FieldMedicine`, `FirstAid`, `WeaponModding`, `AdvancedModding`, `NightOps`, `SilentOps`, `Lockpicking` |
| Trading | `Freetrading`, `Auctions`, `Cleanoperations`, `Barter`, `Shadowconnections`, `Taskperformance` |

## Impacto do clamp ampliado (3.00 → 5.00)

5 skills tiveram seu custo aumentado:

| Skill | Mult antigo | Mult novo | Aumento |
|-------|------------:|----------:|--------:|
| `Immunity` | 3.00 | 3.75 | +25% |
| `LightVests` | 3.00 | 3.75 | +25% |
| `HeavyVests` | 3.00 | 3.75 | +25% |
| `DMR` | 3.00 | 3.75 | +25% |
| `Pistol` | 3.00 *(era exato no limite, nível 5)* | 3.00 *(sem mudança — `15/5 = 3.00`)* | 0% |

Classes potencialmente afetadas pelas mudanças (não bloqueador, ajustar custo total na composição):

- **Sobrevivencialista** — usa `Immunity`. Cada ponto agora custa 3.75 em vez de 3.00.
- **Operador Tático** — usa `LightVests`. Cada ponto 3.75.
- Outras classes não usam essas skills nas propostas atuais.

## Recalibração futura

Quando um personagem de referência mais recente estiver disponível, regerar a tabela:

1. Pegar screenshots de Character → Skills em todas as 4 abas (Ph/M/C/P).
2. Atualizar a coluna "Nível @ lvl X" + recalcular multiplicadores.
3. Premissas (skills não observadas) podem ser substituídas por observações reais.
4. Manter clamp `[0.25, 5.00]` salvo decisão explícita em contrário.

## Histórico

| Data | Evento |
|---|---|
| 2026-05-17 | Arquivo criado — cleanup das 20 skills mortas + clamp 3.00→5.00 + categorização Ph/M/C/P |
