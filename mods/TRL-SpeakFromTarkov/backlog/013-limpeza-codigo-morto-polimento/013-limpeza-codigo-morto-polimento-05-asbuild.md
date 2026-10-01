# 013 — Limpeza de Código Morto e Polimento · As-Built

**Mod:** TRL-SpeakFromTarkov
**Spec funcional:** [013-limpeza-codigo-morto-polimento-01-spec.md](013-limpeza-codigo-morto-polimento-01-spec.md)
**Spec técnica:** [013-limpeza-codigo-morto-polimento-02-spec-tech.md](013-limpeza-codigo-morto-polimento-02-spec-tech.md)
**Última review técnica:** [013-limpeza-codigo-morto-polimento-03-spec-tech-review-01.md](013-limpeza-codigo-morto-polimento-03-spec-tech-review-01.md)
**Build inicial:** 2026-09-09

> Documentação pós-implementação. Reflete o estado real do código entregue pelo `/code-mod` em `modded-V4/`.

## Arquivos alterados (build inicial)

| Ação | Path | Resumo |
| --- | --- | --- |
| MODIFICADO | `mods/TRL-SpeakFromTarkov/modded-V4/Audio/RemoteSpeaker.cs` | `AudioClip` destruído em `OnDestroy` (AUD-03-09); `packetQueue` morto removido + comentário corrigido (AUD-03-12). |
| MODIFICADO | `mods/TRL-SpeakFromTarkov/modded-V4/UI/InRaidVoipHUD.cs` | `_originalPosCaptured` resetado ao re-buscar o painel (AUD-03-13); `Vector3[4]` movido pra campo de instância `_panelCorners` (AUD-03-16). |
| MODIFICADO | `mods/TRL-SpeakFromTarkov/modded-V4/Network/SftNetwork.cs` | `catch { }` de `BroadcastChannelAnnouncement` trocado por `LogErrorThrottled` (AUD-03-14). |
| MODIFICADO | `mods/TRL-SpeakFromTarkov/modded-V4/UI/MenuVoipHUD.cs` | `catch { }` de `FetchServerChannels` trocado por `LogErrorThrottled` (AUD-03-14). |
| MODIFICADO | `mods/TRL-SpeakFromTarkov/modded-V4/GameSessionPatcher.cs` | `PlayerInitPatch` removido (AUD-03-11, Opção B); `FikaVoipSendPatch`/`FikaVoipReceivePatch` removidos (AUD-03-15); `Finalizer` de `BoundSlotViewRefreshSelectViewPatch` tipado só para `NullReferenceException` (AUD-03-18); `GameWorldDisposePatch` chama `ResetVoipCaptureStartedFlag()`. |
| MODIFICADO | `mods/TRL-SpeakFromTarkov/modded-V4/VOIPPlugin.cs` | Removidas as 2 chamadas `.Enable()` órfãs (`FikaVoipSendPatch`/`FikaVoipReceivePatch`, linhas 328-329). |
| MODIFICADO | `mods/TRL-SpeakFromTarkov/modded-V4/Core/VoipController.cs` | Novo campo `_voipCaptureStarted` + poll one-shot em `Update()` (substitui o `PlayerInitPatch` removido) + método `ResetVoipCaptureStartedFlag()` (AUD-03-11, Opção B). |
| MODIFICADO | `mods/TRL-SpeakFromTarkov/modded-V4/Properties/AssemblyInfo.cs` | `AssemblyTitle`/`AssemblyProduct` → `TRL-SpeakFromTarkov`; `AssemblyVersion`/`AssemblyFileVersion` → `1.5.4.0` (sincronizado com `VOIPPlugin.cs`) (AUD-03-17). |

## PA-NN-MM resolvidos durante o build

> Pontos da última review técnica, já resolvidos na spec técnica antes deste build.

| ID | Categoria · Impacto | Resumo da resolução |
| --- | --- | --- |
| PA-01-01 | A — Gap · 🟡 Importante | Local real do `.Enable()` dos patches mortos corrigido para `VOIPPlugin.cs:328-329` — confirmado e removido corretamente durante o build. |
| PA-01-02 | A — Gap · 🟡 Importante | `_voipCaptureStarted` resetado em `GameWorldDisposePatch` — implementado exatamente conforme a correção da review. |

## Decisão de implementação: Opção A vs. Opção B (AUD-03-11)

A spec técnica documentava duas opções pra corrigir o timing do Postfix em `EFT.Player.Init` (método `async`). Na implementação, a **Opção A** (`MethodType.Async` do HarmonyX) foi descartada sem tentativa — sua sintaxe exata é documentação de biblioteca de terceiros não verificável nas fontes deste repo, e arriscar sintaxe não confirmada não trazia benefício sobre a Opção B, que já tinha evidência de Assembly completa. Implementada a **Opção B** integralmente: poll one-shot em `VoipController.Update()` usando `GameWorld.MainPlayer`, com reset em `GameWorldDisposePatch`.

## Mudanças posteriores

(vazio inicialmente — preenchido por `/apply-code-review`)

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-09 | Build concluído via `/code-mod` em `modded-V4/`. Testes em jogo e compilação (`/compile-mod`) ainda pendentes. |
