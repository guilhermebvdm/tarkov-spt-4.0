# 051 — Levers da zona stances · Review Técnica 01

**Spec revisada:** [051-stances-zone-levers-02-spec-tech.md](051-stances-zone-levers-02-spec-tech.md)
**Data:** 2026-07-04 · Review por agente adversarial de contexto limpo; decisões autônomas (/g-autodev) aplicadas no code-mod.

## Resumo

> 🔴 0 · 🟡 3 · 🟢 4 · **Veredito: pronto pra code-mod condicionado aos 🟡** (nenhum é falha de design).

## Fatos estabelecidos (evidência do revisor)

- **"delta<0 ⟺ dreno" VALE nos 16 cenários por construção** — `delta` é a variação literal aplicada em
  `hands.Current` com o vanilla neutralizado; `CachedAimDrainRate` positivo (backend 3f); tooltip do próprio
  stances confirma "<1 drena, 1 mantém, >1 recupera". Não existe dreno com delta>0.
- **Hold-breath sem mirar NÃO EXISTE no EFT** (`MovementContext.HoldBreath` exige `IsAiming`; desmirar força
  `HoldBreath(false)` sincronamente) → o fator do Hunter via `IsAiming` cobre os cenários `*HoldBreath` sempre.
- Fator 0 do Tank: delta 0 → early-return sem eventos = caminho JÁ exercitado pelos defaults neutros do stances
  (barra/tremor não prendem); `HandleExpiration` nunca dispara ✓.
- `Func<float>` cross-assembly ok (tipo mscorlib); namespace real confirmado `CameraRotationMod`.
- Colisão de sessão: HOJE zero (diff dos dois HEADs em `mods/stances*` vazio; único não-commitado lá é HANDOFF.md,
  que este item não toca).

## Pontos e resoluções

| ID | Cat·Imp | Título | Resolução (aplicada no code-mod) |
|---|---|---|---|
| PA-01-01 | A·🟡 | Retry do attach sem gatilho definido | `TryAttach()` no Awake **+ re-try no raid-start** (Postfix existente do `RaidPerksNotificationPatch` — nesse ponto todos os plugins já carregaram). Idempotente. |
| PA-01-02 | A·🟡 | `Factor()` sem fonte do Player/política de log | Provider resolve `Singleton<GameWorld>.MainPlayer` com null-guards → 1f; try/catch com **warn-once**. |
| PA-01-03 | C·🟡 | `Func<float>?` gera CS8632 no stances (csproj sem Nullable) | Campo **sem** anotação `?` no stances. |
| PA-01-04 | B·🟢 | TOCTOU cosmético (delegate lido 2×) | Cópia local `var hook = ...` no Tick. |
| PA-01-05 | B·🟢 | NaN de config manual propagaria | Guard `float.IsNaN → 1f` no provider (paridade: o stances tem o mesmo buraco pré-existente nos próprios Multipliers). |
| PA-01-06 | B·🟢 | Horizonte de merge (mesmo arquivo em 2 branches) | Commit cedo (nesta sessão) + registro na memória do stances. |
| PA-01-07 | C·🟢 | Namespace-fóssil como API externa | Comentário de CONTRATO EXTERNO no campo do stances (não renomear sem coordenar). |

## Histórico

| Data | Evento |
|---|---|
| 2026-07-04 | Review 01 — 7 pontos, 🔴 zero; resoluções incorporadas ao code-mod no mesmo dia |
