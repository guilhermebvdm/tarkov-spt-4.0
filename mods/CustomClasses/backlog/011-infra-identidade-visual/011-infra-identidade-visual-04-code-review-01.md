# 011 — Infra de identidade visual da classe · Code Review 01

**Mod:** CustomClasses
**Spec funcional:** [011-infra-identidade-visual-01-spec.md](011-infra-identidade-visual-01-spec.md)
**Spec técnica:** [011-infra-identidade-visual-02-spec-tech.md](011-infra-identidade-visual-02-spec-tech.md)
**Asbuild:** [011-infra-identidade-visual-05-asbuild.md](011-infra-identidade-visual-05-asbuild.md)
**Data:** 2026-06-08

> Análise crítica do código do `/code-mod`. IDs `CR-01-MM`. 0 bloqueadores → item pode fechar.

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 0 · 🟢 Menores: 2 · ✅ Resolvidos: 0 · Total: 2

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-01-01 | E — Legibilidade | 🟢 | `exampleClass.jsonc` aponta para `exampleClass.png` inexistente | Pendente |
| CR-01-02 | B — Bug latente | 🟢 | `ClassIconCache` cacheia `null` → PNG adicionado sem restart não aparece | Pendente |

## Categorias

- **A — Crítico** · **B — Bug latente** · **C — Gap vs. spec** · **D — Arquitetura** · **E — Legibilidade** · **F — Melhoria**

## Impacto

- 🔴 Bloqueador · 🟠 Forte · 🟡 Médio · 🟢 Menor

---

## Pontos

### CR-01-01 · E — Legibilidade · 🟢 Menor

**`exampleClass.jsonc` aponta para `exampleClass.png` inexistente**

**Local:** [`modded/Server/config/classes/_docs/exampleClass.jsonc`](../../modded/Server/config/classes/_docs/exampleClass.jsonc)

**Problema:** o exemplo doc usa `"iconFile": "exampleClass.png"`, mas esse PNG não existe em `modded/Client/icons/`. Como `_docs/` não é carregado (é só referência), não causa erro — e mesmo que fosse, degradaria para só o nome (CR não-funcional). Mas quem copiar o exemplo verá um ícone faltando.

**Por que importa:** clareza para o autor de classes; nenhum impacto funcional.

**Sugestão:** ou (a) deixar claro no comentário que o nome é ilustrativo (o arquivo precisa existir em `icons/`), ou (b) gerar um `exampleClass.png` placeholder. Preferir (a) — menos um asset.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

### CR-01-02 · B — Bug latente · 🟢 Menor

**`ClassIconCache` cacheia `null` → PNG adicionado depois (sem restart) não aparece**

**Local:** [`modded/Client/UI/ClassIconCache.cs`](../../modded/Client/UI/ClassIconCache.cs)

**Problema:** quando um `iconFile` não existe no disco, o cache guarda `null` para não retentar. Se o usuário adicionar o PNG **com o jogo aberto**, o ícone só aparece após reiniciar (o `null` fica cacheado). O critério da spec ("trocar PNG sem recompilar") é satisfeito **reabrindo o jogo** — então é aceitável.

**Por que importa:** expectativa de "hot-swap" total; na prática exige restart do jogo (coerente com plugin BepInEx).

**Sugestão:** aceitar como está (cachear `null` evita I/O repetido por frame). Documentar que trocar/adicionar ícone exige reiniciar o jogo. Se um dia quiser hot-add, não cachear o `null` (só os sucessos) — fica como dívida opcional.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão (manter; documentar restart)
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

## Histórico

| Data | Evento |
| --- | --- |
| 2026-06-08 | Code review 01 criada via `/code-review` — 0 🔴 · 0 🟠 · 0 🟡 · 2 🟢. Item sólido (infra); achados opcionais. |
