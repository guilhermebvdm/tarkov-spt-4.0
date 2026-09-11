# 021 — Toggle para tombamento de mira lateral · As-Built

**Mod:** stancesAndCameraPositionSPT4.0.11
**Spec funcional:** [021-toggle-tombamento-mira-lateral-01-spec.md](021-toggle-tombamento-mira-lateral-01-spec.md)
**Spec técnica:** [021-toggle-tombamento-mira-lateral-02-spec-tech.md](021-toggle-tombamento-mira-lateral-02-spec-tech.md)
**Última review técnica:** [021-toggle-tombamento-mira-lateral-03-spec-tech-review-01.md](021-toggle-tombamento-mira-lateral-03-spec-tech-review-01.md)
**Build inicial:** 2026-09-08

> Documentação **pós-implementação**. Reflete o estado real do código entregue pelo `/code-mod` e atualizado por `/apply-code-review`. Quando o conteúdo aqui diverge da spec técnica, este documento ganha — a spec é planejamento, o asbuild é o que foi feito.

## ⚠️ Correção de pasta (2026-09-09)

O build inicial (2026-09-08) foi implementado em `modded/` (2.17.0), mas o fork realmente ativo para este mod é `modded-testchannel/` (já em 2.19.16 → 2.20.0 após esta correção — tem features que `modded/` não tem, ex. `HandsStateGuard`). As mudanças em `modded/` foram **revertidas** (`git restore`) e reaplicadas em `modded-testchannel/`. A tabela abaixo já reflete o path correto.

## Arquivos alterados (build inicial, path corrigido)

| Ação | Path | Resumo |
| --- | --- | --- |
| MODIFICADO | `mods/stancesAndCameraPositionSPT4.0.11/modded-testchannel/Plugin.cs` | Novo `ConfigEntry<bool> _FlattenCantedSightTilt` (default `false`) + `Config.Bind` em "Stance Transition & Kick", `Order = 94`. |
| MODIFICADO | `mods/stancesAndCameraPositionSPT4.0.11/modded-testchannel/Patches/ApplyComplexRotationPatch.cs` | `_scopeRotationField` repontado de `"_targetScopeRotation"` para `"_scopeRotation"` (valor já suavizado pelo nativo); `SetPositionAndRotation` final agora compõe `weapRotation * effectiveScopeRotation * CurrentRotation` (identity quando o toggle está ativado). |
| MODIFICADO | `mods/stancesAndCameraPositionSPT4.0.11/modded-testchannel/Patches/ApplySimpleRotationPatch.cs` | Mesma mudança do ApplyComplexRotationPatch. |
| MODIFICADO | `mods/stancesAndCameraPositionSPT4.0.11/PROPRIEDADES.md` | Nova linha na tabela "Stance Transition & Kick" para `Straighten Weapon On Canted Sights`. |
| MODIFICADO | `mods/stancesAndCameraPositionSPT4.0.11/backlog/mod-backlog.md` | Status do item 021: ⚪ → 🟢. |

## PA-NN-MM resolvidos durante o build

> Pontos da última review técnica que foram **aplicados como parte da implementação** (não como /apply-code-review posterior).

| ID | Categoria · Impacto | Resumo da resolução |
| --- | --- | --- |
| PA-01-01 | A — Gap · 🟡 | Resolvido na própria spec técnica (§1, plano B documentado) antes do build — nenhuma mudança de código adicional necessária; o plano B (inverter ordem `CurrentRotation * scopeRotation`) fica registrado para uso condicional na validação in-game. |
| PA-01-02 | A — Gap · 🟢 | Resolvido na spec técnica (§7) com a citação de `method_11()`/`method_24()` — nenhuma mudança de código, é evidência de que o comportamento já é coberto pelo nativo. |
| PA-01-03 | A — Gap · 🟢 | Resolvido na spec técnica (§7) com a nota sobre ausência de guard de raid nos dois Postfix — nenhuma mudança de código. |

## Mudanças posteriores

> Atualizado por `/apply-code-review` a cada rodada. Cada entrada lista os achados aplicados/rejeitados/pulados naquela rodada e os arquivos tocados.

(vazio inicialmente — preenchido por `/apply-code-review`)

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-08 | Build concluído via `/code-mod`. Pendente: compilar (`/compile-mod`) e validar in-game (checklist §8 da spec técnica, itens 7 e 8) antes de considerar o item validado ponta a ponta. |
| 2026-09-08 | Versão bumpada `2.17.0 → 2.18.0` (minor — feature nova visível no F12) em `Plugin.cs` e `CameraRotationMod.csproj`. Compilado via `dotnet build` direto (a pedido do usuário, **sem** o passo de instalação automática do `/compile-mod`) — `.dll` gerado em `mods/stancesAndCameraPositionSPT4.0.11/builds/TRL-StancesAndMobility.dll`, 0 erros/0 warnings. **Nada foi copiado para a pasta do jogo** — instalação manual fica a cargo do usuário. Pendente: validação in-game (checklist §8, item 8). |
| 2026-09-09 | **Correção de pasta.** O usuário apontou que o fork correto é `modded-testchannel/`, não `modded/` — `modded` está travado em 2.17.0 (base incorreta usada no build inicial), enquanto `modded-testchannel` já estava em 2.19.16 com features que `modded` não tem (`HandsStateGuard`, detecção de escada). Revertidas as mudanças em `modded/` (`git restore`) e reaplicadas em `modded-testchannel/` (os dois patches de rotação são idênticos entre os forks, exceto por logging de debug que não afeta os pontos editados). Compilado via `dotnet build` contra `modded-testchannel/CameraRotationMod.csproj` — 0 erros/0 warnings — DLL em `mods/stancesAndCameraPositionSPT4.0.11/builds/TRL-StancesAndMobility.dll` (compilado a partir do `.csproj` de `modded-testchannel/`, saída no `builds/` padrão — gitignored). Versão do testchannel `2.19.16 → 2.20.0` (bump conjunto com o item 018, corrigido na mesma sessão). |
