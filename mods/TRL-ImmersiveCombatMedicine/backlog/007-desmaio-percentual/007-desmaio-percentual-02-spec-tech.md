# 007 — Desmaio 2.0: gatilhos percentuais · Spec Técnica

**Mod:** TRL-ImmersiveCombatMedicine
**Spec funcional:** [007-desmaio-percentual-01-spec.md](007-desmaio-percentual-01-spec.md)
**Criado:** 2026-07-19

> Fonte primária de verdade para qualquer assinatura, fórmula ou ponto de patch: [references/eft-decompiled/Assembly-CSharp/](../../../../references/eft-decompiled/Assembly-CSharp/). Toda referência ao código do EFT cita `arquivo.cs:linha`. Wiki SPT e fontes externas só como complemento.

## 1. Estratégia

Manter o **único** patch Harmony já existente em `Player.ApplyDamageInfo` (virtual, [`Player.cs:30463`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L30463)) — Prefix de alta prioridade + Postfix de baixa prioridade, ambos em `DamageTriggerPatch` (`modded/Patches/Trauma/HealthPatches.cs`). Nenhum patch novo é criado.

O único ponto tocado é a **condição de entrada** do desmaio, dentro do bloco `if (ConfigBlackoutEnabled.Value)` do Postfix: o limiar fixo absoluto (`damageInfo.Damage >= 35f` tórax / `>= 10f` cabeça, sem gate de analgésico) é **removido** e substituído por uma chamada a um novo helper estático (`TraumaBlackoutTrigger.Evaluate`) que compara o dano efetivo do hit contra a vida da parte **imediatamente antes** daquele hit.

**Decisão técnica central (a pesquisa mais importante deste item):** dentro do corpo original de `ApplyDamageInfo`, `ActiveHealthController.ApplyDamage(bodyPartType, damageInfo.Damage, damageInfo)` ([`Player.cs:30480`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L30480)) **já mutou** o HP da parte antes que qualquer Postfix rode — logo, no Postfix, `GetBodyPartHealth(bodyPartType).Current` reflete o HP **pós-hit**, nunca o pré-hit. A opção de reconstrução aritmética (`preHp = postHp + damageInfo.Damage`) foi **descartada**: `damageInfo.Damage` é pré-clamp e, em overkill (dano > HP restante), infla o preHp calculado; e o retorno de `ApplyDamage` (`damageInfo.DidBodyDamage`) é o delta **agregado de `EBodyPart.Common`** (inclui spill de overdamage para outras partes), não o dano na parte atingida — achado já confirmado via `ilspycmd` contra o assembly real em pesquisa anterior do mod (`docs/trauma-primitives.md` §P7, evidência `scratchpad/spike001/ActiveHealthController.cs:3721-3848`, via `ilspycmd_assembly_real`).

A solução adotada é **capturar o HP pré-hit no Prefix** (antes do corpo original rodar) e **passá-lo ao Postfix via parâmetro especial do Harmony `__state`** — não uma solução com campo estático (mais frágil a reentrância) nem com aritmética reversa (distorcida em overkill). `DoWoundRelapse` ([`Player.cs:30475`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L30475)), que roda antes de `ApplyDamage` dentro do mesmo método, só acumula `Wound.BuildUp` — sem mudança síncrona de HP — então o Prefix vê o valor pré-tiro exato.

**Sem agregação de pellets "de graça":** `GameWorld.ShotDelegate(EftBulletClass shotResult)` ([`GameWorld.cs:1966`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/GameWorld.cs#L1966)) processa **um `EftBulletClass` por projétil** (cada pellet de espingarda é sua própria instância) e chama `shotResult.HittedBallisticCollider.ApplyHit(...)` — que, em `BodyPartCollider.ApplyHit` ([`BodyPartCollider.cs:324`](../../../../references/eft-decompiled/Assembly-CSharp/BodyPartCollider.cs#L324)), invoca `PlayerBridge.ApplyShot` → `Player.ApplyShot` ([`Player.cs:30404`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L30404)) → `ApplyDamageInfo` ([`Player.cs:30432`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L30432)) **uma vez por pellet**. Cada chamada tem seu próprio par Prefix/Postfix — o Prefix do pellet N sempre lê o HP já reduzido pelos pellets 1..N-1 (mutados sequencialmente na mesma frame). Não há agregação a implementar: o ponto de patch escolhido já garante avaliação por hit.

## 2. Pontos de patch

| Alvo (Assembly) | Tipo | Motivo |
|---|---|---|
| [`Player.cs:30463`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L30463) (`Player.ApplyDamageInfo`, virtual) | Prefix (`Priority.High`, já existente) | Captura `__state` = HP pré-hit da parte (só Chest/Head) via `GetBodyPartHealth`, ANTES de `ActiveHealthController.ApplyDamage` mutar o valor. Mantém o escudo de dano do desmaio já existente (linhas 16-31 do arquivo atual), inalterado. |
| [`Player.cs:30463`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L30463) (`Player.ApplyDamageInfo`, virtual) | Postfix (`Priority.Low`, já existente) | Recebe `__state`; relê HP pós-hit; calcula `danoEfetivo = __state − postHp`; chama `TraumaBlackoutTrigger.Evaluate` (substitui o limiar fixo). Pipeline pós-gatilho (`BlackoutTimers`, prone, `FikaBridge.SyncFaintStatus`) inalterado. |

Nenhum patch novo. `GetBodyPartHealth` não é um método patcheado — é uma **consulta** de `IHealthController`, já usada extensivamente no mod (`MedicalLogic.cs`, `BandAidNetworkHandler.cs`, `TourniquetManager.cs:157`, `BandAidUI.cs:815`, `MedicHealPatch.cs:411`), com assinatura (`public ValueStruct GetBodyPartHealth(EBodyPart bodyPart, bool rounded = false)`) provada por protótipo compilado contra o assembly real (`docs/trauma-primitives.md` §P7, "Provas por protótipo" — `ilspycmd_assembly_real`; `ActiveHealthController` **não está presente** no dump local, AP-09). **PA-01-01 (review 1):** a citação original apontava `GClass921.cs:1143` — essa classe é, pela tabela de deofuscação, `ObservedPlayerHealthController` (controller do ESPELHO/observado), cujo `GetBodyPartHealth` é um stub que lança `NotImplementedException` (`GClass921.cs:1145`); a assinatura bate por coincidência (membro da interface `IHealthController`, reimplementada em várias classes concretas), mas NÃO é a classe real invocada pelo mod (`ActiveHealthController`, ausente do dump). Citação corrigida para a fonte real acima — nenhuma outra spec deve reusar `GClass921.cs:1143` como evidência de `ActiveHealthController`.

## 3. Novas propriedades F12 (BepInEx)

**Mapeamento Campo C# ↔ Nome (EN) (PA-02-02, review 2 — os nomes de campo NÃO são tradução literal do nome exibido, ao contrário dos itens 003-006; declarar explicitamente evita mismatch entre esta tabela e o stub §5):**

| Seção | Campo C# (`ConfigEntry<T>`) | Nome (EN) | Tipo | Padrão | Faixa | Avançado | Tooltip (pt-BR) |
|---|---|---|---|---|---|---|---|
| `6. Trauma 2.0 (Consumidores)` | `ConfigConsumerBlackout2` (já existe — rename-at-delivery) | `Blackout 2.0` *(RENAME de `Blackout 2.0 (item 007)`)* | bool | `true` | — | — | Gatilho percentual de desmaio (item 007): tórax ≥50% da vida atual (piso 25 de dano absoluto) rola p=50%, imune sob analgésico; cabeça ≥25% da vida atual (piso 10) rola p=50%, p=25% sob analgésico. Governado pelo master "Sistema de Desmaio" — este toggle decide SÓ a lógica de entrada (percentual ou nenhuma); o limiar fixo legado NÃO volta mesmo desligado. |
| `11. Trauma 2.0 (Desmaio)` | `ConfigBlackoutChestPercent` | `Chest Faint Percent Threshold` | float | `50` | 0–100 | — | % da vida ATUAL do tórax (pré-tiro) que um hit precisa remover para rolar desmaio (p=50%; imune sob analgésico — decisão 9). Precisa TAMBÉM atingir o piso absoluto abaixo (decisão 15). |
| `11. Trauma 2.0 (Desmaio)` | `ConfigBlackoutHeadPercent` | `Head Faint Percent Threshold` | float | `25` | 0–100 | — | % da vida ATUAL da cabeça (pré-tiro) que um hit precisa remover para rolar desmaio (p=50% sem analgésico, p=25% sob analgésico — cabeça NÃO fica imune). Precisa TAMBÉM atingir o piso absoluto abaixo. |
| `11. Trauma 2.0 (Desmaio)` | `ConfigBlackoutChestAbsoluteFloor` | `Chest Faint Absolute Damage Floor` | float | `25` | 0–100 | — | Piso de segurança (decisão 15): dano ABSOLUTO mínimo no hit do tórax, além do percentual acima — evita desmaio por hit percentualmente grande mas fisicamente insignificante (ex.: 5 de dano em tórax com 8 de vida = 62% mas só 5 de dano). |
| `11. Trauma 2.0 (Desmaio)` | `ConfigBlackoutHeadAbsoluteFloor` | `Head Faint Absolute Damage Floor` | float | `10` | 0–100 | — | Piso de segurança (decisão 15): dano ABSOLUTO mínimo no hit da cabeça, além do percentual acima. |

As probabilidades de roll (p=50% tórax, p=50%/25% cabeça) são **constantes fixas no código** — a spec funcional lista exatamente 4 números configuráveis (2 percentuais + 2 pisos), não as chances de roll. Documentado para evitar ambiguidade (AP-05).

**Estado neutro:** `Blackout 2.0` = `false` ⇒ nenhum desmaio dispara por dano (o bloco `if (ConfigBlackoutEnabled.Value)` do Postfix roda, mas `shouldFaint` nunca fica `true` sem o sub-toggle — o limiar fixo legado foi removido do arquivo, não existe fallback). `ConfigBlackoutEnabled` = `false` (master) desliga tudo, como hoje.

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `modded/Patches/Trauma/TraumaBlackoutTrigger.cs` | CRIAR | Helper estático stateless: `Evaluate(Player, EBodyPart, float preHitHp)` — piso absoluto, percentual, gate de analgésico (`TraumaEngine.IsUnderPainkiller`), roll `Random.value`, log `[Blackout2]`. |
| `modded/Patches/Trauma/HealthPatches.cs` | MODIFICAR | Prefix ganha `EBodyPart bodyPartType, out float __state` (captura HP pré-hit p/ Chest/Head). Postfix ganha `float __state`; bloco `isChestTrauma`/`isHeadTrauma` (limiar fixo) é REMOVIDO e substituído pela chamada a `TraumaBlackoutTrigger.Evaluate`, gated por `ConfigConsumerBlackout2`. |
| `modded/TRLImmersiveCombatMedicinePlugin.cs` | MODIFICAR | 4 `ConfigEntry<float>` novas (seção 11); rename-at-delivery de `ConfigConsumerBlackout2` (default `true`); bloco novo em `MigrateOrphanedConfigKeys` deletando o órfão `"Blackout 2.0 (item 007)"` — **PA-02-03 (review 2):** template literal mais novo a replicar é o bloco "Stomach Effects (item 006)" em `TRLImmersiveCombatMedicinePlugin.cs:407-428` (troca mecânica de `section`/`key`/mensagens de log para `"Blackout 2.0 (item 007)"` → `"Blackout 2.0"`, mesmo padrão delete-antes-do-save sem copiar valor — lição CR-03-01); bump de versão do `BepInPlugin` (`/code-mod`). |
| `PROPRIEDADES.md` | MODIFICAR | Seção 11 nova (4 entries); linha nova na tabela "Renomeadas" (`Blackout 2.0 (item 007)` → `Blackout 2.0`); **PA-02-04 (review 2):** linha JÁ EXISTENTE `Blackout 2.0 (item 007)` na seção 6 atualizada (nome → `Blackout 2.0`, padrão `false`→`true`, tooltip placeholder → tooltip real do `Config.Bind`) — mesmo padrão que o `Stomach Effects` do item 006 aplicou (`PROPRIEDADES.md:70`). |
| `TraumaEngine.cs` / `TraumaEngineState.cs` | NENHUMA MUDANÇA | Ver §7 — decisão de NÃO registrar em `TraumaConsumerRegistry` (justificada abaixo). |

## 5. Stubs de código

```csharp
// modded/Patches/Trauma/HealthPatches.cs (trechos alterados)
using HarmonyLib;
using EFT;
using UnityEngine;
using TRLImmersiveCombatMedicine;
using TRLImmersiveCombatMedicine.Trauma;

namespace TrueTrauma
{
    [HarmonyPatch(typeof(Player), "ApplyDamageInfo")]
    public static class DamageTriggerPatch
    {
        // PREFIX: escudo de dano (inalterado) + captura de HP PRÉ-tiro p/ o gatilho percentual (item 007)
        [HarmonyPriority(Priority.High)]
        static bool Prefix(Player __instance, DamageInfoStruct damageInfo, EBodyPart bodyPartType, out float __state)
        {
            // ref: Player.cs:30475-30480 — ActiveHealthController.ApplyDamage MUTA o HP da parte dentro do
            // corpo original; capturar AQUI (Prefix, antes do corpo rodar) é o único jeito de ver o valor
            // PRÉ-tiro exato, sem a distorção de overkill que "postHp + damageInfo.Damage" teria.
            // ref: docs/trauma-primitives.md §P7 (ilspycmd_assembly_real) — DidBodyDamage é delta de
            // EBodyPart.Common com spill, NÃO o dano na parte; descartado como fonte.
            __state = -1f;
            if (bodyPartType == EBodyPart.Chest || bodyPartType == EBodyPart.Head)
            {
                var ahc = __instance?.ActiveHealthController; // ref: Player.cs:25291
                if (ahc != null) __state = ahc.GetBodyPartHealth(bodyPartType).Current; // ref: docs/trauma-primitives.md §P7 (ActiveHealthController.GetBodyPartHealth — protótipo compilado, PA-01-01)
            }

            if (!TRLImmersiveCombatMedicinePlugin.ConfigMasterEnabled.Value) return true;
            if (__instance == null || !__instance.HealthController.IsAlive) return true;

            if (!__instance.IsAI && TraumaState.FaintedPlayerIds.Contains(__instance.ProfileId))
            {
                if (damageInfo.DamageType == EDamageType.Bullet
                    || damageInfo.DamageType == EDamageType.Explosion
                    || damageInfo.DamageType == EDamageType.GrenadeFragment
                    || damageInfo.DamageType == EDamageType.Landmine
                    || damageInfo.DamageType == EDamageType.Sniper)
                {
                    return false;
                }
            }
            return true;
        }

        // POSTFIX: onde o desmaio é calculado (limiar fixo legado REMOVIDO — item 007)
        [HarmonyPriority(Priority.Low)]
        static void Postfix(Player __instance, DamageInfoStruct damageInfo, EBodyPart bodyPartType, float __state)
        {
            if (__instance == null || !__instance.HealthController.IsAlive) return;
            if (!TRLImmersiveCombatMedicinePlugin.ConfigMasterEnabled.Value) return;

            float now = Time.time;
            string id = __instance.ProfileId;

            bool isValidTraumaType = damageInfo.DamageType == EDamageType.Bullet ||
                                     damageInfo.DamageType == EDamageType.Explosion ||
                                     damageInfo.DamageType == EDamageType.Sniper ||
                                     damageInfo.DamageType == EDamageType.Landmine ||
                                     damageInfo.DamageType == EDamageType.GrenadeFragment;

            // 1. LÓGICA DE DESMAIO (ConfigBlackoutEnabled continua MASTER de todo o pipeline — decisão da spec funcional)
            if (TRLImmersiveCombatMedicinePlugin.ConfigBlackoutEnabled.Value)
            {
                if (TraumaState.BlackoutTimers.ContainsKey(id) || TraumaState.FaintedPlayerIds.Contains(id)) return;
                if (TraumaState.BotFaintCooldowns.TryGetValue(id, out float cdUntil) && now < cdUntil) return;

                // ref: spec 007 — limiar fixo (>=35 tórax / >=10 cabeça, sem gate de analgésico) REMOVIDO;
                // ConfigConsumerBlackout2 é o ÚNICO gatilho restante (sub-toggle da lógica de entrada).
                // PA-02-05 (review 2): filtro de domínio (Chest/Head) explícito AQUI — sem isto, Evaluate
                // seria chamado para QUALQUER bodyPartType e dependeria do sentinel __state=-1f (setado só
                // p/ Chest/Head no Prefix) coincidir com o guard "preHitHp<=0f" para produzir o mesmo
                // resultado; explícito aqui, o else de domínio dentro de Evaluate fica puramente defensivo.
                bool shouldFaint = isValidTraumaType
                    && (bodyPartType == EBodyPart.Chest || bodyPartType == EBodyPart.Head)
                    && TRLImmersiveCombatMedicinePlugin.ConfigConsumerBlackout2.Value
                    && TraumaBlackoutTrigger.Evaluate(__instance, bodyPartType, __state);

                if (shouldFaint)
                {
                    float duration = TRLImmersiveCombatMedicinePlugin.ConfigBlackoutDuration.Value;
                    TraumaState.BlackoutTimers[id] = now + duration;
                    TraumaState.BlackoutStartTimes[id] = now;

                    if (__instance.Physical != null) __instance.Physical.Stamina.Current = 0f;
                    __instance.MovementContext.IsInPronePose = true;
                    if (__instance.HandsController is IFirearmHandsController firearm) firearm.SetAim(false);

                    FikaBridge.SyncFaintStatus(__instance, true);
                    return;
                }
            }

            // (blocos de comentário legados de pernas/braços/estômago do arquivo real — inalterados, omitidos aqui)
        }
    }
}
```

```csharp
// modded/Patches/Trauma/TraumaBlackoutTrigger.cs (novo arquivo)
using EFT;
using UnityEngine;

namespace TRLImmersiveCombatMedicine.Trauma
{
    /// <summary>Gatilho percentual de desmaio (spec 007) — substitui o limiar fixo absoluto legado.
    /// STATELESS: sem lifecycle de raid (nada a limpar em GameWorld.OnDestroy/BaseLocalGame.Stop — AP-01 N/A).
    /// Sem registro em TraumaConsumerRegistry (ver spec técnica §7 — nenhuma TraumaRegion cobre tórax/cabeça).</summary>
    internal static class TraumaBlackoutTrigger
    {
        // Constantes de decisão 8/9 (docs/trauma-matrix.md) — NÃO configuráveis: a spec funcional lista
        // exatamente 4 números expostos no F12 (2 percentuais + 2 pisos); as chances de roll são fixas.
        private const float ChestRollChance = 0.5f;
        private const float HeadRollChance = 0.5f;
        private const float HeadRollChancePainkiller = 0.25f;

        /// <summary>Chamado 1x por invocação de ApplyDamageInfo (= 1x por pellet/fragmento — decisão 15,
        /// garantido pelo ponto de patch, ver spec técnica §1/§6). preHitHp vem do __state do Prefix.</summary>
        internal static bool Evaluate(Player player, EBodyPart bodyPartType, float preHitHp)
        {
            if (player == null) return false;
            // Corner (spec funcional): vida pré-tiro já <= 0 (parte destruída por hit anterior no mesmo
            // frame) — não dispara o roll percentual (sem divisão por zero, sem percentual inválido).
            if (preHitHp <= 0f) return false;

            var ahc = player.ActiveHealthController; // ref: Player.cs:25291
            if (ahc == null) return false;

            float postHitHp = ahc.GetBodyPartHealth(bodyPartType).Current; // ref: docs/trauma-primitives.md §P7 (ActiveHealthController.GetBodyPartHealth — protótipo compilado, PA-01-01)
            float effectiveDamage = preHitHp - postHitHp; // pós-armadura/multiplicadores; clamp natural em overkill
            if (effectiveDamage <= 0f) return false;

            // Gate de analgésico NO INSTANTE do hit (motor já reservou este predicado p/ o item 007).
            bool underPainkiller = TraumaEngine.IsUnderPainkiller(player); // ref: TraumaEngine.cs:99

            float pctThreshold;
            float absFloor;
            float rollChance;

            if (bodyPartType == EBodyPart.Chest)
            {
                pctThreshold = TRLImmersiveCombatMedicinePlugin.ConfigBlackoutChestPercent.Value / 100f;
                absFloor = TRLImmersiveCombatMedicinePlugin.ConfigBlackoutChestAbsoluteFloor.Value;
                rollChance = underPainkiller ? 0f : ChestRollChance; // decisão 9 — imunidade TOTAL do tórax
            }
            else if (bodyPartType == EBodyPart.Head)
            {
                pctThreshold = TRLImmersiveCombatMedicinePlugin.ConfigBlackoutHeadPercent.Value / 100f;
                absFloor = TRLImmersiveCombatMedicinePlugin.ConfigBlackoutHeadAbsoluteFloor.Value;
                rollChance = underPainkiller ? HeadRollChancePainkiller : HeadRollChance; // cabeça NÃO é imune
            }
            else
            {
                return false; // domínio do desmaio é só tórax/cabeça (fora de escopo — spec funcional)
            }

            if (effectiveDamage < absFloor)
            {
                LogIgnored(player, bodyPartType, effectiveDamage, preHitHp, "piso absoluto");
                return false;
            }
            if (effectiveDamage < pctThreshold * preHitHp)
            {
                LogIgnored(player, bodyPartType, effectiveDamage, preHitHp, "percentual");
                return false;
            }

            // Extremos determinísticos (mesmo idioma do 006 — TraumaStomachConsumer.cs:73): rollChance=0 nunca sucede.
            bool success = rollChance > 0f && Random.value < rollChance;
            if (TRLImmersiveCombatMedicinePlugin.ConfigVerboseEngineLog.Value)
            {
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo(
                    $"[Blackout2] {player.ProfileId} part={bodyPartType} dmg={effectiveDamage:0.#} preHp={preHitHp:0.#} pk={underPainkiller} chance={rollChance:0.##} success={success}");
            }
            return success;
        }

        private static void LogIgnored(Player player, EBodyPart part, float effectiveDamage, float preHitHp, string reason)
        {
            if (!TRLImmersiveCombatMedicinePlugin.ConfigVerboseEngineLog.Value) return;
            TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo(
                $"[Blackout2] {player.ProfileId} part={part} dmg={effectiveDamage:0.#} preHp={preHitHp:0.#} ignorado ({reason})");
        }
    }
}
```

## 6. Fluxo de dados

```
[A] Pellet/bala atinge um BodyPartCollider  →  [B] Prefix (HealthPatches.cs) captura HP pré-hit  →
[C] Corpo original de ApplyDamageInfo muta o HP  →  [D] Postfix computa dano efetivo + avalia gatilho  →
[E] Pipeline pós-gatilho INTOCADO (BlackoutTimers, prone, sync Fika)
```

1. **[A]** `GameWorld.ShotDelegate(EftBulletClass shotResult)` ([`GameWorld.cs:1966`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/GameWorld.cs#L1966)) roda uma vez por projétil → `BodyPartCollider.ApplyHit` ([`BodyPartCollider.cs:324`](../../../../references/eft-decompiled/Assembly-CSharp/BodyPartCollider.cs#L324)) → `PlayerBridge.ApplyShot` ([`BodyPartCollider.cs:44`](../../../../references/eft-decompiled/Assembly-CSharp/BodyPartCollider.cs#L44)) → `Player.ApplyShot` ([`Player.cs:30404`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L30404)), que já processou armadura (`ProceedDamageThroughArmor`) antes de chamar `ApplyDamageInfo` (`Player.cs:30432`).
2. **[B]** O Prefix do Harmony em `DamageTriggerPatch` roda ANTES do corpo original: para `bodyPartType` Chest/Head, lê `__instance.ActiveHealthController.GetBodyPartHealth(bodyPartType).Current` (`Player.cs:25291`) e grava em `__state` — este é o HP real pré-hit (nada mutou ainda).
3. **[C]** O corpo original de `ApplyDamageInfo` (`Player.cs:30463`) roda: `DoWoundRelapse` (`:30475`, sem HP), depois `ActiveHealthController.ApplyDamage(bodyPartType, damageInfo.Damage, damageInfo)` (`:30480`) — **aqui o HP da parte é reduzido** — e o resto do método (side effects, agressor, etc.).
4. **[D]** O Postfix roda DEPOIS: relê `GetBodyPartHealth(bodyPartType).Current` (agora pós-hit), calcula `danoEfetivo = __state − postHp`, e — se `ConfigConsumerBlackout2` estiver ligado e o tipo de dano for válido — chama `TraumaBlackoutTrigger.Evaluate`, que aplica piso absoluto + percentual + gate de analgésico (`TraumaEngine.IsUnderPainkiller`, `TraumaEngine.cs:99`) + roll fixo (50%/25%/0%). **Nota (PA-01-02, review 1):** quando o escudo de dano do Prefix bloqueia o hit (`FaintedPlayerIds.Contains(id)` já true), o Postfix já retorna ANTES de chamar `Evaluate` — o mesmo guard (`BlackoutTimers.ContainsKey(id) || FaintedPlayerIds.Contains(id)`) reaparece no topo do bloco de desmaio do Postfix (preservado do arquivo atual). `Evaluate` nunca chega a ser invocado com HP pré/pós-hit idênticos nesse caminho — a segurança vem desse guard preexistente, não do tratamento `effectiveDamage <= 0f` dentro de `Evaluate` (que existe como rede secundária, caso esse guard mude no futuro).
5. **[E]** Se `Evaluate` retornar `true`, o restante do Postfix — **sem nenhuma mudança deste item** — grava `TraumaState.BlackoutTimers[id]`/`BlackoutStartTimes[id]`, zera stamina, força prone, e chama `FikaBridge.SyncFaintStatus(__instance, true)` (`HealthPatches.cs:90` no arquivo atual) para propagar aos peers Fika.

## 7. Riscos e dependências

- **Patches existentes no mesmo alvo (`Player.ApplyDamageInfo`) — mutação de HP antes da nossa captura (PA-02-01, review 2; valores/regra corrigidos no `/code-review` r1, CR-01-01):** a "decisão técnica central" (§1) depende de sermos o PRIMEIRO código a ler `GetBodyPartHealth` — nada pode ter mutado o HP da parte antes de `__state` ser capturado. Auditoria por decompile dos DLLs instalados (`ilspycmd`, não só a garantia genérica de isolamento do `__state` do Harmony, que responde a uma pergunta diferente): `BringBackConcussion.Patches.ConcussionPatch.PatchPrefix` é `void`, **sem** `[HarmonyPriority]` (prioridade padrão `Normal`=400); o corpo (~55 linhas) só chama `DoContusion`/`DoStun`/emissão de som — **nunca** toca `GetBodyPartHealth` ou qualquer setter de HP por parte, independente de ordem. `VisceralCombat` tem 2 patches nesse alvo, **ambos `[PatchPostfix]`, zero `[PatchPrefix]`** — Postfixes sempre rodam depois do corpo original (que já rodou depois de TODOS os Prefixes), não podem interferir na captura de `__state`. **Ordem entre Prefixes (CR-01-01):** `Priority.High` = **600** (não 200) e a regra real do HarmonyX (`PatchInfoSerialization.PriorityComparer`, decompilado de `0Harmony.dll` 2.7.0 — compile-time e runtime idênticos) é que o MAIOR valor numérico executa PRIMEIRO entre Prefixes do mesmo alvo (`Priority.First`=800 roda primeiro, `Priority.Last`=0 roda por último) — o OPOSTO da regra "menor=maior prioridade" citada num rascunho anterior deste parágrafo. Nosso Prefix (`Priority.High`=600) roda antes do de BBC (`Normal`=400 por default) por essa razão real. Conclusão: a premissa se mantém correta, e agora com a prova E a regra corretas. Comportamento do escudo de dano do nosso Prefix (retorna `false` durante desmaio ativo) **inalterado por este item**.
- **SAIN / ORBIT:** 0 hits em `ApplyDamageInfo`/`ApplyDamageEvent` (grep confirmado no repo e no DLL instalado) — sem risco de conflito.
- **AP-03 (auditoria de overrides, ver §9 check 3):** confirmado que o patch dispara no dono (humano local, bot no host/headless) e é estruturalmente inerte no espelho — sem guard adicional a escrever.
- **Ordem de inicialização:** nenhuma mudança — o Harmony patch continua registrado via `CreateClassProcessor` por classe no `Awake` (`TRLImmersiveCombatMedicinePlugin.cs:246-256`), sem novo ponto de patch.
- **Dependência:** `TraumaEngine.IsUnderPainkiller(Player)` (`TraumaEngine.cs:99`) — já existe e já está documentada no código como reservada para este item ("consumida pelo 007 — decisões 9/15"); assinatura não muda.
- **`TraumaConsumerRegistry` — decisão de NÃO registrar:** `TraumaConsumerId.Blackout2` já existe no enum (`TraumaEngineState.cs:61`) e o comentário do `Register` (`TraumaEngineState.cs:129-130`) permite `regions: null` para "consumidor sem região de estado". Mas `TraumaConsumerRegistry.AnyActiveFor(TraumaRegion region)` só gateia toasts/observabilidade **por `TraumaRegion`** (Legs/Arms/Stomach) — tórax/cabeça não são membros desse enum (por design: `TraumaRegion` cobre só os 3 estados contínuos do motor 002; desmaio é evento, não estado — `TraumaEngineState.cs:8`). Registrar aqui seria boilerplate inerte (nenhum código consome `AnyActiveFor` para uma região vazia). Mantido como **patch simples e independente**, só consumindo `IsUnderPainkiller` + os 2 novos toggles — decisão explícita, não uma omissão.
- **Risco residual herdado do P7 (`docs/trauma-primitives.md`):** dump incompleto (AP-09) cobre só as subclasses conhecidas de `Player` (auditadas nesta spec); dano de peer é quantizado (`PutPackedFloat` 0-1000), podendo mover hits limítrofes do piso em ±1 unidade — aceitável, sem mudança de comportamento em relação ao pipeline atual.
- **Corner "hit simultâneo tórax+cabeça no mesmo frame" (PA-01-03, review 1 — fecha o corner case da spec funcional):** cada região gera sua PRÓPRIA chamada de `ApplyDamageInfo` (uma por hit — §1, "sem agregação de pellets"), nunca uma avaliação combinada. Se ambas as chamadas (tórax e cabeça) tiverem `Evaluate` retornando `true` no mesmo frame, a PRIMEIRA a escrever `TraumaState.BlackoutTimers[id] = now + duration` "vence"; a guard já existente no topo do bloco de desmaio do Postfix (`if (TraumaState.BlackoutTimers.ContainsKey(id) || ...) return;`, inalterada por este item) faz a SEGUNDA chamada retornar antes mesmo de invocar `Evaluate` — sem sobrescrita de deadline, sem dois disparos de `FikaBridge.SyncFaintStatus`. Fecha o corner case da spec funcional (marcar `[x]` na `01-spec.md` referenciando este parágrafo).

## 8. Checklist de implementação

- [x] Criar `modded/Patches/Trauma/TraumaBlackoutTrigger.cs` com `Evaluate(Player, EBodyPart, float preHitHp)` (piso + percentual + gate de analgésico + roll + log `[Blackout2]`).
- [x] Adicionar `EBodyPart bodyPartType, out float __state` ao Prefix existente de `DamageTriggerPatch` (captura HP pré-hit só para Chest/Head).
- [x] Adicionar `float __state` ao Postfix existente; remover o bloco `isChestTrauma`/`isHeadTrauma` (limiar fixo); chamar `TraumaBlackoutTrigger.Evaluate` gated por `ConfigConsumerBlackout2`.
- [x] Adicionar as 4 `ConfigEntry<float>` novas (seção "11. Trauma 2.0 (Desmaio)") em `TRLImmersiveCombatMedicinePlugin.cs` com os nomes de campo exatos da tabela §3 (`ConfigBlackoutChestPercent`/`ConfigBlackoutHeadPercent`/`ConfigBlackoutChestAbsoluteFloor`/`ConfigBlackoutHeadAbsoluteFloor`).
- [x] Rename-at-delivery de `ConfigConsumerBlackout2`: `"Blackout 2.0 (item 007)"` → `"Blackout 2.0"`, default `true`; bloco novo em `MigrateOrphanedConfigKeys` deletando o órfão sem copiar valor — replicou `Plugin.cs:407-428` (bloco "Stomach Effects", padrão CR-03-01).
- [x] Atualizar `PROPRIEDADES.md`: seção 11 nova + linha na tabela "Renomeadas" + atualizar a linha JÁ EXISTENTE `Blackout 2.0 (item 007)` na seção 6 (nome/default/tooltip real).
- [x] Bump de versão do `BepInPlugin` (`/code-mod`, gate mecânico) — v1.8.0 em `[BepInPlugin]`, log do Awake e `<Version>` do csproj.
- [ ] Regenerar grafo do mod (`update-graphs.sh` ou `/update-mod-graph`) — fora do escopo deste `/code-mod` (passo separado, ver relatório).
- [ ] Compilar (`/compile-mod TRL-ImmersiveCombatMedicine`) — 0 erros — fora do escopo deste `/code-mod` (passo separado, ver relatório).

## 9. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid: start hook + stop hooks idempotentes (`GameWorld.OnDestroy` + `BaseLocalGame.Stop`) — AP-01 | N/A | `TraumaBlackoutTrigger` é `static class` **stateless** — nenhuma coleção/flag/campo estático novo a limpar. Reusa `TraumaState.BlackoutTimers`/`BlackoutStartTimes`/`FaintedPlayerIds`/`BotFaintCooldowns`, já resetados em `TraumaState.ResetAll()` (`TRLImmersiveCombatMedicinePlugin.cs:439`, chamado em `OnRaidStartCleanup`), **inalterado por este item**. |
| 2 | Filtro MainPlayer/Fika em todo patch que reage a ação de player — AP-02 | ✅ | Auditoria própria (não só citação do doc de pesquisa): `ObservedPlayer.ApplyDamageInfo` ([`ObservedPlayer.cs:570-577`](../../../../references/fika-plugin/Fika.Core/Main/Players/ObservedPlayer.cs#L570)) NÃO chama `base` (só seta `Last*`) — o patch nunca roda no espelho. `FikaPlayer.ApplyDamageInfo` ([`FikaPlayer.cs:673-687`](../../../../references/fika-plugin/Fika.Core/Main/Players/FikaPlayer.cs#L673)) e `FikaBot.ApplyDamageInfo` ([`FikaBot.cs:246-252`](../../../../references/fika-plugin/Fika.Core/Main/Players/FikaBot.cs#L246)) chamam `base` — patch dispara no dono (humano local e bot no host/headless). Dano de peer humano chega ao dono via `FikaPlayer.HandleDamagePacket` ([`FikaPlayer.cs:1652-1704`](../../../../references/fika-plugin/Fika.Core/Main/Players/FikaPlayer.cs#L1652)) → `base.ApplyDamageInfo`. |
| 3 | Alvos ofuscados/virtuais resolvidos por assinatura; TODOS os overrides auditados — AP-03 | ✅ | `Player.ApplyDamageInfo` é `virtual`. Auditoria direta (grep próprio, não citação): `NetworkPlayer.cs` (0 hits), `LocalPlayer.cs` (0 hits), `HideoutPlayer.cs` (0 hits) — nenhuma dessas subclasses vanilla sobrescreve. `FikaPlayer`/`FikaBot` sobrescrevem E chamam base (linhas acima). `ObservedPlayer` sobrescreve e NÃO chama base — inerte no espelho por construção (satisfaz D16/dono-only sem guard extra). Alvo não-ofuscado (`Player`, `ApplyDamageInfo` são nomes estáveis) — sem risco de renumeração. |
| 4 | Mudança de estado via API canônica do EFT; side-effects mapeados — AP-04 | ✅ | Este item só **lê** `GetBodyPartHealth` (getter público de `IHealthController`, sem side-effect) para decidir a condição de entrada. Nenhuma mutação de estado nova é introduzida: os efeitos que passam a rodar (`BlackoutTimers[id]=...`, `Physical.Stamina.Current=0f`, `MovementContext.IsInPronePose=true`, `FikaBridge.SyncFaintStatus`) já existem no pipeline atual, revisados em CR-04/CR-05 (memória do mod) — inalterados por este item. |
| 5 | Estado entre raids: raid1→exit→raid2 e alt-F4/morte/MIA cobertos | N/A | Nenhuma estrutura nova persiste entre raids (helper stateless). As estruturas reusadas (`BlackoutTimers` etc.) já são resetadas em `OnRaidStartCleanup` — comportamento herdado, não modificado. |
| 6 | Semântica/defaults/faixas de cada ConfigEntry sem ambiguidade (incl. estado neutro) — AP-05 | ✅ | Tabela §3 com faixa/default/tooltip pt-BR para as 4 novas + o rename. Estado neutro documentado explicitamente: `Blackout 2.0 = false` ⇒ zero desmaio por dano (sem fallback ao limiar fixo, que foi removido do arquivo — não é um caminho "desligado", é um caminho que não existe mais). As 3 probabilidades de roll são constantes fixas (não-configuráveis), documentado para não confundir com as 4 `ConfigEntry`. |
| 7 | Re-invocação de método patcheado tem reentry-guard/`ReversePatch` (sem recursão infinita) — AP-07 | N/A | O patch não re-invoca `ApplyDamageInfo` nem faz forwarding do método patcheado. `TraumaBlackoutTrigger.Evaluate` só lê HP e chama a API pública `FikaBridge.SyncFaintStatus` (já existente, não-recursiva) — sem `MethodInfo.Invoke` nem ressurreição de operação. |
| 8 | Flags/caches de intercept validados contra o contexto atual após troca (arma/operação/tela) — AP-08 | N/A | `__state` é uma variável local por-invocação do Harmony (não um campo estático nem cache persistente) — escopo de uma única chamada de `ApplyDamageInfo`; não pode ficar "stale" entre trocas de arma/operação/tela porque não sobrevive além da chamada que a criou. |

## Histórico

| Data | Evento |
|---|---|
| 2026-07-19 | Spec técnica criada via `/create-technical-spec`. Decisão central: Prefix+`__state` captura HP pré-hit (não aritmética reversa, distorcida em overkill) — evidência re-verificada em `Player.cs:25289/25291/30463/30475/30480`, `GameWorld.cs:1966`, `BodyPartCollider.cs:44/324`, e nas 3 subclasses Fika (`FikaPlayer.cs`, `FikaBot.cs`, `ObservedPlayer.cs`), cruzada com a pesquisa prévia já `ilspycmd`-verificada em `docs/trauma-primitives.md §P7`. |
| 2026-07-19 | Review técnica 01 aplicada: PA-01-01 (citação de `GetBodyPartHealth` corrigida — apontava por engano para `GClass921`/mirror stub em vez de `docs/trauma-primitives.md §P7`), PA-01-02 (nota sobre `Evaluate` nunca ser chamado quando o escudo de dano bloqueia o hit), PA-01-03 (corner "hit simultâneo tórax+cabeça" fechado no §7). 0 achados pendentes — pronta para rodada 2. |
| 2026-07-19 | Review técnica 02 (última rodada planejada) aplicada: PA-02-01 (prova real por decompile de BBC/VC substituindo a garantia genérica do `__state`), PA-02-02 (coluna "Campo C#" explícita na tabela §3), PA-02-03 (citação de linha do bloco-molde de migração), PA-02-04 (linha existente do `PROPRIEDADES.md` a atualizar), PA-02-05 (filtro explícito de `bodyPartType` antes de `Evaluate`). 0 achados pendentes — pronta para `/code-mod`. |
