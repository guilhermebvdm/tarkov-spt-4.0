# 023 — Atraso na checagem de carregador (Magazine Check Delay) · Code Review 01

**Mod:** stancesAndCameraPositionSPT4.0.11
**Spec funcional:** [023-atraso-checagem-carregador-01-spec.md](023-atraso-checagem-carregador-01-spec.md)
**Spec técnica:** [023-atraso-checagem-carregador-02-spec-tech.md](023-atraso-checagem-carregador-02-spec-tech.md)
**Review técnica:** [023-atraso-checagem-carregador-03-spec-tech-review-01.md](023-atraso-checagem-carregador-03-spec-tech-review-01.md) — 0 bloqueadores
**Data:** 2026-09-22

> Pré-condição via fallback (b): não existe `05-asbuild.md` (item implementado fora do fluxo formal,
> antes desta auditoria). 3 dos 4 arquivos listados na §4 da spec técnica já existem modificados
> conforme esperado (`MagCheckDelayPatch.cs` criado, `Plugin.cs` e `RaidLifecyclePatches.cs`
> modificados) — 75% ≥ 50%, `/code-mod` considerado concluído. `PROPRIEDADES.md` é o único pendente
> (já rastreado como achado abaixo).

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 3

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| CR-01-01 | E — Legibilidade | 🟢 | Log de exceção usa `ex.Message` em vez de `{ex}` (perde stack trace) | ✅ Aplicado |
| CR-01-02 | C — Gap vs. spec | 🟡 | Tooltips das 2 `ConfigEntry` não mencionam que a checagem de câmara também é atrasada | ✅ Aplicado |
| CR-01-03 | D — Arquitetura | 🟡 | `PROPRIEDADES.md` não documenta as 2 props novas | ✅ Aplicado |

## Categorias

- **A — Crítico** · **B — Bug latente** · **C — Gap vs. spec** · **D — Arquitetura** · **E — Legibilidade/manutenção** · **F — Melhoria opcional**

## Impacto

- 🔴 Bloqueador · 🟠 Forte · 🟡 Médio · 🟢 Menor

---

## Pontos

### CR-01-01 · E — Legibilidade · 🟢 Menor · ✅ Aplicado em 2026-09-22

**Log de exceção usa `ex.Message` em vez de `{ex}` (perde stack trace)**

**Local:** [`mods/stancesAndCameraPositionSPT4.0.11/modded-testchannel/Patches/MagCheckDelayPatch.cs:88`](../../modded-testchannel/Patches/MagCheckDelayPatch.cs#L88) e [linha 129](../../modded-testchannel/Patches/MagCheckDelayPatch.cs#L129)

**Problema:**
```csharp
Plugin.Logger.LogError($"[MagCheckDelay] Erro no Prefix: {ex.Message}");
// ...
Plugin.Logger.LogError($"[MagCheckDelay] Erro na exibição atrasada: {ex.Message}");
```
Usa só `ex.Message` — descarta o stack trace. Todo o resto do mod (`CrouchRunEnableSprintPatch`,
`SprintStateEnableSprintSyncPatch`, `ProneRunSpeedDriverPatch`, etc.) loga `{ex}` (exceção completa,
com stack trace) nos mesmos catches defensivos.

**Por que importa:** se uma exceção real acontecer aqui (ex.: um `NullReferenceException` num caminho
não previsto), o log vai dizer só a mensagem genérica, sem dizer EM QUE LINHA — investigar vira muito
mais lento que nos outros patches deste mesmo mod, que já seguem o padrão certo.

**Sugestão:** trocar `{ex.Message}` por `{ex}` nas duas ocorrências, igualando ao padrão já usado no
resto do arquivo/mod.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Sugestão aplicada conforme proposto.
**Aplicação:** `MagCheckDelayPatch.cs:88,130` — `{ex.Message}` → `{ex}` nas duas ocorrências, com comentário `// ref: CR-01-01`.

### CR-01-02 · C — Gap vs. spec · 🟡 Médio · ✅ Aplicado em 2026-09-22

**Tooltips das 2 `ConfigEntry` não mencionam que a checagem de câmara também é atrasada**

**Local:** [`mods/stancesAndCameraPositionSPT4.0.11/modded-testchannel/Plugin.cs:357-373`](../../modded-testchannel/Plugin.cs#L357-L373)

**Problema:** a spec técnica (§3, já revisada na review-01/PA-01-02 correlata) define o texto-alvo dos
tooltips de `Enable Magazine Check Delay` e `Magazine Check Delay Seconds` mencionando explicitamente
que a checagem de câmara (item 019) também é afetada — confirmado tecnicamente que os dois convergem
em `EftBattleUIScreen.ShowAmmoDetails`. O código em produção ainda tem o texto original, que só fala em
"magazine"/"carregador".

**Por que importa:** o usuário reportou exatamente essa confusão como motivação pra esta auditoria
inteira ("o que mudou de 2.24 pra 2.25?") — sem o tooltip corrigido, qualquer jogador que configure o
delay achando que é só pro carregador vai se surpreender quando a câmara também atrasar, sem saber por
quê.

**Sugestão:** aplicar o texto-alvo já definido na spec técnica §3 nos dois `ConfigDescription` de
`Plugin.cs:357-373` (bind de `_EnableMagCheckDelay` e `_MagCheckDelaySeconds`).

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Sugestão aplicada conforme proposto.
**Aplicação:** `Plugin.cs:357-374` — tooltips de `_EnableMagCheckDelay`/`_MagCheckDelaySeconds` reescritos (EN+pt-BR) mencionando carregador E câmara, com comentário `// ref: CR-01-02`.

### CR-01-03 · D — Arquitetura · 🟡 Médio · ✅ Aplicado em 2026-09-22

**`PROPRIEDADES.md` não documenta as 2 props novas**

**Local:** [`mods/stancesAndCameraPositionSPT4.0.11/PROPRIEDADES.md`](../../PROPRIEDADES.md) (seção `Weapon Inspection`)

**Problema:** `repo-workflow-best-practices` §7 exige que toda `ConfigEntry` nova seja documentada em
`PROPRIEDADES.md` — convenção que `/code-mod` normalmente aplica automaticamente, mas que este item
pulou por ter sido implementado fora do fluxo formal. A seção `Weapon Inspection` ainda lista só a
prop do item 019 (`Show Chamber Ammo On Check`).

**Por que importa:** `PROPRIEDADES.md` é a fonte única de verdade sobre o F12 deste mod — sem essa
atualização, qualquer consulta futura (inclusive `/review-mod-properties` na preparação de publicação)
vai reportar contagem errada e faltar as 2 props que já existem em produção.

**Sugestão:** adicionar 2 linhas na tabela da seção `Weapon Inspection` de `PROPRIEDADES.md`, com os
mesmos textos/faixas de `Plugin.cs` (já com o tooltip corrigido do CR-01-02 refletido no resumo em
pt-BR).

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Sugestão aplicada conforme proposto — e ampliada: a seção `Weapon Inspection` não tinha NENHUMA tabela detalhada em `PROPRIEDADES.md` (gap pré-existente, não só das 2 props deste item), então foi criada do zero com as 3 props da seção (a existente do item 019 + as 2 novas do item 023).
**Aplicação:** `PROPRIEDADES.md` — nova subseção `### Weapon Inspection` (sob `## E — Mecânicas de arma`), com nota explícita sobre o atraso cobrir as duas checagens.

## Histórico

| Data | Evento |
|---|---|
| 2026-09-22 | Aplicação automática de 3 achados via `/apply-code-review` — IDs aplicados: CR-01-01, CR-01-02, CR-01-03; rejeitados: nenhum. |
