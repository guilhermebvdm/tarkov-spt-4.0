# 008 — i18n (multilíngue pt-BR/en) · Review Técnica 01

**Mod:** CustomClasses
**Spec técnica revisada:** [008-i18n-02-spec-tech.md](008-i18n-02-spec-tech.md)
**Data:** 2026-06-07

> Análise crítica da spec técnica. IDs `PA-01-MM`. Resolver bloqueadores antes de `/code-mod`.

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 4 · Total: 4 — todos tratados no `/code-mod` (PA-01-02 a validar no load).

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | C — Erro de Lógica | ✅ | `LocaleService` injetável (Singleton) — confirmado | Resolvido |
| PA-01-02 | C — Erro de Lógica | ✅ | `JsonUtil`/STJ honra o `[JsonConverter]` (compila; validar no load) | Resolvido |
| PA-01-03 | B — Edge Case | ✅ | Enum `Language` simples em `Plugin` | Resolvido |
| PA-01-04 | A — Gap | ✅ | `PROPRIEDADES.md` criado com as 3 ConfigEntry | Resolvido |

## Categorias

- **A — Gaps** · **B — Edge Cases** · **C — Erros de Lógica**

## Impacto

- 🔴 Bloqueador · 🟡 Importante · 🟢 Menor

---

## Pontos

### PA-01-01 · C — Erro de Lógica · ✅ Resolvido em 2026-06-07

**`LocaleService` injetável**

**Verificação:** `[Injectable(InjectionType.Singleton)]` em `LocaleService.cs:9`; injetado por `ItemHelper`, `QuestHelper`, `DataCallbacks` etc. Pode ser adicionado ao ctor do `CustomClassesMod` sem problema.

**Decisão:** `[x]` Aceitar (injetar `LocaleService` no ctor).

### PA-01-02 · C — Erro de Lógica · 🟡 Importante · ✅ Resolvido em 2026-06-07

**O `JsonUtil` (STJ) precisa honrar o `[JsonConverter]` do `LocalizedText`**

**Resolução:** mantido o `[JsonConverter(typeof(LocalizedTextConverter))]` no tipo (idiomático STJ). Build compila 0 warn/err. **Validação final no boot do server** (carregar as classes com `description` objeto) fica no playtest — plano B só se falhar.


**Problema:** a desserialização das classes usa `jsonUtil.Deserialize<ClassDefinition>(...)` (System.Text.Json com options próprias do SPT). O plano depende de o STJ respeitar o `[JsonConverter(typeof(LocalizedTextConverter))]` no tipo `LocalizedText`. STJ honra atributo de conversor por padrão, **mas** o `JsonUtil` adiciona conversores próprios e usa `JsonSerializerOptions` customizadas — há risco (baixo) de conflito/ordem.

**Por que importa:** se o conversor não for aplicado, a desserialização de `description` (objeto) quebra a classe inteira (skip + log) — silenciosamente sem descrição bilíngue.

**Sugestão:** manter o `[JsonConverter]` no tipo (caminho idiomático). **Validar no build + load**: criar 1 classe com `description` objeto e 1 com string, conferir no log que ambas carregam e a descrição resolve. Se o atributo não pegar, plano B: registrar o conversor manualmente não é possível (não controlamos as options do `JsonUtil`) → cair para um DTO `{ en, pt }` simples (sem conversor) e **abandonar** a forma string-legada (migrar os JSONs para objeto). Decidir só se o teste falhar.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão (manter `[JsonConverter]`; validar no load; plano B só se falhar)
- `[ ]` Caminho alternativo: _________________

### PA-01-03 · B — Edge Case · 🟢 Menor · ✅ Resolvido em 2026-06-07

**Enum `Language` aninhado em `Plugin` + label do dropdown**

**Problema:** `Plugin.Language { English, Portugues }` — o identificador `Portugues` (sem acento) é o que aparece no dropdown do F12. Aceitável, mas "Portugues" sem acento é feio.

**Sugestão:** manter o enum simples (`English`, `Portugues`); o seletor é técnico e raramente visto. Se quiser "Português (BR)" no dropdown, exigiria atributo de descrição por valor (mais código) — deixar para depois. Colocar o enum em arquivo próprio (`Language.cs`) ou em `Plugin` — tanto faz; manter em `Plugin` pra simplicidade.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão (enum simples em `Plugin`)
- `[ ]` Caminho alternativo: _________________

### PA-01-04 · A — Gap · 🟢 Menor · ✅ Resolvido em 2026-06-07

**`PROPRIEDADES.md` ausente**

**Problema:** o mod tem agora 3 `ConfigEntry` (`EnableSkillMultipliers`, `ShowMultiplierOnSkills`, e a nova `Language`) e **não** tem `PROPRIEDADES.md` (convenção do repo).

**Por que importa:** rastreabilidade das props do F12 (skill `repo-workflow-best-practices` §7).

**Sugestão:** criar `mods/CustomClasses/PROPRIEDADES.md` com as 3 entradas (Nome EN, tradução pt-BR, tipo, padrão, faixa, tooltip pt-BR), como parte deste code-mod.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão (criar `PROPRIEDADES.md` com as 3 props)
- `[ ]` Caminho alternativo: _________________

## Histórico

| Data | Evento |
|---|---|
| 2026-06-07 | Review 01 criada via `/review-technical-spec` — 0 🔴 · 1 🟡 · 2 🟢 · 1 ✅. Sem bloqueadores → liberado p/ `/code-mod` (PA-01-02 validar no load). |
