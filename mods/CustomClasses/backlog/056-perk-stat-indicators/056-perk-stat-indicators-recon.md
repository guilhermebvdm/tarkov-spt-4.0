# 056 — Indicador de perk em outros stats visíveis (recon)

> **Data:** 2026-07-02<br>
> **Status:** 🟢 Vivo<br>
> **Responsáveis:** Guilherme<br>
> **Referências:** [class-design.md](../../docs/class-design.md)<br>

---

**Objetivo.** Generalizar a "Feature 1" (peso ↔ perk **Pack Mule**) para outros stats: mapear no Assembly do EFT 0.16.x **onde o jogo já exibe um número que um perk/drawback de classe modifica** e decidir **quais valem** o indicador visual "▲/▼ + tooltip" no mesmo padrão do mod (`SkillPanelPatch` = marcador TMP + `HoverTooltipArea`; `MultiplierFormat.Marker`/`PerksCatalog`). Este doc é **read-only** (nenhum código do mod alterado) — só recon + recomendação.

## Nota metodológica (importante — muda o custo de tudo)

Duas descobertas de infra que valem para **qualquer** item de UI daqui pra frente:

1. **A "Feature 1" NÃO tem indicador ▲/▼ hoje.** O único indicador ▲/▼ existente no mod é o `SkillPanelPatch` (tela de **Skills**, ao lado do nome da skill). O Pack Mule ganhou só o **efeito mecânico** (piso em `SkillManager.CarryingWeightRelativeModifier` — `PackMulePatch.cs:26`). Então "generalizar a Feature 1" = **criar** o indicador em painéis de stat que hoje não têm nenhum. O `SkillPanelPatch` é o **molde de implementação** (não um precedente já aplicado a peso).

2. **O decompile offline curado (`references/eft-decompiled/`) NÃO tem a camada `EFT.UI`.** As pastas `EFT.UI*` existem vazias; `SkillPanel`, `HealthParametersPanel`, `CompactCharacteristicPanel`, `InventoryScreen` etc. **não estão lá**. Confirmado: o time resolve alvos de UI via **`ilspycmd` no DLL real** (`013-...-02-spec-tech.md:7` "Refs confirmadas via ilspycmd no DLL real"). **Todas as linhas de UI citadas abaixo vêm de decompile fresco do DLL vivo** `D:\SPT\EscapeFromTarkov_Data\Managed\Assembly-CSharp.dll` (via `ilspycmd -t <FQN>`), não do decompile curado. As linhas do decompile curado (mecânica: getters, `TotalErgonomics`, etc.) continuam válidas e estão marcadas como `[curado]`.

> ⚠️ **Consequência de gating (coop):** todo painel de stat abaixo aparece **fora da raid** (stash/inventário/health tab). O gating de classe do mod (`SkillMultipliers.IsLocalClass`) funciona fora da raid (o `PackMulePatch` já usa isso — `PackMulePatch.cs:44`), mas **não** há `MainPlayer` para checar. Reusar o padrão "gateia só pela classe local" do Pack Mule.

## Distinção-chave: **stat do ITEM** × **stat de RUNTIME do player**

O painel de características da arma (`CompactCharacteristicPanel`) mostra o valor do **atributo do item** (`ItemAttributeClass.Base()`), calculado pelos mods/props da arma — **NÃO** o valor de runtime bufado pelo player. Vários perks patcham o **runtime** (ex.: Bunker em `FirearmController.TotalErgonomics`), que **não** altera o número exibido no painel de item. Isso reprova a maioria dos perks de arma para o indicador "número real anotável", restando o indicador só como **anotação informativa** ("+15% via classe"), não como número correto exibido.

| Onde | O que exibe | Perk mexe nesse número? |
|---|---|---|
| `CompactCharacteristicPanel` (inspeção de item) | `ItemAttributeClass.Base()` — ergo/recuo/peso/velocidade da ARMA, agregado de props+mods | **Não** (perks de arma agem no runtime do player, não no atributo do item) |
| `HealthParametersPanel` (aba Health) — Weight | `TotalWeight` (Current) e `UpperOverweightLimit × CarryingWeightRelativeModifier` (Max) | **Sim** — Pack Mule é exatamente esse `CarryingWeightRelativeModifier` |
| `HealthParametersPanel` — Energy/Hydration | `Energy`/`Hydration` (número) + `EnergyRate`/`HydrationRate` (seta de taxa nativa) | **Sim** (Heavy Frame acelera drain) — mas a seta de taxa **já é nativa** |

## Tabela-mestra

Evidência de UI = DLL vivo via ilspycmd (arquivos salvos no scratchpad da sessão). `EItemAttributeId` e `CompactCharacteristicPanel.SetValues` reproduzíveis com `ilspycmd -t EFT.InventoryLogic.EItemAttributeId` / `-t EFT.UI.CompactCharacteristicPanel`.

| Stat exibido | Perk/drawback que modifica | Onde o jogo mostra (arquivo.cs:linha) | É número anotável? | Vale ▲/▼? | Esforço | Nota |
|---|---|---|---|---|---|---|
| **Peso / limite de carga** (Health tab) | **Pack Mule** (limite ×1.3) | `HealthParametersPanel.cs:68-73` (Show), `:255-260` (method_1), via `HealthParameterPanel.SetParameterValue` → `HealthParameterPanel.cs:72,75` | **Sim** — `Max` já reflete o perk (`CarryingWeightRelativeModifier`, `:64/:251`) | **Sim** | pequeno | Melhor candidato: o número já incorpora o efeito; falta só anotar "de onde vem". Painel plano, **sem** seta nativa. |
| **Energy (fome)** (Health tab) | **Heavy Frame** (drain ×1.3) | `HealthParametersPanel.cs:195` (`_energy.SetParameterValue`), taxa em `:198` (`SetBuffValue(EnergyRate)`) → `BuffableHealthParameterPanel.cs:44-51` | **Sim** (número) + taxa | **talvez** | médio | O jogo **já** mostra seta ▲/▼ nativa da TAXA (`_up/_downBuffArrow`). Perk muda o número/rate mas indicador seria **redundante** com a seta nativa; só agrega atribuição à classe. |
| **Hydration (sede)** (Health tab) | **Heavy Frame** (drain ×1.3) | `HealthParametersPanel.cs:183` (`_hydration.SetParameterValue`), taxa em `:186` (`SetBuffValue(HydrationRate)`) | **Sim** (número) + taxa | **talvez** | médio | Idem Energy — seta nativa de taxa já existe. |
| **Ergonomia da ARMA** (inspeção/modding) | Bunker (+15% ergo, runtime) | `EItemAttributeId.Ergonomics=7`; render em `CompactCharacteristicPanel.SetValues` → `CompactCharacteristicPanel.cs:199,216`; container `ItemSpecificationPanel.cs:769-771,233-235` | **Não** (é o ergo do ITEM; Bunker age em `FirearmController.TotalErgonomics` runtime — `ClassWeaponPatches.cs:171` [curado]) | **não** | médio | Número exibido ≠ valor bufado. Indicador daria número **errado** ou vira só rótulo informativo. Gate de arma pesada + "arma na mão" nem existe fora da raid. |
| **Recuo da ARMA** (inspeção/modding) | Shaky Hands / Adrenaline / Bunker (runtime) | `EItemAttributeId.Recoil=33`, `RecoilUp=49`, `RecoilBack=50`, `CenterOfImpact=32`; mesmo render `CompactCharacteristicPanel.cs:199-216` | **Não** (recuo do ITEM; perks agem em `ProceduralWeaponAnimation.Shoot(str)` runtime — `ClassWeaponPatches.cs:22` [curado]) | **não** | médio | Idem ergo: número do item, não do disparo bufado. Adrenaline é **transiente** (janela 25s) — pior ainda p/ um painel fora da raid. |
| **Dano de melee** (inspeção de faca) | **Execution** (dano ×5) | `EItemAttributeId.KnifeHitSlashDam=25`, `KnifeHitStabDam=26`; render `CompactCharacteristicPanel.cs:199-216` via `ItemSpecificationPanel` | Parcial — número do ITEM (faca), não bufado; Execution multiplica no dano aplicado (fora do decompile curado, 🟡) | **não** | médio | O ×5 não incide no atributo da faca exibido. Anotação seria informativa, não o número real. |
| **ADS / tempo de mira** | Sharpshooter (ADS ×0.85), Iron Lungs, Adrenaline | **Não existe atributo de ADS/aim-time** no `EItemAttributeId` (enum completo, `EItemAttributeId.cs:1-185` — sem membro AimTime/ADSTime) | **Não exibido** | **não** | — | O jogo não mostra tempo de ADS como número em painel nenhum. Perk age em `ProceduralWeaponAnimation.UpdateWeaponVariables` (`ClassWeaponPatches.cs:125` [curado]). |
| **Recuo/aim-punch/sway (mira)** | Rattled, Cool Under Fire, Iron Lungs sway | Sem atributo de UI; efeitos de câmera/mãos em runtime (`ForceEffector.AddForce` — `ClassWeaponPatches.cs:235` [curado]) | **Não exibido** | **não** | — | Feedback é sensorial (câmera treme), sem número em UI. |
| **Ruído do player** | Loud Operator, Ghost Step, Silent Looter | `EItemAttributeId.Loudness=40` existe mas é da ARMA/silenciador; ruído do **player** (passos/ações) não tem atributo de UI | **Não exibido** (o do player) | **não** | — | Loudness=40 mede a arma, não o perk (que afeta ruído do player). Sem número do ruído do player. |
| **Dano recebido** | Bulwark (−15%) | Sem UI numérica; dano é aplicado em `ActiveHealthController.ApplyDamage` (runtime) | **Não exibido** | **não** | — | Não há painel de "resistência a dano %". |
| **Velocidade de movimento** | Heavy Frame (−10%), Rooted, Execution | Sem número de velocidade em UI de personagem (só `ChangeMovementSpeed=5`/`ChangeTurningSpeed=6` de armadura, exibidos na PEÇA, não do player agregado) | **Não** (é o atributo da armadura, não a velocidade do player) | **não** | — | Perks de move-speed patcham `MovementContext.MaxSpeed` runtime; nenhum painel mostra a velocidade final do player como número. |

## Ranking dos "vale a pena"

### 1. 🥇 Peso / limite de carga — Pack Mule (esforço: pequeno)
- **Onde:** `HealthParametersPanel.cs:68-73` (delegate em `Show`), `:255-260` (`method_1`), texto final em `HealthParameterPanel.SetParameterValue` → `HealthParameterPanel.cs:72` (`_currentValue`) e `:75` (`_maxValue`, formato `"/{Maximum:0}"`).
- **Por que vale:** é o **único** caso onde o número exibido **já incorpora o efeito do perk** — o `Max` sai de `UpperOverweightLimit × (skillManager.CarryingWeightRelativeModifier × health.CarryingWeightRelativeModifier)` (`:66-67/:253-254`), e o `PackMulePatch` postfixa exatamente esse `CarryingWeightRelativeModifier` (`PackMulePatch.cs:26`). Ou seja: o +30% **já aparece** no limite; falta só o marcador "▲ via Classe X".
- **Ponto de patch provável:** Postfix em `HealthParametersPanel.method_0` (ou no delegate de `OnWeightUpdated`) para anexar um marcador TMP ao lado do `_weight` (`_maxValue`), reusando `MultiplierFormat.Marker` + `HoverTooltipArea` (padrão `SkillPanelPatch.cs:78-107`). Fecha o par "Feature 1" (efeito já existe, indicador não).
- **Padrão a reusar:** `SkillPanelPatch` (marcador reusável + tooltip resolvido 1×).
- **Caveat:** painel `HealthParameterPanel` (peso) é **plano** — não tem seta nativa, então o marcador não colide com nada. Precisa achar o TMP `_maxValue`/`_currentValue` do sub-panel `_weight` por reflection.

### 2. 🥈 Energy / Hydration — Heavy Frame (esforço: médio; classificação **talvez**)
- **Onde:** `HealthParametersPanel.cs:195` (Energy), `:183` (Hydration); taxa via `BuffableHealthParameterPanel.SetBuffValue` → `BuffableHealthParameterPanel.cs:44-51` (`_upBuffArrow`/`_downBuffArrow`/`_buffValue`).
- **Por que só "talvez":** esses painéis **já têm seta ▲/▼ nativa** da **taxa** de drain (`EnergyRate`/`HydrationRate`). Se o Heavy Frame acelera o drain, a seta nativa vermelha já reage. Um indicador do mod seria **redundante** visualmente — o valor agregado da atribuição ("−30% via Estrutura Pesada") só ajudaria como **tooltip**, não como seta nova.
- **Recomendação:** se implementar, **não** adicionar seta; só um `HoverTooltipArea` no ícone de Energy/Hydration com o texto de atribuição. Menor risco, menor ganho.

## Descartados (número não exibido / transiente / número errado)

- **Ergonomia / recuo da arma (Bunker/Shaky Hands/Adrenaline):** o painel mostra o atributo do **item** (`ItemAttributeClass.Base()`, `CompactCharacteristicPanel.cs:197,199`), não o valor de runtime que o perk buffa → indicador mostraria número **não correspondente**. Adrenaline é transiente (janela).
- **ADS / tempo de mira (Sharpshooter/Iron Lungs):** **não existe** atributo de ADS no `EItemAttributeId` (enum completo verificado) → o jogo nunca mostra esse número.
- **Aim-punch / sway / flinch (Rattled/Cool Under Fire):** feedback sensorial (câmera), sem número em UI.
- **Ruído do player (Loud Operator/Ghost Step/Silent Looter):** `Loudness=40` é da arma; ruído do player não tem atributo de UI.
- **Dano recebido (Bulwark):** sem painel de resistência numérica.
- **Velocidade de movimento (Heavy Frame/Rooted/Execution):** nenhum painel mostra a velocidade final do player; `ChangeMovementSpeed=5` é atributo da **peça de armadura**, não do player agregado.
- **Dano de melee (Execution):** `KnifeHitSlashDam/StabDam` existem como atributo da faca, mas o ×5 do Execution incide no dano aplicado (runtime), não no atributo exibido → número não bate.

## Conclusão / recomendação

Ordem sugerida para virar item/spec:

1. **Peso ↔ Pack Mule (health tab)** — único caso onde o número exibido **já** reflete o perk. Fecha literalmente a "Feature 1" (dá a ela o indicador que o nome do item 056 já pressupõe). Esforço pequeno, ganho claro, mesmo padrão do `SkillPanelPatch`. **Fazer primeiro.**
2. **Energy/Hydration ↔ Heavy Frame (health tab)** — só como **tooltip de atribuição** (a seta de taxa já é nativa). Opcional, baixo ganho. Avaliar se vale o esforço médio.
3. **Descartar explicitamente** ergo/recuo/ADS/melee/ruído/velocidade como indicador de número — ou o stat não é exibido, ou o número exibido é do item (não bufado pelo perk). Se ainda quiser sinalizar esses, o lugar certo continua sendo a **aba Perks/Drawback (053/059)**, não os painéis de stat vanilla.

**Resumo do escopo real:** de ~12 candidatos, **1 "sim" forte** (peso), **2 "talvez" fracos** (energy/hydration, só tooltip), e o resto **descartado** por não-exibição ou número do-item-não-do-player. O item 056 é bem menor do que a lista de candidatos do backlog sugeria — a maioria dos stats de arma/movimento **não é exibida como número editável pelo perk**.

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-02 | Guilherme | Criação (recon do item 056). Evidência de UI via ilspycmd no DLL vivo (o decompile curado não tem `EFT.UI`). Achado: só peso (Pack Mule) é "sim" forte; energy/hydration "talvez" (seta nativa); ergo/recuo/ADS/melee/ruído/velocidade descartados. |
