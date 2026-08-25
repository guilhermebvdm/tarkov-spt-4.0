---
title: "Handoff — Rodada de otimização de performance do TRL-StancesAndMobility"
date: 2026-08-22
status: 🟢 Vivo
authors: Claude (investigação de performance 2026-08-22)
---

# Handoff — Rodada de otimização de performance do TRL-StancesAndMobility

**Comando de entrada da sessão:** `/optimize-mod-performance stancesAndCameraPositionSPT4.0.11`
(pasta com nome antigo; assembly `TRL-StancesAndMobility` v2.17.0; canônico = `modded/`, escopo default correto; deploy manual em `D:\SPT\BepInEx\plugins\TRL-StancesAndMobility\` — a memória antiga citando `RealisticMobility/` está desatualizada)

## Contexto (por que esta rodada existe)

Investigação de performance do Red Line (2026-08-22, baseline CapFrameX + logs — jogo 100% CPU/main-thread-bound). Diferente do ICM, aqui **não há drift de config**: os 5 toggles de debug da config viva já estão `false`. As ~121 linhas/raid vêm de `LogInfo`/`LogWarning` **hardcoded sem gate** — é problema de código.

## Achados para a auditoria (F1) aprofundar

1. **5 linhas de log por troca de stance:** `modded/StanceManager.cs:52` loga a troca e `:53` seta `ApplyComplexRotationPatch.LogNextFrame = true`, que despeja 4 linhas `[Spy-Complex]` no frame seguinte (`modded/Patches/ApplyComplexRotationPatch.cs:390-393`, reset em `:394`). Spy debug sem nenhum gate de config.
2. **Risco de metralhadora de Warning por frame:** `[STANCE-CLAMP] spring overshoot` em `LogWarning` dentro de postfix por-frame de `ProceduralWeaponAnimation` (`modded/Patches/ApplySimpleRotationPatch.cs:54`, `ApplyComplexRotationPatch.cs:119`) — se uma mola divergir, loga todo frame. Usar o helper **`ThrottledLog.cs` que já existe no mod** (hoje só usado para erros em `:45/:59`).
3. **Código morto perigoso:** `ApplySimpleRotationPatch.LogNextFrame` (`:21`) nunca é setado **nem resetado** (`:129-132` não zera, ao contrário do gêmeo Complex `:394`) — se algo um dia setar, loga por frame para sempre. Remover o caminho ou consertar o reset.
4. **Logs por evento de gameplay sem gate:** hold-breath 2 linhas por toggle (`modded/Patches/HoldBreathPatch.cs:86/100`), chambering (`modded/Patches/ManualChamberingPatches.cs:61/352/406`), ~37 linhas `[enable] OK` por boot (`modded/Plugin.cs:1411`, cosmético). Gatear por flag de debug existente na seção `[Debug (Advanced)]` da config ou rebaixar a `LogDebug`.
5. Além do logging, a F1 (`--perf`) deve varrer normalmente as superfícies por-frame do mod (patches de `ProceduralWeaponAnimation` rodam por frame — custo real além do log).

## Fricções conhecidas

- **Backlog:** item **020 "Faxina pré-publicação"** (🟡 em andamento) já é higiene de não-regressão ("o jogo se comporta exatamente como antes") e cobre remover código inerte — **reconciliar esta rodada com o 020** (referenciar, não duplicar). Próximo número livre: **021**.
- Convenções: SemVer bump a cada `/compile-mod`; build fica na pasta do mod; deploy em D:\SPT é manual (fazer backup `TRL-StancesAndMobility.dll` → `.bak-<versão>` antes de sobrescrever).

## Referências

- Relatório-irmão com o baseline completo: [mods/TRL-DynamicSpawn/docs/relatorio-auditoria-codigo-01.md](../../TRL-DynamicSpawn/docs/relatorio-auditoria-codigo-01.md) (§1)
- Plano aprovado: `C:\Users\guime\.claude\plans\vamos-la-precisamos-criar-hidden-lampson.md`
- Validação integrada V2 roda depois que as 3 frentes mergearem — critérios no plano.
