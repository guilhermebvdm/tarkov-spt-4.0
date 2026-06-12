# 001 — skin-tagilla-academia · As-Built

**Mod:** AutoGym
**Spec funcional:** [001-skin-tagilla-academia-01-spec.md](001-skin-tagilla-academia-01-spec.md)
**Spec técnica:** [001-skin-tagilla-academia-02-spec-tech.md](001-skin-tagilla-academia-02-spec-tech.md)
**Última review técnica:** [001-skin-tagilla-academia-03-spec-tech-review-01.md](001-skin-tagilla-academia-03-spec-tech-review-01.md)
**Build inicial:** 2026-06-10

> Documentação **pós-implementação**. Reflete o estado real do código entregue pelo `/code-mod` e atualizado por `/apply-code-review`. Quando o conteúdo aqui diverge da spec técnica, este documento ganha — a spec é planejamento, o asbuild é o que foi feito.

## Arquivos alterados (build inicial)

| Ação | Path | Resumo |
| --- | --- | --- |
| CRIADO | `mods/AutoGym/modded/WorkoutBodySkinSwap.cs` | Helper estático: resolve o body template via `CustomizationSolverClass`, carrega o bundle (Retain/LoadBundles), aplica `PlayerBody.SetSkin` na parte Body e restaura a skin do perfil (`BodyCustomization`) ao fim do treino, com token de geração contra races e saneamento de estado órfão. |
| MODIFICADO | `mods/AutoGym/modded/Plugin.cs` | 2 novas `ConfigEntry` (`Swap Workout Body Skin`, `Workout Body Skin Id`) na seção `Visuals`; Prefix de `PrepareWorkout` chama `WorkoutBodySkinSwap.Apply`; Finalizer de `StopWorkout` chama `WorkoutBodySkinSwap.Restore`. |
| MODIFICADO | `mods/AutoGym/PROPRIEDADES.md` | Documentadas as 2 novas propriedades F12 na seção `Visuals`. |
| MODIFICADO | `mods/AutoGym/modded/AutoGym.csproj` | Referências convertidas de `$(SptPath)` para `References/` locais (padrão do repo) + adicionada `Comfort.dll` (necessária para `Singleton<T>`). |

## PA-NN-MM resolvidos durante o build

> Pontos da última review técnica que foram **aplicados como parte da implementação** (não como /apply-code-review posterior).

| ID | Categoria · Impacto | Resumo da resolução |
| --- | --- | --- |
| PA-01-01 | B — Edge Case · 🟡 | `Apply` saneia estado órfão (corpo Unity-destruído sem `StopWorkout`) antes de qualquer early-return, liberando o handle de bundle retido. |
| PA-01-02 | B — Edge Case · 🟡 | Fechado junto com PA-01-01 (mesmo cenário-raiz). |
| PA-01-03 | C — Lógica · 🟢 | Risco aceito e documentado no §7 da spec técnica (`CustomizationClipping` acumula flags até o corpo ser recriado). |
| PA-01-04 | A — Gap · 🟢 | Implementado: `HasIntergratedArmor` recalculado no Apply e no Restore via `CustomizationSolverClass.HasIntegratedArmor`. |
| PA-01-05 | A — Gap · 🟢 | Caminho `bundle == null` consulta `GetSuite` e loga mensagem específica quando o usuário configurou um suite id em vez do body template id. |

## Mudanças posteriores

> Atualizado por `/apply-code-review` a cada rodada. Cada entrada lista os achados aplicados/rejeitados/pulados naquela rodada e os arquivos tocados.

### 2026-06-10 — Code review 01 (`/apply-code-review`)

- **Aplicados (4):** CR-01-01 (handle de bundle nunca vaza em falha de `SetSkin` — atribuição de estado só após sucesso + release no catch), CR-01-02 (corpo de `Apply` e `Release` do `finally` de `Restore` protegidos por try/catch — patch nunca quebra `PrepareWorkout`/`StopWorkout`), CR-01-03 (`MissingReferenceException` no restore vira `LogDebug` de teardown normal), CR-01-04 (id inválido no config loga mensagem específica em vez de stack trace).
- **Arquivos tocados:** `mods/AutoGym/modded/WorkoutBodySkinSwap.cs`.
- **Recompilado:** 0 warnings / 0 erros; reinstalado em `D:/SPT/BepInEx/plugins/AutoGym/`.

### 2026-06-11 — Code review 02 (`/apply-code-review`)

- **Aplicados (3):** CR-02-01 (`Restore()` sem parâmetro `owner` — premissa single-player documentada; call-site do Finalizer atualizado), CR-02-02 (`.Trim()` no id do config antes do ctor `MongoID`), CR-02-03 (`LogDebug` de sucesso no swap e no restore, para a validação in-game).
- **Arquivos tocados:** `mods/AutoGym/modded/WorkoutBodySkinSwap.cs`, `mods/AutoGym/modded/Plugin.cs`.
- **Recompilado:** 0 warnings / 0 erros; reinstalado em `D:/SPT/BepInEx/plugins/AutoGym/`.

## Histórico

| Data | Evento |
| --- | --- |
| 2026-06-10 | Build concluído via `/code-mod` |
| 2026-06-10 | `/compile-mod` OK (0 warnings, 0 erros); instalado em `D:/SPT/BepInEx/plugins/AutoGym/` |
| 2026-06-10 | Aplicação de 4 achados de code-review 01 via `/apply-code-review` — IDs: CR-01-01, CR-01-02, CR-01-03, CR-01-04; recompilado e reinstalado |
| 2026-06-11 | Aplicação de 3 achados de code-review 02 via `/apply-code-review` — IDs: CR-02-01, CR-02-02, CR-02-03; recompilado e reinstalado |
