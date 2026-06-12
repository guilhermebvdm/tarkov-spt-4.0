# 011 — Infra de identidade visual da classe · Review Técnica 01

**Mod:** CustomClasses
**Spec técnica revisada:** [011-infra-identidade-visual-02-spec-tech.md](011-infra-identidade-visual-02-spec-tech.md)
**Data:** 2026-06-08

> Análise crítica. IDs `PA-01-MM`. Resolver bloqueadores antes de `/code-mod`.

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 4 · Total: 4 — todos tratados no `/code-mod` (ver as-built §"PA resolvidos").

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | C — Erro de Lógica | 🟡 | `visualRegistry.Set` deve vir só no registro efetivo (após validações) | ✅ Resolvido |
| PA-01-02 | C — Erro de Lógica | 🟢 | `ClassIdentityView`: `iconSize` não aplicado ao `LayoutElement` | ✅ Resolvido |
| PA-01-03 | B — Edge Case | 🟢 | `ClassIdentityView` assume filhos `Icon`/`Label` no container existente | ✅ Resolvido |
| PA-01-04 | A — Gap | 🟢 | Confirmar que `modded/Client/icons/*.png` é versionado (não cai no `.gitignore`) | ✅ Resolvido |

## Categorias

- **A — Gaps** · **B — Edge Cases** · **C — Erros de Lógica**

## Impacto

- 🔴 Bloqueador · 🟡 Importante · 🟢 Menor

---

## Pontos

### PA-01-01 · C — Erro de Lógica · 🟡 Importante

**`visualRegistry.Set` deve vir só no registro efetivo (após as validações)**

**Problema:** o stub diz "chamar `visualRegistry.Set(name, …)` após validar 'name', antes de `templates[name] = sides`". Mas entre o nome e o `templates[name] = sides` há validações que podem **retornar false** (colisão de edition, base não encontrada, clone nulo). Se o `Set` rodar antes delas, o registry guardaria uma edition que **não foi registrada** como template → o router devolveria identidade para uma classe inexistente.

**Por que importa:** dessincroniza o `ClassVisualRegistry` dos templates reais; corner case raro mas confuso.

**Sugestão:** chamar `visualRegistry.Set(name, def.IconFile, def.NameColor)` **imediatamente antes (ou junto) de `templates[name] = sides`** ([CustomClassesMod.cs:188](../../modded/Server/CustomClassesMod.cs#L188)), quando o registro é garantido. Espelha o que o `skillMultiplierRegistry.Set` já faz (só quando há dados válidos).

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

### PA-01-02 · C — Erro de Lógica · 🟢 Menor

**`ClassIdentityView`: `iconSize`/`fontSize` parametrizados mas não aplicados no container**

**Problema:** `BuildOrRefresh` recebe `iconSize` (default 48) mas `CreateContainer` **hardcoda** `preferredWidth/Height = 48f` no `LayoutElement`. O parâmetro não tem efeito na criação.

**Sugestão:** aplicar `iconSize` ao `LayoutElement` (passar para `CreateContainer` ou setar no `BuildOrRefresh` após obter o `LayoutElement`). `fontSize` já é aplicado no refresh — ok.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

### PA-01-03 · B — Edge Case · 🟢 Menor

**`ClassIdentityView` assume `Icon`/`Label` no container reaproveitado**

**Problema:** no refresh, `go.transform.Find("Icon")`/`Find("Label")` assume que o container existente tem esses filhos. Se um container com o mesmo nome existir sem eles (improvável — só nós criamos), daria NRE.

**Sugestão:** guard defensivo: se `Find("Icon")`/`Find("Label")` for null, recriar via `CreateContainer` (ou checar e logar). Baixo custo.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

### PA-01-04 · A — Gap · 🟢 Menor

**Confirmar que os PNGs do client são versionados**

**Problema:** o `.gitignore` ignora `mods/**/builds/*`, `mods/**/References/*.dll`, `mods/**/*.bundle`, `mods/**/*.webp`. PNGs em `modded/Client/icons/*.png` **não** casam com nenhum padrão → serão versionados (correto). Vale confirmar no `/code-mod` (e que o `compile-mod` os copia).

**Sugestão:** ao adicionar os PNGs, rodar `git status` e confirmar que aparecem como versionáveis; nenhum ajuste de `.gitignore` esperado.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

## Histórico

| Data | Evento |
|---|---|
| 2026-06-08 | Review 01 criada via `/review-technical-spec` — 0 🔴 · 1 🟡 · 3 🟢 |
| 2026-06-08 | PA-01-01..04 resolvidos no `/code-mod` (detalhes no as-built §"PA resolvidos") |
