# 015 — Ambiente Acústico: Reverb e Curva de Distância · As-Built

**Mod:** TRL-SpeakFromTarkov
**Spec funcional:** [015-ambiente-acustico-reverb-distancia-01-spec.md](015-ambiente-acustico-reverb-distancia-01-spec.md)
**Spec técnica:** [015-ambiente-acustico-reverb-distancia-02-spec-tech.md](015-ambiente-acustico-reverb-distancia-02-spec-tech.md)
**Última review técnica:** [015-ambiente-acustico-reverb-distancia-03-spec-tech-review-01.md](015-ambiente-acustico-reverb-distancia-03-spec-tech-review-01.md)
**Build inicial:** 2026-09-09

> Documentação pós-implementação. Reflete o estado real do código entregue pelo `/code-mod` em `modded-V4/`.

## Arquivos alterados (build inicial)

| Ação | Path | Resumo |
| --- | --- | --- |
| MODIFICADO | `mods/TRL-SpeakFromTarkov/modded-V4/VOIPPlugin.cs` | Novo `ConfigEntry<bool> EnableNativeReverb` (seção "Network & 3D Audio", tooltip em inglês seguindo a convenção da seção). |
| MODIFICADO | `mods/TRL-SpeakFromTarkov/modded-V4/Audio/RemoteSpeaker.cs` | `Initialize()` roteia `audioSource.outputAudioMixerGroup` pro `BetterAudio.ObservedPlayerSpeechMixer` quando o toggle está ativo; instrumentação temporária de log em `OnAudioFilterRead` pra calibração da curva de distância. |
| MODIFICADO | `mods/TRL-SpeakFromTarkov/PROPRIEDADES.md` | Nova entrada `Enable Native Reverb` na seção "Network & 3D Audio" (nota: seção pré-existente não estava documentada por completo antes deste item — só a entrada nova foi adicionada). |

## PA-NN-MM resolvidos durante o build

> Pontos da última review técnica, já resolvidos na spec técnica antes deste build.

| ID | Categoria · Impacto | Resumo da resolução |
| --- | --- | --- |
| PA-01-01 | A — Gap · 🟡 Importante | Nota sobre `bypassReverbZones` (sistema diferente do roteamento por mixer) adicionada ao código como comentário, não só na spec. |
| PA-01-02 | A — Gap · 🟢 Menor | `BetterAudio.Instance`/`BetterAudio.Instantiated` usados diretamente (sem `Singleton<>` explícito) no código real. |

## Correção feita durante a implementação (além da spec técnica)

O stub da instrumentação temporária (seção 5 da spec técnica) usava `Time.unscaledTime` para o throttle do log — **corrigido para `Environment.TickCount`** durante a implementação: `OnAudioFilterRead` roda na audio thread do Unity (confirmado no item `011`), e `Time.*` não é seguro de ler fora da main thread — mesmo motivo pelo qual `SftNetwork.LogErrorThrottled` já usa `Environment.TickCount` em vez de `Time.time`. Usar `Time.unscaledTime` ali teria reintroduzido exatamente a classe de bug que o item `011` corrigiu.

## Mudanças posteriores

(vazio inicialmente — preenchido por `/apply-code-review`)

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-09 | Build concluído via `/code-mod` em `modded-V4/`. Testes em jogo (ambientes diferentes, toggle ligado/desligado, coleta de dados de calibração) e compilação (`/compile-mod`) ainda pendentes. |
