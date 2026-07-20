# 008 — Desmaio: duração aleatória min–max · As-Built

**Mod:** TRL-ImmersiveCombatMedicine
**Spec funcional:** [008-desmaio-duracao-aleatoria-01-spec.md](008-desmaio-duracao-aleatoria-01-spec.md)
**Spec técnica:** [008-desmaio-duracao-aleatoria-02-spec-tech.md](008-desmaio-duracao-aleatoria-02-spec-tech.md)
**Última review técnica:** [008-desmaio-duracao-aleatoria-03-spec-tech-review-01.md](008-desmaio-duracao-aleatoria-03-spec-tech-review-01.md)
**Build inicial:** 2026-07-19

> Documentação **pós-implementação**. Reflete o estado real do código entregue pelo `/code-mod` e atualizado por `/apply-code-review`. Quando o conteúdo aqui diverge da spec técnica, este documento ganha — a spec é planejamento, o asbuild é o que foi feito.

## Arquivos alterados (build inicial)

| Ação | Path | Resumo |
| --- | --- | --- |
| MODIFICADO | `mods/TRL-ImmersiveCombatMedicine/modded/TRLImmersiveCombatMedicinePlugin.cs` | Remove `ConfigBlackoutDuration`; adiciona `ConfigBlackoutDurationMin`/`Max` (seção 3); migração por CÓPIA em `MigrateOrphanedConfigKeys()`; fallback do `Update()` trocado para `ConfigBlackoutDurationMin.Value`; bump v1.9.0 (`[BepInPlugin]` + log do `Awake`). |
| MODIFICADO | `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Trauma/HealthPatches.cs` | Ponto `RANGE-READY`: `Mathf.Min`/`Mathf.Max` normaliza min>max + `UnityEngine.Random.Range(rollMin, rollMax)` sorteia a duração + log `[Blackout] ... duração sorteada` (LogInfo, não gateado). |
| MODIFICADO | `mods/TRL-ImmersiveCombatMedicine/modded/Fika/FikaBridge.cs` | Fallback defensivo de `SyncFaintStatus` trocado de `ConfigBlackoutDuration.Value` para `ConfigBlackoutDurationMin.Value`. |
| MODIFICADO | `mods/TRL-ImmersiveCombatMedicine/modded/TRL-ImmersiveCombatMedicine.csproj` | `<Version>1.8.0</Version>` → `1.9.0`. |
| MODIFICADO | `mods/TRL-ImmersiveCombatMedicine/PROPRIEDADES.md` | Seção 3: 2 linhas novas substituindo `Duracao do Desmaio`; tabela Renomeadas ganha a entrada da migração por cópia; Histórico de Alterações ganha linha do item 008. |
| MODIFICADO | `mods/TRL-ImmersiveCombatMedicine/backlog/mod-backlog.md` | Status do item 008: ⚪ → 🟢. |

**Intocado (confirmado):** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/BandAidNetworkHandler.cs:132` — literal `20f` no fallback do peer receptor permanece como estava (§7 da spec técnica: duração vem do pacote, não da config local do processo receptor).

## PA-NN-MM resolvidos durante o build

> Ambos os pontos da review técnica 01 já haviam sido resolvidos NA PRÓPRIA spec técnica (stubs §5.3/§5.4 corrigidos) antes deste build — o `/code-mod` implementou os stubs já corrigidos, sem decisão nova durante a implementação.

| ID | Categoria · Impacto | Resumo da resolução |
| --- | --- | --- |
| PA-01-01 | C — Erro de Lógica · 🟡 Importante | Migração usa `float.TryParse(entry.Value as string, NumberStyles.Float, CultureInfo.InvariantCulture, out legacyDurationValue)` — evita corrupção do valor legado em processos com cultura pt-BR/de-DE. Implementado literalmente conforme o stub §5.3 corrigido. |
| PA-01-02 | A — Gap de citação · 🟢 Menor | Citação de `UnityEngine.Random.Range(float minInclusive, float maxInclusive)` (inclusivo nos dois extremos) preservada como comentário inline no ponto `RANGE-READY` de `HealthPatches.cs`. |

## Mudanças posteriores

(vazio inicialmente — preenchido por `/apply-code-review`)

## Histórico

| Data | Evento |
| --- | --- |
| 2026-07-19 | Build concluído via `/code-mod` |
