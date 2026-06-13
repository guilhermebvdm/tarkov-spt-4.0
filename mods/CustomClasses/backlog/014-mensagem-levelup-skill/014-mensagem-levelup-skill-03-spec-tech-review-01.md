# 014 — Review da Spec Técnica · 01

**Mod:** CustomClasses · **Data:** 2026-06-09

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 1 · 🟢 Menores: 1

## PA-01-01 · B — Edge case · 🟡 Importante · ✅ Resolvido

**Incerteza: a notificação de skill passa por `DisplayMessageNotification(string)`?**

**Problema:** a string não está em nenhum DLL, então não dá pra confirmar estaticamente que a notificação de level-up usa `DisplayMessageNotification(string)` (vs `DisplayNotification(objeto)`). Se não passar, o prefix nunca a alcança.

**Por que importa:** sem confirmar, o build pode "não fazer nada" e parecer bug.

**Resolução (aceita):** (a) o patch **degrada gracioso** — se não casar, a notificação vanilla fica intacta (sem crash); (b) adicionar `LogDebug` do `message` quando contiver "leveled" mas não casar o padrão de skill, para **diagnóstico no playtest** (o usuário habilita Debug e vê o texto/caminho real). Risco aceitável para um build de teste; se não reescrever in-game, o log aponta o caminho correto.

## PA-01-02 · C — Erro de lógica · 🟢 Menor · ✅ Resolvido

**Nome localizado da skill vs nome do `ESkillId`**

O `skillName` extraído é o display localizado; mapeado ao enum via `Enum.TryParse(normalizado, ignoreCase)`. Cobre as skills comuns (Endurance, Strength, Search, RecoilControl→"Recoil control", …). Display muito divergente do enum não casa → não reescreve (aceitável; documentado).

## Decisão

Sem bloqueadores. Pode ir para `/code-mod` com o `LogDebug` de diagnóstico incluído (PA-01-01).

## Histórico

| Data | Evento |
|---|---|
| 2026-06-09 | Review 01 — 1 🟡 (caminho da notificação, mitigado com degradação + log) + 1 🟢. 0 🔴. |
