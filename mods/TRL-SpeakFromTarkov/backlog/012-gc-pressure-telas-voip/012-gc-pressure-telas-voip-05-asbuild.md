# 012 — GC Pressure nas Telas de VOIP · As-Built

**Mod:** TRL-SpeakFromTarkov
**Spec funcional:** [012-gc-pressure-telas-voip-01-spec.md](012-gc-pressure-telas-voip-01-spec.md)
**Spec técnica:** [012-gc-pressure-telas-voip-02-spec-tech.md](012-gc-pressure-telas-voip-02-spec-tech.md)
**Última review técnica:** [012-gc-pressure-telas-voip-03-spec-tech-review-01.md](012-gc-pressure-telas-voip-03-spec-tech-review-01.md)
**Build inicial:** 2026-09-09

> Documentação pós-implementação. Reflete o estado real do código entregue pelo `/code-mod` em `modded-V4/`.

## Arquivos alterados (build inicial)

| Ação | Path | Resumo |
| --- | --- | --- |
| MODIFICADO | `mods/TRL-SpeakFromTarkov/modded-V4/UI/PlayerVolumeMixerHUD.cs` | 9 `GUIStyle` cacheados (3 com cor/background dinâmicos atualizados por escrita simples); debounce de `SaveConfig()` com flush garantido em `Close()`/`OnDestroy()`/`OnApplicationQuit()`. |
| MODIFICADO | `mods/TRL-SpeakFromTarkov/modded-V4/UI/VoiceCalibrationHUD.cs` | 8 `GUIStyle` cacheados (1 com cor dinâmica). |
| MODIFICADO | `mods/TRL-SpeakFromTarkov/modded-V4/UI/MenuVoipHUD.cs` | Removidos os 2 `.ToList()` — enumeração direta de `HashSet`/`ConcurrentDictionary.Values`. |

## PA-NN-MM resolvidos durante o build

> Ponto da última review técnica, já resolvido na spec técnica antes deste build.

| ID | Categoria · Impacto | Resumo da resolução |
| --- | --- | --- |
| PA-01-01 | A — Gap · 🟢 Menor | Stub de `Update()` (PlayerVolumeMixerHUD) mostrado completo com o bloco `if (IsOpen)` já existente — implementado exatamente assim no código real. |

## Detalhe além da spec técnica

Ao implementar, identifiquei que 4 dos `GUIStyle` cacheados (`_nameStyle`/`_volLabelStyle`/`_muteBtnStyle` em `PlayerVolumeMixerHUD.cs`, `_recStatusStyle` em `VoiceCalibrationHUD.cs`) têm cor ou background que variam por chamada (mute/volume/estado de gravação) — a spec técnica não detalhava esse ponto. Resolvido cacheando o objeto `GUIStyle` (evita a alocação) mas atualizando só o campo de cor/background dinâmico a cada chamada (escrita simples, não alocação) — mesmo espírito da correção, sem perder o comportamento visual dinâmico original.

## Mudanças posteriores

(vazio inicialmente — preenchido por `/apply-code-review`)

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-09 | Build concluído via `/code-mod` em `modded-V4/`. Testes em jogo e compilação (`/compile-mod`) ainda pendentes. |
