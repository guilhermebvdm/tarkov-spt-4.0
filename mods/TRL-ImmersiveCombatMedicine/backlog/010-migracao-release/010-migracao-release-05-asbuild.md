# 010 — Migração de configs + release · As-Built

**Mod:** TRL-ImmersiveCombatMedicine
**Spec funcional:** [010-migracao-release-01-spec.md](010-migracao-release-01-spec.md)
**Spec técnica:** [010-migracao-release-02-spec-tech.md](010-migracao-release-02-spec-tech.md)
**Última review técnica:** [010-migracao-release-03-spec-tech-review-02.md](010-migracao-release-03-spec-tech-review-02.md)
**Build inicial:** 2026-07-25

> Documentação **pós-implementação**. Reflete o estado real do código entregue pelo `/code-mod` e atualizado por `/apply-code-review`. Quando o conteúdo aqui diverge da spec técnica, este documento ganha — a spec é planejamento, o asbuild é o que foi feito.

## Arquivos alterados (build inicial)

| Ação | Path | Resumo |
| --- | --- | --- |
| MODIFICADO | `mods/TRL-ImmersiveCombatMedicine/modded/TRLImmersiveCombatMedicinePlugin.cs` | Bloco A: removidos `ConfigLegsEnabled`/`ConfigArmsEnabled`/`ConfigStomachEnabled` (declaração + `Config.Bind`) + o bloco de migração one-time do mojibake "Sistema de Braços" em `MigrateOrphanedConfigKeys()` (escrevia em `ConfigArmsEnabled.Value`) + campos/heartbeat `[DEBUG-ICM]` (`_debugHost`/`_debugCtrl`/`_debugNextBeat`) + handler morto `OnHealCheckResponseHandler` (com subscribe/unsubscribe — `OnDestroy()` removido por inteiro, ficaria vazio). Bloco B: `Medic Interact Distance` default `5f`→`3.5f`, tooltip sem "para testes". Versão `1.9.1`→`1.10.0` (`BepInPlugin` + log do `Awake`). |
| MODIFICADO | `mods/TRL-ImmersiveCombatMedicine/modded/TRL-ImmersiveCombatMedicine.csproj` | Bump de versão `1.9.1`→`1.10.0` (`<Version>`). |
| MODIFICADO | `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/BandAidController.cs` | Bloco A: sondas `[DEBUG-ICM]` removidas (`OnEnable`/`OnDisable` inteiros, logs de `Awake`/`OnDestroy`, `_dbgUpdateAlive`/`_dbgInRaid` + blocos log-once do `Update()`, acumulador `attached` de `EnsureMedicInteractables`). Bloco C: ~13 notificações + `OnHealCheckResponseHandler` (branch de recusa) migradas para `MedicLocale`/`MedicDenyReasonId`. |
| MODIFICADO | `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/MedicActionsPatch.cs` | Bloco A: sonda `[DEBUG-ICM]` removida (campo `_dbgNextLog` + 2 logs do `Prefix`); lógica de interceptação idêntica. |
| MODIFICADO | `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/BandAidNetworkHandler.cs` | Bloco C: `using TRLImmersiveCombatMedicine;` adicionado; `OnHealCheckReceived` e `TryAnswerForLocalBot` passam a gerar `MedicDenyReasonId` (não mais string); log de diagnóstico em `OnHealCheckResponseReceived` atualizado (`packet.DenyReasonId`); 2 notificações fora do handshake (`TreatedByAlly`, `ShoulderTapReceived`) migradas para `MedicLocale`. |
| MODIFICADO | `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/BandAidHealCheckPacket.cs` | Bloco C: `BandAidHealCheckResponsePacket.DenyReason` (`string`) → `DenyReasonId` (`MedicDenyReasonId`, serializado como `byte`) — mudança de wire format. **Desvio da spec:** campo declarado `internal` (não `public` como no stub) — ver nota de desvio abaixo. |
| MODIFICADO | `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/TourniquetManager.cs` | Bloco C: `GetBodyPartName` virou wrapper de `MedicLocale.BodyPartLong`; 6 notificações migradas para `MedicLocale.Get`. |
| MODIFICADO | `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/MedicInteractable.cs` | Bloco C: os 2 rótulos do ActionPanel (`Name = "..."`) migrados para `MedicLocale.Get(MedicTextId.ActionExamine/ActionShoulderTap)`. |
| MODIFICADO | `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/BandAidUI.cs` | Bloco C: título + rótulos de membro passam a ser (re)aplicados em `ShowUI()` (não mais fixados em `BuildUI()`/`Awake`, achado de design AP-08 da spec — texto congelado no idioma do boot); `PartLabel` virou wrapper de `MedicLocale.BodyPartShort` (dict `PartLabelPt` removido); footer dinâmico migrado (incl. `PressModeVerb`); `"INDISPONÍVEL"` migrado; literais nunca-visíveis de `BuildUI()` (título/footer) trocados de PT para EN por clareza (inconsequentes — sobrescritos antes do canvas aparecer). |
| MODIFICADO | `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Trauma/HealthPatches.cs` | Bloco A.6 (cosmético): comentário em `:139` que citava `ConfigArmsEnabled` (campo removido) atualizado para referenciar a key legada por nome, não o campo C#. |
| CRIADO | `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/MedicLocale.cs` | Classe nova de i18n: `enum MedicDenyReasonId`, `enum MedicTextId` (27 chaves), tabelas `EnTexts`/`PtTexts`, resolvers de rótulo de membro (`BodyPartShort`/`BodyPartLong`, 2 granularidades preservadas), `PressModeVerb`, `Get(...)` e `GetDenyReasonText(...)`. Reusa `TraumaLocale.IsGamePortuguese()` sem duplicar a leitura de idioma. |
| CRIADO | `mods/TRL-ImmersiveCombatMedicine/scripts/package-release.sh` | Bloco D: script bash de empacotamento client-only — builda via `compile-mod.sh`, stage do bundle (`BepInEx/plugins/<mod>/{dll,pdb}`), zip via `Compress-Archive` (PowerShell). Testado uma vez (ver "Notas de execução"); nenhum artefato `dist/` permanece no repo (`.gitignore` já cobre `/dist/`). |
| MODIFICADO | `mods/TRL-ImmersiveCombatMedicine/PROPRIEDADES.md` | Bloco B: 3 linhas removidas da Seção 2 (`Sistema de Pernas/Braços/Estomago`); 3 linhas adicionadas em "Removidas"; `Medic Interact Distance` (Seção 4) atualizado (`3.5`, tooltip sem "para testes"); frase da Seção 5 atualizada; linha nova no Histórico de Alterações. |
| MODIFICADO | `mods/TRL-ImmersiveCombatMedicine/backlog/mod-backlog.md` | Status do item 010: 🟡 → 🟢. |
| MODIFICADO | `mods/TRL-ImmersiveCombatMedicine/backlog/010-migracao-release/010-migracao-release-02-spec-tech.md` | Checklist §8 (21 itens) marcado `[x]`; linha nova no Histórico documentando o desvio CS0052 (ver abaixo). |

## PA-NN-MM resolvidos durante o build

> Pontos da última review técnica que foram **aplicados como parte da implementação** (não como /apply-code-review posterior — a spec já chegou com 0 bloqueadores pendentes; os itens abaixo são os achados 🔴/🟡/🟢 das 2 rodadas anteriores cuja resolução SE MATERIALIZA no código deste build).

| ID | Categoria · Impacto | Resumo da resolução |
| --- | --- | --- |
| PA-01-01 | C — Erro de Lógica · 🔴 | `using Band_Aid;` presente no topo de `MedicLocale.cs` — `GetDenyReasonText` compila (chama `ItemDatabase.GetStats`). |
| PA-01-02 | C — Erro de Lógica · 🔴 | `using TRLImmersiveCombatMedicine;` adicionado ao topo de `BandAidNetworkHandler.cs` — `MedicDenyReasonId` resolvido nos 2 pontos de escrita sem qualificação total. |
| PA-01-03 | C — Erro de Lógica · 🔴 | Terceiro ponto de leitura do campo (`OnHealCheckResponseReceived:930`, log de diagnóstico) migrado para `packet.DenyReasonId` — não ficou órfão do rename. |
| PA-01-04 | — · 🟡 | Handler morto `OnHealCheckResponseHandler` (corpo sempre vazio) removido de `TRLImmersiveCombatMedicinePlugin.cs`, junto do subscribe/unsubscribe — `OnDestroy()` do plugin removido por inteiro (ficaria vazio). |
| PA-01-05 / PA-02-02 | — · 🟡/🟢 | Citações de linha de `OnHealCheckReceived`/`TryAnswerForLocalBot` confirmadas contra o código real antes de editar (ambos os pontos de escrita do `DenyReasonId` localizados e migrados). |
| PA-01-06 / PA-02-01 | — · 🟢 | Ícones ⚠/☠/✈ preservados literalmente nos templates EN/PT de `MedicLocale.cs` (necrose + shoulder tap). |
| PA-02-03 | — · 🟢 | `|| true` mantido na linha `VER=...` do `package-release.sh` (evita abort silencioso de `set -e`/`pipefail`). |
| PA-02-04 | — · 🟢 | Ordem de implementação respeitada: Bloco A.1/A.2 juntos; C.1 (`MedicLocale.cs`) antes de C.2/C.2b/C.3; `package-release.sh` executado (não só criado) só depois de A-C completos. |
| PA-02-05 | — · 🟢 | Comentário sobre `EBodyPart.Common` em `MedicLocale.cs` reproduzido fielmente (fallback `"..."` documentado, sem alegar erro factual). |
| PA-02-06 | — · 🟢 | `package-release.sh` invocado sempre via `bash script.sh` — sem `chmod +x`. |

## Mudanças posteriores

> Atualizado por `/apply-code-review` a cada rodada. Cada entrada lista os achados aplicados/rejeitados/pulados naquela rodada e os arquivos tocados.

| Data | Origem | Resumo |
| --- | --- | --- |
| 2026-07-25 | CR-01-01 (code-review 01, aceito) | `MedicLocale.cs`: 2 chaves novas (`TreatingLabel`/`TreatingLabelWithItem`, índices 27/28, EN+PT). `BandAidUI.ShowTreatment` migrado de PT hardcoded para `MedicLocale.Get(...)` — fecha o único resíduo PT-only do fluxo de cura. Recompilado — 0 erros, mesmos 10 warnings pré-existentes. |

## Desvio da spec técnica (achado durante a implementação)

**CS0052 — "Inconsistent accessibility" em `BandAidHealCheckResponsePacket.DenyReasonId`.** O stub 5 da spec técnica declara:

```csharp
public struct BandAidHealCheckResponsePacket : INetSerializable
{
    ...
    public TRLImmersiveCombatMedicine.MedicDenyReasonId DenyReasonId;
    ...
}
```

Como `MedicDenyReasonId` é `internal` (stub 4, decisão deliberada de encapsulamento — mesmo padrão de `MedicLocale`/`MedicTextId`), um campo **público** de um struct **público** com um tipo **interno** é um erro de compilação em C# (CS0052 — "o tipo de campo é menos acessível que o campo"). Confirmado por build isolado (`dotnet build` contra um repro mínimo de 2 linhas, fora do repo, antes de tocar no código real) — não é uma suposição.

**Fix aplicado:** o campo virou `internal` (mantendo `MedicDenyReasonId` como `internal`, conforme a intenção original da spec):

```csharp
internal TRLImmersiveCombatMedicine.MedicDenyReasonId DenyReasonId;
```

Isso preserva o mesmo acesso de fato — `Band_Aid` (onde o struct e `BandAidNetworkHandler` vivem) e `TRLImmersiveCombatMedicine` (onde `BandAidController`/`MedicLocale` vivem) são a MESMA assembly, então `internal` não bloqueia nenhum dos 3 pontos que leem/escrevem o campo. A alternativa (tornar `MedicDenyReasonId` `public`) foi descartada por expandir a superfície pública do enum sem necessidade — nenhum consumidor está fora da assembly. `Serialize`/`Deserialize` (implementação de `INetSerializable`) não são afetados: acessam o campo de dentro do próprio struct, onde a acessibilidade não importa.

Nenhuma outra parte do stub 5/6 precisou de ajuste — o restante do handshake (2 pontos de escrita, 1 de leitura de diagnóstico, 1 de exibição) compilou exatamente como especificado.

## Notas de execução

- **Compilação:** `bash .agents/scripts/compile-mod.sh TRL-ImmersiveCombatMedicine` — 0 erros, 10 warnings `Harmony003` (pré-existentes em `Patches/Trauma/HealthPatches.cs`, mesmo baseline de antes deste item — nenhum warning novo). Versão `1.9.1` → `1.10.0` confirmada pelo gate do próprio script.
- **Sondas `[DEBUG-ICM]`:** `grep -rc "\[DEBUG-ICM\]" modded --include="*.cs"` retorna vazio (zero ocorrências) em todo `modded/`.
- **Script de release:** `bash mods/TRL-ImmersiveCombatMedicine/scripts/package-release.sh` testado uma vez — gerou `dist/trl-icm-release-v1.10.0.zip` (116K, `BepInEx/plugins/TRL-ImmersiveCombatMedicine/{TRLImmersiveCombatMedicine.dll,TRLImmersiveCombatMedicine.pdb}`). O teste exigiu um `--allow-same-version` **temporário** na chamada interna ao `compile-mod.sh` (removido logo em seguida) porque o compile de verificação (item acima) já havia acabado de gravar a mesma versão `1.10.0` segundos antes — gate de versão do `compile-mod.sh` funcionando como esperado, não é um bug do script. O script entregue no repo NÃO tem a flag (idêntico ao stub 7). `dist/` removido ao final (já coberto por `/dist/` no `.gitignore` — não havia entrada a adicionar).
- **`ShowTreatment` (`BandAidUI.cs`):** o texto `"► TRATANDO: {label}"` / `"► {item} → {label}"` não estava na tabela de rastreio §5 da spec nem no inventário de ~25 pontos originais — deixado de fora deliberadamente na implementação inicial. Migrado depois via CR-01-01 (code-review 01) para fechar o único resíduo PT-only do fluxo de cura (ver "Mudanças posteriores").
- **Verificação de regressão:** cada arquivo tocado no Bloco A foi conferido linha a linha contra o `ANTES`/`DEPOIS` dos stubs — nenhuma lógica funcional mudou, só remoção de sondas/campos mortos. Bloco C conferido contra a tabela de rastreio §5 (todos os `arquivo:linha` migrados).

## Histórico

| Data | Evento |
| --- | --- |
| 2026-07-25 | Build concluído via `/code-mod` — Blocos A (config cleanup + remoção de `[DEBUG-ICM]` + handler morto), B (`Medic Interact Distance` 3.5 + `PROPRIEDADES.md`), C (`MedicLocale` + migração i18n EN/PT + wire format `DenyReasonId`) e D (`package-release.sh`, testado) entregues. Versão `1.9.1` → `1.10.0`. 1 desvio da spec (CS0052 em `DenyReasonId`, ver seção acima) corrigido e documentado. |
